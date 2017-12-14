using ManifestGenerator;
namespace HttpNamespaceManager.UI
{
    partial class NamespaceMngControl
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
			System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(NamespaceMngControl));
			this.listHttpNamespaces = new System.Windows.Forms.ListBox();
			this.buttonAdd = new System.Windows.Forms.Button();
			this.buttonEdit = new System.Windows.Forms.Button();
			this.buttonRemove = new System.Windows.Forms.Button();
			this.SuspendLayout();
			// 
			// listHttpNamespaces
			// 
			resources.ApplyResources(this.listHttpNamespaces, "listHttpNamespaces");
			this.listHttpNamespaces.FormattingEnabled = true;
			this.listHttpNamespaces.Name = "listHttpNamespaces";
			// 
			// buttonAdd
			// 
			resources.ApplyResources(this.buttonAdd, "buttonAdd");
			this.buttonAdd.Image = global::ManifestGenerator.Resource.SecurityIconSmall;
			this.buttonAdd.Name = "buttonAdd";
			this.buttonAdd.UseVisualStyleBackColor = true;
			this.buttonAdd.Click += new System.EventHandler(this.buttonAdd_Click);
			// 
			// buttonEdit
			// 
			resources.ApplyResources(this.buttonEdit, "buttonEdit");
			this.buttonEdit.Image = global::ManifestGenerator.Resource.SecurityIconSmall;
			this.buttonEdit.Name = "buttonEdit";
			this.buttonEdit.UseVisualStyleBackColor = true;
			this.buttonEdit.Click += new System.EventHandler(this.buttonEdit_Click);
			// 
			// buttonRemove
			// 
			resources.ApplyResources(this.buttonRemove, "buttonRemove");
			this.buttonRemove.Image = global::ManifestGenerator.Resource.SecurityIconSmall;
			this.buttonRemove.Name = "buttonRemove";
			this.buttonRemove.UseVisualStyleBackColor = true;
			this.buttonRemove.Click += new System.EventHandler(this.buttonRemove_Click);
			// 
			// NamespaceMngControl
			// 
			resources.ApplyResources(this, "$this");
			this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
			this.Controls.Add(this.buttonAdd);
			this.Controls.Add(this.buttonEdit);
			this.Controls.Add(this.buttonRemove);
			this.Controls.Add(this.listHttpNamespaces);
			this.Name = "NamespaceMngControl";
			this.Load += new System.EventHandler(this.MainForm_Load);
			this.ResumeLayout(false);

        }

        #endregion

        private System.Windows.Forms.ListBox listHttpNamespaces;
        private System.Windows.Forms.Button buttonRemove;
        private System.Windows.Forms.Button buttonEdit;
		private System.Windows.Forms.Button buttonAdd;
    }
}

