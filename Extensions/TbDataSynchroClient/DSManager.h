#pragma once 

#include <TbNameSolver\TBResourceLocker.h>
#include <TbOleDb\TbExtensionsInterface.h>

#include "DSTables.h"

#include "DSMonitor.h"
#include "ValidationMonitorSettings.h"
#include "UIProviders.hjson"

class CDataSynchronizerWrapper;
class SqlSession;
class CSynchroDocInfoArray;

//includere alla fine degli include del .H
#include "beginh.dex"

class TDS_Providers;
class TUDS_Providers;
class RecordArray;


/////////////////////////////////////////////////////////////////////////////
// 				class CVersionControlTabDialog 
/////////////////////////////////////////////////////////////////////////////
//
//==============================================================================
//	CVersionControlTabDialog definition
//==============================================================================
//
//===========================================================================
class CVersionControlTabDialog : public CParsedDialog
{
	DECLARE_DYNAMIC(CVersionControlTabDialog)

	BOOL m_bForceFocusToParent;


public: // constructor
	CVersionControlTabDialog() {};
	virtual BOOL Create();

	DataStr		m_strErrorMsg;

	CStrStatic	m_CtrlErrorMsg;

	void SetErrorMsg(CString errMsg) { m_CtrlErrorMsg.SetValue(errMsg); m_CtrlErrorMsg.UpdateCtrlView(); }

private:
	virtual void PostNcDestroy();

protected:
	virtual BOOL	OnInitDialog();
	virtual void	OnCancel();
	virtual void	OnOK();

	DECLARE_MESSAGE_MAP()
};


//il file è presente nella path App/SynchroProvider/ProviderName/

//=========================================================================
//	class CXMLSynchroProviderInfo: metadati relativi al provider
//=========================================================================
//

//=========================================================================
//	class CSynchroProvider: informazioni sul singolo provider
//=========================================================================
//
class TB_EXPORT CSynchroProvider : public CObject
{
	//friend class CDataSynchroManager;

private:
	BOOL m_bIsValid;
	BOOL m_bNeedMassiveSynchro;

public:
	RecordArray*	m_pParameters;

	CString		m_strXMLParameters;
	CString		m_Name;
	CString		m_Description;	
	CString		m_Url;
	CString		m_Username;
	CString		m_Password;
	BOOL		m_SkipCrtValidation;
	
	CString		m_Application;
	CString		m_Functionality;

	DataBool	m_IsDMSProvider;
	DataStr		m_IAFModules;

	CSynchroProvider* m_pParentSynchroProvider; //vedi caso di dipendenza tra il DMSInfinity e il CRMInfinity (se attivato); normalmente è NULL	

	CMapStringToString	m_strImagoRequestGuid;

private:
	CSynchroDocInfoArray* m_pDocumentsToSynch;
	

public:
	CSynchroProvider();
	~CSynchroProvider();

public:
	BOOL IsActivated() const;
	BOOL IsValid() const { return m_bIsValid; }
	void SetValid(BOOL bSet) { m_bIsValid = bSet; }
	BOOL NeedMassiveSynchro() const { return m_bNeedMassiveSynchro; }
	void SetNeedMassiveSynchro(BOOL bSet) { m_bNeedMassiveSynchro = bSet; }	

	CSynchroDocInfoArray* GetDocumentsToSynch() const { return (m_pParentSynchroProvider) ? m_pParentSynchroProvider->GetDocumentsToSynch() : m_pDocumentsToSynch; }
	CSynchroDocInfoArray* GetDocumentsToSynchImago() const { return  m_pDocumentsToSynch; }
public:
	BOOL Parse(const CString& strSynchroProfilesFile, BOOL isXmlContent = FALSE);
	void ParseValuesFromXMLString(const CString xmlParamValue);		
	CString UnparseValuesToXMLString();
};

//=========================================================================
//	class CSynchroProviderArray: informazioni sul singolo provider
//=========================================================================
//
class TB_EXPORT CSynchroProviderArray : public Array
{
public:
	CSynchroProvider* GetAt(int nIdx) const { return (CSynchroProvider*)Array::GetAt(nIdx); }
	int Add(CSynchroProvider* pSynchroProvider) { return Array::Add(pSynchroProvider); }
	
	CSynchroProvider* GetProvider(const DataStr& providerName) const;
};

///////////////////////////////////////////////////////////////////////////////
//						CDataSynchroMng definition
///////////////////////////////////////////////////////////////////////////////
class TB_EXPORT CDataSynchroManager : public IDataSynchroManager, CTBLockable
{

private:
	CDataSynchronizerWrapper*	m_pDataSynchronizer;
	DECLARE_LOCKABLE(CSynchroProviderArray,		m_aSynchroProviders);
	CStringArray*				m_pDocuments;
	BOOL						m_Pause;
	CVersionControlTabDialog * m_pVersionDlg;
	BOOL					   m_bIsMagoRuntimeInstalled;
	BOOL					   m_bIsAlive;

public:
	CDataSynchroManager(); 
	virtual ~CDataSynchroManager();

private:
	void LoadSynchroProviders();
	CString GetRuntimeFlows(CString providerName);
	void CheckAndUpdateTBGuid(const CString& docNamespace, SqlSession* pSqlSession);
	BOOL CheckProvider(TDS_Providers* pProviderRec, CString& strErrMsg);
	void UpdateProviderRec(CSynchroProvider* pSynchroProvider, TUDS_Providers* pProviderTU);
	

public:
	const CSynchroProviderArray* GetSynchroProviders() const {return (const CSynchroProviderArray*) &m_aSynchroProviders; }

	BOOL    ReadPause					(); //{ return m_Pause; };
	void    ChangedPause				(BOOL m_PauseParam, const CString& providerName);// { m_pDataSynchronizer->PauseResume(providerName, m_PauseParam); m_Pause = m_PauseParam; };
	void	Abort						(const CString& providerName);
	BOOL	IsDocumentToSynchronize		(const CTBNamespace& docNamespace);	
	BOOL	SetProviderParameters		(const CString& providerName, const CString& providerUrl, const CString& providerUser, const CString& providerPwd, BOOL skipCrtValidation, const CString& IAFModule, const CString& parameters, CString& strErrMsg);
	BOOL	TestProviderParameters		(const CString& providerName, const CString& providerUrl, const CString& providerUser, const CString& providerPwd, BOOL skipCrtValidation, const CString& parameters, CString& strErrMsg, BOOL bDisabled = FALSE);
	BOOL	TestProviderParameters		(const CString& providerName, CString& strErrMsg);
	BOOL	SynchronizeErrorsRecovery	(const CString&  providerName);
	BOOL	IsMassiveSynchronizing		();
	BOOL	IsMassiveValidating			();
	// I.Mago
	BOOL	GetSynchroProvider			(const CString& providerName, CString& providerUrl, CString& providerUser, CString& providerPwd, CString& parameters);
	BOOL	CreateExternalServer		(const CString& providerName, const CString& exteservername, const CString& connstr, CString& strErrMsg);
	BOOL	CheckCompaniesToBeMapped	(const CString& providerName, CString& strCompanyList, CString& strErrMsg);
	BOOL	MapCompany					(const CString& providerName, const CString& strAppReg, const int& strMagoCompany, const CString& strInfinityCompany, const CString& strTaxId, CString& strErrMsg);
	BOOL	UploadActionPackage			(const CString& providerName, const CString& strActionPath, CString& strErrMsg);
	BOOL	SaveDataSynchProviderInfo	(const CString& providerName, const CString& providerUrl, const CString& providerUser, const CString& providerPwd, BOOL skipCrtValidation, const CString& parameters, const CString& iafmodules, CString& strErrMsg);
	BOOL	SetConvergenceCriteria		(const CString& providerName, const CString& xmlCriteria, CString& strErrMsg);
	BOOL	GetConvergenceCriteria		(const CString& providerName, const CString& actionName, CString& xmlCriteria, CString& strErrMsg);
	BOOL	GetSynchroProviderInfo		(const CString& providerName, CString& providerUrl, CString& providerUser, CString& providerPwd, CString& parameters, CString& iafmodules, BOOL& skipcrtvalid);
	BOOL	SetGadgetPerm				(const CString& providerName, CString& strErrMsg);
	BOOL	PurgeSynchroConnectorLog	();
	BOOL	CheckVersion				(const CString& providerName, CString& strMagoVersion, CString& strErrMsg);
	void	DestroyVersionDlg			();
	CString GetLogsByNamespace			(CString providerName, CString strNamespace, DataBool bOnlyError=FALSE);
	CString GetLogsByDocId				(CString providerName, CString TbDocGuid);
	CString	GetMassiveSynchroLogs		(CString providerName, CString bFromDelta, CString bOnlyErrors);
	CString GetSynchroLogsByFilters(
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
										const CString& FlowName,
										DataInt& Offset
									);
	CString GetLogsByNamespaceDelta		(CString providerName, CString strNamespace, DataBool bOnlyError = FALSE, DataBool bDelta = TRUE);
	
	BOOL	ImagoStudioRuntimeInstalled	() {return m_bIsMagoRuntimeInstalled;}
	
	BOOL	IsAlive();

	BOOL	SynchronizeErrorsRecoveryImago	(const CString& m_ProviderName, CString& m_strRecoveryGuid);
	BOOL	IsActionQueued					(const CString& m_strRecoveryGuid);
	BOOL	IsActionRunning					(const CString& m_strRecoveryGuid);
public: 
	virtual BOOL					NeedMassiveSynchro			(const CString& providerName) const;
	virtual BOOL					SynchronizeOutbound			(const CString& providerName, const CString& xmlParameters, CString& strMessage, DataDate& startSynchroDate, BOOL bDelta = FALSE);
	virtual BOOL					ValidateOutbound			(RICheckNode* pCheckerProviderNode, BOOL bCheckFK, BOOL bCheckXSD, CString filtersExcluded, CString& message);
	
	virtual IMassiveValidationSettings*	GetMassiveValidationSettings();

	virtual BOOL					SaveSynchroFilter(const CString& docNamespace, const CString& providerName, const CString& xmlParameters, CString& strMessage);
	virtual CString					GetSynchroFilter(const CString& docNamespace, const CString& providerName);
	
	virtual BOOL					IsValid()	const;	
	virtual BOOL					IsProviderValid(const CString& providerName)	const;
	virtual BOOL					IsProviderDisabled(const CString& providerName)	const;

public:
	virtual LPCSTR  GetObjectName() const { return "CDataSynchroManager"; }
};

//=========================================================================================
TB_EXPORT CDataSynchroManager*	AFXAPI AfxGetDataSynchroManager();

#include "endh.dex"