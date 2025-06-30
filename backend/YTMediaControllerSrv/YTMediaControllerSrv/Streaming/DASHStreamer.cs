using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using YTMediaControllerSrv.Types;

namespace YTMediaControllerSrv.Streaming
{
    public class DASHStreamer
    {
        private readonly ConcurrentDictionary<string, YTUrlSource> cache = new ConcurrentDictionary<string, YTUrlSource>();
        public bool IsCached(string streamKey)
        {
            return cache.ContainsKey(streamKey);
        }

        public YTUrlSource GetCachedSource(string streamKey)
        {
            if (cache.TryGetValue(streamKey, out YTUrlSource source))
            {
                return source;
            }
            return null;
        }

        private void AddStreamToCache(string videoId, YTUrlSource source)
        {
            cache.TryAdd(videoId, source);
        }

        public void Load(string videoId, YTUrlSource source)
        {
            AddStreamToCache(videoId, source);
        }
    }
}
