
namespace Microarea.Console.Core.RegressionTestLibrary.Forms
{
    partial class ProcessingForm
    {
        #region Private methods
        private System.ComponentModel.IContainer components = null;
        private System.Windows.Forms.GroupBox DBSingleGroupBox;
        private System.Windows.Forms.Label MissingSingleValue;
        private System.Windows.Forms.Label PastSingleValue;
        private System.Windows.Forms.Label EstimatedSingleValue;
        private System.Windows.Forms.Label EstimatedSingleLabel;
        private System.Windows.Forms.Label MissingSingleLabel;
        private System.Windows.Forms.Label PastSingleLabel;
        private System.Windows.Forms.Label MissingTotalValue;
        private System.Windows.Forms.Label PastTotalValue;
        private System.Windows.Forms.Label EstimatedTotalValue;
        private System.Windows.Forms.Label EstimatedTotalLabel;
        private System.Windows.Forms.Label MissingTotalLabel;
        private System.Windows.Forms.Label PastTotalLabel;
        private System.Windows.Forms.GroupBox DBTotalGroupBox;
        private System.Timers.Timer ProcessTimer = new System.Timers.Timer();
        #endregion

        # region Dispose
        /// <summary>
        /// Clean up any resources being used.
        /// </summary>
        //---------------------------------------------------------------------------
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
        # endregion

        #region Windows Form Designer generated code
        /// <summary>
        /// Required method for Designer support - do not modify
        /// the contents of this method with the code editor.
        /// </summary>
        private void InitializeComponent()
        {
            System.Resources.ResourceManager resources = new System.Resources.ResourceManager(typeof(ProcessingForm));
            this.DBSingleGroupBox = new System.Windows.Forms.GroupBox();
            this.MissingSingleValue = new System.Windows.Forms.Label();
            this.PastSingleValue = new System.Windows.Forms.Label();
            this.EstimatedSingleValue = new System.Windows.Forms.Label();
            this.EstimatedSingleLabel = new System.Windows.Forms.Label();
            this.MissingSingleLabel = new System.Windows.Forms.Label();
            this.PastSingleLabel = new System.Windows.Forms.Label();
            this.DBTotalGroupBox = new System.Windows.Forms.GroupBox();
            this.MissingTotalValue = new System.Windows.Forms.Label();
            this.PastTotalValue = new System.Windows.Forms.Label();
            this.EstimatedTotalValue = new System.Windows.Forms.Label();
            this.EstimatedTotalLabel = new System.Windows.Forms.Label();
            this.MissingTotalLabel = new System.Windows.Forms.Label();
            this.PastTotalLabel = new System.Windows.Forms.Label();
            this.DBSingleGroupBox.SuspendLayout();
            this.DBTotalGroupBox.SuspendLayout();
            this.SuspendLayout();
            // 
            // ExecutionListView
            // 
            this.ExecutionListView.Anchor = ((System.Windows.Forms.AnchorStyles)(resources.GetObject("ExecutionListView.Anchor")));
            this.ExecutionListView.Location = ((System.Drawing.Point)(resources.GetObject("ExecutionListView.Location")));
            this.ExecutionListView.Name = "ExecutionListView";
            this.ExecutionListView.Size = ((System.Drawing.Size)(resources.GetObject("ExecutionListView.Size")));
            // 
            // DetailsButton
            // 
            this.DetailsButton.Anchor = ((System.Windows.Forms.AnchorStyles)(resources.GetObject("DetailsButton.Anchor")));
            this.DetailsButton.Location = ((System.Drawing.Point)(resources.GetObject("DetailsButton.Location")));
            this.DetailsButton.Name = "DetailsButton";
            this.DetailsButton.Size = ((System.Drawing.Size)(resources.GetObject("DetailsButton.Size")));
            // 
            // StopCloseFormButton
            // 
            this.StopCloseFormButton.Anchor = ((System.Windows.Forms.AnchorStyles)(resources.GetObject("StopCloseFormButton.Anchor")));
            this.StopCloseFormButton.Location = ((System.Drawing.Point)(resources.GetObject("StopCloseFormButton.Location")));
            this.StopCloseFormButton.Name = "StopCloseFormButton";
            this.StopCloseFormButton.Size = ((System.Drawing.Size)(resources.GetObject("StopCloseFormButton.Size")));
            this.StopCloseFormButton.Text = resources.GetString("StopCloseFormButton.Text");
            // 
            // ExecutionProgressBar
            // 
            this.ExecutionProgressBar.Anchor = ((System.Windows.Forms.AnchorStyles)(resources.GetObject("ExecutionProgressBar.Anchor")));
            this.ExecutionProgressBar.Location = ((System.Drawing.Point)(resources.GetObject("ExecutionProgressBar.Location")));
            this.ExecutionProgressBar.Name = "ExecutionProgressBar";
            this.ExecutionProgressBar.Size = ((System.Drawing.Size)(resources.GetObject("ExecutionProgressBar.Size")));
            // 
            // TypeOperationLabel
            // 
            this.TypeOperationLabel.Anchor = ((System.Windows.Forms.AnchorStyles)(resources.GetObject("TypeOperationLabel.Anchor")));
            this.TypeOperationLabel.Location = ((System.Drawing.Point)(resources.GetObject("TypeOperationLabel.Location")));
            this.TypeOperationLabel.Name = "TypeOperationLabel";
            this.TypeOperationLabel.Size = ((System.Drawing.Size)(resources.GetObject("TypeOperationLabel.Size")));
            // 
            // PathTextBox
            // 
            this.PathTextBox.Anchor = ((System.Windows.Forms.AnchorStyles)(resources.GetObject("PathTextBox.Anchor")));
            this.PathTextBox.Location = ((System.Drawing.Point)(resources.GetObject("PathTextBox.Location")));
            this.PathTextBox.Name = "PathTextBox";
            this.PathTextBox.Size = ((System.Drawing.Size)(resources.GetObject("PathTextBox.Size")));
            // 
            // DBSingleGroupBox
            // 
            this.DBSingleGroupBox.AccessibleDescription = resources.GetString("DBSingleGroupBox.AccessibleDescription");
            this.DBSingleGroupBox.AccessibleName = resources.GetString("DBSingleGroupBox.AccessibleName");
            this.DBSingleGroupBox.Anchor = ((System.Windows.Forms.AnchorStyles)(resources.GetObject("DBSingleGroupBox.Anchor")));
            this.DBSingleGroupBox.BackgroundImage = ((System.Drawing.Image)(resources.GetObject("DBSingleGroupBox.BackgroundImage")));
            this.DBSingleGroupBox.Controls.Add(this.MissingSingleValue);
            this.DBSingleGroupBox.Controls.Add(this.PastSingleValue);
            this.DBSingleGroupBox.Controls.Add(this.EstimatedSingleValue);
            this.DBSingleGroupBox.Controls.Add(this.EstimatedSingleLabel);
            this.DBSingleGroupBox.Controls.Add(this.MissingSingleLabel);
            this.DBSingleGroupBox.Controls.Add(this.PastSingleLabel);
            this.DBSingleGroupBox.Dock = ((System.Windows.Forms.DockStyle)(resources.GetObject("DBSingleGroupBox.Dock")));
            this.DBSingleGroupBox.Enabled = ((bool)(resources.GetObject("DBSingleGroupBox.Enabled")));
            this.DBSingleGroupBox.FlatStyle = System.Windows.Forms.FlatStyle.System;
            this.DBSingleGroupBox.Font = ((System.Drawing.Font)(resources.GetObject("DBSingleGroupBox.Font")));
            this.DBSingleGroupBox.ImeMode = ((System.Windows.Forms.ImeMode)(resources.GetObject("DBSingleGroupBox.ImeMode")));
            this.DBSingleGroupBox.Location = ((System.Drawing.Point)(resources.GetObject("DBSingleGroupBox.Location")));
            this.DBSingleGroupBox.Name = "DBSingleGroupBox";
            this.DBSingleGroupBox.RightToLeft = ((System.Windows.Forms.RightToLeft)(resources.GetObject("DBSingleGroupBox.RightToLeft")));
            this.DBSingleGroupBox.Size = ((System.Drawing.Size)(resources.GetObject("DBSingleGroupBox.Size")));
            this.DBSingleGroupBox.TabIndex = ((int)(resources.GetObject("DBSingleGroupBox.TabIndex")));
            this.DBSingleGroupBox.TabStop = false;
            this.DBSingleGroupBox.Text = resources.GetString("DBSingleGroupBox.Text");
            this.DBSingleGroupBox.Visible = ((bool)(resources.GetObject("DBSingleGroupBox.Visible")));
            // 
            // MissingSingleValue
            // 
            this.MissingSingleValue.AccessibleDescription = resources.GetString("MissingSingleValue.AccessibleDescription");
            this.MissingSingleValue.AccessibleName = resources.GetString("MissingSingleValue.AccessibleName");
            this.MissingSingleValue.Anchor = ((System.Windows.Forms.AnchorStyles)(resources.GetObject("MissingSingleValue.Anchor")));
            this.MissingSingleValue.AutoSize = ((bool)(resources.GetObject("MissingSingleValue.AutoSize")));
            this.MissingSingleValue.Dock = ((System.Windows.Forms.DockStyle)(resources.GetObject("MissingSingleValue.Dock")));
            this.MissingSingleValue.Enabled = ((bool)(resources.GetObject("MissingSingleValue.Enabled")));
            this.MissingSingleValue.FlatStyle = System.Windows.Forms.FlatStyle.System;
            this.MissingSingleValue.Font = ((System.Drawing.Font)(resources.GetObject("MissingSingleValue.Font")));
            this.MissingSingleValue.Image = ((System.Drawing.Image)(resources.GetObject("MissingSingleValue.Image")));
            this.MissingSingleValue.ImageAlign = ((System.Drawing.ContentAlignment)(resources.GetObject("MissingSingleValue.ImageAlign")));
            this.MissingSingleValue.ImageIndex = ((int)(resources.GetObject("MissingSingleValue.ImageIndex")));
            this.MissingSingleValue.ImeMode = ((System.Windows.Forms.ImeMode)(resources.GetObject("MissingSingleValue.ImeMode")));
            this.MissingSingleValue.Location = ((System.Drawing.Point)(resources.GetObject("MissingSingleValue.Location")));
            this.MissingSingleValue.Name = "MissingSingleValue";
            this.MissingSingleValue.RightToLeft = ((System.Windows.Forms.RightToLeft)(resources.GetObject("MissingSingleValue.RightToLeft")));
            this.MissingSingleValue.Size = ((System.Drawing.Size)(resources.GetObject("MissingSingleValue.Size")));
            this.MissingSingleValue.TabIndex = ((int)(resources.GetObject("MissingSingleValue.TabIndex")));
            this.MissingSingleValue.Text = resources.GetString("MissingSingleValue.Text");
            this.MissingSingleValue.TextAlign = ((System.Drawing.ContentAlignment)(resources.GetObject("MissingSingleValue.TextAlign")));
            this.MissingSingleValue.Visible = ((bool)(resources.GetObject("MissingSingleValue.Visible")));
            // 
            // PastSingleValue
            // 
            this.PastSingleValue.AccessibleDescription = resources.GetString("PastSingleValue.AccessibleDescription");
            this.PastSingleValue.AccessibleName = resources.GetString("PastSingleValue.AccessibleName");
            this.PastSingleValue.Anchor = ((System.Windows.Forms.AnchorStyles)(resources.GetObject("PastSingleValue.Anchor")));
            this.PastSingleValue.AutoSize = ((bool)(resources.GetObject("PastSingleValue.AutoSize")));
            this.PastSingleValue.Dock = ((System.Windows.Forms.DockStyle)(resources.GetObject("PastSingleValue.Dock")));
            this.PastSingleValue.Enabled = ((bool)(resources.GetObject("PastSingleValue.Enabled")));
            this.PastSingleValue.FlatStyle = System.Windows.Forms.FlatStyle.System;
            this.PastSingleValue.Font = ((System.Drawing.Font)(resources.GetObject("PastSingleValue.Font")));
            this.PastSingleValue.Image = ((System.Drawing.Image)(resources.GetObject("PastSingleValue.Image")));
            this.PastSingleValue.ImageAlign = ((System.Drawing.ContentAlignment)(resources.GetObject("PastSingleValue.ImageAlign")));
            this.PastSingleValue.ImageIndex = ((int)(resources.GetObject("PastSingleValue.ImageIndex")));
            this.PastSingleValue.ImeMode = ((System.Windows.Forms.ImeMode)(resources.GetObject("PastSingleValue.ImeMode")));
            this.PastSingleValue.Location = ((System.Drawing.Point)(resources.GetObject("PastSingleValue.Location")));
            this.PastSingleValue.Name = "PastSingleValue";
            this.PastSingleValue.RightToLeft = ((System.Windows.Forms.RightToLeft)(resources.GetObject("PastSingleValue.RightToLeft")));
            this.PastSingleValue.Size = ((System.Drawing.Size)(resources.GetObject("PastSingleValue.Size")));
            this.PastSingleValue.TabIndex = ((int)(resources.GetObject("PastSingleValue.TabIndex")));
            this.PastSingleValue.Text = resources.GetString("PastSingleValue.Text");
            this.PastSingleValue.TextAlign = ((System.Drawing.ContentAlignment)(resources.GetObject("PastSingleValue.TextAlign")));
            this.PastSingleValue.Visible = ((bool)(resources.GetObject("PastSingleValue.Visible")));
            // 
            // EstimatedSingleValue
            // 
            this.EstimatedSingleValue.AccessibleDescription = resources.GetString("EstimatedSingleValue.AccessibleDescription");
            this.EstimatedSingleValue.AccessibleName = resources.GetString("EstimatedSingleValue.AccessibleName");
            this.EstimatedSingleValue.Anchor = ((System.Windows.Forms.AnchorStyles)(resources.GetObject("EstimatedSingleValue.Anchor")));
            this.EstimatedSingleValue.AutoSize = ((bool)(resources.GetObject("EstimatedSingleValue.AutoSize")));
            this.EstimatedSingleValue.Dock = ((System.Windows.Forms.DockStyle)(resources.GetObject("EstimatedSingleValue.Dock")));
            this.EstimatedSingleValue.Enabled = ((bool)(resources.GetObject("EstimatedSingleValue.Enabled")));
            this.EstimatedSingleValue.FlatStyle = System.Windows.Forms.FlatStyle.System;
            this.EstimatedSingleValue.Font = ((System.Drawing.Font)(resources.GetObject("EstimatedSingleValue.Font")));
            this.EstimatedSingleValue.Image = ((System.Drawing.Image)(resources.GetObject("EstimatedSingleValue.Image")));
            this.EstimatedSingleValue.ImageAlign = ((System.Drawing.ContentAlignment)(resources.GetObject("EstimatedSingleValue.ImageAlign")));
            this.EstimatedSingleValue.ImageIndex = ((int)(resources.GetObject("EstimatedSingleValue.ImageIndex")));
            this.EstimatedSingleValue.ImeMode = ((System.Windows.Forms.ImeMode)(resources.GetObject("EstimatedSingleValue.ImeMode")));
            this.EstimatedSingleValue.Location = ((System.Drawing.Point)(resources.GetObject("EstimatedSingleValue.Location")));
            this.EstimatedSingleValue.Name = "EstimatedSingleValue";
            this.EstimatedSingleValue.RightToLeft = ((System.Windows.Forms.RightToLeft)(resources.GetObject("EstimatedSingleValue.RightToLeft")));
            this.EstimatedSingleValue.Size = ((System.Drawing.Size)(resources.GetObject("EstimatedSingleValue.Size")));
            this.EstimatedSingleValue.TabIndex = ((int)(resources.GetObject("EstimatedSingleValue.TabIndex")));
            this.EstimatedSingleValue.Text = resources.GetString("EstimatedSingleValue.Text");
            this.EstimatedSingleValue.TextAlign = ((System.Drawing.ContentAlignment)(resources.GetObject("EstimatedSingleValue.TextAlign")));
            this.EstimatedSingleValue.Visible = ((bool)(resources.GetObject("EstimatedSingleValue.Visible")));
            // 
            // EstimatedSingleLabel
            // 
            this.EstimatedSingleLabel.AccessibleDescription = resources.GetString("EstimatedSingleLabel.AccessibleDescription");
            this.EstimatedSingleLabel.AccessibleName = resources.GetString("EstimatedSingleLabel.AccessibleName");
            this.EstimatedSingleLabel.Anchor = ((System.Windows.Forms.AnchorStyles)(resources.GetObject("EstimatedSingleLabel.Anchor")));
            this.EstimatedSingleLabel.AutoSize = ((bool)(resources.GetObject("EstimatedSingleLabel.AutoSize")));
            this.EstimatedSingleLabel.Dock = ((System.Windows.Forms.DockStyle)(resources.GetObject("EstimatedSingleLabel.Dock")));
            this.EstimatedSingleLabel.Enabled = ((bool)(resources.GetObject("EstimatedSingleLabel.Enabled")));
            this.EstimatedSingleLabel.FlatStyle = System.Windows.Forms.FlatStyle.System;
            this.EstimatedSingleLabel.Font = ((System.Drawing.Font)(resources.GetObject("EstimatedSingleLabel.Font")));
            this.EstimatedSingleLabel.Image = ((System.Drawing.Image)(resources.GetObject("EstimatedSingleLabel.Image")));
            this.EstimatedSingleLabel.ImageAlign = ((System.Drawing.ContentAlignment)(resources.GetObject("EstimatedSingleLabel.ImageAlign")));
            this.EstimatedSingleLabel.ImageIndex = ((int)(resources.GetObject("EstimatedSingleLabel.ImageIndex")));
            this.EstimatedSingleLabel.ImeMode = ((System.Windows.Forms.ImeMode)(resources.GetObject("EstimatedSingleLabel.ImeMode")));
            this.EstimatedSingleLabel.Location = ((System.Drawing.Point)(resources.GetObject("EstimatedSingleLabel.Location")));
            this.EstimatedSingleLabel.Name = "EstimatedSingleLabel";
            this.EstimatedSingleLabel.RightToLeft = ((System.Windows.Forms.RightToLeft)(resources.GetObject("EstimatedSingleLabel.RightToLeft")));
            this.EstimatedSingleLabel.Size = ((System.Drawing.Size)(resources.GetObject("EstimatedSingleLabel.Size")));
            this.EstimatedSingleLabel.TabIndex = ((int)(resources.GetObject("EstimatedSingleLabel.TabIndex")));
            this.EstimatedSingleLabel.Text = resources.GetString("EstimatedSingleLabel.Text");
            this.EstimatedSingleLabel.TextAlign = ((System.Drawing.ContentAlignment)(resources.GetObject("EstimatedSingleLabel.TextAlign")));
            this.EstimatedSingleLabel.Visible = ((bool)(resources.GetObject("EstimatedSingleLabel.Visible")));
            // 
            // MissingSingleLabel
            // 
            this.MissingSingleLabel.AccessibleDescription = resources.GetString("MissingSingleLabel.AccessibleDescription");
            this.MissingSingleLabel.AccessibleName = resources.GetString("MissingSingleLabel.AccessibleName");
            this.MissingSingleLabel.Anchor = ((System.Windows.Forms.AnchorStyles)(resources.GetObject("MissingSingleLabel.Anchor")));
            this.MissingSingleLabel.AutoSize = ((bool)(resources.GetObject("MissingSingleLabel.AutoSize")));
            this.MissingSingleLabel.Dock = ((System.Windows.Forms.DockStyle)(resources.GetObject("MissingSingleLabel.Dock")));
            this.MissingSingleLabel.Enabled = ((bool)(resources.GetObject("MissingSingleLabel.Enabled")));
            this.MissingSingleLabel.FlatStyle = System.Windows.Forms.FlatStyle.System;
            this.MissingSingleLabel.Font = ((System.Drawing.Font)(resources.GetObject("MissingSingleLabel.Font")));
            this.MissingSingleLabel.Image = ((System.Drawing.Image)(resources.GetObject("MissingSingleLabel.Image")));
            this.MissingSingleLabel.ImageAlign = ((System.Drawing.ContentAlignment)(resources.GetObject("MissingSingleLabel.ImageAlign")));
            this.MissingSingleLabel.ImageIndex = ((int)(resources.GetObject("MissingSingleLabel.ImageIndex")));
            this.MissingSingleLabel.ImeMode = ((System.Windows.Forms.ImeMode)(resources.GetObject("MissingSingleLabel.ImeMode")));
            this.MissingSingleLabel.Location = ((System.Drawing.Point)(resources.GetObject("MissingSingleLabel.Location")));
            this.MissingSingleLabel.Name = "MissingSingleLabel";
            this.MissingSingleLabel.RightToLeft = ((System.Windows.Forms.RightToLeft)(resources.GetObject("MissingSingleLabel.RightToLeft")));
            this.MissingSingleLabel.Size = ((System.Drawing.Size)(resources.GetObject("MissingSingleLabel.Size")));
            this.MissingSingleLabel.TabIndex = ((int)(resources.GetObject("MissingSingleLabel.TabIndex")));
            this.MissingSingleLabel.Text = resources.GetString("MissingSingleLabel.Text");
            this.MissingSingleLabel.TextAlign = ((System.Drawing.ContentAlignment)(resources.GetObject("MissingSingleLabel.TextAlign")));
            this.MissingSingleLabel.Visible = ((bool)(resources.GetObject("MissingSingleLabel.Visible")));
            // 
            // PastSingleLabel
            // 
            this.PastSingleLabel.AccessibleDescription = resources.GetString("PastSingleLabel.AccessibleDescription");
            this.PastSingleLabel.AccessibleName = resources.GetString("PastSingleLabel.AccessibleName");
            this.PastSingleLabel.Anchor = ((System.Windows.Forms.AnchorStyles)(resources.GetObject("PastSingleLabel.Anchor")));
            this.PastSingleLabel.AutoSize = ((bool)(resources.GetObject("PastSingleLabel.AutoSize")));
            this.PastSingleLabel.Dock = ((System.Windows.Forms.DockStyle)(resources.GetObject("PastSingleLabel.Dock")));
            this.PastSingleLabel.Enabled = ((bool)(resources.GetObject("PastSingleLabel.Enabled")));
            this.PastSingleLabel.FlatStyle = System.Windows.Forms.FlatStyle.System;
            this.PastSingleLabel.Font = ((System.Drawing.Font)(resources.GetObject("PastSingleLabel.Font")));
            this.PastSingleLabel.Image = ((System.Drawing.Image)(resources.GetObject("PastSingleLabel.Image")));
            this.PastSingleLabel.ImageAlign = ((System.Drawing.ContentAlignment)(resources.GetObject("PastSingleLabel.ImageAlign")));
            this.PastSingleLabel.ImageIndex = ((int)(resources.GetObject("PastSingleLabel.ImageIndex")));
            this.PastSingleLabel.ImeMode = ((System.Windows.Forms.ImeMode)(resources.GetObject("PastSingleLabel.ImeMode")));
            this.PastSingleLabel.Location = ((System.Drawing.Point)(resources.GetObject("PastSingleLabel.Location")));
            this.PastSingleLabel.Name = "PastSingleLabel";
            this.PastSingleLabel.RightToLeft = ((System.Windows.Forms.RightToLeft)(resources.GetObject("PastSingleLabel.RightToLeft")));
            this.PastSingleLabel.Size = ((System.Drawing.Size)(resources.GetObject("PastSingleLabel.Size")));
            this.PastSingleLabel.TabIndex = ((int)(resources.GetObject("PastSingleLabel.TabIndex")));
            this.PastSingleLabel.Text = resources.GetString("PastSingleLabel.Text");
            this.PastSingleLabel.TextAlign = ((System.Drawing.ContentAlignment)(resources.GetObject("PastSingleLabel.TextAlign")));
            this.PastSingleLabel.Visible = ((bool)(resources.GetObject("PastSingleLabel.Visible")));
            // 
            // DBTotalGroupBox
            // 
            this.DBTotalGroupBox.AccessibleDescription = resources.GetString("DBTotalGroupBox.AccessibleDescription");
            this.DBTotalGroupBox.AccessibleName = resources.GetString("DBTotalGroupBox.AccessibleName");
            this.DBTotalGroupBox.Anchor = ((System.Windows.Forms.AnchorStyles)(resources.GetObject("DBTotalGroupBox.Anchor")));
            this.DBTotalGroupBox.BackgroundImage = ((System.Drawing.Image)(resources.GetObject("DBTotalGroupBox.BackgroundImage")));
            this.DBTotalGroupBox.Controls.Add(this.MissingTotalValue);
            this.DBTotalGroupBox.Controls.Add(this.PastTotalValue);
            this.DBTotalGroupBox.Controls.Add(this.EstimatedTotalValue);
            this.DBTotalGroupBox.Controls.Add(this.EstimatedTotalLabel);
            this.DBTotalGroupBox.Controls.Add(this.MissingTotalLabel);
            this.DBTotalGroupBox.Controls.Add(this.PastTotalLabel);
            this.DBTotalGroupBox.Dock = ((System.Windows.Forms.DockStyle)(resources.GetObject("DBTotalGroupBox.Dock")));
            this.DBTotalGroupBox.Enabled = ((bool)(resources.GetObject("DBTotalGroupBox.Enabled")));
            this.DBTotalGroupBox.FlatStyle = System.Windows.Forms.FlatStyle.System;
            this.DBTotalGroupBox.Font = ((System.Drawing.Font)(resources.GetObject("DBTotalGroupBox.Font")));
            this.DBTotalGroupBox.ImeMode = ((System.Windows.Forms.ImeMode)(resources.GetObject("DBTotalGroupBox.ImeMode")));
            this.DBTotalGroupBox.Location = ((System.Drawing.Point)(resources.GetObject("DBTotalGroupBox.Location")));
            this.DBTotalGroupBox.Name = "DBTotalGroupBox";
            this.DBTotalGroupBox.RightToLeft = ((System.Windows.Forms.RightToLeft)(resources.GetObject("DBTotalGroupBox.RightToLeft")));
            this.DBTotalGroupBox.Size = ((System.Drawing.Size)(resources.GetObject("DBTotalGroupBox.Size")));
            this.DBTotalGroupBox.TabIndex = ((int)(resources.GetObject("DBTotalGroupBox.TabIndex")));
            this.DBTotalGroupBox.TabStop = false;
            this.DBTotalGroupBox.Text = resources.GetString("DBTotalGroupBox.Text");
            this.DBTotalGroupBox.Visible = ((bool)(resources.GetObject("DBTotalGroupBox.Visible")));
            // 
            // MissingTotalValue
            // 
            this.MissingTotalValue.AccessibleDescription = resources.GetString("MissingTotalValue.AccessibleDescription");
            this.MissingTotalValue.AccessibleName = resources.GetString("MissingTotalValue.AccessibleName");
            this.MissingTotalValue.Anchor = ((System.Windows.Forms.AnchorStyles)(resources.GetObject("MissingTotalValue.Anchor")));
            this.MissingTotalValue.AutoSize = ((bool)(resources.GetObject("MissingTotalValue.AutoSize")));
            this.MissingTotalValue.Dock = ((System.Windows.Forms.DockStyle)(resources.GetObject("MissingTotalValue.Dock")));
            this.MissingTotalValue.Enabled = ((bool)(resources.GetObject("MissingTotalValue.Enabled")));
            this.MissingTotalValue.FlatStyle = System.Windows.Forms.FlatStyle.System;
            this.MissingTotalValue.Font = ((System.Drawing.Font)(resources.GetObject("MissingTotalValue.Font")));
            this.MissingTotalValue.Image = ((System.Drawing.Image)(resources.GetObject("MissingTotalValue.Image")));
            this.MissingTotalValue.ImageAlign = ((System.Drawing.ContentAlignment)(resources.GetObject("MissingTotalValue.ImageAlign")));
            this.MissingTotalValue.ImageIndex = ((int)(resources.GetObject("MissingTotalValue.ImageIndex")));
            this.MissingTotalValue.ImeMode = ((System.Windows.Forms.ImeMode)(resources.GetObject("MissingTotalValue.ImeMode")));
            this.MissingTotalValue.Location = ((System.Drawing.Point)(resources.GetObject("MissingTotalValue.Location")));
            this.MissingTotalValue.Name = "MissingTotalValue";
            this.MissingTotalValue.RightToLeft = ((System.Windows.Forms.RightToLeft)(resources.GetObject("MissingTotalValue.RightToLeft")));
            this.MissingTotalValue.Size = ((System.Drawing.Size)(resources.GetObject("MissingTotalValue.Size")));
            this.MissingTotalValue.TabIndex = ((int)(resources.GetObject("MissingTotalValue.TabIndex")));
            this.MissingTotalValue.Text = resources.GetString("MissingTotalValue.Text");
            this.MissingTotalValue.TextAlign = ((System.Drawing.ContentAlignment)(resources.GetObject("MissingTotalValue.TextAlign")));
            this.MissingTotalValue.Visible = ((bool)(resources.GetObject("MissingTotalValue.Visible")));
            // 
            // PastTotalValue
            // 
            this.PastTotalValue.AccessibleDescription = resources.GetString("PastTotalValue.AccessibleDescription");
            this.PastTotalValue.AccessibleName = resources.GetString("PastTotalValue.AccessibleName");
            this.PastTotalValue.Anchor = ((System.Windows.Forms.AnchorStyles)(resources.GetObject("PastTotalValue.Anchor")));
            this.PastTotalValue.AutoSize = ((bool)(resources.GetObject("PastTotalValue.AutoSize")));
            this.PastTotalValue.Dock = ((System.Windows.Forms.DockStyle)(resources.GetObject("PastTotalValue.Dock")));
            this.PastTotalValue.Enabled = ((bool)(resources.GetObject("PastTotalValue.Enabled")));
            this.PastTotalValue.FlatStyle = System.Windows.Forms.FlatStyle.System;
            this.PastTotalValue.Font = ((System.Drawing.Font)(resources.GetObject("PastTotalValue.Font")));
            this.PastTotalValue.Image = ((System.Drawing.Image)(resources.GetObject("PastTotalValue.Image")));
            this.PastTotalValue.ImageAlign = ((System.Drawing.ContentAlignment)(resources.GetObject("PastTotalValue.ImageAlign")));
            this.PastTotalValue.ImageIndex = ((int)(resources.GetObject("PastTotalValue.ImageIndex")));
            this.PastTotalValue.ImeMode = ((System.Windows.Forms.ImeMode)(resources.GetObject("PastTotalValue.ImeMode")));
            this.PastTotalValue.Location = ((System.Drawing.Point)(resources.GetObject("PastTotalValue.Location")));
            this.PastTotalValue.Name = "PastTotalValue";
            this.PastTotalValue.RightToLeft = ((System.Windows.Forms.RightToLeft)(resources.GetObject("PastTotalValue.RightToLeft")));
            this.PastTotalValue.Size = ((System.Drawing.Size)(resources.GetObject("PastTotalValue.Size")));
            this.PastTotalValue.TabIndex = ((int)(resources.GetObject("PastTotalValue.TabIndex")));
            this.PastTotalValue.Text = resources.GetString("PastTotalValue.Text");
            this.PastTotalValue.TextAlign = ((System.Drawing.ContentAlignment)(resources.GetObject("PastTotalValue.TextAlign")));
            this.PastTotalValue.Visible = ((bool)(resources.GetObject("PastTotalValue.Visible")));
            // 
            // EstimatedTotalValue
            // 
            this.EstimatedTotalValue.AccessibleDescription = resources.GetString("EstimatedTotalValue.AccessibleDescription");
            this.EstimatedTotalValue.AccessibleName = resources.GetString("EstimatedTotalValue.AccessibleName");
            this.EstimatedTotalValue.Anchor = ((System.Windows.Forms.AnchorStyles)(resources.GetObject("EstimatedTotalValue.Anchor")));
            this.EstimatedTotalValue.AutoSize = ((bool)(resources.GetObject("EstimatedTotalValue.AutoSize")));
            this.EstimatedTotalValue.Dock = ((System.Windows.Forms.DockStyle)(resources.GetObject("EstimatedTotalValue.Dock")));
            this.EstimatedTotalValue.Enabled = ((bool)(resources.GetObject("EstimatedTotalValue.Enabled")));
            this.EstimatedTotalValue.FlatStyle = System.Windows.Forms.FlatStyle.System;
            this.EstimatedTotalValue.Font = ((System.Drawing.Font)(resources.GetObject("EstimatedTotalValue.Font")));
            this.EstimatedTotalValue.Image = ((System.Drawing.Image)(resources.GetObject("EstimatedTotalValue.Image")));
            this.EstimatedTotalValue.ImageAlign = ((System.Drawing.ContentAlignment)(resources.GetObject("EstimatedTotalValue.ImageAlign")));
            this.EstimatedTotalValue.ImageIndex = ((int)(resources.GetObject("EstimatedTotalValue.ImageIndex")));
            this.EstimatedTotalValue.ImeMode = ((System.Windows.Forms.ImeMode)(resources.GetObject("EstimatedTotalValue.ImeMode")));
            this.EstimatedTotalValue.Location = ((System.Drawing.Point)(resources.GetObject("EstimatedTotalValue.Location")));
            this.EstimatedTotalValue.Name = "EstimatedTotalValue";
            this.EstimatedTotalValue.RightToLeft = ((System.Windows.Forms.RightToLeft)(resources.GetObject("EstimatedTotalValue.RightToLeft")));
            this.EstimatedTotalValue.Size = ((System.Drawing.Size)(resources.GetObject("EstimatedTotalValue.Size")));
            this.EstimatedTotalValue.TabIndex = ((int)(resources.GetObject("EstimatedTotalValue.TabIndex")));
            this.EstimatedTotalValue.Text = resources.GetString("EstimatedTotalValue.Text");
            this.EstimatedTotalValue.TextAlign = ((System.Drawing.ContentAlignment)(resources.GetObject("EstimatedTotalValue.TextAlign")));
            this.EstimatedTotalValue.Visible = ((bool)(resources.GetObject("EstimatedTotalValue.Visible")));
            // 
            // EstimatedTotalLabel
            // 
            this.EstimatedTotalLabel.AccessibleDescription = resources.GetString("EstimatedTotalLabel.AccessibleDescription");
            this.EstimatedTotalLabel.AccessibleName = resources.GetString("EstimatedTotalLabel.AccessibleName");
            this.EstimatedTotalLabel.Anchor = ((System.Windows.Forms.AnchorStyles)(resources.GetObject("EstimatedTotalLabel.Anchor")));
            this.EstimatedTotalLabel.AutoSize = ((bool)(resources.GetObject("EstimatedTotalLabel.AutoSize")));
            this.EstimatedTotalLabel.Dock = ((System.Windows.Forms.DockStyle)(resources.GetObject("EstimatedTotalLabel.Dock")));
            this.EstimatedTotalLabel.Enabled = ((bool)(resources.GetObject("EstimatedTotalLabel.Enabled")));
            this.EstimatedTotalLabel.FlatStyle = System.Windows.Forms.FlatStyle.System;
            this.EstimatedTotalLabel.Font = ((System.Drawing.Font)(resources.GetObject("EstimatedTotalLabel.Font")));
            this.EstimatedTotalLabel.Image = ((System.Drawing.Image)(resources.GetObject("EstimatedTotalLabel.Image")));
            this.EstimatedTotalLabel.ImageAlign = ((System.Drawing.ContentAlignment)(resources.GetObject("EstimatedTotalLabel.ImageAlign")));
            this.EstimatedTotalLabel.ImageIndex = ((int)(resources.GetObject("EstimatedTotalLabel.ImageIndex")));
            this.EstimatedTotalLabel.ImeMode = ((System.Windows.Forms.ImeMode)(resources.GetObject("EstimatedTotalLabel.ImeMode")));
            this.EstimatedTotalLabel.Location = ((System.Drawing.Point)(resources.GetObject("EstimatedTotalLabel.Location")));
            this.EstimatedTotalLabel.Name = "EstimatedTotalLabel";
            this.EstimatedTotalLabel.RightToLeft = ((System.Windows.Forms.RightToLeft)(resources.GetObject("EstimatedTotalLabel.RightToLeft")));
            this.EstimatedTotalLabel.Size = ((System.Drawing.Size)(resources.GetObject("EstimatedTotalLabel.Size")));
            this.EstimatedTotalLabel.TabIndex = ((int)(resources.GetObject("EstimatedTotalLabel.TabIndex")));
            this.EstimatedTotalLabel.Text = resources.GetString("EstimatedTotalLabel.Text");
            this.EstimatedTotalLabel.TextAlign = ((System.Drawing.ContentAlignment)(resources.GetObject("EstimatedTotalLabel.TextAlign")));
            this.EstimatedTotalLabel.Visible = ((bool)(resources.GetObject("EstimatedTotalLabel.Visible")));
            // 
            // MissingTotalLabel
            // 
            this.MissingTotalLabel.AccessibleDescription = resources.GetString("MissingTotalLabel.AccessibleDescription");
            this.MissingTotalLabel.AccessibleName = resources.GetString("MissingTotalLabel.AccessibleName");
            this.MissingTotalLabel.Anchor = ((System.Windows.Forms.AnchorStyles)(resources.GetObject("MissingTotalLabel.Anchor")));
            this.MissingTotalLabel.AutoSize = ((bool)(resources.GetObject("MissingTotalLabel.AutoSize")));
            this.MissingTotalLabel.Dock = ((System.Windows.Forms.DockStyle)(resources.GetObject("MissingTotalLabel.Dock")));
            this.MissingTotalLabel.Enabled = ((bool)(resources.GetObject("MissingTotalLabel.Enabled")));
            this.MissingTotalLabel.FlatStyle = System.Windows.Forms.FlatStyle.System;
            this.MissingTotalLabel.Font = ((System.Drawing.Font)(resources.GetObject("MissingTotalLabel.Font")));
            this.MissingTotalLabel.Image = ((System.Drawing.Image)(resources.GetObject("MissingTotalLabel.Image")));
            this.MissingTotalLabel.ImageAlign = ((System.Drawing.ContentAlignment)(resources.GetObject("MissingTotalLabel.ImageAlign")));
            this.MissingTotalLabel.ImageIndex = ((int)(resources.GetObject("MissingTotalLabel.ImageIndex")));
            this.MissingTotalLabel.ImeMode = ((System.Windows.Forms.ImeMode)(resources.GetObject("MissingTotalLabel.ImeMode")));
            this.MissingTotalLabel.Location = ((System.Drawing.Point)(resources.GetObject("MissingTotalLabel.Location")));
            this.MissingTotalLabel.Name = "MissingTotalLabel";
            this.MissingTotalLabel.RightToLeft = ((System.Windows.Forms.RightToLeft)(resources.GetObject("MissingTotalLabel.RightToLeft")));
            this.MissingTotalLabel.Size = ((System.Drawing.Size)(resources.GetObject("MissingTotalLabel.Size")));
            this.MissingTotalLabel.TabIndex = ((int)(resources.GetObject("MissingTotalLabel.TabIndex")));
            this.MissingTotalLabel.Text = resources.GetString("MissingTotalLabel.Text");
            this.MissingTotalLabel.TextAlign = ((System.Drawing.ContentAlignment)(resources.GetObject("MissingTotalLabel.TextAlign")));
            this.MissingTotalLabel.Visible = ((bool)(resources.GetObject("MissingTotalLabel.Visible")));
            // 
            // PastTotalLabel
            // 
            this.PastTotalLabel.AccessibleDescription = resources.GetString("PastTotalLabel.AccessibleDescription");
            this.PastTotalLabel.AccessibleName = resources.GetString("PastTotalLabel.AccessibleName");
            this.PastTotalLabel.Anchor = ((System.Windows.Forms.AnchorStyles)(resources.GetObject("PastTotalLabel.Anchor")));
            this.PastTotalLabel.AutoSize = ((bool)(resources.GetObject("PastTotalLabel.AutoSize")));
            this.PastTotalLabel.Dock = ((System.Windows.Forms.DockStyle)(resources.GetObject("PastTotalLabel.Dock")));
            this.PastTotalLabel.Enabled = ((bool)(resources.GetObject("PastTotalLabel.Enabled")));
            this.PastTotalLabel.FlatStyle = System.Windows.Forms.FlatStyle.System;
            this.PastTotalLabel.Font = ((System.Drawing.Font)(resources.GetObject("PastTotalLabel.Font")));
            this.PastTotalLabel.Image = ((System.Drawing.Image)(resources.GetObject("PastTotalLabel.Image")));
            this.PastTotalLabel.ImageAlign = ((System.Drawing.ContentAlignment)(resources.GetObject("PastTotalLabel.ImageAlign")));
            this.PastTotalLabel.ImageIndex = ((int)(resources.GetObject("PastTotalLabel.ImageIndex")));
            this.PastTotalLabel.ImeMode = ((System.Windows.Forms.ImeMode)(resources.GetObject("PastTotalLabel.ImeMode")));
            this.PastTotalLabel.Location = ((System.Drawing.Point)(resources.GetObject("PastTotalLabel.Location")));
            this.PastTotalLabel.Name = "PastTotalLabel";
            this.PastTotalLabel.RightToLeft = ((System.Windows.Forms.RightToLeft)(resources.GetObject("PastTotalLabel.RightToLeft")));
            this.PastTotalLabel.Size = ((System.Drawing.Size)(resources.GetObject("PastTotalLabel.Size")));
            this.PastTotalLabel.TabIndex = ((int)(resources.GetObject("PastTotalLabel.TabIndex")));
            this.PastTotalLabel.Text = resources.GetString("PastTotalLabel.Text");
            this.PastTotalLabel.TextAlign = ((System.Drawing.ContentAlignment)(resources.GetObject("PastTotalLabel.TextAlign")));
            this.PastTotalLabel.Visible = ((bool)(resources.GetObject("PastTotalLabel.Visible")));
            // 
            // ProcessingForm
            // 
            this.AccessibleDescription = resources.GetString("$this.AccessibleDescription");
            this.AccessibleName = resources.GetString("$this.AccessibleName");
            this.AutoScaleBaseSize = ((System.Drawing.Size)(resources.GetObject("$this.AutoScaleBaseSize")));
            this.AutoScroll = ((bool)(resources.GetObject("$this.AutoScroll")));
            this.AutoScrollMargin = ((System.Drawing.Size)(resources.GetObject("$this.AutoScrollMargin")));
            this.AutoScrollMinSize = ((System.Drawing.Size)(resources.GetObject("$this.AutoScrollMinSize")));
            this.BackgroundImage = ((System.Drawing.Image)(resources.GetObject("$this.BackgroundImage")));
            this.ClientSize = ((System.Drawing.Size)(resources.GetObject("$this.ClientSize")));
            this.Controls.Add(this.DBTotalGroupBox);
            this.Controls.Add(this.DBSingleGroupBox);
            this.Enabled = ((bool)(resources.GetObject("$this.Enabled")));
            this.Font = ((System.Drawing.Font)(resources.GetObject("$this.Font")));
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.Sizable;
            this.Icon = ((System.Drawing.Icon)(resources.GetObject("$this.Icon")));
            this.ImeMode = ((System.Windows.Forms.ImeMode)(resources.GetObject("$this.ImeMode")));
            this.Location = ((System.Drawing.Point)(resources.GetObject("$this.Location")));
            this.MaximumSize = ((System.Drawing.Size)(resources.GetObject("$this.MaximumSize")));
            this.MinimumSize = ((System.Drawing.Size)(resources.GetObject("$this.MinimumSize")));
            this.Name = "ProcessingForm";
            this.RightToLeft = ((System.Windows.Forms.RightToLeft)(resources.GetObject("$this.RightToLeft")));
            this.StartPosition = ((System.Windows.Forms.FormStartPosition)(resources.GetObject("$this.StartPosition")));
            this.Text = resources.GetString("$this.Text");
            this.Controls.SetChildIndex(this.ExecutionListView, 0);
            this.Controls.SetChildIndex(this.TypeOperationLabel, 0);
            this.Controls.SetChildIndex(this.ExecutionProgressBar, 0);
            this.Controls.SetChildIndex(this.StopCloseFormButton, 0);
            this.Controls.SetChildIndex(this.DetailsButton, 0);
            this.Controls.SetChildIndex(this.PathTextBox, 0);
            this.Controls.SetChildIndex(this.DBSingleGroupBox, 0);
            this.Controls.SetChildIndex(this.DBTotalGroupBox, 0);
            this.DBSingleGroupBox.ResumeLayout(false);
            this.DBTotalGroupBox.ResumeLayout(false);
            this.ResumeLayout(false);

        }
        #endregion
    }
}
