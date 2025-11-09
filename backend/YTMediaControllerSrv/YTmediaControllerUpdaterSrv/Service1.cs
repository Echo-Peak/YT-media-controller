using System;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Hosting;
using YTMediaControllerSrv;
using YTMediaControllerSrv.Logging;

namespace YTMediaControllerUpdaterSrv
{
    public partial class Service1 : BackgroundService
    {
        private Updater updater;
        private  TimeSpan defaultUpdateInterval = TimeSpan.FromHours(4);
        private TaskManager checkForUpdatePeriodicTask;
        private  Logger logger;
        private UpdaterApi updaterApi;
        
        public Service1()
        {
        }

        private TimeSpan GetUpdateInterval()
        {
            try
            {
                var result = AppRegistry.Get(AppRegistryKeys.AutoUpdateIntervalMins);
                if (result != null) {

                    return TimeSpan.FromMinutes(Convert.ToInt32(result) * 60);
                }
            }
            catch (Exception err) {
                logger?.Warn("Unable to get update interval from registry. Using default update interval");
            }
            return defaultUpdateInterval;
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            try
            {
                var baseDir = AppDomain.CurrentDomain.BaseDirectory;

                logger = new Logger();
                updaterApi = new UpdaterApi();
                updater = new Updater(logger, updaterApi);

               logger.Info("Starting service");

                checkForUpdatePeriodicTask = new TaskManager(
                    CheckForUpdatePeriodicTask,
                    GetUpdateInterval(),
                    runImmediately: true,
                    fixedRate: true,
                    onError: HandleTaskError
                );

                checkForUpdatePeriodicTask.Start();

                // Keep the service running until cancellation is requested
                while (!stoppingToken.IsCancellationRequested)
                {
                    await Task.Delay(1000, stoppingToken);
                }
            }
            catch (Exception ex)
            {
                SystemConsole.WriteEventLogError(
                    "Application",
                    $"[YTMediaControllerUpdaterService] Fatal error in ExecuteAsync:\r\n{ex}");

                throw;
            }
        }

        private void HandleTaskError(Exception err)
        {
            logger?.Error("Unable to execute checkForUpdate task", err);
        }

        private async Task CheckForUpdatePeriodicTask(CancellationToken token)
        {
            await updater.CheckForUpdate();
        }

        public override async Task StopAsync(CancellationToken cancellationToken)
        {
            logger?.Info("Stopping service");
            if (updater != null)
            {
                Task.Run(updater.Cleanup);
            }
            if (checkForUpdatePeriodicTask != null)
            {
                await checkForUpdatePeriodicTask.StopAsync();
                checkForUpdatePeriodicTask?.Dispose();
                checkForUpdatePeriodicTask = null;
            }
            await base.StopAsync(cancellationToken);
        }
    }
}
