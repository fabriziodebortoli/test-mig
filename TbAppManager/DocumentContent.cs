using System;
using System.ComponentModel;
using System.Windows.Forms;
using Microarea.TaskBuilderNet.Core.Generic;
using WeifenLuo.WinFormsUI.Docking;

namespace Microarea.MenuManager
{
	//================================================================================
	public partial class DocumentContent : DockContent 
	{
        private IntPtr windowHandle;
		bool minimized = false;
		private DocumentDockableForm dockableForm;
        private MenuStrip stripMenu = null;
        bool canClose;

        //--------------------------------------------------------------------------------
        public DocumentContent(IntPtr windowHandle, DocumentDockableForm dockableForm)
		{
			this.windowHandle = windowHandle;
			this.dockableForm = dockableForm;
			InitializeComponent();

            // Accessibility - Property used to uniquely identify an object by Ranorex Spy
            string[] ns = dockableForm.DocumentNamespace.Split('.');
            AccessibleName = ns[ns.Length - 1] + "DocumentContent";
		}

		//------------------------------------------------------------------------------------------------------------------------------
		internal void MergeMenu(WindowItem item)
		{
			BeginInvoke((Action)delegate {
				if (item != null && item.DocumentMenu != null && item.DocumentMenu.Items.Count > 0)
				{
                    stripMenu = item.DocumentMenu;
                    this.ToolStripContainer.TopToolStripPanel.Controls.Add(stripMenu);
				}
			});
		}

		//------------------------------------------------------------------------------------------------------------------------------
		protected override void WndProc(ref Message m)
		{
			if (Disposing)
				return;

            if (m.Msg == ExternalAPI.UM_DESTROYING_DOCKABLE_FRAME)
            {
                canClose = true;
                return;
            }

            base.WndProc(ref m);
		}

        //------------------------------------------------------------------------------------------------------------------------------
        protected override void OnClosing(CancelEventArgs e)
        {
            e.Cancel = !canClose;
            base.OnClosing(e);
        }

        //------------------------------------------------------------------------------------------------------------------------------
        void ClonedMenuItem_Click(object sender, EventArgs e)
		{
			((ToolStripItem)((ToolStripItem)sender).Tag).PerformClick();
		}

		//------------------------------------------------------------------------------------------------------------------------------
		protected override void OnSizeChanged(EventArgs e)
		{
			base.OnSizeChanged(e);
			if (!dockableForm.Attached)//solo quando ho finito il processo di attach effettuo il resize, per evitare chiamate inutili
                return;

            //an. 17333: se minimizzo la finestra o la ripristino da minimizzata, passa di qui ma    
            //non deve fare la MoveWindow altrimenti si innesta un meccanismo di fuochi che dà fastidio al bodyedit
            //(fa una SetValue di troppo col valore vecchio, quindi perdo le modifiche al parsed control che stavo editando)
            Form ownerForm = FindForm();
            if (ownerForm != null)
            {
                if (ownerForm.WindowState == FormWindowState.Minimized)
                {
                    minimized = true;
                    return;
                }

                if (minimized)
                {
                    minimized = false;
                    return;
                }
            }
            int height = stripMenu != null ? Height - stripMenu.Height : Height;
            ExternalAPI.MoveWindow(windowHandle, 0, 0, Width, height, true);
		}

		/// <summary>
		/// Clean up any resources being used.
		/// </summary>
		/// <param name="disposing">true if managed resources should be disposed; otherwise, false.</param>
		//------------------------------------------------------------------------------------------------------------------------------
		protected override void Dispose(bool disposing)
        {
			if (stripMenu != null)
			{
				stripMenu.Dispose();
				stripMenu = null;
			}

			if (disposing && (components != null))
            {
                components.Dispose();
            }

			base.Dispose(disposing);
        }

    }
}
