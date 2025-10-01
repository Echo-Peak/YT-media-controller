using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using System.Web.UI.WebControls;

namespace YTMediaControllerSrv.Logging
{
    internal class FileLogger : ILogger
    {
        private readonly string solutionName = Assembly.GetEntryAssembly().GetName().Name;
        private readonly string eventSourceName = "YTMediaController";
        private readonly string LogName = "";

        public FileLogger(string logName = "") {
            LogName = logName;
        }
        private void WriteFileLog(string logLevel, string message)
        {
            string loggerName = String.IsNullOrEmpty(LogName) ? "" : $" [{LogName.ToUpperInvariant()}]";
            string entry = $"{DateTime.Now} [{logLevel}]{loggerName}: {message}";
            try
            {
                bool logDirExists = Directory.Exists(PathResolver.GetLogsDir());
                if (!logDirExists)
                {
                    try
                    {
                        Directory.CreateDirectory(PathResolver.GetLogsDir());
                    }
                    catch (Exception ex)
                    {
                        Console.WriteLine(ex.ToString());
                    }
                }

                string logFilePath = CreateLogFilePath();
                using (var writer = new StreamWriter(logFilePath, append: true))
                {
                    writer.WriteLine(entry);
                }
            }
            catch (Exception ex) {
                if (!solutionName.Contains("Host"))
                {
                    WriteEventProfilerLog(logLevel, entry, ex);
                }
            }
        }

        private EventLogEntryType GetEventLogType(string logLevel)
        {
            switch (logLevel.ToLowerInvariant())
            {
                case "info": return EventLogEntryType.Information;
                case "warn": return EventLogEntryType.Warning;
                case "error": return EventLogEntryType.Error;
                default: return EventLogEntryType.Information;
            }
        }

        private void WriteEventProfilerLog(string logLevel, string entry, Exception err)
        {
            if (!EventLog.SourceExists(eventSourceName))
            {
                EventLog.CreateEventSource(eventSourceName, "Application");
            }

            var content = new StringBuilder();
            content.AppendLine(entry);

            if (err != null)
            {
                content.AppendLine($"\n ERROR: {err.ToString()} \n {err.StackTrace}");
            }


            EventLog.WriteEntry(eventSourceName, content.ToString(), GetEventLogType(logLevel));
        }

        private string CreateLogFilePath()
        {
            string dateStamp = DateTime.Now.ToString("dd-MM-yyyy");
            string filename = $"{dateStamp}-{solutionName}-stdout.log";
            return Path.Combine(PathResolver.GetLogsDir(), filename);
        }
        public void Error(string message)
        {
            WriteFileLog("ERROR", message);
        }
        public void Error(string message, Exception err)
        {
            string newMessage = message + "\n" + err.Message + "\n" + err.StackTrace;
            WriteFileLog("ERROR", newMessage);
        }

        public void Info(string message)
        {
            WriteFileLog("INFO", message);
        }

        public void Warn(string message)
        {
            WriteFileLog("WARN", message);
        }

        public void Debug(string message)
        {
            WriteFileLog("DEBUG", message);
        }
    }
}
