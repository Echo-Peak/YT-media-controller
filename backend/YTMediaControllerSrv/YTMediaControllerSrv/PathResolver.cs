using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace YTMediaControllerSrv
{
    public class PathResolver
    {
#if WINDOWS
        static string installDir = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ProgramFiles), "YTMediaController");
#else
        static string installDir = "/opt/ytmediacontroller";
#endif

        private static bool IsInstalled()
        {
            return AppDomain.CurrentDomain.BaseDirectory.StartsWith(installDir, StringComparison.OrdinalIgnoreCase);
        }

        private static string GetProjectRoot()
        {
            return Directory.GetParent(AppDomain.CurrentDomain.BaseDirectory).Parent.Parent.Parent.Parent.Parent.Parent.FullName;
        }


        static string GetBrowserExtentionDir()
        {
            if (!IsInstalled())
            {
#if WINDOWS
                return Path.Combine(GetProjectRoot(), "extension\\build");
#else
                return Path.Combine(GetProjectRoot(), "extension/build");
#endif
            }
            return Path.Combine(installDir, "ui");
        }

        public static string GetYtDlpBin()
        {
            if (!IsInstalled())
            {
                string configuration = new DirectoryInfo(AppDomain.CurrentDomain.BaseDirectory).Name;
#if WINDOWS
                return Path.Combine(GetProjectRoot(), $"backend\\externalBins\\yt-dlp.exe");
#else
                return Path.Combine(GetProjectRoot(), "backend/externalBins/yt-dlp");
#endif
            }
#if WINDOWS
            return Path.Combine(installDir, "bin/yt-dlp.exe");
#else
            return Path.Combine(installDir, "bin/yt-dlp");
#endif
        }

        public static string GetFFMpegDir()
        {
            if (!IsInstalled())
            {
                string configuration = new DirectoryInfo(AppDomain.CurrentDomain.BaseDirectory).Name;
#if WINDOWS
                return Path.Combine(GetProjectRoot(), $"backend\\externalBins\\ffmpeg");
#else
                return Path.Combine(GetProjectRoot(), "backend/externalBins/ffmpeg");
#endif
            }
            return Path.Combine(installDir, "bin");
        }

        public static string GetYTControllerSrvBin()
        {
            var binName = "YTMediaControllerSrv";
            if (!IsInstalled())
            {
                string configuration = new DirectoryInfo(AppDomain.CurrentDomain.BaseDirectory).Name;
#if WINDOWS
                return Path.Combine(GetProjectRoot(), $"backend\\YTMediaControllerSrv\\{binName}\\bin\\{configuration}\\{binName}.exe");
#else
                return Path.Combine(GetProjectRoot(), $"backend/YTMediaControllerSrv/{binName}/bin/{configuration}/{binName}");
#endif
            }
#if WINDOWS
            return Path.Combine(installDir, $"{binName}.exe");
#else
            return Path.Combine(installDir, binName);
#endif
        }

        public static string GetPublicKey()
        {
            if (!IsInstalled())
            {
                string configuration = new DirectoryInfo(AppDomain.CurrentDomain.BaseDirectory).Name;
#if WINDOWS
                return Path.Combine(GetProjectRoot(), $"backend\\keys\\updateServerSrv.pub");
#else
                return Path.Combine(GetProjectRoot(), "backend/keys/updateServerSrv.pub");
#endif
            }

            return Path.Combine(installDir, "keys/updateServerSrv.pub");
        }

        public static string GetUninstaller() 
        {
            if (!IsInstalled())
            {
                string configuration = new DirectoryInfo(AppDomain.CurrentDomain.BaseDirectory).Name;
#if WINDOWS
                return Path.Combine(GetProjectRoot(), $"dist/YoutubeMediaControllerUninstaller.exe");
#else
                return Path.Combine(GetProjectRoot(), "dist/YoutubeMediaControllerUninstaller");
#endif
            }

#if WINDOWS
            return Path.Combine(installDir, $"YoutubeMediaControllerUninstaller.exe");
#else
            return Path.Combine(installDir, "YoutubeMediaControllerUninstaller");
#endif
        }
        public static string GetLogsDir()
        {
#if WINDOWS
            return Path.Combine(installDir, "logs");
#else
            return "/var/log/ytmediacontroller";
#endif
        }
    }
}
