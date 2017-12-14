using System;
using System.Globalization;
using Microarea.TaskBuilderNet.Core.Generic;
using Microarea.TaskBuilderNet.Interfaces;

namespace Microarea.TaskBuilderNet.Core.NameSolver
{
	/// <summary>
	/// Estensione di BaseModuleInfo con le informazioni relative alla custom
	/// </summary>
	//=========================================================================
	public class ModuleInfo : BaseModuleInfo, IModuleInfo
	{
		#region Costruttori
		
		/// <summary>
		/// Costruttore
		/// </summary>
		/// <param name="aParentApplicationInfo">lìapplacazione a cui appartiene il modulo</param>
		//-------------------------------------------------------------------------------
		public ModuleInfo(string moduleName, ApplicationInfo parentApplicationInfo)
			:
			base(moduleName, parentApplicationInfo)
		{
			
		}
		
		#endregion

		#region proprietà
		//------------------------------------------------------------------------------
		public new PathFinder PathFinder
		{
			get
			{ 
				if (parentApplicationInfo == null)
					return null;

				return (PathFinder)parentApplicationInfo.PathFinder;
			}
		}
		//------------------------------------------------------------------------------
		public override System.Collections.IList WebMethods
		{
			get
			{
				//se sono agganciato al singleton, allora ho i webmethods valorizzati
				if (BasePathFinder.BasePathFinderInstance == PathFinder)
					return webMethods;
				//altrimenti li recupero dal singleton
				return BasePathFinder.BasePathFinderInstance.GetModuleInfo(NameSpace).WebMethods;
			}
		}
		/// <summary>
		/// Path custom del modulo
		/// </summary>
		//------------------------------------------------------------------------------
		public string CustomPath
		{
			get
			{
				if (
					((ApplicationInfo)parentApplicationInfo).CustomPath == null ||
					((ApplicationInfo)parentApplicationInfo).CustomPath.Length == 0
					)
					return string.Empty;

				return
					((ApplicationInfo)parentApplicationInfo).CustomPath +
					System.IO.Path.DirectorySeparatorChar +
					Name;
			}
		}

		#endregion

		#region Funzioni pubbliche
		
		//-------------------------------------------------------------------------------
		public override string GetCustomModuleObjectPath()
		{
			if (CustomPath == null || CustomPath.Length == 0)
				return string.Empty;

			return CustomPath							+ 
				System.IO.Path.DirectorySeparatorChar	+ 
				NameSolverStrings.ModuleObjects;
		}
		
		//-------------------------------------------------------------------------------
		public string GetCustomDocumentPath(string documentName)
		{
			if (documentName == null || documentName == String.Empty)
				return String.Empty;

			string moduleObjectsPath = GetCustomModuleObjectPath();
			if (moduleObjectsPath == null || moduleObjectsPath == String.Empty)
				return String.Empty;

			return  System.IO.Path.Combine(moduleObjectsPath, documentName);
		}

		#region funzioni per i report

		//-------------------------------------------------------------------------------
		public string GetCustomReportPath()
		{
			return System.IO.Path.Combine(CustomPath, NameSolverStrings.Report);
		}

		//-------------------------------------------------------------------------------
		public string GetCustomReportPath(string userName)
		{
			return System.IO.Path.Combine(GetCustomReportPath(), BasePathFinder.GetUserPath(userName));
		}

		//-------------------------------------------------------------------------------
		public string GetCustomReportFullFilename(string report)
		{
			return GetCustomReportFullFilename(report, NameSolverStrings.AllUsers);
		}
		
		//-------------------------------------------------------------------------------
		public string GetCustomReportFullFilename(string report, string user)
		{
			return System.IO.Path.Combine(GetCustomReportPath(user), report);
		}
		
		#endregion

		#region funzioni per i text

		//-------------------------------------------------------------------------------
		public string GetCustomTextPath()
		{
			//TODOBRUNA
			string fileDir = System.IO.Path.Combine(CustomPath, NameSolverStrings.Files);

			return System.IO.Path.Combine(fileDir, NameSolverStrings.Texts);
		}

        //-------------------------------------------------------------------------------
        public string GetCustomFilePath()
        {
            string fileDir = System.IO.Path.Combine(CustomPath, NameSolverStrings.Files);

            return System.IO.Path.Combine(fileDir, NameSolverStrings.Others);
        }

		//-------------------------------------------------------------------------------
		public string GetCustomTextPath(string userName)
		{		
			return System.IO.Path.Combine(GetCustomTextPath(), BasePathFinder.GetUserPath(userName));
		}

        //-------------------------------------------------------------------------------
        public string GetCustomFilePath(string userName)
        {
            return System.IO.Path.Combine(GetCustomFilePath(), BasePathFinder.GetUserPath(userName));
        }

		//-------------------------------------------------------------------------------
		public string GetCustomTextFullFilename(string text)
		{
			return GetCustomTextFullFilename(text, NameSolverStrings.AllUsers);
		}
		
		//-------------------------------------------------------------------------------
		public string GetCustomTextFullFilename(string text, string user)
		{
			return System.IO.Path.Combine(GetCustomTextPath(user), text);
		}

        //-------------------------------------------------------------------------------
        public string GetCustomFileFullFilename(string text)
        {
            return GetCustomFileFullFilename(text, NameSolverStrings.AllUsers);
        }

        //-------------------------------------------------------------------------------
        public string GetCustomFileFullFilename(string text, string user)
        {
            return System.IO.Path.Combine(GetCustomFilePath(user), text);
        }

		#endregion

		#region funzioni per i file di Excel

		//-------------------------------------------------------------------------------
		public string GetCustomExcelFilesPath()
		{
			return System.IO.Path.Combine(CustomPath, NameSolverStrings.Excel);
		}

		//-------------------------------------------------------------------------------
		public string GetCustomExcelFilesPath(string userName)
		{		
			return System.IO.Path.Combine(GetCustomExcelFilesPath(), BasePathFinder.GetUserPath(userName));
		}

		//-------------------------------------------------------------------------------
		public string GetCustomExcelDocumentFullFilename(string document)
		{
			return GetCustomExcelDocumentFullFilename(document, NameSolverStrings.AllUsers);
		}
		
		//-------------------------------------------------------------------------------
		public string GetCustomExcelDocumentFullFilename(string document, string user)
		{
			string fullFilename = System.IO.Path.Combine(GetCustomExcelFilesPath(user), document);

			string extension = System.IO.Path.GetExtension(fullFilename);
			if (extension == null || extension == String.Empty)
				fullFilename += NameSolverStrings.ExcelDocumentExtension;

			return fullFilename;
		}

		//-------------------------------------------------------------------------------
		public string GetCustomExcelDocument2007FullFilename(string document, string user)
		{
			string fullFilename = System.IO.Path.Combine(GetCustomExcelFilesPath(user), document);

			string extension = System.IO.Path.GetExtension(fullFilename);
			if (extension == null || extension == String.Empty)
				fullFilename += NameSolverStrings.Excel2007DocumentExtension;

			return fullFilename;
		}

		//-------------------------------------------------------------------------------
		public string GetCustomExcelTemplateFullFilename(string template)
		{
			return GetCustomExcelTemplateFullFilename(template, NameSolverStrings.AllUsers);
		}
		
		//-------------------------------------------------------------------------------
		public string GetCustomExcelTemplateFullFilename(string template, string user)
		{
			string fullFilename = System.IO.Path.Combine(GetCustomExcelFilesPath(user), template);

			string extension = System.IO.Path.GetExtension(fullFilename);
			if (extension == null || extension == String.Empty)
				fullFilename += NameSolverStrings.ExcelTemplateExtension;

			return fullFilename;
		}

		//-------------------------------------------------------------------------------
		public string GetCustomExcelTemplate2007FullFilename(string template, string user)
		{
			string fullFilename = System.IO.Path.Combine(GetCustomExcelFilesPath(user), template);

			string extension = System.IO.Path.GetExtension(fullFilename);
			if (extension == null || extension == String.Empty)
				fullFilename += NameSolverStrings.Excel2007TemplateExtension;

			return fullFilename;
		}
		
		#endregion
		
		#region funzioni per i file di Word

		//-------------------------------------------------------------------------------
		public string GetCustomWordFilesPath()
		{
			return System.IO.Path.Combine(CustomPath, NameSolverStrings.Word);
		}

		//-------------------------------------------------------------------------------
		public string GetCustomWordFilesPath(string userName)
		{		
			return System.IO.Path.Combine(GetCustomWordFilesPath(), BasePathFinder.GetUserPath(userName));
		}

		//-------------------------------------------------------------------------------
		public string GetCustomWordDocumentFullFilename(string document)
		{
			return GetCustomWordDocumentFullFilename(document, NameSolverStrings.AllUsers);
		}
		
		//-------------------------------------------------------------------------------
		public string GetCustomWordDocumentFullFilename(string document, string user)
		{
			string fullFilename = System.IO.Path.Combine(GetCustomWordFilesPath(user), document);

			string extension = System.IO.Path.GetExtension(fullFilename);
			if (extension == null || extension == String.Empty)
				fullFilename += NameSolverStrings.WordDocumentExtension;

			return fullFilename;
		}

		//-------------------------------------------------------------------------------
		public string GetCustomWordDocument2007FullFilename(string document, string user)
		{
			string fullFilename = System.IO.Path.Combine(GetCustomWordFilesPath(user), document);

			string extension = System.IO.Path.GetExtension(fullFilename);
			if (extension == null || extension == String.Empty)
				fullFilename += NameSolverStrings.Word2007DocumentExtension;

			return fullFilename;
		}

		//-------------------------------------------------------------------------------
		public string GetCustomWordTemplateFullFilename(string template)
		{
			return GetCustomWordTemplateFullFilename(template, NameSolverStrings.AllUsers);
		}
		
		//-------------------------------------------------------------------------------
		public string GetCustomWordTemplateFullFilename(string template, string user)
		{
			string fullFilename = System.IO.Path.Combine(GetCustomWordFilesPath(user), template);

			string extension = System.IO.Path.GetExtension(fullFilename);
			if (extension == null || extension == String.Empty)
				fullFilename += NameSolverStrings.WordTemplateExtension;

			return fullFilename;
		}

		//-------------------------------------------------------------------------------
		public string GetCustomWordTemplate2007FullFilename(string template, string user)
		{
			string fullFilename = System.IO.Path.Combine(GetCustomWordFilesPath(user), template);

			string extension = System.IO.Path.GetExtension(fullFilename);
			if (extension == null || extension == String.Empty)
				fullFilename += NameSolverStrings.Word2007TemplateExtension;

			return fullFilename;
		}

		#endregion

		#region funzioni per gli schemi di documento

		//-------------------------------------------------------------------------------
		public string GetCustomDocumentSchemaFilesPath(string documentName)
		{
			if (documentName == null || documentName == String.Empty)
				return String.Empty;

			string documentPath = GetCustomDocumentPath(documentName);
			if (documentPath == null || documentPath == String.Empty)
				return String.Empty;

			return System.IO.Path.Combine(documentPath, NameSolverStrings.ExportProfiles);
		}

		//-------------------------------------------------------------------------------
		public string GetCustomDocumentSchemaFilesPath(string documentName, string userName)
		{		
			if (documentName == null || documentName == String.Empty || userName == null || userName == String.Empty)
				return String.Empty;
			//return GetCustomDocumentSchemaFilesPath(documentName);
			return System.IO.Path.Combine(GetCustomDocumentSchemaFilesPath(documentName), BasePathFinder.GetUserPath(userName));
		}

		//-------------------------------------------------------------------------------
		public string GetCustomDocumentSchemaFullFilename(string documentName, string schemaName)
		{
			return GetCustomDocumentSchemaFullFilename(documentName, schemaName, NameSolverStrings.AllUsers);
		}
		
		//-------------------------------------------------------------------------------
		public string GetCustomDocumentSchemaFullFilename(string documentName, string schemaName, string user)
		{
			string fullFilename = System.IO.Path.Combine(GetCustomDocumentSchemaFilesPath(documentName, user), schemaName);

			string extension = System.IO.Path.GetExtension(fullFilename);
			if (extension == null || extension == String.Empty)
				fullFilename += NameSolverStrings.SchemaExtension;

			return fullFilename;
		}

		#endregion

		#region funzioni per gli schemi di report

		//-------------------------------------------------------------------------------
		public string GetCustomReportSchemaFilesPath()
		{
			return GetCustomReportPath();
		}

		//-------------------------------------------------------------------------------
		public string GetCustomReportSchemaFilesPath(string userName)
		{		
			if (userName == null || userName == String.Empty)
				return String.Empty;

			return System.IO.Path.Combine(GetCustomReportSchemaFilesPath(), BasePathFinder.GetUserPath(userName));
		}

		//-------------------------------------------------------------------------------
		public string GetCustomReportSchemaFullFilename(string report)
		{
			return GetCustomReportSchemaFullFilename(report, NameSolverStrings.AllUsers);
		}
		
		//-------------------------------------------------------------------------------
		public string GetCustomReportSchemaFullFilename(string report, string user)
		{
			string fullFilename = System.IO.Path.Combine(GetCustomReportSchemaFilesPath(user), report);

			string extension = System.IO.Path.GetExtension(fullFilename);
			if (extension == null || extension == String.Empty)
				fullFilename += NameSolverStrings.SchemaExtension;

			return fullFilename;
		}

		#endregion

		//---------------------------------------------------------------------
		public string GetCustomFontsFullFilename()
		{
			string moduleObjPath = GetCustomModuleObjectPath();

			if (moduleObjPath == string.Empty)
				return string.Empty;

			return moduleObjPath + System.IO.Path.DirectorySeparatorChar + NameSolverStrings.FontsIniFile;
		}

		//---------------------------------------------------------------------
		public string GetCustomFormatsFullFilename()
		{
			string moduleObjPath = GetCustomModuleObjectPath();

			if (moduleObjPath == string.Empty)
				return string.Empty;

			return moduleObjPath + System.IO.Path.DirectorySeparatorChar + NameSolverStrings.FormatsIniFile;
		}

		//-------------------------------------------------------------------------------
		public string GetCustomAllCompaniesUserSettingsPath()
		{
			if (PathFinder.User == null || 
				PathFinder.User == string.Empty
				)
				return "";

			string pathCustom = System.IO.Path.Combine(AllCompaniesCustomPath, NameSolverStrings.Settings);
			pathCustom = System.IO.Path.Combine(pathCustom, NameSolverStrings.Users);
			return System.IO.Path.Combine(pathCustom, PathFinder.User);
		}

		//-------------------------------------------------------------------------------
		public string GetCustomAllCompaniesUserSettingsFullFilename(string settings)
		{
			string path = GetCustomAllCompaniesUserSettingsPath();
			if (path == null || path == string.Empty)
				return "";

			return System.IO.Path.Combine(path, settings);
		}

		//-------------------------------------------------------------------------------
		public string GetCustomCompanyUserSettingsPath()
		{
			if (CustomPath == null || 
				CustomPath == string.Empty || 
				PathFinder.User == null || 
				PathFinder.User == string.Empty ||
				PathFinder.Company == null || 
				PathFinder.Company == string.Empty
				)
				return "";

			string pathCustom = System.IO.Path.Combine(CustomPath, NameSolverStrings.Settings);
			pathCustom = System.IO.Path.Combine(pathCustom, NameSolverStrings.Users);
			return System.IO.Path.Combine(pathCustom, PathFinder.User);
		}

		//-------------------------------------------------------------------------------
		public string GetCustomCompanyUserSettingsPathFullFilename(string settings)
		{
			string path = GetCustomCompanyUserSettingsPath();
			if (path == null || path == string.Empty)
				return "";
			return System.IO.Path.Combine(path, settings);
		}

		//-------------------------------------------------------------------------------
		public string GetCustomCompanyAllUserSettingsPath()
		{
			if (CustomPath == null || 
				CustomPath == string.Empty || 
				PathFinder.User == null || 
				PathFinder.User == string.Empty ||
				PathFinder.Company == null || 
				PathFinder.Company == string.Empty
				)
				return "";

			string pathCustom = System.IO.Path.Combine(CustomPath, NameSolverStrings.Settings);
			return System.IO.Path.Combine(pathCustom, NameSolverStrings.AllUsers);
		}

		//-------------------------------------------------------------------------------
		public string GetCustomCompanyAllUserSettingsPathFullFilename(string settings)
		{
			string path = GetCustomCompanyAllUserSettingsPath();
			if (path == null || path == string.Empty)
				return "";
			return System.IO.Path.Combine(path, settings);
		}

		//-------------------------------------------------------------------------------
		public string GetCustomImagePath()
		{
			//TODOBRUNA
			string fileDir = System.IO.Path.Combine(CustomPath, NameSolverStrings.Files);

			return System.IO.Path.Combine(fileDir, NameSolverStrings.Images);
		}

		//-------------------------------------------------------------------------------
		public string GetCustomImagePath(string userName)
		{
			return System.IO.Path.Combine(GetCustomImagePath(), BasePathFinder.GetUserPath(userName));
		}

		//-------------------------------------------------------------------------------
		public string GetCustomImageFullFilename(string image)
		{
			return GetCustomImageFullFilename(image, NameSolverStrings.AllUsers);
		}
		
		//-------------------------------------------------------------------------------
		public string GetCustomImageFullFilename(string image, string user)
		{
            int pos = image.LastIndexOfOccurrence(".", 2, image.Length - 1);
            if (pos >= 0)
            {
                image = image.Substring(0, pos + 1).Replace('.', '\\') + image.Substring(pos + 1);
            }
            return System.IO.Path.Combine(GetCustomImagePath(user), image);
		}
		
		//-------------------------------------------------------------------------------

		#endregion
	}
	
	//-------------------------------------------------------------------------------
	public class ApplicationInfo : BaseApplicationInfo, IApplicationInfo
	{
		#region proprietà
		//-------------------------------------------------------------------------------
		public new	PathFinder	PathFinder { get { return (PathFinder)pathFinder; } }
		
		//-------------------------------------------------------------------------------
		private		string		customPath = string.Empty;
		public		string		CustomPath { get { return customPath; } }
		#endregion
		
		#region costruttore
		//-------------------------------------------------------------------------------
		public ApplicationInfo(string aName, string standardAppContainer, string customAppContainer, IPathFinder aPathFinder)
			:
			base(aName, standardAppContainer, aPathFinder)
		{
			if (customAppContainer != null && customAppContainer.Length != 0)
				customPath = customAppContainer + System.IO.Path.DirectorySeparatorChar + aName;
		}
		#endregion

		#region funzioni reimplementate
		//-------------------------------------------------------------------------------
		override protected IBaseModuleInfo CreateModuleInfo(string moduleName)
		{
			if (moduleName == null || moduleName == string.Empty)
				return null;

			return new ModuleInfo(moduleName, this);
		}
		
		/// <summary>
		/// Restituisce il modulo contenuto nell'applicazione con il nome
		/// specificato
		/// </summary>
		/// <param name="moduleName"></param>
		/// <returns>il modulo cercato o null</returns>
		//-------------------------------------------------------------------------------
		public new IModuleInfo GetModuleInfoByName(string moduleName)
		{
			foreach(ModuleInfo moduleInfo in Modules)
				if (string.Compare(moduleInfo.Name, moduleName, true, CultureInfo.InvariantCulture) == 0)
					return moduleInfo;

			return null;
		}

		/// <summary>
		/// Restituisce il modulo contenuto nell'applicazione con il title
		/// specificato
		/// </summary>
		/// <param name="moduleName"></param>
		/// <returns>il modulo cercato o null</returns>
		//-------------------------------------------------------------------------------
		public new IModuleInfo GetModuleInfoByTitle(string moduleTitle)
		{
			foreach(ModuleInfo moduleInfo in Modules)
				if (string.Compare(moduleInfo.Title, moduleTitle, true, CultureInfo.InvariantCulture) == 0)
					return moduleInfo;

			return null;
		}
		#endregion
	}
}