using System;
using System.Collections.Generic;
using System.Windows.Forms;
namespace Microarea.TaskBuilderNet.Interfaces
{
	public interface ICustomizationContext
	{
		event EventHandler<EventArgs> CurrentEasyBuilderAppChanged;

		List<string> ActiveDocuments { get; }
		string CurrentApplication { get; set; }
		IEasyBuilderApp CurrentEasyBuilderApp { get; }
		string CurrentModule { get; set; }
		INameSpace CurrentModuleNamespace { get; }
		IDiagnostic Diagnostic { get; }
		string DynamicLibraryName { get; }
		IList<IEasyBuilderApp> EasyBuilderApplications { get; }
		bool ExistsCurrentEasyBuilderApp { get; }
		bool IsCurrentEasyBuilderAppAStandardization { get; }

		void AddToCurrentCustomizationList(string fileFullPath, bool save = true, bool isActiveDocument = false, string publishedUser = "", string documentNamespace = "");
		void AddToEasyBuilderAppCustomizationList(IEasyBuilderApp app, string fileFullPath, bool save = true, bool isActiveDocument = false, string publishedUser = "", string documentNamespace = "");
		void ChangeEasyBuilderApp(string appName, string modName = "");
		bool ContainsControllerDll(INameSpace controllerNamespace);
		IEasyBuilderApp FindEasyBuilderApp(string customListFile);
		IEasyBuilderApp FindEasyBuilderApp(string appName, string modName);
		IList<IEasyBuilderApp> FindEasyBuilderAppsByApplicationName(string appName);
		INameSpace FormatDynamicNamespaceDocument(string applicationName, string moduleName, string documentName);
		//IList<IEasyBuilderAppDetails> GetAllEasyBuilderAppsFileListPath();
		List<string> GetAllEasyBuilderAppsFileListPath(string application);
		void GetApplicationAndModuleFromCustomFile(string customListFile, out string application, out string module);
		string GetCurrentModuleNamespace();
		string GetEasyBuilderAppAssemblyFullName(INameSpace customizationNameSpace, string user, IEasyBuilderApp app);
		IList<IEasyBuilderApp> GetEasyBuilderApps(string application, ApplicationType applicationType);
		IList<IEBLink> GetEasyBuilderAppStandardizationLinks(INameSpace documentNamespace);
		IBaseModuleInfo GetModuleInfo(string applicationName, string moduleName, ApplicationType applicationType);
		string GetParentPseudoNamespaceFromFullPath(string fullPath);
		string GetPseudoNamespaceFromFullPath(string fullPath, string publishedUser);
		bool IsActiveApplication(string application);
		bool IsActiveDocument(INameSpace documentNamespace);
		bool IsApplicationAlreadyExisting(string applicationName);
		bool IsFileToExport(string filefullPath);
		bool IsSubjectedToPublication(string filePath);
		bool IsValidName(string name);
        bool HasSourceCode(string filePath);
        bool NotAlone(string caption, int maxLogins, int maxDocuments, IWin32Window owner = null);
		void RemoveFromCustomListAndFromFileSystem(string path);
		void RemoveFromCustomListAndFromFileSystem(IEasyBuilderApp app, string path);
		bool ShouldStandardizationsBeAvailable();
		void PublishMenu(string userFilePath);
		void PublishDocument(string documentFolder, string documentFileNameWithoutExtension, string user, bool isActive);
		bool AddNewEasyBuilderApp(string newAppName, string newModName, ApplicationType applicationType);
		IEasyBuilderApp AddNewStandardization(string application, string module);
		IEasyBuilderApp AddNewCustomization(string application, string module = "Module1");
		IEasyBuilderApp CreateNew(string application, string module, ApplicationType appType);
		void DeleteDocumentCustomization(IEasyBuilderApp easyBuilderApp, string fullPath, string publishedUser);
		void SaveSettings();
		void RenameApplication(string oldAppName, string newAppName);

		IEasyBuilderApp Import(string application, string module, ApplicationType applicationType);
		void DeleteApplication(string application);
		void DeleteEasyBuilderApp(IEasyBuilderApp cust);
		void DeleteEasyBuilderApp(string application, string module);
		void DeleteMenu(IEasyBuilderApp easyBuilderApp, string fullPath);
		void UpdateActiveDocuments(bool isActiveDocument, string filePath, string publishedUser);
		void RemoveFileReadonly(string file);
	}
}