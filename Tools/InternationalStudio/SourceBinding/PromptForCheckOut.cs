using System.Windows.Forms;

namespace Microarea.Tools.TBLocalizer.SourceBinding
{
	//=========================================================================
	public class PromptForCheckOut : Form
	{
		#region Controls
		private Label PromptText;
		private CheckBox checkBoxHideNext;
		private Button Yes;
		private Button No;

		private System.ComponentModel.Container components = null;
		#endregion

		#region Private members
		private DialogResult defaultResult = DialogResult.No;
		#endregion

		#region Constructors
		//---------------------------------------------------------------------
		public PromptForCheckOut()
		{
			InitializeComponent();
		}
		#endregion

		#region Public methods
		//---------------------------------------------------------------------
		public DialogResult ShowDialog(IWin32Window owner, string prompt)
		{
			if (checkBoxHideNext.Checked)
				return defaultResult;

			PromptText.Text = prompt;

			return base.ShowDialog(owner);
		}
		#endregion

		#region Private methods
		/// <summary>
		/// Clean up any resources being used.
		/// </summary>
		//---------------------------------------------------------------------
		protected override void Dispose( bool disposing )
		{
			if (disposing)
			{
				if (components != null)
				{
					components.Dispose();
				}
			}

			base.Dispose(disposing);
		}

		//---------------------------------------------------------------------
		private void PromptForCheckOut_Closing(object sender, System.ComponentModel.CancelEventArgs e)
		{
			defaultResult = DialogResult;
		}
		#endregion

		#region Windows Form Designer generated code
		/// <summary>
		/// Required method for Designer support - do not modify
		/// the contents of this method with the code editor.
		/// </summary>
		//--------------------------------------------------------------------------------
		private void InitializeComponent()
		{
			System.Resources.ResourceManager resources = new System.Resources.ResourceManager(typeof(PromptForCheckOut));
			this.PromptText = new System.Windows.Forms.Label();
			this.checkBoxHideNext = new System.Windows.Forms.CheckBox();
			this.Yes = new System.Windows.Forms.Button();
			this.No = new System.Windows.Forms.Button();
			this.SuspendLayout();
			// 
			// PromptText
			// 
			this.PromptText.AccessibleDescription = resources.GetString("PromptText.AccessibleDescription");
			this.PromptText.AccessibleName = resources.GetString("PromptText.AccessibleName");
			this.PromptText.Anchor = ((System.Windows.Forms.AnchorStyles)(resources.GetObject("PromptText.Anchor")));
			this.PromptText.AutoSize = ((bool)(resources.GetObject("PromptText.AutoSize")));
			this.PromptText.Dock = ((System.Windows.Forms.DockStyle)(resources.GetObject("PromptText.Dock")));
			this.PromptText.Enabled = ((bool)(resources.GetObject("PromptText.Enabled")));
			this.PromptText.FlatStyle = System.Windows.Forms.FlatStyle.System;
			this.PromptText.Font = ((System.Drawing.Font)(resources.GetObject("PromptText.Font")));
			this.PromptText.Image = ((System.Drawing.Image)(resources.GetObject("PromptText.Image")));
			this.PromptText.ImageAlign = ((System.Drawing.ContentAlignment)(resources.GetObject("PromptText.ImageAlign")));
			this.PromptText.ImageIndex = ((int)(resources.GetObject("PromptText.ImageIndex")));
			this.PromptText.ImeMode = ((System.Windows.Forms.ImeMode)(resources.GetObject("PromptText.ImeMode")));
			this.PromptText.Location = ((System.Drawing.Point)(resources.GetObject("PromptText.Location")));
			this.PromptText.Name = "PromptText";
			this.PromptText.RightToLeft = ((System.Windows.Forms.RightToLeft)(resources.GetObject("PromptText.RightToLeft")));
			this.PromptText.Size = ((System.Drawing.Size)(resources.GetObject("PromptText.Size")));
			this.PromptText.TabIndex = ((int)(resources.GetObject("PromptText.TabIndex")));
			this.PromptText.Text = resources.GetString("PromptText.Text");
			this.PromptText.TextAlign = ((System.Drawing.ContentAlignment)(resources.GetObject("PromptText.TextAlign")));
			this.PromptText.Visible = ((bool)(resources.GetObject("PromptText.Visible")));
			// 
			// checkBoxHideNext
			// 
			this.checkBoxHideNext.AccessibleDescription = resources.GetString("checkBoxHideNext.AccessibleDescription");
			this.checkBoxHideNext.AccessibleName = resources.GetString("checkBoxHideNext.AccessibleName");
			this.checkBoxHideNext.Anchor = ((System.Windows.Forms.AnchorStyles)(resources.GetObject("checkBoxHideNext.Anchor")));
			this.checkBoxHideNext.Appearance = ((System.Windows.Forms.Appearance)(resources.GetObject("checkBoxHideNext.Appearance")));
			this.checkBoxHideNext.BackgroundImage = ((System.Drawing.Image)(resources.GetObject("checkBoxHideNext.BackgroundImage")));
			this.checkBoxHideNext.CheckAlign = ((System.Drawing.ContentAlignment)(resources.GetObject("checkBoxHideNext.CheckAlign")));
			this.checkBoxHideNext.Dock = ((System.Windows.Forms.DockStyle)(resources.GetObject("checkBoxHideNext.Dock")));
			this.checkBoxHideNext.Enabled = ((bool)(resources.GetObject("checkBoxHideNext.Enabled")));
			this.checkBoxHideNext.FlatStyle = ((System.Windows.Forms.FlatStyle)(resources.GetObject("checkBoxHideNext.FlatStyle")));
			this.checkBoxHideNext.Font = ((System.Drawing.Font)(resources.GetObject("checkBoxHideNext.Font")));
			this.checkBoxHideNext.Image = ((System.Drawing.Image)(resources.GetObject("checkBoxHideNext.Image")));
			this.checkBoxHideNext.ImageAlign = ((System.Drawing.ContentAlignment)(resources.GetObject("checkBoxHideNext.ImageAlign")));
			this.checkBoxHideNext.ImageIndex = ((int)(resources.GetObject("checkBoxHideNext.ImageIndex")));
			this.checkBoxHideNext.ImeMode = ((System.Windows.Forms.ImeMode)(resources.GetObject("checkBoxHideNext.ImeMode")));
			this.checkBoxHideNext.Location = ((System.Drawing.Point)(resources.GetObject("checkBoxHideNext.Location")));
			this.checkBoxHideNext.Name = "checkBoxHideNext";
			this.checkBoxHideNext.RightToLeft = ((System.Windows.Forms.RightToLeft)(resources.GetObject("checkBoxHideNext.RightToLeft")));
			this.checkBoxHideNext.Size = ((System.Drawing.Size)(resources.GetObject("checkBoxHideNext.Size")));
			this.checkBoxHideNext.TabIndex = ((int)(resources.GetObject("checkBoxHideNext.TabIndex")));
			this.checkBoxHideNext.Text = resources.GetString("checkBoxHideNext.Text");
			this.checkBoxHideNext.TextAlign = ((System.Drawing.ContentAlignment)(resources.GetObject("checkBoxHideNext.TextAlign")));
			this.checkBoxHideNext.Visible = ((bool)(resources.GetObject("checkBoxHideNext.Visible")));
			// 
			// Yes
			// 
			this.Yes.AccessibleDescription = resources.GetString("Yes.AccessibleDescription");
			this.Yes.AccessibleName = resources.GetString("Yes.AccessibleName");
			this.Yes.Anchor = ((System.Windows.Forms.AnchorStyles)(resources.GetObject("Yes.Anchor")));
			this.Yes.BackgroundImage = ((System.Drawing.Image)(resources.GetObject("Yes.BackgroundImage")));
			this.Yes.DialogResult = System.Windows.Forms.DialogResult.Yes;
			this.Yes.Dock = ((System.Windows.Forms.DockStyle)(resources.GetObject("Yes.Dock")));
			this.Yes.Enabled = ((bool)(resources.GetObject("Yes.Enabled")));
			this.Yes.FlatStyle = ((System.Windows.Forms.FlatStyle)(resources.GetObject("Yes.FlatStyle")));
			this.Yes.Font = ((System.Drawing.Font)(resources.GetObject("Yes.Font")));
			this.Yes.Image = ((System.Drawing.Image)(resources.GetObject("Yes.Image")));
			this.Yes.ImageAlign = ((System.Drawing.ContentAlignment)(resources.GetObject("Yes.ImageAlign")));
			this.Yes.ImageIndex = ((int)(resources.GetObject("Yes.ImageIndex")));
			this.Yes.ImeMode = ((System.Windows.Forms.ImeMode)(resources.GetObject("Yes.ImeMode")));
			this.Yes.Location = ((System.Drawing.Point)(resources.GetObject("Yes.Location")));
			this.Yes.Name = "Yes";
			this.Yes.RightToLeft = ((System.Windows.Forms.RightToLeft)(resources.GetObject("Yes.RightToLeft")));
			this.Yes.Size = ((System.Drawing.Size)(resources.GetObject("Yes.Size")));
			this.Yes.TabIndex = ((int)(resources.GetObject("Yes.TabIndex")));
			this.Yes.Text = resources.GetString("Yes.Text");
			this.Yes.TextAlign = ((System.Drawing.ContentAlignment)(resources.GetObject("Yes.TextAlign")));
			this.Yes.Visible = ((bool)(resources.GetObject("Yes.Visible")));
			// 
			// No
			// 
			this.No.AccessibleDescription = resources.GetString("No.AccessibleDescription");
			this.No.AccessibleName = resources.GetString("No.AccessibleName");
			this.No.Anchor = ((System.Windows.Forms.AnchorStyles)(resources.GetObject("No.Anchor")));
			this.No.BackgroundImage = ((System.Drawing.Image)(resources.GetObject("No.BackgroundImage")));
			this.No.DialogResult = System.Windows.Forms.DialogResult.No;
			this.No.Dock = ((System.Windows.Forms.DockStyle)(resources.GetObject("No.Dock")));
			this.No.Enabled = ((bool)(resources.GetObject("No.Enabled")));
			this.No.FlatStyle = ((System.Windows.Forms.FlatStyle)(resources.GetObject("No.FlatStyle")));
			this.No.Font = ((System.Drawing.Font)(resources.GetObject("No.Font")));
			this.No.Image = ((System.Drawing.Image)(resources.GetObject("No.Image")));
			this.No.ImageAlign = ((System.Drawing.ContentAlignment)(resources.GetObject("No.ImageAlign")));
			this.No.ImageIndex = ((int)(resources.GetObject("No.ImageIndex")));
			this.No.ImeMode = ((System.Windows.Forms.ImeMode)(resources.GetObject("No.ImeMode")));
			this.No.Location = ((System.Drawing.Point)(resources.GetObject("No.Location")));
			this.No.Name = "No";
			this.No.RightToLeft = ((System.Windows.Forms.RightToLeft)(resources.GetObject("No.RightToLeft")));
			this.No.Size = ((System.Drawing.Size)(resources.GetObject("No.Size")));
			this.No.TabIndex = ((int)(resources.GetObject("No.TabIndex")));
			this.No.Text = resources.GetString("No.Text");
			this.No.TextAlign = ((System.Drawing.ContentAlignment)(resources.GetObject("No.TextAlign")));
			this.No.Visible = ((bool)(resources.GetObject("No.Visible")));
			// 
			// PromptForCheckOut
			// 
			this.AccessibleDescription = resources.GetString("$this.AccessibleDescription");
			this.AccessibleName = resources.GetString("$this.AccessibleName");
			this.AutoScaleBaseSize = ((System.Drawing.Size)(resources.GetObject("$this.AutoScaleBaseSize")));
			this.AutoScroll = ((bool)(resources.GetObject("$this.AutoScroll")));
			this.AutoScrollMargin = ((System.Drawing.Size)(resources.GetObject("$this.AutoScrollMargin")));
			this.AutoScrollMinSize = ((System.Drawing.Size)(resources.GetObject("$this.AutoScrollMinSize")));
			this.BackgroundImage = ((System.Drawing.Image)(resources.GetObject("$this.BackgroundImage")));
			this.ClientSize = ((System.Drawing.Size)(resources.GetObject("$this.ClientSize")));
			this.Controls.Add(this.Yes);
			this.Controls.Add(this.checkBoxHideNext);
			this.Controls.Add(this.PromptText);
			this.Controls.Add(this.No);
			this.Enabled = ((bool)(resources.GetObject("$this.Enabled")));
			this.Font = ((System.Drawing.Font)(resources.GetObject("$this.Font")));
			this.Icon = ((System.Drawing.Icon)(resources.GetObject("$this.Icon")));
			this.ImeMode = ((System.Windows.Forms.ImeMode)(resources.GetObject("$this.ImeMode")));
			this.Location = ((System.Drawing.Point)(resources.GetObject("$this.Location")));
			this.MaximumSize = ((System.Drawing.Size)(resources.GetObject("$this.MaximumSize")));
			this.MinimumSize = ((System.Drawing.Size)(resources.GetObject("$this.MinimumSize")));
			this.Name = "PromptForCheckOut";
			this.RightToLeft = ((System.Windows.Forms.RightToLeft)(resources.GetObject("$this.RightToLeft")));
			this.ShowInTaskbar = false;
			this.StartPosition = ((System.Windows.Forms.FormStartPosition)(resources.GetObject("$this.StartPosition")));
			this.Text = resources.GetString("$this.Text");
			this.Closing += new System.ComponentModel.CancelEventHandler(this.PromptForCheckOut_Closing);
			this.ResumeLayout(false);

		}
		#endregion
	}
}
