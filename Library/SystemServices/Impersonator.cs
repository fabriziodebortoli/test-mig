using System;
using System.Diagnostics;
using System.Globalization;
using System.Security.Principal;
using System.Runtime.InteropServices;

namespace Microarea.Library.SystemServices
{
	// TODO :
	//		*	Capire quali attributi SecurityPermissionAttribute e PermissionSetAttribute
	//			sia meglio usare. Nel frattempo nel dubbio non ne uso nessuno
	//		*	Utilizzare FormatMessage() per reperire il testo dell'errore in base
	//			al suo HRESULT. al momento l'ho commentato perché sembra si debba usare
	//			dichiarandolo unsafe e non sono sicuro di volerlo fare.
	//		*	Partendo da utente locale ASPNET l'impersonation è fallita usando dei
	//			logonType LOGON32_LOGON_BATCH (LogonUser failed with error code : 1385)
	//			e LOGON32_LOGON_NEW_CREDENTIALS (LogonUser failed with error code : 1367)
	//			in un sistema W2K-Pro. Non ho idea se sia da considerare errore o no.

	/// <summary>
	/// La classe Impersonator vuole fornire uno strumento univoco per i casi i
	/// casi in cui è necessario impersonificare un'altro utente.
	/// Usando il logonType opportuno è possibile accedere anche a risorse di rete
	/// </summary>
	/// <remarks>
	/// Federico:
	/// Il codice è una rielaborazione degli esempi presenti su MSDN, cui sono
	/// state apportate alcune correzioni, in parte reperite sulla KB, in parte
	/// sui NG di MS; nel codice sono riportati i riferimenti.
	/// Nell'implentazione ho usato il pattern disposable per invitare i programmatori
	/// a fare il rilascio esplicito delle risorse unmanaged tramite un'interfaccia standard.
	/// </remarks>
	/// <example>
	///		using (Impersonator imp = new Impersonator(domain, user, password))
	///		{
	///			if (NeedToImpersonate())
	///				imp.Impersonate(Impersonator.LogonType.LOGON32_LOGON_NETWORK_CLEARTEXT);
	///			//... code running as impersonated user here...
	///		}
	/// </example>
	/// <example>
	///		Impersonator imp = null;		
	///		try
	///		{
	///			if (NeedToImpersonate())
	///			{
	///				imp = new Impersonator(domain, user, password))
	///				imp.Impersonate();
	///				//... code running as impersonated userhere...
	///			}
	///		}
	///		finally
	///		{
	///			if (imp != null)
	///				imp.Dispose();
	///			imp = null;
	///		}
	/// </example>
	//=========================================================================
	//[assembly:SecurityPermissionAttribute(SecurityAction.RequestMinimum, UnmanagedCode=true)]
	//[assembly:PermissionSetAttribute(SecurityAction.RequestMinimum, Name = "FullTrust")]
	public sealed class Impersonator : IDisposable
	{
		#region extern methods

		/// <summary>
		/// The LogonUser function attempts to log a user on to the local computer
		/// </summary>
		//---------------------------------------------------------------------
		[DllImport("advapi32.dll", SetLastError=true)]
		private extern static bool LogonUser
			(
			string lpszUsername, 
			string lpszDomain, 
			string lpszPassword, 
			int dwLogonType, 
			int dwLogonProvider, 
			ref IntPtr phToken
			);
    
		//---------------------------------------------------------------------
		[DllImport("kernel32.dll", CharSet=CharSet.Auto)]
		private extern static bool CloseHandle(IntPtr handle);

		//---------------------------------------------------------------------
		/// <summary>
		/// The DuplicateToken function creates a new access token that duplicates one already in existence
		/// </summary>
		[DllImport("advapi32.dll", CharSet=CharSet.Auto, SetLastError=true)]
		private extern static bool DuplicateToken
			(
			IntPtr ExistingTokenHandle, 
			int securityImpersonationLevel, 
			ref IntPtr DuplicateTokenHandle
			);

		/*	commentato perché unsafe, al momento preferisco non usarlo
		/// <summary>
		/// The GetErrorMessageText method retrieves the error message text for an HRESULT error code.
		/// If the error message text is localized, it has been localized on the client.
		/// </summary>
		//---------------------------------------------------------------------
		[DllImport("kernel32.dll", CharSet=CharSet.Auto)]
		private unsafe static extern int FormatMessage
			(
			int dwFlags, 
			ref IntPtr lpSource, 
			int dwMessageId, 
			int dwLanguageId, 
			ref String lpBuffer, 
			int nSize, 
			IntPtr *Arguments
			);
		*/

		#endregion

		#region Enums
		/// <remarks>
		/// Da prove sperimentali l'accesso a file in share di rete è stato
		/// possibile usando i seguenti logonType (partendo da utente ASPNET):
		/// LOGON32_LOGON_INTERACTIVE
		/// LOGON32_LOGON_UNLOCK
		/// LOGON32_LOGON_NETWORK_CLEARTEXT
		/// mentre LOGON32_LOGON_SERVICE ha funzionato se l'utente (local admin)
		/// aveva il profilo utente caricato ed ha fallito altrimenti
		/// gli altri valori non hanno permesso l'accesso a risorse di rete.
		/// Riferirsi alla documentazione MSDN per la scelta del valore opportuno
		/// </remarks>
		//---------------------------------------------------------------------
		public enum LogonType : int
		{
			// LogonUser returns a Direct token for most of the logon types,
			// except LOGON32_LOGON_NETWORK, LOGON32_LOGON_NETWORK_CLEARPASSWORD
			// which do return an impersonation token.
			LOGON32_LOGON_INTERACTIVE		= 2,	// This parameter causes LogonUser to create a primary token.
			LOGON32_LOGON_NETWORK			= 3,	// does not yield Network access
			LOGON32_LOGON_BATCH				= 4,
			LOGON32_LOGON_SERVICE			= 5,
			LOGON32_LOGON_UNLOCK			= 7,
			LOGON32_LOGON_NETWORK_CLEARTEXT	= 8,	// Only for Win2K or higher
			LOGON32_LOGON_NEW_CREDENTIALS	= 9		// Only for Win2K or higher
		}

		//---------------------------------------------------------------------
		private enum LogonProvider : int
		{
			LOGON32_PROVIDER_DEFAULT = 0,
			LOGON32_PROVIDER_WINNT35 = 1,
			LOGON32_PROVIDER_WINNT40 = 2,
			LOGON32_PROVIDER_WINNT50 = 3
		};

		//---------------------------------------------------------------------
		private enum SecurityImpersonationLevel
		{
			SecurityAnonymous		= 0,
			SecurityIdentification	= 1,
			SecurityImpersonation	= 2,	// minimo per impersonificazione
			SecurityDelegation		= 3		// impersonifica anche su sistemi remoti (NT non lo supporta)
		}
		#endregion

		//---------------------------------------------------------------------
		private readonly string domainName;
		private readonly string userName;
		private readonly string password;

		private IntPtr tokenHandle		= IntPtr.Zero;
		private IntPtr dupeTokenHandle	= IntPtr.Zero;
		private WindowsImpersonationContext impersonatedUser = null;

		/// <summary>
		/// crea l'istanza dell'oggetto inizializzando i tre campi come da parametri,
		/// ma non compie alcuna azione particolare.
		/// </summary>
		/// <param name="domainName"></param>
		/// <param name="userName"></param>
		/// <param name="password"></param>
		//---------------------------------------------------------------------
		public Impersonator(string domainName, string userName, string password)
		{
			this.domainName	= domainName;
			this.userName	= userName;
			this.password	= password;
		}

		//---------------------------------------------------------------------
		public void Impersonate()
		{
			Impersonate(LogonType.LOGON32_LOGON_INTERACTIVE);
		}

		private string output = string.Empty;
		public string Output { get { return this.output; } }

		//---------------------------------------------------------------------
		/// <summary>
		/// Impersonates required user
		/// </summary>
		/// <remarks>Might throw exceptions</remarks>
		/// <param name="logonType">logon type</param>
		/// <returns>true if impersonation succeeds</returns>
		public bool Impersonate(LogonType logonType)
		{
			output = string.Empty;

			tokenHandle		= IntPtr.Zero;	// The Windows NT user token.
			dupeTokenHandle	= IntPtr.Zero;

			// Call LogonUser to obtain an handle to an access token.
			bool logged = LogonUser
				(
				userName, 
				domainName, 
				password, 
				(int)logonType, 
				(int)LogonProvider.LOGON32_PROVIDER_DEFAULT,
				ref tokenHandle
				);
        
			if (!logged)
			{
				int ret = Marshal.GetLastWin32Error();
				output += string.Format(CultureInfo.InvariantCulture, "LogonUser failed with error code : {0}", ret) + Environment.NewLine;
				Debug.WriteLine(string.Format(CultureInfo.InvariantCulture, "LogonUser failed with error code : {0}", ret));
				//string txtError = GetErrorMessage(ret);
				//Debug.WriteLine(string.Format("\nError: [{0}] {1}\n", ret, txtError));
				return false;
			}

			// NOTE - The LogonUser API has been available and documented since Windows NT 3.51,
			//		and is commonly used to verify user credentials
			//
			// NOTE - GetLastError(), invocata dall'utente ASPNET su Win2000, ritorna:
			//		1314 ERROR_PRIVILEGE_NOT_HELD "A required privilege is not held by the client."
			//		Risolto con:
			//		dato previlegio "Act as part of the operating system."
			//		all'utente locale ASPNET
			//
			// NOTE:
			//		Windows NT and Windows 2000, the process that is calling LogonUser 
			//		must have the SE_TCB_NAME privilege (in User Manager, this is the 
			//		"Act as part of the Operating System" right).
			//		da Windows XP in poi tale previlegio non è più necessario
			//		per invocare LogonUser()

			// Check the identity.
			Debug.WriteLine("Before impersonation: " + WindowsIdentity.GetCurrent().Name);
			output += "Before impersonation: " + WindowsIdentity.GetCurrent().Name + Environment.NewLine;

			// NOTE:
			// LogonUser returns a Direct token for most of the logon types,
			// except LOGON32_LOGON_NETWORK, LOGON32_LOGON_NETWORK_CLEARTEXT
			// which do return an impersonation token.
			// ref: http://groups.google.com/groups?hl=it&lr=&ie=UTF-8&threadm=eCBMzwN%24BHA.2040%40tkmsftngp05&rnum=4&prev=/groups%3Fas_q%3DLogonUser%2520impersonate%26ie%3DUTF-8%26as_ugroup%3Dmicrosoft.public.dotnet*%26lr%3D%26num%3D50%26hl%3Dit
			// NOTE:
			// You must have an impersonation token for the WindowsIdentity.Impersonate method to work.
			// To obtain an impersonation token from a primary token, use the DuplicateToken Win32 function
			switch (logonType)
			{
				case LogonType.LOGON32_LOGON_NETWORK :
				case LogonType.LOGON32_LOGON_NETWORK_CLEARTEXT :
					dupeTokenHandle = new IntPtr((int)tokenHandle);
					break;
				default :
					bool retVal = DuplicateToken(tokenHandle, (int)SecurityImpersonationLevel.SecurityImpersonation, ref dupeTokenHandle);
					if (false == retVal)
					{
						CloseHandle(tokenHandle);
						Debug.WriteLine("Exception in token duplication.");
						output += "Exception in token duplication." + Environment.NewLine;
						return false;
					}
					break;
			}
    
			// l'articolo Nr 319615
			// http://support.microsoft.com/default.aspx?scid=kb%3ben-us%3b319615
			// suggerisce di usare una chiamata API a DuplicateToken()
			// (credo che nel testo faccia un po' di confusione tra primary token e impersonation token)
			// Credo che il problema indicato sia dovuto al fatto che il logonType è interactive.
			// soluzioni alternative posso essere:
			//	* invocare DuplicateToken()
			//	* usare un logonType di tipo network
			//	* invocare l'api ImpersonateLoggedOnUser
			// ref:
			// http://groups.google.com/groups?hl=it&lr=&ie=UTF-8&threadm=eec%24gW2FCHA.1692%40tkmsftngp05&rnum=20&prev=/groups%3Fas_q%3DLogonUser%2520impersonate%26ie%3DUTF-8%26as_ugroup%3Dmicrosoft.public.dotnet*%26lr%3D%26num%3D50%26hl%3Dit
			// NOTA: la documentazione MSDN di Impersonate() è stata aggiornata per usare DuplicateToken(),
			//		facendo l'esempio di un logon type che genera un primary token

			// You must have an impersonation token for the WindowsIdentity.Impersonate method to work
			WindowsIdentity newId = new WindowsIdentity(dupeTokenHandle);
			impersonatedUser = newId.Impersonate();

			// Check the identity.
			Debug.WriteLine("After impersonation: " + WindowsIdentity.GetCurrent().Name);
			output += "After impersonation: " + WindowsIdentity.GetCurrent().Name + Environment.NewLine;
			
			return true;
		}

		#region IDisposable Members, Close method, cleanup and finalizer

		//---------------------------------------------------------------------
		public void Close()
		{
			Dispose();
		}

		//---------------------------------------------------------------------
		public void Dispose()
		{
			CleanUp();
			GC.SuppressFinalize(this);
		}
		
		//---------------------------------------------------------------------
		~Impersonator()
		{
			CleanUp();
		}
		
		//---------------------------------------------------------------------
		private void CleanUp()
		{
			if (impersonatedUser != null)
			{
				impersonatedUser.Undo();	// Stop impersonating.
				impersonatedUser = null;
			}

			// Check the identity.
			Debug.WriteLine("After Undo: " + WindowsIdentity.GetCurrent().Name);

			// Free the tokens.
			if (tokenHandle != IntPtr.Zero)
				CloseHandle(tokenHandle);
			if (dupeTokenHandle != IntPtr.Zero) 
				CloseHandle(dupeTokenHandle);
		}

		#endregion
	}

	//---------------------------------------------------------------------
}
