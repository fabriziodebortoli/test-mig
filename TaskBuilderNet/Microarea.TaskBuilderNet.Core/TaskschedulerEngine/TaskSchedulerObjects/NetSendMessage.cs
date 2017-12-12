using System;
using System.Collections;
using System.Runtime.InteropServices;
using System.ServiceProcess;

namespace Microarea.TaskBuilderNet.Core.TaskschedulerEngine.TaskSchedulerObjects
{
	//====================================================================================
	public class WTENetSendMessage
    {
		// server types accepted by the NetServerEnum function
		private enum ServerType : uint
		{
			SV_TYPE_WORKSTATION			= 0x00000001,
			SV_TYPE_SERVER				= 0x00000002,
			SV_TYPE_SQLSERVER			= 0x00000004,
			SV_TYPE_DOMAIN_CTRL			= 0x00000008,
			SV_TYPE_DOMAIN_BAKCTRL		= 0x00000010,
			SV_TYPE_TIME_SOURCE			= 0x00000020,
			SV_TYPE_AFP					= 0x00000040,
			SV_TYPE_NOVELL				= 0x00000080,
			SV_TYPE_DOMAIN_MEMBER		= 0x00000100,
			SV_TYPE_PRINTQ_SERVER		= 0x00000200,
			SV_TYPE_DIALIN_SERVER		= 0x00000400,
			SV_TYPE_XENIX_SERVER		= 0x00000800,
			SV_TYPE_SERVER_UNIX			= SV_TYPE_XENIX_SERVER,
			SV_TYPE_NT					= 0x00001000,
			SV_TYPE_WFW					= 0x00002000,
			SV_TYPE_SERVER_MFPN			= 0x00004000,
			SV_TYPE_SERVER_NT			= 0x00008000,
			SV_TYPE_POTENTIAL_BROWSER	= 0x00010000,
			SV_TYPE_BACKUP_BROWSER		= 0x00020000,
			SV_TYPE_MASTER_BROWSER		= 0x00040000,
			SV_TYPE_DOMAIN_MASTER		= 0x00080000,
			SV_TYPE_SERVER_OSF			= 0x00100000,
			SV_TYPE_SERVER_VMS			= 0x00200000,
			SV_TYPE_WINDOWS				= 0x00400000, 
			SV_TYPE_DFS					= 0x00800000, 
			SV_TYPE_CLUSTER_NT			= 0x01000000, 
			SV_TYPE_TERMINALSERVER		= 0x02000000, 
			SV_TYPE_CLUSTER_VS_NT		= 0x04000000, 
			SV_TYPE_DCE					= 0x10000000, 
			SV_TYPE_ALTERNATE_XPORT		= 0x20000000, 
			SV_TYPE_LOCAL_LIST_ONLY		= 0x40000000, 
			SV_TYPE_DOMAIN_ENUM			= 0x80000000,
			SV_TYPE_ALL					= 0xFFFFFFFF 
		};
		// Server info function, returned by the NetServerEnum function
		[StructLayout(LayoutKind.Sequential)]
			private struct WorkStationInfo
		{
			[MarshalAs(System.Runtime.InteropServices.UnmanagedType.U4)] public UInt32		PlatformId;
			[MarshalAs(System.Runtime.InteropServices.UnmanagedType.LPWStr)]public string	Name;
			[MarshalAs(System.Runtime.InteropServices.UnmanagedType.U4)] public UInt32		VersionMajor;
			[MarshalAs(System.Runtime.InteropServices.UnmanagedType.U4)] public UInt32		VersionMinor;
			[MarshalAs(System.Runtime.InteropServices.UnmanagedType.U4)] public UInt32		Type;
			[MarshalAs(System.Runtime.InteropServices.UnmanagedType.LPWStr)] public string	Comment;
		};
		private enum PLATFORM_ID
		{
			PLATFORM_ID_DOS = 300,
			PLATFORM_ID_OS2 = 400,
			PLATFORM_ID_NT = 500,
			PLATFORM_ID_OSF = 600,
			PLATFORM_ID_VMS = 700
		}

		//--------------------------------------------------------------------------------------------------------------------------------
		// System functions imported from the NET api
		//--------------------------------------------------------------------------------------------------------------------------------
		
		[DllImport ("Netapi32", CharSet=CharSet.Unicode)] 
		private static extern int NetMessageBufferSend( 
			string	serverName,	// The DNS or NetBIOS name of the remote server on which the function is to execute. If this parameter is NULL, the local computer is used.
			string	msgName,	// The message alias to which the message buffer should be sent
			string	fromName,	// A string specifying who the message is from. If this parameter is NULL, the message is sent from the local computer name.
			string	buffer, 
			int		bufferLength); 


		[DllImport("netapi32.dll",EntryPoint="NetServerEnum")]
		private static extern int NetServerEnum( 
			[MarshalAs(UnmanagedType.LPWStr)]string serverName,			// Reserved; must be NULL. 
			int										level, 
			out IntPtr								bufferPtr, 
			int										preferredMaxLength, // the preferred maximum length of returned data, in bytes 
			ref int									entriesRead,		// receives the count of elements actually enumerated
			ref int									totalEntries,
			ServerType								serverType,
			[MarshalAs(UnmanagedType.LPWStr)]string domain,				// the name of the domain for which a list of servers is to be returned
			int										resumeHandle);

		[DllImport ("Netapi32", CharSet=CharSet.Unicode)] 
		private static extern int  NetServerGetInfo(
			string		serverName,
			int			level, 
			out IntPtr	bufferPtr); 

		[DllImport("netapi32.dll",EntryPoint="NetApiBufferFree")]
		private static extern int NetApiBufferFree(IntPtr bufferPtr);
		//--------------------------------------------------------------------------------------------------------------------------------

		
		private string recipient = null;
		private string message = null;
		//--------------------------------------------------------------------------------------------------------------------------------
		public WTENetSendMessage(string aRecipient, string aMessage)
		{
			// devo controllare che aRecipient sia un server in rete
			if (IsValidWorkstationName(aRecipient))
				recipient = aRecipient;

			message = aMessage;
		}
		
		//--------------------------------------------------------------------------------------------------------------------------------
		public bool Send()
		{
			if (recipient == null || recipient == String.Empty)
				return false;

            // NetMessageBufferSend is used to send messages to any host on network (LAN or 
            // the internet) where messenger service is running. 
            // Moreover on local computer workstation service should also be enabled.

            //ServiceController sc = new ServiceController("Messenger");
            //if (sc.Status != ServiceControllerStatus.Running)
            //    throw new ScheduledTaskException(TaskSchedulerObjectsStrings.MessengerServiceNotRunning);

            // Send the message
            int returnCode = NetMessageBufferSend(null, recipient, null, message, message.Length * 2 + 2);
			return (returnCode == 0);
		}

		//--------------------------------------------------------------------------------------------------------------------------------
		public static int EnumWorkstations(out ArrayList workstations)
		{
			workstations = null;

			int entriesRead = 0;
			int totalEntries = 0;

			// Buffer to store the available servers, filled by the NetServerEnum function
			IntPtr buffer = new IntPtr();

			// Call NetServerEnum function. 
			// The '101' defines the type of structure returned: in this case, 
			// WorkStationInfo which contains the type of information we need. 
			// In the preferred max length parameter '-1' is passed to tell the  
			// function to allocate the necessary amount of memory for the data. 
			// The SV_TYPE_WORKSTATION tells the function to only return workstation
			// type servers.
			int returnCode = NetServerEnum(null, 101, out buffer, -1, ref entriesRead, ref totalEntries, ServerType.SV_TYPE_WORKSTATION, null, 0);

			if (returnCode != 0 || entriesRead <= 0)
				return 0;

			workstations = new ArrayList();

			Int32 ptr = buffer.ToInt32();

			for(int i=0; i < entriesRead; i++)
			{
				// cast the pointer to a WorkStationInfo structure
				WorkStationInfo workstationInfo = (WorkStationInfo)Marshal.PtrToStructure(new IntPtr(ptr),typeof(WorkStationInfo));

				workstations.Add(workstationInfo.Name);
				ptr += Marshal.SizeOf(workstationInfo);
			}

			// free the buffer 
			NetApiBufferFree(buffer);
				
			return totalEntries;
		}

		//--------------------------------------------------------------------------------------------------------------------------------
		public static bool IsValidWorkstationName(string aWorkstationName)
		{
			IntPtr buffer = new IntPtr();
			
			int returnCode = NetServerGetInfo(aWorkstationName, 101, out buffer);
			
			// free the buffer 
			NetApiBufferFree(buffer);
	
			return (returnCode == 0);
		}
	}
}