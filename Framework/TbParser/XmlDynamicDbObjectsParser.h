#pragma once

#include "XmlBaseDescriptionParser.h"

//includere alla fine degli include del .H
#include "beginh.dex"

class DynamicDbObjectsTable;
//----------------------------------------------------------------
class TB_EXPORT CXMLDatabaseObjectsParser : public CXMLBaseDescriptionParser
{
	DECLARE_DYNAMIC(CXMLDatabaseObjectsParser)

public:
	CXMLDatabaseObjectsParser();

public:
	BOOL LoadDatabaseObjects		(const CTBNamespace& aNamespace, DatabaseObjectsTable* pTable);

public:
	static BOOL ParseDynamicData
						(
							CDbObjectDescription* pDescri, 
							CXMLNodeChildsList* pChildNodes,
							const CTBNamespace& moduleNamespace,
							CAlterTableDescription* pAlterDescri
						);
private:
	BOOL Parse				(
								CXMLDocumentObject*		pDoc, 
								DatabaseObjectsTable*	pTable, 
								const CTBNamespace&		aParent
							);
	
	BOOL ParseGroup		(
								CXMLNode*				pNode, 
								DatabaseObjectsTable*	pTable, 
								const CTBNamespace&		aParent, 
								const CString&			sTag
						);
	
	BOOL ParseDbObject	(
								CXMLNode*				pNode,
								CDbObjectDescription*	pDescri, 
								const CTBNamespace&		aParent
						);

	static BOOL ParseFields(
								CXMLNode*				pNode,
								const CString&			sType,
								CDbObjectDescription*	pDescri,
								const CTBNamespace&		moduleNamespace,
								CAlterTableDescription* pAlterDescri,
								BOOL bIsAdditionalColumns
						);
};

#include "endh.dex"
