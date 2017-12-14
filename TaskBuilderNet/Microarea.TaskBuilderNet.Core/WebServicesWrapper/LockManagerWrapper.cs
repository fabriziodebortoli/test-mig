using System;

namespace Microarea.TaskBuilderNet.Core.WebServicesWrapper
{
	/// <summary>
	/// Wrapper per l'utilizzo di lock manager
	/// </summary>
	//============================================================================
	public class LockManager
	{
		private lockMng.MicroareaLockManager lockManager = new lockMng.MicroareaLockManager();

		/// <summary>
		/// Costruttore
		/// </summary>
		/// <param name="lockManagerUrl">Indirizzo di lock manager</param>
		//---------------------------------------------------------------------------
		public LockManager(string lockManagerUrl)
		{
			lockManager.Url = lockManagerUrl;
		}

		/// <summary>
		/// Inizia a gestire i lock per una company
		/// </summary>
		/// <param name="companyDBName">database aziendali sul quale si vogliono effettuare prenotazioni di dati</param>
		/// <param name="authenticationToken">token di autenticazione dell'utente</param>token di autenticazione dell'utente</param>
		/// <returns>true se ha successo</returns>
		//-----------------------------------------------------------------------
		public bool InitLock(string companyDBName, string authenticationToken)
		{
			return lockManager.InitLock(companyDBName, authenticationToken);
		}

		/// <summary>
		/// Prenota un dato
		/// </summary>
		/// <param name="companyDBName">database aziendale</param>
		/// <param name="authenticationToken">token di autenticazione dell'utente</param>token di autenticazione dell'utente</param>
		/// <param name="userName">nome utente</param>
		/// <param name="tableName">tabella che si vuole prenotare</param>
		/// <param name="lockKey">chiave del dato composta</param>
		/// <param name="address">indirizzo di memoria dell'istanza del documento</param>
		/// <param name="processName">nome del processo che effettua il lock</param>
		/// <returns>true se ha successo</returns>
		//-----------------------------------------------------------------------
		public bool LockRecord(string companyDBName, string authenticationToken, string userName, string tableName, string lockKey, string address, string processName)
		{
			return lockManager.LockRecord
				(
				companyDBName,
				authenticationToken,
				userName,
				tableName,
				lockKey, 			
				address,
				processName
				);
		}

        /// <summary>
        /// Nuova funzione per prenotazione dato. Se il dato è già in stato di lock il messaggio di lock viene subito restituito
        /// </summary>
        /// <param name="companyDBName">database aziendale</param>
        /// <param name="authenticationToken">token di autenticazione dell'utente</param>
        /// <param name="userName">nome utente</param>
        /// <param name="tableName">tabella che si vuole prenotare</param>
        /// <param name="lockKey">chiave del dato composta</param>
        /// <param name="address">indirizzo di memoria dell'istanza del documento</param>
        /// <param name="processName">nome del processo che effettua il lock</param>
        /// <param name="lockUser">se il dato è in stato di lock restituisce l'utente che lo sta lockando</param>
        /// <param name="lockApp">se il dato è in stato di lock restituisce il nome dell'applicazione che lo sta lockando</param>
        /// <returns>true se ha successo</returns>
		//-----------------------------------------------------------------------
        public bool LockRecordEx(string companyDBName, string authenticationToken, string userName, string tableName, string lockKey, string address, string processName, out string lockUser, out string lockApp)
		{
            return lockManager.LockRecordEx
				(companyDBName,  
                authenticationToken,  
                userName,  
                tableName,  
                lockKey,  
                address,  
                processName, 
                out lockUser,
                out lockApp);
		}

        		


		/// <summary>
		/// Libera un dato precedentemente prenotato.
		/// </summary>
		/// <param name="companyDBName">database aziendale</param>
		/// <param name="authenticationToken">token di autenticazione dell'utente</param>token di autenticazione dell'utente</param>
		/// <param name="tableName">tabella che si vuole liberare</param>
		/// <param name="lockKey">chiave del dato composta</param>
		/// <param name="address">indirizzo di memoria dell'istanza del documento</param>
		/// <returns>true se ha successo</returns>
		//-----------------------------------------------------------------------
		public bool UnlockRecord(string companyDBName, string authenticationToken, string tableName, string lockKey, string address)
		{
			return lockManager.UnlockRecord(
				companyDBName, 
				authenticationToken, 
				tableName, 
				lockKey, 
				address);
		}
		
		/// <summary>
		/// Indica se un dato è stato prenotato
		/// </summary>
		/// <param name="companyDBName">database aziendale</param>
		/// <param name="tableName">tabella contenente il dato</param>
		/// <param name="lockKey">chiave del dato composta</param>
		/// <param name="address">indirizzo di memoria dell'istanza del documento</param>
		/// <returns>true se il dato è stato prenotato</returns>
		//-----------------------------------------------------------------------
		public bool IsRecordLocked(string companyDBName, string tableName, string lockKey)
		{
			return lockManager.IsRecordLocked(companyDBName, tableName, lockKey);
		}
		
		/// <summary>
		/// Restituisce dati sul lock
		/// </summary>
		/// <param name="companyDBName">database aziendale</param>
		/// <param name="lockKey">chiave del dato composta</param>
		/// <param name="tableName">tabella contenente il dato</param>
		/// <param name="lockerToken">token di autenticazione dell'utente</param>
		/// <param name="lockTime">istante di prenotazione del dato</param>
		/// <param name="processName">nome del processo che ha effettuato il lock</param>
		/// <returns>true se ha successo</returns>
		//-----------------------------------------------------------------------
		public bool GetLockInfo(string companyDBName, string lockKey, string tableName, out string user, out DateTime lockTime, out string processName)
		{
			return lockManager.GetLockInfo(companyDBName, lockKey, tableName, out user, out lockTime, out processName);
		}
		
		/// <summary>
		/// Libera dati prenotati
		/// </summary>
		/// <param name="companyDBName">Database aziendale</param>
		/// <param name="authenticationToken">token di autenticazione dell'utente</param></param>
		/// <param name="address">indirizzo di memoria dell'istanza del documento</param>
		/// <returns>true se ha successo</returns>
		//-----------------------------------------------------------------------
		public bool UnlockAllContext(string companyDBName, string authenticationToken, string address)
		{
			return lockManager.UnlockAllContext(companyDBName, authenticationToken, address);
		}
		
		/// <summary>
		/// Libera dati prenotati
		/// </summary>
		/// <param name="companyDBName">Database aziendale</param>
		/// <param name="authenticationToken">token di autenticazione dell'utente</param></param>
		/// <param name="tableName">tabella contenente il dato</param>
		/// <param name="address">indirizzo di memoria dell'istanza del documento</param>
		/// <returns>true se ha successo</returns>
		//-----------------------------------------------------------------------
		public bool UnlockAll(string companyDBName, string authenticationToken, string tableName, string address)
		{
			return lockManager.UnlockAll(companyDBName, authenticationToken, tableName, address);
		}
		
		/// <summary>
		/// Libera dati prenotati
		/// </summary>
		/// <param name="companyDBName">Database aziendale</param>
		/// <param name="authenticationToken">token di autenticazione dell'utente</param></param>
		/// <returns>true se ha successo</returns>
		//-----------------------------------------------------------------------
		public bool UnlockAllForCurrentConnection(string companyDBName, string authenticationToken)
		{
			return lockManager.UnlockAllForCurrentConnection(companyDBName, authenticationToken);
		}

		/// <summary>
		/// Libera dati prenotati
		/// </summary>
		/// <param name="userName">Nome dell'utente</param>
		/// <returns>true se ha successo</returns>
		//-----------------------------------------------------------------------
		public bool UnlockAllForUser(string userName, string authenticationToken)
		{
			return lockManager.UnlockAllForUser(userName, authenticationToken);
		}

		/// <summary>
		/// Libera dati prenotati
		/// </summary>
		/// <param name="companyDBName">Database aziendale</param>
		/// <returns>true se ha successo</returns>
		//-----------------------------------------------------------------------
		public bool UnlockAllForCompanyDBName(string companyDBName, string authenticationToken)
		{
			return lockManager.UnlockAllForCompanyDBName(companyDBName, authenticationToken);
		}

		/// <summary>
		/// Libera dati prenotati
		/// </summary>
		/// <param name="companyDBName">Database aziendale</param>
		/// <param name="userName">Nome dell'utente</param>
		/// <returns>true se ha successo</returns>
		//-----------------------------------------------------------------------
		public bool UnlockAllForCompanyDBNameAndUser(string companyDBName, string userName, string authenticationToken)
		{
			return lockManager.UnlockAllForCompanyDBNameAndUser(companyDBName, userName, authenticationToken);
		}

		/// <summary>
		/// Libera dati prenotati
		/// </summary>
		/// <param name="companyDBName">Database aziendale</param>
		/// <param name="tableName">tabella contenente il dato</param>
		/// <returns>true se ha successo</returns>
		//-----------------------------------------------------------------------
		public bool UnlockAllForCompanyDBNameAndTable(string companyDBName, string tableName, string authenticationToken)
		{
			return lockManager.UnlockAllForCompanyDBNameAndTable(companyDBName, tableName, authenticationToken);
		}

		/// <summary>
		/// Libera dati prenotati
		/// </summary>
		/// <param name="companyDBName">Database aziendale</param>
		/// <param name="tableName">tabella contenente il dato</param>
		/// <param name="user">Nome dell'utente</param>
		/// <returns>true se ha successo</returns>
		//-----------------------------------------------------------------------
		public bool UnlockAllForCompanyDBNameAndTableAndUser(string companyDBName, string tableName, string userName, string authenticationToken)
		{
			return lockManager.UnlockAllForCompanyDBNameAndTableAndUser(companyDBName, tableName, userName, authenticationToken);
		}

		/// <summary>
		/// Libera dati prenotati
		/// </summary>
		/// <param name="companyDBName">Database aziendale</param>
		/// <param name="tableName">tabella contenente il dato</param>
		/// <param name="lockKey">Chiave di lock composta</param>
		/// <returns>true se ha successo</returns>
		//-----------------------------------------------------------------------
		public bool UnlockCompanyDBNameAndTableAndLock(string companyDBName, string tableName, string lockKey, string authenticationToken)
		{
			return lockManager.UnlockCompanyDBNameAndTableAndLock(companyDBName, tableName, lockKey, authenticationToken);
		}

		//-----------------------------------------------------------------------
		public void RemoveUnusedLocks()
		{
			lockManager.RemoveUnusedLocks();
		}

		/// <summary>
		/// Verifica se il server è disponibile
		/// </summary>
		/// <returns></returns>
		//-----------------------------------------------------------------------
		public bool IsAlive()
		{
			return lockManager.IsAlive();
		}

		/// <summary>
		/// Reinizializza lock manager
		/// </summary>
		//-----------------------------------------------------------------------
		public void Init(string authenticationToken)
		{
			lockManager.Init(authenticationToken);
		}

		/// <summary>
		/// Restituisce l'elenco dei lock attivi (aziende e tabelle con lock)
		/// </summary>
		//-----------------------------------------------------------------------
		public void GetCompanyDBAndTableLocksList()
		{
			lockManager.GetCompanyDBAndTableLocksList();
		}

		/// <summary>
		/// Restituisce l'elenco dei lock di un'azienda e di una tabella
		/// </summary>
		/// <param name="companyDBName"></param>
		/// <param name="tableName"></param>
		//-----------------------------------------------------------------------
		public void GetLocksList(string companyDBName, string tableName)
		{
			lockManager.GetLocksList(companyDBName, tableName);
		}

		/// <summary>
		/// Restituisce l'elenco dei lock attivi
		/// </summary>
		/// <returns>elenco lock attivi</returns>
		//-----------------------------------------------------------------------
		public string GetLockEntriesAtt()
		{
			try
			{
				return lockManager.GetLockEntriesAtt();
			}
			catch
			{
				return string.Empty;
			}
		}
	}
}
