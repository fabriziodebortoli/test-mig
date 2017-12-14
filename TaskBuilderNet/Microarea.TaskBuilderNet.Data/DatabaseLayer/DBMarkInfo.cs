using System;
using System.Collections.Generic;
using System.Data;
using System.Diagnostics;

using Microarea.TaskBuilderNet.Interfaces;

namespace Microarea.TaskBuilderNet.Data.DatabaseLayer
{
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
		private TBConnection tbConnection = null;

		// DataTable in memoria con i dati della DBMark
		private DataTable dbMarkDataTable = null;
		private string dbMarkTableName = string.Empty;

		//---------------------------------------------------------------------
		public string DBMarkTableName { get { return dbMarkTableName; } }

		///<summary>
		/// Costruttore che istanzia la classe ma non carica le informazioni della tabella
		///</summary>
		///<param name="tbConnection">puntatore alla connessione aperta sul database da analizzare</param>
		///<param name="kinfOfDb">tipo di database</param>
		//---------------------------------------------------------------------
		public DBMarkInfo(TBConnection tbConnection, KindOfDatabase kinfOfDb) 
			:
			this(tbConnection, kinfOfDb, false)
		{
		}

		///<summary>
		/// Costruttore che istanzia la classe, carica le informazioni della tabella e relative righe sulla base
		/// del III parametro
		///</summary>
		///<param name="kinfOfDb">tipo di database</param>
		///<param name="tbConnection">puntatore alla connessione aperta sul database da analizzare</param>
		///<param name="withInfoLoading">true: carica info tabella, false: non carica info tabella</param>
		//---------------------------------------------------------------------
		public DBMarkInfo(TBConnection tbConnection, KindOfDatabase kinfOfDb, bool withInfoLoading)
		{
			this.tbConnection = tbConnection;

			// in base al flag stabilisco se devo leggere la TB_DBMark o la MSD_DBMark
			switch (kinfOfDb)
			{
				case KindOfDatabase.Company:
				case KindOfDatabase.Dms:
					{ 
						dbMarkTableName = DatabaseLayerConsts.TB_DBMark;
						break;
					}
				case KindOfDatabase.System:
					{
						dbMarkTableName = DatabaseLayerConsts.MSD_DBMark;
						break;
					}
			}

			if (withInfoLoading)
				LoadInfosFromDBMark();
		}

		///<summary>
		/// controlla se sul database aziendale esiste la tabella TB_DBMark oppure o 
		/// se sul sysdb esiste la tabella MSD_DBMark
		///</summary>
		///<returns>se la tabella esiste</returns>
		//---------------------------------------------------------------------------
		public bool CheckExistDBMark()
		{ 
			// istanzio TBDatabaseSchema sulla connessione
			TBDatabaseSchema mySchema = new TBDatabaseSchema(tbConnection);
			return mySchema.ExistTable(dbMarkTableName);
		}

		/// <summary>
		/// Date le signature di applicazione e modulo, ritorna il numero di release indicato nella DBMark
		/// </summary>
		//---------------------------------------------------------------------------
		public int GetDBReleaseFromDBMark(string application, string module, bool checkForMago4MigrationKit = false)
		{
			DataRow cRow = dbMarkDataTable.Rows.Find(new object[] { application, module });

			if (cRow != null)
				return Convert.ToInt32(cRow["DBRelease"]);

			foreach (DataRow row in dbMarkDataTable.Rows)
			{
				if (string.Compare(application, row["Application"].ToString(), StringComparison.InvariantCultureIgnoreCase) == 0 &&
					string.Compare(module, row["AddOnModule"].ToString(), StringComparison.InvariantCultureIgnoreCase) == 0)
					return Convert.ToInt32(row["DBRelease"]);
			}

			// il flag checkForMago4MigrationKit serve per evitare il problema del check per la migrazione dati
			// nel caso non esista il modulo ERP-Core non faccio il test del migration kit
			return checkForMago4MigrationKit ? 401 : -1;
		}

		/// <summary>
		/// LoadInfosFromDBMark
		/// Viene riempito il DataTable con le tutte le righe della DBMark
		/// </summary>
		//---------------------------------------------------------------------------
		public void LoadInfosFromDBMark()
		{
			TBDataAdapter adapt = new TBDataAdapter(string.Format("SELECT * FROM {0}", dbMarkTableName), tbConnection);

			try
			{
				dbMarkDataTable = new DataTable(dbMarkTableName);
				adapt.Fill(dbMarkDataTable);
				adapt.Dispose();
			}
			catch (TBException e)
			{
				if (adapt != null) 
					adapt.Dispose();
				Debug.WriteLine(string.Format("Error loading info from {0} table", dbMarkTableName));
				Debug.WriteLine(e.Message);
			}

			// setto le chiavi primarie del DataTable
			dbMarkDataTable.PrimaryKey =
				new DataColumn[] { dbMarkDataTable.Columns["Application"], dbMarkDataTable.Columns["AddOnModule"] };
		}

		/// <summary>
		/// InsertInDBMark
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

			bool okInsert = true;
			string query = string.Format
				(
					@"INSERT INTO {0} (Application, AddOnModule, DBRelease, Status, ExecLevel3, UpgradeLevel, Step) 
					VALUES (@Application, @AddOnModule, @DBRelease, @Status, @ExecLevel3, @UpgradeLevel, @Step)",
					dbMarkTableName
				);

			// carico la struttura della tabella DBMark solo se il datatable è a null
			if (dbMarkDataTable == null)
				LoadInfosFromDBMark();

			TBDataAdapter da = null;

			try
			{
				da = new TBDataAdapter(tbConnection);
				da.MissingSchemaAction = MissingSchemaAction.AddWithKey;
				da.InsertCommand = new TBCommand(query, tbConnection);

				int y = da.InsertCommand.Parameters.Add("@Application", SqlDbType.NVarChar, 20);
				TBParameter workParm = da.InsertCommand.Parameters.GetParameterAt(y);
				workParm.SourceColumn = "Application";
				workParm.SourceVersion = DataRowVersion.Original;

				int z = da.InsertCommand.Parameters.Add("@AddOnModule", SqlDbType.NVarChar, 40);
				TBParameter workParm1 = da.InsertCommand.Parameters.GetParameterAt(z);
				workParm1.SourceColumn = "AddOnModule";
				workParm1.SourceVersion = DataRowVersion.Original;

				da.InsertCommand.Parameters.Add("@DBRelease", SqlDbType.SmallInt, 6, "DBRelease");
				da.InsertCommand.Parameters.Add("@Status", SqlDbType.Char, 1, "Status");
				da.InsertCommand.Parameters.Add("@ExecLevel3", SqlDbType.Char, 1, "ExecLevel3");
				da.InsertCommand.Parameters.Add("@UpgradeLevel", SqlDbType.SmallInt, 6, "UpgradeLevel");
				da.InsertCommand.Parameters.Add("@Step", SqlDbType.SmallInt, 6, "Step");

				DataRow newRow;

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

					newRow = dbMarkDataTable.NewRow();

					newRow["Application"] = modInfo.ApplicationSign;
					newRow["AddOnModule"] = modInfo.ModuleSign;
					newRow["DBRelease"] = modInfo.NrRelease;
					newRow["Status"] = (modInfo.StatusOk) ? 1 : 0;
					newRow["ExecLevel3"] = 0;
					newRow["UpgradeLevel"] = modInfo.NrLevel;
					newRow["Step"] = modInfo.NrStep;

					dbMarkDataTable.Rows.Add(newRow);
				}

				da.Update(dbMarkDataTable);
				da.Dispose();
				okInsert = true;
			}
			catch (TBException e)
			{
				error = e.Message;
				Debug.WriteLine(string.Format(DatabaseLayerStrings.ErrInsertingTableDBMark, dbMarkTableName));
				Debug.WriteLine(e.Message);
				okInsert = false;
				if (da != null) da.Dispose();
			}
			catch (System.Data.ConstraintException e)
			{
				error = e.Message;
				Debug.WriteLine(string.Format("InsertInDBMark. System.Data.ConstraintException {0}", e.Message));
				okInsert = false;
				if (da != null) da.Dispose();
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

			bool okUpdate = true;
			string query = string.Format
				(
					@"UPDATE {0} SET DBRelease = @rel, UpgradeLevel = @lev, Status = @status, Step = @step
					WHERE Application = @app AND AddOnModule = @module", dbMarkTableName
				);

			TBDataAdapter da = null;
			try
			{
				da = new TBDataAdapter(tbConnection);
				da.MissingSchemaAction = MissingSchemaAction.AddWithKey;
				da.UpdateCommand = new TBCommand(query, tbConnection);

				da.UpdateCommand.Parameters.Add("@rel", SqlDbType.SmallInt, 6, "DBRelease");
				da.UpdateCommand.Parameters.Add("@lev", SqlDbType.SmallInt, 6, "UpgradeLevel");
				da.UpdateCommand.Parameters.Add("@status", SqlDbType.Char, 1, "Status");
				da.UpdateCommand.Parameters.Add("@step", SqlDbType.SmallInt, 6, "Step");

				int y = da.UpdateCommand.Parameters.Add("@app", SqlDbType.NVarChar, 20);
				TBParameter workParm = da.UpdateCommand.Parameters.GetParameterAt(y);
				workParm.SourceColumn = "Application";
				workParm.SourceVersion = DataRowVersion.Original;

				int z = da.UpdateCommand.Parameters.Add("@module", SqlDbType.NVarChar, 40);
				TBParameter workParm1 = da.UpdateCommand.Parameters.GetParameterAt(z);
				workParm1.SourceColumn = "AddOnModule";
				workParm1.SourceVersion = DataRowVersion.Original;

				foreach (ModuleDBInfo modInfo in moduleDBInfoList)
				{
					if (modInfo.EntryOnlyInDBMark || modInfo.ErrorInFileXML || modInfo.IsNew)
						continue;

					DataRow updateRow =
						dbMarkDataTable.Rows.Find(new object[] { modInfo.ApplicationSign, modInfo.ModuleSign });

					updateRow["DBRelease"] = modInfo.NrRelease;
					updateRow["UpgradeLevel"] = modInfo.NrLevel;
					updateRow["Status"] = (modInfo.StatusOk) ? 1 : 0;
					updateRow["Step"] = modInfo.NrStep;
				}

				da.Update(dbMarkDataTable);
				okUpdate = true;
				da.Dispose();
			}
			catch (TBException e)
			{
				error = e.Message;
				Debug.WriteLine(string.Format(DatabaseLayerStrings.ErrUpdatingTableDBMark, dbMarkTableName));
				Debug.WriteLine(e.Message);
				okUpdate = false;
				if (da != null) da.Dispose();
			}

			return okUpdate;
		}

		///<summary>
		/// Ritorna un DataTable con le sole righe con lo Status = false
		/// per controllare i moduli che necessitano di una procedura di ripristino
		///</summary>
		//---------------------------------------------------------------------------
		public DataTable GetDBRecoveryStatus()
		{
			// The correct way is to clone dbMarkColumns on dtRecovery, this creates the appropriate columns, 
			// then when you want to copy the row from dbMarkColumns into dtRecovery you have to use the ImportRow Method.

			// clono la struttura del DataTable originale
			DataTable dtRecovery = dbMarkDataTable.Clone();

			foreach (DataRow row in dbMarkDataTable.Rows)
			{
				if (Convert.ToInt32(row["Status"]) == 0)
					dtRecovery.ImportRow(row); // non posso utilizzare il metodo Rows.Add(row), ma ImportRow()!!!
			}

			return dtRecovery;
		}

		/// <summary>
		/// controllo sulla tabella DB_Mark se esiste una riga già inserita per quell'
		/// applicazione + modulo + numero di release. se esiste controllo anche il suo status.
		/// </summary>
		// TODO: migliorare, accedendo direttamente al datatable?
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

			// per pararci con Oracle a cui non piacciono le stringhe vuote ma le vuole con il blank
			if (tbConnection.IsOracleConnection())
			{
				if (string.IsNullOrEmpty(application))
					application = " ";
				if (string.IsNullOrEmpty(module))
					module = " ";
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
				DataRow previousAppRow = dbMarkDataTable.Rows.Find(new object[] { module.PreviousApplication, module.PreviousModule });
				if (previousAppRow == null)
					return null; // se non esiste ritorno subito e tutto funziona come prima

				// controllare che non esista gia' una riga con la nuova app, altrimenti l'update del DT mi va in violazione di PK
				DataRow currentAppRow = dbMarkDataTable.Rows.Find(new object[] { module.ApplicationSign, module.ModuleSign });
				if (currentAppRow == null)
				{
					previousAppRow["Application"] = module.ApplicationSign;
					previousAppRow["AddOnModule"] = module.ModuleSign;
				}

				psi = new PreviousSignatureInfo(module.ApplicationSign, module.ModuleSign, module.PreviousApplication, module.PreviousModule);

				int prevDbRel = Convert.ToInt32(previousAppRow["DBRelease"]);
				if (prevDbRel > module.DBRelease)
				{
					previousAppRow["DBRelease"] = module.DBRelease;
					psi.PreviousDBReleaseInDBMark = module.DBRelease;
				}

				dbMarkDataTable.AcceptChanges();
			}
			catch (Exception e)
			{
				Debug.WriteLine(string.Format("Error in DBMarkInfo::CreatePreviousSignatureInfo (ApplicationSignature: {0} ModuleSignature: {1} - PreviousApplication: {2} - PreviousModule: {3})",
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
			// se è già chiusa faccio la Dispose
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
				Debug.WriteLine(string.Format("Error DBMarkInfo::UpdatePreviousSignature for {0} table", dbMarkTableName));
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

		///<summary>
		/// Update della tabella DBMark per aggiornare i numeri di release con la gestione Rewind
		///</summary>
		//---------------------------------------------------------------------------
		public bool UpdateDevelopmentModuleRelease(List<DevelopmentModuleRelease> modulesList, out string message)
		{
			message = string.Empty;

			string updateRow = "UPDATE {0} SET DBRelease = @rel WHERE Application = @app AND AddOnModule = @mod";
			// @@TODO: perfezionare discriminando i moduli con versioni piu' vecchie
			// WHERE Application = @previousApp AND AddOnModule = @previousMod";

			IDbTransaction transaction = null;

			try
			{
				transaction = tbConnection.BeginTransaction();

				using (TBCommand cmd = new TBCommand(tbConnection))
				{
					// metto tutto sotto transazione, cosi se qualcosa non va a buon fine non invalido i dati della DBMark di partenza
					cmd.Transaction = transaction;

					foreach (DevelopmentModuleRelease dmr in modulesList)
					{
						cmd.Parameters.Clear();
						cmd.CommandText = string.Format(updateRow, dbMarkTableName);
						cmd.Parameters.Add("@app", dmr.Application);
						cmd.Parameters.Add("@mod", dmr.Module);
						cmd.Parameters.Add("@rel", (dmr.DBRelease - 1));
						cmd.ExecuteNonQuery();
					}
				}

				transaction.Commit();
			}
			catch (TBException e)
			{
				message = string.Format("Error DBMarkInfo::UpdateDevelopmentModuleInfo for {0} table ended with errors {1}", dbMarkTableName, e.Message);
				Debug.WriteLine(message);
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
}
