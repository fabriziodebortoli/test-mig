using System;
using System.IO;
using System.IO.Pipes;
using System.Text;
using System.Threading;
using Newtonsoft.Json;
using SharedCode;

namespace ClientFormsProvider
{
    internal static class Program
    {
        private const int timeout = 3 * 60000;

        static void Main(string[] args)
        {
            NamedPipeServerStream currentPipeServer = null;
            bool running = true;
            Timer t = new Timer((state) =>
            {
                running = false;
                if (currentPipeServer != null)
                    currentPipeServer.Dispose();
            }, null, timeout, Timeout.Infinite);
            Parser parser = new Parser();
            parser.Parse(Path.Combine(Shared.GetInstallationFolder(), "standard"));
            while (running)
            {
                using (NamedPipeServerStream pipeServer = new NamedPipeServerStream(Shared.namedPipe, PipeDirection.InOut, 1, PipeTransmissionMode.Message, PipeOptions.Asynchronous))
                {
                    try
                    {
                        currentPipeServer = pipeServer;
                        pipeServer.WaitForConnection();
                        t.Change(timeout, Timeout.Infinite);
                        string cmd = "";
                        const int buffSize = 1024;
                        byte[] buff = new byte[buffSize];
                        int read = pipeServer.Read(buff, 0, buffSize);
                        cmd = Encoding.UTF8.GetString(buff, 0, read);

                        Object response = null;
                        switch (cmd)
                        {
                            case Shared.clientFormsCommand:
                                response = parser.ClientForms;
                                break;
                            case Shared.controlClassesCommand:
                                response = parser.ControlClasses;
                                break;
                            default:
                                response = "Unknown command";
                                break;
                        }
                        using (StreamWriter sw = new StreamWriter(pipeServer))
                        {
                            using (JsonTextWriter tw = new JsonTextWriter(sw))
                            {
                                new JsonSerializer().Serialize(tw, response);
                            }
                        }
                    }
                    // Catch the IOException that is raised if the pipe is broken
                    // or disconnected.
                    catch (IOException)
                    {

                    }
                }
            }
        }


    }
}
