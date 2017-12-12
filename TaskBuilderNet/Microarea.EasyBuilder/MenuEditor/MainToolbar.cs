using System;
using System.ComponentModel;
using System.ComponentModel.Design;
using System.Windows.Forms;
using Microarea.EasyBuilder.Packager;
using Microarea.EasyBuilder.Properties;
using Microarea.EasyBuilder.UI;
using Microarea.TaskBuilderNet.Core.EasyBuilder;
using Microarea.TaskBuilderNet.UI.WinControls.Dock;
using WeifenLuo.WinFormsUI.Docking;

namespace Microarea.EasyBuilder.MenuEditor
{
	//--------------------------------------------------------------------------------
	/// <remarks/>
	internal partial class MainToolbar : UserControl
	{
		private TBDockContent<PropertyEditor> propertyEditor;
		private DockPanel hostingPanel;

		private ISelectionService selectionService;
		private MenuDesignerForm menuDesignForm;

		//--------------------------------------------------------------------------------
		/// <remarks/>
		public MainToolbar(DockPanel hostingPanel, MenuDesignerForm menuDesignform)
		{
			this.menuDesignForm = menuDesignform;
			this.menuDesignForm.DirtyChanged += new EventHandler<DirtyChangedEventArgs>(MenuDesignForm_DirtyChanged);

			this.selectionService = menuDesignform.SelectionService;

			this.hostingPanel = hostingPanel;
			this.Text = Resources.MainFormTitle;

			//Riduco la dimensione dei docking
			this.hostingPanel.DockLeftPortion = 0.10F;
			this.hostingPanel.DockRightPortion = 0.20F;
			this.hostingPanel.DockTopPortion = 0.06F;
			this.hostingPanel.DockTopPanelMinSize = 50;

			InitializeComponent();
			PostInitializeComponent();
		}

		//--------------------------------------------------------------------------------
		private void PostInitializeComponent()
		{
			tsbSave.Enabled = menuDesignForm.IsDirty;
			tsbApplyChanges.Enabled = menuDesignForm.IsDirty;

			lblActiveApplication.Text = BaseCustomizationContext.CustomizationContextInstance.CurrentEasyBuilderApp.ApplicationName;
			lblActiveModule.Text = BaseCustomizationContext.CustomizationContextInstance.CurrentEasyBuilderApp.ModuleName;

			CreatePropertyEditor();
		}

		//--------------------------------------------------------------------------------
		/// <remarks/>
		protected override void OnHelpRequested(HelpEventArgs hevent)
		{
			base.OnHelpRequested(hevent);
			FormEditor.ShowHelp();
			hevent.Handled = true;
		}

		//-----------------------------------------------------------------------------
		private void CreatePropertyEditor()
		{
			if (propertyEditor != null)
			{
				propertyEditor.Activate();
				return;
			}

			propertyEditor = TBDockContent<PropertyEditor>.CreateDockablePane(
						 hostingPanel,
						 WeifenLuo.WinFormsUI.Docking.DockState.DockRight,
						 System.Drawing.Icon.FromHandle(Resources.Properties.GetHicon()),
						 null,
						 this.selectionService
						 );

			propertyEditor.FormClosed += (FormClosedEventHandler)delegate { propertyEditor.HostedControl.OnClose(); propertyEditor = null; };
		}

		//--------------------------------------------------------------------------------
		internal void OnOpenProperties(object sender, EventArgs e)
		{
			CreatePropertyEditor();
		}

		//-------------------------------------------------------------------------------
		void MenuDesignForm_DirtyChanged(object sender, DirtyChangedEventArgs e)
		{
			tsbSave.Enabled = menuDesignForm.IsDirty;
			tsbApplyChanges.Enabled = menuDesignForm.IsDirty;
		}
	
		//--------------------------------------------------------------------------------
		private void TsbProperties_Click(object sender, EventArgs e)
		{
			CreatePropertyEditor();
		}

		//--------------------------------------------------------------------------------
		private void TsbSaveAndExit_Click(object sender, EventArgs e)
		{
			CancelEventArgs cancelArg = new CancelEventArgs();

			menuDesignForm.AskAndSaveAndClose(cancelArg);
			if (!cancelArg.Cancel)
				menuDesignForm.Close();
		}

		//--------------------------------------------------------------------------------
		private void tsbApplyChanges_Click(object sender, EventArgs e)
		{
			menuDesignForm.SaveCurrentMenuFile(false);
			CUtility.ReloadAllMenus();
		}
	}
}
