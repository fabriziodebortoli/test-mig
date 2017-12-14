using System;
using System.Collections;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using Microarea.TaskBuilderNet.Core.DiagnosticManager;
using Microarea.TaskBuilderNet.Core.Generic;
using Microarea.TaskBuilderNet.Data.DatabaseLayer;
using Microarea.TaskBuilderNet.Interfaces;

namespace Microarea.TaskBuilderNet.Data.DatabaseItems
{
	/// <summary>
	/// Classe per la gestione dei records in MSD_Companies
	/// </summary>
	//=========================================================================
	public class CompanyDb : DataBaseItem
	{
		///<summary>
		/// Constructor
		///</summary>
		//---------------------------------------------------------------------
		public CompanyDb() {}

		#region Add - Inserimento di una nuova Azienda
		/// <summary>
		/// Inserisce una nuova azienda nella tabella MSD_Companies
		/// </summary>
		//---------------------------------------------------------------------
		public bool Add
			(
				string 	company,
				string 	description, 
				string 	providerId,
				string 	companyDbServer, 
				string 	companyDbName, 
				string 	defaultUser,
				string 	defaultPassword, 
				string 	companyDbOwner,
				bool   	security,
				bool   	activity, 
				bool   	useTransaction, 
				bool   	useKeyedUpdate,
				bool   	companyDbWindowsAuthentication,
				string 	preferredLanguage,
				string 	applicationLanguage,
				bool   	disabled,
				bool   	useUnicode,
				bool   	isValid,
				int		dbCultureLCID,
				bool	supportColsCollation,
				int		port,
				bool	useDBSlave,
				bool	useRowSecurity,
				bool	useDataSynchro
			)
		{
			bool result = false;
			SqlTransaction  myTransCompanySql;
			SqlCommand myCommand	= new SqlCommand();
			myTransCompanySql		= CurrentSqlConnection.BeginTransaction();
			myCommand.Connection	= CurrentSqlConnection;
			myCommand.Transaction	= myTransCompanySql;

			try
			{
				string strQuery =
				@"INSERT INTO MSD_Companies
				(
				Company, Description, ProviderId, CompanyDBServer, CompanyDBName, CompanyDBOwner,
				DBDefaultUser, DBDefaultPassword, UseTransaction, UseSecurity, UseAuditing, UseKeyedUpdate,
				CompanyDBWindowsAuthentication, PreferredLanguage, ApplicationLanguage, Disabled, 
				UseUnicode, IsValid, DatabaseCulture, SupportColumnCollation, Port, UseDBSlave, UseRowSecurity, UseDataSynchro
				)
				VALUES 
				(
				@Company, @Description, @ProviderId, @CompanyDBServer, @CompanyDBName, @CompanyDBOwner,
				@DBDefaultUser, @DBDefaultPassword, @UseTransaction, @UseSecurity, @UseAuditing, @UseKeyedUpdate,
				@CompanyDBWinAuth, @PreferredLanguage, @ApplicationLanguage, @Disabled, 
				@UseUnicode, @IsValid, @DatabaseCulture, @SupportColumnCollation, @Port, @UseDBSlave, @UseRowSecurity, @UseDataSynchro
				)";
											
				myCommand.CommandText = strQuery;
				myCommand.Parameters.Add(new SqlParameter("@Company",					company));
				myCommand.Parameters.Add(new SqlParameter("@Description",				description));
				myCommand.Parameters.Add(new SqlParameter("@ProviderId",				Int32.Parse(providerId)));
				myCommand.Parameters.Add(new SqlParameter("@CompanyDBServer",			companyDbServer));
				myCommand.Parameters.Add(new SqlParameter("@CompanyDBName",				companyDbName));
				myCommand.Parameters.Add(new SqlParameter("@CompanyDBOwner",			Int32.Parse(companyDbOwner)));
				myCommand.Parameters.Add(new SqlParameter("@DBDefaultUser",				defaultUser));
				myCommand.Parameters.Add(new SqlParameter("@DBDefaultPassword",			Crypto.Encrypt(defaultPassword)));
				myCommand.Parameters.Add(new SqlParameter("@UseTransaction",			useTransaction));
				myCommand.Parameters.Add(new SqlParameter("@UseKeyedUpdate",			useKeyedUpdate));
				myCommand.Parameters.Add(new SqlParameter("@CompanyDBWinAuth",			companyDbWindowsAuthentication));
				myCommand.Parameters.Add(new SqlParameter("@UseSecurity",				security));
				myCommand.Parameters.Add(new SqlParameter("@UseAuditing",				activity));
				myCommand.Parameters.Add(new SqlParameter("@PreferredLanguage",			preferredLanguage));
				myCommand.Parameters.Add(new SqlParameter("@ApplicationLanguage",		applicationLanguage));
				myCommand.Parameters.Add(new SqlParameter("@Disabled",					disabled));
				myCommand.Parameters.Add(new SqlParameter("@UseUnicode",				useUnicode));
				myCommand.Parameters.Add(new SqlParameter("@IsValid",					isValid));
				myCommand.Parameters.Add(new SqlParameter("@DatabaseCulture",			dbCultureLCID));
				myCommand.Parameters.Add(new SqlParameter("@SupportColumnCollation",	supportColsCollation));
				myCommand.Parameters.Add(new SqlParameter("@Port",						port));
				myCommand.Parameters.Add(new SqlParameter("@UseDBSlave",				useDBSlave));
				myCommand.Parameters.Add(new SqlParameter("@UseRowSecurity",			useRowSecurity));
				myCommand.Parameters.Add(new SqlParameter("@UseDataSynchro",			useDataSynchro));
				myCommand.ExecuteNonQuery();
				myTransCompanySql.Commit();
				result = true;
			}
			catch (SqlException sqlException)
			{
				myTransCompanySql.Rollback();
				ExtendedInfo extendedInfo = new ExtendedInfo();
				extendedInfo.Add(DatabaseLayerStrings.Description, sqlException.Message);
				extendedInfo.Add(DatabaseLayerStrings.Procedure,	sqlException.Procedure);
				extendedInfo.Add(DatabaseLayerStrings.Server,		sqlException.Server);
				extendedInfo.Add(DatabaseLayerStrings.Number,		sqlException.Number);
				extendedInfo.Add(DatabaseLayerStrings.LineNumber,	sqlException.LineNumber);
				extendedInfo.Add(DatabaseLayerStrings.Function,		"CompanyDb.Add");
				extendedInfo.Add(DatabaseLayerStrings.DefinedInto, "TaskBuilderNet.Data.DatabaseItems");
				Diagnostic.Set(DiagnosticType.Error, string.Format(DatabaseItemsStrings.CompanyAdd, company), extendedInfo);
			}
			return result;
		}
		#endregion

		#region Modify - Modifica di una Azienda 
		/// <summary>
		/// Modify
		/// Modifica i dati di una azienda nella tabella MSD_Companies
		/// </summary>
		//---------------------------------------------------------------------
		public bool Modify
			(
				string 	companyId,
				string 	company,
				string 	description,
				string 	providerId,
				string 	companyDbServer, 
				string 	companyDbName, 
				string 	defaultUser,
				string 	defaultPassword, 
				string 	companyDbOwner,
				bool   	security,
				bool   	auditing, 
				bool   	useTransaction,
				bool   	useKeyedUpdate,
				bool   	companyDbWindowsAuthentication,
				string 	preferredLanguage,
				string 	applicationLanguage,
				bool   	disabled,
				bool   	useUnicode,
				bool   	isValid,
				int		databaseCulture,
				bool	supportColsCollation,
				int		port,
				bool	useDBSlave,
				bool	useRowSecurity,
				bool	useDataSynchro
			)
		{
			bool result = false;
			SqlTransaction myTransSql;
			SqlCommand myCommand	= new SqlCommand();
			myTransSql				= CurrentSqlConnection.BeginTransaction();
			myCommand.Connection	= CurrentSqlConnection;
			myCommand.Transaction	= myTransSql;

			try
			{
				string strQuery = @"UPDATE MSD_Companies
									SET Company						   	= @Company,
										Description					   	= @Description,
										ProviderId					   	= @ProviderId,
										CompanyDBServer				   	= @CompanyDBServer,
										CompanyDBName				   	= @CompanyDBName,
										CompanyDBOwner				   	= @CompanyDBOwner,
										DBDefaultUser				   	= @DBDefaultUser,
										DBDefaultPassword			   	= @DBDefaultPassword,
										UseTransaction				   	= @UseTransaction,
										UseSecurity					   	= @UseSecurity,
										UseAuditing					   	= @UseAuditing,
										UseKeyedUpdate				   	= @UseKeyedUpdate,
										CompanyDBWindowsAuthentication 	= @CompanyDBWinAuth,
										PreferredLanguage			   	= @PreferredLanguage,
										ApplicationLanguage            	= @ApplicationLanguage,
										Disabled					   	= @Disabled,
										UseUnicode                     	= @UseUnicode,
										IsValid		                   	= @IsValid,
										DatabaseCulture					= @DatabaseCulture,
										SupportColumnCollation			= @SupportColumnCollation,
										Port							= @Port,
										UseDBSlave						= @UseDBSlave,
										UseRowSecurity					= @UseRowSecurity,
										UseDataSynchro					= @UseDataSynchro
									 WHERE 
										CompanyId = @CompanyId";

				myCommand.CommandText = strQuery;
				myCommand.Parameters.Add(new SqlParameter("@CompanyId",				Int32.Parse(companyId)));
				myCommand.Parameters.Add(new SqlParameter("@Company",				company));
				myCommand.Parameters.Add(new SqlParameter("@Description",			description));
				myCommand.Parameters.Add(new SqlParameter("@ProviderId",			Convert.ToInt32(providerId)));
				myCommand.Parameters.Add(new SqlParameter("@CompanyDBServer",		companyDbServer));
				myCommand.Parameters.Add(new SqlParameter("@CompanyDBName",			companyDbName));
				myCommand.Parameters.Add(new SqlParameter("@CompanyDBOwner",		Int32.Parse(companyDbOwner)));
				myCommand.Parameters.Add(new SqlParameter("@DBDefaultUser",			defaultUser));
				myCommand.Parameters.Add(new SqlParameter("@DBDefaultPassword",		Crypto.Encrypt(defaultPassword)));
				myCommand.Parameters.Add(new SqlParameter("@UseTransaction",		useTransaction));
				myCommand.Parameters.Add(new SqlParameter("@UseKeyedUpdate",		useKeyedUpdate));
				myCommand.Parameters.Add(new SqlParameter("@CompanyDBWinAuth",		companyDbWindowsAuthentication));
				myCommand.Parameters.Add(new SqlParameter("@UseSecurity",			security));
				myCommand.Parameters.Add(new SqlParameter("@UseAuditing",			auditing));
				myCommand.Parameters.Add(new SqlParameter("@PreferredLanguage",		preferredLanguage));
				myCommand.Parameters.Add(new SqlParameter("@ApplicationLanguage",	applicationLanguage));
				myCommand.Parameters.Add(new SqlParameter("@Disabled",				disabled));
				myCommand.Parameters.Add(new SqlParameter("@UseUnicode",			useUnicode));
				myCommand.Parameters.Add(new SqlParameter("@IsValid",				isValid));
				myCommand.Parameters.Add(new SqlParameter("@DatabaseCulture",		databaseCulture));
				myCommand.Parameters.Add(new SqlParameter("@SupportColumnCollation",supportColsCollation));
				myCommand.Parameters.Add(new SqlParameter("@Port",					port));
				myCommand.Parameters.Add(new SqlParameter("@UseDBSlave",			useDBSlave));
				myCommand.Parameters.Add(new SqlParameter("@UseRowSecurity",		useRowSecurity));
				myCommand.Parameters.Add(new SqlParameter("@UseDataSynchro",		useDataSynchro));
				myCommand.ExecuteNonQuery();                           
				myTransSql.Commit();
				result = true;
			}
			catch (SqlException sqlException)
			{
				myTransSql.Rollback();
				ExtendedInfo extendedInfo = new ExtendedInfo();
				extendedInfo.Add(DatabaseLayerStrings.Description, sqlException.Message);
				extendedInfo.Add(DatabaseLayerStrings.Procedure,	sqlException.Procedure);
				extendedInfo.Add(DatabaseLayerStrings.Server,		sqlException.Server);
				extendedInfo.Add(DatabaseLayerStrings.Number,		sqlException.Number);
				extendedInfo.Add(DatabaseLayerStrings.LineNumber,	sqlException.LineNumber);
				extendedInfo.Add(DatabaseLayerStrings.Function,     "CompanyDb.Modify");
				extendedInfo.Add(DatabaseLayerStrings.DefinedInto, "TaskBuilderNet.Data.DatabaseItems");
				Diagnostic.Set(DiagnosticType.Error, string.Format(DatabaseItemsStrings.CompanyModify, company), extendedInfo);
			}
			return result;
		}
		# endregion

		#region Delete - Cancellazione dei dati anagrafici di una Azienda (non il db)
		/// <summary>
		/// Delete
		/// Cancella i dati anagrafici di una azienda (non il db) utilizzando la stored procedure MSD_DeleteCompany
		/// </summary>
		/// <param name="CompanyId">Id dell'azienda</param>
		//---------------------------------------------------------------------
		public bool Delete (string CompanyId)
		{
			bool successDelete	  = false;
			SqlCommand myCommand  = new SqlCommand();
			myCommand.Connection  = CurrentSqlConnection;
			myCommand.CommandText = "MSD_DeleteCompany";
			myCommand.CommandType = CommandType.StoredProcedure;
			myCommand.Parameters.AddWithValue("@par_companyid", Int32.Parse(CompanyId));

			try
			{
				myCommand.ExecuteNonQuery();
				successDelete = true;
			}
			catch(SqlException sqlException)
			{
				ExtendedInfo extendedInfo = new ExtendedInfo();
				extendedInfo.Add(DatabaseLayerStrings.Description,	sqlException.Message);
				extendedInfo.Add(DatabaseLayerStrings.Procedure,	sqlException.Procedure);
				extendedInfo.Add(DatabaseLayerStrings.Server,		sqlException.Server);
				extendedInfo.Add(DatabaseLayerStrings.Number,		sqlException.Number);
				extendedInfo.Add(DatabaseLayerStrings.LineNumber,	sqlException.LineNumber);
				extendedInfo.Add(DatabaseLayerStrings.Function,     "CompanyDb.Delete");
				extendedInfo.Add(DatabaseLayerStrings.DefinedInto, "TaskBuilderNet.Data.DatabaseItems");
				Diagnostic.Set(DiagnosticType.Error, DatabaseItemsStrings.CompanyDeleted, extendedInfo);
			}
			
			myCommand.Dispose();
			return successDelete;
		}
		#endregion

		#region Clone - Clonazione di una Azienda
		/// <summary>
		/// Clone
		/// Data una company, la clona, riportando nella nuova company tutti
		/// gli utenti, ruoli, e altri oggetti della company sorgente
		/// Viene utilizzata la stored procedure MSD_CloneCompany
		/// </summary>
		/// <param name="srcCompanyId">Id dell'azienda sorgente</param>
		/// <param name="dstCompanyName">Nome dell'azienda destinazione</param>
		/// <param name="companyDbServer">Nome server su cui creare il db dell'azienda destinazione</param>
		/// <param name="companyDbName">Nome db azienda destinazione</param>
		/// <param name="companyDbOwnerId">Id dell'owner del db aziendale di destinazione</param>
		//---------------------------------------------------------------------
		public bool Clone
			(
			string srcCompanyId, 
			string dstCompanyName, 
			string companyDbServer, 
			string companyDbName, 
			string companyDbOwnerId
			)
		{
			bool result = false;
			SqlCommand myCommand = new SqlCommand();
			myCommand.Connection = CurrentSqlConnection;
			myCommand.CommandText = "MSD_CloneCompany";
			myCommand.CommandType = CommandType.StoredProcedure;
			
			myCommand.Parameters.AddWithValue("@par_srccompanyid",		Int32.Parse(srcCompanyId));
			myCommand.Parameters.AddWithValue("@par_newcompanyname",	dstCompanyName.Trim());
			myCommand.Parameters.AddWithValue("@par_newcompanydbserver",companyDbServer.Trim());
			myCommand.Parameters.AddWithValue("@par_newcompanydbname",	companyDbName.Trim());
			myCommand.Parameters.AddWithValue("@par_newcompanydbowner",	Int32.Parse(companyDbOwnerId));

			try
			{
				myCommand.ExecuteNonQuery();
				result = true;
			}
			catch(SqlException sqlException)
			{
				ExtendedInfo extendedInfo = new ExtendedInfo();
				extendedInfo.Add(DatabaseLayerStrings.Description, sqlException.Message);
				extendedInfo.Add(DatabaseLayerStrings.Procedure,	sqlException.Procedure);
				extendedInfo.Add(DatabaseLayerStrings.Server,		sqlException.Server);
				extendedInfo.Add(DatabaseLayerStrings.Number,		sqlException.Number);
				extendedInfo.Add(DatabaseLayerStrings.LineNumber,	sqlException.LineNumber);
				extendedInfo.Add(DatabaseLayerStrings.StoredProcedure, "MSD_CloneCompany");
				extendedInfo.Add(DatabaseLayerStrings.Function,     "CompanyDb.DeleteCompanyDb");
				extendedInfo.Add(DatabaseLayerStrings.DefinedInto, "TaskBuilderNet.Data.DatabaseItems");
				Diagnostic.Set(DiagnosticType.Error, DatabaseItemsStrings.CompanyCloned, extendedInfo);
			}
			return result;
		}
		#endregion

		#region IsDbOwner - Ritorna True se la loginName è un dbowner di una qualche azienda (aziende con identico providerId)
		/// <summary>
		/// IsDbOwner
		/// </summary>
		//---------------------------------------------------------------------
		public bool IsDbOwner(string loginName, string providerId)
		{
			bool isDbOwner = false;
			SqlCommand mySqlCommand = null;
			int recordsCount = 0;

			try
			{
				string sSelect = @"SELECT COUNT(*) FROM	MSD_Companies INNER	JOIN MSD_CompanyLogins 
									ON MSD_CompanyLogins.DBUser = @LoginName
									WHERE MSD_Companies.CompanyDBOwner = MSD_CompanyLogins.LoginId 
									AND MSD_Companies.ProviderId = @ProviderId";
				
				mySqlCommand = new SqlCommand(sSelect, CurrentSqlConnection);
				mySqlCommand.Parameters.AddWithValue("@LoginName",	loginName);
				mySqlCommand.Parameters.AddWithValue("@ProviderId", providerId);
				
				recordsCount = (int)mySqlCommand.ExecuteScalar();
				mySqlCommand.Dispose();
			}
			catch (SqlException)
			{
				if (mySqlCommand != null)
					mySqlCommand.Dispose();
			}
			if (recordsCount > 0)
				isDbOwner = true;
			
			return isDbOwner;
		}
		#endregion

		///<summary>
		/// UpdateUseDBSlaveValue
		/// Metodo che aggiorna il valore della colonna UseDBSlave per una specifica azienda 
		///</summary>
		//---------------------------------------------------------------------
		public bool UpdateUseDBSlaveValue(string companyId, bool value)
		{
			bool success = false;
			
			SqlCommand myCommand = new SqlCommand();

			try
			{
				myCommand.Connection = CurrentSqlConnection;
				myCommand.CommandText = "UPDATE MSD_Companies SET UseDBSlave = @UseSlave WHERE CompanyId = @Id";
				myCommand.Parameters.AddWithValue("@Id", Int32.Parse(companyId));
				myCommand.Parameters.AddWithValue("@UseSlave", value);

				myCommand.ExecuteNonQuery();
				success = true;
			}
			catch (SqlException e)
			{
				ExtendedInfo extendedInfo = new ExtendedInfo();
				extendedInfo.Add(DatabaseLayerStrings.Description, e.Message);
				extendedInfo.Add(DatabaseLayerStrings.Procedure, e.Procedure);
				extendedInfo.Add(DatabaseLayerStrings.Server, e.Server);
				extendedInfo.Add(DatabaseLayerStrings.Number, e.Number);
				extendedInfo.Add(DatabaseLayerStrings.LineNumber, e.LineNumber);
				extendedInfo.Add(DatabaseLayerStrings.Function, "CompanyDb.UpdateUseDBSlaveValue");
				extendedInfo.Add(DatabaseLayerStrings.DefinedInto, "TaskBuilderNet.Data.DatabaseItems");
				Diagnostic.Set(DiagnosticType.Error, string.Format(DatabaseItemsStrings.ReadingTable, "MSD_Companies"), extendedInfo);
			}

			myCommand.Dispose();
			return success;
		}

		#region Funzioni di Ricerca 

		#region SelectCompaniesSameServerAndDb e GetCompaniesSameDataSourceAndDb
		/// <summary>
		/// SelectCompaniesSameServerAndDb
		/// </summary>
		//-------------------------------------------------------------------------------
		public bool SelectCompaniesSameServerAndDb(out ArrayList companies, string serverName, string dbName, string companyCurrent)
		{
			ArrayList localCompanies = new ArrayList();
			bool successFlag = false;
			
			try
			{
				SqlDataReader myDataReader;
				successFlag = GetCompaniesSameDataSourceAndDb(out myDataReader, dbName, serverName);
				
				if (successFlag)
				{
					while(myDataReader.Read())
					{
						if (string.Compare(myDataReader["CompanyId"].ToString(), companyCurrent, StringComparison.InvariantCultureIgnoreCase) == 0)
							continue;
						
						CompanyItem item				= new CompanyItem();
						item.CompanyId					= myDataReader["CompanyId"].ToString();
						item.Company					= myDataReader["Company"].ToString();
						item.DbName						= myDataReader["CompanyDBName"].ToString();
						item.DbOwner					= myDataReader["CompanyDBOwner"].ToString();
						item.DbServer					= myDataReader["CompanyDBServer"].ToString();
						item.Description				= myDataReader["Description"].ToString();
						item.ProviderId					= myDataReader["ProviderId"].ToString();
						item.UseTransaction				= bool.Parse(myDataReader["UseTransaction"].ToString());
						item.DBAuthenticationWindows	= bool.Parse(myDataReader["CompanyDBWindowsAuthentication"].ToString());
						item.PreferredLanguage			= myDataReader["PreferredLanguage"].ToString();
						item.ApplicationLanguage		= myDataReader["ApplicationLanguage"].ToString();
						item.Disabled					= bool.Parse(myDataReader["Disabled"].ToString());
						item.UseSecurity				= bool.Parse(myDataReader["UseSecurity"].ToString());
						item.UseAuditing				= bool.Parse(myDataReader["UseAuditing"].ToString());
						item.UseUnicode		            = bool.Parse(myDataReader["UseUnicode"].ToString());
						item.UseKeyedUpdate				= bool.Parse(myDataReader["UseKeyedUpdate"].ToString());
						item.IsValid					= bool.Parse(myDataReader["IsValid"].ToString());
						item.DatabaseCulture			= Convert.ToInt32(myDataReader["DatabaseCulture"].ToString());
						item.SupportColumnCollation		= bool.Parse(myDataReader["SupportColumnCollation"].ToString());
						item.Port						= Convert.ToInt32(myDataReader["Port"].ToString());
						item.UseDBSlave					= bool.Parse(myDataReader["UseDBSlave"].ToString());
						item.UseDataSynchro				= bool.Parse(myDataReader["UseDataSynchro"].ToString());
						item.UseRowSecurity				= bool.Parse(myDataReader["UseRowSecurity"].ToString());
						item.UseDataSynchro				= bool.Parse(myDataReader["UseDataSynchro"].ToString());
						localCompanies.Add(item);
					}
					myDataReader.Close();
				}
			}
			catch(SqlException sqlException)
			{
				ExtendedInfo extendedInfo = new ExtendedInfo();
				extendedInfo.Add(DatabaseLayerStrings.Description, sqlException.Message);
				extendedInfo.Add(DatabaseLayerStrings.Procedure,	sqlException.Procedure);
				extendedInfo.Add(DatabaseLayerStrings.Server,		sqlException.Server);
				extendedInfo.Add(DatabaseLayerStrings.Number,		sqlException.Number);
				extendedInfo.Add(DatabaseLayerStrings.LineNumber,	sqlException.LineNumber);
				extendedInfo.Add(DatabaseLayerStrings.Function,     "CompanyDb.SelectCompaniesSameServerAndDb");
				extendedInfo.Add(DatabaseLayerStrings.DefinedInto, "TaskBuilderNet.Data.DatabaseItems");
				Diagnostic.Set(DiagnosticType.Error, DatabaseItemsStrings.CompaniesReading, extendedInfo);
				companies = null;
				return successFlag;
			}

			companies = localCompanies;
			return successFlag;
		}

		/// <summary>
		/// GetCompaniesSameDataSourceAndDb
		/// </summary>
		//---------------------------------------------------------------------
		public bool GetCompaniesSameDataSourceAndDb(out SqlDataReader myDataReader, string dbName, string serverName)
		{
			SqlDataReader mylocalDataReader = null;
			string		  myQuery			= string.Empty;
			bool		  mySuccessFlag		= false;

			try
			{
				myQuery = @"SELECT * FROM MSD_Companies 
							WHERE MSD_Companies.CompanyDBServer = @ServerName 
							AND MSD_Companies.CompanyDBName = @DataSourceName
							ORDER BY MSD_Companies.Company";

				SqlCommand myCommand = new SqlCommand(myQuery, CurrentSqlConnection);
				myCommand.Parameters.AddWithValue("@ServerName", serverName);
				myCommand.Parameters.AddWithValue("@DataSourceName", dbName);
				mylocalDataReader = myCommand.ExecuteReader();
				mySuccessFlag = true;
			}
			catch(SqlException sqlException)
			{
				ExtendedInfo extendedInfo = new ExtendedInfo();
				extendedInfo.Add(DatabaseLayerStrings.Description, sqlException.Message);
				extendedInfo.Add(DatabaseLayerStrings.Procedure,	sqlException.Procedure);
				extendedInfo.Add(DatabaseLayerStrings.Server,		sqlException.Server);
				extendedInfo.Add(DatabaseLayerStrings.Number,		sqlException.Number);
				extendedInfo.Add(DatabaseLayerStrings.LineNumber,	sqlException.LineNumber);
				extendedInfo.Add(DatabaseLayerStrings.Function,     "CompanyDb.GetCompaniesSameDataSourceAndDb");
				extendedInfo.Add(DatabaseLayerStrings.DefinedInto, "TaskBuilderNet.Data.DatabaseItems");
				Diagnostic.Set(DiagnosticType.Error, string.Format(DatabaseItemsStrings.ReadingTable, "MSD_Companies"), extendedInfo);
				myDataReader = null;
				return mySuccessFlag;
			}

			myDataReader = mylocalDataReader;
			return mySuccessFlag;
		}
		#endregion

		/// <summary>
		#region SelectCompaniesSameDataSource - Seleziona tutte le aziende che hanno in comune lo stesso database
		/// SelectCompaniesSameDataSource
		/// </summary>
		//---------------------------------------------------------------------
		public bool SelectCompaniesSameDataSource(out ArrayList companiesOnSameDataSource, string dataSource, string server)
		{
			ArrayList localCompaniesSameDataSource = new ArrayList();
			
			try
			{
				SqlDataReader myDataReader;
				if (GetCompaniesSameDataSource(out myDataReader, dataSource, server))
				{
					while(myDataReader.Read())
					{
						CompanyItem item				= new CompanyItem();
						item.CompanyId					= myDataReader["CompanyId"].ToString();
						item.Company					= myDataReader["Company"].ToString();
						item.DbName						= myDataReader["CompanyDBName"].ToString();
						item.DbOwner					= myDataReader["CompanyDBOwner"].ToString();
						item.DbServer					= myDataReader["CompanyDBServer"].ToString();
						item.Description				= myDataReader["Description"].ToString();
						item.ProviderId					= myDataReader["ProviderId"].ToString();
						item.UseTransaction				= bool.Parse(myDataReader["UseTransaction"].ToString());
						item.DBAuthenticationWindows	= bool.Parse(myDataReader["CompanyDBWindowsAuthentication"].ToString());
						item.PreferredLanguage			= myDataReader["PreferredLanguage"].ToString();
						item.ApplicationLanguage		= myDataReader["ApplicationLanguage"].ToString();
						item.Disabled					= bool.Parse(myDataReader["Disabled"].ToString());
						item.UseSecurity				= bool.Parse(myDataReader["UseSecurity"].ToString());
						item.UseAuditing				= bool.Parse(myDataReader["UseAuditing"].ToString());
						item.UseUnicode		            = bool.Parse(myDataReader["UseUnicode"].ToString());
						item.UseKeyedUpdate				= bool.Parse(myDataReader["UseKeyedUpdate"].ToString());
						item.IsValid					= bool.Parse(myDataReader["IsValid"].ToString());
						item.DatabaseCulture			= Convert.ToInt32(myDataReader["DatabaseCulture"].ToString());
						item.SupportColumnCollation		= bool.Parse(myDataReader["SupportColumnCollation"].ToString());
						item.Port						= Convert.ToInt32(myDataReader["Port"].ToString());
						item.UseDBSlave					= bool.Parse(myDataReader["UseDBSlave"].ToString());
						item.UseDataSynchro				= bool.Parse(myDataReader["UseDataSynchro"].ToString());
						item.UseRowSecurity				= bool.Parse(myDataReader["UseRowSecurity"].ToString());
						item.UseDataSynchro				= bool.Parse(myDataReader["UseDataSynchro"].ToString());
						localCompaniesSameDataSource.Add(item);
					}
					if (myDataReader != null && !myDataReader.IsClosed)
					{
						myDataReader.Close();
						myDataReader.Dispose();
					}
				}
			}
			catch(SqlException sqlException)
			{
				ExtendedInfo extendedInfo = new ExtendedInfo();
				extendedInfo.Add(DatabaseLayerStrings.Description, sqlException.Message);
				extendedInfo.Add(DatabaseLayerStrings.Procedure,	sqlException.Procedure);
				extendedInfo.Add(DatabaseLayerStrings.Server,		sqlException.Server);
				extendedInfo.Add(DatabaseLayerStrings.Number,		sqlException.Number);
				extendedInfo.Add(DatabaseLayerStrings.LineNumber,	sqlException.LineNumber);
				extendedInfo.Add(DatabaseLayerStrings.Function,     "CompanyDb.SelectCompaniesSameDataSource");
				extendedInfo.Add(DatabaseLayerStrings.DefinedInto, "TaskBuilderNet.Data.DatabaseItems");
				Diagnostic.Set(DiagnosticType.Error, DatabaseItemsStrings.CompaniesReading, extendedInfo);
				companiesOnSameDataSource = null;
				return false;
			}

			companiesOnSameDataSource = localCompaniesSameDataSource;
			return true;
		}

		/// <summary>
		/// GetCompaniesSameDataSource
		/// </summary>
		//---------------------------------------------------------------------
		public bool GetCompaniesSameDataSource(out SqlDataReader myDataReader, string dataSourceName, string serverName)
		{
			SqlDataReader mylocalDataReader = null;

			try
			{
				string myQuery = @"SELECT * FROM MSD_Companies 
							WHERE MSD_Companies.CompanyDBServer = @ServerName AND MSD_Companies.CompanyDBName = @DataSourceName
							ORDER BY MSD_Companies.Company";

				SqlCommand myCommand = new SqlCommand(myQuery, CurrentSqlConnection);
				myCommand.Parameters.AddWithValue("@ServerName", serverName);
				myCommand.Parameters.AddWithValue("@DataSourceName", dataSourceName);
				mylocalDataReader = myCommand.ExecuteReader();
			}
			catch(SqlException sqlException)
			{
				ExtendedInfo extendedInfo = new ExtendedInfo();
				extendedInfo.Add(DatabaseLayerStrings.Description, sqlException.Message);
				extendedInfo.Add(DatabaseLayerStrings.Procedure,	sqlException.Procedure);
				extendedInfo.Add(DatabaseLayerStrings.Server,		sqlException.Server);
				extendedInfo.Add(DatabaseLayerStrings.Number,		sqlException.Number);
				extendedInfo.Add(DatabaseLayerStrings.LineNumber,	sqlException.LineNumber);
				extendedInfo.Add(DatabaseLayerStrings.Function,     "CompanyDb.GetCompaniesSameDataSource");
				extendedInfo.Add(DatabaseLayerStrings.DefinedInto, "TaskBuilderNet.Data.DatabaseItems");
				Diagnostic.Set(DiagnosticType.Error, string.Format(DatabaseItemsStrings.ReadingTable, "MSD_Companies"), extendedInfo);
				myDataReader = null;
				return false;
			}
			
			myDataReader = mylocalDataReader;
			return true;
		}
		#endregion

		#region SelectCompaniesSameServer - Seleziona tutte le aziende che risiedono su uno stesso server
		/// <summary>
		/// GetCompaniesSameServer
		/// </summary>
		//---------------------------------------------------------------------
		public bool SelectCompaniesSameServer(out ArrayList companiesOnSameServer, string dbCompanyServer)
		{
			ArrayList localCompaniesSameServer = new ArrayList();
			
			try
			{
				SqlDataReader myDataReader;
				if (GetCompaniesSameServer(out myDataReader, dbCompanyServer))
				{
					while(myDataReader.Read())
					{
						CompanyItem item				= new CompanyItem();
						item.CompanyId					= myDataReader["CompanyId"].ToString();
						item.Company					= myDataReader["Company"].ToString();
						item.DbName						= myDataReader["CompanyDBName"].ToString();
						item.DbOwner					= myDataReader["CompanyDBOwner"].ToString();
						item.DbServer					= myDataReader["CompanyDBServer"].ToString();
						item.Description				= myDataReader["Description"].ToString();
						item.ProviderId					= myDataReader["ProviderId"].ToString();
						item.UseTransaction				= bool.Parse(myDataReader["UseTransaction"].ToString());
						item.DBAuthenticationWindows	= bool.Parse(myDataReader["CompanyDBWindowsAuthentication"].ToString());
						item.PreferredLanguage			= myDataReader["PreferredLanguage"].ToString();
						item.ApplicationLanguage		= myDataReader["ApplicationLanguage"].ToString();
						item.Disabled					= bool.Parse(myDataReader["Disabled"].ToString());
						item.UseSecurity				= bool.Parse(myDataReader["UseSecurity"].ToString());
						item.UseAuditing				= bool.Parse(myDataReader["UseAuditing"].ToString());
						item.UseUnicode					= bool.Parse(myDataReader["UseUnicode"].ToString());
						item.UseKeyedUpdate				= bool.Parse(myDataReader["UseKeyedUpdate"].ToString());
						item.IsValid					= bool.Parse(myDataReader["IsValid"].ToString());
						item.DatabaseCulture			= Convert.ToInt32(myDataReader["DatabaseCulture"].ToString());
						item.SupportColumnCollation		= bool.Parse(myDataReader["SupportColumnCollation"].ToString());
						item.Port						= Convert.ToInt32(myDataReader["Port"].ToString());
						item.UseDBSlave					= bool.Parse(myDataReader["UseDBSlave"].ToString());
						item.UseDataSynchro				= bool.Parse(myDataReader["UseDataSynchro"].ToString());
						item.UseRowSecurity				= bool.Parse(myDataReader["UseRowSecurity"].ToString());
						item.UseDataSynchro				= bool.Parse(myDataReader["UseDataSynchro"].ToString());
						localCompaniesSameServer.Add(item);
					}
					if (myDataReader != null && !myDataReader.IsClosed)
					{
						myDataReader.Close();
						myDataReader.Dispose();
					}
				}
			}
			catch(SqlException sqlException)
			{
				ExtendedInfo extendedInfo = new ExtendedInfo();
				extendedInfo.Add(DatabaseLayerStrings.Description, sqlException.Message);
				extendedInfo.Add(DatabaseLayerStrings.Procedure,	sqlException.Procedure);
				extendedInfo.Add(DatabaseLayerStrings.Server,		sqlException.Server);
				extendedInfo.Add(DatabaseLayerStrings.Number,		sqlException.Number);
				extendedInfo.Add(DatabaseLayerStrings.LineNumber,	sqlException.LineNumber);
				extendedInfo.Add(DatabaseLayerStrings.Function,     "CompanyDb.SelectCompaniesSameServer");
				extendedInfo.Add(DatabaseLayerStrings.DefinedInto, "TaskBuilderNet.Data.DatabaseItems");
				Diagnostic.Set(DiagnosticType.Error, DatabaseItemsStrings.CompaniesReading, extendedInfo);
				companiesOnSameServer = null;
				return false;
			}
			
			companiesOnSameServer = localCompaniesSameServer;
			return true;
		}

		/// <summary>
		/// GetCompaniesSameServer
		/// </summary>
		//---------------------------------------------------------------------
		public bool GetCompaniesSameServer(out SqlDataReader myDataReader, string serverName)
		{
			SqlDataReader mylocalDataReader = null;

			try
			{
				string myQuery = @"SELECT * FROM MSD_Companies WHERE MSD_Companies.CompanyDBServer = @ServerName
								  ORDER BY MSD_Companies.Company";

				SqlCommand myCommand = new SqlCommand(myQuery, CurrentSqlConnection);
				myCommand.Parameters.AddWithValue("@ServerName", serverName);
				mylocalDataReader = myCommand.ExecuteReader();
			}
			catch(SqlException sqlException)
			{
				ExtendedInfo extendedInfo = new ExtendedInfo();
				extendedInfo.Add(DatabaseLayerStrings.Description, sqlException.Message);
				extendedInfo.Add(DatabaseLayerStrings.Procedure,	sqlException.Procedure);
				extendedInfo.Add(DatabaseLayerStrings.Server,		sqlException.Server);
				extendedInfo.Add(DatabaseLayerStrings.Number,		sqlException.Number);
				extendedInfo.Add(DatabaseLayerStrings.LineNumber,	sqlException.LineNumber);
				extendedInfo.Add(DatabaseLayerStrings.Function,     "CompanyDb.GetCompaniesSameServer");
				extendedInfo.Add(DatabaseLayerStrings.DefinedInto, "TaskBuilderNet.Data.DatabaseItems");
				Diagnostic.Set(DiagnosticType.Error, string.Format(DatabaseItemsStrings.ReadingTable, "MSD_Companies"), extendedInfo);
				myDataReader = null;
				return false;
			}
			
			myDataReader = mylocalDataReader;
			return true;
		}
		#endregion
		
		#region SelectAllOracleCompanies - Seleziona tutte le aziende Oracle
		/// <summary>
		/// SelectAllOracleCompanies
		/// </summary>
		//---------------------------------------------------------------------
		public bool SelectAllOracleCompanies(out ArrayList oracleCompanies)
		{
			ArrayList localCompanies = new ArrayList();

			try
			{
				SqlDataReader myDataReader;
				
				if (GetAllOracleCompanies(out myDataReader))
				{
					while(myDataReader.Read())
					{
						CompanyItem item	= new CompanyItem();
						item.CompanyId		= myDataReader["CompanyId"].ToString();
						item.DbName			= myDataReader["CompanyDBName"].ToString();
						localCompanies.Add(item);
					}
					if (myDataReader != null && !myDataReader.IsClosed)
					{
						myDataReader.Close();
						myDataReader.Dispose();
					}
				}
			}
			catch(SqlException sqlException)
			{
				ExtendedInfo extendedInfo = new ExtendedInfo();
				extendedInfo.Add(DatabaseLayerStrings.Description, sqlException.Message);
				extendedInfo.Add(DatabaseLayerStrings.Procedure,	sqlException.Procedure);
				extendedInfo.Add(DatabaseLayerStrings.Server,		sqlException.Server);
				extendedInfo.Add(DatabaseLayerStrings.Number,		sqlException.Number);
				extendedInfo.Add(DatabaseLayerStrings.LineNumber,	sqlException.LineNumber);
				extendedInfo.Add(DatabaseLayerStrings.Function,     "CompanyDb.SelectAllOracleCompanies");
				extendedInfo.Add(DatabaseLayerStrings.DefinedInto, "TaskBuilderNet.Data.DatabaseItems");
				Diagnostic.Set(DiagnosticType.Error, DatabaseItemsStrings.CompaniesReading, extendedInfo);
				oracleCompanies = null;
				return false;
			}
			
			oracleCompanies = localCompanies;
			return true;
		}

		/// <summary>
		/// GetAllOracleCompanies
		/// </summary>
		//---------------------------------------------------------------------
		public bool GetAllOracleCompanies(out SqlDataReader myDataReader)
		{
			SqlDataReader mylocalDataReader = null;

			try
			{
				string myQuery = @"SELECT MSD_Companies.CompanyId, MSD_Companies.CompanyDBName
							FROM MSD_Companies, MSD_Providers
							WHERE MSD_Companies.ProviderId = MSD_Providers.ProviderId AND MSD_Providers.Provider = @OracleProvider";
			  
				SqlCommand myCommand = new SqlCommand(myQuery, CurrentSqlConnection);
				myCommand.Parameters.AddWithValue("@OracleProvider", NameSolverDatabaseStrings.OraOLEDBProvider);
				mylocalDataReader = myCommand.ExecuteReader();
			}
			catch(SqlException sqlException)
			{
				ExtendedInfo extendedInfo = new ExtendedInfo();
				extendedInfo.Add(DatabaseLayerStrings.Description, sqlException.Message);
				extendedInfo.Add(DatabaseLayerStrings.Procedure,		sqlException.Procedure);
				extendedInfo.Add(DatabaseLayerStrings.Server,		sqlException.Server);
				extendedInfo.Add(DatabaseLayerStrings.Number,		sqlException.Number);
				extendedInfo.Add(DatabaseLayerStrings.LineNumber,	sqlException.LineNumber);
				extendedInfo.Add(DatabaseLayerStrings.Function,		"CompanyDb.GetAllOracleCompanies");
				extendedInfo.Add(DatabaseLayerStrings.DefinedInto, "TaskBuilderNet.Data.DatabaseItems");
				Diagnostic.Set(DiagnosticType.Error, string.Format(DatabaseItemsStrings.ReadingTable, "MSD_Companies"), extendedInfo);
				myDataReader = null;
				return false;
			}

			myDataReader = mylocalDataReader;
			return true;
		}
		# endregion

		#region SelectAllCompanies - Seleziona tutte le Aziende di SQL Server
		/// <summary>
		/// Seleziona tutte le aziende trovate in MSD_Companies e restituisce il tutto in un array
		/// </summary>
		/// <param name="companies">ArrayList con le aziende trovate</param>
		/// <returns>mySuccessFlag, true se è andato tutto bene</returns>
		//---------------------------------------------------------------------
		public bool SelectAllCompanies(out ArrayList companies)
		{
			ArrayList localCompanies = new ArrayList();
			SqlDataReader myDataReader = null;
            try
            {
                if (GetAllCompanies(out myDataReader))
                {
                    while (myDataReader.Read())
                    {
                        CompanyItem item			= new CompanyItem();
                        item.CompanyId				= myDataReader["CompanyId"].ToString();
                        item.Company				= myDataReader["Company"].ToString();
                        item.DbName					= myDataReader["CompanyDBName"].ToString();
                        item.DbOwner				= myDataReader["CompanyDBOwner"].ToString();
                        item.DbServer				= myDataReader["CompanyDBServer"].ToString();
                        item.Description			= myDataReader["Description"].ToString();
                        item.ProviderId				= myDataReader["ProviderId"].ToString();
                        item.Provider				= myDataReader["Provider"].ToString();
                        item.UseTransaction			= bool.Parse(myDataReader["UseTransaction"].ToString());
                        item.DBAuthenticationWindows = bool.Parse(myDataReader["CompanyDBWindowsAuthentication"].ToString());
                        item.PreferredLanguage		= myDataReader["PreferredLanguage"].ToString();
                        item.ApplicationLanguage	= myDataReader["ApplicationLanguage"].ToString();
                        item.Disabled				= bool.Parse(myDataReader["Disabled"].ToString());
                        item.UseSecurity			= bool.Parse(myDataReader["UseSecurity"].ToString());
                        item.UseAuditing			= bool.Parse(myDataReader["UseAuditing"].ToString());
                        item.UseUnicode				= bool.Parse(myDataReader["UseUnicode"].ToString());
                        item.UseKeyedUpdate			= bool.Parse(myDataReader["UseKeyedUpdate"].ToString());
                        item.IsValid				= bool.Parse(myDataReader["IsValid"].ToString());
                        item.DatabaseCulture		= Convert.ToInt32(myDataReader["DatabaseCulture"].ToString());
                        item.SupportColumnCollation = bool.Parse(myDataReader["SupportColumnCollation"].ToString());
                        item.Port					= Convert.ToInt32(myDataReader["Port"].ToString());
                        item.UseDBSlave				= bool.Parse(myDataReader["UseDBSlave"].ToString());
						item.UseDataSynchro			= bool.Parse(myDataReader["UseDataSynchro"].ToString());
                        item.UseRowSecurity			= bool.Parse(myDataReader["UseRowSecurity"].ToString());
						item.UseDataSynchro			= bool.Parse(myDataReader["UseDataSynchro"].ToString());
						localCompanies.Add(item);
                    }
                }
            }
            catch (SqlException sqlException)
            {
                ExtendedInfo extendedInfo = new ExtendedInfo();
                extendedInfo.Add(DatabaseLayerStrings.Description, sqlException.Message);
                extendedInfo.Add(DatabaseLayerStrings.Procedure, sqlException.Procedure);
                extendedInfo.Add(DatabaseLayerStrings.Server, sqlException.Server);
                extendedInfo.Add(DatabaseLayerStrings.Number, sqlException.Number);
                extendedInfo.Add(DatabaseLayerStrings.LineNumber, sqlException.LineNumber);
                extendedInfo.Add(DatabaseLayerStrings.Function, "CompanyDb.SelectAllCompanies");
                extendedInfo.Add(DatabaseLayerStrings.DefinedInto, "TaskBuilderNet.Data.DatabaseItems");
                Diagnostic.Set(DiagnosticType.Error, DatabaseItemsStrings.CompaniesReading, extendedInfo);
                companies = localCompanies;

                return false;
            }
            catch (Exception exception)
            {
                ExtendedInfo extendedInfo = new ExtendedInfo();
                extendedInfo.Add(DatabaseLayerStrings.Description, exception.Message);
                extendedInfo.Add(DatabaseLayerStrings.Function, "CompanyDb.SelectAllCompanies");
                extendedInfo.Add(DatabaseLayerStrings.DefinedInto, "TaskBuilderNet.Data.DatabaseItems");
                Diagnostic.Set(DiagnosticType.Error, DatabaseItemsStrings.CompaniesReading, extendedInfo);
                companies = localCompanies;

                return false;
            }
            finally
            {
                if (myDataReader != null)
                {
                    myDataReader.Dispose();
                }
            }
			
			companies = localCompanies;
			return true;
		}

		/// <summary>
		/// Riempie un dataReader con tutte le aziende trovate in MSD_Companies
		/// </summary>
		/// <param name="myDataReader">dataReader con i dati delle aziende</param>
		/// <returns>mySuccessFlag, true se è andato tutto bene</returns>
		//---------------------------------------------------------------------
		public bool GetAllCompanies(out SqlDataReader myDataReader)
		{
			SqlDataReader mylocalDataReader = null;

			try
			{
				string myQuery = @"SELECT *, MSD_Providers.Provider FROM MSD_Companies, MSD_Providers
						  WHERE MSD_Companies.ProviderId = MSD_Providers.ProviderId
						  ORDER BY MSD_Companies.Company";

				using (SqlCommand myCommand = new SqlCommand(myQuery, CurrentSqlConnection))
					mylocalDataReader = myCommand.ExecuteReader();
			}
			catch(SqlException sqlException)
			{
				ExtendedInfo extendedInfo = new ExtendedInfo();
				extendedInfo.Add(DatabaseLayerStrings.Description, sqlException.Message);
				extendedInfo.Add(DatabaseLayerStrings.Procedure,	sqlException.Procedure);
				extendedInfo.Add(DatabaseLayerStrings.Server,		sqlException.Server);
				extendedInfo.Add(DatabaseLayerStrings.Number,		sqlException.Number);
				extendedInfo.Add(DatabaseLayerStrings.LineNumber,	sqlException.LineNumber);
				extendedInfo.Add(DatabaseLayerStrings.Function,     "CompanyDb.GetAllCompanies");
				extendedInfo.Add(DatabaseLayerStrings.DefinedInto, "TaskBuilderNet.Data.DatabaseItems");
				Diagnostic.Set(DiagnosticType.Error, string.Format(DatabaseItemsStrings.ReadingTable, "MSD_Companies"), extendedInfo);
				myDataReader = null;
				return false;
			}

			myDataReader = mylocalDataReader;
			return true;
		}
		#endregion

		#region SelectCompanyByDesc - Seleziona una Azienda dato il suo Nome
		/// <summary>
		/// Seleziona una azienda dato il nome e restituisce i suoi dati in un array
		/// </summary>
		/// <param name="companies">Array con i dati dell'azienda</param>
		/// <param name="desc">Nome dell'azienda da cercare</param>
		/// <returns>mySuccessFlag, true se è andato tutto bene </returns>
		//---------------------------------------------------------------------
		public bool SelectCompanyByDesc(out ArrayList companies, string desc)
		{
			ArrayList localCompanies = new ArrayList();
			
			try
			{
				SqlDataReader myDataReader;
				if (GetCompanyByDesc(out myDataReader, desc))
				{
					while(myDataReader.Read())
					{
						CompanyItem item				= new CompanyItem();
						item.CompanyId					= myDataReader["CompanyId"].ToString();
						item.Company					= myDataReader["Company"].ToString();
						item.DbName						= myDataReader["CompanyDBName"].ToString();
						item.DbOwner					= myDataReader["CompanyDBOwner"].ToString();
						item.DbServer					= myDataReader["CompanyDBServer"].ToString();
						item.Description				= myDataReader["Description"].ToString();
						item.ProviderId					= myDataReader["ProviderId"].ToString();
						item.UseTransaction				= bool.Parse(myDataReader["UseTransaction"].ToString());
						item.DBAuthenticationWindows	= bool.Parse(myDataReader["CompanyDBWindowsAuthentication"].ToString());
						item.PreferredLanguage			= myDataReader["PreferredLanguage"].ToString();
						item.ApplicationLanguage		= myDataReader["ApplicationLanguage"].ToString();
						item.UseKeyedUpdate				= bool.Parse(myDataReader["UseKeyedUpdate"].ToString());
						item.UseSecurity				= bool.Parse(myDataReader["UseSecurity"].ToString());
						item.UseAuditing				= bool.Parse(myDataReader["UseAuditing"].ToString());
						item.UseUnicode					= bool.Parse(myDataReader["UseUnicode"].ToString());
						item.Disabled					= bool.Parse(myDataReader["Disabled"].ToString());
						item.IsValid					= bool.Parse(myDataReader["IsValid"].ToString());
						item.DatabaseCulture			= Convert.ToInt32(myDataReader["DatabaseCulture"].ToString());
						item.SupportColumnCollation		= bool.Parse(myDataReader["SupportColumnCollation"].ToString());
						item.Port						= Convert.ToInt32(myDataReader["Port"].ToString());
						item.UseDBSlave					= bool.Parse(myDataReader["UseDBSlave"].ToString());
						item.UseDataSynchro				= bool.Parse(myDataReader["UseDataSynchro"].ToString());
						item.UseRowSecurity				= bool.Parse(myDataReader["UseRowSecurity"].ToString());
						item.UseDataSynchro				= bool.Parse(myDataReader["UseDataSynchro"].ToString());
						localCompanies.Add(item);
					}

					if (myDataReader != null && !myDataReader.IsClosed)
					{
						myDataReader.Close();
						myDataReader.Dispose();
					}
				}
			}
			catch(SqlException sqlException)
			{
				ExtendedInfo extendedInfo = new ExtendedInfo();
				extendedInfo.Add(DatabaseLayerStrings.Description, sqlException.Message);
				extendedInfo.Add(DatabaseLayerStrings.Procedure,	sqlException.Procedure);
				extendedInfo.Add(DatabaseLayerStrings.Server,		sqlException.Server);
				extendedInfo.Add(DatabaseLayerStrings.Number,		sqlException.Number);
				extendedInfo.Add(DatabaseLayerStrings.LineNumber,	sqlException.LineNumber);
				extendedInfo.Add(DatabaseLayerStrings.Function,     "CompanyDb.SelectCompanyByDesc");
				extendedInfo.Add(DatabaseLayerStrings.DefinedInto, "TaskBuilderNet.Data.DatabaseItems");
				Diagnostic.Set(DiagnosticType.Error, DatabaseItemsStrings.CompaniesReading, extendedInfo);
				companies = null;
				return false;
			}
			
			companies = localCompanies;
			return true;
		}
		
		/// <summary>
		/// Riempie un datareader con i dati dell'azienda identificata dal nome
		/// </summary>
		/// <param name="myDataReader">datareader con i dati dell'azienda</param>
		/// <param name="Company">Nome dell'azienda da cercare</param>
		/// <returns>mySuccessFlag, true se è andato tutto bene</returns>
		//---------------------------------------------------------------------
		public bool GetCompanyByDesc(out SqlDataReader myDataReader, string Company)
		{
			SqlDataReader mylocalDataReader = null;

			try
			{
				string myQuery = "SELECT * FROM MSD_Companies WHERE Company = @Company ORDER BY MSD_Companies.Company";
				SqlCommand myCommand = new SqlCommand(myQuery, CurrentSqlConnection);
				myCommand.Parameters.AddWithValue("@Company", Company);
				mylocalDataReader = myCommand.ExecuteReader();
			}
			catch(SqlException sqlException)
			{
				ExtendedInfo extendedInfo = new ExtendedInfo();
				extendedInfo.Add(DatabaseLayerStrings.Description, sqlException.Message);
				extendedInfo.Add(DatabaseLayerStrings.Procedure,	sqlException.Procedure);
				extendedInfo.Add(DatabaseLayerStrings.Server,		sqlException.Server);
				extendedInfo.Add(DatabaseLayerStrings.Number,		sqlException.Number);
				extendedInfo.Add(DatabaseLayerStrings.LineNumber,	sqlException.LineNumber);
				extendedInfo.Add(DatabaseLayerStrings.Function,     "CompanyDb.GetCompanyByDesc");
				extendedInfo.Add(DatabaseLayerStrings.DefinedInto, "TaskBuilderNet.Data.DatabaseItems");
				Diagnostic.Set(DiagnosticType.Error, string.Format(DatabaseItemsStrings.ReadingTable, "MSD_Companies"), extendedInfo);
				myDataReader = null;
				return false;
			}

			myDataReader = mylocalDataReader;
			return true;
		}
		#endregion

		#region SelectCompanyByDbName - Seleziona una Azienda dato il suo DbName
		/// <summary>
		/// Seleziona una azienda dato il nome e restituisce i suoi dati in un array
		/// </summary>
		/// <param name="companies">Array con i dati dell'azienda</param>
		/// <param name="desc">Nome dell'azienda da cercare</param>
		/// <returns>mySuccessFlag, true se è andato tutto bene </returns>
		//---------------------------------------------------------------------
		public bool SelectCompanyByDbName(out ArrayList companies, string dbName)
		{
			ArrayList localCompanies = new ArrayList();
			
			try
			{
				SqlDataReader myDataReader;
				if (GetCompanyByDbName(out myDataReader, dbName))
				{
					while(myDataReader.Read())
					{
						CompanyItem item				= new CompanyItem();
						item.CompanyId					= myDataReader["CompanyId"].ToString();
						item.Company					= myDataReader["Company"].ToString();
						item.DbName						= myDataReader["CompanyDBName"].ToString();
						item.DbOwner					= myDataReader["CompanyDBOwner"].ToString();
						item.DbServer					= myDataReader["CompanyDBServer"].ToString();
						item.Description				= myDataReader["Description"].ToString();
						item.ProviderId					= myDataReader["ProviderId"].ToString();
						item.UseTransaction				= bool.Parse(myDataReader["UseTransaction"].ToString());
						item.DBAuthenticationWindows	= bool.Parse(myDataReader["CompanyDBWindowsAuthentication"].ToString());
						item.PreferredLanguage			= myDataReader["PreferredLanguage"].ToString();
						item.ApplicationLanguage		= myDataReader["ApplicationLanguage"].ToString();
						item.UseKeyedUpdate				= bool.Parse(myDataReader["UseKeyedUpdate"].ToString());
						item.UseSecurity				= bool.Parse(myDataReader["UseSecurity"].ToString());
						item.UseAuditing				= bool.Parse(myDataReader["UseAuditing"].ToString());
						item.UseUnicode					= bool.Parse(myDataReader["UseUnicode"].ToString());
						item.Disabled					= bool.Parse(myDataReader["Disabled"].ToString());
						item.IsValid					= bool.Parse(myDataReader["IsValid"].ToString());
						item.DatabaseCulture			= Convert.ToInt32(myDataReader["DatabaseCulture"].ToString());
						item.SupportColumnCollation		= bool.Parse(myDataReader["SupportColumnCollation"].ToString());
						item.Port						= Convert.ToInt32(myDataReader["Port"].ToString());
						item.UseDBSlave					= bool.Parse(myDataReader["UseDBSlave"].ToString());
						item.UseDataSynchro				= bool.Parse(myDataReader["UseDataSynchro"].ToString());
						item.UseRowSecurity				= bool.Parse(myDataReader["UseRowSecurity"].ToString());
						item.UseDataSynchro				= bool.Parse(myDataReader["UseDataSynchro"].ToString());
						localCompanies.Add(item);
					}
					if (myDataReader != null && !myDataReader.IsClosed)
					{
						myDataReader.Close();
						myDataReader.Dispose();
					}
				}
			}
			catch(SqlException sqlException)
			{
				ExtendedInfo extendedInfo = new ExtendedInfo();
				extendedInfo.Add(DatabaseLayerStrings.Description, sqlException.Message);
				extendedInfo.Add(DatabaseLayerStrings.Procedure,	sqlException.Procedure);
				extendedInfo.Add(DatabaseLayerStrings.Server,		sqlException.Server);
				extendedInfo.Add(DatabaseLayerStrings.Number,		sqlException.Number);
				extendedInfo.Add(DatabaseLayerStrings.LineNumber,	sqlException.LineNumber);
				extendedInfo.Add(DatabaseLayerStrings.Function,     "CompanyDb.SelectCompanyByDbName");
				extendedInfo.Add(DatabaseLayerStrings.DefinedInto, "TaskBuilderNet.Data.DatabaseItems");
				Diagnostic.Set(DiagnosticType.Error, DatabaseItemsStrings.CompaniesReading, extendedInfo);
				companies = null;
				return false;
			}
			
			companies = localCompanies;
			return true;
		}
		
		/// <summary>
		/// Riempie un datareader con i dati dell'azienda identificata dal nome
		/// </summary>
		/// <param name="myDataReader">datareader con i dati dell'azienda</param>
		/// <param name="Company">Nome dell'azienda da cercare</param>
		/// <returns>mySuccessFlag, true se è andato tutto bene</returns>
		//---------------------------------------------------------------------
		public bool GetCompanyByDbName(out SqlDataReader myDataReader, string DbName)
		{
			SqlDataReader mylocalDataReader = null;

			try
			{
				string myQuery = "SELECT * FROM MSD_Companies WHERE CompanyDBName = @DbName ORDER BY MSD_Companies.Company";
				SqlCommand myCommand = new SqlCommand(myQuery, CurrentSqlConnection);
				myCommand.Parameters.AddWithValue("@DbName", DbName);
				mylocalDataReader = myCommand.ExecuteReader();
			}
			catch(SqlException sqlException)
			{
				ExtendedInfo extendedInfo = new ExtendedInfo();
				extendedInfo.Add(DatabaseLayerStrings.Description, sqlException.Message);
				extendedInfo.Add(DatabaseLayerStrings.Procedure,	sqlException.Procedure);
				extendedInfo.Add(DatabaseLayerStrings.Server,		sqlException.Server);
				extendedInfo.Add(DatabaseLayerStrings.Number,		sqlException.Number);
				extendedInfo.Add(DatabaseLayerStrings.LineNumber,	sqlException.LineNumber);
				extendedInfo.Add(DatabaseLayerStrings.Function,     "CompanyDb.GetCompanyByDbName");
				extendedInfo.Add(DatabaseLayerStrings.DefinedInto, "TaskBuilderNet.Data.DatabaseItems");
				Diagnostic.Set(DiagnosticType.Error, string.Format(DatabaseItemsStrings.ReadingTable, "MSD_Companies"), extendedInfo);
				myDataReader = null;
				return false;
			}

			myDataReader = mylocalDataReader;
			return true;
		}
		#endregion

		#region GetAllCompanyFieldsById - Seleziona una Azienda dato il suo CompanyId
		/// <summary>
		/// Seleziona i dati di un'azienda attraverso il suo CompanyId
		/// </summary>
		/// <param name="company">Array con i dati dell'azienda</param>
		/// <param name="companyId">Id che identifica l'azienda</param>
		/// <returns>mySuccessFlag, true se è andato tutto bene</returns>
		//---------------------------------------------------------------------
		public bool GetAllCompanyFieldsById(out ArrayList company, string companyId)
		{
			ArrayList	localCompany	= new ArrayList();
			CompanyItem companyItem		= new CompanyItem();
			
			try
			{
				SqlDataReader myDataReader;
				if (GetCompanyId(out myDataReader, companyId))
				{
					while(myDataReader.Read())
					{
						companyItem.CompanyId				= myDataReader["CompanyId"].ToString();
						companyItem.Company					= myDataReader["Company"].ToString();
						companyItem.Description				= myDataReader["Description"].ToString();
						companyItem.DbDefaultPassword		= Crypto.Decrypt(myDataReader["DBDefaultPassword"].ToString());
						companyItem.DbDefaultUser			= myDataReader["DBDefaultUser"].ToString();
						companyItem.DbName					= myDataReader["CompanyDBName"].ToString();
						companyItem.DbOwner					= myDataReader["CompanyDBOwner"].ToString();
						companyItem.DbServer				= myDataReader["CompanyDBServer"].ToString();
						companyItem.UseAuditing				= bool.Parse(myDataReader["UseAuditing"].ToString());
						companyItem.UseSecurity				= bool.Parse(myDataReader["UseSecurity"].ToString());
						companyItem.ProviderId				= myDataReader["ProviderId"].ToString();
						companyItem.Provider                = myDataReader["Provider"].ToString();
						companyItem.ProviderDesc            = myDataReader["ProviderDesc"].ToString();
						companyItem.UseTransaction			= bool.Parse(myDataReader["UseTransaction"].ToString());
						companyItem.UseKeyedUpdate          = bool.Parse(myDataReader["UseKeyedUpdate"].ToString());
						companyItem.DBAuthenticationWindows = bool.Parse(myDataReader["CompanyDBWindowsAuthentication"].ToString());
						companyItem.PreferredLanguage		= myDataReader["PreferredLanguage"].ToString();
						companyItem.ApplicationLanguage     = myDataReader["ApplicationLanguage"].ToString();
						companyItem.Disabled                = bool.Parse(myDataReader["Disabled"].ToString());
						companyItem.UseUnicode              = bool.Parse(myDataReader["UseUnicode"].ToString());
						companyItem.IsValid                 = bool.Parse(myDataReader["IsValid"].ToString());
						companyItem.Updating				= bool.Parse(myDataReader["Updating"].ToString());
						companyItem.DatabaseCulture			= Convert.ToInt32(myDataReader["DatabaseCulture"].ToString());
						companyItem.SupportColumnCollation	= bool.Parse(myDataReader["SupportColumnCollation"].ToString());
						companyItem.Port					= Convert.ToInt32(myDataReader["Port"].ToString());
						companyItem.UseDBSlave				= bool.Parse(myDataReader["UseDBSlave"].ToString());
						companyItem.UseDataSynchro			= bool.Parse(myDataReader["UseDataSynchro"].ToString());
						companyItem.UseRowSecurity			= bool.Parse(myDataReader["UseRowSecurity"].ToString());
						companyItem.UseDataSynchro			= bool.Parse(myDataReader["UseDataSynchro"].ToString());
						localCompany.Add(companyItem);
					}
					if (myDataReader != null && !myDataReader.IsClosed)
					{
						myDataReader.Close();
						myDataReader.Dispose();
					}
				}
			}
			catch(SqlException sqlException)
			{
				ExtendedInfo extendedInfo = new ExtendedInfo();
				extendedInfo.Add(DatabaseLayerStrings.Description, sqlException.Message);
				extendedInfo.Add(DatabaseLayerStrings.Procedure,	sqlException.Procedure);
				extendedInfo.Add(DatabaseLayerStrings.Server,		sqlException.Server);
				extendedInfo.Add(DatabaseLayerStrings.Number,		sqlException.Number);
				extendedInfo.Add(DatabaseLayerStrings.LineNumber,	sqlException.LineNumber);
				extendedInfo.Add(DatabaseLayerStrings.Function,     "CompanyDb.GetAllCompanyFieldsById");
				extendedInfo.Add(DatabaseLayerStrings.DefinedInto, "TaskBuilderNet.Data.DatabaseItems");
				Diagnostic.Set(DiagnosticType.Error, DatabaseItemsStrings.CompaniesReading, extendedInfo);
				company = null;
				return false;
			}
			
			company = localCompany;
			return true;
		}
		
		/// <summary>
		/// Riempie un dataReader con tutti i dati dell'azienda più quelli relativi al suo provider
		/// </summary>
		/// <param name="myDataReader">DataReader con i dati azienda</param>
		/// <param name="CompanyId">Id che identifica l'azienda</param>
		/// <returns>mySuccessFlag, true se è andato tutto bene</returns>
		//---------------------------------------------------------------------
		private bool GetCompanyId(out SqlDataReader myDataReader, string CompanyId)
		{
			SqlDataReader mylocalDataReader = null;

			try
			{
				string myQuery = @"SELECT *, MSD_Providers.Description AS ProviderDesc, MSD_Providers.Provider
						  FROM MSD_Companies, MSD_Providers
						  WHERE CompanyId = @CompanyId AND MSD_Providers.ProviderId = MSD_Companies.ProviderId";

				SqlCommand myCommand = new SqlCommand(myQuery, CurrentSqlConnection);
				myCommand.Parameters.AddWithValue("@CompanyId", CompanyId);
				mylocalDataReader = myCommand.ExecuteReader();
			}
			catch(SqlException sqlException)
			{
				ExtendedInfo extendedInfo = new ExtendedInfo();
				extendedInfo.Add(DatabaseLayerStrings.Description, sqlException.Message);
				extendedInfo.Add(DatabaseLayerStrings.Procedure,	sqlException.Procedure);
				extendedInfo.Add(DatabaseLayerStrings.Server,		sqlException.Server);
				extendedInfo.Add(DatabaseLayerStrings.Number,		sqlException.Number);
				extendedInfo.Add(DatabaseLayerStrings.LineNumber,	sqlException.LineNumber);
				extendedInfo.Add(DatabaseLayerStrings.Function,     "CompanyDb.GetCompanyId");
				extendedInfo.Add(DatabaseLayerStrings.DefinedInto, "TaskBuilderNet.Data.DatabaseItems");
				Diagnostic.Set(DiagnosticType.Error, string.Format(DatabaseItemsStrings.ReadingTable, "MSD_Companies, MSD_Providers"), extendedInfo);
				myDataReader = null;
				return false;
			}

			myDataReader = mylocalDataReader;
			return true;
		}
		#endregion

		#region LastCompanyId - Restituisce il CompanyId dell'ultima Azienda inserita
		/// <summary>
		/// Legge l'ultima Azienda inserita (in MSD_Companies) e ne restituisce l'Id
		/// </summary>
		/// <returns>result, CompanyId oppure 0</returns>
		//---------------------------------------------------------------------
		public int LastCompanyId()
		{
			int result = 0;
			
			try
			{
				using (SqlCommand sqlCommand = new SqlCommand("SELECT MAX(CompanyId) FROM MSD_Companies", CurrentSqlConnection))
					result = (int)sqlCommand.ExecuteScalar();
			}
			catch(SqlException sqlException)
			{
				ExtendedInfo extendedInfo = new ExtendedInfo();
				extendedInfo.Add(DatabaseLayerStrings.Description, sqlException.Message);
				extendedInfo.Add(DatabaseLayerStrings.Procedure,	sqlException.Procedure);
				extendedInfo.Add(DatabaseLayerStrings.Server,		sqlException.Server);
				extendedInfo.Add(DatabaseLayerStrings.Number,		sqlException.Number);
				extendedInfo.Add(DatabaseLayerStrings.LineNumber,	sqlException.LineNumber);
				extendedInfo.Add(DatabaseLayerStrings.Function,     "CompanyDb.LastCompanyId");
				extendedInfo.Add(DatabaseLayerStrings.DefinedInto, "TaskBuilderNet.Data.DatabaseItems");
				Diagnostic.Set(DiagnosticType.Error, string.Format(DatabaseItemsStrings.ReadingTable, "MSD_Companies"), extendedInfo);
				return result;
			}

			return result;
		}
		#endregion

		#region IsSecurityCompany - Ritorna True se l'azienda è soggetta a Sicurezza, false altrimenti
		/// <summary>
		/// Ritorna True se l'azienda è soggetta a Sicurezza, false altrimenti
		/// </summary>
		//---------------------------------------------------------------------
		public bool IsSecurityCompany(string companyId)
		{
			bool isSecurityCompany = false;
			string sSelect = "SELECT COUNT(*) FROM MSD_Companies WHERE CompanyId = @CompanyId and UseSecurity = 1";

			try
			{
				using (SqlCommand myCommand = new SqlCommand(sSelect, CurrentSqlConnection))
				{
					myCommand.Parameters.AddWithValue("@CompanyId", Convert.ToInt32(companyId));
					isSecurityCompany = (int)myCommand.ExecuteScalar() > 0;
				}
			}
			catch
			{
			}
			
			return isSecurityCompany;
		}
		#endregion

		#region CompaniesNumber - Numero di Azienda configurate nel db di sistema
		/// <summary>
		/// Numero di Azienda configurate nel db di sistema
		/// </summary>
		//---------------------------------------------------------------------
		public int CompaniesNumber()
		{
			int companiesNumber = 0;

			try
			{
				using (SqlCommand myCommand = new SqlCommand("SELECT COUNT(*) FROM MSD_Companies", CurrentSqlConnection))
					companiesNumber = (int)myCommand.ExecuteScalar();
			}
			catch
			{
			}
			return companiesNumber;
		}
		#endregion
		
		#region GetCompanyDatabaseCulture - Ritorna il valore di DatabaseCulture di una company
		/// <summary>
		/// Ritorna il valore di DatabaseCulture di una company, dato il suo id
		/// </summary>
		//---------------------------------------------------------------------
		public int GetCompanyDatabaseCulture(string companyId)
		{
			int dbCulture = 0;

			try
			{
				using (SqlCommand myCommand = new SqlCommand("SELECT DatabaseCulture FROM MSD_Companies WHERE CompanyId = @CompanyId", CurrentSqlConnection))
				{
					myCommand.Parameters.AddWithValue("@companyId", Convert.ToInt32(companyId));
					using (SqlDataReader myReader = myCommand.ExecuteReader())
						while (myReader.Read())
							dbCulture = Convert.ToInt32(myReader["DatabaseCulture"]);
				}
			}
			catch
			{
			}
			
			return dbCulture;
		}
		#endregion

		#region GetCompanyName - Ritorna il nome di una company dato il suo id
		/// <summary>
		/// Ritorna il nome di una company dato il suo id
		/// </summary>
		//---------------------------------------------------------------------
		public string GetCompanyName(string companyId)
		{
			string name = string.Empty;

			try
			{
				using (SqlCommand myCommand = new SqlCommand("SELECT Company FROM MSD_Companies WHERE CompanyId = @companyId", CurrentSqlConnection))
				{
					myCommand.Parameters.AddWithValue("@companyId", Convert.ToInt32(companyId));
					using (SqlDataReader myReader = myCommand.ExecuteReader())
						while (myReader.Read())
							name = myReader["Company"].ToString();
				}
			}
			catch
			{
			}

			return name;
		}
		#endregion

		#region GetAllCompaniesIds- Ritorna la lista dei companyId presenti nel db di sistema
		/// <summary>
		/// Ritorna la lista dei companyId presenti nel db di sistema
		/// </summary>
		/// <param name="excludeDisabled">se true non vengono estratte le aziende disabilitate</param>
		//---------------------------------------------------------------------
		public List<string> GetAllCompaniesIds(bool excludeDisabled = false)
		{
			List<string> idsList = new List<string>();

			string query = "SELECT CompanyId FROM MSD_Companies";
			if (excludeDisabled)
				query += " WHERE Disabled = '0'";

			try
			{
                using (SqlCommand myCommand = new SqlCommand(query, CurrentSqlConnection))
                using (SqlDataReader myReader = myCommand.ExecuteReader())
                {
                    while (myReader.Read())
                        idsList.Add(myReader["CompanyId"].ToString());
                }
			}
			catch(Exception e)
            {
				System.Diagnostics.Debug.WriteLine(string.Format("Company::GetAllCompaniesIds (Exception: {0})", e.Message));
            }

            return idsList;
		}
		#endregion

		#region ExistsCompanyByName - Controlla che non esista un'azienda con lo stesso nome
		/// <summary>
		/// Controlla che non esista un'azienda con lo stesso nome
		/// </summary>
		/// <param name="companyName">nome azienda da ricercare</param>
		/// <returns>true: esiste un'azienda con il nome passato come parametro</returns>
		//---------------------------------------------------------------------
		public bool ExistsCompanyByName(string companyName)
		{
			bool exists = false;

			string query = "SELECT COUNT(*) FROM MSD_Companies WHERE Company = @CompanyName";

			try
			{
				using (SqlCommand myCommand = new SqlCommand(query, CurrentSqlConnection))
				{
					myCommand.Parameters.AddWithValue("@CompanyName", companyName);
					exists = (int)myCommand.ExecuteScalar() > 0;
				}
			}
			catch
			{
			}

			return exists;
		}
		#endregion

		#endregion
	}

	/// <summary>
	/// CompanyItem
	/// Classe per la gestione dei Records della tabella MSD_Companies
	/// </summary>
	//=========================================================================
	public class CompanyItem
	{
		#region Data members
		private string	companyId				= string.Empty;
		private string	company					= string.Empty;
		private string	description				= string.Empty;
		private string	providerId				= string.Empty;
		private string	provider				= string.Empty;
		private string	providerDesc			= string.Empty;
		private string	dbServer				= string.Empty;
		private string	dbName					= string.Empty;
		private string	dbOwner					= string.Empty;
		private string	dbDefaultUser			= string.Empty;
		private string	dbDefaultPassword		= string.Empty;
		private bool	dbAuthenticationWindows	= false;
		private bool	useSecurity				= false;
		private bool	useAuditing				= false;
		private bool	useTransaction			= false;
		private bool	useKeyedUpdate			= false;
		private string	preferredLanguage		= string.Empty;
		private string	applicationLanguage		= string.Empty;
		private bool	disabled				= false;
		private bool	useUnicode				= false;
		private	bool	isValid					= true;
		private bool	updating				= false;
		private int		databaseCulture			= 0;
		private bool	supportColumnCollation	= true;
		private int		port					= 0;
		private bool	useDBSlave				= false;
		private bool	useDataSynchro			= false;
		private bool	useRowSecurity			= false;
		#endregion

		# region Properties
		public string 	CompanyId				{ get { return companyId;				} set { companyId	= value; } }
		public string 	Company					{ get { return company;					} set { company		= value; } }
		public string 	Description				{ get { return description;				} set { description	= value; } }
		public string 	ProviderId				{ get { return providerId;				} set { providerId	= value; } }
		public string 	Provider				{ get { return provider;				} set { provider	= value; } }
		public string 	ProviderDesc			{ get { return providerDesc;			} set { providerDesc= value; } }
		public string 	DbServer				{ get { return dbServer;				} set { dbServer	= value; } }
		public string 	DbName					{ get { return dbName;					} set { dbName		= value; } }
		public string 	DbOwner					{ get { return dbOwner;					} set { dbOwner		= value; } }
		public string 	DbDefaultUser			{ get { return dbDefaultUser;			} set { dbDefaultUser		= value; } }
		public string 	DbDefaultPassword		{ get { return dbDefaultPassword;		} set { dbDefaultPassword	= value; } }
		public bool   	DBAuthenticationWindows	{ get { return dbAuthenticationWindows;	} set { dbAuthenticationWindows = value; } }
		public bool   	UseSecurity				{ get { return useSecurity;				} set { useSecurity			= value; } }
		public bool   	UseAuditing				{ get { return useAuditing;				} set { useAuditing			= value; } }
		public bool   	UseTransaction			{ get { return useTransaction;			} set { useTransaction		= value; } }
		public bool   	UseKeyedUpdate			{ get { return useKeyedUpdate;			} set { useKeyedUpdate		= value; } }
		public string 	PreferredLanguage		{ get { return preferredLanguage;		} set { preferredLanguage	= value; } }
		public string 	ApplicationLanguage		{ get { return applicationLanguage;		} set { applicationLanguage	= value; } }
		public bool   	Disabled				{ get { return disabled;				} set { disabled	= value; } }
		public bool   	UseUnicode				{ get { return useUnicode;				} set { useUnicode	= value; } }
		public bool   	IsValid					{ get { return isValid;					} set { isValid		= value; } }
		public bool		Updating				{ get { return updating;				} set { updating	= value; } }
		public int		DatabaseCulture			{ get { return databaseCulture;			} set { databaseCulture	= value; } }
		public bool		SupportColumnCollation	{ get { return supportColumnCollation;	} set { supportColumnCollation	= value; } }
		public int		Port					{ get { return port;					} set { port			= value; } }
		public bool		UseDBSlave				{ get { return useDBSlave;				} set { useDBSlave		= value; } }
		public bool		UseDataSynchro			{ get { return useDataSynchro;			} set { useDataSynchro	= value; } }
		public bool		UseRowSecurity			{ get { return useRowSecurity;			} set { useRowSecurity	= value; } }
		# endregion

		/// <summary>
		/// Constructor (empty)
		/// </summary>
		//---------------------------------------------------------------------
		public CompanyItem() {}
	}
}