using System;
using System.IO;
using System.Windows.Forms;

namespace MicroareaAddin2005
{
	/// <summary>
	/// Summary description for FileChooser.
	/// </summary>
	public class FileChooser : System.Windows.Forms.UserControl
	{
		private bool isFile;
		private bool checkExistence = false;
		
		private bool allowEmptyEntry = false;
		public event EventHandler FileTextChanged;
		
		private System.Windows.Forms.Label lblDescription;
		private System.Windows.Forms.TextBox txtFile;
		private System.Windows.Forms.Button btnBrowse;
		/// <summary> 
		/// Required designer variable.
		/// </summary>
		private System.ComponentModel.Container components = null;

		//------------------------------------------------------------------------------------
		public string EntryPath { get { return txtFile.Text; } set {txtFile.Text = value;} }
		//------------------------------------------------------------------------------------
		public string Description { get { return lblDescription.Text; } set {lblDescription.Text = value;} }
		//------------------------------------------------------------------------------------
		public bool AllowEmptyEntry { get { return allowEmptyEntry;} set { allowEmptyEntry = value; } }
		//------------------------------------------------------------------------------------
		public bool CheckExistence { get { return checkExistence;} set { checkExistence = value; } }
		//--------------------------------------------------------------------------------
		public bool IsFile
		{
			get { return isFile; }
			set { isFile = value; }
		}
		//--------------------------------------------------------------------------------
		public FileChooser()
		{
			// This call is required by the Windows.Forms Form Designer.
			InitializeComponent();

			// TODO: Add any initialization after the InitializeComponent call

		}

		/// <summary> 
		/// Clean up any resources being used.
		/// </summary>
		//--------------------------------------------------------------------------------
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

		#region Component Designer generated code
		/// <summary> 
		/// Required method for Designer support - do not modify 
		/// the contents of this method with the code editor.
		/// </summary>
		private void InitializeComponent()
		{
			this.lblDescription = new System.Windows.Forms.Label();
			this.txtFile = new System.Windows.Forms.TextBox();
			this.btnBrowse = new System.Windows.Forms.Button();
			this.SuspendLayout();
			// 
			// lblDescription
			// 
			this.lblDescription.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left)
						| System.Windows.Forms.AnchorStyles.Right)));
			this.lblDescription.Location = new System.Drawing.Point(0, 6);
			this.lblDescription.Name = "lblDescription";
			this.lblDescription.Size = new System.Drawing.Size(309, 18);
			this.lblDescription.TabIndex = 0;
			this.lblDescription.Text = "Input file";
			// 
			// txtFile
			// 
			this.txtFile.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left)
						| System.Windows.Forms.AnchorStyles.Right)));
			this.txtFile.Location = new System.Drawing.Point(0, 27);
			this.txtFile.Name = "txtFile";
			this.txtFile.Size = new System.Drawing.Size(279, 23);
			this.txtFile.TabIndex = 1;
			this.txtFile.Leave += new System.EventHandler(this.txtFile_Leave);
			this.txtFile.TextChanged += new System.EventHandler(this.txtFile_TextChanged);
			// 
			// btnBrowse
			// 
			this.btnBrowse.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
			this.btnBrowse.Location = new System.Drawing.Point(285, 27);
			this.btnBrowse.Name = "btnBrowse";
			this.btnBrowse.Size = new System.Drawing.Size(32, 23);
			this.btnBrowse.TabIndex = 2;
			this.btnBrowse.Text = "...";
			this.btnBrowse.Click += new System.EventHandler(this.btnBrowse_Click);
			// 
			// FileChooser
			// 
			this.Controls.Add(this.btnBrowse);
			this.Controls.Add(this.lblDescription);
			this.Controls.Add(this.txtFile);
			this.Font = new System.Drawing.Font("Verdana", 9.75F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
			this.Name = "FileChooser";
			this.Size = new System.Drawing.Size(320, 50);
			this.ResumeLayout(false);
			this.PerformLayout();

		}
		#endregion

		//--------------------------------------------------------------------------------
		private void btnBrowse_Click(object sender, System.EventArgs e)
		{
			if (IsFile)
			{
				OpenFileDialog ofd = new OpenFileDialog();
				if (System.IO.File.Exists(txtFile.Text))
					ofd.FileName = txtFile.Text;

				if (ofd.ShowDialog(this) == DialogResult.OK)
					txtFile.Text = ofd.FileName;
			}
			else 
			{
				FolderBrowserDialog fbd = new FolderBrowserDialog();
				if (System.IO.Directory.Exists(txtFile.Text))
					fbd.SelectedPath = txtFile.Text;

				if (fbd.ShowDialog(this) == DialogResult.OK)
					txtFile.Text = fbd.SelectedPath;
			}
			
			CheckIfValid();
		}

		//--------------------------------------------------------------------------------
		private void txtFile_TextChanged(object sender, EventArgs e)
		{
			if (FileTextChanged != null)
				FileTextChanged(this, e);
		}

		//--------------------------------------------------------------------------------
		private void txtFile_Leave(object sender, System.EventArgs e)
		{
			CheckIfValid();
		}

		//--------------------------------------------------------------------------------
		private void CheckIfValid()
		{
			if (!IsValid())
				txtFile.Focus();

		}

		//--------------------------------------------------------------------------------
		private bool IsValid()
		{
			if (checkExistence && (!allowEmptyEntry || txtFile.Text.Length > 0))
			{
				
				if (IsFile && !File.Exists(txtFile.Text))
					return false;
				if (!IsFile && !Directory.Exists(txtFile.Text))
					return false;
			}

			return txtFile.Text.IndexOfAny(Path.GetInvalidPathChars()) == -1;
		}
	}
}
