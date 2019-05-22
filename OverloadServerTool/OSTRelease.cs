using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OverloadServerTool
{
    public class OSTRelease
    {
        public string DownloadUrl { get; set; }
        public long Size { get; set; }
        public DateTime Created { get; set; }
        public string Version { get; set; }
    }
}
