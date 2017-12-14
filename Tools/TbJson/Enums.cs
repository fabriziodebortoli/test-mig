namespace Microarea.TbJson
{
    //************ATTENZIONE************
    //TENERE ALLINEATO QUESTO ENUMERATIVO CON QUELLO IN TaskBuilder\Framework\TbGeneric\WndObjDescription.h
    public enum WndObjType
    {
        Undefined = 0, View = 1, Label = 2, Button = 3, PdfButton = 4, BeButton = 5, BeButtonRight = 6,
        SaveFileButton = 7, Image = 8, Group = 9, Radio = 10, Check = 11, Combo = 12, Edit = 13, Toolbar = 14,
        ToolbarButton = 15, Tabber = 16, Tab = 17, BodyEdit = 18, Radar = 19, HotLink = 20, Table = 21,
        ColTitle = 22, Cell = 23, List = 24, CheckList = 25, Tree = 26, TreeNode = 27, Menu = 28, MenuItem = 29,
        ListCtrl = 30, ListCtrlItem = 31, ListCtrlDetails = 32, Spin = 33, /*Report = 34,*/ StatusBar = 35, SbPane = 36,
        /*Title = 37 non più usata, il titolo è nella proprietà text del'oggetto, il rettangolo del titolo non ci interessa più*/
        MainMenu = 38, AuxRadarToolbar = 39, Frame = 40, RadarFrame = 41, PrintDialog = 42, Dialog = 43,
        PropertyDialog = 44, GenericWndObj = 45, RadarHeader = 46, FileDialog = 47, BETreeCell = 48, BtnImageAndText = 49,
        MultiChart = 50, TreeAdv = 51, TreeNodeAdv = 52, MailAddressEdit = 53, WebLinkEdit = 54, AddressEdit = 55,
        /*FieldReport = 56, TableReport = 57,*/
        EasyBuilderToolbar = 58, FloatingToolbar = 59, MSCombo = 60, UploadFileButton = 61, Thread = 62,
        ProgressBar = 63, CaptionBar = 64, RadioGroup = 65, Panel = 66, TabbedToolbar = 67, NamespaceEdit = 68,
        Constants = 69,
        //tile description type
        Tile = 71,
        //tile group type
        TileGroup = 72,
        //smaller element contained in a tile 
        TilePart = 73,
        //static section of tile part (usually contains labels)
        TilePartStatic = 74,
        //content section of tile part (usually contains input control)
        TilePartContent = 75,
        TileManager = 76,
        TilePanel = 77,
        LayoutContainer = 78,
        HeaderStrip = 79,
        PropertyGrid = 80,
        PropertyGridItem = 81,
        TreeBodyEdit = 82,
        StatusTile = 83,
        HotFilter = 84,
        StatusTilePanel = 85,
        Splitter = 86,
        DockingPane = 87,

        //enumerativi di utilità solo per il tool
        FrameContent = 200,
        ViewContainer = 201,
        DockPaneContainer = 202

    }

    public enum CommandCategory
    {
        Undefined = 0,
        Edit = 1,
        Navigation = 2,
        Search = 3,
        Print = 4,
        Tools = 5,
        Advanced = 6
    }

    public enum ViewCategory
    {
        Activity = 0,
        Batch = 1,
        DataEntry = 2,
        Finder = 3,
        Parameter = 4,
        Wizard = 5
    }

    public enum TileDialogSize
    {
        Micro = 0,
        Mini = 1,
        Standard = 2,
        Large = 3,
        Wide = 4,
        AutoFill = 5
    }

    public enum TileDialogStyle
    {
        None = 0,
        Normal = 1,
        Filter = 2,
        Header = 3,
        Footer = 4,
        Wizard = 5,
        Parameters = 6,
        Batch = 7
    }

    public enum LayoutType
    {
        None = -1,
        Stripe = 0,
        Column = 1,
        Hbox = 3,
        Vbox = 4
    }
}