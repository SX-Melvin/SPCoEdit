using Microsoft.Extensions.Options;
using RestSharp;
using RestSharp.Authenticators;
using Newtonsoft.Json;
using SPCoEdit.Configurations;

namespace SPCoEdit.Utils
{
    public class SharePointUtils(IOptions<SharePointConfiguration> config)
    {
        private readonly NLog.Logger _logger = NLog.LogManager.GetCurrentClassLogger();
        private readonly RestClient _client = new RestClient(new RestClientOptions(config.Value.SiteUrl + "/_api/web/")
        {
            Authenticator = new HttpBasicAuthenticator(config.Value.Username, config.Value.Password)
        });

        public string UploadOrGetUrl(string localFilePath, string fileName)
        {
            try
            {
                // Check if file exists
                var fileExists = false;
                var checkRequest = new RestRequest($"lists/getbytitle('Documents')/items?$filter=FileLeafRef eq '{fileName}'&$select=FileRef", Method.Get);
                var checkResponse = _client.Execute(checkRequest);
                if (checkResponse.IsSuccessful)
                {
                    var data = JsonConvert.DeserializeObject<SharePointResponse>(checkResponse.Content);
                    fileExists = data?.value?.Any() == true;
                }

                // If file does not exist, upload it first
                if (!fileExists)
                {
                    var uploadRequest = new RestRequest($"lists/getbytitle('Documents')/RootFolder/Files/add(url='{fileName}', overwrite=false)", Method.Post);
                    uploadRequest.AddFile("file", localFilePath);
                    var uploadResponse = _client.Execute(uploadRequest);
                    if (!uploadResponse.IsSuccessful)
                    {
                        _logger.Error($"Upload failed: {uploadResponse.Content}");
                        return null;
                    }
                }

                // Get the URL
                var getRequest = new RestRequest($"lists/getbytitle('Documents')/items?$filter=FileLeafRef eq '{fileName}'&$select=FileRef", Method.Get);
                var getResponse = _client.Execute(getRequest);
                if (getResponse.IsSuccessful)
                {
                    var getData = JsonConvert.DeserializeObject<SharePointResponse>(getResponse.Content);
                    if (getData?.value?.Any() == true)
                    {
                        var fileRef = getData.value[0].FileRef;
                        return config.Value.SiteUrl + fileRef;
                    }
                }
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
        public string FileRef { get; set; }
    }

    public class SharePointResponse
    {
        public List<SharePointItem> value { get; set; }
    }
}
