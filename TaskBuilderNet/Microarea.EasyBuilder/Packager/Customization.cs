using System;
using System.Collections.Generic;
using System.IO;
using System.Xml;
using Microarea.EasyBuilder.MenuEditor;
using Microarea.EasyBuilder.Properties;
using Microarea.TaskBuilderNet.Interfaces.EasyStudioServer;
using Microarea.TaskBuilderNet.Core.EasyStudioServer;
using Microarea.TaskBuilderNet.Core.EasyStudioServer.Services;
using Microarea.TaskBuilderNet.Core.Generic;
using Microarea.TaskBuilderNet.Core.NameSolver;
using Microarea.TaskBuilderNet.Interfaces;
using Microarea.TaskBuilderNet.Core.EasyBuilder;

namespace Microarea.EasyBuilder.Packager
{
	/// <summary>
	/// Represents an EasyBuilder customization.
	/// </summary>
	/// <remarks>
	/// A customization is the collection of all items (menù, repostrs, dlls etc.)
	/// created to modify the standard behaviour of the application.
	/// All customization files live in 'Custom' subfolders.
	/// </remarks>
	/// <seealso cref="Microarea.EasyBuilder.Packager.CustomList"/>
	/// <seealso cref="Microarea.EasyBuilder.Packager.CustomizationContext"/>
	//=========================================================================
	internal class Customization : IEasyBuilderApp
	{
		private const string disabledExtension = ".disabled";

		private string moduleName;
		private string applicationName;
		private BaseModuleInfo moduleInfo;

		private ICustomListManager customListManager;

		public ICustomListManager EasyBuilderAppFileListManager
		{
			get { return customListManager; }
			protected set { customListManager = value; }
		}

		/// <summary>
		/// Gets a value indicating if the customization can be renamed.
		/// </summary>
		public virtual bool CanBeRenamed
		{
			get { return true; }
		}

		/// <summary>
		/// Gets a value indicating if the customization has all files needed to check licence.
		/// </summary>
		public virtual bool IsSubjectedToLicenceCheck
		{
			get { return false; }
		}

		public virtual ApplicationType ApplicationType
		{
			get { return ApplicationType.Customization; }
		}

		/// <summary>
		/// Gets the name of the customization
		/// </summary>
		public string ModuleName { get { return moduleName; } }
		public string ApplicationName { get { return applicationName; } set { applicationName = value; } }
		/// <summary>
		/// Gets the base path of the customization.
		/// </summary>
		public string BasePath { get { return ModuleInfo != null ? ModuleInfo.Path : null; } }

		public bool IsEnabled { get { return customListManager.IsEnabled; } set { customListManager.IsEnabled = value; } }

		/// <summary>
		/// Crea le cartelle necessarie per la customizzazione 
		/// Applicazione e modulo sono specificate dall'utente (uso futuro per verticali)
		/// </summary>
		//-----------------------------------------------------------------------------
		protected internal Customization(string application, string module)
		{
			this.moduleName = module;
			this.applicationName = application;

			EasyBuilderAppFileListManager = new CustomListManager(application, module, GetEasyBuilderAppFolder());

			EasyBuilderAppFileListManager.LoadCustomList();
        }


		//-----------------------------------------------------------------------------
		public virtual void CreateNeededFiles()
		{
			ServicesManager manager = ServicesManager.ServicesManagerInstance;
			bool ok = true;
			ApplicationService appService = manager.GetService(typeof(ApplicationService)) as ApplicationService;
			if (!appService.ExistsApplication(applicationName))
				ok = appService.CreateApplication(applicationName, this.ApplicationType);

			BaseApplicationInfo bai = (BaseApplicationInfo) manager.PathFinder.GetApplicationInfoByName(applicationName);

			if (!ok || bai == null)
			{
				BaseCustomizationContext.CustomizationContextInstance.Diagnostic.SetError(Resources.UnableToAddApplicationToApplicationInfos);
				return;
			}

			if (!appService.ExistsModule(applicationName, moduleName))
			{
				// aggiorna la struttura di basePathFinder e EasyStudio
				bai.AddDynamicModule(moduleName);
				ok = appService.CreateModule(applicationName, moduleName);
			}

			if (!ok)
			{
				BaseCustomizationContext.CustomizationContextInstance.Diagnostic.SetError(string.Format(Resources.UnableToAddModuleToApplicationInfos, applicationName));
				return;
			}

			//aggiungo l'application config alla custom list, ma non la salvo, tanto poche righe dopo salvo il module.config
			EasyBuilderAppFileListManager.AddToCustomList(manager.PathFinder.GetApplicationConfigFullName(applicationName), false);
			EasyBuilderAppFileListManager.AddToCustomList(manager.PathFinder.GetModuleConfigFullName(applicationName, moduleName));

			// avvisa l'applicazione di ricaricare
			CUtility.ReloadApplication(applicationName);
			BasePathFinder.BasePathFinderInstance.RefreshEasyBuilderApps(ApplicationType);
		}


		//-----------------------------------------------------------------------------
		protected virtual string GetApplicationTypeAsString()
		{
			return ApplicationType.ToString();
		}

		//--------------------------------------------------------------------------------
		public virtual IBaseModuleInfo ModuleInfo
		{
			get
			{
				if (moduleInfo == null)
				{
					if (!IsEnabled)
						return null;

					moduleInfo = (BaseModuleInfo)BaseCustomizationContext.CustomizationContextInstance.GetModuleInfo(applicationName, moduleName, ApplicationType);
				}
				return moduleInfo;
			}
		}

		//-----------------------------------------------------------------------------
		public virtual void Delete()
		{
			//Cancello il file di custom list
			try
			{
				if (File.Exists(EasyBuilderAppFileListManager.CustomListFullPath))
					File.Delete(EasyBuilderAppFileListManager.CustomListFullPath);

                for (int i = EasyBuilderAppFileListManager.CustomList.Count - 1; i > -1; --i)
                {
                    if (
                        !IsToBeDeleted(EasyBuilderAppFileListManager.CustomList[i].FilePath)
                        )
                        continue;

                    EasyBuilderAppFileListManager.RemoveFromCustomListAndFromFileSystem(EasyBuilderAppFileListManager.CustomList[i].FilePath);
                }

                ServicesManager manager = ServicesManager.ServicesManagerInstance;
                ApplicationService appService = manager.GetService(typeof(ApplicationService)) as ApplicationService;
                string customizationFolder = GetEasyBuilderAppFolder();

                if (Directory.Exists(customizationFolder) && !appService.DeleteModule(applicationName, moduleName))
                {
                    BaseCustomizationContext.CustomizationContextInstance.Diagnostic.SetError
                        (string.Format(Resources.ErrorDeletingCustomizationDirectory, moduleInfo.Path, ""));
                    return;
                }

            }
            catch (Exception e)
			{
				BaseCustomizationContext.CustomizationContextInstance.Diagnostic.SetError
					(
					string.Format(Resources.ErrorDeletingCustomizationFile, EasyBuilderAppFileListManager.CustomListFullPath, e.Message)
					);
			}

			BasePathFinder.BasePathFinderInstance.RemoveModuleInfoByName(applicationName, moduleName);

			RemoveApplicationFolderIfNeeded();
		}

		//--------------------------------------------------------------------------------
		protected virtual void RemoveApplicationFolderIfNeeded()
		{
			//Se sono l' ultima customizzazione per la data applicazione allora mi porto via anche la cartella di applciazione.
			IList<IEasyBuilderApp> easyBuilderApps = BaseCustomizationContext.CustomizationContextInstance.GetEasyBuilderApps(ApplicationName, ApplicationType);

			//Se non sono l'ultima customizazzione per la data applicazione allora non cancello la cartella
			//di applicazione.
			if (easyBuilderApps == null || easyBuilderApps.Count != 1)
				return;

			string applicationFolder = Path.GetDirectoryName(GetEasyBuilderAppFolder());
			try
			{
				Directory.Delete(applicationFolder, true);
			}
			catch (Exception exc)
			{
				BaseCustomizationContext.CustomizationContextInstance.Diagnostic.SetError(exc.ToString());
			}
		}

		//--------------------------------------------------------------------------------
		protected virtual bool IsToBeDeleted(string filePath)
		{
			//l'application.config non lo cancello, non appartiene solo ad un modulo
			return !(
				filePath.ContainsNoCase(
					NameSolverStrings.Application + NameSolverStrings.ConfigExtension
					));
		}

		//--------------------------------------------------------------------------------
		protected virtual string GetEasyBuilderAppFolder()
		{
			string appFolder = Path.Combine
						 (
						 BasePathFinder.BasePathFinderInstance.GetCustomApplicationsPath(),
						 applicationName,
						 moduleName
						 );
			return appFolder;
		}

		//--------------------------------------------------------------------------------
		public void SaveActiveDocument(string fullPath, string publishedUser)
		{
			if (fullPath.IsNullOrEmpty())
				return;

			//Imposto il documento attivo anche nella custom list 
			EasyBuilderAppFileListManager.CustomList.SetActiveDocument(fullPath);
		}

		//-----------------------------------------------------------------------------
		public void EnableModule()
		{
			if (IsEnabled)
				return;

			foreach (CustomListItem item in EasyBuilderAppFileListManager.CustomList)
			{
				string disabledFile = item.FilePath + disabledExtension;
				if (item.FilePath.IsNullOrEmpty() || !File.Exists(disabledFile))
					continue;

				if (IsInExcludeFileFromEnableDisableList(item.FilePath))
					continue;

				try
				{
					File.Move(disabledFile, item.FilePath);
				}
				catch (Exception exc)
				{
					BaseCustomizationContext.CustomizationContextInstance.Diagnostic.SetError(exc.ToString());
				}
			}

			IsEnabled = true;
		}

		//-----------------------------------------------------------------------------
		public void DisableModule()
		{
			if (!IsEnabled)
				return;

			foreach (CustomListItem item in EasyBuilderAppFileListManager.CustomList)
			{
				if (item.FilePath.IsNullOrEmpty() || !File.Exists(item.FilePath))
					continue;

				if (IsInExcludeFileFromEnableDisableList(item.FilePath))
					continue;

				try
				{
					File.Move(item.FilePath, item.FilePath + disabledExtension);
				}
				catch (Exception exc)
				{
					BaseCustomizationContext.CustomizationContextInstance.Diagnostic.SetError(exc.ToString());
				}
			}

			IsEnabled = false;
		}

		//-----------------------------------------------------------------------------
		public virtual bool IsInExcludeFileFromEnableDisableList(string file)
		{
			IList<IEasyBuilderApp> custs = BaseCustomizationContext.CustomizationContextInstance.FindEasyBuilderAppsByApplicationName(this.applicationName);

			//gli application non vanno disabilitati, almeno che non sia l'unico modulo dell'applicazione
			if (file.ContainsNoCase(NameSolverStrings.Application + NameSolverStrings.ConfigExtension) && custs.Count > 1)
				return true;

			//i file di custom list non vanno disabilitati
			if (file.ContainsNoCase(NameSolverStrings.CustomListFileExtension))
				return true;

			foreach (Customization item in BaseCustomizationContext.CustomizationContextInstance.EasyBuilderApplications)
			{
				//la customizzazione corrente ovviamente non la considero per la ricerca
				if (item.applicationName.CompareNoCase(this.applicationName) && item.moduleName.CompareNoCase(this.moduleName))
					continue;

				//Se il file appartiene anche ad un altro modulo della customizzazione non lo disabilito e segno
				//in diagnostica
				if (item.EasyBuilderAppFileListManager.CustomList.ContainsNoCase(file))
				{
					BaseCustomizationContext.CustomizationContextInstance.Diagnostic.SetWarning(string.Format(Resources.FileNotDisableBelongToOtherModule, file));
					return true;
				}
			}
			return false;
		}

		//-----------------------------------------------------------------------------
		public virtual void RenameEasyBuilderApp(string newModuleName)
		{
			if (newModuleName.CompareNoCase(moduleName))
				return;

			RenameEasyBuilderAppOnFileSystem(newModuleName);

			CUtility.ReloadApplication(applicationName);
			CUtility.ReloadAllMenus();
		}

		//-----------------------------------------------------------------------------
		protected virtual void RenameEasyBuilderAppOnFileSystem(string newModuleName)
		{
			//restituisce path compreso di modulo
			string appFolder = GetEasyBuilderAppFolder();

			DirectoryInfo di = new DirectoryInfo(appFolder);
			//Verifico se è presente la cartella root della customizzazione
			if (!di.Exists)
				return;

			string oldModuleName = this.moduleName;
			this.moduleName = newModuleName;

			string applicationFolder = di.Parent.FullName;
			try
			{
                ServicesManager manager = ServicesManager.ServicesManagerInstance;
                ApplicationService appService = manager.GetService(typeof(ApplicationService)) as ApplicationService;
                appService.RenameModule(applicationName, oldModuleName, newModuleName);
			}
			catch (Exception)
			{
				BaseCustomizationContext.CustomizationContextInstance.Diagnostic.SetError(Resources.UnableToRenameModule);
			}

			//genero il filepath del nuovo file di customlist
			string newCustomListFullPath =
				Path.Combine
				(
					applicationFolder,
					newModuleName + GetEasyBuilderAppListFileExtension()
				);

			EasyBuilderAppFileListManager.CustomList.Remove(EasyBuilderAppFileListManager.CustomListFullPath);
			EasyBuilderAppFileListManager.CustomList.Add(newCustomListFullPath);

			try
			{
                //rinomino il file di customlist con il nome nuovo...
                File.Move(EasyBuilderAppFileListManager.CustomListFullPath, newCustomListFullPath);
			}
			catch (Exception)
			{
				BaseCustomizationContext.CustomizationContextInstance.Diagnostic.SetError(Resources.UnableToRenameCustomList);
			}

			//... e lo associo alla customizzazione corrente
			EasyBuilderAppFileListManager.CustomListFullPath = newCustomListFullPath;

			string toSubstitute = string.Format("\\{0}\\{1}\\{2}", NameSolverStrings.Applications, applicationName, oldModuleName);
			string newString = string.Format("\\{0}\\{1}\\{2}", NameSolverStrings.Applications, applicationName, newModuleName);

			string newMenuFilePath = null;

			//Frugo nella custom list e rinomino anche i path interni
			for (int i = 0; i < EasyBuilderAppFileListManager.CustomList.Count; i++)
			{
				EasyBuilderAppFileListManager.CustomList[i].FilePath = EasyBuilderAppFileListManager.CustomList[i].FilePath.ReplaceNoCase(toSubstitute, newString);
				if (Path.GetExtension(EasyBuilderAppFileListManager.CustomList[i].FilePath) == NameSolverStrings.MenuExtension)
					//Calcolo il path del nuovo file per poterne modificare gli attributi in seguito.
					newMenuFilePath = EasyBuilderAppFileListManager.CustomList[i].FilePath;
			}

			ModuleInfo.Name = newModuleName;

			//Rinomino i file xml di databaseObject
			RenameDatabaseObjects(applicationName, applicationName, oldModuleName, newModuleName);

			//e di addon DatabaseObject
			RenameAddOnDatabaseObjects(applicationName, applicationName, oldModuleName, newModuleName);

			//e dei documentObjects
			RenameDocumentObjects(applicationName, applicationName, oldModuleName, newModuleName);

			MenuEditorEngine.RenameMenuModule(newMenuFilePath, applicationName, newModuleName, oldModuleName);

			EasyBuilderAppFileListManager.SaveCustomList();
		}

		//-----------------------------------------------------------------------------
		protected virtual string GetEasyBuilderAppListFileExtension()
		{
			return NameSolverStrings.CustomListFileExtension;
		}

		//-----------------------------------------------------------------------------
		private void RenameDocumentObjects(string oldApplicationName, string newApplicationName, string oldModuleName, string newModuleName)
		{
			#region AddOnDatabaseObject Sample
			//DocumentObjects.xml
			/*
			 <?xml version="1.0" encoding="utf-8"?>
				<DocumentObjects>
				  <Documents>
					<Document namespace="ProvaApp.ProvaMod.DynamicDocuments.aaa" localize="aaa" classhierarchy="" defaultsecurityroles="" dynamic="true">
					  <ViewModes />
					</Document>
				  </Documents>
				</DocumentObjects>
			*/
			#endregion

			string easyBuilderAppFolder = GetEasyBuilderAppFolder();
			string documentObjectsFile = Path.Combine(easyBuilderAppFolder, NameSolverStrings.ModuleObjects, NameSolverStrings.DocumentObjectsXml);
			if (!File.Exists(documentObjectsFile))
				return;

			XmlDocument xDoc = new XmlDocument();
			try
			{
				xDoc.Load(documentObjectsFile);
			}
			catch (Exception e)
			{
				BaseCustomizationContext.CustomizationContextInstance.Diagnostic.SetError(string.Format(Resources.RenameDatabaseObjectLoadError, e.Message));
				return;
			}

			string xpath = string.Format
				(
					"//{0}/{1}/{2}",
					DocumentsObjectsXML.Element.DocumentObjects,
					DocumentsObjectsXML.Element.Documents,
					DocumentsObjectsXML.Element.Document
				);

			XmlNodeList nodeList = xDoc.SelectNodes(xpath);
			if (nodeList == null)
				return;

			foreach (XmlNode current in nodeList)
				ChangeXmlObjectsNamespace(current, oldApplicationName, newApplicationName, oldModuleName, newModuleName);

			try
			{
				xDoc.Save(documentObjectsFile);
			}
			catch (Exception e)
			{
				BaseCustomizationContext.CustomizationContextInstance.Diagnostic.SetError(string.Format(Resources.RenameDatabaseObjectSaveError, e.Message));
				return;
			}
		}

		//-----------------------------------------------------------------------------
		private void RenameDatabaseObjects(string oldApplicationName, string newApplicationName, string oldModuleName, string newModuleName)
		{
			#region DatabaseObject Sample
			//DatabaseObjects.xml
			/*
			 <?xml version="1.0" encoding="utf-8"?>
				<DatabaseObjects>
				  <Signature>Accounting</Signature>
				  <Release>36</Release>
				  <Tables>
					<Table namespace="ERP.Accounting.Dbl.MA_AccountingDefaults">
					  <Create...
			 */
			#endregion

			string easyBuilderAppFolder = GetEasyBuilderAppFolder();
			string databaseObjectFile = Path.Combine(easyBuilderAppFolder, NameSolverStrings.ModuleObjects, NameSolverStrings.DatabaseObjectsXml);
			if (!File.Exists(databaseObjectFile))
				return;

			XmlDocument xDoc = new XmlDocument();
			try
			{
				xDoc.Load(databaseObjectFile);
			}
			catch (Exception e)
			{
				BaseCustomizationContext.CustomizationContextInstance.Diagnostic.SetError(string.Format(Resources.RenameDatabaseObjectLoadError, e.Message));
				return;
			}

			string xpathSignature = string.Format
				(
					"//{0}/{1}",
					DataBaseObjectsXML.Element.DatabaseObjects,
					DataBaseObjectsXML.Element.Signature
				);

			XmlNode node = xDoc.SelectSingleNode(xpathSignature);
			if (node == null)
				return;

			node.InnerText = newModuleName;

			string xpathTable = string.Format
				(
					"//{0}/{1}/{2}",
					DataBaseObjectsXML.Element.DatabaseObjects,
					DataBaseObjectsXML.Element.Tables,
					DataBaseObjectsXML.Element.Table
				);

			XmlNodeList nodeList = xDoc.SelectNodes(xpathTable);
			if (nodeList == null)
				return;

			foreach (XmlNode current in nodeList)
				ChangeXmlObjectsNamespace(current, oldApplicationName, newApplicationName, oldModuleName, newModuleName);

			try
			{
				xDoc.Save(databaseObjectFile);
			}
			catch (Exception e)
			{
				BaseCustomizationContext.CustomizationContextInstance.Diagnostic.SetError(string.Format(Resources.RenameDatabaseObjectSaveError, e.Message));
				return;
			}
		}

		//-----------------------------------------------------------------------------
		private void RenameAddOnDatabaseObjects(
			string oldApplicationName,
			string newApplicationName,
			string oldModuleName,
			string newModuleName
			)
		{
			#region AddOnDatabaseObject Sample
			//AddOnDatabaseObjects.xml
			/*
			 <?xml version="1.0" encoding="utf-8"?>
				<AddOnDatabaseObjects>
				  <AdditionalColumns>
					<Table namespace="ERP.Items.Dbl.MA_Items">
					  <AlterTable namespace="ERP.PublishingTax.AddOnsItems" release="1" createstep="3" />
					</Table>
					<Table namespace="ERP.Items.Dbl.MA_ItemTypes">
					  <AlterTable namespace="ERP.PublishingTax.AddOnsItems" release="1" createstep="3" />
					</Table>
					<Table namespace="ERP.Sales.Dbl.MA_SaleDocDetail">
					  <AlterTable namespace="ERP.PublishingTax.AddOnsSales" release="1" createstep="4" />
					</Table>
				  </AdditionalColumns>
				</AddOnDatabaseObjects>
			*/
			#endregion

			string easyBuilderAppFolder = GetEasyBuilderAppFolder();
			string addonDatabaseObjectFile = Path.Combine(easyBuilderAppFolder, NameSolverStrings.ModuleObjects, NameSolverStrings.AddOnDatabaseObjectsXml);
			if (!File.Exists(addonDatabaseObjectFile))
				return;

			XmlDocument xDoc = new XmlDocument();
			try
			{
				xDoc.Load(addonDatabaseObjectFile);
			}
			catch (Exception e)
			{
				BaseCustomizationContext.CustomizationContextInstance.Diagnostic.SetError(string.Format(Resources.RenameDatabaseObjectLoadError, e.Message));
				return;
			}

			string xpath = string.Format
				(
					"//{0}/{1}/{2}/{3}",
					AddOnDatabaseObjectsXML.Element.AddOnDatabaseObjects,
					AddOnDatabaseObjectsXML.Element.AdditionalColumns,
					AddOnDatabaseObjectsXML.Element.Table,
					AddOnDatabaseObjectsXML.Element.AlterTable
				);

			XmlNodeList nodeList = xDoc.SelectNodes(xpath);
			if (nodeList == null)
				return;

			foreach (XmlNode current in nodeList)
				ChangeXmlObjectsNamespace(current, oldApplicationName, newApplicationName, oldModuleName, newModuleName);

			try
			{
				xDoc.Save(addonDatabaseObjectFile);
			}
			catch (Exception e)
			{
				BaseCustomizationContext.CustomizationContextInstance.Diagnostic.SetError(string.Format(Resources.RenameDatabaseObjectSaveError, e.Message));
				return;
			}
		}

		//-----------------------------------------------------------------------------
		private void ChangeXmlObjectsNamespace(
			XmlNode current,
			string oldApplicationName,
			string newApplicationName,
			string oldModuleName,
			string newmoduleName
			)
		{
			if (current.Attributes[DataBaseObjectsXML.Attribute.Namespace] == null)
				return;

			//Cerco l'attributo namespace namespace="ERP.VecchioNome.Dbl.MA_AccountingDefaults"
			//sostituisco al posto di ERP.VecchioNome.      ERP.NuovoNome.
			XmlAttribute att = current.Attributes[DataBaseObjectsXML.Attribute.Namespace];
			att.InnerText = att.InnerText.ReplaceNoCase
				(
				string.Format("{0}.{1}.", oldApplicationName, oldModuleName),
				string.Format("{0}.{1}.", newApplicationName, newmoduleName)
				);
		}

		/// <summary>
		/// Determines whether the specified Customization is equal to the current Customization.
		/// </summary>
		/// <param name="obj">The Customization to compare with the current Customization.</param>
		//-----------------------------------------------------------------------------
		public override bool Equals(object obj)
		{
			Customization cust = obj as Customization;
			if (cust == null)
				return false;

			return moduleName.CompareNoCase(cust.moduleName) && applicationName.CompareNoCase(cust.applicationName);
		}

		/// <summary>
		/// Serves as a hash function for a particular type. 
		/// </summary>
		/// <returns>A hash code for the current Customization.</returns>
		//-----------------------------------------------------------------------------
		public override int GetHashCode()
		{
			return base.GetHashCode();
		}

		/// <summary>
		/// Sostituisce in tutti i file della customizzazione la vecchia applicazione con la nuova
		/// (customlist, document objects, database objects, ecc)
		/// </summary>
		/// <param name="oldAppName"></param>
		/// <param name="newAppName"></param>
		/// <param name="moduleName"></param>
		//-----------------------------------------------------------------------------
		public virtual void RenameApplicationReferencesInModule(string moduleName, string oldAppName, string newAppName)
		{
			string stringToSubstitute = string.Format("\\{0}\\{1}", NameSolverStrings.Applications, oldAppName);
			string newString = string.Format("\\{0}\\{1}", NameSolverStrings.Applications, newAppName);

			applicationName = newAppName;

			string newMenuFilePath = null;
			ICustomList customList = EasyBuilderAppFileListManager.CustomList;
			//Frugo nella custom list e rinomino anche i path interni
			for (int i = 0; i < customList.Count; i++)
			{
				customList[i].FilePath = customList[i].FilePath.ReplaceNoCase(stringToSubstitute, newString);

				if (Path.GetExtension(customList[i].FilePath) == NameSolverStrings.MenuExtension)
					//Calcolo il path del nuovo file per poterne modificare gli attributi in seguito.
					newMenuFilePath = customList[i].FilePath;
			}

			//ricalcolo il nuovo path
			EasyBuilderAppFileListManager.CustomListFullPath =
					Path.Combine(
					BasePathFinder.BasePathFinderInstance.GetCustomApplicationsPath(),
					newAppName,
					moduleName + NameSolverStrings.CustomListFileExtension
					);

			RenameDatabaseObjects(oldAppName, newAppName, moduleName, moduleName);
			RenameAddOnDatabaseObjects(oldAppName, newAppName, moduleName, moduleName);
			RenameDocumentObjects(oldAppName, newAppName, moduleName, moduleName);

			if (!newMenuFilePath.IsNullOrEmpty() && File.Exists(newMenuFilePath))
				MenuEditorEngine.RenameMenuApplication(newMenuFilePath, moduleName, newAppName, oldAppName);

			EasyBuilderAppFileListManager.SaveCustomList();
		}
	}
}
