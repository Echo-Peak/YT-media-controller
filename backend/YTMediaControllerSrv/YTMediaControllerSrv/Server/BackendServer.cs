using Newtonsoft.Json;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Reflection;
using System.Runtime.Remoting.Contexts;
using System.Text;
using System.Threading.Tasks;
using YTMediaControllerSrv.Streaming;
using YTMediaControllerSrv.Types;
using static System.Net.WebRequestMethods;

namespace YTMediaControllerSrv.Server
{
    internal class BackendServer
    {
        public CreateHttpServer server { get; set; }
        private UISocketServer uiSockerServer { get; set; }
        private string endpoint = string.Empty;
        private StreamingService streamingService = new StreamingService();
        private HttpClient httpClient = new HttpClient(new HttpClientHandler
        {
            AutomaticDecompression = DecompressionMethods.Deflate
        });

        public BackendServer(string host, int port, UISocketServer uiSockerServer)
        {
            this.uiSockerServer = uiSockerServer;
            this.endpoint = $"http://{host}:{port}/";
        }

        public void Start()
        {
            var pipeline = new MiddlewarePipeline();

            pipeline.Use(CORSMiddleware);
            pipeline.Use(MobileRouterMiddleware);
            pipeline.Use(HlsRouterMiddleware);
            pipeline.Use(UnknownMiddleware);

            var middlewarePipeline = pipeline.Build();
            server = new CreateHttpServer(endpoint);
            server.OnRequest += async (ctx, req, rs) => await middlewarePipeline(ctx);
            Console.WriteLine($"Backend server started at {endpoint}");
        }

        public void Stop()
        {
            server.Stop();
        }

        public async Task Return404(HttpListenerResponse response)
        {
            response.StatusCode = 404;
            byte[] buffer = Encoding.UTF8.GetBytes("<html><body><h1>404 Not Found</h1></body></html>");
            response.ContentLength64 = buffer.Length;
            response.ContentType = "text/html";

            using (var output = response.OutputStream)
            {
                await output.WriteAsync(buffer, 0, buffer.Length);
            }
        }

        private async Task ReturnInvalidHttpMethod(HttpListenerResponse response)
        {
            response.StatusCode = 405;
            byte[] buffer = Encoding.UTF8.GetBytes("<html><body><h1>405 Method not supported</h1></body></html>");
            response.ContentLength64 = buffer.Length;
            response.ContentType = "text/html";

            using (var output = response.OutputStream)
            {
                await output.WriteAsync(buffer, 0, buffer.Length);
            }
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
                    await streamingService.CacheStream(originSource);
                    string videoId = new YTUrlData(originSource).VideoId;

                    await uiSockerServer.Send(new
                    {
                        action = "playVideo",
                        data = new
                        {
                            originSource,
                            dashStreamUrl = this.endpoint + $"video/dash/{videoId}.mp4",
                            hlsStreamUrl = this.endpoint + $"video/playlist/{videoId}.m3u8"
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
                await Console.Out.WriteLineAsync($"Error processing request: {ex.Message}");
                response.StatusCode = 500;
                response.Close();
            }
        }

        private async Task HandleQueueVideoRequest(HttpListenerRequest request, HttpListenerResponse response)
        {
           // await HandleRequest(request, response, playbackManager.QueueVideo);
        }

        private async Task HandleDASHStreamRequest(string videoId, HttpListenerResponse response)
        {
            try
            {
                if (streamingService.IsStreamSourceCached(videoId))
                {
                    Console.WriteLine($"Attempting to stream the video {videoId}");
                    response.StatusCode = 200;
                    response.ContentType = "video/mp4";
                    response.SendChunked = true;
                    await response.OutputStream.FlushAsync();

                    YTUrlSource source = streamingService.GetCachedSource(videoId);


                    using (var videoStream = await httpClient.GetStreamAsync(source.VideoSourceUrl))
                    using (var audioStream = await httpClient.GetStreamAsync(source.AudioSourceUrl))
                    {
                        await FFMpegCore.FFMpegArguments
                            .FromPipeInput(new FFMpegCore.Pipes.StreamPipeSource(videoStream))
                            .AddPipeInput(new FFMpegCore.Pipes.StreamPipeSource(audioStream))
                            .OutputToPipe(new FFMpegCore.Pipes.StreamPipeSink(response.OutputStream), o => o
                                .WithCopyCodec()
                                .WithCustomArgument("-map 0:v:0 -map 1:a:0")
                                .WithCustomArgument("-movflags +frag_keyframe+empty_moov")
                                .ForceFormat("mp4"))
                            .ProcessAsynchronously();
                    }

                    response.OutputStream.Close();

                    response.Close();
                }
                else
                {
                    response.StatusCode = 500;
                    response.Close();
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine("Unable to create DASH stream " + ex.Message);
                response.StatusCode = 502;
                response.Close();
            }
        }
        private string RewriteM3U8(string original)
        {
            var lines = original.Split(new[] { '\n' }, StringSplitOptions.None);
            var rewritten = new List<string>();

            foreach (var line in lines)
            {
                if (line.StartsWith("#") || string.IsNullOrWhiteSpace(line))
                {
                    rewritten.Add(line);
                }
                else
                {
                    var escaped = WebUtility.UrlEncode(line.Trim());
                    var newUrl = $"{this.endpoint}video/hls/segment?url={line.Trim()}";
                    rewritten.Add(newUrl);
                }
            }

            return string.Join("\n", rewritten);
        }
        private async Task HandleHlsPlaylistStreamRequest(string videoId, HttpListenerResponse response)
        {
            bool finishedOk = false;          // track whether we sent a normal reply

            try
            {
                if (!streamingService.IsStreamSourceCached(videoId))
                {
                    response.StatusCode = (int)HttpStatusCode.NotFound;
                    return;                   // skip the rest → finally will close
                }

                var src = streamingService.GetCachedSource(videoId);
                var manifest = await httpClient.GetStringAsync(src.ManifestSourceUrl);
                var rewritten = RewriteM3U8(manifest);
                var bytes = Encoding.UTF8.GetBytes(rewritten);

                response.StatusCode = (int)HttpStatusCode.OK;
                response.ContentType = "application/vnd.apple.mpegurl";
                response.ContentLength64 = bytes.Length;

                await response.OutputStream.WriteAsync(bytes, 0, bytes.Length);
                finishedOk = true;            // mark success
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[HLS] {ex}");
                if (!finishedOk && response.OutputStream.CanWrite)
                    response.StatusCode = (int)HttpStatusCode.BadGateway;   // 502
            }
            finally
            {
                // Close exactly once; suppress any second-close errors
                try { response.OutputStream.Close(); } catch { /* ignored */ }
                try { response.Close(); } catch { /* ignored */ }
            }
        }

        private async Task HandleHlsSegmentRequest(HttpListenerContext context)
        {
            var response = context.Response;
            var request = context.Request;

            try
            {
                var requestUrl = request.Url;
                var queryParams = System.Web.HttpUtility.ParseQueryString(requestUrl.Query);
                var segmentUrl = queryParams["url"];

                if (string.IsNullOrEmpty(segmentUrl))
                {
                    response.StatusCode = 400;
                    response.Close();
                    return;
                }

                var decodedSegmentUrl = WebUtility.UrlDecode(segmentUrl);
                Console.WriteLine("opening segment: {0}", decodedSegmentUrl);
                var segmentStream = await httpClient.GetStreamAsync(decodedSegmentUrl);

                response.StatusCode = 200;
                response.ContentType = "video/MP2T";
                response.SendChunked = true;

                using (var outputStream = response.OutputStream)
                {
                    await segmentStream.CopyToAsync(outputStream);
                }

                response.Close();
            }
            catch (Exception ex)
            {
                Console.WriteLine("Unable to create HLS stream " + ex.Message);
                response.StatusCode = 502;
                response.Close();
            }
        }

        private async Task MobileRouterMiddleware(HttpListenerContext context, Func<Task> next)
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
                            break;
                        }
                    case "mobile/queueVideo":
                        {
                            await HandleQueueVideoRequest(context.Request, context.Response);
                            break;
                        }
                    default:
                        {
                            await next();
                            break;
                        }
                }
            } else if (isMobile)
            {
                await ReturnInvalidHttpMethod(context.Response);
            }
            else
            {
                await next();
            }
        }

        private async Task HlsRouterMiddleware(HttpListenerContext context, Func<Task> next)
        {
            var path = context.Request.Url.AbsolutePath.TrimStart('/');
            if (path.StartsWith("video/dash") && path.EndsWith(".mp4"))
            {
                Console.WriteLine("video/dash");
                var streamParts = Path.GetFileNameWithoutExtension(path.Replace("video/dash", "")).Split('.');
                string videoId = streamParts[0];
                await HandleDASHStreamRequest(videoId, context.Response);
                return;
            }
            
            if (path.StartsWith("video/playlist") && path.EndsWith(".m3u8"))
            {
                var streamParts = Path.GetFileNameWithoutExtension(path.Replace("video/playlist", "")).Split('.');
                string videoId = streamParts[0];
                await HandleHlsPlaylistStreamRequest(videoId, context.Response);
                return;
            }

            if (path.StartsWith("video/hls/segment"))
            {

                await HandleHlsSegmentRequest(context);
                return;
            }
            await next();
        }

        private async Task CORSMiddleware(HttpListenerContext context, Func<Task> next)
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
                return;
            }

            await next();
        }

        private async Task UnknownMiddleware(HttpListenerContext context, Func<Task> next)
        {
            await Return404(context.Response);
        }
    }
}
