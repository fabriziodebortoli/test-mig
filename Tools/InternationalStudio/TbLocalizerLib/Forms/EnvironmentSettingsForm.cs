using System;
using System.Collections;
using System.Diagnostics;
using System.Globalization;
using System.IO;
using System.Windows.Forms;
//
using Microarea.Tools.TBLocalizer.CommonUtilities;
using Microarea.Tools.TBLocalizer.SourceBinding;

namespace Microarea.Tools.TBLocalizer.Forms
{
	public class EnvironmentSettingsForm : Form
	{
		#region Controls
		private Button btnOk;
		private Button btnCancel;
		private Label label2;
		private TextBox txtInstallation;
		private Label label1;
		private TextBox txtDrive;
		private ComboBox txtBaseLanguage;
		private Label label3;
		private TabControl settingsTabControl;
		private TabPage tpVariables;
		private TabPage tpBuild;
		private TabPage tpDictionaryGeneration;
		private GroupBox groupBox3;
		private CheckBox cbDebug;
		private CheckBox cbRelease;
		private CheckBox CkbSatellites;
		private GroupBox groupBox1;
		private CheckBox CkbIgnoreCase;
		private CheckBox CkbIgnoreSpaces;
		private CheckBox CkbIgnorePunctuation;
		private Label label8;
		private Button btnCompare;

		private System.ComponentModel.Container components = null;
		#endregion

		#region Private members
		private EnvironmentSettings environmentSettings;
		private MicroareaAddin2005.FileChooser fcDictFolder;
		private CheckBox CkbDictionaryBinPath;
		private MicroareaAddin2005.FileChooser fcInstallationPath;
		private CaseInsensitiveStringCollection dictionaries;
		#endregion

		#region Constructors
		//---------------------------------------------------------------------
		public EnvironmentSettingsForm(EnvironmentSettings settings, CaseInsensitiveStringCollection dictionaries)
		{
			InitializeComponent();

			this.environmentSettings = settings;
			this.dictionaries = dictionaries;
		}
		#endregion

		#region Private methods

		//---------------------------------------------------------------------
		private void EnvironmentSettingsForm_Load(object sender, System.EventArgs e)
		{
			txtDrive.Text			= environmentSettings.Drive;
			txtInstallation.Text	= environmentSettings.Installation;
			fcInstallationPath.EntryPath = environmentSettings.InstallationPath;

			ArrayList dictionariesList = new ArrayList();

			/// <remarks>
			/// Se dictionaries.Count == 0, è probabile che non sia ancora
			/// stata aperta una solution, quindi sarebbe opportuno disabilitare
			/// il control. Mantenuto così per retrocompatibilità.
			/// Sarebbe però preferibile togliere la possibilità di aprire questa
			/// form se nessuna Solution è ancora stata aperta.
			///</remarks>
			if (dictionaries.Count == 0)
				dictionariesList.AddRange(CultureInfo.GetCultures(CultureTypes.AllCultures));
			else
			{
				foreach (string dictionary in dictionaries)
				{
					try
					{
						dictionariesList.Add(new CultureInfo(dictionary));
					}
					catch (ArgumentException)
					{
						Debug.WriteLine(dictionary + " is not a valid culture!");
					}
				}
			}

			txtBaseLanguage.Items.AddRange((CultureInfo[])dictionariesList.ToArray(typeof(CultureInfo)));
			txtBaseLanguage.SelectedItem = new CultureInfo(DictionaryCreator.MainContext.SolutionDocument.BaseLanguage);

			fcDictFolder.EntryPath = DictionaryCreator.MainContext.SolutionDocument.DictionaryRootPath;

			CkbSatellites.Checked = environmentSettings.BuildDictionary;
			CkbSatellites_CheckedChanged(null, null);

			cbDebug.Checked =
				environmentSettings.AssemblyConfiguration == AssemblyGenerator.ConfigurationType.CFG_BOTH ||
				environmentSettings.AssemblyConfiguration == AssemblyGenerator.ConfigurationType.CFG_DEBUG;
			cbRelease.Checked = 
				environmentSettings.AssemblyConfiguration == AssemblyGenerator.ConfigurationType.CFG_BOTH ||
				environmentSettings.AssemblyConfiguration == AssemblyGenerator.ConfigurationType.CFG_RELEASE;
			CkbDictionaryBinPath.Checked = environmentSettings.BinAsSatelliteAssemblies;
			
			CkbIgnoreCase.Checked		= (environmentSettings.StringComparisonFlags & DictionaryDocument.StringComparisonFlags.IGNORE_CASE) == DictionaryDocument.StringComparisonFlags.IGNORE_CASE;
			CkbIgnoreSpaces.Checked		= (environmentSettings.StringComparisonFlags & DictionaryDocument.StringComparisonFlags.IGNORE_SPACES) == DictionaryDocument.StringComparisonFlags.IGNORE_SPACES;
			CkbIgnorePunctuation.Checked= (environmentSettings.StringComparisonFlags & DictionaryDocument.StringComparisonFlags.IGNORE_PUNCTUATION) == DictionaryDocument.StringComparisonFlags.IGNORE_PUNCTUATION;
		}

		//---------------------------------------------------------------------
		private bool CheckFileExistence(TextBox path)
		{
			if (path.Text.Length == 0)
				return true;

			if (!File.Exists(path.Text))
			{
				MessageBox.Show
				(
					this,
					string.Format(Strings.UnexistingFile, path.Text),
					Application.ProductName,
					MessageBoxButtons.OK,
					MessageBoxIcon.Error
				);

				path.Focus();
				return false;
			}

			return true;
		}

		//---------------------------------------------------------------------
		private void EnvironmentSettingsForm_Closing(object sender, System.ComponentModel.CancelEventArgs e)
		{
			if (DialogResult == DialogResult.OK)
			{
				if (!CheckInstallationPath())
				{
					e.Cancel = true;
					MessageBox.Show(this, "Invalid installation path");
					fcInstallationPath.Focus();
				}
				environmentSettings.Drive			= txtDrive.Text;
				environmentSettings.Installation	= txtInstallation.Text;
				environmentSettings.InstallationPath = fcInstallationPath.EntryPath;	
				environmentSettings.BuildDictionary	= CkbSatellites.Checked;
				environmentSettings.AssemblyConfiguration =
					(cbDebug.Checked && cbRelease.Checked) ?
					AssemblyGenerator.ConfigurationType.CFG_BOTH :
						(cbDebug.Checked ?
						AssemblyGenerator.ConfigurationType.CFG_DEBUG :
							(cbRelease.Checked ?
							AssemblyGenerator.ConfigurationType.CFG_RELEASE :
							AssemblyGenerator.ConfigurationType.CFG_NONE)
						);

				environmentSettings.BinAsSatelliteAssemblies = CkbDictionaryBinPath.Checked;

				environmentSettings.StringComparisonFlags = DictionaryDocument.StringComparisonFlags.PERFECT_MATCH;

				if (CkbIgnoreCase.Checked)
					environmentSettings.StringComparisonFlags |= DictionaryDocument.StringComparisonFlags.IGNORE_CASE;
				if (CkbIgnoreSpaces.Checked)
					environmentSettings.StringComparisonFlags |= DictionaryDocument.StringComparisonFlags.IGNORE_SPACES;
				if (CkbIgnorePunctuation.Checked)
					environmentSettings.StringComparisonFlags |= DictionaryDocument.StringComparisonFlags.IGNORE_PUNCTUATION;

				EnvironmentGlobalSettings.GlobalSettings.Save();


				DictionaryCreator.MainContext.SolutionDocument.BaseLanguage	= ((CultureInfo)txtBaseLanguage.SelectedItem).Name;
			
				//warning: this has to be at the end of the method because may return
				#region end of method
				if (string.Compare(DictionaryCreator.MainContext.SolutionDocument.DictionaryRootPath, fcDictFolder.EntryPath, true, CultureInfo.InvariantCulture) != 0
					&& DictionaryCreator.MainContext.GetDictionaryNodes().Count > 0
					&& DialogResult.Yes == MessageBox.Show(
					this,
					Strings.WantToMigrateDictionaryPath, 
					Strings.WarningCaption, 
					MessageBoxButtons.YesNo, 
					MessageBoxIcon.Question))
				{
					DictionaryCreator.MainContext.BeginCopyDictionariesAndOpenNewSolution(fcDictFolder.EntryPath);
					return;
				}
				DictionaryCreator.MainContext.SolutionDocument.DictionaryRootPath = fcDictFolder.EntryPath;
				#endregion

			}
		}

		//---------------------------------------------------------------------
		private bool CheckInstallationPath()
		{
			if (!Directory.Exists(fcInstallationPath.EntryPath))
				return false;

			return true;
		}

		//---------------------------------------------------------------------
		private void CkbSatellites_CheckedChanged(object sender, System.EventArgs e)
		{
			groupBox3.Enabled = CkbSatellites.Checked;

			if (!CkbSatellites.Checked)
			{
				cbDebug.Checked = false;
				cbRelease.Checked = false;
			}
		}


		//---------------------------------------------------------------------
		private void OnCompareClick(object sender, System.EventArgs e)
		{
			SourceControlOptions options = new SourceControlOptions();
			options.ExePath = environmentSettings.CompareExecutablePath;
			OptionsForm f = new OptionsForm(options);

			if (f.ShowDialog(this) == DialogResult.OK)
				environmentSettings.CompareExecutablePath = options.ExePath;
		}
		#endregion

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
		//---------------------------------------------------------------------
		private void InitializeComponent()
		{
			System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(EnvironmentSettingsForm));
			this.btnOk = new System.Windows.Forms.Button();
			this.btnCancel = new System.Windows.Forms.Button();
			this.txtInstallation = new System.Windows.Forms.TextBox();
			this.txtDrive = new System.Windows.Forms.TextBox();
			this.txtBaseLanguage = new System.Windows.Forms.ComboBox();
			this.label2 = new System.Windows.Forms.Label();
			this.label1 = new System.Windows.Forms.Label();
			this.label3 = new System.Windows.Forms.Label();
			this.settingsTabControl = new System.Windows.Forms.TabControl();
			this.tpVariables = new System.Windows.Forms.TabPage();
			this.fcDictFolder = new MicroareaAddin2005.FileChooser();
			this.btnCompare = new System.Windows.Forms.Button();
			this.tpBuild = new System.Windows.Forms.TabPage();
			this.CkbSatellites = new System.Windows.Forms.CheckBox();
			this.CkbDictionaryBinPath = new System.Windows.Forms.CheckBox();
			this.groupBox3 = new System.Windows.Forms.GroupBox();
			this.cbDebug = new System.Windows.Forms.CheckBox();
			this.cbRelease = new System.Windows.Forms.CheckBox();
			this.tpDictionaryGeneration = new System.Windows.Forms.TabPage();
			this.groupBox1 = new System.Windows.Forms.GroupBox();
			this.label8 = new System.Windows.Forms.Label();
			this.CkbIgnoreCase = new System.Windows.Forms.CheckBox();
			this.CkbIgnoreSpaces = new System.Windows.Forms.CheckBox();
			this.CkbIgnorePunctuation = new System.Windows.Forms.CheckBox();
			this.fcInstallationPath = new MicroareaAddin2005.FileChooser();
			this.settingsTabControl.SuspendLayout();
			this.tpVariables.SuspendLayout();
			this.tpBuild.SuspendLayout();
			this.groupBox3.SuspendLayout();
			this.tpDictionaryGeneration.SuspendLayout();
			this.groupBox1.SuspendLayout();
			this.SuspendLayout();
			// 
			// btnOk
			// 
			resources.ApplyResources(this.btnOk, "btnOk");
			this.btnOk.DialogResult = System.Windows.Forms.DialogResult.OK;
			this.btnOk.Name = "btnOk";
			// 
			// btnCancel
			// 
			resources.ApplyResources(this.btnCancel, "btnCancel");
			this.btnCancel.DialogResult = System.Windows.Forms.DialogResult.Cancel;
			this.btnCancel.Name = "btnCancel";
			// 
			// txtInstallation
			// 
			resources.ApplyResources(this.txtInstallation, "txtInstallation");
			this.txtInstallation.Name = "txtInstallation";
			// 
			// txtDrive
			// 
			resources.ApplyResources(this.txtDrive, "txtDrive");
			this.txtDrive.Name = "txtDrive";
			// 
			// txtBaseLanguage
			// 
			resources.ApplyResources(this.txtBaseLanguage, "txtBaseLanguage");
			this.txtBaseLanguage.DisplayMember = "DisplayName";
			this.txtBaseLanguage.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
			this.txtBaseLanguage.Name = "txtBaseLanguage";
			this.txtBaseLanguage.Sorted = true;
			this.txtBaseLanguage.ValueMember = "Name";
			// 
			// label2
			// 
			resources.ApplyResources(this.label2, "label2");
			this.label2.Name = "label2";
			// 
			// label1
			// 
			resources.ApplyResources(this.label1, "label1");
			this.label1.Name = "label1";
			// 
			// label3
			// 
			resources.ApplyResources(this.label3, "label3");
			this.label3.Name = "label3";
			// 
			// settingsTabControl
			// 
			resources.ApplyResources(this.settingsTabControl, "settingsTabControl");
			this.settingsTabControl.Controls.Add(this.tpVariables);
			this.settingsTabControl.Controls.Add(this.tpBuild);
			this.settingsTabControl.Controls.Add(this.tpDictionaryGeneration);
			this.settingsTabControl.Name = "settingsTabControl";
			this.settingsTabControl.SelectedIndex = 0;
			// 
			// tpVariables
			// 
			this.tpVariables.Controls.Add(this.fcInstallationPath);
			this.tpVariables.Controls.Add(this.fcDictFolder);
			this.tpVariables.Controls.Add(this.btnCompare);
			this.tpVariables.Controls.Add(this.txtBaseLanguage);
			this.tpVariables.Controls.Add(this.label2);
			this.tpVariables.Controls.Add(this.label1);
			this.tpVariables.Controls.Add(this.label3);
			this.tpVariables.Controls.Add(this.txtInstallation);
			this.tpVariables.Controls.Add(this.txtDrive);
			resources.ApplyResources(this.tpVariables, "tpVariables");
			this.tpVariables.Name = "tpVariables";
			// 
			// fcDictFolder
			// 
			this.fcDictFolder.AllowEmptyEntry = true;
			resources.ApplyResources(this.fcDictFolder, "fcDictFolder");
			this.fcDictFolder.CheckExistence = true;
			this.fcDictFolder.Description = "Dictionary root path";
			this.fcDictFolder.EntryPath = "";
			this.fcDictFolder.IsFile = false;
			this.fcDictFolder.Name = "fcDictFolder";
			// 
			// btnCompare
			// 
			resources.ApplyResources(this.btnCompare, "btnCompare");
			this.btnCompare.Name = "btnCompare";
			this.btnCompare.Click += new System.EventHandler(this.OnCompareClick);
			// 
			// tpBuild
			// 
			this.tpBuild.Controls.Add(this.CkbSatellites);
			this.tpBuild.Controls.Add(this.CkbDictionaryBinPath);
			this.tpBuild.Controls.Add(this.groupBox3);
			resources.ApplyResources(this.tpBuild, "tpBuild");
			this.tpBuild.Name = "tpBuild";
			// 
			// CkbSatellites
			// 
			resources.ApplyResources(this.CkbSatellites, "CkbSatellites");
			this.CkbSatellites.Name = "CkbSatellites";
			this.CkbSatellites.CheckedChanged += new System.EventHandler(this.CkbSatellites_CheckedChanged);
			// 
			// CkbDictionaryBinPath
			// 
			resources.ApplyResources(this.CkbDictionaryBinPath, "CkbDictionaryBinPath");
			this.CkbDictionaryBinPath.Name = "CkbDictionaryBinPath";
			// 
			// groupBox3
			// 
			this.groupBox3.Controls.Add(this.cbDebug);
			this.groupBox3.Controls.Add(this.cbRelease);
			this.groupBox3.FlatStyle = System.Windows.Forms.FlatStyle.System;
			resources.ApplyResources(this.groupBox3, "groupBox3");
			this.groupBox3.Name = "groupBox3";
			this.groupBox3.TabStop = false;
			// 
			// cbDebug
			// 
			resources.ApplyResources(this.cbDebug, "cbDebug");
			this.cbDebug.Name = "cbDebug";
			// 
			// cbRelease
			// 
			resources.ApplyResources(this.cbRelease, "cbRelease");
			this.cbRelease.Name = "cbRelease";
			// 
			// tpDictionaryGeneration
			// 
			this.tpDictionaryGeneration.Controls.Add(this.groupBox1);
			resources.ApplyResources(this.tpDictionaryGeneration, "tpDictionaryGeneration");
			this.tpDictionaryGeneration.Name = "tpDictionaryGeneration";
			// 
			// groupBox1
			// 
			resources.ApplyResources(this.groupBox1, "groupBox1");
			this.groupBox1.Controls.Add(this.label8);
			this.groupBox1.Controls.Add(this.CkbIgnoreCase);
			this.groupBox1.Controls.Add(this.CkbIgnoreSpaces);
			this.groupBox1.Controls.Add(this.CkbIgnorePunctuation);
			this.groupBox1.FlatStyle = System.Windows.Forms.FlatStyle.System;
			this.groupBox1.Name = "groupBox1";
			this.groupBox1.TabStop = false;
			// 
			// label8
			// 
			resources.ApplyResources(this.label8, "label8");
			this.label8.Name = "label8";
			// 
			// CkbIgnoreCase
			// 
			resources.ApplyResources(this.CkbIgnoreCase, "CkbIgnoreCase");
			this.CkbIgnoreCase.BackColor = System.Drawing.Color.Lime;
			this.CkbIgnoreCase.Name = "CkbIgnoreCase";
			this.CkbIgnoreCase.UseVisualStyleBackColor = false;
			// 
			// CkbIgnoreSpaces
			// 
			resources.ApplyResources(this.CkbIgnoreSpaces, "CkbIgnoreSpaces");
			this.CkbIgnoreSpaces.BackColor = System.Drawing.Color.Yellow;
			this.CkbIgnoreSpaces.Name = "CkbIgnoreSpaces";
			this.CkbIgnoreSpaces.UseVisualStyleBackColor = false;
			// 
			// CkbIgnorePunctuation
			// 
			resources.ApplyResources(this.CkbIgnorePunctuation, "CkbIgnorePunctuation");
			this.CkbIgnorePunctuation.BackColor = System.Drawing.Color.Red;
			this.CkbIgnorePunctuation.Name = "CkbIgnorePunctuation";
			this.CkbIgnorePunctuation.UseVisualStyleBackColor = false;
			// 
			// fcInstallationPath
			// 
			this.fcInstallationPath.AllowEmptyEntry = true;
			resources.ApplyResources(this.fcInstallationPath, "fcInstallationPath");
			this.fcInstallationPath.CheckExistence = true;
			this.fcInstallationPath.Description = "Installation path";
			this.fcInstallationPath.EntryPath = "";
			this.fcInstallationPath.IsFile = false;
			this.fcInstallationPath.Name = "fcInstallationPath";
			this.fcInstallationPath.FileTextChanged += new System.EventHandler(this.fcInstallationPath_FileTextChanged);
			// 
			// EnvironmentSettingsForm
			// 
			resources.ApplyResources(this, "$this");
			this.Controls.Add(this.settingsTabControl);
			this.Controls.Add(this.btnOk);
			this.Controls.Add(this.btnCancel);
			this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedToolWindow;
			this.Name = "EnvironmentSettingsForm";
			this.ShowInTaskbar = false;
			this.Closing += new System.ComponentModel.CancelEventHandler(this.EnvironmentSettingsForm_Closing);
			this.Load += new System.EventHandler(this.EnvironmentSettingsForm_Load);
			this.settingsTabControl.ResumeLayout(false);
			this.tpVariables.ResumeLayout(false);
			this.tpVariables.PerformLayout();
			this.tpBuild.ResumeLayout(false);
			this.groupBox3.ResumeLayout(false);
			this.tpDictionaryGeneration.ResumeLayout(false);
			this.groupBox1.ResumeLayout(false);
			this.ResumeLayout(false);

		}
		#endregion

		private void fcInstallationPath_FileTextChanged(object sender, EventArgs e)
		{
			if (Directory.Exists(fcInstallationPath.EntryPath))
			{
				txtDrive.Text = Path.GetPathRoot(fcInstallationPath.EntryPath).TrimEnd(Path.DirectorySeparatorChar);
				txtInstallation.Text = Path.GetFileName(fcInstallationPath.EntryPath);
			}
		}

	}
}
