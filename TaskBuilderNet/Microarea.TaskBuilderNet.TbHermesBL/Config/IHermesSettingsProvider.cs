
namespace Microarea.TaskBuilderNet.TbHermesBL.Config
{
	public interface IHermesSettingsProvider
	{
		HermesSettings GetSettings();
		void Refresh();
	}
}
