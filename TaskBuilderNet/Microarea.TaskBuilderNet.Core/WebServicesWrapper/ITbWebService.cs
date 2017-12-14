
namespace Microarea.TaskBuilderNet.Core.WebServicesWrapper
{
	public interface ITbWebService
	{
		bool IsAlive();
		bool Init();
		string Name { get; }
		string Url { get; }
	}
}
