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
using System.Threading;

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
        private static Stream output;
        private static readonly object outputLock = new object();

        static void Main(string[] args)
        {
            appSettings = new AppSettings(PathResolver.GetSettingsFilePath());

            var input = Console.OpenStandardInput();
            output = Console.OpenStandardOutput();

            Thread readerThread = new Thread(() => ListenForMessages(input));
            readerThread.Start();
        }


        private static void ListenForMessages(Stream input)
        {
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
                    SendMessageToExtension(actionResponse);
                }
            }
            catch (Exception ex)
            {
                Console.Error.WriteLine("Error: " + ex);
            }
        }

        public static void SendMessageToExtension(object messageObj)
        {
            string responseJson = JsonConvert.SerializeObject(messageObj);
            byte[] responseBytes = Encoding.UTF8.GetBytes(responseJson);
            byte[] lengthPrefix = BitConverter.GetBytes(responseBytes.Length);

            lock (outputLock)
            {
                output.Write(lengthPrefix, 0, 4);
                output.Write(responseBytes, 0, responseBytes.Length);
                output.Flush();
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
                            backendServerPort = settings.BackgroundServerPort
                        };
                    }
                case "getDeviceNetworkIp":
                    {
                        return new
                        {
                            status = true,
                            deviceNetworkIp = DeviceInfo.GetLocalIPAddress()
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
            }
            return new { status = false, message = $"Unknown action \"{response.Action}\"" };
        }
    }
}
