
#include "stdafx.h"
#include <io.h>

#include <TBNameSolver\ApplicationContext.h>
#include <TBNameSolver\CompanyContext.h>
#include <TBNameSolver\Diagnostic.h>


#include "DatabaseObjectsInfo.h"
#include "TBStrings.h"

//includere come ultimo include all'inizio del cpp
#include "begincpp.dex"

#ifdef _DEBUG
#undef THIS_FILE
static const char THIS_FILE[] = __FILE__;
#endif


//------------------------------------------------------------------------------
const CAlterTableDescriptionArray* AFXAPI AfxGetAddOnFieldsTable()
{
	return AfxGetApplicationContext()->GetObject<const CAlterTableDescriptionArray>(&CApplicationContext::GetAddOnFieldsTable);
}

//------------------------------------------------------------------------------
const CAddColsTableDescription* AFXAPI AfxGetAddOnFieldsOnTable (const CTBNamespace& aTableNs)
{
	return (const CAddColsTableDescription*) AfxGetAddOnFieldsTable()->GetInfo(aTableNs);
}

//=============================================================================        
//					class CDbFieldDescription implementation
//=============================================================================        
IMPLEMENT_DYNAMIC(CDbFieldDescription, CDataObjDescription)

//----------------------------------------------------------------------------------------------
CDbFieldDescription::CDbFieldDescription(const CTBNamespace& ownerModule)
	:
	m_OwnerModule		(ownerModule),
	m_eColType			(CDbFieldDescription::Column),
	m_bIsAddOn			(FALSE),
	m_bIsSegmentKey		(FALSE),
	m_nCreationRelease	(1)
{
}

//----------------------------------------------------------------------------------------------
CDbFieldDescription::CDbFieldDescription(const CString& strName, DataObj* pValue, const CTBNamespace& ownerModule)
	:
	CDataObjDescription	(strName, pValue, FALSE),
	m_OwnerModule		(ownerModule),
	m_eColType			(CDbFieldDescription::Column),
	m_bIsAddOn			(FALSE),
	m_bIsSegmentKey		(FALSE), 
	m_nCreationRelease	(1)
{
}

//----------------------------------------------------------------------------------------------
CDbFieldDescription::CDbFieldDescription(const CDbFieldDescription* pDescri)
	:
	m_eColType			(CDbFieldDescription::Column),
	m_bIsAddOn			(FALSE),
	m_bIsSegmentKey		(FALSE), 
	m_nCreationRelease	(1)
{
	Assign(pDescri);
}


//----------------------------------------------------------------------------------------------
void CDbFieldDescription::Assign (const CDbFieldDescription* pDescri)
{
	CDataObjDescription::Assign (*pDescri);

	m_eColType		= pDescri->GetColType();
	m_bIsAddOn		= pDescri->IsAddOn();
	m_OwnerModule	= pDescri->m_OwnerModule;
	m_bIsSegmentKey = pDescri->m_bIsSegmentKey;
}

//=============================================================================        
//					class CDbObjectDescription implementation
//=============================================================================        
IMPLEMENT_DYNCREATE(CDbObjectDescription, CBaseDescription)

//----------------------------------------------------------------------------------------------
CDbObjectDescription::CDbObjectDescription (CTBNamespace::NSObjectType aNSType)
	:
	CBaseDescription	(aNSType),
	m_nCreationRelease	(0),
	m_DeclarationType	(None),
	m_bMasterTable		(FALSE)
{
}

//----------------------------------------------------------------------------------------------
const CString CDbObjectDescription::GetTitle () const
{
	return AfxBaseLoadDatabaseString (m_sNotLocalizedTitle, m_sName);
}


//----------------------------------------------------------------------------------------------
CDbFieldDescription* CDbObjectDescription::GetDynamicFieldByName (const CString& sName) const
{
	CDbFieldDescription* pDescri;
	for (int i=0; i <= m_arDynamicFields.GetUpperBound(); i++)
	{
		pDescri = (CDbFieldDescription*) m_arDynamicFields.GetAt(i);
		if (pDescri && _tcsicmp(pDescri->GetName(), sName) == 0)
			return pDescri;
	}

	return NULL;
}

//----------------------------------------------------------------------------------------------
void CDbObjectDescription::AddDynamicField(CDbFieldDescription* pField)
{
	m_arDynamicFields.Add(pField);
}
//----------------------------------------------------------------------------------------------
void CDbObjectDescription::RemoveDynamicField(int nIdx)
{
	m_arDynamicFields.RemoveAt(nIdx);
}

//----------------------------------------------------------------------------------------------
void CDbObjectDescription::RemoveAllDynamicFields()
{
	m_arDynamicFields.RemoveAll();
}

//----------------------------------------------------------------------------------------------
const int CDbObjectDescription::GetSqlRecType () const
{
	switch (GetType())
	{
		case::CTBNamespace::VIEW:
			return VIEW_TYPE;
		case::CTBNamespace::PROCEDURE:
			return PROC_TYPE;
		case::CTBNamespace::VIRTUAL_TABLE:
			return VIRTUAL_TYPE;
		default:
			return TABLE_TYPE;
	}
}

//----------------------------------------------------------------------------------------------
//	class CAlterTableDescription implementation
//----------------------------------------------------------------------------------------------
IMPLEMENT_DYNCREATE(CAlterTableDescription, CBaseDescription)

//----------------------------------------------------------------------------------------------
CAlterTableDescription::CAlterTableDescription ()
	:
	CBaseDescription	(CTBNamespace::LIBRARY),
	m_nCreationRelease	(0),
	m_nCreationStep		(0)
{
}


//----------------------------------------------------------------------------------------------
//	class CAddColsTableDescription implementation
//----------------------------------------------------------------------------------------------
IMPLEMENT_DYNCREATE(CAddColsTableDescription, CBaseDescription)

//----------------------------------------------------------------------------------------------
CAddColsTableDescription::CAddColsTableDescription ()
	:
	CBaseDescription(CTBNamespace::TABLE)
{
}

//=============================================================================        
//					class CAlterTableDescriptionArray implementation
//=============================================================================        

IMPLEMENT_DYNAMIC(CAlterTableDescriptionArray, CBaseDescriptionArray)
	
// in realtà mi tengo una sola descrizione per library anche se le AlterTable
// potrebbero essere più di una
//------------------------------------------------------------------------------
void CAlterTableDescriptionArray::AddAddOnFieldOnTable(CAddColsTableDescription* pNewDescri)
{
	// verifico se esiste già una dichiarazione sulla tabella server
	CAddColsTableDescription* pDescri = const_cast<CAddColsTableDescription*>(AfxGetAddOnFieldsOnTable (pNewDescri->GetNamespace()));

	// se esiste aggiungo solo la dichiarazione di provenienza delle
	// Library che non sono già state dichiarate precedentemente
	if (pDescri)
	{
		for (int i=0; i <= pNewDescri->m_arAlterTables.GetUpperBound(); i++)
		{
			CAlterTableDescription* pAlterTable = (CAlterTableDescription*) pNewDescri->m_arAlterTables.GetAt(i); 
			
			// esiste già una dichiarazione sulla stessa library
			if (!pAlterTable || pDescri->m_arAlterTables.GetInfo(pAlterTable->GetNamespace()))
				continue;

			CAlterTableDescription* pNewAlterTable = new CAlterTableDescription();
			*pNewAlterTable = *pAlterTable;
			pDescri->m_arAlterTables.Add(pNewAlterTable);
		}
		
		delete pNewDescri;
	}
	else
		this->Add(pNewDescri);
}

//=============================================================================        
//			DatabaseObjectsTable needed structures
//=============================================================================        
//------------------------------------------------------------------------------
//							General Functions
//-----------------------------------------------------------------------------
DatabaseObjectsTableConstPtr AFXAPI AfxGetDatabaseObjectsTable()
{ 
	CApplicationContext* pContext = AfxGetApplicationContext();
	DatabaseObjectsTable* pTable = NULL;
	
	if (pContext)
		pTable = pContext->GetObject<DatabaseObjectsTable>(&CApplicationContext::GetDatabaseObjectsTable);
	
	return DatabaseObjectsTableConstPtr(pTable, FALSE);
	
}          

//------------------------------------------------------------------------------
DatabaseObjectsTablePtr AFXAPI AfxGetWritableDatabaseObjectsTable () 
{
	CApplicationContext* pContext = AfxGetApplicationContext();
	if (pContext && !pContext->GetDatabaseObjectsTable())
		pContext->AttachDatabaseObjectsTable(new DatabaseObjectsTable());

	return DatabaseObjectsTablePtr(pContext->GetObject<DatabaseObjectsTable>(&CApplicationContext::GetDatabaseObjectsTable), TRUE);
}

//-----------------------------------------------------------------------------
DatabaseReleasesTableConstPtr AFXAPI AfxGetDatabaseReleasesTable()
{ 
	CApplicationContext* pContext = AfxGetApplicationContext();
	DatabaseReleasesTable* pTable = NULL;
	
	if (pContext)
		pTable = (DatabaseReleasesTable*) pContext->GetDeclaredDBReleasesTable();
	
	return DatabaseReleasesTableConstPtr(pTable, FALSE);
	
}          

//------------------------------------------------------------------------------
DatabaseReleasesTablePtr AFXAPI AfxGetWritableDatabaseReleasesTable () 
{
	CApplicationContext* pContext = AfxGetApplicationContext();
	if (pContext && !pContext->GetDeclaredDBReleasesTable())
		pContext->AttachDatabaseReleaesesTable(new DatabaseReleasesTable());

	return DatabaseReleasesTablePtr((DatabaseReleasesTable*) pContext->GetDeclaredDBReleasesTable(), TRUE);
}

//=============================================================================        
//					class CDbReleaseDescription implementation
//=============================================================================        

//------------------------------------------------------------------------------
IMPLEMENT_DYNAMIC(CDbReleaseDescription, CObject)

//------------------------------------------------------------------------------
CDbReleaseDescription::CDbReleaseDescription (const CString& sSignature, const int& nRelease)
	:
	m_sSignature(sSignature),
	m_nRelease  (nRelease)
{
}

//------------------------------------------------------------------------------
void CDbReleaseDescription::SetRelease (const int& nRelease)
{
	m_nRelease = nRelease;
}

//=============================================================================        
//					DatabaseReleasesTable
//=============================================================================        
//------------------------------------------------------------------------------
DatabaseReleasesTable::DatabaseReleasesTable()
{
	m_pReleases = new CMapStringToOb();
}

//------------------------------------------------------------------------------
DatabaseReleasesTable::~DatabaseReleasesTable()
{
	CDbReleaseDescription* pItem;
	CString strKey;
	POSITION pos;

	for (pos = m_pReleases->GetStartPosition(); pos != NULL;)
	{
		m_pReleases->GetNextAssoc(pos, strKey, (CObject*&)pItem);
		if (pItem)
		{
			m_pReleases->RemoveKey(strKey);
			delete pItem;
		}
	}
	delete m_pReleases;
}

//------------------------------------------------------------------------------
const CString DatabaseReleasesTable::GetSignatureOf (CString sKey) const
{
	TB_LOCK_FOR_READ();

	CDbReleaseDescription* pDescri = NULL;
	m_pReleases->Lookup(sKey.MakeLower(), (CObject*&) pDescri);
	
	return pDescri ? pDescri->GetSignature() : _T("");
}

//------------------------------------------------------------------------------
const int DatabaseReleasesTable::GetReleaseOf (CString sKey) const
{
	TB_LOCK_FOR_READ();

	CDbReleaseDescription* pDescri = NULL;
	m_pReleases->Lookup(sKey.MakeLower(), (CObject*&) pDescri);
	
	return pDescri ? pDescri->GetRelease() : 0;
}

//------------------------------------------------------------------------------
BOOL DatabaseReleasesTable::AddRelease(CString sKey, const CString& sSignature, const int& nRelease)
{
	TB_LOCK_FOR_WRITE();

	sKey = sKey.MakeLower();

	CDbReleaseDescription* pExistingDescri = NULL;
	if (m_pReleases->Lookup(sKey, (CObject*&)pExistingDescri) && pExistingDescri)
	{
		// if existing is release 0 i can substitute description
		if (pExistingDescri->GetRelease() == 0 && nRelease > 0)
		{
			pExistingDescri->SetRelease(nRelease);
			return TRUE;
		}

		// release 0 is ignored if a previous exists
		if (nRelease == 0 || pExistingDescri->GetRelease() == nRelease)
			return TRUE;

		CString sMessage;
		sMessage.Format
			(
				_TB("Duplicate database release definition found for %s! Declaration ignored!"), 
				sKey
			);
		AfxGetDiagnostic()->Add (sMessage, CDiagnostic::Error);
		return FALSE;
	}

	m_pReleases->SetAt(sKey, new CDbReleaseDescription(sSignature, nRelease));

	return TRUE;
}

//=============================================================================        
//					class DatabaseObjectsTable implementation
//=============================================================================        

//------------------------------------------------------------------------------
IMPLEMENT_DYNAMIC(DatabaseObjectsTable, CObject)

//------------------------------------------------------------------------------
DatabaseObjectsTable::DatabaseObjectsTable ()
{
}

//------------------------------------------------------------------------------
DatabaseObjectsTable::~DatabaseObjectsTable()
{
	CDbObjectDescription* pItem;
	CString strKey;
	POSITION pos;

	for (pos = m_DbObjects.GetStartPosition(); pos != NULL;)
	{
		m_DbObjects.GetNextAssoc(pos, strKey, (CObject*&)pItem);
		if (pItem)
		{
			m_DbObjects.RemoveKey(strKey);
			delete pItem;
		}
	}
}

//------------------------------------------------------------------------------
void DatabaseObjectsTable::ClearForRelease(int nRelease, const CTBNamespace& ownerModule, CStringArray& arRemovedTables, CStringArray& arRemovedFields)
{
	CDbObjectDescription* pItem;
	CString strKey;
	POSITION pos;

	for (pos = m_DbObjects.GetStartPosition(); pos != NULL;)
	{
		m_DbObjects.GetNextAssoc(pos, strKey, (CObject*&)pItem);
		if (pItem)
		{
			if (pItem->GetCreationRelease() == nRelease &&
				_tcsicmp(pItem->GetNamespace().GetApplicationName(), ownerModule.GetApplicationName()) == 0 &&
				_tcsicmp(pItem->GetNamespace().GetModuleName(), ownerModule.GetModuleName()) == 0)
			{
				
				arRemovedTables.Add(strKey);
				delete pItem;//distruggo l'oggetto, ma lo rimuovo dalla lista quando esco dal loop altrimenti mi invacca l'hashtable
				continue;
			}

			//parto dal fondo per non rovinare la lista
			for (int i = pItem->GetDynamicFields().GetUpperBound(); i >= 0; i--)
			{
				CDbFieldDescription* pField = (CDbFieldDescription*) pItem->GetDynamicFields().GetAt(i);
				if (pField->GetCreationRelease() == nRelease &&
					pField->GetOwnerModule() == ownerModule
					)
				{
					arRemovedFields.Add(strKey + _T('.') + pField->GetName());
					pItem->RemoveDynamicField(i);
				}
			}
		}
	}

	for (int i = 0; i < arRemovedTables.GetCount(); i++)
		m_DbObjects.RemoveKey(arRemovedTables[i]);
				
}

//------------------------------------------------------------------------------
int DatabaseObjectsTable::AddObject (CDbObjectDescription* pDescri)
{
	TB_LOCK_FOR_WRITE();

	CString sKey = pDescri->GetName();
	sKey = sKey.MakeLower();

	CDbObjectDescription* pExistingDescri = NULL;
	if (m_DbObjects.Lookup(sKey, (CObject*&)pExistingDescri) && pExistingDescri)
		return Merge (pExistingDescri, pDescri);

	m_DbObjects.SetAt(sKey, pDescri);
	return 1;
}

//------------------------------------------------------------------------------
int DatabaseObjectsTable::Merge	(
										CDbObjectDescription*	pExistingDescri, 
										CDbObjectDescription*	pNewDescri
									)
{
	if (pExistingDescri->GetType () != pNewDescri->GetType())
	{
		CString sMessage;
		sMessage.Format
			(
				_TB("Description of the database object {0-%s} declared into {1-%s} has been found with wrong namespace type {2-%s} into {3-%s}. Original was declared as {4-%s}. {5-%s} type is ignored!"), 
					pExistingDescri->GetName(), 
					pExistingDescri->GetOwner().ToString (), 
					pNewDescri->GetNamespace().GetTypeString(), 
					pNewDescri->GetOwner().ToString(),
					pExistingDescri->GetNamespace().GetTypeString (), 
					pNewDescri->GetNamespace().GetTypeString()
			);
		AfxGetDiagnostic()->Add (sMessage, CDiagnostic::Warning);
	}

	// se la descrizione è None allora assegno il valore della nuova descrizione da merge-are
	// se sono uguali vuol dire che è già settata del valore corretto, altrimenti diventa dinamico e codificato
	if (pExistingDescri->GetDeclarationType() == CDbObjectDescription::None)
		pExistingDescri->SetDeclarationType(pNewDescri->GetDeclarationType());
	else if (pExistingDescri->GetDeclarationType() != pNewDescri->GetDeclarationType())
		pExistingDescri->SetDeclarationType((CDbObjectDescription::DeclarationType) (CDbObjectDescription::Coded | CDbObjectDescription::Dynamic));


	BOOL bOneMerged = FALSE;

	CDbFieldDescription* pField;
	for (int i=0; i <= pNewDescri->GetDynamicFields().GetUpperBound(); i++)
	{
		pField = (CDbFieldDescription*) pNewDescri->GetDynamicFields().GetAt(i);
		if (pField && MergeField (pExistingDescri, pNewDescri, pField))
			bOneMerged = TRUE;
	}

	return bOneMerged ? 0 : - 1;
}

//------------------------------------------------------------------------------
BOOL DatabaseObjectsTable::MergeField	(
											CDbObjectDescription*		pExistingDescri, 
											const CDbObjectDescription*	pNewDescri,
											const CDbFieldDescription*			pField
										)
{
	// check for duplicates: user cannot modify and customize an existing field.
	// Only new fields declarations are allowed
	CTBNamespace aFieldOwner;
	if (pField->GetOwner().IsEmpty())
		aFieldOwner = pNewDescri->GetOwner();
	else
		aFieldOwner = pField->GetOwner();

	CDbFieldDescription* pExistingField = pExistingDescri->GetDynamicFieldByName(pField->GetName());
	if (pExistingField)
	{
		CString sMessage;
		sMessage.Format
			(
					_TB("Field %s in %s is already declared in %s. Duplicate declaration found in %s ignored!"), 
					pField->GetName(), 
					pNewDescri->GetName(),
					pExistingField->GetOwner().ToString(),
					aFieldOwner.ToString()
			);
		AfxGetDiagnostic()->Add (sMessage, CDiagnostic::Warning);
		return FALSE;
	}

	// as merging with a declaration of another namespace, 
	// field owner is always specified.
	CDbFieldDescription* pNewField = new CDbFieldDescription(pField);
	pNewField->SetOwner (aFieldOwner);

	// User Visibility is reported as it is declared.

	pExistingDescri->AddDynamicField (pNewField);
	return TRUE;
}

//------------------------------------------------------------------------------
CDbObjectDescription* DatabaseObjectsTable::GetDescription (CString sTableName) const
{
	TB_LOCK_FOR_READ();

	CDbObjectDescription* pDescri = NULL;
	m_DbObjects.Lookup(sTableName.MakeLower(), (CObject*&) pDescri);
	
	return pDescri;
}

//------------------------------------------------------------------------------
BOOL DatabaseObjectsTable::ModuleHasObjects(const CTBNamespace& nsModule) const
{
	TB_LOCK_FOR_READ();

	CDbObjectDescription* pItem;
	CString strKey;
	POSITION			pos;

	for (pos = m_DbObjects.GetStartPosition(); pos != NULL;)
	{
		m_DbObjects.GetNextAssoc(pos, strKey, (CObject*&)pItem);
		if (pItem && pItem->GetOwner() == nsModule)
			return TRUE;
	}

	return FALSE;
}

