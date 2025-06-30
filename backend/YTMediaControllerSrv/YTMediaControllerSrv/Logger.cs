using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace YTMediaControllerSrv
{
    public class Logger
    {
        static string solutionName = Assembly.GetExecutingAssembly().GetName().Name;
        static string logFile = Path.Combine(PathResolver.GetLogsDir(), solutionName + "-stdout.log");
        static void CreateLog(string logLevel, string message)
        {
            string entry = $"{DateTime.Now} [{logLevel}]: {message}";
#if DEBUG
            Console.WriteLine(entry);
#else
            WriteLog(entry);
#endif
        }

        static void WriteLog(string entry)
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

            File.AppendAllText(logFile, entry);

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
    }
}
