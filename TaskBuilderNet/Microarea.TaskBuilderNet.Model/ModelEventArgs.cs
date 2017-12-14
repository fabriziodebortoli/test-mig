using System;
using Microarea.TaskBuilderNet.Interfaces.Model;
using Microarea.TaskBuilderNet.Interfaces.View;

namespace Microarea.TaskBuilderNet.Model
{
	//=============================================================================
	public class ValidItemEventArgs : EventArgs
	{
		bool isValid = true;

		ITBBindingListItem bindingItem;

		public bool IsValid { get { return isValid; } set { isValid = value; } }
		public ITBBindingListItem BindingItem { get { return bindingItem; } }

		//----------------------------------------------------------------------------
		public ValidItemEventArgs(ITBBindingListItem bindingItem)
		{
			this.bindingItem = bindingItem;
		}
	}

	//=============================================================================
	public class DataBindingChangedEventArgs : EventArgs
	{
		IDataBinding oldValue;

		public DataBindingChangedEventArgs(IDataBinding oldValue) { this.oldValue = oldValue; }
		public IDataBinding OldValue { get { return oldValue; } }
	}

	//=============================================================================
	public class FormatterChangedEventArgs : EventArgs
	{
		ITBFormatterProvider oldValue;


		public FormatterChangedEventArgs(ITBFormatterProvider oldValue) { this.oldValue = oldValue; }
		public ITBFormatterProvider OldValue { get { return oldValue; } }
	}
}
