using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Xml;
using Microarea.EasyBuilder.BackendCommunication;
using Microarea.EasyBuilder.Licence;
using Microarea.EasyBuilder.Properties;
using Microarea.TaskBuilderNet.Core.EasyBuilder;
using Microarea.TaskBuilderNet.Core.Generic;
using Microarea.TaskBuilderNet.Core.NameSolver;
using Microarea.TaskBuilderNet.Interfaces;
using Microarea.TaskBuilderNet.Licence.Licence;

namespace Microarea.EasyBuilder.Packager
{
	//=========================================================================
	/// <summary>
	/// Rappresenta una personalizzazione i cui files risiedono nella parte
	/// Standard dell'installazione.
	/// Questa personalizzazione è sottoposta a licenza ed attivazione.
	/// Per fare un' analogia, è il corrispondente di EasyBuilder per ciò
	/// che in TaskBuilder C++ si chiama Verticale.
	/// </summary>
	class Standardization : Customization
	{
		public override bool IsSubjectedToLicenceCheck { get { return true; } }
		public override ApplicationType ApplicationType
		{
			get { return ApplicationType.Standardization; }
		}

		//-----------------------------------------------------------------------------
		protected internal Standardization(string application, string module)
			: base(application, module)
		{
			EasyBuilderAppFileListManager = new StandardListManager(application, module, GetEasyBuilderAppFolder());

			EasyBuilderAppFileListManager.LoadCustomList();
		}

		//-----------------------------------------------------------------------------
		public override void Delete()
		{
			base.Delete();

			string solutionFilePath = GetSolutionFilePath();

			//Elimina modulo dal file di solution
			RemoveModuleFromSolutionFile(solutionFilePath, ModuleName);

			//csm e xml per i sales modules sono gi`a rimossi ciclando sulla standard list.
		}

		//--------------------------------------------------------------------------------
		private string GetSolutionFilePath()
		{
			string appFolder = BasePathFinder.BasePathFinderInstance.GetStandardApplicationContainerPath(
						 Microarea.TaskBuilderNet.Interfaces.ApplicationType.Standardization
						 );
			string standardApplicationFolder = Path.Combine(appFolder,ApplicationName);
			string solutionsPath = Path.Combine(standardApplicationFolder,NameSolverStrings.Solutions);
			string solutionFilePath = Path.Combine(solutionsPath,String.Format("{0}{1}",ApplicationName,NameSolverStrings.SolutionExtension));

			return solutionFilePath;
		}

		//-----------------------------------------------------------------------------
		protected override string GetApplicationTypeAsString()
		{
			return NameSolverStrings.EasyBuilderApplication;
		}

		//--------------------------------------------------------------------------------
		protected override string GetEasyBuilderAppFolder()
		{
			string appFolder = Path.Combine
						 (
						 BasePathFinder.BasePathFinderInstance.GetStandardApplicationContainerPath(ApplicationType),
						 ApplicationName,
						 ModuleName
						 );
			return appFolder;
		}

		//--------------------------------------------------------------------------------
		protected override bool IsToBeDeleted(string filePath)
		{
			//l'application.config non lo cancello, non appartiene solo ad un modulo, così come il file il file di Solution
			return
				!filePath.ContainsNoCase(
					NameSolverStrings.SolutionExtension
					)
				&&
				base.IsToBeDeleted(filePath);
		}

		//-----------------------------------------------------------------------------
		public override bool IsInExcludeFileFromEnableDisableList(string file)
		{
			IList<IEasyBuilderApp> easybuilderApp = BaseCustomizationContext.CustomizationContextInstance.FindEasyBuilderAppsByApplicationName(this.ApplicationName);

			//il file di solution non va disabilitato, almeno che non sia l'unica applicazione
			if (file.ContainsNoCase(NameSolverStrings.SolutionExtension) && easybuilderApp.Count > 1)
				return true;

			return base.IsInExcludeFileFromEnableDisableList(file);
		}

		//-----------------------------------------------------------------------------
		public override void CreateNeededFiles()
		{
            base.CreateNeededFiles();

            IBaseApplicationInfo appInfo = BasePathFinder.BasePathFinderInstance.GetApplicationInfoByName(ApplicationName);
            if (appInfo == null)
                return;

			//Creare la cartella Standard\Applications\<newAppName>\Solutions
			string newAppSolutionsPath = Path.Combine
                (
                appInfo.Path,
                NameSolverStrings.Solutions
                );
			if (!Directory.Exists(newAppSolutionsPath))
				Directory.CreateDirectory(newAppSolutionsPath);

			//Creare il file di solution Standard\Applications\<newAppName>\Solutions\<newAppName>.Solution.xml
			string solutionFilePath = Path.Combine(newAppSolutionsPath, String.Format("{0}{1}", ApplicationName, NameSolverStrings.SolutionExtension));
			if (!File.Exists(solutionFilePath))
			{
				CreateSolutionFile(solutionFilePath);
				EasyBuilderAppFileListManager.AddToCustomList(solutionFilePath);
			}
			else
			{
				AddNewModuleIfNeeded(solutionFilePath, ModuleName);
			}

			//Creare la cartella Modules Standard\Applications\<newAppName>\Solutions\Modules
			string newAppModulesPath = Path.Combine(newAppSolutionsPath, NameSolverStrings.Modules);
			if (!Directory.Exists(newAppModulesPath))
				Directory.CreateDirectory(newAppModulesPath);

			CreateAndCryptSalesModuleXmlFile(solutionFilePath,newAppModulesPath);

			CreateLicensedConfigFile();

			CUtility.ReloadApplication(ApplicationName);
			BasePathFinder.BasePathFinderInstance.RefreshEasyBuilderApps(ApplicationType);
		}

		//-----------------------------------------------------------------------------
		private void CreateAndCryptSalesModuleXmlFile(string solutionFilePath,string newAppModulesPath)
		{
			//Creare il file di sales module Standard\Applications\<newAppName>\Solutions\Modules\newModName.xml
			string moduleFilePath = Path.Combine(newAppModulesPath,String.Format("{0}{1}",ModuleName,NameSolverStrings.XmlExtension));

			string modulesFolderPath = Path.GetDirectoryName(moduleFilePath);
			if (!Directory.Exists(modulesFolderPath))
				Directory.CreateDirectory(modulesFolderPath);

			if (!File.Exists(moduleFilePath))
			{
				CreateModuleFile(moduleFilePath);
				EasyBuilderAppFileListManager.AddToCustomList(moduleFilePath);

				string cryptedModuleFilePath = Path.Combine(newAppModulesPath,String.Format("{0}{1}",ModuleName,NameSolverStrings.CsmExtension));
				string solutionName = Path.GetFileName(solutionFilePath);
				int index = 0;
				if ((index = solutionName.IndexOf(NameSolverStrings.SolutionExtension, StringComparison.OrdinalIgnoreCase)) > -1)
					solutionName = solutionName.Substring(0,index);

				try
				{
					CryptModuleFile(solutionName);
					EasyBuilderAppFileListManager.AddToCustomList(cryptedModuleFilePath);
				}
				catch (Exception exc)
				{
					BaseCustomizationContext.CustomizationContextInstance.Diagnostic.SetError(String.Format("Error cryptig sales modules: {0}", exc.ToString()));
				}
			}
		}

		//-----------------------------------------------------------------------------
		private static void AddNewModuleIfNeeded(string solutionFilePath, string newModuleName)
		{
			Product p = Product.FromFile(solutionFilePath);

			if (p == null)
				throw new Exception("Solution.xml file does not exist");

			bool found = false;
			if (p.SalesModules != null && p.SalesModules.Length > 0)
			{
				foreach (ProductSalesModule salesModule in p.SalesModules)
				{
					if (salesModule.Name == newModuleName)
					{
						found = true;
						break;
					}
				}
			}
			if (!found)
			{
				ProductSalesModule newSalesModule = new ProductSalesModule();
				newSalesModule.Name = newModuleName;

				List<ProductSalesModule> temp = null;
				if (p.SalesModules == null)
					temp = new List<ProductSalesModule>();
				else
					temp = new List<ProductSalesModule>(p.SalesModules);

				temp.Add(newSalesModule);

				p.SalesModules = temp.ToArray();

				p.ToFile(solutionFilePath, true);
			}
		}

		//-----------------------------------------------------------------------------
		private static void RemoveModuleFromSolutionFile(string solutionFilePath, string moduleName)
		{
			if (!File.Exists(solutionFilePath))
				return;

			Product p = Product.FromFile(solutionFilePath);

			if (p == null || p.SalesModules == null || p.SalesModules.Length == 0)
				return;

			ProductSalesModule foundModule = null;
			foreach (ProductSalesModule salesModule in p.SalesModules)
			{
				if (salesModule.Name == moduleName)
				{
					foundModule = salesModule;
					break;
				}
			}

			if (foundModule == null)
				return;

			List<ProductSalesModule> temp = new List<ProductSalesModule>(p.SalesModules);
			temp.Remove(foundModule);

			p.SalesModules = temp.ToArray();

			p.ToFile(solutionFilePath, true);
		}

		//-----------------------------------------------------------------------------
		private void CreateLicensedConfigFile()
		{
			string[] modSerialNumbers = null;

			ActivationObject ao = CommonFunctions.GetActivationObject();
			ProductInfo	productInfo = ao.GetProductByName(ApplicationName);

			using (SNGeneratorWrapper snGenerator = new SNGeneratorWrapper(CommonFunctions.GetProxySettings(), null))
			{
				foreach (ArticleInfo art in productInfo.Articles)
				{
					if (art.ShortNames.Count > 0)
					{
						art.SerialList.Clear();

						string appFourChars =
							ApplicationName.Length > 3
							? ApplicationName.Substring(0, 4)
							: ApplicationName.PadRight(4, 'X');


						try
						{
							modSerialNumbers = snGenerator.GetSerialNumbers
												(
												Settings.Default.Username,
												Crypto.Decrypt(Settings.Default.Password),
												ApplicationName,
												String.Format("{0}{1}", appFourChars, art.ShortNames[0]),
												"IT",
												1
												);

						if (modSerialNumbers == null || modSerialNumbers.Length == 0)
							throw new Exception("Serial number generation error");
						}
						catch (Exception exc)
						{
							BaseCustomizationContext.CustomizationContextInstance.Diagnostic.SetError(String.Format("Error generating serial numbers: {0}", exc.ToString()));
							continue;
						}
						

						foreach (string serial in modSerialNumbers)
						{
							if (serial.Length != 16)
							{
								BaseCustomizationContext.CustomizationContextInstance.Diagnostic.SetError("Invalid serial number length");
								return;
							}
						}

						foreach (string sn in modSerialNumbers)
							art.SerialList.Add(new SerialNumberInfo(sn));
						art.Licensed = true;
					}
				}
			}
			//Salvo il licensed
			XmlDocument doc = ProductInfo.WriteToLicensed(productInfo);
			doc.Save(BasePathFinder.BasePathFinderInstance.GetLicensedFile(ApplicationName));
		}

		//-----------------------------------------------------------------------------
		private static void CryptModuleFile(string solutionFileName)
		{
			using (CrypterWrapper crypter = new CrypterWrapper(CommonFunctions.GetProxySettings()))
			{
				string msg = null;
				if (!crypter.CryptSalesModules(Crypto.Decrypt(Settings.Default.Password), Settings.Default.Username, solutionFileName, out msg))
				{
					throw new Exception(msg);
				}
			}
		}

		//-----------------------------------------------------------------------------
		private void CreateSolutionFile(string solutionFilePath)
		{
			Product p = new Product();
			p.Localize = ApplicationName;
			p.ActivationVersion = 2;

			ProductSalesModule m = new ProductSalesModule();
			m.Name = ModuleName;

			p.SalesModules = new ProductSalesModule[] { m };

			p.ToFile(solutionFilePath, true);
		}

		//-----------------------------------------------------------------------------
		private void CreateModuleFile(string moduleFilePath)
		{
			SalesModule m = new SalesModule();
			m.Localize = ModuleName;
			m.InternalCode = Settings.Default.CompanyCode;//TODO MATTEO: verificare che sia valorizzto, altrimenti fare una login per valorizzarlo.

			SalesModuleShortName salesModuleShortName = new SalesModuleShortName();
			salesModuleShortName.Name = GenerateShortName(Path.GetDirectoryName(moduleFilePath));

			m.ShortNames = new SalesModuleShortName[]{ salesModuleShortName};

			SalesModuleApplication application = new SalesModuleApplication();
			application.Container = NameSolverStrings.Applications;
			application.Name = ApplicationName;

			m.Applications = new SalesModuleApplication[] { application };

			SalesModuleApplicationModule module = new SalesModuleApplicationModule();
			module.Name = ModuleName;

			application.Items = new object[] { module };

			m.ToFile(moduleFilePath, true);
		}

		//-----------------------------------------------------------------------------
		private string GenerateShortName(string salesModuleFolder)
		{
			string shortNameMask = String.Format("{0}{{0}}", ModuleName.Substring(0, 3).ToUpperInvariant());
			int i = 0;
			string shortName = null;
			bool nameAlreadyExists = false;
			do
			{
				i++;
				shortName = String.Format(shortNameMask, i);
			} while ((nameAlreadyExists = ExistsShortName(shortName, salesModuleFolder)) && i < 10);

			if (!nameAlreadyExists)
				return shortName;

			shortNameMask = String.Format("{0}{{0}}", ModuleName.Substring(0, 2).ToUpperInvariant());
			i = 9;
			shortName = null;
			nameAlreadyExists = false;
			do
			{
				i++;
				shortName = String.Format(shortNameMask, i);
			} while ((nameAlreadyExists = ExistsShortName(shortName, salesModuleFolder)) && i < 100);

			if (!nameAlreadyExists)
				return shortName;

			throw new Exception("Too many modules, have a break!");
		}

		//-----------------------------------------------------------------------------
		internal static void ActivateAndReinitTBActivationInfo()
		{
			ActivatorWrapper activator = new ActivatorWrapper(CommonFunctions.GetProxySettings());

			try
			{
				activator.Register();

				CUtility.ReinitActivationInfos();

				CUtility.RefreshLogin();
			}
			catch (Exception exc)
			{
				BaseCustomizationContext.CustomizationContextInstance.Diagnostic.SetError(String.Format("Error activating new products: {0}", exc.ToString()));
			}
		}

		//-----------------------------------------------------------------------------
		protected override string GetEasyBuilderAppListFileExtension()
		{
			return NameSolverStrings.StandardListFileExtension;
		}

		//-----------------------------------------------------------------------------
		public override void RenameEasyBuilderApp(string newModuleName)
		{
			if (newModuleName.CompareNoCase(ModuleName))
				return;

			//Elimina modulo dal file di solution
			string solutionFilePath = GetSolutionFilePath();
			RemoveModuleFromSolutionFile(solutionFilePath, ModuleName);

			//Rimozione vecchi file di sales module (xml, csm)
			string modulesFolderPath = Path.Combine(Path.GetDirectoryName(solutionFilePath), NameSolverStrings.Modules);
			string salesModuleXmlPath = Path.Combine(modulesFolderPath, ModuleName);
			salesModuleXmlPath = Path.ChangeExtension(salesModuleXmlPath,NameSolverStrings.XmlExtension);
			//rimuove da standardList e da file system
			EasyBuilderAppFileListManager.RemoveFromCustomListAndFromFileSystem(salesModuleXmlPath);

			string salesModuleCsmPath = Path.ChangeExtension(salesModuleXmlPath, NameSolverStrings.CsmExtension);
			//rimuove da standardList e da file system
			EasyBuilderAppFileListManager.RemoveFromCustomListAndFromFileSystem(salesModuleCsmPath);

			//chiamata al metodo base di customization che lavora su file system
			base.RenameEasyBuilderAppOnFileSystem(newModuleName);
			
			//Aggiunge modulo al file di solution
			AddNewModuleIfNeeded(solutionFilePath, newModuleName);

			//Crea nuovi file csm e xml 
			CreateAndCryptSalesModuleXmlFile(solutionFilePath, modulesFolderPath);
			CreateLicensedConfigFile();

			//Attivazione
			ActivateAndReinitTBActivationInfo();

			CUtility.ReloadApplication(ApplicationName);
		}

		//TODO MATTEO SILVANO: rivedere la logica di controllo dell' esistenza dello short name del sales module.
		//-----------------------------------------------------------------------------
		private static bool ExistsShortName(string shortName, string salesModuleFolder)
		{
			DirectoryInfo modulesDirInfo = new DirectoryInfo(salesModuleFolder);
			if (!modulesDirInfo.Exists)
				return false;

			FileInfo[] salesModules = modulesDirInfo.GetFiles(String.Format("*{0}", NameSolverStrings.XmlExtension), SearchOption.TopDirectoryOnly);
			if (salesModules == null || salesModules.Length == 0)
				return false;


			SalesModule aSm = null;
			bool found = false;
			foreach (FileInfo salesModule in salesModules)
			{
				aSm = SalesModule.FromFile(salesModule.FullName);
				foreach (SalesModuleShortName salesModuleShortName in aSm.ShortNames)
				{
					if (String.Compare(salesModuleShortName.Name, shortName, StringComparison.OrdinalIgnoreCase) == 0)
					{
						found = true;
						break;
					}
				}
			}
			return found;
		}
	}
}
