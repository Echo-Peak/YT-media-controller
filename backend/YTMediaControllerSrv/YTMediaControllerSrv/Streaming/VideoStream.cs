using M3U8Parser;
using M3U8Parser.Attributes.ValueType;
using M3U8Parser.Tags.MultivariantPlaylist;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Policy;
using System.Text;
using System.Threading.Tasks;

namespace YTMediaControllerSrv.Streaming
{
    public class VideoStream
    {
        public class SegmentResource
        {
            public string Url { get; set; }
            public string ContentType { get; set; }

            public SegmentResource(string url, string contentType)
            {
                Url = url.Replace("\"", "").Replace("\\", "").Trim();
                ContentType = contentType;
            }
        }

        private string Endpoint { get; set; }
        public string VideoId { get; set; }
        public string OriginMasterPlayListUrl;
        public Dictionary<string, string> playlistResources = new Dictionary<string, string>();
        public Dictionary<string, SegmentResource> segmentResources = new Dictionary<string, SegmentResource>();

        public VideoStream(string endpoint, string videoID, string originMasterPlayListUrl)
        {
            VideoId = videoID;
            Endpoint = endpoint;
            OriginMasterPlayListUrl = originMasterPlayListUrl;
        }

        public string CreateMasterPlaylist(string originMasterPlaylistText)
        {
            var master = MasterPlaylist.LoadFromText(originMasterPlaylistText);
            return CreateLocalMasterPlaylist(master);
        }


        public string CreateVODPlaylist(string vodPlaylist)
        {
            var lines = vodPlaylist.Split(new[] { '\n' }, StringSplitOptions.None);
            var playlist = new StringBuilder();

            foreach (var rawLine in lines)
            {
                var line = rawLine.TrimEnd('\r');
                if (line.StartsWith("#EXT-X-MAP:URI"))
                {
                    string[] mapParts = line.Split('=');
                    string url = mapParts[1].Trim();
                    var id = Guid.NewGuid().ToString();
                    var rewrittenUrl = $"#EXT-X-MAP:URI=\"{Endpoint}video/hls/segment?resourceId={id}&videoId={VideoId}\"";
                    playlist.AppendLine(rewrittenUrl);
                    segmentResources.Add(id, new SegmentResource(url, "application/octet-stream"));
                    continue;
                }
                if (line.StartsWith("https://", StringComparison.OrdinalIgnoreCase))
                {
                    var trimmedUrl = line.Trim();
                    var id = Guid.NewGuid().ToString();
                    var rewrittenUrl = $"{Endpoint}video/hls/segment?resourceId={id}&videoId={VideoId}";
                    playlist.AppendLine(rewrittenUrl);
                    segmentResources.Add(id, new SegmentResource(trimmedUrl, "video/MP2T"));
                    continue;
                }
                playlist.AppendLine(line);
            }

            return playlist.ToString();
        }

        public string SelectPlaylistResourceUrl(string resourceId)
        {
            if (playlistResources.TryGetValue(resourceId, out var originPlaylistResourceUrl))
            {
                return originPlaylistResourceUrl;
            }
            else
            {
                throw new KeyNotFoundException($"Playlist Resource ID '{resourceId}' not found in linked streams.");
            }
        }

        public SegmentResource SelectSegmentResource(string resourceId)
        {
            if (segmentResources.TryGetValue(resourceId, out var originSegmentResourceUrl))
            {
                return originSegmentResourceUrl;
            }
            else
            {
                throw new KeyNotFoundException($"segment Resource ID '{resourceId}' not found in linked streams.");
            }
        }

        private string CreateLocalMasterPlaylist(MasterPlaylist orignPlaylist)
        {
            var masterPlaylist = new MasterPlaylist(hlsVersion: 6);

            foreach (var media in orignPlaylist.Medias)
            {
                if (media.Type.ToString() == "VIDEO" || media.Type.ToString() == "AUDIO")
                {
                    var id = Guid.NewGuid().ToString();
                    var rewrittenUrl = $"{Endpoint}video/hls/playlist?resourceId={id}&videoId={VideoId}";

                    masterPlaylist.Medias.Add(new Media
                    {
                        Type = media.Type,
                        GroupId = media.GroupId,
                        Name = media.Name,
                        Language = media.Language,
                        Default = media.Default,
                        Uri = rewrittenUrl,
                        AutoSelect = media.AutoSelect,
                        
                    });
                    playlistResources.Add(id, media.Uri);
                }
            }

            foreach (var stream in orignPlaylist.Streams)
            {
                var id = Guid.NewGuid().ToString();
                var rewrittenUrl = $"{Endpoint}video/hls/playlist?resourceId={id}&videoId={VideoId}";
                masterPlaylist.Streams.Add(new StreamInf
                {
                    Uri = rewrittenUrl,
                    Bandwidth = stream.Bandwidth,
                    Codecs = stream.Codecs,
                    Resolution = stream.Resolution,
                    FrameRate = stream.FrameRate,
                    VideoRange = stream.VideoRange,
                    ClosedCaptions = stream.ClosedCaptions,
                });
                playlistResources.Add(id, stream.Uri);
            }

            string generatedPlaylist = masterPlaylist.ToString();
            return CleanAndAddIndependentSegments(generatedPlaylist);
        }

        private string CleanAndAddIndependentSegments(string input)
        {
            var lines = input
                .Split(new[] { "\r\n", "\n" }, StringSplitOptions.None)
                .Where(line => !string.IsNullOrWhiteSpace(line))
                .ToList();

            int extm3uIndex = lines.FindIndex(line => line.Trim() == "#EXTM3U");
            if (extm3uIndex != -1)
            {
                lines.Insert(extm3uIndex + 1, "#EXT-X-INDEPENDENT-SEGMENTS");
            }
            else
            {
                lines.Insert(0, "#EXT-X-INDEPENDENT-SEGMENTS");
                lines.Insert(0, "#EXTM3U");
            }

            return string.Join("\n", lines);
        }
    }
}
