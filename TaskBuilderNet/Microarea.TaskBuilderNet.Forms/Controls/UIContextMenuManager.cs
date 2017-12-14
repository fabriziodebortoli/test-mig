using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Windows.Forms;
using Microarea.TaskBuilderNet.Interfaces.View;
using Telerik.WinControls;
using Telerik.WinControls.UI;

namespace Microarea.TaskBuilderNet.Forms.Controls
{
	//=========================================================================
	[ProvideProperty("UIContextMenu", typeof(Control)), Description("Adds the UIContextMenu dynamic property and enables using UIContextMenu in all controls.")]
    public class UIContextMenuManager : RadContextMenuManager
	{
		private Dictionary<RadContextMenu, UIContextMenu> menus = new Dictionary<RadContextMenu, UIContextMenu>();
		private Dictionary<Keys, List<IUIMenuItem>> shortcuts = new Dictionary<Keys, List<IUIMenuItem>>();

		RadContextMenu contextMenu;
		private IUIForm frame;

		//---------------------------------------------------------------------
		public UIContextMenuManager(IUIForm frame)
		{
			this.frame = frame;
		}

		[Category("Behavior"), DisplayName("UIContextMenu"), DefaultValue(null)]
		//---------------------------------------------------------------------
		public UIContextMenu GetUIContextMenu(Control control)
		{
			RadContextMenu tempMenu = GetRadContextMenu(control);
			if (tempMenu == null)
				return null;

			UIContextMenu context = menus[tempMenu];
			return context;
		}

		//---------------------------------------------------------------------
		public void SetUIContextMenu(Control control, UIContextMenu value)
		{
           	menus.Add(value, value);
            SetRadContextMenu(control, value);

			IUIHostingControl c = control as IUIHostingControl;
			if (c == null)
				return;
			
			c.HostedControl.ContextMenu = new ContextMenu();

			if (value != null)
			{
				c.HostedControl.MouseDown += new MouseEventHandler(this.control_MouseDown);
			}
			else
			{
				c.HostedControl.MouseDown -= new MouseEventHandler(this.control_MouseDown);
			}
		}
		
		//---------------------------------------------------------------------
		private void control_MouseDown(object sender, MouseEventArgs e)
		{
			Control control = sender as Control;
			IUIControl c = FindFirstParentUIControl(control);
			if (c == null)
			{
				return;
			}
			contextMenu = GetRadContextMenu(c as Control);
			if (contextMenu == null || contextMenu.Items.Count == 0 || e.Button != MouseButtons.Right)
			{
				return;
			}
			contextMenu.Show(control, e.X, e.Y);			
		}

		//-------------------------------------------------------------------------
		private IUIControl FindFirstParentUIControl(Control sender)
		{
			IUIControl control = sender as IUIControl;
			if (control != null)
				return control;

			return FindFirstParentUIControl(sender.Parent);
		}

		//-------------------------------------------------------------------------
		private void AddShortcut(Keys keys, IUIMenuItem item)
		{
			List<IUIMenuItem> components = null;
			shortcuts.TryGetValue(keys, out components);
			if (components == null)
			{
				components = new List<IUIMenuItem>();
				shortcuts.Add(keys, components);
			}

			if (!components.Contains(item))
				components.Add(item);
		}

		//-------------------------------------------------------------------------
		private static Control FindFocusedControl(IUIComponent component)
		{
			Control control = component as Control;
			ContainerControl container = control as ContainerControl;
			while (container != null)
			{
				control = container.ActiveControl;
				container = control as ContainerControl;
			}
			return control;
		}

		//-------------------------------------------------------------------------
		private Control FindIUIComponent(Control c)
		{
			if (c is IUIComponent)
				return c;

			if (c is IUIContainer)
				return null;

			return FindIUIComponent(c.Parent);
		}

		//-------------------------------------------------------------------------
		internal bool ProcessShortcut(Keys keys)
		{
			//cerco tutti le voci di menu che corrispondono a quella shortcut
			List<IUIMenuItem> components = null;
			shortcuts.TryGetValue(keys, out components);
			if (components == null)
				return false;

			//cerco il control con il fuoco
			Control c = FindFocusedControl(frame);
			if (c == null)
				return false;

			Control component = FindIUIComponent(c);

			//Recupero il menu contestuale del control con il fuoco
			UIContextMenu uiMenu = GetUIContextMenu(component);
			if (uiMenu == null)
				return false;

			//tra tutte le voci di menu cerco quella a cui corrisponde la shortcut
			bool found = false;
			foreach (RadItem current in uiMenu.Items)
			{
				UIMenuItem item = current as UIMenuItem;
				if (item == null)
					continue;

				if (components.Contains(item))
				{
					found = true;
					item.PerformClick();
				}
			}

			return found;
		}

		//-------------------------------------------------------------------------
		internal void AddContextMenuItem(IUIComponent ownerControl, IMenuItemGeneric item)
		{
			Control uiControl = ownerControl as Control;
			if (uiControl == null)
				return;

			UIContextMenu menu = GetUIContextMenu(uiControl);
			if (menu == null)
			{
				menu = new UIContextMenu();
				SetUIContextMenu(uiControl, menu);
			}

			//Controllo per evitare voci doppie, ad esempio nel menu dell'hotlink
			foreach (RadItem current in menu.Items)
			{
				IUIMenuItem mi = current as IUIMenuItem;
				if (mi == null)
					continue;

				if (mi.Name == item.Name)
					return;
			}

			IMenuItemClickable clickable = item as IMenuItemClickable;
			if (clickable != null)
			{
				UIMenuItem menuItem = new UIMenuItem(clickable.Text, clickable.Tag);
				menuItem.TextSeparatorVisibility = ElementVisibility.Visible;
				menuItem.Name = clickable.Name;
				menuItem.Click += new EventHandler(clickable.OnClick);
				menu.Items.Add(menuItem);

				KeysConverter keyConverter = new KeysConverter();
				menuItem.HintText = clickable.Keys != Keys.None ? keyConverter.ConvertToString(clickable.Keys) : string.Empty;

				AddShortcut(clickable.Keys, menuItem);
			}

			IMenuItemSeparator separator = item as IMenuItemSeparator;
			if (separator != null)
			{
				UIMenuSeparatorItem menuItem = new UIMenuSeparatorItem();
				menuItem.Name = separator.Name;
				menu.Items.Add(menuItem);
			}
		}

		//-------------------------------------------------------------------------
		internal void RemoveContextMenuItem(IUIComponent ownerControl, IMenuItemGeneric item)
		{
			if (item == null)
				return;

			Control uiControl = ownerControl as Control;
			if (uiControl == null)
				return;

			UIContextMenu menu = GetUIContextMenu(uiControl);
			if (menu == null)
				return;

			var menuItems = from menuItem in menu.Items
							where menuItem.Name == item.Name
							select menuItem;

			foreach (var menuItem in menuItems.ToList())
			{
				menu.Items.Remove(menuItem);
				menuItem.Dispose();
			}
		}
	}

	//=========================================================================
    public class UIContextMenu : RadContextMenu
	{
	}

	//=========================================================================
	public class UIMenuSeparatorItem : RadMenuSeparatorItem
	{
		public UIMenuSeparatorItem()
		{
			this.ThemeRole = typeof(RadMenuSeparatorItem).Name;
		}
	}
	
}
