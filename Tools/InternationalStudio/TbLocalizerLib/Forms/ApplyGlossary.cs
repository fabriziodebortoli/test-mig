using System;
using System.Collections;
using System.ComponentModel;
using System.Windows.Forms;
using Microarea.Tools.TBLocalizer.CommonUtilities;

namespace Microarea.Tools.TBLocalizer.Forms
{
	/// <summary>
	/// Seleziona la profondità d'azione relativamente al tree dell'apply glossary.
	/// </summary>
	public class ApplyGlossary : System.Windows.Forms.Form
	{
		private		Button		BtnCancel;
		private		Button		BtnOK;
		private		CheckBox	CkbOverwrite;
		private		ComboBox	CmbDepth;
		private		Label		LblTitle;
		private		Container	components		= null;

		private		NodeType	selectedType;
		private		string		selectedString	= String.Empty;
		private		bool		overwrite		= false;
		private System.Windows.Forms.CheckBox CkbNoTemporary;
		private		bool		noTemporary		= false;

		//properties
		/// <summary> Indice dell'item scelto dalla combo.</summary>
		public		NodeType	SelectedType	{ get {return  selectedType ;} }
		/// <summary> Specifica se sovrascrivere o meno le traduzioni esistenti.</summary>
		public		bool		Overwrite		{ get {return  overwrite;} }
		/// <summary> Stringa dell'item scelto dalla combo.</summary>
		public		string		SelectedString	{ get {return  selectedString ;} }
		
		public		bool		NoTemporary		{ get {return  noTemporary;} }
		
		//-----------------------------------------------------------------
		public ApplyGlossary()
		{
			InitializeComponent();
			PostInitializeComponent();
		}

		/// <summary>
		/// Popola la combo.
		/// </summary>
		//-----------------------------------------------------------------
		private void PostInitializeComponent()
		{
			ArrayList list = new ArrayList();
			list.Add(new GlossaryDepth(NodeType.LASTCHILD,	Strings.CurrentTranslationTable));
			list.Add(new GlossaryDepth(NodeType.RESOURCE,	Strings.CurrentResource));
			list.Add(new GlossaryDepth(NodeType.PROJECT,	Strings.CurrentProject));
			list.Add(new GlossaryDepth(NodeType.SOLUTION,	Strings.EntireSolution));
			
			CmbDepth.DataSource		= list;
			//nomi delle properties ad hoc perchè la combo riconosca key e value
			CmbDepth.DisplayMember	= "DisplayMember";
			CmbDepth.ValueMember	= "ValueMember";
			CmbDepth.SelectedIndex	= 0;			
		}

	
		/// <summary>
		/// Sull'evento OK selezione parte la procedura di traduzione dal translator.
		/// </summary>
		//-----------------------------------------------------------------
		private void BtnOK_Click(object sender, System.EventArgs e)
		{
			if (CmbDepth.SelectedIndex == 0) return;
			selectedType	= ((NodeType)CmbDepth.SelectedValue);
			selectedString	= ((GlossaryDepth)CmbDepth.SelectedItem).DisplayMember;
			DialogResult	= DialogResult.OK;
			Close();
		}

		/// <summary>
		/// Cambio check dell'overwrite.
		/// </summary>
		//-----------------------------------------------------------------
		private void CkbOverwrite_CheckedChanged(object sender, System.EventArgs e)
		{
			overwrite = CkbOverwrite.Checked;
		}
		
		//-----------------------------------------------------------------
		private void CkbNoTemporary_CheckedChanged(object sender, System.EventArgs e)
		{
			if (CkbNoTemporary.Checked)
				CkbNoTemporary.Checked = GlossaryFunctions.CanModifyNoTemporaryFlag();

			noTemporary = CkbNoTemporary.Checked;
		}
		
		//-----------------------------------------------------------------
		private void CmbDepth_SelectedIndexChanged(object sender, System.EventArgs e)
		{
			BtnOK.Enabled = CmbDepth.SelectedIndex != 0;
		}

		/// <summary>
		/// Clean up any resources being used.
		/// </summary>
		//-----------------------------------------------------------------
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
			System.Resources.ResourceManager resources = new System.Resources.ResourceManager(typeof(ApplyGlossary));
			this.CmbDepth = new System.Windows.Forms.ComboBox();
			this.LblTitle = new System.Windows.Forms.Label();
			this.CkbOverwrite = new System.Windows.Forms.CheckBox();
			this.BtnCancel = new System.Windows.Forms.Button();
			this.BtnOK = new System.Windows.Forms.Button();
			this.CkbNoTemporary = new System.Windows.Forms.CheckBox();
			this.SuspendLayout();
			// 
			// CmbDepth
			// 
			this.CmbDepth.AccessibleDescription = resources.GetString("CmbDepth.AccessibleDescription");
			this.CmbDepth.AccessibleName = resources.GetString("CmbDepth.AccessibleName");
			this.CmbDepth.Anchor = ((System.Windows.Forms.AnchorStyles)(resources.GetObject("CmbDepth.Anchor")));
			this.CmbDepth.BackgroundImage = ((System.Drawing.Image)(resources.GetObject("CmbDepth.BackgroundImage")));
			this.CmbDepth.Dock = ((System.Windows.Forms.DockStyle)(resources.GetObject("CmbDepth.Dock")));
			this.CmbDepth.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
			this.CmbDepth.Enabled = ((bool)(resources.GetObject("CmbDepth.Enabled")));
			this.CmbDepth.Font = ((System.Drawing.Font)(resources.GetObject("CmbDepth.Font")));
			this.CmbDepth.ImeMode = ((System.Windows.Forms.ImeMode)(resources.GetObject("CmbDepth.ImeMode")));
			this.CmbDepth.IntegralHeight = ((bool)(resources.GetObject("CmbDepth.IntegralHeight")));
			this.CmbDepth.ItemHeight = ((int)(resources.GetObject("CmbDepth.ItemHeight")));
			this.CmbDepth.Location = ((System.Drawing.Point)(resources.GetObject("CmbDepth.Location")));
			this.CmbDepth.MaxDropDownItems = ((int)(resources.GetObject("CmbDepth.MaxDropDownItems")));
			this.CmbDepth.MaxLength = ((int)(resources.GetObject("CmbDepth.MaxLength")));
			this.CmbDepth.Name = "CmbDepth";
			this.CmbDepth.RightToLeft = ((System.Windows.Forms.RightToLeft)(resources.GetObject("CmbDepth.RightToLeft")));
			this.CmbDepth.Size = ((System.Drawing.Size)(resources.GetObject("CmbDepth.Size")));
			this.CmbDepth.TabIndex = ((int)(resources.GetObject("CmbDepth.TabIndex")));
			this.CmbDepth.Text = resources.GetString("CmbDepth.Text");
			this.CmbDepth.Visible = ((bool)(resources.GetObject("CmbDepth.Visible")));
			this.CmbDepth.SelectedIndexChanged += new System.EventHandler(this.CmbDepth_SelectedIndexChanged);
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
			// CkbOverwrite
			// 
			this.CkbOverwrite.AccessibleDescription = resources.GetString("CkbOverwrite.AccessibleDescription");
			this.CkbOverwrite.AccessibleName = resources.GetString("CkbOverwrite.AccessibleName");
			this.CkbOverwrite.Anchor = ((System.Windows.Forms.AnchorStyles)(resources.GetObject("CkbOverwrite.Anchor")));
			this.CkbOverwrite.Appearance = ((System.Windows.Forms.Appearance)(resources.GetObject("CkbOverwrite.Appearance")));
			this.CkbOverwrite.BackgroundImage = ((System.Drawing.Image)(resources.GetObject("CkbOverwrite.BackgroundImage")));
			this.CkbOverwrite.CheckAlign = ((System.Drawing.ContentAlignment)(resources.GetObject("CkbOverwrite.CheckAlign")));
			this.CkbOverwrite.Dock = ((System.Windows.Forms.DockStyle)(resources.GetObject("CkbOverwrite.Dock")));
			this.CkbOverwrite.Enabled = ((bool)(resources.GetObject("CkbOverwrite.Enabled")));
			this.CkbOverwrite.FlatStyle = ((System.Windows.Forms.FlatStyle)(resources.GetObject("CkbOverwrite.FlatStyle")));
			this.CkbOverwrite.Font = ((System.Drawing.Font)(resources.GetObject("CkbOverwrite.Font")));
			this.CkbOverwrite.Image = ((System.Drawing.Image)(resources.GetObject("CkbOverwrite.Image")));
			this.CkbOverwrite.ImageAlign = ((System.Drawing.ContentAlignment)(resources.GetObject("CkbOverwrite.ImageAlign")));
			this.CkbOverwrite.ImageIndex = ((int)(resources.GetObject("CkbOverwrite.ImageIndex")));
			this.CkbOverwrite.ImeMode = ((System.Windows.Forms.ImeMode)(resources.GetObject("CkbOverwrite.ImeMode")));
			this.CkbOverwrite.Location = ((System.Drawing.Point)(resources.GetObject("CkbOverwrite.Location")));
			this.CkbOverwrite.Name = "CkbOverwrite";
			this.CkbOverwrite.RightToLeft = ((System.Windows.Forms.RightToLeft)(resources.GetObject("CkbOverwrite.RightToLeft")));
			this.CkbOverwrite.Size = ((System.Drawing.Size)(resources.GetObject("CkbOverwrite.Size")));
			this.CkbOverwrite.TabIndex = ((int)(resources.GetObject("CkbOverwrite.TabIndex")));
			this.CkbOverwrite.Text = resources.GetString("CkbOverwrite.Text");
			this.CkbOverwrite.TextAlign = ((System.Drawing.ContentAlignment)(resources.GetObject("CkbOverwrite.TextAlign")));
			this.CkbOverwrite.Visible = ((bool)(resources.GetObject("CkbOverwrite.Visible")));
			this.CkbOverwrite.CheckedChanged += new System.EventHandler(this.CkbOverwrite_CheckedChanged);
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
			// BtnOK
			// 
			this.BtnOK.AccessibleDescription = resources.GetString("BtnOK.AccessibleDescription");
			this.BtnOK.AccessibleName = resources.GetString("BtnOK.AccessibleName");
			this.BtnOK.Anchor = ((System.Windows.Forms.AnchorStyles)(resources.GetObject("BtnOK.Anchor")));
			this.BtnOK.BackgroundImage = ((System.Drawing.Image)(resources.GetObject("BtnOK.BackgroundImage")));
			this.BtnOK.DialogResult = System.Windows.Forms.DialogResult.OK;
			this.BtnOK.Dock = ((System.Windows.Forms.DockStyle)(resources.GetObject("BtnOK.Dock")));
			this.BtnOK.Enabled = ((bool)(resources.GetObject("BtnOK.Enabled")));
			this.BtnOK.FlatStyle = ((System.Windows.Forms.FlatStyle)(resources.GetObject("BtnOK.FlatStyle")));
			this.BtnOK.Font = ((System.Drawing.Font)(resources.GetObject("BtnOK.Font")));
			this.BtnOK.Image = ((System.Drawing.Image)(resources.GetObject("BtnOK.Image")));
			this.BtnOK.ImageAlign = ((System.Drawing.ContentAlignment)(resources.GetObject("BtnOK.ImageAlign")));
			this.BtnOK.ImageIndex = ((int)(resources.GetObject("BtnOK.ImageIndex")));
			this.BtnOK.ImeMode = ((System.Windows.Forms.ImeMode)(resources.GetObject("BtnOK.ImeMode")));
			this.BtnOK.Location = ((System.Drawing.Point)(resources.GetObject("BtnOK.Location")));
			this.BtnOK.Name = "BtnOK";
			this.BtnOK.RightToLeft = ((System.Windows.Forms.RightToLeft)(resources.GetObject("BtnOK.RightToLeft")));
			this.BtnOK.Size = ((System.Drawing.Size)(resources.GetObject("BtnOK.Size")));
			this.BtnOK.TabIndex = ((int)(resources.GetObject("BtnOK.TabIndex")));
			this.BtnOK.Text = resources.GetString("BtnOK.Text");
			this.BtnOK.TextAlign = ((System.Drawing.ContentAlignment)(resources.GetObject("BtnOK.TextAlign")));
			this.BtnOK.Visible = ((bool)(resources.GetObject("BtnOK.Visible")));
			this.BtnOK.Click += new System.EventHandler(this.BtnOK_Click);
			// 
			// CkbNoTemporary
			// 
			this.CkbNoTemporary.AccessibleDescription = resources.GetString("CkbNoTemporary.AccessibleDescription");
			this.CkbNoTemporary.AccessibleName = resources.GetString("CkbNoTemporary.AccessibleName");
			this.CkbNoTemporary.Anchor = ((System.Windows.Forms.AnchorStyles)(resources.GetObject("CkbNoTemporary.Anchor")));
			this.CkbNoTemporary.Appearance = ((System.Windows.Forms.Appearance)(resources.GetObject("CkbNoTemporary.Appearance")));
			this.CkbNoTemporary.BackgroundImage = ((System.Drawing.Image)(resources.GetObject("CkbNoTemporary.BackgroundImage")));
			this.CkbNoTemporary.CheckAlign = ((System.Drawing.ContentAlignment)(resources.GetObject("CkbNoTemporary.CheckAlign")));
			this.CkbNoTemporary.Dock = ((System.Windows.Forms.DockStyle)(resources.GetObject("CkbNoTemporary.Dock")));
			this.CkbNoTemporary.Enabled = ((bool)(resources.GetObject("CkbNoTemporary.Enabled")));
			this.CkbNoTemporary.FlatStyle = ((System.Windows.Forms.FlatStyle)(resources.GetObject("CkbNoTemporary.FlatStyle")));
			this.CkbNoTemporary.Font = ((System.Drawing.Font)(resources.GetObject("CkbNoTemporary.Font")));
			this.CkbNoTemporary.Image = ((System.Drawing.Image)(resources.GetObject("CkbNoTemporary.Image")));
			this.CkbNoTemporary.ImageAlign = ((System.Drawing.ContentAlignment)(resources.GetObject("CkbNoTemporary.ImageAlign")));
			this.CkbNoTemporary.ImageIndex = ((int)(resources.GetObject("CkbNoTemporary.ImageIndex")));
			this.CkbNoTemporary.ImeMode = ((System.Windows.Forms.ImeMode)(resources.GetObject("CkbNoTemporary.ImeMode")));
			this.CkbNoTemporary.Location = ((System.Drawing.Point)(resources.GetObject("CkbNoTemporary.Location")));
			this.CkbNoTemporary.Name = "CkbNoTemporary";
			this.CkbNoTemporary.RightToLeft = ((System.Windows.Forms.RightToLeft)(resources.GetObject("CkbNoTemporary.RightToLeft")));
			this.CkbNoTemporary.Size = ((System.Drawing.Size)(resources.GetObject("CkbNoTemporary.Size")));
			this.CkbNoTemporary.TabIndex = ((int)(resources.GetObject("CkbNoTemporary.TabIndex")));
			this.CkbNoTemporary.Text = resources.GetString("CkbNoTemporary.Text");
			this.CkbNoTemporary.TextAlign = ((System.Drawing.ContentAlignment)(resources.GetObject("CkbNoTemporary.TextAlign")));
			this.CkbNoTemporary.Visible = ((bool)(resources.GetObject("CkbNoTemporary.Visible")));
			this.CkbNoTemporary.CheckedChanged += new System.EventHandler(this.CkbNoTemporary_CheckedChanged);
			// 
			// ApplyGlossary
			// 
			this.AccessibleDescription = resources.GetString("$this.AccessibleDescription");
			this.AccessibleName = resources.GetString("$this.AccessibleName");
			this.AutoScaleBaseSize = ((System.Drawing.Size)(resources.GetObject("$this.AutoScaleBaseSize")));
			this.AutoScroll = ((bool)(resources.GetObject("$this.AutoScroll")));
			this.AutoScrollMargin = ((System.Drawing.Size)(resources.GetObject("$this.AutoScrollMargin")));
			this.AutoScrollMinSize = ((System.Drawing.Size)(resources.GetObject("$this.AutoScrollMinSize")));
			this.BackgroundImage = ((System.Drawing.Image)(resources.GetObject("$this.BackgroundImage")));
			this.CancelButton = this.BtnCancel;
			this.ClientSize = ((System.Drawing.Size)(resources.GetObject("$this.ClientSize")));
			this.ControlBox = false;
			this.Controls.Add(this.CkbNoTemporary);
			this.Controls.Add(this.BtnOK);
			this.Controls.Add(this.BtnCancel);
			this.Controls.Add(this.CkbOverwrite);
			this.Controls.Add(this.LblTitle);
			this.Controls.Add(this.CmbDepth);
			this.Enabled = ((bool)(resources.GetObject("$this.Enabled")));
			this.Font = ((System.Drawing.Font)(resources.GetObject("$this.Font")));
			this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedToolWindow;
			this.Icon = ((System.Drawing.Icon)(resources.GetObject("$this.Icon")));
			this.ImeMode = ((System.Windows.Forms.ImeMode)(resources.GetObject("$this.ImeMode")));
			this.Location = ((System.Drawing.Point)(resources.GetObject("$this.Location")));
			this.MaximumSize = ((System.Drawing.Size)(resources.GetObject("$this.MaximumSize")));
			this.MinimumSize = ((System.Drawing.Size)(resources.GetObject("$this.MinimumSize")));
			this.Name = "ApplyGlossary";
			this.RightToLeft = ((System.Windows.Forms.RightToLeft)(resources.GetObject("$this.RightToLeft")));
			this.ShowInTaskbar = false;
			this.StartPosition = ((System.Windows.Forms.FormStartPosition)(resources.GetObject("$this.StartPosition")));
			this.Text = resources.GetString("$this.Text");
			this.ResumeLayout(false);

		}
		#endregion

		

		

	}
}
