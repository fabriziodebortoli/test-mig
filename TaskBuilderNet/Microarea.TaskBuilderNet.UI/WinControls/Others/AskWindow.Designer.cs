
namespace Microarea.TaskBuilderNet.UI.WinControls
{
    partial class AskWindow
    {
        private System.Windows.Forms.Button btnOk;
        private System.Windows.Forms.Button btnYes;
        private System.Windows.Forms.Button btnNo;
        private System.Windows.Forms.Button btnCancel;
        private System.Windows.Forms.CheckBox ckbDoNotAsk;
        /// <summary>
        /// Required designer variable.
        /// </summary>
        private System.ComponentModel.Container components = null;
        private System.Windows.Forms.Label txtQuestion;

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
            System.Resources.ResourceManager resources = new System.Resources.ResourceManager(typeof(AskWindow));
            this.btnYes = new System.Windows.Forms.Button();
            this.btnNo = new System.Windows.Forms.Button();
            this.btnCancel = new System.Windows.Forms.Button();
            this.ckbDoNotAsk = new System.Windows.Forms.CheckBox();
            this.txtQuestion = new System.Windows.Forms.Label();
            this.btnOk = new System.Windows.Forms.Button();
            this.SuspendLayout();
            // 
            // btnYes
            // 
            this.btnYes.AccessibleDescription = resources.GetString("btnYes.AccessibleDescription");
            this.btnYes.AccessibleName = resources.GetString("btnYes.AccessibleName");
            this.btnYes.Anchor = ((System.Windows.Forms.AnchorStyles)(resources.GetObject("btnYes.Anchor")));
            this.btnYes.BackgroundImage = ((System.Drawing.Image)(resources.GetObject("btnYes.BackgroundImage")));
            this.btnYes.DialogResult = System.Windows.Forms.DialogResult.Yes;
            this.btnYes.Dock = ((System.Windows.Forms.DockStyle)(resources.GetObject("btnYes.Dock")));
            this.btnYes.Enabled = ((bool)(resources.GetObject("btnYes.Enabled")));
            this.btnYes.FlatStyle = ((System.Windows.Forms.FlatStyle)(resources.GetObject("btnYes.FlatStyle")));
            this.btnYes.Font = ((System.Drawing.Font)(resources.GetObject("btnYes.Font")));
            this.btnYes.Image = ((System.Drawing.Image)(resources.GetObject("btnYes.Image")));
            this.btnYes.ImageAlign = ((System.Drawing.ContentAlignment)(resources.GetObject("btnYes.ImageAlign")));
            this.btnYes.ImageIndex = ((int)(resources.GetObject("btnYes.ImageIndex")));
            this.btnYes.ImeMode = ((System.Windows.Forms.ImeMode)(resources.GetObject("btnYes.ImeMode")));
            this.btnYes.Location = ((System.Drawing.Point)(resources.GetObject("btnYes.Location")));
            this.btnYes.Name = "btnYes";
            this.btnYes.RightToLeft = ((System.Windows.Forms.RightToLeft)(resources.GetObject("btnYes.RightToLeft")));
            this.btnYes.Size = ((System.Drawing.Size)(resources.GetObject("btnYes.Size")));
            this.btnYes.TabIndex = ((int)(resources.GetObject("btnYes.TabIndex")));
            this.btnYes.Text = resources.GetString("btnYes.Text");
            this.btnYes.TextAlign = ((System.Drawing.ContentAlignment)(resources.GetObject("btnYes.TextAlign")));
            this.btnYes.Visible = ((bool)(resources.GetObject("btnYes.Visible")));
            // 
            // btnNo
            // 
            this.btnNo.AccessibleDescription = resources.GetString("btnNo.AccessibleDescription");
            this.btnNo.AccessibleName = resources.GetString("btnNo.AccessibleName");
            this.btnNo.Anchor = ((System.Windows.Forms.AnchorStyles)(resources.GetObject("btnNo.Anchor")));
            this.btnNo.BackgroundImage = ((System.Drawing.Image)(resources.GetObject("btnNo.BackgroundImage")));
            this.btnNo.DialogResult = System.Windows.Forms.DialogResult.No;
            this.btnNo.Dock = ((System.Windows.Forms.DockStyle)(resources.GetObject("btnNo.Dock")));
            this.btnNo.Enabled = ((bool)(resources.GetObject("btnNo.Enabled")));
            this.btnNo.FlatStyle = ((System.Windows.Forms.FlatStyle)(resources.GetObject("btnNo.FlatStyle")));
            this.btnNo.Font = ((System.Drawing.Font)(resources.GetObject("btnNo.Font")));
            this.btnNo.Image = ((System.Drawing.Image)(resources.GetObject("btnNo.Image")));
            this.btnNo.ImageAlign = ((System.Drawing.ContentAlignment)(resources.GetObject("btnNo.ImageAlign")));
            this.btnNo.ImageIndex = ((int)(resources.GetObject("btnNo.ImageIndex")));
            this.btnNo.ImeMode = ((System.Windows.Forms.ImeMode)(resources.GetObject("btnNo.ImeMode")));
            this.btnNo.Location = ((System.Drawing.Point)(resources.GetObject("btnNo.Location")));
            this.btnNo.Name = "btnNo";
            this.btnNo.RightToLeft = ((System.Windows.Forms.RightToLeft)(resources.GetObject("btnNo.RightToLeft")));
            this.btnNo.Size = ((System.Drawing.Size)(resources.GetObject("btnNo.Size")));
            this.btnNo.TabIndex = ((int)(resources.GetObject("btnNo.TabIndex")));
            this.btnNo.Text = resources.GetString("btnNo.Text");
            this.btnNo.TextAlign = ((System.Drawing.ContentAlignment)(resources.GetObject("btnNo.TextAlign")));
            this.btnNo.Visible = ((bool)(resources.GetObject("btnNo.Visible")));
            // 
            // btnCancel
            // 
            this.btnCancel.AccessibleDescription = resources.GetString("btnCancel.AccessibleDescription");
            this.btnCancel.AccessibleName = resources.GetString("btnCancel.AccessibleName");
            this.btnCancel.Anchor = ((System.Windows.Forms.AnchorStyles)(resources.GetObject("btnCancel.Anchor")));
            this.btnCancel.BackgroundImage = ((System.Drawing.Image)(resources.GetObject("btnCancel.BackgroundImage")));
            this.btnCancel.DialogResult = System.Windows.Forms.DialogResult.Cancel;
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
            // 
            // ckbDoNotAsk
            // 
            this.ckbDoNotAsk.AccessibleDescription = resources.GetString("ckbDoNotAsk.AccessibleDescription");
            this.ckbDoNotAsk.AccessibleName = resources.GetString("ckbDoNotAsk.AccessibleName");
            this.ckbDoNotAsk.Anchor = ((System.Windows.Forms.AnchorStyles)(resources.GetObject("ckbDoNotAsk.Anchor")));
            this.ckbDoNotAsk.Appearance = ((System.Windows.Forms.Appearance)(resources.GetObject("ckbDoNotAsk.Appearance")));
            this.ckbDoNotAsk.BackgroundImage = ((System.Drawing.Image)(resources.GetObject("ckbDoNotAsk.BackgroundImage")));
            this.ckbDoNotAsk.CheckAlign = ((System.Drawing.ContentAlignment)(resources.GetObject("ckbDoNotAsk.CheckAlign")));
            this.ckbDoNotAsk.Dock = ((System.Windows.Forms.DockStyle)(resources.GetObject("ckbDoNotAsk.Dock")));
            this.ckbDoNotAsk.Enabled = ((bool)(resources.GetObject("ckbDoNotAsk.Enabled")));
            this.ckbDoNotAsk.FlatStyle = ((System.Windows.Forms.FlatStyle)(resources.GetObject("ckbDoNotAsk.FlatStyle")));
            this.ckbDoNotAsk.Font = ((System.Drawing.Font)(resources.GetObject("ckbDoNotAsk.Font")));
            this.ckbDoNotAsk.Image = ((System.Drawing.Image)(resources.GetObject("ckbDoNotAsk.Image")));
            this.ckbDoNotAsk.ImageAlign = ((System.Drawing.ContentAlignment)(resources.GetObject("ckbDoNotAsk.ImageAlign")));
            this.ckbDoNotAsk.ImageIndex = ((int)(resources.GetObject("ckbDoNotAsk.ImageIndex")));
            this.ckbDoNotAsk.ImeMode = ((System.Windows.Forms.ImeMode)(resources.GetObject("ckbDoNotAsk.ImeMode")));
            this.ckbDoNotAsk.Location = ((System.Drawing.Point)(resources.GetObject("ckbDoNotAsk.Location")));
            this.ckbDoNotAsk.Name = "ckbDoNotAsk";
            this.ckbDoNotAsk.RightToLeft = ((System.Windows.Forms.RightToLeft)(resources.GetObject("ckbDoNotAsk.RightToLeft")));
            this.ckbDoNotAsk.Size = ((System.Drawing.Size)(resources.GetObject("ckbDoNotAsk.Size")));
            this.ckbDoNotAsk.TabIndex = ((int)(resources.GetObject("ckbDoNotAsk.TabIndex")));
            this.ckbDoNotAsk.Text = resources.GetString("ckbDoNotAsk.Text");
            this.ckbDoNotAsk.TextAlign = ((System.Drawing.ContentAlignment)(resources.GetObject("ckbDoNotAsk.TextAlign")));
            this.ckbDoNotAsk.Visible = ((bool)(resources.GetObject("ckbDoNotAsk.Visible")));
            // 
            // txtQuestion
            // 
            this.txtQuestion.AccessibleDescription = resources.GetString("txtQuestion.AccessibleDescription");
            this.txtQuestion.AccessibleName = resources.GetString("txtQuestion.AccessibleName");
            this.txtQuestion.Anchor = ((System.Windows.Forms.AnchorStyles)(resources.GetObject("txtQuestion.Anchor")));
            this.txtQuestion.AutoSize = ((bool)(resources.GetObject("txtQuestion.AutoSize")));
            this.txtQuestion.Dock = ((System.Windows.Forms.DockStyle)(resources.GetObject("txtQuestion.Dock")));
            this.txtQuestion.Enabled = ((bool)(resources.GetObject("txtQuestion.Enabled")));
            this.txtQuestion.Font = ((System.Drawing.Font)(resources.GetObject("txtQuestion.Font")));
            this.txtQuestion.Image = ((System.Drawing.Image)(resources.GetObject("txtQuestion.Image")));
            this.txtQuestion.ImageAlign = ((System.Drawing.ContentAlignment)(resources.GetObject("txtQuestion.ImageAlign")));
            this.txtQuestion.ImageIndex = ((int)(resources.GetObject("txtQuestion.ImageIndex")));
            this.txtQuestion.ImeMode = ((System.Windows.Forms.ImeMode)(resources.GetObject("txtQuestion.ImeMode")));
            this.txtQuestion.Location = ((System.Drawing.Point)(resources.GetObject("txtQuestion.Location")));
            this.txtQuestion.Name = "txtQuestion";
            this.txtQuestion.RightToLeft = ((System.Windows.Forms.RightToLeft)(resources.GetObject("txtQuestion.RightToLeft")));
            this.txtQuestion.Size = ((System.Drawing.Size)(resources.GetObject("txtQuestion.Size")));
            this.txtQuestion.TabIndex = ((int)(resources.GetObject("txtQuestion.TabIndex")));
            this.txtQuestion.Text = resources.GetString("txtQuestion.Text");
            this.txtQuestion.TextAlign = ((System.Drawing.ContentAlignment)(resources.GetObject("txtQuestion.TextAlign")));
            this.txtQuestion.Visible = ((bool)(resources.GetObject("txtQuestion.Visible")));
            // 
            // btnOk
            // 
            this.btnOk.AccessibleDescription = resources.GetString("btnOk.AccessibleDescription");
            this.btnOk.AccessibleName = resources.GetString("btnOk.AccessibleName");
            this.btnOk.Anchor = ((System.Windows.Forms.AnchorStyles)(resources.GetObject("btnOk.Anchor")));
            this.btnOk.BackgroundImage = ((System.Drawing.Image)(resources.GetObject("btnOk.BackgroundImage")));
            this.btnOk.DialogResult = System.Windows.Forms.DialogResult.OK;
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
            // 
            // AskWindow
            // 
            this.AcceptButton = this.btnYes;
            this.AccessibleDescription = resources.GetString("$this.AccessibleDescription");
            this.AccessibleName = resources.GetString("$this.AccessibleName");
            this.AutoScaleBaseSize = ((System.Drawing.Size)(resources.GetObject("$this.AutoScaleBaseSize")));
            this.AutoScroll = ((bool)(resources.GetObject("$this.AutoScroll")));
            this.AutoScrollMargin = ((System.Drawing.Size)(resources.GetObject("$this.AutoScrollMargin")));
            this.AutoScrollMinSize = ((System.Drawing.Size)(resources.GetObject("$this.AutoScrollMinSize")));
            this.BackgroundImage = ((System.Drawing.Image)(resources.GetObject("$this.BackgroundImage")));
            this.CancelButton = this.btnCancel;
            this.ClientSize = ((System.Drawing.Size)(resources.GetObject("$this.ClientSize")));
            this.Controls.Add(this.btnOk);
            this.Controls.Add(this.txtQuestion);
            this.Controls.Add(this.ckbDoNotAsk);
            this.Controls.Add(this.btnYes);
            this.Controls.Add(this.btnNo);
            this.Controls.Add(this.btnCancel);
            this.Enabled = ((bool)(resources.GetObject("$this.Enabled")));
            this.Font = ((System.Drawing.Font)(resources.GetObject("$this.Font")));
            this.Icon = ((System.Drawing.Icon)(resources.GetObject("$this.Icon")));
            this.ImeMode = ((System.Windows.Forms.ImeMode)(resources.GetObject("$this.ImeMode")));
            this.Location = ((System.Drawing.Point)(resources.GetObject("$this.Location")));
            this.MaximumSize = ((System.Drawing.Size)(resources.GetObject("$this.MaximumSize")));
            this.MinimumSize = ((System.Drawing.Size)(resources.GetObject("$this.MinimumSize")));
            this.Name = "AskWindow";
            this.RightToLeft = ((System.Windows.Forms.RightToLeft)(resources.GetObject("$this.RightToLeft")));
            this.StartPosition = ((System.Windows.Forms.FormStartPosition)(resources.GetObject("$this.StartPosition")));
            this.Text = resources.GetString("$this.Text");
            this.ResumeLayout(false);

        }
        #endregion
    }
}
