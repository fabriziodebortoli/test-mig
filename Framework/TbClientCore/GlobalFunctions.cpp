#include "stdafx.h"

#include <tbgeneric\functioncall.h>
#include <tbgenlibmanaged\main.h>

#include "globalfunctions.h"

//---------------------------------------------------------------------------------------
BOOL AfxInvokeSoapMethod(CFunctionDescription *pFunctionDescription)
{
	try
	{
		BOOL bInternalCall = pFunctionDescription->GetService().IsEmpty();
		return InvokeWCFFunction(pFunctionDescription, bInternalCall);
	}
	catch (CException* pEx)
	{
		AfxGetDiagnostic()->Add(pEx);
		return FALSE;
	}
	catch (...)
	{
		return FALSE;
	}
}
//---------------------------------------------------------------------------------------
BOOL AfxInvokeSoapMethod(CFunctionDescription *pFunctionDescription, int nPort)
{
	return AfxInvokeSoapMethod(pFunctionDescription);
}
