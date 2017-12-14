
namespace Microarea.Tools.TBLocalizer.Forms
{
	/// <summary>
	/// HtmlDialog.
	/// </summary>
	//==========================================================================
	public class HtmlDialog : System.Windows.Forms.Form
	{
		private AxSHDocVw.AxWebBrowser AxWoormBrowser;
		private System.ComponentModel.Container components = null;
		
		//---------------------------------------------------------------------
		public HtmlDialog()
		{
			InitializeComponent();
		}

		//---------------------------------------------------------------------
		public void Show(string url)
		{
			if (url != null)
			{
				object flags = null, targetFrameName = null, postData = null, headers = null;
				AxWoormBrowser.Navigate(url, ref flags, ref targetFrameName, ref postData, ref headers);
			}
				
			Show();
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
			System.Resources.ResourceManager resources = new System.Resources.ResourceManager(typeof(HtmlDialog));
			this.AxWoormBrowser = new AxSHDocVw.AxWebBrowser();
			((System.ComponentModel.ISupportInitialize)(this.AxWoormBrowser)).BeginInit();
			this.SuspendLayout();
			// 
			// AxWoormBrowser
			// 
			this.AxWoormBrowser.AccessibleDescription = resources.GetString("AxWoormBrowser.AccessibleDescription");
			this.AxWoormBrowser.AccessibleName = resources.GetString("AxWoormBrowser.AccessibleName");
			this.AxWoormBrowser.Anchor = ((System.Windows.Forms.AnchorStyles)(resources.GetObject("AxWoormBrowser.Anchor")));
			this.AxWoormBrowser.BackgroundImage = ((System.Drawing.Image)(resources.GetObject("AxWoormBrowser.BackgroundImage")));
			this.AxWoormBrowser.Dock = ((System.Windows.Forms.DockStyle)(resources.GetObject("AxWoormBrowser.Dock")));
			this.AxWoormBrowser.Enabled = ((bool)(resources.GetObject("AxWoormBrowser.Enabled")));
			this.AxWoormBrowser.Font = ((System.Drawing.Font)(resources.GetObject("AxWoormBrowser.Font")));
			this.AxWoormBrowser.ImeMode = ((System.Windows.Forms.ImeMode)(resources.GetObject("AxWoormBrowser.ImeMode")));
			this.AxWoormBrowser.Location = ((System.Drawing.Point)(resources.GetObject("AxWoormBrowser.Location")));
			this.AxWoormBrowser.OcxState = ((System.Windows.Forms.AxHost.State)(resources.GetObject("AxWoormBrowser.OcxState")));
			this.AxWoormBrowser.RightToLeft = ((bool)(resources.GetObject("AxWoormBrowser.RightToLeft")));
			this.AxWoormBrowser.Size = ((System.Drawing.Size)(resources.GetObject("AxWoormBrowser.Size")));
			this.AxWoormBrowser.TabIndex = ((int)(resources.GetObject("AxWoormBrowser.TabIndex")));
			this.AxWoormBrowser.Text = resources.GetString("AxWoormBrowser.Text");
			this.AxWoormBrowser.Visible = ((bool)(resources.GetObject("AxWoormBrowser.Visible")));
			// 
			// HtmlDialog
			// 
			this.AccessibleDescription = resources.GetString("$this.AccessibleDescription");
			this.AccessibleName = resources.GetString("$this.AccessibleName");
			this.AutoScaleBaseSize = ((System.Drawing.Size)(resources.GetObject("$this.AutoScaleBaseSize")));
			this.AutoScroll = ((bool)(resources.GetObject("$this.AutoScroll")));
			this.AutoScrollMargin = ((System.Drawing.Size)(resources.GetObject("$this.AutoScrollMargin")));
			this.AutoScrollMinSize = ((System.Drawing.Size)(resources.GetObject("$this.AutoScrollMinSize")));
			this.BackgroundImage = ((System.Drawing.Image)(resources.GetObject("$this.BackgroundImage")));
			this.ClientSize = ((System.Drawing.Size)(resources.GetObject("$this.ClientSize")));
			this.Controls.Add(this.AxWoormBrowser);
			this.Enabled = ((bool)(resources.GetObject("$this.Enabled")));
			this.Font = ((System.Drawing.Font)(resources.GetObject("$this.Font")));
			this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.SizableToolWindow;
			this.Icon = ((System.Drawing.Icon)(resources.GetObject("$this.Icon")));
			this.ImeMode = ((System.Windows.Forms.ImeMode)(resources.GetObject("$this.ImeMode")));
			this.Location = ((System.Drawing.Point)(resources.GetObject("$this.Location")));
			this.MaximumSize = ((System.Drawing.Size)(resources.GetObject("$this.MaximumSize")));
			this.MinimumSize = ((System.Drawing.Size)(resources.GetObject("$this.MinimumSize")));
			this.Name = "HtmlDialog";
			this.RightToLeft = ((System.Windows.Forms.RightToLeft)(resources.GetObject("$this.RightToLeft")));
			this.ShowInTaskbar = false;
			this.StartPosition = ((System.Windows.Forms.FormStartPosition)(resources.GetObject("$this.StartPosition")));
			this.Text = resources.GetString("$this.Text");
			((System.ComponentModel.ISupportInitialize)(this.AxWoormBrowser)).EndInit();
			this.ResumeLayout(false);

		}
		#endregion
	}
}
