using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using YTMediaControllerSrv.Types;

namespace YTMediaControllerSrv.YTDLP
{
    public class VideoCache
    {
        private Dictionary<string, YTDlpJsonDump> cache = new Dictionary<string, YTDlpJsonDump>();

        public YTDlpJsonDump Get(string key)
        {
            if (cache.ContainsKey(key))
            {
                return cache[key];
            }
            return null;
        }

        public void Add(string videoId, YTDlpJsonDump jsonDump)
        {
            cache[videoId] = jsonDump;
        }
    }
}
