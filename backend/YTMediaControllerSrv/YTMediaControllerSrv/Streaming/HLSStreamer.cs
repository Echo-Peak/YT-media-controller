using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;

namespace YTMediaControllerSrv.Streaming
{
    public class HLSStreamer
    {
        private readonly ConcurrentDictionary<string, VideoStream> cache = new ConcurrentDictionary<string, VideoStream>();
        private string Endpoint;
        private readonly HttpClient http;
        public HLSStreamer(HttpClient http, string Endpoint)
        {
            this.http = http;
            this.Endpoint = Endpoint;
        }
        public bool IsCached(string streamKey)
        {
            return cache.ContainsKey(streamKey);
        }

        public void Load(string videoId, string masterPlaylist)
        {
            cache.TryAdd(videoId, new VideoStream(Endpoint, videoId, masterPlaylist));
        }

        public async Task<string> LoadPlaylistResource(string videoId, string resourceId)
        {
            try
            {
                cache.TryGetValue(videoId, out var videoStream);
                var playlistResourceUrl = videoStream.SelectPlaylistResourceUrl(resourceId);
                var playlist = await http.GetStringAsync(playlistResourceUrl);
                return videoStream.CreateVODPlaylist(playlist);
            }
            catch (Exception ex) { 
                Logger.Error($"Unable to load playlist resource. ResourceId: {resourceId}, videoId: {videoId}", ex);
                return null;
            }
        }

        public async Task<(System.IO.Stream stream, string contentType)> LoadSegmentResourceAsStream(string videoId, string resourceId)
        {
            try
            {
                cache.TryGetValue(videoId, out var videoStream);
                var segment = videoStream.SelectSegmentResource(resourceId);

                var stream = await http.GetStreamAsync(segment.Url);
                return (stream, segment.ContentType);
            }
            catch (Exception ex)
            {
                Logger.Error($"Unable to load playlist resource. ResourceId: {resourceId}, videoId: {videoId}", ex);
                return (null, null);
            }
        }

        public async Task<string> GenerateMasterPlayList(string videoId)
        {
            if (cache.TryGetValue(videoId, out VideoStream videoStream))
            {
                var source = videoStream.OriginMasterPlayListUrl;
                var playlist = await http.GetStringAsync(source);
                return videoStream.CreateMasterPlaylist(playlist);
            }
            else
            {
                throw new Exception("HLS stream not cached.");
            }
        }
    }
}
