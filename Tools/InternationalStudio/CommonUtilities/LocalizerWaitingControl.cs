
namespace Microarea.Tools.TBLocalizer.CommonUtilities
{
	/// <summary>
	/// Summary description for WaitingWindow.
	/// </summary>
	public class LocalizerWaitingControl : System.Windows.Forms.UserControl
	{
		private System.Windows.Forms.PictureBox pictureBox1;
		private System.Windows.Forms.PictureBox pictureBox2;
		
		private System.Windows.Forms.Label lblMessage;
		/// <summary>
		/// Required designer variable.
		/// </summary>
		private System.ComponentModel.Container components = null;

		//------------------------------------------------------------------------------------
		public string Message { get { return lblMessage.Text;} set { lblMessage.Text = value; } }

		public LocalizerWaitingControl()
		{
			//
			// Required for Windows Form Designer support
			//
			InitializeComponent();

			//
			// TODO: Add any constructor code after InitializeComponent call
			//
		}

		/// <summary>
		/// Clean up any resources being used.
		/// </summary>
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
		private void InitializeComponent()
		{
			System.Resources.ResourceManager resources = new System.Resources.ResourceManager(typeof(LocalizerWaitingControl));
			this.pictureBox1 = new System.Windows.Forms.PictureBox();
			this.pictureBox2 = new System.Windows.Forms.PictureBox();
			this.lblMessage = new System.Windows.Forms.Label();
			this.SuspendLayout();
			// 
			// pictureBox1
			// 
			this.pictureBox1.AccessibleDescription = resources.GetString("pictureBox1.AccessibleDescription");
			this.pictureBox1.AccessibleName = resources.GetString("pictureBox1.AccessibleName");
			this.pictureBox1.Anchor = ((System.Windows.Forms.AnchorStyles)(resources.GetObject("pictureBox1.Anchor")));
			this.pictureBox1.BackgroundImage = ((System.Drawing.Image)(resources.GetObject("pictureBox1.BackgroundImage")));
			this.pictureBox1.Dock = ((System.Windows.Forms.DockStyle)(resources.GetObject("pictureBox1.Dock")));
			this.pictureBox1.Enabled = ((bool)(resources.GetObject("pictureBox1.Enabled")));
			this.pictureBox1.Font = ((System.Drawing.Font)(resources.GetObject("pictureBox1.Font")));
			this.pictureBox1.Image = ((System.Drawing.Image)(resources.GetObject("pictureBox1.Image")));
			this.pictureBox1.ImeMode = ((System.Windows.Forms.ImeMode)(resources.GetObject("pictureBox1.ImeMode")));
			this.pictureBox1.Location = ((System.Drawing.Point)(resources.GetObject("pictureBox1.Location")));
			this.pictureBox1.Name = "pictureBox1";
			this.pictureBox1.RightToLeft = ((System.Windows.Forms.RightToLeft)(resources.GetObject("pictureBox1.RightToLeft")));
			this.pictureBox1.Size = ((System.Drawing.Size)(resources.GetObject("pictureBox1.Size")));
			this.pictureBox1.SizeMode = ((System.Windows.Forms.PictureBoxSizeMode)(resources.GetObject("pictureBox1.SizeMode")));
			this.pictureBox1.TabIndex = ((int)(resources.GetObject("pictureBox1.TabIndex")));
			this.pictureBox1.TabStop = false;
			this.pictureBox1.Text = resources.GetString("pictureBox1.Text");
			this.pictureBox1.Visible = ((bool)(resources.GetObject("pictureBox1.Visible")));
			// 
			// pictureBox2
			// 
			this.pictureBox2.AccessibleDescription = resources.GetString("pictureBox2.AccessibleDescription");
			this.pictureBox2.AccessibleName = resources.GetString("pictureBox2.AccessibleName");
			this.pictureBox2.Anchor = ((System.Windows.Forms.AnchorStyles)(resources.GetObject("pictureBox2.Anchor")));
			this.pictureBox2.BackgroundImage = ((System.Drawing.Image)(resources.GetObject("pictureBox2.BackgroundImage")));
			this.pictureBox2.Dock = ((System.Windows.Forms.DockStyle)(resources.GetObject("pictureBox2.Dock")));
			this.pictureBox2.Enabled = ((bool)(resources.GetObject("pictureBox2.Enabled")));
			this.pictureBox2.Font = ((System.Drawing.Font)(resources.GetObject("pictureBox2.Font")));
			this.pictureBox2.Image = ((System.Drawing.Image)(resources.GetObject("pictureBox2.Image")));
			this.pictureBox2.ImeMode = ((System.Windows.Forms.ImeMode)(resources.GetObject("pictureBox2.ImeMode")));
			this.pictureBox2.Location = ((System.Drawing.Point)(resources.GetObject("pictureBox2.Location")));
			this.pictureBox2.Name = "pictureBox2";
			this.pictureBox2.RightToLeft = ((System.Windows.Forms.RightToLeft)(resources.GetObject("pictureBox2.RightToLeft")));
			this.pictureBox2.Size = ((System.Drawing.Size)(resources.GetObject("pictureBox2.Size")));
			this.pictureBox2.SizeMode = ((System.Windows.Forms.PictureBoxSizeMode)(resources.GetObject("pictureBox2.SizeMode")));
			this.pictureBox2.TabIndex = ((int)(resources.GetObject("pictureBox2.TabIndex")));
			this.pictureBox2.TabStop = false;
			this.pictureBox2.Text = resources.GetString("pictureBox2.Text");
			this.pictureBox2.Visible = ((bool)(resources.GetObject("pictureBox2.Visible")));
			// 
			// lblMessage
			// 
			this.lblMessage.AccessibleDescription = resources.GetString("lblMessage.AccessibleDescription");
			this.lblMessage.AccessibleName = resources.GetString("lblMessage.AccessibleName");
			this.lblMessage.Anchor = ((System.Windows.Forms.AnchorStyles)(resources.GetObject("lblMessage.Anchor")));
			this.lblMessage.AutoSize = ((bool)(resources.GetObject("lblMessage.AutoSize")));
			this.lblMessage.Dock = ((System.Windows.Forms.DockStyle)(resources.GetObject("lblMessage.Dock")));
			this.lblMessage.Enabled = ((bool)(resources.GetObject("lblMessage.Enabled")));
			this.lblMessage.Font = ((System.Drawing.Font)(resources.GetObject("lblMessage.Font")));
			this.lblMessage.Image = ((System.Drawing.Image)(resources.GetObject("lblMessage.Image")));
			this.lblMessage.ImageAlign = ((System.Drawing.ContentAlignment)(resources.GetObject("lblMessage.ImageAlign")));
			this.lblMessage.ImageIndex = ((int)(resources.GetObject("lblMessage.ImageIndex")));
			this.lblMessage.ImeMode = ((System.Windows.Forms.ImeMode)(resources.GetObject("lblMessage.ImeMode")));
			this.lblMessage.Location = ((System.Drawing.Point)(resources.GetObject("lblMessage.Location")));
			this.lblMessage.Name = "lblMessage";
			this.lblMessage.RightToLeft = ((System.Windows.Forms.RightToLeft)(resources.GetObject("lblMessage.RightToLeft")));
			this.lblMessage.Size = ((System.Drawing.Size)(resources.GetObject("lblMessage.Size")));
			this.lblMessage.TabIndex = ((int)(resources.GetObject("lblMessage.TabIndex")));
			this.lblMessage.Text = resources.GetString("lblMessage.Text");
			this.lblMessage.TextAlign = ((System.Drawing.ContentAlignment)(resources.GetObject("lblMessage.TextAlign")));
			this.lblMessage.Visible = ((bool)(resources.GetObject("lblMessage.Visible")));
			// 
			// LocalizerWaitingControl
			// 
			this.AccessibleDescription = resources.GetString("$this.AccessibleDescription");
			this.AccessibleName = resources.GetString("$this.AccessibleName");
			this.AutoScroll = ((bool)(resources.GetObject("$this.AutoScroll")));
			this.AutoScrollMargin = ((System.Drawing.Size)(resources.GetObject("$this.AutoScrollMargin")));
			this.AutoScrollMinSize = ((System.Drawing.Size)(resources.GetObject("$this.AutoScrollMinSize")));
			this.BackColor = System.Drawing.Color.White;
			this.BackgroundImage = ((System.Drawing.Image)(resources.GetObject("$this.BackgroundImage")));
			this.Controls.Add(this.lblMessage);
			this.Controls.Add(this.pictureBox2);
			this.Controls.Add(this.pictureBox1);
			this.Cursor = System.Windows.Forms.Cursors.WaitCursor;
			this.Enabled = ((bool)(resources.GetObject("$this.Enabled")));
			this.Font = ((System.Drawing.Font)(resources.GetObject("$this.Font")));
			this.ImeMode = ((System.Windows.Forms.ImeMode)(resources.GetObject("$this.ImeMode")));
			this.Location = ((System.Drawing.Point)(resources.GetObject("$this.Location")));
			this.Name = "LocalizerWaitingControl";
			this.RightToLeft = ((System.Windows.Forms.RightToLeft)(resources.GetObject("$this.RightToLeft")));
			this.Size = ((System.Drawing.Size)(resources.GetObject("$this.Size")));
			this.ResumeLayout(false);

		}
		#endregion
		
	}
}
