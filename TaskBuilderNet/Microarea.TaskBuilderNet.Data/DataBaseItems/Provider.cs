using System;
using System.Collections;
using System.Data.SqlClient;
using System.Globalization;
using Microarea.TaskBuilderNet.Core.DiagnosticManager;
using Microarea.TaskBuilderNet.Data.DatabaseLayer;
using Microarea.TaskBuilderNet.Interfaces;

namespace Microarea.TaskBuilderNet.Data.DatabaseItems
{
	/// <summary>
	/// ProviderDb
	/// classe per la gestione dei record della tabella MSD_Providers
	/// </summary>
	//========================================================================
	public class ProviderDb : DataBaseItem
	{
		#region Costruttori
		/// <summary>
		/// Costruttore 1
		/// (vuoto)
		/// </summary>
		//---------------------------------------------------------------------
		public ProviderDb()	
		{}
		
		/// <summary>
		/// Costruttore 2
		/// Imposto la stringa di connessione
		/// </summary>
		//---------------------------------------------------------------------
		public ProviderDb(string connectionString)
		{
			ConnectionString = connectionString;
		}
		#endregion

		#region Add - Inserisce un nuovo Provider
		/// <summary>
		/// Add
		/// Inserisco un nuovo provider nella tabella MSD_Providers
		/// </summary>
		//---------------------------------------------------------------------
		public bool Add(DBMSType dbType, bool ODBCProvider=false /*true if provider is ODBC*/)
		{
			bool result = false;
            string provider = string.Empty, providerOdbc = string.Empty, description = string.Empty, descriptionOdbc = string.Empty;
			bool useConstParam = false;

			SqlTransaction  myTransSql;
			SqlCommand myCommand  = new SqlCommand();
			myTransSql			  = CurrentSqlConnection.BeginTransaction();
			myCommand.Connection  = CurrentSqlConnection;
			myCommand.Transaction = myTransSql;

			try
			{
				myCommand.CommandText = 
					@"INSERT INTO MSD_Providers (Provider, Description, UseConstParameter, StripTrailingSpaces) 
                      VALUES (@Provider, @Description, @UseConstParameter, @StripTrailingSpaces)";
				
				switch(dbType)
				{
					case DBMSType.SQLSERVER:
						provider = NameSolverDatabaseStrings.SQLOLEDBProvider;
						description = DatabaseLayerConsts.SqlOleProviderDescription;
                        providerOdbc = NameSolverDatabaseStrings.SQLODBCProvider;
						descriptionOdbc = DatabaseLayerConsts.SqlODBCProviderDescription;
						useConstParam = true;
						break;

					case DBMSType.ORACLE:
						provider = NameSolverDatabaseStrings.OraOLEDBProvider;
						description = DatabaseLayerConsts.OracleProviderDescription;
						useConstParam = false;
						break;

                    case DBMSType.POSTGRE:
                        providerOdbc = NameSolverDatabaseStrings.PostgreOdbcProvider;
                        descriptionOdbc = DatabaseLayerConsts.PostgreProviderDescription;
                        useConstParam = false;
                        break;

					case DBMSType.UNKNOWN:
						return false;
				}

                if (!ODBCProvider)
                {
                    myCommand.Parameters.Add(new SqlParameter("@Provider", provider));
                    myCommand.Parameters.Add(new SqlParameter("@Description", description));
                    myCommand.Parameters.Add(new SqlParameter("@UseConstParameter", useConstParam));
                    myCommand.Parameters.Add(new SqlParameter("@StripTrailingSpaces", true));
                   
                }
                else 
                {
                    myCommand.Parameters.Add(new SqlParameter("@Provider", providerOdbc));
                    myCommand.Parameters.Add(new SqlParameter("@Description", descriptionOdbc));
                    myCommand.Parameters.Add(new SqlParameter("@UseConstParameter", useConstParam));
                    myCommand.Parameters.Add(new SqlParameter("@StripTrailingSpaces", true));
                    
                }

                myCommand.ExecuteNonQuery();
                myTransSql.Commit();

				result = true;
			}
			catch(SqlException sqlException)
			{
				myTransSql.Rollback();
				ExtendedInfo extendedInfo = new ExtendedInfo();
				extendedInfo.Add(DatabaseLayerStrings.Description, sqlException.Message);
				extendedInfo.Add(DatabaseLayerStrings.Procedure,		sqlException.Procedure);
				extendedInfo.Add(DatabaseLayerStrings.Server,		sqlException.Server);
				extendedInfo.Add(DatabaseLayerStrings.Number,		sqlException.Number);
				extendedInfo.Add(DatabaseLayerStrings.LineNumber,	sqlException.LineNumber);
				extendedInfo.Add(DatabaseLayerStrings.Function,     "ProviderDb.Add");
				extendedInfo.Add(DatabaseLayerStrings.DefinedInto,  "SysAdminPlugIn");
				Diagnostic.Set(DiagnosticType.Error, string.Format(DatabaseItemsStrings.ProviderAdding, provider), extendedInfo);
			}
			return result;
		}
		#endregion

		#region Modify - Modifica un Provider esistente
		/// <summary>
		/// Modify
		/// Modifica un provider identificato da ProviderId nella tabella MSD_Providers
		/// </summary>
		/// <param name="ProviderId">Id del provider</param>
		/// <param name="Provider">Nome del provider</param>
		/// <param name="Description">Descrizione del provider</param>
		/// <param name="UseConstParameter">parametro true o false</param>
		/// <param name="StripTrailingSpaces">parametro true o false</param>
		//---------------------------------------------------------------------
		public bool Modify(string providerId, string provider, string description, bool useConstParameter, bool stripTrailingSpaces)
		{
			bool result = false;
			SqlTransaction myTransSql;
			SqlCommand myCommand  = new SqlCommand();
			myTransSql			  = CurrentSqlConnection.BeginTransaction();
			myCommand.Connection  = CurrentSqlConnection;
			myCommand.Transaction = myTransSql;
			try
			{
				string strQuery = @"UPDATE MSD_Providers 
                                    SET	   Provider			   = @Provider,
										   Description		   = @Description,
										   UseConstParameter   = @UseConstParameter,
										   StripTrailingSpaces = @StripTrailingSpaces
                                     WHERE ProviderId = @ProviderId
                                   ";
				myCommand.CommandText = strQuery;
				myCommand.Parameters.Add(new SqlParameter("@ProviderId",			Int32.Parse(providerId)));
				myCommand.Parameters.Add(new SqlParameter("@Provider",				provider));
				myCommand.Parameters.Add(new SqlParameter("@Description",			description));
				myCommand.Parameters.Add(new SqlParameter("@UseConstParameter",		useConstParameter));
				myCommand.Parameters.Add(new SqlParameter("@StripTrailingSpaces",	stripTrailingSpaces));
				myCommand.ExecuteNonQuery();                           
				myTransSql.Commit();
				result = true;
			}
			catch(SqlException sqlException)
			{
				myTransSql.Rollback();
				ExtendedInfo extendedInfo = new ExtendedInfo();
				extendedInfo.Add(DatabaseLayerStrings.Description, sqlException.Message);
				extendedInfo.Add(DatabaseLayerStrings.Procedure,	sqlException.Procedure);
				extendedInfo.Add(DatabaseLayerStrings.Server,		sqlException.Server);
				extendedInfo.Add(DatabaseLayerStrings.Number,		sqlException.Number);
				extendedInfo.Add(DatabaseLayerStrings.LineNumber,	sqlException.LineNumber);
				extendedInfo.Add(DatabaseLayerStrings.Function,     "ProviderDb.Modify");
				extendedInfo.Add(DatabaseLayerStrings.DefinedInto,  "SysAdminPlugIn");
				Diagnostic.Set(DiagnosticType.Error, string.Format(DatabaseItemsStrings.ProviderModify, provider), extendedInfo);
			}
			return result;
		}
		#endregion

		#region Delete - Cancella un Provider, identificato dal ProviderId
		/// <summary>
		/// Delete
		/// Cancella il provider identificato da ProviderId
		/// </summary>
		/// <param name="ProviderId">Id che identifica il provider</param>
		//---------------------------------------------------------------------
		public bool Delete(string providerId)
		{
			bool result = false;
			SqlTransaction  myTransSql;
			SqlCommand myCommand = new SqlCommand();
			string strQuery		 = string.Empty;

			myTransSql				= CurrentSqlConnection.BeginTransaction();
			myCommand.Connection	= CurrentSqlConnection;
			myCommand.Transaction	= myTransSql;
			
			try
			{
				myCommand.CommandText = "SELECT COUNT(*) as NumberOfProvider FROM MSD_Companies WHERE ProviderId = @ProviderId";
				myCommand.Parameters.AddWithValue("@ProviderId", Int32.Parse(providerId));
				//controllo che il provider non sia associato con alcuna company 
				// se isProvider = 0 continuo, altrimenti dò messaggio di errore e non cancello
				int isProvider = (int)myCommand.ExecuteScalar();
				if (isProvider == 0)
				{
					myCommand.CommandText = "DELETE FROM MSD_Providers WHERE ProviderId = @ProviderId";
					myCommand.ExecuteNonQuery();
					myTransSql.Commit();
					result = true;
				}
				else
				{
					myTransSql.Rollback();
					Diagnostic.Set(DiagnosticType.Error, DatabaseItemsStrings.ProviderDeleting);
				}
			}
			catch(SqlException sqlException)
			{
				myTransSql.Rollback();
				ExtendedInfo extendedInfo = new ExtendedInfo();
				extendedInfo.Add(DatabaseLayerStrings.Description, sqlException.Message);
				extendedInfo.Add(DatabaseLayerStrings.Procedure,	sqlException.Procedure);
				extendedInfo.Add(DatabaseLayerStrings.Server,		sqlException.Server);
				extendedInfo.Add(DatabaseLayerStrings.Number,		sqlException.Number);
				extendedInfo.Add(DatabaseLayerStrings.LineNumber,	sqlException.LineNumber);
				extendedInfo.Add(DatabaseLayerStrings.Function,     "ProviderDb.Delete");
				extendedInfo.Add(DatabaseLayerStrings.DefinedInto,  "SysAdminPlugIn");
				Diagnostic.Set(DiagnosticType.Error, string.Format(DatabaseItemsStrings.ProviderDeleted, providerId), extendedInfo);
			}
			return result;
		}
		#endregion

		#region Funzioni di Ricerca e Selezione

		#region GetAllFieldsProviderById - Trova il Provider identificato da ProviderId
		/// <summary>
		/// GetAllFieldsProviderById
		/// Trovo tutti i dati del provider identificato dal ProviderId
		/// </summary>
		/// <param name="provider">ArrayList con i dati</param>
		/// <param name="idProvider">Id del provider da cercare</param>
		/// <returns>mySuccessFlag, true se è andato tutto bene</returns>
		//---------------------------------------------------------------------
		public bool GetAllFieldsProviderById(out ArrayList provider, string idProvider)
		{
			ArrayList	 localProvider  = new ArrayList();
			ProviderItem providerItem   = new ProviderItem();
			bool		 mySuccessFlag	= true;

			try
			{
				SqlDataReader myDataReader;
				bool successFlag = GetProviderId(out myDataReader, idProvider);
				if (successFlag)
				{
					while(myDataReader.Read())
					{
						providerItem.ProviderId			 = myDataReader["ProviderId"].ToString();
						providerItem.ProviderValue		 = myDataReader["Provider"].ToString();
						providerItem.Description		 = myDataReader["Description"].ToString();
						providerItem.StripTrailingSpaces = bool.Parse(myDataReader["StripTrailingSpaces"].ToString());
						providerItem.UseConstParameter	 = bool.Parse(myDataReader["UseConstParameter"].ToString());
						localProvider.Add(providerItem);
					}
					myDataReader.Close();
				}
				else
					mySuccessFlag = successFlag;
			}
			catch(SqlException sqlException)
			{
				ExtendedInfo extendedInfo = new ExtendedInfo();
				extendedInfo.Add(DatabaseLayerStrings.Description, sqlException.Message);
				extendedInfo.Add(DatabaseLayerStrings.Procedure,	sqlException.Procedure);
				extendedInfo.Add(DatabaseLayerStrings.Server,		sqlException.Server);
				extendedInfo.Add(DatabaseLayerStrings.Number,		sqlException.Number);
				extendedInfo.Add(DatabaseLayerStrings.LineNumber,	sqlException.LineNumber);
				extendedInfo.Add(DatabaseLayerStrings.Function,     "ProviderDb.GetAllFieldsProviderById");
				extendedInfo.Add(DatabaseLayerStrings.DefinedInto,  "SysAdminPlugIn");
				Diagnostic.Set(DiagnosticType.Error, DatabaseItemsStrings.ProvidersReading, extendedInfo);
				mySuccessFlag = false;
			}
			provider = localProvider;
			return mySuccessFlag;
		}

		/// <summary>
		/// GetProviderId
		/// Riempie un datareader con i dati di un provider identificato
		/// dalla ProviderId
		/// </summary>
		/// <param name="myDataReader">DataReader con le info sul provider</param>
		/// <param name="providerId">Id che identifica il provider</param>
		/// <returns>mySuccessFlag, true se è andato tutto bene</returns>
		//---------------------------------------------------------------------
		public bool GetProviderId(out SqlDataReader myDataReader, string providerId)
		{
			SqlDataReader mylocalDataReader = null;
			bool		  mySuccessFlag		= true;

			try
			{
				SqlCommand myCommand = 
					new SqlCommand("SELECT * FROM MSD_Providers WHERE ProviderId = @ProviderId", CurrentSqlConnection);
				
				myCommand.Parameters.AddWithValue("@ProviderId", Int32.Parse(providerId));
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
				extendedInfo.Add(DatabaseLayerStrings.Function,     "ProviderDb.GetProviderId");
				extendedInfo.Add(DatabaseLayerStrings.DefinedInto,  "SysAdminPlugIn");
				Diagnostic.Set(DiagnosticType.Error, string.Format(DatabaseItemsStrings.ReadingTable, "MSD_Providers"), extendedInfo);
				mySuccessFlag = false;
			}
			
			myDataReader = mylocalDataReader;
			return mySuccessFlag;
		}
		#endregion

		#region SelectAllProviders - Trova tutti i Providers
		/// <summary>
		/// SelectAllProviders
		/// Riempie un ArrayList con i dati di tutti i providers trovati
		/// nella tabella MSD_Providers
		/// </summary>
		/// <param name="providers">ArrayList con i dati dei providers</param>
		/// <returns>mySuccessFlag, true se è andato tutto bene</returns>
		//---------------------------------------------------------------------
		public bool SelectAllProviders(out ArrayList providers)
		{
			ArrayList localProviders = new ArrayList();
			bool	  mySuccessFlag  = true;

			try
			{
				SqlDataReader myDataReader;
				bool successFlag = GetAllProviders(out myDataReader);
				if (successFlag)
				{
					while(myDataReader.Read())
					{
						ProviderItem providerItem        = new ProviderItem();
						providerItem.ProviderId          = myDataReader["ProviderId"].ToString();
						providerItem.ProviderValue       = myDataReader["Provider"].ToString();
						providerItem.Description		 = myDataReader["Description"].ToString();
						providerItem.StripTrailingSpaces = bool.Parse(myDataReader["StripTrailingSpaces"].ToString());
						providerItem.UseConstParameter	 = bool.Parse(myDataReader["UseConstParameter"].ToString());
						localProviders.Add(providerItem);
					}
					myDataReader.Close();
				}
				else
					mySuccessFlag = successFlag;
			}
			catch(SqlException sqlException)
			{
				ExtendedInfo extendedInfo = new ExtendedInfo();
				extendedInfo.Add(DatabaseLayerStrings.Description, sqlException.Message);
				extendedInfo.Add(DatabaseLayerStrings.Procedure,	sqlException.Procedure);
				extendedInfo.Add(DatabaseLayerStrings.Server,		sqlException.Server);
				extendedInfo.Add(DatabaseLayerStrings.Number,		sqlException.Number);
				extendedInfo.Add(DatabaseLayerStrings.LineNumber,	sqlException.LineNumber);
				extendedInfo.Add(DatabaseLayerStrings.Function,     "ProviderDb.SelectAllProviders");
				extendedInfo.Add(DatabaseLayerStrings.DefinedInto,  "SysAdminPlugIn");
				Diagnostic.Set(DiagnosticType.Error, DatabaseItemsStrings.ProvidersReading, extendedInfo);
				mySuccessFlag = false;
			}
			
			providers = localProviders;
			return mySuccessFlag;
		}

		/// <summary>
		/// GetAllProviders
		/// Riempie un dataReader con tutti i providers trovati nella tabella MSD_Providers
		/// </summary>
		/// <param name="myDataReader">DataReader con i dati dei providers</param>
		/// <returns>mySuccessFlag, true se è andato tutto bene</returns>
		//---------------------------------------------------------------------
		public bool GetAllProviders(out SqlDataReader myDataReader)
		{
			SqlDataReader mylocalDataReader = null;
			bool		  mySuccessFlag		= true;

			try
			{
				SqlCommand myCommand 
					= new SqlCommand("SELECT * FROM MSD_Providers ORDER BY MSD_Providers.Provider", CurrentSqlConnection);
				mylocalDataReader = myCommand.ExecuteReader();
				mySuccessFlag = true;
				
				if (myCommand != null)
					myCommand.Dispose();
			}
			catch(SqlException sqlException)
			{
				ExtendedInfo extendedInfo = new ExtendedInfo();
				extendedInfo.Add(DatabaseLayerStrings.Description, sqlException.Message);
				extendedInfo.Add(DatabaseLayerStrings.Procedure,	sqlException.Procedure);
				extendedInfo.Add(DatabaseLayerStrings.Server,		sqlException.Server);
				extendedInfo.Add(DatabaseLayerStrings.Number,		sqlException.Number);
				extendedInfo.Add(DatabaseLayerStrings.LineNumber,	sqlException.LineNumber);
				extendedInfo.Add(DatabaseLayerStrings.Function,     "ProviderDb.GetAllProviders");
				extendedInfo.Add(DatabaseLayerStrings.DefinedInto,  "SysAdminPlugIn");
				Diagnostic.Set(DiagnosticType.Error, string.Format(DatabaseItemsStrings.ReadingTable, "MSD_Providers"), extendedInfo);
				mySuccessFlag = false;
			}

			myDataReader = mylocalDataReader;
			return mySuccessFlag;
		}
		#endregion

		#region SelectSqlProvider - Seleziona il Provider SQL 
		/// <summary>
		/// SelectSqlProvider
		/// Ritorna la classe ProviderItem valorizzata con i dati del provider SQL 
		/// </summary>
		/// <returns>providerItem classe con i dati del provider SQL</returns>
		//---------------------------------------------------------------------
		public ProviderItem SelectSqlProvider()
		{
			ProviderItem providerItem = new ProviderItem();
			
			try
			{
				SqlDataReader myDataReader;
				if (GetSqlProvider(out myDataReader))
				{
					while(myDataReader.Read())
					{
						providerItem.ProviderId          = myDataReader["ProviderId"].ToString();
						providerItem.ProviderValue       = myDataReader["Provider"].ToString();
						providerItem.Description         = myDataReader["Description"].ToString();
						providerItem.StripTrailingSpaces = bool.Parse(myDataReader["StripTrailingSpaces"].ToString());
						providerItem.UseConstParameter	 = bool.Parse(myDataReader["UseConstParameter"].ToString());
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
				extendedInfo.Add(DatabaseLayerStrings.Function,     "ProviderDb.SelectSqlProvider");
				extendedInfo.Add(DatabaseLayerStrings.DefinedInto,  "SysAdminPlugIn");
				Diagnostic.Set(DiagnosticType.Error, DatabaseItemsStrings.ProvidersReading, extendedInfo);
			}
			
			return providerItem;
		}

		/// <summary>
		/// GetSqlProvider
		/// Tra tutti i providers, cerca quello Sql (che è impostato come default per le aziende 
		/// nella lista dei providers disponibili)
		/// </summary>
		/// <param name="myDataReader">DataReader con i dati del provider SQL</param>
		/// <returns>mySuccessFlag, true se è andato tutto bene</returns>
		//---------------------------------------------------------------------
		public bool GetSqlProvider(out SqlDataReader myDataReader)
		{
			SqlDataReader mylocalDataReader = null;
            bool mySuccessFlag = true;

			try
			{
				SqlCommand myCommand = 
					new SqlCommand("SELECT * FROM MSD_Providers WHERE  Provider = @Provider1 OR Provider = @Provider2", CurrentSqlConnection);
              
				myCommand.Parameters.AddWithValue("@Provider1", NameSolverDatabaseStrings.SQLOLEDBProvider);
                myCommand.Parameters.AddWithValue("@Provider2", NameSolverDatabaseStrings.SQLODBCProvider);
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
				extendedInfo.Add(DatabaseLayerStrings.Function,     "ProviderDb.GetSqlProvider");
				extendedInfo.Add(DatabaseLayerStrings.DefinedInto,  "SysAdminPlugIn");
				Diagnostic.Set(DiagnosticType.Error, string.Format(DatabaseItemsStrings.ReadingTable, "MSD_Providers"), extendedInfo);
				mySuccessFlag = false;
			}

			myDataReader = mylocalDataReader;
			return mySuccessFlag;
		}
		#endregion

		#region IsSqlProvider - True se il provider è SQL, false se è ORACLE
		/// <summary>
		/// IsSqlProvider
		/// ritorna true se il provider è SQL
		/// </summary>
		/// <param name="providerId">Id del provider</param>
		/// <returns>true se è il provider SQl, false altrimenti</returns>
		//---------------------------------------------------------------------
		public bool IsSqlProvider(int providerId)
		{
			bool isSqlProvider = false;
			ArrayList providerData = new ArrayList();

			GetAllFieldsProviderById(out providerData, providerId.ToString());
			if (providerData.Count > 0)
			{
				ProviderItem providerItem = (ProviderItem)providerData[0];
				isSqlProvider = ( 
					string.Compare(providerItem.ProviderValue, NameSolverDatabaseStrings.SQLOLEDBProvider, true, CultureInfo.InvariantCulture) == 0 ||
                    string.Compare(providerItem.ProviderValue, NameSolverDatabaseStrings.SQLODBCProvider, true, CultureInfo.InvariantCulture) == 0 );
			}
			
			return isSqlProvider;
		}
		#endregion

		#region ExistProvider - Verifica l'esistenza del Provider specificato
		/// <summary>
		/// ExistProvider
		/// Dato il provider (short name), ritorna true se esiste nella tabella MSD_Providers
		/// </summary>
		/// <param name="providerCode">Short Name del provider</param>
		/// <returns>existProvider, true se il provider esiste</returns>
		//---------------------------------------------------------------------
		public bool ExistProvider(string providerCode)
		{
			int result = 0;
			SqlDataReader mylocalDataReader = null;

			try
			{
				SqlCommand myCommand = 
					new SqlCommand("SELECT COUNT(*) FROM MSD_Providers WHERE Provider = @Provider", CurrentSqlConnection);
				myCommand.Parameters.AddWithValue("@Provider", providerCode);
				mylocalDataReader = myCommand.ExecuteReader();
			}
			catch(SqlException )
			{
				if (mylocalDataReader != null && !mylocalDataReader.IsClosed)
					mylocalDataReader.Close();
				return false;
			}

			if (mylocalDataReader.Read())
				result = mylocalDataReader.GetInt32(0);

			if (mylocalDataReader != null && !mylocalDataReader.IsClosed)
				mylocalDataReader.Close();
			
			return (result > 0);
		}
		#endregion

		#endregion
	}
		
	/// <summary>
	/// Provider
	/// Classe in cui memorizzo i dati dei provider, che mi servirà per le liste
	/// di visualizzazione
	/// </summary>
	//=========================================================================
	public class ProviderItem
	{
		#region Variabili membro (private)
		private string providerId			= string.Empty;
		private string provider				= string.Empty;
		private string description			= string.Empty;
		private bool   useConstParameter	= false;
		private bool   stripTrailingSpaces	= false;
		#endregion

		#region Proprietà
		//properties
		//---------------------------------------------------------------------
		public string ProviderId		  { get { return providerId;		  } set { providerId		  = value; } }
		public string ProviderValue		  { get { return provider;		      } set { provider			  = value; } }
		public string Description		  { get { return description;		  } set { description		  = value; } }
		public bool   UseConstParameter	  { get { return useConstParameter;   }	set { useConstParameter   = value; } }
		public bool   StripTrailingSpaces {	get { return stripTrailingSpaces; }	set { stripTrailingSpaces = value; } }
		#endregion

		#region Costruttori
		/// <summary>
		/// ProviderItem
		/// Costruttore (vuoto)
		/// </summary>
		//---------------------------------------------------------------------
		public ProviderItem() {}

		/// <summary>
		/// Costruttore
		/// Inizializzo il ProviderId, Provider e Description
		/// </summary>
		//---------------------------------------------------------------------
		public ProviderItem (string id, string text, string description)
		{
			providerId		 = id;
			provider		 = text;
			this.description = description;	
		}

		/// <summary>
		/// Costruttore
		/// Inizializzo tutti i campi
		/// </summary>
		//---------------------------------------------------------------------
		public ProviderItem(string id, string text, string description, bool useConstParameter,	bool stripTrailingSpaces)
		{
			providerId					= id;
			provider					= text;
			this.description			= description;
			this.useConstParameter		= useConstParameter;
			this.stripTrailingSpaces	= stripTrailingSpaces;
		}
		#endregion
	}
}