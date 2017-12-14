using System.Drawing;
using System.IO;
using System.Reflection;
using System.Windows.Forms;

namespace Microarea.TaskBuilderNet.Licence.Licence.Forms
{
	//=========================================================================
	public class ErrorViewer : System.Windows.Forms.Form
	{
		private System.Windows.Forms.Label LblInfo;
		private System.Windows.Forms.RichTextBox TxtErrors;
		private System.Windows.Forms.Button BtnOk;
		private System.Windows.Forms.Button BtnCancel;
		private System.Windows.Forms.Label LblAsk;
		private System.ComponentModel.Container components = null;
		private System.Windows.Forms.PictureBox PictureType;
		private bool onlywarnings = false;
		
		//---------------------------------------------------------------------
		public ErrorViewer(string message, bool onlyWarnings)
		{
			InitializeComponent();
			TxtErrors.Text = message;
			onlywarnings = onlyWarnings;
			string nameSpace;
			if (onlywarnings)
			{
				BtnCancel.Visible = true;
				BtnOk.Text = LicenceStrings.Yes;
				LblAsk.Text = LicenceStrings.IgnoreWarning;
				nameSpace = "Microarea.TaskBuilderNet.Licence.Licence.Forms.Images.Warning.gif";
			}
			else
			{
				LblAsk.Text = LicenceStrings.ImpossibleContinue;
				nameSpace = "Microarea.TaskBuilderNet.Licence.Licence.Forms.Images.Error.gif";
			}
			Stream stream = Assembly.GetExecutingAssembly().GetManifestResourceStream(nameSpace);
			if (stream != null)
			{
				Bitmap bitmap		= new Bitmap(stream);
				PictureType.Image = bitmap;
			}

		}

		//---------------------------------------------------------------------
		private void BtnOk_Click(object sender, System.EventArgs e)
		{
			DialogResult = onlywarnings ? DialogResult.OK : DialogResult.No;
			Close();
		}
		
		//---------------------------------------------------------------------
		private void BtnCancel_Click(object sender, System.EventArgs e)
		{
			DialogResult = DialogResult.No;
			Close();
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
			System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(ErrorViewer));
			this.LblInfo = new System.Windows.Forms.Label();
			this.TxtErrors = new System.Windows.Forms.RichTextBox();
			this.BtnOk = new System.Windows.Forms.Button();
			this.BtnCancel = new System.Windows.Forms.Button();
			this.LblAsk = new System.Windows.Forms.Label();
			this.PictureType = new System.Windows.Forms.PictureBox();
			((System.ComponentModel.ISupportInitialize)(this.PictureType)).BeginInit();
			this.SuspendLayout();
			// 
			// LblInfo
			// 
			this.LblInfo.FlatStyle = System.Windows.Forms.FlatStyle.System;
			resources.ApplyResources(this.LblInfo, "LblInfo");
			this.LblInfo.Name = "LblInfo";
			// 
			// TxtErrors
			// 
			this.TxtErrors.BackColor = System.Drawing.Color.Lavender;
			resources.ApplyResources(this.TxtErrors, "TxtErrors");
			this.TxtErrors.Name = "TxtErrors";
			this.TxtErrors.ReadOnly = true;
			// 
			// BtnOk
			// 
			resources.ApplyResources(this.BtnOk, "BtnOk");
			this.BtnOk.Name = "BtnOk";
			this.BtnOk.Click += new System.EventHandler(this.BtnOk_Click);
			// 
			// BtnCancel
			// 
			resources.ApplyResources(this.BtnCancel, "BtnCancel");
			this.BtnCancel.Name = "BtnCancel";
			this.BtnCancel.Click += new System.EventHandler(this.BtnCancel_Click);
			// 
			// LblAsk
			// 
			resources.ApplyResources(this.LblAsk, "LblAsk");
			this.LblAsk.Name = "LblAsk";
			// 
			// PictureType
			// 
			resources.ApplyResources(this.PictureType, "PictureType");
			this.PictureType.Name = "PictureType";
			this.PictureType.TabStop = false;
			// 
			// ErrorViewer
			// 
			resources.ApplyResources(this, "$this");
			this.BackColor = System.Drawing.Color.Lavender;
			this.Controls.Add(this.PictureType);
			this.Controls.Add(this.LblAsk);
			this.Controls.Add(this.BtnCancel);
			this.Controls.Add(this.BtnOk);
			this.Controls.Add(this.TxtErrors);
			this.Controls.Add(this.LblInfo);
			this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedToolWindow;
			this.Name = "ErrorViewer";
			this.ShowInTaskbar = false;
			((System.ComponentModel.ISupportInitialize)(this.PictureType)).EndInit();
			this.ResumeLayout(false);

		}
		#endregion
	}
}
