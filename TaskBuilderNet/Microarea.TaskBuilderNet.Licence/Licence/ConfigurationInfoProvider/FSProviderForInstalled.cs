
using Microarea.TaskBuilderNet.Interfaces;

namespace Microarea.TaskBuilderNet.Licence.Licence.ConfigurationInfoProvider
{
	public class FSProviderForInstalled : FSProvider, IConfigurationInfoProvider
	{
		//---------------------------------------------------------------------
		public FSProviderForInstalled(IBasePathFinder pathFinder) : base(pathFinder){}

		//---------------------------------------------------------------------
		bool IConfigurationInfoProvider.ArticlesLicensedByDefault { get { return true; } }
	}
}
