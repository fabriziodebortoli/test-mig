using System;
using System.Collections;
using System.ComponentModel;
using System.IO;
using System.Windows.Forms;

namespace Microarea.Tools.TBLocalizer.Forms
{
	/// <summary>
	/// Permette di specificare quali estensioni parsare come file XML.
	/// </summary>
	//=========================================================================
	public class XmlSpecifier : Form
	{
		#region Controls
		private TextBox		TxtXml;
		private ListBox		ListboxXml;
		private Button		BtnRemove;
		private Button		BtnAdd;
		private Button		BtnOk;
		private Button		BtnCancel;
		private Label		LblTitle;
		private Label		LblList;
		private Label		LblMessages;

		private Container	components	= null;
		#endregion

		#region Private members
		private string[] extensions;
		#endregion

		#region Properties
		public string[] Extension { get { return extensions; } set { extensions = value; } }
		#endregion

		#region Constructors
		//---------------------------------------------------------------------
		public XmlSpecifier()
		{
			InitializeComponent();
		}
		#endregion

		#region Private methods
		//---------------------------------------------------------------------
		private void XmlSpecifier_Load(object sender, System.EventArgs e)
		{
			ListboxXml.Items.AddRange(Extension);

			if (ListboxXml.FindStringExact(AllStrings.xmlExtension) != ListBox.NoMatches)
				TxtXml.Text = string.Empty;
			else
				TxtXml.Text = AllStrings.xmlExtension;
		}

		//---------------------------------------------------------------------
		private void BtnOk_Click(object sender, System.EventArgs e)
		{
			LblMessages.Text = string.Empty;
			Extension = new string[ListboxXml.Items.Count];

			for (int i = 0; i < ListboxXml.Items.Count; i++)
				Extension.SetValue(ListboxXml.Items[i] as string, i);

			DialogResult = DialogResult.OK;
			Close();
		}

		//---------------------------------------------------------------------
		private void BtnAdd_Click(object sender, System.EventArgs e)
		{
			LblMessages.Text = string.Empty;

			if (TxtXml.Text == null || TxtXml.Text == string.Empty)
			{
				LblMessages.Text = Strings.TypeExtension;
				return;
			}

			if (TxtXml.Text.IndexOfAny(Path.GetInvalidPathChars()) != -1)
			{
				LblMessages.Text = Strings.RetypeExtension;
				return;
			}

			string extension = (!TxtXml.Text.StartsWith(".")) ? "." + TxtXml.Text : TxtXml.Text;
				
			if (ListboxXml.Items.Contains(extension))
			{
				LblMessages.Text = Strings.RepeatedExtension;
				return;
			}

			ListboxXml.Items.Add(extension);
			TxtXml.Text = string.Empty;
			TxtXml.Focus();
		}

		//---------------------------------------------------------------------
		private void BtnCancel_Click(object sender, System.EventArgs e)
		{
			LblMessages.Text = string.Empty;
			DialogResult = DialogResult.Cancel;
			Close();
		}

		//---------------------------------------------------------------------
		private void BtnRemove_Click(object sender, System.EventArgs e)
		{
			LblMessages.Text = String.Empty;

			if (ListboxXml.SelectedItems.Count == 0)
			{
				LblMessages.Text = Strings.NoSelectedExtension;
				return;
			}

			ArrayList tmpList = new ArrayList(ListboxXml.SelectedItems);
			foreach (Object obj in tmpList)
				ListboxXml.Items.Remove(obj);
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
			System.Resources.ResourceManager resources = new System.Resources.ResourceManager(typeof(XmlSpecifier));
			this.TxtXml = new System.Windows.Forms.TextBox();
			this.ListboxXml = new System.Windows.Forms.ListBox();
			this.BtnRemove = new System.Windows.Forms.Button();
			this.BtnAdd = new System.Windows.Forms.Button();
			this.BtnOk = new System.Windows.Forms.Button();
			this.BtnCancel = new System.Windows.Forms.Button();
			this.LblTitle = new System.Windows.Forms.Label();
			this.LblList = new System.Windows.Forms.Label();
			this.LblMessages = new System.Windows.Forms.Label();
			this.SuspendLayout();
			// 
			// TxtXml
			// 
			this.TxtXml.AccessibleDescription = resources.GetString("TxtXml.AccessibleDescription");
			this.TxtXml.AccessibleName = resources.GetString("TxtXml.AccessibleName");
			this.TxtXml.Anchor = ((System.Windows.Forms.AnchorStyles)(resources.GetObject("TxtXml.Anchor")));
			this.TxtXml.AutoSize = ((bool)(resources.GetObject("TxtXml.AutoSize")));
			this.TxtXml.BackgroundImage = ((System.Drawing.Image)(resources.GetObject("TxtXml.BackgroundImage")));
			this.TxtXml.Dock = ((System.Windows.Forms.DockStyle)(resources.GetObject("TxtXml.Dock")));
			this.TxtXml.Enabled = ((bool)(resources.GetObject("TxtXml.Enabled")));
			this.TxtXml.Font = ((System.Drawing.Font)(resources.GetObject("TxtXml.Font")));
			this.TxtXml.ImeMode = ((System.Windows.Forms.ImeMode)(resources.GetObject("TxtXml.ImeMode")));
			this.TxtXml.Location = ((System.Drawing.Point)(resources.GetObject("TxtXml.Location")));
			this.TxtXml.MaxLength = ((int)(resources.GetObject("TxtXml.MaxLength")));
			this.TxtXml.Multiline = ((bool)(resources.GetObject("TxtXml.Multiline")));
			this.TxtXml.Name = "TxtXml";
			this.TxtXml.PasswordChar = ((char)(resources.GetObject("TxtXml.PasswordChar")));
			this.TxtXml.RightToLeft = ((System.Windows.Forms.RightToLeft)(resources.GetObject("TxtXml.RightToLeft")));
			this.TxtXml.ScrollBars = ((System.Windows.Forms.ScrollBars)(resources.GetObject("TxtXml.ScrollBars")));
			this.TxtXml.Size = ((System.Drawing.Size)(resources.GetObject("TxtXml.Size")));
			this.TxtXml.TabIndex = ((int)(resources.GetObject("TxtXml.TabIndex")));
			this.TxtXml.Text = resources.GetString("TxtXml.Text");
			this.TxtXml.TextAlign = ((System.Windows.Forms.HorizontalAlignment)(resources.GetObject("TxtXml.TextAlign")));
			this.TxtXml.Visible = ((bool)(resources.GetObject("TxtXml.Visible")));
			this.TxtXml.WordWrap = ((bool)(resources.GetObject("TxtXml.WordWrap")));
			// 
			// ListboxXml
			// 
			this.ListboxXml.AccessibleDescription = resources.GetString("ListboxXml.AccessibleDescription");
			this.ListboxXml.AccessibleName = resources.GetString("ListboxXml.AccessibleName");
			this.ListboxXml.Anchor = ((System.Windows.Forms.AnchorStyles)(resources.GetObject("ListboxXml.Anchor")));
			this.ListboxXml.BackgroundImage = ((System.Drawing.Image)(resources.GetObject("ListboxXml.BackgroundImage")));
			this.ListboxXml.ColumnWidth = ((int)(resources.GetObject("ListboxXml.ColumnWidth")));
			this.ListboxXml.Dock = ((System.Windows.Forms.DockStyle)(resources.GetObject("ListboxXml.Dock")));
			this.ListboxXml.Enabled = ((bool)(resources.GetObject("ListboxXml.Enabled")));
			this.ListboxXml.Font = ((System.Drawing.Font)(resources.GetObject("ListboxXml.Font")));
			this.ListboxXml.HorizontalExtent = ((int)(resources.GetObject("ListboxXml.HorizontalExtent")));
			this.ListboxXml.HorizontalScrollbar = ((bool)(resources.GetObject("ListboxXml.HorizontalScrollbar")));
			this.ListboxXml.ImeMode = ((System.Windows.Forms.ImeMode)(resources.GetObject("ListboxXml.ImeMode")));
			this.ListboxXml.IntegralHeight = ((bool)(resources.GetObject("ListboxXml.IntegralHeight")));
			this.ListboxXml.ItemHeight = ((int)(resources.GetObject("ListboxXml.ItemHeight")));
			this.ListboxXml.Location = ((System.Drawing.Point)(resources.GetObject("ListboxXml.Location")));
			this.ListboxXml.Name = "ListboxXml";
			this.ListboxXml.RightToLeft = ((System.Windows.Forms.RightToLeft)(resources.GetObject("ListboxXml.RightToLeft")));
			this.ListboxXml.ScrollAlwaysVisible = ((bool)(resources.GetObject("ListboxXml.ScrollAlwaysVisible")));
			this.ListboxXml.SelectionMode = System.Windows.Forms.SelectionMode.MultiExtended;
			this.ListboxXml.Size = ((System.Drawing.Size)(resources.GetObject("ListboxXml.Size")));
			this.ListboxXml.TabIndex = ((int)(resources.GetObject("ListboxXml.TabIndex")));
			this.ListboxXml.Visible = ((bool)(resources.GetObject("ListboxXml.Visible")));
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
			// BtnOk
			// 
			this.BtnOk.AccessibleDescription = resources.GetString("BtnOk.AccessibleDescription");
			this.BtnOk.AccessibleName = resources.GetString("BtnOk.AccessibleName");
			this.BtnOk.Anchor = ((System.Windows.Forms.AnchorStyles)(resources.GetObject("BtnOk.Anchor")));
			this.BtnOk.BackgroundImage = ((System.Drawing.Image)(resources.GetObject("BtnOk.BackgroundImage")));
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
			this.BtnOk.Click += new System.EventHandler(this.BtnOk_Click);
			// 
			// BtnCancel
			// 
			this.BtnCancel.AccessibleDescription = resources.GetString("BtnCancel.AccessibleDescription");
			this.BtnCancel.AccessibleName = resources.GetString("BtnCancel.AccessibleName");
			this.BtnCancel.Anchor = ((System.Windows.Forms.AnchorStyles)(resources.GetObject("BtnCancel.Anchor")));
			this.BtnCancel.BackgroundImage = ((System.Drawing.Image)(resources.GetObject("BtnCancel.BackgroundImage")));
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
			this.BtnCancel.Click += new System.EventHandler(this.BtnCancel_Click);
			// 
			// LblTitle
			// 
			this.LblTitle.AccessibleDescription = resources.GetString("LblTitle.AccessibleDescription");
			this.LblTitle.AccessibleName = resources.GetString("LblTitle.AccessibleName");
			this.LblTitle.Anchor = ((System.Windows.Forms.AnchorStyles)(resources.GetObject("LblTitle.Anchor")));
			this.LblTitle.AutoSize = ((bool)(resources.GetObject("LblTitle.AutoSize")));
			this.LblTitle.Dock = ((System.Windows.Forms.DockStyle)(resources.GetObject("LblTitle.Dock")));
			this.LblTitle.Enabled = ((bool)(resources.GetObject("LblTitle.Enabled")));
			this.LblTitle.FlatStyle = System.Windows.Forms.FlatStyle.System;
			this.LblTitle.Font = ((System.Drawing.Font)(resources.GetObject("LblTitle.Font")));
			this.LblTitle.Image = ((System.Drawing.Image)(resources.GetObject("LblTitle.Image")));
			this.LblTitle.ImageAlign = ((System.Drawing.ContentAlignment)(resources.GetObject("LblTitle.ImageAlign")));
			this.LblTitle.ImageIndex = ((int)(resources.GetObject("LblTitle.ImageIndex")));
			this.LblTitle.ImeMode = ((System.Windows.Forms.ImeMode)(resources.GetObject("LblTitle.ImeMode")));
			this.LblTitle.Location = ((System.Drawing.Point)(resources.GetObject("LblTitle.Location")));
			this.LblTitle.Name = "LblTitle";
			this.LblTitle.RightToLeft = ((System.Windows.Forms.RightToLeft)(resources.GetObject("LblTitle.RightToLeft")));
			this.LblTitle.Size = ((System.Drawing.Size)(resources.GetObject("LblTitle.Size")));
			this.LblTitle.TabIndex = ((int)(resources.GetObject("LblTitle.TabIndex")));
			this.LblTitle.Text = resources.GetString("LblTitle.Text");
			this.LblTitle.TextAlign = ((System.Drawing.ContentAlignment)(resources.GetObject("LblTitle.TextAlign")));
			this.LblTitle.Visible = ((bool)(resources.GetObject("LblTitle.Visible")));
			// 
			// LblList
			// 
			this.LblList.AccessibleDescription = resources.GetString("LblList.AccessibleDescription");
			this.LblList.AccessibleName = resources.GetString("LblList.AccessibleName");
			this.LblList.Anchor = ((System.Windows.Forms.AnchorStyles)(resources.GetObject("LblList.Anchor")));
			this.LblList.AutoSize = ((bool)(resources.GetObject("LblList.AutoSize")));
			this.LblList.Dock = ((System.Windows.Forms.DockStyle)(resources.GetObject("LblList.Dock")));
			this.LblList.Enabled = ((bool)(resources.GetObject("LblList.Enabled")));
			this.LblList.FlatStyle = System.Windows.Forms.FlatStyle.System;
			this.LblList.Font = ((System.Drawing.Font)(resources.GetObject("LblList.Font")));
			this.LblList.Image = ((System.Drawing.Image)(resources.GetObject("LblList.Image")));
			this.LblList.ImageAlign = ((System.Drawing.ContentAlignment)(resources.GetObject("LblList.ImageAlign")));
			this.LblList.ImageIndex = ((int)(resources.GetObject("LblList.ImageIndex")));
			this.LblList.ImeMode = ((System.Windows.Forms.ImeMode)(resources.GetObject("LblList.ImeMode")));
			this.LblList.Location = ((System.Drawing.Point)(resources.GetObject("LblList.Location")));
			this.LblList.Name = "LblList";
			this.LblList.RightToLeft = ((System.Windows.Forms.RightToLeft)(resources.GetObject("LblList.RightToLeft")));
			this.LblList.Size = ((System.Drawing.Size)(resources.GetObject("LblList.Size")));
			this.LblList.TabIndex = ((int)(resources.GetObject("LblList.TabIndex")));
			this.LblList.Text = resources.GetString("LblList.Text");
			this.LblList.TextAlign = ((System.Drawing.ContentAlignment)(resources.GetObject("LblList.TextAlign")));
			this.LblList.Visible = ((bool)(resources.GetObject("LblList.Visible")));
			// 
			// LblMessages
			// 
			this.LblMessages.AccessibleDescription = resources.GetString("LblMessages.AccessibleDescription");
			this.LblMessages.AccessibleName = resources.GetString("LblMessages.AccessibleName");
			this.LblMessages.Anchor = ((System.Windows.Forms.AnchorStyles)(resources.GetObject("LblMessages.Anchor")));
			this.LblMessages.AutoSize = ((bool)(resources.GetObject("LblMessages.AutoSize")));
			this.LblMessages.Dock = ((System.Windows.Forms.DockStyle)(resources.GetObject("LblMessages.Dock")));
			this.LblMessages.Enabled = ((bool)(resources.GetObject("LblMessages.Enabled")));
			this.LblMessages.FlatStyle = System.Windows.Forms.FlatStyle.System;
			this.LblMessages.Font = ((System.Drawing.Font)(resources.GetObject("LblMessages.Font")));
			this.LblMessages.ForeColor = System.Drawing.Color.Red;
			this.LblMessages.Image = ((System.Drawing.Image)(resources.GetObject("LblMessages.Image")));
			this.LblMessages.ImageAlign = ((System.Drawing.ContentAlignment)(resources.GetObject("LblMessages.ImageAlign")));
			this.LblMessages.ImageIndex = ((int)(resources.GetObject("LblMessages.ImageIndex")));
			this.LblMessages.ImeMode = ((System.Windows.Forms.ImeMode)(resources.GetObject("LblMessages.ImeMode")));
			this.LblMessages.Location = ((System.Drawing.Point)(resources.GetObject("LblMessages.Location")));
			this.LblMessages.Name = "LblMessages";
			this.LblMessages.RightToLeft = ((System.Windows.Forms.RightToLeft)(resources.GetObject("LblMessages.RightToLeft")));
			this.LblMessages.Size = ((System.Drawing.Size)(resources.GetObject("LblMessages.Size")));
			this.LblMessages.TabIndex = ((int)(resources.GetObject("LblMessages.TabIndex")));
			this.LblMessages.Text = resources.GetString("LblMessages.Text");
			this.LblMessages.TextAlign = ((System.Drawing.ContentAlignment)(resources.GetObject("LblMessages.TextAlign")));
			this.LblMessages.Visible = ((bool)(resources.GetObject("LblMessages.Visible")));
			// 
			// XmlSpecifier
			// 
			this.AccessibleDescription = resources.GetString("$this.AccessibleDescription");
			this.AccessibleName = resources.GetString("$this.AccessibleName");
			this.AutoScaleBaseSize = ((System.Drawing.Size)(resources.GetObject("$this.AutoScaleBaseSize")));
			this.AutoScroll = ((bool)(resources.GetObject("$this.AutoScroll")));
			this.AutoScrollMargin = ((System.Drawing.Size)(resources.GetObject("$this.AutoScrollMargin")));
			this.AutoScrollMinSize = ((System.Drawing.Size)(resources.GetObject("$this.AutoScrollMinSize")));
			this.BackgroundImage = ((System.Drawing.Image)(resources.GetObject("$this.BackgroundImage")));
			this.ClientSize = ((System.Drawing.Size)(resources.GetObject("$this.ClientSize")));
			this.Controls.Add(this.LblMessages);
			this.Controls.Add(this.LblList);
			this.Controls.Add(this.LblTitle);
			this.Controls.Add(this.BtnCancel);
			this.Controls.Add(this.BtnOk);
			this.Controls.Add(this.BtnAdd);
			this.Controls.Add(this.BtnRemove);
			this.Controls.Add(this.ListboxXml);
			this.Controls.Add(this.TxtXml);
			this.Enabled = ((bool)(resources.GetObject("$this.Enabled")));
			this.Font = ((System.Drawing.Font)(resources.GetObject("$this.Font")));
			this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedToolWindow;
			this.Icon = ((System.Drawing.Icon)(resources.GetObject("$this.Icon")));
			this.ImeMode = ((System.Windows.Forms.ImeMode)(resources.GetObject("$this.ImeMode")));
			this.Location = ((System.Drawing.Point)(resources.GetObject("$this.Location")));
			this.MaximumSize = ((System.Drawing.Size)(resources.GetObject("$this.MaximumSize")));
			this.MinimumSize = ((System.Drawing.Size)(resources.GetObject("$this.MinimumSize")));
			this.Name = "XmlSpecifier";
			this.RightToLeft = ((System.Windows.Forms.RightToLeft)(resources.GetObject("$this.RightToLeft")));
			this.ShowInTaskbar = false;
			this.StartPosition = ((System.Windows.Forms.FormStartPosition)(resources.GetObject("$this.StartPosition")));
			this.Text = resources.GetString("$this.Text");
			this.Load += new System.EventHandler(this.XmlSpecifier_Load);
			this.ResumeLayout(false);

		}
		#endregion
	}
}
