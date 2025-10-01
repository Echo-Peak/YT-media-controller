using System;
using YTMediaControllerSrv.Logging;

namespace YTMediaControllerSrv.Settings
{
    public class AppSettings
    {
        public int BackendServerPort = 60166;
        public int UISocketServerPort = 52000;

        private readonly ILogger logger;
        public AppSettings(ILogger Logger) {
            logger = Logger;
            SetBackendPort();
            SetUISocketServerPort();
        }

        private void SetBackendPort()
        {
            try
            {
                string backendServerPort = AppRegistry.Get(AppRegistryKeys.BackendServerPort);
                if (backendServerPort != null)
                {
                    BackendServerPort = Convert.ToInt32(backendServerPort);
                }
                else
                {
                    UpdateRegistry(AppRegistryKeys.BackendServerPort, BackendServerPort.ToString());
                }
            }
            catch (Exception err) {
                logger.Error("Unable to set backend port", err);
            }
        }

        private void SetUISocketServerPort()
        {
            try
            {
                string uiSocketServerPort = AppRegistry.Get(AppRegistryKeys.UISocketServerPort);
                if (uiSocketServerPort != null)
                {
                    UISocketServerPort = Convert.ToInt32(uiSocketServerPort);
                }
                else
                {
                    UpdateRegistry(AppRegistryKeys.UISocketServerPort, UISocketServerPort.ToString());
                }
            }catch (Exception err)
            {
                logger.Error("Unable to set UI socket server port", err);
            }
        }

        private void UpdateRegistry(string key, string value)
        {
            try
            {
                AppRegistry.Update(AppRegistryKeys.BackendServerPort, BackendServerPort.ToString());
            }catch(Exception err)
            {
                logger.Error($"Unable to update {key}", err);
            }
        }
        public void Update(string property, object value)
        {
            switch (property)
            {
                case "BackendServerPort":
                    BackendServerPort = Convert.ToInt32(value);
                    UpdateRegistry(AppRegistryKeys.BackendServerPort, BackendServerPort.ToString());
                    break;
                case "UISocketServerPort":
                    UISocketServerPort = Convert.ToInt32(value);
                    UpdateRegistry(AppRegistryKeys.UISocketServerPort, UISocketServerPort.ToString());
                    break;
                default:
                    throw new ArgumentException($"Property '{property}' not found.");
            }
        }
    }
}
