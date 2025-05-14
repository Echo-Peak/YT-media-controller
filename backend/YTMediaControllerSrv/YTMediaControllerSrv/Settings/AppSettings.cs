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
        public AppSettingsJson settings = new AppSettingsJson();
        private string settingsFilePath { get; set; }

        public AppSettings(string settingsFilePath)
        {
            this.settingsFilePath = settingsFilePath;
            LoadAppSettingsFile(settingsFilePath);
        }

        private void LoadAppSettingsFile(string settingsFile)
        {
            try
            {
                string content = File.ReadAllText(settingsFile);
                this.settings = JsonConvert.DeserializeObject<AppSettingsJson>(content);
            }
            catch (Exception err)
            {
                Console.WriteLine("Unable to read settings. Using default settings");
                Console.WriteLine(err.Message);
            }
        }
        public void UpdateSettingsFile(string property, object value)
        {
            try
            {
                settings.Update(property, value);
                string json = JsonConvert.SerializeObject(settings, Formatting.Indented);
                File.WriteAllText(settingsFilePath, json);
                Console.WriteLine($"Updated {property} to {value}");
            }
            catch (Exception err)
            {
                Console.WriteLine("Unable to update settings.");
                Console.WriteLine(err.Message);
            }
        }
    }
}
