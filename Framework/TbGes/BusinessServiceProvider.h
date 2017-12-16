
#pragma once

#include <TbNameSolver\CallbackHandler.h>

#include "EventMng.h"
#include "UIBusinessServiceProvider.h"

#include  "beginh.dex"

#define _BSP_EVENT(ns) _T(ns) 
// dichiarazioni di variabili per utilizzo in JSON
// Nota. pDoc deve essere di tipo CBusinessServiceProviderDoc
#define DECLARE_VAR_BSP_JSON(a)	DeclareVariable(_T(#a), &GetBSP()->m_##a);

#define DECLARE_VAR_BSP_OBJ_JSON(a, pDoc) ASSERT(pDoc); pDoc->DeclareVariable(_T(#a), &m_##a);

#define DECLARE_VAR_WITH_PREFIX_BSP_JSON(prefix, nameVar, pDoc)	ASSERT(pDoc); pDoc->DeclareVariable(prefix#nameVar, &m_##nameVar);

#define DECLARE_VAR_BSP(a) m_pCallerDoc->DeclareVariable(_T(#a), &m_##a);

//////////////////////////////////////////////////////////////////////////////
//					CBSPArray definition
//////////////////////////////////////////////////////////////////////////////
//
class TB_EXPORT CBSPArray : public CArray<CBusinessServiceProviderObj*>
{
public:
	int						 	 Add	(CBusinessServiceProviderObj* pBSP, BOOL bCheckDuplicates = TRUE); 
};

//=============================================================================
class TB_EXPORT MappedEvent
{
public:
	MappedEvent(const CString& strEvent)
	{
		m_strEvent = strEvent;
	}

	MappedFunction* m_pFunction = NULL;
	CObject*		m_pObj = NULL;
	CString			m_strEvent;
};

typedef CMap< int, int, MappedEvent*, MappedEvent* > IDToEvent;

//=============================================================================
class TB_EXPORT MessagesToEvents : public CObject
{
	DECLARE_DYNAMIC(MessagesToEvents);

	friend class CBusinessServiceProviderObj;
	friend class CBusinessServiceProviderClientDoc;

public:
	MessagesToEvents()	{}
	~MessagesToEvents();

public:
	void	OnValueChanged							(int nID, const CString& strEvent);
	void	OnCommand								(int nID, const CString& strEvent)	{ m_OnCommand.SetAt(nID,new MappedEvent(strEvent)); }
	void	OnUpdateCommandUI						(int nID, const CString& strEvent)	{ m_OnUpdateCommandUI.SetAt(nID,new MappedEvent(strEvent)); }
	void	EnableUIWhenFocused						(int nID)							{ m_Enabled.SetAt(nID,new MappedEvent(CString("Enable"))); }
	void	OnCtrlStateChanged						(int nID, const CString& strEvent)	{ m_OnCtrlStateChanged.SetAt(nID,new MappedEvent(strEvent)); }

	// non indicando specifici control su cui si vuole l'abilitazione della UI, questa risulta abilitata per tutti i control
	BOOL	HasEnabledWhenFocused					()									{ return !m_Enabled.IsEmpty(); }
	BOOL	IsUIEnabledWhenFocused					(int nID)							{ return m_Enabled.IsEmpty() || HasEvent(m_Enabled,nID,CString()); }

	void	OnCtrlTreeviewadvSelectionChanged		(int nID, const CString& strEvent)	{ m_OnCtrlTreeviewadvSelectionChanged.		SetAt(nID,new MappedEvent(strEvent)); }
	void	OnCtrlTreeviewadvItemDrag				(int nID, const CString& strEvent)	{ m_OnCtrlTreeviewadvItemDrag.				SetAt(nID,new MappedEvent(strEvent)); }
	void	OnCtrlTreeviewadvDragOver				(int nID, const CString& strEvent)	{ m_OnCtrlTreeviewadvDragOver.				SetAt(nID,new MappedEvent(strEvent)); }
	void	OnCtrlTreeviewadvDragDrop				(int nID, const CString& strEvent)	{ m_OnCtrlTreeviewadvDragDrop.				SetAt(nID,new MappedEvent(strEvent)); }
	void	OnCtrlTreeviewadvMouseUp				(int nID, const CString& strEvent)	{ m_OnCtrlTreeviewadvMouseUp.				SetAt(nID,new MappedEvent(strEvent)); }
	void	OnCtrlTreeviewadvMouseDown				(int nID, const CString& strEvent)	{ m_OnCtrlTreeviewadvMouseDown.				SetAt(nID,new MappedEvent(strEvent)); }
	void	OnCtrlTreeviewadvMouseClick				(int nID, const CString& strEvent)	{ m_OnCtrlTreeviewadvMouseClick.			SetAt(nID,new MappedEvent(strEvent)); }
	void	OnCtrlTreeviewadvContextMenuItemClick	(int nID, const CString& strEvent)	{ m_OnCtrlTreeviewadvContextMenuItemClick.	SetAt(nID,new MappedEvent(strEvent)); }
	void	OnCtrlTreeviewadvMouseDoubleClick		(int nID, const CString& strEvent)	{ m_OnCtrlTreeviewadvMouseDoubleClick.		SetAt(nID,new MappedEvent(strEvent)); }

protected:
	BOOL	HasOnValueChangedEvent					(int nID, CString& strEvent)		{ return HasEvent(m_OnValueChanged, nID, strEvent); }
	BOOL	HasOnCommandEvent						(int nID, CString& strEvent)		{ return HasEvent(m_OnCommand, nID, strEvent); }
	BOOL	HasOnUpdateCommandUI					(int nID, MappedEvent*& pEvent)		{ return HasEvent(m_OnUpdateCommandUI, nID, pEvent); }
	BOOL	HasOnCtrlStateChanged					(int nID, CString& strEvent)		{ return HasEvent(m_OnCtrlStateChanged, nID, strEvent); }

	BOOL	HasOnCtrlTreeviewadvSelectionChanged	(int nID, CString& strEvent)		{ return HasEvent(m_OnCtrlTreeviewadvSelectionChanged,		nID, strEvent); }
	BOOL	HasOnCtrlTreeviewadvItemDrag			(int nID, CString& strEvent)		{ return HasEvent(m_OnCtrlTreeviewadvItemDrag,				nID, strEvent); }
	BOOL	HasOnCtrlTreeviewadvDragOver			(int nID, CString& strEvent)		{ return HasEvent(m_OnCtrlTreeviewadvDragOver,				nID, strEvent); }
	BOOL	HasOnCtrlTreeviewadvDragDrop			(int nID, CString& strEvent)		{ return HasEvent(m_OnCtrlTreeviewadvDragDrop,				nID, strEvent); }
	BOOL	HasOnCtrlTreeviewadvMouseUp				(int nID, CString& strEvent)		{ return HasEvent(m_OnCtrlTreeviewadvMouseUp,				nID, strEvent); }
	BOOL	HasOnCtrlTreeviewadvMouseDown			(int nID, CString& strEvent)		{ return HasEvent(m_OnCtrlTreeviewadvMouseDown,				nID, strEvent); }
	BOOL	HasOnCtrlTreeviewadvMouseClick			(int nID, CString& strEvent)		{ return HasEvent(m_OnCtrlTreeviewadvMouseClick,			nID, strEvent); }
	BOOL	HasOnCtrlTreeviewadvContextMenuItemClick(int nID, CString& strEvent)		{ return HasEvent(m_OnCtrlTreeviewadvContextMenuItemClick,	nID, strEvent); }
	BOOL	HasOnCtrlTreeviewadvMouseDoubleClick	(int nID, CString& strEvent)		{ return HasEvent(m_OnCtrlTreeviewadvMouseDoubleClick,		nID, strEvent); }
	
	BOOL	HasEvent								(IDToEvent& idToEvent, int nID, CString& strEvent);
	BOOL	HasEvent								(IDToEvent& idToEvent, int nID, MappedEvent*& pEvent);
	void	ClearIdToEvent							(IDToEvent& idToEvent);

public: 
	IDToEvent	m_OnCtrlTreeviewadvSelectionChanged;
	IDToEvent	m_OnCtrlTreeviewadvItemDrag;
	IDToEvent	m_OnCtrlTreeviewadvDragOver;
	IDToEvent	m_OnCtrlTreeviewadvDragDrop;
	IDToEvent	m_OnCtrlTreeviewadvMouseUp;
	IDToEvent	m_OnCtrlTreeviewadvMouseDown;
	IDToEvent	m_OnCtrlTreeviewadvMouseClick;
	IDToEvent	m_OnCtrlTreeviewadvContextMenuItemClick;
	IDToEvent	m_OnCtrlTreeviewadvMouseDoubleClick;

private:
	IDToEvent	m_OnValueChanged;
	IDToEvent	m_OnCommand;
	IDToEvent	m_OnUpdateCommandUI;
	IDToEvent	m_Enabled;
	IDToEvent	m_OnCtrlStateChanged;

public:
// Diagnostics
#ifdef _DEBUG
	void Dump(CDumpContext& dc) const 	
	{
		ASSERT_VALID(this);
		AFX_DUMP1(dc,"\n",GetRuntimeClass()->m_lpszClassName);
	}
	void AssertValid() const
	{
		CObject::AssertValid();
	}
#endif // _DEBUG
};

//=============================================================================
class TB_EXPORT CBusinessServiceProviderClientDocObj : public CClientDoc
{
	DECLARE_DYNCREATE(CBusinessServiceProviderClientDocObj)

public:
	CBusinessServiceProviderClientDocObj(CBusinessServiceProviderObj* pBSP = NULL);

public:
	CBusinessServiceProviderObj*	GetBSP()	{	return m_pBSP; }

protected:
	TDisposablePtr<CBusinessServiceProviderObj>	m_pBSP;

};

//=============================================================================
class TB_EXPORT CBusinessServiceProviderObj : public CEventManager
{
	DECLARE_DYNAMIC(CBusinessServiceProviderObj)

	friend class CBusinessServiceProviderClientDoc;
	friend class CBusinessServiceProviderFrame;
	friend class CBusinessServiceProviderDoc;
	friend class CBusinessServiceProviderDockPane;
	CStringArray m_arVariables;
public:
	// available styles for the UI
	enum UIStyle { NONE, POPUP, BE_POPUP, STATUS_TILE, PANE, TAB_PANE, JSON_PANE };
		
public:
	// Il BSP derivato puo' fornire, oltre al documento che lo ha istanziato e il proprio namespace, anche gli elementi
	// che descrivono l'eventuale UI. La UI e' un documento in stato di BROWSE con una master view, il documento 
	// ed il frame sono fissi e predefiniti, la view va derivata da CBusinessServiceProviderView 
	// (vedi UIBusinessServiceProvider.h).
	// Se il BSP ha una UI deve fornire l'ID della bitmap del bottone da aggiungere alla toolbar accessoria, il suo 
	// command ID e il suo acceleratore. Se non li fornisce, allora si assume che il BSP non abbia UI. Si assume 
	// che il documento di UI abbia lo stesso namespace del BSP.
	// NOTA: affinche' la UI possa funzionare, e' indispensabile chiamare il metodo Init (vedi dopo)
	CBusinessServiceProviderObj
			(
						CAbstractFormDoc*			pCallerDoc,
				const	CString&					strNamespace
			);
	~CBusinessServiceProviderObj();

public:
	// Il metodo Init DEVE essere richiamato da chi istanzia il BSP nei seguenti casi:
	// - il BSP prevede una UI
	// - il BSP deve intervenire nel ciclo di vita di chi lo istanzia (es.: OnPrepareAuxData, ecc.)
	// - si vogliono associare eventi del BSP a "changed" di control. In questo caso va passata una classe MessageToEvent
	//   opportunamente reimplementata
	// NOTA: se si passa una MessageToEvent e/o si richiede una UI, implicitamente si ottiene il routing dei
	// messaggi al BSP. Nel caso il routing serva anche in assenza di MTE o di UI (es.: il BSP deve intervenire in 
	// OnOkTransaction), il BSP deve dichiarare che vuole il routing con EnableMessageRouting(TRUE) *PRIMA* che venga
	// richiamato il metodo Init (nel costruttore va bene)
	virtual void Init(MessagesToEvents* pMTEMap = NULL);

	void SetUIStyle(UIStyle aStyle); // To set the style manually

	// Call just after the constructor or in Init() to set the toolbar button (maybe it is optional)
	// This implicitly set the UIStyle = POPUP
	void SetButton
		(
				const	UINT						nButtonCmd,
				const	CString&					sButtonName,
				const	CString&					sButtonCaption,
				const	CString&					sButtonIcon,
				const	UINT						nDocAccelIDR,
				const	CString&					sToolbarName = _T(""),
				const	CString&					sButtonToolTip = _T("")
		);

	void SetJsonButton
		(
				const	UINT						nButtonCmd,
				const	UINT						nDocAccelIDR
		);

	// TODO: sarebbe da intercettare il WMCLICK da bsp della status tile 
	void SetStatusTile();
	void SetJsonPane  (UINT nPaneID = 0);

	// Call just after the constructor or in Init() to set the docking pane UI
	// This implicitly set the UIStyle = PANE (except the TAB_PANE)
	void SetPane	
		(
				const	UINT						nPaneId,
						CRuntimeClass*				pUIViewClass,
				const	CString&					sPaneName,
				const	CString&					sPaneCaption,
				const	CString&					sPaneIcon,
						CString						sTabPaneMainCaption = _T("")
		);
	// Call just after the constructor or in Init() to set the docking pane UI
	// This implicitly set the UIStyle = BE_POPUP
	// In case of this button doesn't open any window but it executes calculations, you have to reimplement OnShowUI with your functionality
	void SetBEButton	
		(
				const	UINT						nButtonCmd,
				const	CString&					sButtonName,
				const	CString&					sButtonCaption,
				const	CString&					sButtonToolTip,
				const	CString&					sButtonIcon,
				const	CString&					sBEName,
				const   UINT						nRowChangedBE = 0,
				const	UINT						nDocAccelIDR = 0
		);

	void SetBEJsonButton
		(
				const	UINT						nButtonCmd,
				const   UINT						nRowChangedBE = 0,
				const	UINT						nDocAccelIDR = 0
		);
	inline void DeclareVariable(const CString& sName, DataObj* pDataObj);
	inline void DeclareVariable(const CString& sName, DataObj& aDataObj);
	virtual void EnableBEButtonUI(BOOL bEnabled);

	// retrieve the BSP underneath the passed view (must be CBusinessServiceProviderView or CBusinessServiceProviderPaneView)
	static CBusinessServiceProviderObj* GetBSP		(CWnd* pView);

	CBusinessServiceProviderDockPane*   GetPane		()								{ return m_pUIPane; }
	void							    SetPaneSize (int initCX, int initCY)		{ m_initCX = initCX; m_initCY = initCY; }
	void								SetTabPane  () { m_UIStyle = TAB_PANE; }
	UIStyle								GetUIStyle	() { return m_UIStyle; }

protected:// Funzioni richiamabili per agire in modo analogo ad un ClientDoc
	// to add buttons to one of the toolbars 
	void AddButton
		(
					UINT						nCommandID,
			const	CString&					sButtonName,
			const	CString&					sButtonCaption,
			const	CString&					sButtonIcon,
			const	CString&					sToolbarName = _T(""),
			const	CString&					sToolbarTooltip = _T(""),
					BOOL						bDropDown = FALSE
		);
	void AddDropdownMenuItem(UINT nCommandID, UINT_PTR nIDNewItem, const CString& sNewItem, const CString& sToolBarName = _T(""));

	// aggiunta tabdialog sul documento principale
	void AddTabDialog
		(
					UINT			nTabDlgID, 
					CRuntimeClass*	pTabDlgClass, 
			const	CString&		strTabMngRTCName, 
					int				nOrdPos = -1,
					UINT			nBeforeIDD = 0

		);
	void AddTileGroup
		(
					CRuntimeClass*  pTileGroupClass,
					CString			sNameTileGroup,
					CString			sTitleTileGroup,
					UINT			nTileGroupID = 0,
					CString			sTileGroupImage = _T(""),
					CString			sTooltip = _T(""),
					int				nOrdPos = -1,
					UINT			nBeforeIDD = 0,
			const	CString&		strTileMngRTName = _T("")
		);
	void AddTileDialog
		(
					UINT				nTileDlgID,
					CRuntimeClass*		pTileDlgClass,
					const	CString&	strTileDlgTitle,
					TileDialogSize		aTileSize,
					CRuntimeClass*		pTileGroupClass,
					UINT				nBeforeIDD = 0,
					int					nFlex = -1
		);
	void AddTileDialog
		(
					UINT				nTileDlgID,
					CRuntimeClass*		pTileDlgClass,
					const	CString&	strTileDlgTitle,
					TileDialogSize		aTileSize,
					UINT				nTileGroupIDC,
					UINT				nBeforeIDD = 0,
					int					nFlex = -1
		);
	void SetDocAccelIDR(UINT nDocAccelIDR) { m_nDocAccelIDR = nDocAccelIDR; }

protected:// Funzioni reimplementabili per agire in modo analogo ad un ClientDoc 
	virtual	CAbstractFormDoc::LockStatus OnLockDocumentForNew		() { return CAbstractFormDoc::ALL_LOCKED; }
	virtual	CAbstractFormDoc::LockStatus OnLockDocumentForEdit		() { return CAbstractFormDoc::ALL_LOCKED; }
	virtual	CAbstractFormDoc::LockStatus OnLockDocumentForDelete	() { return CAbstractFormDoc::ALL_LOCKED; }
	
	virtual	BOOL OnOkDelete						() { return TRUE; }
	virtual BOOL OnOkEdit						() { return TRUE; }
	virtual BOOL OnOkNewRecord					() { return TRUE; }
	virtual BOOL OnBeforeEscape					() { return TRUE; }
	virtual BOOL OnBeforeUndoExtraction			() { return TRUE; }

	virtual	BOOL OnBeforeOkTransaction			() { return TRUE; }
	virtual	BOOL OnOkTransaction				() { return TRUE; }

	virtual	BOOL OnNewTransaction				() { return TRUE; }
	virtual	BOOL OnEditTransaction				() { return TRUE; }
	virtual	BOOL OnDeleteTransaction			() { return TRUE; } 

	virtual	BOOL OnBeforeNewTransaction			() { return TRUE; }
	virtual	BOOL OnBeforeEditTransaction		() { return TRUE; }
	virtual	BOOL OnBeforeDeleteTransaction		() { return TRUE; } 

	virtual	BOOL OnExtraNewTransaction			() { return TRUE; }
	virtual	BOOL OnExtraEditTransaction			() { return TRUE; }
	virtual	BOOL OnExtraDeleteTransaction		() { return TRUE; } 

	virtual BOOL OnBeforeBatchExecute			() { return TRUE; }
	virtual void OnDuringBatchExecute			(SqlRecord* /*pCurrProcessedRecord*/) {}
	virtual void OnAfterBatchExecute			() {}
	virtual void OnAfterSave					() {}

	virtual BOOL OnAttachData				()				{ return TRUE; }
	virtual BOOL OnPrepareAuxData			()				{ return TRUE; }
	virtual BOOL OnInitAuxData				()				{ return TRUE; }
	virtual void OnGoInBrowseMode			()				{}
	virtual void OnBeforeBrowseRecord		()				{}
	virtual void OnAfterBrowseRecord		()				{}
	virtual BOOL OnExistTables				()				{ return TRUE; }
	virtual void OnEnableControlsForFind	(DBTObject*)	{}
	virtual void OnDisableControlsForEdit	(DBTObject*)	{}
	virtual void OnDisableControlsForAddNew	(DBTObject*)	{}
	virtual void OnDisableControlsAlways	(DBTObject*)	{}
	virtual	void OnDisableControlsAlways	(CTabDialog*)   {}
	virtual	void OnDisableControlsForBatch	()				{}
	virtual void OnBeforeCloseDocument	    ();
	virtual void OnBuildDataControlLinks	(CTileDialog*)			{}
	virtual void OnBuildDataControlLinks	(CTabDialog*		)	{}
	virtual void OnBuildDataControlLinks	(CAbstractFormView* )	{}
	virtual void OnPrepareAuxData			(CAbstractFormView* )	{}
	virtual void OnLoadAttachedDocument		(CFinderDoc* pFinder)   {}

	virtual void OnSetViewFocus	(CView* pView, BOOL bActivate) {}

	// serve per agganciare TabDialog e nuovi bottoni e modificare il contesto del documento
	virtual void Customize				()		     {}
	//per aggiungere delle nuove colonne	
	virtual void CustomizeBodyEdit      (CBodyEdit* pBE) {}

	// serve per poter aggiungere degli item al menu popup associato al control
	virtual BOOL OnShowingPopupMenu		(UINT, CMenu*)					{ return TRUE; }

	// posso definire dinamicamente il contenuto dei tooltip, eventualmente ovverridando quelli statici e/o
	// quelli standard. Il formato di strMessage rispetta quanto MFC prevede per le risorse statiche: 
	// la parte fino al primo \n va nella status bar (FlyBy), mantre la restante parte va nel tooltip
	virtual BOOL OnGetToolTipText	(UINT nId, CString& strMessage)		{ return FALSE; }

	// metodi da reimplementare per agire sui DBTSlaveBuffered del documento server
	// vengono chiamati dopo il metodo corrispondente della classe DBTSlaveBuffered		
	virtual void		OnPrepareAuxColumns		(DBTSlaveBuffered*, SqlRecord*) {}
	virtual void		OnPrepareOldAuxColumns	(DBTSlaveBuffered*, SqlRecord*) {}

	virtual	void		OnSetCurrentRow		(DBTSlaveBuffered*)	{}
	virtual void		OnPrepareRow		(DBTSlaveBuffered*, int /*nRow*/, SqlRecord*)	{}
	
	virtual BOOL		OnBeforeAddRow		(DBTSlaveBuffered*, int /*nRow*/)				{ return TRUE;}
	virtual void		OnAfterAddRow		(DBTSlaveBuffered*, int /*nRow*/, SqlRecord*)	{}
	
	virtual BOOL		OnBeforeInsertRow	(DBTSlaveBuffered*, int /*nRow*/)				{ return TRUE; }
	virtual void		OnAfterInsertRow	(DBTSlaveBuffered*, int /*nRow*/, SqlRecord*)	{}

	virtual BOOL		OnBeforeDeleteRow	(DBTSlaveBuffered*, int /*nRow*/) 				{ return TRUE; }
	virtual void		OnAfterDeleteRow	(DBTSlaveBuffered*, int /*nRow*/) {}
	
	virtual DataObj*	OnCheckUserData		(DBTSlaveBuffered*, int /*nRow*/)				{ return NULL; }
	virtual DataObj*	OnCheckUserRecords	(DBTSlaveBuffered*, int& /*nRow*/)				{ return NULL; }

	virtual BOOL		OnToolbarDropDown	(UINT, CMenu& ) { return FALSE; }

	virtual void		OnAddFormsOnDockPane(CTaskBuilderDockPane* pPane) {};

	// json
	virtual BOOL		CanCreateControl		(UINT nIDC)				{ return TRUE; }
	virtual void		OnParsedControlCreated	(CParsedCtrl* pCtrl)	{ }
	virtual void		OnColumnInfoCreated		(ColumnInfo* pColInfo)	{ }
	virtual void		OnPrepareForFind(HotKeyLinkObj* pHKL, SqlRecord* pRec) {}//called to customize hotlink behavior before FindRecord
	virtual void		OnPrepareAuxData(HotKeyLinkObj* pHKL) {}//called to customize hotlink data after FindRecord

protected:
	void				UpdateUI			();
	virtual void		EnableBEButtonUI	();
	BOOL				IsUIOpened			();
	CView*				GetActiveView		();
	// Puntatore al ParsedCtrl che ha correntemente il fuoco: se bUIEnabled = TRUE torna il ParsedCtrl solo se
	// appartiene alla "lista" dei controls che abilitano la UI (EnableUIWhenFocused nella MTE), altrimenti vale per
	// tutti i controls. e' NULL se nessun ParsedCtrl ha il fuoco.
	// ATTENZIONE: il control che ha il fuoco puo' cambiare facilmente quando l'utente lavore sul documento
	// chiamante, quindi questo puntatore deve essere considerato temporaneo
	CParsedCtrl*	GetFocusedCtrl		(BOOL bUIEnabled = TRUE);
	
	// Reimplementare per abilitare / disabilitare il bottone di apertura della UI
	// NOTA: il BSP e' gia' in grado di abilitare / disabilitare il bottone se il focus va a certi particolari
	// controls, indicandoli nella MTE (EnableUIWhenFocused), a questa condizione viene messo in "and" il valore
	// di ritorno di questo metodo. Utilizzarlo per verificare condizioni "generali" di abilitazione (es.: stato del documento)
	// Se non si indicano specifici control nella MTE, l'UI risulta abilitata per tutti i control
	virtual BOOL EnableUI()	{ return TRUE; }

	// Reimplementabile per fare azioni aggiuntive prima / dopo l'apertura della UI. Il metodo viene chiamato quando
	// viene pigiato il bottone associato alla UI o il pannello diventa visibile. L'implementazione standard in caso di UI popup
	// apre il documento se chiuso, se gia' aperto lo porta in primo piano. 
	// Il ParsedCtrl e' quello su cui c'e' correntemente il fuoco (puo' essere NULL)
	// ATTENZIONE: la UI puo' rimanere aperta tanto che l'utente torna sul documento chiamante, quindi il puntatore in questione 
	// deve essere considerato temporaneo
	virtual	void OnShowUI	(CParsedCtrl* pCtrl);
    
	// Reimplementabile per fare azioni sulla chiusura della UI.
	virtual void OnUIClosed	();

	// Viene chiamata dal documento di UI una volta che si e' correttamente aperto
	// reimplementabile per fare azioni personalizzate su questo evento
	virtual void AttachUIDoc	(CBusinessServiceProviderDoc* pUIDoc);

	// Il routing dei messaggi da chi istanzia il BSP al BSP stesso e' per default disabilitato. Viene automaticamente
	// abilitato se:
	// - il BSP prevede una UI
	// - chi istanzia il BSP compila una MessageToEvent per abbinare "changed" a eventi del BSP
	// Nel caso il routing servisse in assenza di queste condizioni, il BSP puo' richiedere il routing chiamando 
	// questo metodo *PRIMA* che venga chiamato il metodo Init (va bene nel costruttore)
	void	EnableMessageRouting(BOOL bEnable = TRUE)		{ m_bMessageRoutingEnabled = bEnable; }

protected:
	CAbstractFormDoc*					m_pCallerDoc = NULL;
	CBusinessServiceProviderClientDoc*	m_pClientDoc = NULL;
	MessagesToEvents*					m_pMTEMap = NULL;
	BOOL								m_bUIAlwaysOnFront;  // dice se la UI deve sempre restare davanti al documento chiamante
	BOOL								m_bEnableUIAlwaysOnFront; // abilita / disabilita il bottone che permette di lasciare la UI in primo piano
	BOOL								m_bEnableSwitchToCaller; // abilita / disabilita il bottone che permette di tornare al documento chiamante

public:
			void								CheckEvents			(IDToEvent& aMap);
			CAbstractFormDoc*					GetCallerDoc		()					{ return m_pCallerDoc; }
			CBusinessServiceProviderClientDoc*	GetClientDoc		()					{ return m_pClientDoc; }
	virtual WebCommandType						GetWebCommandType	(UINT commandID)	{ return WEB_UNDEFINED; }
			CString								GetstrNamespace		()					{ return m_strNamespace; }

private:
	// namespace of the BSP
	CString											m_strNamespace;

	// UI Style: button or pane
	UIStyle											m_UIStyle;

	// button + popup UI
	UINT											m_nButtonCmd;
	CString											m_strButtonName;
	CString											m_strButtonCaption;
	CString											m_strButtonIcon;
	UINT											m_nDocAccelIDR;
	CString											m_strToolbarName;
	CString											m_strBEName;
	CString											m_strButtonToolTip;
	UINT											m_nRowChangedBE;
	CBEButton*										m_pBEBtn;

	// pane UI
	UINT											m_nPaneId;
	CRuntimeClass*									m_pPaneUIViewClass;
	CString											m_strPaneName;
	CString											m_strPaneCaption;
	CString											m_strPaneIcon;
	CString											m_strTabPaneMainCaption;

	BOOL											m_bMessageRoutingEnabled;
	TDisposablePtr<CBusinessServiceProviderDoc>		m_pUIDoc;
	CBusinessServiceProviderDockPane*				m_pUIPane;
	CriticalArea									m_UIOpening;			// permettono di gestire correttamente le richieste di apertura ripetute
	BOOL											m_bUIOpeningRequested;

	int												m_initCX;
	int												m_initCY;

private:
	void	CreateUIPane	();
	void	OnUIPaneSlide	(CParsedCtrl* pCtrl, BOOL bSlideOut);

protected:
	CBusinessServiceProviderDoc* GetDocument() { return m_pUIDoc; }
public:
// Diagnostics
#ifdef _DEBUG
	void Dump		(CDumpContext&)	const;
	void AssertValid()				const;
#endif // _DEBUG
};

//////////////////////////////////////////////////////////////////////////////
//						CBusinessServiceProviderClientDoc					//
//////////////////////////////////////////////////////////////////////////////
//
//=============================================================================
class TB_EXPORT CBusinessServiceProviderClientDoc : public CBusinessServiceProviderClientDocObj 
{
	DECLARE_DYNCREATE(CBusinessServiceProviderClientDoc)

	friend class CBusinessServiceProviderObj;

public:
	CBusinessServiceProviderClientDoc
		(
					CBusinessServiceProviderObj*	pBSP			= NULL,
					MessagesToEvents*				pMTEMap			= NULL
		);
	~CBusinessServiceProviderClientDoc();

protected:	
	virtual	CAbstractFormDoc::LockStatus OnLockDocumentForNew		() { return m_pBSP->OnLockDocumentForNew(); }
	virtual	CAbstractFormDoc::LockStatus OnLockDocumentForEdit		() { return m_pBSP->OnLockDocumentForEdit(); }
	virtual	CAbstractFormDoc::LockStatus OnLockDocumentForDelete	() { return m_pBSP->OnLockDocumentForDelete(); }
	
	virtual	BOOL OnOkDelete					() { return m_pBSP->OnOkDelete(); }
	virtual BOOL OnOkEdit					() { return m_pBSP->OnOkEdit(); }
	virtual BOOL OnOkNewRecord				() { return m_pBSP->OnOkNewRecord(); }
	virtual BOOL OnBeforeEscape				() { return m_pBSP->OnBeforeEscape(); }
	virtual BOOL OnBeforeUndoExtraction()		{ return m_pBSP->OnBeforeUndoExtraction(); }

	virtual	BOOL OnBeforeOkTransaction		() { return m_pBSP->OnBeforeOkTransaction(); }
	virtual	BOOL OnOkTransaction			() { return m_pBSP->OnOkTransaction(); }

	virtual	BOOL OnNewTransaction			() { return m_pBSP->OnNewTransaction(); }
	virtual	BOOL OnEditTransaction			() { return m_pBSP->OnEditTransaction(); }
	virtual	BOOL OnDeleteTransaction		() { return m_pBSP->OnDeleteTransaction(); } 

	virtual	BOOL OnBeforeNewTransaction		() { return m_pBSP->OnBeforeNewTransaction(); }
	virtual	BOOL OnBeforeEditTransaction	() { return m_pBSP->OnBeforeEditTransaction(); }
	virtual	BOOL OnBeforeDeleteTransaction	() { return m_pBSP->OnBeforeDeleteTransaction(); } 

	virtual	BOOL OnExtraNewTransaction		() { return m_pBSP->OnExtraNewTransaction(); }
	virtual	BOOL OnExtraEditTransaction		() { return m_pBSP->OnExtraEditTransaction(); }
	virtual	BOOL OnExtraDeleteTransaction	() { return m_pBSP->OnExtraDeleteTransaction(); } 

	virtual BOOL OnBeforeBatchExecute		()									{ return m_pBSP->OnBeforeBatchExecute(); }
	virtual void OnDuringBatchExecute		(SqlRecord* pCurrProcessedRecord)	{ m_pBSP->OnDuringBatchExecute(pCurrProcessedRecord);}
	virtual void OnAfterBatchExecute		()									{ m_pBSP->OnAfterBatchExecute();}
	virtual void OnAfterSave				()									{ m_pBSP->OnAfterSave(); }

	virtual void OnBuildDataControlLinks	(CTileDialog*		pTileDialog)		{ m_pBSP->OnBuildDataControlLinks(pTileDialog); }
	virtual void OnBuildDataControlLinks	(CTabDialog*		pTabDialog)			{ m_pBSP->OnBuildDataControlLinks(pTabDialog); }
	virtual void OnBuildDataControlLinks	(CAbstractFormView* pAbstractFormView)	{ m_pBSP->OnBuildDataControlLinks(pAbstractFormView); }
	virtual void OnPrepareAuxData			(CAbstractFormView* pAbstractFormView)	{ m_pBSP->OnPrepareAuxData(pAbstractFormView); }
	virtual void OnLoadAttachedDocument		(CFinderDoc* pFinder)					{ m_pBSP->OnLoadAttachedDocument(pFinder); }
	virtual void OnSetViewFocus				(CView* pView, BOOL bActivate)			{ m_pBSP->OnSetViewFocus(pView, bActivate ); }
	
	virtual BOOL OnAttachData				()								{ return m_pBSP->OnAttachData(); }
	virtual BOOL OnPrepareAuxData			()								{ return m_pBSP->OnPrepareAuxData();}
	virtual BOOL OnInitAuxData				()								{ return m_pBSP->OnInitAuxData(); }
	virtual void OnGoInBrowseMode			()								{ m_pBSP->OnGoInBrowseMode(); }
	virtual void OnBeforeBrowseRecord		()								{ m_pBSP->OnBeforeBrowseRecord(); }
	virtual void OnAfterBrowseRecord		()								{ m_pBSP->OnAfterBrowseRecord(); }
	virtual BOOL OnExistTables				()								{ return m_pBSP->OnExistTables(); }
	virtual void OnBeforeCloseDocument		()								{ m_pBSP->OnBeforeCloseDocument(); }
	virtual void OnEnableControlsForFind	(DBTObject* pDBT)				{ m_pBSP->OnEnableControlsForFind(pDBT); }
	virtual void OnDisableControlsForEdit	(DBTObject* pDBT)				{ m_pBSP->OnDisableControlsForEdit(pDBT); }
	virtual void OnDisableControlsForAddNew	(DBTObject* pDBT)				{ m_pBSP->OnDisableControlsForAddNew(pDBT); } 
	virtual void OnDisableControlsAlways	(DBTObject* pDBT)				{ m_pBSP->OnDisableControlsAlways(pDBT); }
	virtual void OnDisableControlsAlways	(CTabDialog* pTabDialog)		{ m_pBSP->OnDisableControlsAlways(pTabDialog); }
	virtual void OnDisableControlsForBatch	()								{ m_pBSP->OnDisableControlsForBatch(); }
	virtual void Customize					();
	virtual void OnFrameCreated				();
	virtual void CustomizeBodyEdit			(CBodyEdit* pBodyEdit);
	virtual BOOL OnShowingPopupMenu			(UINT nID, CMenu* pMenu)		{ return m_pBSP->OnShowingPopupMenu(nID,pMenu); }
	virtual BOOL OnGetToolTipText			(UINT nID, CString& strMessage)	{ return m_pBSP->OnGetToolTipText(nID,strMessage); }
	virtual BOOL OnToolbarDropDown			(UINT nID, CMenu& aMenu)		{ return m_pBSP->OnToolbarDropDown(nID, aMenu); }
	virtual void OnAddFormsOnDockPane		(CTaskBuilderDockPane* pPane)	{ return m_pBSP->OnAddFormsOnDockPane(pPane); };

	virtual void		OnPrepareAuxColumns		(DBTSlaveBuffered* pDBT, SqlRecord* pRec) { m_pBSP->OnPrepareAuxColumns(pDBT, pRec);}
	virtual void		OnPrepareOldAuxColumns	(DBTSlaveBuffered* pDBT, SqlRecord* pRec) { m_pBSP->OnPrepareOldAuxColumns(pDBT, pRec);}

	virtual	void		OnSetCurrentRow		(DBTSlaveBuffered* pDBT)							{ m_pBSP->OnSetCurrentRow(pDBT);}
	virtual void		OnPrepareRow		(DBTSlaveBuffered* pDBT, int nRow, SqlRecord* pRec)	{ m_pBSP->OnPrepareRow(pDBT,nRow,pRec);}
	virtual BOOL		OnBeforeAddRow		(DBTSlaveBuffered* pDBT, int nRow)					{ return  m_pBSP->OnBeforeAddRow(pDBT,nRow);}
	virtual void		OnAfterAddRow		(DBTSlaveBuffered* pDBT, int nRow, SqlRecord* pRec)	{ m_pBSP->OnAfterAddRow(pDBT,nRow,pRec);}
	virtual BOOL		OnBeforeInsertRow	(DBTSlaveBuffered* pDBT, int nRow)					{ return m_pBSP->OnBeforeInsertRow(pDBT,nRow); }
	virtual void		OnAfterInsertRow	(DBTSlaveBuffered* pDBT, int nRow, SqlRecord* pRec)	{ m_pBSP->OnAfterInsertRow(pDBT,nRow,pRec);}
	virtual BOOL		OnBeforeDeleteRow	(DBTSlaveBuffered* pDBT, int nRow) 					{ return m_pBSP->OnBeforeDeleteRow(pDBT,nRow); }
	virtual void		OnAfterDeleteRow	(DBTSlaveBuffered* pDBT, int nRow)					{ m_pBSP->OnAfterDeleteRow(pDBT,nRow);}
	virtual DataObj*	OnCheckUserData		(DBTSlaveBuffered* pDBT, int nRow)					{ return m_pBSP->OnCheckUserData(pDBT,nRow); }
	virtual DataObj*	OnCheckUserRecords	(DBTSlaveBuffered* pDBT, int& nRow)					{ return m_pBSP->OnCheckUserRecords(pDBT,nRow); }
	virtual void		OnBEEnableButton	(CBodyEdit* pBE, CBEButton* pBEButton);
	virtual WebCommandType OnGetWebCommandType(UINT commandID);

	// json
	virtual BOOL		CanCreateControl		(UINT nIDC)				{ return m_pBSP->CanCreateControl(nIDC); }
	virtual void		OnParsedControlCreated	(CParsedCtrl* pCtrl)	{ m_pBSP->OnParsedControlCreated(pCtrl); }
	virtual void		OnColumnInfoCreated		(ColumnInfo* pColInfo)	{ m_pBSP->OnColumnInfoCreated(pColInfo); }
	virtual void		OnPrepareForFind(HotKeyLinkObj* pHKL, SqlRecord* pRec) { m_pBSP->OnPrepareForFind(pHKL, pRec); }
	virtual void		OnPrepareAuxData(HotKeyLinkObj* pHKL)			{ m_pBSP->OnPrepareAuxData(pHKL); }

private:
	BOOL  IsBadCmdMsg(UINT nID);

protected:
	virtual BOOL OnCmdMsg(UINT nID, int nCode, void* pExtra, AFX_CMDHANDLERINFO* pHandlerInfo);

protected:
	CParsedCtrl*	GetFocusedCtrl			();
	BOOL			IsUIEnabledWhenFocused	(int nID);

private:
	MessagesToEvents*				m_pMTEMap;


	BOOL OnCommand(UINT nID);
protected:	
	//{{AFX_MSG(CBusinessServiceProviderClientDoc)
	afx_msg void OnControlChanged						(UINT nID);
	
	afx_msg void OnClicked								(UINT nID);
	afx_msg void OnCtrlStateChanged						(UINT nID);
	afx_msg void OnUpdateShowUI							(CCmdUI* PCCmdUI);
	afx_msg void OnCtrlTreeviewadvSelectionChanged		(UINT nID);
	afx_msg void OnCtrlTreeviewadvItemDrag				(UINT nID);
	afx_msg void OnCtrlTreeviewadvDragOver				(UINT nID);
	afx_msg void OnCtrlTreeviewadvDragDrop				(UINT nID);
	afx_msg void OnCtrlTreeviewadvMouseUp				(UINT nID);
	afx_msg void OnCtrlTreeviewadvMouseDown				(UINT nID);
	afx_msg void OnCtrlTreeviewadvMouseClick			(UINT nID);
	afx_msg void OnCtrlTreeviewadvContextMenuItemClick	(UINT nID);
	afx_msg void OnCtrlTreeviewadvMouseDoubleClick		(UINT nID);
	//}}AFX_MSG
	DECLARE_MESSAGE_MAP()
};

#include "endh.dex"
