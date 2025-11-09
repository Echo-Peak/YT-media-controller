using System;
using System.Diagnostics;
using System.Runtime.InteropServices;

namespace YTMediaControllerSrv.Logging
{
    /// <summary>
    /// Provides an abstraction for system-level console operations, including Windows Event Log.
    /// Performs OS checks before using platform-specific APIs.
    /// </summary>
    public static class SystemConsole
    {
        private static bool IsWindows()
        {
            return RuntimeInformation.IsOSPlatform(OSPlatform.Windows);
        }

        /// <summary>
        /// Writes an entry to the Windows Event Log. Only works on Windows.
        /// </summary>
        /// <param name="source">The source name by which the application is registered on the local computer.</param>
        /// <param name="message">The string to write to the event log.</param>
        /// <param name="entryType">One of the EventLogEntryType values.</param>
        public static void WriteEventLogEntry(string source, string message, EventLogEntryType entryType)
        {
            if (!IsWindows())
            {
                // On non-Windows platforms, fall back to console output
                Console.WriteLine($"[EventLog] [{entryType}] {source}: {message}");
                return;
            }

            try
            {
                EventLog.WriteEntry(source, message, entryType);
            }
            catch (Exception ex)
            {
                // If EventLog fails, fall back to console
                Console.WriteLine($"[EventLog Error] Failed to write to EventLog: {ex.Message}");
                Console.WriteLine($"[EventLog] [{entryType}] {source}: {message}");
            }
        }

        /// <summary>
        /// Writes an entry to the Windows Event Log with Information level. Only works on Windows.
        /// </summary>
        /// <param name="source">The source name by which the application is registered on the local computer.</param>
        /// <param name="message">The string to write to the event log.</param>
        public static void WriteEventLogInfo(string source, string message)
        {
            WriteEventLogEntry(source, message, EventLogEntryType.Information);
        }

        /// <summary>
        /// Writes an entry to the Windows Event Log with Warning level. Only works on Windows.
        /// </summary>
        /// <param name="source">The source name by which the application is registered on the local computer.</param>
        /// <param name="message">The string to write to the event log.</param>
        public static void WriteEventLogWarning(string source, string message)
        {
            WriteEventLogEntry(source, message, EventLogEntryType.Warning);
        }

        /// <summary>
        /// Writes an entry to the Windows Event Log with Error level. Only works on Windows.
        /// </summary>
        /// <param name="source">The source name by which the application is registered on the local computer.</param>
        /// <param name="message">The string to write to the event log.</param>
        public static void WriteEventLogError(string source, string message)
        {
            WriteEventLogEntry(source, message, EventLogEntryType.Error);
        }

        /// <summary>
        /// Determines whether an event source is registered on the local computer. Only works on Windows.
        /// </summary>
        /// <param name="sourceName">The name of the event source.</param>
        /// <returns>true if the event source is registered; otherwise, false.</returns>
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

        /// <summary>
        /// Establishes an application as able to write event information to a particular log on the system. Only works on Windows.
        /// </summary>
        /// <param name="source">The source name by which the application is registered on the local computer.</param>
        /// <param name="logName">The name of the log the source's entries are written to. Possible values include Application, System, or a custom log.</param>
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

        /// <summary>
        /// Converts a log level string to the corresponding EventLogEntryType.
        /// </summary>
        /// <param name="logLevel">The log level string (case-insensitive). Valid values: "info", "warn", "error", "debug".</param>
        /// <returns>The corresponding EventLogEntryType. Returns Information for unknown log levels.</returns>
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

