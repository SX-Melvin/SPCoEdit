using Microsoft.Extensions.Options;
using SPCoEdit.Configurations;

namespace SPCoEdit.Service
{
    public class CronJobBackgroundService(
        IServiceScopeFactory scopeFactory,
        IOptions<CronJobConfiguration> options) : BackgroundService
    {
        private readonly NLog.Logger _logger = NLog.LogManager.GetCurrentClassLogger();

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            var interval = TimeSpan.FromMinutes(options.Value.IntervalMinutes);
            using var timer = new PeriodicTimer(interval);
            _logger.Info($"Scheduled job started with an interval of {options.Value.IntervalMinutes} minutes.");

            try
            {
                // Run once at startup, then on each timer tick. Await each run so
                // executions within this app instance never overlap.
                do
                {
                    try
                    {
                        await using var scope = scopeFactory.CreateAsyncScope();
                        var job = scope.ServiceProvider.GetRequiredService<CronJobService>();
                        await job.ExecuteAsync(stoppingToken);
                    }
                    catch (OperationCanceledException) when (stoppingToken.IsCancellationRequested)
                    {
                        break;
                    }
                    catch (Exception ex)
                    {
                        _logger.Error(ex, "Scheduled job failed. It will retry on the next interval.");
                    }
                }
                while (await timer.WaitForNextTickAsync(stoppingToken));
            }
            catch (OperationCanceledException) when (stoppingToken.IsCancellationRequested)
            {
                // Normal application shutdown.
            }
        }
    }
}
