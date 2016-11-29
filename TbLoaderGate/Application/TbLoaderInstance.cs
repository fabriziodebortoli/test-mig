namespace TBLoaderGate
{
    public class TBLoaderInstance
    {
        public int port = 11000;
        public string server = "localhost";

        public string BaseUrl { get { return string.Concat("http://", server, ":", port); } }


        internal void Execute()
        {
            TBLoaderService svc = new TBLoaderService("localhost", port);
            port = svc.ExecuteRemoteProcess();
        }

    }
}