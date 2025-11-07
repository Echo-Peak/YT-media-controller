using System;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Hosting;
using YTMediaControllerSrv.Logging;

namespace YTMediaControllerSrv
{
    public partial class Service1 : BackgroundService
    {
        private AppContainer app;
        private ILogger Logger;
        
        public Service1()
        {
            app = new AppContainer();
            Logger = app.defaultLogger;
        }

        protected override Task ExecuteAsync(CancellationToken stoppingToken)
        {
            Logger.Info("Service starting");
            app.Start();
            return Task.CompletedTask;
        }

        public override Task StopAsync(CancellationToken cancellationToken)
        {
            Logger.Info("Service stopping");
            app.Stop();
            return base.StopAsync(cancellationToken);
        }
    }
}
