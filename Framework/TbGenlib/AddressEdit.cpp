#include "stdafx.h"  

#include <TbFrameworkImages\CommonImages.h>
#include <TbFrameworkImages\GeneralFunctions.h>

#include <TbGeneric\globals.h>
#include <TbGeneric\dibitmap.h>
#include <TbGeneric\FontsTable.h>
#include <TbGeneric\GeneralFunctions.h>
#include <TbGeneric\DataObj.h>
#include <TbGeneric\DataTypesFormatters.h>
#include <TbGeneric\tools.h>
#include <TbGeneric\pictures.h>
#include <TbGeneric\linefile.h>
#include <TbGeneric\VisualStylesXP.h>
#include <TbGeneric\TBThemeManager.h>

#include <TbStringLoader\Generic.h>

#include <TbParser\SymTable.h>

#include "baseapp.h"
#include "Generic.h"
#include "parsobj.h"
#include "hlinkobj.h"
#include "parsedt.h"
#include "parslbx.h"
#include "TbExplorerInterface.h"

//Local declarations
#include "AddressEdit.h"
#include "AddressEdit.hjson" //JSON AUTOMATIC UPDATE

#include "begincpp.dex"

#ifdef _DEBUG
#undef THIS_FILE
static char THIS_FILE[] = __FILE__;
#endif

#define ASSIGN_VALUE(Tag, Parent, Value) \
	if (Parent) \
	{ \
		CXMLNode* pChildXMLNode	= Parent->SelectSingleNode(XML_GEOCODER_##Tag##); \
		if (pChildXMLNode) \
		{ \
			CString	sTemp = _T(""); \
			if (pChildXMLNode->GetText(sTemp)) \
			{ \
				Value.AssignFromXMLString(sTemp); \
				SAFE_DELETE(pChildXMLNode) \
			} \
			else \
			{ \
				m_Status = _TB("Error retrieving data"); \
				SAFE_DELETE(pChildXMLNode) \
				return FALSE; \
			} \
		} \
		else \
		{ \
			m_Status = _TB("Error retrieving data"); \
			SAFE_DELETE(pChildXMLNode) \
			return FALSE; \
		} \
	} \

#define GET_NODE_VALUE(Node, Value) \
	if (Node) \
	{ \
		CString	sTemp; \
		if (Node->GetText(sTemp)) \
		{ \
			Value.AssignFromXMLString(sTemp); \
		} \
		else \
		{ \
			m_Status = _TB("Error retrieving data"); \
			return FALSE; \
		} \
	} \
	else \
	{ \
		m_Status = _TB("Error retrieving data"); \
		return FALSE; \
	} \

#define SELECT_NODE(Child, Parent, Tag) \
	SAFE_DELETE(Child) \
	Child = Parent->SelectSingleNode(XML_GEOCODER_##Tag##); \
	if (!Child) \
	{ \
		m_Status = _TB("Error retrieving data"); \
		return FALSE; \
	} \

#define SELECT_NODES(Childs, Parent, Tag) \
	SAFE_DELETE(Childs) \
	Childs = Parent->SelectNodes(XML_GEOCODER_##Tag##); \
	if (!Childs || Childs->GetSize() == 0) \
	{ \
		m_Status = _TB("Error retrieving data"); \
		return FALSE; \
	} \

//=============================================================================
//			Class CBaseAddressEdit implementation
//=============================================================================
IMPLEMENT_DYNAMIC (CBaseAddressEdit, CLinkEdit)

//-----------------------------------------------------------------------------
CBaseAddressEdit::CBaseAddressEdit()
	:
	CLinkEdit		(),

	m_pCity			(NULL),
	m_pCounty		(NULL),
	m_pCountry		(NULL),
	m_pISOCode		(NULL),
	m_pZip			(NULL),
	m_pStreetNumber	(NULL),
	m_pRegion		(NULL),
	m_pLatitude		(NULL),
	m_pLongitude	(NULL),
	m_pDistrict		(NULL),
	m_pAddress		(NULL),
	m_pFederalState	(NULL),
	m_pAddressType	(NULL),

	m_nDataIdxCity			(-1),
	m_nDataIdxCounty		(-1),
	m_nDataIdxCountry		(-1),
	m_nDataIdxISOCode		(-1),
	m_nDataIdxZip			(-1),
	m_nDataIdxStreetNumber	(-1),
	m_nDataIdxRegion		(-1),
	m_nDataIdxLatitude		(-1),
	m_nDataIdxLongitude		(-1),
	m_nDataIdxAddress		(-1),
	m_nDataIdxDistrict		(-1),
	m_nDataIdxFederalState	(-1),
	m_nDataIdxAddressType	(-1)
{
}

//-----------------------------------------------------------------------------
CBaseAddressEdit::CBaseAddressEdit(UINT nBtnIDBmp, DataStr* pData /* = NULL */)
	:
	CLinkEdit		(nBtnIDBmp, pData),

	m_pCity			(NULL),
	m_pCounty		(NULL),
	m_pCountry		(NULL),
	m_pISOCode		(NULL),
	m_pZip			(NULL),
	m_pStreetNumber	(NULL),
	m_pRegion		(NULL),
	m_pLatitude		(NULL),
	m_pLongitude	(NULL),
	m_pDistrict		(NULL),
	m_pAddress		(NULL),
	m_pFederalState	(NULL),
	m_pAddressType	(NULL),

	m_nDataIdxCity			(-1),
	m_nDataIdxCounty		(-1),
	m_nDataIdxCountry		(-1),
	m_nDataIdxISOCode		(-1),
	m_nDataIdxZip			(-1),
	m_nDataIdxStreetNumber	(-1),
	m_nDataIdxRegion		(-1),
	m_nDataIdxLatitude		(-1),
	m_nDataIdxLongitude		(-1),
	m_nDataIdxAddress		(-1),
	m_nDataIdxDistrict		(-1),
	m_nDataIdxFederalState	(-1),
	m_nDataIdxAddressType	(-1)
{
}

//-----------------------------------------------------------------------------
CBaseAddressEdit::~CBaseAddressEdit()
{
}

//-----------------------------------------------------------------------------
void CBaseAddressEdit::Attach(DataObj* pDataObj)
{
	__super::Attach (pDataObj);	
	if (GetCtrlData())
		GetCtrlData()->SetCollateCultureSensitive (FALSE);
}

//-----------------------------------------------------------------------------
void CBaseAddressEdit::Bind(DataStr* pField, FieldType eFieldType, int nDataIdx /*= -1*/) 
{
	switch (eFieldType)
	{
		case STREET_NUMBER:
			BindStreetNumber(pField);
			break;

		case ADDRESS_TYPE:
			BindAddressType(pField);
			break;

		case CITY:
			BindCity(pField, nDataIdx);
			break;

		case COUNTY:
			BindCounty(pField, nDataIdx);
			break;

		case COUNTRY:
			BindCountry(pField, nDataIdx);
			break;

		case ISO_CODE:
			BindISOCode(pField, nDataIdx);
			break;

		case ZIP:
			BindZip(pField, nDataIdx);
			break;

		case REGION:
			BindRegion(pField, nDataIdx);
			break;

		case LATITUDE:
			BindLatitude(pField, nDataIdx);
			break;

		case LONGITUDE:
			BindLongitude(pField, nDataIdx);
			break;

		case ADDRESS:
			BindAddress(pField, nDataIdx);
			break;

		case DISTRICT:
			BindDistrict(pField, nDataIdx);
			break;

		case FEDERAL_STATE:
			BindFederalState(pField, nDataIdx);
			break;
	}
}

//=============================================================================
//			Class CAddressEdit implementation
//=============================================================================
IMPLEMENT_DYNCREATE (CAddressEdit, CBaseAddressEdit)

//-----------------------------------------------------------------------------
BEGIN_MESSAGE_MAP(CAddressEdit, CBaseAddressEdit)

	ON_COMMAND			(ID_GOOGLE_SEARCH_ADDRESS,	OnSearchAddress)
	ON_COMMAND			(ID_GOOGLE_SHOW_MAP,		OnShowMap)
	ON_COMMAND			(ID_GOOGLE_SHOW_SATELLITE,	OnShowSatellite)


END_MESSAGE_MAP()
            
//-----------------------------------------------------------------------------
CAddressEdit::CAddressEdit()
	:
	CBaseAddressEdit	(),
	m_pGeocoder			(NULL)
{
	m_pGeocoder = new CGeocoder();
}

//-----------------------------------------------------------------------------
CAddressEdit::CAddressEdit(UINT nBtnIDBmp, DataStr* pData /* = NULL */)
	:
	CBaseAddressEdit	(nBtnIDBmp, pData),
	m_pGeocoder			(NULL)
{
	m_pGeocoder = new CGeocoder();
}

//-----------------------------------------------------------------------------
CAddressEdit::~CAddressEdit()
{
	SAFE_DELETE(m_pGeocoder);
}

//-----------------------------------------------------------------------------
void CAddressEdit::SetAddressDlgClass(CRuntimeClass* pClass)
{
	if (m_pGeocoder)
	{
		m_pGeocoder->SetAddressDlgClass(pClass);
	}
}

//-----------------------------------------------------------------------------
CRuntimeClass* CAddressEdit::GetAddressDlgClass()
{
	if (m_pGeocoder)
	{
		return m_pGeocoder->GetAddressDlgClass();
	}
	return NULL;
}

//-----------------------------------------------------------------------------
void CAddressEdit::SetSelectAddressDlgClass(CRuntimeClass* pClass)
{
	if (m_pGeocoder)
	{
		m_pGeocoder->SetSelectAddressDlgClass(pClass);
	}
}

//-----------------------------------------------------------------------------
CRuntimeClass* CAddressEdit::GetSelectAddressDlgClass()
{
	if (m_pGeocoder)
	{
		return m_pGeocoder->GetSelectAddressDlgClass();
	}
	return NULL;
}


//-----------------------------------------------------------------------------
BOOL CAddressEdit::GetMenuButton (CMenu* pMenu)
{
	BOOL bEmpty = GetCtrlData() == NULL;
	
	if (bEmpty || !GetCtrlData()->IsReadOnly())
	{
		if (m_pGeocoder->IsBrazil())
			pMenu->AppendMenu(MF_STRING, ID_GOOGLE_SEARCH_ADDRESS,	_TB("Get geographic coordinate"));
		else
			pMenu->AppendMenu(MF_STRING, ID_GOOGLE_SEARCH_ADDRESS,	_TB("Search for address"));
		pMenu->AppendMenu(MF_STRING, ID_GOOGLE_SHOW_MAP,		_TB("Show map"));
		pMenu->AppendMenu(MF_STRING, ID_GOOGLE_SHOW_SATELLITE,	_TB("Show satellite view"));
	}
	return TRUE;
}

//-----------------------------------------------------------------------------
CString CAddressEdit::GetMenuButtonImageNS()
{
	return TBIcon(szIconAddress, CONTROL); 
}

//-----------------------------------------------------------------------------
void CAddressEdit::DoCmdMenuButton(UINT nUINT)
{
	if (GetDocument()) GetDocument()->GetNotValidView();

	if (nUINT == ID_GOOGLE_SEARCH_ADDRESS) 
		OnSearchAddress();	
	else if (nUINT == ID_GOOGLE_SHOW_MAP)
		OnShowMap();
	else if (nUINT == ID_GOOGLE_SHOW_SATELLITE)
		OnShowSatellite();	
}
//-----------------------------------------------------------------------------
void CAddressEdit::OnSearchAddress() 
{
	DataStr aCity			= _T("");
	DataStr aCounty			= _T("");
	DataStr aCountry		= _T("");
	DataStr aISOCode		= _T("");
	DataStr aZip			= _T("");
	DataStr aStreetNumber	= _T("");
	CString strAddress		= _T("");
	CString aFederalState	= _T("");
	CString aAddressType	= _T("");
   	CParsedCtrl::GetValue(strAddress);

	if (m_pCity)			aCity			= *m_pCity;
	if (m_pCounty)			aCounty			= *m_pCounty;
	if (m_pCountry)			aCountry		= *m_pCountry;
	if (m_pISOCode)			aISOCode		= *m_pISOCode;
	if (m_pZip)				aZip			= *m_pZip;
	if (m_pStreetNumber)	aStreetNumber	= *m_pStreetNumber;
	if (m_pFederalState)	aFederalState	= *m_pFederalState;
	if (m_pAddressType)		aAddressType	= *m_pAddressType;

	if (m_pGeocoder)
	{
		m_pGeocoder->SetUseAddressType(m_pAddressType != NULL);

		if (m_pAddressType) strAddress = aAddressType + ' ' + strAddress;
		
		m_pGeocoder->SetDocument(GetDocument());

		if (m_pGeocoder->SetGeocoder(DataStr(strAddress), aStreetNumber, aCity, aCounty, aCountry, aISOCode, aZip, aFederalState, TRUE))
		{
			if (m_pAddressType	&& !m_pGeocoder->GetGeocoderAddressType().IsEmpty())			{*m_pAddressType	= m_pGeocoder->GetGeocoderAddressType();		m_pAddressType->SetModified(); }
			if (m_pCity			&& !m_pGeocoder->GetGeocoderCity().IsEmpty())					{*m_pCity			= m_pGeocoder->GetGeocoderCity();				m_pCity->SetModified(); }
			if (m_pCounty		&& !m_pGeocoder->GetGeocoderCounty().IsEmpty())					{*m_pCounty			= m_pGeocoder->GetGeocoderCounty();				m_pCounty->SetModified(); }
			if (m_pCountry		&& !m_pGeocoder->GetGeocoderCountry().IsEmpty()			&& !m_pGeocoder->IsBrazil())	{*m_pCountry		= m_pGeocoder->GetGeocoderCountry();		m_pCountry->SetModified(); }
			if (m_pISOCode		&& !m_pGeocoder->GetGeocoderISOCode().IsEmpty())										{*m_pISOCode		= m_pGeocoder->GetGeocoderISOCode();		m_pISOCode->SetModified(); }
			if (m_pZip			&& !m_pGeocoder->GetGeocoderZipCode().IsEmpty())				{*m_pZip			= m_pGeocoder->GetGeocoderZipCode();			m_pZip->SetModified(); }
			if (m_pStreetNumber	&& !m_pGeocoder->GetGeocoderStreetNumber().IsEmpty()	&& m_pGeocoder->IsBrazil())		{*m_pStreetNumber	= m_pGeocoder->GetGeocoderStreetNumber();	m_pStreetNumber->SetModified(); }
			if (m_pRegion		&& !m_pGeocoder->GetGeocoderRegion().IsEmpty())					{*m_pRegion			= m_pGeocoder->GetGeocoderRegion();				m_pRegion->SetModified(); }
			if (m_pFederalState	&& !m_pGeocoder->GetGeocoderFederalState().IsEmpty()	&& m_pGeocoder->IsBrazil())		{*m_pFederalState	= m_pGeocoder->GetGeocoderFederalState();	m_pFederalState->SetModified(); }
			if (m_pLatitude		&& !m_pGeocoder->GetGeocoderLatitude().IsEmpty())				{*m_pLatitude		= m_pGeocoder->GetGeocoderLatitude();			m_pLatitude->SetModified(); }
			if (m_pLongitude	&& !m_pGeocoder->GetGeocoderLongitude().IsEmpty())				{*m_pLongitude		= m_pGeocoder->GetGeocoderLongitude();			m_pLongitude->SetModified(); }
			
			if (!m_pGeocoder->GetGeocoderAddressWithNumber().IsEmpty())
			{
				CParsedCtrl::SetValue(m_pGeocoder->GetGeocoderAddressWithNumber().GetString());
				CParsedCtrl::SetModifyFlag(TRUE);
				CParsedCtrl::UpdateCtrlData(TRUE);
			}
		}
		else if (!m_pGeocoder->IsGeocoderOK() && GetDocument() && !m_pGeocoder->GetGeocoderStatus().IsEmpty())
			GetDocument()->Message(m_pGeocoder->GetGeocoderStatus().GetString()); 

		if (GetDocument()) GetDocument()->UpdateDataView();
	}
}

//-----------------------------------------------------------------------------
void CAddressEdit::OnShowMap() 
{
	ShowMap();
}

//-----------------------------------------------------------------------------
void CAddressEdit::OnShowSatellite() 
{
	ShowMap(TRUE);
}

//-----------------------------------------------------------------------------
void CAddressEdit::ShowMap(BOOL bSatelliteView/* = FALSE*/) 
{	
	CString strAddress = GetGoogleWebLink(bSatelliteView);
	if (strAddress != _T(""))
		m_pGeocoder->OpenGoogleMaps(strAddress);
}


//----------------------------------------------------------------------------
CString CAddressEdit::GetGoogleWebLink(BOOL bSatelliteView/* = FALSE*/)
{
	CString strAddress = _T("");
   	CParsedCtrl::GetValue(strAddress);

	if (m_pGeocoder)
	{
		if (m_pLatitude && m_pLongitude && !(*m_pLatitude).IsEmpty() && !(*m_pLongitude).IsEmpty())
			return m_pGeocoder->GetGoogleWebLink(*m_pLatitude, *m_pLongitude, bSatelliteView);
		else 
		{
			DataStr aStreetNumber	= _T("");
			DataStr aCity			= _T("");
			DataStr aCounty			= _T("");
			DataStr aCountry		= _T("");
			DataStr	aFederalState	= _T("");
			DataStr aZip			= _T("");
			DataStr aAddressType	= _T("");

			if (m_pCity)			aCity			= *m_pCity;
			if (m_pStreetNumber)	aStreetNumber	= *m_pStreetNumber;
			if (m_pCounty)			aCounty			= *m_pCounty;
			if (m_pCountry)			aCountry		= *m_pCountry;
			if (m_pFederalState)	aFederalState	= *m_pFederalState;
			if (m_pZip)				aZip			= *m_pZip;
			if (m_pAddressType)		aAddressType	= *m_pAddressType;			
 			if (m_pAddressType)
				strAddress = aAddressType + _T(" ") + strAddress;

			return m_pGeocoder->GetGoogleWebLink(DataStr(strAddress), aStreetNumber, aCity, aCounty, aCountry, aFederalState, aZip, bSatelliteView);
		}
	}
	return _T("");
}

//-----------------------------------------------------------------------------
void CAddressEdit::PostClickMessage()
{
	PostMessage(WM_COMMAND, ID_GOOGLE_SHOW_MAP);
}

/////////////////////////////////////////////////////////////////////////////
//				class CGeocoder implementation
/////////////////////////////////////////////////////////////////////////////
//-----------------------------------------------------------------------------
IMPLEMENT_DYNAMIC(CGeocoder, CObject)

//----------------------------------------------------------------------------
CGeocoder::CGeocoder()
{
	m_bIsBrazil = AfxIsActivated(MAGONET_APP, _NS_ACT("MasterData_BR"));
	m_pAddressDlgClass = NULL;
	m_pDoc = NULL;
}

//----------------------------------------------------------------------------
BOOL CGeocoder::SetGeocoder(CString	aAddress, 
						 CString	aStreetNumber, 
						 CString	aCity, 
						 CString	aCounty, 
						 CString	aCountry, 
						 CString	aISOCode, 
						 CString	aZip, 
						 CString	aFederalState,
						 BOOL		bShowDialog/* = FALSE*/)
{
	BOOL bOK = FALSE;
	m_Status = GEOCODER_RESULT_OK;

	m_Address		= aAddress;
	m_StreetNumber	= aStreetNumber;
	m_City			= aCity;
	m_County		= aCounty;
	m_FederalState	= aFederalState;
	m_Country		= aCountry;
	m_ZipCode		= aZip;
	m_ISOCode		= aISOCode;

	CString aLanguage	= _T("&language=");

	if (AfxGetLoginManager()->GetProductLanguage() == _T("BR"))
		aLanguage		+= _T("pt-BR");
	else
		aLanguage		+= AfxGetLoginManager()->GetProductLanguage();

	CString aRegion		= _T("");
	if (!aCountry.IsEmpty())
		aRegion			= _T("&region=") + aCountry;

	if (IsBrazil()) aStreetNumber = _T("");

	if (m_Address.IsEmpty() && m_StreetNumber.IsEmpty() && m_City.IsEmpty() && m_County.IsEmpty() && m_Country.IsEmpty() && m_ISOCode.IsEmpty() && m_ZipCode.IsEmpty())
		aCountry = AfxGetLoginManager()->GetProductLanguage();

	CString aUrl = _T("http://maps.google.com/maps/api/geocode/xml?address=") + Address(aAddress, aStreetNumber, aCity, aCounty, aCountry, aISOCode, aZip) + _T("&sensor=false") + aLanguage + aRegion;

	CXMLDocumentObject* pXmlDoc = new CXMLDocumentObject();

	bOK = pXmlDoc && pXmlDoc->LoadXMLFromUrl(aUrl) && GetData(pXmlDoc, bShowDialog);

	SAFE_DELETE(pXmlDoc);
	return bOK;
}

//----------------------------------------------------------------------------
void CGeocoder::SetAddressDlgClass(CRuntimeClass* pClass)
{
	m_pAddressDlgClass = pClass;
}

//----------------------------------------------------------------------------
CRuntimeClass* CGeocoder::GetAddressDlgClass()
{
	return m_pAddressDlgClass;
}

//----------------------------------------------------------------------------
void CGeocoder::SetSelectAddressDlgClass(CRuntimeClass* pClass)
{
	m_pSelectAddressDlg = pClass;
}

//----------------------------------------------------------------------------
CRuntimeClass* CGeocoder::GetSelectAddressDlgClass()
{
	return m_pSelectAddressDlg;
}

//----------------------------------------------------------------------------
BOOL CGeocoder::GetData(CXMLDocumentObject* pXmlDoc, 
						BOOL				bShowDialog)
{
	CXMLNode* pRootNode = pXmlDoc->SelectSingleNode(XML_GEOCODER_GEOCODE_RESPONSE);
	if (!pRootNode)
	{
		m_Status = _TB("Error retrieving data");
		return FALSE;
	}

	ASSIGN_VALUE(STATUS, pRootNode, m_Status);
	
	if (!CheckStatus())
	{
		SAFE_DELETE(pRootNode);
		return FALSE;
	}

	BOOL				bOK = FALSE;
	CXMLNodeChildsList*	pChilds = NULL;

	SELECT_NODES(pChilds, pRootNode, RESULT);

	if (pChilds->GetSize() == 1)
	{
		bOK = SelectAddress(pChilds->GetAt(0), bShowDialog);
		SAFE_DELETE(pChilds);
		SAFE_DELETE(pRootNode);
		return bOK;
	}

	if (!bShowDialog)
		return TRUE;

	/*se sono br non devo aprire CSelectAddressDlg perché google non è riuscito a trovare 
	l'indirizzo esatto (quindi nemmeno la latit e longit) oppure l'utente non ha utilizzato 
	la funzione "search for zip code" (web service brasiliano), quindi i campi non sono stati 
	compilati correttamente*/
	if (m_bIsBrazil)
	{
		m_Status = _TB("Non-existent address or latitude and/or longitude are in a remote location");
		SAFE_DELETE(pChilds);
		SAFE_DELETE(pRootNode);
		return FALSE;
	}

	CObject* pObject = GetSelectAddressDlgClass()->CreateObject();
	CSelectAddressDlgObj* pSelectAddressDialog = dynamic_cast<CSelectAddressDlgObj *>(pObject);
	if (pSelectAddressDialog)
	{
		pSelectAddressDialog->SetSelectAddressDlg(this);
		pSelectAddressDialog->AttachDocument(m_pDoc);
		for (int i = 0; i <= pChilds->GetUpperBound(); i++)
		{
			GetAddress(pChilds->GetAt(i), TRUE);
			pSelectAddressDialog->AddAddress(m_CompleteAddress, m_Latitude, m_Longitude);
		}

		if (pSelectAddressDialog->ShowDialog() == IDOK)
		{
			for (int i = 0; i <= pChilds->GetUpperBound(); i++)
			{
				GetAddress(pChilds->GetAt(i), TRUE);
				if (GetGeocoderCompleteAddress().GetString() == pSelectAddressDialog->GetAddress())
				{
					bOK = SelectAddress(pChilds->GetAt(i), bShowDialog);
					SAFE_DELETE(pRootNode);
					SAFE_DELETE(pChilds);
					return bOK;
				}
			}
		}

		delete pObject;
	}
	else
		ASSERT_TRACE(FALSE, "Cannot instantiate addess dialog!! Cannot save settings!");
	
	SAFE_DELETE(pRootNode);
	SAFE_DELETE(pChilds);
	return bOK;
}

//----------------------------------------------------------------------------
BOOL CGeocoder::CheckStatus()
{
	if (m_Status == GEOCODER_RESULT_ZERO_RESULTS)
	{
		m_Status = _TB("Non-existent address or latitude and/or longitude are in a remote location");
		return FALSE;
	} 
	else if (m_Status == GEOCODER_RESULT_OVER_QUERY_LIMIT)
	{
		m_Status = _TB("Over quota (2500 requests per day maximum)");
		return FALSE;
	}
	else if (m_Status == GEOCODER_RESULT_REQUEST_DENIED)
	{
		m_Status = _TB("Request denied");
		return FALSE;
	}
	else if (m_Status == GEOCODER_RESULT_INVALID_REQUEST)
	{
		m_Status = _TB("Query is missing");
		return FALSE;
	}
	return TRUE;
}

//----------------------------------------------------------------------------
BOOL CGeocoder::SelectAddress(CXMLNode* pNode, 
							  BOOL		bShowDialog)
{
	BOOL bOk = FALSE;
	CObject* pObject = GetAddressDlgClass()->CreateObject();
	CAddressDlgObj * pAddressDialog = dynamic_cast< CAddressDlgObj *>(pObject);
	if (pAddressDialog)
	{
		
		if (m_bIsBrazil)
		{
			if (GetLatitudeLongitude(pNode))
			{
				if (bShowDialog)
				{
					pAddressDialog->SetAddress(
						this,
						m_AddressType.IsEmpty() ? m_Address : m_AddressType + _T(" ") + m_Address, 
						m_StreetNumber, 
						m_City, 
						m_County, 
						m_Region, 
						m_FederalState, 
						m_Country, 
						m_ZipCode, 
						m_Latitude, 
						m_Longitude, 
						m_AddressType);

					pAddressDialog->AttachDocument(m_pDoc);
					bOk = (pAddressDialog->ShowDialog() == IDOK);
				}
				else
					bOk = TRUE;
			}
			else
				bOk = FALSE;
		}
		else
			if (GetAddress(pNode))
			{
				if (bShowDialog)
				{
					pAddressDialog->SetAddress(this, 
						m_AddressType.IsEmpty() ? m_Address : m_AddressType + _T(" ") + m_Address, 
						m_StreetNumber, 
						m_City, 
						m_County, 
						m_Region, 
						m_FederalState, 
						m_Country, 
						m_ZipCode, 
						m_Latitude, 
						m_Longitude, 
						m_AddressType);

					pAddressDialog->AttachDocument(m_pDoc);
				
					bOk = (pAddressDialog->ShowDialog() == IDOK);
				}
				else
					bOk = TRUE;
			}
			else
				bOk = FALSE;

		delete pObject;
	}
	else
		ASSERT_TRACE(FALSE, "Cannot instantiate addess dialog!! Cannot save settings!");

	return bOk;
}

//----------------------------------------------------------------------------
BOOL CGeocoder::GetAddress(CXMLNode* pNode, 
						   BOOL		 bCompleteAddressOnly/* = FALSE*/)
{
	m_CompleteAddress.	Clear();
	m_Latitude.			Clear();
	m_Longitude.		Clear();
	m_StreetNumber.		Clear();
	m_AddressType.		Clear();
	m_Address.			Clear();
	m_City.				Clear();
	m_County.			Clear();
	m_Region.			Clear();
	m_FederalState.		Clear();	
	m_Country.			Clear();
	m_ISOCode.			Clear();
	m_ZipCode.			Clear();

	DataStr aType = _T("");

	CXMLNode* pTypeNode		= NULL;
	CXMLNode* pChildNode	= NULL;

	ASSIGN_VALUE(FORMATTED_ADDRESS, pNode, m_CompleteAddress);

	SELECT_NODE(pTypeNode,	pNode,		GEOMETRY);
	SELECT_NODE(pChildNode,	pTypeNode,	LOCATION);

	ASSIGN_VALUE(LATITUDE,	pChildNode, m_Latitude);
	ASSIGN_VALUE(LONGITUDE, pChildNode, m_Longitude);

	SAFE_DELETE(pTypeNode);
	SAFE_DELETE(pChildNode);

	if (bCompleteAddressOnly)
		return TRUE;

	CXMLNodeChildsList* pChilds		= NULL; 
	CXMLNodeChildsList* pTypeNodes	= NULL;
	CXMLNode*			pLongNode	= NULL;
	CXMLNode*			pShortNode	= NULL;
	CString				aAddress;
	int n = 0;

	SELECT_NODES(pChilds, pNode, ADDRESS_COMPONENT);

	for (int i = 0; i <= pChilds->GetUpperBound (); i++)
	{
		pChildNode = pChilds->GetAt(i);

		SELECT_NODES(pTypeNodes, pChildNode, TYPE);
		pTypeNode = pTypeNodes->GetAt(0);
		GET_NODE_VALUE(pTypeNode, aType);

		SELECT_NODE(pLongNode,	pChildNode, LONG_NAME);
		SELECT_NODE(pShortNode, pChildNode, SHORT_NAME);

		if (aType == GEOCODER_TYPE_STREET_NUMBER)
		{
			GET_NODE_VALUE(pLongNode, m_StreetNumber);	
		}
		else if (aType == GEOCODER_TYPE_ROUTE)
		{
			GET_NODE_VALUE(pLongNode, m_Address);
			if (UseAddressType())
			{
				aAddress = m_Address.Str();
				n = aAddress.Find(_T(' '));
				if ( n != -1)
				{
					m_Address		= aAddress.Right(aAddress.GetLength() - (n + 1));
					m_AddressType	= aAddress.Left(n);
				}
			}			
		}
		else if (aType == GEOCODER_TYPE_LOCALITY)
		{
			GET_NODE_VALUE(pLongNode, m_City);	
		}
		else if (aType == GEOCODER_TYPE_ADMINISTRATIVE_AREA_LEVEL_2)
		{
			GET_NODE_VALUE(pShortNode, m_County);	
		}
		else if (aType == GEOCODER_TYPE_ADMINISTRATIVE_AREA_LEVEL_1)
		{
			GET_NODE_VALUE(pLongNode,	m_Region);	
			GET_NODE_VALUE(pShortNode,	m_FederalState);	
		}
		else if (aType == GEOCODER_TYPE_COUNTRY)
		{
			GET_NODE_VALUE(pLongNode, m_Country);	
			GET_NODE_VALUE(pShortNode, m_ISOCode);	
		}
		else if (aType == GEOCODER_TYPE_POSTAL_CODE)
		{
			GET_NODE_VALUE(pLongNode, m_ZipCode);	
		}

		SAFE_DELETE(pTypeNodes);
		SAFE_DELETE(pLongNode);
		SAFE_DELETE(pShortNode);
	}
	SAFE_DELETE(pChilds);


	//  Anomalia 18282 - Google cambia nome ad alcune citta' per cui devo fare questo tappullo:
	if (m_City == _T("Reggio nell'Emilia")) 
		m_City = _T("Reggio Emilia");
	if (m_City == _T("Reggio di Calabria")) 
		m_City = _T("Reggio Calabria");

	//  Anomalia 18282 - Google cambia nome ad alcune regioni per cui devo fare questo tappullo:
	if (m_Region == _T("Emilia Romagna")) 
		m_Region = _T("Emilia-Romagna");

	// Anomalia 18329: in Svizzera la regione non è definita ma Google restituisce in Region il Cantone e in County la Città,  
	// per cui altro tappullo: sposto il codice del Cantone in County e pulisco Region
	if (m_ISOCode == _T("CH"))
	{
		m_County = m_FederalState;
		m_Region.Clear();
	}

	//Google restituisce come short name "São Paulo" invece di "SP"
	if (m_FederalState == _T("São Paulo"))
		m_FederalState = _T("SP");
	return TRUE;
}

//----------------------------------------------------------------------------
BOOL CGeocoder::GetLatitudeLongitude (CXMLNode* pNode)
{
	if (m_ZipCode.IsEmpty() || m_ISOCode.IsEmpty())
		return FALSE;

	m_Latitude.			Clear();
	m_Longitude.		Clear();

	DataStr aType = _T("");

	CXMLNode* pTypeNode		= NULL;
	CXMLNode* pChildNode	= NULL;

	SELECT_NODE(pTypeNode,	pNode,		GEOMETRY);
	SELECT_NODE(pChildNode,	pTypeNode,	LOCATION);

	ASSIGN_VALUE(LATITUDE,	pChildNode, m_Latitude);
	ASSIGN_VALUE(LONGITUDE, pChildNode, m_Longitude);

	SAFE_DELETE(pTypeNode);
	SAFE_DELETE(pChildNode);

	return TRUE;
}


//----------------------------------------------------------------------------
void CGeocoder::AddEncodeComp (CString& aCompleteAddress, CString aComp)
{
	aComp.Trim();
	if (!aComp.IsEmpty())
		if (aCompleteAddress.IsEmpty())
			aCompleteAddress = ::HTMLEncode(aComp);
		else
			aCompleteAddress += _T(",+") + ::HTMLEncode(aComp);
}

//----------------------------------------------------------------------------
CString CGeocoder::Address (CString aAddress, 
							CString aStreetNumber, 
						   CString aCity, 
						   CString aCounty, 
						   CString aCountry, 
							CString aISOCode, 
						   CString aZip /*= _T("")*/)
{
	CString aCompleteAddress = _T("");


	AddEncodeComp(aCompleteAddress, aAddress);
	AddEncodeComp(aCompleteAddress, aStreetNumber);
	AddEncodeComp(aCompleteAddress, aCity);
	AddEncodeComp(aCompleteAddress, aCounty);
	AddEncodeComp(aCompleteAddress, aCountry);
	AddEncodeComp(aCompleteAddress, aISOCode);
	AddEncodeComp(aCompleteAddress, aZip);

	return aCompleteAddress;
}

//----------------------------------------------------------------------------
BOOL CGeocoder::OpenGoogleMaps(CString aLatitude, 
							   CString aLongitude, 
							   BOOL	   bSatelliteView/* = FALSE*/)
{
	if (aLongitude == _T("") || aLatitude == _T(""))
		return FALSE;

	HINSTANCE hInst = ShellExecute(NULL, NULL, GetGoogleWebLink(aLatitude, aLongitude, bSatelliteView), NULL, NULL, SW_SHOWDEFAULT); 	
	if (hInst <= (HINSTANCE)32)
	{
		AfxMessageBox(ShellExecuteErrMsg((int)hInst));
		return FALSE;
	}
	else
		return TRUE;
}

//----------------------------------------------------------------------------
CString CGeocoder::GetGoogleWebLink(CString aLatitude, 
								CString aLongitude, 
							   BOOL	   bSatelliteView/* = FALSE*/)
{
	CString aType = _T("m");
	if (bSatelliteView)
		aType = _T("k");

	return cwsprintf(_T("http://maps.google.com/maps?f=q&hl=en&t=%s&q=%s,%s"), aType, aLatitude, aLongitude);
}


//----------------------------------------------------------------------------
BOOL CGeocoder::OpenGoogleMaps (CString webAddress)
{
	HINSTANCE hInst = ShellExecute(NULL, NULL, webAddress, NULL, NULL, SW_SHOWDEFAULT); 	
	if (hInst <= (HINSTANCE)32)
	{
		AfxMessageBox(ShellExecuteErrMsg((int)hInst));
		return FALSE;
	}
	else
		return TRUE;
}
//----------------------------------------------------------------------------
BOOL CGeocoder::OpenGoogleMaps(CString aAddress, 
								CString aStreetNumber, 
								CString aCity, 
								CString aCounty, 
								CString aCountry,
								CString aFederalState,
								CString aZip, 
								BOOL bSatelliteView/* = FALSE*/)
{
	if (aAddress == _T(""))
		return FALSE;

	return OpenGoogleMaps(GetGoogleWebLink(aAddress, aStreetNumber, aCity, aCounty, aCountry, aFederalState, aZip, bSatelliteView));
}

//----------------------------------------------------------------------------
CString CGeocoder::GetGoogleWebLink(CString aAddress, 
								CString aCity, 
								CString aCounty, 
								CString aCountry, 
								CString aZip, 
								BOOL bSatelliteView/* = FALSE*/)
{
	return GetGoogleWebLink(aAddress, _T(""), aCity, aCounty, aCountry, _T(""), aZip, bSatelliteView); 	
}

//----------------------------------------------------------------------------
CString CGeocoder::GetGoogleWebLink(CString aAddress, 
									CString aStreetNumber, 
									CString aCity, 
									CString aCounty, 
									CString aCountry,
									CString aFederalState,
									CString aZip, 
									BOOL bSatelliteView/* = FALSE*/)
{
	CString aType = _T("m");
	if (bSatelliteView) 
		aType = _T("k");

	return cwsprintf(_T("http://maps.google.com/maps?f=q&hl=en&t=%s&q="), aType) + Address(aAddress, aStreetNumber, aCity, aCounty, aCountry, aFederalState, aZip); 	
}
