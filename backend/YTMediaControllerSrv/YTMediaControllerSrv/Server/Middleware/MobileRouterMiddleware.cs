using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;
using YTMediaControllerSrv.Types;
using YTMediaControllerSrv.YTDLP;

namespace YTMediaControllerSrv.Server.Middleware
{
    internal class MobileRouterMiddleware : IHttpMiddleware
    {
        private UISocketServer UISocketServer { get; set; }
        private string BaseUrl { get; set; }

        private YtdlpExec Ytdlp { get; set; }

        public MobileRouterMiddleware(UISocketServer uiSocketServer, YtdlpExec ytdlp, string baseUrl)
        {
            UISocketServer = uiSocketServer;
            BaseUrl = baseUrl;
            Ytdlp = ytdlp;
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
                    var sourceJson = await Ytdlp.Fetch(originSource);
                    YTUrlSource localSource = YTDlpParser.GetBestSource(sourceJson);

                    string videoId = new YTUrlData(originSource).VideoId;
                    bool isDashAvailable = !string.IsNullOrEmpty(localSource.VideoSourceUrl) && !string.IsNullOrEmpty(localSource.AudioSourceUrl);

                    await UISocketServer.Send(new
                    {
                        action = "playVideo",
                        data = new
                        {
                            originSource,
                            dashStreamUrl = isDashAvailable ? BaseUrl + $"video/dash?videoId={videoId}" : null,
                            hlsStreamUrl = BaseUrl + $"video/hls/playlist?videoId={videoId}&url={Convert.ToBase64String(Encoding.UTF8.GetBytes(localSource.MasterPlaylistUrl))}",
                            videoData = new {
                                title = sourceJson.Title,
                                uploader = sourceJson.Uploader
                            }
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
