namespace EADesigner {
	partial class EAConnectionDesigner {
		/// <summary> 
		/// Required designer variable.
		/// </summary>
		private System.ComponentModel.IContainer components = null;

		/// <summary> 
		/// Clean up any resources being used.
		/// </summary>
		/// <param name="disposing">true if managed resources should be disposed; otherwise, false.</param>
		protected override void Dispose(bool disposing) {
			if (disposing && (components != null)) {
				components.Dispose();
			}
			base.Dispose(disposing);
		}

		#region Component Designer generated code

		/// <summary> 
		/// Required method for Designer support - do not modify 
		/// the contents of this method with the code editor.
		/// </summary>
		private void InitializeComponent() {
            this.ServerLbl = new System.Windows.Forms.Label();
            this.ServerTxt = new System.Windows.Forms.TextBox();
            this.InstallationNameLbl = new System.Windows.Forms.Label();
            this.InstallationNameTxt = new System.Windows.Forms.TextBox();
            this.SuspendLayout();
            // 
            // ServerLbl
            // 
            this.ServerLbl.AutoSize = true;
            this.ServerLbl.Location = new System.Drawing.Point(19, 17);
            this.ServerLbl.Name = "ServerLbl";
            this.ServerLbl.Size = new System.Drawing.Size(41, 13);
            this.ServerLbl.TabIndex = 2;
            this.ServerLbl.Text = "Server:";
            // 
            // ServerTxt
            // 
            this.ServerTxt.Location = new System.Drawing.Point(158, 14);
            this.ServerTxt.Name = "ServerTxt";
            this.ServerTxt.Size = new System.Drawing.Size(224, 20);
            this.ServerTxt.TabIndex = 3;
            // 
            // InstallationNameLbl
            // 
            this.InstallationNameLbl.AutoSize = true;
            this.InstallationNameLbl.Location = new System.Drawing.Point(19, 39);
            this.InstallationNameLbl.Name = "InstallationNameLbl";
            this.InstallationNameLbl.Size = new System.Drawing.Size(91, 13);
            this.InstallationNameLbl.TabIndex = 4;
            this.InstallationNameLbl.Text = "Installation Name:";
            // 
            // InstallationNameTxt
            // 
            this.InstallationNameTxt.Location = new System.Drawing.Point(158, 36);
            this.InstallationNameTxt.Name = "InstallationNameTxt";
            this.InstallationNameTxt.Size = new System.Drawing.Size(224, 20);
            this.InstallationNameTxt.TabIndex = 5;
            // 
            // EAConnectionDesigner
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.Controls.Add(this.InstallationNameTxt);
            this.Controls.Add(this.InstallationNameLbl);
            this.Controls.Add(this.ServerTxt);
            this.Controls.Add(this.ServerLbl);
            this.Name = "EAConnectionDesigner";
            this.Size = new System.Drawing.Size(410, 66);
            this.ResumeLayout(false);
            this.PerformLayout();

		}

		#endregion

        private System.Windows.Forms.Label ServerLbl;
        private System.Windows.Forms.TextBox ServerTxt;
        private System.Windows.Forms.Label InstallationNameLbl;
        private System.Windows.Forms.TextBox InstallationNameTxt;
	}
}
