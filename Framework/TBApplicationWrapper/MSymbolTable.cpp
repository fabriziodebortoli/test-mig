#include "StdAfx.h"

#include "MDataObj.h"
#include "MSymbolTable.h"

using namespace Microarea::Framework::TBApplicationWrapper;
using namespace Microarea::TaskBuilderNet::Interfaces::Model;

/////////////////////////////////////////////////////////////////////////////
// 				class MSymTable Implementation
/////////////////////////////////////////////////////////////////////////////

//-----------------------------------------------------------------------------
MSymTable::MSymTable (System::IntPtr ptrSymTable, bool ownTable)
{
	SymTable* pT = (SymTable*) ptrSymTable.ToInt64();

	ASSERT_VALID(pT);

	m_ppSymTable = new SymTablePtr(pT);

	m_ownTable = ownTable;

}

//-----------------------------------------------------------------------------
MSymTable::MSymTable ()
{
	m_ppSymTable = new SymTablePtr(new SymTable());

	m_ownTable = true;
}

//-----------------------------------------------------------------------------
MSymTable::~MSymTable ()
{
	this->!MSymTable();
	System::GC::SuppressFinalize(this);
}

//-----------------------------------------------------------------------------
MSymTable::!MSymTable()
{
	if (m_ownTable)
	{
		//SAFE_DELETE(m_ppSymTable);
		delete m_ppSymTable;
	}
}

//-----------------------------------------------------------------------------
SymTable* MSymTable::GetSymTable()
{
	return (SymTable*) *m_ppSymTable;
}

//-----------------------------------------------------------------------------
IDataObj^ MSymTable::GetFieldValueByName (System::String^ name)
{
	CString sName(name);

	//SymField* pField = GetSymTable()->GetField((LPCTSTR)sName);
	SymField* pField = (*m_ppSymTable)->GetField((LPCTSTR)sName);
	if (!pField)
		return nullptr;

	DataObj* pObj = pField->GetData();
	ASSERT_VALID(pObj);

	pObj = pObj->Clone();

	MDataObj^ obj = MDataObj::Create(pObj);
	return obj;
}

//-----------------------------------------------------------------------------
IDataObj^ MSymTable::GetFieldValueByTag (System::String^ tag)
{
	CString sTag(tag);

	SymField* pField = (*m_ppSymTable)->GetFieldByTag((LPCTSTR)sTag);
	if (!pField)
		return nullptr;

	DataObj* pObj = pField->GetData();
	if (!pObj)
		return nullptr;

	pObj = pObj->Clone();

	MDataObj^ obj = MDataObj::Create(pObj);
	return obj;
}
//-----------------------------------------------------------------------------
void MSymTable::Add (System::String^ name, System::String^ tag, IDataObj^ o)
{
	//	SymField (const CString& strName, DataType dt = DataType::Null, WORD nId = SpecialReportField::NO_INTERNAL_ID, DataObj* pValue = NULL, BOOL bCloneValue = TRUE);
	MDataObj^ mobj = dynamic_cast<MDataObj^>(o);
	ASSERT(mobj != nullptr);

	DataObj* pObj = mobj->GetDataObj();
	ASSERT_VALID(pObj);

	SymField* pField = new SymField(CString(name), pObj->GetDataType(),  SpecialReportField::NO_INTERNAL_ID, pObj, FALSE);
	pField->SetTag(CString(tag));
	
	GetSymTable()->Add(pField);
}

//-----------------------------------------------------------------------------
void MSymTable::Clear()
{
	GetSymTable()->Clear();
}