using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;
using YTMediaControllerSrv.Streaming;

namespace YTMediaControllerSrv.Server.Middleware
{
    internal class HLSMiddleware : IHttpMiddleware
    {
        private StreamingService StreamingService { get; set; }

        public HLSMiddleware(StreamingService streamingService)
        {
            StreamingService = streamingService;
        }
        public async Task Invoke(HttpListenerContext context, Func<Task> next)
        {
            var path = context.Request.Url.AbsolutePath.TrimStart('/');
            bool isHLS = path.StartsWith("video/hls");
            if(!isHLS)
            {
                await next();
                return;
            }

            string endpoint = path.Split('/')[2];
            switch (endpoint)
            {
                case "masterPlaylist":
                    {
                        var streamParts = Path.GetFileNameWithoutExtension(path.Replace("video/playlist", "")).Split('.');
                        string videoId = streamParts[0];
                        await HandleHlsMasterPlaylistRequest(videoId, context.Response);
                        return;
                    }
                case "playlist":
                    {
                        await HandleHlsPlaylistStreamRequest(context.Request, context.Response);
                        return;
                    }
                case "segment":
                    {
                        await HandleHlsSegmentRequest(context);
                        return;
                    }
                default:
                    {
                        break;
                    }
            }
            await next();
        }
        private async Task HandleHlsMasterPlaylistRequest(string videoId, HttpListenerResponse response)
        {
            bool finishedOk = false;
            var streamer = (HLSStreamer)StreamingService.GetStreamer(StreamType.HLS);
            try
            {
                if (!streamer.IsCached(videoId))
                {
                    response.StatusCode = (int)HttpStatusCode.NotFound;
                    response.Close();
                    return;
                }


                var localMasterPlaylist = await StreamingService.Use(StreamType.HLS, videoId);
                Logger.Info($"Generated local Master playlist url: {localMasterPlaylist}");

                var bytes = Encoding.UTF8.GetBytes(localMasterPlaylist);

                response.StatusCode = (int)HttpStatusCode.OK;
                response.ContentType = "application/vnd.apple.mpegurl";
                response.ContentLength64 = bytes.Length;

                await response.OutputStream.WriteAsync(bytes, 0, bytes.Length);
                finishedOk = true;
            }
            catch (Exception ex)
            {
                if (!finishedOk && response.OutputStream.CanWrite)
                    response.StatusCode = (int)HttpStatusCode.BadGateway;
            }
            finally
            {
                try { response.OutputStream.Close(); } catch { }
                try { response.Close(); } catch { }
            }
        }

        private async Task HandleHlsPlaylistStreamRequest(HttpListenerRequest request, HttpListenerResponse response)
        {
            var streamer = (HLSStreamer)StreamingService.GetStreamer(StreamType.HLS);
            bool finishedOk = false;
            try
            {
                NameValueCollection queryParams = request.QueryString;
                foreach (string key in queryParams.AllKeys)
                {
                    string value = queryParams[key];
                }
                var resourceId = queryParams["resourceId"];
                var videoId = queryParams["videoId"];

                var playListData = await streamer.LoadPlaylistResource(videoId, resourceId);

                var bytes = Encoding.UTF8.GetBytes(playListData);

                response.StatusCode = (int)HttpStatusCode.OK;
                response.ContentType = "application/vnd.apple.mpegurl";
                response.ContentLength64 = bytes.Length;

                await response.OutputStream.WriteAsync(bytes, 0, bytes.Length);
                finishedOk = true;
            }
            catch (Exception ex)
            {
                if (!finishedOk && response.OutputStream.CanWrite)
                    response.StatusCode = (int)HttpStatusCode.BadGateway;
            }
            finally
            {
                try { response.OutputStream.Close(); } catch { }
                try { response.Close(); } catch { }
            }
        }

        private async Task HandleHlsSegmentRequest(HttpListenerContext context)
        {
            var response = context.Response;
            var request = context.Request;
            var streamer = (HLSStreamer)StreamingService.GetStreamer(StreamType.HLS);

            try
            {
                var requestUrl = request.Url;
                var queryParams = System.Web.HttpUtility.ParseQueryString(requestUrl.Query);
                var resourceId = queryParams["resourceId"];
                var videoId = queryParams["videoId"];

                if (string.IsNullOrEmpty(resourceId))
                {
                    response.StatusCode = 400;
                    response.Close();
                    return;
                }

                Logger.Debug($"Attempting to load resource: {resourceId}");
                var (segmentStream, segmentContentType) = await streamer.LoadSegmentResourceAsStream(videoId, resourceId);

                response.StatusCode = 200;
                response.ContentType = segmentContentType;
                response.SendChunked = true;

                using (var outputStream = response.OutputStream)
                {
                    await segmentStream.CopyToAsync(outputStream);
                }

                response.Close();
            }
            catch (Exception ex)
            {
                Logger.Error("Unable to create HLS stream", ex);
                response.StatusCode = 502;
                response.Close();
            }
        }
    }
}
