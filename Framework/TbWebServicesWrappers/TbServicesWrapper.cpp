
#include "stdafx.h"

#include <TBGeneric\FunctionCall.h>
#include <TbGenlibManaged\Main.h>

#include "tbserviceswrapper.h"

//-----------------------------------------------------------------------------
TbServicesWrapper::TbServicesWrapper(const CString& strService, const CString& strServiceNamespace, const CString& strServer, int nWebServicesPort)
:
m_strService(strService),
m_strServiceNamespace(strServiceNamespace),
m_strServer(strServer),
m_nWebServicesPort(nWebServicesPort)
{
	
}

//-----------------------------------------------------------------------------
TbServicesWrapper::~TbServicesWrapper(void)
{
}

//-----------------------------------------------------------------------------
void TbServicesWrapper::CloseTb(const CString& authenticationToken)
{
	CFunctionDescription aFunctionDescription(_T("CloseTb"));
	
	aFunctionDescription.SetServer				(m_strServer);
	aFunctionDescription.SetService				(m_strService);
	aFunctionDescription.SetServiceNamespace	(m_strServiceNamespace);
	aFunctionDescription.SetPort				(m_nWebServicesPort);
	
	aFunctionDescription.AddStrParam(_T("authenticationToken"), authenticationToken);

	InvokeWCFFunction(&aFunctionDescription, FALSE);
}