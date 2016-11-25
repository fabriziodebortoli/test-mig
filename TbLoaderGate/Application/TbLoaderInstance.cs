using System;
using System.Diagnostics;
using System.IO;

public class TBLoaderInstance
{
    public int port = 10000;
    public string server = "localhost";
    public int processId = 0;

    static string tbloaderPath = GetTbLoaderPath();

    private static string GetTbLoaderPath()
    {
        string path = AppContext.BaseDirectory;
        while (!Directory.Exists(Path.Combine(path, "Apps\\TBApps")))
        {
            path = Path.GetDirectoryName(path);
            if (string.IsNullOrEmpty(path))
                break;
        }
        if (string.IsNullOrEmpty(path))
        {
            throw new Exception("Cannot find TBLoader path");
        }
        path = Path.Combine(path, "Apps\\TBApps\\debug\\tbloader.exe");
        if (!File.Exists(path))
        {
            throw new Exception("Cannot find TBLoader path");
        }
        return path;
    }

    public string BaseUrl { get { return string.Concat("http://", server, ":", port); } }

    //-----------------------------------------------------------------------
    protected string GetNewSemaphorePath()
    {
        return Path.Combine(Path.GetTempPath(), Guid.NewGuid().ToString());
    }
    internal void Execute()
    {
        Process p = null;
        string semaphore = GetNewSemaphorePath();
        Directory.CreateDirectory(semaphore);
        string arguments  = string.Format(" Semaphore=\"{0}\"", semaphore);

        try
        {
            p = Process.Start(GetTbLoaderPath(), arguments);
            if (p != null)
                WaitForListenerAvailable(semaphore, 90000);
        }
        finally
        {
            if (Directory.Exists(semaphore))
                try
                {
                    Directory.Delete(semaphore);
                }
                catch
                {
                }
        }

        processId = p.Id;
    }

//-----------------------------------------------------------------------
		static bool IsBusy(int port)
		{
			try
			{
				/*IPGlobalProperties ipGP = IPGlobalProperties.GetIPGlobalProperties();
				IPEndPoint[] endpoints = ipGP.GetActiveTcpListeners();
				if (endpoints == null || endpoints.Length == 0) return false;
				for (int i = 0; i < endpoints.Length; i++)
					if (endpoints[i].Port == port)
						return true;*/
				return false;
			}
			catch //esiste un buco del framework per cui si schianta la GetActiveTcpListeners su Win2003SP2
			{
				return false;
			}
		}
    protected void WaitForListenerAvailable(string folder, int timeoutMilliseconds)
		{
			//non uso un FileSystemWatcher perche su certe configurazioni di WIN2003 non funziona
			DateTime start = DateTime.Now;
			while (Directory.Exists(folder))//aspetto la partenza di tbloader, quando avra' finito lo startup cancellera' la cartella
			{
				//Thread.Sleep(1000);

				if ((DateTime.Now - start).TotalMilliseconds > timeoutMilliseconds)
					throw new TimeoutException("TbLoader timeout");
			}
		}
    internal void Attach()
    {
        Process p = Process.GetProcessById(processId);
        processId = p == null ? 0 : p.Id;
    }
}