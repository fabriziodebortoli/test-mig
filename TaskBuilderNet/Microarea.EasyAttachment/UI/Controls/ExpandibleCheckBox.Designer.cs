namespace Microarea.EasyAttachment.UI.Controls
{
    partial class ExpandibleCheckBox
    {
        /// <summary> 
        /// Required designer variable.
        /// </summary>
        private System.ComponentModel.IContainer components = null;

        /// <summary> 
        /// Clean up any resources being used.
        /// </summary>
        /// <param name="disposing">true if managed resources should be disposed; otherwise, false.</param>
        protected override void Dispose(bool disposing)
        {
            if (disposing && (components != null))
            {
                components.Dispose();
            }
            base.Dispose(disposing);
        }

        #region Component Designer generated code

        /// <summary> 
        /// Required method for Designer support - do not modify 
        /// the contents of this method with the code editor.
        /// </summary>
        private void InitializeComponent()
        {
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(ExpandibleCheckBox));
            this.CkbItem = new System.Windows.Forms.CheckBox();
            this.BtnZoom = new System.Windows.Forms.Button();
            this.SuspendLayout();
            // 
            // CkbItem
            // 
            resources.ApplyResources(this.CkbItem, "CkbItem");
            this.CkbItem.Name = "CkbItem";
            this.CkbItem.UseVisualStyleBackColor = false;
            // 
            // BtnZoom
            // 
            resources.ApplyResources(this.BtnZoom, "BtnZoom");
            this.BtnZoom.Name = "BtnZoom";
            this.BtnZoom.UseVisualStyleBackColor = true;
            this.BtnZoom.Click += new System.EventHandler(this.BtnZoom_Click);
            // 
            // ExpandibleCheckBox
            // 
            resources.ApplyResources(this, "$this");
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.Controls.Add(this.BtnZoom);
            this.Controls.Add(this.CkbItem);
            this.Name = "ExpandibleCheckBox";
            this.ResumeLayout(false);

        }

        #endregion

        private System.Windows.Forms.CheckBox CkbItem;
        private System.Windows.Forms.Button BtnZoom;
    }
}
