using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Globalization;
using System.IO;
using System.Linq;
using System.ServiceProcess;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using YTMediaControllerSrv;
using YTMediaControllerSrv.Logging;
using YTMediaControllerUpdaterSrv.Helpers;

namespace YTMediaControllerUpdaterSrv
{
    internal class UpdateOrchestrator
    {
        private ILogger logger;
        public UpdateOrchestrator(ILogger logger)
        {
            this.logger = logger;
        }

        public async Task Install(string installerPath, CancellationTokenSource updaterCts)
        {
            try
            {
                logger.Info("Creating auto update task");
                await UpdateTaskHelper.CreateTask(installerPath);
            }
            catch (Exception ex) {
                this.logger.Error("Unable to install update at this time", ex);
                updaterCts.Cancel();
            }
        }

        public async Task Cleanup()
        {
            try
            {
                logger.Info("Cleaning auto update task");
                await UpdateTaskHelper.Cleanup();
            }
            catch (Exception ex)
            {
                logger.Error("Unable to perform cleanup", ex);
            }
        }
    }
}
