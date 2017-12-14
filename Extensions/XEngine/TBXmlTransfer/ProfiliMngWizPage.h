
#pragma once

#include <afxdtctl.h>
#include <afxtempl.h>

#include <TBGES\BarQyDlg.h>
#include <TBGES\XMLControls.h>
#include <TbGeneric\dibitmap.h>

#include <TBGENLIB\tbWizardMaster.h>


//includere alla fine degli include del .H
#include "beginh.dex"

class CXMLProfileInfo;
class CXMLDocInfo;
class CProfileWizMasterDlg;
class CXMLDefaultInfo;
class CXMLExportCriteria;
class CUserExportCriteria;

#define UM_LIST_CHECK_CHANGED				(WM_USER + 135)
#define PREFERRED_PROFILE_TYPE_CAPTION	" (Preferenziale)"

#define PROF_LIST_STANDARD	0
#define PROF_LIST_CUSTOM	1
#define PROF_LIST_ALL		2

//------------------------------------------------------------------
class CCheckItemData
{
public:
	int m_nItem;
	int m_nNextCheckVal;

public:
	CCheckItemData(int nItem, int nNextCheckVal)
	{
		m_nItem = nItem;
		m_nNextCheckVal = nNextCheckVal;
	}
};



//---------------------------------------------------------------------------------
//class CDBTPropertiesDlg
//---------------------------------------------------------------------------------
class TB_EXPORT CProfileDBTPropertiesDlg : public CLocalizableDialog
{
private:
	CXMLDBTInfo* m_pDBTInfo;
	BOOL		 m_bReadOnly;

public:
	CProfileDBTPropertiesDlg(CXMLDBTInfo*, BOOL = FALSE);
		
protected:
	// Generated message map functions
	//{{AFX_MSG(CProfileDBTPropertiesDlg)
	virtual BOOL OnInitDialog	();
	virtual void OnOK			();
	//}}AFX_MSG
	DECLARE_MESSAGE_MAP()
	DECLARE_DYNAMIC(CProfileDBTPropertiesDlg)
};

//---------------------------------------------------------------------------------
//class CSelectionQueryDlg
//---------------------------------------------------------------------------------
class CSelectionQueryDlg : public CParsedDialog
{
	DECLARE_DYNAMIC(CSelectionQueryDlg)
protected:
	CXMLExportCriteria*		m_pXMLExpCriteria;
	CUserExportCriteria*	m_pUserCriteria;	
	CExpEdit*				m_pEdtExpWClause;
	CEqnEdit*				m_pEdtOrderBy;		

private:	
	const SqlTableInfo*		m_pTableInfo;
	QueryInfo*			m_pQueryInfo;
	
	ProgramData*		m_pPrgData;	
	BOOL				m_bPrgDataOwns;
	BOOL				m_bNewQuery;
	CBrush				m_brSolidWhite; // brush for white background

	
public:
	CSelectionQueryDlg		(CXMLExportCriteria*);
	~CSelectionQueryDlg	();

	void BtnTestEnableWindow	(BOOL bEnable = TRUE);
	void CovertStrOrderByInLF	(CString&);
	void InitFields				();

	CButton* GetChkBoxNativeExpr();

	BOOL IsNativeExpr	()	{ return GetChkBoxNativeExpr()->GetCheck(); }
	BOOL TestQuery		(CString* = NULL);

protected:
	virtual void OnOK			();
	virtual BOOL OnInitDialog	();

	//{{AFX_MSG(CSelectionQueryDlg)
		afx_msg void OnTest			() { TestQuery (); }
		afx_msg void ShowAskRules	();
		afx_msg void OnEnableBtnTest() { BtnTestEnableWindow(); }
		afx_msg HBRUSH	OnCtlColor(CDC*, CWnd*, UINT);
	//}}AFX_MSG
	DECLARE_MESSAGE_MAP()

// Diagnostics
#ifdef _DEBUG
public:
	void Dump		(CDumpContext&)	const;
	void AssertValid()				const { CParsedDialog::AssertValid(); }
#endif // _DEBUG

};

//----------------------------------------------------------------------------------
class CXRefTreeCtrl : public CDocDescrTreeCtrl
{
	DECLARE_DYNCREATE(CXRefTreeCtrl)

private:
	CXMLProfileInfo* m_pXMLProfileInfo;

public:
	CXRefTreeCtrl();

public:
	void			FillTree			(CXMLProfileInfo*);
	virtual	void	SetBackColor		();

public:
	//{{AFX_MSG(CXRefTreeCtrl)
	virtual afx_msg void OnDBTProperties	();
			afx_msg void OnContextMenu		(CWnd*, CPoint);
			afx_msg void OnDoubleClick		(NMHDR*, LRESULT*);
			afx_msg void OnExportXRef		();
			afx_msg void OnUseNoXRef		();
			afx_msg void OnUseAllXRef		();
			afx_msg void OnXRefHotKeyLink	();
			afx_msg void OnKeydown			(NMHDR* pNMHDR, LRESULT* pResult);
	//}}AFX_MSG

	DECLARE_MESSAGE_MAP()
};

//----------------------------------------------------------------------------------
//CWhiteCheckListBox
//----------------------------------------------------------------------------------
class CWhiteCheckListBox : public CCheckListBox
{
	DECLARE_DYNCREATE(CWhiteCheckListBox)

private:
	CBrush	m_brSolidWhite;

public:
	CWhiteCheckListBox();
	~CWhiteCheckListBox();

public:
	//{{AFX_MSG(CWhiteCheckListBox)
	afx_msg void	DrawItem		(LPDRAWITEMSTRUCT lpDrawItemStruct);
	afx_msg HBRUSH	CtlColor		(CDC* pDC, UINT nCtlColor);
	afx_msg void	OnLButtonDown	(UINT nFlags, CPoint point);
	//}}AFX_MSG

	DECLARE_MESSAGE_MAP()
};

//----------------------------------------------------------------------------------
// CSimpleProfilesListCtrl dialog
//----------------------------------------------------------------------------------
class CSimpleProfilesListCtrl : public CTBTreeCtrl
{
	DECLARE_DYNCREATE(CSimpleProfilesListCtrl)

	friend class CProfiliMngWizPage;

private:
	CImageList*			m_pImageList;
	CXMLProfileInfo*	m_pXMLEmptyProfile;		//per creare i nuovi profili
	CXMLDefaultInfo*	m_pXMLDefaultInfo;		//per settare il preferred profile
	CXMLDocInfo*		m_pXMLDocInfo;
	CTBNamespace		m_nsDoc;
	BOOL				m_bSaveDefault;			//salva il file default.xml

	HTREEITEM			m_hPrefered;
	CString				m_strOldProfName;
	CString				m_strNewProfName;

	CObArray			m_arProfileNameList;

public:
	CSimpleProfilesListCtrl	(const CTBNamespace& = CTBNamespace());
	~CSimpleProfilesListCtrl();

public:
	void	SetDocumentNameSpace(const CTBNamespace&);
	void	InitializeImageList	();
	void	Fill				();

	HTREEITEM	GetPrefered	(){return m_hPrefered;}

private:
	void		AddProfileItems		(const CStringArray&);
	void		ModifyProfileItem	(HTREEITEM hit, const CString& strProfilePath, BOOL bSelect = FALSE);

	HTREEITEM	InsertProfileItem	(const CString& strProfilePath, BOOL bSelect = FALSE); 
	void		SetItemIcon			(HTREEITEM hit); 
	
	BOOL		IsPresent			(const CString&) const;
	void		RemoveStringFromArray(CString* pItemStr);


// Generated message map functions
protected:
	//{{AFX_MSG(CSimpleProfilesListCtrl)
	afx_msg void	OnContextMenu	(CWnd*, CPoint);
public:
	afx_msg void	OnNewProfile			();
	afx_msg void	OnCloneCurrentProfile	();
	afx_msg void	OnDeleteCurrentProfile	();
	afx_msg void	OnModifyCurrentProfile	();
	afx_msg void	OnProfileRename			();
	afx_msg void	OnProfilePrefered		();
	afx_msg void	OnLButtonDblClk			(UINT nFlags, CPoint point);
	afx_msg void	OnRButtonDown			(UINT nFlags, CPoint point);
	afx_msg void	OnItemBeginEdit			(NMHDR* pNMHDR, LRESULT* pResult);
	afx_msg void	OnItemEndEdit			(NMHDR* pNMHDR, LRESULT* pResult);
	//}}AFX_MSG
	DECLARE_MESSAGE_MAP()
};

//----------------------------------------------------------------------------------
// CProfiliMngDlg dialog
//----------------------------------------------------------------------------------
class CProfiliMngWizPage : public CLocalizablePropertyPage
{
	DECLARE_DYNAMIC(CProfiliMngWizPage)
private:
	CSimpleProfilesListCtrl	m_ProfilesList;
	CString					m_strDocPreferredProfile;
	CTBNamespace			m_DocumentNameSpace;
	CXMLDocInfo*			m_pXMLDocInfo;
	CString					m_strOldProfName;
	CString					m_strNewProfName;

public:
	CProfiliMngWizPage	(CWnd* pParent = NULL);   // standard constructor
	CProfiliMngWizPage	(const CTBNamespace&, CXMLDocInfo* = NULL);

public:
	void EnableButtons(HTREEITEM hit);

public:
	virtual void OnOK		();

private:
	void EnableButtons	(BOOL bEnable);

protected:
	// Generated message map functions
	//{{AFX_MSG(CProfiliMngWizPage)
	virtual BOOL OnInitDialog				();
	afx_msg void OnNewProfile				();
	afx_msg void OnCloneCurrentProfile		();
	afx_msg void OnDeleteCurrentProfile		();
	afx_msg void OnModifyCurrentProfile		();
	afx_msg void OnRenameCurrentProfile		();
	afx_msg void OnProfListItemStateChanged	(NMHDR*, LRESULT*);
	//}}AFX_MSG
	DECLARE_MESSAGE_MAP()
};

/////////////////////////////////////////////////////////////////////////////
// CProfileWizardPage
/////////////////////////////////////////////////////////////////////////////
//
class CProfileWizardPage : public CWizardPage
{
	DECLARE_DYNAMIC(CProfileWizardPage)

public:
	CProfileWizardPage(UINT, CWnd* =NULL);
	~CProfileWizardPage();

public:
	CProfileWizMasterDlg*	m_pProfileSheet;
	CToolTipCtrl*			m_pToolTip;

// Implementation
protected:
	virtual BOOL OnInitDialog();
	virtual BOOL OnCreatePage();
	
	// Generated message map functions
	//{{AFX_MSG(CProfileWizardPage)
	virtual BOOL PreTranslateMessage	(MSG* pMsg);
	//}}AFX_MSG
	DECLARE_MESSAGE_MAP()

};

//---------------------------------------------------------------------------
// CProfileWizPresentationPage dialog
//---------------------------------------------------------------------------
class CProfileWizPresentationPage : public CProfileWizardPage
{
	DECLARE_DYNCREATE(CProfileWizPresentationPage)

private:
	CBCGPComboBox			m_TransDocComboBox;
	int						m_nProfileVersion;
	CTranspBmpCtrl			m_Image;
	CXMLProfileInfo*		m_pProfileInfo;
	Array					m_arTransDocs;

public:
	CProfileWizPresentationPage();

public:
	virtual void	OnActivate			();
	virtual	LRESULT OnWizardNext		();
	virtual void DoDataExchange			(CDataExchange* pDX);    // DDX/DDV support

protected:
	virtual BOOL	OnInitDialog		();

private:
	void LoadTransformInfo();

protected:
	// Generated message map functions
	//{{AFX_MSG(CProfileWizPresentationPage)
	afx_msg void OnDeltaPosSpinVersion		(NMHDR*, LRESULT*);
	afx_msg void OnTransformCheck			();		
	//}}AFX_MSG
	DECLARE_MESSAGE_MAP()

};

//---------------------------------------------------------------------------
// CProfileExpPropPage dialog
//---------------------------------------------------------------------------
class CProfileExpPropPage : public CProfileWizardPage
{
	DECLARE_DYNCREATE(CProfileExpPropPage)

private:
	int				m_nMaxDim;
	int				m_nMaxDocument;
	CTranspBmpCtrl	m_Image;

public:
	CProfileExpPropPage();

public:
	virtual void	OnActivate	();
	virtual	LRESULT OnWizardNext();
	virtual void DoDataExchange			(CDataExchange* pDX);    // DDX/DDV support

protected:
	virtual BOOL OnInitDialog();

	// Generated message map functions
	//{{AFX_MSG(CProfileExpPropPage)
	afx_msg void OnDeltaPosMaxDim			(NMHDR*, LRESULT*);
	afx_msg void OnDeltaPosSpinMaxDoc		(NMHDR*, LRESULT*);
	//}}AFX_MSG
	DECLARE_MESSAGE_MAP()
};

//---------------------------------------------------------------------------
// CProfileMagicDocsPage dialog
//---------------------------------------------------------------------------
class CProfileMagicDocsPage : public CProfileWizardPage
{
	DECLARE_DYNCREATE(CProfileMagicDocsPage)

private:
	CTranspBmpCtrl	m_Image;

public:
	CProfileMagicDocsPage();

public:
	virtual void	OnActivate	();
	virtual	LRESULT OnWizardNext();

protected:
	virtual BOOL OnInitDialog();

	// Generated message map functions
	//{{AFX_MSG(CProfileExpPropPage)
	afx_msg void OnPostableChanged();
	afx_msg void OnPostBackChanged();
	//}}AFX_MSG
	DECLARE_MESSAGE_MAP()

};

//---------------------------------------------------------------------------
// CProfileDBTPage dialog
//---------------------------------------------------------------------------
class CProfileDBTPage : public CProfileWizardPage
{
	DECLARE_DYNCREATE(CProfileDBTPage)

private:
	CWhiteCheckListBox	m_DBTList;
	CTranspBmpCtrl		m_Image;

public:
	CProfileDBTPage();

public:
	virtual void	OnActivate	();
	virtual	LRESULT OnWizardNext();
	virtual LRESULT OnWizardBack();

protected:
	virtual BOOL OnInitDialog();

// Generated message map functions
	//{{AFX_MSG(CProfileDBTPage)
	//}}AFX_MSG
	DECLARE_MESSAGE_MAP()
};

//---------------------------------------------------------------------------
// CProfileChooseParamPage dialog
//---------------------------------------------------------------------------
class CProfileChooseParamPage : public CProfileWizardPage
{
	DECLARE_DYNCREATE(CProfileChooseParamPage)

public:
	CProfileChooseParamPage();

private:
	CTranspBmpCtrl	m_Image;

public:
	virtual void	OnActivate	();
	virtual	LRESULT OnWizardNext();
	virtual	LRESULT OnWizardBack();

protected:
	virtual BOOL OnInitDialog();

// Generated message map functions
	//{{AFX_MSG(CProfileChooseParamPage)
	//}}AFX_MSG
	DECLARE_MESSAGE_MAP()
};

//---------------------------------------------------------------------------
// CProfileFieldPage dialog
//---------------------------------------------------------------------------
class CProfileFieldPage : public CProfileWizardPage
{
	DECLARE_DYNCREATE(CProfileFieldPage)

private:
	CTBListBox			m_DBTList;
	CWhiteCheckListBox	m_FieldList;
	CXMLDBTInfo*		m_pCurrentDbtInfo;
	CTranspBmpCtrl		m_Image;
	BOOL				m_bSelectAll;

public:
	CProfileFieldPage();

private:
	void FillFieldListBox(CXMLDBTInfo*);
	void SetButtonTitle();
	void SetSelectAllButton();


public:
	virtual void	OnActivate			();
	virtual	LRESULT OnWizardNext		();
	virtual	LRESULT OnFieldCheckChanged	(WPARAM, LPARAM);
	virtual void	OnSelectAllChanged	();

protected:
	virtual BOOL OnInitDialog();

// Generated message map functions
	//{{AFX_MSG(CProfileFieldPage)
	afx_msg void OnSelchangeListDBT();
	//}}AFX_MSG
	DECLARE_MESSAGE_MAP()
};

//---------------------------------------------------------------------------
// CProfileXRefPage dialog
//---------------------------------------------------------------------------
class CProfileXRefPage : public CProfileWizardPage
{
	DECLARE_DYNCREATE(CProfileXRefPage)

private:
	CXRefTreeCtrl		m_TreeCtrl;
	CImageList			m_ProfileTreeImageList;
	CTranspBmpCtrl		m_Image;

public:
	CProfileXRefPage();

public:
	virtual void	OnActivate	();
	virtual	LRESULT OnWizardNext();
	virtual	LRESULT OnWizardBack();

protected:
	virtual BOOL OnInitDialog();

// Generated message map functions
	//{{AFX_MSG(CProfileXRefPage)
	afx_msg void OnContextMenu				(CWnd*, CPoint);
	//}}AFX_MSG
	DECLARE_MESSAGE_MAP()
};

//---------------------------------------------------------------------------
// CProfileSelectionPage dialog
//---------------------------------------------------------------------------
class CProfileSelectionPage : public CProfileWizardPage
{
	DECLARE_DYNCREATE(CProfileSelectionPage)

public:
	CXMLExportCriteria*		m_pXMLExpCriteria;

private:
	CTranspBmpCtrl	m_Image;

protected:
	CUserExportCriteria*	m_pUserCriteria;	
	CExpEdit*				m_pEdtExpWClause;
	CEqnEdit*				m_pEdtOrderBy;		

private:	
	const SqlTableInfo*		m_pTableInfo;
	QueryInfo*				m_pQueryInfo;
	
	ProgramData*			m_pPrgData;	
	BOOL					m_bPrgDataOwns;
	BOOL					m_bNewQuery;
	CBrush					m_brSolidWhite; // brush for white background

public:
	CProfileSelectionPage		();
	~CProfileSelectionPage		();

public:
	virtual void	OnActivate	();
	virtual	LRESULT OnWizardNext();
	virtual	LRESULT OnWizardBack();

protected:
	virtual BOOL OnInitDialog();

public:
	void CovertStrOrderByInLF	(CString&);
	void InitFields				();

	CButton* GetChkBoxNativeExpr();

	BOOL IsNativeExpr	()	{ return GetChkBoxNativeExpr()->GetCheck(); }
	BOOL TestQuery		(CString* = NULL);

// Generated message map functions
	//{{AFX_MSG(CProfileSelectionPage)
		afx_msg void	OnTest			() { TestQuery (); }
		afx_msg void	ShowAskRules	();
		afx_msg HBRUSH	OnCtlColor		(CDC*, CWnd*, UINT);
	//}}AFX_MSG
	DECLARE_MESSAGE_MAP()
};

//---------------------------------------------------------------------------
// CProfileFinishPage dialog
//---------------------------------------------------------------------------
class CProfileFinishPage : public CProfileWizardPage
{
	DECLARE_DYNCREATE(CProfileFinishPage)

public:
	CProfileFinishPage();

private:
	CTranspBmpCtrl	 m_Image;
	CXMLProfileInfo* m_pXMLProfileInfo;
	CStringArray	m_arUsers; //mi serve per fare i controlli di esistenza in caso di inserimento di un nuovo profilo

private:
	BOOL ProfileAlreadyExists(const CString& strProfileName, CPathFinder::PosType ePosType, const CString& strUserName);

public:
	virtual void	OnActivate		();
	virtual	LRESULT OnWizardBack	();
	virtual BOOL	OnWizardFinish	();

protected:
	virtual BOOL OnInitDialog();


// Generated message map functions
	//{{AFX_MSG(CProfileFinishPage)
	//}}AFX_MSG
	DECLARE_MESSAGE_MAP()
};



//---------------------------------------------------------------------------
// CProfileWizMasterDlg dialog
//---------------------------------------------------------------------------
class CProfileWizMasterDlg : public CWizardMasterDialog
{
	DECLARE_DYNAMIC(CProfileWizMasterDlg)
	
public:
	CXMLProfileInfo*			m_pXMLProfileInfo;

	CProfileWizPresentationPage	m_ProfileWizPresentationPage;
	CProfileExpPropPage			m_ProfileExpPropPage;
	CProfileMagicDocsPage		m_ProfileMagicDocsPage;
	CProfileDBTPage				m_ProfileDBTPage;
	CProfileChooseParamPage		m_ProfileChooseParamPage;
	CProfileFieldPage			m_ProfileFieldPage;
	CProfileXRefPage			m_ProfileXRefPage;
	CProfileFinishPage			m_ProfileFinishPage;
	CProfileSelectionPage		m_ProfileSelectionQueryPage;

	BOOL						m_bConfigField;
	BOOL						m_bConfigXRef;
	BOOL						m_bConfigUsrCriteria;

// Construction
public:
	CProfileWizMasterDlg(CXMLProfileInfo*,  CWnd* pParent = NULL);

public:
			void OnSelections	(CXMLProfileInfo*);
	virtual BOOL OnWizardFinish	();

// Implementation
protected:

	// Generated message map functions
	//{{AFX_MSG(CProfileWizMasterDlg)
	virtual BOOL OnInitDialog();
	//}}AFX_MSG
	DECLARE_MESSAGE_MAP()
};


#include "endh.dex"
