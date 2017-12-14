using System;
using System.Collections.Generic;
using System.Data.SqlTypes;
using Microarea.TaskBuilderNet.Interfaces;

namespace Microarea.TaskBuilderNet.Core.Generic
{
	///<summary>
	/// Classe serializzabile utilizzata all'interno del LoginManager
	/// In essa sono presenti le informazioni base di un database di EasyAttachment:
	/// - la stringa di connessione con il dbo
	/// - l'id della company al quale appartiene il database
	/// - l'LCID della cultura di database associata al database
	/// - se i componenti della ricerca FullText sono installati ed abilitati sul server SQL
	/// - se la ricerca FullText e' in uso in EasyAttachment (lato Mago.net)
	/// - dati relativi alla collate per la gestione del fulltext
	/// - se il SOSConnector e' attivato
	/// - la data di ultima sincronizzazione degli stati spedizione del SOS
	///</summary>
	//================================================================================
	[Serializable]
	public class DmsDatabaseInfo
	{
		//--------------------------------------------------------------------------------
		public string CompanyConnectionString { get; set; }
		public DBMSType CompanyDBMSType { get; set; }

		public string DMSConnectionString { get; set; }

		public string Server { get; set; }
		public string Database { get; set; }

        public string Company { get; set; }
		public int CompanyId { get; set; }
		public int LCID { get; set; }

		public bool IsFTSEnabled { get; set; } // se i componenti della ricerca FullText sono installati ed abilitati sul server SQL
		public bool UseFTS { get; set; } // se la ricerca FullText e' in uso nella Company con EasyAttachment (lato Mago.net)
		
		public bool IsAlreadyAnalyzed { get; set; }
		public string ExtensionTypeCollate { get; set; }
		public string FulltextDocumentTypesCollate { get; set; }
		public bool IsSOSActivated { get; set; }
		public DateTime LastSOSUpdateDateTime { get; set; }

		//--------------------------------------------------------------------------------
		public DmsDatabaseInfo()
		{
		}

		//--------------------------------------------------------------------------------
		public DmsDatabaseInfo(string connectionString, int companyId, int lcid)
		{
			DMSConnectionString = connectionString;
			CompanyId = companyId;
			LCID = lcid;
			
			IsFTSEnabled = false;
			UseFTS = false;
			IsAlreadyAnalyzed = false;
			LastSOSUpdateDateTime = (DateTime)SqlDateTime.MinValue;
		}
	}

    ///<summary>
    /// Classe serializzabile utilizzata all'interno del LoginManager
    /// In essa sono presenti le informazioni base di un'azienda che utilizza il DataSynchronizer
    ///</summary>
    //================================================================================
    [Serializable]
    public class DataSynchroDatabaseInfo
    {
        //--------------------------------------------------------------------------------
        public string CompanyConnectionString { get; set; }
        public DBMSType CompanyDBMSType { get; set; }

        public string Server { get; set; }
        public string Database { get; set; }
        public string User { get; set; }
        public string Password { get; set; }
        public bool WinAuthentication { get; set; }

        public string CompanyName { get; set; }
        public int CompanyId { get; set; }

        public string LoginName { get; set; }
        public string LoginPassword { get; set; }
        public bool LoginWindowsAuthentication { get; set; }
        public int LoginId { get; set; }

        // informazioni del db di EA
        public bool UseDBSlave { get; set; }
        public string DmsConnectionString { get; set; }

        // elenco di oggetti di tipo IBaseSynchroProvider
        public List<IBaseSynchroProvider> SynchroProviders { get; set; }

        //--------------------------------------------------------------------------------
        public DataSynchroDatabaseInfo()
        {
        }

        //--------------------------------------------------------------------------------
        public DataSynchroDatabaseInfo(string connectionString, int companyId)
        {
            CompanyConnectionString = connectionString;
            CompanyId = companyId;
        }

        //---------------------------------------------------------------------
        public DataSynchroDatabaseInfo Clone()
        {
            DataSynchroDatabaseInfo dbInfo = new DataSynchroDatabaseInfo();

            dbInfo.CompanyConnectionString = this.CompanyConnectionString;
            dbInfo.CompanyDBMSType = this.CompanyDBMSType;
            dbInfo.Server = this.Server;
            dbInfo.Database = this.Database;
            dbInfo.User = this.User;
            dbInfo.Password = this.Password;
            dbInfo.WinAuthentication = this.WinAuthentication;
            dbInfo.CompanyName = this.CompanyName;
            dbInfo.CompanyId = this.CompanyId;
            dbInfo.LoginName = this.LoginName;
            dbInfo.LoginPassword = this.LoginPassword;
            dbInfo.LoginWindowsAuthentication = this.LoginWindowsAuthentication;
            dbInfo.LoginId = this.LoginId;
            dbInfo.SynchroProviders = new List<IBaseSynchroProvider>(this.SynchroProviders);
            dbInfo.UseDBSlave = this.UseDBSlave;
            dbInfo.DmsConnectionString = this.DmsConnectionString;

            return dbInfo;
        }
    }
}
