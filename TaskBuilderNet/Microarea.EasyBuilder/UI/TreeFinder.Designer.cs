namespace Microarea.EasyBuilder.UI
{
	partial class TreeFinder
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
			this.toolStripLabel1 = new System.Windows.Forms.ToolStripLabel();
			this.toolStripSeparator1 = new System.Windows.Forms.ToolStripSeparator();
			this.tsClose = new System.Windows.Forms.ToolStripButton();
			this.tsPreviousFind = new System.Windows.Forms.ToolStripButton();
			this.tsNextFind = new System.Windows.Forms.ToolStripButton();
			this.tsText = new System.Windows.Forms.ToolStripTextBox();
			this.tslblMatched = new System.Windows.Forms.ToolStripLabel();
			this.SuspendLayout();
			// 
			// toolStripLabel1
			// 
			this.toolStripLabel1.Name = "toolStripLabel1";
			this.toolStripLabel1.Size = new System.Drawing.Size(30, 28);
			this.toolStripLabel1.Text = "Find";
			this.toolStripLabel1.ToolTipText = "Find (Ctrl+F)";
			// 
			// toolStripSeparator1
			// 
			this.toolStripSeparator1.Name = "toolStripSeparator1";
			this.toolStripSeparator1.Size = new System.Drawing.Size(6, 31);
			// 
			// tsClose
			// 
			this.tsClose.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Image;
			this.tsClose.Image = global::Microarea.EasyBuilder.Properties.Resources.Close;
			this.tsClose.ImageTransparentColor = System.Drawing.Color.White;
			this.tsClose.Name = "tsClose";
			this.tsClose.Size = new System.Drawing.Size(23, 28);
			this.tsClose.Text = "X";
			this.tsClose.ToolTipText = "Close";
			this.tsClose.Click += new System.EventHandler(this.tsClose_Click);
			// 
			// tsPreviousFind
			// 
			this.tsPreviousFind.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Image;
			this.tsPreviousFind.Image = global::Microarea.EasyBuilder.Properties.Resources.Previous;
			this.tsPreviousFind.ImageTransparentColor = System.Drawing.Color.Magenta;
			this.tsPreviousFind.Name = "tsPreviousFind";
			this.tsPreviousFind.Size = new System.Drawing.Size(23, 28);
			this.tsPreviousFind.Text = "<<";
			this.tsPreviousFind.ToolTipText = "Previous (Shift + F3)";
			this.tsPreviousFind.Click += new System.EventHandler(this.tsPreviousFind_Click);
			// 
			// tsNextFind
			// 
			this.tsNextFind.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Image;
			this.tsNextFind.Image = global::Microarea.EasyBuilder.Properties.Resources.Next;
			this.tsNextFind.ImageTransparentColor = System.Drawing.Color.Magenta;
			this.tsNextFind.Name = "tsNextFind";
			this.tsNextFind.Size = new System.Drawing.Size(23, 28);
			this.tsNextFind.Text = ">>";
			this.tsNextFind.ToolTipText = "Next (F3)";
			this.tsNextFind.Click += new System.EventHandler(this.tsNextFind_Click);
			// 
			// tsText
			// 
			this.tsText.Name = "tsText";
			this.tsText.Size = new System.Drawing.Size(130, 31);
			this.tsText.KeyDown += new System.Windows.Forms.KeyEventHandler(this.tsText_KeyDown);
			// 
			// tslblMatched
			// 
			this.tslblMatched.Name = "tslblMatched";
			this.tslblMatched.Size = new System.Drawing.Size(0, 28);
			// 
			// TreeFinder
			// 
			this.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.toolStripLabel1,
            this.tsText,
            this.tsNextFind,
            this.tsPreviousFind,
            this.toolStripSeparator1,
            this.tsClose,
            this.tslblMatched});
			this.Size = new System.Drawing.Size(275, 31);
			this.ResumeLayout(false);

		}

		#endregion

		private System.Windows.Forms.ToolStripLabel toolStripLabel1;
		private System.Windows.Forms.ToolStripSeparator toolStripSeparator1;
		private System.Windows.Forms.ToolStripButton tsClose;
		private System.Windows.Forms.ToolStripButton tsPreviousFind;
		private System.Windows.Forms.ToolStripButton tsNextFind;
		private System.Windows.Forms.ToolStripTextBox tsText;

		public System.Windows.Forms.ToolStripTextBox TsText
		{
			get { return tsText; }
			set { tsText = value; }
		}
		private System.Windows.Forms.ToolStripLabel tslblMatched;
	}
}
