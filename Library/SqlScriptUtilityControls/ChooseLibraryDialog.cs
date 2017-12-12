using System;
using System.Drawing;
using System.Collections;
using System.ComponentModel;
using System.Windows.Forms;
using System.Xml;

using Microarea.Library.SqlScriptUtility;

namespace Microarea.Library.SqlScriptUtilityControls
{
	/// <summary>
	/// Summary description for ChooseLibrary.
	/// </summary>
	public class ChooseLibraryDialog : System.Windows.Forms.Form
	{
		private System.Windows.Forms.ListBox LSTLibraries;
		public delegate void ReturnValue(string rValue);
		public event ReturnValue OnReturnValue;
		/// <summary>
		/// Required designer variable.
		/// </summary>
		private System.ComponentModel.Container components = null;

		public ChooseLibraryDialog(string configFileName)
		{
			//
			// Required for Windows Form Designer support
			//
			InitializeComponent();

			//
			// TODO: Add any constructor code after InitializeComponent call
			//
			LeggiXml(configFileName);
		}

		/// <summary>
		/// Clean up any resources being used.
		/// </summary>
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
			System.Resources.ResourceManager resources = new System.Resources.ResourceManager(typeof(ChooseLibraryDialog));
			this.LSTLibraries = new System.Windows.Forms.ListBox();
			this.SuspendLayout();
			// 
			// LSTLibraries
			// 
			this.LSTLibraries.AccessibleDescription = resources.GetString("LSTLibraries.AccessibleDescription");
			this.LSTLibraries.AccessibleName = resources.GetString("LSTLibraries.AccessibleName");
			this.LSTLibraries.Anchor = ((System.Windows.Forms.AnchorStyles)(resources.GetObject("LSTLibraries.Anchor")));
			this.LSTLibraries.BackgroundImage = ((System.Drawing.Image)(resources.GetObject("LSTLibraries.BackgroundImage")));
			this.LSTLibraries.ColumnWidth = ((int)(resources.GetObject("LSTLibraries.ColumnWidth")));
			this.LSTLibraries.Dock = ((System.Windows.Forms.DockStyle)(resources.GetObject("LSTLibraries.Dock")));
			this.LSTLibraries.Enabled = ((bool)(resources.GetObject("LSTLibraries.Enabled")));
			this.LSTLibraries.Font = ((System.Drawing.Font)(resources.GetObject("LSTLibraries.Font")));
			this.LSTLibraries.HorizontalExtent = ((int)(resources.GetObject("LSTLibraries.HorizontalExtent")));
			this.LSTLibraries.HorizontalScrollbar = ((bool)(resources.GetObject("LSTLibraries.HorizontalScrollbar")));
			this.LSTLibraries.ImeMode = ((System.Windows.Forms.ImeMode)(resources.GetObject("LSTLibraries.ImeMode")));
			this.LSTLibraries.IntegralHeight = ((bool)(resources.GetObject("LSTLibraries.IntegralHeight")));
			this.LSTLibraries.ItemHeight = ((int)(resources.GetObject("LSTLibraries.ItemHeight")));
			this.LSTLibraries.Location = ((System.Drawing.Point)(resources.GetObject("LSTLibraries.Location")));
			this.LSTLibraries.Name = "LSTLibraries";
			this.LSTLibraries.RightToLeft = ((System.Windows.Forms.RightToLeft)(resources.GetObject("LSTLibraries.RightToLeft")));
			this.LSTLibraries.ScrollAlwaysVisible = ((bool)(resources.GetObject("LSTLibraries.ScrollAlwaysVisible")));
			this.LSTLibraries.Size = ((System.Drawing.Size)(resources.GetObject("LSTLibraries.Size")));
			this.LSTLibraries.TabIndex = ((int)(resources.GetObject("LSTLibraries.TabIndex")));
			this.LSTLibraries.Visible = ((bool)(resources.GetObject("LSTLibraries.Visible")));
			this.LSTLibraries.SelectedIndexChanged += new System.EventHandler(this.LSTLibraries_SelectedIndexChanged);
			// 
			// ChooseLibraryDialog
			// 
			this.AccessibleDescription = resources.GetString("$this.AccessibleDescription");
			this.AccessibleName = resources.GetString("$this.AccessibleName");
			this.AutoScaleBaseSize = ((System.Drawing.Size)(resources.GetObject("$this.AutoScaleBaseSize")));
			this.AutoScroll = ((bool)(resources.GetObject("$this.AutoScroll")));
			this.AutoScrollMargin = ((System.Drawing.Size)(resources.GetObject("$this.AutoScrollMargin")));
			this.AutoScrollMinSize = ((System.Drawing.Size)(resources.GetObject("$this.AutoScrollMinSize")));
			this.BackgroundImage = ((System.Drawing.Image)(resources.GetObject("$this.BackgroundImage")));
			this.ClientSize = ((System.Drawing.Size)(resources.GetObject("$this.ClientSize")));
			this.ControlBox = false;
			this.Controls.Add(this.LSTLibraries);
			this.Enabled = ((bool)(resources.GetObject("$this.Enabled")));
			this.Font = ((System.Drawing.Font)(resources.GetObject("$this.Font")));
			this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.SizableToolWindow;
			this.Icon = ((System.Drawing.Icon)(resources.GetObject("$this.Icon")));
			this.ImeMode = ((System.Windows.Forms.ImeMode)(resources.GetObject("$this.ImeMode")));
			this.Location = ((System.Drawing.Point)(resources.GetObject("$this.Location")));
			this.MaximumSize = ((System.Drawing.Size)(resources.GetObject("$this.MaximumSize")));
			this.MinimumSize = ((System.Drawing.Size)(resources.GetObject("$this.MinimumSize")));
			this.Name = "ChooseLibraryDialog";
			this.RightToLeft = ((System.Windows.Forms.RightToLeft)(resources.GetObject("$this.RightToLeft")));
			this.ShowInTaskbar = false;
			this.StartPosition = ((System.Windows.Forms.FormStartPosition)(resources.GetObject("$this.StartPosition")));
			this.Text = resources.GetString("$this.Text");
			this.Load += new System.EventHandler(this.ChooseLibrary_Load);
			this.ResumeLayout(false);

		}
		#endregion

		private void LeggiXml(string fileName)
		{
			XmlDocument xDoc = new XmlDocument();
			try
			{
				xDoc.Load(fileName);
			}
			catch
			{
				return;
			}

			foreach (XmlNode n in xDoc.SelectNodes("ModuleInfo/Components/Library"))
			{
				LSTLibraries.Items.Add(n.Attributes["name"].Value);
			}
		}

		private void LSTLibraries_SelectedIndexChanged(object sender, System.EventArgs e)
		{
			if (OnReturnValue != null)
				OnReturnValue((string)LSTLibraries.Items[LSTLibraries.SelectedIndex]);

			Close();
		}

		private void ChooseLibrary_Load(object sender, System.EventArgs e)
		{
			if (LSTLibraries.Items.Count == 0)
				Close();
		}
	}
}
