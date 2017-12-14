using System;
using System.Linq;
using System.Collections.Generic;
using System.IO;
using System.Xml;
using Microarea.EasyBuilder.MenuEditor;
using Microarea.EasyBuilder.Properties;
using Microarea.TaskBuilderNet.Core.DiagnosticManager;
using Microarea.TaskBuilderNet.Core.Generic;
using Microarea.TaskBuilderNet.Core.NameSolver;
using Microarea.TaskBuilderNet.Interfaces;
using Microarea.TaskBuilderNet.Core.EasyBuilder;
using Microarea.TaskBuilderNet.Core.EasyStudioServer;
using Microarea.TaskBuilderNet.Core.EasyStudioServer.Services;

namespace Microarea.EasyBuilder.Packager
{
	//=========================================================================
	public partial class CustomizationContext
	{
		//--------------------------------------------------------------------------------
		/// <summary>
		/// Internal use
		/// </summary>
		public bool AddNewEasyBuilderApp(
			string newAppName,
			string newModName,
			ApplicationType applicationType
			)
		{
			bool created = false;
			switch (applicationType)
			{
				case ApplicationType.Standardization:
					created = CustomizationContextInstance.AddNewStandardization(newAppName, newModName) != null;
					break;
				case ApplicationType.Customization:
					created = CustomizationContextInstance.AddNewCustomization(newAppName, newModName) != null;
					break;
				default:
					break;
			}

			return created;
		}

		/// <summary>
		/// Imposta il context su una diversa customizzazione
		/// </summary>
		//-----------------------------------------------------------------------------
		public IEasyBuilderApp AddNewCustomization(string application, string module = "Module1")
		{
			lock (lockObject)
			{
				IEasyBuilderApp tempCust = BaseCustomizationContext.CustomizationContextInstance.FindEasyBuilderApp(application, module);
				if (tempCust == null)
				{
					tempCust = CustomizationContextInstance.CreateNew(application, module, ApplicationType.Customization);
					CustomizationContextInstance.EasyBuilderApplications.Add(tempCust);
				}
				return tempCust;
			}
		}


		/// <summary>
		/// Internal Use
		/// </summary>
		//-----------------------------------------------------------------------------
		public IEasyBuilderApp AddNewStandardization(string application, string module)
		{
			lock (lockObject)
			{
				IEasyBuilderApp tempCust = CustomizationContextInstance.FindEasyBuilderApp(application, module);
				if (tempCust == null)
				{
					tempCust = CreateNew(application, module, ApplicationType.Standardization);
					EasyBuilderApplications.Add(tempCust);
				}

				return tempCust;
			}
		}

		/// <summary>
		/// Internal Use
		/// </summary>
		//-----------------------------------------------------------------------------
		public IEasyBuilderApp CreateNew(
			string application,
			string module,
			ApplicationType appType
			)
		{
			IEasyBuilderApp app = null;
			switch (appType)
			{
				case ApplicationType.Customization:
					app = new Customization(application, module);
                    ((Customization)app).CreateNeededFiles();

                    break;
				case ApplicationType.Standardization:
					app = new Standardization(application, module);
                    ((Standardization)app).CreateNeededFiles();
                    Standardization.ActivateAndReinitTBActivationInfo();
					break;
				default:
					throw new Exception("Unrecognized EasyBuilder Application type");
			}

			//La LoadCustomList viene fatta anche dal costruttore ma, siccome se non è presente
			//la custom list su disco la LoadCustomList non fa nulla,
			//ci troveremmo con una customizzazione che non ha nessuna custom list caricata.
			//Quindi è necessario ripetere la chiamata qui.
			app.EasyBuilderAppFileListManager.LoadCustomList();

			return app;
		}

		/// <summary>
		/// Internal Use
		/// </summary>
		//-----------------------------------------------------------------------------
		public void DeleteApplication(string application)
		{
            IList<IEasyBuilderApp> custs = CustomizationContextInstance.FindEasyBuilderAppsByApplicationName(application);
            if (custs == null || custs.Count == 0)
                return;

            try
            {
                lock (lockObject)
                {

                    ServicesManager manager = ServicesManager.ServicesManagerInstance;
                    ApplicationService appService = manager.GetService(typeof(ApplicationService)) as ApplicationService;

                    appService.DeleteApplication(application);
                    foreach (IEasyBuilderApp cust in custs)
                        CustomizationContextInstance.EasyBuilderApplications.Remove(cust);

                    BasePathFinder.BasePathFinderInstance.DeleteApplicationByName(application);
                }

                // Standardization.ActivateAndReinitTBActivationInfo();
				//Se abbiamo cancellato una customizzazione allora dobbiamo far ricaricare le informazioni
				//circa i documenti al TB C++ e anche il menu.
				CUtility.ReloadAllMenus();
				//La reload application va ad aggiornare correttamente i file xml di descrizione dei documenti,
				//ma non aggiorna m_pStandardDocumentsTable di application context (che viene caricato una volta 
				//solo all'avvio dell'applicazione e mai aggiornato ed e' usato nella rundocument per cercare la descrizione
				//del documento da lanciare)
				//L'effetto e' che se cancelliamo un'applicazione che ha dei documenti, per la rundocument essi esistono ancora 
				//e non blocca il run.
		

				//Invalido la cache per far si che vengano ricaricati i menu e rigenerata la
				//dll degli enumerativi al lancio successivo dell'applicazione
				BasePathFinder.BasePathFinderInstance.InstallationVer.UpdateCachedDateAndSave();
			}
			catch (Exception exc)
			{
				BaseCustomizationContext.CustomizationContextInstance.Diagnostic.SetError(exc.ToString());
				throw;
			}
		}

		/// <summary>
		/// Internal use
		/// </summary>
		//--------------------------------------------------------------------------------
		public void DeleteMenu(IEasyBuilderApp easyBuilderApp, string fullPath)
		{
			easyBuilderApp.EasyBuilderAppFileListManager.RemoveFromCustomListAndFromFileSystem(fullPath);
		}

		//--------------------------------------------------------------------------------
		/// <summary>
		/// Internal use
		/// </summary>
		public void DeleteDocumentCustomization(
			IEasyBuilderApp easyBuilderApp,
			string fullPath,
			string publishedUser
			)
		{
			DeleteItemFromActiveDocuments(fullPath, publishedUser);

			//se e' un server document va rimosso dal moduleobjects
			RemoveFromModuleObjects(easyBuilderApp, fullPath);
			//e dal file di menu
			RemoveFromMenu(easyBuilderApp, fullPath, publishedUser);

			//Verifico se si tratta di un server document costituito da piu` DLL
			//(come puo` avvenire nel caso di documenti creati con la funzionalita` "Save as new document").
			//Se `e quesot il caso, allora bado a rimuovere anche quelle DLL.
			string[] readOnlyServerDocPaths = easyBuilderApp.EasyBuilderAppFileListManager.GetAllReadOnlyServerDocPart(fullPath);
			if (readOnlyServerDocPaths != null && readOnlyServerDocPaths.Length > 0)
			{
				foreach (var item in readOnlyServerDocPaths)
				{
					easyBuilderApp.EasyBuilderAppFileListManager.RemoveFromCustomListAndFromFileSystem(item);
				}
			}

			easyBuilderApp.EasyBuilderAppFileListManager.RemoveFromCustomListAndFromFileSystem(fullPath);
		}

		//-----------------------------------------------------------------------------
		internal static void DeleteItemFromActiveDocuments(string fullPath, string publishedUser)
		{
			string custNs = BaseCustomizationContext.CustomizationContextInstance.GetPseudoNamespaceFromFullPath(fullPath, publishedUser);
			if (custNs.IsNullOrEmpty())
				return;

			//giro per tutti gli active documents alla ricerca di quelli che hanno per radice il documento papa e li rimuovo
			if (BaseCustomizationContext.CustomizationContextInstance.ActiveDocuments.Count <= 0)
				return;

			for (int i = BaseCustomizationContext.CustomizationContextInstance.ActiveDocuments.Count - 1; i >= 0; i--)
			{
				if (BaseCustomizationContext.CustomizationContextInstance.ActiveDocuments[i].CompareNoCase(custNs))
					BaseCustomizationContext.CustomizationContextInstance.ActiveDocuments.RemoveAt(i);
			}
		}

		//--------------------------------------------------------------------------------
		private void RemoveFromMenu(IEasyBuilderApp app, string fullPath, string publishedUser)
		{
			INameSpace ns = BaseCustomizationContext.CustomizationContextInstance.FormatDynamicNamespaceDocument(app.ApplicationName, app.ModuleName, new DirectoryInfo(Path.GetDirectoryName(fullPath)).Name);
			MenuEditor.MenuEditorEngine.RemoveDocumentFromMenuFile(app, ns.GetNameSpaceWithoutType(), publishedUser);
		}

		//--------------------------------------------------------------------------------
		private void RemoveFromModuleObjects(IEasyBuilderApp app, string fullPath)
		{
			DirectoryInfo di = new DirectoryInfo(Path.GetDirectoryName(fullPath));

			IBaseModuleInfo mi = app.ModuleInfo;
			DocumentsObjectInfo info = (DocumentsObjectInfo)mi.DocumentObjectsInfo;

			for (int i = info.Documents.Count - 1; i >= 0; i--)
			{
				if (di.Name.CompareNoCase(((DocumentInfo)info.Documents[i]).NameSpace.Leaf))
				{
					info.Documents.RemoveAt(i);
					break;
				}
			}
			string documentObjectsPath = mi.GetDocumentObjectsPath();
			if (!info.UnParse(documentObjectsPath))
			{
				Diagnostic.SetError(String.Format(Resources.ErrorSavingDocumentObjects, documentObjectsPath));
			}
		}

		/// <summary>
		/// Internal Use
		/// </summary>
		//-----------------------------------------------------------------------------
		public void DeleteEasyBuilderApp(IEasyBuilderApp cust)
		{
            if (cust == null)
                return;

            cust.Delete();
            CustomizationContextInstance.EasyBuilderApplications.Remove(cust);
        }

        /// <summary>
        /// Internal Use
        /// </summary>
        //-----------------------------------------------------------------------------
        public void DeleteEasyBuilderApp(string application, string module)
		{
			IEasyBuilderApp cust = null;
			lock (lockObject)
			{
				cust = BaseCustomizationContext.CustomizationContextInstance.FindEasyBuilderApp(application, module);

				DeleteEasyBuilderApp(cust);

				//Se non ci sono più customizzazioni, annullo anche l'array
				if (CustomizationContextInstance.EasyBuilderApplications.Count == 0)
					easyBuilderApplications = null;
			}

			Standardization EasyBuilderAppToActivate = cust as Standardization;
			if (EasyBuilderAppToActivate != null)
				Standardization.ActivateAndReinitTBActivationInfo();
			else
			{
				//Se abbiamo cancellato una customizzazione allora dobbiamo far ricaricare le informazioni
				//circa i documenti al TB C++ e anche il menu.
				CUtility.ReloadAllMenus();
				//La reload application va ad aggiornare correttamente i file xml di descrizione dei documenti,
				//ma non aggiorna m_pStandardDocumentsTable di application context (che viene caricato una volta 
				//solo all'avvio dell'applicazione e mai aggiornato ed e' usato nella rundocument per cercare la descrizione
				//del documento da lanciare)
				//L'effetto e' che se cancelliamo un'applicazione che ha dei documenti, per la rundocument essi esistono ancora 
				//e non blocca il run.
				//CUtility.ReloadApplication(application);

				//Invalido la cache per far si che vengano ricaricati i menu e rigenerata la dll degli enumerativi
				//al successivo lancio dell'applicazione
				BasePathFinder.BasePathFinderInstance.InstallationVer.UpdateCachedDateAndSave();
			}
		}

		/// <summary>
		/// 
		/// </summary>
		//-----------------------------------------------------------------------------
		public IEasyBuilderApp Import(
			string application,
			string module,
			ApplicationType applicationType
			)
		{
			IEasyBuilderApp app = (applicationType == ApplicationType.Customization) ?
				new Customization(application, module)
				:
				new Standardization(application, module);

			//La LoadCustomList viene fatta anche dal costruttore ma, siccome se non è presente
			//la custom list su disco la LoadCustomList non fa nulla,
			//ci troveremmo con una customizzazione che non ha nessuna custom list caricata.
			//Quindi è necessario ripetere la chiamata qui.
			app.EasyBuilderAppFileListManager.LoadCustomList();

			return app;
		}

		//-----------------------------------------------------------------------------
		internal static IEasyBuilderApp WrapExisting(
			string application,
			string module,
			ApplicationType appType
			)
		{
			IEasyBuilderApp app = null;
			switch (appType)
			{
				case ApplicationType.Customization:
					app = new Customization(application, module);
					break;
				case ApplicationType.Standardization:
					app = new Standardization(application, module);
					break;
				default:
					throw new Exception("Unrecognized EasyBuilder Application type");
			}

			return app;
		}

		/// <summary>
		/// Internal use
		/// </summary>
		//--------------------------------------------------------------------------------
		public void RenameApplication(string oldAppName, string newAppName)
		{
			//Se i nomi sono uguali non faccio niente
			if (oldAppName.CompareNoCase(newAppName))
				return;

			try
			{
                ServicesManager manager = ServicesManager.ServicesManagerInstance;
                ApplicationService appService = manager.GetService(typeof(ApplicationService)) as ApplicationService;
                appService.RenameApplication(oldAppName, newAppName);
            }
			catch (Exception ex)
			{
				BaseCustomizationContext.CustomizationContextInstance.Diagnostic.SetError(Resources.UnableToRenameApplication);
                BaseCustomizationContext.CustomizationContextInstance.Diagnostic.SetError(ex.Message);
                return;
			}

			BaseApplicationInfo bai = (BaseApplicationInfo)BasePathFinder.BasePathFinderInstance.GetApplicationInfoByName(oldAppName);
			if (bai != null)
				bai.Name = newAppName;

			foreach (Customization cust in BaseCustomizationContext.CustomizationContextInstance.FindEasyBuilderAppsByApplicationName(oldAppName))
				cust.RenameApplicationReferencesInModule(cust.ModuleName, oldAppName, newAppName);

	    	CUtility.ReloadApplication(oldAppName);
			CUtility.ReloadApplication(newAppName);

			//Reload del menu.
			CUtility.ReloadAllMenus();
		}

		/// <summary>
		/// Internal use
		/// </summary>
		//--------------------------------------------------------------------------------
		public void PublishMenu(string userFilePath)
		{
			string destinationFilePath = MenuEditorEngine.GetMenuFullPath(CurrentEasyBuilderApp);

			//Creo un file di backup del file corrente per paramento
			CreateFileBackup(userFilePath);

			try
			{
				RemoveFileReadonly(destinationFilePath);

				File.Delete(destinationFilePath);

				//Sposto il file nella cartella per tutti gli utenti
				File.Move(userFilePath, destinationFilePath);
			}
			catch (Exception e)
			{
				Diagnostic.SetError(string.Format(Resources.UnableToMoveFile, userFilePath, destinationFilePath, e.Message));
			}

			//Pubblico anche eventuale altro contenuto della cartella di menù (immagini, ecc)
			PublishMenuContent(userFilePath, destinationFilePath);

			//rimuovo dalla custom list il file pre pubblicazione e salvo
			CurrentEasyBuilderApp.EasyBuilderAppFileListManager.CustomList.Remove(userFilePath);

			//Aggiungo il nuovo file alla customizzazione
			AddToCurrentCustomizationList(destinationFilePath, true);

			//Dopo aver pubblicato il menu lo ricarico.
			new System.Threading.Thread(new System.Threading.ThreadStart(() => CUtility.ReloadAllMenus())).Start();
		}

		/// <summary>
		/// Sposta tutti i file di immagine ecc dalla cartella utente del menù, a quella pubblica
		/// </summary>
		//--------------------------------------------------------------------------------
		private static void PublishMenuContent(string userFilePath, string destinationFilePath)
		{
			if (userFilePath.IsNullOrEmpty())
				return;
			DirectoryInfo di = null;
			try
			{
				di = new DirectoryInfo(Path.GetDirectoryName(userFilePath));
				if (di == null)
					return;

			}
			catch { }

			FileInfo[] files = di.GetFiles("*.*", SearchOption.TopDirectoryOnly);
			foreach (FileInfo fileInfo in files)
			{
				if (!CustomizationContextInstance.CurrentEasyBuilderApp.EasyBuilderAppFileListManager.CustomList.ContainsNoCase(fileInfo.FullName))
					continue;

				//rimuovo dalla custom list il file pre pubblicazione e salvo
				CustomizationContextInstance.CurrentEasyBuilderApp.EasyBuilderAppFileListManager.CustomList.Remove(fileInfo.FullName);

				string newFile = Path.Combine(Path.GetDirectoryName(destinationFilePath), fileInfo.Name);

				try
				{
					if (File.Exists(newFile))
					{
						CustomizationContextInstance.RemoveFileReadonly(newFile);
						File.Delete(newFile);
					}

					//Sposto il file nella cartella per tutti gli utenti
					File.Move(fileInfo.FullName, newFile);
				}
				catch { continue; }

				//Aggiungo il nuovo file alla customizzazione
				CustomizationContextInstance.AddToCurrentCustomizationList(newFile, true);
			}
		}

		/// <summary>
		/// Pubblica la customizzazione spostando i file dalla cartella utente a quella del documento
		/// </summary>
		//--------------------------------------------------------------------------------
		public void PublishDocument(
            string documentFolder,
            string documentFileNameWithoutExtension,
            string user,
            bool isActive
            )
		{
			if (!user.IsNullOrEmpty())
				user = user.Replace("\\", ".");

            var toBePublishedFiles = new DirectoryInfo(documentFolder)
                .GetFiles("*.*", SearchOption.TopDirectoryOnly)
                .Where(
                    f
                    =>
                    {
                        string ext = Path.GetExtension(f.Name);
                        return
                            String.Compare(ext, ".dll", StringComparison.InvariantCultureIgnoreCase) == 0 //||
                            //String.Compare(ext, ".cs", StringComparison.InvariantCultureIgnoreCase) == 0
                            ;
                    })
                    .Select(f => f.FullName);

            //il path dovrebbe essere nella forma "Erp\Accounting\ModuleObjects\Documento\sa
            string[] tokens = documentFolder.Split(new string[] { "\\" }, StringSplitOptions.RemoveEmptyEntries);

            string stringToReplace = Path.Combine(user, documentFileNameWithoutExtension);
            string destinationFilePath = null;
            foreach (var toBePublishedFile in toBePublishedFiles)
            {
				try
				{
					destinationFilePath = toBePublishedFile.ReplaceNoCase(stringToReplace, documentFileNameWithoutExtension);

					//Creo un file di backup del file corrente per paramento
					CreateFileBackup(toBePublishedFile);

					if (File.Exists(destinationFilePath))
						File.Delete(destinationFilePath);

					//Sposto il file nella cartella per tutti gli utenti
					File.Move(toBePublishedFile, destinationFilePath);
				}
				catch (Exception e)
				{
					Diagnostic.SetError(string.Format(Resources.UnableToMoveFile, toBePublishedFile, destinationFilePath, e.Message));
				}

                ICustomListItem custItem = CurrentEasyBuilderApp.EasyBuilderAppFileListManager.CustomList.FindItem(toBePublishedFile);
                string docNamespace = custItem == null ? string.Empty : custItem.DocumentNamespace;
                //rimuovo dalla custom list il file pre pubblicazione e salvo
                CurrentEasyBuilderApp.EasyBuilderAppFileListManager.CustomList.Remove(toBePublishedFile);

                //Aggiungo il nuovo file alla customizzazione
                AddToCurrentCustomizationList(destinationFilePath, true, isActive, "", docNamespace);
            }
		}
	}
}
