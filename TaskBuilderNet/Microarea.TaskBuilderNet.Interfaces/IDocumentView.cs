using Microarea.TaskBuilderNet.Interfaces.Model;
using Microarea.TaskBuilderNet.Interfaces.View;

namespace Microarea.TaskBuilderNet.Interfaces
{
	public interface IDocumentView
	{
		IDocumentDataManager Document { get; }
		string SerializedName { get; }
		void CallCreateComponents();
		void CreateComponents();
	}
}