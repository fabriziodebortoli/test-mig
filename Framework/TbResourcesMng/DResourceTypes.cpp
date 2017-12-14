
#include "stdafx.h"

#include <TbGes\JsonFormEngineEx.h>

#include "DResourceTypes.h"  

#include "ModuleObjects\ResourceTypes\JsonForms\IDD_RESOURCETYPES.hjson"

#ifdef _DEBUG
#undef THIS_FILE
static char THIS_FILE[] = __FILE__;
#endif

// Parametri per le query                  
static TCHAR szP1[] = _T("p1");

//////////////////////////////////////////////////////////////////////////////
//          class DBTResourceType implementation
//////////////////////////////////////////////////////////////////////////////
//-----------------------------------------------------------------------------	
IMPLEMENT_DYNAMIC(DBTResourceType, DBTMaster)

//-----------------------------------------------------------------------------	
DBTResourceType::DBTResourceType
	(
		CRuntimeClass*		pClass, 
		CAbstractFormDoc*	pDocument
	)
	:
	DBTMaster (pClass, pDocument, _NS_DBT("ResourceTypes"))
{}

//-----------------------------------------------------------------------------
void DBTResourceType::OnEnableControlsForFind()
{
	GetResourceTypes()->f_ResourceType.SetFindable();
	GetResourceTypes()->f_Description.SetFindable();
}

//-----------------------------------------------------------------------------
void DBTResourceType::OnDisableControlsForEdit()
{
	GetResourceTypes()->f_ResourceType.SetReadOnly();
}

//-----------------------------------------------------------------------------	
void DBTResourceType::OnPrepareBrowser(SqlTable* pTable)
{
	TResourceTypes* pRec = (TResourceTypes*)pTable->GetRecord();

	pTable->SelectAll();
	pTable->AddSortColumn(pRec->f_ResourceType);
}

// Serve a definire sia i criteri di sort (ORDER BY chiave primaria in questo caso)
// ed i criterio di filtraggio (WHERE)
//-----------------------------------------------------------------------------
void DBTResourceType::OnDefineQuery()
{
	const DataStr& aResType = GetResourceTypes()->f_ResourceType;
	
	m_pTable->SelectAll			();
	m_pTable->AddParam			(szP1, aResType);
	m_pTable->AddFilterColumn	(aResType);
}

// Serve a valorizzare i parametri di query
//-----------------------------------------------------------------------------
void DBTResourceType::OnPrepareQuery()
{
	m_pTable->SetParamValue(szP1, GetResourceTypes()->f_ResourceType);
}

// Forza il programmatore a controllare che i campi dell'indice primario 
// (PRIMARY INDEX) siano stati valorizzati correttamente onde non archiviare
// records non piu` rintracciabili
//-----------------------------------------------------------------------------
BOOL DBTResourceType::OnCheckPrimaryKey()
{
	return !GetResourceTypes()->f_ResourceType.IsEmpty() || !SetError(_TB("The Resource Type is mandatory"));
}

//////////////////////////////////////////////////////////////////////////////
//                      class DResourceTypes implementation
//////////////////////////////////////////////////////////////////////////////
//-----------------------------------------------------------------------------
IMPLEMENT_DYNCREATE(DResourceTypes, CAbstractFormDoc)

//-----------------------------------------------------------------------------
DResourceTypes::DResourceTypes()
	:
	m_pDBTResourceType(NULL)
{
}

//-----------------------------------------------------------------------------
BOOL DResourceTypes::OnAttachData()
{              
	SetFormTitle(_TB("Resource Types"));

	m_pDBTResourceType = new DBTResourceType(RUNTIME_CLASS(TResourceTypes), this);
	
	// initialize DBT objects and open related files
	return Attach(m_pDBTResourceType);
}

//-----------------------------------------------------------------------------
void DResourceTypes::OnParsedControlCreated(CParsedCtrl* pCtrl)
{
	if (!pCtrl)
		return;

	UINT nIDC = pCtrl->GetCtrlID();

	if (nIDC == IDC_RESOURCETYPES_IMAGE)
	{
		CPictureStatic* pPictureStatic = (CPictureStatic*)pCtrl;
		pPictureStatic->OnCtrlStyleBest();
		return;
	}

	if (nIDC == IDC_RESOURCETYPES_IMAGEPATH)
	{
		CNamespaceEdit* pNamespaceEdit = (CNamespaceEdit*)pCtrl;
		pNamespaceEdit->SetNamespaceType(CTBNamespace::IMAGE);
		pNamespaceEdit->SetNamespace(GetNamespace());
		CJsonTileDialog* pJsonTileDlg = dynamic_cast<CJsonTileDialog*>(pCtrl->GetCtrlParent());
		CPictureStatic* pPictureStatic = pJsonTileDlg ? dynamic_cast<CPictureStatic*>(pJsonTileDlg->GetDlgItem(IDC_RESOURCETYPES_IMAGE)) : NULL;
		if (pPictureStatic)
			pNamespaceEdit->AttachPicture(pPictureStatic);
		return;
	}
}