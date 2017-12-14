using System;
using System.Diagnostics;
using System.DirectoryServices;
using System.Runtime.InteropServices;
using System.Security.Permissions;

[assembly:SecurityPermissionAttribute(SecurityAction.RequestMinimum, Flags = SecurityPermissionFlag.UnmanagedCode)]
namespace Microarea.Library.SystemServices
{
	/// <summary>
	/// Summary description for AccountManager.
	/// </summary>
	public class AccountManager
	{
		// User flags used to set user properties see ADSI doc's in MSDN for other flags
		const int UF_PASSWD_CANT_CHANGE	= 0x0040;
		const int UF_NORMAL_ACCOUNT		= 0x0200;

		//---------------------------------------------------------------------------
		public DirectoryEntry UserCreate
			(
			string user, 
			string password,
			string fullName,
			string description,
			string group
			)
		{
			return UserCreate
				(
				Environment.MachineName,
				user, 
				password,
				fullName,
				description,
				group
				);
		}

		/// <summary>
		/// Crea un utente.
		/// </summary>
		/// <remarks>Può generare eccezioni</remarks>
		//---------------------------------------------------------------------------
		public DirectoryEntry UserCreate
			(
			string machineName,
			string user, 
			string password,
			string fullName,
			string description,
			string group
			)
		{
			DirectoryEntry usr;
			DirectoryEntry comp = new DirectoryEntry("WinNT://" + machineName);
			
			// Add user using the user schema
			usr = comp.Children.Add(user, "user");
			usr.Properties["FullName"].Add(fullName);
			usr.Properties["Description"].Add(description);
			// Set user flags. sets Normal user and pwd can't change
			usr.Properties["userFlags"].Add(UF_NORMAL_ACCOUNT | UF_PASSWD_CANT_CHANGE);
			// invoke native method 'SetPassword' before commiting
			usr.Invoke("SetPassword", new Object[] {password});
			usr.CommitChanges();

			// Add user to group, if specified
			if (group != null && group.Length > 0)
			{
				DirectoryEntry grp = comp.Children.Find(group, "group");
				if (grp.Name != null)
					grp.Invoke("Add", new object[] {usr.Path.ToString()});
			}
			return usr;
		}
		
		/// <summary>
		/// Cancella un utente
		/// </summary>
		/// <param name="user">Nome Utente</param>
		//---------------------------------------------------------------------------
		public void UserDelete(string user) {UserDelete(Environment.MachineName, user);}
		public void UserDelete(string machineName, string user)
		{
			DirectoryEntry comp = new DirectoryEntry("WinNT://" + machineName);
			try
			{
				DirectoryEntry usr = comp.Children.Find(user, "user");	// might throw com exception "The user name could not be found."
				Debug.WriteLine("SchemaClassName = " + usr.SchemaClassName);
				Debug.WriteLine(usr.Properties["FullName"][0]);
				comp.Children.Remove( usr );
			}
			catch (COMException ex)
			{
				Debug.WriteLine(ex.Message);
				// Catch not found exception
			}
		}

		//---------------------------------------------------------------------------
		public bool UserExists(string user){return UserExists(Environment.MachineName, user);}
		public bool UserExists(string machineName, string user)
		{
			return GetUser(machineName, user) != null;
		}

		//---------------------------------------------------------------------------
		public DirectoryEntry GetUser(string user){return GetUser(Environment.MachineName, user);}
		public DirectoryEntry GetUser(string machineName, string user)
		{
			DirectoryEntry comp = new DirectoryEntry("WinNT://" + machineName);
			try
			{
				DirectoryEntry usr = comp.Children.Find(user, "user");	// might throw com exception "The user name could not be found."
				Debug.WriteLine("SchemaClassName = " + usr.SchemaClassName);
				Debug.WriteLine(usr.Properties["FullName"][0]);
				return usr;
			}
			catch (COMException ex)
			{
				Debug.WriteLine(ex.Message);
				// Catch not found exception
				return null;
			}
		}

		//---------------------------------------------------------------------------
		public void AddUserToGroup(DirectoryEntry usr, string machineName, string group)
		{
			if (usr == null)
				return;
			// Add user to group, if specified
			if (group != null && group.Length > 0)
			{
				try
				{
					DirectoryEntry comp = new DirectoryEntry("WinNT://" + machineName);
					DirectoryEntry grp = comp.Children.Find(group, "group");
					if (grp.Name != null)
						grp.Invoke("Add", new object[] {usr.Path.ToString()});
				}
				catch (System.Reflection.TargetInvocationException ex)
				{
					if (ex.InnerException != null && ex.InnerException is COMException)
					{
						Debug.WriteLine(ex.Message);
						// Catch "The specified account name is already a member of the local group." exception
					}
					else throw ex;
				}
			}
		}

		//---------------------------------------------------------------------------
		public DateTime GetExpireDate(string machineName, string user)
		{
			/*
			DirectoryEntry comp = new DirectoryEntry("WinNT://" + machineName);
			try
			{
				DirectoryEntry usr = comp.Children.Find(user, "user");	// might throw com exception "The user name could not be found."
				Debug.WriteLine("SchemaClassName = " + usr.SchemaClassName);
				Debug.WriteLine(usr.Properties["FullName"][0]);
				Debug.WriteLine(usr.Properties["pwdLastSet"][0]);
				//return true;
			}
			catch (COMException ex)
			{
				Debug.WriteLine(ex.Message);
				// Catch not found exception
				//return false;
			}
			*/
			throw new NotImplementedException();
		}

		//---------------------------------------------------------------------------
		public void SetExpireDate(string machineName, string user, DateTime expireTime)
		{
			throw new NotImplementedException();
		}

		//---------------------------------------------------------------------------
		public void PurgeExpireUsers(string machineName, string group)
		{
			throw new NotImplementedException();
		}

		//---------------------------------------------------------------------------
	}

	//=========================================================================
	public class SpecialUsers
	{
		public const string Everyone = "Everyone";
	}

	//=========================================================================
	public class AccountInfo
	{
		private string name;
		private string fullName;
		private string description;
		private UserFlags userFlags;

		public string Name        { get { return this.name; }}
		public string FullName    { get { return this.fullName; }}
		public string Description { get { return this.description; }}

		private AccountInfo() {} // force using factory method
		public static AccountInfo GetAccountInfo(DirectoryEntry userMetaData) // factory method
		{
			AccountInfo user = new AccountInfo();
			user.name        = userMetaData.Properties["Name"].Value as string;
			user.fullName    = userMetaData.Properties["FullName"].Value as string;
			user.description = userMetaData.Properties["Description"].Value as string;
			user.userFlags   = (UserFlags)userMetaData.Properties["UserFlags"].Value;
			return user;
		}

		public bool IsUserLocked { get { return (this.userFlags & UserFlags.LOCKOUT) == UserFlags.LOCKOUT; }}

		public static void UnlockUser(DirectoryEntry userMetaData) // ugly, it just works
		{
			UserFlags userFlags = (UserFlags)userMetaData.Properties["UserFlags"].Value;
			UserFlags newFlags = userFlags & ~UserFlags.LOCKOUT;
			userMetaData.Properties["UserFlags"][0] = newFlags;
			userMetaData.CommitChanges();
		}

		[FlagsAttribute]
		enum UserFlags //ADS_USER_FLAG_ENUM definition from MSDN docs
		{
			SCRIPT									= 1,		// 0x1			// The logon script is executed. This flag does not work for the ADSI LDAP provider on either read or write operations. For the  ADSI WinNT provider, this flag is  read-only data, and it cannot be set for user objects. 
			ACCOUNTDISABLE							= 2,		// 0x2			// The user account is disabled. 
			HOMEDIR_REQUIRED						= 8,		// 0x8,			// The home directory is required. 
			LOCKOUT									= 16,		// 0x10,		// The account is currently locked out. 
			PASSWD_NOTREQD							= 32,		// 0x20,		// No password is required. 
			PASSWD_CANT_CHANGE						= 64,		// 0x40,		// The user cannot change the password. This flag can be read, but not set directly.  For more information and a code example that shows how to prevent a user from changing the password, see User Cannot Change Password. 
			ENCRYPTED_TEXT_PASSWORD_ALLOWED			= 128,		// 0x80,		// The user can send an encrypted password. 
			TEMP_DUPLICATE_ACCOUNT					= 256,		// 0x100,		// This is an account for users whose primary account is in another domain. This account provides user access to this domain, but not to any domain that trusts this domain. Also known as a  local user account. 
			NORMAL_ACCOUNT							= 512,		// 0x200,		// This is a default account type that represents a typical user. 
			INTERDOMAIN_TRUST_ACCOUNT				= 2048,		// 0x800,		// This is a permit to trust account for a system domain that trusts other domains. 
			WORKSTATION_TRUST_ACCOUNT				= 4096,		// 0x1000,		// This is a computer account for a Microsoft Windows NT Workstation/Windows 2000 Professional or Windows NT Server/Windows 2000 Server that is a member of this domain. 
			SERVER_TRUST_ACCOUNT					= 8192,		// 0x2000,		// This is a computer account for a system backup domain controller that is a member of this domain. 
			DONT_EXPIRE_PASSWD						= 65536,	// 0x10000,		// When set, the password will not expire on this account. 
			MNS_LOGON_ACCOUNT						= 131072,	// 0x20000,		// This is an Majority Node Set (MNS) logon account. With MNS, you can configure a multi-node Windows cluster without using a common shared disk. 
			SMARTCARD_REQUIRED						= 262144,	// 0x40000,		// When set, this flag will force the user to log on using a smart card. 
			TRUSTED_FOR_DELEGATION					= 524288,	// 0x80000,		// When set, the service account (user or computer account), under which a service runs, is trusted for Kerberos delegation. Any such service can impersonate a client requesting the service. To enable a service for Kerberos delegation, set this flag on the  userAccountControl property of the service account. 
			NOT_DELEGATED							= 1048576,	// 0x100000,	// When set, the security context of the user will not be delegated to a service even if the service account is set as trusted for Kerberos delegation. 
			USE_DES_KEY_ONLY						= 2097152,	// 0x200000,	// Restrict this principal to use only Data Encryption Standard (DES) encryption types for keys.Active Directory Client Extension:  Not supported. 
			DONT_REQUIRE_PREAUTH					= 4194304,	// 0x400000,	// This account does not require Kerberos preauthentication for logon.Active Directory Client Extension:  Not supported.
			PASSWORD_EXPIRED						= 8388608,	// 0x800000,	// The user password has expired. This flag is created by the system using data from the  password last set attribute and the domain policy.  It is read-only and cannot be set. To manually set a user password as expired, use the NetUserSetInfo function with the USER_INFO_3 (usri3_password_expired member) or USER_INFO_4 (usri4_password_expired member) structure.Active Directory Client Extension:  Not supported.
			TRUSTED_TO_AUTHENTICATE_FOR_DELEGATION	= 16777216	// 0x1000000,	// The account is enabled for delegation. This is a security-sensitive setting; accounts with this option enabled should be strictly controlled. This setting enables a service running under the account to assume a client identity and authenticate as that user to other remote servers on the network.Active Directory Client Extension:  Not supported. 
		}
	}

	//=========================================================================
}
