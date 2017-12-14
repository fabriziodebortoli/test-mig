using System;
using System.ComponentModel;
using System.Diagnostics;
using System.IO;
using System.Windows.Forms;
using System.Xml;

namespace Microarea.Tools.TBLocalizer.SourceBinding
{
	public class OptionsForm : Form
	{
		#region Controls
		private TextBox txtPath;
		private Label label1;
		private Button btnSearch;
		private LinkLabel lblDescription;
		private Button btnOk;
		private Button btnCancel;

		private System.ComponentModel.Container components = null;
		#endregion
		
		#region Private members
		private SourceControlOptions originalOptions;
		protected Microarea.Tools.TBLocalizer.SourceBinding.SourceControlOptionsContainer currentOptions;
		private const string WinMergeUrl = "http://www.winmerge.org";
		#endregion

		#region Constructors
		//---------------------------------------------------------------------
		public OptionsForm(SourceControlOptions options)
		{
			InitializeComponent();
			currentOptions.Options = options.Clone() as SourceControlOptions;
			originalOptions = options;
		}
		#endregion

		/// <summary>
		/// Clean up any resources being used.
		/// </summary>
		//---------------------------------------------------------------------
		protected override void Dispose(bool disposing)
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
		//--------------------------------------------------------------------------------
		private void InitializeComponent()
		{
			System.Resources.ResourceManager resources = new System.Resources.ResourceManager(typeof(OptionsForm));
			this.currentOptions = new Microarea.Tools.TBLocalizer.SourceBinding.SourceControlOptionsContainer();
			this.txtPath = new System.Windows.Forms.TextBox();
			this.label1 = new System.Windows.Forms.Label();
			this.btnSearch = new System.Windows.Forms.Button();
			this.lblDescription = new System.Windows.Forms.LinkLabel();
			this.btnOk = new System.Windows.Forms.Button();
			this.btnCancel = new System.Windows.Forms.Button();
			this.SuspendLayout();
			// 
			// currentOptions
			// 
			this.currentOptions.ExePath = "";
			// 
			// stringContainer
			// 
			// 
			// txtPath
			// 
			this.txtPath.AccessibleDescription = resources.GetString("txtPath.AccessibleDescription");
			this.txtPath.AccessibleName = resources.GetString("txtPath.AccessibleName");
			this.txtPath.Anchor = ((System.Windows.Forms.AnchorStyles)(resources.GetObject("txtPath.Anchor")));
			this.txtPath.AutoSize = ((bool)(resources.GetObject("txtPath.AutoSize")));
			this.txtPath.BackgroundImage = ((System.Drawing.Image)(resources.GetObject("txtPath.BackgroundImage")));
			this.txtPath.DataBindings.Add(new System.Windows.Forms.Binding("Text", this.currentOptions, "ExePath"));
			this.txtPath.Dock = ((System.Windows.Forms.DockStyle)(resources.GetObject("txtPath.Dock")));
			this.txtPath.Enabled = ((bool)(resources.GetObject("txtPath.Enabled")));
			this.txtPath.Font = ((System.Drawing.Font)(resources.GetObject("txtPath.Font")));
			this.txtPath.ImeMode = ((System.Windows.Forms.ImeMode)(resources.GetObject("txtPath.ImeMode")));
			this.txtPath.Location = ((System.Drawing.Point)(resources.GetObject("txtPath.Location")));
			this.txtPath.MaxLength = ((int)(resources.GetObject("txtPath.MaxLength")));
			this.txtPath.Multiline = ((bool)(resources.GetObject("txtPath.Multiline")));
			this.txtPath.Name = "txtPath";
			this.txtPath.PasswordChar = ((char)(resources.GetObject("txtPath.PasswordChar")));
			this.txtPath.RightToLeft = ((System.Windows.Forms.RightToLeft)(resources.GetObject("txtPath.RightToLeft")));
			this.txtPath.ScrollBars = ((System.Windows.Forms.ScrollBars)(resources.GetObject("txtPath.ScrollBars")));
			this.txtPath.Size = ((System.Drawing.Size)(resources.GetObject("txtPath.Size")));
			this.txtPath.TabIndex = ((int)(resources.GetObject("txtPath.TabIndex")));
			this.txtPath.Text = resources.GetString("txtPath.Text");
			this.txtPath.TextAlign = ((System.Windows.Forms.HorizontalAlignment)(resources.GetObject("txtPath.TextAlign")));
			this.txtPath.Visible = ((bool)(resources.GetObject("txtPath.Visible")));
			this.txtPath.WordWrap = ((bool)(resources.GetObject("txtPath.WordWrap")));
			// 
			// label1
			// 
			this.label1.AccessibleDescription = resources.GetString("label1.AccessibleDescription");
			this.label1.AccessibleName = resources.GetString("label1.AccessibleName");
			this.label1.Anchor = ((System.Windows.Forms.AnchorStyles)(resources.GetObject("label1.Anchor")));
			this.label1.AutoSize = ((bool)(resources.GetObject("label1.AutoSize")));
			this.label1.Dock = ((System.Windows.Forms.DockStyle)(resources.GetObject("label1.Dock")));
			this.label1.Enabled = ((bool)(resources.GetObject("label1.Enabled")));
			this.label1.Font = ((System.Drawing.Font)(resources.GetObject("label1.Font")));
			this.label1.Image = ((System.Drawing.Image)(resources.GetObject("label1.Image")));
			this.label1.ImageAlign = ((System.Drawing.ContentAlignment)(resources.GetObject("label1.ImageAlign")));
			this.label1.ImageIndex = ((int)(resources.GetObject("label1.ImageIndex")));
			this.label1.ImeMode = ((System.Windows.Forms.ImeMode)(resources.GetObject("label1.ImeMode")));
			this.label1.Location = ((System.Drawing.Point)(resources.GetObject("label1.Location")));
			this.label1.Name = "label1";
			this.label1.RightToLeft = ((System.Windows.Forms.RightToLeft)(resources.GetObject("label1.RightToLeft")));
			this.label1.Size = ((System.Drawing.Size)(resources.GetObject("label1.Size")));
			this.label1.TabIndex = ((int)(resources.GetObject("label1.TabIndex")));
			this.label1.Text = resources.GetString("label1.Text");
			this.label1.TextAlign = ((System.Drawing.ContentAlignment)(resources.GetObject("label1.TextAlign")));
			this.label1.Visible = ((bool)(resources.GetObject("label1.Visible")));
			// 
			// btnSearch
			// 
			this.btnSearch.AccessibleDescription = resources.GetString("btnSearch.AccessibleDescription");
			this.btnSearch.AccessibleName = resources.GetString("btnSearch.AccessibleName");
			this.btnSearch.Anchor = ((System.Windows.Forms.AnchorStyles)(resources.GetObject("btnSearch.Anchor")));
			this.btnSearch.BackgroundImage = ((System.Drawing.Image)(resources.GetObject("btnSearch.BackgroundImage")));
			this.btnSearch.Dock = ((System.Windows.Forms.DockStyle)(resources.GetObject("btnSearch.Dock")));
			this.btnSearch.Enabled = ((bool)(resources.GetObject("btnSearch.Enabled")));
			this.btnSearch.FlatStyle = ((System.Windows.Forms.FlatStyle)(resources.GetObject("btnSearch.FlatStyle")));
			this.btnSearch.Font = ((System.Drawing.Font)(resources.GetObject("btnSearch.Font")));
			this.btnSearch.Image = ((System.Drawing.Image)(resources.GetObject("btnSearch.Image")));
			this.btnSearch.ImageAlign = ((System.Drawing.ContentAlignment)(resources.GetObject("btnSearch.ImageAlign")));
			this.btnSearch.ImageIndex = ((int)(resources.GetObject("btnSearch.ImageIndex")));
			this.btnSearch.ImeMode = ((System.Windows.Forms.ImeMode)(resources.GetObject("btnSearch.ImeMode")));
			this.btnSearch.Location = ((System.Drawing.Point)(resources.GetObject("btnSearch.Location")));
			this.btnSearch.Name = "btnSearch";
			this.btnSearch.RightToLeft = ((System.Windows.Forms.RightToLeft)(resources.GetObject("btnSearch.RightToLeft")));
			this.btnSearch.Size = ((System.Drawing.Size)(resources.GetObject("btnSearch.Size")));
			this.btnSearch.TabIndex = ((int)(resources.GetObject("btnSearch.TabIndex")));
			this.btnSearch.Text = resources.GetString("btnSearch.Text");
			this.btnSearch.TextAlign = ((System.Drawing.ContentAlignment)(resources.GetObject("btnSearch.TextAlign")));
			this.btnSearch.Visible = ((bool)(resources.GetObject("btnSearch.Visible")));
			this.btnSearch.Click += new System.EventHandler(this.btnSearch_Click);
			// 
			// lblDescription
			// 
			this.lblDescription.AccessibleDescription = resources.GetString("lblDescription.AccessibleDescription");
			this.lblDescription.AccessibleName = resources.GetString("lblDescription.AccessibleName");
			this.lblDescription.Anchor = ((System.Windows.Forms.AnchorStyles)(resources.GetObject("lblDescription.Anchor")));
			this.lblDescription.AutoSize = ((bool)(resources.GetObject("lblDescription.AutoSize")));
			this.lblDescription.BorderStyle = System.Windows.Forms.BorderStyle.Fixed3D;
			this.lblDescription.Dock = ((System.Windows.Forms.DockStyle)(resources.GetObject("lblDescription.Dock")));
			this.lblDescription.Enabled = ((bool)(resources.GetObject("lblDescription.Enabled")));
			this.lblDescription.FlatStyle = System.Windows.Forms.FlatStyle.System;
			this.lblDescription.Font = ((System.Drawing.Font)(resources.GetObject("lblDescription.Font")));
			this.lblDescription.Image = ((System.Drawing.Image)(resources.GetObject("lblDescription.Image")));
			this.lblDescription.ImageAlign = ((System.Drawing.ContentAlignment)(resources.GetObject("lblDescription.ImageAlign")));
			this.lblDescription.ImageIndex = ((int)(resources.GetObject("lblDescription.ImageIndex")));
			this.lblDescription.ImeMode = ((System.Windows.Forms.ImeMode)(resources.GetObject("lblDescription.ImeMode")));
			this.lblDescription.LinkArea = ((System.Windows.Forms.LinkArea)(resources.GetObject("lblDescription.LinkArea")));
			this.lblDescription.Location = ((System.Drawing.Point)(resources.GetObject("lblDescription.Location")));
			this.lblDescription.Name = "lblDescription";
			this.lblDescription.RightToLeft = ((System.Windows.Forms.RightToLeft)(resources.GetObject("lblDescription.RightToLeft")));
			this.lblDescription.Size = ((System.Drawing.Size)(resources.GetObject("lblDescription.Size")));
			this.lblDescription.TabIndex = ((int)(resources.GetObject("lblDescription.TabIndex")));
			this.lblDescription.Text = resources.GetString("lblDescription.Text");
			this.lblDescription.TextAlign = ((System.Drawing.ContentAlignment)(resources.GetObject("lblDescription.TextAlign")));
			this.lblDescription.Visible = ((bool)(resources.GetObject("lblDescription.Visible")));
			this.lblDescription.LinkClicked += new System.Windows.Forms.LinkLabelLinkClickedEventHandler(this.lblDescription_LinkClicked);
			// 
			// btnOk
			// 
			this.btnOk.AccessibleDescription = resources.GetString("btnOk.AccessibleDescription");
			this.btnOk.AccessibleName = resources.GetString("btnOk.AccessibleName");
			this.btnOk.Anchor = ((System.Windows.Forms.AnchorStyles)(resources.GetObject("btnOk.Anchor")));
			this.btnOk.BackgroundImage = ((System.Drawing.Image)(resources.GetObject("btnOk.BackgroundImage")));
			this.btnOk.DialogResult = System.Windows.Forms.DialogResult.OK;
			this.btnOk.Dock = ((System.Windows.Forms.DockStyle)(resources.GetObject("btnOk.Dock")));
			this.btnOk.Enabled = ((bool)(resources.GetObject("btnOk.Enabled")));
			this.btnOk.FlatStyle = ((System.Windows.Forms.FlatStyle)(resources.GetObject("btnOk.FlatStyle")));
			this.btnOk.Font = ((System.Drawing.Font)(resources.GetObject("btnOk.Font")));
			this.btnOk.Image = ((System.Drawing.Image)(resources.GetObject("btnOk.Image")));
			this.btnOk.ImageAlign = ((System.Drawing.ContentAlignment)(resources.GetObject("btnOk.ImageAlign")));
			this.btnOk.ImageIndex = ((int)(resources.GetObject("btnOk.ImageIndex")));
			this.btnOk.ImeMode = ((System.Windows.Forms.ImeMode)(resources.GetObject("btnOk.ImeMode")));
			this.btnOk.Location = ((System.Drawing.Point)(resources.GetObject("btnOk.Location")));
			this.btnOk.Name = "btnOk";
			this.btnOk.RightToLeft = ((System.Windows.Forms.RightToLeft)(resources.GetObject("btnOk.RightToLeft")));
			this.btnOk.Size = ((System.Drawing.Size)(resources.GetObject("btnOk.Size")));
			this.btnOk.TabIndex = ((int)(resources.GetObject("btnOk.TabIndex")));
			this.btnOk.Text = resources.GetString("btnOk.Text");
			this.btnOk.TextAlign = ((System.Drawing.ContentAlignment)(resources.GetObject("btnOk.TextAlign")));
			this.btnOk.Visible = ((bool)(resources.GetObject("btnOk.Visible")));
			// 
			// btnCancel
			// 
			this.btnCancel.AccessibleDescription = resources.GetString("btnCancel.AccessibleDescription");
			this.btnCancel.AccessibleName = resources.GetString("btnCancel.AccessibleName");
			this.btnCancel.Anchor = ((System.Windows.Forms.AnchorStyles)(resources.GetObject("btnCancel.Anchor")));
			this.btnCancel.BackgroundImage = ((System.Drawing.Image)(resources.GetObject("btnCancel.BackgroundImage")));
			this.btnCancel.DialogResult = System.Windows.Forms.DialogResult.Cancel;
			this.btnCancel.Dock = ((System.Windows.Forms.DockStyle)(resources.GetObject("btnCancel.Dock")));
			this.btnCancel.Enabled = ((bool)(resources.GetObject("btnCancel.Enabled")));
			this.btnCancel.FlatStyle = ((System.Windows.Forms.FlatStyle)(resources.GetObject("btnCancel.FlatStyle")));
			this.btnCancel.Font = ((System.Drawing.Font)(resources.GetObject("btnCancel.Font")));
			this.btnCancel.Image = ((System.Drawing.Image)(resources.GetObject("btnCancel.Image")));
			this.btnCancel.ImageAlign = ((System.Drawing.ContentAlignment)(resources.GetObject("btnCancel.ImageAlign")));
			this.btnCancel.ImageIndex = ((int)(resources.GetObject("btnCancel.ImageIndex")));
			this.btnCancel.ImeMode = ((System.Windows.Forms.ImeMode)(resources.GetObject("btnCancel.ImeMode")));
			this.btnCancel.Location = ((System.Drawing.Point)(resources.GetObject("btnCancel.Location")));
			this.btnCancel.Name = "btnCancel";
			this.btnCancel.RightToLeft = ((System.Windows.Forms.RightToLeft)(resources.GetObject("btnCancel.RightToLeft")));
			this.btnCancel.Size = ((System.Drawing.Size)(resources.GetObject("btnCancel.Size")));
			this.btnCancel.TabIndex = ((int)(resources.GetObject("btnCancel.TabIndex")));
			this.btnCancel.Text = resources.GetString("btnCancel.Text");
			this.btnCancel.TextAlign = ((System.Drawing.ContentAlignment)(resources.GetObject("btnCancel.TextAlign")));
			this.btnCancel.Visible = ((bool)(resources.GetObject("btnCancel.Visible")));
			// 
			// OptionsForm
			// 
			this.AccessibleDescription = resources.GetString("$this.AccessibleDescription");
			this.AccessibleName = resources.GetString("$this.AccessibleName");
			this.AutoScaleBaseSize = ((System.Drawing.Size)(resources.GetObject("$this.AutoScaleBaseSize")));
			this.AutoScroll = ((bool)(resources.GetObject("$this.AutoScroll")));
			this.AutoScrollMargin = ((System.Drawing.Size)(resources.GetObject("$this.AutoScrollMargin")));
			this.AutoScrollMinSize = ((System.Drawing.Size)(resources.GetObject("$this.AutoScrollMinSize")));
			this.BackgroundImage = ((System.Drawing.Image)(resources.GetObject("$this.BackgroundImage")));
			this.ClientSize = ((System.Drawing.Size)(resources.GetObject("$this.ClientSize")));
			this.Controls.Add(this.btnOk);
			this.Controls.Add(this.lblDescription);
			this.Controls.Add(this.btnSearch);
			this.Controls.Add(this.label1);
			this.Controls.Add(this.txtPath);
			this.Controls.Add(this.btnCancel);
			this.Enabled = ((bool)(resources.GetObject("$this.Enabled")));
			this.Font = ((System.Drawing.Font)(resources.GetObject("$this.Font")));
			this.Icon = ((System.Drawing.Icon)(resources.GetObject("$this.Icon")));
			this.ImeMode = ((System.Windows.Forms.ImeMode)(resources.GetObject("$this.ImeMode")));
			this.Location = ((System.Drawing.Point)(resources.GetObject("$this.Location")));
			this.MaximumSize = ((System.Drawing.Size)(resources.GetObject("$this.MaximumSize")));
			this.MinimumSize = ((System.Drawing.Size)(resources.GetObject("$this.MinimumSize")));
			this.Name = "OptionsForm";
			this.RightToLeft = ((System.Windows.Forms.RightToLeft)(resources.GetObject("$this.RightToLeft")));
			this.ShowInTaskbar = false;
			this.StartPosition = ((System.Windows.Forms.FormStartPosition)(resources.GetObject("$this.StartPosition")));
			this.Text = resources.GetString("$this.Text");
			this.Closing += new System.ComponentModel.CancelEventHandler(this.Options_Closing);
			this.Load += new System.EventHandler(this.OptionsForm_Load);
			this.ResumeLayout(false);

		}
		#endregion

		#region Private methods
		//---------------------------------------------------------------------
		private bool IsValidFile(string path)
		{
			if (path == null || path.Length == 0)
				return true;

			return File.Exists(path);
		}

		//---------------------------------------------------------------------
		private void lblDescription_LinkClicked(object sender, System.Windows.Forms.LinkLabelLinkClickedEventArgs e)
		{
			// Determine which link was clicked within the LinkLabel.
			lblDescription.Links[lblDescription.Links.IndexOf(e.Link)].Visited = true;

			Process.Start(WinMergeUrl);
		}

		//---------------------------------------------------------------------
		private void Options_Closing(object sender, System.ComponentModel.CancelEventArgs e)
		{
			if (DialogResult == DialogResult.OK)
			{
				if (!IsValidFile(txtPath.Text))
				{
					MessageBox.Show
					(
						this,
						"Invalid file",
						Application.ProductName,
						MessageBoxButtons.OK,
						MessageBoxIcon.Error
					);

					txtPath.Focus();
					e.Cancel = true;
					return;
				}

				currentOptions.Options.Save();
				originalOptions.Assign(currentOptions.Options);
			}
		}

		//---------------------------------------------------------------------
		private void btnSearch_Click(object sender, System.EventArgs e)
		{
			OpenFileDialog ofd = new OpenFileDialog();

			if (File.Exists(txtPath.Text))
				ofd.FileName = txtPath.Text;

			ofd.Filter = "Executable file (*.exe)|*.exe" ;

			if (ofd.ShowDialog(this) == DialogResult.OK )
			{
				if (!IsValidFile(ofd.FileName))
				{
					MessageBox.Show
					(
						this,
						"Invalid file",
						Application.ProductName,
						MessageBoxButtons.OK,
						MessageBoxIcon.Error
					);

					txtPath.Focus();
					return;
				}

				txtPath.Text = ofd.FileName;
				txtPath.Focus();
			}
		}

		//---------------------------------------------------------------------
		private void OptionsForm_Load(object sender, System.EventArgs e)
		{
			lblDescription.Text = string.Format(lblDescription.Text, WinMergeUrl);
			lblDescription.LinkArea = new LinkArea(lblDescription.Text.IndexOf(WinMergeUrl), WinMergeUrl.Length);
		}
		#endregion
	}

	//=========================================================================
	public class SourceControlOptionsContainer : Component
	{
		public SourceControlOptions Options = new SourceControlOptions();

		//---------------------------------------------------------------------
		public string ExePath { get { return Options.ExePath; } set { Options.ExePath = value; } }
	}

	//=========================================================================
	public class SourceControlOptions: ICloneable
	{
		private static string statePath =
			Path.Combine
			(
				Path.GetDirectoryName(System.Reflection.Assembly.GetExecutingAssembly().Location),
				"SourceControlOptions.xml"
			);

		private string exePath = string.Empty;

		//---------------------------------------------------------------------
		public string ExePath { get { return exePath; } set { exePath = value; } }
		//---------------------------------------------------------------------
		public SourceControlOptions()
		{
		}

		//---------------------------------------------------------------------
		public void Save()
		{
			XmlDocument d = new XmlDocument();
			d.AppendChild(d.CreateElement("Options"));
			XmlElement el = (XmlElement)d.DocumentElement.AppendChild(d.CreateElement("ExePath"));
			el.SetAttribute("Value", ExePath);
			d.Save(statePath);
		}

		//---------------------------------------------------------------------
		public static SourceControlOptions GetFromXml()
		{
			SourceControlOptions o = new SourceControlOptions();

			if (File.Exists(statePath))
			{
				try
				{
					XmlDocument d = new XmlDocument();
					d.Load(statePath);
					XmlAttribute a = (XmlAttribute)d.DocumentElement.SelectSingleNode("ExePath/@Value");
					o.ExePath = a.Value;
				}
				catch 
				{
				}
			}

			return o;
		}


		#region ICloneable Members
		//---------------------------------------------------------------------
		public object Clone()
		{
			SourceControlOptions newOptions = new SourceControlOptions();
			newOptions.Assign(this);
			return newOptions;
		}

		//---------------------------------------------------------------------
		public void Assign(SourceControlOptions options)
		{
			this.ExePath = options.ExePath;
		}
		#endregion
	}
}
