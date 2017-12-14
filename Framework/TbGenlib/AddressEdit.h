#pragma once

#include <TbGeneric\mlistbox.h>
#include "ParsEdtOther.h"

#include "beginh.dex"

class CAddressDlg;
class CSelectAddressDlg;
class CGeocoder;
class CAbstractFormDoc;

#define XML_GEOCODER_GEOCODE_RESPONSE				_T("GeocodeResponse")
#define XML_GEOCODER_STATUS							_T("status")
#define XML_GEOCODER_RESULT							_T("result")
#define XML_GEOCODER_TYPE							_T("type")
#define XML_GEOCODER_FORMATTED_ADDRESS				_T("formatted_address")
#define XML_GEOCODER_ADDRESS_COMPONENT				_T("address_component")
#define XML_GEOCODER_LONG_NAME						_T("long_name")
#define XML_GEOCODER_SHORT_NAME						_T("short_name")
#define XML_GEOCODER_GEOMETRY						_T("geometry")
#define XML_GEOCODER_LOCATION						_T("location")
#define XML_GEOCODER_LATITUDE						_T("lat")
#define XML_GEOCODER_LONGITUDE						_T("lng")

#define GEOCODER_RESULT_OK							_T("OK")
#define GEOCODER_RESULT_ZERO_RESULTS				_T("ZERO_RESULTS")
#define GEOCODER_RESULT_OVER_QUERY_LIMIT			_T("OVER_QUERY_LIMIT")
#define GEOCODER_RESULT_REQUEST_DENIED				_T("REQUEST_DENIED")
#define GEOCODER_RESULT_INVALID_REQUEST				_T("INVALID_REQUEST")

#define GEOCODER_TYPE_STREET_ADDRESS 				_T("street_address")
#define GEOCODER_TYPE_ROUTE			 				_T("route")
#define GEOCODER_TYPE_INTERSECTION 					_T("intersection")
#define GEOCODER_TYPE_POLITICAL 					_T("political")
#define GEOCODER_TYPE_COUNTRY 						_T("country")
#define GEOCODER_TYPE_ADMINISTRATIVE_AREA_LEVEL_1 	_T("administrative_area_level_1")
#define GEOCODER_TYPE_ADMINISTRATIVE_AREA_LEVEL_2 	_T("administrative_area_level_2")
#define GEOCODER_TYPE_ADMINISTRATIVE_AREA_LEVEL_3 	_T("administrative_area_level_3")
#define GEOCODER_TYPE_COLLOQUIAL_AREA  				_T("colloquial_area")
#define GEOCODER_TYPE_LOCALITY 						_T("locality")
#define GEOCODER_TYPE_SUBLOCALITY  					_T("sublocality")
#define GEOCODER_TYPE_NEIGHBORHOOD 					_T("neighborhood")
#define GEOCODER_TYPE_PREMISE 						_T("premise")
#define GEOCODER_TYPE_SUBPREMISE 					_T("subpremise")
#define GEOCODER_TYPE_POSTAL_CODE  					_T("postal_code")
#define GEOCODER_TYPE_NATURAL_FEATURE  				_T("natural_feature")
#define GEOCODER_TYPE_AIRPORT  						_T("airport")
#define GEOCODER_TYPE_PARK  						_T("park")
#define GEOCODER_TYPE_POINT_OF_INTEREST  			_T("point_of_interest")
#define GEOCODER_TYPE_POST_BOX  					_T("post_box")
#define GEOCODER_TYPE_STREET_NUMBER 				_T("street_number")
#define GEOCODER_TYPE_FLOOR  						_T("floor")
#define GEOCODER_TYPE_ROOM  						_T("room")

/////////////////////////////////////////////////////////////////////////////
//	class CAddressDlgObj definition
/////////////////////////////////////////////////////////////////////////////
class TB_EXPORT CAddressDlgObj
{
public:
	virtual int  ShowDialog() = 0;
	virtual void AttachDocument(CBaseDocument* pDoc) = 0;
	virtual void SetAddress(CGeocoder*	pGeocoder,
		CString		aAddress,
		CString		aStreetNumber,
		CString		aCity,
		CString		aCounty,
		CString		aRegion,
		CString		aFederalState,
		CString		aCountry,
		CString		aZipCode,
		CString		aLatitude,
		CString		aLongitude,
		CString	aAddressType = _T("")) = 0;
};

/////////////////////////////////////////////////////////////////////////////
//	class CBaseAddressEditObj definition
/////////////////////////////////////////////////////////////////////////////

class TB_EXPORT CSelectAddressDlgObj
{
public:
	virtual int  ShowDialog() = 0;
	virtual void SetSelectAddressDlg(CGeocoder* pGeocoder) = 0;
	virtual CString	GetAddress() = 0;
	virtual void AddAddress(CString aAddress, CString aLatitude, CString aLongitude) = 0;
	virtual void AttachDocument(CBaseDocument* pDoc) = 0;
};

//=============================================================================
//			Class CBaseAddressEdit
//=============================================================================
class TB_EXPORT CBaseAddressEdit : public CLinkEdit
{
DECLARE_DYNAMIC(CBaseAddressEdit)

public:
	// Construction
	CBaseAddressEdit			();
	CBaseAddressEdit			(UINT nBtnIDBmp, DataStr* = NULL);
	virtual ~CBaseAddressEdit	();

	enum FieldType { STREET_NUMBER , CITY , COUNTY , COUNTRY , ISO_CODE , ZIP, REGION , LATITUDE, LONGITUDE, ADDRESS, DISTRICT, FEDERAL_STATE, ADDRESS_TYPE };

public:
	DataStr* m_pStreetNumber;
	DataStr* m_pCity;
	DataStr* m_pCounty;
	DataStr* m_pCountry;
	DataStr* m_pISOCode;
	DataStr* m_pZip;
	DataStr* m_pRegion;
	DataStr* m_pLatitude;
	DataStr* m_pLongitude;
	DataStr* m_pDistrict;
	DataStr* m_pFederalState;
	DataStr* m_pAddressType;

	DataStr* m_pAddress;

	int		m_nDataIdxStreetNumber;
	int		m_nDataIdxCity;
	int		m_nDataIdxCounty;
	int		m_nDataIdxCountry;
	int		m_nDataIdxISOCode;
	int		m_nDataIdxZip;
	int		m_nDataIdxRegion;
	int		m_nDataIdxLatitude;
	int		m_nDataIdxLongitude;
	int		m_nDataIdxDistrict;
	int		m_nDataIdxFederalState;
	int		m_nDataIdxAddressType;

	int		m_nDataIdxAddress;
	
public:
	void Bind			(DataStr* pField, FieldType eFieldType, int nDataIdx = -1);	//Bodyedit e RowView

	void BindStreetNumber	(DataStr* pStreetNumber,	int nDataIdx = -1)					{ m_pStreetNumber	= pStreetNumber;	m_nDataIdxStreetNumber	= nDataIdx; }
	void BindCity			(DataStr* pCity,			int nDataIdx = -1)					{ m_pCity			= pCity;			m_nDataIdxCity			= nDataIdx; }
	void BindCounty			(DataStr* pCounty,			int nDataIdx = -1)					{ m_pCounty			= pCounty;			m_nDataIdxCounty		= nDataIdx; }
	void BindCountry		(DataStr* pCountry,			int nDataIdx = -1)					{ m_pCountry		= pCountry;			m_nDataIdxCountry		= nDataIdx; }
	void BindISOCode		(DataStr* pISOCode,			int nDataIdx = -1)					{ m_pISOCode		= pISOCode;			m_nDataIdxISOCode		= nDataIdx; }
	void BindZip			(DataStr* pZip,				int nDataIdx = -1)					{ m_pZip			= pZip;				m_nDataIdxZip			= nDataIdx; }
	void BindRegion			(DataStr* pRegion,			int nDataIdx = -1)					{ m_pRegion			= pRegion;			m_nDataIdxRegion		= nDataIdx; }
	void BindLatitude		(DataStr* pLatitude,		int nDataIdx = -1)					{ m_pLatitude		= pLatitude;		m_nDataIdxLatitude		= nDataIdx; }
	void BindLongitude		(DataStr* pLongitude,		int nDataIdx = -1)					{ m_pLongitude		= pLongitude;		m_nDataIdxLongitude		= nDataIdx; }
	
	void BindAddress		(DataStr* pAddress,			int nDataIdx = -1)					{ m_pAddress		= pAddress;			m_nDataIdxAddress		= nDataIdx; }
	void BindDistrict		(DataStr* pDistrict,		int nDataIdx = -1)					{ m_pDistrict		= pDistrict;		m_nDataIdxDistrict		= nDataIdx; }
	void BindFederalState	(DataStr* pFederalState,	int nDataIdx = -1)					{ m_pFederalState	= pFederalState;	m_nDataIdxFederalState	= nDataIdx; }
	void BindAddressType	(DataStr* pAddressType,		int nDataIdx = -1)					{ m_pAddressType	= pAddressType;		m_nDataIdxAddressType	= nDataIdx; }

public:
			void	Attach				(UINT nBtnID)	{ m_nButtonIDBmp = (nBtnID == NO_BUTTON) ? NO_BUTTON : BTN_MENU_ID; }
	virtual void	Attach				(DataObj*);
};

//=============================================================================
//			Class CAddressEdit
//=============================================================================
class TB_EXPORT CAddressEdit : public CBaseAddressEdit
{
DECLARE_DYNCREATE(CAddressEdit)

private:
	CGeocoder*			m_pGeocoder;
	
public:
	void Bind		(DataStr* pField, FieldType eFieldType)
			{ __super::Bind(pField, (CBaseAddressEdit::FieldType) eFieldType); }

	// Construction
	CAddressEdit			();
	CAddressEdit			(UINT nBtnIDBmp, DataStr* = NULL);
	virtual ~CAddressEdit	();

public:
	virtual	BOOL	GetMenuButton		(CMenu*);
	virtual	void	DoCmdMenuButton		(UINT nUINT);

	virtual CString	GetMenuButtonImageNS();
	virtual void	PostClickMessage();

	void	ShowMap(BOOL bSatelliteView = FALSE);
	CString GetGoogleWebLink(BOOL bSatelliteView = FALSE);

	void SetAddressDlgClass(CRuntimeClass* pClass);
	CRuntimeClass* GetAddressDlgClass();

	void SetSelectAddressDlgClass(CRuntimeClass* pClass);
	CRuntimeClass* GetSelectAddressDlgClass();

protected:
	//{{AFX_MSG(CAddressEdit)
	afx_msg void	OnSearchAddress		();
	afx_msg void	OnShowMap			();
	afx_msg void	OnShowSatellite		();
	//}}AFX_MSG

    DECLARE_MESSAGE_MAP()
};

//=============================================================================
class TB_EXPORT CGeocoder : public CObject
{      
	DECLARE_DYNAMIC(CGeocoder)

public:
	CGeocoder();

private:
	BOOL	m_bIsBrazil;
	BOOL	m_bUseAddressType;
	DataStr	m_Status;
	DataStr	m_CompleteAddress;
	DataStr	m_AddressType;
	DataStr m_Address;
	DataStr m_StreetNumber;
	DataStr m_City;
	DataStr m_County;
	DataStr m_Region;
	DataStr	m_FederalState;
	DataStr m_Country;
	DataStr m_ISOCode;
	DataStr m_ZipCode;
	DataStr m_Latitude;
	DataStr m_Longitude;
	CRuntimeClass* m_pAddressDlgClass;
	CRuntimeClass* m_pSelectAddressDlg;
	CBaseDocument* m_pDoc;

private:
	static CString	Address		(CString aAddress, 
								 CString	aStreetNumber, 
						 CString aCity, 
						 CString aCounty, 
						 CString aCountry, 
								 CString	aISOCode, 
						 CString aZip = _T(""));
	static void AddEncodeComp	(CString&	aCompleteAddress, 
								 CString	aComp);

	BOOL	GetData		(CXMLDocumentObject*	pXmlDoc,
						 BOOL					bShowDialog = FALSE);
	BOOL	GetAddress	(CXMLNode*				pNode, 
						 BOOL 				    bCompleteAddressOnly	= FALSE);
	BOOL	GetLatitudeLongitude (CXMLNode*				pNode);
	BOOL	SelectAddress		(CXMLNode*				pNode,
						 BOOL					bShowDialog = FALSE);
	BOOL	CheckStatus	 (); 

public:

	VOID	SetDocument(CBaseDocument* pDoc) { m_pDoc = pDoc; }

	BOOL	SetGeocoder			(CString aAddress, 
								 CString aStreetNumber, 
								 CString aCity, 
								 CString aCounty,
								 CString aCountry,
								 CString aISOCode, 
								 CString aZip,
								 CString aFederalState,
								 BOOL bShowDialog = FALSE);

	void SetAddressDlgClass(CRuntimeClass* pClass);
	CRuntimeClass* GetAddressDlgClass();

	void SetSelectAddressDlgClass(CRuntimeClass* pClass);
	CRuntimeClass* GetSelectAddressDlgClass();

public:
	BOOL	IsBrazil					() { return m_bIsBrazil; }
	BOOL	UseAddressType				() { return m_bUseAddressType; }
	void	SetUseAddressType			(BOOL bUseAddressType)	{ m_bUseAddressType = bUseAddressType;}
	BOOL	IsGeocoderOK				() { return m_Status == GEOCODER_RESULT_OK; }
	DataStr	GetGeocoderStatus			() { return m_Status; }

	DataStr	GetGeocoderCompleteAddress	() { return m_CompleteAddress; }
	DataStr	GetGeocoderAddressType		() { return m_AddressType; }
	DataStr	GetGeocoderAddress			() { return m_Address; }
	DataStr	GetGeocoderAddressWithNumber() { return m_bIsBrazil || m_Address.IsEmpty() ? m_Address : m_Address + _T(" ") + m_StreetNumber; }
	DataStr	GetGeocoderStreetNumber		() { return m_StreetNumber; }
	DataStr	GetGeocoderCity				() { return m_City; }
	DataStr	GetGeocoderCounty			() { return m_County; }
	DataStr	GetGeocoderRegion			() { return m_Region; }
	DataStr	GetGeocoderFederalState		() { return m_FederalState; }
	DataStr	GetGeocoderCountry			() { return m_Country; }
	DataStr	GetGeocoderISOCode			() { return m_ISOCode; }
	DataStr	GetGeocoderZipCode			() { return m_ZipCode; }
	DataStr	GetGeocoderLatitude			() { return m_Latitude; }
	DataStr	GetGeocoderLongitude		() { return m_Longitude; }

	static CString GetGoogleWebLink		(CString aLatitude, 
										 CString aLongitude, 
										 BOOL bSatelliteView = FALSE);
	static CString GetGoogleWebLink		(CString aAddress, 
										 CString aCity, 
										 CString aCounty, 
										 CString aCountry, 
										 CString aZip, 
										 BOOL	 bSatelliteView = FALSE);
	static CString GetGoogleWebLink		(CString aAddress, 
										 CString aStreetNumber, 
										 CString aCity, 
										 CString aCounty,
										 CString aCountry,
										 CString aFederalState,
										 CString aZip, 
										 BOOL	 bSatelliteView = FALSE);

public:
	BOOL	OpenGoogleMaps		(CString aLatitude, 
								 CString aLongitude, 
								 BOOL	 bSatelliteView = FALSE);
	BOOL	OpenGoogleMaps		(CString aAddress, 
								 CString aStreetNumber, 
								 CString aCity, 
								 CString aCounty, 
								 CString aCountry, 
								 CString aFederalState,
								 CString aZip, 
								 BOOL	 bSatelliteView = FALSE);
	BOOL	OpenGoogleMaps		(CString webAddress);
};

//=============================================================================

#include "endh.dex"
