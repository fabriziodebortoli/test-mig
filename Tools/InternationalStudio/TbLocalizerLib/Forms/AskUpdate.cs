using System;
using System.ComponentModel;
using System.Windows.Forms;

namespace Microarea.Tools.TBLocalizer.Forms
{
	/// <summary>
	/// MessageBox, informa della presenza eventuale del dizionario e chiede se ricrearlo o updatarlo.
	/// </summary>
	public class AskUpdate : System.Windows.Forms.Form
	{
		private Container	components	= null;
		private Button		BtnUpdate;
		private Button		BtnCreate;
		private Button		BtnCancel;
		private Label		LblTitle;
		private CheckBox	CkbApplyAll;

		private bool		applyAll	= false;
		public	bool		ApplyAll	{ get {return applyAll;}}

		//---------------------------------------------------------------------
		public AskUpdate(string dictionary)
		{
			InitializeComponent();
			LblTitle.Text = String.Format(LblTitle.Text, dictionary);		
		}

		//---------------------------------------------------------------------
		private void BtnCancel_Click(object sender, System.EventArgs e)
		{
			DialogResult = DialogResult.Cancel;
			CloseWithResult();
		}

		//---------------------------------------------------------------------
		private void BtnUpdate_Click(object sender, System.EventArgs e)
		{
			DialogResult = DialogResult.Yes;
			CloseWithResult();
		}

		//---------------------------------------------------------------------
		private void BtnCreate_Click(object sender, System.EventArgs e)
		{
			DialogResult = DialogResult.No;
			CloseWithResult();
		}

		//Salva il valore della checkBox e chiude la form
		//---------------------------------------------------------------------
		private void CloseWithResult()
		{
			applyAll = CkbApplyAll.Checked;
			Close();
		}

		/// <summary>
		/// Clean up any resources being used.
		/// </summary>
		/// //---------------------------------------------------------------------
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
			System.Resources.ResourceManager resources = new System.Resources.ResourceManager(typeof(AskUpdate));
			this.BtnUpdate = new System.Windows.Forms.Button();
			this.BtnCreate = new System.Windows.Forms.Button();
			this.BtnCancel = new System.Windows.Forms.Button();
			this.LblTitle = new System.Windows.Forms.Label();
			this.CkbApplyAll = new System.Windows.Forms.CheckBox();
			this.SuspendLayout();
			// 
			// BtnUpdate
			// 
			this.BtnUpdate.AccessibleDescription = resources.GetString("BtnUpdate.AccessibleDescription");
			this.BtnUpdate.AccessibleName = resources.GetString("BtnUpdate.AccessibleName");
			this.BtnUpdate.Anchor = ((System.Windows.Forms.AnchorStyles)(resources.GetObject("BtnUpdate.Anchor")));
			this.BtnUpdate.BackgroundImage = ((System.Drawing.Image)(resources.GetObject("BtnUpdate.BackgroundImage")));
			this.BtnUpdate.Dock = ((System.Windows.Forms.DockStyle)(resources.GetObject("BtnUpdate.Dock")));
			this.BtnUpdate.Enabled = ((bool)(resources.GetObject("BtnUpdate.Enabled")));
			this.BtnUpdate.FlatStyle = ((System.Windows.Forms.FlatStyle)(resources.GetObject("BtnUpdate.FlatStyle")));
			this.BtnUpdate.Font = ((System.Drawing.Font)(resources.GetObject("BtnUpdate.Font")));
			this.BtnUpdate.Image = ((System.Drawing.Image)(resources.GetObject("BtnUpdate.Image")));
			this.BtnUpdate.ImageAlign = ((System.Drawing.ContentAlignment)(resources.GetObject("BtnUpdate.ImageAlign")));
			this.BtnUpdate.ImageIndex = ((int)(resources.GetObject("BtnUpdate.ImageIndex")));
			this.BtnUpdate.ImeMode = ((System.Windows.Forms.ImeMode)(resources.GetObject("BtnUpdate.ImeMode")));
			this.BtnUpdate.Location = ((System.Drawing.Point)(resources.GetObject("BtnUpdate.Location")));
			this.BtnUpdate.Name = "BtnUpdate";
			this.BtnUpdate.RightToLeft = ((System.Windows.Forms.RightToLeft)(resources.GetObject("BtnUpdate.RightToLeft")));
			this.BtnUpdate.Size = ((System.Drawing.Size)(resources.GetObject("BtnUpdate.Size")));
			this.BtnUpdate.TabIndex = ((int)(resources.GetObject("BtnUpdate.TabIndex")));
			this.BtnUpdate.Text = resources.GetString("BtnUpdate.Text");
			this.BtnUpdate.TextAlign = ((System.Drawing.ContentAlignment)(resources.GetObject("BtnUpdate.TextAlign")));
			this.BtnUpdate.Visible = ((bool)(resources.GetObject("BtnUpdate.Visible")));
			this.BtnUpdate.Click += new System.EventHandler(this.BtnUpdate_Click);
			// 
			// BtnCreate
			// 
			this.BtnCreate.AccessibleDescription = resources.GetString("BtnCreate.AccessibleDescription");
			this.BtnCreate.AccessibleName = resources.GetString("BtnCreate.AccessibleName");
			this.BtnCreate.Anchor = ((System.Windows.Forms.AnchorStyles)(resources.GetObject("BtnCreate.Anchor")));
			this.BtnCreate.BackgroundImage = ((System.Drawing.Image)(resources.GetObject("BtnCreate.BackgroundImage")));
			this.BtnCreate.Dock = ((System.Windows.Forms.DockStyle)(resources.GetObject("BtnCreate.Dock")));
			this.BtnCreate.Enabled = ((bool)(resources.GetObject("BtnCreate.Enabled")));
			this.BtnCreate.FlatStyle = ((System.Windows.Forms.FlatStyle)(resources.GetObject("BtnCreate.FlatStyle")));
			this.BtnCreate.Font = ((System.Drawing.Font)(resources.GetObject("BtnCreate.Font")));
			this.BtnCreate.Image = ((System.Drawing.Image)(resources.GetObject("BtnCreate.Image")));
			this.BtnCreate.ImageAlign = ((System.Drawing.ContentAlignment)(resources.GetObject("BtnCreate.ImageAlign")));
			this.BtnCreate.ImageIndex = ((int)(resources.GetObject("BtnCreate.ImageIndex")));
			this.BtnCreate.ImeMode = ((System.Windows.Forms.ImeMode)(resources.GetObject("BtnCreate.ImeMode")));
			this.BtnCreate.Location = ((System.Drawing.Point)(resources.GetObject("BtnCreate.Location")));
			this.BtnCreate.Name = "BtnCreate";
			this.BtnCreate.RightToLeft = ((System.Windows.Forms.RightToLeft)(resources.GetObject("BtnCreate.RightToLeft")));
			this.BtnCreate.Size = ((System.Drawing.Size)(resources.GetObject("BtnCreate.Size")));
			this.BtnCreate.TabIndex = ((int)(resources.GetObject("BtnCreate.TabIndex")));
			this.BtnCreate.Text = resources.GetString("BtnCreate.Text");
			this.BtnCreate.TextAlign = ((System.Drawing.ContentAlignment)(resources.GetObject("BtnCreate.TextAlign")));
			this.BtnCreate.Visible = ((bool)(resources.GetObject("BtnCreate.Visible")));
			this.BtnCreate.Click += new System.EventHandler(this.BtnCreate_Click);
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
			// 
			// AskUpdate
			// 
			this.AccessibleDescription = resources.GetString("$this.AccessibleDescription");
			this.AccessibleName = resources.GetString("$this.AccessibleName");
			this.AutoScaleBaseSize = ((System.Drawing.Size)(resources.GetObject("$this.AutoScaleBaseSize")));
			this.AutoScroll = ((bool)(resources.GetObject("$this.AutoScroll")));
			this.AutoScrollMargin = ((System.Drawing.Size)(resources.GetObject("$this.AutoScrollMargin")));
			this.AutoScrollMinSize = ((System.Drawing.Size)(resources.GetObject("$this.AutoScrollMinSize")));
			this.BackgroundImage = ((System.Drawing.Image)(resources.GetObject("$this.BackgroundImage")));
			this.ClientSize = ((System.Drawing.Size)(resources.GetObject("$this.ClientSize")));
			this.Controls.Add(this.CkbApplyAll);
			this.Controls.Add(this.LblTitle);
			this.Controls.Add(this.BtnCancel);
			this.Controls.Add(this.BtnCreate);
			this.Controls.Add(this.BtnUpdate);
			this.Enabled = ((bool)(resources.GetObject("$this.Enabled")));
			this.Font = ((System.Drawing.Font)(resources.GetObject("$this.Font")));
			this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedToolWindow;
			this.Icon = ((System.Drawing.Icon)(resources.GetObject("$this.Icon")));
			this.ImeMode = ((System.Windows.Forms.ImeMode)(resources.GetObject("$this.ImeMode")));
			this.Location = ((System.Drawing.Point)(resources.GetObject("$this.Location")));
			this.MaximumSize = ((System.Drawing.Size)(resources.GetObject("$this.MaximumSize")));
			this.MinimumSize = ((System.Drawing.Size)(resources.GetObject("$this.MinimumSize")));
			this.Name = "AskUpdate";
			this.RightToLeft = ((System.Windows.Forms.RightToLeft)(resources.GetObject("$this.RightToLeft")));
			this.ShowInTaskbar = false;
			this.StartPosition = ((System.Windows.Forms.FormStartPosition)(resources.GetObject("$this.StartPosition")));
			this.Text = resources.GetString("$this.Text");
			this.ResumeLayout(false);

		}
		#endregion

	}
}
