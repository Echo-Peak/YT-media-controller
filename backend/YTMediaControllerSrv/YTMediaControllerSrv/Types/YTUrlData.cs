using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace YTMediaControllerSrv.Types
{

    internal class YTUrlData
    {
        public enum SourceType {
            DEFAULT,
            LIVE,
            SHORT
        }
        public string VideoId { get; set; }
        public string SourceUrl { get; set; } = string.Empty;
        public SourceType sourceType { get; set; } = SourceType.DEFAULT;

        public YTUrlData(string sourceUrl)
        {
            SourceUrl = sourceUrl;
            VideoId = ExtractVideoId(sourceUrl);
            if (sourceUrl.Contains("shorts/"))
            {
                sourceType = SourceType.SHORT;
            }
            else if (sourceUrl.Contains("live/"))
            {
                sourceType = SourceType.LIVE;
            }
        }
        private string ExtractVideoId(string url)
        {
            string videoId = "";
            if (url.Contains("v="))
            {
                int startIndex = url.IndexOf("v=") + 2;
                int endIndex = url.IndexOf("&", startIndex);
                if (endIndex == -1)
                {
                    endIndex = url.Length;
                }
                videoId = url.Substring(startIndex, endIndex - startIndex);
            }
            return videoId;
        }
    }
}
