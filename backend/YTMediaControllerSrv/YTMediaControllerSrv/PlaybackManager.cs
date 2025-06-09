using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using YTMediaControllerSrv.Server;
using YTMediaControllerSrv.Types;

namespace YTMediaControllerSrv
{
    internal class PlaybackManager
    {
        private List<string> videoQueue = new List<string>();
        private ControlServer controlServer;
        private YTDLP ytdlp = new YTDLP();
        private readonly ConcurrentDictionary<string, string> videoIdToManifestUrl = new ConcurrentDictionary<string, string>();

        public PlaybackManager(ControlServer controlServer)
        {
            this.controlServer = controlServer;
        }

        public void PlayVideo(string sourceUrl)
        {
            YTUrlData yTUrlData = new YTUrlData(sourceUrl);
            YTDlpJsonDump videoMetadata = ytdlp.GetVideoMetadata(sourceUrl);
            string sourceVideoManifest = YTDlpParser.GetBestVideoManifest(videoMetadata);


            bool inQueue = videoQueue.Contains(yTUrlData.VideoId);
            if (inQueue) return;
            SendPlayEvent(yTUrlData);
        }

        public string GetVideoManifest(string videoID)
        {
            if (videoIdToManifestUrl.TryGetValue(videoID, out string manifestUrl))
            {
                return manifestUrl;
            }else
            {
                throw new Exception($"Manifest for video ID {videoID} not found in cache.");
            }
        }

        public bool IsManifestCached(string videoID)
        {
            return videoIdToManifestUrl.ContainsKey(videoID);
        }

        private void SendPlayEvent(YTUrlData data)
        {
            Task.Run(async () =>
            {
                await controlServer.Send(new
                {
                    action = "newVideo",
                    data,
                });
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
