using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.ServiceProcess;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using YTMediaControllerSrv.Logging;

namespace YTMediaControllerUpdaterSrv
{
    internal class UpdateOrchestrator
    {
        private ILogger logger;
        public UpdateOrchestrator(ILogger logger)
        {
            this.logger = logger;
        }

        public async Task Install(string installerPath, CancellationTokenSource updaterCts)
        {
            try
            {
                bool isStopped = StopService("YTMediaControllerService", 3000);
                if (!isStopped) {
                    throw new Exception("Unable to stop service");
                }
                await StopNativeHost();
                await ExecInstaller(installerPath);
                await PostCleanup(installerPath);
            }
            catch (Exception ex) {
                this.logger.Error("Unable to install update at this time", ex);
                updaterCts.Cancel();
            }
        }
        private bool StopService(string serviceName, int timeout)
        {
            logger.Info($"Stopping service: \"{serviceName}\"");
            try
            {
                using (ServiceController service = new ServiceController(serviceName))
                {
                    if (service.Status == ServiceControllerStatus.Running ||
                        service.Status == ServiceControllerStatus.StartPending)
                    {
                        service.Stop();
                        service.WaitForStatus(ServiceControllerStatus.Stopped, TimeSpan.FromMilliseconds(timeout));
                    }
                    bool isStopped = service.Status == ServiceControllerStatus.Stopped;
                    logger.Info($"{serviceName} Service is stopped");
                    return isStopped;
                }
            }
            catch (Exception ex)
            {
                logger.Error($"Unable to stop service \"{serviceName}\"", ex);
                return false;
            }
        }

        private async Task StopProcess(string processName, int intervalDelay, CancellationToken cancellationToken)
        {
            int attempt = 0;
            int maxTries = 6;

            while (attempt < maxTries && !cancellationToken.IsCancellationRequested)
            {
                try
                {
                    var processes = Process.GetProcessesByName(processName);
                    if (processes.Length == 0)
                    {
                        logger.Info($"The process \"{processName}\" is not running");
                        break;
                    }
                    foreach (var proc in processes)
                    {
                        logger.Info($"Terminating process {proc.ProcessName} (PID {proc.Id})");
                        proc.Kill();


                        await WaitForExitAsync(proc, cancellationToken);
                    }
                }
                catch (Exception ex)
                {
                    logger.Error($"Unable to stop process \"{processName}\"", ex);
                }
                attempt += 1;
                if (attempt >= maxTries)
                {
                    logger.Info($"StopProcess loop reached {attempt} iterations");
                    break;
                }
                await Task.Delay(1000, cancellationToken);
            }
        }
        private Task WaitForExitAsync(Process process, CancellationToken cancellationToken)
        {
            var tcs = new TaskCompletionSource<bool>();

            process.EnableRaisingEvents = true;
            process.Exited += (s, e) => tcs.TrySetResult(true);

            if (process.HasExited)
                tcs.TrySetResult(true);

            // Cancel if requested
            cancellationToken.Register(() => tcs.TrySetCanceled());

            return tcs.Task;
        }
        private async Task StopNativeHost()
        {
            var cts = new CancellationTokenSource();
            await StopProcess("YTMediaControllerHost.exe", 1000, cts.Token);
        }

        private Task ExecInstaller(string installerPath)
        {
            logger.Info($"Attempting to execute installer: {installerPath}");
            var tcs = new TaskCompletionSource<bool>();

            var proc = new Process
            {
                StartInfo = new ProcessStartInfo()
                {
                    FileName = installerPath,
                    Arguments = "/S",
                    UseShellExecute = true,
                    CreateNoWindow = true,
                },
                EnableRaisingEvents = true
            };

            proc.Exited += (sender, args) =>
            {
                logger.Info($"Inataller has terminated with exit code: {proc.ExitCode}");
                tcs.TrySetResult(true);
                proc.Dispose();
            };

            if (!proc.Start())
            {
                tcs.TrySetException(new InvalidOperationException("Failed to start process."));
            }

            return tcs.Task;
        }
        private Task PostCleanup(string installerPath)
        {
            return Task.Run(() =>
            {
                File.Delete(installerPath);

            });
        }
    }
}
