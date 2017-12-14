using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.Drawing;
using System.Windows.Forms;
using Microarea.TaskBuilderNet.Forms.Controls;
using Microarea.TaskBuilderNet.Interfaces.View;


namespace Microarea.TaskBuilderNet.Forms
{
	//=========================================================================
	/// <summary>
	/// TBButtonExtenderProvider
	/// </summary>
	public class TBWinButtonExtender : TBWinExtender
	{
		Keys shortcutKey;
		UIClickablePanel clickablePanel;
		IList<IMenuItemGeneric> menuItems;
		Image image;

		public event EventHandler<TBWinButtonExtenderEventArgs> ButtonClicked;
		public event EventHandler<CancelEventArgs> ButtonClicking;

		//---------------------------------------------------------------------
		protected UIClickablePanel Button
		{
			get { return clickablePanel; }
		}

		//---------------------------------------------------------------------
		/// <summary>
		/// Create a new instance of TBButtonExtenderProvider
		/// </summary>
		/// <param name="controller">the controller</param>
		/// <param name="dataObjForButtonState">databinding related to the status of the extender,
		/// it causes for example the change of the clickablePanel image (autonumbering)</param>
		/// <param name="menuItems">menu items</param>
		/// <param name="shortcutKey">shortcut key associated to the clickablePanel click</param>
		public TBWinButtonExtender(
			ITBCUI controller,
			Image image,
			IList<IMenuItemGeneric> menuItems = null,
			Keys shortcutKey = Keys.None
			)
			: base(controller)
		{
			this.image = image;
			this.menuItems = menuItems;
			this.shortcutKey = shortcutKey;
		}

		//---------------------------------------------------------------------
		public override void OnDataReadOnlyChanged(bool newReadOnly)
		{
			base.OnDataReadOnlyChanged(newReadOnly);

			if (clickablePanel != null)
				clickablePanel.Enabled = !newReadOnly;
		}

		//---------------------------------------------------------------------
		public override IUIControl CreateExtenderUIControl()
		{
			this.clickablePanel = new UIClickablePanel();

			this.clickablePanel.BackgroundImage = image;

			this.clickablePanel.Size = new Size(20, 20);

			this.clickablePanel.Click += new EventHandler(Button_Click);

			return clickablePanel;
		}

		//---------------------------------------------------------------------
		void Button_Click(object sender, EventArgs e)
		{
			CancelEventArgs cancel = new CancelEventArgs();
			OnButtonClicking(cancel);
			if (!cancel.Cancel)
				OnButtonClicked(new TBWinButtonExtenderEventArgs(Controller.Control, Data));
		}

		//---------------------------------------------------------------------
		protected virtual void OnButtonClicking(CancelEventArgs args)
		{
			if (ButtonClicking != null)
			{
				ButtonClicking(this, args);
			}
		}

		//---------------------------------------------------------------------
		protected virtual void OnButtonClicked(TBWinButtonExtenderEventArgs args)
		{
			if (ButtonClicked != null)
			{
				ButtonClicked(this, args);
			}
		}

		//---------------------------------------------------------------------
		public override IList<IMenuItemGeneric> GetContextMenuItems()
		{
			if (menuItems != null && menuItems.Count > 0)
			{
				EnsureUniqueShortcutKeys(menuItems);

				return menuItems;
			}

			IList<IMenuItemGeneric> items = base.GetContextMenuItems();

			EnsureUniqueShortcutKeys(items);

			return items;
		}

		//---------------------------------------------------------------------
		[Conditional("DEBUG")]
		private void EnsureUniqueShortcutKeys(IList<IMenuItemGeneric> items)
		{
			foreach (var current in items)
			{
				IMenuItemClickable item = current as IMenuItemClickable;
				if (item == null)
					continue;

				Debug.Assert(item.Keys != shortcutKey, "Shortcut identico per bottone dell'extender e voce menu contestuale " + item.Name);
			}
		}

		//---------------------------------------------------------------------
		public override void DestroyExtenderUIControl()
		{
			base.DestroyExtenderUIControl();

			if (clickablePanel != null)
			{
				this.clickablePanel.Click -= new EventHandler(Button_Click);

				if (!clickablePanel.IsDisposed)
				{
					clickablePanel.Dispose();
				}
				clickablePanel = null;
			}
		}

		//---------------------------------------------------------------------
		public override void OnShortcutKey(Keys keys)
		{
			if (keys == shortcutKey)
			{
				Button_Click(this, EventArgs.Empty);
			}
		}
	}
}
