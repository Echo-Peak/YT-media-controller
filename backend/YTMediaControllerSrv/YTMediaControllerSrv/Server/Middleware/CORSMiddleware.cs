using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;

namespace YTMediaControllerSrv.Server.Middleware
{
    internal class CORSMiddleware
    {
        public Task Invoke(HttpListenerContext context, Func<Task> next)
        {
            var response = context.Response;
            var request = context.Request;

            response.Headers.Add("Access-Control-Allow-Origin", "*");
            response.Headers.Add("Access-Control-Allow-Methods", "GET, POST, OPTIONS");
            response.Headers.Add("Access-Control-Allow-Headers", "Content-Type, Authorization");

            if (request.HttpMethod == "OPTIONS")
            {
                response.StatusCode = 200;
                response.Close();
                return Task.CompletedTask;
            }

            return next();
        }
    }
}
