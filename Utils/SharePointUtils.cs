using Microsoft.Extensions.Options;
using RestSharp;
using Newtonsoft.Json;
using SPCoEdit.Configurations;

namespace SPCoEdit.Utils
{
    public class SharePointUtils : IDisposable
    {
        private readonly NLog.Logger _logger = NLog.LogManager.GetCurrentClassLogger();
        private readonly SharePointConfiguration _config;
        private readonly SharePointOnlineTokenProvider _tokens;
        private readonly RestClient _client;
        private readonly string _library;

        public SharePointUtils(IOptions<SharePointConfiguration> config, SharePointOnlineTokenProvider tokens)
        {
            _config = config.Value;
            _tokens = tokens;
            if (!_config.IsOnline && !_config.Mode.Equals("OnPrem", StringComparison.OrdinalIgnoreCase))
                throw new InvalidOperationException("SharePoint:Mode must be OnPrem or Online.");
            if (!Uri.TryCreate(_config.SiteUrl, UriKind.Absolute, out var site) ||
                (site.Scheme != Uri.UriSchemeHttp && site.Scheme != Uri.UriSchemeHttps) ||
                (_config.IsOnline && site.Scheme != Uri.UriSchemeHttps))
                throw new InvalidOperationException("SharePoint:SiteUrl must be a valid HTTP(S) URL; Online requires HTTPS.");
            if (string.IsNullOrWhiteSpace(_config.LibraryTitle))
                throw new InvalidOperationException("SharePoint:LibraryTitle is required.");

            var clientOptions = new RestClientOptions(_config.SiteUrl.TrimEnd('/') + "/_api/web/");
            if (!_config.IsOnline)
                clientOptions.Credentials = new System.Net.NetworkCredential(_config.Username, _config.Password);
            _client = new RestClient(clientOptions);
            _library = $"lists/getbytitle('{EscapeOData(_config.LibraryTitle)}')";
        }

        private static string EscapeOData(string value) => Uri.EscapeDataString(value.Replace("'", "''"));

        private RestResponse Execute(RestRequest request)
        {
            if (!request.Parameters.Any(p => p.Name?.Equals("Accept", StringComparison.OrdinalIgnoreCase) == true))
                request.AddHeader("Accept", "application/json;odata=nometadata");
            if (_config.IsOnline)
                request.AddHeader("Authorization", "Bearer " + _tokens.GetAccessToken());
            return _client.Execute(request);
        }

        public void Dispose() => _client.Dispose();

        private string? GetRequestDigest()
        {
            var request = new RestRequest("../contextinfo", Method.Post);
            request.AddHeader("Accept", "application/json;odata=verbose");
            var response = Execute(request);
            if (response.IsSuccessful)
            {
                var data = Newtonsoft.Json.Linq.JObject.Parse(response.Content ?? "{}");
                return (string?)data["d"]?["GetContextWebInformation"]?["FormDigestValue"];
            }
            _logger.Error($"Failed to get request digest: {response.Content}");
            return null;
        }

        public string? UploadOrGetUrl(string localFilePath, string fileName)
        {
            try
            {
                // Check if file exists
                var fileExists = false;
                var checkRequest = new RestRequest($"{_library}/items?$filter=FileLeafRef eq '{EscapeOData(fileName)}'&$select=FileRef", Method.Get);
                var checkResponse = Execute(checkRequest);
                _logger.Debug($"Check file response: {checkResponse.Content}");
                if (checkResponse.IsSuccessful)
                {
                    var data = JsonConvert.DeserializeObject<SharePointResponse>(checkResponse.Content);
                    fileExists = data?.value?.Any() == true;
                }
                else
                {
                    _logger.Error($"Failed to check file: HTTP {(int)checkResponse.StatusCode}, {checkResponse.Content}");
                    return null;
                }

                // If file does not exist, upload it first
                if (!fileExists)
                {
                    var uploadRequest = new RestRequest($"{_library}/RootFolder/Files/add(url='{EscapeOData(fileName)}', overwrite=true)", Method.Post);
                    // OAuth requests do not need a form digest; on-premises credentials do.
                    if (!_config.IsOnline)
                    {
                        var digest = GetRequestDigest();
                        if (digest == null) return null;
                        uploadRequest.AddHeader("X-RequestDigest", digest);
                    }
                    // SharePoint expects the file itself as the body, without multipart boundaries.
                    uploadRequest.AlwaysSingleFileAsContent = true;
                    uploadRequest.AddFile("file", localFilePath, "application/octet-stream");
                    var uploadResponse = Execute(uploadRequest);
                    _logger.Debug($"Upload response: {uploadResponse.Content}");
                    if (!uploadResponse.IsSuccessful)
                    {
                        _logger.Error($"Upload failed: {uploadResponse.Content}");
                        return null;
                    }
                }

                // Get the URL
                var getRequest = new RestRequest($"{_library}/items?$filter=FileLeafRef eq '{EscapeOData(fileName)}'&$select=FileRef", Method.Get);
                var getResponse = Execute(getRequest);
                if (getResponse.IsSuccessful)
                {
                    var getData = JsonConvert.DeserializeObject<SharePointResponse>(getResponse.Content);
                    if (getData?.value?.Any() == true)
                    {
                        var fileRef = getData.value[0].FileRef;
                        var webUrl = string.IsNullOrWhiteSpace(_config.WebUrl)
                            ? new Uri(_config.SiteUrl).GetLeftPart(UriPartial.Authority)
                            : _config.WebUrl.TrimEnd('/');
                        return webUrl + fileRef;
                    }
                }
            }
            catch (Exception ex)
            {
                _logger.Error(ex);
            }
            return null;
        }

        public string? DownloadFile(string fileName, string filePath)
        {
            try
            {
                var request = new RestRequest($"{_library}/items?$filter=FileLeafRef eq '{EscapeOData(fileName)}'&$select=FileRef", Method.Get);
                var response = Execute(request);
                if (!response.IsSuccessful)
                {
                    _logger.Error($"Failed to find file: {response.Content}");
                    return null;
                }

                var data = JsonConvert.DeserializeObject<SharePointResponse>(response.Content);
                if (data?.value?.Any() != true)
                {
                    _logger.Error($"File not found: {fileName}");
                    return null;
                }

                var fileRef = data.value[0].FileRef;
                var downloadRequest = new RestRequest($"GetFileByServerRelativeUrl('{EscapeOData(fileRef)}')/$value", Method.Get);
                var downloadResponse = Execute(downloadRequest);
                if (downloadResponse.IsSuccessful && downloadResponse.RawBytes is { Length: > 0 } bytes)
                {
                    File.WriteAllBytes(filePath, bytes);
                    return filePath;
                }

                _logger.Error($"Download failed: {downloadResponse.Content}");
            }
            catch (Exception ex)
            {
                _logger.Error(ex);
            }
            return null;
        }
    }

    public class SharePointItem
    {
        public string FileRef { get; set; } = "";
    }

    public class SharePointResponse
    {
        public List<SharePointItem> value { get; set; } = [];
    }
}
