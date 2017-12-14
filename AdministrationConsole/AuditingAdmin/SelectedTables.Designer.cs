
namespace Microarea.Console.Plugin.AuditingAdmin
{
    partial class SelectedTables
    {
        private System.Windows.Forms.ListView lstTables;
        private System.ComponentModel.Container components = null;
        private System.Windows.Forms.Button btnOk;
        private System.Windows.Forms.Button btnCancel;

        //---------------------------------------------------------------------
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
            System.Resources.ResourceManager resources = new System.Resources.ResourceManager(typeof(SelectedTables));
            this.lstTables = new System.Windows.Forms.ListView();
            this.btnOk = new System.Windows.Forms.Button();
            this.btnCancel = new System.Windows.Forms.Button();
            this.SuspendLayout();
            // 
            // lstTables
            // 
            this.lstTables.AccessibleDescription = resources.GetString("lstTables.AccessibleDescription");
            this.lstTables.AccessibleName = resources.GetString("lstTables.AccessibleName");
            this.lstTables.Alignment = ((System.Windows.Forms.ListViewAlignment)(resources.GetObject("lstTables.Alignment")));
            this.lstTables.Anchor = ((System.Windows.Forms.AnchorStyles)(resources.GetObject("lstTables.Anchor")));
            this.lstTables.BackgroundImage = ((System.Drawing.Image)(resources.GetObject("lstTables.BackgroundImage")));
            this.lstTables.Dock = ((System.Windows.Forms.DockStyle)(resources.GetObject("lstTables.Dock")));
            this.lstTables.Enabled = ((bool)(resources.GetObject("lstTables.Enabled")));
            this.lstTables.Font = ((System.Drawing.Font)(resources.GetObject("lstTables.Font")));
            this.lstTables.ImeMode = ((System.Windows.Forms.ImeMode)(resources.GetObject("lstTables.ImeMode")));
            this.lstTables.LabelWrap = ((bool)(resources.GetObject("lstTables.LabelWrap")));
            this.lstTables.Location = ((System.Drawing.Point)(resources.GetObject("lstTables.Location")));
            this.lstTables.Name = "lstTables";
            this.lstTables.RightToLeft = ((System.Windows.Forms.RightToLeft)(resources.GetObject("lstTables.RightToLeft")));
            this.lstTables.Size = ((System.Drawing.Size)(resources.GetObject("lstTables.Size")));
            this.lstTables.TabIndex = ((int)(resources.GetObject("lstTables.TabIndex")));
            this.lstTables.Text = resources.GetString("lstTables.Text");
            this.lstTables.Visible = ((bool)(resources.GetObject("lstTables.Visible")));
            // 
            // btnOk
            // 
            this.btnOk.AccessibleDescription = resources.GetString("btnOk.AccessibleDescription");
            this.btnOk.AccessibleName = resources.GetString("btnOk.AccessibleName");
            this.btnOk.Anchor = ((System.Windows.Forms.AnchorStyles)(resources.GetObject("btnOk.Anchor")));
            this.btnOk.BackgroundImage = ((System.Drawing.Image)(resources.GetObject("btnOk.BackgroundImage")));
            this.btnOk.Dock = ((System.Windows.Forms.DockStyle)(resources.GetObject("btnOk.Dock")));
            this.btnOk.Enabled = ((bool)(resources.GetObject("btnOk.Enabled")));
            this.btnOk.FlatStyle = ((System.Windows.Forms.FlatStyle)(resources.GetObject("btnOk.FlatStyle")));
            this.btnOk.Font = ((System.Drawing.Font)(resources.GetObject("btnOk.Font")));
            this.btnOk.Image = ((System.Drawing.Image)(resources.GetObject("btnOk.Image")));
            this.btnOk.ImageAlign = ((System.Drawing.ContentAlignment)(resources.GetObject("btnOk.ImageAlign")));
            this.btnOk.ImageIndex = ((int)(resources.GetObject("btnOk.ImageIndex")));
            this.btnOk.ImeMode = ((System.Windows.Forms.ImeMode)(resources.GetObject("btnOk.ImeMode")));
            this.btnOk.Location = ((System.Drawing.Point)(resources.GetObject("btnOk.Location")));
            this.btnOk.Name = "btnOk";
            this.btnOk.RightToLeft = ((System.Windows.Forms.RightToLeft)(resources.GetObject("btnOk.RightToLeft")));
            this.btnOk.Size = ((System.Drawing.Size)(resources.GetObject("btnOk.Size")));
            this.btnOk.TabIndex = ((int)(resources.GetObject("btnOk.TabIndex")));
            this.btnOk.Text = resources.GetString("btnOk.Text");
            this.btnOk.TextAlign = ((System.Drawing.ContentAlignment)(resources.GetObject("btnOk.TextAlign")));
            this.btnOk.Visible = ((bool)(resources.GetObject("btnOk.Visible")));
            this.btnOk.Click += new System.EventHandler(this.btnOk_Click);
            // 
            // btnCancel
            // 
            this.btnCancel.AccessibleDescription = resources.GetString("btnCancel.AccessibleDescription");
            this.btnCancel.AccessibleName = resources.GetString("btnCancel.AccessibleName");
            this.btnCancel.Anchor = ((System.Windows.Forms.AnchorStyles)(resources.GetObject("btnCancel.Anchor")));
            this.btnCancel.BackgroundImage = ((System.Drawing.Image)(resources.GetObject("btnCancel.BackgroundImage")));
            this.btnCancel.Dock = ((System.Windows.Forms.DockStyle)(resources.GetObject("btnCancel.Dock")));
            this.btnCancel.Enabled = ((bool)(resources.GetObject("btnCancel.Enabled")));
            this.btnCancel.FlatStyle = ((System.Windows.Forms.FlatStyle)(resources.GetObject("btnCancel.FlatStyle")));
            this.btnCancel.Font = ((System.Drawing.Font)(resources.GetObject("btnCancel.Font")));
            this.btnCancel.Image = ((System.Drawing.Image)(resources.GetObject("btnCancel.Image")));
            this.btnCancel.ImageAlign = ((System.Drawing.ContentAlignment)(resources.GetObject("btnCancel.ImageAlign")));
            this.btnCancel.ImageIndex = ((int)(resources.GetObject("btnCancel.ImageIndex")));
            this.btnCancel.ImeMode = ((System.Windows.Forms.ImeMode)(resources.GetObject("btnCancel.ImeMode")));
            this.btnCancel.Location = ((System.Drawing.Point)(resources.GetObject("btnCancel.Location")));
            this.btnCancel.Name = "btnCancel";
            this.btnCancel.RightToLeft = ((System.Windows.Forms.RightToLeft)(resources.GetObject("btnCancel.RightToLeft")));
            this.btnCancel.Size = ((System.Drawing.Size)(resources.GetObject("btnCancel.Size")));
            this.btnCancel.TabIndex = ((int)(resources.GetObject("btnCancel.TabIndex")));
            this.btnCancel.Text = resources.GetString("btnCancel.Text");
            this.btnCancel.TextAlign = ((System.Drawing.ContentAlignment)(resources.GetObject("btnCancel.TextAlign")));
            this.btnCancel.Visible = ((bool)(resources.GetObject("btnCancel.Visible")));
            this.btnCancel.Click += new System.EventHandler(this.btnCancel_Click);
            // 
            // SelectedTables
            // 
            this.AccessibleDescription = resources.GetString("$this.AccessibleDescription");
            this.AccessibleName = resources.GetString("$this.AccessibleName");
            this.AutoScaleBaseSize = ((System.Drawing.Size)(resources.GetObject("$this.AutoScaleBaseSize")));
            this.AutoScroll = ((bool)(resources.GetObject("$this.AutoScroll")));
            this.AutoScrollMargin = ((System.Drawing.Size)(resources.GetObject("$this.AutoScrollMargin")));
            this.AutoScrollMinSize = ((System.Drawing.Size)(resources.GetObject("$this.AutoScrollMinSize")));
            this.BackgroundImage = ((System.Drawing.Image)(resources.GetObject("$this.BackgroundImage")));
            this.ClientSize = ((System.Drawing.Size)(resources.GetObject("$this.ClientSize")));
            this.Controls.Add(this.btnCancel);
            this.Controls.Add(this.btnOk);
            this.Controls.Add(this.lstTables);
            this.Enabled = ((bool)(resources.GetObject("$this.Enabled")));
            this.Font = ((System.Drawing.Font)(resources.GetObject("$this.Font")));
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedSingle;
            this.Icon = ((System.Drawing.Icon)(resources.GetObject("$this.Icon")));
            this.ImeMode = ((System.Windows.Forms.ImeMode)(resources.GetObject("$this.ImeMode")));
            this.Location = ((System.Drawing.Point)(resources.GetObject("$this.Location")));
            this.MaximizeBox = false;
            this.MaximumSize = ((System.Drawing.Size)(resources.GetObject("$this.MaximumSize")));
            this.MinimizeBox = false;
            this.MinimumSize = ((System.Drawing.Size)(resources.GetObject("$this.MinimumSize")));
            this.Name = "SelectedTables";
            this.RightToLeft = ((System.Windows.Forms.RightToLeft)(resources.GetObject("$this.RightToLeft")));
            this.StartPosition = ((System.Windows.Forms.FormStartPosition)(resources.GetObject("$this.StartPosition")));
            this.Text = resources.GetString("$this.Text");
            this.Closed += new System.EventHandler(this.SelectedTables_Closed);
            this.ResumeLayout(false);

        }
        #endregion	

    }
}
