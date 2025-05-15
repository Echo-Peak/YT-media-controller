using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace YTMediaControllerSrv.Settings
{
    public class AppSettingsJson
    {
        public int BackgroundServerPort { get; set; } = 9200;

        public void Update(string property, object value)
        {
            switch (property)
            {
                case "BackgroundServerPort":
                    BackgroundServerPort = Convert.ToInt32(value);
                    break;
                default:
                    throw new ArgumentException($"Property '{property}' not found.");
            }
        }
    }
}
