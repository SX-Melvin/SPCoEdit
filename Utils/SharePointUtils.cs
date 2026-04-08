using Microsoft.Extensions.Options;
using RestSharp;
using SPCoEdit.Configurations;

namespace SPCoEdit.Utils
{
    public class SharePointUtils(IOptions<SharePointConfiguration> config)
    {
        private readonly NLog.Logger _logger = NLog.LogManager.GetCurrentClassLogger();
        private readonly RestClient _client = new RestClient(config.Value.SiteUrl + "/_api/web/");

    }
}
