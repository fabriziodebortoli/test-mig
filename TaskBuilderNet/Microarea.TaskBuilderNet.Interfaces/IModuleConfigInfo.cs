using System.Collections;

namespace Microarea.TaskBuilderNet.Interfaces
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
		IBaseModuleInfo ParentModuleInfo { get; }
		bool Parse();
		string Title { get; }
        string Signature { get; }
        int Release { get; }
	}
}
