#pragma once

#include <TbParser\XmlBaseDescriptionParser.h>
#include <TbXmlCore\XMLDocObj.h>
#include <TbGeneric\ReferenceObjectsInfo.h>
//includere alla fine degli include del .H
#include "beginh.dex"

//----------------------------------------------------------------
class TB_EXPORT CXMLReferenceObjectsParserBase : public CObject
{
	DECLARE_DYNAMIC(CXMLReferenceObjectsParserBase)

public:
	virtual BOOL Parse	(CXMLDocumentObject*, CReferenceObjectsDescription*, const CTBNamespace&) = 0;
};

#include "endh.dex"
