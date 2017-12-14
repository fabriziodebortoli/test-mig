using System;
using System.Windows.Forms;

namespace Microarea.TaskBuilderNet.UI.WinControls
{

	//=========================================================================
	partial class PictureWithBalloon
	{
		private System.ComponentModel.IContainer components;
		/// <summary> 
		/// Clean up any resources being used.
		/// </summary>
		//--------------------------------------------------------------------------- 
		protected override void Dispose(bool disposing)
		{
			if (disposing)
			{
				if (components != null)
					components.Dispose();

				if (toolTip != null)
				{
					toolTip.Dispose();
					toolTip = null;
				}
				if (blinkTimer != null)
				{
					blinkTimer.Enabled = false;
					blinkTimer.Dispose();
					blinkTimer = null;
				}

				Form parentForm = this.ParentForm;
				if (parentForm != null && !parentForm.IsDisposed)
					parentForm.Activated -= new EventHandler(ParentForm_Activated);
			}
			base.Dispose(disposing);
		}

		#region Component Designer generated code
		/// <summary> 
		/// Required method for Designer support - do not modify 
		/// the contents of this method with the code editor.
		/// </summary>
		private void InitializeComponent()
		{
            this.components = new System.ComponentModel.Container();
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(PictureWithBalloon));
            this.pictureBox = new System.Windows.Forms.PictureBox();
            this.toolTip = new System.Windows.Forms.ToolTip(this.components);
            this.notifyIcon1 = new System.Windows.Forms.NotifyIcon(this.components);
            ((System.ComponentModel.ISupportInitialize)(this.pictureBox)).BeginInit();
            this.SuspendLayout();
            // 
            // pictureBox
            // 
            resources.ApplyResources(this.pictureBox, "pictureBox");
            this.pictureBox.Name = "pictureBox";
            this.pictureBox.TabStop = false;
            // 
            // notifyIcon1
            // 
            resources.ApplyResources(this.notifyIcon1, "notifyIcon1");
            // 
            // PictureWithBalloon
            // 
            this.Controls.Add(this.pictureBox);
            this.Name = "PictureWithBalloon";
            resources.ApplyResources(this, "$this");
            ((System.ComponentModel.ISupportInitialize)(this.pictureBox)).EndInit();
            this.ResumeLayout(false);

		}
		#endregion

        private NotifyIcon notifyIcon1;

	}
}
