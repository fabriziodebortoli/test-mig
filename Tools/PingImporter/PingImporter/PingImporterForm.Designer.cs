namespace Microarea.Internals.PingImporter
{
	partial class PingImporterForm
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
			if (disposing)
			{
				if (token != null)
				{
					token.Dispose();
					token = null;
				}
				if (components != null)
				{
					components.Dispose();
				}
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
			System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(PingImporterForm));
			this.btnFileSelection = new System.Windows.Forms.Button();
			this.btnImport = new System.Windows.Forms.Button();
			this.txtUrl = new System.Windows.Forms.TextBox();
			this.txtOutput = new System.Windows.Forms.TextBox();
			this.lblPingRate = new System.Windows.Forms.Label();
			this.txtPingRate = new System.Windows.Forms.TextBox();
			this.btnStopImport = new System.Windows.Forms.Button();
			this.progressTasksCounter = new System.Windows.Forms.ProgressBar();
			this.txtPingsPath = new System.Windows.Forms.TextBox();
			this.lblServerUrl = new System.Windows.Forms.Label();
			this.ckbMaxRate = new System.Windows.Forms.CheckBox();
			this.label1 = new System.Windows.Forms.Label();
			this.menuStrip1 = new System.Windows.Forms.MenuStrip();
			this.informazioniToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
			this.menuStrip1.SuspendLayout();
			this.SuspendLayout();
			// 
			// btnFileSelection
			// 
			resources.ApplyResources(this.btnFileSelection, "btnFileSelection");
			this.btnFileSelection.Name = "btnFileSelection";
			this.btnFileSelection.UseVisualStyleBackColor = true;
			this.btnFileSelection.Click += new System.EventHandler(this.FileSelectionButton_Click);
			// 
			// btnImport
			// 
			resources.ApplyResources(this.btnImport, "btnImport");
			this.btnImport.Name = "btnImport";
			this.btnImport.UseVisualStyleBackColor = true;
			this.btnImport.Click += new System.EventHandler(this.ImportButton_Click);
			// 
			// txtUrl
			// 
			resources.ApplyResources(this.txtUrl, "txtUrl");
			this.txtUrl.Name = "txtUrl";
			// 
			// txtOutput
			// 
			resources.ApplyResources(this.txtOutput, "txtOutput");
			this.txtOutput.Name = "txtOutput";
			this.txtOutput.ReadOnly = true;
			// 
			// lblPingRate
			// 
			resources.ApplyResources(this.lblPingRate, "lblPingRate");
			this.lblPingRate.Name = "lblPingRate";
			// 
			// txtPingRate
			// 
			resources.ApplyResources(this.txtPingRate, "txtPingRate");
			this.txtPingRate.Name = "txtPingRate";
			// 
			// btnStopImport
			// 
			resources.ApplyResources(this.btnStopImport, "btnStopImport");
			this.btnStopImport.Name = "btnStopImport";
			this.btnStopImport.UseVisualStyleBackColor = true;
			this.btnStopImport.Click += new System.EventHandler(this.btnStopImport_Click);
			// 
			// progressTasksCounter
			// 
			resources.ApplyResources(this.progressTasksCounter, "progressTasksCounter");
			this.progressTasksCounter.Name = "progressTasksCounter";
			this.progressTasksCounter.Step = 1;
			// 
			// txtPingsPath
			// 
			resources.ApplyResources(this.txtPingsPath, "txtPingsPath");
			this.txtPingsPath.Name = "txtPingsPath";
			this.txtPingsPath.Leave += new System.EventHandler(this.txtPingsPath_Leave);
			// 
			// lblServerUrl
			// 
			resources.ApplyResources(this.lblServerUrl, "lblServerUrl");
			this.lblServerUrl.Name = "lblServerUrl";
			// 
			// ckbMaxRate
			// 
			resources.ApplyResources(this.ckbMaxRate, "ckbMaxRate");
			this.ckbMaxRate.Name = "ckbMaxRate";
			this.ckbMaxRate.UseVisualStyleBackColor = true;
			this.ckbMaxRate.CheckedChanged += new System.EventHandler(this.ckbMaxRate_CheckedChanged);
			// 
			// label1
			// 
			resources.ApplyResources(this.label1, "label1");
			this.label1.Name = "label1";
			// 
			// menuStrip1
			// 
			this.menuStrip1.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.informazioniToolStripMenuItem});
			resources.ApplyResources(this.menuStrip1, "menuStrip1");
			this.menuStrip1.Name = "menuStrip1";
			// 
			// informazioniToolStripMenuItem
			// 
			this.informazioniToolStripMenuItem.Name = "informazioniToolStripMenuItem";
			resources.ApplyResources(this.informazioniToolStripMenuItem, "informazioniToolStripMenuItem");
			this.informazioniToolStripMenuItem.Click += new System.EventHandler(this.informazioniToolStripMenuItem_Click);
			// 
			// PingImporterForm
			// 
			resources.ApplyResources(this, "$this");
			this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
			this.Controls.Add(this.ckbMaxRate);
			this.Controls.Add(this.txtPingsPath);
			this.Controls.Add(this.progressTasksCounter);
			this.Controls.Add(this.lblServerUrl);
			this.Controls.Add(this.label1);
			this.Controls.Add(this.lblPingRate);
			this.Controls.Add(this.txtOutput);
			this.Controls.Add(this.txtPingRate);
			this.Controls.Add(this.txtUrl);
			this.Controls.Add(this.btnStopImport);
			this.Controls.Add(this.btnImport);
			this.Controls.Add(this.btnFileSelection);
			this.Controls.Add(this.menuStrip1);
			this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedToolWindow;
			this.MainMenuStrip = this.menuStrip1;
			this.MaximizeBox = false;
			this.MinimizeBox = false;
			this.Name = "PingImporterForm";
			this.menuStrip1.ResumeLayout(false);
			this.menuStrip1.PerformLayout();
			this.ResumeLayout(false);
			this.PerformLayout();

		}

		#endregion

		private System.Windows.Forms.Button btnFileSelection;
		private System.Windows.Forms.Button btnImport;
		private System.Windows.Forms.TextBox txtUrl;
		private System.Windows.Forms.TextBox txtOutput;
		private System.Windows.Forms.Label lblPingRate;
		private System.Windows.Forms.TextBox txtPingRate;
		private System.Windows.Forms.Button btnStopImport;
		private System.Windows.Forms.ProgressBar progressTasksCounter;
		private System.Windows.Forms.TextBox txtPingsPath;
		private System.Windows.Forms.Label lblServerUrl;
		private System.Windows.Forms.CheckBox ckbMaxRate;
		private System.Windows.Forms.Label label1;
		private System.Windows.Forms.MenuStrip menuStrip1;
		private System.Windows.Forms.ToolStripMenuItem informazioniToolStripMenuItem;
	}
}

