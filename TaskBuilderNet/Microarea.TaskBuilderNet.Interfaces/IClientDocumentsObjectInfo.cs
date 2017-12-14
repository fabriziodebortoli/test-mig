using System.Collections;

namespace Microarea.TaskBuilderNet.Interfaces
{
	//=========================================================================
	public interface IClientDocumentsObjectInfo
	{
		string FilePath { get; }
		bool Parse();
		string ParsingError { get; }
		IList ServerDocuments { get; }
		bool Valid { get; }
	}
}
