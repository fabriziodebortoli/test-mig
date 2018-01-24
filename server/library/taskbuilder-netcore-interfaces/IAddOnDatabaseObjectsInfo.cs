using System.Collections;

namespace TaskBuilderNetCore.Interfaces
{
	//=========================================================================
	public interface IAddOnDatabaseObjectsInfo
	{
		IList AdditionalColumns { get; }
		string AppName { get; set; }
		string FilePath { get; }
		string ModName { get; set; }
		BaseModuleInfo ParentModuleInfo { get; }
		bool Parse();
		string ParsingError { get; set; }
		bool Valid { get; set; }
	}
}
