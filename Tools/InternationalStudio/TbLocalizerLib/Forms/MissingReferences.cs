using System;
using System.ComponentModel;
using System.Windows.Forms;

namespace Microarea.Tools.TBLocalizer.Forms
{
	/// <summary>
	/// Missing References Report.
	/// </summary>
	//=========================================================================
	public class MissingReferences : System.Windows.Forms.Form
	{
		private Button		BtnNo;
		private Container	components = null;
		private System.Windows.Forms.CheckBox CkbApplyAll;
		private System.Windows.Forms.Button BtnYes;
		private System.Windows.Forms.RichTextBox TxtMissingReferences;
		private System.Windows.Forms.Label LblTitle;
		private System.Windows.Forms.Label LblQuestion;

		private bool toAll	= false;
		
		public	bool ToAll  { get {return toAll;}  set {toAll = value;}}
		
		//---------------------------------------------------------------------
		public MissingReferences(string project, string message)
		{
			InitializeComponent();
			PostInitializeComponent(project, message);
		}

		//---------------------------------------------------------------------
		public void PostInitializeComponent(string project, string message)
		{
			LblQuestion.Text = Strings.ContinueWithMissingRefences;
			LblTitle.Text =  String.Format(Strings.ReferencesNotFound, project);
			TxtMissingReferences.Text = message;
		}

		//---------------------------------------------------------------------
		private void BtnYes_Click(object sender, System.EventArgs e)
		{
			DialogResult = DialogResult.Yes;
			Close();
		}

		//---------------------------------------------------------------------
		private void BtnNo_Click(object sender, System.EventArgs e)
		{
			DialogResult = DialogResult.No;
			Close();
		}

		//---------------------------------------------------------------------
		private void CkbApplyAll_CheckedChanged(object sender, System.EventArgs e)
		{
			ToAll = CkbApplyAll.Checked;
		}

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
			System.Resources.ResourceManager resources = new System.Resources.ResourceManager(typeof(MissingReferences));
			this.BtnNo = new System.Windows.Forms.Button();
			this.CkbApplyAll = new System.Windows.Forms.CheckBox();
			this.BtnYes = new System.Windows.Forms.Button();
			this.TxtMissingReferences = new System.Windows.Forms.RichTextBox();
			this.LblTitle = new System.Windows.Forms.Label();
			this.LblQuestion = new System.Windows.Forms.Label();
			this.SuspendLayout();
			// 
			// BtnNo
			// 
			this.BtnNo.AccessibleDescription = resources.GetString("BtnNo.AccessibleDescription");
			this.BtnNo.AccessibleName = resources.GetString("BtnNo.AccessibleName");
			this.BtnNo.Anchor = ((System.Windows.Forms.AnchorStyles)(resources.GetObject("BtnNo.Anchor")));
			this.BtnNo.BackgroundImage = ((System.Drawing.Image)(resources.GetObject("BtnNo.BackgroundImage")));
			this.BtnNo.Dock = ((System.Windows.Forms.DockStyle)(resources.GetObject("BtnNo.Dock")));
			this.BtnNo.Enabled = ((bool)(resources.GetObject("BtnNo.Enabled")));
			this.BtnNo.FlatStyle = ((System.Windows.Forms.FlatStyle)(resources.GetObject("BtnNo.FlatStyle")));
			this.BtnNo.Font = ((System.Drawing.Font)(resources.GetObject("BtnNo.Font")));
			this.BtnNo.Image = ((System.Drawing.Image)(resources.GetObject("BtnNo.Image")));
			this.BtnNo.ImageAlign = ((System.Drawing.ContentAlignment)(resources.GetObject("BtnNo.ImageAlign")));
			this.BtnNo.ImageIndex = ((int)(resources.GetObject("BtnNo.ImageIndex")));
			this.BtnNo.ImeMode = ((System.Windows.Forms.ImeMode)(resources.GetObject("BtnNo.ImeMode")));
			this.BtnNo.Location = ((System.Drawing.Point)(resources.GetObject("BtnNo.Location")));
			this.BtnNo.Name = "BtnNo";
			this.BtnNo.RightToLeft = ((System.Windows.Forms.RightToLeft)(resources.GetObject("BtnNo.RightToLeft")));
			this.BtnNo.Size = ((System.Drawing.Size)(resources.GetObject("BtnNo.Size")));
			this.BtnNo.TabIndex = ((int)(resources.GetObject("BtnNo.TabIndex")));
			this.BtnNo.Text = resources.GetString("BtnNo.Text");
			this.BtnNo.TextAlign = ((System.Drawing.ContentAlignment)(resources.GetObject("BtnNo.TextAlign")));
			this.BtnNo.Visible = ((bool)(resources.GetObject("BtnNo.Visible")));
			this.BtnNo.Click += new System.EventHandler(this.BtnNo_Click);
			// 
			// CkbApplyAll
			// 
			this.CkbApplyAll.AccessibleDescription = resources.GetString("CkbApplyAll.AccessibleDescription");
			this.CkbApplyAll.AccessibleName = resources.GetString("CkbApplyAll.AccessibleName");
			this.CkbApplyAll.Anchor = ((System.Windows.Forms.AnchorStyles)(resources.GetObject("CkbApplyAll.Anchor")));
			this.CkbApplyAll.Appearance = ((System.Windows.Forms.Appearance)(resources.GetObject("CkbApplyAll.Appearance")));
			this.CkbApplyAll.BackgroundImage = ((System.Drawing.Image)(resources.GetObject("CkbApplyAll.BackgroundImage")));
			this.CkbApplyAll.CheckAlign = ((System.Drawing.ContentAlignment)(resources.GetObject("CkbApplyAll.CheckAlign")));
			this.CkbApplyAll.Dock = ((System.Windows.Forms.DockStyle)(resources.GetObject("CkbApplyAll.Dock")));
			this.CkbApplyAll.Enabled = ((bool)(resources.GetObject("CkbApplyAll.Enabled")));
			this.CkbApplyAll.FlatStyle = ((System.Windows.Forms.FlatStyle)(resources.GetObject("CkbApplyAll.FlatStyle")));
			this.CkbApplyAll.Font = ((System.Drawing.Font)(resources.GetObject("CkbApplyAll.Font")));
			this.CkbApplyAll.Image = ((System.Drawing.Image)(resources.GetObject("CkbApplyAll.Image")));
			this.CkbApplyAll.ImageAlign = ((System.Drawing.ContentAlignment)(resources.GetObject("CkbApplyAll.ImageAlign")));
			this.CkbApplyAll.ImageIndex = ((int)(resources.GetObject("CkbApplyAll.ImageIndex")));
			this.CkbApplyAll.ImeMode = ((System.Windows.Forms.ImeMode)(resources.GetObject("CkbApplyAll.ImeMode")));
			this.CkbApplyAll.Location = ((System.Drawing.Point)(resources.GetObject("CkbApplyAll.Location")));
			this.CkbApplyAll.Name = "CkbApplyAll";
			this.CkbApplyAll.RightToLeft = ((System.Windows.Forms.RightToLeft)(resources.GetObject("CkbApplyAll.RightToLeft")));
			this.CkbApplyAll.Size = ((System.Drawing.Size)(resources.GetObject("CkbApplyAll.Size")));
			this.CkbApplyAll.TabIndex = ((int)(resources.GetObject("CkbApplyAll.TabIndex")));
			this.CkbApplyAll.Text = resources.GetString("CkbApplyAll.Text");
			this.CkbApplyAll.TextAlign = ((System.Drawing.ContentAlignment)(resources.GetObject("CkbApplyAll.TextAlign")));
			this.CkbApplyAll.Visible = ((bool)(resources.GetObject("CkbApplyAll.Visible")));
			this.CkbApplyAll.CheckedChanged += new System.EventHandler(this.CkbApplyAll_CheckedChanged);
			// 
			// BtnYes
			// 
			this.BtnYes.AccessibleDescription = resources.GetString("BtnYes.AccessibleDescription");
			this.BtnYes.AccessibleName = resources.GetString("BtnYes.AccessibleName");
			this.BtnYes.Anchor = ((System.Windows.Forms.AnchorStyles)(resources.GetObject("BtnYes.Anchor")));
			this.BtnYes.BackgroundImage = ((System.Drawing.Image)(resources.GetObject("BtnYes.BackgroundImage")));
			this.BtnYes.Dock = ((System.Windows.Forms.DockStyle)(resources.GetObject("BtnYes.Dock")));
			this.BtnYes.Enabled = ((bool)(resources.GetObject("BtnYes.Enabled")));
			this.BtnYes.FlatStyle = ((System.Windows.Forms.FlatStyle)(resources.GetObject("BtnYes.FlatStyle")));
			this.BtnYes.Font = ((System.Drawing.Font)(resources.GetObject("BtnYes.Font")));
			this.BtnYes.Image = ((System.Drawing.Image)(resources.GetObject("BtnYes.Image")));
			this.BtnYes.ImageAlign = ((System.Drawing.ContentAlignment)(resources.GetObject("BtnYes.ImageAlign")));
			this.BtnYes.ImageIndex = ((int)(resources.GetObject("BtnYes.ImageIndex")));
			this.BtnYes.ImeMode = ((System.Windows.Forms.ImeMode)(resources.GetObject("BtnYes.ImeMode")));
			this.BtnYes.Location = ((System.Drawing.Point)(resources.GetObject("BtnYes.Location")));
			this.BtnYes.Name = "BtnYes";
			this.BtnYes.RightToLeft = ((System.Windows.Forms.RightToLeft)(resources.GetObject("BtnYes.RightToLeft")));
			this.BtnYes.Size = ((System.Drawing.Size)(resources.GetObject("BtnYes.Size")));
			this.BtnYes.TabIndex = ((int)(resources.GetObject("BtnYes.TabIndex")));
			this.BtnYes.Text = resources.GetString("BtnYes.Text");
			this.BtnYes.TextAlign = ((System.Drawing.ContentAlignment)(resources.GetObject("BtnYes.TextAlign")));
			this.BtnYes.Visible = ((bool)(resources.GetObject("BtnYes.Visible")));
			this.BtnYes.Click += new System.EventHandler(this.BtnYes_Click);
			// 
			// TxtMissingReferences
			// 
			this.TxtMissingReferences.AccessibleDescription = resources.GetString("TxtMissingReferences.AccessibleDescription");
			this.TxtMissingReferences.AccessibleName = resources.GetString("TxtMissingReferences.AccessibleName");
			this.TxtMissingReferences.Anchor = ((System.Windows.Forms.AnchorStyles)(resources.GetObject("TxtMissingReferences.Anchor")));
			this.TxtMissingReferences.AutoSize = ((bool)(resources.GetObject("TxtMissingReferences.AutoSize")));
			this.TxtMissingReferences.BackgroundImage = ((System.Drawing.Image)(resources.GetObject("TxtMissingReferences.BackgroundImage")));
			this.TxtMissingReferences.BulletIndent = ((int)(resources.GetObject("TxtMissingReferences.BulletIndent")));
			this.TxtMissingReferences.Dock = ((System.Windows.Forms.DockStyle)(resources.GetObject("TxtMissingReferences.Dock")));
			this.TxtMissingReferences.Enabled = ((bool)(resources.GetObject("TxtMissingReferences.Enabled")));
			this.TxtMissingReferences.Font = ((System.Drawing.Font)(resources.GetObject("TxtMissingReferences.Font")));
			this.TxtMissingReferences.ImeMode = ((System.Windows.Forms.ImeMode)(resources.GetObject("TxtMissingReferences.ImeMode")));
			this.TxtMissingReferences.Location = ((System.Drawing.Point)(resources.GetObject("TxtMissingReferences.Location")));
			this.TxtMissingReferences.MaxLength = ((int)(resources.GetObject("TxtMissingReferences.MaxLength")));
			this.TxtMissingReferences.Multiline = ((bool)(resources.GetObject("TxtMissingReferences.Multiline")));
			this.TxtMissingReferences.Name = "TxtMissingReferences";
			this.TxtMissingReferences.ReadOnly = true;
			this.TxtMissingReferences.RightMargin = ((int)(resources.GetObject("TxtMissingReferences.RightMargin")));
			this.TxtMissingReferences.RightToLeft = ((System.Windows.Forms.RightToLeft)(resources.GetObject("TxtMissingReferences.RightToLeft")));
			this.TxtMissingReferences.ScrollBars = ((System.Windows.Forms.RichTextBoxScrollBars)(resources.GetObject("TxtMissingReferences.ScrollBars")));
			this.TxtMissingReferences.Size = ((System.Drawing.Size)(resources.GetObject("TxtMissingReferences.Size")));
			this.TxtMissingReferences.TabIndex = ((int)(resources.GetObject("TxtMissingReferences.TabIndex")));
			this.TxtMissingReferences.Text = resources.GetString("TxtMissingReferences.Text");
			this.TxtMissingReferences.Visible = ((bool)(resources.GetObject("TxtMissingReferences.Visible")));
			this.TxtMissingReferences.WordWrap = ((bool)(resources.GetObject("TxtMissingReferences.WordWrap")));
			this.TxtMissingReferences.ZoomFactor = ((System.Single)(resources.GetObject("TxtMissingReferences.ZoomFactor")));
			// 
			// LblTitle
			// 
			this.LblTitle.AccessibleDescription = resources.GetString("LblTitle.AccessibleDescription");
			this.LblTitle.AccessibleName = resources.GetString("LblTitle.AccessibleName");
			this.LblTitle.Anchor = ((System.Windows.Forms.AnchorStyles)(resources.GetObject("LblTitle.Anchor")));
			this.LblTitle.AutoSize = ((bool)(resources.GetObject("LblTitle.AutoSize")));
			this.LblTitle.Dock = ((System.Windows.Forms.DockStyle)(resources.GetObject("LblTitle.Dock")));
			this.LblTitle.Enabled = ((bool)(resources.GetObject("LblTitle.Enabled")));
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
			// LblQuestion
			// 
			this.LblQuestion.AccessibleDescription = resources.GetString("LblQuestion.AccessibleDescription");
			this.LblQuestion.AccessibleName = resources.GetString("LblQuestion.AccessibleName");
			this.LblQuestion.Anchor = ((System.Windows.Forms.AnchorStyles)(resources.GetObject("LblQuestion.Anchor")));
			this.LblQuestion.AutoSize = ((bool)(resources.GetObject("LblQuestion.AutoSize")));
			this.LblQuestion.Dock = ((System.Windows.Forms.DockStyle)(resources.GetObject("LblQuestion.Dock")));
			this.LblQuestion.Enabled = ((bool)(resources.GetObject("LblQuestion.Enabled")));
			this.LblQuestion.Font = ((System.Drawing.Font)(resources.GetObject("LblQuestion.Font")));
			this.LblQuestion.Image = ((System.Drawing.Image)(resources.GetObject("LblQuestion.Image")));
			this.LblQuestion.ImageAlign = ((System.Drawing.ContentAlignment)(resources.GetObject("LblQuestion.ImageAlign")));
			this.LblQuestion.ImageIndex = ((int)(resources.GetObject("LblQuestion.ImageIndex")));
			this.LblQuestion.ImeMode = ((System.Windows.Forms.ImeMode)(resources.GetObject("LblQuestion.ImeMode")));
			this.LblQuestion.Location = ((System.Drawing.Point)(resources.GetObject("LblQuestion.Location")));
			this.LblQuestion.Name = "LblQuestion";
			this.LblQuestion.RightToLeft = ((System.Windows.Forms.RightToLeft)(resources.GetObject("LblQuestion.RightToLeft")));
			this.LblQuestion.Size = ((System.Drawing.Size)(resources.GetObject("LblQuestion.Size")));
			this.LblQuestion.TabIndex = ((int)(resources.GetObject("LblQuestion.TabIndex")));
			this.LblQuestion.Text = resources.GetString("LblQuestion.Text");
			this.LblQuestion.TextAlign = ((System.Drawing.ContentAlignment)(resources.GetObject("LblQuestion.TextAlign")));
			this.LblQuestion.Visible = ((bool)(resources.GetObject("LblQuestion.Visible")));
			// 
			// MissingReferences
			// 
			this.AccessibleDescription = resources.GetString("$this.AccessibleDescription");
			this.AccessibleName = resources.GetString("$this.AccessibleName");
			this.AutoScaleBaseSize = ((System.Drawing.Size)(resources.GetObject("$this.AutoScaleBaseSize")));
			this.AutoScroll = ((bool)(resources.GetObject("$this.AutoScroll")));
			this.AutoScrollMargin = ((System.Drawing.Size)(resources.GetObject("$this.AutoScrollMargin")));
			this.AutoScrollMinSize = ((System.Drawing.Size)(resources.GetObject("$this.AutoScrollMinSize")));
			this.BackgroundImage = ((System.Drawing.Image)(resources.GetObject("$this.BackgroundImage")));
			this.ClientSize = ((System.Drawing.Size)(resources.GetObject("$this.ClientSize")));
			this.Controls.Add(this.LblQuestion);
			this.Controls.Add(this.LblTitle);
			this.Controls.Add(this.TxtMissingReferences);
			this.Controls.Add(this.CkbApplyAll);
			this.Controls.Add(this.BtnNo);
			this.Controls.Add(this.BtnYes);
			this.Enabled = ((bool)(resources.GetObject("$this.Enabled")));
			this.Font = ((System.Drawing.Font)(resources.GetObject("$this.Font")));
			this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedToolWindow;
			this.Icon = ((System.Drawing.Icon)(resources.GetObject("$this.Icon")));
			this.ImeMode = ((System.Windows.Forms.ImeMode)(resources.GetObject("$this.ImeMode")));
			this.Location = ((System.Drawing.Point)(resources.GetObject("$this.Location")));
			this.MaximumSize = ((System.Drawing.Size)(resources.GetObject("$this.MaximumSize")));
			this.MinimumSize = ((System.Drawing.Size)(resources.GetObject("$this.MinimumSize")));
			this.Name = "MissingReferences";
			this.RightToLeft = ((System.Windows.Forms.RightToLeft)(resources.GetObject("$this.RightToLeft")));
			this.ShowInTaskbar = false;
			this.StartPosition = ((System.Windows.Forms.FormStartPosition)(resources.GetObject("$this.StartPosition")));
			this.Text = resources.GetString("$this.Text");
			this.ResumeLayout(false);

		}
		#endregion
	}

	/// <summary>
	/// Informazioni relative al progetto ed alla sua reference non trovata
	/// </summary>
	//=========================================================================
	public struct MissingReference
	{
		public string ProjectName;
		public string ReferenceName;

		//---------------------------------------------------------------------
		public MissingReference(string project, string reference)
		{
			this.ProjectName	= project;
			this.ReferenceName	= reference;
		}

		//-----------------------------------------------------------------------------
		public override string ToString()
		{
			return String.Concat(ProjectName, " -> ", ReferenceName, Environment.NewLine);
		}

		//-----------------------------------------------------------------------------
		public override int GetHashCode()
		{
			return (ProjectName.GetHashCode() + ReferenceName.GetHashCode());
		}

		//-----------------------------------------------------------------------------
		public override bool Equals(object obj)
		{
			if (!(obj is MissingReference)) return false;
			MissingReference missingRef = (MissingReference)obj;

			return	(
				String.Compare(ProjectName, missingRef.ProjectName, true) == 0 &&
				String.Compare(ReferenceName, missingRef.ReferenceName, true) == 0) ;																  
		}
	}
}
