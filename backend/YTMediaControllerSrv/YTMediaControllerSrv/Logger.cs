using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using System.Web.UI.WebControls;

namespace YTMediaControllerSrv
{
    public class Logger
    {
        static string solutionName = Assembly.GetEntryAssembly().GetName().Name;
        static string eventSourceName = "YTMediaController";
        static void CreateLog(string logLevel, string message)
        {
            string entry = $"{DateTime.Now} [{logLevel}]: {message}";
#if DEBUG
            Console.WriteLine(entry);
#else
            try
            {
                WriteFileLog(entry);
            }
            catch (Exception ex)
            {
                if (!solutionName.Contains("Host"))
                {
                    WriteEventProfilerLog(logLevel, message, ex);
                }
            }
#endif
        }

        static void WriteFileLog(string entry)
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

        static string CreateLogFilePath()
        {
            string dateStamp = DateTime.Now.ToString("dd-MM-yyyy");
            string filename = $"{dateStamp}-{solutionName}-stdout.log";
            return Path.Combine(PathResolver.GetLogsDir(), filename);
        }
        public static void Info(string message)
        {
            CreateLog("INFO", message);
        }

        public static void Debug(string message)
        {
#if DEBUG
            Console.WriteLine(message);
#endif
        }

        public static void Warn(string message)
        {
            CreateLog("WARN", message);
        }

        public static void Error(string message, Exception err)
        {
            CreateLog("ERROR", $"{message}\n Error: {err.Message}, Stack: {err.StackTrace}");
        }

        static EventLogEntryType GetEventLogType(string logLevel)
        {
            switch (logLevel.ToLowerInvariant()) {
                case "info": return EventLogEntryType.Information;
                case "warn": return EventLogEntryType.Warning;
                case "error": return EventLogEntryType.Error;
                default: return EventLogEntryType.Information;
            }
        }

        private static void WriteEventProfilerLog(string logLevel, string logMessage, Exception err=null)
        {
            if (!EventLog.SourceExists(eventSourceName))
            {
                EventLog.CreateEventSource(eventSourceName, "Application");
            }
            
            var content = new StringBuilder();
            content.AppendLine(logMessage);

            if (err != null) { 
                content.AppendLine($"\n ERROR: {err.ToString()} \n {err.StackTrace}");
            }


            EventLog.WriteEntry(eventSourceName, content.ToString(), GetEventLogType(logLevel));
        }
    }
}
