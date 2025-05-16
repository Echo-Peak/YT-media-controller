using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace YTMediaControllerSrv.Types
{
    public class NativeHostManifestJson
    {
        public string name { get; set; }
        public int manifest_version { get; set; }
        public string version { get; set; }
        public string description { get; set; }
        public string path { get; set; }
        public string type { get; set; }
        public string[] allowed_origins { get; set; }

    }
}
