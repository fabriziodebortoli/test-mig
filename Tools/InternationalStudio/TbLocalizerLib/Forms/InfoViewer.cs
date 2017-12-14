
namespace Microarea.Tools.TBLocalizer.Forms
{
	public class InfoViewer : System.Windows.Forms.Form
	{
		private System.Windows.Forms.TextBox TxtMessage;

		private System.ComponentModel.Container components = null;

		public InfoViewer(string message)
		{	
			InitializeComponent();
			Fill(message);
		}

		public void Fill(string message)
		{	
			TxtMessage.Text = message;
			TxtMessage.SelectionLength = 0;
		}

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
			System.Resources.ResourceManager resources = new System.Resources.ResourceManager(typeof(InfoViewer));
			this.TxtMessage = new System.Windows.Forms.TextBox();
			this.SuspendLayout();
			// 
			// TxtMessage
			// 
			this.TxtMessage.AccessibleDescription = resources.GetString("TxtMessage.AccessibleDescription");
			this.TxtMessage.AccessibleName = resources.GetString("TxtMessage.AccessibleName");
			this.TxtMessage.Anchor = ((System.Windows.Forms.AnchorStyles)(resources.GetObject("TxtMessage.Anchor")));
			this.TxtMessage.AutoSize = ((bool)(resources.GetObject("TxtMessage.AutoSize")));
			this.TxtMessage.BackgroundImage = ((System.Drawing.Image)(resources.GetObject("TxtMessage.BackgroundImage")));
			this.TxtMessage.Cursor = System.Windows.Forms.Cursors.Arrow;
			this.TxtMessage.Dock = ((System.Windows.Forms.DockStyle)(resources.GetObject("TxtMessage.Dock")));
			this.TxtMessage.Enabled = ((bool)(resources.GetObject("TxtMessage.Enabled")));
			this.TxtMessage.Font = ((System.Drawing.Font)(resources.GetObject("TxtMessage.Font")));
			this.TxtMessage.ImeMode = ((System.Windows.Forms.ImeMode)(resources.GetObject("TxtMessage.ImeMode")));
			this.TxtMessage.Location = ((System.Drawing.Point)(resources.GetObject("TxtMessage.Location")));
			this.TxtMessage.MaxLength = ((int)(resources.GetObject("TxtMessage.MaxLength")));
			this.TxtMessage.Multiline = ((bool)(resources.GetObject("TxtMessage.Multiline")));
			this.TxtMessage.Name = "TxtMessage";
			this.TxtMessage.PasswordChar = ((char)(resources.GetObject("TxtMessage.PasswordChar")));
			this.TxtMessage.ReadOnly = true;
			this.TxtMessage.RightToLeft = ((System.Windows.Forms.RightToLeft)(resources.GetObject("TxtMessage.RightToLeft")));
			this.TxtMessage.ScrollBars = ((System.Windows.Forms.ScrollBars)(resources.GetObject("TxtMessage.ScrollBars")));
			this.TxtMessage.Size = ((System.Drawing.Size)(resources.GetObject("TxtMessage.Size")));
			this.TxtMessage.TabIndex = ((int)(resources.GetObject("TxtMessage.TabIndex")));
			this.TxtMessage.Text = resources.GetString("TxtMessage.Text");
			this.TxtMessage.TextAlign = ((System.Windows.Forms.HorizontalAlignment)(resources.GetObject("TxtMessage.TextAlign")));
			this.TxtMessage.Visible = ((bool)(resources.GetObject("TxtMessage.Visible")));
			this.TxtMessage.WordWrap = ((bool)(resources.GetObject("TxtMessage.WordWrap")));
			// 
			// InfoViewer
			// 
			this.AccessibleDescription = resources.GetString("$this.AccessibleDescription");
			this.AccessibleName = resources.GetString("$this.AccessibleName");
			this.AutoScaleBaseSize = ((System.Drawing.Size)(resources.GetObject("$this.AutoScaleBaseSize")));
			this.AutoScroll = ((bool)(resources.GetObject("$this.AutoScroll")));
			this.AutoScrollMargin = ((System.Drawing.Size)(resources.GetObject("$this.AutoScrollMargin")));
			this.AutoScrollMinSize = ((System.Drawing.Size)(resources.GetObject("$this.AutoScrollMinSize")));
			this.BackColor = System.Drawing.SystemColors.Window;
			this.BackgroundImage = ((System.Drawing.Image)(resources.GetObject("$this.BackgroundImage")));
			this.ClientSize = ((System.Drawing.Size)(resources.GetObject("$this.ClientSize")));
			this.Controls.Add(this.TxtMessage);
			this.Enabled = ((bool)(resources.GetObject("$this.Enabled")));
			this.Font = ((System.Drawing.Font)(resources.GetObject("$this.Font")));
			this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.SizableToolWindow;
			this.Icon = ((System.Drawing.Icon)(resources.GetObject("$this.Icon")));
			this.ImeMode = ((System.Windows.Forms.ImeMode)(resources.GetObject("$this.ImeMode")));
			this.Location = ((System.Drawing.Point)(resources.GetObject("$this.Location")));
			this.MaximumSize = ((System.Drawing.Size)(resources.GetObject("$this.MaximumSize")));
			this.MinimumSize = ((System.Drawing.Size)(resources.GetObject("$this.MinimumSize")));
			this.Name = "InfoViewer";
			this.RightToLeft = ((System.Windows.Forms.RightToLeft)(resources.GetObject("$this.RightToLeft")));
			this.ShowInTaskbar = false;
			this.StartPosition = ((System.Windows.Forms.FormStartPosition)(resources.GetObject("$this.StartPosition")));
			this.Text = resources.GetString("$this.Text");
			this.ResumeLayout(false);

		}
		#endregion

	}
}
