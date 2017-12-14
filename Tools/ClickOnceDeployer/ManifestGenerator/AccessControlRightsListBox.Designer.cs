namespace HttpNamespaceManager.UI
{
    partial class AccessControlRightsListBox
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
			System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(AccessControlRightsListBox));
			this.panelListBox = new System.Windows.Forms.Panel();
			this.tableRights = new System.Windows.Forms.TableLayoutPanel();
			this.tableHeader = new System.Windows.Forms.TableLayoutPanel();
			this.panelListBox.SuspendLayout();
			this.SuspendLayout();
			// 
			// panelListBox
			// 
			resources.ApplyResources(this.panelListBox, "panelListBox");
			this.panelListBox.BackColor = System.Drawing.SystemColors.Window;
			this.panelListBox.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
			this.panelListBox.Controls.Add(this.tableRights);
			this.panelListBox.Name = "panelListBox";
			// 
			// tableRights
			// 
			resources.ApplyResources(this.tableRights, "tableRights");
			this.tableRights.Name = "tableRights";
			// 
			// tableHeader
			// 
			resources.ApplyResources(this.tableHeader, "tableHeader");
			this.tableHeader.Name = "tableHeader";
			// 
			// AccessControlRightsListBox
			// 
			resources.ApplyResources(this, "$this");
			this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
			this.BackColor = System.Drawing.SystemColors.Control;
			this.Controls.Add(this.tableHeader);
			this.Controls.Add(this.panelListBox);
			this.Name = "AccessControlRightsListBox";
			this.panelListBox.ResumeLayout(false);
			this.ResumeLayout(false);
			this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.Panel panelListBox;
        private System.Windows.Forms.TableLayoutPanel tableRights;
        private System.Windows.Forms.TableLayoutPanel tableHeader;

    }
}
