using System;
using System.Diagnostics;
using System.IO;
namespace TBLoaderGate
{
    public class TBLoaderInstance
    {
        public int port = 10000;
        public string server = "localhost";

        public string BaseUrl { get { return string.Concat("http://", server, ":", port); } }


        internal void Execute()
        {
            TBLoaderService svc = new TBLoaderService("localhost", 11000);
            port = svc.ExecuteRemoteProcess();
        }

    }
}