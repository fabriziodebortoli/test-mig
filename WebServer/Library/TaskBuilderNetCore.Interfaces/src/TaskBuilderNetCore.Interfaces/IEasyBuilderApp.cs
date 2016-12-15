using System.Xml;


namespace TaskBuilderNetCore.Interfaces
{
	//=========================================================================
	/// <remarks />
	public interface IEasyBuilderApp
	{
		//---------------------------------------------------------------------
		/// <remarks />
		ICustomListManager EasyBuilderAppFileListManager { get; }

		//---------------------------------------------------------------------
		/// <remarks />
		bool IsEnabled { get; set; }
		/// <remarks />
		bool IsInExcludeFileFromEnableDisableList(string file);
		/// <remarks />
		bool IsSubjectedToLicenceCheck { get; }
		/// <remarks />
		IBaseModuleInfo ModuleInfo { get; }
		/// <remarks />
		ApplicationType ApplicationType { get; }
		/// <remarks />
		string ApplicationName { get; set; }
		/// <remarks />
		string ModuleName { get; }
		/// <remarks />
		string BasePath { get; }

		//---------------------------------------------------------------------
		/// <remarks />
		void ChangeXmlObjectsNamespace(XmlNode current, string oldApplicationName, string newApplicationName, string oldModuleName, string newmoduleName);

		//---------------------------------------------------------------------
		/// <remarks />
		void CreateApplicationConfig(string filePath);
		/// <remarks />
		void CreateModuleConfig(string moduleConfigFile);
		/// <remarks />
		void CreateNeededFiles();

		//---------------------------------------------------------------------
		/// <remarks />
		void SaveActiveDocument(string fullPath, string publishedUser);

		//---------------------------------------------------------------------
		/// <remarks />
		void Delete();

		//---------------------------------------------------------------------
		/// <remarks />
		void DisableModule();
		/// <remarks />
		void EnableModule();

		//---------------------------------------------------------------------
		/// <remarks />
		bool Equals(object obj);

		//---------------------------------------------------------------------
		/// <remarks />
		void RenameAddOnDatabaseObjects(string oldApplicationName, string newApplicationName, string oldModuleName, string newModuleName);
		/// <remarks />
		void RenameApplicationReferencesInModule(string moduleName, string oldAppName, string newAppName);
		/// <remarks />
		void RenameEasyBuilderApp(string newModuleName);
		/// <remarks />
		void RenameDatabaseObjects(string oldApplicationName, string newApplicationName, string oldModuleName, string newModuleName);
		/// <remarks />
		void RenameDocumentObjects(string oldApplicationName, string newApplicationName, string oldModuleName, string newModuleName);
		/// <remarks />
		void RenameModuleConfig(string applicationName, string oldModuleName, string newModuleName);
	}
}
