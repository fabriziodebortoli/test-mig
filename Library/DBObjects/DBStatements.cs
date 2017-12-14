using System;
using System.Collections.Generic;
using System.Text;
using System.Collections;
using System.Globalization;

using Microarea.TaskBuilderNet.Interfaces;
using Microarea.Library.TBWizardProjects;
using Microarea.Library.SqlScriptUtility;

namespace Microarea.Library.DBObjects
{
	///<summary>
	/// Enumerativo per identificare il tipo di operazione che viene eseguita nello statement
	///</summary>
	//=================================================================================
	public enum DBStatementType
	{
		TABLE,
		VIEW,
		PROCEDURE,
		EXTRAADDEDCOLUMN,
		TABLEUPDATE,
		TBAFTERSCRIPT, 
		UPGRADE //x gestire lo scatto di release
	}

	///<summary>
	/// DBObjectStatement
	/// Classe di memorizzazione delle informazioni di ogni statement da eseguire, comprensiva di:
	/// - applicazione di appartenenza dell'oggetto
	/// - modulo di appartenenza dell'oggetto
	/// - release che sta eseguendo lo statement (non é = a quella del modulo)
	/// - nome dell'oggetto di database coinvolto nello statement, 
	/// - tipo dell'operazione che si sta effettuando, 
	/// - statement da eseguire
	///</summary>
	//=================================================================================
	public class DBObjectStatement
	{
		private string objectName = string.Empty;	// nome oggetto di db (tabella, view, procedure)
		private string columnName = string.Empty;	// nome colonna (serve per le ExtraAddColumn)
		private string statement = string.Empty;	// statement sql generato dinamicamente

		private DBStatementType operationType = DBStatementType.TABLE; // tipo operazione da eseguire
		
		private uint release = 0;	// release che sta eseguendo lo statement (non é = a quella del modulo)
		private uint step = 0;		// nr di step (serve per i TBAfterScript/Upgrade)

		// struttura che contiene le info di applicazione + modulo al quale appartiene lo statement corrente
		private DBObjectExtendedInfo dbObjExtendedInfo; 

		// Properties
		//---------------------------------------------------------------------
		public string ObjectName { get { return objectName; } }
		public string ColumnName { get { return columnName; } }
		public DBStatementType OperationType { get { return operationType; } }
		public string Statement { get { return statement; } }
		public uint Release { get { return release; } set { release = value; } }
		public uint Step { get { return step; } set { step = value; } }

		public DBObjectExtendedInfo ExtendendInfo { get { return dbObjExtendedInfo; } set { dbObjExtendedInfo = value; } }

		///<summary>
		/// costruttore
		///</summary>
		//---------------------------------------------------------------------
		public DBObjectStatement
			(
			string objectName, 
			DBStatementType type, 
			string statement, 
			uint release,
			DBObjectExtendedInfo extendedInfo
			)
		{
			this.objectName = objectName;
			this.operationType = type;
			this.statement = statement;
			this.release = release;
			this.dbObjExtendedInfo = extendedInfo;
		}

		///<summary>
		/// costruttore
		///</summary>
		//---------------------------------------------------------------------
		public DBObjectStatement
			(
			string objectName,
			string columnName,
			DBStatementType type,
			string statement,
			uint release,
			DBObjectExtendedInfo extendedInfo
			)
		{
			this.objectName = objectName;
			this.columnName = columnName;
			this.operationType = type;
			this.statement = statement;
			this.release = release;
			this.dbObjExtendedInfo = extendedInfo;
		}
	}

	///<summary>
	/// DBObjectExtendedInfo: classe che contiene le informazioni relative al modulo + 
	/// applicazione + release di appartenenza
	///</summary>
	//=================================================================================
	public class DBObjectExtendedInfo
	{
		private string	application;				// nome applicazione (file system)
		private string	applicationSignature;		// db signature applicazione (dall'application.config)
		private string	module;						// nome modulo (file system)
		private string	moduleTitle;				// title modulo (nome localizzato)
		private string	moduleSignature;			// db signature modulo (dal module.config)
		private int		moduleRelease = 0;			// db release modulo (dal module.config)
		private string	dbxmlPath;					// path del file dbxml analizzato

		// Properties
		//---------------------------------------------------------------------
		public string Application { get { return application; } set { application = value; } }
		public string ApplicationSignature { get { return applicationSignature; } set { applicationSignature = value; } }
		public string Module { get { return module; } set { module = value; } }
		public string ModuleTitle { get { return moduleTitle; } set { moduleTitle = value; } }
		public string ModuleSignature { get { return moduleSignature; } set { moduleSignature = value; } }
		public int ModuleRelease { get { return moduleRelease; } set { moduleRelease = value; } }
		public string DbxmlPath { get { return dbxmlPath; } set { dbxmlPath = value; } }

		//---------------------------------------------------------------------
		public DBObjectExtendedInfo(IBaseModuleInfo moduleInfo)
		{
			this.application = moduleInfo.ParentApplicationName;
			this.applicationSignature = moduleInfo.ParentApplicationInfo.ApplicationConfigInfo.DbSignature;
			this.module = moduleInfo.Name;
			this.moduleTitle = moduleInfo.ModuleConfigInfo.Title;
			this.moduleSignature = moduleInfo.ModuleConfigInfo.Signature;
			this.moduleRelease = moduleInfo.ModuleConfigInfo.Release;
		}
	}

	///<summary>
	/// ExtendedWizardTableInfo: classe che estende l'attuale WizardTableInfo aggiungendo le
	/// informazioni relative al modulo + applicazione + release di appartenenza
	/// [necessarie per tenere traccia dello statement che stiamo eseguendo]
	///</summary>
	//=================================================================================
	public class ExtendedWizardTableInfo
	{
		private WizardTableInfo wizTableInfo;
		private DBObjectExtendedInfo dbObjExtendedInfo;

		// Properties
		//---------------------------------------------------------------------
		public WizardTableInfo WizardTableInfo { get { return wizTableInfo; } set { wizTableInfo = value; } }
		public DBObjectExtendedInfo ExtendendInfo { get { return dbObjExtendedInfo; } set { dbObjExtendedInfo = value; } }

		//---------------------------------------------------------------------
		public ExtendedWizardTableInfo(WizardTableInfo wizTableInfo, IBaseModuleInfo moduleInfo, string dbxmlPath)
		{
			this.wizTableInfo = wizTableInfo;
			dbObjExtendedInfo = new DBObjectExtendedInfo(moduleInfo);
			dbObjExtendedInfo.DbxmlPath = dbxmlPath;
		}

        //---------------------------------------------------------------------
        // Operatore di conversione esplicita da ExtendedWizardTableInfo a WizardTableInfo.
        public static explicit operator WizardTableInfo(ExtendedWizardTableInfo info)
        {
            return (info != null) ? info.WizardTableInfo : null;
        }
    }

	///<summary>
	/// ExtendedSqlView: classe che estende l'attuale SqlView aggiungendo le
	/// informazioni relative al modulo + applicazione + release di appartenenza
	/// [necessarie per tenere traccia dello statement che stiamo eseguendo]
	///</summary>
	//=================================================================================
	public class ExtendedSqlView
	{
		private SqlView sqlView;
		private DBObjectExtendedInfo dbObjExtendedInfo;

		// Properties
		//---------------------------------------------------------------------
		public SqlView SqlView { get { return sqlView; } set { sqlView = value; } }
		public DBObjectExtendedInfo ExtendendInfo { get { return dbObjExtendedInfo; } set { dbObjExtendedInfo = value; } }

		//---------------------------------------------------------------------
		public ExtendedSqlView(SqlView sqlView, IBaseModuleInfo moduleInfo, string dbxmlPath)
		{
			this.sqlView = sqlView;
			dbObjExtendedInfo = new DBObjectExtendedInfo(moduleInfo);
			dbObjExtendedInfo.DbxmlPath = dbxmlPath;
		}

        //---------------------------------------------------------------------
        // Operatore di conversione esplicita da ExtendedSqlView a SqlView.
        public static explicit operator SqlView(ExtendedSqlView info)
        {
            return (info != null) ? info.SqlView : null;
        }
    }

	///<summary>
	/// ExtendedSqlProcedure: classe che estende l'attuale SqlProcedure aggiungendo le
	/// informazioni relative al modulo + applicazione + release di appartenenza
	/// [necessarie per tenere traccia dello statement che stiamo eseguendo]
	///</summary>
	//=================================================================================
	public class ExtendedSqlProcedure
	{
		private SqlProcedure sqlProcedure;
		private DBObjectExtendedInfo dbObjExtendedInfo;

		// Properties
		//---------------------------------------------------------------------
		public SqlProcedure SqlProcedure { get { return sqlProcedure; } set { sqlProcedure = value; } }
		public DBObjectExtendedInfo ExtendendInfo { get { return dbObjExtendedInfo; } set { dbObjExtendedInfo = value; } }

		//---------------------------------------------------------------------
		public ExtendedSqlProcedure(SqlProcedure sqlProcedure, IBaseModuleInfo moduleInfo, string dbxmlPath)
		{
			this.sqlProcedure = sqlProcedure;
			dbObjExtendedInfo = new DBObjectExtendedInfo(moduleInfo);
			dbObjExtendedInfo.DbxmlPath = dbxmlPath;
		}
	}

	///<summary>
	/// ExtendedWizardExtraAddedColumnsInfo: classe che estende l'attuale WizardExtraAddedColumnsInfo aggiungendo le
	/// informazioni relative al modulo + applicazione + release di appartenenza
	/// [necessarie per tenere traccia dello statement che stiamo eseguendo]
	///</summary>
	//=================================================================================
	public class ExtendedWizardExtraAddedColumnsInfo
	{
		private WizardExtraAddedColumnsInfo extraAddedColumnsInfo;
		private DBObjectExtendedInfo dbObjExtendedInfo;

		// Properties
		//---------------------------------------------------------------------
		public WizardExtraAddedColumnsInfo ExtraAddedColumnsInfo { get { return extraAddedColumnsInfo; } set { extraAddedColumnsInfo = value; } }
		public DBObjectExtendedInfo ExtendendInfo { get { return dbObjExtendedInfo; } set { dbObjExtendedInfo = value; } }

		public string ColumnName { get { return extraAddedColumnsInfo.ColumnAtZeroIndex.Name; } }
		//---------------------------------------------------------------------
		public ExtendedWizardExtraAddedColumnsInfo
			(
			WizardExtraAddedColumnsInfo extraAddedColumnsInfo, 
			IBaseModuleInfo moduleInfo,
			string dbxmlPath
			)
		{
			this.extraAddedColumnsInfo = extraAddedColumnsInfo;
			dbObjExtendedInfo = new DBObjectExtendedInfo(moduleInfo);
			dbObjExtendedInfo.DbxmlPath = dbxmlPath;
		}
	}

	///<summary>
	/// ExtendedTableUpdate: classe che estende l'attuale TableUpdate aggiungendo le
	/// informazioni relative al modulo + applicazione + release di appartenenza
	/// [necessarie per tenere traccia dello statement che stiamo eseguendo]
	///</summary>
	//=================================================================================
	public class ExtendedTableUpdate
	{
		private TableUpdate tableUpdate;
		private DBObjectExtendedInfo dbObjExtendedInfo;

		// Properties
		//---------------------------------------------------------------------
		public TableUpdate TableUpdate { get { return tableUpdate; } set { tableUpdate = value; } }
		public DBObjectExtendedInfo ExtendendInfo { get { return dbObjExtendedInfo; } set { dbObjExtendedInfo = value; } }

		//---------------------------------------------------------------------
		public ExtendedTableUpdate(TableUpdate tableUpdate, IBaseModuleInfo moduleInfo, string dbxmlPath)
		{
			this.tableUpdate = tableUpdate;
			dbObjExtendedInfo = new DBObjectExtendedInfo(moduleInfo);
			dbObjExtendedInfo.DbxmlPath = dbxmlPath;
		}
	}

	///<summary>
	/// ExtendedTBAfterScript: classe che estende l'attuale TBAfterScript aggiungendo le
	/// informazioni relative al modulo + applicazione + release di appartenenza
	/// [necessarie per tenere traccia dello statement che stiamo eseguendo]
	///</summary>
	//=================================================================================
	public class ExtendedTBAfterScript
	{
		private TBAfterScript tbAfterScript;
		private DBObjectExtendedInfo dbObjExtendedInfo;

		// Properties
		//---------------------------------------------------------------------
		public TBAfterScript TBAfterScript { get { return tbAfterScript; } set { tbAfterScript = value; } }
		public DBObjectExtendedInfo ExtendendInfo { get { return dbObjExtendedInfo; } set { dbObjExtendedInfo = value; } }

		//---------------------------------------------------------------------
		public ExtendedTBAfterScript(TBAfterScript tbAfterScript, IBaseModuleInfo moduleInfo, string dbxmlPath)
		{
			this.tbAfterScript = tbAfterScript;
			dbObjExtendedInfo = new DBObjectExtendedInfo(moduleInfo);
			dbObjExtendedInfo.DbxmlPath = dbxmlPath;
		}
	}

	// per ordinare i TBAfterScript per numero di step crescente
	//============================================================================
	public class SortExtendedTBAfterScriptList : IComparer
	{
		//---------------------------------------------------------------------------
		int IComparer.Compare(Object tBAfterScript1, Object tBAfterScript2)
		{
			return (new CaseInsensitiveComparer(CultureInfo.InvariantCulture)).Compare
				(
				((ExtendedTBAfterScript)tBAfterScript1).TBAfterScript.Step,
				((ExtendedTBAfterScript)tBAfterScript2).TBAfterScript.Step
				);
		}
	}
}