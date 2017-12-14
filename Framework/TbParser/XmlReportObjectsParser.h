#pragma once

#include "XmlBaseDescriptionParser.h"

//includere alla fine degli include del .H
#include "beginh.dex"


//----------------------------------------------------------------
class TB_EXPORT CXMLDocumentReportParser : public CObject
{
	DECLARE_DYNCREATE(CXMLDocumentReportParser)

private:
	CXMLBaseDescriptionParser	m_BaseParser;

public:
	CXMLDocumentReportParser ();

public:
	virtual BOOL Parse			(CXMLNode*, CBaseDescription*, const CTBNamespace&, Array* pGroupArray = NULL);
	virtual void Unparse		(CXMLNode*, CBaseDescription*);
};


// grammatica delle funzioni
//----------------------------------------------------------------
class TB_EXPORT CXMLReportObjectsParser : public CObject
{
	DECLARE_DYNAMIC(CXMLReportObjectsParser)

private:
	CXMLDocumentReportParser	m_ObjectParser;
	CTBNamespace				m_NsDefault;
	CTBNamespace				m_CountryNsDefault;
	

public:
	CXMLReportObjectsParser();

public:
	BOOL Parse	(CXMLDocumentObject*, CReportObjectsDescription*, const CTBNamespace&, CTBNamespace& NsDefault);
	void Unparse(CXMLDocumentObject*, CReportObjectsDescription*, CDocumentReportDescription* pDefault = NULL);

private:
	BOOL ParseDefaultReport	(CXMLNode* pNode, CReportObjectsDescription* pReportObjects);
};

#include "endh.dex"
