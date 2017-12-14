using System;
using System.Collections;
using System.Diagnostics;
using System.Globalization;
using System.IO;
using System.Xml;
using Microarea.TaskBuilderNet.Core.NameSolver;
using Microarea.TaskBuilderNet.Core.Generic;

namespace Microarea.WebServices.LockManager
{
	/// <summary>
	/// Istanza unica dell'applicazione LockManager
	/// </summary>
	public class LockApplication
	{
		private static LockGlobal lockGlobal = null;
		public	static LockGlobal Locks		{ get { return lockGlobal; } }

		/// <summary>
		/// Costruttore statico
		/// </summary>
		static LockApplication()
		{
			lockGlobal = new LockGlobal();
		}
	}

	/// <summary>
	/// Struttura dati lock comune a tutte le istanze.
	/// </summary>
	//=========================================================================
	public class LockGlobal
	{
		private ArrayList		companyLocks = new ArrayList();
        private Guid            lockSessionID    = Guid.Empty;
		/// <summary>
		/// Array delle strutture relative alle compagnie gestite da LockManager
		/// </summary>
		public	ArrayList		CompanyLocks { get { return companyLocks; } }

		private LoginCache loginCache;

		//-----------------------------------------------------------------------
        public Guid LockSessionID { get { return lockSessionID; } }        
        
        /// <summary>
		/// costruttore
		/// </summary>
		//-----------------------------------------------------------------------
		public LockGlobal()
		{
			loginCache = new LoginCache(BasePathFinder.BasePathFinderInstance.LoginManagerUrl);
            Initialize();
		}

		//-----------------------------------------------------------------------
        internal void Initialize()
        {
            lockSessionID = Guid.NewGuid();
        }

		//-----------------------------------------------------------------------
		internal bool IsValidToken(string authenticationToken)
		{
			return loginCache.CheckToken(authenticationToken);
		}

		//---------------------------------------------------------------------------
		internal bool IsValidTokenForConsole(string authenticationToken)
		{
			return authenticationToken == InstallationData.ServerConnectionInfo.SysDBConnectionString;
		}

		/// <summary>
		/// Aggiunge la gestione dei lock di una compagnia
		/// </summary>
		/// <param name="companyID">ID dell'azienda</param>
		/// <returns>true se la compagnia era già gestita o se è riuscito l'inserimento di una nuova</returns>
		//-----------------------------------------------------------------------
		internal CompanyLockInfo AddCompany(string companyDBName)
		{
			CompanyLockInfo aCompanyLockInfo = GetCompanyLockInfo(companyDBName);
			if (aCompanyLockInfo == null)
			{
				aCompanyLockInfo = new CompanyLockInfo(companyDBName);
				if (companyLocks.Add(aCompanyLockInfo) >= 0)
					return aCompanyLockInfo;
			}

			return aCompanyLockInfo;
		}

		/// <summary>
		/// Restituisce il gestore dei lock di una compagnia
		/// </summary>
		/// <param name="companyDBName">database aziendale</param>
		/// <returns>CompanyLockInfo</returns>
		//-----------------------------------------------------------------------
		public CompanyLockInfo GetCompanyLockInfo(string companyDBName)
		{
			if (companyLocks == null)
				return null;

			foreach(CompanyLockInfo aCompanyLockInfo in companyLocks)
			{
				if (aCompanyLockInfo.CompanyDBName == companyDBName)
					return aCompanyLockInfo;
			}

			return null;
		}

		/// <summary>
		/// Elimina la gestione dei lock di una compagnia e tutti i lock in essa finora gestiti
		/// </summary>
		/// <param name="companyDBName">Nome del db aziendale</param>
		/// <returns>true se riesce a rimuovere i dati dell'azienda specificata</returns>
		//-----------------------------------------------------------------------
		private bool RemoveCompany(string companyDBName)
		{
			if (companyDBName == null || companyDBName == string.Empty)
				return false;

			if (companyLocks == null)
				return true;

			CompanyLockInfo aCompanyLockInfo = GetCompanyLockInfo(companyDBName);

			companyLocks.Remove(aCompanyLockInfo);

			return true;
		}

		/// <summary>
		/// Verfica che il lock sia stato effettuato dall'utente con il token di autenticazione passato
		/// </summary>
		/// <param name="aLockEntry">Lock da verificare</param>
		/// <param name="authenticationToken">token di autenticazione dell'utente</param>
		/// <returns>true se il lock gli appartiene</returns>
		//-----------------------------------------------------------------------
		private bool IsMyAuthenticationToken(SqlLockEntry aLockEntry, string authenticationToken)
		{
			return aLockEntry.AuthenticationToken == authenticationToken;	
		}

		/// <summary>
		/// Prenota un dato
		/// </summary>
		/// <param name="companyDBName">Nome del db aziendale</param>
		/// <param name="authenticationToken">token di autenticazione dell'utente</param>
		/// <param name="userName">nome utente</param>
		/// <param name="tableName">nome tabella</param>
		/// <param name="lockKey">chiave primaria del dato da prenotare</param>
		/// <param name="address">indirizzo in memoria del documento</param>
		/// <param name="processName">nome del processo che effettua un lock</param>
        /// <param name="lockUser">in caso di record già in stato di lock restituisce l'utente che impegna il dato</param>
        /// <param name="lockApp">in caso di record già in stato di lock restituisce l'applicazione che impegna il dato</param>
        /// <returns>true se il dato è stato prenotato con successo</returns>
        //---------------------------------------------------------------------------
        internal bool AddLock(string companyDBName, string authenticationToken, string userName, string tableName, string lockKey, string address, string processName, out string lockUser, out string lockApp)
        {
            CompanyLockInfo companyLockInfo = AddCompany(companyDBName);

            SqlLockEntry sqlLockEntry = new SqlLockEntry
                (
                lockKey,
                authenticationToken,
                userName,
                address,
                DateTime.Now,
                processName
                );

            return companyLockInfo.AddLock(tableName, sqlLockEntry, out lockUser, out lockApp);
        }

		/// <summary>
		/// Verifica se un dato è stato prenotato
		/// </summary>
		/// <param name="companyDBName">Nome del database aziendale</param>
		/// <param name="tableName">Nome della tabella</param>
		/// <param name="lockKey">Chiave primaria del lock</param>
		/// <param name="address">Indirizzo in memoria del documento</param>
		/// <returns>true se la funzione ha successo</returns>
		//-----------------------------------------------------------------------
		internal bool IsRecordLocked(string companyDBName, string tableName, string lockKey)
		{
			CompanyLockInfo companyLockInfo = GetCompanyLockInfo(companyDBName);

			if (companyLockInfo == null)
				return false;

			TableLockEntry aTableLockEntry = companyLockInfo.GetTableLockEntry(tableName);
			if (aTableLockEntry == null)
				return false;

			return (aTableLockEntry.GetLockByLockKey(lockKey) != null);
		}

		/// <summary>
		/// Verifica se un dato è stato prenotato da un altro contesto
		/// </summary>
		/// <param name="companyDBName">Nome del database aziendale</param>
		/// <param name="tableName">Nome della tabella</param>
		/// <param name="lockKey">Chiave primaria del lock</param>
		/// <param name="address">Indirizzo in memoria del contesto che richiede se il dato è in stato di locked</param>
		/// <returns>true il dato è stato prenotato da un altro contesto false se non è stato prenotato oppure è prenotato dallo stesso contesto</returns>
		//-----------------------------------------------------------------------
		internal bool IsCurrentLocked(string companyDBName, string tableName, string lockKey, string address)
		{
			CompanyLockInfo companyLockInfo = GetCompanyLockInfo(companyDBName);

			if (companyLockInfo == null)
				return false;

			TableLockEntry aTableLockEntry = companyLockInfo.GetTableLockEntry(tableName);
			if (aTableLockEntry == null)
				return false;

			SqlLockEntry aSqlLockEntry = aTableLockEntry.GetLockByLockKey(lockKey);
			if (aSqlLockEntry == null)
				return false;

			return aSqlLockEntry.Address != address;
		}

		
		//-----------------------------------------------------------------------
		/// <summary>
		/// Verifica se il record passato come chiave è stato lockato dal contesto stesso individuato da address
		/// E' l'opposto dell'IsCurrentLocked
		/// </summary>
		/// <param name="companyDBName">Nome del database aziendale</param>
		/// <param name="tableName">Nome della tabella</param>
		/// <param name="lockKey">Chiave primaria del lock</param>
		/// <param name="address">Indirizzo in memoria del contesto che richiede se il dato è in stato di locked</param>
		/// <returns>true il dato è stato prenotato dallo stesso contesto false se non è stato prenotato oppure è prenotato da altro contesto</returns>
		internal bool IsMyLock(string companyDBName, string tableName, string lockKey, string address)
		{
			CompanyLockInfo companyLockInfo = GetCompanyLockInfo(companyDBName);

			if (companyLockInfo == null)
				return false;

			TableLockEntry aTableLockEntry = companyLockInfo.GetTableLockEntry(tableName);
			if (aTableLockEntry != null)
			{
				SqlLockEntry aSqlLockEntry = aTableLockEntry.GetLockByLockKey(lockKey);
				return aSqlLockEntry != null && aSqlLockEntry.Address == address;
			}

			return false;
		}

		/// <summary>
		/// Prende informazioni su un lock
		/// </summary>
		/// <param name="companyDBName">Nome del database aziendale</param>
		/// <param name="lockKey">Chiave primaria del lock</param>
		/// <param name="tableName">Nome della tabella</param>
		/// <param name="lockerToken">Token di autenticazione dell'utente</param>
		/// <param name="lockTime">Istante di prenotazione del dato</param>
		/// <param name="processName">Nome del processo che ha prenotato il dato</param>
		/// <returns>true se la funzione ha avuto successo</returns>
		//-----------------------------------------------------------------------
		internal bool GetLockInfo(string companyDBName, string lockKey, string tableName, out string user, out DateTime lockTime, out string processName)
		{
			user		= string.Empty;
			lockTime	= DateTime.MinValue;
			processName = string.Empty;
			
			CompanyLockInfo companyLockInfo = GetCompanyLockInfo(companyDBName);
			if (companyLockInfo == null)
				return false;

			TableLockEntry tableLockEntry = companyLockInfo.GetTableLockEntry(tableName);
			if (tableLockEntry == null)
				return false;

			SqlLockEntry sqlLockEntry = tableLockEntry.GetLockByLockKey(lockKey);
			if (sqlLockEntry == null)
				return false;

			user		= sqlLockEntry.UserName;
			lockTime	= sqlLockEntry.LockDate;
			processName	= sqlLockEntry.ProcessName;

			return true;
		}

		#region funzioni per la rimozione di lock

		/// <summary>
		/// Rimuove tutti i lock
		/// </summary>
		//-----------------------------------------------------------------------
		internal void UnlockAll()
		{
			if (companyLocks != null)
				companyLocks.Clear();
		}

		/// <summary>
		/// Rimuove tutte le prenotazioni di un utente di un'azienda
		/// </summary>
		/// <param name="authenticationToken"></param>
		/// <param name="companyDBName">Nome del database aziendale</param>
		/// <returns>true se la funzione ha avuto successo</returns>
		//-----------------------------------------------------------------------
		internal bool UnlockAllForUser(string authenticationToken, string companyDBName)
		{
			CompanyLockInfo companyLockInfo = GetCompanyLockInfo(companyDBName);

			if (companyLockInfo == null)
				return false;
			
			foreach(TableLockEntry aTableLockEntry in companyLockInfo.TablelocksEntries)
				aTableLockEntry.RemoveAuthenticationTokenEntries(authenticationToken);
			
			return true;
		}

		/// <summary>
		/// Rimuove la prenotazione di un record
		/// </summary>
		/// <param name="companyDBName">Nome del database aziendale</param>
		/// <param name="authenticationToken"></param>
		/// <param name="tableName">Nome della tabella</param>
		/// <param name="lockKey">Chiave primaria del lock</param>
		/// <param name="address">Indirizzo in memoria del documento</param>
		/// <returns>true se la funzione ha avuto successo</returns>
		//---------------------------------------------------------------------------
		internal bool UnlockRecord(string companyDBName, string authenticationToken, string tableName, string lockKey, string address)
		{
			CompanyLockInfo companyLockInfo = GetCompanyLockInfo(companyDBName);

			if (companyLockInfo == null)
				return false;

			TableLockEntry aTableLockEntry = companyLockInfo.GetTableLockEntry(tableName);
			
			if (aTableLockEntry == null)
				return false;

			SqlLockEntry aSqlLockEntry = aTableLockEntry.GetLockByLockKey(lockKey);
			if (aSqlLockEntry == null)
				return false;
				
			if (
				IsMyAuthenticationToken(aSqlLockEntry, authenticationToken) &&
                (string.Compare(aSqlLockEntry.Address, address, true) == 0)
				)
				aTableLockEntry.SqlLocksEntries.Remove(aSqlLockEntry);

			if (aTableLockEntry.SqlLocksEntries.Count == 0)
				companyLockInfo.TablelocksEntries.Remove(aTableLockEntry);

			if (companyLockInfo.TablelocksEntries.Count == 0)
				RemoveCompany(companyDBName);
		
			return true;
		}

		/// <summary>
		/// Rimuove tutte le prenotazioni di un contesto di documento
		/// </summary>
		/// <param name="companyDBName">Nome del database aziendale</param>
		/// <param name="authenticationToken"></param>
		/// <param name="address">Indirizzo in memoria del documento</param>
		/// <returns>true se la funzione ha avuto successo</returns>
		//-----------------------------------------------------------------------
		internal bool UnlockAllContext(string companyDBName, string authenticationToken, string address)
		{
			CompanyLockInfo companyLockInfo = GetCompanyLockInfo(companyDBName);

			if (companyLockInfo == null || companyLockInfo.TablelocksEntries == null)
				return false;

			foreach(TableLockEntry aTableLockEntry in companyLockInfo.TablelocksEntries)
			{
				if (aTableLockEntry.SqlLocksEntries == null)
					continue;

				for (int i = aTableLockEntry.SqlLocksEntries.Count - 1 ; i >= 0 ; i--)
				{
					SqlLockEntry sqlLockEntry = (SqlLockEntry)aTableLockEntry.SqlLocksEntries[i];

					if (
						sqlLockEntry != null &&
						IsMyAuthenticationToken(sqlLockEntry, authenticationToken)	&&
                        (string.Compare(sqlLockEntry.Address, address, true) == 0)
						)
						aTableLockEntry.SqlLocksEntries.RemoveAt(i);
				}
			}

			return true;
		}

		/// <summary>
		/// Rimuove tutte le prenotazioni di un contesto di documento su una tabella
		/// </summary>
		/// <param name="companyDBName">Nome del database aziendale</param>
		/// <param name="authenticationToken"></param>
		/// <param name="tableName">Nome della tabella</param>
		/// <param name="address">Indirizzo in memoria del documento</param>
		/// <returns>true se la funzione ha avuto successo</returns>
		//-----------------------------------------------------------------------
		internal bool UnlockAll(string companyDBName, string authenticationToken, string tableName, string address)
		{
			CompanyLockInfo companyLockInfo = GetCompanyLockInfo(companyDBName);

			if (companyLockInfo == null || companyLockInfo.TablelocksEntries == null)
				return false;

			TableLockEntry tableLockEntry = companyLockInfo.GetTableLockEntry(tableName);
			if (tableLockEntry == null)
				return false;
			
			for (int i = tableLockEntry.SqlLocksEntries.Count - 1 ; i >= 0 ; i--)
			{
				SqlLockEntry sqlLockEntry = (SqlLockEntry)tableLockEntry.SqlLocksEntries[i];

				if (
					IsMyAuthenticationToken(sqlLockEntry, authenticationToken)	&&
                    (string.Compare(sqlLockEntry.Address, address, true) == 0)
					)
					tableLockEntry.SqlLocksEntries.RemoveAt(i);
			}
		
			return true;
		}

		/// <summary>
		/// Rimuove tutte le prenotazioni di un utente connesso ad un'azienda
		/// </summary>
		/// <param name="companyDBName">Nome del database aziendale</param>
		/// <param name="authenticationToken"></param>
		/// <returns>true se la funzione ha avuto successo</returns>
		//-----------------------------------------------------------------------
		internal bool UnlockAllForCurrentConnection(string companyDBName, string authenticationToken)
		{
			CompanyLockInfo companyLockInfo = GetCompanyLockInfo(companyDBName);

			if (
				companyLockInfo		== null			|| 
				authenticationToken == string.Empty	||	
				companyLockInfo.TablelocksEntries == null
				)
				return false;

			foreach(TableLockEntry aTableLockEntry in companyLockInfo.TablelocksEntries)
			{				
				for (int i = aTableLockEntry.SqlLocksEntries.Count - 1 ; i >= 0 ; i--)
				{
					SqlLockEntry aSqlLockEntry = (SqlLockEntry)aTableLockEntry.SqlLocksEntries[i];

					if (aSqlLockEntry == null)
						continue;

					if (IsMyAuthenticationToken(aSqlLockEntry, authenticationToken))
						aTableLockEntry.SqlLocksEntries.RemoveAt(i);
				}				
			}
			
			return true;
		}

		/// <summary>
		/// Rimuove tutte le prenotazioni di un utente
		/// </summary>
		/// <param name="userName"></param>
		/// <returns>true se la funzione ha avuto successo</returns>
		//-----------------------------------------------------------------------
		internal bool UnlockAllForUser(string userName)
		{
			if (userName == string.Empty || companyLocks == null)
				return false;

			foreach(CompanyLockInfo aCompanyLockInfo in companyLocks)
			{
				if (aCompanyLockInfo.TablelocksEntries == null)
					continue;

				foreach(TableLockEntry aTableLockEntry in aCompanyLockInfo.TablelocksEntries)
				{
					if (aTableLockEntry.SqlLocksEntries == null)
						continue;

					for( int i = aTableLockEntry.SqlLocksEntries.Count - 1 ; i >= 0 ; i--)
					{
						if (string.Compare(((SqlLockEntry) aTableLockEntry.SqlLocksEntries[i]).UserName, userName, true, CultureInfo.InvariantCulture) == 0)
							aTableLockEntry.SqlLocksEntries.RemoveAt(i);
					}
				}
			}

			return true;
		}

		/// <summary>
		/// Rimuove tutte le prenotazioni di un'azienda
		/// </summary>
		/// <param name="companyDBName">Nome del database aziendale</param>
		/// <returns>true se la funzione ha avuto successo</returns>
		//-----------------------------------------------------------------------
		internal bool UnlockAllForCompanyDBName(string companyDBName)
		{
			return RemoveCompany(companyDBName);
		}

		/// <summary>
		/// Rimuove tutte le prenotazioni di un utente appartenente ad un'azienda
		/// </summary>
		/// <param name="companyDBName">Nome del database aziendale</param>
		/// <param name="userName"></param>
		/// <returns>true se la funzione ha avuto successo</returns>
		//-----------------------------------------------------------------------
		internal bool UnlockAllForCompanyDBNameAndUser(string companyDBName, string userName)
		{
			if (companyDBName == string.Empty || userName == string.Empty || companyLocks == null)
				return false;

			CompanyLockInfo aCompanyLockInfo = GetCompanyLockInfo(companyDBName);
			
			if (aCompanyLockInfo == null || aCompanyLockInfo.TablelocksEntries == null)
				return true;

			foreach(TableLockEntry aTableLockEntry in aCompanyLockInfo.TablelocksEntries)
				aTableLockEntry.RemoveUserLocks(userName);
			
			return true;
		}

		/// <summary>
		/// Rimuove tutte le prenotazioni di una tabella per un'azienda
		/// </summary>
		/// <param name="companyDBName">Nome del database aziendale</param>
		/// <param name="tableName">Nome della tabella</param>
		/// <returns>true se la funzione ha avuto successo</returns>
		//-----------------------------------------------------------------------
		internal bool UnlockAllForCompanyDBNameAndTable(string companyDBName, string tableName)
		{
			if (companyDBName == string.Empty || tableName == string.Empty || companyLocks == null)
				return false;

			CompanyLockInfo companyLockInfo = GetCompanyLockInfo(companyDBName);
			if (companyLockInfo == null)
				return true;

			return companyLockInfo.RemoveTableLocks(tableName);
		}

		/// <summary>
		/// Rimuove tutte le prenotazioni per utente azienda e tabella
		/// </summary>
		/// <param name="companyDBName">Nome del database aziendale</param>
		/// <param name="tableName">Nome della tabella</param>
		/// <param name="userName"></param>
		/// <returns>true se la funzione ha avuto successo</returns>
		//-----------------------------------------------------------------------
		internal bool UnlockAllForCompanyDBNameAndTableAndUser(string companyDBName, string tableName, string userName)
		{
			if (
				companyDBName	== string.Empty || 
				tableName		== string.Empty || 
				userName		== string.Empty || 
				companyLocks	== null
				)
				return false;
			
			CompanyLockInfo companyLockInfo = GetCompanyLockInfo(companyDBName);
			if (companyLockInfo == null)
				return true;

			TableLockEntry tableLockEntry = companyLockInfo.GetTableLockEntry(tableName);
			if (tableLockEntry == null)
				return true;
		
			return tableLockEntry.RemoveUserLocks(userName);
		}

		/// <summary>
		/// Rimuove tutte le prenotazioni per pk azienda e tabella
		/// </summary>
		/// <param name="companyDBName">Nome del database aziendale</param>
		/// <param name="tableName">Nome della tabella</param>
		/// <param name="lockKey">Chiave primaria del lock</param>
		/// <returns>true se la funzione ha avuto successo</returns>
		//-----------------------------------------------------------------------
		internal bool UnlockCompanyDBNameAndTableAndLock(string companyDBName, string tableName, string lockKey)
		{
			if (
				companyDBName	== string.Empty || 
				tableName		== string.Empty || 
				lockKey			== string.Empty || 
				companyLocks	== null
				)
				return false;
			
			CompanyLockInfo companyLockInfo = GetCompanyLockInfo(companyDBName);
			if (companyLockInfo == null)
				return true;

			TableLockEntry tableLockEntry = companyLockInfo.GetTableLockEntry(tableName);
			if (tableLockEntry == null)
				return true;
		
			return tableLockEntry.RemoveKeyLocks(lockKey);
		}

		//-----------------------------------------------------------------------
		internal void RemoveUnusedLocks()
		{
			Microarea.TaskBuilderNet.Core.WebServicesWrapper.LoginManager lm = 
				new Microarea.TaskBuilderNet.Core.WebServicesWrapper.LoginManager(BasePathFinder.BasePathFinderInstance.LoginManagerUrl, 10000);
			if (companyLocks == null)
				return;

			foreach(CompanyLockInfo aCompanyLockInfo in companyLocks)
			{
				if (aCompanyLockInfo.TablelocksEntries == null)
					continue;

				foreach(TableLockEntry aTableLockEntry in aCompanyLockInfo.TablelocksEntries)
				{
					if (aTableLockEntry.SqlLocksEntries == null)
						continue;

					for( int i = aTableLockEntry.SqlLocksEntries.Count - 1 ; i >= 0 ; i--)
					{
						if (!lm.IsValidToken(((SqlLockEntry) aTableLockEntry.SqlLocksEntries[i]).AuthenticationToken))
							aTableLockEntry.SqlLocksEntries.RemoveAt(i);
					}
				}
			}
		}

		#endregion

		#region funzioni che scrivono xml con la situazione dei lock

		/// <summary>
		/// Crea un dom con l'elenco delle prenotazione
		/// </summary>
		/// <returns>true se la funzione ha avuto successo</returns>
		//-----------------------------------------------------------------------
		private XmlDocument GetXmlLockEntries()
		{
			if (companyLocks == null || companyLocks.Count == 0)
			{
				if (File.Exists(BasePathFinder.BasePathFinderInstance.GetLockLogFile()))
					File.Delete(BasePathFinder.BasePathFinderInstance.GetLockLogFile());
				
				return null;
			}

			XmlDocument doc = new XmlDocument();			
			
			XmlElement root = doc.CreateElement(Strings.Companies);
			doc.AppendChild(root);
			
			foreach(CompanyLockInfo companyLockInfo in companyLocks)
			{
				if (companyLockInfo.TablelocksEntries == null || companyLockInfo.TablelocksEntries.Count == 0)
					continue;

				XmlElement companyElement = doc.CreateElement(Strings.Company);
				companyElement.SetAttribute(Strings.CompanyDBName, companyLockInfo.CompanyDBName);
				root.AppendChild(companyElement);

				foreach(TableLockEntry aTableLockEntry in companyLockInfo.TablelocksEntries)
				{
					if (aTableLockEntry.SqlLocksEntries == null || aTableLockEntry.SqlLocksEntries.Count == 0)
						continue;
					
					XmlElement table = doc.CreateElement(Strings.Table);
					table.SetAttribute(Strings.Name, aTableLockEntry.TableName);

					companyElement.AppendChild(table);

					foreach(SqlLockEntry sqlLockEntry in aTableLockEntry.SqlLocksEntries)
					{
						XmlElement lockEntryElement = doc.CreateElement(Strings.Lock);
						
						lockEntryElement.SetAttribute(Strings.AuthenticationToken,	sqlLockEntry.AuthenticationToken);
						lockEntryElement.SetAttribute(Strings.Address,				sqlLockEntry.Address);
						lockEntryElement.SetAttribute(Strings.LockDate,				XmlConvert.ToString(sqlLockEntry.LockDate, @"yyyy-MM-ddTHH\:mm\:ss"));
						lockEntryElement.SetAttribute(Strings.LockKey,				sqlLockEntry.LockKey);
						lockEntryElement.SetAttribute(Strings.UserName,				sqlLockEntry.UserName);
						lockEntryElement.SetAttribute(Strings.ProcessName,			sqlLockEntry.ProcessName);

						table.AppendChild(lockEntryElement);
					}
				}
			}
			
			return doc;
		}

		/// <summary>
		/// Salva il DOM con le prenotazioni su file system
		/// </summary>
		//-----------------------------------------------------------------------
		private void SaveLockEntries()
		{
			XmlDocument doc = GetXmlLockEntries();
			if (doc == null)
				return;
			
			try
			{
				doc.Save(BasePathFinder.BasePathFinderInstance.GetLockLogFile());
			}
			catch(Exception err)
			{
				Debug.Fail(err.Message.ToString());
			}
		}

		/// <summary>
		/// Ritorna il DOM con le prenotazioni su stringa
		/// </summary>
		/// <returns>true se la funzione ha avuto successo</returns>
		//-----------------------------------------------------------------------
		internal string GetLockEntries()
		{
			XmlDocument doc = GetXmlLockEntries();
			if (doc == null)
				return string.Empty;
			
			try
			{
				return doc.InnerXml;
			}
			catch(Exception err)
			{
				Debug.Fail(err.Message.ToString());
				return string.Empty;
			}
		}

		/// <summary>
		/// Ritorna il dom con tutte le aziende e tabelle con prenotazioni
		/// </summary>
		/// <returns>true se la funzione ha avuto successo</returns>
		//-----------------------------------------------------------------------
		private XmlDocument GetXmlCompanyDBAndTableLocksList()
		{
			if (companyLocks == null || companyLocks.Count == 0)
			{
				if (File.Exists(BasePathFinder.BasePathFinderInstance.GetLockLogFile()))
					File.Delete(BasePathFinder.BasePathFinderInstance.GetLockLogFile());
				
				return null;
			}

			XmlDocument doc = new XmlDocument();			
			
			XmlElement root = doc.CreateElement(Strings.Companies);
			doc.AppendChild(root);
			
			foreach (CompanyLockInfo companyLockInfo in companyLocks)
			{
				if (companyLockInfo.TablelocksEntries == null || companyLockInfo.TablelocksEntries.Count == 0)
					continue;

				XmlElement companyElement = doc.CreateElement(Strings.Company);
				companyElement.SetAttribute(Strings.CompanyDBName, companyLockInfo.CompanyDBName);
				root.AppendChild(companyElement);

				foreach(TableLockEntry aTableLockEntry in companyLockInfo.TablelocksEntries)
				{
					if (aTableLockEntry.SqlLocksEntries == null || aTableLockEntry.SqlLocksEntries.Count == 0)
						continue;

					XmlElement table = doc.CreateElement(Strings.Table);
					table.SetAttribute(Strings.Name, aTableLockEntry.TableName);

					companyElement.AppendChild(table);
				}
			}

			return doc;
		}

		/// <summary>
		/// Salva il dom con tutte le aziende e tabelle con prenotazioni
		/// </summary>
		//-----------------------------------------------------------------------
		internal void GetCompanyDBAndTableLocksList()
		{
			XmlDocument doc = GetXmlCompanyDBAndTableLocksList();
			if (doc == null)
				return;

			try
			{
				doc.Save(BasePathFinder.BasePathFinderInstance.GetLockLogFile());
			}
			catch(Exception err)
			{
				Debug.Fail(err.Message.ToString());
			}
		}

		/// <summary>
		/// Ritorna il dom con tutte le prenotazioni di una tabella di un'azienda
		/// </summary>
		/// <param name="companyDBName">Nome del database aziendale</param>
		/// <param name="tableName">Nome della tabella</param>
		/// <returns>true se la funzione ha avuto successo</returns>
		//-----------------------------------------------------------------------
		private XmlDocument GetXmlLocksList(string companyDBName, string tableName)
		{
			if (
				companyLocks		== null			|| 
				companyLocks.Count	== 0			||
				companyDBName		== null			|| 
				companyDBName		== string.Empty	|| 
				tableName			== null			|| 
				tableName			== string.Empty
				)
			{
				if (File.Exists(BasePathFinder.BasePathFinderInstance.GetLockLogFile()))
					File.Delete(BasePathFinder.BasePathFinderInstance.GetLockLogFile());
			
				return null;
			}

			CompanyLockInfo companyLockInfo = GetCompanyLockInfo(companyDBName);
			if (companyLockInfo == null)
			{
				if (File.Exists(BasePathFinder.BasePathFinderInstance.GetLockLogFile()))
					File.Delete(BasePathFinder.BasePathFinderInstance.GetLockLogFile());
				return null;
			}

			TableLockEntry tableLockEntry = companyLockInfo.GetTableLockEntry(tableName);
			if (tableLockEntry == null)
			{
				if (File.Exists(BasePathFinder.BasePathFinderInstance.GetLockLogFile()))
					File.Delete(BasePathFinder.BasePathFinderInstance.GetLockLogFile());	
				return null;
			}
			
			XmlDocument doc = new XmlDocument();			
			
			XmlElement table = doc.CreateElement(Strings.Table);
			table.SetAttribute(Strings.Name, tableName);
			doc.AppendChild(table);
			
			foreach(SqlLockEntry sqlLockEntry in tableLockEntry.SqlLocksEntries)
			{
				XmlElement lockEntryElement = doc.CreateElement(Strings.Lock);
				
				lockEntryElement.SetAttribute(Strings.AuthenticationToken,	sqlLockEntry.AuthenticationToken);
				lockEntryElement.SetAttribute(Strings.Address,				sqlLockEntry.Address);
				lockEntryElement.SetAttribute(Strings.LockDate,				XmlConvert.ToString(sqlLockEntry.LockDate, @"yyyy-MM-ddTHH\:mm\:ss"));
				lockEntryElement.SetAttribute(Strings.LockKey,				sqlLockEntry.LockKey);
				lockEntryElement.SetAttribute(Strings.UserName,				sqlLockEntry.UserName);
				lockEntryElement.SetAttribute(Strings.ProcessName,			sqlLockEntry.ProcessName);

				table.AppendChild(lockEntryElement);
			}

			return doc;
		}

		/// <summary>
		/// Salva il dom con tutte le prenotazioni di una tabella di un'azienda
		/// </summary>
		/// <param name="companyDBName">Nome del database aziendale</param>
		/// <param name="tableName">Nome della tabella</param>
		//-----------------------------------------------------------------------
		internal void GetLocksList(string companyDBName, string tableName)
		{
			XmlDocument doc = GetXmlLocksList(companyDBName, tableName);

			if (doc == null)
				return;
			
			try
			{
				doc.Save(BasePathFinder.BasePathFinderInstance.GetLockLogFile());
			}
			catch(Exception err)
			{
				Debug.Fail(err.Message.ToString());
			}
		}

		#endregion
	}
}
