#pragma once

#include <TbGeneric\DataObj.h>
#include "beginh.dex"

class CFunctionDescription;
class CLoginManagerInterface;
class CAbstractFormDoc;
class RICheckNode;

enum ActionType { INSERT_DOC, UPDATE_DOC, DELETE_DOC };

static const CString AUTHENTICATION_TOKEN = _T("{2E8164FA-7A8B-4352-B0DC-479984070222}");

//----------------------------------------------------------------------------
class TB_EXPORT CDataSynchronizerWrapper
{
private:
	const CString		m_strService;				// nome del WEB service (se esterno)
	const CString		m_strServiceNamespace;		// namespace del WEB service (se esterno)
	const CString		m_strServer;				// nome del server del WEB service (se esterno)
	const int			m_nWebServicesPort;			// numero di porta di IIS

public:
	CDataSynchronizerWrapper(
								const CString& strService, 
								const CString& strServiceNamespace, 
								const CString& strServer, 
								int nWebServicesPort
							);

	CDataSynchronizerWrapper(CDataSynchronizerWrapper* ds);

	~CDataSynchronizerWrapper(){}

public:
	void			InitFunction				(CFunctionDescription& aFunctionDescription) const;
	CString			GetActionsForDocument		(const CString& docNamepace, const CString& providerName);

	BOOL			Notify						(DataLng& logId, const CString& providerName, const CString& tableName, const CString& nameSpace, const CString& guidDoc, const CString& sOnlyForDMS, const CString& iMagoConfigurations);
	BOOL			SynchronizeOutbound			(const CString& providerName, const CString& xmlParametes = _T(""));
	BOOL			SynchronizeOutboundDelta	(const CString& providerName, const CString& startSynchroDate,const CString& xmlParametes = _T(""));
	BOOL			SynchronizeErrorsRecovery	(const CString&  providerName);

	BOOL			SetProviderParameters		(const CString& providerName, const CString& url, const CString& username, const CString&  password, BOOL  skipCrtValidation, const CString&  IAFModule, const CString&  parameters, CString&  message);
	BOOL			TestProviderParameters		(const CString& providerName, const CString& url, const CString& username, const CString&  password, BOOL skipCrtValidation, const CString& parameters, CString& strErrMsg, BOOL bDisabled);
	BOOL			IsMassiveSynchronizing		();
	BOOL			IsMassiveValidating			();
	BOOL			ValidateDocument			(const CString& providerName, const CString& nameSpace, const CString& tableName, const CString& guidDoc, const CString& serializedErrors, CString& message);
	BOOL			ValidateOutbound			(RICheckNode* pProviderNode, BOOL bCheckFK, BOOL bCheckXSD,  CString filtersExcluded, CString& message);
	BOOL			CreateExternalServer		(const CString& providerName, const CString& servername, const CString& connstr, CString& resultMsg);
	BOOL			CheckCompaniesToBeMapped	(const CString& providerName, CString& strCompanyList, CString& strErrMsg);
	BOOL			MapCompany					(const CString& providerName, const CString& strAppReg, const int& strMagoCompany, const CString& strInfinityCompany, const CString& strTaxId, CString& resultMsg);
	BOOL			UploadActionPackage			(const CString& providerName, const CString& strActionPath, CString& strErrMsg);
	BOOL			SetConvergenceCriteria		(const CString& providerName, const CString& xmlCriteria, CString& strErrMsg);
	BOOL			GetConvergenceCriteria		(const CString& providerName, const CString& actionName, CString& xmlCriteria, CString& strErrMsg);
	BOOL			SetGadgetPerm				(const CString& providerName, CString& strErrMsg);
	BOOL			PurgeSynchroConnectorLog	();
	BOOL			CheckVersion				(const CString& providerName, CString& strMagoVersion, CString& strErrMsg);
	BOOL			PauseResume					(const CString& providerName, BOOL bPause);
	BOOL			MassiveAbort				(const CString& providerName);
	CString			GetRuntimeFlows				(const CString& providerName);
	CString			GetLogsByNamespace			(const CString& providerName, const CString& strNamespace, DataBool& bOnlyError=DataBool(FALSE));
	CString			GetLogsByDocId				(const CString& providerName, const CString& TbDocGuid);
	CString			GetMassiveSynchroLogs		(const CString& providerName, const CString& bfromDelta, const CString& bOnlyError);
	CString			GetSynchroLogsByFilters		(
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
												);
	BOOL			ImagoStudioRuntimeInstalled	(); 
	BOOL			IsAlive						();

	BOOL			NeedMassiveSynchro			(const CString& providerName);
	CString			GetLogsByNamespaceDelta(const CString& providerName, const CString& strNamespace, DataBool& bOnlyError, DataBool& bDelta);

	BOOL			NotifyGuid					(DataLng& logId,const CString& providerName, const CString& tableName, const CString& nameSpace, const CString& guidDoc, const CString& sOnlyForDMS, const CString& iMagoConfigurations, CString& gRequestGuid);
	BOOL			IsActionQueued				(const CString& strRequestGuid);
	BOOL			SynchronizeErrorsRecoveryImago(const CString&  providerName, CString& strRequestGuid);
	BOOL			IsActionRunning				(const CString& strRequestGuid);
	BOOL			UpdateUserMapping			(const CString& windowsUsername, const CString& computerName);
};

#include "endh.dex"