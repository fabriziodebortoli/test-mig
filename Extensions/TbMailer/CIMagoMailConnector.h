
#pragma once

#include <TbNamesolver/ApplicationContext.h>
#include <TbGeneric/CMapi.h>

//includere alla fine degli include del .H
#include "beginh.dex"

class CDiagnostic;

//=============================================================================
class TB_EXPORT CIMagoMailConnector: public IMailConnector
{
public:
	virtual ~CIMagoMailConnector();

	virtual IMapiSession* NewMapiSession (BOOL bMultiThreaded = TRUE, BOOL bNoLogonUI = FALSE, BOOL bNoInitializeMAPI = FALSE, CDiagnostic* pDiagnostic = NULL);

	virtual BOOL SmtpSendMail 
		(
			CMapiMessage& msg,
			BOOL* bRequestDeliveryNotification = NULL, 
			BOOL* bRequestReadNotification = NULL,
			CDiagnostic* pDiagnostic = NULL
		);

	virtual BOOL ShowEmailDlg 		
		(
			CDocument* pCallerDoc,
			CMapiMessage& msg, 

			BOOL* pbAttachRDE = NULL, 
			BOOL* pbAttachPDF = NULL, 
			BOOL* pbCompressAttach = NULL,
			int*  = NULL,
			int*  = NULL,
			BOOL* pbConcatAttachPDF = NULL,
			BOOL* bRequestDeliveryNotification = NULL, 
			BOOL* bRequestReadNotification = NULL,
			LPCTSTR	pszCaptionOkBtn = NULL
		);

	virtual BOOL ShowEmailWithChildDlg		
		(
			BOOL* pbAttachRDE = NULL, 
			BOOL* pbAttachPDF = NULL, 
			BOOL* pbCompressAttach = NULL,
			BOOL* pbConcatAttachPDF = NULL
		);

	virtual BOOL Html2Mime (CMapiMessage& msgMapi);

	virtual BOOL MapiShowAddressBook(HWND hWnd, CString& strAddress, CDiagnostic* pDiagnostic = NULL);

	virtual BOOL SendMail 
		(
			CMapiMessage& msgMapi,
			BOOL* pbRequestDeliveryNotification = NULL,
			BOOL* pbRequestReadNotification = NULL,
			CDiagnostic* pDiagnostic = NULL
		);

	//used by EasyAttachment to send the archived documents as attachments
	virtual BOOL SendAsAttachments(CStringArray& arAttachmentsFiles, CStringArray& arAttachmentsTitles, CDiagnostic* pDiagnostic = NULL);

	void AttachToApplicationContext() { AfxGetApplicationContext()->AttachMailConnector(this); }

	virtual CString FormatFaxAddress(CString sFax);

	/*virtual BOOL IsPostaLiteEnabled();
	virtual BOOL PostaLiteEnqueueMsg(CMapiMessage* pMsg, SqlSession*, CDiagnostic* pDiagnostic = NULL);
	virtual BOOL PostaLitePdfMargins(CRect&);

	virtual BOOL ShowSendPostaLiteDlg 		
		(
			CDocument* pCallerDoc,
			CMapiMessage& msg
		);
	
	virtual DataEnum GetPostaLiteDefaultDeliveryType	();
	virtual DataEnum GetPostaLiteDefaultPrintType		();

	BOOL PostaLiteCheckCountry	(CMapiMessage* pMsg, CPostaLiteAddress* pAddr);
	void PostaLiteSentNotify	(CMapiMessage* pMsg, CPostaLiteAddress* pAddr, CString sDescription, int eIcon);

	virtual BOOL RotateLandscape	();*/
};

//=============================================================================
#include "endh.dex"
