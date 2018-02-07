#include "StdAfx.h"

#include <TbGeneric\ParametersSections.h>
#include <TbGeneric\LineFile.h>
#include <TbGeneric\TBStrings.h>
#include <TbGeneric\CMapi.h>

#include <TbNameSolver\PathFinder.h>
#include <TbNameSolver\LoginContext.h>
#include <TbNameSolver\FileSystemFunctions.h>

#include "SmtpNet.h"

///////////////////////////////////////////////////////////////////////////////
//using namespace System::Net;
//using namespace System::Net::Mail;
//-----------------------------------------------------------------------------

CSmtpNet::CSmtpNet(void)
{

}

CSmtpNet::~CSmtpNet(void)
{
}

//--------------------------------------------------------------------------

BOOL CSmtpNet::SendMail 
(
	SmtpMailConnectorParams& params,
	CMapiMessage& mapiMsg,
	BOOL* pbRequestDeliveryNotification/* = NULL*/, 
	BOOL* pbRequestReadNotification/* = NULL*/,
	CDiagnostic* pDiagnostic/* = NULL*/
)
{
	BOOL returnValue = FALSE;
	
	System::Net::Mail::MailMessage ^mailMsg = nullptr;
	System::Net::Mail::SmtpClient^ client = nullptr;

    try
    {
		MailConnectorParams* generalParams = AfxGetIMailConnector()->GetParams();
	
		mailMsg = gcnew System::Net::Mail::MailMessage();

		//TODO params.GetMimeEncoding()

		mailMsg->SubjectEncoding = System::Text::Encoding::UTF8;
		mailMsg->Subject = gcnew System::String(mapiMsg.m_sSubject); 

		BOOL bOk = TRUE;
		mailMsg->BodyEncoding = System::Text::Encoding::UTF8;

		if (!mapiMsg.m_sBody.IsEmpty())
		{
			if (mapiMsg.m_bBodyFromFile)
			{
				CString strName = mapiMsg.m_sBody;
				mapiMsg.m_sBody.Empty();
				LoadLineTextFile(strName, mapiMsg.m_sBody);
				mapiMsg.m_bBodyFromFile = FALSE;
			}
		}

		System::String^ mt;
		if (params.GetHtmlEncoding() || !mapiMsg.m_sHtml.IsEmpty())
		{
			mailMsg->IsBodyHtml = true;
			mapiMsg.m_bNeedEncoding = TRUE;

			bOk = AfxGetIMailConnector()->Html2Mime (mapiMsg);

			mt = System::Net::Mime::MediaTypeNames::Text::Html;
		}
		else
		{
			mailMsg->IsBodyHtml = false;
			mt = System::Net::Mime::MediaTypeNames::Text::Plain;	// Xml, RichText for RTF
		}

		CString sAllTo, sAllCc, sAllBcc;
		for (int i = 0; i < mapiMsg.m_To.GetSize(); i++)
		{
			sAllTo += mapiMsg.m_To.GetAt(i) ;
			sAllTo += L"; ";
		}
		for (int i = 0; i < mapiMsg.m_CC.GetSize(); i++)
		{
			sAllCc += mapiMsg.m_CC.GetAt(i) ;
			sAllCc += L"; ";
		}
		for (int i = 0; i < mapiMsg.m_BCC.GetSize(); i++)
		{
			sAllBcc += mapiMsg.m_BCC.GetAt(i) ;
			sAllBcc += L"; ";
		}

		CString sRedirectTo = generalParams->GetRedirectToAddress();
		CString strOrigDest;
		if (!sRedirectTo.IsEmpty())
		{
			CString strNL("\n");
			if (!mapiMsg.m_sHtml.IsEmpty())
				strNL += L"<br />\n";

			strOrigDest += L"Original addressee To :\n";
			strOrigDest += sAllTo + strNL;

			strOrigDest += L"Original addressee Cc :\n";
			strOrigDest += sAllCc + strNL;

			strOrigDest += L"Original addressee Bcc :\n";
			strOrigDest += sAllBcc + strNL;

			strOrigDest += strNL;
		}

		System::Net::Mail::AlternateView^ altView = System::Net::Mail::AlternateView::CreateAlternateViewFromString 
			(
				gcnew System::String(strOrigDest + (mapiMsg.m_sHtml.IsEmpty() ? mapiMsg.m_sBody : mapiMsg.m_sHtml)),
				System::Text::Encoding::UTF8, 
				mt
			);
	
		mailMsg->AlternateViews->Add(altView);

		CString sFromAddress	= !mapiMsg.m_sFrom.IsEmpty()		? mapiMsg.m_sFrom		: params.GetFromAddress();
		CString sFromName		= !mapiMsg.m_sFromName.IsEmpty()	? mapiMsg.m_sFromName	: params.GetFromName();
		if (sFromName.IsEmpty()) 
		{
			mailMsg->From = gcnew System::Net::Mail::MailAddress(gcnew System::String(sFromAddress));
		}
		else 
		{
			mailMsg->From = gcnew System::Net::Mail::MailAddress(gcnew System::String(sFromAddress), gcnew System::String(sFromName), System::Text::Encoding::UTF8);
		}
		mailMsg->Sender = mailMsg->From;

		CString sReplyToAddress = params.GetReplyToAddress();
		if (sReplyToAddress.IsEmpty()) sReplyToAddress = sFromAddress;
		CString sReplyToName = params.GetReplyToName();
		if (sReplyToName.IsEmpty()) sReplyToName = sFromName;

		if (sReplyToName.IsEmpty()) 
		{
			mailMsg->ReplyTo = gcnew System::Net::Mail::MailAddress(gcnew System::String(sReplyToAddress));
		}
		else 
		{
			mailMsg->ReplyTo = gcnew System::Net::Mail::MailAddress(gcnew System::String(sReplyToAddress), gcnew System::String(sReplyToName), System::Text::Encoding::UTF8);
		}

		if (sRedirectTo.IsEmpty())
		{
			for (int i = 0; i < mapiMsg.m_To.GetSize(); i++)
			{
				mailMsg->To->Add(gcnew System::String(mapiMsg.m_To.GetAt(i)));
			}
			for (int i = 0; i < mapiMsg.m_CC.GetSize(); i++)
			{
				mailMsg->CC->Add(gcnew System::String(mapiMsg.m_CC.GetAt(i)));
			}
			for (int i = 0; i < mapiMsg.m_BCC.GetSize(); i++)
			{
				mailMsg->Bcc->Add(gcnew System::String(mapiMsg.m_BCC.GetAt(i)));
			}
		}
		else
		{
			if (sRedirectTo.Find(';') >= 0)
			{
				CString str = sRedirectTo;

				if (str.Right(1) != ';' && str.Right(1) != '\n')
					str += ';';

				CStringArray ar;
				CStringArray_Split (ar, str);

				for (int i = 0; i < ar.GetSize(); i++)
				{
					mailMsg->To->Add(gcnew System::String(ar.GetAt(i)));
				}
			}
			else
				mailMsg->To->Add(gcnew System::String(sRedirectTo));
		}

		CString sTrackingAddress = generalParams->GetTrackingAddressForSentEmails();
		if (!sTrackingAddress.IsEmpty())
		{
			if (sTrackingAddress.Find(';') >= 0)
			{
				CString str = sTrackingAddress;

				if (str.Right(1) != ';' && str.Right(1) != '\n')
					str += ';';

				CStringArray ar;
				CStringArray_Split (ar, str);

				for (int i = 0; i < ar.GetSize(); i++)
				{
					mailMsg->Bcc->Add(gcnew System::String(ar.GetAt(i)));
				}
			}
			else
				mailMsg->Bcc->Add(gcnew System::String(sTrackingAddress));
		}

		BOOL bRequestDeliveryNotifications = pbRequestDeliveryNotification ? *pbRequestDeliveryNotification : generalParams->GetRequestDeliveryNotifications();
		if (bRequestDeliveryNotifications)
		{
			mailMsg->DeliveryNotificationOptions = System::Net::Mail::DeliveryNotificationOptions::OnSuccess;
		}

		if (pbRequestReadNotification ? *pbRequestReadNotification : generalParams->GetRequestReadNotifications())
		{
			mailMsg->Headers->Add("Disposition-Notification-To", gcnew System::String(params.GetFromAddress()));
		}

		//bug. 17256
		//System::Net::Mime::ContentType^ contentTypePdf = gcnew System::Net::Mime::ContentType("application/pdf");
		//System::Net::Mime::ContentType^ contentTypeZip = gcnew System::Net::Mime::ContentType("application/zip");
		//System::Net::Mime::ContentType^ contentTypeOctet = gcnew System::Net::Mime::ContentType("application/octet-stream");
		System::Net::Mime::ContentType^ contentType = nullptr; 
	
		for (int i = 0; i < mapiMsg.m_Attachments.GetSize(); i++)
		{
			CString sPath = mapiMsg.m_Attachments.GetAt(i);
			if (!ExistFile(sPath))
				continue;
			System::Net::Mail::Attachment^ attachment = nullptr;
			int nTry = 0;
			while (true)
			{
				try
				{
					//il file potrebbe essere ancora locckato (ad es. da Excel)
					//faccio 10 tentativi prima di schiantarmi
					attachment = gcnew System::Net::Mail::Attachment(gcnew System::String(sPath));	//, Mime::MediaTypeNames::Application::Pdf
					break;
				}
				catch (System::Exception^ ex)
				{
					if (nTry++ == 10)
					{
						if (pDiagnostic)
							pDiagnostic->Add(CString(ex->Message));
						return FALSE;
					}
					Sleep(1000);
				}
			}
		
			attachment->NameEncoding = System::Text::Encoding::UTF8;
			CString sTitle;
			if (i < mapiMsg.m_AttachmentTitles.GetSize())
				sTitle = mapiMsg.m_AttachmentTitles.GetAt(i);
			if (!sTitle.IsEmpty())
				attachment->Name =  gcnew System::String(sTitle);

			CString sExt = ::GetExtension(sPath);
			if (sExt.CompareNoCase(_T(".pdf")) == 0)
			{
				contentType = gcnew System::Net::Mime::ContentType("application/pdf");
				attachment->ContentType = contentType;
			}
			else if (sExt.CompareNoCase(_T(".zip")) == 0)
			{
				contentType = gcnew System::Net::Mime::ContentType("application/zip");
				attachment->ContentType = contentType;
			}
			else
			{
				contentType = gcnew System::Net::Mime::ContentType("application/octet-stream");
				attachment->ContentType = contentType;
			}

			mailMsg->Attachments->Add(attachment);
		}

		ASSERT(mapiMsg.m_Images.GetSize() == mapiMsg.m_ImagesCIDs.GetSize());

		CString sPath;
		CString sTemplateFileName = mapiMsg.m_sTemplateFileName;
		if (!sTemplateFileName.IsEmpty())
		{
			if (!::IsFileName(sTemplateFileName))
			{
				CTBNamespace aFileNs(CTBNamespace::TEXT);
				aFileNs.SetNamespace(sTemplateFileName);
				if (aFileNs.IsValid() && (aFileNs.GetType() == CTBNamespace::TEXT || aFileNs.GetType() == CTBNamespace::FILE))
					sTemplateFileName = AfxGetPathFinder()->GetFileNameFromNamespace(aFileNs, AfxGetLoginInfos()->m_strUserName);
			}

			if (::IsFileName(sTemplateFileName))
				sPath = ::GetPath(sTemplateFileName, TRUE);
		}
		for (int i = 0; i < mapiMsg.m_Images.GetSize(); i++)
		{
			CString sImg = mapiMsg.m_Images.GetAt(i);
			if (!IsFileName(sImg))
				sImg = sPath + sImg;
			if (!IsFileName(sImg))
				continue;

			CString sImgCID = mapiMsg.m_ImagesCIDs.GetAt(i);
			CString sExt = ::GetExtension(sImg);
			sExt = _T("image/")+ sExt.Mid(1);
			try
			{
				System::Net::Mail::LinkedResource^ embeddedPicture = gcnew System::Net::Mail::LinkedResource
					(
						gcnew System::String(sImg),
						gcnew System::String(sExt)
					);
				embeddedPicture->ContentId = gcnew System::String(sImgCID);

				altView->LinkedResources->Add(embeddedPicture);
			}
			catch (System::Exception^ ex)
			{
				if (pDiagnostic)
					pDiagnostic->Add(CString(ex->Message) + sImg);
			}
		}

		int nPort = params.GetPort(); 

		System::String^ host =  gcnew System::String(params.GetHostName());
		client = gcnew System::Net::Mail::SmtpClient(host, nPort);
		
		client->EnableSsl = params.GetUseExplicitSSL() ? true : false;
		if (client->EnableSsl)
		{
			/*
			System::Net::SecurityProtocolType::Ssl3 |
			System::Net::SecurityProtocolType::Tls |
			System::Net::SecurityProtocolType::Tls11 |
			System::Net::SecurityProtocolType::Tls12;
			*/
			int secProt = params.GetSecurityProtocolType();
			if (secProt)
			{
				try {
					System::Net::ServicePointManager::SecurityProtocol = (System::Net::SecurityProtocolType)secProt;
				} 
				catch(...)
				{}
			}
		}

		client->Timeout = params.GetTimeout();

		CString sName = params.GetUserName();
		if (sName.IsEmpty())
		{
			client->UseDefaultCredentials = true;

			client->Credentials = System::Net::CredentialCache::DefaultNetworkCredentials;
		}
		else
		{
			CString sPwd = params.GetPassword();

			client->DeliveryMethod = System::Net::Mail::SmtpDeliveryMethod::Network;

			client->UseDefaultCredentials = false;
			
			if (params.GetConfiguration() == 1)
			{
				client->Port = nPort;
				System::Net::NetworkCredential^ netCredential = gcnew System::Net::NetworkCredential
																	(
																		gcnew System::String(sName), 
																		gcnew System::String(params.GetPassword())
																	);
				client->Credentials = netCredential;
			}
			else
			{
				System::Net::NetworkCredential^ netCredential = gcnew System::Net::NetworkCredential
													(
														gcnew System::String(sName), 
														gcnew System::String(params.GetPassword()),
														""
													);

				System::Net::CredentialCache^ netCredentialCache = gcnew System::Net::CredentialCache();
        
				netCredentialCache->Add(host, nPort, "NTLM", netCredential);
		
				client->Credentials = netCredentialCache->GetCredential(host, nPort, "NTLM");

				//System::Net::NetworkCredential^ netCredential = gcnew System::Net::NetworkCredential
				//									(
				//										gcnew System::String(sName), 
				//										gcnew System::String(params.GetPassword())
				//									);
				//System::Net::CredentialCache^ netCredentialCache = gcnew System::Net::CredentialCache();        
				//System::String^ authenticationType =  gcnew System::String(params.GetAuthenticationType());
				//netCredentialCache->Add(host, nPort, authenticationType, netCredential);		
				//client->Credentials = netCredentialCache->GetCredential(host, nPort, authenticationType);
			}
		}

		//client->Port = nPort;
		client->Send(mailMsg);  
	
		returnValue = TRUE;
	}
	catch (System::Net::Mail::SmtpException^ ex)
    {
        if (pDiagnostic)
		{
			System::Net::Mail::SmtpStatusCode status = ex->StatusCode;
			pDiagnostic->Add(CString(status.ToString()));
			pDiagnostic->Add(CString(ex->Message));
			if (ex->InnerException != nullptr)
				pDiagnostic->Add(CString(ex->InnerException->Message));
		}
    }
	catch (System::Exception^ ex)
	{
		if (pDiagnostic)
			pDiagnostic->Add(CString(ex->Message));
		delete ex;
	}
	catch (CException* pE)
	{
		if (pDiagnostic)
			pDiagnostic->Add(pE);
		pE->Delete();
	}
	catch (...)
	{
		if (pDiagnostic)
			pDiagnostic->Add(_TB("Internal error on sent email by smtp"));
	}
	//------------
	try 
	{
		//bug #19139
		if (mailMsg != nullptr)
		{
			for each (System::Net::Mail::Attachment^ a in  mailMsg->Attachments)
			{
				delete(a);
			}

			delete mailMsg;
		}

		if (client != nullptr)
			delete client;
	}
	catch(...)
	{}

	return returnValue;
}

//-----------------------------------------------------------------------------


/*
System.Web.Mail.MailMessage newMail = new System.Web.Mail.MailMessage();
 
newMail.From = "miamail@pec.it";
newMail.To = "[dest]";
newMail.Subject = "[object]";
newMail.BodyFormat = System.Web.Mail.MailFormat.Text;
newMail.Body = "[body]";
newMail.Priority = System.Web.Mail.MailPriority.High;

newMail.Fields.Add("http://schemas.microsoft.com/cdo/configuration/smtpserver", "smtps.pec.aruba.it");
newMail.Fields.Add("http://schemas.microsoft.com/cdo/configuration/smtpserverport", "465");
newMail.Fields.Add("http://schemas.microsoft.com/cdo/configuration/sendusing", "2");
newMail.Fields.Add("http://schemas.microsoft.com/cdo/configuration/smtpauthenticate", "1");
newMail.Fields.Add("http://schemas.microsoft.com/cdo/configuration/sendusername", "miamail@pec.it");
newMail.Fields.Add("http://schemas.microsoft.com/cdo/configuration/sendpassword", "password");
newMail.Fields.Add("http://schemas.microsoft.com/cdo/configuration/smtpusessl", "true");
 
System.Web.Mail.SmtpMail.SmtpServer = "smtps.pec.aruba.it:465";
System.Web.Mail.SmtpMail.Send(newMail);

-salvare una copia della mail inviata nella cartella "Inviate" dell'account di posta? 
Ho provato ad aggiungere
newMail.Fields.Add("http://schemas.microsoft.com/cdo/configuration/savesentitems", "true")
ma purtroppo il messaggio inviato non viene salvato.

-allegati senza aggiungere MailEncoding.Base64:
NON FUNZIONA! System.Web.Mail.MailAttachment allegato = new System.Web.Mail.MailAttachment("pec.txt", MailEncoding.Base64);
System.Web.Mail.MailAttachment allegato = new System.Web.Mail.MailAttachment("pec.txt");
newMail.Attachments.Add(allegato);
Chiaramente il percorso del file deve essere assoluto.

-newMail.Headers.Add("Delivery-Notification-To", Configuration.ConfigurationManager.AppSettings("REPLY_TO"));
newMail.Headers.Add("Disposition-Notification-To", Configuration.ConfigurationManager.AppSettings("REPLY_TO"));
newMail.Headers.Add("Return-Receipt-To", Configuration.ConfigurationManager.AppSettings("REPLY_TO"));

*/

//disabilito il warning sulla classe deprecata
#pragma warning(disable: 4947) 
#pragma warning(disable: 4996) 

void TrimColon(CString& s)
{
	s.Trim();
	if (!s.IsEmpty())
	{
		if (s[0] == ';') 
			s.SetAt(0, ' ');
		if (s[s.GetLength() - 1] == ';') 
			s.SetAt(s.GetLength() - 1, ' ');
		s.Trim();
	}
}

BOOL CSmtpNet::SendMail2 
(
	SmtpMailConnectorParams& params,
	CMapiMessage& mapiMsg,
	BOOL* pbRequestDeliveryNotification/* = NULL*/, 
	BOOL* pbRequestReadNotification/* = NULL*/,
	CDiagnostic* pDiagnostic/* = NULL*/
)
{
	BOOL returnValue = FALSE;

	System::Web::Mail::MailMessage ^mailMsg = nullptr;

    try
    {
		MailConnectorParams* generalParams = AfxGetIMailConnector()->GetParams();

		mailMsg = gcnew System::Web::Mail::MailMessage();

		mailMsg->Subject = gcnew System::String(mapiMsg.m_sSubject); 

		BOOL bOk = TRUE;

		if (!mapiMsg.m_sBody.IsEmpty())
		{
			if (mapiMsg.m_bBodyFromFile)
			{
				CString strName = mapiMsg.m_sBody;
				mapiMsg.m_sBody.Empty();
				LoadLineTextFile(strName, mapiMsg.m_sBody);
				mapiMsg.m_bBodyFromFile = FALSE;
			}
		}

		if (params.GetHtmlEncoding() || !mapiMsg.m_sHtml.IsEmpty())
		{
			bOk = AfxGetIMailConnector()->Html2Mime (mapiMsg);

			mailMsg->BodyFormat = System::Web::Mail::MailFormat::Html;
		}
		else
		{
			mailMsg->BodyFormat = System::Web::Mail::MailFormat::Text;
		}

		mailMsg->BodyEncoding = System::Text::Encoding::UTF8;
		mailMsg->Body = gcnew System::String(mapiMsg.m_sHtml.IsEmpty() ? mapiMsg.m_sBody : mapiMsg.m_sHtml);

		CString sAllTo, sAllCc, sAllBcc;
		for (int i = 0; i < mapiMsg.m_To.GetSize(); i++)
		{
			sAllTo += mapiMsg.m_To.GetAt(i) ;
			sAllTo += L"; ";
		}
		for (int i = 0; i < mapiMsg.m_CC.GetSize(); i++)
		{
			sAllCc += mapiMsg.m_CC.GetAt(i) ;
			sAllCc += L"; ";
		}
		for (int i = 0; i < mapiMsg.m_BCC.GetSize(); i++)
		{
			sAllBcc += mapiMsg.m_BCC.GetAt(i) ;
			sAllBcc += L"; ";
		}

		CString sRedirectTo = generalParams->GetRedirectToAddress();
		CString strOrigDest;
		if (!sRedirectTo.IsEmpty())
		{
			CString strNL("\n");
			if (!mapiMsg.m_sHtml.IsEmpty())
				strNL += L"<br />\n";

			strOrigDest += L"Original addressee To :\n";
			strOrigDest += sAllTo + strNL;

			strOrigDest += L"Original addressee Cc :\n";
			strOrigDest += sAllCc + strNL;

			strOrigDest += L"Original addressee Bcc :\n";
			strOrigDest += sAllBcc + strNL;

			strOrigDest += strNL;
		}

		CString sFromAddress = !mapiMsg.m_sFrom.IsEmpty()		? mapiMsg.m_sFrom		: params.GetFromAddress();
		CString sFromName		= !mapiMsg.m_sFromName.IsEmpty()	? mapiMsg.m_sFromName	: params.GetFromName();
		if (sFromName.IsEmpty())
			mailMsg->From = gcnew System::String(sFromAddress);
		else
		{
			CString s; s.Format(L"\"%s\" <%s>", sFromName, sFromAddress);
			mailMsg->From = gcnew System::String(s);
		}

		CString sReplyToAddress = params.GetReplyToAddress();
		if (sReplyToAddress.IsEmpty()) 
			sReplyToAddress = sFromAddress;
		mailMsg->Headers->Add ( L"Reply-To", gcnew System::String(sReplyToAddress) );

		CString sTrackingAddress = generalParams->GetTrackingAddressForSentEmails();
		if (!sTrackingAddress.IsEmpty() && params.GetCurrentSection().CompareNoCase(CMapiMessage::TAG_CERTIFIED) != 0)
		{
			if (!sAllBcc.IsEmpty())
			{
				TCHAR c = sAllBcc[sAllBcc.GetLength() - 1];
				if (c != ';' && c != '\n')
					sAllBcc += ';';
			}
			sAllBcc += sTrackingAddress;
		}

		TrimColon(sAllTo);
		TrimColon(sAllCc);
		TrimColon(sAllBcc);
		TrimColon(sRedirectTo);

		if (sRedirectTo.IsEmpty())
		{
			mailMsg->To = gcnew System::String(sAllTo);
		
			mailMsg->Cc = gcnew System::String(sAllCc);
		
			mailMsg->Bcc = gcnew System::String(sAllBcc);
		}
		else
		{
			mailMsg->To = gcnew System::String(sRedirectTo);
		}

		BOOL bRequestDeliveryNotifications = pbRequestDeliveryNotification ? *pbRequestDeliveryNotification : generalParams->GetRequestDeliveryNotifications();
		if (bRequestDeliveryNotifications)
		{
			mailMsg->Headers->Add(gcnew System::String(L"Delivery-Notification-To"), gcnew System::String(params.GetFromAddress()));
			mailMsg->Headers->Add(gcnew System::String(L"Return-Receipt-To"), gcnew System::String(params.GetFromAddress()));
		}

		if (pbRequestReadNotification ? *pbRequestReadNotification : generalParams->GetRequestReadNotifications())
		{
			mailMsg->Headers->Add(gcnew System::String(L"Disposition-Notification-To"), gcnew System::String(params.GetFromAddress()));
		}

		CString tempDir;
		for (int i = 0; i < mapiMsg.m_Attachments.GetSize(); i++)
		{
			CString sFilePath = mapiMsg.m_Attachments.GetAt(i);
			if (!ExistFile(sFilePath))
				continue;

			//rinomina gli allegati
			try 
			{
				CString sTitle;
				if (i <  mapiMsg.m_AttachmentTitles.GetSize())
					sTitle = mapiMsg.m_AttachmentTitles.GetAt(i);
				if (!sTitle.IsEmpty())
				{
					if (tempDir.IsEmpty())
					{
						DataGuid dg; dg.AssignNewGuid();
						TCHAR szTmpPath[255];
						if (!GetTempPath(255, szTmpPath))
							goto l_send;

						tempDir =  CString(szTmpPath) + dg.Str(TRUE);
						RecursiveCreateFolders(tempDir);
					}
				
					int idx = sFilePath.ReverseFind('.');
					if (idx > -1)
					{
						CString sExt = sFilePath.Mid(idx);
						CString sPath = sFilePath.Left(idx-1);

						int idx2 = sTitle.ReverseFind('.');
						if (idx2 < 0)
							sTitle += sExt;
					
						CString sName = tempDir + '\\' + sTitle;
						CString sNewName (sName); 
						int i;
						for (i = 1; ::ExistFile(sNewName) && i < 100; i++ ) 
							sNewName.Format(L"%s_%d", sName, i);

						if (i < 100 && ::CopyFile(sFilePath, sNewName, FALSE))
							sFilePath = sNewName;
					}
				}
			}
			catch(...)
			{;}
l_send:;
			System::Web::Mail::MailAttachment^ attachment = nullptr;
			int nTry = 0;
			while (true)
			{
				try
				{
					//il file potrebbe essere ancora locckato (ad es. da Excel)
					//faccio 10 tentativi prima di schiantarmi
					attachment = gcnew System::Web::Mail::MailAttachment(gcnew System::String(sFilePath));	//, Mime::MediaTypeNames::Application::Pdf
					break;
				}
				catch (System::Exception^ ex)
				{
					if (nTry++ == 10)
					{
						if (pDiagnostic)
							pDiagnostic->Add(CString(ex->Message));
						return FALSE;
					}
					Sleep(1000);
				}
			}
		
			mailMsg->Attachments->Add(attachment);
		}

		ASSERT(mapiMsg.m_Images.GetSize() == mapiMsg.m_ImagesCIDs.GetSize());

		CString sPath;
		CString sTemplateFileName = mapiMsg.m_sTemplateFileName;
		if (!sTemplateFileName.IsEmpty())
		{
			if (!::IsFileName(sTemplateFileName))
			{
				CTBNamespace aFileNs(CTBNamespace::TEXT);
				aFileNs.SetNamespace(sTemplateFileName);
				if (aFileNs.IsValid() && (aFileNs.GetType() == CTBNamespace::TEXT || aFileNs.GetType() == CTBNamespace::FILE))
					sTemplateFileName = AfxGetPathFinder()->GetFileNameFromNamespace(aFileNs, AfxGetLoginInfos()->m_strUserName);
			}

			if (::IsFileName(sTemplateFileName))
				sPath = ::GetPath(sTemplateFileName, TRUE);
		}
		for (int i = 0; i < mapiMsg.m_Images.GetSize(); i++)
		{
			CString sImg = mapiMsg.m_Images.GetAt(i);
			if (!IsFileName(sImg))
				sImg = sPath + sImg;
			if (!IsFileName(sImg))
				continue;

			CString sImgCID = mapiMsg.m_ImagesCIDs.GetAt(i);

			System::Web::Mail::MailAttachment^ attachment = nullptr;

			try
			{
				attachment = gcnew System::Web::Mail::MailAttachment(gcnew System::String(sImg));	
				mailMsg->Attachments->Add(attachment);
			}
			catch (System::Exception^ ex)
			{
				if (pDiagnostic)
					pDiagnostic->Add(CString(ex->Message) + sImg);
			}
		}

		System::String^ host	= gcnew System::String(params.GetHostName());
		System::String^ name	= gcnew System::String(params.GetUserName());
		System::String^ pwd		= gcnew System::String(params.GetPassword());
		int port				= params.GetPort();
		System::String^ sPort	= gcnew System::String(port.ToString());

		mailMsg->Fields->Add(gcnew System::String(L"http://schemas.microsoft.com/cdo/configuration/smtpserver"), host);
		mailMsg->Fields->Add(gcnew System::String(L"http://schemas.microsoft.com/cdo/configuration/smtpserverport"), sPort);
		mailMsg->Fields->Add(gcnew System::String(L"http://schemas.microsoft.com/cdo/configuration/sendusing"), gcnew System::String(L"2"));
		mailMsg->Fields->Add(gcnew System::String(L"http://schemas.microsoft.com/cdo/configuration/smtpauthenticate"), gcnew System::String(L"1"));
		mailMsg->Fields->Add(gcnew System::String(L"http://schemas.microsoft.com/cdo/configuration/sendusername"), name);
		mailMsg->Fields->Add(gcnew System::String(L"http://schemas.microsoft.com/cdo/configuration/sendpassword"), pwd);
		mailMsg->Fields->Add(gcnew System::String(L"http://schemas.microsoft.com/cdo/configuration/smtpusessl"), params.GetUseImplicitSSL() ? gcnew System::String(L"true") : gcnew System::String(L"false"));
 
		System::Web::Mail::SmtpMail::SmtpServer = host + gcnew System::String(L":") + port.ToString();
		System::Web::Mail::SmtpMail::Send(mailMsg);
		
		returnValue = TRUE;
	}
	catch (System::Exception^ ex)
	{
		if (pDiagnostic)
			pDiagnostic->Add(CString(ex->Message));
		delete ex;
	}
	catch (CException* pE)
	{
		if (pDiagnostic)
			pDiagnostic->Add(pE);
		pE->Delete();
	}
	catch (...)
	{
		if (pDiagnostic)
			pDiagnostic->Add(_TB("Internal error on sent email by smtp"));
	}
	//------------
	try
	{
		//bug #19139
		if (mailMsg != nullptr)
		{
			for each (System::Web::Mail::MailAttachment^ a in  mailMsg->Attachments)
			{
				delete(a);
			}
		}
	}
	catch (...)
	{}

	return returnValue;
}

//-----------------------------------------------------------------------------
