using System;
using System.Diagnostics;
using System.Text;
using YTMediaControllerSrv.Logging;

namespace YTMediaControllerSrv
{
    internal class FirewallManager
    {
        private readonly ILogger logger;
        
        public FirewallManager(ILogger Logger) {
            logger = Logger;
        }
        
        private bool RuleExists(string ruleName)
        {
            try
            {
                var processInfo = new ProcessStartInfo
                {
                    FileName = "cmd.exe",
                    Arguments = $"/c netsh advfirewall firewall show rule name=\"{ruleName}\" >nul 2>&1",
                    UseShellExecute = false,
                    RedirectStandardOutput = true,
                    RedirectStandardError = true,
                    CreateNoWindow = true
                };

                using (var process = Process.Start(processInfo))
                {
                    if (process != null)
                    {
                        process.WaitForExit();
                        // Exit code 0 means rule exists, non-zero means it doesn't
                        return process.ExitCode == 0;
                    }
                }
            }
            catch (Exception ex)
            {
                logger.Error($"Error checking if firewall rule exists: {ruleName}", ex);
            }
            return false;
        }
        
        private bool ExecuteNetshCommand(string arguments, out string output, out string error)
        {
            output = string.Empty;
            error = string.Empty;
            
            try
            {
                var processInfo = new ProcessStartInfo
                {
                    FileName = "cmd.exe",
                    Arguments = $"/c netsh {arguments}",
                    UseShellExecute = false,
                    RedirectStandardOutput = true,
                    RedirectStandardError = true,
                    CreateNoWindow = true
                };

                using (var process = Process.Start(processInfo))
                {
                    if (process != null)
                    {
                        output = process.StandardOutput.ReadToEnd();
                        error = process.StandardError.ReadToEnd();
                        process.WaitForExit();
                        return process.ExitCode == 0;
                    }
                }
            }
            catch (Exception ex)
            {
                error = ex.Message;
                logger.Error($"Error executing netsh command: {arguments}", ex);
            }
            return false;
        }
        
        public void Update(string ruleName, int port)
        {
            bool exists = RuleExists(ruleName);
            
            if (!exists)
            {
                logger.Info($"Creating inbound FW rule \"{ruleName}\" to use port {port}");
                
                // Delete existing rule with same name if it exists (in case of different protocol)
                ExecuteNetshCommand($"advfirewall firewall delete rule name=\"{ruleName}\"", out _, out _);
                
                // Add new rule: inbound, allow, TCP, specific port, all profiles
                string command = $"advfirewall firewall add rule name=\"{ruleName}\" dir=in action=allow protocol=TCP localport={port} profile=any";
                bool success = ExecuteNetshCommand(command, out string output, out string error);
                
                if (!success)
                {
                    logger.Error($"Failed to create firewall rule \"{ruleName}\". Output: {output}, Error: {error}");
                }
                else
                {
                    logger.Info($"Successfully created firewall rule \"{ruleName}\" for port {port}");
                }
            }
            else
            {
                logger.Info($"Updating FW rule \"{ruleName}\" to use port {port}");
                
                // Delete existing rule
                ExecuteNetshCommand($"advfirewall firewall delete rule name=\"{ruleName}\"", out _, out _);
                
                // Add updated rule with new port
                string command = $"advfirewall firewall add rule name=\"{ruleName}\" dir=in action=allow protocol=TCP localport={port} profile=any";
                bool success = ExecuteNetshCommand(command, out string output, out string error);
                
                if (!success)
                {
                    logger.Error($"Failed to update firewall rule \"{ruleName}\". Output: {output}, Error: {error}");
                }
                else
                {
                    logger.Info($"Successfully updated firewall rule \"{ruleName}\" to port {port}");
                }
            }
        }

        public void Remove(string ruleName)
        {
            if (RuleExists(ruleName))
            {
                logger.Info($"Removing FW rule \"{ruleName}\"");
                
                string command = $"advfirewall firewall delete rule name=\"{ruleName}\"";
                bool success = ExecuteNetshCommand(command, out string output, out string error);
                
                if (!success)
                {
                    logger.Error($"Unable to remove FW rule \"{ruleName}\". Output: {output}, Error: {error}");
                }
                else
                {
                    logger.Info($"Successfully removed firewall rule \"{ruleName}\"");
                }
            }
            else
            {
                logger.Debug($"Did not find FW rule \"{ruleName}\" to remove");
            }
        }
    }
}
