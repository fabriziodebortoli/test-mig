
var color_enable = "#FFFFFF";
var color_disable = "#C0C0C0";
var readOnlySuffix = "@@RO";
var propertyNotRendered = "Property not rendered in editor";

// Document state obj
var DescState_REMOVED   = 0;
var DescState_UNCHANGED = 1;
var DescState_UPDATED   = 2;
var DescState_ADDED = 3;
var DescState_PARSED = 4;

// Obj object type
var WndObjType_Undefined = 0;
var WndObjType_View = 1;
var WndObjType_Label = 2;
var WndObjType_Button = 3;
var WndObjType_PdfButton = 4;
var WndObjType_BeButton = 5;
var WndObjType_BeButtonRight = 6;
var WndObjType_SaveFileButton = 7;
var WndObjType_Image = 8;
var WndObjType_Group = 9;
var WndObjType_Radio = 10;
var WndObjType_Check = 11;
var WndObjType_Combo = 12;
var WndObjType_Edit = 13;
var WndObjType_Toolbar = 14;
var WndObjType_ToolbarButton = 15;
var WndObjType_Tabber = 16;
var WndObjType_Tab = 17;
var WndObjType_BodyEdit = 18;
var WndObjType_Radar = 19;
var WndObjType_HotLink = 20;
var WndObjType_Table = 21;
var WndObjType_ColTitle = 22;
var WndObjType_Cell = 23;
var WndObjType_List = 24;
var WndObjType_CheckList = 25;
var WndObjType_Tree = 26;
var WndObjType_TreeNode = 27;
var WndObjType_Menu = 28;
var WndObjType_MenuItem = 29;
var WndObjType_ListCtrl = 30;
var WndObjType_ListCtrlItem = 31;
var WndObjType_ListCtrlDetails = 32;
var WndObjType_Spin = 33;
var WndObjType_Report = 34;
var WndObjType_StatusBar = 35;
var WndObjType_SbPane = 36;
var WndObjType_Title = 37;
var WndObjType_MainMenu = 38;
var WndObjType_AuxRadarToolbar = 39;
var WndObjType_Frame = 40;
var WndObjType_RadarFrame = 41;
var WndObjType_PrintDialog = 42;
var WndObjType_Dialog = 43;
var WndObjType_PropertyDialog = 44;
var WndObjType_GenericWndObj = 45;
var WndObjType_RadarHeader = 46;
var WndObjType_FileDialog = 47;
var WndObjType_BETreeCell = 48;
var WndObjType_BtnImageAndText = 49;
var WndObjType_MultiChart = 50;
var WndObjType_TreeAdv = 51;
var WndObjType_TreeNodeAdv = 52;
var WndObjType_MailAddressEdit = 53;
var WndObjType_WebLinkEdit = 54;
var WndObjType_AddressEdit = 55;
var WndObjType_FieldReport = 56;
var WndObjType_TableReport = 57;
var WndObjType_EasyBuilderToolbar = 58;
var WndObjType_FloatingToolbar = 59;
var WndObjType_MSCombo = 60;
var WndObjType_UploadFileButton = 61;
var WndObjType_Thread = 62;
var WndObjType_ProgressBar = 63;
var WndObjType_CaptionBar = 64;
var WndObjType_RadioGroup = 65;
var WndObjType_Panel = 66;
var WndObjType_TabbedToolbar = 67;
var WndObjType_NamespaceEdit = 68;
//tile description type
var WndObjType_Tile = 71;
//tile group type
var WndObjType_TileGroup = 72;
//smaller element contained in a tile 
var WndObjType_TilePart = 73;
//static section of tile part (usually contains labels)
var WndObjType_TilePartStatic = 74;
//content section of tile part (usually contains input control)
var WndObjType_TilePartContent = 75;
var WndObjType_TileManager = 76;
var WndObjType_LayoutContainer = 78;
var WndObjType_HeaderStrip = 79;

//var WndObjType_PropertyGrid = 80;         //Not yet managed
//var WndObjType_PropertyGridItem = 81;     //Not yet managed


function getTypeDescription(type) {
    switch (type) {
        case WndObjType_Undefined:          return "Undefined";
        case WndObjType_View:               return "View";
        case WndObjType_Label:              return "Label";
        case WndObjType_Button:             return "Button";
        case WndObjType_PdfButton:          return "PdfButton";
        case WndObjType_BeButton:           return "BeButton";
        case WndObjType_BeButtonRight:      return "BeButtonRight";
        case WndObjType_SaveFileButton:     return "SaveFileButton";
        case WndObjType_Image:              return "Image";
        case WndObjType_Group:              return "Group";
        case WndObjType_Radio:              return "Radio";
        case WndObjType_Check:              return "Check";
        case WndObjType_Combo:              return "Combo";
        case WndObjType_Edit:               return "Edit";
        case WndObjType_Toolbar:            return "Toolbar";
        case WndObjType_ToolbarButton:      return "ToolbarButton";
        case WndObjType_Tabber:             return "Tabber";
        case WndObjType_Tab:                return "Tab";
        case WndObjType_BodyEdit:           return "BodyEdit";
        case WndObjType_Radar:              return "Radar";
        case WndObjType_HotLink:            return "HotLink";
        case WndObjType_Table:              return "Table";
        case WndObjType_ColTitle:           return "ColTitle";
        case WndObjType_Cell:               return "Cell";
        case WndObjType_List:               return "List";
        case WndObjType_CheckList:          return "CheckList";
        case WndObjType_Tree:               return "Tree";
        case WndObjType_TreeNode:           return "TreeNode";
        case WndObjType_Menu:               return "Menu";
        case WndObjType_MenuItem:           return "MenuItem";
        case WndObjType_ListCtrl:           return "ListCtrl";
        case WndObjType_ListCtrlItem:       return "ListCtrlItem";
        case WndObjType_ListCtrlDetails:    return "ListCtrlDetails";
        case WndObjType_Spin:               return "Spin";
        case WndObjType_Report:             return "Report";
        case WndObjType_StatusBar:          return "StatusBar";
        case WndObjType_SbPane:             return "SbPane";
        case WndObjType_Title:              return "Title";
        case WndObjType_MainMenu:           return "MainMenu";
        case WndObjType_AuxRadarToolbar:    return "AuxRadarToolbar";
        case WndObjType_Frame:              return "Frame";
        case WndObjType_RadarFrame:         return "RadarFrame";
        case WndObjType_PrintDialog:        return "PrintDialog";
        case WndObjType_Dialog:             return "Dialog";
        case WndObjType_PropertyDialog:     return "PropertyDialog";
        case WndObjType_GenericWndObj:      return "GenericWndObj";
        case WndObjType_RadarHeader:        return "RadarHeader";
        case WndObjType_FileDialog:         return "FileDialog";
        case WndObjType_BETreeCell:         return "BETreeCell";
        case WndObjType_BtnImageAndText:    return "BtnImageAndText";
        case WndObjType_MultiChart:         return "MultiChart";
        case WndObjType_TreeAdv:            return "TreeAdv";
        case WndObjType_TreeNodeAdv:        return "TreeNodeAdv";
        case WndObjType_MailAddressEdit:    return "MailAddressEdit";
        case WndObjType_WebLinkEdit:        return "WebLinkEdit";
        case WndObjType_AddressEdit:        return "AddressEdit";
        case WndObjType_FieldReport:        return "FieldReport";
        case WndObjType_TableReport:        return "TableReport";
        case WndObjType_EasyBuilderToolbar: return "EasyBuilderToolbar";
        case WndObjType_FloatingToolbar:    return "FloatingToolbar";
        case WndObjType_MSCombo:            return "MSCombo";
        case WndObjType_UploadFileButton:   return "UploadFileButton";
        case WndObjType_Thread:             return "Thread";
        case WndObjType_Panel:              return "Panel";
        case WndObjType_TabbedToolbar:      return "TabbedToolbar";
        case WndObjType_NamespaceEdit:      return "NamespaceEdit";
        case WndObjType_Tile:               return "Tile";
        case WndObjType_TileGroup:          return "TileGroup";
        case WndObjType_TilePart:           return "TilePart";
        case WndObjType_TilePartStatic:     return "TilePartStatic";
        case WndObjType_TilePartContent:    return "TilePartContent";
        case WndObjType_TileManager:        return "TileManager";
        case WndObjType_LayoutContainer:    return "LayoutContainer";
        case WndObjType_HeaderStrip:        return "HeaderStrip";
        default: return "";
    }
}

var LayoutType = {
    STRIPE  : { value : 0, text: 'stripe'}, 
    COLUMN  : { value : 1, text: 'column' },
    FIT     : { value: 2, text: 'fit' },
    HBOX    : { value: 3, text: 'hbox' },
    VBOX    : { value: 4, text: 'vbox' },
};

function getLayoutTypeDescription(layoutType) {
    switch (layoutType) {
        case LayoutType.STRIPE.value: return LayoutType.STRIPE.text;
        case LayoutType.COLUMN.value: return LayoutType.COLUMN.text;
        case LayoutType.FIT.value: return LayoutType.FIT.text;
        case LayoutType.HBOX.value: return LayoutType.HBOX.text;
        case LayoutType.VBOX.value: return LayoutType.VBOX.text;

        default: return "";
    }
}

//horizontal alignment
var Alignment_Left = { value: 0, text: "Left" };
var Alignment_Center = { value: 1, text: "Center" };
var Alignment_Right = { value: 2, text: "Right" };

//vertical alignment
var Vertical_Alignment_Top = { value: 0, text: "Top" };
var Vertical_Alignment_Center = { value: 1, text: "Center" };
var Vertical_Alignment_Bottom = { value: 2, text: "Bottom" };

var EVENT_ITEM_CREATED = "ic";
var EVENT_ALL_CHANGED = "ac";
var EVENT_UNDO_REDO = "ur";


//combobox comboType
var comboType_Simple = { value: 0, text: "Simple" };
var comboType_Dropdown = { value: 1, text: "Dropdown" };
var comboType_DropdownList = { value: 2, text: "DropdownList" };

//combobox ownerDraw
var ownerDraw_No = { value: 0, text: "No" };
var ownerDraw_Fixed = { value: 1, text: "Fixed" };
var ownerDraw_Variable = { value: 2, text: "Variable" };

//listbox selection
var selection_Single = { value: 0, text: "Single" };
var selection_Multiple = { value: 1, text: "Multiple" };
var selection_Extended = { value: 2, text: "Extended" };
var selection_None = { value: 3, text: "None" };

