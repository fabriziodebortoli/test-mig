using System;
using System.IO;
using System.Management;
using System.Net;
using System.Net.Sockets;
using System.Runtime.Serialization.Formatters.Binary;
using System.Threading;
using Microarea.TaskBuilderNet.Core.Generic;
using Microarea.TaskBuilderNet.Interfaces;

namespace Microarea.TaskBuilderNet.Core.WebServicesWrapper
{
	public class ProcessWMI
	{
		public int ProcessId;
		private string user;
		private string password;
		private string remoteComputerName;
		private int remoteServicePort;
		//-----------------------------------------------------------------------
		public ProcessWMI(string user, string password, string remoteComputerName, int remoteServicePort)
		{
			this.ProcessId = 0;
			this.user = user;
			this.password = password;
			this.remoteComputerName = remoteComputerName;
			this.remoteServicePort = remoteServicePort;
		}

		//-----------------------------------------------------------------------
		private static Socket ConnectSocket(string server, int port)
		{
			Socket s = null;
			IPHostEntry hostEntry = null;

			// Get host related information.
			hostEntry = Dns.GetHostEntry(server);

			// Loop through the AddressList to obtain the supported AddressFamily. This is to avoid
			// an exception that occurs when the host IP Address is not compatible with the address family
			// (typical in the IPv6 case).
			foreach (IPAddress address in hostEntry.AddressList)
			{
				IPEndPoint ipe = new IPEndPoint(address, port);
				Socket tempSocket =
					new Socket(ipe.AddressFamily, SocketType.Stream, ProtocolType.Tcp);

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
		}

		// This method requests the home page content for the specified server.
		//-----------------------------------------------------------------------
		private static TBLoaderResponse SocketSendReceive(string server, int port, TBLoaderCommand request)
		{
			BinaryFormatter fmt = new BinaryFormatter();
			MemoryStream ms = new MemoryStream();
			fmt.Serialize(ms, request);
			Byte[] bytesReceived = new Byte[256];
			TBLoaderResponse resp = new TBLoaderResponse();
			// Create a socket connection with the specified server and port.
			Socket s = ConnectSocket(server, port);

			if (s == null)
			{
				resp.Result = false;
				resp.Message = "Connection failed";
				return resp;
			}
			byte[] bytesSent = ms.ToArray();

			// Send request to the server.
			s.Send(bytesSent, bytesSent.Length, 0);

			// Receive the server home page content.
			int bytes = 0;
			ms = new MemoryStream();

			// The following will block until te page is transmitted.
			do
			{
				bytes = s.Receive(bytesReceived, bytesReceived.Length, 0);
				ms.Write(bytesReceived, 0, bytes);

			}
			while (bytes > 0);
			ms.Seek(0, SeekOrigin.Begin);
			try
			{
				resp = (TBLoaderResponse)fmt.Deserialize(ms);
			}
			catch (Exception ex)
			{
				resp.Result = false;
				resp.Message = ex.ToString();
			}
			return resp;
		}

		//-----------------------------------------------------------------------
		public void ExecuteRemoteProcessWMI(string path, string args)
		{
			try
			{
				if (remoteServicePort != 0)
				{
					TBLoaderCommand cmd = new TBLoaderCommand();
					cmd.Type = TBLoaderCommand.CommandType.Start;
					cmd.Path = path;
					cmd.Arguments = args;
					TBLoaderResponse resp = SocketSendReceive(remoteComputerName, remoteServicePort, cmd);
					if (!resp.Result)
						throw new Exception(resp.Message);
					ProcessId = resp.ProcessId;
				}
				else
				{
					string filename = Path.Combine(Functions.GetExecutingAssemblyFolderPath(), "PsExec.exe");
					if (File.Exists(filename))
					{
						ExecuteUsingPsExec(filename, path + ' ' + args);
					}
					else
					{
						ExecuteUsingManagementObject(path + ' ' + args);
					}
				}

			}
			catch (Exception e)
			{
				throw new Exception(string.Format("Execute process failed Machinename {0}, ProcessName {1}, RunAs {2}, Error is {3}, Stack trace {4}", remoteComputerName, path + ' ' + args, user, e.Message, e.StackTrace), e);
			}
		}

		//-----------------------------------------------------------------------
		private void ExecuteUsingManagementObject(string arguments)
		{
			ConnectionOptions connOptions = new ConnectionOptions();
			if (!remoteComputerName.Equals(Environment.MachineName, StringComparison.InvariantCultureIgnoreCase))
			{
				connOptions.Username = user;
				connOptions.Password = password;
			}
			connOptions.EnablePrivileges = true;
			ManagementScope manScope = new ManagementScope(String.Format(@"\\{0}\ROOT\CIMV2", remoteComputerName), connOptions);

			try
			{
				manScope.Connect();
			}
			catch (Exception e)
			{
				throw new Exception("Management Connect to remote machine " + remoteComputerName + " as user " + user + " failed with the following error " + e.Message);
			}
			ObjectGetOptions objectGetOptions = new ObjectGetOptions();
			ManagementPath managementPath = new ManagementPath("Win32_Process");
			using (ManagementClass processClass = new ManagementClass(manScope, managementPath, objectGetOptions))
			{
				using (ManagementBaseObject inParams = processClass.GetMethodParameters("Create"))
				{
					inParams["CommandLine"] = arguments;
					using (ManagementBaseObject outParams = processClass.InvokeMethod("Create", inParams, null))
					{

						if ((uint)outParams["returnValue"] != 0)
						{
							throw new Exception("Error while starting process " + arguments + " creation returned an exit code of " + outParams["returnValue"] + ". It was launched as " + user + " on " + remoteComputerName);
						}
						this.ProcessId = (int)(uint)outParams["processId"];
					}
				}
			}

			SelectQuery CheckProcess = new SelectQuery("Select * from Win32_Process Where ProcessId = " + ProcessId);
			using (ManagementObjectSearcher ProcessSearcher = new ManagementObjectSearcher(manScope, CheckProcess))
			{
				using (ManagementObjectCollection MoC = ProcessSearcher.Get())
				{
					if (MoC.Count == 0)
					{
						throw new Exception("ERROR AS WARNING: Process " + arguments + " terminated before it could be tracked on " + remoteComputerName);
					}
				}
			}


		}

		//-----------------------------------------------------------------------
		private void ExecuteUsingPsExec(string psexecPath, string arguments)
		{
			TBLoaderLauncherWrapper tbLoaderLauncherWrapper = new TBLoaderLauncherWrapper();
			ProcessId = tbLoaderLauncherWrapper.ExecuteUsingPsExec(
														psexecPath,
														arguments,
														remoteComputerName,
														user,
														password);
		}

		//-----------------------------------------------------------------------
		internal void KillRemoteProcess(int procID)
		{
			if (remoteServicePort != 0)
			{
				TBLoaderCommand cmd = new TBLoaderCommand();
				cmd.Type = TBLoaderCommand.CommandType.Stop;
				cmd.ProcessId = procID;
				TBLoaderResponse resp = SocketSendReceive(remoteComputerName, remoteServicePort, cmd);
				if (!resp.Result)
					throw new Exception(resp.Message);
				ProcessId = 0;
			}
			else
			{
				ManagementObjectCollection coll = GetTbloaderProcessList(procID);
				foreach (ManagementObject obj in coll)
					obj.InvokeMethod("Terminate", null);
			}
		}

		//-----------------------------------------------------------------------
		private ManagementObjectCollection GetTbloaderProcessList(int procID)
		{
			ConnectionOptions options = new ConnectionOptions();
			if (!remoteComputerName.Equals(Environment.MachineName, StringComparison.InvariantCultureIgnoreCase))
			{
				options.Username = user;
				options.Password = password;
			}

			ManagementScope scope = new ManagementScope(string.Format("\\\\{0}\\root\\cimv2", remoteComputerName), options);
			scope.Connect();

			ObjectQuery query = new ObjectQuery(string.Format("SELECT * FROM Win32_Process WHERE Name='tbloader.exe' AND ProcessId={0}", procID));
			ManagementObjectSearcher searcher = new ManagementObjectSearcher(scope, query);
			ManagementObjectCollection coll = searcher.Get();
			return coll;
		}

		//-----------------------------------------------------------------------
		internal bool WaitForExit(int milliseconds)
		{
			DateTime start = DateTime.Now;
			while (true)
			{
				ManagementObjectCollection coll = GetTbloaderProcessList(ProcessId);
				if (coll.Count == 0)
					return true;
				if ((DateTime.Now - start).TotalMilliseconds > milliseconds)
					return false;
				Thread.Sleep(1000);
			}
		}
    }
}
