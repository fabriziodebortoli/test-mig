
namespace Microarea.TaskBuilderNet.Interfaces
{
	//=========================================================================
	/// <summary>
	/// Corrispettivo del file Application.config che decrive un'applicazione
	/// per TaskBuilder.
	/// </summary>
	public interface IBaseApplicationInfo
	{
		int AddModule(IBaseModuleInfo aModuleInfo);
		IApplicationConfigInfo ApplicationConfigInfo { get; }
		ApplicationType ApplicationType { get; }
		string GetCustomPath(string companyName);
		IBaseModuleInfo GetModuleInfoByName(string moduleName);
		IBaseModuleInfo GetModuleInfoByTitle(string moduleTitle);
		bool IsKindOf(ApplicationType applicationType);
		System.Collections.ICollection Modules { get; }
		string Name { get; }
		string Path { get; }
		IBasePathFinder PathFinder { get; }
	}
}
