using TaskBuilderNetCore.Interfaces.Model;

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