
#include "stdafx.h"
#ifdef new
#undef new
#endif

#include <TbNameSolver\chars.h>
#include <TbNameSolver\USerMessages.h>

#include <TbFrameworkImages\CommonImages.h>
#include <TbFrameworkImages\GeneralFunctions.h>

#include <TbGeneric\ParametersSections.h>
#include <TbGeneric\LocalizableObjs.h>
#include <TbGeneric\Array.h>
#include <TbGeneric\LineFile.h>

#include <TbGenlib\BaseDoc.h>
#include <TbGenlib\baseapp.h>

#include <TbOledb\Sqlrec.h>
#include <TBWoormEngine\report.h>

#include <TbGes\DBT.h>
#include <TbGes\Hotlink.h>
#include <TbGes\ExtDocAbstract.h>

#include "CMapiSession.h"
#include "CMailConnector.h"

#include "email.h"

#include "tbmailer.hjson" //JSON AUTOMATIC UPDATE
#include "commands.hrc"
#ifdef _DEBUG
#define new DEBUG_NEW
#undef THIS_FILE
static const char THIS_FILE[] = __FILE__;
#endif

///////////////////////////////////////////////////////////////////////////////

BOOL CEditEx::PreTranslateMessage(MSG* pMsg) 
{
	if ((pMsg->message == WM_KEYDOWN) && (pMsg->wParam == VK_TAB)) 
	{
		// get the char index of the caret position
		int nPos = LOWORD(CharFromPos(GetCaretPos()));

		// select zero chars
		SetSel(nPos, nPos);

		// then replace that selection with a TAB
		ReplaceSel(_T("\t"), TRUE);

		// no need to do a msg translation, so quit. 
		// that way no further processing gets done
		return TRUE;
	}
	else if ((pMsg->message == WM_KEYDOWN) && (pMsg->wParam == VK_RETURN)) 
	{
		// get the char index of the caret position
		int nPos = LOWORD(CharFromPos(GetCaretPos()));

		// select zero chars
		SetSel(nPos, nPos);

		// then replace that selection with a TAB
		ReplaceSel(_T("\r\n"), TRUE);

		// no need to do a msg translation, so quit. 
		// that way no further processing gets done
		return TRUE;
	}

	 //just let other massages to work normally
	 return CBCGPEdit::PreTranslateMessage(pMsg);
}
/////////////////////////////////////////////////////////////////////////////////
//

BEGIN_MESSAGE_MAP(CEmailEdit, CStrEdit)
	ON_COMMAND_RANGE		(ID_SHOW_ADDRESS_BOOK, (UINT)(ID_HKL_FACTORY), CmdMenuButton)
	ON_WM_CONTEXTMENU		()
	ON_MESSAGE				(UM_PUSH_BUTTON_CTRL,	OnPushButtonCtrl)
END_MESSAGE_MAP()

//-----------------------------------------------------------------------------
CEmailEdit::CEmailEdit()
	:
	m_pHklCustSuppEmail	(NULL),
	m_pHklContactEmail	(NULL),
	m_pHklCompanyEmail	(NULL),
	m_pHklProducerEmail	(NULL),
	m_pHklProspectiveSuppEmail	(NULL),
	m_pHklBankEmail	(NULL),
	m_pHklCarriersEmail	(NULL)
{
	m_nButtonIDBmp = BTN_MENU_ID;
}

//-----------------------------------------------------------------------------
CEmailEdit::~CEmailEdit()
{
	DetachHotKeyLink ();

	SAFE_DELETE(m_pHklCustSuppEmail);
	SAFE_DELETE(m_pHklContactEmail);
	SAFE_DELETE(m_pHklCompanyEmail);
	SAFE_DELETE(m_pHklProducerEmail);
	SAFE_DELETE(m_pHklProspectiveSuppEmail);
	SAFE_DELETE(m_pHklBankEmail);
	SAFE_DELETE(m_pHklCarriersEmail);
}

//-----------------------------------------------------------------------------
CString CEmailEdit::GetMenuButtonImageNS()
{
	return TBIcon(szIconAddressBook, CONTROL); 
}

//-----------------------------------------------------------------------------
void CEmailEdit::OnContextMenu(CWnd* pWnd, CPoint mousePos)
{	
	CMenu menu;
	menu.CreatePopupMenu();

	if (!GetMenuButton(&menu))
	{
		return;
	}
	
	menu.TrackPopupMenu(TPM_LEFTALIGN | TPM_RIGHTBUTTON, mousePos.x, mousePos.y, this);	
}

//-----------------------------------------------------------------------------
BOOL CEmailEdit::GetMenuButton (CMenu* pMenu)
{
	pMenu->AppendMenu(MF_STRING, ID_SHOW_ADDRESS_BOOK,			_TB("Address Book..."));
	pMenu->AppendMenu(MF_SEPARATOR);

	if (AfxIsActivated(MAGONET_APP, L"Core"))
	{
		pMenu->AppendMenu(MF_STRING, ID_HKL_CUSTOMERS,				_TB("Customers..."));
		pMenu->AppendMenu(MF_STRING, ID_HKL_CONTACTS,				_TB("Contacts..."));
		pMenu->AppendMenu(MF_STRING, ID_HKL_SUPPLIERS,				_TB("Suppliers..."));
		pMenu->AppendMenu(MF_STRING, ID_HKL_PROSPECTIVESUPPLIERS,	_TB("Prospective Suppliers..."));
		pMenu->AppendMenu(MF_STRING, ID_HKL_PRODUCERS,				_TB("Producers..."));
		pMenu->AppendMenu(MF_STRING, ID_HKL_COMPANY,				_TB("Company..."));
		pMenu->AppendMenu(MF_STRING, ID_HKL_BANKS,					_TB("Banks..."));
		pMenu->AppendMenu(MF_STRING, ID_HKL_CARRIERS,				_TB("Carriers..."));
	}
	return TRUE;
}

//-----------------------------------------------------------------------------
LRESULT CEmailEdit::OnPushButtonCtrl(WPARAM wParam, LPARAM)
{
	//DoPushButtonCtrl(wParam, lParam);
	DoCmdMenuButton (wParam);
	return (LRESULT) 0L;
}

//-----------------------------------------------------------------------------
void CEmailEdit::OnShowAddressBook ()
{
	CString strAddress;
	GetValue(strAddress);

	CString strAddressNew;
	if (AfxGetIMailConnector()->MapiShowAddressBook(this->m_hWnd, strAddressNew))
	{
		SetValue(strAddress.IsEmpty() ? strAddressNew : strAddress + ';' + strAddressNew);
		SetModifyFlag(TRUE);
	}
}

//-----------------------------------------------------------------------------
void CEmailEdit::DoCmdMenuButton (UINT nID)
{
	//ReattachHotKeyLink(NULL); //non serve e fa casino: disabilita il pulsante dopo il primo click

	if (nID == ID_SHOW_ADDRESS_BOOK)
	{
		OnShowAddressBook();
		return;
	}
	else if (nID == ID_HKL_CUSTOMERS)
	{
		if (!m_pHklCustSuppEmail)
		{
			m_pHklCustSuppEmail = new DynamicHotKeyLink(_NS_HKL("Extensions.TbMailer.TbMailer.CustSuppEmails"));
		}

		m_pHklCustSuppEmail->SetParamValue(_NID("p_CustSuppType"), &DataLng(3211264));

		ReattachHotKeyLink(m_pHklCustSuppEmail);
	}
	else if (nID == ID_HKL_SUPPLIERS)
	{
		if (!m_pHklCustSuppEmail)
		{
			m_pHklCustSuppEmail = new DynamicHotKeyLink(_NS_HKL("Extensions.TbMailer.TbMailer.CustSuppEmails"));
		}

		m_pHklCustSuppEmail->SetParamValue(_NID("p_CustSuppType"), &DataLng(3211265));

		ReattachHotKeyLink(m_pHklCustSuppEmail);
	}
	else if (nID == ID_HKL_CONTACTS)
	{
		if (!m_pHklContactEmail)
		{
			m_pHklContactEmail = new DynamicHotKeyLink(_NS_HKL("Extensions.TbMailer.TbMailer.ContactEmails"));
		}

		ReattachHotKeyLink(m_pHklContactEmail);
	}
	else if (nID == ID_HKL_PROSPECTIVESUPPLIERS)
	{
		if (!m_pHklProspectiveSuppEmail)
		{
			m_pHklProspectiveSuppEmail = new DynamicHotKeyLink(_NS_HKL("Extensions.TbMailer.TbMailer.ProspectiveSupplierEmails"));
		}

		ReattachHotKeyLink(m_pHklProspectiveSuppEmail);
	}
	else if (nID == ID_HKL_PRODUCERS)
	{
		if (!m_pHklProducerEmail)
		{
			m_pHklProducerEmail = new DynamicHotKeyLink(_NS_HKL("Extensions.TbMailer.TbMailer.ProducerEmails"));
		}

		ReattachHotKeyLink(m_pHklProducerEmail);
	}
	else if (nID == ID_HKL_COMPANY)
	{
		if (!m_pHklCompanyEmail)
		{
			m_pHklCompanyEmail = new DynamicHotKeyLink(_NS_HKL("Extensions.TbMailer.TbMailer.CompanyEmails"));
		}

		ReattachHotKeyLink(m_pHklCompanyEmail);
	}
	else if (nID == ID_HKL_BANKS)
	{
		if (!m_pHklBankEmail)
		{
			m_pHklBankEmail = new DynamicHotKeyLink(_NS_HKL("Extensions.TbMailer.TbMailer.BankEmails"));
		}

		ReattachHotKeyLink(m_pHklBankEmail);
	}
	else if (nID == ID_HKL_CARRIERS)
	{
		if (!m_pHklCarriersEmail)
		{
			m_pHklCarriersEmail = new DynamicHotKeyLink(_NS_HKL("Extensions.TbMailer.TbMailer.CarriersEmails"));
		}

		ReattachHotKeyLink(m_pHklCarriersEmail);
	}
	else
	{
		return;
	}

	if (m_pHotKeyLink == NULL)
		return;

	DoPushButtonCtrl(0, 0);
}

/////////////////////////////////////////////////////////////////////////////
//			Class CEMailDlg Implementation
/////////////////////////////////////////////////////////////////////////////
//
//-----------------------------------------------------------------------------
CEMailDlg::CEMailDlg
	(
		CDocument* pCallerDoc,
		CMapiMessage& e,
		BOOL* pbAttachRDE/*=NULL*/,
		BOOL* pbAttachPDF/*=NULL*/,
		BOOL* pbCompressAttach/*=NULL*/,
		BOOL* pbConcatAttachPDF/*=NULL*/,
		BOOL* pbRequestDeliveryNotification/* = NULL*/,
		BOOL* pbRequestReadNotification/* = NULL*/,
		LPCTSTR	pszCaptionOkBtn/* = NULL*/,
		CString sFromAddress /*= CString()*/
	)
	:
	CParsedDialogWithTiles			(IDD_SEND_EMAIL_DLG),
	m_Email							(e),
	m_pbAttachRDE					(pbAttachRDE),
	m_pbAttachPDF					(pbAttachPDF),
	m_pbCompressAttach				(pbCompressAttach),
	m_pbConcatAttachPDF				(pbConcatAttachPDF),
	m_pbRequestDeliveryNotification	(pbRequestDeliveryNotification),
	m_pbRequestReadNotification		(pbRequestReadNotification),
	m_pszCaptionOkBtn				(pszCaptionOkBtn),
	m_pCallerDoc					(NULL),
	m_sFromAddress					(sFromAddress)
{
	
	if (pCallerDoc)
	{
		ASSERT_VALID(pCallerDoc);
		m_pCallerDoc = dynamic_cast<CAbstractFormDoc*>(pCallerDoc);
	}
}

//-----------------------------------------------------------------------------
CEMailDlg::~CEMailDlg()
{
	ASSERT(_CrtCheckMemory());
}

//-----------------------------------------------------------------------------
IMPLEMENT_DYNAMIC(CEMailDlg, CParsedDialogWithTiles)
//-----------------------------------------------------------------------------
BEGIN_MESSAGE_MAP(CEMailDlg, CParsedDialogWithTiles)
	//{{AFX_MSG_MAP(CUpdateDlg)
	ON_COMMAND(IDC_FINDFILE,			OnClickFindFile)
	ON_MESSAGE(UM_GET_WEB_COMMAND_TYPE, OnGetWebCommandType)
	//}}AFX_MSG_MAP
END_MESSAGE_MAP()

//-----------------------------------------------------------------------------
BOOL CEMailDlg::OnInitDialog()
{
	CParsedDialogWithTiles::OnInitDialog();
	SetToolbarStyle(CParsedDialog::BOTTOM, DEFAULT_TOOLBAR_HEIGHT, TRUE, TRUE);

	m_editFrom.SubclassDlgItem (IDC_FROM,	this);
	m_editSubject.SubclassDlgItem (IDC_SUBJECT1,	this);
	m_editContents.SubclassDlgItem (IDC_CONTENTS,	this);

	m_editTo.SubclassEdit (IDC_TO1,	this, _T("Email_To"));
	m_editCc.SubclassEdit (IDC_CC1,	this, _T("Email_Cc"));
	m_editBcc.SubclassEdit (IDC_BCC1,	this, _T("Email_Bcc"));

	m_editAttachments.SubclassDlgItem (IDC_ATTACHMENTS,	this);
	
	m_editFrom.SetWindowText (m_Email.m_sFrom.IsEmpty() ? m_sFromAddress : m_Email.m_sFrom);

	m_editSubject.SetWindowText (m_Email.m_sSubject);

	CString strText;
	if (m_Email.m_bBodyFromFile)
		LoadLineTextFile(m_Email.m_sBody, strText);
	else
		strText = m_Email.m_sBody;

	m_editContents.SetWindowText (strText);

	MailConnectorParams* params = AfxGetIMailConnector()->GetParams();

	CString str;

	m_btnAttachRDE.						SubclassDlgItem (IDC_CHECK_RDE,	this);
	m_btnAttachPDF.						SubclassDlgItem (IDC_CHECK_PDF,	this);
	m_btnCompressAttach.				SubclassDlgItem (IDC_CHECK_COMPRESS,	this);
	m_btnConcatAttachPDF.				SubclassDlgItem (IDC_CHECK_CONCAT_PDF,	this);
	m_btnRequestDeliveryNotification.	SubclassDlgItem (IDC_CHECK_REQUEST_DELIVERY_NOTIFICATION,	this);
	m_btnRequestReadNotification.		SubclassDlgItem (IDC_CHECK_REQUEST_READ_NOTIFICATION,	this);
	
	m_btnRequestDeliveryNotification.	EnableWindow(!params->GetUseMapi());
	m_btnRequestReadNotification.		EnableWindow(!params->GetUseMapi());

	m_btnAttachRDE.			ShowWindow (m_pbAttachRDE ? SW_SHOW : SW_HIDE);
	m_btnAttachPDF.			ShowWindow (m_pbAttachPDF ? SW_SHOW : SW_HIDE);
	m_btnCompressAttach.	ShowWindow (m_pbCompressAttach ? SW_SHOW : SW_HIDE);
	m_btnConcatAttachPDF.	ShowWindow (m_pbConcatAttachPDF ? SW_SHOW : SW_HIDE);

	m_btnAttachRDE.			SetCheck (m_pbAttachRDE && *m_pbAttachRDE ? 1 : 0);
	m_btnAttachPDF.			SetCheck (m_pbAttachPDF && *m_pbAttachPDF ? 1 : 0);
	m_btnCompressAttach.	SetCheck (m_pbCompressAttach && *m_pbCompressAttach ? 1 : 0);
	m_btnConcatAttachPDF.	SetCheck (m_pbConcatAttachPDF && *m_pbConcatAttachPDF ? 1 : 0);

	m_btnRequestDeliveryNotification.	SetCheck (m_pbRequestDeliveryNotification && *m_pbRequestDeliveryNotification ? 1 : 0);
	m_btnRequestReadNotification.		SetCheck (m_pbRequestReadNotification && *m_pbRequestReadNotification ? 1 : 0);

	//m_editFrom.SetWindowText (m_Email.m_sFrom);

	str.Empty();
	CStringArray_Concat (m_Email.m_To, str);	
	m_editTo.SetWindowText (str);

	str.Empty();
	CStringArray_Concat (m_Email.m_CC, str);	
	m_editCc.SetWindowText (str);

	str.Empty();
	CStringArray_Concat (m_Email.m_BCC, str);	
	m_editBcc.SetWindowText (str);

	str.Empty();
	CStringArray_ConcatAttachmentsWithTitle (m_Email.m_Attachments, m_Email.m_AttachmentTitles, str);	
	m_editAttachments.SetWindowText (str);

	//----
	m_editTo.Attach(&m_sTo);
	m_editCc.Attach(&m_sCc);
	m_editBcc.Attach(&m_sBcc);

	return TRUE;
}

//----------------------------------------------------------------------------
void CEMailDlg::OnCustomizeToolbar()
{
	m_pToolBar->AddButtonToRight(IDOK, _NS_TOOLBARBTN("&Ok"), TBIcon(szIconOk, TOOLBAR), _TB("Ok"), _TB("Ok\nOk"));
	m_pToolBar->AddButtonToRight(IDCANCEL, _NS_TOOLBARBTN("&Cancel"), TBIcon(szIconEscape, TOOLBAR), _TB("Cancel"), _TB("Cancel\nCancel"));
	m_pToolBar->AddButton(IDC_FINDFILE, _NS_TOOLBARBTN("&Attachments"), TBIcon(szIconAttach, TOOLBAR), _TB("Attachments"), _TB("Attachments\nAttachments"));
	m_pToolBar->SetDefaultAction(IDOK);
}

//-----------------------------------------------------------------------------
void CEMailDlg::OnOK ()
{
	//m_editFrom.GetWindowText(m_Email.m_sFrom);
	m_editSubject.GetWindowText(m_Email.m_sSubject);

	if (m_Email.m_bBodyFromFile)
		m_Email.m_bBodyFromFile = FALSE;

	m_editContents.GetWindowText(m_Email.m_sBody);

	CString str;

	m_Email.m_To.RemoveAll();
	m_editTo.GetWindowText(str);
	CStringArray_Split (m_Email.m_To, str);	

	m_Email.m_CC.RemoveAll();
	m_editCc.GetWindowText(str);
	CStringArray_Split (m_Email.m_CC, str);	

	m_Email.m_BCC.RemoveAll();
	m_editBcc.GetWindowText(str);
	CStringArray_Split (m_Email.m_BCC, str);	

	if (m_Email.m_To.GetSize() == 0 && m_Email.m_CC.GetSize() == 0 && m_Email.m_BCC.GetSize() == 0)
	{
		AfxMessageBox(_TB("\"To\" address is missing"));
		return;
	}

	m_Email.m_Attachments.RemoveAll();
	m_Email.m_AttachmentTitles.RemoveAll();
	m_editAttachments.GetWindowText(str);
	CStringArray_SplitAttachmentsWithTitle (m_Email.m_Attachments, m_Email.m_AttachmentTitles, str);
	
	if (m_pbAttachRDE) 
		*m_pbAttachRDE = m_btnAttachRDE.GetCheck() == 1;
	if (m_pbAttachPDF) 
		*m_pbAttachPDF = m_btnAttachPDF.GetCheck() == 1;
	if (m_pbCompressAttach) 
		*m_pbCompressAttach = m_btnCompressAttach.GetCheck() == 1;
	if (m_pbConcatAttachPDF) 
		*m_pbConcatAttachPDF = m_btnConcatAttachPDF.GetCheck() == 1;

	if (m_pbRequestDeliveryNotification) 
		*m_pbRequestDeliveryNotification = m_btnRequestDeliveryNotification.GetCheck() == 1;
	if (m_pbRequestReadNotification) 
		*m_pbRequestReadNotification = m_btnRequestReadNotification.GetCheck() == 1;

	m_Email.m_deEmailAddressType = E_EMAIL_ADDRESS_TYPE_ALL;

	CParsedDialogWithTiles::OnOK();
}

// Consento di browsare su tutto il disco per gli allegati di una email, senza
// utilizzare la TBExplorer che limiterebbe la funzionalità
//-----------------------------------------------------------------------------
void CEMailDlg::OnClickFindFile()
{	
	CString strAttachments;
	m_editAttachments.GetWindowText(strAttachments);

	if (AfxGetOleDbMng()->EasyAttachmentEnable() && m_pCallerDoc && m_pCallerDoc->ValidCurrentRecord() && 
		AfxGetIDMSRepositoryManager()->GetAttachmentsCount(m_pCallerDoc->GetNamespace().ToString(), m_pCallerDoc->m_pDBTMaster->GetRecord()->GetPrimaryKeyNameValue(), OnlyAttachment) > 0  &&
		AfxMessageBox(_TB("Would you like to choose from EasyAttachment documents?"),  MB_YESNO) == IDYES
		)
	{		
		CUIntArray selectAttachments;
		bool bOnlyForMail = false; //@@BAUZI : da settare con un parametro
		m_pCallerDoc->GetDMSAttachmentManager()->OpenAttachmentsListForm(&selectAttachments, bOnlyForMail);
		for (int i = 0; i < selectAttachments.GetCount(); i++)
		{
			CString strFile = AfxGetIDMSRepositoryManager()->GetAttachmentTempFile(selectAttachments.GetAt(i));
			if (::ExistFile(strFile))
			{
				CString strT;
				int nPos = strFile.ReverseFind(SLASH_CHAR);
				if (nPos < 0)
					strT = strFile;
				else
					strT = strFile.Right(strFile.GetLength() - nPos -1);

				strFile = strFile + _T(" <")+ strT + _T(">;\r\n");
				strAttachments = strFile + strAttachments;
			}
		}
		selectAttachments.RemoveAll();
	}
	else
	{
		TCHAR szBuff[8128]; szBuff[0] = '\0';
		CString title = _TB("Insert File");
		CFileDialog  dlgFile(TRUE, _T(""), _T("c:\\*.*"), OFN_ALLOWMULTISELECT, _T(""), this);

		dlgFile.m_ofn.lpstrTitle =title;
		dlgFile.m_ofn.lpstrFile = szBuff;
		dlgFile.m_ofn.nMaxFile = 4096;

		if (dlgFile.DoModal() != IDOK) 
		{
			return;
		}
	
	
	
		POSITION p = dlgFile.GetStartPosition();
		while (p != NULL)
		{
			CString strFile = dlgFile.GetNextPathName(p);
		
			CString strT;
			int nPos = strFile.ReverseFind(SLASH_CHAR);
			if (nPos < 0)
				strT = strFile;
			else
				strT = strFile.Right(strFile.GetLength() - nPos -1);

			strFile = strFile + _T(" <")+ strT + _T(">;\r\n");

			strAttachments = strFile + strAttachments;
		}
	}

	m_editAttachments.SetWindowText(strAttachments);
}

//-----------------------------------------------------------------------------
void CEMailDlg::OnClickShowAddressBookTo()
{
	CString str, old;
	m_editTo.GetWindowText(str);
	old = str;

	if (AfxGetIMailConnector()->MapiShowAddressBook(m_hWnd, str))
	{
		m_editTo.SetWindowText(str);
	}
	else if (str != old && !str.IsEmpty())
		m_editTo.SetWindowText(str);
}

//-----------------------------------------------------------------------------
void CEMailDlg::OnClickShowAddressBookCc()
{
	CString str, old;
	m_editCc.GetWindowText(str);
	old = str;

	if (AfxGetIMailConnector()->MapiShowAddressBook(m_hWnd, str))
	{
		m_editCc.SetWindowText(str);
	}
	else if (str != old && !str.IsEmpty())
		m_editCc.SetWindowText(str);
}

//-----------------------------------------------------------------------------
void CEMailDlg::OnClickShowAddressBookBcc()
{
	CString str, old;
	m_editBcc.GetWindowText(str);
	old = str;

	if (AfxGetIMailConnector()->MapiShowAddressBook(m_hWnd, str))
	{
		m_editBcc.SetWindowText(str);
	}
	else if (str != old && !str.IsEmpty())
		m_editBcc.SetWindowText(str);
}

//-----------------------------------------------------------------------------
LRESULT CEMailDlg::OnGetWebCommandType(WPARAM wParam, LPARAM lParam)
{
	WebCommandType* type = (WebCommandType*)lParam;
	*type = WEB_UNDEFINED;
	if (wParam == IDC_FINDFILE)
		*type = WEB_UNSUPPORTED;
	return 1L;
}

///////////////////////////////////////////////////////////////////////////////
CEMailWithChildDlg::CEMailWithChildDlg 
				(
					BOOL* pbAttachRDE/*=NULL*/, 
					BOOL* pbAttachPDF/*=NULL*/, 
					BOOL* pbCompressAttach/*=NULL*/,
					BOOL* pbConcatAttachPDF/*=NULL*/
				)
	:
	CParsedDialogWithTiles (IDD_SEND_EMAILCHILDS),
	m_pbAttachRDE(pbAttachRDE),
	m_pbAttachPDF(pbAttachPDF),
	m_pbCompressAttach(pbCompressAttach),
	m_pbConcatAttachPDF(pbConcatAttachPDF)
{
}

//-----------------------------------------------------------------------------
IMPLEMENT_DYNAMIC(CEMailWithChildDlg, CParsedDialogWithTiles)
//-----------------------------------------------------------------------------
BEGIN_MESSAGE_MAP(CEMailWithChildDlg, CParsedDialogWithTiles)

END_MESSAGE_MAP()

//-----------------------------------------------------------------------------
BOOL CEMailWithChildDlg::OnInitDialog	()
{
	__super::OnInitDialog();
	SetToolbarStyle(CParsedDialog::BOTTOM, DEFAULT_TOOLBAR_HEIGHT, TRUE, TRUE);
	//MailConnectorParams* params = AfxGetIMailConnector()->GetParams();

	m_btnAttachRDE.			SubclassDlgItem (IDC_CHECK_RDE,	this);
	m_btnAttachPDF.			SubclassDlgItem (IDC_CHECK_PDF,	this);
	m_btnCompressAttach.	SubclassDlgItem (IDC_CHECK_COMPRESS,	this);
	m_btnConcatAttachPDF.	SubclassDlgItem (IDC_CHECK_CONCAT_PDF,	this);

	m_btnAttachRDE.			ShowWindow	(m_pbAttachRDE ? SW_SHOW : SW_HIDE);
	m_btnAttachPDF.			ShowWindow	(m_pbAttachPDF ? SW_SHOW : SW_HIDE);
	m_btnCompressAttach.	ShowWindow	(m_pbCompressAttach ? SW_SHOW : SW_HIDE);
	m_btnConcatAttachPDF.	ShowWindow	(m_pbConcatAttachPDF ? SW_SHOW : SW_HIDE);

	m_btnAttachRDE.			SetCheck	(m_pbAttachRDE && *m_pbAttachRDE ? 1 : 0);
	m_btnAttachPDF.			SetCheck	(m_pbAttachPDF && *m_pbAttachPDF ? 1 : 0);
	m_btnCompressAttach.	SetCheck	(m_pbCompressAttach && *m_pbCompressAttach ? 1 : 0);
	m_btnConcatAttachPDF.	SetCheck	(m_pbConcatAttachPDF && *m_pbConcatAttachPDF ? 1 : 0);

	return TRUE;
}

//----------------------------------------------------------------------------
void CEMailWithChildDlg::OnCustomizeToolbar()
{
	m_pToolBar->AddButtonToRight(IDOK, _NS_TOOLBARBTN("&Ok"), TBIcon(szIconOk, TOOLBAR), _TB("Ok"), _TB("Ok\nOk"));
	m_pToolBar->AddButtonToRight(IDCANCEL, _NS_TOOLBARBTN("&Cancel"), TBIcon(szIconEscape, TOOLBAR), _TB("Cancel"), _TB("Cancel\nCancel"));
	m_pToolBar->SetDefaultAction(IDOK);
}

//-----------------------------------------------------------------------------
void CEMailWithChildDlg::OnOK ()
{	
	CString str;
	if (m_pbAttachRDE) 
		*m_pbAttachRDE = m_btnAttachRDE.GetCheck() == 1;
	if (m_pbAttachPDF) 
		*m_pbAttachPDF = m_btnAttachPDF.GetCheck() == 1;
	if (m_pbCompressAttach) 
		*m_pbCompressAttach = m_btnCompressAttach.GetCheck() == 1;
	if (m_pbConcatAttachPDF) 
		*m_pbConcatAttachPDF = m_btnConcatAttachPDF.GetCheck() == 1;

	__super::OnOK();
}
//-----------------------------------------------------------------------------

