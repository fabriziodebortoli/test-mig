using System;
using System.Collections;
using System.ComponentModel;
using System.Drawing;
using System.Data;
using System.Windows.Forms;

namespace Microarea.Library.WorkFlowWindowsControls
{
	/// <summary>
	/// Summary description for WorkFlowTemplateMainPage.
	/// </summary>
	public class WorkFlowTemplateMainPage : System.Windows.Forms.UserControl
	{
		private System.Windows.Forms.Panel CaptionPanel;
		private System.Windows.Forms.Label LblCaptionTemplateWorkFlow;
		private System.Windows.Forms.Label DescriptionLabel;
		/// <summary> 
		/// Required designer variable.
		/// </summary>
		private System.ComponentModel.Container components = null;

		public WorkFlowTemplateMainPage()
		{
			// This call is required by the Windows.Forms Form Designer.
			InitializeComponent();

			// TODO: Add any initialization after the InitializeComponent call

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

		#region Component Designer generated code
		/// <summary> 
		/// Required method for Designer support - do not modify 
		/// the contents of this method with the code editor.
		/// </summary>
		private void InitializeComponent()
		{
			System.Resources.ResourceManager resources = new System.Resources.ResourceManager(typeof(WorkFlowTemplateMainPage));
			this.CaptionPanel = new System.Windows.Forms.Panel();
			this.LblCaptionTemplateWorkFlow = new System.Windows.Forms.Label();
			this.DescriptionLabel = new System.Windows.Forms.Label();
			this.CaptionPanel.SuspendLayout();
			this.SuspendLayout();
			// 
			// CaptionPanel
			// 
			this.CaptionPanel.AccessibleDescription = resources.GetString("CaptionPanel.AccessibleDescription");
			this.CaptionPanel.AccessibleName = resources.GetString("CaptionPanel.AccessibleName");
			this.CaptionPanel.Anchor = ((System.Windows.Forms.AnchorStyles)(resources.GetObject("CaptionPanel.Anchor")));
			this.CaptionPanel.AutoScroll = ((bool)(resources.GetObject("CaptionPanel.AutoScroll")));
			this.CaptionPanel.AutoScrollMargin = ((System.Drawing.Size)(resources.GetObject("CaptionPanel.AutoScrollMargin")));
			this.CaptionPanel.AutoScrollMinSize = ((System.Drawing.Size)(resources.GetObject("CaptionPanel.AutoScrollMinSize")));
			this.CaptionPanel.BackColor = System.Drawing.Color.Lavender;
			this.CaptionPanel.BackgroundImage = ((System.Drawing.Image)(resources.GetObject("CaptionPanel.BackgroundImage")));
			this.CaptionPanel.BorderStyle = System.Windows.Forms.BorderStyle.Fixed3D;
			this.CaptionPanel.Controls.Add(this.LblCaptionTemplateWorkFlow);
			this.CaptionPanel.Controls.Add(this.DescriptionLabel);
			this.CaptionPanel.Dock = ((System.Windows.Forms.DockStyle)(resources.GetObject("CaptionPanel.Dock")));
			this.CaptionPanel.DockPadding.Left = 4;
			this.CaptionPanel.DockPadding.Right = 4;
			this.CaptionPanel.DockPadding.Top = 10;
			this.CaptionPanel.Enabled = ((bool)(resources.GetObject("CaptionPanel.Enabled")));
			this.CaptionPanel.Font = ((System.Drawing.Font)(resources.GetObject("CaptionPanel.Font")));
			this.CaptionPanel.ImeMode = ((System.Windows.Forms.ImeMode)(resources.GetObject("CaptionPanel.ImeMode")));
			this.CaptionPanel.Location = ((System.Drawing.Point)(resources.GetObject("CaptionPanel.Location")));
			this.CaptionPanel.Name = "CaptionPanel";
			this.CaptionPanel.RightToLeft = ((System.Windows.Forms.RightToLeft)(resources.GetObject("CaptionPanel.RightToLeft")));
			this.CaptionPanel.Size = ((System.Drawing.Size)(resources.GetObject("CaptionPanel.Size")));
			this.CaptionPanel.TabIndex = ((int)(resources.GetObject("CaptionPanel.TabIndex")));
			this.CaptionPanel.Text = resources.GetString("CaptionPanel.Text");
			this.CaptionPanel.Visible = ((bool)(resources.GetObject("CaptionPanel.Visible")));
			// 
			// LblCaptionTemplateWorkFlow
			// 
			this.LblCaptionTemplateWorkFlow.AccessibleDescription = resources.GetString("LblCaptionTemplateWorkFlow.AccessibleDescription");
			this.LblCaptionTemplateWorkFlow.AccessibleName = resources.GetString("LblCaptionTemplateWorkFlow.AccessibleName");
			this.LblCaptionTemplateWorkFlow.Anchor = ((System.Windows.Forms.AnchorStyles)(resources.GetObject("LblCaptionTemplateWorkFlow.Anchor")));
			this.LblCaptionTemplateWorkFlow.AutoSize = ((bool)(resources.GetObject("LblCaptionTemplateWorkFlow.AutoSize")));
			this.LblCaptionTemplateWorkFlow.CausesValidation = false;
			this.LblCaptionTemplateWorkFlow.Dock = ((System.Windows.Forms.DockStyle)(resources.GetObject("LblCaptionTemplateWorkFlow.Dock")));
			this.LblCaptionTemplateWorkFlow.Enabled = ((bool)(resources.GetObject("LblCaptionTemplateWorkFlow.Enabled")));
			this.LblCaptionTemplateWorkFlow.FlatStyle = System.Windows.Forms.FlatStyle.System;
			this.LblCaptionTemplateWorkFlow.Font = ((System.Drawing.Font)(resources.GetObject("LblCaptionTemplateWorkFlow.Font")));
			this.LblCaptionTemplateWorkFlow.ForeColor = System.Drawing.Color.RoyalBlue;
			this.LblCaptionTemplateWorkFlow.Image = ((System.Drawing.Image)(resources.GetObject("LblCaptionTemplateWorkFlow.Image")));
			this.LblCaptionTemplateWorkFlow.ImageAlign = ((System.Drawing.ContentAlignment)(resources.GetObject("LblCaptionTemplateWorkFlow.ImageAlign")));
			this.LblCaptionTemplateWorkFlow.ImageIndex = ((int)(resources.GetObject("LblCaptionTemplateWorkFlow.ImageIndex")));
			this.LblCaptionTemplateWorkFlow.ImeMode = ((System.Windows.Forms.ImeMode)(resources.GetObject("LblCaptionTemplateWorkFlow.ImeMode")));
			this.LblCaptionTemplateWorkFlow.Location = ((System.Drawing.Point)(resources.GetObject("LblCaptionTemplateWorkFlow.Location")));
			this.LblCaptionTemplateWorkFlow.Name = "LblCaptionTemplateWorkFlow";
			this.LblCaptionTemplateWorkFlow.RightToLeft = ((System.Windows.Forms.RightToLeft)(resources.GetObject("LblCaptionTemplateWorkFlow.RightToLeft")));
			this.LblCaptionTemplateWorkFlow.Size = ((System.Drawing.Size)(resources.GetObject("LblCaptionTemplateWorkFlow.Size")));
			this.LblCaptionTemplateWorkFlow.TabIndex = ((int)(resources.GetObject("LblCaptionTemplateWorkFlow.TabIndex")));
			this.LblCaptionTemplateWorkFlow.Text = resources.GetString("LblCaptionTemplateWorkFlow.Text");
			this.LblCaptionTemplateWorkFlow.TextAlign = ((System.Drawing.ContentAlignment)(resources.GetObject("LblCaptionTemplateWorkFlow.TextAlign")));
			this.LblCaptionTemplateWorkFlow.Visible = ((bool)(resources.GetObject("LblCaptionTemplateWorkFlow.Visible")));
			// 
			// DescriptionLabel
			// 
			this.DescriptionLabel.AccessibleDescription = resources.GetString("DescriptionLabel.AccessibleDescription");
			this.DescriptionLabel.AccessibleName = resources.GetString("DescriptionLabel.AccessibleName");
			this.DescriptionLabel.Anchor = ((System.Windows.Forms.AnchorStyles)(resources.GetObject("DescriptionLabel.Anchor")));
			this.DescriptionLabel.AutoSize = ((bool)(resources.GetObject("DescriptionLabel.AutoSize")));
			this.DescriptionLabel.Dock = ((System.Windows.Forms.DockStyle)(resources.GetObject("DescriptionLabel.Dock")));
			this.DescriptionLabel.Enabled = ((bool)(resources.GetObject("DescriptionLabel.Enabled")));
			this.DescriptionLabel.Font = ((System.Drawing.Font)(resources.GetObject("DescriptionLabel.Font")));
			this.DescriptionLabel.Image = ((System.Drawing.Image)(resources.GetObject("DescriptionLabel.Image")));
			this.DescriptionLabel.ImageAlign = ((System.Drawing.ContentAlignment)(resources.GetObject("DescriptionLabel.ImageAlign")));
			this.DescriptionLabel.ImageIndex = ((int)(resources.GetObject("DescriptionLabel.ImageIndex")));
			this.DescriptionLabel.ImeMode = ((System.Windows.Forms.ImeMode)(resources.GetObject("DescriptionLabel.ImeMode")));
			this.DescriptionLabel.Location = ((System.Drawing.Point)(resources.GetObject("DescriptionLabel.Location")));
			this.DescriptionLabel.Name = "DescriptionLabel";
			this.DescriptionLabel.RightToLeft = ((System.Windows.Forms.RightToLeft)(resources.GetObject("DescriptionLabel.RightToLeft")));
			this.DescriptionLabel.Size = ((System.Drawing.Size)(resources.GetObject("DescriptionLabel.Size")));
			this.DescriptionLabel.TabIndex = ((int)(resources.GetObject("DescriptionLabel.TabIndex")));
			this.DescriptionLabel.Text = resources.GetString("DescriptionLabel.Text");
			this.DescriptionLabel.TextAlign = ((System.Drawing.ContentAlignment)(resources.GetObject("DescriptionLabel.TextAlign")));
			this.DescriptionLabel.Visible = ((bool)(resources.GetObject("DescriptionLabel.Visible")));
			// 
			// WorkFlowTemplateMainPage
			// 
			this.AccessibleDescription = resources.GetString("$this.AccessibleDescription");
			this.AccessibleName = resources.GetString("$this.AccessibleName");
			this.AutoScroll = ((bool)(resources.GetObject("$this.AutoScroll")));
			this.AutoScrollMargin = ((System.Drawing.Size)(resources.GetObject("$this.AutoScrollMargin")));
			this.AutoScrollMinSize = ((System.Drawing.Size)(resources.GetObject("$this.AutoScrollMinSize")));
			this.BackColor = System.Drawing.Color.Lavender;
			this.BackgroundImage = ((System.Drawing.Image)(resources.GetObject("$this.BackgroundImage")));
			this.Controls.Add(this.CaptionPanel);
			this.Enabled = ((bool)(resources.GetObject("$this.Enabled")));
			this.Font = ((System.Drawing.Font)(resources.GetObject("$this.Font")));
			this.ImeMode = ((System.Windows.Forms.ImeMode)(resources.GetObject("$this.ImeMode")));
			this.Location = ((System.Drawing.Point)(resources.GetObject("$this.Location")));
			this.Name = "WorkFlowTemplateMainPage";
			this.RightToLeft = ((System.Windows.Forms.RightToLeft)(resources.GetObject("$this.RightToLeft")));
			this.Size = ((System.Drawing.Size)(resources.GetObject("$this.Size")));
			this.CaptionPanel.ResumeLayout(false);
			this.ResumeLayout(false);

		}
		#endregion
	}
}
