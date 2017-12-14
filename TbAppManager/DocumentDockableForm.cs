using System;
using System.ComponentModel;
using System.Drawing;
using System.Runtime.InteropServices;
using System.Threading;
using System.Windows.Forms;
using Microarea.TaskBuilderNet.Core.Generic;
using WeifenLuo.WinFormsUI.Docking;

namespace Microarea.MenuManager
{
	public partial class DocumentDockableForm : GenericDockableForm
	{
		//trucco per eseguire del codice nel thread di documento:
		//aggancio una native window all'handle di finestra mi inserisco nella window proc
		//poi quando ho eseguito il codice mi sgancio
		public class MyNativeWindow : NativeWindow, IDisposable
		{
			private bool alreadyCreated = false;
			private DocumentDockableForm documentDockableForm;

			//------------------------------------------------------------------------------------------------------------------------------
			public MyNativeWindow(DocumentDockableForm documentDockableForm)
			{
				this.documentDockableForm = documentDockableForm;
			}

			//------------------------------------------------------------------------------------------------------------------------------
			public void Dispose()
			{
				ReleaseHandle();
				if (documentDockableForm != null)
					documentDockableForm = null;
			}

			//------------------------------------------------------------------------------------------------------------------------------
			protected override void WndProc(ref Message m)//questo metodo viene eseguito nel thread di documento
			{
				//la prima volta i sgancio, non mi serve più questa classe
				//ReleaseHandle();
				//creo il pannello che ospita le finestre dockabili nel thread di documento
				if (!alreadyCreated)
				{
					documentDockableForm.CreateHostingPanel();
					alreadyCreated = true;
				}

				if (m.Msg == ExternalAPI.WM_DESTROY)
				{
					documentDockableForm.DestroyHostingPanel();
					ReleaseHandle();
				}

				base.WndProc(ref m);
			}

		}

		IntPtr windowHandle;

		private string documentNamespace;
		private DocumentState state = null;
		private DocumentStates documentStates = null;
			
		private DocumentContent content;	//form che ospita il contenuto del documento (la frame di documento è figlia di questa form)
		private DockPanel contentPanel;		//pannello che ospita le finestre dockabili
		private Panel outerPanel;			//pannello esterno necessario per fornire una parent al contentPanel (necessario nelle logiche di docking)
			
		private IntPtr contentHandle;
		private IntPtr outerPanelHandle;
		private string fullText;
		private string nickName;
		
		MyNativeWindow nativeWindow;
		internal bool attached = false;
		//------------------------------------------------------------------------------------------------------------------------------
		public bool Attached { get { return attached; } set { attached = value; ResizeOuterPanel(); } }
		//------------------------------------------------------------------------------------------------------------------------------
		public string DocumentNamespace { get { return documentNamespace; } }
		//------------------------------------------------------------------------------------------------------------------------------
		public IntPtr HostingHandle { get { return contentHandle; } }
		//------------------------------------------------------------------------------------------------------------------------------
		public IntPtr WindowHandle { get { return windowHandle; } }
		//------------------------------------------------------------------------------------------------------------------------------
		public override bool CanClone { get { return true; } }
		//------------------------------------------------------------------------------------------------------------------------------
		public override bool CanBeDeactivated 
		{ 
			get 
			{
				bool b = true;
				//su task parallelo per evitare deadlock in caso di finestra modale!
				Functions.DoParallelProcedure((Action)delegate
				{
					IntPtr result = ExternalAPI.SendMessage(WindowHandle, ExternalAPI.UM_HAS_INVALID_VIEW, IntPtr.Zero, IntPtr.Zero);
					b = result.ToInt32() == 0;
				});
				
				return b;
			} 
		}

		//------------------------------------------------------------------------------------------------------------------------------
		public DocumentDockableForm(IntPtr windowHandle, DocumentStates documentStates)
		{
			this.windowHandle = windowHandle;
			this.documentStates = documentStates;
	
			int nChars = 1024;
			IntPtr hGlobal = Marshal.AllocHGlobal(nChars*2);;//1 carattere=2 bytes
			IntPtr hIcon = ExternalAPI.SendMessage(windowHandle, ExternalAPI.UM_GET_DOC_NAMESPACE_ICON, hGlobal, (IntPtr)nChars);
			documentNamespace = Marshal.PtrToStringUni(hGlobal);
            Marshal.FreeHGlobal(hGlobal);
			//loading document icon
			if (hIcon.ToInt32() != 0) 
				this.Icon = Icon.FromHandle(hIcon);

			if (documentStates != null && ExternalAPI.SendMessage(windowHandle, ExternalAPI.UM_IS_ROOT_DOCUMENT, IntPtr.Zero, IntPtr.Zero) == (IntPtr)1)
			{
				state = documentStates.Get(documentNamespace);
				documentStates.AddNamespace(documentNamespace);
			}
			else
			{
				state = new DocumentState();
			}
			InitializeComponent();

			this.DockStateChanged += new System.EventHandler(this.DocumentTabPage_DockStateChanged);

			nativeWindow = new MyNativeWindow(this);
			nativeWindow.AssignHandle(windowHandle);
			UpdateTitle();

            // Accessibility - Property used to uniquely identify an object by Ranorex Spy
            string[] ns = documentNamespace.Split('.');
            AccessibleName = ns[ns.Length - 1] + "DocumentDockableForm";
        }

        //------------------------------------------------------------------------------------------------------------------------------
        private void DestroyHostingPanel()
		{
			if (content != null)
			{
				content.Dispose();
				content = null;
			}

			if (contentPanel != null)
			{
				contentPanel.Dispose();
				contentPanel = null;
			}

			if (outerPanel != null)
			{
				outerPanel.Dispose();
				outerPanel = null;
			}

			GC.Collect();
		}

		//------------------------------------------------------------------------------------------------------------------------------
		private void CreateHostingPanel()
		{
			//creo la form che ospita il documento
			content = new DocumentContent(WindowHandle, this);
			//creo il pannello esterno
			outerPanel = new Panel();
			//creo il pannello per il docking
			contentPanel = new DockPanel();
			contentPanel.Dock = DockStyle.Fill;
			contentPanel.ShowCloseButtonOnTab = true;
			contentPanel.ShowDocumentIcon = true;
			//aggiungo il pannello di docking al proprio contenitore
			outerPanel.Controls.Add(contentPanel);

			contentPanel.DocumentStyle = DocumentStyle.DockingWindow;
			content.Show(contentPanel, DockState.Document);
			content.Pane.HideDocumentTabs = true;//non voglio vedere la linguetta del documento
			contentHandle = content.ToolStripContainer.ContentPanel.Handle;
			outerPanelHandle = outerPanel.Handle;
   		}

		//------------------------------------------------------------------------------------------------------------------------------
		public void AlignParent()
		{
			ExternalAPI.SetParent(outerPanelHandle, Handle);
		}
		
		//-----------------------------------------------------------------------------------------
		protected override void OnSizeChanged(EventArgs e)
		{
			base.OnSizeChanged(e);
			ResizeOuterPanel();
		}

		//-----------------------------------------------------------------------------------------
		private void ResizeOuterPanel()
		{
            if (outerPanel == null || !attached || Disposing)
				return;

			try
			{
				Rectangle rect = ClientRectangle;
				outerPanel.BeginInvoke((ThreadStart)delegate
				{
					outerPanel.Location = rect.Location;
					outerPanel.Size = rect.Size;
				});

				SaveFormRectangleIfNeeded();
			}
			catch
			{
				//in fase di chiusura del documento potrebbe dare eccezione

			}
		}
		
		//-----------------------------------------------------------------------------------------
		private void SaveFormRectangleIfNeeded()
		{
			if (state.DockState == WeifenLuo.WinFormsUI.Docking.DockState.Float
				&& this.FloatPane != null 
				&& this.FloatPane.FloatWindow != null)
			{
				Form f = this.FloatPane.FloatWindow;
				if (f.WindowState == FormWindowState.Normal)
					state.Rectangle = new Rectangle(f.Location, f.Size);
				state.WindowState = f.WindowState;
			}
		}
		//-----------------------------------------------------------------------------------------
		/// <summary>
		/// Aggiorna il titolo della tab ed il suo tooltip chiedendolo alla finestra associata
		/// </summary>
		/// <param name="windowHandle"></param>
		public void UpdateTitle()
		{
			int nChars = 1024;
			IntPtr hGlobal = Marshal.AllocHGlobal(nChars*2);//1 carattere=2 bytes
			int nCharSeparator = (int)ExternalAPI.SendMessage(windowHandle, ExternalAPI.UM_GET_DOCUMENT_TITLE_INFO, hGlobal, (IntPtr)nChars);
			fullText = Marshal.PtrToStringUni(hGlobal);
			Marshal.FreeHGlobal(hGlobal);
			
			nickName = nCharSeparator > 0
				? fullText.Substring(0, nCharSeparator).Trim()
				: fullText;
			string tip = nCharSeparator > 0
				? fullText.Substring(nCharSeparator).TrimStart(new char[]{' ', '-'})
				: fullText;
			Text = nickName;
			ToolTipText = tip;
		}

		//------------------------------------------------------------------------------------------------------------------------------
		private void DocumentTabPage_DockStateChanged(object sender, EventArgs e)
		{
			if (this.DockState == WeifenLuo.WinFormsUI.Docking.DockState.Float)
			{
				if (DocumentMenu != null)
				{
					this.FloatPane.FloatWindow.MainMenuStrip = DocumentMenu;
					DocumentMenu.Visible = true;

					//aggiungo il menu nei controlli della finestra floating (viene tolto dai controlli del parent corrente)
					this.FloatPane.FloatWindow.Controls.Add(DocumentMenu);
					DocumentMenu.BringToFront();
				}
				this.FloatPane.FloatWindow.FormBorderStyle = System.Windows.Forms.FormBorderStyle.Sizable;
				this.FloatPane.FloatWindow.ShowInTaskbar = true;
				this.FloatPane.FloatWindow.Icon = this.Icon;
				this.FloatPane.FloatWindow.Move += new EventHandler(FloatWindow_Move);
				Text = fullText;
			}
			else if (this.FloatPane != null && this.FloatPane.FloatWindow != null)
			{
				this.FloatPane.FloatWindow.ShowInTaskbar = false;
				this.FloatPane.FloatWindow.Move -= new EventHandler(FloatWindow_Move);
				this.FloatPane.FloatWindow.MainMenuStrip = null;
				Text = nickName;
			}

			if (this.DockState != WeifenLuo.WinFormsUI.Docking.DockState.Unknown)
			{
				state.DockState = this.DockState;
				ResizeOuterPanel();
			}
		}

		//-----------------------------------------------------------------------------------------
		void FloatWindow_Move(object sender, EventArgs e)
		{
			SaveFormRectangleIfNeeded();
		}

		//------------------------------------------------------------------------------------------------------------------------------
		protected override void OnActivated(EventArgs e)
		{
			//non devo fare postmessage, altrimenti non apre il radar alternativo sugli articoli (piu' in generale, la Activate arriva
			//quando il radar e' gia' aperto, e questo si richiude subito
			ActivateFormWindow(true, false);
			base.OnActivated(e);
		}

		//------------------------------------------------------------------------------------------------------------------------------
		protected override void OnVisibleChanged(EventArgs e)
		{
			//non devo fare postmessage, altrimenti non apre il radar alternativo sugli articoli (piu' in generale, la Activate arriva
			//quando il radar e' gia' aperto, e questo si richiude subito
			ActivateFormWindow(Visible, false);
			ActivateFormWindowChilds(Visible);
			base.OnVisibleChanged(e);
		}

		//------------------------------------------------------------------------------------------------------------------------------
		private void ActivateFormWindowChilds(bool activate)
		{
			ExternalAPI.PostMessage
				(
				WindowHandle,
				ExternalAPI.UM_ACTIVATE_TAB,
				(IntPtr)(activate ? 1 : 0),
				IntPtr.Zero
				);
		}
		//------------------------------------------------------------------------------------------------------------------------------
		protected override void OnDeactivate(EventArgs e)
		{
			ActivateFormWindow(false, false);
			base.OnDeactivate(e);
		}

		//------------------------------------------------------------------------------------------------------------------------------
		public void ActivateAppFormWindow(bool activate, bool posted)
		{
			if (activate)
				BringToFront();

			if (posted)
				ExternalAPI.PostMessage
					(
					WindowHandle,
					ExternalAPI.WM_ACTIVATEAPP,
					activate ? (IntPtr)ExternalAPI.WA_ACTIVE : (IntPtr)ExternalAPI.WA_INACTIVE,
					IntPtr.Zero
					);
			else
				ExternalAPI.SendMessage
					(
					WindowHandle,
					ExternalAPI.WM_ACTIVATEAPP,
					activate ? (IntPtr)ExternalAPI.WA_ACTIVE : (IntPtr)ExternalAPI.WA_INACTIVE,
					IntPtr.Zero
					);
		}

		//------------------------------------------------------------------------------------------------------------------------------
		public override void ActivateFormWindow(bool activate, bool posted)
		{
			if (activate)
				BringToFront();

			if (posted)
				ExternalAPI.PostMessage
					(
					WindowHandle,
					ExternalAPI.WM_ACTIVATE,
					activate ? (IntPtr)ExternalAPI.WA_ACTIVE : (IntPtr)ExternalAPI.WA_INACTIVE,
					IntPtr.Zero
					);
			else
				ExternalAPI.SendMessage
					(
					WindowHandle,
					ExternalAPI.WM_ACTIVATE,
					activate ? (IntPtr)ExternalAPI.WA_ACTIVE : (IntPtr)ExternalAPI.WA_INACTIVE,
					IntPtr.Zero
					);
		}


		//--------------------------------------------------------------------------------
		protected override void OnClosing(CancelEventArgs e)
		{
			e.Cancel = true;
			if (ExternalAPI.IsWindowEnabled(windowHandle))
				ExternalAPI.PostMessage(windowHandle, ExternalAPI.WM_CLOSE, IntPtr.Zero, IntPtr.Zero);
			base.OnClosing(e);
		}

		//--------------------------------------------------------------------------------
		protected override void Clone()
		{
			ExternalAPI.PostMessage(WindowHandle, ExternalAPI.UM_CLONE_DOCUMENT, IntPtr.Zero, IntPtr.Zero);
		}

		//--------------------------------------------------------------------------------
		internal void MergeMenu(WindowItem item)
		{
			content.MergeMenu(item);
		}

		/// <summary>
		/// Clean up any resources being used.
		/// </summary>
		/// <param name="disposing">true if managed resources should be disposed; otherwise, false.</param>
		protected override void Dispose(bool disposing)
		{
			base.Dispose(disposing);
			if (nativeWindow != null)
			{
				nativeWindow.Dispose();
				nativeWindow = null;
			}

			if (disposing)
			{
				if (components != null)
					components.Dispose();

				if (documentStates != null)
					documentStates.RemoveNamespace(documentNamespace);

				DestroyHostingPanel();

				outerPanelHandle = IntPtr.Zero;
				contentHandle = IntPtr.Zero;

				ExternalAPI.DestroyIcon(this.Icon.Handle);
			}

		}

	}

}
