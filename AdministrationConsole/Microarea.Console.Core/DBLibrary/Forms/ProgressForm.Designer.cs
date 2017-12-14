namespace Microarea.Console.Core.DBLibrary.Forms
{
	partial class ProgressForm
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
			System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(ProgressForm));
			this._CircularProgressBar = new Microarea.TaskBuilderNet.UI.WinControls.CircularProgressBar();
			this.LblElab = new System.Windows.Forms.Label();
			this.SuspendLayout();
			// 
			// _CircularProgressBar
			// 
			this._CircularProgressBar.ActiveSegmentColour = System.Drawing.SystemColors.Highlight;
			this._CircularProgressBar.AutoIncrementFrequency = 100D;
			this._CircularProgressBar.BehindTransistionSegmentIsActive = false;
			resources.ApplyResources(this._CircularProgressBar, "_CircularProgressBar");
			this._CircularProgressBar.Name = "_CircularProgressBar";
			this._CircularProgressBar.TransistionSegment = 1;
			this._CircularProgressBar.TransistionSegmentColour = System.Drawing.SystemColors.ActiveCaption;
			// 
			// LblElab
			// 
			resources.ApplyResources(this.LblElab, "LblElab");
			this.LblElab.Name = "LblElab";
			// 
			// ProgressForm
			// 
			resources.ApplyResources(this, "$this");
			this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
			this.Controls.Add(this.LblElab);
			this.Controls.Add(this._CircularProgressBar);
			this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedToolWindow;
			this.MaximizeBox = false;
			this.MinimizeBox = false;
			this.Name = "ProgressForm";
			this.ShowIcon = false;
			this.ShowInTaskbar = false;
			this.FormClosing += new System.Windows.Forms.FormClosingEventHandler(this.ProgressForm_FormClosing);
			this.Load += new System.EventHandler(this.ProgressForm_Load);
			this.ResumeLayout(false);
			this.PerformLayout();

		}

		#endregion
		private TaskBuilderNet.UI.WinControls.CircularProgressBar _CircularProgressBar;
		private System.Windows.Forms.Label LblElab;
	}
}