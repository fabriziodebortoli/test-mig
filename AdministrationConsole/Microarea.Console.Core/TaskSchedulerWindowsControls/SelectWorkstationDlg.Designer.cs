
namespace Microarea.Console.Core.TaskSchedulerWindowsControls
{
    partial class SelectWorkstationDlg
    {
        private System.Windows.Forms.CheckedListBox WorkStationsListBox;
        private System.Windows.Forms.Button OkBtn;
        private System.Windows.Forms.Button CancelBtn;
        /// <summary>
        /// Required designer variable.
        /// </summary>
        private System.ComponentModel.Container components = null;

        /// <summary>
        /// Clean up any resources being used.
        /// </summary>
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
        private void InitializeComponent()
        {
            System.Resources.ResourceManager resources = new System.Resources.ResourceManager(typeof(SelectWorkstationDlg));
            this.CancelBtn = new System.Windows.Forms.Button();
            this.OkBtn = new System.Windows.Forms.Button();
            this.WorkStationsListBox = new System.Windows.Forms.CheckedListBox();
            this.SuspendLayout();
            // 
            // CancelBtn
            // 
            this.CancelBtn.AccessibleDescription = resources.GetString("CancelBtn.AccessibleDescription");
            this.CancelBtn.AccessibleName = resources.GetString("CancelBtn.AccessibleName");
            this.CancelBtn.Anchor = ((System.Windows.Forms.AnchorStyles)(resources.GetObject("CancelBtn.Anchor")));
            this.CancelBtn.BackgroundImage = ((System.Drawing.Image)(resources.GetObject("CancelBtn.BackgroundImage")));
            this.CancelBtn.DialogResult = System.Windows.Forms.DialogResult.Cancel;
            this.CancelBtn.Dock = ((System.Windows.Forms.DockStyle)(resources.GetObject("CancelBtn.Dock")));
            this.CancelBtn.Enabled = ((bool)(resources.GetObject("CancelBtn.Enabled")));
            this.CancelBtn.FlatStyle = ((System.Windows.Forms.FlatStyle)(resources.GetObject("CancelBtn.FlatStyle")));
            this.CancelBtn.Font = ((System.Drawing.Font)(resources.GetObject("CancelBtn.Font")));
            this.CancelBtn.Image = ((System.Drawing.Image)(resources.GetObject("CancelBtn.Image")));
            this.CancelBtn.ImageAlign = ((System.Drawing.ContentAlignment)(resources.GetObject("CancelBtn.ImageAlign")));
            this.CancelBtn.ImageIndex = ((int)(resources.GetObject("CancelBtn.ImageIndex")));
            this.CancelBtn.ImeMode = ((System.Windows.Forms.ImeMode)(resources.GetObject("CancelBtn.ImeMode")));
            this.CancelBtn.Location = ((System.Drawing.Point)(resources.GetObject("CancelBtn.Location")));
            this.CancelBtn.Name = "CancelBtn";
            this.CancelBtn.RightToLeft = ((System.Windows.Forms.RightToLeft)(resources.GetObject("CancelBtn.RightToLeft")));
            this.CancelBtn.Size = ((System.Drawing.Size)(resources.GetObject("CancelBtn.Size")));
            this.CancelBtn.TabIndex = ((int)(resources.GetObject("CancelBtn.TabIndex")));
            this.CancelBtn.Text = resources.GetString("CancelBtn.Text");
            this.CancelBtn.TextAlign = ((System.Drawing.ContentAlignment)(resources.GetObject("CancelBtn.TextAlign")));
            this.CancelBtn.Visible = ((bool)(resources.GetObject("CancelBtn.Visible")));
            // 
            // OkBtn
            // 
            this.OkBtn.AccessibleDescription = resources.GetString("OkBtn.AccessibleDescription");
            this.OkBtn.AccessibleName = resources.GetString("OkBtn.AccessibleName");
            this.OkBtn.Anchor = ((System.Windows.Forms.AnchorStyles)(resources.GetObject("OkBtn.Anchor")));
            this.OkBtn.BackgroundImage = ((System.Drawing.Image)(resources.GetObject("OkBtn.BackgroundImage")));
            this.OkBtn.DialogResult = System.Windows.Forms.DialogResult.OK;
            this.OkBtn.Dock = ((System.Windows.Forms.DockStyle)(resources.GetObject("OkBtn.Dock")));
            this.OkBtn.Enabled = ((bool)(resources.GetObject("OkBtn.Enabled")));
            this.OkBtn.FlatStyle = ((System.Windows.Forms.FlatStyle)(resources.GetObject("OkBtn.FlatStyle")));
            this.OkBtn.Font = ((System.Drawing.Font)(resources.GetObject("OkBtn.Font")));
            this.OkBtn.Image = ((System.Drawing.Image)(resources.GetObject("OkBtn.Image")));
            this.OkBtn.ImageAlign = ((System.Drawing.ContentAlignment)(resources.GetObject("OkBtn.ImageAlign")));
            this.OkBtn.ImageIndex = ((int)(resources.GetObject("OkBtn.ImageIndex")));
            this.OkBtn.ImeMode = ((System.Windows.Forms.ImeMode)(resources.GetObject("OkBtn.ImeMode")));
            this.OkBtn.Location = ((System.Drawing.Point)(resources.GetObject("OkBtn.Location")));
            this.OkBtn.Name = "OkBtn";
            this.OkBtn.RightToLeft = ((System.Windows.Forms.RightToLeft)(resources.GetObject("OkBtn.RightToLeft")));
            this.OkBtn.Size = ((System.Drawing.Size)(resources.GetObject("OkBtn.Size")));
            this.OkBtn.TabIndex = ((int)(resources.GetObject("OkBtn.TabIndex")));
            this.OkBtn.Text = resources.GetString("OkBtn.Text");
            this.OkBtn.TextAlign = ((System.Drawing.ContentAlignment)(resources.GetObject("OkBtn.TextAlign")));
            this.OkBtn.Visible = ((bool)(resources.GetObject("OkBtn.Visible")));
            // 
            // WorkStationsListBox
            // 
            this.WorkStationsListBox.AccessibleDescription = resources.GetString("WorkStationsListBox.AccessibleDescription");
            this.WorkStationsListBox.AccessibleName = resources.GetString("WorkStationsListBox.AccessibleName");
            this.WorkStationsListBox.Anchor = ((System.Windows.Forms.AnchorStyles)(resources.GetObject("WorkStationsListBox.Anchor")));
            this.WorkStationsListBox.BackgroundImage = ((System.Drawing.Image)(resources.GetObject("WorkStationsListBox.BackgroundImage")));
            this.WorkStationsListBox.ColumnWidth = ((int)(resources.GetObject("WorkStationsListBox.ColumnWidth")));
            this.WorkStationsListBox.Dock = ((System.Windows.Forms.DockStyle)(resources.GetObject("WorkStationsListBox.Dock")));
            this.WorkStationsListBox.Enabled = ((bool)(resources.GetObject("WorkStationsListBox.Enabled")));
            this.WorkStationsListBox.Font = ((System.Drawing.Font)(resources.GetObject("WorkStationsListBox.Font")));
            this.WorkStationsListBox.HorizontalExtent = ((int)(resources.GetObject("WorkStationsListBox.HorizontalExtent")));
            this.WorkStationsListBox.HorizontalScrollbar = ((bool)(resources.GetObject("WorkStationsListBox.HorizontalScrollbar")));
            this.WorkStationsListBox.ImeMode = ((System.Windows.Forms.ImeMode)(resources.GetObject("WorkStationsListBox.ImeMode")));
            this.WorkStationsListBox.IntegralHeight = ((bool)(resources.GetObject("WorkStationsListBox.IntegralHeight")));
            this.WorkStationsListBox.Location = ((System.Drawing.Point)(resources.GetObject("WorkStationsListBox.Location")));
            this.WorkStationsListBox.Name = "WorkStationsListBox";
            this.WorkStationsListBox.RightToLeft = ((System.Windows.Forms.RightToLeft)(resources.GetObject("WorkStationsListBox.RightToLeft")));
            this.WorkStationsListBox.ScrollAlwaysVisible = ((bool)(resources.GetObject("WorkStationsListBox.ScrollAlwaysVisible")));
            this.WorkStationsListBox.Size = ((System.Drawing.Size)(resources.GetObject("WorkStationsListBox.Size")));
            this.WorkStationsListBox.Sorted = true;
            this.WorkStationsListBox.TabIndex = ((int)(resources.GetObject("WorkStationsListBox.TabIndex")));
            this.WorkStationsListBox.ThreeDCheckBoxes = true;
            this.WorkStationsListBox.Visible = ((bool)(resources.GetObject("WorkStationsListBox.Visible")));
            // 
            // SelectWorkstationDlg
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
            this.Controls.Add(this.WorkStationsListBox);
            this.Controls.Add(this.CancelBtn);
            this.Controls.Add(this.OkBtn);
            this.Enabled = ((bool)(resources.GetObject("$this.Enabled")));
            this.Font = ((System.Drawing.Font)(resources.GetObject("$this.Font")));
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedDialog;
            this.Icon = ((System.Drawing.Icon)(resources.GetObject("$this.Icon")));
            this.ImeMode = ((System.Windows.Forms.ImeMode)(resources.GetObject("$this.ImeMode")));
            this.Location = ((System.Drawing.Point)(resources.GetObject("$this.Location")));
            this.MaximizeBox = false;
            this.MaximumSize = ((System.Drawing.Size)(resources.GetObject("$this.MaximumSize")));
            this.MinimizeBox = false;
            this.MinimumSize = ((System.Drawing.Size)(resources.GetObject("$this.MinimumSize")));
            this.Name = "SelectWorkstationDlg";
            this.RightToLeft = ((System.Windows.Forms.RightToLeft)(resources.GetObject("$this.RightToLeft")));
            this.ShowInTaskbar = false;
            this.StartPosition = ((System.Windows.Forms.FormStartPosition)(resources.GetObject("$this.StartPosition")));
            this.Text = resources.GetString("$this.Text");
            this.ResumeLayout(false);

        }
        #endregion
    }
}
