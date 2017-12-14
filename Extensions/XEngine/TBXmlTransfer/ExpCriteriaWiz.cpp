#include "stdafx.h" 

#include <TbClientCore\ClientObjects.h>
#include <TbWebServicesWrappers\LoginManagerInterface.h>

#include <TBGENLIB\baseapp.h>

#include <TBGES\DBT.H>
#include <TBGES\XMLGesInfo.h>
#include <TBGES\ExtDocView.h>
#include <TBGES\Tabber.h>
#include <TBGES\BARQUERY.H>
#include <TBGES\bodyedit.h>
#include <TBGES\extdoc.h>

#include <TBWoormEngine\ActionsRepEngin.h>
#include <TBWoormEngine\askdata.h>
#include <TBWoormEngine\inputmng.h>
#include <TBWoormEngine\prgdata.h>
#include <TBWoormEngine\askdlg.h>
#include <TBWoormEngine\reptable.h>

#include <XEngine\TBXMLEnvelope\XEngineObject.h>

#include "XMLDataMng.h"
#include "ExpCriteriaWiz.h"
#include "ExpCriteriaDlg.h"
#include "XMLProfileInfo.h"
#include "ExpCriteriaDlg.hjson"
#include "GenFunc.h"



//////////////////////////////////////////////////////////////////////////////////
// 
//		CExpCriteriaWizardDoc implementation
//
//////////////////////////////////////////////////////////////////////////////////
IMPLEMENT_DYNCREATE(CExpCriteriaWizardDoc, CWizardFormDoc)

//------------------------------------------------------------------
CExpCriteriaWizardDoc::CExpCriteriaWizardDoc()
:
	m_pPreferences		(NULL),
	m_pExportCriteria	(NULL),
	m_pXMLExportDocSel	(NULL),
	m_pCurrentProfile	(NULL),
	m_pDataManagerExport(NULL),
	m_bSaveCriteria		(TRUE)
{
	m_Type = VMT_BATCH;	// TODOBRUNA???serve ?

	DisableFamilyClientDoc(TRUE);
	DisableLoadXMLDocInfo();
	UseAutoExpression(TRUE);
}

//---------------------------------------------------------------------------
BOOL CExpCriteriaWizardDoc::OnOpenDocument(LPCTSTR pParam)
{
    if (pParam)
        m_pDataManagerExport = (CXMLDataManager*) GET_AUXINFO(pParam);

	if (m_pDataManagerExport)
	{
		m_pXMLExportDocSel = m_pDataManagerExport->GetXMLExportDocSelection();	
		m_pDataManagerExport->SetContinueImportExport(FALSE);
	}
	
	if (AfxGetDynamicInstancePath().IsEmpty())
	{
		if (!IsInUnattendedMode())
			AfxMessageBox(_TB("Unable to create path for export process.\r\nCheck export parameters."));
		return FALSE;
	}

    return CAbstractFormDoc::OnOpenDocument(pParam);
}

//------------------------------------------------------------------------------
HotKeyLink* CExpCriteriaWizardDoc::GetHotLink(const CString& sName, const CTBNamespace& aNameSpace /*= CTBNamespace(_T(""))*/)
{
	//prima lo cerco nel mio documento, senza passare il namespace così non lo crea se non lo trova
	HotKeyLink* pHotKeyLink = __super::GetHotLink(sName);
	if (pHotKeyLink)
		return pHotKeyLink;
	if (m_pExportCriteria && m_pExportCriteria->m_pDoc)
	{
		//poi lo cerco nel documento chiamante, senza passare il namespace così non lo crea se non lo trova
		pHotKeyLink = m_pExportCriteria->m_pDoc->GetHotLink(sName);

		//se non lo trovo, lo faccio creare al volo, ammesso che abbia il namespace valido
		if (!pHotKeyLink && !aNameSpace.IsEmpty())
			pHotKeyLink = m_pExportCriteria->m_pDoc->GetHotLink(sName, aNameSpace);
	}
	//se non lo trovo, lo faccio creare al volo al documento chiamante, ammesso che abbia il namespace valido
	if (!pHotKeyLink && !aNameSpace.IsEmpty())
		pHotKeyLink = __super::GetHotLink(sName);
	return pHotKeyLink;
}
//---------------------------------------------------------------------------
BOOL CExpCriteriaWizardDoc::OnAttachData()
{
	SetFormTitle(_TB("Set export criteria"));

	return TRUE;
}

//---------------------------------------------------------------------------
CAbstractFormDoc* CExpCriteriaWizardDoc::GetExportedDocument() const 	
{
	CAbstractFormDoc* pCurrDoc =
						(
							m_pDataManagerExport	&&
							m_pDataManagerExport->GetDocument() && 
							m_pDataManagerExport->GetDocument()->IsKindOf(RUNTIME_CLASS(CAbstractFormDoc))
							) ? (CAbstractFormDoc*)m_pDataManagerExport->GetDocument() : NULL;

	return pCurrDoc;
}

//---------------------------------------------------------------------------
CXMLBaseAppCriteria* CExpCriteriaWizardDoc::GetBaseExportCriteria() const
{
	return  (GetExportedDocument())
			? GetExportedDocument()->GetBaseExportCriteria()
			: NULL; 
}

//---------------------------------------------------------------------------
void CExpCriteriaWizardDoc::UpdateCurrentProfile()
{
	if (m_pXMLExportDocSel)
		m_pCurrentProfile = m_pXMLExportDocSel ? m_pXMLExportDocSel->GetProfileInfo() : NULL;	

	if (m_pCurrentProfile && m_pXMLExportDocSel && !m_pXMLExportDocSel->m_strExpCriteriaFileName.IsEmpty())
	{
		m_pCurrentProfile->m_strExpCriteriaFileName = m_pXMLExportDocSel->m_strExpCriteriaFileName;
		if (!IsDriveName(m_pCurrentProfile->m_strExpCriteriaFileName))
			m_pCurrentProfile->m_strExpCriteriaFileName = GetExpCriteriaVarFile(m_pCurrentProfile->GetNamespaceDoc(), m_pCurrentProfile->GetName(), m_pCurrentProfile->m_strExpCriteriaFileName);
	}
	
	// se ho modificato il profilo devo poi riallocarmi i criteri
	m_pExportCriteria = NULL;
	m_pPreferences = NULL;		
}

//---------------------------------------------------------------------------
CXMLExportCriteria* CExpCriteriaWizardDoc::AllocNewExportCriteria()
{
	if (!m_pCurrentProfile)
	{
		if (m_pExportCriteria)
			delete 	m_pExportCriteria;
		m_pExportCriteria = NULL;
		return NULL;
	}
	// non devo crearmi una nuova istanza di criteri. Va bene quella creata prima
	if (
			m_pExportCriteria &&
			m_pCurrentProfile->GetName().CompareNoCase(m_pExportCriteria->GetProfileName()) == 0
		)
		return m_pExportCriteria;
	else
	{
		if (m_pExportCriteria)
			delete m_pExportCriteria;
		m_pExportCriteria = NULL;
	}

	CAbstractFormDoc* pCurrDoc = GetExportedDocument();	
	if 
		(
			m_pCurrentProfile->GetXMLExportCriteria() && 
			pCurrDoc	&&
			pCurrDoc->m_pDBTMaster
		)
	{
		m_pExportCriteria = new CXMLExportCriteria(*m_pCurrentProfile->GetXMLExportCriteria());
		m_pExportCriteria->SetExternalRecord(pCurrDoc->m_pDBTMaster->GetRecord());
	}
	else
		m_pExportCriteria = new CXMLExportCriteria(m_pCurrentProfile, pCurrDoc);
	
	if (!m_pExportCriteria)
	{
		ASSERT(FALSE);
		return NULL;
	}
	
	if (!m_pExportCriteria->m_pUserExportCriteria)
		m_pExportCriteria->m_pUserExportCriteria = new CUserExportCriteria(m_pExportCriteria);
	
	if (!m_pExportCriteria->m_pOSLExportCriteria)
		m_pExportCriteria->m_pOSLExportCriteria = new COSLExportCriteria;

	if (!m_pExportCriteria->m_pAppExportCriteria)
		m_pExportCriteria->m_pAppExportCriteria = new CAppExportCriteria(m_pExportCriteria);

	m_pCurrentProfile->SetCurrentXMLExportCriteria(m_pExportCriteria);
	m_pPreferences = m_pExportCriteria->GetPreferencesCriteria();

	if (m_pPreferences)
	{
		m_pPreferences->SetCriteriaModeOSL(m_pPreferences->IsCriteriaModeOSL()&& IsTracedDBTMasterTable());
		m_pPreferences->SetCriteriaModeApp(m_pPreferences->IsCriteriaModeApp() && (GetFirstAppTabIDD() != 0));

		CUserExportCriteria* pUsrExpCriteria = GetUserExportCriteria();
		m_pPreferences->SetCriteriaModeUsr
							(
								m_pPreferences->IsCriteriaModeUser() &&
								pUsrExpCriteria &&
								pUsrExpCriteria->m_pQueryInfo &&
								(
									!pUsrExpCriteria->m_pQueryInfo->m_TableInfo.m_strFilter.IsEmpty() || 
									!pUsrExpCriteria->m_pQueryInfo->m_TableInfo.m_strSort.IsEmpty()
								)
							);	
	}
		
	return m_pExportCriteria;
}

//-----------------------------------------------------------------------------
BOOL CExpCriteriaWizardDoc::CheckDate() const
{
	COSLExportCriteria* pOSLExportCriteria = GetOSLExportCriteria();

	if (!pOSLExportCriteria || pOSLExportCriteria->m_ToDate < pOSLExportCriteria->m_FromDate)
	{
		AfxGetBaseApp()->SetError(_TB("End date of tracking period cannot be prior to start date."));
		return FALSE;
	}
	return TRUE;
}
//-----------------------------------------------------------------------------
BOOL CExpCriteriaWizardDoc::IsTracedDBTMasterTable() const
{
	//verifico se la tabella é sotto tracciatura
	CString strTableName = (GetExportedDocument()->m_pDBTMaster)
							? GetExportedDocument()->m_pDBTMaster->GetTable()->GetTableName()
							: _T("");
	const SqlCatalogEntry* pEntry = GetExportedDocument()->GetSqlConnection()->GetCatalogEntry(strTableName);

	return (pEntry) ? pEntry->IsTraced() : FALSE;
}

//////////////////////////////////////////////////////////////////////////////////
// 
//		CExpCriteriaWizardFrame implementation
//
//////////////////////////////////////////////////////////////////////////////////
//
IMPLEMENT_DYNCREATE(CExpCriteriaWizardFrame, CWizardFrame)


//////////////////////////////////////////////////////////////////////////////////
// 
//		CExpCriteriaWizardView implementation
//
//////////////////////////////////////////////////////////////////////////////////
//
IMPLEMENT_DYNCREATE(CExpCriteriaWizardView, CWizardFormView)

//-----------------------------------------------------------------------------
BEGIN_MESSAGE_MAP(CExpCriteriaWizardView, CWizardFormView)
	ON_COMMAND	(IDC_WIZARD_FINISH,	OnWizardFinish)
	ON_COMMAND	(IDCANCEL,	OnWizardCancel)
END_MESSAGE_MAP() 


//-----------------------------------------------------------------------------
CExpCriteriaWizardDoc* CExpCriteriaWizardView::GetExpCriteriaWizardDoc() const
{
	ASSERT(GetDocument() && GetDocument()->IsKindOf(RUNTIME_CLASS(CExpCriteriaWizardDoc)));
	return (CExpCriteriaWizardDoc*)m_pDocument; // GetDocument();
}

//-----------------------------------------------------------------------------
void CExpCriteriaWizardView::CustomizeTabWizard(CTabManager* pTabWizard)
{    
	if (!pTabWizard)
	{
		ASSERT(FALSE);
		return;
	}

	pTabWizard->AddDialog(RUNTIME_CLASS(CXMLExportPresentationPage),IDD_WIZARD_PRESENTATION_PAGE);
	pTabWizard->AddDialog(RUNTIME_CLASS(CXMLExportSchemaPage),		IDD_WIZARD_SCHEMA_PAGE);
	pTabWizard->AddDialog(RUNTIME_CLASS(CXMLExportSelDocPage),		IDD_WIZARD_SEL_DOC_PAGE);
	pTabWizard->AddDialog(RUNTIME_CLASS(CXMLExportSelProfilePage),	IDD_WIZARD_SEL_PROFILE_PAGE);
	pTabWizard->AddDialog(RUNTIME_CLASS(CXMLExportCriteriaPage),	IDD_WIZARD_SEL_CRITERIA_PAGE);
	pTabWizard->AddDialog(RUNTIME_CLASS(CXMLExportOSLCriteriaPage), IDD_WIZARD_SEL_AUDIT_PAGE);
	pTabWizard->AddDialog(RUNTIME_CLASS(CXMLExportUserCriteriaPage),IDD_WIZARD_SEL_USR_CRITERIA_PAGE);	
	pTabWizard->AddDialog(RUNTIME_CLASS(CXMLExportOptionPage),		IDD_WIZARD_OPTION_PAGE);	
	pTabWizard->AddDialog(RUNTIME_CLASS(CXMLExportSummaryPage),		IDD_WIZARD_SUMMARY_PAGE);
}

//-----------------------------------------------------------------------------
void CExpCriteriaWizardView::OnWizardFinish()
{
	CExpCriteriaWizardDoc*	pWizDoc = GetExpCriteriaWizardDoc();
	CXMLDataManager* pDataManager = pWizDoc ? pWizDoc->m_pDataManagerExport : NULL;

	if (!pDataManager)
	{
		ASSERT(FALSE);
		return;
	}

	// devo fare l'esportazione
	pDataManager->SetContinueImportExport(TRUE);
	CWizardFormView::OnWizardFinish();
}

//-----------------------------------------------------------------------------
void CExpCriteriaWizardView::OnWizardCancel()
{
	CExpCriteriaWizardDoc*	pWizDoc = GetExpCriteriaWizardDoc();
	CXMLDataManager* pDataManager = pWizDoc ? pWizDoc->m_pDataManagerExport : NULL;

	if (!pDataManager)
	{
		ASSERT(FALSE);
		return CWizardFormView::OnWizardCancel();;
	}
	// devo liberare la memoria senza effettuare l'esportazione
	pDataManager->SetContinueImportExport(FALSE);
	CWizardFormView::OnWizardCancel();
}

// Lo stato è rappresentato dai valori assegnati ai dati gestiti dal documento
//	ogni volta che si effettua il Next di una pagina:
//	1) viene controllato la validità delle informazioni inseriti (se necessario)
//	2) se tutto ok viene effettuato il next alla pagina richiesta
//  3) viene controllato lo stato per decidere se visualizzare o meno la pagina richiesta.
//	4) se non da visualizzare si effettua richiama ricorsivamente la next sulla pagina da non visualizzare

/* validità e next di ciascuna pagina del wizard:
	1) pagina di presentazione: IDD_WIZARD_PRESENTATION_PAGE. 
		Sempre da visualizzare e sempre validata
		NEXT: pagina di selezione del documento 
	
	2) pagina di selezione del documento: IDD_WIZARD_SEL_DOC_PAGE
		Sempre da visualizzare e sempre validata
		NEXT: pagina di scelta dei profili
	
	3) pagina di scelta dei profili:IDD_WIZARD_SEL_PROFILE_PAGE
		da visualizzare solo se: esiste un solo profilo di esportazione (che può essere il 
			predefinito o se non esiste la descrizione del documento)
		valida se l'utente ha scelto il profilo preferenziale oppure un profilo specifico tra
			quelli elencati nella combo
		NEXT: se è stato scelto di esportare un solo documento o tutti i documenti 
			  senza criteri di estrazione: pagina di sommario
			  altrimenti la pagina di scelta di utilizzo dei criteri (se impostarli o se scegliere quelli preferenziali)

	4) pagina di scelta dei criteri da utilizzare: IDD_WIZARD_SEL_CRITERIA_PAGE
		da visualizzare solo se esiste la possibilità di utilizzo di uno dei tre criteri stabiliti:
			Gestionali o predefiniti: sono quelli scelti dal programmatori ed associati alla classe del 
						documento da esportare. Attiva solo se implementati ed associati.
			Personalizzati: sono quelli creati dall'utente (con parametri o no). Attiva solo se l'utente 
						ha creato dei criteri dalla wizard dei profili
			OSL: criteri legati alla gestione di tracciatura di OSL. Attiva solo se caricato OSL
		NEXT: in ordine a seconda delle scelte effettuate:
			Tracciatura OSL:
			Criteri predefiniti (possono essere + pagine, viene attivata la prima pagina
			Criteri personalizzati
			Pagina di impostazione opzioni di esportazione
			Sommario

	5) pagina di impostazione valori per i criteri di tracciatura OSL: IDD_WIZARD_SEL_AUDIT_PAGE
		NEXT: in ordine a seconda delle scelte effettuate
			Criteri predefiniti
			Criteri personalizzati
			Pagina di impostazione opzioni di esportazione
			Sommario

	6) pagina di impostazione valori per i criteri predefiniti
		NEXT: in ordine a seconda dell'implementazione del programmatore e delle scelte effettuate
			Pagina successiva dei criteri predefiniti
			Criteri personalizzati
			Pagina di impostazione opzioni di esportazione
			Sommario
			
	7) pagina di impostazione valori preferenziali: IDD_WIZARD_SEL_USR_CRITERIA_PAGE
		da visualizzare: solo se l'utente ha da inserire dei parametri
		NEXT: 
			Pagina di impostazione opzioni di esportazione
			Sommario
	7) pagina di impostazione opzioni di esportazione:IDD_WIZARD_OPTION_PAGE 
		da visualizzare sempre
		NEXT: Sommario
	8) pagina di Sommario: IDD_WIZARD_SUMMARY_PAGE
*/

//-----------------------------------------------------------------------------
LRESULT CExpCriteriaWizardView::OnWizardNext(UINT nIDD)
{
	//CXMLExportDocSelection*	pDocSel = GetXMLExportDocSel();
	CExpCriteriaWizardDoc*	pWizDoc = GetExpCriteriaWizardDoc();
	CPreferencesCriteria*	pPreferencesCriteria = pWizDoc ? pWizDoc->GetPreferencesCriteria() : NULL;

	CAbstractFormDoc* pExportedDocument = pWizDoc ? pWizDoc->GetExportedDocument() : NULL;

	if (nIDD == IDD_WIZARD_PRESENTATION_PAGE)
	{
		if (CheckViewPage(IDD_WIZARD_SCHEMA_PAGE))
			return IDD_WIZARD_SCHEMA_PAGE;
		return OnWizardNext(IDD_WIZARD_SCHEMA_PAGE);
	}

	if (nIDD == IDD_WIZARD_SCHEMA_PAGE)
	{
		if (CheckViewPage(IDD_WIZARD_SEL_DOC_PAGE))
			return IDD_WIZARD_SEL_DOC_PAGE;
		return OnWizardNext(IDD_WIZARD_SEL_DOC_PAGE);
	}

	if (nIDD == IDD_WIZARD_SEL_DOC_PAGE)
	{
		if (CheckViewPage(IDD_WIZARD_SEL_PROFILE_PAGE))
			return IDD_WIZARD_SEL_PROFILE_PAGE;
		return OnWizardNext(IDD_WIZARD_SEL_PROFILE_PAGE);
	}

	if (nIDD == IDD_WIZARD_SEL_PROFILE_PAGE)
		return OnSelProfilePageWizardNext();

	if (nIDD == IDD_WIZARD_SEL_CRITERIA_PAGE)
		return OnSelCriteriaPageWizardNext();

	if (nIDD == IDD_WIZARD_SEL_AUDIT_PAGE)
	{
		if (
			pPreferencesCriteria &&
			pPreferencesCriteria->IsCriteriaModeOSL() &&
			!pWizDoc->CheckDate()
			)
			return WIZARD_SAME_TAB;
		if (
			pPreferencesCriteria &&
			pPreferencesCriteria->IsCriteriaModeUser() &&
			CheckViewPage(IDD_WIZARD_SEL_USR_CRITERIA_PAGE)
			)
			return IDD_WIZARD_SEL_USR_CRITERIA_PAGE;
		return OnWizardNext(IDD_WIZARD_SEL_USR_CRITERIA_PAGE);
	}

	if (nIDD == IDD_WIZARD_SEL_USR_CRITERIA_PAGE)
		return IDD_WIZARD_OPTION_PAGE;

	if (
		pWizDoc &&
		pPreferencesCriteria &&
		(nIDD == pWizDoc->GetLastAppTabIDD())
		)
	{
		if (!pPreferencesCriteria->IsCriteriaModeOSL())
			return OnWizardNext(IDD_WIZARD_SEL_AUDIT_PAGE);

		return IDD_WIZARD_SEL_AUDIT_PAGE;
	}


	return WIZARD_DEFAULT_TAB;
}


// metodo per controllare la visualizzazione della pagina
//-----------------------------------------------------------------------------
BOOL CExpCriteriaWizardView::CheckViewPage(UINT nIDD)
{
	CXMLExportDocSelection*	pDocSel = GetXMLExportDocSel();
	CExpCriteriaWizardDoc*	pWizDoc = GetExpCriteriaWizardDoc();

	CAbstractFormDoc* pExportedDocument = pWizDoc ? pWizDoc->GetExportedDocument() : NULL;

	if (nIDD == IDD_WIZARD_SCHEMA_PAGE)
	{
		if (pExportedDocument && pExportedDocument->IsEditingParamsFromExternalController())
			return FALSE;
		return TRUE;
	}
	if (nIDD == IDD_WIZARD_SEL_DOC_PAGE)
	{
		if (!pExportedDocument)
			return FALSE;

		if (pDocSel->IsOnlySchemaToExport() || pDocSel->IsSmartSchemaToExport())
			return FALSE;

		if
			(
			pDocSel &&
			pDocSel->IsOnlyCurrentDocToExport() &&
			!pExportedDocument->ValidCurrentRecord()
			)
			pDocSel->m_nDocSelType = EXPORT_DOC_SET;

		return TRUE;
	}

	if (nIDD == IDD_WIZARD_SEL_PROFILE_PAGE)
	{
		if (!pExportedDocument || !pDocSel)
			return FALSE;
		pDocSel->LoadAllProfile();
		return (pDocSel->m_aProfNamesArray.GetSize() > 0 ||
			(pDocSel->m_bExistPredefined && !pDocSel->m_strPreferredProfile.IsEmpty()));
	}

	if (nIDD == IDD_WIZARD_SEL_CRITERIA_PAGE)
		return	(
		pDocSel &&
		(pDocSel->IsOnlyDocToExport() || pDocSel->IsDocAndSchemaToExport()) &&
		pDocSel->MustUseCriteria() &&
		pWizDoc &&
		pWizDoc->m_pCurrentProfile &&
		ExistOneCriteria()
		);

	if (nIDD == IDD_WIZARD_SEL_USR_CRITERIA_PAGE)
		// visualizzo la maschera dei criteri personalizzati solo se ho dei parametri da inserire
	{
		CUserExportCriteria* pUserCrit = pWizDoc ? pWizDoc->GetUserExportCriteria() : NULL;
		return (
			pUserCrit &&
			pUserCrit->m_pQueryInfo &&
			pUserCrit->m_pQueryInfo->m_pPrgData &&
			(pDocSel->IsOnlyDocToExport() || pDocSel->IsDocAndSchemaToExport())
			);
	}


	return TRUE;
}
//-----------------------------------------------------------------------------
LRESULT CExpCriteriaWizardView::OnSelProfilePageWizardNext()
{
	CXMLExportDocSelection*	pDocSel			= GetXMLExportDocSel();
	CAbstractFormDoc*		pExportedDoc	= GetExportedDocument();
	CExpCriteriaWizardDoc*	pWizDoc			= GetExpCriteriaWizardDoc();
	
	if (!pWizDoc || !pExportedDoc || !pDocSel)
	{
		ASSERT(FALSE);
		return WIZARD_DEFAULT_TAB;
	}

	CAutoExpressionMng* pAutoExpressionMng = pWizDoc ? pWizDoc->m_pAutoExpressionMng : NULL;

	BOOL bMustUpdate = TRUE;
	
	CString strProfile;

	//se ho selezionato lo stesso profilo di prima non faccio niente
	if (pDocSel->IsSelectedProfileToUse())
	{
		if (pDocSel->m_strProfileName.IsEmpty())
		{
			AfxMessageBox(_TB("Enter the profile name to apply to the export."));
			return WIZARD_SAME_TAB;
		}
		CXMLProfileInfo* pExpProfile = pDocSel->GetProfileInfo();
		if	(
				pExpProfile &&
				(pExpProfile->GetName().CompareNoCase(pDocSel->m_strProfileName.GetString()) == 0)
			)
			bMustUpdate = !pWizDoc->m_pCurrentProfile;
	}

	
	// l'utente ha scelto il predefinito
	if (pDocSel->IsPreferredProfileToUse() && !pDocSel->m_strPreferredProfile.IsEmpty())
		// se non esiste il predefinito considero la descrizione del documento
		pDocSel->m_strProfileName  = ExistProfile(pExportedDoc->GetNamespace(), pDocSel->m_strPreferredProfile)
									? pDocSel->m_strPreferredProfile
									: _T("");
	
	// l'utente ha scelto il predefinito oppure il preferenziale ma il profilo non esiste
	if (pDocSel->IsPredefinedProfileToUse() || pDocSel->m_strProfileName.IsEmpty())
		// se non esiste il predefinito considero la descrizione del documento
		pDocSel->m_strProfileName  = ExistProfile(pExportedDoc->GetNamespace(), szPredefined)
									? szPredefined
									: _T("");	

	if (
			!pDocSel->m_strProfileName.IsEmpty() && 
			!ExistProfile(pExportedDoc->GetNamespace(), pDocSel->m_strProfileName.GetString())
		)
	{
		AfxMessageBox(_TB("Check that the file Document.xml for the selected profile exists."));
		return WIZARD_SAME_TAB;
	}

	
	// se sto generando uno schema XSD per Smart Documents devo controllare che il profilo predefinito non sia solo 
	// standard. Nell'eventualità lo sia e non ci sono altri profili custom da utilizzare non faccio proseguire con
	// la generazione
	if (pDocSel->IsSmartSchemaToExport() && !AfxGetBaseApp()->IsDevelopment())
	{
		if (pDocSel->m_strProfileName.IsEmpty())
		{
			AfxMessageBox(_TB("To create the schema file you need custom export profiles. These don't exist for the logged user."));
			return WIZARD_SAME_TAB;
		}
		CTBNamespace nsProfile;
		nsProfile.AutoCompleteNamespace(CTBNamespace::PROFILE, pDocSel->m_strProfileName.Str(), pExportedDoc->GetNamespace());
		CString strFileName = AfxGetPathFinder()->GetFileNameFromNamespace(nsProfile, AfxGetLoginInfos()->m_strUserName);
		CPathFinder::PosType ePosType = AfxGetPathFinder()->GetPosTypeFromPath(strFileName);
		if (ePosType == CPathFinder::STANDARD) 
		{
			AfxMessageBox(_TB("To create the schema file you need custom export profiles. These don't exist for the logged user."));
			return WIZARD_SAME_TAB;
		}           		
	}

	
	
	if (pDocSel->SetCurrentProfileInfo(pDocSel->m_strProfileName.GetString(), pAutoExpressionMng))
	{
		if (bMustUpdate)
			pWizDoc->UpdateCurrentProfile();

		if (pDocSel->m_pCurrentProfile && pDocSel->m_pCurrentProfile->IsTransformProfile())
		{
			//se è un profilo di trasformazione e non sto esportando i dati allora non faccio proseguire
			if (!pDocSel->IsOnlyDocToExport())
			{
				AfxMessageBox(_TB("Is is not possible to create a schema using a transformation profile.\r\nPlease select a different profile or a different type of export"));
				return WIZARD_SAME_TAB;
			}
		}

		if (pDocSel->IsSmartSchemaToExport() || !pDocSel->MustUseCriteria())
			return IDD_WIZARD_OPTION_PAGE;

		pWizDoc->AllocNewExportCriteria();

		// se non esiste nessun criterio
		// segnalo all'utente che la query di estrazione non fornirà nessun risultato
		// di scegliere un altro profilo
		if (!ExistOneCriteria())
		{
			AfxMessageBox(_TB("No extraction criteria available.\r\nSelect a profile with custom export criteria\r\nor select a different type of export"));
			return WIZARD_SAME_TAB;
		}

		// gestisco le variabile per le autoexpression
		BindingVariable();

		
	}
	else
		{
			AfxMessageBox(_TB("Error loading. Unable to use the selected profile."));
			return WIZARD_SAME_TAB;
		}

	return	 (CheckViewPage(IDD_WIZARD_SEL_CRITERIA_PAGE))
			? IDD_WIZARD_SEL_CRITERIA_PAGE
			: IDD_WIZARD_OPTION_PAGE;	
}


//-----------------------------------------------------------------------------
void CExpCriteriaWizardView::BindingVariable()
{
	CAbstractFormDoc*		pExportedDoc	= GetExportedDocument();
	CExpCriteriaWizardDoc*	pWizDoc			= GetExpCriteriaWizardDoc();		//al documento wizard aggiungo le variabili definite nel documento 
	//da esportare
	if (!pWizDoc->m_pVariablesArray)
		pWizDoc->m_pVariablesArray = new CXMLVariableArray();

	// variabili dei criteri predefiniti
	if (pExportedDoc && pExportedDoc->m_pBaseExportCriteria)
		pWizDoc->m_pVariablesArray->AddVariables(pExportedDoc->m_pBaseExportCriteria->m_pVariablesArray);

	
	if (pWizDoc->m_pExportCriteria)
	{

		// variabili dei criteri di OSL
		if (pWizDoc->m_pExportCriteria->m_pOSLExportCriteria)
			pWizDoc->m_pExportCriteria->m_pOSLExportCriteria->AttachVariables(pWizDoc->m_pVariablesArray);

		// variabili dei criteri personalizzati
		if (pWizDoc->m_pExportCriteria->m_pUserExportCriteria)
			pWizDoc->m_pExportCriteria->m_pUserExportCriteria->AttachVariables(pWizDoc->m_pVariablesArray);

	}
	

	CAutoExpressionMng* pAutoExpressionMng = pWizDoc ? pWizDoc->m_pAutoExpressionMng : NULL;
	
	//aggiorno i puntatori ai dataobj dell' autoexpressionmanager dato che sono stati riallocati
	for(int i = 0; i < pWizDoc->m_pVariablesArray->GetSize() ; i++)
	{
		for(int n = 0 ; n < pAutoExpressionMng->GetSize() ; n++)
			{
				if (pWizDoc->m_pVariablesArray->GetAt(i)->GetName().CompareNoCase(pAutoExpressionMng->GetAt(n)->GetVarName()) == 0)
					pAutoExpressionMng->GetAt(n)->SetDataObj(pWizDoc->m_pVariablesArray->GetAt(i)->GetDataObj());
			}
	}
}

//-----------------------------------------------------------------------------
BOOL CExpCriteriaWizardView::ExistOneCriteria()
{
	CExpCriteriaWizardDoc*	pWizDoc = GetExpCriteriaWizardDoc();
	CUserExportCriteria* pUserCriteria = pWizDoc ? pWizDoc->GetUserExportCriteria() : NULL;

	return  
			(
				pWizDoc &&
				(
					pWizDoc->IsTracedDBTMasterTable() ||
					pWizDoc->GetFirstAppTabIDD() != 0 ||
					(
						pUserCriteria &&
						pUserCriteria->m_pQueryInfo &&
						(
							!pUserCriteria->m_pQueryInfo->m_TableInfo.m_strFilter.IsEmpty() || 
							!pUserCriteria->m_pQueryInfo->m_TableInfo.m_strSort.IsEmpty()
						)
					)
				)
			);	
}


//-----------------------------------------------------------------------------
LRESULT CExpCriteriaWizardView::OnSelCriteriaPageWizardNext()
{
	CExpCriteriaWizardDoc*	pWizDoc = GetExpCriteriaWizardDoc();
	CPreferencesCriteria* pPreferencesCriteria = pWizDoc ? pWizDoc->GetPreferencesCriteria() : NULL;

	if (pPreferencesCriteria)
	{
		if (pPreferencesCriteria->IsCriteriaModeApp())
		{
			if (pWizDoc->CreateAppExpCriteriaTabDlgs(GetTabManager(), GetTabManager()->GetTabDialogPos(IDD_WIZARD_SEL_CRITERIA_PAGE)))
				return pWizDoc->GetFirstAppTabIDD();
		}
		
		if (pPreferencesCriteria->IsCriteriaModeOSL())
			return IDD_WIZARD_SEL_AUDIT_PAGE;					
		
		return OnWizardNext(IDD_WIZARD_SEL_AUDIT_PAGE);
	}

	return IDD_WIZARD_OPTION_PAGE;
}		
	
