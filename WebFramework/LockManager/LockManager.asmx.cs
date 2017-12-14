using System;
using System.Diagnostics;
using System.Threading;
using System.Web.Services;


//using Microarea.TaskBuilderNet.Core.NameSolver;

namespace Microarea.WebServices.LockManager
{
	[WebService(Namespace="http://microarea.it/LockManager/")]
	public class MicroareaLockManager : System.Web.Services.WebService
	{
		static long hitCount = 0;
		static long requests = 0;
		static TimeSpan elapsed = new TimeSpan(0, 0, 0);
		static Timer logTimer = new Timer(new TimerCallback(Tick), null, TimeSpan.FromHours(0), TimeSpan.FromHours(2));
		static EventLog eventLog = new EventLog("MA Server", ".", "LockManager");
		static readonly object staticLockTicket = new object();

		#region inizializzazione
		//-----------------------------------------------------------------------
		public MicroareaLockManager()
		{
		}

		//-----------------------------------------------------------------------
		static void Tick(object state)
		{
			try
			{
				lock (staticLockTicket)
				{
					eventLog.WriteEntry
						(
						string.Format(
							"Lock manager busy resources found for {0} times; total requests: {1}; total wait time: {2} milliseconds", 
							hitCount,
							requests,
							elapsed.TotalMilliseconds
							),
						EventLogEntryType.Information
						);
					hitCount = 0;
					requests = 0;
					elapsed = new TimeSpan(0, 0, 0);
				}
			}
			catch
			{
			}
		}

		//-----------------------------------------------------------------------
		static void WriteError(Exception ex)
		{
			try
			{
				lock (staticLockTicket)
				{
					eventLog.WriteEntry
					(
					ex.ToString(),
					EventLogEntryType.Error
					);
				}
			}
			catch
			{
			}
		}

		//-----------------------------------------------------------------------
		private void LockResources()
		{
			if (!Monitor.TryEnter(logTimer))
			{
				DateTime start = DateTime.Now;
				Monitor.Enter(logTimer);
				hitCount++;
				elapsed.Add(DateTime.Now - start);
			}
			requests++;
		}
		//-----------------------------------------------------------------------
		private void UnlockResources()
		{
			Monitor.Exit(logTimer);
		}
		
		/// <summary>
		/// Reinizializza il lock manager
		/// </summary>
		[WebMethod]
			//-----------------------------------------------------------------------
		public void Init(string authenticationToken)
		{
			if (authenticationToken != "3B5B4C2F-E563-4b52-B187-A3071B65688E")
				return;
			
			try
			{
				LockResources();			
				LockApplication.Locks.UnlockAll();
                LockApplication.Locks.Initialize();
			}
			catch (Exception ex)
			{
				WriteError(ex);
				throw ex;
			}
			finally
			{
				UnlockResources();
			}
		}

		/// <summary>
		/// Inizia a gestire i lock per una company
		/// </summary>
		/// <param name="companyDBName">database aziendali sul quale si vogliono effettuare prenotazioni di dati</param>
		/// <param name="authenticationToken">token di autenticazione dell'utente</param>token di autenticazione dell'utente</param>
		/// <returns>true se ha successo</returns>
		[WebMethod]
			//-----------------------------------------------------------------------
		public bool InitLock(string companyDBName, string authenticationToken)
		{
			try
			{
				LockResources();			
				
				if (!LockApplication.Locks.IsValidToken(authenticationToken))
					return false;

				LockApplication.Locks.AddCompany(companyDBName);

				//rimuovo tutti i lock di quell'utente perchè se tb è crashato
				//non li ha puliti.
				LockApplication.Locks.UnlockAllForUser(authenticationToken, companyDBName);

				return true;
			}
			catch (Exception ex)
			{
				WriteError(ex);
				throw ex;
			}
			finally
			{
				UnlockResources();
			}
		}
		
		/// <summary>
		/// Serve a ritornare il Session ID preso in partenza
		/// </summary>
		/// <returns>Guid di partenza</returns>
		[WebMethod]
			//-----------------------------------------------------------------------
        public string GetLockSessionID()
		{
            string guid;
            try
            {
                LockResources();
                guid = LockApplication.Locks.LockSessionID.ToString();
            }
            catch (Exception ex)
            {
                WriteError(ex);
                throw ex;
            }
            finally
            {
                UnlockResources();
            }
			return guid;
		}

        /// <summary>
        /// Serve a verificare l'attività di lock manager
        /// </summary>
        /// <returns>Sempre true</returns>
        [WebMethod]
        //-----------------------------------------------------------------------
        public bool IsAlive()
        {
            return true;
        }
        
        #endregion

		#region info su lock
		
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
		[WebMethod]
			//-----------------------------------------------------------------------
		public bool GetLockInfo(string companyDBName, string lockKey, string tableName, out string user, out DateTime lockTime, out string processName)
		{
			try
			{
				LockResources();			
				return LockApplication.Locks.GetLockInfo(companyDBName, lockKey, tableName, out user, out lockTime, out processName);
			}
			catch (Exception ex)
			{
				WriteError(ex);
				throw ex;
			}
			finally
			{
				UnlockResources();
			}
		}
		
		/// <summary>
		/// Restituisce l'elenco dei lock attivi
		/// </summary>
		[WebMethod]
			//-----------------------------------------------------------------------
		public string GetLockEntriesAtt()
		{
			try
			{
				LockResources();			
				return LockApplication.Locks.GetLockEntries();
			}
			catch (Exception ex)
			{
				WriteError(ex);
				throw ex;
			}
			finally
			{
				UnlockResources();
			}
		}

		/// <summary>
		/// Restituisce l'elenco dei lock attivi (aziende e tabelle con lock)
		/// </summary>
		[WebMethod]
			//-----------------------------------------------------------------------
		public void GetCompanyDBAndTableLocksList()
		{
			try
			{
				LockResources();			
				LockApplication.Locks.GetCompanyDBAndTableLocksList();
				
			}
			catch (Exception ex)
			{
				WriteError(ex);
				throw ex;
			}
			finally
			{
				UnlockResources();
			}
		}

		/// <summary>
		/// Restituisce l'elenco dei lock di un'azienda e di una tabella
		/// </summary>
		/// <param name="companyDBName"></param>
		/// <param name="tableName"></param>
		[WebMethod]
			//-----------------------------------------------------------------------
		public void GetLocksList(string companyDBName, string tableName)
		{
			try
			{
				LockResources();			
				LockApplication.Locks.GetLocksList(companyDBName, tableName);
				
			}
			catch (Exception ex)
			{
				WriteError(ex);
				throw ex;
			}
			finally
			{
				UnlockResources();
			}
		}

		#endregion

		#region lock dati
		
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
		[WebMethod]
			//-----------------------------------------------------------------------
		public bool LockRecord(string companyDBName, string authenticationToken, string userName, string tableName, string lockKey, string address, string processName)
		{
			try
			{
				LockResources();			
				if (!LockApplication.Locks.IsValidToken(authenticationToken))
					return false;

				string lockUser = string.Empty;
				string lockApp = string.Empty;
				
				return LockApplication.Locks.AddLock(
					companyDBName,
					authenticationToken,
					userName,
					tableName,
					lockKey, 
					address,
					processName,
					out lockUser,
					out lockApp);
			}
			catch (Exception ex)
			{
				WriteError(ex);
				throw ex;
			}
			finally
			{
				UnlockResources();
			}
			
		}
		/// <summary>
		/// Nuova funzione per prenotazione dato. Se il dato è già in stato di lock il messaggio di lock viene subito restituito
		/// </summary>
		/// <param name="companyDBName">database aziendale</param>
		/// <param name="authenticationToken">token di autenticazione dell'utente</param>
		/// <param name="tableName">tabella che si vuole prenotare</param>
		/// <param name="lockKey">chiave del dato composta</param>
		/// <param name="address">indirizzo di memoria dell'istanza del documento</param>
		/// <param name="processName">nome del processo che effettua il lock</param>
		/// <param name="lockUser">se il dato è in stato di lock restituisce l'utente che lo sta lockando</param>
		/// <param name="lockApp">se il dato è in stato di lock restituisce il nome dell'applicazione che lo sta lockando</param>
		/// <returns>true se ha successo</returns>
		[WebMethod]
	    //-----------------------------------------------------------------------
		public bool LockRecordEx(string companyDBName, string authenticationToken, string userName, string tableName, string lockKey, string address, string processName, out string lockUser, out string lockApp)
		{
			try
			{
				LockResources();			
				lockUser = string.Empty;
				lockApp = string.Empty;

				if (!LockApplication.Locks.IsValidToken(authenticationToken))
					return false;

				return LockApplication.Locks.AddLock(
					companyDBName,
					authenticationToken,
					userName,
					tableName,
					lockKey, 
					address,
					processName,
					out lockUser,
					out lockApp);
			}
			catch (Exception ex)
			{
				WriteError(ex);
				throw ex;
			}
			finally
			{
				UnlockResources();
			}
			
		}
		
		/// <summary>
		/// Indica se un dato è stato prenotato
		/// </summary>
		/// <param name="companyDBName">database aziendale</param>
		/// <param name="tableName">tabella contenente il dato</param>
		/// <param name="lockKey">chiave del dato composta</param>
		/// <param name="address">indirizzo di memoria dell'istanza del documento</param>
		/// <returns>true se il dato è stato prenotato</returns>
		[WebMethod]
			//-----------------------------------------------------------------------
		public bool IsRecordLocked(string companyDBName, string tableName, string lockKey)
		{
			try
			{
				LockResources();			
				return LockApplication.Locks.IsRecordLocked(companyDBName, tableName, lockKey);
			}
			catch (Exception ex)
			{
				WriteError(ex);
				throw ex;
			}
			finally
			{
				UnlockResources();
			}
			
		}

		/// <summary>
		/// Verifica se un dato è stato prenotato da un altro contesto
		/// </summary>
		/// <param name="companyDBName">database aziendale</param>
		/// <param name="tableName">tabella contenente il dato</param>
		/// <param name="lockKey">chiave del dato composta</param>
		/// <param name="address">indirizzo di memoria dell'istanza del documento</param>
		/// <returns>true il dato è stato prenotato da un altro contesto false se non è stato prenotato oppure è prenotato dallo stesso contesto</returns>
		[WebMethod]
			//-----------------------------------------------------------------------
		public bool IsCurrentLocked(string companyDBName, string tableName, string lockKey, string address)
		{
			try
			{
				LockResources();			
				return LockApplication.Locks.IsCurrentLocked(companyDBName, tableName, lockKey, address);
			}
			catch (Exception ex)
			{
				WriteError(ex);
				throw ex;
			}
			finally
			{
				UnlockResources();
			}
		}

		/// <summary>
		/// Verifica se il record passato come chiave è stato prenotato dal contesto stesso individuato da address
		/// E' l'opposto dell'IsCurrentLocked
		/// </summary>
		/// <param name="companyDBName">database aziendale</param>
		/// <param name="tableName">tabella contenente il dato</param>
		/// <param name="lockKey">chiave del dato composta</param>
		/// <param name="address">indirizzo di memoria dell'istanza del documento</param>
		/// <returns>true il dato è stato prenotato dallo stesso contesto false se non è stato prenotato oppure è prenotato da altro contesto</returns>
		[WebMethod]
		//-----------------------------------------------------------------------
		public bool IsMyLock(string companyDBName, string tableName, string lockKey, string address)
		{
			try
			{
				LockResources();
				return LockApplication.Locks.IsMyLock(companyDBName, tableName, lockKey, address);
			}
			catch (Exception ex)
			{
				WriteError(ex);
				throw ex;
			}
			finally
			{
				UnlockResources();
			}
		}
		#endregion

		#region unlock dati
		/// <summary>
		/// Libera un dato precedentemente prenotato.
		/// </summary>
		/// <param name="companyDBName">database aziendale</param>
		/// <param name="authenticationToken">token di autenticazione dell'utente</param>token di autenticazione dell'utente</param>
		/// <param name="tableName">tabella che si vuole liberare</param>
		/// <param name="lockKey">chiave del dato composta</param>
		/// <param name="address">indirizzo di memoria dell'istanza del documento</param>
		/// <returns>true se ha successo</returns>
		[WebMethod]
			//-----------------------------------------------------------------------
		public bool UnlockRecord(string companyDBName, string authenticationToken, string tableName, string lockKey, string address)
		{
			try
			{
				LockResources();			
				if (!LockApplication.Locks.IsValidToken(authenticationToken))
					return false;

				return LockApplication.Locks.UnlockRecord(
					companyDBName, 
					authenticationToken, 				
					tableName, 
					lockKey, 				
					address);
			}
			catch (Exception ex)
			{
				WriteError(ex);
				throw ex;
			}
			finally
			{
				UnlockResources();
			}
		}
		
		/// <summary>
		/// Libera dati prenotati
		/// </summary>
		/// <param name="companyDBName">Database aziendale</param>
		/// <param name="authenticationToken">token di autenticazione dell'utente</param></param>
		/// <param name="address">indirizzo di memoria dell'istanza del documento</param>
		/// <returns>true se ha successo</returns>
		[WebMethod]
			//-----------------------------------------------------------------------
		public bool UnlockAllContext(string companyDBName, string authenticationToken, string address)
		{
			try
			{
				LockResources();			
				if (!LockApplication.Locks.IsValidToken(authenticationToken))
					return false;

				return LockApplication.Locks.UnlockAllContext(companyDBName, authenticationToken, address);
			}
			catch (Exception ex)
			{
				WriteError(ex);
				throw ex;
			}
			finally
			{
				UnlockResources();
			}
			
		}
		
		/// <summary>
		/// Libera dati prenotati
		/// </summary>
		/// <param name="companyDBName">Database aziendale</param>
		/// <param name="authenticationToken">token di autenticazione dell'utente</param></param>
		/// <param name="tableName">tabella contenente il dato</param>
		/// <param name="address">indirizzo di memoria dell'istanza del documento</param>
		/// <returns>true se ha successo</returns>
		[WebMethod]
			//-----------------------------------------------------------------------
		public bool UnlockAll(string companyDBName, string authenticationToken, string tableName, string address)
		{
			try
			{
				LockResources();			
				if (!LockApplication.Locks.IsValidToken(authenticationToken))
					return false;

				return LockApplication.Locks.UnlockAll(companyDBName, authenticationToken, tableName, address);
			}
			catch (Exception ex)
			{
				WriteError(ex);
				throw ex;
			}
			finally
			{
				UnlockResources();
			}
			
		}
		
		/// <summary>
		/// Libera dati prenotati
		/// </summary>
		/// <param name="companyDBName">Database aziendale</param>
		/// <param name="authenticationToken">token di autenticazione dell'utente</param></param>
		/// <returns>true se ha successo</returns>
		[WebMethod]
			//-----------------------------------------------------------------------
		public bool UnlockAllForCurrentConnection(string companyDBName, string authenticationToken)
		{
			try
			{
				LockResources();			
				if (!LockApplication.Locks.IsValidToken(authenticationToken))
					return false;

				return LockApplication.Locks.UnlockAllForCurrentConnection(companyDBName, authenticationToken);
			}
			catch (Exception ex)
			{
				WriteError(ex);
				throw ex;
			}
			finally
			{
				UnlockResources();
			}
			
		}

		/// <summary>
		/// Libera dati prenotati
		/// </summary>
		/// <param name="userName">Nome dell'utente</param>
		/// <returns>true se ha successo</returns>
		[WebMethod]
			//-----------------------------------------------------------------------
		public bool UnlockAllForUser(string userName, string authenticationToken)
		{
			try
			{
				LockResources();			
				if (LockApplication.Locks.IsValidTokenForConsole(authenticationToken))
					return LockApplication.Locks.UnlockAllForUser(userName);

				if (!LockApplication.Locks.IsValidToken(authenticationToken))
					return false;

				return LockApplication.Locks.UnlockAllForUser(userName);
			}
			catch (Exception ex)
			{
				WriteError(ex);
				throw ex;
			}
			finally
			{
				UnlockResources();
			}
			
		}
		
		/// <summary>
		/// Libera dati prenotati
		/// </summary>
		/// <param name="companyDBName">Database aziendale</param>
		/// <returns>true se ha successo</returns>
		[WebMethod]
			//-----------------------------------------------------------------------
		public bool UnlockAllForCompanyDBName(string companyDBName, string authenticationToken)
		{
			try
			{
				LockResources();			
				if (LockApplication.Locks.IsValidTokenForConsole(authenticationToken))
					return LockApplication.Locks.UnlockAllForCompanyDBName(companyDBName);

				if (!LockApplication.Locks.IsValidToken(authenticationToken))
					return false;

				return LockApplication.Locks.UnlockAllForCompanyDBName(companyDBName);
			}
			catch (Exception ex)
			{
				WriteError(ex);
				throw ex;
			}
			finally
			{
				UnlockResources();
			}
			
		}

		/// <summary>
		/// Libera dati prenotati
		/// </summary>
		/// <param name="companyDBName">Database aziendale</param>
		/// <param name="userName">Nome dell'utente</param>
		/// <returns>true se ha successo</returns>
		[WebMethod]
			//-----------------------------------------------------------------------
		public bool UnlockAllForCompanyDBNameAndUser(string companyDBName, string userName, string authenticationToken)
		{
			try
			{
				LockResources();			
				if (LockApplication.Locks.IsValidTokenForConsole(authenticationToken))
					return LockApplication.Locks.UnlockAllForCompanyDBNameAndUser(companyDBName, userName);

				if (!LockApplication.Locks.IsValidToken(authenticationToken))
					return false;

				return LockApplication.Locks.UnlockAllForCompanyDBNameAndUser(companyDBName, userName);
			}
			catch (Exception ex)
			{
				WriteError(ex);
				throw ex;
			}
			finally
			{
				UnlockResources();
			}
			
		}

		/// <summary>
		/// Libera dati prenotati
		/// </summary>
		/// <param name="companyDBName">Database aziendale</param>
		/// <param name="tableName">tabella contenente il dato</param>
		/// <returns>true se ha successo</returns>
		[WebMethod]
			//-----------------------------------------------------------------------
		public bool UnlockAllForCompanyDBNameAndTable(string companyDBName, string tableName, string authenticationToken)
		{
			try
			{
				LockResources();

				if (LockApplication.Locks.IsValidTokenForConsole(authenticationToken))
					return LockApplication.Locks.UnlockAllForCompanyDBNameAndTable(companyDBName, tableName);

				if (!LockApplication.Locks.IsValidToken(authenticationToken))
					return false;

				return LockApplication.Locks.UnlockAllForCompanyDBNameAndTable(companyDBName, tableName);
			}
			catch (Exception ex)
			{
				WriteError(ex);
				throw ex;
			}
			finally
			{
				UnlockResources();
			}
			
		}

		/// <summary>
		/// Libera dati prenotati
		/// </summary>
		/// <param name="companyDBName">Database aziendale</param>
		/// <param name="tableName">tabella contenente il dato</param>
		/// <param name="user">Nome dell'utente</param>
		/// <returns>true se ha successo</returns>
		[WebMethod]
			//-----------------------------------------------------------------------
		public bool UnlockAllForCompanyDBNameAndTableAndUser(string companyDBName, string tableName, string user, string authenticationToken)
		{
			try
			{
				LockResources();			
				if (LockApplication.Locks.IsValidTokenForConsole(authenticationToken))
					return LockApplication.Locks.UnlockAllForCompanyDBNameAndTableAndUser(companyDBName, tableName, user);

				if (!LockApplication.Locks.IsValidToken(authenticationToken))
					return false;

				return LockApplication.Locks.UnlockAllForCompanyDBNameAndTableAndUser(companyDBName, tableName, user);
		
			}
			catch (Exception ex)
			{
				WriteError(ex);
				throw ex;
			}
			finally
			{
				UnlockResources();
			}
		}

		/// <summary>
		/// Libera dati prenotati
		/// </summary>
		/// <param name="companyDBName">Database aziendale</param>
		/// <param name="tableName">tabella contenente il dato</param>
		/// <param name="lockKey">Chiave di lock composta</param>
		/// <returns>true se ha successo</returns>
		[WebMethod]
			//-----------------------------------------------------------------------
		public bool UnlockCompanyDBNameAndTableAndLock(string companyDBName, string tableName, string lockKey, string authenticationToken)
		{
			try
			{
				LockResources();			
				if (!LockApplication.Locks.IsValidToken(authenticationToken))
					return false;

				return LockApplication.Locks.UnlockCompanyDBNameAndTableAndLock(companyDBName, tableName, lockKey);

			}
			catch (Exception ex)
			{
				WriteError(ex);
				throw ex;
			}
			finally
			{
				UnlockResources();
			}
		}

		[WebMethod]
			//-----------------------------------------------------------------------
		public void RemoveUnusedLocks()
		{
			try
			{
				LockResources();			
				LockApplication.Locks.RemoveUnusedLocks();
			}
			catch (Exception ex)
			{
				WriteError(ex);
				throw ex;
			}
			finally
			{
				UnlockResources();
			}
			
		}
	
		#endregion
	}
}

