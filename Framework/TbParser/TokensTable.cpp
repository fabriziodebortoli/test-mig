#include "stdafx.h"
#include <string.h>

#include "TokensTable.h"
#include "Lexan.h"

//includere come ultimo include all'inizio del cpp
#include "begincpp.dex"

#ifdef _DEBUG
#undef THIS_FILE
static const char THIS_FILE[] = __FILE__;
#endif

///////////////////////////////////////////////////////////////////////////////
//						Static Objects
///////////////////////////////////////////////////////////////////////////////
//

static const TToken g_WoormTokens [] = 
{
	{ T_BLANK                , _T(" ") },
	{ T_BW_NOT               , _T("~") },
	{ T_OP_NOT               , _T("!") },
	{ T_QUESTION_MARK        , _T("?") },
	{ T_NE                   , _T("!=") },
	{ T_PERC                 , _T("%") },
	{ T_BW_AND               , _T("&") },
	{ T_OP_AND               , _T("&&") },
	{ T_ROUNDOPEN            , _T("(") },
	{ T_ROUNDCLOSE           , _T(")") },
	{ T_STAR                 , _T("*") },
	{ T_PLUS                 , _T("+") },
	{ T_INC                  , _T("++") },
	{ T_INCASS               , _T("+=") },
	{ T_COMMA                , _T(",") },
	{ T_MINUS                , _T("-") },
	{ T_DEC                  , _T("--") },
	{ T_DECASS               , _T("-=") },
	{ T_SLASH                , _T("/") },
	{ T_COLON                , _T(":") },
	{ T_SEP                  , _T(";") },
	{ T_LT                   , _T("<") },
	{ T_LE                   , _T("<=") },
	{ T_DIFF                 , _T("<>") },
	{ T_ASSIGN               , _T("=") },
	{ T_EQ                   , _T("==") },
	{ T_GT                   , _T(">") },
	{ T_GE                   , _T(">=") },
	{ T_SQUAREOPEN           , _T("[") },
	{ T_SQUARECLOSE          , _T("]") },
	{ T_BW_XOR               , _T("^") },
	{ T_EXP                  , _T("^^") },
	{ T_BRACEOPEN            , _T("{") },
	{ T_BW_OR                , _T("|") },
	{ T_OP_OR                , _T("||") },
	{ T_BRACECLOSE           , _T("}") },
	{ T_NEWLINE              , _T("\n") },
	{ T_TAB                  , _T("\t") },
	{ T_COPYRIGHT            , _T("// Woorm code behind") },
	{ T_COMMENT_SEP			 , _T("//==============================================================================") },
	{ T_FABS                 , _T("Abs") },
	{ T_AFTER                , _T("After") },
	{ T_ALIAS                , _T("Alias") },
	{ T_ALIASES              , _T("Aliases") },
	{ T_ALIGN                , _T("Align") },
	{ T_ALL                  , _T("All") },
	{ T_ALWAYS               , _T("Always") },
	
	{ T_ANCHOR_PAGE_LEFT	 , _T("AnchorPageLeft") },			//printer attributes for single field
	{ T_ANCHOR_PAGE_RIGHT	 , _T("AnchorPageRight") },
	{ T_ANCHOR_COLUMN_ID	 , _T("AnchorColumn") },			    //end printer attributes for single field

	{ T_AND                  , _T("And") },
	{ T_FAPP_DATE			 , _T("AppDate") },
	{ T_FAPP_YEAR			 , _T("AppYear") },
	{ T_APPEND               , _T("Append") },
	{ T_ARCHIVE              , _T("Archive") },
	{ T_ARRAY				 , _T("Array") },

	{ T_FARRAY_ATTACH		 , _T("Array_Attach") },
	{ T_FARRAY_DETACH		 , _T("Array_Detach") },
	{ T_FARRAY_CLEAR		 , _T("Array_Clear") },
	{ T_FARRAY_COPY			 , _T("Array_Copy") },
	{ T_FARRAY_FIND			 , _T("Array_Find") },
	{ T_FARRAY_GETAT		 , _T("Array_GetAt") },
	{ T_FARRAY_SIZE			 , _T("Array_Size") },
	{ T_FARRAY_SETAT		 , _T("Array_SetAt") },
	{ T_FARRAY_SORT			 , _T("Array_Sort") },
	
	{ T_FARRAY_ADD			 , _T("Array_Add") },
	{ T_FARRAY_APPEND		 , _T("Array_Append") },
	{ T_FARRAY_INSERT		 , _T("Array_Insert") },
	{ T_FARRAY_REMOVE		 , _T("Array_Remove") },
	{ T_FARRAY_CONTAINS		 , _T("Array_Contains") },
	{ T_FARRAY_CREATE		 , _T("Array_Create") },
	{ T_FARRAY_SUM			 , _T("Array_Sum") },

	{ T_FASC                 , _T("Asc") },
	{ T_ASK                  , _T("Ask") },
	{ T_AUTOINCREMENTAL		 , _T("AutoIncremental") },
	//{ T_BARCODE				 , _T("BarCode") },
	{ T_BARCODE_STRIP		 , _T("BarCodeStrip") },
	{ T_BEFORE               , _T("Before") },
	{ T_BEGIN                , _T("Begin") },
	{ T_BETWEEN              , _T("Between") },
	{ T_BITMAP                , _T("Bitmap") },
	{ T_METAFILE             , _T("Metafile") },
	{ T_PROPORTIONAL		 , _T("Proportional") },
	{ T_BKGCOLOR             , _T("BkgColor") },
	{ T_TBLOB				 , _T("Blob") },
	{ T_BODY                 , _T("Body") },
	{ T_BOLD                 , _T("Bold") },
	{ T_TBOOL                , _T("Bool") },
	{ T_BORDERS              , _T("Borders") },
	{ T_BOTTOM               , _T("Bottom") },
	{ T_BREAK				 , _T("Break") },
	{ T_BREAKING             , _T("Breaking") },
	{ T_BY                   , _T("By") },
	{ T_CALL                 , _T("Call") },
	{ T_CALL_DLL			 , _T("CallDll") },
	{ T_CASE                 , _T("Case") },
	{ T_FCDOW                , _T("Cdow") },
	{ T_CCAT                 , _T("Ccat") },
	{ T_FCEIL                , _T("Ceiling") },
	{ T_CELL                 , _T("Cell") },
	{ T_CHECK                , _T("Check") },
	{ T_FCHR                 , _T("Chr") },
	{ T_CLOSE                , _T("Close") },
	{ T_CMAX                 , _T("Cmax") },
	{ T_CMD                  , _T("Cmd") },
	{ T_CMIN                 , _T("Cmin") },
	{ T_FCMONTH              , _T("Cmonth") },
	{ T_COLTOTAL             , _T("ColTotal") },
	{ T_COLUMN               , _T("Column") },
	{ T_COLUMN_ANCHOR_LEFT	 , _T("AnchorLeft") },        //printer columns tokens
	{ T_COLUMN_ANCHOR_RIGHT	 , _T("AnchorRight") },
	{ T_COLUMN_FIXED		 , _T("Fixed") },
	{ T_COLUMN_HIDE_WHEN_EMPTY,_T("HideWhenEmpty") },
	{ T_COLUMN_OPTIMIZE_WIDTH, _T("OptimizeWidth") },     
	{ T_COLUMN_SPLITTER		 , _T("Splitter") },		  //end printer columns tokens	
	{ T_COLUMN_PEN           , _T("ColumnPen") },
	{ T_COLUMNS              , _T("Columns") },
	{ T_COLUMN_TITLES        , _T("ColumnTitles") },
	{ T_CONST				 , _T("const") },
	{ T_CONTAINS             , _T("Contains") },
	{ T_CONTINUE			 , _T("Continue") },
	{ T_CONDITIONAL			 , _T("Conditional") },
	{ T_CREATE               , _T("Create") },
	{ T_CREATE_XSD           , _T("CreateSchema") },
	{ T_CSUM                 , _T("Csum") },
	{ T_FCTOD                , _T("Ctod") },
	{ T_DATASOURCE           , _T("DataSource") },
	{ T_DEFAULTSECURITYROLES , _T("DefaultSecurityRoles") },
	{ T_TDATE                , _T("Date") },
	{ T_TDATETIME            , _T("DateTime") },
	{ T_FDAY                 , _T("Day") },
	{ T_DDE                  , _T("DDE") },
	{ T_DEBUG				 , _T("Debug") },
	{ T_DEFINE				 , _T("Define") },
	{ T_DEFAULT              , _T("Default") },
	{ T_DELETE               , _T("Delete") },
	{ T_DESCENDING			 , _T("Desc") },
	{ T_DIALOG				 , _T("Dialog") },
	{ T_DISPLAY              , _T("Display") },
	{ T_DISPLAY_FREE_FIELDS  , _T("DisplayFreeFields") },
	{ T_DISPLAY_TABLE_ROW    , _T("DisplayTableRow") },
	{ T_DISTINCT             , _T("Distinct") },
	{ T_DO                   , _T("Do") },
	{ T_TDOUBLE              , _T("Double") },
	{ T_FDTOC                , _T("Dtoc") },
	{ T_FELAPSED_TIME        , _T("ElapsedTime") },		
	{ T_ELSE                 , _T("Else") },
	{ T_END                  , _T("End") },
	{ T_TENUM				 , _T("Enum") },
	{ T_EOF                  , _T("EOF") },
	{ T_ESCAPE               , _T("Escape") }, 
	{ T_EVAL                 , _T("Eval") },
	{ T_EVENTS               , _T("Events") },
	{ T_EXEC                 , _T("Exec") },
	{ T_EXPORT				 , _T("Export") },
	{ T_EXTENSION            , _T("Extension") },
	{ T_FACENAME             , _T("Facename") },
	{ T_FALSE                , _T("False") },
	{ T_FIELD                , _T("Field") },
	{ T_FILE                 , _T("File") },
	{ T_FFIND                , _T("Find") },
	{ T_FREVERSEFIND         , _T("ReverseFind") },
	{ T_FREMOVENEWLINE		 , _T("RemoveNewLine") },
	{ T_FREPLACE			 , _T("Replace") },
	{ T_FINT                 , _T("FInt") },
	{ T_FLOAT 				 , _T("Float") },
	{ T_FLONG                , _T("FLong") },
	{ T_FFLOOR               , _T("Floor") },
	{ T_FONTSTYLE            , _T("FontStyle") },
	{ T_FONTSTYLES           , _T("FontStyles") },
	{ T_FOR                  , _T("For") },
	{ T_FFORMAT				 , _T("Format") },
	{ T_FORMATSTYLE          , _T("FormatStyle") },
	{ T_FORMATSTYLES         , _T("FormatStyles") },
	{ T_FORMFEED             , _T("FormFeed") },
	{ T_FROM                 , _T("From") },
	{ T_FUNCPROTO            , _T("FuncPrototypes") },
	{ T_GOTO                 , _T("Goto") },
	{ T_GROUP                , _T("Group") },
	{ T_TTEXT				 , _T("LongString") },
	{ T_HAVING               , _T("Having") },
	{ T_HEIGHTS              , _T("Heights") },
	{ T_HELP				 , _T("Help") },
	{ T_HIDDEN               , _T("Hidden") },
	{ T_HIDE_LS0             , _T("HideLs0") },
	{ T_HIDE_MS0             , _T("HideMs0") },
	{ T_HOTLINK              , _T("HotLink") },
	{ T_IF                   , _T("If") },
	{ T_INCH                 , _T("Inch") },
	{ T_INDEX                , _T("Index") },
	{ T_INIT                 , _T("Init") },
	{ T_INPUT                , _T("Input") },
	{ T_INSERT               , _T("Insert") },
	{ T_TINTEGER             , _T("Integer") },
	{ T_INTERLINE            , _T("InterLine") },
	{ T_INTO                 , _T("Into") },
	{ T_INVALID              , _T("Invalid") },
	{ T_INVERT_ORIENTATION   , _T("InvertOrientation") },
	{ T_IS                   , _T("Is") },
	{ T_ITALIC               , _T("Italic") },
	{ T_KEY                  , _T("Key") },
	{ T_LABEL                , _T("Label") },
	{ T_LANDSCAPE            , _T("Landscape") },
	{ T_FLAST_MONTH_DAY		 , _T("LastMonthDay") },
	{ T_FLEFT                , _T("Left") },
	{ T_FLEN                 , _T("Len") },
	{ T_LIKE                 , _T("Like") },
	{ T_LOGIC                , _T("Logic") },
	{ T_TLONG                , _T("Long") },
	{ T_FLOWER               , _T("Lower") },
	{ T_LOWER_LIMIT			 , _T("LowerLimit") },
	{ T_FLTRIM               , _T("Ltrim") },
	{ T_FMAX                 , _T("Max") },
	{ T_MAXIMIZED            , _T("Maximized") },
	{ T_MENU                 , _T("Menu") },
	{ T_MENUITEM             , _T("MenuItem") },
	{ T_MESSAGE_BOX			 , _T("Message") },
	{ T_FMIN                 , _T("Min") },
	{ T_MINIMIZED            , _T("Minimized") },
	{ T_FMOD                 , _T("Mod") },
	{ T_TMONEY               , _T("Money") },
	{ T_FMONTH               , _T("Month") },
	{ T_FMONTH_DAYS			 , _T("MonthDays") },
	{ T_FMONTH_NAME			 , _T("MonthName") },
	{ T_HIDE_TABLE_TITLE     , _T("HideTitle") },
	{ T_HIDE_ALL_TABLE_TITLE , _T("HideAllTitles") },
	{ T_EASYVIEW			 , _T("EasyView") },
	{ T_NATIVE				 , _T("Native") },
	{ T_NEXTLINE             , _T("NextLine") },
	{ T_NO_BOB               , _T("NoBodyBottom") },
	{ T_NO_BOL               , _T("NoBodyLeft") },
	{ T_NO_BOR               , _T("NoBodyRight") },
	{ T_NO_BOT               , _T("NoBodyTop") },
	{ T_NO_BORDERS           , _T("NoBorders") },
	{ T_NO_CSE               , _T("NoColSep") },
	{ T_NO_CTL               , _T("NoColTitleLeft") },
	{ T_NO_CTR               , _T("NoColTitleRight") },
	{ T_NO_CTS               , _T("NoColTitleSep") },
	{ T_NO_CTT               , _T("NoColTitleTop") },
	{ T_NODUPKEY             , _T("NODUP") },
	{ T_NO_HRULER            , _T("NoHRuler") },
	{ T_NO_INTERFACE		 , _T("NoInterface") },
	{ T_NO_PRN_BKGN_BITMAP   , _T("NoPrinterBkgnBitmap") },
	{ T_NO_PRN_BORDERS       , _T("NoPrinterBorders") },
	{ T_NO_PRN_LABELS        , _T("NoPrinterLabels") },
	{ T_NO_PRN_TITLES        , _T("NoPrinterTitles") },
	{ T_NO_STATUSBAR         , _T("NoStatusBar") },
	{ T_NOT                  , _T("Not") },
	{ T_NO_TOOLBAR           , _T("NoIconbar") },
	{ T_NO_TTL               , _T("NoTitleLeft") },
	{ T_NO_TTR               , _T("NoTitleRight") },
	{ T_NO_TTT               , _T("NoTitleTop") },
	{ T_NOTOKEN              , _T("NoToken") },
	{ T_NO_TOB               , _T("NoTotalBottom") },
	{ T_NO_TOL               , _T("NoTotalLeft") },
	{ T_NO_TOR               , _T("NoTotalRight") },
	{ T_NO_CON_BKGN_BITMAP   , _T("NoVideoBkgnBitmap") },
	{ T_NO_CON_BORDERS       , _T("NoVideoBorders") },
	{ T_NO_CON_LABELS        , _T("NoVideoLabels") },
	{ T_NO_CON_TITLES        , _T("NoVideoTitles") },
	{ T_NO_VRULER            , _T("NoVRuler") },
	{ T_NULL                 , _T("NULL") },
	{ T_NUMERIC				 , _T("Numeric") },
	{ T_TRANSPARENT			 , _T("Transparent") },
	{ T_SPECIAL_FIELD		 , _T("Special") },
	{ T_MARGINS				 , _T("Margins") },
	{ T_FISCAL_END			 , _T("FiscalEnd") },
	{ T_USE_DRAFT_FONT		 , _T("DraftFont") },
	{ T_OBJECTS              , _T("Objects") },
	{ T_OF                   , _T("of") },
	{ T_ON                   , _T("On") },
	{ T_ONLY_GRAPH           , _T("OnlyGraphInfo") },
	{ T_OPEN                 , _T("Open") },
	{ T_OPTIONS              , _T("Options") },
	{ T_OR                   , _T("Or") },
	{ T_ORDER                , _T("Order") },
	{ T_ORIGIN               , _T("Origin") },
	//{ T_OWNER_ID             , _T("OwnerID") },
	{ T_PADDED               , _T("Padded") },
	{ T_PAGE_INFO            , _T("PageInfo") },
	{ T_PAGE_LAYOUT          , _T("PageLayout") },
	{ T_PAGE_PRINTER_INFO	 , _T("PrinterPageInfo") },
	{ T_PAGE_HSPLITTER		 , _T("HPageSplitter") },
	{ T_PAGE_VSPLITTER		 , _T("VPageSplitter") },
	{ T_PATH				 , _T("Path") },
	{ T_PEN                  , _T("Pen") },
	{ T_TPERCENT			 , _T("Percent") },
	{ T_POPUP                , _T("Popup") },
	{ T_POSTFIX              , _T("Postfix") },
	{ T_PRECISION            , _T("Precision") },
	{ T_PREFIX               , _T("Prefix") },
	{ T_PROCEDURE            , _T("Procedure") },
	{ T_PROCEDURES           , _T("Procedures") },
	{ T_PROMPT               , _T("Prompt") },
	{ T_TQUANTITY			 , _T("Quantity") },
	{ T_QUERIES              , _T("Queries") },
	{ T_QUERY                , _T("Query") },
	{ T_QUIT                 , _T("Quit") },
	{ T_RADARS               , _T("Radars") },
	{ T_RATIO                , _T("Ratio") },
	{ T_READ_ONLY            , _T("ReadOnly") },
	{ T_RECT                 , _T("Rect") },
	{ T_RELEASE              , _T("Release") },
	{ T_REPORT               , _T("Report") },
	{ T_REPORTS              , _T("Reports") },
	{ T_RESET                , _T("Reset") },
	{ T_COLOR                , _T("Color") },
	{ T_FRIGHT               , _T("Right") },
	{ T_RNDRECT              , _T("RndRect") },
	{ T_FROUND               , _T("Round") },
	{ T_ROW                  , _T("Row") },
	{ T_ROWSEP               , _T("RowSep") },
	{ T_FRTRIM               , _T("Rtrim") },
	{ T_RULES                , _T("Rules") },
	{ T_SAVE                 , _T("Save") },
	{ T_SEGMENTED			 , _T("Segmented") },
	{ T_SELECT               , _T("Select") },
	{ T_SEPARATOR            , _T("Separator") },
	{ T_TSET				 , _T("Set") },
	{ T_FSIGN                , _T("Sign") },
	{ T_SIZE                 , _T("Size") },
	{ T_SIZEOF               , _T("Sizeof") },
	{ T_FSPACE               , _T("Space") },
	{ T_SPACELINE            , _T("SpaceLine") },
	{ T_SPAWN                , _T("Spawn") },
	{ T_SQL_EXEC             , _T("SqlExec") },
	{ T_SQRRECT              , _T("SqrRect") },
	{ T_STATUS               , _T("Status") },
	{ T_FSTR                 , _T("Str") },
	{ T_TSTR                 , _T("String") },
	{ T_STRIKEOUT            , _T("StrikeOut") },
	{ T_STRUCT               , _T("Struct") },
	{ T_STYLE                , _T("Style") },
	{ T_FSUBSTR              , _T("Substr") },
	{ T_FSUBSTRWW            , _T("SubstrWW") },
	{ T_SUBTOTAL             , _T("SubTotal") },
	{ T_SUBTOTALS            , _T("SubTotals") },
	{ T_SWITCH               , _T("Switch") },
	{ T_TABLE                , _T("Table") },
	{ T_TABLES	             , _T("Tables") },
	{ T_TEXT                 , _T("Text") },
	{ T_TEXTCOLOR            , _T("TextColor") },
	{ T_THOUSAND             , _T("Thousand") },
	{ T_FTIME                , _T("Time") },
	{ T_TITLE                , _T("Title") },
	{ T_THEN                 , _T("Then") },
	{ T_TOP                  , _T("Top") },
	{ T_TOTAL                , _T("Total") },
	{ T_TOTALS               , _T("Totals") },
	{ T_FTRIM                , _T("Trim") },
	{ T_TRUE                 , _T("True") },
	{ T_TYPE				 , _T("Type") },
	{ T_FTYPED_BARCODE		 , _T("TypedBarCode") },
	{ T_FGETBARCODE_ID		 , _T("GetBarCodeID") },
	{ T_TYPEDEF				 , _T("Typedef") },
	{ T_UNDEF				 , _T("Undef") },
	{ T_UNDERLINE            , _T("Underline") },
	{ T_UPDATE               , _T("Update") },
	{ T_FUPPER               , _T("Upper") },
	{ T_UPPER_LIMIT			 , _T("UpperLimit") },
	{ T_UUID				 , _T("uuid") },
	{ T_FVAL                 , _T("Val") },
	{ T_VAR                  , _T("Variables") },
	{ T_WEEKDAY              , _T("WeekDay") },
	{ T_WHEN                 , _T("When") },
	{ T_WHERE                , _T("Where") },
	{ T_WHILE                , _T("While") },
	{ T_WIDTH                , _T("Width") },
	{ T_FYEAR                , _T("Year") },
	{ T_BUILD_IDS			, _T("BUILD_IDS") },
	{ T_INCLUDE				, _T("include") },
	{ T_STRINGTABLE			, _T("STRINGTABLE") },
	{ T_HTMLFILE			, _T("HtmlFile") },
	{ T_CFG					, _T("Cfg") },
	{ T_CMDSHORTCUT			, _T("ShortCut") },
	{ T_PROPERTIES			, _T("Properties") },
    { T_SUBJECT				, _T("Subject") },
    { T_AUTHOR				, _T("Author") },
    { T_REPORTPRODUCER		, _T("ReportProducer") },
    { T_COMMENTS			, _T("Comments") },
    { T_FDAYOFWEEK			, _T("DayOfWeek") },
    { T_FWEEKOFMONTH		, _T("WeekOfMonth") },
    { T_FWEEKOFYEAR			, _T("WeekOfYear") },
    { T_FGIULIANDATE		, _T("GiulianDate") },
    { T_FDAYOFYEAR			, _T("DayOfYear") },
	{ T_ORDER_FIND_FIELD	, _T("OrderFindField") },
	{ T_FIND_SLAVE_FIELD	, _T("FindSlaveField") },
    { T_CONTROLS			, _T("Controls") },
	{ T_FLOADTEXT			, _T("LoadText") },
	{ T_FSAVETEXT			, _T("SaveText") },
	{ T_FLOCALIZE			, _T("Localize") },
	{ T_FISACTIVATED		, _T("IsActivated") },
	{ T_FISADMIN			, _T("IsAdmin") },
	{ T_OPTIONAL			, _T("Optional") },
	{ T_LINKS				, _T("Links") },
	{ T_LINKFORM			, _T("LinkForm") },
	{ T_LINKFUNCTION		, _T("LinkFunction") },
	{ T_LINKRADAR			, _T("LinkRadar") },
	{ T_LINKREPORT			, _T("LinkReport") },
	{ T_LINKURL				, _T("LinkUrl") },
	{ T_REINIT				, _T("ReInit") },
	{ T_STATIC				, _T("Static") },
	{ T_MAIL				, _T("Mail") },
	{ T_DIALOGS				, _T("Dialogs") },
	{ T_DEVELOPMENT			, _T("Development") },
	{ T_ABORT				, _T("Abort") },
	{ T_FINALIZE			, _T("Finalize") },
	{ T_NO_WEB				, _T("NoWeb") },
	{ T_DYNAMIC				, _T("Dynamic") },
	{ T_FRGB				, _T("Rgb") },

	{ T_FGETAPPTITLE_FROM_NS, _T("GetApplicationTitleFromNs") },
	{ T_FGETMODTITLE_FROM_NS, _T("GetModuleTitleFromNs") },
	{ T_FGETDOCTITLE_FROM_NS, _T("GetDocumentTitleFromNs") },
	{ T_FGETPATH_FROM_NS,	  _T("GetPathFromNs") },
	{ T_FGETNS_FROM_PATH,	  _T("GetNsFromPath") },

	{ T_IN					, _T("In") },
	{ T_OUT					, _T("Out") },
	{ T_REF					, _T("Ref") },
	{ T_COL					, _T("Col") },
	{ T_EXPAND				, _T("Expand") },

	{ T_FGETDATABASETYPE		, _T("GetDatabaseType") },
	{ T_FGETEDITION				, _T("GetEdition") },
	{ T_FGETPRODUCTLANGUAGE		, _T("GetProductLanguage") },
	{ T_FGETCOMPUTERNAME		, _T("GetComputerName") },
	{ T_FGETWINDOWUSER			, _T("GetWindowUser") },
	{ T_FGETINSTALLATIONNAME	, _T("GetInstallationName") },
	{ T_FGETINSTALLATIONPATH	, _T("GetInstallationPath") },
	{ T_FGETINSTALLATIONVERSION	, _T("GetInstallationVersion") },
	{ T_FGETCOMPANYNAME			, _T("GetCompanyName") },
	{ T_FGETLOGINNAME			, _T("GetLoginName") },

	{ T_FGETUSERDESCRIPTION		, _T("GetUserDescription") },
	{ T_FGETNEWGUID				, _T("GetNewGuid") },
	
	{ T_FMAKELOWERLIMIT			, _T("MakeLowerLimit") },
	{ T_FMAKEUPPERLIMIT			, _T("MakeUpperLimit") },
	{ T_FGETUPPERLIMIT			, _T("GetUpperLimit") },

	{ T_FCONTENTOF				, _T("ContentOf") },
	{ T_FVALUEOF				, _T("ValueOf")  },

	{ T_FTABLEEXISTS			, _T("TableExists") },		
	{ T_FFILEEXISTS				, _T("FileExists") },		
	{ T_FGETSETTING				, _T("GetSetting") },
	{ T_FSETSETTING				, _T("SetSetting") },

	{ T_FISDATABASEUNICODE		, _T("IsDatabaseUnicode") },

	{ T_FISREMOTEINTERFACE		, _T("IsRemoteInterface") },
	{ T_FISRUNNINGFROMEXTERNALCONTROLLER		, _T("IsRunningFromExternalController") },

	{ T_FGETCULTURE				, _T("GetCulture") },
	{ T_FSETCULTURE				, _T("SetCulture") },

	{ T_DROPSHADOW				, _T("DropShadow") },
	{ T_TEMPLATE				, _T("ReportTemplate") },
	{ T_TOOLTIP					, _T("Tooltip") },
	{ T_FULL					, _T("Full") },
	
	{ T_AS						, _T("As") },
	{ T_NO_XML					, _T("NoXml") },

	{ T_VMERGE_EMPTY_CELL		, _T("VMergeEmptyCell") },
	{ T_VMERGE_EQUAL_CELL		, _T("VMergeEqualCell") },
	{ T_VMERGE_TAIL_CELL		, _T("VMergeTailCell") },

	{ T_TITLE_BOTTOM			, _T("TitleBottom") },
	{ T_COLTITLE_BOTTOM			, _T("ColTitleBottom") },
	{ T_TOTAL_TOP				, _T("TotalTop") },

	//scripting post 3.2
	{ T_TVAR					, _T("Var") },
	{ T_RETURN					, _T("Return") },
	//3.8
	{ T_REPEATER				, _T("Repeater") },
	//3.8.1
	{ T_FDECODE					, _T("Decode") },

	{ T_FORCE					, _T("Force") },

	//4.0
	{ T_OUTER					, _T("Outer") },
	{ T_INNER					, _T("Inner") },
	{ T_JOIN					, _T("Join") },
	{ T_CROSS					, _T("Cross") },

	{ T_MULTI_SELECTIONS		, _T("MultiSelections") },	//askentry con hkl e combo multiselections

	{ T_FISEMPTY				, _T("IsEmpty") },
	{ T_FISNULL					, _T("IsNull") },
	{ T_FISWEB					, _T("IsWeb") },

	{ T_FDATEADD				, _T("DateAdd") },
	{ T_FWEEKSTARTDATE			, _T("WeekStartDate") },
	{ T_FISLEAPYEAR				, _T("IsLeapYear") },
	{ T_FEASTERSUNDAY			, _T("EasterSunday") },

	{ T_FWILDCARD_MATCH			, _T("WildcardMatch") },

	{ T_FSEND_BALLOON			, _T("SendBalloon") },
	{ T_FFORMAT_TBLINK			, _T("FormatTbLink") },

	{ T_FCHOOSE					, _T("Choose") },
	{ T_FIIF					, _T("IIF") },

	{ T_HTML					, _T("Html") },
	{ T_LAYER					, _T("Layer") },

	//M4 2.0
	{ T_TITLELINE				, _T("TitleLine") },
	{ T_SUBTITLELINE			, _T("SubTitleLine") },

	{ T_FREPLICATE				, _T("Replicate") },
	{ T_FPADLEFT				, _T("PadLeft") },
	{ T_FPADRIGHT				, _T("PadRight") },
	{ T_FCOMPARE_NO_CASE		, _T("CompareNoCase") },

	{ T_CHART					, _T("Chart") },
	{ T_CHART_CATEGORIES        , _T("ChartCategories") },
	{ T_CHART_SERIES			, _T("ChartSeries") },
	{ T_CHART_LEGEND			, _T("ChartLegend") },
	{ T_CHART_AXIS				, _T("ChartAxis") },

	{ T_GAUGE					, _T("Gauge") },
	{ T_GAUGE_POINTER			, _T("GaugePointer") },
	{ T_GAUGE_RANGE_COLOR		, _T("GaugeRangeColor") },
	{ T_GAUGE_SCALE				, _T("GaugeScale") },

	{ T_CCOUNT			, _T("CCount") },
	{ T_CAVG			, _T("CAvg") },
	{ T_CFIRST			, _T("CFirst") },
	{ T_CLAST			, _T("CLast") },

//UNDOCUMENTED
	{ T_RECORD					, _T("Record") },		//datatype  custom record
	{ T_SQLRECORD				, _T("SqlRecord") },	//datatype  es: SQLRECORD TCustSupp rec; rec.CustSupp = '0001'

	{ T_FCOLUMN_GETAT			, _T("Column_GetAt") },
	{ T_FCOLUMN_FIND			, _T("Column_Find") },
	{ T_FCOLUMN_SIZE			, _T("Column_Size") },
	{ T_FCOLUMN_SUM				, _T("Column_Sum") },

	{ T_FRECORD_GETFIELD		, _T("Record_GetField") },
	{ T_FSQLRECORD_GETFIELD		, _T("SqlRecord_GetField") },
	{ T_FOBJECT_GETFIELD		, _T("Object_GetField") },

	{ T_DISPLAY_CHART			, _T("DisplayChart") },

	//{ T_TOBJECT					, _T("Object") },
	{ T_FCONVERT				, _T("Convert") },
	{ T_FTYPEOF					, _T("TypeOf") },
	{ T_FADDRESSOF				, _T("AddressOf") },
	{ T_FEXECUTESCRIPT			, _T("ExecuteScript") },

	//dynamic hkl di TbMailer
	{ T_FGETTITLE				, _T("GetTitle") },

	{ T_FPREV_VALUE				, _T("PrevValue") },
	{ T_FNEXT_VALUE				, _T("NextValue") },

	//TB 4.0 attributo variabile  di Thread Context o sezione per variabili Document Context
	{ T_CONTEXT					, _T("Context") },
	{ T_FGETTHREADCONTEXT		, _T("GetThreadContext") },
	{ T_FOWNTHREADCONTEXT		, _T("OwnThreadContext") },
	
	//terminatore per loop sui token
	{ T_NULL_TOKEN			, NULL}
};

//-----------------------------------------------------------------------------
static const TokensTable	g_WoormTokensTable(g_WoormTokens, sizeof(g_WoormTokens)/sizeof(TToken));

//------------------------------------------------------------------------------
TB_EXPORT const TokensTable* AFXAPI AfxGetTokensTable ()
{
	return &g_WoormTokensTable;
}

///////////////////////////////////////////////////////////////////////////////
//						class TokenManager implementation
///////////////////////////////////////////////////////////////////////////////

//-----------------------------------------------------------------------------
TokensTable::TokensTable(const TToken* arTokens, int size)
	: 
	m_Tokens(arTokens),
	m_nSize (size)

{
	m_pOperators	= new CMapStringToPtr;
	m_pBrackets		= new CMapStringToPtr;
	m_pKeywords		= new CMapStringToPtr;

	LoadStandardTokens();
}


//-----------------------------------------------------------------------------
TokensTable::~TokensTable()
{
	ASSERT(m_pOperators);
	ASSERT(m_pBrackets);
	ASSERT(m_pKeywords);

	delete m_pOperators;
	delete m_pBrackets;
	delete m_pKeywords;
	
	m_pOperators	= NULL;
	m_pBrackets		= NULL;
	m_pKeywords		= NULL;
}


//------------------------------------------------------------------------------
void TokensTable::LoadStandardTokens ()
{
	CString strKey;
	Token t;
	for (int n = 0; n < m_nSize; n++)
	{
		t = m_Tokens[n].m_TokenId;
		if (t == T_NULL_TOKEN) return;

		strKey = m_Tokens[n].m_sToken;
		ASSERT(!strKey.IsEmpty());
		strKey.MakeLower();

		if (t >= T_OPERATORS_START && t <= T_OPERATORS_END)
		{
			m_pOperators->SetAt (strKey, MAKE_TOKEN(t));
		}
		else if (t >= T_BRACKETS_START && t <= T_BRACKETS_END)
		{
			m_pBrackets->SetAt (strKey, MAKE_TOKEN(t));
		}		
		else if (t >= T_KEYWORDS_START && t <= T_KEYWORDS_END)
		{
			m_pKeywords->SetAt (strKey, MAKE_TOKEN(t));
		}
	}
}

//-----------------------------------------------------------------------------
CString TokensTable::ToString (Token t) const 
{
	for (int n = 0; n < m_nSize; n++)
	{
		if (t == m_Tokens[n].m_TokenId)
			return m_Tokens[n].m_sToken;
	}
	return _T("");
}

//------------------------------------------------------------------------------
Token TokensTable::GetOperatorsToken (LPCTSTR pszLexeme) const
{
	return GetTokenFromMap (m_pOperators, pszLexeme);
}

//------------------------------------------------------------------------------
Token TokensTable::GetBracketsToken (LPCTSTR pszLexeme) const
{
	return GetTokenFromMap (m_pBrackets, pszLexeme);
}

//------------------------------------------------------------------------------
Token TokensTable::GetKeywordsToken (LPCTSTR pszLexeme) const
{
	return GetTokenFromMap (m_pKeywords, pszLexeme);
}

//------------------------------------------------------------------------------
Token TokensTable::GetTokenFromMap (CMapStringToPtr* pMap, LPCTSTR pszLexeme) const
{
 	void FAR*	pValue;
	CString		strKey(pszLexeme);
	
	// normalize key for case insensitive match
	strKey.MakeLower();
	
	if (pMap->Lookup(strKey, pValue))
		return GET_TOKEN(pValue);

	return T_NOTOKEN;
}
//-----------------------------------------------------------------------------
BOOL TokensTable::IsInLanguage (const CString& sToken) const
{
	return GetKeywordsToken(sToken) != T_NOTOKEN || GetOperatorsToken(sToken) != T_NOTOKEN;
}