#include "stdafx.h"

#include "Validator.h"


IMPLEMENT_DYNAMIC(CValidator, CObject)
//-----------------------------------------------------------------------------
CValidator::CValidator()
{

}

//-----------------------------------------------------------------------------
CValidator::~CValidator()
{
}

IMPLEMENT_DYNCREATE(CEmptyValidator, CValidator)

//-----------------------------------------------------------------------------
BOOL CEmptyValidator::IsValid(DataObj* pDataObj, CString& message, CDiagnostic::MsgType& msgType)
{
	if (pDataObj->IsEmpty())
	{
		message = _TB("The data is mandatory.");
		msgType = CDiagnostic::Error;
		return FALSE;
	}
	return TRUE;
}


