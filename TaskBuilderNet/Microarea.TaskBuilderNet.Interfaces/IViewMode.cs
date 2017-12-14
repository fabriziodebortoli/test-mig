
namespace Microarea.TaskBuilderNet.Interfaces
{
	//=========================================================================
	public interface IViewMode
	{
		bool IsBackGround { get; }
		bool IsBatch { get; }
		bool IsDataEntry { get; }
		bool IsDefault { get; }
		bool IsFinder { get; }
		string Name { get; }
		string Title { get; }
		string Type { get; }
	}
}
