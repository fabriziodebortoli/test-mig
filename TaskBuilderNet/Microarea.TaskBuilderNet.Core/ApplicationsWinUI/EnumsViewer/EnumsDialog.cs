using System;
using System.Collections;
using System.IO;
using System.Windows.Forms;
using System.Xml;
using Microarea.TaskBuilderNet.Core.Applications;
using Microarea.TaskBuilderNet.Core.CoreTypes;
using Microarea.TaskBuilderNet.Core.DiagnosticManager;
using Microarea.TaskBuilderNet.Core.DiagnosticUI;
using Microarea.TaskBuilderNet.Core.Generic;
using Microarea.TaskBuilderNet.Core.NameSolver;
using Microarea.TaskBuilderNet.Core.WebServicesWrapper;
using Microarea.TaskBuilderNet.Interfaces;

namespace Microarea.TaskBuilderNet.Core.ApplicationsWinUI.EnumsViewer
{
	/// <summary>
	/// Enums Viewer Dialog
	/// </summary>
	//============================================================================
	public partial class EnumsDialog : System.Windows.Forms.Form
	{
		#region Data Members 

		// Copy Into the clipboard syntax
		//-----------------------------------------------------------------------------
		private static string DefaultSeparator			= " , ";
		private static string WoormDeclaration			= "{0} {1} : {2} {3}\r\n";
		private static string EnumsNameSeparator		= "_";
		private static string EnumsTagDeclaration		= "\r\nconst  WORD E_{0}		 = {1};\r\n";
		private static string EnumsItemDeclaration		= "const DWORD E_{0}		 = MAKELONG ( {1}, {2} ); //{3} \r\n";
		private static string EnumsDefaultDeclaration	= "const DWORD E_{0}_DEFAULT = E_{1};\r\n";
		private static string DbValueDeclaration		= "{0} = {1}\r\n";
		private static char[] EnumsNotAllowed			= new char[] {		'`', '\'' , '&' , '.' , ',' , '+' , '-', '*' , '/', '(' , ')' };
		
		// ListBox Positions
		//-----------------------------------------------------------------------------
		private static int PosTagName		= 0;
		private static int PosTagValue		= 1;
		private static int PosTagDefault	= 2;
		private static int PosItemName		= 3;
		private static int PosItemValue		= 4;
		private static int PosDbValue		= 5;

		// Variables to save old columns widths in collapsed mode
		//-----------------------------------------------------------------------------
		private int oldColWidthDefault		= 0;
		private int oldColWidthItemName		= 0;
		private int oldColWidthItemValue	= 0;
		private int oldColWidthDbValue		= 0;

		//-----------------------------------------------------------------------------
		private Enums							enums			= null;
		private Settings						settings		= null;
		private ArrayList						activationCache	= new ArrayList ();
		private Diagnostic diagnostic = new Diagnostic(ApplicationsWinUIStrings.MessageBoxCaption);
		private BrandLoader						brandLoader		= null;
		
		#endregion

		#region Properties

		public Diagnostic Messages { get { return diagnostic; } }

		#endregion

        #region Constructors, Destructors and Initializations

        /// <summary>
		/// Settings parameters are mandatory
		/// </summary>
		//-----------------------------------------------------------------------------
		public EnumsDialog (Settings settings)
		{
			this.Visible = false;

			// initialization failed, settings not intilized
			if (settings == null)
				throw (new EnumsDialogException(ApplicationsWinUIStrings.SettingsNotInitialized, true));

			Cursor = Cursors.WaitCursor;

			this.settings = settings;
			this.settings.SettingsFile.Diagnostic = diagnostic;
			
			// language
			if (settings.Culture != null && settings.Culture != string.Empty)
				InitCulture (settings.Culture);


			// localization
			InitCulture	(settings.Culture);

			// brand management for application titles

			LoginManager loginManager = new LoginManager();

			brandLoader = new BrandLoader();

			// This call is required by the Windows.Forms Form Designer.
			InitializeComponent		();
			SaveColumnsState		();
			
			InitDefaults			();
			InitActivationsCache	();

			enums = new Enums();
			try
			{
				enums.LoadXml();
			}
			catch (EnumsException e)
			{
				diagnostic.SetError(e.Message);
			}

			Find ();

			
			Cursor = Cursors.Arrow;

			// messages to the user
			if (diagnostic.Error || diagnostic.Warning || diagnostic.Information)
			{
				DiagnosticViewer.ShowDiagnostic(diagnostic);
				diagnostic.Clear ();
			}

			// update the title bar
			this.Text = this.Text + string.Format(ApplicationsWinUIStrings.EnumsDialogTitleExtension, settings.Instance, settings.RemoteServer);
		}

		/// <summary>
		/// Culture and language initialization
		/// </summary>
		//---------------------------------------------------------------------
		private void InitCulture(string culture)
		{
            string error;
            EnumsViewerManager.InitCulture(culture, out error);
           
			if (!string.IsNullOrEmpty(error))
               diagnostic.SetError (error);
		}

		/// <summary>
		/// Defaults initialization
		/// </summary>
		//---------------------------------------------------------------------
		private void InitDefaults ()
		{
			CbxFindEnumName.	Items.Clear	();
			CbxFindEnumValue.	Items.Clear	();
			CbxFindItemName.	Items.Clear	();


			switch (settings.OrderBy)
			{
				case Settings.OrderByMode.EnumName:
					LbxOrderBy.SelectedIndex = 0;
					break;

				case Settings.OrderByMode.EnumValue:
					LbxOrderBy.SelectedIndex = 0;
					break;
			}

			MnuCopyAsCode.Visible = settings.VsNetIntegrated;

			MnuViewCollapse.Checked = false;
			TbxCollapse.Checked = false;

			LoadSettings ();

            // sets tooltips on controls
			TtipSearch.SetToolTip(CbxFindEnumName, ApplicationsWinUIStrings.TooltipFindEnumName);
			TtipSearch.SetToolTip(CbxFindEnumValue, ApplicationsWinUIStrings.TooltipFindEnumValue);
			TtipSearch.SetToolTip(CbxFindItemName, ApplicationsWinUIStrings.TooltipFindItemName);
			TtipSearch.SetToolTip(CbxFindDbValue, ApplicationsWinUIStrings.TooltipFindDbValue);
        }

		/// <summary>
		/// Contacts LoginManager and caches the activation status for each module.
		/// It speeds the tool's performance because it calls the web service only
		/// once, during the initialization process.
		/// </summary>
		//-----------------------------------------------------------------------------
		protected void InitActivationsCache ()
		{
			LoginManager loginManager = new LoginManager();

			foreach (BaseApplicationInfo app in BasePathFinder.BasePathFinderInstance.ApplicationInfos)
				foreach (BaseModuleInfo module in app.Modules)
					if (loginManager.IsActivated (app.Name, module.Name))
						activationCache.Add (app.Name.ToLower() + NameSpace.TokenSeparator + module.Name.ToLower());
			
			loginManager = null;
		}

		#endregion

		#region Filters and Order By Management

		/// <summary>
		/// Clear the listbox and reload it depending on the filters and the order by 
		/// </summary>
		//-----------------------------------------------------------------------------
		internal void LoadEnumsTable ()
		{
			if (enums == null)
				return;

			Cursor = Cursors.WaitCursor;

			LbxEnums.Items.Clear();

			foreach (EnumTag tag in enums.Tags)
			{
				if (!IsTagToDisplay (tag))
				{
					continue;
				}
				
				string tagName = tag.LocalizedName;
				string tagValue = tag.Value.ToString();

				foreach (EnumItem item in tag.EnumItems)
				{
					if (!IsItemToDisplay(item))
					{
						continue;
					}

					DataEnum dataEnum	= new DataEnum(tag.Value, item.Value);
					string dbValue		= ((uint) dataEnum).ToString();
					string deflt		= tag.DefaultValue == item.Value ? "v" : "" ;
					string appTitle		= item.OwnerModule.ParentApplicationInfo.Name;

					string brandMenuTitle = brandLoader.GetApplicationBrandMenuTitle(item.OwnerModule.ParentApplicationInfo.Name);

					if (!brandMenuTitle.IsNullOrEmpty())
						appTitle = brandMenuTitle;

					ListViewItem lbxItem = new ListViewItem(new string[] { tagName, tagValue }, -1 );
			
					if (TbxCollapse.Checked)
					{
						lbxItem.SubItems.Add ("");
						lbxItem.SubItems.Add ("");
						lbxItem.SubItems.Add ("");
						lbxItem.SubItems.Add ("");
					}
					else
					{
						lbxItem.SubItems.Add (deflt);
						lbxItem.SubItems.Add (item.LocalizedName);
						lbxItem.SubItems.Add (item.Value.ToString());
						lbxItem.SubItems.Add (dbValue);
					}

					lbxItem.SubItems.Add (appTitle);
					lbxItem.SubItems.Add (item.OwnerModule.Title);

					AddOnOrderByRule (tag, item, lbxItem);

					if (TbxCollapse.Checked)
						break;
					
					tagValue = string.Empty;
					tagName = string.Empty;
				}
			}

			// recents 
			UpdateRecents				();
			LbxEnums.Update();

			Cursor = Cursors.Arrow;
		}

		/// <summary>
		/// Checks if the tag can be displayed
		/// </summary>
		//-----------------------------------------------------------------------------
		internal bool IsTagToDisplay (EnumTag tag)
		{
			string moduleKey = tag.OwnerModule.ParentApplicationInfo.Name + NameSpace.TokenSeparator + tag.OwnerModule.Name;
			moduleKey = moduleKey.ToLower();

			// filters management
			if (tag.Hidden)
				return false;

			if (MnuFilterActivated.Checked && !activationCache.Contains (moduleKey))
				return false;

			if (settings.Application != string.Empty && string.Compare (tag.OwnerModule.ParentApplicationInfo.Name, settings.Application, true) != 0)
				return false;	

			string searchText	= string.Empty;
			string where		= string.Empty;

			// enum name find 
			if (CbxFindEnumName.Text != null &&  CbxFindEnumName.Text != string.Empty)
			{
				where = tag.LocalizedName.ToLower();
				where.Trim();
				searchText = CbxFindEnumName.Text.ToLower();
				searchText.Trim();

				if (where.IndexOf(searchText) < 0)
					return false;
			}

			// enum value find 
			if (CbxFindEnumValue.Text != null &&  CbxFindEnumValue.Text != string.Empty)
			{
				where =  tag.Value.ToString().ToLower();
				where.Trim();
				searchText = CbxFindEnumValue.Text.ToLower();
				searchText.Trim();

				if (where.IndexOf(searchText) < 0)
					return false;
			}

			bool ok = false;

			// item name find
			if (CbxFindItemName.Text != null &&  CbxFindItemName.Text != string.Empty)
			{
				searchText = CbxFindItemName.Text.ToLower();
				searchText.Trim();

				foreach (EnumItem item in tag.EnumItems)
				{
					where = item.LocalizedName.ToLower();
					where.Trim();

					if (where.IndexOf(searchText) >= 0)
					{
						ok = true;
						break;
					}
				}

				if (!ok)
					return false;
			}

			// database value find
			ok = false;
			if (CbxFindDbValue.Text != null &&  CbxFindDbValue.Text != string.Empty)
			{
				searchText = CbxFindDbValue.Text.ToLower();
				searchText.Trim();

				foreach (EnumItem item in tag.EnumItems)
				{
					DataEnum dataEnum = new DataEnum (tag.Value, item.Value);
					where = ((uint) dataEnum).ToString ();
					where.Trim();

					if (where.IndexOf(searchText) >= 0)
					{
						ok = true;
						break;
					}
				}

				if (!ok)
					return false;
			}

			return true;
		}

		/// <summary>
		/// Checks if the item can be displayed 
		/// </summary>
		//-----------------------------------------------------------------------------
		internal bool IsItemToDisplay (EnumItem item)
		{
			string moduleKey = item.OwnerModule.ParentApplicationInfo.Name + NameSpace.TokenSeparator + item.OwnerModule.Name;
			moduleKey = moduleKey.ToLower ();

			// filters management
			if (item.Hidden)
				return false;

			if (MnuFilterActivated.Checked && !activationCache.Contains (moduleKey))
				return false;

			if (settings.Application != string.Empty && string.Compare (item.OwnerModule.ParentApplicationInfo.Name, settings.Application, true) != 0)
				return false;	

			return true;
		}
	
		/// <summary>
		/// Add the item in the listbox on the base of the order by selection.
		/// SearchItemPositionByName returns -1 when the item must be added at
		/// the end of the list box.
		/// </summary>
		//-----------------------------------------------------------------------------
		private void AddOnOrderByRule (EnumTag tag, EnumItem item, ListViewItem lbxItem)
		{
			// order by name
			int nPos = -1;

			// is the first
			bool isFirst = tag.EnumItems[0] == item;

			foreach (ListViewItem currItem in LbxEnums.Items)
			{
				nPos++;

				if (currItem.SubItems[PosTagName].Text == string.Empty)
					continue;

				// by name order by
				if (LbxOrderBy.SelectedIndex == 0 && string.Compare(currItem.SubItems[PosTagName].Text, tag.LocalizedName, true) > 0)
					break;
					// by value order by
				else if (LbxOrderBy.SelectedIndex == 1 && Int32.Parse(currItem.SubItems[PosTagValue].Text) > tag.Value)
					break;
			}

			if (nPos >= (LbxEnums.Items.Count - 1))
				nPos = -1;

			if (nPos < 0)
				LbxEnums.Items.Add (lbxItem);
			else
				LbxEnums.Items.Insert (nPos,lbxItem);
		}

		#endregion

		#region Settings Management

		/// <summary>
		/// Adding Recent Search Management 
		/// </summary>
		//-----------------------------------------------------------------------------
		private void AddRecentSearch (ComboBox.ObjectCollection items, string text)
		{
			items.Insert (0, text);

			// removes duplicates exluding the index 0
			for (int i = items.Count - 1; i > 0; i--)
			{
				string s = (string) items[i];
				if (string.Compare(s, text, true) == 0)
					items.RemoveAt (i);
			}
		}

		/// <summary>
		/// Updates all Recents Collections
		/// </summary>
		//-----------------------------------------------------------------------------
		private void UpdateRecents ()
		{
			if (CbxFindEnumName.Text != null &&  CbxFindEnumName.Text != string.Empty)
			{
				AddRecentSearch (CbxFindEnumName.Items, CbxFindEnumName.Text);
				CbxFindEnumName.SelectedIndex = 0;
				CbxFindEnumName .Update();
			}

			if (CbxFindEnumValue.Text != null &&  CbxFindEnumValue.Text != string.Empty)
			{
				AddRecentSearch (CbxFindEnumValue.Items, CbxFindEnumValue.Text);
				CbxFindEnumValue.SelectedIndex = 0;
				CbxFindEnumValue.Update();
			}

			if (CbxFindItemName.Text != null &&  CbxFindItemName.Text != string.Empty)
			{
				AddRecentSearch (CbxFindItemName.Items, CbxFindItemName.Text);
				CbxFindItemName.SelectedIndex = 0;
				CbxFindItemName.Update();
			}

			if (CbxFindDbValue.Text != null &&  CbxFindDbValue.Text != string.Empty)
			{
				AddRecentSearch (CbxFindDbValue.Items, CbxFindDbValue.Text);
				CbxFindDbValue.SelectedIndex = 0;
				CbxFindDbValue.Update();
			}

		}

		/// <summary>
		/// Settings loading and initialization
		/// </summary>
		//---------------------------------------------------------------------
		private void LoadSettings ()
		{
			if (settings.SettingsFileName == string.Empty)
				return;

			// the file doesn't exist or is corrupted
			if (!settings.SettingsFile.Load (settings.SettingsFileName))
				return;

			string [] recents = settings.SettingsFile.GetRecents (CbxFindEnumName.Name);
			if (recents != null)
			{
				foreach (string s in recents)
					if (s != string.Empty)
						CbxFindEnumName.Items.Add (s);

				CbxFindEnumName.Text = CbxFindEnumName.Items[0].ToString();
			}

			recents = settings.SettingsFile.GetRecents (CbxFindEnumValue.Name);
			if (recents != null)
			{
				foreach (string s in recents)
					if (s != string.Empty)
						CbxFindEnumValue.Items.Add (s);

				CbxFindEnumValue.Text = CbxFindEnumValue.Items[0].ToString();
			}

			recents = settings.SettingsFile.GetRecents (CbxFindItemName.Name);
			if (recents != null)
			{
				foreach (string s in recents)
					if (s != string.Empty)
						CbxFindItemName.Items.Add (s);

				CbxFindItemName.Text = CbxFindItemName.Items[0].ToString();
			}
	
			recents = settings.SettingsFile.GetRecents (CbxFindDbValue.Name);
			if (recents != null)
			{
				foreach (string s in recents)
					if (s != string.Empty)
						CbxFindDbValue.Items.Add (s);

				CbxFindDbValue.Text = CbxFindDbValue.Items[0].ToString();
			}
		
			recents = settings.SettingsFile.GetRecents (LbxOrderBy.Name);
			if (recents != null)
				try  {  LbxOrderBy.SelectedIndex = Int32.Parse(recents[0]);  }
				catch (Exception e)
				{
					diagnostic.SetWarning(string.Format(ApplicationsWinUIStrings.ReadSettingsRecentsError, settings.SettingsFileName, LbxOrderBy.Name, e.Message));
					diagnostic.SetInformation(ApplicationsWinUIStrings.UseDefaultSettings);
					LbxOrderBy.SelectedIndex = 0;
				}

			recents = settings.SettingsFile.GetRecents ("MnuViewActivated");
			if (recents != null)
			{
				bool isChecked = false;
				try { isChecked = bool.Parse(recents[0]); }
				catch (Exception e)
				{
					diagnostic.SetWarning(string.Format(ApplicationsWinUIStrings.ReadSettingsRecentsError, settings.SettingsFileName, "MnuViewActivated", e.Message));
					diagnostic.SetInformation(ApplicationsWinUIStrings.UseDefaultSettings);
				}

				MnuFilterActivated.Checked = isChecked;
				MnuViewActivated.Checked = isChecked;
				MnuFilterAll.Checked = !isChecked;
				MnuViewAll.Checked = !isChecked;
			}

			recents = settings.SettingsFile.GetRecents ("MnuViewCollapse");
			if (recents != null)
			{
				bool isChecked = false;
				try { isChecked = bool.Parse(recents[0]); }
				catch (Exception e)
				{
					diagnostic.SetWarning(string.Format(ApplicationsWinUIStrings.ReadSettingsRecentsError, settings.SettingsFileName, "MnuViewCollapse", e.Message));
					diagnostic.SetInformation(ApplicationsWinUIStrings.UseDefaultSettings);
				}

				MnuViewCollapse.Checked = isChecked;
				TbxCollapse.Checked = isChecked;
			}

            recents = settings.SettingsFile.GetRecents("MnuViewShowClipboardMessage");
            if (recents != null)
            {
                bool isChecked = false;
                try { isChecked = bool.Parse(recents[0]); }
                catch (Exception e)
                {
					diagnostic.SetWarning(string.Format(ApplicationsWinUIStrings.ReadSettingsRecentsError, settings.SettingsFileName, "MnuViewShowClipboardMessage", e.Message));
					diagnostic.SetInformation(ApplicationsWinUIStrings.UseDefaultSettings);
                }

                MnuViewShowClipboardMessage.Checked = isChecked;
            }     
        }

		/// <summary>
		/// Settings save
		/// </summary>
		//---------------------------------------------------------------------
		private void SaveSettings (bool showMessage)
		{
			if (!settings.SaveSettings)
				return;

            if (showMessage && settings.AskSaveSettings)
            {
				if (MessageBox.Show(ApplicationsWinUIStrings.SettingsAskToSave, ApplicationsWinUIStrings.MessageBoxCaption, MessageBoxButtons.YesNo, MessageBoxIcon.Question) == DialogResult.No)
                    return;
            }

			// SetRecents cleanup dom if count is 0
			string [] strings = new string[CbxFindEnumName.Items.Count];
			for (int i = 0; i <= CbxFindEnumName.Items.Count -1; i++)
				strings[i] = (string) CbxFindEnumName.Items[i];

			settings.SettingsFile.SetRecents(CbxFindEnumName.Name, strings);

			strings = new string[CbxFindEnumValue.Items.Count];
			for (int i = 0; i <= CbxFindEnumValue.Items.Count -1; i++)
				strings[i] = (string) CbxFindEnumValue.Items[i];

			settings.SettingsFile.SetRecents(CbxFindEnumValue.Name, strings);

			strings = new string[CbxFindItemName.Items.Count];
			for (int i = 0; i <= CbxFindItemName.Items.Count -1; i++)
				strings[i] = (string) CbxFindItemName.Items[i];

			settings.SettingsFile.SetRecents(CbxFindItemName.Name, strings);

			strings = new string[CbxFindDbValue.Items.Count];
			for (int i = 0; i <= CbxFindDbValue.Items.Count -1; i++)
				strings[i] = (string) CbxFindDbValue.Items[i];

			settings.SettingsFile.SetRecents(CbxFindDbValue.Name, strings);

			strings = new string[1] { LbxOrderBy.SelectedIndex.ToString() };
			settings.SettingsFile.SetRecents(LbxOrderBy.Name, strings);

			strings = new string[1] { MnuFilterActivated.Checked.ToString() };
			settings.SettingsFile.SetRecents("MnuViewActivated", strings);

			strings = new string[1] { MnuViewCollapse.Checked.ToString() };
			settings.SettingsFile.SetRecents("MnuViewCollapse", strings);

            strings = new string[1] { MnuViewShowClipboardMessage.Checked.ToString() };
            settings.SettingsFile.SetRecents("MnuViewShowClipboardMessage", strings);

            settings.SettingsFile.Save ();

			// messages to the user
			if (diagnostic.Error || diagnostic.Warning || diagnostic.Information)
			{
				DiagnosticViewer.ShowDiagnostic(diagnostic);
				diagnostic.Clear ();
			}
		}

		#endregion

		#region Actions

		/// <summary>
		/// Execute the find action requested by the user
		/// </summary>
		//-----------------------------------------------------------------------------
		private void Find ()
		{
			LoadEnumsTable				();

			EnableDisableOrderBy		();
			UpdateColumnsView			();
			EnableDisableCopyOptions	(false);

		}

		/// <summary>
		/// Execute the clear selections action
		/// </summary>
		//-----------------------------------------------------------------------------
		private void ClearSelections ()
		{
			CbxFindEnumName.Text	= string.Empty;
			CbxFindEnumValue.Text	= string.Empty;
			CbxFindItemName.Text	= string.Empty;
			CbxFindDbValue.	Text	= string.Empty;
		}

		/// <summary>
		/// Execute the clear history action
		/// </summary>
		//-----------------------------------------------------------------------------
		private void ClearHistory ()
		{
			if (MessageBox.Show(ApplicationsWinUIStrings.ClearHistoryAskTo, ApplicationsWinUIStrings.MessageBoxCaption, MessageBoxButtons.YesNo, MessageBoxIcon.Question) == DialogResult.No)
				return;

			CbxFindEnumName.	Items.Clear ();
			CbxFindEnumValue.	Items.Clear ();
			CbxFindItemName.	Items.Clear ();
			CbxFindDbValue.		Items.Clear ();

			SaveSettings (true);
		}

		/// <summary>
		/// Execute the copy action requested by the user
		/// </summary>
		//-----------------------------------------------------------------------------
		private void Copy (Settings.CopyAs copyAs)
		{
			ArrayList indices = new ArrayList(LbxEnums.SelectedIndices);

			string s			= string.Empty;
			string tagName		= string.Empty;
			string tagLocName	= string.Empty;
			string tagValue		= string.Empty;

			foreach (int index in indices)
			{

				string	dbValue		= string.Empty;
				string	deflt		= string.Empty;
				string	itemLocName	= string.Empty;
				string	itemValue	= string.Empty;
				string  tempTagValue= LbxEnums.Items[index].SubItems[PosTagValue].Text;
				bool	isAnItem	= tempTagValue == string.Empty;

				DataEnum dataEnum;

				// the next columns could be hidden
				if (TbxCollapse.Checked)
				{
					dataEnum = new DataEnum (ushort.Parse(tempTagValue), 0);
					dbValue = dataEnum.ToString ();
				}
				else
				{
					dbValue		= LbxEnums.Items[index].SubItems[PosDbValue].Text;
					deflt		= LbxEnums.Items[index].SubItems[PosTagDefault].Text;
					itemLocName	= LbxEnums.Items[index].SubItems[PosItemName].Text;
					itemValue	= LbxEnums.Items[index].SubItems[PosItemValue].Text;
					dataEnum	= new DataEnum (UInt32.Parse(dbValue));
				}

				string	itemName = enums.ItemName(dataEnum);
			
				if (itemName == null || itemName == string.Empty)
					itemName = itemLocName;

				if (!isAnItem)
				{
					tagLocName	= LbxEnums.Items[index].SubItems[PosTagName].Text;
					tagValue	= tempTagValue;
					tagName		= enums.TagName(dataEnum);
						
					if (tagName == null || tagName == string.Empty)
						tagName = tagLocName;
				}

				bool tagNameMissing	= (tagName == string.Empty);
				
				// if tag name is missing from the selected items
				// I have to search it into the listbox.
				if (tagNameMissing)
					 SearchPreviousTagName (index, ref tagName, ref tagLocName, ref tagValue);

				// tag name is really missing. Diagnostic message
				tagNameMissing			 = (tagName == string.Empty);
				string tagMissingMessage = string.Format(ApplicationsWinUIStrings.CopyTagNameMissing, "'" + itemLocName + "=" + dbValue + "'");

				switch (copyAs)
				{
					case Settings.CopyAs.As:
						s += tagLocName + DefaultSeparator + tagValue;

						if (itemLocName != string.Empty)  
							s += DefaultSeparator + itemLocName + DefaultSeparator + itemValue + DefaultSeparator + dbValue ;
						
						s += "\r\n";
						break;
					case Settings.CopyAs.AsNames:

						if (tagNameMissing) 
							diagnostic.SetWarning (tagMissingMessage);
						
						s += tagLocName + DefaultSeparator + itemLocName + "\r\n";
						break;
					
					case Settings.CopyAs.AsWoorm:
						if (tagNameMissing) 
							diagnostic.SetWarning (tagMissingMessage);

						s += string.Format (WoormDeclaration, "{", tagValue, itemValue, "}");
						break;

					case Settings.CopyAs.AsCode:
						if (tagNameMissing) 
							diagnostic.SetWarning (tagMissingMessage);

						string tagForEnums	= tagName.ToUpper().Replace(" " , EnumsNameSeparator);
						string itemForEnums = itemName.ToUpper().Replace(" " , EnumsNameSeparator);

						// unsupported chars cleanup
						foreach (char c in EnumsNotAllowed)
						{
							tagForEnums	 = tagForEnums.Replace(c.ToString() , "");
							itemForEnums = itemForEnums.Replace(c.ToString() , "");
						}

						// tag 
						if (!isAnItem)
							s += string.Format (EnumsTagDeclaration, tagForEnums.ToUpper(), tagValue);

						// tag item
						s += string.Format (EnumsItemDeclaration, tagForEnums + EnumsNameSeparator + itemForEnums, tagValue, itemValue, dbValue);
	
						// default value
						if (deflt != string.Empty)
							s += string.Format (EnumsDefaultDeclaration, tagForEnums, tagForEnums + EnumsNameSeparator + itemForEnums);

						break;

					case Settings.CopyAs.AsDbValue:
						if (itemLocName == string.Empty)
							s += string.Format (DbValueDeclaration, tagName,  dbValue);
						else
							s += string.Format (DbValueDeclaration, itemLocName,  dbValue);
						break;
				}
			}

			Clipboard.SetDataObject(s);

            if (MnuViewShowClipboardMessage.Checked)
            {
				diagnostic.SetInformation(ApplicationsWinUIStrings.CopyIntoTheClipboard);
                DiagnosticViewer.ShowDiagnostic(diagnostic);
                diagnostic.Clear();
            }
		}

		/// <summary>
		/// Searches the previous tag name into the listbox starting from a known index
		/// </summary>
		//-----------------------------------------------------------------------------
		private void SearchPreviousTagName (int index, ref string tagName, ref string tagLocName, ref string tagValue)
		{
			tagName		= string.Empty;
			tagLocName	= string.Empty;
			tagValue	= string.Empty;

			for (int i=index; i >= 0; i--)
			{
				tagLocName	= LbxEnums.Items[i].SubItems[PosTagName].Text;
				if (tagLocName == string.Empty)
					continue;

				tagValue			= LbxEnums.Items[i].SubItems[PosTagValue].Text;
				string dbValue		= LbxEnums.Items[i].SubItems[PosDbValue].Text;
				DataEnum dataEnum	= new DataEnum(UInt32.Parse(dbValue));
				tagName				= enums.TagName(dataEnum);
				break;
			}
		}

		/// <summary>
		/// Execute the expand/collapse action requested by the user
		/// </summary>
		//-----------------------------------------------------------------------------
		private void ExpandCollapse (bool collapse)
		{
			MnuViewCollapse.Checked = collapse;
			TbxCollapse.Checked = collapse;
			EnableDisableCopyOptions(LbxEnums.SelectedIndices.Count > 0);

			Find ();
		}
		
		delegate void EnumsViewerExitCallback();
		//---------------------------------------------------------------------------------------
		public void Exit()
		{
			if (this.IsDisposed)
				return;

			// InvokeRequired required compares the thread ID of the calling thread to the 
			// thread ID of the creating thread. If these threads are different, it returns true.
			if (this.InvokeRequired)
			{
				EnumsViewerExitCallback d = new EnumsViewerExitCallback(Exit);
				this.Invoke(d);
			}
			else
				this.Close();
		}


		/// <summary>
		/// Execute Help Request
		/// </summary>
		//-----------------------------------------------------------------------------
		private void CallHelp ()
		{
			Generic.HelpManager.CallOnlineHelp("RefGuide.ClientNet.EnumsViewer", settings.Culture);
		}


		/// <summary>
		/// Shows application Informations
		/// </summary>
		//-----------------------------------------------------------------------------
        private void ShowAppInfo()
        {
            // object to explore info
            IEnumsViewerAppInfo appInfo = new IEnumsViewerAppInfo();
            appInfo.EnumsDefined = enums;
            appInfo.BrandLoader = brandLoader;
            appInfo.ActivationCache = activationCache;

            EnumsViewerAppInfo appInfoDlg = new EnumsViewerAppInfo(appInfo);
            appInfoDlg.Show();
        }

		#endregion

        #region Graphical Events

        /// <summary>
		/// Grants closing operations 
		/// </summary>
		//-----------------------------------------------------------------------------
		private void EnumsDialog_Closing(object sender, System.ComponentModel.CancelEventArgs e)
		{
			SaveSettings(false);
		}

		/// <summary>
		/// Toolbar Buttons Click Management
		/// </summary>
		//-----------------------------------------------------------------------------
		private void ToolbarMain_ButtonClick(object sender, System.Windows.Forms.ToolBarButtonClickEventArgs e)
		{
			switch (e.Button.ImageIndex)
			{
				case 0:
					Find ();
					break;
				case 1:
					ClearSelections ();
					break;
				case 2:
					ClearHistory ();
					break;
				case 3:
					Copy (Settings.CopyAs.As);
					break;
				case 4:
					Copy (Settings.CopyAs.AsNames);
					break;
				case 5:
					Copy (Settings.CopyAs.AsWoorm);
					break;
				case 6:			
					Copy (Settings.CopyAs.AsDbValue);
					break;
				case 7:
                    e.Button.Pushed = !e.Button.Pushed;
					ExpandCollapse (e.Button.Pushed);
					break;
				case 8:
					break;
				case 9:
					SaveSettings (true);
					break;
				case 10:
					Exit ();
					break;
				case 11:
					CallHelp ();
					break;
			}
		}

		/// <summary>
		/// Main Menu Find Click
		/// </summary>
		//-----------------------------------------------------------------------------
		private void MnuFind_Click(object sender, System.EventArgs e)
		{
			Find ();
		}

		/// <summary>
		/// Main Menu Clear History Click
		/// </summary>
		//-----------------------------------------------------------------------------
		private void MnuClearSelections_Click (object sender, System.EventArgs e)
		{
			ClearSelections ();
		}

		/// <summary>
		/// Main Menu Clear History Click
		/// </summary>
		//-----------------------------------------------------------------------------
		private void MnuClearHistory_Click (object sender, System.EventArgs e)
		{
			ClearHistory ();
		}
		
		/// <summary>
		/// Main Menu Find Click
		/// </summary>
		//-----------------------------------------------------------------------------
		private void MnuSaveSettings_Click (object sender, System.EventArgs e)
		{
			SaveSettings (true);
		}
		
		/// <summary>
		/// Main Menu View All
		/// </summary>
		//-----------------------------------------------------------------------------
		private void MnuViewAll_Click(object sender, System.EventArgs e)
		{
			MnuFilterActivated.Checked	= false;
			MnuViewActivated.Checked	= false;
			MnuFilterAll.Checked		= true;
			MnuViewAll.Checked			= true;

			Find ();
		}

		/// <summary>
		/// Main Menu View Only Activated
		/// </summary>
		//-----------------------------------------------------------------------------
		private void MnuViewActivated_Click(object sender, System.EventArgs e)
		{
			MnuFilterActivated.Checked	= true;
			MnuViewActivated.Checked	= true;
			MnuFilterAll.Checked		= false;
			MnuViewAll.Checked			= false;

			Find ();		
		}

		/// <summary>
		/// Main Menu Exit Click
		/// </summary>
		//-----------------------------------------------------------------------------
		private void MnuExit_Click(object sender, System.EventArgs e)
		{
			Exit ();
		}

		/// <summary>
		/// Context Menu Copy Click
		/// </summary>
		//-----------------------------------------------------------------------------
		private void MnuCopy_Click(object sender, System.EventArgs e)
		{
			Copy (Settings.CopyAs.As);
		}

		/// <summary>
		/// Context Menu Copy as Names Only Click
		/// </summary>
		//-----------------------------------------------------------------------------
		private void MnuCopyAsNames_Click(object sender, System.EventArgs e)
		{
			Copy (Settings.CopyAs.AsNames);
		}

		/// <summary>
		/// Context Menu Copy as Woorm Syntax Click
		/// </summary>
		//-----------------------------------------------------------------------------
		private void MnuCopyAsWoorm_Click (object sender, System.EventArgs e)
		{
			Copy (Settings.CopyAs.AsWoorm);
		}

		/// <summary>
		/// Context Menu Copy as code Click
		/// </summary>
		//-----------------------------------------------------------------------------
		private void MnuCopyAsCode_Click (object sender, System.EventArgs e)
		{
			Copy (Settings.CopyAs.AsCode);
		}
		
		/// <summary>
		/// Context Menu Copy as Database values Click
		/// </summary>
		//-----------------------------------------------------------------------------
		private void MnuCopyAsDbValue_Click(object sender, System.EventArgs e)
		{
			Copy (Settings.CopyAs.AsDbValue);
		}

		/// <summary>
		/// Order By Selection
		/// </summary>
		//-----------------------------------------------------------------------------
		private void LbxOrderBy_SelectedIndexChanged(object sender, System.EventArgs e)
		{
			Find ();		
		}

		/// <summary>
		/// Expand / Collapse the elements 
		/// </summary>
		//-----------------------------------------------------------------------------
		private void MnuViewCollapse_Click(object sender, System.EventArgs e)
		{
			MnuViewCollapse.Checked = !MnuViewCollapse.Checked;
			ExpandCollapse (MnuViewCollapse.Checked);
		}

		/// <summary>
		/// Call Help on line
		/// </summary>
		//-----------------------------------------------------------------------------
		private void MnuHelpCall_Click(object sender, System.EventArgs e)
		{
			CallHelp();
		}

        /// <summary>
        /// Show Clipboard Message
        /// </summary>
        //-----------------------------------------------------------------------------
        private void MnuViewShowClipboardMessage_Click(object sender, EventArgs e)
        {
            MnuViewShowClipboardMessage.Checked = !MnuViewShowClipboardMessage.Checked;
        }

        /// <summary>
        /// Shows Application Info Dialog
        /// </summary>
        //-----------------------------------------------------------------------------
        private void MnuViewAppInfo_Click(object sender, EventArgs e)
        {
            ShowAppInfo();
        }

        /// <summary>
        /// Shows Application Info Dialog
        /// </summary>
        //-----------------------------------------------------------------------------
        private void MnuAppInfo_Click(object sender, EventArgs e)
        {
            ShowAppInfo();
        }
               
        /// <summary>
		/// List Box rows selection 
		/// </summary>
		//-----------------------------------------------------------------------------
		private void EnableDisableCopyOptions (bool enabled)
		{
			TbxCopy.Enabled				= enabled;
			TbxCopyAsNames.Enabled		= enabled && !TbxCollapse.Checked;
			TbxCopyAsWoorm.Enabled		= enabled && !TbxCollapse.Checked;
			TbxCopyAsDbValues.Enabled	= enabled ;
			MnuCopy.Enabled				= enabled;
			MnuCopyAsNames.Enabled		= enabled && !TbxCollapse.Checked;
			MnuCopyAsCode.Enabled		= enabled && !TbxCollapse.Checked;
			MnuCopyAsWoorm.Enabled		= enabled && !TbxCollapse.Checked;
			MnuCopyAsDbValue.Enabled	= enabled;
		}

		/// <summary>
		/// Save the state of the columns to hide in collapsed mode 
		/// </summary>
		//-----------------------------------------------------------------------------
		private void SaveColumnsState ()
		{
			if (LbxEnums.Columns[PosTagDefault].Width > 0)
				oldColWidthDefault	= LbxEnums.Columns[PosTagDefault].Width;

			if (LbxEnums.Columns[PosTagDefault].Width > 0)
				oldColWidthItemName	= LbxEnums.Columns[PosItemName].Width;

			if (LbxEnums.Columns[PosItemValue].Width > 0)
				oldColWidthItemValue= LbxEnums.Columns[PosItemValue].Width;

			if (LbxEnums.Columns[PosDbValue].Width > 0)
				oldColWidthDbValue	= LbxEnums.Columns[PosDbValue].Width;
		}

		/// <summary>
		/// Restore the state of the columns to show in expanded mode 
		/// </summary>
		//-----------------------------------------------------------------------------
		private void RestoreColumnsState ()
		{
			LbxEnums.Columns[PosTagDefault].Width	= oldColWidthDefault;
			LbxEnums.Columns[PosItemName].Width		= oldColWidthItemName;
			LbxEnums.Columns[PosItemValue].Width	= oldColWidthItemValue;
			LbxEnums.Columns[PosDbValue].Width		= oldColWidthDbValue;
		}

		/// <summary>
		/// Restore the state of the columns to show in expanded mode 
		/// </summary>
		//-----------------------------------------------------------------------------
		private void HideColumns ()
		{
			LbxEnums.Columns[PosTagDefault].Width	= 0;
			LbxEnums.Columns[PosItemName].Width		= 0;
			LbxEnums.Columns[PosItemValue].Width	= 0;
			LbxEnums.Columns[PosDbValue].Width		= 0;
		}

		//-----------------------------------------------------------------------------
		private void UpdateColumnsView ()
		{
			if (TbxCollapse.Checked)
				HideColumns ();
			else
				RestoreColumnsState();
		}

		//-----------------------------------------------------------------------------
		private void EnableDisableOrderBy ()
		{
			bool disable = (
								CbxFindEnumName.Focused || 
								CbxFindEnumValue.Focused || 
								CbxFindItemName.Focused || 
								CbxFindDbValue.Focused
							) && 
							(
								CbxFindEnumName.Text != string.Empty	||
								CbxFindEnumValue.Text != string.Empty	||
								CbxFindItemName.Text != string.Empty	||
								CbxFindDbValue.Text != string.Empty
							);
			
			// order by disabled when there's only one enum into the listbox
			if (!disable)
			{
				int tagNumbers = 0;
				foreach (ListViewItem item in LbxEnums.Items)
				{
					if (item.SubItems[PosTagValue].Text != string.Empty)
						tagNumbers++;

					if (tagNumbers > 1)
						break;
				}
				
				disable = tagNumbers <= 1;
			}

			LbxOrderBy.Enabled = !disable;
		}
	
		/// <summary>
		/// List Box rows selection 
		/// </summary>
		//-----------------------------------------------------------------------------
		private void LbxEnums_SelectedIndexChanged(object sender, System.EventArgs e)
		{
			EnableDisableCopyOptions (LbxEnums.SelectedIndices.Count > 0);
		}

		/// <summary>
		/// List Box losing focus 
		/// </summary>
		//-----------------------------------------------------------------------------
		private void LbxEnums_Leave(object sender, System.EventArgs e)
		{
			EnableDisableCopyOptions(LbxEnums.SelectedIndices.Count > 0);
		}

		/// <summary>
		/// Resize of columns must to save widths to restore from Collapse
		/// </summary>
		//-----------------------------------------------------------------------------
		private void LbxEnums_Resize(object sender, System.EventArgs e)
		{
			SaveColumnsState ();
		}

		/// <summary>
		/// Search operations disable the order by combo
		/// </summary>
		//-----------------------------------------------------------------------------
		private void CbxFindEnumName_Enter(object sender, System.EventArgs e)
		{
			EnableDisableOrderBy ();
		}

		/// <summary>
		/// Restore the order by combo enable state
		/// </summary>
		//-----------------------------------------------------------------------------
		private void CbxFindEnumName_Leave(object sender, System.EventArgs e)
		{
			EnableDisableOrderBy ();
		}

        /// <summary>
        /// Starts Search on ENTER key
        /// </summary>
        //-----------------------------------------------------------------------------
        private void CbxFindEnumName_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.KeyCode == Keys.Return)
                Find();
        }

		/// <summary>
		/// Search operations disable the order by combo
		/// </summary>
		//-----------------------------------------------------------------------------
		private void CbxFindEnumValue_Enter(object sender, System.EventArgs e)
		{
			EnableDisableOrderBy ();
		}

		/// <summary>
		/// Restore the order by combo enable state
		/// </summary>
		//-----------------------------------------------------------------------------
		private void CbxFindEnumValue_Leave(object sender, System.EventArgs e)
		{
			EnableDisableOrderBy ();
		}

        /// <summary>
        /// Starts Search on ENTER key
        /// </summary>
        //-----------------------------------------------------------------------------
        private void CbxFindEnumValue_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.KeyCode == Keys.Return)
                Find();
        }
        
        /// <summary>
		/// Search operations disable the order by combo
		/// </summary>
		//-----------------------------------------------------------------------------
		private void CbxFindItemName_Enter(object sender, System.EventArgs e)
		{
			EnableDisableOrderBy ();

		}

		/// <summary>
		/// Restore the order by combo enable state
		/// </summary>
		//-----------------------------------------------------------------------------
		private void CbxFindItemName_Leave(object sender, System.EventArgs e)
		{
			EnableDisableOrderBy ();
		}

        /// <summary>
        /// Starts Search on ENTER key
        /// </summary>
        //-----------------------------------------------------------------------------
        private void CbxFindItemName_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.KeyCode == Keys.Return)
                Find();
        }
 
        /// <summary>
		/// Search operations disable the order by combo
		/// </summary>
		//-----------------------------------------------------------------------------
		private void CbxFindDbValue_Enter(object sender, System.EventArgs e)
		{
			EnableDisableOrderBy ();
		}

		/// <summary>
		/// Restore the order by combo enable state
		/// </summary>
		//-----------------------------------------------------------------------------
		private void CbxFindDbValue_Leave(object sender, System.EventArgs e)
		{
			EnableDisableOrderBy ();		
		}

        /// <summary>
        /// Starts Search on ENTER key
        /// </summary>
        //-----------------------------------------------------------------------------
        private void CbxFindDbValue_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.KeyCode == Keys.Return)
                Find();
        }

		/// <summary>
		/// Search operations disable the order by combo
		/// </summary>
		//-----------------------------------------------------------------------------
		private void CbxFindEnumName_TextChanged(object sender, System.EventArgs e)
		{
			EnableDisableOrderBy ();
		}

		/// <summary>
		/// Search operations disable the order by combo
		/// </summary>
		//-----------------------------------------------------------------------------
		private void CbxFindEnumValue_TextChanged(object sender, System.EventArgs e)
		{
			EnableDisableOrderBy ();
		}

		/// <summary>
		/// Search operations disable the order by combo
		/// </summary>
		//-----------------------------------------------------------------------------
		private void CbxFindItemName_TextChanged(object sender, System.EventArgs e)
		{
			EnableDisableOrderBy ();
		}

		/// <summary>
		/// Search operations disable the order by combo
		/// </summary>
		//-----------------------------------------------------------------------------
		private void CbxFindDbValue_TextChanged(object sender, System.EventArgs e)
		{
			EnableDisableOrderBy ();
		}

        /// <summary>
        /// Toolbar Buttons Click Management
        /// </summary>
        //-----------------------------------------------------------------------------
        private void TbxFind_Click(object sender, EventArgs e)
        {
            Find();
        }

        /// <summary>
        /// Toolbar Buttons Click Management
        /// </summary>
        //-----------------------------------------------------------------------------
        private void TbxClearSelections_Click(object sender, EventArgs e)
        {
            ClearSelections();
        }

        /// <summary>
        /// Toolbar Buttons Click Management
        /// </summary>
        //-----------------------------------------------------------------------------
        private void TbxClearHistory_Click(object sender, EventArgs e)
        {
            ClearHistory();
        }

        /// <summary>
        /// Toolbar Buttons Click Management
        /// </summary>
        //-----------------------------------------------------------------------------
        private void TbxCopy_Click(object sender, EventArgs e)
        {
            Copy(Settings.CopyAs.As);
        }

        /// <summary>
        /// Toolbar Buttons Click Management
        /// </summary>
        //-----------------------------------------------------------------------------
        private void TbxCopyAsNames_Click(object sender, EventArgs e)
        {
            Copy(Settings.CopyAs.AsNames);
        }

        /// <summary>
        /// Toolbar Buttons Click Management
        /// </summary>
        //-----------------------------------------------------------------------------
        private void TbxCopyAsWoorm_Click(object sender, EventArgs e)
        {
            Copy(Settings.CopyAs.AsWoorm);
        }

        /// <summary>
        /// Toolbar Buttons Click Management
        /// </summary>
        //-----------------------------------------------------------------------------
        private void TbxCopyAsDbValues_Click(object sender, EventArgs e)
        {
            Copy(Settings.CopyAs.AsDbValue);
        }

        /// <summary>
        /// Toolbar Buttons Click Management
        /// </summary>
        //-----------------------------------------------------------------------------
        private void TbxCollapse_CheckedChanged(object sender, EventArgs e)
        {
            ExpandCollapse(TbxCollapse.Checked);
        }

        /// <summary>
        /// Toolbar Buttons Click Management
        /// </summary>
        //-----------------------------------------------------------------------------
        private void TbxSave_Click(object sender, EventArgs e)
        {
            SaveSettings(true);
        }

        /// <summary>
        /// Toolbar Buttons Click Management
        /// </summary>
        //-----------------------------------------------------------------------------
        private void TbxExit_Click(object sender, EventArgs e)
        {
            Exit();
        }

        /// <summary>
        /// Toolbar Buttons Click Management
        /// </summary>
        //-----------------------------------------------------------------------------
        private void TbxHelp_Click(object sender, EventArgs e)
        {
            CallHelp();
        }

        /// <summary>
        /// Filter only all modules
        /// </summary>
        //-----------------------------------------------------------------------------
        private void MnuFilterAll_Checked(object sender, EventArgs e)
        {
            MnuFilterActivated.CheckOnClick = false;
            MnuFilterActivated.Checked = !MnuFilterAll.Checked;
            MnuFilterActivated.CheckOnClick = true;

            // Main Menu alignment
            MnuViewActivated.Checked = MnuFilterActivated.Checked;
            MnuViewAll.Checked = MnuFilterAll.Checked;

            Find();
        }

        /// <summary>
        /// Filter only activated modules
        /// </summary>
        //-----------------------------------------------------------------------------
        private void MnuFilterActivated_CheckedChanged(object sender, EventArgs e)
        {
            MnuFilterAll.CheckOnClick = false;
            MnuFilterAll.Checked = !MnuFilterActivated.Checked;
            MnuFilterAll.CheckOnClick = true;

            // Main Menu alignment
            MnuViewActivated.Checked = MnuFilterActivated.Checked;
            MnuViewAll.Checked = MnuFilterAll.Checked;

            Find();
        }

        /// <summary>
        /// Form Close
        /// </summary>
        //-----------------------------------------------------------------------------
        private void EnumsDialog_FormClosing(object sender, FormClosingEventArgs e)
        {
			if (!settings.AskSaveSettings)
				return;

			if (MessageBox.Show(this, ApplicationsWinUIStrings.DoYouWantToExit, ApplicationsWinUIStrings.MessageBoxCaption, MessageBoxButtons.YesNo, MessageBoxIcon.Question) == DialogResult.No)
                e.Cancel = true;
        }



    #endregion
  }


	/// <summary>
	/// Class for Settings Initialization
	/// </summary>
	//============================================================================
	public class Settings
	{
		/// <summary>
		/// Settings File Management
		/// </summary>
		//=============================================================================        
		internal class File
		{
			#region XmlGrammar
			
			/// <summary>
			/// Settings File Xml Grammar
			/// </summary>
			//=============================================================================        
			internal class XmlGrammar
			{
				public static char[] RecentsSeparator	=  new char [] { '/','*',',','*','/' };
				public static string Version			=  "1.0";
				public static string Encoding			=  "utf-8";
				public static string Standalone			=  "yes";
						
				//----------------------------------------------------------------------------------------------
				public class Elements
				{
					public static string EnumsViewer	        = "EnumsViewer";
					public static string Recents		        = "Recents";
					public static string Recent			        = "Recent";
				}

				//----------------------------------------------------------------------------------------------
				public class Attributes
				{
					public static string Control		= "control";
					public static string Value			= "value";
				}

				//----------------------------------------------------------------------------------------------
				public class Queries
				{
					public static string ParameterChar =  "%";

					public static string SelectRoot				            = "/" + Elements.EnumsViewer;
					public static string SelectRecents			            = SelectRoot + "/" + Elements.Recents;
					public static string SelectRecentsForControl            = SelectRecents + "/" + Elements.Recent + "[@" + Attributes.Control + "='" + ParameterChar + "']";
                }
			}
			
			#endregion

			#region Data Members
		
			//-----------------------------------------------------------------------------
			public static string DefaultName = "EnumsViewerSettings.xml";

			//-----------------------------------------------------------------------------
			private bool							loaded		= false;
			private string							fileName	= string.Empty;
			private XmlDocument						doc			= new XmlDocument();
			private	Diagnostic	diagnostic	= null;

			#endregion

			#region Properties
			
			public bool							Loaded			{ get { return loaded; } }
			public Diagnostic Diagnostic		{ get { return diagnostic; } set { diagnostic = value; } }


			#endregion

			#region Constructors and Initialization

			/// <summary>
			/// Default Constructor
			/// </summary>
			//-----------------------------------------------------------------------------
			public File ()
			{
			}
	
			#endregion

			#region Read and Write Management
			
			/// <summary>
			/// Loads the Settings File
			/// </summary>
			//-----------------------------------------------------------------------------
			public bool Load (string fileName)
			{
				this.fileName = fileName;
				
				try 
				{
					doc.Load(fileName);
					
					// root checking
					XmlNode root = doc.SelectSingleNode (XmlGrammar.Queries.SelectRoot);

					if (root == null)
					{
						loaded = false;
						diagnostic.SetWarning(string.Format(ApplicationsWinUIStrings.ReadSettingsFileNoRootError, fileName, XmlGrammar.Elements.EnumsViewer));
						diagnostic.SetInformation(ApplicationsWinUIStrings.UseDefaultSettings);
					}
					else
						loaded = true;
				}
				catch (FileNotFoundException)
				{
					loaded = false;
				}
				catch (XmlException e)
				{
					loaded = false;
					diagnostic.SetWarning(string.Format(ApplicationsWinUIStrings.ReadSettingsFileError, fileName, e.Message));
					diagnostic.SetInformation(ApplicationsWinUIStrings.UseDefaultSettings);
				}
				catch (Exception e)
				{
					loaded = false;
					diagnostic.SetError(string.Format(ApplicationsWinUIStrings.GenericExceptionError, e.Message));
				}

				return loaded;
			}

			/// <summary>
			/// Loads the Settings File
			/// </summary>
			//-----------------------------------------------------------------------------
			public bool Save ()
			{
				try 
				{
					doc.Save (fileName);
				}
				catch (XmlException e)
				{
					diagnostic.SetWarning(string.Format(ApplicationsWinUIStrings.WriteSettingsFileError, fileName, e.Message));
					diagnostic.SetInformation(ApplicationsWinUIStrings.UseDefaultSettings);
				}
				catch (Exception e)
				{
					diagnostic.SetError(string.Format(ApplicationsWinUIStrings.GenericExceptionError, e.Message));
					return false;
				}
				
				return true;
			}
            
			/// <summary>
			/// Gets recents Values for requested control name
			/// </summary>
			//-----------------------------------------------------------------------------
			public string[] GetRecents (string controlName)
			{
				if (!loaded)
					return null;

				string query = XmlGrammar.Queries.SelectRecentsForControl;
				query = query.Replace (XmlGrammar.Queries.ParameterChar, controlName);

				ArrayList recents = new ArrayList();

				try 
				{
					XmlNode recentsNode = doc.SelectSingleNode (query);

					if (recentsNode != null)
					{
						XmlAttribute attribute = recentsNode.Attributes[XmlGrammar.Attributes.Value];
						
						if (attribute != null && attribute.Value != string.Empty)
							return ReadRecentsAttribute(attribute.Value);
					}
				}
				catch (XmlException e)
				{
					diagnostic.SetWarning(string.Format(ApplicationsWinUIStrings.ReadSettingsRecentsError, fileName, controlName, e.Message));
					diagnostic.SetInformation(ApplicationsWinUIStrings.UseDefaultSettings);
				}
				catch (Exception e)
				{
					loaded = false;
					diagnostic.SetError(string.Format(ApplicationsWinUIStrings.GenericExceptionError, e.Message));
				}
		
				return null;
			}

			/// <summary>
			/// Sets recents Values for requested control name
			/// </summary>
			//-----------------------------------------------------------------------------
			public void SetRecents (string controlName, string[] strings)
			{
				if (strings.Length > 0 && !PrepareHeader ())
					return;

				string query = XmlGrammar.Queries.SelectRecentsForControl;
				query = query.Replace (XmlGrammar.Queries.ParameterChar, controlName);

				try 
				{
					XmlNode recentRootNode		= doc.SelectSingleNode(XmlGrammar.Queries.SelectRecents);
					XmlNodeList recentsNodes	= doc.SelectNodes (query);

					// verifico se ne esistono e cancello i precedenti
					if (recentsNodes != null)
					{
						foreach (XmlNode node in recentsNodes)
							recentRootNode.RemoveChild(node);
					}

					if (strings.Length == 0)
						return;

					// new node for recents
					XmlNode recent = doc.CreateNode (XmlNodeType.Element, XmlGrammar.Elements.Recent, "");
					
					// attributes
					XmlAttribute newAttribute = doc.CreateAttribute("", XmlGrammar.Attributes.Control, "");
					newAttribute.Value = controlName;
					recent.Attributes.Append(newAttribute);

					newAttribute = doc.CreateAttribute("", XmlGrammar.Attributes.Value, "");
					newAttribute.Value = PrepareRecentsToSave(strings);
					recent.Attributes.Append(newAttribute);
					
					recentRootNode.AppendChild (recent);
				}
				catch (XmlException e)
				{
					diagnostic.SetWarning(string.Format(ApplicationsWinUIStrings.PrepareSettingsRecentsError, fileName, controlName, e.Message));
					diagnostic.SetInformation(ApplicationsWinUIStrings.UseDefaultSettings);
				}
				catch (Exception e)
				{
					diagnostic.SetError(string.Format(ApplicationsWinUIStrings.GenericExceptionError, e.Message));
				}
			}

			/// <summary>
			/// Prepares Recents Syntax 
			/// </summary>
			//-----------------------------------------------------------------------------
			private string PrepareRecentsToSave (string[] strings)
			{
				string recent	= string.Empty;
				string separator= string.Empty;

				foreach (char c in XmlGrammar.RecentsSeparator)
					separator += c;
			
				for (int i=0; i <= (strings.Length -1); i++)
					recent += strings[i] + (i < (strings.Length -1) ? separator : "");

				return recent;
			}

			/// <summary>
			/// Prepares Recents Syntax 
			/// </summary>
			//-----------------------------------------------------------------------------
			private string[] ReadRecentsAttribute (string attribute)
			{
				return attribute.Split(XmlGrammar.RecentsSeparator);
			}

			/// <summary>
			/// Prepares the file Header Syntax 
			/// </summary>
			//-----------------------------------------------------------------------------
			private bool PrepareHeader ()
			{
				try 
				{
					if (doc == null)
						doc = new XmlDocument();
					
					XmlNode root = doc.SelectSingleNode(XmlGrammar.Elements.EnumsViewer);
					if (root == null)
					{
						if (!doc.HasChildNodes)
							doc.AppendChild(doc.CreateXmlDeclaration(XmlGrammar.Version, XmlGrammar.Encoding, XmlGrammar.Standalone));
						root = doc.CreateNode (XmlNodeType.Element, XmlGrammar.Elements.EnumsViewer, "");
						doc.AppendChild(root);
					}

					XmlNode recents = doc.SelectSingleNode(XmlGrammar.Queries.SelectRecents);
					if (recents == null)
					{
						recents = doc.CreateNode (XmlNodeType.Element, XmlGrammar.Elements.Recents, "");
						root.AppendChild(recents);
					}
				}
				catch (XmlException e)
				{
					diagnostic.SetWarning(string.Format(ApplicationsWinUIStrings.PrepareSettingsHeaderError, fileName, e.Message));
					return false;
				}

				return true;
			}

			#endregion
		}

		#region Data Members

		/// <summary>
		/// Enumerations
		/// </summary>
		//-----------------------------------------------------------------------------
		public enum OrderByMode { EnumName, EnumValue };
		public enum CopyAs		{ As, AsNames, AsWoorm, AsCode, AsDbValue };

		/// <summary>
		/// Mandatory Data members
		/// </summary>
		//-----------------------------------------------------------------------------
		private string instance	= string.Empty;

		
		/// <summary>
		/// Optional Data members
		/// </summary>
		//-----------------------------------------------------------------------------
		private string		remoteServer		= string.Empty;
		private string		culture				= string.Empty;
		private string		application			= string.Empty;
		private string		configuration		= string.Empty;
		private string		authenticationToken = string.Empty;
		private bool		saveSettings		= true;
        private bool        askSaveSettings     = true;
        private string      settingsFileName    = string.Empty;
		private File		settingsFile		= new File();
		private OrderByMode orderBy				= OrderByMode.EnumName;
		private bool		vsNetIntegrated		= false;
		#endregion
	
		#region Properties

		public string		Instance		{	
												get { return instance; }			
												set { if (! CheckInstance (value)) instance = string.Empty;  }
											}
		public string RemoteServer				{ get { return remoteServer; }		set { remoteServer		= value; } }
		public string		Application			{ get { return application; }		set { application		= value; } }
		public string		Configuration		{ get { return configuration; }		set { configuration		= value; } }
		public string		Culture				{ get { return culture; }			set { culture			= value; } }
        public bool         AskSaveSettings 	{ get { return askSaveSettings; }   set { askSaveSettings   = value; } }
        public bool         SaveSettings    	{ get { return saveSettings; }      set { saveSettings      = value; } }
		public string		SettingsFileName	{ get { return settingsFileName; }	set { settingsFileName	= value; } }
		public OrderByMode	OrderBy				{ get { return orderBy; }			set { orderBy			= value; } }
		public bool			VsNetIntegrated		{ get { return vsNetIntegrated; }	set { vsNetIntegrated	= value; } }
		public string		AuthenticationToken	{ get { return authenticationToken; } set { authenticationToken = value; } }
		
		internal File		SettingsFile	{ get { return settingsFile; } }

		#endregion

		#region Constructors and Initialization

		/// <summary>
		/// Mandatory Parameters Constructor
		/// </summary>
		//-----------------------------------------------------------------------------
		public Settings (string instance)
		{
			CheckInstance (instance);
			// default recents file initialization
			InitDefaultSettingsFile ();
		}

		/// <summary>
		/// Check the correct initialization of the mandatory settings 
		/// </summary>
		//-----------------------------------------------------------------------------
		private bool CheckInstance (string instance)
		{
			this.instance = BasePathFinder.BasePathFinderInstance.Installation;
			this.remoteServer = BasePathFinder.BasePathFinderInstance.RemoteFileServer;
			return true;
		}

		/// <summary>
		/// Check the correct initialization of the mandatory settings 
		/// </summary>
		//-----------------------------------------------------------------------------
		private void InitDefaultSettingsFile ()
		{
			if (saveSettings)
				settingsFileName = Path.Combine(BasePathFinder.BasePathFinderInstance.GetAppDataPath(true), File.DefaultName);
			else
				settingsFileName = string.Empty;
		}
				
		/// <summary>
		/// Check the correct initialization of the instance 
		/// </summary>
		//-----------------------------------------------------------------------------

		#endregion
	}

	/// <summary>
	/// Exception for the calling assemblies
	/// </summary>
	//============================================================================
	public class EnumsDialogException : System.Exception
	{
		#region Data Members

		private bool cannotRun = false;

		#endregion

		#region Properties

		public bool CannotRun { get { return cannotRun; } }

		#endregion

		#region Constructors

		/// <summary>
		/// Constructor
		/// </summary>
		//-----------------------------------------------------------------------------
		public EnumsDialogException (string message, bool cannotRun)
			:
			base (message)
		{
			this.cannotRun = cannotRun;
		}

		#endregion
	}
}
