namespace Microarea.EasyAttachment.UI.Controls
{
	partial class MassiveAttachDetailsListItem
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
			this.documentName = new System.Windows.Forms.Label();
			this.keyDescription = new System.Windows.Forms.Label();
			this.OpenMagoDocBtn = new System.Windows.Forms.Button();
			this.resultLabel = new System.Windows.Forms.Label();
			this.showErrorBtn = new System.Windows.Forms.Button();
			this.imageBtn = new System.Windows.Forms.Button();
			this.SuspendLayout();
			// 
			// documentName
			// 
			this.documentName.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left)
						| System.Windows.Forms.AnchorStyles.Right)));
			this.documentName.BackColor = System.Drawing.SystemColors.GradientInactiveCaption;
			this.documentName.Font = new System.Drawing.Font("Verdana", 8.25F, System.Drawing.FontStyle.Bold);
			this.documentName.ImeMode = System.Windows.Forms.ImeMode.NoControl;
			this.documentName.Location = new System.Drawing.Point(39, 3);
			this.documentName.Name = "documentName";
			this.documentName.Size = new System.Drawing.Size(186, 14);
			this.documentName.TabIndex = 8;
			this.documentName.Text = "<documentTitle>\r\n";
			// 
			// keyDescription
			// 
			this.keyDescription.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left)
						| System.Windows.Forms.AnchorStyles.Right)));
			this.keyDescription.Font = new System.Drawing.Font("Verdana", 6.75F);
			this.keyDescription.ImeMode = System.Windows.Forms.ImeMode.NoControl;
			this.keyDescription.Location = new System.Drawing.Point(40, 21);
			this.keyDescription.Name = "keyDescription";
			this.keyDescription.Size = new System.Drawing.Size(186, 14);
			this.keyDescription.TabIndex = 9;
			this.keyDescription.Text = "<keyDescription>";
			// 
			// OpenMagoDocBtn
			// 
			this.OpenMagoDocBtn.Image = global::Microarea.EasyAttachment.Properties.Resources.DataEntry32x32;
			this.OpenMagoDocBtn.ImeMode = System.Windows.Forms.ImeMode.NoControl;
			this.OpenMagoDocBtn.Location = new System.Drawing.Point(6, 3);
			this.OpenMagoDocBtn.Name = "OpenMagoDocBtn";
			this.OpenMagoDocBtn.Size = new System.Drawing.Size(32, 32);
			this.OpenMagoDocBtn.TabIndex = 12;
			this.OpenMagoDocBtn.UseVisualStyleBackColor = true;
			this.OpenMagoDocBtn.Click += new System.EventHandler(this.OpenMagoDocBtn_Click);
			// 
			// resultLabel
			// 
			this.resultLabel.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left)
						| System.Windows.Forms.AnchorStyles.Right)));
			this.resultLabel.AutoSize = true;
			this.resultLabel.Font = new System.Drawing.Font("Verdana", 6.75F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
			this.resultLabel.Location = new System.Drawing.Point(40, 39);
			this.resultLabel.Name = "resultLabel";
			this.resultLabel.Size = new System.Drawing.Size(174, 12);
			this.resultLabel.TabIndex = 14;
			this.resultLabel.Text = "Attachment successfully created!";
			// 
			// showErrorBtn
			// 
			this.showErrorBtn.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
			this.showErrorBtn.Image = global::Microarea.EasyAttachment.Properties.Resources.Help;
			this.showErrorBtn.Location = new System.Drawing.Point(233, 10);
			this.showErrorBtn.Name = "showErrorBtn";
			this.showErrorBtn.Size = new System.Drawing.Size(30, 30);
			this.showErrorBtn.TabIndex = 15;
			this.showErrorBtn.UseVisualStyleBackColor = true;
			this.showErrorBtn.Visible = false;
			this.showErrorBtn.Click += new System.EventHandler(this.showErrorBtn_Click);
			// 
			// imageBtn
			// 
			this.imageBtn.Image = global::Microarea.EasyAttachment.Properties.Resources.OK;
			this.imageBtn.Location = new System.Drawing.Point(6, 23);
			this.imageBtn.Name = "imageBtn";
			this.imageBtn.Size = new System.Drawing.Size(22, 22);
			this.imageBtn.TabIndex = 16;
			this.imageBtn.UseVisualStyleBackColor = true;
			this.imageBtn.Click += new System.EventHandler(this.imageBtn_Click);
			// 
			// MassiveAttachDetailsListItem
			// 
			this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
			this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
			this.BackColor = System.Drawing.SystemColors.GradientInactiveCaption;
			this.Controls.Add(this.imageBtn);
			this.Controls.Add(this.showErrorBtn);
			this.Controls.Add(this.resultLabel);
			this.Controls.Add(this.OpenMagoDocBtn);
			this.Controls.Add(this.keyDescription);
			this.Controls.Add(this.documentName);
			this.Name = "MassiveAttachDetailsListItem";
			this.Size = new System.Drawing.Size(265, 57);
			this.ResumeLayout(false);
			this.PerformLayout();

		}

		#endregion

		private System.Windows.Forms.Label documentName;
		private System.Windows.Forms.Label keyDescription;
        private System.Windows.Forms.Button OpenMagoDocBtn;
		private System.Windows.Forms.Label resultLabel;
		private System.Windows.Forms.Button showErrorBtn;
		private System.Windows.Forms.Button imageBtn;
	}
}