#pragma once

#include <TbGenlib\XmlReferenceObjectsParser.h>

//includere alla fine degli include del .H
#include "beginh.dex"

//////////////////////////////////////////////////////////////////////////////
//             					CXMLReferenceObjectsParser
//////////////////////////////////////////////////////////////////////////////
//
class TB_EXPORT CXMLReferenceObjectsParser : public CXMLReferenceObjectsParserBase
{
	DECLARE_DYNCREATE(CXMLReferenceObjectsParser)

public:
	CXMLReferenceObjectsParser ();

public:
	virtual BOOL Parse	(CXMLDocumentObject*, CReferenceObjectsDescription*, const CTBNamespace&);
	BOOL ParseHotLink	(CXMLNode*, CHotlinkDescription*, const CTBNamespace&);
private:	

	BOOL ParseComboColumn	(CXMLNode*, CComboColumnDescription*, const CTBNamespace&);
};

#include "endh.dex"
