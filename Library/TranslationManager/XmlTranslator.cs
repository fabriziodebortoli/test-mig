using System;
using System.Diagnostics;
using System.Xml;
using System.IO;
using System.Collections;
using System.Data;

using Microarea.TaskBuilderNet.Core.NameSolver;
using Microarea.Library.TranslationManager;
using Microarea.TaskBuilderNet.Interfaces;
using Microarea.TaskBuilderNet.Core.Generic;

namespace Microarea.Library.TranslationManager
{
	/// <summary>
	/// Summary description for TranslateModule.
	/// </summary>
	public class XmlTranslator : BaseTranslator
	{
		//---------------------------------------------------------------------------
		public XmlTranslator()
		{
		
		}

		protected TranslationManager	tm;

		//---------------------------------------------------------------------------
		public override string ToString()
		{
			return "Xml translator";
		}

		//---------------------------------------------------------------------------
		public override void Run(TranslationManager	tm)
		{
			this.tm = tm;

			SetProgressMessage("	Traduzione application.config");
			TranslateApplicationConfig		(tm.GetNewApplicationInfo());

			foreach (BaseModuleInfo mi in tm.GetNewApplicationInfo().Modules)
			{
				SetProgressMessage("Traduzione xml del modulo " + mi.Name);
				
				SetProgressMessage("	Traduzione module.config");
				TranslateModuleConfig		(mi);
				
				SetProgressMessage("	Traduzione ModuleObjects ");
				TranslateModuleObjects		(mi);
				
				SetProgressMessage("	Traduzione ReferenceObjects ");
				TranslateReferenceObjects	(mi);
				
				SetProgressMessage("	Traduzione Menu ");
				
				TranslateMenus				(mi);
				
				SetProgressMessage("	Traduzione DBFiles ");
				TranslateDBFiles			(mi);

				SetProgressMessage("	Traduzione Migrations ");
				TranslateMigration			(mi);

				SetProgressMessage("	Traduzione Settings ");
				TranslateSettings			(mi);

				SetProgressMessage("	Traduzione DataFiles ");
				TranslateDataFiles			(mi);

				SetProgressMessage("Traduzione xml del modulo " + mi.Name + " completata");
			}

			SetProgressMessage("	Traduzione Setting delle Combo ");
			IPathFinder pathFinder = tm.GetNewApplicationInfo().PathFinder;
			if (pathFinder != null)
			{
				IBaseApplicationInfo ai = pathFinder.GetApplicationInfoByName("Framework");
				if (ai != null)
				{
					IBaseModuleInfo mi = ai.GetModuleInfoByName("TbGenlib");
					if (mi != null)
						TranslateComboSettings(mi);
				}
			}

			EndRun(false);
		}

		//---------------------------------------------------------------------------
		private bool TranslateModuleConfig(BaseModuleInfo mi)
		{
			string fileName = mi.ModuleConfigInfo.ModuleConfigFile;
			if (!File.Exists(fileName))
				return true;

			XmlDocument doc = new XmlDocument();
			try
			{
				doc.Load(fileName);
			}
			catch(Exception exc)
			{
				Debug.Fail(exc.Message);
				SetProgressMessage(exc.Message);
				return false;
			}

			XmlNodeList nodeList = doc.SelectNodes("ModuleInfo/Components/Library[@name]");
			foreach (XmlElement el in nodeList)
			{
				string oldNS = el.GetAttribute("name");
				string newNS = string.Empty;
				if (oldNS != string.Empty)
				{
					newNS = tm.GetLibraryNameTranslation(mi.Name, oldNS);
					el.SetAttribute("name", newNS);
				}

				oldNS = el.GetAttribute("sourcefolder");
				if (oldNS != string.Empty)
				{
					newNS = tm.GetLibraryTranslationFolder(oldNS);
					el.SetAttribute("sourcefolder", newNS);
				}
			}

			try
			{
				doc.Save(fileName);
			}
			catch(Exception exc)
			{
				Debug.Fail(exc.Message);
				SetProgressMessage(exc.Message);
				return false;
			}

			return true;
		}

		//---------------------------------------------------------------------------
		private bool TranslateApplicationConfig(IBaseApplicationInfo ai)
		{
			if (ai== null)
				return false;

			string applicationConfigFile =	ai.Path		+ 
				System.IO.Path.DirectorySeparatorChar	+ 
				NameSolverStrings.application			+ 
				NameSolverStrings.ConfigExtension;

			TranslateNamespace(
				applicationConfigFile, 
				new string[] {"ApplicationInfo/DbSignature"});

			return true;
		}

		//---------------------------------------------------------------------------
		private bool TranslateModuleObjects(BaseModuleInfo mi)
		{
			TranslateNamespace(
				mi.GetAddOnDatabaseObjectsPath(), 
				new string[] {	 "AddOnDatabaseObjects/AdditionalColumns/Table/@namespace",
								 "AddOnDatabaseObjects/AdditionalColumns/Table/AlterTable/@namespace"});

			TranslateNamespace(
				mi.GetClientDocumentObjectsPath(), 
				new string[] {	 "ClientDocumentObjects/ClientDocuments/ServerDocument/ClientDocument/@namespace", 
								 "ClientDocumentObjects/ClientDocuments/ServerDocument/@namespace"});
			
			TranslateNamespace(
				mi.GetDatabaseObjectsPath(),
				new string[] {	"DatabaseObjects/Tables/Table/@namespace", 
								"DatabaseObjects/Signature", 
								"DatabaseObjects/Views/View/@namespace",
								"DatabaseObjects/Release",
								"//Create/@release"});
			
			TranslateNamespace(
				mi.GetDocumentObjectsPath(), 
				"DocumentObjects/Documents/Document/@namespace");			

			TranslateNamespace(
				mi.GetEventHandlerObjectsPath(), 
				"FunctionObjects/Functions/Function/@namespace");

			TranslateNamespace(
				mi.GetWebMethodsPath(), 
				"FunctionObjects/Functions/Function/@namespace");

			TranslateXTech(mi);

			return true;
		}

		//---------------------------------------------------------------------------
		private bool TranslateProfile(string xProfilePath)
		{
			DirectoryInfo xpdi = new DirectoryInfo(xProfilePath);
			string xProfileName = xpdi.Name;
						
			string DocumentFile			= Path.Combine(xProfilePath,	"Document.xml");
			TranslateNamespace(
				DocumentFile, 
				new string[]{"Document/DataUrl", "Document/EnvelopeClass"});

			string ExternalReferencesFile	= Path.Combine(xProfilePath,	"ExternalReferences.xml");
			TranslateNamespace(
				ExternalReferencesFile, 
				new string[]{	"MainExternalReferences/DBT/@namespace",
								"MainExternalReferences/DBT/ExternalReferences/ExternalReference/@namespace",
								"//DataUrl",
								"//Name",
								"MainExternalReferences/DBT/ExternalReferences/ExternalReference/ProfileName"});

			string FieldsFile				= Path.Combine(xProfilePath,	"Field.xml");
			TranslateNamespace(
				FieldsFile, 
				"DBTS/DBT/@namespace");

			string newProfName = tm.GetNameSpaceConversion(LookUpFileType.Profile, xProfileName);
						
			try
			{
				if (xProfilePath != Path.Combine(xpdi.Parent.FullName, newProfName))
					Directory.Move(xProfilePath, Path.Combine(xpdi.Parent.FullName, newProfName));
			}
			catch (Exception exc)
			{
				Debug.Fail(exc.Message);
				SetLogError(exc.Message, ToString());
				return false;
			}

			return true;
		}

		//---------------------------------------------------------------------------
		private bool TranslateXTech(BaseModuleInfo mi)
		{
			string[] xdocs = mi.XTechDocuments;
			foreach (string xdocPath in xdocs)
			{
				DirectoryInfo di = new DirectoryInfo(xdocPath);
				string xdocName = di.Name;
				
				string xDescriptionFolder		= Path.Combine(xdocPath,		"Description");
				
				string DbtsFile					= Path.Combine(xDescriptionFolder,	"Dbts.xml");
				TranslateNamespace(
					DbtsFile, 
					new string[]{	"//UniversalKey/@ukname",
									"DBTs/Master/@namespace", 
									"DBTs/Master/Table/@namespace", 
									"DBTs/Master/Slaves/Slave/@namespace",
									"DBTs/Master/Slaves/Slave/Table/@namespace",
									"DBTs/Master/Slaves/Slavable/@namespace",
									"DBTs/Master/Slaves/Slavable/Table/@namespace",
									"DBTs/Master/Slaves/SlaveBuffered/@namespace",
									"DBTs/Master/Slaves/SlaveBuffered/Table/@namespace",
									"DBTs/Slaves/Slave/@namespace",
									"DBTs/Slaves/Slave/Table/@namespace",
									"DBTs/Slaves/SlaveBuffered/@namespace",
									"DBTs/Slaves/SlaveBuffered/Table/@namespace",
									"DBTs/Slaves/Slavable/@namespace",
									"DBTs/Slaves/Slavable/Table/@namespace"
								});
				
				string DocumentFile				= Path.Combine(xDescriptionFolder,	"Document.xml");
				TranslateNamespace(
					DocumentFile, 
					new string[]{"Document/DataUrl", "Document/EnvelopeClass"});

				string DefaultsFile				= Path.Combine(xDescriptionFolder,	"Defaults.xml");
				TranslateNamespace(
					DefaultsFile, 
					"Defaults/DocumentNamespace");

				string ExternalReferencesFile	= Path.Combine(xDescriptionFolder,	"ExternalReferences.xml");
				TranslateNamespace(
					ExternalReferencesFile, 
					new string[]{	"MainExternalReferences/DBT/@namespace",
									"MainExternalReferences/DBT/ExternalReferences/ExternalReference/@namespace",
									"//DataUrl",
									"//Name"});

				string ReportsFile				= Path.Combine(xDescriptionFolder,	"Reports.xml");
				TranslateNamespace(
					ReportsFile, 
					new string[]{	"ReportObjects/Reports/@defaultReport",
									"ReportObjects/Reports/Report/@namespace"});	

				string xProfilesFolder			= Path.Combine(xdocPath,		"ExportProfiles");
				if (Directory.Exists(xProfilesFolder))
				{
					string[] prifilesPaths = Directory.GetDirectories(xProfilesFolder);
					foreach (string xProfilePath in prifilesPaths)
					{
						string[] subF = Directory.GetDirectories(xProfilePath);
						if (subF.Length == 0)
							TranslateProfile(xProfilePath);
						else
						{
                            //sono in una cartella client doc
							foreach (string xClientDocProfilePath in subF)
								TranslateProfile(xClientDocProfilePath);
							
							DirectoryInfo cdDI = new DirectoryInfo(xProfilePath);
							//rinomino la cartella di client doc
							string newCDName = tm.GetNameSpaceConversion(LookUpFileType.Documents, cdDI.Name);
			
							try
							{
								if (xProfilePath != Path.Combine(cdDI.Parent.FullName, newCDName))
									Directory.Move(xProfilePath, Path.Combine(cdDI.Parent.FullName, newCDName));
							}
							catch (Exception exc)
							{
								Debug.Fail(exc.Message);
								SetLogError(exc.Message, ToString());
								return false;
							}
						}
					}
				}

				string newDocName = tm.GetConversion(LookUpFileType.Documents, xdocName, mi.Name);
				if (newDocName == xdocName)
					newDocName = tm.GetConversion(LookUpFileType.ClientDocuments, xdocName, mi.Name);
				string newDocPath = Path.Combine(di.Parent.FullName, newDocName);
				if (string.Compare(xdocPath, newDocPath, true) != 0)
				{
					try
					{
						Directory.Move(xdocPath, newDocPath);
					}
					catch (Exception exc)
					{
						Debug.Fail(exc.Message);
						SetLogError(exc.Message, ToString());
					}
				}
			}

			return true;
		}

		//---------------------------------------------------------------------------
		private bool TranslateMenus(BaseModuleInfo mi)
		{
			string menuPath = Path.Combine(mi.Path, "menu");
			if (!Directory.Exists(menuPath))
				return true;

			string[] menuFiles = Directory.GetFiles(menuPath, "*.menu");

			foreach (string menuFile in menuFiles)
			{
				FileInfo fi = new FileInfo(menuFile);
				string menuName = fi.Name;

				TranslateNamespace(
					menuFile, 
					new string[]{	"AppMenu/Application/@name",
									"AppMenu/Application/Group/@name",
									"//Report/Object",
									"//Document/Object",
									"//Batch/Object",
									"//Function/Object",
								});

				XmlDocument menuDoc = new XmlDocument();
				try
				{
					menuDoc.Load(menuFile);
				}
				catch(Exception exc)
				{
					Debug.Fail(exc.Message);
					SetLogError(exc.Message, ToString());
					continue;
				}

				XmlNodeList activationElements = menuDoc.SelectNodes("//@activation");
				foreach (XmlNode activationElement in activationElements)
				{
					string tmp = activationElement.InnerText;
					tmp = tmp.Replace("!", "");
					tmp = tmp.Replace("(", "");
					tmp = tmp.Replace(")", "");
					string[] activationStrings = tmp.Split(new char[]{'&', '|'});
					foreach (string activationString in activationStrings)
					{
						string[] ss = activationString.Split('.');
						if (ss.Length != 2)
						{
							Debug.Assert(false);
							continue;
						}
						string newAppName = tm.GetNameSpaceConversion(LookUpFileType.Application, ss[0]);
						string newModName = tm.GetNameSpaceConversion(LookUpFileType.Module, ss[1]);
						if (newModName == ss[1])
							newModName = tm.GetNameSpaceConversion(LookUpFileType.Activation, ss[1]);

						activationElement.InnerText = activationElement.InnerText.Replace(ss[0], newAppName);
						activationElement.InnerText = activationElement.InnerText.Replace(ss[1], newModName);
					}
				}

				menuDoc.Save(menuFile);

				if (string.Compare(menuName, mi.Name, true) != 0)
				{
					string tmp = fi.Name.Replace(".menu", "");
					string newName = tm.GetMenuTranslation(tmp);
					string newFileName = Path.Combine(fi.Directory.FullName, newName) + ".menu";
					if (string.Compare(menuFile, newFileName, true) != 0)
					{
						try
						{
							fi.MoveTo(newFileName);
						}
						catch (Exception exc)
						{
							Debug.Fail(exc.Message);
							SetLogError(exc.Message, ToString());
						}
					}
				}

				//rinominazione gif
				string[] gifFiles = Directory.GetFiles(menuPath, "*.gif");
				foreach (string gifFile in gifFiles)
				{
					FileInfo fiGif = new FileInfo(gifFile);
					string tmp = fiGif.Name.Replace(".gif", "");
					string newName = tm.GetNameSpaceConversion(LookUpFileType.Gif, tmp);
					string newFileName = Path.Combine(fiGif.Directory.FullName, newName) + ".gif";
					
					if (string.Compare(newFileName, gifFile, true) != 0)
					{
						try
						{
							fiGif.MoveTo(newFileName);
						}
						catch (Exception exc)
						{
							Debug.Fail(exc.Message);
							SetLogError(exc.Message, ToString());
						}
					}
				}
			}

			return true;
		}

		//---------------------------------------------------------------------------
		private bool TranslateDBFiles(BaseModuleInfo mi)
		{
			string createInfoFile = mi.PathFinder.GetStandardCreateInfoXML(mi.ParentApplicationName, mi.Name);
			TranslateNamespace(
				createInfoFile, 
				new string[] {	 "CreateInfo/ModuleInfo/@name",
								 "//Dependency/@app",
								 "//Dependency/@module"});
			
			string upgradeInfoFile = mi.PathFinder.GetStandardUpgradeInfoXML(mi.ParentApplicationName, mi.Name);
			TranslateNamespace(
				upgradeInfoFile, 
				"UpgradeInfo/ModuleInfo/@name");

			return true;
		}

		//---------------------------------------------------------------------------
		private bool TranslateMigration(BaseModuleInfo mi)
		{
			string migrationXPPath = mi.GetMigrationXpPath();
			string migrationNETPath = mi.GetMigrationNetPath();

			string SecurityMigrationInfoFile = Path.Combine(migrationXPPath, "SecurityMigrationInfo.xml");
			TranslateNamespace(
				SecurityMigrationInfoFile, 
				new string[]{
						"Objects/Object[@type='3']/@namespace",
						"Objects/Object[@type='4']/@namespace",
						"Objects/Object[@type='5']/@namespace",
						"Objects/Object[@type='7']/@namespace",
						"Objects/Object[@type='10']/@namespace",
						"Objects/Object[@type='11']/@namespace",
						"Objects/Object[@type='21']/@namespace"
							});

			string LinkFormFile = Path.Combine(migrationXPPath, "LinkForms.xml");
			TranslateNamespace(
				LinkFormFile, 
				"LinkForms/LinkForm/@newName");

			string LinkReportFile = Path.Combine(migrationXPPath, "LinkReports.xml");
			TranslateNamespace(
				LinkReportFile, 
				"LinkReports/LinkReport/@newName");

			string HotLinksFile = Path.Combine(migrationXPPath, "HotLinks.xml");
			TranslateNamespace(
				HotLinksFile, 
				"HotLinks/HotLink/@newName");

			string FunctionsFile = Path.Combine(migrationXPPath, "Functions.xml");
			TranslateNamespace(
				FunctionsFile, 
				"Functions/Function/@newName");

			string HotLinksFileNET = Path.Combine(migrationNETPath, "HotLinks.xml");
			TranslateNamespace(
				HotLinksFileNET, 
				"HotLinks/HotLink/@newName");

			string FunctionsFileNET = Path.Combine(migrationNETPath, "Functions.xml");
			TranslateNamespace(
				FunctionsFileNET, 
				"Functions/Function/@newName");

			string DocumentsFileNET = Path.Combine(migrationNETPath, "Documents.xml");
			TranslateNamespace(
				DocumentsFileNET, 
				"Documents/Document/@newName");

			string ApplicationFileNET = Path.Combine(migrationNETPath, "Libraries.xml");
			TranslateNamespace(
				ApplicationFileNET, 
				"Libraries/Library/@newName");

			string ReportsFileNET = Path.Combine(migrationNETPath, "Reports.xml");
			TranslateNamespace(
				ReportsFileNET, 
				"Reports/Report/@newName");

			string ActivationsFileNET = Path.Combine(migrationNETPath, "Activations.xml");
			TranslateNamespace(
				ActivationsFileNET, 
				new string[]{
						"Activations/Activation/@newName",
						"Activations/@newAppName"
							});

			return true;
		}

		//---------------------------------------------------------------------------
		private bool TranslateReferenceObjects(BaseModuleInfo mi)
		{
			string refPath = mi.GetReferenceObjectsPath();

			if (!Directory.Exists(refPath))
				return true;

			string[] refObjFiles = Directory.GetFiles(refPath, "*.xml");

			foreach (string refObjFile in refObjFiles)
			{
				string renamed = TranslateNamespace(refObjFile, "HotKeyLink/Function/@namespace");
				renamed = renamed.Replace(".xml", "");
				string newFileName = Path.Combine(refPath, GetLastToken(renamed)) + ".xml";
				
				if (string.Compare(newFileName, refObjFile, true) != 0)
				{
					try
					{
						File.Move(refObjFile, newFileName);
					}
					catch (Exception exc)
					{
						Debug.Fail(exc.Message);
						SetLogError(exc.Message, ToString());
					}
				}
			}
	
			return true;
		}
		
		//---------------------------------------------------------------------------
		private bool TranslateSettings(BaseModuleInfo mi)
		{
			string setPath = mi.GetStandardSettingsPath();

			if (!Directory.Exists(setPath))
				return true;

			string[] setFiles = Directory.GetFiles(setPath, "*.config");

			foreach (string setFile in setFiles)
			{
				TranslateNamespace(setFile, new string[] {
									 "ParameterSettings/Section/@name",
									 "ParameterSettings/Section/Setting/@name"});


				DirectoryInfo di = new DirectoryInfo(setFile);
				string newSetName = tm.GetNameSpaceConversion(LookUpFileType.SettingFile, di.Name);
						
				try
				{
					if (setFile != Path.Combine(di.Parent.FullName, newSetName))
						Directory.Move(setFile, Path.Combine(di.Parent.FullName, newSetName));
				}
				catch (Exception exc)
				{
					Debug.Fail(exc.Message);
					SetLogError(exc.Message, ToString());
					return false;
				}

			}
	
			return true;
		}

		//---------------------------------------------------------------------------
		private bool TranslateComboSettings(IBaseModuleInfo mi)
		{
			string setPath = mi.GetStandardSettingsPath();

			if (!Directory.Exists(setPath))
				return true;

			string[] setFiles = Directory.GetFiles(setPath, "*.config");

			foreach (string setFile in setFiles)
			{
				TranslateNamespace(setFile, "ParameterSettings/Section/Setting[@name='HotlinkComboDefaultFields']/@value");
			}
	
			return true;
		}

		//---------------------------------------------------------------------------
		private bool TranslateDataFiles(BaseModuleInfo mi)
		{
			string datafileFolderName = Path.Combine(Path.Combine(mi.Path, "DataManager"), "DataFile");

			if (!Directory.Exists(datafileFolderName))
				return true;

			DirectoryInfo dfDI = new DirectoryInfo(datafileFolderName);
			foreach (DirectoryInfo di in dfDI.GetDirectories())
			{
				foreach (FileInfo fi in di.GetFiles("*.xml"))
				{
					TranslateNamespace(fi.FullName, new string[] {
																 "//Fieldtype/@name",
																 "//Field/@name"});


					string oldDatafileName = fi.Name.Substring(0, fi.Name.IndexOf("."));
					string newDatafileName = tm.GetNameSpaceConversion(LookUpFileType.DataFile, oldDatafileName) + ".xml";
						
					try
					{
						if (fi.FullName != Path.Combine(Path.Combine(datafileFolderName, di.Name), newDatafileName))
							File.Move(fi.FullName, Path.Combine(Path.Combine(datafileFolderName, di.Name), newDatafileName));
					}
					catch (Exception exc)
					{
						Debug.Fail(exc.Message);
						SetLogError(exc.Message, ToString());
						return false;
					}

				}
			}
	
			return true;
		}

		//---------------------------------------------------------------------------
		private LookUpFileType GetLookUpType(string xPathQuery)
		{
			switch(xPathQuery)
			{
				case "DBTs/Master/@namespace":
				case "DBTs/Master/Slaves/Slave/@namespace":
				case "DBTs/Master/Slaves/SlaveBuffered/@namespace":
				case "DBTs/Master/Slaves/Slavable/@namespace":
				case "MainExternalReferences/DBT/@namespace":
				case "DBTS/DBT/@namespace":
					return LookUpFileType.Dbts;

				case "DBTs/Slaves/Slave/@namespace":
				case "DBTs/Slaves/SlaveBuffered/@namespace":
				case "DBTs/Slaves/Slavable/@namespace":
					return LookUpFileType.CDDbts;

				case "AddOnDatabaseObjects/AdditionalColumns/Table/@namespace":
				case "DatabaseObjects/Tables/Table/@namespace":
				case "DatabaseObjects/Views/View/@namespace":
				case "Objects/Object[@type='10']/@namespace":
				case "DBTs/Master/Table/@namespace":
				case "DBTs/Slaves/Slave/Table/@namespace":
				case "DBTs/Slaves/SlaveBuffered/Table/@namespace":
				case "DBTs/Slaves/Slavable/Table/@namespace":
				case "DBTs/Master/Slaves/Slave/Table/@namespace":
				case "DBTs/Master/Slaves/SlaveBuffered/Table/@namespace":
				case "DBTs/Master/Slaves/Slavable/Table/@namespace":
					return LookUpFileType.Tables;

				case "AddOnDatabaseObjects/AdditionalColumns/Table/AlterTable/@namespace":
				case "Libraries/Library/@newName":
					return LookUpFileType.Structure;

				case "documentnamespace":
				case "MainExternalReferences/DBT/ExternalReferences/ExternalReference/@namespace":
				case "ClientDocumentObjects/ClientDocuments/ServerDocument/@namespace":
				case "DocumentObjects/Documents/Document/@namespace":
				case "Documents/Document/@newName":
				case "Objects/Object[@type='5']/@namespace":
				case "Objects/Object[@type='7']/@namespace":
				case "Objects/Object[@type='21']/@namespace":
					return LookUpFileType.Documents;

				case "ClientDocumentObjects/ClientDocuments/ServerDocument/ClientDocument/@namespace":
					return LookUpFileType.ClientDocuments;

				case "Defaults/DocumentNamespace":
				case "//Batch/Object":
				case "//Document/Object":
				case "LinkForms/LinkForm/@newName":
					return LookUpFileType.Documents;

				case "ReportObjects/Reports/@defaultReport":
				case "ReportObjects/Reports/Report/@namespace":
				case "//Report/Object":
				case "LinkReports/LinkReport/@newName":
				case "Reports/Report/@newName":
				case "Objects/Object[@type='4']/@namespace":
					return LookUpFileType.Report;

				case "CreateInfo/ModuleInfo/@name":
				case "UpgradeInfo/ModuleInfo/@name":
				case "DatabaseObjects/Signature":
				case "//Dependency/@module":
					return LookUpFileType.Module;

				case "FunctionObjects/Functions/Function/@namespace":
				case "//Function/Object":
				case "Functions/Function/@newName":
				case "Objects/Object[@type='3']/@namespace":
					return LookUpFileType.WebMethods;

				case "HotKeyLink/Function/@namespace":
				case "HotLinks/HotLink/@newName":
				case "Objects/Object[@type='11']/@namespace":
					return LookUpFileType.ReferenceObjects;

				case "Document/DataUrl":
				case "//DataUrl":
				case "//Name":
					return LookUpFileType.XTech;

				case "Document/EnvelopeClass":
				case "SalesModule/Application/Module/@name":
					return LookUpFileType.Module;

				case "AppMenu/Application/@name":
				case "ApplicationInfo/DbSignature":
				case "//Dependency/@app":
				case "SalesModule/Application/@name":
				case "Activations/@newAppName":
					return LookUpFileType.Application;

				case "AppMenu/Application/Group/@name":
					return LookUpFileType.Group;

				case "MainExternalReferences/DBT/ExternalReferences/ExternalReference/ProfileName":
					return LookUpFileType.XTech;
			
				case "//UniversalKey/@ukname":
					return LookUpFileType.UniveralKey; 

				case "DatabaseObjects/Release":
				case "//Create/@release":
					return LookUpFileType.Version;

				case "//SalesModule/@name":
				case "SalesModule/SalesModuleDependency/@name":
					return LookUpFileType.SalesModule;

				case "SalesModule/Application/Functionality/@name":
				case "Activations/Activation/@newName":
					return LookUpFileType.Activation;

				case "ParameterSettings/Section/@name":
					return LookUpFileType.SettingSection;

				case "ParameterSettings/Section/Setting/@name":
					return LookUpFileType.SettingName;

				case "//Fieldtype/@name":
				case "//Field/@name":
					return LookUpFileType.DataFileElement;

				case "ParameterSettings/Section/Setting[@name='HotlinkComboDefaultFields']/@value":
					return LookUpFileType.ComboSettingValue;
			}
	
			Debug.Assert(false);
			return LookUpFileType.Invalid;
		}

		//---------------------------------------------------------------------------
		private string GetLastToken(string ns)
		{
			if (ns == null || ns == string.Empty)
				return string.Empty;

			string[] tokens = ns.Split('.');

			if (tokens.Length < 1)
				return string.Empty;

			return tokens[tokens.Length - 1];
		}

		//---------------------------------------------------------------------------
		protected string TranslateNamespace(string fileName, string xPathQuery)
		{
			string[] xPathQueries = new string[1];
			xPathQueries[0] = xPathQuery;
			return TranslateNamespace(fileName, xPathQueries);
		}

		//---------------------------------------------------------------------------
		protected string TranslateNamespace(string fileName, string[] xPathQueries)
		{
			if (!File.Exists(fileName))
				return string.Empty;
			
			XmlDocument doc = new XmlDocument();
			try
			{
				doc.Load(fileName);
			}
			catch(Exception exc)
			{
				Debug.Fail(exc.Message);
				SetLogError(exc.Message, ToString());
				return string.Empty;
			}

			string retVal = string.Empty;
			foreach (string xPathQuery in xPathQueries)
			{
				XmlNodeList nodeList = doc.SelectNodes(xPathQuery);
				if (nodeList.Count == 0)
					continue;

				foreach (XmlNode el in nodeList)
				{
					string tmp = el.InnerText;
					string ex = string.Empty;
					if (tmp.EndsWith(".xml") && xPathQuery.EndsWith("DataUrl"))
					{
						tmp = tmp.Replace(".xml", "");
						ex = ".xml";
					}

					retVal = el.InnerText = tm.GetNameSpaceConversion(GetLookUpType(xPathQuery), tmp) + ex;
				}
			}
			
			try
			{
				File.SetAttributes(fileName, FileAttributes.Normal);
				doc.Save(fileName);
			}
			catch(Exception exc)
			{
				Debug.Fail(exc.Message);
				SetLogError(exc.Message, ToString());
				return string.Empty;
			}

			return retVal;
		}
	}
}
