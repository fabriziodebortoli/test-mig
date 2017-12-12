using System.Xml;


namespace Microarea.TaskBuilderNet.Interfaces
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
		void RenameApplicationReferencesInModule(string moduleName, string oldAppName, string newAppName);
		/// <remarks />
		void RenameEasyBuilderApp(string newModuleName);

		/// <remarks />
	}
}
