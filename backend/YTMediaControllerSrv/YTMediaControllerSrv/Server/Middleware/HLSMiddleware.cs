using FFMpegCore;
using System;
using System.Buffers.Text;
using System.Collections.Specialized;
using System.IO;
using System.Net;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;

namespace YTMediaControllerSrv.Server.Middleware
{
    internal class HLSMiddleware : IHttpMiddleware
    {
        private string Endpoint { get; set; }
        private HttpClient Http { get; set; }
        public HLSMiddleware(HttpClient http, string endpoint)
        {
            Endpoint = endpoint;
            Http = http;
        }
        public async Task Invoke(HttpListenerContext context, Func<Task> next)
        {
            var path = context.Request.Url.AbsolutePath.TrimStart('/');
            bool isHLS = path.StartsWith("video/hls");
            if (!isHLS)
            {
                await next();
                return;
            }

            string endpoint = path.Split('/')[2];
            switch (endpoint)
            {
                case "playlist":
                    {
                        NameValueCollection queryParams = context.Request.QueryString;
                        foreach (string key in queryParams.AllKeys)
                        {
                            string value = queryParams[key];
                        }
                        var originUrl = DecodeUrl(queryParams["url"]);
                        var videoId = queryParams["videoId"];

                        await HandlePlaylistRequest(videoId, originUrl, context.Response);
                        return;
                    }
                case "segment":
                    {
                        NameValueCollection queryParams = context.Request.QueryString;
                        foreach (string key in queryParams.AllKeys)
                        {
                            string value = queryParams[key];
                        }
                        var segmentUrl = DecodeUrl(queryParams["url"]);
                        await HandleSegmentRequest(segmentUrl, context);
                        return;
                    }
                default:
                    {
                        break;
                    }
            }
            await next();
        }

        private string EncodeUrl(string originUrl)
        {
            return Convert.ToBase64String(Encoding.UTF8.GetBytes(originUrl));
        }

        private string DecodeUrl(string originBase64Url)
        {
            return Encoding.UTF8.GetString(Convert.FromBase64String(originBase64Url));
        }

        private async Task HandlePlaylistRequest(string videoId, string masterPlaylistUrl, HttpListenerResponse response)
        {
            bool finishedOk = false;
            try
            {


                var originResponse = await Http.SendAsync(new HttpRequestMessage(HttpMethod.Get, masterPlaylistUrl));
                var content = await originResponse.Content.ReadAsStringAsync();


                var rewritten = RewriteM3U8(content, videoId);
                var bytes = Encoding.UTF8.GetBytes(rewritten);

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

        private HttpRequestMessage CreateSegmentRequest(string externalSegmentUrl)
        {
            var request = new HttpRequestMessage(HttpMethod.Get, externalSegmentUrl);
            request.Headers.UserAgent.ParseAdd("Mozilla/5.0 (Windows NT 10.0; WOW64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/138.0.0.0 Safari/537.36");
            return request;
        }

        private async Task HandleSegmentRequest(string segmentUrl, HttpListenerContext context)
        {
            var response = context.Response;

            if (string.IsNullOrWhiteSpace(segmentUrl))
            {
                response.StatusCode = 400;
                response.Close();
                return;
            }

            try
            {
                var request = CreateSegmentRequest(segmentUrl);

                using (var segmentResponse = await Http.SendAsync(request, HttpCompletionOption.ResponseHeadersRead))
                {
                    if(segmentResponse.StatusCode != HttpStatusCode.OK)
                    {
                        response.StatusCode = (int)segmentResponse.StatusCode;
                        response.Close();
                        return;
                    }
                    var contentType = segmentResponse.Content.Headers.ContentType?.MediaType ?? "application/octet-stream";
                    response.ContentType = contentType;

                    using (var upstream = await segmentResponse.Content.ReadAsStreamAsync())
                    {
                        byte[] probeBuffer = new byte[10];
                        int probeRead = await upstream.ReadAsync(probeBuffer, 0, probeBuffer.Length);
                        string probeText = Encoding.UTF8.GetString(probeBuffer, 0, probeRead);
                        bool isTextM3U8 = probeText.StartsWith("#EXTM3U");

                        if (isTextM3U8)
                        {
                            using (var reader = new StreamReader(upstream, Encoding.UTF8, true, 1024, leaveOpen: true))
                            {
                                string restOfText = await reader.ReadToEndAsync();
                                string fullM3U8 = probeText + restOfText;

                                string rewritten = RewriteM3U8(fullM3U8, null);
                                byte[] data = Encoding.UTF8.GetBytes(rewritten);

                                response.ContentType = "application/vnd.apple.mpegurl";
                                response.ContentLength64 = data.Length;
                                await response.OutputStream.WriteAsync(data, 0, data.Length);
                            }
                        }
                        else
                        {
                            response.ContentType = segmentUrl.Contains(".m4s") || segmentUrl.Contains(".mp4")
                                ? "video/mp4"
                                : "video/MP2T";

                            await response.OutputStream.WriteAsync(probeBuffer, 0, probeRead);
                            await upstream.CopyToAsync(response.OutputStream, 128 * 1024);
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                Logger.Error("Unable to fetch segment", ex);
                response.StatusCode = 502;
            }
            finally
            {
                try { response.OutputStream.Close(); } catch { }
                try { response.Close(); } catch { }
            }
        }

        private string RewriteM3U8(string original, string videoId)
        {
            var lines = original.Split(new[] { '\n' }, StringSplitOptions.None);
            var rewritten = new StringBuilder();

            foreach (var line in lines)
            {
                var trimmed = line.TrimEnd();

                if (trimmed.StartsWith("#EXT-X-MEDIA:") && trimmed.Contains("URI=\""))
                {
                    int uriStart = trimmed.IndexOf("URI=\"", StringComparison.Ordinal);
                    int uriEnd = trimmed.IndexOf("\"", uriStart + 5);

                    if (uriStart >= 0 && uriEnd > uriStart)
                    {
                        string originalUri = trimmed.Substring(uriStart + 5, uriEnd - uriStart - 5);
                        string encoded = Convert.ToBase64String(Encoding.UTF8.GetBytes(originalUri));
                        string localUri = $"{Endpoint}video/hls/segment?url={encoded}";
                        string newLine = trimmed.Substring(0, uriStart + 5) + localUri + trimmed.Substring(uriEnd);
                        rewritten.AppendLine(newLine);
                        continue;
                    }
                }
                else if (trimmed.StartsWith("#EXT-X-MAP:") && trimmed.Contains("URI=\""))
                {
                    int uriStart = trimmed.IndexOf("URI=\"", StringComparison.Ordinal);
                    int uriEnd = trimmed.IndexOf("\"", uriStart + 5);

                    if (uriStart >= 0 && uriEnd > uriStart)
                    {
                        string originalUri = trimmed.Substring(uriStart + 5, uriEnd - uriStart - 5);
                        string encoded = Convert.ToBase64String(Encoding.UTF8.GetBytes(originalUri));
                        string localUri = $"{Endpoint}video/hls/segment?url={encoded}";
                        string newLine = trimmed.Substring(0, uriStart + 5) + localUri + trimmed.Substring(uriEnd);
                        rewritten.AppendLine(newLine);
                        continue;
                    }
                }
                else if (!trimmed.StartsWith("#") && trimmed.StartsWith("http"))
                {
                    string encoded = Convert.ToBase64String(Encoding.UTF8.GetBytes(trimmed));
                    rewritten.AppendLine($"{Endpoint}video/hls/segment?url={encoded}");
                }
                else
                {
                    // Keep as-is, just trim trailing whitespace to preserve `#`
                    rewritten.AppendLine(trimmed);
                }
            }

            return rewritten.ToString();
        }
    }
}
