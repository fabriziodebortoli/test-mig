using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Data.SqlClient;
using System.Data;
using System.Collections;

namespace Microarea.LockManager
{
	
	public class LockManager
    {
		private string companyConnectionString = string.Empty;
		private string accountName = string.Empty;
		private string processName = string.Empty;
		private bool keepConnectionOpen = false;  //performance

		private SqlConnection sqlConnection = null;
		private string errorMessage = string.Empty;
		private bool enableLockCache = false;
		//gestione cache
		private CacheLocksEntries cacheLocksEntries = new CacheLocksEntries();

		public bool KeepConnectionOpen { set { keepConnectionOpen = value; } }

		public bool EnableLockCache { set { enableLockCache = value; } }

		public string ErrorMessage { get { string errMsg = errorMessage; errorMessage = string.Empty; return errMsg; } }

		public LockManager()
		{
		}

		//----------------------------------------------------------------------------------
		public void Init(string companyConnectionString, string userName, string processName)
		{
			this.companyConnectionString = companyConnectionString;
			this.accountName = userName;
			this.processName = processName;

			sqlConnection = new SqlConnection(companyConnectionString);
		}

		//----------------------------------------------------------------------------------
		public void Destroy()
		{
			//UnlockAll();
			if (sqlConnection.State != ConnectionState.Closed)
				sqlConnection.Close();
			sqlConnection.Dispose();
		}

		//----------------------------------------------------------------------------------
		private void OpenCompanyConnection()
		{
			if (string.IsNullOrEmpty(companyConnectionString))
				return ;  //throw SqlException("Connection string empty"));
				
			try
			{
				if (sqlConnection.State == ConnectionState.Open)
					return;				

				sqlConnection.Open();
			}
			catch (SqlException e)
			{				
				throw e;
			}
		}
		//----------------------------------------------------------------------------------
		private void CloseCompanyConnection()
		{
			try
			{
				if (keepConnectionOpen || sqlConnection.State == ConnectionState.Closed)
					return;					

				sqlConnection.Close();
			}
			catch (SqlException e)
			{

				throw e;
			}
		}
		/// <summary>
		/// Prenota un dato
		/// </summary>
		/// <param name="tableName">nome tabella</param>
		/// <param name="lockKey">chiave primaria del dato da prenotare</param>
		/// <param name="context">indirizzo in memoria del documento</param>
		/// <param name="lockUser">in caso di record già in stato di lock restituisce l'account che impegna il dato</param>
		/// <param name="lockApp">in caso di record già in stato di lock restituisce l'applicazione che impegna il dato</param>
		/// <param name="errorMessage">eventuale messaggio di errore</param>
		/// <returns>true se il dato è stato prenotato con successo, false altrimenti</returns>
		//----------------------------------------------------------------------------------
		public bool LockCurrent(string tableName, string lockKey, string context, ref string lockUser, ref string lockApp, ref DateTime lockDate)
		{
			//per prima cosa verifico che non sia un lock già presente nella cache 
			LockEntry lockEntry = cacheLocksEntries.GetLockEntry(tableName, lockKey);
			if (lockEntry != null)
				return lockEntry.IsSameLock(context);

			bool result = true;			 
			OpenCompanyConnection();

			//performance: costruisco secca la commandstring senza passare dai parametri e senza la string.Format
			string fieldsValue = "'" + tableName + "', '" + lockKey + "', '" + accountName + "', '" + context + "', '" + processName + "'";
			string insertText = "INSERT INTO TB_Locks (TableName, LockKey, AccountName, Context, ProcessName) VALUE" + "(" + fieldsValue + ")";
			SqlCommand sqlCommand = new SqlCommand(insertText, sqlConnection);
			try
			{
				sqlCommand.ExecuteNonQuery();
				sqlCommand.Dispose();
				//ho effettuato il lock, ne faccio il cache
				cacheLocksEntries.AddLockEntry(tableName, lockKey, context, DateTime.Now);

			}
			catch (SqlException e)
			{
				result = false;
				//il record è già locked
				if (e.Number == 2601)
				{
					lockEntry = ExtractLockEntry(tableName, lockKey, ref errorMessage);
					lockUser = lockEntry.LockUser;
					lockApp = lockEntry.LockApp;
					lockDate = lockEntry.LockDate;
				}
				else
					errorMessage = e.Message;
			}
			finally
			{
				if (sqlCommand != null)
					sqlCommand.Dispose();

				CloseCompanyConnection();
			}
			return result;
		}


		//-----------------------------------------------------------------------
		private LockEntry ExtractLockEntry(string tableName, string lockKey, ref string error)
		{
			string selectText = "SELECT * FROM TB_Locks WHERE TableName = '" + tableName + "' AND LockKey = '" + lockKey + "'";
			SqlCommand selectCommand = new SqlCommand(selectText, sqlConnection);
			SqlDataReader dataReader = null;
			LockEntry lockEntry = null;
			try
			{
				dataReader = selectCommand.ExecuteReader();
				if (dataReader.Read())
				{
					lockEntry = new LockEntry(lockKey, (string)dataReader["Context"], (DateTime)dataReader["LockDate"]);
					lockEntry.LockUser = (string)dataReader["AccountName"];
					lockEntry.LockApp = (string)dataReader["ProcessName"];
				}
				dataReader.Close();
				selectCommand.Dispose();				
			}
			catch (SqlException e)
			{
				lockEntry = null;
				error = e.Message;
			}
			finally
			{
				if (dataReader != null && !dataReader.IsClosed)
					dataReader.Close();
				if (selectCommand != null)
					selectCommand.Dispose();
			}
			return lockEntry;
		}

		/// <summary>
		/// Verifica se un dato è stato prenotato da un altro contesto/utente
		/// </summary>
		/// <param name="companyDBName">Nome del database aziendale</param>
		/// <param name="tableName">Nome della tabella</param>
		/// <param name="lockKey">Chiave primaria del lock</param>
		/// <param name="context">Indirizzo in memoria del contesto che richiede se il dato è in stato di locked</param>
		/// <returns>true il dato è stato prenotato da un altro contesto false se non è stato prenotato oppure è prenotato dallo stesso contesto</returns>
		//-----------------------------------------------------------------------
		public bool IsCurrentLocked(string tableName, string lockKey, string context)
		{
			//per prima cosa verifico che non sia un lock già presente nella cache con lo stesso indirizzo o meno
			LockEntry lockEntry = cacheLocksEntries.GetLockEntry(tableName, lockKey);
			if (lockEntry != null)
				return !lockEntry.IsSameLock(context); //vuol dire che è locked da un altro contesto

			bool result = false;
			OpenCompanyConnection();
			lockEntry = ExtractLockEntry(tableName, lockKey, ref errorMessage);
			result = lockEntry != null && lockEntry.Context != context;
			CloseCompanyConnection();
			
			return result;
		}

		/// <summary>
		/// Verifica se il record passato come chiave è stato lockato dal contesto stesso individuato da context
		/// E' l'opposto dell'IsCurrentLocked
		/// </summary>
		/// <param name="tableName">Nome della tabella</param>
		/// <param name="lockKey">Chiave primaria del lock</param>
		/// <param name="context">Indirizzo in memoria del contesto che richiede se il dato è in stato di locked</param>
		/// <returns>true il dato è stato prenotato dallo stesso contesto false se non è stato prenotato oppure è prenotato da altro contesto</returns>
		//----------------------------------------------------------------------------------
		public bool IsMyLock(string tableName, string lockKey, string context)
		{
			//Se il mio è per forza nella cache
			return cacheLocksEntries.ExistLockEntry(tableName, lockKey, context);
		}

		/// <summary>
		/// Prende informazioni su un lock
		/// </summary>
		/// <param name="lockKey">Chiave primaria del lock</param>
		/// <param name="tableName">Nome della tabella</param>
		/// <param name="lockUser">in caso di record già in stato di lock restituisce l'account che impegna il dato</param>
		/// <param name="lockTime">Istante di prenotazione del dato</param>
		/// <param name="processName">Nome del processo che ha prenotato il dato</param>
		/// <returns>true se la funzione ha avuto successo</returns>
		//-----------------------------------------------------------------------
		public bool GetLockInfo(string lockKey, string tableName, ref string lockUser, ref string lockApp, ref DateTime lockDate)
		{
			//prima guardo nella cache
			LockEntry lockEntry = cacheLocksEntries.GetLockEntry(tableName, lockKey);
			if (lockEntry != null)
			{
				lockUser = accountName;
				lockApp = processName;
				lockDate = lockEntry.LockDate;
				return true;
			}

			//non è un dato in cache, leggo dalla tabella TB_Locks
			bool result = false;
			OpenCompanyConnection();
			lockEntry = ExtractLockEntry(tableName, lockKey, ref errorMessage);
			if (lockEntry != null)
			{
				lockUser = lockEntry.LockUser;
				lockApp = lockEntry.LockApp;
				lockDate = lockEntry.LockDate;
			}

			CloseCompanyConnection();

			return result;
		}



		/// <summary>
		/// Rimuove la prenotazione di un record
		/// </summary>
		/// <param name="tableName">Nome della tabella</param>
		/// <param name="lockKey">Chiave primaria del lock</param>
		/// <param name="context">Indirizzo in memoria del documento</param>
		/// <returns>true se la funzione ha avuto successo</returns>
		//----------------------------------------------------------------------------------		
		public bool UnlockCurrent(string tableName, string lockKey, string context)
		{
			bool result = true;

			OpenCompanyConnection();
			//per prima tolgo la riga dalla tabella
			//performance: costruisco secca la commandstring senza passare dai parametri e senza la string.Format
			string deleteText = @"DELETE FROM TB_Locks WHERE TableName = '" + tableName + "' AND LockKey = '" + lockKey +"'";
			SqlCommand sqlCommand = new SqlCommand(deleteText, sqlConnection);
			try
			{
				sqlCommand.ExecuteNonQuery();
				sqlCommand.Dispose();
				//elimino l'entry dalla cache
				cacheLocksEntries.RemoveLockEntry(tableName, lockKey);

			}
			catch (SqlException e)
			{
				result = false;
				errorMessage = e.Message;
			}
			finally
			{
				CloseCompanyConnection();
			}
			return result;
		}


		/// <summary>
		/// Rimuove tutti i lock su una tabella per un determinato contesto
		/// </summary>
		//----------------------------------------------------------------------------------		
		public bool UnlockAllTableContext(string tableName, string context)
		{
			bool result = true;
			//prima cancello la cache
			cacheLocksEntries.RemoveEntriesForContext(tableName, context);

			OpenCompanyConnection();
			//per prima tolgo la riga dalla tabella
			//performance: costruisco secca la commandstring senza passare dai parametri e senza la string.Format
			string deleteText = @"DELETE FROM TB_Locks WHERE TableName = '" + tableName + "AND Context = '" + context + "' AND AccountName = '" + accountName + "'";
			SqlCommand sqlCommand = new SqlCommand(deleteText, sqlConnection);
			try
			{
				sqlCommand.ExecuteNonQuery();
				sqlCommand.Dispose();

			}
			catch (SqlException e)
			{
				result = false;
				errorMessage = e.Message;
			}
			finally
			{
				CloseCompanyConnection();
			}
			return result;
		}

		/// <summary>
		/// Rimuove tutti i lock di un determinato contesto
		/// </summary>
		//----------------------------------------------------------------------------------		
		public bool UnlockAllContext(string context)
		{
			bool result = true;
			//prima cancello la cache
			cacheLocksEntries.RemoveEntriesForContext(context);

			OpenCompanyConnection();
			//per prima tolgo la riga dalla tabella
			//performance: costruisco secca la commandstring senza passare dai parametri e senza la string.Format
			string deleteText = @"DELETE FROM TB_Locks WHERE Context = '" + context + "' AND AccountName = '" + accountName + "'";
			SqlCommand sqlCommand = new SqlCommand(deleteText, sqlConnection);
			try
			{
				sqlCommand.ExecuteNonQuery();
				sqlCommand.Dispose();

			}
			catch (SqlException e)
			{
				result = false;
				errorMessage = e.Message;
			}
			finally
			{
				CloseCompanyConnection();
			}
			return result;
		}

		/// <summary>
		/// Rimuove tutti i lock per l'account corrente
		/// </summary>
		//----------------------------------------------------------------------------------		
		public bool UnlockAll()
		{
			bool result = true;
			//prima cancello la cache
			cacheLocksEntries.Clear();

			OpenCompanyConnection();
			//per prima tolgo la riga dalla tabella
			//performance: costruisco secca la commandstring senza passare dai parametri e senza la string.Format
			string deleteText = @"DELETE FROM TB_Locks WHERE AccountName = '" + accountName + "'";
			SqlCommand sqlCommand = new SqlCommand(deleteText, sqlConnection);
			try
			{
				sqlCommand.ExecuteNonQuery();
				sqlCommand.Dispose();				

			}
			catch (SqlException e)
			{
				result = false;
				errorMessage = e.Message;
			}
			finally
			{
				CloseCompanyConnection();
			}
			return result;

		}

	}
}
