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
    public enum StreamType
    {
        DASH,
        HLS
    }
    public class StreamingService
    {
        private YTDLP ytdlp = new YTDLP();
        private DASHStreamer DashStreamer;
        private HLSStreamer HlsStreamer;

        public StreamingService(DASHStreamer dashStreamer, HLSStreamer hlsStreamer)
        {
            DashStreamer = dashStreamer;
            HlsStreamer = hlsStreamer;
        }
        public object GetStreamer(StreamType streamerType)
        {
            if (streamerType == StreamType.DASH)
            {
                return DashStreamer;
            }
            else if (streamerType == StreamType.HLS)
            {
                return HlsStreamer;
            }
            throw new ArgumentException("Invalid streamer type specified.");
        }

        public async Task<string> Use(StreamType streamType, string videoId)
        {
            if (streamType == StreamType.DASH)
            {
                if (DashStreamer.IsCached(videoId))
                {
                    YTUrlSource source = DashStreamer.GetCachedSource(videoId);
                    return source.VideoSourceUrl;
                }
                else
                {
                    throw new Exception("DASH stream not cached.");
                }
            }
            else if (streamType == StreamType.HLS)
            {
                if (HlsStreamer.IsCached(videoId))
                {

                    return await HlsStreamer.GenerateMasterPlayList(videoId);
                }
                else
                {
                    throw new Exception("HLS stream not cached.");
                }
            }
            throw new ArgumentException("Invalid stream type specified.");
        }

        async public Task Prepare(string sourceUrl)
        {
            YTUrlData yTUrlData = new YTUrlData(sourceUrl);
            YTDlpJsonDump videoMetadata = await ytdlp.GetVideoMetadata(sourceUrl);
            YTUrlSource source = YTDlpParser.GetBestSource(videoMetadata);

            if(source == null)
            {
                throw new Exception("Failed to retrieve video manifest URL.");
            }
            Logger.Debug($"Master playlist orgin url: {source.MasterPlaylistUrl}");
            DashStreamer.Load(yTUrlData.VideoId, source);
            HlsStreamer.Load(yTUrlData.VideoId, source.MasterPlaylistUrl);
        }
    }
}
