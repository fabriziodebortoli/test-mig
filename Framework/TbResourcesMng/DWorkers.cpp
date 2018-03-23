
#include "stdafx.h"

#include <TbFrameworkImages\CommonImages.h>
#include <TbFrameworkImages\GeneralFunctions.h>

#include "RMControls.h"
#include "TResources.h"
#include "TWorkers.h"
#include "TAbsenceReasons.h"
#include "UIWorkers.h" 
#include "DWorkers.h"  

#include "ModuleObjects\Workers\JsonForms\IDD_WORKERS.hjson"

#ifdef _DEBUG
#undef THIS_FILE
static char THIS_FILE[] = __FILE__;
#endif

static TCHAR szParamWorkerID [] = _T("p1");
static TCHAR szParamIsWorker [] = _T("p2");

//////////////////////////////////////////////////////////////////////////////
//             class DBTWorkers implementation
//////////////////////////////////////////////////////////////////////////////
//============================================================================
IMPLEMENT_DYNAMIC(DBTWorkers, DBTMaster)

//-----------------------------------------------------------------------------	
DBTWorkers::DBTWorkers
	(
		CRuntimeClass*		pClass, 
		CAbstractFormDoc*	pDocument
	)
	:
	DBTMaster (pClass, pDocument, _NS_DBT("Workers"))
{
	BindAutoincrement(&GetWorkers()->f_WorkerID, _T("Framework.TbResourcesMng.Workers.WorkerId"));
}

//-----------------------------------------------------------------------------
void DBTWorkers::OnDefineQuery ()
{   
	m_pTable->SelectAll();
	m_pTable->AddParam			(szParamWorkerID, GetWorkers()->f_WorkerID);
	m_pTable->AddFilterColumn	(GetWorkers()->f_WorkerID);
	m_pTable->AddSortColumn		(GetWorkers()->f_WorkerID);
}

//-----------------------------------------------------------------------------
void DBTWorkers::OnPrepareQuery ()
{   
	m_pTable->SetParamValue(szParamWorkerID, GetWorkers()->f_WorkerID);
}

//-----------------------------------------------------------------------------
void DBTWorkers::OnDisableControlsForAddNew ()
{
	GetDocument()->UpdateDataView();
}

//-----------------------------------------------------------------------------
void DBTWorkers::OnDisableControlsForEdit ()
{
	GetWorkers()->f_CompanyLogin.SetReadOnly(GetWorkers()->f_Disabled);
}

///////////////////////////////////////////////////////////////////////////////
//             class DBTWorkersFields implementation
///////////////////////////////////////////////////////////////////////////////
//=============================================================================
IMPLEMENT_DYNAMIC(DBTWorkersFields, DBTSlaveBuffered)

//-----------------------------------------------------------------------------	
DBTWorkersFields::DBTWorkersFields
	(
		CRuntimeClass*		pClass, 
		CAbstractFormDoc*	pDocument
	)
	:
	DBTSlaveBuffered (pClass, pDocument, _NS_DBT("WorkersFields"), ALLOW_EMPTY_BODY, TRUE)
{
}

//-----------------------------------------------------------------------------	
void DBTWorkersFields::OnDefineQuery ()
{
	m_pTable->SelectAll			();
	m_pTable->AddParam			(szParamWorkerID, GetWorkersField()->f_WorkerID);
	m_pTable->AddFilterColumn	(GetWorkersField()->f_WorkerID);
	m_pTable->AddSortColumn		(GetWorkersField()->f_Line);
}

//-----------------------------------------------------------------------------
void DBTWorkersFields::OnPrepareQuery ()
{
	m_pTable->SetParamValue(szParamWorkerID, GetWorkers()->f_WorkerID);
}

//-----------------------------------------------------------------------------
void DBTWorkersFields::OnPreparePrimaryKey(int nRow, SqlRecord* pSqlRec)
{
	ASSERT (pSqlRec->IsKindOf(RUNTIME_CLASS(TWorkersFields)));
	TWorkersFields* pRec = (TWorkersFields*) pSqlRec;

	pRec->f_WorkerID	= GetWorkers()->f_WorkerID;
	pRec->f_Line		= nRow;
	pRec->SetStorable(TRUE);
}

//-----------------------------------------------------------------------------
DataObj* DBTWorkersFields::GetDuplicateKeyPos(SqlRecord* pRec)
{
	ASSERT (pRec->IsKindOf(RUNTIME_CLASS(TWorkersFields)));
	return &((TWorkersFields*)pRec)->f_Line;
}

//-----------------------------------------------------------------------------
void DBTWorkersFields::OnAfterInsertRow(int nRow, SqlRecord* pRec)
{
	DBTSlaveBuffered::OnAfterInsertRow(nRow, pRec);
	CheckRows(nRow);
}

//-----------------------------------------------------------------------------
void DBTWorkersFields::OnAfterAddRow(int nRow, SqlRecord* pRec)
{
	DBTSlaveBuffered::OnAfterAddRow(nRow, pRec);
	CheckRows(nRow);
}

//-----------------------------------------------------------------------------
void DBTWorkersFields::CheckRows(int nRow)
{
	int nStart = nRow + 1;
	TWorkersFields* pRec;

	for (int i = nStart; i <= GetUpperBound(); i++)
	{
		pRec = GetWorkersField(i);
		pRec->f_Line = i + 1; 
		pRec->SetStorable(!pRec->f_FieldName.IsEmpty());
	}
	GetDocument()->UpdateDataView();
}

//-----------------------------------------------------------------------------	
DataObj* DBTWorkersFields::OnCheckUserData(int nRow)
{ 
	if (GetDocument()->IsInUnattendedMode())
		return NULL; 

	TWorkersFields* pRec = GetWorkersField(nRow);

	if (!pRec->f_FieldValue.IsEmpty() && pRec->f_FieldName.IsEmpty())
	{
		SetError(_TB("The field name is mandatory"));
		return &pRec->f_FieldName;
	}
	return NULL;
}

///////////////////////////////////////////////////////////////////////////////
//             class DBTWorkersArrangement implementation
///////////////////////////////////////////////////////////////////////////////
//=============================================================================
IMPLEMENT_DYNAMIC(DBTWorkersArrangement, DBTSlaveBuffered)

//-----------------------------------------------------------------------------	
DBTWorkersArrangement::DBTWorkersArrangement
	(
		CRuntimeClass*		pClass, 
		CAbstractFormDoc*	pDocument
	)
	:
	DBTSlaveBuffered (pClass, pDocument, _NS_DBT("WorkersArrangements"), ALLOW_EMPTY_BODY, TRUE)
{
}

//-----------------------------------------------------------------------------	
void DBTWorkersArrangement::OnDefineQuery ()
{
	m_pTable->SelectAll			();
	m_pTable->AddParam			(szParamWorkerID, GetWorkersArrangement()->f_WorkerID);
	m_pTable->AddFilterColumn	(GetWorkersArrangement()->f_WorkerID);
	m_pTable->AddSortColumn		(GetWorkersArrangement()->f_Line);
}

//-----------------------------------------------------------------------------
void DBTWorkersArrangement::OnPrepareQuery ()
{
	m_pTable->SetParamValue(szParamWorkerID, GetWorkers()->f_WorkerID);
}

//-----------------------------------------------------------------------------
void DBTWorkersArrangement::OnPreparePrimaryKey(int nRow, SqlRecord* pSqlRec)
{
	ASSERT (pSqlRec->IsKindOf(RUNTIME_CLASS(TWorkersArrangement)));
	TWorkersArrangement* pRec = (TWorkersArrangement*) pSqlRec;

	pRec->f_WorkerID= GetWorkers()->f_WorkerID;
	pRec->f_Line	= nRow;
	pRec->SetStorable(TRUE);
}

//-----------------------------------------------------------------------------
DataObj* DBTWorkersArrangement::GetDuplicateKeyPos(SqlRecord* pRec)
{
	ASSERT (pRec->IsKindOf(RUNTIME_CLASS(TWorkersArrangement)));
	return &((TWorkersArrangement*)pRec)->f_Line;
}

//-----------------------------------------------------------------------------
void DBTWorkersArrangement::OnAfterInsertRow(int nRow, SqlRecord* pReci)
{
	DBTSlaveBuffered::OnAfterInsertRow(nRow,pReci);

	int nStart = nRow + 1;
	TWorkersArrangement* pRec;

	for (int i = nStart; i <= GetUpperBound(); i++)
	{
		pRec = GetWorkersArrangement(i);
		pRec->f_Line = i + 1; 
		pRec->SetStorable(!pRec->f_Arrangement.IsEmpty());
	}

	GetDocument()->UpdateDataView();
}

///////////////////////////////////////////////////////////////////////////////
//             class DBTWorkersAbsences implementation
///////////////////////////////////////////////////////////////////////////////
//=============================================================================
IMPLEMENT_DYNAMIC(DBTWorkersAbsences, DBTSlaveBuffered)

//-----------------------------------------------------------------------------	
DBTWorkersAbsences::DBTWorkersAbsences
	(
		CRuntimeClass*		pClass, 
		CAbstractFormDoc*	pDocument
	)
	:
	DBTSlaveBuffered (pClass, pDocument, _NS_DBT("WorkersAbsences"), ALLOW_EMPTY_BODY, TRUE)
{
}

//-----------------------------------------------------------------------------	
void DBTWorkersAbsences::OnDefineQuery()
{
	m_pTable->SelectAll			();
	m_pTable->AddParam			(szParamWorkerID, GetWorkersAbsences()->f_WorkerID);
	m_pTable->AddFilterColumn	(GetWorkersAbsences()->f_WorkerID);
	m_pTable->AddSortColumn		(GetWorkersAbsences()->f_StartingDate);
}

//-----------------------------------------------------------------------------
void DBTWorkersAbsences::OnPrepareQuery()
{
	m_pTable->SetParamValue(szParamWorkerID, GetWorkers()->f_WorkerID);
}

//-----------------------------------------------------------------------------
void DBTWorkersAbsences::OnPreparePrimaryKey(int nRow, SqlRecord* pSqlRec)
{
	ASSERT(pSqlRec->IsKindOf(RUNTIME_CLASS(TWorkersAbsences)));
	TWorkersAbsences* pRec = (TWorkersAbsences*)pSqlRec;

	pRec->f_WorkerID = GetWorkers()->f_WorkerID;
	pRec->SetStorable(TRUE);
}

//-----------------------------------------------------------------------------
DataObj* DBTWorkersAbsences::GetDuplicateKeyPos(SqlRecord* pRec)
{
	ASSERT(pRec->IsKindOf(RUNTIME_CLASS(TWorkersAbsences)));
	return &((TWorkersAbsences*)pRec)->f_Reason;
}

//-----------------------------------------------------------------------------
DataObj* DBTWorkersAbsences::OnCheckPrimaryKey(int nRow, SqlRecord* pSqlRec)
{
	ASSERT(pSqlRec->IsKindOf(RUNTIME_CLASS(TWorkersAbsences)));
	TWorkersAbsences* pRec = (TWorkersAbsences*)pSqlRec;

	if	(pRec->f_Reason.IsEmpty() && !pRec->f_StartingDate.IsEmpty())
	{
		SetError(_TB("The Reason code cannot be empty"));
		return &(pRec->f_Reason);
	}
	
	if	(!pRec->f_Reason.IsEmpty() && pRec->f_StartingDate.IsEmpty())
	{
		SetError(_TB("The Starting Date cannot be empty"));
		return &(pRec->f_StartingDate);
	}

	return NULL;
}

//-----------------------------------------------------------------------------
void DBTWorkersAbsences::OnPrepareAuxColumns(SqlRecord* pSqlRec)
{
	TWorkersAbsences* pRec = (TWorkersAbsences*)pSqlRec;
	pRec->l_ManagerDes = GetDocument()->GetHotLink<HKLWorkers>(L"Workers")->GetNameComplete();

}

//-----------------------------------------------------------------------------	
DataObj* DBTWorkersAbsences::OnCheckUserData(int nRow)
{
	((TWorkersAbsences*)GetWorkersAbsences(nRow))->SetStorable(!GetWorkersAbsences(nRow)->f_Reason.IsEmpty());
	return NULL;
}

///////////////////////////////////////////////////////////////////////////////
//             class DBTWorkersDetails implementation
///////////////////////////////////////////////////////////////////////////////
//=============================================================================
IMPLEMENT_DYNAMIC(DBTWorkersDetails, DBTSlaveBuffered)

//-----------------------------------------------------------------------------	
DBTWorkersDetails::DBTWorkersDetails
	(
	CRuntimeClass*		pClass,
	CAbstractFormDoc*	pDocument
	)
	:
	DBTSlaveBuffered(pClass, pDocument, _NS_DBT("WorkersDetails"), ALLOW_EMPTY_BODY, TRUE)
{
}

//-----------------------------------------------------------------------------	
void DBTWorkersDetails::OnDefineQuery()
{
	m_pTable->SelectAll			();
	m_pTable->AddParam			(szParamWorkerID, GetWorkersDetails()->f_WorkerID);
	m_pTable->AddFilterColumn	(GetWorkersDetails()->f_WorkerID);
	m_pTable->AddSortColumn		(GetWorkersDetails()->f_WorkerID);
	m_pTable->AddSortColumn		(GetWorkersDetails()->f_ChildResourceCode);
}

//-----------------------------------------------------------------------------
void DBTWorkersDetails::OnPrepareQuery()
{
	m_pTable->SetParamValue(szParamWorkerID, GetWorkers()->f_WorkerID);
}

//-----------------------------------------------------------------------------
void DBTWorkersDetails::OnPreparePrimaryKey(int nRow, SqlRecord* pSqlRec)
{
	ASSERT(pSqlRec->IsKindOf(RUNTIME_CLASS(TWorkersDetails)));
	TWorkersDetails* pRec = (TWorkersDetails*)pSqlRec;

	pRec->f_WorkerID = GetWorkers()->f_WorkerID;
	pRec->SetStorable(TRUE);
}

//-----------------------------------------------------------------------------
DataObj* DBTWorkersDetails::GetDuplicateKeyPos(SqlRecord* pRec)
{
	ASSERT(pRec->IsKindOf(RUNTIME_CLASS(TWorkersDetails)));
	return &((TWorkersDetails*)pRec)->f_IsWorker;
}

//-----------------------------------------------------------------------------
DataObj* DBTWorkersDetails::OnCheckPrimaryKey(int nRow, SqlRecord* pSqlRec)
{
	ASSERT(pSqlRec->IsKindOf(RUNTIME_CLASS(TWorkersDetails)));
	TWorkersDetails* pRec = (TWorkersDetails*)pSqlRec;

	if (pRec->f_IsWorker)
	{
		if (pRec->f_ChildWorkerID.IsEmpty())
		{
			SetError(_TB("The Worker cannot be empty"));
			return &(pRec->f_ChildWorkerID);
		}
	}
	else
	{
		if (pRec->f_ChildResourceType.IsEmpty() || pRec->f_ChildResourceCode.IsEmpty())
		{
			SetError(_TB("The Resource Type and/or the Resource Code cannot be empty"));
			return &(pRec->f_ChildResourceType);
		}
	}

	return NULL;
}

//-----------------------------------------------------------------------------
void DBTWorkersDetails::OnPrepareAuxColumns(SqlRecord* pSqlRec)
{
	TWorkersDetails* pRec = (TWorkersDetails*)pSqlRec;

	//TODO
	if (pRec->f_IsWorker)
	{
		GetDocument()->GetHotLink<HKLWorkers>(L"Workers")->FindRecord(&pRec->f_ChildWorkerID); //LARA ANNA
		pRec->l_WorkerDesc = GetDocument()->GetHotLink<HKLWorkers>(L"Workers")->GetNameComplete();
	}
	else
	{
		GetDocument()->GetHotLink<HKLResources>(L"ResourcesCode")->SetCodeType(pRec->f_ChildResourceType);
		GetDocument()->GetHotLink<HKLResources>(L"ResourcesCode")->FindRecord(&pRec->f_ChildResourceCode); 
		pRec->l_WorkerDesc = GetDocument()->GetHotLink<HKLResources>(L"ResourcesCode")->GetRecord()->f_Description;
		GetDocument()->m_pTRWorkers->FindRecord(GetDocument()->GetHotLink<HKLResources>(L"ResourcesCode")->GetRecord()->f_Manager);
		pRec->l_ManagerDesc = GetDocument()->GetHotLink<HKLWorkers>(L"Workers")->GetNameComplete();
	}
}

//-----------------------------------------------------------------------------
DataObj* DBTWorkersDetails::OnCheckUserData(int nRow)
{
	TWorkersDetails* pRec = (TWorkersDetails*)GetWorkersDetails(nRow);
	if (pRec->f_IsWorker)
		pRec->SetStorable(pRec->f_ChildWorkerID > 0);
	else
		pRec->SetStorable(!pRec->f_ChildResourceType.IsEmpty() || !pRec->f_ChildResourceCode.IsEmpty());
	
	return NULL;
}

//-----------------------------------------------------------------------------
void DBTWorkersDetails::OnDisableControlsForEdit()
{
	for (int i = 0; i <= GetUpperBound(); i++)
	{
		TWorkersDetails* pRec = GetWorkersDetails(i);
		if (pRec->f_IsWorker)
		{
			pRec->f_ChildResourceType.SetReadOnly();
			pRec->f_ChildResourceCode.SetReadOnly();
		}
		else
			pRec->f_ChildWorkerID.SetReadOnly();
	}
}

//-----------------------------------------------------------------------------
void DBTWorkersDetails::OnPrepareRow(int nRow, SqlRecord* pSqlRec)
{
	ASSERT(pSqlRec->IsKindOf(RUNTIME_CLASS(TWorkersDetails)));
	TWorkersDetails* pRec = (TWorkersDetails*)pSqlRec;
	if (pRec->f_IsWorker)
	{
		pRec->f_ChildResourceType.SetReadOnly();
		pRec->f_ChildResourceCode.SetReadOnly();
	}
	else
		pRec->f_ChildWorkerID.SetReadOnly();
}

///////////////////////////////////////////////////////////////////////////////
//                         	DWorkers								
///////////////////////////////////////////////////////////////////////////////
//-----------------------------------------------------------------------------
IMPLEMENT_DYNCREATE(DWorkers, CAbstractFormDoc)

//-----------------------------------------------------------------------------
BEGIN_MESSAGE_MAP(DWorkers, CAbstractFormDoc)
	ON_EN_VALUE_CHANGED(IDC_WRK_ABSENCES_BE_MANAGER,	OnManagerChanged)
	ON_EN_VALUE_CHANGED(IDC_WRK_ARRANG_BE_FROMDATE,		OnArrangementFromDateChanged)
	ON_EN_VALUE_CHANGED(IDC_WRK_ARRANG_BE_TODATE,		OnArrangementToDateChanged)
	ON_EN_VALUE_CHANGED(IDC_WRK_ABSENCES_BE_STARTDATE,	OnAbsencesFromDateChanged)
	ON_EN_VALUE_CHANGED(IDC_WRK_ABSENCES_BE_TODATE,		OnAbsencesToDateChanged)
	ON_EN_VALUE_CHANGED(IDC_WRK_ARRANG_BE_ARRANG,		OnArrangementChanged)
	ON_EN_VALUE_CHANGED(IDC_WRK_LOGIN_PIN,				OnPINChanged)
	ON_EN_VALUE_CHANGED(IDC_WRK_HEAD_DISABLED,			OnIsDisabledChanged)
	ON_EN_VALUE_CHANGED(IDC_WRK_DETAILS_BE_TYPE,		OnResourceTypeChanged)
	ON_EN_VALUE_CHANGED(IDC_WRK_DETAILS_BE_CODE,		OnResourceCodeChanged)
	ON_EN_VALUE_CHANGED(IDC_WRK_DETAILS_BE_WORKER,		OnWorkerChanged)
	ON_EN_VALUE_CHANGED(IDC_WRK_DETAILS_BE_IS_WORKER,	OnIsWorkerChanged)
END_MESSAGE_MAP()                                           

//-----------------------------------------------------------------------------
TWorkers* DWorkers::GetWorkers() const { return (TWorkers*)	m_pDBTWorkers->GetRecord(); }

//------------------------------------------------------------------------------ 
DWorkers::DWorkers()
	:
	m_pDBTWorkers				(NULL),
	m_pDBTWorkersArrangement	(NULL),
	m_pDBTWorkersFields			(NULL),
	m_pDBTWorkersAbsences		(NULL),
	m_pDBTWorkersDetails		(NULL),
	m_pSimHKLUser				(NULL),
	m_pTRWorkersByName			(NULL),
	m_pTRWorkersByLogin			(NULL),
	m_pTRWorkersByPIN			(NULL),
	m_pTUWorkers				(NULL),
	m_pTRResources				(NULL),
	m_pTRWorkers				(NULL),
	m_StandardEdition			(FALSE)
{
	m_StandardEdition = AfxGetLoginManager()->GetEdition() == _T("Standard");
}

//-----------------------------------------------------------------------------
BOOL DWorkers::OnAttachData()
{              
	SetFormTitle(_TB("Workers"));

	m_pTRWorkersByName	= new TRWorkersByName(this);
	m_pTRWorkersByLogin = new TRWorkersByLogin(this);
	m_pTRWorkersByPIN	= new TRWorkersByPIN(this);
	m_pTUWorkers		= new TUWorkers(this);
	m_pTRResources		= new TRResources(this);
	m_pTRWorkers		= new TRWorkers(this);

	m_pDBTWorkers				= new DBTWorkers			(RUNTIME_CLASS(TWorkers),			this);
	m_pDBTWorkersArrangement	= new DBTWorkersArrangement	(RUNTIME_CLASS(TWorkersArrangement),this);
	m_pDBTWorkersFields			= new DBTWorkersFields		(RUNTIME_CLASS(TWorkersFields),		this);
	m_pDBTWorkersAbsences		= new DBTWorkersAbsences	(RUNTIME_CLASS(TWorkersAbsences),	this);
	m_pDBTWorkersDetails		= new DBTWorkersDetails		(RUNTIME_CLASS(TWorkersDetails),	this);

	m_pSimHKLUser = new SimHKLUser(); Attach(m_pSimHKLUser,	_T("HKLCompanyLogin"));

	m_pDBTWorkers->Attach(m_pDBTWorkersArrangement);
	m_pDBTWorkers->Attach(m_pDBTWorkersFields);
	m_pDBTWorkers->Attach(m_pDBTWorkersAbsences);
	m_pDBTWorkers->Attach(m_pDBTWorkersDetails);

	RegisterControl(IDC_WRK_FIELDS_BODYEDIT, RUNTIME_CLASS(CWorkersFieldsBodyEdit));

	return Attach(m_pDBTWorkers);
}

//-----------------------------------------------------------------------------
void DWorkers::OnParsedControlCreated(CParsedCtrl* pCtrl)
{
	if (!pCtrl)
		return;

	UINT nIDC = pCtrl->GetCtrlID();

	if (nIDC == IDC_WRK_MASTER_PICTURE)
	{
		CPictureStatic* pPictureStatic = (CPictureStatic*)pCtrl;
		if (pPictureStatic)
			pPictureStatic->OnCtrlStyleBest();
		return;
	}

	if (nIDC == IDC_WRK_MASTER_PATH)
	{
		CNamespaceEdit* pNamespaceEdit = (CNamespaceEdit*)pCtrl;
		if (!pNamespaceEdit) return;
		pNamespaceEdit->SetNamespaceType(CTBNamespace::IMAGE);
		pNamespaceEdit->SetNamespace(GetNamespace());
		CJsonTileDialog* pJsonTileDlg = dynamic_cast<CJsonTileDialog*>(pCtrl->GetCtrlParent());
		CPictureStatic* pPictureStatic = pJsonTileDlg ? dynamic_cast<CPictureStatic*>(pJsonTileDlg->GetDlgItem(IDC_WRK_MASTER_PICTURE)) : NULL;
		if (pPictureStatic)
			pNamespaceEdit->AttachPicture(pPictureStatic);
		return;
	}

	if (nIDC == IDC_WRK_MASTER_ADDRESS)
	{
		CAddressEdit* pEAddressEdit = (CAddressEdit*)pCtrl;
		if (!pEAddressEdit) return;
		pEAddressEdit->BindCity(&(GetWorkers()->f_DomicilyCity));
		pEAddressEdit->BindCounty(&(GetWorkers()->f_DomicilyCounty));
		pEAddressEdit->BindZip(&(GetWorkers()->f_DomicilyZip));
		pEAddressEdit->BindCountry(&(GetWorkers()->f_DomicilyCountry));
		pEAddressEdit->BindLatitude(&GetWorkers()->f_Latitude);
		pEAddressEdit->BindLongitude(&GetWorkers()->f_Longitude);
		return;
	}
}

//-----------------------------------------------------------------------------
BOOL DWorkers::OnRunReport(CWoormInfo* pWoormInfo)
{
	if (!pWoormInfo)
		return TRUE;

	pWoormInfo->AddParam(_NS_WRMVAR("w_AskDialog"),			&DataBool(FALSE));
	pWoormInfo->AddParam(_NS_WRMVAR("w_UseWorkerIdFilter"),	&DataBool(TRUE));
	pWoormInfo->AddParam(_NS_WRMVAR("w_WorkerIdFilter"),	&GetWorkers()->f_WorkerID); 
	
	return TRUE;
}

//-----------------------------------------------------------------------------
void DWorkers::DeleteContents()
{
	SAFE_DELETE(m_pTRWorkersByName);
	SAFE_DELETE(m_pTRWorkersByPIN);
	SAFE_DELETE(m_pTUWorkers);
	SAFE_DELETE(m_pTRResources);
	SAFE_DELETE(m_pTRWorkers);
	SAFE_DELETE(m_pTRWorkersByLogin);

	return CAbstractFormDoc::DeleteContents();
}

//-----------------------------------------------------------------------------
BOOL DWorkers::CanDoNewRecord()
{
	return !m_StandardEdition && CAbstractFormDoc::CanDoNewRecord();
}

//-----------------------------------------------------------------------------
BOOL DWorkers::OnPrepareAuxData()    
{	
	DataDbl pin;
	BOOL    bIsEqual = TRUE;
	
	if (((GetFormMode() == CBaseDocument::NEW) || (GetFormMode() == CBaseDocument::EDIT)) && GetWorkers()->f_PIN.IsEmpty())
	{
		while (bIsEqual)
		{
			pin = ((double)rand() / (RAND_MAX + 1) * (99999999 + 1 - 10000000) + 10000000);
			GetWorkers()->f_PIN = pin.Str(8, 0);
			m_pTRWorkersByPIN->ExcludeWorkerID(GetWorkers()->f_WorkerID);
			m_pTRWorkersByPIN->SetForceQuery();
			if (m_pTRWorkersByPIN->FindRecord(GetWorkers()->f_PIN) != TableReader::FOUND)
				bIsEqual = FALSE;

		}
		SetModifiedFlag();
	}

	if (GetWorkers()->f_ImagePath.IsEmpty())
		GetWorkers()->f_ImagePath = AfxGetPathFinder()->GetFileNameFromNamespace(TBGlyph(szGlyphWorkerBig), _T(""));

	return	TRUE;
}

//-----------------------------------------------------------------------------
BOOL DWorkers::OnOkDelete()
{
	if (GetWorkers()->f_WorkerID == AfxGetWorkerId())
	{
		Message(_TB("The current worker cannot be deleted"), MB_OK | MB_ICONSTOP);
		return FALSE;
	}

	if (GetWorkers()->f_WorkerID.IsEmpty())
		return FALSE;

	DeleteWorkerResult aResult = CheckIfWorkerIsUsed();

	switch (aResult)
	{
		case DeleteWorkerResult::ERROR_OCCURRED :
		{
			Message(_TB("Error checking if worker has been used"), MB_OK | MB_ICONSTOP);
			return false;
		}

		case DeleteWorkerResult::USED :
		{
			if (GetWorkers()->f_Disabled)
				Message(_TB("A worker that has been already used cannot be deleted"), MB_OK | MB_ICONSTOP);
			else 
				if (Message(_TB("A worker that has been already used cannot be deleted. Do you want to disable it?"), MB_YESNO | MB_ICONQUESTION | MB_DEFBUTTON1) == IDYES)
				{
					m_pTUWorkers->FindRecord(GetWorkers()->f_WorkerID,TRUE);
					m_pTUWorkers->SetAutocommit();
					m_pTUWorkers->GetRecord()->f_Disabled = TRUE;
					m_pTUWorkers->UpdateRecord();
					BrowseRecord();
				}
			return FALSE;
		}

		default: // DeleteWorkerResult::NOT_USED
		{
			m_DeletedWorkerID = GetWorkers()->f_WorkerID;
			return TRUE;
		}
	}
	return TRUE;
}

//-----------------------------------------------------------------------------
BOOL DWorkers::OnOkTransaction()
{
	// se il flag Disabled e' a false devo controllare che sia stata indicata una
	// login e che questa non sia stata assegnata ad un altro worker
	if (!GetWorkers()->f_Disabled)
	{
		// 06/10/2015: per esigenze di calcolo costi ed organigramma si fa salvare il
		// worker anche senza login aziendale (Maurizio e Michela)
		/*if (GetWorkers()->f_CompanyLogin.IsEmpty())
		{
			if (Message(_TB("The company login cannot be empty"), MB_ICONEXCLAMATION))
				return FALSE;
		}*/
		
		if (!GetWorkers()->f_CompanyLogin.IsEmpty())
		{
			m_pTRWorkersByLogin->ExcludeWorkerID(GetWorkers()->f_WorkerID);
			m_pTRWorkersByLogin->SetForceQuery();
			if (m_pTRWorkersByLogin->FindRecord(GetWorkers()->f_CompanyLogin) == TableReader::FOUND)
			{
				if (Message(_TB("A worker with same company login already exists"), MB_ICONEXCLAMATION))
					return FALSE;
			}
		}
	}

	m_pTRWorkersByName->ExcludeWorkerID(GetWorkers()->f_WorkerID);
	if (m_pTRWorkersByName->FindRecord(GetWorkers()->f_Name, GetWorkers()->f_LastName) == TableReader::FOUND)
	{
		if (Message(_TB("A worker with the same name already exists. Do you want to continue?"), MB_YESNO | MB_ICONQUESTION) == IDNO)
			return FALSE;
	}

	m_pTRWorkersByPIN->ExcludeWorkerID(GetWorkers()->f_WorkerID);
	m_pTRWorkersByPIN->SetForceQuery();
	if (m_pTRWorkersByPIN->FindRecord(GetWorkers()->f_PIN) == TableReader::FOUND)
	{
		if (IsInUnattendedMode())
			m_pMessages->Add(_TB("A worker with the same PIN already exists"));
		else
			Message(_TB("A worker with the same PIN already exists"), MB_ICONEXCLAMATION);
		return FALSE;
	}

	if (GetWorkers()->f_PIN.Str().GetLength() < 8)
	{
		if (IsInUnattendedMode())
			m_pMessages->Add(_TB("The PIN length must be at least of 8 digits"));
		else
			Message(_TB("The PIN length must be at least of 8 digits"), MB_ICONEXCLAMATION);
		return FALSE;
	}

	if (GetWorkers()->f_ImagePath == AfxGetPathFinder()->GetFileNameFromNamespace(TBGlyph(szGlyphWorkerBig), _T("")) /*szWorkerBigImage*/)
		GetWorkers()->f_ImagePath.Clear();

	if (GetWorkers()->f_LastName.IsEmpty() && GetWorkers()->f_Name.IsEmpty())
	{
		if (IsInUnattendedMode())
			m_pMessages->Add(_TB("Name or Last name is mandatory"));
		else
			Message(_TB("Name or Last name is mandatory"), MB_ICONEXCLAMATION);
		return FALSE;
	}

	if (GetWorkers()->f_PIN.IsEmpty())
	{
		if (IsInUnattendedMode())
			m_pMessages->Add(_TB("PIN is mandatory"));
		else
			Message(_TB("PIN is mandatory"), MB_ICONEXCLAMATION);
		return FALSE;
	}

	if (!CheckRecursion())
	{
		m_pMessages->Show(TRUE);
		return FALSE;
	}

	return CAbstractFormDoc::OnOkTransaction();
}

//-----------------------------------------------------------------------------
BOOL DWorkers::OnNewTransaction()
{
	BOOL bOK = TRUE;

	// arrivo dal layout e sto inserendo un nuovo figlio ad un padre di tipo risorsa
	if (
		!m_ParentResource.IsEmpty() &&
		Message(cwsprintf(_TB("Do you want to assign {0-%s} {1-%s} to the {2-%s} {3-%s}?"), 
		GetWorkers()->f_Name.GetString(), GetWorkers()->f_LastName.GetString(), m_ParentResourceType.GetString(), m_ParentResource.GetString()), 
		MB_YESNO | MB_ICONQUESTION) == IDYES)
	{
		TUResourcesDetails aTUResources(this);

		if (aTUResources.FindRecord(m_ParentResourceType, m_ParentResource, GetWorkers()->f_WorkerID, TRUE) == TableUpdater::NOT_FOUND)
		{
			aTUResources.GetRecord()->f_ResourceType		= m_ParentResourceType;
			aTUResources.GetRecord()->f_ResourceCode		= m_ParentResource;
			aTUResources.GetRecord()->f_IsWorker			= TRUE;
			aTUResources.GetRecord()->f_ChildResourceType	= DataStr(_T("")); 
			aTUResources.GetRecord()->f_ChildResourceCode	= DataStr(_T(""));
			aTUResources.GetRecord()->f_ChildWorkerID		= GetWorkers()->f_WorkerID;

			bOK = aTUResources.UpdateRecord();
			aTUResources.UnlockCurrent();
		}
	}

	// arrivo dal layout e sto inserendo un nuovo figlio ad un padre di tipo worker
	if (
		!m_ParentWorkerID.IsEmpty() &&
		Message(cwsprintf(_TB("Do you want to assign {0-%s} {1-%s} to the {2-%s}?"),
		GetWorkers()->f_Name.GetString(), GetWorkers()->f_LastName.GetString(), m_ParentWorkerNameComplete.GetString()), 
		MB_YESNO | MB_ICONQUESTION) == IDYES)
	{
		TUWorkersDetails aTUWorkers(this);

		if (aTUWorkers.FindRecord(m_ParentWorkerID, GetWorkers()->f_WorkerID, TRUE) == TableUpdater::NOT_FOUND)
		{
			aTUWorkers.GetRecord()->f_WorkerID			= m_ParentWorkerID;
			aTUWorkers.GetRecord()->f_IsWorker			= TRUE;
			aTUWorkers.GetRecord()->f_ChildResourceType = DataStr(_T(""));
			aTUWorkers.GetRecord()->f_ChildResourceCode = DataStr(_T(""));
			aTUWorkers.GetRecord()->f_ChildWorkerID		= GetWorkers()->f_WorkerID;

			bOK = aTUWorkers.UpdateRecord();
			aTUWorkers.UnlockCurrent();
		}
	}

	return bOK && CAbstractFormDoc::OnNewTransaction();
}

//-----------------------------------------------------------------------------
BOOL DWorkers::OnDeleteTransaction()
{
	BOOL bMessageDone = FALSE;

	// vado a eliminare il riferimento del worker che sto eliminando se figlio di altre risorse
	TResourcesDetails aRec;
	SqlTable aTbl(&aRec, GetUpdatableSqlSession());

	aTbl.Open(TRUE);
	aTbl.SelectAll();

	aTbl.AddParam		(szParamIsWorker, aRec.f_IsWorker);
	aTbl.AddParam		(szParamWorkerID, aRec.f_ChildWorkerID);
	aTbl.AddFilterColumn(aRec.f_IsWorker);
	aTbl.AddFilterColumn(aRec.f_ChildWorkerID);
	aTbl.SetParamValue	(szParamIsWorker, DataBool(TRUE));
	aTbl.SetParamValue	(szParamWorkerID, m_DeletedWorkerID);

	TRY
	{
		aTbl.Query();
		
		while (!aTbl.IsEOF())
		{
			if (!bMessageDone)
			{
				if (Message(_TB("Do you want to update all the resources to which this Worker was assigned?"), MB_YESNO | MB_ICONQUESTION) == IDNO)
				{
					aTbl.Close();
					return TRUE;
				}
				bMessageDone = TRUE;
			}
			aTbl.LockCurrent();
			aTbl.Delete();
			aTbl.MoveNext();
		}
		aTbl.UnlockAll();
		aTbl.Close();
	}
	CATCH(SqlException, e)	
	{
		if (aTbl.IsOpen()) aTbl.Close();
		e->ShowError();
		return FALSE;
	}
	END_CATCH

	// vado a eliminare il riferimento del worker che sto eliminando se figlio di altri workers
	TWorkersDetails aRecWD;
	SqlTable aTblWD(&aRecWD, GetUpdatableSqlSession());

	aTblWD.Open(TRUE);
	aTblWD.SelectAll();

	aTblWD.AddParam			(szParamIsWorker, aRecWD.f_IsWorker);
	aTblWD.AddParam			(szParamWorkerID, aRecWD.f_WorkerID);
	aTblWD.AddFilterColumn	(aRecWD.f_IsWorker);
	aTblWD.AddFilterColumn	(aRecWD.f_WorkerID);
	aTblWD.SetParamValue	(szParamIsWorker, DataBool(TRUE));
	aTblWD.SetParamValue	(szParamWorkerID, m_DeletedWorkerID);

	TRY
	{
		aTblWD.Query();

		while (!aTblWD.IsEOF())
		{
			if (!bMessageDone)
			{
				if (Message(_TB("Do you want to update all workers to which this Worker was assigned?"), MB_YESNO | MB_ICONQUESTION) == IDNO)
				{
					aTblWD.Close();
					return TRUE;
				}
				bMessageDone = TRUE;
			}
			aTblWD.LockCurrent();
			aTblWD.Delete();
			aTblWD.MoveNext();
		}
		aTblWD.UnlockAll();
		aTblWD.Close();
	}
	CATCH(SqlException, e)
	{
		if (aTblWD.IsOpen()) aTblWD.Close();
		e->ShowError();
		return FALSE;
	}
	END_CATCH

	return TRUE;	
}

////-----------------------------------------------------------------------------
void DWorkers::OnManagerChanged()
{
	m_pDBTWorkersAbsences->GetCurrentRow()->l_ManagerDes = GetDocument()->GetHotLink<HKLWorkers>(L"Workers")->GetNameComplete();


}

//-----------------------------------------------------------------------------
void DWorkers::OnIsDisabledChanged()
{
	if (GetWorkers()->f_Disabled)
		GetWorkers()->f_CompanyLogin.Clear();

	GetWorkers()->f_CompanyLogin.SetReadOnly(GetWorkers()->f_Disabled);

	UpdateDataView();

	TResources aResRec;
	SqlTable aTable(&aResRec, GetReadOnlySqlSession());

	TAbsenceReasons aAbsRec;
	SqlTable aAbsTable(&aAbsRec, GetReadOnlySqlSession());

	TRY
	{
		aTable.SelectAll();
		aTable.Open();
		aTable.Query();
		BOOL bLock;
		bLock = aTable.LockCurrent();
		bLock = aTable.LockCurrent();
		bLock = aTable.IsCurrentLocked();
		bLock = aTable.UnlockCurrent();
		while (!aTable.IsEOF())
		{
			bLock = aTable.LockCurrent();
			aTable.MoveNext();
		}
		bLock = aTable.LockTable(_T("DummyTable"));
		bLock = aTable.UnlockTable(_T("DummyTable"));

		aAbsTable.SelectAll();
		aAbsTable.Open();
		aAbsTable.Query();
		while (!aAbsTable.IsEOF())
		{
			bLock = aAbsTable.LockCurrent();
			aAbsTable.MoveNext();
		}
		bLock = aAbsTable.m_pContext->GetLockMng(GetReadOnlySqlSession()->GetSqlConnection()->GetDatabaseName())->IsMyLock(&aAbsTable);
		bLock = aAbsTable.m_pContext->GetLockMng(GetReadOnlySqlSession()->GetSqlConnection()->GetDatabaseName())->IsCurrentLocked(&aTable);

		bLock = aAbsTable.UnlockAll();
		bLock = aTable.UnlockAll();
		aAbsTable.Close();
		aTable.Close();

	}
	CATCH(SqlException, e)
	{
		if (aAbsTable.IsOpen())
			aAbsTable.Close();
		
		if (aTable.IsOpen())
			aTable.Close();
	}
	END_CATCH
}

//-----------------------------------------------------------------------------
void DWorkers::OnPINChanged()
{
	m_pTRWorkersByPIN->ExcludeWorkerID(GetWorkers()->f_WorkerID);
	m_pTRWorkersByPIN->SetForceQuery();
	if (m_pTRWorkersByPIN->FindRecord(GetWorkers()->f_PIN) == TableReader::FOUND)
		SetError(_TB("A worker with the same PIN already exists"));

	if (GetWorkers()->f_PIN.Str().GetLength() < 8)
		SetError(_TB("The PIN length must be at least of 8 digits"));
}

//-----------------------------------------------------------------------------
void DWorkers::OnAbsencesFromDateChanged()
{
	TWorkersAbsences* pRec = m_pDBTWorkersAbsences->GetCurrentRow();
	pRec->f_EndingDate = DataDate(pRec->f_StartingDate.Day(), pRec->f_StartingDate.Month(), pRec->f_StartingDate.Year(), 23, 59, 00);

	DoCheckAbsencesTime();
	UpdateDataView();
}

//-----------------------------------------------------------------------------
void DWorkers::OnAbsencesToDateChanged()
{
	DoCheckAbsencesTime();
	UpdateDataView();
}

//-----------------------------------------------------------------------------
BOOL DWorkers::DoCheckAbsencesTime()
{
	BOOL bOk = TRUE;

	TWorkersAbsences* pRec = m_pDBTWorkersAbsences->GetCurrentRow();

	if (pRec->f_StartingDate > pRec->f_EndingDate)
	{
		bOk = FALSE;
		SetError(_TB("Starting date cannot follow ending date"));
	}
	return bOk;
}  

//-----------------------------------------------------------------------------
void DWorkers::OnArrangementFromDateChanged()
{
	TWorkersArrangement* pRec = m_pDBTWorkersArrangement->GetCurrentRow();  
	pRec->f_ToDate = pRec->f_FromDate;

	DoCheckArrangementTime();
	UpdateDataView();
}

//-----------------------------------------------------------------------------
void DWorkers::OnArrangementToDateChanged()
{
	DoCheckArrangementTime();
	UpdateDataView();
}

//-----------------------------------------------------------------------------
void DWorkers::OnArrangementChanged()
{
	TWorkersArrangement* pRec = m_pDBTWorkersArrangement->GetCurrentRow();  
	if (!pRec) return;
	 
	pRec->f_ArrangementLevel	= GetDocument()->GetHotLink<HKLArrangements>(L"Arrangement")->GetRecord()->f_ArrangementLevel;// m_pHKLArrangements->GetRecord()->f_ArrangementLevel;
	pRec->f_BasicPay			= GetDocument()->GetHotLink<HKLArrangements>(L"Arrangement")->GetRecord()->f_BasicPay;//m_pHKLArrangements->GetRecord()->f_BasicPay;
	pRec->f_TotalPay			= GetDocument()->GetHotLink<HKLArrangements>(L"Arrangement")->GetRecord()->f_TotalPay;//m_pHKLArrangements->GetRecord()->f_TotalPay;

	if (pRec->f_FromDate.IsEmpty())
		pRec->f_FromDate =  AfxGetApplicationDate();
	
	UpdateDataView();
}

//-----------------------------------------------------------------------------
BOOL DWorkers::DoCheckArrangementTime()
{
	BOOL bOk = TRUE;

	TWorkersArrangement* pRec = m_pDBTWorkersArrangement->GetCurrentRow();  

	if	(pRec->f_FromDate > pRec->f_ToDate)
	{
		bOk = FALSE;
		SetError(_TB("Starting date cannot follow ending date"));
	}
	return bOk;
}  

//-----------------------------------------------------------------------------
void DWorkers::SetWorker(DataLng aWorkerID)
{
	GetWorkers()->f_WorkerID = aWorkerID;
	BrowseRecord	();
	UpdateDataView	();
}

//-----------------------------------------------------------------------------
void DWorkers::SetParentWorkerID(DataLng aWorkerID)
{
	m_ParentWorkerID = aWorkerID;

	// mi tengo da parte il nome completo del padre (utile per visualizzare il msg all'utente)
	if (m_pTRWorkers->FindRecord(aWorkerID) == TableReader::FOUND)
		m_ParentWorkerNameComplete = m_pTRWorkers->GetWorker();

	UpdateDataView();
}

//-----------------------------------------------------------------------------
void DWorkers::SetParentResource(DataStr aResourceType, DataStr aResource)
{
	m_ParentResourceType = aResourceType;
	m_ParentResource	 = aResource;
	UpdateDataView();
}

//-----------------------------------------------------------------------------
void DWorkers::OnResourceTypeChanged() 
{
	TWorkersDetails* pRec = m_pDBTWorkersDetails->GetCurrentRow();
	if (!pRec) return;

	DataStr& OldResourceType = (DataStr&)AfxGetBaseApp()->GetOldCtrlData();

	if (AfxGetBaseApp()->IsValidOldCtrlData() && OldResourceType == pRec->f_ChildResourceType)
		return;
	else
	{
		pRec->f_ChildResourceCode.Clear();
		pRec->f_ChildWorkerID.Clear();
		pRec->l_ManagerDesc.Clear();
	}

	DoResourceTypeChanged();
	UpdateDataView();
}

//-----------------------------------------------------------------------------
void DWorkers::DoResourceTypeChanged() 
{
	TWorkersDetails* pRec = m_pDBTWorkersDetails->GetCurrentRow();
	if (!pRec) return;
	
	GetHotLink<HKLResources>(L"ResourcesCode")->SetCodeType(pRec->f_ChildResourceType);

	if (!pRec->f_IsWorker)
		pRec->l_ManagerDesc =  GetDocument()->GetHotLink<HKLResources>(L"ResourcesCode")->GetRecord()->f_Description;

	pRec->f_ChildWorkerID.SetReadOnly(!pRec->f_IsWorker);
	pRec->f_ChildResourceCode.SetReadOnly(pRec->f_IsWorker);
}

//-----------------------------------------------------------------------------
void DWorkers::OnResourceCodeChanged() 
{
	TWorkersDetails* pRec = m_pDBTWorkersDetails->GetCurrentRow();
	pRec->l_WorkerDesc = GetDocument()->GetHotLink<HKLResources>(L"ResourcesCode")->GetRecord()->f_Description;
	GetDocument()->GetHotLink<HKLWorkers>(L"Workers")->FindRecord(&GetDocument()->GetHotLink<HKLResources>(L"ResourcesCode")->GetRecord()->f_Manager);

	pRec->l_ManagerDesc = GetDocument()->GetHotLink<HKLWorkers>(L"Workers")->GetNameComplete();
	UpdateDataView();
}

//-----------------------------------------------------------------------------
void DWorkers::OnWorkerChanged() 
{
	TWorkersDetails* pRec = m_pDBTWorkersDetails->GetCurrentRow(); 
	pRec->l_WorkerDesc = GetHotLink<HKLWorkers>(L"Workers")->GetNameComplete();
	UpdateDataView();
}

//---------------------------------------------------------------------------------------
void DWorkers::OnIsWorkerChanged() 
{
	TWorkersDetails* pRec = m_pDBTWorkersDetails->GetCurrentRow();
	if (!pRec) return;

	pRec->l_WorkerDesc.Clear();
	pRec->l_ManagerDesc.Clear();

	if (pRec->f_IsWorker)
	{
		pRec->f_ChildResourceCode.Clear();
		pRec->f_ChildResourceType.Clear();
	}
	else
		pRec->f_ChildWorkerID.Clear();

	pRec->f_ChildResourceType.SetReadOnly(pRec->f_IsWorker);
	pRec->f_ChildResourceCode.SetReadOnly(pRec->f_IsWorker);
	pRec->f_ChildWorkerID.SetReadOnly(!pRec->f_IsWorker);

	UpdateDataView();
}

//-----------------------------------------------------------------------------
BOOL DWorkers::CheckRecursion()
{
	BOOL bOK = TRUE;
	BOOL bIsRecursive = FALSE;

	TWorkersDetails*	pRec = NULL;
	CheckResourcesRecursion	aCheckRecursion(this);

	// controllo che  la risorsa inserita nel dbt di testa non sia inserita anche nelle righe
	if (!OkRecurs())
	{
		//m_pStrArray->RemoveAll();
		return FALSE;
	}

	for (int i = 0; i <= m_pDBTWorkersDetails->GetUpperBound(); i++)
	{
		pRec = m_pDBTWorkersDetails->GetWorkersDetails(i);

		// controllo se tale risorsa e' già censita ne controllo la ricorsività
		BOOL bExist = TRUE;

		if (pRec->f_IsWorker && ExistsWorker(pRec->f_ChildWorkerID))
			bIsRecursive = aCheckRecursion.IsRecursive(GetWorkers()->f_WorkerID.Str(), _T(""), pRec->f_ChildWorkerID.Str(), _T(""));
		else
		{
			if (ExistsResource(pRec->f_ChildResourceCode, pRec->f_ChildResourceType))
				bIsRecursive = aCheckRecursion.IsRecursive(GetWorkers()->f_WorkerID.Str(), _T(""), pRec->f_ChildResourceCode, pRec->f_ChildResourceType);
		}

		if (bIsRecursive)
		{
			bOK = FALSE;
			break;
		}
	}

	return bOK;
}

//-----------------------------------------------------------------------------
BOOL DWorkers::ExistsWorker(DataLng& aWorker)
{
	return (m_pTRWorkers->FindRecord(aWorker) == TableReader::FOUND);
}

//-----------------------------------------------------------------------------
BOOL DWorkers::ExistsResource(const DataStr& aResourceCode, const DataStr& aResourceType)
{
	return (m_pTRResources->FindRecord(aResourceCode, aResourceType) == TableReader::FOUND);
}

//-----------------------------------------------------------------------------
BOOL DWorkers::OkRecurs()
{
	TWorkersDetails* pRec = NULL;
	for (int i = 0; i <= m_pDBTWorkersDetails->GetUpperBound(); i++)
	{
		pRec = m_pDBTWorkersDetails->GetWorkersDetails(i);

		if (pRec->f_IsWorker &&
			GetWorkers()->f_WorkerID == pRec->f_ChildWorkerID)
		{
			if (!IsInUnattendedMode())
				Message(cwsprintf(_TB("Unable to save. Recursiveness found")));
			return FALSE;
		}
	}

	return TRUE;
}

// scorro tutte le tabelle dichiarate come master e per ognuna vado a controllare
// se e' presente almeno una riga creata o modificata dal worker corrente
//-----------------------------------------------------------------------------
DeleteWorkerResult DWorkers::CheckIfWorkerIsUsed()
{
	POSITION			pos;
	CString				dummy(_T(""));
	SqlCatalogEntry*	pCatalogEntry;
	DeleteWorkerResult	aResult = NOT_USED;

	SqlCatalogConstPtr pCatalog = GetDocument()->m_pSqlConnection->GetCatalog();

	for (pos = pCatalog->GetStartPosition(); pos != NULL;)
	{
		pCatalog->GetNextAssoc(pos, dummy, (CObject*&)pCatalogEntry);

		if (pCatalogEntry && pCatalogEntry->IsMasterTable())  // It's a 'master' table
		{
			aResult = CheckMasterTableForWorker(pCatalogEntry->m_strTableName);
			if (aResult == USED || aResult == ERROR_OCCURRED)
				return aResult;
		}
	}

	return NOT_USED;
}

//-----------------------------------------------------------------------------
DeleteWorkerResult DWorkers::CheckMasterTableForWorker(CString aTableName)
{
	SqlTable aTbl(GetDocument()->GetReadOnlySqlSession());

	aTbl.m_strSQL = cwsprintf(
		_T("SELECT TOP(1) %s FROM %s WHERE %s = %d OR %s = %d"),
		CREATED_ID_COL_NAME,
		aTableName,
		CREATED_ID_COL_NAME,
		(int)GetWorkers()->f_WorkerID,
		MODIFIED_ID_COL_NAME,
		(int)GetWorkers()->f_WorkerID
	);

	DeleteWorkerResult aResult = NOT_USED;

	TRY
	{
		aTbl.Open();
		aTbl.Query();
		aResult = aTbl.IsEmpty() ? NOT_USED : USED;
		aTbl.Close();
	}
	CATCH(SqlException, e)
	{
		if (aTbl.IsOpen()) aTbl.Close();

		if (aTbl.m_pSqlConnection->GetDBMSType() == DBMS_ORACLE && e->m_wNativeErrCode == 904) // TBCreatedID or TBModifiedID fields don't exist
			return NOT_USED;
		else if (aTbl.m_pSqlConnection->GetDBMSType() == DBMS_SQLSERVER && e->m_wNativeErrCode == 207) // TBCreatedID or TBModifiedID fields don't exist
			return NOT_USED;
		else
		{
			e->ShowError();
			TRACE(cwsprintf(_T("ERROR! Table %s, error %s"), aTableName, e->m_strError));
			return ERROR_OCCURRED;
		}
	}
	END_CATCH

	return aResult;
}
