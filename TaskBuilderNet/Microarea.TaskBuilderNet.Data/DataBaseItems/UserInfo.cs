using System.IO;

using Microarea.TaskBuilderNet.Interfaces;

using Microarea.TaskBuilderNet.Data.DatabaseLayer;

namespace Microarea.TaskBuilderNet.Data.DatabaseItems
{
	/// <summary>
	/// Per le informazioni dell'utente in fase di creazione azienda
	/// </summary>
	//============================================================================
	public class UserInfo
	{
		//impersonificazione
		private UserImpersonatedData dataToConnectionServer;

		private string companyId = string.Empty;
		private string databaseName = string.Empty;
		private string loginId = string.Empty;
		private string serverPrimary = string.Empty;
		private string serverIstance = string.Empty;
		private string loginName = string.Empty;
		private string loginPassword = string.Empty;
		private string userOracleName = string.Empty;
		private string userOraclePwd = string.Empty;
		private string domain = string.Empty;
		private string serverComplete = string.Empty;
		private string oracleService = string.Empty;
		private bool isWinNT = false;
		public bool UseUnicode = false;
		public DBMSType DbType = DBMSType.SQLSERVER;

		//Properties
		//---------------------------------------------------------------------
		public string CompanyId { get { return companyId; } set { companyId = value; } }
		//--------------------------------------------------------------------------------
		public string DatabaseName { get { return databaseName; } set { databaseName = value; } }
		//--------------------------------------------------------------------------------
		public string LoginId { get { return loginId; } set { loginId = value; } }
		//--------------------------------------------------------------------------------
		public string LoginName { get { return loginName; } set { loginName = value; } }
		//--------------------------------------------------------------------------------
		public string LoginPassword { get { return loginPassword; } set { loginPassword = value; } }
		//--------------------------------------------------------------------------------
		public string UserOracleName { get { return userOracleName; } set { userOracleName = value; } }
		//--------------------------------------------------------------------------------
		public string UserOraclePwd { get { return userOraclePwd; } set { userOraclePwd = value; } }

		//--------------------------------------------------------------------------------
		public string ServerPrimary { get { return serverPrimary; } set { serverPrimary = value; } }
		//--------------------------------------------------------------------------------
		public string ServerIstance { get { return serverIstance; } set { serverIstance = value; } }
		//--------------------------------------------------------------------------------
		public string OracleService { get { return oracleService; } set { oracleService = value; } }

		//--------------------------------------------------------------------------------
		public UserImpersonatedData Impersonification { get { return dataToConnectionServer; } set { dataToConnectionServer = value; } }

		//--------------------------------------------------------------------------------
		public string ServerComplete { get { return (serverIstance.Length == 0) ? serverPrimary : Path.Combine(serverPrimary, serverIstance); } }

		//--------------------------------------------------------------------------------
		public string Domain
		{
			get { return domain; }
			set { domain = value; isWinNT = (domain.Length > 0); }
		}

		//--------------------------------------------------------------------------------
		public bool IsWinNT { get { return isWinNT; } }

		/// <summary>
		/// costruttore vuoto
		/// </summary>
		//---------------------------------------------------------------------
		public UserInfo()
		{
		}
	}
}