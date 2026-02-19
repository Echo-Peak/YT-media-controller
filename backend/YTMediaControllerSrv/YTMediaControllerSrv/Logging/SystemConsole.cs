using System;
using System.Diagnostics;
using System.Runtime.InteropServices;

#if WINDOWS
using System.Diagnostics;
#endif

namespace YTMediaControllerSrv.Logging
{
    public static class SystemConsole
    {
        private static bool IsWindows()
        {
            return RuntimeInformation.IsOSPlatform(OSPlatform.Windows);
        }

        public static void WriteEventLogEntry(string source, string message, EventLogEntryType entryType)
        {
#if WINDOWS
            if (!IsWindows())
            {
                Console.WriteLine($"[EventLog] [{entryType}] {source}: {message}");
                return;
            }

            try
            {
                EventLog.WriteEntry(source, message, entryType);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[EventLog Error] Failed to write to EventLog: {ex.Message}");
                Console.WriteLine($"[EventLog] [{entryType}] {source}: {message}");
            }
#else
            Console.WriteLine($"[EventLog] [{entryType}] {source}: {message}");
#endif
        }

        public static void WriteEventLogInfo(string source, string message)
        {
            WriteEventLogEntry(source, message, EventLogEntryType.Information);
        }

        public static void WriteEventLogWarning(string source, string message)
        {
            WriteEventLogEntry(source, message, EventLogEntryType.Warning);
        }

        public static void WriteEventLogError(string source, string message)
        {
            WriteEventLogEntry(source, message, EventLogEntryType.Error);
        }

#if WINDOWS
        public static bool EventLogSourceExists(string sourceName)
        {
            if (!IsWindows())
            {
                return false;
            }

            try
            {
                return EventLog.SourceExists(sourceName);
            }
            catch (Exception)
            {
                return false;
            }
        }

        public static void CreateEventLogSource(string source, string logName)
        {
            if (!IsWindows())
            {
                Console.WriteLine($"[EventLog] Would create event source '{source}' in log '{logName}' (not supported on this platform)");
                return;
            }

            try
            {
                if (!EventLog.SourceExists(source))
                {
                    EventLog.CreateEventSource(source, logName);
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[EventLog Error] Failed to create event source '{source}': {ex.Message}");
            }
        }
#else
        public static bool EventLogSourceExists(string sourceName)
        {
            return false;
        }

        public static void CreateEventLogSource(string source, string logName)
        {
            Console.WriteLine($"[EventLog] Would create event source '{source}' in log '{logName}' (not supported on this platform)");
        }
#endif

        public static EventLogEntryType GetEventLogType(string logLevel)
        {
            switch (logLevel.ToLowerInvariant())
            {
                case "info": return EventLogEntryType.Information;
                case "warn": return EventLogEntryType.Warning;
                case "error": return EventLogEntryType.Error;
                case "debug": return EventLogEntryType.Information;
                default: return EventLogEntryType.Information;
            }
        }
    }
}
