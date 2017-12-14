namespace Microarea.TaskBuilderNet.Forms.Containers
{
	partial class UIDynamicRowView
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
			this.dynamicView = new UIDynamicView();
			this.SuspendLayout();
			// 
			// button1
			// 
			this.dynamicView.Dock = System.Windows.Forms.DockStyle.Fill;
			this.dynamicView.Location = new System.Drawing.Point(0, 0);
			this.dynamicView.Name = "button1";
			this.dynamicView.Size = new System.Drawing.Size(284, 262);
			this.dynamicView.TabIndex = 0;
			this.dynamicView.Text = "button1";
			// 
			// UIDynamicRowView
			// 
			this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
			this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
			this.ClientSize = new System.Drawing.Size(284, 262);
			this.Controls.Add(this.dynamicView);
			this.Name = "UIDynamicRowView";
			this.Text = "UIDynamicRowView";
			this.ResumeLayout(false);

		}

		#endregion

		private UIDynamicView dynamicView;
	}
}