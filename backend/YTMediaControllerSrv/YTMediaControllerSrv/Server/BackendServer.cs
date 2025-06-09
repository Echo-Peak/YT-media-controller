using Newtonsoft.Json;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using YTMediaControllerSrv.Types;

namespace YTMediaControllerSrv.Server
{
    internal class BackendServer
    {
        public CreateHttpServer server { get; set; }
        private PlaybackManager playbackManager { get; set; }
        private string endpoint = string.Empty;

        public BackendServer(string host, int port, PlaybackManager playbackManager)
        {
            this.playbackManager = playbackManager;
            this.endpoint = $"http://{host}:{port}/";
        }

        public void Start()
        {
            server = new CreateHttpServer(endpoint);
            server.OnRequest += HandleRequest;
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

        private async Task HandleRequest(HttpListenerRequest request, HttpListenerResponse response, Action<string> videoAction)
        {
            if (request.HttpMethod != "POST")
            {
                await ReturnInvalidHttpMethod(response);
                return;
            }

            try
            {
                var jsonData = await ExtractData(request, response);
                bool containsVideoId = jsonData.SourceUrl != null && !string.IsNullOrEmpty(jsonData.SourceUrl);

                    if (containsVideoId)
                    {
                        videoAction(jsonData.SourceUrl);
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

        private async Task HandlePlayVideoRequest(HttpListenerRequest request, HttpListenerResponse response)
        {
            await HandleRequest(request, response, playbackManager.PlayVideo);
        }

        private async Task HandleQueueVideoRequest(HttpListenerRequest request, HttpListenerResponse response)
        {
            await HandleRequest(request, response, playbackManager.QueueVideo);
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
                    rewritten.Add("/segment?url=" + escaped);
                }
            }

            return string.Join("\n", rewritten);
        }

        private async Task HandleVideoManifestRequest(string videoId, HttpListenerResponse response)
        {
            try
            {
                if (playbackManager.IsManifestCached(videoId))
                {
                    response.StatusCode = 200;
                    byte[] buffer = Encoding.UTF8.GetBytes(playbackManager.GetVideoManifest(videoId));
                    response.ContentType = "application/vnd.apple.mpegurl";
                    response.ContentLength64 = buffer.Length;
                    await response.OutputStream.WriteAsync(buffer, 0, buffer.Length);
                    response.Close();
                    return;
                }else
                {
                    Console.WriteLine("m3u8 file could not be cached");
                    response.StatusCode = 404;
                    await Return404(response);
                    return;
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine("Error rewriting m3u8: " + ex.Message);
                response.StatusCode = 502;
                response.Close();
            }
        }

        public async void HandleRequest(HttpListenerContext context, HttpListenerRequest request, HttpListenerResponse response)
        {
            response.Headers.Add("Access-Control-Allow-Origin", "*");
            response.Headers.Add("Access-Control-Allow-Methods", "GET, POST, OPTIONS");
            response.Headers.Add("Access-Control-Allow-Headers", "Content-Type, Authorization");

            if (request.HttpMethod == "OPTIONS")
            {
                response.StatusCode = 200;
                response.Close();
                return;
            }

            string requestedPath = request.Url.AbsolutePath.TrimStart('/');
            await Console.Out.WriteLineAsync($"Requesting path: {requestedPath}");

            if (requestedPath.StartsWith("video/") && requestedPath.EndsWith(".m3u8"))
            {
                string videoId = Path.GetFileNameWithoutExtension(requestedPath.Replace("video/", ""));
                await HandleVideoManifestRequest(videoId, response);
                return;
            }

            switch (requestedPath)
            {
                case "mobile/playVideo":
                    {
                        await HandlePlayVideoRequest(request, response);
                        break;
                    }
                case "mobile/queueVideo":
                    {
                        await HandleQueueVideoRequest(request, response);
                        break;
                    }
                default:
                    {
                        await Return404(response);
                        break;
                    }
            }
        }
    }
}
