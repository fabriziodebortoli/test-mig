namespace SampleApp
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
            this.panel1 = new System.Windows.Forms.Panel();
            this.simpleExample = new SampleApp.SimpleExample();
            this.panel1.SuspendLayout();
            this.SuspendLayout();
            // 
            // panel1
            // 
            this.panel1.BorderStyle = System.Windows.Forms.BorderStyle.Fixed3D;
            this.panel1.Controls.Add(this.simpleExample);
            this.panel1.Dock = System.Windows.Forms.DockStyle.Fill;
            this.panel1.Location = new System.Drawing.Point(0, 0);
            this.panel1.Name = "panel1";
            this.panel1.Size = new System.Drawing.Size(653, 524);
            this.panel1.TabIndex = 0;
            // 
            // simpleExample
            // 
            this.simpleExample.Dock = System.Windows.Forms.DockStyle.Fill;
            this.simpleExample.Location = new System.Drawing.Point(0, 0);
            this.simpleExample.Name = "simpleExample";
            this.simpleExample.Size = new System.Drawing.Size(649, 520);
            this.simpleExample.TabIndex = 0;
            this.simpleExample.Load += new System.EventHandler(this.simpleExample_Load);
            // 
            // MainForm
            // 
            this.ClientSize = new System.Drawing.Size(653, 524);
            this.Controls.Add(this.panel1);
            this.Name = "MainForm";
            this.Load += new System.EventHandler(this.MainForm_Load);
            this.panel1.ResumeLayout(false);
            this.ResumeLayout(false);

		}

		#endregion

        private System.Windows.Forms.Panel panel1;
        private SimpleExample simpleExample;
      
		
	}
}

