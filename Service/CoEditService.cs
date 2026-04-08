using SPCoEdit.Dto;
using SPCoEdit.Utils;
using System.Text.RegularExpressions;

namespace SPCoEdit.Service
{
    public class CoEditService(OTCSUtils oTCSUtils, SharePointUtils sharePointUtils)
    {
        private readonly NLog.Logger _logger = NLog.LogManager.GetCurrentClassLogger();
        public APIResponse<string> StartCoEdit(long ID, int version, string fileName)
        {
            var response = new APIResponse<string>();

            try
            {
                var ticket = oTCSUtils.GetTicket();
                fileName = $"{Path.GetFileNameWithoutExtension(fileName)} [NodeID={ID}]{Path.GetExtension(fileName)}";
                var filePath = oTCSUtils.DownloadFile(ID, version, fileName, ticket);
                if (filePath != null)
                {
                    var sharePointUrl = sharePointUtils.UploadOrGetUrl(filePath, fileName);
                    if (sharePointUrl != null)
                    {
                        response.Data = sharePointUrl;
                    }
                    else
                    {
                        response.Error = "Failed to upload to SharePoint";
                    }
                }
            }
            catch (Exception ex)
            {
                _logger.Error(ex);
                response.Error = ex.Message;
            }

            return response;
        }
        public APIResponse<string> StopCoEdit(string fileName)
        {
            var response = new APIResponse<string>();

            try
            {
                var nodeId = Regex.Match(fileName, @"NodeID=\{([^}]+)\}");

                if (nodeId.Success)
                {
                    string id = nodeId.Groups[1].Value;
                    var ticket = oTCSUtils.GetTicket();
                    var actualFileName = Regex.Replace(fileName, @"\s*\[.*?\]", "");
                    actualFileName = actualFileName.TrimEnd();
                    //oTCSUtils.AddFileVersion(Int64.Parse(id), actualFileName, ticket);
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
