using System.Collections;

namespace Microarea.TaskBuilderNet.Interfaces
{
	//=========================================================================
	public interface IAddOnDatabaseObjectsInfo
	{
		IList AdditionalColumns { get; }
		string AppName { get; set; }
		string FilePath { get; }
		string ModName { get; set; }
		IBaseModuleInfo ParentModuleInfo { get; }
		bool Parse();
		string ParsingError { get; set; }
		bool Valid { get; set; }
	}
}
