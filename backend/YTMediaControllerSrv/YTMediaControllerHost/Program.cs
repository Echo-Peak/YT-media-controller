using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace YTMediaControllerHost
{
    class JsonResponse
    {
        public string Action { get; set; }
        public int Port { get; set; }
    }

    internal class Program
    {
        static void Main(string[] args)
        {
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


                    bool actionStatus = HandleAction(data);

                    var response = new { status = actionStatus };
                    string responseJson = JsonConvert.SerializeObject(response);
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

        static bool HandleAction(JsonResponse response)
        {
            switch (response.Action)
            {
                case "updateBackendServerPort":
                    {
                        return true;
                    }
                case "updateControlServerPort":
                    {
                        return true;
                    }
            }
            return false;
        }
    }
}
