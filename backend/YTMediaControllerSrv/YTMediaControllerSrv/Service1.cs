using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Diagnostics;
using System.Linq;
using System.ServiceProcess;
using System.Text;
using System.Threading.Tasks;

namespace YTMediaControllerSrv
{
    public partial class Service1 : ServiceBase
    {
        private AppContainer app;
        public Service1()
        {
            app = new AppContainer();
            InitializeComponent();
        }

        protected override void OnStart(string[] args)
        {
            app.Start();
        }

        protected override void OnStop()
        {
            app.Stop();
        }
    }
}
