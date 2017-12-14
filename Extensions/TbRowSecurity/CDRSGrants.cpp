#include "stdafx.h" 

#include <TbNameSolver\TBNamespaces.h>
#include <TbNameSolver\ThreadContext.h>
#include <TbNameSolver\LoginContext.h>

#include <TbGes\EXTDOC.H>
#include <TbGes\DBT.H>
#include <TbGes\NumbererService.h>

#include <ExtensionsImages\CommonImages.h>
#include <TbResourcesMng\TWorkers.h>

#include "TBRowSecurityEnums.h"
#include "RSStructures.h"
#include "RSManager.h"
#include "RSTables.h"
#include "UIRSGrants.hjson" //JSON AUTOMATIC UPDATE
#include "UIRSGrants.h"
#include "CDRSGrants.h"

const TaskBuilderToolbarImageSet	IMGSET_OPEN_GRANTS_FORM	(_NS_LIB("Extensions.TBRowSecurity.TBRowSecurity"),	IDB_RS_OPEN_GRANTS_FORM);
static TCHAR szNoGrants[] = _T("Image.Extensions.TBRowSecurity.Images.NoGrant.ico");
static TCHAR szOnlyRead[] = _T("Image.Extensions.TBRowSecurity.Images.OnlyRead.ico");
static TCHAR szReadWrite[] = _T("Image.Extensions.TBRowSecurity.Images.ReadWrite.ico");
//----------------------------------------------------------------------------
//	Class CRSGrantsClientDoc definition
//----------------------------------------------------------------------------
IMPLEMENT_DYNAMIC(CRSGrantsClientDoc, CClientDoc)

//----------------------------------------------------------------------------
BEGIN_MESSAGE_MAP(CRSGrantsClientDoc, CClientDoc)
	//{{AFX_MSG_MAP(CRSGrantsClientDoc)
	ON_EN_VALUE_CHANGED	(IDC_RS_GRANTS_PROTECTED_CHECK,	OnProtectedCheckChanged)

	ON_COMMAND			(ID_RS_OPEN_GRANTS_FORM,	OnOpenFormGrant)
	ON_COMMAND			(ID_RS_GRANTS_NOGRANT_SHOW, OnNoGrantShow)
	ON_COMMAND			(ID_RS_GRANTS_READ_SHOW,	OnReadGrantShow)
	ON_COMMAND			(ID_RS_GRANTS_FULL_SHOW,	OnFullGrantShow)
	
	ON_UPDATE_COMMAND_UI(ID_RS_OPEN_GRANTS_FORM,	OnUpdateOpenFormGrant)
	ON_UPDATE_COMMAND_UI(ID_RS_GRANTS_NOGRANT_SHOW, OnUpdateGrantShow)
	ON_UPDATE_COMMAND_UI(ID_RS_GRANTS_READ_SHOW, OnUpdateReadGrantShow)
	ON_UPDATE_COMMAND_UI(ID_RS_GRANTS_FULL_SHOW, OnUpdateFullGrantShow)

	//}}AFX_MSG_MAP
END_MESSAGE_MAP()

//----------------------------------------------------------------------------
CRSGrantsClientDoc::CRSGrantsClientDoc()
:
	m_bTreeView				 (TRUE),
	m_bValidDocument		 (FALSE),
	m_bGrantToLoad			 (TRUE),
	m_pEntityInfo			 (NULL),
	m_pCurrSubjectsGrantsRec (NULL),	
	m_bShowDeny				 (TRUE),
	m_bShowRead				 (TRUE),
	m_bShowFull				 (TRUE),
	m_CurrSubjectGrantType	 (E_GRANT_TYPE_DEFAULT)
{
	//SetMsgRoutingMode(CClientDoc::CD_MSG_AFTER);
}

//----------------------------------------------------------------------------
CRSGrantsClientDoc::~CRSGrantsClientDoc()
{
}

//----------------------------------------------------------------------------
CAbstractFormDoc* CRSGrantsClientDoc::GetServerDoc()
{
	CBaseDocument*	pServerDoc = GetMasterDocument();
	ASSERT(pServerDoc->IsKindOf(RUNTIME_CLASS(CAbstractFormDoc)));
	return (CAbstractFormDoc*)pServerDoc;
}
//----------------------------------------------------------------------------
BOOL CRSGrantsClientDoc::OnAttachData()
{
	m_pEntityInfo = AfxGetRowSecurityManager()->GetEntityInfo(GetServerDoc()->GetNamespace());
	m_pDBTEntitySubjectsGrants = new DBTEntitySubjectsGrants(RUNTIME_CLASS(TRS_SubjectsGrants), GetServerDoc(), m_pEntityInfo->m_strName);
	m_pDBTEntitySubjectsGrants->SetReadOnly(TRUE);
	GetServerDoc()->GetMaster()->Attach(m_pDBTEntitySubjectsGrants);
	//alla tabella master chiedo di estrarre i dati relativi alla protezione per l'utente corrente
	GetServerDoc()->GetMaster()->GetTable()->SetSelectGrantInformation(NULL);
	return TRUE;
}

//----------------------------------------------------------------------------
void CRSGrantsClientDoc::OnAfterCreateAndInitDBT(DBTObject* pDBTObj)
{
	DBTMaster* pDBTMaster = GetServerDoc()->m_pDBTMaster;

	if (
		pDBTObj && 
		//@@TODO controllare che si tratta di un DBTMaster!
		pDBTObj->GetRecord()->GetIndexFromColumnName(RowSecurityAddOnFields::s_sRowSecurityID) > -1
		)
	{
		RSEntityInfo* pEntityInfo = AfxGetRowSecurityManager()->GetEntityInfo(GetServerDoc()->GetNamespace());
		if (pEntityInfo)
		{
			m_bValidDocument = TRUE;
			pDBTObj->BindAutoincrement(pDBTObj->GetRecord()->GetDataObjFromColumnName(RowSecurityAddOnFields::s_sRowSecurityID), pEntityInfo->m_strAutonumberNamespace);
		}
	}
}

//----------------------------------------------------------------------------
TRS_SubjectsGrants* CRSGrantsClientDoc::GetGrantRecordForSubject(int nSubjectID)
{
	return m_pDBTEntitySubjectsGrants->GetGrantRecordForSubject(nSubjectID);
}

//----------------------------------------------------------------------------
void CRSGrantsClientDoc::ModifyImplicitGrant(int nOldWorkerID, int nNewWorkerID)
{
	if (!*(DataBool*)GetServerDoc()->m_pDBTMaster->GetRecord()->GetDataObjFromColumnName(RowSecurityAddOnFields::s_sIsProtected))
		return;
	
	if (nOldWorkerID == nNewWorkerID)
		return; 

	Array* pSubjectsToGrants;
	if (nOldWorkerID > 0)
	{
		//devo togliere i grant impliciti a nOldWorkerID 
		//ovvero vado a modificare il grant nei records corrispondenti nel DBTEntitySubjectsGrants
		pSubjectsToGrants = AfxGetGrantsManager()->GetSubjectsToImplicitGrant(nOldWorkerID);
		m_pDBTEntitySubjectsGrants->AddRemoveImplicitGrants(pSubjectsToGrants, nOldWorkerID, TRUE);
		if (pSubjectsToGrants)
			delete pSubjectsToGrants;
	}

	if (nNewWorkerID > 0)
	{
		//devo togliere i grant impliciti a nOldWorkerID ed ai suoi master/fratelli
		//ovvero vado a modificare il grant nei records corrispondenti nel DBTEntitySubjectsGrants
		pSubjectsToGrants = AfxGetGrantsManager()->GetSubjectsToImplicitGrant(nNewWorkerID);
		m_pDBTEntitySubjectsGrants->AddRemoveImplicitGrants(pSubjectsToGrants, nNewWorkerID, FALSE);
		if (pSubjectsToGrants)
			delete pSubjectsToGrants;
	}	
	RefreshGrantsTree();
}

//----------------------------------------------------------------------------
Array* CRSGrantsClientDoc::ModifyExplicitGrant(TRS_SubjectsGrants* pSubjectsGrantsRec, DataEnum grantType)
{
	//se il subject è di tipo risorsa allora devo dare il grant esplicito anche a tutti i suoi worker
	//altrimenti il grant deve essere assegnato solo al subject
	Array* pSubjectsToGrants = AfxGetGrantsManager()->GetSubjectsToExplictGrant(pSubjectsGrantsRec->f_SubjectID);
	m_pDBTEntitySubjectsGrants->ModifyExplicitGrants(pSubjectsGrantsRec, pSubjectsToGrants, grantType);	
	return pSubjectsToGrants;
}

//----------------------------------------------------------------------------
BOOL CRSGrantsClientDoc::CurrentWorkerHasFullGrant()
{
	RowSecurityAddOnFields* pAddOnFields = (RowSecurityAddOnFields*)GetServerDoc()->m_pDBTMaster->GetRecord()->GetAddOnFields(RUNTIME_CLASS(RowSecurityAddOnFields));
	return (!GetServerDoc()->ValidCurrentRecord() || !pAddOnFields->f_IsProtected || pAddOnFields->l_CurrentWorkerGrantType == DataEnum(E_GRANT_TYPE_READWRITE));
}

//----------------------------------------------------------------------------
BOOL CRSGrantsClientDoc::OnPrepareAuxData()
{
	return TRUE;
}

//----------------------------------------------------------------------------
void CRSGrantsClientDoc::SetCurrSubjectsGrantsRec(TRS_SubjectsGrants* pCurrSubjectsGrantsRec)
{
	m_pCurrSubjectsGrantsRec = pCurrSubjectsGrantsRec;
}

//----------------------------------------------------------------------------
void CRSGrantsClientDoc::OnProtectedCheckChanged()
{
	RowSecurityAddOnFields* pAddOnFields = (RowSecurityAddOnFields*)GetServerDoc()->m_pDBTMaster->GetRecord()->GetAddOnFields(RUNTIME_CLASS(RowSecurityAddOnFields));
	
	//devo aggiornare il campo locale del master su cui si appoggia l'enable/disable del bottone dei grant
	TRS_SubjectsGrants* pSubjectsGrantsRec =  m_pDBTEntitySubjectsGrants->GetGrantRecordForSubject(AfxGetLoggedSubject()->m_SubjectID);

	if (pAddOnFields->f_IsProtected)
	{
		Array* pSubjects = ModifyExplicitGrant(pSubjectsGrantsRec, E_GRANT_TYPE_READWRITE);
		if (pSubjects)
			delete pSubjects;
	}
	else
		m_pDBTEntitySubjectsGrants->InitSubjectsGrants();	//se il valore dell'entità è posto sotto protezione viene assegnato in automatico un grant full al worker corrente (altrimenti se non fornisco dei grant full non si ha più la possibilità di modificare i grant)
	
	pAddOnFields->l_CurrentWorkerGrantType = (pSubjectsGrantsRec) ? pSubjectsGrantsRec->f_GrantType : E_GRANT_TYPE_DENY;

	RefreshGrantsTree();
}

//---------------------------------------------------------------------------------------------------------------
void CRSGrantsClientDoc::DoCurrentSubjectGrantsChanged()
{
	m_CurrSubjectGrantType = (m_pCurrSubjectsGrantsRec) ? m_pCurrSubjectsGrantsRec->f_GrantType : E_GRANT_TYPE_DENY;
	m_strGrantInherited = (m_pCurrSubjectsGrantsRec && m_pCurrSubjectsGrantsRec->f_Inherited) ? _TB("The grant is enherited from the resource") : _T("");

	m_CurrSubjectGrantType.SetHide(!m_pCurrSubjectsGrantsRec);
	m_strGrantInherited.SetHide(!m_pCurrSubjectsGrantsRec);

	SetImageAndDescription();	
}


//---------------------------------------------------------------------------------------------------------------v
void CRSGrantsClientDoc::DoGrantTypeChanged()
{
	if (!m_pCurrSubjectsGrantsRec)
		return;

	Array* pSubjectsToGrants = NULL;

	if (m_CurrSubjectGrantType != m_pCurrSubjectsGrantsRec->f_GrantType)
	{
		// modifica i grants a tutti i soggetti coinvolti
		pSubjectsToGrants = ModifyExplicitGrant(m_pCurrSubjectsGrantsRec, m_CurrSubjectGrantType);
		SetImageAndDescription();
	}

	//se ho cambiato grants a più subject allora ricarico l'albero altrimento cambio l'immagine solo al nodo selezionato
	if (pSubjectsToGrants && pSubjectsToGrants->GetSize() > 0)
		(pSubjectsToGrants->GetSize() > 1) ? RefreshGrantsTree() : RefreshCurrentNode();
	
	delete pSubjectsToGrants;
}



//---------------------------------------------------------------------------------------------------------------
void CRSGrantsClientDoc::SetImageAndDescription()
{
	if (!m_pCurrSubjectsGrantsRec)
	{
		m_strGrantDescription = _T("");
		m_strGrantPicture = szNoGrants;
	}
	else
	{

		//CWorker* pWorker = AfxGetWorkersTable()->GetWorker(m_pCurrSubjectsGrantsRec->f_TBCreatedID);
		//CString strCreatedUser = (pWorker) ? cwsprintf(_T("%s %s"), pWorker->GetName(), pWorker->GetLastName()) : cwsprintf(_T("%d"), pGrantForSubject->f_TBCreatedID);
		//CString strName = cwsprintf(_T("%s codice %s"), GetRSGrantsClientDoc()->m_pEntityInfo->m_strDescription, GetDocument()->m_pDBTMaster->GetRecord()->GetPrimaryKeyDescription());

		CString strName = GetServerDoc()->m_pDBTMaster->GetRecord()->GetPrimaryKeyDescription();
		CSubjectCache* pSubjectCache = AfxGetSubjectsManager()->GetSubjectCache(m_pCurrSubjectsGrantsRec->f_SubjectID);
		switch (m_pCurrSubjectsGrantsRec->f_GrantType.GetValue())
		{
		case E_GRANT_TYPE_READ_ONLY:
			if (pSubjectCache->IsWorker())
				m_strGrantDescription = cwsprintf(_TB("The worker {0-%s} can read only this {1-%s}"), pSubjectCache->GetSubjectTitle(), strName);
			else
				m_strGrantDescription = cwsprintf(_TB("The workers of the resource {0-%s} can read only this {1-%s}"), pSubjectCache->GetSubjectTitle(), strName);
			m_strGrantPicture = szOnlyRead;
			break;
		case E_GRANT_TYPE_DENY:
			if (pSubjectCache->IsWorker())
				m_strGrantDescription = cwsprintf(_TB("For the worker {0-%s} no action is possible on this {1-%s}"), pSubjectCache->GetSubjectTitle(), strName);
			else
				m_strGrantDescription = cwsprintf(_TB("For the workers of the resource {0-%s} no action is possible on this {1-%s}"), pSubjectCache->GetSubjectTitle(), strName);
			m_strGrantPicture = szNoGrants;
			break;
		case E_GRANT_TYPE_READWRITE:
			if (pSubjectCache->IsWorker())
				m_strGrantDescription = cwsprintf(_TB("The worker {0-%s} has full control on this {1-%s}"), pSubjectCache->GetSubjectTitle(), strName);
			else
				m_strGrantDescription = cwsprintf(_TB("The workers of the resource {0-%s} has full control on this {1-%s}"), pSubjectCache->GetSubjectTitle(), strName);
			m_strGrantPicture = szReadWrite;
			break;
		}
	}

	m_strGrantDescription.SetHide(!m_pCurrSubjectsGrantsRec);
	m_strGrantPicture.SetHide(!m_pCurrSubjectsGrantsRec);

	GetServerDoc()->UpdateDataView();
}


//----------------------------------------------------------------------------
BOOL CRSGrantsClientDoc::SaveExplicitGrants()
{
	RowSecurityAddOnFields* pOldAddOnFields = (RowSecurityAddOnFields*)GetServerDoc()->m_pDBTMaster->GetOldRecord()->GetAddOnFields(RUNTIME_CLASS(RowSecurityAddOnFields));
	RowSecurityAddOnFields* pAddOnFields = (RowSecurityAddOnFields*)GetServerDoc()->m_pDBTMaster->GetRecord()->GetAddOnFields(RUNTIME_CLASS(RowSecurityAddOnFields));
	
	TRY
	{
		if (pAddOnFields->f_IsProtected)
			AfxGetGrantsManager()->SaveExplicitGrants(m_pDBTEntitySubjectsGrants);

		if (pOldAddOnFields->f_IsProtected && !pAddOnFields->f_IsProtected)
			AfxGetGrantsManager()->DeleteAllGrants(m_pEntityInfo->m_strName, pAddOnFields->f_RowSecurityID);
	}
	CATCH(SqlException, e)
	{
		GetServerDoc()->Message(e->m_strError);
		GetServerDoc()->m_pMessages->Show(TRUE);
		return FALSE;
	}
	END_CATCH
	
	TRS_SubjectsGrants* pSubjectsGrantsRec =  m_pDBTEntitySubjectsGrants->GetGrantRecordForSubject(AfxGetLoggedSubject()->m_SubjectID);	
	pAddOnFields->l_CurrentWorkerGrantType = (pSubjectsGrantsRec) ? pSubjectsGrantsRec->f_GrantType : E_GRANT_TYPE_DENY;

	return TRUE;
}

//----------------------------------------------------------------------------
BOOL CRSGrantsClientDoc::OnExtraNewTransaction()
{
	return SaveExplicitGrants();
}

//----------------------------------------------------------------------------
BOOL CRSGrantsClientDoc::OnExtraEditTransaction()
{
	return SaveExplicitGrants();
}

//----------------------------------------------------------------------------
BOOL CRSGrantsClientDoc::OnExtraDeleteTransaction()
{
	RowSecurityAddOnFields* pAddOnFields = (RowSecurityAddOnFields*)GetServerDoc()->m_pDBTMaster->GetRecord()->GetAddOnFields(RUNTIME_CLASS(RowSecurityAddOnFields));

	TRY
	{
		if (pAddOnFields->f_IsProtected)
			AfxGetGrantsManager()->DeleteAllGrants(m_pEntityInfo->m_strName, pAddOnFields->f_RowSecurityID);
	}
	CATCH(SqlException, e)
	{
		GetServerDoc()->Message(e->m_strError);
		GetServerDoc()->m_pMessages->Show(TRUE);
		return FALSE;
	}
	END_CATCH
	return TRUE;
}

//----------------------------------------------------------------------------
void CRSGrantsClientDoc::Customize()
{
	if (
		m_pEntityInfo && m_pEntityInfo->m_bUsed && m_bValidDocument &&
		GetServerDoc()->m_pDBTMaster->GetRecord()->GetIndexFromColumnName(RowSecurityAddOnFields::s_sIsProtected) > -1
		)
	{
		//SetDocAccel(IDR_RS_GRANTS);
		AddButton(ID_RS_OPEN_GRANTS_FORM, _NS_TOOLBARBTN("OpenGrantsForm"), ExtensionsIcon(szIconUnlock, TOOLBAR), _T("Open Grants Form"),  szToolbarNameMain);
		CTBToolBar* pToobar = GetToolBar(szToolbarNameMain);
		if (pToobar)
			pToobar->SetTextToolTip(ID_RS_OPEN_GRANTS_FORM, _TB("Open Grants Tree\nOpen Grants Tree"));

	}
		
}

//----------------------------------------------------------------------------
void CRSGrantsClientDoc::OnOpenFormGrant()
{
	if (!GetServerDoc() || !GetServerDoc()->ValidCurrentRecord() || !m_bValidDocument)
	{
		ASSERT(FALSE);
		return;
	}

//solo l'admin oppure l'utente che ha il permesso può aprire la form dell'assegnazione dei grant
	if (!AfxGetLoginInfos()->m_bAdmin)
	{
		TRWorkers aTRWorker(GetServerDoc());
		DataLng currWorker(AfxGetWorkerId());
		if (aTRWorker.FindRecord(currWorker) != TableReader::FOUND || !aTRWorker.GetRecord()->f_IsRSEnabled)
		{
			AfxMessageBox(_TB("The logged user can't open the row security grants form"));
			return;
		}
	}

	//CString strTitle = cwsprintf(_TB("Row Security Grants for %s %s"),m_pEntityInfo->m_strName, GetServerDoc()->m_pDBTMaster->GetRecord()->GetPrimaryKeyDescription());
	GetServerDoc()->CreateSlaveView
			( 
				RUNTIME_CLASS(CRSEntityGrantsView), 
				_TB("Row Security Grants"),
				GetServerDoc()->GetRuntimeClass()
			);
}

//-----------------------------------------------------------------------------
void CRSGrantsClientDoc::OnUpdateOpenFormGrant(CCmdUI* pCmdUI)
{
	CAbstractFormDoc* pServerDoc = GetServerDoc();
	RowSecurityAddOnFields* pAddOnFields = (RowSecurityAddOnFields*)GetServerDoc()->m_pDBTMaster->GetRecord()->GetAddOnFields(RUNTIME_CLASS(RowSecurityAddOnFields));
	
	CTBToolBar* pToobar = GetToolBar(szToolbarNameMain);
	pToobar->SetButtonInfo(ID_RS_OPEN_GRANTS_FORM, TBSTATE_ENABLED, (pServerDoc->ValidCurrentRecord() && pAddOnFields->f_IsProtected) ? ExtensionsIcon(szIconLock, TOOLBAR) : ExtensionsIcon(szIconUnlock, TOOLBAR), NULL);

	pCmdUI->Enable(GetServerDoc()->ValidCurrentRecord() && (!pAddOnFields->f_IsProtected || pAddOnFields->l_CurrentWorkerGrantType != E_GRANT_TYPE_DENY));
}


//---------------------------------------------------------------------------------------------------------------
void CRSGrantsClientDoc::RefreshGrantsTree()
{
	CRSEntityGrantsView* pSlaveView = NULL;
	//se la slave view è già aperta allora invio messaggio di reload dell'albero
	if (pSlaveView = (CRSEntityGrantsView*)GetServerDoc()->ViewAlreadyPresent(RUNTIME_CLASS(CRSEntityGrantsView)))
		pSlaveView->GetFrame()->SendMessageToDescendants(WM_COMMAND, ID_RS_LOAD_GRANTS_TREE);
}
//---------------------------------------------------------------------------------------------------------------
void CRSGrantsClientDoc::RefreshCurrentNode()
{
	CRSEntityGrantsView* pSlaveView = NULL;
	//se la slave view è già aperta allora invio messaggio di refresh immagine del nodo corrente
	if (pSlaveView = (CRSEntityGrantsView*)GetServerDoc()->ViewAlreadyPresent(RUNTIME_CLASS(CRSEntityGrantsView)))
		pSlaveView->GetFrame()->SendMessageToDescendants(WM_COMMAND, ID_RS_REFRESH_CURRENT_NODE);
}

//---------------------------------------------------------------------------------------------------------------
void CRSGrantsClientDoc::OnNoGrantShow()
{
	m_bShowDeny = !m_bShowDeny;	
	RefreshGrantsTree();
}

//---------------------------------------------------------------------------------------------------------------
void CRSGrantsClientDoc::OnReadGrantShow()
{
	m_bShowRead = !m_bShowRead;
	RefreshGrantsTree();
}
//---------------------------------------------------------------------------------------------------------------
void CRSGrantsClientDoc::OnFullGrantShow()
{
	m_bShowFull = !m_bShowFull;
	RefreshGrantsTree();
}
//---------------------------------------------------------------------------------------------------------------
void CRSGrantsClientDoc::OnUpdateGrantShow(CCmdUI* pCmdUI)
{
	pCmdUI->Enable(TRUE);
	pCmdUI->SetCheck(m_bShowDeny);
}

//---------------------------------------------------------------------------------------------------------------
void CRSGrantsClientDoc::OnUpdateReadGrantShow(CCmdUI* pCmdUI)
{
	pCmdUI->Enable(TRUE);
	pCmdUI->SetCheck(m_bShowRead);
}

//---------------------------------------------------------------------------------------------------------------
void CRSGrantsClientDoc::OnUpdateFullGrantShow(CCmdUI* pCmdUI)
{
	pCmdUI->Enable(TRUE);
	pCmdUI->SetCheck(m_bShowFull);
}

