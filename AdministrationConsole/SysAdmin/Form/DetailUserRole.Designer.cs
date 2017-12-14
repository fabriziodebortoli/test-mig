using System.ComponentModel;
using System.Windows.Forms;

namespace Microarea.Console.Plugin.SysAdmin.Form
{
    partial class DetailUserRole
    {
        private Label lblLogin;
        private Label lblDescrizione;
        private Label LabelTitle;
        private Label LblExplication;

        private TextBox tbLogin;
        private TextBox tbDescription;
        /// <summary>
        /// Required designer variable.
        /// </summary>
        private Container components = null;

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
            System.Resources.ResourceManager resources = new System.Resources.ResourceManager(typeof(DetailUserRole));
            this.lblLogin = new System.Windows.Forms.Label();
            this.tbLogin = new System.Windows.Forms.TextBox();
            this.lblDescrizione = new System.Windows.Forms.Label();
            this.tbDescription = new System.Windows.Forms.TextBox();
            this.LabelTitle = new System.Windows.Forms.Label();
            this.LblExplication = new System.Windows.Forms.Label();
            this.SuspendLayout();
            // 
            // lblLogin
            // 
            this.lblLogin.AccessibleDescription = resources.GetString("lblLogin.AccessibleDescription");
            this.lblLogin.AccessibleName = resources.GetString("lblLogin.AccessibleName");
            this.lblLogin.Anchor = ((System.Windows.Forms.AnchorStyles)(resources.GetObject("lblLogin.Anchor")));
            this.lblLogin.AutoSize = ((bool)(resources.GetObject("lblLogin.AutoSize")));
            this.lblLogin.Dock = ((System.Windows.Forms.DockStyle)(resources.GetObject("lblLogin.Dock")));
            this.lblLogin.Enabled = ((bool)(resources.GetObject("lblLogin.Enabled")));
            this.lblLogin.Font = ((System.Drawing.Font)(resources.GetObject("lblLogin.Font")));
            this.lblLogin.Image = ((System.Drawing.Image)(resources.GetObject("lblLogin.Image")));
            this.lblLogin.ImageAlign = ((System.Drawing.ContentAlignment)(resources.GetObject("lblLogin.ImageAlign")));
            this.lblLogin.ImageIndex = ((int)(resources.GetObject("lblLogin.ImageIndex")));
            this.lblLogin.ImeMode = ((System.Windows.Forms.ImeMode)(resources.GetObject("lblLogin.ImeMode")));
            this.lblLogin.Location = ((System.Drawing.Point)(resources.GetObject("lblLogin.Location")));
            this.lblLogin.Name = "lblLogin";
            this.lblLogin.RightToLeft = ((System.Windows.Forms.RightToLeft)(resources.GetObject("lblLogin.RightToLeft")));
            this.lblLogin.Size = ((System.Drawing.Size)(resources.GetObject("lblLogin.Size")));
            this.lblLogin.TabIndex = ((int)(resources.GetObject("lblLogin.TabIndex")));
            this.lblLogin.Text = resources.GetString("lblLogin.Text");
            this.lblLogin.TextAlign = ((System.Drawing.ContentAlignment)(resources.GetObject("lblLogin.TextAlign")));
            this.lblLogin.Visible = ((bool)(resources.GetObject("lblLogin.Visible")));
            // 
            // tbLogin
            // 
            this.tbLogin.AccessibleDescription = resources.GetString("tbLogin.AccessibleDescription");
            this.tbLogin.AccessibleName = resources.GetString("tbLogin.AccessibleName");
            this.tbLogin.Anchor = ((System.Windows.Forms.AnchorStyles)(resources.GetObject("tbLogin.Anchor")));
            this.tbLogin.AutoSize = ((bool)(resources.GetObject("tbLogin.AutoSize")));
            this.tbLogin.BackgroundImage = ((System.Drawing.Image)(resources.GetObject("tbLogin.BackgroundImage")));
            this.tbLogin.Dock = ((System.Windows.Forms.DockStyle)(resources.GetObject("tbLogin.Dock")));
            this.tbLogin.Enabled = ((bool)(resources.GetObject("tbLogin.Enabled")));
            this.tbLogin.Font = ((System.Drawing.Font)(resources.GetObject("tbLogin.Font")));
            this.tbLogin.ImeMode = ((System.Windows.Forms.ImeMode)(resources.GetObject("tbLogin.ImeMode")));
            this.tbLogin.Location = ((System.Drawing.Point)(resources.GetObject("tbLogin.Location")));
            this.tbLogin.MaxLength = ((int)(resources.GetObject("tbLogin.MaxLength")));
            this.tbLogin.Multiline = ((bool)(resources.GetObject("tbLogin.Multiline")));
            this.tbLogin.Name = "tbLogin";
            this.tbLogin.PasswordChar = ((char)(resources.GetObject("tbLogin.PasswordChar")));
            this.tbLogin.ReadOnly = true;
            this.tbLogin.RightToLeft = ((System.Windows.Forms.RightToLeft)(resources.GetObject("tbLogin.RightToLeft")));
            this.tbLogin.ScrollBars = ((System.Windows.Forms.ScrollBars)(resources.GetObject("tbLogin.ScrollBars")));
            this.tbLogin.Size = ((System.Drawing.Size)(resources.GetObject("tbLogin.Size")));
            this.tbLogin.TabIndex = ((int)(resources.GetObject("tbLogin.TabIndex")));
            this.tbLogin.Text = resources.GetString("tbLogin.Text");
            this.tbLogin.TextAlign = ((System.Windows.Forms.HorizontalAlignment)(resources.GetObject("tbLogin.TextAlign")));
            this.tbLogin.Visible = ((bool)(resources.GetObject("tbLogin.Visible")));
            this.tbLogin.WordWrap = ((bool)(resources.GetObject("tbLogin.WordWrap")));
            // 
            // lblDescrizione
            // 
            this.lblDescrizione.AccessibleDescription = resources.GetString("lblDescrizione.AccessibleDescription");
            this.lblDescrizione.AccessibleName = resources.GetString("lblDescrizione.AccessibleName");
            this.lblDescrizione.Anchor = ((System.Windows.Forms.AnchorStyles)(resources.GetObject("lblDescrizione.Anchor")));
            this.lblDescrizione.AutoSize = ((bool)(resources.GetObject("lblDescrizione.AutoSize")));
            this.lblDescrizione.Dock = ((System.Windows.Forms.DockStyle)(resources.GetObject("lblDescrizione.Dock")));
            this.lblDescrizione.Enabled = ((bool)(resources.GetObject("lblDescrizione.Enabled")));
            this.lblDescrizione.Font = ((System.Drawing.Font)(resources.GetObject("lblDescrizione.Font")));
            this.lblDescrizione.Image = ((System.Drawing.Image)(resources.GetObject("lblDescrizione.Image")));
            this.lblDescrizione.ImageAlign = ((System.Drawing.ContentAlignment)(resources.GetObject("lblDescrizione.ImageAlign")));
            this.lblDescrizione.ImageIndex = ((int)(resources.GetObject("lblDescrizione.ImageIndex")));
            this.lblDescrizione.ImeMode = ((System.Windows.Forms.ImeMode)(resources.GetObject("lblDescrizione.ImeMode")));
            this.lblDescrizione.Location = ((System.Drawing.Point)(resources.GetObject("lblDescrizione.Location")));
            this.lblDescrizione.Name = "lblDescrizione";
            this.lblDescrizione.RightToLeft = ((System.Windows.Forms.RightToLeft)(resources.GetObject("lblDescrizione.RightToLeft")));
            this.lblDescrizione.Size = ((System.Drawing.Size)(resources.GetObject("lblDescrizione.Size")));
            this.lblDescrizione.TabIndex = ((int)(resources.GetObject("lblDescrizione.TabIndex")));
            this.lblDescrizione.Text = resources.GetString("lblDescrizione.Text");
            this.lblDescrizione.TextAlign = ((System.Drawing.ContentAlignment)(resources.GetObject("lblDescrizione.TextAlign")));
            this.lblDescrizione.Visible = ((bool)(resources.GetObject("lblDescrizione.Visible")));
            // 
            // tbDescription
            // 
            this.tbDescription.AccessibleDescription = resources.GetString("tbDescription.AccessibleDescription");
            this.tbDescription.AccessibleName = resources.GetString("tbDescription.AccessibleName");
            this.tbDescription.Anchor = ((System.Windows.Forms.AnchorStyles)(resources.GetObject("tbDescription.Anchor")));
            this.tbDescription.AutoSize = ((bool)(resources.GetObject("tbDescription.AutoSize")));
            this.tbDescription.BackgroundImage = ((System.Drawing.Image)(resources.GetObject("tbDescription.BackgroundImage")));
            this.tbDescription.Dock = ((System.Windows.Forms.DockStyle)(resources.GetObject("tbDescription.Dock")));
            this.tbDescription.Enabled = ((bool)(resources.GetObject("tbDescription.Enabled")));
            this.tbDescription.Font = ((System.Drawing.Font)(resources.GetObject("tbDescription.Font")));
            this.tbDescription.ImeMode = ((System.Windows.Forms.ImeMode)(resources.GetObject("tbDescription.ImeMode")));
            this.tbDescription.Location = ((System.Drawing.Point)(resources.GetObject("tbDescription.Location")));
            this.tbDescription.MaxLength = ((int)(resources.GetObject("tbDescription.MaxLength")));
            this.tbDescription.Multiline = ((bool)(resources.GetObject("tbDescription.Multiline")));
            this.tbDescription.Name = "tbDescription";
            this.tbDescription.PasswordChar = ((char)(resources.GetObject("tbDescription.PasswordChar")));
            this.tbDescription.ReadOnly = true;
            this.tbDescription.RightToLeft = ((System.Windows.Forms.RightToLeft)(resources.GetObject("tbDescription.RightToLeft")));
            this.tbDescription.ScrollBars = ((System.Windows.Forms.ScrollBars)(resources.GetObject("tbDescription.ScrollBars")));
            this.tbDescription.Size = ((System.Drawing.Size)(resources.GetObject("tbDescription.Size")));
            this.tbDescription.TabIndex = ((int)(resources.GetObject("tbDescription.TabIndex")));
            this.tbDescription.Text = resources.GetString("tbDescription.Text");
            this.tbDescription.TextAlign = ((System.Windows.Forms.HorizontalAlignment)(resources.GetObject("tbDescription.TextAlign")));
            this.tbDescription.Visible = ((bool)(resources.GetObject("tbDescription.Visible")));
            this.tbDescription.WordWrap = ((bool)(resources.GetObject("tbDescription.WordWrap")));
            // 
            // LabelTitle
            // 
            this.LabelTitle.AccessibleDescription = resources.GetString("LabelTitle.AccessibleDescription");
            this.LabelTitle.AccessibleName = resources.GetString("LabelTitle.AccessibleName");
            this.LabelTitle.Anchor = ((System.Windows.Forms.AnchorStyles)(resources.GetObject("LabelTitle.Anchor")));
            this.LabelTitle.AutoSize = ((bool)(resources.GetObject("LabelTitle.AutoSize")));
            this.LabelTitle.BackColor = System.Drawing.Color.CornflowerBlue;
            this.LabelTitle.BorderStyle = System.Windows.Forms.BorderStyle.Fixed3D;
            this.LabelTitle.Dock = ((System.Windows.Forms.DockStyle)(resources.GetObject("LabelTitle.Dock")));
            this.LabelTitle.Enabled = ((bool)(resources.GetObject("LabelTitle.Enabled")));
            this.LabelTitle.FlatStyle = System.Windows.Forms.FlatStyle.System;
            this.LabelTitle.Font = ((System.Drawing.Font)(resources.GetObject("LabelTitle.Font")));
            this.LabelTitle.ForeColor = System.Drawing.Color.White;
            this.LabelTitle.Image = ((System.Drawing.Image)(resources.GetObject("LabelTitle.Image")));
            this.LabelTitle.ImageAlign = ((System.Drawing.ContentAlignment)(resources.GetObject("LabelTitle.ImageAlign")));
            this.LabelTitle.ImageIndex = ((int)(resources.GetObject("LabelTitle.ImageIndex")));
            this.LabelTitle.ImeMode = ((System.Windows.Forms.ImeMode)(resources.GetObject("LabelTitle.ImeMode")));
            this.LabelTitle.Location = ((System.Drawing.Point)(resources.GetObject("LabelTitle.Location")));
            this.LabelTitle.Name = "LabelTitle";
            this.LabelTitle.RightToLeft = ((System.Windows.Forms.RightToLeft)(resources.GetObject("LabelTitle.RightToLeft")));
            this.LabelTitle.Size = ((System.Drawing.Size)(resources.GetObject("LabelTitle.Size")));
            this.LabelTitle.TabIndex = ((int)(resources.GetObject("LabelTitle.TabIndex")));
            this.LabelTitle.Text = resources.GetString("LabelTitle.Text");
            this.LabelTitle.TextAlign = ((System.Drawing.ContentAlignment)(resources.GetObject("LabelTitle.TextAlign")));
            this.LabelTitle.Visible = ((bool)(resources.GetObject("LabelTitle.Visible")));
            // 
            // LblExplication
            // 
            this.LblExplication.AccessibleDescription = resources.GetString("LblExplication.AccessibleDescription");
            this.LblExplication.AccessibleName = resources.GetString("LblExplication.AccessibleName");
            this.LblExplication.Anchor = ((System.Windows.Forms.AnchorStyles)(resources.GetObject("LblExplication.Anchor")));
            this.LblExplication.AutoSize = ((bool)(resources.GetObject("LblExplication.AutoSize")));
            this.LblExplication.Dock = ((System.Windows.Forms.DockStyle)(resources.GetObject("LblExplication.Dock")));
            this.LblExplication.Enabled = ((bool)(resources.GetObject("LblExplication.Enabled")));
            this.LblExplication.FlatStyle = System.Windows.Forms.FlatStyle.System;
            this.LblExplication.Font = ((System.Drawing.Font)(resources.GetObject("LblExplication.Font")));
            this.LblExplication.Image = ((System.Drawing.Image)(resources.GetObject("LblExplication.Image")));
            this.LblExplication.ImageAlign = ((System.Drawing.ContentAlignment)(resources.GetObject("LblExplication.ImageAlign")));
            this.LblExplication.ImageIndex = ((int)(resources.GetObject("LblExplication.ImageIndex")));
            this.LblExplication.ImeMode = ((System.Windows.Forms.ImeMode)(resources.GetObject("LblExplication.ImeMode")));
            this.LblExplication.Location = ((System.Drawing.Point)(resources.GetObject("LblExplication.Location")));
            this.LblExplication.Name = "LblExplication";
            this.LblExplication.RightToLeft = ((System.Windows.Forms.RightToLeft)(resources.GetObject("LblExplication.RightToLeft")));
            this.LblExplication.Size = ((System.Drawing.Size)(resources.GetObject("LblExplication.Size")));
            this.LblExplication.TabIndex = ((int)(resources.GetObject("LblExplication.TabIndex")));
            this.LblExplication.Text = resources.GetString("LblExplication.Text");
            this.LblExplication.TextAlign = ((System.Drawing.ContentAlignment)(resources.GetObject("LblExplication.TextAlign")));
            this.LblExplication.Visible = ((bool)(resources.GetObject("LblExplication.Visible")));
            // 
            // DetailUserRole
            // 
            this.AccessibleDescription = resources.GetString("$this.AccessibleDescription");
            this.AccessibleName = resources.GetString("$this.AccessibleName");
            this.AutoScaleBaseSize = ((System.Drawing.Size)(resources.GetObject("$this.AutoScaleBaseSize")));
            this.AutoScroll = ((bool)(resources.GetObject("$this.AutoScroll")));
            this.AutoScrollMargin = ((System.Drawing.Size)(resources.GetObject("$this.AutoScrollMargin")));
            this.AutoScrollMinSize = ((System.Drawing.Size)(resources.GetObject("$this.AutoScrollMinSize")));
            this.BackgroundImage = ((System.Drawing.Image)(resources.GetObject("$this.BackgroundImage")));
            this.ClientSize = ((System.Drawing.Size)(resources.GetObject("$this.ClientSize")));
            this.Controls.Add(this.LblExplication);
            this.Controls.Add(this.LabelTitle);
            this.Controls.Add(this.tbDescription);
            this.Controls.Add(this.lblDescrizione);
            this.Controls.Add(this.tbLogin);
            this.Controls.Add(this.lblLogin);
            this.Enabled = ((bool)(resources.GetObject("$this.Enabled")));
            this.Font = ((System.Drawing.Font)(resources.GetObject("$this.Font")));
            this.Icon = ((System.Drawing.Icon)(resources.GetObject("$this.Icon")));
            this.ImeMode = ((System.Windows.Forms.ImeMode)(resources.GetObject("$this.ImeMode")));
            this.Location = ((System.Drawing.Point)(resources.GetObject("$this.Location")));
            this.MaximumSize = ((System.Drawing.Size)(resources.GetObject("$this.MaximumSize")));
            this.MinimumSize = ((System.Drawing.Size)(resources.GetObject("$this.MinimumSize")));
            this.Name = "DetailUserRole";
            this.RightToLeft = ((System.Windows.Forms.RightToLeft)(resources.GetObject("$this.RightToLeft")));
            this.StartPosition = ((System.Windows.Forms.FormStartPosition)(resources.GetObject("$this.StartPosition")));
            this.Text = resources.GetString("$this.Text");
            this.Closing += new System.ComponentModel.CancelEventHandler(this.DetailUserRole_Closing);
            this.VisibleChanged += new System.EventHandler(this.DetailUserRole_VisibleChanged);
            this.Deactivate += new System.EventHandler(this.DetailUserRole_Deactivate);
            this.ResumeLayout(false);

        }
        #endregion
    }
}
