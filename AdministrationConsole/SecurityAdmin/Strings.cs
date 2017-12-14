using System;
using System.Reflection;
using Microarea.TaskBuilderNet.Core.SecurityLayer;
using Microarea.TaskBuilderNet.Interfaces;

namespace Microarea.Console.Plugin.SecurityAdmin
{

    //=========================================================================
	public class SecurityConstString
	{
		public const string SecurityAdmin						= "SecurityAdmin";
		public static readonly string SecurityAdminPlugIn		= Assembly.GetExecutingAssembly().GetName().Name;
		public const string SecurityAdminPlugInNamespace		= "Microarea.Console.Plugin.SecurityAdmin";
		public const string SecurityLibraryNamespace			= "Microarea.Console.Core.SecurityLibrary";
		public const string SetGrants							= "SetGrants";
		public const string SetEasyGrants						= "SetEasyGrantsForm";
		public const string OSLXPSecurity						= "OSLXPSecurity";
		public const string SecurityAdministrator				= "Security Administrator";
		public const string ReferenceObjects					= "ReferenceObjects";
		public const string Reports								= "Report";
		public const string GroupGrants							= "GroupGrants";
		public const string WebMethods							= "WebMethods";

		public const string ObjectId							= "ObjectId";
		public const string Description							= "Description";
		public const string Type								= "Type";
		public const string NameSpace							= "NameSpace";
		public const string Protected							= "Protected";
		public const string XmlFolder							= "Xml";
		public const string LogFolder							= "Log";
		public const string LogFileName							= "NewObjects_";
        public const string AdministrationConsole               = "AdministrationConsole";
        public const string DefaultRolesXmlFile                 = "DefaultRoles.xml";
        public const string OFMApplicationName                  = "OFM";
        public const string OFMCoreModuleName                   = "Core";

    }				 

	//=========================================================================
    public class ControlsString
	{
		//---------------------------------------------------------------------
		public static string GetControlDescription (SecurityType type)
		{
			switch (type)
			{
				case SecurityType.Report:
					return Strings.Report;

				case SecurityType.Table:
                    return Strings.Table;

				case SecurityType.View:
                    return Strings.View;

				case SecurityType.HotKeyLink:
                    return Strings.HotLink;

				case SecurityType.Control:
                    return Strings.Control;

				case SecurityType.Grid:
                    return Strings.Grid;

				case SecurityType.GridColumn:
                    return Strings.GridColumn;

				case SecurityType.Tab:
                    return Strings.Tab;

                case SecurityType.Tabber:
                    return Strings.Tabber;

                case SecurityType.Tile:
                    return Strings.Tile;

                case SecurityType.TileManager:
                    return Strings.TileManager;

                case SecurityType.Toolbar:
                    return Strings.Toolbar;
                case SecurityType.ToolbarButton:
                    return Strings.ToolbarButton;

				case SecurityType.ChildForm:
                    return Strings.ChildWindow;

                case SecurityType.EmbeddedView:
                    return Strings.EmbeddedView;

				case SecurityType.Batch:
                    return Strings.Batch;

				case SecurityType.Finder:
                    return Strings.Radar;

				case SecurityType.Function:
                    return Strings.Function;

				case SecurityType.DataEntry:
                    return Strings.DataEntry;

				case SecurityType.RowView:
                    return Strings.RowView;

				case SecurityType.WordDocument:
                    return Strings.WordDocument;

				case SecurityType.ExcelDocument:
                    return Strings.ExcelDocument;

				case SecurityType.WordTemplate:
                    return Strings.WordTemplate;

				case SecurityType.ExcelTemplate:
                    return Strings.ExcelTemplate;

				case SecurityType.ExeShortcut:
                    return Strings.ExcelTemplate;

				case SecurityType.Executable:
                    return Strings.Executable;

                case SecurityType.Text:
                    return Strings.Text;

                case SecurityType.All:
                    return Strings.All;

                case SecurityType.TilePanel:
                    return Strings.TilePanel;

                case SecurityType.TilePanelTab:
                    return Strings.TilePanelTab;

				default:
					return String.Empty;
			}
		}

        //---------------------------------------------------------------------
        public static SecurityType GetSecurityTypeFromControlDescription(string type)
        {
            if (string.Compare(type,Strings.Report) ==0)
                return SecurityType.Report;

            if (string.Compare(type,Strings.Table) ==0)
                return SecurityType.Table;

            if (string.Compare(type, Strings.View) ==0)
                return SecurityType.View;

            if (string.Compare(type, Strings.HotLink) ==0)
                return SecurityType.HotKeyLink;

            if (string.Compare(type, Strings.Control) ==0)
                return SecurityType.Control;

            if (string.Compare(type, Strings.Grid) ==0)
                return SecurityType.Grid;

            if (string.Compare(type, Strings.GridColumn) ==0)
                return SecurityType.GridColumn;

            if (string.Compare(type, Strings.Tab) ==0)
                return SecurityType.Tab;
            if (string.Compare(type, Strings.Tabber) == 0)
                return SecurityType.Tabber;

            if (string.Compare(type, Strings.Tile) == 0)
                return SecurityType.Tile;
            if (string.Compare(type, Strings.TileManager) == 0)
                return SecurityType.TileManager;

            if (string.Compare(type, Strings.ChildWindow) == 0)
                return SecurityType.ChildForm;
            if (string.Compare(type, Strings.EmbeddedView) == 0)
                return SecurityType.EmbeddedView;

            if (string.Compare(type, Strings.Batch) == 0)
                return SecurityType.Batch;

            if (string.Compare(type, Strings.Radar) ==0)
                return SecurityType.Finder;

            //if (string.Compare(type, Strings.Constraint) ==0)
            //    return SecurityType.Constraint;

            if (string.Compare(type, Strings.Function) ==0)
                return SecurityType.Function;

            if (string.Compare(type, Strings.DataEntry) ==0)
                return SecurityType.DataEntry;

            if (string.Compare(type, Strings.RowView) ==0)
                return SecurityType.RowView;

            if (string.Compare(type, Strings.WordDocument) ==0)
                return SecurityType.WordDocument;

            if (string.Compare(type, Strings.ExcelDocument) ==0)
                return SecurityType.ExcelDocument;
            
            if (string.Compare(type, Strings.WordTemplate) ==0)
                return SecurityType.WordTemplate;

            if (string.Compare(type, Strings.ExcelTemplate) ==0)
                return SecurityType.ExcelTemplate;

            if (string.Compare(type, Strings.Executable) ==0)
                return SecurityType.Executable;

            if (string.Compare(type, Strings.Text) ==0)
                return SecurityType.Text;

            if (string.Compare(type, Strings.PropertyGrid) == 0) 
                return SecurityType.PropertyGrid;

            return SecurityType.All;
 
        }
	}
}
