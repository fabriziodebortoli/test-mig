

namespace Microarea.Console.Core.TaskSchedulerWindowsControls
{
	public partial class TaskBuilderSchedulerPanel 
	{
		#region Windows Form Designer generated code
		private void InitializeComponent()
		{
			this.components = new System.ComponentModel.Container();
			System.Resources.ResourceManager resources = new System.Resources.ResourceManager(typeof(TaskBuilderSchedulerPanel));
			this.TitleLabel = new System.Windows.Forms.Label();
			this.PanelImageList = new System.Windows.Forms.ImageList(this.components);
			this.SuspendLayout();
			// 
			// TitleLabel
			// 
			this.TitleLabel.Cursor = System.Windows.Forms.Cursors.Default;
			this.TitleLabel.Dock = System.Windows.Forms.DockStyle.Top;
			this.TitleLabel.ForeColor = System.Drawing.Color.Navy;
			this.TitleLabel.Location = new System.Drawing.Point(241, 0);
			this.TitleLabel.Name = "TitleLabel";
			this.TitleLabel.Size = new System.Drawing.Size(200, 24);
			this.TitleLabel.TabIndex = 0;
			this.TitleLabel.Text = "Title";
			this.TitleLabel.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
			this.TitleLabel.Paint += new System.Windows.Forms.PaintEventHandler(this.TitleLabel_Paint);
			this.TitleLabel.MouseUp += new System.Windows.Forms.MouseEventHandler(this.TitleLabel_MouseUp);
			this.TitleLabel.MouseMove += new System.Windows.Forms.MouseEventHandler(this.TitleLabel_MouseMove);
			// 
			// PanelImageList
			// 
			this.PanelImageList.ColorDepth = System.Windows.Forms.ColorDepth.Depth32Bit;
			this.PanelImageList.ImageSize = new System.Drawing.Size(16, 16);
			this.PanelImageList.ImageStream = ((System.Windows.Forms.ImageListStreamer)(resources.GetObject("PanelImageList.ImageStream")));
			this.PanelImageList.TransparentColor = System.Drawing.Color.White;
			// 
			// TaskBuilderSchedulerPanel
			// 
			this.Controls.Add(this.TitleLabel);
			this.ResumeLayout(false);

		}
		#endregion

		private System.ComponentModel.IContainer components;
		private System.Windows.Forms.Label TitleLabel;
		private System.Windows.Forms.ImageList PanelImageList;
	}
}