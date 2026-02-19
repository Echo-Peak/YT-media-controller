using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using YTMediaControllerSrv;
using YTMediaControllerSrv.Logging;

#if WINDOWS
using TaskScheduler = Microsoft.Win32.TaskScheduler;
using Microsoft.Win32.TaskScheduler;
#endif

namespace YTMediaControllerUpdaterSrv.Helpers
{
    internal class UpdateTaskHelper
    {
#if WINDOWS
        private static readonly string TaskName = @"YTMC_Update";

        public static Task CreateTask(string installerPath)
        {
            var uninstallerPath = PathResolver.GetUninstaller();
            var startAt = DateTime.Now.AddMinutes(1);
            startAt = new DateTime(startAt.Year, startAt.Month, startAt.Day, startAt.Hour, startAt.Minute, 0);
            var powershellBin = Environment.ExpandEnvironmentVariables(@"%SystemRoot%\System32\WindowsPowerShell\v1.0\powershell.exe");
            using (var ts = new TaskScheduler.TaskService())
            {
                var td = ts.NewTask();
                td.RegistrationInfo.Description = "Update helper task for YTMediaController";
                td.Principal.UserId = "SYSTEM";
                td.Principal.LogonType = TaskLogonType.ServiceAccount;
                td.Principal.RunLevel = TaskRunLevel.Highest;
                td.Settings.Hidden = true;
                td.Settings.StartWhenAvailable = false;
                td.Settings.DisallowStartIfOnBatteries = false;
                td.Settings.StopIfGoingOnBatteries = false;
                td.Settings.MultipleInstances = TaskInstancesPolicy.IgnoreNew;
                td.Triggers.Add(new TimeTrigger(startAt));

                td.Actions.Add(CreateInstallerAction(powershellBin, uninstallerPath));
                td.Actions.Add(CreateInstallerAction(powershellBin, installerPath));
                td.Actions.Add(new ExecAction(powershellBin, $"-NoProfile -ExecutionPolicy Bypass -Command \"Remove-Item -Path '{installerPath}'\"", null));


                ts.RootFolder.RegisterTaskDefinition(TaskName, td, TaskScheduler.TaskCreation.CreateOrUpdate, null, null, TaskScheduler.TaskLogonType.ServiceAccount, null);
                ts.GetTask(TaskName)?.Run();
            }

            return Task.CompletedTask;
        }

        private static ExecAction CreateInstallerAction(string powershellBin, string binPath)
        {
            
            string command = $"-NoProfile -ExecutionPolicy Bypass -Command \"Start-Process -FilePath '{binPath}' -ArgumentList '/S' -Wait\"";

            return new ExecAction(powershellBin, command, null);
            
        }

        public static async Task Cleanup()
        {
            await StopTask();
            await RemoveTask();
        }

        private static Task StopTask()
        {
            using (var ts = new TaskScheduler.TaskService())
            {
                var task = ts.GetTask(TaskName);
                if (task == null) return Task.CompletedTask;
                foreach (var inst in task.GetInstances())
                {
                    try { inst.Stop(); } catch { }
                }
                try { task.Stop(); } catch { }
            }
            return Task.CompletedTask;
        }

        private static Task RemoveTask()
        {
            using (var ts = new TaskScheduler.TaskService())
            {
                var task = ts.GetTask(TaskName);
                if (task == null) return Task.CompletedTask;
                ts.RootFolder.DeleteTask(TaskName, false);
            }
            return Task.CompletedTask;
        }
#else
        public static Task CreateTask(string installerPath)
        {
            return Task.CompletedTask;
        }

        public static Task Cleanup()
        {
            return Task.CompletedTask;
        }
#endif
    }
}
