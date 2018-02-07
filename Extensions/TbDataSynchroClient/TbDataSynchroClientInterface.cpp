
#include "stdafx.h" 

#include <TbNameSolver\CompanyContext.h>
#include <TbGenlib\messages.h>
#include <TbGenlib\baseapp.h>
#include <TbOleDb\InterfaceMacros.h>
#include <TbOleDb\SqlConnect.h>
#include <TBGES\InterfaceMacros.h>
#include <TbGes\extdoc.h>
#include <TbGes\JsonFrame.h>

#include "DSManager.h"
#include "DSTables.h"
#include "CDNotification.h"
#include "UINotification.h"
#include "CDUpdateTBModifiedMaster.h"
#include "DProviders.h"
#include "UIProviders.h"
#include "DSMonitor.h"
#include "UIDSMonitor.h"
#include "DValidationMonitor.h"
#include "UIValidationMonitor.h"
#include "UINotification.hjson"
#include "UIValidationMonitor.hjson"
#include "DDSSettings.h"
#include "ModuleObjects\DSSettings\JsonForms\IDD_DS_COMPANYUSER_SETTINGS.hjson"

/////////////////////////////////////////////////////////////////////////////
//				AddOn-Interface declaration
/////////////////////////////////////////////////////////////////////////////
//---------------------------------------------------------------------------------
BOOL InitDataSynchronizeManager(FunctionDataInterface* pRDI)
{
	if (
			!AfxIsActivated(TBEXT_APP, _NS_ACT("DataSynchroFunctionality")) ||
			!AfxGetLoginInfos()->m_bDataSynchro
		)
		return TRUE;

	CCompanyContext* pContext = AfxGetCompanyContext();
	if (pContext && !pContext->GetDataSynchroManager())
		pContext->AttachDataSynchroManager(new CDataSynchroManager());
		
	return TRUE;
}

//---------------------------------------------------------------------------------
BEGIN_ADDON_INTERFACE()
	DATABASE_RELEASE(402)

	//-----------------------------------------------------------------------------
	BEGIN_TABLES()
		BEGIN_REGISTER_TABLES		()		
			REGISTER_TABLE			(TDS_ActionsLog)	
			REGISTER_TABLE			(TDS_Providers)
			REGISTER_TABLE			(TDS_SynchronizationInfo)
			REGISTER_TABLE			(TDS_AttachmentSynchroInfo)
			REGISTER_TABLE			(TDS_ValidationInfo)
			REGISTER_TABLE			(TDS_ValidationFKtoFix)
			REGISTER_VIRTUAL_TABLE	(VProviderParams)
			REGISTER_VIRTUAL_TABLE	(VSynchronizationInfo)
			REGISTER_VIRTUAL_TABLE	(VSynchroInfoDocSummary)
		END_REGISTER_TABLES()
	END_TABLES()
	
	//-----------------------------------------------------------------------------	
	BEGIN_TEMPLATE()
		BEGIN_DOCUMENT (_NS_DOC("Providers"), TPL_NO_PROTECTION)
			//REGISTER_MASTER_TEMPLATE	(szDefaultViewMode,	DProviders,			CMasterFrame,		CProvidersView)
			REGISTER_MASTER_JSON_TEMPLATE(szDefaultViewMode, DProviders, IDD_DS_PROVIDERS)
		END_DOCUMENT ()	

		BEGIN_DOCUMENT (_NS_DOC("DSMonitor"), TPL_NO_PROTECTION)
			REGISTER_MASTER_TEMPLATE	(szDefaultViewMode, DSMonitor,			CDSMonitorFrame,	CDSMonitorView)
		END_DOCUMENT ()	

		BEGIN_DOCUMENT (_NS_DOC("DValidationMonitor"), TPL_NO_PROTECTION)
			//REGISTER_MASTER_TEMPLATE	(szDefaultViewMode, DValidationMonitor, CValidationFrame,	CValidationMonitorView)
			REGISTER_MASTER_JSON_TEMPLATE(szDefaultViewMode, DValidationMonitor, IDD_DATAVALIDATION_MONITOR)
		END_DOCUMENT ()	
		BEGIN_DOCUMENT(_NS_DOC("DSSettings"), TPL_ADMIN_PROTECTION)
			REGISTER_MASTER_JSON_TEMPLATE(szDefaultViewMode, DDSSettings, IDD_DS_COMPANYUSER_SETTINGS)
		END_DOCUMENT()			
	END_TEMPLATE()
	

	//-----------------------------------------------------------------------------
	BEGIN_FAMILY_CLIENT_DOC()

		WHEN_FAMILY_SERVER_DOC(CAbstractFormDoc)
			EXCLUDE_DOC_FROM_ATTACH_WHEN(!AfxDataSynchronizeEnabled())
			ATTACH_FAMILY_CLIENT_DOC(CDUpdateTBModifiedMaster, _NS_CD("CDUpdateTBModifiedMaster"))
	
			EXCLUDE_DOC_FROM_ATTACH_WHEN(!AfxDataSynchronizeEnabled() || !AfxGetDataSynchroManager()->IsDocumentToSynchronize(pDoc->GetNamespace()))
			ATTACH_FAMILY_CLIENT_DOC(CDNotification, _NS_CD("CDNotification"))
		END_FAMILY_SERVER_DOC()
	
	END_FAMILY_CLIENT_DOC()

	//-----------------------------------------------------------------------------
	BEGIN_FUNCTIONS()
		REGISTER_EVENT_HANDLER	(_NS_EH("OnDSNChanged"),	InitDataSynchronizeManager)
	END_FUNCTIONS()

	//manage json controls
	BEGIN_REGISTER_CONTROLS()
		REGISTER_JSON_CONTROL(IDD_DATAVALIDATION_MONITOR_VIEW,		CValidationMonitorView)
		REGISTER_JSON_CONTROL(IDD_DATAVALIDATION_MONITOR_FK_TO_FIX, CValidationMonitorFKToFixPanel)
		//REGISTER_JSON_CONTROL(IDD_DATASYNCHRO_DOCK_PANE, CTaskBuilderDockPane)
		REGISTER_JSON_CONTROL(IDD_DATASYNCHRO_VIEW, CDataSynchroClientView)
		REGISTER_COMBODROPDOWNLIST_CONTROL(CSynchroProviderCombo,		_T("SynchroProviders"),			CSynchroProviderCombo,		FALSE)
		REGISTER_COMBODROPDOWNLIST_CONTROL(CDocumentsToSynchCombo,		_T("DocumentsToSynch"),			CDocumentsToSynchCombo,		FALSE)
		REGISTER_COMBODROPDOWNLIST_CONTROL(CDocumentsToValidateCombo,	_T("DocumentsToValidate"),		CDocumentsToValidateCombo,	FALSE)
	END_REGISTER_CONTROLS()

END_ADDON_INTERFACE()