#include "stdafx.h"
#ifdef new
#undef new
#endif

#include <stdio.h>
#include <string.h>
#include <direct.h>
#include <locale.h> 

#include <winsock2.h>
#include <windows.h>
#include <iostream>

#pragma comment(lib,"ws2_32.lib")
using namespace std;

#include <TbNameSolver\chars.h>
#include <TbGeneric\ParametersSections.h>
#include <TbGeneric\LocalizableObjs.h>
#include <TbGeneric\Array.h>
#include <TbGeneric\GeneralFunctions.h>
#include <TbGeneric\LineFile.h>

#include <TbGenlib\Messages.h>

#include <TbGenlibManaged\SmtpNet.h>

#include <TbClientCore\ClientObjects.h>

#include <TbOleDb\sqltable.h>

#include "CMapiSession.h"
#include "CMailConnector.h"
#include "CIMagoMailConnector.h"

#include "Email.h"
#include "PostaLiteSettings.h"
#include "PostaLiteTables.h"

#include <TbGenlibManaged\PostaLiteNet.h>
#include <TbGenlibManaged\MIMagoMailConnector.h>

#include "tbmailer.hjson" //JSON AUTOMATIC UPDATE
#include <TbNameSolver\JsonSerializer.h>
#include <Tbgeneric\CollateCultureFunctions.h>
#include <TbGenlib\CEFClasses.h>

//#include "TBHttpModule.h"
//#include "TBAppLoader.h"
#ifdef _DEBUG
#define new DEBUG_NEW

#undef THIS_FILE
static const char THIS_FILE[] = __FILE__;
#endif

//=============================================================================
CIMagoMailConnector::~CIMagoMailConnector()
{
}

//-----------------------------------------------------------------------------
IMapiSession* CIMagoMailConnector::NewMapiSession(BOOL bMultiThreaded /*= TRUE*/, BOOL bNoLogonUI /*= FALSE*/, BOOL bNoInitializeMAPI /*= FALSE*/, CDiagnostic* pDiagnostic /*= NULL*/)
{
	return ::NewMapiSession(bMultiThreaded, bNoLogonUI, bNoInitializeMAPI, pDiagnostic);
}

//-----------------------------------------------------------------------------
BOOL CIMagoMailConnector::ShowEmailDlg
		(
			CDocument* pCallerDoc,
			CMapiMessage& msg, 
			BOOL* pbAttachRDE, 
			BOOL* pbAttachPDF, 
			BOOL* pbCompressAttach,
			int* ,
			int* ,
			BOOL* pbConcatAttachPDF,
			BOOL* pbRequestDeliveryNotification, 
			BOOL* pbRequestReadNotification,
			LPCTSTR	pszCaptionOkBtn/* = NULL*/
		)
{
	CString sFrom;
	MailConnectorParams* params = AfxGetIMailConnector()->GetParams();

	if (params->GetUseSmtp())
	{
		SmtpMailConnectorParams smtpParams;

		CString sN = msg.m_sFromName.IsEmpty() ? smtpParams.GetFromName() : msg.m_sFromName;
		sN.Trim();
		if (!sN.IsEmpty())
			sFrom = '<' + sN + '>';

		if (msg.m_sFrom.IsEmpty())
			sFrom += smtpParams.GetFromAddress();

		smtpParams.SetCurrentSection(CMapiMessage::TAG_CERTIFIED);

		CString sCF = smtpParams.GetFromAddress();
		if (!sCF.IsEmpty())
		{
			sFrom += L";[C] ";

			CString sN = smtpParams.GetFromName(); sN.Trim();
			if (!sN.IsEmpty())
				sFrom += '<' + sN + '>';

			sFrom += sCF;
		}
	}

	CString token = AfxGetLoginManager()->GetIToken();
	//Qui devo chiamare il managed
	MIMagoMailConnector iMagoConnector;

	return iMagoConnector.OpenInfinityMailer(token.GetString(), AfxGetLoginManager()->GetNonProviderCompanyConnectionString(),  msg.m_sSubject, msg.m_sBody, msg.m_sFrom, msg.m_To, msg.m_BCC, msg.m_CC, msg.m_Attachments, false);
	 
}


//-----------------------------------------------------------------------------
BOOL CIMagoMailConnector::ShowEmailWithChildDlg
		(
			BOOL* pbAttachRDE, 
			BOOL* pbAttachPDF, 
			BOOL* pbCompressAttach,
			BOOL* pbConcatAttachPDF
		)
{
	CEMailWithChildDlg dlg(pbAttachRDE, pbAttachPDF, pbCompressAttach, pbConcatAttachPDF);

	return dlg.DoModal() == IDOK;
}

//-----------------------------------------------------------------------------
BOOL CIMagoMailConnector::Html2Mime (CMapiMessage& msg)
{
	return ::Html2Mime (msg);
}

//-----------------------------------------------------------------------------
BOOL CIMagoMailConnector::MapiShowAddressBook(HWND hWnd, CString& strAddress, CDiagnostic* pDiagnostic /*= NULL*/)
{
	//return ::MapiShowAddressBook (hWnd, strAddress);

	CStringArray arOldNames, arNewNames;

	CStringArray_Split(arOldNames, strAddress);

	MailConnectorParams* params = AfxGetIMailConnector()->GetParams();

	BOOL bOk = FALSE;
	IMapiSession* pMSession = NewMapiSession(TRUE, FALSE, FALSE, pDiagnostic);
	if (!pMSession)
	{
		if (pDiagnostic)
			pDiagnostic->Add(CString(_TB("Fail to open MAPI Session")));
		delete pMSession;
		return FALSE;
	}

	if (!pMSession->MapiInstalled())
	{
		if (pDiagnostic)
			pDiagnostic->Add(CString(_TB("Mapi are not installed")));
		delete pMSession;
		return FALSE;
	}

	if (!pMSession->Logon(params->GetOutlookProfile()))
	{
		if (pDiagnostic)
			pDiagnostic->Add(CString(_TB("Failed to Mapi Logon")));
		delete pMSession;
		return FALSE;
	}
	if (!pMSession->ShowAddressBook(arOldNames, arNewNames, hWnd))
	{
		if (pDiagnostic)
			pDiagnostic->Add(CString(_TB("Failed to send email by Mapi")));
		delete pMSession;
		return FALSE;
	}

	delete pMSession;
	
	CStringArray_Concat (arNewNames, strAddress);

	return TRUE;
}

//-----------------------------------------------------------------------------
BOOL CIMagoMailConnector::SendAsAttachments(CStringArray& arAttachmentsFiles, CStringArray& arAttachmentsTitles, CDiagnostic* pDiagnostic /*= NULL*/)
{
	// Invio email.
	CMapiMessage Email;
	BOOL bSent = FALSE;

	CString strLocal = ::_tsetlocale (LC_ALL, NULL);
	for (int i = 0; i <= arAttachmentsFiles.GetUpperBound(); i++)
		Email.m_Attachments.Add(arAttachmentsFiles.GetAt(i));
		
	for (int i = 0; i <= arAttachmentsTitles.GetUpperBound(); i++)
		Email.m_AttachmentTitles.Add(arAttachmentsTitles.GetAt(i));
	
	BOOL bCompressAttach = FALSE;

	if (AfxGetIMailConnector()->ShowEmailDlg(NULL, Email, NULL, NULL, &bCompressAttach))
	{
		if (bCompressAttach)
		{
			CString strZipFile;
			CString strZipTitle;
			Email.m_Attachments.RemoveAll();
			Email.m_AttachmentTitles.RemoveAll();
			for (int i = 0; i <= arAttachmentsFiles.GetUpperBound(); i++)
			{
				strZipFile = arAttachmentsFiles.GetAt(i) +_T(".zip");
				strZipTitle =  arAttachmentsTitles.GetAt(i) + _T(".zip");			
				::ZCompress(arAttachmentsFiles.GetAt(i), strZipFile, strZipTitle);
				Email.m_Attachments.Add(strZipFile);
				Email.m_AttachmentTitles.Add(strZipTitle);
			}
		}
		bSent = AfxGetIMailConnector()->SendMail (Email,NULL, NULL, pDiagnostic);
	}

	CString strLocal2 = ::_tsetlocale (LC_ALL, NULL);
	if (strLocal != strLocal2)
	{
		::_tsetlocale (LC_ALL, strLocal);
	}

	return bSent;
}


//=============================================================================
BOOL CIMagoMailConnector::SmtpSendMail
		(
			CMapiMessage& msg,
			BOOL* bRequestDeliveryNotification/* = NULL*/, 
			BOOL* bRequestReadNotification/* = NULL*/,
			CDiagnostic* pDiagnostic/* = NULL*/
		)
{
	if (!AfxIsActivated(TBEXT_APP,  MAILCONNECTOR_ACT))
	{
		if (pDiagnostic)
			pDiagnostic->Add(CString(_TB("MailConnector not activated")));						

		return FALSE;
	}

	if (msg.m_To.GetSize() == 0 && msg.m_CC.GetSize() == 0 && msg.m_BCC.GetSize() == 0)
	{
		if (msg.m_PostaLiteMsg.GetCount())
			return TRUE;

		if (pDiagnostic)
			pDiagnostic->Add(CString(_TB("Empty addressee")));						

		return FALSE;
	}


	CMapiMessage msgNormal;
	CMapiMessage msgCertified;

	msg.SplitCertifiedAddress(msgNormal, msgCertified);

	CSmtpNet lib;

	BOOL bSentNormal = FALSE;
	BOOL bSentCertified = FALSE;
	BOOL bOkNormal = FALSE;
	BOOL bOkCertified = FALSE;

	if (!msgNormal.IsEmptyRecipients() &&
		(msgNormal.m_deEmailAddressType == E_EMAIL_ADDRESS_TYPE_ALL || msgNormal.m_deEmailAddressType == E_EMAIL_ADDRESS_TYPE_NORMAL))
	{
		SmtpMailConnectorParams params;

		BOOL b = params.UseSystemWebMail();
		if (b)
			bOkNormal = lib.SendMail2(params, msgNormal, bRequestDeliveryNotification, bRequestReadNotification, pDiagnostic);
		else
			bOkNormal = lib.SendMail (params, msgNormal, bRequestDeliveryNotification, bRequestReadNotification, pDiagnostic);
		
		bSentNormal = TRUE;
	}

	if (!msgCertified.IsEmptyRecipients() &&
		(msgCertified.m_deEmailAddressType == E_EMAIL_ADDRESS_TYPE_ALL || msgCertified.m_deEmailAddressType == E_EMAIL_ADDRESS_TYPE_CERTIFIED))
	{
		SmtpMailConnectorParams params(CMapiMessage::TAG_CERTIFIED);

		if (params.GetFromAddress().IsEmpty() || params.GetHostName().IsEmpty())
			;	//Account secondario NON compilato
		else
		{
			BOOL b = params.UseSystemWebMail();
			if (b)
				bOkCertified = lib.SendMail2(params, msgCertified, bRequestDeliveryNotification, bRequestReadNotification, pDiagnostic);
			else
				bOkCertified = lib.SendMail (params, msgCertified, bRequestDeliveryNotification, bRequestReadNotification, pDiagnostic);
			
			bSentCertified = TRUE;
		}
	}

	if (bSentNormal && bOkNormal && bSentCertified && bOkCertified)
		return TRUE;

	if (bSentNormal && bOkNormal && !bSentCertified)
		return TRUE;

	if (bSentCertified && bOkCertified && !bSentNormal)
		return TRUE;

	return FALSE;
}

//=============================================================================

BOOL CIMagoMailConnector::SendMail
		(
			CMapiMessage& msg,
			BOOL* pbRequestDeliveryNotification /*= NULL*/,
			BOOL* pbRequestReadNotification /*= NULL*/,
			CDiagnostic* pDiagnostic /*= NULL*/
		) 
{
	if (!AfxIsActivated(TBEXT_APP,  MAILCONNECTOR_ACT))
	{
		if (pDiagnostic)
			pDiagnostic->Add(CString(_TB("MailConnector not activated")));						

		return FALSE;
	}

	if (msg.m_To.GetSize() == 0 && msg.m_CC.GetSize() == 0 && msg.m_BCC.GetSize() == 0)
	{
		if (msg.m_PostaLiteMsg.GetCount())
			return TRUE;

		if (pDiagnostic)
			pDiagnostic->Add(CString(_TB("Empty addressee")));						

		return FALSE;
	}

	BOOL bOk = TRUE;

	MailConnectorParams* params = AfxGetIMailConnector()->GetParams();

	//if (params->GetUseMapi())
	//{
	//	//elimino eventuali TAG dagli indirizzi
	//	msg.SplitAllAddress();	//un singolo indirizzo per ogni elemento dell'array 

	//	if (msg.m_deEmailAddressType == E_EMAIL_ADDRESS_TYPE_NORMAL)
	//	{
	//		CStringArray_RemoveWhenPrefixed		(msg.m_To, CMapiMessage::TAG_CERTIFIED);
	//		CStringArray_RemoveWhenPrefixed		(msg.m_CC, CMapiMessage::TAG_CERTIFIED);
	//		CStringArray_RemoveWhenPrefixed		(msg.m_BCC, CMapiMessage::TAG_CERTIFIED);
	//	}
	//	else if (msg.m_deEmailAddressType == E_EMAIL_ADDRESS_TYPE_CERTIFIED)
	//	{
	//		CStringArray_RemoveWhenNotPrefixed	(msg.m_To, CMapiMessage::TAG_CERTIFIED);
	//		CStringArray_RemoveWhenNotPrefixed	(msg.m_CC, CMapiMessage::TAG_CERTIFIED);
	//		//CStringArray_RemoveWhenNotPrefixed	(msg.m_BCC, CMapiMessage::TAG_CERTIFIED);
	//		msg.m_BCC.RemoveAll();	//Non ammessi
	//	}

	//	CStringArray_RemovePrefix	(msg.m_To, CMapiMessage::TAG_CERTIFIED);
	//	CStringArray_RemovePrefix	(msg.m_CC, CMapiMessage::TAG_CERTIFIED);
	//	CStringArray_RemovePrefix	(msg.m_BCC, CMapiMessage::TAG_CERTIFIED);
	//	//----

	//	CString strLocal = ::_tsetlocale (LC_ALL, NULL);

	//	if (msg.m_sMapiProfile.IsEmpty())
	//		msg.SetMapiProfile (params->GetOutlookProfile());

	//	IMapiSession* pMSession = NULL;
	//	try
	//	{
	//		bOk = FALSE;
	//		IMapiSession* pMSession = AfxGetIMailConnector()->NewMapiSession(TRUE, FALSE, params->GetSupportOutlookExpress());
	//		if (pMSession != NULL)
	//		{
	//			if (!pMSession->MapiInstalled())
	//			{
	//				if (pDiagnostic)
	//					pDiagnostic->Add(CString(_TB("Mapi are not installed")));
	//			}
	//			else if (!pMSession->Logon(msg.m_sMapiProfile))
	//			{
	//				if (pDiagnostic)
	//					pDiagnostic->Add(CString(_TB("Failed to Mapi Logon")));
	//			}
	//			else if (!pMSession->Send(msg))
	//			{
	//				if (pDiagnostic)
	//					pDiagnostic->Add(CString(_TB("Failed to send email by Mapi")));						
	//			}
	//			else 
	//				bOk = TRUE;
	//			delete pMSession;
	//		}
	//		else if (pDiagnostic)
	//			pDiagnostic->Add(CString(_TB("Failed to open Mapi Session")));

	//	}
	//	catch (CException* pE)
	//	{
	//		bOk = FALSE;
	//		if (pDiagnostic)
	//			pDiagnostic->Add(pE);
	//		pE->Delete();
	//	}
	//	SAFE_DELETE(pMSession);

	//	CString strLocal2 = ::_tsetlocale (LC_ALL, NULL);
	//	if (strLocal != strLocal2)
	//	{
	//		::_tsetlocale (LC_ALL, strLocal);
	//	}
	//}
	//else //if (params->GetUseSmtp())
	//{
	//	if (!SmtpSendMail (msg, pbRequestDeliveryNotification, pbRequestReadNotification, pDiagnostic))
	//	{
	//		bOk = FALSE;
	//	}
	//}

	CString token = AfxGetLoginManager()->GetIToken();
	//Qui devo chiamare il managed
	MIMagoMailConnector iMagoConnector;

	bOk =  iMagoConnector.OpenInfinityMailer(token.GetString(), AfxGetLoginManager()->GetNonProviderCompanyConnectionString(), msg.m_sSubject, msg.m_sBody, msg.m_sFrom, msg.m_To, msg.m_BCC, msg.m_CC, msg.m_Attachments, true);


	if(bOk)
	{
		//---- bug 16326
		msg.m_To.RemoveAll();
		msg.m_CC.RemoveAll();
		msg.m_BCC.RemoveAll();
		msg.m_Attachments.RemoveAll();
		msg.m_AttachmentTitles.RemoveAll();
	}

	return bOk; 
}

//=============================================================================
CString CIMagoMailConnector::FormatFaxAddress(CString sFax)
{ 
	MailConnectorParams* params = AfxGetIMailConnector()->GetParams();

	CString sFormat = params->GetFAXFormatTemplate();
	if (sFormat.Replace(_T("##"), _T("%s")) < 1)
	{
		if (sFormat.Find(_T("%s")) < 0)
			return sFax;
	}
	sFax.Format(sFormat, sFax);
	return sFax; 
}

//=============================================================================

