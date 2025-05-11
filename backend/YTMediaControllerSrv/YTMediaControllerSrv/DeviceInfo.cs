using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Sockets;
using System.Net;
using System.Text;
using System.Threading.Tasks;

namespace YTMediaControllerSrv
{
    internal class DeviceInfo
    {
        public static string GetLocalIPAddress()
        {
            return Dns.GetHostEntry(Dns.GetHostName())
                              .AddressList
                              .FirstOrDefault(ip => ip.AddressFamily == AddressFamily.InterNetwork &&
                                                    ip.ToString().StartsWith("192.168."))?
                              .ToString() ?? "No 192.168.x.x IP found";
        }
    }
}
