
#include "stdafx.h"

#include <TbFrameworkImages\CommonImages.h>
#include <TbFrameworkImages\GeneralFunctions.h>

#include "RMFunctions.h"
#include "RMControls.h"

#include "DResources.h"  
#include "UIResources.h" 
#include "ModuleObjects\Resources\JsonForms\IDD_RESOURCES.hjson"

#ifdef _DEBUG
#undef THIS_FILE
static char THIS_FILE[] = __FILE__;
#endif

static TCHAR szParamResource			[] = _T("p1");
static TCHAR szParamResourceType		[] = _T("p2");
static TCHAR szParamChildResourceType	[] = _T("p3");
static TCHAR szParamChildResourceCode	[] = _T("p4");

//////////////////////////////////////////////////////////////////////////////
//             class DBTResources implementation
//////////////////////////////////////////////////////////////////////////////
//============================================================================
IMPLEMENT_DYNAMIC(DBTResources, DBTMaster)

//-----------------------------------------------------------------------------	
DBTResources::DBTResources
	(
		CRuntimeClass*		pClass, 
		CAbstractFormDoc*	pDocument
	)
	:
	DBTMaster (pClass, pDocument, _NS_DBT("Resources"))
{}

//-----------------------------------------------------------------------------
void DBTResources::OnDefineQuery ()
{   
	m_pTable->SelectAll			();
	m_pTable->AddParam			(szParamResourceType, GetResources()->f_ResourceType);
	m_pTable->AddFilterColumn	(GetResources()->f_ResourceType);
	m_pTable->AddParam			(szParamResource, GetResources()->f_ResourceCode);
	m_pTable->AddFilterColumn	(GetResources()->f_ResourceCode);

	m_pTable->AddSortColumn		(GetResources()->f_ResourceType);
	m_pTable->AddSortColumn		(GetResources()->f_ResourceCode);
}

//-----------------------------------------------------------------------------
void DBTResources::OnPrepareQuery ()
{   
	m_pTable->SetParamValue(szParamResourceType,	GetResources()->f_ResourceType);
	m_pTable->SetParamValue(szParamResource,		GetResources()->f_ResourceCode);
}

//-----------------------------------------------------------------------------
void DBTResources::OnDisableControlsForAddNew ()
{
	GetDocument()->UpdateDataView();
}

//-----------------------------------------------------------------------------
void DBTResources::OnDisableControlsForEdit ()
{
	TResources* pRec = GetResources();

	pRec->f_ResourceCode.		SetReadOnly();
	pRec->f_ResourceType.		SetReadOnly();
	pRec->f_DomicilyAddress.	SetReadOnly(!GetResources()->f_Branch.IsEmpty());
	pRec->f_DomicilyCity.		SetReadOnly(!GetResources()->f_Branch.IsEmpty());
	pRec->f_DomicilyCounty.		SetReadOnly(!GetResources()->f_Branch.IsEmpty());
	pRec->f_DomicilyZip.		SetReadOnly(!GetResources()->f_Branch.IsEmpty());
	
	pRec->f_Telephone1.			SetReadOnly(!GetResources()->f_Branch.IsEmpty());
	pRec->f_Telephone2.			SetReadOnly(!GetResources()->f_Branch.IsEmpty());
	pRec->f_Telephone3.			SetReadOnly(!GetResources()->f_Branch.IsEmpty());
	pRec->f_Telephone4.			SetReadOnly(!GetResources()->f_Branch.IsEmpty());

	pRec->f_URL.				SetReadOnly(!GetResources()->f_Branch.IsEmpty());
	pRec->f_Email.				SetReadOnly(!GetResources()->f_Branch.IsEmpty());

	GetDocument()->UpdateDataView();
}

//-----------------------------------------------------------------------------
BOOL DBTResources::OnCheckPrimaryKey()
{ 
	if (GetResources()->f_ResourceType.IsEmpty())
		return !GetDocument()->Message(_TB("Enter the Resource Type."), MB_ICONEXCLAMATION);
	
	return
		!GetResources()->f_ResourceCode.IsEmpty() ||
		!GetDocument()->Message(_TB("Enter the Resource code."), MB_ICONEXCLAMATION);   
}

///////////////////////////////////////////////////////////////////////////////
//             class DBTResourcesDetails implementation
///////////////////////////////////////////////////////////////////////////////
//=============================================================================
IMPLEMENT_DYNAMIC(DBTResourcesDetails, DBTSlaveBuffered)

//-----------------------------------------------------------------------------	
DBTResourcesDetails::DBTResourcesDetails
	(
		CRuntimeClass*		pClass, 
		CAbstractFormDoc*	pDocument
	)
	:
	DBTSlaveBuffered (pClass, pDocument, _NS_DBT("ResourcesDetails"), ALLOW_EMPTY_BODY, TRUE)
{
}

//-----------------------------------------------------------------------------	
void DBTResourcesDetails::OnDefineQuery ()
{
	m_pTable->SelectAll			();
	m_pTable->AddParam			(szParamResourceType, GetResourcesDetails()->f_ResourceType);
	m_pTable->AddFilterColumn	(GetResourcesDetails()->f_ResourceType);
	m_pTable->AddParam			(szParamResource, GetResourcesDetails()->f_ResourceCode);
	m_pTable->AddFilterColumn	(GetResourcesDetails()->f_ResourceCode);

	m_pTable->AddSortColumn		(GetResourcesDetails()->f_ResourceType);
	m_pTable->AddSortColumn		(GetResourcesDetails()->f_ResourceCode);
}

//-----------------------------------------------------------------------------
void DBTResourcesDetails::OnPrepareQuery ()
{
	m_pTable->SetParamValue(szParamResourceType,	GetResources()->f_ResourceType);
	m_pTable->SetParamValue(szParamResource,		GetResources()->f_ResourceCode);
}

//-----------------------------------------------------------------------------
void DBTResourcesDetails::OnPreparePrimaryKey(int nRow, SqlRecord* pSqlRec)
{
	ASSERT(pSqlRec->IsKindOf(RUNTIME_CLASS(TResourcesDetails)));

	TResourcesDetails* pRec = (TResourcesDetails*) pSqlRec;

	pRec->f_ResourceType = GetResources()->f_ResourceType;
	pRec->f_ResourceCode = GetResources()->f_ResourceCode;
	pRec->SetStorable(TRUE);
}

//-----------------------------------------------------------------------------
DataObj* DBTResourcesDetails::GetDuplicateKeyPos(SqlRecord* pRec)
{
	ASSERT (pRec->IsKindOf(RUNTIME_CLASS(TResourcesDetails)));
	return &((TResourcesDetails*)pRec)->f_ResourceCode;
}

//-----------------------------------------------------------------------------
DataObj* DBTResourcesDetails::OnCheckPrimaryKey(int nRow, SqlRecord* pSqlRec)
{
	ASSERT(pSqlRec->IsKindOf(RUNTIME_CLASS(TResourcesDetails)));
	TResourcesDetails* pRec = (TResourcesDetails*)pSqlRec;

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
void DBTResourcesDetails::OnPrepareAuxColumns(SqlRecord* pSqlRec)
{
	TResourcesDetails*  pRec = (TResourcesDetails*)pSqlRec;

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
		GetDocument()->m_TRWorkers.FindRecord(GetDocument()->GetHotLink<HKLResources>(L"ResourcesCode")->GetRecord()->f_Manager);
		pRec->l_ManagerDesc = GetDocument()->m_TRWorkers.GetNameComplete();
	}
}

//-----------------------------------------------------------------------------
DataObj* DBTResourcesDetails::OnCheckUserData(int nRow)
{
	TResourcesDetails* pRec = (TResourcesDetails*)GetResourcesDetails(nRow);
	pRec->SetStorable(pRec->f_ChildWorkerID > 0 || !pRec->f_ChildResourceCode.IsEmpty());

	return NULL;
}

//-----------------------------------------------------------------------------
void DBTResourcesDetails::OnDisableControlsForEdit()
{
	for (int i = 0; i <= GetUpperBound(); i++)
	{
		TResourcesDetails* pRec = GetResourcesDetails(i);
		if (pRec->f_IsWorker)
		{
			pRec->f_ChildResourceType.SetReadOnly();
			pRec->f_ChildResourceCode.SetReadOnly();
		}
		else
			pRec->f_ChildWorkerID.SetReadOnly();
	}
}

///////////////////////////////////////////////////////////////////////////////
//             class DBTResourcesFields implementation
///////////////////////////////////////////////////////////////////////////////
//=============================================================================
IMPLEMENT_DYNAMIC(DBTResourcesFields, DBTSlaveBuffered)

//-----------------------------------------------------------------------------	
DBTResourcesFields::DBTResourcesFields
	(
		CRuntimeClass*		pClass, 
		CAbstractFormDoc*	pDocument
	)
	:
	DBTSlaveBuffered (pClass, pDocument, _NS_DBT("ResourcesFields"), ALLOW_EMPTY_BODY, TRUE)
{
}

//-----------------------------------------------------------------------------	
void DBTResourcesFields::OnDefineQuery ()
{
	m_pTable->SelectAll			();

	m_pTable->AddParam			(szParamResourceType, GetResourcesFields()->f_ResourceType);
	m_pTable->AddFilterColumn	(GetResourcesFields()->f_ResourceType);
	m_pTable->AddParam			(szParamResource, GetResourcesFields()->f_ResourceCode);
	m_pTable->AddFilterColumn	(GetResourcesFields()->f_ResourceCode);

	m_pTable->AddSortColumn		(GetResourcesFields()->f_Line);
}

//-----------------------------------------------------------------------------
void DBTResourcesFields::OnPrepareQuery ()
{
	m_pTable->SetParamValue(szParamResourceType,	GetResources()->f_ResourceType);
	m_pTable->SetParamValue(szParamResource,		GetResources()->f_ResourceCode);
}

//-----------------------------------------------------------------------------
void DBTResourcesFields::OnPreparePrimaryKey(int nRow, SqlRecord* pSqlRec)
{
	ASSERT (pSqlRec->IsKindOf(RUNTIME_CLASS(TResourcesFields)));

	TResourcesFields* pRec = (TResourcesFields*) pSqlRec;
	pRec->f_ResourceType	= GetResources()->f_ResourceType;
	pRec->f_ResourceCode	= GetResources()->f_ResourceCode;
	pRec->f_Line			= nRow;
	pRec->SetStorable(TRUE);
}

//-----------------------------------------------------------------------------
DataObj* DBTResourcesFields::GetDuplicateKeyPos(SqlRecord* pRec)
{
	ASSERT (pRec->IsKindOf(RUNTIME_CLASS(TResourcesFields)));
	return &((TResourcesFields*)pRec)->f_Line;
}

//-----------------------------------------------------------------------------
void DBTResourcesFields::OnAfterInsertRow(int nRow, SqlRecord* pRec)
{
	DBTSlaveBuffered::OnAfterInsertRow(nRow, pRec);
	CheckRows(nRow);
};

//-----------------------------------------------------------------------------
void DBTResourcesFields::OnAfterAddRow(int nRow, SqlRecord* pRec)
{
	DBTSlaveBuffered::OnAfterAddRow(nRow, pRec);
	CheckRows(nRow);
}

//-----------------------------------------------------------------------------
void DBTResourcesFields::CheckRows(int nRow)
{
	int nStart = nRow + 1;
	TResourcesFields* pRec;

	for (int i = nStart; i <= GetUpperBound(); i++)
	{
		pRec = GetResourcesFields(i);
		pRec->f_Line = i + 1; 
		pRec->SetStorable(!pRec->f_FieldName.IsEmpty());
	}

	GetDocument()->UpdateDataView();
}

//-----------------------------------------------------------------------------	
DataObj* DBTResourcesFields::OnCheckUserData(int nRow)
{ 
	if (GetDocument()->IsInUnattendedMode())
		return NULL; 

   	TResourcesFields* pRec = GetResourcesFields(nRow);

	if (!pRec->f_FieldValue.IsEmpty() && pRec->f_FieldName.IsEmpty())
	{
		SetError(_TB("The field name is mandatory"));
		return &pRec->f_FieldName;
	}
	return NULL;
}

///////////////////////////////////////////////////////////////////////////////
//             class DBTResourcesAbsence implementation
///////////////////////////////////////////////////////////////////////////////
//=============================================================================
IMPLEMENT_DYNAMIC(DBTResourcesAbsences, DBTSlaveBuffered)

//-----------------------------------------------------------------------------	
DBTResourcesAbsences::DBTResourcesAbsences
	(
		CRuntimeClass*		pClass, 
		CAbstractFormDoc*	pDocument
	)
	:
	DBTSlaveBuffered (pClass, pDocument, _NS_DBT("ResourcesAbsences"), ALLOW_EMPTY_BODY, TRUE)
{
}

//-----------------------------------------------------------------------------	
void DBTResourcesAbsences::OnDefineQuery()
{
	m_pTable->SelectAll			();

	m_pTable->AddParam			(szParamResourceType, GetResourcesAbsences()->f_ResourceType);
	m_pTable->AddFilterColumn	(GetResourcesAbsences()->f_ResourceType);
	m_pTable->AddParam			(szParamResource, GetResourcesAbsences()->f_ResourceCode);
	m_pTable->AddFilterColumn	(GetResourcesAbsences()->f_ResourceCode);

	m_pTable->AddSortColumn		(GetResourcesAbsences()->f_Reason);
}

//-----------------------------------------------------------------------------
void DBTResourcesAbsences::OnPrepareQuery()
{
	m_pTable->SetParamValue(szParamResourceType, GetResources()->f_ResourceType);
	m_pTable->SetParamValue(szParamResource, GetResources()->f_ResourceCode);
}

//-----------------------------------------------------------------------------
void DBTResourcesAbsences::OnPreparePrimaryKey(int nRow, SqlRecord* pSqlRec)
{
	ASSERT(pSqlRec->IsKindOf(RUNTIME_CLASS(TResourcesAbsences)));
	TResourcesAbsences* pRec = (TResourcesAbsences*)pSqlRec;

	pRec->f_ResourceType = GetResources()->f_ResourceType;
	pRec->f_ResourceCode = GetResources()->f_ResourceCode;
	pRec->SetStorable(TRUE);
}

//-----------------------------------------------------------------------------
DataObj* DBTResourcesAbsences::GetDuplicateKeyPos(SqlRecord* pRec)
{
	ASSERT(pRec->IsKindOf(RUNTIME_CLASS(TResourcesAbsences)));
	return &((TResourcesAbsences*)pRec)->f_Reason;
}

//-----------------------------------------------------------------------------
DataObj* DBTResourcesAbsences::OnCheckPrimaryKey(int nRow, SqlRecord* pSqlRec)
{
	ASSERT (pSqlRec->IsKindOf(RUNTIME_CLASS(TResourcesAbsences)));
	TResourcesAbsences* pRec = (TResourcesAbsences*)pSqlRec;

	if	(pRec->f_Reason.IsEmpty() && !pRec->f_StartingDate.IsEmpty())
	{
		SetError(_TB("The Reason code cannot be empty"));
		return &(pRec->f_Reason);
    }
    
	if	(pRec->f_StartingDate.IsEmpty() && !pRec->f_Reason.IsEmpty())
	{
		SetError(_TB("The Starting Date cannot be empty"));
		return &(pRec->f_StartingDate);
    }

    return NULL;
}

//-----------------------------------------------------------------------------
DataObj* DBTResourcesAbsences::OnCheckUserData(int nRow)
{
	TResourcesAbsences* pRec = (TResourcesAbsences*)GetResourcesAbsences(nRow);
	pRec->SetStorable(!pRec->f_Reason.IsEmpty());
	return NULL;
}
//-----------------------------------------------------------------------------
void DBTResourcesAbsences::OnPrepareAuxColumns(SqlRecord* pSqlRec)
{
	TResourcesAbsences* pRec = (TResourcesAbsences*)pSqlRec;
	pRec->l_ManagerDesc = GetDocument()->GetHotLink<HKLWorkers>(L"WorkersBrkd")->GetNameComplete();
}

///////////////////////////////////////////////////////////////////////////////
//                         	DResources								
///////////////////////////////////////////////////////////////////////////////
//-----------------------------------------------------------------------------
IMPLEMENT_DYNCREATE(DResources, CAbstractFormDoc)

//-----------------------------------------------------------------------------
BEGIN_MESSAGE_MAP(DResources, CAbstractFormDoc)
	ON_CONTROL(BEN_ROW_CHANGED, IDC_RESOURCES_DETAILS_BODYEDIT, OnResourcesDetailChanged)

	ON_EN_VALUE_CHANGED(IDC_RESOURCES_TYPE, OnTypeChanged)
	ON_EN_VALUE_CHANGED(IDC_RSS_GEN_MANAGER, OnManagerChanged)
	ON_EN_VALUE_CHANGED(IDC_RSS_RESOUR_BE_IS_WORKER, OnIsWorkerChanged)
	ON_EN_VALUE_CHANGED(IDC_RSS_RESOUR_BE_TYPE,	OnResourceTypeChanged)
	ON_EN_VALUE_CHANGED(IDC_RSS_RESOUR_BE_CODE,	OnResourceCodeChanged)
	ON_EN_VALUE_CHANGED(IDC_RSS_RESOUR_BE_WORKER, OnWorkerChanged)
	ON_EN_VALUE_CHANGED(IDC_RSS_BREAKD_BE_MANAGER, OnBreakDManagerChanged)
END_MESSAGE_MAP()                                           

//-----------------------------------------------------------------------------
TResources* DResources::GetResources() const { return (TResources*)	m_pDBTResources->GetRecord(); }

//------------------------------------------------------------------------------ 
DResources::DResources()
	:
	m_pDBTResources			(NULL),
	m_pDBTResourcesDetails	(NULL),
	m_pDBTResourcesFields	(NULL),
	m_pDBTResourcesAbsences	(NULL),
	m_TRResources			(this),
	m_TRWorkers				(this),
	m_TRResourceTypes		(this),
	m_pWorkerCtrl			(NULL)
{
}

//-----------------------------------------------------------------------------
BOOL DResources::OnAttachData()
{              
	SetFormTitle(_TB("Resources"));

	m_pDBTResources			= new DBTResources			(RUNTIME_CLASS(TResources),			this);
	m_pDBTResourcesDetails	= new DBTResourcesDetails	(RUNTIME_CLASS(TResourcesDetails),	this);
	m_pDBTResourcesFields	= new DBTResourcesFields	(RUNTIME_CLASS(TResourcesFields),	this);
	m_pDBTResourcesAbsences	= new DBTResourcesAbsences	(RUNTIME_CLASS(TResourcesAbsences), this);

	m_pDBTResources->Attach(m_pDBTResourcesDetails);
	m_pDBTResources->Attach(m_pDBTResourcesFields);
	m_pDBTResources->Attach(m_pDBTResourcesAbsences);

	return Attach(m_pDBTResources);
}

//-----------------------------------------------------------------------------
void DResources::OnParsedControlCreated(CParsedCtrl* pCtrl)
{
	if (!pCtrl)
		return;

	UINT nIDC = pCtrl->GetCtrlID();

	if (nIDC == IDC_RSS_MASTER_PICTURE)
	{
		CPictureStatic* pPictureStatic = (CPictureStatic*)pCtrl;
		pPictureStatic->OnCtrlStyleBest();
		return;
	}

	if (nIDC == IDC_RSS_MASTER_PATH)
	{
		CNamespaceEdit* pNamespaceEdit = (CNamespaceEdit*)pCtrl;
		pNamespaceEdit->SetNamespaceType(CTBNamespace::IMAGE);
		pNamespaceEdit->SetNamespace(GetNamespace());
		CJsonTileDialog* pJsonTileDlg = dynamic_cast<CJsonTileDialog*>(pCtrl->GetCtrlParent());
		CPictureStatic* pPictureStatic = pJsonTileDlg ? dynamic_cast<CPictureStatic*>(pJsonTileDlg->GetDlgItem(IDC_RSS_MASTER_PICTURE)) : NULL;
		if (pPictureStatic)
			pNamespaceEdit->AttachPicture(pPictureStatic);
		return;
	}

	if (nIDC == IDC_RSS_MASTER_ADDRESS)
	{
		CAddressEdit* pEdit = (CAddressEdit*)pCtrl;
		pEdit->BindCity(&(GetResources()->f_DomicilyCity));
		pEdit->BindCounty(&(GetResources()->f_DomicilyCounty));
		pEdit->BindZip(&(GetResources()->f_DomicilyZip));
		pEdit->BindCountry(&(GetResources()->f_DomicilyCountry));
		pEdit->BindLatitude(&GetResources()->f_Latitude);
		pEdit->BindLongitude(&GetResources()->f_Longitude);
		return;
	}

	if (nIDC == IDC_RSS_GEN_MANAGER_DESC)
	{
		m_pWorkerCtrl = (CWorkerStatic*)pCtrl;
		m_pWorkerCtrl->SetWorker(&(GetResources()->f_Manager));
	}
}

//-----------------------------------------------------------------------------
BOOL DResources::OnInitAuxData()
{
	return TRUE;
}

//-----------------------------------------------------------------------------
void DResources::DeleteContents()
{
	return CAbstractFormDoc::DeleteContents();
}

//-----------------------------------------------------------------------------
BOOL DResources::CanDoNewRecord()
{
	return CAbstractFormDoc::CanDoNewRecord();
}

//-----------------------------------------------------------------------------
BOOL DResources::CanDoEditRecord()
{
	return CAbstractFormDoc::CanDoEditRecord();
}

//-----------------------------------------------------------------------------
BOOL DResources::CanDoDeleteRecord()
{
	return CAbstractFormDoc::CanDoDeleteRecord();
}

//-----------------------------------------------------------------------------
BOOL DResources::OnPrepareAuxData()    
{
	GetResources()->l_ManagerDes = GetDocument()->GetHotLink<HKLWorkers>(L"Workers")->GetNameComplete();
	SetDefaultImage();

	return	TRUE;
}
//-----------------------------------------------------------------------------
void DResources::DisableControlsForEdit()    
{
	DoResourceTypeChanged();
	CAbstractFormDoc::DisableControlsForEdit();
}

//-----------------------------------------------------------------------------
BOOL DResources::OnOkTransaction()
{
	if (GetResources()->f_ImagePath == AfxGetPathFinder()->GetFileNameFromNamespace(TBGlyph(szGlyphResourceBig), _T("")))
		GetResources()->f_ImagePath.Clear();

	BOOL bOK;
	bOK = CheckRecursion();

	if (!bOK)
		m_pMessages->Show(TRUE);

	return bOK && CAbstractFormDoc::OnOkTransaction();
}

//-----------------------------------------------------------------------------
BOOL DResources::OnOkDelete()
{
	m_DeletedResourceType = GetResources()->f_ResourceType;
	m_DeletedResourceCode = GetResources()->f_ResourceCode;
	return TRUE;
}

//-----------------------------------------------------------------------------
BOOL DResources::OnNewTransaction()
{
	BOOL bOK = TRUE;

	// arrivo dal layout e sto inserendo un nuovo figlio ad un padre di tipo risorsa
	if (
		!m_ParentResource.IsEmpty() &&
		Message(cwsprintf(_TB("Do you want to assign this {0-%s} {1-%s} to the {2-%s} {3-%s}?"), 
		GetResources()->f_ResourceType.GetString(),
		GetResources()->f_ResourceCode.GetString(),
		m_ParentResourceType.GetString(),
		m_ParentResource.GetString()), MB_YESNO | MB_ICONQUESTION) == IDYES
		)
	{
		TUResourcesDetails aTUResources(this);

		if (aTUResources.FindRecord(m_ParentResourceType, m_ParentResource, GetResources()->f_ResourceType, GetResources()->f_ResourceCode, TRUE) == TableUpdater::NOT_FOUND)
		{
			aTUResources.GetRecord()->f_ResourceType		= m_ParentResourceType;
			aTUResources.GetRecord()->f_ResourceCode		= m_ParentResource;
			aTUResources.GetRecord()->f_IsWorker			= FALSE;
			aTUResources.GetRecord()->f_ChildResourceType	= GetResources()->f_ResourceType;
			aTUResources.GetRecord()->f_ChildResourceCode	= GetResources()->f_ResourceCode;
			aTUResources.GetRecord()->f_ChildWorkerID		= DataLng(0);

			bOK = aTUResources.UpdateRecord();
			aTUResources.UnlockCurrent();
		}
	}

	// arrivo dal layout e sto inserendo un nuovo figlio ad un padre di tipo worker
	if (
		!m_ParentWorkerID.IsEmpty() &&
		Message(cwsprintf(_TB("Do you want to assign this {0-%s} to the {1-%s} {2-%s}?"), 
		m_ParentWorkerNameComplete.GetString(),
		m_ParentResourceType.GetString(), 
		m_ParentResource.GetString()), MB_YESNO | MB_ICONQUESTION) == IDYES
		)
	{
		TUWorkersDetails aTUWorkers(this);

		if (aTUWorkers.FindRecord(m_ParentWorkerID, GetResources()->f_ResourceType, GetResources()->f_ResourceCode, TRUE) == TableUpdater::NOT_FOUND)
		{
			aTUWorkers.GetRecord()->f_WorkerID			= m_ParentWorkerID;
			aTUWorkers.GetRecord()->f_IsWorker			= FALSE;
			aTUWorkers.GetRecord()->f_ChildResourceType = GetResources()->f_ResourceType;
			aTUWorkers.GetRecord()->f_ChildResourceCode = GetResources()->f_ResourceCode;
			aTUWorkers.GetRecord()->f_ChildWorkerID		= DataLng(0);

			bOK = aTUWorkers.UpdateRecord();
			aTUWorkers.UnlockCurrent();
		}
	}

	return bOK && CAbstractFormDoc::OnNewTransaction();
}

//-----------------------------------------------------------------------------
BOOL DResources::OnRunReport(CWoormInfo* pWoormInfo)
{
	if (!pWoormInfo)
		return TRUE;

	pWoormInfo->AddParam(_NS_WRMVAR("w_AskDialog"),			&DataBool(FALSE));
	pWoormInfo->AddParam(_NS_WRMVAR("w_UseResoucerFilter"),	&DataBool(TRUE));
	pWoormInfo->AddParam(_NS_WRMVAR("w_ResourceTypeFilter"),&GetResources()->f_ResourceType); 
	pWoormInfo->AddParam(_NS_WRMVAR("w_ResourceFilter"),	&GetResources()->f_ResourceCode); 
	
	return TRUE;
}

//-----------------------------------------------------------------------------
BOOL DResources::OnDeleteTransaction()
{
	BOOL bMessageDone = FALSE;

	// vado a eliminare il riferimento della risorsa che sto eliminando se figlia di altre risorse
	TResourcesDetails aRec;
	SqlTable aTbl(&aRec, GetUpdatableSqlSession());

	aTbl.Open(TRUE);
	aTbl.SelectAll();

	aTbl.AddParam		(szParamChildResourceType,	aRec.f_ChildResourceType);
	aTbl.AddParam		(szParamChildResourceCode,	aRec.f_ChildResourceCode);
	aTbl.AddFilterColumn(aRec.f_ChildResourceType);
	aTbl.AddFilterColumn(aRec.f_ChildResourceCode);
	aTbl.SetParamValue	(szParamChildResourceType,	m_DeletedResourceType);
	aTbl.SetParamValue	(szParamChildResourceCode,	m_DeletedResourceCode);

	TRY
	{
		aTbl.Query();
		
		while (!aTbl.IsEOF())
		{
			if (!bMessageDone)
			{
				if (Message(cwsprintf(_TB("Do you want to update all the resources to which this {0-%s} {1-%s} was assigned?"), 
					m_DeletedResourceType.GetString(), m_DeletedResourceCode.GetString()), 
					MB_YESNO | MB_ICONQUESTION) == IDNO)
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

	// vado a eliminare il riferimento della risorsa che sto eliminando se figlia di altri workers
	TWorkersDetails aRecWD;
	SqlTable aTblWD(&aRecWD, GetUpdatableSqlSession());

	aTblWD.Open(TRUE);
	aTblWD.SelectAll();

	aTblWD.AddParam			(szParamChildResourceType, aRecWD.f_ChildResourceType);
	aTblWD.AddParam			(szParamChildResourceCode, aRecWD.f_ChildResourceCode);
	aTblWD.AddFilterColumn	(aRecWD.f_ChildResourceType);
	aTblWD.AddFilterColumn	(aRecWD.f_ChildResourceCode);
	aTblWD.SetParamValue	(szParamChildResourceType, m_DeletedResourceType);
	aTblWD.SetParamValue	(szParamChildResourceCode, m_DeletedResourceCode);

	TRY
	{
		aTblWD.Query();

		while (!aTblWD.IsEOF())
		{
			if (!bMessageDone)
			{
				if (Message(cwsprintf(_TB("Do you want to update all the workers to which this {0-%s} {1-%s} was assigned?"), 
					m_DeletedResourceType.GetString(), m_DeletedResourceCode.GetString()), 
					MB_YESNO | MB_ICONQUESTION) == IDNO)
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

//-----------------------------------------------------------------------------
BOOL DResources::CheckRecursion()
{
	BOOL bOK = TRUE;
	BOOL bIsRecursive = FALSE;
	
	TResourcesDetails*	pRec = NULL;
	CheckResourcesRecursion	aCheckRecursion(this);

	// controllo che  la risorsa inserita nel dbt di testa non sia inserita anche nelle righe
	if (!OkRecurs())
	{
		//m_pStrArray->RemoveAll();
		return FALSE;
	}
		
	for (int i = 0; i <= m_pDBTResourcesDetails->GetUpperBound(); i++)
	{
		pRec = m_pDBTResourcesDetails->GetResourcesDetails(i);

		// controllo se tale risorsa e' già censita ne controllo la ricorsività
		if (ExistsResource(pRec->f_ChildResourceCode, pRec->f_ChildResourceType))
			bIsRecursive = aCheckRecursion.IsRecursive(GetResources()->f_ResourceCode, GetResources()->f_ResourceType, pRec->f_ChildResourceCode, pRec->f_ChildResourceType);
		
		if (bIsRecursive)
		{
			 bOK = FALSE;
			 break;
		}
	}

	return bOK;
}

//-----------------------------------------------------------------------------
BOOL DResources::ExistsResource(const DataStr& aResourceCode, const DataStr& aResourceType)
{
	return (m_TRResources.FindRecord(aResourceCode, aResourceType) == TableReader::FOUND);
}

//-----------------------------------------------------------------------------
BOOL DResources::OkRecurs()
{
	TResourcesDetails*	pRec = NULL;
	for (int i = 0; i <= m_pDBTResourcesDetails->GetUpperBound(); i++)
	{
		pRec = m_pDBTResourcesDetails->GetResourcesDetails(i);
		
		if	(GetResources()->f_ResourceCode == pRec->f_ChildResourceCode &&
			GetResources()->f_ResourceType == pRec->f_ChildResourceType ) 
		{
			if (!IsInUnattendedMode())
				Message(cwsprintf(_TB("Unable to save. Recursiveness found")));
			//m_pStrArray->RemoveAt(m_pStrArray->GetUpperBound());
			return FALSE;
		}
	}
	return TRUE;
}

//-----------------------------------------------------------------------------
void DResources::OnTypeChanged()
{
	if (GetResources()->f_ImagePath == AfxGetPathFinder()->GetFileNameFromNamespace(TBGlyph(szGlyphResourceBig), _T("")))
		GetResources()->f_ImagePath.Clear();

	SetDefaultImage();
	UpdateDataView();
}

//-----------------------------------------------------------------------------
void DResources::OnManagerChanged()
{
	GetDocument()->GetHotLink<HKLWorkers>(L"WorkersManager")->FindRecord(&GetResources()->f_Manager);
	GetResources()->l_ManagerDes = GetDocument()->GetHotLink<HKLWorkers>(L"WorkersManager")->GetNameComplete();

	UpdateDataView();
}

//-----------------------------------------------------------------------------
void DResources::OnResourcesDetailChanged()
{
	DoResourceTypeChanged();
	UpdateDataView();
}

//-----------------------------------------------------------------------------
void DResources::OnResourceTypeChanged()
{
	TResourcesDetails* pRec = m_pDBTResourcesDetails->GetCurrentRow();
	if (!pRec)
		return;

	DataStr& OldResourceType = (DataStr&)AfxGetBaseApp()->GetOldCtrlData();
	
	if (AfxGetBaseApp()->IsValidOldCtrlData() && OldResourceType == pRec->f_ChildResourceType)
		return;
	else
	{
		pRec->f_ChildResourceCode.Clear();
		pRec->f_ChildWorkerID.Clear();
		pRec->l_WorkerDesc.Clear();
		pRec->l_ManagerDesc.Clear();
	}
	
	DoResourceTypeChanged();
	UpdateDataView();
}

//-----------------------------------------------------------------------------
void DResources::DoResourceTypeChanged()
{
	TResourcesDetails* pRec = m_pDBTResourcesDetails->GetCurrentRow();

	if (!pRec)
		return;
	GetDocument()->GetHotLink<HKLResources>(L"ResourcesCode")->SetCodeType(pRec->f_ChildResourceType);

	if (!pRec->f_IsWorker)
		pRec->l_WorkerDesc = GetDocument()->GetHotLink<HKLResources>(L"ResourcesCode")->GetRecord()->f_Description;

	pRec->f_ChildWorkerID.SetReadOnly(!pRec->f_IsWorker);
	pRec->f_ChildResourceCode.SetReadOnly(pRec->f_IsWorker);
}

//-----------------------------------------------------------------------------
void DResources::OnResourceCodeChanged()
{
	TResourcesDetails* pRec = m_pDBTResourcesDetails->GetCurrentRow();
	
	pRec->l_WorkerDesc = GetDocument()->GetHotLink<HKLResources>(L"ResourcesCode")->GetRecord()->f_Description;
	m_TRWorkers.FindRecord(GetDocument()->GetHotLink<HKLResources>(L"ResourcesCode")->GetRecord()->f_Manager) ;
	pRec->l_ManagerDesc = m_TRWorkers.GetNameComplete();// GetDocument()->GetHotLink<HKLWorkers>(L"Workers")->GetNameComplete(); //m_pHKLWorkers->GetNameComplete();

	UpdateDataView();
}


//-----------------------------------------------------------------------------
void DResources::OnWorkerChanged()
{
	TResourcesDetails* pRec = m_pDBTResourcesDetails->GetCurrentRow();
	pRec->l_WorkerDesc = GetDocument()->GetHotLink<HKLWorkers>(L"Workers")->GetNameComplete();

	UpdateDataView();
}

//-----------------------------------------------------------------------------
void DResources::OnIsWorkerChanged()
{
	TResourcesDetails* pRec = m_pDBTResourcesDetails->GetCurrentRow();

	pRec->l_WorkerDesc.Clear();
	pRec->l_ManagerDesc.Clear();

	if (pRec->f_IsWorker)
	{
		pRec->f_ChildResourceCode.Clear();
		pRec->f_ChildResourceType.Clear();
	}
	else
		pRec->f_ChildWorkerID.Clear();
	
	pRec->f_ChildResourceCode.SetReadOnly(pRec->f_IsWorker);
	pRec->f_ChildResourceType.SetReadOnly(pRec->f_IsWorker);
	pRec->f_ChildWorkerID.SetReadOnly(!pRec->f_IsWorker);
}

//-----------------------------------------------------------------------------
void DResources::OnBreakDManagerChanged()
{
	TResourcesAbsences*  pRec = (TResourcesAbsences*)m_pDBTResourcesAbsences->GetCurrentRow();
	pRec->l_ManagerDesc = GetDocument()->GetHotLink<HKLWorkers>(L"WorkersBrkd")->GetNameComplete();
}

//-----------------------------------------------------------------------------
void DResources::SetResource(DataStr aResourceType, DataStr aResource)
{
	GetResources()->f_ResourceType = aResourceType;
	GetResources()->f_ResourceCode = aResource;
	BrowseRecord	();
	UpdateDataView	();
}

//-----------------------------------------------------------------------------
void DResources::SetParentResource(DataStr aResourceType, DataStr aResource)
{
	m_ParentResourceType	= aResourceType;
	m_ParentResource		= aResource;
	GetResources()->f_ResourceType = aResourceType;
	UpdateDataView();
}

//-----------------------------------------------------------------------------
void DResources::SetParentWorkerID(DataLng aWorkerID)
{
	m_ParentWorkerID = aWorkerID;

	// mi tengo da parte il nome completo del padre (utile per visualizzare il msg all'utente)
	TRWorkers aTRWorker;
	if (aTRWorker.FindRecord(aWorkerID) == TableReader::FOUND)
		m_ParentWorkerNameComplete = aTRWorker.GetWorker();

	UpdateDataView();
}

//-----------------------------------------------------------------------------
void DResources::SetDefaultImage()
{
	if (!GetResources()->f_ImagePath.IsEmpty())
		return;

	if
		(
		m_TRResourceTypes.FindRecord(GetResources()->f_ResourceType) == TableReader::FOUND &&
		!m_TRResourceTypes.GetRecord()->f_ImagePath.IsEmpty()
		)
		GetResources()->f_ImagePath = m_TRResourceTypes.GetRecord()->f_ImagePath;
	else
		GetResources()->f_ImagePath = AfxGetPathFinder()->GetFileNameFromNamespace(TBGlyph(szGlyphResourceBig), _T(""));
}
