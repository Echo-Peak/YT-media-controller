using System;
using TaskScheduler = Microsoft.Win32.TaskScheduler;
using YTMediaControllerSrv;
using Microsoft.Win32.TaskScheduler;
using Task = System.Threading.Tasks.Task;

namespace YTMediaControllerUpdaterSrv.Helpers
{
    internal class UpdateTaskHelper
    {
        private static readonly string TaskName = @"\YTMC_Update";

        public static Task CreateTask(string installerPath)
        {
            var uninstallerPath = PathResolver.GetUninstaller();
            var commandLine = $"\"{uninstallerPath}\" /S && \"{installerPath}\" /qn /norestart";
            var startAt = DateTime.Now.AddMinutes(1);
            startAt = new DateTime(startAt.Year, startAt.Month, startAt.Day, startAt.Hour, startAt.Minute, 0);
            var cmdBin = Environment.ExpandEnvironmentVariables(@"%SystemRoot%\System32\cmd.exe");
            using (var ts = new TaskScheduler.TaskService())
            {
                var td = ts.NewTask();
                td.RegistrationInfo.Description = "Update helper task for YTMediaController";
                td.Principal.UserId = "SYSTEM";
                td.Principal.LogonType = TaskLogonType.ServiceAccount;
                td.Principal.RunLevel = TaskRunLevel.Highest;
                td.Settings.Hidden = true;
                td.Settings.StartWhenAvailable = true;
                td.Settings.DisallowStartIfOnBatteries = false;
                td.Settings.StopIfGoingOnBatteries = false;
                td.Settings.MultipleInstances = TaskInstancesPolicy.IgnoreNew;
                td.Triggers.Add(new TimeTrigger(startAt));
                td.Actions.Add(new ExecAction(uninstallerPath, "/S", null));
                td.Actions.Add(new ExecAction(installerPath, "/S", null));
                td.Actions.Add(new ExecAction(cmdBin, $"/c del \"{installerPath}\"", null));

                ts.RootFolder.RegisterTaskDefinition(TaskName, td, TaskScheduler.TaskCreation.CreateOrUpdate, null, null, TaskScheduler.TaskLogonType.ServiceAccount, null);
                ts.GetTask(TaskName)?.Run();
            }

            return Task.CompletedTask;
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
    }
}
