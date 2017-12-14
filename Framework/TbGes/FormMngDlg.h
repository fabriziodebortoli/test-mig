
#pragma once

#include <TbGenlib\TbExplorerInterface.h>
#include <TbGenlib\PARSLBX.H>
#include <TbGenlib\TbTreeCtrl.h>

#include "FormMng.h"

//includere alla fine degli include del .H
#include "beginh.dex"

class CPropertyReportDlg;

/////////////////////////////////////////////////////////////////////////////
//							CFormMngCache
///////////////////////////////////////////////////////////////////////////////
//

class CAdminToolDocReportCache : public CObject, public CTBLockable
{
	DECLARE_DYNCREATE(CAdminToolDocReportCache)
public:
	CTBNamespace	m_LastUsedNameSpace;
	CString			m_LastCompanyConnected;
	CString			m_LastUserConnected;
	virtual LPCSTR  GetObjectName() const { return "CAdminToolDocReportCache"; }

};

DECLARE_SMART_LOCK_PTR(CAdminToolDocReportCache);
	

/////////////////////////////////////////////////////////////////////////////
//							CTreeFormReportCtrl
///////////////////////////////////////////////////////////////////////////////
//
class CTreeFormReportCtrl : public CTBTreeCtrl 
{	
	DECLARE_DYNAMIC(CTreeFormReportCtrl)

protected:
	BOOL				m_bAfterCtrl;
	CLocalizableMenu	m_Menu;

public:
	CTreeFormReportCtrl();

protected:
	//{{AFX_MSG(CTreeFormReportCtrl)
	afx_msg void OnItemBeginEdit	(NMHDR*, LRESULT*);
	afx_msg void OnItemEndEdit		(NMHDR*, LRESULT*);
	afx_msg void OnRButtonDown		(UINT nFlags, CPoint point);
	afx_msg void OnContextMenu		(CWnd*, CPoint);
	afx_msg void OnKeyDown			(UINT nChar, UINT nRepCnt, UINT nFlags);
	afx_msg void OnKeyUp			(UINT nChar, UINT nRepCnt, UINT nFlags);
	afx_msg void OnSetAsDefault		();
	afx_msg void OnDelete			();
	afx_msg void OnRename			();
	afx_msg void OnProperty			();
	//}}AFX_MSG
	DECLARE_MESSAGE_MAP()	
};

/////////////////////////////////////////////////////////////////////////////
//							CFormReportDlg
///////////////////////////////////////////////////////////////////////////////
//
class CFormReportDlg : public CLocalizablePropertyPage
{

	DECLARE_DYNAMIC(CFormReportDlg)
	friend CFormSheet;
	friend class CTreeFormReportCtrl;

protected:
	CFormSheet*		m_pSheet;
	ReportMngDlg*	m_pRepSheet;
//	BOOL			m_bModified;

	CReportManager&	m_FormReports;
	CString			m_sUserForSave;
	BOOL			m_bIsRadar;
	BOOL            m_bIsBarcode;
public:
	CString			m_sNewReport;

public:
	// Dialog Data
	CTreeFormReportCtrl	m_ctrlTree;
	CImageList			m_ImageList;
	CImageList			m_ImageListState;
	CButton				m_ctrlFormMngReportAdd;

	BOOL				m_bSecondApply;
	CTBNamespace		m_sDefaultReportNamespace;
	CString				m_sDefaultReportTitle;

public:
	CFormReportDlg(CFormSheet*	 pSheet,	CReportManager& aFormReports);
	CFormReportDlg(ReportMngDlg* pRepSheet, CReportManager& aFormReports);
	~ CFormReportDlg();

public:
	void	SetCaption		(const CString& str) {  m_strCaption = str;}

protected:
	void 	LoadImageList	();
	void 	FillTree		();
	void 	AddTreeElement	(CDocumentReportDescription* pReportInfo, BOOL bSelect = FALSE);
	BOOL	ExistReport		(CTBNamespace* Ns);

	void 	SetAsDefault	(BOOL bSetDef = TRUE);
	void 	DeleteReport	();
	void 	OnRemaneLabel	();
	void 	RenameTitle		(const CString& strSelItem);
	void 	AddReport		();
	void	AddReport		(CTBNamespace* Ns, BOOL& bMod);

	BOOL	CanDelete		();

	int	 	GetImage		(CFunctionDescription* pReportInfo);
	void 	SetModified		(BOOL bModified = TRUE);

	void 	Property		(CDocumentReportDescription* pRepsel);
	BOOL	DoSave			(BOOL bFromApply);

// Implementation
protected:
	// Generated message map functions
	//{{AFX_MSG(CFormReportDlg)
	virtual BOOL OnInitDialog		();
	virtual void OnOK				();
	virtual BOOL OnApply			();

	afx_msg void OnDoubleClickTree	(NMHDR *pNMHDR, LRESULT *pResult);
	afx_msg void OnTreeSelchanged	(NMHDR *pNMHDR, LRESULT *pResult);
	afx_msg void OnAddReport		();
	//}}AFX_MSG
	DECLARE_MESSAGE_MAP()
};

/////////////////////////////////////////////////////////////////////////////
//							CPropertyReportDlg
///////////////////////////////////////////////////////////////////////////////
//
class CPropertyReportDlg : public CLocalizableDialog
{
	DECLARE_DYNAMIC(CPropertyReportDlg)

public:	
	CDocumentReportDescription*	 m_pReportDescription;
	int							 m_nImage;

public:
	CPropertyReportDlg(CDocumentReportDescription* pReportDescription, int nImage = 0/*, CWnd* pParent = NULL*/);   
	virtual ~CPropertyReportDlg();

public:
	int GetImageReport	();

protected:
	virtual void DoDataExchange(CDataExchange* pDX);    

protected:
//{{AFX_MSG(CTBFileDlg)
	virtual BOOL OnInitDialog		();	
//}}AFX_MSG
DECLARE_MESSAGE_MAP()
};

///////////////////////////////////////////////////////////////////////////////
//							CFormSheet
///////////////////////////////////////////////////////////////////////////////
//
class CFormBodyDlg;
class CFormPropertiesDlg;

class CFormSheet : public CLocalizablePropertySheet
{
	friend CFormBodyDlg;
	friend CFormReportDlg;
	friend CFormPropertiesDlg;
	
	DECLARE_DYNAMIC(CFormSheet);

public:
	CFormBodyDlg*		m_pFormBodyDlg;
	CFormPropertiesDlg*	m_pFormPropertiesDlg;
	CFormReportDlg*		m_pFormReportDlg;
	CFormReportDlg*		m_pFormRadarDlg;
	CFormReportDlg*		m_pFormBarcodeDlg;
	CPropertyPage*		m_pSecurityAdmin;
	CPropertyPage*		m_pProfilesWizPropPage;

protected:
	CFormManager&	m_FormManagerSource;
	CFormManager	m_FormManager;
	BOOL			m_bExecEnabled;
	CString			m_sCaption;
	CString			m_sCaptionBarcode;
	BOOL			m_bTBFModified;
	BOOL			m_bXMLModified;

public:
	CFormSheet		(CFormManager& aFormManager, const CString& strTitle, BOOL bExecEnabled);
	~CFormSheet	();

	CFormManager* GetFormManager() { return &m_FormManager; }

	BOOL SaveSheet	(const CString& sUserForSave);

protected:
//{{AFX_MSG(CFormSheet)
	virtual BOOL	OnInitDialog			();
//}}AFX_MSG
DECLARE_MESSAGE_MAP()

// Diagnostics
#ifdef _DEBUG
	void Dump(CDumpContext& dc) const
		{ ASSERT_VALID(this); AFX_DUMP0(dc, " CFormSheet\n"); CLocalizablePropertySheet::Dump(dc); }
#endif // _DEBUG
};

//===========================================================================
class CItemNoLocInDocRep : public CObject		// per localizzazione (parte non localizzata)
{
public: 
	CString			m_strName;
	CTBNamespace	m_DocNamespace;
	static CItemNoLocInDocRep* Create(Array& garbageCollector, const CString& strName);
	static CItemNoLocInDocRep* Create(Array& garbageCollector, const CString& strName, const CTBNamespace& Ns);
private: 
	CItemNoLocInDocRep (const CString& strName);
	CItemNoLocInDocRep (const CString& strName, const CTBNamespace& Ns);	
};

/////////////////////////////////////////////////////////////////////////////
//							CTreeFormReportAdminDlg
///////////////////////////////////////////////////////////////////////////////
//
class CTreeFormReportAdminDlg : public CTBTreeCtrl
{	
	DECLARE_DYNAMIC(CTreeFormReportAdminDlg)

public:
	BOOL	m_bRename;

public:
	CTreeFormReportAdminDlg ();

protected:
	//{{AFX_MSG(CTreeFormReportAdminDlg)
	afx_msg void OnSetAsDefault		();
	afx_msg void OnDelete			();
	afx_msg void OnRename			();
	afx_msg void OnProperty			();

	afx_msg void OnItemBeginEdit	(NMHDR*, LRESULT*);
	afx_msg void OnItemEndEdit		(NMHDR*, LRESULT*);

	afx_msg void OnKeyDown			(UINT nChar, UINT nRepCnt, UINT nFlags);
	afx_msg void OnKeyUp			(UINT nChar, UINT nRepCnt, UINT nFlags);	

	afx_msg void OnRButtonDown		(UINT nFlags, CPoint point);
	afx_msg void OnContextMenu		(CWnd*, CPoint);
	//}}AFX_MSG
	DECLARE_MESSAGE_MAP()	
};

/////////////////////////////////////////////////////////////////////////////
//							CAdminToolDocReportDlg
///////////////////////////////////////////////////////////////////////////////
//
class TB_EXPORT CAdminToolDocReportDlg : public CParsedDialog
{
	friend CTreeFormReportAdminDlg;

	DECLARE_DYNAMIC(CAdminToolDocReportDlg)

protected:	
	CWnd*						m_pWndParent;
	CTreeFormReportAdminDlg		m_Tree;
	CImageList					m_ImageList;
	CImageList					m_ImageListState;
/*	CBCGPComboBox				m_ComboApp;
	CBCGPComboBox				m_ComboUsr;
	CBCGPComboBox				m_ModulesCombo;
	CBCGPEdit					m_EditForText;*/

	CTBListBox					m_ListMod;
	CTBListBox					m_ListDoc;
	CBCGPListCtrl				m_ApplicationsList;
	CImageList					m_AppsImageList;
	CButton						m_BtnSave;	
	CButton						m_BtnSetDefAllUsr;
	

    CTBNamespace				m_NameSpace;
	CString						m_strUsr;
	Array						m_arAppItemLoc;

	BOOL						m_bBtnUsrPressed;
	BOOL						m_bBtnAllUsrPressed;
	BOOL						m_bBtnStdPressed;	
	BOOL						m_bModifiedUsr;
	BOOL						m_bModifiedAllUsr;
	BOOL						m_bUsrModConditioned;  // se usr modificato da modifica in AllUsr (modifica indiretta) o aggiunta report esistente in AllUsrs
	int							m_nPrevModSel;
	int							m_nPrevAppSel;
	int							m_nPrevDocSel;	

private:
	CReportManager				m_ReportsMng;
	BOOL						m_bUseAuditing;

public:
	CAdminToolDocReportDlg();   
	virtual ~CAdminToolDocReportDlg();

protected:
	void LoadImageList			();

	void AddTreeElement			(CDocumentReportDescription* pReportInfo, BOOL bSelect = FALSE);
	void FillTree					(BOOL bModified = FALSE);
	void FillTreeStd				();
	void FillTreeAllUsrs			(BOOL bModified = FALSE);
	void FillTreeUsr				(const CString& strUsr, BOOL bModified  = FALSE);
	void FillTreeUserIfNecessary	(BOOL bModified  = FALSE);

	void FillComboApp			();
	void FillAppsList			();
	void FillComboUsr			();
	void FillComboMods			(const CString& strLabel = _T(""), bool bFirst = FALSE );
	void FillListDoc			();
	void ClearStoredInfo		();
			
	void SetAsDefaultUsr		(BOOL bSetDef = TRUE);
	void SetAsDefaultAllUsr		();
	void SetStartNamespace		();
	
	void Property				(CDocumentReportDescription* pRepsel);
	void RemaneLabel			();
	
	BOOL ExistReportInTot		(CTBNamespace* Ns);
	BOOL ExistReportInUsr		(CTBNamespace* ns);

	void FilterUsrClicked		(BOOL bPressed);
	void FilterAllUsrClicked	(BOOL bPressed);
	void FilterStdClicked		(BOOL bPressed);
		
	CString GetSelectedUsr		();
	
	BOOL	Save				(BOOL bAllUsers, const CString& strUsr);

protected:	
	virtual BOOL OnCreateToolbar		();						
	virtual BOOL GetToolTipProperties   (CTooltipProperties& tp);
	
	virtual int	 GetImage			(CFunctionDescription* pReportInfo);
	
	virtual void Fill					();
	virtual void FillForAllUsrs			();
	virtual void FillForStd				();
	virtual void FillForUsr				(const CString& strUsr, BOOL bModified  = FALSE);
	virtual void FillDoc				(const CBaseDescriptionArray* pDocArray);
	virtual void SetUsrModified			(BOOL bMod);
	virtual void SetAllUsrModified		(BOOL bMod);

	virtual void ClearTree				();
	
	virtual BOOL CanDelete				();
	virtual void DeleteElement			();	
	virtual void RenameTitle			(const CString& strSelItem);
	
	virtual void SetAllButtonsState		(BOOL bEnabled = TRUE);
	virtual void SetButtonsState		();
	virtual void SetAsDefault			();

private:
	//@@AUDITING
	CTBNamespace*	CreateAuditReport();
	BOOL			InsertNewReports(CTBNamespaceArray*);

protected:
	virtual BOOL OnInitDialog	();
	virtual void DoDataExchange	(CDataExchange* pDX);    
	virtual void OnClose		();

protected:
//{{AFX_MSG(CAdminToolDocReportDlg)

	afx_msg	void	OnComboUsrChanged		();
	afx_msg	void	OnComboModsChanged		();

	afx_msg	void	OnListDocChanged		();
	afx_msg	void	OnListAppSelchanged		(NMHDR *pNMHDR, LRESULT *pResult);

	afx_msg void	OnSave					();

	afx_msg void	OnBtnFilterStd			();
	afx_msg void	OnBtnFilterAllUsrs		();
	afx_msg void	OnBtnFilterUsr			();
	
	afx_msg void	OnBtnDelete				();
	afx_msg void	OnBtnRename				();
	afx_msg void	OnBtnAdd				();
	afx_msg void	OnBtnAddAuditReport		();

	afx_msg	void	OnNMDblclkTree			(NMHDR *pNMHDR, LRESULT *pResult);
	afx_msg void	OnTreeSelchanged		(NMHDR *pNMHDR, LRESULT *pResult);
//}}AFX_MSG
DECLARE_MESSAGE_MAP()
};

/////////////////////////////////////////////////////////////////////////////
//							CAdminProfileItem
///////////////////////////////////////////////////////////////////////////////
//
#define NAME_MAIN_TOOLBAR		_T("Main")

class CAdminProfileItem : public CObject		
{
private: 
	CPathFinder::PosType	m_PosType;
	CString					m_strProfileName;
	CString					m_strPath;
	CString					m_strUsrName;
	BOOL					m_bIsPreferential;

public: 
	CAdminProfileItem (CString strPath, BOOL bIsPreferential, CPathFinder::PosType aPosType = CPathFinder::STANDARD, const CString& strUsrName = _T(""));
	CAdminProfileItem (CString strPath, CPathFinder::PosType aPosType = CPathFinder::STANDARD, const CString& strUsrName = _T(""));

public:
	const CString&			GetProfilePath	()				{return m_strPath;}
	const CString&			GetProfileName	()				{return m_strProfileName;}
	const CString&			GetUserName		()				{return m_strUsrName;}
	CPathFinder::PosType	GetPosType		()				{return m_PosType;}
	BOOL					IsPreferential	()				{return m_bIsPreferential;}
	
	void					SetPreferential (BOOL bPref)	{m_bIsPreferential = bPref;}
};

/////////////////////////////////////////////////////////////////////////////
//							CAdminToolDocProfileDlg
///////////////////////////////////////////////////////////////////////////////
//
class TB_EXPORT CAdminToolDocProfileDlg : public CAdminToolDocReportDlg
{
	friend CTreeFormReportAdminDlg;

	DECLARE_DYNAMIC(CAdminToolDocProfileDlg)

protected:
	CStringArray	m_arStdProfile;
	CStringArray	m_arAllUsersProfile;
	CStringArray	m_arUserProfile;

public:
	CAdminToolDocProfileDlg();   
	virtual ~CAdminToolDocProfileDlg();

protected:
	int	 GetImage				(CPathFinder::PosType aType = CPathFinder::USERS);

	void FindAllProfiles		();

	void FindStdProfiles		();
	void FindAllUsersProfiles	();
	void FindUserProfiles		();

	void AddTreeStdEl			(const CString& strProfilePath, BOOL bSelect = FALSE);
	void AddTreeAllUsrsEl		(const CString& strProfilePath, BOOL bSelect = FALSE);
	void AddTreeUsrEl			(const CString& strProfilePath, BOOL bSelect = FALSE);
	void AddTreeEl				(CAdminProfileItem* pAdminProfileItem, BOOL bSelect = FALSE);
	void SetUsrModified			(BOOL bMod);
	void SetAllUsrModified		(BOOL bMod);

	void Fill					();
	void FillForStd				();
	void FillForAllUsrs			();
	void FillForUsr				(const CString& strUsr, BOOL bModified  = FALSE);

	void ClearTree				();
	BOOL CanDelete				();
	void DeleteElement			();

	void RenameTitle			(const CString& strSelItem);
	void SetAsDefault			();
	void BtnModify				(CAdminProfileItem* pAdminProfileItem);
	
	CAdminProfileItem*		GetTreeHItemValue		();
	void					RefreshAfterModifies	();


protected:
	virtual BOOL OnInitDialog	();
	virtual void OnCancel		();
	virtual void OnOK			();

	virtual void SetAllButtonsState	(BOOL bEnabled = TRUE);
	virtual void SetButtonsState	();	
	virtual void FillDoc			(const CBaseDescriptionArray* pDocArray);


protected:
//{{AFX_MSG(CAdminToolDocProfileDlg)
	
	afx_msg void	OnBtnNew		();
	afx_msg void	OnBtnModify		();
	afx_msg void	OnBtnDelete		();
	afx_msg void	OnBtnRename		();
	afx_msg void	OnBtnClone		();
	//afx_msg void	OnBtnInsert		();
	afx_msg void	OnBtnCopyIn		();
	afx_msg void	OnBtnMoveIn		();

	afx_msg	void	OnNMDblclkTree	(NMHDR *pNMHDR, LRESULT *pResult);
	afx_msg	void	OnTreeSelchanged(NMHDR *pNMHDR, LRESULT *pResult);
//}}AFX_MSG
DECLARE_MESSAGE_MAP()
};

//--------------------------------------------------------------------------
class CCopyMoveProfileDialog : public CParsedDialog
{
	DECLARE_DYNAMIC(CCopyMoveProfileDialog)

public:
	CStringArray			m_arUsrsSel;	
	CPathFinder::PosType	m_PosTypeSelected;

protected:
	CBCGPListCtrl			m_ListUsers;
	CTBListBox				m_ListProfile;

	BOOL					m_bCopy;
	CTBNamespace			m_Namespace;
	CPathFinder::PosType	m_PosTypeExcluded;
	CStringArray			m_arStdProfile;
	CStringArray			m_arAllUsersProfile;
	CStringArray			m_arUserProfile;
	CString					m_strUsr;	

public:	
	CCopyMoveProfileDialog(BOOL bCopy, CPathFinder::PosType aPosTypeExcluded, CTBNamespace Ns, const CStringArray& arStd, const CStringArray& arAllUsrs, const CStringArray& arUsr);   
	virtual ~CCopyMoveProfileDialog();

protected:
	void			PrepareControls			();
	void			FillUsr					();
	void			FillProfileList			();

protected:
	virtual BOOL	OnCreateToolbar			() {return TRUE;}						
	virtual BOOL	GetToolTipProperties    (CTooltipProperties& tp) {return TRUE;}

protected:
//{{AFX_MSG(CCopyMoveProfileDialog)
virtual BOOL	OnInitDialog			();
afx_msg void	OnOK					();
afx_msg void	OnCheckRadioButton		();
afx_msg void	OnUsrChanged			(NMHDR *pNMHDR, LRESULT *pResult);
//}}AFX_MSG
DECLARE_MESSAGE_MAP()
};

//---------------------------------------------------------------------------------
//class DocumentExplorerListCtrl 
//---------------------------------------------------------------------------------
class TB_EXPORT DocumentExplorerListCtrl : public CBCGPListCtrl
{
	DECLARE_DYNCREATE(DocumentExplorerListCtrl)

	public:
		DocumentExplorerListCtrl();
		virtual ~DocumentExplorerListCtrl();

		virtual BOOL IsInternalScrollBarThemed() const;
	protected:
		//{{AFX_MSG(DocumentExplorerListCtrl)
		//}}AFX_MSG
	DECLARE_MESSAGE_MAP()
};

/////////////////////////////////////////////////////////////////////////////
//							CDocumentExplorerDlg
///////////////////////////////////////////////////////////////////////////////
//
class TB_EXPORT CDocumentExplorerDlg : public CBaseDocumentExplorerDlg
{
	DECLARE_DYNCREATE(CDocumentExplorerDlg)
public:	
	CDocumentExplorerDlg();   
	virtual ~CDocumentExplorerDlg();

protected:	
	CWnd*						m_pWndParent;
	DocumentExplorerListCtrl	m_ListCtrlApps;
	CBCGPComboBox				m_ComboModule;
	CTBListBox					m_ListDocument;
	CButton						m_ButtonSelect;
	CButton						m_ButtonClose;

	CImageList					m_AppsImageList;
	CImageList					m_ImageList;
	Array						m_arAppItemLoc;

	CTBNamespace				m_NameSpace;
	CBCGPEdit					m_ShowFullNameSpace;

protected:
	void			FillModuleComboBox			(const CString& strLabel = _T(""), bool bFirst = FALSE );
	void			FillApplicationsListControl	();
	void			FillDocumentListBox			();
	void			SetStartNamespace			();
	void			SelectNameSpaceAndClose		();
	void			FillDoc						(const CBaseDescriptionArray* pDocArray);
protected:
//{{AFX_MSG(CDocumentExplorerDlg)
	virtual BOOL OnInitDialog					();
	afx_msg void OnClose						();
	afx_msg void OnModuleComboSelectItem		();
	afx_msg void OnDocumentListSelectItem		();
	afx_msg void OnAppsItemChanged				(NMHDR *pNMHDR, LRESULT *pResult);
	afx_msg void OnDocumentDoubleClick			();
	afx_msg void OnSelectNamespace				();
//}}AFX_MSG
DECLARE_MESSAGE_MAP()
};

//=============================================================================
#include "endh.dex"
