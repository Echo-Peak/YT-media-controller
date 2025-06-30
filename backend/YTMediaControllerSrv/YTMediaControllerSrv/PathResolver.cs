using System;
using System.Collections.Generic;
using System.Configuration.Install;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace YTMediaControllerSrv
{
    public class PathResolver
    {
        static string installDir = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ProgramFiles), "YTMediaController");

        private static bool IsInstalled()
        {
            return AppDomain.CurrentDomain.BaseDirectory.StartsWith(installDir, StringComparison.OrdinalIgnoreCase);
        }

        private static string GetProjectRoot()
        {
            return Directory.GetParent(AppDomain.CurrentDomain.BaseDirectory).Parent.Parent.Parent.Parent.Parent.FullName;
        }

        public static string GetSettingsFilePath()
        {
            if (!IsInstalled())
            {
                return Path.Combine(GetProjectRoot(), "backend\\debug.settings.json");
            }

            return Path.Combine(installDir, "settings.json");
        }

        static string GetBrowserExtentionDir()
        {
            if(!IsInstalled())
            {
                return Path.Combine(GetProjectRoot(), "extension\\build");
            }
            return Path.Combine(installDir, "ui");
        }

        public static string GetNativeHostManifestPath()
        {
            return Path.Combine(GetBrowserExtentionDir(), "nativeHost.json");
        }

        public static string GetNativeHostBinPath()
        {
            if (!IsInstalled())
            {
                string configuration = new DirectoryInfo(AppDomain.CurrentDomain.BaseDirectory).Name;
                Console.WriteLine(configuration);
                Console.WriteLine(GetProjectRoot());
                return Path.Combine(GetProjectRoot(), $"backend\\YTMediaControllerSrv\\YTMediaControllerHost\\bin\\{configuration}\\YTMediaControllerHost.exe");
            }
            return Path.Combine(installDir, "YTMediaControllerHost.exe");
        }

        public static string GetYtDlpBin()
        {
            if (!IsInstalled())
            {
                string configuration = new DirectoryInfo(AppDomain.CurrentDomain.BaseDirectory).Name;
                return Path.Combine(GetProjectRoot(), $"backend\\externalBins\\yt-dlp.exe");
            }
            return Path.Combine(installDir, "bin/yt-dlp.exe");
        }

        public static string GetFFMpegDir()
        {
            if (!IsInstalled())
            {
                string configuration = new DirectoryInfo(AppDomain.CurrentDomain.BaseDirectory).Name;
                return Path.Combine(GetProjectRoot(), $"backend\\externalBins\\ffmpeg");
            }
            return Path.Combine(installDir, "bin");
        }
    }
}
