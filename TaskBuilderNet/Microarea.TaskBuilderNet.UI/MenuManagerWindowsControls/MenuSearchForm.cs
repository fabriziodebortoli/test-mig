using System;
using System.Windows.Forms;

using Microarea.TaskBuilderNet.Core.MenuManagerLoader;

namespace Microarea.TaskBuilderNet.UI.MenuManagerWindowsControls
{
	/// <summary>
	/// Summary description for MenuSearchDialog.
	/// </summary>
	public partial class MenuSearchForm : System.Windows.Forms.Form
	{
		private MenuLoader menuLoader = null;
		private MenuSearchEngine appsSearchEngine = null;
		private MenuSearchEngine favoritesSearchEngine = null;
		private MenuSearchEngine environmentSearchEngine = null;

		private MenuXmlNodeCollection lastSearchResults = null;

		private bool enableFilter = true;
		private bool enableApplicationsFilter = true;
		private bool enableEnvironmentFilter = true;

		private const uint maximumSearchCriteriaToStore = 24;


		public event MenuSearchFormEventHandler RunFoundCommand;

		public MenuSearchForm(MenuLoader aMenuLoader, System.Windows.Forms.Form ownerForm)
		{
			//
			// Required for Windows Form Designer support
			//
			InitializeComponent();

			MenuLoader = aMenuLoader;
			
			this.Owner = ownerForm;
		}

		public MenuSearchForm(MenuLoader aMenuLoader) : this(aMenuLoader, null)
		{
		}

		#region MenuSearchForm protected overridden methods

		protected override void OnLoad(EventArgs e)
		{
			base.OnLoad (e);

			this.PerformLayout();
		}

		#endregion
		
		#region MenuSearchForm event handlers
		
		//--------------------------------------------------------------------------------------------------------------------------------
		private void SearchCriteriaComboBox_TextChanged(object sender, System.EventArgs e)
		{
			SearchButton.Enabled = (SearchCriteriaComboBox.Text != null && SearchCriteriaComboBox.Text.Length > 0);
		}

		//--------------------------------------------------------------------------------------------------------------------------------
		private void SearchInTitlesOnlyCheckBox_CheckedChanged(object sender, System.EventArgs e)
		{
			if (!SearchInTitlesOnlyCheckBox.Checked && !SearchResultsView.ShowCommandsDescriptions)
				SearchResultsView.ShowCommandsDescriptions = true;

		}
		//--------------------------------------------------------------------------------------------------------------------------------
		private void SearchResultsView_ShowFlagsChanged(object sender, System.EventArgs e)
		{
			FillSearchResultsView();
		}

		//--------------------------------------------------------------------------------------------------------------------------------
		private void SearchResultsView_RunCommand(object sender, MenuMngCtrlEventArgs e)
		{
			if (SearchResultsView.SelectedCommand == null)
				return;

			if (SearchResultsView.SelectedCommand.Tag == null || !(SearchResultsView.SelectedCommand.Tag is MenuXmlParser))
				return;

			if (RunFoundCommand != null)
				RunFoundCommand(this, new MenuSearchFormEventArgs(SearchResultsView.SelectedCommand.Node, (MenuXmlParser)SearchResultsView.SelectedCommand.Tag));
		}
		
		//--------------------------------------------------------------------------------------------------------------------------------
		private void SearchButton_Click(object sender, System.EventArgs e)
		{
			if (SearchCriteriaComboBox.Text == null || SearchCriteriaComboBox.Text.Trim().Length == 0)
				return;

			lastSearchResults = new MenuXmlNodeCollection();
			
			SearchButton.Enabled = false;
			Cursor.Current = Cursors.WaitCursor;

			SearchStatusBarPanel.Text = MenuManagerWindowsControlsStrings.MenuSearchInProgressStatusBarMessage;

			try
			{
				if (FilteredByComboBox.SelectedIndex == 0 || String.Compare(FilteredByComboBox.Text, MenuManagerWindowsControlsStrings.MenuSearchFilteredByApplications) == 0)
					lastSearchResults.AddRange(Search(appsSearchEngine));
				if (FilteredByComboBox.SelectedIndex == 0 || String.Compare(FilteredByComboBox.Text, MenuManagerWindowsControlsStrings.MenuSearchFilteredByFavorites) == 0)
					lastSearchResults.AddRange(Search(favoritesSearchEngine));
				if (FilteredByComboBox.SelectedIndex == 0 || String.Compare(FilteredByComboBox.Text, MenuManagerWindowsControlsStrings.MenuSearchFilteredByEnvironment) == 0)
					lastSearchResults.AddRange(Search(environmentSearchEngine));

				lastSearchResults.SortByTitles();
			}
			catch(MenuXmlParserException exception)
			{
				MessageBox.Show(this, String.Format(MenuManagerWindowsControlsStrings.MenuSearchFailedMessageText, exception.Message), MenuManagerWindowsControlsStrings.MenuSearchFailedMessageCaption, MessageBoxButtons.OK, MessageBoxIcon.Error);
			}
			catch(Exception)
			{
				MessageBox.Show(this, MenuManagerWindowsControlsStrings.MenuSearchGenericFailureMessageText, MenuManagerWindowsControlsStrings.MenuSearchFailedMessageCaption, MessageBoxButtons.OK, MessageBoxIcon.Error);
			}

			Cursor.Current = Cursors.Default;
			SearchButton.Enabled = true;

			FillSearchResultsView();

			if (lastSearchResults == null || lastSearchResults.Count == 0)
			{
				SearchInPreviousResultsCheckBox.Checked = false;
				SearchInPreviousResultsCheckBox.Enabled = false;
			}
			else
				SearchInPreviousResultsCheckBox.Enabled = true;

			if (SearchCriteriaComboBox.FindStringExact(SearchCriteriaComboBox.Text.Trim()) == -1)
			{
				if (SearchCriteriaComboBox.Items.Count == maximumSearchCriteriaToStore)
					SearchCriteriaComboBox.Items.RemoveAt(SearchCriteriaComboBox.Items.Count - 1);

				SearchCriteriaComboBox.Items.Add(SearchCriteriaComboBox.Text.Trim());
			}
		}

		#endregion

		#region MenuSearchForm private methods

		//--------------------------------------------------------------------------------------------------------------------------------
		private void CreateSearchEngines()
		{
			lastSearchResults = null;
			SearchInPreviousResultsCheckBox.Enabled = false;

			FilteredByComboBox.Items.Clear();
			FilteredByComboBox.Items.Add(MenuManagerWindowsControlsStrings.MenuSearchNoFilter);
			FilteredByComboBox.SelectedIndex = 0;

			if (appsSearchEngine != null)
			{
				appsSearchEngine.Dispose();
				appsSearchEngine = null;
			}
			if (menuLoader != null && menuLoader.PathFinder != null && menuLoader.AppsMenuXmlParser != null)
			{
				appsSearchEngine = new MenuSearchEngine(menuLoader.PathFinder, menuLoader.AppsMenuXmlParser);
				appsSearchEngine.ExtractAll = true;

				FilteredByComboBox.Items.Add(MenuManagerWindowsControlsStrings.MenuSearchFilteredByApplications);
			}

			if (favoritesSearchEngine != null)
			{
				favoritesSearchEngine.Dispose();
				favoritesSearchEngine = null;
			}
			
			if (environmentSearchEngine != null)
			{
				environmentSearchEngine.Dispose();
				environmentSearchEngine = null;
			}
			if (menuLoader != null && menuLoader.PathFinder != null && menuLoader.EnvironmentXmlParser != null)
			{
				environmentSearchEngine = new MenuSearchEngine(menuLoader.PathFinder, menuLoader.EnvironmentXmlParser);
				environmentSearchEngine.ExtractAll = true;

				FilteredByComboBox.Items.Add(MenuManagerWindowsControlsStrings.MenuSearchFilteredByEnvironment);
			}
		}

		//--------------------------------------------------------------------------------------------------------------------------------
		private MenuXmlNodeCollection Search(MenuSearchEngine aSearchEngine)
		{
			if (aSearchEngine == null)
				return null;

			aSearchEngine.SearchDescriptions = !SearchInTitlesOnlyCheckBox.Checked;
			aSearchEngine.ExactWord = SearchExactWordsCheckBox.Checked;
			aSearchEngine.CaseSensitive = MatchCaseCheckBox.Checked;
			aSearchEngine.SearchInPreviousResult = SearchInPreviousResultsCheckBox.Checked;

			if (!aSearchEngine.SearchExpression(SearchCriteriaComboBox.Text))
				return null;
			
			return aSearchEngine.LastResults;
		}

		//--------------------------------------------------------------------------------------------------------------------------------
		private void FillSearchResultsView()
		{
			SearchResultsView.Items.Clear();

			if (lastSearchResults != null && lastSearchResults.Count > 0)
			{
				foreach (MenuXmlNode aCommandNode in lastSearchResults)
				{
					MenuXmlParser commandParser = null;
					if (appsSearchEngine != null && appsSearchEngine.LastResults != null && appsSearchEngine.LastResults.Contains(aCommandNode))
						commandParser = appsSearchEngine.Parser;
					else if (favoritesSearchEngine != null && favoritesSearchEngine.LastResults != null && favoritesSearchEngine.LastResults.Contains(aCommandNode))
						commandParser = favoritesSearchEngine.Parser;
					else if (environmentSearchEngine != null && environmentSearchEngine.LastResults != null && environmentSearchEngine.LastResults.Contains(aCommandNode))
						commandParser = environmentSearchEngine.Parser;

					SearchResultsView.AddMenuCommand(aCommandNode, commandParser);
				}
			}

			if (SearchResultsView.Items.Count > 0)
				SearchStatusBarPanel.Text = String.Format(MenuManagerWindowsControlsStrings.MenuSearchResultsStatusBarMessage, SearchResultsView.Items.Count.ToString());
			else
				SearchStatusBarPanel.Text = MenuManagerWindowsControlsStrings.MenuSearchNoResultStatusBarMessage;
		}
		
		#endregion

		#region MenuSearchForm public properties
		
		//--------------------------------------------------------------------------------------------------------------------------------
		public MenuLoader MenuLoader
		{
			get { return menuLoader; }
			set 
			{
				if (menuLoader == value)
					return;

				menuLoader = value; 

				SearchResultsView.PathFinder = (menuLoader!= null) ? menuLoader.PathFinder : null;

				CreateSearchEngines();
			}
		}

		//--------------------------------------------------------------------------------------------------------------------------------
		public bool EnableFilter 
		{
			get { return enableFilter; }
			set
			{
				enableFilter = value;

				FilteredByLabel.Visible = FilteredByComboBox.Visible = enableFilter;

				if (!enableFilter)
					FilteredByComboBox.SelectedIndex = 0;
			}
		}

		//--------------------------------------------------------------------------------------------------------------------------------
		public bool EnableApplicationsFilter 
		{
			get { return enableApplicationsFilter; }
			set
			{
				enableApplicationsFilter = value && (menuLoader != null && menuLoader.AppsMenuXmlParser != null && menuLoader.AppsMenuXmlParser.HasCommandDescendantsNodes);

				int idx = FilteredByComboBox.FindStringExact(MenuManagerWindowsControlsStrings.MenuSearchFilteredByApplications);
				if (enableApplicationsFilter)
				{
					if (idx == -1)
						FilteredByComboBox.Items.Add(MenuManagerWindowsControlsStrings.MenuSearchFilteredByApplications);
				}
				else
				{
					if (idx >= 0)
						FilteredByComboBox.Items.RemoveAt(idx);
				}
			}
		}

		//--------------------------------------------------------------------------------------------------------------------------------
		public bool EnableEnvironmentFilter 
		{
			get { return enableEnvironmentFilter; }
			set
			{
				enableEnvironmentFilter = value && (menuLoader != null && menuLoader.EnvironmentXmlParser != null && menuLoader.EnvironmentXmlParser.HasCommandDescendantsNodes);

				int idx = FilteredByComboBox.FindStringExact(MenuManagerWindowsControlsStrings.MenuSearchFilteredByEnvironment);
				if (enableEnvironmentFilter)
				{
					if (idx == -1)
						FilteredByComboBox.Items.Add(MenuManagerWindowsControlsStrings.MenuSearchFilteredByEnvironment);
				}
				else
				{
					if (idx >= 0)
						FilteredByComboBox.Items.RemoveAt(idx);
				}
			}
		}

		#endregion

		#region MenuSearchForm public methods
		
		//--------------------------------------------------------------------------------------------------------------------------------
		public void InitNewSearch()
		{
			SearchCriteriaComboBox.Text = String.Empty;

			if (SearchCriteriaComboBox.CanFocus)
				SearchCriteriaComboBox.Focus();
		}

		//---------------------------------------------------------------------------
		public void EnableShowDocumentsOption(bool enableOption)
		{
			if (SearchResultsView == null)
				return;

			SearchResultsView.EnableShowDocumentsOption(enableOption);
		}

		//---------------------------------------------------------------------------
		public void EnableShowReportsOption(bool enableOption)
		{
			if (SearchResultsView == null)
				return;

			SearchResultsView.EnableShowReportsOption(enableOption);
		}

		//---------------------------------------------------------------------------
		public void EnableShowBatchesOption(bool enableOption)
		{
			if (SearchResultsView == null)
				return;

			SearchResultsView.EnableShowBatchesOption(enableOption);
		}

		//---------------------------------------------------------------------------
		public void EnableShowFunctionsOption(bool enableOption)
		{
			if (SearchResultsView == null)
				return;

			SearchResultsView.EnableShowFunctionsOption(enableOption);
		}

		//---------------------------------------------------------------------------
		public void EnableShowExecutablesOption(bool enableOption)
		{
			if (SearchResultsView == null)
				return;

			SearchResultsView.EnableShowExecutablesOption(enableOption);
		}

		//---------------------------------------------------------------------------
		public void EnableShowTextsOption(bool enableOption)
		{
			if (SearchResultsView == null)
				return;

			SearchResultsView.EnableShowTextsOption(enableOption);
		}

		//---------------------------------------------------------------------------
		public void EnableShowOfficeItemsOption(bool enableOption)
		{
			if (SearchResultsView == null)
				return;

			SearchResultsView.EnableShowOfficeItemsOption(enableOption);
		}

		#endregion
	}

	public delegate void MenuSearchFormEventHandler(object sender, MenuSearchFormEventArgs e);
	//============================================================================
	public class MenuSearchFormEventArgs : EventArgs
	{
		private MenuXmlNode node = null;
		private MenuXmlParser parser = null;

		//--------------------------------------------------------------------------------------------------------------------------------
		public MenuSearchFormEventArgs(MenuXmlNode aNode, MenuXmlParser aParser)
		{
			node = aNode;
			parser = aParser;
		}

		//--------------------------------------------------------------------------------------------------------------------------------
		public MenuXmlNode Node { get { return node; } }
		//--------------------------------------------------------------------------------------------------------------------------------
		public MenuXmlParser Parser { get { return parser; } }
	}
}
