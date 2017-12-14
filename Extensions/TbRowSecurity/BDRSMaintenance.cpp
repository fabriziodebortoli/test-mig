#include "stdafx.h"

//NOW INCLUDED IN COMMON PCH: #include <tbgenlib\parsobj.h>

#include <TbFrameworkImages\GeneralFunctions.h>
#include <TbFrameworkImages\CommonImages.h>

#include <TbResourcesMng\TWorkers.h>
#include <TbResourcesMng\TResources.h>

// Locals
#include "RSManager.h"
#include "BDRSMaintenance.h"
#include "UIRSMaintenance.h"
#include "UIRSMaintenance.hjson"



static TCHAR szP1[] = _T("P1");
static TCHAR szP2[] = _T("P2");
static TCHAR szP3[] = _T("P3");


//////////////////////////////////////////////////////////////////////////////
// VRSEntities
//////////////////////////////////////////////////////////////////////////////
//------------------------------------------------------------------------------
IMPLEMENT_DYNCREATE(VRSEntities, SqlVirtualRecord)

//----------------------------------------------------------------------------
VRSEntities::VRSEntities(BOOL bCallInit /*= TRUE*/)
	:
	SqlVirtualRecord(_T("VRSEntities")),
	l_IsProtected(FALSE)
{
	BindRecord();

	if (bCallInit) Init();

	SetValid(TRUE);
}

//-----------------------------------------------------------------------------------------------
void VRSEntities::BindRecord()
{
	BEGIN_BIND_DATA();
	LOCAL_STR(_NS_FLD("EntityName"), l_EntityName, 255);
	LOCAL_DATA(_NS_FLD("IsProtected"), l_IsProtected);
	END_BIND_DATA();
}

//-----------------------------------------------------------------------------
LPCTSTR VRSEntities::GetStaticName() { return _NS_TBL("VRSEntities"); }

///////////////////////////////////////////////////////////////////////////////
//             class DBTRowSecurityEntities implementation
///////////////////////////////////////////////////////////////////////////////
//
//=============================================================================
IMPLEMENT_DYNAMIC(DBTRSEntities, DBTSlaveBuffered)

//-----------------------------------------------------------------------------	
DBTRSEntities::DBTRSEntities
(
	CRuntimeClass*		pClass,
	CAbstractFormDoc*	pDocument
)
	:
	DBTSlaveBuffered(pClass, pDocument, _NS_DBT("DBTRSEntities"), ALLOW_EMPTY_BODY, FALSE)
{
}



///////////////////////////////////////////////////////////////////////////////
// 				class BDRSMaintenance Implementation
///////////////////////////////////////////////////////////////////////////////
//-----------------------------------------------------------------------------
IMPLEMENT_DYNCREATE(BDRSMaintenance, CAbstractFormDoc)

//-----------------------------------------------------------------------------
BEGIN_MESSAGE_MAP(BDRSMaintenance, CAbstractFormDoc)
END_MESSAGE_MAP()

//----------------------------------------------------------------------------
BDRSMaintenance::BDRSMaintenance()
	:
	m_pResources			(NULL),
	m_pHierarchies			(NULL),
	m_pPictureStatic_Step1	(NULL),
	m_pPictureStatic_Step2	(NULL),
	m_pPictureStatic_Step3	(NULL),
	m_pPictureStatic_Step4	(NULL),
	pTile					(NULL),
	m_pDBTRSEntities		(NULL)
{
	m_bBatch = TRUE;
	PathDone = TBGlyph(szGlyphOk);
	PathNotDone = TBGlyph(szIconError);
}

//----------------------------------------------------------------------------
BDRSMaintenance::~BDRSMaintenance()
{
	SAFE_DELETE(m_pDBTRSEntities);
}

//-----------------------------------------------------------------------------
BOOL BDRSMaintenance::OnAttachData()
{              
	SetFormTitle(_TB("Row Security Maintenance"));

	m_pDBTRSEntities = new DBTRSEntities(RUNTIME_CLASS(VRSEntities), this);
	
	CStringArray* pArr = AfxGetRowSecurityManager()->GetAllEntities();
	CString sUsedEntities = AfxGetRowSecurityManager()->GetUsedEntities();
	for (int i = 0; i < pArr->GetSize(); i++)
	{
		CString s = pArr->GetAt(i);
		DataBool b = sUsedEntities.Find(s) > -1;

		VRSEntities* pRec = (VRSEntities*)m_pDBTRSEntities->AddRecord();
		pRec->l_EntityName = s;
		pRec->l_IsProtected = b;
	}
	delete pArr;

	return TRUE;
}

//-----------------------------------------------------------------------------
BOOL BDRSMaintenance::CanRunDocument()
{
	if (!AfxRowSecurityEnabled()) 
	{
		AfxMessageBox(_TB("Unable to open Row Security Maintenance procedure!\r\nPlease, check in Administration Console if this company uses RowSecurity."));
		return FALSE;
	}

	return TRUE;
}

//-----------------------------------------------------------------------------
BOOL BDRSMaintenance::CanDoBatchExecute()
{
	return TRUE;
}

//-----------------------------------------------------------------------------
void BDRSMaintenance::HideAll()
{

	if(!pTile)
		return;

	//nascondo ancora tutto
	//pTile->GetDlgItem(IDC_STATIC_START)->ShowWindow(SW_HIDE);
	StepStartImg.Clear();
	
	//pTile->GetDlgItem(IDC_STATIC_STEP1)->ShowWindow(SW_HIDE);		
	Step1Img.Clear();
	m_pPictureStatic_Step1->SetToolTipBuffer(L"");

	//pTile->GetDlgItem(IDC_STATIC_STEP2)->ShowWindow(SW_HIDE);		
	Step2Img.Clear();
	m_pPictureStatic_Step2->SetToolTipBuffer(L"");

	//pTile->GetDlgItem(IDC_STATIC_STEP3)->ShowWindow(SW_HIDE);		
	Step3Img.Clear();
	m_pPictureStatic_Step3->SetToolTipBuffer(L"");

	//pTile->GetDlgItem(IDC_STATIC_STEP4)->ShowWindow(SW_HIDE);		
	Step4Img.Clear();
	m_pPictureStatic_Step4->SetToolTipBuffer(L"");

	//pTile->GetDlgItem(IDC_STATIC_END)->ShowWindow(SW_HIDE);
	StepEndImg.Clear();

	UpdateDataView();
}

//-----------------------------------------------------------------------------
BOOL BDRSMaintenance::SaveUsedEntities()
{
	//costruzione stringa concatenata da ; delle entità selezionate
	CString strUsedEntities;
	for (int i = 0; i <= m_pDBTRSEntities->GetUpperBound(); i++)
	{
		VRSEntities* pRec = (VRSEntities*)m_pDBTRSEntities->GetRow(i);
		if (pRec->l_IsProtected)
			strUsedEntities.Append(pRec->l_EntityName + L";");
	}

	if (strUsedEntities.IsEmpty())
	{
			Message(_TB("Unable to proceed because you have to choose the entities to protect first!"));
			GetMessages()->Show(TRUE);
			return FALSE;
	}

	strUsedEntities = strUsedEntities.Mid(0, strUsedEntities.GetLength() - 1);	

	if (strUsedEntities.CompareNoCase(AfxGetIRowSecurityManager()->GetUsedEntities()) != 0)
	{
		//aggiorno UsedEntities
		AfxGetRowSecurityManager()->UpdateUsedEntities(strUsedEntities);
		//Aggiorna la riga di RS_Configuration
		return AfxGetRowSecurityManager()->UpdateRSConfiguration(FALSE);
	}

	return TRUE;
}

//-----------------------------------------------------------------------------
void BDRSMaintenance::OnBatchExecute()
{
	HideAll();

	BeginWaitCursor();

	if (!CanRunDocumentInStandAloneMode())
	{
		Message(_TB("In order to preserve the Row Security Layer, this procedure can be run only if no other user is logged to this company and no other document is opened!"));
		GetMessages()->Show(TRUE);
		return;
	}

	int result = Message(_TB("Confirm Row Security maintenance procedure execution?"), MB_YESNO);
	
	BOOL bOk = TRUE;

	if (result == IDYES || result == NO_MSG_BOX_SHOWN)
	{
		if (SaveUsedEntities())
			ExecuteMaintenanceProcedure();			
	}

	EndWaitCursor();
	UpdateDataView();
}

//-----------------------------------------------------------------------------
BOOL BDRSMaintenance::ExecuteMaintenanceProcedure()
{
	Start();
	return Step1() && Step2() && Step3() && Step4() && End();
}

//-----------------------------------------------------------------------------
void BDRSMaintenance::Start()
{
	//START
	//if(pTile)
	//	pTile->GetDlgItem(IDC_STATIC_START)->ShowWindow(SW_SHOW);
	StepStartImg = PathDone;
	UpdateDataView();
}

//-----------------------------------------------------------------------------
BOOL BDRSMaintenance::Step1()
{
	BOOL bOk = AfxGetIRowSecurityManager()->ValorizeRowSecurityID(); 	

	if (pTile)
		pTile->GetDlgItem(IDC_STATIC_STEP1)->ShowWindow(SW_SHOW);

	Step1Img = bOk ? PathDone: PathNotDone;

	if (m_pPictureStatic_Step1)
		m_pPictureStatic_Step1->SetToolTipBuffer(bOk ? _TB("RowSecurityID field values checking successfully completed") : _TB("RowSecurityID field values checking ended with errors"));

	UpdateDataView();

	return bOk;
}

//-----------------------------------------------------------------------------
BOOL BDRSMaintenance::Step2()
{
	BOOL bOk = ManageSubjects();	

	if(pTile)
		pTile->GetDlgItem(IDC_STATIC_STEP2)->ShowWindow(SW_SHOW);

	Step2Img = bOk ? PathDone: PathNotDone;

	if(m_pPictureStatic_Step2)
		m_pPictureStatic_Step2->SetToolTipBuffer(bOk ? _TB("Subjects maintenance successfully completed") : _TB("Subjects maintenance ended with errors"));

	UpdateDataView();
	
	return bOk;
}

//-----------------------------------------------------------------------------
BOOL BDRSMaintenance::Step3()
{
	BOOL bOk = ManageHierarchies();

	 if (pTile)
		pTile->GetDlgItem(IDC_STATIC_STEP3)->ShowWindow(SW_SHOW);

	Step3Img = bOk ? PathDone: PathNotDone;

	if(m_pPictureStatic_Step3)
		m_pPictureStatic_Step3->SetToolTipBuffer(bOk ? _TB("Hierarchies between the subjects generation successfully completed") : _TB("Hierarchies between the subjects generation ended with errors"));

	UpdateDataView();

	return bOk;
}

//-----------------------------------------------------------------------------
BOOL BDRSMaintenance::Step4()
{
	BOOL bOk = AfxGetIRowSecurityManager()->ManageSubjectsGrants();

	if(pTile)
		pTile->GetDlgItem(IDC_STATIC_STEP4)->ShowWindow(SW_SHOW);

	Step4Img = bOk ? PathDone: PathNotDone;

	if(m_pPictureStatic_Step4)
		m_pPictureStatic_Step4->SetToolTipBuffer(bOk ? _TB("Subject grants maintenance successfully completed") : _TB("Subject grants maintenance ended with errors"));
		
	UpdateDataView();

	return bOk;
}

//-----------------------------------------------------------------------------
BOOL BDRSMaintenance::End()
{
	// se e' andato tutto a buon fine aggiorno la colonna IsValid della tabella RS_Configuration a TRUE
	BOOL bOk = AfxGetIRowSecurityManager()->UpdateRSConfiguration(TRUE);

	if(pTile)
	   pTile->GetDlgItem(IDC_STATIC_END)->ShowWindow(SW_SHOW);

	StepEndImg = bOk ? PathDone: PathNotDone;

	UpdateDataView();

	return bOk;
}


// Viene riempito un array con tutte le risorse e i workers censiti sul database.
// Poi viene passato al TBRowSecurity che si occupa di aggiungere od eliminare
// record nella tabella RS_Subjects.
//-----------------------------------------------------------------------------
BOOL BDRSMaintenance::ManageSubjects()
{
	CRSResourcesArray resourcesArray;

	TWorkers aWorkerRec;
	SqlTable aWorkerTable(&aWorkerRec, GetReadOnlySqlSession());

	TResources aResRec;
	SqlTable aResTable(&aResRec, GetReadOnlySqlSession());


	TRY
	{
		aWorkerTable.Open();
		aWorkerTable.Select(aWorkerRec.f_WorkerID);
		//aWorkerTable.Select(GET_ADDON_FIELD(aWorkerRec,f_CompleteName));
		aWorkerTable.Select(aWorkerRec.f_Name);
		aWorkerTable.Select(aWorkerRec.f_LastName);
		// Disabled si o no???
		aWorkerTable.AddFilterColumn(aWorkerRec.f_Disabled);
		aWorkerTable.AddParam(_T("P1"), aWorkerRec.f_Disabled);
		aWorkerTable.SetParamValue(_T("P1"), DataBool(FALSE));
		aWorkerTable.Query();

		aWorkerTable.Query();
		while(!aWorkerTable.IsEOF())
		{
			resourcesArray.AddWorker(aWorkerRec.f_WorkerID, cwsprintf(_T("%s %s"), aWorkerRec.f_Name.Str(), aWorkerRec.f_LastName.Str()));
			aWorkerTable.MoveNext();
		}
		aWorkerTable.Close();

		// leggo le resources		
		aResTable.Open();
		aResTable.Select(aResRec.f_ResourceCode);
		aResTable.Select(aResRec.f_ResourceType);
		aResTable.Select(aResRec.f_Description);

		aResTable.Query();
		while (!aResTable.IsEOF())
		{
			resourcesArray.AddResource(aResRec.f_ResourceType, aResRec.f_ResourceCode, aResRec.f_Description);
			aResTable.MoveNext();
		}
		aResTable.Close();
	
		// se non ci sono record nelle tabelle RM_Workers e RM
		// chiedo all'utente se desidera eliminare tutte le informazioni nelle tabelle della RowSecurity
		// @@TODO: decidere se farlo....
		/*if (resourcesArray.GetCount() == 0)
		{
			int nResult = Message(_TB("Confirm full subjects deletion?"), MB_YESNO);
			if (nResult == IDYES)
				AfxGetIRowSecurityManager()->DeleteAllSubjects();
			return TRUE;
		}*/
	
		// eseguo una copia della cache dei subjects
		AfxGetIRowSecurityManager()->CreateOldSubjectsCacheArray();
	
		AfxGetIRowSecurityManager()->ManageSubjects(&resourcesArray);
	}
	CATCH(SqlException, e)
	{
		if (aWorkerTable.IsOpen()) aWorkerTable.Close();
		if (aResTable.IsOpen()) aResTable.Close();
		e->ShowError();
		return FALSE;
	}
	END_CATCH

	return TRUE;
}

// Metodo che si occupa di riempire le strutture in memoria con l'organigramma 
// completo dei workers e resources
// Sara' poi il TBRowSecurity che si occupera' di popolare la tabella RS_SubjectsHierarchy
//-----------------------------------------------------------------------------
BOOL BDRSMaintenance::ManageHierarchies()
{
	if (!m_pResources) // array di puntatori di appoggio contenente tutti i worker/resource analizzati
		m_pResources = new CRSResourcesArray(); 

	if (!m_pHierarchies) // array di puntatori con i "legami" tra risorse, utilizzato per l'inserimento delle gerarchie
		m_pHierarchies = new CHierarchyArray();

	TRY
	{
		// analizzo la tabella OM_ResourcesDetails
		AnalyzeResourcesDetails();

		// analizzo la tabella OM_WorkersDetails
		AnalyzeWorkersDetails();
	
		// chiamo l'interfaccia per comunicare con il TBRowSecurity, sono se ci sono delle gerarchie
		if (m_pHierarchies->GetCount() > 0)
			AfxGetIRowSecurityManager()->ManageHierarchies(m_pHierarchies);
	}
	CATCH(SqlException, e)
	{
		SAFE_DELETE(m_pResources);
		SAFE_DELETE(m_pHierarchies);
		e->ShowError();
		return FALSE;
	}
	END_CATCH

	SAFE_DELETE(m_pResources);
	SAFE_DELETE(m_pHierarchies);

	return TRUE;
}

// leggo le righe nella tabella OM_ResourcesDetails ed esplodo i vari child
/*
SELECT D.ResourceType,D.ResourceCode,IsWorker,ChildResourceType,ChildResourceCode
FROM OM_ResourcesDetails D, OM_Resources R
WHERE D.ResourceCode = R.ResourceCode AND D.ResourceType = R.ResourceType AND R.[Disabled] = '0'
ORDER BY ResourceType,ResourceCode,IsWorker,ChildResourceType,ChildResourceCode
*/
//-----------------------------------------------------------------------------
void BDRSMaintenance::AnalyzeResourcesDetails()
{
	TResourcesDetails aRecResDetail;
	TResources aRecResource;
	SqlTable aTblResDetail(&aRecResDetail, GetReadOnlySqlSession());
	
	TRY
	{
		aTblResDetail.Open();

		aRecResource.SetQualifier();
		aRecResDetail.SetQualifier();

		aTblResDetail.FromTable(&aRecResource);
		aTblResDetail.FromTable(&aRecResDetail);

		aTblResDetail.Select(aRecResDetail.f_ResourceType);
		aTblResDetail.Select(aRecResDetail.f_ResourceCode);
		aTblResDetail.Select(aRecResDetail.f_IsWorker);
		aTblResDetail.Select(aRecResDetail.f_ChildResourceType);
		aTblResDetail.Select(aRecResDetail.f_ChildResourceCode);
		aTblResDetail.Select(aRecResDetail.f_ChildWorkerID);

		aTblResDetail.AddCompareColumn(aRecResDetail.f_ResourceType, &aRecResource, aRecResource.f_ResourceType);
		aTblResDetail.AddCompareColumn(aRecResDetail.f_ResourceCode, &aRecResource, aRecResource.f_ResourceCode);

		aTblResDetail.AddFilterColumn(&aRecResource, aRecResource.f_Disabled);
		aTblResDetail.AddParam(_T("P1"), aRecResource.f_Disabled);
		aTblResDetail.SetParamValue(_T("P1"), DataBool(FALSE));

		aTblResDetail.AddSortColumn(aRecResDetail.f_ResourceType);
		aTblResDetail.AddSortColumn(aRecResDetail.f_ResourceCode);
		aTblResDetail.AddSortColumn(aRecResDetail.f_IsWorker);
		aTblResDetail.AddSortColumn(aRecResDetail.f_ChildResourceType);
		aTblResDetail.AddSortColumn(aRecResDetail.f_ChildResourceCode);
		aTblResDetail.Query();

		CRSResourcesArray pParentArray;
		pParentArray.SetOwns(FALSE);

		CRSResourceElement* pMasterElem;
		CRSResourceElement* pChildElem;
		int nrLevel = 1;

		CHierarchyElement* pHierarchyElem;

		while (!aTblResDetail.IsEOF())
		{
			pMasterElem = m_pResources->GetResource(-1, FALSE, aRecResDetail.f_ResourceType, aRecResDetail.f_ResourceCode);
			pChildElem = m_pResources->GetResource(aRecResDetail.f_ChildWorkerID, aRecResDetail.f_IsWorker,
				aRecResDetail.f_ChildResourceType, aRecResDetail.f_ChildResourceCode);

			// memorizzo nell'array dei parent il pMasterElem
			pParentArray.Add(pMasterElem);

			// inserisco l'elemento nella gerarchia
			pHierarchyElem = new CHierarchyElement(pMasterElem, pChildElem, nrLevel);
			if (!m_pHierarchies->Contains(pHierarchyElem))
				m_pHierarchies->Add(pHierarchyElem);
			else
				SAFE_DELETE(pHierarchyElem);

			// esplodo i figli
			if (aRecResDetail.f_IsWorker)
				ExploreWorker(aRecResDetail.f_ChildWorkerID, nrLevel, &pParentArray);
			else
				ExploreResource(aRecResDetail.f_ChildResourceType, aRecResDetail.f_ChildResourceCode, nrLevel, &pParentArray);

			aTblResDetail.MoveNext();

			pParentArray.RemoveAll();
		}
		aTblResDetail.Close();
	}
	CATCH(SqlException, e)
	{
		if (aTblResDetail.IsOpen()) 
			aTblResDetail.Close();
		THROW_LAST();
	}
	END_CATCH
}


// leggo le righe nella tabella RM_WorkersDetails ed esplodo i vari child
//-----------------------------------------------------------------------------
void BDRSMaintenance::AnalyzeWorkersDetails()
{
	TWorkers		aRecWorkers;
	TWorkersDetails aRecWorkerDetail;
	SqlTable aTblWorkDetails(&aRecWorkerDetail, GetReadOnlySqlSession());

	TRY
	{
		aRecWorkers.SetQualifier();
		aRecWorkerDetail.SetQualifier();

		aTblWorkDetails.Open();
	/*aTblWorkDetails.m_strSQL = cwsprintf
		(
			_T("SELECT D.WorkerId, D.IsWorker, D.ChildResourceType, D.ChildResourceCode, D.ChildWorkerId FROM RM_WorkersDetails D, RM_Workers W WHERE D.WorkerId = W.WorkerId AND W.IsArchived = %s ORDER BY D.ChildWorkerId"),
			GetSqlConnection()->NativeConvert(&DataBool(FALSE))
		);*/
		aTblWorkDetails.FromTable(&aRecWorkerDetail);
		aTblWorkDetails.FromTable(&aRecWorkers);

		aTblWorkDetails.Select(aRecWorkerDetail.f_WorkerID);
		aTblWorkDetails.Select(aRecWorkerDetail.f_IsWorker);
		aTblWorkDetails.Select(aRecWorkerDetail.f_ChildResourceType);
		aTblWorkDetails.Select(aRecWorkerDetail.f_ChildResourceCode);
		aTblWorkDetails.Select(aRecWorkerDetail.f_ChildWorkerID);


		aTblWorkDetails.AddFilterColumn(&aRecWorkers, aRecWorkers.f_Disabled);
		aTblWorkDetails.AddParam(szP1, aRecWorkers.f_Disabled);
		aTblWorkDetails.SetParamValue(szP1, DataBool(FALSE));

		aTblWorkDetails.AddCompareColumn(aRecWorkerDetail.f_WorkerID, &aRecWorkers, aRecWorkers.f_WorkerID);

		aTblWorkDetails.AddSortColumn(aRecWorkerDetail.f_WorkerID);
		aTblWorkDetails.AddSortColumn(aRecWorkerDetail.f_IsWorker);
		aTblWorkDetails.AddSortColumn(aRecWorkerDetail.f_ChildResourceType);
		aTblWorkDetails.AddSortColumn(aRecWorkerDetail.f_ChildResourceCode);
		aTblWorkDetails.AddSortColumn(aRecWorkerDetail.f_ChildWorkerID);
		aTblWorkDetails.Query();

		CRSResourcesArray pParentArray;
		pParentArray.SetOwns(FALSE);

		CRSResourceElement* pMasterElem;
		CRSResourceElement* pChildElem;
		int nrLevel = 1;

		CHierarchyElement* pHierarchyElem;

		while (!aTblWorkDetails.IsEOF())
		{
			pMasterElem = m_pResources->GetResource(aRecWorkerDetail.f_WorkerID, TRUE, _T(""), _T(""));
			pChildElem = m_pResources->GetResource(aRecWorkerDetail.f_ChildWorkerID, aRecWorkerDetail.f_IsWorker,
				aRecWorkerDetail.f_ChildResourceType, aRecWorkerDetail.f_ChildResourceCode);

			// memorizzo nell'array dei parent il pMasterElem
			pParentArray.Add(pMasterElem);

			// inserisco l'elemento nella gerarchia
			pHierarchyElem = new CHierarchyElement(pMasterElem, pChildElem, nrLevel);
			if (!m_pHierarchies->Contains(pHierarchyElem))
				m_pHierarchies->Add(pHierarchyElem);
			else
				SAFE_DELETE(pHierarchyElem);

			// esplodo i figli
			if (aRecWorkerDetail.f_IsWorker)
				ExploreWorker(aRecWorkerDetail.f_ChildWorkerID, nrLevel, &pParentArray);
			else
				ExploreResource(aRecWorkerDetail.f_ChildResourceType, aRecWorkerDetail.f_ChildResourceCode, nrLevel, &pParentArray);

			aTblWorkDetails.MoveNext();

			pParentArray.RemoveAll();
		}
		aTblWorkDetails.Close();
	}
	CATCH(SqlException, e)
	{
		if (aTblWorkDetails.IsOpen()) aTblWorkDetails.Close();
		THROW_LAST();
	}
	END_CATCH
}

// Esplosione worker
//-----------------------------------------------------------------------------
void BDRSMaintenance::ExploreWorker(int aWorkerId, int aNrLevel, CRSResourcesArray* pParentArray)
{
	TWorkers		aRecWorkers;
	TWorkersDetails aRecWorkerDetails;
	SqlTable aTblWorkerDetails(&aRecWorkerDetails, GetReadOnlySqlSession());

	Array arWorkDetails;
	arWorkDetails.SetOwns(FALSE);

	TRY
	{
		aRecWorkers.SetQualifier();
		aRecWorkerDetails.SetQualifier();
		aTblWorkerDetails.Open();

	/*aTblWorkerDetails.m_strSQL = cwsprintf
	(
		_T("SELECT D.IsWorker, D.ChildResourceType, D.ChildResourceCode, D.ChildWorkerId FROM RM_WorkersDetails D, RM_Workers W WHERE D.WorkerId = %s AND D.ChildWorkerId = W.WorkerId AND W.IsArchived = %s ORDER BY D.ChildWorkerId"),
		GetSqlConnection()->NativeConvert(&DataInt(aWorkerId)),
		GetSqlConnection()->NativeConvert(&DataBool(FALSE))
	);*/

		aTblWorkerDetails.FromTable(&aRecWorkerDetails);
		aTblWorkerDetails.FromTable(&aRecWorkers);

		aTblWorkerDetails.Select(aRecWorkerDetails.f_IsWorker);
		aTblWorkerDetails.Select(aRecWorkerDetails.f_ChildResourceType);
		aTblWorkerDetails.Select(aRecWorkerDetails.f_ChildResourceCode);
		aTblWorkerDetails.Select(aRecWorkerDetails.f_ChildWorkerID);	
	
		aTblWorkerDetails.AddFilterColumn(aRecWorkerDetails.f_WorkerID);
		aTblWorkerDetails.AddParam(szP1, aRecWorkerDetails.f_WorkerID);
		aTblWorkerDetails.SetParamValue(szP1, DataInt(aWorkerId));

		aTblWorkerDetails.AddFilterColumn(&aRecWorkers, aRecWorkers.f_Disabled);
		aTblWorkerDetails.AddParam(szP2, aRecWorkers.f_Disabled);
		aTblWorkerDetails.SetParamValue(szP2, DataBool(FALSE));

		aTblWorkerDetails.AddCompareColumn(aRecWorkerDetails.f_ChildWorkerID, &aRecWorkers, aRecWorkers.f_WorkerID);
	
		aTblWorkerDetails.AddSortColumn(aRecWorkerDetails.f_ChildWorkerID);
		aTblWorkerDetails.AddSortColumn(aRecWorkerDetails.f_IsWorker);
		aTblWorkerDetails.AddSortColumn(aRecWorkerDetails.f_ChildResourceType);
		aTblWorkerDetails.AddSortColumn(aRecWorkerDetails.f_ChildResourceCode);

		aTblWorkerDetails.Query();

		while (!aTblWorkerDetails.IsEOF())
		{
			arWorkDetails.Add(m_pResources->GetResource(aRecWorkerDetails.f_ChildWorkerID, aRecWorkerDetails.f_IsWorker,
				aRecWorkerDetails.f_ChildResourceType, aRecWorkerDetails.f_ChildResourceCode));
			aTblWorkerDetails.MoveNext();
		}
		aTblWorkerDetails.Close();

		CRSResourceElement* pMasterElem;
		CRSResourceElement* pChildElem;

		CHierarchyElement* pHierarchyElem;
		CHierarchyElement* pParentHierarchyElem;

		for (int i = 0; i <= arWorkDetails.GetUpperBound(); i++)
		{
			pMasterElem = m_pResources->GetResource(aWorkerId, TRUE, _T(""), _T(""));
			pChildElem = ((CRSResourceElement*)(arWorkDetails.GetAt(i)));

			// memorizzo nell'array dei parent il pMasterElem
			pParentArray->Add(pMasterElem);

			// inserisco l'elemento nella gerarchia
			pHierarchyElem = new CHierarchyElement(pMasterElem, pChildElem, aNrLevel);
			if (!m_pHierarchies->Contains(pHierarchyElem))
				m_pHierarchies->Add(pHierarchyElem);
			else
				SAFE_DELETE(pHierarchyElem);

			// aggiungo i legami con i nodi parent
			int level = aNrLevel;
			for (int i = pParentArray->GetUpperBound(); i >= 0; i--)
			{
				if (!m_pHierarchies->MatchElements(pParentArray->GetAt(i), pMasterElem))
				{
					level++;
					pParentHierarchyElem = new CHierarchyElement(pParentArray->GetAt(i), pChildElem, level);
					if (!m_pHierarchies->Contains(pParentHierarchyElem))
						m_pHierarchies->Add(pParentHierarchyElem);
					else
						SAFE_DELETE(pParentHierarchyElem);
				}
			}

			// esplodo i figli
			if (pChildElem->m_IsWorker)
				ExploreWorker(pChildElem->m_WorkerID, aNrLevel, pParentArray);
			else
				ExploreResource(pChildElem->m_ResourceType, pChildElem->m_ResourceCode, aNrLevel, pParentArray);
		}

		if (pParentArray->GetCount() > 0)
			pParentArray->RemoveAt(pParentArray->GetUpperBound());
	}
	CATCH(SqlException, e)
	{
		if (aTblWorkerDetails.IsOpen()) aTblWorkerDetails.Close();
		THROW_LAST();
	}
	END_CATCH
}

// Esplosione risorsa

/*
SELECT D.ResourceType,D.ResourceCode,IsWorker,ChildResourceType,ChildResourceCode
FROM RM_ResourcesDetails D, RM_Resources R
WHERE D.ResourceType = aChildResourceCode AND D.ResourceCode = aChildResourceCode AND D.ResourceCode = R.ResourceCode AND D.ResourceType = R.ResourceType AND R.[Disabled] = '0'
ORDER BY D.ResourceType, D.ResourceCode, IsWorker, ChildResourceType, ChildResourceCode
*/
//-----------------------------------------------------------------------------
void BDRSMaintenance::ExploreResource(const DataStr& aChildResourceType, const DataStr& aChildResourceCode, int aNrLevel, CRSResourcesArray* pParentArray)
{
	TResources aRecResources;

	TResourcesDetails aRecResourcesDetails;
	SqlTable aTblResourcesDetails(&aRecResourcesDetails, GetReadOnlySqlSession());

	Array arResourcesDetails;
	arResourcesDetails.SetOwns(FALSE);

	TRY
	{
		aRecResources.SetQualifier();
		aRecResourcesDetails.SetQualifier();

		aTblResourcesDetails.Open();
	/*aTblResourcesDetails.m_strSQL = cwsprintf
	(
		_T("SELECT D.IsWorker, D.ChildResourceType, D.ChildResourceCode, D.WorkerId FROM RM_ResourcesDetails D, RM_Resources R WHERE D.ResourceType = %s AND D.ResourceCode = %s AND D.ResourceCode = R.ResourceCode AND D.ResourceType = R.ResourceType AND R.Disabled = %s ORDER BY D.ResourceType, D.ResourceCode, IsWorker, ChildResourceType, ChildResourceCode"),
		GetSqlConnection()->NativeConvert(&aChildResourceType),
		GetSqlConnection()->NativeConvert(&aChildResourceCode),
		GetSqlConnection()->NativeConvert(&DataBool(FALSE))
	);
*/
		aTblResourcesDetails.FromTable(&aRecResourcesDetails);
		aTblResourcesDetails.FromTable(&aRecResources);

		aTblResourcesDetails.Select(aRecResourcesDetails.f_IsWorker);
		aTblResourcesDetails.Select(aRecResourcesDetails.f_ChildResourceType);
		aTblResourcesDetails.Select(aRecResourcesDetails.f_ChildResourceCode);
		aTblResourcesDetails.Select(aRecResourcesDetails.f_ChildWorkerID);

		aTblResourcesDetails.AddFilterColumn(aRecResourcesDetails.f_ResourceType);
		aTblResourcesDetails.AddParam(szP1, aRecResourcesDetails.f_ResourceType);
		aTblResourcesDetails.SetParamValue(szP1, aChildResourceType);

		aTblResourcesDetails.AddFilterColumn(aRecResourcesDetails.f_ResourceCode);
		aTblResourcesDetails.AddParam(szP2, aRecResourcesDetails.f_ResourceCode);
		aTblResourcesDetails.SetParamValue(szP2, aChildResourceCode);
	
		aTblResourcesDetails.AddFilterColumn(&aRecResources, aRecResources.f_Disabled);
		aTblResourcesDetails.AddParam(szP3, aRecResources.f_Disabled);
		aTblResourcesDetails.SetParamValue(szP3, DataBool(FALSE));

		aTblResourcesDetails.AddCompareColumn(aRecResourcesDetails.f_ResourceType, &aRecResources, aRecResources.f_ResourceType);
		aTblResourcesDetails.AddCompareColumn(aRecResourcesDetails.f_ResourceCode, &aRecResources, aRecResources.f_ResourceCode);

		aTblResourcesDetails.AddSortColumn(aRecResourcesDetails.f_ResourceType);
		aTblResourcesDetails.AddSortColumn(aRecResourcesDetails.f_ResourceCode);
		aTblResourcesDetails.AddSortColumn(aRecResourcesDetails.f_IsWorker);
		aTblResourcesDetails.AddSortColumn(aRecResourcesDetails.f_ChildResourceType);
		aTblResourcesDetails.AddSortColumn(aRecResourcesDetails.f_ChildResourceCode);
		aTblResourcesDetails.AddSortColumn(aRecResourcesDetails.f_ChildWorkerID);


		aTblResourcesDetails.Query();

		while (!aTblResourcesDetails.IsEOF())
		{
			arResourcesDetails.Add(m_pResources->GetResource(aRecResourcesDetails.f_ChildWorkerID, aRecResourcesDetails.f_IsWorker,
				aRecResourcesDetails.f_ChildResourceType, aRecResourcesDetails.f_ChildResourceCode));
			aTblResourcesDetails.MoveNext();
		}
		aTblResourcesDetails.Close();

		CRSResourceElement* pMasterElem;
		CRSResourceElement* pChildElem;

		CHierarchyElement* pHierarchyElem;
		CHierarchyElement* pParentHierarchyElem;

		for (int i = 0; i <= arResourcesDetails.GetUpperBound(); i++)
		{
			pMasterElem = m_pResources->GetResource(-1, FALSE, aChildResourceType, aChildResourceCode);
			pChildElem = ((CRSResourceElement*)(arResourcesDetails.GetAt(i)));

			// memorizzo nell'array dei parent il pMasterElem
			pParentArray->Add(pMasterElem);

			// inserisco l'elemento nella gerarchia
			pHierarchyElem = new CHierarchyElement(pMasterElem, pChildElem, aNrLevel);
			if (!m_pHierarchies->Contains(pHierarchyElem))
				m_pHierarchies->Add(pHierarchyElem);
			else
				SAFE_DELETE(pHierarchyElem);

			// aggiungo i legami con i nodi parent
			int level = aNrLevel;
			for (int i = pParentArray->GetUpperBound(); i >= 0; i--)
			{
				if (!m_pHierarchies->MatchElements(pParentArray->GetAt(i), pMasterElem))
				{
					level++;
					pParentHierarchyElem = new CHierarchyElement(pParentArray->GetAt(i), pChildElem, level);
					if (!m_pHierarchies->Contains(pParentHierarchyElem))
						m_pHierarchies->Add(pParentHierarchyElem);
					else
						SAFE_DELETE(pParentHierarchyElem);
				}
			}

			// esplodo i figli
			if (pChildElem->m_IsWorker)
				ExploreWorker(pChildElem->m_WorkerID, aNrLevel, pParentArray);
			else
				ExploreResource(pChildElem->m_ResourceType, pChildElem->m_ResourceCode, aNrLevel, pParentArray);
		}

		if (pParentArray->GetCount() > 0)
			pParentArray->RemoveAt(pParentArray->GetUpperBound());
	}
	CATCH(SqlException, e)
	{
		if (aTblResourcesDetails.IsOpen()) aTblResourcesDetails.Close();
		THROW_LAST();
	}
	END_CATCH
}