using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;

namespace YTMediaControllerSrv.Server.Middleware
{
    internal interface IHttpMiddleware
    {
        Task Invoke(HttpListenerContext context, Func<Task> next);
    }
}
