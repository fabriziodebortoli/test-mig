namespace SecurityObjectsReleaseCompare
{
    partial class MainForm
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

        #region Windows Form Designer generated code

        /// <summary>
        /// Required method for Designer support - do not modify
        /// the contents of this method with the code editor.
        /// </summary>
        private void InitializeComponent()
        {
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(MainForm));
            this.label1 = new System.Windows.Forms.Label();
            this.label2 = new System.Windows.Forms.Label();
            this.OldReleasePathTextBox = new System.Windows.Forms.TextBox();
            this.NewReleasePathTextBox = new System.Windows.Forms.TextBox();
            this.OldPathButton = new System.Windows.Forms.Button();
            this.CompareReleaseButton = new System.Windows.Forms.Button();
            this.NewPathButton = new System.Windows.Forms.Button();
            this.openFileDialog = new System.Windows.Forms.OpenFileDialog();
            this.ProductNameComboBox = new System.Windows.Forms.ComboBox();
            this.label3 = new System.Windows.Forms.Label();
            this.DecorateWRMButton = new System.Windows.Forms.Button();
            this.ApplicationFolderTextBox = new System.Windows.Forms.TextBox();
            this.folderBrowserDialog1 = new System.Windows.Forms.FolderBrowserDialog();
            this.OpenFolderButton = new System.Windows.Forms.Button();
            this.label4 = new System.Windows.Forms.Label();
            this.label5 = new System.Windows.Forms.Label();
            this.RoleFileTextBox = new System.Windows.Forms.TextBox();
            this.RoleFileButton = new System.Windows.Forms.Button();
            this.label6 = new System.Windows.Forms.Label();
            this.AdvancedRoleTextBox = new System.Windows.Forms.TextBox();
            this.button1 = new System.Windows.Forms.Button();
            this.button2 = new System.Windows.Forms.Button();
            this.SuspendLayout();
            // 
            // label1
            // 
            resources.ApplyResources(this.label1, "label1");
            this.label1.Name = "label1";
            // 
            // label2
            // 
            resources.ApplyResources(this.label2, "label2");
            this.label2.Name = "label2";
            // 
            // OldReleasePathTextBox
            // 
            resources.ApplyResources(this.OldReleasePathTextBox, "OldReleasePathTextBox");
            this.OldReleasePathTextBox.Name = "OldReleasePathTextBox";
            // 
            // NewReleasePathTextBox
            // 
            resources.ApplyResources(this.NewReleasePathTextBox, "NewReleasePathTextBox");
            this.NewReleasePathTextBox.Name = "NewReleasePathTextBox";
            // 
            // OldPathButton
            // 
            resources.ApplyResources(this.OldPathButton, "OldPathButton");
            this.OldPathButton.Name = "OldPathButton";
            this.OldPathButton.UseVisualStyleBackColor = true;
            this.OldPathButton.Click += new System.EventHandler(this.OldPathButton_Click);
            // 
            // CompareReleaseButton
            // 
            resources.ApplyResources(this.CompareReleaseButton, "CompareReleaseButton");
            this.CompareReleaseButton.Name = "CompareReleaseButton";
            this.CompareReleaseButton.UseVisualStyleBackColor = true;
            this.CompareReleaseButton.Click += new System.EventHandler(this.CompareReleaseButton_Click);
            // 
            // NewPathButton
            // 
            resources.ApplyResources(this.NewPathButton, "NewPathButton");
            this.NewPathButton.Name = "NewPathButton";
            this.NewPathButton.UseVisualStyleBackColor = true;
            this.NewPathButton.Click += new System.EventHandler(this.NewPathButton_Click);
            // 
            // openFileDialog
            // 
            resources.ApplyResources(this.openFileDialog, "openFileDialog");
            this.openFileDialog.FileOk += new System.ComponentModel.CancelEventHandler(this.openFileDialog_FileOk);
            // 
            // ProductNameComboBox
            // 
            this.ProductNameComboBox.FormattingEnabled = true;
            this.ProductNameComboBox.Items.AddRange(new object[] {
            resources.GetString("ProductNameComboBox.Items"),
            resources.GetString("ProductNameComboBox.Items1")});
            resources.ApplyResources(this.ProductNameComboBox, "ProductNameComboBox");
            this.ProductNameComboBox.Name = "ProductNameComboBox";
            // 
            // label3
            // 
            resources.ApplyResources(this.label3, "label3");
            this.label3.Name = "label3";
            // 
            // DecorateWRMButton
            // 
            resources.ApplyResources(this.DecorateWRMButton, "DecorateWRMButton");
            this.DecorateWRMButton.Name = "DecorateWRMButton";
            this.DecorateWRMButton.UseVisualStyleBackColor = true;
            this.DecorateWRMButton.Click += new System.EventHandler(this.button1_Click);
            // 
            // ApplicationFolderTextBox
            // 
            resources.ApplyResources(this.ApplicationFolderTextBox, "ApplicationFolderTextBox");
            this.ApplicationFolderTextBox.Name = "ApplicationFolderTextBox";
            // 
            // OpenFolderButton
            // 
            resources.ApplyResources(this.OpenFolderButton, "OpenFolderButton");
            this.OpenFolderButton.Name = "OpenFolderButton";
            this.OpenFolderButton.UseVisualStyleBackColor = true;
            this.OpenFolderButton.Click += new System.EventHandler(this.OpenFolderButton_Click);
            // 
            // label4
            // 
            resources.ApplyResources(this.label4, "label4");
            this.label4.Name = "label4";
            // 
            // label5
            // 
            resources.ApplyResources(this.label5, "label5");
            this.label5.Name = "label5";
            // 
            // RoleFileTextBox
            // 
            resources.ApplyResources(this.RoleFileTextBox, "RoleFileTextBox");
            this.RoleFileTextBox.Name = "RoleFileTextBox";
            // 
            // RoleFileButton
            // 
            resources.ApplyResources(this.RoleFileButton, "RoleFileButton");
            this.RoleFileButton.Name = "RoleFileButton";
            this.RoleFileButton.UseVisualStyleBackColor = true;
            this.RoleFileButton.Click += new System.EventHandler(this.RoleFileButton_Click);
            // 
            // label6
            // 
            resources.ApplyResources(this.label6, "label6");
            this.label6.Name = "label6";
            // 
            // AdvancedRoleTextBox
            // 
            resources.ApplyResources(this.AdvancedRoleTextBox, "AdvancedRoleTextBox");
            this.AdvancedRoleTextBox.Name = "AdvancedRoleTextBox";
            // 
            // button1
            // 
            resources.ApplyResources(this.button1, "button1");
            this.button1.Name = "button1";
            this.button1.UseVisualStyleBackColor = true;
            this.button1.Click += new System.EventHandler(this.button1_Click_1);
            // 
            // button2
            // 
            resources.ApplyResources(this.button2, "button2");
            this.button2.Name = "button2";
            this.button2.UseVisualStyleBackColor = true;
            this.button2.Click += new System.EventHandler(this.button2_Click);
            // 
            // MainForm
            // 
            resources.ApplyResources(this, "$this");
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.Controls.Add(this.button2);
            this.Controls.Add(this.button1);
            this.Controls.Add(this.AdvancedRoleTextBox);
            this.Controls.Add(this.label6);
            this.Controls.Add(this.RoleFileButton);
            this.Controls.Add(this.RoleFileTextBox);
            this.Controls.Add(this.label5);
            this.Controls.Add(this.label4);
            this.Controls.Add(this.OpenFolderButton);
            this.Controls.Add(this.ApplicationFolderTextBox);
            this.Controls.Add(this.DecorateWRMButton);
            this.Controls.Add(this.label3);
            this.Controls.Add(this.ProductNameComboBox);
            this.Controls.Add(this.NewPathButton);
            this.Controls.Add(this.CompareReleaseButton);
            this.Controls.Add(this.OldPathButton);
            this.Controls.Add(this.NewReleasePathTextBox);
            this.Controls.Add(this.OldReleasePathTextBox);
            this.Controls.Add(this.label2);
            this.Controls.Add(this.label1);
            this.Name = "MainForm";
            this.Load += new System.EventHandler(this.MainForm_Load);
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.Label label1;
        private System.Windows.Forms.Label label2;
        private System.Windows.Forms.TextBox OldReleasePathTextBox;
        private System.Windows.Forms.TextBox NewReleasePathTextBox;
        private System.Windows.Forms.Button OldPathButton;
        private System.Windows.Forms.Button CompareReleaseButton;
        private System.Windows.Forms.Button NewPathButton;
        private System.Windows.Forms.OpenFileDialog openFileDialog;
        private System.Windows.Forms.ComboBox ProductNameComboBox;
        private System.Windows.Forms.Label label3;
        private System.Windows.Forms.Button DecorateWRMButton;
        private System.Windows.Forms.TextBox ApplicationFolderTextBox;
        private System.Windows.Forms.FolderBrowserDialog folderBrowserDialog1;
        private System.Windows.Forms.Button OpenFolderButton;
        private System.Windows.Forms.Label label4;
        private System.Windows.Forms.Label label5;
        private System.Windows.Forms.TextBox RoleFileTextBox;
        private System.Windows.Forms.Button RoleFileButton;
        private System.Windows.Forms.Label label6;
        private System.Windows.Forms.TextBox AdvancedRoleTextBox;
        private System.Windows.Forms.Button button1;
        private System.Windows.Forms.Button button2;
    }
}

