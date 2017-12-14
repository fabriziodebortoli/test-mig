using System;
using System.ComponentModel;
using System.Windows.Forms;
using System.Xml;
using Microarea.EasyBuilder.MenuEditor;
using Microarea.TaskBuilderNet.Core.Generic;
using Microarea.TaskBuilderNet.Core.MenuManagerLoader;
using Microarea.TaskBuilderNet.Core.WebServicesWrapper;
using Microarea.TaskBuilderNet.Interfaces;
using WeifenLuo.WinFormsUI.Docking;

namespace Microarea.MenuManager
{
	//=========================================================================
	public partial class MenuEditorDockableForm : DockContent
	{
		private MenuDesignerForm menuDesignerForm;
		private XmlDocument fullMenuXmlDoc;

		private DockPanel contentPanel;
		private Panel outerPanel;

		//---------------------------------------------------------------------
		/// <remarks />
		[Browsable(false)]
		public string CurrentUser
		{
			get { return menuDesignerForm.CurrentUser; }
			set { menuDesignerForm.CurrentUser = value; }
		}

		//---------------------------------------------------------------------
		/// <remarks />
		[Browsable(false)]
		public bool AtLeastOnModificationOccurred
		{
			get { return (menuDesignerForm!= null && menuDesignerForm.AtLeastOnModificationOccurred); }
		}

		//---------------------------------------------------------------------
		public DockPanel ContentDockPanel
		{
			get { return contentPanel; }
		}
		
		//---------------------------------------------------------------------
		public MenuEditorDockableForm(IPathFinder pathFinder, LoginManager loginManager)
		{
			InitializeComponent();

			MenuLoader menuLoader = new MenuLoader(pathFinder);
			menuLoader.LoadAllMenus(false, false);

			this.fullMenuXmlDoc = menuLoader.AppsMenuXmlParser.MenuXmlDoc;

			CreateHostingPanel(loginManager);
		}

		//---------------------------------------------------------------------
		private void CreateHostingPanel(LoginManager loginManager)
		{
			//creo la form che ospita il menu editor
			this.menuDesignerForm = new MenuDesignerForm(this.fullMenuXmlDoc, loginManager.UserName);
			this.menuDesignerForm.FormClosed += new FormClosedEventHandler(MenuDesignerForm_FormClosed);
			
			//creo il pannello esterno
			outerPanel = new Panel();
			outerPanel.Dock = DockStyle.Fill;
			this.Controls.Add(outerPanel);
			
			//creo il pannello per il docking
			contentPanel = new DockPanel();
			contentPanel.Dock = DockStyle.Fill;
			contentPanel.ShowCloseButtonOnTab = true;
			contentPanel.ShowDocumentIcon = true;
			
			//aggiungo il pannello di docking al proprio contenitore
			outerPanel.Controls.Add(contentPanel);

			contentPanel.DocumentStyle = DocumentStyle.DockingWindow;

			DictionaryFunctions.SetCultureInfo(loginManager.PreferredLanguage, loginManager.ApplicationLanguage);

			menuDesignerForm.Show(contentPanel, WeifenLuo.WinFormsUI.Docking.DockState.Document);
			menuDesignerForm.Pane.HideDocumentTabs = true;//non voglio vedere la linguetta del documento

			this.Text = menuDesignerForm.Text;
			menuDesignerForm.TextChanged += new EventHandler(MenuDesignerForm_TextChanged);
		}

		//---------------------------------------------------------------------
		protected override void OnClosing(CancelEventArgs e)
		{
			menuDesignerForm.AskAndSaveAndClose(e);

			base.OnClosing(e);
		}

		//---------------------------------------------------------------------
		void MenuDesignerForm_FormClosed(object sender, FormClosedEventArgs e)
		{
			Close();
		}

		//---------------------------------------------------------------------
		void MenuDesignerForm_TextChanged(object sender, EventArgs e)
		{
			 this.Text = menuDesignerForm.Text;
		}
	}
}
