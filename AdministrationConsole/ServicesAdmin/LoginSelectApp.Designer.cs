namespace Microarea.Console.Plugin.ServicesAdmin
{
	partial class LoginSelectApp
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
            this.AppListBox = new System.Windows.Forms.ListBox();
            this.OKButton = new System.Windows.Forms.Button();
            this.CancButton = new System.Windows.Forms.Button();
            this.SuspendLayout();
            // 
            // AppListBox
            // 
            this.AppListBox.FormattingEnabled = true;
            this.AppListBox.ItemHeight = 16;
            this.AppListBox.Location = new System.Drawing.Point(12, 12);
            this.AppListBox.Name = "AppListBox";
            this.AppListBox.SelectionMode = System.Windows.Forms.SelectionMode.MultiSimple;
            this.AppListBox.Size = new System.Drawing.Size(204, 100);
            this.AppListBox.TabIndex = 0;
            // 
            // OKButton
            // 
            this.OKButton.FlatStyle = System.Windows.Forms.FlatStyle.System;
            this.OKButton.Location = new System.Drawing.Point(32, 117);
            this.OKButton.Name = "OKButton";
            this.OKButton.Size = new System.Drawing.Size(75, 23);
            this.OKButton.TabIndex = 1;
            this.OKButton.Text = "OK";
            this.OKButton.UseVisualStyleBackColor = true;
            this.OKButton.Click += new System.EventHandler(this.OKButton_Click);
            // 
            // CancButton
            // 
            this.CancButton.Location = new System.Drawing.Point(123, 117);
            this.CancButton.Name = "CancButton";
            this.CancButton.Size = new System.Drawing.Size(75, 23);
            this.CancButton.TabIndex = 2;
            this.CancButton.Text = "Cancel";
            this.CancButton.UseVisualStyleBackColor = true;
            this.CancButton.Click += new System.EventHandler(this.CancelButton_Click);
            // 
            // LoginSelectApp
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(8F, 16F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(231, 147);
            this.Controls.Add(this.CancButton);
            this.Controls.Add(this.OKButton);
            this.Controls.Add(this.AppListBox);
            this.Font = new System.Drawing.Font("Verdana", 9.75F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedToolWindow;
            this.Margin = new System.Windows.Forms.Padding(4);
            this.Name = "LoginSelectApp";
            this.ShowIcon = false;
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterParent;
            this.Text = "Select Applications";
            this.ResumeLayout(false);

		}

		#endregion

		private System.Windows.Forms.ListBox AppListBox;
		private System.Windows.Forms.Button OKButton;
		private System.Windows.Forms.Button CancButton;
	}
}