using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;
using YTMediaControllerSrv.Streaming;
using YTMediaControllerSrv.Types;

namespace YTMediaControllerSrv.Server.Middleware
{
    internal class MobileRouterMiddleware : IHttpMiddleware
    {
        private StreamingService StreamingService {  get; set; }
        private UISocketServer UISocketServer { get; set; }
        private string BaseUrl { get; set; }

        public MobileRouterMiddleware(StreamingService streamingService,  UISocketServer uiSocketServer, string baseUrl)
        {
            StreamingService = streamingService;
            UISocketServer = uiSocketServer;
            BaseUrl = baseUrl;
        }
        public async Task Invoke(HttpListenerContext context, Func<Task> next)
        {
            string requestedPath = context.Request.Url.AbsolutePath.TrimStart('/');
            var isMobile = requestedPath.StartsWith("mobile");
            var isPost = context.Request.HttpMethod == "POST";

            if (isMobile && isPost)
            {
                switch (requestedPath)
                {
                    case "mobile/playVideo":
                        {
                            await HandlePlayVideoRequest(context.Request, context.Response);
                            return;
                        }
                    case "mobile/queueVideo":
                        {
                            await HandleQueueVideoRequest(context.Request, context.Response);
                            return;
                        }
                    default:
                        {
                            break;
                        }
                }
            }
            await next();
            
        }

        private async Task HandleQueueVideoRequest(HttpListenerRequest request, HttpListenerResponse response)
        {
            response.StatusCode = 501;
            response.Close();
        }

        private async Task<BackendRequest> ExtractData(HttpListenerRequest request, HttpListenerResponse response)
        {

            using (var reader = new StreamReader(request.InputStream, request.ContentEncoding))
            {
                string requestBody = await reader.ReadToEndAsync();
                var jsonData = JsonConvert.DeserializeObject<BackendRequest>(requestBody);

                bool containsVideoId = jsonData.SourceUrl != null && !string.IsNullOrEmpty(jsonData.SourceUrl);

                if (containsVideoId)
                {
                    return jsonData;
                }
                else
                {
                    throw new Exception("Invalid BackgroundRequest data");
                }
            }
        }

        private async Task HandlePlayVideoRequest(HttpListenerRequest request, HttpListenerResponse response)
        {
            try
            {
                var jsonData = await ExtractData(request, response);
                bool containsVideoId = jsonData.SourceUrl != null && !string.IsNullOrEmpty(jsonData.SourceUrl);

                if (containsVideoId)
                {
                    string originSource = jsonData.SourceUrl;
                    await StreamingService.Prepare(originSource);

                    string videoId = new YTUrlData(originSource).VideoId;

                    await UISocketServer.Send(new
                    {
                        action = "playVideo",
                        data = new
                        {
                            originSource,
                            dashStreamUrl = BaseUrl + $"video/dash/{videoId}.mp4",
                            hlsStreamUrl = BaseUrl + $"video/hls/masterPlaylist/{videoId}.m3u8"
                        }
                    });
                    response.StatusCode = 200;
                    response.Close();
                }
                else
                {
                    response.StatusCode = 400;
                    response.Close();
                }

            }
            catch (Exception ex)
            {
                Logger.Error($"Error processing request", ex);
                response.StatusCode = 500;
                response.Close();
            }
        }
    }
}
