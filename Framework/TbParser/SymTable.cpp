
#include "stdafx.h"

#include <TbGeneric\DataObj.h>
#include <TbGeneric\DataObjDescription.h>
#include <TbGeneric\GeneralFunctions.h>

#include "parser.h"
#include "symtable.h"


#ifdef _DEBUG
#undef THIS_FILE
static const char BASED_CODE THIS_FILE[] = __FILE__;
#endif

////////////////////////////////////////////////////////////////////////////////
//									SymField
////////////////////////////////////////////////////////////////////////////////
//
IMPLEMENT_DYNAMIC(SymField, CObject)

SymField::SymField 
				(
					const CString& strName, 
					DataType dt /*= DataType::Null*/, 
					WORD wId /*= SpecialReportField::NO_INTERNAL_ID*/, 
					DataObj* pValue /*= NULL*/, 
					BOOL bOwnData /*= TRUE*/
				)
	:
	BaseField(strName, dt, pValue, bOwnData),

	m_wId			(wId),
	m_pTable		(NULL),
	m_parMethods	(NULL),
	m_nRefCount		(0),
	m_nLeftRefCount (0),
	m_pProvider		(NULL),
	m_pCustomData	(NULL)
{ 
}

//------------------------------------------------------------------------------
SymField::SymField (const SymField& f)
	:
	BaseField		(f),

	m_wId			(f.m_wId),
	m_pTable		(f.m_pTable),
	m_parMethods	(NULL/*f.m_parMethods*/),	//TODO
	m_nRefCount		(f.m_nRefCount),
	m_nLeftRefCount (f.m_nLeftRefCount),
	m_pProvider		(f.m_pProvider),
	m_pCustomData	(f.m_pCustomData)
{ 
}

//------------------------------------------------------------------------------
SymField::~SymField ()
{ 
	SAFE_DELETE(m_parMethods);
}

//------------------------------------------------------------------------------
void SymField::SetName (LPCTSTR pszName)
{
	if (m_pTable && m_pTable->m_pmapFieldsByName)
	{
		if (!m_strName.IsEmpty())
		{
			m_strName.MakeLower();
			m_pTable->m_pmapFieldsByName->RemoveKey(m_strName);
		}

		m_strName = pszName;

		if (!m_strName.IsEmpty())
		{
			CString sKey(pszName); sKey.MakeLower();
			m_pTable->m_pmapFieldsByName->SetAt(sKey, this);
		}
	}
	else
		m_strName = pszName;	
}	

void SymField::SetTag (LPCTSTR pszTag)
{ 
	if (m_pTable && m_pTable->m_pmapFieldsByTag)
	{
		if (!m_strTag.IsEmpty())
		{		
			m_strTag.MakeUpper();
			m_pTable->m_pmapFieldsByTag->RemoveKey(m_strTag);
		}

		m_strTag = pszTag;

		if (!m_strTag.IsEmpty())
		{
			CString sKey(pszTag); sKey.MakeUpper();
			m_pTable->m_pmapFieldsByTag->SetAt(sKey, this);
		}
	}
	else
		m_strTag = pszTag;	
}	

//------------------------------------------------------------------------------
DataObj* SymField::GetData (int /*nDataLevel = -1*/) const 
{ 
	if (m_pProvider)
	{
		SymField* pMe = const_cast<SymField*>(this);

		DataObj* pD = m_pProvider->GetData(m_strName);
		if (!pD)
			return NULL;
		ASSERT_VALID(pD);

		IDataProvider* pP = m_pProvider; pMe->m_pProvider = NULL;
		pMe->AssignData(*pD);
		pMe->m_pProvider = pP;
	}
	return m_pData; 
}		

//------------------------------------------------------------------------------
void SymField::AssignData (const DataObj& aData)
{
	__super::AssignData(aData);

	if (m_pProvider)
	{
		DataObj* pD = m_pProvider->GetData(m_strName);
		ASSERT_VALID(pD);
		pD->Assign(aData);
	}
}

//------------------------------------------------------------------------------
const DataObj* SymField::GetIndexedData (int nIdx) const
{ 
	ASSERT (m_pProvider);
	ASSERT (nIdx >= 0);

	DataObj* pD = m_pProvider->GetData(m_strName, nIdx);
	if (pD)
	{
		ASSERT_VALID(pD);
		SymField* pMe = const_cast<SymField*>(this);
		IDataProvider* pP = pMe->m_pProvider; pMe->m_pProvider = NULL;

		pMe->AssignIndexedData(nIdx, *pD);
		pMe->m_pProvider = pP;
	}
	else if (m_pData) m_pData->Clear();

	return m_pData; 
}		

//------------------------------------------------------------------------------
void SymField::AssignIndexedData (int nIdx, const DataObj& aData)
{
	ASSERT (m_pProvider);
	ASSERT (nIdx >= 0);

	if (m_pData == NULL)
	{
		m_DataType = aData.GetDataType();
		AllocData();
	}
	ASSERT (DataType::IsCompatible(aData.GetDataType(), m_DataType)); 
	
	m_pData->Assign(aData);

	if (m_pProvider)
	{
		DataObj* pD = m_pProvider->GetData(m_strName, nIdx);
		ASSERT_VALID(pD);
		if (pD) pD->Assign(aData);
	}
}

//------------------------------------------------------------------------------
void SymField::AddMethods (const CFunctionDescriptionArray& arMethods)
{
	if (!m_parMethods)
	{
		m_parMethods = new CFunctionDescriptionArray();
		m_parMethods->m_sClassName = arMethods.m_sClassName;
	}
	m_parMethods->Append(arMethods);
}

//------------------------------------------------------------------------------
BOOL SymField::AddMethods (const CString& sClassName, const CMapFunctionDescription* mapMethods)
{
	if (sClassName.IsEmpty())
		return FALSE;
	if (GetDataType() != DataType::Object && GetDataType() != DataType::Long)
	{
		ASSERT(FALSE);
		return FALSE;
	}

	CStringArray arClasses;
	CStringArray_Split(arClasses, sClassName, _T(","));
	for (int i = 0; i < arClasses.GetSize(); i++)
	{
		CString c = arClasses.GetAt(i); c.Trim();

		CFunctionDescriptionArray* parMethods = NULL;
		if (mapMethods->Lookup(c, (CObject*&)parMethods))
		{
			AddMethods(*parMethods);
		}
	}
	return m_parMethods && m_parMethods->GetSize();
}

//----------------------------------------------------------------------------
BOOL SymField::AddMethods (const CRuntimeClass* prtcStopBaseClassName, const CMapFunctionDescription* mapMethods)
{
	CObject* pHandle = (CObject*) (long) *(DataLng*)(GetData());
	if (!pHandle || !pHandle->IsKindOf(prtcStopBaseClassName))
		return FALSE;

	for (
			CRuntimeClass* prtc = pHandle->GetRuntimeClass();
			prtc;
			prtc = prtc->m_pfnGetBaseClass()
		)
	{
		CString sClassName(prtc->m_lpszClassName);

		AddMethods(sClassName, mapMethods);

		if (prtc == prtcStopBaseClassName)
			break;
	}

	return m_parMethods && m_parMethods->GetSize();
}

//------------------------------------------------------------------------------
CFunctionDescription* SymField::FindMethod(const CString& sName) const
{
	if (!m_parMethods)
		return NULL;

	CFunctionDescription* pF = m_parMethods->GetFunction(sName);
	if (pF)
		return pF;

	return NULL;
}

/////////////////////////////////////////////////////////////////////////////
// Diagnostics

#ifdef _DEBUG
void SymField::Dump(CDumpContext& dc) const
{
	ASSERT_VALID(this);
	AFX_DUMP1(dc, "\nSymField = ", GetName());
}

void SymField::AssertValid() const
{
	CObject::AssertValid();
}
#endif //_DEBUG

////////////////////////////////////////////////////////////////////////////////
//									SymTable
////////////////////////////////////////////////////////////////////////////////
//
//------------------------------------------------------------------------------
IMPLEMENT_DYNAMIC(SymTable, Array)

CString SymTable::s_SelfReference = _T("Environment");

//------------------------------------------------------------------------------
SymTable::SymTable()
	:
	m_pDocument				(NULL),
	m_pParentSymTable		(NULL),
	m_nDataLevel			(0),
	m_nCountSpecialFields	(0),
	m_pmapFieldsByName		(NULL),
	m_pmapFieldsByTag		(NULL),
	m_parFieldsModified		(NULL)
{
	AddSpecial ();
}

//------------------------------------------------------------------------------
SymTable::~SymTable()
{
	m_Handler.FireDisposing(this);

	SAFE_DELETE(m_pmapFieldsByName);
	SAFE_DELETE(m_pmapFieldsByTag);
}


//------------------------------------------------------------------------------
void SymTable::Clear ()
{
	m_nCountSpecialFields = 0;
	
	if (m_pmapFieldsByName)
		m_pmapFieldsByName->RemoveAll();

	if (m_pmapFieldsByTag)
		m_pmapFieldsByTag->RemoveAll();

	if (m_parFieldsModified)
		m_parFieldsModified->RemoveAll();

	RemoveAll();
}

//------------------------------------------------------------------------------
void SymTable::UseMapNames ()
{
	ASSERT(!m_pmapFieldsByName);
	if (m_pmapFieldsByName)
		SAFE_DELETE (m_pmapFieldsByName);

	m_pmapFieldsByName = new CMapStringToOb();

	if (GetSize())
	{
		for (int i = 0; i <= GetUpperBound(); i++)
		{
			SymField* pF = GetAt(i);
			
			CString sKey(pF->GetName()); sKey.MakeLower();
			m_pmapFieldsByName->SetAt(sKey, pF);
		}
	}
}

//------------------------------------------------------------------------------
void SymTable::UseMapTags ()
{
	ASSERT(!m_pmapFieldsByTag);
	if (m_pmapFieldsByTag)
		SAFE_DELETE (m_pmapFieldsByTag);

	m_pmapFieldsByTag = new CMapStringToOb();

	if (GetSize())
	{
		for (int i = 0; i <= GetUpperBound(); i++)
		{
			SymField* pF = GetAt(i);
			
			CString sKey(pF->GetTag()); sKey.MakeUpper();
			m_pmapFieldsByTag->SetAt(sKey, pF);
		}
	}
}

//------------------------------------------------------------------------------
void SymTable::AddSpecial ()
{
	/*
	DataLng me((long)this);
	me.SetAsHandle();
	me.SetValid();

	SymField* pF = new SymField(s_SelfReference, DataType::Object, SYMTABLE_SELF_REFERENCE_ID, &me);
	Add(pF);

	m_nCountSpecialFields++;
	*/
}

//------------------------------------------------------------------------------
SymField* SymTable::GetFieldByID (WORD id)	const
{
	for (int i = 0; i <= GetUpperBound(); i++)
	{
		SymField* pF = GetAt(i);
		if (pF->GetId() == id)
			return pF;
	}

	if (m_pParentSymTable)
		return (SymField*) m_pParentSymTable->GetFieldByID(id);
	return NULL;
}

//------------------------------------------------------------------------------
SymField* SymTable::GetField (LPCTSTR szName, BOOL bFindParent /*= TRUE*/)	const
{
	if (m_pmapFieldsByName)
	{
		CObject* pF = NULL;
		CString sKey(szName); sKey.MakeLower();
		if (m_pmapFieldsByName->Lookup(sKey, pF))
		{
			ASSERT_VALID(pF);
			return dynamic_cast<SymField*>(pF);
		}
	}
	else
	{
		int n = GetSize();
		for (int i = 0; i < n; i++)
		{
			if ( _tcsicmp (GetAt(i)->m_strName.GetString(), szName) )
				continue;

			SymField* pF = GetAt(i);
			ASSERT(pF->GetDataType() != DataType::Null);
			return pF;
		}
	}

	if (m_pParentSymTable && bFindParent)
		return (SymField*) m_pParentSymTable->GetField(szName);

	return NULL;
}

//------------------------------------------------------------------------------
SymField* SymTable::GetFieldByTag (LPCTSTR szTag) const
{
	if (m_pmapFieldsByTag)
	{
		CObject* pF = NULL;
		CString sKey(szTag); sKey.MakeUpper();
		if (m_pmapFieldsByTag->Lookup(sKey, pF))
		{
			ASSERT_VALID(pF);
			return dynamic_cast<SymField*>(pF);
		}
	}
	else
	{
		int n = GetSize();
		for (int i = 0; i < n; i++)
		{
			if ( _tcsicmp (GetAt(i)->m_strTag.GetString(), szTag) )
				continue;

			SymField* pF = GetAt(i);
			ASSERT(pF->GetDataType() != DataType::Null);
			return pF;
		}
	}

	if (m_pParentSymTable)
		return (SymField*) m_pParentSymTable->GetFieldByTag(szTag);

	return NULL;
}

//----------------------------------------------------------------------------
BOOL SymTable::RenameField(LPCTSTR pszOldFieldName, LPCTSTR pszNewFieldName)
{
	SymField* pField = NULL;
	int i;
	for (i = 0; i <= GetUpperBound(); i++)
	{
		pField = GetAt(i);
		if (_tcsicmp(pszOldFieldName, pField->GetName()) == 0)
		{
			break;
		}
	}
	if (i == GetSize())
		return FALSE;

	if (m_pmapFieldsByName)
	{
		CString sKey(pField->GetName()); sKey.MakeLower();
		if (!sKey.IsEmpty())
			m_pmapFieldsByName->RemoveKey(sKey);
		sKey = pszNewFieldName; sKey.MakeLower();
		m_pmapFieldsByName->SetAt(sKey, pField);
	}
	pField->SetName(pszNewFieldName);
	return TRUE;
}

//----------------------------------------------------------------------------
BOOL SymTable::DelField(LPCTSTR pszFieldName)
{
	SymField* pField = NULL;
	int i;
	for (i = 0; i <= GetUpperBound(); i++)
	{
		pField	= GetAt(i);
		if (_tcsicmp(pszFieldName, pField->GetName()) == 0)
		{
			break;
		}
	}
	if (i == GetSize()) 
		return FALSE;

	if (OnBeforeDelField(pField))
	{
		if (m_pmapFieldsByName )
		{
			CString sKey(pField->GetName()); sKey.MakeLower();
			if (!sKey.IsEmpty())
				m_pmapFieldsByName->RemoveKey(sKey);
		}
		if (m_pmapFieldsByTag )
		{
			CString sKey(pField->GetTag()); sKey.MakeUpper();
			if (!sKey.IsEmpty())
				m_pmapFieldsByTag->RemoveKey(sKey);
		}

		RemoveAt(i);
	}
	
	return TRUE;
}

//----------------------------------------------------------------------------
BOOL SymTable::DelField(WORD wId)
{
	SymField* pField	= NULL;
	int i;
	for (i = 0; i <= GetUpperBound(); i++)
	{
		pField	= GetAt(i);
		if (pField->GetId() == wId)
		{
			break;
		}
	}
	if (i == GetSize()) 
		return FALSE;

	if (OnBeforeDelField(pField))
	{
		if (m_pmapFieldsByName )
		{
			CString sKey(pField->GetName()); sKey.MakeLower();
			if (!sKey.IsEmpty())
				m_pmapFieldsByName->RemoveKey(sKey);
		}
		if (m_pmapFieldsByTag )
		{
			CString sKey(pField->GetTag()); sKey.MakeUpper();
			if (!sKey.IsEmpty())
				m_pmapFieldsByTag->RemoveKey(sKey);
		}

		RemoveAt(i);
	}
	
	return TRUE;
}

//----------------------------------------------------------------------------
INT_PTR SymTable::Add(SymField* pF)
{
	pF->SetSymTable(this);

	if (m_pmapFieldsByName)
	{
		CString sKey(pF->GetName()); sKey.MakeLower();
		if (!sKey.IsEmpty())
			m_pmapFieldsByName->SetAt(sKey, pF);
	}
	if (m_pmapFieldsByTag)
	{
		CString sKey(pF->GetTag()); sKey.MakeUpper();
		if (!sKey.IsEmpty())
			m_pmapFieldsByTag->SetAt(sKey, pF);
	}
	
	return __super::Add(pF);
}

//----------------------------------------------------------------------------
INT_PTR	SymTable::Append (const SymTable& t)
{
	if (t.GetSize() == 0) 
		return -1;

	INT_PTR nRet = GetSize();

	for (int i = 0; i < t.GetSize(); i++)
	{
		SymField* pF = t.GetAt(i);
		if (!pF)
		{
			ASSERT_TRACE(FALSE, "Error on Append Symbol Table\n");
			continue;
		}

		SymField* pNewF = this->GetField(pF->GetName());
		if (pNewF)
		{
			//se esiste ne aggiorno il valore
			pNewF->AssignData(*pF->GetData());
			continue;
		}

		pNewF = new SymField(*pF);
		
		Add (pNewF);
	}

	return nRet;
}

//---------------------------------------------------------------------------
SymTable* SymTable::CreateLocalScope()
{
	SymTable* pLocal = new SymTable();
	pLocal->SetParent(this);
	
	return pLocal;
}

void SymTable::DeleteMeAsLocalScope()
{
	delete this;
}

//------------------------------------------------------------------------------
BOOL SymTable::ResolveCallMethod(CString sFuncName, CFunctionDescription& aMethod, CString& sHandleName) const
{
	sHandleName.Empty();
	int idx = sFuncName.Find('.');
	if (idx < 0)
		return FALSE;

	CString sName = sFuncName.Left(idx);
	sFuncName = sFuncName.Mid(idx + 1);

	SymField* pField = GetField(sName);
	if (!pField || (pField->GetDataType() != DataType::Object && pField->GetDataType() != DataType::Long))
		return FALSE;

	CFunctionDescription* pF = pField->FindMethod(sFuncName);
	if (!pF)
		return FALSE;

	aMethod = *pF;
	sHandleName = sName;
	return TRUE;
}

//------------------------------------------------------------------------------
BOOL SymTable::ExpandAlias	(const CString& sName, CString& sExpandedName) const
{
	int idx = sName.Find('.');
	if (idx < 0)
		return FALSE;

	CString sPrefix = sName.Left(idx);
	CString sFuncName = sName.Mid(idx);	//conserva il .

	for (int i = 0; i < m_arAlias.GetSize(); i++)
	{
		Alias* pA = (Alias*) m_arAlias.GetAt(i);

		if(pA->m_strAlias.CompareNoCase(sPrefix))
			continue;

		sExpandedName = pA->m_strExpanded + sFuncName;
		return TRUE;
	}

	return m_pParentSymTable ? m_pParentSymTable->ExpandAlias(sName, sExpandedName) : FALSE;
}

//------------------------------------------------------------------------------
BOOL SymTable::UnParse (CXMLNode* pRootNode)
{
	pRootNode = pRootNode->CreateNewChild(L"SymbolTable");

	if (m_pParentSymTable)
	{
		m_pParentSymTable->UnParse(pRootNode->CreateNewChild(L"Parent"));	
	}

	CString sPrefix;
	CXMLNode* pParentNode = NULL;
	int s = GetSize();
	for (int i = 0; i < s; i++)
	{
		SymField* pF = GetAt(i);

		CXMLNode* pNode = NULL;
		CString sName = pF->GetName();
		int idx = sName.Find('.');

		if (idx < 0)
		{
			pNode = pRootNode->CreateNewChild(sName);
		}
		else
		{
			CString sPref = sName.Left(idx);
			if (sPref != sPrefix)
			{
				sPrefix = sPref;
				pParentNode = pRootNode->CreateNewChild(sPrefix);
			}
			ASSERT_VALID(pParentNode);
			pNode = pParentNode->CreateNewChild(sName.Mid(idx + 1));
		}

		if (!pF->GetTag().IsEmpty())
			pNode->SetAttribute(L"tag", pF->GetTag());

		if (pF->GetProvider())
		{
			pNode->SetAttribute(L"rows", L"true");

			//pNode->SetAttribute(L"dataType", pF->GetData()->GetDataType().ToString());

			for (int r = 0; r < pF->GetProvider()->GetRowCount(); r++)
			{
				CXMLNode* pRowNode = pNode->CreateNewChild(L"Record");

				DataObj* pRowVal = pF->GetProvider()->GetData(sName, r);
				ASSERT_VALID(pRowVal);

				pRowNode->SetAttribute(L"row", cwsprintf(L"%d", r));
				pRowNode->SetAttribute(L"value", pRowVal->Str());
			}
		}
		else
		{
			pNode->SetAttribute(L"value", pF->GetData()->Str());
			pNode->SetAttribute(L"dataType", pF->GetData()->GetDataType().ToString());
			if (pF->GetData()->IsKindOf(RUNTIME_CLASS(DataLng)) && pF->GetData()->IsHandle())
				pNode->SetAttribute(L"handle", L"true");
		}

		//if (!pF->GetTitle().IsEmpty())
		//	pNode->SetAttribute(L"title", pF->GetTitle());
	}

	return TRUE;
}

//-----------------------------------------------------------------------------
CStringArray* SymTable::TraceFieldsModified(CStringArray* ar) 
{ 
	ASSERT_VALID(GetRoot());
	if (ar) ASSERT_VALID(ar);
	CStringArray* arOld = GetRoot()->m_parFieldsModified; 
	GetRoot()->m_parFieldsModified = ar; 
	if (arOld) ASSERT_VALID(arOld);
	return arOld;
}

void SymTable::TraceFieldModify(const CString& name, BOOL noDuplicate/* = TRUE*/)
{
	ASSERT_VALID(GetRoot());
	if (GetRoot()->m_parFieldsModified)
	{
		ASSERT_VALID(GetRoot()->m_parFieldsModified);
		if (noDuplicate)
			if (CStringArray_Find(*GetRoot()->m_parFieldsModified, name) > -1) return;

		GetRoot()->m_parFieldsModified->Add(name);
	}
}

CStringArray* SymTable::TraceFieldsUsed(CStringArray* ar)
{
	ASSERT_VALID(GetRoot());
	if (ar) ASSERT_VALID(ar);
	CStringArray* arOld = GetRoot()->m_parFieldsUsed; 
	GetRoot()->m_parFieldsUsed = ar;
	if (arOld) ASSERT_VALID(arOld);
	return arOld;
}

void SymTable::TraceFieldsUsed(const CString& name, BOOL noDuplicate/* = TRUE*/)
{
	ASSERT_VALID(GetRoot());
	if (GetRoot()->m_parFieldsUsed)
	{
		ASSERT_VALID(GetRoot()->m_parFieldsUsed);
		if (noDuplicate)
			if (CStringArray_Find(*GetRoot()->m_parFieldsUsed, name) > -1) return;

		GetRoot()->m_parFieldsUsed->Add(name);
	}
}

//////////////////////////////////////////////////////////////////////////////////////////////////

CString SymTable::GenerateName(LPCTSTR pszFromName, const CString& strPrefix)
{
	CString strTmpName(cwsprintf(strPrefix, pszFromName));
	CString	strNewName(strTmpName);
	int		i = 0;

	while (ExistField(strNewName))
	{
		i++;
		strNewName = strTmpName + cwsprintf(_T("_%d"), GetCurId() + i);
	}

	return strNewName;
}

//////////////////////////////////////////////////////////////////////////////////////////////////
int CompareFieldByName(CObject* po1, CObject* po2)
{
	SymField* p1 = (SymField*)po1;
	SymField* p2 = (SymField*)po2;

	return p1->GetName().CompareNoCase(p2->GetName());
}

int CompareFieldByID(CObject* po1, CObject* po2)
{
	SymField* p1 = (SymField*)po1;
	SymField* p2 = (SymField*)po2;

	return p1->GetId() < p2->GetId() ? -1 : (p1->GetId() > p2->GetId() ? 1 : 0);
}

//////////////////////////////////////////////////////////////////////////////////////////////////
