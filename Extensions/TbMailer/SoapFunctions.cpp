
#include "StdAfx.h"

#ifdef new
#undef new
#endif

#include <TBNameSolver/ApplicationContext.h>
#include <TbGenlibUI/SettingsTableManager.h>
#include <TbClientCore/ClientObjects.h>
#include <TbGeneric\SettingsTable.h>
#include "soapfunctions.h"

#include "CMailConnector.h"
#include "CIMagoMailConnector.h"

#include "SmtpParametersSections.h"
#include "SmtpConfigurationDlg.h"
#include "GeneralConfigurationDlg.h"


///////////////////////////////////////////////////////////////////////////////
#ifdef _DEBUG
#define new DEBUG_NEW
#endif

//----------------------------------------------------------------------------
///<summary>
/// Initialize Mail Connector extension
///</summary>
//[TBWebMethod(securityhidden=true, woorm_method=false)]
DataBool StartMailConnector()
{
	CTBNamespace ns(_NS_MOD("Module.Extensions.TbMailer"));

	DataObj* pSetting = AfxGetSettingValue(ns, _T("MailServer"), _T("UseMago"), DataBool(TRUE));
	BOOL useMago = pSetting ? *((DataBool*)pSetting) : TRUE;

 	if (AfxIsActivated(MAGONET_APP, _NS_ACT("IMago")) && AfxGetLoginContext()->GetLoginInfos()->m_bDataSynchro && useMago == FALSE)
		(new CIMagoMailConnector())->AttachToApplicationContext();
	else
		(new CMailConnector())->AttachToApplicationContext();
	return TRUE;
}
//----------------------------------------------------------------------------
///<summary>
/// SMTP Send Settings Configuration
///</summary>
//[TBWebMethod(woorm_method=false, defaultsecurityroles="aMailConnector Parameter Manager")]
DataBool  ConfigureSmtpParameter()
{
	CMailConfigurationDlg dlg;
	dlg.DoModal();
	return TRUE;
}

//----------------------------------------------------------------------------

///<summary>
/// Mail Settings Configuration
///</summary>
//[TBWebMethod(woorm_method=false, defaultsecurityroles="aMailConnector Parameter Manager")]
DataBool  ConfigureMailParameters()
{
	MailConnectorParams params;

	CGeneralConfigurationDlg dlg;

	dlg.m_sOutlookProfile			= params.GetOutlookProfile			();
	dlg.m_bSupportOutlookExpress	= params.GetSupportOutlookExpress	();
	dlg.m_bMailCompress				= params.GetMailCompress			();
	dlg.m_bCryptFile				= params.GetCryptFile				();
	dlg.m_nProtocol					= params.GetUseMapi() ? 0 : 1;
	dlg.m_sPassword					= params.GetPassword				();
	dlg.m_bRequestDeliveryNotification	= params.GetRequestDeliveryNotifications ();
	dlg.m_bRequestReadNotification		= params.GetRequestReadNotifications ();
	dlg.m_sReplyToAddress				= params.GetReplyToAddress ();
	dlg.m_sTrackingAddress				= params.GetTrackingAddressForSentEmails ();
	dlg.m_sPrinterTemplate				= params.GetPrinterTemplate();
	dlg.m_sFaxFormatTemplate			= params.GetFAXFormatTemplate();

	if (dlg.DoModal() == IDOK)
	{
		params.SetOutlookProfile			(dlg.m_sOutlookProfile);
		params.SetPassword					(dlg.m_sPassword);
		params.SetSupportOutlookExpress		(dlg.m_bSupportOutlookExpress);
		params.SetMailCompress				(dlg.m_bMailCompress);
		params.SetCryptFile					(dlg.m_bCryptFile);
		params.SetUseMapi					(dlg.m_nProtocol == 0);
		params.SetUseSmtp					(dlg.m_nProtocol == 1);
		params.SetRequestDeliveryNotifications	(dlg.m_bRequestDeliveryNotification);
		params.SetRequestReadNotifications		(dlg.m_bRequestReadNotification);
		params.SetReplyToAddress				(dlg.m_sReplyToAddress);
		params.SetTrackingAddressForSentEmails	(dlg.m_sTrackingAddress);
		params.SetPrinterTemplate				(dlg.m_sPrinterTemplate);
		params.SetFAXFormatTemplate				(dlg.m_sFaxFormatTemplate);

		AfxSaveSettingsFile(&params, TRUE);
	}

	return TRUE;
}

//=============================================================================
