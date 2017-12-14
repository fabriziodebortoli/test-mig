using System.Windows.Forms;

namespace Microarea.Console.Plugin.SysAdmin.Form
{
    partial class ConfirmPassword
    {
        private Label lblNewPassword;
        private Button btnOk;
        private Button btnCancel;
        protected TextBox txtNewPassword;
        private System.Windows.Forms.PictureBox ImageKey;
        /// <summary>
        /// Required designer variable.
        /// </summary>
        private System.ComponentModel.Container components = null;

        /// <summary>
        /// Clean up any resources being used.
        /// </summary>
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
            System.Resources.ResourceManager resources = new System.Resources.ResourceManager(typeof(ConfirmPassword));
            this.lblNewPassword = new System.Windows.Forms.Label();
            this.btnOk = new System.Windows.Forms.Button();
            this.btnCancel = new System.Windows.Forms.Button();
            this.txtNewPassword = new System.Windows.Forms.TextBox();
            this.ImageKey = new System.Windows.Forms.PictureBox();
            this.SuspendLayout();
            // 
            // lblNewPassword
            // 
            this.lblNewPassword.AccessibleDescription = resources.GetString("lblNewPassword.AccessibleDescription");
            this.lblNewPassword.AccessibleName = resources.GetString("lblNewPassword.AccessibleName");
            this.lblNewPassword.Anchor = ((System.Windows.Forms.AnchorStyles)(resources.GetObject("lblNewPassword.Anchor")));
            this.lblNewPassword.AutoSize = ((bool)(resources.GetObject("lblNewPassword.AutoSize")));
            this.lblNewPassword.Dock = ((System.Windows.Forms.DockStyle)(resources.GetObject("lblNewPassword.Dock")));
            this.lblNewPassword.Enabled = ((bool)(resources.GetObject("lblNewPassword.Enabled")));
            this.lblNewPassword.Font = ((System.Drawing.Font)(resources.GetObject("lblNewPassword.Font")));
            this.lblNewPassword.Image = ((System.Drawing.Image)(resources.GetObject("lblNewPassword.Image")));
            this.lblNewPassword.ImageAlign = ((System.Drawing.ContentAlignment)(resources.GetObject("lblNewPassword.ImageAlign")));
            this.lblNewPassword.ImageIndex = ((int)(resources.GetObject("lblNewPassword.ImageIndex")));
            this.lblNewPassword.ImeMode = ((System.Windows.Forms.ImeMode)(resources.GetObject("lblNewPassword.ImeMode")));
            this.lblNewPassword.Location = ((System.Drawing.Point)(resources.GetObject("lblNewPassword.Location")));
            this.lblNewPassword.Name = "lblNewPassword";
            this.lblNewPassword.RightToLeft = ((System.Windows.Forms.RightToLeft)(resources.GetObject("lblNewPassword.RightToLeft")));
            this.lblNewPassword.Size = ((System.Drawing.Size)(resources.GetObject("lblNewPassword.Size")));
            this.lblNewPassword.TabIndex = ((int)(resources.GetObject("lblNewPassword.TabIndex")));
            this.lblNewPassword.Text = resources.GetString("lblNewPassword.Text");
            this.lblNewPassword.TextAlign = ((System.Drawing.ContentAlignment)(resources.GetObject("lblNewPassword.TextAlign")));
            this.lblNewPassword.Visible = ((bool)(resources.GetObject("lblNewPassword.Visible")));
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
            // txtNewPassword
            // 
            this.txtNewPassword.AcceptsReturn = true;
            this.txtNewPassword.AcceptsTab = true;
            this.txtNewPassword.AccessibleDescription = resources.GetString("txtNewPassword.AccessibleDescription");
            this.txtNewPassword.AccessibleName = resources.GetString("txtNewPassword.AccessibleName");
            this.txtNewPassword.Anchor = ((System.Windows.Forms.AnchorStyles)(resources.GetObject("txtNewPassword.Anchor")));
            this.txtNewPassword.AutoSize = ((bool)(resources.GetObject("txtNewPassword.AutoSize")));
            this.txtNewPassword.BackgroundImage = ((System.Drawing.Image)(resources.GetObject("txtNewPassword.BackgroundImage")));
            this.txtNewPassword.Dock = ((System.Windows.Forms.DockStyle)(resources.GetObject("txtNewPassword.Dock")));
            this.txtNewPassword.Enabled = ((bool)(resources.GetObject("txtNewPassword.Enabled")));
            this.txtNewPassword.Font = ((System.Drawing.Font)(resources.GetObject("txtNewPassword.Font")));
            this.txtNewPassword.ImeMode = ((System.Windows.Forms.ImeMode)(resources.GetObject("txtNewPassword.ImeMode")));
            this.txtNewPassword.Location = ((System.Drawing.Point)(resources.GetObject("txtNewPassword.Location")));
            this.txtNewPassword.MaxLength = ((int)(resources.GetObject("txtNewPassword.MaxLength")));
            this.txtNewPassword.Multiline = ((bool)(resources.GetObject("txtNewPassword.Multiline")));
            this.txtNewPassword.Name = "txtNewPassword";
            this.txtNewPassword.PasswordChar = ((char)(resources.GetObject("txtNewPassword.PasswordChar")));
            this.txtNewPassword.RightToLeft = ((System.Windows.Forms.RightToLeft)(resources.GetObject("txtNewPassword.RightToLeft")));
            this.txtNewPassword.ScrollBars = ((System.Windows.Forms.ScrollBars)(resources.GetObject("txtNewPassword.ScrollBars")));
            this.txtNewPassword.Size = ((System.Drawing.Size)(resources.GetObject("txtNewPassword.Size")));
            this.txtNewPassword.TabIndex = ((int)(resources.GetObject("txtNewPassword.TabIndex")));
            this.txtNewPassword.Text = resources.GetString("txtNewPassword.Text");
            this.txtNewPassword.TextAlign = ((System.Windows.Forms.HorizontalAlignment)(resources.GetObject("txtNewPassword.TextAlign")));
            this.txtNewPassword.Visible = ((bool)(resources.GetObject("txtNewPassword.Visible")));
            this.txtNewPassword.WordWrap = ((bool)(resources.GetObject("txtNewPassword.WordWrap")));
            // 
            // ImageKey
            // 
            this.ImageKey.AccessibleDescription = resources.GetString("ImageKey.AccessibleDescription");
            this.ImageKey.AccessibleName = resources.GetString("ImageKey.AccessibleName");
            this.ImageKey.Anchor = ((System.Windows.Forms.AnchorStyles)(resources.GetObject("ImageKey.Anchor")));
            this.ImageKey.BackgroundImage = ((System.Drawing.Image)(resources.GetObject("ImageKey.BackgroundImage")));
            this.ImageKey.Dock = ((System.Windows.Forms.DockStyle)(resources.GetObject("ImageKey.Dock")));
            this.ImageKey.Enabled = ((bool)(resources.GetObject("ImageKey.Enabled")));
            this.ImageKey.Font = ((System.Drawing.Font)(resources.GetObject("ImageKey.Font")));
            this.ImageKey.Image = ((System.Drawing.Image)(resources.GetObject("ImageKey.Image")));
            this.ImageKey.ImeMode = ((System.Windows.Forms.ImeMode)(resources.GetObject("ImageKey.ImeMode")));
            this.ImageKey.Location = ((System.Drawing.Point)(resources.GetObject("ImageKey.Location")));
            this.ImageKey.Name = "ImageKey";
            this.ImageKey.RightToLeft = ((System.Windows.Forms.RightToLeft)(resources.GetObject("ImageKey.RightToLeft")));
            this.ImageKey.Size = ((System.Drawing.Size)(resources.GetObject("ImageKey.Size")));
            this.ImageKey.SizeMode = ((System.Windows.Forms.PictureBoxSizeMode)(resources.GetObject("ImageKey.SizeMode")));
            this.ImageKey.TabIndex = ((int)(resources.GetObject("ImageKey.TabIndex")));
            this.ImageKey.TabStop = false;
            this.ImageKey.Text = resources.GetString("ImageKey.Text");
            this.ImageKey.Visible = ((bool)(resources.GetObject("ImageKey.Visible")));
            // 
            // ConfirmPassword
            // 
            this.AcceptButton = this.btnOk;
            this.AccessibleDescription = resources.GetString("$this.AccessibleDescription");
            this.AccessibleName = resources.GetString("$this.AccessibleName");
            this.AutoScaleBaseSize = ((System.Drawing.Size)(resources.GetObject("$this.AutoScaleBaseSize")));
            this.AutoScroll = ((bool)(resources.GetObject("$this.AutoScroll")));
            this.AutoScrollMargin = ((System.Drawing.Size)(resources.GetObject("$this.AutoScrollMargin")));
            this.AutoScrollMinSize = ((System.Drawing.Size)(resources.GetObject("$this.AutoScrollMinSize")));
            this.BackgroundImage = ((System.Drawing.Image)(resources.GetObject("$this.BackgroundImage")));
            this.ClientSize = ((System.Drawing.Size)(resources.GetObject("$this.ClientSize")));
            this.Controls.Add(this.ImageKey);
            this.Controls.Add(this.txtNewPassword);
            this.Controls.Add(this.lblNewPassword);
            this.Controls.Add(this.btnCancel);
            this.Controls.Add(this.btnOk);
            this.Enabled = ((bool)(resources.GetObject("$this.Enabled")));
            this.Font = ((System.Drawing.Font)(resources.GetObject("$this.Font")));
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedDialog;
            this.Icon = ((System.Drawing.Icon)(resources.GetObject("$this.Icon")));
            this.ImeMode = ((System.Windows.Forms.ImeMode)(resources.GetObject("$this.ImeMode")));
            this.Location = ((System.Drawing.Point)(resources.GetObject("$this.Location")));
            this.MaximumSize = ((System.Drawing.Size)(resources.GetObject("$this.MaximumSize")));
            this.MinimumSize = ((System.Drawing.Size)(resources.GetObject("$this.MinimumSize")));
            this.Name = "ConfirmPassword";
            this.RightToLeft = ((System.Windows.Forms.RightToLeft)(resources.GetObject("$this.RightToLeft")));
            this.ShowInTaskbar = false;
            this.StartPosition = ((System.Windows.Forms.FormStartPosition)(resources.GetObject("$this.StartPosition")));
            this.Text = resources.GetString("$this.Text");
            this.TopMost = true;
            this.ResumeLayout(false);

        }
        #endregion
    }
}
