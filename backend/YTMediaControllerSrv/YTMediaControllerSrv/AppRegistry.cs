using Microsoft.Win32;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace YTMediaControllerSrv
{
    public static class AppRegistryKeys
    {
            public static string BackendServerPort = "backendServerPort";
            public static string UISocketServerPort = "uiSocketServerPort";

     }
    public class AppRegistry
    {
        static string appPath = "SOFTWARE\\YTMediaController";

        public static void Update(string key, string value)
        {
            using (var root = Registry.LocalMachine.OpenSubKey(appPath, true))
            {
                if(root != null)
                {
                    root.SetValue(key, value, RegistryValueKind.String);
                }
                else
                {
                    using (var newRoot = Registry.LocalMachine.CreateSubKey(appPath))
                    {
                        newRoot.SetValue(key, value, RegistryValueKind.String);
                    }
                }
            }
        }

        public static string Get(string key)
        {
            using (var root = Registry.LocalMachine.OpenSubKey(appPath))
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
            using (var root = Registry.LocalMachine.OpenSubKey(appPath, true))
            {
                if (root != null) { 
                    root.DeleteValue(key);
                }
            }
        }
    }
}
