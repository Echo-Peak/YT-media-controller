using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace YTMediaControllerSrv.Types
{
    public class YtDlpFormat
    {
        public string FormatId { get; set; }
        public string Url { get; set; }
        public string Manifest_Url { get; set; }
        public string Protocol { get; set; }
        public string Ext { get; set; }
        public string Vcodec { get; set; }
        public string Acodec { get; set; }
        public int? Width { get; set; }
        public int? Height { get; set; }
        public float? Fps { get; set; }
        public float? Tbr { get; set; }
        public float? Asr { get; set; }
        public float? Abr { get; set; }
        public long? Filesize { get; set; }
        public string Format_Note { get; set; }
        public string Container { get; set; }
        public string Resolution { get; set; }
        public int? Preference { get; set; }
        public string Video_Ext { get; set; }
        public string Audio_Ext { get; set; }
        public bool? IsDash { get; set; }
        public bool? IsHls { get; set; }
    }

    internal class YTDlpJsonDump
    {
        public string Id { get; set; }
        public string Title { get; set; }
        public string Uploader { get; set; }
        public string WebpageUrl { get; set; }
        public string Description { get; set; }
        public List<YtDlpFormat> Formats { get; set; }
    }
}
