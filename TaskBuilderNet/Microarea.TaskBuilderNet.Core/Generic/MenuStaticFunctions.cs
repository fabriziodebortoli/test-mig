using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using Microarea.TaskBuilderNet.Core.MenuManagerLoader;
using Microarea.TaskBuilderNet.Core.NameSolver;

namespace Microarea.TaskBuilderNet.Core.Generic
{
	public class MenuStaticFunctions
	{
		//--------------------------------------------------------------------------------------------------------------------------------
		public static void PingViaSMS()
		{
			string url = BasePathFinder.BasePathFinderInstance.PingViaSMSPage;
			Process.Start(url);
		}
//--------------------------------------------------------------------------------------------------------------------------------
        public static void OpenProducerSite()
		{
            string url = InstallationData.BrandLoader.GetBrandedStringBySourceString("ProducerSite");
			Process.Start(url);
		}
		//----------------------------------------------------------------------------
		public static void ViewLicensesToolStripMenuItem_Click(object sender, EventArgs e)
		{
			// se esiste la cartella di cultura corrente apro quella  (File tradotti installati dal setup)
			//se no  apro i file nella appdata (file intestati  salvati dal login manager in attivazine.)
			string aLicensesPath = BasePathFinder.BasePathFinderInstance.GetLogManAppDataPath();
			if (aLicensesPath == null || aLicensesPath.Length == 0)
				return;

			if (!Directory.Exists(aLicensesPath))
				return;

			DirectoryInfo aLicensesDirectory = new DirectoryInfo(aLicensesPath);
			string culture = Thread.CurrentThread.CurrentUICulture.Name;

			DirectoryInfo[] dirs = aLicensesDirectory.GetDirectories(culture);
			if (dirs.Length > 0 && dirs[0].GetFiles().Length > 0)
				aLicensesDirectory = dirs[0];

			FileInfo[] aLicensesFiles = aLicensesDirectory.GetFiles("*.pdf");//apro solo i pdf);

			if (aLicensesFiles == null || aLicensesFiles.Length == 0)
				return;

			Process aProcess = null;
			bool anErrorOccurred = false;
			foreach (FileInfo aFileInfo in aLicensesFiles)
			{
				try
				{
					aProcess = Process.Start(aFileInfo.FullName);
				}
				catch
				{
					anErrorOccurred = true;
					break;
				}
				finally
				{
					if (aProcess != null)
						aProcess.Dispose();
				}
			}

			if (!anErrorOccurred)
				return;

			try
			{
				aProcess = Process.Start(aLicensesDirectory.FullName);
			}
			finally
			{
				if (aProcess != null)
					aProcess.Dispose();
			}
		}

		//go to producer site = InstallationData.BrandLoader.GetBrandedStringBySourceString("ProducerSite");
		//--------------------------------------------------------------------------------------------------------------------------------
		public static void GoToProducerSite()
		{
			HelpManager.ConnectToProducerSite();
		}

		//  /\
		//  questi due metodi invertiti come da mail di germano del 27/6/11
		//  \/
		//go to private area = BasePathFinder.BasePathFinderInstance.LoginManagerBaseUrl + "SitePrivateArea.aspx" + "?u=" + HttpUtility.UrlEncode(CreateTempToken(LoginManagerPrivateAreaUrl))
		//--------------------------------------------------------------------------------------------------------------------------------
		public static void GoToPrivateArea(string authToken)
		{
			HelpManager.ConnectToProducerSiteLoginPage(authToken);
		}

		//--------------------------------------------------------------------------------------------------------------------------------
		public static bool ProcessOfficeFile(string application, string ns, string subType, string user, string company, string preferredLanguage = "")
		{
			MenuXmlNode.MenuXmlNodeCommandSubType commandSubType = null;
			MenuXmlNode.OfficeItemApplication officeApp = (application.CompareNoCase("Excel")) ? officeApp = MenuXmlNode.OfficeItemApplication.Excel : MenuXmlNode.OfficeItemApplication.Word;

			if (subType.CompareNoCase(MenuXmlNode.MenuXmlNodeCommandSubType.XML_OFFICE_ITEM_SUBTYPE_DOCUMENT))
				commandSubType = new MenuXmlNode.MenuXmlNodeCommandSubType(MenuXmlNode.MenuXmlNodeCommandSubType.XML_OFFICE_ITEM_SUBTYPE_DOCUMENT);

			if (subType.CompareNoCase(MenuXmlNode.MenuXmlNodeCommandSubType.XML_OFFICE_ITEM_SUBTYPE_DOCUMENT_2007))
				commandSubType = new MenuXmlNode.MenuXmlNodeCommandSubType(MenuXmlNode.MenuXmlNodeCommandSubType.XML_OFFICE_ITEM_SUBTYPE_DOCUMENT_2007);

			if (subType.CompareNoCase(MenuXmlNode.MenuXmlNodeCommandSubType.XML_OFFICE_ITEM_SUBTYPE_TEMPLATE))
				commandSubType = new MenuXmlNode.MenuXmlNodeCommandSubType(MenuXmlNode.MenuXmlNodeCommandSubType.XML_OFFICE_ITEM_SUBTYPE_TEMPLATE);

			if (subType.CompareNoCase(MenuXmlNode.MenuXmlNodeCommandSubType.XML_OFFICE_ITEM_SUBTYPE_TEMPLATE_2007))
				commandSubType = new MenuXmlNode.MenuXmlNodeCommandSubType(MenuXmlNode.MenuXmlNodeCommandSubType.XML_OFFICE_ITEM_SUBTYPE_TEMPLATE_2007);

			string officeFileToOpen = String.Empty;

			officeFileToOpen = MenuInfo.GetOfficeItemFileName(ns,
																commandSubType,
																officeApp,
																new PathFinder(company, user),
																preferredLanguage
																);

			if (!preferredLanguage.IsNullOrEmpty())
				officeFileToOpen = MenuInfo.GetLocalizedFileName(officeFileToOpen, preferredLanguage);

			//Gestione voci aggiunte con il menù editor, cerca in allcompanies, nelle sottocartelle word o excel
			if (officeFileToOpen.IsNullOrEmpty())
				officeFileToOpen = MenuInfo.GetMenuEditorOfficeItemFileName(ns, commandSubType, officeApp);

			if (officeFileToOpen != null && officeFileToOpen.Length > 0 && File.Exists(officeFileToOpen))
			{
				try
				{
					ProcessStartInfo pInfo = new ProcessStartInfo();
					//Set the file name member. 
					pInfo.FileName = officeFileToOpen;
					pInfo.UseShellExecute = true;
					Process.Start(pInfo);
					return true;
				}
				catch (Exception)
				{
					return false;
				}
			}
			else
			{
				return false;
			}
		}

	}
}
