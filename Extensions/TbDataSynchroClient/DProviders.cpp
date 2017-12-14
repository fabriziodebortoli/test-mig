#include "stdafx.h"

#include <TbFrameworkImages\CommonImages.h>
#include <TbFrameworkImages\GeneralFunctions.h>

#include "DSManager.h"
#include "DProviders.h"
#include "UIProviders.h"
#include "UIProviders.hjson"

static TCHAR szP1[] = _T("P1");

static TCHAR szNamespace[] = _T("Image.Framework.TbFrameworkImages.Images.%s.%s.png");

static TCHAR szGlyphF[] = _T("Glyph");

//////////////////////////////////////////////////////////////////////////////
//									DBTProviders                          //
//////////////////////////////////////////////////////////////////////////////
//-----------------------------------------------------------------------------	
IMPLEMENT_DYNAMIC (DBTProviders, DBTMaster)

//-----------------------------------------------------------------------------	
DBTProviders::DBTProviders
	(
		CRuntimeClass*		pClass, 
		CAbstractFormDoc*	pDocument
	)
	:
	DBTMaster (pClass, pDocument, _NS_DBT("Providers"))
{
}

//-----------------------------------------------------------------------------
void DBTProviders::OnDefineQuery ()
{
	m_pTable->SelectAll			();
	m_pTable->AddParam       	(szP1, GetProvider()->f_Name);
	m_pTable->AddFilterColumn	(GetProvider()->f_Name);
}

//-----------------------------------------------------------------------------
void DBTProviders::OnPrepareQuery ()
{
	if (!GetDocument()->m_ProviderName.IsEmpty())
		m_pTable->SetParamValue(szP1, GetDocument()->m_ProviderName);
	else
		m_pTable->SetParamValue(szP1, GetProvider()->f_Name);
}

//-----------------------------------------------------------------------------
BOOL DBTProviders::OnCheckPrimaryKey()
{
	return TRUE;
}

//-----------------------------------------------------------------------------
void DBTProviders::OnPreparePrimaryKey()
{
}

//--------------------------------------------------------------------------------
void DBTProviders::OnDisableControlsForEdit()
{
	GetProvider()->f_Name.SetReadOnly();	
	GetProvider()->f_Description.SetReadOnly();	
}

//////////////////////////////////////////////////////////////////////////////
//            			class DBTVProviderParams implementation
//////////////////////////////////////////////////////////////////////////////
//-----------------------------------------------------------------------------	
IMPLEMENT_DYNAMIC(DBTVProviderParams, DBTSlaveBuffered)

//-----------------------------------------------------------------------------	
DBTVProviderParams::DBTVProviderParams
	(
		CRuntimeClass*		pClass, 
		CAbstractFormDoc*	pDocument
	)
	:
	DBTSlaveBuffered (pClass, pDocument, _NS_DBT("ProvidersParameters"), ALLOW_EMPTY_BODY, FALSE)
{
}

// Serve a definire sia i criteri di sort (ORDER BY chiave primaria in questo caso)
// ed i criterio di filtraggio (WHERE)
// La routine parent deve essere chiamata perche inizializza il vettore di parametri
//-----------------------------------------------------------------------------
void DBTVProviderParams::OnDefineQuery ()
{
	m_pTable->SelectAll			();
}

// Serve a valorizzare i parametri di query. 
//-----------------------------------------------------------------------------
void DBTVProviderParams::OnPrepareQuery ()
{
}

//-----------------------------------------------------------------------------	
DataObj* DBTVProviderParams::OnCheckPrimaryKey(int /*nRow*/, SqlRecord* /*pRec*/)
{ 
    return NULL;
}

// Mettere qui eventuali inizializzazioni di segmenti di chiave primaria non 
// inseriti dall'utente ma precalcolati
//-----------------------------------------------------------------------------
void DBTVProviderParams::OnPreparePrimaryKey(int nRow, SqlRecord* pRec)
{
}

//-----------------------------------------------------------------------------
DataObj* DBTVProviderParams::OnCheckUserData(int nRow)
{
	return NULL;
}

//-----------------------------------------------------------------------------
void DBTVProviderParams::OnDisableControlsForEdit()
{
}

//////////////////////////////////////////////////////////////////////////////
//									DProviders                          //
//////////////////////////////////////////////////////////////////////////////
//
//-----------------------------------------------------------------------------
IMPLEMENT_DYNCREATE(DProviders, CAbstractFormDoc)
//-----------------------------------------------------------------------------
BEGIN_MESSAGE_MAP(DProviders, CAbstractFormDoc)
	//{{AFX_MSG_MAP(DProviders)
//}}AFX_MSG_MAP
	ON_EN_VALUE_CHANGED(IDC_PROVIDERS_DISABLED, OnDisableChanged)
END_MESSAGE_MAP()

//-----------------------------------------------------------------------------
TDS_Providers* DProviders::GetProvider () const { return (TDS_Providers*) m_pDBTProviders->GetRecord(); }

//-----------------------------------------------------------------------------
DProviders::DProviders()
	: 
	m_pProvider(NULL),
	m_pDBTProviderParameters(NULL),
	m_bDisabledChanged (FALSE)
{
	
}

//-----------------------------------------------------------------------------
DProviders::~DProviders()
{   
	if (m_pDBTProviderParameters)
		delete m_pDBTProviderParameters;
}

//-----------------------------------------------------------------------------
BOOL DProviders::OnAttachData()
{           
	SetFormTitle (_TB("Synchronization Providers"));
	m_pDBTProviders	= new DBTProviders (RUNTIME_CLASS (TDS_Providers), this);
	m_pDBTProviderParameters = new DBTVProviderParams(RUNTIME_CLASS(VProviderParams), this);

	//manage json variables
	DECLARE_VAR_JSON(ProviderStatus);
	DECLARE_VAR_JSON(ProviderStatusImage);

	return Attach (m_pDBTProviders);
}

//-----------------------------------------------------------------------------
BOOL DProviders::OnPrepareAuxData()
{   
	m_pDBTProviderParameters->RemoveAll(TRUE);
	m_pProvider = AfxGetDataSynchroManager()->GetSynchroProviders()->GetProvider(GetProvider()->f_Name);
	if (!m_pProvider)
		return TRUE;

	if (m_pProvider->m_pParameters)
	{
		m_pProvider->ParseValuesFromXMLString(GetProvider()->f_ProviderParameters);
		for (int i = 0; i < m_pProvider->m_pParameters->GetSize(); i++)
		{
			VProviderParams* pVParam = (VProviderParams*)m_pDBTProviderParameters->AddNewRecord(FALSE);
			*pVParam = *((VProviderParams*)m_pProvider->m_pParameters->GetAt(i));
		}
	}
	BOOL bTestOK = FALSE;
	CString strMessage;	
	DoTestConnection(strMessage);	
	m_bDisabledChanged = GetProvider()->f_Disabled == TRUE;
	return TRUE;
}

//-----------------------------------------------------------------------------
BOOL DProviders::CanRunDocument()
{    
	if (AfxIsActivated(TBEXT_APP, _NS_ACT("DataSynchroFunctionality")) && 
		AfxGetLoginInfos()->m_bDataSynchro && 
        AfxGetIDataSynchroManager())
		return TRUE;

	AfxMessageBox(_TB("The company in use is not enabled to the exchange data connector. Open Administration Console and check 'Use exchange data connector' checkbox in the company properties."));
	return FALSE; 
}

//-----------------------------------------------------------------------------
BOOL DProviders::OnOkTransaction()
{
	if (m_pProvider)
	{
		for (int i = 0; i < m_pProvider->m_pParameters->GetSize(); i++)
		{
			VProviderParams* pVParam = m_pDBTProviderParameters->GetParameter(i);
			 *((VProviderParams*)m_pProvider->m_pParameters->GetAt(i)) = *pVParam;
		}
		// compongo la stringa xml con i parametri
		GetProvider()->f_ProviderParameters = m_pProvider->UnparseValuesToXMLString();
	}
		
	CString strMessage;
	if (!DoTestConnection(strMessage))
	{
		if (!strMessage.IsEmpty())
			Message(strMessage);

		return FALSE;
	}

	if (!GetProvider()->f_Disabled && GetProvider()->f_Name.CompareNoCase(CRMInfinityProvider) == 0)
	{
		CString aMsg;
		CString strBuildVersion = AfxGetLoginManager()->GetMasterProductBrandedName() + _T(";") + AfxGetLoginManager()->GetInstallationVersion();
		
		BOOL isOk = AfxGetIDataSynchroManager()->CheckVersion(
			CRMInfinityProvider,
			strBuildVersion,
			aMsg
		);
		if (!isOk)
		{
			CString strErrMsg = cwsprintf(_TB("Warning! Mago version is not compatible with Infinity patch number: {0-%s}"), aMsg);
			Message(strErrMsg);

		}
	}
	return TRUE;
}

//-----------------------------------------------------------------------------
BOOL DProviders::OnOkEdit()
{
	if (m_pProvider && m_pProvider->m_Name.CompareNoCase(DMSInfinityProvider) == 0 )
	{
		//se sto per editare il DMSInfinity allora verifico prima che CRMInfinity sia valido oppure disabilitato
		if (!m_pProvider->m_pParentSynchroProvider->IsValid())
		{
			Message(cwsprintf(_TB("Before edit data for this provider, you must enable or insert the correct information for the provider {0-%s}."), m_pProvider->m_pParentSynchroProvider->m_Description));
			return FALSE;
		}
	}

	return TRUE;
}

//-----------------------------------------------------------------------------
BOOL DProviders::SetProviderParameters()
{
	//se è cambiato qualcosa devo mandare le informazioni al DataSynchronizer
	CString strMessage = _T("");
	BOOL bIsOk = FALSE;

	TDS_Providers* pRec = GetProvider();
	if (pRec == NULL)
		return bIsOk;

	bIsOk = AfxGetDataSynchroManager()->SetProviderParameters
										(
											pRec->f_Name,
											pRec->f_ProviderUrl,
											pRec->f_ProviderUser,
											pRec->f_ProviderPassword,
											pRec->f_SkipCrtValidation,
											pRec->f_IAFModules,
											pRec->f_ProviderParameters,
											strMessage
										);
	if (!strMessage.IsEmpty())
		Message(strMessage);

	return bIsOk;
}

//-----------------------------------------------------------------------------
void DProviders::SetEnabledDMSProvider()
{
	TDS_Providers* pRec = GetProvider();
	if (pRec == NULL)
		return;

	//se il provider è il CRMInfinity devo verificare la sua validità. Se non valido allora metto invalido anche il DMSInfinity.
	//se CRMInfinity è disabilitato allora devo disabilitare anche il DMSInfinity
	if (m_pProvider && m_pProvider->m_Name.CompareNoCase(CRMInfinityProvider) == 0 && (!m_pProvider->IsValid() || pRec->f_Disabled))
	{
		CSynchroProvider* pDMSProvider = AfxGetDataSynchroManager()->GetSynchroProviders()->GetProvider(DMSInfinityProvider);
		if (pDMSProvider)
		{
			pDMSProvider->SetValid(FALSE);
			TUDS_Providers aProviderTU;
			aProviderTU.SetSqlSession(GetUpdatableSqlSession());
			aProviderTU.SetAutocommit();

			if (aProviderTU.FindRecord(DMSInfinityProvider, TRUE) == TableUpdater::FOUND)
			{
				aProviderTU.GetRecord()->f_Disabled = TRUE;
				aProviderTU.UpdateRecord();
			}
		}
	}

}

//-----------------------------------------------------------------------------
void DProviders::OnExtraEditTransaction()
{
	TDS_Providers* pRec = GetProvider();
	if (pRec == NULL)
		return;

	BOOL bIsOk = SetProviderParameters();
	m_pProvider->SetValid(bIsOk && !pRec->f_Disabled);

	SetEnabledDMSProvider();

	TRY
	{
		if (pRec->f_Disabled || !AfxGetIDataSynchroManager()->NeedMassiveSynchro(pRec->f_Name))
			return;
	}
	CATCH(SqlException, e)
	{
		return;
	}
	END_CATCH

	CString msg = _TB("Do you want to open the Massive Data Synchronization procedure that is next step in the synchronization process? ");
	if (!IsInUnattendedMode() && AfxMessageBox(msg, MB_YESNO | MB_ICONQUESTION | MB_DEFBUTTON2) == IDNO)
		return;

	CAbstractFormDoc* pDoc;
	if (pRec->f_Name == InfiniteCRMProvider)
		pDoc = (CAbstractFormDoc*)AfxGetTbCmdManager()->RunDocument	(
																		_NS_DOC("ERP.SynchroConnector.Services.InfiniteCRMMassiveSynchro"),
																		szDefaultViewMode,
																		FALSE,
																		NULL
																	);
	else
		pDoc = (CAbstractFormDoc*)AfxGetTbCmdManager()->RunDocument	(
																		_NS_DOC("ERP.SynchroConnector.Services.CRMInfinityMassiveSynchro"),
																		szDefaultViewMode,
																		FALSE,
																		NULL
																	);
	CloseDocument();
}

//-----------------------------------------------------------------------------
BOOL DProviders::DoTestConnection(CString& strMessage)
{
	BOOL bConnectionOK = TRUE;	

	BeginWaitCursor();
	TDS_Providers* pRec = GetProvider();
	if (pRec == NULL)
		return FALSE;

	if (!pRec->f_ProviderUrl.IsEmpty() && !pRec->f_Disabled)
	{	
		TCHAR bufferOk[512];
		int nResult1 = swprintf_s(bufferOk, szNamespace, szGlyphF, szGlyphOk);
		
		TCHAR bufferErr[512];
		int nResult2 = swprintf_s(bufferErr, szNamespace, szGlyphF, szIconError);
		
		bConnectionOK = AfxGetDataSynchroManager()->TestProviderParameters(pRec->f_Name, pRec->f_ProviderUrl, pRec->f_ProviderUser, pRec->f_ProviderPassword, 
			pRec->f_SkipCrtValidation, pRec->f_ProviderParameters, strMessage, TRUE);
		
		m_ProviderStatusImage = bConnectionOK ? bufferOk : bufferErr;	//bConnectionOK ? TBGlyph(szGlyphOk) : TBGlyph(szGlyphError);
		CString strConn = cwsprintf(_TB("The connection to {0-%s} is "), pRec->f_ProviderUrl.Str());
		m_ProviderStatus = !bConnectionOK ? cwsprintf(_TB("{0-%s} invalid due to the following error: {1-%s}"), strConn, strMessage): cwsprintf(_TB("{0-%s} valid"), strConn);
	}
	else
	{
		m_ProviderStatusImage.Clear();
		m_ProviderStatus.Clear();
	}
	UpdateDataView();

	EndWaitCursor();

	return bConnectionOK;
}

//-------------------------------------------------------------------------------------
void DProviders::OnUpdateTitle(CTileDialog* pTileDialog)
{
	CString sTitle = _T("");

	if (!GetProvider())
	{
		pTileDialog->SetCollapsedDescription(_T(""));
		return;
	}

	if (pTileDialog->GetNamespace().GetObjectName() == _NS_TILEDLG("MainDataTileDlg"))
	{
		sTitle = GetProvider()->f_Name.FormatData() + (!GetProvider()->f_Description.IsEmpty() ? _T(" - ") + GetProvider()->f_Description.FormatData() : _T(""));
		
		if (!sTitle.IsEmpty())
			pTileDialog->SetCollapsedDescription(cwsprintf(_TB("Provider Name: {0-%s}"), sTitle));
	}

	if (pTileDialog->GetNamespace().GetObjectName() == _NS_TILEDLG("ConnectionDataTileDlg"))
	{
		sTitle = GetProvider()->f_ProviderUrl;

		if (!sTitle.IsEmpty())
			pTileDialog->SetCollapsedDescription(cwsprintf(_TB("URL: {0-%s}"), sTitle));
	}
}
//-------------------------------------------------------------------------------------
BOOL DProviders::OnOpenDocument(LPCTSTR param)
{
	if (param)
	{
		m_ProviderName = *(DataStr*)GET_AUXINFO(param);
	}
	return __super::OnOpenDocument(param);
}

//-------------------------------------------------------------------------------------
void DProviders::OnDisableChanged()
{
	TDS_Providers * pRec = GetProvider();
	
	if (!pRec->f_Disabled)
		m_bDisabledChanged = TRUE;
}