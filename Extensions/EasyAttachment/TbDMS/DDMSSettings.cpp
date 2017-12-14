#include "stdafx.h"

#include "TBRepositoryManager.h"

#include "EasyAttachment\JsonForms\UIDMSSettings\IDD_DMS_SETTINGS.hjson"

#include "DDMSSettings.h"

#ifdef _DEBUG
#undef THIS_FILE
static char THIS_FILE[] = __FILE__;
#endif

//////////////////////////////////////////////////////////////////////////////
//							DBTDMSSettings									//
//////////////////////////////////////////////////////////////////////////////
//-----------------------------------------------------------------------------	
IMPLEMENT_DYNAMIC(DBTDMSSettings, DBTMaster)

//-----------------------------------------------------------------------------	
DBTDMSSettings::DBTDMSSettings
	(
		CRuntimeClass*		pClass, 
		CAbstractFormDoc*	pDocument
	)
	:
	DBTMaster (pClass, pDocument, _NS_DBT("Settings"))
{
}

//-----------------------------------------------------------------------------	
void DBTDMSSettings::OnPrepareBrowser(SqlTable* pTable)
{
}

//-----------------------------------------------------------------------------
void DBTDMSSettings::OnDefineQuery()
{
}

//-----------------------------------------------------------------------------
void DBTDMSSettings::OnPrepareQuery()
{
}

//-----------------------------------------------------------------------------
BOOL DBTDMSSettings::OnCheckPrimaryKey()
{
	return TRUE;
}

//-----------------------------------------------------------------------------
void DBTDMSSettings::OnPreparePrimaryKey()
{
}

//--------------------------------------------------------------------------------
void DBTDMSSettings::OnDisableControlsForEdit()
{
	if (!AfxGetTbRepositoryManager()->SearchInContentEnabled())
	{
		GetVSettings()->l_EnableFTS = FALSE;
		GetVSettings()->l_EnableFTS.SetReadOnly();
	}

	GetVSettings()->l_FTSNotConsiderPdF.SetReadOnly(!GetVSettings()->l_EnableFTS);
	GetVSettings()->l_MaxElementsInEnvelope.SetReadOnly(!GetVSettings()->l_EnableSOS);

	GetVSettings()->l_BarcodeType.SetReadOnly(!GetVSettings()->l_EnableBarcode);
	GetVSettings()->l_BarcodePrefix.SetReadOnly(!GetVSettings()->l_EnableBarcode);
	GetVSettings()->l_AutomaticBarcodeDetection.SetReadOnly(!GetVSettings()->l_EnableBarcode);
	GetVSettings()->l_BarcodeDetectionAction.SetReadOnly(!GetVSettings()->l_EnableBarcode);
	GetVSettings()->l_BCActionForBatch.SetReadOnly(!GetVSettings()->l_EnableBarcode);
	GetVSettings()->l_BCActionForDocument.SetReadOnly(!GetVSettings()->l_EnableBarcode);
	GetVSettings()->l_PrintBarcodeInReport.SetReadOnly(!GetVSettings()->l_EnableBarcode);

	GetVSettings()->l_StorageFolderPath.SetReadOnly(!GetVSettings()->l_StorageToFileSystem);
}

//--------------------------------------------------------------------------------
BOOL DBTDMSSettings::FindData(BOOL /*bPreparedOld*/)
{
	GetDocument()->m_pDMSSettings = AfxGetTbRepositoryManager()->GetDMSSettings();

	if (GetDocument()->m_pDMSSettings)
	{
		VSettings* pRec = GetDocument()->m_pDMSSettings->GetSettings();
		*m_pRecord = *pRec;
	}

	return TRUE;
}

//--------------------------------------------------------------------------------
BOOL DBTDMSSettings::Update()
{
	VSettings* pRec = GetVSettings();
	VSettings* pRecSettings = GetDocument()->m_pDMSSettings->GetSettings();
	*pRecSettings = *pRec;

	RecordArray* pArExtensions = GetDocument()->m_pDMSSettings->GetExtensions();
	pArExtensions->RemoveAll();

	VExtensionMaxSize* pExtRec;
	for (int i = 0; i <= GetDocument()->m_pDBTExtensions->GetUpperBound(); i++)
	{
		pExtRec = new VExtensionMaxSize();
		*pExtRec = *(VExtensionMaxSize*)GetDocument()->m_pDBTExtensions->GetRow(i);
		pArExtensions->Add(pExtRec);
	}

	return AfxGetTbRepositoryManager()->SaveDMSSettings();
}

//////////////////////////////////////////////////////////////////////////////
//             class DBTDMSExtensions implementation
//////////////////////////////////////////////////////////////////////////////
//-----------------------------------------------------------------------------	
IMPLEMENT_DYNAMIC(DBTDMSExtensions, DBTSlaveBuffered)

//-----------------------------------------------------------------------------	
DBTDMSExtensions::DBTDMSExtensions(CRuntimeClass* pClass, CAbstractFormDoc*	pDocument)
	:
	DBTSlaveBuffered(pClass, pDocument, _NS_DBT("DMSExtensions"), ALLOW_EMPTY_BODY, FALSE)
{
}

//-----------------------------------------------------------------------------
void DBTDMSExtensions::OnDefineQuery()
{
}

// Serve a valorizzare i parametri di query.
//-----------------------------------------------------------------------------
void DBTDMSExtensions::OnPrepareQuery()
{
	// nessun altro criterio di preparazione
}

//-----------------------------------------------------------------------------	
DataObj* DBTDMSExtensions::OnCheckPrimaryKey(int /*nRow */, SqlRecord* pRec)
{
	return NULL;
}

// Controlli di primary key 
//-----------------------------------------------------------------------------	
void DBTDMSExtensions::OnPreparePrimaryKey(int nRow, SqlRecord* pRec)
{
}

// aggiungo manualmente i SqlRecord al DBT
//-----------------------------------------------------------------------------	
BOOL DBTDMSExtensions::LocalFindData(BOOL bPrepareOld)
{
	RemoveAll();

	RecordArray* pArExtensions = GetDocument()->m_pDMSSettings->GetExtensions();
	if (!pArExtensions)
		return FALSE;

	VExtensionMaxSize* pRec;
	for (int i = 0; i < pArExtensions->GetCount(); i++)
	{
		pRec = (VExtensionMaxSize*)pArExtensions->GetAt(i);
		VExtensionMaxSize* pNewRec = (VExtensionMaxSize*)AddRecord();
		*pNewRec = *pRec;
	}

	return TRUE;
}

//////////////////////////////////////////////////////////////////////////////
//						DDMSSettings				                        //
//////////////////////////////////////////////////////////////////////////////
//-----------------------------------------------------------------------------
IMPLEMENT_DYNCREATE(DDMSSettings, CAbstractFormDoc)

//-----------------------------------------------------------------------------
BEGIN_MESSAGE_MAP(DDMSSettings, CAbstractFormDoc)
	//{{AFX_MSG_MAP(DDMSSettings)
	ON_EN_VALUE_CHANGED(IDC_DMS_SETTINGS_ENABLE_FTS,			OnEnableFTS)
	ON_EN_VALUE_CHANGED(IDC_DMS_SETTINGS_ENABLE_BARCODE,		OnEnableBarcode)
	ON_EN_VALUE_CHANGED(IDC_DMS_SETTINGS_ENABLE_SOS,			OnEnableSOS)
	ON_EN_VALUE_CHANGED(IDC_DMS_SETTINGS_STORAGE_TO_FILESYSTEM, OnEnableStorageToFileSystem)
	//}}AFX_MSG_MAP
END_MESSAGE_MAP()

//-----------------------------------------------------------------------------
DDMSSettings::DDMSSettings()
	:
	m_pDBTSettings	(NULL),
	m_pDBTExtensions(NULL),
	m_pDMSSettings	(NULL),
	m_bDMSSOSEnable	(FALSE),
	m_bFileStorageIsValid (FALSE)
{
	DisableFamilyClientDoc(TRUE);

	m_bDMSSOSEnable = AfxGetOleDbMng()->DMSSOSEnable();
}

//-----------------------------------------------------------------------------
DDMSSettings::~DDMSSettings()
{
	SAFE_DELETE(m_pDBTExtensions);
}

//-----------------------------------------------------------------------------
VSettings* DDMSSettings::GetVSettings() const { return (VSettings*)m_pDBTSettings->GetRecord(); }

//-----------------------------------------------------------------------------
BOOL DDMSSettings::OnAttachData()
{ 
	SetFormTitle(_TB("DMS Settings"));
	
	SetOnlyOneRecord(TRUE);

	m_pDBTSettings		= new DBTDMSSettings(RUNTIME_CLASS(VSettings), this);
	m_pDBTExtensions	= new DBTDMSExtensions(RUNTIME_CLASS(VExtensionMaxSize), this);
		
	Attach(m_pDBTSettings);
	m_pDBTSettings->FindData();
	SaveCurrentRecord();

	m_pDBTExtensions->LocalFindData(FALSE);

	DECLARE_VAR_JSON(bDMSSOSEnable);

	return TRUE;
}

//-----------------------------------------------------------------------------
BOOL DDMSSettings::CanRunDocument()
{
	if (!AfxGetOleDbMng()->EasyAttachmentEnable() && AfxGetOleDbMng()->GetDMSStatus() != StorageInvalid)
	{
		AfxMessageBox(_TB("Impossible to open DMS settings form!\r\nPlease, check in Administration Console if this company uses DMS."));
		return FALSE;
	}

	return TRUE;
}

//-----------------------------------------------------------------------------
BOOL DDMSSettings::OnPrepareAuxData()
{   
	return TRUE;
}

//-----------------------------------------------------------------------------
void DDMSSettings::OnPropertyCreated(CTBProperty* pProperty)
{
	CParsedCtrl* pCtrl = pProperty->GetControl();
	UINT nIDC = pCtrl ? pCtrl->GetCtrlID() : 0;

	if (nIDC == IDC_DMS_SETTINGS_DPI_QUALITY_IMAGE)
	{
		CIntEdit* pIntEdit = (CIntEdit*)pProperty->GetControl();
		if (pIntEdit)
			pIntEdit->SetRange(200, 600); 
		return;
	}

	if (nIDC == IDC_DMS_SETTINGS_MAX_ELEMENTS_IN_ENVELOPE)
	{
		CIntEdit * pIntEdit = (CIntEdit *)pProperty->GetControl();
		if (pIntEdit)
			pIntEdit->SetRange(1, 10000); 
		return;
	}

	if (nIDC == IDC_DMS_SETTINGS_STORAGE_FOLDER_PATH)
	{
		CBrowsePathEdit* pPathEdit = dynamic_cast<CBrowsePathEdit*>(pCtrl);
		if (pPathEdit)
			pPathEdit->SetCtrlStyle(pPathEdit->GetCtrlStyle() | PATH_STYLE_AS_PATH);
		return;
	}
}

//-----------------------------------------------------------------------------
void DDMSSettings::OnExtraEditTransaction()
{
	if (AfxGetOleDbMng()->GetDMSStatus() == StorageInvalid && m_bFileStorageIsValid)
		AfxGetOleDbMng()->SetDMSStatus(DMSStatusEnum::Valid);
}

//-----------------------------------------------------------------------------
BOOL DDMSSettings::OnOkTransaction()
{
	if (!CanSaveParameters())
	{
		AfxMessageBox(_TB("Before saving you have to close all other opened documents."));
		return FALSE;
	}

	VSettings* pRec = GetVSettings();
	if (!pRec)
		return FALSE;

	if (!pRec->l_ExcludedExtensions.IsEmpty())
	{
		CString sExtensions = pRec->l_ExcludedExtensions.GetString();
		sExtensions.MakeLower();
		if (sExtensions.Find(_T("pdf")) > -1)
		{
			if (AfxMessageBox(_TB("If you exclude .pdf extension no report can be archived!\r\nAre you sure to continue?"), MB_YESNO | MB_ICONQUESTION | MB_DEFBUTTON2) == IDNO)
				return FALSE;
		}
	}

	if (pRec->l_StorageToFileSystem)
	{
		m_bFileStorageIsValid = FALSE;
		if (pRec->l_StorageFolderPath.IsEmpty() || !::IsServerPath(pRec->l_StorageFolderPath))
		{
			Message(_TB("Please specify a remote path for shared folder."), MB_ICONSTOP);
			return FALSE;
		}


		if (!ExistPath(pRec->l_StorageFolderPath))
		{
			Message(_TB("The specified shared folder does not exists. Please choose a valid path."), MB_ICONSTOP);
			return FALSE;
		}

		if (!CheckDirectoryAccess(pRec->l_StorageFolderPath))
		{
			Message(_TB("The specified shared folder does not have the Read/Write permissions. Please check these settings."), MB_ICONSTOP);
			return FALSE;
		}
		m_bFileStorageIsValid = TRUE;
	}

	return CAbstractFormDoc::OnOkTransaction();
}

//-----------------------------------------------------------------------------
BOOL DDMSSettings::CanDoEditRecord()
{
	return CAbstractFormDoc::CanDoEditRecord() && AfxGetLoginInfos()->m_bAdmin;
}

//-----------------------------------------------------------------------------
void DDMSSettings::OnEnableFTS()
{
	GetVSettings()->l_FTSNotConsiderPdF.SetReadOnly(!GetVSettings()->l_EnableFTS);
	GetVSettings()->l_FTSNotConsiderPdF = FALSE;
	UpdateDataView();
}

//-----------------------------------------------------------------------------
void DDMSSettings::OnEnableBarcode()
{
	GetVSettings()->l_BarcodeType.SetReadOnly(!GetVSettings()->l_EnableBarcode);
	GetVSettings()->l_BarcodePrefix.SetReadOnly(!GetVSettings()->l_EnableBarcode);
	GetVSettings()->l_AutomaticBarcodeDetection.SetReadOnly(!GetVSettings()->l_EnableBarcode);
	GetVSettings()->l_BarcodeDetectionAction.SetReadOnly(!GetVSettings()->l_EnableBarcode);
	GetVSettings()->l_BCActionForBatch.SetReadOnly(!GetVSettings()->l_EnableBarcode);
	GetVSettings()->l_BCActionForDocument.SetReadOnly(!GetVSettings()->l_EnableBarcode);
	GetVSettings()->l_PrintBarcodeInReport.SetReadOnly(!GetVSettings()->l_EnableBarcode);

	UpdateDataView();
}

//-----------------------------------------------------------------------------
void DDMSSettings::OnEnableSOS()
{
	GetVSettings()->l_MaxElementsInEnvelope.SetReadOnly(!GetVSettings()->l_EnableSOS);
	if (GetVSettings()->l_MaxElementsInEnvelope.IsEmpty())
		GetVSettings()->l_MaxElementsInEnvelope = 200;

	UpdateDataView();
}

//-----------------------------------------------------------------------------
void DDMSSettings::OnEnableStorageToFileSystem()
{
	GetVSettings()->l_StorageFolderPath.SetReadOnly(!GetVSettings()->l_StorageToFileSystem);
	UpdateDataView();
}