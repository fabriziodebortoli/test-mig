#include "stdafx.h"

#include <TbGeneric\GeneralFunctions.h>
#include <tbgeneric\functioncall.h>
#include <TbGenlibManaged\Main.h>
#include <TbGenlib\BaseApp.h>
#include <TbGeneric\CRITICAL.H>
#include <TbGes\ExtDoc.h>
#include <TbGes\DBT.h>
#include <TbOleDB\SqlRec.h>
#include <TbOleDb\RIChecker.h>

#include "DataSynchronizerWrapper.h"


///////////////////////////////////////////////////////////////////////////////
//					CDataSynchronizerWrapper declaration
///////////////////////////////////////////////////////////////////////////////
//
//----------------------------------------------------------------------------
CDataSynchronizerWrapper::CDataSynchronizerWrapper(const CString& strService, const CString& strServiceNamespace, const CString& strServer, int nWebServicesPort)
:
	m_strService(strService),
	m_strServiceNamespace(strServiceNamespace),
	m_strServer(strServer),
	m_nWebServicesPort(nWebServicesPort)
{
	
}

//----------------------------------------------------------------------------
CDataSynchronizerWrapper::CDataSynchronizerWrapper(CDataSynchronizerWrapper* ds)
	:
	m_strService(ds->m_strService),
	m_strServiceNamespace(ds->m_strServiceNamespace),
	m_strServer(ds->m_strServer),
	m_nWebServicesPort(ds->m_nWebServicesPort)
{

}

//----------------------------------------------------------------------------
void CDataSynchronizerWrapper::InitFunction(CFunctionDescription& aFunctionDescription) const
{
	aFunctionDescription.SetServer				(m_strServer);
	aFunctionDescription.SetService				(m_strService);
	aFunctionDescription.SetServiceNamespace	(m_strServiceNamespace);
	aFunctionDescription.SetPort				(m_nWebServicesPort);
}

//----------------------------------------------------------------------------
BOOL CDataSynchronizerWrapper::Notify(DataLng& logId, const CString& providerName, const CString& tableName,const CString& nameSpace, const CString& guidDoc,const CString& sOnlyForDMS, const CString& iMagoConfigurations)
{
	if (logId.IsEmpty())
		return TRUE;

	CFunctionDescription aFunctionDescription(_T("Notify"));
	InitFunction(aFunctionDescription);

	aFunctionDescription.AddStrParam(_T("authenticationToken"), AfxGetAuthenticationToken());
	aFunctionDescription.AddParam(_T("logId"), &logId);
	aFunctionDescription.AddStrParam(_T("providerName"), providerName);	
	aFunctionDescription.AddStrParam(_T("tableName"), tableName);
	aFunctionDescription.AddStrParam(_T("docNamespace"), nameSpace);
	aFunctionDescription.AddStrParam(_T("docGuid"), guidDoc);
	aFunctionDescription.AddStrParam(_T("onlyForDMS"), sOnlyForDMS);
	aFunctionDescription.AddStrParam(_T("iMagoConfigurations"), iMagoConfigurations);

	aFunctionDescription.SetReturnValueDescription(CDataObjDescription(_T(""), DataType::Bool, CDataObjDescription::_OUT));

	if(!InvokeWCFFunction(&aFunctionDescription, FALSE))
	{
		ASSERT(FALSE);
		return FALSE;
	}
	
	DataBool* pdbVal = (DataBool*)aFunctionDescription.GetReturnValue();
	return (pdbVal ? *pdbVal : FALSE);
}

//----------------------------------------------------------------------------
BOOL CDataSynchronizerWrapper::NotifyGuid(DataLng& logId, const CString& providerName, const CString& tableName, const CString& nameSpace, const CString& guidDoc, const CString& sOnlyForDMS, const CString& iMagoConfigurations, CString& gRequestGuid)
{
	if (logId.IsEmpty())
		return TRUE;

	CFunctionDescription aFunctionDescription(_T("NotifyGuid"));
	InitFunction(aFunctionDescription);
	DataStr reqGuid;
	aFunctionDescription.AddStrParam(_T("authenticationToken"), AfxGetAuthenticationToken());
	aFunctionDescription.AddParam(_T("logId"), &logId);
	aFunctionDescription.AddStrParam(_T("providerName"), providerName);
	aFunctionDescription.AddStrParam(_T("tableName"), tableName);
	aFunctionDescription.AddStrParam(_T("docNamespace"), nameSpace);
	aFunctionDescription.AddStrParam(_T("docGuid"), guidDoc);
	aFunctionDescription.AddStrParam(_T("onlyForDMS"), sOnlyForDMS);
	aFunctionDescription.AddStrParam(_T("iMagoConfigurations"), iMagoConfigurations);
	aFunctionDescription.AddOutParam(_T("requestGuid"), &reqGuid);

	aFunctionDescription.SetReturnValueDescription(CDataObjDescription(_T(""), DataType::Bool, CDataObjDescription::_OUT));

	if (!InvokeWCFFunction(&aFunctionDescription, FALSE))
	{
		ASSERT(FALSE);
		return FALSE;
	}

	DataBool* pdbVal = (DataBool*)aFunctionDescription.GetReturnValue();
	if (pdbVal && !reqGuid.IsEmpty())
		gRequestGuid = reqGuid;
	return (pdbVal ? *pdbVal : FALSE);
}



//----------------------------------------------------------------------------
BOOL CDataSynchronizerWrapper::SynchronizeOutbound(const CString& providerName, const CString& xmlParametes)
{
	if (providerName.IsEmpty())
		return TRUE;

	CFunctionDescription aFunctionDescription(_T("SynchronizeOutbound"));
	InitFunction(aFunctionDescription);
		
	aFunctionDescription.AddStrParam(_T("authenticationToken"), AfxGetAuthenticationToken());
	aFunctionDescription.AddStrParam(_T("providerName"), providerName);	
	aFunctionDescription.AddStrParam(_T("xmlParametes"), xmlParametes);
	aFunctionDescription.SetReturnValueDescription(CDataObjDescription(_T(""), DataType::Bool, CDataObjDescription::_OUT));

	if(!InvokeWCFFunction(&aFunctionDescription, FALSE))
	{
		ASSERT(FALSE);
		return FALSE;
	}
	
	DataBool* pdbVal = (DataBool*)aFunctionDescription.GetReturnValue();
	return (pdbVal ? *pdbVal : FALSE);
}

BOOL CDataSynchronizerWrapper::SynchronizeOutboundDelta(const CString& providerName, const CString& startSynchroDate, const CString& xmlParametes)
{
	if (providerName.IsEmpty())
		return TRUE;

	CFunctionDescription aFunctionDescription(_T("SynchronizeOutboundDelta"));
	InitFunction(aFunctionDescription);

	aFunctionDescription.AddStrParam(_T("authenticationToken"), AfxGetAuthenticationToken());
	aFunctionDescription.AddStrParam(_T("providerName"), providerName);
	aFunctionDescription.AddStrParam(_T("startSynchroDate"), startSynchroDate);
	aFunctionDescription.AddStrParam(_T("xmlParametes"), xmlParametes);
	aFunctionDescription.SetReturnValueDescription(CDataObjDescription(_T(""), DataType::Bool, CDataObjDescription::_OUT));

	if (!InvokeWCFFunction(&aFunctionDescription, FALSE))
	{
		ASSERT(FALSE);
		return FALSE;
	}

	DataBool* pdbVal = (DataBool*)aFunctionDescription.GetReturnValue();
	return (pdbVal ? *pdbVal : FALSE);
}


//----------------------------------------------------------------------------
BOOL CDataSynchronizerWrapper::SynchronizeErrorsRecovery(const CString&  providerName)
{
	if (providerName.IsEmpty())
		return TRUE;

	CFunctionDescription aFunctionDescription(_T("SynchronizeErrorsRecovery"));
	InitFunction(aFunctionDescription);
		
	aFunctionDescription.AddStrParam(_T("authenticationToken"), AfxGetAuthenticationToken());
	aFunctionDescription.AddStrParam(_T("providerName"), providerName);	
	
	aFunctionDescription.SetReturnValueDescription(CDataObjDescription(_T(""), DataType::Bool, CDataObjDescription::_OUT));

	if(!InvokeWCFFunction(&aFunctionDescription, FALSE))
	{
		ASSERT(FALSE);
		return FALSE;
	}
	
	DataBool* pdbVal = (DataBool*)aFunctionDescription.GetReturnValue();
	return (pdbVal ? *pdbVal : FALSE);
}

//----------------------------------------------------------------------------
BOOL CDataSynchronizerWrapper::SynchronizeErrorsRecoveryImago(const CString&  providerName, CString& strRequestGuid)
{
	if (providerName.IsEmpty())
		return TRUE;

	CFunctionDescription aFunctionDescription(_T("SynchronizeErrorsRecoveryImago"));
	InitFunction(aFunctionDescription);
	DataStr strGuid;
	aFunctionDescription.AddStrParam(_T("authenticationToken"), AfxGetAuthenticationToken());
	aFunctionDescription.AddStrParam(_T("providerName"), providerName);
	aFunctionDescription.AddOutParam(_T("requestGuid"),	&strGuid);
	aFunctionDescription.SetReturnValueDescription(CDataObjDescription(_T(""), DataType::Bool, CDataObjDescription::_OUT));

	if (!InvokeWCFFunction(&aFunctionDescription, FALSE))
	{
		ASSERT(FALSE);
		return FALSE;
	}

	DataBool* pdbVal = (DataBool*)aFunctionDescription.GetReturnValue();
	if (pdbVal && (*pdbVal) && !strGuid.IsEmpty())
		strRequestGuid = strGuid;
	return (pdbVal ? *pdbVal : FALSE);
}

//----------------------------------------------------------------------------
BOOL CDataSynchronizerWrapper::IsMassiveSynchronizing(/*const CString&  providerName*/)
{
	/*if (providerName.IsEmpty())
		return TRUE;*/

	CFunctionDescription aFunctionDescription(_T("IsMassiveSynchronizing"));
	InitFunction(aFunctionDescription);
		
	aFunctionDescription.AddIntParam(_T("companyId"),	 AfxGetLoginInfos()->m_nCompanyId);
	//aFunctionDescription.AddStrParam(_T("providerName"), providerName);	
	
	aFunctionDescription.SetReturnValueDescription(CDataObjDescription(_T(""), DataType::Bool, CDataObjDescription::_OUT));

	if(!InvokeWCFFunction(&aFunctionDescription, FALSE))
	{
		ASSERT(FALSE);
		return FALSE;
	}
	
	DataBool* pdbVal = (DataBool*)aFunctionDescription.GetReturnValue();
	return (pdbVal ? *pdbVal : FALSE);
}

//----------------------------------------------------------------------------
BOOL CDataSynchronizerWrapper::IsMassiveValidating()
{
	CFunctionDescription aFunctionDescription(_T("IsMassiveValidating"));
	InitFunction(aFunctionDescription);
	
	aFunctionDescription.SetReturnValueDescription(CDataObjDescription(_T(""), DataType::Bool, CDataObjDescription::_OUT));

	if (!InvokeWCFFunction(&aFunctionDescription, FALSE))
	{
		ASSERT(FALSE);
		return FALSE;
	}

	DataBool* pdbVal = (DataBool*)aFunctionDescription.GetReturnValue();
	return (pdbVal ? *pdbVal : FALSE);
}

//----------------------------------------------------------------------------
CString CDataSynchronizerWrapper::GetActionsForDocument(const CString& docNamepace, const CString& providerName)
{
	CFunctionDescription aFunctionDescription(_T("GetActionsForDocument"));
	InitFunction(aFunctionDescription);
	aFunctionDescription.AddStrParam(_T("docNamespace"), docNamepace);
	aFunctionDescription.AddStrParam(_T("providerName"), providerName);

	aFunctionDescription.SetReturnValueDescription(CDataObjDescription(_T(""), DataType::String, CDataObjDescription::_OUT));

	if (!InvokeWCFFunction(&aFunctionDescription, FALSE))
	{
		ASSERT(FALSE);
		return _T("");
	}

	DataStr * pAction = (DataStr*)aFunctionDescription.GetReturnValue();
	return (pAction ? *pAction : _T(""));
}


//----------------------------------------------------------------------------
BOOL CDataSynchronizerWrapper::TestProviderParameters
(
	const CString& providerName, 
	const CString& url, 
	const CString& username, 
	const CString&  password,
	BOOL skipCrtValidation,
	const CString& parameters, 
	CString& strErrMsg,
	BOOL bDisabled
)
{
	DataStr aMsg;
	DataBool bParamDisabled = bDisabled;
	DataBool bSkipCrtValidation = skipCrtValidation;
	CFunctionDescription aFunctionDescription(_T("TestProviderParameters"));
	InitFunction(aFunctionDescription);

	aFunctionDescription.AddStrParam(_T("authenticationToken"), AfxGetAuthenticationToken());
	aFunctionDescription.AddStrParam(_T("providerName"),		providerName);
	aFunctionDescription.AddStrParam(_T("url"),					url);
	aFunctionDescription.AddStrParam(_T("username"),			username);
	aFunctionDescription.AddStrParam(_T("password"),			password);
	aFunctionDescription.AddParam(_T("skipCrtValidation"), &bSkipCrtValidation);
	aFunctionDescription.AddStrParam(_T("parameters"),			parameters);	
	aFunctionDescription.AddOutParam(_T("message"),				&aMsg);
	aFunctionDescription.AddParam	(_T("disabled"),			&bParamDisabled);

	aFunctionDescription.SetReturnValueDescription(CDataObjDescription(_T(""), DataType::Bool, CDataObjDescription::_OUT));

	if(!InvokeWCFFunction(&aFunctionDescription, FALSE))
	{
		strErrMsg = _TB("Impossible to call the function TestProviderParameters of the webservice DataSynchronizer.");
		return FALSE;
	}
	
	DataBool* pdbVal = (DataBool*)aFunctionDescription.GetReturnValue();

	BOOL bOK = (pdbVal ? *pdbVal : FALSE);
	if (!bOK && !aMsg.IsEmpty())
	    strErrMsg =  aMsg;

	return bOK;
}

//----------------------------------------------------------------------------
BOOL CDataSynchronizerWrapper::SetProviderParameters
(
	const CString& providerName, 
	const CString& url, 
	const CString& username, 
	const CString& password, 
	BOOL     skipCrtValidation,
	const CString& IAFModule, 
	const CString& parameters,
		  CString& message
)
{
	DataStr aMsg;
	DataBool bSkipCrtValidation = skipCrtValidation;
	CFunctionDescription aFunctionDescription(_T("SetProviderParameters"));
	InitFunction(aFunctionDescription);

	aFunctionDescription.AddStrParam(_T("authenticationToken"),	AfxGetAuthenticationToken());	// solo in questo caso passo il token valido!
	aFunctionDescription.AddStrParam(_T("providerName"),		providerName);
	aFunctionDescription.AddStrParam(_T("url"),					url);
	aFunctionDescription.AddStrParam(_T("username"),			username);
	aFunctionDescription.AddStrParam(_T("password"),			password);	
	aFunctionDescription.AddParam   (_T("skipCrtValidation"),   &bSkipCrtValidation);
	aFunctionDescription.AddStrParam(_T("IAFModule"),			IAFModule);
	aFunctionDescription.AddStrParam(_T("parameters"),			parameters);	
	aFunctionDescription.AddOutParam(_T("message"),				&aMsg);

	aFunctionDescription.SetReturnValueDescription(CDataObjDescription(_T(""), DataType::Bool, CDataObjDescription::_OUT));

	if(!InvokeWCFFunction(&aFunctionDescription, FALSE))
	{
		message = _TB("Impossible to call the function SetProviderParameters of the webservice DataSynchronizer.");
		return FALSE;
	}
	
	DataBool* pdbVal = (DataBool*)aFunctionDescription.GetReturnValue();

	BOOL bOK = (pdbVal ? *pdbVal : FALSE);
	if (!bOK && !aMsg.IsEmpty())
		message =  aMsg;

	return bOK;
}

//----------------------------------------------------------------------------
BOOL CDataSynchronizerWrapper::ValidateDocument(const CString& providerName, const CString& nameSpace, const CString& tableName, const CString& guidDoc, const CString& serializedErrors, CString& message)
{
	DataStr aMsg;
	CFunctionDescription aFunctionDescription(_T("ValidateDocument"));
	InitFunction(aFunctionDescription);

	aFunctionDescription.AddStrParam(_T("authenticationToken"),		AfxGetAuthenticationToken());
	aFunctionDescription.AddStrParam(_T("providerName"),			providerName);
	aFunctionDescription.AddStrParam(_T("nameSpace"),				nameSpace);
	aFunctionDescription.AddStrParam(_T("tableName"),				tableName);
	aFunctionDescription.AddStrParam(_T("guidDoc"),					guidDoc);
	aFunctionDescription.AddStrParam(_T("serializedErrors"),		serializedErrors);
	aFunctionDescription.AddIntParam(_T("workerId"),				AfxGetWorkerId());
	aFunctionDescription.AddOutParam(_T("message"),					&aMsg);
	
	aFunctionDescription.SetReturnValueDescription(CDataObjDescription(_T(""), DataType::Bool, CDataObjDescription::_OUT));

	if (!InvokeWCFFunction(&aFunctionDescription, FALSE))
	{
		message = _TB("Impossible to call the function Validation of the webservice DataSynchronizer.");
		return FALSE;
	}

	DataBool* pdbVal = (DataBool*)aFunctionDescription.GetReturnValue();

	BOOL bOK = (pdbVal ? *pdbVal : FALSE);
	if (!bOK && !aMsg.IsEmpty())
		message = aMsg;

	return bOK;
}

//----------------------------------------------------------------------------
BOOL CDataSynchronizerWrapper::ValidateOutbound(RICheckNode* pProviderNode, BOOL bCheckFK, BOOL bCheckXSD, CString filters, CString& message)
{
	CString providerName = pProviderNode->GetName();
	if (providerName.IsEmpty())
		return TRUE;
	
	DataStr aMsg;
	DataBool b_CheckFK	= bCheckFK;
	DataBool b_CheckXSD	= bCheckXSD;
	DataStr strFilters	= filters;
	CFunctionDescription aFunctionDescription(_T("ValidateOutbound"));
	InitFunction(aFunctionDescription);

	pProviderNode->m_xmlValidationFilters = strFilters;

	aFunctionDescription.AddStrParam(_T("authenticationToken"), AfxGetAuthenticationToken());
	aFunctionDescription.AddStrParam(_T("providerName"),		providerName);
	aFunctionDescription.AddParam	(_T("bCheckFK"),			&b_CheckFK);
	aFunctionDescription.AddParam	(_T("bCheckXSD"),			&b_CheckXSD);
	aFunctionDescription.AddStrParam(_T("filters"),				strFilters);
	aFunctionDescription.AddStrParam(_T("serializedTree"),		pProviderNode->Serialize());
	aFunctionDescription.AddIntParam(_T("workerId"),			AfxGetWorkerId());
	aFunctionDescription.AddOutParam(_T("message"),				&aMsg);
	
	aFunctionDescription.SetReturnValueDescription(CDataObjDescription(_T(""), DataType::Bool, CDataObjDescription::_OUT));

	if(!InvokeWCFFunction(&aFunctionDescription, FALSE))
	{
		ASSERT(FALSE);
		return FALSE;
	}
	
	DataBool* pdbVal = (DataBool*)aFunctionDescription.GetReturnValue();

	BOOL bOK = (pdbVal ? *pdbVal : FALSE);
	if (!bOK && !aMsg.IsEmpty())
	    message =  aMsg;

	return bOK;
}

//----------------------------------------------------------------------------
BOOL CDataSynchronizerWrapper::CreateExternalServer
(
	const CString& providerName,
	const CString& servername,
	const CString& connstr,
		  CString& resultMsg
)
{
	DataStr aMsg;
	CFunctionDescription aFunctionDescription(_T("CreateExternalServer"));
	InitFunction(aFunctionDescription);

	aFunctionDescription.AddStrParam(_T("authenticationToken"), AfxGetAuthenticationToken());
	aFunctionDescription.AddStrParam(_T("providerName"),		providerName);
	aFunctionDescription.AddStrParam(_T("extservername"),		servername);
	aFunctionDescription.AddStrParam(_T("connstr"),				connstr);
	aFunctionDescription.AddOutParam(_T("message"),				&aMsg);
	
	aFunctionDescription.SetReturnValueDescription(CDataObjDescription(_T(""), DataType::Bool, CDataObjDescription::_OUT));

	if (!InvokeWCFFunction(&aFunctionDescription, FALSE))
	{
		resultMsg = _TB("Impossible to call the function CreateExternalServer of exchange data connector.");
		return FALSE;
	}

	DataBool* pdbVal = (DataBool*)aFunctionDescription.GetReturnValue();

	BOOL bOK = (pdbVal ? *pdbVal : FALSE);
	//if (!bOK && !aMsg.IsEmpty())
	resultMsg = aMsg;

	return bOK;
}

//----------------------------------------------------------------------------
BOOL CDataSynchronizerWrapper::CheckCompaniesToBeMapped
(
	const CString& providerName,
		  CString& strCompanyList,
		  CString& strErrMsg
)
{
	DataStr aMsg;
	DataStr aCompanyList;
	CFunctionDescription aFunctionDescription(_T("CheckCompaniesToBeMapped"));
	InitFunction(aFunctionDescription);

	aFunctionDescription.AddStrParam(_T("authenticationToken"), AfxGetAuthenticationToken());
	aFunctionDescription.AddStrParam(_T("providerName"),		providerName);
	aFunctionDescription.AddOutParam(_T("companylist"),			&aCompanyList);
	aFunctionDescription.AddOutParam(_T("message"),				&aMsg);

	aFunctionDescription.SetReturnValueDescription(CDataObjDescription(_T(""), DataType::Bool, CDataObjDescription::_OUT));

	if (!InvokeWCFFunction(&aFunctionDescription, FALSE))
	{
		strErrMsg = _TB("Impossible to call the function CheckMappedCompany of exchange data connector.");
		return FALSE;
	}

	DataBool* pdbVal = (DataBool*)aFunctionDescription.GetReturnValue();

	BOOL bOK = (pdbVal ? *pdbVal : FALSE);
	if (!bOK && !aMsg.IsEmpty())
		strErrMsg = aMsg;

	if (bOK && !aCompanyList.IsEmpty())
		strCompanyList = aCompanyList;

	return bOK;
}

//----------------------------------------------------------------------------
BOOL CDataSynchronizerWrapper::MapCompany
(
	const CString& providerName,
	const CString& strAppReg,
	const int& strMagoCompany,
	const CString& strInfinityCompany,
	const CString& strTaxId,
	CString& resultMsg
)
{
	DataStr aMsg;
	DataStr aCompanyList;
	CFunctionDescription aFunctionDescription(_T("MapCompany"));
	InitFunction(aFunctionDescription);

	aFunctionDescription.AddStrParam(_T("authenticationToken"), AfxGetAuthenticationToken());
	aFunctionDescription.AddStrParam(_T("providerName"), providerName);
	aFunctionDescription.AddStrParam(_T("appreg"),	strAppReg);
	aFunctionDescription.AddIntParam(_T("magocompany"), strMagoCompany);
	aFunctionDescription.AddStrParam(_T("infinitycompany"), strInfinityCompany);
	aFunctionDescription.AddStrParam(_T("taxid"), strTaxId);
	aFunctionDescription.AddOutParam(_T("message"), &aMsg);

	aFunctionDescription.SetReturnValueDescription(CDataObjDescription(_T(""), DataType::Bool, CDataObjDescription::_OUT));

	if (!InvokeWCFFunction(&aFunctionDescription, FALSE))
	{
		resultMsg = _TB("Impossible to call the function MapCompany of exchange data connector.");
		return FALSE;
	}

	DataBool* pdbVal = (DataBool*)aFunctionDescription.GetReturnValue();

	BOOL bOK = (pdbVal ? *pdbVal : FALSE);

	resultMsg = aMsg;

	return bOK;
}

//----------------------------------------------------------------------------
BOOL CDataSynchronizerWrapper::UploadActionPackage
(
	const CString& providerName,
	const CString& strActionPath,
	CString& resultMsg
)
{
	DataStr aMsg;
	DataStr aCompanyList;
	CFunctionDescription aFunctionDescription(_T("UploadActionPackage"));
	InitFunction(aFunctionDescription);

	aFunctionDescription.AddStrParam(_T("authenticationToken"), AfxGetAuthenticationToken());
	aFunctionDescription.AddStrParam(_T("providerName"), providerName);
	aFunctionDescription.AddStrParam(_T("actionpath"), strActionPath);
	aFunctionDescription.AddOutParam(_T("message"), &aMsg);

	aFunctionDescription.SetReturnValueDescription(CDataObjDescription(_T(""), DataType::Bool, CDataObjDescription::_OUT));

	if (!InvokeWCFFunction(&aFunctionDescription, FALSE))
	{
		resultMsg = _TB("Impossible to call the function UploadActionPackage of exchange data connector.");
		return FALSE;
	}

	DataBool* pdbVal = (DataBool*)aFunctionDescription.GetReturnValue();

	BOOL bOK = (pdbVal ? *pdbVal : FALSE);

	resultMsg = aMsg;

	return bOK;
}

//----------------------------------------------------------------------------
BOOL CDataSynchronizerWrapper::SetConvergenceCriteria
(
	const CString& providerName,
	const CString& xmlCriteria,
	CString& strErrMsg
)
{
	DataStr aMsg;
	CFunctionDescription aFunctionDescription(_T("SetConvergenceCriteria"));
	InitFunction(aFunctionDescription);

	aFunctionDescription.AddStrParam(_T("authenticationToken"), AfxGetAuthenticationToken());
	aFunctionDescription.AddStrParam(_T("providerName"), providerName);
	aFunctionDescription.AddStrParam(_T("xmlcriteria"), xmlCriteria);
	aFunctionDescription.AddOutParam(_T("message"), &aMsg);

	aFunctionDescription.SetReturnValueDescription(CDataObjDescription(_T(""), DataType::Bool, CDataObjDescription::_OUT));

	if (!InvokeWCFFunction(&aFunctionDescription, FALSE))
	{
		strErrMsg = _TB("Impossible to call the function SetConvergenceCriteria of exchange data connector.");
		return FALSE;
	}

	DataBool* pdbVal = (DataBool*)aFunctionDescription.GetReturnValue();

	BOOL bOK = (pdbVal ? *pdbVal : FALSE);
	if (!aMsg.IsEmpty())
		strErrMsg = aMsg;

	return bOK;
}

//----------------------------------------------------------------------------
BOOL CDataSynchronizerWrapper::SetGadgetPerm(
	const CString& providerName,
	CString& strErrMsg
)
{
		DataStr aMsg;

		CFunctionDescription aFunctionDescription(_T("SetGadgetPerm"));
		InitFunction(aFunctionDescription);
		aFunctionDescription.AddStrParam(_T("authenticationToken"), AfxGetAuthenticationToken());
		aFunctionDescription.AddStrParam(_T("providerName"), providerName);
		aFunctionDescription.AddOutParam(_T("message"), &aMsg);

		aFunctionDescription.SetReturnValueDescription(CDataObjDescription(_T(""), DataType::Bool, CDataObjDescription::_OUT));

		if (!InvokeWCFFunction(&aFunctionDescription, FALSE))
		{
			strErrMsg = _TB("Impossible to call the function SetGadgetPerm of exchange data connector.");
			return FALSE;
		}

		DataBool* pdbVal = (DataBool*)aFunctionDescription.GetReturnValue();

		DataBool bOK = (pdbVal ? *pdbVal : FALSE);
		
		strErrMsg = aMsg;
		return bOK;
}

//----------------------------------------------------------------------------
BOOL CDataSynchronizerWrapper::PurgeSynchroConnectorLog()
{
	CFunctionDescription aFunctionDescription(_T("PurgeSynchroConnectorLog"));
	InitFunction(aFunctionDescription);
	aFunctionDescription.AddIntParam(_T("companyId"), AfxGetLoginInfos()->m_nCompanyId);

	aFunctionDescription.SetReturnValueDescription(CDataObjDescription(_T(""), DataType::Bool, CDataObjDescription::_OUT));

	if (!InvokeWCFFunction(&aFunctionDescription, FALSE))
	{
		return FALSE;
	}

	DataBool* pdbVal = (DataBool*)aFunctionDescription.GetReturnValue();

	DataBool bOK = (pdbVal ? *pdbVal : FALSE);

	return bOK;
}

//----------------------------------------------------------------------------
BOOL CDataSynchronizerWrapper::CheckVersion
(
	const CString& providerName,
	CString& strMagoVersion,
	CString& strErrMsg
)
{
	DataStr aMsg;
	DataStr aCompanyList;
	CFunctionDescription aFunctionDescription(_T("CheckVersion"));
	InitFunction(aFunctionDescription);

	aFunctionDescription.AddStrParam(_T("authenticationToken"), AfxGetAuthenticationToken());
	aFunctionDescription.AddStrParam(_T("providerName"), providerName);
	aFunctionDescription.AddStrParam(_T("magoVersion"), strMagoVersion);
	aFunctionDescription.AddOutParam(_T("message"), &aMsg);

	aFunctionDescription.SetReturnValueDescription(CDataObjDescription(_T(""), DataType::Bool, CDataObjDescription::_OUT));

	if (!InvokeWCFFunction(&aFunctionDescription, FALSE))
	{
		strErrMsg = _TB("Impossible to call the function CheckVersion of exchange data connector.");
		return FALSE;
	}

	DataBool* pdbVal = (DataBool*)aFunctionDescription.GetReturnValue();

	BOOL bOK = (pdbVal ? *pdbVal : FALSE);
	if (!aMsg.IsEmpty())
		strErrMsg = aMsg;

	return bOK;
}

//----------------------------------------------------------------------------
BOOL CDataSynchronizerWrapper::GetConvergenceCriteria
(
	const CString& providerName,
	const CString& actionName,
	CString& xmlCriteria,
	CString& strErrMsg
	)
{
	DataStr aMsg;
	DataStr aXmlCriteria;

	CFunctionDescription aFunctionDescription(_T("GetConvergenceCriteria"));
	InitFunction(aFunctionDescription);

	aFunctionDescription.AddStrParam(_T("authenticationToken"), AfxGetAuthenticationToken());
	aFunctionDescription.AddStrParam(_T("providerName"), providerName);
	aFunctionDescription.AddStrParam(_T("actionName"), actionName);
	aFunctionDescription.AddOutParam(_T("xmlCriteria"), &aXmlCriteria);
	aFunctionDescription.AddOutParam(_T("message"), &aMsg);

	aFunctionDescription.SetReturnValueDescription(CDataObjDescription(_T(""), DataType::Bool, CDataObjDescription::_OUT));

	if (!InvokeWCFFunction(&aFunctionDescription, FALSE))
	{
		strErrMsg = _TB("Impossible to call the function GetConvergenceCriteria of Exchange data connector.");
		return FALSE;
	}


	DataBool* pdbVal = (DataBool*)aFunctionDescription.GetReturnValue();

	DataBool bOK = (pdbVal ? *pdbVal : FALSE);
	if (!bOK && !aMsg.IsEmpty())
		strErrMsg = aMsg;
	if (bOK)
		xmlCriteria = aXmlCriteria;

	return bOK;
}

//----------------------------------------------------------------------------
BOOL CDataSynchronizerWrapper::PauseResume(const CString& providerName, BOOL bPause)
{
	DataBool PbPause = bPause;
	CFunctionDescription aFunctionDescription(_T("PauseResume"));
	InitFunction(aFunctionDescription);

	aFunctionDescription.AddStrParam(_T("authenticationToken"), AfxGetAuthenticationToken());
	aFunctionDescription.AddStrParam(_T("providerName"), providerName);
	aFunctionDescription.AddParam(_T("bPause"), &PbPause);

	aFunctionDescription.SetReturnValueDescription(CDataObjDescription(_T(""), DataType::Bool, CDataObjDescription::_OUT));

	if (!InvokeWCFFunction(&aFunctionDescription, FALSE))
	{
		ASSERT(FALSE);
		return FALSE;
	}

	DataBool* pdbVal = (DataBool*)aFunctionDescription.GetReturnValue();
	return (pdbVal ? *pdbVal : FALSE);
}


//----------------------------------------------------------------------------
BOOL CDataSynchronizerWrapper::MassiveAbort(const CString& providerName)
{

	CFunctionDescription aFunctionDescription(_T("MassiveAbort"));
	InitFunction(aFunctionDescription);

	aFunctionDescription.AddStrParam(_T("authenticationToken"), AfxGetAuthenticationToken());
	aFunctionDescription.AddStrParam(_T("providerName"), providerName);

	aFunctionDescription.SetReturnValueDescription(CDataObjDescription(_T(""), DataType::Bool, CDataObjDescription::_OUT));

	if (!InvokeWCFFunction(&aFunctionDescription, FALSE))
	{
		ASSERT(FALSE);
		return FALSE;
	}

	DataBool* pdbVal = (DataBool*)aFunctionDescription.GetReturnValue();
	return (pdbVal ? *pdbVal : FALSE);
}

//----------------------------------------------------------------------------
CString CDataSynchronizerWrapper::GetRuntimeFlows(const CString& providerName) 
{
	CFunctionDescription aFunctionDescription(_T("GetRuntimeFlows"));
	InitFunction(aFunctionDescription);
	aFunctionDescription.AddStrParam(_T("authenticationToken"), AfxGetAuthenticationToken());
	aFunctionDescription.AddStrParam(_T("providerName"), providerName);

	aFunctionDescription.SetReturnValueDescription(CDataObjDescription(_T(""), DataType::String, CDataObjDescription::_OUT));

	if (!InvokeWCFFunction(&aFunctionDescription, FALSE))
	{
		ASSERT(FALSE);
		return _T("");
	}

	DataStr * pFlows = (DataStr*)aFunctionDescription.GetReturnValue();
	return (pFlows ? *pFlows : _T(""));
}

//----------------------------------------------------------------------------
CString CDataSynchronizerWrapper::GetLogsByNamespace(const CString& providerName, const CString& strNamespace,DataBool& bOnlyError)
{
	CFunctionDescription aFunctionDescription(_T("GetLogsByNamespace"));
	InitFunction(aFunctionDescription);
	aFunctionDescription.AddStrParam(_T("authenticationToken"), AfxGetAuthenticationToken());
	aFunctionDescription.AddStrParam(_T("providerName"), providerName);
	aFunctionDescription.AddStrParam(_T("strNamespace"), strNamespace);
	aFunctionDescription.AddParam(_T("bOnlyError"), &bOnlyError);

	aFunctionDescription.SetReturnValueDescription(CDataObjDescription(_T(""), DataType::String, CDataObjDescription::_OUT));

	if (!InvokeWCFFunction(&aFunctionDescription, FALSE))
	{
		ASSERT(FALSE);
		return _T("");
	}

	DataStr * pFlows = (DataStr*)aFunctionDescription.GetReturnValue();
	return (pFlows ? *pFlows : _T(""));
}

//----------------------------------------------------------------------------
CString CDataSynchronizerWrapper::GetLogsByDocId(const CString& providerName, const CString& TbDocGuid)
{
	CFunctionDescription aFunctionDescription(_T("GetLogsByDocId"));
	InitFunction(aFunctionDescription);
	aFunctionDescription.AddStrParam(_T("authenticationToken"), AfxGetAuthenticationToken());
	aFunctionDescription.AddStrParam(_T("providerName"), providerName);
	aFunctionDescription.AddStrParam(_T("TbDocGuid"), TbDocGuid);

	aFunctionDescription.SetReturnValueDescription(CDataObjDescription(_T(""), DataType::String, CDataObjDescription::_OUT));

	if (!InvokeWCFFunction(&aFunctionDescription, FALSE))
	{
		ASSERT(FALSE);
		return _T("");
	}

	DataStr * pFlows = (DataStr*)aFunctionDescription.GetReturnValue();
	return (pFlows ? *pFlows : _T(""));
}


//----------------------------------------------------------------------------
CString CDataSynchronizerWrapper::GetMassiveSynchroLogs(const CString& providerName, const CString& bfromDelta, const CString& bOnlyError)
{
	CFunctionDescription aFunctionDescription(_T("GetMassiveSynchroLogs"));
	InitFunction(aFunctionDescription);
	aFunctionDescription.AddStrParam(_T("authenticationToken"), AfxGetAuthenticationToken());
	aFunctionDescription.AddStrParam(_T("providerName"), providerName);
	aFunctionDescription.AddStrParam(_T("fromDelta"), bfromDelta);
	aFunctionDescription.AddStrParam(_T("OnlyError"), bOnlyError);

	aFunctionDescription.SetReturnValueDescription(CDataObjDescription(_T(""), DataType::String, CDataObjDescription::_OUT));

	if (!InvokeWCFFunction(&aFunctionDescription, FALSE))
	{
		ASSERT(FALSE);
		return _T("");
	}

	DataStr * pFlows = (DataStr*)aFunctionDescription.GetReturnValue();
	return (pFlows ? *pFlows : _T(""));
}

//----------------------------------------------------------------------------
BOOL CDataSynchronizerWrapper::ImagoStudioRuntimeInstalled()
{

	CFunctionDescription aFunctionDescription(_T("ImagoStudioRuntimeInstalled"));
	InitFunction(aFunctionDescription);

	aFunctionDescription.AddStrParam(_T("authenticationToken"), AfxGetAuthenticationToken());

	aFunctionDescription.SetReturnValueDescription(CDataObjDescription(_T(""), DataType::Bool, CDataObjDescription::_OUT));

	if (!InvokeWCFFunction(&aFunctionDescription, FALSE))
	{
		ASSERT(FALSE);
		return FALSE;
	}

	DataBool* pdbVal = (DataBool*)aFunctionDescription.GetReturnValue();
	return (pdbVal ? *pdbVal : FALSE);
}

//----------------------------------------------------------------------------
BOOL CDataSynchronizerWrapper::IsAlive()
{

	CFunctionDescription aFunctionDescription(_T("IsAlive"));
	InitFunction(aFunctionDescription);

	//aFunctionDescription.AddStrParam(_T("authenticationToken"), AfxGetAuthenticationToken());

	aFunctionDescription.SetReturnValueDescription(CDataObjDescription(_T(""), DataType::Bool, CDataObjDescription::_OUT));

	if (!InvokeWCFFunction(&aFunctionDescription, FALSE))
	{
		ASSERT(FALSE);
		return FALSE;
	}

	DataBool* pdbVal = (DataBool*)aFunctionDescription.GetReturnValue();
	return (pdbVal ? *pdbVal : FALSE);
}



//----------------------------------------------------------------------------
CString		CDataSynchronizerWrapper::GetSynchroLogsByFilters(
	const CString& providerName,
	const CString& strNamespace,
	DataBool& FromDelta,
	DataBool& FromBatch,
	DataBool& AllStatus,
	DataBool& Status,
	DataBool& AllDate,
	DataDate& FromDate,
	DataDate& ToDate,
	DataDate& SynchDate,
	const CString&  FlowName,
	DataInt&  Offset
)
{
	CFunctionDescription aFunctionDescription(_T("GetSynchroLogsByFilters"));
	InitFunction(aFunctionDescription);
	aFunctionDescription.AddStrParam(_T("authenticationToken"), AfxGetAuthenticationToken());
	aFunctionDescription.AddStrParam(_T("providerName"), providerName);
	aFunctionDescription.AddStrParam(_T("Namespace"), strNamespace);
	aFunctionDescription.AddParam(_T("FromDelta"), &FromDelta);
	aFunctionDescription.AddParam(_T("FromBatch"), &FromBatch);
	aFunctionDescription.AddParam(_T("AllStatus"), &AllStatus);
	aFunctionDescription.AddParam(_T("Status"), &Status);
	aFunctionDescription.AddParam(_T("AllDate"), &AllDate);
	aFunctionDescription.AddParam(_T("FromDate"), &FromDate);
	aFunctionDescription.AddParam(_T("ToDate"), &ToDate);
	aFunctionDescription.AddParam(_T("SynchDate"), &SynchDate);
	aFunctionDescription.AddStrParam(_T("FlowName"), FlowName);
	aFunctionDescription.AddParam(_T("Offset"), &Offset);

	aFunctionDescription.SetReturnValueDescription(CDataObjDescription(_T(""), DataType::String, CDataObjDescription::_OUT));

	if (!InvokeWCFFunction(&aFunctionDescription, FALSE))
	{
		ASSERT(FALSE);
		return _T("");
	}

	DataStr * pFlows = (DataStr*)aFunctionDescription.GetReturnValue();
	return (pFlows ? *pFlows : _T(""));
}


//----------------------------------------------------------------------------
BOOL CDataSynchronizerWrapper::NeedMassiveSynchro(const CString& strProviderName)
{

	CFunctionDescription aFunctionDescription(_T("NeedMassiveSynchro"));
	InitFunction(aFunctionDescription);

	aFunctionDescription.AddStrParam(_T("authenticationToken"), AfxGetAuthenticationToken());
	aFunctionDescription.AddStrParam(_T("providerName"), strProviderName);
	aFunctionDescription.SetReturnValueDescription(CDataObjDescription(_T(""), DataType::Bool, CDataObjDescription::_OUT));

	if (!InvokeWCFFunction(&aFunctionDescription, FALSE))
	{
		ASSERT(FALSE);
		return FALSE;
	}

	DataBool* pdbVal = (DataBool*)aFunctionDescription.GetReturnValue();
	return (pdbVal ? *pdbVal : FALSE);
}

//----------------------------------------------------------------------------
CString CDataSynchronizerWrapper::GetLogsByNamespaceDelta(const CString& providerName, const CString& strNamespace, DataBool& bOnlyError, DataBool& bDelta)
{
	CFunctionDescription aFunctionDescription(_T("GetLogsByNamespaceDelta"));
	InitFunction(aFunctionDescription);
	aFunctionDescription.AddStrParam(_T("authenticationToken"), AfxGetAuthenticationToken());
	aFunctionDescription.AddStrParam(_T("providerName"), providerName);
	aFunctionDescription.AddStrParam(_T("strNamespace"), strNamespace);
	aFunctionDescription.AddParam(_T("bOnlyError"), &bOnlyError);
	aFunctionDescription.AddParam(_T("bDelta"), &bDelta);
	aFunctionDescription.SetReturnValueDescription(CDataObjDescription(_T(""), DataType::String, CDataObjDescription::_OUT));

	if (!InvokeWCFFunction(&aFunctionDescription, FALSE))
	{
		ASSERT(FALSE);
		return _T("");
	}

	DataStr * pFlows = (DataStr*)aFunctionDescription.GetReturnValue();
	return (pFlows ? *pFlows : _T(""));
}

//----------------------------------------------------------------------------
BOOL CDataSynchronizerWrapper::IsActionQueued(const CString& strRequestGuid)
{
	CFunctionDescription aFunctionDescription(_T("IsActionQueued"));
	InitFunction(aFunctionDescription);
	aFunctionDescription.AddStrParam(_T("authenticationToken"), AfxGetAuthenticationToken());
	aFunctionDescription.AddStrParam(_T("requestGuid"), strRequestGuid);
	aFunctionDescription.SetReturnValueDescription(CDataObjDescription(_T(""), DataType::Bool, CDataObjDescription::_OUT));

	if (!InvokeWCFFunction(&aFunctionDescription, FALSE))
	{
		ASSERT(FALSE);
		return FALSE;
	}

	DataBool* pdbVal = (DataBool*)aFunctionDescription.GetReturnValue();
	return (pdbVal ? *pdbVal : FALSE);
}


//----------------------------------------------------------------------------
BOOL CDataSynchronizerWrapper::IsActionRunning(const CString& strRequestGuid)
{
	CFunctionDescription aFunctionDescription(_T("IsActionRunning"));
	InitFunction(aFunctionDescription);
	aFunctionDescription.AddStrParam(_T("authenticationToken"), AfxGetAuthenticationToken());
	aFunctionDescription.AddStrParam(_T("requestGuid"), strRequestGuid);
	aFunctionDescription.SetReturnValueDescription(CDataObjDescription(_T(""), DataType::Bool, CDataObjDescription::_OUT));

	if (!InvokeWCFFunction(&aFunctionDescription, FALSE))
	{
		ASSERT(FALSE);
		return FALSE;
	}

	DataBool* pdbVal = (DataBool*)aFunctionDescription.GetReturnValue();
	return (pdbVal ? *pdbVal : FALSE);
}

//----------------------------------------------------------------------------
BOOL CDataSynchronizerWrapper::UpdateUserMapping(const CString& windowsUsername, const CString& computerName)
{
	CFunctionDescription aFunctionDescription(_T("UpdateUserMapping"));
	InitFunction(aFunctionDescription);
	aFunctionDescription.AddStrParam(_T("authenticationToken"), AfxGetAuthenticationToken());
	aFunctionDescription.AddStrParam(_T("windowsUsername"), windowsUsername);
	aFunctionDescription.AddStrParam(_T("computerName"), computerName);
	aFunctionDescription.SetReturnValueDescription(CDataObjDescription(_T(""), DataType::Bool, CDataObjDescription::_OUT));
	if (!InvokeWCFFunction(&aFunctionDescription, FALSE))
	{
		ASSERT(FALSE);
		return FALSE;
	}

	DataBool* pdbVal = (DataBool*)aFunctionDescription.GetReturnValue();
	return (pdbVal ? *pdbVal : FALSE);
}