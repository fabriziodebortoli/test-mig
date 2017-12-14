#include "stdafx.h"
#ifdef new
#undef new
#endif

#include <stdio.h>
#include <string.h>
#include <direct.h>
#include <locale.h> 

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

#include "Email.h"
#include "PostaLiteSettings.h"
#include "PostaLiteTables.h"

#include <TbGenlibManaged\PostaLiteNet.h>

#include "tbmailer.hjson" //JSON AUTOMATIC UPDATE

#ifdef _DEBUG
#define new DEBUG_NEW

#undef THIS_FILE
static const char THIS_FILE[] = __FILE__;
#endif

//=============================================================================
CMailConnector::~CMailConnector()
{
}

//-----------------------------------------------------------------------------
IMapiSession* CMailConnector::NewMapiSession(BOOL bMultiThreaded /*= TRUE*/, BOOL bNoLogonUI /*= FALSE*/, BOOL bNoInitializeMAPI /*= FALSE*/, CDiagnostic* pDiagnostic /*= NULL*/)
{
	return ::NewMapiSession(bMultiThreaded, bNoLogonUI, bNoInitializeMAPI, pDiagnostic);
}

//-----------------------------------------------------------------------------
BOOL CMailConnector::ShowEmailDlg	
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
			sFrom += smtpParams.GetFromAddress ();

		smtpParams.SetCurrentSection(CMapiMessage::TAG_CERTIFIED);

		CString sCF = smtpParams.GetFromAddress ();
		if (!sCF.IsEmpty())
		{
			sFrom += L";[C] ";
			
			CString sN = smtpParams.GetFromName(); sN.Trim();
			if (!sN.IsEmpty())
				sFrom += '<' + sN + '>';

			sFrom += sCF;
		}
	}

	CEMailDlg dlg
		(
			pCallerDoc,
			msg, 
			pbAttachRDE, pbAttachPDF, pbCompressAttach, 
			pbConcatAttachPDF, 
			pbRequestDeliveryNotification, pbRequestReadNotification, 
			pszCaptionOkBtn,
			sFrom
		);

	return dlg.DoModal() == IDOK;
}

//-----------------------------------------------------------------------------
BOOL CMailConnector::ShowEmailWithChildDlg	
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
BOOL CMailConnector::Html2Mime (CMapiMessage& msg)
{
	return ::Html2Mime (msg);
}

//-----------------------------------------------------------------------------
BOOL CMailConnector::MapiShowAddressBook(HWND hWnd, CString& strAddress, CDiagnostic* pDiagnostic /*= NULL*/)
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
BOOL CMailConnector::SendAsAttachments(CStringArray& arAttachmentsFiles, CStringArray& arAttachmentsTitles, CDiagnostic* pDiagnostic /*= NULL*/)
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

///////////////////////////////////////////////////////////////////////////////
// Converte un carattere hex nel valore
int HexToDigit(TCHAR i)
{
	return (i >= '0' && i <= '9') ? (i - '0') : (_totupper(i) - 'A' + 10);	
}

///////////////////////////////////////////////////////////////////////////////
// Converte un carattere hex nel valore
BOOL IsHexDigit(TCHAR i)
{
	return (i >= '0' && i <= '9') || (_totupper(i) >= 'A' && _totupper(i) <= 'F');	
}


///////////////////////////////////////////////////////////////////////////////
// Convert HTML file to MIME compliant file for SMTP transmission (convert <img src=... e <a href=... tags)
//
BOOL Html2Mime (CMapiMessage& msgMapi)
{
	//-------------------------------------------------------------------------
	class Streamer
	{
	public:
		CString	m_strInputHtml;
		int		m_len;
		int		m_idx;

		Streamer	(CMapiMessage& amsgMapi)
			:
			m_len (-1),
			m_idx (0)
		{
			if (amsgMapi.m_bHtmlFromFile)
			{
				CString strName = amsgMapi.m_sHtml;
				amsgMapi.m_sHtml.Empty();
				if (!LoadLineTextFile (strName, m_strInputHtml))
					return;

				m_len = m_strInputHtml.GetLength();

				if (m_len > 0 && amsgMapi.m_Images.GetSize() == 0)
				{
					// se vengono recuperati i file automaticamente
					// ci si sposta nella dir del html per gestire il caso di 
					// path relative
					//
					TCHAR szDrive  [_MAX_DRIVE];
					TCHAR szDir    [_MAX_DIR];
					TCHAR szFile   [_MAX_FNAME];
					TCHAR szExt    [_MAX_EXT];
					
					_tsplitpath_s(strName, szDrive, _MAX_DRIVE, szDir, _MAX_DIR, szFile, _MAX_FNAME, szExt, _MAX_EXT);

					_chdrive(_totupper( szDrive[0] ) - 'A' + 1);
					_tchdir(szDir);
				}
			}
			else
			{
				m_strInputHtml = amsgMapi.m_sHtml.IsEmpty() ? amsgMapi.m_sBody : amsgMapi.m_sHtml;
				m_len = m_strInputHtml.GetLength();
			}
		}

		TCHAR getc()
		{
			if (m_len > 0)
			{
				m_len--;
				return m_strInputHtml[m_idx++];
			}

			return 0;
		}
	}; //class Streamer -------------------------------------------------------

	if (!msgMapi.m_bNeedEncoding || msgMapi.m_bAlreadyEncoded)
	{
		msgMapi.m_bNeedEncoding = FALSE;
		return TRUE;
	}

	msgMapi.m_Images.RemoveAll();
	msgMapi.m_ImagesCIDs.RemoveAll();

	Streamer stream(msgMapi);
	if (stream.m_len < 0)
	{
		TRACE(_T("Invalid stream\n"));
		ASSERT(FALSE);
		return FALSE;			// open error
	}

	TCHAR szDrive  [_MAX_DRIVE];
	TCHAR szDir    [_MAX_DIR];
	TCHAR szFile   [_MAX_FNAME];
	TCHAR szExt    [_MAX_EXT];

	CString strEncodedHtml;

	TCHAR imagename[2048], buffer[2048], basename[_MAX_FNAME+1];
	TCHAR data ;

	int html = 0, status = 0;

	// deve caricare la lista delle immagini referenziate nel html
	BOOL bFillImages = msgMapi.m_Images.GetSize() == 0;

	while ((data = stream.getc()) != 0)
	{
		if (bFillImages)
			switch (status)
			{	
				case 0:
					if (html == 0)				// Are we within a tag ? No
					{
						if (data == '<')				// Check for start tag
							html = 1;
					}
					else						// Are we within a tag ? Yes
					{
						if ((data == 's') || (data == 'S'))		// Check for <img Src=... tag
							status = 1;
						if ((data == 'h') || (data == 'H'))		// Check for <a Href=... tag
							status = -1;
						if (data == '>')				// Check for end tag
							html = 0;
					}
					break;
				case 1:
					if ((data == 'r') || (data == 'R'))		// Check for <img SRc=... tag
						status = 2;
					else
						status = 0;
					break;
				case 2:
					if ((data == 'c') || (data == 'C'))		// Check for <img SRC=... tag
						status = -4;
					else
						status = 0;
					break;
				case -1:
					if ((data == 'r') || (data == 'R'))		// Check for <a HRef=... tag
						status = -2;
					else
						status = 0;
					break;
				case -2:
					if ((data == 'e') || (data == 'E'))		// Check for <a HREf=... tag
						status = -3;
					else
						status = 0;
					break;
				case -3:
					if ((data == 'f') || (data == 'F'))		// Check for <a HREF=... tag
						status = -4;
					else
						status = 0;
					break;
				case -4:
					status = 0;					// Found <img SRC=... or <a HREF=... tag
					while (data == ' ')			// Drop leading spaces (ex. <img src    =...)
						data = stream.getc();

					if (data != '=')			// Now we must have =
					{
						TRACE(_T("Missing 'equal'\n"));
						goto emitch;
					}

					strEncodedHtml += data;		// Se necessario trasformarlo in quoted-printable code

					do			// Drop leading spaces (ex. <img src=    ...)
					{
						if ((data = stream.getc()) == 0)
						{
							TRACE(_T("EOF encountered\n"));
							goto emitch;
						}
					}
					while (data == ' ');

					if (data == '\"')				// If parameter start with a quote
					{
						strEncodedHtml += data;

						if ((data = stream.getc()) == 0)
						{
							TRACE(_T("EOF encountered\n"));
							goto emitch;
						}
						int i = 0;
						for (; (data != 0) && (data != '\"') && (i < _MAX_PATH); i++)
						{
							imagename [i] = data;			// Save parameter
							data = stream.getc();
						}
						imagename [i] = '\0';

						if (data != '\"')				// Check parameter for quote termination
						{
							TRACE(_T("Missing 'double quote' for %s\n"), imagename);
							goto emitimg;
						}
					}
					else // If parameter start without a quote
					{
						int i = 0;
						for (; (data != 0) && (data != ' ') && (data != '>') && (i < _MAX_PATH); i++)
						{
							imagename [i] = data;			// Save parameter
							data = stream.getc();
						}
						imagename [i] = '\0';

						if ((data != ' ') && (data != '>'))	// Check parameter for space termination or end tag
						{
							TRACE(_T("Missing 'terminator' for %s\n"), imagename);
							goto emitimg;
						}

						if (data == '>')				// If end tag, from now we are not within a tag
							html = 0;
					}

					if (_tcschr (imagename, '#') == NULL)
					{
						TB_TCSCPY (buffer, imagename);
						_tcsupr_s (buffer, _tcslen(imagename) + 1);				// Convert parameter all to upper case

						LPCTSTR ptr = _tcschr (buffer, ':');
						if (ptr != NULL)
							// Check for implicit or explicit file declaration
							if (_tcsstr (buffer, _T("FILE:")) != NULL)
								TB_TCSCPY (buffer, imagename + 5);		// Drop initial "file:" if explicit file declaration
							else
								if (ptr == &buffer[1])			// <drive>:
									TB_TCSCPY (buffer, imagename);		// Else copy implicit file declaration
								else
									goto emitimg;				// altri casi (http: ftp:....)
						else
							TB_TCSCPY (buffer, imagename);		// Else copy implicit file declaration
						
						int ii = 0;
						for (int i = 0; i < (int)_tcslen(buffer); i++)
							if (buffer[i] == '%' && IsHexDigit(buffer[i + 1])	&& IsHexDigit(buffer[i + 2]))
								buffer[ii++] = (HexToDigit(buffer[++i]) << 4) + HexToDigit(buffer[++i]);
							else
								buffer[ii++] = buffer[i];
						buffer[ii] = '\0';

						_tsplitpath_s(buffer, szDrive, _MAX_DRIVE, szDir, _MAX_DIR, szFile, _MAX_FNAME, szExt, _MAX_EXT);
						if	(
								_tcsicmp(szExt, _T(".JPG"))	!= 0 &&
								_tcsicmp(szExt, _T(".JPEG"))!= 0 &&
								_tcsicmp(szExt, _T(".GIF"))	!= 0 &&
								_tcsicmp(szExt, _T(".TIFF"))!= 0 &&
								_tcsicmp(szExt, _T(".TIF"))!= 0 &&
								_tcsicmp(szExt, _T(".TGA"))!= 0 &&
								_tcsicmp(szExt, _T(".BMP"))!= 0 &&
								_tcsicmp(szExt, _T(".ICO"))!= 0 &&
								_tcsicmp(szExt, _T(".ICON"))!= 0 &&
								_tcsicmp(szExt, _T(".EXIF"))!= 0 &&
								_tcsicmp(szExt, _T(".WMF"))!= 0 &&
								_tcsicmp(szExt, _T(".EMF"))!= 0 &&
								_tcsicmp(szExt, _T(".PNG"))	!= 0
							)
							goto emitimg;

						// Search for already referenced filename
						int index = 0;
						for (; index < msgMapi.m_Images.GetSize(); index++)
							if (msgMapi.m_Images.GetAt(index).CompareNoCase(buffer) == 0)
								break;	// found

						if (index < msgMapi.m_Images.GetSize())
							TB_TCSCPY(basename, msgMapi.m_ImagesCIDs.GetAt(index));	// found
						else
						{
							TB_TCSCPY (basename, szDir);
							TB_TCSCAT (basename, szFile);
							TB_TCSCAT (basename, szExt);
							TB_TCSCAT (basename, cwsprintf(_T("@%08X"), stream.m_len)); // unique place-holder with a positional reference

							// since basename belongs from a filename we substitute eventual blank
							for (int i = 0; i < (int)_tcslen(basename); i++)
								if (basename[i] == ' ') basename[i] = '_';

							msgMapi.SetImage(buffer, basename);
						}

						TB_TCSCPY (imagename, _T("cid:"));		// Set initial "cid:" attached file declaration
						TB_TCSCAT (imagename, basename);			// Add only filename without path

						strEncodedHtml += imagename;

						break;	// exit from case -4
					}
emitimg:
					for (int i = 0; i < (int)_tcslen(imagename); i++)
						strEncodedHtml += imagename[i];		// Se necessario trasformarlo in quoted-printable code

					break;	// exit from case -4
			}	// If char is . and = convert it to mime compliant

emitch:
		if (data != 0)
			strEncodedHtml += data;		// Se necessario trasformarlo in quoted-printable code
	}
	
	msgMapi.SetHtml(strEncodedHtml, FALSE, FALSE);
	msgMapi.m_bAlreadyEncoded = TRUE;

	return TRUE;
}

//=============================================================================
BOOL CMailConnector::SmtpSendMail 
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

BOOL CMailConnector::SendMail 
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

	if (params->GetUseMapi())
	{
		//elimino eventuali TAG dagli indirizzi
		msg.SplitAllAddress();	//un singolo indirizzo per ogni elemento dell'array 

		if (msg.m_deEmailAddressType == E_EMAIL_ADDRESS_TYPE_NORMAL)
		{
			CStringArray_RemoveWhenPrefixed		(msg.m_To, CMapiMessage::TAG_CERTIFIED);
			CStringArray_RemoveWhenPrefixed		(msg.m_CC, CMapiMessage::TAG_CERTIFIED);
			CStringArray_RemoveWhenPrefixed		(msg.m_BCC, CMapiMessage::TAG_CERTIFIED);
		}
		else if (msg.m_deEmailAddressType == E_EMAIL_ADDRESS_TYPE_CERTIFIED)
		{
			CStringArray_RemoveWhenNotPrefixed	(msg.m_To, CMapiMessage::TAG_CERTIFIED);
			CStringArray_RemoveWhenNotPrefixed	(msg.m_CC, CMapiMessage::TAG_CERTIFIED);
			//CStringArray_RemoveWhenNotPrefixed	(msg.m_BCC, CMapiMessage::TAG_CERTIFIED);
			msg.m_BCC.RemoveAll();	//Non ammessi
		}

		CStringArray_RemovePrefix	(msg.m_To, CMapiMessage::TAG_CERTIFIED);
		CStringArray_RemovePrefix	(msg.m_CC, CMapiMessage::TAG_CERTIFIED);
		CStringArray_RemovePrefix	(msg.m_BCC, CMapiMessage::TAG_CERTIFIED);
		//----

		CString strLocal = ::_tsetlocale (LC_ALL, NULL);

		if (msg.m_sMapiProfile.IsEmpty())
			msg.SetMapiProfile (params->GetOutlookProfile());

		IMapiSession* pMSession = NULL;
		try
		{
			bOk = FALSE;
			IMapiSession* pMSession = AfxGetIMailConnector()->NewMapiSession(TRUE, FALSE, params->GetSupportOutlookExpress());
			if (pMSession != NULL)
			{
				if (!pMSession->MapiInstalled())
				{
					if (pDiagnostic)
						pDiagnostic->Add(CString(_TB("Mapi are not installed")));
				}
				else if (!pMSession->Logon(msg.m_sMapiProfile))
				{
					if (pDiagnostic)
						pDiagnostic->Add(CString(_TB("Failed to Mapi Logon")));
				}
				else if (!pMSession->Send(msg))
				{
					if (pDiagnostic)
						pDiagnostic->Add(CString(_TB("Failed to send email by Mapi")));						
				}
				else 
					bOk = TRUE;
				delete pMSession;
			}
			else if (pDiagnostic)
				pDiagnostic->Add(CString(_TB("Failed to open Mapi Session")));

		}
		catch (CException* pE)
		{
			bOk = FALSE;
			if (pDiagnostic)
				pDiagnostic->Add(pE);
			pE->Delete();
		}
		SAFE_DELETE(pMSession);

		CString strLocal2 = ::_tsetlocale (LC_ALL, NULL);
		if (strLocal != strLocal2)
		{
			::_tsetlocale (LC_ALL, strLocal);
		}
	}
	else //if (params->GetUseSmtp())
	{
		if (!SmtpSendMail (msg, pbRequestDeliveryNotification, pbRequestReadNotification, pDiagnostic))
		{
			bOk = FALSE;
		}
	}

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
CString CMailConnector::FormatFaxAddress(CString sFax) 
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

