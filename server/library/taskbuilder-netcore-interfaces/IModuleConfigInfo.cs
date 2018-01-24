using System.Collections;

namespace TaskBuilderNetCore.Interfaces
{
	//===================================================================================
	public interface IModuleConfigInfo
	{
		string DestinationFolder { get; }
		string GetOutPutPath(bool debug);
		IList Libraries { get; }
		int MenuViewOrder { get; }
		string ModuleConfigFile { get; }
		IList ModuleFolders { get; }
		string ModuleName { get; }
		bool Optional { get; }
	//	BaseModuleInfo ParentModuleInfo { get; }
		bool Parse();
		string Title { get; }
        string Signature { get; }
        int Release { get; }
	}
}
