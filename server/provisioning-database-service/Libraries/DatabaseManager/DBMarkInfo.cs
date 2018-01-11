using System;
using System.Collections.Generic;
using System.Data;
using System.Diagnostics;

namespace Microarea.ProvisioningDatabase.Libraries.DatabaseManager
{
	/// <summary>
	/// Classe che mappa le colonne della tabella TB_DBMark
	/// </summary>
	//=========================================================================
	public class DBMarkRow
	{
		public string Application = string.Empty;
		public string AddOnModule = string.Empty;
		public int DBRelease = -1;
		public bool Status = false;
		public bool ExecLevel3 = false;
		public int UpgradeLevel = 1;
		public int Step = 1;
	}

	/// <summary>
	/// Classe che mappa in memoria la tabella TB_DBMark e le sue colonne
	/// </summary>
	//=========================================================================
	public class DBMarkTable
	{
		private TBConnection tbConnection = null;

		private List<DBMarkRow> rows = null;
		private string dbMarkTableName = string.Empty;

		//---------------------------------------------------------------------
		public string TableName { get { return dbMarkTableName; } }
		public List<DBMarkRow> Rows { get { return rows; } }

		//---------------------------------------------------------------------
		public DBMarkTable(TBConnection tbConnection, string dbMarkTableName) // potremmo togliere il passaggio del nome tabella, visto che il sysdb non esiste piu'
		{
			this.tbConnection = tbConnection;

			this.dbMarkTableName = dbMarkTableName;
			if (rows == null)
				rows = new List<DBMarkRow>();
			// vedere se chiamare subito la Load
		}

		/// <summary>
		/// Viene riempita la struttura in memoria con tutte le righe della DBMark
		/// </summary>
		//---------------------------------------------------------------------------
		public void Load()
		{
			Rows.Clear();

			try
			{
				using (TBCommand command = new TBCommand(string.Format("SELECT * FROM {0}", dbMarkTableName), tbConnection))
				using (IDataReader reader = command.ExecuteReader())
					while (reader.Read())
					{
						DBMarkRow row = new DBMarkRow();
						row.Application		= reader["Application"].ToString();
						row.AddOnModule		= reader["AddOnModule"].ToString();
						row.DBRelease		= Convert.ToInt32(reader["DBRelease"]);
						row.Status			= (reader["Status"].ToString() == "1");
						row.ExecLevel3		= (reader["ExecLevel3"].ToString() == "1");
						row.UpgradeLevel	= Convert.ToInt32(reader["UpgradeLevel"]);
						row.Step			= Convert.ToInt32(reader["Step"]);
						rows.Add(row);
					}
			}
			catch (Exception e)
			{
				Debug.WriteLine(string.Format("Error loading info from {0} table", dbMarkTableName));
				Debug.WriteLine(e.Message);
			}
		}

		//---------------------------------------------------------------------------
		public bool InsertRow(DBMarkRow rowToInsert)
		{
			string insertQuery = string.Format
				(
					@"INSERT INTO {0} (Application, AddOnModule, DBRelease, Status, ExecLevel3, UpgradeLevel, Step) 
					VALUES (@Application, @AddOnModule, @DBRelease, @Status, @ExecLevel3, @UpgradeLevel, @Step)",
					dbMarkTableName
				);

			try
			{
				using (TBCommand command = new TBCommand(insertQuery, tbConnection))
				{
					command.Parameters.Add("@Application",	rowToInsert.Application);
					command.Parameters.Add("@AddOnModule",	rowToInsert.AddOnModule);
					command.Parameters.Add("@DBRelease",	rowToInsert.DBRelease);
					command.Parameters.Add("@Status",		rowToInsert.Status);
					command.Parameters.Add("@ExecLevel3",	rowToInsert.ExecLevel3);
					command.Parameters.Add("@UpgradeLevel", rowToInsert.UpgradeLevel);
					command.Parameters.Add("@Step",			rowToInsert.Step);
					command.ExecuteNonQuery();
				}
			}
			catch (TBException e)
			{
				Debug.WriteLine(string.Format(DatabaseManagerStrings.ErrUpdatingTableDBMark, dbMarkTableName));
				Debug.WriteLine(e.Message);
				return false;
			}
			return true;
		}

		//---------------------------------------------------------------------------
		public bool UpdateRow(DBMarkRow rowToUpdate)
		{
			string updateQuery = string.Format
				(
					@"UPDATE {0} SET DBRelease = @DBRelease, UpgradeLevel = @UpgradeLevel, Status = @Status, Step = @Step
					WHERE Application = @Application AND AddOnModule = @AddOnModule", dbMarkTableName
				);

			try
			{
				using (TBCommand command = new TBCommand(updateQuery, tbConnection))
				{
					command.Parameters.Add("@DBRelease",	rowToUpdate.DBRelease);
					command.Parameters.Add("@UpgradeLevel", rowToUpdate.UpgradeLevel);
					command.Parameters.Add("@Status",		rowToUpdate.Status);
					command.Parameters.Add("@Step",			rowToUpdate.Step);
					command.Parameters.Add("@Application",	rowToUpdate.Application);
					command.Parameters.Add("@AddOnModule",	rowToUpdate.AddOnModule);
					command.ExecuteNonQuery();
				}
			}
			catch (TBException e)
			{
				Debug.WriteLine(string.Format(DatabaseManagerStrings.ErrUpdatingTableDBMark, dbMarkTableName));
				Debug.WriteLine(e.Message);
				return false;
			}
			return true;
		}

		///<summary>
		/// controlla se nel database esiste la tabella TB_DBMark
		///</summary>
		//---------------------------------------------------------------------------
		public bool Exists()
		{
			// istanzio TBDatabaseSchema sulla connessione
			TBDatabaseSchema mySchema = new TBDatabaseSchema(tbConnection);
			return mySchema.ExistTable(dbMarkTableName);
		}

		//---------------------------------------------------------------------------
		public DBMarkRow GetRow(string application, string module)
		{
			return Rows.Find(r => (string.Compare(r.Application, application, StringComparison.OrdinalIgnoreCase) == 0 &&
								string.Compare(r.AddOnModule, module, StringComparison.OrdinalIgnoreCase) == 0));
		}

		/// <summary>
		/// Date le signature di applicazione e modulo, ritorna il numero di release indicato nella DBMark
		/// </summary>
		//---------------------------------------------------------------------------
		public int GetDBReleaseFromDBMark(string application, string module, bool checkForMago4MigrationKit = false)
		{
			DBMarkRow row = GetRow(application, module);
			if (row != null)
				return row.DBRelease;

			// il flag checkForMago4MigrationKit serve per evitare il problema del check per la migrazione dati
			// nel caso non esista il modulo ERP-Core non faccio il test del migration kit
			return checkForMago4MigrationKit ? 401 : -1;
		}

		/// <summary>
		/// Data la stringa Application.AddOnModule.DBRelease (ad es ERP.Core.34)
		/// ritorna la riga corrispondente a quell'Applicazione + AddOnModule
		/// </summary>
		//---------------------------------------------------------------------------
		public DBMarkRow GetRow(string appModRel)
		{
			string application = string.Empty;
			string module = string.Empty;

			string[] str = appModRel.Split(new Char[] { '.' });

			// prima estrapolo il nome dell'application
			application = str[0].ToString();

			// se la Length è > 1 vuol dire che mi hanno passato il namespace
			// in questo caso estrapolo anche il module name e anche il numero di release
			if (str.Length > 1)
				module = str[1].ToString();

			return GetRow(application, module);
		}

		///<summary>
		/// Ritorna un nuovo oggetto DBMarkTable con le sole righe con lo Status = false
		/// per controllare i moduli che necessitano di una procedura di ripristino
		///</summary>
		//---------------------------------------------------------------------
		public DBMarkTable GetRecoveryRows()
		{
			DBMarkTable tblRecovery = new DBMarkTable(tbConnection, dbMarkTableName);
			/*foreach (DBMarkRow r in this.Rows)
			{
				if (r.Status)
					continue;
				tblRecovery.Rows.Add(r);
			}*/

			tblRecovery.Rows.AddRange(this.Rows.FindAll(rec => (!rec.Status)));

			return tblRecovery;
		}


		/// <summary>
		/// controllo sulla tabella DB_Mark se esiste una riga già inserita per quell'
		/// applicazione + modulo + numero di release. se esiste controllo anche il suo status.
		/// </summary>
		//---------------------------------------------------------------------------
		public bool CheckEntryInDBMark
		(
			string nameSpace,
			out string application,
			out string module,
			out int release,
			out bool status
		)
		{
			bool exist = false;

			status = false;
			application = string.Empty;
			module = string.Empty;
			release = 0;
			string[] str = nameSpace.Split(new Char[] { '.' });

			// prima estrapolo il nome dell'application
			application = str[0].ToString();

			// se la Length è > 1 vuol dire che mi hanno passato il namespace
			// in questo caso estrapolo anche il module name e anche il numero di release
			if (str.Length > 1)
				module = str[1].ToString();
			if (str.Length > 2)
				release = Convert.ToInt32(str[2]);

			// se la connessione è appesa la chiudo e faccio la Dispose
			// se è già chiusa faccio la Dispose
			if (tbConnection.State == ConnectionState.Closed ||
				tbConnection.State == ConnectionState.Broken)
			{
				if (tbConnection.State == ConnectionState.Broken)
					tbConnection.Close();

				tbConnection.Dispose();
				return false;
			}

			try
			{
				string query = string.Format("SELECT * FROM {0} WHERE Application = @app AND AddOnModule = @module", dbMarkTableName);

				TBCommand tbCommand = new TBCommand(query, tbConnection);
				tbCommand.Parameters.Add("@app", application);
				tbCommand.Parameters.Add("@module", module);
				using (IDataReader reader = tbCommand.ExecuteReader())
					while (reader.Read())
					{
						status = Convert.ToBoolean(int.Parse(reader["Status"].ToString()));
						exist = true;
					}
			}
			catch (TBException e)
			{
				Debug.WriteLine(string.Format("Error loading info from {0} table", dbMarkTableName));
				Debug.WriteLine(e.Message);
				exist = false;
			}

			return exist;
		}

		///<summary>
		/// Aggiorna il datatable della DBMark caricato in memoria 
		/// modificando la vecchia signature con la nuova coppia application+module
		/// ed eventualmente anche il nr di release se necessario
		/// (serve per il SOLO check del database in modo da proporre le informazioni corrette)
		///</summary>
		//---------------------------------------------------------------------------
		public PreviousSignatureInfo CreatePreviousSignatureInfo(ModuleDBInfo module)
		{
			PreviousSignatureInfo psi = null;

			try
			{
				// prima carico la riga con la previous app+module
				DBMarkRow previousAppRow = GetRow(module.PreviousApplication, module.PreviousModule);
				if (previousAppRow == null)
					return null; // se non esiste ritorno subito e tutto funziona come prima

				// controllare che non esista gia' una riga con la nuova app, altrimenti l'update del DT mi va in violazione di PK
				DBMarkRow currentAppRow = GetRow(module.ApplicationSign, module.ModuleSign);
				if (currentAppRow == null)
				{
					previousAppRow.Application = module.ApplicationSign;
					previousAppRow.AddOnModule = module.ModuleSign;
				}

				psi = new PreviousSignatureInfo(module.ApplicationSign, module.ModuleSign, module.PreviousApplication, module.PreviousModule);

				if (previousAppRow.DBRelease > module.DBRelease)
				{
					previousAppRow.DBRelease = module.DBRelease;
					psi.PreviousDBReleaseInDBMark = module.DBRelease;
				}
			}
			catch (Exception e)
			{
				Debug.WriteLine(string.Format("Error in DBMarkTable::CreatePreviousSignatureInfo (ApplicationSignature: {0} ModuleSignature: {1} - PreviousApplication: {2} - PreviousModule: {3})",
					module.ApplicationSign, module.ModuleSign, module.PreviousApplication, module.PreviousModule));
				Debug.WriteLine(e.Message);
				return null;
			}

			return psi;
		}

		///<summary>
		/// Update preventivo nella tabella DBMark per aggiornare la vecchia signature 
		/// con	quella nuova
		///</summary>
		//---------------------------------------------------------------------------
		public bool UpdatePreviousSignature(List<PreviousSignatureInfo> previousSignatureList, out string message)
		{
			message = string.Empty;

			// se la connessione è appesa la chiudo e faccio la Dispose
			if (tbConnection.State == ConnectionState.Closed || tbConnection.State == ConnectionState.Broken)
			{
				if (tbConnection.State == ConnectionState.Broken)
					tbConnection.Close();

				tbConnection.Dispose();
				return false;
			}

			string existRow = "SELECT COUNT(*) FROM {0} WHERE Application = @app AND AddOnModule = @mod";
			string updateRow = "UPDATE {0} SET Application = @app, AddOnModule = @mod WHERE Application = @previousApp AND AddOnModule = @previousMod";
			string fullUpdateRow = "UPDATE {0} SET Application = @app, AddOnModule = @mod, DBRelease = @rel WHERE Application = @previousApp AND AddOnModule = @previousMod";
			string deleteRow = "DELETE FROM {0} WHERE Application = @previousApp AND AddOnModule = @previousMod";

			IDbTransaction transaction = tbConnection.BeginTransaction();

			try
			{
				using (TBCommand cmd = new TBCommand(tbConnection))
				{
					// metto tutto sotto transazione, cosi se qualcosa non va a buon fine non invalido i dati
					// della TB_DBMark di partenza
					cmd.Transaction = transaction;

					foreach (PreviousSignatureInfo psi in previousSignatureList)
					{
						cmd.Parameters.Clear();
						cmd.CommandText = string.Format(existRow, dbMarkTableName);
						cmd.Parameters.Add("@app", psi.PreviousApplication);
						cmd.Parameters.Add("@mod", psi.PreviousModule);
						// controllo se esiste la riga con la applicazione precedente
						bool existPreviousAppRow = cmd.ExecuteTBScalar() > 0;

						cmd.Parameters.Clear();

						cmd.CommandText = string.Format(existRow, dbMarkTableName);
						cmd.Parameters.Add("@app", psi.NewApplication);
						cmd.Parameters.Add("@mod", psi.NewModule);
						// controllo se esiste la riga con la nuova applicazione (altrimenti ho problemi di PK duplicata)
						bool existNewAppRow = cmd.ExecuteTBScalar() > 0;

						cmd.Parameters.Clear();

						// se la riga con la vecchia app NON esiste vado avanti 
						// (non dovrebbe mai succedere perche' viene gia' fatto un controllo a monte)
						if (!existPreviousAppRow)
							continue;

						// anche la riga con la nuova app esiste
						if (existNewAppRow)
						{
							// aggiorno la riga della nuova app 
							if (psi.PreviousDBReleaseInDBMark > 0)
								cmd.CommandText = string.Format(fullUpdateRow, dbMarkTableName);
							else
								cmd.CommandText = string.Format(updateRow, dbMarkTableName);

							cmd.Parameters.Add("@app", psi.NewApplication);
							cmd.Parameters.Add("@mod", psi.NewModule);
							cmd.Parameters.Add("@previousApp", psi.NewApplication);
							cmd.Parameters.Add("@previousMod", psi.NewModule);
							if (psi.PreviousDBReleaseInDBMark > 0)
								cmd.Parameters.Add("@rel", psi.PreviousDBReleaseInDBMark);
							cmd.ExecuteNonQuery();

							// elimino la riga della vecchia app
							cmd.Parameters.Clear();
							cmd.CommandText = string.Format(deleteRow, dbMarkTableName);
							cmd.Parameters.Add("@previousApp", psi.PreviousApplication);
							cmd.Parameters.Add("@previousMod", psi.PreviousModule);
							cmd.ExecuteNonQuery();
						}
						else
						{
							// se la riga della nuova app non esiste faccio solo l'update della riga
							// con la vecchia app sostituendo i valori
							if (psi.PreviousDBReleaseInDBMark > 0)
								cmd.CommandText = string.Format(fullUpdateRow, dbMarkTableName);
							else
								cmd.CommandText = string.Format(updateRow, dbMarkTableName);
							cmd.Parameters.Add("@app", psi.NewApplication);
							cmd.Parameters.Add("@mod", psi.NewModule);
							cmd.Parameters.Add("@previousApp", psi.PreviousApplication);
							cmd.Parameters.Add("@previousMod", psi.PreviousModule);
							if (psi.PreviousDBReleaseInDBMark > 0)
								cmd.Parameters.Add("@rel", psi.PreviousDBReleaseInDBMark);
							cmd.ExecuteNonQuery();
						}
					}
				}

				transaction.Commit();
			}
			catch (TBException e)
			{
				message = e.Message;
				Debug.WriteLine(string.Format("Error DBMarkTable::UpdatePreviousSignature for {0} table", dbMarkTableName));
				Debug.WriteLine(e.Message);
				transaction.Rollback();
				return false;
			}
			finally
			{
				if (transaction != null)
					transaction.Dispose();
			}

			return true;
		}
	}

	///<summary>
	/// DBMarkInfo
	/// Si occupa caricare in memoria in un DataTable le informazioni della tabella DBMark
	/// Inoltre espone metodi per inserire/aggiornare le righe nella tabella a seconda 
	/// dell'aggiornamento del database effettuato, nonche' ritorna alcune informazioni utili
	/// (ad es. se esiste la tabella, il numero di release di una modulo specifico, etc.)
	///</summary>
	//=========================================================================
	public class DBMarkInfo
	{
		public DBMarkTable DBMarkTable { get; private set; }

		///<summary>
		/// Costruttore che istanzia la classe ma non carica le informazioni della tabella
		///</summary>
		///<param name="tbConnection">puntatore alla connessione aperta sul database da analizzare</param>
		//---------------------------------------------------------------------
		public DBMarkInfo(TBConnection tbConnection) 
		{
			DBMarkTable = new DBMarkTable(tbConnection, DatabaseLayerConsts.TB_DBMark);
		}

		/// <summary>
		/// Inserisce una riga nella DBMark per tutti i moduli presenti nella lista 
		/// Per tutti i moduli per i quali e' necessario inserire una riga nella DBMark
		/// procedo 
		/// tramite la definizione di un InsertCommand su un TBDataAdapeter, eseguo una query di Insert
		/// sul DataTable creato in memoria dalla tabella DBMark
		/// </summary>
		/// <param name="moduleList">array dei moduli da considerare</param>
		/// <param name="error">ritorna un'eventuale stringa di errore</param>
		/// <returns>successo dell'operazione</returns>
		//---------------------------------------------------------------------------
		public bool InsertInDBMark(ModuleDBInfoList moduleList, out string error, bool onlyForMissingModules)
		{
			error = string.Empty;

			bool okInsert = true;

			// carico la struttura della tabella DBMark solo se il datatable è a null
			//if (dbMarkDataTable == null) LoadInfosFromDBMark();

			//@@TODOMICHI: da capire se va bene
			if (DBMarkTable == null)
				DBMarkTable.Load();

			try
			{
				// considero l'array dei moduli da creare oppure quello dei moduli mancanti
				foreach (ModuleDBInfo modInfo in moduleList)
				{
					// faccio l'update dell'entry del modulo solo se è necessario 
					// (cioè se l'elemento nell'array dei moduli è stato inserito solo 
					// per tenerne traccia o se ha subìto effettivamente un upgrade)
					if (modInfo.EntryOnlyInDBMark)
						continue;

					// se e' solo per i moduli mancanti e il modulo non e' missing non procedo
					if (onlyForMissingModules && !modInfo.IsNew)
						continue;

					DBMarkRow newRow = new DBMarkRow();
					newRow.Application	= modInfo.ApplicationSign;
					newRow.AddOnModule	= modInfo.ModuleSign;
					newRow.DBRelease	= modInfo.NrRelease;
					newRow.Status		= modInfo.StatusOk;
					newRow.ExecLevel3	= false;
					newRow.UpgradeLevel = modInfo.NrLevel;
					newRow.Step			= modInfo.NrStep;
					DBMarkTable.InsertRow(newRow);
				}

				okInsert = true;
			}
			catch (TBException e)
			{
				error = e.Message;
				Debug.WriteLine(string.Format(DatabaseManagerStrings.ErrInsertingTableDBMark, DBMarkTable.TableName));
				Debug.WriteLine(e.Message);
				okInsert = false;
			}

			return okInsert;
		}

		/// <summary>
		/// Update_DBMark
		/// tramite la definizione di UpdateCommand su un TBDataAdapter, eseguo una query di Update
		/// sul DataTable creato in memoria dalla tabella DBMark
		/// </summary>
		/// <param name="moduleDBInfoList">array dei moduli da considerare</param>
		/// <param name="error">ritorna un'eventuale stringa di errore</param>
		/// <returns>successo dell'operazione</returns>
		//---------------------------------------------------------------------------
		public bool Update_DBMark(ModuleDBInfoList moduleDBInfoList, out string error)
		{
			error = string.Empty;

			bool okUpdate = true;

			try
			{
				foreach (ModuleDBInfo modInfo in moduleDBInfoList)
				{
					if (modInfo.EntryOnlyInDBMark || modInfo.ErrorInFileXML || modInfo.IsNew)
						continue;

					DBMarkRow updateRow		= DBMarkTable.GetRow(modInfo.ApplicationSign, modInfo.ModuleSign);
					updateRow.DBRelease		= modInfo.NrRelease;
					updateRow.UpgradeLevel	= modInfo.NrLevel;
					updateRow.Status		= modInfo.StatusOk;
					updateRow.Step			= modInfo.NrStep;

					DBMarkTable.UpdateRow(updateRow);
				}
				okUpdate = true;
			}
			catch (TBException e)
			{
				error = e.Message;
				Debug.WriteLine(string.Format(DatabaseManagerStrings.ErrUpdatingTableDBMark, DBMarkTable.TableName));
				Debug.WriteLine(e.Message);
				okUpdate = false;
			}

			return okUpdate;
		}
	}
}