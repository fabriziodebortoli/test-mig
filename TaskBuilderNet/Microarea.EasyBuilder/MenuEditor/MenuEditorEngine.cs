using System;
using System.Collections;
using System.ComponentModel;
using System.Diagnostics;
using System.IO;
using System.Xml;
using Microarea.EasyBuilder.ComponentModel;
using Microarea.EasyBuilder.Packager;
using Microarea.EasyBuilder.Properties;
using Microarea.TaskBuilderNet.Core.EasyBuilder;
using Microarea.TaskBuilderNet.Core.Generic;
using Microarea.TaskBuilderNet.Core.MenuManagerLoader;
using Microarea.TaskBuilderNet.Core.NameSolver;
using Microarea.TaskBuilderNet.Interfaces;

namespace Microarea.EasyBuilder.MenuEditor
{
	//=========================================================================
	internal sealed class MenuEditorEngine
	{
		//---------------------------------------------------------------------
        private MenuEditorEngine()
		{ }

		//-----------------------------------------------------------------------------
		internal static String PrepareMenuFileForEasyBuilderApp(IEasyBuilderApp cust, string user)
		{
			//Calcolo il percorso al file dell'utente
			string myMenuFilePath = GetMenuFullPath(cust, user);

			//Se esiste un file per l'utente in questione allora significa che l'utente stesso sta già lavorando su quel menù
			//per cui ne ritorno il percorso.
			if (File.Exists(myMenuFilePath))
				return myMenuFilePath;

			//Altrimenti significa che l'utente in questione non stava lavorando su un file di menu
			//per cui calcolo il percorso al file di menu pubblicato per tutti
			string publishedMenuFilePath = GetMenuFullPath(cust);

			//Se non esiste neanche un file di menu per tutti significa che sono in una situazione vergine, per cui ritorno il percorso
			//al mio file di menu che verrà poi creato dal menu editor.
			if (!File.Exists(publishedMenuFilePath))
			{
				CreateMenuFileIfNecessaryAndAddToCurrentCustomizationContext(cust, myMenuFilePath, null as XmlDocument, user);
				return myMenuFilePath;
			}

			//Se invece il file di menu pubblicato esiste
			//allora devo copiarlo nella cartella specifica dell'utente per rendergli disponibile una copia di lavoro
			//e ne ritorno il percorso.
			DirectoryInfo myMenuDirInfo = new DirectoryInfo(Path.GetDirectoryName(myMenuFilePath));
			if (!myMenuDirInfo.Exists)
				myMenuDirInfo.Create();

			try
			{
				string destination = Path.Combine(myMenuDirInfo.FullName, Path.GetFileName(publishedMenuFilePath));

				//Rimuove l'attributo read-only
				FileInfo fi = new FileInfo(destination);
				if (fi.Exists && fi.IsReadOnly)
					fi.IsReadOnly = false;

				XmlDocument currentPublishedMenu = new XmlDocument();
				currentPublishedMenu.Load(publishedMenuFilePath);
				CreateMenuFileIfNecessaryAndAddToCurrentCustomizationContext(cust, destination, currentPublishedMenu, user);
			}
			catch (Exception exc)
			{
				BaseCustomizationContext.CustomizationContextInstance.Diagnostic.SetError(exc.ToString());
			}

			return myMenuFilePath;
		}

		//---------------------------------------------------------------------
        private static void AddMenuBranches(object targetObject, ArrayList menusToAdd)
        {
            if
                (
                targetObject == null ||
                !(targetObject is MenuGroup || targetObject is MenuBranch) ||
                menusToAdd == null ||
                menusToAdd.Count == 0
                )
                return;

            foreach (MenuXmlNode menuNodeToAdd in menusToAdd)
            {
                if (menuNodeToAdd == null)
                    continue;

                string menuName = menuNodeToAdd.GetNameAttribute();
                string menuTitle = menuNodeToAdd.Title;
                if ((menuName == null || menuName.Length == 0) && (menuTitle == null || menuTitle.Length == 0))
                    continue;

                MenuBranch menuBranch = null;
                if (menuName != null && menuName.Length > 0)
                    menuBranch = (targetObject is MenuGroup) ? ((MenuGroup)targetObject).GetMenuBranch(menuName) : ((MenuBranch)targetObject).GetMenuBranch(menuName);
                else
                    menuBranch = (targetObject is MenuGroup) ? ((MenuGroup)targetObject).GetMenuBranchByTitle(menuTitle) : ((MenuBranch)targetObject).GetMenuBranchByTitle(menuTitle);
                
				if (menuBranch == null)
                {
					menuBranch = new MenuBranch(menuNodeToAdd, targetObject as IContainer);

                    if (targetObject is MenuGroup)
                        ((MenuGroup)targetObject).Add(menuBranch);
                    else
                        ((MenuBranch)targetObject).Add(menuBranch);

                    continue;
                }
				else
					menuBranch.Site = new TBSite(menuBranch, targetObject as IContainer, null, menuBranch.Name);

                AddMenuBranches(menuBranch, menuNodeToAdd.MenuItems);

                ArrayList commandsToAdd = menuNodeToAdd.CommandItems;
				if (commandsToAdd == null && commandsToAdd.Count < 0)
					continue;

                foreach (MenuXmlNode commandNodeToAdd in commandsToAdd)
                {
                    if (commandNodeToAdd == null)
                        continue;

                    string commandTitle = commandNodeToAdd.Title;
                    if (commandTitle == null || commandTitle.Length == 0)
                        continue;

                    MenuCommand menuCommand = menuBranch.GetCommandByTitle(commandTitle);
					if (menuCommand == null)
					{
						menuCommand = GetMenuObjectFromXmlNode(commandNodeToAdd, menuBranch) as MenuCommand;
						menuBranch.Add(menuCommand);
					}
					menuCommand.Site = new TBSite(menuCommand, menuBranch, null, menuCommand.Name);
                }
            }
        }

		//---------------------------------------------------------------------
		public static MenuModel LoadMenuModel(XmlDocument fullMenuXmlDocument)
		{
			if (fullMenuXmlDocument == null)
				return null;

			try
			{
				MenuXmlNode menuRoot = new MenuXmlNode(fullMenuXmlDocument.DocumentElement);
				ArrayList applicationsMenuToLoad = menuRoot.ApplicationsItems;
				if (applicationsMenuToLoad == null && applicationsMenuToLoad.Count <= 0)
					return null;

				MenuModel aMenuModel = new MenuModel();
				aMenuModel.Site = new TBSite(aMenuModel, null, null, MenuXmlNode.XML_TAG_MENU_ROOT);
				aMenuModel.CanRaiseMenuItemEvents = false;

				foreach (MenuXmlNode appNodeToAdd in applicationsMenuToLoad)
				{
					if (appNodeToAdd == null)
						continue;

					string applicationName = appNodeToAdd.GetNameAttribute();
					if (applicationName == null || applicationName.Length == 0)
						continue;

					// Controllo se ho già aggiunto un'applicazione con lo stesso nome
					MenuApplication menuApplication = aMenuModel.GetApplication(applicationName);
					if (menuApplication == null)
					{
						menuApplication = new MenuApplication(appNodeToAdd, aMenuModel);
						aMenuModel.Add(menuApplication);
						continue;
					}

					ArrayList groupsToAdd = appNodeToAdd.GroupItems;
					if (groupsToAdd == null && groupsToAdd.Count <= 0)
						continue;
					
					foreach (MenuXmlNode groupNodeToAdd in groupsToAdd)
					{
						if (groupNodeToAdd == null)
							continue;

						string groupName = groupNodeToAdd.GetNameAttribute();
						if (groupName == null || groupName.Length == 0)
							continue;

						// Controllo se ho già aggiunto un gruppo con lo stesso nome
						MenuGroup menuGroup = menuApplication.GetGroup(groupName);
						if (menuGroup == null)
						{
							menuGroup = new MenuGroup(groupNodeToAdd, menuApplication);
							menuApplication.Add(menuGroup);
							continue;
						}

						AddMenuBranches(menuGroup, groupNodeToAdd.MenuItems);
					}
				}
				
				aMenuModel.CanRaiseMenuItemEvents = true;

				return aMenuModel;
			}
			catch (Exception exception)
			{
				Debug.Fail("Exception raised in TBMenuEditor.LoadMenuModel: " + exception.Message);
				return null;
			}
		}

        //---------------------------------------------------------------------
        internal static BaseMenuItem GetMenuObjectFromXmlNode(MenuXmlNode menuXmlNode, IContainer container)
        {
            if (menuXmlNode == null)
                return null;

            if (menuXmlNode.IsApplication)
                return new MenuApplication(menuXmlNode, container);

            if (menuXmlNode.IsGroup)
				return new MenuGroup(menuXmlNode, container);

            if (menuXmlNode.IsMenu)
                return new MenuBranch(menuXmlNode, container);

            if (menuXmlNode.IsCommand)
            {
                if (menuXmlNode.IsRunDocument)
                    return new DocumentMenuCommand(menuXmlNode, container);

                if (menuXmlNode.IsRunReport)
					return new ReportMenuCommand(menuXmlNode, container);

                if (menuXmlNode.IsRunBatch)
					return new BatchMenuCommand(menuXmlNode, container);

                if (menuXmlNode.IsRunFunction)
					return new FunctionMenuCommand(menuXmlNode, container);

                if (menuXmlNode.IsRunText)
					return new TextMenuCommand(menuXmlNode, container);

                if (menuXmlNode.IsRunExecutable)
					return new ExeMenuCommand(menuXmlNode, container);

                if (menuXmlNode.IsOfficeItem)
					return new OfficeItemMenuCommand(menuXmlNode, container);
            }

            return null;
        }

		//
		//--------------------------------------------------------------------------------
		internal static void RemoveDocumentFromMenuFile(
			IEasyBuilderApp easyBuilderApp,
			string documentNamespace,
			string currentUser
			)
		{
			string menuFilePath = PrepareMenuFileForEasyBuilderApp(
				easyBuilderApp,
				currentUser
				);
			if (!File.Exists(menuFilePath))
				return;

			XmlDocument menuDoc = new XmlDocument();
			menuDoc.Load(menuFilePath);

			string xpath= String.Format("//{0}[{1} = '{2}']", MenuXmlNode.XML_TAG_DOCUMENT, MenuXmlNode.XML_TAG_OBJECT, documentNamespace);
			XmlNodeList toBeRemovedNode = menuDoc.DocumentElement.SelectNodes(
				xpath
				);

			if (toBeRemovedNode == null || toBeRemovedNode.Count != 1)
			{
				BaseCustomizationContext.CustomizationContextInstance.Diagnostic.SetError(Resources.MenuItemNotFound);
				return;
			}
			toBeRemovedNode[0].ParentNode.RemoveChild(toBeRemovedNode[0]);
			menuDoc.Save(menuFilePath);

            //Invalidiamo la cache del menu affinchè venga ricaricato presentando nascondendo il menu apportato dal modulo.
            try
            {
                BasePathFinder.BasePathFinderInstance.InstallationVer.UpdateCachedDateAndSave();
            }
            catch (ApplicationException ex)
            {
                // se non si aggiorna il file installation.ver non mi arrabbio, do messaggio e continuo
                Debug.Assert(false, ex.ToString());
            }
			CUtility.ReloadAllMenus();
		}

		//<Document><Title localizable="true">{Title}</Title><Object>{DocumentNamespace}</Object></Document>
		//--------------------------------------------------------------------------------
		internal static void CreateMenuFileIfNecessaryAndAddToCurrentCustomizationContext(
			string documentTitle,
			string documentNamespace,
			string currentUser, 
			bool isBatch
			)
		{
			string menuFilePath = PrepareMenuFileForEasyBuilderApp(
				BaseCustomizationContext.CustomizationContextInstance.CurrentEasyBuilderApp,
				currentUser
				);
			XmlDocument workingDoc = new XmlDocument();
			if (!File.Exists(menuFilePath))
			{
				workingDoc.LoadXml(String.Format("<{0} />", MenuXmlNode.XML_TAG_MENU_ROOT));
				string menuFileDir = Path.GetDirectoryName(menuFilePath);
				if (!Directory.Exists(menuFileDir))
					Directory.CreateDirectory(menuFileDir);

				workingDoc.Save(menuFilePath);
			}
			else
				workingDoc.Load(menuFilePath);

			string owner = MenuEditorEngine.GetMyOwnerString();
			XmlElement documentElem = workingDoc.CreateElement(isBatch ? MenuXmlNode.XML_TAG_BATCH : MenuXmlNode.XML_TAG_DOCUMENT);
			documentElem.SetAttribute(MenuXmlNode.XML_ATTRIBUTE_OWNER, owner);

			XmlElement titleElem = workingDoc.CreateElement(MenuXmlNode.XML_TAG_TITLE);
			titleElem.SetAttribute(MenuXmlNode.XML_ATTRIBUTE_LOCALIZABLE, "true");
			titleElem.InnerText = documentTitle;

			documentElem.AppendChild(titleElem);

			XmlElement objectElem = workingDoc.CreateElement(MenuXmlNode.XML_TAG_OBJECT);
			objectElem.InnerText = documentNamespace;

			documentElem.AppendChild(objectElem);

			try
			{
				new DomUpdater(workingDoc).AddNode(
					String.Format(
                        "/AppMenu/Application[@name=\"{0}\" and @owner=\"{1}\"]/Group[@name=\"Group1\" and @owner=\"{1}\"]/Menu[@name=\"Menu1\" and @owner=\"{1}\"]/Menu[@name=\"Menu11\" and @owner=\"{1}\"]",
						BaseCustomizationContext.CustomizationContextInstance.CurrentEasyBuilderApp.ApplicationName,
						owner
						),
					"",
					documentElem
					);
			}
			catch
			{}
			
			workingDoc.Save(menuFilePath);
            try
            {
                BasePathFinder.BasePathFinderInstance.InstallationVer.UpdateCachedDateAndSave();
            }
            catch (ApplicationException ex)
            {
                // se non si aggiorna il file installation.ver non mi arrabbio, do messaggio e continuo
                Debug.Assert(false, ex.ToString());
            }

            CUtility.ReloadAllMenus();
		}

		//---------------------------------------------------------------------
		private static void CreateMenuFileIfNecessaryAndAddToCurrentCustomizationContext(
			IEasyBuilderApp easyBuilderApp,
			string filePath,
			XmlDocument menuXmlDoc,
			string publishedUser
			)
		{
			if (menuXmlDoc == null)
			{
				menuXmlDoc = new XmlDocument();
				XmlElement appMenuEl = menuXmlDoc.CreateElement(MenuXmlNode.XML_TAG_MENU_ROOT);
				menuXmlDoc.AppendChild(appMenuEl);
			}

			string menuFileDir = Path.GetDirectoryName(filePath);
			if (!Directory.Exists(menuFileDir))
				Directory.CreateDirectory(menuFileDir);

			menuXmlDoc.Save(filePath);

			BaseCustomizationContext.CustomizationContextInstance.AddToCurrentCustomizationList(filePath, true, false, easyBuilderApp.ApplicationType == ApplicationType.Customization ? publishedUser : String.Empty);// le standardizzazioni sono pubblicate di default.
		}

		//--------------------------------------------------------------------------------
		internal static void RenameMenuModule(
			string menuFileFullPath,
			string applicationName,
			string newModuleName,
			string oldModuleName
			)
		{
			if (
				menuFileFullPath.IsNullOrEmpty() ||
				applicationName.IsNullOrEmpty() ||
				newModuleName.IsNullOrEmpty() ||
				oldModuleName.IsNullOrEmpty()
				)
				return;

			if (!File.Exists(menuFileFullPath))
				return;

			XmlDocument menuDoc = new XmlDocument();
			menuDoc.Load(menuFileFullPath);

			XmlNodeList toBeUpdatedNodes = menuDoc.DocumentElement.SelectNodes(
				String.Format("//{0}[contains(.,'.{1}.')]", MenuXmlNode.XML_TAG_OBJECT, oldModuleName)
				);

			string workingString = null;
			int firstIndexOfDot = -1;
			int secondIndexOfDot = -1;
			foreach (XmlNode xmlNode in toBeUpdatedNodes)
			{
				if (xmlNode == null || (workingString = xmlNode.InnerText) == null)
					continue;

				firstIndexOfDot = workingString.IndexOf('.');
				if (firstIndexOfDot < 0)
					continue;

				secondIndexOfDot = workingString.IndexOf('.', firstIndexOfDot + 1);
				if (secondIndexOfDot < 0)
					continue;

				workingString = workingString.Remove(firstIndexOfDot + 1, secondIndexOfDot - firstIndexOfDot - 1);
				workingString = workingString.Insert(firstIndexOfDot + 1, newModuleName);

				xmlNode.InnerText = workingString;
			}

			string newOwnerString = MenuEditorEngine.FormatMyOwnerString(applicationName, newModuleName);

			int firstIndexOfComma = newOwnerString.IndexOf(',');
			if (firstIndexOfComma < 0)
				return;

			string oldOwnerString = newOwnerString.Remove(firstIndexOfComma + 1);
			oldOwnerString = oldOwnerString.Insert(firstIndexOfComma + 1, oldModuleName);

			RenameMenuOwner(menuDoc, newOwnerString, oldOwnerString);

			menuDoc.Save(menuFileFullPath);
		}

		/// <summary>
		/// Returns the full path of the menù related to the specified customization
		/// User can be null or empty and the fullpath will be the menu (eg AllCompanies\Applications\App1\Mod1\Menu\Menu.Menu)
		/// otherwise will contain the user subfolder (eg AllCompanies\Applications\App1\Mod1\Menu\sa\Menu.Menu)
		/// </summary>
		//-----------------------------------------------------------------------------
		internal static string GetMenuFullPath(IEasyBuilderApp cust, string user = "")
		{
			string appRootPath = null;
			switch (cust.ApplicationType)
			{
				case ApplicationType.Customization:
					appRootPath = BasePathFinder.BasePathFinderInstance.GetCustomApplicationsPath();
					break;
				case ApplicationType.Standardization:
					appRootPath = BasePathFinder.BasePathFinderInstance.GetStandardApplicationContainerPath(ApplicationType.Standardization);
					break;
				default:
					throw new ArgumentException("Unrecognized EasyBuilder Application type");
			}
			return (cust.ApplicationType == ApplicationType.Standardization || user.IsNullOrEmpty())
				? Path.Combine
					(
					appRootPath,
					cust.ApplicationName,
					cust.ModuleName,
					NameSolverStrings.Menu,
					String.Format("{0}{1}", NameSolverStrings.Menu, NameSolverStrings.MenuExtension)
					)
				: Path.Combine
						(
						appRootPath,
						cust.ApplicationName,
						cust.ModuleName,
						NameSolverStrings.Menu,
						user,
						String.Format("{0}{1}", NameSolverStrings.Menu, NameSolverStrings.MenuExtension)
						);
		}

		//--------------------------------------------------------------------------------
		internal static void RenameMenuApplication(
			string menuFileFullPath,
			string moduleName,
			string newApplicationName,
			string oldApplicationName
			)
		{
			if (
				menuFileFullPath.IsNullOrEmpty() ||
				newApplicationName.IsNullOrEmpty() ||
				oldApplicationName.IsNullOrEmpty()
				)
				return;

			if (!File.Exists(menuFileFullPath))
				return;

			XmlDocument menuDoc = new XmlDocument();
			menuDoc.Load(menuFileFullPath);

			DomUpdater upd = new DomUpdater(menuDoc);

			try
			{
				upd.UpdateNodeProperty(
					newApplicationName,
					XmlNodeType.Attribute,
					MenuXmlNode.XML_ATTRIBUTE_NAME,
					String.Format(
						"/{0}/{1}[@{2}='{3}']",
						MenuXmlNode.XML_TAG_MENU_ROOT,
						MenuXmlNode.XML_TAG_APPLICATION,
						MenuXmlNode.XML_ATTRIBUTE_NAME,
						oldApplicationName
						)
					);
			}
			catch
			{}

			XmlNodeList toBeUpdatedNodes = menuDoc.DocumentElement.SelectNodes(
				String.Format("//{0}[starts-with(.,'{1}.')]", MenuXmlNode.XML_TAG_OBJECT, oldApplicationName)
				);

			string workingString = null;
			int firstIndexOfDot = -1;
			foreach (XmlNode xmlNode in toBeUpdatedNodes)
			{
				if (xmlNode == null || (workingString = xmlNode.InnerText) == null)
					continue;

				firstIndexOfDot = workingString.IndexOf('.');
				if (firstIndexOfDot < 0)
					continue;

				workingString = workingString.Remove(0, firstIndexOfDot);
				workingString = workingString.Insert(0, newApplicationName);

				xmlNode.InnerText = workingString;
			}

			string newOwnerString = MenuEditorEngine.FormatMyOwnerString(newApplicationName, moduleName);

			int firstIndexOfComma = newOwnerString.IndexOf(',');
			if (firstIndexOfComma < 0)
				return;

			string oldOwnerString = newOwnerString.Remove(0, firstIndexOfComma);
			oldOwnerString = oldOwnerString.Insert(0, oldApplicationName);

			RenameMenuOwner(menuDoc, newOwnerString, oldOwnerString);

			menuDoc.Save(menuFileFullPath);
		}

		//-----------------------------------------------------------------------------
		/// <remarks />
		internal static void RenameMenuOwner(
			XmlDocument menuDoc,
			string newOwnerString,
			string oldOwnerString
			)
		{
			if (menuDoc == null)
				return;

			XmlNodeList toBeUpdatedNodes = menuDoc.DocumentElement.SelectNodes(
				String.Format("//*[@{0}='{1}']", MenuXmlNode.XML_ATTRIBUTE_OWNER, oldOwnerString)
				);

			foreach (XmlNode xmlNode in toBeUpdatedNodes)
			{
				if (xmlNode == null)
					continue;

				XmlAttribute ownerAttribute = xmlNode.Attributes[MenuXmlNode.XML_ATTRIBUTE_OWNER];
				if (ownerAttribute == null)
					continue;

				ownerAttribute.Value = newOwnerString;
			}
		}

		//-----------------------------------------------------------------------------
		/// <remarks />
		public static string GetMyOwnerString()
		{
			return FormatMyOwnerString(
				BaseCustomizationContext.CustomizationContextInstance.CurrentEasyBuilderApp.ApplicationName,
				BaseCustomizationContext.CustomizationContextInstance.CurrentEasyBuilderApp.ModuleName
				);
		}

		//--------------------------------------------------------------------------------
		private static string FormatMyOwnerString(
			string applicationName,
			string moduleName
			)
		{
			return String.Concat(applicationName, ",", moduleName);
		}
	}
}
