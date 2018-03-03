#pragma once

#include <CRTDEFS.H>

#include <TbGeneric\Array.h>
#include <TbGeneric\critical.h>
#include <TbGeneric\FontsTable.h>
#include <TbGeneric\stack.h>
#include <TbGeneric\CMapi.h>

#include <TbNameSolver\TbWinThread.h>

#include <TbParser\Parser.h>
#include <TbGenlib\ExpParse.h>

#include <TbGenlib\Messages.h>
#include <TbGenlib\basedoc.h>
#include <TbGenlib\oslinfo.h>
#include <TbGenlib\funproto.h>
#include <TbGenlib\TBToolBar.h>

#include <TbNameSolver\MacroToRedifine.h>

#include <TbOledb\sqlobject.h>

#include <TbWoormEngine\ReportLink.h>
#include <TbWoormEngine\report.h>
#include <TbWoormViewer\pageinfo.h>

//casino a compilare #include "RSEditorUI.h"

//includere alla fine degli include del .H
#include "beginh.dex"

#define IDT_WOORM				200	// id del timer attivato da woorm
#define IDT_WOORM_AUTOSAVE		201 // id auto save timer
#define IDT_WOORM_RERUN			202 // id re run woorm

extern TCHAR* DEFAULT_CRYPT_PDF_PASSWORD;

// tipo degli oggetti per la gestione delle path relative

//===========================================================================
class CWoormView;
class ViewParser;
class ViewUnparser;
class BaseObj;
class SelectionRect;
class Array;
class FormatEntry;
class Options;
class WoormIni;
class ReportEngine;
class RDEManager;
class ActiveRect;
class EditorManager;	
class FormatStyleTable;
class FontStyleTable;
class Stack;
class InvalidArea;
class CWoormFrame;
class CBaseDocument;
class CExportData;
class DragRect;
class WoormLink;
class WoormLinkFilter;
class CDocProperties;
class AddOnApplication;
class CXSDGenerator;
class WoormTable;
class Table;
class CFindWordDlg;
class FieldRect;
class TableCell;
class BaseRect;
class CWoormTemplate;
class CToolCbx;
class CToolCbxStr;
class TableColumn;
class TblRuleData;
class MultiColumnSelection;
class CNodeTreeArray;
class CRSTreeCtrl;
class CNodeTree;
class SqlColumnInfo;
class TotalCell;
class CMyStatusBar;

//===========================================================================
class TB_EXPORT CLayout : public Array
{
public:
	CString			m_strLayoutName;
	CWoormDocMng*	m_pWoorm;
	int				m_nCountAutoObjects;
	BOOL			m_bInvertOrientation = FALSE;

	CLayout (const CString& strLayoutName, CWoormDocMng* pWDoc, BOOL bAddAutoSpecialFields = TRUE);
	CLayout (const CLayout& source);

	virtual CLayout* Clone() const { return new CLayout(*this); }

	// overloaded operator helpers
	::BaseObj*	operator[](int nIndex) const;
	::BaseObj*&	operator[](int nIndex);

	int					FindIndexByID		(WORD id);
	BaseObj*			FindByID			(WORD id);
	const TableColumn*	FindColumnByID		(WORD id);
	const BOOL			ExistsColumnID		(WORD id);
	Table*				FindTableByID		(WORD id = 0/*first*/);
	CObject*			FindObjectByID		(WORD id);

	BOOL CanDeleteField	(LPCTSTR pszFieldName, CString& sLog) const;
	void Unparse (ViewUnparser& ofile, BOOL bIsSavingTemplate, BOOL& bThereIsTemplate);

	void 	Paint			(CDC&, CPrintInfo*);
	void	Draw			(CDC&, BOOL bPreview);
	void	GetObjDescription	();
	void	ApplyRepeater	();
	void	MoveBaseRect	(int xOffset, int yOffset, BOOL bIgnoreBorder = FALSE);
	void	ClearDynamicAttributes ();
	void	DisableData		();

	BOOL	FindFreeRect	(int height, int width, CRect& rect);
protected:
	BOOL	FindFreeRect	(int width, CRect& rectTarget);
	//BOOL FindFreeRect (int width, CRect& rectTarget, int& nMaxBottom, int& nMinTop);
};                                        

typedef void (CWoormDocMng::*TFEnumObjects)  (CLayout*);
//===========================================================================

class TB_EXPORT CMultiLayouts : public CMapStringToOb
{
public:
	CWoormDocMng*		m_pDocument;

	CMultiLayouts			(CWoormDocMng* pDoc);
	virtual ~CMultiLayouts	() { RemoveAll(); }

	virtual void	RemoveAll	();
	void			EnumObjects	(TFEnumObjects pfAction);
	BOOL			SetCurrent	(CString sName);
	CLayout*		Add			(CString sName);
	BOOL			Lookup		(CString sLayoutName) const;
	BOOL			Lookup		(CString sLayoutName, CLayout*& pObject) const;

	BOOL			ExistsFieldID	(WORD id, BOOL bSkipCurrentLayout) const;
	BOOL			ExistsID		(WORD id, CString sLayoutName) const;
	BaseObj*		FindByID		(WORD id, CString sLayoutName) const;

	BOOL				ExistsColumnID	(WORD id, CString sLayoutName) const;
	const TableColumn*	FindColumnByID	(WORD id, CString sLayoutName) const ;

	BOOL			Rename		(CString sPrevName, const CString& sNewName);

	BOOL			CanDeleteField	(LPCTSTR pszFieldName, CString& sLog) const;
};                                        
// class array to avoid ripetitive cast
//===========================================================================
class TB_EXPORT CInvalidArray : public Array
{
public:
	// overloaded operator helpers
	InvalidArea* GetAt(int nIndex) const		{ return (InvalidArea*) Array::GetAt(nIndex); }
	InvalidArea* operator[](int nIndex) const	{ return (InvalidArea*) Array::GetAt(nIndex); }
	InvalidArea*& operator[](int nIndex)		{ return (InvalidArea*&) ElementAt(nIndex); }
};                                        

//=============================================================================
class TB_EXPORT InvalidArea : public CObject
{
	DECLARE_DYNAMIC (InvalidArea)
	
protected:
	BOOL	m_bRepaint;
	CRect	m_invalid;
	
public:
	InvalidArea (const CRect&, BOOL);
	
			BOOL	EraseBkg	()	const { return m_bRepaint; }
	const	CRect&	Invalid		()	const { return m_invalid; }
};

// contiene default personalizzabili dalle derivazioni successive
//=============================================================================
class TB_EXPORT DataDefaults
{
public:
	COLORREF	m_rgbColumnBorder;
	COLORREF	m_rgbColumnTitleBorder;
	COLORREF	m_rgbPageBkgn;
	BOOL		m_bUseAsRadar;
	
public:
	DataDefaults()
		:
		m_rgbColumnBorder		(RGB(0,0,0)),
		m_rgbColumnTitleBorder	(RGB(0,0,0)),
		m_rgbPageBkgn			(RGB(241, 241, 241)),
		m_bUseAsRadar			(FALSE)
	{}

};

//=============================================================================
class TB_EXPORT CWoormFileDialogPDF : public CFileDialog
{
	DECLARE_DYNAMIC(CWoormFileDialogPDF)

// Construction
public:
	CWoormFileDialogPDF
				(
					BOOL bOpenFileDialog, // TRUE for FileOpen, FALSE for FileSaveAs
					LPCTSTR lpszDefExt = _T("pdf"),
					LPCTSTR lpszFileName = NULL,
					DWORD dwFlags = OFN_HIDEREADONLY | OFN_OVERWRITEPROMPT,
					LPCTSTR lpszFilter = NULL,
					CWnd* pParentWnd = NULL
				);

// Attributes
public:

	BOOL	m_bCompress;
	BOOL	m_bPreview;

// Implementation
protected:
	virtual void OnInitDone();
	virtual BOOL OnFileNameOK();

	//{{AFX_MSG(CWoormFileDialog)
		// NOTE - the ClassWizard will add and remove member functions here.
	//}}AFX_MSG
	DECLARE_MESSAGE_MAP()
};

//=============================================================================
class TB_EXPORT CObArrayLockable : protected CObArray, public CTBLockable
{
public:
	CObArrayLockable(){}
	virtual ~CObArrayLockable() { RemoveAll(); }

	virtual INT_PTR Add (CObject*);

	virtual LPCSTR  GetObjectName() const { return "CObArrayLockable"; }

	virtual void RemoveAll ();
};

///////////////////////////////////////////////////////////////////////////////

class TB_EXPORT CWoormTemplate
{
public:
	CString			m_sNsTemplate;
	CWoormDocMng*		m_pWoormTpl;

	BOOL			m_bIsSavingTemplate;
	BOOL			m_bIsTemplate;
	BOOL			m_bLoaded;

	CWoormTemplate() : m_pWoormTpl (NULL), m_bIsSavingTemplate (FALSE), m_bIsTemplate (FALSE), m_bLoaded(FALSE) {}
	~CWoormTemplate();

	void Unload();

	static CString s_sCustom_StyleName;
	static CString s_sCopyStaticObject_StyleName;
	static CString s_sCopiedStaticObject_StyleName;
	static CString s_sInheritStaticObject_StyleName;
	static CString s_sEA_BarCode_StyleName;
};

//=============================================================================
class CNodeTree;

class TB_EXPORT CWoormDocMng : public CWoormDoc
{
	friend class CWoormView;
	friend class CWoormFrame;
	friend class CFindWordDlg; 
	friend class CRS_ObjectPropertyView;
	friend class CRSFileNameProp;
	friend class CRSImgOriginProp;
	friend class CRSSearchTbDialogProp;
	friend class CRSLayoutTemplateProp;
	friend class CRSImportStaticObjects;
	friend class CRSGroupFieldProp;		
	friend class CRSEditorDebugFrame;

	DECLARE_DYNCREATE(CWoormDocMng)

private:
	enum ObjectType
		{
			SQRRECT ,	TEXTRECT ,	GRAPHRECT ,	
			FILERECT ,	FIELDRECT ,	TABLE ,	
			METAFILERECT,	URLRECT , REPEATER , BARCODE , CHART, NONE 
		};
public:
	enum MSGSBType
	{
		MSG_SB_NORMAL, MSG_SB_SUCCESS, MSG_SB_INFO, MSG_SB_WARNING, MSG_SB_ERROR
	};

// private data
private:
	// indicator for compile and run status
	long	m_nRetrieveTic;
	long	m_nSearchTic;
	BOOL	m_bRunIndicatorActive;
	TCHAR	pchSortStatusString[4];
	BOOL	m_bReportComputing;
	BOOL	m_bIsNew; //mi dice se é un nuovo documento oppure uno esistente
	BOOL	m_bIsInParse;

	UINT	m_nTimerID;			//timer used for updating running status bar
	UINT	m_nTimerAutoSave;	// Auto save timer
	UINT    m_nReRunTimer;
	UINT	m_nReRun;
	BOOL	m_bAllowReRun = TRUE;

	BOOL	m_bIsUserReportsDeveloper = FALSE;

	CLineFile::FileFormat m_FileFormat;

	BOOL	m_bLoadingTemplate;
public:
	int		m_nCurrentLayer = 0;
	BOOL	m_bBetaFeatures = FALSE;
// private function
protected:
	void	SaveDocument		();

	BOOL	ParseHeadData		(ViewParser& lex);	
	BOOL	ParseProperties		(ViewParser& lex);
	BOOL	ParsePageInfo		(ViewParser& lex);

	void	UnparseHeadData		(ViewUnparser& ofile, BOOL bOnlyGraphInfo);
	void	UnParseOSLInfo		(ViewUnparser& ofile);
	void	UnparseProperties	(ViewUnparser& ofile);
	void	UnparsePageInfo		(ViewUnparser& ofile);
	void	UnparseLayouts		(ViewUnparser& ofile); 

	void	SetCursorTrackPoint		(CPoint);
	void	UpdateSpecialField		();
	void	SetPageIndicator		();
	void	SetRunIndicator 		();
	void	FileAutoSave			();
	void	ReRun					();
	void	PasteDIBitmap			();
	//void	PasteText				(BOOL bIsOemText = FALSE);
	BOOL	ExistObject				(BaseObj*);
	void	VKMoveObject			(CSize offset);
	void	VKSizeObject			(int increment, int direction);

	CString GetReportParametersKey	();
	
	virtual BOOL	CanShowInOpenDocuments	() { return TRUE; }

protected:	
	BOOL	IsDataLoaded			();
	BOOL	CanSaveReport			();
	BOOL	CanSaveReportData		();

protected:     
	// data member
	BOOL m_bOnlyGraphInfo;
		
protected:     
	BOOL	EngineStopped		();
	void	ExecuteNextReport	();

	void	SortObjectsOnPosition();
	void	SortObjectsOnPosition(CLayout* pObjects);

	void	ApplyRepeater		(CLayout* pObjects);

// per ottimizzazione della gestione dei font
public:     
	void	SortLayoutObjectsOnPosition();

	CFont*	m_pCurrentFont;
	CFont*	m_pOldFont;
	LOGFONT	m_LogFont;

	void	ResetCurrentFont(CDC& DC);
	CFont*	SetCurrentFont(LOGFONT* pLogFont, CDC& DC);
	
	CString GetFilePath(){ return m_strPathName; }

	virtual void SetModifiedFlag(BOOL bModified = TRUE);

	void	InvalidateStackedObjects();

//public data
public:
	// status flags
	BOOL	m_bReportChanged;
	BOOL	m_bViewChanged;
	BOOL	m_bPlayback;

	BOOL	m_bPrintingOnDefault;

	// trackin indicator related data public
	BOOL	m_bTrackRectIndicatorActive;
	BOOL	m_bTrackRowsIndicatorActive;
	CRect	m_TrackRect;
	WORD	m_wTrackRows;

	// general option
	Options*	m_pOptions;
	WoormIni*	m_pWoormIni;

	// report and RDE protocol related objects
	ReportEngine*	m_pEngine;
	RDEManager*		m_pRDEmanager;

	// generic public data
	int				m_nRelease;			// release con cui unparsero il report corrente
	int				m_nReportRelease;	// release number del report appena letto (se non nuovo)
	int				m_nModifyRelease;	// sub-release del report per la gestione degli outdated

	PageInfo		m_PageInfo;
	CWoormInfo*		m_pOldWoormInfo;
	WoormDocPtr		m_pChildReport;
	DataDefaults*	m_pDataDefaults;

	// creation related (strTmp used) data member
	ObjectType		m_Creating;			
	WORD			m_wCreatingId;
	CWordArray		m_wCreatingColumnIds;
	CString			m_strTextField;
	CString			m_strBmpName;
	CString			m_strTxtName;

	BaseObj*		m_pCurrentObj;

//MULTI LAYOUT
	CLayout*		m_Objects;	//current layout, tipicamente è quello chiamato "Default"
	CLayout&		GetObjects() { return *m_Objects; }
	CMultiLayouts	m_Layouts;	//mappa dei Layout

	Array			m_RemovedObj;
//SPLITTED LAYOUT
	CLayout*		m_pFullLayoutSplitted;	//salva m_Objects durante la stampa/preview delle splitter pages
	Array			m_arSplittedObjects;	//splitted objects pages del current layout
	int				m_nCurrentSplittedPage; //indice della splitted page visualizzata
	int				m_nSplittedPageMaxWidth;
//----

	CString			m_sStartUICulture;

	// rettangoli di selezione
	CInvalidArray*	m_pInvalidAreas;

	SelectionRect*	m_pMultipleSelObj;
	ActiveRect*		m_pActiveRect;		//sostituito da CWoormView::m_pTransActiveRect

	MultiColumnSelection*	m_pMultiColumns;		//contiene un array di ActiveRect
	CNodeTreeArray*			m_pNodesSelection;

	DragRect*		m_pDragRectObj;		// gestisce la multipla selezione by drag
	CPoint			m_ptCurrPos;
	//---------------------------

	CExportData*	m_pExportData;

	// for manage critical area of code (like readin page from strFile)
	CriticalArea	m_ReadingPage;

	// accessory 
	EditorManager*		m_pEditorManager;

	FormatStyleTable*	m_pFormatStyles;
	FontStyleTable*		m_pFontStyles;

	Stack*				m_pUndoStack;
	CDocProperties*		m_pDocProperties;
	
	BOOL				m_bAscendingOrderByColumn;

	WoormLinkArray		m_arWoormLinks;
	
	WoormDocPtr			m_pWaitingSubReport;
	BOOL				m_bMultiCopies;
	BOOL				m_bMustReRun;
	BOOL				m_bRunWithOutChild;
	BOOL				m_bCloseChildOnEndPrint;

	BOOL				m_bUsePrevAskInput;
	BOOL				m_bReRun;

	CMapiMessage		m_Email;
	BOOL				m_bAttachReportChild;
	BOOL				m_bDirectSendMail;
	BOOL				m_bDirectAttachRDE;
	BOOL				m_bDirectAttachPDF;
	BOOL				m_bDirectCompressAttach;
	BOOL				m_bDirectConcatPDF;

	BOOL				m_bNoWeb;

	BOOL				m_bSaveAsWithCurrentLanguage;
	CString				m_strOriginFileName;
	CString				m_strOriginDictionaryPath;

	BOOL				m_bNeedFinalInvalidate;
	BOOL				m_bReplaceHiddenWhenExpr;
	UINT				m_idcCounter;

	CFindWordDlg*		m_FindDlg;

	CWoormTemplate		m_Template;

	CString				m_sCulture_FIRST;
	CString				m_sCulture_LAST;
	BOOL				m_bCultureVariable;

	CString				m_sCopyText;
	BOOL				b_TransformColumnOrField;

	BOOL				m_bCallPostOpenWoormDoc = TRUE;

protected:
	CTBPicture*		m_pHiddenIcon;
	CTBPicture*		m_pHiddenWhenIcon;
	CTBPicture*		m_pAnchorIcon;
	CTBPicture*		m_pAnchorLeftIcon;
	CTBPicture*		m_pMailIcon;
	CTBPicture*		m_pLockedIcon;
	CTBPicture*		m_pRepeaterIcon;
	CTBPicture*		m_pRepeaterFieldIcon;
	CTBPicture*		m_pSplitterIcon;		//image draw in edit mode when column has option to force vertical break table on printing
	CTBPicture*		m_pPinnedIcon;			//image draw in edit mode when column has option to repeat column into breaked table block on printing

	CTBPicture*		LoadBmp				(int idb, CDC& DC);

public:
	CTBPicture*		GetHiddenIcon		(CDC& DC);
	CTBPicture*		GetHiddenWhenIcon	(CDC& DC);
	CTBPicture*		GetAnchorIcon		(CDC& DC);
	CTBPicture*		GetAnchorLeftIcon	(CDC& DC);
	CTBPicture*		GetMailIcon			(CDC& DC);
	CTBPicture*		GetLockedIcon		(CDC& DC);
	CTBPicture*		GetRepeaterIcon		(CDC& DC);
	CTBPicture*		GetRepeaterFieldIcon (CDC& DC);
	CTBPicture*		GetSplitterIcon		(CDC& DC);
	CTBPicture*		GetPinnedIcon		(CDC& DC);

// Constructors and destructor
public:
	CWoormDocMng();
	virtual ~CWoormDocMng(); 
		
	virtual void	AttachCallerDocument(CBaseDocument* pCallerDoc);

public:  
	CString			GetReportNamespace() const;

	virtual BOOL	IsReportRunning() ;

	virtual WoormTable* GetEngineSymTable();// { return m_pEngine ? m_pEngine->GetSymTable() : NULL; }
	virtual WoormTable* GetEditorSymTable();// { return m_pEditorManager && m_pEditorManager->GetPrgData() ? m_pEditorManager->GetPrgData()->GetSymTable() : NULL; }

	virtual CWoormFrame*	GetWoormFrame	(BOOL bCheck = TRUE) const;
	virtual CWoormView*		GetWoormView	(BOOL bCheck = TRUE) const;

	BaseObj*		SetCurrentObj	(CPoint, BOOL Pop = FALSE);
	BaseObj*		SetCurrentObj	(int nID, CPoint& pt);
	BaseObj*		SetCurrentObj	(BaseObj* pO, CPoint& pt);

	int		GetPosition		(int nID);
	
	TB_OLD_METHOD void	MDIActivate	()	{ Activate(); } //YOU SHOULD USE Activate()!!!
	virtual void	RunReport		(BOOL bActivate = FALSE, BOOL bPost = TRUE);

	void	AttachWoormInfo	(CWoormInfo*);

	void	MultipleSelect				(CRect includeRect, BOOL bAll);
	void 	SelectForExportData			(CPoint point, BOOL bSelForTitles = FALSE, BOOL bClickAsShift = FALSE);
	void	AdjustRowsExportSelection	(WORD , int);
	void	ShowValue					();

	// parse and deparse member function
	void	UnparseReport		(const CString& strTmp);
	void	UnparseWoorm		(const CString& strFile, BOOL bOnlyGraphInfo = FALSE);
	BOOL	ParseLayouts		(ViewParser& lex);
	BOOL	ParseObjects 		(ViewParser& lex, BOOL bWithPageLayout);
	BOOL	ParseViewInfo		(ViewParser& lex);
	BOOL	ParseWoormLinks (ViewParser& lex);

	BOOL	ForceEngineParse	();
	
	// RDEManager manipulation member function
	void	ReadNextPage			(BOOL = FALSE);
	void	ReadPrevPage			(BOOL = FALSE);
	void	ReadFirstPage			();
	void	ReadLastPage			();
	void	ReadSelectedPage		(int page, BOOL bUpdateWnd = TRUE);
	void	ReadPage 				(BOOL bUpdateWnd = TRUE);
	BOOL	ReadDataFromId			(BOOL& bPrepared);
	BOOL	ReadCommandFromId		();
	BOOL	ReadGeneralCommand		(BOOL bUpdateWnd = FALSE);
	BOOL	ReadArrayFields			(WORD wId);
	void	CheckEmailParameter		(FieldRect* p);

	void	InitializeDisplayStatus	();
	int		GetCurrentPage			();

	void	ExitCreatingMode		();
	BOOL	EditorManagerCreateField(int anID, TblRuleData* pTblRule = NULL);

	// currentObj method used in buttondown
	void	CreateListObj		(CDC&, CBCGPScrollView*, BOOL bSkipTrackPoint = FALSE);

	// gestione del rettangolo di Drag
	void CreateDragRect	(CDC& aDC, CBCGPScrollView* aView);
	void DeleteDragRect	();

	// creation object member function
	void	CreateField(int anID, TblRuleData* pTblRule = NULL);

	//void	CreateTableFromExpField		();
	//void	CreateTableFromHiddenField	();
	//void	CreateTableFromVarField		();

	// accesory functions
	void	MoveColumn		(BOOL);
	void	UpdateStatusBar	();

	// formatting member
	void	FormatAll		();

	// accessory member function
	BOOL	NoActiveColumn		() const;
	BOOL	MoreThanOneColumn	() const;
	BOOL	ActiveColumnHasTotal		() const;
	BOOL	ActiveColumnCanHaveTotal	() const;
	BOOL	CurrentIsTable		() const;

	// multiple selected objects action function 
	//void	AddObjects	();
	void	MoveObjects	();

	// generic function member
	BOOL	CheckTypeConsistence(CString& sError);
	BOOL	IsFilledWithData	();

	// background processing
	BOOL	RunEngine			(const CString& strFile);
	BOOL	PrepareRunEngine	(const CString& strFile);
	void	CompileEngine		(const CString& strFile);

	virtual void	OpenEditViewFullText(const CString& strErrors = CString(), int line = -1, int pos = -1);

	// cursor control and apparence  methods
	void ClipCursorToActiveView	();
	void SetAppropriateCursor	();
	
	// Invalidate and update simulation for multiple views
	void InvalidateRect	(const CRect& aRect, BOOL bRepaint= FALSE);
	void Invalidate		(BOOL bRepaint = TRUE);
	void UpdateWindow	();
	BOOL AlmostOneTrasparent();
	
public:
	void		UpdateViewSymbolTable();
	void		UpdateViewSymbolTable(BaseObj* pTableOrRepeater, int nRow, BOOL updateOnlyTailCell = FALSE);

	BOOL		GetAliasObjectFromPoint(int& nAlias, int& nRow, BaseObj*& pTableOrRepeater, BaseRect*& pField, CPoint* pt = NULL);
	RDEData*	GetRDEDataFromAlias(int nA, int nRow = -1);
	BOOL		GetObjectFromAlias(int nA, int nRow, FieldRect*& pField, TableCell*& pCell);

	BOOL		EvalLinkParameters(CFunctionDescription* pFD, WoormLink*, WoormTable*, int nRow);
	WoormLink*	GetEnableLink(BaseObj* pTableOrRepeater, int nAlias, int nRow, BOOL bUpdateSymTable = TRUE);
	BOOL		OnLinkSelected();
	CString		OnMouseHoverLink();
	BOOL		IsMouseHoverLink(CString* sGetToolTip = NULL);
	//BOOL		ThereIsWoormLink(int nAlias);
	
	BOOL		OnLinkSelectedFromWeb(int nAlias, int nRow);
	BOOL		DoBrowseLink(BaseObj* pTableOrRepeater, FieldRect* pField, int nAlias, int nRow);
	//------------------------
	virtual	void		OnFrameCreated	();

	//invio email
	BOOL DoSendMail (BOOL bAuto, BOOL bAttachRDE, BOOL bAttachPDF, BOOL bCompressAttach, BOOL bRequestDeliveryNotification, BOOL bRequestReadNotification, BOOL bAttachOther, BOOL bExpandMailTemplate = TRUE);
	BOOL DoAttachReport (BOOL bAuto, BOOL bAttachRDE, BOOL bAttachPDF, BOOL bCompressAttach, BOOL bAttachOther = FALSE);

	// gestione delle sessioni di lavoro
	virtual ::SqlSession*		GetReadOnlySqlSession	() const;
	virtual ::SqlSession*		GetUpdatableSqlSession  () const;
	virtual ::SqlConnection*	GetSqlConnection		() const;

	void	SetDecorateTitle				();

	CString GetReportSchemaString	(const CString& forUser);
	CString GetSchemaString			(LPCTSTR pszTargetUser = NULL);
	void	GetSchema				(CString strFile);
	void	GetSchema				(
										CPathFinder::PosType ePosType, 
										const CString& strUser, 
										const CTBNamespace& ns, 
										CXSDGenerator*& XMLSchema
									);

	BOOL	GetAskDialogsParameters(CArray<CExportDataNetAskDialog*> & m_arAskDialogs);

	void ClearDynamicAttributes(CLayout* ar);
	// funzione per la FindWord
	BOOL FindWord (CString strFindWord,  BOOL bIsDown);
	BOOL IsFindWordOnSingleColumn ();
	void ClearFindCellArray(CLayout* pObjects);

	BOOL		SetTotalRectActiveById	(int nID);
	TotalCell*	GetTotalCellById		(int nID);

	void	SetMessageInStatusBar(CString message, MSGSBType eMsgType = MSG_SB_NORMAL);
	void	ClearMessageInStatusBar();

private:
	void SetCulture(CString strLanguage);
	void RestoreCulture();

private:
	void SetCursor						(int nCursor);

	BOOL DoPDFOutput					(CString* psFile = NULL);
	BOOL DoRDEOutput					();
	BOOL DoExportOutput					();
	BOOL DoArchivePdfFormat				();

	BOOL DoPostaLiteOutput				(CMapiMessage*);
	BOOL PostaLiteCheckFile				(CMapiMessage*);

	void ConcatPDFAndShowWhenRequired	(CString* psFile = NULL);
	BOOL ConcatPDF						(const CString strOrigin, const CString strDestination);
	BOOL ConcatPDFAttachments			(const CString strDestination);

	BOOL ReplaceEmailParameters			();
	BOOL AddEmailAttachments
										(
											const CString& sAttachmens,
											const CString& sTitles,
											BOOL bAppend
										);

	void GetSchemaObjects				(CXSDGenerator*, WoormTable*);

	BOOL PrepareLayout					();
	virtual void UpdateLayout			();

	//Integrazione con il DMS
	BOOL ArchivePdfFormat(BOOL bShowMsg = TRUE);
	BOOL GeneratePapery(BOOL bShowMsg = TRUE);
	void	ObjectUndo	(BaseObj * pObject);
public:
	virtual BOOL	InitDocument		();
	virtual void	DeleteContents		();
	virtual BOOL    OnNewDocument		();
	virtual BOOL    OnOpenDocument		(LPCTSTR pszPathName);
	virtual BOOL    ParseReport(CWoormInfo*, CString strReport, BOOL bIsReportString = TRUE);
	virtual BOOL	OnSaveDocument		(LPCTSTR pszPathName);
	virtual BOOL	PrepareFileNameForSave(CString& sPathName);
	virtual void	PreCloseFrame		(CFrameWnd* /*pFrameArg*/);
	virtual void	SetPathName(LPCTSTR lpszPathName, BOOL = TRUE);
	virtual void	SetReportPathName(LPCTSTR lpszPathName, BOOL = TRUE);
	 
	virtual void	OnCloseDocument	();
	virtual BOOL	CanCloseDocument();

	virtual BOOL	DispatchFunctionCall	(CFunctionDescription*);
protected:
			BOOL		DispatchFunctionCallSync	(CFunctionDescription* pRDI, const CString& strF);
			BOOL		DispatchFunctionCallAsync	(CFunctionDescription* pRDI, const CString& strF);
			DataBool	DispatchFunctionCallAsyncWrp	(DataLng dlRDI, DataStr strF);
public:
	// passano di default il contesto del documento
	virtual Formatter*	GetFormatter (const FormatIdx& nIdx)	const;
	virtual Formatter*	GetFormatter (const CString& strStyle)	const;
	virtual Formatter*	GetFormatter (const DataType& aType)	const;

	// debug methods          
protected:
#ifdef _DEBUG
	virtual void AssertValid() const;
	virtual void Dump(CDumpContext& dc) const;
#endif

public:
	BOOL DoSave(LPCTSTR pszPathName, BOOL bReplace = TRUE);
	virtual BOOL SaveModified(); // return TRUE if ok to continue
	virtual HMENU GetDefaultMenu(); // get menu depending on state

protected:
	BOOL DoSave2(LPCTSTR pszPathName, BOOL bReplace, const CString& newName, BOOL bSetNs);

	virtual int CalculateFieldWidth		(int objectId, const CString& strText);
	virtual BOOL GetFieldWidthFactors	(int objectId, FieldWidthFactors& fieldWidthFactors, BOOL bSubTotal = FALSE);
	virtual BOOL UseOptimizedLineBreak	();

	BOOL CanDoCutObjects();
	BOOL CanDoMoveObjects();
	BOOL CanDoSizeObjects();
	BOOL CanDoToggleTransparent();
	BOOL CanDoSnapObjects();

	virtual BOOL CanDoExportData (UINT nID);
	virtual BOOL CanDoSendMail ();
	virtual BOOL CanDoFileSavePDF ();

	CString			GetAlternativeNameForEA	() const;
	void			AddEABarCodeObject		(CLayout* pObjects);
	BaseRect*		CreateBarcodeAtPosition	(LONG X, LONG y, BOOL bVert= FALSE);
	void			ValorizeEABarCode		(const CString strAlternativeName, BOOL bGenerateNewBarcode);
	virtual void	ValorizeEABarCode		();
	virtual BOOL	CanShowEABarCode		();

public:
	void ChangeLayout(const CString& sName);

	BOOL CanDoAllowEditingButton	(BOOL bPrepare = FALSE);
	BOOL CanDoEditReport			();

	void AnchorFieldToColumn(CLayout* pObjects);
	void AnchorFieldToColumn(CLayout* pObjects, int nStartIndex);
	
	BOOL SplitCurrentLayout();
	void RecalcLayoutSplittedPage(int nSplittedPage);
	void RecalcLayoutSplittedPage(int nSplittedPage, Table*);
	void RecalcLayoutSplittedPage(int nSplittedPage, BaseRect*);
	LONG GetPosXofAnchoredRect	 (BaseRect* pBaseRect, BOOL bLeft);
	void RestoreCurrentLayout();
	void CalcSplittedPageMaxWidth();
	int GetNumberOfSplittedPage() const { return m_arSplittedObjects.GetSize(); }
	int GetCurrentSplittedPage() const { return m_nCurrentSplittedPage; }
	int GetSplittedPageMaxWidth() const { return m_nSplittedPageMaxWidth; }
	BOOL SetFirstSplitterPage();
	BOOL SetLastSplitterPage();
	BOOL SetNextSplitterPage();
	BOOL SetPrevSplitterPage();
	BOOL AllowNextSplitterPage();
	BOOL AllowPrevSplitterPage();
	int	 GetTableCount();
	CList<Table*>* GetTables();
	Table*	GetTableFromColumnId(WORD columnId);

	void DoSnapToGrid();

	BOOL CheckReportData (CWoormDocMng* pTesterWDoc, CStringArray& arMsgs);
	BOOL CheckReportCompareData
		(
			CWoormDocMng* pTesterWDoc, 
			CStringArray& arMsgs,
			TableCell* pTestCell, 
			FieldRect* pTestField, 
			int page, 
			int nA,
			int nRow = -1
		);

	void FindWordClear();

	COLORREF	GetObjectSelectionColor();
	void		RemoveFocusFromBtn();
	void		SetAutoSaveTimer();

	void		SetReRunTimer(UINT nTime);
	BOOL		StartReRunTimer();
	BOOL		StopReRunTimer();

	FIND_BAR_METHOD()

public:
	afx_msg	void OnObjectCut();

// Generated message map functions
protected:
	afx_msg void OnFileNew			();
	afx_msg void OnFileOpen			();
	afx_msg void OnFileSave			();
	afx_msg void OnFileSaveAs		();
	afx_msg	void OnFileSaveRDE		();
	afx_msg	void OnSendMail			();
			BOOL OnSendMailForIMago ();
	afx_msg	void OnSendPostaLite	();
	afx_msg	void OnFileSavePDF		();

	afx_msg void OnAllowEditing		();
	afx_msg void OnShowEditToolbars	();
	afx_msg void OnPageSetup		();
	afx_msg void OnFileProperties	();

	afx_msg void ToggleSaveOnlyGraphInfo();
	
	afx_msg	void OnObjectCopy		();
	afx_msg	void OnObjectPaste		();
	afx_msg	void OnObjectUndo		();
	afx_msg	void OnClearAll			();
	afx_msg	void OnSelectAll		();
	afx_msg	void OnDeselectAll		();

	// message function handler for vertical scroll handling
	afx_msg	void OnFirstPage		();
	afx_msg	void OnLastPage			();
	afx_msg	void OnNextPage			();
	afx_msg	void OnPrevPage			();
	// message generated from m_pEngine signaling new page available
	afx_msg	void OnNewPage			();

	// on command function member
	afx_msg	void OnExportData		(UINT nID);

	afx_msg	void OnRun				();
	afx_msg	void OnStop				();
	afx_msg	void OnCompile			();
	afx_msg	void OnPause			();
	afx_msg	void OnResume			();
	afx_msg	void OnGotoPage			();
	afx_msg	void OnRunStop			();
	afx_msg	void OnPauseResume		();
	afx_msg	void OnRunDebugger		();

	afx_msg void OnActionsCopy		();
	afx_msg void OnXmlActionsCopy	();
			BOOL PrepareParamsForReRun();

	afx_msg	void OnReInitHiddenInput ();

	afx_msg	void OnRunWithOutChild	();
	afx_msg	void OnRunAndAttachChild();

	afx_msg	void OnPrintWithChild	();
	afx_msg	void OnPrintMultiCopies	();
	afx_msg	void OnPrintWithDialog	();
	afx_msg	void OnTogglePrintOnLetterhead();
	
	afx_msg	void OnArchivePdfFormat	();
	afx_msg	void OnArchiveWithChilds();
	afx_msg void OnGeneratePapery();
	
	afx_msg	void OnAddSqrRect		();
	afx_msg	void OnAddTextRect		();
	afx_msg	void OnAddGraphRect		();
	afx_msg	void OnAddFileRect		();
	afx_msg	void OnAddRepeater		();
	afx_msg	void OnAddChart			();

	afx_msg	void OnAddTable();
		afx_msg	void OnAddTableFromDB();
		afx_msg	void OnAddTableFromFuncExpr();
		afx_msg	void OnAddTableFromHiddens();

	afx_msg void OnCreateDbObj();

	afx_msg void OnCreateFuncExprObj();

	afx_msg void OnCreateObjFromHiddenField();

	afx_msg	void OnColLeftMove		();
	afx_msg	void OnColRightMove		();
	afx_msg	void OnColAddTotal		();
	afx_msg	void OnColDelete		();

	afx_msg	void OnMulSelAlign		();
	afx_msg	void OnMulSelSize		();
	afx_msg	void OnOptions			();
	afx_msg	void OnWoormIni			();

	afx_msg	void OnFontStyles		();
	afx_msg	void OnFormatStyles		();

	afx_msg	void OnEnumsViewer		();
	afx_msg void OnUpdateEnumsViewer(CCmdUI* pCmdUI);
	
	afx_msg void OnUpdateTools		(CCmdUI* pCmdUI);
	
	afx_msg void OnGeneralSettings();
	afx_msg void OnUpdateGeneralSettings(CCmdUI* pCmdUI);
	
	afx_msg void OnProperties();
	afx_msg void OnUpdateProperties(CCmdUI* pCmdUI);

	afx_msg void OnPageInfo();
	afx_msg void OnUpdatePageInfo(CCmdUI* pCmdUI);

	//afx_msg	void OnEntries			();
	//afx_msg	void OnEvents			();
	//afx_msg	void OnProcedures		();
	//afx_msg	void OnOrderBy			();
	//afx_msg	void OnAskRules			();
	afx_msg	void OnShowReportParameters	();
	//afx_msg	void OnShowObjectsList		();
	//afx_msg	void OnSubTotals		();
	//afx_msg	void OnQueries			();

	afx_msg void OnReportBegin		();
	afx_msg void OnReportEnd		();
	afx_msg void OnBeginSort		();
	afx_msg void OnEndSort			();
	afx_msg void OnBeginGroup		();
	afx_msg void OnEndGroup			();
	afx_msg void OnBeginMemGroup	();
	afx_msg void OnEndMemGroup		();
	afx_msg void OnSearchTic		();
	afx_msg void OnRetrieveTic		();
	
	afx_msg	void OnVKLeft			();
	afx_msg	void OnVKRight			();
	afx_msg	void OnVKUp				();
	afx_msg	void OnVKDown			();

	afx_msg	void OnVKShiftLeft		();
	afx_msg	void OnVKShiftRight		();
	afx_msg	void OnVKShiftUp		();
	afx_msg	void OnVKShiftDown		();

	afx_msg	void OnVKCtrlLeft		();
	afx_msg	void OnVKCtrlRight		();
	afx_msg	void OnVKCtrlUp			();
	afx_msg	void OnVKCtrlDown		();

	afx_msg	void OnVKTab			();
	afx_msg	void OnVKShiftTab		();
	afx_msg	void OnVKReturn			();
   
	afx_msg	void OnSnapToGrid		();
	afx_msg	void OnToggleTransparent();

	afx_msg	BOOL OnToggleBorder		(UINT nID);
	afx_msg BOOL OnAlignmentBar		(UINT nID);

	afx_msg void OnUsePrevAskInput		 ();
	afx_msg void OnUpdateUsePrevAskInput (CCmdUI* pCmdUI);

	afx_msg void OnReRunStartStop		();
	afx_msg void OnUpdateReRunStartStop (CCmdUI* pCmdUI);
	afx_msg void OnUpdateComboReRun(CCmdUI* pCmdUI);
	
	afx_msg void OnToggleCreateSchema1		 ();
	afx_msg void OnToggleCreateSchema2		 ();
	afx_msg void OnUpdateCreateSchema1 (CCmdUI* pCmdUI);
	afx_msg void OnUpdateCreateSchema2 (CCmdUI* pCmdUI);

	afx_msg void OnFindWord		();
	afx_msg void OnFindPrev		();
	afx_msg void OnFindNext		();
	afx_msg void OnFilePrintPreview();

	afx_msg void OnTabSwitch();
	afx_msg void OnTabSwitchRange(UINT nID);

	afx_msg void OnUpdateActionsCopy	(CCmdUI* pCmdUI);
	afx_msg void OnUpdateFileNew		(CCmdUI* pCmdUI);
	afx_msg void OnUpdateFileOpen		(CCmdUI* pCmdUI);
	afx_msg void OnUpdateFileSave		(CCmdUI* pCmdUI);
	afx_msg void OnUpdatePageSetup		(CCmdUI* pCmdUI);
	afx_msg void OnUpdateFileProperties	(CCmdUI* pCmdUI);	
	afx_msg void OnUpdateClearAll		(CCmdUI* pCmdUI);
	afx_msg void OnUpdateCopy			(CCmdUI* pCmdUI);
	afx_msg void OnUpdatePaste			(CCmdUI* pCmdUI);
	afx_msg void OnUpdateUndo			(CCmdUI* pCmdUI);
	afx_msg	void OnUpdateExportData		(CCmdUI* pCmdUI);
	afx_msg	void OnUpdateExportDropDown	(CCmdUI* pCmdUI);
	afx_msg void OnUpdateRun			(CCmdUI* pCmdUI);
	afx_msg void OnUpdateStop			(CCmdUI* pCmdUI);
	afx_msg void OnUpdatePause			(CCmdUI* pCmdUI);
	afx_msg	void OnUpdateRunStop		(CCmdUI* pCmdUI);
	afx_msg	void OnUpdatePauseResume	(CCmdUI* pCmdUI);
	afx_msg void OnUpdateResume			(CCmdUI* pCmdUI);
	afx_msg void OnUpdateGotoPage		(CCmdUI* pCmdUI);
	afx_msg void OnUpdateMovePage		(CCmdUI* pCmdUI);
	afx_msg void OnUpdateFileSaveRDE	(CCmdUI* pCmdUI);
	afx_msg void OnUpdateSendMail		(CCmdUI* pCmdUI);
	afx_msg void OnUpdateFileSavePDF	(CCmdUI* pCmdUI);
	afx_msg void OnUpdateAddRect		(CCmdUI* pCmdUI);
	afx_msg void OnUpdateAddTableOrFields	(CCmdUI* pCmdUI);
	afx_msg void OnUpdateAddColumns		(CCmdUI* pCmdUI);
	afx_msg void OnUpdateAddLink		(CCmdUI* pCmdUI);
	afx_msg void OnUpdateSelectAll		(CCmdUI* pCmdUI);
	afx_msg void OnUpdateSelection		(CCmdUI* pCmdUI);
	afx_msg void OnUpdateOptions		(CCmdUI* pCmdUI);
	afx_msg void OnUpdateWoormOptions	(CCmdUI* pCmdUI);
	afx_msg void OnUpdateEditMode		(CCmdUI* pCmdUI);
	afx_msg void OnUpdateShowReportParameters (CCmdUI* pCmdUI);
	afx_msg void OnUpdateShowReportObjects (CCmdUI* pCmdUI);
	afx_msg void OnUpdateMakerMode		(CCmdUI* pCmdUI);
	afx_msg void OnUpdateFindWord		(CCmdUI* pCmdUI);
	afx_msg void OnUpdateTabSwitch			(CCmdUI* pCmdUI);
	afx_msg void OnUpdateAllowEditingButton	(CCmdUI* pCmdUI);
	afx_msg void OnUpdateShowEditToolbars	(CCmdUI* pCmdUI);
	afx_msg void OnUpdateSaveOnlyGraphInfo	(CCmdUI* pCmdUI);
	afx_msg void OnUpdateTrackRectIndicator (CCmdUI* pCmdUI);
	afx_msg	void OnUpdateFilePreviewPrint(CCmdUI* pCmdUI);
public:
	afx_msg void OnUpdateCut(CCmdUI* pCmdUI);
	afx_msg void OnUpdateColMove(CCmdUI* pCmdUI);
	afx_msg void OnUpdateAlignmentBar		(CCmdUI* pCmdUI);
protected:
	afx_msg void OnEasyReadingByColor			();
	afx_msg void OnEasyReadingByColorDynamic	();
	afx_msg void OnEasyReadingByLine			();
	afx_msg void OnEasyReadingByLineDynamic		();
	afx_msg void OnUpdateEasyReading			(CCmdUI* pCmdUI);

	afx_msg void OnPostOpenWoormDoc			(BOOL bRunReport);

	afx_msg void OnRecordValidation			();
	afx_msg void OnUpdateRecordValidation	(CCmdUI* pCmdUI);

	afx_msg void OnCopyText					();

	afx_msg void OnToggleConcatPdf			();
	
	afx_msg void OnOpenAsText				();
	afx_msg void OnUpdateOpenAsText			(CCmdUI* pCmdUI);

	afx_msg void OnAddEABarcode				();
	afx_msg void OnUpdateAddEABarcode		(CCmdUI* pCmdUI);

	afx_msg void OnFileSaveAsTemplate		();
	afx_msg void OnLoadTemplate				();
	afx_msg void OnUnloadTemplate			();
	afx_msg void OnUpdateLoadTemplate		(CCmdUI* pCmdUI);
	afx_msg void OnUpdateUnloadTemplate		(CCmdUI* pCmdUI);
	void LoadTemplate();
			BOOL LoadTemplate				(BOOL bEditing, BOOL bShowDialog = TRUE, BOOL bImportStaticObjects=TRUE, BOOL bUpdateDoc = TRUE);
			void UnloadTemplate				();
			void RemoveTemplateStyles		();
			void RemoveTemplateStyles		(CLayout* pObjects);
			void ApplyTemplateStyles		(BOOL bEditing = FALSE);
			void ApplyTemplateStyles		(CLayout* pObjects);
			void ApplyTemplateStyles		(BaseObj* pO);
			void AddStaticObjects			(CLayout* pObjects, BOOL bDynamicInherit = TRUE);
				void AddInheritObjects		(CLayout* pObjects);
				void AddAllStaticObjects	(CLayout* pObjects);
			void RemoveInheritObjects		(CLayout* pObjects);
			void UnparseInheritTemplateObjects (ViewUnparser& ofile);
			void LoadTemplateFontStyles		();
			void UnloadTemplateFontStyles	();

			void ClearCustomStyles (CLayout* pObjects);
	afx_msg void OnClearCustomStyles		();
	afx_msg void OnUpdateClearCustomStyles	(CCmdUI* pCmdUI);

	//afx_msg void OnRenameLayout				();
	//afx_msg void OnUpdateRenameLayout		(CCmdUI* pCmdUI);
	afx_msg void OnLayoutCopyObject			();
	afx_msg void OnLayoutMoveObject			();
	afx_msg void OnUpdateLayoutCopyMoveObject	(CCmdUI* pCmdUI);

	//afx_msg void OnRemoveLayoutObject();
	afx_msg void OnAddLink();

	//overwrite basedocument's methods
	afx_msg void OnUpdateSwitchTo			(CCmdUI* pCmdUI);
	afx_msg void OnUpdateBackToMenu			(CCmdUI* pCmdUI);

			BOOL GetLayoutSourceObject		(CLayout*& pSourceObjects, BaseObj*& pObj);
public:
	afx_msg void OnUpdateVKSize(CCmdUI* pCmdUI);
	afx_msg void OnUpdateVKMove(CCmdUI* pCmdUI);
	afx_msg void OnUpdateSnapToGrid(CCmdUI* pCmdUI);
	afx_msg void OnUpdateToggleTransparent(CCmdUI* pCmdUI);
	afx_msg void OnUpdateToggleBorder(CCmdUI* pCmdUI);

public:
			void LayoutCopyObject			(CLayout* pSourceObjects, CLayout* pTargetObjects, BaseObj* pSourceObj);
			void LayoutMoveObject			(CLayout* pSourceObjects, CLayout* pTargetObjects, BaseObj* pSourceObj);

	virtual BOOL		ExistsFieldID		(WORD id, BOOL bSkipCurrentLayout) const;
	virtual	BOOL		ExistsID			(WORD id, CString sLayoutName) const;
	virtual	IBaseObj*	FindByID			(WORD id, CString sLayoutName) const;
	virtual BOOL		ExistsColumnID		(WORD id, CString sLayoutName) const;
	virtual BOOL		ColumnShowedAsImage (WORD id, CString sLayoutName) const; 
	virtual	CObject*	GetColumnPtr		(WORD id, CString sLayoutName) const;
	const TableColumn*	FindColumnByID		(WORD id, CString sLayoutName) const;


			void OnBringToFront				(UINT nID);
			void OnSendToBack				(BaseObj*);

	static BOOL GetReportVariables			(CTBNamespace& nsReport, DataTypeNamedArray& arReportColumns, DataTypeNamedArray& arReportAskFields);
	static BOOL GetSchemaReportVariables	(CTBNamespace& nsReport, DataTypeNamedArray& arReportColumns, DataTypeNamedArray& arReportAskFields)
					{
						return GetReportVariables(nsReport, arReportColumns, arReportAskFields);
					}

	typedef struct _STRUCT_FINDWORD
	{
		CString 	m_strFindWord;
		BOOL		m_bIsDown;
		int			m_nCurrPage;
		int			m_nCurrRow;
		int			m_nCurrCol;
		DataType	m_Type;
		ECompareType m_eCmp;

		struct _STRUCT_FINDWORD()
			:
			m_nCurrCol(0),
			m_nCurrPage(0),
			m_nCurrRow(0),
			m_bIsDown(TRUE),
			m_Type (DataType::String),
			m_eCmp (ECompareType::CMP_CONTAINS)
		{}

		void Clear()
		{
			m_strFindWord.Empty();
			m_bIsDown = TRUE;
			m_nCurrPage = 0;
			m_nCurrCol = 0;
			m_nCurrRow  = 0;
			m_Type = DataType::String;
			m_eCmp = ECompareType::CMP_CONTAINS;
		}

	} STRUCT_FINDWORD;

	STRUCT_FINDWORD m_findWordInfo;

	DECLARE_MESSAGE_MAP()
public:
	virtual BOOL OnCmdMsg(UINT nID, int nCode, void* pExtra, AFX_CMDHANDLERINFO* pHandlerInfo);

	virtual BOOL IsReportString() const;
	virtual CString GetReportString() const;
	
	BOOL OnGetToolTipText (CPoint* pt, CString& strMessage);

	virtual WebCommandType OnGetWebCommandType(UINT commandID);
		
	virtual BOOL CanDeleteField		(LPCTSTR, CString&, BOOL* onlyUI = NULL) const;

	/*TBWebMethod*/DataBool Print(DataBool bClose);
	/*TBWebMethod*/DataBool Close();

	//----
	CObArray	m_arBringToFrontObjects;
	BOOL BringToFrontPopup			(BaseObj* pCurrent, CMenu& menu);
	
	//----
	virtual CString GetUICulture();
	virtual CString SetUICulture(const CString&);
	void			SetCustomUICulture();
	void			ResetCustomUICulture();
	void			SetCultureStrings	(const CString& sCulture);

	//----
	void AddViewField			(WoormField* pF, BOOL bDropDown = TRUE);
	void RemoveViewField		(const CString& strName);
	
	//----
	BOOL FillComboTemplates	();
	void FillComboTemplates (UINT nID, LPCTSTR sNs);

	void OnComboTemplatesSelChanged	();

	BOOL SetTemplateStyle		(CObject*);
	BOOL OpenReportTemplate		(const CString& sNsName);
	BOOL LoadReportTemplate		(const CString& strPathName);

	CObject*		GetSelectedTableObject(Table* pTable);

	CView*			CreateSlaveView(CRuntimeClass* pViewClass);

	CFontStylesDlg* CreateFontStyleDlg(int& nFontIdx);

	virtual EditorManager*	GetEditorManager() const;

	virtual void PreDeleteField(LPCTSTR pszFieldName);
	void RemoveObjectFromItemData(CObject* pItemData);

	CMyStatusBar* GetStatusBar();

	//----------------------------------------------------------
	//ERefreshEditor GetRefreshType(CNodeTree::ENodeType nt);
	virtual HTREEITEM	RefreshRSTree(ERefreshEditor, CObject* itemDataToSelect = NULL, BOOL selectRoot = FALSE);
	CRSTreeCtrl*	GetRSTree(ERefreshEditor);
	HTREEITEM		SelectRSTreeItemData(ERefreshEditor, CObject* itemDataToSelect, BOOL appendToSelected = FALSE);
	HTREEITEM		DeselectRSTreeItemData(ERefreshEditor, CObject* itemDataToSelect);
	void			UpdateRSTreeNode(ERefreshEditor e, CNodeTree* pNode, BOOL bSelect = TRUE);
	void			UpdateRSTreeNode(ERefreshEditor e, MultiColumnSelection* pColumns);
	void			UpdateRSTreeNode(ERefreshEditor e, SelectionRect* pMulSel);
	void			UpdateRSTreeNode(ERefreshEditor e, BaseObj* pObj, BOOL bSelect);
	void			RSTreeEnsureVisibleItem(ERefreshEditor e, CObject* pObj, BOOL bSelect = TRUE, BOOL bExpand = FALSE, BOOL bPassive = FALSE);
	void			ClearSelectionFromAllTrees(CRSTreeCtrl* pExceptTree = NULL);
	void			ClearMultiSelection(BOOL ifMultiselect);
	HTREEITEM		FindRSTreeItemData(ERefreshEditor eRe, CObject* itemDataToSelect);
	BOOL			RSTreeContainsItemData(ERefreshEditor eRe, CObject* itemDataToSelect);
	BOOL			RSTreeIsSelectedItemData(ERefreshEditor eRe, CObject* itemDataToSelect);
	void			RefreshObjectPropertyView();

	TblRuleData*	AddDBColumns_GetOrCreateTblRule		(CNodeTree* pNode);
	WoormField*		AddDBColumns_CreateField			(SqlColumnInfo*, TblRuleData*, int idxTable);
	WoormField*		AddDBColumns_CreateField			(CNodeTree* pNode, TblRuleData*, int idxTable);
	WoormField*		AddDBColumns_FromToolBar			(CNodeTree* pNode, BOOL bSelectIt, TblRuleData* pCurrTblRule, int idxTable, BOOL bRefresh = TRUE, CStringArray* varNames = NULL);
	WoormField*		AddDBColumns_FromToolBar			(CNodeTreeArray*, BOOL bSelectLast, TblRuleData* pCurrTblRule, int idxTable, CStringArray* varNames = NULL);

	//------
	virtual BOOL	CanCloseFrame(CFrameWnd* pFrameArg);

	//------
	virtual BOOL		OpenDebugger		(ActionObj* pCurrCmd);
			DataBool	DoOpenDebuggerWrp	(DataLng dlCurrCmdHandle);
	virtual BOOL		DoOpenDebugger		(ActionObj* pCurrCmd);

	//------
	virtual DataArray* GetDataArrayFromId(WORD /*nID*/, DataArray*);
	virtual BOOL FillSeries(CBCGPChartSeries* /*pSeries*/, WORD /*nID*/);
	virtual BOOL FillSeries(CStringArray& /*pCategories*/, WORD /*nID*/);
	virtual BOOL FillSeries(CBCGPChartSeries* /*pSeries*/, WORD /*nID*/, WORD /*nID*/);
};

///////////////////////////////////////////////////////////////////////////////
#include "endh.dex"
