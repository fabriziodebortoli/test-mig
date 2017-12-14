using System;
using System.Diagnostics;
using System.Runtime.InteropServices;
using System.Security.Principal;
using Microarea.TaskBuilderNet.Core.DiagnosticManager;
using Microarea.TaskBuilderNet.Interfaces;

namespace Microarea.TaskBuilderNet.Data.DatabaseLayer
{
	/// <summary>
	/// LogonUserNS
	/// (Vedi articolo N.319615 "Unable to Impersonate User" Error Message when you use WindowsIdentity.Impersonate Method
	/// nella Knowledge Base Inglese http://support.microsoft.com/default.aspx?scid=kb%3ben-us%3b319615 )
	/// </summary>
	// ========================================================================
	public class LogonUserNS
	{
		#region Costanti
		//---------------------------------------------------------------------
		const int LOGON32_PROVIDER_DEFAULT			= 0;
		//This parameter causes LogonUser to create a primary token.
		const int LOGON32_LOGON_INTERACTIVE			= 2;
		const int LOGON32_LOGON_NETWORK_CLEARTEXT	= 3;
		const int SecurityImpersonation				= 2;
		#endregion

		#region Variabili Private
		//---------------------------------------------------------------------
		private WindowsPrincipal			currentUserNT	 = new WindowsPrincipal(WindowsIdentity.GetCurrent());
		private WindowsImpersonationContext impersonatedUser;
		private IntPtr						tokenHandle		 = new IntPtr(0);
		private IntPtr						dupeTokenHandle	 = new IntPtr(0);

		private string	login			= string.Empty;
		private string	password		= string.Empty;
		private string	domain			= string.Empty;

		private Diagnostic diagnostic = new Diagnostic("LogonUserNS");
		#endregion

		#region Proprietà
		//---------------------------------------------------------------------
		public string Login				{ get { return login; } set { login = value;} }
		public string Password			{ get { return password; } set { password = value;} }
		public string Domain			{ get { return domain; } set { domain = value;} }
		public IntPtr TokenHandle		{ get { return tokenHandle; } set { tokenHandle = value;} }
		public IntPtr DupeTokenHandle	{ get { return dupeTokenHandle; } set { dupeTokenHandle	 = value;} }
		public string CurrentUser		{ get {return WindowsIdentity.GetCurrent().Name;} }

		public WindowsImpersonationContext ImpersonatedUser { get { return impersonatedUser; } set { impersonatedUser = value; } }

		public Diagnostic Diagnostic { get { return diagnostic; } }
		#endregion

		#region DllImport da advapi32.dll, kernel32.dll
		//---------------------------------------------------------------------
		[DllImport("advapi32.dll", SetLastError=true)]
		public extern static bool LogonUser(String lpszUsername, String lpszDomain, String lpszPassword, 
			int dwLogonType, int dwLogonProvider, ref IntPtr phToken);
		//---------------------------------------------------------------------
		[DllImport("kernel32.dll", CharSet=CharSet.Auto)]
		public extern static bool CloseHandle(IntPtr handle);
		//---------------------------------------------------------------------
		[DllImport("advapi32.dll", CharSet=CharSet.Auto, SetLastError=true)]
		public extern static bool DuplicateToken(IntPtr ExistingTokenHandle, 
			int SECURITY_IMPERSONATION_LEVEL, ref IntPtr DuplicateTokenHandle);
		//---------------------------------------------------------------------
		[DllImport("Kernel32.dll", CharSet=CharSet.Auto, SetLastError=true)]
		public static extern int GetLastError();
		#endregion

		#region Costruttore - Esegue l'impersonificazione
		/// <summary>
		/// Costruttore
		/// </summary>
		//---------------------------------------------------------------------
		public LogonUserNS(string login, string password, string domain)
		{
			tokenHandle		= IntPtr.Zero;
			dupeTokenHandle = IntPtr.Zero;
			Login			= login;
			Password		= password;
			Domain			= domain;

			if (LogonUserAPI()) 
				StartImpersonification();
			else
				diagnostic.Set(DiagnosticType.Error, DatabaseLayerStrings.WrongImpersonation);
		}
		#endregion

		#region LogonUserAPI - Chiamo la LogonUser e mi loggo su NT con le nuove credenziali
		/// <summary>
		/// LogonUserAPI
		/// Chiamo la LogonUser e mi loggo su NT con le nuove credenziali
		/// </summary>
		//---------------------------------------------------------------------
		public bool LogonUserAPI()
		{
			bool successImpersonation = false;
			
			try
			{
				successImpersonation = LogonUser
					(
						Login, 
						Domain, 
						Password, 
						LOGON32_LOGON_INTERACTIVE, 
						LOGON32_PROVIDER_DEFAULT,
						ref tokenHandle
					);
			}
			catch(Exception ex)
			{
				Debug.Fail(ex.Message);
				ExtendedInfo extendedInfo = new ExtendedInfo();
				extendedInfo.Add(DatabaseLayerStrings.Description, ex.Message);
				extendedInfo.Add(DatabaseLayerStrings.StackTrace, ex.StackTrace);
				extendedInfo.Add(DatabaseLayerStrings.Source, ex.Source);
				extendedInfo.Add(DatabaseLayerStrings.Procedure, "LogonUserAPI");
				extendedInfo.Add(DatabaseLayerStrings.Library, "SQLDataAccess");
				diagnostic.Set(DiagnosticType.Error, DatabaseLayerStrings.WrongImpersonation, extendedInfo);
			}

			int ret = GetLastError();

			if (!successImpersonation)
			{
				ExtendedInfo extendedInfo = new ExtendedInfo();
				extendedInfo.Add(DatabaseLayerStrings.Description, ret);
				diagnostic.Set(DiagnosticType.Error, DatabaseLayerStrings.WrongCredential, extendedInfo);
			}

			return successImpersonation;
		}
		#endregion
		
		#region StartImpersonification - Esegue Impersonificazione con le nuove credenziali
		/// <summary>
		/// StartImpersonification
		/// Esegue Impersonificazione con le nuove credenziali
		/// Da Notare, la chiamata alla DuplicateToken, senza la quale
		/// si avrebbe una exception
		/// (Vedi articolo N.319615 "Unable to Impersonate User" Error Message when you use WindowsIdentity.Impersonate Method
		/// nella Knowledge Base Inglese http://support.microsoft.com/default.aspx?scid=kb%3ben-us%3b319615 )
		/// </summary>
		//---------------------------------------------------------------------
		public bool StartImpersonification()
		{
			bool retVal = false;
			
			try
			{
				retVal = DuplicateToken(tokenHandle, SecurityImpersonation, ref dupeTokenHandle);
				
				if (!retVal)
				{
					CloseHandle(tokenHandle);
					ExtendedInfo extendedInfo = new ExtendedInfo();
					extendedInfo.Add(DatabaseLayerStrings.Description, retVal);
					diagnostic.Set(DiagnosticType.Error, DatabaseLayerStrings.WrongImpersonation, extendedInfo);
					return retVal;
				}
				
				WindowsIdentity newId	= new WindowsIdentity(dupeTokenHandle);
				impersonatedUser		= newId.Impersonate();
			}
			catch(Exception ex)
			{
				ExtendedInfo extendedInfo = new ExtendedInfo();
				extendedInfo.Add(DatabaseLayerStrings.Description, ex.Message);
				extendedInfo.Add(DatabaseLayerStrings.StackTrace, ex.StackTrace);
				extendedInfo.Add(DatabaseLayerStrings.Source, ex.Source);
				extendedInfo.Add(DatabaseLayerStrings.Procedure, "DuplicateToken");
				extendedInfo.Add(DatabaseLayerStrings.Procedure, "StartImpersonification");
				extendedInfo.Add(DatabaseLayerStrings.Library, "SQLDataAccess");
				diagnostic.Set(DiagnosticType.Error, DatabaseLayerStrings.WrongImpersonation, extendedInfo);
				retVal = false;
			}

			return retVal;
		}
		#endregion

		#region StopImpersonification - Termina l'impersonificazione
		/// <summary>
		/// StopImpersonification
		/// Interrompe l'impersonificazione
		/// </summary>
		//---------------------------------------------------------------------
		public void StopImpersonification()
		{
			impersonatedUser.Undo();
			if (TokenHandle != IntPtr.Zero)
				CloseHandle(TokenHandle);
			if (DupeTokenHandle != IntPtr.Zero) 
				CloseHandle(DupeTokenHandle);
		}
		#endregion
	}

	#region UserImpersonatedData (for SQL users)
	/// <summary>
	/// UserImpersonificatedData
	/// Dopo che l'utente è stato impersonificato questi sono i suoi dati
	/// </summary>
	//=========================================================================
	public class UserImpersonatedData
	{
		//---------------------------------------------------------------------
		private string	login					= string.Empty;
		private string	password				= string.Empty;
		private string	domain					= string.Empty;
		private string	userBeforeImpersonate	= string.Empty;
		private bool	windowsAuthentication	= false;
		
		private WindowsImpersonationContext userAfterImpersonate = null; 

		//---------------------------------------------------------------------
		public string Login		 { get { return login; } set { login = value;} }
		public string Password	 { get { return password; } set { password = value;} }
		public string Domain	 { get { return domain; } set { domain = value;} }
		public bool	 WindowsAuthentication { get { return windowsAuthentication; } set { windowsAuthentication = value;} }
		public string UserBeforeImpersonate { get {return userBeforeImpersonate;  } set { userBeforeImpersonate = value;} }
		
		public WindowsImpersonationContext UserAfterImpersonate	 { get {return userAfterImpersonate;   } set { userAfterImpersonate  = value;} }

		//---------------------------------------------------------------------
		public UserImpersonatedData()
		{
		}

		/// <summary>
		/// Undo - Termina l'impersonificazione
		/// </summary>
		//---------------------------------------------------------------------
		public void Undo()
		{
			if (userAfterImpersonate != null)
				userAfterImpersonate.Undo();
		}
	}
	#endregion


	#region OracleUserImpersonatedData (for Oracle users)
	//=========================================================================
	public class OracleUserImpersonatedData
	{
		//---------------------------------------------------------------------
		private string login = string.Empty;
		private string password = string.Empty;
		private string domain = string.Empty;
		private string oracleService = string.Empty;
		private bool isDba = false;
		private bool windowsAuthentication = false;
		private bool isCurrentUser = false;
		private string language = string.Empty;
		private string territory = string.Empty;
		private string userBeforeImpersonate = string.Empty;

		private WindowsImpersonationContext userAfterImpersonate = null;

		//---------------------------------------------------------------------
		public string Login { get { return login; } set { login = value; } }
		public string Password { get { return password; } set { password = value; } }
		public string Domain { get { return domain; } set { domain = value; } }
		public string OracleService { get { return oracleService; } set { oracleService = value; } }
		public bool IsDba { get { return isDba; } set { isDba = value; } }
		public bool WindowsAuthentication { get { return windowsAuthentication; } set { windowsAuthentication = value; } }
		public bool IsCurrentUser { get { return isCurrentUser; } set { isCurrentUser = value; } }
		public string Language { get { return language; } set { language = value; } }
		public string Territory { get { return territory; } set { territory = value; } }
		public string UserBeforeImpersonate { get { return userBeforeImpersonate; } set { userBeforeImpersonate = value; } }
		public WindowsImpersonationContext UserAfterImpersonate { get { return userAfterImpersonate; } set { userAfterImpersonate = value; } }

		//---------------------------------------------------------------------
		public OracleUserImpersonatedData()
		{
		}

		/// <summary>
		/// Undo - Termina l'impersonificazione
		/// </summary>
		//---------------------------------------------------------------------
		public void Undo()
		{
			if (userAfterImpersonate != null)
				userAfterImpersonate.Undo();
		}
	}
	#endregion

    #region PostgreImpersonatedData (for Postgre users)
    /// <summary>
    /// UserImpersonificatedData
    /// Dopo che l'utente è stato impersonificato questi sono i suoi dati
    /// </summary>
    //=========================================================================
    public class PostgreImpersonatedData
    {
        //---------------------------------------------------------------------
        private string login = string.Empty;
        private string password = string.Empty;
        private string domain = string.Empty;
        private string userBeforeImpersonate = string.Empty;
        private bool windowsAuthentication = false;

        private WindowsImpersonationContext userAfterImpersonate = null;

        //---------------------------------------------------------------------
        public string Login { get { return login; } set { login = value; } }
        public string Password { get { return password; } set { password = value; } }
        public string Domain { get { return domain; } set { domain = value; } }
        public bool WindowsAuthentication { get { return windowsAuthentication; } set { windowsAuthentication = value; } }
        public string UserBeforeImpersonate { get { return userBeforeImpersonate; } set { userBeforeImpersonate = value; } }

        public WindowsImpersonationContext UserAfterImpersonate { get { return userAfterImpersonate; } set { userAfterImpersonate = value; } }

        //---------------------------------------------------------------------
        public PostgreImpersonatedData()
        {
        }

        /// <summary>
        /// Undo - Termina l'impersonificazione
        /// </summary>
        //---------------------------------------------------------------------
        public void Undo()
        {
            if (userAfterImpersonate != null)
                userAfterImpersonate.Undo();
        }
    }
    #endregion
}