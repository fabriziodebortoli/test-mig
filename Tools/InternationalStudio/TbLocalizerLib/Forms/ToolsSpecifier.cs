using System.Collections;
using System.ComponentModel;
using System.Globalization;
using System.Windows.Forms;

namespace Microarea.Tools.TBLocalizer.Forms
{
	/// <summary>
	/// Permette di specificare il percorso di un eseguibile o di un
	/// indirizzo internet.
	/// </summary>
	//=========================================================================
	public class ToolsSpecifier : Form
	{
		#region Controls
		private Label			LblInfo;
		private Label			LblUrl;
		private Label			LblName;
		private Label LblArgs;
		private ColumnHeader	ColumnUrl;
		private ColumnHeader	ColumnName;
		private ColumnHeader ColumnArgs;
		private Button			BtnAdd;
		private Button			BtnRemove;
		private Button			BtnCancel;
		private Button			BtnOk;
		private TextBox			TxtUrl;
		private TextBox			TxtName;
		private TextBox TxtArgs;
		private ListView		ListTools;

		private Container components = null;
		#endregion

		#region Public members
		public ArrayList ToolsInfo = new ArrayList();
		#endregion

		#region Constructors
		//---------------------------------------------------------------------
		public ToolsSpecifier(ArrayList toolsInfo)
		{
			InitializeComponent();
			ToolsInfo = toolsInfo;

			if (ToolsInfo != null && ToolsInfo.Count > 0)
				FillList();
		}
		#endregion

		#region Private methods
		//---------------------------------------------------------------------
		private void FillList()
		{
			foreach (ToolInfo ti in ToolsInfo)
				AddItem(ti, false);
		}
		
		//---------------------------------------------------------------------
		private bool AddItem(ToolInfo ti, bool addToList)
		{
			if (ToolsInfo == null)
				ToolsInfo = new ArrayList();

			if (addToList && ToolsInfo.Contains(ti)) 
				return false;

			ListViewItem item = new ListViewItem(ti.Name); 
			item.Tag = ti;
			item.SubItems.AddRange(new string[]{ti.Url, ti.Args}); 
			ListTools.Items.Add(item); 

			if (addToList)
				ToolsInfo.Add(ti);

			return true;
		}

		//---------------------------------------------------------------------
		private void BtnAdd_Click(object sender, System.EventArgs e)
		{
			string url	= TxtUrl.Text;
			string name = TxtName.Text;
			string args = TxtArgs.Text;

			if (url  == null || url.Length == 0 || 
				name == null || name.Length == 0)
				return;

			if (AddItem(new ToolInfo(url, name, args), true))
				TxtName.Text = TxtUrl.Text = TxtArgs.Text = string.Empty;
		}

		//---------------------------------------------------------------------	
		private void BtnRemove_Click(object sender, System.EventArgs e)
		{
			if (ListTools.SelectedItems == null || ListTools.SelectedItems.Count == 0)
				return;

			ListViewItem i = ListTools.SelectedItems[0];
			ListTools.Items.Remove(i);
			ToolsInfo.Remove(i.Tag as ToolInfo);
		}
		#endregion

		/// <summary>
		/// Clean up any resources being used.
		/// </summary>
		//---------------------------------------------------------------------
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
			System.Resources.ResourceManager resources = new System.Resources.ResourceManager(typeof(ToolsSpecifier));
			this.LblInfo = new System.Windows.Forms.Label();
			this.BtnAdd = new System.Windows.Forms.Button();
			this.BtnRemove = new System.Windows.Forms.Button();
			this.BtnCancel = new System.Windows.Forms.Button();
			this.BtnOk = new System.Windows.Forms.Button();
			this.LblUrl = new System.Windows.Forms.Label();
			this.LblName = new System.Windows.Forms.Label();
			this.TxtUrl = new System.Windows.Forms.TextBox();
			this.TxtName = new System.Windows.Forms.TextBox();
			this.ListTools = new System.Windows.Forms.ListView();
			this.ColumnName = new System.Windows.Forms.ColumnHeader();
			this.ColumnUrl = new System.Windows.Forms.ColumnHeader();
			this.ColumnArgs = new System.Windows.Forms.ColumnHeader();
			this.TxtArgs = new System.Windows.Forms.TextBox();
			this.LblArgs = new System.Windows.Forms.Label();
			this.SuspendLayout();
			// 
			// LblInfo
			// 
			this.LblInfo.AccessibleDescription = resources.GetString("LblInfo.AccessibleDescription");
			this.LblInfo.AccessibleName = resources.GetString("LblInfo.AccessibleName");
			this.LblInfo.Anchor = ((System.Windows.Forms.AnchorStyles)(resources.GetObject("LblInfo.Anchor")));
			this.LblInfo.AutoSize = ((bool)(resources.GetObject("LblInfo.AutoSize")));
			this.LblInfo.Dock = ((System.Windows.Forms.DockStyle)(resources.GetObject("LblInfo.Dock")));
			this.LblInfo.Enabled = ((bool)(resources.GetObject("LblInfo.Enabled")));
			this.LblInfo.Font = ((System.Drawing.Font)(resources.GetObject("LblInfo.Font")));
			this.LblInfo.Image = ((System.Drawing.Image)(resources.GetObject("LblInfo.Image")));
			this.LblInfo.ImageAlign = ((System.Drawing.ContentAlignment)(resources.GetObject("LblInfo.ImageAlign")));
			this.LblInfo.ImageIndex = ((int)(resources.GetObject("LblInfo.ImageIndex")));
			this.LblInfo.ImeMode = ((System.Windows.Forms.ImeMode)(resources.GetObject("LblInfo.ImeMode")));
			this.LblInfo.Location = ((System.Drawing.Point)(resources.GetObject("LblInfo.Location")));
			this.LblInfo.Name = "LblInfo";
			this.LblInfo.RightToLeft = ((System.Windows.Forms.RightToLeft)(resources.GetObject("LblInfo.RightToLeft")));
			this.LblInfo.Size = ((System.Drawing.Size)(resources.GetObject("LblInfo.Size")));
			this.LblInfo.TabIndex = ((int)(resources.GetObject("LblInfo.TabIndex")));
			this.LblInfo.Text = resources.GetString("LblInfo.Text");
			this.LblInfo.TextAlign = ((System.Drawing.ContentAlignment)(resources.GetObject("LblInfo.TextAlign")));
			this.LblInfo.Visible = ((bool)(resources.GetObject("LblInfo.Visible")));
			// 
			// BtnAdd
			// 
			this.BtnAdd.AccessibleDescription = resources.GetString("BtnAdd.AccessibleDescription");
			this.BtnAdd.AccessibleName = resources.GetString("BtnAdd.AccessibleName");
			this.BtnAdd.Anchor = ((System.Windows.Forms.AnchorStyles)(resources.GetObject("BtnAdd.Anchor")));
			this.BtnAdd.BackgroundImage = ((System.Drawing.Image)(resources.GetObject("BtnAdd.BackgroundImage")));
			this.BtnAdd.Dock = ((System.Windows.Forms.DockStyle)(resources.GetObject("BtnAdd.Dock")));
			this.BtnAdd.Enabled = ((bool)(resources.GetObject("BtnAdd.Enabled")));
			this.BtnAdd.FlatStyle = ((System.Windows.Forms.FlatStyle)(resources.GetObject("BtnAdd.FlatStyle")));
			this.BtnAdd.Font = ((System.Drawing.Font)(resources.GetObject("BtnAdd.Font")));
			this.BtnAdd.Image = ((System.Drawing.Image)(resources.GetObject("BtnAdd.Image")));
			this.BtnAdd.ImageAlign = ((System.Drawing.ContentAlignment)(resources.GetObject("BtnAdd.ImageAlign")));
			this.BtnAdd.ImageIndex = ((int)(resources.GetObject("BtnAdd.ImageIndex")));
			this.BtnAdd.ImeMode = ((System.Windows.Forms.ImeMode)(resources.GetObject("BtnAdd.ImeMode")));
			this.BtnAdd.Location = ((System.Drawing.Point)(resources.GetObject("BtnAdd.Location")));
			this.BtnAdd.Name = "BtnAdd";
			this.BtnAdd.RightToLeft = ((System.Windows.Forms.RightToLeft)(resources.GetObject("BtnAdd.RightToLeft")));
			this.BtnAdd.Size = ((System.Drawing.Size)(resources.GetObject("BtnAdd.Size")));
			this.BtnAdd.TabIndex = ((int)(resources.GetObject("BtnAdd.TabIndex")));
			this.BtnAdd.Text = resources.GetString("BtnAdd.Text");
			this.BtnAdd.TextAlign = ((System.Drawing.ContentAlignment)(resources.GetObject("BtnAdd.TextAlign")));
			this.BtnAdd.Visible = ((bool)(resources.GetObject("BtnAdd.Visible")));
			this.BtnAdd.Click += new System.EventHandler(this.BtnAdd_Click);
			// 
			// BtnRemove
			// 
			this.BtnRemove.AccessibleDescription = resources.GetString("BtnRemove.AccessibleDescription");
			this.BtnRemove.AccessibleName = resources.GetString("BtnRemove.AccessibleName");
			this.BtnRemove.Anchor = ((System.Windows.Forms.AnchorStyles)(resources.GetObject("BtnRemove.Anchor")));
			this.BtnRemove.BackgroundImage = ((System.Drawing.Image)(resources.GetObject("BtnRemove.BackgroundImage")));
			this.BtnRemove.Dock = ((System.Windows.Forms.DockStyle)(resources.GetObject("BtnRemove.Dock")));
			this.BtnRemove.Enabled = ((bool)(resources.GetObject("BtnRemove.Enabled")));
			this.BtnRemove.FlatStyle = ((System.Windows.Forms.FlatStyle)(resources.GetObject("BtnRemove.FlatStyle")));
			this.BtnRemove.Font = ((System.Drawing.Font)(resources.GetObject("BtnRemove.Font")));
			this.BtnRemove.Image = ((System.Drawing.Image)(resources.GetObject("BtnRemove.Image")));
			this.BtnRemove.ImageAlign = ((System.Drawing.ContentAlignment)(resources.GetObject("BtnRemove.ImageAlign")));
			this.BtnRemove.ImageIndex = ((int)(resources.GetObject("BtnRemove.ImageIndex")));
			this.BtnRemove.ImeMode = ((System.Windows.Forms.ImeMode)(resources.GetObject("BtnRemove.ImeMode")));
			this.BtnRemove.Location = ((System.Drawing.Point)(resources.GetObject("BtnRemove.Location")));
			this.BtnRemove.Name = "BtnRemove";
			this.BtnRemove.RightToLeft = ((System.Windows.Forms.RightToLeft)(resources.GetObject("BtnRemove.RightToLeft")));
			this.BtnRemove.Size = ((System.Drawing.Size)(resources.GetObject("BtnRemove.Size")));
			this.BtnRemove.TabIndex = ((int)(resources.GetObject("BtnRemove.TabIndex")));
			this.BtnRemove.Text = resources.GetString("BtnRemove.Text");
			this.BtnRemove.TextAlign = ((System.Drawing.ContentAlignment)(resources.GetObject("BtnRemove.TextAlign")));
			this.BtnRemove.Visible = ((bool)(resources.GetObject("BtnRemove.Visible")));
			this.BtnRemove.Click += new System.EventHandler(this.BtnRemove_Click);
			// 
			// BtnCancel
			// 
			this.BtnCancel.AccessibleDescription = resources.GetString("BtnCancel.AccessibleDescription");
			this.BtnCancel.AccessibleName = resources.GetString("BtnCancel.AccessibleName");
			this.BtnCancel.Anchor = ((System.Windows.Forms.AnchorStyles)(resources.GetObject("BtnCancel.Anchor")));
			this.BtnCancel.BackgroundImage = ((System.Drawing.Image)(resources.GetObject("BtnCancel.BackgroundImage")));
			this.BtnCancel.DialogResult = System.Windows.Forms.DialogResult.Cancel;
			this.BtnCancel.Dock = ((System.Windows.Forms.DockStyle)(resources.GetObject("BtnCancel.Dock")));
			this.BtnCancel.Enabled = ((bool)(resources.GetObject("BtnCancel.Enabled")));
			this.BtnCancel.FlatStyle = ((System.Windows.Forms.FlatStyle)(resources.GetObject("BtnCancel.FlatStyle")));
			this.BtnCancel.Font = ((System.Drawing.Font)(resources.GetObject("BtnCancel.Font")));
			this.BtnCancel.Image = ((System.Drawing.Image)(resources.GetObject("BtnCancel.Image")));
			this.BtnCancel.ImageAlign = ((System.Drawing.ContentAlignment)(resources.GetObject("BtnCancel.ImageAlign")));
			this.BtnCancel.ImageIndex = ((int)(resources.GetObject("BtnCancel.ImageIndex")));
			this.BtnCancel.ImeMode = ((System.Windows.Forms.ImeMode)(resources.GetObject("BtnCancel.ImeMode")));
			this.BtnCancel.Location = ((System.Drawing.Point)(resources.GetObject("BtnCancel.Location")));
			this.BtnCancel.Name = "BtnCancel";
			this.BtnCancel.RightToLeft = ((System.Windows.Forms.RightToLeft)(resources.GetObject("BtnCancel.RightToLeft")));
			this.BtnCancel.Size = ((System.Drawing.Size)(resources.GetObject("BtnCancel.Size")));
			this.BtnCancel.TabIndex = ((int)(resources.GetObject("BtnCancel.TabIndex")));
			this.BtnCancel.Text = resources.GetString("BtnCancel.Text");
			this.BtnCancel.TextAlign = ((System.Drawing.ContentAlignment)(resources.GetObject("BtnCancel.TextAlign")));
			this.BtnCancel.Visible = ((bool)(resources.GetObject("BtnCancel.Visible")));
			// 
			// BtnOk
			// 
			this.BtnOk.AccessibleDescription = resources.GetString("BtnOk.AccessibleDescription");
			this.BtnOk.AccessibleName = resources.GetString("BtnOk.AccessibleName");
			this.BtnOk.Anchor = ((System.Windows.Forms.AnchorStyles)(resources.GetObject("BtnOk.Anchor")));
			this.BtnOk.BackgroundImage = ((System.Drawing.Image)(resources.GetObject("BtnOk.BackgroundImage")));
			this.BtnOk.DialogResult = System.Windows.Forms.DialogResult.OK;
			this.BtnOk.Dock = ((System.Windows.Forms.DockStyle)(resources.GetObject("BtnOk.Dock")));
			this.BtnOk.Enabled = ((bool)(resources.GetObject("BtnOk.Enabled")));
			this.BtnOk.FlatStyle = ((System.Windows.Forms.FlatStyle)(resources.GetObject("BtnOk.FlatStyle")));
			this.BtnOk.Font = ((System.Drawing.Font)(resources.GetObject("BtnOk.Font")));
			this.BtnOk.Image = ((System.Drawing.Image)(resources.GetObject("BtnOk.Image")));
			this.BtnOk.ImageAlign = ((System.Drawing.ContentAlignment)(resources.GetObject("BtnOk.ImageAlign")));
			this.BtnOk.ImageIndex = ((int)(resources.GetObject("BtnOk.ImageIndex")));
			this.BtnOk.ImeMode = ((System.Windows.Forms.ImeMode)(resources.GetObject("BtnOk.ImeMode")));
			this.BtnOk.Location = ((System.Drawing.Point)(resources.GetObject("BtnOk.Location")));
			this.BtnOk.Name = "BtnOk";
			this.BtnOk.RightToLeft = ((System.Windows.Forms.RightToLeft)(resources.GetObject("BtnOk.RightToLeft")));
			this.BtnOk.Size = ((System.Drawing.Size)(resources.GetObject("BtnOk.Size")));
			this.BtnOk.TabIndex = ((int)(resources.GetObject("BtnOk.TabIndex")));
			this.BtnOk.Text = resources.GetString("BtnOk.Text");
			this.BtnOk.TextAlign = ((System.Drawing.ContentAlignment)(resources.GetObject("BtnOk.TextAlign")));
			this.BtnOk.Visible = ((bool)(resources.GetObject("BtnOk.Visible")));
			// 
			// LblUrl
			// 
			this.LblUrl.AccessibleDescription = resources.GetString("LblUrl.AccessibleDescription");
			this.LblUrl.AccessibleName = resources.GetString("LblUrl.AccessibleName");
			this.LblUrl.Anchor = ((System.Windows.Forms.AnchorStyles)(resources.GetObject("LblUrl.Anchor")));
			this.LblUrl.AutoSize = ((bool)(resources.GetObject("LblUrl.AutoSize")));
			this.LblUrl.Dock = ((System.Windows.Forms.DockStyle)(resources.GetObject("LblUrl.Dock")));
			this.LblUrl.Enabled = ((bool)(resources.GetObject("LblUrl.Enabled")));
			this.LblUrl.Font = ((System.Drawing.Font)(resources.GetObject("LblUrl.Font")));
			this.LblUrl.Image = ((System.Drawing.Image)(resources.GetObject("LblUrl.Image")));
			this.LblUrl.ImageAlign = ((System.Drawing.ContentAlignment)(resources.GetObject("LblUrl.ImageAlign")));
			this.LblUrl.ImageIndex = ((int)(resources.GetObject("LblUrl.ImageIndex")));
			this.LblUrl.ImeMode = ((System.Windows.Forms.ImeMode)(resources.GetObject("LblUrl.ImeMode")));
			this.LblUrl.Location = ((System.Drawing.Point)(resources.GetObject("LblUrl.Location")));
			this.LblUrl.Name = "LblUrl";
			this.LblUrl.RightToLeft = ((System.Windows.Forms.RightToLeft)(resources.GetObject("LblUrl.RightToLeft")));
			this.LblUrl.Size = ((System.Drawing.Size)(resources.GetObject("LblUrl.Size")));
			this.LblUrl.TabIndex = ((int)(resources.GetObject("LblUrl.TabIndex")));
			this.LblUrl.Text = resources.GetString("LblUrl.Text");
			this.LblUrl.TextAlign = ((System.Drawing.ContentAlignment)(resources.GetObject("LblUrl.TextAlign")));
			this.LblUrl.Visible = ((bool)(resources.GetObject("LblUrl.Visible")));
			// 
			// LblName
			// 
			this.LblName.AccessibleDescription = resources.GetString("LblName.AccessibleDescription");
			this.LblName.AccessibleName = resources.GetString("LblName.AccessibleName");
			this.LblName.Anchor = ((System.Windows.Forms.AnchorStyles)(resources.GetObject("LblName.Anchor")));
			this.LblName.AutoSize = ((bool)(resources.GetObject("LblName.AutoSize")));
			this.LblName.Dock = ((System.Windows.Forms.DockStyle)(resources.GetObject("LblName.Dock")));
			this.LblName.Enabled = ((bool)(resources.GetObject("LblName.Enabled")));
			this.LblName.Font = ((System.Drawing.Font)(resources.GetObject("LblName.Font")));
			this.LblName.Image = ((System.Drawing.Image)(resources.GetObject("LblName.Image")));
			this.LblName.ImageAlign = ((System.Drawing.ContentAlignment)(resources.GetObject("LblName.ImageAlign")));
			this.LblName.ImageIndex = ((int)(resources.GetObject("LblName.ImageIndex")));
			this.LblName.ImeMode = ((System.Windows.Forms.ImeMode)(resources.GetObject("LblName.ImeMode")));
			this.LblName.Location = ((System.Drawing.Point)(resources.GetObject("LblName.Location")));
			this.LblName.Name = "LblName";
			this.LblName.RightToLeft = ((System.Windows.Forms.RightToLeft)(resources.GetObject("LblName.RightToLeft")));
			this.LblName.Size = ((System.Drawing.Size)(resources.GetObject("LblName.Size")));
			this.LblName.TabIndex = ((int)(resources.GetObject("LblName.TabIndex")));
			this.LblName.Text = resources.GetString("LblName.Text");
			this.LblName.TextAlign = ((System.Drawing.ContentAlignment)(resources.GetObject("LblName.TextAlign")));
			this.LblName.Visible = ((bool)(resources.GetObject("LblName.Visible")));
			// 
			// TxtUrl
			// 
			this.TxtUrl.AccessibleDescription = resources.GetString("TxtUrl.AccessibleDescription");
			this.TxtUrl.AccessibleName = resources.GetString("TxtUrl.AccessibleName");
			this.TxtUrl.Anchor = ((System.Windows.Forms.AnchorStyles)(resources.GetObject("TxtUrl.Anchor")));
			this.TxtUrl.AutoSize = ((bool)(resources.GetObject("TxtUrl.AutoSize")));
			this.TxtUrl.BackgroundImage = ((System.Drawing.Image)(resources.GetObject("TxtUrl.BackgroundImage")));
			this.TxtUrl.Dock = ((System.Windows.Forms.DockStyle)(resources.GetObject("TxtUrl.Dock")));
			this.TxtUrl.Enabled = ((bool)(resources.GetObject("TxtUrl.Enabled")));
			this.TxtUrl.Font = ((System.Drawing.Font)(resources.GetObject("TxtUrl.Font")));
			this.TxtUrl.ImeMode = ((System.Windows.Forms.ImeMode)(resources.GetObject("TxtUrl.ImeMode")));
			this.TxtUrl.Location = ((System.Drawing.Point)(resources.GetObject("TxtUrl.Location")));
			this.TxtUrl.MaxLength = ((int)(resources.GetObject("TxtUrl.MaxLength")));
			this.TxtUrl.Multiline = ((bool)(resources.GetObject("TxtUrl.Multiline")));
			this.TxtUrl.Name = "TxtUrl";
			this.TxtUrl.PasswordChar = ((char)(resources.GetObject("TxtUrl.PasswordChar")));
			this.TxtUrl.RightToLeft = ((System.Windows.Forms.RightToLeft)(resources.GetObject("TxtUrl.RightToLeft")));
			this.TxtUrl.ScrollBars = ((System.Windows.Forms.ScrollBars)(resources.GetObject("TxtUrl.ScrollBars")));
			this.TxtUrl.Size = ((System.Drawing.Size)(resources.GetObject("TxtUrl.Size")));
			this.TxtUrl.TabIndex = ((int)(resources.GetObject("TxtUrl.TabIndex")));
			this.TxtUrl.Text = resources.GetString("TxtUrl.Text");
			this.TxtUrl.TextAlign = ((System.Windows.Forms.HorizontalAlignment)(resources.GetObject("TxtUrl.TextAlign")));
			this.TxtUrl.Visible = ((bool)(resources.GetObject("TxtUrl.Visible")));
			this.TxtUrl.WordWrap = ((bool)(resources.GetObject("TxtUrl.WordWrap")));
			// 
			// TxtName
			// 
			this.TxtName.AccessibleDescription = resources.GetString("TxtName.AccessibleDescription");
			this.TxtName.AccessibleName = resources.GetString("TxtName.AccessibleName");
			this.TxtName.Anchor = ((System.Windows.Forms.AnchorStyles)(resources.GetObject("TxtName.Anchor")));
			this.TxtName.AutoSize = ((bool)(resources.GetObject("TxtName.AutoSize")));
			this.TxtName.BackgroundImage = ((System.Drawing.Image)(resources.GetObject("TxtName.BackgroundImage")));
			this.TxtName.Dock = ((System.Windows.Forms.DockStyle)(resources.GetObject("TxtName.Dock")));
			this.TxtName.Enabled = ((bool)(resources.GetObject("TxtName.Enabled")));
			this.TxtName.Font = ((System.Drawing.Font)(resources.GetObject("TxtName.Font")));
			this.TxtName.ImeMode = ((System.Windows.Forms.ImeMode)(resources.GetObject("TxtName.ImeMode")));
			this.TxtName.Location = ((System.Drawing.Point)(resources.GetObject("TxtName.Location")));
			this.TxtName.MaxLength = ((int)(resources.GetObject("TxtName.MaxLength")));
			this.TxtName.Multiline = ((bool)(resources.GetObject("TxtName.Multiline")));
			this.TxtName.Name = "TxtName";
			this.TxtName.PasswordChar = ((char)(resources.GetObject("TxtName.PasswordChar")));
			this.TxtName.RightToLeft = ((System.Windows.Forms.RightToLeft)(resources.GetObject("TxtName.RightToLeft")));
			this.TxtName.ScrollBars = ((System.Windows.Forms.ScrollBars)(resources.GetObject("TxtName.ScrollBars")));
			this.TxtName.Size = ((System.Drawing.Size)(resources.GetObject("TxtName.Size")));
			this.TxtName.TabIndex = ((int)(resources.GetObject("TxtName.TabIndex")));
			this.TxtName.Text = resources.GetString("TxtName.Text");
			this.TxtName.TextAlign = ((System.Windows.Forms.HorizontalAlignment)(resources.GetObject("TxtName.TextAlign")));
			this.TxtName.Visible = ((bool)(resources.GetObject("TxtName.Visible")));
			this.TxtName.WordWrap = ((bool)(resources.GetObject("TxtName.WordWrap")));
			// 
			// ListTools
			// 
			this.ListTools.AccessibleDescription = resources.GetString("ListTools.AccessibleDescription");
			this.ListTools.AccessibleName = resources.GetString("ListTools.AccessibleName");
			this.ListTools.Alignment = ((System.Windows.Forms.ListViewAlignment)(resources.GetObject("ListTools.Alignment")));
			this.ListTools.Anchor = ((System.Windows.Forms.AnchorStyles)(resources.GetObject("ListTools.Anchor")));
			this.ListTools.BackgroundImage = ((System.Drawing.Image)(resources.GetObject("ListTools.BackgroundImage")));
			this.ListTools.Columns.AddRange(new System.Windows.Forms.ColumnHeader[] {
																						this.ColumnName,
																						this.ColumnUrl,
																						this.ColumnArgs});
			this.ListTools.Dock = ((System.Windows.Forms.DockStyle)(resources.GetObject("ListTools.Dock")));
			this.ListTools.Enabled = ((bool)(resources.GetObject("ListTools.Enabled")));
			this.ListTools.Font = ((System.Drawing.Font)(resources.GetObject("ListTools.Font")));
			this.ListTools.FullRowSelect = true;
			this.ListTools.HeaderStyle = System.Windows.Forms.ColumnHeaderStyle.Nonclickable;
			this.ListTools.ImeMode = ((System.Windows.Forms.ImeMode)(resources.GetObject("ListTools.ImeMode")));
			this.ListTools.LabelWrap = ((bool)(resources.GetObject("ListTools.LabelWrap")));
			this.ListTools.Location = ((System.Drawing.Point)(resources.GetObject("ListTools.Location")));
			this.ListTools.MultiSelect = false;
			this.ListTools.Name = "ListTools";
			this.ListTools.RightToLeft = ((System.Windows.Forms.RightToLeft)(resources.GetObject("ListTools.RightToLeft")));
			this.ListTools.Size = ((System.Drawing.Size)(resources.GetObject("ListTools.Size")));
			this.ListTools.TabIndex = ((int)(resources.GetObject("ListTools.TabIndex")));
			this.ListTools.Text = resources.GetString("ListTools.Text");
			this.ListTools.View = System.Windows.Forms.View.Details;
			this.ListTools.Visible = ((bool)(resources.GetObject("ListTools.Visible")));
			// 
			// ColumnName
			// 
			this.ColumnName.Text = resources.GetString("ColumnName.Text");
			this.ColumnName.TextAlign = ((System.Windows.Forms.HorizontalAlignment)(resources.GetObject("ColumnName.TextAlign")));
			this.ColumnName.Width = ((int)(resources.GetObject("ColumnName.Width")));
			// 
			// ColumnUrl
			// 
			this.ColumnUrl.Text = resources.GetString("ColumnUrl.Text");
			this.ColumnUrl.TextAlign = ((System.Windows.Forms.HorizontalAlignment)(resources.GetObject("ColumnUrl.TextAlign")));
			this.ColumnUrl.Width = ((int)(resources.GetObject("ColumnUrl.Width")));
			// 
			// ColumnArgs
			// 
			this.ColumnArgs.Text = resources.GetString("ColumnArgs.Text");
			this.ColumnArgs.TextAlign = ((System.Windows.Forms.HorizontalAlignment)(resources.GetObject("ColumnArgs.TextAlign")));
			this.ColumnArgs.Width = ((int)(resources.GetObject("ColumnArgs.Width")));
			// 
			// TxtArgs
			// 
			this.TxtArgs.AccessibleDescription = resources.GetString("TxtArgs.AccessibleDescription");
			this.TxtArgs.AccessibleName = resources.GetString("TxtArgs.AccessibleName");
			this.TxtArgs.Anchor = ((System.Windows.Forms.AnchorStyles)(resources.GetObject("TxtArgs.Anchor")));
			this.TxtArgs.AutoSize = ((bool)(resources.GetObject("TxtArgs.AutoSize")));
			this.TxtArgs.BackgroundImage = ((System.Drawing.Image)(resources.GetObject("TxtArgs.BackgroundImage")));
			this.TxtArgs.Dock = ((System.Windows.Forms.DockStyle)(resources.GetObject("TxtArgs.Dock")));
			this.TxtArgs.Enabled = ((bool)(resources.GetObject("TxtArgs.Enabled")));
			this.TxtArgs.Font = ((System.Drawing.Font)(resources.GetObject("TxtArgs.Font")));
			this.TxtArgs.ImeMode = ((System.Windows.Forms.ImeMode)(resources.GetObject("TxtArgs.ImeMode")));
			this.TxtArgs.Location = ((System.Drawing.Point)(resources.GetObject("TxtArgs.Location")));
			this.TxtArgs.MaxLength = ((int)(resources.GetObject("TxtArgs.MaxLength")));
			this.TxtArgs.Multiline = ((bool)(resources.GetObject("TxtArgs.Multiline")));
			this.TxtArgs.Name = "TxtArgs";
			this.TxtArgs.PasswordChar = ((char)(resources.GetObject("TxtArgs.PasswordChar")));
			this.TxtArgs.RightToLeft = ((System.Windows.Forms.RightToLeft)(resources.GetObject("TxtArgs.RightToLeft")));
			this.TxtArgs.ScrollBars = ((System.Windows.Forms.ScrollBars)(resources.GetObject("TxtArgs.ScrollBars")));
			this.TxtArgs.Size = ((System.Drawing.Size)(resources.GetObject("TxtArgs.Size")));
			this.TxtArgs.TabIndex = ((int)(resources.GetObject("TxtArgs.TabIndex")));
			this.TxtArgs.Text = resources.GetString("TxtArgs.Text");
			this.TxtArgs.TextAlign = ((System.Windows.Forms.HorizontalAlignment)(resources.GetObject("TxtArgs.TextAlign")));
			this.TxtArgs.Visible = ((bool)(resources.GetObject("TxtArgs.Visible")));
			this.TxtArgs.WordWrap = ((bool)(resources.GetObject("TxtArgs.WordWrap")));
			// 
			// LblArgs
			// 
			this.LblArgs.AccessibleDescription = resources.GetString("LblArgs.AccessibleDescription");
			this.LblArgs.AccessibleName = resources.GetString("LblArgs.AccessibleName");
			this.LblArgs.Anchor = ((System.Windows.Forms.AnchorStyles)(resources.GetObject("LblArgs.Anchor")));
			this.LblArgs.AutoSize = ((bool)(resources.GetObject("LblArgs.AutoSize")));
			this.LblArgs.Dock = ((System.Windows.Forms.DockStyle)(resources.GetObject("LblArgs.Dock")));
			this.LblArgs.Enabled = ((bool)(resources.GetObject("LblArgs.Enabled")));
			this.LblArgs.Font = ((System.Drawing.Font)(resources.GetObject("LblArgs.Font")));
			this.LblArgs.Image = ((System.Drawing.Image)(resources.GetObject("LblArgs.Image")));
			this.LblArgs.ImageAlign = ((System.Drawing.ContentAlignment)(resources.GetObject("LblArgs.ImageAlign")));
			this.LblArgs.ImageIndex = ((int)(resources.GetObject("LblArgs.ImageIndex")));
			this.LblArgs.ImeMode = ((System.Windows.Forms.ImeMode)(resources.GetObject("LblArgs.ImeMode")));
			this.LblArgs.Location = ((System.Drawing.Point)(resources.GetObject("LblArgs.Location")));
			this.LblArgs.Name = "LblArgs";
			this.LblArgs.RightToLeft = ((System.Windows.Forms.RightToLeft)(resources.GetObject("LblArgs.RightToLeft")));
			this.LblArgs.Size = ((System.Drawing.Size)(resources.GetObject("LblArgs.Size")));
			this.LblArgs.TabIndex = ((int)(resources.GetObject("LblArgs.TabIndex")));
			this.LblArgs.Text = resources.GetString("LblArgs.Text");
			this.LblArgs.TextAlign = ((System.Drawing.ContentAlignment)(resources.GetObject("LblArgs.TextAlign")));
			this.LblArgs.Visible = ((bool)(resources.GetObject("LblArgs.Visible")));
			// 
			// ToolsSpecifier
			// 
			this.AccessibleDescription = resources.GetString("$this.AccessibleDescription");
			this.AccessibleName = resources.GetString("$this.AccessibleName");
			this.AutoScaleBaseSize = ((System.Drawing.Size)(resources.GetObject("$this.AutoScaleBaseSize")));
			this.AutoScroll = ((bool)(resources.GetObject("$this.AutoScroll")));
			this.AutoScrollMargin = ((System.Drawing.Size)(resources.GetObject("$this.AutoScrollMargin")));
			this.AutoScrollMinSize = ((System.Drawing.Size)(resources.GetObject("$this.AutoScrollMinSize")));
			this.BackgroundImage = ((System.Drawing.Image)(resources.GetObject("$this.BackgroundImage")));
			this.ClientSize = ((System.Drawing.Size)(resources.GetObject("$this.ClientSize")));
			this.Controls.Add(this.TxtArgs);
			this.Controls.Add(this.TxtName);
			this.Controls.Add(this.TxtUrl);
			this.Controls.Add(this.LblArgs);
			this.Controls.Add(this.ListTools);
			this.Controls.Add(this.LblName);
			this.Controls.Add(this.LblUrl);
			this.Controls.Add(this.BtnOk);
			this.Controls.Add(this.BtnCancel);
			this.Controls.Add(this.BtnRemove);
			this.Controls.Add(this.BtnAdd);
			this.Controls.Add(this.LblInfo);
			this.Enabled = ((bool)(resources.GetObject("$this.Enabled")));
			this.Font = ((System.Drawing.Font)(resources.GetObject("$this.Font")));
			this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.SizableToolWindow;
			this.Icon = ((System.Drawing.Icon)(resources.GetObject("$this.Icon")));
			this.ImeMode = ((System.Windows.Forms.ImeMode)(resources.GetObject("$this.ImeMode")));
			this.Location = ((System.Drawing.Point)(resources.GetObject("$this.Location")));
			this.MaximumSize = ((System.Drawing.Size)(resources.GetObject("$this.MaximumSize")));
			this.MinimumSize = ((System.Drawing.Size)(resources.GetObject("$this.MinimumSize")));
			this.Name = "ToolsSpecifier";
			this.RightToLeft = ((System.Windows.Forms.RightToLeft)(resources.GetObject("$this.RightToLeft")));
			this.ShowInTaskbar = false;
			this.StartPosition = ((System.Windows.Forms.FormStartPosition)(resources.GetObject("$this.StartPosition")));
			this.Text = resources.GetString("$this.Text");
			this.ResumeLayout(false);

		}
		#endregion
	}


	//========================================================================
	public class ToolInfo
	{
		public string Url;
		public string Name;
		public string Args;

		//---------------------------------------------------------------------
		public ToolInfo (string url, string name, string args)
		{
			Url  = url;
			Name = name;
			Args = args;
		}

		//---------------------------------------------------------------------
		public override int GetHashCode()
		{
			return Name.GetHashCode();
		}

		//---------------------------------------------------------------------
		public override bool Equals(object obj)
		{
			if (!(obj is ToolInfo))
				return false;

			ToolInfo objToCompare = (ToolInfo)obj;
			return string.Compare(Name, objToCompare.Name, true, CultureInfo.InvariantCulture) == 0;
		}

		//---------------------------------------------------------------------
		public static void SplitPathAndArgs(string pathAndArgs, out string path, out string args)
		{
			if (pathAndArgs == null || pathAndArgs.Length == 0)
			{
				path = args = string.Empty;
				return;
			}

			int index = pathAndArgs.IndexOf(ConfigStrings.ArgsSeparator);
			if (index == -1) 
			{
				path = pathAndArgs;
				args = string.Empty;
				return;
			}

			path = pathAndArgs.Substring(0, index);
			args = pathAndArgs.Substring(index + ConfigStrings.ArgsSeparator.Length);
		}

		//---------------------------------------------------------------------
		public static string ConcatPathAndArgs(string path, string args)
		{
			if (path == null || path.Length == 0)
				return string.Empty;
			if (args == null || args.Length == 0)
				return path;

			return string.Concat(path, ConfigStrings.ArgsSeparator, args);
		}
	}
}
