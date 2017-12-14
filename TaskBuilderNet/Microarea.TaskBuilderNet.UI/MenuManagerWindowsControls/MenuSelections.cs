using System;
using System.Collections;

namespace Microarea.TaskBuilderNet.UI.MenuManagerWindowsControls
{
	#region MenuParserSelection class

	public class MenuParserSelection
	{
		internal enum ParserType : short
		{
			Unknown			= 0x0001,
			Applications	= 0x0002,
			Favorites		= 0x0003,
			Environment		= 0x0004
		}
		private ParserType	parserType		= ParserType.Unknown;
		private string		applicationName	= String.Empty;
		private string		groupName		= String.Empty;
		private string		menuPath		= String.Empty;		
		private string		commandPath		= String.Empty;
		
		//--------------------------------------------------------------------
		public MenuParserSelection()
		{
		}

		//--------------------------------------------------------------------
		public MenuParserSelection(string aApplicationName, string aGroupName, string aMenuPath, string aCommandPath)
		{
			applicationName	= aApplicationName;	
			groupName		= aGroupName;
			menuPath		= aMenuPath;
			commandPath		= aCommandPath;
		}		

		//--------------------------------------------------------------------
		internal MenuParserSelection(ParserType aParserType)
		{
			parserType = aParserType;
		}

		//-------------------------------------------------------------------------------------------
		public string ApplicationName	{ get { return applicationName; } set { applicationName = value; } }
		//-------------------------------------------------------------------------------------------
		public string GroupName			{ get { return groupName; } set { groupName = value; } }
		//-------------------------------------------------------------------------------------------
		public string MenuPath			{ get { return menuPath; }set { menuPath = value; } }
		//-------------------------------------------------------------------------------------------
		public string CommandPath		{ get { return commandPath; }set { commandPath = value; } }

		//--------------------------------------------------------------------
		public bool IsReferredToApplications
		{
			get { return parserType == ParserType.Applications; }
			set 
			{ 
				if (value) 
					parserType = ParserType.Applications; 
			}
		}

		//--------------------------------------------------------------------
		public bool IsReferredToFavorites
		{
			get { return parserType == ParserType.Favorites; }
			set 
			{ 
				if (value) 
					parserType = ParserType.Favorites; 
			}
		}

		//--------------------------------------------------------------------
		public bool IsReferredToEnvironment
		{
			get { return parserType == ParserType.Environment; }
			set 
			{ 
				if (value) 
					parserType = ParserType.Environment; 
			}
		}
	}
	
	#endregion

	#region MenuSelections class

	/// <summary>
	/// Summary description for MenuSelections.
	/// </summary>
	public class MenuSelections
	{
		private ArrayList	selections = null;
		private bool		noSeparatedEnvironment = false;
		
		#region MenuSelections constructors

		//--------------------------------------------------------------------
		public MenuSelections(bool aNoSeparatedEnvironmentFlag)
		{
			noSeparatedEnvironment = aNoSeparatedEnvironmentFlag;

			selections = new ArrayList();

			selections.Add(new MenuParserSelection(MenuParserSelection.ParserType.Applications));
			selections.Add(new MenuParserSelection(MenuParserSelection.ParserType.Favorites));

			if (!noSeparatedEnvironment)
				selections.Add(new MenuParserSelection(MenuParserSelection.ParserType.Environment));
		}

		//--------------------------------------------------------------------
		public MenuSelections() : this(false)
		{
		}
		
		#endregion

		#region MenuSelections public properties

		//--------------------------------------------------------------------
		public MenuParserSelection ApplicationsSelection 
		{
			get
			{
				if (selections == null || selections.Count == 0)
					return null;

				foreach(MenuParserSelection selection in selections)
				{
					if (selection.IsReferredToApplications)
						return selection;
				}
				return null;
			}
		}

		//--------------------------------------------------------------------
		public MenuParserSelection FavoritesSelection 
		{
			get
			{
				if (selections == null || selections.Count == 0)
					return null;

				foreach(MenuParserSelection selection in selections)
				{
					if (selection.IsReferredToFavorites)
						return selection;
				}
				return null;
			}
		}
	
		//--------------------------------------------------------------------
		public MenuParserSelection EnvironmentSelection 
		{
			get
			{
				if (noSeparatedEnvironment || selections == null || selections.Count == 0)
					return null;

				foreach(MenuParserSelection selection in selections)
				{
					if (selection.IsReferredToEnvironment)
						return selection;
				}
				return null;
			}
		}

		#endregion

		#region MenuSelections private methods
		
		//--------------------------------------------------------------------
		private void SetSelection(MenuParserSelection aSelection, string aApplicationName, string aGroupName, string aMenuPath, string aCommandPath)
		{
			if (aSelection == null || !selections.Contains(aSelection))
				return;
					
			aSelection.ApplicationName	= aApplicationName;	
			aSelection.GroupName		= aGroupName;
			aSelection.MenuPath			= aMenuPath;
			aSelection.CommandPath		= aCommandPath;
		}

		#endregion

		#region MenuSelections public methods
		
		//--------------------------------------------------------------------
		public void SetApplicationsSelection(string aApplicationName, string aGroupName, string aMenuPath, string aCommandPath)
		{
			SetSelection(this.ApplicationsSelection, aApplicationName, aGroupName, aMenuPath, aCommandPath);
		}

		//--------------------------------------------------------------------
		public void SetFavoritesSelection(string aApplicationName, string aGroupName, string aMenuPath, string aCommandPath)
		{
			SetSelection(this.FavoritesSelection, aApplicationName, aGroupName, aMenuPath, aCommandPath);
		}

		//--------------------------------------------------------------------
		public void SetEnvironmentSelection(string aApplicationName, string aGroupName, string aMenuPath, string aCommandPath)
		{
			if (noSeparatedEnvironment)
				return;

			SetSelection(this.EnvironmentSelection, aApplicationName, aGroupName, aMenuPath, aCommandPath);
		}

		#endregion
	}

	#endregion
}
