using Microarea.TaskBuilderNet.Interfaces;
namespace ManifestGenerator
{
	interface ILogger : ILogWriter
	{
		void Start ();
		void Stop ();

		void PerformStep ();
		void SetProgressTop(int top);
	}
}
