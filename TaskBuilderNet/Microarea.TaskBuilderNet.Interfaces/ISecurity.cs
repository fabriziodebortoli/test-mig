using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Microarea.TaskBuilderNet.Interfaces
{
    public enum SecurityType
    {
        Undefined = 0,
        Function = 3,
        Report = 4,
        DataEntry = 5,
        ChildForm = 6,
        Batch = 7,
        Tab = 8,
        //Constraint				= 9,
        Table = 10,
        HotLink = 11,
        HotKeyLink = 11,
        View = 13,
        RowView = 14,
        Grid = 15,
        GridColumn = 16,
        Control = 17,
        Finder = 21,

        BkgAdmTemplate = 22,

        WordDocument = 23,
        ExcelDocument = 24,
        WordTemplate = 25,
        ExcelTemplate = 26,
        ExeShortcut = 27,
        Executable = 28,
        Text = 29,

        Tabber = 30,
        TileManager = 31,
        Tile = 32,
        Toolbar = 33,
        ToolbarButton = 34,
        EmbeddedView = 35,
        PropertyGrid = 36,
        TilePanel = 37,
        TilePanelTab = 38,
        All = 50
    }

    public enum GrantType
    {
        Execute = 1,
        Edit = 2,
        New = 4,
        Delete = 8,
        Browse = 16,
        CustomizeForm = 32,
        EditQuery = 64,
        Import = 128,
        Export = 256,
        SilentMode = 512
    }

    public interface ISecurity: IDisposable
    {
        bool IsSecurityLicensed { get; }
        bool IsAdmin { get; }
        bool IsCompanyProtected { get; }

        bool ExistExecuteGrant(string ns, SecurityType st);
        bool ExistExecuteGrant(string ns, int type);
        bool ExistGrant(int grants, GrantType type);
 
    }
}
