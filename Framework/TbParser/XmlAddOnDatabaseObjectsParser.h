#pragma once

#include "XmlBaseDescriptionParser.h"

//includere alla fine degli include del .H
#include "beginh.dex"

// AdditionalColumns/AlterTable
//----------------------------------------------------------------
class TB_EXPORT CXMLAlterTableDescriptionParser : public CXMLBaseDescriptionParser
{
	DECLARE_DYNAMIC(CXMLAlterTableDescriptionParser)

public:
	CXMLAlterTableDescriptionParser ();

public:
	virtual BOOL Parse		(CXMLNode*, CBaseDescription* pDescri, const CTBNamespace& aParent, const CString &strTableToAlter);
	virtual void Unparse	(CXMLNode*, CBaseDescription* pDescri, const CTBNamespace& aParent, const CString &strTableToAlter);
};

// AdditionalColumns/Table
//----------------------------------------------------------------
class TB_EXPORT CXMLAddColsTableDescriptionParser : public CXMLBaseDescriptionParser
{
	DECLARE_DYNAMIC(CXMLAddColsTableDescriptionParser)

private:
	CXMLAlterTableDescriptionParser	m_AlterTableParser;

public:
	CXMLAddColsTableDescriptionParser ();

public:
	virtual BOOL Parse		(CXMLNode*, CBaseDescription* pDescri, const CTBNamespace& aParent);
	virtual void Unparse	(CXMLNode*, CBaseDescription* pDescri, const CTBNamespace& aParent);
};

// lettura del file AddOnDatabaseObjects.xml. Non ho fatto la 
// Unparse perchè per ora mi servono raggruppate per library,
// in memoria mi tengo solo un sottoinsieme della struttura
//----------------------------------------------------------------
class TB_EXPORT CXMLAddOnDatabaseObjectsParser : public CObject
{
	DECLARE_DYNAMIC(CXMLAddOnDatabaseObjectsParser)

private:
	CXMLAddColsTableDescriptionParser	m_AddColsParser;

public:
	CXMLAddOnDatabaseObjectsParser();

private:
	BOOL Parse(CXMLDocumentObject*, const CTBNamespace& aParent);
	void Unparse(CXMLDocumentObject*, CAddColsTableDescription*, const CTBNamespace& aParent);
public:
	BOOL LoadAdddOnDatabaseObjects(const CTBNamespace& aModuleNS);	
	BOOL SaveAdddOnDatabaseObjects(CAlterTableDescriptionArray* pAlterTableDescription);
};
#include "endh.dex"
