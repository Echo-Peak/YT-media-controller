using System;
using System.Collections.Generic;
using System.Linq;
using System.ServiceProcess;
using System.Text;
using System.Threading.Tasks;
using YTMediaControllerSrv;

namespace YTMediaControllerUpdaterSrv
{
    internal static class Program
    {
        /// <summary>
        /// The main entry point for the application.
        /// </summary>
        static void Main()
        {
#if DEBUG
            StartCliApp();
#else
            StartServiceApp();
#endif
        }
        static void StartCliApp()
        {
            var logger = new Logger("AUTO_UPDATER");
            var ghRelease = new GHReleases();
            var updater = new Updater(logger, ghRelease);

            Task.Run(async () =>
            {
                await updater.CheckForUpdate();
            }).Wait();
        }

        static void StartServiceApp()
        {
            try
            {
                ServiceBase.Run(new ServiceBase[] { new Service1() });
            }
            catch (Exception ex)
            {
                System.Diagnostics.EventLog.WriteEntry(
                    "Application",
                    $"[AutoUpdaterSvc] Fatal error before connecting to SCM:\r\n{ex}",
                    System.Diagnostics.EventLogEntryType.Error);
                throw;
            }
        }
    }
}
