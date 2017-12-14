using System;
using System.Collections.Generic;
using System.Text;

using Microarea.TaskBuilderNet.Interfaces;
using Microarea.TaskBuilderNet.Core.NameSolver;

namespace Microarea.Library.DBObjects 
{
	/// <summary>
	/// Classe che raggruppa le informazioni di applicazione necessarie per la gestione
	/// del database
	/// </summary>
	//=====================================================================
	public class DBApplicationInfo
	{
		private string name = string.Empty;			// nome applicazione (nome fisico del folder su filesystem)
		private string brandTitle = string.Empty;	// nome applicazione estrapolato dalle info di Brand
		private string dbSignature = string.Empty;	// signature di database

		private IList<DBModuleInfo> modules = new List<DBModuleInfo>();	// lista di moduli appartenenti all'applicazione

		public bool AllTablesAbsent = true;
		public bool AllTablesPresent = true;

		//---------------------------------------------------------------------------
		public string Name { get { return name; } }
		public string BrandTitle { get { return brandTitle; } }
		public string DbSignature { get { return dbSignature; } set { dbSignature = value; } }

		public IList<DBModuleInfo> Modules { get { return modules; } }

		//---------------------------------------------------------------------------
		public DBApplicationInfo(string name)
		{
			this.name = name;
		}

		//---------------------------------------------------------------------------
		public DBApplicationInfo(string name, string title)
		{
			this.name = name;
			this.brandTitle = title;
		}
	}

	/// <summary>
	/// Classe che raggruppa le informazioni di modulo necessarie per la gestione
	/// del database
	/// </summary>
	//=====================================================================
	public class DBModuleInfo
	{
		private string name = string.Empty;						// nome modulo (nome fisico del folder su filesystem)
		private string brandTitle = string.Empty;				// nome modulo estrapolato dalle info di Brand
		private string dbSignature = string.Empty;				// signature di database del modulo
		private int dbRelease = 0;

		private DBApplicationInfo applicationInfo = null;
		
		private IList<DBEntryInfo> tables = null;		// lista di tabelle appartenenti al modulo
		private IList<DBEntryInfo> views = null;		// lista di view appartenenti al modulo
		private IList<DBEntryInfo> procedures = null;	// lista di procedure appartenenti al modulo

		public int NumExistTables = 0;

		//---------------------------------------------------------------------------
		public string Name { get { return name; } }
		public string BrandTitle { get { return brandTitle; } }
		public string DBSignature { get { return dbSignature; } set { dbSignature = value; } }
		public int DBRelease { get { return dbRelease; } set { dbRelease = value; } }

		public IList<DBEntryInfo> Tables { get { return tables; } }
		public IList<DBEntryInfo> Views { get { return views; } }
		public IList<DBEntryInfo> Procedures { get { return procedures; } }

		//---------------------------------------------------------------------------
		public DBModuleInfo(string name, DBApplicationInfo applicationInfo)
		{
			this.name = name;
			this.applicationInfo = applicationInfo;
		}

		//---------------------------------------------------------------------------
		public DBModuleInfo(string name, string title, DBApplicationInfo applicationInfo)
		{
			this.name = name;
			this.brandTitle = title;
			this.applicationInfo = applicationInfo;
		}

		/// <summary>
		/// Funzione che analizza la struttura delle informazioni caricate dai file di struttura dati versione 3.0
		/// (.dbxml) e riempimento della mia struttura specifica
		/// </summary>
		//---------------------------------------------------------------------------
		public bool LoadDBObjectsInfo(IDBObjects dbObjects)
		{
			if (dbObjects == null)
				return false;

			DBEntryInfo entryDBInfo = null;

			if (dbObjects.TableInfoList != null)
			{
				foreach (ITableInfo table in dbObjects.TableInfoList)
				{
					entryDBInfo = new DBEntryInfo(table.Name, table.Release);
					Tables.Add(entryDBInfo);
				}
			}

			if (dbObjects.ViewInfoList != null)
			{
				foreach (IDbObjectInfo view in dbObjects.ViewInfoList)
				{
					entryDBInfo = new DBEntryInfo(view.Name, view.Release);
					Views.Add(entryDBInfo);
				}
			}

			if (dbObjects.ProcedureInfoList != null)
			{
				foreach (IDbObjectInfo proc in dbObjects.ProcedureInfoList)
				{
					entryDBInfo = new DBEntryInfo(proc.Name, proc.Release);
					Procedures.Add(entryDBInfo);
				}
			}

			return true;
		}
	}

	/// <summary>
	/// Classe che raggruppa le informazioni di singolo oggetto di database
	/// </summary>
	//=====================================================================
	public class DBEntryInfo
	{
		private string name = string.Empty;		// nome oggetto (nome fisico del folder su filesystem)
		private int createDbRelease = 0;		// release di creazione oggetto
		private bool exist = false;
		
		public IList<IAddOnDbObjectInfo> addColumnsList = new List<IAddOnDbObjectInfo>();
		
		//---------------------------------------------------------------------------
		public string Name { get { return name; } }
		public int CreateDBRelease { get { return createDbRelease; } }
		public bool Exist { get { return exist; } set { exist = value; } }
		public IList<IAddOnDbObjectInfo> AddColumnsList { get { return addColumnsList; } set { addColumnsList = value; } }

		//---------------------------------------------------------------------------
		public DBEntryInfo(string name, int release)
		{
			this.name = name;
			this.createDbRelease = release;
		}
	}
}
