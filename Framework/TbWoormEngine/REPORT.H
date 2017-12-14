
#pragma once

#include <TbGeneric\Array.h>
#include <TbGeneric\schedule.h>
#include <TbGeneric\EnumsTable.h>
#include <TbGeneric\CMapi.h>

#include <TbGenlib\BaseFrm.h>
#include <TbGenlib\DMSAttachmentInfo.h>
#include <TbGenlibManaged\MExport.h>

#include "repwrkid.h"
#include "RepTable.h"
#include "RDEProtocolCode.h"

//includere alla fine degli include del .H
#include "beginh.dex"

class RepEngine;
class InputMng;
class WoormTable;
class RDEManager;
class Parser;
class FontStyle;
class WoormTable;
class CWinThread;
class ActionObj;

//============================================================================
//		class FieldWidthFactors
//============================================================================
class TB_EXPORT FieldWidthFactors
{
public:
	CWnd*		m_pWnd = NULL;
	CDC*		m_pDC = NULL;
	int			m_nWidth = 0;
	int			m_nHeight = 0;
	FontStyle*	m_pFontStyle = NULL;
	Formatter*	m_pFormatStyle = NULL;
	BOOL		m_bMiniHtml = FALSE;

	FieldWidthFactors () 
		: m_pWnd (NULL), m_pDC (NULL), m_nWidth (0), m_nHeight(0), m_pFontStyle (NULL) , m_pFormatStyle (NULL)
		{}
	~FieldWidthFactors() { if (m_pWnd && m_pDC) m_pWnd->ReleaseDC(m_pDC); }
};

//============================================================================
//		class CWoormDoc
//============================================================================
class CWoormFrame;
class CWoormInfo;
class EditorManager;
class CWoormView;
class CBCGPChartSeries;

enum ERefreshEditor { Layouts, Links, Variables, Rules, RuleGroup, Events, Procedures, Queries, Dialogs, ToolBox };

class TB_EXPORT CWoormDoc : public CBaseDocument
{
	friend class CAbstractWoormFrame;
	DECLARE_DYNCREATE(CWoormDoc)

public:
	WoormTable		m_ViewSymbolTable;

	BOOL			m_bUpdateOutputParametersEvenIfReportDoesNotFetchRecords = FALSE;
	BOOL			m_bSkipContext = FALSE;

	DataLng			m_dlCurrentPageEngine; //mapped on SpecialReportField::NAME.PAGE 
	DataStr			m_dsCurrentLayoutEngine; //mapped on SpecialReportField::NAME.LAYOUT
	DataLng			m_dlCurrentPageView; //mapped on SpecialReportField::NAME.PAGE 
	DataStr			m_dsCurrentLayoutView; //mapped on SpecialReportField::NAME.LAYOUT

	DataEnum		m_deReportStatus;

	BOOL			m_bCanPreviewPagesOnPrinting = TRUE; //because Report uses special field such as SPECIAL_TOT_PAGE or auto dynamic field ReportLastPageNumber

	DataBool		m_bPrintOnLetterhead = FALSE;
	DataInt			m_nCurrentCopyNumber = 1;

	CTypedBarcode	m_dsEABarCode;

	DataBool		m_bRunningInAThread = TRUE;

	DataBool		m_bHideAllAskDialogs = FALSE;

	BOOL			m_bAllowEditing = FALSE;

	CWoormInfo*		m_pWoormInfo = NULL;

	CString			m_strDocumentFilter;

	BOOL			m_bBreakpointsDisabled = FALSE;

	DataBool		m_bIsPrinting = FALSE;
	DataBool		m_bIsExporting = FALSE;
	DataBool		m_bIsArchiving = FALSE;

	DataBool		m_bIsFirstTuple = FALSE;
	DataBool		m_bIsLastTuple = FALSE;

protected:
	BOOL			m_bEngineRunning = FALSE;

	BOOL			m_bCanClose = TRUE;

	CString			m_sRunnedFilePath;
public:
	CWoormDoc ();
	virtual ~CWoormDoc	()	{}

	BOOL			IsEngineRunning()	 { return m_bEngineRunning; }
	virtual BOOL	IsReportRunning()	 { return m_bEngineRunning || m_bIsPrinting || m_bIsExporting; }

	void SetJsonResult(CJsonParser& json);
	void SetAuxDocumentFilter(const CString& strDocumentFilter) { m_strDocumentFilter = strDocumentFilter; }

	void SyncronizeViewSymbolTable (WoormTable* pSymTable);
	void SyncronizeViewSymbolTable (WoormField* pField);

	virtual BOOL 	IsAWoormRunningMultithread		() { return m_bRunningInAThread; }

	virtual int		CalculateFieldWidth		(int objectId, const CString& strText) { ASSERT(FALSE); return 0; }
	virtual BOOL	GetFieldWidthFactors	(int objectId, FieldWidthFactors& pWidthFactors, BOOL bSubTotal = FALSE) { ASSERT(FALSE); return FALSE; }
	virtual BOOL	UseOptimizedLineBreak	();
	virtual void	UpdateLayout			() { ASSERT(FALSE); };

	virtual BOOL	IsReportString		() const { ASSERT(FALSE); return FALSE; }
	virtual CString GetReportString		() const { ASSERT(FALSE); return CString(); }

	virtual void	AddViewField		(WoormField* pF, BOOL bDropDown = TRUE) { ASSERT(FALSE); };
	virtual void	RemoveViewField		(const CString& strName) { ASSERT(FALSE); };
	virtual BOOL	CanDeleteField		(LPCTSTR, CString&, BOOL* /*onlyUI*/ = NULL) const { ASSERT(FALSE); return FALSE; }

	virtual BOOL		ExistsFieldID	(WORD id, BOOL bSkipCurrentLayout) const { ASSERT(FALSE); return FALSE; }
	virtual BOOL		ExistsID		(WORD id, CString sLayoutName) const { ASSERT(FALSE); return FALSE; }
	virtual IBaseObj*	FindByID		(WORD id, CString sLayoutName) const { ASSERT(FALSE); return NULL; }
	virtual BOOL		ExistsColumnID	(WORD id, CString sLayoutName) const { ASSERT(FALSE); return FALSE; }
	virtual BOOL		ColumnShowedAsImage(WORD id, CString sLayoutName) const { return FALSE; }
	virtual CObject*	GetColumnPtr(WORD id, CString sLayoutName) const { return NULL; }

	virtual SymTable* GetSymTable() { return &m_ViewSymbolTable; }
	virtual WoormTable* GetEngineSymTable() { ASSERT(FALSE); return NULL; }
	virtual WoormTable* GetEditorSymTable() { ASSERT(FALSE); return NULL; }

	virtual DataBool Close() { ASSERT(FALSE); return FALSE; }

	virtual CWoormFrame*	GetWoormFrame	(BOOL bCheck = TRUE) const { ASSERT(FALSE); return NULL; }
	virtual CWoormView*		GetWoormView	(BOOL bCheck = TRUE) const { ASSERT(FALSE); return NULL; }

	virtual int			GetCurrentPage() { ASSERT(FALSE); return 0; }
	virtual BOOL		OnLinkSelectedFromWeb(int nAlias, int nRow) { ASSERT(FALSE); return FALSE; }
	virtual CString		GetReportSchemaString(const CString& forUser) { ASSERT(FALSE); return CString(); }

	virtual void		RunReport(BOOL bActivate = FALSE, BOOL bPost = TRUE) { ASSERT(FALSE); }

	virtual HTREEITEM	RefreshRSTree(ERefreshEditor, CObject* itemDataToSelect = NULL, BOOL selectRoot = FALSE) { ASSERT(FALSE); return NULL; }

	virtual void		OpenEditViewFullText(const CString& strErrors = CString(), int line = -1, int pos = -1) { ASSERT(FALSE); }

	virtual BOOL		ForceEngineParse() { ASSERT(FALSE); return FALSE; }

	//DMS-Barcode
	virtual void		ValorizeEABarCode() { ASSERT(FALSE); };
	virtual BOOL		CanShowEABarCode() { ASSERT(FALSE); return FALSE; };

	//-----
	virtual BOOL			IsHideFrame() const;

	virtual EditorManager*	GetEditorManager() const { ASSERT(FALSE); return NULL; }
	virtual void			PreDeleteField(LPCTSTR pszFieldName) { ASSERT(FALSE); }

	//-----
	virtual BOOL OpenDebugger	(ActionObj*) { ASSERT(FALSE); return FALSE; }
	virtual BOOL DoOpenDebugger (ActionObj*) { ASSERT(FALSE); return FALSE; }

	BOOL m_bDebugging = FALSE;

	//-----
	virtual DataArray* GetDataArrayFromId(WORD /*nID*/, DataArray*) { return NULL; }
	virtual BOOL FillSeries(CBCGPChartSeries* /*pSeries*/, WORD /*nColumnID*/)	{ return FALSE; }
	virtual BOOL FillSeries(CStringArray& /*pCategories*/, WORD /*nID*/)		{ return FALSE; }
	virtual BOOL FillSeries(CBCGPChartSeries* /*pSeries*/, WORD /*nID*/, WORD /*nID*/) { return FALSE; }

	virtual BOOL OnOpenDocument(LPCTSTR pszPathName);
			void AttachWoormInfoFromDocInvocationInfo();
			void AttachWoormInfo(CWoormInfo*);
};
//===========================================================================

typedef			DataBool (CObject::*BOOL_FUNC)(void) ;
typedef			void (CObject::*VOID_FUNC)(void) ;

//===========================================================================
class TB_EXPORT CAbstractWoormFrame : public CBaseFrame
{
	friend class CWoormDoc;

	DECLARE_DYNCREATE(CAbstractWoormFrame)
public:
	CAbstractWoormFrame() {}         

	CWoormDoc* GetDocument() { return (CWoormDoc*)GetActiveDocument(); }

	LRESULT OnGetComponent(WPARAM wParam, LPARAM lParam);
	void 	AssignWoormParameters(CString reportOutParameters);

	DECLARE_MESSAGE_MAP()
};
//-----------------------------------------------------------------------------
class TB_EXPORT CAbstractWoormView : public CBCGPScrollView
{
	DECLARE_DYNCREATE(CAbstractWoormView)
public:
	CAbstractWoormView() {}

	virtual void OnDraw(CDC*) {}
};

//============================================================================
//		class EngineScheduler
//============================================================================
class TB_EXPORT EngineScheduler : public Scheduler
{

public:
	enum MessageID 
	{
			EMPTY_MESSAGE = 0,
			QUERY_RULES_USER_BREAK,
			QUERY_RULES_UNK_FIELD_LEN,
			QUERY_RULES_TEMPORARY_CREATE,
			QUERY_RULES_TEMPORARY_ORINDEX,
			QUERY_RULES_TEMPORARY_GRINDEX,
			QUERY_EXP_RULE_BACKTRACK,
			QUERY_EXP_RULE_EVAL_EXP_ERR,
			QUERY_EXP_RULE_EVAL_FILT_ERR,
			QUERY_EXP_RULE_UNK_FIELD,
			QUERY_COND_RULE_BACKTRACK,
			QUERY_COND_RULE_EVAL_TEST_ERR,
			QUERY_COND_RULE_EVAL_THEN_ERR,
			QUERY_COND_RULE_EVAL_ELSE_ERR,
			QUERY_COND_RULE_EVAL_FILT_ERR,
			QUERY_COND_RULE_UNK_FIELD,
			QUERY_DATTBL_RULE_BACKTRACK,
			QUERY_DATTBL_RULE_READ_ERR,
			QUERY_DATTBL_RULE_WRITE_ERR,
			QUERY_DATTBL_RULE_UPDATE_ERR,
			QUERY_DATTBL_RULE_EVAL_WHERE_ERR,
			QUERY_DATTBL_RULE_EVAL_HAVING_ERR,
			QUERY_DATTBL_RULE_EVAL_JOINON_ERR,
			QUERY_DATTBL_RULE_UNK_FIELD,
			QUERY_DATTBL_RULE_OPEN_ERR,
			
			QUERY_EVAL_QUERY_FILTER,
			QUERY_EVAL_GROUP_EXPR,
			QUERY_EVAL_GROUP_FUNC,   

			REPORT_EVAL_EVENT_EXPR,
			REPORT_EVAL_EVENT_FUNC,
			REPORT_EVAL_COND_EXPR,
			REPORT_DISP_FIELD,
			REPORT_RDE_WRITE_NEWPAGE,
			REPORT_WRITE_TBL_CMD,
			REPORT_EVAL_INIT_EXPR,
			REPORT_ENGINE_TERMINATED,
			REPORT_INVALID_HELP_FILE,
			REPORT_EXEC_TABLE_ACTIONS,
			REPORT_INTERLINE_WHILE_RASTER_FIELDS ,
			REPORT_DISPLAY_WHILE_RASTER_FIELDS,
			
			REPORT_UNKNOWN_ACTION,
			REPORT_FIELD_NOT_COLUMN ,
			REPORT_ILLEGAL_PRECISION,
			REPORT_ILLEGAL_ALIAS,
			REPORT_UNKNOWN_DISPLAY_TABLE,
			REPORT_FIELD_EXIST,
			REPORT_DISPLAY_TABLE_ALREADY_DEFINED,
			REPORT_BAD_MESSAGE,
			REPORT_PROGRAM_NOT_LOADED,
			REPORT_ENGINE_ABORTED,
			REPORT_PROGRAM_RUNNING ,
			REPORT_PROGRAM_COMPILED,
			REPORT_RDE_MANAGER_NOTOPEN,

			REPORT_EVAL_WHEN_EXP,
			REPORT_EVAL_ON_EXP,
			REPORT_EVAL_ONABORTMESSAGE_EXP,
			REPORT_DIALOG_ONABORT_MESSAGE,
			REPORT_ABORT_MESSAGE,

			REPORT_AUTO_DISPLAY,
			REPORT_EMPTY_PROC,
			REPORT_EMPTY_DIALOG,

			QUERY_NOT_SAME_FIELD,
			QUERY_NOT_YET_IMPLEMENTED,
			QUERY_UNKNOWN_FUNCTION,
			QUERY_INCOMPATIBLE_FUNCTION_OPERAND,
   
			QUERY_RULES_RECURSIVE_ERR,
			QUERY_RULES_TUPLE_PROC_ERR,
			QUERY_RULES_NORULE_ERR,

			OSL_MISSING_GRANT,

			UNKNOWN_FIELD,
			UNKNOWN_DB_COL_FIELD,
			UNKNOWN_PROCEDURE,

			REPORT_UNATTENDED,

			DIALOG_MISSING,
			DIALOG_UNKNOWN,
			DIALOG_NOT_ON_ASK,

			REPORT_QUIT,
			REPORT_ACTION_ERR
	};

public:
	BOOL			m_bUseMultithreading;

	CObject*		m_pObject;
	BOOL_FUNC		m_BoolCommand;
	VOID_FUNC		m_VoidCommand;
	BOOL			m_CommandResult;

	MSG				m_HoldMsg;
	BOOL			m_bHoldingMsg;

	volatile bool	m_bExecuteCommandWorking;

	void			RunSQLCommand();
	BOOL			ExecuteSQLCommandRetBool(CObject*, BOOL_FUNC);
	void			ExecuteSQLCommandRetVoid(CObject*, VOID_FUNC);

private:
	MessageID		m_nErrorCode;
	CString			m_strErrorExpl;
	CString			m_strFailedName;
	ReportEngine*	m_pReportEngine;

public:
	EngineScheduler			 (ReportEngine* = NULL);
	virtual ~EngineScheduler ();

	void	SetRTError     		(MessageID nErrCod, const CString&  strErrIdx = _T(""), LPCTSTR = NULL);
	void	ResetRTError 	  	();
	CString	FormatRTError		();

	MessageID	GetRTErrorCode 		()	const { return m_nErrorCode; }
	CString	GetRTErrorExpl 		()	const { return m_strErrorExpl; }
	LPCTSTR	GetRTErrorFailedName()	const { return m_strFailedName; }

	BOOL	UserBreak			();
	void	LoopUntil			(volatile const short& nSemaphore, BOOL bFromRunEngine = FALSE, CTBToolBar* pTBToolBar = NULL);

	static CString FormatMessage(MessageID ID);
};

//============================================================================
//		class ReportEngine
//============================================================================
class TB_EXPORT ReportEngine : public CObject
{
	friend class RepEngine;
	friend class CWoormDocMng;
	friend class AskDialogInputMng;
	friend class EditorManager;

protected:
	CWoormDoc*			m_pCallerDoc;
	CString				m_strReportPath;
	CString				m_strHelpFile;
	EngineScheduler*	m_pScheduler;
	RepEngine*			m_pMainEngine;
	WoormTable*			m_pSymTable;
	BOOL				m_bRunning;
	BOOL				m_bCompiled;
	BOOL				m_bKilled;

	CStaticFieldArrayCache m_arStaticField;

public:
	//ReportEngine	(const CString& strSourcePath);
	ReportEngine	(CWoormDoc*);
	~ReportEngine	();

public:
	HWND				m_hComunicationWnd;

	void	AttachFrame		(CWnd* pWnd);
	void	AttachCallerDoc	(CWoormDoc* pDoc)	{ m_pCallerDoc = pDoc; if (m_pSymTable) m_pSymTable->SetDocument(pDoc); }

	RepEngine*	GetEngine	() { return m_pMainEngine; }
	WoormTable*	GetSymTable	() { return m_pSymTable; }

public:
	BOOL	Load	(const CString& strSourcePath);
	BOOL	Compile	(CString &errors, long &line,BOOL bMustFree = TRUE, BOOL bFullText = FALSE, CString sReportText = L"");
	BOOL	Run		(RDEManager*, Array* = NULL, CString* = NULL, BOOL bUsePrevAskInput = FALSE, CFunctionDescription* = NULL, WoormTable* = NULL);

	BOOL		IsRunning	()	{ return(m_bRunning); }
	BOOL		IsPaused	()	{ return(m_pScheduler->IsPaused()); }

	void	Pause			()	{ m_pScheduler->Pause(); }
	void	Resume			()	{ m_pScheduler->Resume(); }
	void	Reset			()	{ m_pScheduler->Start(); }
	void	Terminate		()	{ m_pScheduler->Terminate(); }
	void	ForceDetach		();
	void	Kill			()	{ m_bKilled = TRUE; Terminate(); }

	BOOL	OnPreparePrinting	();
	BOOL	OnBeginPrinting		(BOOL bPreview);
	BOOL	OnEndPrinting		();
	BOOL	OnAbortPrinting		();
	BOOL	OnCloseReport		();

	BOOL		IsRunningFromExternalController() const;
	BOOL		IsEditingParamsFromExternalController() const;

	CWoormDoc*	GetCallerDoc() { return m_pCallerDoc; }
	void		ClearStaticField() { m_arStaticField.RemoveAll(); }

	CString GetReportPath(){ return m_strReportPath; }

protected:
	void	FreeMemory		();

private:
	BOOL	RunningMessage	();
	BOOL	ParseAndBuild	(Parser&);
	BOOL	SkipGraphSection(Parser&);
	BOOL	ExecSpecialProc	(LPCTSTR);
};

//===============================================================================
class TB_EXPORT CExportInfo : public CObject
{
	DECLARE_DYNAMIC(CExportInfo);

	friend class CExportInfoDlg;
	friend class CExportData;

protected:
	BOOL	m_bSync = FALSE;
	
	BOOL	m_bCurrentPage = FALSE;
	BOOL	m_bAllPages = TRUE;
	int		m_nPagesNum = 0;
	int		m_nCurrPage = 0;
	int		m_nFromPage = 0;
	int		m_nToPage = 0;

	CString m_strCSVSep;
	CString m_strDelimStr;
	BOOL	m_bEncodeCSV = FALSE;
	BOOL	m_bisClipboard = FALSE;

	CString m_strFileName;
	CString m_strSheetName;
	int m_nOffsetRow = 0;
	int m_nOffsetCol = 0;

	BOOL m_bRepeatColumnTitles = FALSE;
	BOOL m_bColumnTitles = TRUE;
	BOOL m_bHiddenColumns = TRUE;

	BOOL m_bAutoSave = FALSE;
public:
	CString		m_sDateTypeFormat;
	CString		m_sDateTimeTypeFormat;
	CString		m_sTimeTypeFormat;

	BOOL		m_bMultiRows = FALSE;
	BOOL		m_bExportTotals = FALSE;
	BOOL		m_bExportSubTotals = FALSE;

	CLineFile::FileFormat m_FileFormat = CLineFile::FileFormat::UTF8;
	CString		m_strReportNameSpace;
	CString		m_strReportName;
	TypeOfExport m_nExportType = TypeOfExport::EXPORT_EXCELNET_TYPE;

	CArray<CPlaceHolderExportInfo*> m_arPlaceHolders;
public:
	CExportInfo();
	virtual 	~CExportInfo();

	CExportInfo& operator= (CExportInfo&);

	BOOL GetColumnTitles(){ return m_bColumnTitles; }
	BOOL GetHiddenColumns(){ return m_bHiddenColumns; }

	void SetColumnTitles(BOOL bColumnTitles) { m_bColumnTitles= bColumnTitles; }
	void SetHiddenColumns(BOOL bHiddenColumns) {  m_bHiddenColumns = bHiddenColumns; }

	void	SetPageRange(int nFrom, int nTo) 				{ m_nFromPage = (nFrom < nTo) ? nFrom : nTo; m_nToPage = (nFrom < nTo) ? nTo : nFrom; }
	void	SetCurrentPage(int nCurrPage)					{ m_nCurrPage = nCurrPage; }
	void	SetPagesNum(USHORT nPages) 					{ m_nPagesNum = nPages; }
	int		GetPagesNum() 								{ return m_nPagesNum; }

	void	SetFileName(const CString& strFileName)		{ m_strFileName = strFileName; }
	void	GetFileName(CString& strFileName)			{ strFileName = m_strFileName; }

	void	SetSheetName(const CString& strSheetName)		{ m_strSheetName = strSheetName; }
	void	GetSheetName(CString& strSheetName)			{ strSheetName = m_strSheetName; }

	void	SetCSVSep(const CString& strCSVSep)		{ m_strCSVSep = strCSVSep; }
	void	GetCSVSep(CString& strCSVSep)				{ strCSVSep = m_strCSVSep; }
	void	SetDelimStr(const CString& strDelimStr)		{ m_strDelimStr = strDelimStr; }
	void	GetDelimStr(CString& strDelimStr)			{ strDelimStr = m_strDelimStr; }
	void	SetEncodeCSV(BOOL bEncodeCSV) 				{ m_bEncodeCSV = bEncodeCSV; }
	BOOL	GetEncodeCSV() 								{ return m_bEncodeCSV; }

	void	SetAutoSave(BOOL bAutoSave) { m_bAutoSave = bAutoSave; }
	BOOL	GetAutoSave() { return m_bAutoSave; }

	void	SetClipBoard(BOOL bisClipboard) 					{ m_bisClipboard = bisClipboard; }
	BOOL	GetIsClipboard() 										{ return m_bisClipboard; }

	void	SetOffsetRow(const int nOffsetRow)			{ m_nOffsetRow = nOffsetRow; }
	int		GetOffsetRow()									{ return m_nOffsetRow; }
	void	SetOffsetCol(const int nOffsetCol)			{ m_nOffsetCol = nOffsetCol; }
	int		GetOffsetCol()									{ return m_nOffsetCol; }

	void			SetExportType(TypeOfExport nExpTyp)	{ m_nExportType = nExpTyp; }
	TypeOfExport	GetExportType()							{ return m_nExportType; }

	CString			GetExportFileExtension();

	// diagnostics
#ifdef _DEBUG
public:
	void Dump(CDumpContext& dc) const { ASSERT_VALID(this); AFX_DUMP0(dc, "CExportInfo\n"); }
	void AssertValid() const{ CObject::AssertValid(); }
#endif //_DEBUG
};

//===========================================================================

class TB_EXPORT CWoormInfo : public CFunctionDescription
{
	friend class CWoormDoc;
	friend class CWoormDocMng;
	friend class CWoormView;
	friend class CTbCommandManager;

	DECLARE_DYNAMIC(CWoormInfo)

private:
	int		m_nCopies;			// numero di copie da effettuare in  una sequenza multipla 
	BOOL	m_bLockedCopies;	// indica che il numero di copie è blindato
	int		m_nNextReport;		// indica il prossimo report da eseguire nella sequenza
	// se non faccio comparire la print dialog
	BOOL	m_bRunReport;		// indica se eseguire il report dopo l'apertura
	//handles to printer - used to mantain a scope during batch procedure which print n report in cascade
	HANDLE			m_hDevMode;
	HANDLE			m_hDevNames;
	BOOL			m_bMassiveOperation; // indica che si è in un'esecuzione multipla di report

public:
	// Contiene i nomi dei record da lanciare in cascata, deve contenere
	// almeno un nome (quello del primo report)
	CStringArray m_ReportNames;
	CStringArray m_arReportTitles;

	BOOL m_bPrintAborted;	// serve per segnalare che in una sequenza di report uno e` stato abortito.
	BOOL m_bPrinted;	// notify that printing was happended

	BOOL m_bNoPrintDialog;
	BOOL m_bOnePrintDialog;
	BOOL m_bPrintOnLetterHead;

	BOOL m_bMultiCopies;
	BOOL m_bShowPrintDialogBeforeRunning;
	CString	m_strPrinterName;

	BOOL m_bCloseOnEndPrint;// Chiude il report alla fine della stampa
	BOOL m_bAutoPrint;		// Inizia la stampa in automatico al lancio del report
	BOOL m_bIconized;
	BOOL m_bOwnedByReport;	// si deleta automaticamente dopo il run dell'ultimo record

	BOOL			m_bIsChildReport; //report chiamato da un altro report
	BOOL			m_bHideFrame;

	BOOL			m_bSendEmail;
	CMapiMessage	m_Email;
	BOOL			m_bErrorOnSendEmail;

	BOOL			m_bAttachRDE;
	BOOL			m_bAttachOther;
	BOOL			m_bAttachPDF;

	BOOL			m_bCompressAttach;

	BOOL			m_bUniqueMail;
	BOOL			m_bConcatPDF;

	BOOL			m_bRDEOutput;
	BOOL			m_bPDFOutput;
	BOOL			m_bPDFOutputPreview;
	BOOL			m_bShowUI2SendMail;

	CStringArray	m_arstrOutputFileNames;
	Array			m_arExportInfo;

	BOOL			m_bIsReportString		= FALSE;
	CString			m_sUICulture;

	//DMS
	BOOL			m_bArchivePdfFormat		= FALSE;
	BOOL			m_bArchived				= FALSE;
	CBaseDocument*	m_pDocumentAttachTo		= NULL;
	BOOL			m_bUseOutputFileName	= FALSE;

	CStringArray	m_arErrors;
	int				m_nLine = -1;
	int				m_nCol = -1;

	BOOL			m_bBlockReRun = FALSE;

	//TODO
	//int				m_nReRunTimer = 0;			
	//----
public:
	CWoormInfo();
	CWoormInfo(const CFunctionDescription&);
	CWoormInfo(const CWoormInfo&);

	///OCCHIO! Questo costruttore si comporta diversamente dagli altri per quanto riguarda l'inizializzazione
	//della variabile m_bOwnedByReport (che nasce a FALSE in questo caso)
	CWoormInfo(DataStr reportNamespace);

	virtual ~CWoormInfo();

#undef new
#undef delete

#ifdef _DEBUG
	void* operator	new		(size_t size, LPCSTR pszFile = NULL, int nLine = 0);
	void operator	delete	(void*, LPCSTR pszFile, int nLine);
#else
	void* operator	new		(size_t size);
#endif
	void operator	delete	(void*);

	void Init();

	BOOL IsLastReport() { return m_ReportNames.GetSize() == m_nNextReport; }
	void AddReport(DataStr reportNamespace);
	void SetNoPrintDialog(DataBool noPrintDialog);
	void SetCloseOnEndPrint(DataBool closeOnEndPrint);
	void SetAutoPrint(DataBool autoPrint);
	void SetIconized(DataBool iconized);
	void SetHideFrame(DataBool hideFrame);
	void SetSendEmail(DataBool sendEmail);
	void SetAttachRDE(DataBool attachRDE);
	void SetAttachPDF(DataBool attachPDF);
	void SetAttachOther(DataBool attachOther, DataStr expType);
	void SetCompressAttach(DataBool compressAttach);
	void SetOnePrintDialog(DataBool onePrintDialog);
	void SetUniqueMail(DataBool uniqueMail);
	void SetConcatPDF(DataBool concatPDF);
	void SetPDFOutput(DataBool PDFOutput);
	void SetPDFOutputPreview(DataBool PDFOutputPreview);
	void SetRDEOutput(DataBool RDEOutput);
	void SetExcelOutput(DataBool ExcelOutput);
	void SetExportType(DataInt exportType);
	void AddOutputFileName(DataStr fileName);

	void MailTo(DataStr to);
	void MailCc(DataStr to);
	void MailBcc(DataStr to);
	void MailAttach(DataStr sFile, DataStr sTitleFile);
	void MailSubject(DataStr subject);
	void MailBody(DataStr subject, DataBool bHtml);

	void	SetDevMode(HANDLE hDevMode);
	void	SetDevNames(HANDLE hDevNames);
	void	FreePrinterDeviceMode();
	void	FreePrinterDeviceNames();
	HANDLE	GetDevMode(){ return m_hDevMode; }
	HANDLE	GetDevNames(){ return m_hDevNames; };

	void SetMassiveOperation(BOOL massiveOperation);
	BOOL GetMassiveOperation();

	void SetCopies(int nCopies);
	BOOL AreCopiesLocked() const;
	void LockCopies(BOOL bLocked);

	void AddParameter(DataStr paramName, DataObj* refValue);
	void SetParamValue(DataStr paramName, DataObj* refValue);

	void SetUICulture(DataStr culture);

	void SetArchivePdfFormat(DataBool archive);

	void SetPrinterName(DataStr name);

	void SetExportOutputType(DataStr expType);

	void SetSilentMode(DataBool silent); //same as hideframe

	// diagnostics
#ifdef _DEBUG
public:
	void Dump(CDumpContext& dc) const { ASSERT_VALID(this); AFX_DUMP0(dc, " CWoormInfo\n"); }
	void AssertValid() const{ CObject::AssertValid(); }
#endif //_DEBUG
};


///////////////////////////////////////////////////////////////////////////////
//						Static Objects
///////////////////////////////////////////////////////////////////////////////

// Struct to manage buttons of Reporting Studio toolbar
// It permits to set icon of a button or to hide/show it
struct RSToolbarIcons {
	UINT m_nCmd;
	CString m_sIcon;
	BOOL m_bVisible;
} ;
  
//=============================================================================        
RSToolbarIcons* GetRSIcon(UINT commandID);

TB_EXPORT CString AFXAPI AfxGetRSIcon(UINT commandID);
TB_EXPORT BOOL AFXAPI AfxIsRSIconVisible(UINT commandID);
TB_EXPORT BOOL AFXAPI AfxSetRSIcon(UINT commandID, CString sIcon);
TB_EXPORT void AFXAPI AfxSetRSIconVisible(UINT commandID, BOOL isVisible = TRUE);
TB_EXPORT void AFXAPI AfxDeleteRSIconArray();

#include "endh.dex"
