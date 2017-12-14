#include "stdafx.h"

#include <TbFrameworkImages\CommonImages.h>
#include <TbFrameworkImages\GeneralFunctions.h>

#include <TbClientCore\ClientObjects.h>
#include <TbWebServicesWrappers\LoginManagerInterface.h>

#include <TBGenlib\Baseapp.h>
#include <TBGenlib\ParsCtrl.h>
#include <TBGenlib\SettingsTableManager.h>

//Local declarations
#include "CAddressDlg.h"

//includere come ultimo include all'inizio del cpp
#include "begincpp.dex"

#ifdef _DEBUG
#undef THIS_FILE
static const char BASED_CODE THIS_FILE[] = __FILE__;
#endif

/////////////////////////////////////////////////////////////////////////////
// 				class CAddressDlg Implementation
/////////////////////////////////////////////////////////////////////////////
//
IMPLEMENT_DYNCREATE(CAddressDlg, CParsedDialogWithTiles)

BEGIN_MESSAGE_MAP(CAddressDlg, CParsedDialogWithTiles)
	//{{AFX_MSG_MAP( CParsedDialogWithTiles )
	ON_BN_CLICKED(ID_ADDRESS_SHOW_MAP, OnShowAddress)
	//}}AFX_MSG_MAP
END_MESSAGE_MAP()

//----------------------------------------------------------------------------
CAddressDlg::CAddressDlg() :
	CParsedDialogWithTiles	(IDD_ADDRESS, NULL, _NS_DLG("Framework.TbGenlib.TbGenlib.Address")),
	m_pGeocoder				(NULL),
	m_AddressControl		(NULL, &m_Address),
	m_StreetNumberControl	(NULL, &m_StreetNumber),
	m_CityControl			(NULL, &m_City),
	m_CountyControl			(NULL, &m_County),
	m_RegionControl			(NULL, &m_Region),
	m_FederalStateControl	(NULL, &m_FederalState),
	m_CountryControl		(NULL, &m_Country),
	m_ZipCodeControl		(NULL, &m_ZipCode),
	m_LatitudeControl		(NULL, &m_Latitude),
	m_LongitudeControl		(NULL, &m_Longitude)
{
}

//----------------------------------------------------------------------------
void CAddressDlg::SetAddress(
	CGeocoder* pGeocoder,
	CString	aAddress,
	CString	aStreetNumber,
	CString	aCity,
	CString	aCounty,
	CString	aRegion,
	CString	aFederalState,
	CString	aCountry,
	CString	aZipCode,
	CString	aLatitude,
	CString	aLongitude,
	CString	aAddressType)
{
	m_pGeocoder = pGeocoder;
	CParsedDialogWithTiles(IDD_ADDRESS, NULL, _NS_DLG("Framework.TbGenlib.TbGenlib.Address"));
	
	m_Address = aAddress;
	m_StreetNumber = aStreetNumber;
	m_City = aCity;
	m_County = aCounty;
	m_Region = aRegion;
	m_FederalState = aFederalState;
	m_Country = aCountry;
	m_ZipCode = aZipCode;
	m_Latitude = aLatitude;
	m_Longitude = aLongitude;
	m_AddressType = aAddressType;
}

//----------------------------------------------------------------------------
void CAddressDlg::UpdateAllLocalCtrl()
{
	m_AddressControl.UpdateCtrlView();
	m_StreetNumberControl.UpdateCtrlView();
	m_CityControl.UpdateCtrlView();
	m_CountyControl.UpdateCtrlView();
	m_RegionControl.UpdateCtrlView();
	m_FederalStateControl.UpdateCtrlView();
	m_CountryControl.UpdateCtrlView();
	m_ZipCodeControl.UpdateCtrlView();
	m_LatitudeControl.UpdateCtrlView();
	m_LongitudeControl.UpdateCtrlView();
}

//----------------------------------------------------------------------------
BOOL CAddressDlg::OnInitDialog()
{
	CParsedDialogWithTiles::OnInitDialog();

	AddTileGroup(IDC_TG_ADDRESS, RUNTIME_CLASS(CAddressTileGrp), _NS_TILEGRP("AddressGrp"));
	SetToolbarStyle(CParsedDialogWithTiles::BOTTOM, 32, TRUE, TRUE);
	CenterWindow();
	UpdateAllLocalCtrl();
	return TRUE;
}

//----------------------------------------------------------------------------
void CAddressDlg::OnCustomizeToolbar()
{	m_pToolBar->AddButtonToRight(IDOK, _NS_TOOLBARBTN("Ok"), TBIcon(szIconOk, TOOLBAR), _TB("Ok"));
	m_pToolBar->AddButtonToRight(IDCANCEL, _NS_TOOLBARBTN("Cancel"), TBIcon(szIconEscape, TOOLBAR), _TB("Cancel"));
	m_pToolBar->AddButton(ID_ADDRESS_SHOW_MAP, _NS_TOOLBARBTN("ShowAddress"), TBIcon(szIconSearch, TOOLBAR), _TB("Show"));
}

//----------------------------------------------------------------------------
void CAddressDlg::OnOK()
{
	EndDialog(IDOK);
}

//----------------------------------------------------------------------------
void CAddressDlg::OnCancel()
{
	EndDialog(IDCANCEL);
}

//----------------------------------------------------------------------------
void CAddressDlg::OnShowAddress()
{
	if (!m_pGeocoder)
		return;

	if (m_Latitude != _T("") && m_Longitude != _T(""))
		m_pGeocoder->OpenGoogleMaps(m_Latitude, m_Longitude);
	else if (m_Address != _T(""))
		m_pGeocoder->OpenGoogleMaps(m_Address, m_StreetNumber, m_City, m_County, m_Country, m_FederalState, m_ZipCode);
}

//----------------------------------------------------------------------------
CString CAddressDlg::GetGoogleWebLink()
{
	if (!m_pGeocoder)
		return _T("");

	if (m_Latitude != _T("") && m_Longitude != _T(""))
		return m_pGeocoder->GetGoogleWebLink(m_Latitude, m_Longitude);
	else if (m_Address != _T(""))
		return m_pGeocoder->GetGoogleWebLink(m_Address, m_StreetNumber, m_City, m_County, m_Country, m_FederalState, m_ZipCode);

	return _T("");
}

//----------------------------------------------------------------------------
int  CAddressDlg::ShowDialog()
{
	return DoModal();
}

//====================================================================
//			 CAddressTileGrp 
//====================================================================
//
IMPLEMENT_DYNCREATE(CAddressTileGrp, CTileGroup)

//-----------------------------------------------------------------------------
void CAddressTileGrp::Customize()
{
	SetLayoutType(CLayoutContainer::VBOX);

	AddTile(RUNTIME_CLASS(CAddressTileDlg), IDD_TD_ADDRESS, _TB("Address"), TILE_STANDARD);
}

//=============================================================================
IMPLEMENT_DYNCREATE(CAddressTileDlg, CTileDialog)

//-----------------------------------------------------------------------------
CAddressTileDlg::CAddressTileDlg()
	:
	CTileDialog(_NS_TILEDLG("PLComponentSearch"), IDD_TD_ADDRESS)
{
	SetCollapsible(FALSE);
}

//-----------------------------------------------------------------------------
CAddressDlg* CAddressTileDlg::GetParentParsedDlg()
{
	CWnd* pWnd = GetParent();
	while (pWnd != NULL && pWnd->GetRuntimeClass() != RUNTIME_CLASS(CAddressDlg))
	{
		pWnd = pWnd->GetParent();
	}

	if (pWnd && pWnd->IsKindOf(RUNTIME_CLASS(CAddressDlg)))
		return (CAddressDlg*)pWnd;

	else return NULL;
}

//-----------------------------------------------------------------------------
void CAddressTileDlg::BuildDataControlLinks()
{
	CAddressDlg* pParentDlg = GetParentParsedDlg();

	pParentDlg->m_AddressControl.		SubclassEdit(IDC_ADDRESS_ADDRESS,		this, _NS_CTRL("Address"));
	pParentDlg->m_StreetNumberControl.	SubclassEdit(IDC_ADDRESS_STREET_NUMBER,	this, _NS_CTRL("StreetNumber"));
	pParentDlg->m_CityControl.			SubclassEdit(IDC_ADDRESS_CITY,			this, _NS_CTRL("City"));
	pParentDlg->m_CountyControl.		SubclassEdit(IDC_ADDRESS_COUNTY,		this, _NS_CTRL("County"));
	pParentDlg->m_RegionControl.		SubclassEdit(IDC_ADDRESS_REGION,		this, _NS_CTRL("Region"));
	pParentDlg->m_FederalStateControl.	SubclassEdit(IDC_ADDRESS_REGION_CODE,	this, _NS_CTRL("FederalState"));
	pParentDlg->m_CountryControl.		SubclassEdit(IDC_ADDRESS_COUNTRY,		this, _NS_CTRL("Country"));
	pParentDlg->m_ZipCodeControl.		SubclassEdit(IDC_ADDRESS_ZIP,			this, _NS_CTRL("ZipCode"));
	pParentDlg->m_LatitudeControl.		SubclassEdit(IDC_ADDRESS_LATITUDE,		this, _NS_CTRL("Latitude"));
	pParentDlg->m_LongitudeControl.		SubclassEdit(IDC_ADDRESS_LONGITUDE,		this, _NS_CTRL("Longitude"));

	if (pParentDlg->m_pGeocoder->IsBrazil()) 
	{
		CWnd* pWnd = NULL;

		pParentDlg->m_CountyControl.ShowWindow(SW_HIDE);
		pParentDlg->m_RegionControl.ShowWindow (SW_HIDE);

		pWnd = GetDlgItem(IDC_ADDRESS_REGION_STATIC);
		if (pWnd) pWnd->SetWindowTextW(_TB("Federal state"));

		pWnd = GetDlgItem(IDC_ADDRESS_COUNTY_STATIC);
		if (pWnd) pWnd->ShowWindow(SW_HIDE);
	}
	else
		pParentDlg->m_FederalStateControl.ShowWindow(SW_HIDE);
}

///////////////////////////////////////////////////////////////////////////////
//					CAddressListBox - implementation
///////////////////////////////////////////////////////////////////////////////
IMPLEMENT_DYNCREATE(CAddressListBox, CStrListBox)

//-----------------------------------------------------------------------------
CAddressListBox::CAddressListBox()
:
CStrListBox(),
m_pDlg(NULL)
{}

//-----------------------------------------------------------------------------
CAddressListBox::CAddressListBox(UINT nBtnIDBmp, DataStr* pData, CSelectAddressDlg* pDlg)
	:
	CStrListBox(nBtnIDBmp, pData),
	m_pDlg(pDlg)
{}

//-----------------------------------------------------------------------------
BOOL CAddressListBox::OnInitCtrl()
{
	BOOL bOK = CStrListBox::OnInitCtrl();

	FillListBox();
	return bOK;
}

//-----------------------------------------------------------------------------
void CAddressListBox::OnFillListBox()
{
	CStrListBox::OnFillListBox();

	if (m_pDlg) for (int i = 0; i <= m_pDlg->GetAddresses().GetUpperBound(); i++)
	{
		AddAssociation(m_pDlg->GetAddresses().GetAt(i), m_pDlg->GetAddresses().GetAt(i));
	}
}

/////////////////////////////////////////////////////////////////////////////
// 				class CSelectAddressDlg Implementation
/////////////////////////////////////////////////////////////////////////////
//
IMPLEMENT_DYNCREATE(CSelectAddressDlg, CParsedDialogWithTiles)
BEGIN_MESSAGE_MAP(CSelectAddressDlg, CParsedDialogWithTiles)
	//{{AFX_MSG_MAP( CSelectAddressDlg )
	ON_LBN_DBLCLK(IDC_ADDRESS_LIST, OnLButtonDblClk)
	ON_LBN_SELCHANGE(IDC_ADDRESS_LIST, OnLButtonClk)
	ON_BN_CLICKED(ID_ADDRESS_SELECTION_SHOW_MAP, OnShowAddress)
	//}}AFX_MSG_MAP
END_MESSAGE_MAP()

//----------------------------------------------------------------------------
CSelectAddressDlg::CSelectAddressDlg() :
	CParsedDialogWithTiles(IDD_ADDRESS_SELECTION, NULL, _NS_DLG("Framework.TbGenlib.TbGenlib.SelectAddress")),
	m_pGeocoder(NULL),
	m_AddressControl(NULL, &m_Address, this)
{
}

//----------------------------------------------------------------------------
void CSelectAddressDlg::SetSelectAddressDlg(CGeocoder*	pGeocoder)
{
	m_pGeocoder = pGeocoder;
}

//----------------------------------------------------------------------------
int CSelectAddressDlg::ShowDialog()
{
	return DoModal();
}

//----------------------------------------------------------------------------
void CSelectAddressDlg::DisableShowButton()
{
	//an. 23633: Il bottone viene disabilitato se non ci sono indirizzi selezionati
	CWnd* pWnd = GetDlgItem(ID_ADDRESS_SELECTION_SHOW_MAP);
	if (pWnd)	pWnd->EnableWindow(!m_Address.IsEmpty());
}

//----------------------------------------------------------------------------
void CSelectAddressDlg::UpdateAllLocalCtrl()
{
	m_AddressControl.UpdateCtrlView();
}

//----------------------------------------------------------------------------
BOOL CSelectAddressDlg::OnInitDialog()
{
	CParsedDialog::OnInitDialog();

	m_AddressControl.SubclassEdit(IDC_ADDRESS_LIST, this, _NS_CTRL("AddressSelection"));

	DisableShowButton();

	CenterWindow();
	UpdateAllLocalCtrl();
	return TRUE;
}

//-----------------------------------------------------------------------------	
void CSelectAddressDlg::OnLButtonDblClk()
{
	EndDialog(IDOK);
}

//-----------------------------------------------------------------------------	
void CSelectAddressDlg::OnLButtonClk()
{
	DisableShowButton();
}

//----------------------------------------------------------------------------
void CSelectAddressDlg::OnOK()
{
	EndDialog(IDOK);
}

//----------------------------------------------------------------------------
void CSelectAddressDlg::OnCancel()
{
	EndDialog(IDCANCEL);
}

//----------------------------------------------------------------------------
void CSelectAddressDlg::AddAddress(CString aAddress, CString aLatitude, CString aLongitude)
{
	m_AddressArray.Add(aAddress);
	m_LatitudeArray.Add(aLatitude);
	m_LongitudeArray.Add(aLongitude);
}

//----------------------------------------------------------------------------
void CSelectAddressDlg::OnShowAddress()
{
	if (!m_pGeocoder || m_Address.IsEmpty())
		return;

	int i = 0;

	for (i = 0; i <= m_AddressArray.GetUpperBound(); i++)
	{
		if (m_AddressArray.GetAt(i) == m_Address)
			break;
	}

	if (m_LatitudeArray.GetAt(i) != _T("") && m_LongitudeArray.GetAt(i) != _T(""))
		m_pGeocoder->OpenGoogleMaps(m_LatitudeArray.GetAt(i), m_LongitudeArray.GetAt(i));
}

//----------------------------------------------------------------------------
CString CSelectAddressDlg::GetGoogleWebLink()
{
	if (!m_pGeocoder)
		return _T("");

	int i = 0;

	for (i = 0; i <= m_AddressArray.GetUpperBound(); i++)
	{
		if (m_AddressArray.GetAt(i) == m_Address)
			break;
	}

	if (m_LatitudeArray.GetAt(i) != _T("") && m_LongitudeArray.GetAt(i) != _T(""))
		return m_pGeocoder->GetGoogleWebLink(m_LatitudeArray.GetAt(i), m_LongitudeArray.GetAt(i));

	return _T("");
}