using Microsoft.Win32;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using YTMediaControllerSrv;
using YTMediaControllerSrv.Types;

namespace YTMediaControllerSrv
{
    internal class BrowserExtensionNativeHost
    {
        public BrowserExtensionNativeHost()
        {
            var (nativeHostId, nativeHostPath) = SetupManifest();
            SetupRegistryEntry(nativeHostId, nativeHostPath);
        }

        private (string, string) SetupManifest()
        {
            string nativeHostManifestPath = PathResolver.GetNativeHostManifestPath();
            string nativeHostJson = File.ReadAllText(nativeHostManifestPath);
            NativeHostManifestJson manifest = JsonConvert.DeserializeObject<NativeHostManifestJson>(nativeHostJson);
            manifest.path = PathResolver.GetNativeHostBinPath();
            File.WriteAllText(nativeHostManifestPath, JsonConvert.SerializeObject(manifest, Formatting.Indented));
            Logger.Info($"Updated native host manifest at {nativeHostManifestPath}");

            return (manifest.name, nativeHostManifestPath);
        }

        private void SetupRegistryEntry(string nativeHostId, string nativeHostPath)
        {
            const string googleKeyPath = @"SOFTWARE\Google";
            const string nativeMessagingPath = @"SOFTWARE\Google\Chrome\NativeMessagingHosts";

            using (RegistryKey googleKey = Registry.LocalMachine.OpenSubKey(googleKeyPath))
            {
                if (googleKey == null)
                {
                    Logger.Warn("Google registry key not found.");
                    return;
                }
            }

            using (RegistryKey baseKey = Registry.LocalMachine.CreateSubKey(nativeMessagingPath))
            {
                if (baseKey == null)
                {
                    Logger.Warn("Failed to open or create NativeMessagingHosts key.");
                    return;
                }

                using (RegistryKey hostKey = baseKey.CreateSubKey(nativeHostId))
                {
                    if (hostKey == null)
                    {
                        Logger.Warn($"Failed to create key for {nativeHostId}");
                        return;
                    }

                    hostKey.SetValue(null, nativeHostPath);
                    Logger.Info($"Successfully registered {nativeHostId} with manifest path: {nativeHostPath}");
                }
            }
        }
    }
}
