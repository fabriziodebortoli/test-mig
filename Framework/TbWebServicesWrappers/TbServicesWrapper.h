#pragma once

#include "Beginh.dex"

//----------------------------------------------------------------------------
class TB_EXPORT TbServicesWrapper : public CObject
{
private:
	const CString	m_strService;			// nome del WEB service (se esterno)
	const CString	m_strServiceNamespace;	// namespace del WEB service (se esterno)
	const CString	m_strServer;			// nome del server del WEB service (se esterno)
	const int		m_nWebServicesPort;		// numero di porta di IIS

public:
	TbServicesWrapper(const CString& strService, const CString& strServiceNamespace, const CString& strServer, int nWebServicesPort);
	~TbServicesWrapper(void);

public:
	void CloseTb(const CString& authenticationToken);
};

#include "Endh.dex"