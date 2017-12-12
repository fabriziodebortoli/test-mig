using System;
using System.Collections;
using System.Data;
using System.Data.SqlClient;
using System.Diagnostics;
using System.Drawing;
using System.Globalization;
using System.IO;
using System.Reflection;
using System.Windows.Forms;
using Microarea.TaskBuilderNet.Core.Applications;
using Microarea.TaskBuilderNet.Core.NameSolver;
using Microarea.TaskBuilderNet.Interfaces;
using Microarea.TaskBuilderNet.UI.EasyLookCustomization;

namespace Microarea.Console.Plugin.EasyLookCustomizer
{
	/// <summary>
	/// Summary description for EasyLookCustomizerControl.
	/// </summary>
	public partial class EasyLookCustomizerControl : System.Windows.Forms.UserControl
	{
		private SqlConnection			currentConnection = null;
		private string					currentConnectionString = String.Empty;
		private int						currentLoginId = -1;
		private int						currentCompanyId = -1;
		private EasyLookCustomSettings	currentSettings = null;
		private bool					modifiedLayout = false;

		/// <summary> 
		/// Required designer variable.
		/// </summary>
		private System.ComponentModel.Container components = null;

		public delegate void SettingsExistenceChangedEventHandler (object sender, bool exist);
		public event SettingsExistenceChangedEventHandler SettingsExistenceChanged;
		
		//--------------------------------------------------------------------------------------------------------------------------------
		public EasyLookCustomizerControl()
		{
			// This call is required by the Windows.Forms Form Designer.
			InitializeComponent();

			FontFamilyComboBox.FillWithAllInstalledFont();
		}

	
		#region EasyLookCustomizerControl private methods
		
		//--------------------------------------------------------------------------------------------------------------------------------
		private void SetControlsValuesFromCustomSettings(bool layoutOnly)
		{			
			LogoImageURLRichTextBox.Text = (currentSettings != null) ? currentSettings.LogoImageURL : EasyLookCustomSettings.DefaultLogoImageURL;
			if (LogoImageURLRichTextBox.Text == null || LogoImageURLRichTextBox.Text == String.Empty)
				LogoImageURLRichTextBox.Text = Strings.DefaultImageURLDescription;
			AppPanelBkgndColorWebColorPicker.Color = (currentSettings != null) ? Color.FromArgb(currentSettings.AppPanelBkgndColor) : EasyLookCustomSettings.DefaultAppPanelBkgndColor;
			GroupsPanelBkgndColorRadioButton.Checked = (currentSettings != null && currentSettings.IsGroupsPanelBkgndColorDefined);
			GroupsPanelBkgndColorWebColorPicker.Color = (currentSettings != null) ? Color.FromArgb(currentSettings.GroupsPanelBkgndColor) : EasyLookCustomSettings.DefaultGroupsPanelBkgndColor;
			GroupsPanelBkgndImageRadioButton.Checked = !GroupsPanelBkgndColorRadioButton.Checked;
			GroupsPanelBkgndImageURLRichTextBox.Text = (currentSettings != null) ? currentSettings.GroupsPanelBkgndImageURL : EasyLookCustomSettings.DefaultGroupsPanelBkgndImageURL;
			if (GroupsPanelBkgndImageURLRichTextBox.Text == null || GroupsPanelBkgndImageURLRichTextBox.Text == String.Empty)
				GroupsPanelBkgndImageURLRichTextBox.Text = Strings.DefaultImageURLDescription;
			MenuTreeBkgndColorWebColorPicker.Color = (currentSettings != null) ? Color.FromArgb(currentSettings.MenuTreeBkgndColor) : EasyLookCustomSettings.DefaultMenuTreeBkgndColor;
			CommandListBkgndColorWebColorPicker.Color = (currentSettings != null) ? Color.FromArgb(currentSettings.CommandListBkgndColor) : EasyLookCustomSettings.DefaultCommandListBkgndColor;
			FontFamilyComboBox.SelectedFontName = (currentSettings != null) ? currentSettings.FontFamily : EasyLookCustomSettings.DefaultFontFamily;	
			AllUsersTitleColorWebColorPicker.Color = (currentSettings != null) ? Color.FromArgb(currentSettings.AllUsersReportTitleColor) : EasyLookCustomSettings.DefaultAllUsersReportTitleColor;
			CurrentUserTitleColorWebColorPicker.Color = (currentSettings != null) ? Color.FromArgb(currentSettings.CurrentUserReportTitleColor) : EasyLookCustomSettings.DefaultCurrentUserReportTitleColor;

			ResetLayoutDefaultsButton.Enabled = (currentSettings != null) ? !currentSettings.ApplyDefaultLayout : false;
			
			if (layoutOnly)
				return;

			MaxWrmHistoryNumNumericUpDown.Value  = (decimal)((currentSettings != null) ? currentSettings.MaxWrmHistoryNum : EasyLookCustomSettings.DefaultMaxWrmHistoryNumber);
			WrmHistoryAutoDelEnabledCheckBox.Checked = (currentSettings != null && currentSettings.IsWrmHistoryAutoDelEnabled);
			WrmHistoryAutoDelPeriodNumericUpDown.Value = (decimal)((currentSettings != null) ? currentSettings.WrmHistoryAutoDelPeriod : (UInt16)1);
			
			SetWrmHistoryAutoDelType();
			
			UpdateWrmHistoryAutoDelState();
			
			WrmHistoryPanel.Visible = true;
		}

		//--------------------------------------------------------------------------------------------------------------------------------
		private void SetControlsValuesFromCustomSettings()
		{
			SetControlsValuesFromCustomSettings(false);
		}
		
		//--------------------------------------------------------------------------------------------------------------------------------
		private void UpdateCurrentEasyLookCustomSettings()
		{
			if (currentSettings == null)
				return;

			string logoImageURL = LogoImageURLRichTextBox.Text;
			if (logoImageURL != null && logoImageURL != String.Empty && String.Compare(logoImageURL, Strings.DefaultImageURLDescription) != 0)
				currentSettings.LogoImageURL = logoImageURL;
			else
				currentSettings.LogoImageURL = EasyLookCustomSettings.DefaultLogoImageURL;

			Color appPanelBkgndColor = AppPanelBkgndColorWebColorPicker.Color;
			if (!appPanelBkgndColor.Equals(Color.Empty))
				currentSettings.AppPanelBkgndColor = appPanelBkgndColor.ToArgb();
			else
				currentSettings.AppPanelBkgndColor = EasyLookCustomSettings.DefaultAppPanelBkgndColor.ToArgb();
			
			if (GroupsPanelBkgndImageRadioButton.Checked)
			{
				currentSettings.IsGroupsPanelBkgndColorDefined = false;

				string groupsPanelBkgndImageURL = GroupsPanelBkgndImageURLRichTextBox.Text;
				if (groupsPanelBkgndImageURL != null && groupsPanelBkgndImageURL != String.Empty && String.Compare(groupsPanelBkgndImageURL, Strings.DefaultImageURLDescription) != 0)
					currentSettings.GroupsPanelBkgndImageURL = groupsPanelBkgndImageURL;
				else
					currentSettings.GroupsPanelBkgndImageURL = EasyLookCustomSettings.DefaultGroupsPanelBkgndImageURL;
				
				currentSettings.GroupsPanelBkgndColor = EasyLookCustomSettings.DefaultGroupsPanelBkgndColor.ToArgb();
			}
			else
			{
				currentSettings.IsGroupsPanelBkgndColorDefined = true;

				currentSettings.GroupsPanelBkgndImageURL = EasyLookCustomSettings.DefaultGroupsPanelBkgndImageURL;
				
				Color groupsPanelBkgndColor = GroupsPanelBkgndColorWebColorPicker.Color;
				if (!groupsPanelBkgndColor.Equals(Color.Empty))
					currentSettings.GroupsPanelBkgndColor = groupsPanelBkgndColor.ToArgb();
				else
					currentSettings.GroupsPanelBkgndColor = EasyLookCustomSettings.DefaultGroupsPanelBkgndColor.ToArgb();
			}

			Color menuTreeBkgndColor = MenuTreeBkgndColorWebColorPicker.Color;
			if (!menuTreeBkgndColor.Equals(Color.Empty))
				currentSettings.MenuTreeBkgndColor = menuTreeBkgndColor.ToArgb();
			else
				currentSettings.MenuTreeBkgndColor = EasyLookCustomSettings.DefaultMenuTreeBkgndColor.ToArgb();

			Color commandListBkgndColor = CommandListBkgndColorWebColorPicker.Color;
			if (!commandListBkgndColor.Equals(Color.Empty))
				currentSettings.CommandListBkgndColor = commandListBkgndColor.ToArgb();
			else
				currentSettings.CommandListBkgndColor = EasyLookCustomSettings.DefaultCommandListBkgndColor.ToArgb();

			if (FontFamilyComboBox.SelectedItem != null)
				currentSettings.FontFamily = FontFamilyComboBox.SelectedFontName;
			else
				currentSettings.FontFamily = EasyLookCustomSettings.DefaultFontFamily;

			Color allUsersReportTitleColor = AllUsersTitleColorWebColorPicker.Color;
			if (!allUsersReportTitleColor.Equals(Color.Empty))
				currentSettings.AllUsersReportTitleColor = allUsersReportTitleColor.ToArgb();
			else
				currentSettings.AllUsersReportTitleColor = EasyLookCustomSettings.DefaultAllUsersReportTitleColor.ToArgb();

			Color currentUserReportTitleColor = CurrentUserTitleColorWebColorPicker.Color;
			if (!currentUserReportTitleColor.Equals(Color.Empty))
				currentSettings.CurrentUserReportTitleColor = currentUserReportTitleColor.ToArgb();
			else
				currentSettings.CurrentUserReportTitleColor = EasyLookCustomSettings.DefaultCurrentUserReportTitleColor.ToArgb();

			currentSettings.MaxWrmHistoryNum = Convert.ToInt32(MaxWrmHistoryNumNumericUpDown.Value);
			
			currentSettings.IsWrmHistoryAutoDelEnabled = WrmHistoryAutoDelEnabledCheckBox.Checked;

			currentSettings.WrmHistoryAutoDelPeriod = Convert.ToUInt16(WrmHistoryAutoDelPeriodNumericUpDown.Value);
			
			switch (WrmHistoryAutoDelTypeComboBox.SelectedIndex)
			{
				case 0:
					currentSettings.AutoDelWrmHistoryYearly = true;
					break;
				case 1:
					currentSettings.AutoDelWrmHistoryMonthly = true;
					break;
				case 2:
					currentSettings.AutoDelWrmHistoryWeekly = true;
					break;
				case 3:
					currentSettings.AutoDelWrmHistoryDaily = true;
					break;
				default:
					currentSettings.AutoDelWrmHistoryUndefined = true;
					break;

			}
		}

		//--------------------------------------------------------------------------------------------------------------------------------
		private void RefreshCustomSettings(bool layoutOnly)
		{
			SetControlsValuesFromCustomSettings(layoutOnly);
		}

		//--------------------------------------------------------------------------------------------------------------------------------
		private void RefreshCustomSettings()
		{
			RefreshCustomSettings(false);
		}

		//--------------------------------------------------------------------------------------------------------------------------------
		private void UpdateWrmHistoryAutoDelState()
		{
			WrmHistoryAutoDelOlderThanLabel.Enabled = WrmHistoryAutoDelEnabledCheckBox.Checked;
			WrmHistoryAutoDelPeriodNumericUpDown.Enabled = WrmHistoryAutoDelEnabledCheckBox.Checked;
			WrmHistoryAutoDelTypeComboBox.Enabled = WrmHistoryAutoDelEnabledCheckBox.Checked;
		}

		//--------------------------------------------------------------------------------------------------------------------------------
		private void SetWrmHistoryAutoDelType()
		{
			if (currentSettings == null || currentSettings.AutoDelWrmHistoryUndefined)
			{
				WrmHistoryAutoDelTypeComboBox.SelectedIndex = -1;
				return;
			}
			if (currentSettings.AutoDelWrmHistoryYearly)
				WrmHistoryAutoDelTypeComboBox.SelectedIndex = 0;
			else if (currentSettings.AutoDelWrmHistoryMonthly)
				WrmHistoryAutoDelTypeComboBox.SelectedIndex = 1;
			else if (currentSettings.AutoDelWrmHistoryWeekly)
				WrmHistoryAutoDelTypeComboBox.SelectedIndex = 2;
			else if (currentSettings.AutoDelWrmHistoryDaily)
				WrmHistoryAutoDelTypeComboBox.SelectedIndex = 3;
			else
				WrmHistoryAutoDelTypeComboBox.SelectedIndex = -1;
		}

		#endregion
			
		#region EasyLookCustomizerControl event handlers

		//--------------------------------------------------------------------------------------------------------------------------------
		protected override void OnLoad(System.EventArgs e)
		{
			// Invoke base class implementation
			base.OnLoad(e);
			
			Stream bitmapStream = Assembly.GetExecutingAssembly().GetManifestResourceStream("Microarea.Console.Plugin.EasyLookCustomizer.Bitmaps.EasyLookCustomizer.bmp");
			if (bitmapStream != null)
			{
				System.Drawing.Bitmap bitmap = new Bitmap(bitmapStream);
				if (bitmap != null)
				{
					bitmap.MakeTransparent(Color.Lavender);
					EasyLookCustomizerPictureBox.Image = bitmap;
				}
			}

			bitmapStream = Assembly.GetExecutingAssembly().GetManifestResourceStream("Microarea.Console.Plugin.EasyLookCustomizer.Bitmaps.Snapshot.bmp");
			if (bitmapStream != null)
			{
				System.Drawing.Bitmap bitmap = new Bitmap(bitmapStream);
				if (bitmap != null)
				{
					bitmap.MakeTransparent(Color.Lavender);
					SnapshotPictureBox.Image = bitmap;
				}
			}

		}

		//--------------------------------------------------------------------------------------------------------------------------------
		private void LogoImageURLRichTextBox_Validating(object sender, System.ComponentModel.CancelEventArgs e)
		{
			if (LogoImageURLRichTextBox.Text != null && LogoImageURLRichTextBox.Text != String.Empty)
			{
				Uri uri = EasyLookCustomSettings.GetValidURI(LogoImageURLRichTextBox.Text);
				if (uri != null)
				{
					LogoImageURLRichTextBox.Text = uri.AbsoluteUri;
					return;
				}
				MessageBox.Show(String.Format(Strings.InvalidURIErrorMsgFmt, LogoImageURLRichTextBox.Text));
				LogoImageURLRichTextBox.Text = String.Empty;
				if (LogoImageURLRichTextBox.CanFocus)
					LogoImageURLRichTextBox.Focus();
			}
		}

		//--------------------------------------------------------------------------------------------------------------------------------
		private void LogoImageURLRichTextBox_Validated(object sender, System.EventArgs e)
		{
			if 
				(
				currentSettings != null &&
				String.Compare(currentSettings.LogoImageURL, LogoImageURLRichTextBox.Text, true, CultureInfo.InvariantCulture) != 0
				)
				modifiedLayout = true;

			if (!ResetLayoutDefaultsButton.Enabled)
				ResetLayoutDefaultsButton.Enabled = modifiedLayout && (String.Compare(EasyLookCustomSettings.DefaultLogoImageURL, LogoImageURLRichTextBox.Text, true, CultureInfo.InvariantCulture) != 0);
		}

		//--------------------------------------------------------------------------------------------------------------------------------
		private void AppPanelBkgndColorWebColorPicker_SelectedColorChanged(object sender, System.EventArgs e)
		{
			if 
				(
				currentSettings != null &&
				currentSettings.AppPanelBkgndColor != AppPanelBkgndColorWebColorPicker.Color.ToArgb()
				)
				modifiedLayout = true;

			if (!ResetLayoutDefaultsButton.Enabled)
				ResetLayoutDefaultsButton.Enabled = modifiedLayout && EasyLookCustomSettings.DefaultAppPanelBkgndColor.ToArgb() != AppPanelBkgndColorWebColorPicker.Color.ToArgb();
		}

		//--------------------------------------------------------------------------------------------------------------------------------
		private void GroupsPanelBkgndRadioButton_CheckedChanged(object sender, System.EventArgs e)
		{
			GroupsPanelBkgndImageURLRichTextBox.Enabled = GroupsPanelBkgndImageRadioButton.Checked;
			GroupsPanelBkgndColorWebColorPicker.Enabled = GroupsPanelBkgndColorRadioButton.Checked;
		}

		//--------------------------------------------------------------------------------------------------------------------------------
		private void GroupsPanelBkgndImageURLRichTextBox_Validating(object sender, System.ComponentModel.CancelEventArgs e)
		{
			if (GroupsPanelBkgndImageURLRichTextBox.Text != null && GroupsPanelBkgndImageURLRichTextBox.Text != String.Empty)
			{
				Uri uri = EasyLookCustomSettings.GetValidURI(GroupsPanelBkgndImageURLRichTextBox.Text);
				if (uri != null)
				{
					GroupsPanelBkgndImageURLRichTextBox.Text = uri.AbsoluteUri;
					return;
				}
				MessageBox.Show(String.Format(Strings.InvalidURIErrorMsgFmt, GroupsPanelBkgndImageURLRichTextBox.Text));
				GroupsPanelBkgndImageURLRichTextBox.Text = String.Empty;
				if (GroupsPanelBkgndImageURLRichTextBox.CanFocus)
					GroupsPanelBkgndImageURLRichTextBox.Focus();
			}
		}

		//--------------------------------------------------------------------------------------------------------------------------------
		private void GroupsPanelBkgndImageURLRichTextBox_Validated(object sender, System.EventArgs e)
		{
			if 
				(
				currentSettings != null &&
				String.Compare(currentSettings.GroupsPanelBkgndImageURL, GroupsPanelBkgndImageURLRichTextBox.Text, true, CultureInfo.InvariantCulture) != 0
				)
				modifiedLayout = true;

			if (!ResetLayoutDefaultsButton.Enabled)
				ResetLayoutDefaultsButton.Enabled = modifiedLayout && (String.Compare(EasyLookCustomSettings.DefaultGroupsPanelBkgndImageURL, GroupsPanelBkgndImageURLRichTextBox.Text, true, CultureInfo.InvariantCulture) != 0);
		}

		//--------------------------------------------------------------------------------------------------------------------------------
		private void GroupsPanelBkgndColorWebColorPicker_SelectedColorChanged(object sender, System.EventArgs e)
		{
			if 
				(
				currentSettings != null &&
				currentSettings.GroupsPanelBkgndColor != GroupsPanelBkgndColorWebColorPicker.Color.ToArgb()
				)
				modifiedLayout = true;

			if (!ResetLayoutDefaultsButton.Enabled)
				ResetLayoutDefaultsButton.Enabled = modifiedLayout && EasyLookCustomSettings.DefaultGroupsPanelBkgndColor.ToArgb() != GroupsPanelBkgndColorWebColorPicker.Color.ToArgb();
		}

		//--------------------------------------------------------------------------------------------------------------------------------
		private void MenuTreeBkgndColorWebColorPicker_SelectedColorChanged(object sender, System.EventArgs e)
		{
			if 
				(
				currentSettings != null &&
				currentSettings.MenuTreeBkgndColor != MenuTreeBkgndColorWebColorPicker.Color.ToArgb()
				)
				modifiedLayout = true;

			if (!ResetLayoutDefaultsButton.Enabled)
				ResetLayoutDefaultsButton.Enabled = modifiedLayout && EasyLookCustomSettings.DefaultMenuTreeBkgndColor.ToArgb() != MenuTreeBkgndColorWebColorPicker.Color.ToArgb();
		}

		//--------------------------------------------------------------------------------------------------------------------------------
		private void CommandListBkgndColorWebColorPicker_SelectedColorChanged(object sender, System.EventArgs e)
		{
			if 
				(
				currentSettings != null &&
				currentSettings.CommandListBkgndColor != CommandListBkgndColorWebColorPicker.Color.ToArgb()
				)
				modifiedLayout = true;

			if (!ResetLayoutDefaultsButton.Enabled)
				ResetLayoutDefaultsButton.Enabled = modifiedLayout && EasyLookCustomSettings.DefaultCommandListBkgndColor.ToArgb() != CommandListBkgndColorWebColorPicker.Color.ToArgb();
		}

		//--------------------------------------------------------------------------------------------------------------------------------
		private void FontFamilyComboBox_SelectedIndexChanged(object sender, System.EventArgs e)
		{
			if 
				(
				currentSettings != null &&
				String.Compare(currentSettings.FontFamily, FontFamilyComboBox.Text) != 0
				)
				modifiedLayout = true;

			if (!ResetLayoutDefaultsButton.Enabled)
				ResetLayoutDefaultsButton.Enabled = modifiedLayout && (String.Compare(EasyLookCustomSettings.DefaultFontFamily, FontFamilyComboBox.Text) != 0);
		}

		//--------------------------------------------------------------------------------------------------------------------------------
		private void AllUsersTitleColorWebColorPicker_SelectedColorChanged(object sender, System.EventArgs e)
		{
			if 
				(
				currentSettings != null &&
				currentSettings.AllUsersReportTitleColor != AllUsersTitleColorWebColorPicker.Color.ToArgb()
				)
				modifiedLayout = true;

			if (!ResetLayoutDefaultsButton.Enabled)
				ResetLayoutDefaultsButton.Enabled = modifiedLayout && EasyLookCustomSettings.DefaultAllUsersReportTitleColor.ToArgb() != AllUsersTitleColorWebColorPicker.Color.ToArgb();
		}

		//--------------------------------------------------------------------------------------------------------------------------------
		private void CurrentUserTitleColorWebColorPicker_SelectedColorChanged(object sender, System.EventArgs e)
		{
			if 
				(
				currentSettings != null &&
				currentSettings.CurrentUserReportTitleColor != CurrentUserTitleColorWebColorPicker.Color.ToArgb()
				)
				modifiedLayout = true;

			if (!ResetLayoutDefaultsButton.Enabled)
				ResetLayoutDefaultsButton.Enabled = modifiedLayout && EasyLookCustomSettings.DefaultCurrentUserReportTitleColor.ToArgb() != CurrentUserTitleColorWebColorPicker.Color.ToArgb();
		}

		//--------------------------------------------------------------------------------------------------------------------------------
		private void WrmHistoryAutoDelEnabledCheckBox_CheckedChanged(object sender, System.EventArgs e)
		{
			UpdateWrmHistoryAutoDelState();
		}

		//--------------------------------------------------------------------------------------------------------------------------------
		private void CurrentRunnedReportsDataGrid_VisibleChanged(object sender, System.EventArgs e)
		{
			DelRunnedReportsButton.Visible = CurrentRunnedReportsDataGrid.Visible;
		}

		//--------------------------------------------------------------------------------------------------------------------------------
		private void CurrentRunnedReportsDataGrid_RowsSelectionChanged(object sender, System.EventArgs e)
		{
			DelRunnedReportsButton.Enabled = (CurrentRunnedReportsDataGrid.SelectedRows != null && CurrentRunnedReportsDataGrid.SelectedRows.Count > 0);
		}

		//--------------------------------------------------------------------------------------------------------------------------------
		private void DelRunnedReportsButton_Click(object sender, System.EventArgs e)
		{
			CurrentRunnedReportsDataGrid.DeleteSelectedReports();
		}

		//--------------------------------------------------------------------------------------------------------------------------------
		private void ResetDefaultsButton_Click(object sender, System.EventArgs e)
		{
			// Per ripristinare le impostazioni di layout predefinite è sufficiente cancellare
			// reinizializzare currentSettings
			if (currentSettings != null)
			{
				currentSettings.ResetLayoutDefaults();
				RefreshCustomSettings(true);
				modifiedLayout = false;
			}
			ResetLayoutDefaultsButton.Enabled = false;
		}

		#endregion

		#region EasyLookCustomizerControl public properties
		
		//--------------------------------------------------------------------------------------------------------------------------------
		public string ConnectionString 
		{
			get{ return currentConnectionString; } 
			set
			{
				try
				{
					CloseConnection();

					currentConnectionString = value;
					if (currentConnectionString == null || currentConnectionString == String.Empty)
						return;

					currentConnection = new SqlConnection(currentConnectionString);
					
					// The Open method uses the information in the ConnectionString
					// property to contact the data source and establish an open connection
					currentConnection.Open();

					CurrentRunnedReportsDataGrid.Connection = currentConnection;
				}
				catch (SqlException e)
				{
					MessageBox.Show(String.Format(Strings.ConnectionErrorMsgFmt, e.Message));

					currentConnection = null;
					currentConnectionString = String.Empty;
				}
			}
		}
		
		//--------------------------------------------------------------------------------------------------------------------------------
		public bool IsConnectionOpen { get{ return (currentConnection != null) && ((currentConnection.State & ConnectionState.Open) == ConnectionState.Open); } }

		//--------------------------------------------------------------------------------------------------------------------------------
		public bool CurrentSettingsExist 
		{
			get{ return IsConnectionOpen ? EasyLookCustomSettings.Exists(currentConnection, currentCompanyId, currentLoginId) : false; } 
		}
		
		#endregion

		#region EasyLookCustomizerControl public methods
		
		//--------------------------------------------------------------------------------------------------------
		public void CloseConnection()
		{
			if (currentConnection != null)
			{
				if (IsConnectionOpen)
					currentConnection.Close();
			
				currentConnection.Dispose();
			}

			currentConnection = null;
			currentConnectionString = String.Empty;

			CurrentRunnedReportsDataGrid.Connection = null;
		}

		//--------------------------------------------------------------------------------------------------------------------------------
		public bool SetCurrentAuthentication(int companyId, int loginId, PathFinder pathFinder)
		{
			currentCompanyId = companyId;
			currentLoginId = loginId;

			currentSettings = null;
			
			WrmHistoryPanel.Visible = false;

			string loginName = String.Empty;
			string companyName = String.Empty;

			CaptionLabel.Text = String.Empty;

			if (currentConnection == null || currentCompanyId == -1)
				return true;

			if (!EasyLookCustomSettings.GetLoginDataFromIds(currentConnection, currentCompanyId, currentLoginId, out loginName, out companyName))
				return false;

			currentSettings = new EasyLookCustomSettings(pathFinder, currentCompanyId, currentLoginId);

			CurrentRunnedReportsDataGrid.SetAuthentication(currentCompanyId, currentLoginId);

			if (currentLoginId == -1)
				CaptionLabel.Text = String.Format(Strings.AllUsersSettingsCaptionFmt, companyName);
			else
				CaptionLabel.Text = String.Format(Strings.UserSettingsCaptionFmt, loginName, companyName);

			RefreshCustomSettings();

			modifiedLayout = false;

			if (SettingsExistenceChanged != null)
				SettingsExistenceChanged(this, CurrentSettingsExist);

			return true;
		}

		//--------------------------------------------------------------------------------------------------------------------------------
		public void Clear()
		{
			currentSettings = null;

			SetControlsValuesFromCustomSettings();

			CurrentRunnedReportsDataGrid.Clear();
		}
		
		//--------------------------------------------------------------------------------------------------------------------------------
		public void SaveCurrentSettings()
		{
			if (currentSettings == null)
				return;

			UpdateCurrentEasyLookCustomSettings();

			bool inserted = false;
			if (!currentSettings.Save(ref inserted))
			{
				MessageBox.Show(this, Strings.SettingsSaveFailedErrorMsg);
				return;
			}
	
			modifiedLayout = false;
		
			if (inserted && SettingsExistenceChanged != null)
				SettingsExistenceChanged(this, true);
		}

		//--------------------------------------------------------------------------------------------------------------------------------
		public void DeleteCurrentSettings()
		{
			if (currentSettings == null)
				return;

			if (!currentSettings.Delete())
			{
				MessageBox.Show(this, Strings.SettingsDeletionFailedErrorMsg);
				return;
			}

			RefreshCustomSettings();

			modifiedLayout = false;
	
			if (SettingsExistenceChanged != null)
				SettingsExistenceChanged(this, false);
		}

		#endregion

		private void LogoImageOpenFileButton_Click(object sender, System.EventArgs e)
		{
		
			SelectImageFile(LogoImageURLRichTextBox);
			this.currentSettings.LogoImageFullPath = LogoImageOpenFileDialog.FileName;
		}

		//---------------------------------------------------------------------------
		private void SelectImageFile(RichTextBox textBox)
		{
			LogoImageOpenFileDialog.InitialDirectory = "c:\\" ;
			LogoImageOpenFileDialog.Filter = "Image Files(*.BMP;*.JPG;*.GIF)|*.BMP;*.JPG;*.GIF";
			LogoImageOpenFileDialog.FilterIndex = 2 ;
			LogoImageOpenFileDialog.RestoreDirectory = true ;

			if(LogoImageOpenFileDialog.ShowDialog() == DialogResult.OK)
			{
				if((LogoImageOpenFileDialog.OpenFile())!= null)
				{
					string filename = LogoImageOpenFileDialog.FileName.Substring(LogoImageOpenFileDialog.FileName.LastIndexOf("\\") + 1);
					textBox.Text = filename;
				}
			}
		}

		private void ApplicationImageOpenFileButton_Click(object sender, System.EventArgs e)
		{
			SelectImageFile(GroupsPanelBkgndImageURLRichTextBox);
			this.currentSettings.GroupsPanelBkgndImageFullPath = LogoImageOpenFileDialog.FileName;
		}



		#region RunnedReportsDataGrid Class

		//=========================================================================
		public class RunnedReportsDataGrid : System.Windows.Forms.DataGrid
		{
			private const string RunnedReportsDataTableName		= "RunnedReportsDataTable";
			private const string UserColumnName					= "User";
			private const string OwnerReportNamespaceColumnName = "OwnerReportNamespace";
			private const string TimeStampColumnName			= "TimeStamp";
			private const string DescriptionColumnName			= "Description";
			private const string RunnedReportColumnName			= "RunnedReport";

			private const int minimumDataGridStringColumnWidth = 240;
			
			private SqlConnection	connection = null;
			private RunnedReportMng	runnedReportManager = null;

			public event System.EventHandler RowsSelectionChanged;

			#region RunnedReportsDataGrid protected overridden methods

			//---------------------------------------------------------------------------
			protected override void OnCreateControl()
			{	
				// Invoke base class implementation
				base.OnCreateControl();

				this.ResizeRedraw = true;
				this.VertScrollBar.Visible = false;
				this.VertScrollBar.VisibleChanged += new System.EventHandler(this.VertScrollBar_VisibleChanged);
			}

			//---------------------------------------------------------------------------
			protected override void OnResize(EventArgs e)
			{	
				// Invoke base class implementation
				base.OnResize(e);

				AdjustLastColumnWidth();	
			}

			//---------------------------------------------------------------------------
			protected override void OnSizeChanged(EventArgs e)
			{	
				// Invoke base class implementation
				base.OnSizeChanged(e);

				AdjustLastColumnWidth();
			}

			//---------------------------------------------------------------------------
			protected new void OnRowHeaderClick(EventArgs e)
			{
				int previouslySelectedRowIndex = this.CurrentRowIndex;

				// Invoke base class implementation
				base.OnRowHeaderClick(e);

				if (previouslySelectedRowIndex != this.CurrentRowIndex && RowsSelectionChanged != null)
					RowsSelectionChanged(this, System.EventArgs.Empty);

			}
			
			//---------------------------------------------------------------------------
			// Override the OnMouseDown event to select the whole row
			// when the user clicks anywhere on a row.
			protected override void OnMouseDown(MouseEventArgs e) 
			{
				// Get the HitTestInfo to return the row and pass
				// that value to the IsSelected property of the DataGrid.
				DataGrid.HitTestInfo hit = this.HitTest(e.X, e.Y);

				if 
					(
						this.DataSource == null || 
						!(this.DataSource is DataTable) ||
						String.Compare(((DataTable)this.DataSource).TableName, RunnedReportsDataTableName) != 0 ||
						((DataTable)this.DataSource).Rows.Count == 0 ||
						hit.Type != DataGrid.HitTestType.Cell || 
						hit.Row < 0
					)
				{
					// Invoke base class implementation
					base.OnMouseDown(e);
					return;
				}
			
				if (!IsSelected(hit.Row))
				{
					int previouslySelectedRowIndex = this.CurrentRowIndex;
					if 
						(
						previouslySelectedRowIndex >= 0 && 
						previouslySelectedRowIndex != hit.Row
						)
					{
						if (Control.ModifierKeys == Keys.Shift)
						{
							int startSelIndex = Math.Min(hit.Row, previouslySelectedRowIndex);
							int endSelIndex = Math.Max(hit.Row, previouslySelectedRowIndex);
							for (int i = startSelIndex; i <= endSelIndex; i++)
								this.Select(i);
						}
						else if 
							(
							Control.ModifierKeys != Keys.Control &&
							IsSelected(previouslySelectedRowIndex)
							)
						{
							for (int i = 0; i < ((DataTable)this.DataSource).Rows.Count; i++)
								this.UnSelect(i);
						}
					}

					this.Select(hit.Row);
					this.CurrentRowIndex = hit.Row;
				}

				if (RowsSelectionChanged != null)
					RowsSelectionChanged(this, System.EventArgs.Empty);
			}
			
			#endregion

			#region RunnedReportsDataGrid event handlers
			//--------------------------------------------------------------------------
			private void VertScrollBar_VisibleChanged(object sender, EventArgs e)
			{	
				if (sender != this.VertScrollBar)
					return;
			
				AdjustLastColumnWidth();
			}
		
			//--------------------------------------------------------------------------------------------------------------------------------
			private void GridColumn_WidthChanged(object sender, System.EventArgs e)
			{
				AdjustLastColumnWidth();
			}
			
			#endregion
			
			#region RunnedReportsDataGrid private methods

			//--------------------------------------------------------------------------------------------------------------------------------
			private void SetRunnedReportsList(ArrayList aRunnedReportsList)
			{
				this.Visible = false;

				if 
					(
					this.DataSource == null || 
					!(this.DataSource is DataTable) ||
					String.Compare(((DataTable)this.DataSource).TableName, RunnedReportsDataTableName) != 0
					)
				{
					this.DataSource = new DataTable(RunnedReportsDataTableName);
					((DataTable)this.DataSource).Columns.Add(UserColumnName, Type.GetType("System.String"));
					((DataTable)this.DataSource).Columns.Add(OwnerReportNamespaceColumnName, Type.GetType("System.String"));
					((DataTable)this.DataSource).Columns.Add(TimeStampColumnName, Type.GetType("System.DateTime"));
					((DataTable)this.DataSource).Columns.Add(DescriptionColumnName, Type.GetType("System.String"));
					((DataTable)this.DataSource).Columns.Add(RunnedReportColumnName, typeof(RunnedReport));
				}
				
				((DataTable)this.DataSource).Rows.Clear();

				if (aRunnedReportsList != null && aRunnedReportsList.Count > 0)
				{				
					foreach (RunnedReport aRunnedReport in aRunnedReportsList)
					{
						DataRow row = ((DataTable)this.DataSource).NewRow();
						row[UserColumnName]					= aRunnedReport.User;
						row[OwnerReportNamespaceColumnName]	= aRunnedReport.OwnerReportNamespace;
						row[TimeStampColumnName]			= aRunnedReport.TimeStamp;
						row[DescriptionColumnName]			= aRunnedReport.Description;
						row[RunnedReportColumnName]			= aRunnedReport;
						
						((DataTable)this.DataSource).Rows.Add(row);
					}
				}

				UpdateVisibility();
			}

			//---------------------------------------------------------------------------
			private void UpdateVisibility()
			{	
				if 
					(
					this.DataSource == null ||
					!(this.DataSource is DataTable) ||
					String.Compare(((DataTable)this.DataSource).TableName, RunnedReportsDataTableName) != 0 ||
					((DataTable)this.DataSource).Rows.Count == 0
					)
				{
					this.Visible = false;
					return;
				}

				if (this.TableStyles == null || this.TableStyles.Count == 0)
					SetTableStyle();
				
				this.Visible = true;

				AdjustLastColumnWidth();
			}

			//--------------------------------------------------------------------------------------------------------------------------------
			private void SetTableStyle()
			{
				this.TableStyles.Clear();

				System.Windows.Forms.DataGridTableStyle runnedReportsDataGridTableStyle = new System.Windows.Forms.DataGridTableStyle();
				runnedReportsDataGridTableStyle.MappingName = RunnedReportsDataTableName;
				runnedReportsDataGridTableStyle.GridLineStyle = System.Windows.Forms.DataGridLineStyle.Solid;
				runnedReportsDataGridTableStyle.RowHeadersVisible = true;
				runnedReportsDataGridTableStyle.ColumnHeadersVisible = true;
				runnedReportsDataGridTableStyle.HeaderFont = new System.Drawing.Font("Verdana", 8.25F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((System.Byte)(0)));
				runnedReportsDataGridTableStyle.PreferredRowHeight = runnedReportsDataGridTableStyle.HeaderFont.Height;
				runnedReportsDataGridTableStyle.PreferredColumnWidth = 100;
				runnedReportsDataGridTableStyle.ReadOnly = true;
				runnedReportsDataGridTableStyle.RowHeaderWidth = 14;
				runnedReportsDataGridTableStyle.AlternatingBackColor = this.AlternatingBackColor;
				runnedReportsDataGridTableStyle.BackColor = this.BackColor;
				runnedReportsDataGridTableStyle.ForeColor = this.ForeColor;
				runnedReportsDataGridTableStyle.GridLineStyle = this.GridLineStyle;
				runnedReportsDataGridTableStyle.GridLineColor = this.GridLineColor;
				runnedReportsDataGridTableStyle.HeaderBackColor = this.HeaderBackColor;
				runnedReportsDataGridTableStyle.HeaderForeColor = this.HeaderForeColor;
				runnedReportsDataGridTableStyle.SelectionBackColor = this.SelectionBackColor;
				runnedReportsDataGridTableStyle.SelectionForeColor = this.SelectionForeColor;

				DataGridTextBoxColumn userColumnStyle = new DataGridTextBoxColumn();
				userColumnStyle.MappingName = UserColumnName;
				userColumnStyle.ReadOnly = true;
				userColumnStyle.HeaderText = Strings.UserColumnHeaderText;
				userColumnStyle.Width = 100;
				userColumnStyle.WidthChanged += new System.EventHandler(this.GridColumn_WidthChanged);

				runnedReportsDataGridTableStyle.GridColumnStyles.Add(userColumnStyle);

				DataGridTextBoxColumn ownerReportNamespaceColumnStyle = new DataGridTextBoxColumn();
				ownerReportNamespaceColumnStyle.MappingName = OwnerReportNamespaceColumnName;
				ownerReportNamespaceColumnStyle.ReadOnly = true;
				ownerReportNamespaceColumnStyle.HeaderText = Strings.OwnerReportNamespaceColumnHeaderText;
				ownerReportNamespaceColumnStyle.Width = 320;
				ownerReportNamespaceColumnStyle.WidthChanged += new System.EventHandler(this.GridColumn_WidthChanged);

				runnedReportsDataGridTableStyle.GridColumnStyles.Add(ownerReportNamespaceColumnStyle);

				DataGridTextBoxColumn timeStampColumnStyle = new DataGridTextBoxColumn();
				timeStampColumnStyle.MappingName = TimeStampColumnName;
				timeStampColumnStyle.ReadOnly = true;
				timeStampColumnStyle.HeaderText = Strings.TimeStampColumnHeaderText;
				timeStampColumnStyle.Width = 140;
				timeStampColumnStyle.WidthChanged += new System.EventHandler(this.GridColumn_WidthChanged);

				runnedReportsDataGridTableStyle.GridColumnStyles.Add(timeStampColumnStyle);

				DataGridTextBoxColumn descriptionColumnStyle = new DataGridTextBoxColumn();
				descriptionColumnStyle.MappingName = DescriptionColumnName;
				descriptionColumnStyle.ReadOnly = true;
				descriptionColumnStyle.HeaderText = Strings.DescriptionColumnHeaderText;
				descriptionColumnStyle.Width = minimumDataGridStringColumnWidth;
				descriptionColumnStyle.WidthChanged += new System.EventHandler(this.GridColumn_WidthChanged);

				runnedReportsDataGridTableStyle.GridColumnStyles.Add(descriptionColumnStyle);
				
				this.TableStyles.Add(runnedReportsDataGridTableStyle);
		
				this.PerformLayout();
			}

			//--------------------------------------------------------------------------------------------------------------------------------
			private void AdjustLastColumnWidth()
			{
				if (this.TableStyles == null || this.TableStyles.Count == 0)
					return;

				DataGridTableStyle tableStyle = this.TableStyles[RunnedReportsDataTableName]; 

				if 
					(
					tableStyle == null || 
					tableStyle.GridColumnStyles == null ||
					tableStyle.GridColumnStyles.Count == 0
					)
					return;

				int colswidth = tableStyle.RowHeaderWidth;
				for (int i = 0; i < tableStyle.GridColumnStyles.Count -1; i++)
					colswidth += tableStyle.GridColumnStyles[i].Width;

				int newColumnWidth = this.DisplayRectangle.Width - colswidth;
				if (this.VertScrollBar.Visible)
					newColumnWidth -= this.VertScrollBar.Width;
				newColumnWidth = Math.Max
					(
					minimumDataGridStringColumnWidth, 
					newColumnWidth - 4
					);

				DataGridColumnStyle lastColumnStyle = tableStyle.GridColumnStyles[tableStyle.GridColumnStyles.Count -1];
				if (lastColumnStyle.Width != newColumnWidth)
					lastColumnStyle.Width = newColumnWidth;

				this.PerformLayout();
				this.Refresh();
			}

			#endregion

			#region RunnedReportsDataGrid public properties
		
			public SqlConnection Connection { get { return connection; } set { connection = value; } }
			
			//--------------------------------------------------------------------------------------------------------------------------------
			public ArrayList SelectedRows
			{
				get
				{
					if 
						(
						this.DataSource == null || 
						!(this.DataSource is DataTable) ||
						String.Compare(((DataTable)this.DataSource).TableName, RunnedReportsDataTableName) != 0 ||
						((DataTable)this.DataSource).Rows.Count == 0
						)
						return null;
		
					ArrayList selectedRows = new ArrayList();
					for (int i = 0; i < ((DataTable)this.DataSource).Rows.Count; i++)
					{
						if (IsSelected(i))
							selectedRows.Add(((DataTable)this.DataSource).Rows[i]);
					}
					return selectedRows;
				}
			}
			
			#endregion

			#region RunnedReportsDataGrid public methods
			
			//--------------------------------------------------------------------------------------------------------------------------------
			public void Clear()
			{
				runnedReportManager = null;
				
				if 
					(
					this.DataSource != null && 
					this.DataSource is DataTable &&
					String.Compare(((DataTable)this.DataSource).TableName, RunnedReportsDataTableName) == 0
					)
					((DataTable)this.DataSource).Rows.Clear();
		
				if (RowsSelectionChanged != null)
					RowsSelectionChanged(this, System.EventArgs.Empty);
			}
		
			//--------------------------------------------------------------------------------------------------------------------------------
			public void DeleteSelectedReports()
			{
				if 
					(
					this.DataSource == null || 
					!(this.DataSource is DataTable) ||
					String.Compare(((DataTable)this.DataSource).TableName, RunnedReportsDataTableName) != 0 ||
					((DataTable)this.DataSource).Rows.Count == 0
					)
					return;
		
				ArrayList selectedRows = this.SelectedRows;
				if (selectedRows == null || selectedRows.Count == 0)
					return;

				foreach (DataRow selectedRow in selectedRows)
				{
					((RunnedReport)selectedRow[RunnedReportColumnName]).Delete();
					((DataTable)this.DataSource).Rows.Remove(selectedRow);
				}			
	
				if (RowsSelectionChanged != null)
					RowsSelectionChanged(this, System.EventArgs.Empty);
			}
		
			//--------------------------------------------------------------------------------------------------------------------------------
			public void SetAuthentication(int companyId, int loginId)
			{
				Clear();

				if (connection == null || companyId == -1)
					return;

				string loginName = String.Empty;
				string companyName = String.Empty;
				if (!EasyLookCustomSettings.GetLoginDataFromIds(connection, companyId, loginId, out loginName, out companyName))
					return;
				
				this.SuspendLayout();

				runnedReportManager = new RunnedReportMng();

				ArrayList runnedReportsList = new ArrayList();
		
				RunnedReport[] runnedReports = runnedReportManager.GetRunnedReports(companyName, loginName);
				if (runnedReports != null && runnedReports.Length > 0)
					runnedReportsList.AddRange(runnedReports);

				// Se sono sull'azienda con utente "AllUsers" devo anche caricare lo
				// storico di tutti i report relativi a ciacun utente dell'azienda
				if 
					(
					connection != null &&
					String.Compare(loginName, NameSolverStrings.AllUsers) == 0
					)
				{
					SqlCommand selectUsersSqlCommand = null;
					SqlDataReader usersReader = null;

					try
					{
						string sSelect =@"SELECT MSD_CompanyLogins.LoginId, MSD_Logins.Login FROM MSD_CompanyLogins INNER JOIN
							MSD_Logins ON MSD_Logins.LoginId = MSD_CompanyLogins.LoginId WHERE CompanyId = " + companyId;
			
						selectUsersSqlCommand = new SqlCommand(sSelect, connection);

						usersReader  = selectUsersSqlCommand.ExecuteReader();

						while (usersReader.Read())
						{
							RunnedReport[] userRunnedReports = runnedReportManager.GetRunnedReports(companyName, usersReader["Login"].ToString());
							if (userRunnedReports != null && userRunnedReports.Length > 0)
								runnedReportsList.AddRange(userRunnedReports);
						}

						usersReader.Close();
					}
					catch(SqlException exception)
					{
						Debug.Fail("SqlException raised in EasyLookCustomizerControl.RefreshRunnedReportsDataGrid: " + exception.Message);
					}
					finally
					{
						if (usersReader != null && !usersReader.IsClosed)
							usersReader.Close();

						if (selectUsersSqlCommand != null)
							selectUsersSqlCommand.Dispose();
					}
				}
				
				SetRunnedReportsList(runnedReportsList);
				
				this.ResumeLayout(true);

			}
			
			#endregion

		}

		#endregion
	}
}
