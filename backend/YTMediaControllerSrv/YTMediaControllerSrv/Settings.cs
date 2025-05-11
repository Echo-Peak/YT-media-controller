using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace YTMediaControllerSrv
{
    internal class AppSettingsJson
    {
        public int BackgroundServerPort { get; set; }
        public int ControlServerPort { get; set; }
    }

    internal class AppSettings
    {
        public int BackgroundServerPort { get; set; } = 9200;
        public int ControlServerPort { get; set; } = 9300;

        public AppSettings(string settingsFilePath)
        {
            try
            {
                string content = File.ReadAllText(settingsFilePath);

                var settings = JsonConvert.DeserializeObject<AppSettingsJson>(content);
                if (settings != null)
                {
                    if (settings.BackgroundServerPort > 0)
                    {
                        BackgroundServerPort = settings.BackgroundServerPort;
                    }
                    else
                    {
                        Console.WriteLine($"Could not find BackgroundServerPort property in settings.json. Defaulting to {BackgroundServerPort}");
                    }

                    if (settings.ControlServerPort > 0)
                    {
                        ControlServerPort = settings.ControlServerPort;
                    }
                    else
                    {
                        Console.WriteLine($"Could not find ControlServerPort property in settings.json. Defaulting to {ControlServerPort}");
                    }
                }
            }
            catch (Exception err)
            {
                Console.WriteLine("Unable to read settings. Using default settings");
                Console.WriteLine(err.Message);
            }
        }
    }
}
