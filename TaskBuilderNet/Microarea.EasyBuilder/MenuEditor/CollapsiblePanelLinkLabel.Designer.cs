using System.Windows.Forms;
namespace Microarea.EasyBuilder.MenuEditor
{
    partial class CollapsiblePanelLinkLabel
    {
        /// <summary> 
        /// Required designer variable.
        /// </summary>
        private System.ComponentModel.IContainer components = null;

        /// <summary> 
        /// Clean up any resources being used.
        /// </summary>
        /// <param name="disposing">true if managed resources should be disposed; otherwise, false.</param>
        protected override void Dispose(bool disposing)
        {
			if (disposing)
			{
				if (components != null)
				{
					components.Dispose();
				}
				StopMouseDownTimer();
				this.LinkLabel.KeyUp -= new KeyEventHandler(ChildControl_KeyUp);
				this.PictureBox.KeyUp -= new KeyEventHandler(ChildControl_KeyUp);
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
			System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(CollapsiblePanelLinkLabel));
			this.LinkLabel = new System.Windows.Forms.LinkLabel();
			this.PictureBox = new System.Windows.Forms.PictureBox();
			this.LinklabelToolTip = new System.Windows.Forms.ToolTip(this.components);
			((System.ComponentModel.ISupportInitialize)(this.PictureBox)).BeginInit();
			this.SuspendLayout();
			// 
			// LinkLabel
			// 
			this.LinkLabel.ActiveLinkColor = System.Drawing.Color.Fuchsia;
			this.LinkLabel.AutoEllipsis = true;
			this.LinkLabel.BackColor = System.Drawing.Color.Transparent;
			this.LinkLabel.DisabledLinkColor = System.Drawing.Color.Silver;
			resources.ApplyResources(this.LinkLabel, "LinkLabel");
			this.LinkLabel.ForeColor = System.Drawing.Color.Navy;
			this.LinkLabel.LinkBehavior = System.Windows.Forms.LinkBehavior.HoverUnderline;
			this.LinkLabel.LinkColor = System.Drawing.Color.MidnightBlue;
			this.LinkLabel.Name = "LinkLabel";
			this.LinkLabel.TabStop = true;
			this.LinkLabel.VisitedLinkColor = System.Drawing.Color.Purple;
			this.LinkLabel.LinkClicked += new System.Windows.Forms.LinkLabelLinkClickedEventHandler(this.LinkLabel_LinkClicked);
			this.LinkLabel.TextChanged += new System.EventHandler(this.LinkLabel_TextChanged);
			this.LinkLabel.MouseDown += new System.Windows.Forms.MouseEventHandler(this.LinkLabel_MouseDown);
			this.LinkLabel.MouseLeave += new System.EventHandler(this.LinkLabel_MouseLeave);
			this.LinkLabel.MouseUp += new System.Windows.Forms.MouseEventHandler(this.LinkLabel_MouseUp);
			// 
			// PictureBox
			// 
			this.PictureBox.BackColor = System.Drawing.Color.Transparent;
			resources.ApplyResources(this.PictureBox, "PictureBox");
			this.PictureBox.Name = "PictureBox";
			this.PictureBox.TabStop = false;
			this.PictureBox.MouseDown += new System.Windows.Forms.MouseEventHandler(this.PictureBox_MouseDown);
			this.PictureBox.MouseUp += new System.Windows.Forms.MouseEventHandler(this.PictureBox_MouseUp);
			// 
			// LinklabelToolTip
			// 
			this.LinklabelToolTip.ShowAlways = true;
			// 
			// CollapsiblePanelLinkLabel
			// 
			this.AllowDrop = true;
			this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.None;
			this.BackColor = System.Drawing.Color.Transparent;
			this.Controls.Add(this.PictureBox);
			this.Controls.Add(this.LinkLabel);
			resources.ApplyResources(this, "$this");
			this.Name = "CollapsiblePanelLinkLabel";
			((System.ComponentModel.ISupportInitialize)(this.PictureBox)).EndInit();
			this.ResumeLayout(false);
			this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.LinkLabel LinkLabel;
        private System.Windows.Forms.PictureBox PictureBox;
        private System.Windows.Forms.ToolTip LinklabelToolTip;
    }
}
