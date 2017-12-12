using System;
using System.Collections;
using System.Data;
using System.Text;
using System.Web;
using System.Web.UI;
using System.Web.UI.WebControls;
using Microarea.TaskBuilderNet.Core.Applications;
using Microarea.TaskBuilderNet.Core.Generic;
using Microarea.TaskBuilderNet.Core.MenuManagerLoader;
using Microarea.TaskBuilderNet.Interfaces;
using Microarea.TaskBuilderNet.UI.EasyLookCustomization;


namespace Microarea.Web.EasyLook
{

	//=========================================================================
	public partial class SearchPage : System.Web.UI.Page
	{

		#region Data Member
		/// <summary>
		/// Contain custom settings
		/// </summary>
		protected	EasyLookCustomSettings	easyLookCustomSettings	= null;
		protected const string backslashCharacter		= "\\";
		#endregion

		#region Load della pagina
		//---------------------------------------------------------------------
		/// <summary>
		/// Load of Page
		/// </summary>
		/// <param name="sender"></param>
		/// <param name="e"></param>
		protected void Page_Load(object sender, System.EventArgs e)
		{
			// Disabled cache
			this.Response.Cache.SetCacheability(HttpCacheability.NoCache);

			SetCulture();

			SearchButton.labelString = this.ResultLabel.ClientID;
			//Localized strings
			SetLocalizedLabel();

			//If is first search PrevResaultCheckBox is disabled
			PrevResaultCheckBox.Enabled = (Session[SessionKey.ResultNodeCollection] != null);

			if (!Page.IsPostBack)
			{
				//Apply  custom settings
				ApplyCustomSetting();
				//Load Applications and groups. Add items to DropDownList
				LoadApplicationAndModule();
			}
		}
		#endregion

		#region init functions
		//--------------------------------------------------------------------------
		protected virtual void AddLocalizationKeys()
		{

			StringBuilder sb = new StringBuilder();
			sb.AppendFormat("CheckboxNotCheched='{0}';", LabelStrings.NocheckedobjectType);

			ClientScriptManager cs = Page.ClientScript;
			if (!cs.IsClientScriptBlockRegistered("check"))
			{
				string script = "<script language='javascript'> " + sb.ToString() + "</script>";
				cs.RegisterClientScriptBlock(this.GetType(), "check", script);
			}
		}
		//---------------------------------------------------------------------
		/// <summary>
		/// Set the UserInfo Culture
		/// </summary>
		private void SetCulture()
		{
			//Setto la culture
			UserInfo ui = UserInfo.FromSession();

			if (ui == null)
				return;

			ui.SetCulture();
		}

		//---------------------------------------------------------------------
		/// <summary>
		/// Load Applications and groups add items in FilterByDropDownList
		/// </summary>
		private void LoadApplicationAndModule()
		{
			//Add fist item "(no filter)"
			ListItem item = new ListItem(LabelStrings.NoFilter);
			item.Value	  = string.Empty ;
			FilterByDropDownList.Items.Add(item);

			MenuXmlParser parser = Helper.GetMenuXmlParser();

			foreach(MenuXmlNode applicationNode in parser.Root.ApplicationsItems)
			{
				//Add application item
				item = new ListItem(applicationNode.Title);
				item.Value = applicationNode.GetApplicationName();
				FilterByDropDownList.Items.Add(item);

				foreach(MenuXmlNode groupNode in applicationNode.GroupItems)
				{
					//Add group item
					item = new ListItem("---" + " " + groupNode.Title);
					item.Value = groupNode.GetNameAttribute();
					FilterByDropDownList.Items.Add(item);
				}
			}
		}

		//---------------------------------------------------------------------
		/// <summary>
		/// Apply Custom Settings
		/// </summary>
		private void ApplyCustomSetting()
		{
			//Get EasyLookCustomSettings from session object
			easyLookCustomSettings = (EasyLookCustomSettings)Session[EasyLookCustomSettings.SessionKey];
			if (easyLookCustomSettings == null)
				return;

			//Set Font Name property and Font Size property to all control
			LookForLabel.Font.Name = easyLookCustomSettings.FontFamily;
			LookForLabel.Font.Size = FontUnit.XSmall;

			LookForTextBox.Font.Name = easyLookCustomSettings.FontFamily;
			LookForTextBox.Font.Size = FontUnit.XSmall;

			FilterByLabel.Font.Name = easyLookCustomSettings.FontFamily;
			FilterByLabel.Font.Size = FontUnit.XSmall;

			FilterByDropDownList.Font.Name = easyLookCustomSettings.FontFamily;
			FilterByDropDownList.Font.Size = FontUnit.XSmall;

			SearchButton.Font.Name = easyLookCustomSettings.FontFamily;
			SearchButton.Font.Size = FontUnit.XSmall;

			TitlesOnlyCheckBox.Font.Name = easyLookCustomSettings.FontFamily;
			TitlesOnlyCheckBox.Font.Size = FontUnit.XSmall;

			ExactlyCheckBox.Font.Name = easyLookCustomSettings.FontFamily;
			ExactlyCheckBox.Font.Size = FontUnit.XSmall;

			PrevResaultCheckBox.Font.Name = easyLookCustomSettings.FontFamily;
			PrevResaultCheckBox.Font.Size = FontUnit.XSmall;

			MatchCaseCheckBox.Font.Name = easyLookCustomSettings.FontFamily;
			MatchCaseCheckBox.Font.Size = FontUnit.XSmall;

			SearchResultDataGrid.Font.Name = easyLookCustomSettings.FontFamily;
			SearchResultDataGrid.Font.Size = FontUnit.XSmall;

			ResultLabel.Font.Name = easyLookCustomSettings.FontFamily;
			ResultLabel.Font.Size = FontUnit.XSmall;
		}

		//---------------------------------------------------------------------
		/// <summary>
		/// Set localized strings by user culture 
		/// dell'Utente corrente
		/// </summary>
		private void SetLocalizedLabel()
		{
			LookForLabel.Text			= LabelStrings.LookFor;
			FilterByLabel.Text			= LabelStrings.FilterBy;
			SearchButton.Text			= LabelStrings.Search;
			TitlesOnlyCheckBox.Text		= LabelStrings.TitlesOnly;
			ExactlyCheckBox.Text		= LabelStrings.ExactlyWords;
			PrevResaultCheckBox.Text	= LabelStrings.PrevResault;
			MatchCaseCheckBox.Text		= LabelStrings.MatchCase;
			SearchReportCheckbox.Text	= LabelStrings.SearchReport;
			SearchDocumentCheckbox.Text	= LabelStrings.SearchDocument;

			SearchResultDataGrid.Columns[0].HeaderText = LabelStrings.Type;
			SearchResultDataGrid.Columns[1].HeaderText = LabelStrings.ObjectTitle;
			SearchResultDataGrid.Columns[2].HeaderText = LabelStrings.ObjectPath;
		}

		#endregion



		#region DataGrid Creation
		//---------------------------------------------------------------------
		/// <summary>
		/// Create and polulate DataGrid data source
		/// </summary>
		private void CreateDataGridSource()
		{
			DataTable dataTable	= CreateDataSourceStructure();

			UserInfo ui = UserInfo.FromSession();
			if (ui == null)
				return;

			IPathFinder pathFinder = ui.PathFinder;

			if (Helper.GetMenuXmlParser() == null)
				return;

			MenuXmlParser parser = Helper.GetMenuXmlParser();

			//Search results
			MenuXmlNodeCollection currentResults = Search(pathFinder, parser);
			Session.Add(SessionKey.ResultNodeCollection, currentResults);

			if (currentResults == null || currentResults.Count == 0)
			{
				ResultLabel.Text = LabelStrings.NoResult;
				return;
			}

			ResultLabel.Text = String.Empty;

			easyLookCustomSettings = (EasyLookCustomSettings)Session[EasyLookCustomSettings.SessionKey];

			PopulateDataTable(ref dataTable, pathFinder, currentResults);
			
			DataView dv	= new DataView(dataTable);
			SearchResultDataGrid.DataSource = dv;

			if (SearchResultDataGrid.DataSource == null)
				return;

			SearchResultDataGrid.DataBind();	
		}
		
		//---------------------------------------------------------------------
		/// <summary>
		/// Create DataSource struture
		/// </summary>
		/// <returns></returns>
		private DataTable CreateDataSourceStructure()
		{
			DataTable dataTable = new DataTable();

			dataTable.Columns.Add(new DataColumn("ReportName",			typeof(string)));
			dataTable.Columns.Add(new DataColumn("Link",				typeof(string)));
			dataTable.Columns.Add(new DataColumn("Description",			typeof(string)));
			dataTable.Columns.Add(new DataColumn("MenuPath",			typeof(string)));
			dataTable.Columns.Add(new DataColumn("MenuTitle",			typeof(string)));
			dataTable.Columns.Add(new DataColumn("MenuPathWithName",	typeof(string)));
			dataTable.Columns.Add(new DataColumn("ReportNameSpace",		typeof(string)));
			dataTable.Columns.Add(new DataColumn("IsReport",			typeof(bool)));

			return dataTable;
		}

		//---------------------------------------------------------------------
		/// <summary>
		/// Populate DataTable.
		/// DataTable is dataGrid DataSource
		/// </summary>
		/// <param name="dataTable"></param>
		private void PopulateDataTable(ref DataTable dataTable, IPathFinder pathFinder, MenuXmlNodeCollection searchResults)
		{
			if (searchResults == null || searchResults.Count == 0)
				return;

			DataRow	dataRow	= null;

			foreach(MenuXmlNode node in searchResults)
			{
				dataRow = dataTable.NewRow();
				dataRow["ReportName"]		= node.Title;
				dataRow["Link"]				= GetLink(node);
				dataRow["Description"]		= MenuInfo.SetExternalDescription(pathFinder, node);
				dataRow["MenuPath"]			= CreateCommandPath(node);
				dataRow["MenuTitle"]		= node.GetParentMenuTitle();
				dataRow["MenuPathWithName"]	= GetPathForScript(node);
				dataRow["ReportNameSpace"]	= node.ItemObject;
				dataRow["IsReport"]			= node.IsRunReport;
				dataTable.Rows.Add(dataRow);
			}
		}

		//---------------------------------------------------------------------
		/// <summary>
		/// Return report position on menù structure
		/// </summary>
		/// <param name="node"></param>
		/// <returns></returns>
		private string  GetPathForScript(MenuXmlNode node)
		{
            string s = node.GetApplicationName() + @"\"  + node.GetGroupName();
			string menuPath = s.EncodeBase16() + MenuXmlNode.ActionMenuPathSeparator ;
			ArrayList nodeToTraceHierarchyList = node.GetMenuHierarchyList();

			if (nodeToTraceHierarchyList != null)
			{
				for (int i=0; i<nodeToTraceHierarchyList.Count; i++)
				{
                    s = ((MenuXmlNode)nodeToTraceHierarchyList[0]).Title;
					menuPath += s.EncodeBase16() + MenuXmlNode.ActionMenuPathSeparator ;
					if ( i< nodeToTraceHierarchyList.Count -1)
						menuPath += MenuXmlNode.ActionMenuPathSeparator ;
				}
			}
		
			return menuPath;
		}

		//---------------------------------------------------------------------
		/// <summary>
		/// Return link for javascript
		/// </summary>
		/// <param name="node"></param>
		/// <returns></returns>
		private string GetLink(MenuXmlNode node)
		{
			MenuXmlNode appNode = node.GetApplicationNode();
			if (appNode == null)
				return string.Empty;

			MenuXmlNode moduleNode = node.GetGroupNode();
			if (moduleNode == null)
				return string.Empty;

			return appNode.GetApplicationName()  + MenuXmlNode.ActionMenuPathSeparator + moduleNode.GetGroupName();
		}

		//---------------------------------------------------------------------
		/// <summary>
		/// Create report path string
		/// </summary>
		/// <param name="node"></param>
		/// <returns></returns>
		private string CreateCommandPath(MenuXmlNode node)
		{
			string menuPath = String.Empty;

			MenuXmlNode appNode = node.GetApplicationNode();
			if (appNode == null)
				return string.Empty;

			MenuXmlNode moduleNode = node.GetGroupNode();
			if (moduleNode == null)
				return string.Empty;

			menuPath = appNode.Title  + backslashCharacter + moduleNode.Title + backslashCharacter;
			ArrayList nodeToTraceHierarchyList = node.GetMenuHierarchyList();
			if (nodeToTraceHierarchyList != null)
			{
				foreach (MenuXmlNode ascendant in nodeToTraceHierarchyList)
					menuPath += ascendant.Title +backslashCharacter;
			}

			return menuPath;
		}


		//---------------------------------------------------------------------
		/// <summary>
		/// Return the html code for report image or document image
		/// </summary>
		//---------------------------------------------------------------------
		protected string GetFieldTypeImage(bool isReport)
		{
			return string.Format("<img id=\"Image1\" src=\"{0}\"></asp:Image>",isReport ? Helper.GetImageUrl("RunReport.GIF") : Helper.GetImageUrl("RunDocument.GIF"));
		}

		//---------------------------------------------------------------------
		/// <summary>
		/// Return the html code for href tag and script
		/// </summary>
		/// <param name="title"></param>
		/// <param name="objectNamespace"></param>
		/// <param name="path"></param>
		/// <param name="menuTitle"></param>
		/// <returns></returns>
		public string GetUrlField (string title, string objectPath, string pathWithName, string menuTitle, string reportNameSpace, bool isReport)
		{
			if (objectPath == null || objectPath == string.Empty)
				return "";

			string titleVisible = title;

            objectPath = objectPath.EncodeBase16();
            title = title.EncodeBase16();
            menuTitle = menuTitle.EncodeBase16();

			return string.Format("<a href=\"javascript:SelectCommand('{0}', '{1}', '{2}', '{3}', '{4}', '{6}')\">{5}</a>", objectPath, pathWithName, title, reportNameSpace, menuTitle, titleVisible, isReport);
		}

		//---------------------------------------------------------------------
		/// <summary>
		/// return custom FontName if custom settins exist
		/// </summary>
		/// <returns></returns>
		public string GetFontNames()
		{
			if (easyLookCustomSettings == null || easyLookCustomSettings.FontFamily == string.Empty)
				return string.Empty;

			return easyLookCustomSettings.FontFamily;
		}
		#endregion

		#region Search
		//---------------------------------------------------------------------
		/// <summary>
		/// Start search process
		/// </summary>
		/// <param name="sender"></param>
		/// <param name="e"></param>
		protected void SearchButton_Click(object sender, System.EventArgs e)
		{
			DateTime dt = DateTime.Now.AddSeconds(3);
			SearchResultDataGrid.DataSource = null;
			SearchResultDataGrid.DataBind();
			
			ResultLabel.Text = String.Empty;

			CreateDataGridSource();

			while (DateTime.Now < dt)
			{
				// do nothing; simulate a 5-second pause
			}
			
			if (Session[SessionKey.ResultNodeCollection] != null)
				PrevResaultCheckBox.Enabled = true;
		}
		//---------------------------------------------------------------------
		/// <summary>
		/// create MenuSearchEngine and call SearchExpression function 
		/// </summary>
		/// <param name="pathFinder"></param>
		/// <param name="parser"></param>
		/// <returns></returns>
		private MenuXmlNodeCollection Search(IPathFinder pathFinder, MenuXmlParser parser)
		{
			MenuSearchEngine search = new MenuSearchEngine(pathFinder, parser);
			
			//Setto i parametri per la ricerca
			search.CaseSensitive		= MatchCaseCheckBox.Checked;
			search.ExactWord			= ExactlyCheckBox.Checked;
			search.SearchDescriptions	= !TitlesOnlyCheckBox.Checked;

			if (FilterByDropDownList.SelectedValue != string.Empty)
				search.StartSearchNode = GetStartNode(parser);

			search.SearchInPreviousResult	= PrevResaultCheckBox.Checked;
			if (search.SearchInPreviousResult)
				search.LastResults = (MenuXmlNodeCollection) Session[SessionKey.ResultNodeCollection];

			search.ExtractReports	= this.SearchReportCheckbox.Checked;
			search.ExtractDocuments = this.SearchDocumentCheckbox.Checked;

			if (search.SearchExpression(LookForTextBox.Text))
				return search.LastResults;

			return null;
		}
		//---------------------------------------------------------------------
		/// <summary>
		/// return MenuXmlNode for StartSearchNode property
		/// </summary>
		/// <param name="parser"></param>
		/// <returns></returns>
		private MenuXmlNode GetStartNode(MenuXmlParser parser)
		{

			string[] token =  FilterByDropDownList.SelectedValue.Split('.');

			if (token.Length ==1)
				//Ho selezionato un'applicazione
				return parser.GetApplicationNodeByName(token[0]);
			else
			{
				MenuXmlNode applicationNode = parser.GetApplicationNodeByName(token[0]);
				return applicationNode.GetGroupNodeByName(FilterByDropDownList.SelectedValue);
			}
			
		}

		#endregion

		protected void PrevResaultCheckBox_CheckedChanged(object sender, System.EventArgs e)
		{
			if (PrevResaultCheckBox.Checked &&Session[SessionKey.ResultNodeCollection] != null && 
				((MenuXmlNodeCollection)Session[SessionKey.ResultNodeCollection]).Count == 0)

				PrevResaultCheckBox.Checked = false;
		}
	
	}
	//=========================================================================
}
