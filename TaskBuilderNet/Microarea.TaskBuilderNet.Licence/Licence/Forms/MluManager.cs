using System;
using System.Windows.Forms;

namespace Microarea.TaskBuilderNet.Licence.Licence.Forms
{
	/// <summary>
	/// Summary description for MluManager.
	/// </summary>
	//---------------------------------------------------------------------
	public class MluManager : System.Windows.Forms.Form
	{
		private System.Windows.Forms.Button BtnOk;
		private System.Windows.Forms.RadioButton RbMicroarea;
		private System.Windows.Forms.RadioButton RbReseller;
		private System.Windows.Forms.RadioButton RbLater;
		private System.Windows.Forms.Label LblInfo;
		public enum MluManagement  {Microarea, Reseller, Later, None};
		public MluManagement Choice = MluManagement.None;
		private System.Windows.Forms.PictureBox pictureBox1;
		private System.Windows.Forms.Label LblLater;
		private System.Windows.Forms.Label LblReseller;
		private System.Windows.Forms.Label LblMicroarea;
		private System.Windows.Forms.GroupBox GbChoice;
		private PictureBox pictureBox2;
		private PictureBox pictureBox4;
		private PictureBox pictureBox3;
		private System.ComponentModel.IContainer components = null;

		//---------------------------------------------------------------------
		public string ChoiceText 
		{
			get 
			{
				if (Choice == MluManagement.Reseller)
					return RbReseller.Text;
				if (Choice == MluManagement.Later)
					return RbLater.Text;
				if (Choice == MluManagement.Microarea)
					return RbMicroarea.Text;
				return string.Empty;
			}
		}

		//---------------------------------------------------------------------
		public MluManager()
		{
			//
			// Required for Windows Form Designer support
			//
			InitializeComponent();

			//
			// TODO: Add any constructor code after InitializeComponent call
			//
		}

		/// <summary>
		/// Clean up any resources being used.
		/// </summary>
		//---------------------------------------------------------------------
		protected override void Dispose( bool disposing )
		{
			if( disposing )
			{
				if(components != null)
				{
					components.Dispose();
				}
			}
			base.Dispose( disposing );
		}

		#region Windows Form Designer generated code
		/// <summary>
		/// Required method for Designer support - do not modify
		/// the contents of this method with the code editor.
		/// </summary>
		private void InitializeComponent()
		{
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(MluManager));
            this.BtnOk = new System.Windows.Forms.Button();
            this.GbChoice = new System.Windows.Forms.GroupBox();
            this.RbReseller = new System.Windows.Forms.RadioButton();
            this.pictureBox4 = new System.Windows.Forms.PictureBox();
            this.pictureBox3 = new System.Windows.Forms.PictureBox();
            this.pictureBox1 = new System.Windows.Forms.PictureBox();
            this.LblLater = new System.Windows.Forms.Label();
            this.LblReseller = new System.Windows.Forms.Label();
            this.LblMicroarea = new System.Windows.Forms.Label();
            this.RbMicroarea = new System.Windows.Forms.RadioButton();
            this.RbLater = new System.Windows.Forms.RadioButton();
            this.LblInfo = new System.Windows.Forms.Label();
            this.pictureBox2 = new System.Windows.Forms.PictureBox();
            this.GbChoice.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.pictureBox4)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.pictureBox3)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.pictureBox1)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.pictureBox2)).BeginInit();
            this.SuspendLayout();
            // 
            // BtnOk
            // 
            resources.ApplyResources(this.BtnOk, "BtnOk");
            this.BtnOk.Name = "BtnOk";
            this.BtnOk.Click += new System.EventHandler(this.BtnOk_Click);
            // 
            // GbChoice
            // 
            this.GbChoice.Controls.Add(this.RbReseller);
            this.GbChoice.Controls.Add(this.pictureBox4);
            this.GbChoice.Controls.Add(this.pictureBox3);
            this.GbChoice.Controls.Add(this.pictureBox1);
            this.GbChoice.Controls.Add(this.LblLater);
            this.GbChoice.Controls.Add(this.LblReseller);
            this.GbChoice.Controls.Add(this.LblMicroarea);
            this.GbChoice.Controls.Add(this.RbMicroarea);
            this.GbChoice.Controls.Add(this.RbLater);
            this.GbChoice.FlatStyle = System.Windows.Forms.FlatStyle.System;
            resources.ApplyResources(this.GbChoice, "GbChoice");
            this.GbChoice.Name = "GbChoice";
            this.GbChoice.TabStop = false;
            // 
            // RbReseller
            // 
            resources.ApplyResources(this.RbReseller, "RbReseller");
            this.RbReseller.Name = "RbReseller";
            this.RbReseller.CheckedChanged += new System.EventHandler(this.RbReseller_CheckedChanged);
            // 
            // pictureBox4
            // 
            resources.ApplyResources(this.pictureBox4, "pictureBox4");
            this.pictureBox4.Name = "pictureBox4";
            this.pictureBox4.TabStop = false;
            // 
            // pictureBox3
            // 
            resources.ApplyResources(this.pictureBox3, "pictureBox3");
            this.pictureBox3.Name = "pictureBox3";
            this.pictureBox3.TabStop = false;
            // 
            // pictureBox1
            // 
            resources.ApplyResources(this.pictureBox1, "pictureBox1");
            this.pictureBox1.Name = "pictureBox1";
            this.pictureBox1.TabStop = false;
            // 
            // LblLater
            // 
            resources.ApplyResources(this.LblLater, "LblLater");
            this.LblLater.Name = "LblLater";
            // 
            // LblReseller
            // 
            resources.ApplyResources(this.LblReseller, "LblReseller");
            this.LblReseller.Name = "LblReseller";
            // 
            // LblMicroarea
            // 
            resources.ApplyResources(this.LblMicroarea, "LblMicroarea");
            this.LblMicroarea.Name = "LblMicroarea";
            // 
            // RbMicroarea
            // 
            resources.ApplyResources(this.RbMicroarea, "RbMicroarea");
            this.RbMicroarea.Name = "RbMicroarea";
            this.RbMicroarea.CheckedChanged += new System.EventHandler(this.RbMicroarea_CheckedChanged);
            // 
            // RbLater
            // 
            resources.ApplyResources(this.RbLater, "RbLater");
            this.RbLater.Name = "RbLater";
            this.RbLater.CheckedChanged += new System.EventHandler(this.RbLater_CheckedChanged);
            // 
            // LblInfo
            // 
            resources.ApplyResources(this.LblInfo, "LblInfo");
            this.LblInfo.Name = "LblInfo";
            // 
            // pictureBox2
            // 
            resources.ApplyResources(this.pictureBox2, "pictureBox2");
            this.pictureBox2.Name = "pictureBox2";
            this.pictureBox2.TabStop = false;
            // 
            // MluManager
            // 
            this.AcceptButton = this.BtnOk;
            resources.ApplyResources(this, "$this");
            this.BackColor = System.Drawing.Color.Lavender;
            this.ControlBox = false;
            this.Controls.Add(this.pictureBox2);
            this.Controls.Add(this.BtnOk);
            this.Controls.Add(this.GbChoice);
            this.Controls.Add(this.LblInfo);
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedToolWindow;
            this.MinimizeBox = false;
            this.Name = "MluManager";
            this.ShowInTaskbar = false;
            this.TopMost = true;
            this.Closing += new System.ComponentModel.CancelEventHandler(this.MluManager_Closing);
            this.GbChoice.ResumeLayout(false);
            this.GbChoice.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)(this.pictureBox4)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.pictureBox3)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.pictureBox1)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.pictureBox2)).EndInit();
            this.ResumeLayout(false);

		}
		#endregion

		//---------------------------------------------------------------------
		private void RbMicroarea_CheckedChanged(object sender, System.EventArgs e)
		{
			BtnOk.Enabled = (RbLater.Checked || RbMicroarea.Checked || RbReseller.Checked);
			Choice = MluManagement.Microarea;
		}

		//---------------------------------------------------------------------
		private void RbReseller_CheckedChanged(object sender, System.EventArgs e)
		{
			BtnOk.Enabled = (RbLater.Checked || RbMicroarea.Checked || RbReseller.Checked);
			Choice = MluManagement.Reseller;
		}

		//---------------------------------------------------------------------
		private void RbLater_CheckedChanged(object sender, System.EventArgs e)
		{
			BtnOk.Enabled = (RbLater.Checked || RbMicroarea.Checked || RbReseller.Checked);
			Choice = MluManagement.Later;
		}

		//---------------------------------------------------------------------
		private void BtnOk_Click(object sender, System.EventArgs e)
		{
			DialogResult r = MessageBox.Show(this, String.Format(LicenceStrings.MLUAcceptanceConfirm, ChoiceText), LicenceStrings.MLUAcceptance, MessageBoxButtons.YesNo, MessageBoxIcon.Question, MessageBoxDefaultButton.Button2);
			if (r == DialogResult.Yes)
				DialogResult = DialogResult.OK;
			else
				DialogResult = DialogResult.None;
			Close();

		}

		//---------------------------------------------------------------------
		private void MluManager_Closing(object sender, System.ComponentModel.CancelEventArgs e)
		{
			e.Cancel = (DialogResult != DialogResult.OK);
				
		}

	}
}
