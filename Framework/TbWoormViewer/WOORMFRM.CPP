
#include "stdafx.h"

#include <TbFrameworkImages\CommonImages.h>
#include <TbFrameworkImages\GeneralFunctions.h>

#include <TbGeneric\BCMenu.h>
#include <TbGenlib\generic.h>
#include <TbGenlib\baseapp.h>
#include <TbGenlib\commands.hrc>

#include <TbGenlibManaged\HelpManager.h>
#include <TbGenlibManaged\GlobalFunctions.h>
#include <TbGenlibManaged\MExport.h>

#include <TbGenlibUI\BrowserDlg.h>
#include <TbOledb\oledbmng.h>

#include <TBOleDb\SqlRecoveryManager.h>

#include <TbWoormEngine\REPORT.H>

#include "woormdoc.h"
#include "mulselob.h"
#include "woormfrm.h"
#include "table.h"
#include "RSEditorUI.h"
// resources

#include "commands.hrc"
#include "rectobj.hjson" //JSON AUTOMATIC UPDATE
#include "woormfrm.hjson" //JSON AUTOMATIC UPDATE
#include "woormdoc.hjson" //JSON AUTOMATIC UPDATE
#include <TbGenlib\generic.hjson> //JSON AUTOMATIC UPDATE
#include <TbGes\extdoc.hjson> //JSON AUTOMATIC UPDATE
#include <TbGenlib\BASEFRM.hjson> //JSON AUTOMATIC UPDATE

//includere come ultimo include all'inizio del cpp
#include "begincpp.dex"

#define RS_NS	L"Framework.TbWoormViewer.TbWoormViewer.RS.Button"


#ifdef _DEBUG
#undef THIS_FILE
static const char BASED_CODE THIS_FILE[] = __FILE__;
#endif

#define CHECK_HIDDEN_STATUS \
	if (m_pDoc && m_pDoc->m_pWoormInfo && m_pDoc->m_pWoormInfo->m_bHideFrame) return TRUE;

#define CHECK_HIDDEN_STATUS_VOID \
	if ((m_pDoc && m_pDoc->m_pWoormInfo && m_pDoc->m_pWoormInfo->m_bHideFrame) || !m_pWToolBar || !m_pWToolBar->m_hWnd ) return;

/////////////////////////////////////////////////////////////////////////////
// CMyStatusBar
BEGIN_MESSAGE_MAP(CMyStatusBar, CTaskBuilderStatusBar)

	//{{AFX_MSG_MAP(CMyStatusBar)
	ON_WM_PAINT()
	//}}AFX_MSG_MAP
END_MESSAGE_MAP()

//-----------------------------------------------------------------------------
void CMyStatusBar::OnPaint()
{
	__super::OnPaint();

	//CWoormFrame* pFrame = (CWoormFrame*) GetParent();
	//CWoormDocMng* pDocument = pFrame->GetDocument();
	//
	//if (pDocument)
	//	pDocument->UpdateStatusBar();
}

//-----------------------------------------------------------------------------
BOOL CMyStatusBar::PreCreateWindow(CREATESTRUCT& cs)
{
	cs.style = cs.style &~ SBARS_SIZEGRIP;
	return __super::PreCreateWindow(cs);
}

/////////////////////////////////////////////////////////////////////////////
// CToolCbxStr

//------------------------------------------------------------------------------
int CToolCbxStr::AddEntry(const CString& sItemData)
{
	int idxData = m_arStringItemData.Add(sItemData);

	return idxData;
}

//------------------------------------------------------------------------------
void CToolCbxStr::ResetContent()
{ 
	m_arStringItemData.RemoveAll();
	m_arStringItemData.Add(_T(""));
};


/////////////////////////////////////////////////////////////////////////////
// CWoormFrame

//------------------------------------------------------------------------------
IMPLEMENT_DYNCREATE(CWoormFrame, CAbstractWoormFrame)

//------------------------------------------------------------------------------
BEGIN_MESSAGE_MAP(CWoormFrame, CAbstractWoormFrame)

	ON_WM_SIZE()
	ON_WM_ACTIVATE()

	ON_COMMAND_EX(ID_VIEW_OBJECT_TOOLBAR,	OnBarCheck)
	ON_COMMAND_EX(ID_VIEW_SIZE_TOOLBAR,		OnBarCheck)
	ON_COMMAND_EX(ID_VIEW_ALIGN_TOOLBAR,	OnBarCheck)

	ON_WM_HELPINFO()

	ON_UPDATE_COMMAND_UI(ID_VIEW_OBJECT_TOOLBAR,	OnUpdateControlBarMenu)
	ON_UPDATE_COMMAND_UI(ID_VIEW_SIZE_TOOLBAR,		OnUpdateControlBarMenu)
	ON_UPDATE_COMMAND_UI(ID_VIEW_ALIGN_TOOLBAR,		OnUpdateControlBarMenu)
	
	ON_COMMAND					(ID_HELP_WOORM, 							OnWoormHelp)
	ON_COMMAND_RANGE			(ID_HELP_LIST, (UINT)(ID_HELP_LIST  +100),	OnUserHelpList)
	ON_COMMAND					(ID_HELP_INDEX,								OnWoormHelp)

	ON_COMMAND					(ID_GOTO_PRODUCERSITE, 				OnGoToProducerSite)
	ON_COMMAND					(ID_GOTO_SITE_PRIVATE_AREA, 		OnGoToSitePrivateArea)
	
	ON_UPDATE_COMMAND_UI(ID_WRM_EASY_READING,		OnUpdateWrmEasyReading)

	ON_COMMAND(ID_BTN_FILE_PRINT,					OnBtnFilePrint)

	ON_MESSAGE(UM_GET_DOC_NAMESPACE_ICON,			OnGetNamespaceAndIcon)
	ON_MESSAGE(UM_GET_DOCUMENT_TITLE_INFO,			OnGetDocumentTitleInfo)

	ON_COMMAND_RANGE(ID_EXTDOC_DDTB_ENUMREPORT0, (UINT)(ID_EXTDOC_DDTB_ENUMREPORT0 + ID_EXTDOC_DDTB_ENUMREPORT_MAX), OnCultureSelected)
	
	ON_COMMAND (ID_GOTO_SITE_PRIVATE_AREA, 			OnGoToSitePrivateArea)
		
	ON_COMMAND (ID_TOGGLE_ALIGN_TOOLBAR_FLOATING,	ToggleAlignToolbarFloating)
	ON_MESSAGE (UM_CLONE_DOCUMENT,					OnCloneDocument)
	ON_MESSAGE (UM_IS_ROOT_DOCUMENT,				OnIsRootDocument)
	ON_MESSAGE (UM_GET_SPLITTED_STRING,				OnGetSplittedString)

	ON_COMMAND (IDC_DROPDOWN_TEMPLATES,	OnDropDownTemplate)

	ON_WM_GETMINMAXINFO()

	ON_MESSAGE(UM_ACTIVATE_TAB, OnActivateTab)
END_MESSAGE_MAP()

//-----------------------------------------------------------------------------

static const UINT BASED_CODE indicators[] =
{   
	ID_INDICATOR_PAGE,
	ID_INDICATOR_RUN,
	ID_INDICATOR_TRACK_RECT,
	ID_INDICATOR_VOID,
	ID_INDICATOR_MESSAGE,
	ID_STATUS_BAR_SWITCH,
	ID_STATUS_BAR_HOME
};

//-----------------------------------------------------------------------------
CWoormFrame::CWoormFrame()
	: 
	m_ptAnchor (CPoint(0,0))
{
	// Use special font for speed enhancement in statusbar standard behaviour
	m_StatusBarFont.CreateFont
		(
		13, 0, 0, 0, FW_DONTCARE, FALSE, FALSE, FALSE, ANSI_CHARSET,
		OUT_DEFAULT_PRECIS, CLIP_DEFAULT_PRECIS, DEFAULT_QUALITY,
		DEFAULT_PITCH | FF_SWISS, NULL
		);

	m_bCanUseReportEditor = AfxGetBaseApp()->CanUseReportEditor();

	LoadInstalledLanguages(m_arUICultures, m_arUICultureDescriptions);

	m_bHasStatusBar = AfxGetThemeManager()->HasStatusBar();
}

//-----------------------------------------------------------------------------
void CWoormFrame::Dispose()
{
	//DEVE ESSERE RIPETIBILE!!!
	//vedi void CDockableFrame::PostNcDestroy()
	DestroyMenu(m_hMenu);
}

//-----------------------------------------------------------------------------
CWoormFrame::~CWoormFrame()
{
	ASSERT_VALID(this);

	SAFE_DELETE(m_pWTabbedToolBar);
}

//-----------------------------------------------------------------------------
BOOL CWoormFrame::LoadFrame(UINT nIDResource, DWORD dwDefaultStyle /*= WS_OVERLAPPEDWINDOW | FWS_ADDTOTITLE*/, CWnd* pParentWnd /*= NULL*/, CCreateContext* pContext /*= NULL*/)
{
	m_pDoc = pContext ? dynamic_cast<CWoormDocMng*>(pContext->m_pCurrentDoc) : NULL;

	if (!__super::LoadFrame(nIDResource, dwDefaultStyle, pParentWnd, pContext))
		return FALSE;

	//CreateMenu();

	//se sono nel contesto di WebLook, non carico gli acceleratori, consumano risorse inutilmente
	return AfxIsRemoteInterface() ? TRUE : LoadAccelTable(MAKEINTRESOURCE(IDR_LISTER));
}
//-----------------------------------------------------------------------------
void CWoormFrame::CreateMenu()
{
	CMenu menu;
	menu.CreateMenu();
	/*
	 POPUP "&File"
    BEGIN
        MENUITEM SEPARATOR
        MENUITEM "&Save\tCtrl+A",               ID_FILE_SAVE
        MENUITEM "Sav&e as...",                 ID_FILE_SAVE_AS
        MENUITEM "Save as &template...",        ID_FILE_SAVE_AS_TEMPLATE
        MENUITEM SEPARATOR
        MENUITEM "Print Previe&w",              ID_FILE_PRINT_PREVIEW
        MENUITEM "&Print...\tCtrl+P",           ID_PRINT_WITH_DIALOG
        MENUITEM "Print set&up...",             ID_FILE_PRINT_SETUP
        MENUITEM "Set pa&ge...",                ID_FILE_PAGE_SETUP
        MENUITEM SEPARATOR
        MENUITEM "&Close Report\tCtrl+F4",      ID_FILE_CLOSE
    END
	*/
	CMenu fileMenu;
	fileMenu.CreatePopupMenu();
	fileMenu.AppendMenu(MF_SEPARATOR);
	fileMenu.AppendMenu(MF_STRING, ID_FILE_SAVE, _TB("&Save\tCtrl+A"));
	fileMenu.AppendMenu(MF_STRING, ID_FILE_SAVE_AS, _TB("Sav&e as..."));
	fileMenu.AppendMenu(MF_STRING, ID_FILE_SAVE_AS_TEMPLATE, _TB("Save as &template..."));
	fileMenu.AppendMenu(MF_SEPARATOR);
	fileMenu.AppendMenu(MF_STRING, ID_FILE_PRINT_PREVIEW, _TB("Print Previe&w"));
	fileMenu.AppendMenu(MF_STRING, ID_PRINT_WITH_DIALOG, _TB("&Print...\tCtrl+P"));
	fileMenu.AppendMenu(MF_STRING, ID_FILE_PRINT_SETUP, _TB("Print set&up..."));
	fileMenu.AppendMenu(MF_STRING, ID_FILE_PAGE_SETUP, _TB("Set pa&ge..."));
	fileMenu.AppendMenu(MF_SEPARATOR);
	fileMenu.AppendMenu(MF_STRING, ID_FILE_CLOSE, _TB("&Close Report\tCtrl+F4"));
	menu.AppendMenu(MF_POPUP|MF_STRING, (UINT_PTR)fileMenu.Detach(), _TB("&File"));
	/*
	 POPUP "&Edit"
    BEGIN
        MENUITEM "&Undo\tCtrl+Z",               ID_OBJECT_UNDO
        MENUITEM SEPARATOR
        MENUITEM "&Delete\tCtrl+X",             ID_OBJECT_CUT
        MENUITEM "Co&py\tCtrl+C",               ID_OBJECT_COPY
        MENUITEM "P&aste\tCtrl+V",              ID_OBJECT_PASTE
        MENUITEM " Erase &all",                 ID_CLEAR_ALL
        MENUITEM SEPARATOR
        MENUITEM "&Select all\tF8",             ID_SELECT_ALL
        MENUITEM "Desele&ct all\tCtrl+F8",      ID_UNSELECT_ALL
        MENUITEM SEPARATOR
        MENUITEM "Align&ment\tCtrl+F7",         ID_MULSEL_ALIGN
        MENUITEM "Si&ze\tF7",                   ID_MULSEL_SIZE
        POPUP "C&olumn"
        BEGIN
            MENUITEM "Move &Left",                  ID_COL_LMOVE
            MENUITEM "Move &Right",                 ID_COL_RMOVE
            MENUITEM "&Delete",                     ID_COL_DELETE
        END
        MENUITEM SEPARATOR
		MENUITEM "F&ind",							ID_TOOLBAR_FIND_EDITBOX
    END
	*/
	CMenu editMenu;
	editMenu.CreatePopupMenu();
	editMenu.AppendMenu(MF_STRING, ID_OBJECT_UNDO, _TB("&Undo\tCtrl+Z"));
	editMenu.AppendMenu(MF_SEPARATOR);
	editMenu.AppendMenu(MF_STRING, ID_OBJECT_CUT, _TB("&Delete\tCtrl+X"));
	editMenu.AppendMenu(MF_STRING, ID_OBJECT_COPY, _TB("Co&py\tCtrl+C"));
	editMenu.AppendMenu(MF_STRING, ID_OBJECT_PASTE, _TB("P&aste\tCtrl+V"));
	editMenu.AppendMenu(MF_STRING, ID_CLEAR_ALL, _TB("Erase &all"));
	editMenu.AppendMenu(MF_SEPARATOR);
	editMenu.AppendMenu(MF_STRING, ID_SELECT_ALL, _TB("&Select all\tF8"));
	editMenu.AppendMenu(MF_STRING, ID_UNSELECT_ALL, _TB("Desele&ct all\tCtrl+F8"));
	editMenu.AppendMenu(MF_SEPARATOR);
	editMenu.AppendMenu(MF_STRING, ID_MULSEL_ALIGN, _TB("Align&ment\tCtrl+F7"));
	editMenu.AppendMenu(MF_STRING, ID_MULSEL_SIZE, _TB("Si&ze\tF7"));
	editMenu.AppendMenu(MF_SEPARATOR);
	//CMenu columnMenu;
	//columnMenu.CreatePopupMenu();
	//columnMenu.AppendMenu(MF_STRING, ID_COL_LMOVE, _TB("Move &Left"));
	//columnMenu.AppendMenu(MF_STRING, ID_COL_RMOVE, _TB("Move &Right"));
	////columnMenu.AppendMenu(MF_STRING, ID_COL_DELETE, _TB("&Delete"));
	//editMenu.AppendMenu(MF_POPUP | MF_STRING, (UINT_PTR)columnMenu.Detach(), _TB("C&olumn"));
	//editMenu.AppendMenu(MF_SEPARATOR);
	//editMenu.AppendMenu(MF_STRING, ID_TOOLBAR_FIND_EDITBOX, _TB("F&ind"));
	menu.AppendMenu(MF_POPUP | MF_STRING, (UINT_PTR)editMenu.Detach(), _TB("&Edit"));
	
	/*
	 POPUP "Obje&cts"
    BEGIN
        MENUITEM "Allow &edit",                 ID_ALLOW_EDITING
        MENUITEM SEPARATOR
        MENUITEM "&Filled rectangle",           ID_ADD_SQR_RECT
        MENUITEM "&Image",                      ID_ADD_GRAPH_RECT
        MENUITEM "Te&xt",                       ID_ADD_TEXT_RECT
        MENUITEM "Text f&ile",                  ID_ADD_FILE_RECT
        MENUITEM SEPARATOR
        MENUITEM "&Data field",                 ID_ADD_DATA_FIELD
        MENUITEM "&Expression field...",        ID_ADD_EXP_FIELD
        MENUITEM "&Hidden field...",            ID_ADD_HIDDEN_FIELD
        MENUITEM "&Function field...",          ID_ADD_VAR_FIELD
        MENUITEM SEPARATOR
        POPUP "&Table"
        BEGIN
            MENUITEM "from &Archive field",         ID_ADD_TABLE_DATA_FIELD
            MENUITEM "from &Expression field",      ID_ADD_TABLE_EXP_FIELD
            MENUITEM "from &Hidden field",          ID_ADD_TABLE_HIDDEN_FIELD
            MENUITEM "from &Function field",        ID_ADD_TABLE_VAR_FIELD
        END
        MENUITEM "Column Tot&al",               ID_COL_ADD_TOTAL
        MENUITEM SEPARATOR
        MENUITEM "Repeater",                    ID_ADD_REPEATER
        MENUITEM SEPARATOR
        MENUITEM "EasyAttachment barcode",      ID_ADD_EA_BARCODE
        MENUITEM SEPARATOR
        MENUITEM "&Graphical Attributes",       ID_RMOUSE_DOWN
        MENUITEM "&Clear all custom style",     ID_CLEAR_ALL_CUSTOM_STYLE
        MENUITEM SEPARATOR
        MENUITEM "New Layout",                  ID_LAYOUT_NEW
        MENUITEM "Rename Layout",               ID_LAYOUT_RENAME
        MENUITEM "Clone Object of an other Layout", ID_LAYOUT_CLONE_OBJECT
        MENUITEM "Move Object from an other Layout", ID_LAYOUT_MOVE_OBJECT
        MENUITEM SEPARATOR
    END
	*/

	CMenu objectsMenu;
	objectsMenu.CreatePopupMenu();
	//objectsMenu.AppendMenu(MF_STRING, ID_ALLOW_EDITING, _TB("Allow &edit"));
	//objectsMenu.AppendMenu(MF_SEPARATOR);
	//objectsMenu.AppendMenu(MF_STRING, ID_ADD_SQR_RECT, _TB("&Filled rectangle"));
	//objectsMenu.AppendMenu(MF_STRING, ID_ADD_GRAPH_RECT, _TB("&Image"));
	//objectsMenu.AppendMenu(MF_STRING, ID_ADD_TEXT_RECT, _TB("Te&xt"));
	//objectsMenu.AppendMenu(MF_STRING, ID_ADD_FILE_RECT, _TB("Text f&ile"));
	//objectsMenu.AppendMenu(MF_STRING, ID_ADD_REPEATER, _TB("Repeater"));
	//objectsMenu.AppendMenu(MF_SEPARATOR);
	//objectsMenu.AppendMenu(MF_STRING, ID_ADD_DATA_FIELD, _TB("&Data field"));
	//objectsMenu.AppendMenu(MF_STRING, ID_ADD_EXP_FIELD, _TB("&Expression field..."));
	//objectsMenu.AppendMenu(MF_STRING, ID_ADD_HIDDEN_FIELD, _TB("&Hidden field..."));
	//objectsMenu.AppendMenu(MF_STRING, ID_ADD_VAR_FIELD, _TB("&Function field..."));
	//CMenu tableMenu;
	//tableMenu.CreatePopupMenu();
	//tableMenu.AppendMenu(MF_STRING, ID_ADD_TABLE_DATA_FIELD, _TB("from &Archive field"));
	//tableMenu.AppendMenu(MF_STRING, ID_ADD_TABLE_EXP_FIELD, _TB("from &Expression field"));
	//tableMenu.AppendMenu(MF_STRING, ID_ADD_TABLE_HIDDEN_FIELD, _TB("from &Hidden field"));
	//tableMenu.AppendMenu(MF_STRING, ID_ADD_TABLE_VAR_FIELD, _TB("from &Function field"));
	//objectsMenu.AppendMenu(MF_POPUP | MF_STRING, (UINT_PTR)tableMenu.Detach(), _TB("&Table"));
	//objectsMenu.AppendMenu(MF_STRING, ID_COL_ADD_TOTAL, _TB("Column Tot&al"));
	//objectsMenu.AppendMenu(MF_SEPARATOR);
	//objectsMenu.AppendMenu(MF_SEPARATOR);
	objectsMenu.AppendMenu(MF_STRING, ID_ADD_EA_BARCODE, _TB("Add DMS Barcode"));
	objectsMenu.AppendMenu(MF_SEPARATOR);
	//objectsMenu.AppendMenu(MF_STRING, ID_RMOUSE_DOWN, _TB("&Graphical Attributes"));
	objectsMenu.AppendMenu(MF_STRING, ID_CLEAR_ALL_CUSTOM_STYLE, _TB("&Clear all custom styles"));
	objectsMenu.AppendMenu(MF_SEPARATOR);
	//objectsMenu.AppendMenu(MF_STRING, ID_LAYOUT_NEW, _TB("New Layout"));
	//objectsMenu.AppendMenu(MF_STRING, ID_LAYOUT_RENAME, _TB("Rename Layout"));
	objectsMenu.AppendMenu(MF_STRING, ID_LAYOUT_CLONE_OBJECT, _TB("Clone Object of an other Layout"));
	objectsMenu.AppendMenu(MF_STRING, ID_LAYOUT_MOVE_OBJECT, _TB("Move Object from an other Layout"));
	//objectsMenu.AppendMenu(MF_SEPARATOR);

	menu.AppendMenu(MF_POPUP | MF_STRING, (UINT_PTR)objectsMenu.Detach(), _TB("Obje&cts"));
	
	/*
	  POPUP "&Report"
    BEGIN
        MENUITEM "&Report Options...",          ID_OPTIONS
        MENUITEM "Generate XSD Schema (Standard)", ID_OPTION_CREATE_SCHEMA1
        MENUITEM "Generate XSD Schema (Advanced)", ID_OPTION_CREATE_SCHEMA2
        MENUITEM "&Woorm options...",           ID_WOORM_INI
        MENUITEM "Save only &graphical data",   ID_SAVE_ONLY_GRAPH_INFO
        MENUITEM SEPARATOR
        MENUITEM "Show Report's script",        ID_OPEN_AS_TEXT
        MENUITEM "P&roperties",                 ID_FILE_PROPERTIES
        MENUITEM "Show &Parameters",            ID_REPORT_PARAMETER
        MENUITEM SEPARATOR
        MENUITEM "&Font style...",              ID_FONT_STYLES
        MENUITEM "&Format style...",            ID_FORMAT_STYLES
        MENUITEM "&Enums Viewer",               ID_ENUMS_VIEWER
        MENUITEM SEPARATOR
        MENUITEM "Fi&elds...",                  ID_ENTRIES
        MENUITEM "&Events...",                  ID_EVENTS
        MENUITEM "&Procedures...",              ID_PROCEDURES
        MENUITEM "Grou&pings...",               ID_ORDERBY
        MENUITEM "&Request rules",              ID_ASK_RULES
        MENUITEM "&Subtotals",                  ID_SUB_TOTALS
        MENUITEM "&Queries...",                 ID_QUERIES
        MENUITEM SEPARATOR
        MENUITEM "Load st&yle template",        ID_LOAD_TEMPLATE
        MENUITEM "Unload style &template",      ID_UNLOAD_TEMPLATE
    END
	*/

	CMenu reportMenu;
	reportMenu.CreatePopupMenu();
	reportMenu.AppendMenu(MF_STRING, ID_REPORT_PARAMETER, _TB("Show &Parameters"));
	reportMenu.AppendMenu(MF_SEPARATOR);
	reportMenu.AppendMenu(MF_STRING, ID_FONT_STYLES, _TB("&Font style..."));
	reportMenu.AppendMenu(MF_STRING, ID_FORMAT_STYLES, _TB("&Format style..."));
	reportMenu.AppendMenu(MF_STRING, ID_ENUMS_VIEWER, _TB("&Enums Viewer"));
	reportMenu.AppendMenu(MF_SEPARATOR);

	reportMenu.AppendMenu(MF_STRING, ID_FILE_PROPERTIES, _TB("P&roperties"));
	reportMenu.AppendMenu(MF_STRING, ID_OPTIONS, _TB("&Report Options..."));
	reportMenu.AppendMenu(MF_STRING, ID_WOORM_INI, _TB("&Woorm options..."));
	reportMenu.AppendMenu(MF_SEPARATOR);
	reportMenu.AppendMenu(MF_STRING, ID_OPEN_AS_TEXT, _TB("Show Report's script"));
	reportMenu.AppendMenu(MF_SEPARATOR);
	reportMenu.AppendMenu(MF_STRING, ID_OPTION_CREATE_SCHEMA1, _TB("Generate XSD Schema (Standard)"));
	reportMenu.AppendMenu(MF_STRING, ID_OPTION_CREATE_SCHEMA2, _TB("Generate XSD Schema (Advanced)"));
	reportMenu.AppendMenu(MF_STRING, ID_SAVE_ONLY_GRAPH_INFO, _TB("Save only &graphical data"));
	reportMenu.AppendMenu(MF_SEPARATOR);

	//reportMenu.AppendMenu(MF_STRING, ID_ENTRIES, _TB("Fi&elds..."));
	//reportMenu.AppendMenu(MF_STRING, ID_EVENTS, _TB("&Events..."));
	//reportMenu.AppendMenu(MF_STRING, ID_PROCEDURES, _TB("&Procedures..."));
	//reportMenu.AppendMenu(MF_STRING, ID_ORDERBY, _TB("Grou&pings..."));
	//reportMenu.AppendMenu(MF_STRING, ID_ASK_RULES, _TB("&Request rules"));
	//reportMenu.AppendMenu(MF_STRING, ID_SUB_TOTALS, _TB("&Subtotals"));
	//reportMenu.AppendMenu(MF_STRING, ID_QUERIES, _TB("&Queries..."));
	//reportMenu.AppendMenu(MF_SEPARATOR);

	reportMenu.AppendMenu(MF_STRING, ID_LOAD_TEMPLATE, _TB("Load st&yle template"));
	reportMenu.AppendMenu(MF_STRING, ID_UNLOAD_TEMPLATE, _TB("Unload style &template"));

	menu.AppendMenu(MF_POPUP | MF_STRING, (UINT_PTR)reportMenu.Detach(), _TB("&Report"));
	/*
	POPUP "&Run"
		BEGIN
		MENUITEM "&Run\tF9", ID_RUN
		MENUITEM "&Stop\tEsc", ID_STOP
		MENUITEM "&Validity check\tCtrl+F9", ID_COMPILE
		MENUITEM "&Pause\tCtrl+S", ID_PAUSE
		MENUITEM "&Resume\tCtrl+Q", ID_RESUME
		MENUITEM SEPARATOR
		MENUITEM "&Go to page...", ID_GOTO_PAGE
		MENUITEM "Pa&ge up", ID_PG_UP
		MENUITEM "Pa&ge down", ID_PG_DN
		MENUITEM "Go to fir&st page", ID_PG_HOME
		MENUITEM "Go to &last page", ID_PG_END
		MENUITEM SEPARATOR
		MENUITEM "Export data in Excel.&Net", ID_EXPORT_EXCELNET
		MENUITEM "Export data on &HTML file...", ID_EXPORT_HTML
		MENUITEM "Export da&ta on text file...", ID_EXPORT_TEXT
		MENUITEM "Export da&ta in OpenOffice", ID_EXPORT_OPENOFFICE
		MENUITEM "Export da&ta in XML", ID_EXPORT_XML
		MENUITEM "Export da&ta in Word.Net", ID_EXPORT_WORD_NET
		MENUITEM "Export da&ta in Open Office ODT", ID_EXPORT_OPENOFFICE_ODT
		MENUITEM SEPARATOR
		MENUITEM "&Save on file with data (RDE format)...", ID_FILE_SAVE_RDE
		MENUITEM "&Save on file with data (PDF format - Portable Document Format)...", ID_FILE_SAVE_PDF
		MENUITEM "&Send Email with attached data report", ID_SEND_MAIL
		MENUITEM "&Send data report by PostaLite service", ID_SEND_POSTALITE
		END
	*/
	CMenu runMenu;
	runMenu.CreatePopupMenu();
	runMenu.AppendMenu(MF_STRING, ID_RUN, _TB("&Run\tF9"));
	runMenu.AppendMenu(MF_STRING, ID_STOP, _TB("&Stop\tEsc"));
	runMenu.AppendMenu(MF_STRING, ID_COMPILE, _TB("&Validity check\tCtrl+F9"));
	runMenu.AppendMenu(MF_STRING, ID_PAUSE, _TB("&Pause\tCtrl+S"));
	runMenu.AppendMenu(MF_STRING, ID_RESUME, _TB("&Resume\tCtrl+Q"));
	runMenu.AppendMenu(MF_SEPARATOR);
	runMenu.AppendMenu(MF_STRING, ID_GOTO_PAGE, _TB("&Go to page..."));
	runMenu.AppendMenu(MF_STRING, ID_PG_UP, _TB("Pa&ge up"));
	runMenu.AppendMenu(MF_STRING, ID_PG_DN, _TB("Pa&ge down"));
	runMenu.AppendMenu(MF_STRING, ID_PG_HOME, _TB("Go to fir&st page"));
	runMenu.AppendMenu(MF_STRING, ID_PG_END, _TB("Go to &last page"));
	/*runMenu.AppendMenu(MF_SEPARATOR);
	runMenu.AppendMenu(MF_STRING, ID_EXPORT_EXCELNET, _TB("Export data in Excel.&Net"));
	runMenu.AppendMenu(MF_STRING, ID_EXPORT_HTML, _TB("Export data on &HTML file..."));
	runMenu.AppendMenu(MF_STRING, ID_EXPORT_TEXT, _TB("Export da&ta on text file..."));
	runMenu.AppendMenu(MF_STRING, ID_EXPORT_OPENOFFICE, _TB("Export da&ta in OpenOffice"));
	runMenu.AppendMenu(MF_STRING, ID_EXPORT_XML, _TB("Export da&ta in XML"));
	runMenu.AppendMenu(MF_STRING, ID_EXPORT_XML_FULL, _TB("Export da&ta in XML FULL"));
	runMenu.AppendMenu(MF_STRING, ID_EXPORT_WORD_NET, _TB("Export da&ta in Word.Net"));
	runMenu.AppendMenu(MF_STRING, ID_EXPORT_OPENOFFICE_ODT, _TB("Export da&ta in Open Office ODT"));
	runMenu.AppendMenu(MF_SEPARATOR);
	runMenu.AppendMenu(MF_STRING, ID_FILE_SAVE_RDE, _TB("&Save on file with data (RDE format)..."));
	runMenu.AppendMenu(MF_STRING, ID_FILE_SAVE_PDF, _TB("&Save on file with data (PDF format - Portable Document Format)..."));
	runMenu.AppendMenu(MF_STRING, ID_SEND_MAIL, _TB("&Send Email with attached data report"));
	runMenu.AppendMenu(MF_STRING, ID_SEND_POSTALITE, _TB("&Send data report by PostaLite service."));*/
	menu.AppendMenu(MF_POPUP | MF_STRING, (UINT_PTR)runMenu.Detach(), _TB("&Run"));

	/*
	POPUP "W&indows"
    BEGIN
        MENUITEM "S&tatus Bar",                 ID_VIEW_STATUS_BAR
    END
	*/
	CMenu windowsMenu;
	windowsMenu.CreatePopupMenu();
	windowsMenu.AppendMenu(MF_STRING, ID_VIEW_STATUS_BAR, _TB("S&tatus Bar"));
	menu.AppendMenu(MF_POPUP | MF_STRING, (UINT_PTR)windowsMenu.Detach(), _TB("W&indows"));

	/*
	POPUP "&Actions"
	BEGIN
		MENUITEM "&Copy report link",			ID_ACTIONS_REP_COPY
		MENUITEM "Copy report &Xml link",		ID_ACTIONS_REP_XML_COPY
	END
	*/
	CMenu actionsMenu;
	actionsMenu.CreatePopupMenu();
	actionsMenu.AppendMenu(MF_STRING, ID_ACTIONS_REP_COPY, _TB("&Copy report link"));
	actionsMenu.AppendMenu(MF_STRING, ID_ACTIONS_REP_XML_COPY, _TB("Copy report &Xml link"));
	menu.AppendMenu(MF_POPUP | MF_STRING, (UINT_PTR)actionsMenu.Detach(), _TB("&Actions"));
	/*
	POPUP "&?"
    BEGIN
        MENUITEM "&Help on line (F1)",          ID_HELP_INDEX
        MENUITEM "&Go to producer site",        ID_GOTO_PRODUCERSITE
        MENUITEM "Go &to Private Area",         ID_GOTO_SITE_PRIVATE_AREA
    END
	*/
	CMenu qMenu;
	qMenu.CreatePopupMenu();
	qMenu.AppendMenu(MF_STRING, ID_HELP_INDEX, _TB("&Help on line (F1)"));
	qMenu.AppendMenu(MF_STRING, ID_GOTO_PRODUCERSITE, _TB("&Go to producer site"));
	qMenu.AppendMenu(MF_STRING, ID_GOTO_SITE_PRIVATE_AREA, _TB("Go &to Private Area"));
	menu.AppendMenu(MF_POPUP | MF_STRING, (UINT_PTR)qMenu.Detach(), _TB("&?"));

	VERIFY(SetMenu(&menu));
	m_hMenu = menu.Detach();
}
//-----------------------------------------------------------------------------
BOOL CWoormFrame::OnCreateClient(LPCREATESTRUCT lpcs, CCreateContext* pContext)
{
	//VERIFY(CreateStatusBar());

	if (!__super::OnCreateClient(lpcs, pContext))
		return FALSE;

	m_pDoc = pContext ? (CWoormDocMng *)(pContext->m_pCurrentDoc) : NULL;
	
	this->SetMenu(NULL);
	return TRUE;
}
//-----------------------------------------------------------------------------
BOOL CWoormFrame::OnCommand(WPARAM wParam, LPARAM lParam)
{
	return __super::OnCommand(wParam, lParam);
}

//-----------------------------------------------------------------------------
void CWoormFrame::OnUpdateWrmEasyReading (CCmdUI* pCmdUI)
{
	pCmdUI->Enable(TRUE);
}

//-----------------------------------------------------------------------------
void CWoormFrame::OnUserHelpList (UINT nID)
{
}

//-----------------------------------------------------------------------------
void RunReport(DataStr reportNamespace)
{ 
	CWoormInfo* pFDI = new CWoormInfo();
	pFDI->m_ReportNames.Add(reportNamespace.GetString());

	AfxGetTbCmdManager()->RunWoormReport(pFDI);
}

//-----------------------------------------------------------------------------
LRESULT CWoormFrame::OnIsRootDocument(WPARAM /*wParam*/, LPARAM /*lParam*/)
{
	CBaseDocument* pDocument = (CBaseDocument*) GetActiveDocument();
	//faccio eseguire la rundocument al thread di login
	if (pDocument && !pDocument->GetCallerDocument())
		return 1L;
	return 0L;
}

//-----------------------------------------------------------------------------
LRESULT CWoormFrame::OnCloneDocument(WPARAM, LPARAM)
{
	CBaseDocument* pDocument = (CBaseDocument*) GetActiveDocument();
	//faccio eseguire la rundocument al thread di login
	if (pDocument)
		AfxInvokeAsyncThreadGlobalProcedure<DataStr>(AfxGetLoginContext()->m_nThreadID, &RunReport, pDocument->GetNamespace().ToString());
	return 1L;
}

//-----------------------------------------------------------------------------
void CWoormFrame::OnWoormHelp()
{
	CWoormDocMng* pDoc = m_pDoc ? m_pDoc : (CWoormDocMng*) GetActiveDocument();
	if (!pDoc)
	{
		ASSERT(FALSE);
		return;
	}

	//TODO_MARCO vedere se va bene questo namespace per woorm
	CTBNamespace aNs = pDoc->GetNamespace();
	if (!aNs.IsValid())
		aNs.SetNamespace(szWoormNamespace);

	if (pDoc->m_bAllowEditing)
	{
		ShowHelp(RS_HELP_MAIN);
	}
	else
		ShowHelp(aNs.ToString());
}

//-----------------------------------------------------------------------------
BOOL CWoormFrame::OnHelpInfo(HELPINFO* pHelpInfo)
{
	CWoormDocMng* pDoc = m_pDoc ? m_pDoc : (CWoormDocMng*) GetActiveDocument();
	if (!pDoc)
	{
		ASSERT(FALSE);
		return FALSE;
	}
	
	CTBNamespace aNs = pDoc->GetNamespace();
	if (!aNs.IsValid())
		aNs.SetNamespace(szWoormNamespace);

	if (pDoc->m_bAllowEditing)
	{
		ShowHelp(RS_HELP_MAIN);
	}
	else
		ShowHelp(aNs.ToString());

	// TODO HELP CHIEDERE A RICCARDO
	// commentato per non far partire da un report di woorm 2 pagine di help (una magonet e una framework)
	// return CFrameWnd::OnHelpInfo(pHelpInfo);
	return TRUE;
}

//-----------------------------------------------------------------------------
void CWoormFrame::OnGoToProducerSite()
{
	ConnectToProducerSite();
}

//-----------------------------------------------------------------------------
void CWoormFrame::OnSize(UINT nType, int cx, int cy)
{
	if (IsLayoutSuspended())
		return;

	__super::OnSize(nType, cx, cy);

	if (m_pWTabbedToolBar)
		m_pWTabbedToolBar->SetWindowPos(NULL, 0, 0, cx, m_pWTabbedToolBar->CalcMaxButtonHeight(), SWP_NOMOVE | SWP_NOZORDER);

	if (m_pLayoutPane)
		m_pLayoutPane->ShowControlBar(TRUE, FALSE, TRUE);
}

//-----------------------------------------------------------------------------
void CWoormFrame::OnGetMinMaxInfo(MINMAXINFO* lpMMI)
{
	// TabbedToolBar Width Size Modal - overlap of ico
	if (m_pWTabbedToolBar)
		lpMMI->ptMinTrackSize.x = m_pWTabbedToolBar->CalcMinimumWidth();
}

//-----------------------------------------------------------------------------
void CWoormFrame::OnGoToSitePrivateArea()
{
	ConnectToProducerSiteLoginPage();
}

//-----------------------------------------------------------------------------
void CWoormFrame::ShowRunButton()   
{ 
	if (m_pDoc && m_pDoc->m_pWoormInfo && m_pDoc->m_pWoormInfo->m_bHideFrame)
		return;
	if (!m_pWToolBar || !m_pWToolBar->m_hWnd)
		return;
	m_pWToolBar->SetButtonInfo (ID_RUN_STOP, TBBS_BUTTON | TBSTYLE_DROPDOWN, AfxGetRSIcon(ID_RUN_STOP));
	m_pWToolBar->Invalidate();
	RecalcLayout();
}

void CWoormFrame::ShowStopButton()	
{ 
	if (m_pDoc && m_pDoc->m_pWoormInfo && m_pDoc->m_pWoormInfo->m_bHideFrame)
		return;
	if (!m_pWToolBar || !m_pWToolBar->m_hWnd)
		return;
	m_pWToolBar->SetButtonInfo(ID_RUN_STOP, TBBS_BUTTON, AfxGetRSIcon(ID_RUN_STOP_STOP));
	m_pWToolBar->Invalidate();
	RecalcLayout();
}

//-----------------------------------------------------------------------------
void CWoormFrame::ShowReRunButton()
{
	if (!m_pWEditToolBar) return;

	m_pWEditToolBar->SetButtonInfo(ID_WMR_RERUN_STARTSTOP, TBBS_BUTTON | TBSTYLE_DROPDOWN, AfxGetRSIcon(ID_WMR_RERUN_STARTSTOP_START));
	m_pWEditToolBar->Invalidate();
	RecalcLayout();
}

void CWoormFrame::ShowReStopButton()
{
	if (!m_pWEditToolBar) return;
	if (!m_pDoc)
		return;

	m_pWEditToolBar->SetButtonInfo(ID_WMR_RERUN_STARTSTOP, TBBS_BUTTON, AfxGetRSIcon(ID_WMR_RERUN_STARTSTOP_STOP));
	m_pWEditToolBar->Invalidate();
	RecalcLayout();
	LPCTSTR pStrTime = m_pWEditToolBar->GetComboItemSelText(ID_WMR_COMBO_TIME_RERUN);
	if (!pStrTime) 
		return;
	int iTime = 0;
	try
	{
		iTime = _ttoi(pStrTime);
	}
	catch (const std::exception&)
	{
		iTime = 0;
	}
	m_pDoc->SetReRunTimer(iTime);
}

void CWoormFrame::ShowPauseButton()	
{ 
	if (m_pDoc && m_pDoc->m_pWoormInfo && m_pDoc->m_pWoormInfo->m_bHideFrame)
		return;
	if (!m_pWToolBar || !m_pWToolBar->m_hWnd)
		return;
	m_pWToolBar->SetButtonInfo(ID_PAUSE_RESUME, TBBS_BUTTON, AfxGetRSIcon(ID_PAUSE_RESUME));
	RecalcLayout(); 
}

void CWoormFrame::ShowResumeButton()
{ 
	if (m_pDoc && m_pDoc->m_pWoormInfo && m_pDoc->m_pWoormInfo->m_bHideFrame)
		return;
	if (!m_pWToolBar || !m_pWToolBar->m_hWnd)
		return;
	m_pWToolBar->SetButtonInfo(ID_PAUSE_RESUME, TBBS_BUTTON, AfxGetRSIcon(ID_PAUSE_RESUME_RESUME));
	RecalcLayout();
}

//-----------------------------------------------------------------------------
void CWoormFrame::ToggleAlignToolbarFloating()
{ 
	if (m_pDoc && m_pDoc->m_pWoormInfo && m_pDoc->m_pWoormInfo->m_bHideFrame)
		return;
	if (!m_pWAlignMoveToolBar || !m_pWAlignMoveToolBar->m_hWnd)
		return;

	m_bAlignToolbarFloating = ! m_bAlignToolbarFloating;
	
	RecalcLayout();
}

//-----------------------------------------------------------------------------
#define SET_CHECKP(a)	if (a) pCmdUI->SetCheck((a->GetStyle() & WS_VISIBLE) != 0)
#define SET_CHECKR(a)	if (a) pCmdUI->SetCheck((a.GetStyle() & WS_VISIBLE) != 0)

#define SHOWP(a)		if (a) { ShowControlBar(a, (a->GetStyle() & WS_VISIBLE) == 0, FALSE, TRUE); RecalcLayout(); }
#define SHOWR(a)		if (a) { ShowControlBar(&a, (a.GetStyle() & WS_VISIBLE) == 0, FALSE, TRUE); RecalcLayout(); }
#define SHOWPFLOAT(a)	if (a) { ShowControlBar(a, (a->GetStyle() & WS_VISIBLE) == 0, FALSE, TRUE);  }

//-----------------------------------------------------------------------------
void CWoormFrame::OnUpdateControlBarMenu(CCmdUI* pCmdUI)
{
	if (pCmdUI->m_nID == ID_VIEW_WOORM_TOOLBAR)
	{
		SET_CHECKP(m_pWToolBar);
		SET_CHECKP(m_pWEditToolBar);
		return;
	}
	if (pCmdUI->m_nID == ID_VIEW_OBJECT_TOOLBAR)
	{
		SET_CHECKP(m_pWObjectToolBar);
		return;
	}
	if (pCmdUI->m_nID == ID_VIEW_SIZE_TOOLBAR)
	{
		SET_CHECKP(m_pWSizeToolBar);
		SET_CHECKP(m_pWBorderToolBar);
		return;
	}
	if (pCmdUI->m_nID == ID_VIEW_ALIGN_TOOLBAR)
	{
		SET_CHECKP(m_pWAlignMoveToolBar);
		return;
	}
	if (pCmdUI->m_nID == AFX_IDW_STATUS_BAR)
	{
		SET_CHECKR(m_RSStatusBar);
		return;
	}
}

//-----------------------------------------------------------------------------
void CWoormFrame::ShowAlignToolbar(BOOL bShow /*= TRUE*/)
{
	if (m_bAlignToolbarFloating && m_pWAlignMoveToolBar)
		ShowControlBar(m_pWAlignMoveToolBar, bShow, FALSE, TRUE);
}

//-----------------------------------------------------------------------------
BOOL CWoormFrame::OnBarCheck(UINT nID)
{
	//CWoormDocMng* pDocument = (CWoormDocMng*)GetActiveDocument();
	if (m_bAlignToolbarFloating)
		if (nID == ID_VIEW_WOORM_TOOLBAR)
		{
			SHOWP(m_pWToolBar);
			return TRUE;
		}
	if (nID == ID_VIEW_OBJECT_TOOLBAR)
	{
		SHOWP(m_pWObjectToolBar);
		return TRUE;
	}
	if (nID == ID_VIEW_SIZE_TOOLBAR)
	{
		SHOWP(m_pWSizeToolBar);
		SHOWP(m_pWBorderToolBar);
		return TRUE;
	}

	if (nID == ID_VIEW_ALIGN_TOOLBAR && m_pDoc)
	{
		if (m_bAlignToolbarFloating)
		{
			if (m_pDoc->m_pMultipleSelObj && m_pDoc->m_pMultipleSelObj->GetSize())
				SHOWPFLOAT(m_pWAlignMoveToolBar);
		}
		else
			SHOWP(m_pWAlignMoveToolBar);

		return TRUE;
	}
	if (nID == AFX_IDW_STATUS_BAR)
	{
		SHOWR(m_RSStatusBar);
		return TRUE;
	}
	return FALSE;
}

//-----------------------------------------------------------------------------
void CWoormFrame::SetAllowEditButtonInfo(BOOL bAllowEditing)
{
	if (!m_pWTabbedToolBar || !m_pWTabbedToolBar->m_hWnd) 
		return;
	if (m_pDoc && m_pDoc->m_pWoormInfo && m_pDoc->m_pWoormInfo->m_bHideFrame)
		return;

	ShowDockingPanels(bAllowEditing);

	// visualizza la toolbar ausiliarie per lo stato di edit
	BOOL b = m_pDoc && m_pDoc->m_pWoormIni &&  m_pDoc->m_pWoormIni->m_bShowAllToolbars;

	m_pWTabbedToolBar->SetSwitchEnableAlwaysDropDown(bAllowEditing && b);

	if (b)
	{
		if (!m_pWObjectToolBar)
			CreateObjectToolBar(m_pDoc);

		if (!m_pWSizeToolBar)
			CreateSizeToolBar();

		if (!m_pWAlignMoveToolBar)
			CreateAlignMoveToolBar(m_ptAnchor);
	}

	if (m_pWEditToolBar)
	{
		if (bAllowEditing)
			m_pWEditToolBar->SetText(ID_ALLOW_EDITING, _TB("Disable changes"), _TB("Disable changes to report"));
		else
			m_pWEditToolBar->SetText(ID_ALLOW_EDITING, _TB("Enable changes"), _TB("Enable changes to report"));
	}
	if (m_pWObjectToolBar)		ShowControlBar(m_pWObjectToolBar, bAllowEditing, FALSE, TRUE);
	if (m_pWSizeToolBar)		ShowControlBar(m_pWSizeToolBar, bAllowEditing, FALSE, TRUE);
	if (m_pWBorderToolBar)		ShowControlBar(m_pWBorderToolBar, bAllowEditing, FALSE, TRUE);
	if (m_pWAlignMoveToolBar)	ShowControlBar(m_pWAlignMoveToolBar, m_bAlignToolbarFloating ? FALSE : bAllowEditing, FALSE, TRUE);

	RecalcLayout(); 
}

//-----------------------------------------------------------------------------
void CWoormFrame::OnActivate(UINT nState, CWnd* pWndOther, BOOL bMinimized)
{
	__super::OnActivate(nState, pWndOther, bMinimized);

	if (!m_pWTabbedToolBar && m_pDoc && m_pDoc->m_pWoormInfo && m_pDoc->m_pWoormInfo->m_bIconized && !m_pDoc->m_pWoormInfo->m_bHideFrame)
	{
		if (nState == WA_ACTIVE || nState == WA_CLICKACTIVE)
			CreateTools(m_pDoc);
	}
}

//-----------------------------------------------------------------------------
BOOL CWoormFrame::CreateTools(CWoormDocMng* pDoc/* = NULL*/)
{
	ASSERT_VALID(m_pDoc);
	ASSERT(m_pDoc == pDoc);

	VERIFY(CreateStatusBar());

	CRect rectClient; 
	GetClientRect(&rectClient);

	m_ptAnchor = CPoint(rectClient.left + rectClient.Width() / 2, rectClient.top + rectClient.Height() / 2);

	if (m_pDoc)
		m_bCanUseReportEditor = m_pDoc->CanDoAllowEditingButton(TRUE); //AfxGetBaseApp()->CanUseReportEditor();

	CreateToolBar(m_pDoc);
	
	SetAllowEditButtonInfo(m_pDoc ? m_pDoc->m_bAllowEditing : FALSE);

	if (m_pDoc && m_pDoc->m_bIsNew && m_pWObjectToolBar)
		m_pWTabbedToolBar->SetActiveTab(m_pWObjectToolBar->GetName());

	m_pWTabbedToolBar->ResumeLayout();
	m_pWTabbedToolBar->AdjustLayout();

	return TRUE;
}

//-----------------------------------------------------------------------------
void CWoormFrame::OnBtnFilePrint()
{
	CWoormDocMng* pDocument = GetDocument();
	BOOL bMultiCopies = pDocument && pDocument->m_pWoormInfo  && pDocument->m_pWoormInfo->m_bMultiCopies;
	
	SendMessage(WM_COMMAND, bMultiCopies ? ID_PRINT_MULTICOPIES : ID_FILE_PRINT);
}

//------------------------------------------------------------------------------
//LPTSTR CWoormFrame::GetMenuCaption(UINT id, CString& strBuffer)
//{
//	strBuffer = cwsprintf(id); 
//	int idx = strBuffer.Find('\n');
//	if (idx > 0) 
//		strBuffer = strBuffer.Left(idx);		
//	strBuffer.Trim();
//	return (LPTSTR) (LPCTSTR) strBuffer;
//}

//------------------------------------------------------------------------------
BOOL CWoormFrame::OnPopulatedDropDown(UINT nIdCommand)
{
	if (!m_pWTabbedToolBar)
		return FALSE;

	if (m_pWTabbedToolBar->OnPopulatedDropDown(nIdCommand))
	{
		return TRUE;
	}

	if (!m_pDoc)
		return FALSE;

	CTBToolBarMenu menu;
	menu.CreateMenu();

	// Switch on button command id's.
	//case ID_TOOLBAR_FIND_NEXT ---------------------------
	if (nIdCommand == ID_TOOLBAR_FIND_NEXT)
	{
		if (!m_pWToolBar) return FALSE;

		DataObj* pDataObj = DataObj::DataObjCreate(m_pDoc->m_findWordInfo.m_Type);
		m_pDoc->SetFindCondition(m_pDoc->m_findWordInfo.m_eCmp);
		DROPDOWN_MENU_POPOLATE(m_pWToolBar, pDataObj, m_pDoc->GetFindCondition())
	}
	//case ID_RUN_STOP ------------------------------------
	else if (nIdCommand == ID_RUN_STOP || nIdCommand == ID_RUN_STOP_2)
	{
		menu.AppendMenu(MF_STRING, ID_RUN_STOP, (LPTSTR)(LPCTSTR)_TB("&Run"));
		menu.AppendMenu(MF_SEPARATOR);

		if (m_pDoc->m_bAllowEditing && m_pDoc->m_bBetaFeatures)
		{
			menu.AppendMenu(MF_STRING /*| (m_pDoc->m_bDebugging ? MF_CHECKED : 0)*/, ID_RS_RUN_DEBUGGER, (LPTSTR)(LPCTSTR)_TB("Run in debugging mode"), TBIcon(szIconDebug, TOOLBAR), TBIcon(szIconDebug, TOOLBAR));
			menu.AppendMenu(MF_SEPARATOR);
		}

		menu.AppendMenu(MF_STRING, ID_RUN_WITHOUT_CHILDS, (LPTSTR)(LPCTSTR)_TB("&Run &without children reports"));
		if (AfxIsActivated(TBEXT_APP, MAILCONNECTOR_ACT))
		{
			menu.AppendMenu(
				MF_STRING,
				ID_RUN_AND_ATTACH_PDF_CHILDS,
				(LPTSTR)(LPCTSTR)_TB("Run and attach children reports to &Emails"));
		}

		if (m_arUICultures.GetSize())
		{
			menu.AppendMenu(MF_SEPARATOR);
			for (int i = 0; i < min(m_arUICultures.GetSize(), ID_EXTDOC_DDTB_ENUMREPORT_MAX); i++)
			{
				CString str = m_arUICultures.GetAt(i);
				if (str.IsEmpty()) str = _T("en");
				BOOL bIsCurrent = str.CompareNoCase(m_pDoc->GetUICulture()) == 0;
				str = m_arUICultureDescriptions.GetAt(i);
				menu.AppendMenu(MF_STRING | (bIsCurrent ? MF_CHECKED : 0), ID_EXTDOC_DDTB_ENUMREPORT0 + i, (LPTSTR)(LPCTSTR)str);
			}
		}
		if (nIdCommand == ID_RUN_STOP)
		{
			if (m_pWToolBar) m_pWToolBar->UpdateDropdownMenu(nIdCommand, &menu);
		}
		else if (nIdCommand == ID_RUN_STOP_2)
		{
			if (m_pWEditToolBar) m_pWEditToolBar->UpdateDropdownMenu(nIdCommand, &menu);
		}
	}
	//case ID_BTN_FILE_PRINT ------------------------------------
	else if (nIdCommand == ID_BTN_FILE_PRINT)
	{
		BOOL bMultiCopies = m_pDoc->m_pWoormInfo  && m_pDoc->m_pWoormInfo->m_bMultiCopies;

		menu.AppendMenu(MF_STRING | (!bMultiCopies ? MF_CHECKED : 0), ID_FILE_PRINT, (LPTSTR)(LPCTSTR)_TB("&Print on report preferred printer"));
		menu.AppendMenu(MF_STRING, ID_PRINT_WITH_DIALOG, (LPTSTR)(LPCTSTR)_TB("P&rint on..."));
		{
			WoormField* pF = GetDocument()->m_ViewSymbolTable.GetField(SpecialReportField::NAME.PRINT_ON_LETTERHEAD);
			BOOL bShowLetterhead = pF && (pF->GetRefCount() > 0);
			if (bShowLetterhead)
			{
				menu.AppendMenu(MF_STRING | (m_pDoc->m_bPrintOnLetterhead ? MF_CHECKED : 0), ID_PRINT_ON_LETTERHEAD, (LPTSTR)(LPCTSTR)_TB("Print on &letterhead"));
			}
		}
		menu.AppendMenu(MF_SEPARATOR);
		menu.AppendMenu(MF_STRING, ID_PRINT_WITH_CHILDS, (LPTSTR)(LPCTSTR)_TB("Run and print also &children reports"));
		menu.AppendMenu(MF_STRING | (bMultiCopies ? MF_CHECKED : 0), ID_PRINT_MULTICOPIES, (LPTSTR)(LPCTSTR)_TB("Run and print also &multicopies of report"));

		if (m_pWToolBar) m_pWToolBar->UpdateDropdownMenu(nIdCommand, &menu);
	}
	//case ID_WRM_EASY_READING ------------------------------------
	else if (nIdCommand == ID_WRM_EASY_READING)
	{
		Table* pT = NULL;
		for (int i = 0; i <= m_pDoc->GetObjects().GetUpperBound(); i++)
		{
			BaseObj* pObj = m_pDoc->GetObjects()[i];
			if (!pObj->IsKindOf(RUNTIME_CLASS(Table)))
				continue;

			pT = (Table*)pObj;
			break;
		}
		if (pT)
		{
			menu.AppendMenu(MF_STRING | (pT->EasyReadingByColorEnabled() ? MF_CHECKED : 0),
				ID_WRM_EASY_READING_BYCOLOR,
				(LPTSTR)(LPCTSTR)_TB("&Easy reading"));
			menu.AppendMenu(MF_STRING | (pT->EasyReadingByColorDynamicEnabled() ? MF_CHECKED : 0),
				ID_WRM_EASY_READING_BYCOLOR_DYNAMIC,
				(LPTSTR)(LPCTSTR)_TB("Dynamic easy reading"));
			
			menu.AppendMenu(MF_SEPARATOR);

			menu.AppendMenu(MF_STRING | (pT->EasyReadingByLineEnabled() ? MF_CHECKED : 0),
				ID_WRM_EASY_READING_BYLINE,
				(LPTSTR)(LPCTSTR)_TB("&Show row separator"));
			menu.AppendMenu(MF_STRING | (pT->EasyReadingByLineDynamicEnabled() ? MF_CHECKED : 0),
				ID_WRM_EASY_READING_BYLINE_DYNAMIC,
				(LPTSTR)(LPCTSTR)_TB("Show dynamic rows separator"));

			if (m_pWEditToolBar) m_pWEditToolBar->UpdateDropdownMenu(nIdCommand, &menu);
		}
	}
	//case ID_ARCHIVE_PDF_FORMAT ------------------------------------
	else if (nIdCommand == ID_ARCHIVE_PDF_FORMAT)
	{
		menu.AppendMenu(MF_STRING, ID_ARCHIVE_PDF_FORMAT, (LPTSTR)(LPCTSTR)_TB("&Archive"));
		menu.AppendMenu(MF_STRING, ID_ARCHIVE_WITH_CHILDS, (LPTSTR)(LPCTSTR)_TB("Archive with &childs"));
		if (m_pDoc->GetCallerDocument() && AfxGetIDMSRepositoryManager()->BarcodeEnabled())
			menu.AppendMenu(MF_STRING, ID_GENERATE_PAPERY, (LPTSTR)(LPCTSTR)_TB("&Generate only papery"));

		if (m_pWEditToolBar) m_pWEditToolBar->UpdateDropdownMenu(nIdCommand, &menu);
	}
	//case ID_EXPORT_DATA ------------------------------------
	else if (nIdCommand == ID_EXPORT_DATA)
	{
		menu.AppendMenu(MF_STRING, ID_EXPORT_EXCELNET, (LPTSTR)(LPCTSTR)_TB("Export data to Microsoft Excel"), IDB_WOORM_EXPORT_EXCELNET, 0, TRUE);

		if (GetDocument()->m_bBetaFeatures)
			menu.AppendMenu(MF_STRING, ID_EXPORT_OPENXML_EXCEL, (LPTSTR)(LPCTSTR)_TB("Export data on OpenXml Excel format file"), IDB_WOORM_EXPORT_EXCELNET, 0, TRUE);

		menu.AppendMenu(MF_STRING, ID_EXPORT_WORD_NET, (LPTSTR)(LPCTSTR)_TB("Export data to Microsoft Word"), IDB_WOORM_EXPORT_WORD, 0, TRUE);

		menu.AppendMenu(MF_STRING, ID_EXPORT_OPENOFFICE_ODS, (LPTSTR)(LPCTSTR)_TB("Export data to OpenOffice Sheet"), IDB_WOORM_EXPORT_OPENOFFICE, 0, TRUE);
		menu.AppendMenu(MF_STRING, ID_EXPORT_OPENOFFICE_ODT, (LPTSTR)(LPCTSTR)_TB("Export data to OpenOffice Document"), IDB_WOORM_EXPORT_OPENOFFICE_ODT, 0, TRUE);

		menu.AppendMenu(MF_STRING, ID_EXPORT_HTML, (LPTSTR)(LPCTSTR)_TB("Export data to HTML format file"), IDB_WOORM_EXPORT_HTML, 0, TRUE);
		menu.AppendMenu(MF_STRING, ID_EXPORT_TEXT, (LPTSTR)(LPCTSTR)_TB("Export data to plain text file"), IDB_WOORM_EXPORT_TEXT, 0, TRUE);

		menu.AppendMenu(MF_STRING, ID_EXPORT_XML, (LPTSTR)(LPCTSTR)_TB("Export data to XML format file"), IDB_WOORM_EXPORT_XML, 0, TRUE);

		if (GetDocument()->m_bBetaFeatures)
		{
			menu.AppendMenu(MF_STRING, ID_EXPORT_JSON,		(LPTSTR)(LPCTSTR)_TB("Export data on JSON format file"),		IDB_WOORM_EXPORT_XML, 0, TRUE);
			menu.AppendMenu(MF_STRING, ID_EXPORT_XML_FULL,	(LPTSTR)(LPCTSTR)_TB("Export data on XML FULL format file"),	IDB_WOORM_EXPORT_XML, 0, TRUE);
		}

		menu.AppendMenu(MF_SEPARATOR);
		
		menu.AppendMenu(MF_STRING, ID_SEND_MAIL, (LPTSTR)(LPCTSTR)_TB("Send report by email"), IDB_WOORM_SEND_EMAIL, 0, TRUE);
		if (AfxGetIMailConnector()->IsPostaLiteEnabled())
			menu.AppendMenu(MF_STRING, ID_SEND_POSTALITE, (LPTSTR)(LPCTSTR)_TB("Send report by PostaLite service"), IDB_WOORM_SEND_EMAIL, 0, TRUE);

		menu.AppendMenu(MF_STRING, ID_FILE_SAVE_PDF, (LPTSTR)(LPCTSTR)_TB("Save report to PDF format file"), IDB_WOORM_EXPORT_PDF, 0, TRUE);
		menu.AppendMenu(MF_STRING | (GetDocument()->m_bDirectConcatPDF ? MF_CHECKED : 0), ID_TOGGLE_CONCAT_PDF, (LPTSTR)(LPCTSTR)_TB("&Concat Pdf children reports"), (GetDocument()->m_bDirectConcatPDF ? IDB_TOGGLE_CONCAT_PDF : 0/*IDB_TOGGLE_CONCAT_PDF_OFF */), 0, TRUE);
		
		if (m_pWEditToolBar) m_pWEditToolBar->UpdateDropdownMenu(nIdCommand, &menu);
	}
	//case ID_ACTIONS_REP_COPY ------------------------------------
	else if (nIdCommand == ID_ACTIONS_REP_COPY)
	{
		menu.AppendMenu(MF_STRING, ID_ACTIONS_REP_COPY, (LPTSTR)(LPCTSTR)_TB("&Copy report link"));
		menu.AppendMenu(MF_STRING, ID_ACTIONS_REP_XML_COPY, (LPTSTR)(LPCTSTR)_TB("Copy report &XML link"));

		if (m_pWEditToolBar) m_pWEditToolBar->UpdateDropdownMenu(nIdCommand, &menu);
	}
	else if (nIdCommand == ID_RS_ADD_TABLE_FROM_DB)
	{
		menu.AppendMenu(MF_STRING, ID_RS_ADD_TABLE_FROM_DB,				(LPTSTR)(LPCTSTR)_TB("Add a table from DB"));
		menu.AppendMenu(MF_STRING, ID_RS_ADD_TABLE_NEW_FUNCEXPR,		(LPTSTR)(LPCTSTR)_TB("Add a table from a new expression/function variable"));
		menu.AppendMenu(MF_STRING, ID_RS_ADD_TABLE_FROM_HIDDEN_FIELDS,	(LPTSTR)(LPCTSTR)_TB("Add a table from hiddens variables"));
		if (m_pWObjectToolBar) m_pWObjectToolBar->UpdateDropdownMenu(nIdCommand, &menu);
	}
	else if (nIdCommand == ID_RS_ADD_COLUMNS_FROM_DB)
	{
		menu.AppendMenu(MF_STRING, ID_RS_ADD_COLUMNS_FROM_DB,			(LPTSTR)(LPCTSTR)_TB("Add columns from DB"));
		menu.AppendMenu(MF_STRING, ID_RS_ADD_COLUMN_NEW_FUNCEXPR,		(LPTSTR)(LPCTSTR)_TB("Add column from a new expression/function variable"));
		menu.AppendMenu(MF_STRING, ID_RS_ADD_COLUMNS_FROM_HIDDENS,		(LPTSTR)(LPCTSTR)_TB("Add columns from hiddens variables"));
		if (m_pWObjectToolBar) m_pWObjectToolBar->UpdateDropdownMenu(nIdCommand, &menu);
	}
	else if (nIdCommand == ID_RS_ADD_FIELDS_FROM_DB)
	{
		menu.AppendMenu(MF_STRING, ID_RS_ADD_FIELDS_FROM_DB,			(LPTSTR)(LPCTSTR)_TB("Add fields from DB"));
		menu.AppendMenu(MF_STRING, ID_RS_ADD_FIELD_NEW_FUNCEXPR,		(LPTSTR)(LPCTSTR)_TB("Add field from a new expression/function variable"));
		menu.AppendMenu(MF_STRING, ID_RS_ADD_FIELDS_FROM_HIDDENS,		(LPTSTR)(LPCTSTR)_TB("Add fields from hiddens variables"));
		if (m_pWObjectToolBar) m_pWObjectToolBar->UpdateDropdownMenu(nIdCommand, &menu);
	}
	else if (nIdCommand == ID_FILE_SAVE || nIdCommand == ID_FILE_SAVE_2)
	{
		menu.AppendMenu(MF_STRING, ID_FILE_SAVE,						(LPTSTR)(LPCTSTR)_TB("Save"));
		menu.AppendMenu(MF_STRING, ID_FILE_SAVE_AS,						(LPTSTR)(LPCTSTR)_TB("Sav&e as..."));
		menu.AppendMenu(MF_SEPARATOR);

		menu.AppendMenu(MF_STRING, ID_FILE_SAVE_AS_TEMPLATE,			(LPTSTR)(LPCTSTR)_TB("Save as &template..."));
		menu.AppendMenu(MF_STRING, ID_OPTION_CREATE_SCHEMA1,			(LPTSTR)(LPCTSTR)_TB("Generate XSD Schema (Standard)"));
		menu.AppendMenu(MF_STRING, ID_OPTION_CREATE_SCHEMA2,			(LPTSTR)(LPCTSTR)_TB("Generate XSD Schema (Advanced)"));
		menu.AppendMenu(MF_STRING, ID_SAVE_ONLY_GRAPH_INFO,				(LPTSTR)(LPCTSTR)_TB("Save only &graphical data"));

		if (nIdCommand == ID_FILE_SAVE)
			if (m_pWEditToolBar) m_pWEditToolBar->UpdateDropdownMenu(nIdCommand, &menu);
		else if (nIdCommand == ID_FILE_SAVE_2)
			if (m_pWToolBar) m_pWToolBar->UpdateDropdownMenu(nIdCommand, &menu);
	}
	else if (nIdCommand == ID_TOOLS || nIdCommand == ID_TOOLS_2)
	{
		if (AfxIsActivated(OFM_APP, _NS_ACT("Core")) && !m_pDoc->m_bAllowEditing)
		{
			menu.AppendMenu(MF_STRING, ID_PROPERTIES, (LPTSTR)(LPCTSTR)_TB("&Properties"));
			menu.AppendMenu(MF_STRING, ID_PAGE_INFO, (LPTSTR)(LPCTSTR)_TB("Page &info"));
			menu.AppendMenu(MF_SEPARATOR);
			menu.AppendMenu(MF_STRING, ID_VIEW_STATUS_BAR, (LPTSTR)(LPCTSTR)_TB("S&tatus Bar"));
		}
		else
		{
			menu.AppendMenu(MF_STRING, ID_GENERAL_SETTINGS, (LPTSTR)(LPCTSTR)_TB("&General Settings"));
			menu.AppendMenu(MF_STRING, ID_PROPERTIES, (LPTSTR)(LPCTSTR)_TB("&Properties"));
			menu.AppendMenu(MF_STRING, ID_PAGE_INFO, (LPTSTR)(LPCTSTR)_TB("Page &info"));
			menu.AppendMenu(MF_STRING, ID_REPORT_PARAMETER, (LPTSTR)(LPCTSTR)_TB("Show &Parameters"));
			menu.AppendMenu(MF_SEPARATOR);
			menu.AppendMenu(MF_STRING, ID_FONT_STYLES, (LPTSTR)(LPCTSTR)_TB("&Font style..."));
			menu.AppendMenu(MF_STRING, ID_FORMAT_STYLES, (LPTSTR)(LPCTSTR)_TB("&Format style..."));
			menu.AppendMenu(MF_STRING, ID_ENUMS_VIEWER, (LPTSTR)(LPCTSTR)_TB("&Enums Viewer..."));
			menu.AppendMenu(MF_SEPARATOR);
			menu.AppendMenu(MF_STRING, ID_LOAD_TEMPLATE, (LPTSTR)(LPCTSTR)_TB("Load st&yle template"));
			menu.AppendMenu(MF_STRING, ID_UNLOAD_TEMPLATE, (LPTSTR)(LPCTSTR)_TB("Unload style &template"));
			menu.AppendMenu(MF_STRING, ID_CLEAR_ALL_CUSTOM_STYLE, (LPTSTR)(LPCTSTR)_TB("&Clear all custom styles"));
			menu.AppendMenu(MF_SEPARATOR);
			menu.AppendMenu(MF_STRING, ID_ADD_EA_BARCODE, (LPTSTR)(LPCTSTR)_TB("Add DMS Barcode"));
			menu.AppendMenu(MF_SEPARATOR);
			menu.AppendMenu(MF_STRING, ID_COMPILE, (LPTSTR)(LPCTSTR)_TB("&Validity check\tCtrl+F9"));
			menu.AppendMenu(MF_SEPARATOR);
			menu.AppendMenu(MF_STRING, ID_VIEW_STATUS_BAR, (LPTSTR)(LPCTSTR)_TB("S&tatus Bar"));
		}

		if (nIdCommand == ID_TOOLS)
			if (m_pWEditToolBar) m_pWEditToolBar->UpdateDropdownMenu(nIdCommand, &menu);
		else if (nIdCommand == ID_TOOLS_2)
			if (m_pWToolBar) m_pWToolBar->UpdateDropdownMenu(nIdCommand, &menu);
	}

	return TRUE;
}

//-----------------------------------------------------------------------------
 void CWoormFrame::OnCultureSelected (UINT nID)
 {
	 int nIdx = nID - ID_EXTDOC_DDTB_ENUMREPORT0;
	 if (nIdx < 0 || nIdx > m_arUICultures.GetUpperBound())
		return;

	CString sUICulture = m_arUICultures[nIdx];

	m_pDoc->m_bCultureVariable = TRUE;
	m_pDoc->SetUICulture(sUICulture);
	m_pDoc->SetCultureStrings(sUICulture);

	SendMessage(WM_COMMAND, ID_RUN_STOP);
 }

//-----------------------------------------------------------------------------
BOOL CWoormFrame::OnDrawMenuImage (	CDC* pDC, const CBCGPToolbarMenuButton* pMenuButton, const CRect& rectImage)
{
	// Draw the Checked
	BOOL bIcon = FALSE;

	if (m_pWTabbedToolBar && pMenuButton)
	{
		HICON hIcon = m_pWTabbedToolBar->GetIconDropdownMenu(pMenuButton);

		if (hIcon)
		{
			// Calculate for center the icon
			CSize sz = rectImage.Size(); 
			int xSpace = (int) ((sz.cx / 2) - (TOOLBARMENU_ICON_SIZE / 2));
			int ySpace = (int) ((sz.cy / 2) - (TOOLBARMENU_ICON_SIZE/ 2));
			int xPos = rectImage.left + xSpace;
			int yPos = rectImage.top + ySpace;

			// Draw the icon 
			::DrawIconEx (pDC->GetSafeHdc (), xPos, yPos, hIcon, TOOLBARMENU_ICON_SIZE, TOOLBARMENU_ICON_SIZE, 0, NULL, DI_NORMAL);
			bIcon = TRUE;
		}
	}

	// Draw the Checked
	if (pMenuButton->m_nStyle & TBBS_CHECKED)
	{
		if (bIcon)
		{
			CRect rect = CRect(rectImage.left + 15, rectImage.top, rectImage.right + 15, rectImage.bottom);
			CBCGPMenuImages::Draw (pDC, (CBCGPMenuImages::IMAGES_IDS) CBCGPMenuImages::IdCheck, rect, CBCGPMenuImages::ImageGray);
		}
		else
		{
			CBCGPVisualManager::GetInstance ()->OnDrawMenuCheck (pDC, (CBCGPToolbarMenuButton*) pMenuButton, rectImage, 16, FALSE);
		}
	}

	return TRUE;
}

//-----------------------------------------------------------------------------
BOOL CWoormFrame::CreateToolBar(CWoormDocMng* /*pDoc = NULL*/)
{
	m_pWTabbedToolBar = new CTBTabbedToolbar();
	//ALTERNATIVA m_pTabbedToolBar->SetAutoHideToolBarButton(FALSE);
	m_pWTabbedToolBar->EnableHighlightedColorClick(TRUE);
	//m_pTabbedToolBar->SetSwitchEnableAlwaysDropDown(TRUE);

	if (!m_pWTabbedToolBar->Create(this))
		return FALSE;

	m_pWTabbedToolBar->SuspendLayout();
	m_pWTabbedToolBar->SetWindowText (_T("Tabbed"));
	m_pWTabbedToolBar->SetIDSwitch(ID_WOORM_TAB_SWITCH, ID_WOORM_TAB_SWITCH_MENU_START);

	//ALTERNATIVA m_pTabbedToolBar->SetShowToolBarTab();

	m_pWToolBar	= new CTBToolBar();	
	if(!m_pWToolBar->CreateEmptyTabbedToolbar(this, NAME_MAIN_TOOLBAR, _TB("Main")))
	{ 
		TRACE(L"Failed to create toolbar\n");
		return FALSE; 
	}
	m_pWTabbedToolBar->AddTab(m_pWToolBar, TRUE, TRUE);

	m_pWEditToolBar  = new CTBToolBar();
	if (!m_pWEditToolBar->CreateEmptyTabbedToolbar(this, NAME_MAIN_EDIT_TOOLBAR, _TB("Tools"))) 
	{ 
		TRACE(L"Failed to create toolbar\n"); 
		return FALSE; 
	}
	m_pWTabbedToolBar->AddTab(m_pWEditToolBar);

	m_pWTabbedToolBar->ChangeImage(ID_WOORM_TAB_SWITCH, AfxGetRSIcon(ID_WOORM_TAB_SWITCH));
	m_pWTabbedToolBar->SetCollapsedImage(AfxGetRSIcon(ID_WOORM_TOOLBAR_COLLAPSED));

	/////////////////////////////////////////////////////////////////////////////////////////
	if (IsRadarFrame())
	{	
		if(AfxIsRSIconVisible(ID_WRMRADAR_FIXED))
			m_pWToolBar->AddButton(ID_WRMRADAR_FIXED, RS_NS, AfxGetRSIcon(ID_WRMRADAR_FIXED), _TB("Pin Radar"), _TB("Leave the radar open"));
		m_pWToolBar->AddSeparator();
	}
	/*if (m_bCanUseReportEditor)
	{
		m_pToolBar->AddButton(ID_FILE_SAVE, RS_NS, TBIcon(szIconSave, TOOLBAR), _TB("Save report"), _TB("Save active document"));
		m_pToolBar->AddSeparator();
	}*/

	if (AfxIsRSIconVisible(ID_PG_HOME))
		m_pWToolBar->AddButton(ID_PG_HOME, RS_NS, AfxGetRSIcon(ID_PG_HOME), _TB("First"), _TB("Go to first page"));
	if (AfxIsRSIconVisible(ID_PG_UP))
		m_pWToolBar->AddButton(ID_PG_UP, RS_NS, AfxGetRSIcon(ID_PG_UP), _TB("Previous"), _TB("Go to previous page"));
	if (AfxIsRSIconVisible(ID_PG_DN))
		m_pWToolBar->AddButton(ID_PG_DN, RS_NS, AfxGetRSIcon(ID_PG_DN), _TB("Next"), _TB("Go to next page"));
	if (AfxIsRSIconVisible(ID_PG_END))
		m_pWToolBar->AddButton(ID_PG_END, RS_NS, AfxGetRSIcon(ID_PG_END), _TB("Last"), _TB("Go to last page"));
	m_pWToolBar->AddSeparator();

	if (AfxIsRSIconVisible(ID_BTN_FILE_PRINT))
	{ 
		m_pWToolBar->AddButton(ID_BTN_FILE_PRINT, RS_NS, AfxGetRSIcon(ID_BTN_FILE_PRINT), _TB("Print"), _TB("Print on preferred printer"));
		m_pWToolBar->SetDropdown(ID_BTN_FILE_PRINT);
		m_pWToolBar->EnableAlwaysDropDown(ID_BTN_FILE_PRINT, FALSE);
	}

	if (AfxIsRSIconVisible(ID_FILE_PRINT_PREVIEW))
		m_pWToolBar->AddButton(ID_FILE_PRINT_PREVIEW, RS_NS, AfxGetRSIcon(ID_FILE_PRINT_PREVIEW), _TB("Print Preview"), _TB("Display full pages"));

	if (!IsRadarFrame())
	{
		m_pWToolBar->AddSeparator();
		if (m_bCanUseReportEditor && (AfxIsRSIconVisible(ID_FILE_SAVE_2)))
		{
			m_pWToolBar->AddButton(ID_FILE_SAVE_2, RS_NS, AfxGetRSIcon(ID_FILE_SAVE_2), _TB("Save report"), _TB("Save active document"));
			m_pWToolBar->SetDropdown(ID_FILE_SAVE_2);
			m_pWToolBar->EnableAlwaysDropDown(ID_FILE_SAVE_2, FALSE);
		}

		if (AfxIsRSIconVisible(ID_RUN_STOP))
		{
			m_pWToolBar->AddButton(ID_RUN_STOP, RS_NS, AfxGetRSIcon(ID_RUN_STOP), _TB("Run/Stop"), _TB("Run or Stop the report"));
			m_pWToolBar->SetDropdown(ID_RUN_STOP);
			m_pWToolBar->EnableAlwaysDropDown(ID_RUN_STOP, FALSE);
		}
		if (AfxIsRSIconVisible(ID_PAUSE_RESUME))
		{ 
			m_pWToolBar->AddButton(ID_PAUSE_RESUME, RS_NS, AfxGetRSIcon(ID_PAUSE_RESUME), _TB("Suspend/Resume"), _TB("Suspend or Resume the report"));		
			m_pWToolBar->AddSeparatorAfter(ID_PAUSE_RESUME);
		}

		m_pWToolBar->AddSeparator();
		if (AfxIsRSIconVisible(ID_USE_PREV_ASK_INPUT))
		{
			m_pWToolBar->AddButton(ID_USE_PREV_ASK_INPUT, RS_NS, AfxGetRSIcon(ID_USE_PREV_ASK_INPUT), _TB("Keep values"), _TB("Keep the field values of the request rules"));
			m_pWToolBar->PressButton(ID_USE_PREV_ASK_INPUT, TRUE);	// ID_USE_PREV_ASK_INPUT, stile TBBS_CHECKBOX.
		}
	}

	m_pWToolBar->AddEditToRight(ID_TOOLBAR_FIND_EDITBOX, RS_NS, _T("FindEdit"), 150, ES_AUTOHSCROLL, _T(""), _TB("Search:")); 
	if (AfxIsRSIconVisible(ID_TOOLBAR_FIND_PREV))
		m_pWToolBar->AddButtonToRight(ID_TOOLBAR_FIND_PREV, RS_NS, AfxGetRSIcon(ID_TOOLBAR_FIND_PREV), _TB("Previous"), _TB("Search previous record"));
	if (AfxIsRSIconVisible(ID_TOOLBAR_FIND_NEXT))
	{ 
		m_pWToolBar->AddButtonToRight(ID_TOOLBAR_FIND_NEXT, RS_NS, AfxGetRSIcon(ID_TOOLBAR_FIND_NEXT), _TB("Next"), _TB("Search next record"));
		m_pWToolBar->SetDropdown(ID_TOOLBAR_FIND_NEXT);
		m_pWToolBar->EnableAlwaysDropDown(ID_TOOLBAR_FIND_NEXT, FALSE);
	}

	if (AfxGetApplicationContext()->m_MacroRecorderStatus == CApplicationContext::RECORDING 
		|| AfxIsActivated(_T("RanorexTestSupport"), _T("RanorexTestSupport")))
		{ m_pWToolBar->AddButton(ID_REC_VALIDATION, RS_NS, _T("RecValidation"), IDB_REC_VALIDATION_LARGE, _TB("Rec Val"), FALSE); }

	//Show Full Report Script
	if (AfxIsRSIconVisible(ID_OPEN_AS_TEXT))
		m_pWToolBar->AddButtonToRight(ID_OPEN_AS_TEXT, RS_NS, AfxGetRSIcon(ID_OPEN_AS_TEXT), _TB("Editor"), _TB("&Show full report scripts within a editor view"));
	//Tools
	if (AfxIsRSIconVisible(ID_TOOLS_2))
	{
		m_pWToolBar->AddButtonToRight(ID_TOOLS_2, RS_NS, AfxGetRSIcon(ID_TOOLS_2), _TB("Tools"), _TB("&Tools"));
		m_pWToolBar->SetDropdown(ID_TOOLS_2);
		m_pWToolBar->EnableAlwaysDropDown(ID_TOOLS_2, TRUE);
		//separator
		m_pWToolBar->AddSeparatorAfter(ID_TOOLS_2);
	}
	
	if (AfxIsRSIconVisible(ID_FILE_CLOSE))
		m_pWToolBar->AddButtonToRight/*AllTab*/(ID_FILE_CLOSE, RS_NS, AfxGetRSIcon(ID_FILE_CLOSE), _TB("Exit"), _TB("&Exit"));

	// by default help button is disable, it will be enabled when 
	// OnCommandUI message will start to be notified
	m_pWToolBar->EnableButton(ID_HELP_WOORM, FALSE);

	// ---------------------------------------------------------------------------------------------
	// NEW TOOLBAR FOR EDITING
	if (m_bCanUseReportEditor)
	{
		if (AfxIsRSIconVisible(ID_ALLOW_EDITING))
			m_pWEditToolBar->AddButton(ID_ALLOW_EDITING, RS_NS, AfxGetRSIcon(ID_ALLOW_EDITING), _TB("Enable changes"), _TB("Enable changes to report"));
		if (AfxIsRSIconVisible(ID_FILE_SAVE))
		{
			m_pWEditToolBar->AddButton(ID_FILE_SAVE, RS_NS, AfxGetRSIcon(ID_FILE_SAVE), _TB("Save report"), _TB("Save active document"));
			m_pWEditToolBar->SetDropdown(ID_FILE_SAVE);
			m_pWEditToolBar->EnableAlwaysDropDown(ID_FILE_SAVE, FALSE);
		}
		if (AfxIsRSIconVisible(ID_RUN_STOP_2))
		{
			m_pWEditToolBar->AddButton(ID_RUN_STOP_2, RS_NS, AfxGetRSIcon(ID_RUN_STOP_2), _TB("Run/Stop"), _TB("Run or Stop the report"));
			m_pWEditToolBar->SetDropdown(ID_RUN_STOP_2);
			m_pWEditToolBar->EnableAlwaysDropDown(ID_RUN_STOP_2, FALSE);
		}
		if (AfxIsRSIconVisible(ID_PAUSE_RESUME))
			m_pWEditToolBar->AddButton(ID_PAUSE_RESUME, RS_NS, AfxGetRSIcon(ID_PAUSE_RESUME), _TB("Suspend/Resume"), _TB("Suspend or Resume the report"));

		m_pWEditToolBar->AddSeparator();
	}

	//szIconExport oggi è uguale a szIconExit, usiamo temporaneamente szIconExtract
	if (AfxIsRSIconVisible(ID_EXPORT_DATA))
	{
		m_pWEditToolBar->AddButton(ID_EXPORT_DATA, RS_NS, AfxGetRSIcon(ID_EXPORT_DATA), _TB("Export data..."), _TB("Export data..."));
		m_pWEditToolBar->SetDropdown(ID_EXPORT_DATA); 
	}
	//Easy Attachment	
	if (AfxGetOleDbMng()->EasyAttachmentFullEnable() && AfxIsRSIconVisible(ID_ARCHIVE_PDF_FORMAT))
	{
		m_pWEditToolBar->AddButton(ID_ARCHIVE_PDF_FORMAT, RS_NS, AfxGetRSIcon(ID_ARCHIVE_PDF_FORMAT), _TB("Archive report"), _TB("Archive report in DMS repository using PDF format"));
		m_pWEditToolBar->SetDropdown(ID_ARCHIVE_PDF_FORMAT);
	}

	m_pWEditToolBar->AddSeparator();
	if (AfxIsRSIconVisible(ID_WRM_EASY_READING))
	{
		m_pWEditToolBar->AddButton(ID_WRM_EASY_READING, RS_NS, AfxGetRSIcon(ID_WRM_EASY_READING), _TB("Easy reading"), _TB("Enable/Disable easy reading"));
		m_pWEditToolBar->SetDropdown(ID_WRM_EASY_READING);
	}
	if (IsRadarFrame())
	{
		if (AfxIsRSIconVisible(ID_WRM_MINUS_LINES))
			m_pWEditToolBar->AddButton(ID_WRM_MINUS_LINES, RS_NS, AfxGetRSIcon(ID_WRM_MINUS_LINES), _TB("Decrease"), _TB("Reduce row height"));
		if (AfxIsRSIconVisible(ID_WRM_PLUS_LINES))
			m_pWEditToolBar->AddButton(ID_WRM_PLUS_LINES, RS_NS, AfxGetRSIcon(ID_WRM_PLUS_LINES), _TB("Increase row height"));
	}

	if (!IsRadarFrame())
	{
		if (AfxIsRSIconVisible(ID_WMR_RERUN_STARTSTOP))
		{
			m_pWEditToolBar->AddButtonToRight(ID_WMR_RERUN_STARTSTOP, RS_NS, AfxGetRSIcon(ID_WMR_RERUN_STARTSTOP), _TB("ReRun report"), _TB("Run or Stop the ReRun report"));
			m_pWEditToolBar->PressButton(ID_WMR_RERUN_STARTSTOP, FALSE);		
			m_pWEditToolBar->AddComboBoxEditToRight(ID_WMR_COMBO_TIME_RERUN, RS_NS, 100, _TB("Time in Minutes"), -1, _T("5"), FALSE);
			for (int k = 1; k <= 15; k++)
			{
				CString str;
				str.Format(_T("%d"), k);
				m_pWEditToolBar->AddComboItem(ID_WMR_COMBO_TIME_RERUN, str);
			}
			m_pWEditToolBar->AddSeparatorAfter(ID_WMR_COMBO_TIME_RERUN);
		}
		if (AfxIsRSIconVisible(ID_ACTIONS_REP_COPY))
		{
			m_pWEditToolBar->AddButtonToRight(ID_ACTIONS_REP_COPY, RS_NS, AfxGetRSIcon(ID_ACTIONS_REP_COPY), _TB("Share"), _TB("Share report link"));
			m_pWEditToolBar->SetDropdown(ID_ACTIONS_REP_COPY);
		}
	}

	//Show Full Report Script
	if (AfxIsRSIconVisible(ID_OPEN_AS_TEXT))
		m_pWEditToolBar->AddButtonToRight(ID_OPEN_AS_TEXT, RS_NS, AfxGetRSIcon(ID_OPEN_AS_TEXT), _TB("Editor"), _TB("&Show full report scripts within a editor view"));
	//Tools
	if (AfxIsRSIconVisible(ID_TOOLS))
	{
		m_pWEditToolBar->AddButtonToRight(ID_TOOLS, RS_NS, AfxGetRSIcon(ID_TOOLS), _TB("Tools"), _TB("&Tools"));
		m_pWEditToolBar->SetDropdown(ID_TOOLS);
		m_pWEditToolBar->EnableAlwaysDropDown(ID_TOOLS, TRUE);
		//separator
		m_pWEditToolBar->AddSeparatorAfter(ID_TOOLS);
	}

	if (AfxIsRSIconVisible(ID_FILE_CLOSE))
		m_pWEditToolBar->AddButtonToRight/*AllTab*/(ID_FILE_CLOSE, RS_NS, AfxGetRSIcon(ID_FILE_CLOSE), _TB("Exit"), _TB("&Exit"));

	m_pWEditToolBar->AddSeparator();
	m_pWEditToolBar->AddComboBox(IDC_DROPDOWN_TEMPLATES, RS_NS, _T("WoormComboTemplates"), 150, (WS_CHILD | WS_VISIBLE | CBS_NOINTEGRALHEIGHT | CBS_DROPDOWNLIST | WS_VSCROLL /*| CBS_SORT*/), _TB("Template"));
	if (!m_pDoc->m_Template.m_pWoormTpl)
		m_pWEditToolBar->HideButton(IDC_DROPDOWN_TEMPLATES, TRUE);

	//ID_WRM_COLLAPSED_START
	m_pWTabbedToolBar->SetStartIDCollapsed(ID_WRM_COLLAPSED_START);

	// resize Woorm frame
	CRect cRect;GetWindowRect(cRect);
	cRect.right = cRect.left + max(m_pWTabbedToolBar->CalcMinimumWidth(), cRect.Width());
	SetWindowPos ( NULL,0,0,cRect.Width(), cRect.Height(), SWP_NOMOVE | SWP_NOACTIVATE | SWP_NOZORDER );
	
	CreateDDLTemplates();

	return TRUE;
}

//-----------------------------------------------------------------------------
BOOL CWoormFrame::CreateObjectToolBar(CWoormDocMng* pDoc /*= NULL*/)
{
	ASSERT(m_pWObjectToolBar == NULL);
	if (m_pWObjectToolBar)
		return TRUE;

	m_pWObjectToolBar = new CTBToolBar();

	if (!m_pWObjectToolBar->CreateEmptyTabbedToolbar(this, NAME_OBJECT_TOOLBAR, _TB("Object")))
	{
		TRACE(L"Failed to create object toolbar\n");
		return FALSE;
	}

	m_pWTabbedToolBar->AddTab(m_pWObjectToolBar, FALSE);

	m_pWObjectToolBar->AddButton(ID_ADD_SQR_RECT, RS_NS,				TBIcon(szIconRectangle, TOOLBAR), _TB("Rectangle"));
	m_pWObjectToolBar->AddButton(ID_ADD_GRAPH_RECT, RS_NS,			TBIcon(szIconPicture, TOOLBAR), _TB("Image"));
	m_pWObjectToolBar->AddButton(ID_ADD_TEXT_RECT, RS_NS,			TBIcon(szIconGenericText, TOOLBAR), _TB("Text"));
	m_pWObjectToolBar->AddButton(ID_ADD_FILE_RECT, RS_NS,			TBIcon(szIconImportTxt, TOOLBAR), _TB("Text File"));
	m_pWObjectToolBar->AddButton(ID_ADD_REPEATER, RS_NS,				TBIcon(szIconRepeater, TOOLBAR), _TB("Repeater"));
	
	m_pWObjectToolBar->AddButton(ID_RS_ADD_TABLE_FROM_DB, RS_NS,		TBIcon(szIconTable, TOOLBAR),		_TB("Table"), _TB("\nAdd a new Table"));
	m_pWObjectToolBar->SetDropdown(ID_RS_ADD_TABLE_FROM_DB);

	m_pWObjectToolBar->AddButton(ID_RS_ADD_COLUMNS_FROM_DB, RS_NS,	TBIcon(szIconRSColumn, TOOLBAR),	_TB("Columns"), _TB("\nAdd columns"));
	m_pWObjectToolBar->SetDropdown(ID_RS_ADD_COLUMNS_FROM_DB);

	m_pWObjectToolBar->AddButton(ID_RS_ADD_FIELDS_FROM_DB, RS_NS,	TBIcon(szIconRSField, TOOLBAR),	_TB("Fields"), _TB("\nAdd fields"));
	m_pWObjectToolBar->SetDropdown(ID_RS_ADD_FIELDS_FROM_DB);

	m_pWObjectToolBar->AddButton(ID_RS_REMOVE_LAYOUT_OBJECT, RS_NS,	TBIcon(szIconRemoveField, TOOLBAR),		_TB("Remove object"), _TB("\nRemove layout object. The variable will become hidden."));
	m_pWObjectToolBar->AddButton(ID_RS_ADD_LINK, RS_NS,				TBIcon(szIconLinkTo, TOOLBAR),			_TB("Add Link"), _TB("\nAdd new hyperlink"));

	m_pWObjectToolBar->SetAutoHideToolBarButton(FALSE);
	return TRUE;
}

//Cambio l'img dei pulsanti relativi alla creazione di field o colonne (da db e derivati da funzioni o espressioni)
//e quello per rimuovere l'oggetto grafico di una variabile che verrà messa nell'elenco di quelle nascoste
void CWoormFrame::ChangeFieldsButtons(BOOL isActiveColumn)
{
	CTBToolBar* pT = GetToolBar(NAME_OBJECT_TOOLBAR);
	if (!pT)
		return;

	if (isActiveColumn)
	{
		pT->ChangeImage(ID_RS_REMOVE_LAYOUT_OBJECT, TBIcon(szIconRemoveColumn, TOOLBAR));
	}
	else
	{
		pT->ChangeImage(ID_RS_REMOVE_LAYOUT_OBJECT, TBIcon(szIconRemoveField, TOOLBAR));
	}
}

//-----------------------------------------------------------------------------
CPoint CWoormFrame::GetPositionSwitchTo()
{
	CPoint point(0, 0);
	if (!m_RSStatusBar || m_pStatusbarButtonSwitch == NULL)
	{
		//se il pulsante e' nella toolbar
		CRect r;
		GetWindowRect(r);

		int y = r.top + 30;

		if (GetDocument())
		{
			CView* pView = GetDocument()->GetFirstView();
			if (pView)
			{
				CRect rt;
				pView->GetWindowRect(rt);
				y = rt.top;
			}
		}

		CPoint point(r.right - 100, y);
		return point;
	}
	else
	{
		//se invece e' nella status bar
		CRect rect;
		m_pStatusbarButtonSwitch->GetWindowRect(rect);
		point.x = rect.left;
		point.y = rect.top;
		return point;
	}
}

//-----------------------------------------------------------------------------
void CWoormFrame::ShowDDLTemplates(CWoormDocMng* /*pDoc = NULL*/)
{
	if (!m_pDoc) return;

	m_pDoc->FillComboTemplates();
}

//-----------------------------------------------------------------------------
BOOL CWoormFrame::CreateDDLTemplates(CWoormDocMng* pDoc/* = NULL*/)
{
	ShowDDLTemplates(pDoc);
	
	return TRUE;
}

//-----------------------------------------------------------------------------
BOOL CWoormFrame::CreateSizeToolBar()
{
	ASSERT(m_pWSizeToolBar == NULL);
	if (m_pWSizeToolBar)
		return TRUE;

	m_pWSizeToolBar = new CTBToolBar();
	if (!m_pWSizeToolBar->CreateEmptyTabbedToolbar(this, NAME_SIZE_TOOLBAR, _TB("Size")))
	{
		TRACE(L"Failed to create Size toolbar\n");
		return FALSE;
	}
	m_pWTabbedToolBar->AddTab(m_pWSizeToolBar, FALSE);

	m_pWSizeToolBar->AddButton(ID_COL_LMOVE, RS_NS,		TBIcon(szIconMoveColLeft, TOOLBAR), _TB("Move column"));
	m_pWSizeToolBar->AddButton(ID_COL_RMOVE, RS_NS,		TBIcon(szIconMoveColRight, TOOLBAR), _TB("Move column"));
	m_pWSizeToolBar->AddSeparator();
	m_pWSizeToolBar->AddButton(ID_VK_SHIFT_UP, RS_NS,	TBIcon(szIconEnlargeTop, TOOLBAR), _TB("Enlarge object toward top"));
	m_pWSizeToolBar->AddButton(ID_VK_SHIFT_DOWN, RS_NS,	TBIcon(szIconNarrowTop, TOOLBAR), _TB("Narrow object from top"));
	m_pWSizeToolBar->AddButton(ID_VK_SHIFT_LEFT, RS_NS,	TBIcon(szIconNarrowRight, TOOLBAR), _TB("Narrow object from right"));
	m_pWSizeToolBar->AddButton(ID_VK_SHIFT_RIGHT, RS_NS, TBIcon(szIconEnlargeRight, TOOLBAR), _TB("Enlarge object toward right"));
	m_pWSizeToolBar->AddSeparator();
	m_pWSizeToolBar->AddButton(ID_VK_CTRL_UP, RS_NS,	TBIcon(szIconNarrowBottom, TOOLBAR), _TB("Narrow object from bottom"));
	m_pWSizeToolBar->AddButton(ID_VK_CTRL_DOWN, RS_NS,	TBIcon(szIconEnlargeBottom, TOOLBAR), _TB("Enlarge object toward bottom"));
	m_pWSizeToolBar->AddButton(ID_VK_CTRL_LEFT, RS_NS,	TBIcon(szIconEnlargeLeft, TOOLBAR), _TB("Enlarge object toward left"));
	m_pWSizeToolBar->AddButton(ID_VK_CTRL_RIGHT, RS_NS,	TBIcon(szIconNarrowLeft, TOOLBAR), _TB("Narrow object from left"));
	m_pWSizeToolBar->AddSeparator();
	m_pWSizeToolBar->AddButton(ID_VK_UP, RS_NS,			TBIcon(szIconArrowUp, TOOLBAR), _TB("Move up"));
	m_pWSizeToolBar->AddButton(ID_VK_DOWN, RS_NS,		TBIcon(szIconArrowDown, TOOLBAR), _TB("Move down"));
	m_pWSizeToolBar->AddButton(ID_VK_LEFT, RS_NS,		TBIcon(szIconArrowLeft, TOOLBAR), _TB("Move left"));
	m_pWSizeToolBar->AddButton(ID_VK_RIGHT, RS_NS,		TBIcon(szIconArrowRight, TOOLBAR), _TB("Move right"));
	m_pWSizeToolBar->SetAutoHideToolBarButton(FALSE);

	//-------------------------------
	m_pWBorderToolBar = new CTBToolBar();
	if (!m_pWBorderToolBar->CreateEmptyTabbedToolbar(this, NAME_BORDER_TOOLBAR, _TB("Border")))
	{
		TRACE(L"Failed to create Size toolbar\n");
		return FALSE;
	}
	m_pWTabbedToolBar->AddTab(m_pWBorderToolBar, FALSE);

	m_pWBorderToolBar->AddButton(ID_TOGGLE_BORDER_UP, RS_NS,	TBIcon(szIconBorderTop, TOOLBAR), _TB("Top border"));
	m_pWBorderToolBar->AddButton(ID_TOGGLE_BORDER_DOWN, RS_NS,	TBIcon(szIconBorderBottom, TOOLBAR), _TB("Bottom border"));
	m_pWBorderToolBar->AddButton(ID_TOGGLE_BORDER_LEFT, RS_NS,	TBIcon(szIconBorderLeft, TOOLBAR), _TB("Left border"));
	m_pWBorderToolBar->AddButton(ID_TOGGLE_BORDER_RIGHT, RS_NS,	TBIcon(szIconBorderRight, TOOLBAR), _TB("Right border"));
	m_pWBorderToolBar->AddButton(ID_TOGGLE_BORDER_ALL, RS_NS,	TBIcon(szIconBorderAll, TOOLBAR), _TB("All borders"));
	m_pWBorderToolBar->AddSeparator();
	m_pWBorderToolBar->AddButton(ID_SNAP_TO_GRID, RS_NS,		TBIcon(szIconMoveOnGrid, TOOLBAR), _TB("Move on grid"));
	m_pWBorderToolBar->AddButton(ID_TOGGLE_TRANSPARENT, RS_NS,	TBIcon(szIconTransparent, TOOLBAR), _TB("Transparent"));

	//---------------------------
	m_pWBorderToolBar->SetAutoHideToolBarButton(FALSE);
	return TRUE;
}

//-----------------------------------------------------------------------------
BOOL CWoormFrame::CreateAlignMoveToolBar(const CPoint& pt)
{
	ASSERT(m_pWAlignMoveToolBar == NULL);
	if (m_pWAlignMoveToolBar)
		return TRUE;

	m_pWAlignMoveToolBar = new CTBToolBar();
	if (!m_pWAlignMoveToolBar->CreateEmptyTabbedToolbar(this, NAME_ALIGNMOVE_TOOLBAR, _TB("Align move")))
	{
		TRACE(L"Failed to create Align move toolbar\n");
		return FALSE;
	}
	m_pWTabbedToolBar->AddTab(m_pWAlignMoveToolBar, FALSE);

	m_pWAlignMoveToolBar->AddButton(ID_ALIGN_HLEFT, RS_NS,			TBIcon(szIconAlignLeft, TOOLBAR),				_TB("Align left"));
	m_pWAlignMoveToolBar->AddButton(ID_ALIGN_HRIGHT, RS_NS,			TBIcon(szIconAlignRight, TOOLBAR),				_TB("Align right"));
	m_pWAlignMoveToolBar->AddButton(ID_ALIGN_VTOP, RS_NS,			TBIcon(szIconAlignTop, TOOLBAR),				_TB("Align top"));
	m_pWAlignMoveToolBar->AddButton(ID_ALIGN_VBOTTOM, RS_NS,		TBIcon(szIconAlignBottom, TOOLBAR),				_TB("Align bottom"));

	m_pWAlignMoveToolBar->AddButton(ID_ALIGN_HSPACE_EQUAL, RS_NS,	TBIcon(szIconEqualHorizontalSpacing, TOOLBAR),	_TB("Equal horizontal spacing"));
	m_pWAlignMoveToolBar->AddButton(ID_ALIGN_VSPACE_EQUAL, RS_NS,	TBIcon(szIconEqualVerticalSpacing, TOOLBAR),	_TB("Equal vertical spacing"));

	m_pWAlignMoveToolBar->AddButton(ID_LAST_LARGE, RS_NS,			TBIcon(szIconSameWidth, TOOLBAR),				_TB("Same width"));
	m_pWAlignMoveToolBar->AddButton(ID_LAST_HIGH, RS_NS,			TBIcon(szIconSameHeight, TOOLBAR),				_TB("Same height"));

	m_pWAlignMoveToolBar->AddButton(ID_ALIGN_STACK_LEFT, RS_NS,		TBIcon(szIconTileLeft, TOOLBAR),				_TB("Tile on the left"));
	m_pWAlignMoveToolBar->AddButton(ID_ALIGN_STACK_RIGHT, RS_NS,	TBIcon(szIconTileRight, TOOLBAR),				_TB("Tile on the right"));
	m_pWAlignMoveToolBar->AddButton(ID_ALIGN_STACK_TOP, RS_NS,		TBIcon(szIconTileTop, TOOLBAR),					_TB("Tile on top"));
	m_pWAlignMoveToolBar->AddButton(ID_ALIGN_STACK_BOTTOM, RS_NS,	TBIcon(szIconTileBottom, TOOLBAR),				_TB("Tile on bottom"));

	m_pWAlignMoveToolBar->AddButton(ID_ALIGN_CUT_H_LEFT, RS_NS,		TBIcon(szIconJustifyLeft, TOOLBAR),				_TB("Justify left"));
	m_pWAlignMoveToolBar->AddButton(ID_ALIGN_CUT_H_RIGHT, RS_NS,	TBIcon(szIconJustifyRight, TOOLBAR),			_TB("Justify right"));
	m_pWAlignMoveToolBar->AddButton(ID_ALIGN_CUT_V_TOP, RS_NS,		TBIcon(szIconJustifyTop, TOOLBAR),				_TB("Justify top"));
	m_pWAlignMoveToolBar->AddButton(ID_ALIGN_CUT_V_BOTTOM, RS_NS,	TBIcon(szIconJustifyBottom, TOOLBAR),			_TB("Justify bottom"));

	//-------------
	m_pWAlignMoveToolBar->SetAutoHideToolBarButton(FALSE);
	return TRUE;
}

//-----------------------------------------------------------------------------
BOOL CWoormFrame::CreateStatusBar()
{
	if (!m_bHasStatusBar)
		return TRUE;

	if (!m_RSStatusBar.Create(this) ||
		!m_RSStatusBar.SetIndicators(indicators, sizeof(indicators)/sizeof(UINT)))
	{
		TRACE(L"Failed to create status bar\n");
		return FALSE;      // fail to create
	}

	SetPaneInfo(ID_INDICATOR_PAGE,			SBPS_NORMAL, 70);
	SetPaneInfo(ID_INDICATOR_RUN,			SBPS_NORMAL, 120);
	SetPaneInfo(ID_INDICATOR_TRACK_RECT,	SBPS_NORMAL, 300);
	SetPaneInfo(ID_INDICATOR_MESSAGE,		SBPS_NORMAL, 300);
	SetPaneInfo(ID_INDICATOR_VOID,			SBPS_STRETCH, 0);
	
	if (!IsRadarFrame())
	{	
		if(AfxIsRSIconVisible(ID_EXTDOC_BACKTOMENU))
			m_pStatusbarButtonHome = AddButtonToPane(&m_RSStatusBar, ID_STATUS_BAR_HOME, ID_EXTDOC_BACKTOMENU, _T("Home"), AfxGetRSIcon(ID_EXTDOC_BACKTOMENU), _TB("Return to Home"));
		if (AfxIsRSIconVisible(ID_EXTDOC_SWITCHTO))
			m_pStatusbarButtonSwitch = AddButtonToPane(&m_RSStatusBar, ID_STATUS_BAR_SWITCH, ID_EXTDOC_SWITCHTO, _T("Switch"), AfxGetRSIcon(ID_EXTDOC_SWITCHTO), _TB("Switch Document"));
	}
	return TRUE;
}

//-----------------------------------------------------------------------------
void CWoormFrame::SetPaneInfo (UINT nId, UINT nStyle, int cwWidth)
{
	int idx =  m_RSStatusBar.CommandToIndex(nId);
	ASSERT(idx >= 0);
	if (idx < 0) return;
	m_RSStatusBar.SetPaneInfo(idx, nId, nStyle, cwWidth);
}

//-----------------------------------------------------------------------------
//Server in Windows XP (in Windows Seven è inutile)
void CWoormFrame::OnDropDownTemplate()
{ 
}

//-----------------------------------------------------------------------------
BOOL CWoormFrame::PreCreateWindow(CREATESTRUCT& cs)
{
	return CLocalizableFrame::PreCreateWindow(cs);
}

//-----------------------------------------------------------------------------
void CWoormFrame::OnUpdateFrameTitle(BOOL bAddToTitle)
{
	if ((GetStyle() & FWS_ADDTOTITLE) == 0)
		return;     // leave child window alone!

	CWoormDocMng* pDocument = GetDocument();
	if (bAddToTitle && pDocument != NULL)
	{
		CString strOld;
		GetWindowText(strOld);
		
		CString strText = pDocument->GetTitle();

		strText += _T(" - ");
		strText += cwsprintf(_TB("Company: {0-%s} - User: {1-%s} - Operation date: [{2-%s}]"), 
			AfxGetLoginInfos()->m_strCompanyName,
			AfxGetLoginInfos()->m_strUserName, 
			AfxGetApplicationDate().Str(1)
			);

		if (pDocument->m_bPlayback)
			strText += _TB(" (Playback)");
			
		if (m_nWindow > 0)
			strText = cwsprintf(_T("%s:%d"), strText, m_nWindow);

		// set title if changed, but don't remove completely
		if (strText != strOld)
		{
			SetWindowText(strText);
			// gestisce il titolo delle toolbar di tipo dockable
			if (m_pWToolBar && ::IsWindow(m_pWToolBar->m_hWnd))
				m_pWToolBar->SetWindowText(strText);
		}
		
		//comunico al menu che il titolo e' cambiato cosi` aggiorna il testo della tab
		SendTitleUpdatesToMenu();
	}
}

//-----------------------------------------------------------------------------
void CWoormFrame::SaveSettingsAndCleanUp(BOOL bRecalcLayout/* = TRUE*/)
{
	ASSERT_VALID(this);

	if (m_pWObjectToolBar && m_pWObjectToolBar->m_hWnd)
		ShowControlBar(m_pWObjectToolBar,	FALSE, FALSE, TRUE);
	if (m_pWSizeToolBar && m_pWSizeToolBar->m_hWnd)
		ShowControlBar(m_pWSizeToolBar,		FALSE, FALSE, TRUE);
	if (m_pWBorderToolBar && m_pWBorderToolBar->m_hWnd)
		ShowControlBar(m_pWBorderToolBar, FALSE, FALSE, TRUE);
	if (m_pWAlignMoveToolBar && m_pWAlignMoveToolBar->m_hWnd)
		ShowControlBar(m_pWAlignMoveToolBar, FALSE, FALSE, TRUE);

	if (bRecalcLayout) RecalcLayout();
}
//-----------------------------------------------------------------------------
LRESULT CWoormFrame::OnGetDocumentTitleInfo(WPARAM wParam, LPARAM lParam)
{
	CDocument* pDoc = GetActiveDocument();
	if (pDoc && pDoc->IsKindOf(RUNTIME_CLASS(CBaseDocument)))
	{
		TCHAR* pMsgBuff = (TCHAR*) wParam;
		UINT nSize = (UINT) lParam;
		
		//restituisco il testo della finestra, più la lunghezza del titolo
		//usato dal menu per sapere quanta parte del titolo finestra corrisponde al titolo documento
		//(il testo della finestra infatti inizia sempre col titolo del documento)
		CString strWindowText;
		GetWindowText(strWindowText);

		_tcscpy_s(pMsgBuff, nSize, strWindowText);
		return ((CBaseDocument*)pDoc)->GetTitle().GetLength();
	}
	return 0;
}

//-----------------------------------------------------------------------------
LRESULT CWoormFrame::OnGetNamespaceAndIcon(WPARAM wParam, LPARAM lParam)
{
	CDocument* pDoc = GetActiveDocument();
	if (pDoc && pDoc->IsKindOf(RUNTIME_CLASS(CBaseDocument)))
	{
		TCHAR* pMsgBuff = (TCHAR*) wParam;
		UINT nSize = (UINT) lParam;
		_tcscpy_s(pMsgBuff, nSize, ((CBaseDocument*)pDoc)->GetNamespace().ToString());
	}

	return GetClassLong(m_hWnd, GCL_HICON);
}

//-----------------------------------------------------------------------------
LRESULT CWoormFrame::OnGetSplittedString(WPARAM wParam, LPARAM lParam)
{
	CWoormDocMng* pDocument = GetDocument();
	ASSERT_VALID(pDocument);

	WoormField* pField = (WoormField*) lParam;
	ASSERT_VALID(pField);
	ASSERT_KINDOF(WoormField, pField);

	TableColumn* pCol = (TableColumn*)wParam;
	if (pCol)
	{
		ASSERT_VALID(pCol);
		ASSERT_KINDOF(TableColumn, pCol);
		ASSERT_VALID(pCol->m_pBitmap);
		ASSERT(pCol->m_bMultipleRow);

		int w = pCol->m_nWidth;
		int h = pCol->CellRect(0).Height();

		return pField->SplitImageToMultiline(pDocument, h, w, pCol->m_pBitmap->m_ImageFitMode) ? 1 : 0;
	}

	return pField->SplitStringOptimized(pDocument) ? 1 : 0;
}
