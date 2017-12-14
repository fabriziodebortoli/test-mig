using System;
using System.Collections;
using System.Collections.Specialized;
using System.Windows.Forms;

namespace Microarea.Tools.TBLocalizer.Forms
{
	/// <summary>
	/// Summary description for CategoryFilter.
	/// </summary>
	public class CategoryFilter : System.Windows.Forms.UserControl
	{
		private System.Windows.Forms.GroupBox GBFilter;
		private System.Windows.Forms.CheckBox CBMenu;
		private System.Windows.Forms.CheckBox CBStringtable;
		private System.Windows.Forms.CheckBox CBSource;
		private System.Windows.Forms.CheckBox CBXml;
		private System.Windows.Forms.CheckBox CBDatabase;
		private System.Windows.Forms.CheckBox CBReport;
		private System.Windows.Forms.CheckBox CBOther;
		private System.Windows.Forms.CheckBox CBStrings;
		private System.Windows.Forms.CheckBox CBForms;
		private System.Windows.Forms.CheckBox CBDialog;
		private System.Windows.Forms.CheckBox CBFilter;
        private System.Windows.Forms.CheckBox CBDBInfo;
        private System.Windows.Forms.CheckBox CBJsonForms;
        private CheckBox CBWeb;

        /// <summary> 
        /// Required designer variable.
        /// </summary>
        private System.ComponentModel.Container components = null;

		//=========================================================================
		public delegate void ApplyFilterChanged(object sender, EventArgs e);
		public event ApplyFilterChanged OnApplyFilterChanged;

		//--------------------------------------------------------------------------------
		public	bool			ApplyFilter			{get {return CBFilter.Checked;} }

        //--------------------------------------------------------------------------------
        public StringCollection AvailableFilters
        {
            get
            {
                StringCollection s = new StringCollection();
                foreach (CheckBox cb in GBFilter.Controls)
                {
                        s.Add(cb.Name);
                }

                return s;
            }
        }

        //--------------------------------------------------------------------------------
		public	string[]		Filters				
		{
			get 
			{
				ArrayList l = new ArrayList();
				foreach (CheckBox cb in GBFilter.Controls)
				{
					if (cb.Checked)
						l.Add(cb.Name);
				}

				return (string[])l.ToArray(typeof(string));
			}
		}

		public CategoryFilter()
		{
			// This call is required by the Windows.Forms Form Designer.
			InitializeComponent();

			// TODO: Add any initialization after the InitializeComponent call

		}

		
		//--------------------------------------------------------------------------------
		public void EnableFilter()
		{
			GBFilter.Enabled = CBFilter.Checked;
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

		#region Component Designer generated code
		/// <summary> 
		/// Required method for Designer support - do not modify 
		/// the contents of this method with the code editor.
		/// </summary>
		private void InitializeComponent()
		{
            this.GBFilter = new System.Windows.Forms.GroupBox();
            this.CBJsonForms = new System.Windows.Forms.CheckBox();
            this.CBDBInfo = new System.Windows.Forms.CheckBox();
            this.CBMenu = new System.Windows.Forms.CheckBox();
            this.CBStringtable = new System.Windows.Forms.CheckBox();
            this.CBSource = new System.Windows.Forms.CheckBox();
            this.CBXml = new System.Windows.Forms.CheckBox();
            this.CBDatabase = new System.Windows.Forms.CheckBox();
            this.CBReport = new System.Windows.Forms.CheckBox();
            this.CBOther = new System.Windows.Forms.CheckBox();
            this.CBStrings = new System.Windows.Forms.CheckBox();
            this.CBForms = new System.Windows.Forms.CheckBox();
            this.CBDialog = new System.Windows.Forms.CheckBox();
            this.CBFilter = new System.Windows.Forms.CheckBox();
            this.CBWeb = new System.Windows.Forms.CheckBox();
            this.GBFilter.SuspendLayout();
            this.SuspendLayout();
            // 
            // GBFilter
            // 
            this.GBFilter.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.GBFilter.Controls.Add(this.CBJsonForms);
            this.GBFilter.Controls.Add(this.CBDBInfo);
            this.GBFilter.Controls.Add(this.CBMenu);
            this.GBFilter.Controls.Add(this.CBStringtable);
            this.GBFilter.Controls.Add(this.CBSource);
            this.GBFilter.Controls.Add(this.CBXml);
            this.GBFilter.Controls.Add(this.CBDatabase);
            this.GBFilter.Controls.Add(this.CBReport);
            this.GBFilter.Controls.Add(this.CBWeb);
            this.GBFilter.Controls.Add(this.CBOther);
            this.GBFilter.Controls.Add(this.CBStrings);
            this.GBFilter.Controls.Add(this.CBForms);
            this.GBFilter.Controls.Add(this.CBDialog);
            this.GBFilter.FlatStyle = System.Windows.Forms.FlatStyle.System;
            this.GBFilter.Location = new System.Drawing.Point(0, 28);
            this.GBFilter.Name = "GBFilter";
            this.GBFilter.Size = new System.Drawing.Size(376, 219);
            this.GBFilter.TabIndex = 1;
            this.GBFilter.TabStop = false;
            this.GBFilter.Text = "Categories";
            // 
            // CBJsonForms
            // 
            this.CBJsonForms.Checked = true;
            this.CBJsonForms.CheckState = System.Windows.Forms.CheckState.Checked;
            this.CBJsonForms.FlatStyle = System.Windows.Forms.FlatStyle.System;
            this.CBJsonForms.ImeMode = System.Windows.Forms.ImeMode.NoControl;
            this.CBJsonForms.Location = new System.Drawing.Point(183, 24);
            this.CBJsonForms.Name = "CBJsonForms";
            this.CBJsonForms.Size = new System.Drawing.Size(144, 24);
            this.CBJsonForms.TabIndex = 1;
            this.CBJsonForms.Text = "Json Forms";
            // 
            // CBDBInfo
            // 
            this.CBDBInfo.Checked = true;
            this.CBDBInfo.CheckState = System.Windows.Forms.CheckState.Checked;
            this.CBDBInfo.FlatStyle = System.Windows.Forms.FlatStyle.System;
            this.CBDBInfo.ImeMode = System.Windows.Forms.ImeMode.NoControl;
            this.CBDBInfo.Location = new System.Drawing.Point(24, 145);
            this.CBDBInfo.Name = "CBDBInfo";
            this.CBDBInfo.Size = new System.Drawing.Size(144, 24);
            this.CBDBInfo.TabIndex = 10;
            this.CBDBInfo.Text = "DBInfo";
            // 
            // CBMenu
            // 
            this.CBMenu.Checked = true;
            this.CBMenu.CheckState = System.Windows.Forms.CheckState.Checked;
            this.CBMenu.FlatStyle = System.Windows.Forms.FlatStyle.System;
            this.CBMenu.ImeMode = System.Windows.Forms.ImeMode.NoControl;
            this.CBMenu.Location = new System.Drawing.Point(24, 48);
            this.CBMenu.Name = "CBMenu";
            this.CBMenu.Size = new System.Drawing.Size(144, 24);
            this.CBMenu.TabIndex = 2;
            this.CBMenu.Text = "Menu";
            // 
            // CBStringtable
            // 
            this.CBStringtable.Checked = true;
            this.CBStringtable.CheckState = System.Windows.Forms.CheckState.Checked;
            this.CBStringtable.FlatStyle = System.Windows.Forms.FlatStyle.System;
            this.CBStringtable.ImeMode = System.Windows.Forms.ImeMode.NoControl;
            this.CBStringtable.Location = new System.Drawing.Point(24, 72);
            this.CBStringtable.Name = "CBStringtable";
            this.CBStringtable.Size = new System.Drawing.Size(144, 24);
            this.CBStringtable.TabIndex = 4;
            this.CBStringtable.Text = "Stringtable";
            // 
            // CBSource
            // 
            this.CBSource.Checked = true;
            this.CBSource.CheckState = System.Windows.Forms.CheckState.Checked;
            this.CBSource.FlatStyle = System.Windows.Forms.FlatStyle.System;
            this.CBSource.ImeMode = System.Windows.Forms.ImeMode.NoControl;
            this.CBSource.Location = new System.Drawing.Point(24, 96);
            this.CBSource.Name = "CBSource";
            this.CBSource.Size = new System.Drawing.Size(144, 24);
            this.CBSource.TabIndex = 6;
            this.CBSource.Text = "Source";
            // 
            // CBXml
            // 
            this.CBXml.Checked = true;
            this.CBXml.CheckState = System.Windows.Forms.CheckState.Checked;
            this.CBXml.FlatStyle = System.Windows.Forms.FlatStyle.System;
            this.CBXml.ImeMode = System.Windows.Forms.ImeMode.NoControl;
            this.CBXml.Location = new System.Drawing.Point(24, 120);
            this.CBXml.Name = "CBXml";
            this.CBXml.Size = new System.Drawing.Size(144, 24);
            this.CBXml.TabIndex = 8;
            this.CBXml.Text = "Xml";
            // 
            // CBDatabase
            // 
            this.CBDatabase.Checked = true;
            this.CBDatabase.CheckState = System.Windows.Forms.CheckState.Checked;
            this.CBDatabase.FlatStyle = System.Windows.Forms.FlatStyle.System;
            this.CBDatabase.ImeMode = System.Windows.Forms.ImeMode.NoControl;
            this.CBDatabase.Location = new System.Drawing.Point(183, 48);
            this.CBDatabase.Name = "CBDatabase";
            this.CBDatabase.Size = new System.Drawing.Size(144, 24);
            this.CBDatabase.TabIndex = 3;
            this.CBDatabase.Text = "Database";
            // 
            // CBReport
            // 
            this.CBReport.Checked = true;
            this.CBReport.CheckState = System.Windows.Forms.CheckState.Checked;
            this.CBReport.FlatStyle = System.Windows.Forms.FlatStyle.System;
            this.CBReport.ImeMode = System.Windows.Forms.ImeMode.NoControl;
            this.CBReport.Location = new System.Drawing.Point(183, 72);
            this.CBReport.Name = "CBReport";
            this.CBReport.Size = new System.Drawing.Size(144, 24);
            this.CBReport.TabIndex = 5;
            this.CBReport.Text = "Report";
            // 
            // CBOther
            // 
            this.CBOther.Checked = true;
            this.CBOther.CheckState = System.Windows.Forms.CheckState.Checked;
            this.CBOther.FlatStyle = System.Windows.Forms.FlatStyle.System;
            this.CBOther.ImeMode = System.Windows.Forms.ImeMode.NoControl;
            this.CBOther.Location = new System.Drawing.Point(183, 172);
            this.CBOther.Name = "CBOther";
            this.CBOther.Size = new System.Drawing.Size(144, 24);
            this.CBOther.TabIndex = 11;
            this.CBOther.Text = "Other";
            // 
            // CBStrings
            // 
            this.CBStrings.Checked = true;
            this.CBStrings.CheckState = System.Windows.Forms.CheckState.Checked;
            this.CBStrings.FlatStyle = System.Windows.Forms.FlatStyle.System;
            this.CBStrings.ImeMode = System.Windows.Forms.ImeMode.NoControl;
            this.CBStrings.Location = new System.Drawing.Point(183, 96);
            this.CBStrings.Name = "CBStrings";
            this.CBStrings.Size = new System.Drawing.Size(144, 24);
            this.CBStrings.TabIndex = 7;
            this.CBStrings.Text = "Strings";
            // 
            // CBForms
            // 
            this.CBForms.Checked = true;
            this.CBForms.CheckState = System.Windows.Forms.CheckState.Checked;
            this.CBForms.FlatStyle = System.Windows.Forms.FlatStyle.System;
            this.CBForms.ImeMode = System.Windows.Forms.ImeMode.NoControl;
            this.CBForms.Location = new System.Drawing.Point(183, 120);
            this.CBForms.Name = "CBForms";
            this.CBForms.Size = new System.Drawing.Size(144, 24);
            this.CBForms.TabIndex = 9;
            this.CBForms.Text = "Forms";
            // 
            // CBDialog
            // 
            this.CBDialog.Checked = true;
            this.CBDialog.CheckState = System.Windows.Forms.CheckState.Checked;
            this.CBDialog.FlatStyle = System.Windows.Forms.FlatStyle.System;
            this.CBDialog.ImeMode = System.Windows.Forms.ImeMode.NoControl;
            this.CBDialog.Location = new System.Drawing.Point(24, 24);
            this.CBDialog.Name = "CBDialog";
            this.CBDialog.Size = new System.Drawing.Size(144, 24);
            this.CBDialog.TabIndex = 0;
            this.CBDialog.Text = "Dialog";
            // 
            // CBFilter
            // 
            this.CBFilter.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.CBFilter.FlatStyle = System.Windows.Forms.FlatStyle.System;
            this.CBFilter.ImeMode = System.Windows.Forms.ImeMode.NoControl;
            this.CBFilter.Location = new System.Drawing.Point(0, 0);
            this.CBFilter.Name = "CBFilter";
            this.CBFilter.Size = new System.Drawing.Size(184, 24);
            this.CBFilter.TabIndex = 0;
            this.CBFilter.Text = "Filter by category";
            this.CBFilter.CheckedChanged += new System.EventHandler(this.CBFilter_CheckedChanged);
            // 
            // CBWeb
            // 
            this.CBWeb.Checked = true;
            this.CBWeb.CheckState = System.Windows.Forms.CheckState.Checked;
            this.CBWeb.FlatStyle = System.Windows.Forms.FlatStyle.System;
            this.CBWeb.ImeMode = System.Windows.Forms.ImeMode.NoControl;
            this.CBWeb.Location = new System.Drawing.Point(183, 145);
            this.CBWeb.Name = "CBWeb";
            this.CBWeb.Size = new System.Drawing.Size(144, 24);
            this.CBWeb.TabIndex = 11;
            this.CBWeb.Text = "Web";
            // 
            // CategoryFilter
            // 
            this.Controls.Add(this.GBFilter);
            this.Controls.Add(this.CBFilter);
            this.Font = new System.Drawing.Font("Verdana", 9.75F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.Name = "CategoryFilter";
            this.Size = new System.Drawing.Size(376, 251);
            this.Load += new System.EventHandler(this.CategoryFilter_Load);
            this.GBFilter.ResumeLayout(false);
            this.ResumeLayout(false);

		}
		#endregion

		private void CategoryFilter_Load(object sender, System.EventArgs e)
		{
			CBSource.Name = AllStrings.source;
			CBStringtable.Name = AllStrings.stringtable;
			CBMenu.Name = AllStrings.menu;
			CBDialog.Name = AllStrings.dialog;
			CBReport.Name = AllStrings.report;
			CBOther.Name = AllStrings.other;
			CBXml.Name = AllStrings.xml;
			CBStrings.Name = AllStrings.strings;
			CBForms.Name = AllStrings.forms;
			CBDatabase.Name = AllStrings.database;
            CBDBInfo.Name = AllStrings.dbinfo;
            CBJsonForms.Name = AllStrings.jsonforms;

			CBSource.Text = AllStrings.source;
			CBStringtable.Text = AllStrings.stringtable;
			CBMenu.Text = AllStrings.menu;
			CBDialog.Text = AllStrings.dialog;
			CBReport.Text = AllStrings.report;
			CBOther.Text = AllStrings.other;
			CBXml.Text = AllStrings.xml;
			CBStrings.Text = AllStrings.strings;
			CBForms.Text = AllStrings.forms;
			CBDatabase.Text = AllStrings.database;
            CBDBInfo.Text = AllStrings.dbinfo;
            CBJsonForms.Text = AllStrings.jsonforms;

			EnableFilter();

		}

		private void CBFilter_CheckedChanged(object sender, System.EventArgs e)
		{
			EnableFilter();
			if (OnApplyFilterChanged != null)
				OnApplyFilterChanged(sender, e);
		}

		
	}
}
