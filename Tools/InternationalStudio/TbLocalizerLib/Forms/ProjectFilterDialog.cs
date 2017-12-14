using System;
using System.Collections;
using System.Collections.Specialized;
using System.Globalization;
using System.IO;
using System.Windows.Forms;
//
using Microarea.Tools.TBLocalizer.CommonUtilities;

namespace Microarea.Tools.TBLocalizer.Forms
{
	public class ProjectFilterDialog : Form
	{
		#region Controls

        private Button btnOk;
		private Button btnCamcel;
		private GroupBox GBAdvancedFilter;
        private CheckBox CkbWebMethods;
		private CheckBox CkbEnums;
        private GroupBox GBLanguage;
        //
		private CategoryFilter categoryFilter;
		private CultureInfoComboBox cmbDictionaries;
		//
		private System.ComponentModel.Container components = null;
		#endregion

		#region Private members
		private ArrayList filesToExclude = new ArrayList();
        bool globalFiltersOnly = false;
		#endregion

		#region Public properties
		//---------------------------------------------------------------------
		public bool ApplyFilter			{ get { return categoryFilter.ApplyFilter; } }
		//---------------------------------------------------------------------
        public StringCollection AvailableFilters { get { return categoryFilter.AvailableFilters; } }
		//---------------------------------------------------------------------
		public string[] Filters			{ get { return categoryFilter.Filters; } }
		//---------------------------------------------------------------------
		public string[] AvailableDictionaries	{ set { cmbDictionaries.CultureStrings = value; } }
		//---------------------------------------------------------------------
		public	ArrayList FilesToExclude{ get { return filesToExclude; } }
		//---------------------------------------------------------------------
		public CultureInfo ChoosedLanguage		{ get { return cmbDictionaries.SelectedItem as CultureInfo; } }
        //---------------------------------------------------------------------
        public bool GlobalFiltersOnly { get { return globalFiltersOnly; } set { globalFiltersOnly = value; GlobalFiltersOnly_changed(); } }
		#endregion

		//---------------------------------------------------------------------
		public enum AdvancedFilter { DBINFO, WEBMETHODS, ENUMS }


		#region Constructors
		//---------------------------------------------------------------------
		public ProjectFilterDialog()
		{
			InitializeComponent();
			PostInitializeComponent();
		}
		#endregion

		/// <summary>
		/// Clean up any resources being used.
		/// </summary>
		//---------------------------------------------------------------------
		protected override void Dispose(bool disposing)
		{
			if (disposing)
			{
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
		//--------------------------------------------------------------------------------
		private void InitializeComponent()
		{
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(ProjectFilterDialog));
            this.btnOk = new System.Windows.Forms.Button();
            this.btnCamcel = new System.Windows.Forms.Button();
            this.GBAdvancedFilter = new System.Windows.Forms.GroupBox();
            this.CkbWebMethods = new System.Windows.Forms.CheckBox();
            this.CkbEnums = new System.Windows.Forms.CheckBox();
            this.cmbDictionaries = new Microarea.Tools.TBLocalizer.CommonUtilities.CultureInfoComboBox();
            this.categoryFilter = new Microarea.Tools.TBLocalizer.Forms.CategoryFilter();
            this.GBLanguage = new System.Windows.Forms.GroupBox();
            this.GBAdvancedFilter.SuspendLayout();
            this.GBLanguage.SuspendLayout();
            this.SuspendLayout();
            // 
            // btnOk
            // 
            resources.ApplyResources(this.btnOk, "btnOk");
            this.btnOk.DialogResult = System.Windows.Forms.DialogResult.OK;
            this.btnOk.Name = "btnOk";
            // 
            // btnCamcel
            // 
            resources.ApplyResources(this.btnCamcel, "btnCamcel");
            this.btnCamcel.DialogResult = System.Windows.Forms.DialogResult.Cancel;
            this.btnCamcel.Name = "btnCamcel";
            // 
            // GBAdvancedFilter
            // 
            resources.ApplyResources(this.GBAdvancedFilter, "GBAdvancedFilter");
            this.GBAdvancedFilter.Controls.Add(this.CkbWebMethods);
            this.GBAdvancedFilter.Controls.Add(this.CkbEnums);
            this.GBAdvancedFilter.FlatStyle = System.Windows.Forms.FlatStyle.System;
            this.GBAdvancedFilter.Name = "GBAdvancedFilter";
            this.GBAdvancedFilter.TabStop = false;
            // 
            // CkbWebMethods
            // 
            this.CkbWebMethods.Checked = true;
            this.CkbWebMethods.CheckState = System.Windows.Forms.CheckState.Checked;
            resources.ApplyResources(this.CkbWebMethods, "CkbWebMethods");
            this.CkbWebMethods.Name = "CkbWebMethods";
            this.CkbWebMethods.CheckedChanged += new System.EventHandler(this.CkbWebMethods_CheckedChanged);
            // 
            // CkbEnums
            // 
            this.CkbEnums.Checked = true;
            this.CkbEnums.CheckState = System.Windows.Forms.CheckState.Checked;
            resources.ApplyResources(this.CkbEnums, "CkbEnums");
            this.CkbEnums.Name = "CkbEnums";
            this.CkbEnums.CheckedChanged += new System.EventHandler(this.CkbEnums_CheckedChanged);
            // 
            // cmbDictionaries
            // 
            resources.ApplyResources(this.cmbDictionaries, "cmbDictionaries");
            this.cmbDictionaries.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.cmbDictionaries.Name = "cmbDictionaries";
            // 
            // categoryFilter
            // 
            resources.ApplyResources(this.categoryFilter, "categoryFilter");
            this.categoryFilter.Name = "categoryFilter";
            // 
            // GBLanguage
            // 
            this.GBLanguage.Controls.Add(this.cmbDictionaries);
            resources.ApplyResources(this.GBLanguage, "GBLanguage");
            this.GBLanguage.Name = "GBLanguage";
            this.GBLanguage.TabStop = false;
            // 
            // ProjectFilterDialog
            // 
            resources.ApplyResources(this, "$this");
            this.Controls.Add(this.GBLanguage);
            this.Controls.Add(this.GBAdvancedFilter);
            this.Controls.Add(this.btnOk);
            this.Controls.Add(this.categoryFilter);
            this.Controls.Add(this.btnCamcel);
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedToolWindow;
            this.Name = "ProjectFilterDialog";
            this.ShowInTaskbar = false;
            this.GBAdvancedFilter.ResumeLayout(false);
            this.GBLanguage.ResumeLayout(false);
            this.ResumeLayout(false);

		}
		#endregion

		#region Private methods
        void GlobalFiltersOnly_changed()
        {
            GBLanguage.Enabled = !globalFiltersOnly;
            GBAdvancedFilter.Enabled = categoryFilter.ApplyFilter && !globalFiltersOnly;
        }

		//---------------------------------------------------------------------
		private void PostInitializeComponent()
		{
			categoryFilter.OnApplyFilterChanged += new Microarea.Tools.TBLocalizer.Forms.CategoryFilter.ApplyFilterChanged(categoryFilter_OnApplyFilterChanged);
            GBLanguage.Enabled = !globalFiltersOnly;
            GBAdvancedFilter.Enabled = categoryFilter.ApplyFilter && !globalFiltersOnly;
			CkbEnums.Tag			 = AdvancedFilter.ENUMS;
			CkbWebMethods.Tag		 = AdvancedFilter.WEBMETHODS;
		}

		//---------------------------------------------------------------------
		private void UpdateFilesToExclude()
		{
			filesToExclude = new ArrayList();
			foreach (CheckBox cb in GBAdvancedFilter.Controls)
				if (!cb.Checked)
					filesToExclude.Add(Path.GetFileNameWithoutExtension(cb.Text));
		}

		//---------------------------------------------------------------------
		private void categoryFilter_OnApplyFilterChanged(object sender, EventArgs e)
		{
            GBAdvancedFilter.Enabled = categoryFilter.ApplyFilter && !globalFiltersOnly;
		}

		//---------------------------------------------------------------------
		private void CkbEnums_CheckedChanged(object sender, System.EventArgs e)
		{
			UpdateFilesToExclude();
		}

		//---------------------------------------------------------------------
		private void CkbWebMethods_CheckedChanged(object sender, System.EventArgs e)
		{
			UpdateFilesToExclude();
		}
		#endregion
	}
}
