using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;
using YTMediaControllerSrv.Logging;

namespace YTMediaControllerSrv.Server.Middleware
{
    internal class LoggingMiddleware : IHttpMiddleware
    {
        private readonly ILogger Logger;
        public LoggingMiddleware(ILogger logger) { 
            Logger = logger;
        }
        public async Task Invoke(HttpListenerContext context, Func<Task> next)
        {
            string url = context.Request.Url.OriginalString;
            string method = context.Request.HttpMethod.ToString();
            Logger.Debug($"{method} - {url}");
            await next();
        }
    }
}
