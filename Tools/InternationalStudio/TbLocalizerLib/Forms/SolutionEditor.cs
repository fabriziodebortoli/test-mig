using System;
using System.Collections;
using System.Data;
using System.IO;
using System.Windows.Forms;

namespace Microarea.Tools.TBLocalizer.Forms
{
	public class SolutionEditor : Form
	{
		#region Controls
		private DataGrid dgProjects;
		private Label label1;
		private DataColumn dataColumnDummy;
		private DataView dwProjects;
		private DataSet dsProjects;
		private DataTable dtProjects;
		private DataColumn dataColumnPath;
		private DataGridTableStyle dataGridTableStyleProjects;
		private DataGridTextBoxColumn dataGridTextBoxColumnPath;
		private Button btnOK;
		private Button btnAdd;
		private Button btnCancel;
		private SolutionDocument solutionDocument;
		private ContextMenu ProjectContextMenu;
		private MenuItem MIEnvironment;

		private System.ComponentModel.Container components = null;
		#endregion

		#region Public properties
		//---------------------------------------------------------------------
		public string[] SelectedProjects
		{
			get
			{
				ArrayList list = new ArrayList();
				foreach (DataRow r in dtProjects.Rows)
					list.Add(r[dataColumnPath.ColumnName]);

				return (string[])list.ToArray(typeof(string));
			}
		}
		#endregion

		#region Constructors
		//---------------------------------------------------------------------
		public SolutionEditor(SolutionDocument solutionDocument)
		{
			InitializeComponent();

			this.dtProjects.RowChanging += new DataRowChangeEventHandler(Projects_RowChanging);
			this.solutionDocument = solutionDocument;
		}
		#endregion

		/// <summary>
		/// Clean up any resources being used.
		/// </summary>
		//--------------------------------------------------------------------------------
		protected override void Dispose( bool disposing )
		{
			if( disposing )
			{
				if(components != null)
				{
					components.Dispose();
				}
			}
			base.Dispose( disposing );
		}


		#region Windows Form Designer generated code
		/// <summary>
		/// Required method for Designer support - do not modify
		/// the contents of this method with the code editor.
		/// </summary>
		//--------------------------------------------------------------------------------
		private void InitializeComponent()
		{
			System.Resources.ResourceManager resources = new System.Resources.ResourceManager(typeof(SolutionEditor));
			this.dgProjects = new System.Windows.Forms.DataGrid();
			this.dwProjects = new System.Data.DataView();
			this.dtProjects = new System.Data.DataTable();
			this.dataColumnPath = new System.Data.DataColumn();
			this.dataColumnDummy = new System.Data.DataColumn();
			this.dataGridTableStyleProjects = new System.Windows.Forms.DataGridTableStyle();
			this.dataGridTextBoxColumnPath = new System.Windows.Forms.DataGridTextBoxColumn();
			this.ProjectContextMenu = new System.Windows.Forms.ContextMenu();
			this.MIEnvironment = new System.Windows.Forms.MenuItem();
			this.label1 = new System.Windows.Forms.Label();
			this.dsProjects = new System.Data.DataSet();
			this.btnOK = new System.Windows.Forms.Button();
			this.btnAdd = new System.Windows.Forms.Button();
			this.btnCancel = new System.Windows.Forms.Button();
			((System.ComponentModel.ISupportInitialize)(this.dgProjects)).BeginInit();
			((System.ComponentModel.ISupportInitialize)(this.dwProjects)).BeginInit();
			((System.ComponentModel.ISupportInitialize)(this.dtProjects)).BeginInit();
			((System.ComponentModel.ISupportInitialize)(this.dsProjects)).BeginInit();
			this.SuspendLayout();
			// 
			// dgProjects
			// 
			this.dgProjects.AccessibleDescription = resources.GetString("dgProjects.AccessibleDescription");
			this.dgProjects.AccessibleName = resources.GetString("dgProjects.AccessibleName");
			this.dgProjects.Anchor = ((System.Windows.Forms.AnchorStyles)(resources.GetObject("dgProjects.Anchor")));
			this.dgProjects.BackgroundImage = ((System.Drawing.Image)(resources.GetObject("dgProjects.BackgroundImage")));
			this.dgProjects.CaptionFont = ((System.Drawing.Font)(resources.GetObject("dgProjects.CaptionFont")));
			this.dgProjects.CaptionText = resources.GetString("dgProjects.CaptionText");
			this.dgProjects.DataMember = "";
			this.dgProjects.DataSource = this.dwProjects;
			this.dgProjects.Dock = ((System.Windows.Forms.DockStyle)(resources.GetObject("dgProjects.Dock")));
			this.dgProjects.Enabled = ((bool)(resources.GetObject("dgProjects.Enabled")));
			this.dgProjects.Font = ((System.Drawing.Font)(resources.GetObject("dgProjects.Font")));
			this.dgProjects.HeaderForeColor = System.Drawing.SystemColors.ControlText;
			this.dgProjects.ImeMode = ((System.Windows.Forms.ImeMode)(resources.GetObject("dgProjects.ImeMode")));
			this.dgProjects.Location = ((System.Drawing.Point)(resources.GetObject("dgProjects.Location")));
			this.dgProjects.Name = "dgProjects";
			this.dgProjects.RightToLeft = ((System.Windows.Forms.RightToLeft)(resources.GetObject("dgProjects.RightToLeft")));
			this.dgProjects.Size = ((System.Drawing.Size)(resources.GetObject("dgProjects.Size")));
			this.dgProjects.TabIndex = ((int)(resources.GetObject("dgProjects.TabIndex")));
			this.dgProjects.TableStyles.AddRange(new System.Windows.Forms.DataGridTableStyle[] {
																								   this.dataGridTableStyleProjects});
			this.dgProjects.Visible = ((bool)(resources.GetObject("dgProjects.Visible")));
			// 
			// dwProjects
			// 
			this.dwProjects.AllowNew = false;
			this.dwProjects.Table = this.dtProjects;
			// 
			// dtProjects
			// 
			this.dtProjects.Columns.AddRange(new System.Data.DataColumn[] {
																			  this.dataColumnPath,
																			  this.dataColumnDummy});
			this.dtProjects.TableName = "TableProjects";
			// 
			// dataColumnPath
			// 
			this.dataColumnPath.Caption = "Path";
			this.dataColumnPath.ColumnName = "ColumnPath";
			// 
			// dataColumnDummy
			// 
			this.dataColumnDummy.ColumnName = "ColumnDummy";
			// 
			// dataGridTableStyleProjects
			// 
			this.dataGridTableStyleProjects.DataGrid = this.dgProjects;
			this.dataGridTableStyleProjects.GridColumnStyles.AddRange(new System.Windows.Forms.DataGridColumnStyle[] {
																														 this.dataGridTextBoxColumnPath});
			this.dataGridTableStyleProjects.HeaderFont = ((System.Drawing.Font)(resources.GetObject("dataGridTableStyleProjects.HeaderFont")));
			this.dataGridTableStyleProjects.HeaderForeColor = System.Drawing.SystemColors.ControlText;
			this.dataGridTableStyleProjects.MappingName = "TableProjects";
			this.dataGridTableStyleProjects.PreferredColumnWidth = ((int)(resources.GetObject("dataGridTableStyleProjects.PreferredColumnWidth")));
			this.dataGridTableStyleProjects.PreferredRowHeight = ((int)(resources.GetObject("dataGridTableStyleProjects.PreferredRowHeight")));
			this.dataGridTableStyleProjects.RowHeaderWidth = ((int)(resources.GetObject("dataGridTableStyleProjects.RowHeaderWidth")));
			// 
			// dataGridTextBoxColumnPath
			// 
			this.dataGridTextBoxColumnPath.Alignment = ((System.Windows.Forms.HorizontalAlignment)(resources.GetObject("dataGridTextBoxColumnPath.Alignment")));
			this.dataGridTextBoxColumnPath.Format = "";
			this.dataGridTextBoxColumnPath.FormatInfo = null;
			this.dataGridTextBoxColumnPath.HeaderText = resources.GetString("dataGridTextBoxColumnPath.HeaderText");
			this.dataGridTextBoxColumnPath.MappingName = resources.GetString("dataGridTextBoxColumnPath.MappingName");
			this.dataGridTextBoxColumnPath.NullText = resources.GetString("dataGridTextBoxColumnPath.NullText");
			this.dataGridTextBoxColumnPath.Width = ((int)(resources.GetObject("dataGridTextBoxColumnPath.Width")));
			// 
			// ProjectContextMenu
			// 
			this.ProjectContextMenu.MenuItems.AddRange(new System.Windows.Forms.MenuItem[] {
																							   this.MIEnvironment});
			this.ProjectContextMenu.RightToLeft = ((System.Windows.Forms.RightToLeft)(resources.GetObject("ProjectContextMenu.RightToLeft")));
			// 
			// MIEnvironment
			// 
			this.MIEnvironment.Enabled = ((bool)(resources.GetObject("MIEnvironment.Enabled")));
			this.MIEnvironment.Index = 0;
			this.MIEnvironment.Shortcut = ((System.Windows.Forms.Shortcut)(resources.GetObject("MIEnvironment.Shortcut")));
			this.MIEnvironment.ShowShortcut = ((bool)(resources.GetObject("MIEnvironment.ShowShortcut")));
			this.MIEnvironment.Text = resources.GetString("MIEnvironment.Text");
			this.MIEnvironment.Visible = ((bool)(resources.GetObject("MIEnvironment.Visible")));
			this.MIEnvironment.Click += new System.EventHandler(this.MIEnvironment_Click);
			// 
			// label1
			// 
			this.label1.AccessibleDescription = resources.GetString("label1.AccessibleDescription");
			this.label1.AccessibleName = resources.GetString("label1.AccessibleName");
			this.label1.Anchor = ((System.Windows.Forms.AnchorStyles)(resources.GetObject("label1.Anchor")));
			this.label1.AutoSize = ((bool)(resources.GetObject("label1.AutoSize")));
			this.label1.Dock = ((System.Windows.Forms.DockStyle)(resources.GetObject("label1.Dock")));
			this.label1.Enabled = ((bool)(resources.GetObject("label1.Enabled")));
			this.label1.Font = ((System.Drawing.Font)(resources.GetObject("label1.Font")));
			this.label1.Image = ((System.Drawing.Image)(resources.GetObject("label1.Image")));
			this.label1.ImageAlign = ((System.Drawing.ContentAlignment)(resources.GetObject("label1.ImageAlign")));
			this.label1.ImageIndex = ((int)(resources.GetObject("label1.ImageIndex")));
			this.label1.ImeMode = ((System.Windows.Forms.ImeMode)(resources.GetObject("label1.ImeMode")));
			this.label1.Location = ((System.Drawing.Point)(resources.GetObject("label1.Location")));
			this.label1.Name = "label1";
			this.label1.RightToLeft = ((System.Windows.Forms.RightToLeft)(resources.GetObject("label1.RightToLeft")));
			this.label1.Size = ((System.Drawing.Size)(resources.GetObject("label1.Size")));
			this.label1.TabIndex = ((int)(resources.GetObject("label1.TabIndex")));
			this.label1.Text = resources.GetString("label1.Text");
			this.label1.TextAlign = ((System.Drawing.ContentAlignment)(resources.GetObject("label1.TextAlign")));
			this.label1.Visible = ((bool)(resources.GetObject("label1.Visible")));
			// 
			// dsProjects
			// 
			this.dsProjects.DataSetName = "Projects";
			this.dsProjects.Locale = new System.Globalization.CultureInfo("it-IT");
			this.dsProjects.Tables.AddRange(new System.Data.DataTable[] {
																			this.dtProjects});
			// 
			// btnOK
			// 
			this.btnOK.AccessibleDescription = resources.GetString("btnOK.AccessibleDescription");
			this.btnOK.AccessibleName = resources.GetString("btnOK.AccessibleName");
			this.btnOK.Anchor = ((System.Windows.Forms.AnchorStyles)(resources.GetObject("btnOK.Anchor")));
			this.btnOK.BackgroundImage = ((System.Drawing.Image)(resources.GetObject("btnOK.BackgroundImage")));
			this.btnOK.DialogResult = System.Windows.Forms.DialogResult.OK;
			this.btnOK.Dock = ((System.Windows.Forms.DockStyle)(resources.GetObject("btnOK.Dock")));
			this.btnOK.Enabled = ((bool)(resources.GetObject("btnOK.Enabled")));
			this.btnOK.FlatStyle = ((System.Windows.Forms.FlatStyle)(resources.GetObject("btnOK.FlatStyle")));
			this.btnOK.Font = ((System.Drawing.Font)(resources.GetObject("btnOK.Font")));
			this.btnOK.Image = ((System.Drawing.Image)(resources.GetObject("btnOK.Image")));
			this.btnOK.ImageAlign = ((System.Drawing.ContentAlignment)(resources.GetObject("btnOK.ImageAlign")));
			this.btnOK.ImageIndex = ((int)(resources.GetObject("btnOK.ImageIndex")));
			this.btnOK.ImeMode = ((System.Windows.Forms.ImeMode)(resources.GetObject("btnOK.ImeMode")));
			this.btnOK.Location = ((System.Drawing.Point)(resources.GetObject("btnOK.Location")));
			this.btnOK.Name = "btnOK";
			this.btnOK.RightToLeft = ((System.Windows.Forms.RightToLeft)(resources.GetObject("btnOK.RightToLeft")));
			this.btnOK.Size = ((System.Drawing.Size)(resources.GetObject("btnOK.Size")));
			this.btnOK.TabIndex = ((int)(resources.GetObject("btnOK.TabIndex")));
			this.btnOK.Text = resources.GetString("btnOK.Text");
			this.btnOK.TextAlign = ((System.Drawing.ContentAlignment)(resources.GetObject("btnOK.TextAlign")));
			this.btnOK.Visible = ((bool)(resources.GetObject("btnOK.Visible")));
			// 
			// btnAdd
			// 
			this.btnAdd.AccessibleDescription = resources.GetString("btnAdd.AccessibleDescription");
			this.btnAdd.AccessibleName = resources.GetString("btnAdd.AccessibleName");
			this.btnAdd.Anchor = ((System.Windows.Forms.AnchorStyles)(resources.GetObject("btnAdd.Anchor")));
			this.btnAdd.BackgroundImage = ((System.Drawing.Image)(resources.GetObject("btnAdd.BackgroundImage")));
			this.btnAdd.Dock = ((System.Windows.Forms.DockStyle)(resources.GetObject("btnAdd.Dock")));
			this.btnAdd.Enabled = ((bool)(resources.GetObject("btnAdd.Enabled")));
			this.btnAdd.FlatStyle = ((System.Windows.Forms.FlatStyle)(resources.GetObject("btnAdd.FlatStyle")));
			this.btnAdd.Font = ((System.Drawing.Font)(resources.GetObject("btnAdd.Font")));
			this.btnAdd.Image = ((System.Drawing.Image)(resources.GetObject("btnAdd.Image")));
			this.btnAdd.ImageAlign = ((System.Drawing.ContentAlignment)(resources.GetObject("btnAdd.ImageAlign")));
			this.btnAdd.ImageIndex = ((int)(resources.GetObject("btnAdd.ImageIndex")));
			this.btnAdd.ImeMode = ((System.Windows.Forms.ImeMode)(resources.GetObject("btnAdd.ImeMode")));
			this.btnAdd.Location = ((System.Drawing.Point)(resources.GetObject("btnAdd.Location")));
			this.btnAdd.Name = "btnAdd";
			this.btnAdd.RightToLeft = ((System.Windows.Forms.RightToLeft)(resources.GetObject("btnAdd.RightToLeft")));
			this.btnAdd.Size = ((System.Drawing.Size)(resources.GetObject("btnAdd.Size")));
			this.btnAdd.TabIndex = ((int)(resources.GetObject("btnAdd.TabIndex")));
			this.btnAdd.Text = resources.GetString("btnAdd.Text");
			this.btnAdd.TextAlign = ((System.Drawing.ContentAlignment)(resources.GetObject("btnAdd.TextAlign")));
			this.btnAdd.Visible = ((bool)(resources.GetObject("btnAdd.Visible")));
			this.btnAdd.Click += new System.EventHandler(this.btnAdd_Click);
			// 
			// btnCancel
			// 
			this.btnCancel.AccessibleDescription = resources.GetString("btnCancel.AccessibleDescription");
			this.btnCancel.AccessibleName = resources.GetString("btnCancel.AccessibleName");
			this.btnCancel.Anchor = ((System.Windows.Forms.AnchorStyles)(resources.GetObject("btnCancel.Anchor")));
			this.btnCancel.BackgroundImage = ((System.Drawing.Image)(resources.GetObject("btnCancel.BackgroundImage")));
			this.btnCancel.DialogResult = System.Windows.Forms.DialogResult.Cancel;
			this.btnCancel.Dock = ((System.Windows.Forms.DockStyle)(resources.GetObject("btnCancel.Dock")));
			this.btnCancel.Enabled = ((bool)(resources.GetObject("btnCancel.Enabled")));
			this.btnCancel.FlatStyle = ((System.Windows.Forms.FlatStyle)(resources.GetObject("btnCancel.FlatStyle")));
			this.btnCancel.Font = ((System.Drawing.Font)(resources.GetObject("btnCancel.Font")));
			this.btnCancel.Image = ((System.Drawing.Image)(resources.GetObject("btnCancel.Image")));
			this.btnCancel.ImageAlign = ((System.Drawing.ContentAlignment)(resources.GetObject("btnCancel.ImageAlign")));
			this.btnCancel.ImageIndex = ((int)(resources.GetObject("btnCancel.ImageIndex")));
			this.btnCancel.ImeMode = ((System.Windows.Forms.ImeMode)(resources.GetObject("btnCancel.ImeMode")));
			this.btnCancel.Location = ((System.Drawing.Point)(resources.GetObject("btnCancel.Location")));
			this.btnCancel.Name = "btnCancel";
			this.btnCancel.RightToLeft = ((System.Windows.Forms.RightToLeft)(resources.GetObject("btnCancel.RightToLeft")));
			this.btnCancel.Size = ((System.Drawing.Size)(resources.GetObject("btnCancel.Size")));
			this.btnCancel.TabIndex = ((int)(resources.GetObject("btnCancel.TabIndex")));
			this.btnCancel.Text = resources.GetString("btnCancel.Text");
			this.btnCancel.TextAlign = ((System.Drawing.ContentAlignment)(resources.GetObject("btnCancel.TextAlign")));
			this.btnCancel.Visible = ((bool)(resources.GetObject("btnCancel.Visible")));
			// 
			// SolutionEditor
			// 
			this.AccessibleDescription = resources.GetString("$this.AccessibleDescription");
			this.AccessibleName = resources.GetString("$this.AccessibleName");
			this.AutoScaleBaseSize = ((System.Drawing.Size)(resources.GetObject("$this.AutoScaleBaseSize")));
			this.AutoScroll = ((bool)(resources.GetObject("$this.AutoScroll")));
			this.AutoScrollMargin = ((System.Drawing.Size)(resources.GetObject("$this.AutoScrollMargin")));
			this.AutoScrollMinSize = ((System.Drawing.Size)(resources.GetObject("$this.AutoScrollMinSize")));
			this.BackgroundImage = ((System.Drawing.Image)(resources.GetObject("$this.BackgroundImage")));
			this.ClientSize = ((System.Drawing.Size)(resources.GetObject("$this.ClientSize")));
			this.Controls.Add(this.btnOK);
			this.Controls.Add(this.label1);
			this.Controls.Add(this.dgProjects);
			this.Controls.Add(this.btnAdd);
			this.Controls.Add(this.btnCancel);
			this.Enabled = ((bool)(resources.GetObject("$this.Enabled")));
			this.Font = ((System.Drawing.Font)(resources.GetObject("$this.Font")));
			this.Icon = ((System.Drawing.Icon)(resources.GetObject("$this.Icon")));
			this.ImeMode = ((System.Windows.Forms.ImeMode)(resources.GetObject("$this.ImeMode")));
			this.Location = ((System.Drawing.Point)(resources.GetObject("$this.Location")));
			this.MaximumSize = ((System.Drawing.Size)(resources.GetObject("$this.MaximumSize")));
			this.MinimumSize = ((System.Drawing.Size)(resources.GetObject("$this.MinimumSize")));
			this.Name = "SolutionEditor";
			this.RightToLeft = ((System.Windows.Forms.RightToLeft)(resources.GetObject("$this.RightToLeft")));
			this.ShowInTaskbar = false;
			this.StartPosition = ((System.Windows.Forms.FormStartPosition)(resources.GetObject("$this.StartPosition")));
			this.Text = resources.GetString("$this.Text");
			this.Load += new System.EventHandler(this.SolutionEditor_Load);
			((System.ComponentModel.ISupportInitialize)(this.dgProjects)).EndInit();
			((System.ComponentModel.ISupportInitialize)(this.dwProjects)).EndInit();
			((System.ComponentModel.ISupportInitialize)(this.dtProjects)).EndInit();
			((System.ComponentModel.ISupportInitialize)(this.dsProjects)).EndInit();
			this.ResumeLayout(false);

		}
		#endregion

		#region Private methods
		//---------------------------------------------------------------------
		private void SolutionEditor_Load(object sender, System.EventArgs e)
		{
			foreach (string project in solutionDocument.ReadProjects())
				AddProject(project);

			dataGridTextBoxColumnPath.TextBox.ContextMenu = ProjectContextMenu;
		}

		//---------------------------------------------------------------------
		private void btnAdd_Click(object sender, System.EventArgs e)
		{
			OpenFileDialog fileDialog	= new OpenFileDialog();
			fileDialog.Filter			= AllStrings.FILTERPRJ;
			fileDialog.InitialDirectory = Directory.GetCurrentDirectory();
			fileDialog.Title			= Strings.AddProjectCaption;

			if (fileDialog.ShowDialog(this) != DialogResult.OK)
				return;

			string		sourceFilePath	= fileDialog.FileName;
			string		sourceFileFolder = Path.GetDirectoryName(sourceFilePath);
			/*A questo punto si deve verificare che tipo di file è stato aggiunto: 
			 * se si tratta di un cs o tblprj posso proseguire, se si tratta di un 
			 * module.config devo verificare la presenza di quanti e quali file di 
			 * progetto vc o cs ci sono associati(più di uno, tra l'altro ad un 
			 * livello più profondo, solo in caso di split di library c++), 
			 * se si tratta di un vcproj devo verificare la presenza di un 
			 * module.config ad esso fratello.
			*/

			ProjectDocument.ProjectType extensionType;

			ArrayList projectPaths = DataDocumentFunctions.GetProjectFiles(sourceFileFolder, out extensionType);

			if (projectPaths == null || projectPaths.Count == 0)
			{
				MessageBox.Show(this, Strings.ProjectNotValid, Strings.WarningCaption);
				return;
			}

			string newProjectName = CommonFunctions.GetProjectName(sourceFilePath);

			foreach (DataRow r in dtProjects.Rows)
			{
				string existingProjectName = CommonFunctions.GetProjectName(r[dataColumnPath.ColumnName] as string);

				//non si possono aggiungere due progetti con lo stesso nome anche se diversi.
				if (string.Compare(existingProjectName, newProjectName, true) == 0)
				{
					MessageBox.Show(this, Strings.RepeatedProject, Strings.WarningCaption);
					return;
				}
			}

			AddProject(sourceFilePath);
		}

		//---------------------------------------------------------------------
		private void AddProject(string path)
		{
			DataRow r = dtProjects.NewRow();
			r[dataColumnPath.ColumnName] = path;
			dtProjects.Rows.Add(r);	
		}

		//---------------------------------------------------------------------
		private void MIEnvironment_Click(object sender, System.EventArgs e)
		{
			CommonFunctions.InsertEnvironmentVariable(dataGridTextBoxColumnPath.TextBox, this);
			dgProjects[dgProjects.CurrentCell] = dataGridTextBoxColumnPath.TextBox.Text;
		}

		

		//---------------------------------------------------------------------
		private void Projects_RowChanging(object sender, DataRowChangeEventArgs e)
		{
			if (e.Action != DataRowAction.Change)
				return;

			string path = e.Row[dataColumnPath.ColumnName] as string;

			path = CommonFunctions.LogicalPathToPhysicalPath(path);

			if (!File.Exists(path))
				throw new Exception(string.Format(Strings.FileNotFound, path));
		}
		#endregion
	}
}
