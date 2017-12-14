using System;
using System.Collections;
using System.IO;
using Microarea.TaskBuilderNet.Interfaces;

namespace Microarea.TaskBuilderNet.Core.NameSolver
{	
	//=========================================================================
	public class PathFinder : BasePathFinder, IPathFinder
	{
		
		//---------------------------------------------------------------------
		private string					company		= string.Empty;
		public	string					Company		{ get { return company; } }

		//---------------------------------------------------------------------
		private string					user		= string.Empty;
		public	string					User		{ get { return user; } }

		// in alcuni metodi è necessario specificare l'edizione.
		//---------------------------------------------------------------------
		private string					edition		= string.Empty;
		public	string					Edition		{ get { return edition; }  set { edition = value; } }


		//nome utente con la virgola al posto dello \
		//----------------------------------------------------------------------------
		public string UserForFileSystem		
		{
			get 
			{ 
				return User.Replace(Path.DirectorySeparatorChar, DomainUserSeparator);
			} 
		}
        
        bool? singleThreaded;
        //---------------------------------------------------------------------
		public bool SingleThreaded
        {
            get 
            {
                if (singleThreaded == null)
                {
                    SettingItem si = GetSettingItem("Framework", "TBGenlib", "Environment", "SingleThreaded");
                    singleThreaded = si != null && si.Values[0].Equals("1");
                }
                return singleThreaded.Value;
            }
        }
		//---------------------------------------------------------------------
		public PathFinder(string company, string user)
		{	
			this.company	= company;
			this.user		= user;

			if (!Init())
				throw new Exception(Messages.PathFinderInitFailed);
		}

		//---------------------------------------------------------------------
		public PathFinder(string server, string installation, string company, string user)
		{
			this.company = company;
			this.user = user; 

			if (!Init(server, installation))
				throw new Exception(Messages.PathFinderInitFailed);
		}

		/// <summary>
		/// L'array viene riempito di ApplicationInfo anzichè BaseApplicationInfo
		/// posso riempire l'array in base alla configurazione
		/// </summary>
		//----------------------------------------------------------------------------
		public override IList ApplicationInfos
		{ 
			get 
			{ 
				if (applications != null)
					return applications;
				
				applications = new ArrayList();

				if (!AddApplicationsByType(ApplicationType.All))
					return new ArrayList();
				return applications; 
			} 
		}


		/// <summary>
		/// Aggiunge all'arra delle applicazioni ApplicationInfo al posto di BaseApplicationInfo
		/// </summary>
		/// <param name="applicationName"></param>
		/// <param name="applicationType"></param>
		/// <param name="aEnumImages"></param>
		/// <param name="appsPath"></param>
		//---------------------------------------------------------------------
		public override void CreateApplicationInfo
			(
			string			applicationName, 
			ApplicationType applicationType,
			string			appsPath
			)
		{
			if	(
				applicationName == null							||
				applicationName == string.Empty					||
				applicationType == ApplicationType.Undefined	||
				!IsApplicationDirectory(appsPath + Path.DirectorySeparatorChar + applicationName)
				)
				return;
						
			//path del contenitore di applicazioni custom
			string customAppContainerPath = GetCustomApplicationContainerPath(applicationType);
			
			//oggetto contenente le info di un'applicazione
			ApplicationInfo applicationInfo = new ApplicationInfo
				(
				applicationName, 
				appsPath, 
				customAppContainerPath, 
				this
				);

			//aggiungo l'applicazione all'array
			if (applicationInfo.IsKindOf(applicationType))
				applications.Add(applicationInfo);
		}

				
		//-----------------------------------------------------------------------------
		public string GetCustomCompanyPath() 
		{
			return base.GetCustomCompanyPath(Company);
		}

		//-----------------------------------------------------------------------------
		public string GetCustomApplicationContainerPath(ApplicationType aApplicationType)
		{
			return base.GetCustomApplicationContainerPath(Company, aApplicationType);
		}

		/// <summary>
		/// Restituisce il path dell'applicazione nell'istanza custom
		/// </summary>
		/// <param name="applicationName">nome dell'applicazione</param>
		/// <returns>path dell'applicazione</returns>
		//---------------------------------------------------------------------
		public string GetCustomApplicationPath(string applicationName)
		{
			return base.GetCustomApplicationPath(Company, applicationName);
		}

		/// <summary>
		/// Restituisce la path custom di un modulo
		/// </summary>
		/// <param name="applicationName">nome applicazione</param>
		/// <param name="moduleName">nome modulo</param>
		/// <returns>la path del modulo</returns>
		//---------------------------------------------------------------------
		public string GetCustomModulePath(string applicationName, string moduleName)
		{
			return base.GetCustomModulePath(Company, applicationName, moduleName);
		}

		/// <summary>
		/// Restituisce Custom\Contenitore\Applicazione\Modulo\Dictionary
		/// </summary>
		/// <param name="appName">nome dell'applicazione</param>
		/// <param name="moduleName">nome del modulo</param>
		/// <param name="createDir">indica se creare la cartella se non presente</param>
		/// <returns>la pth richiesta</returns>
		//---------------------------------------------------------------------
		public string GetCustomModuleDictionaryPath(string appName, string moduleName, bool createDir)  
		{
			return base.GetCustomModuleDictionaryPath(Company, appName, moduleName, createDir);
		}

		// Custom\AllCompanies\TaskBuilderApplication\Application\Module\DataManager
		//---------------------------------------------------------------------
		public string GetCustomDataManagerPath(string application, string module)
		{
			return base.GetCustomDataManagerPath(Company, application, module);
		}
		
		// Custom\AllCompanies\TaskBuilderApplication\Application\Module\DataManager\Default\<language>\<edition>		
		//---------------------------------------------------------------------
		public string GetCustomDataManagerDefaultPath(string application, string module, string language)
		{
			return base.GetCustomDataManagerDefaultPath(Company, application, module, language, edition);
		}
				
		// Custom\AllCompanies\TaskBuilderApplication\Application\Module\DataManager\Sample\<language>\<edition>		
		//---------------------------------------------------------------------
		public string GetCustomDataManagerSamplePath(string application, string module, string language)
		{
			return base.GetCustomDataManagerSamplePath(Company, application, module, language, edition);
		}
		
		// Standard\TaskBuilderApplication\Application\Module\DataManager\Default\<language>\<edition>		
		//---------------------------------------------------------------------
		public string GetStandardDataManagerDefaultPath(string application, string module, string language)
		{
			return base.GetStandardDataManagerDefaultPath(application, module, language, edition);
		}
		
		// Standard\TaskBuilderApplication\Application\Module\DataManager\Sample\<language>\<edition>	
		//---------------------------------------------------------------------
		public string GetStandardDataManagerSamplePath(string application, string module, string language)
		{
			return base.GetStandardDataManagerSamplePath(application, module, language, edition);
		}
		
		//---------------------------------------------------------------------
		public string GetCustomAppContainerPath(INameSpace aNameSpace)
		{
			return base.GetCustomAppContainerPath(Company, aNameSpace);
		}

		//-----------------------------------------------------------------------------
		private string GetCustomFilename(ModuleInfo aModuleInfo, INameSpace aNamespace, string aUser)
		{
			if (aModuleInfo == null || !aNamespace.IsValid())
				return String.Empty;

			string fullFileName = String.Empty;

			switch (aNamespace.NameSpaceType.Type)
			{
				case NameSpaceObjectType.Report:
				{
					string reportFileName = aNamespace.GetReportFileName();
					if (reportFileName == null || reportFileName == String.Empty)
						return String.Empty;

					if (aUser == null)
						fullFileName = aModuleInfo.GetCustomReportFullFilename(reportFileName);
					else
						fullFileName = aModuleInfo.GetCustomReportFullFilename(reportFileName, aUser);
					break;
				}

				case NameSpaceObjectType.Image:
					fullFileName = aModuleInfo.GetCustomImageFullFilename(aNamespace.Image, aUser);
					break;

				case NameSpaceObjectType.Text:
					fullFileName = aModuleInfo.GetCustomTextFullFilename(aNamespace.Text, aUser);
					break;

                case NameSpaceObjectType.File:
                    fullFileName = aModuleInfo.GetCustomFileFullFilename(aNamespace.File, aUser);
                    break;

				case NameSpaceObjectType.ExcelDocument:
				{
					string documentFileName = aNamespace.GetExcelDocumentFileName();
					if (documentFileName == null || documentFileName == String.Empty)
						return String.Empty;

					fullFileName = aModuleInfo.GetCustomExcelDocumentFullFilename(documentFileName, aUser);
					break;
				}

				case NameSpaceObjectType.ExcelTemplate:
				{
					string templateFileName = aNamespace.GetExcelTemplateFileName();
					if (templateFileName == null || templateFileName == String.Empty)
						return String.Empty;
					
					fullFileName = aModuleInfo.GetCustomExcelTemplateFullFilename(templateFileName, aUser);
					break;
				}
			
				case NameSpaceObjectType.WordDocument:
				{
					string documentFileName = aNamespace.GetWordDocumentFileName();
					if (documentFileName == null || documentFileName == String.Empty)
						return String.Empty;

					fullFileName = aModuleInfo.GetCustomWordDocumentFullFilename(documentFileName, aUser);
					break;
				}
			
				case NameSpaceObjectType.WordTemplate:
				{
					string templateFileName = aNamespace.GetWordTemplateFileName();
					if (templateFileName == null || templateFileName == String.Empty)
						return String.Empty;

					fullFileName = aModuleInfo.GetCustomWordTemplateFullFilename(templateFileName, aUser);
					break;
				}

				case NameSpaceObjectType.ExcelDocument2007:
				{
					string documentFileName = aNamespace.GetExcel2007DocumentFileName();
					if (documentFileName == null || documentFileName == String.Empty)
						return String.Empty;

					fullFileName = aModuleInfo.GetCustomExcelDocument2007FullFilename(documentFileName, aUser);
					break;
				}

				case NameSpaceObjectType.ExcelTemplate2007:
				{
					string templateFileName = aNamespace.GetExcel2007TemplateFileName();
					if (templateFileName == null || templateFileName == String.Empty)
						return String.Empty;

					fullFileName = aModuleInfo.GetCustomExcelTemplate2007FullFilename(templateFileName, aUser);
					break;
				}

				case NameSpaceObjectType.WordDocument2007:
				{
					string documentFileName = aNamespace.GetWord2007DocumentFileName();
					if (documentFileName == null || documentFileName == String.Empty)
						return String.Empty;

					fullFileName = aModuleInfo.GetCustomWordDocument2007FullFilename(documentFileName, aUser);
					break;
				}

				case NameSpaceObjectType.WordTemplate2007:
				{
					string templateFileName = aNamespace.GetWord2007TemplateFileName();
					if (templateFileName == null || templateFileName == String.Empty)
						return String.Empty;

					fullFileName = aModuleInfo.GetCustomWordTemplate2007FullFilename(templateFileName, aUser);
					break;
				}

				case NameSpaceObjectType.DocumentSchema:
					fullFileName = aModuleInfo.GetCustomDocumentSchemaFullFilename(aNamespace.WordDocument, aNamespace.DocumentSchema, aUser);
					break;
				
				case NameSpaceObjectType.ReportSchema:
					fullFileName = aModuleInfo.GetCustomReportSchemaFullFilename(aNamespace.ReportSchema, aUser);
					break;
			}	
			
			if (fullFileName != null && fullFileName != String.Empty && File.Exists(fullFileName))
				return fullFileName;

			return String.Empty;
		}

		//-----------------------------------------------------------------------------
		private string GetCurrentUserCustomFilename(ModuleInfo aModuleInfo, INameSpace aNamespace)
		{
			if 
				(
				user == null || 
				user == String.Empty || 
				String.Compare(user, NameSolverStrings.AllUsers) == 0
				)
				return String.Empty;
			
			return GetCustomFilename(aModuleInfo, aNamespace, user);
		}

		//-----------------------------------------------------------------------------
		private string GetAllUsersCustomFilename(ModuleInfo aModuleInfo, INameSpace aNamespace)
		{
			return GetCustomFilename(aModuleInfo, aNamespace, NameSolverStrings.AllUsers);
		}
		
		//-----------------------------------------------------------------------------
		public string GetFilename(INameSpace aNamespace, ref CommandOrigin aCommandOrigin, string language)
		{
			aCommandOrigin = CommandOrigin.Unknown;

			if (!aNamespace.IsValid())
				return String.Empty;

			ModuleInfo aModuleInfo = (ModuleInfo)GetModuleInfoByName(aNamespace.Application, aNamespace.Module);
			if (aModuleInfo == null)
				return String.Empty;

			// prima prova sullo user corrente
			string fullFileName = GetCurrentUserCustomFilename(aModuleInfo, aNamespace);
			if (fullFileName != null && fullFileName != String.Empty && File.Exists(fullFileName))
			{
				aCommandOrigin = CommandOrigin.CustomCurrentUser;

				return fullFileName;
			}

			// poi su AllUsers
			fullFileName = GetAllUsersCustomFilename(aModuleInfo, aNamespace);
			if (fullFileName != null && fullFileName != String.Empty && File.Exists(fullFileName))
			{
				aCommandOrigin = CommandOrigin.CustomAllUsers;

				return fullFileName;
			}

			// e, infine, sulla standard
			fullFileName = GetStandardFilename(aModuleInfo, aNamespace, language);
			if (fullFileName != null && fullFileName != String.Empty)
			{
				aCommandOrigin = CommandOrigin.Standard;

				return fullFileName;
			}

			return String.Empty;
		}

		//-----------------------------------------------------------------------------
		public string GetFilename(INameSpace aNamespace, string language)
		{
			CommandOrigin commandOrigin = CommandOrigin.Unknown;
			return GetFilename(aNamespace, ref commandOrigin, language);
		}
		
		//---------------------------------------------------------------------------------
		public string GetCustomReportPathFromNamespace(INameSpace ns)
		{			
			return GetCustomReportPathFromNamespace(ns, company, user);
		}
		
		//---------------------------------------------------------------------------------
		public string GetAllUsersCustomReportPathFromNamespace(INameSpace ns)
		{
			return GetCustomReportPathFromNamespace(ns, company, NameSolverStrings.AllUsers);
		}

		//---------------------------------------------------------------------------------
		public string GetCustomExcelPathFromNamespace(INameSpace ns)
		{
			return GetCustomExcelPathFromNamespace(ns, company, user);
		}
		
		//---------------------------------------------------------------------------------
		public string GetAllUsersCustomExcelPathFromNamespace(INameSpace ns)
		{
			return GetCustomExcelPathFromNamespace(ns, company, NameSolverStrings.AllUsers);
		}

		//---------------------------------------------------------------------------------
		public string GetCustomWordPathFromNamespace(INameSpace ns)
		{
			return GetCustomWordPathFromNamespace(ns, company, user);
		}
		
		//---------------------------------------------------------------------------------
		public string GetAllUsersCustomWordPathFromNamespace(INameSpace ns)
		{
			return GetCustomWordPathFromNamespace(ns, company, NameSolverStrings.AllUsers);
		}
		//---------------------------------------------------------------------------------
		public string GetCustomUserApplicationDataPath()
		{
			return String.Concat(
				GetCustomCompanyPath(),
				Path.DirectorySeparatorChar,
				NameSolverStrings.AppData,
				Path.DirectorySeparatorChar,
				User
				);
		}
		//---------------------------------------------------------------------------------
		public string GetCustomUserApplicationStateFile()
		{
			return Path.Combine(GetCustomUserApplicationDataPath(), NameSolverStrings.CustomAppStateFile);
		}

        //---------------------------------------------------------------------------------
        public string GetAllCompaniesUserDefaultThemeFilePath()
        {
            return Path.Combine(
                GetCustomAllCompaniesModulePath(NameSolverStrings.Framework, "TbGeneric"),
                NameSolverStrings.Settings,
                User,
                defaultThemeFileName
                );
        }

        //---------------------------------------------------------------------------------
        public string GetCompanyAllUsersDefaultThemeFilePath()
        {
            return Path.Combine(
                GetCustomModulePath(NameSolverStrings.Framework, "TbGeneric"),
                NameSolverStrings.Settings,
                NameSolverStrings.AllUsers,
                defaultThemeFileName
                );
        }

        //---------------------------------------------------------------------------------
        public string GetCompanyUserDefaultThemeFilePath()
        {
            return Path.Combine(
                GetCustomModulePath(NameSolverStrings.Framework, "TbGeneric"),
                NameSolverStrings.Settings,
                User,
                defaultThemeFileName
                );
        }
    }
}
