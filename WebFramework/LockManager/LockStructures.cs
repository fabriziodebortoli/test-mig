using System;
using System.Collections;
using System.Globalization;

namespace Microarea.WebServices.LockManager
{
	/// <summary>
	/// Oggetto che rappresenta un lock di database in memoria
	/// </summary>
	//=========================================================================
	public class SqlLockEntry
	{
		/// <summary>
		/// Chiave primaria composta del record dul database
		/// </summary>
		public string		LockKey;

		/// <summary>
		/// Token di autenticazione legatop all'utente / processo che ha effettuato il lock
		/// </summary>
		public string		AuthenticationToken;

		public string		UserName;

		/// <summary>
		/// Indirizzo in memoria dell'oggetto (il contesto) che ha eseguito il lock
		/// </summary>
		public string		Address;
		
		/// <summary>
		/// date del lock
		/// </summary>
		public DateTime		LockDate;

		/// <summary>
		/// Nome del processo che ha effettuato il lock
		/// </summary>
		public string		ProcessName;
		
		/// <summary>
		/// Costruttore di un lock
		/// </summary>
		/// <param name="aLockKey">Chiave primaria composta</param>
		/// <param name="authenticationToken">Token di autenticazione utente</param>
		/// <param name="aAddress">Indirizzo in memoria dell'oggetto che ha richiesto il lock</param>
		/// <param name="alockDate">Data di lock</param>
		//-----------------------------------------------------------------------
		public SqlLockEntry(string aLockKey, string authenticationToken, string userName, string aAddress, DateTime alockDate, string aProcessName)
		{
			LockKey				= aLockKey;
			AuthenticationToken	= authenticationToken;
			UserName			= userName;
			Address				= aAddress;
			LockDate			= alockDate;
			ProcessName			= aProcessName;
		}

		/// <summary>
		/// verifica se é un lock giá effettuato per la stessa macchina, stesso processo,
		/// stesso utente, stessa transazione
		/// </summary>
		/// <param name="aSqlLockEntry">Il lock da controllare</param>
		/// <returns>true se il lock non era presente e quindi lo inserisce o se il documento aveva già lo stesso lock sulla tabella</returns>
		//-----------------------------------------------------------------------
		public bool IsSameLock(SqlLockEntry aSqlLockEntry)
		{
			return
				(
				aSqlLockEntry		!= null									&&
				AuthenticationToken	== aSqlLockEntry.AuthenticationToken	&&
				Address				== aSqlLockEntry.Address  
				);			
		}
	}

	/// <summary>
	/// Oggetto che contiene tutti i lock per una tabella
	/// </summary>
	//=========================================================================
	public class TableLockEntry
	{
		/// <summary>
		/// Nome della tabella
		/// </summary>
		public	 string		TableName;

		/// <summary>
		/// Array dei lock della tabella
		/// </summary>
		public ArrayList	SqlLocksEntries = null;

		/// <summary>
		/// costruttore
		/// </summary>
		/// <param name="aTableName">Nome della tabella</param>
		//-----------------------------------------------------------------------
		public TableLockEntry(string aTableName)
		{
			SqlLocksEntries = new ArrayList();
			TableName = aTableName;
		}

		//-----------------------------------------------------------------------
		public SqlLockEntry GetLockByLockKey(string lockKey)
		{
			if (lockKey == string.Empty)
				return null;

			//verifico che il lock non sia stato già inserito.
			foreach(SqlLockEntry aSqlLockEntry in SqlLocksEntries)
			{
				//se è già stato inserito
				if (aSqlLockEntry.LockKey == lockKey)
					return aSqlLockEntry;
			}

			return null;
		}
		
		/// <summary>
		/// Aggiunge un lock sulla tabella
		/// </summary>
		/// <param name="newSqlLockEntry">Il lock da inserire</param>
		/// <returns>true se il lock non era presente e quindi lo inserisce o se il documento aveva già lo stesso lock sulla tabella</returns>
		//-----------------------------------------------------------------------
        //-----------------------------------------------------------------------
        public bool AddLockEntry(SqlLockEntry newSqlLockEntry, out SqlLockEntry existLockEntry)
        {
            //controllo se é lo stesso lock
            SqlLockEntry aSqlLockEntry = GetLockByLockKey(newSqlLockEntry.LockKey);
            existLockEntry = aSqlLockEntry;
            if (aSqlLockEntry != null)
                //controllo se é lo stesso lock
                // true se è lo stesso lock
                // false risorsa locked 
                return aSqlLockEntry.IsSameLock(newSqlLockEntry);

            //il lock non esiste => lo aggiungo.
            return (SqlLocksEntries.Add(newSqlLockEntry) >= 0);
        }

		/// <summary>
		/// Rimuove tutti i lock con un particolare autentication token
		/// </summary>
		/// <param name="authenticationToken">Il token di autenticazione dell'utente</param>
		/// <returns>true se ha avuto successo</returns>
		//-----------------------------------------------------------------------
		public bool RemoveAuthenticationTokenEntries(string authenticationToken)
		{
			if (authenticationToken == string.Empty)
				return false;

			if (SqlLocksEntries == null)
				return true;

			for (int i = SqlLocksEntries.Count - 1 ; i >= 0 ; i--)
			{
				SqlLockEntry aSqlLockEntry = (SqlLockEntry)SqlLocksEntries[i];

				if (aSqlLockEntry == null)
					continue;

				//se l'ha inserito lo stesso doc lo rimuovo
				if (string.Compare(aSqlLockEntry.AuthenticationToken, authenticationToken, true, CultureInfo.InvariantCulture) == 0)
				{
					SqlLocksEntries.RemoveAt(i);
					continue;
				}
			}

			return true;
		}

		/// <summary>
		/// Rimuove tutti i lock con un particolare utente
		/// </summary>
		/// <param name="userName">Il nome dell'utente</param>
		/// <returns>true se ha avuto successo</returns>
		//-----------------------------------------------------------------------
		public bool RemoveUserLocks(string userName)
		{
			if (userName == string.Empty)
				return false;

			if (SqlLocksEntries == null)
				return true;

			for( int i = SqlLocksEntries.Count - 1 ; i >= 0 ; i--)
			{
				if (((SqlLockEntry) SqlLocksEntries[i]).UserName == userName)
					SqlLocksEntries.RemoveAt(i);
			}

			return true;
		}

		/// <summary>
		/// Rimuove i lock con una particolare chiave primaria
		/// </summary>
		/// <param name="lockKey">PK del lock</param>
		/// <returns>true se ha avuto successo</returns>
		//-----------------------------------------------------------------------
		public bool RemoveKeyLocks(string lockKey)
		{
			if (lockKey == string.Empty)
				return false;

			if (SqlLocksEntries == null)
				return true;

			for( int i = SqlLocksEntries.Count - 1 ; i >= 0 ; i--)
			{
				if (((SqlLockEntry) SqlLocksEntries[i]).LockKey == lockKey)
					SqlLocksEntries.RemoveAt(i);
			}

			return true;
		}
	}
		
	/// <summary>
	/// Oggetto che contiene tutti i lock relativi ad una compagnia
	/// </summary>
	//=========================================================================
	public class CompanyLockInfo
	{
		/// <summary>
		/// ID della compagnia
		/// </summary>
		public	string		CompanyDBName;

		/// <summary>
		/// Array delle tabelle gestite dalla compagnia
		/// </summary>
		public ArrayList	TablelocksEntries;
		
		/// <summary>
		/// costruttore
		/// </summary>
		/// <param name="aCompanyId">ID della compagnia</param>
		//-----------------------------------------------------------------------
		public CompanyLockInfo(string companyDBName)
		{
			CompanyDBName = companyDBName;
			TablelocksEntries =  new ArrayList();
		}

		/// <summary>
		/// Restituisce il TableLockEntry relativo al nome tabella passato
		/// </summary>
		/// <param name="tableName">nome tabella</param>
		/// <returns>Un TableLockEntry o null se non esiste</returns>
		//-----------------------------------------------------------------------
		public TableLockEntry GetTableLockEntry(string tableName)
		{
			if (TablelocksEntries == null || tableName == null || tableName.Length == 0)
				return null;

			foreach(TableLockEntry aTableLockEntry in TablelocksEntries)
			{
				if (aTableLockEntry.TableName == tableName)
					return aTableLockEntry;
			}

			return null;
		}

		/// <summary>
		/// Rimuove tutti i lock di una tabella
		/// </summary>
		/// <param name="tableName">Il nome della tabella</param>
		/// <returns>true se ha avuto successo</returns>
		//-----------------------------------------------------------------------
		public bool RemoveTableLocks(string tableName)
		{
			if (tableName == string.Empty)
				return false;

			TableLockEntry tableLockEntry = GetTableLockEntry(tableName);

			if (tableLockEntry == null)
				return true;

			TablelocksEntries.Remove(tableLockEntry);
			
			return true;
		}

        /// <summary>
        /// Aggiunge un lock ad una tabella
        /// </summary>
        /// <param name="aSqlLockEntry">Il lock da inserire</param>
        /// <param name="tableName">La tabella in cui mettere il lock</param>
        /// <param name="lockUser">in caso di record già in stato di lock restituisce l'utente che impegna il dato</param>
        /// <param name="lockApp">in caso di record già in stato di lock restituisce l'applicazione che impegna il dato</param>
        /// <returns>true se il lock viene inserito correttamente</returns>
        //-----------------------------------------------------------------------
        public bool AddLock(string tableName, SqlLockEntry aSqlLockEntry, out string lockUser, out string lockApp)
        {
            lockUser = string.Empty;
            lockApp = string.Empty;

            if (TablelocksEntries == null)
                return false;

            if (
                tableName == string.Empty ||
                aSqlLockEntry.ProcessName == string.Empty ||
                aSqlLockEntry.UserName == string.Empty ||
                aSqlLockEntry.LockKey == string.Empty ||
                aSqlLockEntry.Address == string.Empty
                )
                return false;

            TableLockEntry aTableLockEntry = GetTableLockEntry(tableName);
            if (aTableLockEntry == null)
            {
                aTableLockEntry = new TableLockEntry(tableName);
                TablelocksEntries.Add(aTableLockEntry);
            }

            SqlLockEntry existLockEntry = null;
            if (!aTableLockEntry.AddLockEntry(aSqlLockEntry, out existLockEntry))
            {
                if (existLockEntry != null)
                {
                    lockUser = existLockEntry.UserName;
                    lockApp = existLockEntry.ProcessName;
                }
                return false;
            }
            return true;
        }
	}
}
