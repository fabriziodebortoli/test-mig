using System.IO;
using Microarea.EasyBuilder.Packager;
using Microarea.TaskBuilderNet.Core.Generic;
using Microarea.TaskBuilderNet.Core.NameSolver;
using Microarea.TaskBuilderNet.Interfaces.Model;
using Microarea.TaskBuilderNet.Interfaces;
using Microarea.TaskBuilderNet.Core.EasyBuilder;

namespace Microarea.EasyBuilder.DBScript
{
	//================================================================================
	class DBScriptGenerator
	{
		private DatabaseChanges changes;

		//--------------------------------------------------------------------------------
		public DBScriptGenerator(DatabaseChanges changes)
		{
			this.changes = changes;
		}

		//--------------------------------------------------------------------------------
		internal string Generate()
		{
			string backFolder = "";
			string backDataBaseObjects = "", backAddonDataBaseObjects = "";
			BaseModuleInfo module = (BaseModuleInfo)BaseCustomizationContext.CustomizationContextInstance.CurrentEasyBuilderApp.ModuleInfo;
			string databaseScriptPath = module.GetDatabaseScriptPath();

			if (Directory.Exists(databaseScriptPath))
			{
				backFolder = GetBackupFolderName(databaseScriptPath);
				Directory.Move(databaseScriptPath, backFolder);
			}
			try
			{
				//inizializzo il writer del databaseobjects.xml
				string file = module.DatabaseObjectsInfo.FilePath;

				if (File.Exists(file))
				{
					backDataBaseObjects = GetBackupFileName(file);
					File.Move(file, backDataBaseObjects);
				}
				DatabaseObjectsWriter aDatabaseObjectsWriter = new DatabaseObjectsWriter(file, BaseCustomizationContext.CustomizationContextInstance.CurrentEasyBuilderApp.ModuleInfo.Name);
				//imposto la release corrente
				aDatabaseObjectsWriter.SetReleaseNumber(DatabaseChanges.GetDatabaseRelease(BaseCustomizationContext.CustomizationContextInstance.CurrentEasyBuilderApp));

				//inizializzo il writer del createinfo.xml
				CreateInfoWriter aCreateInfoWriter = new CreateInfoWriter(databaseScriptPath, module.Name);
				//genero il file createinfo.xml se non esiste, quindi
				//ripulisco tutti gli script e li ricreo in base a quelli che trovo nel file system
				aCreateInfoWriter.New();

				//inizializzo il writer del upgradeinfo.xml
				UpgradeInfoWriter anUpgradeInfoWriter = new UpgradeInfoWriter(module.GetDatabaseScriptPath(), module.Name);
				anUpgradeInfoWriter.New();

				AddonDatabaseObjectsWriter aAddonDatabaseObjectsWriter = null;
				if (changes.NewAddonFields.Count > 0)
				{
					string addonFile = module.AddOnDatabaseObjectsInfo.FilePath;
					if (File.Exists(addonFile))
					{
						backAddonDataBaseObjects = GetBackupFileName(addonFile);
						File.Move(addonFile, backAddonDataBaseObjects);
					}
					aAddonDatabaseObjectsWriter = new AddonDatabaseObjectsWriter(addonFile, BaseCustomizationContext.CustomizationContextInstance.CurrentEasyBuilderApp.ModuleInfo.Name);
				}

				NameSpace libNamespace = new NameSpace(
						string.Format("{0}.{1}.{2}",
						module.ParentApplicationName,
						module.Name,
						BaseCustomizationContext.CustomizationContextInstance.DynamicLibraryName),
						TaskBuilderNet.Interfaces.NameSpaceObjectType.Library);

				//per ogni tabella che mi appartiene (aggiunta adesso o in precedenza), genero lo script di creazione
				string scriptFile;
				foreach (IRecord rec in changes.NewTables)
				{
					GenerateCreateScripts(module, rec, out scriptFile);
					int createStep = aCreateInfoWriter.AddSQLScript(scriptFile, null, null);

					NameSpace tableNamespace = new NameSpace(
					string.Format("{0}.{1}.{2}.{3}",
					module.ParentApplicationName,
					module.Name,
					BaseCustomizationContext.CustomizationContextInstance.DynamicLibraryName,
					rec.Name), TaskBuilderNet.Interfaces.NameSpaceObjectType.Table);
					aDatabaseObjectsWriter.AddTable(tableNamespace, rec, createStep);

					if (rec.CreationRelease > 1)
						anUpgradeInfoWriter.AddSQLScript(scriptFile, rec.CreationRelease, null, null);
					
					//se nella tabella ci sono campi aggiunti in release successive, devo generare gli script di upgrade
					foreach (IRecordField field in rec.Fields)
					{
						if (field.CreationRelease > rec.CreationRelease)
							AddUpgradeScript(anUpgradeInfoWriter, scriptFile, tableNamespace, field);
					}
				}

				aDatabaseObjectsWriter.Save();

				//per ogni campo aggiunto dalla customizzazione (adesso o in precedenza), genero lo script di upgrade nella cartella di create
				//e, per le release superiori a 1, nella corrispondente cartella di upgrade
				string createFolderAll = aCreateInfoWriter.CreateScriptPathAll;
				string createFolderOracle = aCreateInfoWriter.CreateScriptPathOracle;
				foreach (string tableNamespace in changes.NewAddonFields.Keys)
				{
					foreach (IRecordField field in changes.NewAddonFields[tableNamespace])
					{
						BaseModuleInfo dependsOnModule = (BaseModuleInfo)BasePathFinder.BasePathFinderInstance.GetModuleInfo(new NameSpace(tableNamespace));
						string mod = dependsOnModule.DatabaseObjectsInfo.Signature;
						string app = dependsOnModule.ParentApplicationInfo.ApplicationConfigInfo.DbSignature;

						GenerateUpgradeScripts(createFolderAll, createFolderOracle, new NameSpace(tableNamespace).Leaf, field, out scriptFile);
						int aStep = aCreateInfoWriter.AddSQLScript(scriptFile, app, mod);
						aAddonDatabaseObjectsWriter.AddAlterTable(tableNamespace, libNamespace, field, aStep);
						if (field.CreationRelease > 1)
							AddUpgradeScript(anUpgradeInfoWriter, scriptFile, tableNamespace, field);

					}
				}

				if (aAddonDatabaseObjectsWriter != null)
					aAddonDatabaseObjectsWriter.Save();

				return backFolder;
			}
			catch
			{
				//cancello la cartella di lavoro
				if (Directory.Exists(databaseScriptPath))
					Directory.Delete(databaseScriptPath, true);
				//ripristino il backup della database script
				if (Directory.Exists(backFolder))
					Directory.Move(backFolder, databaseScriptPath);

				//ripristino il backup del databaseobjects.xml (se c'era)
				if (File.Exists(module.DatabaseObjectsInfo.FilePath))
					File.Delete(module.DatabaseObjectsInfo.FilePath);
				if (File.Exists(backDataBaseObjects))
					File.Move(backDataBaseObjects, module.DatabaseObjectsInfo.FilePath);

				//ripristino il backup del addondatabaseobjects.xml (se c'era)
				if (File.Exists(module.AddOnDatabaseObjectsInfo.FilePath))
					File.Delete(module.AddOnDatabaseObjectsInfo.FilePath);
				if (File.Exists(backAddonDataBaseObjects))
					File.Move(backAddonDataBaseObjects, module.AddOnDatabaseObjectsInfo.FilePath);
				throw;
			}

		}

		//--------------------------------------------------------------------------------
		private void AddUpgradeScript(UpgradeInfoWriter anUpgradeInfoWriter, string scriptFile, NameSpace tableNamespace, IRecordField field)
		{
			string upgradeFolderAll = anUpgradeInfoWriter.GetUpgradeScriptPathAll(field.CreationRelease);
			string upgradeFolderOracle = anUpgradeInfoWriter.GetUpgradeScriptPathOracle(field.CreationRelease);
			GenerateUpgradeScripts(upgradeFolderAll, upgradeFolderOracle, new NameSpace(tableNamespace).Leaf, field, out scriptFile);
			anUpgradeInfoWriter.AddSQLScript(scriptFile, field.CreationRelease, null, null);
            BaseCustomizationContext.CustomizationContextInstance.AddToCurrentCustomizationList(Path.Combine(upgradeFolderAll,scriptFile));
            BaseCustomizationContext.CustomizationContextInstance.AddToCurrentCustomizationList(Path.Combine(upgradeFolderOracle, scriptFile));
        }

		//--------------------------------------------------------------------------------
		private static string GetBackupFolderName(string folderPath)
		{
			string backFolder = folderPath + "_bak";
			while (Directory.Exists(backFolder))
				Directory.Delete(backFolder, true);
			return backFolder;
		}
		//--------------------------------------------------------------------------------
		private static string GetBackupFileName(string filePath)
		{
			string backFile = filePath + NameSolverStrings.BakExtension;
			while (File.Exists(backFile))
				File.Delete(backFile);
			return backFile;
		}
		//--------------------------------------------------------------------------------
		private static void CleanFolder(string createFolder)
		{
			if (Directory.Exists(createFolder))
				Directory.Delete(createFolder, true); //TODOPERASSO fare backup
		}

		//--------------------------------------------------------------------------------
		private bool GenerateCreateScripts(BaseModuleInfo module, IRecord addedRecord, out string fileName)
		{
			string aTablePhysicalName = addedRecord.Name;
			string aComment = string.Empty;

			IndexManager IdxManager = new IndexManager("");

			Tag aTag = new Tag(aComment);
			aTag.TablePhysicalName = aTablePhysicalName;
			aTag.IndexFields = IdxManager.GetFields(false);
			aTag.IndexFieldsOracle = IdxManager.GetFields(true);
			aTag.IndexName = IdxManager.Name;

			CreateSQLScriptWriter aCreateSQLScriptWriter = new CreateSQLScriptWriter(module.GetDatabaseScriptPath(), aTag);

			string sqlFile = aCreateSQLScriptWriter.GetFilename(false);
			fileName = Path.GetFileName(sqlFile);
			aCreateSQLScriptWriter.New(false);
			string oracleFile = aCreateSQLScriptWriter.GetFilename(true);
			aCreateSQLScriptWriter.New(true);

			foreach (IRecordField field in addedRecord.Fields)
			{
				SQLField sf = new SQLField(field);
				sf.Oracle = false;
				aTag.Type = sf.Type;
				aTag.DefaultValue = sf.DefaultValue;
				sf.Oracle = true;
				aTag.TypeOracle = sf.Type;
				aTag.Constraint = "DF_" + addedRecord.Name + "_" + field.Name + "_00";
				aTag.DefaultValueOracle = sf.DefaultValue;
				aTag.FieldPhysicalName = field.Name;

				aCreateSQLScriptWriter.AddField(field.IsSegmentKey);
			}

			//gli script di create, nel caso siano associati ad una release di database  maggiore di 1, devono essere messi
			//anche nella cartelle di upgrade dello step di release corrispondente
			if (addedRecord.CreationRelease > 1)
			{
                string upgradeFile = GetCorrespondingUpgradePath(sqlFile, addedRecord.CreationRelease);
                File.Copy(sqlFile, upgradeFile);
                BaseCustomizationContext.CustomizationContextInstance.AddToCurrentCustomizationList(upgradeFile);
                upgradeFile = GetCorrespondingUpgradePath(oracleFile, addedRecord.CreationRelease);
                File.Copy(oracleFile, upgradeFile);
                BaseCustomizationContext.CustomizationContextInstance.AddToCurrentCustomizationList(upgradeFile);
            }

			return true;
		}

		
		//--------------------------------------------------------------------------------
		private string GetCorrespondingUpgradePath(string createFile, int databaseRelease)
		{
			string file = Path.GetFileName(createFile);
			string folder = createFile.Replace("\\Create\\", "\\Upgrade\\");
			folder = Path.Combine(Path.GetDirectoryName(folder), string.Format("Release_{0}", databaseRelease));

			if (!Directory.Exists(folder))
				Directory.CreateDirectory(folder);
			return Path.Combine(folder, file);
		}


		//--------------------------------------------------------------------------------
		private bool GenerateUpgradeScripts(string allPath, string oraclePath, string tableName, IRecordField field, out string fileName)
		{
			string aComment = string.Empty;

			IndexManager IdxManager = new IndexManager("");

			Tag aTag = new Tag(aComment);
			aTag.TablePhysicalName = tableName;
			aTag.IndexFields = IdxManager.GetFields(false);
			aTag.IndexFieldsOracle = IdxManager.GetFields(true);
			aTag.IndexName = IdxManager.Name;

			UpgradeSQLScriptWriter aUpgradeSQLScriptWriter = new UpgradeSQLScriptWriter(allPath, oraclePath, false, aTag);

			string sqlFile = aUpgradeSQLScriptWriter.GetFilename(false);

			fileName = Path.GetFileName(sqlFile);

			string oracleFile = aUpgradeSQLScriptWriter.GetFilename(true);

			SQLField sf = new SQLField(field);
			sf.Oracle = false;
			aTag.Type = sf.Type;
			aTag.DefaultValue = sf.DefaultValue;
			aTag.UpdateScript = sf.UpdateScript;
			sf.Oracle = true;
			aTag.TypeOracle = sf.Type;
			aTag.Constraint = "DF_" + tableName + "_" + field.Name + "_00";
			aTag.DefaultValueOracle = sf.DefaultValue;
			aTag.FieldPhysicalName = field.Name;
			aTag.UpdateScriptOracle = sf.UpdateScript;

			aUpgradeSQLScriptWriter.AddField();
			aUpgradeSQLScriptWriter.AddUpdateScript();

			return true;
		}


	}
}
