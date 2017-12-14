using System;

namespace Microarea.TaskBuilderNet.Core.Generic
{
	///<summary>
	/// Classe serializzabile utilizzata all'interno del LoginManager
	/// In essa sono presenti le informazioni base di un database di EasyAttachment:
	/// - la stringa di connessione con il dbo
	/// - l'id della company al quale appartiene il database
	/// - l'LCID della cultura di database associata al database
	///</summary>
	//================================================================================
	[Serializable]
	public class TbSenderDatabaseInfo
	{
		//--------------------------------------------------------------------------------
		public string ConnectionString { get; set; }
		public int CompanyId { get; set; }

		public string Username  { get; set; }
		public string Password { get; set; }
		public string ServerName  { get; set; }
		public string DatabaseName  { get; set; }
		public bool WinAuthentication  { get; set; }
		public string Company { get; set; }
		public bool IsEnabled { get; set; }
		public string CompanyCulture { get; set; }
		public string CompanyCultureUI { get; set; }

		//--------------------------------------------------------------------------------
		public TbSenderDatabaseInfo()
		{
			IsEnabled = true;
		}

		//--------------------------------------------------------------------------------
		public TbSenderDatabaseInfo(string connectionString)
			: this()
		{
			ConnectionString = connectionString;
		}

		//--------------------------------------------------------------------------------
		public TbSenderDatabaseInfo(string connectionString, int companyId)
			: this(connectionString)
		{
			CompanyId = companyId;
		}
	}
}
