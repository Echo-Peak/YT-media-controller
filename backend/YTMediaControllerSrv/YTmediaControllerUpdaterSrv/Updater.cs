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
        private readonly GHReleases gHReleases;
        private readonly UpdateOrchestrator updateOrchestrator;
        private CancellationTokenSource currentUpdaterCts;
        public Updater(ILogger logger, GHReleases GHRelease)
        {
            this.Logger = logger;
            this.gHReleases = GHRelease;
            this.updateOrchestrator = new UpdateOrchestrator(logger);
        }

        private string SelectChannel()
        {
#if RELEASE
            return "release";
#endif

#if STAGING
            return "staging"
#endif

            return "dev";
        }

        public async Task CheckForUpdate()
        {
            currentUpdaterCts = new CancellationTokenSource();
            Logger.Info("Checking for update");
            try
            {
                var currentVersion = GetInstalledVersion();
                var latestVersion = await gHReleases.GetLatest(SelectChannel());

                bool updateAvailable = latestVersion.ComparePrecedenceTo(currentVersion) > 0;
                if (!updateAvailable)
                {
                    Logger.Info($"No updates are available at this time. Current installed version is: {currentVersion}, remote version is: {latestVersion}");
                    return;
                }

                Logger.Info($"Update is available. Remote version: {latestVersion}, Installed version: {currentVersion}");

                var manifest = await gHReleases.GetAsset<ManifestData>("manifest.json");
                string downloadDir = Path.GetTempPath();

                var installerDownloadPath = await gHReleases.DownloadAsset(manifest.InstallerComponent, downloadDir);

                bool validDownload = VerifyInstall(manifest, installerDownloadPath);
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

        private bool VerifyInstall(ManifestData manifest, string downloadedInstallerPath)
        {
            Logger.Info("Verifying file");
            using (FileStream stream = File.OpenRead(downloadedInstallerPath))
            using (SHA256 sha256 = SHA256.Create())
            {
                byte[] hashBytes = sha256.ComputeHash(stream);
                string actualChecksum = BitConverter.ToString(hashBytes).Replace("-", "").ToLowerInvariant();

                return string.Equals(manifest.Sha256Checksum.ToLowerInvariant(), actualChecksum, StringComparison.OrdinalIgnoreCase);
            }
        }

        private SemVersion GetInstalledVersion()
        {
            var mainSrvBinPath = PathResolver.GetYTControllerSrvBin();
            var versionInfo = FileVersionInfo.GetVersionInfo(mainSrvBinPath);
            var major = versionInfo.ProductMajorPart;
            var privatePart = versionInfo.ProductPrivatePart;
            var minor = versionInfo.ProductMinorPart;
            var build = versionInfo.ProductBuildPart;
            var channel = SelectChannel();
            var semVer = $"{major}.{privatePart}.{minor}-{channel}.{build}";

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
