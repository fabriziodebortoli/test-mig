using System;
using System.Drawing;
using System.Collections;
using System.ComponentModel;
using System.Windows.Forms;
using System.IO;
using System.Xml;
using System.Data;

namespace Microarea.Library.TranslationManager
{
	/// <summary>
	/// Summary description for RepetitionManager.
	/// </summary>
	public class RepetitionManager : System.Windows.Forms.Form
	{
		private System.Windows.Forms.DataGrid DGrid;
		private System.Windows.Forms.DataGridTableStyle DGStyle;
		private System.Windows.Forms.DataGridTextBoxColumn CStyleSource;
		private System.Windows.Forms.DataGridTextBoxColumn CStyleTarget;
		private System.Windows.Forms.DataGridTextBoxColumn CStyleModule;
		private System.Windows.Forms.DataGridTextBoxColumn CStyleType;
		private System.Windows.Forms.DataGridBoolColumn CStyleAllow;
		private System.Windows.Forms.ListBox LSTTerms;
		private XmlDocument xGlossary = new XmlDocument();
		private string startupPath = string.Empty;
		private RepetitionListTable rlTable = null;
		private Hashtable repetitions = new Hashtable();
		private System.Windows.Forms.Button CMDSaveExit;
		private System.Windows.Forms.Button CMDSaveContinue;
		private System.Windows.Forms.Button CMDExit;
		private System.Windows.Forms.Button CMDCheck;
		private System.Windows.Forms.Button CMDUncheck;
		/// <summary>
		/// Required designer variable.
		/// </summary>
		private System.ComponentModel.Container components = null;

		public RepetitionManager()
		{
			//
			// Required for Windows Form Designer support
			//
			InitializeComponent();

			//
			// TODO: Add any constructor code after InitializeComponent call
			//
		}

		public RepetitionManager(string sPath, Hashtable rep)
		{
			//
			// Required for Windows Form Designer support
			//
			InitializeComponent();

			startupPath = sPath;
			repetitions = rep;
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
			System.Resources.ResourceManager resources = new System.Resources.ResourceManager(typeof(RepetitionManager));
			this.DGrid = new System.Windows.Forms.DataGrid();
			this.DGStyle = new System.Windows.Forms.DataGridTableStyle();
			this.CStyleSource = new System.Windows.Forms.DataGridTextBoxColumn();
			this.CStyleTarget = new System.Windows.Forms.DataGridTextBoxColumn();
			this.CStyleModule = new System.Windows.Forms.DataGridTextBoxColumn();
			this.CStyleType = new System.Windows.Forms.DataGridTextBoxColumn();
			this.CStyleAllow = new System.Windows.Forms.DataGridBoolColumn();
			this.LSTTerms = new System.Windows.Forms.ListBox();
			this.CMDSaveExit = new System.Windows.Forms.Button();
			this.CMDSaveContinue = new System.Windows.Forms.Button();
			this.CMDExit = new System.Windows.Forms.Button();
			this.CMDCheck = new System.Windows.Forms.Button();
			this.CMDUncheck = new System.Windows.Forms.Button();
			((System.ComponentModel.ISupportInitialize)(this.DGrid)).BeginInit();
			this.SuspendLayout();
			// 
			// DGrid
			// 
			this.DGrid.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
				| System.Windows.Forms.AnchorStyles.Left) 
				| System.Windows.Forms.AnchorStyles.Right)));
			this.DGrid.CaptionVisible = false;
			this.DGrid.DataMember = "";
			this.DGrid.HeaderFont = new System.Drawing.Font("Microsoft Sans Serif", 9.75F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((System.Byte)(0)));
			this.DGrid.HeaderForeColor = System.Drawing.SystemColors.ControlText;
			this.DGrid.Location = new System.Drawing.Point(168, 8);
			this.DGrid.Name = "DGrid";
			this.DGrid.Size = new System.Drawing.Size(616, 256);
			this.DGrid.TabIndex = 1;
			this.DGrid.TableStyles.AddRange(new System.Windows.Forms.DataGridTableStyle[] {
																							  this.DGStyle});
			// 
			// DGStyle
			// 
			this.DGStyle.DataGrid = this.DGrid;
			this.DGStyle.GridColumnStyles.AddRange(new System.Windows.Forms.DataGridColumnStyle[] {
																									  this.CStyleSource,
																									  this.CStyleTarget,
																									  this.CStyleModule,
																									  this.CStyleType,
																									  this.CStyleAllow});
			this.DGStyle.HeaderForeColor = System.Drawing.SystemColors.ControlText;
			this.DGStyle.MappingName = "";
			// 
			// CStyleSource
			// 
			this.CStyleSource.Format = "";
			this.CStyleSource.FormatInfo = null;
			this.CStyleSource.HeaderText = "Source";
			this.CStyleSource.MappingName = "Source";
			this.CStyleSource.ReadOnly = true;
			this.CStyleSource.Width = 130;
			// 
			// CStyleTarget
			// 
			this.CStyleTarget.Format = "";
			this.CStyleTarget.FormatInfo = null;
			this.CStyleTarget.HeaderText = "Target";
			this.CStyleTarget.MappingName = "Target";
			this.CStyleTarget.ReadOnly = true;
			this.CStyleTarget.Width = 130;
			// 
			// CStyleModule
			// 
			this.CStyleModule.Format = "";
			this.CStyleModule.FormatInfo = null;
			this.CStyleModule.HeaderText = "Module";
			this.CStyleModule.MappingName = "Module";
			this.CStyleModule.ReadOnly = true;
			this.CStyleModule.Width = 75;
			// 
			// CStyleType
			// 
			this.CStyleType.Format = "";
			this.CStyleType.FormatInfo = null;
			this.CStyleType.HeaderText = "Type";
			this.CStyleType.MappingName = "Type";
			this.CStyleType.ReadOnly = true;
			this.CStyleType.Width = 50;
			// 
			// CStyleAllow
			// 
			this.CStyleAllow.Alignment = System.Windows.Forms.HorizontalAlignment.Center;
			this.CStyleAllow.FalseValue = false;
			this.CStyleAllow.HeaderText = "Duplicable";
			this.CStyleAllow.MappingName = "AllowDuplicate";
			this.CStyleAllow.NullValue = ((object)(resources.GetObject("CStyleAllow.NullValue")));
			this.CStyleAllow.TrueValue = true;
			this.CStyleAllow.Width = 60;
			// 
			// LSTTerms
			// 
			this.LSTTerms.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
				| System.Windows.Forms.AnchorStyles.Left)));
			this.LSTTerms.Location = new System.Drawing.Point(8, 8);
			this.LSTTerms.Name = "LSTTerms";
			this.LSTTerms.Size = new System.Drawing.Size(152, 290);
			this.LSTTerms.Sorted = true;
			this.LSTTerms.TabIndex = 2;
			this.LSTTerms.SelectedIndexChanged += new System.EventHandler(this.LSTTerms_SelectedIndexChanged);
			// 
			// CMDSaveExit
			// 
			this.CMDSaveExit.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
			this.CMDSaveExit.Location = new System.Drawing.Point(704, 272);
			this.CMDSaveExit.Name = "CMDSaveExit";
			this.CMDSaveExit.Size = new System.Drawing.Size(80, 32);
			this.CMDSaveExit.TabIndex = 3;
			this.CMDSaveExit.Text = "Save and Exit";
			this.CMDSaveExit.Click += new System.EventHandler(this.CMDSaveExit_Click);
			// 
			// CMDSaveContinue
			// 
			this.CMDSaveContinue.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
			this.CMDSaveContinue.Location = new System.Drawing.Point(624, 272);
			this.CMDSaveContinue.Name = "CMDSaveContinue";
			this.CMDSaveContinue.Size = new System.Drawing.Size(80, 32);
			this.CMDSaveContinue.TabIndex = 4;
			this.CMDSaveContinue.Text = "Save and Continue";
			this.CMDSaveContinue.Click += new System.EventHandler(this.CMDSaveContinue_Click);
			// 
			// CMDExit
			// 
			this.CMDExit.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
			this.CMDExit.Location = new System.Drawing.Point(544, 272);
			this.CMDExit.Name = "CMDExit";
			this.CMDExit.Size = new System.Drawing.Size(80, 32);
			this.CMDExit.TabIndex = 5;
			this.CMDExit.Text = "Exit Without Saving";
			this.CMDExit.Click += new System.EventHandler(this.CMDExit_Click);
			// 
			// CMDCheck
			// 
			this.CMDCheck.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
			this.CMDCheck.Location = new System.Drawing.Point(168, 264);
			this.CMDCheck.Name = "CMDCheck";
			this.CMDCheck.Size = new System.Drawing.Size(64, 24);
			this.CMDCheck.TabIndex = 6;
			this.CMDCheck.Text = "Check";
			this.CMDCheck.Click += new System.EventHandler(this.CMDCheck_Click);
			// 
			// CMDUncheck
			// 
			this.CMDUncheck.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
			this.CMDUncheck.Location = new System.Drawing.Point(232, 264);
			this.CMDUncheck.Name = "CMDUncheck";
			this.CMDUncheck.Size = new System.Drawing.Size(64, 24);
			this.CMDUncheck.TabIndex = 7;
			this.CMDUncheck.Text = "Uncheck";
			this.CMDUncheck.Click += new System.EventHandler(this.CMDUncheck_Click);
			// 
			// RepetitionManager
			// 
			this.AutoScaleBaseSize = new System.Drawing.Size(5, 13);
			this.ClientSize = new System.Drawing.Size(792, 318);
			this.ControlBox = false;
			this.Controls.Add(this.CMDUncheck);
			this.Controls.Add(this.CMDCheck);
			this.Controls.Add(this.CMDExit);
			this.Controls.Add(this.CMDSaveContinue);
			this.Controls.Add(this.CMDSaveExit);
			this.Controls.Add(this.LSTTerms);
			this.Controls.Add(this.DGrid);
			this.Name = "RepetitionManager";
			this.Text = "Repetition Manager";
			this.Load += new System.EventHandler(this.RepetitionManager_Load);
			((System.ComponentModel.ISupportInitialize)(this.DGrid)).EndInit();
			this.ResumeLayout(false);

		}
		#endregion

		private void RepetitionManager_Load(object sender, System.EventArgs e)
		{
			string glossaryFileName = Path.Combine(startupPath, "MagoNet-ITtoENGlossary.xml");
			if (!File.Exists(glossaryFileName))
				return;

			xGlossary.Load(glossaryFileName);

			foreach (XmlNode n in repetitions.Values)
			{
				string target = n.Attributes["target"].Value.ToString();

				if (!LSTTerms.Items.Contains(target))
					LSTTerms.Items.Add(target);
			}
		}

		private void LSTTerms_SelectedIndexChanged(object sender, System.EventArgs e)
		{
			if (rlTable != null)
				rlTable.GetValues(ref xGlossary);

			string target = (string) LSTTerms.SelectedItem;
			rlTable = new RepetitionListTable(target);
			rlTable.Fill(target, xGlossary);
			
			DGrid.DataSource = rlTable;
			rlTable.DefaultView.AllowNew = false;
			DGStyle.MappingName = target;

			DGrid.AllowSorting = false;
			DGrid.CaptionText = target;
		}

		private void CMDExit_Click(object sender, System.EventArgs e)
		{
			Close();
		}

		private void CMDSaveExit_Click(object sender, System.EventArgs e)
		{
			if (rlTable != null)
				rlTable.GetValues(ref xGlossary);

			string glossaryFileName = Path.Combine(startupPath, "MagoNet-ITtoENGlossary.xml");
			if (!File.Exists(glossaryFileName))
				return;

			try
			{
				xGlossary.Save(glossaryFileName);
			}
			catch
			{
				MessageBox.Show(string.Format("Errore in fase di salvataggio del file {0}", glossaryFileName));
			}
			Close();
		}

		private void CMDSaveContinue_Click(object sender, System.EventArgs e)
		{
			if (rlTable != null)
				rlTable.GetValues(ref xGlossary);

			string glossaryFileName = Path.Combine(startupPath, "MagoNet-ITtoENGlossary.xml");
			if (!File.Exists(glossaryFileName))
				return;

			try
			{
				xGlossary.Save(glossaryFileName);
			}
			catch
			{
				MessageBox.Show(string.Format("Errore in fase di salvataggio del file {0}", glossaryFileName));
			}
		}

		private void CMDCheck_Click(object sender, System.EventArgs e)
		{
			rlTable.SetValues(true);
		}

		private void CMDUncheck_Click(object sender, System.EventArgs e)
		{
			rlTable.SetValues(false);
		}
	}

	public class RepetitionListTable : DataTable
	{
		public RepetitionListTable(string targetValue)
		{
			this.TableName = targetValue;

			DataColumn cSource	= new DataColumn("Source",					typeof(string));
			DataColumn cTarget	= new DataColumn("Target",					typeof(string));
			DataColumn cModule	= new DataColumn("Module",					typeof(string));
			DataColumn cType	= new DataColumn("Type",					typeof(string));
			DataColumn cAllow	= new DataColumn("AllowDuplicate",			typeof(bool));

			Columns.Add(cSource);
			Columns.Add(cTarget);
			Columns.Add(cModule);
			Columns.Add(cType);
			Columns.Add(cAllow);
		}

		public void Fill(string targetValue, XmlDocument xGlossary)
		{
			string sXPathQuery = string.Format("Glossary/Term[@target='{0}']", targetValue);
			foreach (XmlNode nTerm in xGlossary.SelectNodes(sXPathQuery))
			{
				DataRow dRow = NewRow();

				dRow["Source"]			= nTerm.Attributes["source"].Value.ToString();
				dRow["Target"]			= nTerm.Attributes["target"].Value.ToString();
				dRow["Module"]			= nTerm.Attributes["module"].Value.ToString();
				dRow["Type"]			= nTerm.Attributes["types"].Value.ToString();
				dRow["AllowDuplicate"]	= bool.Parse(nTerm.Attributes["allowDuplicate"].Value.ToString());

				Rows.Add(dRow);
			}
		}

		public void GetValues(ref XmlDocument xGlossary)
		{
			foreach (DataRow dRow in this.Rows)
			{
				string sXPathQuery = string.Format("Glossary/Term[@source='{0}']", dRow["Source"]);
				XmlNode nTerm = xGlossary.SelectSingleNode(sXPathQuery);
				if (dRow["AllowDuplicate"].ToString() != string.Empty)
					nTerm.Attributes["allowDuplicate"].Value	= dRow["AllowDuplicate"].ToString();
			}
		}

		public void SetValues(bool isChecked)
		{
			foreach (DataRow dRow in this.Rows)
			{
				dRow["AllowDuplicate"] = isChecked;
			}
		}
	}
}
