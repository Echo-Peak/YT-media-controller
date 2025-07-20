using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace YTMediaControllerSrv.Settings
{
    public class AppSettingsJson
    {
        public int BackendServerPort;
        public int UISocketServerPort;

        public AppSettingsJson(int backendServerPort, int uiSocketServerPort) {
            BackendServerPort = backendServerPort;
            UISocketServerPort = uiSocketServerPort;
        }
        public void Update(string property, object value)
        {
            switch (property)
            {
                case "BackendServerPort":
                    BackendServerPort = Convert.ToInt32(value);
                    break;
                default:
                    throw new ArgumentException($"Property '{property}' not found.");
            }
        }
    }
}
