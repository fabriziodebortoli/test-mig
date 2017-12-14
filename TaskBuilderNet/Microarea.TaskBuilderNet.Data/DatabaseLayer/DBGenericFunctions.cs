using System;
using System.Data;
using System.Globalization;
using Microarea.TaskBuilderNet.Core.Generic;
using Microarea.TaskBuilderNet.Core.NameSolver;
using Microarea.TaskBuilderNet.Interfaces;

namespace Microarea.TaskBuilderNet.Data.DatabaseLayer
{
	/// <summary>
	/// DBGenericFunctions
	/// Collezione di funzioni generiche utilizzate in tutto il SysAdmin
	/// </summary>
	//=========================================================================
	public class DBGenericFunctions
	{
		/// <summary>
		/// AssignDatabaseCultureValue
		/// Leggo la Collate direttamente dal db e cerco un LCID valido da assegnare.
		/// Prima guardo sul ServerConnection.config il valore dell'ApplicationLanguage,
		/// poi salgo alla PreferredLanguage (se non neutral) e 
		/// poi a quella più vicina all'IsoStato di installazione
		/// </summary>
		//---------------------------------------------------------------------------
		public static int AssignDatabaseCultureValue
			(
			string isoState, 
			string companyConnectionString, 
			DBMSType dbType, 
			bool useUnicode // usato solo in mysql
			)
		{
			string validCollate = ReadCollationFromDatabase(companyConnectionString, dbType);

			// richiamare la funzione di Carlotta per avere un elenco di LCID validi e poi vado in scaletta
			int[] lcids = null;

			if (dbType == DBMSType.SQLSERVER)
				lcids = CultureHelper.GetCompatibleLocaleIDs(validCollate);

			// se per la collate c'è un solo LCID prendo quello (situazione ottimale)
			if (lcids != null && lcids.Length == 1)
				return lcids[0];

			int lcidToFind = 0;
			CultureInfo ci = null;
			CultureInfo[] cults = null;

			if (InstallationData.ServerConnectionInfo.ApplicationLanguage.Length > 0)
			{
				ci = new CultureInfo(InstallationData.ServerConnectionInfo.ApplicationLanguage);
				lcidToFind = ci.LCID;
			}
			else
			{
				if (InstallationData.ServerConnectionInfo.PreferredLanguage.Length > 0)
				{
					ci = new CultureInfo(InstallationData.ServerConnectionInfo.PreferredLanguage);
					if (!ci.IsNeutralCulture)
						lcidToFind = ci.LCID;
					else
					{
						cults = CultureInfo.GetCultures(CultureTypes.AllCultures);
						for (int i = 0; i < cults.Length; i++)
						{
							if (string.Compare(cults[i].TwoLetterISOLanguageName, isoState, true, CultureInfo.InvariantCulture) == 0)
							{
								if (!cults[i].IsNeutralCulture)
								{
									lcidToFind = cults[i].LCID;
									break;
								}
							}
						}
					}
				}
			}

			int databaseCulture = 0;
			if (lcids != null)
			{
				// cerco nell'array di possibili LCID compatibili quello uguale al mio
				for (int i = 0; i < lcids.Length ; i++)
				{
					if (lcidToFind == lcids[i])
					{
						databaseCulture = lcids[i];
						break;
					}
				}
			}

			// se la DatabaseCulture è sempre uguale a zero allora considero la prima culture non neutral
			// che si avvicina di più all'iso stato.
			if (databaseCulture == 0)
			{
				if (cults == null)
					cults = CultureInfo.GetCultures(CultureTypes.AllCultures);

				for (int i = 0; i < cults.Length; i++)
				{
					if (string.Compare(cults[i].TwoLetterISOLanguageName, isoState, true, CultureInfo.InvariantCulture) == 0)
					{
						if (!cults[i].IsNeutralCulture)
						{
							databaseCulture = cults[i].LCID;
							break;
						}
					}
				}
			}

			return databaseCulture;
		}

		/// <summary>
		/// ReadCollationFromDatabase
		/// mi connetto al volo al database aziendale e vado a leggere la COLLATE con la solita
		/// scaletta: colonna, database, server
		/// </summary>
		/// <param name="companyConnectionString">stringa di connessione</param>
		/// <returns>stringa di COLLATE</returns>
		//---------------------------------------------------------------------
		public static string ReadCollationFromDatabase(string companyConnectionString, DBMSType dbType)
		{
			if (companyConnectionString.Length <= 0)
				return string.Empty;

			string collate = string.Empty;
			TBConnection tbConn = null;

			try
			{
				tbConn = new TBConnection(companyConnectionString, dbType);
				tbConn.Open();
				collate = TBCheckDatabase.GetValidCollationPropertyForDB(tbConn);
				tbConn.Close();
				tbConn.Dispose();
			}
			catch
			{
				if (tbConn != null && tbConn.State == ConnectionState.Open)
				{
					tbConn.Close();
					tbConn.Dispose();
				}
			}
			
			return collate;
		}

		/// <summary>
		/// CheckDatabaseUserTables
		/// Controllo se sul database ci sono delle user tables.
		/// Se il db è vuoto allora cambio la collate del db in latina (ovviamente solo se è diversa)
		/// </summary>
		/// <param name="companyConnectionString">stringa di connessione al db</param>
		//---------------------------------------------------------------------
		public static void SetDatabaseCollationIfNoUserTables(string companyConnectionString, DBMSType dbType)
		{
			if (companyConnectionString.Length <= 0 || dbType == DBMSType.ORACLE)
				return;

			TBConnection tbConn = null;
			TBDatabaseSchema schema = null;
			SchemaDataTable dataTable = null;

			bool existUserTables = true;

			try
			{
				tbConn = new TBConnection(companyConnectionString, dbType);
				tbConn.Open();
				schema = new TBDatabaseSchema(tbConn);
				dataTable = schema.GetAllSchemaObjects(DBObjectTypes.TABLE);
				if (dataTable != null)
					existUserTables = (dataTable.Rows.Count >= 1);
			}
			catch
			{
				if (tbConn != null && tbConn.State == ConnectionState.Open)
				{
					tbConn.Close();
					tbConn.Dispose();
				}
				return;
			}

			// se sul db non esistono tabelle dell'utente altero la collate del database con la latina (solo se diversa)
			if (!existUserTables)
			{
				string alterCollate = "ALTER DATABASE {0} COLLATE {1}";

				if (dbType == DBMSType.SQLSERVER)
				{
					if (string.Compare
						(TBCheckDatabase.GetDatabaseCollation(tbConn),
						NameSolverDatabaseStrings.SQLLatinCollation,
						true,
						CultureInfo.InvariantCulture) != 0
						)
					{
						TBCommand command = new TBCommand(tbConn);
						string companyDbName = tbConn.Database;
						tbConn.ChangeDatabase(DatabaseLayerConsts.MasterDatabase);
						command.CommandText = string.Format(alterCollate, companyDbName, NameSolverDatabaseStrings.SQLLatinCollation);

						try
						{
							command.ExecuteReader();
						}
						catch
						{
						}

						tbConn.ChangeDatabase(companyDbName);
					}
				}
			}

			if (tbConn != null && tbConn.State == ConnectionState.Open)
			{
				tbConn.Close();
				tbConn.Dispose();
			}
		}

		//---------------------------------------------------------------------
		public static bool CalculateSupportColumnCollation(string connectionString, int companyLcid, DBMSType dbType, bool useUnicode)
		{
			string dbCollation = string.Empty;
			string colCollation = string.Empty;

			return CalculateSupportColumnCollation(connectionString, companyLcid, out dbCollation, out colCollation, dbType, useUnicode);
		}

		//---------------------------------------------------------------------
		public static bool CalculateSupportColumnCollation
			(
			string connectionString, 
			int companyLcid, 
			out string dbCollation, 
			out string columnCollation,
			DBMSType dbType,
			bool useUnicode // questo flag serve solo per MySql
			)
		{
			bool supportColsCollation = false;

			TBConnection tbConn = null;

			dbCollation		= string.Empty;
			columnCollation = string.Empty;

			if (dbType == DBMSType.SQLSERVER)
			{
				try
				{
					tbConn = new TBConnection(connectionString, dbType);
					tbConn.Open();
					columnCollation	= TBCheckDatabase.GetColumnCollation(tbConn);
					if (columnCollation.Length == 0)
						columnCollation	= CultureHelper.GetWindowsCollation(companyLcid);

					dbCollation = TBCheckDatabase.GetDatabaseCollation(tbConn);

					tbConn.Close();
					tbConn.Dispose();
				}
				catch
				{
					if (tbConn != null && tbConn.State == ConnectionState.Open)
					{
						tbConn.Close();
						tbConn.Dispose();
					}
				}

				if (dbType == DBMSType.SQLSERVER)
					supportColsCollation = ((columnCollation != dbCollation) && 
						!CultureHelper.IsCollationCompatibleWithCulture(companyLcid, dbCollation));
			}

			if (dbType == DBMSType.ORACLE)
			{
				try
				{
					tbConn = new TBConnection(connectionString, dbType);
					tbConn.Open();
					supportColsCollation = TBCheckDatabase.IsSupportedColumnCollationForOracle(tbConn);
					tbConn.Close();
					tbConn.Dispose();
				}
				catch
				{
					if (tbConn != null && tbConn.State == ConnectionState.Open)
					{
						tbConn.Close();
						tbConn.Dispose();
					}
				}
			}

			return supportColsCollation;
		}

		//---------------------------------------------------------------------------
		public static int AssignOracleDatabaseCultureValue
			(
			PathFinder pathFinder, 
			string isoState, 
			string language, 
			string territory
			)
		{
			// richiamare la funzione di Carlotta per avere un elenco di LCID validi e poi vado in scaletta
			int[] lcids = CultureHelper.GetCompatibleLocaleIDsWithOracleLanguageAndTerritory(language, territory);

			// se per la collate c'è un solo LCID prendo quello (situazione ottimale)
			if (lcids != null && lcids.Length == 1)
				return lcids[0];

			int lcidToFind = 0;
			CultureInfo ci = null;
			CultureInfo[] cults = null;

			if (!string.IsNullOrWhiteSpace(InstallationData.ServerConnectionInfo.ApplicationLanguage))
			{
				ci = new CultureInfo(InstallationData.ServerConnectionInfo.ApplicationLanguage);
				lcidToFind = ci.LCID;
			}
			else
			{
				if (!string.IsNullOrWhiteSpace(InstallationData.ServerConnectionInfo.PreferredLanguage))
				{
					ci = new CultureInfo(InstallationData.ServerConnectionInfo.PreferredLanguage);
					if (!ci.IsNeutralCulture)
						lcidToFind = ci.LCID;
					else
					{
						cults = CultureInfo.GetCultures(CultureTypes.AllCultures);
						for (int i = 0; i < cults.Length; i++)
						{
							if (string.Compare(cults[i].TwoLetterISOLanguageName, isoState, StringComparison.InvariantCultureIgnoreCase) == 0)
							{
								if (!cults[i].IsNeutralCulture)
								{
									lcidToFind = cults[i].LCID;
									break;
								}
							}
						}
					}
				}
			}

			int databaseCulture = 0;
			if (lcids != null)
			{
				// cerco nell'array di possibili LCID compatibili quello uguale al mio
				for (int i = 0; i < lcids.Length ; i++)
				{
					if (lcidToFind == lcids[i])
					{
						databaseCulture = lcids[i];
						break;
					}
				}
			}

			// se la DatabaseCulture è sempre uguale a zero allora considero la prima culture non neutral
			// che si avvicina di più all'iso stato.
			if (databaseCulture == 0)
			{
				if (cults == null)
					cults = CultureInfo.GetCultures(CultureTypes.AllCultures);

				for (int i = 0; i < cults.Length; i++)
				{
					if (string.Compare(cults[i].TwoLetterISOLanguageName, isoState, StringComparison.InvariantCultureIgnoreCase) == 0)
					{
						if (!cults[i].IsNeutralCulture)
						{
							databaseCulture = cults[i].LCID;
							break;
						}
					}
				}
			}

			return databaseCulture;
		}

		# region PurposeDatabaseCultureForDBCreation (inizializzazione culture db aziendale)
		///<summary>
		/// PurposeDatabaseCultureForDBCreation
		/// Per identificare la culture con cui inizializzare la form dei parametri per la creazione del db aziendale
		/// (per Sql Server e MySql)
		///</summary>
		//---------------------------------------------------------------------
		public static string PurposeDatabaseCultureForDBCreation(string isoState)
		{
			// PROPOSIZIONE DELLA CULTURE DI DATABASE (leggo dal ServerConnection.config)
			// 1. prendo l'ApplicationLanguage. Oppure:
			// 2. prendo il PreferredLanguage (se non è neutral o non vuoto). Oppure:
			// 3. leggo il CountryCode della Licence e imposto la più prossima CultureInfo non neutral che inizia così
			string initializeDBCulture = InstallationData.ServerConnectionInfo.ApplicationLanguage;

			// se l'ApplicationLanguage e' buona la ritorno subito
			if (!string.IsNullOrWhiteSpace(initializeDBCulture))
				return initializeDBCulture;

			CultureInfo[] cults = CultureInfo.GetCultures(CultureTypes.AllCultures);

			if (!string.IsNullOrWhiteSpace(InstallationData.ServerConnectionInfo.PreferredLanguage))
			{
				CultureInfo ci = new CultureInfo(InstallationData.ServerConnectionInfo.PreferredLanguage);

				if (!ci.IsNeutralCulture)
					initializeDBCulture = ci.Name;
				else
				{
					for (int i = 0; i < cults.Length; i++)
					{
						if (string.Compare(cults[i].TwoLetterISOLanguageName, isoState, StringComparison.InvariantCultureIgnoreCase) == 0)
						{
							if (!cults[i].IsNeutralCulture)
							{
								initializeDBCulture = cults[i].Name;
								break;
							}
						}
					}
				}
			}

			// caso estremo che non si dovrebbe mai verificare
			if (string.IsNullOrWhiteSpace(initializeDBCulture))
			{
				for (int i = 0; i < cults.Length; i++)
				{
					if (string.Compare(cults[i].TwoLetterISOLanguageName, isoState, true, CultureInfo.InvariantCulture) == 0)
					{
						if (!cults[i].IsNeutralCulture)
						{
							initializeDBCulture = cults[i].Name;
							break;
						}
					}
				}
			}

			return initializeDBCulture;
		}
		# endregion
	}
}