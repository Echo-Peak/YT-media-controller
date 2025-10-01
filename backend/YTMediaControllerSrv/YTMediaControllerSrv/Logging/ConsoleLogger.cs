using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace YTMediaControllerSrv.Logging
{
    internal class ConsoleLogger : ILogger
    {
        private readonly string LogName = "";
        public ConsoleLogger(string logName="") { 
            LogName = logName;
        }
        private string CreateLog(string logLevel, string message)
        {
            string loggerName = String.IsNullOrEmpty(LogName) ? "" : $" [{LogName.ToUpperInvariant()}]";
            return $"{DateTime.Now} [{logLevel}]{loggerName}: {message}";
        }
        public void Error(string message)
        {
            Console.WriteLine(CreateLog("ERROR", message));
        }
        public void Error(string message, Exception err)
        {
            string newMessage = message + "\n" + err.Message + "\n" + err.StackTrace;
            Console.WriteLine(CreateLog("ERROR", newMessage));
        }

        public void Info(string message)
        {
            Console.WriteLine(CreateLog("INFO", message));
        }

        public void Warn(string message)
        {
            Console.WriteLine(CreateLog("WARN", message));
        }

        public void Debug(string message)
        {
            Console.WriteLine(CreateLog("DEBUG", message));
        }
    }
}
