using System;
using System.Text.RegularExpressions;
using System.Threading;

namespace Microarea.TbJson.Utils
{
    public class MyCriticalSession : IDisposable
    {
        private readonly Mutex mutex;

        public MyCriticalSession(string file)
        {
            string mutexName = "Global\\" + Regex.Replace(file, "\\\\|/", ".");
            mutex = new Mutex(true, mutexName, out bool mutexWasCreated);
            if (!mutexWasCreated)
                mutex.WaitOne(Timeout.Infinite, false);
        }

        public void Dispose()
        {
            mutex?.ReleaseMutex();
        }
    }
}
