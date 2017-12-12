using System.Collections;
using System.Drawing;

namespace Microarea.Tools.TBLocalizer.SourceBinding
{
	/// <summary>
	/// Summary description for Summary.
	/// </summary>
	//================================================================================
	public class Summary : System.Windows.Forms.Form
	{
		private System.Windows.Forms.Button OK;
		private System.Windows.Forms.Button Cancel;
		public System.Windows.Forms.TextBox TxtActions;
		/// <summary>
		/// Required designer variable.
		/// </summary>
		private System.ComponentModel.Container components = null;
		private System.Windows.Forms.Label labelCaption;
		private System.Windows.Forms.Label labelQuestion;
		private ArrayList messageList;

		//--------------------------------------------------------------------------------
		public Summary(ArrayList messageList, string caption, string question, bool ask)
		{
			//
			// Required for Windows Form Designer support
			//
			InitializeComponent();

			this.messageList = messageList;
			if (caption != null)
				this.labelCaption.Text = caption;

			if (question != null)
				this.labelQuestion.Text = question;

			if (!ask)
			{
				OK.Visible = false;
				Cancel.Text = Strings.Ok;
				Cancel.Location = new Point((this.Width / 2 - Cancel.Width / 2), Cancel.Location.Y); 
			}
		}
		
		//--------------------------------------------------------------------------------
		public Summary(ArrayList messageList) : this (messageList, null, null, true)
		{
		}

		//--------------------------------------------------------------------------------
		public Summary(ArrayList messageList, string caption) : this (messageList, caption, null, true)
		{	
		}

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
			System.Resources.ResourceManager resources = new System.Resources.ResourceManager(typeof(Summary));
			this.labelCaption = new System.Windows.Forms.Label();
			this.labelQuestion = new System.Windows.Forms.Label();
			this.OK = new System.Windows.Forms.Button();
			this.Cancel = new System.Windows.Forms.Button();
			this.TxtActions = new System.Windows.Forms.TextBox();
			this.SuspendLayout();
			// 
			// labelCaption
			// 
			this.labelCaption.AccessibleDescription = resources.GetString("labelCaption.AccessibleDescription");
			this.labelCaption.AccessibleName = resources.GetString("labelCaption.AccessibleName");
			this.labelCaption.Anchor = ((System.Windows.Forms.AnchorStyles)(resources.GetObject("labelCaption.Anchor")));
			this.labelCaption.AutoSize = ((bool)(resources.GetObject("labelCaption.AutoSize")));
			this.labelCaption.Dock = ((System.Windows.Forms.DockStyle)(resources.GetObject("labelCaption.Dock")));
			this.labelCaption.Enabled = ((bool)(resources.GetObject("labelCaption.Enabled")));
			this.labelCaption.Font = ((System.Drawing.Font)(resources.GetObject("labelCaption.Font")));
			this.labelCaption.Image = ((System.Drawing.Image)(resources.GetObject("labelCaption.Image")));
			this.labelCaption.ImageAlign = ((System.Drawing.ContentAlignment)(resources.GetObject("labelCaption.ImageAlign")));
			this.labelCaption.ImageIndex = ((int)(resources.GetObject("labelCaption.ImageIndex")));
			this.labelCaption.ImeMode = ((System.Windows.Forms.ImeMode)(resources.GetObject("labelCaption.ImeMode")));
			this.labelCaption.Location = ((System.Drawing.Point)(resources.GetObject("labelCaption.Location")));
			this.labelCaption.Name = "labelCaption";
			this.labelCaption.RightToLeft = ((System.Windows.Forms.RightToLeft)(resources.GetObject("labelCaption.RightToLeft")));
			this.labelCaption.Size = ((System.Drawing.Size)(resources.GetObject("labelCaption.Size")));
			this.labelCaption.TabIndex = ((int)(resources.GetObject("labelCaption.TabIndex")));
			this.labelCaption.Text = resources.GetString("labelCaption.Text");
			this.labelCaption.TextAlign = ((System.Drawing.ContentAlignment)(resources.GetObject("labelCaption.TextAlign")));
			this.labelCaption.Visible = ((bool)(resources.GetObject("labelCaption.Visible")));
			// 
			// labelQuestion
			// 
			this.labelQuestion.AccessibleDescription = resources.GetString("labelQuestion.AccessibleDescription");
			this.labelQuestion.AccessibleName = resources.GetString("labelQuestion.AccessibleName");
			this.labelQuestion.Anchor = ((System.Windows.Forms.AnchorStyles)(resources.GetObject("labelQuestion.Anchor")));
			this.labelQuestion.AutoSize = ((bool)(resources.GetObject("labelQuestion.AutoSize")));
			this.labelQuestion.Dock = ((System.Windows.Forms.DockStyle)(resources.GetObject("labelQuestion.Dock")));
			this.labelQuestion.Enabled = ((bool)(resources.GetObject("labelQuestion.Enabled")));
			this.labelQuestion.Font = ((System.Drawing.Font)(resources.GetObject("labelQuestion.Font")));
			this.labelQuestion.Image = ((System.Drawing.Image)(resources.GetObject("labelQuestion.Image")));
			this.labelQuestion.ImageAlign = ((System.Drawing.ContentAlignment)(resources.GetObject("labelQuestion.ImageAlign")));
			this.labelQuestion.ImageIndex = ((int)(resources.GetObject("labelQuestion.ImageIndex")));
			this.labelQuestion.ImeMode = ((System.Windows.Forms.ImeMode)(resources.GetObject("labelQuestion.ImeMode")));
			this.labelQuestion.Location = ((System.Drawing.Point)(resources.GetObject("labelQuestion.Location")));
			this.labelQuestion.Name = "labelQuestion";
			this.labelQuestion.RightToLeft = ((System.Windows.Forms.RightToLeft)(resources.GetObject("labelQuestion.RightToLeft")));
			this.labelQuestion.Size = ((System.Drawing.Size)(resources.GetObject("labelQuestion.Size")));
			this.labelQuestion.TabIndex = ((int)(resources.GetObject("labelQuestion.TabIndex")));
			this.labelQuestion.Text = resources.GetString("labelQuestion.Text");
			this.labelQuestion.TextAlign = ((System.Drawing.ContentAlignment)(resources.GetObject("labelQuestion.TextAlign")));
			this.labelQuestion.Visible = ((bool)(resources.GetObject("labelQuestion.Visible")));
			// 
			// OK
			// 
			this.OK.AccessibleDescription = resources.GetString("OK.AccessibleDescription");
			this.OK.AccessibleName = resources.GetString("OK.AccessibleName");
			this.OK.Anchor = ((System.Windows.Forms.AnchorStyles)(resources.GetObject("OK.Anchor")));
			this.OK.BackgroundImage = ((System.Drawing.Image)(resources.GetObject("OK.BackgroundImage")));
			this.OK.DialogResult = System.Windows.Forms.DialogResult.Yes;
			this.OK.Dock = ((System.Windows.Forms.DockStyle)(resources.GetObject("OK.Dock")));
			this.OK.Enabled = ((bool)(resources.GetObject("OK.Enabled")));
			this.OK.FlatStyle = ((System.Windows.Forms.FlatStyle)(resources.GetObject("OK.FlatStyle")));
			this.OK.Font = ((System.Drawing.Font)(resources.GetObject("OK.Font")));
			this.OK.Image = ((System.Drawing.Image)(resources.GetObject("OK.Image")));
			this.OK.ImageAlign = ((System.Drawing.ContentAlignment)(resources.GetObject("OK.ImageAlign")));
			this.OK.ImageIndex = ((int)(resources.GetObject("OK.ImageIndex")));
			this.OK.ImeMode = ((System.Windows.Forms.ImeMode)(resources.GetObject("OK.ImeMode")));
			this.OK.Location = ((System.Drawing.Point)(resources.GetObject("OK.Location")));
			this.OK.Name = "OK";
			this.OK.RightToLeft = ((System.Windows.Forms.RightToLeft)(resources.GetObject("OK.RightToLeft")));
			this.OK.Size = ((System.Drawing.Size)(resources.GetObject("OK.Size")));
			this.OK.TabIndex = ((int)(resources.GetObject("OK.TabIndex")));
			this.OK.Text = resources.GetString("OK.Text");
			this.OK.TextAlign = ((System.Drawing.ContentAlignment)(resources.GetObject("OK.TextAlign")));
			this.OK.Visible = ((bool)(resources.GetObject("OK.Visible")));
			// 
			// Cancel
			// 
			this.Cancel.AccessibleDescription = resources.GetString("Cancel.AccessibleDescription");
			this.Cancel.AccessibleName = resources.GetString("Cancel.AccessibleName");
			this.Cancel.Anchor = ((System.Windows.Forms.AnchorStyles)(resources.GetObject("Cancel.Anchor")));
			this.Cancel.BackgroundImage = ((System.Drawing.Image)(resources.GetObject("Cancel.BackgroundImage")));
			this.Cancel.DialogResult = System.Windows.Forms.DialogResult.No;
			this.Cancel.Dock = ((System.Windows.Forms.DockStyle)(resources.GetObject("Cancel.Dock")));
			this.Cancel.Enabled = ((bool)(resources.GetObject("Cancel.Enabled")));
			this.Cancel.FlatStyle = ((System.Windows.Forms.FlatStyle)(resources.GetObject("Cancel.FlatStyle")));
			this.Cancel.Font = ((System.Drawing.Font)(resources.GetObject("Cancel.Font")));
			this.Cancel.Image = ((System.Drawing.Image)(resources.GetObject("Cancel.Image")));
			this.Cancel.ImageAlign = ((System.Drawing.ContentAlignment)(resources.GetObject("Cancel.ImageAlign")));
			this.Cancel.ImageIndex = ((int)(resources.GetObject("Cancel.ImageIndex")));
			this.Cancel.ImeMode = ((System.Windows.Forms.ImeMode)(resources.GetObject("Cancel.ImeMode")));
			this.Cancel.Location = ((System.Drawing.Point)(resources.GetObject("Cancel.Location")));
			this.Cancel.Name = "Cancel";
			this.Cancel.RightToLeft = ((System.Windows.Forms.RightToLeft)(resources.GetObject("Cancel.RightToLeft")));
			this.Cancel.Size = ((System.Drawing.Size)(resources.GetObject("Cancel.Size")));
			this.Cancel.TabIndex = ((int)(resources.GetObject("Cancel.TabIndex")));
			this.Cancel.Text = resources.GetString("Cancel.Text");
			this.Cancel.TextAlign = ((System.Drawing.ContentAlignment)(resources.GetObject("Cancel.TextAlign")));
			this.Cancel.Visible = ((bool)(resources.GetObject("Cancel.Visible")));
			// 
			// TxtActions
			// 
			this.TxtActions.AccessibleDescription = resources.GetString("TxtActions.AccessibleDescription");
			this.TxtActions.AccessibleName = resources.GetString("TxtActions.AccessibleName");
			this.TxtActions.Anchor = ((System.Windows.Forms.AnchorStyles)(resources.GetObject("TxtActions.Anchor")));
			this.TxtActions.AutoSize = ((bool)(resources.GetObject("TxtActions.AutoSize")));
			this.TxtActions.BackgroundImage = ((System.Drawing.Image)(resources.GetObject("TxtActions.BackgroundImage")));
			this.TxtActions.Dock = ((System.Windows.Forms.DockStyle)(resources.GetObject("TxtActions.Dock")));
			this.TxtActions.Enabled = ((bool)(resources.GetObject("TxtActions.Enabled")));
			this.TxtActions.Font = ((System.Drawing.Font)(resources.GetObject("TxtActions.Font")));
			this.TxtActions.ImeMode = ((System.Windows.Forms.ImeMode)(resources.GetObject("TxtActions.ImeMode")));
			this.TxtActions.Location = ((System.Drawing.Point)(resources.GetObject("TxtActions.Location")));
			this.TxtActions.MaxLength = ((int)(resources.GetObject("TxtActions.MaxLength")));
			this.TxtActions.Multiline = ((bool)(resources.GetObject("TxtActions.Multiline")));
			this.TxtActions.Name = "TxtActions";
			this.TxtActions.PasswordChar = ((char)(resources.GetObject("TxtActions.PasswordChar")));
			this.TxtActions.ReadOnly = true;
			this.TxtActions.RightToLeft = ((System.Windows.Forms.RightToLeft)(resources.GetObject("TxtActions.RightToLeft")));
			this.TxtActions.ScrollBars = ((System.Windows.Forms.ScrollBars)(resources.GetObject("TxtActions.ScrollBars")));
			this.TxtActions.Size = ((System.Drawing.Size)(resources.GetObject("TxtActions.Size")));
			this.TxtActions.TabIndex = ((int)(resources.GetObject("TxtActions.TabIndex")));
			this.TxtActions.Text = resources.GetString("TxtActions.Text");
			this.TxtActions.TextAlign = ((System.Windows.Forms.HorizontalAlignment)(resources.GetObject("TxtActions.TextAlign")));
			this.TxtActions.Visible = ((bool)(resources.GetObject("TxtActions.Visible")));
			this.TxtActions.WordWrap = ((bool)(resources.GetObject("TxtActions.WordWrap")));
			// 
			// Summary
			// 
			this.AccessibleDescription = resources.GetString("$this.AccessibleDescription");
			this.AccessibleName = resources.GetString("$this.AccessibleName");
			this.AutoScaleBaseSize = ((System.Drawing.Size)(resources.GetObject("$this.AutoScaleBaseSize")));
			this.AutoScroll = ((bool)(resources.GetObject("$this.AutoScroll")));
			this.AutoScrollMargin = ((System.Drawing.Size)(resources.GetObject("$this.AutoScrollMargin")));
			this.AutoScrollMinSize = ((System.Drawing.Size)(resources.GetObject("$this.AutoScrollMinSize")));
			this.BackgroundImage = ((System.Drawing.Image)(resources.GetObject("$this.BackgroundImage")));
			this.ClientSize = ((System.Drawing.Size)(resources.GetObject("$this.ClientSize")));
			this.Controls.Add(this.TxtActions);
			this.Controls.Add(this.OK);
			this.Controls.Add(this.Cancel);
			this.Controls.Add(this.labelCaption);
			this.Controls.Add(this.labelQuestion);
			this.Enabled = ((bool)(resources.GetObject("$this.Enabled")));
			this.Font = ((System.Drawing.Font)(resources.GetObject("$this.Font")));
			this.Icon = ((System.Drawing.Icon)(resources.GetObject("$this.Icon")));
			this.ImeMode = ((System.Windows.Forms.ImeMode)(resources.GetObject("$this.ImeMode")));
			this.Location = ((System.Drawing.Point)(resources.GetObject("$this.Location")));
			this.MaximumSize = ((System.Drawing.Size)(resources.GetObject("$this.MaximumSize")));
			this.MinimumSize = ((System.Drawing.Size)(resources.GetObject("$this.MinimumSize")));
			this.Name = "Summary";
			this.RightToLeft = ((System.Windows.Forms.RightToLeft)(resources.GetObject("$this.RightToLeft")));
			this.ShowInTaskbar = false;
			this.StartPosition = ((System.Windows.Forms.FormStartPosition)(resources.GetObject("$this.StartPosition")));
			this.Text = resources.GetString("$this.Text");
			this.Load += new System.EventHandler(this.Summary_Load);
			this.ResumeLayout(false);

		}
		#endregion

		//--------------------------------------------------------------------------------
		private void Summary_Load(object sender, System.EventArgs e)
		{
			if (messageList.Count == 0)
			{
				TxtActions.Text = Strings.NoActionNeeded;
				return;
			}

			messageList.Sort();

			System.Text.StringBuilder sb = new System.Text.StringBuilder();

			foreach (string line in messageList)
			{
				sb.Append(line);
				sb.Append("\r\n");
			}

			TxtActions.Text += sb.ToString();
		}
	}
}
