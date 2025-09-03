using System;
using System.Diagnostics;
using System.ServiceProcess;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using YTMediaControllerSrv;

namespace YTmediaControllerUpdaterSrv
{
    public partial class Service1 : ServiceBase
    {
        private Updater updater;
        private readonly TimeSpan updateInterval = TimeSpan.FromHours(4);
        private TaskManager checkForUpdatePeriodicTask;
        private readonly Logger logger;
        public Service1()
        {
            logger = new Logger("AUTO_UPDATER");
            var ghRelease = new GHReleases();
            updater = new Updater(logger, ghRelease);

            InitializeComponent();
        }

        protected override void OnStart(string[] args)
        {
            checkForUpdatePeriodicTask = new TaskManager(
                CheckForUpdatePeriodicTask,
                updateInterval,
                runImmediately: true,
                fixedRate: true,
                HandleTaskError
                );

            checkForUpdatePeriodicTask.Start();
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
            Task.Run(updater.Cleanup);
            checkForUpdatePeriodicTask?.StopAsync().GetAwaiter().GetResult();
            checkForUpdatePeriodicTask?.Dispose();
            checkForUpdatePeriodicTask = null;
        }
    }
}
