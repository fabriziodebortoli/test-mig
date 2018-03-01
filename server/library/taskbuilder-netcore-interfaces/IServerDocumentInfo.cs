using System.Collections.Generic;
	
namespace TaskBuilderNetCore.Interfaces
{
	public interface IServerDocumentInfo
	{
		List<IClientDocumentInfo> ClientDocsInfos { get; }
		string DocumentClass { get; }
		INameSpace NameSpace { get; }
		string Type { get; }

		int AddClientDoc(IClientDocumentInfo aClientDocumentInfo);
	}
}