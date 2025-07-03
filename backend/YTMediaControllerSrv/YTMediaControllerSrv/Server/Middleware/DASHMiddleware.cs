using System;
using System.Collections.Specialized;
using System.IO;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;
using YTMediaControllerSrv.YTDLP;

namespace YTMediaControllerSrv.Server.Middleware
{
    internal class DASHMiddleware : IHttpMiddleware
    {
        private HttpClient HttpClient { get; set; }
        private VideoCache Cache { get; set; }
        public DASHMiddleware(HttpClient httpClient, VideoCache cache)
        {
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

                using (var videoStream = await HttpClient.GetStreamAsync(bestSource.VideoSourceUrl))
                using (var audioStream = await HttpClient.GetStreamAsync(bestSource.AudioSourceUrl))
                {
                    await FFMpegCore.FFMpegArguments
                        .FromPipeInput(new FFMpegCore.Pipes.StreamPipeSource(videoStream))
                        .AddPipeInput(new FFMpegCore.Pipes.StreamPipeSource(audioStream))
                        .OutputToPipe(new FFMpegCore.Pipes.StreamPipeSink(response.OutputStream), o => o
                            .WithCopyCodec()
                            .WithCustomArgument("-map 0:v:0 -map 1:a:0")
                            .WithCustomArgument("-movflags +frag_keyframe+empty_moov")
                            .WithCustomArgument("-preset ultrafast")
                            .WithCustomArgument("-fflags +nobuffer")
                            .ForceFormat("mp4"))
                        .ProcessAsynchronously();
                }
            }
            catch (IOException ioEx) when (ioEx.Message.Contains("pipe") || ioEx.Message.Contains("broken"))
            {
                Logger.Debug("Client disconnected during DASH stream.");
            }
            catch (Exception ex)
            {
                Logger.Error("Unable to create DASH stream", ex);
                if (context.Response.OutputStream.CanWrite)
                    context.Response.StatusCode = 502;
            }
            finally
            {
                try { response.OutputStream.Close(); } catch { }
                try { response.Close(); } catch { }
            }
        }
    }
}
