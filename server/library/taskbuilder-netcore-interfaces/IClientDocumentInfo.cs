using System.Collections.Generic;

namespace TaskBuilderNetCore.Interfaces
{
	public interface IClientDocumentInfo
	{
		List<IDocumentInfoComponent> Components { get; set; }
		bool IsDynamic { get; set; }
		INameSpace NameSpace { get; }
		string OjectType { get; set; }
		string Title { get; }
	}
}