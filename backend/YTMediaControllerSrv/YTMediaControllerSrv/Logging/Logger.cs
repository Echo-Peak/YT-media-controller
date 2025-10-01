using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using System.Web.UI.WebControls;
using YTMediaControllerSrv.Logging;

namespace YTMediaControllerSrv
{
    public class Logger : ILogger
    {
        private List<ILogger> loggers = new List<ILogger>();

        public Logger(string loggerName = "")
        {
#if DEBUG 
            loggers.Add(new ConsoleLogger(loggerName));
#else
            loggers.Add(new FileLogger(loggerName));
#endif
        }
        public void Error(string message)
        {
            foreach (var logger in loggers)
            {
                logger.Error(message);
            }
        }

        public void Error(string message, Exception err)
        {
            foreach (var logger in loggers)
            {
                logger.Error(message, err);
            }
        }

        public void Info(string message)
        {
            foreach (var logger in loggers)
            {
                logger.Info(message);
            }
        }

        public void Warn(string message)
        {
            foreach (var logger in loggers)
            {
                logger.Warn(message);
            }
        }
        public void Debug(string message)
        {
            foreach (var logger in loggers)
            {
                logger.Debug(message);
            }
        }
    }
}
