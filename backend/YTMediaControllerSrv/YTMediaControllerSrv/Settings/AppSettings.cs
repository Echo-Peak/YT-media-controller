using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Data;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace YTMediaControllerSrv.Settings
{
    public class AppSettings
    {
        private string settingsFilePath { get; set; }

        public AppSettings(string settingsFilePath)
        {
            this.settingsFilePath = settingsFilePath;
        }

        public AppSettingsJson Load()
        {
            try
            {
                string content = File.ReadAllText(settingsFilePath);
                return JsonConvert.DeserializeObject<AppSettingsJson>(content);
            }
            catch (Exception err)
            {
                Logger.Error("Unable to read settings. Using default settings", err);
                return new AppSettingsJson(9200, 9201);
            }
        }
    }
}
