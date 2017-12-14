#pragma once

#include <tbgeneric\array.h>

#include <TbWebServicesWrappers\LoginManagerInterface.h>

#include <TbNameSolver\TbNamespaces.h>
#include <TbNameSolver\MacroToRedifine.h>
#include <TbNameSolver\ApplicationContext.h>
#include "BeginH.dex"

class CServerConnectionInfo;
class CLockManagerInterface;
class CLoginManagerInterface;
class TbServicesWrapper;
class CClientObjects;
class CLockManagerInterface;

//============================================================================
// Global Functions 
//=============================================================================
TB_EXPORT void						AFXAPI AfxReinitActivationInfos();
TB_EXPORT CClientObjects*			AFXAPI AfxGetCommonClientObjects();

TB_EXPORT TbServicesWrapper*		AFXAPI AfxGetTbServices			();
TB_EXPORT CLockManagerInterface*	AFXAPI AfxGetLockManager		();
TB_EXPORT CLockManagerInterface*	AFXAPI AfxCreateLockManager		();
TB_EXPORT CLoginManagerInterface*	AFXAPI AfxGetLoginManager		();

//-----------------------------------------------------------------------------
TB_EXPORT BOOL						AFXAPI AfxIsActivated			(const CString& strApplication, const CString& strModuleOrFunctionality, BOOL bForceCall = FALSE);
TB_EXPORT BOOL						AFXAPI AfxIsCalAvailable		(const CString& strApplication, const CString& strModuleOrFunctionality);
TB_EXPORT BOOL						AFXAPI AfxIsValidDate			(const DataDate operationDate, DataDate& maxDate);
TB_EXPORT BOOL						AFXAPI AfxIsActivated			(const CTBNamespace& sLibraryNamespace);
TB_EXPORT BOOL						AFXAPI AfxSetCompanyInfo		(const CString& strName, const CString& strValue);

//----------------------------------------------------------------------------
class TB_EXPORT CClientObjects : public CObject
{
private:
	CServerConnectionInfo*				m_pServerConnectionInfo;
	
	TbServicesWrapper*					m_pTbServicesWrapper;
	CLoginManagerInterface*				m_pLoginManagerInterface;

	// activation structures
	const CMapStringToOb				m_ActivationInfos;

	void		InitActivationInfos();
public:
	void		ReinitActivationInfos();

public:
	CClientObjects();
	~CClientObjects();

	BOOL InitWebServicesConnections(const CString& strServer, const CString& strInstallation);

	//please use CLoginManagerInterface::InitLogin
TB_OLD_METHOD BOOL InitLogin(const CString& sAuthenticationToken) { return AfxGetLoginManager()->InitLogin(sAuthenticationToken); }

	// activation data
TB_OLD_METHOD void	InitActivationStateInfo		() { AfxGetLoginManager()->InitActivationStateInfo(); }
	void			GetActivationStateWarning	();
	const CString	GetActivationStateInfo		();
	
	BOOL IsActivated (const CString& strApplication, const CString& strModuleOrFunctionality);
	BOOL IsActivated (const CTBNamespace& aNamespace);
	void AddInActivationInfo	(const CString& sModule);
	void AddInActivationInfo	(const CStringArray& arModules);

public:
	const CServerConnectionInfo*	GetServerConnectionInfo		() const;
	
	TbServicesWrapper*				GetTbServices				()	{ return m_pTbServicesWrapper; }
	CLoginManagerInterface*			GetLoginManager				()	{ return m_pLoginManagerInterface; }
};


#define TBNET_APP					_NS_APP("Framework")
#define TBEXT_APP					_NS_APP("Extensions")
#define CLIENTNET_APP				_NS_APP("ClientNet")
#define TESTMANAGER_APP				_NS_APP("TestManager")

#define MAGONET_APP					_NS_APP("Erp")
#define MDC_APP						_NS_APP("MDC")
#define RETAIL_APP					_NS_APP("Retail")
#define PAINET_APP					_NS_APP("PaiNet")

#define OFM_APP						_NS_APP("OFM")
#define ACM_APP						_NS_APP("ACM")
#define FSM_APP						_NS_APP("FSM")

//moduli e funzionalità per controllo attivazione
#define XGATE_MOD					_NS_MOD("XGate")
#define MAGICPANE_MOD				_NS_MOD("MagicPane")
#define MAGICLINK_MOD				_NS_MOD("MagicDocumentsPlatform")

#define XENGINE_ACT					_NS_ACT("XEngine")
#define INTERACTIVE_FUNCTIONALITY	_NS_ACT("InteractiveFunctionality")
#define INTERACTIVE_IMP_EXP			_NS_ACT("InteractiveImportExport")

// DMS
#define EASYATTACHMENT_ACT			_NS_ACT("EasyAttachment")
#define EASYATTACHMENT_FULL_ACT		_NS_ACT("EasyAttachmentFull")
#define DMS_ACT						_NS_ACT("DMS")
#define DMS_SOS_ACT					_NS_ACT("DMSSOS")
#define DMS_MASSIVE_ACT				_NS_ACT("DMSMassive")
#define SOSCONNECTOR_FUNCTIONALITY	_NS_ACT("SOSConnectorFunctionality")
#define DMS_FILESYSTEMSTORAGE_FUNCTIONALITY _NS_ACT("FileSystemStorage")
//

// questo rappresenta il nome del modulo su file system ed anche implicitamente
// l'esistenza del runtime di EasyStudio visto che si trova nel sales module del TBF
#define EASYSTUDIO_MODULE_NAME		_NS_MOD("EasyBuilder")
#define EASYSTUDIO_DESIGNER_ACT		_NS_ACT("EasyStudioDesigner")

#define MAILCONNECTOR_ACT			_NS_ACT("MailConnector")
#define TBAUDITING_ACT				_NS_ACT("TBAuditing")
#define POSTALITE_ACT				_NS_ACT("PostaLite")
#define TBSECURITY_ACT				_NS_ACT("TbSecurity")
#define TBDATASYNCHRO_ACT			_NS_ACT("TBDataSynchroClient")
#define REPORTEDITOR_ACT			_NS_ACT("ReportEditor")
#define IMAGO_ACT					_NS_ACT("IMago")

#define INFINITECRM_FUNCTIONALITY	_NS_MOD("InfiniteCRM")
#define CRMINFINITY_FUNCTIONALITY	_NS_MOD("CRMInfinity")
#define DMSINFINITY_FUNCTIONALITY	_NS_MOD("DMSInfinity")

#define LICENZE_TERMINATE _TB("Licences for this functionality are all busy. Contact application administrator")
//=============================================================================

#include "EndH.dex"
