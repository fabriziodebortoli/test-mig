
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
#include <TbGenlibManaged\PostaLiteNet.h>
#include <TbOledb\Sqlrec.h>
#include <TBWoormEngine\report.h>
#include <TbGes\DBT.h>
#include <TbGes\ExtDocAbstract.h>

#include "CMapiSession.h"
#include "CMailConnector.h"

#include "email.h"
#include "SendPostaLiteDlg.h"

#include "tbmailer.hjson" //JSON AUTOMATIC UPDATE
#include "postalite.hjson" //JSON AUTOMATIC UPDATE
#include "commands.hrc"
#ifdef _DEBUG
#define new DEBUG_NEW
#undef THIS_FILE
static const char THIS_FILE[] = __FILE__;
#endif

///////////////////////////////////////////////////////////////////////////////

BEGIN_MESSAGE_MAP(CAddresseeEdit, CStrEdit)
	ON_COMMAND_RANGE		(ID_SHOW_ADDRESS_BOOK, (UINT)(ID_HKL_FACTORY), CmdMenuButton)
	ON_WM_CONTEXTMENU		()
	ON_MESSAGE				(UM_PUSH_BUTTON_CTRL,	OnPushButtonCtrl)
END_MESSAGE_MAP()

//-----------------------------------------------------------------------------
CAddresseeEdit::CAddresseeEdit()
	:
	m_pHklCustSuppAddrs	(NULL),
	m_pHklContactEmail	(NULL),
	m_pHklCompanyEmail	(NULL),
	m_pHklProducerEmail	(NULL),
	m_pHklProspectiveSuppEmail	(NULL),
	m_pHklBankEmail	(NULL),
	m_pHklCarriersEmail	(NULL),
	m_pHklStoragesEmail	(NULL)
{
	m_nButtonIDBmp = BTN_MENU_ID;
}

//-----------------------------------------------------------------------------
CAddresseeEdit::~CAddresseeEdit()
{
	DetachHotKeyLink ();

	SAFE_DELETE(m_pHklCustSuppAddrs);
	SAFE_DELETE(m_pHklContactEmail);
	SAFE_DELETE(m_pHklCompanyEmail);
	SAFE_DELETE(m_pHklProducerEmail);
	SAFE_DELETE(m_pHklProspectiveSuppEmail);
	SAFE_DELETE(m_pHklBankEmail);
	SAFE_DELETE(m_pHklCarriersEmail);
	SAFE_DELETE(m_pHklStoragesEmail);
}

//-----------------------------------------------------------------------------
CString CAddresseeEdit::GetMenuButtonImageNS()
{
	return TBIcon(szIconAddressBook, CONTROL); 
}

//-----------------------------------------------------------------------------
void CAddresseeEdit::OnContextMenu(CWnd* pWnd, CPoint mousePos)
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
BOOL CAddresseeEdit::GetMenuButton (CMenu* pMenu)
{
	if (AfxIsActivated(MAGONET_APP, L"Core"))
	{
		pMenu->AppendMenu(MF_STRING, ID_HKL_CUSTOMERS,				_TB("Customers..."));
		pMenu->AppendMenu(MF_STRING, ID_HKL_CONTACTS,				_TB("Contacts..."));
		pMenu->AppendMenu(MF_STRING, ID_HKL_SUPPLIERS,				_TB("Suppliers..."));
		pMenu->AppendMenu(MF_STRING, ID_HKL_PROSPECTIVESUPPLIERS,	_TB("Prospective Suppliers..."));
		pMenu->AppendMenu(MF_STRING, ID_HKL_PRODUCERS,				_TB("Producers..."));
		//pMenu->AppendMenu(MF_STRING, ID_HKL_COMPANY,				_TB("Company..."));
		pMenu->AppendMenu(MF_STRING, ID_HKL_BANKS,					_TB("Banks..."));
		pMenu->AppendMenu(MF_STRING, ID_HKL_CARRIERS,				_TB("Carriers..."));
		pMenu->AppendMenu(MF_STRING, ID_HKL_STORAGES,				_TB("Storages..."));
	}
	return TRUE;
}


//-----------------------------------------------------------------------------
LRESULT CAddresseeEdit::OnPushButtonCtrl(WPARAM wParam, LPARAM)
{
	DoCmdMenuButton (wParam);
	return (LRESULT) 0L;
}

//-----------------------------------------------------------------------------
void CAddresseeEdit::DoCmdMenuButton (UINT nID)
{
	//ReattachHotKeyLink(NULL); //non serve e fa casino: disabilita il pulsante dopo il primo click

	if (nID == ID_HKL_CUSTOMERS)
	{
		if (!m_pHklCustSuppAddrs)
			m_pHklCustSuppAddrs = new DynamicHotKeyLink(_NS_HKL("Extensions.TbMailer.TbMailer.CustSuppAddress"));

		m_pHklCustSuppAddrs->SetParamValue(_NID("p_CustSuppType"), &DataLng(3211264));

		ReattachHotKeyLink(m_pHklCustSuppAddrs);
	}
	else if (nID == ID_HKL_SUPPLIERS)
	{
		if (!m_pHklCustSuppAddrs)
			m_pHklCustSuppAddrs = new DynamicHotKeyLink(_NS_HKL("Extensions.TbMailer.TbMailer.CustSuppAddress"));

		m_pHklCustSuppAddrs->SetParamValue(_NID("p_CustSuppType"), &DataLng(3211265));

		ReattachHotKeyLink(m_pHklCustSuppAddrs);
	}
	else if (nID == ID_HKL_CONTACTS)
	{
		if (!m_pHklContactEmail)
			m_pHklContactEmail = new DynamicHotKeyLink(_NS_HKL("Extensions.TbMailer.TbMailer.ContactAddress"));

		ReattachHotKeyLink(m_pHklContactEmail);
	}
	else if (nID == ID_HKL_PROSPECTIVESUPPLIERS)
	{
		if (!m_pHklProspectiveSuppEmail)
			m_pHklProspectiveSuppEmail = new DynamicHotKeyLink(_NS_HKL("Extensions.TbMailer.TbMailer.ProspectiveSupplierAddress"));

		ReattachHotKeyLink(m_pHklProspectiveSuppEmail);
	}
	else if (nID == ID_HKL_PRODUCERS)
	{
		if (!m_pHklProducerEmail)
			m_pHklProducerEmail = new DynamicHotKeyLink(_NS_HKL("Extensions.TbMailer.TbMailer.ProducerAddress"));

		ReattachHotKeyLink(m_pHklProducerEmail);
	}
	else if (nID == ID_HKL_COMPANY)
	{
		if (!m_pHklCompanyEmail)
			m_pHklCompanyEmail = new DynamicHotKeyLink(_NS_HKL("Extensions.TbMailer.TbMailer.CompanyAddress"));

		ReattachHotKeyLink(m_pHklCompanyEmail);
	}
	else if (nID == ID_HKL_BANKS)
	{
		if (!m_pHklBankEmail)
			m_pHklBankEmail = new DynamicHotKeyLink(_NS_HKL("Extensions.TbMailer.TbMailer.BankAddress"));

		ReattachHotKeyLink(m_pHklBankEmail);
	}
	else if (nID == ID_HKL_CARRIERS)
	{
		if (!m_pHklCarriersEmail)
			m_pHklCarriersEmail = new DynamicHotKeyLink(_NS_HKL("Extensions.TbMailer.TbMailer.CarrierAddress"));

		ReattachHotKeyLink(m_pHklCarriersEmail);
	}
	else if (nID == ID_HKL_STORAGES)
	{
		if (!m_pHklStoragesEmail)
			m_pHklStoragesEmail = new DynamicHotKeyLink(_NS_HKL("Extensions.TbMailer.TbMailer.StorageAddress"));

		ReattachHotKeyLink(m_pHklStoragesEmail);
	}
	else
	{
		return;
	}

	if (m_pHotKeyLink == NULL)
		return;

	DoPushButtonCtrl(0, 0);
}

//-----------------------------------------------------------------------------
CString CAddresseeEdit::GetRefDocNamespace(HotKeyLink* pHkl/*=NULL*/)
{
	if (!pHkl)
		pHkl = (HotKeyLink*) this->GetHotLink();
	if (!pHkl)
		return L"";
	ASSERT_VALID(pHkl);

	if (pHkl == m_pHklCustSuppAddrs)
	{
		DataEnum deTCS (*(DataEnum*)(m_pHklCustSuppAddrs->GetAttachedRecord()->GetDataObjFromColumnName(_T("CustSuppType"))));
		return deTCS == 3211264 ? 
			_NS_DOC("Document.ERP.CustomersSuppliers.Documents.Customers")
			: 
			_NS_DOC("Document.ERP.CustomersSuppliers.Documents.Suppliers")
			;
	}
	else if (pHkl == m_pHklContactEmail)
		return _NS_DOC("Document.ERP.Contacts.Documents.Contacts");
	else if (pHkl == m_pHklProspectiveSuppEmail)
		return _NS_DOC("Document.ERP.Contacts.Documents.ProspectiveSuppliers");
	else if (pHkl == m_pHklProducerEmail)
		return _NS_DOC("Document.ERP.CustomersSuppliers.Documents.Producers");
	else if (pHkl == m_pHklCompanyEmail)
		return _NS_DOC("Document.ERP.Company.Documents.Company");
	else if (pHkl == m_pHklBankEmail)
		return _NS_DOC("Document.ERP.Banks.Documents.CustSuppBanks");
	else if (pHkl == m_pHklCarriersEmail)
		return _NS_DOC("Document.ERP.Banks.Documents.Carriers");
	else if (pHkl == m_pHklStoragesEmail)
		return _NS_DOC("Document.ERP.Banks.Documents.Storages");

	return L"";
}

//-----------------------------------------------------------------------------

///////////////////////////////////////////////////////////////////////////////
CSendPostaLiteDlg::CSendPostaLiteDlg 
				(
					CDocument* pCallerDoc,
					CMapiMessage& msg
				)
	:
	CParsedDialogWithTiles (IDD_SEND_POSTALITE_DLG),
	m_Email(msg),
	m_pCallerDoc (NULL),
	m_deDeliveryType (E_POSTALITE_DELIVERY_TYPE_DEFAULT),
	m_dePrintType (E_POSTALITE_PRINT_TYPE_DEFAULT)
{
	if (pCallerDoc && pCallerDoc->IsKindOf(RUNTIME_CLASS(CBaseDocument)))
		m_pCallerDoc = (CAbstractFormDoc*) pCallerDoc;

	m_dsSubject = m_Email.m_sSubject;

	m_fontZucchetti.CreateFont
		(
			14, 0, 0, 0,
			FW_NORMAL, TRUE, FALSE, FALSE, ANSI_CHARSET,
			OUT_DEFAULT_PRECIS, CLIP_DEFAULT_PRECIS, DEFAULT_QUALITY, 
			DEFAULT_PITCH | FF_SWISS, _T("Verdana")
		);
	
	CParsedForm::SetBackgroundImage(IDB_POSTALITE_BIG_IMAGE);
	//this->CDialogEx::SetBackgroundImage(IDB_POSTALITE_BIG_IMAGE);
}

CSendPostaLiteDlg::~CSendPostaLiteDlg()
{
	m_fontZucchetti.DeleteObject();
	ASSERT(_CrtCheckMemory());
}

//-----------------------------------------------------------------------------
IMPLEMENT_DYNAMIC(CSendPostaLiteDlg, CParsedDialogWithTiles)

//-----------------------------------------------------------------------------
BEGIN_MESSAGE_MAP(CSendPostaLiteDlg, CParsedDialogWithTiles)

	ON_MESSAGE			(UM_GET_WEB_COMMAND_TYPE,	OnGetWebCommandType)
	ON_EN_VALUE_CHANGED	(IDC_PL_CITY,				OnCityChanged)
	ON_CONTROL			(EN_AFTER_VALUE_CHANGED_BY_HKL, IDC_PL_ADDRESSEE, OnAfterRecordSelected)
	ON_EN_VALUE_CHANGED	(IDC_PL_DELIVERYTYPE,		OnDeliveryTypeChanged)

END_MESSAGE_MAP()

//----------------------------------------------------------------------------
void CSendPostaLiteDlg::OnCustomizeToolbar()
{
	m_pToolBar->AddButtonToRight(IDOK, _NS_TOOLBARBTN("&Ok"), TBIcon(szIconOk, TOOLBAR), _TB("Ok"), _TB("Confirm "));
	m_pToolBar->AddButtonToRight(IDCANCEL, _NS_TOOLBARBTN("&Cancel"), TBIcon(szIconEscape, TOOLBAR), _TB("Cancel"), _TB("Cancel"));
}

//-----------------------------------------------------------------------------
BOOL CSendPostaLiteDlg::OnInitDialog()
{
	__super::OnInitDialog();
	SetToolbarStyle(CParsedDialog::BOTTOM, DEFAULT_TOOLBAR_HEIGHT, TRUE, TRUE);

	m_Email.SetUsePostaLite(TRUE);

	if (m_Email.m_PostaLiteMsg.GetCount())
	{
		m_Addr = *(CPostaLiteAddress*) m_Email.m_PostaLiteMsg.GetAt(m_Email.m_PostaLiteMsg.GetCount() - 1);
		
		if (!m_Addr.m_sFax.IsEmpty() && m_Addr.m_sFax[0] == '+')
			m_Addr.m_sCountry = _T("00") + m_Addr.m_sFax.Mid(1);

		if (m_Addr.m_sISOCode == _T("IT") && m_Addr.m_sCountry.IsEmpty())
			m_Addr.m_sCountry = _T("Italia");

		m_dsAddressee	= m_Addr.m_sAddressee;
		m_dsAddress		= m_Addr.m_sAddress;
		m_dsCity		= m_Addr.m_sCity;
  		m_dsZipCode		= m_Addr.m_sZipCode;
		m_dsCounty		= m_Addr.m_sCounty;
		m_dsCountry		= m_Addr.m_sCountry;
		m_dsISOCode		= m_Addr.m_sISOCode;
		m_dsFax			= m_Addr.m_sFax;

		m_deDeliveryType	= m_Addr.m_deDeliveryType;
		m_dePrintType		= m_Addr.m_dePrintType;

		m_dsSubject		= m_Email.m_sSubject;

		m_Email.m_PostaLiteMsg.SetSize(m_Email.m_PostaLiteMsg.GetCount() - 1);
	}
	else
	{
		m_deDeliveryType	= AfxGetIMailConnector()->GetPostaLiteDefaultDeliveryType();
		m_dePrintType		= AfxGetIMailConnector()->GetPostaLiteDefaultPrintType();
	}

	m_edtAddressee.SubclassEdit(IDC_PL_ADDRESSEE, this, _T("Addressee"));
	m_edtAddressee.Attach(&m_dsAddressee);

	m_edtAddress.SubclassEdit(IDC_PL_ADDRESS, this, _T("Address"));
	m_edtAddress.Attach(&m_dsAddress);

	m_cbxCity.SubclassEdit(IDC_PL_CITY, this, _T("City"));
	m_cbxCity.SetNameSpace(_NS_DF("DataFile.ERP.Company.City"));
	m_cbxCity.Attach(&m_dsCity);
	m_cbxCity.Attach(&m_dsCounty,	_NS_DFEL("County"));
	m_cbxCity.Attach(&m_dsZipCode,	_NS_DFEL("ZIPCode"));

	m_edtZipCode.SubclassEdit(IDC_PL_ZIPCODE, this, _T("ZipCode"));
	m_edtZipCode.Attach(&m_dsZipCode);

	m_cbxCounty.SubclassEdit(IDC_PL_COUNTY, this, _T("County"));
	m_cbxCounty.SetNameSpace(_NS_DF("DataFile.ERP.Company.County"));
	m_cbxCounty.Attach(&m_dsCounty);

	m_cbxCountry.SubclassEdit(IDC_PL_COUNTRY, this, _T("Country"));
	m_cbxCountry.UseProductLanguage(TRUE);
	m_cbxCountry.SetNameSpace(_NS_DF("DataFile.Extensions.TbMailer.State"));
	m_cbxCountry.Attach(&m_dsCountry);

	m_edtSubject.SubclassDlgItem (IDC_SUBJECT1,	this);
	m_edtSubject.Attach (&m_dsSubject);

	m_cbxDeliveryType.SubclassEdit(IDC_PL_DELIVERYTYPE, this, _T("DeliveryType"));
	m_cbxDeliveryType.Attach(&m_deDeliveryType);
	m_cbxDeliveryType.SetCurSel(0);

	m_cbxPrintType.SubclassEdit(IDC_PL_PRINTTYPE, this, _T("PrintType"));
	m_cbxPrintType.Attach(&m_dePrintType);
	m_cbxPrintType.SetCurSel(0);

	GetDlgItem(IDC_PL_ZUCCHETTI)->SetFont(&m_fontZucchetti);

	UpdateCtrl();

	return TRUE;
}

//-----------------------------------------------------------------------------
void CSendPostaLiteDlg::OnOK ()
{
	UpdateCtrl();

	m_dsFax.Trim();
	m_dsAddressee.Trim();
	m_dsAddress.Trim();
	m_dsCity.Trim();
	m_dsCounty.Trim();
	m_dsCountry.Trim();
	m_dsZipCode.Trim();

	if (!m_dsFax.IsEmpty() && m_dsFax[0] == '+')
		m_dsFax = _T("00") + m_dsFax.GetString().Mid(1);

	if (this->m_deDeliveryType == E_POSTALITE_DELIVERY_TYPE_FAX)
	{
		if (m_dsFax.IsEmpty())
		{
			AfxMessageBox(_TB("The Fax number is empty"));
			return;
		}
		if (m_dsCountry.GetString().CompareNoCase(_T("Italia")) &&  m_dsFax.GetString().Left(2) != _T("00"))
		{
			AfxMessageBox(_TB("The Fax number required internation prefix number"));
			return;
		}
	}
	else if (m_dsAddressee.IsEmpty() || m_dsAddress.IsEmpty() || m_dsCity.IsEmpty() || m_dsCounty.IsEmpty() || m_dsZipCode.IsEmpty())
	{
		AfxMessageBox(_TB("The address is not complete"));
		return;
	}

	//----
	if (m_Email.m_PostaLiteMsg.m_nFilePages)
	{
		CString sErrMaxPages(_TB("The Document has too many pages"));

		if (
			m_Email.m_PostaLiteMsg.m_nFilePages > 49 && 
			(
				m_Addr.m_dePrintType == E_POSTALITE_PRINT_TYPE_FRONT_BW 
				||
				m_Addr.m_dePrintType == E_POSTALITE_PRINT_TYPE_FRONT_COLOR 
			)
		)
		{
			if (m_Email.m_PostaLiteMsg.m_nFilePages < 99)
			{
				if (
					AfxMessageBox(
						sErrMaxPages + '\n' +
						_TB("Do You want change print type to Front-Back ?"),
						MB_YESNO) == IDNO
					)
					return;
				if (m_Addr.m_dePrintType == E_POSTALITE_PRINT_TYPE_FRONT_BW)
					m_Addr.m_dePrintType = E_POSTALITE_PRINT_TYPE_FRONTBACK_BW;
				else if (m_Addr.m_dePrintType == E_POSTALITE_PRINT_TYPE_FRONT_COLOR)
					m_Addr.m_dePrintType = E_POSTALITE_PRINT_TYPE_FRONTBACK_COLOR;
			}
			else
				AfxMessageBox(sErrMaxPages);
			return;
		}
		if (
				m_Email.m_PostaLiteMsg.m_nFilePages > 98 && 
				(
					m_Addr.m_dePrintType == E_POSTALITE_PRINT_TYPE_FRONTBACK_BW 
					||
					m_Addr.m_dePrintType == E_POSTALITE_PRINT_TYPE_FRONTBACK_COLOR 
				)
			)
		{
			AfxMessageBox(sErrMaxPages);
			return;
		}
	}
	//----------------------------------------------
	//if (
	//		m_dsAddressee != m_Addr.m_sAddressee ||
	//		m_dsAddress != m_Addr.m_sAddress ||
	//		m_dsCity != m_Addr.m_sCity ||
	//		m_dsCounty != m_Addr.m_sCounty ||
	//		m_dsCountry != m_Addr.m_sCountry ||
	//		m_dsZipCode != m_Addr.m_sZipCode ||
	//		m_dsFax != m_Addr.m_sFax
	//	)
	//{
	//	DataGuid g; g.AssignNewGuid();
	//  if (m_Email.m_PostaLiteMsg.m_sAddresseePrimaryKey.Right(1) != ';') m_Email.m_PostaLiteMsg.m_sAddresseePrimaryKey += ';';
	//	m_Email.m_PostaLiteMsg.m_sAddresseePrimaryKey += _T("custom=") + g.Str() +';';
	//}

	m_Email.m_sSubject = m_dsSubject;

	m_Email.AddPostaLiteMsg
				(
					m_dsFax,
					m_dsAddressee,
					m_dsAddress,
					m_dsZipCode,
					m_dsCity,
					m_dsCounty,
					m_dsCountry,
					m_dsISOCode,
					m_deDeliveryType,
					m_dePrintType
				);
	
	m_Email.m_PostaLiteMsg.m_bUsePostaLite = TRUE;

	if (m_pCallerDoc)
	{
		if (m_Email.m_PostaLiteMsg.m_sDocNamespace.IsEmpty())
		{
			m_Email.m_PostaLiteMsg.m_sDocNamespace = m_pCallerDoc->GetNamespace().ToString();

			if (m_pCallerDoc->IsKindOf(RUNTIME_CLASS(CAbstractFormDoc)))
			{
				CAbstractFormDoc* pDoc = (CAbstractFormDoc*) m_pCallerDoc;
				if (pDoc->GetMaster() && pDoc->GetMaster()->GetRecord()) 
					m_Email.m_PostaLiteMsg.m_sDocPrimaryKey = pDoc->GetMaster()->GetRecord()->GetPrimaryKeyNameValue();
			}
		}
	}
	else
	{
		//if (m_Email.m_PostaLiteMsg.m_sDocNamespace.IsEmpty())
		//TODO  m_Email.m_PostaLiteMsg.m_sDocNamespace = pWoormDoc->GetNamespace().ToString();
	}

	__super::OnOK();
}

//-----------------------------------------------------------------------------
void CSendPostaLiteDlg::OnAfterRecordSelected()
{
	HotKeyLink* pHkl = (HotKeyLink*) m_edtAddressee.GetHotLink();
	if (pHkl == NULL) 
		return;
	ASSERT_VALID(pHkl);
	
	SqlRecord* pRec = pHkl->GetAttachedRecord ();
	if (pRec == NULL)
		return;
	ASSERT_VALID(pRec);

	DataObj* pObj =	pRec->GetDataObjFromColumnName(_T("Address"));
	ASSERT_VALID(pObj);
	if (pObj && pObj->IsKindOf(RUNTIME_CLASS(DataStr)))
	{
		//pDlg->m_dsAddress = pRec[_T("Address")];
		m_dsAddress = *pObj;
	}

	pObj =	pRec->GetDataObjFromColumnName(_T("City"));
	ASSERT_VALID(pObj);
	if (pObj && pObj->IsKindOf(RUNTIME_CLASS(DataStr)))
	{
		m_dsCity = *pObj;
	}
	pObj =	pRec->GetDataObjFromColumnName(_T("County"));
	ASSERT_VALID(pObj);
	if (pObj && pObj->IsKindOf(RUNTIME_CLASS(DataStr)))
	{
		m_dsCounty = *pObj;
	}
	pObj =	pRec->GetDataObjFromColumnName(_T("Country"));
	ASSERT_VALID(pObj);
	if (pObj && pObj->IsKindOf(RUNTIME_CLASS(DataStr)))
	{
		m_dsCountry = *pObj;
	}
	pObj =	pRec->GetDataObjFromColumnName(_T("Fax"));
	ASSERT_VALID(pObj);
	if (pObj && pObj->IsKindOf(RUNTIME_CLASS(DataStr)))
	{
		m_dsFax = *pObj;
	}
	pObj =	pRec->GetDataObjFromColumnName(_T("ZipCode"));
	ASSERT_VALID(pObj);
	if (pObj && pObj->IsKindOf(RUNTIME_CLASS(DataStr)))
	{
		m_dsZipCode = *pObj;
	}
	pObj =	pRec->GetDataObjFromColumnName(_T("ISOCountryCode"));
	ASSERT_VALID(pObj);
	if (pObj && pObj->IsKindOf(RUNTIME_CLASS(DataStr)))
	{
		m_dsISOCode = *pObj;

		if (!m_dsISOCode.IsEmpty())
		{
			m_dsCountry = CPostaLiteNet::GetPostaliteCountryFromIsoCode(m_dsISOCode);
		}
	}

	m_Email.m_PostaLiteMsg.m_sAddresseeNamespace = m_edtAddressee.GetRefDocNamespace(pHkl);
	m_Email.m_PostaLiteMsg.m_sAddresseePrimaryKey = pRec->GetPrimaryKeyNameValue();

	UpdateCtrl();
}

//-----------------------------------------------------------------------------
void CSendPostaLiteDlg::UpdateCtrl()
{
	m_edtAddressee.UpdateCtrlStatus();
	m_edtAddressee.UpdateCtrlView();

	m_edtAddress.UpdateCtrlStatus();
	m_edtAddress.UpdateCtrlView();

	m_cbxCity.UpdateCtrlStatus();
	m_cbxCity.UpdateCtrlView();

	m_edtZipCode.UpdateCtrlStatus();
	m_edtZipCode.UpdateCtrlView();

	m_cbxCounty.UpdateCtrlStatus();
	m_cbxCounty.UpdateCtrlView();

	m_cbxCountry.UpdateCtrlStatus();
	m_cbxCountry.UpdateCtrlView();

	m_edtSubject.UpdateCtrlStatus();
	m_edtSubject.UpdateCtrlView();

	m_cbxDeliveryType.UpdateCtrlStatus();
	m_cbxDeliveryType.UpdateCtrlView();

	m_cbxPrintType.UpdateCtrlStatus();
	m_cbxPrintType.UpdateCtrlView();
}

//-----------------------------------------------------------------------------
void CSendPostaLiteDlg::OnCityChanged()
{
	m_cbxCounty.UpdateCtrlStatus();
	m_cbxCounty.UpdateCtrlView();

	m_edtZipCode.UpdateCtrlStatus();
	m_edtZipCode.UpdateCtrlView();
}

//-----------------------------------------------------------------------------
void CSendPostaLiteDlg::OnDeliveryTypeChanged ()
{
	if (this->m_deDeliveryType == E_POSTALITE_DELIVERY_TYPE_FAX)
	{
		//m_cbxPrintType.SetSetCurSel(0);
		this->m_dePrintType = E_POSTALITE_PRINT_TYPE_FRONT_BW;

		m_cbxPrintType.UpdateCtrlStatus();
		m_cbxPrintType.UpdateCtrlView();
		
		m_cbxPrintType.EnableWindow(FALSE);
	}
	else
		m_cbxPrintType.EnableWindow(TRUE);
}

//-----------------------------------------------------------------------------
LRESULT CSendPostaLiteDlg::OnGetWebCommandType(WPARAM wParam, LPARAM lParam)
{
	WebCommandType* type = (WebCommandType*) lParam;
	*type = WEB_UNDEFINED;
	if (wParam == IDC_FINDFILE)
		*type = WEB_UNSUPPORTED;
	return 1L;
}

