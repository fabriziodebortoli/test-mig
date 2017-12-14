
#pragma once

#include <TbGeneric\Array.h>
#include <TbGenlib\TbTreeCtrl.h>
#include <TbGenlib\AddOnMng.h>
#include <TbGenlib\baseapp.h>
#include <TbGenlib\TbExplorerInterface.h>

#include <TbNameSolver\PathFinder.h>
#include <TbNameSolver\TBNameSpaces.h>

//includere alla fine degli include del .H
#include "beginh.dex"

// costanti stringa
BEGIN_TB_STRING_MAP(StaticStrings)
	TB_LOCALIZED(USERS,		"Specific user")
	TB_LOCALIZED(ALL_USERS, "All the Users")
	TB_LOCALIZED(STANDARD,	"Original")
	TB_LOCALIZED(ALL,		"All possibilities")
END_TB_STRING_MAP()

class CTBObjDetailsArray;
class CTBExplorerUserDlg;

class CTBExplorerCache : public CObject, public CTBLockable
{
	DECLARE_DYNCREATE(CTBExplorerCache)
public:
	CTBExplorerCache()
		: m_LastUserFiltered(0)
	{
	}
	virtual LPCSTR  GetObjectName() const { return "CTBExplorerCache"; }

	CTBNamespace	m_LastUsedNameSpace;
	CString			m_LastUserConnected;
	CString			m_LastCompanyConnected;
	int				m_LastUserFiltered;
};

DECLARE_SMART_LOCK_PTR(CTBExplorerCache);

//===========================================================================
class CItemNoLoc : public CObject		// per localizzazione (parte non localizzata)
{
public: 
	CString m_strName;
	CString	m_strTitle;

public: 
	CItemNoLoc (CString strName, CString strTitle);
};

//--------------------------------------------------------------------------
class CTreeItemSel : public CObject
{
public:
	CString		m_strFullPathName;
	HTREEITEM	m_hSel;

public:
	CTreeItemSel (const CString& sFullPathName = _T(""), HTREEITEM hSel = NULL);
    
};
//--------------------------------------------------------------------------
class CTreeItemSelArray : public Array
{
public:
	int				Add				(CTreeItemSel* pEl)		{ return Array::Add (pEl); }
	CTreeItemSel* 	GetAt			(int nIdx) const		{ return (CTreeItemSel*) Array::GetAt(nIdx);	}
};

//--------------------------------------------------------------------------
class TB_EXPORT CTBMultiSelTreeExplorer : public CTBTreeCtrl 
{	
DECLARE_DYNAMIC(CTBMultiSelTreeExplorer)

public:
	BOOL m_bSave;
	BOOL m_bEditMode;

public:
	CTBMultiSelTreeExplorer(BOOL bSave = FALSE);

public:
	void GetSelectedTBExplorerObjs	(CTBObjDetailsArray*);
	BOOL CanSelect					(HTREEITEM hItem)		{ return TRUE;}

protected:
	//{{AFX_MSG(CMultiSelTree)
	virtual afx_msg void OnContextMenu	(CWnd*, CPoint);
	afx_msg void OnRButtonDown			(UINT nFlags, CPoint point);
	afx_msg void OnOpen					();
	afx_msg void OnDelete				();
	afx_msg void OnRenameLabel			();
	afx_msg void OnCutPath				();
	afx_msg void OnItemBeginEdit		(NMHDR*, LRESULT*);
	afx_msg void OnItemEndEdit			(NMHDR*, LRESULT*);
	afx_msg void OnKeyDown				(UINT nChar, UINT nRepCnt, UINT nFlags);
	afx_msg void OnVKReturn				();
	//}}AFX_MSG
	DECLARE_MESSAGE_MAP()	
};

//--------------------------------------------------------------------------
class TB_EXPORT CTBExplorer : public ITBExplorer
{
	DECLARE_DYNAMIC(CTBExplorer)
public:
	enum ForceExplorerUser
		{
			DEFAULT = 0, FORCE_USER = 1, FORCE_ADMIN = 2
		};

protected:
	TBExplorerType					m_ExplorerType;
	CTBNamespace					m_NameSpace;
	CTreeItemSelArray*				m_pSelElements;
	CTBExplorerUserDlg*				m_pTBExplorerDlg;
	BOOL							m_bIsNew;
	BOOL							m_bOnlyStdAndAllusr;
	BOOL							m_bIsMultiOpen;
	ForceExplorerUser				m_ForceExplorerUser;
	BOOL							m_bSaveForAdmin;
	BOOL							m_bCanLink;
	BOOL							m_bSaveInCurrentLanguage;

public:
   // standard constructor
	CTBExplorer(TBExplorerType aType, const CTBNamespace& aNameSpace, BOOL bIsNew = FALSE, BOOL bOnlyStdAndAllusr = FALSE, ForceExplorerUser = DEFAULT, BOOL bSaveForAdmin = FALSE);
	virtual ~CTBExplorer();

public:
	BOOL	Open					();													// apertura dell'oggetto
	void	SetMultiOpen			();								// consente la multiselezione degli oggetti
	virtual void	SetCanLink		() {m_bCanLink = TRUE;}			// consente di impostare la futura possibilità di poter importate o linkare	(per Woorm)
	virtual int	GetSavePath			(CStringArray&);				// restituzione array di stringhe delle path in cui salvare
	void	ClearStoredInfo			();								// Cancella le informazioni memorizzate relative a ultimo user e modulo selezionato

	void	GetSelPathElement		(CString& StrSelectedPath);		// restituzione stringa del path dell'oggetto selezionato
	void	GetSelPathElements		(CStringArray* aSelectedPaths); // restituzione array di stringhe dei path degli oggetti selezionati

	void	GetSelNameSpace			(CTBNamespace& Ns);				// restituzione NameSpace dell'oggetto selezionato
	void	GetSelArrayNameSpace	(CTBNamespaceArray& aNs);		// restituzione array di NameSpace degli oggetti selezionati

	BOOL	GetIsSavedInCurrentLanguage() { return m_bSaveInCurrentLanguage; }
};

//-----------------------------------------------------------------------
class CTBObjLocation : public CObject
{
public:
	CString					m_sFullFileName;	// nome con path
	CString					m_sUserName;		// nome user 
	CPathFinder::PosType	m_enPosType;		// tipo posizione(STD, ALLUSRS, USR)
	
public:
	CTBObjLocation(const CString& sUserName, const CString& sFullFileName, CPathFinder::PosType enPosType);
	CTBObjLocation(CTBObjLocation& aCopy);
	
	BOOL IsAllUser	()	{ return (m_sUserName.CompareNoCase(szAllUserDirName) == 0);}
	BOOL IsStandard	()	{ return (m_sUserName.CompareNoCase(szStandard) == 0);}
	BOOL IsUser		()	{ return (!IsStandard() && !IsAllUser());}

public: //operator
	CTBObjLocation& 	operator =(const CTBObjLocation& aCopy);
};

//-----------------------------------------------------------------------
class CTBObjLocationArray : public Array
{	
public:
	int					Add				(CTBObjLocation* pEl)	{ return Array::Add (pEl); }
	CTBObjLocation* 	GetAt			(int nIdx) const		{ return (CTBObjLocation*) Array::GetAt(nIdx);	}
	
	CTBObjLocation* 	GetLocation					(const CString& userName);
	CTBObjLocation*		GetLocationForUserDlg		();							//ritorna locazione per lo user loginato
	void				GetAllOwner					(CStringArray* pUser); 		//ritorna tutti gli user dell'oggetto
    void				GetAllLocations				(CStringArray* pLocation);	//ritorna tutte le locazioni dell'oggetto
	CTBObjLocation*		GetLocationForSpecificUser	(const CString& strUser);	//ritorna locazione per uno user specifico
	CTBObjLocation*		GetLocationStandard			();							//ritorna locazione standard

public: //operator
	CTBObjLocationArray& operator = (const CTBObjLocationArray& aCopy);
};

//--------------------------------------------------------------------------
class CTBObjDetails : public CObject
{
public:
	CString				m_sName;		//nome obj ed estesione
	CString				m_sFullName;

//private:
	CTBObjLocationArray	m_TBObjLocationArray; //array delle possibili locazioni degli oggetti

public:
	CTBObjDetails 	(const CString&);
	CTBObjDetails 	(CTBObjDetails& aCopy);
	~CTBObjDetails	();

public:
	int AddLocation (const CString& strUserName, const CString& sFullFileName, CPathFinder::PosType enPosType);
	int AddLocation (CTBObjLocation* pTBObjLocation);

	CTBObjLocation* GetLocation					(const CString& userName)	{return m_TBObjLocationArray.GetLocation(userName);}
	CTBObjLocation* GetLocationForUserDlg		()							{return m_TBObjLocationArray.GetLocationForUserDlg();}
	void			GetAllOwner					(CStringArray* pUser)		{return m_TBObjLocationArray.GetAllOwner(pUser);}		//ritorna tutti gli user dell'oggetto
 	void			GetAllLocations				(CStringArray* pLocation)	{return m_TBObjLocationArray.GetAllLocations(pLocation);}	//ritorna tutte le locazioni dell'oggetto
	CTBObjLocation*	GetLocationForSpecificUser	(const CString& strUser)	{return m_TBObjLocationArray.GetLocationForSpecificUser(strUser);}
	CTBObjLocation*	GetLocationStandard			()							{return m_TBObjLocationArray.GetLocationStandard();}

public:
	BOOL IsAllUser	(int n = 0);	
	BOOL IsStandard	(int n = 0);
	BOOL IsUser		(int n = 0);

public: //operator
	CTBObjDetails& 	operator =(const CTBObjDetails& aCopy);
};

//-----------------------------------------------------------------------
class CTBObjDetailsArray : public Array
{
public:
	int				Add				(CTBObjDetails* pEl)	{ return Array::Add (pEl);}
	CTBObjDetails* 	GetAt			(int nIdx) const		{ return (CTBObjDetails*) Array::GetAt(nIdx);}
};

//--------------------------------------------------------------------------
class CTBExplorerStatic : public CStatic
{
private:
	UINT	m_IDI;

public:
	void SetIcon		(UINT nIdResource);

protected:
	virtual void DrawItem	(LPDRAWITEMSTRUCT lpDrawItemStruct);
};

//--------------------------------------------------------------------------
class CTBExplorerUserDlg : public CParsedDialog
{
	friend class CTBMultiSelTreeExplorer;

	DECLARE_DYNAMIC(CTBExplorerUserDlg)

public:
	Array						m_aPresentObjs;		//obj presenti nel tree
    CTreeItemSelArray*			m_pSelElements;
	CTBExplorer*				m_pTBExplorer;
	CString						m_strLinkFullPath;

protected:	
	CTBNamespace				m_NameSpace;
	CTBExplorer::TBExplorerType	m_ExplorerType;		//modalità apertura dlg: EXPLORE, OPEN, SAVE

	CStringArray				m_aUsers;			//array utenti
	CStringArray				m_aSaveUsers;		//Array di proprietari per cui salvare(caso usrdlg == usr loginato)
	CStringArray				m_aFilterUser;		//array dei filtri

	CTBMultiSelTreeExplorer		m_TreeObj;
	CImageList					m_ImageList;
	CBCGPComboBox				m_ComboObj; //estensioni, tipo di oggetti
	
	CBCGPListCtrl				m_ListApplications;
	CImageList					m_AppsImageList;
	CBCGPButton					m_Btn_Ok;
	CBCGPEdit					m_EditForText;	
	CBCGPStatic					m_LabelObj;
  
	Array						m_arAppItemLoc;
	Array						m_arModItemLoc;
	CBCGPEdit					m_EditObj;			//controllo editabile
	CString						m_strEditTreeObj;	//editazione dal tree
	CString						m_strInitEditObj;	//valore iniziale del m_EditObj
	CString						m_sStatusBar;
	CString						m_sPathForCFileDlg;

	BOOL						m_bUsr;
	BOOL						m_bAllUsrs;
	BOOL						m_bStd;
	BOOL						m_bIsNew;
	BOOL						m_bIsMultiOpen;
	BOOL						m_bCanLink;
	BOOL						m_bFirstFill;
	BOOL						m_bBtnUsrPressed;
	BOOL						m_bBtnAllUsrPressed;
	BOOL						m_bBtnStdPressed;
	BOOL						m_bSaveForAllUsrs;
	BOOL						m_bSaveForStandard;
	BOOL						m_bSaveInCurrentLanguage;

public:
   // standard constructor
	CTBExplorerUserDlg			(UINT IDD, CTBNamespace& aNameSpace, CTBExplorer::TBExplorerType aExplorerType, CTreeItemSelArray* pSelElements, BOOL bIsNew, BOOL bIsMultiOpen, BOOL bCanLink);
	virtual ~CTBExplorerUserDlg	();

public:
	BOOL			FillListApps			();
	BOOL			FillModsCombo			(const CString& strLabel = _T(""), bool bFirst = FALSE );
	BOOL			FillObjCombo			();

	virtual BOOL	FillObjTreeInit			(CStringArray& aOwner);
	virtual	BOOL	FillObjTree				(CStringArray& aOwner);

	virtual	void	SetControlDlg			();
	virtual	void	LoadImageList			();

	virtual	void	SearchObjList			(CTBNamespace::NSObjectType aType, const CStringArray& aUserSel, BOOL bStd = TRUE, BOOL bAllUsrs = TRUE);
	void			SortObj					();
	void			SortUser				();
	virtual void	ApplyFilter				();

	virtual void	GetSelElements			();
	CString			GetFullPathName			(const CString& sName, const CString& sUser = _T(""), CPathFinder::PosType ePosType = CPathFinder::USERS);
	const CString&	GetTitleFromItemLoc		(const CString& strName, BOOL bIsModule = FALSE);
	
	virtual BOOL 	IsAllUser				(const CString& sName);		//path completa
	virtual BOOL 	IsStandard				(const CString& sName); 	//path completa
	virtual BOOL 	IsUser					(const CString& sName); 	//path completa

	BOOL			CanDelete				();
	virtual	BOOL	CanOpen					();
	virtual	BOOL	CanRename				();
	virtual	BOOL	CanCutPath				();

	virtual BOOL	CanDeleteObj			(const CTreeItemSel& aObj);
	virtual void	GetSelElementsAtLevel	(HTREEITEM hSel);

	virtual void	OnDelete				();
	virtual void	OnRename				(const CString& strSelItem);
	virtual void	OnRenameLabel			(const CHTreeItemsArray& aHTreeItemsArray);
	virtual	void	OnRefresh				();
	
	void			GetObjsNameForSave		(CStringArray& aFileName);
	BOOL			CanOverWrite			(const CString& strPathFile);
	void			OverWriteObject			(const CString& strPathFile);
	BOOL			OnSavePath				();
	virtual void	SaveImportedObjs		(CStringArray& aPathForSave);
	void			SaveObjs				(CStringArray& aPathForSave, CString& UsrForSave);
	BOOL			ImportCopyFile			(const CString& strTargetPath, const CString& strUsr, CStringArray* aMsg, CStringArray* aPathForSave);

	virtual void 	OnDlgOK					()	{OnOK();}
	BOOL			IsTreeCtrlEditMessage	(WPARAM KeyCode);

	virtual void	OnCustomizeToolbar		();
	virtual BOOL	OnToolbarDropDown		(UINT id, CMenu& menu);
	virtual BOOL	GetToolTipProperties    (CTooltipProperties& tp);

	BOOL			GetIsSavedInCurrentLanguage() { return m_bSaveInCurrentLanguage; }

protected:
	// Generated message map functions
	//{{AFX_MSG(CTBFileDlg)
	virtual BOOL	OnInitDialog			();
	afx_msg	void	OnComboObjChanged		();
	afx_msg	void	OnComboModsChanged		();
	afx_msg	void	OnBtnRefreshClicked 	();
	afx_msg	void	OnInsertObj				();
	afx_msg void	OnDoubleClickTree		(NMHDR *pNMHDR, LRESULT *pResult);
	afx_msg	LRESULT OnGetWebCommandType		(WPARAM wParam, LPARAM lParam);
	afx_msg void	OnEditTextChanged		();

	virtual afx_msg	void	OnBtnFilterApply		();
    virtual afx_msg void	OnItemSelectionChange	(NMHDR *pNMHDR, LRESULT *pResult);
    virtual afx_msg void	OnListSelchanged	(NMHDR *pNMHDR, LRESULT *pResult);
	
	virtual void 	OnOK					();
	virtual void 	OnCancel				();
	virtual BOOL	IsSelectedApplicationACustomization();

public:	
	afx_msg	void			OnKeyDown				(UINT nChar, UINT nRepCnt, UINT nFlags);
	virtual afx_msg	void	OnBtnFilterUsrClicked	();	
	virtual afx_msg	void	OnBtnFilterAllUsrClicked();	
	virtual afx_msg	void	OnBtnFilterStdClicked	();	
	virtual afx_msg	void	OnUpdateBtnAllUsr(CCmdUI* pCmdUI);
	virtual afx_msg	void	OnUpdateBtnStd(CCmdUI* pCmdUI);
	//}}AFX_MSG
	DECLARE_MESSAGE_MAP()
};
//--------------------------------------------------------------------------
class CTBExplorerAdminDlg : public CTBExplorerUserDlg
{
	DECLARE_DYNAMIC(CTBExplorerAdminDlg)

public:
   // standard constructor
	CTBExplorerAdminDlg(CTBNamespace& aNameSpace, CTBExplorer::TBExplorerType aExplorerType, CTreeItemSelArray* pSelElements, BOOL bIsNew, BOOL bIsMultiOpen, BOOL bCanLink, BOOL bOnlyStdAndAllusr, BOOL bSaveForAdmin);

public:
	BOOL			m_bOnlyStdAndAllusr;

protected:
	CStringArray	m_OwnerObj;
	BOOL			m_bFirst;	
	BOOL			m_bSaveForAdmin;
	
public:
	BOOL			CanOpen					();
	BOOL			CanRename				();
	BOOL			CanCutPath				();
	BOOL			CanDeleteObj			(const CTreeItemSel& aObj);
	void			GetSelElementsAtLevel	(HTREEITEM hSel);
	void			GetAllLocations			(const CString& strObjName, CStringArray* pLocation); 	//ritorna tutte le locazioni dell'oggetto

	BOOL			FillObjTreeInit			(CStringArray& aOwner);
	BOOL			FillObjTree				(CStringArray& aOwner);
	BOOL			FillUserCombo			();

	BOOL			IsAllUser				(const CString& sNamePath)	{return IsUser(sNamePath);}								//path completa
	BOOL			IsStandard				(const CString& sNamePath)  {return AfxGetPathFinder()->IsStandardPath(sNamePath);}	//path completa
	BOOL			IsUser					(const CString& sNamePath)	{return AfxGetPathFinder()->IsCustomPath(sNamePath);}	//path completa

	void			OnRefresh				();
	void			SetControlDlg			();
	void			OnCutPath				();	
	
	void			ApplyFilter				();

	void			SaveImportedObjs		(CStringArray& aPathForSave);
	void			SelTreeItem				(const CString& strItemName);
	virtual void	OnCustomizeToolbar		();

	void			FilterUsrClicked		(BOOL bPressed);
	void			FilterAllUsrClicked		(BOOL bPressed);
	void			FilterStdClicked		(BOOL bPressed);

protected:
	// Generated message map functions
	//{{AFX_MSG(CTBExplorerAdminDlg)
	BOOL			OnInitDialog			();
	afx_msg	void	OnComboUserChanged		();
	afx_msg	void	OnBtnFilterUsrClicked	();	
	afx_msg	void	OnBtnFilterAllUsrClicked();	
	afx_msg	void	OnBtnFilterStdClicked	();	
	afx_msg	void	OnBtnFilterApply		();
	afx_msg	void	OnCheckAllUsrs			();
	afx_msg	void	OnCheckStd				();
	afx_msg void	OnItemSelectionChange(NMHDR *pNMHDR, LRESULT *pResult);
	virtual afx_msg	void	OnUpdateBtnUsr		(CCmdUI* pCmdUI);
	//virtual afx_msg	void	OnUpdateBtnInsert	(CCmdUI* pCmdUI);

	virtual void 	OnOK					();
public:
	afx_msg void	OnSelAllUsr				();
	afx_msg void	OnSelStd				();
	afx_msg void	OnSelUsr				();
	//}}AFX_MSG
	DECLARE_MESSAGE_MAP()

};

//=============================================================================
class TB_EXPORT CTBExplorerFactoryUI : public CTBExplorerFactory
{
	DECLARE_DYNAMIC(CTBExplorerFactoryUI)

public:
	CTBExplorerFactoryUI() {}
	//deve avere gli stessi parametri del costruttore
	virtual ITBExplorer* CreateInstance(ITBExplorer::TBExplorerType aType, const CTBNamespace& aNameSpace, BOOL bIsNew = FALSE, BOOL bOnlyStdAndAllusr = FALSE, BOOL bIsUsr = FALSE, BOOL bSaveForAdmin = FALSE);
};

#include "endh.dex"
