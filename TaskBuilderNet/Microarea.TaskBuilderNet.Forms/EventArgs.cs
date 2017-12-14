using System;
using System.ComponentModel;
using System.Windows.Forms;
using Microarea.TaskBuilderNet.Forms.Controls;
using Microarea.TaskBuilderNet.Interfaces.Model;
using Microarea.TaskBuilderNet.Interfaces.View;
using Telerik.WinControls;
using Telerik.WinControls.Enumerations;
using Telerik.WinControls.UI;
using Telerik.WinControls.UI.Data;
using DataPositionChangedEventArgs = Telerik.WinControls.UI.Data.PositionChangedEventArgs;

namespace Microarea.TaskBuilderNet.Forms
{
	//=========================================================================
	/// <summary>
	/// TBWinButtonExtenderEventArgs
	/// </summary>
	public class TBWinButtonExtenderEventArgs : EventArgs
	{
		//---------------------------------------------------------------------
		public IUIControl Control
		{
			private set;
			get;
		}

		//---------------------------------------------------------------------
		public IDataObj ControlDataObj
		{
			private set;
			get;
		}

		//---------------------------------------------------------------------
		public IDataObj StateDataObj
		{
			internal set;
			get;
		}

		//---------------------------------------------------------------------
		public TBWinButtonExtenderEventArgs(IUIControl control, IDataObj controlDataObj, IDataObj stateDataObj = null)
		{
			this.Control = control;
			this.ControlDataObj = controlDataObj;
			this.StateDataObj = stateDataObj;
		}
	}

	//=============================================================================================
	public class UIPositionChangedEventArgs : EventArgs
	{
		private DataPositionChangedEventArgs args;
		public int Position { get { return args.Position; } }

		//-----------------------------------------------------------------------------------------
		internal UIPositionChangedEventArgs(DataPositionChangedEventArgs args)
		{
			this.args = args;
		}

	}

	//=============================================================================================
	public class UIPositionChangingCancelEventArgs : EventArgs
	{
		private PositionChangingCancelEventArgs args;

		public bool Cancel { get { return args.Cancel; } set { args.Cancel = value; } }
		public int Position { get { return args.Position; } }

		//--------------------------------------------------------------------------------------
		internal UIPositionChangingCancelEventArgs(PositionChangingCancelEventArgs args)
		{
			this.args = args;
		}

	}

	//=============================================================================================
	public class UIStateChangedEventArgs : EventArgs
	{
		private StateChangedEventArgs args;

		public UIToggleState ToggleState { get { return (UIToggleState)args.ToggleState; } }

		//--------------------------------------------------------------------------------------
		internal UIStateChangedEventArgs(StateChangedEventArgs args)
		{
			this.args = args;
		}
	}

	//=============================================================================================
	public class UIStateChangingEventArgs : EventArgs
	{
		private StateChangingEventArgs args;

		public bool Cancel { get { return args.Cancel; } set { args.Cancel = value; } }
		public UIToggleState NewValue { get { return (UIToggleState)args.NewValue; } set { args.NewValue = (ToggleState)value; } }
		public UIToggleState OldValue { get { return (UIToggleState)args.OldValue; } }

		//--------------------------------------------------------------------------------------
		internal UIStateChangingEventArgs(StateChangingEventArgs args)
		{
			this.args = args;
		}
	}

    //================================================================================================================
    public class UIContextMenuItemAddedEventArgs : EventArgs
    {
		private IMenuItemGeneric item;

		public UIContextMenuItemAddedEventArgs(IMenuItemGeneric item)
        {
            this.item = item;
        }

		public IMenuItemGeneric Item
        {
            get { return item; }
        }
    }

	//=========================================================================
	public class UITreeNodeEventArgs : EventArgs
	{
		RadTreeNode uiTreeNode;

		//---------------------------------------------------------------------------
		public RadTreeNode UiTreeNode
		{
			get { return uiTreeNode as RadTreeNode; }
		}

		//---------------------------------------------------------------------------
		public UITreeNodeEventArgs(RadTreeNode uiTreeNode)
		{
			this.uiTreeNode = uiTreeNode;
		}
	}

	//=============================================================================================
	public class UITreeCancelEventArgs : CancelEventArgs
	{
		UITreeNode node;
		public UITreeNode Node { get { return node; } }

		//---------------------------------------------------------------------------
		internal UITreeCancelEventArgs(RadTreeViewCancelEventArgs radEventArgs)
		{
			this.Cancel = radEventArgs.Cancel;
			this.node = radEventArgs.Node as UITreeNode;
		}
	}

	//===================================================================================
	public class ShortcutAddedEventArgs : EventArgs
	{
		Keys keys;

		public Keys Keys
		{
			get { return keys; }
			set { keys = value; }
		}

		RadItem owner;

		public RadItem Owner
		{
			get { return owner; }
			set { owner = value; }
		}

		public ShortcutAddedEventArgs(RadItem owner, Keys keys)
		{
			this.owner = owner;
			this.keys = keys;
		}

	}

	//=============================================================================================
	public enum UIToggleState
	{
		Off = ToggleState.Off,
		On = ToggleState.On,
		Indeterminate = ToggleState.Indeterminate
	}
}
