
#pragma once

#include <TbGeneric\LocalizableObjs.h>
#include <TbGeneric\CMapi.h>

#include <TBGenlib\TBStrings.h>
#include <TBGenlib\parsedt.h>

#include <TbGes\TileManager.h>

//includere alla fine degli include del .H
#include "beginh.dex"

class DynamicHotKeyLink;

/////////////////////////////////////////////////////////////////////////////
//
class TB_EXPORT CEditEx: public CBCGPEdit
{
public:
	CEditEx() {};

	virtual BOOL PreTranslateMessage(MSG *);
};

/////////////////////////////////////////////////////////////////////////////
//
class TB_EXPORT CEmailEdit: public CStrEdit
{
public:
	CEmailEdit();
	~CEmailEdit();

	afx_msg void	OnContextMenu		(CWnd* pWnd, CPoint ptMousePos);
	afx_msg void	OnShowAddressBook	();
	afx_msg	void	CmdMenuButton		(UINT nID) { DoCmdMenuButton(nID);}

	virtual	BOOL	GetMenuButton		(CMenu*);
	virtual	void	DoCmdMenuButton		(UINT nID);
	virtual CString	GetMenuButtonImageNS();

protected:
	DynamicHotKeyLink*	m_pHklCustSuppEmail;
	DynamicHotKeyLink*	m_pHklContactEmail;
	DynamicHotKeyLink*	m_pHklProspectiveSuppEmail;
	DynamicHotKeyLink*	m_pHklProducerEmail;
	DynamicHotKeyLink*	m_pHklCompanyEmail;
	DynamicHotKeyLink*	m_pHklBankEmail;
	DynamicHotKeyLink*	m_pHklCarriersEmail;

	afx_msg	LRESULT	OnPushButtonCtrl				(WPARAM wParam, LPARAM lParam);

	DECLARE_MESSAGE_MAP()
};

/////////////////////////////////////////////////////////////////////////////
//							CEMailDlg
class TB_EXPORT CEMailDlg : public CParsedDialogWithTiles
{
	DECLARE_DYNAMIC(CEMailDlg)
public:
	CMapiMessage&		m_Email;

	CBCGPEdit	m_editFrom;
	CBCGPEdit	m_editSubject;
	CEditEx		m_editContents;

	DataStr		m_sTo;
	DataStr		m_sCc;
	DataStr		m_sBcc;

	CEmailEdit		m_editTo;
	CEmailEdit		m_editCc;
	CEmailEdit		m_editBcc;

	CBCGPEdit	m_editAttachments;

	CButton		m_btnAttachRDE;
	CButton		m_btnAttachPDF;
	CButton		m_btnCompressAttach;
	CButton		m_btnConcatAttachPDF;

	BOOL*		m_pbAttachRDE;
	BOOL*		m_pbAttachPDF;
	BOOL*		m_pbCompressAttach;
	BOOL*		m_pbConcatAttachPDF;

	BOOL*		m_pbRequestDeliveryNotification;
	BOOL*		m_pbRequestReadNotification;

	CButton		m_btnRequestDeliveryNotification;
	CButton		m_btnRequestReadNotification;

	LPCTSTR		m_pszCaptionOkBtn;

	CAbstractFormDoc* m_pCallerDoc;
	CString		m_sFromAddress;

public:
	CEMailDlg
		(
			CDocument* pCallerDoc,
			CMapiMessage& e, 
			BOOL* pbAttachRDE = NULL, 
			BOOL* pbAttachPDF = NULL, 
			BOOL* pbCompressAttach = NULL,
			BOOL* pbConcatAttachPDF = NULL,
			BOOL* pbRequestDeliveryNotification = NULL,
			BOOL* pbRequestReadNotification = NULL,
			LPCTSTR	pszCaptionOkBtn = NULL,
			CString sFromAddress = CString()
		);
	~CEMailDlg();
		
protected:
	virtual	BOOL	OnInitDialog		();
	virtual	void	OnOK				();
	virtual void	OnCustomizeToolbar	();

	void OnClickFindFile ();
	void OnClickShowAddressBookTo ();
	void OnClickShowAddressBookCc ();
	void OnClickShowAddressBookBcc ();

	afx_msg	LRESULT OnGetWebCommandType (WPARAM wParam, LPARAM lParam);

	DECLARE_MESSAGE_MAP()
};

///////////////////////////////////////////////////////////////////////////////
class TB_EXPORT CEMailWithChildDlg : public CParsedDialogWithTiles
{

	DECLARE_DYNAMIC(CEMailWithChildDlg)
protected:
	CButton		m_btnAttachRDE;
	CButton		m_btnAttachPDF;
	CButton		m_btnCompressAttach;
	CButton		m_btnConcatAttachPDF;

	BOOL*		m_pbAttachRDE;
	BOOL*		m_pbAttachPDF;
	BOOL*		m_pbCompressAttach;
	BOOL*		m_pbConcatAttachPDF;

public:
	CEMailWithChildDlg
		(
			BOOL* pbAttachRDE = NULL, 
			BOOL* pbAttachPDF = NULL, 
			BOOL* pbCompressAttach = NULL,
			BOOL* pbConcatAttachPDF = NULL
		);

protected:
	virtual	BOOL	OnInitDialog		();
	virtual	void	OnOK				();
	virtual void	OnCustomizeToolbar	();

	DECLARE_MESSAGE_MAP()
};

//////////////////////////////////////////////////////////////////////////////////////
#include "endh.dex"
