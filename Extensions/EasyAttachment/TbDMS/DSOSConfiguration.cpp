#include "stdafx.h"

#include "TBRepositoryManager.h"
#include "CommonObjects.h"

#include "DSOSConfiguration.h"

#include "EasyAttachment\JsonForms\UISOSConfiguration\IDD_SOS_CONFIGURATION.hjson"

#ifdef _DEBUG
#undef THIS_FILE
static char THIS_FILE[] = __FILE__;
#endif

//////////////////////////////////////////////////////////////////////////////
//							DBTSOSConfiguration								//
//////////////////////////////////////////////////////////////////////////////
//-----------------------------------------------------------------------------	
IMPLEMENT_DYNAMIC(DBTSOSConfiguration, DBTMaster)

//-----------------------------------------------------------------------------	
DBTSOSConfiguration::DBTSOSConfiguration
	(
		CRuntimeClass*		pClass, 
		CAbstractFormDoc*	pDocument
	)
	:
	DBTMaster (pClass, pDocument, _NS_DBT("SOSConfiguration"))
{
}

//-----------------------------------------------------------------------------	
void DBTSOSConfiguration::OnPrepareBrowser(SqlTable* pTable)
{
}

//-----------------------------------------------------------------------------
void DBTSOSConfiguration::OnDefineQuery()
{
}

//-----------------------------------------------------------------------------
void DBTSOSConfiguration::OnPrepareQuery()
{
}

//-----------------------------------------------------------------------------
BOOL DBTSOSConfiguration::OnCheckPrimaryKey()
{
	return TRUE;
}

//-----------------------------------------------------------------------------
void DBTSOSConfiguration::OnPreparePrimaryKey()
{
}

//--------------------------------------------------------------------------------
void DBTSOSConfiguration::OnDisableControlsForEdit()
{
	GetVSOSConfiguration()->l_FTPSharedFolder.SetReadOnly(!GetVSOSConfiguration()->l_FTPSend);
	GetVSOSConfiguration()->l_FTPUpdateDayOfWeek.SetReadOnly(!GetVSOSConfiguration()->l_FTPSend);
}

//--------------------------------------------------------------------------------
BOOL DBTSOSConfiguration::FindData(BOOL /*bPreparedOld*/)
{
	GetDocument()->m_pSOSConfiguration = AfxGetTbRepositoryManager()->GetSOSConfiguration();

	if (GetDocument()->m_pSOSConfiguration)
		*m_pRecord = *GetDocument()->m_pSOSConfiguration->m_pRecSOSConfiguration;

	return TRUE;
}

//--------------------------------------------------------------------------------
BOOL DBTSOSConfiguration::Update()
{
	VSOSConfiguration* pRec = GetVSOSConfiguration();
	*GetDocument()->m_pSOSConfiguration->m_pRecSOSConfiguration = *pRec;
	return AfxGetTbRepositoryManager()->SaveSOSConfiguration();
}

//////////////////////////////////////////////////////////////////////////////
//             class DBTSOSDocClasses implementation
//////////////////////////////////////////////////////////////////////////////
//-----------------------------------------------------------------------------	
IMPLEMENT_DYNAMIC(DBTSOSDocClasses, DBTSlaveBuffered)

//-----------------------------------------------------------------------------	
DBTSOSDocClasses::DBTSOSDocClasses(CRuntimeClass* pClass, CAbstractFormDoc*	pDocument)
	:
	DBTSlaveBuffered(pClass, pDocument, _NS_DBT("SOSDocClasses"), ALLOW_EMPTY_BODY, FALSE)
{
}

//-----------------------------------------------------------------------------
void DBTSOSDocClasses::OnDefineQuery()
{
}

// Serve a valorizzare i parametri di query.
//-----------------------------------------------------------------------------
void DBTSOSDocClasses::OnPrepareQuery()
{
	// nessun altro criterio di preparazione
}

//-----------------------------------------------------------------------------	
DataObj* DBTSOSDocClasses::OnCheckPrimaryKey(int /*nRow */, SqlRecord* pRec)
{
	return NULL;
}

// Controlli di primary key 
//-----------------------------------------------------------------------------	
void DBTSOSDocClasses::OnPreparePrimaryKey(int nRow, SqlRecord* pRec)
{
}

// aggiungo manualmente i SqlRecord al DBT
//-----------------------------------------------------------------------------	
BOOL DBTSOSDocClasses::LocalFindData(BOOL bPrepareOld)
{
	RemoveAll();

	RecordArray* pDocClasses = GetDocument()->m_pSOSConfiguration->m_pSOSDocClassesArray;
	if (!pDocClasses)
		return FALSE;

	VSOSDocClass* pRec;
	for (int i = 0; i < pDocClasses->GetCount(); i++)
	{
		pRec = (VSOSDocClass*)pDocClasses->GetAt(i);
		VSOSDocClass* pNewRec = (VSOSDocClass*)AddRecord();
		*pNewRec = *pRec;
	}

	return TRUE;
}

//////////////////////////////////////////////////////////////////////////////
//						DSOSConfiguration				                    //
//////////////////////////////////////////////////////////////////////////////
//-----------------------------------------------------------------------------
IMPLEMENT_DYNCREATE(DSOSConfiguration, CAbstractFormDoc)

//-----------------------------------------------------------------------------
BEGIN_MESSAGE_MAP(DSOSConfiguration, CAbstractFormDoc)
	//{{AFX_MSG_MAP(DSOSConfiguration)
	ON_COMMAND			(ID_SOS_CONFIGURATION_RELOAD_DOC_CLASSES,	OnToolbarReloadDocClasses)
	ON_UPDATE_COMMAND_UI(ID_SOS_CONFIGURATION_RELOAD_DOC_CLASSES,	OnUpdateToolbarReloadDocClasses)
	ON_EN_VALUE_CHANGED	(IDC_SOS_CONFIGURATION_FTP_SEND,			OnEnableFTPSend)
	ON_EN_VALUE_CHANGED	(IDC_SOS_CONFIGURATION_SUBJECT,				OnSubjectCodeChanged)
	ON_EN_VALUE_CHANGED	(IDC_SOS_CONFIGURATION_MYSOS_USER,			OnMySOSUserChanged)
	ON_EN_VALUE_CHANGED	(IDC_SOS_CONFIGURATION_MYSOS_PASSWORD,		OnMySOSPasswordChanged)
	ON_EN_VALUE_CHANGED	(IDC_SOS_CONFIGURATION_SOS_URL,				OnSOSUrlChanged)
	//}}AFX_MSG_MAP
END_MESSAGE_MAP()

//-----------------------------------------------------------------------------
VSOSConfiguration* DSOSConfiguration::GetVSOSConfiguration() const { return (VSOSConfiguration*)m_pDBTSOSConfiguration->GetRecord(); }

//-----------------------------------------------------------------------------
DSOSConfiguration::DSOSConfiguration()
	:
	m_pDBTSOSConfiguration	(NULL),
	m_pDBTSOSDocClasses		(NULL),
	m_pSOSConfiguration		(NULL)
{
	DisableFamilyClientDoc(TRUE);
}

//-----------------------------------------------------------------------------
DSOSConfiguration::~DSOSConfiguration()
{
	SAFE_DELETE(m_pDBTSOSDocClasses);
}

//-----------------------------------------------------------------------------
BOOL DSOSConfiguration::OnAttachData()
{ 
	SetFormTitle(_TB("SOS Configuration"));
	
	SetOnlyOneRecord(TRUE);

	m_pDBTSOSConfiguration	= new DBTSOSConfiguration(RUNTIME_CLASS(VSOSConfiguration), this);
	m_pDBTSOSDocClasses		= new DBTSOSDocClasses(RUNTIME_CLASS(VSOSDocClass), this);

	Attach(m_pDBTSOSConfiguration);
	m_pDBTSOSConfiguration->FindData();
	SaveCurrentRecord();
	
	m_pDBTSOSDocClasses->LocalFindData(FALSE);

	return TRUE;
}

//-----------------------------------------------------------------------------
BOOL DSOSConfiguration::CanRunDocument()
{
	if (!AfxGetOleDbMng()->EasyAttachmentEnable())
	{
		AfxMessageBox(_TB("Impossible to open DMS SOS configuration form!\r\nPlease, check in Administration Console if this company uses DMS."));
		return FALSE;
	}
	
	if (!AfxGetTbRepositoryManager()->GetDMSSettings()->GetSettings()->l_EnableSOS)
	{
		AfxMessageBox(_TB("You are not allowed to open SOS Configuration form! In order to open it change your setting parameter."));
		return FALSE;
	}

	return TRUE;
}

//-----------------------------------------------------------------------------
BOOL DSOSConfiguration::OnOkTransaction()
{
	if (!CanSaveParameters())
	{
		AfxMessageBox(_TB("Before saving you have to close all other opened documents."));
		return FALSE;
	}

	if (!CheckInputData())
		return FALSE;

	return CAbstractFormDoc::OnOkTransaction();
}

//-----------------------------------------------------------------------------
BOOL DSOSConfiguration::CanDoEditRecord()
{
	return CAbstractFormDoc::CanDoEditRecord() && AfxGetLoginInfos()->m_bAdmin;
}

//-----------------------------------------------------------------------------
void DSOSConfiguration::OnPropertyCreated(CTBProperty* pProperty)
{
	CParsedCtrl* pCtrl = pProperty->GetControl();
	UINT nIDC = pCtrl ? pCtrl->GetCtrlID() : 0;

	if (nIDC == IDC_SOS_CONFIGURATION_CHUNK_DIMENSION)
	{
		CLongEdit* pLongEdit = (CLongEdit*)pProperty->GetControl();
		if (pLongEdit)
			pLongEdit->SetRange(5, 100);
		return;
	}

	if (nIDC == IDC_SOS_CONFIGURATION_ENVELOPE_DIMENSION)
	{
		CLongEdit * pLongEdit = (CLongEdit *)pProperty->GetControl();
		if (pLongEdit)
			pLongEdit->SetRange(200, 1000);
		return;
	}

	if (nIDC == IDC_SOS_CONFIGURATION_FTP_SHARED_FOLDER)
	{
		CBrowsePathEdit* pPathEdit = dynamic_cast<CBrowsePathEdit*>(pCtrl);
		if (pPathEdit)
			pPathEdit->SetCtrlStyle(pPathEdit->GetCtrlStyle() | PATH_STYLE_AS_PATH);
		return;
	}
}

//-----------------------------------------------------------------------------
BOOL DSOSConfiguration::CheckInputData()
{
	VSOSConfiguration* pRec = GetVSOSConfiguration();
	if (!pRec)
		return FALSE;

	if (pRec->l_SubjectCode.IsEmpty() || pRec->l_SOSWebServiceUrl.IsEmpty() || pRec->l_KeeperCode.IsEmpty() || pRec->l_MySOSUser.IsEmpty() || pRec->l_MySOSPassword.IsEmpty())
	{
		Message(_TB("You have to specify the information about subject, user, password and SOS Web Service URL first."), MB_ICONSTOP);
		return FALSE;
	}

	// in questo modo evito di salvare il nome utente con degli spazi in fondo a destra
	// (rilevato successivo errore di spedizione con Fabio in data 27/11/17)
	CString sosUserTrimmed = pRec->l_MySOSUser.Rtrim();
	pRec->l_MySOSUser = sosUserTrimmed;

	if (m_pDBTSOSDocClasses->GetRowCount() < 1)
	{
		Message(_TB("No document classes have been loaded. Please check your SOS credentials."), MB_ICONSTOP);
		return FALSE;
	}

	if (pRec->l_FTPSend)
	{
		if (pRec->l_FTPSharedFolder.IsEmpty())
		{
			Message(_TB("Please specify a path for FTP shared folder."), MB_ICONSTOP);
			return FALSE;
		}

		if (!ExistPath(pRec->l_FTPSharedFolder))
		{
			Message(_TB("The specified folder for FTP does not exists. Please choose a valid path."), MB_ICONSTOP);
			return FALSE; 
		}

		if (!CheckDirectoryAccess(pRec->l_FTPSharedFolder))
		{
			Message(_TB("The specified folder for FTP does not have the Read/Write permissions. Please check these settings."), MB_ICONSTOP);
			return FALSE;
		}
	}

	return TRUE;
}

//-----------------------------------------------------------------------------
BOOL DSOSConfiguration::LoadSOSDocumentClasses()
{
	VSOSConfiguration* pRec = GetVSOSConfiguration();

	// le classi documentali le posso caricare solo se sono presenti i dati per la connessione
	if (pRec->l_SubjectCode.IsEmpty() || pRec->l_SOSWebServiceUrl.IsEmpty() || pRec->l_KeeperCode.IsEmpty() || pRec->l_MySOSUser.IsEmpty() || pRec->l_MySOSPassword.IsEmpty())
		return FALSE;

	m_pSOSConfiguration->m_pRecSOSConfiguration->l_ParamID				= pRec->l_ParamID;
	m_pSOSConfiguration->m_pRecSOSConfiguration->l_SubjectCode			= pRec->l_SubjectCode;
	m_pSOSConfiguration->m_pRecSOSConfiguration->l_AncestorCode			= pRec->l_AncestorCode;
	m_pSOSConfiguration->m_pRecSOSConfiguration->l_KeeperCode			= pRec->l_KeeperCode;
	m_pSOSConfiguration->m_pRecSOSConfiguration->l_MySOSUser			= pRec->l_MySOSUser;
	m_pSOSConfiguration->m_pRecSOSConfiguration->l_MySOSPassword		= pRec->l_MySOSPassword;
	m_pSOSConfiguration->m_pRecSOSConfiguration->l_SOSWebServiceUrl		= pRec->l_SOSWebServiceUrl;
	m_pSOSConfiguration->m_pRecSOSConfiguration->l_ChunkDimension		= pRec->l_ChunkDimension;
	m_pSOSConfiguration->m_pRecSOSConfiguration->l_EnvelopeDimension	= pRec->l_EnvelopeDimension;
	m_pSOSConfiguration->m_pRecSOSConfiguration->l_FTPSend				= pRec->l_FTPSend;
	m_pSOSConfiguration->m_pRecSOSConfiguration->l_FTPSharedFolder		= pRec->l_FTPSharedFolder;
	m_pSOSConfiguration->m_pRecSOSConfiguration->l_FTPUpdateDayOfWeek	= pRec->l_FTPUpdateDayOfWeek;

	AfxGetApp()->BeginWaitCursor();

	// carico l'elenco delle classi documentali
	BOOL bLoaded = AfxGetTbRepositoryManager()->LoadSOSDocumentClasses();

	AfxGetApp()->EndWaitCursor();

	if (bLoaded)
		m_pDBTSOSDocClasses->LocalFindData(FALSE); 	
	else
		m_pDBTSOSDocClasses->RemoveAll();

	UpdateDataView();

	return bLoaded;
}

//-----------------------------------------------------------------------------
void DSOSConfiguration::ClearSOSDocumentClasses()
{
	m_pDBTSOSDocClasses->RemoveAll();
	UpdateDataView();
}

//-----------------------------------------------------------------------------
void DSOSConfiguration::OnToolbarReloadDocClasses()
{
	if (CheckInputData())
		LoadSOSDocumentClasses();
}

//-----------------------------------------------------------------------------
void DSOSConfiguration::OnUpdateToolbarReloadDocClasses(CCmdUI* pCmdUI)
{
	pCmdUI->Enable(GetFormMode() == EDIT);
}

//-----------------------------------------------------------------------------
void DSOSConfiguration::OnEnableFTPSend()
{
	GetVSOSConfiguration()->l_FTPSharedFolder.SetReadOnly(!GetVSOSConfiguration()->l_FTPSend);
	GetVSOSConfiguration()->l_FTPUpdateDayOfWeek.SetReadOnly(!GetVSOSConfiguration()->l_FTPSend);
	UpdateDataView();
}

//-----------------------------------------------------------------------------
void DSOSConfiguration::OnSOSUrlChanged()
{
	VSOSConfiguration* pRec = GetVSOSConfiguration();

	if (pRec->l_SOSWebServiceUrl.IsEmpty() || AfxGetBaseApp()->IsCtrlDataChanged())
	{
		// se l'url e' variata faccio la Clear delle credenziali
		pRec->l_MySOSUser.Clear();
		pRec->l_MySOSPassword.Clear();
		ClearSOSDocumentClasses();
	}
}

//-----------------------------------------------------------------------------
void DSOSConfiguration::OnSubjectCodeChanged()
{
	VSOSConfiguration* pRec = GetVSOSConfiguration();
	// il codice soggetto padre e' uguale al codice soggetto
	pRec->l_AncestorCode = pRec->l_SubjectCode;

	if (AfxGetBaseApp()->IsCtrlDataChanged())
	{
		// se il codice soggetto e' variato faccio la Clear delle credenziali
		pRec->l_MySOSUser.Clear();
		pRec->l_MySOSPassword.Clear();
		ClearSOSDocumentClasses();
	}

	LoadSOSDocumentClasses();
}

//-----------------------------------------------------------------------------
void DSOSConfiguration::OnMySOSUserChanged()
{
	VSOSConfiguration* pRec = GetVSOSConfiguration();

	if (AfxGetBaseApp()->IsCtrlDataChanged())
	{
		// se il codice soggetto e' variato faccio la Clear della password
		pRec->l_MySOSPassword.Clear();
		ClearSOSDocumentClasses();
	}

	LoadSOSDocumentClasses();
}

//-----------------------------------------------------------------------------
void DSOSConfiguration::OnMySOSPasswordChanged()
{
	LoadSOSDocumentClasses();
}