using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace YTMediaControllerSrv.YTDLP
{
    public class YTUrlSource
    {
        public string VideoSourceUrl { get; set; }
        public string AudioSourceUrl { get; set; }
        public string MasterPlaylistUrl { get; set; }

        public YTUrlSource(string videoSource, string audioSource, string masterPlayListSource)
        {
            VideoSourceUrl = videoSource;
            AudioSourceUrl = audioSource;
            MasterPlaylistUrl = masterPlayListSource;
        }
    }
}
