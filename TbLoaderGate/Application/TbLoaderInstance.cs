using System.Diagnostics;
public class TBLoaderInstance
{
    public int port = 10000;
    public string server = "localhost";
    public int processId = 0; 


    public string BaseUrl { get { return string.Concat("http://", server, ":", port); } }

    internal void Execute()
    {
        Process p = Process.Start(@"D:\development\Apps\TBApps\Debug\tbloader.exe");
        processId = p.Id;
    }

    internal void Attach()
    {
        Process p = Process.GetProcessById(processId);
        processId = p == null ? 0 : p.Id;
    }
}