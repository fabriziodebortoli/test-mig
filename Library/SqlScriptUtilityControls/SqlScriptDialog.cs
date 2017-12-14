using System;
using System.Drawing;
using System.Collections;
using System.ComponentModel;
using System.Windows.Forms;

using Microarea.Library.SqlScriptUtility;

namespace Microarea.Library.SqlScriptUtilityControls
{
	/// <summary>
	/// Summary description for SqlScriptDialog.
	/// </summary>
	public class SqlScriptDialog : System.Windows.Forms.Form
	{
		private System.Windows.Forms.Panel PANWizard;
		private System.Windows.Forms.TabControl Tabber;
		private System.Windows.Forms.Button CMDPrev;
		private System.Windows.Forms.Button CMDNext;
		private PageSqlDescription PageTableList;
		private System.Windows.Forms.Button CMDAnnulla;
		private FileWriter fileWriter = null;
		/// <summary>
		/// Required designer variable.
		/// </summary>
		private System.ComponentModel.Container components = null;

		public SqlScriptDialog()
		{
			//
			// Required for Windows Form Designer support
			//
			InitializeComponent();

			//
			// TODO: Add any constructor code after InitializeComponent call
			//
			this.PageTableList = new PageSqlDescription();
			this.Tabber.Controls.Add(this.PageTableList);
		}

		public SqlScriptDialog(FileWriter writer)
		{
			//
			// Required for Windows Form Designer support
			//
			InitializeComponent();

			//
			// TODO: Add any constructor code after InitializeComponent call
			//
			this.PageTableList = new PageSqlDescription();
			this.Tabber.Controls.Add(this.PageTableList);

			fileWriter = writer;
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
			System.Resources.ResourceManager resources = new System.Resources.ResourceManager(typeof(SqlScriptDialog));
			this.PANWizard = new System.Windows.Forms.Panel();
			this.CMDAnnulla = new System.Windows.Forms.Button();
			this.CMDNext = new System.Windows.Forms.Button();
			this.CMDPrev = new System.Windows.Forms.Button();
			this.Tabber = new System.Windows.Forms.TabControl();
			this.PANWizard.SuspendLayout();
			this.SuspendLayout();
			// 
			// PANWizard
			// 
			this.PANWizard.AccessibleDescription = resources.GetString("PANWizard.AccessibleDescription");
			this.PANWizard.AccessibleName = resources.GetString("PANWizard.AccessibleName");
			this.PANWizard.Anchor = ((System.Windows.Forms.AnchorStyles)(resources.GetObject("PANWizard.Anchor")));
			this.PANWizard.AutoScroll = ((bool)(resources.GetObject("PANWizard.AutoScroll")));
			this.PANWizard.AutoScrollMargin = ((System.Drawing.Size)(resources.GetObject("PANWizard.AutoScrollMargin")));
			this.PANWizard.AutoScrollMinSize = ((System.Drawing.Size)(resources.GetObject("PANWizard.AutoScrollMinSize")));
			this.PANWizard.BackgroundImage = ((System.Drawing.Image)(resources.GetObject("PANWizard.BackgroundImage")));
			this.PANWizard.Controls.Add(this.CMDAnnulla);
			this.PANWizard.Controls.Add(this.CMDNext);
			this.PANWizard.Controls.Add(this.CMDPrev);
			this.PANWizard.Controls.Add(this.Tabber);
			this.PANWizard.Dock = ((System.Windows.Forms.DockStyle)(resources.GetObject("PANWizard.Dock")));
			this.PANWizard.Enabled = ((bool)(resources.GetObject("PANWizard.Enabled")));
			this.PANWizard.Font = ((System.Drawing.Font)(resources.GetObject("PANWizard.Font")));
			this.PANWizard.ImeMode = ((System.Windows.Forms.ImeMode)(resources.GetObject("PANWizard.ImeMode")));
			this.PANWizard.Location = ((System.Drawing.Point)(resources.GetObject("PANWizard.Location")));
			this.PANWizard.Name = "PANWizard";
			this.PANWizard.RightToLeft = ((System.Windows.Forms.RightToLeft)(resources.GetObject("PANWizard.RightToLeft")));
			this.PANWizard.Size = ((System.Drawing.Size)(resources.GetObject("PANWizard.Size")));
			this.PANWizard.TabIndex = ((int)(resources.GetObject("PANWizard.TabIndex")));
			this.PANWizard.Text = resources.GetString("PANWizard.Text");
			this.PANWizard.Visible = ((bool)(resources.GetObject("PANWizard.Visible")));
			// 
			// CMDAnnulla
			// 
			this.CMDAnnulla.AccessibleDescription = resources.GetString("CMDAnnulla.AccessibleDescription");
			this.CMDAnnulla.AccessibleName = resources.GetString("CMDAnnulla.AccessibleName");
			this.CMDAnnulla.Anchor = ((System.Windows.Forms.AnchorStyles)(resources.GetObject("CMDAnnulla.Anchor")));
			this.CMDAnnulla.BackgroundImage = ((System.Drawing.Image)(resources.GetObject("CMDAnnulla.BackgroundImage")));
			this.CMDAnnulla.DialogResult = System.Windows.Forms.DialogResult.Cancel;
			this.CMDAnnulla.Dock = ((System.Windows.Forms.DockStyle)(resources.GetObject("CMDAnnulla.Dock")));
			this.CMDAnnulla.Enabled = ((bool)(resources.GetObject("CMDAnnulla.Enabled")));
			this.CMDAnnulla.FlatStyle = ((System.Windows.Forms.FlatStyle)(resources.GetObject("CMDAnnulla.FlatStyle")));
			this.CMDAnnulla.Font = ((System.Drawing.Font)(resources.GetObject("CMDAnnulla.Font")));
			this.CMDAnnulla.Image = ((System.Drawing.Image)(resources.GetObject("CMDAnnulla.Image")));
			this.CMDAnnulla.ImageAlign = ((System.Drawing.ContentAlignment)(resources.GetObject("CMDAnnulla.ImageAlign")));
			this.CMDAnnulla.ImageIndex = ((int)(resources.GetObject("CMDAnnulla.ImageIndex")));
			this.CMDAnnulla.ImeMode = ((System.Windows.Forms.ImeMode)(resources.GetObject("CMDAnnulla.ImeMode")));
			this.CMDAnnulla.Location = ((System.Drawing.Point)(resources.GetObject("CMDAnnulla.Location")));
			this.CMDAnnulla.Name = "CMDAnnulla";
			this.CMDAnnulla.RightToLeft = ((System.Windows.Forms.RightToLeft)(resources.GetObject("CMDAnnulla.RightToLeft")));
			this.CMDAnnulla.Size = ((System.Drawing.Size)(resources.GetObject("CMDAnnulla.Size")));
			this.CMDAnnulla.TabIndex = ((int)(resources.GetObject("CMDAnnulla.TabIndex")));
			this.CMDAnnulla.Text = resources.GetString("CMDAnnulla.Text");
			this.CMDAnnulla.TextAlign = ((System.Drawing.ContentAlignment)(resources.GetObject("CMDAnnulla.TextAlign")));
			this.CMDAnnulla.Visible = ((bool)(resources.GetObject("CMDAnnulla.Visible")));
			this.CMDAnnulla.Click += new System.EventHandler(this.CMDAnnulla_Click);
			// 
			// CMDNext
			// 
			this.CMDNext.AccessibleDescription = resources.GetString("CMDNext.AccessibleDescription");
			this.CMDNext.AccessibleName = resources.GetString("CMDNext.AccessibleName");
			this.CMDNext.Anchor = ((System.Windows.Forms.AnchorStyles)(resources.GetObject("CMDNext.Anchor")));
			this.CMDNext.BackgroundImage = ((System.Drawing.Image)(resources.GetObject("CMDNext.BackgroundImage")));
			this.CMDNext.Dock = ((System.Windows.Forms.DockStyle)(resources.GetObject("CMDNext.Dock")));
			this.CMDNext.Enabled = ((bool)(resources.GetObject("CMDNext.Enabled")));
			this.CMDNext.FlatStyle = ((System.Windows.Forms.FlatStyle)(resources.GetObject("CMDNext.FlatStyle")));
			this.CMDNext.Font = ((System.Drawing.Font)(resources.GetObject("CMDNext.Font")));
			this.CMDNext.Image = ((System.Drawing.Image)(resources.GetObject("CMDNext.Image")));
			this.CMDNext.ImageAlign = ((System.Drawing.ContentAlignment)(resources.GetObject("CMDNext.ImageAlign")));
			this.CMDNext.ImageIndex = ((int)(resources.GetObject("CMDNext.ImageIndex")));
			this.CMDNext.ImeMode = ((System.Windows.Forms.ImeMode)(resources.GetObject("CMDNext.ImeMode")));
			this.CMDNext.Location = ((System.Drawing.Point)(resources.GetObject("CMDNext.Location")));
			this.CMDNext.Name = "CMDNext";
			this.CMDNext.RightToLeft = ((System.Windows.Forms.RightToLeft)(resources.GetObject("CMDNext.RightToLeft")));
			this.CMDNext.Size = ((System.Drawing.Size)(resources.GetObject("CMDNext.Size")));
			this.CMDNext.TabIndex = ((int)(resources.GetObject("CMDNext.TabIndex")));
			this.CMDNext.Text = resources.GetString("CMDNext.Text");
			this.CMDNext.TextAlign = ((System.Drawing.ContentAlignment)(resources.GetObject("CMDNext.TextAlign")));
			this.CMDNext.Visible = ((bool)(resources.GetObject("CMDNext.Visible")));
			this.CMDNext.Click += new System.EventHandler(this.CMDNext_Click);
			// 
			// CMDPrev
			// 
			this.CMDPrev.AccessibleDescription = resources.GetString("CMDPrev.AccessibleDescription");
			this.CMDPrev.AccessibleName = resources.GetString("CMDPrev.AccessibleName");
			this.CMDPrev.Anchor = ((System.Windows.Forms.AnchorStyles)(resources.GetObject("CMDPrev.Anchor")));
			this.CMDPrev.BackgroundImage = ((System.Drawing.Image)(resources.GetObject("CMDPrev.BackgroundImage")));
			this.CMDPrev.Dock = ((System.Windows.Forms.DockStyle)(resources.GetObject("CMDPrev.Dock")));
			this.CMDPrev.Enabled = ((bool)(resources.GetObject("CMDPrev.Enabled")));
			this.CMDPrev.FlatStyle = ((System.Windows.Forms.FlatStyle)(resources.GetObject("CMDPrev.FlatStyle")));
			this.CMDPrev.Font = ((System.Drawing.Font)(resources.GetObject("CMDPrev.Font")));
			this.CMDPrev.Image = ((System.Drawing.Image)(resources.GetObject("CMDPrev.Image")));
			this.CMDPrev.ImageAlign = ((System.Drawing.ContentAlignment)(resources.GetObject("CMDPrev.ImageAlign")));
			this.CMDPrev.ImageIndex = ((int)(resources.GetObject("CMDPrev.ImageIndex")));
			this.CMDPrev.ImeMode = ((System.Windows.Forms.ImeMode)(resources.GetObject("CMDPrev.ImeMode")));
			this.CMDPrev.Location = ((System.Drawing.Point)(resources.GetObject("CMDPrev.Location")));
			this.CMDPrev.Name = "CMDPrev";
			this.CMDPrev.RightToLeft = ((System.Windows.Forms.RightToLeft)(resources.GetObject("CMDPrev.RightToLeft")));
			this.CMDPrev.Size = ((System.Drawing.Size)(resources.GetObject("CMDPrev.Size")));
			this.CMDPrev.TabIndex = ((int)(resources.GetObject("CMDPrev.TabIndex")));
			this.CMDPrev.Text = resources.GetString("CMDPrev.Text");
			this.CMDPrev.TextAlign = ((System.Drawing.ContentAlignment)(resources.GetObject("CMDPrev.TextAlign")));
			this.CMDPrev.Visible = ((bool)(resources.GetObject("CMDPrev.Visible")));
			this.CMDPrev.Click += new System.EventHandler(this.CMDPrev_Click);
			// 
			// Tabber
			// 
			this.Tabber.AccessibleDescription = resources.GetString("Tabber.AccessibleDescription");
			this.Tabber.AccessibleName = resources.GetString("Tabber.AccessibleName");
			this.Tabber.Alignment = ((System.Windows.Forms.TabAlignment)(resources.GetObject("Tabber.Alignment")));
			this.Tabber.Anchor = ((System.Windows.Forms.AnchorStyles)(resources.GetObject("Tabber.Anchor")));
			this.Tabber.Appearance = ((System.Windows.Forms.TabAppearance)(resources.GetObject("Tabber.Appearance")));
			this.Tabber.BackgroundImage = ((System.Drawing.Image)(resources.GetObject("Tabber.BackgroundImage")));
			this.Tabber.Dock = ((System.Windows.Forms.DockStyle)(resources.GetObject("Tabber.Dock")));
			this.Tabber.Enabled = ((bool)(resources.GetObject("Tabber.Enabled")));
			this.Tabber.Font = ((System.Drawing.Font)(resources.GetObject("Tabber.Font")));
			this.Tabber.ImeMode = ((System.Windows.Forms.ImeMode)(resources.GetObject("Tabber.ImeMode")));
			this.Tabber.ItemSize = ((System.Drawing.Size)(resources.GetObject("Tabber.ItemSize")));
			this.Tabber.Location = ((System.Drawing.Point)(resources.GetObject("Tabber.Location")));
			this.Tabber.Name = "Tabber";
			this.Tabber.Padding = ((System.Drawing.Point)(resources.GetObject("Tabber.Padding")));
			this.Tabber.RightToLeft = ((System.Windows.Forms.RightToLeft)(resources.GetObject("Tabber.RightToLeft")));
			this.Tabber.SelectedIndex = 0;
			this.Tabber.ShowToolTips = ((bool)(resources.GetObject("Tabber.ShowToolTips")));
			this.Tabber.Size = ((System.Drawing.Size)(resources.GetObject("Tabber.Size")));
			this.Tabber.TabIndex = ((int)(resources.GetObject("Tabber.TabIndex")));
			this.Tabber.Text = resources.GetString("Tabber.Text");
			this.Tabber.Visible = ((bool)(resources.GetObject("Tabber.Visible")));
			// 
			// SqlScriptDialog
			// 
			this.AcceptButton = this.CMDNext;
			this.AccessibleDescription = resources.GetString("$this.AccessibleDescription");
			this.AccessibleName = resources.GetString("$this.AccessibleName");
			this.AutoScaleBaseSize = ((System.Drawing.Size)(resources.GetObject("$this.AutoScaleBaseSize")));
			this.AutoScroll = ((bool)(resources.GetObject("$this.AutoScroll")));
			this.AutoScrollMargin = ((System.Drawing.Size)(resources.GetObject("$this.AutoScrollMargin")));
			this.AutoScrollMinSize = ((System.Drawing.Size)(resources.GetObject("$this.AutoScrollMinSize")));
			this.BackgroundImage = ((System.Drawing.Image)(resources.GetObject("$this.BackgroundImage")));
			this.CancelButton = this.CMDAnnulla;
			this.ClientSize = ((System.Drawing.Size)(resources.GetObject("$this.ClientSize")));
			this.Controls.Add(this.PANWizard);
			this.Enabled = ((bool)(resources.GetObject("$this.Enabled")));
			this.Font = ((System.Drawing.Font)(resources.GetObject("$this.Font")));
			this.Icon = ((System.Drawing.Icon)(resources.GetObject("$this.Icon")));
			this.ImeMode = ((System.Windows.Forms.ImeMode)(resources.GetObject("$this.ImeMode")));
			this.Location = ((System.Drawing.Point)(resources.GetObject("$this.Location")));
			this.MaximumSize = ((System.Drawing.Size)(resources.GetObject("$this.MaximumSize")));
			this.MinimumSize = ((System.Drawing.Size)(resources.GetObject("$this.MinimumSize")));
			this.Name = "SqlScriptDialog";
			this.RightToLeft = ((System.Windows.Forms.RightToLeft)(resources.GetObject("$this.RightToLeft")));
			this.StartPosition = ((System.Windows.Forms.FormStartPosition)(resources.GetObject("$this.StartPosition")));
			this.Text = resources.GetString("$this.Text");
			this.Load += new System.EventHandler(this.FRMSqlScriptDialog_Load);
			this.PANWizard.ResumeLayout(false);
			this.ResumeLayout(false);

		}
		#endregion

		private void FRMSqlScriptDialog_Load(object sender, System.EventArgs e)
		{
			if (Tabber.TabCount == 1)
				CMDNext.Text = "Fine";
			
			Tabber.SelectedTab.Select();

			if (Tabber.SelectedIndex == 0)
				CMDPrev.Enabled = false;
		}

		
		private void CMDPrev_Click(object sender, System.EventArgs e)
		{
			NavigatePrev();
		}

		private void CMDNext_Click(object sender, System.EventArgs e)
		{
			NavigateNext();
		}

		private void Save()
		{
			bool bOk = true;
			foreach (WizardPage page in Tabber.Controls)
			{
				bOk = bOk && page.Save();
			}

			if (bOk)
			{
				this.DialogResult = DialogResult.OK;
				if (fileWriter != null)
					fileWriter.Save();
			}
			else
				this.DialogResult = DialogResult.Abort;

			Close();
		}

		public void EnableNext(bool bSelected)
		{
			CMDNext.Enabled = bSelected;
		}

		public void InsertWizardPage(WizardPage page, int insertIndex)
		{
			page.OnValidateControls += new WizardPage.ValidateControlsHandler(EnableNext);

			ArrayList OldControlPos = new ArrayList();

			if (insertIndex > Tabber.Controls.Count - 1 || Tabber.Controls.Count == 0)
			{
				Tabber.Controls.Add(page);
				return;
			}
			
			for (int i = Tabber.Controls.Count - 1; i >= insertIndex; i--)
			{
				OldControlPos.Add(Tabber.Controls[i]);
				Tabber.Controls.RemoveAt(i);
			}

			Tabber.Controls.Add(page);

			for (int i = OldControlPos.Count - 1; i >= 0; i--)
			{
				Tabber.Controls.Add((WizardPage)OldControlPos[i]);
			}
		}

		public void AppendWizardPage(WizardPage page)
		{
			page.OnValidateControls += new WizardPage.ValidateControlsHandler(EnableNext);

			Tabber.Controls.Add(page);

			if (Tabber.SelectedIndex == Tabber.TabCount - 1)
				CMDNext.Text = "Fine";
			else
				CMDNext.Text = "Avanti";
		}

		public void RemoveWizardPage(string pageName)
		{
			foreach (WizardPage page in Tabber.Controls)
			{
				if (page.Name == pageName)
				{
					if (Tabber.SelectedTab != page)
						Tabber.Controls.Remove(page);

					break;
				}
			}

			if (Tabber.SelectedIndex == 0)
				CMDPrev.Enabled = false;
		}

		public void SetParser(SqlParserUpdater parser)
		{
			PageTableList.SetParser(parser);
		}

		public void NavigateNext()
		{
			if (Tabber.SelectedIndex == Tabber.TabCount - 1)
			{
				Save();
				return;
			}

			Tabber.SelectedIndex ++;
			CMDPrev.Enabled = true;
			
			if (Tabber.SelectedIndex == Tabber.TabCount - 1)
				CMDNext.Text = "Fine";

			Tabber.SelectedTab.Select();
		}

		public void NavigatePrev()
		{
			Tabber.SelectedIndex --;
			CMDNext.Enabled = true;
			CMDNext.Text = "Avanti";
			
			if (Tabber.SelectedIndex == 0)
				CMDPrev.Enabled = false;

			Tabber.SelectedTab.Select();
		}

		private void CMDAnnulla_Click(object sender, System.EventArgs e)
		{
			this.DialogResult = DialogResult.Cancel;
			Close();
		}
	}
}
