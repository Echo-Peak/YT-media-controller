using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;

namespace YTMediaControllerSrv.Server.Middleware
{
    internal class BaseMiddleware : IHttpMiddleware
    {
        public async Task Invoke(HttpListenerContext context, Func<Task> next)
        {
            var response = context.Response;

            if (response.OutputStream.CanWrite)
            {
                response.StatusCode = (int)HttpStatusCode.NotFound;
                response.ContentType = "text/html";

                var content = "<html><body><h1>404 Not Found</h1></body></html>";
                byte[] buffer = Encoding.UTF8.GetBytes(content);
                response.ContentLength64 = buffer.Length;

                await response.OutputStream.WriteAsync(buffer, 0, buffer.Length);
            }

            try { response.OutputStream.Close(); } catch { }
            try { response.Close(); } catch { }
        }
    }
}
