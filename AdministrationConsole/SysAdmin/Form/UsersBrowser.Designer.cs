using System.Windows.Forms;
using Microarea.TaskBuilderNet.UI.WinControls.Lists;

namespace Microarea.Console.Plugin.SysAdmin.Form
{
    partial class UsersBrowser
    {
        private Button BtnCancel;
        private System.Windows.Forms.Button BtnOK;
        private System.Windows.Forms.Button BtnHelp;
        private System.Windows.Forms.Panel panelBottom;

        //---------------------------------------------------------------------
        /// <summary>
        /// Required designer variable.
        /// </summary>
        private System.ComponentModel.Container components = null;

        private UsersBrowserControl usersBrowserControl;

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
            System.Resources.ResourceManager resources = new System.Resources.ResourceManager(typeof(UsersBrowser));
            this.BtnCancel = new System.Windows.Forms.Button();
            this.BtnOK = new System.Windows.Forms.Button();
            this.BtnHelp = new System.Windows.Forms.Button();
            this.panelBottom = new System.Windows.Forms.Panel();
            this.usersBrowserControl = new Microarea.TaskBuilderNet.UI.WinControls.Lists.UsersBrowserControl();
            this.panelBottom.SuspendLayout();
            this.SuspendLayout();
            // 
            // BtnCancel
            // 
            this.BtnCancel.AccessibleDescription = resources.GetString("BtnCancel.AccessibleDescription");
            this.BtnCancel.AccessibleName = resources.GetString("BtnCancel.AccessibleName");
            this.BtnCancel.Anchor = ((System.Windows.Forms.AnchorStyles)(resources.GetObject("BtnCancel.Anchor")));
            this.BtnCancel.BackgroundImage = ((System.Drawing.Image)(resources.GetObject("BtnCancel.BackgroundImage")));
            this.BtnCancel.DialogResult = System.Windows.Forms.DialogResult.Cancel;
            this.BtnCancel.Dock = ((System.Windows.Forms.DockStyle)(resources.GetObject("BtnCancel.Dock")));
            this.BtnCancel.Enabled = ((bool)(resources.GetObject("BtnCancel.Enabled")));
            this.BtnCancel.FlatStyle = ((System.Windows.Forms.FlatStyle)(resources.GetObject("BtnCancel.FlatStyle")));
            this.BtnCancel.Font = ((System.Drawing.Font)(resources.GetObject("BtnCancel.Font")));
            this.BtnCancel.Image = ((System.Drawing.Image)(resources.GetObject("BtnCancel.Image")));
            this.BtnCancel.ImageAlign = ((System.Drawing.ContentAlignment)(resources.GetObject("BtnCancel.ImageAlign")));
            this.BtnCancel.ImageIndex = ((int)(resources.GetObject("BtnCancel.ImageIndex")));
            this.BtnCancel.ImeMode = ((System.Windows.Forms.ImeMode)(resources.GetObject("BtnCancel.ImeMode")));
            this.BtnCancel.Location = ((System.Drawing.Point)(resources.GetObject("BtnCancel.Location")));
            this.BtnCancel.Name = "BtnCancel";
            this.BtnCancel.RightToLeft = ((System.Windows.Forms.RightToLeft)(resources.GetObject("BtnCancel.RightToLeft")));
            this.BtnCancel.Size = ((System.Drawing.Size)(resources.GetObject("BtnCancel.Size")));
            this.BtnCancel.TabIndex = ((int)(resources.GetObject("BtnCancel.TabIndex")));
            this.BtnCancel.Text = resources.GetString("BtnCancel.Text");
            this.BtnCancel.TextAlign = ((System.Drawing.ContentAlignment)(resources.GetObject("BtnCancel.TextAlign")));
            this.BtnCancel.Visible = ((bool)(resources.GetObject("BtnCancel.Visible")));
            // 
            // BtnOK
            // 
            this.BtnOK.AccessibleDescription = resources.GetString("BtnOK.AccessibleDescription");
            this.BtnOK.AccessibleName = resources.GetString("BtnOK.AccessibleName");
            this.BtnOK.Anchor = ((System.Windows.Forms.AnchorStyles)(resources.GetObject("BtnOK.Anchor")));
            this.BtnOK.BackgroundImage = ((System.Drawing.Image)(resources.GetObject("BtnOK.BackgroundImage")));
            this.BtnOK.Dock = ((System.Windows.Forms.DockStyle)(resources.GetObject("BtnOK.Dock")));
            this.BtnOK.Enabled = ((bool)(resources.GetObject("BtnOK.Enabled")));
            this.BtnOK.FlatStyle = ((System.Windows.Forms.FlatStyle)(resources.GetObject("BtnOK.FlatStyle")));
            this.BtnOK.Font = ((System.Drawing.Font)(resources.GetObject("BtnOK.Font")));
            this.BtnOK.Image = ((System.Drawing.Image)(resources.GetObject("BtnOK.Image")));
            this.BtnOK.ImageAlign = ((System.Drawing.ContentAlignment)(resources.GetObject("BtnOK.ImageAlign")));
            this.BtnOK.ImageIndex = ((int)(resources.GetObject("BtnOK.ImageIndex")));
            this.BtnOK.ImeMode = ((System.Windows.Forms.ImeMode)(resources.GetObject("BtnOK.ImeMode")));
            this.BtnOK.Location = ((System.Drawing.Point)(resources.GetObject("BtnOK.Location")));
            this.BtnOK.Name = "BtnOK";
            this.BtnOK.RightToLeft = ((System.Windows.Forms.RightToLeft)(resources.GetObject("BtnOK.RightToLeft")));
            this.BtnOK.Size = ((System.Drawing.Size)(resources.GetObject("BtnOK.Size")));
            this.BtnOK.TabIndex = ((int)(resources.GetObject("BtnOK.TabIndex")));
            this.BtnOK.Text = resources.GetString("BtnOK.Text");
            this.BtnOK.TextAlign = ((System.Drawing.ContentAlignment)(resources.GetObject("BtnOK.TextAlign")));
            this.BtnOK.Visible = ((bool)(resources.GetObject("BtnOK.Visible")));
            this.BtnOK.Click += new System.EventHandler(this.BtnOK_Click);
            // 
            // BtnHelp
            // 
            this.BtnHelp.AccessibleDescription = resources.GetString("BtnHelp.AccessibleDescription");
            this.BtnHelp.AccessibleName = resources.GetString("BtnHelp.AccessibleName");
            this.BtnHelp.Anchor = ((System.Windows.Forms.AnchorStyles)(resources.GetObject("BtnHelp.Anchor")));
            this.BtnHelp.BackgroundImage = ((System.Drawing.Image)(resources.GetObject("BtnHelp.BackgroundImage")));
            this.BtnHelp.Dock = ((System.Windows.Forms.DockStyle)(resources.GetObject("BtnHelp.Dock")));
            this.BtnHelp.Enabled = ((bool)(resources.GetObject("BtnHelp.Enabled")));
            this.BtnHelp.FlatStyle = ((System.Windows.Forms.FlatStyle)(resources.GetObject("BtnHelp.FlatStyle")));
            this.BtnHelp.Font = ((System.Drawing.Font)(resources.GetObject("BtnHelp.Font")));
            this.BtnHelp.Image = ((System.Drawing.Image)(resources.GetObject("BtnHelp.Image")));
            this.BtnHelp.ImageAlign = ((System.Drawing.ContentAlignment)(resources.GetObject("BtnHelp.ImageAlign")));
            this.BtnHelp.ImageIndex = ((int)(resources.GetObject("BtnHelp.ImageIndex")));
            this.BtnHelp.ImeMode = ((System.Windows.Forms.ImeMode)(resources.GetObject("BtnHelp.ImeMode")));
            this.BtnHelp.Location = ((System.Drawing.Point)(resources.GetObject("BtnHelp.Location")));
            this.BtnHelp.Name = "BtnHelp";
            this.BtnHelp.RightToLeft = ((System.Windows.Forms.RightToLeft)(resources.GetObject("BtnHelp.RightToLeft")));
            this.BtnHelp.Size = ((System.Drawing.Size)(resources.GetObject("BtnHelp.Size")));
            this.BtnHelp.TabIndex = ((int)(resources.GetObject("BtnHelp.TabIndex")));
            this.BtnHelp.Text = resources.GetString("BtnHelp.Text");
            this.BtnHelp.TextAlign = ((System.Drawing.ContentAlignment)(resources.GetObject("BtnHelp.TextAlign")));
            this.BtnHelp.Visible = ((bool)(resources.GetObject("BtnHelp.Visible")));
            // 
            // panelBottom
            // 
            this.panelBottom.AccessibleDescription = resources.GetString("panelBottom.AccessibleDescription");
            this.panelBottom.AccessibleName = resources.GetString("panelBottom.AccessibleName");
            this.panelBottom.Anchor = ((System.Windows.Forms.AnchorStyles)(resources.GetObject("panelBottom.Anchor")));
            this.panelBottom.AutoScroll = ((bool)(resources.GetObject("panelBottom.AutoScroll")));
            this.panelBottom.AutoScrollMargin = ((System.Drawing.Size)(resources.GetObject("panelBottom.AutoScrollMargin")));
            this.panelBottom.AutoScrollMinSize = ((System.Drawing.Size)(resources.GetObject("panelBottom.AutoScrollMinSize")));
            this.panelBottom.BackgroundImage = ((System.Drawing.Image)(resources.GetObject("panelBottom.BackgroundImage")));
            this.panelBottom.Controls.Add(this.BtnHelp);
            this.panelBottom.Controls.Add(this.BtnCancel);
            this.panelBottom.Controls.Add(this.BtnOK);
            this.panelBottom.Dock = ((System.Windows.Forms.DockStyle)(resources.GetObject("panelBottom.Dock")));
            this.panelBottom.Enabled = ((bool)(resources.GetObject("panelBottom.Enabled")));
            this.panelBottom.Font = ((System.Drawing.Font)(resources.GetObject("panelBottom.Font")));
            this.panelBottom.ImeMode = ((System.Windows.Forms.ImeMode)(resources.GetObject("panelBottom.ImeMode")));
            this.panelBottom.Location = ((System.Drawing.Point)(resources.GetObject("panelBottom.Location")));
            this.panelBottom.Name = "panelBottom";
            this.panelBottom.RightToLeft = ((System.Windows.Forms.RightToLeft)(resources.GetObject("panelBottom.RightToLeft")));
            this.panelBottom.Size = ((System.Drawing.Size)(resources.GetObject("panelBottom.Size")));
            this.panelBottom.TabIndex = ((int)(resources.GetObject("panelBottom.TabIndex")));
            this.panelBottom.Text = resources.GetString("panelBottom.Text");
            this.panelBottom.Visible = ((bool)(resources.GetObject("panelBottom.Visible")));
            // 
            // usersBrowserControl
            // 
            this.usersBrowserControl.AccessibleDescription = resources.GetString("usersBrowserControl.AccessibleDescription");
            this.usersBrowserControl.AccessibleName = resources.GetString("usersBrowserControl.AccessibleName");
            this.usersBrowserControl.Anchor = ((System.Windows.Forms.AnchorStyles)(resources.GetObject("usersBrowserControl.Anchor")));
            this.usersBrowserControl.AutoScroll = ((bool)(resources.GetObject("usersBrowserControl.AutoScroll")));
            this.usersBrowserControl.AutoScrollMargin = ((System.Drawing.Size)(resources.GetObject("usersBrowserControl.AutoScrollMargin")));
            this.usersBrowserControl.AutoScrollMinSize = ((System.Drawing.Size)(resources.GetObject("usersBrowserControl.AutoScrollMinSize")));
            this.usersBrowserControl.BackgroundImage = ((System.Drawing.Image)(resources.GetObject("usersBrowserControl.BackgroundImage")));
            this.usersBrowserControl.ComputerName = "USR-GIUSTINADI1";
            this.usersBrowserControl.Dock = ((System.Windows.Forms.DockStyle)(resources.GetObject("usersBrowserControl.Dock")));
            this.usersBrowserControl.DomainName = "MICROAREA";
            this.usersBrowserControl.Enabled = ((bool)(resources.GetObject("usersBrowserControl.Enabled")));
            this.usersBrowserControl.Font = ((System.Drawing.Font)(resources.GetObject("usersBrowserControl.Font")));
            this.usersBrowserControl.ImeMode = ((System.Windows.Forms.ImeMode)(resources.GetObject("usersBrowserControl.ImeMode")));
            this.usersBrowserControl.Location = ((System.Drawing.Point)(resources.GetObject("usersBrowserControl.Location")));
            this.usersBrowserControl.Name = "usersBrowserControl";
            this.usersBrowserControl.RightToLeft = ((System.Windows.Forms.RightToLeft)(resources.GetObject("usersBrowserControl.RightToLeft")));
            this.usersBrowserControl.Size = ((System.Drawing.Size)(resources.GetObject("usersBrowserControl.Size")));
            this.usersBrowserControl.TabIndex = ((int)(resources.GetObject("usersBrowserControl.TabIndex")));
            this.usersBrowserControl.Visible = ((bool)(resources.GetObject("usersBrowserControl.Visible")));
            // 
            // UsersBrowser
            // 
            this.AccessibleDescription = resources.GetString("$this.AccessibleDescription");
            this.AccessibleName = resources.GetString("$this.AccessibleName");
            this.AutoScaleBaseSize = ((System.Drawing.Size)(resources.GetObject("$this.AutoScaleBaseSize")));
            this.AutoScroll = ((bool)(resources.GetObject("$this.AutoScroll")));
            this.AutoScrollMargin = ((System.Drawing.Size)(resources.GetObject("$this.AutoScrollMargin")));
            this.AutoScrollMinSize = ((System.Drawing.Size)(resources.GetObject("$this.AutoScrollMinSize")));
            this.BackgroundImage = ((System.Drawing.Image)(resources.GetObject("$this.BackgroundImage")));
            this.ClientSize = ((System.Drawing.Size)(resources.GetObject("$this.ClientSize")));
            this.Controls.Add(this.usersBrowserControl);
            this.Controls.Add(this.panelBottom);
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
            this.Name = "UsersBrowser";
            this.RightToLeft = ((System.Windows.Forms.RightToLeft)(resources.GetObject("$this.RightToLeft")));
            this.ShowInTaskbar = false;
            this.StartPosition = ((System.Windows.Forms.FormStartPosition)(resources.GetObject("$this.StartPosition")));
            this.Text = resources.GetString("$this.Text");
            this.panelBottom.ResumeLayout(false);
            this.ResumeLayout(false);

        }
        #endregion

    }
}
