#pragma once

#include "XmlBaseDescriptionParser.h"

//includere alla fine degli include del .H
#include "beginh.dex"

class COutDateObjectsDescription;

// grammatica degli oggetti outdate
//----------------------------------------------------------------
class TB_EXPORT CXMLOutDateObjectsParser : public CObject
{
	DECLARE_DYNAMIC(CXMLOutDateObjectsParser)

public:
	CXMLOutDateObjectsParser();
	~CXMLOutDateObjectsParser();

public:
	BOOL Load	(COutDateObjectsDescription*, const CTBNamespace& aModuleNs, CPathFinder* pPathFinder);

private:
	BOOL Parse				(
								CXMLDocumentObject*, 
								COutDateObjectsDescription*, 
								const CTBNamespace&
							);
	BOOL ParseObject		(
								CXMLNode*, 
								const CTBNamespace::NSObjectType&, 
								COutDateObjectDescription*, 
								const CTBNamespace&
							);
	BOOL ParseParamSection	(
								CXMLNode*, 
								COutDateSettingsSectionDescription*, 
								const CTBNamespace&
							);

	CString	OperatorToString (const COutDateObjectDescription::OutDateOperator&);
	COutDateObjectDescription::OutDateOperator StringToOperator(const CString&);
};

#include "endh.dex"
