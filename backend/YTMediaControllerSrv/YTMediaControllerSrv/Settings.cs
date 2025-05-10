using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace YTMediaControllerSrv
{
    internal class SettingsJSON
    {
        public int UIPort { get; set; }
        public int APIServerPort { get; set; }

        public SettingsJSON()
        {
            UIPort = 8080;
            APIServerPort = 9000;
        }
    }

    internal class ReadSettings
    {
        public SettingsJSON settings = new SettingsJSON();

        public ReadSettings()
        {
            try
            {
                var installDir = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ProgramFilesX86), "YTMediaControllerSrv");
                string settingsFile = Path.Combine(installDir, "settings.json");
                string content = File.ReadAllText(settingsFile);

                settings = JsonConvert.DeserializeObject<SettingsJSON>(content);
            }
            catch (Exception err)
            {
                Console.WriteLine("Unable to read settings. Using default settings");
                Console.WriteLine(err.Message);
            }
        }
    }
}
