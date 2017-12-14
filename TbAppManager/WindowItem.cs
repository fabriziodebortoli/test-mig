using System;
using System.Collections.Generic;
using System.Windows.Forms;
using Microarea.Library.TBApplicationWrapper;

namespace Microarea.MenuManager
{
	internal class WindowItem : IDisposable
	{
		public IntPtr Handle;
		public IntPtr MenuHandle;
		public DocumentDockableForm Form;
		
		private MenuStrip documentMenu;
		private TbApplicationClientInterface tbInterface;
		//------------------------------------------------------------------------------------------------------------------------------
		public MenuStrip DocumentMenu
		{
			get
			{
				if (documentMenu == null || documentMenu.IsDisposed)
				{
					if (tbInterface != null)
						documentMenu = tbInterface.CreateMenuStrip(Handle, MenuHandle);
					
					if (documentMenu == null)
						documentMenu = new MenuStrip();
				}
				return documentMenu;
			}
		}

		//------------------------------------------------------------------------------------------------------------------------------
		public WindowItem(IntPtr handle, IntPtr menuHandle, DocumentDockableForm form, TbApplicationClientInterface tbInterface)
		{
			this.Handle = handle;
			this.Form = form;
			this.MenuHandle = menuHandle;
			this.tbInterface = tbInterface;
		}

		//------------------------------------------------------------------------------------------------------------------------------
		public override bool Equals(object obj)
		{
			WindowItem i = (WindowItem)obj;
			return i.Handle == this.Handle
				&& i.Form == this.Form
				&& i.MenuHandle == this.MenuHandle;
		}

		//------------------------------------------------------------------------------------------------------------------------------
		public override int GetHashCode()
		{
			return Handle.GetHashCode();
		}

		//------------------------------------------------------------------------------------------------------------------------------
		public static int FindIndex(List<WindowItem> list, IntPtr handleToFind)
		{
			lock (typeof(WindowItem))
			{
				return list.FindIndex((curr) => { return curr.Handle == handleToFind; });
			}
		}
		//------------------------------------------------------------------------------------------------------------------------------
		public static WindowItem Find(List<WindowItem> list, IntPtr handleToFind)
		{
			lock (typeof(WindowItem))
			{
				return list.Find((curr) => { return curr.Handle == handleToFind; });
			}
		}

		//------------------------------------------------------------------------------------------------------------------------------
		public static WindowItem Find(List<WindowItem> list, GenericDockableForm formToFind)
		{
			lock (typeof(WindowItem))
			{
				return list.Find((curr) => { return curr.Form == formToFind; });
			}
		}

		//------------------------------------------------------------------------------------------------------------------------------
		internal void UpdateFormTitle (out string title, out string tooltip)
		{
			Form.UpdateTitle();
			tooltip = Form.Text;
			title = Form.Text;
		}
		#region IDisposable Members

		//------------------------------------------------------------------------------------------------------------------------------
		public void Dispose()
		{
			if (Form != null)
				Form.Dispose();
			if (documentMenu != null)
				documentMenu.Dispose();
		}

		#endregion
	}
}
