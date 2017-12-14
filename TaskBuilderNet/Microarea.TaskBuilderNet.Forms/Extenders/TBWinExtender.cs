using System;
using System.Collections.Generic;
using System.Windows.Forms;
using Microarea.Framework.TBApplicationWrapper;
using Microarea.TaskBuilderNet.Interfaces.Model;
using Microarea.TaskBuilderNet.Interfaces.View;

namespace Microarea.TaskBuilderNet.Forms
{
	//=========================================================================
	/// <summary>
	/// TBUIExtenderProvider
	/// </summary>
	public abstract class TBWinExtender : ITBUIExtenderProvider, IDisposable
	{
		TBWFCUIControl controller;
		MDataObj data;
		UIExtenderPosition position = UIExtenderPosition.Right;

		internal TBWFCUIControl Controller { get { return controller; } }
		internal MDataObj Data { get { return data; } }
		internal IUIControl Extendee { get { return controller != null ? controller.Control : null; } }

		//---------------------------------------------------------------------
		/// <summary>
		/// The position must be set before the AddExtender call,
		/// otherwise it will not be considered.
		/// </summary>
		public UIExtenderPosition Position
		{
			get { return this.position; }
			set { this.position = value; }
		}

		//---------------------------------------------------------------------
		protected TBWinExtender(ITBCUI controller)
		{
			this.controller = controller as TBWFCUIControl;
			this.data = Controller.DataBinding != null ? Controller.DataBinding.Data as MDataObj : null;
		}

		//---------------------------------------------------------------------
		public void Dispose()
		{
			Dispose(true);
			GC.SuppressFinalize(this);
		}

		//---------------------------------------------------------------------
		protected virtual void Dispose(bool disposing)
		{
			DestroyExtenderUIControl();
		}

		//---------------------------------------------------------------------
		public virtual void DestroyExtenderUIControl()
		{}

		//---------------------------------------------------------------------
		public virtual IList<IMenuItemGeneric> GetContextMenuItems()
		{
			return new List<IMenuItemGeneric>();
		}

		//---------------------------------------------------------------------
		public virtual void OnFormModeChanged(FormModeType newFormMode)
		{}

		//---------------------------------------------------------------------
		public virtual void OnDataReadOnlyChanged(bool newReadOnly)
		{}

		//----------------------------------------------------------------------------
		public virtual void OnControlEnabledChanged(bool newEnabled)
		{}

		//---------------------------------------------------------------------
		public virtual bool CanEnableExtendee()
		{
			return true;
		}

		//---------------------------------------------------------------------
		public virtual void OnShortcutKey(Keys keys)
		{ }

		//---------------------------------------------------------------------
		public abstract IUIControl CreateExtenderUIControl();
	}
}
