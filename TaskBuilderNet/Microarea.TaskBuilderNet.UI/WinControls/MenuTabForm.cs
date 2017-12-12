using System;
using System.Drawing;
using System.Runtime.InteropServices;
using System.Text;
using System.Windows.Forms;

using Microarea.TaskBuilderNet.Core.Generic;

namespace Microarea.TaskBuilderNet.UI.WinControls
{
	///<summary>
	/// Form da cui derivare per fare in modo di ottenere le form C# "tabbate" dentro il MenuManager
	///</summary>
	//================================================================================
	public partial class MenuTabForm : Form
	{
		protected IntPtr parentHandle; 
		private bool docked;

		//--------------------------------------------------------------------------------
		protected virtual string Title { get { return Text; } }
		protected virtual string ToolTip { get { return Text; } }
		public IntPtr ParentHandle { get { return parentHandle; } }

		//-------------------------------------------------------------------------
		public MenuTabForm()
		{
			InitializeComponent();
		}

		//-------------------------------------------------------------------------
		public void Show(IntPtr parentHandle)
		{
			this.parentHandle = parentHandle;
			if (parentHandle.Equals(IntPtr.Zero))
				Show();
			else
				ExternalAPI.PostMessage(parentHandle, ExternalAPI.UM_DOCUMENT_CREATED, Handle, IntPtr.Zero);
		}

		//-------------------------------------------------------------------------
		protected override void OnActivated(EventArgs e)
		{
            Activate();
			base.OnActivated(e);
		}

        //-------------------------------------------------------------------------
		public new void Activate()
        {
            base.Activate();
            ExternalAPI.SendMessage(ParentHandle, ExternalAPI.UM_FRAME_ACTIVATE, Handle, IntPtr.Zero);
        }
		//-------------------------------------------------------------------------
		protected override void WndProc(ref Message m)
		{
			if (m.Msg == ExternalAPI.UM_UPDATE_FRAME_STATUS)
			{
				DockUndock(ref m);
				return;
			}
			if (m.Msg == ExternalAPI.UM_GET_DOCUMENT_TITLE_INFO)
			{
				SendTitleInfo(ref m);
				return;
			}
			if (m.Msg == ExternalAPI.UM_GET_DOC_NAMESPACE_ICON)
			{
				SendNamespaceIconInfo(ref m);
				return;
			}
			base.WndProc(ref m);
		}

		//-------------------------------------------------------------------------
		private void SendNamespaceIconInfo(ref Message m)
		{
			IntPtr msgBuff = m.WParam;
			int nBuffLength = (int)m.LParam;
			String text = "";//namespace?
			WriteText(text, msgBuff, nBuffLength);

			m.Result = Icon.Handle;
		}

		//-------------------------------------------------------------------------
		private void SendTitleInfo(ref Message m)
		{
			String text = string.Concat(this.Title, " - ", this.ToolTip);
			IntPtr msgBuff = m.WParam;
			int nBuffLength = (int)m.LParam;
			WriteText(text, msgBuff, nBuffLength);
			m.Result = (IntPtr)this.Title.Length;
		}

		private void WriteText(String text, IntPtr msgBuff, int nBuffLength)
		{
			byte[] buff = Encoding.Unicode.GetBytes(text);
			int i;
			for (i = 0; i < buff.Length && i < nBuffLength; i++)
				Marshal.WriteByte(msgBuff, i, buff[i]);
			Marshal.WriteByte(msgBuff, i++, 0);//terminatori di stringa
			Marshal.WriteByte(msgBuff, i++, 0);//terminatori di stringa
		}

		//-------------------------------------------------------------------------
		protected override void OnFormClosed(FormClosedEventArgs e)
		{
			base.OnFormClosed(e);
			if (!IsDisposed)
			{
				ExternalAPI.PostMessage(ParentHandle, ExternalAPI.UM_DOCUMENT_DESTROYED, Handle, IntPtr.Zero);
			}
		}

		//-------------------------------------------------------------------------
		private void DockUndock(ref Message m)
		{
			bool bDocked = !m.WParam.Equals(0);
			IntPtr hwndParent = m.LParam;

			uint style = (uint)ExternalAPI.GetWindowLong(Handle, (int)ExternalAPI.GetWindowLongIndex.GWL_STYLE);

			uint dwRemoveForDockStyle = (uint)ExternalAPI.WindowStyles.WS_THICKFRAME | (uint)ExternalAPI.WindowStyles.WS_CAPTION | (uint)ExternalAPI.WindowStyles.WS_POPUP;
			uint dwAddForDockStyle = (uint)ExternalAPI.WindowStyles.WS_CHILD;

			if (bDocked)   //draw window in tab rectangle
			{
				if (!docked)
				{
					docked = true;
					this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.None;
					style |= dwAddForDockStyle;
					style &= ~dwRemoveForDockStyle;
					//GetWindowRect(m_FloatingRect);
					ExternalAPI.SetWindowLong(Handle, (int)ExternalAPI.GetWindowLongIndex.GWL_STYLE, (int)style);
					ExternalAPI.SetParent(Handle, hwndParent);
					Show();
				}
			}
			else  //draw window in original position with frame
			{
				if (docked)
				{
					ExternalAPI.SetParent(Handle, hwndParent);
					docked = false;
					style |= dwRemoveForDockStyle;
					style &= ~dwAddForDockStyle;
					ExternalAPI.SetWindowLong(Handle, (int)ExternalAPI.GetWindowLongIndex.GWL_STYLE, (int)style);
					this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.Sizable;
				}
			}
		}
	}
}