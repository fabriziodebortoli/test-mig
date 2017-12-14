using System;
using System.IO;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Windows.Forms;
using Microarea.EasyBuilder.ComponentModel;
using Microarea.EasyBuilder.Properties;
using Microarea.TaskBuilderNet.Core.Generic;
using Microarea.Framework.TBApplicationWrapper;
using Microarea.TaskBuilderNet.UI.WinControls.Dock;
using WeifenLuo.WinFormsUI.Docking;
using Microarea.TaskBuilderNet.Core.NameSolver;
using Microarea.TaskBuilderNet.Interfaces;
using Microarea.TaskBuilderNet.Core.SerializableTypes;
using Microarea.TaskBuilderNet.Core.EasyBuilder;
using Newtonsoft.Json.Linq;
using Microarea.TaskBuilderNet.UI.TBWebFormControl;
using System.Text;

namespace Microarea.EasyBuilder.UI
{
	//--------------------------------------------------------------------------------
	/// <remarks/>
	public partial class MainFormJsonEditor : MainForm
	{
		/* enum PanelItems in the same order as the toolbar*/
		internal TBDockContent<ToolboxControl> toolboxControl;
		internal TBDockContent<PropertyEditor> propertyEditor;
		internal TBDockContent<ViewOutlineTreeControl> viewOutlineTree;
		internal TBDockContent<JsonFormsTreeControl> jsonFormsTree;
		internal TBDockContent<JsonCodeControl> jsonCodeControl;
		internal TBDockContent<DocumentOutlineTreeControl> docOutlineTree;
		/* enum PanelItems*/

		private DockPanel hostingPanel;
		internal FormEditor formEditor;
		internal bool weAreInDocOutlineMod = false;

		internal JsonCodeControl JsonCodeControl
		{
			get { return jsonCodeControl == null ? null : jsonCodeControl.HostedControl; }
		}
		List<string> elemForDocOutline = new List<string>() {
				WndObjDescription.WndObjType.Frame.ToString(),              ((int)WndObjDescription.WndObjType.Frame).ToString(),
				WndObjDescription.WndObjType.View.ToString(),               ((int)WndObjDescription.WndObjType.View).ToString(),
				WndObjDescription.WndObjType.Toolbar.ToString(),            ((int)WndObjDescription.WndObjType.Toolbar).ToString(),
				WndObjDescription.WndObjType.TileManager.ToString(),        ((int)WndObjDescription.WndObjType.TileManager).ToString(),
				WndObjDescription.WndObjType.TileGroup.ToString(),          ((int)WndObjDescription.WndObjType.TileGroup).ToString(),
				WndObjDescription.WndObjType.LayoutContainer.ToString(),    ((int)WndObjDescription.WndObjType.LayoutContainer).ToString()
			};
		/// <remarks/>
		public IWin32Window OwnerWindow { get { return this; } }

		/// <remarks/>
		public string FileLocation { get; private set; }

		/// <remarks/>
		//--------------------------------------------------------------------------------
		public MainFormJsonEditor(DockPanel hostingPanel, FormEditor formEditor)
		{
			InitializeComponent();

			CleanRecents();

			this.formEditor = formEditor;
			this.hostingPanel = hostingPanel;
			this.Text = Resources.MainFormTitle;

			EditorImages.ViewModelImages = ImageLists.ViewModelImages;

			//Riduco la dimensione dei docking
			this.hostingPanel.DockLeftPortion = 0.12F;
			this.hostingPanel.DockRightPortion = 0.20F;
			this.hostingPanel.DockTopPortion = 0.06F;
			this.hostingPanel.DockTopPanelMinSize = 50;

			formEditor.RequestedSave += new EventHandler(FormEditor_RequestedSave);
			formEditor.DirtyChanged += new EventHandler<DirtyChangedEventArgs>(FormEditor_DirtyChanged);
			formEditor.CreateControllerCompleted += FormEditor_CreateControllerCompleted;
			formEditor.SelectedObjectUpdated += new EventHandler(FormEditor_SelectedObjectUpdated);

			Site = new TBSite(this, formEditor, formEditor, "MainForm");

			CreateJsonCodeWindow(); //creare per prima, così la mette fra quella di destra e quella di sinistra
			CreateToolboxControl();
			CreatePropertyEditor();
			CreateViewOutline();
			CreateJsonFormsWindow();

			tsbSave.Enabled = false;
		}

		/// <remarks/>

		/// <remarks/>
		//-------------------------------------------------------------------------------
		public override void SelectFileInTree(string file)
		{
			jsonFormsTree.HostedControl.SelectJsonForm(file, null);
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
				SafeDispose(ref propertyEditor);
				SafeDispose(ref toolboxControl);
				SafeDispose(ref jsonFormsTree);
				SafeDispose(ref viewOutlineTree);
				SafeDispose(ref docOutlineTree);

				if (formEditor != null)
					formEditor.DirtyChanged -= new EventHandler<DirtyChangedEventArgs>(FormEditor_DirtyChanged);
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
		internal void FormEditor_RequestedSave(object sender, EventArgs e)
		{
			formEditor.Save(false);
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

		/// <remarks/>
		//-----------------------------------------------------------------------------
		public override void CloseCodeEditorsThreadContext()
		{
			if (jsonCodeControl != null && !jsonCodeControl.IsDisposed)
			{
				jsonCodeControl.Dispose();
				jsonCodeControl = null;
			}
		}

		//--------------------------------------------------------------------------------
		/// <remarks/>
		public override void UpdateObjectViewModel(IComponent component)
		{
			if (
				viewOutlineTree != null && !viewOutlineTree.IsDisposed &&
				viewOutlineTree.HostedControl != null && !viewOutlineTree.HostedControl.IsDisposed
				)
				viewOutlineTree.HostedControl.UpdateViewOutline(component);
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

		//--------------------------------------------------------------------------------
		/// <remarks/>
		protected override void OnHelpRequested(HelpEventArgs hevent)
		{
			base.OnHelpRequested(hevent);
			FormEditor.ShowHelp(GetType());
			hevent.Handled = true;
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

			toolboxControl.FormClosed += (FormClosedEventHandler)delegate
			{ toolboxControl = null; };
			RefreshTemplates();
		}


		//-----------------------------------------------------------------------------
		internal void CreateDocOutlineTree()
		{
			if (docOutlineTree != null)
			{
				docOutlineTree.Activate();
				return;
			}
			docOutlineTree = TBDockContent<DocumentOutlineTreeControl>.CreateDockablePane(
						 hostingPanel,
						 WeifenLuo.WinFormsUI.Docking.DockState.DockLeft,
						 Icon.FromHandle(Resources.Toolbox.GetHicon()),
						 formEditor
						 );

			docOutlineTree.HostedControl.Site = new TBSite(null, null, formEditor, "DocOutlineTree");
			docOutlineTree.HostedControl.JsonFormOpenSelected += JsonFormOpenSelected;
			docOutlineTree.FormClosed += (FormClosedEventHandler)delegate { docOutlineTree = null; };
			docOutlineTree.HostedControl.OpenProperties += new EventHandler(OnOpenProperties);
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
			viewOutlineTree.HostedControl.PromoteControl += new EventHandler(OnPromoteControl);
			viewOutlineTree.HostedControl.OpenProperties += new EventHandler(OnOpenProperties);
		}

		//-----------------------------------------------------------------------------
		private void OnPromoteControl(object sender, EventArgs e)
		{
			formEditor.PromoteControls();
		}

		//--------------------------------------------------------------------------------
		internal void CreateJsonFormsWindow()
		{
			if (jsonFormsTree != null)
			{
				jsonFormsTree.Activate();
				return;
			}
			jsonFormsTree = TBDockContent<JsonFormsTreeControl>.CreateDockablePane(
						 hostingPanel,
						 WeifenLuo.WinFormsUI.Docking.DockState.DockRight,
						 Icon.FromHandle(Resources.DocumentViewModel.GetHicon()),
						 formEditor
						 );
			jsonFormsTree.HostedControl.JsonItemAdd += JsonItemAdd;
			jsonFormsTree.HostedControl.JsonItemRename += JsonItemRename;
			jsonFormsTree.HostedControl.JsonFormOpenSelected += JsonFormOpenSelected;
			jsonFormsTree.HostedControl.JsonFormDeleted += JsonFormDeleted;
			jsonFormsTree.FormClosed += (FormClosedEventHandler)delegate
			{
				jsonFormsTree.HostedControl.JsonItemAdd -= JsonItemAdd;
				jsonFormsTree.HostedControl.JsonItemRename -= JsonItemRename;
				jsonFormsTree.HostedControl.JsonFormOpenSelected -= JsonFormOpenSelected;
				jsonFormsTree.HostedControl.JsonFormDeleted -= JsonFormDeleted;
				jsonFormsTree = null;
			};
			JsonSelectLastForm(); //riseleziona l'ultima form aperta nell'ultima sessione
		}

		//--------------------------------------------------------------------------------
		internal void CreateJsonCodeWindow()
		{
			if (jsonCodeControl != null)
			{
				jsonCodeControl.Activate();
				return;
			}
			jsonCodeControl = TBDockContent<JsonCodeControl>.CreateDockablePane(
						 hostingPanel,
						 WeifenLuo.WinFormsUI.Docking.DockState.DockBottom,
						 Icon.FromHandle(Resources.DocumentViewModel.GetHicon())
						 );
			if (formEditor.jsonEditorConnector != null)
			{
				formEditor.jsonEditorConnector.AttachCodeEditor(jsonCodeControl.HostedControl);
			}
			jsonCodeControl.HostedControl.CodeChanged += JsonCodeControl_CodeChanged;
			jsonCodeControl.FormClosed += (FormClosedEventHandler)delegate
			{
				jsonCodeControl.HostedControl.CodeChanged -= JsonCodeControl_CodeChanged;
				jsonCodeControl = null;
				if (formEditor.jsonEditorConnector != null)
					formEditor.jsonEditorConnector.AttachCodeEditor(null);
			};

		}

		//-----------------------------------------------------------------------------
		internal override string SaveDocOutlineSerialized()
		{
			if (weAreInDocOutlineMod)
				return docOutlineTree?.HostedControl?.SaveDocOutline();
			return null;
		}

		//-----------------------------------------------------------------------------
		void JsonCodeControl_CodeChanged(object sender, EventArgs e)
		{
			formEditor.RefreshWrappers(true);
			if (weAreInDocOutlineMod)
				docOutlineTree?.HostedControl?.DeserializeJsonByCode(JsonCodeControl.Code);
		}

		//-----------------------------------------------------------------------------
		private void JsonItemAdd(object sender, JsonNodeSelectedEventArgs e)
		{
			AddItem(e.NameSpace, e.Folder);
		}

		//-----------------------------------------------------------------------------
		internal override string AddItem(INameSpace ns, string folder, bool isFromDocOutline = false)
		{
			string path = string.IsNullOrEmpty(folder) ? BasePathFinder.BasePathFinderInstance.GetJsonFormPath(ns) : folder;
			if (string.IsNullOrEmpty(path))
				return String.Empty;

			string newpath = String.Empty;
			bool canBeOpen = false;

			using (SafeThreadCallContext context = new SafeThreadCallContext())
			{
				AddItem dlg = new AddItem(path, isFromDocOutline);
				if (dlg.ShowDialog(this) != DialogResult.OK)
					return string.Empty;

				newpath = dlg.JsonFile;
				switch (dlg.FormType)
				{
					case UI.AddItem.ItemType.Folder:
						string parentPath = Path.GetDirectoryName(path);
						string thisModuleName = ns.Module.ToString();
						if (!parentPath.EndsWith(thisModuleName))
						{
							MessageBox.Show(String.Format(Resources.InvalidChoice, Resources.NestingNotPermitted));
							return string.Empty;       
						}
											
						Directory.CreateDirectory(Path.Combine(path, dlg.JsonFile));
						break;
					case UI.AddItem.ItemType.Panel:
					case UI.AddItem.ItemType.Tile_Mini:
					case UI.AddItem.ItemType.Tile_Standard:
					case UI.AddItem.ItemType.Tile_Wide:
					case UI.AddItem.ItemType.Generic_Element:
						if (string.IsNullOrEmpty(folder) && ns.NameSpaceType.Type != NameSpaceObjectType.Document)           //non esiste cartella corrispon
						{
							string newFolderName = Path.GetFileNameWithoutExtension(dlg.JsonFile).Replace("IDD_", "UI");
							Directory.CreateDirectory(Path.Combine(path, newFolderName));
							newpath = Path.Combine(path, newFolderName, Path.GetFileName(dlg.JsonFile));
						}
						formEditor.CreateJsonForm(newpath, dlg.FormType, isFromDocOutline);
						canBeOpen = true;
						break;
					case UI.AddItem.ItemType.View:
					case UI.AddItem.ItemType.Frame:
					case UI.AddItem.ItemType.Href:
						formEditor.CreateJsonForm(newpath, dlg.FormType, true, dlg.PathHref);
						formEditor.CreateHJson(newpath);
						weAreInDocOutlineMod = false;
						canBeOpen = true;
						break;
					default:
						break;
				}

				if (jsonFormsTree != null)
				{
					jsonFormsTree.HostedControl.PopulateTree();
					jsonFormsTree.HostedControl.SelectJsonForm(newpath, ns);
				}

				if (canBeOpen) // se sono nel DocOutl, apre tutto in new window
					JsonFormOpenSelected(null, new JsonFormSelectedEventArgs(newpath, ns, weAreInDocOutlineMod));

				return newpath;
			}
		}
		
		//-----------------------------------------------------------------------------
		private void JsonItemRename(object sender, JsonFormSelectedEventArgs e)
		{
			RenameItem(e);
		}

		//-----------------------------------------------------------------------------
		private void RenameItem(JsonFormSelectedEventArgs e)
		{
			string fromTbJson = e.JsonFile;
			string fromHJson = fromTbJson.Replace(NameSolverStrings.TbjsonExtension, NameSolverStrings.HjsonExtension);
			if (formEditor.CurrentJsonFile == fromTbJson)
			{
				MessageBox.Show(this, Resources.CannotRenameFileOpen, Resources.EasyBuilder, MessageBoxButtons.OK, MessageBoxIcon.Error);
				return;
			}

			if (string.IsNullOrEmpty(fromTbJson))
				return;

			using (SafeThreadCallContext context = new SafeThreadCallContext())
			{
				RenameItem dlg = new RenameItem(fromTbJson);
				if (dlg.ShowDialog(this) != DialogResult.OK)
					return;

				string toTbJson = Path.Combine(dlg.NewFileLocat, dlg.NewFileName);
				string toHJson = toTbJson.Replace(NameSolverStrings.TbjsonExtension, NameSolverStrings.HjsonExtension);

				if (fromTbJson.Equals(toTbJson))
					return;
				if ((new FileInfo(fromTbJson)).IsReadOnly || (new FileInfo(fromHJson)).IsReadOnly)
				{
					MessageBox.Show(this, Resources.FileReadOnly, Resources.EasyBuilder, MessageBoxButtons.OK, MessageBoxIcon.Error);
					return;
				}
				try
				{
					if (!Path.HasExtension(fromTbJson))
					{
						if (!Path.HasExtension(toTbJson))
						{
							Directory.Move(fromTbJson, toTbJson);
							return;
						}
						else
							MessageBox.Show(this, Resources.CommandNotAllowed, Resources.EasyBuilder, MessageBoxButtons.OK, MessageBoxIcon.Error);
					}
					File.Move(fromTbJson, toTbJson);
					string text = File.ReadAllText(toTbJson);
					JsonProcessor.ChangeIdName(ref text, Path.GetFileNameWithoutExtension(toTbJson));
					File.WriteAllText(toTbJson, text);

					if (File.Exists(fromHJson))
					{
						File.Move(fromHJson, toHJson);
						/*IN CASO DI CHANGE DIRECTORY bisogna modificare il context all'interno del 
						string contextFrom = CUtility.GetJsonContext(fromHJson);
						string contextTo = CUtility.GetJsonContext(toHJson);
						if (!contextFrom.Equals(contextTo))		{ text.Replace(contextFrom, contextTo); } */
						text = File.ReadAllText(toHJson);
						text = text.Replace(Path.GetFileNameWithoutExtension(fromHJson), Path.GetFileNameWithoutExtension(toHJson));
						File.WriteAllText(toHJson, text);
					}

				}
				catch (DirectoryNotFoundException)
				{
					MessageBox.Show(this, Resources.NotFound, Resources.EasyBuilder, MessageBoxButtons.OK, MessageBoxIcon.Error);
				}
				catch (FileNotFoundException)
				{
					MessageBox.Show(this, Resources.NotFound, Resources.EasyBuilder, MessageBoxButtons.OK, MessageBoxIcon.Error);
				}
				catch (IOException)
				{
					MessageBox.Show(this, Resources.FormAlreadyExisting, Resources.EasyBuilder, MessageBoxButtons.OK, MessageBoxIcon.Error);
				}
				RemoveFromRecents(fromTbJson);
				AddToRecent(toTbJson);
				jsonFormsTree.HostedControl.PopulateTree();//as refresh
			}
		}

		//-----------------------------------------------------------------------------
		private void JsonFormDeleted(object sender, JsonFormSelectedEventArgs e)
		{
			if (!formEditor.DeleteJsonForm(e.JsonFile))
				return;
			RemoveFromRecents(e.JsonFile);
			if (jsonFormsTree != null)
				jsonFormsTree.HostedControl.PopulateTree();
		}

		//-----------------------------------------------------------------------------
		void JsonFormOpenSelected(object sender, JsonFormSelectedEventArgs e)
		{
			if (e == null || e.JsonFile == null)
				return;
			AddToRecent(e.JsonFile);

			if (!String.IsNullOrEmpty(formEditor.CurrentJsonFile) && e.OpenInNewWindow)
				CUtility.RunDocument("Document.Extensions.EasyBuilder.TbEasyBuilder.EasyStudioDesigner", e.JsonFile);
			else
				formEditor.OpenJsonForm(e.JsonFile);
		}

		//-----------------------------------------------------------------------------
		internal override bool OpenDocOutlineIfNeeded(JsonFormSelectedEventArgs e)
		{
			FileLocation = e.JsonFile;
			if (!CheckHasView(e.JsonFile))
			{
				CloseDocOutline();
				return false;
			}

			CreateDocOutlineTree();
			docOutlineTree.HostedControl.DeserializeJson(e.JsonFile);
			HideComponents(true);
			return true;
		}

		//-----------------------------------------------------------------------------
		private void HideComponents(bool hideOrShow)
		{
			if (hideOrShow)
			{
				viewOutlineTree?.Close();
				toolboxControl?.Close();
			}
			else
			{
				CreateViewOutline();
				CreateToolboxControl();
			}

			tsbViewOutline.Enabled =
				tsbToolbox.Enabled =
				formEditor.Enabled = !hideOrShow;
		}

		//-----------------------------------------------------------------------------
		internal override void UpdateDocOutline(string code)
		{
			if (string.IsNullOrEmpty(code))
				//è stata cambiata una proprietà nella propertygrid, quindi aggiorna code editor e tree
				docOutlineTree?.HostedControl?.UpdateCodeEditor(true);
			else if (!weAreInDocOutlineMod)
				return;
			//setta la stringa JsonDeserialized con il codice passato + aggiorna code editor e tree
			else docOutlineTree.HostedControl.DeserializeJsonByCode(code);
		}

		//-----------------------------------------------------------------------------
		private bool CheckHasView(string fileJsonPath, string valueProp = "1", string name = "type")
		{
			string json = System.IO.File.ReadAllText(fileJsonPath);
			JObject obj = null;
			try
			{
				obj = JObject.Parse(json);
				bool typeOrHref = GetTypeorHref(obj);
				weAreInDocOutlineMod = typeOrHref;
			}
			catch (Exception) { return false; }

			return weAreInDocOutlineMod;
		}


		//-----------------------------------------------------------------------------
		internal bool GetTypeorHref(JToken containerToken)
		{


			foreach (JProperty child in containerToken.Children<JProperty>())
			{
				if (child.Name == "type" && elemForDocOutline.Contains(child.Value.ToString()))
				{
					return true;
				}
			}
			foreach (JProperty child in containerToken.Children<JProperty>())
			{
				if (child.Name == "href")
				{
					string path = CalculateId(child.Value.ToString());
					return CheckHasView(path);
				}
			}
			return false;
		}


		//-----------------------------------------------------------------------------
		internal string CalculateId(string item)
		{
			string jsonFileId = item;
			if (jsonFileId.StartsWith("M.") || jsonFileId.StartsWith("D."))
				return StaticFunctions.GetFileFromJsonFileId(jsonFileId);
			string jsonFile = Path.GetDirectoryName(FileLocation);
			return Path.Combine(jsonFile, jsonFileId + NameSolverStrings.TbjsonExtension);
		}

		/*	//-----------------------------------------------------------------------------
			internal List<JToken> FindTokens(JToken containerToken, string valueProp, string name)
			{
				List<JToken> matches = new List<JToken>();
				if (containerToken.Type == JTokenType.Object)
				{
					foreach (JProperty child in containerToken.Children<JProperty>())
					{
						if (child.Name == name && child.Value.ToString() == valueProp)
						{
							matches.Add(child.Value);
							return matches;
						}
						if (child.Name == "href")
						{
							MessageBox.Show("ispezionare href");
							matches.Add(child.Value);
							return matches;
						}

						FindTokens(child.Value, name, valueProp);
					}
				}
				else if (containerToken.Type == JTokenType.Array)
				{
					foreach (JToken child in containerToken.Children())
					{
						FindTokens(child, name, valueProp);
					}
				}
				return matches;
			}

			//-----------------------------------------------------------------------------
			internal bool CheckTokens(JToken containerToken, string valueProp, string name)
			{
				return FindTokens(containerToken, valueProp, name).Count > 0;
			}*/

		//-----------------------------------------------------------------------------
		internal void HostedControl_DeclareComponent(object sender, DeclareComponentEventArgs e)
		{
			formEditor.DeclareComponent(e);
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
			UpdateObjectViewModel(formEditor.Controller);
		}

		//-----------------------------------------------------------------------------
		internal void HostedControl_RefreshModel(object sender, EventArgs e)
		{
			formEditor.RefreshObjectModel();
		}

		//-----------------------------------------------------------------------------
		internal void HostedControl_DeleteObject(object sender, DeleteObjectEventArgs e)
		{
			formEditor.TryDeleteEasyBuilderComponents(new List<EasyBuilderComponent>() { e.Component });
		}

		//--------------------------------------------------------------------------------
		internal void OnOpenProperties(object sender, EventArgs e)
		{
			CreatePropertyEditor();
		}

		//--------------------------------------------------------------------------------
		internal void TsbSave_Click(object sender, EventArgs e)
		{
			formEditor.Save(false);
		}

		//--------------------------------------------------------------------------------
		internal void TsbSaveAs_Click(object sender, EventArgs e)
		{
			formEditor.Save(true);
		}

		//--------------------------------------------------------------------------------
		internal void TsbExit_Click(object sender, EventArgs e)
		{
			Form parentForm = FindForm();
			if (parentForm != null)
				parentForm.Close();
		}


		/*	//--------------------------------------------------------------------------------
			private void LoadRecents()
			{
				ApplicationCache cache;
				ListRecentForms recents = GetListRecents(out cache);
				if (recents == null)
					return;
				LoadRecents(recents);
			}*/

		//--------------------------------------------------------------------------------
		private void LoadRecents(ListRecentForms recents)
		{
			if (recents == null)
				return;
			tscbRecent.Items.Clear();
			List<RecentForm> forms = new List<RecentForm>();
			foreach (RecentForm item in recents)
				forms.Add(item);
			forms.Reverse();
			tscbRecent.Items.AddRange(forms.ToArray());
		}

		//-----------------------------------------------------------------------------
		private void AddToRecent(string jsonFile)
		{
			ApplicationCache cache;
			ListRecentForms listOfRecents = GetListRecents(out cache);
			RecentForm form = GetRecentFromPath(jsonFile);
			if (form == null)
			{
				form = new RecentForm(jsonFile);
				if (listOfRecents.Count >= 10)
					listOfRecents.RemoveAt(0);
				listOfRecents.Add(form);
				LoadRecents(listOfRecents);
			}
			else
				BringToHead(form);
			cache.Save();
		}

		//--------------------------------------------------------------------------------
		internal RecentForm GetRecentFromPath(string form)
		{
			ApplicationCache cache;
			ListRecentForms recents = GetListRecents(out cache);
			foreach (RecentForm item in recents)
			{
				if (form.Equals(item.FormFullPath))
					return item;
			}
			return null;
		}

		//--------------------------------------------------------------------------------
		private void BringToHead(RecentForm form)
		{
			ApplicationCache cache;
			ListRecentForms listOfRecents = GetListRecents(out cache);
			if (listOfRecents == null)
				return;
			foreach (RecentForm item in listOfRecents)
			{
				if (form.FormFullPath.Equals(item.FormFullPath))
				{
					bool b = listOfRecents.Remove(form);
					listOfRecents.Add(form);
					break;
				}

			}
			LoadRecents(listOfRecents);
			cache.Save();

		}

		//--------------------------------------------------------------------------------
		internal void CleanRecents()
		{
			ApplicationCache cache;
			ListRecentForms listOfRecents = GetListRecents(out cache);
			ListRecentForms copy = new ListRecentForms();
			foreach (RecentForm item in listOfRecents)
				if (!File.Exists(item.FormFullPath))
					copy.Add(item);
			foreach (var item in copy)
				listOfRecents.Remove(item);
			LoadRecents(listOfRecents);
			cache.Save();
		}

		//--------------------------------------------------------------------------------
		internal void RemoveFromRecents(string jsonFile)
		{
			ApplicationCache cache;
			ListRecentForms listOfRecents = GetListRecents(out cache);
			RecentForm form = GetRecentFromPath(jsonFile);
			if (form == null)//non è stata trovata nella lista
				return;
			listOfRecents.Remove(form);
			LoadRecents(listOfRecents);
			cache.Save();
		}

		//--------------------------------------------------------------------------------
		private void SelectFormFromRecents(RecentForm rf)
		{
			if (rf == null)//non è stata trovata nella lista
				return;
			BringToHead(rf);
			JsonFormSelectedEventArgs args = new JsonFormSelectedEventArgs(rf.FormFullPath, null);
			args.OpenInNewWindow = true;
			JsonFormOpenSelected(null, args);
		}

		//-----------------------------------------------------------------------------
		private ListRecentForms GetListRecents(out ApplicationCache cache)
		{
			cache = ApplicationCache.Load();
			ListRecentForms listOf = cache.GetObject<ListRecentForms>();

			if (listOf == null)
			{
				listOf = new ListRecentForms();
				cache.PutObject<ListRecentForms>(listOf);
			}
			return listOf;
		}

		//-----------------------------------------------------------------------------
		private void JsonSelectLastForm()
		{
			ApplicationCache cache;
			ListRecentForms listOfRecents = GetListRecents(out cache);
			if (listOfRecents == null)
				return;
			if (listOfRecents.Count > 0)
			{
				String jsonFile = listOfRecents[(listOfRecents.Count - 1)].FormFullPath;
				jsonFormsTree.HostedControl.SelectJsonForm(jsonFile, null);
			}
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

		//-----------------------------------------------------------------------------
		private void toolStrip_ItemClicked(object sender, ToolStripItemClickedEventArgs e)
		{

			object o = e.ClickedItem.Tag;
			if (o != null && (o is Selections.Action))
				formEditor.PerformAlignAction((Selections.Action)o);
		}

		//-----------------------------------------------------------------------------
		private void tsbSave_Click(object sender, EventArgs e)
		{
			formEditor.Save(false);
		}

		//-----------------------------------------------------------------------------
		private void tsbToolbox_Click(object sender, EventArgs e)
		{
			CreateToolboxControl();
		}

		//-----------------------------------------------------------------------------
		private void tsbProperties_Click(object sender, EventArgs e)
		{
			CreatePropertyEditor();
		}

		//-----------------------------------------------------------------------------
		private void tsbViewOutline_Click(object sender, EventArgs e)
		{
			CreateViewOutline();
		}

		//-----------------------------------------------------------------------------
		private void tsbFormsExplorer_Click(object sender, EventArgs e)
		{
			CreateJsonFormsWindow();
		}

		//-----------------------------------------------------------------------------
		private void tsbUndo_Click(object sender, EventArgs e)
		{
			formEditor.Undo();
		}

		//-----------------------------------------------------------------------------
		private void tsbRedo_Click(object sender, EventArgs e)
		{
			formEditor.Redo();
		}

		//-----------------------------------------------------------------------------
		private void tscbRecent_SelectedIndexChanged(object sender, EventArgs e)
		{
			RecentForm rf = tscbRecent.SelectedItem as RecentForm;

			SelectFormFromRecents(rf);
		}

		//-----------------------------------------------------------------------------
		private void tsbCloseCurrentDialog_Click(object sender, EventArgs e)
		{
			formEditor.EditTabOrder = false;
			tsbTabOrder.Checked = false;
			if (!formEditor.CloseJsonForm())
				return;
			CloseDocOutline();
		}

		//-----------------------------------------------------------------------------
		private void CloseDocOutline()
		{
			if (docOutlineTree != null)
			{
				docOutlineTree.Close();
				docOutlineTree = null;
				HideComponents(false); //False To Show
			}
			jsonFormsTree.Activate();
		}

		//-----------------------------------------------------------------------------
		private void tsbRefreshRecents_Click(object sender, EventArgs e)
		{
			CleanRecents();
		}

		//-----------------------------------------------------------------------------
		private void tsbTabOrder_Click(object sender, EventArgs e)
		{
			formEditor.EditTabOrder = tsbTabOrder.Checked;
			if (formEditor.EditTabOrder)
				toolTip.Show(Resources.TabIndexTooltip, this, new Point(tsbTabOrder.Bounds.X, tsbTabOrder.Bounds.Bottom), 5000);
		}

		//-----------------------------------------------------------------------------
		private void tsbOpenJsonEditor_Click(object sender, EventArgs e)
		{
			CreateJsonCodeWindow();
		}

	}
}
