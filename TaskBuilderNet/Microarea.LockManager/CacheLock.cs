using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Microarea.LockManager
{
	/// <summary>
	/// Oggetto che rappresenta un lock di database in memoria
	/// </summary>
	//=========================================================================
	public class LockEntry
	{
		/// <summary>
		/// Chiave primaria composta del record del database
		/// </summary>
		public string LockKey;

		/// <summary>
		/// Indirizzo in memoria dell'oggetto (il contesto) che ha eseguito il lock
		/// </summary>
		public string Context;

		/// <summary>
		/// Data ed ora in cui si è eseguito il lock
		/// </summary>
		public DateTime LockDate;

		/// <summary>
		/// Data ed ora in cui si è eseguito il lock
		/// </summary>
		public string LockUser;

		/// <summary>
		/// Data ed ora in cui si è eseguito il lock
		/// </summary>
		public string LockApp;

		/// <summary>
		/// Costruttore di un lock
		/// </summary>
		/// <param name="aLockKey">Chiave primaria composta</param>
		/// <param name="aAddress">Indirizzo in memoria dell'oggetto che ha richiesto il lock</param>
		/// <param name="alockDate">Data di lock</param>
		//-----------------------------------------------------------------------
		public LockEntry(string aLockKey, string aAddress, DateTime lockTime)
		{
			LockKey = aLockKey;
			Context = aAddress;
			LockDate = lockTime;
		}

		/// <summary>
		/// verifica se é un lock giá effettuato per la stessa macchina, stesso processo,
		/// stesso utente, stessa transazione
		/// </summary>
		/// <param name="aSqlLockEntry">Il lock da controllare</param>
		/// <returns>true se il lock non era presente e quindi lo inserisce o se il documento aveva già lo stesso lock sulla tabella</returns>
		//-----------------------------------------------------------------------
		public bool IsSameLock(LockEntry aSqlLockEntry)
		{
			return
				(
				aSqlLockEntry != null &&
				LockKey == aSqlLockEntry.LockKey &&
				Context == aSqlLockEntry.Context
				);
		}

		//-----------------------------------------------------------------------
		public bool IsSameLock(string context)
		{
			return Context == context;
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
		public string TableName;

		/// <summary>
		/// Array dei lock della tabella
		/// </summary>
		public ArrayList SqlLocksEntries = null;

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
		public LockEntry GetLockByLockKey(string lockKey)
		{
			if (lockKey == string.Empty)
				return null;

			//verifico che il lock non sia stato già inserito.
			foreach (LockEntry aSqlLockEntry in SqlLocksEntries)
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
		/// <returns>aggiungo il lock alla cache</returns>
		//-----------------------------------------------------------------------
		//-----------------------------------------------------------------------
		public void AddLockEntry(LockEntry newSqlLockEntry)
		{		
			SqlLocksEntries.Add(newSqlLockEntry);
		}

			/// <summary>
		/// Rimuove i lock con una particolare chiave primaria
		/// </summary>
		/// <param name="lockKey">PK del lock</param>
		/// <returns>true se ha avuto successo</returns>
		//-----------------------------------------------------------------------
		public bool RemoveLockEntry(string lockKey)
		{
			if (lockKey == string.Empty)
				return false;

			if (SqlLocksEntries == null)
				return true;

			for (int i = SqlLocksEntries.Count - 1; i >= 0; i--)
			{
				if (((LockEntry)SqlLocksEntries[i]).LockKey == lockKey)
				{
					SqlLocksEntries.RemoveAt(i);
					break;
				}
			}

			return true;
		}

		/// <summary>
		/// Rimuove i lock aventi il contesto passato come parametro
		/// </summary>
		/// <param name="context">contesto di cui rimuovere i lock</param>
		/// <returns>true se ha avuto successo</returns>
		//-----------------------------------------------------------------------
		public bool RemoveEntriesForContext(string context)
		{
			if (SqlLocksEntries == null)
				return true;

			for (int i = SqlLocksEntries.Count - 1; i >= 0; i--)
			{
				if (((LockEntry)SqlLocksEntries[i]).Context == context)
				{
					SqlLocksEntries.RemoveAt(i);
					break;
				}
			}

			return true;

		}
	}
	public class CacheLocksEntries : ArrayList
	{
		//-----------------------------------------------------------------------
		public TableLockEntry GetTableLockEntry(string tableName)
		{
			if (string.IsNullOrEmpty(tableName))
				return null;

			foreach (TableLockEntry aTableLockEntry in this)
			{
				if (aTableLockEntry.TableName == tableName)
					return aTableLockEntry;
			}

			return null;
		}


		//-----------------------------------------------------------------------
		public bool ExistLockEntry(string tableName, string lockKey, string context)
		{
			TableLockEntry tableLockEntry = GetTableLockEntry(tableName);
			if (tableLockEntry != null)
			{
				LockEntry lockEntry = tableLockEntry.GetLockByLockKey(lockKey);
				return lockEntry != null && lockEntry.IsSameLock(context);
			}
			return false;
		}

		//-----------------------------------------------------------------------
		public LockEntry GetLockEntry(string tableName, string lockKey)
		{
			TableLockEntry tableLockEntry = GetTableLockEntry(tableName);
			return (tableLockEntry != null) ? tableLockEntry.GetLockByLockKey(lockKey) : null;
		}


		//-----------------------------------------------------------------------
		public void AddLockEntry(string tableName, string lockKey, string context, DateTime lockTime)
		{
			TableLockEntry tableLockEntry = GetTableLockEntry(tableName);
			if (tableLockEntry == null)
			{
				tableLockEntry = new TableLockEntry(tableName);
				Add(tableLockEntry);
			}
			tableLockEntry.AddLockEntry(new LockEntry(lockKey, context, lockTime));
		}

		//-----------------------------------------------------------------------
		public void RemoveLockEntry(string tableName, string lockKey)
		{
			TableLockEntry tableLockEntry = GetTableLockEntry(tableName);
			if (tableLockEntry == null)
				return;
			tableLockEntry.RemoveLockEntry(lockKey);
		}

		//-----------------------------------------------------------------------
		public void RemoveEntriesForContext(string context)
		{
			foreach (TableLockEntry tableLockEntry in this)
			{
				tableLockEntry.RemoveEntriesForContext(context);
			}
		}

		//-----------------------------------------------------------------------
		public void RemoveEntriesForContext(string tableName, string context)
		{
			TableLockEntry tableLockEntry = GetTableLockEntry(tableName);
			if (tableLockEntry != null)
				tableLockEntry.RemoveEntriesForContext(context);
		}

	}

}
