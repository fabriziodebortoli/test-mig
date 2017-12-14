#include "stdafx.h" 

#include <TbGenlib\baseapp.h>
#include <TbGenlib\messages.h>
#include <TbGes\extdoc.h>
#include <TbOleDb\InterfaceMacros.h>
#include <TbOleDb\SqlConnect.h>
#include "BDPostaliteDocumentsGraphicNavigation.h"
#include "UIPostaliteDocumentsGraphicNavigation.h"
#include "PostaliteAccountManagementDlg.h"
#include "PostaLiteTables.h"
#include "CIMagoMailConnector.h"
#include "CMailConnector.h"

////////////////////////////////////////////////////////////////////////////
//				INIZIO definizione della interfaccia di Add-On
/////////////////////////////////////////////////////////////////////////////
BOOL DsnChanged(FunctionDataInterface* pRDI)
{
	if (!AfxGetLoginContext())
		return TRUE;

	CTBNamespace ns(_NS_MOD("Module.Extensions.TbMailer"));

	DataObj* pSetting = AfxGetSettingValue(ns, _T("MailServer"), _T("UseMago"), DataBool(TRUE));
	BOOL useMago = pSetting ? *((DataBool*)pSetting) : TRUE;

	if (AfxIsActivated(MAGONET_APP, _NS_ACT("IMago")) && AfxGetLoginContext()->GetLoginInfos()->m_bDataSynchro && useMago == FALSE)
		(new CIMagoMailConnector())->AttachToApplicationContext();
	else
		(new CMailConnector())->AttachToApplicationContext();

	return TRUE;
}
/////////////////////////////////////////////////////////////////////////////
//				AddOn-Interface declaration
/////////////////////////////////////////////////////////////////////////////
//
//-----------------------------------------------------------------------------
BEGIN_ADDON_INTERFACE()

	DATABASE_RELEASE(400)

	BEGIN_TEMPLATE()

		BEGIN_DOCUMENT (_NS_DOC("PostaliteAccountManagementWizard"), TPL_SECURITY_PROTECTION)
			REGISTER_MASTER_TEMPLATE	(szDefaultViewMode,  	CPostaliteAccountManagementWizardDoc,	CPostaliteAccountManagementWizardFrame,	CPostaliteAccountManagementWizardView)
		    REGISTER_BKGROUND_TEMPLATE	(szBackgroundViewMode,	CPostaliteAccountManagementWizardDoc)
		END_DOCUMENT ()

		BEGIN_DOCUMENT (_NS_DOC("PostaliteDocumentsGraphicNavigation"), TPL_NO_PROTECTION)
			REGISTER_MASTER_TEMPLATE (szDefaultViewMode, BDPostaliteDocumentsGraphicNavigation, CPostaliteDocumentsGraphicNavigationFrame, UIPostaliteDocumentsGraphicNavigation)
		END_DOCUMENT ()

	END_TEMPLATE()

	//-----------------------------------------------------------------------------
	BEGIN_TABLES()
		BEGIN_REGISTER_TABLES	()
			REGISTER_TABLE	(TMsgQueue)
			REGISTER_TABLE  (TMsgLots)
		END_REGISTER_TABLES		()
	END_TABLES()

	BEGIN_FUNCTIONS()
		REGISTER_EVENT_HANDLER(_NS_EH("OnDSNChanged"), DsnChanged)
	END_FUNCTIONS()
END_ADDON_INTERFACE()
