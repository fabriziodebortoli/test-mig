using System;
using System.Data.SqlClient;
using System.Globalization;
using System.IO;
using System.Reflection;
using System.Text;
using System.Xml.Serialization;

using Microarea.TaskBuilderNet.Core.DiagnosticManager;
using Microarea.TaskBuilderNet.Core.Generic;
using Microarea.TaskBuilderNet.Data.DatabaseItems;
using Microarea.TaskBuilderNet.Data.DatabaseLayer;
using Microarea.TaskBuilderNet.Data.SQLDataAccess;
using Microarea.TaskBuilderNet.Interfaces;
using Microarea.Tools.ProvisioningConfigurator.ProvisioningConfigurator.Properties;

namespace Microarea.Tools.ProvisioningConfigurator.ProvisioningConfigurator
{
	/// <summary>
	/// Classe che si occupa di memorizzare tutte le informazioni necessarie
	/// al sistema di provisioning 
	/// </summary>
	[Serializable]
	//---------------------------------------------------------------------
	public class ProvisioningData
	{
		private bool addCompanyMode = false;
		private const string provisioningDataFileName = "ProvisioningData.xml";
		private string connectionString = string.Empty;

		private TransactSQLAccess sqlTransact = null;

		public static Diagnostic ProvisioningDataDiagnostic = new Diagnostic("ProvisioningData");

		public static string FilePath { get { return Path.Combine(Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location), provisioningDataFileName); } }

		/// <summary>
		/// Per capire se si tratta di una nuova configurazione oppure se andiamo 
		/// in aggiunta di un'azienda
		/// </summary>
		//---------------------------------------------------------------------
		[XmlIgnore]
		public bool AddCompanyMode
		{
			get { return addCompanyMode; }
			set { addCompanyMode = value; }
		}

		///<summary>
		/// Nome del database di sistema
		///</summary>
		//---------------------------------------------------------------------
		public string SystemDbName { get; set; }

		///<summary>
		/// Nome del database aziendale
		///</summary>
		//---------------------------------------------------------------------
		public string CompanyDbName { get; set; }

		///<summary>
		/// Nome del database del DMS
		///</summary>
		//---------------------------------------------------------------------
		public string DMSDbName { get; set; }

		///<summary>
		/// Nome dell'azienda
		///</summary>
		//---------------------------------------------------------------------
		public string CompanyName { get; set; }

		// credenziali di amministrazione per la connessione a SQL Server
		//---------------------------------------------------------------------
		public string Server { get; set; }
		public string User {get; set; }
		[XmlIgnore]
		public string Password { get; set; }

		// login e utenti da creare per il servizio di provisioning
		//---------------------------------------------------------------------
		public string AdminLoginName { get; set; }
        [XmlIgnore]
        public string AdminLoginPassword { get; set; }
        public string CryptedAdminLoginPassword { get { return Crypto.Encrypt(AdminLoginPassword); } set { AdminLoginPassword = Crypto.Decrypt(value); } }
        public string UserLoginName { get; set; }
        [XmlIgnore]
        public string UserLoginPassword { get; set; }
        public string CryptedUserLoginPassword { get { return Crypto.Encrypt(UserLoginPassword);} set { UserLoginPassword = Crypto.Decrypt(value); } }

        public string IsoCountry { get; set; }
		public string Edition { get; set; }

		/// <summary>
		/// costruttore per utilizzo con interfaccia
		/// </summary>
		//---------------------------------------------------------------------
		public ProvisioningData()
		{
		}

		/// <summary>
		/// costruttore per utilizzo da riga di comando
		/// </summary>
		//---------------------------------------------------------------------
		public ProvisioningData(CommandLineParam[] arguments) : this()
		{
			SystemDbName = CommandLineParam.GetCommandLineParameterValue("SystemDbName");
			CompanyDbName = CommandLineParam.GetCommandLineParameterValue("CompanyDbName");
			DMSDbName = CommandLineParam.GetCommandLineParameterValue("DMSDbName");
			CompanyName = CommandLineParam.GetCommandLineParameterValue("CompanyName");
			Server = CommandLineParam.GetCommandLineParameterValue("Server");
			User = CommandLineParam.GetCommandLineParameterValue("User");
			Password = CommandLineParam.GetCommandLineParameterValue("Password");
			AdminLoginName = CommandLineParam.GetCommandLineParameterValue("AdminLoginName");
			AdminLoginPassword = CommandLineParam.GetCommandLineParameterValue("AdminLoginPassword");
			UserLoginName = CommandLineParam.GetCommandLineParameterValue("UserLoginName");
			UserLoginPassword = CommandLineParam.GetCommandLineParameterValue("UserLoginPassword");
			IsoCountry = CommandLineParam.GetCommandLineParameterValue("IsoCountry");
			Edition = CommandLineParam.GetCommandLineParameterValue("Edition");
		}

		/// <summary>
		/// Carica la struttura in memoria da file system
		/// </summary>
		//---------------------------------------------------------------------------
		public static ProvisioningData Load()
		{
			try
			{
				if (!File.Exists(FilePath))
					return new ProvisioningData();

				XmlSerializer x = new XmlSerializer(typeof(ProvisioningData));
				using (FileStream fs = new FileStream(FilePath, FileMode.OpenOrCreate, FileAccess.Read))
					return (ProvisioningData)x.Deserialize(fs);
			}
			catch (Exception e)
			{
				try
				{
					if (File.Exists(FilePath))
						File.Delete(FilePath);
				}
				catch { }

				ProvisioningDataDiagnostic.SetError(string.Format(Resources.ErrorLoadingFile, provisioningDataFileName, e.Message));
				return new ProvisioningData();
			}
		}

		/// <summary>
		/// Salva la struttura su file system
		/// </summary>
		//---------------------------------------------------------------------------
		public void Save()
		{
			Save(FilePath);
		}

		/// <summary>
		/// Salva la struttura su file system
		/// </summary>
		//---------------------------------------------------------------------------
		private void Save(string path)
		{
			try
			{
				XmlSerializer x = new XmlSerializer(GetType());
				using (FileStream fs = new FileStream(path, FileMode.Create, FileAccess.Write))
					x.Serialize(fs, this);
			}
			catch (Exception e)
			{
				try
				{
					if (File.Exists(path))
						File.Delete(path);
				}
				catch
				{ }

				ProvisioningDataDiagnostic.SetError(string.Format(Resources.ErrorSavingFile, provisioningDataFileName, e.Message));
			}
		}

		//---------------------------------------------------------------------
		public override string ToString()
		{
			StringBuilder sb = new StringBuilder();
			sb.AppendLine(string.Format("{0}: \"{1}\"", "SystemDbName", SystemDbName));
			sb.AppendLine(string.Format("{0}: \"{1}\"", "CompanyDbName", CompanyDbName));
			sb.AppendLine(string.Format("{0}: \"{1}\"", "DMSDbName", DMSDbName));
			sb.AppendLine(string.Format("{0}: \"{1}\"", "CompanyName", CompanyName));
			sb.AppendLine(string.Format("{0}: \"{1}\"", "Server", Server));
			sb.AppendLine(string.Format("{0}: \"{1}\"", "User", User));
			sb.AppendLine(string.Format("{0}: \"{1}\"", "AdminLoginName", AdminLoginName));
			sb.AppendLine(string.Format("{0}: \"{1}\"", "UserLoginName", UserLoginName));
			sb.AppendLine(string.Format("{0}: \"{1}\"", "IsoCountry", IsoCountry));
			sb.AppendLine(string.Format("{0}: \"{1}\"", "Edition", Edition));
			return sb.ToString();
		}

		//---------------------------------------------------------------------
		public bool Validate(bool lite=  false)
		{
            if (lite)//sostituisco admin user con sa che in lite non esiste
            {
                User = AdminLoginName;
                Password = AdminLoginPassword;
            }
			return CheckEmptyValues() && (lite ? CheckDataLITE() : CheckData());
		}

		//---------------------------------------------------------------------
		public bool CheckEmptyValues()
		{
			if
				(
				   String.IsNullOrWhiteSpace(IsoCountry)	||
				   String.IsNullOrWhiteSpace(Edition)		||
				   String.IsNullOrWhiteSpace(Server)		||
				   String.IsNullOrWhiteSpace(DMSDbName)		||
				   String.IsNullOrWhiteSpace(CompanyName)	||
				   String.IsNullOrWhiteSpace(CompanyDbName) ||
				   String.IsNullOrWhiteSpace(SystemDbName)	||
				   String.IsNullOrWhiteSpace(User)			||
				   String.IsNullOrWhiteSpace(AdminLoginName)||
				   String.IsNullOrWhiteSpace(UserLoginName)
				)
			{
				ProvisioningDataDiagnostic.SetError(Resources.MissingData);
				return false;
			}

			if (string.Compare(DMSDbName, CompanyDbName, StringComparison.InvariantCultureIgnoreCase) == 0 ||
				string.Compare(DMSDbName, SystemDbName, StringComparison.InvariantCultureIgnoreCase) == 0 ||
				string.Compare(CompanyDbName, SystemDbName, StringComparison.InvariantCultureIgnoreCase) == 0)
			{
				ProvisioningDataDiagnostic.SetError(Resources.DBNamesDuplicated);
				return false;
			}

			// non e' possibile specificare l'utente sa perche' poi non si riesce a grantare
			// con l'errore di SQL: Cannot use the special principal 'sa'. Numero errore: 15405
			if (string.Compare(AdminLoginName, "sa", StringComparison.InvariantCultureIgnoreCase) == 0 ||
				string.Compare(UserLoginName, "sa", StringComparison.InvariantCultureIgnoreCase) == 0)
			{
				ProvisioningDataDiagnostic.SetError(Resources.InvalidLoginName);
				return false;
			}

			if (CompanyName.Length > 50)
			{
				ProvisioningDataDiagnostic.SetError(string.Format(Resources.FieldExceedNrChar, Resources.CompanyName, "50"));
				return false;
			}

			if (CompanyDbName.Length > 255 || DMSDbName.Length > 255 || SystemDbName.Length > 255)
			{
				ProvisioningDataDiagnostic.SetError(string.Format(Resources.FieldExceedNrChar, Resources.DatabaseName, "255"));
				return false;
			}

			if (AdminLoginName.Length > 50 || UserLoginName.Length > 50)
			{
				ProvisioningDataDiagnostic.SetError(string.Format(Resources.FieldExceedNrChar, Resources.UserName, "50"));
				return false;
			}

			if (AdminLoginPassword.Length > 128 || UserLoginPassword.Length > 128)
			{
				ProvisioningDataDiagnostic.SetError(string.Format(Resources.FieldExceedNrChar, Resources.UserPassword, "128"));
				return false;
			}

			return true;
		}

        //---------------------------------------------------------------------
        private bool CheckDataLITE()
        {
            try
            {
                RegionInfo info = new RegionInfo(IsoCountry);
            }
            catch (ArgumentException)
            {
                ProvisioningDataDiagnostic.SetError(Resources.IsoCountryMissing);
                return false;
            }

			sqlTransact = new TransactSQLAccess();
			sqlTransact.CurrentStringConnection = string.Format(NameSolverDatabaseStrings.SQLConnection, Server, DatabaseLayerConsts.MasterDatabase, User, Password);

			// provo a connettermi l'utente Admin
			if (!sqlTransact.TryToConnect())
			{
				if (sqlTransact.Diagnostic.Error)
					ProvisioningDataDiagnostic.Set(sqlTransact.Diagnostic);
				
				ProvisioningDataDiagnostic.SetError(string.Format(Resources.LoginFailed, Server, User));
				return false;
			}

			sqlTransact.CurrentStringConnection = string.Format(NameSolverDatabaseStrings.SQLConnection, Server, DatabaseLayerConsts.MasterDatabase, UserLoginName, UserLoginPassword);
			// provo a connettermi l'utente User
			if (!sqlTransact.TryToConnect())
			{
				if (sqlTransact.Diagnostic.Error)
					ProvisioningDataDiagnostic.Set(sqlTransact.Diagnostic);
				
				ProvisioningDataDiagnostic.SetError(string.Format(Resources.LoginFailed, Server, UserLoginName));
				return false;
			}

			// check if databases already exist on server!
			if (DBsAlreadyExist(true))
				return false;

			//---------------------------------------
			// CHECK DB DI SISTEMA
			//---------------------------------------
			// devo testare l'esistenza della login sul database di sistema (sia per l'utente admin che per l'utente r/w, ma devo essere connesso con admin!)
            sqlTransact.CurrentStringConnection = string.Format(NameSolverDatabaseStrings.SQLConnection, Server, SystemDbName, AdminLoginName, AdminLoginPassword);

			if (!sqlTransact.ExistUserIntoDb(AdminLoginName, SystemDbName))
			{
				if (sqlTransact.Diagnostic.Error)
					ProvisioningDataDiagnostic.Set(sqlTransact.Diagnostic);
				else
					ProvisioningDataDiagnostic.SetError(string.Format(Resources.LoginFailed, Server, AdminLoginName));
				return false;
			}
			else
			{
				if (!sqlTransact.LoginIsDBOwnerRole(AdminLoginName))
				{
					if (sqlTransact.Diagnostic.Error)
						ProvisioningDataDiagnostic.Set(sqlTransact.Diagnostic);
					else
						ProvisioningDataDiagnostic.SetError(string.Format(Resources.LoginHasNotExpectedRole, AdminLoginName, DatabaseLayerConsts.RoleDbOwner));
					return false;
				}
			}

			if (!sqlTransact.ExistUserIntoDb(UserLoginName, SystemDbName))
			{
				if (sqlTransact.Diagnostic.Error)
					ProvisioningDataDiagnostic.Set(sqlTransact.Diagnostic);
				else
					ProvisioningDataDiagnostic.SetError(string.Format(Resources.LoginFailed, Server, UserLoginName));
				return false;
			}
			else
			{
				if (!sqlTransact.LoginIsDBRoleDataReaderAndWriter(UserLoginName))
				{
					if (sqlTransact.Diagnostic.Error)
						ProvisioningDataDiagnostic.Set(sqlTransact.Diagnostic);
					else
						ProvisioningDataDiagnostic.SetError(string.Format(Resources.LoginHasNotExpectedRole, UserLoginName, DatabaseLayerConsts.RoleDataReader + "/" + DatabaseLayerConsts.RoleDataWriter));
					return false;
				}
			}

			//---------------------------------------
			// CHECK DB AZIENDALE
			//---------------------------------------
			// devo testare l'esistenza della login sul database aziendale (sia per l'utente admin che per l'utente r/w, ma devo essere connesso con admin!)
			sqlTransact.CurrentStringConnection = string.Format(NameSolverDatabaseStrings.SQLConnection, Server, CompanyDbName, AdminLoginName, AdminLoginPassword);

			if (!sqlTransact.ExistUserIntoDb(AdminLoginName, CompanyDbName))
			{
				if (sqlTransact.Diagnostic.Error)
					ProvisioningDataDiagnostic.Set(sqlTransact.Diagnostic);
				else
					ProvisioningDataDiagnostic.SetError(string.Format(Resources.LoginFailed, Server, AdminLoginName));
				return false;
			}
			else
			{
				if (!sqlTransact.LoginIsDBOwnerRole(AdminLoginName))
				{
					if (sqlTransact.Diagnostic.Error)
						ProvisioningDataDiagnostic.Set(sqlTransact.Diagnostic);
					else
						ProvisioningDataDiagnostic.SetError(string.Format(Resources.LoginHasNotExpectedRole, AdminLoginName, DatabaseLayerConsts.RoleDbOwner));
					return false;
				}
			}

			if (!sqlTransact.ExistUserIntoDb(UserLoginName, CompanyDbName))
			{
				if (sqlTransact.Diagnostic.Error)
					ProvisioningDataDiagnostic.Set(sqlTransact.Diagnostic);
				else
					ProvisioningDataDiagnostic.SetError(string.Format(Resources.LoginFailed, Server, UserLoginName));
				return false;
			}
			else
			{
				if (!sqlTransact.LoginIsDBRoleDataReaderAndWriter(UserLoginName))
				{
					if (sqlTransact.Diagnostic.Error)
						ProvisioningDataDiagnostic.Set(sqlTransact.Diagnostic);
					else
						ProvisioningDataDiagnostic.SetError(string.Format(Resources.LoginHasNotExpectedRole, UserLoginName, DatabaseLayerConsts.RoleDataReader + "/" + DatabaseLayerConsts.RoleDataWriter));
					return false;
				}
			}
			//---------------------------------------
			// CHECK DB DMS
			//---------------------------------------
			// devo testare l'esistenza della login sul database DMS (sia per l'utente admin che per l'utente r/w, ma devo essere connesso con admin!)
			sqlTransact.CurrentStringConnection = string.Format(NameSolverDatabaseStrings.SQLConnection, Server, DMSDbName, AdminLoginName, AdminLoginPassword);

			if (!sqlTransact.ExistUserIntoDb(AdminLoginName, DMSDbName))
			{
				if (sqlTransact.Diagnostic.Error)
					ProvisioningDataDiagnostic.Set(sqlTransact.Diagnostic);
				else
					ProvisioningDataDiagnostic.SetError(string.Format(Resources.LoginFailed, Server, AdminLoginName));
				return false;
			}
			else
			{
				if (!sqlTransact.LoginIsDBOwnerRole(AdminLoginName))
				{
					if (sqlTransact.Diagnostic.Error)
						ProvisioningDataDiagnostic.Set(sqlTransact.Diagnostic);
					else
						ProvisioningDataDiagnostic.SetError(string.Format(Resources.LoginHasNotExpectedRole, AdminLoginName, DatabaseLayerConsts.RoleDbOwner));
					return false;
				}
			}

			if (!sqlTransact.ExistUserIntoDb(UserLoginName, DMSDbName))
			{
				if (sqlTransact.Diagnostic.Error)
					ProvisioningDataDiagnostic.Set(sqlTransact.Diagnostic);
				else
					ProvisioningDataDiagnostic.SetError(string.Format(Resources.LoginFailed, Server, UserLoginName));
				return false;
			}
			else
			{
				if (!sqlTransact.LoginIsDBRoleDataReaderAndWriter(UserLoginName))
				{
					if (sqlTransact.Diagnostic.Error)
						ProvisioningDataDiagnostic.Set(sqlTransact.Diagnostic);
					else
						ProvisioningDataDiagnostic.SetError(string.Format(Resources.LoginHasNotExpectedRole, UserLoginName, DatabaseLayerConsts.RoleDataReader + "/" + DatabaseLayerConsts.RoleDataWriter));
					return false;
				}
			}

            // se sono in fase di aggiunta azienda, devo eseguire un ulteriore check sul nome, in modo che non sia gia' presente nel db di sistema
            if (addCompanyMode)
            {
                connectionString = string.Format(NameSolverDatabaseStrings.SQLConnection, Server, SystemDbName, User, Password);

                using (SqlConnection myConnection = new SqlConnection(connectionString))
                {
                    myConnection.Open();
                    CompanyDb companyDb = new CompanyDb();
                    companyDb.CurrentSqlConnection = myConnection;

                    if (companyDb.ExistsCompanyByName(CompanyName))
                    {
                        ProvisioningDataDiagnostic.SetError(string.Format(Resources.CompanyAlreadyExists, CompanyName));
                        return false;
                    }
                }
            }

            return true;
        }

        //---------------------------------------------------------------------
        private bool CheckData()
        {
            try
            {
                RegionInfo info = new RegionInfo(IsoCountry);
            }
            catch (ArgumentException)
            {
                ProvisioningDataDiagnostic.SetError(Resources.IsoCountryMissing);
                return false;
            }

            connectionString = string.Format(NameSolverDatabaseStrings.SQLConnection, Server, DatabaseLayerConsts.MasterDatabase, User, Password);

            sqlTransact = new TransactSQLAccess();
            sqlTransact.CurrentStringConnection = connectionString;

            // try connect to server and show error messages
            if (!sqlTransact.TryToConnect())
            {
                if (sqlTransact.Diagnostic.Error)
                    ProvisioningDataDiagnostic.Set(sqlTransact.Diagnostic);
                
                ProvisioningDataDiagnostic.SetError(string.Format(Resources.LoginFailed, Server, User));
                return false;
            }

            //controllo utenti sysadmin
            if (!sqlTransact.LoginIsSystemAdminRole(User, DatabaseLayerConsts.RoleSysAdmin))
            {
                if (sqlTransact.Diagnostic.Error)
                    ProvisioningDataDiagnostic.Set(sqlTransact.Diagnostic);
                else
                    ProvisioningDataDiagnostic.SetError(string.Format(Resources.LoginHasNotSysAdminRole, User));
                return false;
            }

            // check if databases already exist on server!
            if (DBsAlreadyExist())
                return false;

            //esistenza login admin e verifica password
            if (sqlTransact.ExistLogin(AdminLoginName))
            {
                connectionString = string.Format(NameSolverDatabaseStrings.SQLConnection, Server, DatabaseLayerConsts.MasterDatabase, AdminLoginName, AdminLoginPassword);
                sqlTransact.CurrentStringConnection = connectionString;
                if (!sqlTransact.TryToConnect())
                {
                    if (sqlTransact.Diagnostic.Error)
                        ProvisioningDataDiagnostic.Set(sqlTransact.Diagnostic);
                    
                    ProvisioningDataDiagnostic.SetError(string.Format(Resources.LoginFailed, Server, AdminLoginName));
                    return false;
                }
            }

            //esistenza login basic e verifica password
            if (sqlTransact.ExistLogin(UserLoginName))
            {
                connectionString = string.Format(NameSolverDatabaseStrings.SQLConnection, Server, DatabaseLayerConsts.MasterDatabase, UserLoginName, UserLoginPassword);
                sqlTransact.CurrentStringConnection = connectionString;
                if (!sqlTransact.TryToConnect())
                {
                    if (sqlTransact.Diagnostic.Error)
                        ProvisioningDataDiagnostic.Set(sqlTransact.Diagnostic);
                    
                    ProvisioningDataDiagnostic.SetError(string.Format(Resources.LoginFailed, Server, UserLoginName));
                    return false;
                }
            }

            // se sono in fase di aggiunta azienda, devo eseguire un ulteriore check sul nome, in modo che non sia gia' presente nel db di sistema
            if (addCompanyMode)
            {
                connectionString = string.Format(NameSolverDatabaseStrings.SQLConnection, Server, SystemDbName, User, Password);

                using (SqlConnection myConnection = new SqlConnection(connectionString))
                {
                    myConnection.Open();
                    CompanyDb companyDb = new CompanyDb();
                    companyDb.CurrentSqlConnection = myConnection;

                    if (companyDb.ExistsCompanyByName(CompanyName))
                    {
                        ProvisioningDataDiagnostic.SetError(string.Format(Resources.CompanyAlreadyExists, CompanyName));
                        return false;
                    }
                }
            }

            return true;

        }

        /// <summary>
        /// compone la stringa con i parametri da passare esternamente 
        /// </summary>
        //---------------------------------------------------------------------
        internal string CommandLine()
		{
			StringBuilder sb = new StringBuilder();
			sb.Append(string.Format("/{0} \"{1}\" ", "SystemDbName", SystemDbName));
			sb.Append(string.Format("/{0} \"{1}\" ", "CompanyDbName", CompanyDbName));
			sb.Append(string.Format("/{0} \"{1}\" ", "DMSDbName", DMSDbName));
			sb.Append(string.Format("/{0} \"{1}\" ", "CompanyName", CompanyName));
			sb.Append(string.Format("/{0} \"{1}\" ", "Server", Server));
			sb.Append(string.Format("/{0} \"{1}\" ", "User", User));
			sb.Append(string.Format("/{0} \"{1}\" ", "Password", Password));
			sb.Append(string.Format("/{0} \"{1}\" ", "AdminLoginName", AdminLoginName));
			sb.Append(string.Format("/{0} \"{1}\" ", "AdminLoginPassword", AdminLoginPassword));
			sb.Append(string.Format("/{0} \"{1}\" ", "UserLoginName", UserLoginName));
			sb.Append(string.Format("/{0} \"{1}\" ", "UserLoginPassword", UserLoginPassword));
			sb.Append(string.Format("/{0} \"{1}\" ", "IsoCountry", IsoCountry));
			sb.Append(string.Format("/{0} \"{1}\" ", "Edition", Edition));

			return sb.ToString();
		}

		///<summary>
		/// Controllo nella tabella sysdatabases se esistono i database con i nomi impostati per la creazione
		///</summary>
		//---------------------------------------------------------------------
		private bool DBsAlreadyExist(bool lite = false)
		{
			bool existSysDB = false;
			bool existDMSDB = false;
			bool existCompanyDB = false;

			try
			{
				if (lite || !addCompanyMode)
					existSysDB = sqlTransact.ExistDataBase(SystemDbName);
				existCompanyDB = sqlTransact.ExistDataBase(CompanyDbName);
				existDMSDB = sqlTransact.ExistDataBase(DMSDbName);
			}
			catch (Exception exc)
			{
				ProvisioningDataDiagnostic.SetError(exc.ToString());
				return false;
			}

			// se sto eseguendo la versione LITE i database devono esistere!
			if (lite)
			{
				if (!existSysDB)
					ProvisioningDataDiagnostic.SetError(string.Format(Resources.DBNotExist, SystemDbName, Server));

				if (!existCompanyDB)
					ProvisioningDataDiagnostic.SetError(string.Format(Resources.DBNotExist, CompanyDbName, Server));

				if (!existDMSDB)
					ProvisioningDataDiagnostic.SetError(string.Format(Resources.DBNotExist, DMSDbName, Server));

				return !(existSysDB && existCompanyDB && existDMSDB);
			}

			if (existSysDB)
				ProvisioningDataDiagnostic.SetError(string.Format(Resources.DBAlreadyExists, SystemDbName, Server));

			if (existCompanyDB)
				ProvisioningDataDiagnostic.SetError(string.Format(Resources.DBAlreadyExists, CompanyDbName, Server));

			if (existDMSDB)
				ProvisioningDataDiagnostic.SetError(string.Format(Resources.DBAlreadyExists, DMSDbName, Server));

			return (existSysDB || existCompanyDB || existDMSDB);
		}
	}
}