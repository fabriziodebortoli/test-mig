using System;
using System.Collections;
using System.IO;

using Microarea.TaskBuilderNet.Core.NameSolver;
using Microarea.Library.TranslationManager;
using Microarea.TaskBuilderNet.Interfaces;
using Microarea.TaskBuilderNet.Core.Generic;

namespace Microarea.Library.TranslationManager
{
	#region Enum

	public enum TaskTypeEnum 
	{
		Undefined			= 0x00000000,		
		Batch				= 0x00000001,				
		Report				= 0x00000002,
		Function			= 0x00000003,
		Executable			= 0x00000004,
		Message				= 0x00000005,						
		Mail				= 0x00000006,
		DataExport			= 0x00000007,
		DataImport			= 0x00000008,
		WebPage				= 0x00000009,
		Sequence			= 0x0000000A,
		DelRunnedReports	= 0x0000000B,
		BackupCompanyDB		= 0x0000000C
	};

	public enum SecurityTypeEnum 
	{	
		Function			= 3,				
		Report				= 4,
		DataEntry			= 5,
		Batch				= 7,
		Table				= 10,
		HotLink				= 11,
		View				= 13,
		Radar				= 21
	};

	#endregion

	/// <summary>
	/// 
	/// </summary>
	public class SqlTranslator: BaseTranslator
	{

		
		#region Data Menber

		private TranslationManager	tm;

		private Hashtable	documentHashtable			= null;
		private Hashtable	reportHashtable				= null;
		private Hashtable	referenceObjectsHashtable	= null;
		private Hashtable	tablesHashtable				= null;
		private Hashtable	webMethodsHashtable			= null;
		private string		securityScriptPath			= string.Empty;
		private string		schedulerScriptPath			= string.Empty;
		private string		otherScriptPath				= string.Empty;

		#endregion

		#region Properties
		public  TranslationManager	TranslationManager { get { return tm; }}
		#endregion

		#region Costructor
		public SqlTranslator()
		{
			
		}
		#endregion

		#region Run Method
		//---------------------------------------------------------------------
		public override void Run(TranslationManager	tm)
		{
			this.tm = tm;
			IBaseApplicationInfo app = tm.GetApplicationInfo();
			IBaseModuleInfo mod = app.PathFinder.GetModuleInfoByName("MicroareaConsole", "SecurityAdmin");
			
			if (mod == null)
				return;
				
			securityScriptPath = GetAllUpgradePath(mod) + 
								Path.DirectorySeparatorChar	+
								"Release_6" +
								Path.DirectorySeparatorChar	;

			if (!Directory.Exists(securityScriptPath))
				Directory.CreateDirectory(securityScriptPath);

			mod = app.PathFinder.GetModuleInfoByName("MicroareaConsole", "TaskScheduler");

			if (mod == null)
				return;

			schedulerScriptPath = GetAllUpgradePath(mod) + 
									Path.DirectorySeparatorChar	+
									"Release_4" +
									Path.DirectorySeparatorChar	;
	
			if (!Directory.Exists(schedulerScriptPath))
				Directory.CreateDirectory(schedulerScriptPath);

			otherScriptPath = mod.PathFinder.GetStandardApplicationContainerPath(ApplicationType.TaskBuilderNet);
			
			otherScriptPath += Path.DirectorySeparatorChar	+ 
								"Tools" +
								Path.DirectorySeparatorChar	+ 
								"MNTranslator" + 
								Path.DirectorySeparatorChar	+
								"OtherScript" + 
								Path.DirectorySeparatorChar;

			if (!Directory.Exists(otherScriptPath))
				Directory.CreateDirectory(otherScriptPath);

			CreateUpdateScripts();
			EndRun(false);

		}
		#endregion
		
		//---------------------------------------------------------------------
		private string GetAllUpgradePath(IBaseModuleInfo mod)
		{
			return mod.GetStandardDatabaseScriptPath()	+ 
					Path.DirectorySeparatorChar							+ 
					NameSolverStrings.UpgradeScript						+
					Path.DirectorySeparatorChar							+
					NameSolverStrings.All								;

		}

		//---------------------------------------------------------------------
		/// <summary>
		/// Funzione principale che  chiama tutte le funzioni  che  creano  gli
		/// script di UpGrade sia per la Security che per lo Scheduler
		/// </summary>
		private void CreateUpdateScripts()
		{
			SetProgressMessage("Inizio lettura NameSpace");
			LoadNameSpace();
			SetProgressMessage("Inizio creazione script per il SecurityAdminPlugIn");
			CreateSecurityScript();
			SetProgressMessage("Fine creazione script per il SecurityAdminPlugIn");
			SetProgressMessage("Inizio creazione script di upgrade per il TaskSchedulerPlugIn");
			CreateScheduledTasksUpdateScript();
			SetProgressMessage("Fine creazione script di upgrade per il TaskSchedulerPlugIn");
			SetProgressMessage("Inizio creazione script di upgrade per XTech");
			CreateXTechUpdateScript();
			SetProgressMessage("Fine creazione script di upgrade per XTech");
	
		}
		

		//---------------------------------------------------------------------
		/// <summary>
		/// Funzione che legge l'insieme dei namespace 
		/// </summary>
		private void LoadNameSpace()
		{
			documentHashtable			= tm.GetDocumentNamespaceList();
			reportHashtable				= tm.GetReportNamespaceList();
			referenceObjectsHashtable	= tm.GetReferenceObjectsNamespaceList();
			tablesHashtable				= tm.GetTablesNamespaceList();
			webMethodsHashtable			= tm.GetWebMethodsNamespaceList();
		}


		//---------------------------------------------------------------------
		/// <summary>
		/// Funzione che chiama le funzioni per la creazione degli script  per
		/// gli update della security
		/// </summary>
		private void CreateSecurityScript()
		{
			SetProgressMessage("Inizio creazione script di upgrade SecurityDocumentsUpdateScript.sql . Oggetti di tipo Document nella tabella MSD_Objects");
			CreateSecurityDocumentsUpdateScript();
			SetProgressMessage("File creazione script di upgrade SecurityDocumentsUpdateScript.sql");
			
			SetProgressMessage("Inizio creazione script di upgrade ReferenceObjectsUpdateScript.sql . Oggetti di tipo HotLink nella tabella MSD_Objects");
			CreateReferenceObjectsUpdateScript();
			SetProgressMessage("Fine creazione script di upgrade ReferenceObjectsUpdateScript.sql");
			
			SetProgressMessage("Inizio creazione script di upgrade TablesUpdateScript.sql . Oggetti di tipo Table nella tabella MSD_Objects");
			CreateTableAndViewUpdateScript();
			SetProgressMessage("Fine creazione script di upgrade TablesUpdateScript.sql");

			SetProgressMessage("Inizio creazione script di upgrade SecurityWebMethodUpdateScript.sql . Oggetti di tipo Function nella tabella MSD_Objects");
			CreateWebMethodsUpdateScript(true);
			SetProgressMessage("Fine creazione script di upgrade SecurityWebMethodUpdateScript.sql");

			SetProgressMessage("Inizio creazione script di upgrade SecurityReportsUpdateScript.sql . Oggetti di tipo Report  nella tabella MSD_Objects");
			CreateReportsUpdateScript(true);
			SetProgressMessage("Fine creazione script di upgrade SecurityReportsUpdateScript.sql");	
		}

		//---------------------------------------------------------------------
		/// <summary>
		/// Funzione che chiama le funzioni per la creazione degli script  per
		/// gli update dello Scheduler
		/// </summary>
		private void CreateScheduledTasksUpdateScript()
		{
			SetProgressMessage("Inizio creazione script di upgrade ScheduledDocumentsUpdateScript.sql . Oggetti di tipo Document nella tabella MSD_ScheduledTasks");
			CreateScheduledDocumentsUpdateScript();		
			SetProgressMessage("Fine creazione script di upgrade ScheduledDocumentsUpdateScript.sql");
			
			SetProgressMessage("Inizio creazione script di upgrade SchedulerWebMethodUpdateScript.sql . Oggetti di tipo Function nella tabella MSD_ScheduledTasks");
			CreateWebMethodsUpdateScript(false);
			SetProgressMessage("Fine creazione script di upgrade SchedulerWebMethodUpdateScript.sql");
			
			SetProgressMessage("Inizio creazione script di upgrade SchedulerReportsUpdateScript.sql . Oggetti di tipo Report nella tabella MSD_ScheduledTasks");
			CreateReportsUpdateScript(false);
			SetProgressMessage("Fine creazione script di upgrade SchedulerReportsUpdateScript.sql");
			
		}

		//---------------------------------------------------------------------
		private void CreateXTechUpdateScript()
		{
			SetProgressMessage("Inizio creazione script di upgrade XTech_PostMigration.sql . Oggetti di tipo Document");
			CreateXTech_PostMigrationScript();		
			SetProgressMessage("Fine creazione script di upgrade XTech_PostMigration.sql");

			SetProgressMessage("Inizio creazione script di upgrade AUDITING_PostMigration.sql . Oggetti di tipo Document");
			CreateAUDITING_PostMigrationScript();		
			SetProgressMessage("Fine creazione script di upgrade AUDITING_PostMigration.sql");
		}

		//---------------------------------------------------------------------
		private void CreateXTech_PostMigrationScript()
		{
			
			if (documentHashtable != null && documentHashtable.Count >0)
			{
				
				FileInfo xTechScriptFile = new FileInfo(otherScriptPath + tm.GetApplicationInfo().Name + "_XTech_PostMigrationScript.sql");
				
				using (StreamWriter sw = xTechScriptFile.CreateText())
				{
					sw.Write(SqlTranslatorStrings.startXTechKeyExtensionScriptString);
					
					foreach(string obj in documentHashtable.Keys)
					{
						sw.Write(SqlTranslatorStrings.begin);
						sw.Write(SqlTranslatorStrings.update + SqlTranslatorStrings.keyExtensionTableName);
						sw.Write(SqlTranslatorStrings.setKey + SqlTranslatorStrings.documentNameSpaceColumn +  "'" + documentHashtable[obj].ToString() + "'" + "\r\n");
						sw.Write(SqlTranslatorStrings.where  + SqlTranslatorStrings.documentNameSpaceColumn +  "'" + obj  + "')"  + "\r\n");
						sw.Write(SqlTranslatorStrings.end   + "\r\n");
						sw.Write(SqlTranslatorStrings.go + "\r\n");
					} 

					sw.Write(SqlTranslatorStrings.startXTechXE_LostAndFoundScriptString);

					foreach(string obj in documentHashtable.Keys)
					{
						sw.Write(SqlTranslatorStrings.begin);
						sw.Write(SqlTranslatorStrings.update + SqlTranslatorStrings.lostAndFoundTableName);
						sw.Write(SqlTranslatorStrings.setKey + SqlTranslatorStrings.documentNameSpaceColumn +  "'" + documentHashtable[obj].ToString() + "'" + "\r\n");
						sw.Write(SqlTranslatorStrings.where  + SqlTranslatorStrings.documentNameSpaceColumn +  "'" + obj  + "')" + "\r\n");
						sw.Write(SqlTranslatorStrings.end   + "\r\n");
						sw.Write(SqlTranslatorStrings.go + "\r\n");
					} 

				}    
			
			}
		}
		
		//---------------------------------------------------------------------
		private void CreateAUDITING_PostMigrationScript()
		{
			CreateAUDITING_PostMigrationDocumentScript();
			CreateAUDITING_PostMigrationReportScript();

		}

		//---------------------------------------------------------------------
		private void CreateAUDITING_PostMigrationDocumentScript()
		{
			if (documentHashtable != null && documentHashtable.Count >0)
			{
				
				FileInfo auditingScriptFile = new FileInfo(otherScriptPath + tm.GetApplicationInfo().Name + "_AUDITING_PostMigrationScriptDocument.sql");
				
				using (StreamWriter sw = auditingScriptFile.CreateText())
				{
					sw.Write(SqlTranslatorStrings.startAudit_NamescpacesScriptString);
					
					foreach(string obj in documentHashtable.Keys)
					{
						sw.Write(SqlTranslatorStrings.begin);
						sw.Write(SqlTranslatorStrings.update + SqlTranslatorStrings.namespacesTableName);
						sw.Write(SqlTranslatorStrings.setKey + SqlTranslatorStrings.namespaceColumn +  "'" + documentHashtable[obj].ToString() + "'" + "\r\n");
						sw.Write(SqlTranslatorStrings.where  + SqlTranslatorStrings.namespaceColumn +  "'" + obj  + "')"  + "\r\n");
						sw.Write(SqlTranslatorStrings.end   + "\r\n");
						sw.Write(SqlTranslatorStrings.go + "\r\n");
					} 
				}    
			}
		}

		//---------------------------------------------------------------------
		private void CreateAUDITING_PostMigrationReportScript()
		{
			if (reportHashtable != null && reportHashtable.Count >0)
			{
				
				FileInfo auditingScriptFile = new FileInfo(otherScriptPath + tm.GetApplicationInfo().Name + "_AUDITING_PostMigrationScriptReport.sql");
				
				using (StreamWriter sw = auditingScriptFile.CreateText())
				{
					sw.Write(SqlTranslatorStrings.startAudit_NamescpacesScriptString);
					
					foreach(string obj in reportHashtable.Keys)
					{
						sw.Write(SqlTranslatorStrings.begin);
						sw.Write(SqlTranslatorStrings.update + SqlTranslatorStrings.namespacesTableName);
						sw.Write(SqlTranslatorStrings.setKey + SqlTranslatorStrings.namespaceColumn +  "'" + reportHashtable[obj].ToString() + "'" + "\r\n");
						sw.Write(SqlTranslatorStrings.where  + SqlTranslatorStrings.namespaceColumn +  "'" + obj  + "')"  + "\r\n");
						sw.Write(SqlTranslatorStrings.end   + "\r\n");
						sw.Write(SqlTranslatorStrings.go + "\r\n");
					} 
				}    
			}
		}
		//---------------------------------------------------------------------
		/// <summary>
		/// Funzione che crea lo script di upgrade "SecurityDocumentsUpdateScript.sql"
		/// aggiorna i  namespace degli oggetti  di tipo document  all'interno 
		/// della tabella MSD_Objects
		/// </summary>
		private void CreateSecurityDocumentsUpdateScript()
		{
			
			if (documentHashtable != null && documentHashtable.Count >0)
			{
				
				FileInfo securityUpdateScriptFile = new FileInfo(securityScriptPath + tm.GetApplicationInfo().Name + "_SecurityDocumentsUpdateScript.sql");
				
				using (StreamWriter sw = securityUpdateScriptFile.CreateText())
				{
					sw.Write(SqlTranslatorStrings.startSecurityScriptString);
					sw.Write(SqlTranslatorStrings.declarationDocumentUpdateScript);
					foreach(string obj in documentHashtable.Keys)
					{
						sw.Write(SqlTranslatorStrings.begin);
						sw.Write(SqlTranslatorStrings.update + SqlTranslatorStrings.objectsTableName);
						sw.Write(SqlTranslatorStrings.setKey + SqlTranslatorStrings.namespaceColumn +  "'" + documentHashtable[obj].ToString() + "'" + "\r\n");
						sw.Write(SqlTranslatorStrings.where  + SqlTranslatorStrings.namespaceColumn +  "'" + obj  + "'" + SqlTranslatorStrings.and + "\r\n");
						sw.Write(SqlTranslatorStrings.roundnBracketOpen + "\r\n"); 
						sw.Write(SqlTranslatorStrings.typeIdColumn + SqlTranslatorStrings.documentTypeId +  SqlTranslatorStrings.or + "\r\n");
						sw.Write(SqlTranslatorStrings.typeIdColumn + SqlTranslatorStrings.finderTypeId   +  SqlTranslatorStrings.or + "\r\n");
						sw.Write(SqlTranslatorStrings.typeIdColumn + SqlTranslatorStrings.batchTypeId    +  "\r\n");
						sw.Write(SqlTranslatorStrings.roundnBracketClose +  SqlTranslatorStrings.roundnBracketClose + "\r\n");
						sw.Write(SqlTranslatorStrings.end   + "\r\n");
					} 
					sw.Write(SqlTranslatorStrings.go);
				}    
			
			}
		}


		//---------------------------------------------------------------------
		/// <summary>
		/// Funzione che crea lo script di upgrade "ScheduledDocumentsUpdateScript.sql"
		/// aggiorna  i  namespace degli  oggetti  di  tipo "Importa Dati"  ed 
		/// "Esporta Dati"  all'interno della tabella MSD_ScheduledTasks
		/// </summary>
		private void CreateScheduledDocumentsUpdateScript()
		{
			if (documentHashtable != null && documentHashtable.Count >0)
			{
				FileInfo updateScriptFile = new FileInfo(schedulerScriptPath + tm.GetApplicationInfo().Name + "_ScheduledDocumentsUpdateScript.sql");
				
				using (StreamWriter sw = updateScriptFile.CreateText())
				{
					sw.Write(SqlTranslatorStrings.startSchedulerScriptString);
					foreach(string obj in documentHashtable.Keys)
					{
						sw.Write(SqlTranslatorStrings.begin);
						sw.Write(SqlTranslatorStrings.update + SqlTranslatorStrings.taskTableName);
						sw.Write(SqlTranslatorStrings.setKey + SqlTranslatorStrings.commandColumn +  "'" + documentHashtable[obj].ToString() + "'" + "\r\n");
						sw.Write(SqlTranslatorStrings.where  + SqlTranslatorStrings.commandColumn +  "'" + obj  + "'" + SqlTranslatorStrings.and + "\r\n");
						sw.Write(SqlTranslatorStrings.roundnBracketOpen + "\r\n"); 
						sw.Write(SqlTranslatorStrings.typeColumn+ (int)TaskTypeEnum.Batch +  SqlTranslatorStrings.or + "\r\n");
						sw.Write(SqlTranslatorStrings.typeColumn + (int)TaskTypeEnum.DataExport   +  SqlTranslatorStrings.or + "\r\n");
						sw.Write(SqlTranslatorStrings.typeColumn + (int)TaskTypeEnum.DataImport    +  "\r\n");
						sw.Write(SqlTranslatorStrings.roundnBracketClose +  SqlTranslatorStrings.roundnBracketClose + "\r\n");
						sw.Write(SqlTranslatorStrings.end   + "\r\n");
					} 
					sw.Write(SqlTranslatorStrings.go);
				}    
			
			}
		}


		//---------------------------------------------------------------------
		/// <summary>
		/// Funzione che, a seconda della volorizzazione dei suoi paramentri di
		/// imput crea lo script di UpGrade o della tabella MSD_Objects (per il
		/// SecurityAdminPlugIn) o della MSD_ScheduledTasks (TaskSchedulerAdminPlugIn)
		/// </summary>
		/// <param name="hashtable"></param>
		/// <param name="type"></param>
		/// <param name="fileName"></param>
		/// <param name="isSecurity"></param>
		private void CreateUpdateScript(Hashtable hashtable, string type, string fileName, bool isSecurity)
		{
			string tableName	= GetTableToUpdate(isSecurity);
			string upDateColumn	= GetUpDateColumn(isSecurity);
			string typeColumn	= GetTypeColumn(isSecurity);

			FileInfo securityUpdateScriptFile = new FileInfo(fileName);
				
			using (StreamWriter sw = securityUpdateScriptFile.CreateText())
			{
				sw.Write(GetStartScript(isSecurity) + "\r\n");
				if (isSecurity)
					sw.Write(GetDeclarationStrig(type));

				foreach(string obj in hashtable.Keys)
				{
					sw.Write(SqlTranslatorStrings.begin);
					sw.Write(SqlTranslatorStrings.update + tableName);
					sw.Write(SqlTranslatorStrings.setKey + upDateColumn +  "'" + hashtable[obj].ToString() + "'" + "\r\n");
					sw.Write(SqlTranslatorStrings.where + upDateColumn + "'" + obj + "'" + SqlTranslatorStrings.and + "\r\n");
					sw.Write(typeColumn + type + "\r\n");
					sw.Write(SqlTranslatorStrings.roundnBracketClose + "\r\n");
					sw.Write(SqlTranslatorStrings.end   + "\r\n");
				} 
				sw.Write(SqlTranslatorStrings.go);
			}    
		}
		
		//---------------------------------------------------------------------
		private void CreateTableAndViewUpdateScript()
		{
			if (tablesHashtable  != null && tablesHashtable.Count >0)
			{
				
				FileInfo securityUpdateScriptFile = new FileInfo(securityScriptPath + tm.GetApplicationInfo().Name + "_SecurityTablesViewsUpdateScript.sql");
				
				using (StreamWriter sw = securityUpdateScriptFile.CreateText())
				{
					sw.Write(SqlTranslatorStrings.startSecurityScriptString);
					sw.Write(SqlTranslatorStrings.declarationTablesUpdateScript);
					foreach(string obj in tablesHashtable.Keys)
					{
						sw.Write(SqlTranslatorStrings.begin);
						sw.Write(SqlTranslatorStrings.update + SqlTranslatorStrings.objectsTableName);
						sw.Write(SqlTranslatorStrings.setKey + SqlTranslatorStrings.namespaceColumn +  "'" + tablesHashtable[obj].ToString() + "'" + "\r\n");
						sw.Write(SqlTranslatorStrings.where  + SqlTranslatorStrings.namespaceColumn +  "'" + obj  + "'" + SqlTranslatorStrings.and + "\r\n");
						sw.Write(SqlTranslatorStrings.roundnBracketOpen + "\r\n"); 
						sw.Write(SqlTranslatorStrings.typeIdColumn + SqlTranslatorStrings.tableTypeId +  SqlTranslatorStrings.or + "\r\n");
						sw.Write(SqlTranslatorStrings.typeIdColumn + SqlTranslatorStrings.viewTypeId  + "\r\n");
						sw.Write(SqlTranslatorStrings.roundnBracketClose +  SqlTranslatorStrings.roundnBracketClose + "\r\n");
						sw.Write(SqlTranslatorStrings.end   + "\r\n");
					} 
					sw.Write(SqlTranslatorStrings.go);
				}    
			
			}
		}

		//---------------------------------------------------------------------

		/// <summary>
		/// Funzione che richiama la  CreateUpdateScript e  crea lo  script per 
		/// gli oggetti di tipo report
		/// </summary>
		/// <param name="isSecurityScript"></param>
		private void CreateReportsUpdateScript(bool isSecurityScript)
		{
			if (reportHashtable != null && reportHashtable.Count >0)
			{
				string fileName = string.Empty;
				string typeId	= string.Empty;

				if(isSecurityScript)
				{
					fileName	= securityScriptPath + tm.GetApplicationInfo().Name + "_SecurityReportsUpdateScript.sql";
					typeId		= SqlTranslatorStrings.reportTypeId;
				}
				else
				{
					fileName	= schedulerScriptPath + tm.GetApplicationInfo().Name + "_SchedulerReportsUpdateScript.sql";
					typeId		= ((int)TaskTypeEnum.Report).ToString();
				}
				CreateUpdateScript(reportHashtable, typeId, fileName, isSecurityScript);
			}
		}


		//---------------------------------------------------------------------
		/// <summary>
		/// Funzione che richiama la  CreateUpdateScript e  crea lo  script per
		/// gli oggetti di tipo HotLink
		/// </summary>
		private void CreateReferenceObjectsUpdateScript()
		{
			if (referenceObjectsHashtable != null && referenceObjectsHashtable.Count >0)
			{
				string fileName = securityScriptPath + tm.GetApplicationInfo().Name + "_SecurityReferenceObjectsUpdateScript.sql";
				CreateUpdateScript(referenceObjectsHashtable, SqlTranslatorStrings.referenceObjectTypeId, fileName, true);
			}
		}


		//---------------------------------------------------------------------
		/// <summary>
		/// Funzione che richiama la  CreateUpdateScript e  crea lo  script per
		/// gli oggetti di tipo Funzione
		/// </summary>
		/// <param name="isSecurityScript"></param>
		private void CreateWebMethodsUpdateScript(bool isSecurityScript)
		{
			if (webMethodsHashtable != null && webMethodsHashtable.Count >0)
			{
				string fileName = string.Empty;
				string typeId	= string.Empty;

				if(isSecurityScript)
				{
					fileName	= securityScriptPath + tm.GetApplicationInfo().Name + "_SecurityWebMethodUpdateScript.sql";
					typeId		= SqlTranslatorStrings.webMethodTypeId;
				}
				else
				{
					fileName	= schedulerScriptPath + tm.GetApplicationInfo().Name + "_SchedulerWebMethodUpdateScript.sql";
					typeId		= ((int)TaskTypeEnum.Function).ToString();
				}

				CreateUpdateScript(webMethodsHashtable, typeId, fileName, true); 
			}
		}
		

		#region Funzioni che restituiscono stringhe per creare lo script

		//---------------------------------------------------------------------
		/// <summary>
		/// Restituisce la 1° stringa dello script a seconda che sia uno script 
		/// per il securityPlugIn o no
		/// </summary>
		/// <param name="isSecurity"></param>
		/// <returns></returns>
		private string GetStartScript(bool isSecurity)
		{
			if (isSecurity)
				return SqlTranslatorStrings.startSecurityScriptString;
			else
				return SqlTranslatorStrings.startSchedulerScriptString;
		}


		//---------------------------------------------------------------------
		/// <summary>
		/// Reatituisce il nome della tabella sulla quale girerà lo script 
		/// </summary>
		/// <param name="isSecurity"></param>
		/// <returns></returns>
		private string GetTableToUpdate(bool isSecurity)
		{
			if (isSecurity)
				return SqlTranslatorStrings.objectsTableName;
			else
				return SqlTranslatorStrings.taskTableName;
		}


		//---------------------------------------------------------------------
		/// <summary>
		/// Reastituisce il nome della colonna da aggiornare
		/// </summary>
		/// <param name="isSecurity"></param>
		/// <returns></returns>
		private string GetUpDateColumn(bool isSecurity)
		{
			if (isSecurity)
				return SqlTranslatorStrings.namespaceColumn;
			else
				return SqlTranslatorStrings.commandColumn;
		}


		//---------------------------------------------------------------------
		/// <summary>
		/// Restituisce il nome della colonna contenente il tipo del  namespace
		/// </summary>
		/// <param name="isSecurity"></param>
		/// <returns></returns>
		private string GetTypeColumn(bool isSecurity)
		{
			if (isSecurity)
				return SqlTranslatorStrings.typeIdColumn;
			else
				return SqlTranslatorStrings.typeColumn;
		}


		//---------------------------------------------------------------------
		/// <summary>
		/// Restituisce la parte di script con le DECLARE a seconda del tipo di
		/// namespace da oggiornare
		/// </summary>
		/// <param name="type"></param>
		/// <returns></returns>
		private string GetDeclarationStrig(string type)
		{
			if (string.Compare(SqlTranslatorStrings.tableTypeId, type)==0 )
				return SqlTranslatorStrings.declarationTablesUpdateScript;

			if (string.Compare(SqlTranslatorStrings.webMethodTypeId, type)==0)
				return SqlTranslatorStrings.declarationWebMethodsUpdateScript;

			if (string.Compare(SqlTranslatorStrings.reportTypeId, type)==0)
				return SqlTranslatorStrings.declarationReportsUpdateScript;

			if (string.Compare(SqlTranslatorStrings.referenceObjectTypeId, type)==0)
				return SqlTranslatorStrings.declarationReferenceObjectsUpdateScript;

			if (string.Compare(SqlTranslatorStrings.finderTypeId, type)==0 ||
					string.Compare(SqlTranslatorStrings.batchTypeId, type)==0 || 
				string.Compare(SqlTranslatorStrings.documentTypeId, type)==0)
				return SqlTranslatorStrings.declarationDocumentUpdateScript;
			
			return string.Empty;

		}


		#endregion

		public override string ToString()
		{
			return "Sql Translator";
		}




	}

	//=========================================================================
	public class SqlTranslatorStrings
	{
		public static string begin						= "BEGIN \r\n";
		public static string end						= "END";
		public static string go							= "GO";
		public static string update						= "UPDATE \r\n";
		public static string setKey						= "SET ";
		public static string where						= "WHERE( ";
		public static string namespaceColumn			= "NameSpace = ";
		public static string documentNameSpaceColumn	= "DocumentNameSpace = ";
		public static string commandColumn			= "Command = ";
		public static string and					= " AND ";
		public static string typeIdColumn			= "TypeId = ";
		public static string typeColumn				= "Type = ";
		public static string objectsTableName		= "MSD_Objects ";
		public static string taskTableName			= "MSD_ScheduledTasks ";
		public static string keyExtensionTableName	= "XE_KeyExtension ";
		public static string lostAndFoundTableName	= "XE_LostAndFound ";
		public static string namespacesTableName	= "AUDIT_Namespaces ";

		public static string roundnBracketOpen		= "(";
		public static string roundnBracketClose		= ")";
		public static string or						= " OR ";
		public static string documentTypeId			= "@documentTypeId";
		public static string batchTypeId			= "@batchTypeId";
		public static string finderTypeId			= "@finderTypeId";
		public static string reportTypeId			= "@reportTypeId";
		public static string referenceObjectTypeId	= "@referenceObjectTypeId";
		public static string tableTypeId			= "@tableTypeId";
		public static string webMethodTypeId		= "@webMethodTypeId";
		public static string viewTypeId				= "@viewTypeId";

		public static string startSecurityScriptString		= 
											@"
											if exists (select * from dbo.sysobjects where id = object_id(N'[dbo].[MSD_Objects]') and OBJECTPROPERTY(id, N'IsUserTable') = 1)";

		public static string startXTechKeyExtensionScriptString		= 
											@"
											if exists (select * from dbo.sysobjects where id = object_id(N'[dbo].[XE_KeyExtension]') and OBJECTPROPERTY(id, N'IsUserTable') = 1)";

		public static string startXTechXE_LostAndFoundScriptString		= 
											@"
											if exists (select * from dbo.sysobjects where id = object_id(N'[dbo].[XE_KeyExtension]') and OBJECTPROPERTY(id, N'IsUserTable') = 1)";


		public static string startSchedulerScriptString		= 
											@"
											if exists (select * from dbo.sysobjects where id = object_id(N'[dbo].[MSD_ScheduledTasks]') and OBJECTPROPERTY(id, N'IsUserTable') = 1)";

		public static string startAudit_NamescpacesScriptString		= 
			@"
											if exists (select * from dbo.sysobjects where id = object_id(N'[dbo].[AUDIT_Namespaces]') and OBJECTPROPERTY(id, N'IsUserTable') = 1)";


		public static string declarationDocumentUpdateScript	= 
@" 
	DECLARE " + documentTypeId + @" as integer 
	SET " + documentTypeId + @" = (SELECT TypeId FROM MSD_ObjectTypes WHERE Type = " + (int)SecurityTypeEnum.DataEntry + @")
	DECLARE " + batchTypeId + @" as integer 
	SET " + batchTypeId + @" = (SELECT TypeId FROM MSD_ObjectTypes WHERE Type = " + (int)SecurityTypeEnum.Batch + @")
	DECLARE  " + finderTypeId + @" as integer 
	SET " + finderTypeId + @" = (SELECT TypeId FROM MSD_ObjectTypes WHERE Type = " + (int)SecurityTypeEnum.Radar + @")
";
		public static string declarationReportsUpdateScript	= 
@" 
	DECLARE " + reportTypeId + @" as integer 
	SET " + reportTypeId + @" = (SELECT TypeId FROM MSD_ObjectTypes WHERE Type = " + (int)SecurityTypeEnum.Report + @")
";

		public static string declarationReferenceObjectsUpdateScript	= 
@" 
	DECLARE " + referenceObjectTypeId + @" as integer 
	SET " + referenceObjectTypeId + @" = (SELECT TypeId FROM MSD_ObjectTypes WHERE Type =" + (int)SecurityTypeEnum.HotLink + @")
";

		public static string declarationTablesUpdateScript	= 
@" 
	DECLARE " + tableTypeId + @" as integer 
	SET " + tableTypeId + @" = (SELECT TypeId FROM MSD_ObjectTypes WHERE Type = " + (int)SecurityTypeEnum.Table  + @")
	DECLARE "+ viewTypeId + @" as integer 
	SET " + viewTypeId + @" = (SELECT TypeId FROM MSD_ObjectTypes WHERE Type = "  + (int)SecurityTypeEnum.View  + @")
";

		public static string declarationWebMethodsUpdateScript	= 
			@" 
	DECLARE " + webMethodTypeId + @" as integer 
	SET " + webMethodTypeId + @" = (SELECT TypeId FROM MSD_ObjectTypes WHERE Type = " + (int)SecurityTypeEnum.Function + @")
";
	}
}
