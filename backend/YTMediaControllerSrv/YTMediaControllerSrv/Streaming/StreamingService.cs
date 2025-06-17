using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using YTMediaControllerSrv.Types;

namespace YTMediaControllerSrv.Streaming
{
    public class StreamingService
    {
        private YTDLP ytdlp = new YTDLP();
        private readonly ConcurrentDictionary<string, YTUrlSource> cachedLocalManifests = new ConcurrentDictionary<string, YTUrlSource>();

        public bool IsStreamSourceCached(string streamKey)
        {
            return cachedLocalManifests.ContainsKey(streamKey);
        }

        public YTUrlSource GetCachedSource(string streamKey)
        {
            if (cachedLocalManifests.TryGetValue(streamKey, out YTUrlSource source))
            {
                return source;
            }
            return null;
        }

        private void AddStreamToCache(string videoId, YTUrlSource source)
        {
               cachedLocalManifests.TryAdd(videoId, source);
        }

        async public Task CacheStream(string sourceUrl)
        {
            YTUrlData yTUrlData = new YTUrlData(sourceUrl);
            YTDlpJsonDump videoMetadata = await ytdlp.GetVideoMetadata(sourceUrl);
            YTUrlSource source = YTDlpParser.GetBestSource(videoMetadata);

            if(source == null)
            {
                throw new Exception("Failed to retrieve video manifest URL.");
            }


            AddStreamToCache(yTUrlData.VideoId, source);
        }
    }
}
