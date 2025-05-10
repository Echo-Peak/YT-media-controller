using System;
using System.Text;
using System.Text.Json;
using WindowsInput;

namespace ChromeNativeMessagingHost
{
    class JsonResponse
    {
        public string Action { get; set; }
    }
    internal class Program
    {
        static IKeyboardSimulator keyboardEmulator = new InputSimulator().Keyboard;
        static void Main(string[] args)
        {
            var input = Console.OpenStandardInput();

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
                    var data = JsonSerializer.Deserialize<JsonResponse>(requestJson);

                    if (data == null || data.Action == null) continue;

                    HandleAction(data.Action);
                }
            }
            catch (Exception ex)
            {
                Console.Error.WriteLine("Error: " + ex);
            }
        }

        static void HandleAction(string action)
        {
            switch (action) {
                case "enterFullScreen":
                    {
                        keyboardEmulator.ModifiedKeyStroke(WindowsInput.Native.VirtualKeyCode.LWIN, WindowsInput.Native.VirtualKeyCode.VK_F);
                        break;
                    }
            }

        }
    }
}
