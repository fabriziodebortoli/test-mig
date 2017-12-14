#pragma once

#include <TbGes\DBT.H>

#include <TbGes\TBBaseNavigation.h>

#include "TWorkers.h"
#include "TResources.h"
#include "ADMResourcesMng.h"
#include "RMControls.h"
#include "RMFunctions.h"

#include "beginh.dex"

class BDResourcesLayout;
class ResourcesLayoutDetailsStrings;
class CGeocoder;

const DWORD E_LOCAL_COMPANY_TAG = 999;
const DWORD E_LOCAL_RESOURCE_TYPE_COMPANY = MAKELONG(E_LOCAL_COMPANY_TAG, 414); 

#define ADD_DETAIL_LAYOUT(Name, Value, HasHyperlink) AddDetail(ResourcesLayoutDetailsStrings::##Name(), ##Value, ##HasHyperlink);
#define ADD_SEPARATOR_LAYOUT(Name) AddSeparator(ResourcesLayoutDetailsStrings::##Name());

enum DragDropAction { DRAG_DROP_COPY, DRAG_DROP_MOVE, DRAG_DROP_CANCEL };
enum DocumentAction { OPEN_DOCUMENT, NEW_RESOURCE, NEW_WORKER };

// tipi risorsa memorizzati nella key di ogni nodo (0: default(=empty), 1: company, 2: risorsa, 3: worker)
const int RESOURCE_DEFAULT = 0;
const int RESOURCE_ROOT = 1;
const int RESOURCE_RESOURCE = 2;
const int RESOURCE_WORKER = 3;

//=============================================================================
BEGIN_TB_STRING_MAP(ResourcesLayoutDetailsStrings)
	TB_LOCALIZED(COMPANY,		"Company")
	TB_LOCALIZED(TELEPHONE_1,	"Telephone 1")
	TB_LOCALIZED(TELEPHONE_2,	"Telephone 2")
	TB_LOCALIZED(TELEX,			"Telex")
	TB_LOCALIZED(OTHER_DATA,	"Other data")
	TB_LOCALIZED(WEB,			"Website")
	TB_LOCALIZED(EMAIL,			"E-Mail")
	TB_LOCALIZED(RESOURCE,		"Resource")
	TB_LOCALIZED(TYPE,			"Type")
	TB_LOCALIZED(RESOURCE_CODE,	"Code")
	TB_LOCALIZED(DESCRIPTION,	"Description")
	TB_LOCALIZED(DISABLED,		"Disabled")
	TB_LOCALIZED(MANAGER,		"Manager")
	TB_LOCALIZED(COST_CENTER,	"Cost center")
	TB_LOCALIZED(NOTES,			"Notes")
	TB_LOCALIZED(WORKER,		"Worker")
	TB_LOCALIZED(WORKER_ID,		"Worker ID")
	TB_LOCALIZED(TITLE,			"Title")
	TB_LOCALIZED(NAME,			"Name")
	TB_LOCALIZED(LAST_NAME,		"Last name")
	TB_LOCALIZED(MASTER_DATA,	"Master data")
	TB_LOCALIZED(ADDRESS,		"Address")
	TB_LOCALIZED(CITY,			"City")
	TB_LOCALIZED(COUNTY,		"County")
	TB_LOCALIZED(ZIP_CODE,		"Zip code")
	TB_LOCALIZED(COUNTRY,		"Country")
	TB_LOCALIZED(TELEPHONE,		"Telephone")
	TB_LOCALIZED(OFFICE,		"Office")
	TB_LOCALIZED(MOBILE,		"Mobile")
	TB_LOCALIZED(FAX,			"Fax")
	TB_LOCALIZED(HOME,			"Home")
	TB_LOCALIZED(BIRTH_DATE,	"Birth date")
	TB_LOCALIZED(BIRTH_CITY,	"City of birth")
	TB_LOCALIZED(CIVIL_STATUS,	"Civil status")
	TB_LOCALIZED(GENDER,		"Gender")
	TB_LOCALIZED(FISCAL_CODE,	"Fiscal code")
	TB_LOCALIZED(REGISTER,		"Register")
	TB_LOCALIZED(EMPLOYMENT,	"Employment")
	TB_LOCALIZED(RESIGNATION,	"Resignation")
	TB_LOCALIZED(SKYPE,			"Skype")
	TB_LOCALIZED(CUSTOM_DATA,	"Custom data")
	TB_LOCALIZED(OTHER,			"Other")
	TB_LOCALIZED(EMPTY,			"")
END_TB_STRING_MAP()


/////////////////////////////////////////////////////////////////////////////////
//							CRootDetails
/////////////////////////////////////////////////////////////////////////////////
//===============================================================================
class TB_EXPORT CRootDetails : public CObject
{
	DECLARE_DYNCREATE(CRootDetails)

public:
	CRootDetails() {}

public:
	DataStr m_TitleCode;
	DataStr m_CompanyName;
	DataStr m_Address;
	DataStr m_City;
	DataStr m_County;
	DataStr m_ZipCode;
	DataStr m_Status;
	DataStr m_Telephone1;
	DataStr m_Telephone2;
	DataStr m_Fax;
	DataStr m_Telex;
	DataStr m_InternetAddress;
	DataStr m_Email;
};

///////////////////////////////////////////////////////////////////////////////
//					HKLDetailWorkers declaration
///////////////////////////////////////////////////////////////////////////////
//=============================================================================
class TB_EXPORT HKLDetailWorkers : public HKLWorkers
{
	DECLARE_DYNCREATE (HKLDetailWorkers)

public:
      HKLDetailWorkers();

protected:
      virtual void OnDefineQuery	(SelectionType nQuerySelection = DIRECT_ACCESS);
      virtual void OnPrepareQuery	(DataObj*, SelectionType nQuerySelection = DIRECT_ACCESS);
};

//=============================================================================================
class TB_EXPORT BDResourcesLayout : public CTBBaseNavigationDocument, public ADMResourcesLayoutObj
{
	DECLARE_DYNCREATE(BDResourcesLayout)
	
	virtual ADMObj* GetADM() { return this; }

public:
	BDResourcesLayout();
	~BDResourcesLayout();

private:
	DataBool	m_bUserIsAdmin;
	int			m_QuantityDecimalNumbers;
	int			m_MoneyDecimalNumbers;
	int			m_Index;

	DataBool	m_bNodeKeyIsEmpty;
	DataBool	m_bNodeKeyCanBeDisabled;

public:
	CRootDetails*		m_pRootDetails;
	CString				m_RootKeyDescription;
	TRResources			m_TRResources;
	TRWorkers			m_TRWorkers;
	TUWorkers			m_TUWorkers;
	TUResources			m_TUResources;
	TUResourcesDetails	m_TUResourcesDetails;
	TUWorkersDetails	m_TUWorkersDetails;

public:
	int					m_ResourceTypeId; // tipo risorsa (in base al tipo nodo)
	DataStr				m_ResourceType;
	DataStr				m_Resource;
	DataBool			m_ResourceDisabled;
	DataBool			m_DisabledToo;
	DataBool			m_AllResources;
	DataBool			m_SelectResource;
	DataStr				m_TileImageCaption;

	DataBool			m_TreeView;
	DataStr				m_DetailResourceType;
	DataLng				m_DetailWorker;
	DataStr				m_DetailEmail;
	DataStr				m_DetailWeb;
	DataStr				m_DetailAddress;
	DataStr				m_DetailCity;
	DataStr				m_DetailCounty;
	DataStr				m_DetailCountry;
	DataStr				m_DetailZip;
	DataStr				m_DetailImage;
	DataStr				m_DetailLongitude;
	DataStr				m_DetailLatitude;
	DataStr				m_DetailName;

	// gestione jsonslaveview visualizzata durante il drag nel tree
	DragDropAction		m_DragDropAction;
	DataStr				m_sActionMessage;
	DataStr				m_sButton1;
	DataStr				m_sButton2;

public:
	CheckResourcesRecursion*	m_pCheckResourcesRecursion;
	CResourcesFunctions*		m_pFunctions;
	CGeocoder*					m_pGecoder;
	CResourcesPictureStatic*	m_pResourcePicture;

public:
	virtual void DoSelectionNodeChanged	(CString nodeKey);
	virtual void DoDragDrop				();
	virtual void ShowEmptyDetails		();
	virtual BOOL PopulateTree			();
	virtual void DoDetailGridRowChanged	();
	virtual void DoCtxMenuItemClick		(CString nodeKey);
	virtual void InitTree				();
	virtual BOOL DoEnableFind			();
	virtual BOOL DoEnableDetails		();
	virtual BOOL OnFilterValidate		();
	
private:
	void	DoTreeDragDrop				();
	void	ConfirmDragDrop				(const long aKeyType, const CString aResourceType, const CString aResourceCode, 
										 const long aFromKeyType, const CString aFromResourceType, const CString aFromResourceCode,
										 const long aToKeyType, const CString aToResourceType, const CString aToResourceCode);
	void	MoveResource				(const long aKeyType, const CString aResourceType, const CString aResourceCode, const DataLng aWorkerID,
										 const long aFromKeyType, const CString aFromResourceType, const CString aFromResourceCode, const DataLng aFromWorkerID,
										 const long aToKeyType, const CString aToResourceType, const CString aToResourceCode, const DataLng aToWorkerID,
										 int aNewParentKeyIndex);

	BOOL	CheckRecursivity			(DataStr aToResourceType, DataStr aToResource, DataStr aResourceType, DataStr aResource);

	void	RunDocument					(DocumentAction aAction = OPEN_DOCUMENT);
	BOOL	EnableDisable				();
	void	LoadAllResources			(CString aKey);
	void	LoadResourceChildren		(const DataStr&	aResourceType, const DataStr& aResource, int aIndex);
	void	LoadAllWorkers				(CString aKey);
	void	LoadWorkerChildren			(const DataLng& aWorkerID, int aIndex);

	void	ShowRootDetails				();
	void	ShowResourceDetails			(const DataStr& aResourceType, const DataStr& aResource);
	void	ShowWorkerDetails			(DataLng& aWorker);
	void	LoadWorkerCustomData		(DataLng& aWorker);
	void	LoadResourceCustomData		(const DataStr& aResourceType, const DataStr& aResource);
	void	SetDefaultImage				();

	CString GetResourceImage			(const DataStr&	aKey, BOOL bDisabled = FALSE);
	CString GetResourceKey				(int aIndex, int aTypeId, const DataStr& aResourceType, const DataStr& aResource);
	CString GetResourceDescription		(const DataStr& aResourceType, const DataStr& aResource);
	CString GetWorkerDescription		(DataLng& aWorker);
	void	OpenUrl						();
	void	SendEMail					();
	void	ShowMap						(BOOL bSatelliteView = FALSE);

	void	SetFilterDefaultValue		();
	void	ExtractResourceInfoFromKey	(const CString& aKey, long& aKeyType, CString& aResourceType, CString& aResourceCode, DataLng& aWorkerID);
	void	InitCtxMenuIndexes			();
	CString	ResolveAmp					(CString aValue);

private:
	virtual BOOL OnAttachData			();
	virtual	void DisableControlsForBatch();
	virtual void OnPrepareAuxData		(CAbstractFormView* pView);
	virtual void OnParsedControlCreated	(CParsedCtrl* pCtrl);

protected:
	//{{AFX_MSG(BDResourcesLayout)
	afx_msg void OnResourceChanged				();
	afx_msg void OnResourceTypeChanged			();
	afx_msg void OnSelectAllResourcesChanged	();
	afx_msg void OnToolbarInternet				();
	afx_msg void OnToolbarMail					();
	afx_msg void OnToolbarMap					();
	afx_msg void OnToolbarSatellite				();
	afx_msg void OnToolbarDisable				();
	afx_msg void OnToolbarOpenDocument			();
	afx_msg void OnToolbarNewResource			();
	afx_msg void OnToolbarNewWorker				();
	afx_msg void OnUpdateToolbarInternet		(CCmdUI*);
	afx_msg void OnUpdateToolbarMail			(CCmdUI*);
	afx_msg void OnUpdateToolbarMap				(CCmdUI*);
	afx_msg void OnUpdateToolbarSatellite		(CCmdUI*);
	afx_msg void OnUpdateToolbarDisable			(CCmdUI*);
	afx_msg void OnUpdateToolbarOpenDocument	(CCmdUI*);
	afx_msg void OnUpdateToolbarNewResource		(CCmdUI*);
	afx_msg void OnUpdateToolbarNewWorker		(CCmdUI*);
	//}}AFX_MSG

	DECLARE_MESSAGE_MAP()
};

#include "endh.dex"
