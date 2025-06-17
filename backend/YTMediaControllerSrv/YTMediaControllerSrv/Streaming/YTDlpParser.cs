using Microsoft.SqlServer.Server;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using YTMediaControllerSrv.Streaming;
using YTMediaControllerSrv.Types;

namespace YTMediaControllerSrv {
    internal class YTDlpParser
    {
        public static YTUrlSource GetBestSource(YTDlpJsonDump dump)
        {

            var formats = dump.Formats;
            if (formats == null)
            {
                throw new Exception("Unable to find any formats in json dump");
            }
                

            var audioCandidates = formats
                .Where(f => IsHttps(f) && IsAudioOnly(f))
                .Select(f => new AudioScore(f))
                .OrderByDescending(a => a)
                .ToList();

            var videoCandidates = formats
                .Where(f => IsHttps(f) && IsVideoOnly(f))
                .Select(f => new VideoScore(f))
                .OrderByDescending(v => v)
                .ToList();

            var manifestCandidates = formats
                .Where(f => IsManifestOnly(f))
                .Select(f => new VideoScore(f))
                .OrderByDescending(v => v)
                .ToList();

            var audioUrl = audioCandidates.FirstOrDefault()?.Url;
            var videoUrl = videoCandidates.FirstOrDefault()?.Url;
            var manifestUrl = manifestCandidates.FirstOrDefault()?.Url;

            return new YTUrlSource(videoUrl, audioUrl, manifestUrl);
        }

        private static bool IsHttps(YtDlpFormat f)
        {
            return f.Protocol == "https";
        }

        private static bool IsManifestOnly(YtDlpFormat f)
        {
            return f.Protocol == "m3u8_native";
        }

        private static bool IsAudioOnly(YtDlpFormat f)
        {
            return f.Vcodec == "none" && f.Acodec != "none";
        }

        private static bool IsVideoOnly(YtDlpFormat f)
        {
            return f.Acodec == "none" && f.Vcodec != "none";
        }
        private class AudioScore : IComparable<AudioScore>
        {
            public string Url { get; }
            public int CodecRank { get; }
            public double Abr { get; }
            public int NoteRank { get; }

            public AudioScore(YtDlpFormat f)
            {
                Url = f.Url;
                var acodec = (f.Acodec ?? "").ToLowerInvariant();

                if (acodec.Contains("mp4a"))
                    CodecRank = 0;
                else if (acodec.Contains("opus"))
                    CodecRank = 1;
                else
                    CodecRank = 2;

                Abr = (double?)f.Abr ?? 0;

                var note = (f.Format_Note ?? "").ToLowerInvariant();
                if (note.Contains("high"))
                    NoteRank = 0;
                else if (note.Contains("medium"))
                    NoteRank = 1;
                else if (note.Contains("low"))
                    NoteRank = 2;
                else
                    NoteRank = 3;
            }

            public int CompareTo(AudioScore other)
            {
                int c = CodecRank.CompareTo(other.CodecRank);
                if (c != 0) return -c;

                c = Abr.CompareTo(other.Abr);
                if (c != 0) return c;

                return -NoteRank.CompareTo(other.NoteRank);
            }
        }
        private class VideoScore : IComparable<VideoScore>
        {
            public string Url { get; }
            public int CodecRank { get; }
            public int Height { get; }
            public double Tbr { get; }

            public VideoScore(YtDlpFormat f)
            {
                Url = f.Url;
                var vcodec = (f.Vcodec ?? "").ToLowerInvariant();

                if (vcodec.Contains("av01"))
                    CodecRank = 0;
                else if (vcodec.Contains("vp09"))
                    CodecRank = 1;
                else if (vcodec.Contains("h264"))
                    CodecRank = 2;
                else
                    CodecRank = 3;

                Height = (int?)f.Height ?? 0;
                Tbr = (double?)f.Tbr ?? 0;
            }

            public int CompareTo(VideoScore other)
            {
                int c = CodecRank.CompareTo(other.CodecRank);
                if (c != 0) return -c;

                c = Height.CompareTo(other.Height);
                if (c != 0) return c;

                return Tbr.CompareTo(other.Tbr);
            }
        }
    }
}
