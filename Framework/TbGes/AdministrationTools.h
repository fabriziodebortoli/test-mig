
#pragma once

#include <TbGenlib\Parsobj.h>
#include <TbGenlib\TBToolbar.h>
#include <TbGenlib\BaseTileDialog.h>

//includere alla fine degli include del .H
#include "beginh.dex"


//==============================================================================
//          Class CAdminDocDescriptionDlg implementation
//==============================================================================
class CAdminDocDescriptionDlg : public CParsedDialogWithTiles
{
	DECLARE_DYNAMIC(CAdminDocDescriptionDlg)
private:
	CImageList			m_ImageList;
private:
	CTBNamespace 			m_nsDoc;
	CAbstractFormDoc*		m_pDocument;
	CDocDescrTreeCtrl		m_TreeCtrl;
	int						m_nProfileVersion;
	int						m_nMaxDocument;
	CBCGPComboBox			m_cbEnvelopeClass;
	CXMLDocInfo*			m_pOldDocObjInfo;

public:
	CAdminDocDescriptionDlg();
	~CAdminDocDescriptionDlg();

private:
	void RisizeDialogRect();
	void InitToolbar();

protected:
	virtual BOOL OnInitDialog();
	virtual void OnCustomizeToolbar();
	virtual void ResizeOtherComponents(CRect aRect);

protected:
	//{{AFX_MSG(CAdminDocDescriptionDlg)
	//}}AFX_MSG
	DECLARE_MESSAGE_MAP()
};


//----------------------------------------------------------------------------------
// CDocDescrMngPage dialog
//----------------------------------------------------------------------------------
class TB_EXPORT CDocDescrMngPage : public CLocalizablePropertyPage
{
private:
	CTBNamespace 			m_nsDoc;
	CAbstractFormDoc*		m_pDocument;
	CDocDescrTreeCtrl		m_TreeCtrl;
	int						m_nProfileVersion;
	int						m_nMaxDocument;
	CBCGPComboBox			m_cbEnvelopeClass;
	CXMLDocInfo*			m_pOldDocObjInfo;

public:
	BOOL					m_bModified;

public:
	CDocDescrMngPage(CWnd* pParent = NULL);   // standard constructor
	CDocDescrMngPage(const CTBNamespace&, CAbstractFormDoc* = NULL);
	CDocDescrMngPage(const CTBNamespace&, LPCTSTR, CAbstractFormDoc* = NULL);
	~CDocDescrMngPage();

private:
	void	FillHeader();
	void	FillEnvClassCombo(const CString&);
	void	UpdateDataValue(CXMLDocObjectInfo*);

public:
	virtual void DoDataExchange(CDataExchange* pDX);    // DDX/DDV support
	virtual void OnOK();
	virtual void OnCancel();

protected:
	// Generated message map functions
	//{{AFX_MSG(CDocDescrMngPage)
	virtual BOOL OnInitDialog();
	afx_msg void OnDeltaPosSpinVersion(NMHDR*, LRESULT*);
	afx_msg void OnContextMenu(CWnd*, CPoint);
	//}}AFX_MSG
	DECLARE_MESSAGE_MAP()
};
