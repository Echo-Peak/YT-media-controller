using Semver;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using YTMediaControllerSrv;
using YTMediaControllerSrv.Logging;
using YTMediaControllerUpdaterSrv.Types;

namespace YTMediaControllerUpdaterSrv
{
    internal class Updater
    {
        private readonly ILogger Logger;
        private readonly UpdaterApi updaterApi;
        private readonly UpdateOrchestrator updateOrchestrator;
        private CancellationTokenSource currentUpdaterCts;
        private readonly List<string> updateChannels = new List<string>()
        {
            "stable",
            "beta",
            "alpha"
        };
        public Updater(ILogger logger, UpdaterApi updaterApi)
        {
            this.Logger = logger;
            this.updaterApi = updaterApi;
            this.updateOrchestrator = new UpdateOrchestrator(logger);
            PerformPreStartCleanup();
        }
        private void PerformPreStartCleanup()
        {
            try
            {
                updateOrchestrator.Cleanup().ConfigureAwait(false).GetAwaiter().GetResult();
            }
            catch (Exception ex) {
                Logger.Error("Unable to remove update task", ex);
            }
        }

        private string GetBuiltInUpdateChannel()
        {
#if STABLE
            return "stable";
#endif

#if BETA
            return "beta";
#endif

            return "alpha";
        }
        private string GetUpdateChannel()
        {
            try
            {
                var result = AppRegistry.Get(AppRegistryKeys.AutoUpdateChannel);
                if(result != null && updateChannels.Contains(result))
                {
                    return result;
                }
            }catch(Exception err)
            {
                Logger.Warn("Unable to get AutoUpdateChannel via registry. Using built-in channel");
            }
            return GetBuiltInUpdateChannel();
        }

        private bool GetAutoUpdateFlag()
        {
            try
            {
                var result = AppRegistry.Get(AppRegistryKeys.DisableAutoUpdate);

                if (result != null) {
                    return bool.Parse(result);
                }
            }catch(Exception err)
            {
                Logger.Warn("Unable to get autoupdate flag. Defaulting to true");
            }
            return true;
        }

        public async Task CheckForUpdate()
        {
            currentUpdaterCts = new CancellationTokenSource();
            bool canUpdate = GetAutoUpdateFlag();
            if (!canUpdate) {
                Logger.Info("Skipping checking for update. AutoUpdate is disabled via registry");
                currentUpdaterCts.Cancel();
                return;
            }
            Logger.Info("Checking for update");
            try
            {
                var currentVersion = GetInstalledVersion();
                var latestInfo = await updaterApi.GetLatest(GetUpdateChannel());
                var latestVersion = SemVersion.Parse(latestInfo.Version);

                bool updateAvailable = latestVersion.ComparePrecedenceTo(currentVersion) > 0;
                if (!updateAvailable)
                {
                    Logger.Info($"No updates are available at this time. Current installed version is: {currentVersion}, remote version is: {latestVersion}");
                    return;
                }

                Logger.Info($"Update is available. Remote version: {latestVersion}, Installed version: {currentVersion}");

                string downloadDir = Path.GetTempPath();

                var installerDownloadPath = await updaterApi.Download(latestInfo, downloadDir);

                bool validDownload = VerifyInstall(latestInfo.Checksum, installerDownloadPath);
                if (!validDownload)
                {
                    throw new Exception("Checksum missmatch");
                }
                else
                {
                    Logger.Info("Starting install process");
                    await updateOrchestrator.Install(installerDownloadPath, currentUpdaterCts);
                }
            }
            catch (Exception ex)
            {
                currentUpdaterCts.Cancel();
                Logger.Error("Unable to check for updates at this time", ex);
            }
        }

        private bool VerifyInstall(string expectedChecksum, string downloadedInstallerPath)
        {
            Logger.Info("Verifying file");
            using (FileStream stream = File.OpenRead(downloadedInstallerPath))
            using (SHA256 sha256 = SHA256.Create())
            {
                byte[] hashBytes = sha256.ComputeHash(stream);
                string actualChecksum = BitConverter.ToString(hashBytes).Replace("-", "").ToLowerInvariant();

                return string.Equals(expectedChecksum.ToLowerInvariant(), actualChecksum, StringComparison.OrdinalIgnoreCase);
            }
        }

        private SemVersion GetInstalledVersion()
        {
            var mainSrvBinPath = PathResolver.GetYTControllerSrvBin();
            var versionInfo = FileVersionInfo.GetVersionInfo(mainSrvBinPath);
            var major = versionInfo.FileMajorPart;
            var privatePart = versionInfo.FilePrivatePart;
            var minor = versionInfo.FileMinorPart;
            var build = versionInfo.FileBuildPart;
            var channel = GetBuiltInUpdateChannel();
            var semVer = $"{major}.{minor}.{build}-{channel}.{privatePart}";

            SemVersion.TryParse(semVer, SemVersionStyles.Any, out var version);

            return version;
        }

        public void Cleanup()
        {
            currentUpdaterCts.Cancel();
            currentUpdaterCts.Dispose();
        }
    }
}
