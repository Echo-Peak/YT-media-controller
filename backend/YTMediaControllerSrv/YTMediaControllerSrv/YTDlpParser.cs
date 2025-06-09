using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using YTMediaControllerSrv.Types;

namespace YTMediaControllerSrv
{
    internal class YTDlpParser
    {
        public static string GetBestVideoManifest(YTDlpJsonDump dump)
        {
            YtDlpFormat bestVideoOnly = null;

            foreach (var format in dump.Formats)
            {
                if (string.IsNullOrEmpty(format.Protocol) || !format.Protocol.Contains("m3u8"))
                    continue;

                bool hasVideo = format.Vcodec != "none";
                bool hasAudio = format.Acodec != "none";

                if (hasVideo && !hasAudio)
                {
                    if (bestVideoOnly == null || (format.Height ?? 0) > (bestVideoOnly.Height ?? 0))
                    {
                        bestVideoOnly = format;
                    }
                }
            }

            return bestVideoOnly?.ManifestUrl ?? bestVideoOnly?.Url;
        }
    }
}
