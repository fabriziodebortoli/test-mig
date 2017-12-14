namespace Microarea.EasyBuilder.UI
{
	partial class ToolboxControl
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
			this.toolBox = new Microarea.EasyBuilder.UI.ToolBox();
			this.SuspendLayout();
			// 
			// toolBox
			// 
			this.toolBox.Dock = System.Windows.Forms.DockStyle.Fill;
			this.toolBox.Font = new System.Drawing.Font("Verdana", 9F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
			this.toolBox.Location = new System.Drawing.Point(0, 0);
			this.toolBox.Name = "toolBox";
			this.toolBox.Size = new System.Drawing.Size(194, 304);
			this.toolBox.TabIndex = 2;
			// 
			// ToolboxControl
			// 
			this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
			this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
			this.Controls.Add(this.toolBox);
			this.Name = "ToolboxControl";
			this.Size = new System.Drawing.Size(194, 304);
			this.ResumeLayout(false);

		}

		#endregion

		private ToolBox toolBox;
	}
}
