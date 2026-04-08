using SPCoEdit.Dto.OTCS;
using System.Text.RegularExpressions;
using RestSharp;
using Newtonsoft.Json;
using SPCoEdit.Configurations;
using Microsoft.Extensions.Options;

namespace SPCoEdit.Utils
{
    public class OTCSUtils(IOptions<OTCSConfiguration> config)
    {
        private readonly NLog.Logger _logger = NLog.LogManager.GetCurrentClassLogger();

        private readonly RestClient _client = new RestClient(config.Value.ApiUrl);
        public string? DownloadFile(long ID, int version, string fileName, string ticket, string? path = null)
        {
            string? result = null;

            try
            {
                fileName = Regex.Replace(fileName, @"[^\u0000-\u007F]", " ");
                var request = new RestRequest($"v2/nodes/{ID}/versions/{version}/content", Method.Get);
                request.AddHeader("OTCSTicket", ticket);

                var savePath = Path.GetTempPath();
                if (path != null)
                {
                    savePath = path;
                }

                var response = _client.Execute(request);

                if (response.IsSuccessful)
                {
                    File.WriteAllBytes(Path.Combine(savePath, fileName), response.RawBytes);
                    result = Path.Combine(savePath, fileName);
                }
            }
            catch (Exception ex)
            {
                _logger.Error(ex);
            }

            return result;
        }
        public string? GetTicket()
        {
            string? result = null;

            try
            {
                var request = new RestRequest($"v1/auth", Method.Post);
                request.AddParameter("username", config.Value.Username);
                request.AddParameter("password", config.Value.Password);
                var response = _client.Execute(request);
                _logger.Info($"GetTicket Response: {response.Content}");
                var responseData = JsonConvert.DeserializeObject<GetTicketResponse>(response.Content);

                if (responseData.Error != null)
                {
                    _logger.Error(responseData.Error);
                }
                else
                {
                    result = responseData.Ticket;
                }
            }
            catch (Exception ex)
            {
                _logger.Error(ex);
            }

            return result;
        }
        public GetAuthInfoResponse? GetAuthInfo(string ticket)
        {
            GetAuthInfoResponse? result = null;

            try
            {
                var request = new RestRequest($"v1/auth", Method.Get);
                request.AddHeader("OTCSTicket", ticket);
                var response = _client.Execute(request);

                _logger.Info($"GetAuthInfo Response: {response.Content}");
                var responseData = JsonConvert.DeserializeObject<GetAuthInfoResponse>(response.Content);

                if (responseData.Error != null)
                {
                    _logger.Error(responseData.Error);
                }
                else
                {
                    result = responseData;
                }
            }
            catch (Exception ex)
            {
                _logger.Error(ex);
            }

            return result;
        }
        public SearchResponse? SearchFile(SearchRequest body, string ticket, string? token = null)
        {
            SearchResponse? result = null;

            try
            {
                var request = new RestRequest($"v2/search", Method.Post);
                if (token != null)
                {
                    request.AddHeader("Authorization", $"Bearer {token}");
                }
                else
                {
                    request.AddHeader("OTCSTicket", ticket);
                }
                request.AddJsonBody(body);
                var response = _client.Execute(request);

                _logger.Info($"SearchFile Response: {response.Content?[..Math.Min(response.Content?.Length ?? 0, 300)]}");
                var responseData = JsonConvert.DeserializeObject<SearchResponse>(response.Content);
                result = responseData;
            }
            catch (Exception ex)
            {
                _logger.Error(ex);
            }

            return result;
        }
        public GetAuthInfoResponse? GetAuthInfoByToken(string token)
        {
            GetAuthInfoResponse? result = null;

            try
            {
                var request = new RestRequest($"v1/auth", Method.Get);
                request.AddHeader("Authorization", $"Bearer {token}");
                var response = _client.Execute(request);

                _logger.Info($"GetAuthInfoByToken Response: {response.Content}");
                var responseData = JsonConvert.DeserializeObject<GetAuthInfoResponse>(response.Content);

                if (responseData.Error != null)
                {
                    _logger.Error(responseData.Error);
                }
                else
                {
                    result = responseData;
                }
            }
            catch (Exception ex)
            {
                _logger.Error(ex);
            }

            return result;
        }
        public CreateNodeResponse? CreateNode(CreateNodeRequest body)
        {
            CreateNodeResponse? result = null;

            try
            {
                var request = new RestRequest($"v1/nodes", Method.Post);

                if (body.OTCSTicket != null)
                {
                    request.AddHeader("OTCSTicket", body.OTCSTicket);
                }
                else
                {
                    request.AddHeader("Authorization", $"Bearer {body.Token}");
                }

                request.AddJsonBody(new
                {
                    type = body.Type,
                    parent_id = body.ParentID,
                    name = body.Name
                });
                var response = _client.Execute(request);

                _logger.Info($"CreateNode Response: {response.Content}");
                var responseData = JsonConvert.DeserializeObject<CreateNodeResponse>(response.Content);

                if (responseData.Error != null)
                {
                    _logger.Error(responseData.Error);
                }
                else
                {
                    result = responseData;
                }
            }
            catch (Exception ex)
            {
                _logger.Error(ex);
            }

            return result;
        }
    }
}
