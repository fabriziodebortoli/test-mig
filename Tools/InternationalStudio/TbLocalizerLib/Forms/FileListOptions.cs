using System;
using System.Drawing;
using System.Drawing.Printing;
using System.IO;
using System.Windows.Forms;

namespace Microarea.Tools.TBLocalizer.Forms
{
	/// <summary>
	/// Summary description for FileListOptions.
	/// </summary>
	//=========================================================================
	public class FileListOptions : System.Windows.Forms.Form
	{
		private System.Windows.Forms.Label LblInfo;
		private System.Windows.Forms.Button BtnOk;
		private System.Windows.Forms.Button BtnOpenFile;
		private System.Windows.Forms.Button BtnPrint;
		private System.ComponentModel.Container components = null;
		private string filePath = String.Empty;
		private StreamReader	streamToPrint;
		private Font printFont;
		public enum ModeType {FileList, TranslationTable, None};
		public ModeType Mode = ModeType.None;

		//---------------------------------------------------------------------
		public FileListOptions(string filePath, string node,  ModeType mode)
		{
			InitializeComponent();
			this.filePath = filePath;
			this.Mode = mode;
			switch (Mode)
			{
			
				case ModeType.FileList:
					LblInfo.Text  = String.Format(Strings.FileListOptions, filePath, node);
					break;
				case ModeType.TranslationTable:
					BtnPrint.Visible = false;
					LblInfo.Text  = String.Format(Strings.TranslationTableOptions, filePath, node);
					break;
			}
			
		}

		//---------------------------------------------------------------------
		private void BtnOk_Click(object sender, System.EventArgs e)
		{
			Close();
		}

		//---------------------------------------------------------------------
		private void BtnOpenFile_Click(object sender, System.EventArgs e)
		{
			Close();
			System.Diagnostics.Process.Start(filePath);
		}

		//---------------------------------------------------------------------
		private void BtnPrint_Click(object sender, System.EventArgs e)
		{
			Close();
			try 
			{
			streamToPrint = new StreamReader(filePath);
					
				try 
				{
					printFont			= new Font("Arial", 10);
					PrintDocument pd	= new PrintDocument();
					pd.PrintPage		+= new PrintPageEventHandler(this.pd_PrintPage);
					pd.DefaultPageSettings.Landscape = true;
					pd.Print();
				}  
				finally 
				{
					streamToPrint.Close();
				}
			}  
			catch (Exception ex) 
			{
				MessageBox.Show(DictionaryCreator.ActiveForm, ex.Message);
			}
			
		}

		// The PrintPage event is raised for each page to be printed.
		//---------------------------------------------------------------------
		private void pd_PrintPage(object sender, PrintPageEventArgs e) 
		{
			float	linesPerPage	= 0;
			float	yPos			= 0;
			int		count			= 0;
			float	leftMargin		= e.MarginBounds.Left;
			float	topMargin		= e.MarginBounds.Top;
			string	line			= null;

			// Calculate the number of lines per page.
			linesPerPage = e.MarginBounds.Height / printFont.GetHeight(e.Graphics);

			// Print each line of the file.
			while(
					count < linesPerPage && 
					((line = streamToPrint.ReadLine()) != null)
				 ) 
			{
				yPos = topMargin + (count * printFont.GetHeight(e.Graphics));
					
				e.Graphics.DrawString
					(
						line, 
						printFont,
						Brushes.Black, 
						leftMargin, 
						yPos, 
						new StringFormat()
					);
				count++;
			}

			// If more lines exist, print another page.
			if (line != null)
				e.HasMorePages = true;
			else
				e.HasMorePages = false;
			
		}

		/// <summary>
		/// Clean up any resources being used.
		/// </summary>
		//---------------------------------------------------------------------
		protected override void Dispose( bool disposing )
		{
			if ( disposing )
			{
				if (components != null)
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
			System.Resources.ResourceManager resources = new System.Resources.ResourceManager(typeof(FileListOptions));
			this.LblInfo = new System.Windows.Forms.Label();
			this.BtnOk = new System.Windows.Forms.Button();
			this.BtnOpenFile = new System.Windows.Forms.Button();
			this.BtnPrint = new System.Windows.Forms.Button();
			this.SuspendLayout();
			// 
			// LblInfo
			// 
			this.LblInfo.AccessibleDescription = resources.GetString("LblInfo.AccessibleDescription");
			this.LblInfo.AccessibleName = resources.GetString("LblInfo.AccessibleName");
			this.LblInfo.Anchor = ((System.Windows.Forms.AnchorStyles)(resources.GetObject("LblInfo.Anchor")));
			this.LblInfo.AutoSize = ((bool)(resources.GetObject("LblInfo.AutoSize")));
			this.LblInfo.Dock = ((System.Windows.Forms.DockStyle)(resources.GetObject("LblInfo.Dock")));
			this.LblInfo.Enabled = ((bool)(resources.GetObject("LblInfo.Enabled")));
			this.LblInfo.FlatStyle = System.Windows.Forms.FlatStyle.System;
			this.LblInfo.Font = ((System.Drawing.Font)(resources.GetObject("LblInfo.Font")));
			this.LblInfo.Image = ((System.Drawing.Image)(resources.GetObject("LblInfo.Image")));
			this.LblInfo.ImageAlign = ((System.Drawing.ContentAlignment)(resources.GetObject("LblInfo.ImageAlign")));
			this.LblInfo.ImageIndex = ((int)(resources.GetObject("LblInfo.ImageIndex")));
			this.LblInfo.ImeMode = ((System.Windows.Forms.ImeMode)(resources.GetObject("LblInfo.ImeMode")));
			this.LblInfo.Location = ((System.Drawing.Point)(resources.GetObject("LblInfo.Location")));
			this.LblInfo.Name = "LblInfo";
			this.LblInfo.RightToLeft = ((System.Windows.Forms.RightToLeft)(resources.GetObject("LblInfo.RightToLeft")));
			this.LblInfo.Size = ((System.Drawing.Size)(resources.GetObject("LblInfo.Size")));
			this.LblInfo.TabIndex = ((int)(resources.GetObject("LblInfo.TabIndex")));
			this.LblInfo.Text = resources.GetString("LblInfo.Text");
			this.LblInfo.TextAlign = ((System.Drawing.ContentAlignment)(resources.GetObject("LblInfo.TextAlign")));
			this.LblInfo.Visible = ((bool)(resources.GetObject("LblInfo.Visible")));
			// 
			// BtnOk
			// 
			this.BtnOk.AccessibleDescription = resources.GetString("BtnOk.AccessibleDescription");
			this.BtnOk.AccessibleName = resources.GetString("BtnOk.AccessibleName");
			this.BtnOk.Anchor = ((System.Windows.Forms.AnchorStyles)(resources.GetObject("BtnOk.Anchor")));
			this.BtnOk.BackgroundImage = ((System.Drawing.Image)(resources.GetObject("BtnOk.BackgroundImage")));
			this.BtnOk.Dock = ((System.Windows.Forms.DockStyle)(resources.GetObject("BtnOk.Dock")));
			this.BtnOk.Enabled = ((bool)(resources.GetObject("BtnOk.Enabled")));
			this.BtnOk.FlatStyle = ((System.Windows.Forms.FlatStyle)(resources.GetObject("BtnOk.FlatStyle")));
			this.BtnOk.Font = ((System.Drawing.Font)(resources.GetObject("BtnOk.Font")));
			this.BtnOk.Image = ((System.Drawing.Image)(resources.GetObject("BtnOk.Image")));
			this.BtnOk.ImageAlign = ((System.Drawing.ContentAlignment)(resources.GetObject("BtnOk.ImageAlign")));
			this.BtnOk.ImageIndex = ((int)(resources.GetObject("BtnOk.ImageIndex")));
			this.BtnOk.ImeMode = ((System.Windows.Forms.ImeMode)(resources.GetObject("BtnOk.ImeMode")));
			this.BtnOk.Location = ((System.Drawing.Point)(resources.GetObject("BtnOk.Location")));
			this.BtnOk.Name = "BtnOk";
			this.BtnOk.RightToLeft = ((System.Windows.Forms.RightToLeft)(resources.GetObject("BtnOk.RightToLeft")));
			this.BtnOk.Size = ((System.Drawing.Size)(resources.GetObject("BtnOk.Size")));
			this.BtnOk.TabIndex = ((int)(resources.GetObject("BtnOk.TabIndex")));
			this.BtnOk.Text = resources.GetString("BtnOk.Text");
			this.BtnOk.TextAlign = ((System.Drawing.ContentAlignment)(resources.GetObject("BtnOk.TextAlign")));
			this.BtnOk.Visible = ((bool)(resources.GetObject("BtnOk.Visible")));
			this.BtnOk.Click += new System.EventHandler(this.BtnOk_Click);
			// 
			// BtnOpenFile
			// 
			this.BtnOpenFile.AccessibleDescription = resources.GetString("BtnOpenFile.AccessibleDescription");
			this.BtnOpenFile.AccessibleName = resources.GetString("BtnOpenFile.AccessibleName");
			this.BtnOpenFile.Anchor = ((System.Windows.Forms.AnchorStyles)(resources.GetObject("BtnOpenFile.Anchor")));
			this.BtnOpenFile.BackgroundImage = ((System.Drawing.Image)(resources.GetObject("BtnOpenFile.BackgroundImage")));
			this.BtnOpenFile.Dock = ((System.Windows.Forms.DockStyle)(resources.GetObject("BtnOpenFile.Dock")));
			this.BtnOpenFile.Enabled = ((bool)(resources.GetObject("BtnOpenFile.Enabled")));
			this.BtnOpenFile.FlatStyle = ((System.Windows.Forms.FlatStyle)(resources.GetObject("BtnOpenFile.FlatStyle")));
			this.BtnOpenFile.Font = ((System.Drawing.Font)(resources.GetObject("BtnOpenFile.Font")));
			this.BtnOpenFile.Image = ((System.Drawing.Image)(resources.GetObject("BtnOpenFile.Image")));
			this.BtnOpenFile.ImageAlign = ((System.Drawing.ContentAlignment)(resources.GetObject("BtnOpenFile.ImageAlign")));
			this.BtnOpenFile.ImageIndex = ((int)(resources.GetObject("BtnOpenFile.ImageIndex")));
			this.BtnOpenFile.ImeMode = ((System.Windows.Forms.ImeMode)(resources.GetObject("BtnOpenFile.ImeMode")));
			this.BtnOpenFile.Location = ((System.Drawing.Point)(resources.GetObject("BtnOpenFile.Location")));
			this.BtnOpenFile.Name = "BtnOpenFile";
			this.BtnOpenFile.RightToLeft = ((System.Windows.Forms.RightToLeft)(resources.GetObject("BtnOpenFile.RightToLeft")));
			this.BtnOpenFile.Size = ((System.Drawing.Size)(resources.GetObject("BtnOpenFile.Size")));
			this.BtnOpenFile.TabIndex = ((int)(resources.GetObject("BtnOpenFile.TabIndex")));
			this.BtnOpenFile.Text = resources.GetString("BtnOpenFile.Text");
			this.BtnOpenFile.TextAlign = ((System.Drawing.ContentAlignment)(resources.GetObject("BtnOpenFile.TextAlign")));
			this.BtnOpenFile.Visible = ((bool)(resources.GetObject("BtnOpenFile.Visible")));
			this.BtnOpenFile.Click += new System.EventHandler(this.BtnOpenFile_Click);
			// 
			// BtnPrint
			// 
			this.BtnPrint.AccessibleDescription = resources.GetString("BtnPrint.AccessibleDescription");
			this.BtnPrint.AccessibleName = resources.GetString("BtnPrint.AccessibleName");
			this.BtnPrint.Anchor = ((System.Windows.Forms.AnchorStyles)(resources.GetObject("BtnPrint.Anchor")));
			this.BtnPrint.BackgroundImage = ((System.Drawing.Image)(resources.GetObject("BtnPrint.BackgroundImage")));
			this.BtnPrint.Dock = ((System.Windows.Forms.DockStyle)(resources.GetObject("BtnPrint.Dock")));
			this.BtnPrint.Enabled = ((bool)(resources.GetObject("BtnPrint.Enabled")));
			this.BtnPrint.FlatStyle = ((System.Windows.Forms.FlatStyle)(resources.GetObject("BtnPrint.FlatStyle")));
			this.BtnPrint.Font = ((System.Drawing.Font)(resources.GetObject("BtnPrint.Font")));
			this.BtnPrint.Image = ((System.Drawing.Image)(resources.GetObject("BtnPrint.Image")));
			this.BtnPrint.ImageAlign = ((System.Drawing.ContentAlignment)(resources.GetObject("BtnPrint.ImageAlign")));
			this.BtnPrint.ImageIndex = ((int)(resources.GetObject("BtnPrint.ImageIndex")));
			this.BtnPrint.ImeMode = ((System.Windows.Forms.ImeMode)(resources.GetObject("BtnPrint.ImeMode")));
			this.BtnPrint.Location = ((System.Drawing.Point)(resources.GetObject("BtnPrint.Location")));
			this.BtnPrint.Name = "BtnPrint";
			this.BtnPrint.RightToLeft = ((System.Windows.Forms.RightToLeft)(resources.GetObject("BtnPrint.RightToLeft")));
			this.BtnPrint.Size = ((System.Drawing.Size)(resources.GetObject("BtnPrint.Size")));
			this.BtnPrint.TabIndex = ((int)(resources.GetObject("BtnPrint.TabIndex")));
			this.BtnPrint.Text = resources.GetString("BtnPrint.Text");
			this.BtnPrint.TextAlign = ((System.Drawing.ContentAlignment)(resources.GetObject("BtnPrint.TextAlign")));
			this.BtnPrint.Visible = ((bool)(resources.GetObject("BtnPrint.Visible")));
			this.BtnPrint.Click += new System.EventHandler(this.BtnPrint_Click);
			// 
			// FileListOptions
			// 
			this.AccessibleDescription = resources.GetString("$this.AccessibleDescription");
			this.AccessibleName = resources.GetString("$this.AccessibleName");
			this.AutoScaleBaseSize = ((System.Drawing.Size)(resources.GetObject("$this.AutoScaleBaseSize")));
			this.AutoScroll = ((bool)(resources.GetObject("$this.AutoScroll")));
			this.AutoScrollMargin = ((System.Drawing.Size)(resources.GetObject("$this.AutoScrollMargin")));
			this.AutoScrollMinSize = ((System.Drawing.Size)(resources.GetObject("$this.AutoScrollMinSize")));
			this.BackgroundImage = ((System.Drawing.Image)(resources.GetObject("$this.BackgroundImage")));
			this.ClientSize = ((System.Drawing.Size)(resources.GetObject("$this.ClientSize")));
			this.Controls.Add(this.BtnPrint);
			this.Controls.Add(this.BtnOpenFile);
			this.Controls.Add(this.BtnOk);
			this.Controls.Add(this.LblInfo);
			this.Enabled = ((bool)(resources.GetObject("$this.Enabled")));
			this.Font = ((System.Drawing.Font)(resources.GetObject("$this.Font")));
			this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedToolWindow;
			this.Icon = ((System.Drawing.Icon)(resources.GetObject("$this.Icon")));
			this.ImeMode = ((System.Windows.Forms.ImeMode)(resources.GetObject("$this.ImeMode")));
			this.Location = ((System.Drawing.Point)(resources.GetObject("$this.Location")));
			this.MaximumSize = ((System.Drawing.Size)(resources.GetObject("$this.MaximumSize")));
			this.MinimumSize = ((System.Drawing.Size)(resources.GetObject("$this.MinimumSize")));
			this.Name = "FileListOptions";
			this.RightToLeft = ((System.Windows.Forms.RightToLeft)(resources.GetObject("$this.RightToLeft")));
			this.ShowInTaskbar = false;
			this.StartPosition = ((System.Windows.Forms.FormStartPosition)(resources.GetObject("$this.StartPosition")));
			this.Text = resources.GetString("$this.Text");
			this.ResumeLayout(false);

		}
		#endregion

		
	}
}
