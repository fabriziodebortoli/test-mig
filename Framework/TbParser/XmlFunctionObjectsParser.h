#pragma once

#include "XmlBaseDescriptionParser.h"

//includere alla fine degli include del .H
#include "beginh.dex"


// grammatica delle funzioni
//----------------------------------------------------------------
class TB_EXPORT CXMLFunctionObjectsParser : public CObject
{
	DECLARE_DYNAMIC(CXMLFunctionObjectsParser)

	// consente di cambiare namespace alla struttura
	CTBNamespace::NSObjectType	m_NSType;

public:
	CXMLFunctionObjectsParser();

public:
	virtual	BOOL Parse	(CXMLDocumentObject*,	CFunctionObjectsDescription*, const CTBNamespace&, BOOL bSkipDuplicate);
	virtual	BOOL Parse1	(CXMLNode*,				CFunctionObjectsDescription*, const CTBNamespace&, BOOL bSkipDuplicate);
	virtual BOOL Parse2 (CXMLNode* pFunNode, CXMLNode*,				CFunctionObjectsDescription*, const CTBNamespace&, BOOL bSkipDuplicate);

	virtual void Unparse(CXMLDocumentObject*,	CFunctionObjectsDescription*);

	void SetFunctionType	(const CTBNamespace::NSObjectType aType);

	virtual CFunctionDescription* NewFuncDescr(CTBNamespace::NSObjectType aNSType);
};


// grammatica delle funzioni WOORM
//----------------------------------------------------------------
class TB_EXPORT CInternalFunctionObjectsParser : public CXMLFunctionObjectsParser
{
	DECLARE_DYNAMIC(CInternalFunctionObjectsParser)
public:
	class CGroupFunctions: public CObject
	{
	public:
		CString m_sName;
		CString  m_sTitle;
		CFunctionObjectsDescription* m_parFunctions;

		CGroupFunctions(CString sName, CString sTitle, CFunctionObjectsDescription* parFunctions)
			 :
				m_sName (sName),
				m_sTitle(sTitle),
				m_parFunctions (parFunctions)
			{}
		virtual ~CGroupFunctions() { SAFE_DELETE(m_parFunctions); }
	};

	CInternalFunctionObjectsParser ();
	virtual ~CInternalFunctionObjectsParser();

	CArray<CGroupFunctions*, CGroupFunctions*> m_arFunctionGroups;

public:
	virtual	BOOL Parse1(CXMLNode*, CFunctionObjectsDescription*, const CTBNamespace&, BOOL bSkipDuplicate);

	virtual CFunctionDescription* NewFuncDescr(CTBNamespace::NSObjectType aNSType);

	BOOL Load();
};

#include "endh.dex"
