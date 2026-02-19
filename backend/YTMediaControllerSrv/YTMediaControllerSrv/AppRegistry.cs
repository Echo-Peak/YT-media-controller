using System;
using System.Runtime.InteropServices;

#if !WINDOWS
using YTMediaControllerSrv;
#endif

namespace YTMediaControllerSrv
{
    public static class AppRegistryKeys
    {
        public static string BackendServerPort = "backendServerPort";
        public static string UISocketServerPort = "uiSocketServerPort";
        public static string DisableAutoUpdate = "disableAutoUpdate";
        public static string AutoUpdateIntervalMins = "autoUpdateIntervalMins";
        public static string AutoUpdateChannel = "autoUpdateChannel";
    }

    public class AppRegistry
    {
#if WINDOWS
        static string appPath = "SOFTWARE\\YTMediaController";

        public static void Update(string key, string value)
        {
            using (var root = Microsoft.Win32.Registry.LocalMachine.OpenSubKey(appPath, true))
            {
                if(root != null)
                {
                    root.SetValue(key, value, Microsoft.Win32.RegistryValueKind.String);
                }
                else
                {
                    using (var newRoot = Microsoft.Win32.Registry.LocalMachine.CreateSubKey(appPath))
                    {
                        newRoot.SetValue(key, value, Microsoft.Win32.RegistryValueKind.String);
                    }
                }
            }
        }

        public static string Get(string key)
        {
            using (var root = Microsoft.Win32.Registry.LocalMachine.OpenSubKey(appPath))
            {
                if (root != null) { 
                    var result = root.GetValue(key);
                    return result == null ? null : result.ToString();
                }
            }

            return null;
        }

        public void Remove(string key)
        {
            using (var root = Microsoft.Win32.Registry.LocalMachine.OpenSubKey(appPath, true))
            {
                if (root != null) { 
                    root.DeleteValue(key);
                }
            }
        }
#else
        public static string Get(string key)
        {
            return LinuxConfig.Get(key);
        }

        public static void Update(string key, string value)
        {
            LinuxConfig.Update(key, value);
        }
#endif
    }
}
