using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime;
using System.Text;
using System.Threading.Tasks;
using YTMediaControllerSrv.Settings;
using YTMediaControllerSrv;

namespace YTMediaControllerHost
{
    class JsonResponse
    {
        public string Action { get; set; }
        public int Port { get; set; }
    }

    internal class Program
    {
        static AppSettings appSettings { get; set; }
        static void Main(string[] args)
        {
            appSettings = new AppSettings(PathResolver.GetSettingsFilePath());
            var input = Console.OpenStandardInput();
            var output = Console.OpenStandardOutput();

            try
            {
                while (true)
                {
                    byte[] lengthBytes = new byte[4];
                    int bytesRead = input.Read(lengthBytes, 0, 4);
                    if (bytesRead == 0) break;

                    int messageLength = BitConverter.ToInt32(lengthBytes, 0);
                    if (messageLength <= 0 || messageLength > 10_000) break;

                    byte[] buffer = new byte[messageLength];
                    int total = 0;
                    while (total < messageLength)
                    {
                        int read = input.Read(buffer, total, messageLength - total);
                        if (read == 0) break;
                        total += read;
                    }

                    string requestJson = Encoding.UTF8.GetString(buffer);
                    var data = JsonConvert.DeserializeObject<JsonResponse>(requestJson);

                    if (data == null || data.Action == null) continue;


                    object actionResponse = HandleAction(data);

                    string responseJson = JsonConvert.SerializeObject(actionResponse);
                    byte[] responseBytes = Encoding.UTF8.GetBytes(responseJson);

                    output.Write(BitConverter.GetBytes(responseBytes.Length), 0, 4);
                    output.Write(responseBytes, 0, responseBytes.Length);
                    output.Flush();
                }
            }
            catch (Exception ex)
            {
                Console.Error.WriteLine("Error: " + ex);
            }
        }

        static object HandleAction(JsonResponse response)
        {
            var settings = appSettings.settings;
            switch (response.Action)
            {
                case "getBackendSettings":
                    {
                        return new
                        {
                            status = true,
                            backendServerPort = settings.BackgroundServerPort,
                            controlServerPort = settings.ControlServerPort
                        };
                    }
                case "getDeviceNetworkIp":
                    {
                        return new
                        {
                            status = true,
                            deviceNetworkIp = YTMediaControllerSrv.DeviceInfo.GetLocalIPAddress()
                        };
                    }
                case "updateBackendServerPort":
                    {
                        try
                        {
                            appSettings.UpdateSettingsFile("BackgroundServerPort", response.Port);
                            return new { status = true };
                        }catch (Exception err)
                        {
                            return new
                            {
                                status = false,
                                message = "Unable to update backend server port",
                                error = err.Message
                            };
                        }
                        
                    }
                case "updateControlServerPort":
                    {
                        try
                        {
                            appSettings.UpdateSettingsFile("ControlServerPort", response.Port);
                            return new { status = true };
                        }
                        catch (Exception err)
                        {
                            return new
                            {
                                status = false,
                                message = "Unable to update control server port",
                                error = err.Message
                            };
                        }
                    }
            }
            return new { status = false, message = $"Unknown action \"{response.Action}\"" };
        }
    }
}
