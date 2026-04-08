using SPCoEdit.Dto;
using SPCoEdit.Utils;

namespace SPCoEdit.Service
{
    public class CoEditService(OTCSUtils oTCSUtils)
    {
        private readonly NLog.Logger _logger = NLog.LogManager.GetCurrentClassLogger();
        public APIResponse<string> StartCoEdit(long ID, int version, string fileName)
        {
            var response = new APIResponse<string>();

            try
            {
                var ticket = oTCSUtils.GetTicket();
                var filePath = oTCSUtils.DownloadFile(ID, version, fileName, ticket);
                if (filePath != null)
                {
                    // TODO Upload to Sharepoint and get the URL
                }
            }
            catch (Exception ex)
            {
                _logger.Error(ex);
                response.Error = ex.Message;
            }

            return response;
        }
    }
}
