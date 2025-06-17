using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace YTMediaControllerSrv.Streaming
{
    public class YTUrlSource
    {
        public string VideoSourceUrl { get; set; }
        public string AudioSourceUrl { get; set; }

        public string ManifestSourceUrl { get; set; }

        public YTUrlSource(string videoSource, string audioSource, string manifestSourceUrl)
        {
            VideoSourceUrl = videoSource;
            AudioSourceUrl = audioSource;
            ManifestSourceUrl = manifestSourceUrl;
        }
    }
}
