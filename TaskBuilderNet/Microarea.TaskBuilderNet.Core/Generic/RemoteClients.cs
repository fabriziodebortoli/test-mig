using System;
using System.Collections.Generic;
using System.IO;
using System.Runtime.ConstrainedExecution;
using System.Runtime.InteropServices;
using System.Security;
using System.Xml;
using Microarea.TaskBuilderNet.Core.NameSolver;
using Microarea.TaskBuilderNet.Interfaces;
using Microsoft.Win32.SafeHandles;

namespace Microarea.TaskBuilderNet.Core.Generic
{
	public class RemoteClient
	{
		public string Host;
        public int Port;
		public string RemotePath;
		public string User;
		public string Password;
		public bool EasyLook = false;
		public int RemoteServicePort = 0;
		public int count = 0;

        public static bool IsRemote(RemoteClient host)
        {
            return host != null && !host.Host.CompareNoCase("localhost") && !host.Host.CompareNoCase(System.Environment.MachineName);
        }

		public static List<RemoteClient> Read(bool excludeEasylook)
		{
			List<RemoteClient> remoteClients = new List<RemoteClient>();
			string file = Path.Combine(BasePathFinder.BasePathFinderInstance.GetAppsPath(), NameSolverStrings.RemoteClientsFile);
			if (!File.Exists(file))
				return remoteClients;
			XmlDocument doc = new XmlDocument();
			doc.Load(file);
			foreach (XmlElement el in doc.DocumentElement.GetElementsByTagName("RemoteClient"))
			{
                bool easyLook = "true".Equals(el.GetAttribute("easylook"));
                if (easyLook && excludeEasylook)
                    continue;

				RemoteClient h = new RemoteClient();
				h.Host = el.GetAttribute("host");
                if (!int.TryParse(el.GetAttribute("port"), out h.Port))
                    h.Port = 10000;
				if (!int.TryParse(el.GetAttribute("serviceport"), out h.RemoteServicePort))
					h.RemoteServicePort = 0;
				h.RemotePath = el.GetAttribute("folder");
				h.User = el.GetAttribute("user");
				h.Password = el.GetAttribute("password");
                h.EasyLook = easyLook;
				remoteClients.Add(h);
			}

			return remoteClients;
		}

        public static SafeTokenHandle ImpersonateAs(string user, string password)
        {
            const int LOGON32_PROVIDER_DEFAULT = 0;
            //This parameter causes LogonUser to create a primary token. 
            const int LOGON32_LOGON_INTERACTIVE = 2;

            string domain, username;
            string[] tokens = user.Split('\\');
            if (tokens.Length == 2)
            {
                domain = tokens[0];
                username = tokens[1];
            }
            else
            {
                domain = "";
                username = user;
            }

            SafeTokenHandle safeTokenHandle;
            // Call LogonUser to obtain a handle to an access token. 
            bool returnValue = LogonUser(username, domain, password,
                LOGON32_LOGON_INTERACTIVE, LOGON32_PROVIDER_DEFAULT,
                out safeTokenHandle);

            if (false == returnValue)
            {
                int ret = Marshal.GetLastWin32Error();
                throw new ApplicationException(string.Format("PSExec : LogonUser failed with error code : {0}", ret));
            }

            return safeTokenHandle;
        }

        [DllImport("advapi32.dll", SetLastError = true, CharSet = CharSet.Unicode)]
        public static extern bool LogonUser(String lpszUsername, String lpszDomain, String lpszPassword,
            int dwLogonType, int dwLogonProvider, out SafeTokenHandle phToken);

        [DllImport("kernel32.dll", CharSet = CharSet.Auto)]
        public extern static bool CloseHandle(IntPtr handle);
	}

    public sealed class SafeTokenHandle : SafeHandleZeroOrMinusOneIsInvalid
    {
        private SafeTokenHandle()
            : base(true)
        {
        }

        [DllImport("kernel32.dll")]
        [ReliabilityContract(Consistency.WillNotCorruptState, Cer.Success)]
        [SuppressUnmanagedCodeSecurity]
        [return: MarshalAs(UnmanagedType.Bool)]
        private static extern bool CloseHandle(IntPtr handle);

        protected override bool ReleaseHandle()
        {
            return CloseHandle(handle);
        }
    }
}