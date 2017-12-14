using System;
using System.Collections;
using System.ComponentModel;
using System.Drawing;
using System.Windows.Forms;
using Microarea.Tools.TBLocalizer.CommonUtilities;

namespace Microarea.Tools.TBLocalizer.Forms
{
	/// <summary>
	///Permette di scegliere le cartelle per i file inclusi
	/// da utilizzarsi durante il parsing.
	/// </summary>
	//=========================================================================
	public class DirectoriesSpecifier : Form
	{
		#region Controls

		private Button		BtnSave;
		private Button		BtnCancel;
		private Button 		BtnAdd;
		private Button 		BtnRemove;
		private Button 		BtnModify;
		private Button 		BtnUp;
		private Button 		BtnDown;
		private Label LblMessages;
		#endregion
		private IContainer components;

		#region Private members
		private bool		saved = true;
		private DataGrid DgDir;
		private BindingSource DirectoryPathBindingSource;
		private DataGridTableStyle dataGridTableStyle1;
		private MyAutoSizeTextColumnStyle dataGridTextBoxColumn1;
		private ContextMenu contextMenu;
		private MenuItem MIEnvironment;
		private string[]	dirs;
		#endregion

		#region Public properties
		public  string[] Dirs { get { return dirs; } set { dirs = value; } }

		public IList Items { get { return DirectoryPathBindingSource; } }
		public DirectoryPath SelectedItem { get { return (DirectoryPath) DirectoryPathBindingSource.Current; } }
		public int SelectedIndex { get { return DirectoryPathBindingSource.Position; } set { } }
		#endregion

		#region Constructors
		//---------------------------------------------------------------------
		public DirectoriesSpecifier()
		{
			InitializeComponent();
			PostInitializeComponent();
		}
		#endregion

		#region Private methods
		//---------------------------------------------------------------------
		private void PostInitializeComponent()
		{
			this.Closing += new CancelEventHandler(this.DirectoriesSpecifier_Cancel);
		}

		//---------------------------------------------------------------------
		internal void Fill () 
		{
			Items.Clear();
			foreach (string s in Dirs)
				Items.Add(new DirectoryPath(s));
		}

		//cancella eventualmente l'evento del closing
		//---------------------------------------------------------------------
		private void DirectoriesSpecifier_Cancel (object sender, CancelEventArgs e) 
		{
			e.Cancel = (!AskSaving());
		}

		//Chiedi salvataggio
		//---------------------------------------------------------------------
		private bool AskSaving()
		{
			if (saved)
				return true;

			DialogResult result =
				MessageBox.Show
				(
					DictionaryCreator.ActiveForm, 
					Strings.SaveDirectoriesQuestion, 
					Strings.MainFormCaption, 
					MessageBoxButtons.YesNoCancel, 
					MessageBoxIcon.Question, 
					MessageBoxDefaultButton.Button1
				);

			//cancel: niente
			//no: chiudo la solution corrente senza salvare
			//si: salvo e chiudo la solution corrente
			if (result == DialogResult.Cancel)
				return false;

			if (result == DialogResult.Yes)
				SaveDirs();

			return true;
		}

		//Rimuove gli items selezionati
		//---------------------------------------------------------------------
		private void BtnRemove_Click(object sender, System.EventArgs e)
		{
			LblMessages.Text = String.Empty;
			if (SelectedItem != null)
			{
				Items.RemoveAt(SelectedIndex);
				saved = false;
			}
		}

		//chiude senza salvare le ultime modifiche
		//---------------------------------------------------------------------
		private void BtnCancel_Click(object sender, System.EventArgs e)
		{
			DialogResult = DialogResult.Cancel;
			this.Close();
		}

		//chiude e salva le modifiche
		//---------------------------------------------------------------------
		private void BtnSave_Click(object sender, System.EventArgs e)
		{
			if (Items.Count == 0)
			{
				MessageBox.Show(DictionaryCreator.ActiveForm, Strings.NoSelectedDirectories , Strings.WarningCaption);
				return;
			}
			DialogResult = DialogResult.OK;
			SaveDirs();
			saved = true;
			Close();
		}

		//chiude e salva le modifiche
		//---------------------------------------------------------------------
		private void SaveDirs()
		{
			Dirs = new string[Items.Count];
			for (int i = 0; i < Items.Count; i++)
				Dirs[i] = ((DirectoryPath)Items[i]).Path;
		}

		//per modificare l'item selezionato, apre la dialog
		//---------------------------------------------------------------------
		private void BtnModify_Click(object sender, System.EventArgs e)
		{
			LblMessages.Text = String.Empty;
			if (SelectedItem == null)	return;
				
			string toModify  = CommonFunctions.LogicalPathToPhysicalPath(SelectedItem.ToString());
			int index		 = SelectedIndex;
			string selection = OpenDialog(toModify);
			if (selection != String.Empty)
			{
				Items[index] = new DirectoryPath(CommonFunctions.PhysicalPathToLogicalPath(selection));
				saved = false;
			}
		}

		//apre la dialog per la scelta della directory
		//---------------------------------------------------------------------
		private void BtnAdd_Click(object sender, System.EventArgs e)
		{	
			LblMessages.Text = String.Empty;
			string selection = OpenDialog(String.Empty);
			DirectoryPath newPath = new DirectoryPath(CommonFunctions.PhysicalPathToLogicalPath(selection));
			if (Items.Contains(newPath))
			{
				LblMessages.Text = Strings.RepeatedEntry;
				return;
			}

			if (selection != String.Empty)
			{
				Items.Add(newPath);
				saved = false;
			}
		}

		//sposta in alto l'item selezionato
		//---------------------------------------------------------------------
		private void BtnUp_Click(object sender, System.EventArgs e)
		{
			LblMessages.Text = String.Empty;
			if (SelectedItem != null && SelectedIndex != 0)
				MoveItem(+1);
		}

		//sposta in basso l'item selezionato
		//---------------------------------------------------------------------
		private void BtnDown_Click(object sender, System.EventArgs e)
		{
			LblMessages.Text = String.Empty;
			if (SelectedItem != null && SelectedIndex != (Items.Count-1))
				MoveItem(-1);
		}

		//sposta in basso  o in alto l'item selezionato
		//---------------------------------------------------------------------
		private void MoveItem(int i)
		{
			int index = SelectedIndex;
			int newIndex = index - i;
			object swap				= SelectedItem;
			Items[index] = Items[newIndex];
			Items[newIndex] = swap;
			((BindingSource)Items).Position = newIndex;
			saved = false;
		}

		//apre la dialog
		//---------------------------------------------------------------------
		private string OpenDialog(string selected)
		{
			FolderBrowserDialog folderDialog = new FolderBrowserDialog();
			folderDialog.ShowNewFolderButton = false;
			folderDialog.SelectedPath = selected;
			if (folderDialog.ShowDialog(this) == DialogResult.OK)
				return  folderDialog.SelectedPath;

			return string.Empty;
		}
		#endregion

		//---------------------------------------------------------------------
		/// <summary>
		/// Clean up any resources being used.
		/// </summary>
		protected override void Dispose( bool disposing )
		{
			if ( disposing )
			{
				if (components != null)
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
		private void InitializeComponent()
		{
			this.components = new System.ComponentModel.Container();
			System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(DirectoriesSpecifier));
			this.BtnSave = new System.Windows.Forms.Button();
			this.BtnCancel = new System.Windows.Forms.Button();
			this.BtnAdd = new System.Windows.Forms.Button();
			this.BtnRemove = new System.Windows.Forms.Button();
			this.BtnModify = new System.Windows.Forms.Button();
			this.BtnUp = new System.Windows.Forms.Button();
			this.BtnDown = new System.Windows.Forms.Button();
			this.LblMessages = new System.Windows.Forms.Label();
			this.DgDir = new System.Windows.Forms.DataGrid();
			this.DirectoryPathBindingSource = new System.Windows.Forms.BindingSource(this.components);
			this.dataGridTableStyle1 = new System.Windows.Forms.DataGridTableStyle();
			this.dataGridTextBoxColumn1 = new Microarea.Tools.TBLocalizer.Forms.MyAutoSizeTextColumnStyle();
			this.contextMenu = new System.Windows.Forms.ContextMenu();
			this.MIEnvironment = new System.Windows.Forms.MenuItem();
			((System.ComponentModel.ISupportInitialize)(this.DgDir)).BeginInit();
			((System.ComponentModel.ISupportInitialize)(this.DirectoryPathBindingSource)).BeginInit();
			this.SuspendLayout();
			// 
			// BtnSave
			// 
			resources.ApplyResources(this.BtnSave, "BtnSave");
			this.BtnSave.Name = "BtnSave";
			this.BtnSave.Click += new System.EventHandler(this.BtnSave_Click);
			// 
			// BtnCancel
			// 
			resources.ApplyResources(this.BtnCancel, "BtnCancel");
			this.BtnCancel.Name = "BtnCancel";
			this.BtnCancel.Click += new System.EventHandler(this.BtnCancel_Click);
			// 
			// BtnAdd
			// 
			resources.ApplyResources(this.BtnAdd, "BtnAdd");
			this.BtnAdd.Name = "BtnAdd";
			this.BtnAdd.Click += new System.EventHandler(this.BtnAdd_Click);
			// 
			// BtnRemove
			// 
			resources.ApplyResources(this.BtnRemove, "BtnRemove");
			this.BtnRemove.Name = "BtnRemove";
			this.BtnRemove.Click += new System.EventHandler(this.BtnRemove_Click);
			// 
			// BtnModify
			// 
			resources.ApplyResources(this.BtnModify, "BtnModify");
			this.BtnModify.Name = "BtnModify";
			this.BtnModify.Click += new System.EventHandler(this.BtnModify_Click);
			// 
			// BtnUp
			// 
			resources.ApplyResources(this.BtnUp, "BtnUp");
			this.BtnUp.Name = "BtnUp";
			this.BtnUp.Click += new System.EventHandler(this.BtnUp_Click);
			// 
			// BtnDown
			// 
			resources.ApplyResources(this.BtnDown, "BtnDown");
			this.BtnDown.Name = "BtnDown";
			this.BtnDown.Click += new System.EventHandler(this.BtnDown_Click);
			// 
			// LblMessages
			// 
			resources.ApplyResources(this.LblMessages, "LblMessages");
			this.LblMessages.FlatStyle = System.Windows.Forms.FlatStyle.System;
			this.LblMessages.ForeColor = System.Drawing.Color.Red;
			this.LblMessages.Name = "LblMessages";
			// 
			// DgDir
			// 
			resources.ApplyResources(this.DgDir, "DgDir");
			this.DgDir.DataMember = "";
			this.DgDir.DataSource = this.DirectoryPathBindingSource;
			this.DgDir.HeaderForeColor = System.Drawing.SystemColors.ControlText;
			this.DgDir.Name = "DgDir";
			this.DgDir.TableStyles.AddRange(new System.Windows.Forms.DataGridTableStyle[] {
            this.dataGridTableStyle1});
			// 
			// DirectoryPathBindingSource
			// 
			this.DirectoryPathBindingSource.DataSource = typeof(Microarea.Tools.TBLocalizer.Forms.DirectoryPath);
			// 
			// dataGridTableStyle1
			// 
			this.dataGridTableStyle1.DataGrid = this.DgDir;
			this.dataGridTableStyle1.GridColumnStyles.AddRange(new System.Windows.Forms.DataGridColumnStyle[] {
            this.dataGridTextBoxColumn1});
			this.dataGridTableStyle1.HeaderForeColor = System.Drawing.SystemColors.ControlText;
			this.dataGridTableStyle1.MappingName = "DirectoryPath";
			// 
			// dataGridTextBoxColumn1
			// 
			this.dataGridTextBoxColumn1.Format = "";
			this.dataGridTextBoxColumn1.FormatInfo = null;
			resources.ApplyResources(this.dataGridTextBoxColumn1, "dataGridTextBoxColumn1");
			// 
			// ContextMenu
			// 
			this.contextMenu.MenuItems.AddRange(new System.Windows.Forms.MenuItem[] {
            this.MIEnvironment});
			// 
			// MIEnvironment
			// 
			this.MIEnvironment.Index = 0;
			resources.ApplyResources(this.MIEnvironment, "MIEnvironment");
			this.MIEnvironment.Click += new System.EventHandler(this.MIEnvironment_Click);
			// 
			// DirectoriesSpecifier
			// 
			resources.ApplyResources(this, "$this");
			this.Controls.Add(this.DgDir);
			this.Controls.Add(this.LblMessages);
			this.Controls.Add(this.BtnDown);
			this.Controls.Add(this.BtnUp);
			this.Controls.Add(this.BtnModify);
			this.Controls.Add(this.BtnRemove);
			this.Controls.Add(this.BtnAdd);
			this.Controls.Add(this.BtnCancel);
			this.Controls.Add(this.BtnSave);
			this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedToolWindow;
			this.Name = "DirectoriesSpecifier";
			this.ShowInTaskbar = false;
			this.Load += new System.EventHandler(this.DirectoriesSpecifier_Load);
			((System.ComponentModel.ISupportInitialize)(this.DgDir)).EndInit();
			((System.ComponentModel.ISupportInitialize)(this.DirectoryPathBindingSource)).EndInit();
			this.ResumeLayout(false);

		}
		#endregion

		private void DirectoriesSpecifier_Load(object sender, EventArgs e)
		{
			dataGridTextBoxColumn1.TextBox.ContextMenu = this.contextMenu;
		}

		private void MIEnvironment_Click(object sender, EventArgs e)
		{
			CommonFunctions.InsertEnvironmentVariable(dataGridTextBoxColumn1.TextBox, this);
			DgDir[DgDir.CurrentCell] = dataGridTextBoxColumn1.TextBox.Text;
		}
	}

	public class DirectoryPath
	{
		public DirectoryPath(string path) { this.Path = path;  }
		public string Path { get; set; }
		public static implicit operator string (DirectoryPath dp ){ return dp.Path; }

		public override bool Equals(object obj)
		{
			return Path.Equals(((DirectoryPath)obj).Path, StringComparison.InvariantCultureIgnoreCase);
		}

		public override int GetHashCode()
		{
			return Path.GetHashCode();
		}

		public override string ToString()
		{
			return Path;
		}
	}

	//================================================================================
	class MyAutoSizeTextColumnStyle : DataGridTextBoxColumn
	{
		//--------------------------------------------------------------------------------
		protected override void Paint(Graphics g, Rectangle bounds, CurrencyManager source, int rowNum, Brush backBrush, Brush foreBrush, bool alignToRight)
		{
			DirectoryPath row = (DirectoryPath)source.List[rowNum];

			string text = row.Path;
			SizeF size = g.MeasureString(text, this.DataGridTableStyle.DataGrid.Font);
			int actualWidth = Convert.ToInt32(size.Width) + 5;
			if (actualWidth > Width)
				Width = actualWidth;

			DataGridRowHeightSetter setter = new DataGridRowHeightSetter(DataGridTableStyle.DataGrid);
			int actualHeigth = Convert.ToInt32(size.Height) + 5;
			if (actualHeigth > setter[rowNum])
				setter[rowNum] = actualHeigth;

			base.Paint(g, bounds, source, rowNum, backBrush, foreBrush, alignToRight);
		}
	}
}
