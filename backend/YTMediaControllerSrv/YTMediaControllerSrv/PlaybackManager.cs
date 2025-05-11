using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using YTMediaControllerSrv.Server;

namespace YTMediaControllerSrv
{
    internal class PlaybackManager
    {
        private List<string> videoQueue = new List<string>();
        private ControlServer controlServer;
        public PlaybackManager(ControlServer controlServer)
        {
            this.controlServer = controlServer;
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

        public void PlayVideo(string sourceUrl)
        {
            string videoId = ExtractVideoId(sourceUrl);
            bool inQueue = videoQueue.Contains(videoId);
            if (inQueue) return;
            SendPlayEvent(videoId);
        }

        private void SendPlayEvent(string videoId)
        {
            controlServer.Send(new
            {
                action = "playbackStarted",
                videoId,
            });
        }

        public void QueueVideo(string videoId)
        {
            videoQueue.Add(videoId);

        }

        public void OnVideoEnd(string videoId)
        {
            videoQueue.Remove(videoId);
            if (videoQueue.Count > 0)
            {
                string nextVideoId = videoQueue[0];
                SendPlayEvent(nextVideoId);
            }
        }
    }
}
