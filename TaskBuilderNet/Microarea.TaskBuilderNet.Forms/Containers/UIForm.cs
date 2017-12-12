using System;
using System.ComponentModel;
using System.Diagnostics;
using System.Drawing;
using System.Windows.Forms;
using Microarea.Framework.TBApplicationWrapper;
using Microarea.TaskBuilderNet.Core.Generic;
using Microarea.TaskBuilderNet.Forms.Controls;
using Microarea.TaskBuilderNet.Interfaces.Model;
using Microarea.TaskBuilderNet.Interfaces.View;
using Microarea.TaskBuilderNet.Model.TBCUI;
using Microarea.TaskBuilderNet.Themes;
using Microarea.TaskBuilderNet.UI.WinControls;

namespace Microarea.TaskBuilderNet.Forms
{

	//================================================================================================================
	/// <summary>
	///   Tb Window Form 
	/// </summary>
	[ToolboxItem(false)]
	public partial class UIForm : MenuTabForm, IUIForm
	{
		// fa caricare il tema;
		static ITBThemeProvider theme = TBThemeManager.Theme;

		TBCUIManager cui;

		[Browsable(false)]
		virtual public ITBCUI CUI { get { return cui; } }

		private bool closingDocument = false;
		private bool ownsDocument = true;

			
		private UIToolbarManager uIToolbarManager;
		private UIContextMenuManager uiContextMenuManager;

		public bool OwnsDocument
		{
			get { return ownsDocument; }
			set { ownsDocument = value; }
		}
		public PrimaryToolbarStyle PrimaryToolbarStyle { set { uIToolbarManager.PrimaryToolbarStyle = value; } }
		public AuxiliaryToolbarStyle AuxiliaryToolbarStyle
		{
			set
			{
				uIToolbarManager.AuxiliaryToolbarStyle = value;
			}
		}

		//-------------------------------------------------------------------------
		public IUIToolbar PrimaryToolbar
		{
			get { return uIToolbarManager.PrimaryToolbar; }
		}

		//-------------------------------------------------------------------------
		public IUIToolbar AuxiliaryToolbar
		{
			get { return uIToolbarManager.AuxiliaryToolbar; }
		}

		//-------------------------------------------------------------------------
		public void AddContextMenuItem(IUIComponent ownerControl, IMenuItemGeneric item)
		{
			uiContextMenuManager.AddContextMenuItem(ownerControl, item);
		}

		//-------------------------------------------------------------------------
		public void RemoveContextMenuItem(IUIComponent ownerControl, IMenuItemGeneric item)
		{
			uiContextMenuManager.RemoveContextMenuItem(ownerControl, item);
		}

		//-------------------------------------------------------------------------
		void document_DocumentClosed(object sender, EventArgs e)
		{
			CUI.Document.DocumentClosed -= new EventHandler<EventArgs>(document_DocumentClosed);
			closingDocument = true;
			Close();
		}
		[Browsable(false)]
		[DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
		public IMAbstractFormDoc Document { get { return CUI.Document; } set { CUI.Document = value; } }


		/// <summary>
		///   Constructor
		/// </summary>
		//-------------------------------------------------------------------------
		public UIForm()
		{
			cui = new TBCUIManager(this);
			InitializeComponent();

			uIToolbarManager = new UIToolbarManager();
			uiContextMenuManager = new UIContextMenuManager(this);

			Bitmap bmp = new Bitmap(typeof(Microarea.TaskBuilderNet.UI.MenuManagerWindowsControls.ImageLists).Assembly.GetManifestResourceStream("Microarea.TaskBuilderNet.UI.MenuManagerWindowsControls.Bitmaps.RunDocument.bmp"));
			bmp.MakeTransparent(System.Drawing.Color.Magenta);
			Icon = Icon.FromHandle(bmp.GetHicon());
		}

	
		/// <summary>
		/// Creates and shows the form
		/// </summary>
		/// <param name="control"></param>
		/// <param name="document"></param>
		/// <param name="menuHandle"></param>
		//-------------------------------------------------------------------------
		public void Show(IUIUserControl userControl, IMAbstractFormDoc document, IntPtr menuHandle)
		{
			Debug.Assert(document is MAbstractFormDoc);


			CUI.Document = document;
			CUI.UIManager = CUI as ITBCUIManager;
			CUI.Document.DocumentClosed += new EventHandler<EventArgs>(document_DocumentClosed);

			this.splitContainer.Panel1.Controls.Add(uIToolbarManager);
			UIUserControl control = userControl as UIUserControl;
			if (control != null)
			{
				this.splitContainer.Panel2.Controls.Add(control); //this.control.Parent = this;
				control.Dock = DockStyle.Fill;
			}
			/*UIFormDeprecated form = userControl as UIFormDeprecated;
			if (form != null)
			{
				Panel p = new Panel();
				this.splitContainer.Panel2.Controls.Add(p);
				p.Dock = DockStyle.Fill;
				p.SizeChanged += (sender, args) => { form.Size = p.ClientRectangle.Size; };
				ExternalAPI.SetParent(form.Handle, p.Handle);
				form.Show();
				form.Location = new Point(0, 0);
				form.FormBorderStyle = System.Windows.Forms.FormBorderStyle.None;
				
				((TBCUI)CUI).RecursiveAttach(form.Controls);
			}
			*/
			((TBCUI)CUI).RecursiveAttach(this.Controls);
			userControl.CreateComponents();

			Control userControlAsControl = userControl as Control;
			userControlAsControl.SuspendLayout();
			userControl.CreateExtenders();
			userControlAsControl.ResumeLayout();

			uIToolbarManager.Dock = DockStyle.Fill;

			this.splitContainer.SplitterDistance = uIToolbarManager.ToolbarHeight;

			splitContainer.IsSplitterFixed = true;

			cui.CallCreateComponents();

			Text = cui.Document.Title;
			CUtility.AddWindowRef(Handle, false);

			//Lanciamo qui il form mode changed (anche se il documento non ha effettivamente cambiato stato)
			//per far si che tutti i controller e tutti gli extender si sincronizzino sullo stato attuale del documento.
			//Prima ogni TBCUIControl.AttachDataObj faceva questo lavoro.
			//E` necessario perche` l'evento DocumentFormModeChanged arriva prima della registrazione degli event handler.
			((MAbstractFormDoc)Document).raise_FormModeChanged(Document, EventArgs.Empty);

			base.Show(menuHandle);
            AdjustElementsPosition(); //per rowview dinamica
		}

		//-------------------------------------------------------------------------
		protected override void OnSizeChanged(EventArgs e)
		{
			base.OnSizeChanged(e);
            AdjustElementsPosition();
		}

        //Sposta la view in basso in dipendenza dell'altezza toolbar
        //-------------------------------------------------------------------------
        private void AdjustElementsPosition()
        {
            if (uIToolbarManager != null)
            {
                this.splitContainer.IsSplitterFixed = false;
                this.splitContainer.SplitterDistance = uIToolbarManager.ToolbarHeight;
                this.splitContainer.IsSplitterFixed = true;
                uIToolbarManager.ToolbarWidth = splitContainer.Width;
            }
        }

		/// <summary>
		/// Internal use
		/// </summary>
		/// <param name="e"></param>
		//-------------------------------------------------------------------------
		protected override void OnFormClosing(FormClosingEventArgs e)
		{
			base.OnFormClosing(e);
			if (ownsDocument && !closingDocument && !cui.Document.SaveModified)
				e.Cancel = true;
		}

		/// <summary>
		/// Internal use
		/// </summary>
		/// <param name="e"></param>
		//-------------------------------------------------------------------------
		protected override void OnFormClosed(FormClosedEventArgs e)
		{
			if (ownsDocument && !closingDocument)
				cui.Document.Close();
			if (!IsDisposed)
			{
				CUtility.RemoveWindowRef(Handle, false);
			}
			base.OnFormClosed(e);
		}

		//-------------------------------------------------------------------------
		protected override bool ProcessDialogKey(Keys keyData)
		{
			base.ProcessDialogKey(keyData);

			if (uIToolbarManager.ProcessShortcut(keyData))
				return true;

			return uiContextMenuManager.ProcessShortcut(keyData);
		}

		//-------------------------------------------------------------------------
		protected override void WndProc(ref Message m)
		{
			if (m.Msg == ExternalAPI.UM_UPDATE_FRAME_STATUS)
			{
				cui.HostingHandle = m.LParam;
			}

			base.WndProc(ref m);
		}
	}
}
