using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace YTMediaControllerSrv.Types
{
    public class NamedPipeMessage
    {
        public string Action { get; set; }
        public object Data { get; set; }
    }
}
