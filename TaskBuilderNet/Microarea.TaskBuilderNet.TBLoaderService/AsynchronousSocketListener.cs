using System;
using System.Diagnostics;
using System.IO;
using System.Net;
using System.Net.Sockets;
using System.Threading;
using Microarea.TaskBuilderNet.Interfaces;
using Microarea.TaskBuilderNet.TbLoaderService.Properties;
using Newtonsoft.Json;
using System.Text;

namespace Microarea.TaskBuilderNet.TbLoaderService
{
    // State object for reading client data asynchronously
    public class StateObject
    {
        // Client  socket.
        public Socket workSocket = null;
        // Size of receive buffer.
        public const int BufferSize = 1024;
        // Receive buffer.
        public byte[] buffer = new byte[BufferSize];
        // Received data string.
        public MemoryStream bytes = new MemoryStream();
    }


    public class AsynchronousSocketListener
    {
        // Thread signal.
        public static ManualResetEvent allDone;
        private static bool listening;
        private static int port = Settings.Default.Port;
        private static Engine engine = new Engine();

        public static void StopListening()
        {
            listening = false;
            allDone.Set();
            engine.StopAll();
        }
        public static void StartListening()
        {
            if (listening)
                return;

            listening = true;
            allDone = new ManualResetEvent(false);
            Thread t = new Thread(new ThreadStart(() =>
            {
                ListeningProcedure();
            }));
            t.Start();
        }
        public static void ListeningProcedure()
        {
            //engine.CreateSlot(null);
            // Establish the local endpoint for the socket.
            // The DNS name of the computer
            IPHostEntry ipHostInfo = Dns.GetHostEntry("localhost");
            IPAddress ipAddress = null;
            for (int i = 0; i < ipHostInfo.AddressList.Length; i++)
                if (ipHostInfo.AddressList[i].AddressFamily == System.Net.Sockets.AddressFamily.InterNetwork)
                {
                    ipAddress = ipHostInfo.AddressList[i];
                    break;
                }

            if (ipAddress == null)
            {
                engine.Message("No IPv4 address for this computer", DiagnosticType.Error);
                return;
            }

            IPEndPoint localEndPoint = new IPEndPoint(ipAddress, port);
            Console.WriteLine(string.Concat("Listening on port ", port));
            // Create a TCP/IP socket.
            Socket listener = new Socket(AddressFamily.InterNetwork,
                SocketType.Stream, ProtocolType.Tcp);

            // Bind the socket to the local endpoint and listen for incoming connections.
            try
            {
                listener.Bind(localEndPoint);
                listener.Listen(100);

                while (listening)
                {
                    // Set the event to nonsignaled state.
                    allDone.Reset();

                    // Start an asynchronous socket to listen for connections.
                    listener.BeginAccept(
                        new AsyncCallback(AcceptCallback),
                        listener);

                    // Wait until a connection is made before continuing.
                    allDone.WaitOne();
                }

            }
            catch (Exception e)
            {
                engine.Message(e.ToString(), DiagnosticType.Error);
            }
            allDone.Dispose();
            allDone = null;
        }

        public static void AcceptCallback(IAsyncResult ar)
        {
            // Signal the main thread to continue.
            allDone.Set();

            // Get the socket that handles the client request.
            Socket listener = (Socket)ar.AsyncState;
            Socket handler = listener.EndAccept(ar);

            // Create the state object.
            StateObject state = new StateObject();
            state.workSocket = handler;
            handler.BeginReceive(state.buffer, 0, StateObject.BufferSize, 0,
                new AsyncCallback(ReadCallback), state);
        }

        public static void ReadCallback(IAsyncResult ar)
        {
            try
            {
                String content = String.Empty;

                // Retrieve the state object and the handler socket
                // from the asynchronous state object.
                StateObject state = (StateObject)ar.AsyncState;
                Socket handler = state.workSocket;

                // Read data from the client socket. 
                int bytesRead = handler.EndReceive(ar);

                if (bytesRead > 0)
                {
                    // There  might be more data, so store the data received so far.
                    state.bytes.Write(state.buffer, 0, bytesRead);

                    // Check for end-of-file tag (newline). If it is not there, read 
                    // more data.
                    if (handler.Available == 0)
                    {
                        try
                        {
                            state.bytes.Seek(0, SeekOrigin.Begin);
                            string json = Encoding.UTF8.GetString(state.bytes.ToArray());
                            TBLoaderCommand cmd = JsonConvert.DeserializeObject<TBLoaderCommand>(json);
                            TBLoaderResponse resp = DoCommand(cmd);
                            Send(handler, resp);
                        }
                        catch (Exception ex)
                        {
                            TBLoaderResponse resp = new TBLoaderResponse();
                            resp.Message = ex.ToString();
                            resp.Result = false;
                            Send(handler, resp);
                        }
                    }
                    else
                    {
                        // Not all data received. Get more.
                        handler.BeginReceive(state.buffer, 0, StateObject.BufferSize, 0,
                        new AsyncCallback(ReadCallback), state);
                    }
                }
            }
            catch (Exception ex)
            {
                engine.Message(ex.ToString(), DiagnosticType.Error);
            }
        }

        private static void Send(Socket handler, TBLoaderResponse resp)
        {
            string json = JsonConvert.SerializeObject(resp);
            Send(handler, Encoding.UTF8.GetBytes(json));
        }

        private static TBLoaderResponse DoCommand(TBLoaderCommand cmd)
        {
            switch (cmd.Type)
            {
                case TBLoaderCommand.CommandType.Ping:
                    {
                        bool result = false;
                        Process p = Process.GetProcessById(cmd.ProcessId);
                        result = p != null && !p.HasExited && p.Responding;
                        return new TBLoaderResponse() { Result = result };
                    }
                case TBLoaderCommand.CommandType.Start:
                    engine.Message("Executing command: " + cmd.Type.ToString(), DiagnosticType.Information);
                    return engine.Start(cmd, false);
                case TBLoaderCommand.CommandType.Stop:
                    engine.Message("Executing command: " + cmd.Type.ToString(), DiagnosticType.Information);
                    return engine.Stop(cmd);
            }
            return null;
        }










        private static void Send(Socket handler, byte[] byteData)
        {

            // Begin sending the data to the remote device.
            handler.BeginSend(byteData, 0, byteData.Length, 0,
                new AsyncCallback(SendCallback), handler);
        }

        private static void SendCallback(IAsyncResult ar)
        {
            try
            {
                // Retrieve the socket from the state object.
                Socket handler = (Socket)ar.AsyncState;

                // Complete sending the data to the remote device.
                int bytesSent = handler.EndSend(ar);
                handler.Shutdown(SocketShutdown.Both);
                handler.Close();

            }
            catch (Exception e)
            {
                engine.Message(e.ToString(), DiagnosticType.Error);
            }
        }



    }
}