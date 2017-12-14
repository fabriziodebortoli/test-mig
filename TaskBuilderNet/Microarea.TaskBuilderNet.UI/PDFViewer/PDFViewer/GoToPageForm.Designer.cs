namespace Microarea.TaskBuilderNet.UI.PDFViewer
{
	partial class GoToPageForm
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
			this.label1 = new System.Windows.Forms.Label();
			this.pageNumberTextBox = new System.Windows.Forms.TextBox();
			this.goButton = new System.Windows.Forms.Button();
			this.SuspendLayout();
			// 
			// label1
			// 
			this.label1.AutoSize = true;
			this.label1.Font = new System.Drawing.Font("Microsoft Sans Serif", 9F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
			this.label1.Location = new System.Drawing.Point(8, 22);
			this.label1.Name = "label1";
			this.label1.Size = new System.Drawing.Size(78, 15);
			this.label1.TabIndex = 0;
			this.label1.Text = "Go To Page: ";
			// 
			// pageNumberTextBox
			// 
			this.pageNumberTextBox.Location = new System.Drawing.Point(106, 17);
			this.pageNumberTextBox.Name = "pageNumberTextBox";
			this.pageNumberTextBox.Size = new System.Drawing.Size(79, 20);
			this.pageNumberTextBox.TabIndex = 1;
			this.pageNumberTextBox.KeyPress += new System.Windows.Forms.KeyPressEventHandler(this.pageNumberTextBox_KeyPress);
			this.pageNumberTextBox.Validating += new System.ComponentModel.CancelEventHandler(this.pageNumberTextBox_Validating);
			// 
			// goButton
			// 
			this.goButton.Location = new System.Drawing.Point(110, 57);
			this.goButton.Name = "goButton";
			this.goButton.Size = new System.Drawing.Size(75, 23);
			this.goButton.TabIndex = 2;
			this.goButton.Text = "Go";
			this.goButton.UseVisualStyleBackColor = true;
			this.goButton.Click += new System.EventHandler(this.goButton_Click);
			// 
			// GoToPageForm
			// 
			this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
			this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
			this.AutoScroll = true;
			this.ClientSize = new System.Drawing.Size(197, 92);
			this.Controls.Add(this.goButton);
			this.Controls.Add(this.pageNumberTextBox);
			this.Controls.Add(this.label1);
			this.MaximizeBox = false;
			this.MinimizeBox = false;
			this.Name = "GoToPageForm";
			this.StartPosition = System.Windows.Forms.FormStartPosition.CenterScreen;
			this.Text = "Go to page";
			this.ResumeLayout(false);
			this.PerformLayout();

		}

		#endregion

		private System.Windows.Forms.Label label1;
		private System.Windows.Forms.TextBox pageNumberTextBox;
		private System.Windows.Forms.Button goButton;
	}
}