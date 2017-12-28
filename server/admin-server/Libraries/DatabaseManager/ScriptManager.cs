using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Reflection;
using System.Text.RegularExpressions;
using Microarea.Common.DiagnosticManager;
using TaskBuilderNetCore.Interfaces;

namespace Microarea.AdminServer.Libraries.DatabaseManager
{
	/// <summary>
	/// Classe che si occupa dell'esecuzione di script, applicando tutte le regole e sostituzione di caratteri
	/// per creare database in formato unicode o meno, per applicare la collation su colonne
	/// </summary>
	// ========================================================================
	public class ScriptManager
	{
		#region Private variables
		// per sapere se devo gestire la trasformazione da varchar a nvarchar (idem per char e text)
		private bool useUnicode = false;
		// tipo di database (per impostare o meno la COLLATE di colonna)
		private KindOfDatabase kindOfDb = KindOfDatabase.Company;
		// ritorna l'LCID della DatabaseCulture dell'azienda da cui estrapolare la COLLATE da applicare alle colonne
		private int databaseCulture = 0;
		// se il database a cui sono connessa  vuoto o contiene delle tabelle
		private bool isEmptyDB = false;

		// nome COLLATION estrapolata dall'LCID della DatabaseCulture
		private string columnCollation = string.Empty;
		// nome COLLATION estrapolata letta o dal database o dal server SQL
		private string databaseCollation = string.Empty;
		// se columnCollation e databaseCollation sono diverse 
		private bool differentCollation = false;

		// per i database DMS, serve per identificare se l'LCID della cultura scelta per il database
		// ha una corrispondenza nei languages del Full Text Search
		private bool isSupportedLanguageForFullTextSearch = false;

		// per non fare il controllo della Collation (di default è a true)
		// (usata nell'esecuzione degli script del MigrationKit che non tengono conto della Collation)
		private bool checkDifferentCollation = true;

		private string stringConnection = string.Empty;
		private TBConnection connection = null;
		private FileInfo fi = null;

		private Diagnostic diagnostic = null;
		#endregion

		#region Properties
		//---------------------------------------------------------------------
		public bool UseUnicode { get { return useUnicode; } set { useUnicode = value; } }

		//---------------------------------------------------------------------
		public TBConnection Connection
		{
			get { return connection; }

			set
			{
				connection = value;

				// per skippare a priori il controllo della collation (MigrationKit e RegressionTest)
				// GESTIONE PER IL SOLO DATABASE AZIENDALE 
				if (checkDifferentCollation && kindOfDb != KindOfDatabase.System)
				{
					// SQL SERVER
					if (connection.IsSqlConnection())
					{
						// questa mi serve cmq per passarla alla Regex
						columnCollation = string.Empty; //CultureHelper.GetWindowsCollation(this.databaseCulture); //@@TODOMICHI

						databaseCollation = TBCheckDatabase.GetDatabaseCollation(connection);
						if (databaseCollation.Length <= 0)
							databaseCollation = TBCheckDatabase.GetServerCollation(connection);

						// se la collation non è compatibile con l'LCID allora le considero diverse e la applico
						differentCollation = false; //!CultureHelper.IsCollationCompatibleWithCulture(this.databaseCulture, databaseCollation); //@@TODOMICHI

						// Devo effettuare un ulteriore controllo per evitare di applicare una Collation mista 
						// e quindi di incappare in errori strani eseguendo gli script
						// Se il db non è vuoto e la columnCollation da applicare risulta non compatibile con quella della
						// cultura dell'azienda, allora devo controllare se la columnCollation è diversa dalla COLLATE della 
						// colonna Status della TB_DBMark. Se così fosse non devo applicare la collation per evitare conflitti
						if (differentCollation && !isEmptyDB)
						{
							if (string.Compare(columnCollation, TBCheckDatabase.GetColumnCollation(connection), StringComparison.OrdinalIgnoreCase) != 0)
								differentCollation = false;
						}

						if (kindOfDb == KindOfDatabase.Dms)
							isSupportedLanguageForFullTextSearch = TBCheckDatabase.IsSupportedLanguageForFullTextSearch(connection, this.databaseCulture);
					}

					if (connection.IsPostgreConnection()) differentCollation = false;

				}
			}
		}

		//---------------------------------------------------------------------
		public Diagnostic Diagnostic { get { return diagnostic; } }
		#endregion

		#region Constructors
		/// <summary>
		/// Costruttore 3 parametri
		/// Usato dall'ApplicationDBAdmin in fase di creazione/aggiornamento database
		/// </summary>
		/// <param name="useUnicode">se il db usa l'unicode</param>
		/// <param name="kindOfDb">tipo database</param>
		/// <param name="dbCulture">database culture associata al db</param>
		/// <param name="isEmptyDB">se il database è vuoto</param>
		//---------------------------------------------------------------------
		public ScriptManager(bool useUnicode, KindOfDatabase kindOfDb, int dbCulture, bool isEmptyDB, Diagnostic diagnostic)
		{
			this.useUnicode = useUnicode;
			this.kindOfDb = kindOfDb;
			this.databaseCulture = dbCulture;
			this.isEmptyDB = isEmptyDB;
			this.diagnostic = diagnostic;
		}

		/// <summary>
		/// Costruttore 1 parametro
		/// USATA SOLO DAL MIGRATIONKIT E REGRESSIONTEST!!!
		/// consente di skippare il controllo della collation tra culture di database e colonna
		/// </summary>
		/// <param name="checkDifferentCollation">false: per skippare il controllo della collation. il default è true</param>
		//---------------------------------------------------------------------
		public ScriptManager(bool checkDifferentCollation)
		{
			this.checkDifferentCollation = checkDifferentCollation;
			diagnostic = new Diagnostic("ScriptManager");
		}
		#endregion

		#region Split - Interpreta le porzioni di script separate dal GO
		/// <summary>
		/// Split
		/// </summary>
		//---------------------------------------------------------------------
		private void Split(string streamToSplit, out List<string> batches)
		{
			batches = new List<string>();
			int pos = 0;
			int start = 0;

			if (streamToSplit.Length == 0)
				return;

			string stream =
				Regex.Replace(streamToSplit, @"\bgo\b", "GO", RegexOptions.IgnoreCase | RegexOptions.Compiled);

			while (true)
			{
				pos = 0;

				// devo eliminare i blank /r /n all'inizio della riga altrimenti Oracle si incastra
				while (pos < stream.Length && IsCarriageReturn(stream[pos]))
					pos++;

				// se una stringa di soli blank /r /n
				if (pos == stream.Length)
					return;

				stream = stream.Substring(pos);

				pos = stream.IndexOf("GO", start);

				if (pos == -1)
				{
					if (IsValidString(stream))
						batches.Add(stream);

					break;
				}
				else
				{
					if (
						((pos - 1 >= 0) && !IsCarriageReturn(stream[pos - 1])) ||
						((pos + 2 < stream.Length) && !IsCarriageReturn(stream[pos + 2]))
						)
					{
						start = pos + 2;
						continue;
					}

					start = 0;
					//devo eliminare i blank /r /n in fondo alla riga altrimenti Oracle si incastra
					int remPos = 2;
					while ((pos - 1 >= 0) && IsCarriageReturn(stream[pos - 1]))
					{
						pos--;
						remPos++;
					}
					batches.Add(stream.Substring(0, pos));
					stream = stream.Remove(0, pos + remPos);
				}
			}
		}
		#endregion

		#region Funzioni per il check di un char o di una stringa (per isolare il GO)
		/// <summary>
		/// se il char ?un carriage return, un new line o uno spazio bianco (include anche il tab)
		/// </summary>
		//---------------------------------------------------------------------
		private bool IsCarriageReturn(char c)
		{
			return (c == '\r' || c == '\n' || char.IsWhiteSpace(c));
		}

		/// <summary>
		/// se una stringa ?valida: non ?vuota o non ?un carattere carrige, new line o spazio bianco
		/// </summary>
		//---------------------------------------------------------------------
		private bool IsValidString(string s)
		{
			if (s.Length == 0)
				return false;

			for (int i = 0; i < s.Length; i++)
			{
				if (!IsCarriageReturn(s[i]))
					return true;
			}

			return false;
		}
		#endregion

		#region ExecuteFileSql - Apre un file con estensione .sql (con il path completo) ed esegue ogni statement SQL presente nello script
		/// <summary>
		/// ExecuteFileSql
		/// Apre un file con estensione .sql (con il path completo) ed esegue 
		/// ogni statement SQL presente nello script
		/// </summary>
		/// <param name="pathFileSql">path completo del file</param>
		/// <param name="error">stringa dell'eventuale errore</param>
		/// <returns>la corretta esecuzione dello script</returns>
		//---------------------------------------------------------------------------
		public bool ExecuteFileSql(string pathFileSql, out string error)
		{
			return ExecuteFileSql(pathFileSql, out error, 0);
		}

		/// <summary>
		/// ExecuteFileSql
		/// Apre un file con estensione .sql (con il path completo) ed esegue 
		/// ogni statement SQL presente nello script
		/// </summary>
		/// <param name="pathFileSql">path completo del file</param>
		/// <param name="error">stringa dell'eventuale errore</param>
		/// <param name="timeout">timeout da impostare esternamente (di default e' 0, ovvero infinito)
		/// <remarks>peccato che non sempre impostare timeout infinito funziona</remarks></param>
		/// <returns>la corretta esecuzione dello script</returns>
		//---------------------------------------------------------------------------
		public bool ExecuteFileSql(string pathFileSql, out string error, int timeout)
		{
			error = string.Empty;
			string scriptText = string.Empty;

			fi = new FileInfo(pathFileSql);

			if (!fi.Exists)
			{
				error = DatabaseManagerStrings.FileNotFound;
				Diagnostic.Set(DiagnosticType.Error, DatabaseManagerStrings.FileNotFound);
				return false;
			}

			try
			{
				using (Stream fs = fi.Open(FileMode.Open, FileAccess.Read, FileShare.None))
				using (StreamReader reader = new StreamReader(fs))
					scriptText = reader.ReadToEnd();
			}
			catch (IOException e)
			{
				Debug.Fail(e.Message);
				ExtendedInfo extendedInfo = new ExtendedInfo();
				extendedInfo.Add(DatabaseManagerStrings.Description, e.Message);
				extendedInfo.Add(DatabaseManagerStrings.Function, "ExecuteFileSql");
				extendedInfo.Add(DatabaseManagerStrings.Library, "Microarea.TaskBuilderNet.Data.DatabaseLayer");
				extendedInfo.Add(DatabaseManagerStrings.Source, e.Source);
				extendedInfo.Add(DatabaseManagerStrings.StackTrace, e.StackTrace);
				error = e.Message;
				Diagnostic.Set(DiagnosticType.Error, e.Message, extendedInfo);
				return false;
			}

			return ExecuteSql(scriptText, out error, false, timeout);
		}

		//---------------------------------------------------------------------------
		public bool ExecuteSql(string scriptText, out string error)
		{
			return ExecuteSql(scriptText, out error, true);
		}

		//---------------------------------------------------------------------------
		public bool ExecuteSql(string scriptText, out string error, bool altOnError)
		{
			return ExecuteSql(scriptText, out error, altOnError, 30);
		}

		//---------------------------------------------------------------------------
		public bool ExecuteSql(string scriptText, out string error, bool altOnError, int timeOut)
		{
			int rowAffected = 0;

			return ExecuteSql(scriptText, out error, altOnError, timeOut, out rowAffected);
		}

		//---------------------------------------------------------------------------
		public bool ExecuteSql(string scriptText, out string error, bool altOnError, int timeOut, out int rowAffected)
		{
			string strConvert = string.Empty;

			// applico la Regex per la gestione dei caratteri unicode (SQL + ORACLE + ogni database)
			if (useUnicode)
				ApplyUnicodeRegex(scriptText, out strConvert);
			else
				strConvert = scriptText;

			rowAffected = 0;
			error = string.Empty;

			bool errorFound = false;

			// gli statement da eseguire sono splittati tramite il GO
			Split(strConvert, out List<string> strList);

			if (fi != null)
				this.diagnostic.Set(DiagnosticType.LogOnFile, string.Format(DatabaseManagerStrings.ProcessingFile, fi.FullName));

			TBCommand command = new TBCommand(connection);
			command.CommandTimeout = timeOut;

			for (int i = 0; i < strList.Count; i++)
			{
				string statement = strList[i].ToString();

				// se lo statement è empty passo allo statement successivo (nel caso di GO non corretti)
				if (string.IsNullOrWhiteSpace(statement))
					continue;

				// se si tratta del db aziendale su SQL Server devo effettuare degli ulteriori controlli
				// sulle porzioni di script, skippando view, stored procedure e functions
				// ed applicando le Regex per la sostituzione della Collation sulle colonne
				if (kindOfDb != KindOfDatabase.System && !connection.IsPostgreConnection())
				{
					if (!IsSpecialCommand(statement))
					{
						statement = string.Empty;

						if (connection.IsSqlConnection())
							ApplyCollationRegex(strList[i].ToString(), out statement);
					}
				}

				if (kindOfDb == KindOfDatabase.Company && connection.IsSqlConnection())
				{
					string script = statement;
					ApplyMandatoryColumnRegex(script, out statement);
				}

				Diagnostic.Set(DiagnosticType.LogOnFile, string.Format(DatabaseManagerStrings.ExecutionOfStatement, statement));

				try
				{
					command.CommandText = statement;
					rowAffected += command.ExecuteNonQuery();
					Diagnostic.Set(DiagnosticType.LogOnFile, DatabaseManagerStrings.ElaborationSuccessfullyCompleted);
				}
				catch (TBException e)
				{
					// Eccezioni da gestire per Sql Server, che non generano errori bloccanti:
					// - 8152:	modifica della dimensione di una colonna di un valore inferiore alla dimensione originale, 
					//			che causerebbe il troncamento della stringa
					// - 4928:	tentativo di alterare una colonna in ntext, che è gia' di questo tipo
					if (connection.IsSqlConnection())
						if (e.Number == 8152 || (e.Number == 4928 && e.Message.IndexOf("ntext") > 0))
						{
							Diagnostic.Set(DiagnosticType.LogOnFile, string.Format(DatabaseManagerStrings.HandledTBException, e.Message));
							continue;
						}

					Debug.Fail(e.Message);

					// se la lunghezza del message è superiore ai 400 chr la tronco 
					// (altrimenti ho un errore in fase di visualizzazione nel Diagnostico)
					error = (e.Message.Length > 400) ? e.Message.Substring(0, 400) : e.Message;

					ExtendedInfo extendedInfo = new ExtendedInfo();
					extendedInfo.Add(DatabaseManagerStrings.Description, error);
					extendedInfo.Add(DatabaseManagerStrings.ErrorCode, e.Number);
					extendedInfo.Add(DatabaseManagerStrings.Function, "ExecuteFileSql");
					extendedInfo.Add(DatabaseManagerStrings.Library, "Microarea.TaskBuilderNet.Data.DatabaseLayer");
					extendedInfo.Add(DatabaseManagerStrings.StackTrace, statement);
					error += string.Concat(" [ ", DatabaseManagerStrings.ErrorCode, ": ", e.Number, "]");
					error += "\r\n";
					Diagnostic.Set(DiagnosticType.Error, e.Message, extendedInfo);
					Diagnostic.Set(DiagnosticType.LogOnFile, error);

					Debug.WriteLine(e.Message);
					Debug.WriteLine(fi != null ? "Path file: " + fi.FullName : string.Empty);
					Debug.WriteLine("Script text: " + statement);

					if (altOnError)
						return false;
					else
					{
						errorFound = true;
						continue;
					}
				}
				catch (Exception e)
				{
					Debug.Fail(e.Message);
					ExtendedInfo extendedInfo = new ExtendedInfo();
					extendedInfo.Add(DatabaseManagerStrings.Description, e.Message);
					extendedInfo.Add(DatabaseManagerStrings.Function, "ExecuteFileSql");
					extendedInfo.Add(DatabaseManagerStrings.Library, "Microarea.TaskBuilderNet.Data.DatabaseLayer");
					extendedInfo.Add(DatabaseManagerStrings.StackTrace, e.StackTrace);
					error += e.Message;
					error += "\r\n";
					Diagnostic.Set(DiagnosticType.Error, e.Message, extendedInfo);
					Diagnostic.Set(DiagnosticType.LogOnFile, error);

					if (altOnError)
						return false;
					else
					{
						errorFound = true;
						continue;
					}
				}
			}

			command.Dispose();

			return !errorFound;
		}
		#endregion

		#region ApplyMandatoryColumnRegex (per inserire le colonne obbligatorie dinamicamente)
		/// <summary>
		/// ApplyMandatoryColumnRegex
		/// Solo negli script che hanno una CREATE TABLE vado ad inserire prima del CONSTRAINT di PRIMARY KEY 
		/// le colonne obbligatorie TBCreated, TBModified, TBCreatedID, TBModifiedID
		/// in modo da velocizzare la creazione del database senza dover effettuare le ALTER TABLE in successione
		/// </summary>
		/// <param name="script">testo da analizzare</param>
		/// <param name="statement">testo modificato</param>
		//---------------------------------------------------------------------------
		private void ApplyMandatoryColumnRegex(string script, out string statement)
		{
			statement = script;

			// se non si tratta di una creazione di tabella non procedo
			if (!Regex.IsMatch(statement, @"\bCREATE\b\s*\bTABLE\b", RegexOptions.IgnoreCase | RegexOptions.Compiled))
				return;

			try
			{
				// per individuare il nome della tabella (mi serve per concatenare il nome dei constraint di default)
				string createTablePattern = @"\bCREATE\s+TABLE\s*(((\[\s*dbo\s*\])|dbo)\s*\.)?\s*\[?\s*(?<Table>\w+)\s*\]?";
				
				// per individuare l'indice dell'ultima virgola (esclusa e solo se presente) prima del constraint di PK dove inserire le colonne obbligatorie
				string pkConstraintPattern = @"\,?\s*\bCONSTRAINT\b.*\bPRIMARY\s*KEY\b";

				int i = 0;

				MatchCollection tblMatches = Regex.Matches(statement, createTablePattern, RegexOptions.IgnoreCase | RegexOptions.Compiled);
				// faccio un loop perche' potrei avere due CREATE TABLE di seguito senza GO
				foreach (Match mtch in tblMatches)
				{
					string tableName = mtch.Groups["Table"].ToString();

					if (string.IsNullOrWhiteSpace(tableName))
						continue;

					MatchCollection pkMatches = Regex.Matches(statement, pkConstraintPattern);
					// se i match sono diversi mi fermo
					if (tblMatches.Count != pkMatches.Count)
						break;

					Match pkMtch = pkMatches[i];
					statement = statement.Insert(pkMtch.Index, string.Format(DatabaseLayerConsts.RegexAddMandatoryColums, tableName));

					i++;
				}
			}
			catch (Exception e)
			{
				ExtendedInfo extendedInfo = new ExtendedInfo();
				extendedInfo.Add(DatabaseManagerStrings.Description, e.Message);
				extendedInfo.Add(DatabaseManagerStrings.Function, "ApplyMandatoryColumnRegex");
				extendedInfo.Add(DatabaseManagerStrings.Library, "Microarea.TaskBuilderNet.Data.DatabaseLayer");
				extendedInfo.Add(DatabaseManagerStrings.StackTrace, e.StackTrace);
				Diagnostic.Set(DiagnosticType.Error, e.Message, extendedInfo);
				Diagnostic.Set(DiagnosticType.LogOnFile, e.Message);
			}
		}
		#endregion

		#region ApplyFullTextSearchRegex (per la gestione del FullTextSearch nel database del DMS)
		/// <summary>
		/// ApplyFullTextSearchRegex (si utilizza solo per il database DMS, che e' solo SQL)
		/// NON UTILIZZATA, la tengo per ricordo :-)
		/// Applica la RegularExpression per la creazione degli oggetti necessari alla gestione del Full Text Search
		/// </summary>
		//--------------------------------------------------------------------------------
		private void ApplyFullTextSearchRegex(string scriptPortion, out string statement)
		{
			// prima sostituisco il placeholder [CATALOGNAME] con il nome del database seguito dal suffisso _FTS
			// questo e' il placeholder per la creazione del fulltextcatalog
			string createfullTextCatalog = Regex.Replace
				(
				scriptPortion,
				@"\[CATALOGNAME\]",
				string.Concat("[", this.Connection.Database, DatabaseLayerConsts.FullTextSearchSuffix, "]"),
				RegexOptions.IgnoreCase | RegexOptions.Compiled
				);

			// poi sostituisco il placeholder $CATALOGNAME$ con il nome del database seguito dal suffisso _FTS
			// questo e' il placeholder per il check nell'if exists e va SENZA PARENTESI QUADRE!
			string existsFullTextCatalog = Regex.Replace
				(
				createfullTextCatalog,
				@"\$CATALOGNAME\$",
				string.Concat(this.Connection.Database, DatabaseLayerConsts.FullTextSearchSuffix),
				RegexOptions.IgnoreCase | RegexOptions.Compiled
				);

			// dopo sostituisco il placeholder [LNGSID] con l'LCID estrapolato dalla cultura associata al database
			// ma solo se e' presente tra le languages del full text search
			statement = Regex.Replace
				(
				existsFullTextCatalog,
				@"\[LNGSID\]",
				isSupportedLanguageForFullTextSearch ? string.Concat("LANGUAGE ", this.databaseCulture.ToString()) : string.Empty,
				RegexOptions.IgnoreCase | RegexOptions.Compiled
				);
		}
		#endregion

		#region ApplyUnicodeRegex (per il supporto dei caratteri Unicode)
		/// <summary>
		/// ApplyUnicodeRegex (si utilizza solo per i database SQL Server e Oracle)
		/// Applica la RegularExpression per la sostituzione dei caratteri Unicode
		/// </summary>
		/// <param name="scriptText">testo contenuto nel file sql</param>
		/// <param name="strConvert">testo dopo l'applicazione della Regex</param>
		//---------------------------------------------------------------------------
		private void ApplyUnicodeRegex(string scriptText, out string strConvert)
		{
			strConvert = string.Empty;

			// se ho specificato il supporto per i caratteri Unicode sostituisco (utilizzando le Regex):
			// 1. tutte le occorrenze di tipo varchar (o VARCHAR2 per Oracle) con nvarchar (o NVARCHAR2 per Oracle) 
			// 2. tutte le occorrenze di tipo text (o CLOB per Oracle) con ntext (o NCLOB per Oracle) 
			// 3. tutte le occorrenze di tipo char (o CHAR per Oracle) con nchar (o NCHAR per Oracle) 
			// 4. solo per Oracle: devo anche eliminare dallo script le eventuali occorrenze della sola parola byte
			if (connection.IsSqlConnection())
				strConvert = Regex.Replace(scriptText, @"\b(?<type>varchar|text|char)\b", "n${type}", RegexOptions.IgnoreCase | RegexOptions.Compiled);
		}
		#endregion

		#region ApplyCollationRegex (per la gestione delle COLLATE negli script)
		/// <summary>
		/// ApplyCollationRegex
		/// Applica la RegularExpression per la sostituzione della COLLATE
		/// </summary>
		/// <param name="scriptText">testo contenuto nel file sql</param>
		/// <param name="strConvert">testo dopo l'applicazione della Regex</param>
		//---------------------------------------------------------------------------
		private void ApplyCollationRegex(string scriptPortion, out string statement)
		{
			statement = string.Empty;

			// se la COLLATE propria del database e la DatabaseCulture specificata dall'utente sul database sono uguali 
			// sostituisco solo la parola chiave [DBCOLLATE] con COLLATE dbCollate
			if (!differentCollation)
			{
				// se la stringa databaseCollation è vuota sostituisco la parola chiave con una stringa vuota (per evitare l'errore di sintassi) 
				statement = Regex.Replace
					(
					scriptPortion,
					@"\[DBCOLLATE\]",
					(databaseCollation.Length > 0) ? string.Format("COLLATE {0}", databaseCollation) : string.Empty,
					RegexOptions.IgnoreCase | RegexOptions.Compiled
					);

				// TODO MICHI E BRUNA!!!!
				// QUANDO ESCE LA 2.5 BISOGNA RICORDARSI DI SKIPPARE IN QUESTO PRECISO PUNTO ANCHE L'APPLICAZIONE 
				// DELLA COLLATE Latin1_General_CI_AS CABLATA NEGLI SCRIPT PER PRESERVARE I DATABASE CREATI PRIMA 
				// DELLA 2.5 E UPGRADATI ALLA REL. 2.5.X O 2.6 E SUPERIORI
				// CHI PASSA DALLA 2.1.5 ALLA 2.6 DIRETTAMENTE NON DOVRA' VEDERE APPLICATA LA COLLATION SULLE COLONNE
				// SCRITTO NEGLI SCRIPT DI UPGRADE
				if (string.Compare(databaseCollation, "Latin1_General_CI_AS", StringComparison.OrdinalIgnoreCase) != 0)
					statement = Regex.Replace(statement, @"COLLATE Latin1_General_CI_AS", string.Empty, RegexOptions.IgnoreCase | RegexOptions.Compiled);
			}
			else
			{
				// se la COLLATE propria del database e la DatabaseCulture specificata dall'utente sul database sono diverse devo:
				// per ogni colonna di tipo varchar, nvarchar, char, nchar, text, ntext aggiungo l'istruzione:
				// COLLATE + DatabaseCulture specificata in fase di creazione dell'utente
				// skippando però quelle righe che hanno già specificato un'istruzione di COLLATE
				// e quelle che hanno la parola chiave [DBCOLLATE]
				statement = Regex.Replace
					(
					scriptPortion,
					@"(?<collate>\bn?(varchar|char)\b\s*\]?\s*\(\s*\d*\s*\)(?!\s*COLLATE)(?!\s*\[DBCOLLATE\]))|(?<collate>\bn?text\b\s*\]?\s*(?!\s*COLLATE)(?!\s*\[DBCOLLATE\]))",
					(columnCollation.Length > 0) ? string.Format("${{collate}} COLLATE {0} ", columnCollation) : "${collate} ",
					RegexOptions.IgnoreCase | RegexOptions.Compiled
					);

				// alla fine di tutto sostituisco solo la parola chiave [DBCOLLATE] con COLLATE databaseCollation
				// se la stringa databaseCollation è vuota sostituisco la parola chiave con una stringa vuota (per evitare l'errore di sintassi) 
				statement = Regex.Replace
					(
					statement,
					@"\[DBCOLLATE\]",
					(databaseCollation.Length > 0) ? string.Format("COLLATE {0}", databaseCollation) : string.Empty,
					RegexOptions.IgnoreCase | RegexOptions.Compiled
					);
			}
		}
		#endregion

		#region IsSpecialCommand (trova VIEW/SP/FUNCTION e altre sintassi particolari)
		/// <summary>
		/// IsSpecialCommand
		/// Cerca nella porzione di script se esistono i comandi CREATE/DROP VIEW, CREATE/DROP PROCEDURE, CREATE/DROP FUNCTION oppure DECLARE 
		/// oppure .value (case-sensitive, utilizzato negli script del DMS per leggere il contenuto delle colonne di tipo xml)
		/// </summary>
		/// <param name="scriptPortion">porzione di script da controllare</param>
		/// <returns>true: se ha trovato i comandi speciali che ho ricercato</returns>
		//---------------------------------------------------------------------------
		private bool IsSpecialCommand(string scriptPortion)
		{
			return
				Regex.IsMatch(scriptPortion, @"\bDROP\b\s*\bVIEW\b", RegexOptions.IgnoreCase | RegexOptions.Compiled) ||
				Regex.IsMatch(scriptPortion, @"\bCREATE\b\s*\bVIEW\b", RegexOptions.IgnoreCase | RegexOptions.Compiled) ||
				Regex.IsMatch(scriptPortion, @"\bDROP\b\s*\bPROCEDURE\b", RegexOptions.IgnoreCase | RegexOptions.Compiled) ||
				Regex.IsMatch(scriptPortion, @"\bCREATE\b\s*\bPROCEDURE\b", RegexOptions.IgnoreCase | RegexOptions.Compiled) ||
				Regex.IsMatch(scriptPortion, @"\bDECLARE\b", RegexOptions.IgnoreCase | RegexOptions.Compiled) ||
				Regex.IsMatch(scriptPortion, @"\bCREATE\b\s*\bFUNCTION\b", RegexOptions.IgnoreCase | RegexOptions.Compiled) ||
				Regex.IsMatch(scriptPortion, @"\bDROP\b\s*\bFUNCTION\b", RegexOptions.IgnoreCase | RegexOptions.Compiled) ||
				Regex.IsMatch(scriptPortion, @"\b.value\b", RegexOptions.Compiled);
		}
		#endregion

		///<summary>
		/// Metodo che effettua il caricamento in memoria di una dll e, se la classe deriva
		/// dall'interfaccia IUpgradeDatabaseTask, richiama il metodo entry-point Run
		///</summary>
		//---------------------------------------------------------------------------
		/*public bool ExecuteLibrary(string pathLibrary, out string error)
		{
			error = string.Empty;

			fi = new FileInfo(pathLibrary);

			// se non c'e' l'estensione dll non procedo
			if (string.Compare(Path.GetExtension(fi.Extension), ".dll", StringComparison.OrdinalIgnoreCase) != 0)
			{
				error = DatabaseManagerStrings.DllFileExpected;
				Diagnostic.Set(DiagnosticType.Error, DatabaseManagerStrings.DllFileExpected);
				return false;
			}

			// se il file non esiste non procedo
			if (!fi.Exists)
			{
				error = DatabaseManagerStrings.FileNotFound;
				Diagnostic.Set(DiagnosticType.Error, DatabaseManagerStrings.FileNotFound);
				return false;
			}

			try
			{
				// load the assembly
				Assembly assembly = AssembliesLoader.Load(pathLibrary);

				// Walk through each type in the assembly looking for a class IUpgradeDatabaseTask
				foreach (Type type in assembly.GetTypes())
				{
					if (typeof(IUpgradeDatabaseTask).IsAssignableFrom(type))
					{
						// create an instance of the object
						IUpgradeDatabaseTask classObj = (IUpgradeDatabaseTask)Activator.CreateInstance(type);

						// call Run entry-point method
						if (!classObj.Run(Connection))
						{
							// assign the Diagnostic with errors
							Diagnostic.Set(classObj.GetDiagnostic());
							return false;
						}

						// appena ne incontro una faccio break
						break;
					}
				}
			}
			catch (TBException e)
			{
				// se la lunghezza del message è superiore ai 400 chr la tronco 
				// (altrimenti ho un errore in fase di visualizzazione nel Diagnostico)
				error = (e.Message.Length > 400) ? e.Message.Substring(0, 400) : e.Message;

				ExtendedInfo extendedInfo = new ExtendedInfo();
				extendedInfo.Add(DatabaseManagerStrings.Description, error);
				extendedInfo.Add(DatabaseManagerStrings.ErrorCode, e.Number);
				extendedInfo.Add(DatabaseManagerStrings.Function, "ExecuteDll");
				extendedInfo.Add(DatabaseManagerStrings.Library, "Microarea.TaskBuilderNet.Data.DatabaseLayer");
				error += string.Concat(" [ ", DatabaseManagerStrings.ErrorCode, ": ", e.Number, "]");
				error += "\r\n";
				Diagnostic.Set(DiagnosticType.Error, e.Message, extendedInfo);

				Debug.WriteLine(e.Message);
				Debug.WriteLine("Path file: " + pathLibrary);

				return false;
			}
			catch (FileLoadException fle)
			{
				// se la lunghezza del message è superiore ai 400 chr la tronco 
				// (altrimenti ho un errore in fase di visualizzazione nel Diagnostico)
				error = (fle.Message.Length > 400) ? fle.Message.Substring(0, 400) : fle.Message;

				ExtendedInfo extendedInfo = new ExtendedInfo();
				extendedInfo.Add(DatabaseManagerStrings.Description, error);
				extendedInfo.Add(DatabaseManagerStrings.Function, "ExecuteDll");
				extendedInfo.Add(DatabaseManagerStrings.Library, "Microarea.TaskBuilderNet.Data.DatabaseLayer");
				extendedInfo.Add(DatabaseManagerStrings.StackTrace, fle.StackTrace);
				error += "\r\n";
				Diagnostic.Set(DiagnosticType.Error, fle.Message, extendedInfo);

				Debug.WriteLine(fle.Message);
				Debug.WriteLine("Path file: " + pathLibrary);
				return false;
			}
			catch (Exception ex)
			{
				// se la lunghezza del message è superiore ai 400 chr la tronco 
				// (altrimenti ho un errore in fase di visualizzazione nel Diagnostico)
				error = (ex.Message.Length > 400) ? ex.Message.Substring(0, 400) : ex.Message;

				ExtendedInfo extendedInfo = new ExtendedInfo();
				extendedInfo.Add(DatabaseManagerStrings.Description, error);
				extendedInfo.Add(DatabaseManagerStrings.Function, "ExecuteDll");
				extendedInfo.Add(DatabaseManagerStrings.Library, "Microarea.TaskBuilderNet.Data.DatabaseLayer");
				extendedInfo.Add(DatabaseManagerStrings.StackTrace, ex.StackTrace);
				error += "\r\n";
				Diagnostic.Set(DiagnosticType.Error, ex.Message, extendedInfo);

				Debug.WriteLine(ex.Message);
				Debug.WriteLine("Path file: " + pathLibrary);
				return false;
			}

			return true;
		}*/

	}
}
