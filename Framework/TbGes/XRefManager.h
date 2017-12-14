
#pragma once

#include <afxdtctl.h>
#include <afxtempl.h>

#include <TbGeneric\dibitmap.h>

#include <TbGenlib\tbWizardMaster.h>
#include <TbGenlib\AddOnMng.h>

#include <TbWoormEngine\edtcmm.h>

#include "BarQyDlg.h"
#include "xmlgesinfo.h"
#include "xmlcontrols.h"

//includere alla fine degli include del .H//includere alla fine degli include del .H
#include "beginh.dex"

class CXMLXRefInfo;
class CXRefWizMasterDlg;
class SqlColumnInfo;
class SqlTableInfo;

/////////////////////////////////////////////////////////////////////////////
// CWizardInformation
/////////////////////////////////////////////////////////////////////////////
//
class TB_EXPORT CWizardInformation
{
public:
	CStringArray		m_FieldXRefAr;
	CString				m_strDocument;
	CXMLXRefInfo*		m_pXMLXRefInfo;
	BOOL				m_bDeleteFkPk;

public:
	CWizardInformation();
	~CWizardInformation();
};

/////////////////////////////////////////////////////////////////////////////
// CXRefWizardPage
/////////////////////////////////////////////////////////////////////////////
//
class TB_EXPORT CXRefWizardPage : public CWizardPage
{
	DECLARE_DYNAMIC(CXRefWizardPage)

public:
	CXRefWizardPage(UINT, CWnd* =NULL);
	~CXRefWizardPage();

public:
	CXRefWizMasterDlg*	m_pXRefSheet;
	CToolTipCtrl*		m_pToolTip;
	
// Implementation
protected:
	virtual BOOL OnInitDialog();
	virtual BOOL OnCreatePage();
	
	// Generated message map functions
	//{{AFX_MSG(CXRefWizardPage)
	virtual BOOL PreTranslateMessage	(MSG* pMsg);
	//}}AFX_MSG
	DECLARE_MESSAGE_MAP()

};

/////////////////////////////////////////////////////////////////////////////
// CXRefWizPresentationPage dialog
/////////////////////////////////////////////////////////////////////////////
//
class TB_EXPORT CXRefWizPresentationPage : public CXRefWizardPage
{
	DECLARE_DYNCREATE(CXRefWizPresentationPage)

private:
	CTranspBmpCtrl		m_Image;

public:
	CBCGPEdit				m_edName;
	CBCGPEdit				m_edUrl;

public:
	CXRefWizPresentationPage();

public:
	virtual void	OnActivate			();
	virtual	LRESULT OnWizardNext		();

protected:
	virtual BOOL OnInitDialog();

// Generated message map functions
	//{{AFX_MSG(CXRefWizPresentationPage)
	afx_msg void OnExportCheck();
	//}}AFX_MSG
	DECLARE_MESSAGE_MAP()
};

/////////////////////////////////////////////////////////////////////////////
// CXRefFieldsSelectPage dialog
/////////////////////////////////////////////////////////////////////////////
//
class TB_EXPORT CXRefFieldsSelectPage : public CXRefWizardPage
{
	DECLARE_DYNCREATE(CXRefFieldsSelectPage)

private:
	CTranspBmpCtrl		m_Image;

private:
	CTBListBox	m_FieldListBox;
	CTBListBox	m_FieldListBoxSelected;

// Construction
public:
	CXRefFieldsSelectPage();

private:
	void FillFieldListBox();

// Overrides
public:
	virtual void	OnActivate	();
	virtual	LRESULT OnWizardNext();


// Implementation
protected:
	virtual BOOL OnInitDialog();

// Generated message map functions
	//{{AFX_MSG(CXRefFieldsSelectPage)
	afx_msg void OnAddSegment			();
	afx_msg void OnRemoveSegment		();
	afx_msg void OnDblClickListAdd		();
	afx_msg void OnDblClickListRemove	();
	//}}AFX_MSG
	DECLARE_MESSAGE_MAP()
};

/////////////////////////////////////////////////////////////////////////////
// CXRefFieldsPropPage dialog
/////////////////////////////////////////////////////////////////////////////
//
class TB_EXPORT CXRefFieldsPropPage : public CXRefWizardPage
{
	DECLARE_DYNCREATE(CXRefFieldsPropPage)

private:
	CTranspBmpCtrl		m_Image;

// Construction
public:
	CXRefFieldsPropPage();

public:
	CButton		m_bnExist;
	CButton		m_bnNull;
	CButton		m_bnIndip;
	CButton		m_bnSubjectTo;
	DataStr		m_strExpression;
	CEqnEdit	m_EdtExpression; //editor della condizione di validità gestito solo se m_bnSubjectTo = TRUE
	BOOL		m_bCreateSymTable;

// Overrides
public:
	virtual	LRESULT OnWizardNext();
	virtual void	OnActivate	();
	virtual	LRESULT OnWizardBack();

	// Implementation
protected:
	virtual BOOL OnInitDialog();

protected:
	// Generated message map functions
	//{{AFX_MSG(CXRefFieldsPropPage)
	afx_msg void OnChangeSubjectTo();
	//}}AFX_MSG
	DECLARE_MESSAGE_MAP()

};


//----------------------------------------------------------------------------------
// CXRefDocSelPropPage dialog
//----------------------------------------------------------------------------------
class TB_EXPORT CXRefDocSelPropPage : public CXRefWizardPage
{
private:
	CAppDocumentsTreeCtrl	m_AppDocTreeCtrl;
	CProfileCombo			m_ProfileCombo;
	int						m_nProfCheck;
	CTranspBmpCtrl		m_Image;

public:
	CXRefDocSelPropPage();
	~CXRefDocSelPropPage();

protected:
	void SelectCurrentAppDoc		();
	void GetProfileList				(const CTBNamespace& aDocNamespace, CStringArray& aProfileList);


protected:
	virtual	LRESULT OnWizardNext	();
	virtual BOOL	OnInitDialog	();
	virtual void	OnActivate		();
	virtual LRESULT OnWizardBack	();

protected:
	// Generated message map functions
	//{{AFX_MSG(CXRefDocSelPropPage)
	afx_msg void OnAppDocSelChanged	(NMHDR*, LRESULT*);
	afx_msg void OnPrefProfChek();
	afx_msg void OnNoPrefProfChek();
	//}}AFX_MSG
	DECLARE_MESSAGE_MAP()
};

//---------------------------------------------------------------------------------
//class CXRefSegmentsPage 
//---------------------------------------------------------------------------------
class TB_EXPORT CXRefSegmentsPage : public CXRefWizardPage
{
public:
	CXMLXRefInfo*	m_pXRefInfo;

private:
	CSegmentsGrid	m_SegmentsListCtrl;
	CBCGPComboBox	m_FKCombo;
	CBCGPComboBox	m_ReferencedCombo;
	CTranspBmpCtrl	m_Image;
	CBCGPComboBox	m_cbxDBTs;	
	CObArray		m_RefDBTs;
	CBCGPEdit		m_edFKFixedValue;
	
public:
	CXRefSegmentsPage();
	~CXRefSegmentsPage();

public:
	virtual void	OnActivate		();
	BOOL			IsFkSelected	(const CString&); 
	
private:
	BOOL FkAsCompatiblePk		(SqlColumnInfo*, SqlTableInfo*);
	void FillExtRefDBTs			();
	BOOL CanAddSegment			();
	void DoFKSegmentSelChange	();

protected:
	// Generated message map functions
	//{{AFX_MSG(CXRefSegmentsPage)
	virtual BOOL	OnInitDialog			();
	virtual	LRESULT OnWizardNext			();
	afx_msg void	OnSegmentStateChanged	(NMHDR*, LRESULT*); 
	afx_msg void	OnFKSegmentSelChange	();
	afx_msg void	OnPKSegmentSelChange	();
	afx_msg void	OnDBTSegmentSelChange	();
	afx_msg void	OnModifySegment			();
	afx_msg void	OnAddSegment			();
	afx_msg void	OnRemoveSegment			();
	//}}AFX_MSG
	DECLARE_MESSAGE_MAP()
};

/////////////////////////////////////////////////////////////////////////////
// CXRefUniversalKeyPage dialog
/////////////////////////////////////////////////////////////////////////////
class TB_EXPORT CXRefUniversalKeyPage : public CXRefWizardPage
{
	DECLARE_DYNCREATE(CXRefUniversalKeyPage)

private:
	CTBListBox		m_UkListBox;
	CTBListBox		m_UkSegmentListBox;
	CTranspBmpCtrl	m_Image;


// Construction
public:
	CXRefUniversalKeyPage		();

private:
	void FillUkListBox			();

// Overrides
public:
	virtual void	OnActivate	();
	virtual	LRESULT OnWizardNext();
	virtual	LRESULT OnWizardBack();

// Implementation
protected:
	virtual BOOL OnInitDialog	();

// Generated message map functions
	//{{AFX_MSG(CXRefUniversalKeyPage)
	afx_msg void FillUkSegmentListBox	();
	//}}AFX_MSG
	DECLARE_MESSAGE_MAP()
};

//---------------------------------------------------------------------------------
//class CXRefWizFinishPage 
//---------------------------------------------------------------------------------
class TB_EXPORT CXRefWizFinishPage : public CXRefWizardPage
{
private:
	CTranspBmpCtrl		m_Image;

public:
	CXRefWizFinishPage();

public:
	virtual void	OnActivate		();
	virtual BOOL	OnWizardFinish	();
	virtual	LRESULT OnWizardBack();

protected:
	// Generated message map functions
	//{{AFX_MSG(CXRefSegmentsPage)
	virtual BOOL OnInitDialog	();
	//}}AFX_MSG
	DECLARE_MESSAGE_MAP()
};


/////////////////////////////////////////////////////////////////////////////
// CExpWizMasterDlg dialog
/////////////////////////////////////////////////////////////////////////////
//
class TB_EXPORT CXRefWizMasterDlg : public CWizardMasterDialog
{
	DECLARE_DYNAMIC(CXRefWizMasterDlg)

public:
	BOOL						m_bReadOnly;
	BOOL						m_bDescription;
	CXMLDocObjectInfo*			m_pDocObjectInfo;
	CXRefWizPresentationPage	m_XRefPresentationPage;
	CXRefFieldsSelectPage		m_XRefFieldsSelectPage;
	CXRefFieldsPropPage			m_XRefFieldsPropPage;
	CXRefDocSelPropPage			m_XRefDocumentSelectPage;
	CXRefSegmentsPage			m_XRefSegmentSelectPage;
	CXRefUniversalKeyPage		m_XRefUniversalKeyPage;
	CXRefWizFinishPage			m_XRefWizFinishPage;

	CXMLFieldInfoArray*			m_pXMLFieldInfoArray;
	

public:
	CWizardInformation			m_WizardInfo;

// Construction
#include "endh.dex"

public:
	CXRefWizMasterDlg(CXMLXRefInfo*, CXMLDocObjectInfo*, CXMLFieldInfoArray* = NULL, BOOL = TRUE, CWnd* pParent = NULL);

	virtual BOOL OnWizardFinish	();

// Implementation
protected:

	// Generated message map functions
	//{{AFX_MSG(CXRefWizMasterDlg)
	virtual BOOL OnInitDialog();
	//}}AFX_MSG
	DECLARE_MESSAGE_MAP()
};

#include "endh.dex"

