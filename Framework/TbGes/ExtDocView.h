#pragma once

#include <atlimage.h>

#include <TbGenlib\oslinfo.h>
#include <TbGenlib\parsedt.h>
#include <TbGenlib\CEFClasses.h>
#include <TbGenlib\parsedt.h>
#include <TbGenlib\TABCORE.H>

#include "ExtDocViewTool.h"

//-------------------------------------------------------------------------
//includere alla fine degli include del .H
#include "beginh.dex"
//============================================================================
class CBodyEdit;
class ColumnInfo;
class TabManagers;
class CTabManager;
class CBaseTabManager;
class CMasterFrame;
class CSlaveFrame;
class DBTSlaveBuffered;
class DBTObject;
class CTabWizard;
class CBoolCheckListBox;
class HotKeyLink;
class CAbstractFormDoc;
class CExtButtonExtendedInfo;
class CLabelStatic;
class CGroupBoxBtn;
class CDBTTreeEdit;
class CSlaveViewContainer;
class CParsedPanel;
class CModelessBrowserDlg;
class CTileGroup;
class TileGroups;
class CTileManager;
class CHeaderStrip;
class CTBHotlinkControl;
class CTBGridControl;
class CTaskBuilderBreadcrumb;
class CTBPropertyGrid;
class CTBProperty;
class CBETooltipProperties;
class CTileDialog;
class CJsonContext;
#define WIZARD_DEFAULT_TAB			0
#define WIZARD_SAME_TAB				1

/////////////////////////////////////////////////////////////////////////////
/////////////////////////////////////////////////////////////////////////////
//
//	WARNING !! :
//		
//	ALL controls in ALL views (master/slave/row-view and tabdialog) 
//	of the SAME Document
//	MUST have different IDC (play attention by Client Documents)
//			
/////////////////////////////////////////////////////////////////////////////
/////////////////////////////////////////////////////////////////////////////


////////////////////////////////////////////////////////////////////////////////
//				funzioni di uso generale
////////////////////////////////////////////////////////////////////////////////
//
//-----------------------------------------------------------------------------
TB_EXPORT void	EnableControlLinks	(ControlLinks* pControlLinks, BOOL bEnable = TRUE, BOOL bMustSetOSLReadOnly = FALSE);
TB_EXPORT void	SetOSLReadOnlyOnControlLinks (ControlLinks* pControlLinks, const Array& aDataToCtrlMap);
TB_EXPORT int	AddDataBoolToCheckLB
					(
						CBoolCheckListBox*,
						LPCSTR,
						SqlRecord*, 
						DataObj*
					);

//-----------------------------------------------------------------------------
TB_EXPORT CBodyEdit* GetBodyEdits(ControlLinks* pControlLinks, int* pnStartIdx = NULL);
TB_EXPORT CBodyEdit* GetBodyEdits(ControlLinks* pControlLinks, const CTBNamespace& aNS);

//-----------------------------------------------------------------------------
TB_EXPORT CParsedCtrl* AddLink
	(
		const CString&		sName,
		CWnd*				pParentWnd,
		ControlLinks*		pControlLinks,
		UINT				nIDC, 
		SqlRecord*			pRecord, 
		DataObj*			pDataObj, 
		CRuntimeClass*		pParsedCtrlClass,

		HotKeyLink*			pHotKeyLink			= NULL,
		UINT				nBtnID				= BTN_DEFAULT
	);

TB_EXPORT CExtButton*	AddLink
	(
		const CString&			sName,
		CExtButtonExtendedInfo*	pExtInfo,
		ControlLinks*			pControlLinks,
		UINT					nIDC, 
		SqlRecord*				pRecord	= NULL, 
		DataObj*				pDataObj = NULL
	);
							
TB_EXPORT CBodyEdit*	AddLink
	(
		CParsedForm*		pParentForm,	//CAbstractFormView/TabDialog
		CWnd*				pParentWnd,
		ControlLinks*		pControlLinks,
		UINT				nIDC, 
		DBTSlaveBuffered*	pDBT, 
		CRuntimeClass*		pBodyEditClass,
		CRuntimeClass*		pRowFormViewClass	= NULL, 
		CString				strTitle			= _T(""),	// titolo della RowView e del b.e. nella customizzazione della form
		CString				sBodyName			= _T(""),	// bodyedit name for dynamic binding
		CString				sRowViewName		= _T("")	// row view name for dynamic binding
	);

TB_EXPORT CParsedCtrl* AddLinkAndCreateControl
	(
		const CString&	sName,
		DWORD			dwStyle, 
		const CRect&	rect,
		CWnd*			pWnd,
		ControlLinks*	pControlLinks,
		UINT			nIDC, 
		SqlRecord*		pRecord, 
		DataObj*		pDataObj, 
		CRuntimeClass*	pParsedCtrlClass,
		void*			pHotKeyLink			= NULL,
		BOOL			bIsARuntimeClass	= FALSE,
		UINT			nBtnID				= BTN_DEFAULT
	);

TB_EXPORT CBodyEdit* AddLinkAndCreateBodyEdit
	(
		CRect				rect,
		CParsedForm*		pParentForm,
		CWnd*				pParentWnd,
		ControlLinks*		pControlLinks,
		UINT				nIDC, 
		DBTSlaveBuffered*	pDBT, 
		CRuntimeClass*		pBodyEditClass,
		CRuntimeClass*		pRowFormViewClass	= NULL,
		CString				strRowFormViewTitle = _T(""),
		CString				sBodyName			= _T(""),
		CString				sRowViewName		= _T("")
	);

TB_EXPORT CDBTTreeEdit* AddLink
	(
		CParsedForm*		pParentForm,
		CWnd*				pParentWnd,
		ControlLinks*		pControlLinks,
		UINT				nIDC, 
		DBTSlaveBuffered*	pDBT, 
		CString				sName,
		CRuntimeClass*		pTreeClass = NULL
	);

TB_EXPORT CLabelStatic* AddLabelLink (CWnd* pParentWnd, UINT nIDC);

TB_EXPORT CLabelStatic* AddLabelLinkWithLine(CWnd* pParentWnd, UINT nIDC, COLORREF titleColor, int nSizePen = 1, /*ELinePos*/int pos = CLabelStatic::LP_TOP);

TB_EXPORT CLabelStatic* AddSeparatorLink (CWnd* pParentWnd, UINT nIDC, COLORREF crBorder, int nSizePen = 1, BOOL  bVertical = FALSE, CLabelStatic::ELinePos pos = CLabelStatic::LP_VCENTER);

TB_EXPORT CGroupBoxBtn* AddGroupBoxLink (CWnd* pParentWnd, UINT nIDC);

TB_EXPORT CParsedCtrl*  ReplaceAddLink
	(
		CWnd*				pParentWnd,
		ControlLinks*		pControlLinks,
		UINT				nIDC, 
		CRuntimeClass*		pParsedCtrlClass,
		HotKeyLink*			pHotKeyLink			 = NULL ,
		UINT				nBtnID				 = BTN_DEFAULT
	);


TB_EXPORT CTBGridControl*	AddLinkGridInternal
	(
		CParsedForm*		pParentForm,	//CAbstractFormView/TabDialog
		CWnd*				pParentWnd,
		ControlLinks*		pControlLinks,
		UINT				nIDC, 
		DBTSlaveBuffered*	pDBT, 
		CRuntimeClass*		pGridRuntimeClass = NULL,
		CString				sGridName			= _T("")
	);

TB_EXPORT CParsedPanel* AddLink
	(
				CWnd*			pParentWnd,
				ControlLinks*	pControlLinks,
				UINT			nIDC, 
				CRuntimeClass*	pParsedPanelClass, 
				CObject*		pPanelOwner,
		const	CString&		sName, 
		const	CString&		sCaption = _T(""), 
				BOOL			bCallOnInitialUpdate = TRUE
	);

TB_EXPORT CTBPropertyGrid*	AddLinkPropertyGrid
	(
		CTBPropertyGrid*	pGrid,		
		CParsedForm*		pParentForm,
		CWnd*				pParentWnd,
		ControlLinks*		pControlLinks,
		UINT				nIDC,
		CString				sName,
		CRect				rect = CRect(0, 0, 0, 0)
	);
TB_EXPORT CTBPropertyGrid*	AddLinkPropertyGrid
	(
		CParsedForm*		pParentForm,
		CWnd*				pParentWnd,
		ControlLinks*		pControlLinks,
		UINT				nIDC,
		CString				sName,
		CRuntimeClass*		pRuntimeClass = NULL,
		CRect				rect = CRect(0,0,0,0)
	);

TB_EXPORT BOOL SubclassParsedControl
	(
		CWnd*			pParent,
		UINT			nIDC,
		CWnd*			pControl,
		DataObj*		pDataObj,
		const CString&	sName,
		const CString&	sNsHotKeyLink = L"",
		UINT			nBtnID = BTN_DEFAULT
	);

//=============================================================================
// Record & Data Connection)

#define RDC(a,b)	(\
	a->IsVirtual(a->Lookup(&(a->b))) ? a->GetColumnName(&(a->b)) : a->GetTableName() + _T("_") + a->GetColumnName(&(a->b))\
	),a,&(a->b)

#define ADDON_RDC(a,b)	(\
	a->IsVirtual(a->Lookup(&(b))) ? a->GetColumnName(&(b)) : a->GetTableName() + _T("_") + a->GetColumnName(&(b))\
	),a,&(b)

#define SDC(b)		NULL, &(GetDocument()->b)
#define ClientDocSDC(cldoc,b)		NULL, &(cldoc->b)
// per i campi aggiunti (AddOn-Field-Connection il primo parametro deve essere il puntatore ad un
// oggetto istanza di classe derivata da SqlAddOnFieldsColumn
#define AFC(a,b)	a->GetRecParent(), &(a->b)
#define ROW_AFC(a,b,c)	a, &(b->c)
//bind ausiliari sui parsed control nelle rowview (CAddressEdit, CZipCodeEdit, CPhoneEdit, DynamicDecimalFormatter (?)
#define RW_BIND(a,b)	&(a->b),a->Lookup(&(a->b))
 
/*****************************************************************************
*                         F O R M    V I E W S								 *
******************************************************************************/

//===========================================================================
class TB_EXPORT CAbstractFormView : public CBaseFormView
{
	DECLARE_DYNAMIC(CAbstractFormView)

	friend class CAbstractFormDoc;
	friend class CAbstractFormFrame;
	friend class CTabber;
	friend class CTabDialog;
	friend class CBodyEdit;

private:
	BOOL				m_bInitialUpdateDone;
	int					m_mwTop;
	//Prj. 6709 - tentativo di risolvere il flickering
	//flag necessario per memorizzare lo stato "hide" della view e fare "show" una volta sola
	BOOL				m_bTemporaryHidden;

public:		// Data Member
	TabManagers*		m_pTabManagers;		// contiene i tab dialog manager
	TileGroups*			m_pTileGroups;	//contiene i tile managers
	DlgInfoArray		m_DlgInfos;

	CString				m_strStop;
	CString				m_strStart;
	CString				m_strResume;
	CString				m_strPause;

	CMap<CString, LPCTSTR, HWND, HWND> m_HWNDPositionsMap;//mappa ad uso e consumo di easybuilder per trovare gli handle delle finestre non registrate

protected:
	// constructors
	// don't must implement CAbstractFormView(LPCTSTR lpszTemplateName) constructor
	CAbstractFormView(const CString& sName, UINT nIDTemplate);

public:
	virtual ~CAbstractFormView();

public:	// my function implementation
	void				SetTemporaryHidden(BOOL bSet) { m_bTemporaryHidden = bSet; }
	CAbstractFormDoc*	GetDocument() const;
	CAbstractFormFrame*	GetFrame() const;
	CParsedCtrl*		GetLinkedParsedCtrl(UINT nIDC);
	CParsedCtrl*		GetLinkedParsedCtrl(const CTBNamespace& aNS);
	CParsedCtrl*		GetLinkedParsedCtrl(DataObj* pDataObj);
	CWnd*				GetWndLinkedCtrl(UINT nIDC);
	virtual CWnd*		GetWndLinkedCtrl(const CTBNamespace& aNS);
	CBodyEdit*			GetBodyEdits(int* pnStartIdx = NULL);
	CBodyEdit*			GetBodyEdits(const CTBNamespace& aNS);
	CBaseTabManager*	GetTabber(UINT nIDC);
	CWnd*				GetWndCtrl(UINT nIDC);
	void				ShiftControl(UINT nTop, UINT nExpandHeight);
	void				MoveControls(CSize offset);
	BOOL				SetControlValue(UINT nIDC, const DataObj& val);

	void				PrepareTabDialogNamespaces();
	void				SetCustomMoveTopWindows(int top);


	void				CalculateOSLInfo();

	CTileGroup*			GetTileGroup(UINT nIDC);
	CBaseTileDialog*	GetTileDialog(UINT nIDD);
	void				RemoveTileGroup(UINT nIDC);
	void				MoveTileGroup(CBaseTileGroup* pTileGroup, int indexNew);

protected:
	BOOL	TabDialogActivate(const CString& sNsTab);
	int		TabDialogActivate(UINT nTabIDC, UINT nIDD);
	int		TabDialogShow(UINT nTabIDC, UINT nIDD, BOOL /*= TRUE*/);
	int		TabDialogEnable(UINT nTabIDC, UINT nIDD, BOOL /*= TRUE*/);
	int		TileGroupEnable(UINT nTileManagerIDC, UINT nIDDTileGroup, BOOL bEnable /* = TRUE */);
	int		TileGroupShow(UINT nTileManagerIDC, UINT nIDDTileGroup, BOOL bShow);
	int		TileDialogEnable(UINT nIDDTileGroup, UINT nIDD, BOOL bEnable);
	int		TilePanelEnable(UINT nIDDTileGroup, UINT nIDD, BOOL bEnable);
	int		TileGroupActivate(UINT nTileManagerIDC, UINT nTileGroupIDC);

	//virtuale perchè permetto alla view gestioni diverse da quelle classiche in base al cambio di stato del documento (vedi tbDockPane e AttachmentView del DMS)
	virtual void	EnableViewControls();
	virtual BOOL	BatchEnableViewControls();
	void	SetBatchViewButtonState();

public:
	virtual void SyncExternalControllerInfo(BOOL bSave);
	BOOL	DispatchPrepareAuxData();

public:
	// Nuova gestione delle invalidazioni
	virtual	void EnableViewControlLinks(BOOL bEnable = TRUE, BOOL bMustSetOSLReadOnly = FALSE);
	virtual	void OnUpdateControls(BOOL bParentIsVisible = TRUE);
	virtual	void OnResetDataObjs();
	virtual BOOL OnPrepareAuxData() { return TRUE; }
	virtual BOOL OnPrepareAuxData(CTileDialog*) { return TRUE; }
	virtual CString GetCaption();
	virtual CString OnGetCaption(CAbstractFormView* ) { return _T(""); }
	virtual void OnUpdateTitle(CTileDialog*) {}
	void OnFindHotLinks();
protected:
	virtual void	SetDefaultFocus();
	// per la gestione dei Task schedulati
	virtual BOOL SetControlAutomaticExpression(UINT nID, const CString& strExp);
	virtual BOOL SetControlAutomaticExpression(DataObj*, const CString&);
	virtual BOOL GetControlAutomaticExpression(UINT& nID, CString& strExp);

	// add control links into objects array
	virtual	void BuildDataControlLinks() = 0;
	virtual void OnSetBatchButtonIDS() {/* default do nothig*/ }

public:
	virtual CParsedCtrl* AddLink
		(
			UINT			nIDC,
			const CString&	sName,
			SqlRecord*		pRecord,
			DataObj*		pDataObj,
			CRuntimeClass*	pParsedCtrlClass,

			HotKeyLink*		pHotKeyLink = NULL,
			UINT			nBtnID = BTN_DEFAULT
			);
	virtual CParsedCtrl* AddLink
		(
			UINT			nIDC,
			const CString&	sName,
			SqlRecord*		pRecord,
			DataObj*		pDataObj,
			CRuntimeClass*	pParsedCtrlClass,

			CString			sNsHotKeyLink,
			UINT			nBtnID = BTN_DEFAULT
			);

	virtual CExtButton* AddLink
		(
			UINT nIDC,
			const CString&		sName,
			SqlRecord* = NULL,
			DataObj* = NULL
			);

	virtual CBodyEdit*	AddLink
		(
			UINT				nIDC,
			DBTSlaveBuffered*	pDBT,
			CRuntimeClass*		pBodyEditClass,
			CRuntimeClass*		pRowFormViewClass, // = NULL
			CString				strTitle, //titolo della RowView e del b.e. nella customizzazione della form
			CString				sName = _T("")
			);

	virtual  CTBGridControl* AddLinkGrid
		(
			UINT				nIDC,
			DBTSlaveBuffered*	pDBT,
			CRuntimeClass*		pGridControlClass,
			CString				sName
			);

	virtual CTBGridControl* AddLinkGrid
		(
			UINT				nIDC,
			SqlTable*			sqlTbl,
			CRuntimeClass*		pGridControlClass,
			CString				sName

			);

	virtual CDBTTreeEdit*		AddLink
		(
			UINT				nIDC,
			DBTSlaveBuffered*	pDBT,
			CString				sName = _T(""),
			CRuntimeClass*		pTreeClass = NULL
			);

	virtual CParsedPanel* AddLink
		(
			UINT			nIDC,
			CRuntimeClass*	pParsedPanelClass,
			CObject*		pPanelOwner,
			const	CString&		sName,
			const	CString&		sCaption = _T(""),
			BOOL			bCallOnInitialUpdate = TRUE
			);

	CParsedCtrl* AddLinkAndCreateControl
		(
			const CString&	sName,
			DWORD			dwStyle,
			const CRect&	rect,
			UINT			nIDC,
			SqlRecord*		pRecord,
			DataObj*		pDataObj,
			CRuntimeClass*	pParsedCtrlClass,
			HotKeyLink*		pHotKeyLink = NULL,
			UINT			nBtnID = BTN_DEFAULT
			);

	CBodyEdit* AddLinkAndCreateBodyEdit
		(
			CRect				rect,
			UINT				nIDC,
			DBTSlaveBuffered*	pDBT,
			CRuntimeClass*		pBodyEditClass,
			CRuntimeClass*		pRowFormViewClass = NULL,
			CString				strRowFormViewTitle = _T(""),
			CString				sBodyName = _T(""),
			CString				sRowViewName = _T("")
			);

	CLabelStatic* AddLabelLink(UINT nIDC);
	CLabelStatic* AddLabelLinkWithLine(UINT nIDC, int nSizePen = 1, int pos = CLabelStatic::LP_TOP);
	CLabelStatic* AddSeparatorLink(UINT nIDC, COLORREF crBorder, int nSizePen = 1, BOOL  bVertical = FALSE, CLabelStatic::ELinePos pos = CLabelStatic::LP_VCENTER);
	CGroupBoxBtn* AddGroupBoxLink(UINT nIDC);

	CParsedCtrl*   ReplaceAddLink
		(
			UINT			nIDC,
			CRuntimeClass*	pParsedCtrlClass,
			HotKeyLink*		pHotKeyLink = NULL,
			UINT			nBtnID = BTN_DEFAULT
			);

	CTBPropertyGrid*	AddLinkPropertyGrid
		(
			UINT				nIDC,
			CString				sName,
			CRuntimeClass*		pRuntimeClass = NULL
			);

	virtual CBaseTabManager*	AddBaseTabManager(UINT nIDC, CRuntimeClass* pClass, const CString& sName, BOOL bCallOnInitialUpdate = TRUE) { return (CBaseTabManager*)AddTabManager(nIDC, pClass, sName, bCallOnInitialUpdate); }

	// Istanzia un nuovo CTabManager della classe indicata e lo inizializza
	virtual CTabManager* AddTabManager(UINT nIDC, CRuntimeClass* pClass, const CString& sName, BOOL bCallOnInitialUpdate = TRUE);
	virtual CTabManager* CreateTabManager(UINT nIDC, CRuntimeClass* pClass, UINT nHeight, const CString& sName);

	CTileManager*	AddTileManager(UINT nIDC, CRuntimeClass* pClass, const CString& sName, BOOL bCallOnInitialUpdate = TRUE);

	CTileGroup*		AddTileGroup(UINT nIDC, CRuntimeClass* pClass, const CString& sName, BOOL bCallOnInitialUpdate = TRUE, CRect rectWnd = CRect(0, 0, 0, 0));

	CHeaderStrip*	AddHeaderStrip(UINT nIDC, const CString& sDefaultCaption, BOOL bCallInitialUpdate = TRUE, CRect rectWnd = CRect(0, 0, 0, 0), CRuntimeClass* pClass = NULL);

	virtual void PerformBatchOperations();
	virtual CWnd* GetWndFromIDC(const DWORD nIDC) { return GetWndCtrl(nIDC); }
	virtual void CustomizeTabber(CTabManager*) {}
	virtual 	BOOL	CanCreateControl(UINT idc) { return TRUE; }//permette di evitare la creazione del controllo o della colonna
	virtual 	void	OnParsedControlCreated(CParsedCtrl* pCtrl) {}//chiamato dopo la creazione del controllo, permette di modificarne lo stato
	virtual 	void	OnColumnInfoCreated(ColumnInfo* pColInfo) {}//chiamato dopo la creazione della colonna, permette di modificarne lo stato
	virtual 	void	OnPropertyCreated(CTBProperty* pProperty) {}//chiamato dopo la creazione della property, permette di modificarne lo stato
	virtual		BOOL	OnGetToolTipProperties(CBETooltipProperties* pTooltip) { return FALSE; }
	virtual 	void	EnableBodyEditButtons(CBodyEdit* pBodyEdit) {}
	virtual		void	CustomizeBodyEdit(CBodyEdit*) { }
				BOOL	ReCreateControls();
private:
	void ActivateTabDialogs(TabManagers* pTabManagers);

protected:	// Implementation enhancement
	virtual void OnInitialUpdate	();
	virtual	BOOL PreTranslateMessage(MSG* pMsg);

	// Generated message map functions
	//{{AFX_MSG(CAbstractFormView)
	afx_msg LRESULT OnValueChanged	(WPARAM, LPARAM);
	afx_msg LRESULT OnRunBatch		(WPARAM, LPARAM);

	// gestione dei messaggi per TabPro
	afx_msg LRESULT OnGetControlDescription(WPARAM, LPARAM);
	afx_msg LRESULT OnGetLocalizerInfo(WPARAM wParam, LPARAM lParam);
    // status bar message management
	afx_msg void OnScrollUp		();
	afx_msg void OnScrollDown	();
	afx_msg void OnScrollBotton	();
	afx_msg void OnScrollRight	();
	//gestione centratura controlli
	afx_msg void OnSize(UINT nType, int cx, int cy);
	afx_msg BOOL OnEraseBkgnd(CDC* pDC);
	afx_msg void OnRButtonDown	(UINT nFlag, CPoint ptMousePos);
	afx_msg void OnPaint();
	//}}AFX_MSG
	DECLARE_MESSAGE_MAP()

public:
	afx_msg void OnScrollLeft	();
	afx_msg void OnScrollTop	();

// Diagnostics
#ifdef _DEBUG
public:
	void Dump (CDumpContext&) const;
	void AssertValid() const;
#endif // _DEBUG
	afx_msg void OnDestroy();
};


//=============================================================================
class TB_EXPORT CMasterFormView : public CAbstractFormView
{
	DECLARE_DYNAMIC(CMasterFormView)

protected:	// Construction
	CMasterFormView(const CString& sName, UINT nIDTemplate);

public:		// Attributes
	CMasterFrame*		GetFrame() const;

protected:
	//{{AFX_MSG(CMasterFormView)
	//}}AFX_MSG
	DECLARE_MESSAGE_MAP()
	virtual void OnInitialUpdate();

// Diagnostics
#ifdef _DEBUG
public:
	void Dump (CDumpContext&) const;
	void AssertValid() const;
#endif // _DEBUG
};

//=============================================================================
class TB_EXPORT CSlaveFormView : public CAbstractFormView
{
	DECLARE_DYNAMIC(CSlaveFormView)
	
protected:	// Construction
	CSlaveFormView(const CString& sName, UINT nIDTemplate);

public:		// Attributes
	CSlaveFrame*		GetFrame	() const { return (CSlaveFrame*) GetParentFrame(); }

protected:
	virtual	void EnableViewControlLinks	(BOOL bEnable = TRUE, BOOL bMustSetOSLReadOnly = FALSE);
	virtual void SetDefaultFocus		();
	virtual void OnActivateView(BOOL bActive, CView* pActivateView, CView* pDeactiveView);
	DECLARE_MESSAGE_MAP()

// Diagnostics
#ifdef _DEBUG
public:
	void Dump (CDumpContext&) const;
	void AssertValid() const;
#endif // _DEBUG
};

//=============================================================================
class TB_EXPORT CJsonSlaveFormView : public CSlaveFormView
{
private:
	DECLARE_DYNCREATE(CJsonSlaveFormView)
public:
	CJsonSlaveFormView();
	CJsonSlaveFormView(UINT nIDTemplate);
protected:
	virtual	void BuildDataControlLinks() {};
};

//=============================================================================
class TB_EXPORT CSlaveFixedSizeView : public CSlaveFormView
{
	DECLARE_DYNAMIC(CSlaveFixedSizeView)
	
public:	
	CSlaveFixedSizeView(const CString& sName, UINT nIDTemplate);

	virtual BOOL PreCreateWindow(CREATESTRUCT& cs); 
};

//=============================================================================
class TB_EXPORT CRowFormView  : public CAbstractFormView
{
	DECLARE_DYNAMIC(CRowFormView)
	
	friend class CRowFormFrame;
	friend class CBodyEdit;
	friend class CRowTabDialog;
	friend class CTileDialog;

protected:
	CRuntimeClass*		m_pTemplateRecordClass;

private:
	CBodyEdit*				m_pBodyEdit;
	Array					m_DataToCtrlMap;
	
protected:	// Construction
	CRowFormView(const CString& sName, UINT nIDTemplate);

public:
	~CRowFormView();

public:		// Attributes
	CRowFormFrame*		GetFrame	() const { return (CRowFormFrame*) GetParentFrame(); }

	int					GetDataIdxMappedToCtrl	(int nCtrlIdx);
	DBTSlaveBuffered*	GetDBT					() const;
	CBodyEdit*			GetBodyEdit				() const;

public:		// Attributes
	void RebuildMappedLinks			(SqlRecord*, ControlLinks*, const Array&);
	void BuildMappedDataToCtrlLink	(SqlRecord*, DataObj*, Array&, int, int = -1);
 
public:
	virtual CParsedCtrl* AddLink
	(
		UINT			nIDC, 
		const CString&	sName,
		SqlRecord*		pRecord, 
		DataObj*		pDataObj, 
		CRuntimeClass*	pParsedCtrlClass,

		HotKeyLink*		pHotKeyLink			= NULL,
		UINT			nBtnID				= BTN_DEFAULT
	);

	CExtButton*	AddLink 
		(
			UINT nIDC, 
			const CString& sName,
			SqlRecord* = NULL, 
			DataObj* = NULL
		);

	CBodyEdit*	AddLink
		(
			UINT				nIDC, 
			DBTSlaveBuffered*	pDBT, 
			CRuntimeClass*		pBodyEditClass,
			CRuntimeClass*		pRowFormViewClass = NULL,
			CString				strTitle = _T(""), //titolo della RowView e del b.e. nella customizzazione della form
			CString				sName = _T("")
		);

	CParsedCtrl* AddLinkAndCreateControl
	(
		const CString&	sName,
		DWORD			dwStyle, 
		const CRect&	rect,
		UINT			nIDC, 
		SqlRecord*		pRecord, 
		DataObj*		pDataObj, 
		CRuntimeClass*	pParsedCtrlClass,
		void*			pHotKeyLink			= NULL,
		BOOL			bIsARuntimeClass	= FALSE,
		UINT			nBtnID				= BTN_DEFAULT
	);

	int AddDataBoolToCheckLB
		(
			CBoolCheckListBox*,
			LPCTSTR,
			SqlRecord*, 
			DataObj*
		);

protected:
	void RebuildLinks			(SqlRecord*);

	void Attach	(CBodyEdit* pBodyEdit);
	void Detach	();

protected:
	virtual	void EnableViewControlLinks	(BOOL bEnable = TRUE, BOOL bMustSetOSLReadOnly = FALSE);
	virtual	void OnUpdateControls		(BOOL bParentIsVisible = TRUE);
	virtual void SetDefaultFocus		();
	virtual void OnInitialUpdate		();
protected:
	virtual void OnActivateView		(BOOL, CView*, CView*);

	//{{AFX_MSG(CRowFormView)
	afx_msg LRESULT OnValueChanged					(WPARAM, LPARAM);
	afx_msg	void	OnSetFocus						(CWnd* pOldWnd);
	afx_msg void	OnUpdateTotalRecordsIndicator	(CCmdUI*);

	afx_msg void	OnGotoMaster		();
	afx_msg void	OnMoveToPrevRow		();
	afx_msg void	OnMoveToNextRow		();
	afx_msg void	OnMoveToFirstRow	();
	afx_msg void	OnMoveToLastRow		();
	
	afx_msg void	OnUpdateMoveToPrevRow	(CCmdUI*);
	afx_msg void	OnUpdateMoveToNextRow	(CCmdUI*);
	afx_msg void	OnUpdateMoveToFirstRow	(CCmdUI*);
	afx_msg void	OnUpdateMoveToLastRow	(CCmdUI*);
	
	afx_msg void	OnDeleteRow			();
	afx_msg void	OnInsertRow			();

	afx_msg void	OnUpdateDeleteRow		(CCmdUI*);
	afx_msg void	OnUpdateInsertRow		(CCmdUI*);

			void	SetCurrRowByMove	(int nRow);
			void	DoMessageOnRecordChanged ();
	//}}AFX_MSG
	DECLARE_MESSAGE_MAP()

public:
	//usate anche dalle CRowTabDialog
	static void EnableMappedControlLinks(ControlLinks*, const Array&, BOOL bEnable, BOOL bMustSetOSLReadOnly);
	void OnUpdateMappedControls	(ControlLinks*, const Array&);

private:
	ColumnInfo* GetLinkedColumnInfo (int nDataIdx);
public:
	BOOL	CanDoMoveToPrevRow	();
	BOOL	CanDoMoveToNextRow	();
	BOOL	CanDoMoveToFirstRow	();
	BOOL	CanDoMoveToLastRow	();

	BOOL	CanDoDeleteRow		();
	BOOL	CanDoInsertRow		();
// Diagnostics
#ifdef _DEBUG
public:
	void Dump (CDumpContext&) const;
	void AssertValid() const;
#endif // _DEBUG
};

//=============================================================================
class TB_EXPORT CDynamicFormView : public CMasterFormView
{
private:

	DECLARE_DYNCREATE(CDynamicFormView)
public:
	CDynamicFormView(const CString& sName = _T("Dynamic"));
protected:
	virtual	void BuildDataControlLinks	() {};
};

//=============================================================================
class TB_EXPORT CJsonFormView : public CMasterFormView
{
	friend class CJsonFrame;
private:

	DECLARE_DYNCREATE(CJsonFormView)
public:
	CJsonFormView();
	CJsonFormView(UINT nIDTemplate);
protected:
	virtual	void BuildDataControlLinks() {};
	void AssignJsonContext(CJsonContext* pContext);
};

class CFrameStepper;
//=============================================================================
class TB_EXPORT CWizardFormView : public CMasterFormView
{
	friend class CWizardFormDoc;
	friend class CAbstractFormDoc;

	DECLARE_DYNCREATE(CWizardFormView)

protected:	
	UINT			m_IDCBitmap;
	CUIntArray		m_PreviousTabs;
	Array			m_arAnchorControls;
	BOOL			m_bIsDirectCallToWizardFinishCall;
	BOOL			m_bWizardFinished;
	BOOL			m_bUseOldButtonStyle;
	BOOL			m_bReExecutable;

public:
	CWizardFormView ();
	CWizardFormView (const CString& sName, UINT nIDTemplate);
	~CWizardFormView ();

private:
	void DoAnchorages	();
	void UpdateStepper		(int nDirection = 1);
	DlgInfoItem* GetLastEnabledItem();
public:		
	CTabWizard*				GetTabManager				();
	CFrameStepper*			GetStepper					();
	virtual CString			GetStepperRootDescription	();

	void DoWizardNext			();
	void SetWizardBitmap		(UINT nIDRes);

	virtual BOOL IsToEnableWizardNext();
	virtual BOOL IsToEnableWizardBack();
	virtual BOOL IsToEnableWizardFinish();
	virtual BOOL IsToEnableWizardRestart();

	AFX_DEPRECATED("Please use document method") const BOOL& IsReExcecutable() const;
	AFX_DEPRECATED("Please use document method") void SetReExcecutable(BOOL bValue);

protected:	
	void SetIDCBitmap				(UINT	IDCBitmap) {m_IDCBitmap = IDCBitmap; }
	void EnableButtons				(int position);


public: //virtual public methods
	virtual void	SetWizardButtons		(DWORD dwFlags);
	virtual	void	OnBuildDataControlLinks	()  {}
	virtual	void	CustomizeTabWizard		(CTabManager* ) {}
	virtual LRESULT OnWizardNext			(UINT /*IDD*/)	{return WIZARD_DEFAULT_TAB;}
	virtual LRESULT OnWizardBack			(UINT /*IDD*/);
	virtual void	OnWizardInit			()	{}
	virtual void	OnWizardEnd				()	{}
	virtual void	OnWizardAbort			()	{}
	virtual void	PerformBatchOperations			();
	virtual CTabManager* AddTabManager(UINT nIDC, CRuntimeClass* pClass, const CString& sName, BOOL bCallOnInitialUpdate = TRUE);
	virtual void SyncExternalControllerInfo(BOOL bSave);

protected:	//virtual protected methods
	virtual	void BuildDataControlLinks	(); 
	
public:		
	//{{AFX_MSG(CWizardFormView)
	afx_msg void OnWizardNext			();
	afx_msg void OnWizardBack			();
	afx_msg void OnWizardFinish			();
	afx_msg void OnWizardCancel			();
	afx_msg void OnWizardRestart		();
	//afx_msg void OnBeforeWizardFinish	();
	afx_msg void OnWizardStart			();
	afx_msg void OnSize					(UINT nType, int cx, int cy);
	afx_msg	void OnVScroll				(UINT nSBCode, UINT nPos, CScrollBar* pScrollBar);
	//afx_msg	void OnBatchStartStop		();

	afx_msg	LRESULT	OnBatchCompleted	(WPARAM wParam, LPARAM lParam);

	//}}AFX_MSG
	DECLARE_MESSAGE_MAP()

// Diagnostics
#ifdef _DEBUG
public:
	void Dump (CDumpContext&) const;
	void AssertValid() const;
#endif // _DEBUG
};

//=============================================================================
class TB_EXPORT CJsonWizardFormView : public CWizardFormView
{
	friend class CJsonFrame;
private:

	DECLARE_DYNCREATE(CJsonWizardFormView)
public:
protected:
	virtual	void BuildDataControlLinks() {};
	void AssignJsonContext(CJsonContext* pContext);
};
//-----------------------------------------------------------------------------
TB_EXPORT CAbstractFormDoc* GetDocument(CWnd* pWnd);

//==========================================================================================
#include "endh.dex"
