#include "stdafx.h"

#include <TBGes\extdoc.h>
#include <TBGES\xmlgesinfo.h>

#include <TBWebServicesWrappers\LoginManagerInterface.h>
#include "TXEParameters.h"
#include "XEngineObject.h"

//----------------------------------------------------------------------------
// TXEParameters implementation
//-----------------------------------------------------------------------------
//-----------------------------------------------------------------------------
IMPLEMENT_DYNCREATE(TXEParameters, SqlRecord)

//-----------------------------------------------------------------------------
TXEParameters::TXEParameters(BOOL bCallInit)
	:
	SqlRecord		(GetStaticName()),
	f_IdParam		(1),
	f_EncodTypeUTF8	(GetDefaultForUseUTF8())
{
	BindRecord();	
	if (bCallInit) Init(); 
}

//-----------------------------------------------------------------------------
void TXEParameters::Init()
{
	f_EncodTypeUTF8 = GetDefaultForUseUTF8();

	f_DomainName = _T("DEFAULT_DOMAIN");
	f_SiteName = _T("DEFAULT_SITE");
	f_SiteCode = _T("DESI");

	f_MaxDoc = HEADER_DEFAULT_DOCUMENT_NUM;
	f_MaxKByte = HEADER_DEFAULT_DOC_DIMENSION;
	f_EnvPaddingNum = HEADER_DEFAULT_PADDING_NUM;
}

//-----------------------------------------------------------------------------
BOOL TXEParameters::GetDefaultForUseUTF8()
{
	return TRUE;
}

//-----------------------------------------------------------------------------
void TXEParameters::BindRecord()
{
	BEGIN_BIND_DATA	()
		BIND_DATA	(_T("IdParam"),			f_IdParam);
		BIND_DATA	(_T("DomainName"),		f_DomainName);
		BIND_DATA	(_T("SiteName"),		f_SiteName);
		BIND_DATA	(_T("SiteCode"),		f_SiteCode );
		BIND_DATA	(_T("EncodingType"),	f_EncodTypeUTF8);
		BIND_DATA	(_T("ImportPath"),		f_ImportPath);
		BIND_DATA	(_T("ExportPath"),		f_ExportPath);
		BIND_DATA	(_T("MaxDoc"),			f_MaxDoc);
		BIND_DATA	(_T("MaxKByte"),		f_MaxKByte);
		BIND_DATA	(_T("UseEnvClassExt"),	f_UseEnvClassExt);
		BIND_DATA	(_T("EnvPaddingNum"),	f_EnvPaddingNum);
		BIND_DATA	(_T("UseAttribute"),	f_UseAttribute);
		BIND_DATA	(_T("UseEnumAsNum"),	f_UseEnumAsNum);
	END_BIND_DATA()
}

//-----------------------------------------------------------------------------
LPCTSTR TXEParameters::GetStaticName() { return _T("XE_Parameters"); }

/////////////////////////////////////////////////////////////////////////////
//	TableReader				
/////////////////////////////////////////////////////////////////////////////
//=============================================================================
IMPLEMENT_DYNAMIC(TRXEParameters, TableReader)

//------------------------------------------------------------------------------
TRXEParameters::TRXEParameters(CAbstractFormDoc* pDocument)
	: 
	TableReader(RUNTIME_CLASS(TXEParameters), pDocument)
{
}

//------------------------------------------------------------------------------
void TRXEParameters::OnDefineQuery()
{
	m_pTable->SelectAll();	
}
	
//------------------------------------------------------------------------------
void TRXEParameters::OnPrepareQuery()
{
}

//------------------------------------------------------------------------------
BOOL TRXEParameters::IsEmptyQuery()
{
	return FALSE;
}

/////////////////////////////////////////////////////////////////////////////
// Diagnostics

#ifdef _DEBUG
void TRXEParameters::Dump(CDumpContext& dc) const
{
	ASSERT_VALID(this);
	AFX_DUMP0(dc, "\nTRXEParameters:");
}

void TRXEParameters::AssertValid() const
{
	TableReader::AssertValid();
}
#endif