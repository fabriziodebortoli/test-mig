using System.Windows.Forms;

namespace Microarea.Console.Plugin.ServicesAdmin
{
    partial class DeleteTraceRecords
    {
        /// <summary>
        /// Required designer variable.
        /// </summary>
        private System.ComponentModel.Container components = null;
        private Button BtnOk;
        private Button BtnCancel;
        private DateTimePicker selectedToDate;
        private Label LblExplication;

        /// <summary>
        /// Dispose
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
            System.Resources.ResourceManager resources = new System.Resources.ResourceManager(typeof(DeleteTraceRecords));
            this.BtnOk = new System.Windows.Forms.Button();
            this.BtnCancel = new System.Windows.Forms.Button();
            this.LblExplication = new System.Windows.Forms.Label();
            this.selectedToDate = new System.Windows.Forms.DateTimePicker();
            this.SuspendLayout();
            // 
            // BtnOk
            // 
            this.BtnOk.AccessibleDescription = resources.GetString("BtnOk.AccessibleDescription");
            this.BtnOk.AccessibleName = resources.GetString("BtnOk.AccessibleName");
            this.BtnOk.Anchor = ((System.Windows.Forms.AnchorStyles)(resources.GetObject("BtnOk.Anchor")));
            this.BtnOk.BackgroundImage = ((System.Drawing.Image)(resources.GetObject("BtnOk.BackgroundImage")));
            this.BtnOk.DialogResult = System.Windows.Forms.DialogResult.OK;
            this.BtnOk.Dock = ((System.Windows.Forms.DockStyle)(resources.GetObject("BtnOk.Dock")));
            this.BtnOk.Enabled = ((bool)(resources.GetObject("BtnOk.Enabled")));
            this.BtnOk.FlatStyle = ((System.Windows.Forms.FlatStyle)(resources.GetObject("BtnOk.FlatStyle")));
            this.BtnOk.Font = ((System.Drawing.Font)(resources.GetObject("BtnOk.Font")));
            this.BtnOk.Image = ((System.Drawing.Image)(resources.GetObject("BtnOk.Image")));
            this.BtnOk.ImageAlign = ((System.Drawing.ContentAlignment)(resources.GetObject("BtnOk.ImageAlign")));
            this.BtnOk.ImageIndex = ((int)(resources.GetObject("BtnOk.ImageIndex")));
            this.BtnOk.ImeMode = ((System.Windows.Forms.ImeMode)(resources.GetObject("BtnOk.ImeMode")));
            this.BtnOk.Location = ((System.Drawing.Point)(resources.GetObject("BtnOk.Location")));
            this.BtnOk.Name = "BtnOk";
            this.BtnOk.RightToLeft = ((System.Windows.Forms.RightToLeft)(resources.GetObject("BtnOk.RightToLeft")));
            this.BtnOk.Size = ((System.Drawing.Size)(resources.GetObject("BtnOk.Size")));
            this.BtnOk.TabIndex = ((int)(resources.GetObject("BtnOk.TabIndex")));
            this.BtnOk.Text = resources.GetString("BtnOk.Text");
            this.BtnOk.TextAlign = ((System.Drawing.ContentAlignment)(resources.GetObject("BtnOk.TextAlign")));
            this.BtnOk.Visible = ((bool)(resources.GetObject("BtnOk.Visible")));
            this.BtnOk.Click += new System.EventHandler(this.BtnOk_Click);
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
            this.BtnCancel.Click += new System.EventHandler(this.BtnCancel_Click);
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
            // selectedToDate
            // 
            this.selectedToDate.AccessibleDescription = resources.GetString("selectedToDate.AccessibleDescription");
            this.selectedToDate.AccessibleName = resources.GetString("selectedToDate.AccessibleName");
            this.selectedToDate.Anchor = ((System.Windows.Forms.AnchorStyles)(resources.GetObject("selectedToDate.Anchor")));
            this.selectedToDate.BackgroundImage = ((System.Drawing.Image)(resources.GetObject("selectedToDate.BackgroundImage")));
            this.selectedToDate.CalendarFont = ((System.Drawing.Font)(resources.GetObject("selectedToDate.CalendarFont")));
            this.selectedToDate.Dock = ((System.Windows.Forms.DockStyle)(resources.GetObject("selectedToDate.Dock")));
            this.selectedToDate.DropDownAlign = ((System.Windows.Forms.LeftRightAlignment)(resources.GetObject("selectedToDate.DropDownAlign")));
            this.selectedToDate.Enabled = ((bool)(resources.GetObject("selectedToDate.Enabled")));
            this.selectedToDate.Font = ((System.Drawing.Font)(resources.GetObject("selectedToDate.Font")));
            this.selectedToDate.Format = System.Windows.Forms.DateTimePickerFormat.Short;
            this.selectedToDate.ImeMode = ((System.Windows.Forms.ImeMode)(resources.GetObject("selectedToDate.ImeMode")));
            this.selectedToDate.Location = ((System.Drawing.Point)(resources.GetObject("selectedToDate.Location")));
            this.selectedToDate.Name = "selectedToDate";
            this.selectedToDate.RightToLeft = ((System.Windows.Forms.RightToLeft)(resources.GetObject("selectedToDate.RightToLeft")));
            this.selectedToDate.Size = ((System.Drawing.Size)(resources.GetObject("selectedToDate.Size")));
            this.selectedToDate.TabIndex = ((int)(resources.GetObject("selectedToDate.TabIndex")));
            this.selectedToDate.Visible = ((bool)(resources.GetObject("selectedToDate.Visible")));
            // 
            // DeleteTraceRecords
            // 
            this.AcceptButton = this.BtnOk;
            this.AccessibleDescription = resources.GetString("$this.AccessibleDescription");
            this.AccessibleName = resources.GetString("$this.AccessibleName");
            this.AutoScaleBaseSize = ((System.Drawing.Size)(resources.GetObject("$this.AutoScaleBaseSize")));
            this.AutoScroll = ((bool)(resources.GetObject("$this.AutoScroll")));
            this.AutoScrollMargin = ((System.Drawing.Size)(resources.GetObject("$this.AutoScrollMargin")));
            this.AutoScrollMinSize = ((System.Drawing.Size)(resources.GetObject("$this.AutoScrollMinSize")));
            this.BackgroundImage = ((System.Drawing.Image)(resources.GetObject("$this.BackgroundImage")));
            this.CancelButton = this.BtnCancel;
            this.ClientSize = ((System.Drawing.Size)(resources.GetObject("$this.ClientSize")));
            this.Controls.Add(this.selectedToDate);
            this.Controls.Add(this.LblExplication);
            this.Controls.Add(this.BtnCancel);
            this.Controls.Add(this.BtnOk);
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
            this.Name = "DeleteTraceRecords";
            this.RightToLeft = ((System.Windows.Forms.RightToLeft)(resources.GetObject("$this.RightToLeft")));
            this.ShowInTaskbar = false;
            this.StartPosition = ((System.Windows.Forms.FormStartPosition)(resources.GetObject("$this.StartPosition")));
            this.Text = resources.GetString("$this.Text");
            this.ResumeLayout(false);

        }
        #endregion
    }
}
