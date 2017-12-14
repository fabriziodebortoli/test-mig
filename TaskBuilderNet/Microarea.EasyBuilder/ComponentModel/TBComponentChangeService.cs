using System.ComponentModel;
using System.ComponentModel.Design;
using Microarea.TaskBuilderNet.Interfaces.View;

namespace Microarea.EasyBuilder.ComponentModel
{
	//================================================================================
	class TBComponentChangeService : ITBComponentChangeService
	{
		public event ComponentEventHandler ComponentAdded;
		public event ComponentEventHandler ComponentAdding;
		public event ComponentChangedEventHandler ComponentChanged;
		public event ComponentChangingEventHandler ComponentChanging;
		public event ComponentEventHandler ComponentRemoved;
		public event ComponentEventHandler ComponentRemoving;
		public event ComponentRenameEventHandler ComponentRename;

		//--------------------------------------------------------------------------------
		public void OnComponentChanged(object component, MemberDescriptor member, object oldValue, object newValue)
		{
			if (ComponentChanged != null)
				ComponentChanged(this, new ComponentChangedEventArgs(component, member, oldValue, newValue));
		}

		//--------------------------------------------------------------------------------
		public void OnComponentChanging(object component, MemberDescriptor member)
		{
			if (ComponentChanging != null)
				ComponentChanging(this, new ComponentChangingEventArgs(component, member));
		}

		//--------------------------------------------------------------------------------
		public void OnComponentAdding(object sender, IComponent component)
		{
			if (ComponentAdding != null)
				ComponentAdding(sender, new ComponentEventArgs(component));
		}

		//--------------------------------------------------------------------------------
		public void OnComponentAdded(object sender, IComponent component)
		{
			if (ComponentAdded != null)
				ComponentAdded(sender, new ComponentEventArgs(component));
		}

		//--------------------------------------------------------------------------------
		public void OnComponentRemoving(object sender, IComponent component)
		{
			if (ComponentRemoving != null)
				ComponentRemoving(sender, new ComponentEventArgs(component));
		}

		//--------------------------------------------------------------------------------
		public void OnComponentRemoved(object sender, IComponent component)
		{
			if (ComponentRemoved != null)
				ComponentRemoved(sender, new ComponentEventArgs(component));
		}

		//--------------------------------------------------------------------------------
		public void OnComponentRename(object sender, IComponent component, string oldName, string newName)
		{
			if (ComponentRename != null)
				ComponentRename(sender, new ComponentRenameEventArgs(component, oldName, newName));
		}
	}
}
