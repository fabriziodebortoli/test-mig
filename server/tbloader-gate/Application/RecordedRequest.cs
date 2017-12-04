using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Microarea.TbLoaderGate.Application
{
    public class RecordedHttpRequest
    {
        public const string RecordingFolder = "Recordings";
        public string Url { get; set; }
        public string Body { get; set; }
        public string Response { get; set; }
    }

    public class RecordedWSRequest
    {
        public string RequestMessage { get; set; }
        public string Body { get; set; }
        public string Response { get; set; }
    }
}
