
#pragma once

#include <TbGenlib\BaseTileDialog.h>
#include <TbGenlib\TbTreeCtrl.h>

#include "hotlink.h"

//includere alla fine degli include del .H
#include "beginh.dex"

////////////////////////////////////////////////////////////////////////////////
//				class CClientDocTabDlg definition
////////////////////////////////////////////////////////////////////////////////
//

#define RTC_NAME(n) _T(n)
//==============================================================================

class TB_EXPORT CClientDocTabDlg : public CObject
{
	friend class CClientDocTabDialogs;
	
public:
	UINT			m_nTabDlgID;		// identificatore della TabDlg da aggiungere
	CRuntimeClass*  m_pTabDlgClass;		// classe della TabDialog da aggiungere
	CString			m_strTabMngRTName;	// nome dell classe del TabManager a cui aggiungere la TabDialog
	int				m_nOrdPos;
	UINT			m_nBeforeIDD;		// se voglio inserire la TabDlg prima delle TabDlg identificata da m_nBeforeID
	BOOL			m_bAttached;
	CString			m_nsSelectorImage;
	CString			m_sSelectorTooltip;

public:
	CClientDocTabDlg
		(
			UINT			nTabDlgID,
			CRuntimeClass*	pTabDlgClass,
			const CString&	strTabMngRTName,
			int				nOrdPos = -1, 
			UINT			nBeforeIDD = 0,
			const CString nsSelectorImage = _T(""), 
			const CString sSelectorTooltip = _T("")

		)
	:
		m_nTabDlgID			(nTabDlgID),	
		m_pTabDlgClass		(pTabDlgClass),
		m_strTabMngRTName	(strTabMngRTName),
		m_nOrdPos			(nOrdPos),
		m_nBeforeIDD		(nBeforeIDD),
		m_bAttached			(FALSE),
		m_nsSelectorImage	(nsSelectorImage),
		m_sSelectorTooltip	(sSelectorTooltip)
	{
	}

};

////////////////////////////////////////////////////////////////////////////////
//				class CClientDocTabDialogs definition
////////////////////////////////////////////////////////////////////////////////
//
//==============================================================================
class TB_EXPORT CClientDocTabDialogs : public Array
{
public:
	// overloaded operator helpers
	CClientDocTabDlg*	GetAt		(int nIndex) const	{ return (CClientDocTabDlg*) Array::GetAt(nIndex);}
	CClientDocTabDlg*	operator[]	(int nIndex) const	{ return GetAt(nIndex);}
	CClientDocTabDlg*&	operator[]	(int nIndex)		{ return (CClientDocTabDlg*&) ElementAt(nIndex);}

	//UINT			GetTabDlgID		(int nIndex) const { return GetAt(nIndex)->m_nTabDlgID;		}
	//CRuntimeClass*	GetTabDlgClass	(int nIndex) const { return GetAt(nIndex)->m_pTabDlgClass;	}
	//CString&		GetTabMngName	(int nIndex) const { return GetAt(nIndex)->m_strTabMngRTName; }
	//int				GetOrdPos		(int nIndex) const { return GetAt(nIndex)->m_nOrdPos;		}
	//UINT			GetBeforeIDD	(int nIndex) const { return GetAt(nIndex)->m_nBeforeIDD;	}	
};     

////////////////////////////////////////////////////////////////////////////////
//				class CClientDocTileGroup definition
////////////////////////////////////////////////////////////////////////////////
//
//==============================================================================
class TB_EXPORT CClientDocTileGroup : public CObject
{
	friend class CClientDocTileGroups;
	
public:
	CRuntimeClass*  m_pTileGroupClass;
	CString			m_sNameTileGroup;
	CString			m_sTitleTileGroup; 
	CString			m_sTileGroupImage;
	CString			m_sTooltip;
	UINT			m_nTileGroupID;
	CString			m_strTileMngRTName;
	BOOL			m_bAttached;

public:
	CClientDocTileGroup
		(
			CRuntimeClass*  pTileGroupClass,
			CString			sNameTileGroup,
			CString			sTitleTileGroup, 
			CString			sTileGroupImage,
			CString			sTooltip,
			UINT			nTileGroupID,
			const	CString&		strTileMngRTName = _T("")
		)
	:
		m_pTileGroupClass (pTileGroupClass),
		m_sNameTileGroup  (sNameTileGroup),
		m_sTitleTileGroup (sTitleTileGroup),
		m_sTileGroupImage (sTileGroupImage),
		m_sTooltip		  (sTooltip),
		m_nTileGroupID    (nTileGroupID),
		m_strTileMngRTName(strTileMngRTName),
		m_bAttached		  (FALSE)
	{}
};

////////////////////////////////////////////////////////////////////////////////
//				class CClientDocTileGroups definition
////////////////////////////////////////////////////////////////////////////////
//
//==============================================================================
class TB_EXPORT CClientDocTileGroups : public Array
{
public:
	CClientDocTileGroup*	GetAt		(int nIndex) const	{ return (CClientDocTileGroup*) Array::GetAt(nIndex);}
	CClientDocTileGroup*	operator[]	(int nIndex) const	{ return GetAt(nIndex);}
	CClientDocTileGroup*&	operator[]	(int nIndex)		{ return (CClientDocTileGroup*&) ElementAt(nIndex);}
};     


////////////////////////////////////////////////////////////////////////////////
//				class CClientDocTileDialog definition
////////////////////////////////////////////////////////////////////////////////
//
//==============================================================================
class TB_EXPORT CClientDocTileDialog : public CObject
{
	friend class CClientDocTileDialogs;
	
private:
	int				m_nFlex;

public:
	UINT			m_nTileDlgID = 0;
	CRuntimeClass*  m_pTileDlgClass = NULL;		
	CString			m_strTileDlgTitle; 
	TileDialogSize	m_TileSize = TILE_STANDARD;
	CRuntimeClass*	m_pTileGroupClass = NULL;
	UINT			m_nTileGroupID = 0;
	UINT			m_nBeforeIDD = 0;		
	UINT			m_nAfterIDD = 0;
	BOOL			m_bAttached = 0;

public:
	CClientDocTileDialog
		(
					UINT			nTileDlgID,
					CRuntimeClass*	pTileDlgClass,
			const	CString&		strTileDlgTitle,
					TileDialogSize	aTileSize,
					int				nFlex,
					CRuntimeClass*	pTileGroupClass,
					UINT			nBeforeIDD = 0,
					UINT			nAfterIDD = 0
		)
	:
		m_nTileDlgID		(nTileDlgID),	
		m_pTileDlgClass		(pTileDlgClass),
		m_strTileDlgTitle	(strTileDlgTitle),
		m_TileSize			(aTileSize),
		m_nFlex				(nFlex),
		m_pTileGroupClass	(pTileGroupClass),
		m_nBeforeIDD		(nBeforeIDD),
		m_nAfterIDD			(nAfterIDD),
		m_bAttached			(FALSE)
	{}

	CClientDocTileDialog
		(
		UINT			nTileDlgID,
		CRuntimeClass*	pTileDlgClass,
		const	CString&		strTileDlgTitle,
		TileDialogSize	aTileSize,
		int				nFlex,
		UINT			nTileGroupID,
		UINT			nBeforeIDD = 0,
		UINT			nAfterIDD = 0
		)
		:
		m_nTileDlgID(nTileDlgID),
		m_pTileDlgClass(pTileDlgClass),
		m_strTileDlgTitle(strTileDlgTitle),
		m_TileSize(aTileSize),
		m_nFlex(nFlex),
		m_nTileGroupID(nTileGroupID),
		m_nBeforeIDD(nBeforeIDD),
		m_nAfterIDD(nAfterIDD),
		m_bAttached(FALSE)
	{}
};

////////////////////////////////////////////////////////////////////////////////
//				class CClientDocTileDialogs definition
////////////////////////////////////////////////////////////////////////////////
//
//==============================================================================
class TB_EXPORT CClientDocTileDialogs : public Array
{
public:
	CClientDocTileDialog*	GetAt		(int nIndex) const	{ return (CClientDocTileDialog*) Array::GetAt(nIndex);}
	CClientDocTileDialog*	operator[]	(int nIndex) const	{ return GetAt(nIndex);}
	CClientDocTileDialog*&	operator[]	(int nIndex)		{ return (CClientDocTileDialog*&) ElementAt(nIndex);}
};     

//------------------------------------------------------------------------------
#define BEGIN_CUSTOMIZE(a,b) { USES_CONVERSION; if (CString(_T(#a)).CompareNoCase(A2T((LPSTR)(LPCSTR)b->GetRuntimeClass()->m_lpszClassName)) == 0) {
#define END_CUSTOMIZE()	 }}

//------------------------------------------------------------------------------
#define BEGIN_CUSTOMIZE_BE(a,b)		BEGIN_CUSTOMIZE(a,b)
#define END_CUSTOMIZE_BE()			END_CUSTOMIZE()

//------------------------------------------------------------------------------
#define BEGIN_CUSTOMIZE_ADDLINK(a,b)	BEGIN_CUSTOMIZE(a,b)
#define END_CUSTOMIZE_ADDLINK()			END_CUSTOMIZE()

//------------------------------------------------------------------------------
#define DECLARE_VAR_CD(a, n)	if(!m_pServerDocument->m_pVariablesArray)m_pServerDocument->m_pVariablesArray = new CXMLVariableArray();m_pServerDocument->m_pVariablesArray->Add(a, n);
#define DECLARE_VAR_CD_JSON(a)	DECLARE_VAR_CD(_T(#a), m_##a)

// Oggetto aggiungibile al documento base da parte degli AddOns a cui ruotare
// messaggi e chiamate a funzioni virtuali.
//=============================================================================
//////////////////////////////////////////////////////////////////////////////
//					CClientDoc definition
//////////////////////////////////////////////////////////////////////////////
//
class ColumnInfo;
class CTBNamespace;
class CTBTreeCtrl;
class CBEButton;
class CBETooltipProperties;

class TB_EXPORT CClientDoc : public CCmdTarget, public IDisposingSourceImpl
{
	friend class CClientDocArray;
	friend class CAbstractFormDoc;
	friend class CEventManager;
	friend class CXMLEventManager;
	friend class CRadarDoc;

DECLARE_DYNCREATE(CClientDoc)

public:
	enum MsgRoutingMode { CD_MSG_BEFORE, CD_MSG_AFTER, CD_MSG_BOTH };
	enum MsgState		{ ON_BEFORE_MSG, ON_AFTER_MSG };

	CEventManager*			m_pEventManager;


protected:
	CAbstractFormDoc*	m_pServerDocument;

	//il messaggio (dal MasterDoc) viene ruotato dopo (di default viene ruotato prima)
	// vedi OnCmdMsg di CAbstractFormDoc
	MsgRoutingMode			m_MsgRouting;
	/*MsgState*/DataInt		m_MsgState;

	CClientDocTabDialogs	m_TabDialogs; // contiene le info inerenti le tabdialog aggiunte dal ClientDoc
	CClientDocTileGroups	m_TileGroups; // contiene le info inerenti le tilegroup aggiunte dal ClientDoc
	CClientDocTileDialogs	m_TileDialogs; // contiene ... mah, non saprei

	HINSTANCE				m_hResourceModule = NULL;
public:
	CTBNamespace			m_Namespace;

public:
	CClientDoc();
	~CClientDoc();

public:
	void			Init	(const CString& sDocName, const CTBNamespace& aParent);

	virtual void	Attach(CAbstractFormDoc* pDocument);
	void			Attach(CEventManager* pEvMng);
	CAbstractFormDoc*	GetMasterDocument()							{ return m_pServerDocument; }
	void			SetMsgRoutingMode(MsgRoutingMode eMsgRouting)   { m_MsgRouting = eMsgRouting; }
	MsgRoutingMode	GetMsgRoutingMode()		const					{ return m_MsgRouting; }
	void			SetMsgState(MsgState eMsgState)					{ m_MsgState = eMsgState; }
	MsgState		GetMsgState()			const					{ return (MsgState)(short)m_MsgState; }

	void			CustomizeTabber				(CTabManager*);
	virtual void	CustomizeTileGroup			(CTileGroup*);
	void			SetTabDialogImage			(UINT nTabberIDD, UINT nTabDialogIDD, CString aNsImage);
	void			SetTabTileGroupImage		(UINT nTabberIDD, UINT nTileGroupID, CString aNsImage);

	HINSTANCE		GetResourceModule()					{ return m_hResourceModule ? m_hResourceModule : GetDllInstance(GetRuntimeClass()); }
	void SetResourceModule(HINSTANCE hResourceModule)	{ m_hResourceModule = hResourceModule; }
	// DOC_DIAGNOSTIC
	CDiagnostic*	GetDiagnostic	() const { return m_pServerDocument->GetDiagnostic(); }
	
private:
	CAbstractFormView* GetViewByCtrlID(UINT nIDC);
	
protected:
	inline void DeclareVariable(const CString& sName, DataObj* pDataObj);
	inline void DeclareVariable(const CString& sName, DataObj& aDataObj);
	CTBToolBar* GetToolBar(const CString& sToolBarName);
	BOOL CreateJsonToolbar(UINT nID);
	void AttachHotLink		(HotKeyLink*, CString strName = _T(""));
	BOOL AttachDBTSlave		(DBTSlave*);

	// permette di agganciare una tabdialog ad un tabmanager (identificato tramite il "nome" della sua CRuntimeClass oppure 
	// se vuoto viene aggaciato al primo tabmanager)
	void AddTabDialog
		(
		UINT			nTabDlgID,
		CRuntimeClass*	pTabDlgClass,
		const	CString&		strTabMngRTName = _T(""),
		int				nOrdPos = -1,
		UINT			nBeforeIDD = 0,
		const CString nsSelectorImage = _T(""),
		const CString sSelectorTooltip = _T("")
		);

	void AddTileGroup
		(
		CRuntimeClass*  pTileGroupClass,
		CString			sNameTileGroup,
		CString			sTitleTileGroup,
		UINT			nTileGroupID,
		CString			sTileGroupImage = _T(""),
		CString			sTooltip = _T(""),
		int				nOrdPos = -1,
		UINT			nBeforeIDD = 0,
		const	CString&		strTileMngRTName = _T("")
		);
	void AddTileDialog
		(
		UINT			nTileDlgID,
		CRuntimeClass*	pTileDlgClass,
		const CString&	strTileDlgTitle,
		TileDialogSize	aTileSize,
		CRuntimeClass*	pTileGroupClass,
		UINT			nBeforeIDD = 0,
		int				nFlex = -1,
		UINT			nAfterIDD = 0
		);

	void AddTileDialog
		(
		UINT			nTileDlgID,
		CRuntimeClass*	pTileDlgClass,
		const CString&	strTileDlgTitle,
		TileDialogSize	aTileSize,
		UINT			nTileGroupID,
		UINT			nBeforeIDD = 0,
		int				nFlex = -1,
		UINT			nAfterIDD = 0
		);


	void AddComboBox(UINT nID, const CString& aLibNamespace, const CString& sName, int nWidth, DWORD dwStyle = ( WS_CHILD | WS_VISIBLE | CBS_NOINTEGRALHEIGHT | CBS_DROPDOWNLIST | WS_VSCROLL | CBS_SORT), const CString& sToolBarName = _T(""));
	void AddEdit (UINT nID, const CString& aLibNamespace, const CString& sName, int nWidth = 150, DWORD dwStyle = ES_AUTOHSCROLL, const CString& sToolBarName = _T(""));
	void AddLabel ( UINT nID, const CString& szText, const CString& sToolBarName = _T(""));
	void AddSeparator(const CString& sToolBarName = _T(""));
	void AddDropdownMenuItem(UINT nCommandID, UINT_PTR nIDNewItem, const CString& sNewItem, const CString& sToolBarName = _T(""));

	
	CTBToolbarButton* FindButtonPtr(UINT nCommandID, const CString& sToolBarName);


	void AddButton 
		(
			UINT nCommandID, 
			const CString& sButtonNameSpace, 
			const CString& sImageNameSpace,
			const CString& szText, 
			const CString& sToolBarName = _T(""),
			const CString& sToolTip = _T(""),
			BOOL	bDropdown = FALSE
		);

	void SetButtonInfo(UINT nID, UINT nStyle, UINT nIDImage, LPCTSTR lpszText = NULL, BOOL bPng = TRUE);
	
	CView*	CreateSlaveView	(const CRuntimeClass* pClass, const CString& strSubTitle, const CRuntimeClass* pClientClass, const CString& strFormName);
	CView*	CreateSlaveView(UINT nFrameId, CWnd* pParent =NULL, BOOL bModal = FALSE);

	// gestione degli acceleratori
	void SetDocAccel		(UINT nDocAccelIDR)		{ m_pServerDocument->SetDocAccel(nDocAccelIDR); }
	void SetDocAccel		(LPCTSTR sAccelName)	{ m_pServerDocument->SetDocAccel(sAccelName); }
	virtual BOOL PreTranslateMsg	(HWND hWnd, MSG* pMsg)	{ return FALSE; }

protected:
	virtual	CAbstractFormDoc::LockStatus OnLockDocumentForNew		() { return CAbstractFormDoc::ALL_LOCKED; }
	virtual	CAbstractFormDoc::LockStatus OnLockDocumentForEdit		() { return CAbstractFormDoc::ALL_LOCKED; }
	virtual	CAbstractFormDoc::LockStatus OnLockDocumentForDelete	() { return CAbstractFormDoc::ALL_LOCKED; }
	virtual	BOOL SaveModified			 () { return TRUE; }
	virtual	BOOL OnBeforeOkDelete		 () { return TRUE; }
	virtual	BOOL OnOkDelete				 () { return TRUE; }
	virtual BOOL OnOkEdit				 () { return TRUE; }
	virtual BOOL OnOkNewRecord			 () { return TRUE; }
	
	virtual	BOOL CanDoDeleteRecord		 () { return TRUE; }
	virtual BOOL CanDoEditRecord		 () { return TRUE; }
	virtual BOOL CanDoNewRecord			 () { return TRUE; }
	virtual	BOOL CanDoFindRecord		 ()	{ return TRUE; }
	virtual	BOOL CanDoFirstRecord		 ()	{ return TRUE; }
	virtual	BOOL CanDoPrevRecord		 ()	{ return TRUE; }
	virtual	BOOL CanDoNextRecord		 ()	{ return TRUE; }
	virtual	BOOL CanDoLastRecord		 ()	{ return TRUE; }
	virtual	BOOL CanDoQuery				 ()	{ return TRUE; }
	virtual BOOL CanDoRadar				 ()	{ return TRUE; }
	virtual BOOL CanDoExecQuery			 ()	{ return TRUE; }
	virtual BOOL CanDoEditQuery			 ()	{ return TRUE; }
	virtual BOOL CanCreateControl		(UINT idc)				{ return TRUE; }//permette di evitare la creazione del controllo o della colonna
	virtual void OnParsedControlCreated	(CParsedCtrl* pCtrl)	{  }//chiamato dopo la creazione del controllo, permette di modificarne lo stato
	virtual void OnColumnInfoCreated	(ColumnInfo* pColInfo)	{  }//chiamato dopo la creazione della colonna, permette di modificarne lo stato
	virtual void OnPinUnpin				(CBaseTileDialog* pTileDialog) {}
	virtual void OnPinUnpin				(UINT nDialogId, bool isPinned) {}
	virtual void OnUpdateTitle			(CBaseTileDialog* pTileDialog) {}
	virtual void OnPropertyCreated		(CTBProperty* pProperty) {}
	virtual	BOOL OnGetToolTipProperties	(CBETooltipProperties* pTooltip) {return FALSE;}
	virtual void EnableBodyEditButtons	(CBodyEdit* pBodyEdit) {}

	virtual	BOOL OnBeforeDeleteRecord	() { return TRUE; }
	virtual BOOL OnBeforeEditRecord		() { return TRUE; }
	virtual BOOL OnBeforeNewRecord		() { return TRUE; }

	virtual	BOOL OnBeforeOkTransaction	 () { return TRUE; }
	virtual	BOOL OnOkTransaction		 () { return TRUE; }

	virtual	BOOL OnBeforeNewTransaction		 () { return TRUE; }
	virtual	BOOL OnBeforeEditTransaction	 () { return TRUE; }
	virtual	BOOL OnBeforeDeleteTransaction	 () { return TRUE; } 

	virtual	BOOL OnNewTransaction		 () { return TRUE; }
	virtual	BOOL OnEditTransaction		 () { return TRUE; }
	virtual	BOOL OnDeleteTransaction	 () { return TRUE; } 

	virtual	BOOL OnExtraNewTransaction	 () { return TRUE; }
	virtual	BOOL OnExtraEditTransaction	 () { return TRUE; }
	virtual	BOOL OnExtraDeleteTransaction() { return TRUE; } 

	virtual BOOL OnBeforeBatchExecute	 () { return TRUE; }
	virtual void OnDuringBatchExecute	 (SqlRecord* /*pCurrProcessedRecord*/) {}
	virtual void OnAfterBatchExecute	 () {}

	//permette al clientdoc di intervenire subito dopo la DispatchLoadAttached del CFinderDoc passato come parametro
	virtual void OnLoadAttachedDocument	(CFinderDoc*) {}; 	

	virtual BOOL OnAttachData			 ()	{ return TRUE; }
	virtual BOOL OnPrepareAuxData		 ()	{ return TRUE; }

	virtual BOOL OnInitAuxData			 ()	{ return TRUE; }
	virtual void OnBeforeBrowseRecord	 () {}
	virtual void OnAfterBrowseRecord	 () {}
	virtual void OnGoInBrowseMode		 ()	{}
	virtual BOOL OnExistTables			 ()	{ return TRUE; }
	virtual void OnBeforeCloseDocument	 ()	{}
	virtual void OnCloseServerDocument	 ()	{}
	virtual	void OnDocumentCreated		 ()	{ } //default does nothing
	virtual	void OnFrameCreated			()	{ } //default does nothing
	virtual BOOL OnInitDocument			 () { return TRUE; }

	virtual void OnAfterSetFormMode		 (CBaseDocument::FormMode oldFormMode) {}; //used by EasyAttachement to know the form mode changing. The parameters cointains the old form mode value
	virtual void OnSaveCurrentRecord() {}; //used by EasyAttachement to know when the current record of server document has been changed
	
	// serve per agganciare TabDialog e nuovi bottoni e modificare il contesto del documento
	virtual void Customize				 ()	{}

	// serve per poter aggiungere degli item al menu popup associato al control
	virtual BOOL OnShowingPopupMenu		 (UINT, CMenu*)		{ return TRUE; }
	// serve per poter aggiungere degli dei parametri nel lancio di un report da un documento
	virtual BOOL OnRunReport			 (CWoormInfo*)		{ return TRUE; }

	// posso definire dinamicamente il contenuto dei tooltip, eventualmente ovverridando quelli statici e/o
	// quelli standard. Il formato di strMessage rispetta quanto MFC prevede per le risorse statiche: 
	// la parte fino al primo \n va nella status bar (FlyBy), mantre la restante parte va nel tooltip
	virtual BOOL OnGetToolTipText	(UINT nId, CString& strMessage)		{ return FALSE; }

	//per aggiungere delle nuove colonne e visualizzare un context menu	
	virtual void		CustomizeBodyEdit				(CBodyEdit*)		{}
	virtual void		CustomizeGridControl			(CTBGridControl*)	{}
	virtual BOOL		OnPostCreateClient				(CBodyEdit*)	{ return TRUE; }
	virtual BOOL		OnShowingBodyEditContextMenu	(CBodyEdit*, CMenu*, int /*nCol*/, int /*nRow*/, CPoint /*ptClient*/) { return TRUE; }

	virtual void		OnBuildingSecurityTree			(CTBTreeCtrl* pTree, Array* arInfoTreeItems)	{}
	
	// per intervenire sulla BuildDataControlLinks
	virtual void		OnInitializeUI			(const CTBNamespace& aFormNs)	{}
	virtual void		OnBuildDataControlLinks (CTabDialog*)					{}
	virtual void		OnBuildDataControlLinks (CAbstractFormView*)			{}
	virtual void		OnBuildDataControlLinks (CTileDialog*)					{}
	virtual void		OnPrepareAuxData		(CTabDialog*)					{}
	virtual void		OnPrepareAuxData		(CTileGroup*)					{}
	virtual void		OnPrepareAuxData		(UINT nID)						{}
	virtual void		OnPrepareAuxData		(CAbstractFormView*)			{}
	virtual void		OnPrepareAuxData		(CTileDialog*)					{}
	virtual void		OnDestroyTabDialog		(CTabDialog*)					{}
	virtual CString		OnGetCaption			(CAbstractFormView*)			{ return _T(""); }

	//used by those client documents attached to CTBActivityDocument
	virtual BOOL		OnBeforeLoadDBT			() { return TRUE; }
	virtual BOOL		OnLoadDBT				() { return TRUE;}
	virtual BOOL		OnAfterLoadDBT			() { return TRUE; }
	virtual BOOL		OnBeforeUndoExtraction	() { return TRUE; }
	///////////////////////////

	//per cambiare il colore alle celle dei bodyedit
	virtual BOOL		OnGetCustomColor 
								(
									const CBodyEdit*, 
									CBodyEditRowSelected* /*CurRow*/
								) 
									{ return FALSE; }
	virtual BOOL		OnDblClick 
								(
									const CBodyEdit*,
									UINT /*nFlags*/, 
									CBodyEditRowSelected* /*CurRow*/
								) 
									{ return FALSE; }

	virtual  void OnBESelCell		(CBodyEdit*, SqlRecord* , ColumnInfo* )  {}
	virtual  void OnBEShowCtrl		(CBodyEdit*, SqlRecord* , ColumnInfo* )  {}
	virtual  void OnBEHideCtrl		(CBodyEdit*, SqlRecord* , ColumnInfo* )  {}
	virtual  void OnBEEnableButton	(CBodyEdit*, CBEButton*) {}

	virtual BOOL OnBEBeginMultipleSel		(CBodyEdit*) { return FALSE; }
	virtual BOOL OnBEEndMultipleSel			(CBodyEdit*) { return FALSE; }
	virtual	BOOL OnBECustomizeSelections	(CBodyEdit*, SelArray&) { return FALSE; }
	virtual BOOL OnBECandDoDeleteRow		(CBodyEdit*) { return TRUE; }

	virtual BOOL		OnEnableTabSelChanging(UINT /*nTabber*/, UINT /*nFromIDD*/, UINT /*nToIDD*/) { return TRUE; }
	virtual void		OnTabSelChanged(UINT /*nTabber*/, UINT /*nTabIDD*/) { }
	virtual BOOL		OnToolbarDropDown (UINT, CMenu& ) { return FALSE; }

protected:
	// metodi da reimplementare per agire sui control associati ai campi gestiti da un 
	// qualcunque DBT (master/slave/slavebuffere) del docuememto server
	// vengono chiamati dopo il metodo corrispondente della classe DBTObject
	virtual void		OnEnableControlsForFind		(DBTObject*) {}
	virtual void		OnDisableControlsForEdit	(DBTObject*) {}
	virtual void		OnDisableControlsForAddNew	(DBTObject*) {}
	virtual void		OnDisableControlsAlways		(DBTObject*) {}

	virtual	void		OnDisableControlsForBatch	()			 {}
	virtual	void		OnDisableControlsForAddNew	()			 {}
	virtual	void		OnDisableControlsForEdit	()			 {}
	virtual	void		OnEnableControlsForFind		()			 {}
	virtual	void		OnDisableControlsAlways		()			 {}
	virtual	void		OnDisableControlsAlways		(CTabDialog*){}

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

	virtual void OnPreparePrimaryKey	(DBTObject*) {};
	virtual void OnPreparePrimaryKey	(DBTSlaveBuffered*, int /*nRow*/, SqlRecord*) {};

public:
	virtual void		OnBodyEditRowView	(CBodyEdit* pBodyEdit) {}
	virtual void		OnRowFormViewDied	(CRowFormView* pRowFormView){}

	virtual CManagedDocComponentObj*	GetComponent	(CString& sParentNamespace, CString& sName);
	virtual void						GetComponents	(CManagedDocComponentObj* pRequest, Array& returnedComponens);

protected:
	virtual DataObj*	OnCheckUserData		(DBTSlaveBuffered*, int /*nRow*/)				{ return NULL; }
	virtual DataObj*	OnCheckUserRecords	(DBTSlaveBuffered*, int& /*nRow*/)				{ return NULL; }
	
	// posso aggiungere dei filtri alla query del dbt passato come argomento
	virtual void		OnModifyDBTDefineQuery	(DBTObject*, SqlTable*)	{}
	virtual void		OnModifyDBTPrepareQuery	(DBTObject*, SqlTable*) {} 
	virtual void		OnPrepareBrowser		(SqlTable*)				{}
	virtual void		OnPrepareFindQuery		(SqlTable*)				{}
	virtual void		OnAfterCreateAndInitDBT (DBTObject*)			{} //consente di poter intervenire alla fine del processo di inizializzazione di un DBTObject

	//per intervenire prima o dopo la registrazione del DBT (serve per poter aggiungere a RunTime dei 
	virtual void		OnBeforeRegisterDBT		(DBTObject*) {}

	// posso aggiungere dei filtri alla query dell'hotlink passago come argomento
	virtual void		OnModifyHKLDefineQuery	(HotKeyLink*, SqlTable*, HotKeyLink::SelectionType = HotKeyLink::DIRECT_ACCESS) {}
	virtual void		OnModifyHKLPrepareQuery	(HotKeyLink*, SqlTable*, DataObj*, HotKeyLink::SelectionType  = HotKeyLink::DIRECT_ACCESS) {}
	virtual int			OnModifyHKLSearchComboQueryData (HotKeyLink*, const int& nMaxItems, DataObjArray& pKeyData, CStringArray& arDescriptions) { return 0; }

	// posso intervenire durante la fase di run dell'hotlink
	virtual	void OnBeforeCallLink	(CParsedCtrl* pCtrl) {}
	virtual	void OnHotLinkRun		() {}
	virtual	void OnHotLinkStop		() {}

	virtual BOOL			OnHKLIsValid				(HotKeyLink* pHotKeyLink)						{ return TRUE; }
	virtual BOOL			OnValidateRadarSelection	(SqlRecord*)									{ return TRUE; }
	virtual BOOL			OnValidateRadarSelection	(SqlRecord* pRec, HotKeyLink* pHotKeyLink)		{ return OnValidateRadarSelection(pRec); }
	virtual BOOL			OnValidateRadarSelection	(SqlRecord*, CTBNamespace nsHotLinkNamespace)   { return TRUE; }
	virtual BOOL			OnValidateRadarSelection	(SqlRecord* pRec, CTBNamespace nsHotLinkNamespace, HotKeyLink* pHotKeyLink)   { return OnValidateRadarSelection(pRec); }
	virtual CParsedCtrl*	OnCreateParsedCtrl			(UINT /*nIDC*/, CRuntimeClass* /*pParsedCtrlClass*/)	{ return NULL; }
	virtual BOOL			OnShowStatusBarMsg			(CString&)										{ return FALSE; }
	virtual void			OnBeforeSave				()												{ }
	virtual void			OnAfterSave					()												{ }
	virtual void			OnBeforeDelete				()												{ }
	virtual void			OnAfterDelete				()												{ }

	//for CWizardFormDoc attach
	virtual	LRESULT	OnWizardNext   			(UINT nDlgIDD) { return WIZARD_DEFAULT_TAB; }
	virtual LRESULT	OnWizardBack   			(UINT nDlgIDD) { return WIZARD_DEFAULT_TAB; }
	virtual LRESULT OnGetBitmapID  			(UINT nDlgIDD) { return WIZARD_DEFAULT_TAB; } 
	virtual LRESULT	OnWizardFinish 			(UINT nDlgIDD) { return WIZARD_DEFAULT_TAB; }
	virtual LRESULT	OnWizardCancel 			(UINT nDlgIDD) { return WIZARD_DEFAULT_TAB; }
	virtual LRESULT	OnBeforeWizardFinish	(UINT nDlgIDD) { return WIZARD_DEFAULT_TAB; }

	virtual void	OnUpdateWizardButtons(UINT nDlgIDD)	{}
	virtual void	OnWizardActivate	 (UINT nDlgIDD)	{}
	virtual void	OnWizardDeactivate	 (UINT nDlgIDD)	{}
	virtual void OnPrepareForFind(HotKeyLinkObj* pHKL, SqlRecord* pRec) {}//called to customize hotlink behavior before FindRecord
	virtual void OnPrepareAuxData(HotKeyLinkObj* pHKL) {}//called to customize hotlink data after FindRecord

	//---- Drop-Paste events
	virtual BOOL OnPasteDBTRows			(CTBEDataCoDecPastedRecord& /*pr*/) { return TRUE; }

	virtual BOOL OnValidatePasteDBTRows (RecordArray& arRows, CTBEDataCoDecRecordToValidate& vr)
					{
						for (int i= 0; i < arRows.GetSize(); i++)
							if (!OnValidatePasteDBTRows(arRows.GetAt(i), vr))
								return FALSE;
						return TRUE;
					}

	virtual BOOL OnValidatePasteDBTRows	(SqlRecord* pRec, CTBEDataCoDecRecordToValidate&) { return TRUE; }
	virtual void OnAddFormsOnDockPane(CTaskBuilderDockPane* pPane) {};

public:
	virtual void 		OnBeforeXMLImport	() {}
	virtual void 		OnAfterXMLImport	() {}
	virtual void 		OnBeforeXMLExport	() {}
	virtual void 		OnAfterXMLExport	() {}

	virtual void		OnActivate			(CAbstractFormFrame* pFrame, UINT nState, CWnd* pWndOther, BOOL bMinimized) {}
	
	virtual  BOOL		OnBeforeEscape		() { return TRUE; }
	
	virtual SymTable*	GetSymTable			() { return m_pServerDocument ? m_pServerDocument->GetSymTable() : NULL; }

protected:
	virtual WebCommandType OnGetWebCommandType(UINT commandID) { return WEB_UNDEFINED; }

	//TBSCRIPT - special case of OnCmdMsg, avoid MESSAGE_MAP use
	virtual	BOOL OnValueChanged			(CString sControlNamespace) { return FALSE; } //reserved, DO NOT OVERRIDE IT 
	virtual	BOOL OnValueChangedForFind	(CString sControlNamespace) { return FALSE; } //reserved, DO NOT OVERRIDE IT 
	virtual	BOOL OnClicked				(CString sControlNamespace) { return FALSE; } //reserved, DO NOT OVERRIDE IT 
	virtual	BOOL OnControlStateChanged	(CString sControlNamespace) { return FALSE; } //reserved, DO NOT OVERRIDE IT 
	virtual	BOOL OnRowChanged			(CString sControlNamespace) { return FALSE; } //reserved, DO NOT OVERRIDE IT 

	virtual CRuntimeClass* OnModifySqlRecordClass	(DBTObject*, const CString& /*sDBTName*/, CRuntimeClass* pSqlRecordClass) { return pSqlRecordClass; }

protected: //for DMS
	virtual void OnDMSEvent(DMSEventTypeEnum eventType, int eventKey) {}
	public:
		BOOL NamespaceEquals(const CString& aCDNamespace);
};

//////////////////////////////////////////////////////////////////////////////
//					CClientDocArray definition
//////////////////////////////////////////////////////////////////////////////
//
class TB_EXPORT CClientDocArray : public Array
{
	friend class CAbstractDoc;
	friend class CAbstractFormDoc;
	friend class CWizardFormDoc;
	friend class CAbstractFormFrame;
	friend class DBTObject;
	friend class DBTMaster;
	friend class DBTSlave;
	friend class DBTSlaveBuffered;
	friend class DBTTree;
	friend class ADMObj;
	friend class CTabDialog;
	friend class CJsonFormEngine;
	friend class CJsonContext;
	friend class CJsonBodyEdit;
	friend class CJsonPropertyGrid;
	friend class CTileDialog;
	friend class CTileGroup;
	friend class CAbstractFormView;

public:
	CClientDoc* GetAt	(int nIndex) const			{ return (CClientDoc*) Array::GetAt(nIndex);}
	void		Attach	(CAbstractFormDoc* pDocument);

	// normalmente prima di associare il ClientDoc controllo che non sia stato già agganciato, ma posso inibire
	// questo comportamento se attacco il ClientDoc in modo esplicito
	int			Add		(CClientDoc* pClientDoc, BOOL bCheckDuplicates = TRUE); 

protected:
	CAbstractFormDoc::LockStatus  OnLockDocumentForNew		();
	CAbstractFormDoc::LockStatus  OnLockDocumentForEdit		();
	CAbstractFormDoc::LockStatus  OnLockDocumentForDelete	();

	BOOL OnBeforeOkDelete				 ();
	BOOL OnOkDelete						 ();
	BOOL OnOkEdit						 ();
	BOOL OnOkNewRecord					 ();
	BOOL OnBeforeOkTransaction			 ();
	BOOL SaveModified					 ();
	BOOL OnOkTransaction				 ();
	BOOL OnBeforeNewTransaction			 ();
	BOOL OnBeforeEditTransaction		 ();
	BOOL OnBeforeDeleteTransaction		 ();
	BOOL OnEditTransaction				 ();
	BOOL OnNewTransaction				 ();
	BOOL OnDeleteTransaction			 ();
	BOOL OnExtraEditTransaction			 ();
	BOOL OnExtraNewTransaction			 ();
	BOOL OnExtraDeleteTransaction		 ();

	BOOL CanDoDeleteRecord		();
	BOOL CanDoEditRecord		();
	BOOL CanDoNewRecord			();
	BOOL CanDoFindRecord		();
	BOOL CanDoFirstRecord		();
	BOOL CanDoPrevRecord		();
	BOOL CanDoNextRecord		();
	BOOL CanDoLastRecord		();
	BOOL CanDoQuery				();
	BOOL CanDoRadar				();
	BOOL CanDoExecQuery			();
	BOOL CanDoEditQuery			();

	BOOL OnBeforeDeleteRecord	();
	BOOL OnBeforeEditRecord		();
	BOOL OnBeforeNewRecord		();


	void OnBeforeBrowseRecord	();
	void OnAfterBrowseRecord	();
	void OnGoInBrowseMode		();
	BOOL OnBeforeBatchExecute	();
	void OnDuringBatchExecute	(SqlRecord* pCurrProcessedRecord);
	void OnAfterBatchExecute	();

	void OnLoadAttachedDocument	(CFinderDoc*);

	void CustomizeTabber				(CTabManager*);
	void CustomizeTileGroup				(CTileGroup*);
	BOOL PreTranslateMsg				(HWND hWnd, MSG* pMsg);

	void CustomizeBodyEdit				(CBodyEdit*);
	void CustomizeGridControl			(CTBGridControl*);
	BOOL OnPostCreateClient				(CBodyEdit*);
	BOOL OnShowingBodyEditContextMenu	(CBodyEdit*, CMenu*, int /*nCol*/, int /*nRow*/, CPoint /*ptClient*/);


	void OnInitializeUI			(const CTBNamespace& aFormNs);
	void OnBuildDataControlLinks(CTabDialog*);
	void OnBuildDataControlLinks(CAbstractFormView*);
	void OnBuildDataControlLinks(CTileDialog*);
	void OnPrepareAuxData		(UINT nID);
	void OnPrepareAuxData		(CTileGroup*);
	void OnPrepareAuxData		(CTabDialog*);
	void OnPrepareAuxData		(CAbstractFormView*);
	void OnPrepareAuxData		(CTileDialog*);
	void OnDestroyTabDialog		(CTabDialog*);
	
	BOOL OnAttachData			();
	BOOL OnPrepareAuxData		();

	BOOL OnInitAuxData			();
	BOOL OnExistTables			();
	void OnBeforeCloseDocument	();
	void OnCloseServerDocument	();
	void OnDocumentCreated		();
	void OnFrameCreated			();
	BOOL OnInitDocument			();
	void OnAfterSetFormMode		(CBaseDocument::FormMode oldFormMode);
	void OnSaveCurrentRecord();

	void OnEnableControlsForFind	(DBTObject*);
	void OnDisableControlsForEdit	(DBTObject*);
	void OnDisableControlsForAddNew	(DBTObject*);
	void OnDisableControlsAlways	(DBTObject*);

	void OnDisableControlsForBatch	();
	void OnDisableControlsForAddNew	();
	void OnDisableControlsForEdit	();
	void OnEnableControlsForFind	();
	void OnDisableControlsAlways	();
	void OnDisableControlsAlways	(CTabDialog*);

	void OnPreparePrimaryKey	(DBTObject*);
	void OnPreparePrimaryKey	(DBTSlaveBuffered*, int /*nRow*/, SqlRecord*);

	void OnPrepareAuxColumns	(DBTSlaveBuffered*, SqlRecord*);
	void OnPrepareOldAuxColumns	(DBTSlaveBuffered*, SqlRecord*);

	void OnSetCurrentRow		(DBTSlaveBuffered*);
	void OnPrepareRow			(DBTSlaveBuffered*, int /*nRow*/, SqlRecord*);
	
	BOOL OnBeforeAddRow			(DBTSlaveBuffered*, int /*nRow*/);
	void OnAfterAddRow			(DBTSlaveBuffered*, int /*nRow*/, SqlRecord*);
	
	BOOL OnBeforeInsertRow		(DBTSlaveBuffered*, int /*nRow*/);
	void OnAfterInsertRow		(DBTSlaveBuffered*, int /*nRow*/, SqlRecord*);

	void OnAddFormsOnDockPane	(CTaskBuilderDockPane* pPane);
	BOOL CanCreateControl		(UINT idc);
	void OnParsedControlCreated	(CParsedCtrl* pCtrl);
	void OnColumnInfoCreated	(ColumnInfo* pColInfo);
	void OnPinUnpin				(CBaseTileDialog* pTileDialog);
	void OnPinUnpin				(UINT nDialogId, bool isPinned);
	void OnUpdateTitle			(CBaseTileDialog* pTileDialog);
	void OnPropertyCreated		(CTBProperty * pProperty);
	BOOL OnGetToolTipProperties	(CBETooltipProperties* pTooltip);
	void EnableBodyEditButtons	(CBodyEdit* pBodyEdit);
	CString	OnGetCaption		(CAbstractFormView*);

public:
	void OnBodyEditRowView		(CBodyEdit* pBodyEdit);
	void OnRowFormViewDied		(CRowFormView* pRowFormView);

	CManagedDocComponentObj*	GetComponent	(CString& sParentNamespace, CString& sName);
	void						GetComponents	(CManagedDocComponentObj* pRequest, Array& returnedComponens);

	//used by those client documents attached to CTBActivityDocument
	BOOL OnBeforeLoadDBT();
	BOOL OnLoadDBT();
	BOOL OnAfterLoadDBT();
	BOOL OnBeforeUndoExtraction();

protected:
	BOOL OnBeforeDeleteRow		(DBTSlaveBuffered*, int /*nRow*/);
	void OnAfterDeleteRow		(DBTSlaveBuffered*, int /*nRow*/);
	
	DataObj* OnCheckUserData	(DBTSlaveBuffered*, int /*nRow*/);
	DataObj* OnCheckUserRecords	(DBTSlaveBuffered*, int& /*nRow*/);

	void OnModifyDBTDefineQuery	(DBTObject*, SqlTable*);
	void OnModifyDBTPrepareQuery(DBTObject*, SqlTable*);
	void OnPrepareBrowser		(SqlTable*);
	void OnPrepareFindQuery		(SqlTable*);
	void OnAfterCreateAndInitDBT(DBTObject*);

	void OnModifyHKLDefineQuery	(HotKeyLink*, SqlTable*, HotKeyLink::SelectionType nQuerySelection = HotKeyLink::DIRECT_ACCESS);
	void OnModifyHKLPrepareQuery(HotKeyLink*, SqlTable*, DataObj*, HotKeyLink::SelectionType nQuerySelection = HotKeyLink::DIRECT_ACCESS);
	int  OnModifyHKLSearchComboQueryData(HotKeyLink*, const int& nMaxItems, DataObjArray& pKeyData, CStringArray& arDescriptions);

	void OnBeforeCallLink	(CParsedCtrl* pCtrl);
	void OnHotLinkRun		();
	void OnHotLinkStop		();

	BOOL OnShowingPopupMenu		(UINT, CMenu*);
	BOOL OnRunReport			(CWoormInfo*);

	BOOL OnGetToolTipText		(UINT nId, CString& strMessage);

	BOOL OnGetCustomColor		(const CBodyEdit*, CBodyEditRowSelected* /*CurRow*/);
	BOOL OnDblClick				(const CBodyEdit*, UINT /*nFlags*/, CBodyEditRowSelected* /*CurRow*/); 
	void OnBESelCell			(CBodyEdit*, SqlRecord* pCurRec, ColumnInfo* pCol);
	void OnBEShowCtrl			(CBodyEdit*, SqlRecord* pCurRec, ColumnInfo* pCol);
	void OnBEHideCtrl			(CBodyEdit*, SqlRecord* pCurRec, ColumnInfo* pCol);
	void OnBEEnableButton		(CBodyEdit*, CBEButton*);

	BOOL OnBEBeginMultipleSel	(CBodyEdit*);
	BOOL OnBEEndMultipleSel		(CBodyEdit*);
	BOOL OnBECustomizeSelections(CBodyEdit*, SelArray& sel);

	BOOL OnBECandDoDeleteRow	(CBodyEdit*);

	BOOL OnEnableTabSelChanging	(UINT /*nTabber*/, UINT /*nFromIDD*/, UINT /*nToIDD*/);
	void OnTabSelChanged		(UINT /*nTabber*/, UINT /*nTabIDD*/);

	BOOL OnToolbarDropDown		(UINT nID, CMenu&);

	BOOL OnHKLIsValid				(HotKeyLink*);
	BOOL OnValidateRadarSelection	(SqlRecord* pRec);
	BOOL OnValidateRadarSelection	(SqlRecord* pRec, HotKeyLink* pHotKeyLink);
	BOOL OnValidateRadarSelection	(SqlRecord* pRec, CTBNamespace nsHotLinkNamespace);
	BOOL OnValidateRadarSelection	(SqlRecord* pRec, CTBNamespace nsHotLinkNamespace, HotKeyLink* pHotKeyLink);
	void OnPrepareForFind(HotKeyLinkObj* pHKL, SqlRecord* pRec);
	void OnPrepareAuxData(HotKeyLinkObj* pHKL);

	BOOL OnShowStatusBarMsg			(CString& sMsg);

	//Metodi usati come notifica per inizio e fine di operazioni di save o delete di record
	void OnBeforeSave				();
	void OnAfterSave				();
	void OnBeforeDelete				();
	void OnAfterDelete				();

	CParsedCtrl* OnCreateParsedCtrl	(UINT /*nIDC*/, CRuntimeClass* /*pParsedCtrlClass*/);

	//for CWizardFormDoc attach
	LRESULT	OnWizardNext   			(UINT nDlgIDD); 
	LRESULT	OnWizardBack   			(UINT nDlgIDD); 
	LRESULT OnGetBitmapID  			(UINT nDlgIDD);  
	LRESULT	OnWizardFinish 			(UINT nDlgIDD); 
	LRESULT	OnWizardCancel 			(UINT nDlgIDD); 
	LRESULT	OnBeforeWizardFinish	(UINT nDlgIDD); 

	void	OnUpdateWizardButtons(UINT nDlgIDD);
	void	OnWizardActivate	 (UINT nDlgIDD);
	void	OnWizardDeactivate	 (UINT nDlgIDD);

	BOOL OnPasteDBTRows			(CTBEDataCoDecPastedRecord&);
	BOOL OnValidatePasteDBTRows (RecordArray& arRows,	CTBEDataCoDecRecordToValidate&);
	BOOL OnValidatePasteDBTRows (SqlRecord* pRec,		CTBEDataCoDecRecordToValidate&);
	
	BOOL OnBeforeEscape			();
	void OnBuildingSecurityTree	(CTBTreeCtrl* pTree, Array* arInfoTreeItems);

public:
	void 	OnBeforeXMLImport	();
	void 	OnAfterXMLImport	();
	void 	OnBeforeXMLExport	();
	void 	OnAfterXMLExport	();

	void OnActivate	(CAbstractFormFrame* pFrame, UINT nState, CWnd* pWndOther, BOOL bMinimized);

	CRuntimeClass* OnModifySqlRecordClass	(DBTObject*, const CString&, CRuntimeClass*);
	
	//for DMS
	void OnDMSEvent(DMSEventTypeEnum eventType, int eventKey);
};

//client document di ausilio per eventuali filtri sul documento. Vedi Data Synchronizazion: serve per capire se un documento partecipa
//al processo di sincronizzazione in base ai filtri gestionali. Questi filtri sono salvati su una tabella di ERP e sono selezionabili 
//dall'utente in fase di sincronizzazione dati massiva
//esempio: sincronizzo solo gli articoli di una determinata categoria
//questo significa che quando vado ad aggiungere un nuovo articolo questo va sincronizzato solo se  verifica la condizione sulla categoria. 
//Idem se modifico l'articolo
//Per ottimizzazione i filtri sono utilizzati in memoria
// mi baso su un array di variabili
//=============================================================================
//////////////////////////////////////////////////////////////////////////////
//					CDFilterManager definition
//////////////////////////////////////////////////////////////////////////////
//
class TB_EXPORT CDFilterManager : public CClientDoc
{
	DECLARE_DYNCREATE(CDFilterManager)

public:
	Array*			m_pFilterXMLVariableList; //lista CXMLVariableArray ciascuno identificato da un nome univoco

											  //CXMLVariableArray* m_pVariableArray;

public:
	CDFilterManager();
	~CDFilterManager();

public:
	void AddVariableArray(const CString& filterName, CXMLVariableArray*);
	void ParseVariable(const CString& filterName, const CString& strXMLFilter);
	CXMLVariableArray* GetVariableArray(const CString& filterName);

public:
	//controlla se il record passato come parametro verifica o meno la condizione imposta dai filtri. La condizione deve essere implementata dalle classi derivate
	// non lo posso fare pure virtual poichè la classe deve essere dyncreate
	virtual BOOL CheckFilterCondition(SqlRecord* pRec, const CString& filterName) { return TRUE; }
};

//==============================================================================
#include "endh.dex"
