using System;
using System.Collections.Generic;
using System.Globalization;
using System.ServiceProcess;
using System.Windows.Forms;
using Microarea.Console.Core.PlugIns;
using Microarea.TaskBuilderNet.Core.DiagnosticManager;
using Microarea.TaskBuilderNet.Data.DatabaseLayer;
using Microarea.TaskBuilderNet.Interfaces;

namespace Microarea.Console
{
	# region class ConsoleStatus
	/// <summary>
	/// ConsoleStatus
	/// Tiene traccia dello stato della console (impostazioni di layout e visualizzazione degli oggetti)
	/// </summary>
	//=========================================================================
	public class ConsoleStatus
	{
		#region Variabili private
		/// <summary>
		/// Inizializzo tutto a true e metto Detail come View nella lista
		/// </summary>
		private bool isVisibleConsoleTree			= true;
		private bool isVisibleStandardMenuConsole	= true;
		private bool isVisibleStandardToolbarConsole= true;
		private bool isVisibleStatusBarConsole		= true;
		private bool isVisibleMenuPlugIn			= true;

		private bool isVisibleLoginAdvancedOptions	= false;

		private StatusType		status							= StatusType.None;
		private List<string>	plugInsAlsoToLoad				= new List<string>();
		private List<AuthenticatedUser> autenticatedUsers		= new List<AuthenticatedUser>();
		private View			defaultView						= View.Details;
		private Diagnostic		diagnostic						= new Diagnostic("MicroareaConsole");
		private LicenceInfo     licenceInformation				= new LicenceInfo();
		private string			language						= string.Empty;
		private string			localCulture					= string.Empty;
		private List<DatabaseService> sqlServiceDatabase		= new List<DatabaseService>();
		#endregion

		#region Proprietà e Metodi
		//---------------------------------------------------------------------
		public bool	IsVisibleConsoleTree			{ get { return isVisibleConsoleTree;			} set { isVisibleConsoleTree			= value; }}
		public bool	IsVisibleStandardMenuConsole	{ get { return isVisibleStandardMenuConsole;	} set { isVisibleStandardMenuConsole	= value; }}
		public bool	IsVisibleStandardToolbarConsole	{ get { return isVisibleStandardToolbarConsole; } set { isVisibleStandardToolbarConsole = value; }}
		public bool	IsVisibleStatusBarConsole		{ get { return isVisibleStatusBarConsole;		} set { isVisibleStatusBarConsole		= value; }}
		public bool	IsVisibleMenuPlugIn				{ get { return isVisibleMenuPlugIn;				} set { isVisibleMenuPlugIn				= value; }}
		public bool IsVisibleLoginAdvancedOptions	{ get { return isVisibleLoginAdvancedOptions;	} set { isVisibleLoginAdvancedOptions	= value; } }
		
		public StatusType		Status					        { get { return status;                          } set { status                          = value; }}
		public List<AuthenticatedUser> AutenticatedUsers		{ get { return autenticatedUsers; }				set { autenticatedUsers = value; } }
		public List<string>		PlugInsAlsoToLoad				{ get { return plugInsAlsoToLoad; }				set { plugInsAlsoToLoad = value; } }
		public View				DefaultView						{ get { return defaultView;						} set { defaultView						= value; }}
		public LicenceInfo      LicenceInformation              { get { return licenceInformation;				} set { licenceInformation				= value; }}
		public Diagnostic		Diagnostic						{ get { return diagnostic; }}
		public string			Language						{ get { return language;						} set { language						= value; }}
		public string			LocalCulture					{ get { return localCulture;					} set { localCulture					= value; }}
		public List<DatabaseService> SqlServiceDatabase			{ get { return sqlServiceDatabase;				} set { sqlServiceDatabase				= value; }}
		#endregion

		#region Costruttore (vuoto)
		/// <summary>
		/// Costruttore
		/// </summary>
		//---------------------------------------------------------------------
		public ConsoleStatus()
		{
		}
		#endregion

		#region GetAuthenticatedUserPwd - Legge la password dell'utente autenticato
		/// <summary>
		/// GetAuthenticatedUserPwd
		/// </summary>
		//---------------------------------------------------------------------
		public string GetAuthenticatedUserPwd(string login, string serverName)
		{
			string passwordAuthenticated = string.Empty;

			IEnumerator<AuthenticatedUser> myEnumerator = AutenticatedUsers.GetEnumerator();

			while (myEnumerator.MoveNext())
			{
				AuthenticatedUser currentUser = (AuthenticatedUser)myEnumerator.Current;
				
				// Ho trovato la login
				if (string.Compare(currentUser.LoginName, login, StringComparison.InvariantCultureIgnoreCase) == 0)
				{
					if (string.Compare(currentUser.Server, serverName, StringComparison.InvariantCultureIgnoreCase) == 0)
					{
						passwordAuthenticated = currentUser.Password;
						break;
					}
				}
			}

			return passwordAuthenticated;
		}
		#endregion

		#region AddAuthenticatedUser - Aggiunge l'utente all'array
		/// <summary>
		/// AddAuthenticatedUser
		/// </summary>
		//---------------------------------------------------------------------
		public void AddAuthenticatedUser(string login, string password, string serverName, DBMSType dbType)
		{
			if (!IsUserAuthenticated(login, password, serverName))
			{
				AuthenticatedUser newAuthenticatedUser = new AuthenticatedUser();

				newAuthenticatedUser.LoginName = login;
				newAuthenticatedUser.Password = password;
				newAuthenticatedUser.Server	= serverName;
				newAuthenticatedUser.ProviderType = dbType; 
				newAuthenticatedUser.LoginDateTime = DateTime.Now;
				
				// lo aggiungo alla lista degli utenti autenticati
				AutenticatedUsers.Add(newAuthenticatedUser);
			}
		}
		#endregion

		#region IsUserAuthenticated - Cerca nell'array se l'utente è autenticato oppure no
		/// <summary>
		/// IsUserAuthenticated
		/// </summary>
		//---------------------------------------------------------------------
		public bool IsUserAuthenticated(string login, string password, string serverName)
		{
			bool isAuthenticated = false;
			IEnumerator<AuthenticatedUser> myEnumerator = AutenticatedUsers.GetEnumerator();
			
			while (myEnumerator.MoveNext())
			{
				AuthenticatedUser currentUser = (AuthenticatedUser)myEnumerator.Current;
				
				// Ho trovato la login
				if (string.Compare(currentUser.LoginName, login, StringComparison.InvariantCultureIgnoreCase) == 0)
				{
					if (string.Compare(currentUser.Server, serverName, StringComparison.InvariantCultureIgnoreCase) == 0)
					{
						// se ho dato la pwd controllo anche la password
						if (password.Length > 0)
						{
							if (string.Compare(currentUser.Password, password, StringComparison.InvariantCultureIgnoreCase) == 0)
							{
								isAuthenticated = true;
								break;
							}
						}
						else 
						{
							// pwd vuota: può essere realmente vuota oppure si è sbagliato a digitarla come faccio?????
							// se è un utente NT ok 
							if (currentUser.LoginName.Split(System.IO.Path.DirectorySeparatorChar).Length > 1)
								isAuthenticated = true;
							else
								if (currentUser.ProviderType == DBMSType.ORACLE)
									isAuthenticated = true;
								else
								{
									//se è non NT e la pwd risulta diversa sbagliato!!!
									if (string.Compare(currentUser.Password, password, StringComparison.InvariantCultureIgnoreCase) != 0)
										isAuthenticated = false;
									else
										isAuthenticated = true;
								}
							break;
						}
					}
				}
			}

			return isAuthenticated;
		}
		#endregion
	}
	# endregion

	# region AuthenticatedUser
	/// <summary>
	/// AuthenticatedUser
	/// Tiene traccia degli utenti autenticati all'interno dell'AdministrationConsole
	/// </summary>
	//=========================================================================
	public class AuthenticatedUser
	{
		#region Variabili private
		//---------------------------------------------------------------------
		public string	LoginName		= string.Empty;
		public string	Password		= string.Empty;
		public string	Server			= string.Empty;
		public int		Port			= 0;
		public DBMSType ProviderType	= DBMSType.SQLSERVER;
		public DateTime LoginDateTime	= System.DateTime.MinValue;
		#endregion

		#region Costruttore (vuoto)
		//---------------------------------------------------------------------
		public AuthenticatedUser()
		{
		}
		#endregion
	}
	# endregion

	# region class DatabaseService (per gestire la visualizzazione del servizio SQL Server in basso a dx)
	///<summary>
	/// Class DatabaseService
	/// Classe di appoggio per memorizzare le informazioni caricate dall'Administration Console
	/// relative allo stato del servizio di SQL Server al quale ci siamo connessi
	///</summary>
	//=========================================================================
	public class DatabaseService
	{
		private string				serviceName			= string.Empty;
		private string				serviceDisplayName	= string.Empty;
		private string				computerName		= string.Empty;
		private string				activationDatabase  = string.Empty;
		private StatusType			serviceStatus		= StatusType.None;
		private ServiceController	serviceController	= null;

		//---------------------------------------------------------------------
		public string		ServiceDisplayName	{ get { return serviceDisplayName;	} set { serviceDisplayName	= value;}}
		public string		ServiceName			{ get { return serviceName;			} set { serviceName			= value;}}
		public string		ComputerName		{ get { return computerName;		} set { computerName		= value;}}
		public string		ActivationDatabase  { get { return activationDatabase;	} set { activationDatabase	= value;}}
		public StatusType			ServiceStatus		{ get { return serviceStatus;		} set { serviceStatus		= value;}}
		public ServiceController	ServiceController	{ get { return serviceController;	} set { serviceController	= value;}}

		//---------------------------------------------------------------------
		public DatabaseService()
		{
		}
	}
	# endregion
}