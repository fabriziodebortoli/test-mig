
namespace Microarea.Console.Core.PlugIns
{
	/// <summary>
	/// ConsoleEnvironmentInfo.
	/// Informazioni sull'ambiente di console quali
	/// - Configurazione (impostato dalla linea di comando)
	/// - Se la console gira lato client o server
	/// - L'utente connesso alla console (che accede cioè al db di sistema - considerato di MConsole)
	/// </summary
	// ========================================================================
	public class ConsoleEnvironmentInfo
	{
		//---------------------------------------------------------------------
		private string				configuration		= string.Empty;
		private bool				runningFromServer	= false;
		private UserConnectedInfo	userConnectedInfo	= new UserConnectedInfo();
		private UserGuestInfo		userGuestInfo		= new UserGuestInfo();
		private StatusType			consoleStatus		= StatusType.None;
		private bool				isLiteConsole		= false;

		//---------------------------------------------------------------------
		public bool					RunningFromServer		{ get { return runningFromServer;	} set { runningFromServer	= value; }}
		public	UserConnectedInfo	ConsoleUserInfo			{ get { return userConnectedInfo;	} set { userConnectedInfo	= value; }}
		public	UserGuestInfo		ConsoleUserGuestInfo	{ get { return userGuestInfo;		} set { userGuestInfo		= value; }}
		public  StatusType			ConsoleStatus			{ get { return consoleStatus;		} set { consoleStatus		= value; }}
		public bool					IsLiteConsole			{ get { return isLiteConsole;		} set { isLiteConsole		= value; }}

		//---------------------------------------------------------------------
		public ConsoleEnvironmentInfo()
		{
		}
	}

	/// <summary>
	/// UserConnectedInfo
	/// Info relative all'utente connesso in MConsole (aggiornate dal SysAdmin)
	/// </summary>
	// ========================================================================
	public class UserConnectedInfo
	{
		//---------------------------------------------------------------------
		private string userName		= string.Empty;
		private string userPwd		= string.Empty;
		private bool   isWinAuth	= false;
		private string serverName	= string.Empty;
		private string dbName		= string.Empty;

		//---------------------------------------------------------------------
		public string UserName		{ get { return userName; } set { userName = value; } }
		public string UserPwd		{ get { return userPwd;		} set { userPwd		= value; }}
		public bool	  IsWinAuth		{ get { return isWinAuth;	} set { isWinAuth	= value; }}
		public string ServerName	{ get { return serverName;	} set { serverName	= value; }}
		public string DbName		{ get { return dbName;		} set { dbName		= value; }}

		//---------------------------------------------------------------------
		public UserConnectedInfo()
		{
		}
	}

	/// <summary>
	/// UserGuestInfo
	/// Info relative all'utente Guest
	/// </summary>
	// ========================================================================
	public class UserGuestInfo
	{
		private string userName = string.Empty;
		private string userPwd  = string.Empty;
		private bool   exist	= false;

		//---------------------------------------------------------------------
		public string UserName { get { return userName; } set { userName = value; } }
		public string UserPwd  { get { return userPwd;  } set { userPwd  = value; }}
		public bool	  Exist    { get { return exist;    } set { exist    = value; }}

		//---------------------------------------------------------------------
		public UserGuestInfo()
		{
		}
	}
}