using System.Collections;

namespace TaskBuilderNetCore.Interfaces
{
	//===================================================================================
	public interface ILibraryInfo
	{
		string Name { get; set; }
		string AggregateName { get; set; }
		string Path { get; set; }
		string ParentModuleName { get; }

	//	ModuleInfo ParentModuleInfo { get; }
		IList Documents { get; }
		IDocumentInfo GetDocumentInfoByNameSpace(string nameSpace);
	}
}
