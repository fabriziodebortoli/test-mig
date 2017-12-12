
namespace Microarea.TaskBuilderNet.Interfaces
{
	//=========================================================================
	public interface IModule
	{
		string Name { get; }
		string Application { get; }
		string Container { get; }
	}

	//=========================================================================
	public interface IDeployModule : IModule
	{
		PolicyType DeploymentPolicy { get; }
	}

	//=========================================================================
	public enum PolicyType { Unknown = -1, Base, Full } // deployment policy for a given _module_
}
