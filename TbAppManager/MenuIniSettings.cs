using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Configuration;
using System.Diagnostics;
using System.Globalization;
using System.IO;
using System.Windows.Forms;
using System.Xml;
using System.Xml.Schema;
using System.Xml.Serialization;

namespace Microarea.MenuManager
{
	/// <summary>
	/// Summary description for MenuFormIniSettings.
	/// </summary>
	public class MenuFormIniSettings
	{
		public const string CFG_APP_SETTING_APPLICATION					= "Application";
		public const string CFG_APP_SETTING_GROUP						= "Group";
		public const string CFG_APP_SETTING_MENU						= "Menu";
		public const string CFG_APP_SETTING_COMMAND						= "Command";
		public const string	CFG_APP_SETTING_FRAME_WINDOW_STATE			= "WindowState";
		public const string CFG_APP_SETTING_FRAME_X_POS					= "FrameXPos";
		public const string CFG_APP_SETTING_FRAME_Y_POS					= "FrameYPos";
		public const string CFG_APP_SETTING_FRAMEWIDTH					= "FrameWidth";
		public const string CFG_APP_SETTING_FRAMEHEIGHT					= "FrameHeight";
		public const string CFG_APP_SETTING_APPSPANELWIDTH				= "ApplicationsPanelWidth";
		public const string CFG_APP_SETTING_TREEWIDTH					= "MenuTreeWidth";
		public const string CFG_APP_SETTING_SHOW_TYPE					= "ShowType";
		public const string CFG_APP_SETTING_SHOWSHORTCUTS				= "ShowShortcuts";
        public const string CFG_APP_SETTING_SHOWSTARTUPSHORTCUTS        = "ShowStartupShortcuts";

        public const string CFG_APP_SHOW_TOOLBAR_BUTTONS_TEXT           = "ShowToolBarButtonsText";
		public const string CFG_APP_SETTING_ENHANCED_COMMANDSVIEW		= "EnhancedCommandsView";
		public const string CFG_APP_SETTING_ENHANCED_SHOW_FLAGS			= "EnhancedCommandsShowFlags";
		public const string CFG_APP_SETTING_ENHANCED_SHOW_TOOLBAR		= "ShowEnhancedCommandsToolBar";
		public const string CFG_APP_SETTING_ENHANCED_SHOW_DESCRIPTIONS	= "ShowEnhancedCommandsDescriptions";
		public const string CFG_APP_SETTING_ENHANCED_SHOW_REPORT_DATES	= "ShowEnhancedCommandsReportDates";
		public const string CFG_APP_SETTING_DOCKABLE_WINDOWS			= "DockableWindows";

		private string	applicationName						= String.Empty;
		private string	groupName							= String.Empty;	
		private string	menuPath	   						= String.Empty;
		private string	commandPath   						= String.Empty;
		private int		frameWindowState					= (int)FormWindowState.Normal;
		private int		frameXPos							= 0;
		private int		frameYPos							= 0;
		private int		frameWidth							= 0;
		private int		frameHeight							= 0;
		private int		applicationsPanelWidth				= 0;
		private int		menuTreeWidth						= 0;
		private int		showType							= 0;
		private bool	showShortcuts						= true;
		private bool	showStartupShortcuts				= false;
		private bool	enhancedCommandsView				= false;
		private int		enhancedCommandsShowFlags			= -1;	
		private bool	showEnhancedCommandsToolBar			= true;
		private bool	showEnhancedCommandsDescriptions	= false;
		private bool	showEnhancedCommandsReportDates		= false;
		private bool	showToolBarButtonsText				= true;
		private bool	dockableWindows						= true;

		private string lastError;

		//--------------------------------------------------------------------------------------------------------------------------------
		public MenuFormIniSettings()
		{
		}

		//--------------------------------------------------------------------------------------------------------------------------------
		public MenuFormIniSettings(MenuMngForm aMenuMngform) : this()
		{
			if (aMenuMngform == null)
				return;

	
			frameWindowState					= (int)aMenuMngform.WindowState;
			frameXPos							= aMenuMngform.Location.X;
			frameYPos							= aMenuMngform.Location.Y;
			frameWidth							= Math.Max(aMenuMngform.Width, aMenuMngform.MinimumSize.Width);
			frameHeight							= Math.Max(aMenuMngform.Height, aMenuMngform.MinimumSize.Height);
	

				showType = 0;
		}

		//--------------------------------------------------------------------------------------------------------------------------------
		internal string ApplicationName		{ get {	return applicationName;	} }
		//--------------------------------------------------------------------------------------------------------------------------------
		internal string GroupName			{ get { return groupName; } }
		//--------------------------------------------------------------------------------------------------------------------------------
		internal string MenuPath			{ get { return menuPath; }	}
		//--------------------------------------------------------------------------------------------------------------------------------
		internal string CommandPath			{ get {	return commandPath;	}}
		//--------------------------------------------------------------------------------------------------------------------------------
		internal FormWindowState WindowState{ get {	return (FormWindowState)frameWindowState; } set { frameWindowState = (int)value; } }
		//--------------------------------------------------------------------------------------------------------------------------------
		internal int FrameXPos				{ get {	return frameXPos; } }
		//--------------------------------------------------------------------------------------------------------------------------------
		internal int FrameYPos				{ get { return frameYPos; } }
		//--------------------------------------------------------------------------------------------------------------------------------
		internal int FrameWidth				{ get {	return frameWidth; } }
		//--------------------------------------------------------------------------------------------------------------------------------
		internal int FrameHeight			{ get { return frameHeight; } }
		//--------------------------------------------------------------------------------------------------------------------------------
		internal int ApplicationsPanelWidth	{ get { return applicationsPanelWidth; } }
		//--------------------------------------------------------------------------------------------------------------------------------
		internal int MenuTreeWidth			{ get { return menuTreeWidth; } }
		//--------------------------------------------------------------------------------------------------------------------------------
		internal bool ShowFavorites			{ get { return showType == 1; } }
		//--------------------------------------------------------------------------------------------------------------------------------
		internal bool ShowEnvironment		{ get { return showType == 2; } }
		//--------------------------------------------------------------------------------------------------------------------------------
		internal bool ShowShortcuts			{ get { return showShortcuts; }	}
		//--------------------------------------------------------------------------------------------------------------------------------
        internal bool ShowStartupShortcuts { get { return showStartupShortcuts; } }
        //--------------------------------------------------------------------------------------------------------------------------------
        internal bool ShowEnhancedCommandsView { get { return enhancedCommandsView; } }
		//--------------------------------------------------------------------------------------------------------------------------------
		internal int EnhancedCommandsShowFlags { get { return enhancedCommandsShowFlags; }	}
		//--------------------------------------------------------------------------------------------------------------------------------
		internal bool ShowEnhancedCommandsToolBar { get { return showEnhancedCommandsToolBar; }	}
		//--------------------------------------------------------------------------------------------------------------------------------
		internal bool ShowEnhancedCommandsDescriptions { get { return showEnhancedCommandsDescriptions; }	}
		//--------------------------------------------------------------------------------------------------------------------------------
		internal bool ShowEnhancedCommandsReportDates { get { return showEnhancedCommandsReportDates; }	}
		//--------------------------------------------------------------------------------------------------------------------------------
		internal bool ShowToolBarButtonsText { get { return showToolBarButtonsText; } }
		//--------------------------------------------------------------------------------------------------------------------------------
		internal bool DockableWindows { get { return dockableWindows; } set { dockableWindows = value; } }

        //--------------------------------------------------------------------------------------------------------------------------------
        private bool LoadFromConfigurationTheOldWay()
		{
			// ConfigurationSettings.GetConfig returns configuration settings for a user-defined configuration section.
			// ConfigurationSettings.AppSettings gets configuration settings in the <appSettings> Element configuration section.
            NameValueCollection config = (NameValueCollection)ConfigurationManager.AppSettings;
			if (config == null)
				return false;

			try
			{
				applicationName = (string)config.Get(CFG_APP_SETTING_APPLICATION);
				groupName		= (string)config.Get(CFG_APP_SETTING_GROUP);
				menuPath		= (string)config.Get(CFG_APP_SETTING_MENU);
				commandPath		= (string)config.Get(CFG_APP_SETTING_COMMAND);

				if (config.Get(CFG_APP_SETTING_FRAME_WINDOW_STATE) != null)
					frameWindowState = Convert.ToInt32(config.Get(CFG_APP_SETTING_FRAME_WINDOW_STATE));
				if (config.Get(CFG_APP_SETTING_FRAME_X_POS) != null)
					frameXPos = Convert.ToInt32(config.Get(CFG_APP_SETTING_FRAME_X_POS));
				if (config.Get(CFG_APP_SETTING_FRAME_Y_POS) != null)
					frameYPos = Convert.ToInt32(config.Get(CFG_APP_SETTING_FRAME_Y_POS));
				if (config.Get(CFG_APP_SETTING_FRAMEWIDTH) != null)
					frameWidth = Convert.ToInt32(config.Get(CFG_APP_SETTING_FRAMEWIDTH));
				if (config.Get(CFG_APP_SETTING_FRAMEHEIGHT) != null)
					frameHeight = Convert.ToInt32(config.Get(CFG_APP_SETTING_FRAMEHEIGHT));
				if (config.Get(CFG_APP_SETTING_APPSPANELWIDTH) != null)
					applicationsPanelWidth = Convert.ToInt32(config.Get(CFG_APP_SETTING_APPSPANELWIDTH));
				if (config.Get(CFG_APP_SETTING_TREEWIDTH) != null)
					menuTreeWidth = Convert.ToInt32(config.Get(CFG_APP_SETTING_TREEWIDTH));
				if (config.Get(CFG_APP_SETTING_SHOW_TYPE) != null)
					showType = Convert.ToInt32(config.Get(CFG_APP_SETTING_SHOW_TYPE));
				if (config.Get(CFG_APP_SETTING_SHOWSHORTCUTS) != null)
					showShortcuts = Convert.ToBoolean(config.Get(CFG_APP_SETTING_SHOWSHORTCUTS));				
				if (config.Get(CFG_APP_SETTING_SHOWSTARTUPSHORTCUTS) != null)
					showStartupShortcuts = Convert.ToBoolean(config.Get(CFG_APP_SETTING_SHOWSTARTUPSHORTCUTS));				
				if (config.Get(CFG_APP_SETTING_ENHANCED_COMMANDSVIEW) != null)
					enhancedCommandsView = Convert.ToBoolean(config.Get(CFG_APP_SETTING_ENHANCED_COMMANDSVIEW));
				if (config.Get(CFG_APP_SETTING_ENHANCED_SHOW_FLAGS) != null)
					enhancedCommandsShowFlags = Convert.ToInt32(config.Get(CFG_APP_SETTING_ENHANCED_SHOW_FLAGS));
				if (config.Get(CFG_APP_SETTING_ENHANCED_SHOW_TOOLBAR) != null)
					showEnhancedCommandsToolBar = Convert.ToBoolean(config.Get(CFG_APP_SETTING_ENHANCED_SHOW_TOOLBAR));
				if (config.Get(CFG_APP_SETTING_ENHANCED_SHOW_DESCRIPTIONS) != null)
					showEnhancedCommandsDescriptions = Convert.ToBoolean(config.Get(CFG_APP_SETTING_ENHANCED_SHOW_DESCRIPTIONS));
				if (config.Get(CFG_APP_SETTING_ENHANCED_SHOW_REPORT_DATES) != null)
					showEnhancedCommandsReportDates = Convert.ToBoolean(config.Get(CFG_APP_SETTING_ENHANCED_SHOW_REPORT_DATES));
				if (config.Get(CFG_APP_SHOW_TOOLBAR_BUTTONS_TEXT) != null)
					showToolBarButtonsText = Convert.ToBoolean(config.Get(CFG_APP_SHOW_TOOLBAR_BUTTONS_TEXT));
				if (config.Get(CFG_APP_SETTING_DOCKABLE_WINDOWS) != null)
					dockableWindows = Convert.ToBoolean(config.Get(CFG_APP_SETTING_DOCKABLE_WINDOWS));

				return true;
			}
			catch(FormatException formatException)
			{
				Debug.Fail("MenuFormIniSettings.LoadFromConfiguration Error: " + formatException.Message);
			}
			catch(OverflowException overflowException)
			{
				Debug.Fail("MenuFormIniSettings.LoadFromConfiguration Error: " + overflowException.Message);
			}
	
			return false;
		}

		//--------------------------------------------------------------------------------------------------------------------------------
		public bool LoadFromConfiguration()
		{
			Settings settings = Settings.Load();
			if (settings == null || settings.IsEmpty)
			{
				return LoadFromConfigurationTheOldWay();
			}

			try
			{
				applicationName = (string)settings.Item[CFG_APP_SETTING_APPLICATION];
				groupName = (string)settings.Item[CFG_APP_SETTING_GROUP];
				menuPath = (string)settings.Item[CFG_APP_SETTING_MENU];
				commandPath = (string)settings.Item[CFG_APP_SETTING_COMMAND];

				if (settings.Item[CFG_APP_SETTING_FRAME_WINDOW_STATE] != null)
					frameWindowState = Convert.ToInt32(settings.Item[CFG_APP_SETTING_FRAME_WINDOW_STATE]);
				if (settings.Item[CFG_APP_SETTING_FRAME_X_POS] != null)
					frameXPos = Convert.ToInt32(settings.Item[CFG_APP_SETTING_FRAME_X_POS]);
				if (settings.Item[CFG_APP_SETTING_FRAME_Y_POS] != null)
					frameYPos = Convert.ToInt32(settings.Item[CFG_APP_SETTING_FRAME_Y_POS]);
				if (settings.Item[CFG_APP_SETTING_FRAMEWIDTH] != null)
					frameWidth = Convert.ToInt32(settings.Item[CFG_APP_SETTING_FRAMEWIDTH]);
				if (settings.Item[CFG_APP_SETTING_FRAMEHEIGHT] != null)
					frameHeight = Convert.ToInt32(settings.Item[CFG_APP_SETTING_FRAMEHEIGHT]);
				if (settings.Item[CFG_APP_SETTING_APPSPANELWIDTH] != null)
					applicationsPanelWidth = Convert.ToInt32(settings.Item[CFG_APP_SETTING_APPSPANELWIDTH]);
				if (settings.Item[CFG_APP_SETTING_TREEWIDTH] != null)
					menuTreeWidth = Convert.ToInt32(settings.Item[CFG_APP_SETTING_TREEWIDTH]);
				if (settings.Item[CFG_APP_SETTING_SHOW_TYPE] != null)
					showType = Convert.ToInt32(settings.Item[CFG_APP_SETTING_SHOW_TYPE]);
				if (settings.Item[CFG_APP_SETTING_SHOWSHORTCUTS] != null)
					showShortcuts = Convert.ToBoolean(settings.Item[CFG_APP_SETTING_SHOWSHORTCUTS]);
				if (settings.Item[CFG_APP_SETTING_SHOWSTARTUPSHORTCUTS] != null)
					showStartupShortcuts = Convert.ToBoolean(settings.Item[CFG_APP_SETTING_SHOWSTARTUPSHORTCUTS]);
				if (settings.Item[CFG_APP_SETTING_ENHANCED_COMMANDSVIEW] != null)
					enhancedCommandsView = Convert.ToBoolean(settings.Item[CFG_APP_SETTING_ENHANCED_COMMANDSVIEW]);
				if (settings.Item[CFG_APP_SETTING_ENHANCED_SHOW_FLAGS] != null)
					enhancedCommandsShowFlags = Convert.ToInt32(settings.Item[CFG_APP_SETTING_ENHANCED_SHOW_FLAGS]);
				if (settings.Item[CFG_APP_SETTING_ENHANCED_SHOW_TOOLBAR] != null)
					showEnhancedCommandsToolBar = Convert.ToBoolean(settings.Item[CFG_APP_SETTING_ENHANCED_SHOW_TOOLBAR]);
				if (settings.Item[CFG_APP_SETTING_ENHANCED_SHOW_DESCRIPTIONS] != null)
					showEnhancedCommandsDescriptions = Convert.ToBoolean(settings.Item[CFG_APP_SETTING_ENHANCED_SHOW_DESCRIPTIONS]);
				if (settings.Item[CFG_APP_SETTING_ENHANCED_SHOW_REPORT_DATES] != null)
					showEnhancedCommandsReportDates = Convert.ToBoolean(settings.Item[CFG_APP_SETTING_ENHANCED_SHOW_REPORT_DATES]);
				if (settings.Item[CFG_APP_SHOW_TOOLBAR_BUTTONS_TEXT] != null)
					showToolBarButtonsText = Convert.ToBoolean(settings.Item[CFG_APP_SHOW_TOOLBAR_BUTTONS_TEXT]);
				if (settings.Item[CFG_APP_SETTING_DOCKABLE_WINDOWS] != null)
					dockableWindows = Convert.ToBoolean(settings.Item[CFG_APP_SETTING_DOCKABLE_WINDOWS]);

				return true;
			}
			catch (FormatException formatException)
			{
				Debug.Fail("MenuFormIniSettings.LoadFromConfiguration Error: " + formatException.Message);
			}
			catch (OverflowException overflowException)
			{
				Debug.Fail("MenuFormIniSettings.LoadFromConfiguration Error: " + overflowException.Message);
			}

			return false;
		}

		//--------------------------------------------------------------------------------------------------------------------------------
		public bool SaveToConfiguration()
		{
			try
			{
				Settings settings = Settings.Load();
				if (settings == null)
					return false;

				settings.Item[CFG_APP_SETTING_APPLICATION] = applicationName;
				settings.Item[CFG_APP_SETTING_GROUP] = groupName;
				settings.Item[CFG_APP_SETTING_MENU] = menuPath;
				settings.Item[CFG_APP_SETTING_COMMAND] = commandPath;

				settings.Item[CFG_APP_SETTING_FRAME_WINDOW_STATE] = frameWindowState.ToString(CultureInfo.InvariantCulture);
				settings.Item[CFG_APP_SETTING_FRAME_X_POS] = frameXPos.ToString(CultureInfo.InvariantCulture);
				settings.Item[CFG_APP_SETTING_FRAME_Y_POS] = frameYPos.ToString(CultureInfo.InvariantCulture);
				settings.Item[CFG_APP_SETTING_FRAMEWIDTH] = frameWidth.ToString(CultureInfo.InvariantCulture);
				settings.Item[CFG_APP_SETTING_FRAMEHEIGHT] = frameHeight.ToString(CultureInfo.InvariantCulture);
				settings.Item[CFG_APP_SETTING_APPSPANELWIDTH] = applicationsPanelWidth.ToString(CultureInfo.InvariantCulture);
				settings.Item[CFG_APP_SETTING_TREEWIDTH] = menuTreeWidth.ToString(CultureInfo.InvariantCulture);
				settings.Item[CFG_APP_SETTING_SHOW_TYPE] = showType.ToString(CultureInfo.InvariantCulture);
				settings.Item[CFG_APP_SETTING_SHOWSHORTCUTS] = showShortcuts.ToString(CultureInfo.InvariantCulture);
				settings.Item[CFG_APP_SETTING_SHOWSTARTUPSHORTCUTS] = showStartupShortcuts.ToString(CultureInfo.InvariantCulture);
				settings.Item[CFG_APP_SETTING_ENHANCED_COMMANDSVIEW] = enhancedCommandsView.ToString(CultureInfo.InvariantCulture);
				settings.Item[CFG_APP_SETTING_ENHANCED_SHOW_FLAGS] = enhancedCommandsShowFlags.ToString(CultureInfo.InvariantCulture);
				settings.Item[CFG_APP_SETTING_ENHANCED_SHOW_TOOLBAR] = showEnhancedCommandsToolBar.ToString(CultureInfo.InvariantCulture);
				settings.Item[CFG_APP_SETTING_ENHANCED_SHOW_DESCRIPTIONS] = showEnhancedCommandsDescriptions.ToString(CultureInfo.InvariantCulture);
				settings.Item[CFG_APP_SETTING_ENHANCED_SHOW_REPORT_DATES] = showEnhancedCommandsReportDates.ToString(CultureInfo.InvariantCulture);
				settings.Item[CFG_APP_SHOW_TOOLBAR_BUTTONS_TEXT] = showToolBarButtonsText.ToString(CultureInfo.InvariantCulture);
				settings.Item[CFG_APP_SETTING_DOCKABLE_WINDOWS] = dockableWindows.ToString(CultureInfo.InvariantCulture);

				settings.Save();
				return true;
			}
			catch(XmlException exception) 
			{
				lastError = exception.Message;
				Debug.Fail("MenuFormIniSettings.SaveToConfiguration Error: " + lastError);
				return false;
			}		
			catch(IOException exception) 
			{
				lastError = exception.Message;
				Debug.Fail("MenuFormIniSettings.SaveToConfiguration Error: " + lastError);
				return false;
			}		
			catch(UnauthorizedAccessException exception)
			{
				// L'utente non possiede le autorizzazioni necessarie per salvare il file
				lastError = exception.Message;
				Debug.Fail("MenuFormIniSettings.SaveToConfiguration Error: " + lastError);
				return false;
			}
			catch(ArgumentException exception) 
			{
				// Può essere fallita la File.SetAttributes
				lastError = exception.Message;
				Debug.Fail("MenuFormIniSettings.SaveToConfiguration Error: " + lastError);
				return false;
			}
		}
	}

	//=========================================================================
	[Serializable]
	[XmlTypeAttribute(AnonymousType = true)]
	[XmlRootAttribute(Namespace = "", IsNullable = false)]
	public class Settings
	{
		//---------------------------------------------------------------------
		private Settings ()
		{
			Item = new SettingsKeys();
		}

		//---------------------------------------------------------------------
		[XmlElement("Keys", Form = XmlSchemaForm.Unqualified)]
		public SettingsKeys Item
		{
			get;
			set;
		}

		//---------------------------------------------------------------------
		[XmlIgnore]
		public bool IsEmpty
		{
			get
			{
				return Item == null || Item.Keys == null || Item.Keys.Length == 0;
			}
		}

		//---------------------------------------------------------------------
		public static Settings Load()
		{
			try
			{
				string fromPath = Path.Combine(Path.GetDirectoryName(typeof(Settings).Assembly.Location), "user.settings");
				using (StreamReader input = new StreamReader(fromPath))
				{
					using (XmlReader xmlReader = XmlReader.Create(input))
					{
						XmlSerializer serializer = new XmlSerializer(typeof(Settings));
						return serializer.Deserialize(xmlReader) as Settings;
					}
				}
			}
			catch
			{
				return new Settings();
			}
		}

		//---------------------------------------------------------------------
		public void Save()
		{
			try
			{
				string toPath = Path.Combine(Path.GetDirectoryName(typeof(Settings).Assembly.Location), "user.settings");
				using (StreamWriter output = new StreamWriter(toPath))
				{
					XmlSerializer serializer = new XmlSerializer(typeof(Settings));
					serializer.Serialize(output, this);
				}
			}
			catch
			{}
		}
	}

	//=========================================================================
	[Serializable]
	[XmlType(AnonymousType = true)]
	public class SettingsKeys
	{
		List<SettingsKeysKey> keys = new List<SettingsKeysKey>();

		//---------------------------------------------------------------------
		public string this[string key]
		{
			get
			{
				foreach (var settingsKeysKey in Keys)
				{
					if (String.Compare(key, settingsKeysKey.Name, StringComparison.InvariantCultureIgnoreCase) == 0)
					{
						return settingsKeysKey.Value;
					}
				}
				return null;
			}
			set
			{
				bool found = false;
				foreach (var settingsKeysKey in Keys)
				{
					if (String.Compare(key, settingsKeysKey.Name, StringComparison.InvariantCultureIgnoreCase) == 0)
					{
						settingsKeysKey.Value = value;
						found = true;
						break;
					}
				}

				if (!found)
				{
					keys.Add(new SettingsKeysKey() { Name = key, Value = value });
				}
			}
		}

		//---------------------------------------------------------------------
		[XmlElement("key", Form = XmlSchemaForm.Unqualified)]
		public SettingsKeysKey[] Keys
		{
			get { return keys.ToArray(); }
			set { keys = new List<SettingsKeysKey>(value); }
		}
	}

	//=========================================================================
	[Serializable]
	[XmlType(AnonymousType = true)]
	public class SettingsKeysKey
	{
		//---------------------------------------------------------------------
		public SettingsKeysKey()
		{
			Name = "";
			Value = "";
		}
		//---------------------------------------------------------------------
		[XmlAttribute(AttributeName = "name")]
		public string Name
		{
			get;
			set;
		}

		//---------------------------------------------------------------------
		[XmlAttribute(AttributeName = "value")]
		public string Value
		{
			get;
			set;
		}

        //--------------------------------------------------------------------------------------------------------------------------------
        /// <summary>
        /// Adds (if not already done) or updates the appSettings element to the
        /// appSettings collection.
        /// </summary>
        /// <param name="cfgXmlDoc">The settings document wrapper</param>
        /// <param name="appSettingsNode">The node the new setting has to be added to or the updated setting belongs to.</param>
        /// <param name="sParamKey">Setting Key.</param>
        /// <param name="sValue">Setting value.</param>
        private void AddOrUpdateParameter(ConfigXmlDocument cfgXmlDoc, XmlNode appSettingsNode, string sParamKey, string sValue)
        {
            // input domain checks
            if (cfgXmlDoc == null || appSettingsNode == null || sParamKey == null) 
            {
                return;
            }

            XmlNode oNode = appSettingsNode.SelectSingleNode("//add[@key='" + sParamKey + "']");
            XmlElement newElem = null;
            if (oNode != null)
            {
                // xml element is already in the "added" settings,
                // just update it.
                newElem = (XmlElement)oNode;
                newElem.SetAttribute("value", sValue);
            }
            else
            {
                // xml element is not already among the "added" settings, 
                // create and append it.
                newElem = cfgXmlDoc.CreateElement("add");
                newElem.SetAttribute("key", sParamKey);
                newElem.SetAttribute("value", sValue);
                appSettingsNode.AppendChild(newElem);
            }
        }
	}
}
