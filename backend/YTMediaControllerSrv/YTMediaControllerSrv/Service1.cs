using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Diagnostics;
using System.Linq;
using System.ServiceProcess;
using System.Text;
using System.Threading.Tasks;
using YTMediaControllerSrv.Logging;

namespace YTMediaControllerSrv
{
    public partial class Service1 : ServiceBase
    {
        private AppContainer app;
        private ILogger Logger;
        public Service1()
        {
            app = new AppContainer();
            Logger = app.defaultLogger;
            InitializeComponent();
        }

        protected override void OnStart(string[] args)
        {
            Logger.Info("Service starting");
            app.Start();
        }

        protected override void OnStop()
        {
            Logger.Info("Service stopping");
            app.Stop();
        }
    }
}
