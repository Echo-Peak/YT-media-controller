using Octokit;
using System;
using System.Diagnostics;
using System.ServiceProcess;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using YTMediaControllerSrv;

namespace YTMediaControllerUpdaterSrv
{
    public partial class Service1 : ServiceBase
    {
        private Updater updater;
        private  TimeSpan updateInterval = TimeSpan.FromHours(4);
        private TaskManager checkForUpdatePeriodicTask;
        private  Logger logger;
        private GHReleases ghRelease;
        public Service1()
        {
            InitializeComponent();
            this.ServiceName = "YTMediaControllerUpdaterService";
        }

        protected override void OnStart(string[] args)
        {
            try
            {
                var baseDir = AppDomain.CurrentDomain.BaseDirectory;

                logger = new Logger();
                ghRelease = new GHReleases();
                updater = new Updater(logger, ghRelease);

               logger.Info("Starting service");

                checkForUpdatePeriodicTask = new TaskManager(
                    CheckForUpdatePeriodicTask,
                    updateInterval,
                    runImmediately: true,
                    fixedRate: true,
                    onError: HandleTaskError
                );

                checkForUpdatePeriodicTask.Start();
            }
            catch (Exception ex)
            {
                EventLog.WriteEntry(
                    "Application",
                    $"[{ServiceName}] Fatal error in OnStart:\r\n{ex}",
                    EventLogEntryType.Error);

                throw;
            }
        }

        private void HandleTaskError(Exception err)
        {
            logger.Error("Unable to execute checkForUpdate task", err);
        }

        private async Task CheckForUpdatePeriodicTask(CancellationToken token)
        {
            await updater.CheckForUpdate();
        }


        protected override void OnStop()
        {
            logger.Info("Stopping service");
            Task.Run(updater.Cleanup);
            checkForUpdatePeriodicTask?.StopAsync().GetAwaiter().GetResult();
            checkForUpdatePeriodicTask?.Dispose();
            checkForUpdatePeriodicTask = null;
        }
    }
}
