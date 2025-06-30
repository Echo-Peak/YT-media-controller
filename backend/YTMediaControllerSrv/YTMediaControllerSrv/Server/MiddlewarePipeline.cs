using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;

namespace YTMediaControllerSrv.Server
{
    internal class MiddlewarePipeline
    {
        public delegate Task MiddlewareDelegate(HttpListenerContext context, Func<Task> next);
        private readonly List<MiddlewareDelegate> middlewares = new List<MiddlewareDelegate>();

        public void Use(MiddlewareDelegate middleware)
        {
            middlewares.Add(middleware);
        }

        public Func<HttpListenerContext, Task> Build()
        {
            return async context =>
            {
                int index = -1;

                async Task Next()
                {
                    index++;
                    if (index < middlewares.Count)
                    {
                        var current = middlewares[index];
                        await current(context, Next);
                    }
                }

                await Next();
            };
        }
    }
}
