#pragma once

#include <TbGeneric/CMapi.h>

#include "beginh.dex"
class TB_EXPORT CSmtpNet
{
public:
	CSmtpNet(void);
	virtual ~CSmtpNet(void);

	virtual BOOL SendMail 
	(
		SmtpMailConnectorParams& params,
		CMapiMessage& msg,
		BOOL* bRequestDeliveryNotification = NULL, 
		BOOL* bRequestReadNotification = NULL,
		CDiagnostic* pDiagnostic = NULL
	);

	virtual BOOL SendMail2 
	(
		SmtpMailConnectorParams& params,
		CMapiMessage& msg,
		BOOL* bRequestDeliveryNotification = NULL, 
		BOOL* bRequestReadNotification = NULL,
		CDiagnostic* pDiagnostic = NULL
	);

};

#include "endh.dex"