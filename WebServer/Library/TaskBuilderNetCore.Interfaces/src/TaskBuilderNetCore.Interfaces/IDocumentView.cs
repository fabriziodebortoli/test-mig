using TaskBuilderNetCore.Interfaces.Model;
using TaskBuilderNetCore.Interfaces.View;

namespace TaskBuilderNetCore.Interfaces
{
	public interface IDocumentView
	{
		IDocumentDataManager Document { get; }
		string SerializedName { get; }
		void CallCreateComponents();
		void CreateComponents();
	}
}