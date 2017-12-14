using System.Collections;

namespace Microarea.TaskBuilderNet.Interfaces
{
	//=========================================================================
	public interface IDocumentsObjectInfo
	{
		IList Documents { get; }
		bool Parse(string file);
		string ParsingError { get; }
		bool Valid { get; }
	}
}
