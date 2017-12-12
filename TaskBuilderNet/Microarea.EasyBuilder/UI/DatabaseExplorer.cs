using System;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using System.ComponentModel.Design;
using System.Diagnostics;
using System.IO;
using System.Windows.Forms;
using Microarea.EasyBuilder.ComponentModel;
using Microarea.EasyBuilder.DBScript;
using Microarea.EasyBuilder.Packager;
using Microarea.EasyBuilder.Properties;
using Microarea.Framework.TBApplicationWrapper;
using Microarea.TaskBuilderNet.Core.CoreTypes;
using Microarea.TaskBuilderNet.Core.DiagnosticManager;
using Microarea.TaskBuilderNet.Core.DiagnosticUI;
using Microarea.TaskBuilderNet.Core.Generic;
using Microarea.TaskBuilderNet.Core.NameSolver;
using Microarea.TaskBuilderNet.Core.WebServicesWrapper;
using Microarea.TaskBuilderNet.Data.DatabaseLayer;
using Microarea.TaskBuilderNet.Interfaces.Model;
using System.Threading.Tasks;
using Microarea.TaskBuilderNet.Core.EasyBuilder;
using Microarea.TaskBuilderNet.Interfaces;

namespace Microarea.EasyBuilder.UI
{
	//================================================================================
	/// <remarks/>
	public partial class DatabaseExplorer : UserControl
	{
		/// <remarks/>
		public event EventHandler OpenProperties;
		/// <remarks/>
		public event EventHandler OpenTwinPanel;	
		/// <remarks/>
		public event EventHandler CatalogChanged;

		private MSqlCatalog catalog = new MSqlCatalog();
        private Editor editor;
		private TreeNode dragNode = null;
		private TreeNode dbNode;
		private ISelectionService selectionService;
		private LoginManager loginManager;
		private bool isDirty = false;
		private bool loadingTree = false;
		
		//--------------------------------------------------------------------------------
		/// <remarks/>
		public LoginManager LoginManager
		{
			get
			{
				if (loginManager == null)
					loginManager = new LoginManager();
				return loginManager;
			}
		}
		//--------------------------------------------------------------------------------
		/// <remarks/>
		public bool IsDirty
		{
			get { return isDirty; }
			set { isDirty = value; }
		}

		//--------------------------------------------------------------------------------
		/// <remarks/>
        public DatabaseExplorer(Editor editor)
		{
			InitializeComponent();

            tvTables.Scrollable = true;

			this.Text = Resources.DatabaseExplorerTitle;

			tvTables.ImageList = tvTables.StateImageList = ImageLists.DatabaseExplorerTree;
			this.editor = editor;
			selectionService = editor.GetService(typeof(ISelectionService)) as ISelectionService;
			this.Site = new TBSite(this, null, editor, Name);
			IsDirty = false;

			LoadTree(null);
		}

		/// <summary> 
		/// Clean up any resources being used.
		/// </summary>
		/// <param name="disposing">true if managed resources should be disposed; otherwise, false.</param>
		//--------------------------------------------------------------------------------
		protected override void Dispose(bool disposing)
		{
			if (disposing)
			{

				if (components != null)
					components.Dispose();

				EventHandlers.RemoveEventHandlers(ref OpenProperties);
				EventHandlers.RemoveEventHandlers(ref CatalogChanged);

				tvTables.Nodes.Clear(); //svuoto il tree per problemi di perfomance
				tvTables.Dispose();
                try
                {
                    SaveToDBInfo(GetDatabaseChanges().GetCurrentReleaseChanges(BaseCustomizationContext.CustomizationContextInstance.CurrentEasyBuilderApp));
				}
				catch { }
			}
			base.Dispose(disposing);
		}


		/// <remarks/>
		//--------------------------------------------------------------------------------
		protected override void OnHelpRequested(HelpEventArgs hevent)
		{
			base.OnHelpRequested(hevent);
			FormEditor.ShowHelp();
			hevent.Handled = true;
		}

		//--------------------------------------------------------------------------------
		private void LoadTree(bool? onlyMyItems)
		{
			Cursor = Cursors.WaitCursor;
			//inibisce il check changed event handler
			loadingTree = true;
			tvTables.BeginUpdate();
			try
			{
				tvTables.Nodes.Clear();
				dbNode = new TreeNode(Resources.DatabaseNode, (int)ImageLists.DatabaseExplorerImageIndex.Database, (int)ImageLists.DatabaseExplorerImageIndex.Database);
				tvTables.Nodes.Add(dbNode);

				int databaseRelease = DatabaseChanges.GetDatabaseRelease(BaseCustomizationContext.CustomizationContextInstance.CurrentEasyBuilderApp);

				foreach (IRecord record in catalog.Tables)
				{
					if (record.RecordType != DataModelEntityType.Table)
						continue;
					//se nel catalog ho una tabella non ancora persistita su database, 
					//allora devo inserire un AddedRecord e non un MSqlRecord, così lo posso modificare
					if (IsCustomTable(record) && record.CreationRelease == databaseRelease)
					{
						AddedRecord newTable = new AddedRecord(record, databaseRelease, false);
						newTable.PropertyChanging += new EventHandler<MyPropertyChangingArgs>(table_PropertyChanging);
						newTable.PropertyChanged += new PropertyChangedEventHandler(table_PropertyChanged);
						AddTable(dbNode.Nodes, newTable);
					}
					else
					{
						AddTable(dbNode.Nodes, record);
					}
				}
				DBScriptInfo info = DBScriptInfo.Load();
				if (info != null && info.DatabaseRelease > BaseCustomizationContext.CustomizationContextInstance.CurrentEasyBuilderApp.ModuleInfo.CurrentDBRelease)
					LoadFromDbInfo(info);


				if (onlyMyItems.HasValue)
					tbFilter.Checked = onlyMyItems.Value;

				if (tbFilter.Checked)
					FilterNodes();
				tvTables.TreeViewNodeSorter = new TreeNodeComparer();
				tvTables.Sort();

			}
			finally
			{
				Cursor = Cursors.Default;
				loadingTree = false;
                try
                {
                    tvTables.EndUpdate();

                    if (!dbNode.IsExpanded)
                    {
                        dbNode.Expand();
                    }
                }
                catch
                {}
			}
		}

		//--------------------------------------------------------------------------------
		private void FilterNodes()
		{
			for (int i = dbNode.Nodes.Count - 1; i >= 0; i--)
			{
				TreeNode node = dbNode.Nodes[i];
				if (IsCustomTable((IRecord)node.Tag))
					continue;

				bool customField = false;
				foreach (TreeNode nodeField in node.Nodes)
					if (IsCustomField((IRecordField)nodeField.Tag))
					{
						customField = true;
						break;
					}
				if (customField)
					continue;
				node.Remove();
			}
		}


		//--------------------------------------------------------------------------------
		private void SaveToDBInfo(DatabaseChangesCurrentRelease releaseChenges)
		{
			DBScriptInfo info = new DBScriptInfo();
			foreach (AddedRecord record in releaseChenges.AddedTables)
				info.NewTables.Add(record);

			foreach (AddedField field in releaseChenges.AddedFields)
				info.Add((NameSpace)field.Record.NameSpace, field);

			info.OnlyMyItems = tbFilter.Checked;
			info.DatabaseRelease = DatabaseChanges.GetDatabaseRelease(BaseCustomizationContext.CustomizationContextInstance.CurrentEasyBuilderApp);
			info.Save();
		}

		//--------------------------------------------------------------------------------
		private TreeNode AddTable(TreeNodeCollection nodes, IRecord record)
		{
			TreeNode tableNode = CreateTableNode(record);
			nodes.Add(tableNode);
			if (record is AddedRecord)
				((AddedRecord)record).TreeNode = tableNode;

			AddFields(tableNode.Nodes, record);
			return tableNode;
		}

		//--------------------------------------------------------------------------------
		private TreeNode CreateTableNode(IRecord record)
		{
			TreeNode tableNode = new TreeNode(record.Name, (int)ImageLists.DatabaseExplorerImageIndex.Table, (int)ImageLists.DatabaseExplorerImageIndex.Table);
			tableNode.Tag = record;
			tableNode.ContextMenuStrip = contextMenu;
			tableNode.ToolTipText = record.Name;

			if (record is AddedRecord && !((AddedRecord)record).InCatalog)
			{
				tableNode.StateImageIndex = (int)ImageLists.DatabaseExplorerImageIndex.New;
				tableNode.ToolTipText = tableNode.ToolTipText + " " + Resources.CustomTableNotYetAvailableInObjectModel;
			}
			else if (IsCustomTable((IRecord)tableNode.Tag))
			{
				tableNode.StateImageIndex = (int)ImageLists.DatabaseExplorerImageIndex.EBNode;
				tableNode.ToolTipText = tableNode.ToolTipText + " " + Resources.CustomTableAvailableInObjectModel;
			}
			else if (!((IRecord)tableNode.Tag).IsRegistered)
			{
				tableNode.StateImageIndex = (int)ImageLists.DatabaseExplorerImageIndex.UnregisteredTable;
				tableNode.ToolTipText = tableNode.ToolTipText + " " + Resources.NonRegisteredTable;
			}
			return tableNode;
		}

		//--------------------------------------------------------------------------------
		private void AddFields(TreeNodeCollection nodes, IRecord record)
		{
			foreach (IRecordField field in record.Fields)
			{
				//se nel catalog ho un campo non ancora persistito su database, 
				//alloradevo inserire un AddedField e non un MSqlRecordField, così lo posso modificare
				TreeNode fieldNode = null;
				if (IsCustomField(field) &&
					field.CreationRelease == DatabaseChanges.GetDatabaseRelease(BaseCustomizationContext.CustomizationContextInstance.CurrentEasyBuilderApp))
				{
					AddedField newField = field as AddedField; //forse ho già un AddedField?
					if (newField == null)
						newField = new AddedField(record, field);

					newField.PropertyChanged += new PropertyChangedEventHandler(field_PropertyChanged);
					newField.PropertyChanging += new EventHandler<MyPropertyChangingArgs>(field_PropertyChanging);

					fieldNode = CreateFieldNode(newField);
					newField.TreeNode = fieldNode;
				}
				else
					fieldNode = CreateFieldNode(field);
				nodes.Add(fieldNode);
			}
		}

		//--------------------------------------------------------------------------------
		private TreeNode CreateFieldNode(IRecordField field)
		{
			int imageIndex = GetFieldNodeImageIndex(field);

			TreeNode fieldNode = new TreeNode(field.Name, imageIndex, imageIndex);
			fieldNode.Tag = field;
			fieldNode.ToolTipText = field.Name;
			if (field is AddedField && !((AddedField)field).InCatalog)
			{
				fieldNode.StateImageIndex = (int)ImageLists.DatabaseExplorerImageIndex.New;
				fieldNode.ToolTipText = fieldNode.ToolTipText + " " + Resources.CustomFieldNotYetAvailableInObjectModel;
			}
			else if (IsCustomField(field))
			{
				fieldNode.StateImageIndex = (int)ImageLists.DatabaseExplorerImageIndex.EBNode;
				fieldNode.ToolTipText = fieldNode.ToolTipText + " " + Resources.CustomFieldAvailableInObjectModel;
			}
			return fieldNode;
		}

		//--------------------------------------------------------------------------------
		private bool IsCustomField(IRecordField field)
		{
			if (field is MSqlRecordItem)
			{
				MSqlRecordItem item = (MSqlRecordItem)field;
				NameSpace ownerModule = item.OwnerModule;
				return (
					ownerModule != null &&
					ownerModule.Application == BaseCustomizationContext.CustomizationContextInstance.CurrentEasyBuilderApp.ApplicationName &&
					ownerModule.Module == BaseCustomizationContext.CustomizationContextInstance.CurrentEasyBuilderApp.ModuleName
					);
			}
			return true;
		}
		//--------------------------------------------------------------------------------
		private bool IsCustomTable(IRecord iRecord)
		{
			if (iRecord is MSqlRecord)
			{
				MSqlRecord rec = (MSqlRecord)iRecord;
				NameSpace ownerModule = rec.NameSpace;
				return (
					ownerModule != null &&
					ownerModule.Application == BaseCustomizationContext.CustomizationContextInstance.CurrentEasyBuilderApp.ApplicationName &&
					ownerModule.Module == BaseCustomizationContext.CustomizationContextInstance.CurrentEasyBuilderApp.ModuleName
					);
			}
			return true;
		}

		//--------------------------------------------------------------------------------
		private static int GetFieldNodeImageIndex(IRecordField field)
		{
			int imageIndex = field.IsSegmentKey
						 ? (int)ImageLists.DatabaseExplorerImageIndex.Key
						 : (int)ImageLists.DatabaseExplorerImageIndex.Field;
			return imageIndex;
		}


		//--------------------------------------------------------------------------------
		[DebuggerStepThrough]
		void field_PropertyChanging(object sender, MyPropertyChangingArgs e)
		{
			if (e.PropertyName == "Name")
			{
				AddedField field = ((AddedField)sender);
				foreach (IRecordField f in field.Record.Fields)
					if (string.Compare(f.Name, (string)e.NewValue, StringComparison.InvariantCultureIgnoreCase) == 0)
						throw new ArgumentException(Resources.DuplicateResourceName, e.PropertyName);
				if (!IsValidName((string)e.NewValue))
					throw new ArgumentException(e.PropertyName);
			}
			else if (e.PropertyName == "IsSegmentKey")
			{
				if (e.NewValue.Equals(false))
				{
					AddedField field = ((AddedField)sender);
					if (field.Record.PrimaryKeyFields.Count <= 1)
						throw new ArgumentException(Resources.AtLeastOneKey, e.PropertyName);
				}
				else
				{
					AddedField field = ((AddedField)sender);
				
					if (field.DataObjType ==  DataType.Text)
						throw new ArgumentException(Resources.TextNoKey, e.PropertyName);
					if (field.Record.CreationRelease != field.CreationRelease)
						throw new ArgumentException(Resources.NoModifyKey, e.PropertyName);
						
				}
			}
			else if (e.PropertyName == "DataObjType")
			{
				AddedField field = ((AddedField)sender);
				if (field.IsSegmentKey && e.NewValue.Equals(DataType.Text))
					throw new ArgumentException(Resources.TextNoKey, e.PropertyName);

                if (field.DataObjType == DataType.Text || e.NewValue.Equals(DataType.Text))
                {
                    foreach (IRecordField col in field.Record.Fields)
                    {
                        if ((Microarea.TaskBuilderNet.Core.CoreTypes.DataType) col.DataObjType == DataType.Text && string.Compare(col.Name, field.Name, true) != 0)
                            throw new ArgumentException(Resources.NoMoreDataText, e.PropertyName);
                    }
                }
            }
		}

		//--------------------------------------------------------------------------------
		void field_PropertyChanged(object sender, PropertyChangedEventArgs e)
		{
			if (e.PropertyName == "Name")
			{
				AddedField f = ((AddedField)sender);
				f.TreeNode.Text = f.Name;
				f.TreeNode.ToolTipText = f.Name;
			}
			else if (e.PropertyName == "IsSegmentKey")
			{
				AddedField f = ((AddedField)sender);
				int imageIndex = GetFieldNodeImageIndex(f);
				f.TreeNode.ImageIndex = f.TreeNode.SelectedImageIndex = imageIndex;
				if (f.IsSegmentKey)
					f.Record.PrimaryKeyFields.Add(f);
				else
					f.Record.PrimaryKeyFields.Remove(f);
			}

			IsDirty = true;
		}

		//--------------------------------------------------------------------------------
		void table_PropertyChanged(object sender, PropertyChangedEventArgs e)
		{
			if (e.PropertyName == "Name")
			{
				AddedRecord f = ((AddedRecord)sender);
				f.TreeNode.Text = f.Name;
				f.TreeNode.ToolTipText = f.Name;
			}

			IsDirty = true;
		}

		//--------------------------------------------------------------------------------
		[DebuggerStepThrough]
		void table_PropertyChanging(object sender, MyPropertyChangingArgs e)
		{
			if (e.PropertyName == "Name")
			{
				if (GetTableInCatalog((string)e.NewValue) != null)
					throw new ArgumentException(Resources.DuplicateResourceName, e.PropertyName);
				if (!IsValidName((string)e.NewValue))
					throw new ArgumentException(e.PropertyName);
			}
		}

		//--------------------------------------------------------------------------------
		IRecord GetTableInCatalog(string name)
		{
			foreach (IRecord f in catalog.Tables)
				if (string.Compare(f.Name, name, StringComparison.InvariantCultureIgnoreCase) == 0)
					return f;
			return null;
		}

		//--------------------------------------------------------------------------------
		IRecord GetTableInTree(string name)
		{
			TreeNode n = GetTableNode(name);
			return n == null? null : n.Tag as IRecord;
		}

		//--------------------------------------------------------------------------------
		private bool IsValidName(string tableOrFieldName)
		{
			if (tableOrFieldName.Length == 0)
				return false;
			if (tableOrFieldName.Length > 30)
				return false;
			if (Char.IsDigit(tableOrFieldName[0]))
				return false;

			foreach (char ch in tableOrFieldName)
				if (!IsValidChar(ch))
					return false;
		
			return true;
		}

		//--------------------------------------------------------------------------------
		private bool IsValidChar(char ch)
		{
			if (Char.IsLetterOrDigit(ch))
				return true;
			switch (ch)
			{
				case '_':
				case '-':
					return true;
				default:
					return false;
			}
		}

		//--------------------------------------------------------------------------------
		private void addTableToolStripMenuItem_Click(object sender, EventArgs e)
		{
			DoAddTable(false);
		}

        //--------------------------------------------------------------------------------
        private void addMasterTableToolStripMenuItem_Click(object sender, EventArgs e)
        {
            DoAddTable(true);
        }

        //--------------------------------------------------------------------------------
        private void DoAddTable(bool master)
		{
			AddedRecord rec = new AddedRecord(GetNewName(dbNode.Nodes, "NewTable"));
            rec.IsMasterTable = master;
			NameSpace ns = GetNewTableNamespace(rec);

			rec.NameSpace = ns.ToString();
			TreeNode newTableNode = AddTableToTree(rec);
			tvTables.SelectedNode = newTableNode;
			UpdateSelectedComponent(rec, null, rec.Name);
			IsDirty = true;
			FireOnProperties();
		}

		//--------------------------------------------------------------------------------
		private static NameSpace GetNewTableNamespace(IRecord rec)
		{
			NameSpace ns = new NameSpace(
						 string.Format("{0}.{1}.{2}.{3}",
						 BaseCustomizationContext.CustomizationContextInstance.CurrentEasyBuilderApp.ApplicationName,
						 BaseCustomizationContext.CustomizationContextInstance.CurrentEasyBuilderApp.ModuleName,
						 BaseCustomizationContext.CustomizationContextInstance.DynamicLibraryName,
						 rec.Name),
						 TaskBuilderNet.Interfaces.NameSpaceObjectType.Table);
			return ns;
		}

		//--------------------------------------------------------------------------------
		private TreeNode AddTableToTree(AddedRecord rec)
		{
			rec.PropertyChanging += new EventHandler<MyPropertyChangingArgs>(table_PropertyChanging);
			rec.PropertyChanged += new PropertyChangedEventHandler(table_PropertyChanged);
			TreeNode newTableNode = CreateTableNode(rec);
			rec.TreeNode = newTableNode;
			dbNode.Nodes.Add(newTableNode);
			return newTableNode;
		}

		//--------------------------------------------------------------------------------
		private void DoDeleteTable()
		{
			if (tvTables.SelectedNode == null)
				return;
			IRecord recToDelete = tvTables.SelectedNode.Tag as IRecord;
			if (recToDelete == null)
				return;
			tvTables.SelectedNode.Remove();
			//se si trattava di una tabella esistente ma non registrata, devo rimettere 
			//la tabella originaria
			ImportedRecord impRec = recToDelete as ImportedRecord;
			if (impRec == null)
				return;

			tvTables.SelectedNode = AddTable(dbNode.Nodes, impRec.UnregisteredRecord);
		}

		//--------------------------------------------------------------------------------
		private void addFieldToolStripMenuItem_Click(object sender, EventArgs e)
		{
			DoAddField();
		}

		//--------------------------------------------------------------------------------
		private void DoAddField()
		{
			TreeNode contextNode = tvTables.SelectedNode;
			if (contextNode == null)
				return;
			AddFieldToNode(contextNode);
		}

		//--------------------------------------------------------------------------------
		private AddedField AddFieldToNode(TreeNode contextNode)
		{
			IRecord record = contextNode.Tag as IRecord;
			if (record == null || !record.IsRegistered)
				return null;

			AddedField field = new AddedField(record, GetNewName(contextNode.Nodes, "NewField"));
			record.Fields.Add(field);
			TreeNode newFieldNode = AddFieldToTree(contextNode, field);
			tvTables.SelectedNode = newFieldNode;
			//fare dopo la AddFieldToTree, altrimenti non ci sono gli eventi allineati
			if (record.Fields.Count == 1 && IsCustomTable(record))
				field.IsSegmentKey = true;

			UpdateSelectedComponent(field, field.Record as IContainer, field.Name);
			IsDirty = true;
			FireOnProperties();
			return field;
		}

		//--------------------------------------------------------------------------------
		private TreeNode AddFieldToTree(TreeNode parentNode, AddedField field)
		{
			field.PropertyChanged += new PropertyChangedEventHandler(field_PropertyChanged);
			field.PropertyChanging += new EventHandler<MyPropertyChangingArgs>(field_PropertyChanging);
			TreeNode newFieldNode = CreateFieldNode(field);
			field.TreeNode = newFieldNode;
			parentNode.Nodes.Add(newFieldNode);
			return newFieldNode;
		}

		//--------------------------------------------------------------------------------
		private void DoDeleteField()
		{
			if (tvTables.SelectedNode == null)
				return;

			AddedField fieldToDelete = tvTables.SelectedNode.Tag as AddedField;
			if (fieldToDelete == null)
				return;

			tvTables.SelectedNode.Remove();
			fieldToDelete.Record.Fields.Remove(fieldToDelete);
		}

		//--------------------------------------------------------------------------------
		private string GetNewName(TreeNodeCollection nodes, string templateName)
		{
			int idx = 1;
			bool found = false;
			string name;
			do
			{
				found = false;
				name = templateName + idx;
				foreach (TreeNode n in nodes)
					if (n.Text == name)
					{
						found = true;
						break;
					}
				idx++;
			}
			while (found);
			return name;
		}

		//--------------------------------------------------------------------------------
		private void tvTables_AfterSelect(object sender, TreeViewEventArgs e)
		{
			object o = e.Node.Tag;

			deleteToolStripMenuItem.Enabled = CanDeleteField(false) || CanDeleteTable(false);
			importTableToolStripMenuItem.Enabled = CanImportTable();
            importMasterTableToolStripMenuItem.Enabled = CanImportTable();

            //Abilito l'aggiunta di un campo solo se ho selezionato una tabella
            this.addFieldToolStripMenuItem.Enabled = (e.Node != null && e.Node.Tag as IRecord != null && ((IRecord)e.Node.Tag).IsRegistered);

			//Abilito la voce proprietà solo se non ho selezionato la root dell'albero.
			this.propertiesToolStripMenuItem.Enabled = (e.Node != null && e.Node.Parent != null);

			if (o is IRecord)
			{
				UpdateSelectedComponent((IComponent)o, null, ((IRecord)o).Name);
				return;
			}
            IRecordField recordField = o as IRecordField;

            if (recordField != null)
			{
                IComponent c = o as IComponent;
                IContainer co = recordField.Record as IContainer;
                UpdateSelectedComponent(c, co, recordField.Name);
				return;
			}
		}
		//--------------------------------------------------------------------------------
		private void UpdateSelectedComponent(IComponent component, IContainer container, string name)
		{
			if (component != null)
				component.Site = new TBSite(component, container, editor, name);
			selectionService.SetSelectedComponents(new Object[] { component });
		}
		//--------------------------------------------------------------------------------
		private void propertiesToolStripMenuItem_Click(object sender, EventArgs e)
		{
			FireOnProperties();
		}

		/// <summary>
		/// 
		/// </summary>
		//--------------------------------------------------------------------------------
		protected override void OnVisibleChanged(EventArgs e)
		{
			if (Visible && OpenTwinPanel != null)
				OpenTwinPanel(this, e);
		}

		//--------------------------------------------------------------------------------
		private void FireOnProperties()
		{
			if (OpenProperties != null)
				OpenProperties(this, EventArgs.Empty);
		}

		//--------------------------------------------------------------------------------
		private void tvTables_MouseDoubleClick(object sender, MouseEventArgs e)
		{
			FireOnProperties();
		}

		//--------------------------------------------------------------------------------
		private bool ApplyChangesToObjectModel(bool unattended)
		{
			DatabaseChanges changes = null;
			DatabaseChangesCurrentRelease currentChanges = null;
			try
			{
				changes = GetDatabaseChanges();
				currentChanges = changes.GetCurrentReleaseChanges(BaseCustomizationContext.CustomizationContextInstance.CurrentEasyBuilderApp);
				SaveToDBInfo(currentChanges);
			}
			catch (IOException)
			{
				if (!unattended)
					MessageBox.Show(this, Resources.EbDbInfoReadonly, Resources.EBDbInfoErrorCaption, MessageBoxButtons.OK, MessageBoxIcon.Error);
				return false;
			}
			try
			{
				ApplyChangesToObjectModel(currentChanges, true);//isvirtual true: non esiste su DB
				if (!unattended)
                {
                    //ricarico l'albero del database
                    LoadTree(null);
                    MessageBox.Show(
                            this,
                            Resources.ModelChangesOK,
                            this.Text,
                            MessageBoxButtons.OK,
                            MessageBoxIcon.Information
                            );
                }
                return true;
			}
			catch (Exception ex)
			{
				if (!unattended)
					MessageBox.Show(
						this,
						ex.Message,
						this.Text,
						MessageBoxButtons.OK,
						MessageBoxIcon.Error);
				return false;
			}
		}

		//--------------------------------------------------------------------------------
		private void ApplyChangesToObjectModel(DatabaseChangesCurrentRelease currentChanges, bool isVirtual)
		{
			currentChanges.ApplyChangesToObjectModel(catalog, isVirtual);
			if (CatalogChanged != null)
				CatalogChanged(this, EventArgs.Empty);
		}


		//--------------------------------------------------------------------------------
		private void ApplyChangesToDB()
		{
			/*int users = LoginManager.GetLoggedUsersNumber();
			if (users > 1)
			{
				MessageBox.Show(
					this,
					string.Format(Resources.OtherLoggedUsers, users - 1),
					this.Text,
					MessageBoxButtons.OK, MessageBoxIcon.Error);
				return;
			}*/
			DatabaseChanges changes = GetDatabaseChanges();
			DatabaseChangesCurrentRelease currentChanges = changes.GetCurrentReleaseChanges(BaseCustomizationContext.CustomizationContextInstance.CurrentEasyBuilderApp);
			if (currentChanges.IsEmpty)
			{
				MessageBox.Show(
					this,
					Resources.NoChangesToDb,
					this.Text,
					MessageBoxButtons.OK,
					MessageBoxIcon.Information);
				return;
			}

			if (DialogResult.Yes != MessageBox.Show(this, Resources.ThisOperationWillChangeYourDatabase, this.Text, MessageBoxButtons.YesNo, MessageBoxIcon.Warning))
				return;

			try
			{
				SaveToDBInfo(currentChanges);
			}
			catch (IOException)
			{
				MessageBox.Show(this, Resources.EbDbInfoReadonly, Resources.EBDbInfoErrorCaption, MessageBoxButtons.OK, MessageBoxIcon.Error);
				return;
			}

			Diagnostic d = new Diagnostic(NameSolverStrings.EasyStudio);

			try
			{
                using (var splash = EBSplash.StartSplash())
				{
					splash.SetMessage(Resources.GeneratingDatabaseScripts);
					DBScriptGenerator generator = new DBScriptGenerator(changes);

					string backFolder = generator.Generate();

					d.SetInformation(Resources.ScriptGenerated);
					if (!string.IsNullOrEmpty(backFolder))
						d.SetInformation(string.Format(Resources.BackupFolderCreated, backFolder));

					Diagnostic dbDiagnostic = null;
					splash.SetMessage(Resources.ApplyingChangesToDatabase);

					// l'applicazione delle modifiche su database non si applicano per oracle
					bool dbOk = catalog.DbmsType == DBMSType.ORACLE ? true : ApplyToDb(out dbDiagnostic);

					if (!dbOk)
					{
						d.Set(dbDiagnostic);
						d.SetError(Resources.DBChangesError);
					}
					else
						d.SetInformation(
											catalog.DbmsType == DBMSType.ORACLE ?
											Resources.UseAdminConsoleForOracle :
											Resources.DBChangesOk
										);

					if (dbOk)
					{
						splash.SetMessage(Resources.ApplyingChangesToObjectModel);
						ApplyChangesToObjectModel(currentChanges, false); //isvirtual false: le colonne esistono su database
						BaseCustomizationContext.CustomizationContextInstance.CurrentEasyBuilderApp.ModuleInfo.CurrentDBRelease++;
						d.SetInformation(Resources.ModelChangesOK);
					}
				}
				IsDirty = false;
			}
			catch (Exception ex)
			{
				d.SetError(ex.Message);
			}
			using (SafeThreadCallContext context = new SafeThreadCallContext())
				DiagnosticViewer.ShowDiagnostic(d);

            //ricarico l'albero del database
            LoadTree(null);
        }

        /// <summary>
        /// Recupera tutte le modifiche che la customizzazione apporta al database, sia nella corrente release che in quelle precedenti
        /// </summary>
        /// <returns></returns>
        //--------------------------------------------------------------------------------
        private DatabaseChanges GetDatabaseChanges()
		{
			DatabaseChanges changes = new DatabaseChanges();
			foreach (TreeNode tableNode in dbNode.Nodes)
			{
				//il namespace della tabella è mio? devo metterla nella lista delle tabelle
				//da mettere nella createinfo (sia che lo abbia aggiunto adesso sia in uno scatto di release precedente)
				IRecord rec = (IRecord)tableNode.Tag;
				bool customTable = IsCustomTable(rec);
				if (customTable && rec.Fields.Count > 0)
				{
					if (rec.PrimaryKeyFields.Count == 0)
						throw new ApplicationException(string.Format(Resources.NoPrimaryKey, rec.Name));

					AddedRecord clonedRec = rec is ImportedRecord
						? new ImportedRecord(rec)
						: new AddedRecord(rec, -1, true);
					changes.NewTables.Add(clonedRec);
				}

				//il namespace della tabella non è mio? controllo solo l'aggiunta di campi
				foreach (TreeNode fieldNode in tableNode.Nodes)
				{
					if (!customTable && IsCustomField((IRecordField)fieldNode.Tag))
					{
						changes.AddAddonField((NameSpace)((IRecord)tableNode.Tag).NameSpace, (IRecordField)fieldNode.Tag);
					}
				}
			}

			return changes;
		}


		//--------------------------------------------------------------------------------
		private bool ApplyToDb(out Diagnostic diagnostic)
		{
			int companyId = CUtility.GetCompanyId();
			PathFinder pf = new PathFinder(CUtility.GetCompany(), CUtility.GetUser());
			DatabaseEngine engine = new DatabaseEngine(pf, LoginManager.GetDBNetworkType(), InstallationData.Country, InstallationData.BrandLoader, CUtility.GetDataBaseCultureLCID());
			bool result = engine.CreateUpgradeCompanyDatabase(companyId.ToString());
			diagnostic = engine.DbEngineDiagnostic;
			return result;
		}


		//--------------------------------------------------------------------------------
		private void LoadFromDbInfo(DBScriptInfo info)
		{
			try
			{
				foreach (AddedRecord record in info.NewAddOnFields)
				{
					TreeNode parent = GetNodeByName(dbNode.Nodes, record.Name);
					if (parent == null) continue;

					foreach (AddedField field in record.Fields)
					{
						IRecord parentRecord = (IRecord)parent.Tag;

						//controllo se esiste già (caso in cui ho applicato la modifica al database
						//e il file serializzato è rimasto)
						if (GetFieldNode(parent, field.Name) != null)
							continue;

						//aggancio il parent vero a quello fittizio
						field.Record = parentRecord;
						parentRecord.Fields.Add(field);

						AddFieldToTree(parent, field);
						IsDirty = true;
					}
				}

				foreach (AddedRecord dummyRecord in info.NewTables)
				{
					//controllo se esiste già (caso in cui ho applicato la modifica al database
					//e il file serializzato è rimasto)
					TreeNode parent = GetTableNode(dummyRecord.Name);
					if (parent == null)
						parent = AddTableToTree(dummyRecord);
					else if (!((IRecord)parent.Tag).IsRegistered)
						parent = ImportTableNode(parent, dummyRecord.IsMasterTable);

					if (parent == null) continue;
					IRecord realRecord = parent.Tag as IRecord;
					foreach (AddedField field in dummyRecord.Fields)
					{
						//controllo se esiste già (caso in cui ho applicato la modifica al database
						//e il file serializzato è rimasto)
						if (GetFieldNode(parent, field.Name) != null)
							continue;

						//metto a posto il parent che si è perso nella serializzazione
						field.Record = realRecord;
						AddFieldToTree(parent, field);
						IsDirty = true;
					}

				}
				tbFilter.Checked = info.OnlyMyItems;
			}
			catch (Exception ex)
			{
				System.Diagnostics.Debug.Fail(ex.Message);
			}
		}

		//--------------------------------------------------------------------------------
		private TreeNode GetTableNode(string name)
		{
			foreach (TreeNode tn in dbNode.Nodes)
				if (((IRecord)tn.Tag).Name == name)
					return tn;
			return null;
		}
		//--------------------------------------------------------------------------------
		private TreeNode GetFieldNode(TreeNode tableNode, string name)
		{
			foreach (TreeNode tn in tableNode.Nodes)
				if (((IRecordField)tn.Tag).Name == name)
					return tn;
			return null;
		}
		//--------------------------------------------------------------------------------
		private static TreeNode GetNodeByName(TreeNodeCollection treeNodeCollection, string name)
		{
			foreach (TreeNode node in treeNodeCollection)
				if (((IRecord)node.Tag).Name == name)
					return node;
			return null;
		}
		//--------------------------------------------------------------------------------
		private void tvTables_MouseDown(object sender, MouseEventArgs e)
		{
			if (e.Button == System.Windows.Forms.MouseButtons.Right)
				tvTables.SelectedNode = tvTables.GetNodeAt(e.X, e.Y);
			else if (e.Button == System.Windows.Forms.MouseButtons.Left)
			{
				dragNode = (TreeNode)tvTables.GetNodeAt(e.Location);
				//Se il nodo selezionato è nullo o diverso dal precedente selezionato, 
				//viene impostato il nodo su cui si è effettuata il click per il drag drop
				if (dragNode != null && (tvTables.SelectedNode == null || tvTables.SelectedNode != dragNode))
					tvTables.SelectedNode = dragNode;
			}
		}
		//--------------------------------------------------------------------------------
		private void tvTables_MouseMove(object sender, MouseEventArgs e)
		{
			if (e.Button != System.Windows.Forms.MouseButtons.Left)
				return;

			if (dragNode == null)
				return;

			try
			{
				string tableName = null;
				MSqlRecord rec = dragNode.Tag as MSqlRecord;
				if (rec != null)
					tableName = rec.IsRegistered ? rec.Name : "";
				else
				{
					AddedRecord ar = dragNode.Tag as AddedRecord;
					if (ar != null)
						tableName = ar.Name;
				}
				if (!tableName.IsNullOrEmpty())
					DoDragDrop(tableName, DragDropEffects.Copy);
			}
			catch { }
		}

		//--------------------------------------------------------------------------------
		private void cbMyItems_CheckedChanged(object sender, EventArgs e)
		{
			if (loadingTree)
				return;
			try
			{
				SaveToDBInfo(GetDatabaseChanges().GetCurrentReleaseChanges(BaseCustomizationContext.CustomizationContextInstance.CurrentEasyBuilderApp));
			}
			catch (IOException)
			{
				MessageBox.Show(this, Resources.EbDbInfoReadonly, Resources.EBDbInfoErrorCaption, MessageBoxButtons.OK, MessageBoxIcon.Error);
				return;
			}

			LoadTree(tbFilter.Checked);
		}

		//--------------------------------------------------------------------------------
		internal void AddField(MSqlRecord mSqlRecord)
		{
			//elimino eventuali filtraggi
			if (tbFilter.Checked)
				tbFilter.Checked = false;

			foreach (TreeNode node in dbNode.Nodes)
				if (((IRecord)node.Tag).Name == mSqlRecord.Name)
				{
					AddedField field = AddFieldToNode(node);

					break;
				}
		}

		//--------------------------------------------------------------------------------
		private void tsbSaveToObjectModel_Click(object sender, EventArgs e)
		{
			if (NotAlone())
				return;

			ApplyChangesToObjectModel(false);

		}

		//--------------------------------------------------------------------------------
		private void tbSaveToDatabase_Click(object sender, EventArgs e)
		{
			if (NotAlone())
				return;

			ApplyChangesToDB();
		}

		//--------------------------------------------------------------------------------
		private bool NotAlone()
		{
			return BaseCustomizationContext.CustomizationContextInstance.NotAlone(this.Text, 1, 1, this);
		}

		//--------------------------------------------------------------------------------
		private void tbAdd_Click(object sender, EventArgs e)
		{
			if (tvTables.SelectedNode == null || !(tvTables.SelectedNode.Tag is IRecord))
			{
				DoAddTable(false);
				return;
			}
			//di default aggiungo una tabella, a meno che il selected node sia una tabella allora aggiungo un campo
			DoAddField();
		}

		//--------------------------------------------------------------------------------
		private void tbProperties_Click(object sender, EventArgs e)
		{
			FireOnProperties();
		}

		//--------------------------------------------------------------------------------
		private void deleteToolStripMenuItem_Click(object sender, EventArgs e)
		{
			if (tvTables.SelectedNode == null)
				return;

			if (CanDeleteTable(true))
			{
				DoDeleteTable();
				return;
			}
			if (CanDeleteField(true))
			{
				DoDeleteField();
				return;
			}

		}

		//--------------------------------------------------------------------------------
		private bool CanDeleteField(bool ask)
		{
			if (tvTables.SelectedNode == null) return false;

			IRecordField fieldToDelete = tvTables.SelectedNode.Tag as IRecordField;
			if (fieldToDelete == null ||
				!IsCustomField(fieldToDelete) ||
				fieldToDelete.CreationRelease != DatabaseChanges.GetDatabaseRelease(BaseCustomizationContext.CustomizationContextInstance.CurrentEasyBuilderApp))
				return false;
			if (!ask) return true;
			return DialogResult.Yes == MessageBox.Show(
				this,
				String.Format(Resources.ConfirmDeleteField, fieldToDelete.Name),
				Text,
				MessageBoxButtons.YesNo,
				MessageBoxIcon.Question
				);
		}

		//--------------------------------------------------------------------------------
		private bool CanDeleteTable(bool ask)
		{
			if (tvTables.SelectedNode == null) return false;
			IRecord recToDelete = tvTables.SelectedNode.Tag as IRecord;
			if (recToDelete == null ||
				!IsCustomTable(recToDelete) ||
				recToDelete.CreationRelease != DatabaseChanges.GetDatabaseRelease(BaseCustomizationContext.CustomizationContextInstance.CurrentEasyBuilderApp))
				return false;
			if (!ask) return true;
			return DialogResult.Yes == MessageBox.Show(
				this,
				String.Format(Resources.ConfirmDeleteTable, recToDelete.Name),
				Text,
				MessageBoxButtons.YesNo,
				MessageBoxIcon.Question
				);
		}

		//--------------------------------------------------------------------------------
		private bool CanImportTable()
		{
			if (tvTables.SelectedNode == null) return false;
			IRecord recToImport = tvTables.SelectedNode.Tag as IRecord;
			if (recToImport == null ||
				recToImport.IsRegistered)
				return false;
			return true;
		}

        //--------------------------------------------------------------------------------
        private void importMasterTableToolStripMenuItem_Click(object sender, EventArgs e)
        {
            DoImportTable(true);
        }

        //--------------------------------------------------------------------------------
        private void importTableToolStripMenuItem_Click(object sender, EventArgs e)
        {
            DoImportTable();
        }
        //--------------------------------------------------------------------------------
        private void DoImportTable(bool isMasterTable = false)
		{
			TreeNode n = tvTables.SelectedNode;
			if (n == null)
				return;
			IRecord rec = n.Tag as IRecord;
			if (rec.PrimaryKeyFields.Count == 0)
			{
				MessageBox.Show(this, Resources.CannotImportTable);
				return;
			}
			tvTables.SelectedNode = ImportTableNode(n, isMasterTable);
			tvTables.SelectedNode.Expand();
		}

		//--------------------------------------------------------------------------------
		private TreeNode ImportTableNode(TreeNode existingUnregisteredTableNode, bool isMasterTable = false)
		{
			IRecord rec = existingUnregisteredTableNode.Tag as IRecord;
			ImportedRecord newTable = new ImportedRecord(rec);
                        NameSpace ns = GetNewTableNamespace(rec);
			newTable.NameSpace = ns;
            newTable.IsMasterTable = isMasterTable;
            newTable.PropertyChanging += new EventHandler<MyPropertyChangingArgs>(table_PropertyChanging);
			newTable.PropertyChanged += new PropertyChangedEventHandler(table_PropertyChanged);
			existingUnregisteredTableNode.Remove();
			return AddTable(dbNode.Nodes, newTable);
		}

		//--------------------------------------------------------------------------------
		internal bool EnsureValidTable(string tableName)
		{
			IRecord table = GetTableInTree(tableName);
			if (table == null)
				return false;
			if (table is AddedRecord && !((AddedRecord)table).InCatalog)
				return ApplyChangesToObjectModel(true);
			return true;
		}
	}

	//=========================================================================
	class TreeNodeComparer : IComparer, IComparer<TreeNode>
	{
		//---------------------------------------------------------------------
		public int Compare(object x, object y)
		{
			return Compare(x as TreeNode, y as TreeNode);
		}

		//---------------------------------------------------------------------
		public int Compare(TreeNode x, TreeNode y)
		{
			//non applico ordinamento sui campi, ma solo sulle tabelle
			//così i campi vengono inseriti nell'ordine specificato dall'utente
			if (x.Tag is IRecordField || y.Tag is IRecordField)
				return 0;

			return x.Text.CompareTo(y.Text);
		}
	}
}
