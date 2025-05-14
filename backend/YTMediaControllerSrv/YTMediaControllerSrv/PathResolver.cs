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
        public static string GetSettingsFilePath()
        {
#if DEBUG
            var installDir = Directory.GetParent(AppDomain.CurrentDomain.BaseDirectory).Parent.Parent.Parent.Parent.FullName;
            return Path.Combine(installDir, "debug.settings.json");
#else
            return Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "settings.json");
#endif
        }
    }
}
