using System;
using System.Collections.Specialized;
using System.IO;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;
using YTMediaControllerSrv.Logging;
using YTMediaControllerSrv.YTDLP;

namespace YTMediaControllerSrv.Server.Middleware
{
    internal class DASHMiddleware : IHttpMiddleware
    {
        private HttpClient HttpClient { get; set; }
        private VideoCache Cache { get; set; }
        private readonly ILogger Logger;
        public DASHMiddleware(HttpClient httpClient, VideoCache cache, ILogger logger)
        {
            Logger = logger;
            HttpClient = httpClient;
            Cache = cache;
        }
        public async Task Invoke(HttpListenerContext context, Func<Task> next)
        {
            var path = context.Request.Url.AbsolutePath.TrimStart('/');
            bool isDash = path.StartsWith("video/dash");
            if (!isDash)
            {
                await next();
                return;
            }

            NameValueCollection queryParams = context.Request.QueryString;
            foreach (string key in queryParams.AllKeys)
            {
                string value = queryParams[key];
            }
            var videoId = queryParams["videoId"];
            if (string.IsNullOrEmpty(videoId))
            {
                await next();
                return;
            }

            await HandleDASHStreamRequest(videoId, context);

        }
        private async Task HandleDASHStreamRequest(string videoId, HttpListenerContext context)
        {
            var response = context.Response;

            try
            {
                var sourceJson = Cache.Get(videoId);
                if (sourceJson == null)
                {
                    response.StatusCode = 404;
                    response.Close();
                    return;
                }

                Logger.Info($"Attempting to stream the video {videoId}");
                response.StatusCode = 200;
                response.ContentType = "video/mp4";
                response.SendChunked = true;

                await response.OutputStream.FlushAsync();

                var bestSource = YTDlpParser.GetBestSource(sourceJson);
                var audioSource = bestSource.AudioSourceUrl;
                var videoSource = bestSource.VideoSourceUrl;
                bool missingSource = string.IsNullOrEmpty(videoSource) && string.IsNullOrEmpty(audioSource);
                if (missingSource)
                {
                    throw new Exception($"Missing a source url. Audio source: \"{audioSource}\", Video source: \"{videoSource}\"");
                }

                await FFMpegCore.FFMpegArguments
                    .FromUrlInput(new Uri(bestSource.VideoSourceUrl))
                    .AddUrlInput(new Uri(bestSource.AudioSourceUrl))
                    .OutputToPipe(new FFMpegCore.Pipes.StreamPipeSink(response.OutputStream), o => o
                        .WithAudioCodec("aac")
                        .WithVideoCodec("copy")
                        .WithCustomArgument("-map 0:v:0 -map 1:a:0")
                        .WithCustomArgument("-movflags +frag_keyframe+empty_moov")
                        .ForceFormat("mp4"))
                    .ProcessAsynchronously();
                
            }
            catch (IOException ioEx) when (ioEx.Message.Contains("pipe") || ioEx.Message.Contains("broken"))
            {
                Logger.Debug("Client disconnected during DASH stream.");
            }
            catch (Exception ex)
            {
                if (context.Response.OutputStream.CanWrite)
                {
                    context.Response.StatusCode = 502;
                }
            }
            finally
            {
                try
                {
                    if (response.OutputStream.CanWrite)
                    {
                        response.OutputStream.Close();
                    }
                }
                catch(Exception streamError) {
                    Logger.Error("Unable to close output stream", streamError);
                }

                try { response.Close(); } catch(Exception resError) {
                    Logger.Error("Unable to close response", resError);
                }
            }
        }
    }
}
