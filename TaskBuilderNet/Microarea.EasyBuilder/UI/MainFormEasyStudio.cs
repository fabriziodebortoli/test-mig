using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.ComponentModel.Design;
using System.Drawing;
using System.Globalization;
using System.Threading;
using System.Windows.Forms;
using Microarea.EasyBuilder.CodeEditorProviders;
using Microarea.EasyBuilder.ComponentModel;
using Microarea.EasyBuilder.Properties;
using Microarea.Framework.TBApplicationWrapper;
using Microarea.TaskBuilderNet.Core.CoreTypes;
using Microarea.TaskBuilderNet.Core.Generic;
using Microarea.TaskBuilderNet.UI.WinControls.Dock;
using WeifenLuo.WinFormsUI.Docking;
using ICSharpCode.NRefactory.CSharp;
using Microarea.TaskBuilderNet.Core.EasyBuilder;

namespace Microarea.EasyBuilder.UI
{
	//--------------------------------------------------------------------------------
	/// <remarks/>
	public partial class MainFormEasyStudio : MainForm
	{

		/* enum PanelItems in the same order as the toolbar*/
		internal TBDockContent<ToolboxControl> toolboxControl;
		internal TBDockContent<Localization> localization;
		internal TBDockContent<PropertyEditor> propertyEditor;
		internal TBDockContent<ObjectModelTreeControl> objectModelTree;
		internal TBDockContent<ViewOutlineTreeControl> viewOutlineTree;
		internal TBDockContent<DatabaseExplorer> dbExplorer;
		internal TBDockContent<HotLinksExplorer> hotLinksExplorer;
		internal TBDockContent<BusinessObjectsExplorer> businessObjectsExplorer;
		internal TBDockContent<EnumsTreeControl> enumsTreeControl;
		/* enum PanelItems*/

		private IEasyBuilderCodeEditor codeEditorForm;
		private DockPanel hostingPanel;
		internal FormEditor formEditor;

		/// <remarks/>
		public IWin32Window OwnerWindow { get { return this; } }

		/// <summary>
		/// If the UserMethods file has no methods, the button OpenCodeeditor is disabled
		/// </summary>
		public bool IsUserMethodsNotEmpty
		{
			get { return formEditor?.Sources?.GetUserMethods().Count > 0;  }
		}

		/// <remarks/>
		//--------------------------------------------------------------------------------
		public MainFormEasyStudio(DockPanel hostingPanel, FormEditor formEditor)
		{
			InitializeComponent();

			this.formEditor		= formEditor;
			this.hostingPanel	= hostingPanel;
			this.Text			= Resources.MainFormTitle;

			//Riduco la dimensione dei docking
			this.hostingPanel.DockLeftPortion		= 0.12F;
			this.hostingPanel.DockRightPortion		= 0.20F;
			this.hostingPanel.DockTopPortion		= 0.06F;
			this.hostingPanel.DockTopPanelMinSize	= 50;

			formEditor.OpenCodeEditor				+= OpenCodeEditor;
			formEditor.CreateControllerCompleted	+= FormEditor_CreateControllerCompleted;
			formEditor.RequestedSave			+= new EventHandler(FormEditor_RequestedSave);
			formEditor.RequestedOptions			+= new EventHandler(FormEditor_RequestedOptions);
			formEditor.RequestedOpenCodeEditor  += new EventHandler(FormEditor_RequestedOpenCodeEditor);
			formEditor.RequestedObjectModel		+= new EventHandler(FormEditor_RequestedDocumentModel);
			formEditor.DirtyChanged				+= new EventHandler<DirtyChangedEventArgs>(FormEditor_DirtyChanged);
			formEditor.SelectedObjectUpdated	+= new EventHandler(FormEditor_SelectedObjectUpdated);
			formEditor.HotLinkAdded				+= new EventHandler<EventArgs>(formEditor_HotLinkAdded);

			Site = new TBSite(this, formEditor, formEditor, "MainForm");

			CreateToolboxControl();
			CreatePropertyEditor();
			CreateObjectModelWindow();
			CreateViewOutline();

			//popolo la combo delle lingue selezionabili
			RefreshComboLanguages();

			if (tscomboLanguages.ComboBox != null)
			{
				tscomboLanguages.ComboBox.SelectedItem = Thread.CurrentThread.CurrentUICulture;

				if (this.tscomboLanguages.ComboBox.SelectedItem == null)//se non ho la lingua nella combo, allora ripristino quella del thread corrente a invariant
				{
					this.tscomboLanguages.ComboBox.SelectedItem = CultureInfo.InvariantCulture;
					Thread.CurrentThread.CurrentUICulture = CultureInfo.InvariantCulture;
				}
			}

			EditorImages.ViewModelImages = ImageLists.ViewModelImages;
			EditorImages.DataModelImages = ImageLists.ObjectModelTree;

			toolStripLabelActiveAppAndMod.Text = "Context:";
			if (BaseCustomizationContext.CustomizationContextInstance.CurrentEasyBuilderApp != null)
			{
				lblActiveAppAndModule.Text =
					BaseCustomizationContext.CustomizationContextInstance.CurrentEasyBuilderApp.ApplicationName.ToString() + ", "
					+ BaseCustomizationContext.CustomizationContextInstance.CurrentEasyBuilderApp.ModuleName.ToString();
			}
			tsbSave.Enabled = false;
			EnableCodeEditorButton();
		}

		/// <remarks/>
		//-------------------------------------------------------------------------------
		public override void EnableCodeEditorButton()
		{
			if(tsbOpenCodeEditor!=null)
				tsbOpenCodeEditor.Enabled = IsUserMethodsNotEmpty;
		}

		/// <summary>
		/// Clean up any resources being used.
		/// </summary>
		/// <param name="disposing">true if managed resources should be disposed; otherwise, false.</param>
		//-------------------------------------------------------------------------------
		protected override void Dispose(bool disposing)
		{
			if (disposing)
			{
				components?.Dispose();

				CloseCodeEditorsThreadContext();

				SafeDispose<TBDockContent<EnumsTreeControl>>(ref enumsTreeControl);
				SafeDispose<TBDockContent<Localization>>(ref localization);
				SafeDispose<TBDockContent<DatabaseExplorer>>(ref dbExplorer);
				SafeDispose<TBDockContent<PropertyEditor>>(ref propertyEditor);
				SafeDispose<TBDockContent<ObjectModelTreeControl>>(ref objectModelTree);

				
				SafeDispose<TBDockContent<ToolboxControl>>(ref toolboxControl);
				SafeDispose<TBDockContent<HotLinksExplorer>>(ref hotLinksExplorer);
				SafeDispose<TBDockContent<BusinessObjectsExplorer>>(ref businessObjectsExplorer);

				SafeDispose<TBDockContent<ViewOutlineTreeControl>>(ref viewOutlineTree);


				if (formEditor != null)
				{
					formEditor.DirtyChanged -= new EventHandler<DirtyChangedEventArgs>(FormEditor_DirtyChanged);
				}

			}
			base.Dispose(disposing);
		}

		//-------------------------------------------------------------------------------
		internal void SafeDispose<T>(ref T ctrl) where T : DockContent
		{
			if (ctrl != null)
			{
				try
				{
					ctrl.Close();
					ctrl = null;
				}
				catch { }
			}
		}

		//-------------------------------------------------------------------------------
		void estb_ItemClicked(object sender, ToolStripItemClickedEventArgs e)
		{
			object o = e.ClickedItem.Tag;
			if (o != null && (o is Selections.Action))
				formEditor.PerformAlignAction((Selections.Action)o);
		}

		//-------------------------------------------------------------------------------
		internal void FormEditor_RequestedSave(object sender, EventArgs e)
		{
			formEditor.Save(false);
		}

		/// <summary>
		/// Chiede a tutte le finestre di codeedit con modifiche appese di travasare il codice nei sorgenti, in vista di 
		/// una successiva build
		/// </summary>
		//--------------------------------------------------------------------------------
		public override bool CollectAndSaveCodeChanges(bool runIfClosed)
		{
			if (codeEditorForm == null)
			{
				if (runIfClosed)
					RunCodeEditorForm(null, true);
				else
					return true;		
			}

			return codeEditorForm.CollectAndSaveCodeChanges();
		}

		/// <summary>
		/// se è stato cambiato il NS della personalizzazione, devo rigenerare userMethods.cs per avere in giusto NS
		/// </summary>
		//--------------------------------------------------------------------------------
		public override bool UpdateCodeEditor(ControllerSources sources)
		{
			bool reopenCodeEditor = codeEditorForm != null;
			CloseCodeEditorsThreadContext();

			//a questo punto, userMethodCode e userCompilation sono disallineati, ognuno ha un'info corretta e una sbagliata
			//nel caso in cui apro ES, cambio nome dell apersonalizz, e poi apro subito il code editor.
			string codemethods = sources.CustomizationInfos.UserMethodsCode;
			string usings = codemethods.ToString().Split(new string[] { "namespace" }, StringSplitOptions.None)[0];

			var formattingOptions = FormattingOptionsFactory.CreateAllman();			
			string codeCompilation = sources.CustomizationInfos.UserMethodsCompilationUnit.ToString(formattingOptions);
			string body = codeCompilation.Split(new string[] { "namespace" }, StringSplitOptions.None)[1];

			string newCode = usings + "namespace"+body;
			sources.CustomizationInfos.UpdateUserMethodsCompilationUnit(newCode, sources.GetUserMethodsFilePath());
			return reopenCodeEditor;
		}

		/// <summary>
		/// Chiede a tutte le finestre di codeedit con modifiche appese di travasare il codice nei sorgenti, in vista di 
		/// una successiva build
		/// </summary>
		//--------------------------------------------------------------------------------
		public override void CollectOriginalMethodBodys()
		{
			if (codeEditorForm == null)
				return;

			codeEditorForm.CollectOriginalMethodBody();
			EnableCodeEditorButton();
		}

		//-------------------------------------------------------------------------------
		internal void FormEditor_DirtyChanged(object sender, DirtyChangedEventArgs e)
		{
			if (this.InvokeRequired)
			{
				this.Invoke(new Action(() => FormEditor_DirtyChanged(sender, e)));
				return;
			}
			tsbSave.Enabled = e.Dirty;
		}

		//-------------------------------------------------------------------------------
		internal void ModuleEditor_DirtyChanged(object sender, DirtyChangedEventArgs e)
		{
			if (this.InvokeRequired)
			{
				this.Invoke(new Action(() => FormEditor_DirtyChanged(sender, e)));
				return;
			}
			tsbSave.Enabled = e.Dirty;
		}

		/// <summary>
		/// 
		/// </summary>
		//-----------------------------------------------------------------------------
		public override void CloseCodeEditorsThreadContext()
		{
			if (codeEditorForm != null && !codeEditorForm.IsDisposed)
			{
				codeEditorForm.Dispose();
				codeEditorForm = null;
			}
		}

		//-------------------------------------------------------------------------------
		private void EnumsTreeControl_DirtyChanged(object sender, DirtyChangedEventArgs e)
		{
			if (this.InvokeRequired)
			{
				this.Invoke(new Action(() => EnumsTreeControl_DirtyChanged(sender, e)));
				return;
			}

			formEditor.SetDirty(e.Dirty);
		}

		//-------------------------------------------------------------------------------
		private void CodeEditor_DirtyChanged(object sender, DirtyChangedEventArgs e)
		{
			if (this.InvokeRequired)
			{
				this.Invoke(new Action(() => CodeEditor_DirtyChanged(sender, e)));
				return;
			}
			formEditor.SetDirty(e.Dirty);
		}

	/*	//-----------------------------------------------------------------------------
		void CloseCodeEditor(object sender, CodeMethodEditorEventArgs e)
		{
			if (codeEditorForm != null)
			{
				codeEditorForm.IgnoreChangesOnFormClosing = e.IgnoreChangesOnFormClosing;
				codeEditorForm.Close();
			}
		}*/

		//--------------------------------------------------------------------------------
		/// <remarks/>
		public override void UpdateObjectViewModel(IComponent component)
		{
			if (
				objectModelTree != null && !objectModelTree.IsDisposed &&
				objectModelTree.HostedControl != null && !objectModelTree.HostedControl.IsDisposed
				)
				objectModelTree.HostedControl.UpdateObjectModel(component);
			if (
				viewOutlineTree != null && !viewOutlineTree.IsDisposed &&
				viewOutlineTree.HostedControl != null && !viewOutlineTree.HostedControl.IsDisposed
				)
				viewOutlineTree.HostedControl.UpdateViewOutline(component);

			codeEditorForm?.SetDirty(true);
		}

		//--------------------------------------------------------------------------------
		/// <remarks/>
		public override void PopulateLocalizationStrings()
		{
			if (localization != null && tscomboLanguages.SelectedItem is CultureInfo)
				localization.HostedControl.PopulateStrings(((CultureInfo)tscomboLanguages.SelectedItem).Name);
		}

		//-----------------------------------------------------------------------------
		/// <remarks/>
		public override void CreatePropertyEditor()
		{
			if (propertyEditor != null)
			{
				propertyEditor.Activate();
				return;
			}
			List<object> args = new List<object>();
			args.Add(formEditor);
			args.Add(formEditor.SelectionService);

			propertyEditor = TBDockContent<PropertyEditor>.CreateDockablePane(
				hostingPanel,
				WeifenLuo.WinFormsUI.Docking.DockState.DockRight,
				Icon.FromHandle(Resources.Properties.GetHicon()),
				args.ToArray()
				);

			ChangeEnablePropertyEditor();

			propertyEditor.HostedControl.Site = new TBSite(null, null, formEditor, "PropertyEditor");
			propertyEditor.FormClosed += (FormClosedEventHandler)delegate
			{ propertyEditor.HostedControl.OnClose(); propertyEditor = null; };
		}

		//-----------------------------------------------------------------------------
		/// <remarks/>
		public override void ChangeEnablePropertyEditor()
		{
			if (propertyEditor == null)
				return;
			propertyEditor.Enabled = formEditor.CanChangeEnablePropertyEditor();
		}

		/// <summary>
		/// Lancia su un altro thread e su un altro Application.Run la form del Code Editor.
		/// In mancanza dell'application.run non viene predisposto il message loop fondamentale 
		/// per il corretto funzionamento del code editor stesso
		/// </summary>
		//-----------------------------------------------------------------------------
		private void RunCodeEditorForm(MethodDeclaration method, bool displayErrors = false)
		{
			//serve perché le form girano su un altro thread e quando si chiudono si tolgono dalla lista
			if (codeEditorForm != null)
			{
				codeEditorForm.Activate();
				codeEditorForm.ReinitializeForm(method);
				return;
			}

			codeEditorForm = CreateCodeEditorForm();

			codeEditorForm.FormClosed += CodeEditorForm_FormClosed;
			codeEditorForm.Disposed += CodeEditorForm_Disposed;
			codeEditorForm.RefreshPropertyGrid += CodeEditorForm_RefreshPropertyGrid;
			codeEditorForm.RefreshIntellisense += CodeEditorForm_RefreshIntellisense;

			codeEditorForm.Show(hostingPanel, WeifenLuo.WinFormsUI.Docking.DockState.Document);
			hostingPanel.ActiveDocumentPane.HideDocumentTabs = false;//voglio vedere la linguetta del documento

			Editor editor = null;
			Sources sources = null;

			if (method == null || formEditor.Sources.ContainsCodeMemberMethod(method))  //default aggiunto per aprire il codeditor dal pulsante toolbar
			{
				editor = formEditor;
				sources = formEditor.Sources;
			}
			else
				throw new InvalidOperationException("Unable to open code editor, unknown sources");

			if (method == null) //se apro il code editor da toolbar, apro l'objectmodel
				objectModelTree?.Activate();

			codeEditorForm.Initialize(editor, method);

			if (displayErrors)
				codeEditorForm.PopulateErrors();
		}

		//-----------------------------------------------------------------------------
		private void CodeEditorForm_Disposed(object sender, EventArgs e)
		{
			IDirtyManager dirtyManager = codeEditorForm as IDirtyManager;
			if (dirtyManager != null)
				dirtyManager.DirtyChanged -= new EventHandler<DirtyChangedEventArgs>(CodeEditor_DirtyChanged);
		}

		//-----------------------------------------------------------------------------
		private void CodeEditorForm_FormClosed(object sender, FormClosedEventArgs e)
		{
			if (codeEditorForm != null)
			{
				codeEditorForm.FormClosed -= CodeEditorForm_FormClosed;
				codeEditorForm.Disposed -= CodeEditorForm_Disposed;
				codeEditorForm.RefreshPropertyGrid -= CodeEditorForm_RefreshPropertyGrid;
				codeEditorForm.RefreshIntellisense -= CodeEditorForm_RefreshIntellisense;
				codeEditorForm = null;
			}

			hostingPanel.ActiveDocumentPane.HideDocumentTabs = true; //voglio vedere la linguetta del documento
			objectModelTree?.Activate();
		}

		//-----------------------------------------------------------------------------
		private void CodeEditorForm_RefreshPropertyGrid(object sender, EventArgs e)
		{
			propertyEditor?.HostedControl?.RefreshPropertyGrid();
		}

		//-----------------------------------------------------------------------------
		private void CodeEditorForm_RefreshIntellisense(object sender, EventArgs e)
		{
			formEditor?.RefreshIntellisense();
		}
		
		//-----------------------------------------------------------------------------
		internal IEasyBuilderCodeEditor CreateCodeEditorForm()
		{
			IEasyBuilderCodeEditor easyBuilderCodeEditor = new CodeEditorForm();

			IDirtyManager dirtyManager = easyBuilderCodeEditor as IDirtyManager;
			if (dirtyManager != null)
				dirtyManager.DirtyChanged += new EventHandler<DirtyChangedEventArgs>(CodeEditor_DirtyChanged);


			return easyBuilderCodeEditor;
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
		internal void OpenSourceCode(object sender, CodeSourceEditorEventArgs e)
		{
			using (SafeThreadCallContext context = new SafeThreadCallContext())
			{
				CodeViewer form = new CodeViewer(e.Source, e.Line, e.Column);
				form.ShowDialog(this);
			}
		}

		//-----------------------------------------------------------------------------
		internal void OpenCodeEditor(object sender, CodeMethodEditorEventArgs e)
		{
			RunCodeEditorForm(e?.Method);
		}

		//-------------------------------------------------------------------------------
		internal void FormEditor_RequestedOpenCodeEditor(object sender, EventArgs e)
		{
			RunCodeEditorForm(null);
		}

		//-----------------------------------------------------------------------------
		internal void CreateEnumsTreeControl()
		{
			if (enumsTreeControl != null)
			{
				enumsTreeControl.Activate();
				return;
			}

			enumsTreeControl = TBDockContent<EnumsTreeControl>.CreateDockablePane(
								hostingPanel,
								WeifenLuo.WinFormsUI.Docking.DockState.DockLeft,
								Icon.FromHandle(Resources.Enums.GetHicon()),
								formEditor.GetService(typeof(ISelectionService)) as ISelectionService,
								formEditor
								);

			enumsTreeControl.FormClosed += (FormClosedEventHandler)delegate	{ enumsTreeControl = null; };
			enumsTreeControl.HostedControl.OpenProperties += new EventHandler<EventArgs>(OnOpenProperties);
			enumsTreeControl.HostedControl.DirtyChanged += new EventHandler<DirtyChangedEventArgs>(EnumsTreeControl_DirtyChanged);
		}

		//-----------------------------------------------------------------------------
		internal void CreateLocalization()
		{
			if (localization != null)
			{
				localization.Activate();
				return;
			}
			CultureInfo cu = tscomboLanguages.SelectedItem as CultureInfo;
			string culture = (cu != null)
				? cu.Name
				: Thread.CurrentThread.CurrentUICulture.Name;

			localization = TBDockContent<Localization>.CreateDockablePane(
						 hostingPanel,
						 WeifenLuo.WinFormsUI.Docking.DockState.DockRight,
						 Icon.FromHandle(Resources.Localize.GetHicon()),
						 formEditor,
						 culture
						 );
			localization.FormClosed += (FormClosedEventHandler)delegate
			{ localization = null; };
		}

		//-----------------------------------------------------------------------------
		internal void CreateDatabaseExplorer()
		{
			if (dbExplorer != null)
			{
				dbExplorer.Activate();
				return;
			}

			try
			{
				Cursor = Cursors.WaitCursor;

				dbExplorer = TBDockContent<DatabaseExplorer>.CreateDockablePane(
							 hostingPanel,
							 WeifenLuo.WinFormsUI.Docking.DockState.DockLeft,
								Icon.FromHandle(Resources.Database.GetHicon()),
								formEditor
							 );
				dbExplorer.HostedControl.OpenProperties += new EventHandler(OnOpenProperties);
				dbExplorer.HostedControl.OpenTwinPanel += new EventHandler(OnOpenTwinPanel);
				dbExplorer.HostedControl.CatalogChanged += new EventHandler(HostedControl_CatalogChanged);
				dbExplorer.FormClosed += (FormClosedEventHandler)delegate
				{ dbExplorer = null; };

			}
			finally
			{
				Cursor = Cursors.Default;
				OnOpenTwinPanel(PanelItems.DatabaseExplorer, EventArgs.Empty);
			}

		}

		//-----------------------------------------------------------------------------
		internal void CreateHotLinksExplorer()
		{
			if (hotLinksExplorer != null)
			{
				hotLinksExplorer.Activate();
				return;
			}

			try
			{
				Cursor = Cursors.WaitCursor;
                Action<TreeNode> treeFiller = null;
                if (this.formEditor.RunAsEasyStudioDesigner)
				    treeFiller = new Action<TreeNode>(
					    (TreeNode aNode) =>
					    {
						    SortedDictionary<string, string> hotLinks = formEditor.Document.GetUnWrappedHotLinks();
						    foreach (string name in hotLinks.Keys)
						    {
							    TreeNode hklNode = new TreeNode(string.Format("{0}: {1}", name, hotLinks[name]), 4, 4);
							    aNode.Nodes.Add(hklNode);
							    hklNode.Tag = name;
						    }
					    }
					    );
				hotLinksExplorer = TBDockContent<HotLinksExplorer>.CreateDockablePane(
									hostingPanel,
									WeifenLuo.WinFormsUI.Docking.DockState.DockLeft,
									Icon.FromHandle(Resources.HotLinks.GetHicon()),
									treeFiller, false
							 );

				hotLinksExplorer.HostedControl.OpenTwinPanel += new EventHandler(OnOpenTwinPanel);
				hotLinksExplorer.FormClosed += (FormClosedEventHandler)delegate
				{ hotLinksExplorer = null; };

			}
			finally
			{
				Cursor = Cursors.Default;
				OnOpenTwinPanel(PanelItems.HotLinksExplorer, EventArgs.Empty);
			}
		}

		//-----------------------------------------------------------------------------
		internal void CreateBusinessObjectsExplorer()
		{
			if (businessObjectsExplorer != null)
			{
				businessObjectsExplorer.Activate();
				return;
			}

			try
			{
				Cursor = Cursors.WaitCursor;

				businessObjectsExplorer = TBDockContent<BusinessObjectsExplorer>.CreateDockablePane(
									hostingPanel,
									WeifenLuo.WinFormsUI.Docking.DockState.DockLeft,
									Icon.FromHandle(Resources.BusinessObjects24x24.GetHicon()),
									formEditor
							 );

				businessObjectsExplorer.HostedControl.OpenTwinPanel += new EventHandler(OnOpenTwinPanel);
				businessObjectsExplorer.FormClosed += (FormClosedEventHandler)delegate
				{ businessObjectsExplorer = null; };

			}
			finally
			{
				Cursor = Cursors.Default;
				OnOpenTwinPanel(PanelItems.BusinessObjectsExplorer, EventArgs.Empty);
			}
		}

		//-----------------------------------------------------------------------------
		internal void HostedControl_CatalogChanged(object sender, EventArgs e)
		{
			formEditor.RefreshRecords();
		}

		//-----------------------------------------------------------------------------
		internal void CreateOptions()
		{
			using (SafeThreadCallContext context = new SafeThreadCallContext())
			{
				SettingForm options = new SettingForm(this.formEditor.Sources);
				if (DialogResult.OK != options.ShowDialog(this))
					return;

				this.formEditor.SetDirty(options.ReferencesChanged);

				formEditor.Controller.View.SwitchVisibility(Settings.Default.ShowHiddelFields);
			}
		}

		//-----------------------------------------------------------------------------
		internal void CreateToolboxControl()
		{
			if (toolboxControl != null)
			{
				toolboxControl.Activate();
				return;
			}
			toolboxControl = TBDockContent<ToolboxControl>.CreateDockablePane(
						 hostingPanel,
						 WeifenLuo.WinFormsUI.Docking.DockState.DockLeft,
						 Icon.FromHandle(Resources.Toolbox.GetHicon()),
						 formEditor.RunAsEasyStudioDesigner
						 );

			toolboxControl.HostedControl.Site = new TBSite(null, null, formEditor, "ToolboxControl");

			toolboxControl.HostedControl.OpenTwinPanel += new EventHandler(OnOpenTwinPanel);
			toolboxControl.FormClosed += (FormClosedEventHandler)delegate
			{ toolboxControl = null; };
			RefreshTemplates();
			OnOpenTwinPanel(PanelItems.ToolboxControl, EventArgs.Empty);
		}

		//-----------------------------------------------------------------------------
		internal void CreateViewOutline()
		{
			if (viewOutlineTree != null)
			{
				viewOutlineTree.Activate();
				return;
			}

			this.hostingPanel.DockRightPortion = 0.15F;
			viewOutlineTree = TBDockContent<ViewOutlineTreeControl>.CreateDockablePane(
						 hostingPanel,
						 WeifenLuo.WinFormsUI.Docking.DockState.DockRight,
						 Icon.FromHandle(Resources.ObjectModel.GetHicon()),
						 formEditor
						 );


			viewOutlineTree.FormClosed += new FormClosedEventHandler(
						(object sender, FormClosedEventArgs e) => { viewOutlineTree = null; this.hostingPanel.DockRightPortion = 0.15F; });

			viewOutlineTree.HostedControl.DeleteObject += new EventHandler<DeleteObjectEventArgs>(HostedControl_DeleteObject);
			viewOutlineTree.HostedControl.OpenProperties += new EventHandler(OnOpenProperties);
		}

		//-----------------------------------------------------------------------------
		internal void CreateObjectModelWindow()
		{
			if (objectModelTree != null)
			{
				objectModelTree.Activate();
				return;
			}
			objectModelTree = TBDockContent<ObjectModelTreeControl>.CreateDockablePane(
						 hostingPanel,
						 WeifenLuo.WinFormsUI.Docking.DockState.DockRight,
						 Icon.FromHandle(Resources.ObjectModel.GetHicon()),
						 formEditor,
						 formEditor.RunAsEasyStudioDesigner
						 );
			objectModelTree.FormClosed += (FormClosedEventHandler)delegate
			{ objectModelTree = null; };

			objectModelTree.HostedControl.OpenProperties += new EventHandler(OnOpenProperties);
			objectModelTree.HostedControl.AddDbt += new EventHandler<AddDataManagerEventArgs>(HostedControl_AddDbt);
			objectModelTree.HostedControl.AddField += new EventHandler<AddFieldEventArgs>(HostedControl_AddField);
			objectModelTree.HostedControl.AddHotLink += new EventHandler<AddHotLinkEventArgs>(HostedControl_AddHotLink);
			objectModelTree.HostedControl.AddDataManager += new EventHandler<AddDataManagerEventArgs>(HostedControl_AddDataManager);
			objectModelTree.HostedControl.DeleteObject += new EventHandler<DeleteObjectEventArgs>(HostedControl_DeleteObject);
			objectModelTree.HostedControl.DeclareComponent += new EventHandler<DeclareComponentEventArgs>(HostedControl_DeclareComponent);
			objectModelTree.HostedControl.RefreshModel += new EventHandler(HostedControl_RefreshModel);

		}

		//-----------------------------------------------------------------------------
		internal void HostedControl_DeclareComponent(object sender, DeclareComponentEventArgs e)
		{
			formEditor.DeclareComponent(e);
		}

		//-----------------------------------------------------------------------------
		internal void HostedControl_RemovingMethod(object sender, CodeMethodEditorEventArgs e)
		{
			formEditor.ComponentDeclarator.ClearReferencedBy(e.Method.Name);
		}
		
		//-----------------------------------------------------------------------------
		internal void HostedControl_AddHotLink(object sender, AddHotLinkEventArgs e)
		{
			//Correlato all'an.20951: rimosso controllo qui perche` gia` fatto a posteriori nel metodo FormEditor.AddHotLink:
			//viene aperta una finestra che permette di impostare il nome all'hotlink e questa finestra filtra
			//gia` per le tabelle nel catalog
			//In caso di aggiunta di nuovo hkl ad un documento e.Source e` stringa vuota.
			//Questo valore non e` buono per il metodo EnsureValidTable.
			//if (dbExplorer != null && 
			//    e.Request == AddHotLinkEventArgs.RequestType.FromTable && 
			//    !dbExplorer.HostedControl.EnsureValidTable(e.Source))
			//    return;

			formEditor.AddHotLink(e);
		}

		//-----------------------------------------------------------------------------
		internal void HostedControl_AddDataManager(object sender, AddDataManagerEventArgs e)
		{
			//Correlato all'an.20951: rimosso controllo qui perche` gia` fatto a posteriori nel metodo FormEditor.AddDataManager:
			//viene aperta una finestra che permette di impostare il nome al datamanager e questa finestra filtra
			//gia` per le tabelle nel catalog.
			//if (dbExplorer != null && !dbExplorer.HostedControl.EnsureValidTable(e.TableName))
			//    return;
			formEditor.AddDataManager(e);
		}

		//-----------------------------------------------------------------------------
		internal void formEditor_HotLinkAdded(object sender, EventArgs e)
		{
			if (hotLinksExplorer != null)
				hotLinksExplorer.HostedControl.RefreshDocument();
		}

		//-----------------------------------------------------------------------------
		internal void FormEditor_SelectedObjectUpdated(object sender, EventArgs e)
		{
			if (
				propertyEditor != null && !propertyEditor.IsDisposed &&
				propertyEditor.HostedControl != null && !propertyEditor.HostedControl.IsDisposed
				)
				propertyEditor.HostedControl.RefreshPropertyGrid();

		}

		//-----------------------------------------------------------------------------
		internal void FormEditor_CreateControllerCompleted(object sender, EventArgs e)
		{
			if (
				objectModelTree != null && !objectModelTree.IsDisposed &&
				objectModelTree.HostedControl != null && !objectModelTree.HostedControl.IsDisposed
				)
				objectModelTree.HostedControl.PopulateTree(formEditor.Controller);

			if (
				viewOutlineTree != null && !viewOutlineTree.IsDisposed &&
				viewOutlineTree.HostedControl != null && !viewOutlineTree.HostedControl.IsDisposed
				)
				viewOutlineTree.HostedControl.UpdateViewOutline(formEditor.Controller);
		}

		//-----------------------------------------------------------------------------
		internal void HostedControl_RefreshModel(object sender, EventArgs e)
		{
			formEditor.RefreshObjectModel();
		}

		//-----------------------------------------------------------------------------
		internal void HostedControl_AddField(object sender, AddFieldEventArgs e)
		{
			if (e.Local)
			{
				//determino un nome univoco
				MSqlRecord rec = (MSqlRecord)e.DataManager.Record;
				int i = 1;
				string name = null;
				while (rec.HasComponent(name = "LocalField" + i))
					i++;
				MLocalSqlRecordItem item = rec.AddLocalField(name, DataType.Integer, 4);
				formEditor.RefreshDocumentClassAndSelectObject(item);

				CreatePropertyEditor();
			}
			else
			{
				CreateDatabaseExplorer();
				dbExplorer.HostedControl.AddField((MSqlRecord)e.DataManager.Record);
			}
		}

		//-----------------------------------------------------------------------------
		internal void HostedControl_DeleteObject(object sender, DeleteObjectEventArgs e)
		{
			formEditor.TryDeleteEasyBuilderComponents(new List<EasyBuilderComponent>() { e.Component });
		}

		//-----------------------------------------------------------------------------
		internal void HostedControl_AddDbt(object sender, AddDataManagerEventArgs e)
		{
			//An.20951: rimosso controllo qui perche` gia` fatto a posteriori nel metodo FormEditor.AddDbt:
			//viene aperta una finestra che permette di impostare il nome al dbt e questa finestra filtra
			//gia` per le tabelle nel catalog.
			//Nel caso di batch e.TableName e` fissato a "EB_LocalData" mentre in caso di aggiunta di nuovo dbt ad un documento e` stringa vuota.
			//Entrambi questi valori non sono buoni per il metodo EnsureValidTable.
			//if (dbExplorer != null && !dbExplorer.HostedControl.EnsureValidTable(e.TableName))
			//    return;
			formEditor.AddDbt(e.TableName, e.Dbt);
		}

		//--------------------------------------------------------------------------------
		private void tsbAddLanguage_Click(object sender, EventArgs e)
		{
			AddLanguage addLanguage = new AddLanguage(formEditor.Sources.Localization.GetAvailableLanguages());
			using (SafeThreadCallContext sc = new SafeThreadCallContext())
			{
				DialogResult res = addLanguage.ShowDialog(this);
				if (res != DialogResult.OK)
					return;

				formEditor.Sources.Localization.AddLocalizableLanguage(addLanguage.SelectedLanguage.Name);
				formEditor.UpdateController();
				formEditor.Controller?.ApplyResources();

				formEditor.SetDirty(true);

				RefreshComboLanguages();
				if (tscomboLanguages.ComboBox != null)
					tscomboLanguages.ComboBox.SelectedItem = addLanguage.SelectedLanguage;
			}
		}

		//--------------------------------------------------------------------------------
		private void tsbRemoveLanguage_Click(object sender, EventArgs e)
		{
			if (tscomboLanguages.SelectedItem == null || tscomboLanguages.SelectedItem as CultureInfo == null)
				return;

			CultureInfo ci = tscomboLanguages.SelectedItem as CultureInfo;
			DialogResult res = MessageBox.Show(
				this,
				string.Format(Resources.ConfirmDeleteLanguage, ci.DisplayName),
				Resources.DeleteLanguage,
				MessageBoxButtons.YesNo,
				MessageBoxIcon.Question
				);

			if (res != DialogResult.Yes)
				return;

			formEditor.Sources.Localization.RemoveLocalizableLanguage(ci.Name);
			formEditor.Controller?.ApplyResources();

			formEditor.SetDirty(true);

			RefreshComboLanguages();
			if (tscomboLanguages.ComboBox != null)
				tscomboLanguages.ComboBox.SelectedIndex = tscomboLanguages.ComboBox.Items.Count - 1;
		}

		//--------------------------------------------------------------------------------
		internal void OnOpenProperties(object sender, EventArgs e)
		{
			CreatePropertyEditor();
		}

		//--------------------------------------------------------------------------------
		internal void OnOpenTwinPanel(object sender, EventArgs e)
		{
			string panelItem = sender is PanelItems ? ((PanelItems)sender).ToString() :
							(sender is UserControl ? ((UserControl)sender).Name : string.Empty);

			if (panelItem == PanelItems.DatabaseExplorer.ToString() ||
				panelItem == PanelItems.HotLinksExplorer.ToString() ||
				panelItem == PanelItems.BusinessObjectsExplorer.ToString())
				CreateObjectModelWindow();
			else
				CreateViewOutline();
		}

		//--------------------------------------------------------------------------------
		internal void TsbSaveAs_Click(object sender, EventArgs e)
		{
			formEditor.Save(true);
		}

		//--------------------------------------------------------------------------------
		internal void TsbLocalization_Click(object sender, EventArgs e)
		{
			CreateLocalization();
		}

		//-------------------------------------------------------------------------------
		internal void FormEditor_RequestedDocumentModel(object sender, EventArgs e)
		{
			CreateObjectModelWindow();
		}

		//-------------------------------------------------------------------------------
		internal void FormEditor_RequestedOptions(object sender, EventArgs e)
		{
			CreateOptions();
		}

		//-----------------------------------------------------------------------------
		/// <remarks/>
		public override void ChangeFilterViewTree(bool imEditingOnlyThisTile)
		{
			if (imEditingOnlyThisTile)
			{
				viewOutlineTree.HostedControl.TsFilterLabel.Enabled = false;
				viewOutlineTree.HostedControl.SetFilter();
				viewOutlineTree.HostedControl.SelectOrFilter(formEditor.SelectionService.PrimarySelection);
				return;
			}
			viewOutlineTree.HostedControl.ResetFIlter();
			viewOutlineTree.HostedControl.TsFilterLabel.Enabled = true;
		}

		//-----------------------------------------------------------------------------
		/// <remarks/>
		public override void RefreshTemplates()
		{
			if (formEditor.TemplateService != null)
				toolboxControl.HostedControl.RefreshTemplates(formEditor.TemplateService.Templates);
		}

		//--------------------------------------------------------------------------------
		private void tscomboLanguages_SelectedIndexChanged(object sender, EventArgs e)
		{
			if (
				tscomboLanguages.SelectedItem == null ||
				tscomboLanguages.SelectedItem as CultureInfo == null ||
				Thread.CurrentThread.CurrentUICulture == null
				)
				return;

			//Il bottone rimuovi è abilitato solamente se non sto lavorando sull'invariant culture
			tsbRemoveLanguage.Enabled = ((CultureInfo)tscomboLanguages.SelectedItem).Name != CultureInfo.InvariantCulture.Name;

			if (Thread.CurrentThread.CurrentUICulture.Name == ((CultureInfo)tscomboLanguages.SelectedItem).Name)
				return;

			Thread.CurrentThread.CurrentUICulture = (CultureInfo)tscomboLanguages.SelectedItem;
		}

		//--------------------------------------------------------------------------------
		private void tsbToolbox_Click(object sender, EventArgs e)
		{
			CreateToolboxControl();
		}

		//--------------------------------------------------------------------------------
		private void tsbOpenCodeEditor_Click(object sender, EventArgs e)
		{
			RunCodeEditorForm(null, true);
		}

		//--------------------------------------------------------------------------------
		private void tsbLocalization_Click(object sender, EventArgs e)
		{
			CreateLocalization();
		}

		//--------------------------------------------------------------------------------
		private void tsbProperties_Click(object sender, EventArgs e)
		{
			CreatePropertyEditor();
		}

		//--------------------------------------------------------------------------------
		private void tsbObjectModel_Click(object sender, EventArgs e)
		{
			CreateObjectModelWindow();
		}

		//--------------------------------------------------------------------------------
		private void tsbViewOutline_Click(object sender, EventArgs e)
		{
			CreateViewOutline();
		}

		private void tsbDBExplorer_Click(object sender, EventArgs e)
		{
			CreateDatabaseExplorer();
		}

		//--------------------------------------------------------------------------------
		private void tsbHotLinks_Click(object sender, EventArgs e)
		{
			CreateHotLinksExplorer();
		}

		//--------------------------------------------------------------------------------
		private void tsbBusinessObjects_Click(object sender, EventArgs e)
		{
			CreateBusinessObjectsExplorer();
		}

		//--------------------------------------------------------------------------------
		private void tsbEnums_Click(object sender, EventArgs e)
		{
			CreateEnumsTreeControl();
		}

		//--------------------------------------------------------------------------------
		private void tsbSave_Click(object sender, EventArgs e)
		{
			formEditor.ExitFromEditing();       //voglio uscire dall'editing allo scatto di eventi della toolbar
			formEditor.Save(false);
		}

		//--------------------------------------------------------------------------------
		private void tsbSaveAs_Click(object sender, EventArgs e)
		{
			formEditor.ExitFromEditing();       //voglio uscire dall'editing allo scatto di eventi della toolbar
			formEditor.Save(true);
		}

		//--------------------------------------------------------------------------------
		private void tsbOptions_Click(object sender, EventArgs e)
		{
			CreateOptions();
		}

		//--------------------------------------------------------------------------------
		private void tsbTabOrder_Click(object sender, EventArgs e)
		{
			formEditor.EditTabOrder = tsbTabOrder.Checked;
			if (formEditor.EditTabOrder)
				toolTip.Show(Resources.TabIndexTooltip, this, new Point(tsbTabOrder.Bounds.X, tsbTabOrder.Bounds.Bottom), 5000);
		}

		//--------------------------------------------------------------------------------
		private void tsbExit_Click(object sender, EventArgs e)
		{
			ExitFromCustomization();
		}

		/// <remarks/>
		//--------------------------------------------------------------------------------
		public override void ExitFromCustomization()
		{
			formEditor.ExitFromEditing();       //voglio uscire dall'editing allo scatto di eventi della toolbar
			formEditor.EditTabOrder = false;
			tsbTabOrder.Checked = false;
			Form parentForm = FindForm();
			parentForm?.Close();
		}

		//--------------------------------------------------------------------------------
		internal void RefreshComboLanguages()
		{
			ComboBox tscomboLanguagesComboBox = tscomboLanguages.ComboBox;
			if (tscomboLanguagesComboBox == null)
				return;
			tscomboLanguagesComboBox.Items.Clear();

			tscomboLanguagesComboBox.Items.AddRange(formEditor.Sources.Localization.GetLocalizedLanguages().ToArray());

			tscomboLanguagesComboBox.Sorted = true;
			tscomboLanguagesComboBox.DisplayMember = "DisplayName";
		}
	}
}
