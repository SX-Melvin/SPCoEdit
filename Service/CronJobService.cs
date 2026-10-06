using Microsoft.Extensions.Options;
using SPCoEdit.Configurations;
using SPCoEdit.Utils;

namespace SPCoEdit.Service
{
    public class CronJobService(DbUtils dbUtils, SharePointUtils sharePointUtils, OTCSUtils oTCSUtils, IOptions<CronJobConfiguration> options, IOptions<OTCSConfiguration> otcsOptions)
    {
        private readonly NLog.Logger _logger = NLog.LogManager.GetCurrentClassLogger();

        public async Task ExecuteAsync(CancellationToken cancellationToken)
        {
            cancellationToken.ThrowIfCancellationRequested();
            // Match the local timestamps written by DbUtils.InsertSession.
            var cutoff = DateTime.Now.AddMinutes(-options.Value.IdleMinutes);
            var groups = await dbUtils.GetIdleSessionGroupsAsync(cutoff, cancellationToken);

            _logger.Info($"Found {groups.Count} session groups idle for at least {options.Value.IdleMinutes} minutes.");
            foreach (var group in groups)
            {
                cancellationToken.ThrowIfCancellationRequested();
                _logger.Info($"Idle NodeID={group.NodeID}, Sessions={group.SessionCount}, LatestSessionCreatedAt={group.LatestSessionCreatedAt:O}");

                var ticket = oTCSUtils.GetTicket();
                var node = dbUtils.GetNode(group.NodeID);

                if(node == null)
                {
                    _logger.Warn($"NodeID={group.NodeID} not found in DTreeCore.");
                    continue;
                }

                if(ticket == null)
                {
                    _logger.Error("Failed to get ticket");
                    continue;
                }

                var fileName = Uri.UnescapeDataString(
                    new Uri(group.WebUrl).AbsolutePath.Split('/').Last()
                );
                var spFile = sharePointUtils.DownloadFile(fileName, Path.Combine(Path.GetTempPath(), node.Name));
                        
                _logger.Info($"Uploading the latest version of NodeID={group.NodeID} spFile={spFile} to OTCS.");

                oTCSUtils.ReserveNode(new()
                {
                    ReservedUserID = null
                }, group.NodeID, ticket);

                oTCSUtils.AddFileVersion(group.NodeID, spFile, ticket);

                oTCSUtils.ReserveNode(new()
                {
                    ReservedUserID = otcsOptions.Value.AdminUserID
                }, group.NodeID, ticket);
            }
        }
    }
}
