
#include "stdafx.h"
#include <TbNameSolver\FileSystemFunctions.h>
#include <TbGenlibManaged\Main.h>
#include "ServerConnectionInfo.h"


//includere come ultimo include all'inizio del cpp
#include "begincpp.dex"

static const TCHAR szXmlOslConnection[]				= _T("OslConnectionString");
static const TCHAR szXmlApplicationServerPort[]		= _T("ApplicationServerPort");
static const TCHAR szXmlPreferredLanguage[]			= _T("PreferredLanguage");
static const TCHAR szXmlApplicationLanguage[]		= _T("ApplicationLanguage");
static const TCHAR szXmlValue[]						= _T("value");
static const TCHAR szXmlWebServicesPort[]			= _T("WebServicesPort");
static const TCHAR szXmlWebServicesTimeOut[]		= _T("WebServicesTimeOut");
static const TCHAR szXmlTBLoaderTimeOut[]			= _T("TBLoaderTimeOut");
static const TCHAR szXmlTbWCFDefaultTimeout[]		= _T("TbWCFDefaultTimeout");
static const TCHAR szXmlTbWCFDataTransferTimeout[]	= _T("TbWCFDataTransferTimeout");

//----------------------------------------------------------------------------
CServerConnectionInfo::CServerConnectionInfo()
	:
	m_nWebServicesPort(80),
	m_nWebServicesTimeout(-1),
	m_nTBLoaderTimeOut(-1),
	m_nsTbWCFDefaultTimeout(1),
	m_nsTbWCFDataTransferTimeout(20)
{
}

//----------------------------------------------------------------------------
BOOL CServerConnectionInfo::Parse(const CString& sContent)
{
	CXMLDocumentObject doc;

	if (!doc.LoadXML(sContent))
		return FALSE;

	CXMLNode * pRoot = doc.GetRoot();
	if (!pRoot)
		return FALSE;

	CXMLNode* pNode;
	pNode = pRoot->GetChildByName(szXmlPreferredLanguage);
	if (pNode)
	{
		pNode->GetAttribute(szXmlValue, m_sPreferredLanguage);
	}
	pNode = pRoot->GetChildByName(szXmlApplicationLanguage);
	if (pNode)
	{
		pNode->GetAttribute(szXmlValue, m_sApplicationLanguage);
	}

	CString sTmp;
	pNode = pRoot->GetChildByName(szXmlWebServicesPort);
	if (pNode)
		pNode->GetAttribute(szXmlValue, sTmp);

	if (!sTmp.IsEmpty())
	{
		m_nWebServicesPort = _ttoi(sTmp);
		if (m_nWebServicesPort == 0)
			m_nWebServicesPort = 80;
	}
	
	pNode = pRoot->GetChildByName(szXmlWebServicesTimeOut);
	if (pNode)
		pNode->GetAttribute(szXmlValue, sTmp);

	if (!sTmp.IsEmpty())
	{
		m_nWebServicesTimeout = _ttoi(sTmp);
		if (m_nWebServicesTimeout == 0)
			m_nWebServicesTimeout = -1;
	}

	pNode = pRoot->GetChildByName(szXmlTBLoaderTimeOut);
	if (pNode)
		pNode->GetText(sTmp);

	if (!sTmp.IsEmpty())
	{
		m_nTBLoaderTimeOut = _ttoi(sTmp);
		if (m_nTBLoaderTimeOut == 0)
			m_nTBLoaderTimeOut = -1;
	}

	pNode = pRoot->GetChildByName(szXmlTbWCFDefaultTimeout);
	if (pNode)
		pNode->GetText(sTmp);

	if (!sTmp.IsEmpty())
	{
		m_nsTbWCFDefaultTimeout = _ttoi(sTmp);
		if (m_nsTbWCFDefaultTimeout == 0)
			m_nsTbWCFDefaultTimeout = 1;
	}

	pNode = pRoot->GetChildByName(szXmlTbWCFDataTransferTimeout);
	if (pNode)
		pNode->GetText(sTmp);

	if (!sTmp.IsEmpty())
	{
		m_nsTbWCFDataTransferTimeout = _ttoi(sTmp);
		if (m_nsTbWCFDataTransferTimeout == 0)
			m_nsTbWCFDataTransferTimeout = 20;
	}
	
	//imposto il timeout sulla dll di piu' basso livello che non riesce ad accedere al questa informazione
	SetWebServicesTimeout(m_nWebServicesTimeout);
	
	return TRUE;
}