using System;
using System.Diagnostics;
using System.IO;
using System.IO.Pipes;
using System.Text;
using Newtonsoft.Json;
using SharedCode;

namespace Microarea.TbJson
{
    internal static class CacheConnector
    {
        static ClientFormMap clientForms;
        static ControlClassMap controlClasses;
        static IconsMatchMap iconsMatch;
        private const string serverProcess = "TbJsonCacheServer";

        //-----------------------------------------------------------------------------
        internal static ClientFormMap GetClientForms()
        {
            if (clientForms == null)
            {
                clientForms = GetCachedObject<ClientFormMap>(Shared.clientFormsCommand);
            }
            return clientForms;
        }
        //-----------------------------------------------------------------------------
        internal static ControlClassMap GetControlClasses()
        {
            if (controlClasses == null)
            {
                controlClasses = GetCachedObject<ControlClassMap>(Shared.controlClassesCommand);
            }
            return controlClasses;
        }

        //-----------------------------------------------------------------------------
        internal static IconsMatchMap GetIconsMatch()
        {
            if (iconsMatch == null)
            {
                iconsMatch = GetCachedObject<IconsMatchMap>(Shared.iconsMatchCommand);
            }
            return iconsMatch;
        }

        //-----------------------------------------------------------------------------
        private static T GetCachedObject<T>(string cmd)
        {
            using (NamedPipeClientStream pipeClient = GetNamedPipeStream())
            {
                byte[] buff = Encoding.UTF8.GetBytes(cmd);
                pipeClient.Write(buff, 0, buff.Length);

                using (StreamReader sr = new StreamReader(pipeClient))
                {
                    string s = sr.ReadToEnd();
                    return new JsonSerializer().Deserialize<T>(new JsonTextReader(new StringReader(s)));
                }
            }
        }

        //-----------------------------------------------------------------------------
        private static NamedPipeClientStream GetNamedPipeStream()
        {
            bool processRunning = Process.GetProcessesByName(serverProcess).Length > 0;
            NamedPipeClientStream pipeClient = new NamedPipeClientStream(".", Shared.namedPipe, PipeDirection.InOut);
            if (!processRunning)
                RunServerProcess();
            try
            {
                pipeClient.Connect(10000);
            }
            catch (TimeoutException)
            {
                throw new Exception(string.Concat("Cannot connect to ", serverProcess));
            }
            return pipeClient;
        }

        private static void RunServerProcess()
        {
            string procName = Path.Combine(Path.GetDirectoryName(System.Reflection.Assembly.GetEntryAssembly().Location), serverProcess);
            ProcessStartInfo psi = new ProcessStartInfo(procName);
            psi.UseShellExecute = true;
            psi.CreateNoWindow = true;
            psi.WindowStyle = ProcessWindowStyle.Hidden;
            Process p = Process.Start(psi);
        }
    }
}
