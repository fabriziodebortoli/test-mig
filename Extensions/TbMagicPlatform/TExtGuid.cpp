
#include "stdafx.h"             

#include <TBGes\extdoc.h>

#include "TExtGuid.h"

#ifdef _DEBUG
#undef THIS_FILE
static const char THIS_FILE[] = __FILE__;
#endif

//....................................................................  Parameters
static const TCHAR szP1[]	= _T("P1");

/////////////////////////////////////////////////////////////////////////////
//						class TExtGuid Implementation
/////////////////////////////////////////////////////////////////////////////
//
//=============================================================================
IMPLEMENT_DYNCREATE(TExtGuid, SqlRecord) 

//-----------------------------------------------------------------------------
TExtGuid::TExtGuid(BOOL bCallInit)
	:
	SqlRecord 					(GetStaticName())
{
	BindRecord();	

	if (bCallInit) Init(); 
}

//-----------------------------------------------------------------------------
void TExtGuid::BindRecord()
{
	BEGIN_BIND_DATA	();
		BIND_DATA	(_NS_FLD("DocGuid"),					f_DocGuid);
	END_BIND_DATA();
}

//-----------------------------------------------------------------------------
LPCTSTR TExtGuid::GetStaticName() { return _NS_TBL("TB_ExtGuid"); }

/////////////////////////////////////////////////////////////////////////////
//				class TRExtGuid Implementation
/////////////////////////////////////////////////////////////////////////////
//
//=============================================================================
IMPLEMENT_DYNAMIC (TRExtGuid, TableReader)

//------------------------------------------------------------------------------
TRExtGuid::TRExtGuid (CAbstractFormDoc* pDocument /* NULL */)
	: 
	TableReader	(RUNTIME_CLASS(TExtGuid),pDocument)
{
}

//------------------------------------------------------------------------------
void TRExtGuid::OnDefineQuery ()
{
	m_pTable->SelectAll			();
	m_pTable->AddFilterColumn	(GetRecord()->f_DocGuid);
	m_pTable->AddParam			(szP1, GetRecord()->f_DocGuid);
}
	
//------------------------------------------------------------------------------
void TRExtGuid::OnPrepareQuery ()
{
	m_pTable->SetParamValue(szP1,	m_DocGuid);
}

//------------------------------------------------------------------------------
BOOL TRExtGuid::IsEmptyQuery()
{
	return m_DocGuid.IsEmpty();
}

//------------------------------------------------------------------------------
TableReader::FindResult TRExtGuid::FindRecord(const DataGuid& aDocGuid)
{
	m_DocGuid = aDocGuid;
	
	return TableReader::FindRecord();
}

/////////////////////////////////////////////////////////////////////////////
//					class TUExtGuid Implementation
/////////////////////////////////////////////////////////////////////////////
//
//=============================================================================
IMPLEMENT_DYNAMIC (TUExtGuid, TableUpdater)
//------------------------------------------------------------------------------
TUExtGuid::TUExtGuid
	(
		CAbstractFormDoc* 	pDocument,
		CDiagnostic*		pDiagnostic
	)													
	: 
	TableUpdater(RUNTIME_CLASS(TExtGuid), pDocument, pDiagnostic)
{
}

//------------------------------------------------------------------------------
void TUExtGuid::OnDefineQuery ()
{
	m_pTable->SelectAll			();

	m_pTable->AddFilterColumn	(GetRecord()->f_DocGuid);
	m_pTable->AddParam			(szP1,	GetRecord()->f_DocGuid);
}
	
//------------------------------------------------------------------------------
void TUExtGuid::OnPrepareQuery ()
{
	m_pTable->SetParamValue(szP1, m_DocGuid);
}

//------------------------------------------------------------------------------
BOOL TUExtGuid::IsEmptyQuery()
{
	return	m_DocGuid.IsEmpty();
}

//------------------------------------------------------------------------------
TableUpdater::FindResult TUExtGuid::FindRecord (const DataGuid& aDocGuid, BOOL bLock)
{
	m_DocGuid	= aDocGuid;
	return TableUpdater::FindRecord(bLock);
}