using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;

namespace YTMediaControllerSrv.Streaming
{
    internal class M3u8Parser
    {
        public static string RewritePlaylist(string localEndpoint, string playListManifest)
        {
            var lines = playListManifest.Split(new[] { '\n' }, StringSplitOptions.None);
            var result = new StringBuilder();

            foreach (var rawLine in lines)
            {
                var line = rawLine.TrimEnd('\r');

                if (line.StartsWith("#EXT-X-MEDIA:URI="))
                {
                    var uriStart = line.IndexOf("URI=\"", StringComparison.Ordinal);
                    var uriEnd = line.LastIndexOf('"');

                    if (uriStart >= 0 && uriEnd > uriStart + 5)
                    {
                        var originalUrl = line.Substring(uriStart + 5, uriEnd - (uriStart + 5));

                        var rewrittenUrl = $"{localEndpoint}video/hls/playlistFormat?url={WebUtility.UrlEncode(originalUrl)}";
                        result.AppendLine($"#EXT-X-MAP:URI=\"{rewrittenUrl}\"");
                        continue;
                    }
                }
                else if (line.StartsWith("https://", StringComparison.OrdinalIgnoreCase))
                {
                    var trimmedUrl = line.Trim();
                    var rewrittenUrl = $"{localEndpoint}video/hls/playlistFormat={WebUtility.UrlEncode(trimmedUrl)}";
                    result.AppendLine(rewrittenUrl);
                    continue;
                }

                result.AppendLine(line);
            }
            return result.ToString();
        }

        public static string RewriteSegment(string localEndpoint, string playlist)
        {
            var lines = playlist.Split(new[] { '\n' }, StringSplitOptions.None);
            var result = new StringBuilder();

            foreach (var rawLine in lines)
            {
                var line = rawLine.TrimEnd('\r');

                if (line.StartsWith("https://", StringComparison.OrdinalIgnoreCase))
                {
                    var trimmedUrl = line.Trim();
                    var rewrittenUrl = $"{localEndpoint}video/hls/segment={WebUtility.UrlEncode(trimmedUrl)}";
                    result.AppendLine(rewrittenUrl);
                    continue;
                }
                result.AppendLine(line);
            }
            return result.ToString();
        }
    }
}
