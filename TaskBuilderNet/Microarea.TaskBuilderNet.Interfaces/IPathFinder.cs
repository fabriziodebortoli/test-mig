
namespace Microarea.TaskBuilderNet.Interfaces
{
	//=========================================================================
	public interface IPathFinder : IBasePathFinder
	{
		string Company { get; }
		string Edition { get; set; }
		string GetAllUsersCustomExcelPathFromNamespace(INameSpace ns);
		string GetAllUsersCustomReportPathFromNamespace(INameSpace ns);
		string GetAllUsersCustomWordPathFromNamespace(INameSpace ns);
		string GetCustomAppContainerPath(INameSpace aNameSpace);
		string GetCustomApplicationContainerPath(ApplicationType aApplicationType);
		string GetCustomApplicationPath(string applicationName);
		string GetCustomDataManagerDefaultPath(string application, string module, string language);
		string GetCustomDataManagerPath(string application, string module);
		string GetCustomDataManagerSamplePath(string application, string module, string language);
		string GetCustomExcelPathFromNamespace(INameSpace ns);
		string GetCustomModuleDictionaryPath(string appName, string moduleName, bool createDir);
		string GetCustomModulePath(string applicationName, string moduleName);
		string GetCustomReportPathFromNamespace(INameSpace ns);
		string GetCustomWordPathFromNamespace(INameSpace ns);
		string GetCustomCompanyPath();
		string GetFilename(INameSpace aNamespace, ref CommandOrigin aCommandOrigin, string language);
		string GetFilename(INameSpace aNamespace, string language);
		string GetStandardDataManagerDefaultPath(string application, string module, string language);
		string GetStandardDataManagerSamplePath(string application, string module, string language);
		string GetCustomUserApplicationDataPath();
		string User { get; }
		string UserForFileSystem { get; }
	}
}
