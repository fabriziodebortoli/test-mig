
namespace Microarea.TaskBuilderNet.Interfaces
{
	public interface ILogWriter
	{
		void WriteLine (string message, params object[] args);
	}
}
