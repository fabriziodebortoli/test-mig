
#pragma once

#include <TbXmlCore\XmlDocObj.h>

#include "BeginH.dex"

//----------------------------------------------------------------------------
class TB_EXPORT CServerConnectionInfo : public CObject
{
public:
	CString		m_sPreferredLanguage;
	CString		m_sApplicationLanguage;

	int	m_nWebServicesPort;
	int	m_nWebServicesTimeout;
	int	m_nTBLoaderTimeOut;
	int	m_nsTbWCFDefaultTimeout;
	int	m_nsTbWCFDataTransferTimeout;


public:
	CServerConnectionInfo();

public:
	BOOL	Parse (const CString& sContent);
};

#include "EndH.dex"
