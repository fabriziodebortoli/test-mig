using System.ComponentModel;

namespace TaskBuilderNetCore.Interfaces.View
{
	public interface ISourcesUpdater
	{
		void UpdateSources(IComponent component);
		void UpdateSources(IComponent[] components);
	}
}
