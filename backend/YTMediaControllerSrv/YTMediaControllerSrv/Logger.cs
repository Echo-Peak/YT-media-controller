using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace YTMediaControllerSrv
{
    public class Logger
    {
        static void WriteLog(string logLevel, string message)
        {
#if DEBUG
            Console.WriteLine($"{DateTime.Now} [{logLevel}]: {message}");
#else
            try
            {
                using (System.Diagnostics.EventLog eventLog = new System.Diagnostics.EventLog("Application"))
                {
                    eventLog.Source = "YTMediaControllerSrv";
                    eventLog.WriteEntry($"{logLevel}: {message}", System.Diagnostics.EventLogEntryType.Information);
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Failed to write to Event Viewer: {ex.Message}");
            }
#endif
        }
        public static void Log(string message)
        {
            WriteLog("INFO", message);
        }
    }
}
