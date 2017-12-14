using System;
using System.ComponentModel;
using System.Threading;
using System.Windows.Forms;
using Microarea.TaskBuilderNet.Core.Generic;
using Microarea.TaskBuilderNet.Core.WebSockets;
using WeifenLuo.WinFormsUI.Docking;


namespace Microarea.MenuManager
{
	public partial class GenericDockableForm : DockContent
	{
		MenuStrip menu = null;

		//------------------------------------------------------------------------------------------------------------------------------
		public MenuStrip DocumentMenu
		{
			get { return menu; }
			set 
			{
				menu = value;
				if (menu != null)
				{
					menu.MenuActivate += new EventHandler(MenuActivate);
					menu.MenuDeactivate += new EventHandler(MenuDeactivate);
				}
			}
		}

		//------------------------------------------------------------------------------------------------------------------------------
		public virtual bool CanClone { get { return false; } }
		
		//------------------------------------------------------------------------------------------------------------------------------
		public GenericDockableForm()
			: this("")
		{ 
		}

		/// <summary>
		/// Clean up any resources being used.
		/// </summary>
		/// <param name="disposing">true if managed resources should be disposed; otherwise, false.</param>
		//------------------------------------------------------------------------------------------------------------------------------
		protected override void Dispose(bool disposing)
		{
			base.Dispose(disposing);
			if (disposing && (components != null))
			{
				components.Dispose();
			}

			if (menu != null)
			{
				menu.MenuActivate -= new EventHandler(MenuActivate);
				menu.MenuDeactivate -= new EventHandler(MenuDeactivate);

				menu.Dispose();
				menu = null;
			}
		}

		//------------------------------------------------------------------------------------------------------------------------------
		protected override void OnShown(EventArgs e)
		{
			base.OnShown(e);
			//ServerWebSocketConnector.PushToClients("", "DocumentListUpdated", "");
		}

		//------------------------------------------------------------------------------------------------------------------------------
		void MenuDeactivate(object sender, EventArgs e)
		{
			ActivateFormWindow(true, false);
		}

		//------------------------------------------------------------------------------------------------------------------------------
		void MenuActivate(object sender, EventArgs e)
		{
			((MenuStrip)sender).Focus();
		}
		//------------------------------------------------------------------------------------------------------------------------------
		public virtual void ActivateFormWindow(bool activate, bool posted)
		{ 
		
		}
		//------------------------------------------------------------------------------------------------------------------------------
		public GenericDockableForm(string caption)
		{
			InitializeComponent();

			Text = caption;
		}

		//------------------------------------------------------------------------------------------------------------------------------
		protected override void OnActivated(EventArgs e)
		{
			base.OnActivated(e);
			AdjustMenuVisibility();
		}

		//------------------------------------------------------------------------------------------------------------------------------
		protected override void OnVisibleChanged(EventArgs e)
		{
			if (Visible)
				AdjustMenuVisibility();
			base.OnVisibleChanged(e);
		}
		
		//------------------------------------------------------------------------------------------------------------------------------
		private void AdjustMenuVisibility()
		{
			if (Disposing || IsDisposed)
				return;
			
			if (Parent == null)
				return;

			try
			{
				Form ownerForm = Parent.FindForm();
				if (ownerForm != null &&
					!ownerForm.IsDisposed &&
					ownerForm.MainMenuStrip != menu &&
					this.DockState != WeifenLuo.WinFormsUI.Docking.DockState.Float)
				{
					ownerForm.SuspendLayout();

					if (DocumentMenu != null)
						DocumentMenu.Visible = true;

					if (ownerForm.MainMenuStrip != null)
					{
						ownerForm.MainMenuStrip.Visible = false;
						if (DocumentMenu != null)
							DocumentMenu.BackColor = ownerForm.MainMenuStrip.BackColor;
					}


					ownerForm.MainMenuStrip = menu;
					if (menu != null && !menu.IsDisposed)
						ownerForm.Controls.Add(menu);

					ownerForm.ResumeLayout(true);
				}
			}
			catch 
			{
				//in caso di oggetto disposed potrebbe dare un'eccezione, non è il caso di gestirla
			}
		}

		//------------------------------------------------------------------------------------------------------------------------------
		private void closeToolStripMenuItem_Click(object sender, EventArgs e)
		{
			Close();
		}

		//------------------------------------------------------------------------------------------------------------------------------
		private void closaAllButThisToolStripMenuItem_Click(object sender, EventArgs e)
		{
			foreach (IDockContent content in Pane.Contents)
			{
				if (content is DocumentDockableForm && content != this)
					((DocumentDockableForm)content).Close();

			}
		}

		//------------------------------------------------------------------------------------------------------------------------------
		/// <summary>
		/// Questo metodo è necessario perchè la finestra del menu è l'unica ad essere istanziata pre-login e viene localizzata con la lingua di applicazione.
		/// quando viene cambiata la culture post login, la finestra non sente le modifiche, quindi tutte le sue stringhe sono da localizzare nuovamente
		/// </summary>
		public void ApplyResources()
		{
			ComponentResourceManager rm = new ComponentResourceManager(typeof(GenericDockableForm));
			rm.ApplyResources(contextMenuStrip, contextMenuStrip.Name, Thread.CurrentThread.CurrentUICulture);
			rm.ApplyResources(closeToolStripMenuItem, closeToolStripMenuItem.Name, Thread.CurrentThread.CurrentUICulture);
			rm.ApplyResources(closaAllButThisToolStripMenuItem, closaAllButThisToolStripMenuItem.Name, Thread.CurrentThread.CurrentUICulture);
			rm.ApplyResources(cloneToolStripMenuItem, cloneToolStripMenuItem.Name, Thread.CurrentThread.CurrentUICulture);
		}

		//------------------------------------------------------------------------------------------------------------------------------
		private void contextMenuStrip_Opening(object sender, System.ComponentModel.CancelEventArgs e)
		{
			cloneToolStripMenuItem.Visible = CanClone;
			e.Cancel = !(contextMenuStrip.SourceControl is DockPaneStripBase);//per evitare che si apra cliccando sulla client area ddel documento
		}

		//------------------------------------------------------------------------------------------------------------------------------
		private void cloneToolStripMenuItem_Click(object sender, EventArgs e)
		{
			Clone();
		}

		//------------------------------------------------------------------------------------------------------------------------------
		protected virtual void Clone()
		{
			//does nothing!
		}

		//------------------------------------------------------------------------------------------------------------------------------
		protected override void WndProc(ref Message m)
		{
			if (IsDisposed || Disposing)
				return;

			if (m.Msg == ExternalAPI.WM_MOUSEACTIVATE)
			{
				//fictitious click to hide menuStrip(s), because click in document clientArea doesn't hide menu
				if (contextMenuStrip.Visible || SelectMenuVisible())
					ExternalAPI.PostMessage(FindForm().Handle, ExternalAPI.WM_LBUTTONDOWN, IntPtr.Zero, IntPtr.Zero);
			}
			base.WndProc(ref m);
		}

		//------------------------------------------------------------------------------------------------------------------------------
		private bool SelectMenuVisible()
		{
			return DockPanel != null &&
				DockPanel.ActivePane != null &&
				DockPanel.ActivePane.TabStripControl != null &&
				DockPanel.ActivePane.TabStripControl.SelectMenu != null &&
				DockPanel.ActivePane.TabStripControl.SelectMenu.Visible;
		}
		
	}
}
