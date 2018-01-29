using Microarea.TaskBuilderNet.Core.DiagnosticManager;
using Microarea.TaskBuilderNet.Core.Generic;
using Microarea.TaskBuilderNet.Core.NameSolver;
using Microarea.TaskBuilderNet.Interfaces;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;
using System.Text;

namespace Microarea.TaskBuilderNet.Core.EasyBuilder
{
	//=========================================================================
	public class BaseContextFunctions
	{
		private IDiagnostic diagnostic;

		private IList<BaseContextItem> easyBuilderApplications;

		/// <summary>
		/// Internal Use
		/// </summary>
		private string currentApplication = null;

		/// <summary>
		/// Internal Use
		/// </summary>
		private string currentModule = null;

		/// <summary>
		/// L'applicazione corrente della customizzazione
		/// </summary>
		//-----------------------------------------------------------------------------
		public string CurrentApplication { get { return currentApplication; } set { currentApplication = value; } }

		/// <summary>
		/// Il modulo corrente della customizzazione
		/// </summary>
		//-----------------------------------------------------------------------------
		public string CurrentModule { get { return currentModule; } set { currentModule = value; } }


		protected static readonly object lockObject = new object();

		//-----------------------------------------------------------------------------
		/// <summary>
		/// Internal use - Gets a Diagnostic to log errors.
		/// </summary>
		public IDiagnostic Diagnostic
		{
			get
			{
				lock (lockObject)
				{
					if (diagnostic == null)
						diagnostic = new Diagnostic("CustomizationManager");

					return diagnostic;
				}
			}
		}

		/// <summary>
		/// 
		/// </summary>
		/// <returns></returns>
		public string GetAllApps()
		{
			StringBuilder sb = new StringBuilder();
			StringWriter sw = new StringWriter(sb);
			JsonWriter jsonWriter = new JsonTextWriter(sw);
			jsonWriter.WriteStartObject();
			jsonWriter.WritePropertyName("Customizations");
			jsonWriter.WriteStartArray();


			foreach (var ebApp in BaseCustomizationContext.CustomizationContextInstance.EasyBuilderApplications)
			{
				if (ebApp == null || ebApp.ModuleInfo == null)
					continue;
				jsonWriter.WriteValue(ebApp.ApplicationName);
			}
			jsonWriter.WriteEndArray();
			jsonWriter.WriteEndObject();

			string output = sw.ToString();

			jsonWriter.Close();
			sw.Close();

			return sw.ToString();
		}














		//-----------------------------------------------------------------------------
		internal static BaseContextItem WrapExisting(
			string application,
			string module
			)
		{
			return new BaseContextItem(application, module);
		}
		/// <summary>
		/// Ritorna una lista delle customizzazioni attive
		/// </summary>
		//-----------------------------------------------------------------------------
		public IList<BaseContextItem> EasyBuilderApplications
		{
			get
			{
				lock (lockObject)
				{
					if (easyBuilderApplications != null)
						return easyBuilderApplications;

					easyBuilderApplications = new List<BaseContextItem>();

					foreach (EasyBuilderAppDetails current in GetAllEasyBuilderAppsFileListPath())
						easyBuilderApplications.Add(WrapExisting(current.Application, current.Module));

					return easyBuilderApplications;
				}
			}
		}

		/// <summary>
		/// internal use
		/// </summary>
		//-----------------------------------------------------------------------------
		public IEasyBuilderApp FindEasyBuilderApp(string customListFile)
		{
			foreach (IEasyBuilderApp item in EasyBuilderApplications)
			{
				if (item.EasyBuilderAppFileListManager.CustomListFullPath.CompareNoCase(customListFile))
					return item;
			}
			return null;
		}

		/// <summary>
		/// internal use
		/// </summary>
		//-----------------------------------------------------------------------------
		public IEasyBuilderApp FindEasyBuilderApp(string appName, string modName)
		{
			foreach (IEasyBuilderApp item in EasyBuilderApplications)
			{
				if (modName.CompareNoCase(item.ModuleName) && appName.CompareNoCase(item.ApplicationName))
					return item;
			}
			return null;
		}

		/// <summary>
		/// internal use
		/// </summary>
		//-----------------------------------------------------------------------------
		public IList<IEasyBuilderApp> FindEasyBuilderAppsByApplicationName(string appName)
		{
			List<IEasyBuilderApp> custs = new List<IEasyBuilderApp>();
			foreach (IEasyBuilderApp item in EasyBuilderApplications)
			{
				if (appName.CompareNoCase(item.ApplicationName))
					custs.Add(item);
			}
			return custs;
		}

		/// <summary>
		/// internal use
		/// </summary>
		//-----------------------------------------------------------------------------
		public bool IsApplicationAlreadyExisting(string applicationName)
		{
			foreach (EasyBuilderAppDetails item in GetAllEasyBuilderAppsFileListPath())
			{
				if (item.Application.CompareNoCase(applicationName))
					return true;
			}
			return false;
		}

		/// <summary>
		/// Returns full file paths for all the custom list.
		/// </summary>
		/// <returns>Full file paths for all the custom list,
		/// either in the 'Custom' folder or in the 'Standard' folder</returns>
		//-----------------------------------------------------------------------------
		public IList<EasyBuilderAppDetails> GetAllEasyBuilderAppsFileListPath()
		{
			List<EasyBuilderAppDetails> easyBuilderAppDetailsList = new List<EasyBuilderAppDetails>();

			try
			{
				//directory AllCompanies\Applications
				string appsPath = BasePathFinder.BasePathFinderInstance.GetCustomApplicationsPath();

				//Cerco tutti le sottodirectory
				if (!Directory.Exists(appsPath))
					Directory.CreateDirectory(appsPath);

				string[] dirs = Directory.GetDirectories(appsPath);
				easyBuilderAppDetailsList.AddRange(GetAllEasyBuilderAppDetailsFromFileSystem(dirs, ApplicationType.Customization));

				dirs = Directory.GetDirectories(appsPath);
				easyBuilderAppDetailsList.AddRange(GetAllEasyBuilderAppDetailsFromFileSystem(dirs, ApplicationType.Standardization));

				return easyBuilderAppDetailsList;
			}
			catch
			{
				return easyBuilderAppDetailsList;
			}
		}

		//-----------------------------------------------------------------------------
		private IList<EasyBuilderAppDetails> GetAllEasyBuilderAppDetailsFromFileSystem(
			string[] dirs,
			ApplicationType appType
			)
		{
			string searchCriteria =
				appType == ApplicationType.Customization
				?
				NameSolverStrings.CustomListFileSearchCriteria
				:
				NameSolverStrings.StandardListFileSearchCriteria;

			string searchExtension =
				appType == ApplicationType.Customization
				?
				NameSolverStrings.CustomListFileExtension
				:
				NameSolverStrings.StandardListFileExtension;

			List<EasyBuilderAppDetails> customizationDetailsList = new List<EasyBuilderAppDetails>();
			foreach (string dir in dirs)
			{
				//per ogni sottodirectory cerco i file filtrandoli per "*.customList.xml" o "*.standardList.xml"
				if (!Directory.Exists(dir))
					continue;

				DirectoryInfo aDirInfo = new DirectoryInfo(dir);
				string application, module = null;
				FileInfo[] fileInfos = aDirInfo.GetFiles(searchCriteria, SearchOption.TopDirectoryOnly);
				if (fileInfos != null && fileInfos.Length > 0)
				{
					foreach (FileInfo aFileInfo in fileInfos)
					{
						application = aFileInfo.Directory.Name;
						module = aFileInfo.Name.ReplaceNoCase(searchExtension, string.Empty);

						customizationDetailsList.Add(new EasyBuilderAppDetails(application, module, appType));
					}
				}
			}

			return customizationDetailsList;
		}

		/// <summary>
		/// Returns all EasyBuilderApps given the application name.
		/// </summary>
		/// <param name="application">The application name to be searhed.</param>
		/// <param name="applicationType">Type of application</param>
		//-----------------------------------------------------------------------------
		public IList<BaseContextItem> GetEasyBuilderApps(string application, ApplicationType applicationType)
		{
			IList<BaseContextItem> easyBuilderApps = new List<BaseContextItem>();

			if (String.IsNullOrWhiteSpace(application) || EasyBuilderApplications.Count == 0)
				return easyBuilderApps;

			if (applicationType != ApplicationType.Customization && applicationType != ApplicationType.Standardization)
				return easyBuilderApps;

			foreach (var easyBuilderApp in EasyBuilderApplications)
				if (easyBuilderApp.ApplicationName.CompareNoCase(application) && easyBuilderApp.ApplicationType == applicationType)
					easyBuilderApps.Add(easyBuilderApp);

			return easyBuilderApps;
		}

		/// <summary>
		/// Returns full file paths for all the custom list.
		/// </summary>
		/// <param name="application">The application name to be searhed.</param>
		/// <returns>Full file paths for all the custom list.</returns>
		//-----------------------------------------------------------------------------
		public List<string> GetAllEasyBuilderAppsFileListPath(string application)
		{
			List<string> customListFilesPath = new List<string>();

			try
			{
				string appsPath = null;
				string path = null;
				string[] files = null;
				IBaseApplicationInfo appInfo = BasePathFinder.BasePathFinderInstance.GetApplicationInfoByName(application);
				switch (appInfo.ApplicationType)
				{
					case ApplicationType.Customization:
						{
							//directory AllCompanies\Applications
							appsPath = BasePathFinder.BasePathFinderInstance.GetCustomApplicationsPath();

							path = Path.Combine(appsPath, application);
							if (Directory.Exists(path))
							{
								files = Directory.GetFiles(path, NameSolverStrings.CustomListFileSearchCriteria);
								if (files.Length > 0)
								{
									customListFilesPath.AddRange(files);
									return customListFilesPath;
								}
							}
							break;
						}
					case ApplicationType.Standardization:
						{
							appsPath = BasePathFinder
								.BasePathFinderInstance
								.GetStandardApplicationContainerPath(ApplicationType.Standardization);

							path = Path.Combine(appsPath, application);
							if (!Directory.Exists(path))
								return customListFilesPath;

							files = Directory.GetFiles(path, NameSolverStrings.StandardListFileSearchCriteria);
							if (files.Length > 0)
								customListFilesPath.AddRange(files);

							break;
						}
					default:
						break;
				}

				return customListFilesPath;
			}
			catch (Exception exc)
			{
				BaseCustomizationContext.CustomizationContextInstance.Diagnostic.SetError(exc.ToString());
				return customListFilesPath;
			}
		}


		//-----------------------------------------------------------------------------
		private int FindTokenIndex(string[] tokens, string tokenToFind)
		{
			int tokenFolder = -1;
			for (int i = 0; i < tokens.Length; i++)
			{
				if (tokens[i].CompareNoCase(tokenToFind))
					tokenFolder = i;
			}

			return tokenFolder;
		}

	}
}
