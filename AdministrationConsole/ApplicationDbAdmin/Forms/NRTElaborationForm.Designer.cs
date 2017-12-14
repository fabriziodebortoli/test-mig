
namespace Microarea.Console.Plugin.ApplicationDBAdmin.Forms
{
    partial class NRTElaborationForm
    {
        private System.Windows.Forms.CheckBox CHKByStep;
        private System.Windows.Forms.RichTextBox ENTLog;
        private System.Windows.Forms.Button CMDExit;
        private System.ComponentModel.IContainer components = null;

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
            System.Resources.ResourceManager resources = new System.Resources.ResourceManager(typeof(NRTElaborationForm));
            this.ENTLog = new System.Windows.Forms.RichTextBox();
            this.CMDExit = new System.Windows.Forms.Button();
            this.CHKByStep = new System.Windows.Forms.CheckBox();
            this.SuspendLayout();
            // 
            // ENTLog
            // 
            this.ENTLog.AccessibleDescription = resources.GetString("ENTLog.AccessibleDescription");
            this.ENTLog.AccessibleName = resources.GetString("ENTLog.AccessibleName");
            this.ENTLog.Anchor = ((System.Windows.Forms.AnchorStyles)(resources.GetObject("ENTLog.Anchor")));
            this.ENTLog.AutoSize = ((bool)(resources.GetObject("ENTLog.AutoSize")));
            this.ENTLog.BackgroundImage = ((System.Drawing.Image)(resources.GetObject("ENTLog.BackgroundImage")));
            this.ENTLog.BulletIndent = ((int)(resources.GetObject("ENTLog.BulletIndent")));
            this.ENTLog.Dock = ((System.Windows.Forms.DockStyle)(resources.GetObject("ENTLog.Dock")));
            this.ENTLog.Enabled = ((bool)(resources.GetObject("ENTLog.Enabled")));
            this.ENTLog.Font = ((System.Drawing.Font)(resources.GetObject("ENTLog.Font")));
            this.ENTLog.ImeMode = ((System.Windows.Forms.ImeMode)(resources.GetObject("ENTLog.ImeMode")));
            this.ENTLog.Location = ((System.Drawing.Point)(resources.GetObject("ENTLog.Location")));
            this.ENTLog.MaxLength = ((int)(resources.GetObject("ENTLog.MaxLength")));
            this.ENTLog.Multiline = ((bool)(resources.GetObject("ENTLog.Multiline")));
            this.ENTLog.Name = "ENTLog";
            this.ENTLog.RightMargin = ((int)(resources.GetObject("ENTLog.RightMargin")));
            this.ENTLog.RightToLeft = ((System.Windows.Forms.RightToLeft)(resources.GetObject("ENTLog.RightToLeft")));
            this.ENTLog.ScrollBars = ((System.Windows.Forms.RichTextBoxScrollBars)(resources.GetObject("ENTLog.ScrollBars")));
            this.ENTLog.Size = ((System.Drawing.Size)(resources.GetObject("ENTLog.Size")));
            this.ENTLog.TabIndex = ((int)(resources.GetObject("ENTLog.TabIndex")));
            this.ENTLog.Text = resources.GetString("ENTLog.Text");
            this.ENTLog.Visible = ((bool)(resources.GetObject("ENTLog.Visible")));
            this.ENTLog.WordWrap = ((bool)(resources.GetObject("ENTLog.WordWrap")));
            this.ENTLog.ZoomFactor = ((System.Single)(resources.GetObject("ENTLog.ZoomFactor")));
            this.ENTLog.KeyPress += new System.Windows.Forms.KeyPressEventHandler(this.ENTLog_KeyPress);
            this.ENTLog.TextChanged += new System.EventHandler(this.ENTLog_TextChanged);
            // 
            // CMDExit
            // 
            this.CMDExit.AccessibleDescription = resources.GetString("CMDExit.AccessibleDescription");
            this.CMDExit.AccessibleName = resources.GetString("CMDExit.AccessibleName");
            this.CMDExit.Anchor = ((System.Windows.Forms.AnchorStyles)(resources.GetObject("CMDExit.Anchor")));
            this.CMDExit.BackgroundImage = ((System.Drawing.Image)(resources.GetObject("CMDExit.BackgroundImage")));
            this.CMDExit.Dock = ((System.Windows.Forms.DockStyle)(resources.GetObject("CMDExit.Dock")));
            this.CMDExit.Enabled = ((bool)(resources.GetObject("CMDExit.Enabled")));
            this.CMDExit.FlatStyle = ((System.Windows.Forms.FlatStyle)(resources.GetObject("CMDExit.FlatStyle")));
            this.CMDExit.Font = ((System.Drawing.Font)(resources.GetObject("CMDExit.Font")));
            this.CMDExit.Image = ((System.Drawing.Image)(resources.GetObject("CMDExit.Image")));
            this.CMDExit.ImageAlign = ((System.Drawing.ContentAlignment)(resources.GetObject("CMDExit.ImageAlign")));
            this.CMDExit.ImageIndex = ((int)(resources.GetObject("CMDExit.ImageIndex")));
            this.CMDExit.ImeMode = ((System.Windows.Forms.ImeMode)(resources.GetObject("CMDExit.ImeMode")));
            this.CMDExit.Location = ((System.Drawing.Point)(resources.GetObject("CMDExit.Location")));
            this.CMDExit.Name = "CMDExit";
            this.CMDExit.RightToLeft = ((System.Windows.Forms.RightToLeft)(resources.GetObject("CMDExit.RightToLeft")));
            this.CMDExit.Size = ((System.Drawing.Size)(resources.GetObject("CMDExit.Size")));
            this.CMDExit.TabIndex = ((int)(resources.GetObject("CMDExit.TabIndex")));
            this.CMDExit.Text = resources.GetString("CMDExit.Text");
            this.CMDExit.TextAlign = ((System.Drawing.ContentAlignment)(resources.GetObject("CMDExit.TextAlign")));
            this.CMDExit.Visible = ((bool)(resources.GetObject("CMDExit.Visible")));
            this.CMDExit.Click += new System.EventHandler(this.CMDExit_Click);
            // 
            // CHKByStep
            // 
            this.CHKByStep.AccessibleDescription = resources.GetString("CHKByStep.AccessibleDescription");
            this.CHKByStep.AccessibleName = resources.GetString("CHKByStep.AccessibleName");
            this.CHKByStep.Anchor = ((System.Windows.Forms.AnchorStyles)(resources.GetObject("CHKByStep.Anchor")));
            this.CHKByStep.Appearance = ((System.Windows.Forms.Appearance)(resources.GetObject("CHKByStep.Appearance")));
            this.CHKByStep.BackgroundImage = ((System.Drawing.Image)(resources.GetObject("CHKByStep.BackgroundImage")));
            this.CHKByStep.CheckAlign = ((System.Drawing.ContentAlignment)(resources.GetObject("CHKByStep.CheckAlign")));
            this.CHKByStep.Dock = ((System.Windows.Forms.DockStyle)(resources.GetObject("CHKByStep.Dock")));
            this.CHKByStep.Enabled = ((bool)(resources.GetObject("CHKByStep.Enabled")));
            this.CHKByStep.FlatStyle = ((System.Windows.Forms.FlatStyle)(resources.GetObject("CHKByStep.FlatStyle")));
            this.CHKByStep.Font = ((System.Drawing.Font)(resources.GetObject("CHKByStep.Font")));
            this.CHKByStep.Image = ((System.Drawing.Image)(resources.GetObject("CHKByStep.Image")));
            this.CHKByStep.ImageAlign = ((System.Drawing.ContentAlignment)(resources.GetObject("CHKByStep.ImageAlign")));
            this.CHKByStep.ImageIndex = ((int)(resources.GetObject("CHKByStep.ImageIndex")));
            this.CHKByStep.ImeMode = ((System.Windows.Forms.ImeMode)(resources.GetObject("CHKByStep.ImeMode")));
            this.CHKByStep.Location = ((System.Drawing.Point)(resources.GetObject("CHKByStep.Location")));
            this.CHKByStep.Name = "CHKByStep";
            this.CHKByStep.RightToLeft = ((System.Windows.Forms.RightToLeft)(resources.GetObject("CHKByStep.RightToLeft")));
            this.CHKByStep.Size = ((System.Drawing.Size)(resources.GetObject("CHKByStep.Size")));
            this.CHKByStep.TabIndex = ((int)(resources.GetObject("CHKByStep.TabIndex")));
            this.CHKByStep.Text = resources.GetString("CHKByStep.Text");
            this.CHKByStep.TextAlign = ((System.Drawing.ContentAlignment)(resources.GetObject("CHKByStep.TextAlign")));
            this.CHKByStep.Visible = ((bool)(resources.GetObject("CHKByStep.Visible")));
            this.CHKByStep.CheckedChanged += new System.EventHandler(this.CHKByStep_CheckedChanged);
            // 
            // NRTElaborationForm
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
            this.Controls.Add(this.CHKByStep);
            this.Controls.Add(this.CMDExit);
            this.Controls.Add(this.ENTLog);
            this.Enabled = ((bool)(resources.GetObject("$this.Enabled")));
            this.Font = ((System.Drawing.Font)(resources.GetObject("$this.Font")));
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedDialog;
            this.Icon = ((System.Drawing.Icon)(resources.GetObject("$this.Icon")));
            this.ImeMode = ((System.Windows.Forms.ImeMode)(resources.GetObject("$this.ImeMode")));
            this.Location = ((System.Drawing.Point)(resources.GetObject("$this.Location")));
            this.MaximumSize = ((System.Drawing.Size)(resources.GetObject("$this.MaximumSize")));
            this.MinimumSize = ((System.Drawing.Size)(resources.GetObject("$this.MinimumSize")));
            this.Name = "NRTElaborationForm";
            this.RightToLeft = ((System.Windows.Forms.RightToLeft)(resources.GetObject("$this.RightToLeft")));
            this.StartPosition = ((System.Windows.Forms.FormStartPosition)(resources.GetObject("$this.StartPosition")));
            this.Text = resources.GetString("$this.Text");
            this.Load += new System.EventHandler(this.NRTElaborationForm_Load);
            this.ResumeLayout(false);

        }
        #endregion
    }
}
