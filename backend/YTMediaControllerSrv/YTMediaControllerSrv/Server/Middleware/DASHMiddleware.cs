using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Runtime.Remoting.Contexts;
using System.Text;
using System.Threading.Tasks;
using YTMediaControllerSrv.Streaming;

namespace YTMediaControllerSrv.Server.Middleware
{
    internal class DASHMiddleware : IHttpMiddleware
    {
        private StreamingService StreamingService { get; set; }
        private HttpClient HttpClient { get; set; }
        public DASHMiddleware(StreamingService streamingService, HttpClient httpClient)
        {
            StreamingService = streamingService;
            HttpClient = httpClient;
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
            if (path.StartsWith("video/dash") && path.EndsWith(".mp4"))
            {
                var streamParts = Path.GetFileNameWithoutExtension(path.Replace("video/dash", "")).Split('.');
                string videoId = streamParts[0];
                await HandleDASHStreamRequest(videoId, context);
                return;
            }
        }
        private async Task HandleDASHStreamRequest(string videoId, HttpListenerContext context)
        {
            var response = context.Response;
            var streamer = (DASHStreamer)StreamingService.GetStreamer(StreamType.DASH);

            try
            {

                if (!streamer.IsCached(videoId))
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
                YTUrlSource source = streamer.GetCachedSource(videoId);


                using (var videoStream = await HttpClient.GetStreamAsync(source.VideoSourceUrl))
                using (var audioStream = await HttpClient.GetStreamAsync(source.AudioSourceUrl))
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
            catch (Exception ex)
            {
                Logger.Error("Unable to create DASH stream", ex);
                response.StatusCode = 502;
                response.Close();
            }
        }
    }
}
