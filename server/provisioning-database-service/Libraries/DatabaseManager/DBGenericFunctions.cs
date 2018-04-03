using System.Data;
using System.Globalization;
using TaskBuilderNetCore.Interfaces;

namespace Microarea.ProvisioningDatabase.Libraries.DatabaseManager
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
		/// poi a quella pi� vicina all'IsoStato di installazione
		/// </summary>
		//---------------------------------------------------------------------------
		public static int AssignDatabaseCultureValue
			(
			string isoState,
			string culture,
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

			// se per la collate c'� un solo LCID prendo quello (situazione ottimale)
			if (lcids != null && lcids.Length == 1)
				return lcids[0];

			CultureInfo ci = new CultureInfo(culture);
			int lcidToFind = ci.LCID;

			CultureInfo[] cults = null;

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

			// se la DatabaseCulture � sempre uguale a zero allora considero la prima culture non neutral
			// che si avvicina di pi� all'iso stato.
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
		/// Se il db � vuoto allora cambio la collate del db in latina (ovviamente solo se � diversa)
		/// </summary>
		/// <param name="companyConnectionString">stringa di connessione al db</param>
		//---------------------------------------------------------------------
		public static void SetDatabaseCollationIfNoUserTables(string companyConnectionString, DBMSType dbType)
		{
			if (string.IsNullOrWhiteSpace(companyConnectionString))
				return;

			TBConnection tbConn = null;
			TBDatabaseSchema schema = null;

			bool existUserTables = true;

			try
			{
				tbConn = new TBConnection(companyConnectionString, dbType);
				tbConn.Open();
				schema = new TBDatabaseSchema(tbConn);
				existUserTables = schema.GetAllSchemaObjects(DBObjectTypes.TABLE).Count >= 1;

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
			}
			catch
			{
			}
			finally
			{
				if (tbConn != null && tbConn.State == ConnectionState.Open)
				{
					tbConn.Close();
					tbConn.Dispose();
				}
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

			return supportColsCollation;
		}

		//---------------------------------------------------------------------
		public int GetLCIDForCollationCulture(string collationCulture)
		{
			if (string.IsNullOrWhiteSpace(collationCulture))
				return 0;

			// mi serve per assegnare il valore dell'LCID dalla collation
			// comanda sempre quella impostata sul database
			CultureInfo ci = new CultureInfo(collationCulture);
			int companyLcid = ci.LCID;

			string columnCollation = CultureHelper.GetWindowsCollation(ci.LCID);

			bool supportColsCollation = ((columnCollation != NameSolverDatabaseStrings.SQLLatinCollation) &&
				!CultureHelper.IsCollationCompatibleWithCulture(companyLcid, NameSolverDatabaseStrings.SQLLatinCollation));

			return 0;
		}
	}
}