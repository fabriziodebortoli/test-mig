using System.Collections;
using System.Collections.Generic;
using TaskBuilderNetCore.Documents.Model.Interfaces;

namespace TaskBuilderNetCore.Interfaces
{
	//=========================================================================
	public interface IClientDocumentsObjectInfo
	{
		string FilePath { get; }
		bool Parse();
		string ParsingError { get; }
		IList<IServerDocumentInfo> ServerDocuments { get; }
		bool Valid { get; }

        IList GetClientDocumentsFor(IDocument document);
    }
}
