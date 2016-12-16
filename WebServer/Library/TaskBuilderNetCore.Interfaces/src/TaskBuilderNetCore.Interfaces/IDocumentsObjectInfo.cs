using System.Collections;

namespace TaskBuilderNetCore.Interfaces
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
