using SPCoEdit.Dto;
using SPCoEdit.Dto.CoEdit;
using SPCoEdit.Utils;
using System.Text.RegularExpressions;

namespace SPCoEdit.Service
{
    public class CoEditService(OTCSUtils oTCSUtils, SharePointUtils sharePointUtils, DbUtils dbUtils)
    {
        private readonly NLog.Logger _logger = NLog.LogManager.GetCurrentClassLogger();
        public APIResponse<string> StartCoEdit(CoEditRequest body)
        {
            var response = new APIResponse<string>();

            try
            {
                var ticket = oTCSUtils.GetTicket();
                var session = dbUtils.InsertSession(body.NodeID);

                if(ticket == null)
                {
                    response.Error = "Failed to get ticket";
                    return response;
                }

                var fileName = $"{Path.GetFileNameWithoutExtension(body.FileName)} [NodeID={session.NodeID}]{Path.GetExtension(body.FileName)}";
                var filePath = oTCSUtils.DownloadFile(session.NodeID, body.Version, fileName, ticket);
                if (filePath != null)
                {
                    var sharePointUrl = sharePointUtils.UploadOrGetUrl(filePath, fileName);
                    if (sharePointUrl != null)
                    {
                        response.Data = sharePointUrl;
                        oTCSUtils.ReserveNode(new()
                        {
                            ReservedUserID = body.UserID
                        }, session.NodeID, ticket);
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
                var nodeId = Regex.Match(fileName, @"\[NodeID=(\d+)\]");
                var actualFileName = Regex.Replace(fileName, @"\s*\[NodeID=\d+\]", "");
                var ticket = oTCSUtils.GetTicket();

                if (nodeId.Success && ticket != null)
                {
                    long nodeID = long.Parse(nodeId.Groups[1].Value);
                    dbUtils.DeleteSession(nodeID);

                    if (dbUtils.IsSessionEmpty(nodeID))
                    {
                        _logger.Info($"No more active sessions for NodeID={nodeID}. Uploading the latest version to SharePoint.");
                        oTCSUtils.ReserveNode(new()
                        {
                            ReservedUserID = null
                        }, nodeID, ticket);

                        var spFile = sharePointUtils.DownloadFile(fileName, Path.Combine(Path.GetTempPath(), actualFileName));
                        
                        _logger.Info($"Uploading the latest version of NodeID={nodeID} spFile={spFile} to OTCS.");
                        oTCSUtils.AddFileVersion(nodeID, spFile, ticket);
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
    }
}
