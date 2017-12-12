using System.Collections;

namespace Microarea.TaskBuilderNet.Interfaces
{
	//===================================================================================
	public interface ILibraryInfo
	{
		string Name { get; set; }
		string AggregateName { get; set; }
		string Path { get; set; }
		string ParentModuleName { get; }

		IBaseModuleInfo ParentModuleInfo { get; }
		IList Documents { get; }
		IDocumentInfo GetDocumentInfoByNameSpace(string nameSpace);
	}
}
