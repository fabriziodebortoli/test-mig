#include "stdafx.h"

#include <TbFrameworkImages\CommonImages.h>
#include <TbFrameworkImages\GeneralFunctions.h>

//locals
#include "RMSettings.h"
#include "BDResourcesLayout.h"
#include "UIResourcesLayout.h"
#include "UIResourcesLayoutAction.h"

#include "JsonForms\ResourcesLayoutAction\IDD_RESOURCES_LAYOUT_ACTION.hjson"

static TCHAR szParamHKLWorkers			[] = _T("p0");
static TCHAR szParamIsWorker			[] = _T("p1");
static TCHAR szParamResourceType		[] = _T("p2");
static TCHAR szParamResource			[] = _T("p3");
static TCHAR szParamWorker3				[] = _T("p4");
static TCHAR szParamResourceHide		[] = _T("p5");
static TCHAR szParamResourceHide2		[] = _T("p6");
static TCHAR szParamResourceHide3		[] = _T("p7");
static TCHAR szParamResourceHide4		[] = _T("p8"); 
static TCHAR szParamResourceDisabled	[] = _T("p9");
static TCHAR szParamResourceDisabled2	[] = _T("p10");
static TCHAR szParamResourceCode4		[] = _T("p11");
static TCHAR szParamResourceType4		[] = _T("p12");
static TCHAR szParamIsWorker2			[] = _T("p13");

static TCHAR szCompanyImage				[] = _T("Company");
static TCHAR szResourceImage			[] = _T("Resource");
static TCHAR szWorkerImage				[] = _T("Worker");
static TCHAR szCompanyImageDisabled		[] = _T("Company_Disabled");
static TCHAR szResourceImageDisabled	[] = _T("Resource_Disabled");
static TCHAR szWorkerImageDisabled		[] = _T("Worker_Disabled");

DWORD CONTEXT_MENU_EXPAND_COLLAPSE		= 0;
DWORD CONTEXT_MENU_ENABLE_DISABLE		= 0;
DWORD CONTEXT_MENU_OPEN_DOCUMENT		= 0;
DWORD CONTEXT_MENU_NEW_RESOURCE			= 0;
DWORD CONTEXT_MENU_NEW_WORKER			= 0;
DWORD CONTEXT_MENU_OPEN_WEBSITE			= 0;
DWORD CONTEXT_MENU_SEND_EMAIL			= 0;
DWORD CONTEXT_MENU_SHOW_MAP				= 0;
DWORD CONTEXT_MENU_SHOW_SATELLITE_VIEW	= 0;

// nr. caratteri per effettuare il padding dei codici risorsa/worker
const int RESOURCEPADCHARNR = 8;
const int WORKERPADCHARNR	= 6;

#ifdef _DEBUG
#undef THIS_FILE                                                        
static char THIS_FILE[] = __FILE__;     
#endif   

//////////////////////////////////////////////////////////////////////////////
//							CRootDetails implementation class
////////////////////////////////////////////////////////////////////////////////
//--------------------------------------------------------------------------------
IMPLEMENT_DYNCREATE(CRootDetails, CObject)


///////				ATTENZIONE!!!!! questa classe col passaggio a json viene
//					solo istanziata ma MAI consumata (vecchia ReattachHotlink in RowChanged del BodyEdit di dettaglio!!!!!)
/////////////////////////////////////////////////////////////////////////////
//    Hotlink          ### HKLDetailWorkers ###              HKLDetailWorkers
/////////////////////////////////////////////////////////////////////////////
//
//-----------------------------------------------------------------------------
IMPLEMENT_DYNCREATE (HKLDetailWorkers, HKLWorkers)

//------------------------------------------------------------------------------
HKLDetailWorkers::HKLDetailWorkers() 
	: 
	HKLWorkers()
{}

//------------------------------------------------------------------------------
void HKLDetailWorkers::OnDefineQuery (SelectionType nQuerySelection)
{
	m_pTable->SelectAll();
      
	switch (nQuerySelection)
	{
		case DIRECT_ACCESS:
			m_pTable->AddFilterColumn    (GetRecord()->f_WorkerID);
			m_pTable->AddParam           (szParamHKLWorkers, GetRecord()->f_WorkerID);
			break;
                  
		case UPPER_BUTTON:
			m_pTable->AddSortColumn      (GetRecord()->f_WorkerID);
			m_pTable->AddFilterColumn	 (GetRecord()->f_WorkerID);
			m_pTable->AddParam           (szParamHKLWorkers, GetRecord()->f_WorkerID);
			break;

		case COMBO_ACCESS:
		case LOWER_BUTTON:
			m_pTable->AddSortColumn      (GetRecord()->f_LastName);
			break;
	}     
}

//------------------------------------------------------------------------------
void HKLDetailWorkers::OnPrepareQuery (DataObj* pDataObj, SelectionType nQuerySelection)
{
	ASSERT(pDataObj->IsKindOf(RUNTIME_CLASS(DataStr)));
    
	DataStr aStr;
	aStr.Assign(*pDataObj);
	DataLng aWorkerID = _wtol((aStr.GetString()));

	switch (nQuerySelection)
	{
		case DIRECT_ACCESS:
			m_pTable->SetParamValue(szParamHKLWorkers, aWorkerID);
			break;

		case UPPER_BUTTON:
			m_pTable->SetParamValue(szParamHKLWorkers, aWorkerID);
			break;
                  
		case COMBO_ACCESS:
		case LOWER_BUTTON:
			break;
	}
}

/////////////////////////////////////////////////////////////////////////////
//				class BDResourcesLayout Implementation
/////////////////////////////////////////////////////////////////////////////
//----------------------------------------------------------------------------
IMPLEMENT_DYNCREATE(BDResourcesLayout, CTBBaseNavigationDocument)

//----------------------------------------------------------------------------
BEGIN_MESSAGE_MAP(BDResourcesLayout, CTBBaseNavigationDocument)
	////{{AFX_MSG_MAP(BDResourcesLayout)
	ON_EN_VALUE_CHANGED	(IDC_RESOURCES_LAYOUT_RESOURCE,			OnResourceChanged)
	ON_EN_VALUE_CHANGED	(IDC_RESOURCES_LAYOUT_RESOURCE_TYPE,	OnResourceTypeChanged)
	ON_EN_VALUE_CHANGED	(IDC_RESOURCES_LAYOUT_BTN_ALL,			OnSelectAllResourcesChanged)
	ON_COMMAND			(ID_RESOURCES_LAYOUT_TB_OPEN,			OnToolbarOpenDocument)
	ON_UPDATE_COMMAND_UI(ID_RESOURCES_LAYOUT_TB_OPEN,			OnUpdateToolbarOpenDocument)
	ON_COMMAND			(ID_RESOURCES_LAYOUT_TB_NEWRESOURCE,	OnToolbarNewResource)
	ON_UPDATE_COMMAND_UI(ID_RESOURCES_LAYOUT_TB_NEWRESOURCE,	OnUpdateToolbarNewResource)
	ON_COMMAND			(ID_RESOURCES_LAYOUT_TB_NEWWORKER,		OnToolbarNewWorker)
	ON_UPDATE_COMMAND_UI(ID_RESOURCES_LAYOUT_TB_NEWWORKER,		OnUpdateToolbarNewWorker)
	ON_COMMAND			(ID_RESOURCES_LAYOUT_TB_INTERNET,		OnToolbarInternet)
	ON_UPDATE_COMMAND_UI(ID_RESOURCES_LAYOUT_TB_INTERNET,		OnUpdateToolbarInternet)
	ON_COMMAND			(ID_RESOURCES_LAYOUT_TB_MAIL,			OnToolbarMail)
	ON_UPDATE_COMMAND_UI(ID_RESOURCES_LAYOUT_TB_MAIL,			OnUpdateToolbarMail)
	ON_COMMAND			(ID_RESOURCES_LAYOUT_TB_MAP,			OnToolbarMap)
	ON_UPDATE_COMMAND_UI(ID_RESOURCES_LAYOUT_TB_MAP,			OnUpdateToolbarMap)
	ON_COMMAND			(ID_RESOURCES_LAYOUT_TB_SATELLITE,		OnToolbarSatellite)
	ON_UPDATE_COMMAND_UI(ID_RESOURCES_LAYOUT_TB_SATELLITE,		OnUpdateToolbarSatellite)
	ON_COMMAND			(ID_RESOURCES_LAYOUT_TB_DISABLE,		OnToolbarDisable)
	ON_UPDATE_COMMAND_UI(ID_RESOURCES_LAYOUT_TB_DISABLE,		OnUpdateToolbarDisable)
	////}}AFX_MSG_MAP
END_MESSAGE_MAP()

//----------------------------------------------------------------------------
BDResourcesLayout::BDResourcesLayout()
	:
	m_pCheckResourcesRecursion	(NULL),
	m_pGecoder					(NULL),
	m_pResourcePicture			(NULL),
	m_pFunctions				(NULL),
	m_TRResources				(this),
	m_TRWorkers					(this),
	m_TUWorkers					(this),
	m_TUResources				(this),
	m_TUResourcesDetails		(this),
	m_TUWorkersDetails			(this),
	m_ResourceDisabled			(FALSE),
	m_AllResources				(TRUE),
	m_bUserIsAdmin				(FALSE),
	m_bNodeKeyIsEmpty			(FALSE),
	m_bNodeKeyCanBeDisabled		(TRUE),
	m_RootKeyDescription		(_T("ROOT")),
	m_pRootDetails				(NULL),
	m_sButton1					(_T("")),
	m_sButton2					(_T("")),
	m_DragDropAction			(DRAG_DROP_CANCEL)
{	
	m_Resource.SetUpperCase();

	SetFilterDefaultValue();

	CQtaFormatter* pQtyFmt = (CQtaFormatter*)AfxGetFormatStyleTable()->GetFormatter(DataType::Quantity, &GetNamespace());
	m_QuantityDecimalNumbers = (pQtyFmt) ? pQtyFmt->GetDecNumber() : 2;

	CMonFormatter* pMonFmt = (CMonFormatter*)AfxGetFormatStyleTable()->GetFormatter(DataType::Money, &GetNamespace());
	m_MoneyDecimalNumbers = (pMonFmt) ? pMonFmt->GetDecNumber() : 2;

	m_pRootDetails = new CRootDetails();

	// leggo i settings
	ResourcesLayoutSettings aSettings;
	SetShowDetails(aSettings.GetShowDetails());
	m_DisabledToo = aSettings.GetDisabledToo();

	CustomizeFilters(E_ACTIVITY_PANELACTION::ACTIVITY_COLLAPSE);
	SetViews		(RUNTIME_CLASS(CResourcesLayoutTreeVView), RUNTIME_CLASS(CResourcesLayoutDetailsView));

	RegisterControl (IDD_RESOURCES_LAYOUT_TREE_VIEW,	RUNTIME_CLASS(CResourcesLayoutTreeVView));
	RegisterControl	(IDD_RESOURCES_LAYOUT_ACTION_VIEW,	RUNTIME_CLASS(CResourcesLayoutActionSlaveView));
}

//-----------------------------------------------------------------------------
BDResourcesLayout::~BDResourcesLayout()
{
	// salvo i settings
	ResourcesLayoutSettings aSettings;
	aSettings.SetShowDetails(GetShowDetails());
	aSettings.SetDisabledToo(m_DisabledToo);
	aSettings.WriteParameters();

	SAFE_DELETE(m_pGecoder);
	SAFE_DELETE(m_pFunctions);
	SAFE_DELETE(m_pCheckResourcesRecursion);
	SAFE_DELETE(m_pRootDetails);
}

//---------------------------------------------------------------------------------
void BDResourcesLayout::OnAfterPrepareAuxData(CAbstractFormView* pView)
{
	if (!pView->IsKindOf(RUNTIME_CLASS(CResourcesLayoutTreeVView)))
		return;

	LoadStart();
}

//-----------------------------------------------------------------------------
BOOL BDResourcesLayout::OnAttachData()
{            
	__super::OnAttachData();

	SetFormTitle	(_TB("Resources Layout"));
	SetFormName		(_TB("Resources Layout"));
	
	m_bUserIsAdmin = AfxGetLoginInfos()->m_bAdmin;

	GetHotLink<HKLResources>(L"Resources")->SetCodeType(m_ResourceType);

	m_pCheckResourcesRecursion	= new CheckResourcesRecursion(this);
	m_pGecoder					= new CGeocoder();
	m_pFunctions				= new CResourcesFunctions(this);
	
	SetDefaultImage();

	DECLARE_VAR_JSON(AllResources);
	DECLARE_VAR_JSON(SelectResource);
	DECLARE_VAR_JSON(ResourceType);
	DECLARE_VAR_JSON(Resource);
	DECLARE_VAR_JSON(DisabledToo);
	DECLARE_VAR_JSON(TreeView);
	DECLARE_VAR_JSON(DetailImage);
	DECLARE_VAR_JSON(TileImageCaption);

	DECLARE_VAR_JSON(bUserIsAdmin);
	DECLARE_VAR_JSON(bNodeKeyIsEmpty);
	DECLARE_VAR_JSON(bNodeKeyCanBeDisabled);

	DECLARE_VAR_JSON(sActionMessage);
	DECLARE_VAR_JSON(sButton1);
	DECLARE_VAR_JSON(sButton2);

	return TRUE;
} 

//-----------------------------------------------------------------------------
void BDResourcesLayout::OnParsedControlCreated(CParsedCtrl* pCtrl)
{
	__super::OnParsedControlCreated(pCtrl);
	
	UINT nIDC = pCtrl->GetCtrlID();

	if (nIDC == IDC_RESOURCES_LAYOUT_PICTURE)
	{
		m_pResourcePicture = dynamic_cast<CResourcesPictureStatic*>(pCtrl);
		m_pResourcePicture->OnCtrlStyleBest();
	}
}

//-----------------------------------------------------------------------------
void BDResourcesLayout::DisableControlsForBatch()
{
	m_ResourceType	.SetReadOnly(m_AllResources);
	m_Resource		.SetReadOnly(m_AllResources);
}

// reimposta il default sui filtri
//------------------------------------------------------------------------------------------
void BDResourcesLayout::SetFilterDefaultValue()
{
	m_DisabledToo		= FALSE;
	m_AllResources		= TRUE;
	m_SelectResource	= FALSE;
	m_ResourceType		.Clear();
	m_Resource			.Clear();

	UpdateDataView();
}

//-----------------------------------------------------------------------------------------
void BDResourcesLayout::InitTree()
{
	SetNodeStateIcon(TRUE);
	
	AddImage(szCompanyImage,			AfxGetPathFinder()->GetFileNameFromNamespace(TBGlyph(szIconRoot),				AfxGetLoginInfos()->m_strUserName));
	AddImage(szResourceImage,			AfxGetPathFinder()->GetFileNameFromNamespace(TBGlyph(szIconResource),			AfxGetLoginInfos()->m_strUserName));
	AddImage(szWorkerImage,				AfxGetPathFinder()->GetFileNameFromNamespace(TBGlyph(szIconWorker),				AfxGetLoginInfos()->m_strUserName));
	AddImage(szCompanyImageDisabled,	AfxGetPathFinder()->GetFileNameFromNamespace(TBGlyph(szIconRootDisabled),		AfxGetLoginInfos()->m_strUserName));
	AddImage(szResourceImageDisabled,	AfxGetPathFinder()->GetFileNameFromNamespace(TBGlyph(szIconResourceDisabled),	AfxGetLoginInfos()->m_strUserName));
	AddImage(szWorkerImageDisabled,		AfxGetPathFinder()->GetFileNameFromNamespace(TBGlyph(szIconWorkerDisabled),		AfxGetLoginInfos()->m_strUserName));

	AddControls();
	SetAllowDrop(TRUE);
	SetViewContextMenu(TRUE);
}

// il Find e' sempre disabilitato
//-------------------------------------------------------------------------------------
BOOL BDResourcesLayout::DoEnableFind()
{
	return FALSE;
}

// ho deciso di togliere la possibilita' di nascondere i dettagli 
// emergeva il problema di referenziare lo splitter definito in JSON, al momento non gestito
//-------------------------------------------------------------------------------------
BOOL BDResourcesLayout::DoEnableDetails()
{
	return FALSE;
}

//-------------------------------------------------------------------------------------
BOOL BDResourcesLayout::OnFilterValidate()
{
	if (!m_AllResources && (m_ResourceType.IsEmpty() || m_Resource.IsEmpty()))
	{
		m_pMessages->Add(_TB("Missing resource type/code"));
		m_pMessages->Show();
		return FALSE;
	}

	return TRUE;
}

//--------------------------------------------------------------------------------
void BDResourcesLayout::OnResourceChanged()
{
	m_ResourceDisabled = GetDocument()->GetHotLink<HKLResources>(L"Resources")->GetRecord()->f_Disabled;
	UpdateDataView();
}

//--------------------------------------------------------------------------------
void BDResourcesLayout::OnResourceTypeChanged()
{
	DataStr& oldResourceType = (DataStr&)AfxGetBaseApp()->GetOldCtrlData();
	if (AfxGetBaseApp()->IsValidOldCtrlData() && oldResourceType == m_ResourceType)
		return;

	m_Resource.Clear();
	GetDocument()->GetHotLink<HKLResources>(L"Resources")->SetCodeType(m_ResourceType);
	UpdateDataView();
}

//--------------------------------------------------------------------------------
void BDResourcesLayout::OnSelectAllResourcesChanged()
{
	m_ResourceType.SetReadOnly(m_AllResources);
	m_Resource.SetReadOnly(m_AllResources);

	if (m_AllResources)
	{
		m_ResourceType.Clear();
		m_Resource.Clear();
	}

	UpdateDataView();
}

//-----------------------------------------------------------------------------
void BDResourcesLayout::DoDetailGridRowChanged()
{
	//TODO - RIMANE DA GESTIRE IN UN ALTRO MODO - LATO CLIENT!!!! (PIU' AVANTI)
}

//------------------------------------------------------------------------------------
BOOL BDResourcesLayout::PopulateTree()
{
	m_Index = 0;

	ClearTree();

	// inserisco un primo nodo fittizio per identificare la root
	CString aKey = GetResourceKey(m_Index, RESOURCE_ROOT, _T("Root"), _T("Company"));
	AddNode(m_RootKeyDescription, aKey, szCompanyImage);

	if (m_ResourceType.IsEmpty() && m_Resource.IsEmpty())
	{
		LoadAllResources(aKey);
		LoadAllWorkers(aKey);
	}
	else
	{
		aKey = GetResourceKey(m_Index, RESOURCE_RESOURCE, m_ResourceType, m_Resource);
		AddNode(GetResourceDescription(m_ResourceType, m_Resource), aKey, GetResourceImage(aKey, m_ResourceDisabled));
		LoadResourceChildren(m_ResourceType, m_Resource, m_Index);
	}

	SetNodeAsSelected(aKey);

	DoSelectionNodeChanged(aKey);

	ExpandAllFromSelectedNode();
	
	return TRUE;
}

//-------------------------------------------------------------------------------------------------------
void BDResourcesLayout::OnToolbarOpenDocument()
{
	GetNotValidView(TRUE);
	RunDocument(OPEN_DOCUMENT);
}

//-------------------------------------------------------------------------------------------------------
void BDResourcesLayout::OnToolbarNewResource()
{
	GetNotValidView(TRUE);
	RunDocument(NEW_RESOURCE);
}

//-------------------------------------------------------------------------------------------------------
void BDResourcesLayout::OnToolbarNewWorker()
{
	GetNotValidView(TRUE);
	RunDocument(NEW_WORKER);
}

//-------------------------------------------------------------------------------------------------------
void BDResourcesLayout::OnToolbarInternet()
{
	GetNotValidView(TRUE);
	OpenUrl();
}

//-------------------------------------------------------------------------------------------------------
void BDResourcesLayout::OnToolbarMail()
{
	GetNotValidView(TRUE);
	SendEMail();
}

//-------------------------------------------------------------------------------------------------------
void BDResourcesLayout::OnToolbarMap()
{
	GetNotValidView(TRUE);
	ShowMap();
}

//-------------------------------------------------------------------------------------------------------
void BDResourcesLayout::OnToolbarSatellite()
{
	GetNotValidView(TRUE);
	ShowMap(TRUE);
}

//-------------------------------------------------------------------------------------------------------
void BDResourcesLayout::OnToolbarDisable()
{
	GetNotValidView(TRUE);
	EnableDisable();
	LoadTree();
}

//----------------------------------------------------------------------------
void BDResourcesLayout::OnUpdateToolbarOpenDocument(CCmdUI* pCmdUI)
{
	pCmdUI->Enable(IsTreeViewLoaded());
}

//----------------------------------------------------------------------------
void BDResourcesLayout::OnUpdateToolbarNewResource(CCmdUI* pCmdUI)
{
	pCmdUI->Enable(IsTreeViewLoaded());
}

//----------------------------------------------------------------------------
void BDResourcesLayout::OnUpdateToolbarNewWorker(CCmdUI* pCmdUI)
{
	pCmdUI->Enable(IsTreeViewLoaded());
}

//----------------------------------------------------------------------------
void BDResourcesLayout::OnUpdateToolbarInternet(CCmdUI* pCmdUI)
{
	pCmdUI->Enable(IsTreeViewLoaded() && !m_DetailWeb.IsEmpty());
}

//----------------------------------------------------------------------------
void BDResourcesLayout::OnUpdateToolbarMail(CCmdUI* pCmdUI)
{
	pCmdUI->Enable(IsTreeViewLoaded() && !m_DetailEmail.IsEmpty());
}

//----------------------------------------------------------------------------
void BDResourcesLayout::OnUpdateToolbarMap(CCmdUI* pCmdUI)
{
	pCmdUI->Enable	(IsTreeViewLoaded() && (!m_DetailAddress.IsEmpty() ||
					(!m_DetailLatitude.IsEmpty() && !m_DetailLongitude.IsEmpty())));
}

//----------------------------------------------------------------------------
void BDResourcesLayout::OnUpdateToolbarSatellite(CCmdUI* pCmdUI)
{
	pCmdUI->Enable	(IsTreeViewLoaded() && (!m_DetailAddress.IsEmpty() ||
					(!m_DetailLatitude.IsEmpty() && !m_DetailLongitude.IsEmpty())));
}

//----------------------------------------------------------------------------
void BDResourcesLayout::OnUpdateToolbarDisable(CCmdUI* pCmdUI)
{
	BOOL bOK		= FALSE;
	CString aKey	= GetSelectedNodeKey();

	if (!aKey.IsEmpty())
	{
		long aKeyType = _wtol(aKey.Mid(6, 1));
		if (aKeyType > 1) // posso disabilitare solo le risorse e i worker
			bOK = TRUE; 
	}
	pCmdUI->Enable(bOK);
}

//----------------------------------------------------------------------------
void BDResourcesLayout::ShowRootDetails()
{
	if (!m_pRootDetails)
		return;

	m_ResourceTypeId	= RESOURCE_ROOT;
	
	m_DetailEmail		= m_pRootDetails->m_Email;
	m_DetailWeb			= m_pRootDetails->m_InternetAddress;
	m_DetailAddress		= m_pRootDetails->m_Address;
	m_DetailCity		= m_pRootDetails->m_City;
	m_DetailCounty		= m_pRootDetails->m_County;
	m_DetailCountry		= m_pRootDetails->m_Status;
	m_DetailZip			= m_pRootDetails->m_ZipCode; 
	m_DetailImage		.Clear();
	m_DetailLongitude	.Clear();
	m_DetailLatitude	.Clear();
	m_DetailName		= m_pRootDetails->m_CompanyName;

	SetDefaultImage();

	ADD_SEPARATOR_LAYOUT	(COMPANY);
	ADD_DETAIL_LAYOUT		(TITLE,			m_pRootDetails->m_TitleCode,			FALSE);
	ADD_DETAIL_LAYOUT		(NAME,			m_pRootDetails->m_CompanyName,			FALSE);
	ADD_SEPARATOR_LAYOUT	(MASTER_DATA);
	ADD_DETAIL_LAYOUT		(ADDRESS,		m_pRootDetails->m_Address,				TRUE);
	ADD_DETAIL_LAYOUT		(CITY,			m_pRootDetails->m_City,					FALSE);
	ADD_DETAIL_LAYOUT		(COUNTY,		m_pRootDetails->m_County,				FALSE);
	ADD_DETAIL_LAYOUT		(ZIP_CODE,		m_pRootDetails->m_ZipCode,				FALSE);
	ADD_DETAIL_LAYOUT		(COUNTRY,		m_pRootDetails->m_Status,				FALSE);
	ADD_SEPARATOR_LAYOUT	(TELEPHONE);
	ADD_DETAIL_LAYOUT		(TELEPHONE_1,	m_pRootDetails->m_Telephone1,			FALSE);
	ADD_DETAIL_LAYOUT		(TELEPHONE_2,	m_pRootDetails->m_Telephone2,			FALSE);
	ADD_DETAIL_LAYOUT		(FAX,			m_pRootDetails->m_Fax,					FALSE);
	ADD_DETAIL_LAYOUT		(TELEX,			m_pRootDetails->m_Telex,				FALSE);
	ADD_SEPARATOR_LAYOUT	(OTHER_DATA);
	ADD_DETAIL_LAYOUT		(WEB,			m_pRootDetails->m_InternetAddress,		TRUE);
	ADD_DETAIL_LAYOUT		(EMAIL,			m_pRootDetails->m_Email,				TRUE);

	if (m_pResourcePicture) m_pResourcePicture->SetRoot();

	UpdateDataView();
}

//----------------------------------------------------------------------------
void BDResourcesLayout::ShowResourceDetails(const DataStr& aResourceType, const DataStr& aResource)
{
	m_TRResources.	SetForceQuery();
	m_TRWorkers.	SetForceQuery();

	m_TRResources.	FindRecord(aResource, aResourceType);
	m_TRWorkers.	FindRecord(m_TRResources.GetRecord()->f_Manager);

	m_ResourceTypeId		= RESOURCE_RESOURCE;
	m_DetailResourceType	= m_TRResources.GetRecord()->f_ResourceType;
	m_DetailEmail			= m_TRResources.GetRecord()->f_Email;
	m_DetailWeb				= m_TRResources.GetRecord()->f_URL;
	m_DetailAddress			= m_TRResources.GetRecord()->f_DomicilyAddress,
	m_DetailCity			= m_TRResources.GetRecord()->f_DomicilyCity,
	m_DetailCounty			= m_TRResources.GetRecord()->f_DomicilyCounty,
	m_DetailCountry			= m_TRResources.GetRecord()->f_DomicilyCountry,
	m_DetailZip				= m_TRResources.GetRecord()->f_DomicilyZip,
	m_DetailImage			= m_TRResources.GetRecord()->f_ImagePath;
	m_DetailLongitude		= m_TRResources.GetRecord()->f_Longitude;
	m_DetailLatitude		= m_TRResources.GetRecord()->f_Latitude;
	m_DetailName			= m_TRResources.GetRecord()->f_Description;

	SetDefaultImage();

	ADD_SEPARATOR_LAYOUT	(RESOURCE);
	ADD_DETAIL_LAYOUT		(TYPE,			m_TRResources.GetRecord()->f_ResourceType,		FALSE);
	ADD_DETAIL_LAYOUT		(RESOURCE_CODE,	m_TRResources.GetRecord()->f_ResourceCode,		TRUE);
	ADD_DETAIL_LAYOUT		(DESCRIPTION,	m_TRResources.GetRecord()->f_Description,		FALSE);
	if (m_TRResources.GetRecord()->f_Manager.IsEmpty())
	{
		ADD_DETAIL_LAYOUT	(MANAGER,		DataStr(_T("")),								FALSE);
	}
	else
	{
		ADD_DETAIL_LAYOUT	(MANAGER,		m_TRResources.GetRecord()->f_Manager,			TRUE);
	}
	ADD_DETAIL_LAYOUT		(NAME,			m_TRWorkers.GetWorker(),						FALSE);
	ADD_DETAIL_LAYOUT		(DISABLED,		m_TRResources.GetRecord()->f_Disabled,			FALSE);
	ADD_SEPARATOR_LAYOUT	(MASTER_DATA);
	ADD_DETAIL_LAYOUT		(ADDRESS,		m_TRResources.GetRecord()->f_DomicilyAddress,	TRUE);
	ADD_DETAIL_LAYOUT		(CITY,			m_TRResources.GetRecord()->f_DomicilyCity,		FALSE);
	ADD_DETAIL_LAYOUT		(COUNTY,		m_TRResources.GetRecord()->f_DomicilyCounty,	FALSE);
	ADD_DETAIL_LAYOUT		(ZIP_CODE,		m_TRResources.GetRecord()->f_DomicilyZip,		FALSE);
	ADD_DETAIL_LAYOUT		(COUNTRY,		m_TRResources.GetRecord()->f_DomicilyCountry,	FALSE);
	ADD_DETAIL_LAYOUT		(NOTES,			m_TRResources.GetRecord()->f_Notes,				FALSE);
	ADD_SEPARATOR_LAYOUT	(TELEPHONE);
	ADD_DETAIL_LAYOUT		(TELEPHONE_1,	m_TRResources.GetRecord()->f_Telephone1,		FALSE);
	ADD_DETAIL_LAYOUT		(TELEPHONE_2,	m_TRResources.GetRecord()->f_Telephone2,		FALSE);
	ADD_DETAIL_LAYOUT		(FAX,			m_TRResources.GetRecord()->f_Telephone3,		FALSE);
	ADD_DETAIL_LAYOUT		(OTHER,			m_TRResources.GetRecord()->f_Telephone4,		FALSE);
	ADD_SEPARATOR_LAYOUT	(OTHER_DATA);
	ADD_DETAIL_LAYOUT		(WEB,			m_TRResources.GetRecord()->f_URL,				TRUE);
	ADD_DETAIL_LAYOUT		(EMAIL,			m_TRResources.GetRecord()->f_Email,				TRUE);
	ADD_DETAIL_LAYOUT		(SKYPE,			m_TRResources.GetRecord()->f_SkypeID,			FALSE);

	LoadResourceCustomData(aResourceType, aResource);

	m_pResourcePicture->SetResource(aResourceType, aResource);

	UpdateDataView();
}

//----------------------------------------------------------------------------
void BDResourcesLayout::LoadResourceCustomData(const DataStr& aResourceType, const DataStr& aResource)
{
	TResourcesFields	aRec;
	SqlTable			aTable(&aRec, GetReadOnlySqlSession());

	TRY
	{
		aTable.Open();
		aTable.Select(aRec.f_FieldName);
		aTable.Select(aRec.f_FieldValue);

		aTable.AddParam			(szParamResourceCode4, aRec.f_ResourceCode);
		aTable.AddFilterColumn	(aRec.f_ResourceCode);
		aTable.SetParamValue	(szParamResourceCode4, aResource);

		aTable.AddParam			(szParamResourceType4, aRec.f_ResourceType);
		aTable.AddFilterColumn	(aRec.f_ResourceType);
		aTable.SetParamValue	(szParamResourceType4, aResourceType);

		aTable.AddParam			(szParamResourceHide4, aRec.f_HideOnLayout);
		aTable.AddFilterColumn	(aRec.f_HideOnLayout);
		aTable.SetParamValue	(szParamResourceHide4, DataBool(FALSE));

		aTable.AddSortColumn(aRec.f_FieldName);

		aTable.Query();

		if (!aTable.IsEmpty())
			ADD_SEPARATOR_LAYOUT(CUSTOM_DATA);

		while(!aTable.IsEOF())
		{
			AddDetail(aRec.f_FieldName, aRec.f_FieldValue, FALSE);
			aTable.MoveNext();
		}
		aTable.Close();
	}
	CATCH(SqlException, e)
	{
		aTable.Close();
		e->ShowError();
	}
	END_CATCH
}

//----------------------------------------------------------------------------
void BDResourcesLayout::ShowWorkerDetails(DataLng& aWorker)
{
	m_TRWorkers.SetForceQuery();
	m_TRWorkers.FindRecord(aWorker);

	m_ResourceTypeId		= RESOURCE_WORKER;
	m_DetailEmail			= m_TRWorkers.GetRecord()->f_Email;
	m_DetailWeb				= m_TRWorkers.GetRecord()->f_URL;
	m_DetailAddress			= m_TRWorkers.GetRecord()->f_DomicilyAddress,
	m_DetailCity			= m_TRWorkers.GetRecord()->f_DomicilyCity,
	m_DetailCounty			= m_TRWorkers.GetRecord()->f_DomicilyCounty,
	m_DetailCountry			= m_TRWorkers.GetRecord()->f_DomicilyCountry,
	m_DetailZip				= m_TRWorkers.GetRecord()->f_DomicilyZip,
	m_DetailImage			= m_TRWorkers.GetRecord()->f_ImagePath;
	m_DetailLongitude		= m_TRWorkers.GetRecord()->f_Longitude;
	m_DetailLatitude		= m_TRWorkers.GetRecord()->f_Latitude;
	m_DetailName			= m_TRWorkers.GetWorker();

	SetDefaultImage();

	ADD_SEPARATOR_LAYOUT	(WORKER);
	ADD_DETAIL_LAYOUT		(WORKER_ID,		m_TRWorkers.GetRecord()->f_WorkerID,			TRUE);
	ADD_DETAIL_LAYOUT		(TITLE,			m_TRWorkers.GetRecord()->f_Title,				FALSE);
	ADD_DETAIL_LAYOUT		(NAME,			m_TRWorkers.GetRecord()->f_Name,				FALSE);
	ADD_DETAIL_LAYOUT		(LAST_NAME,		m_TRWorkers.GetRecord()->f_LastName,			FALSE);
	ADD_DETAIL_LAYOUT		(DISABLED,		m_TRWorkers.GetRecord()->f_Disabled,			FALSE);
	ADD_SEPARATOR_LAYOUT	(MASTER_DATA);
	ADD_DETAIL_LAYOUT		(ADDRESS,		m_TRWorkers.GetRecord()->f_DomicilyAddress,		TRUE);
	ADD_DETAIL_LAYOUT		(CITY,			m_TRWorkers.GetRecord()->f_DomicilyCity,		FALSE);
	ADD_DETAIL_LAYOUT		(COUNTY,		m_TRWorkers.GetRecord()->f_DomicilyCounty,		FALSE);
	ADD_DETAIL_LAYOUT		(ZIP_CODE,		m_TRWorkers.GetRecord()->f_DomicilyZip,			FALSE);
	ADD_DETAIL_LAYOUT		(COUNTRY,		m_TRWorkers.GetRecord()->f_DomicilyCountry,		FALSE);
	ADD_DETAIL_LAYOUT		(NOTES,			m_TRWorkers.GetRecord()->f_Notes,				FALSE);
	ADD_SEPARATOR_LAYOUT	(TELEPHONE);
	ADD_DETAIL_LAYOUT		(OFFICE,		m_TRWorkers.GetRecord()->f_Telephone1,			FALSE);
	ADD_DETAIL_LAYOUT		(MOBILE,		m_TRWorkers.GetRecord()->f_Telephone2,			FALSE);
	ADD_DETAIL_LAYOUT		(FAX,			m_TRWorkers.GetRecord()->f_Telephone3,			FALSE);
	ADD_DETAIL_LAYOUT		(HOME,			m_TRWorkers.GetRecord()->f_Telephone4,			FALSE);
	ADD_SEPARATOR_LAYOUT	(OTHER_DATA);
	if (m_bUserIsAdmin)
	{
		ADD_DETAIL_LAYOUT	(BIRTH_DATE,	m_TRWorkers.GetRecord()->f_DateOfBirth,			FALSE);
		ADD_DETAIL_LAYOUT	(BIRTH_CITY,	m_TRWorkers.GetRecord()->f_CityOfBirth,			FALSE);
		ADD_DETAIL_LAYOUT	(CIVIL_STATUS,	m_TRWorkers.GetRecord()->f_CivilStatus,			FALSE);
		ADD_DETAIL_LAYOUT	(GENDER,		m_TRWorkers.GetRecord()->f_Gender,				FALSE);
		ADD_DETAIL_LAYOUT	(FISCAL_CODE,	m_TRWorkers.GetRecord()->f_DomicilyFC,			FALSE);
		ADD_DETAIL_LAYOUT	(REGISTER,		m_TRWorkers.GetRecord()->f_RegisterNumber,		FALSE);
		ADD_DETAIL_LAYOUT	(EMPLOYMENT,	m_TRWorkers.GetRecord()->f_EmploymentDate,		FALSE);
		ADD_DETAIL_LAYOUT	(RESIGNATION,	m_TRWorkers.GetRecord()->f_ResignationDate,		FALSE);
		ADD_SEPARATOR_LAYOUT(EMPTY);
	}
	ADD_DETAIL_LAYOUT		(WEB,			m_TRWorkers.GetRecord()->f_URL,					TRUE);
	ADD_DETAIL_LAYOUT		(EMAIL,			m_TRWorkers.GetRecord()->f_Email,				TRUE);
	ADD_DETAIL_LAYOUT		(SKYPE,			m_TRWorkers.GetRecord()->f_SkypeID,				FALSE);

	LoadWorkerCustomData(aWorker);

	m_pResourcePicture->SetWorker(aWorker);

	UpdateDataView();
}

//----------------------------------------------------------------------------
void BDResourcesLayout::LoadWorkerCustomData(DataLng& aWorker)
{
	TWorkersFields	aRec;
	SqlTable		aTable(&aRec, GetReadOnlySqlSession());

	TRY
	{
		aTable.Open();
		aTable.Select(aRec.f_FieldName);
		aTable.Select(aRec.f_FieldValue);

		aTable.AddParam			(szParamWorker3, aRec.f_WorkerID);
		aTable.AddFilterColumn	(aRec.f_WorkerID);
		aTable.SetParamValue	(szParamWorker3, aWorker);

		aTable.AddParam			(szParamResourceHide3, aRec.f_HideOnLayout);
		aTable.AddFilterColumn	(aRec.f_HideOnLayout);
		aTable.SetParamValue	(szParamResourceHide3, DataBool(FALSE));

		aTable.AddSortColumn(aRec.f_FieldName);

		aTable.Query();

		if (!aTable.IsEmpty())
			ADD_SEPARATOR_LAYOUT(CUSTOM_DATA);

		while(!aTable.IsEOF())
		{
			AddDetail(aRec.f_FieldName, aRec.f_FieldValue, FALSE);
			aTable.MoveNext();
		}
		aTable.Close();
	}
	CATCH(SqlException, e)
	{
		aTable.Close();
		e->ShowError();
	}
	END_CATCH
}

//----------------------------------------------------------------------------
void BDResourcesLayout::ShowEmptyDetails()
{
	__super::ShowEmptyDetails();

	m_ResourceTypeId		= RESOURCE_DEFAULT;
	m_DetailEmail.			Clear();
	m_DetailWeb.			Clear();
	m_DetailAddress.		Clear();
	m_DetailCity.			Clear();
	m_DetailCounty.			Clear();
	m_DetailCountry.		Clear();
	m_DetailZip.			Clear();
	m_DetailImage.			Clear();
	m_DetailLongitude.		Clear();
	m_DetailLatitude.		Clear();
	m_DetailName.			Clear();

	UpdateDataView();
}

// Carico tutte le risorse che non hanno padri
/*SELECT RM_Resources.ResourceType,RM_Resources.ResourceCode,RM_Resources.Disabled
FROM RM_Resources WHERE RM_Resources.HideOnLayout = 0 AND RM_Resources.Disabled = 0 AND RM_Resources.ResourceCode
NOT IN (SELECT RM_ResourcesDetails.ChildResourceCode
FROM RM_ResourcesDetails WHERE RM_ResourcesDetails.ChildResourceType = RM_Resources.ResourceType
AND RM_ResourcesDetails.ChildResourceCode = RM_Resources.ResourceCode) AND RM_Resources.ResourceCode
NOT IN (SELECT RM_WorkersDetails.ChildResourceCode FROM RM_WorkersDetails WHERE
RM_WorkersDetails.ChildResourceType = RM_Resources.ResourceType
AND RM_WorkersDetails.ChildResourceCode = RM_Resources.ResourceCode) ORDER BY RM_Resources.ResourceCode*/
//-----------------------------------------------------------------------------
void BDResourcesLayout::LoadAllResources(CString aKey)
{
	TResources			aRec;
	TResourcesDetails	aRecDetail;
	TWorkersDetails		aRecWorkersDetail;

	SqlTable aTable(&aRec, GetReadOnlySqlSession());

	aRec.		SetQualifier();
	aRecDetail.	SetQualifier();

	TRY
	{
		aTable.Open();

		aTable.Select(aRec.f_ResourceType);
		aTable.Select(aRec.f_ResourceCode);
		aTable.Select(aRec.f_Disabled);
		aTable.AddSortColumn(aRec.f_ResourceCode);

		aTable.AddParam				(szParamResourceHide,		aRec.f_HideOnLayout);
		aTable.AddFilterColumn		(aRec.f_HideOnLayout);
		aTable.SetParamValue		(szParamResourceHide,		DataBool(FALSE));

		if (!m_DisabledToo)
		{
			aTable.AddParam			(szParamResourceDisabled,	aRec.f_Disabled);
			aTable.AddFilterColumn	(aRec.f_Disabled);
			aTable.SetParamValue	(szParamResourceDisabled,	DataBool(FALSE));
		}

		aTable.m_strFilter += cwsprintf
		(
			_T(" AND %s NOT IN (SELECT %s FROM %s WHERE %s = %s AND %s = %s)"),
			(LPCTSTR)aRec.		GetQualifiedColumnName(&aRec.f_ResourceCode),
			(LPCTSTR)aRecDetail.GetQualifiedColumnName(&aRecDetail.f_ChildResourceCode),
			(LPCTSTR)aRecDetail.GetTableName(),
			(LPCTSTR)aRecDetail.GetQualifiedColumnName(&aRecDetail.f_ChildResourceType),
			(LPCTSTR)aRec.		GetQualifiedColumnName(&aRec.f_ResourceType),
			(LPCTSTR)aRecDetail.GetQualifiedColumnName(&aRecDetail.f_ChildResourceCode),
			(LPCTSTR)aRec.		GetQualifiedColumnName(&aRec.f_ResourceCode)
		);

		aTable.m_strFilter += cwsprintf
			(
			_T(" AND %s NOT IN (SELECT %s FROM %s WHERE %s = %s AND %s = %s)"),
			(LPCTSTR)aRec.GetQualifiedColumnName(&aRec.f_ResourceCode),
			(LPCTSTR)aRecWorkersDetail.GetQualifiedColumnName(&aRecWorkersDetail.f_ChildResourceCode),
			(LPCTSTR)aRecWorkersDetail.GetTableName(),
			(LPCTSTR)aRecWorkersDetail.GetQualifiedColumnName(&aRecWorkersDetail.f_ChildResourceType),
			(LPCTSTR)aRec.GetQualifiedColumnName(&aRec.f_ResourceType),
			(LPCTSTR)aRecWorkersDetail.GetQualifiedColumnName(&aRecWorkersDetail.f_ChildResourceCode),
			(LPCTSTR)aRec.GetQualifiedColumnName(&aRec.f_ResourceCode)
			);

		aTable.Query();

		while(!aTable.IsEOF())
		{
			CString sCurrentKey = GetResourceKey(++m_Index, RESOURCE_RESOURCE, aRec.f_ResourceType, aRec.f_ResourceCode);
			InsertChild
				(
				aKey, 
				GetResourceDescription(aRec.f_ResourceType, aRec.f_ResourceCode),
				sCurrentKey, 
				GetResourceImage(sCurrentKey, aRec.f_Disabled)
				);
			LoadResourceChildren(aRec.f_ResourceType, aRec.f_ResourceCode, m_Index);
			aTable.MoveNext();
		}
		aTable.Close();
	}
	CATCH(SqlException, e)
	{
		aTable.Close();
		e->ShowError();
	}
	END_CATCH
}

// Carico tutte le matricole che non hanno padri
/*SELECT RM_Workers.WorkerID, RM_Workers.Disabled
FROM RM_Workers WHERE RM_Workers.HideOnLayout = 0 AND RM_Workers.Disabled = 0 AND RM_Workers.WorkerID NOT IN 
(SELECT RM_ResourcesDetails.ChildWorkerID FROM RM_ResourcesDetails WHERE
RM_ResourcesDetails.IsWorker = 1 AND RM_ResourcesDetails.ChildWorkerID = RM_Workers.WorkerID) AND RM_Workers.WorkerID NOT IN 
(SELECT RM_WorkersDetails.ChildWorkerID FROM RM_WorkersDetails WHERE RM_WorkersDetails.IsWorker = 1 
AND RM_WorkersDetails.ChildWorkerID = RM_Workers.WorkerID) ORDER BY RM_Workers.LastName*/
//-----------------------------------------------------------------------------
void BDResourcesLayout::LoadAllWorkers(CString aKey)
{
	TWorkers			aRec;
	TResourcesDetails	aRecDetail;
	TWorkersDetails		aRecWorkersDetail;

	SqlTable aTable(&aRec, GetReadOnlySqlSession());

	aRec.		SetQualifier();
	aRecDetail.	SetQualifier();

	TRY
	{
		aTable.Open();
		aTable.Select(aRec.f_WorkerID);
		aTable.Select(aRec.f_Disabled);
		aTable.AddSortColumn(aRec.f_LastName);

		aTable.AddParam				(szParamResourceHide2, aRec.f_HideOnLayout);
		aTable.SetParamValue		(szParamResourceHide2, DataBool(FALSE));
		aTable.AddFilterColumn		(aRec.f_HideOnLayout);

		if (!m_DisabledToo)
		{
			aTable.AddParam			(szParamResourceDisabled2,	aRec.f_Disabled);
			aTable.AddFilterColumn	(aRec.f_Disabled);
			aTable.SetParamValue	(szParamResourceDisabled2,	DataBool(FALSE));
		}

		aTable.AddParam				(szParamIsWorker, aRecDetail.f_IsWorker);
		aTable.m_strFilter += cwsprintf
		(
			_T(" AND %s NOT IN (SELECT %s FROM %s WHERE %s = ? AND %s = %s)"),
			(LPCTSTR)aRec.		GetQualifiedColumnName(&aRec.f_WorkerID),
			(LPCTSTR)aRecDetail.GetQualifiedColumnName(&aRecDetail.f_ChildWorkerID),
			(LPCTSTR)aRecDetail.GetTableName(),
			(LPCTSTR)aRecDetail.GetQualifiedColumnName(&aRecDetail.f_IsWorker),
			(LPCTSTR)aRecDetail.GetQualifiedColumnName(&aRecDetail.f_ChildWorkerID),
			(LPCTSTR)aRec.		GetQualifiedColumnName(&aRec.f_WorkerID)
		);
		aTable.SetParamValue		(szParamIsWorker, DataBool(TRUE));

		aTable.AddParam				(szParamIsWorker2, aRecWorkersDetail.f_IsWorker);
		aTable.m_strFilter += cwsprintf
			(
			_T(" AND %s NOT IN (SELECT %s FROM %s WHERE %s = ? AND %s = %s)"),
			(LPCTSTR)aRec.GetQualifiedColumnName(&aRec.f_WorkerID),
			(LPCTSTR)aRecWorkersDetail.GetQualifiedColumnName(&aRecWorkersDetail.f_ChildWorkerID),
			(LPCTSTR)aRecWorkersDetail.GetTableName(),
			(LPCTSTR)aRecWorkersDetail.GetQualifiedColumnName(&aRecWorkersDetail.f_IsWorker),
			(LPCTSTR)aRecWorkersDetail.GetQualifiedColumnName(&aRecWorkersDetail.f_ChildWorkerID),
			(LPCTSTR)aRec.GetQualifiedColumnName(&aRec.f_WorkerID)
			);
		aTable.SetParamValue		(szParamIsWorker2, DataBool(TRUE));

		aTable.Query();

		while(!aTable.IsEOF())
		{
			CString sCurrentKey = GetResourceKey(++m_Index, RESOURCE_WORKER, _T(""), aRec.f_WorkerID.Str());
			InsertChild
				(
				aKey, 
				GetWorkerDescription(aRec.f_WorkerID), 
				sCurrentKey, 
				GetResourceImage(sCurrentKey, aRec.f_Disabled)
				);

			LoadWorkerChildren(aRec.f_WorkerID, m_Index);
			aTable.MoveNext();
		}
		aTable.Close();
	}
	CATCH(SqlException, e)
	{
		aTable.Close();
		e->ShowError();
	}
	END_CATCH
}

// metodo ricorsivo che va a ricercare i figli di una matricola
//-----------------------------------------------------------------------------
void BDResourcesLayout::LoadWorkerChildren(const DataLng& aWorkerID, int aParent)
{
	TWorkersDetails aRec;
	SqlTable aTable(&aRec, GetReadOnlySqlSession());

	TRY
	{
		aTable.Open();
		aTable.SelectAll();

		aTable.AddParam(szParamResource, aRec.f_WorkerID);
		aTable.AddFilterColumn(aRec.f_WorkerID);
		aTable.SetParamValue(szParamResource, aWorkerID);

		aTable.AddSortColumn(aRec.f_ChildResourceType);
		aTable.AddSortColumn(aRec.f_ChildResourceCode);
		aTable.AddSortColumn(aRec.f_ChildWorkerID);

		aTable.Query();

		while (!aTable.IsEOF())
		{
			CString sCurrentKey = _T("");

			if (aRec.f_IsWorker)
			{
				m_TRWorkers.FindRecord(aRec.f_ChildWorkerID);

				if (!m_TRWorkers.GetRecord()->f_HideOnLayout && (m_DisabledToo || !m_TRWorkers.GetRecord()->f_Disabled))
				{
					sCurrentKey = GetResourceKey(++m_Index, RESOURCE_WORKER, _T(""), aRec.f_ChildWorkerID.Str());

					InsertChild
						(
						GetResourceKey(aParent, RESOURCE_WORKER, _T(""), aWorkerID.Str()),
						GetWorkerDescription(aRec.f_ChildWorkerID),
						sCurrentKey,
						GetResourceImage(sCurrentKey, m_TRWorkers.GetRecord()->f_Disabled)
						);
				}
				LoadWorkerChildren(aRec.f_ChildWorkerID, m_Index); // vado in ricorsione a ricercare ulteriori figli
			}
			else
			{
				m_TRResources.FindRecord(aRec.f_ChildResourceCode, aRec.f_ChildResourceType);

				if (!m_TRResources.GetRecord()->f_HideOnLayout && (m_DisabledToo || !m_TRResources.GetRecord()->f_Disabled))
				{
					sCurrentKey = GetResourceKey(++m_Index, RESOURCE_RESOURCE, aRec.f_ChildResourceType, aRec.f_ChildResourceCode);

					InsertChild
						(
						GetResourceKey(aParent, RESOURCE_WORKER, _T(""), aWorkerID.Str()),
						GetResourceDescription(aRec.f_ChildResourceType, aRec.f_ChildResourceCode),
						sCurrentKey,
						GetResourceImage(sCurrentKey, m_TRResources.GetRecord()->f_Disabled)
						);
					LoadResourceChildren(aRec.f_ChildResourceType, aRec.f_ChildResourceCode, m_Index); // vado in ricorsione a ricercare ulteriori figli
				}
			}
			aTable.MoveNext();
		}
		aTable.Close();
	}
	CATCH(SqlException, e)
	{
		aTable.Close();
		e->ShowError();
	}
	END_CATCH
}

// metodo ricorsivo che va a ricercare i figli di una risorsa
//-----------------------------------------------------------------------------
void BDResourcesLayout::LoadResourceChildren(const DataStr& aResourceType, const DataStr& aResource, int aParent)
{
	TResourcesDetails aRec;
	SqlTable aTable(&aRec, GetReadOnlySqlSession());

	TRY
	{
		aTable.Open();
		aTable.SelectAll();

		aTable.AddParam			(szParamResourceType,	aRec.f_ResourceType);
		aTable.AddFilterColumn	(aRec.f_ResourceType);
		aTable.SetParamValue	(szParamResourceType,	aResourceType);
		
		aTable.AddParam			(szParamResource,		aRec.f_ResourceCode);
		aTable.AddFilterColumn	(aRec.f_ResourceCode);
		aTable.SetParamValue	(szParamResource,		aResource);
	
		aTable.AddSortColumn(aRec.f_ChildResourceCode);
		aTable.AddSortColumn(aRec.f_ChildWorkerID);

		aTable.Query();

		while(!aTable.IsEOF())
		{
			CString sCurrentKey = _T("");
			if (aRec.f_IsWorker)
			{
				m_TRWorkers.FindRecord(aRec.f_ChildWorkerID);

				if (!m_TRWorkers.GetRecord()->f_HideOnLayout && (m_DisabledToo || !m_TRWorkers.GetRecord()->f_Disabled))
				{
					sCurrentKey = GetResourceKey(++m_Index, RESOURCE_WORKER, _T(""), aRec.f_ChildWorkerID.Str());
					InsertChild
						(
						GetResourceKey(aParent, RESOURCE_RESOURCE, aResourceType, aResource),
						GetWorkerDescription(aRec.f_ChildWorkerID),
						sCurrentKey,
						GetResourceImage(sCurrentKey, m_TRWorkers.GetRecord()->f_Disabled)
						);
				}
				LoadWorkerChildren(aRec.f_ChildWorkerID, m_Index);  // vado in ricorsione a ricercare ulteriori figli
			}
			else
			{
				m_TRResources.FindRecord(aRec.f_ChildResourceCode, aRec.f_ChildResourceType);

				if (!m_TRResources.GetRecord()->f_HideOnLayout && (m_DisabledToo || !m_TRResources.GetRecord()->f_Disabled))
				{
					sCurrentKey = GetResourceKey(++m_Index, RESOURCE_RESOURCE, aRec.f_ChildResourceType, aRec.f_ChildResourceCode);
					InsertChild
						(
						GetResourceKey(aParent, RESOURCE_RESOURCE, aResourceType, aResource), 
						GetResourceDescription(aRec.f_ChildResourceType, aRec.f_ChildResourceCode),
						sCurrentKey, 
						GetResourceImage(sCurrentKey, m_TRResources.GetRecord()->f_Disabled)
						);
					LoadResourceChildren(aRec.f_ChildResourceType, aRec.f_ChildResourceCode, m_Index);  // vado in ricorsione a ricercare ulteriori figli
				}
			}
			aTable.MoveNext();
		}
		aTable.Close();
	}
	CATCH(SqlException, e)
	{
		aTable.Close();
		e->ShowError();
	}
	END_CATCH
}

//------------------------------------------------------------------------------------
void BDResourcesLayout::InitCtxMenuIndexes()
{
	CONTEXT_MENU_EXPAND_COLLAPSE		= 0;
	CONTEXT_MENU_ENABLE_DISABLE			= 0;
	CONTEXT_MENU_OPEN_DOCUMENT			= 0;
	CONTEXT_MENU_NEW_RESOURCE			= 0;
	CONTEXT_MENU_NEW_WORKER				= 0;
	CONTEXT_MENU_OPEN_WEBSITE			= 0;
	CONTEXT_MENU_SEND_EMAIL				= 0;
	CONTEXT_MENU_SHOW_MAP				= 0;
	CONTEXT_MENU_SHOW_SATELLITE_VIEW	= 0;
}

//------------------------------------------------------------------------------
void BDResourcesLayout::DoSelectionNodeChanged(CString aKey)
{
	// vado ad estrarre il chr nr. 7 per capire di che tipo risorsa si tratta
	// variabili JSON per pilotare lo show/hide dei menuitem del pulsante Actions della toolbar. NON VA TOCCATA!
	long kType				= _wtol(aKey.Mid(6, 1)); 
	m_bNodeKeyIsEmpty		= (kType == 0);
	m_bNodeKeyCanBeDisabled = (kType > 1);
	//

	long aKeyType = 0;
	CString aResourceType;
	CString aResourceCode;
	DataLng aWorkerID;

	InitCtxMenuIndexes();

	ExtractResourceInfoFromKey(aKey, aKeyType, aResourceType, aResourceCode, aWorkerID);

	if (aKeyType == 0) // nodo empty
	{
		AddContextMenuItem(_TB("Expand/Collapse"));
		ShowEmptyDetails();
		return;
	}

	switch (aKeyType)
	{
	case 1: // nodo tipo root 
		ShowRootDetails();
		break;
	case 2: // nodo tipo risorsa
		ShowResourceDetails(aResourceType, aResourceCode);
		break;
	case 3: // nodo tipo worker
		ShowWorkerDetails(aWorkerID);
		break;
	default:
		ShowEmptyDetails();
		break;
	}

	int aMenuPosition = 0;

	aMenuPosition++;
	CONTEXT_MENU_OPEN_DOCUMENT = aMenuPosition;
	AddContextMenuItem(_TB("Open"));

	if (m_bUserIsAdmin)
	{
		aMenuPosition++;
		CONTEXT_MENU_NEW_RESOURCE = aMenuPosition;
		AddContextMenuItem(_TB("New resource"));

		aMenuPosition++;
		CONTEXT_MENU_NEW_WORKER = aMenuPosition;
		AddContextMenuItem(_TB("New worker"));

		if (aKeyType > 1) // funzionalita' solo per risorse e workers
		{
			aMenuPosition++;
			CONTEXT_MENU_ENABLE_DISABLE = aMenuPosition;
			AddContextMenuItem(_TB("Enable/Disable"));
		}
	}

	if (!m_DetailWeb.IsEmpty() || !m_DetailEmail.IsEmpty() || !m_DetailAddress.IsEmpty() || (!m_DetailLatitude.IsEmpty() && !m_DetailLongitude.IsEmpty()))
	{
		aMenuPosition++;
		AddContextMenuSeparator();

		if (!m_DetailWeb.IsEmpty())
		{
			aMenuPosition++;
			CONTEXT_MENU_OPEN_WEBSITE = aMenuPosition;
			AddContextMenuItem(_TB("Open website"));
		}

		if (!m_DetailEmail.IsEmpty())
		{
			aMenuPosition++;
			CONTEXT_MENU_SEND_EMAIL = aMenuPosition;
			AddContextMenuItem(_TB("Send Email"));
		}

		if (!m_DetailAddress.IsEmpty() || (!m_DetailLatitude.IsEmpty() && !m_DetailLongitude.IsEmpty()))
		{
			aMenuPosition++;
			CONTEXT_MENU_SHOW_MAP = aMenuPosition;
			AddContextMenuItem(_TB("Show map"));
			aMenuPosition++;
			CONTEXT_MENU_SHOW_SATELLITE_VIEW = aMenuPosition;
			AddContextMenuItem(_TB("Show satellite view"));
		}
	}

	aMenuPosition++;
	AddContextMenuSeparator();
	aMenuPosition++;
	CONTEXT_MENU_EXPAND_COLLAPSE = aMenuPosition;
	AddContextMenuItem(_TB("Expand/Collapse"));
}

//-----------------------------------------------------------------------------
void BDResourcesLayout::DoDragDrop()
{
	DoTreeDragDrop();

	switch (m_DragDropAction)
	{
		case DRAG_DROP_CANCEL :
			SetCancelDragDrop();
			break;

		case DRAG_DROP_MOVE :
			if (m_ResourceType.IsEmpty() || m_Resource.IsEmpty())
				SetFilterDefaultValue();
			LoadTree();
			break;

		case DRAG_DROP_COPY :
			if (m_ResourceType.IsEmpty() || m_Resource.IsEmpty())
				SetFilterDefaultValue();
			SetCancelDragDrop();
			LoadTree();
			break;
	}

	UpdateDataView();
}

//-----------------------------------------------------------------------------
void BDResourcesLayout::DoTreeDragDrop()
{
	CString aKey			= GetSelectedNodeKey();
	CString aFromParentKey	= GetParentKey(aKey);
	CString aNewParentKey	= GetNewParentKey();

	if (aKey.IsEmpty() || aFromParentKey.IsEmpty())
	{
		m_DragDropAction = DRAG_DROP_CANCEL;
		return;
	}

	// calcolo le informazioni del nodo selezionato
	long aKeyType = 0;
	CString aResourceType;
	CString aResourceCode;
	DataLng aWorkerID;
	ExtractResourceInfoFromKey(aKey, aKeyType, aResourceType, aResourceCode, aWorkerID);

	if (aKeyType == 0 || aFromParentKey == aNewParentKey || aKey == aNewParentKey)
	{
		m_DragDropAction = DRAG_DROP_CANCEL;
		return;
	}

	// calcolo le informazioni del nodo di partenza
	long aFromKeyType = 0;
	CString aFromResourceType;
	CString aFromResourceCode;
	DataLng aFromWorkerID;
	ExtractResourceInfoFromKey(aFromParentKey, aFromKeyType, aFromResourceType, aFromResourceCode, aFromWorkerID);

	if (aFromKeyType == 0)
	{
		m_DragDropAction = DRAG_DROP_CANCEL;
		return;
	}

	// calcolo le informazioni del nodo di arrivo
	long aToKeyType = 0;
	CString aToResourceType;
	CString aToResourceCode;
	DataLng aToWorkerID;
	ExtractResourceInfoFromKey(aNewParentKey, aToKeyType, aToResourceType, aToResourceCode, aToWorkerID);

	if (aToKeyType == 0)
	{
		m_DragDropAction = DRAG_DROP_CANCEL;
		return;
	}

	int aNewParentKeyIndex = _wtol(aNewParentKey.Mid(0, 6));

	return MoveResource
		(
		aKeyType, aResourceType, aResourceCode, aWorkerID, 
		aFromKeyType, aFromResourceType, aFromResourceCode, aFromWorkerID, 
		aToKeyType, aToResourceType, aToResourceCode, aToWorkerID, 
		aNewParentKeyIndex
		);
}

//-----------------------------------------------------------------------------
void BDResourcesLayout::MoveResource
(
	const long aKeyType, const CString aResourceType, const CString aResourceCode, const DataLng aWorkerID,
	const long aFromKeyType, const CString aFromResourceType, const CString aFromResourceCode, const DataLng aFromWorkerID,
	const long aToKeyType, const CString aToResourceType, const CString aToResourceCode, const DataLng aToWorkerID,
	int aNewParentKeyIndex
)
{
	BOOL			bOK = TRUE;
	DataLng			aWorker = DataLng(0);
	DataStr			aResourceDes(_T(""));
	DataStr			aFromResourceDes(_T(""));
	DataStr			aToResourceDes(_T(""));

	// mi tengo da parte la descrizione della risorsa/worker corrente, di partenza e di arrivo
	// per poter visualizzare dei testi corretti
	switch (aKeyType)
	{
		case 1:
		default:
			aResourceDes = _T("Azienda description /*temporary string*/");
			break;
		case 2:
			m_TRResources.FindRecord(aResourceCode, aResourceType);
			aResourceDes = m_TRResources.GetRecord()->f_Description;
			break;
		case 3:
			m_TRWorkers.FindRecord(DataLng(aWorkerID));
			aResourceDes = m_TRWorkers.GetWorker();
			break;
	}
	switch (aFromKeyType)
	{
		case 1:
		default:
			aFromResourceDes = _T("Azienda description /*temporary string*/");
			break;
		case 2:
			m_TRResources.FindRecord(aFromResourceCode, aFromResourceType);
			aFromResourceDes = m_TRResources.GetRecord()->f_Description;
			break;
		case 3:
			m_TRWorkers.FindRecord(DataLng(aFromWorkerID));
			aFromResourceDes = m_TRWorkers.GetWorker();
			break;
	}
	switch (aToKeyType)
	{
		case 1:
		default:
			aToResourceDes = _T("Azienda description /*temporary string*/");
			break;
		case 2:
			m_TRResources.FindRecord(aToResourceCode, aToResourceType);
			aToResourceDes = m_TRResources.GetRecord()->f_Description;
			break;
		case 3:
			m_TRWorkers.FindRecord(DataLng(aToWorkerID));
			aToResourceDes = m_TRWorkers.GetWorker();
			break;
	}
	// 

	BOOL bCheckRecursivity = TRUE; // nasce a true cosi non faccio il check per le risorse/worker che sposto  sotto la root

	// analizzo la ricorsivita' del worker che sto spostando/copiando
	if (aKeyType == 3)
	{
		if (aToKeyType == 2) // copio sotto una risorsa
			bCheckRecursivity = CheckRecursivity(aToResourceType, aToResourceCode, _T(""), aWorkerID.Str());
		if (aToKeyType == 3) // copio sotto un worker
			bCheckRecursivity = CheckRecursivity(_T(""), aToWorkerID.Str(), _T(""), aWorkerID.Str());
	}

	// analizzo la ricorsivita' del worker che sto spostando/copiando
	if (aKeyType == 2)
	{
		if (aToKeyType == 2) // copio sotto una risorsa
			bCheckRecursivity = CheckRecursivity(aToResourceType, aToResourceCode, aResourceType, aResourceCode);
		if (aToKeyType == 3) // copio sotto un worker
			bCheckRecursivity = CheckRecursivity(_T(""), aToWorkerID.Str(), aResourceType, aResourceCode);
	}

	// se sono state individuate delle ricorsioni visualizzo un msg e torno
	if (!bCheckRecursivity)
	{
		m_pMessages->Add(cwsprintf(_TB("You cannot move or copy '{0-%s} {1-%s}'\nto '{2-%s} {3-%s}' because it will be recursive."),
			aResourceType.GetString(), aResourceDes.Str(), aToResourceType.GetString(), aToResourceDes.Str()));
		m_pMessages->Show();
		m_DragDropAction = DRAG_DROP_CANCEL;
		return;
	}

	BeginWaitCursor();

	// sto agendo su un nodo di tipo worker
	if (aKeyType == 3) 
	{
		aWorker = DataLng(aWorkerID);
		m_TRWorkers.FindRecord(aWorker);

		ConfirmDragDrop(aKeyType, aResourceType, m_TRWorkers.GetWorker(), aFromKeyType, aFromResourceType, aFromResourceDes, aToKeyType, aToResourceType, aToResourceDes);

		// mi sto spostando da una risorsa ad un'altra risorsa
		if (m_DragDropAction == DRAG_DROP_MOVE && aFromKeyType == 2 &&
			m_TUResourcesDetails.FindRecord(aFromResourceType, aFromResourceCode, aWorker, TRUE) == TableUpdater::FOUND)
		{
			if (!m_pTbContext->StartTransaction())
			{
				m_DragDropAction = DRAG_DROP_CANCEL;
				return;
			}

			bOK = m_TUResourcesDetails.DeleteRecord();
			m_TUResourcesDetails.UnlockCurrent();
			bOK ? m_pTbContext->Commit() : m_pTbContext->Rollback();
		}

		if (bOK && (m_DragDropAction == DRAG_DROP_MOVE || m_DragDropAction == DRAG_DROP_COPY) && aToKeyType == 2 &&
			m_TUResourcesDetails.FindRecord(aToResourceType, DataStr(aToResourceCode), aWorker, TRUE) == TableUpdater::NOT_FOUND)
		{
			if (!m_pTbContext->StartTransaction())
			{
				m_DragDropAction = DRAG_DROP_CANCEL;
				return;
			}

			m_TUResourcesDetails.GetRecord()->f_ResourceType = aToResourceType;
			m_TUResourcesDetails.GetRecord()->f_ResourceCode = DataStr(aToResourceCode);
			m_TUResourcesDetails.GetRecord()->f_ChildResourceType = DataStr(_T(""));
			m_TUResourcesDetails.GetRecord()->f_ChildResourceCode = DataStr(_T(""));
			m_TUResourcesDetails.GetRecord()->f_ChildWorkerID = aWorker;
			m_TUResourcesDetails.GetRecord()->f_IsWorker = TRUE;

			bOK = m_TUResourcesDetails.UpdateRecord();
			m_TUResourcesDetails.UnlockCurrent();
			bOK ? m_pTbContext->Commit() : m_pTbContext->Rollback();
		}

		// mi sto spostando da un worker ad un altro worker
		if (m_DragDropAction == DRAG_DROP_MOVE && aFromKeyType == 3 &&
			m_TUWorkersDetails.FindRecord(aFromWorkerID, aWorker, TRUE) == TableUpdater::FOUND)
		{
			if (!m_pTbContext->StartTransaction())
			{
				m_DragDropAction = DRAG_DROP_CANCEL;
				return;
			}

			bOK = m_TUWorkersDetails.DeleteRecord();
			m_TUWorkersDetails.UnlockCurrent();
			bOK ? m_pTbContext->Commit() : m_pTbContext->Rollback();
		}

		if (bOK && (m_DragDropAction == DRAG_DROP_MOVE || m_DragDropAction == DRAG_DROP_COPY) && aToKeyType == 3 &&
			m_TUWorkersDetails.FindRecord(aToWorkerID, aWorker, TRUE) == TableUpdater::NOT_FOUND)
		{
			if (!m_pTbContext->StartTransaction())
			{
				m_DragDropAction = DRAG_DROP_CANCEL;
				return;
			}

			m_TUWorkersDetails.GetRecord()->f_WorkerID = aToWorkerID;
			m_TUWorkersDetails.GetRecord()->f_ChildResourceType = DataStr(_T(""));
			m_TUWorkersDetails.GetRecord()->f_ChildResourceCode = DataStr(_T(""));
			m_TUWorkersDetails.GetRecord()->f_ChildWorkerID = aWorker;
			m_TUWorkersDetails.GetRecord()->f_IsWorker = TRUE;

			bOK = m_TUWorkersDetails.UpdateRecord();
			m_TUWorkersDetails.UnlockCurrent();
			bOK ? m_pTbContext->Commit() : m_pTbContext->Rollback();
		}
	}
	else // sto agendo su un nodo di tipo risorsa
	{
		m_TRResources.FindRecord(aResourceCode, aResourceType);

		ConfirmDragDrop(aKeyType, aResourceType, m_TRResources.GetRecord()->f_Description, aFromKeyType, aFromResourceType, aFromResourceDes, aToKeyType, aToResourceType, aToResourceDes);

		// mi sto spostando da una risorsa ad un'altra risorsa
		if (m_DragDropAction == DRAG_DROP_MOVE && aFromKeyType == 2 &&
			m_TUResourcesDetails.FindRecord(aFromResourceType, aFromResourceCode, aResourceType, aResourceCode, TRUE) == TableUpdater::FOUND)
		{
			if (!m_pTbContext->StartTransaction())
			{
				m_DragDropAction = DRAG_DROP_CANCEL;
				return;
			}

			bOK = m_TUResourcesDetails.DeleteRecord();
			m_TUResourcesDetails.UnlockCurrent();
			bOK ? m_pTbContext->Commit() : m_pTbContext->Rollback();
		}

		if (bOK && (m_DragDropAction == DRAG_DROP_MOVE || m_DragDropAction == DRAG_DROP_COPY) && aToKeyType == 2 &&
			m_TUResourcesDetails.FindRecord(aToResourceType, aToResourceCode, aResourceType, aResourceCode, TRUE) == TableUpdater::NOT_FOUND)
		{
			if (!m_pTbContext->StartTransaction())
			{
				m_DragDropAction = DRAG_DROP_CANCEL;
				return;
			}

			m_TUResourcesDetails.GetRecord()->f_ResourceType		= aToResourceType;
			m_TUResourcesDetails.GetRecord()->f_ResourceCode		= aToResourceCode;
			m_TUResourcesDetails.GetRecord()->f_ChildResourceType	= aResourceType;
			m_TUResourcesDetails.GetRecord()->f_ChildResourceCode	= aResourceCode;
			m_TUResourcesDetails.GetRecord()->f_ChildWorkerID		= DataLng(0);
			m_TUResourcesDetails.GetRecord()->f_IsWorker			= FALSE;

			bOK = m_TUResourcesDetails.UpdateRecord();
			m_TUResourcesDetails.UnlockCurrent();
			bOK ? m_pTbContext->Commit() : m_pTbContext->Rollback();
		}

		// mi sto spostando da una risorsa ad un worker
		if (m_DragDropAction == DRAG_DROP_MOVE && aFromKeyType == 3 &&
			m_TUWorkersDetails.FindRecord(aFromWorkerID, aResourceType, aResourceCode, TRUE) == TableUpdater::FOUND)
		{
			if (!m_pTbContext->StartTransaction())
			{
				m_DragDropAction = DRAG_DROP_CANCEL;
				return;
			}

			bOK = m_TUWorkersDetails.DeleteRecord();
			m_TUWorkersDetails.UnlockCurrent();
			bOK ? m_pTbContext->Commit() : m_pTbContext->Rollback();
		}

		if (bOK && (m_DragDropAction == DRAG_DROP_MOVE || m_DragDropAction == DRAG_DROP_COPY) && aToKeyType == 3 &&
			m_TUWorkersDetails.FindRecord(aToWorkerID, aResourceType, aResourceCode, TRUE) == TableUpdater::NOT_FOUND)
		{
			if (!m_pTbContext->StartTransaction())
			{
				m_DragDropAction = DRAG_DROP_CANCEL;
				return;
			}

			m_TUWorkersDetails.GetRecord()->f_WorkerID = aToWorkerID;
			m_TUWorkersDetails.GetRecord()->f_ChildResourceType = aResourceType;
			m_TUWorkersDetails.GetRecord()->f_ChildResourceCode = aResourceCode;
			m_TUWorkersDetails.GetRecord()->f_ChildWorkerID = DataLng(0);
			m_TUWorkersDetails.GetRecord()->f_IsWorker = FALSE;

			bOK = m_TUWorkersDetails.UpdateRecord();
			m_TUWorkersDetails.UnlockCurrent();
			bOK ? m_pTbContext->Commit() : m_pTbContext->Rollback();
		}
	}

	EndWaitCursor();

	if (!bOK)
	{
		m_DragDropAction = DRAG_DROP_CANCEL;
		return;
	}

	if (bOK && m_DragDropAction == DRAG_DROP_COPY)
	{
		CString aKey;
		if (aToKeyType == 2)
		{
			aKey = GetResourceKey(aNewParentKeyIndex, RESOURCE_RESOURCE, aToResourceType, aToResourceCode);
			InsertChild
				(
				aKey,
				GetResourceDescription(aResourceType, aResourceCode),
				GetResourceKey(++m_Index, RESOURCE_RESOURCE, aResourceType, aResourceCode),
				GetResourceImage(aKey, m_TRResources.GetRecord()->f_Disabled)
				);
		}

		if (aToKeyType == 3)
		{
			aKey = GetResourceKey(aNewParentKeyIndex, RESOURCE_WORKER, _T(""), aToWorkerID.Str());
			InsertChild
				(
				aKey,
				GetWorkerDescription(aWorker),
				GetResourceKey(++m_Index, RESOURCE_WORKER, _T(""), aWorkerID.Str()),
				GetResourceImage(aKey, m_TRWorkers.GetRecord()->f_Disabled)
				);
		}
	}
}

//----------------------------------------------------------------------------
CString	BDResourcesLayout::ResolveAmp(CString aValue)
{
	int	aPos = aValue.Find(_T("&"));

	if (aPos != -1)
		aValue.Insert(aPos, _T("&"));

	return aValue;
}

//-----------------------------------------------------------------------------
void BDResourcesLayout::ConfirmDragDrop
(	
	const long aKeyType,		const CString aResourceType,	const CString aResourceCode, 
	const long aFromKeyType,	const CString aFromResourceType,const CString aFromResourceCode,
	const long aToKeyType,		const CString aToResourceType,	const CString aToResourceCode
)
{
	BOOL bOK = FALSE;
	
	CString aResType		= aResourceType;
	CString aResCode		= aResourceCode;
	CString aFromResType	= aFromResourceType;
	CString aFromResCode	= aFromResourceCode;
	CString aToResType		= aToResourceType;
	CString aToResCode		= aToResourceCode;

	if (aFromKeyType == 1)
	{
		m_sActionMessage = cwsprintf(_TB("Do you want to assign or copy '{0-%s} {1-%s}' to '{2-%s} {3-%s}'?"),
			ResolveAmp(aResType).Trim(), ResolveAmp(aResCode).Trim(), ResolveAmp(aToResType).Trim(), ResolveAmp(aToResCode).Trim());
		m_sButton1 = _TB("Assign");
		m_sButton2 = _TB("Copy");
	}
	else if (aToKeyType == 1)
	{
		m_sActionMessage = cwsprintf(_TB("Do you want to remove or copy '{0-%s} {1-%s}' from '{2-%s} {3-%s}'?"),
			ResolveAmp(aResType).Trim(), ResolveAmp(aResCode).Trim(), ResolveAmp(aFromResType).Trim(), ResolveAmp(aFromResCode).Trim());
		m_sButton1 = _TB("Remove");
		m_sButton2 = _TB("Copy");
	}
	else
	{
		m_sActionMessage = cwsprintf(_TB("Do you want to move or copy '{0-%s} {1-%s}' from '{2-%s} {3-%s}' to '{4-%s} {5-%s}'?"),
			ResolveAmp(aResType).Trim(), ResolveAmp(aResCode).Trim(), ResolveAmp(aFromResType).Trim(), ResolveAmp(aFromResCode).Trim(), 
			ResolveAmp(aToResType).Trim(), ResolveAmp(aToResCode).Trim());
		m_sButton1 = _TB("Move");
		m_sButton2 = _TB("Copy");
	}

	CreateSlaveView(IDD_RESOURCES_LAYOUT_ACTION, NULL, 1);
}

//-----------------------------------------------------------------------------
BOOL BDResourcesLayout::CheckRecursivity(DataStr aToResourceType, DataStr aToResource, DataStr aResourceType, DataStr aResource)
{
	return !m_pCheckResourcesRecursion->IsRecursive(aToResource, aToResourceType,aResource, aResourceType);
}

//-----------------------------------------------------------------------------
void BDResourcesLayout::DoCtxMenuItemClick(CString nodeKey)
{
	int aMenuItem = GetIdxContextMenuItemClicked() + 1;

	if (aMenuItem == CONTEXT_MENU_OPEN_DOCUMENT)
		RunDocument(OPEN_DOCUMENT);
	else if (aMenuItem == CONTEXT_MENU_NEW_RESOURCE)
		RunDocument(NEW_RESOURCE);
	else if (aMenuItem == CONTEXT_MENU_NEW_WORKER)
		RunDocument(NEW_WORKER);
	else if (aMenuItem == CONTEXT_MENU_ENABLE_DISABLE)
	{
		EnableDisable();
		LoadTree();
	}
	else if (aMenuItem == CONTEXT_MENU_OPEN_WEBSITE)
		OpenUrl();
	else if (aMenuItem == CONTEXT_MENU_SEND_EMAIL)
		SendEMail();
	else if (aMenuItem == CONTEXT_MENU_SHOW_MAP)
		ShowMap();
	else if (aMenuItem == CONTEXT_MENU_SHOW_SATELLITE_VIEW)
		ShowMap(TRUE);
	else if (aMenuItem == CONTEXT_MENU_EXPAND_COLLAPSE)
		DoExpandCollapse();
}

//-----------------------------------------------------------------------------
void BDResourcesLayout::RunDocument(DocumentAction aAction/* = OPEN_DOCUMENT*/)
{
	CString aKey = GetSelectedNodeKey();
	if (aKey.IsEmpty())
		return;

	long aKeyType = 0;
	CString aResourceType;
	CString aResourceCode;
	DataLng aWorkerID;

	ExtractResourceInfoFromKey(aKey, aKeyType, aResourceType, aResourceCode, aWorkerID);

	if (aKeyType == 0) return;

	switch (aAction)
	{
		case NEW_RESOURCE :
		{
			ADMResourcesObj* pDoc = (ADMResourcesObj*) AfxGetTbCmdManager()->RunDocument(ADM_CLASS(ADMResourcesObj), szDefaultViewMode, NULL);
			if (pDoc && pDoc->ADMNewDocument())
			{
				if (aKeyType == RESOURCE_RESOURCE)
					pDoc->SetParentResource(aResourceType, aResourceCode);
				if (aKeyType == RESOURCE_WORKER)
					pDoc->SetParentWorkerID(aWorkerID);
			}
			break;
		}
		case NEW_WORKER :
		{
			ADMWorkersObj* pDoc = (ADMWorkersObj*) AfxGetTbCmdManager()->RunDocument(ADM_CLASS(ADMWorkersObj), szDefaultViewMode, NULL);
			if (pDoc && pDoc->ADMNewDocument())
			{
				if (aKeyType == RESOURCE_RESOURCE)
					pDoc->SetParentResource(aResourceType, aResourceCode);
				if (aKeyType == RESOURCE_WORKER)
					pDoc->SetParentWorkerID(aWorkerID);
			}
			break;
		}
		default : // OPEN_DOCUMENT
		{
			if (aKeyType == RESOURCE_ROOT) // nodo azienda
				GetFrame()->SendMessage(WM_COMMAND, ID_RESOURCES_LAYOUT_TB_ROOT_OPEN); // se ne occupa il client-doc in ERP
			else 
				if (aKeyType == RESOURCE_WORKER)
				{
					ADMWorkersObj* pDoc = (ADMWorkersObj*) AfxGetTbCmdManager()->RunDocument(ADM_CLASS(ADMWorkersObj), szDefaultViewMode, NULL);
					if (pDoc) pDoc->SetWorker(aWorkerID);	
				}
				else
				{
					ADMResourcesObj* pDoc = (ADMResourcesObj*) AfxGetTbCmdManager()->RunDocument(ADM_CLASS(ADMResourcesObj), szDefaultViewMode, NULL);
					if (pDoc) pDoc->SetResource(aResourceType, aResourceCode);
				}
			break;
		}
	}
}

// metodo che data la chiave del nodo selezionato ritorna:
// il tipo chiave (0: default(=empty), 1: company, 2: risorsa, 3: worker)
// Il tipo risorsa + risorsa 
// L'ID del worker 
//-----------------------------------------------------------------------------
void BDResourcesLayout::ExtractResourceInfoFromKey(const CString& aKey, long& aKeyType, CString& aResourceType, CString& aResourceCode, DataLng& aWorkerID)
{
	// vado ad estrarre il chr nr. 7 per capire di che tipo risorsa si tratta
	aKeyType = _wtol(aKey.Mid(6, 1));

	switch (aKeyType)
	{
		case 1:
		default:
			break;
		case 2:
			aResourceType = aKey.Mid(7, 8);
			aResourceCode = aKey.Mid(15, 8);
			break;
		case 3:
			aWorkerID = DataLng(_wtol(aKey.Mid(7, 6)));
			break;
	}
}

//-----------------------------------------------------------------------------
BOOL BDResourcesLayout::EnableDisable()
{
	BOOL bOK = FALSE;

	CString aKey = GetSelectedNodeKey();
	if (aKey.IsEmpty())
		return bOK;

	long aKeyType = 0;
	CString aResourceType;
	CString aResourceCode;
	DataLng aWorkerID;

	ExtractResourceInfoFromKey(aKey, aKeyType, aResourceType, aResourceCode, aWorkerID);

	if (aKeyType <= 1) // posso disabilitare solo worker/risorse
		return bOK;

	if (!m_pTbContext->StartTransaction())
		return bOK;

	if (aKeyType == RESOURCE_WORKER)
	{
		if (m_TUWorkers.FindRecord(aWorkerID, TRUE) == TableUpdater::FOUND)
		{
			m_TUWorkers.GetRecord()->f_Disabled = !m_TUWorkers.GetRecord()->f_Disabled;
			bOK = m_TUWorkers.UpdateRecord();
			m_TUWorkers.UnlockCurrent();		
		}
	}
	else
	{
		if (m_TUResources.FindRecord(aResourceType, aResourceCode, TRUE) == TableUpdater::FOUND)
		{
			m_TUResources.GetRecord()->f_Disabled = !m_TUResources.GetRecord()->f_Disabled;
			bOK = m_TUResources.UpdateRecord();
			m_TUResources.UnlockCurrent();		
		}
	}

	bOK ? m_pTbContext->Commit() : m_pTbContext->Rollback();

	return bOK;
}

// per mettere l'immagine corretta nel nodo del tree
//-----------------------------------------------------------------------------
CString BDResourcesLayout::GetResourceImage(const DataStr& sKey, BOOL bDisabled/* = FALSE*/)
{
	CString aKey = sKey;
	long aKeyType = _wtol(aKey.Mid(6, 1));

	CString sImageName = _T("");

	switch (aKeyType)
	{
		case 2:
			sImageName = bDisabled ? szResourceImageDisabled : szResourceImage;
			break;

		case 3:
			sImageName = bDisabled ? szWorkerImageDisabled : szWorkerImage;
			break;

		default:
			break;
	}

	return sImageName;
}

// funzione per aggiungere n-spazi in fondo alle chiavi ed eseguire senza problemi 
// i Mid per estrarre i singoli valori dalla chiave principale
//------------------------------------------------------------------------------
void PadString(CString& strToPadd, int aPaddedLen)
{
	if (strToPadd.GetLength() == aPaddedLen)
		return;

	//pad Right
	int posTo = aPaddedLen - strToPadd.GetLength();
	if (posTo <= 0)
		return;

	CString strPad(BLANK_CHAR, posTo);
	strToPadd.Append(strPad);
}

// calcola la chiave di ogni nodo
// e' composta da un numero univoco di 6 cifre, il tipo di risorsa, tipo risorsa, codice risorsa o workerid
//-----------------------------------------------------------------------------
CString BDResourcesLayout::GetResourceKey(int aIndex, int aTypeId, const DataStr& aResourceType, const DataStr& aResource)
{
	CString sResType = aResourceType.Str();
	CString sResource = aResource.Str();

	// TypeId sta ad identificare il tipo di nodo: 1 = nodo root; 2 = nodo risorsa, 3 = nodo worker
	switch (aTypeId)
	{
		case 1:
		default:
			break;
		case 2:
			PadString(sResType, RESOURCEPADCHARNR);
			PadString(sResource, RESOURCEPADCHARNR);
			break;
		case 3:
			PadString(sResource, WORKERPADCHARNR);
			break;
	}

	//TRACE1("Add Key: %s\n", cwsprintf(_T("%06d-%d-%s-%s-"), aIndex, aTypeId, sResType, sResource));
	return cwsprintf(_T("%06d%d%s%s"), aIndex, aTypeId, sResType, sResource);
}

//-----------------------------------------------------------------------------
CString BDResourcesLayout::GetResourceDescription(const DataStr& aResourceType, const DataStr& aResource)
{
	CString aDescription = aResource.GetString();

	m_TRResources.FindRecord(aResource, aResourceType);

	if (!m_TRResources.GetRecord()->f_Description.IsEmpty())
		aDescription = m_TRResources.GetRecord()->f_Description.GetString();

	if (m_TRResources.GetRecord()->f_Manager.IsEmpty())
		return aDescription;
	else
	{
		m_TRWorkers.FindRecord(m_TRResources.GetRecord()->f_Manager);
		return cwsprintf(_TB("{0-%s} [Manager: {1-%s}]"), aDescription, m_TRWorkers.GetWorker().GetString());
	}
}

//-----------------------------------------------------------------------------
CString BDResourcesLayout::GetWorkerDescription(DataLng& aWorker)
{
	m_TRWorkers.FindRecord(aWorker);
	return m_TRWorkers.GetWorker().GetString();
}

// per impostare l'immagine del nodo (prima carico quello specificato nella tabella, 
// altrimenti metto l'immagine di default)
//-----------------------------------------------------------------------------
void BDResourcesLayout::SetDefaultImage()
{
	CString aDetail =  ResolveAmp(m_DetailName.GetString());
	
	switch (m_ResourceTypeId)
	{
		case RESOURCE_WORKER:
			if (m_DetailImage.IsEmpty()) m_DetailImage = TBGlyph(szGlyphWorkerBig); //szWorkerBigImage;
			m_TileImageCaption = cwsprintf(_TB("Worker: %s"), aDetail);
			break;

		case RESOURCE_RESOURCE:
			if (m_DetailImage.IsEmpty()) m_DetailImage = TBGlyph(szGlyphResourceBig); //szResourceBigImage;
			m_TileImageCaption = cwsprintf(_TB("Resource: %s"), aDetail);
			break;

		case RESOURCE_ROOT:
			if (m_DetailImage.IsEmpty()) m_DetailImage = TBGlyph(szGlyphRootBig); //szCompanyBigImage;
			m_TileImageCaption = cwsprintf(_TB("Company: %s"), aDetail);

			break;

		default: 
			if (m_DetailImage.IsEmpty()) m_DetailImage = _T("");
			m_TileImageCaption = _T("");
			break;
	}

	UpdateDataView();

}

//-----------------------------------------------------------------------------
void BDResourcesLayout::OpenUrl()
{
	if (!m_pFunctions || m_DetailWeb.IsEmpty())
		return;

	m_pFunctions->OpenUrl(m_DetailWeb.GetString());
}

//-----------------------------------------------------------------------------
void BDResourcesLayout::SendEMail()
{
	if (!m_pFunctions || m_DetailEmail.IsEmpty())
		return;

	m_pFunctions->SendEmailbyWorker(m_DetailEmail.GetString());
}

//-----------------------------------------------------------------------------
void BDResourcesLayout::ShowMap(BOOL bSatelliteView/* = FALSE*/)
{
	if (!m_pGecoder || (m_DetailAddress.IsEmpty() && (m_DetailLatitude.IsEmpty() || m_DetailLongitude.IsEmpty())))
		return;

	if (m_DetailLatitude.IsEmpty() || m_DetailLongitude.IsEmpty())
		m_pGecoder->OpenGoogleMaps(m_DetailAddress.GetString(), _T(""), m_DetailCity.GetString(), m_DetailCounty.GetString(), m_DetailCountry.GetString(), _T(""), m_DetailZip.GetString(), bSatelliteView);
	else
		m_pGecoder->OpenGoogleMaps(m_DetailLatitude.GetString(), m_DetailLongitude.GetString(), bSatelliteView);
}