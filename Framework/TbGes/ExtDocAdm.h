
#pragma once

//------------------------------------------------------------------------------------------
//includere alla fine degli include del .H
#include "beginh.dex"
//==========================================================================================

/////////////////////////////////////////////////////////////////////////////
//							ADM (Abstract Document Manager)				   //
/////////////////////////////////////////////////////////////////////////////
//

// Non sono state definite le opportune macros per supportare il link statico di MFC
#ifndef _AFXDLL
#error "Bisogna usare solo le extension DLLs di MFX (_AFXDLL)"
#endif

// Devo poter sapere se il ramo destro di una derivazione e` un ADM della
// classe indicata. A differenza delaa IsKindOf() non itera negli ancestor
// ma esegue un match esatto sulla base del nome della class ADM
//
#define DECLARE_ADMCLASS(adm_name) \
	public: \
		static ADMClass adm##adm_name; \
		virtual ADMClass* GetADMClass() const;

#define IMPLEMENT_ADMCLASS(adm_name) \
	extern TCHAR _lpsz##adm_name[] = _T(#adm_name); \
	ADMClass adm_name::adm##adm_name = _lpsz##adm_name; \
	ADMClass* adm_name::GetADMClass() const \
		{ return &adm_name::adm##adm_name; }

// il metodo IsADMClass simula il comportamento della IsKindOf
// ad esempio : pGenricADM->IsADMClass(ADM_CLASS(ADMContabilita))
//
#define ADM_CLASS(adm_name) \
	(&adm_name::adm##adm_name)

//=============================================================================
class TB_EXPORT ADMObj : public IDisposingSource
{
	DECLARE_ADMCLASS(ADMObj)
protected:
	CAbstractFormDoc*	m_pDocument;
	CMessages*			m_pOriginalDocMessages; // puntatore ai messages del doc originale, 
												// da ripristinare prima di chiudere l'ADM

public:
	ADMObj();
	virtual ~ADMObj();
	
public:
	BOOL				HasDocument		()	{ return m_pDocument != NULL; }
	CAbstractFormDoc*	GetDocument		()	{ ASSERT(m_pDocument); return m_pDocument; }
	SqlRecord*			GetMasterRecord	();
	void				SetWoormInfo	(CWoormInfo* pWoormInfo) { GetDocument()->SetWoormInfo(pWoormInfo); }
	CString				GetDocDefaultReport () { return GetDocument()->GetDefaultReport(); }

public:
	BOOL	IsADMClass			(ADMClass* pADMClass)		{ return pADMClass == GetADMClass(); }
	void	ADMAttach			(CAbstractFormDoc* pDoc)	{ m_pDocument = pDoc; }
	BOOL	ADMIsAborted		()							{ ASSERT(m_pDocument); return m_pDocument->m_BatchScheduler.IsAborted(); }
	BOOL	ADMFetchDocument	(BOOL bForEdit = TRUE);
	BOOL	ADMBrowseDocument	();

	//aggiunge una callback da chiamare alla distruzione del documento
	inline virtual void AddDisposingHandler (CObject* pListener, ON_DISPOSING_METHOD pHandler)		{ if (m_pDocument) m_pDocument->AddDisposingHandler(pListener, pHandler); }
	inline virtual void RemoveDisposingHandlers (CObject* pListener)								{ if (m_pDocument) m_pDocument->RemoveDisposingHandlers(pListener); }
public:
 	
 	// Metodo per permettere ai documenti ADM derivati di fare qualche cosa
 	// poco prima di andare in Browse Mode
	virtual void	ADMOnGoInBrowseMode	() {}

	// devono poter essere reimplementate
	// per sistemare ulteriori puntatori
	virtual void	ADMSetMessages		(CMessages* pMessages);
	virtual void	ADMRestoreMessages	();

	virtual	BOOL	ADMSaveDocument		();
	virtual	BOOL	ADMNewDocument		();
	virtual	BOOL	ADMEditDocument		();
	virtual	BOOL	ADMDeleteDocument	();
	virtual	BOOL	ADMOnSaveDocument	()	{ return TRUE; }
			void	ADMGoInBrowseMode	();
};

 
//=============================================================================
class TB_EXPORT ADMFrame : public CMasterFrame
{
	DECLARE_DYNCREATE(ADMFrame)
private:
	CSize m_Size;

public:		//	Constructor
	ADMFrame();
	~ADMFrame();

public:		
	
	// reimplementate per cambiare lo standard behaviour, niente toolbar e status bar
	virtual BOOL CreateStatusBar	();

	// la finestra nasce iconizzata, con il bordo da dialog
	virtual	BOOL PreCreateWindow	(CREATESTRUCT& cs);

	// reimplementata per evitare il riposizionamento della finestra
	virtual	void SetFrameSize		(CSize csDialogSize);
	virtual void OnFrameCreated		();
	
	// ProgresBar 
	void RemoveProgressBar();
	void DisableAutoProgressBar();
	void EnableAutoProgressBar();
	void SetRangeProgressBar(INT nMin, INT nMax);
	void SetPosProgressBar(INT nPos);
	INT  GetPosProgressBar();
	void SetTextProgressBar(const CString& strText);
	void AddLogButton();

private:
	BOOL m_bLogButton;

protected:
	BOOL		m_bAutoProgressBar;
	UINT_PTR	m_nTimerPtr;
	CTBStatusBarProgressBar* m_pProgressBar;
	CTaskBuilderStatusBar*   m_pStatusBar;
	CTBProgressBarButton*	 m_pProgressBarButton;

protected:
	virtual BOOL OnCreateClient(LPCREATESTRUCT lpcs, CCreateContext* pContext);
	void LogButtonAppend();

public:
	//{{AFX_MSG(ADMFrame)
	afx_msg void OnSysCommand( UINT nID, LPARAM lParam );
	afx_msg void OnActivate(UINT nState, CWnd* pWndOther, BOOL bMinimized);
	afx_msg	LRESULT OnGetControlDescription(WPARAM wParam, LPARAM lParam);
	afx_msg	LRESULT OnIsUnattendedWindow(WPARAM wParam, LPARAM lParam);
	afx_msg	void OnTimer(UINT nIDEvent);
	afx_msg	void OnDestroy();

	//}}AFX_MSG
	DECLARE_MESSAGE_MAP()
};

//=============================================================================
class TB_EXPORT ADMView : public CMasterFormView
{
	DECLARE_DYNCREATE(ADMView)

public:
	ADMView();
	~ADMView();

protected:
	void BuildDataControlLinks	() {}
	void OnInitialUpdate		();

public:
	//{{AFX_MSG(ADMView)
	//}}AFX_MSG
	DECLARE_MESSAGE_MAP() 
}; 

//=============================================================================
//								Frame senza toolbars
//=============================================================================
class TB_EXPORT AuxDataFrame : public CSlaveFrame
{
	DECLARE_DYNCREATE(AuxDataFrame)

public:		
	AuxDataFrame();
	// reimplementate per cambiare lo standard behaviour, niente toolbar e status bar
	virtual BOOL CreateStatusBar	();

	// We want a NOT WS_MAXIMIZEBOX and NOT WS_THICKFRAME window
	virtual	BOOL PreCreateWindow	(CREATESTRUCT& cs);

	virtual void OnUpdateFrameTitle	(BOOL bAddToTitle);
};

//=============================================================================
//						AuxDataFrameResizable
//=============================================================================
class TB_EXPORT AuxDataFrameResizable : public AuxDataFrame
{
	DECLARE_DYNCREATE(AuxDataFrameResizable)
public:
	AuxDataFrameResizable () {}

	virtual BOOL PreCreateWindow	(CREATESTRUCT& cs);

};

//=============================================================================
//						AuxDataFrameWithBottomToolbar
//=============================================================================
class TB_EXPORT AuxDataFrameWithBottomToolbar : public AuxDataFrame
{
	DECLARE_DYNCREATE(AuxDataFrameWithBottomToolbar)

public:
	AuxDataFrameWithBottomToolbar	();
	~AuxDataFrameWithBottomToolbar	();
	
protected:
	CTBToolBar*		m_pBottomToolbar;

protected:
	virtual BOOL	CreateAuxObjects	(CCreateContext* pCreateContext);
	virtual void	AddCustomButtons			()	{}
	virtual void	AdjustTabbedToolBar			();
	virtual BOOL OnCustomizeJsonToolBar			();
	virtual BOOL OnCustomizeTabbedToolBar(CTBTabbedToolbar* pTabbedBar);
	//{{AFX_MSG(AuxDataFrameWithBottomToolbar)
	afx_msg void OnSize(UINT nType, int cx, int cy);
	//}}AFX_MSG
	DECLARE_MESSAGE_MAP()
};

//==========================================================================================

#define POPUP_SIZE_ONE_MINI				CSize(350, 350)			// Una tile MINI
#define POPUP_SIZE_TWO_MINI				CSize(650, 350)			// Due tile MINI su stessa riga
#define POPUP_SIZE_THREE_MINI			CSize(950, 350)			// Tre tile MINI su stessa riga

#define POPUP_SIZE_WIDE_LONG			CSize(1220, 850)		// Larghezza per una tile WIDE, Altezza lunga
#define POPUP_SIZE_STD_LONG				CSize(650,	850)		// Larghezza per una tile STANDARD, Altezza lunga

#define POPUP_SIZE_WIDE_MEDIUM			CSize(1220, 600)		// Larghezza per una tile WIDE, Altezza media (Dimensione della ROW VIEW)
#define POPUP_SIZE_STD_MEDIUM			CSize(600,	600)		// Larghezza per una tile STANDARD, Altezza media 

/////////////////////////////////////////////////////////////////////////////
//				class CAuxDataSearchFrame definition
/////////////////////////////////////////////////////////////////////////////
//
//=============================================================================
class TB_EXPORT CAuxDataSearchFrame : public AuxDataFrameWithBottomToolbar
{
	DECLARE_DYNCREATE(CAuxDataSearchFrame)

protected:
	virtual void	AddCustomButtons();
	virtual void	OnAdjustFrameSize(CSize& size);
};

/////////////////////////////////////////////////////////////////////////////
//				class CAuxDataOkFrame definition
/////////////////////////////////////////////////////////////////////////////
//
//=============================================================================
class TB_EXPORT CAuxDataOkFrame : public AuxDataFrameWithBottomToolbar
{
	DECLARE_DYNCREATE(CAuxDataOkFrame)

protected:
	virtual void	AddCustomButtons();
	virtual void	OnAdjustFrameSize(CSize& size);
};

/////////////////////////////////////////////////////////////////////////////
//				class CAuxDataOkDoubleFrame definition
/////////////////////////////////////////////////////////////////////////////
//
//=============================================================================
class TB_EXPORT CAuxDataOkDoubleFrame : public CAuxDataOkFrame
{
	DECLARE_DYNCREATE(CAuxDataOkDoubleFrame)

protected:
	virtual void	OnAdjustFrameSize(CSize& size);
};

/////////////////////////////////////////////////////////////////////////////
//				class CAuxDataSearchDoubleFrame definition
/////////////////////////////////////////////////////////////////////////////
//
//=============================================================================
class TB_EXPORT CAuxDataSearchDoubleFrame : public CAuxDataSearchFrame
{
	DECLARE_DYNCREATE(CAuxDataSearchDoubleFrame)

protected:
	virtual void	OnAdjustFrameSize(CSize& size);
};

//==========================================================================================
#include "endh.dex"
