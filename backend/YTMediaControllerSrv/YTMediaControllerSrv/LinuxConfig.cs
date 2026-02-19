using System;
using System.IO;
using System.Text.Json;

namespace YTMediaControllerSrv
{
    public class AppSettingsConfig
    {
        public string BackendServerPort { get; set; } = "60166";
        public string UISocketServerPort { get; set; } = "52000";
        public string DisableAutoUpdate { get; set; } = "false";
        public string AutoUpdateIntervalMins { get; set; } = "320";
        public string AutoUpdateChannel { get; set; } = "stable";
    }

    public static class LinuxConfig
    {
        private static readonly string ConfigPath = "/etc/ytmediacontroller/config.json";
        private static readonly string DefaultConfigPath = "appSettings.json";
        private static AppSettingsConfig _config;

        public static void Load()
        {
            if (_config != null) return;

            // Try to load from /etc/ytmediacontroller/config.json first
            if (File.Exists(ConfigPath))
            {
                try
                {
                    var json = File.ReadAllText(ConfigPath);
                    _config = JsonSerializer.Deserialize<AppSettingsConfig>(json) ?? new AppSettingsConfig();
                    return;
                }
                catch
                {
                    // Fall through to defaults
                }
            }

            // Try to load from working directory (for development)
            if (File.Exists(DefaultConfigPath))
            {
                try
                {
                    var json = File.ReadAllText(DefaultConfigPath);
                    _config = JsonSerializer.Deserialize<AppSettingsConfig>(json) ?? new AppSettingsConfig();
                    return;
                }
                catch
                {
                    // Fall through to defaults
                }
            }

            // Use defaults
            _config = new AppSettingsConfig();
        }

        public static void EnsureConfigDirectory()
        {
            var dir = Path.GetDirectoryName(ConfigPath);
            if (!Directory.Exists(dir))
            {
                Directory.CreateDirectory(dir);
            }
        }

        public static string Get(string key)
        {
            Load();
            return key switch
            {
                "backendServerPort" => _config.BackendServerPort,
                "uiSocketServerPort" => _config.UISocketServerPort,
                "disableAutoUpdate" => _config.DisableAutoUpdate,
                "autoUpdateIntervalMins" => _config.AutoUpdateIntervalMins,
                "autoUpdateChannel" => _config.AutoUpdateChannel,
                _ => null
            };
        }

        public static void Update(string key, string value)
        {
            Load();
            EnsureConfigDirectory();

            switch (key)
            {
                case "backendServerPort":
                    _config.BackendServerPort = value;
                    break;
                case "uiSocketServerPort":
                    _config.UISocketServerPort = value;
                    break;
                case "disableAutoUpdate":
                    _config.DisableAutoUpdate = value;
                    break;
                case "autoUpdateIntervalMins":
                    _config.AutoUpdateIntervalMins = value;
                    break;
                case "autoUpdateChannel":
                    _config.AutoUpdateChannel = value;
                    break;
            }

            var json = JsonSerializer.Serialize(_config, new JsonSerializerOptions { WriteIndented = true });
            File.WriteAllText(ConfigPath, json);
        }
    }
}
