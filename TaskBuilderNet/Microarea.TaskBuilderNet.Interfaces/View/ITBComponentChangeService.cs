using System.ComponentModel;
using System.ComponentModel.Design;

namespace Microarea.TaskBuilderNet.Interfaces.View
{
	//=========================================================================
	public interface ITBComponentChangeService : IComponentChangeService
	{
		//---------------------------------------------------------------------
		void OnComponentAdded(object sender, IComponent component);

		//---------------------------------------------------------------------
		void OnComponentRemoved(object sender, IComponent component);
	}
}
