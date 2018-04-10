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

        public string TbLoaderGateFullUrl
        {
            get {
                if (TbLoaderGatePort == 0 || TbLoaderGatePort == 80)
                {
                    return string.Format("{0}://{1}", TbLoaderGateProtocol, TbLoaderGateHost);
                }
                return string.Format("{0}://{1}:{2}", TbLoaderGateProtocol, TbLoaderGateHost, TbLoaderGatePort);
            }
        }
    }
}
