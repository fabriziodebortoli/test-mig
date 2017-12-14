
using Microarea.TaskBuilderNet.UI.WinControls.Labels;
namespace Microarea.TaskBuilderNet.UI.WinControls
{
    partial class ScrollingWaitingWindow
    {
        private Labels.ScrollingTextLabel messageBox;
        private System.ComponentModel.Container components = null;

        /// <summary>
        /// Clean up any resources being used.
        /// </summary>
        //--------------------------------------------------------------------------------
        protected override void Dispose(bool disposing)
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

        #region Windows Form Designer generated code
        /// <summary>
        /// Required method for Designer support - do not modify
        /// the contents of this method with the code editor.
        /// </summary>
        //--------------------------------------------------------------------------------
        private void InitializeComponent()
        {
            System.Resources.ResourceManager resources = new System.Resources.ResourceManager(typeof(ScrollingWaitingWindow));
            this.messageBox = new ScrollingTextLabel();
            this.SuspendLayout();
            // 
            // messageBox
            // 
            this.messageBox.AccessibleDescription = resources.GetString("messageBox.AccessibleDescription");
            this.messageBox.AccessibleName = resources.GetString("messageBox.AccessibleName");
            this.messageBox.Anchor = ((System.Windows.Forms.AnchorStyles)(resources.GetObject("messageBox.Anchor")));
            this.messageBox.AutoScroll = ((bool)(resources.GetObject("messageBox.AutoScroll")));
            this.messageBox.AutoScrollMargin = ((System.Drawing.Size)(resources.GetObject("messageBox.AutoScrollMargin")));
            this.messageBox.AutoScrollMinSize = ((System.Drawing.Size)(resources.GetObject("messageBox.AutoScrollMinSize")));
            this.messageBox.BackgroundImage = ((System.Drawing.Image)(resources.GetObject("messageBox.BackgroundImage")));
            this.messageBox.Dock = ((System.Windows.Forms.DockStyle)(resources.GetObject("messageBox.Dock")));
            this.messageBox.Enabled = ((bool)(resources.GetObject("messageBox.Enabled")));
            this.messageBox.Font = ((System.Drawing.Font)(resources.GetObject("messageBox.Font")));
            this.messageBox.ImeMode = ((System.Windows.Forms.ImeMode)(resources.GetObject("messageBox.ImeMode")));
            this.messageBox.Interval = 100;
            this.messageBox.Location = ((System.Drawing.Point)(resources.GetObject("messageBox.Location")));
            this.messageBox.Name = "messageBox";
            this.messageBox.RightToLeft = ((System.Windows.Forms.RightToLeft)(resources.GetObject("messageBox.RightToLeft")));
            this.messageBox.ScrollingText = "Please wait...";
            this.messageBox.Size = ((System.Drawing.Size)(resources.GetObject("messageBox.Size")));
            this.messageBox.Speed = 5;
            this.messageBox.TabIndex = ((int)(resources.GetObject("messageBox.TabIndex")));
            this.messageBox.Visible = ((bool)(resources.GetObject("messageBox.Visible")));
            // 
            // ScrollingWaitingWindow
            // 
            this.AccessibleDescription = resources.GetString("$this.AccessibleDescription");
            this.AccessibleName = resources.GetString("$this.AccessibleName");
            this.AutoScaleBaseSize = ((System.Drawing.Size)(resources.GetObject("$this.AutoScaleBaseSize")));
            this.AutoScroll = ((bool)(resources.GetObject("$this.AutoScroll")));
            this.AutoScrollMargin = ((System.Drawing.Size)(resources.GetObject("$this.AutoScrollMargin")));
            this.AutoScrollMinSize = ((System.Drawing.Size)(resources.GetObject("$this.AutoScrollMinSize")));
            this.BackgroundImage = ((System.Drawing.Image)(resources.GetObject("$this.BackgroundImage")));
            this.ClientSize = ((System.Drawing.Size)(resources.GetObject("$this.ClientSize")));
            this.ControlBox = false;
            this.Controls.Add(this.messageBox);
            this.Enabled = ((bool)(resources.GetObject("$this.Enabled")));
            this.Font = ((System.Drawing.Font)(resources.GetObject("$this.Font")));
            this.Icon = ((System.Drawing.Icon)(resources.GetObject("$this.Icon")));
            this.ImeMode = ((System.Windows.Forms.ImeMode)(resources.GetObject("$this.ImeMode")));
            this.Location = ((System.Drawing.Point)(resources.GetObject("$this.Location")));
            this.MaximizeBox = false;
            this.MaximumSize = ((System.Drawing.Size)(resources.GetObject("$this.MaximumSize")));
            this.MinimizeBox = false;
            this.MinimumSize = ((System.Drawing.Size)(resources.GetObject("$this.MinimumSize")));
            this.Name = "ScrollingWaitingWindow";
            this.RightToLeft = ((System.Windows.Forms.RightToLeft)(resources.GetObject("$this.RightToLeft")));
            this.ShowInTaskbar = false;
            this.StartPosition = ((System.Windows.Forms.FormStartPosition)(resources.GetObject("$this.StartPosition")));
            this.Text = resources.GetString("$this.Text");
            this.Paint += new System.Windows.Forms.PaintEventHandler(this.ScrollingWaitingWindow_Paint);
            this.ResumeLayout(false);

        }
        #endregion
    }
}
