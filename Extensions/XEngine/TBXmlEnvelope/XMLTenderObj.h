#pragma once

#include <TBgenlib\baseapp.h>

#include "XTender.h"

//includere alla fine degli include del .H
#include "beginh.dex"

struct	IXTender;
class	TXEParameters;

#define ENVELOPE_TO_IMPORT_EXIST		0
#define ENVELOPE_TO_IMPORT_NO_EXIST		1
#define ENVELOPE_TO_IMPORT_ERROR		2

//----------------------------------------------------------------
//class CXMLTender 
//----------------------------------------------------------------
class TB_EXPORT CXTender
{
	friend class DXEParameters;
	friend class XEngineObject;

private:
	IXTender*	m_pIXTender;
	BOOL		m_bCreateFailed;

public:
	CXTender	();
	~CXTender	();

private:
	BOOL	Initialize			();
	void	Close				();
	BOOL	InitParameter		();
	BOOL	SetRepositoyProperty(); 
	BOOL	SetParameter		(TXEParameters*);

public:
	BOOL	GetAvailableEnvelope	(const CString& strEnvClass, const CString& strEnvSite, DWORD& dwThread);
	BOOL	GetAvailableSites		(CStringArray* pSites, const CString& strEnvClass = _T(""));	// restituisce la lista dei site da cui posso ricevere 
																									// Eventualmente anche su una singola EnvelopeClass
	CString GetLastError			();
	long	GetSupMaxTime			();
	long	GetInfMaxTime			();
	long	GetInfMaxAttempts		();
	long	GetSupMaxAttempts		();
	CString GetAppRoot				();
	BOOL	PutAppRoot				(const CString&);
	CString GetRepositoryServerURL	();
	BOOL	PutRepositoryServerURL	(const CString&);
	CString GetDomain				();
	BOOL	PutDomain				(const CString&);
	CString GetSite					();
	BOOL	PutSite					(const CString&);
	CString GetRXSubpath			();
	BOOL	PutRXSubpath			(const CString&);
	CString GetTXSubpath			();
	BOOL	PutTXSubpath			(const CString&);
	CString GetPendingSubpath		();
	BOOL	PutPendingSubpath		(const CString&);
	CString GetFailureSubpath		();
	BOOL	PutFailureSubpath		(const CString&);
	CString GetSuccessSubpath		();
	BOOL	PutSuccessSubpath		(const CString&);
	CString GetUserID				();
	BOOL	PutUserID				(const CString&);
	CString GetPassword				();
	BOOL	PutPassword				(const CString&);
	long	GetMaxTime				();
	BOOL	PutMaxTime				(long);
	long	GetMaxAttempts			();
	BOOL	PutMaxAttempts			(long);
	long	GetCompressSize			();
	BOOL	PutCompressSize			(long);

	CString GetXSLFile				();
	BOOL	PutXSLFile				(const CString&);
	
	EncodingType	GetEncoding		();
	BOOL			PutEncoding		(EncodingType eType);

	BOOL	SendEnvelope			(const CString&, const CString&, DWORD& dwThread, BOOL = FALSE, const CString& = _T(""));
	BOOL	SendPendingEnvelopes	();

	CString	GetDescription			(DWORD);
	float	GetEnvelopeProgress		(DWORD);
	float	GetClassProgress		(DWORD);
	CString	GetCurrentEnvelope		(DWORD);
	CString	GetCurrentFile			(DWORD);
	CString	GetCurrentClass			(DWORD);
	CString	GetCurrentSenderSite	(DWORD);
	
	BOOL	Disconnect				(long);
	BOOL	Connect					(IUnknown*, long*);
	BOOL	CanDestroyTender		();
	BOOL	IsThreadAlive			(long dwThreadID);
	BOOL	StopThread				(long dwThreadID);
	HANDLE  GetThreadHandle			(long dwThreadID);

	BOOL	IsTenderIstantiated		() { return !m_bCreateFailed;}
	BOOL	IsTenderActivated		() ;

	BOOL	TestURL					(CString& strLastError);
};

#include "endh.dex"
