using System;
using System.IO;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading.Tasks;
using Newtonsoft.Json;

namespace Microarea.TbLoaderGate
{
    public class TBLoaderCommand
    {
        public enum CommandType { Start, Stop }
        public CommandType Type { get; set; }
        public int ProcessId { get; set; }
        public string ClientId { get; set; }
    }

    public class TBLoaderResponse
    {
		public bool Result { get; set; }
		public string Message { get; set; }
		public int Port { get; set; }
		public int ProcessId { get; set; }
	}

    public class TBLoaderConnectionParameters
    {
        public int TbLoaderServicePort { get; set; }
        public string TbLoaderServiceHost { get; set; }
        public bool RecordingMode { get; set; }
        public bool TestMode { get; set; }
    }

    public class TBLoaderService
    {
        private string serviceComputerName = "";
        private int servicePort = -1;
        //-----------------------------------------------------------------------
        public TBLoaderService(string server, int port)
        {
            this.serviceComputerName = server;
            this.servicePort = port;
        }

        //-----------------------------------------------------------------------
        private Task<Socket> ConnectSocket(string server, int port)
        {
            // Get host related information.
            return Dns.GetHostEntryAsync(server).ContinueWith(t =>
            {
                Socket s = null;
                IPHostEntry hostEntry = t.Result;

                // Loop through the AddressList to obtain the supported AddressFamily. This is to avoid
                // an exception that occurs when the host IP Address is not compatible with the address family
                // (typical in the IPv6 case).
                foreach (IPAddress address in hostEntry.AddressList)
                {
                    if (address.AddressFamily != System.Net.Sockets.AddressFamily.InterNetwork)
                        continue;
                    
					IPEndPoint ipe = new IPEndPoint(address, port);
                    Socket tempSocket = new Socket(ipe.AddressFamily, SocketType.Stream, ProtocolType.Tcp);
                    tempSocket.Connect(ipe);

                    if (tempSocket.Connected)
                    {
                        s = tempSocket;
                        break;
                    }
                    else
                    {
                        continue;
                    }
                }
                return s;
            });
        }

        // This method requests the home page content for the specified server.
        //-----------------------------------------------------------------------
        private Task<TBLoaderResponse> SocketSendReceive(string server, int port, TBLoaderCommand request)
        {
            // Create a socket connection with the specified server and port.
            return ConnectSocket(server, port).ContinueWith(t =>
            {
                Socket s = t.Result;
                if (s == null)
                {
                    TBLoaderResponse resp = new TBLoaderResponse();
                    resp.Result = false;
                    resp.Message = "Connection failed";
                    return resp;
                }

                string json = JsonConvert.SerializeObject(request);
                byte[] bytesSent = Encoding.UTF8.GetBytes(json);

                // Send request to the server.
                s.Send(bytesSent, bytesSent.Length, 0);

                // Receive the server home page content.
                int bytes = 0;
                MemoryStream ms = new MemoryStream();

                // The following will block until te page is transmitted.
                do
                {
                    Byte[] bytesReceived = new Byte[256];
                    bytes = s.Receive(bytesReceived, bytesReceived.Length, 0);
                    ms.Write(bytesReceived, 0, bytes);

                }
                while (bytes > 0);
                ms.Seek(0, SeekOrigin.Begin);
                try
                {
                    string jsonResp = Encoding.UTF8.GetString(ms.ToArray());
                    TBLoaderResponse resp = JsonConvert.DeserializeObject<TBLoaderResponse>(jsonResp);
                    s.Dispose();
                    return resp;
                }
                catch (Exception ex)
                {
                    TBLoaderResponse resp = new TBLoaderResponse();
                    resp.Result = false;
                    resp.Message = ex.ToString();
                    s.Dispose();
                    return resp;
                }
            });
        }

        //-----------------------------------------------------------------------
        public async Task<TBLoaderResponse> ExecuteRemoteProcessAsync(string clientID)
        {
            try
            {
                TBLoaderCommand cmd = new TBLoaderCommand();
                cmd.Type = TBLoaderCommand.CommandType.Start;
                cmd.ClientId = clientID;
                TBLoaderResponse res = await SocketSendReceive(serviceComputerName, servicePort, cmd);
                if (!res.Result)
                    throw new Exception(res.Message);
                return res;
            }
            catch (Exception e)
            {
                throw new Exception(string.Format("Execute process failed; machinename {0}, Error is {1}, Stack trace {2}", serviceComputerName, e.Message, e.StackTrace), e);
            }
        }


        //-----------------------------------------------------------------------
        internal void KillRemoteProcess(int procID)
        {
            TBLoaderCommand cmd = new TBLoaderCommand();
            cmd.Type = TBLoaderCommand.CommandType.Stop;
            cmd.ProcessId = procID;
            Task<TBLoaderResponse> t = SocketSendReceive(serviceComputerName, servicePort, cmd);
            t.Wait();
            TBLoaderResponse resp = t.Result;
            if (!resp.Result)
                throw new Exception(resp.Message);
        }
    }
}
