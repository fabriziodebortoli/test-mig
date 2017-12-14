// Security descriptors and NTFS file objects
using System;
using System.Globalization;
using System.Text;
using System.Runtime.InteropServices;
using System.Diagnostics;

namespace Microarea.Library.SystemServices
{
	/// <summary>
	/// Summary description for SecurityDescriptor.
	/// </summary>
	public class SecurityDescriptor
	{
		//---------------------------------------------------------------------
		[DllImportAttribute("advapi32.dll", SetLastError=true)]
		private static extern int GetNamedSecurityInfo (
			string pObjectName,
			int ObjectType,
			int SecurityInfo,
			out IntPtr ppsidOwner,
			out IntPtr ppsidGroup,
			out IntPtr ppDacl,
			out IntPtr ppSacl,
			out IntPtr ppSecurityDescriptor
			);

		//---------------------------------------------------------------------
		[DllImportAttribute("advapi32.dll", SetLastError=true)]
		private static extern int SetNamedSecurityInfo (
			string pObjectName,
			int ObjectType,
			int SecurityInfo,
			IntPtr ppsidOwner,
			IntPtr ppsidGroup,
			IntPtr ppDacl,
			IntPtr ppSacl
			);

		//---------------------------------------------------------------------
		[DllImportAttribute("advapi32.dll", SetLastError=true)]
		internal static extern int GetAclInformation(
			IntPtr pAcl,
			out  ACL_SIZE_INFORMATION pAclInformation,
			int AclInformationLength,
			int dwAclInformationClass
			);

		//---------------------------------------------------------------------
		[DllImportAttribute("advapi32.dll", SetLastError=true)]
		internal static extern bool InitializeAcl(
			IntPtr pAcl,
			int AclInformationLength,
			int AclRevision
			);


		//---------------------------------------------------------------------
		[DllImportAttribute("advapi32.dll", SetLastError=true)]
		internal static extern int GetSidLengthRequired ( byte SubAuthCount);

		const int ACL_REVISION = 2;

		enum ACL_INFORMATION_CLASS
		{
			AclRevisionInformation = 1,
			AclSizeInformation
		}

		//---------------------------------------------------------------------
		[DllImportAttribute("advapi32.dll", SetLastError=true)]
		internal static extern int GetAce (
			IntPtr pAcl,
			int dwAceIndex,
			out IntPtr pAce
			);

		//---------------------------------------------------------------------
		[DllImportAttribute("advapi32.dll", SetLastError=true)]
		internal static extern int AddAccessAllowedAceEx (
			IntPtr pAcl,
			int dwAceRevision,
			int AceFlags,
			uint AccessMask, // int AccessMask, //fred
			IntPtr pSid
			);

		//---------------------------------------------------------------------
		[DllImportAttribute("advapi32.dll", SetLastError=true)]
		internal static extern int AddAce (
			IntPtr pAcl,
			int dwAceRevision,
			uint dwStartingAceIndex,
			IntPtr pAceList,
			int nAceListLength
			);

		//---------------------------------------------------------------------
		[DllImport( "advapi32.dll", CharSet=CharSet.Auto, SetLastError=true)]
		private static extern int LookupAccountName( string systemName, string accountName,
			IntPtr psid, ref int cbsid,
			[Out] StringBuilder domainName, ref int cbDomainName, out int use );

		// by fred
		//---------------------------------------------------------------------
		[DllImport( "advapi32.dll", CharSet=CharSet.Auto, SetLastError=true)]
		private static extern int ConvertStringSidToSid
			(
			[In] StringBuilder stringSid, 
			[Out] out IntPtr pSid
			);

		//---------------------------------------------------------------------
		[StructLayoutAttribute(LayoutKind.Sequential)]
		internal struct ACL_SIZE_INFORMATION 
		{
			internal int AceCount;
			internal int AclBytesInUse;
			internal int AclBytesFree;
		}

		//---------------------------------------------------------------------
		[StructLayoutAttribute(LayoutKind.Sequential)]
		internal struct ACCESS_ALLOWED_ACE 
		{
			internal ACE_HEADER Header;
			internal uint Mask;
			internal uint SidStart;
		}

		//---------------------------------------------------------------------
		[StructLayoutAttribute(LayoutKind.Sequential)]
		internal struct ACE_HEADER 
		{
			internal byte AceType;
			internal byte AceFlags;
			internal short AceSize;
		}

		/*
		//---------------------------------------------------------------------
		[StructLayoutAttribute(LayoutKind.Sequential)]
		internal struct SID 
		{
			internal byte Revision;
			internal byte  SubAuthorityCount;
			internal SID_IDENTIFIER_AUTHORITY IdentifierAuthority;
			internal uint SubAuthority;
		}

		//---------------------------------------------------------------------
		internal class SID_IDENTIFIER_AUTHORITY 
		{
			internal byte[] Value; // TODO init Value

			internal SID_IDENTIFIER_AUTHORITY()
			{
				Value[0] = 0;
			}
		}
		*/

		internal const int SECURITY_DIALUP_RID = 0x00000001;
		internal const int SECURITY_NETWORK_RID = 0x00000002;
		internal const int SECURITY_BATCH_RID = 0x00000003;

		internal byte[]  SECURITY_NT_AUTHORITY = {0,0,0,0,0,5} ;  // ntifs

		enum SE_OBJECT_TYPE
		{
			SE_UNKNOWN_OBJECT_TYPE = 0,
			SE_FILE_OBJECT,
			SE_SERVICE,
			SE_PRINTER,
			SE_REGISTRY_KEY,
			SE_LMSHARE,
			SE_KERNEL_OBJECT,
			SE_WINDOW_OBJECT,
			SE_DS_OBJECT,
			SE_DS_OBJECT_ALL,
			SE_PROVIDER_DEFINED_OBJECT,
			SE_WMIGUID_OBJECT,
			SE_REGISTRY_WOW64_32KEY
		}
		const int DACL_SECURITY_INFORMATION = 4;
		const int SE_SELF_RELATIVE =0x8000;

		const int  OBJECT_INHERIT_ACE   =             (0x1);
		const int  CONTAINER_INHERIT_ACE    =         (0x2);
		const int  NO_PROPAGATE_INHERIT_ACE  =        (0x4);
		const int  INHERIT_ONLY_ACE    =              (0x8);
		const int  INHERITED_ACE    =                 (0x10);
		const int  VALID_INHERIT_FLAGS  =             (0x1F);

		//-----------------------------------------------
		IntPtr psd = IntPtr.Zero;
		IntPtr pZero = IntPtr.Zero;
		IntPtr _acl = IntPtr.Zero;
		IntPtr pDacl = IntPtr.Zero;
		IntPtr _daclNew = IntPtr.Zero;

		static ACL_SIZE_INFORMATION sizeInfo;
		const int SID_MAX_SUB_AUTHORITIES  = 15;
		int _maxVers2AceSize;
		string _objectName;

		//---------------------------------------------------------------------
		public IntPtr DaclPtr	{	get	{	return pDacl;	}	}

		//---------------------------------------------------------------------
		public string ObjectName
		{
			get	{	return _objectName;		}
			set	{	_objectName = value;	}	// TODO: validity check
		}

		//---------------------------------------------------------------------
		public int Vers2AceSize	{	get	{	return _maxVers2AceSize;	}	}

		//---------------------------------------------------------------------
		// Ctor.
		public SecurityDescriptor(string objectName)
		{
			int errorReturn = GetNamedSecurityInfo(objectName, (int)SE_OBJECT_TYPE.SE_FILE_OBJECT,
				DACL_SECURITY_INFORMATION,
				out pZero, out pZero, out pDacl, out pZero, out psd);
			// Calculate Max sid size
			_maxVers2AceSize =
				Marshal.SizeOf(typeof(ACCESS_ALLOWED_ACE))  - Marshal.SizeOf(typeof(uint)) +
				(SID_MAX_SUB_AUTHORITIES * Marshal.SizeOf(typeof(uint)));
			_objectName = objectName;
		}

		//---------------------------------------------------------------------
		internal bool CommitChanges()
		{
			bool ret = false;
			const int ERROR_SUCCESS = 0;
			//TODO check valid string
			string objectName = ObjectName;
			int errorReturn = SetNamedSecurityInfo(objectName, (int)SE_OBJECT_TYPE.SE_FILE_OBJECT,
				DACL_SECURITY_INFORMATION,
				pZero,  pZero,  _daclNew, pZero);
			// Beware this API reurns 0 if success
			if(errorReturn == ERROR_SUCCESS)
				ret = true;
			Marshal.FreeHGlobal(_daclNew);
			return ret;
		}

		//---------------------------------------------------------------------
		//internal void  InsertAllowedAce(int fMask, int fInherit, string accountName)
		internal void  InsertAllowedAce(uint fMask, int fInherit, string accountName)
		{
			const uint MAXDWORD = UInt32.MaxValue;
			_acl = DaclPtr;
			IntPtr pAce;
			IntPtr _pSid = IntPtr.Zero;
			int errorReturn = 0;

			sizeInfo = new ACL_SIZE_INFORMATION();
			int AclInformationLength = Marshal.SizeOf(typeof(ACL_SIZE_INFORMATION));
			GetAclInformation(_acl,
				out sizeInfo, AclInformationLength, (int)ACL_INFORMATION_CLASS.AclSizeInformation);
			int _newDacSize = sizeInfo.AclBytesInUse + Vers2AceSize;
			//  Debug.WriteLine("New DACL size: " + _newDacSize);
			// Allocate DACL with one extra ACE
			_daclNew = Marshal.AllocHGlobal(_newDacSize);
			// Initialize the new DACL
			InitializeAcl(_daclNew, _newDacSize, ACL_REVISION);

			bool _inserted = false;
			ACE_HEADER aceh; // Needed to determine ACE size
			for (int i = 0; i < sizeInfo.AceCount; i++)
			{
				// Add Direct positive ACE right before the inherited ACEs
				errorReturn = GetAce(_acl, i, out pAce);
				if(errorReturn == 0)
					Debug.WriteLine("Error retrieving ACE");
				aceh = (ACE_HEADER)Marshal.PtrToStructure(pAce, typeof(ACE_HEADER));
				if(!_inserted && ((aceh.AceFlags & INHERITED_ACE) == INHERITED_ACE)) 
				{
					ACCESS_ALLOWED_ACE acea = (ACCESS_ALLOWED_ACE)Marshal.PtrToStructure(pAce, typeof(ACCESS_ALLOWED_ACE));
							
					bool found = false;
					if (string.Compare(accountName, "Everyone", true, System.Globalization.CultureInfo.InvariantCulture) == 0)
					{ // by fred
						_pSid = GetEveryoneSid();
						found = _pSid != IntPtr.Zero;
					}
					else
					{
						// Lookup trustee
						found = GetSidFromAccountName(accountName, ref _pSid);
					}
					if (found)
					{
						// add the SID and Mask to new ACE
						// TODO: Check return
						errorReturn = AddAccessAllowedAceEx(_daclNew, ACL_REVISION, fInherit, fMask,  _pSid);
						if(errorReturn == 0)
							Debug.WriteLine("Add error " + Marshal.GetLastWin32Error());
						else
							_inserted = true;
					}
				}
				// Add ACE to new DACL
				errorReturn = AddAce(_daclNew, ACL_REVISION, MAXDWORD, pAce, aceh.AceSize);
				if(errorReturn == 0)
					Debug.WriteLine("Add Ace error " + Marshal.GetLastWin32Error());
			}
			if(!_inserted) 
			{
				errorReturn = AddAccessAllowedAceEx(_daclNew, ACL_REVISION, fInherit, fMask,  _pSid);
				if(errorReturn == 0)
					Debug.WriteLine("Add error " + Marshal.GetLastWin32Error());
			}
			Marshal.FreeHGlobal( _pSid );
		}

		// Get the SID from the account name
		// This can be slow when trying to look up account names over the network!!!!
		//---------------------------------------------------------------------
		internal static bool GetSidFromAccountName(string accountName, ref IntPtr psid)
		{
			bool retVal = false;
			int errReturn = 0;
			const int ERROR_INSUFFICIENT_BUFFER = 122;
			int cbSid = 0;
			int cbDomain = 0;
			int use;
			// Determine buffer sizes first
			int errorReturn  = LookupAccountName( null, accountName,  IntPtr.Zero, ref cbSid,   null, ref cbDomain, out use );
			if(errorReturn == 0)
			{
				int err = Marshal.GetLastWin32Error();
				if(ERROR_INSUFFICIENT_BUFFER != err )
				{
					Debug.WriteLine( "LookupAccountName: error {0}", err.ToString(CultureInfo.InvariantCulture) );
					return retVal;
				}
			}
			psid = Marshal.AllocHGlobal( cbSid );
			StringBuilder domain = new StringBuilder( cbDomain );
			// Start look up on local machine
			errorReturn  = LookupAccountName( null, accountName, psid, ref cbSid,
				domain, ref cbDomain, out use );
			if(errReturn != 0)
				retVal = true; //Success
			return retVal;
		}

		// by fred
		//---------------------------------------------------------------------
		public static IntPtr GetEveryoneSid()
		{
			IntPtr pSid = IntPtr.Zero;
			StringBuilder stringSid = new StringBuilder("S-1-1-0"); // Everyone
			if (ConvertStringSidToSid(stringSid, out pSid) == 0)
			{
				Console.WriteLine(
					"ConvertStringSidToSid Error: {0}",
					Marshal.GetLastWin32Error());
				return IntPtr.Zero;
			}
			Debug.WriteLine("Everyone SID is: " + pSid.ToString());
			return pSid;
		}
	}

	//---------------------------------------------------------------------
	[Flags]
	public  enum AccessRights : uint
	{
		FileReadData = 0x00000001,
		FileWriteData = 0x00000002,
		FileAppendData = 0x00000004,
		FileReadEA = 0x00000008,
		FileWriteEA = 0x00000010,
		FileExecute = 0x00000020,
		FileDeleteChild = 0x00000040,
		FileReadAttributes = 0x00000080,
		FileWriteAttributes  = 0x00000100,

		Delete = 0x00010000,
		ReadControl = 0x00020000,
		WriteDac = 0x00040000,
		WriteOwner = 0x00080000,
		Synchronize = 0x00100000,
		NoStandard = 0xFFE0FFFF,

		AccesSystemSecurity = 0x01000000,
		MaximumAllowed = 0x02000000,

		GenericAll = 0x10000000,
		GenericExecute= 0x20000000,
		GenericWrite = 0x40000000,
		GenericRead = 0x80000000,
		All = 0xffffffff
	}

	//---------------------------------------------------------------------
	[Flags]
	public  enum InheritFlags : int
	{
		ObjectInheritAce = 0x1,
		ContainerInheritAce = 0x2,
		NoPropagate   =   0x4,
		InheritOnlyAce = 0x8,
		InheritedAce = 0x10,
		ValidInheritFlags   = 0x1F
	}
}
/*
class Driver 
{
	public static void Main() 
	{
		string accountName = "Testuser";
		SecurityDescriptor sd = new SecurityDescriptor(@"c:\pipo\t.txt");
		// add container inherit ace with read access rights for 'Testuser'
		// Only Direct positive ACE's in this sample
		sd.InsertAllowedAce( (int)AccessRights.FileReadData,
			(int)(InheritFlags.ContainerInheritAce | InheritFlags.NoPropagate),
			accountName);
		sd.CommitChanges();
	}
}
*/