//using System;
//using Microarea.Common.MenuLoader;
//using Microarea.Common.NameSolver;

//namespace Microarea.Common.SecurityLayer.SecurityLightObjects
//{
//	/// <summary>
//	/// Summary description for SecurityLightMenuLoader.
//	/// </summary>
//	//=========================================================================
//	public class SecurityLightMenuLoader
//	{
//		private PathFinder		pathFinder = null; 
//		private MenuXmlParser	currentMenuParser = null;
//		private int				modulesTotalCount = 0;
	
//		public event MenuParserEventHandler ScanStandardMenuComponentsStarted;
//		public event MenuParserEventHandler ScanStandardMenuComponentsModuleIndexChanged;
//		public event MenuParserEventHandler ScanStandardMenuComponentsEnded;

//		public event MenuParserEventHandler ScanCustomMenuComponentsStarted;
//		public event MenuParserEventHandler ScanCustomMenuComponentsModuleIndexChanged;
//		public event MenuParserEventHandler ScanCustomMenuComponentsEnded;
		
//		public event MenuParserEventHandler LoadAllMenuFilesStarted;
//		public event MenuParserEventHandler LoadAllMenuFilesModuleIndexChanged;
//		public event MenuParserEventHandler LoadAllMenuFilesEnded;

//		//---------------------------------------------------------------------
//		public SecurityLightMenuLoader(PathFinder aPathFinder)
//		{
//			pathFinder = aPathFinder;

//			modulesTotalCount = 0;
//			foreach(ApplicationInfo appInfo in aPathFinder.ApplicationInfos)
//			{
//				if (
//					appInfo == null || 
//					appInfo.Modules == null || 
//					appInfo.Modules.Count == 0 || 
//					appInfo.ApplicationType == ApplicationType.TaskBuilderNet
//					)
//					continue; 
//				modulesTotalCount += appInfo.Modules.Count;
//			}
//		}

//		#region SecurityLightMenuLoader public properties

//		//---------------------------------------------------------------------
//		public string CompanyName
//		{
//			get { return (pathFinder != null) ? pathFinder.Company : String.Empty ; }
//		}
		
//		//---------------------------------------------------------------------
//		public string UserName
//		{
//			get { return (pathFinder != null) ? pathFinder.User : String.Empty ; }
//		}

//		//---------------------------------------------------------------------
//		public PathFinder PathFinder { get { return pathFinder; } }
//		//---------------------------------------------------------------------
//		public MenuXmlParser CurrentMenuParser { get { return currentMenuParser; } }

//		#endregion // SecurityLightMenuLoader public properties
	
//		#region SecurityLightMenuLoader public methods

//		//---------------------------------------------------------------------
//		public MenuXmlParser LoadAllMenuFiles()
//		{
//			if 
//				(
//				pathFinder == null || 
//				pathFinder.ApplicationInfos == null ||
//				pathFinder.ApplicationInfos.Count == 0
//				)
//				return null;

//			MenuInfo menuInfo = new MenuInfo(pathFinder);

//			menuInfo.ScanStandardMenuComponentsStarted				+= new MenuParserEventHandler(MenuInfo_ScanStandardMenuComponentsStarted);
//			menuInfo.ScanStandardMenuComponentsModuleIndexChanged	+= new MenuParserEventHandler(MenuInfo_ScanStandardMenuComponentsModuleIndexChanged);
//			menuInfo.ScanStandardMenuComponentsEnded				+= new MenuParserEventHandler(MenuInfo_ScanStandardMenuComponentsEnded);
//			//Eventi x la progress mentre carico i menù provenienti dalla Custom
//			menuInfo.ScanCustomMenuComponentsStarted				+= new MenuParserEventHandler(MenuInfo_ScanCustomMenuComponentsStarted);
//			menuInfo.ScanCustomMenuComponentsModuleIndexChanged		+= new MenuParserEventHandler(MenuInfo_ScanCustomMenuComponentsModuleIndexChanged);
//			menuInfo.ScanCustomMenuComponentsEnded					+= new MenuParserEventHandler(MenuInfo_ScanCustomMenuComponentsEnded);

//			menuInfo.LoadAllMenuFilesStarted						+= new MenuParserEventHandler(MenuInfo_LoadAllMenuFilesStarted);
//			menuInfo.LoadAllMenuFilesModuleIndexChanged				+= new MenuParserEventHandler(MenuInfo_LoadAllMenuFilesModuleIndexChanged);
//			menuInfo.LoadAllMenuFilesEnded							+= new MenuParserEventHandler(MenuInfo_LoadAllMenuFilesEnded);

//			CommandsTypeToLoad commandsToLoad = CommandsTypeToLoad.Form | CommandsTypeToLoad.Batch | CommandsTypeToLoad.Report | CommandsTypeToLoad.Function | CommandsTypeToLoad.OfficeItem;

//			menuInfo.ScanStandardMenuComponents(commandsToLoad);
//			menuInfo.ScanCustomMenuComponents();
		
//			currentMenuParser = menuInfo.LoadAllMenuFiles(commandsToLoad, true, true);

//			return currentMenuParser;
//		}

//		//---------------------------------------------------------------------
//		public void CleanAllAccessibilityNodeStates()
//		{
//			if (currentMenuParser == null || currentMenuParser.MenuXmlDoc == null)
//				return;

//			MenuXmlNodeCollection existingCommands = currentMenuParser.GetAllCommands();
//			if (existingCommands == null || existingCommands.Count == 0)
//				return;

//			foreach (MenuXmlNode aCommand in existingCommands)
//			{
//				aCommand.AccessAllowedState = false;
//				aCommand.AccessDeniedState = false;
//				aCommand.AccessPartiallyAllowedState = false;
//			}
//		}
		
//		#endregion // SecurityLightMenuLoader public methods
		
//		#region Event handlers called during menu loading

//		//----------------------------------------------------------------------------
//		public void MenuInfo_ScanStandardMenuComponentsStarted(object sender, MenuParserEventArgs e)
//		{ 
//			if (ScanStandardMenuComponentsStarted != null)
//				ScanStandardMenuComponentsStarted(this, e);
//		}

//		//----------------------------------------------------------------------------
//		public void MenuInfo_ScanStandardMenuComponentsModuleIndexChanged(object sender, MenuParserEventArgs e)
//		{ 
//			if (ScanStandardMenuComponentsModuleIndexChanged != null)
//				ScanStandardMenuComponentsModuleIndexChanged(this, e);
//		}

//		//----------------------------------------------------------------------------
//		public void MenuInfo_ScanStandardMenuComponentsEnded(object sender, MenuParserEventArgs e)
//		{ 
//			if (ScanStandardMenuComponentsEnded != null)
//				ScanStandardMenuComponentsEnded(this, e);
//		}
		
//		//----------------------------------------------------------------------------
//		public void MenuInfo_ScanCustomMenuComponentsStarted (object sender, MenuParserEventArgs e)
//		{ 
//			if (ScanCustomMenuComponentsStarted != null)
//				ScanCustomMenuComponentsStarted(this, e);
//		}

//		//----------------------------------------------------------------------------
//		public void MenuInfo_ScanCustomMenuComponentsModuleIndexChanged (object sender, MenuParserEventArgs e)
//		{ 
//			if (ScanCustomMenuComponentsModuleIndexChanged != null)
//				ScanCustomMenuComponentsModuleIndexChanged(this, e);
//		}

//		//----------------------------------------------------------------------------
//		public void MenuInfo_ScanCustomMenuComponentsEnded (object sender, MenuParserEventArgs e)
//		{ 
//			if (ScanCustomMenuComponentsEnded != null)
//				ScanCustomMenuComponentsEnded(this, e);
//		}

//		//----------------------------------------------------------------------------
//		public void MenuInfo_LoadAllMenuFilesStarted (object sender, MenuParserEventArgs e)
//		{ 
//			if (LoadAllMenuFilesStarted != null)
//				LoadAllMenuFilesStarted(this, e);
//		}

//		//----------------------------------------------------------------------------
//		public void MenuInfo_LoadAllMenuFilesModuleIndexChanged (object sender, MenuParserEventArgs e)
//		{ 
//			if (LoadAllMenuFilesModuleIndexChanged != null)
//				LoadAllMenuFilesModuleIndexChanged(this, e);
//		}
		
//		//----------------------------------------------------------------------------
//		public void MenuInfo_LoadAllMenuFilesEnded (object sender, MenuParserEventArgs e)
//		{ 
//			if (LoadAllMenuFilesEnded != null)
//				LoadAllMenuFilesEnded(this, e);
//		}

//		#endregion // Event handlers called during menu loading
//	}
//}
