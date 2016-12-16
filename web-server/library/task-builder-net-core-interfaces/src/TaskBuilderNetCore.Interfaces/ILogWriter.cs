
namespace TaskBuilderNetCore.Interfaces
{
	public interface ILogWriter
	{
		void WriteLine (string message, params object[] args);
	}
}
