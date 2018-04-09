using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Microarea.Common
{
    public class TbLoaderGateConfigParameters
    {
        public string TbLoaderGateProtocol { get; set; }
        public string TbLoaderGateHost { get; set; }
        public int TbLoaderGatePort { get; set; }
        public bool IsStandAlone { get; set; }

        public string TbLoaderGateFullUrl { get { return string.Format("{0}://{1}:{2}", TbLoaderGateProtocol, TbLoaderGateHost, TbLoaderGatePort); } }
    }
}
