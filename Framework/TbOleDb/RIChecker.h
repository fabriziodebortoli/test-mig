#pragma once

#include <TbGeneric\dataobj.h>

#include "beginh.dex"

class SqlRecord;
#define ROOT_RICHECKNODE_NAME _T("ROOT")

enum eFilterSetType {SET_EMPTY, SET_IN, SET_NOTIN};

//////////////////////////////////////////////////////////////////////////////////
//							 CheckerClass										//
//////////////////////////////////////////////////////////////////////////////////
//
//-------------------------------------------------------------------------------
class TB_EXPORT CheckerClass : public CObject
{		
	DECLARE_DYNCREATE(CheckerClass)

public:
	CheckerClass();
	virtual ~CheckerClass();

public:
	CRuntimeClass* m_pRuntimeClass;
	CString		   m_sQuery;
	CString		   m_sMasterTable;
};

//////////////////////////////////////////////////////////////////////////////////
//							 ReferentialIntegrityChecker						//
//////////////////////////////////////////////////////////////////////////////////
//
//-------------------------------------------------------------------------------
class TB_EXPORT ReferentialIntegrityChecker : public CObject
{	
	DECLARE_DYNAMIC (ReferentialIntegrityChecker)

public:
	ReferentialIntegrityChecker();
	virtual ~ReferentialIntegrityChecker();

	virtual DataStr	GetDocumentNamespace()																		= 0;	
	virtual BOOL	FindForValidation	(const SqlRecord* pSqlRec, DataStr& GUIDToFind, CStringArray& arMsgOut)	= 0;
};

//////////////////////////////////////////////////////////////////////////////////
//								RICheckNode										//
//////////////////////////////////////////////////////////////////////////////////
//
//-------------------------------------------------------------------------------
class TB_EXPORT RICheckNode : public CObject
{
	DECLARE_DYNCREATE(RICheckNode)

public:
	RICheckNode();
	virtual ~RICheckNode();

private:
	DataStr											m_Name;
	CArray<CheckerClass*,CheckerClass*>				m_RICheckerClassList;
	CArray<RICheckNode*, RICheckNode*>				m_Sons;

public:
	CString											m_xmlValidationFilters;
	CStringArray									m_NamespaceToSkip;
	CStringArray									m_NamespaceFilter_IN;
	CStringArray									m_NamespaceFilter_NOTIN;

public:
	virtual DataStr									GetName						()										{ return m_Name;}
	virtual void									SetName						(DataStr sName)							{ m_Name = sName;}

	virtual void									SetIsRoot					()										{ m_Name = ROOT_RICHECKNODE_NAME; }

	virtual CArray<CheckerClass*,CheckerClass*>&	GetRICheckerClassList		()										{ return m_RICheckerClassList;}
	virtual CArray<RICheckNode*,RICheckNode*>&		GetSons						()										{ return m_Sons;}
	virtual BOOL									IsRoot						()										{ return m_Name.Str().CompareNoCase(ROOT_RICHECKNODE_NAME) == 0;}
	virtual BOOL									LookUp						(DataStr sName, RICheckNode*& pNode);
	virtual BOOL									IsValid						(const SqlRecord* pSqlRec, CStringArray& arMsgTot);
	virtual CString									Serialize					();
	virtual void									SetXMLNode					(CXMLNode* pXMLNode, RICheckNode* pCheckNode, eFilterSetType setType = SET_EMPTY);
	virtual CString									SerializeErrors				(const CStringArray& arMsg);
	virtual CString									DisplayErrors				(const CString& sSerializedErrors);
	virtual void									DeserializeValidationFilters(CStringArray& namespaceToSkip, CStringArray& namespaceFilter_IN, CStringArray& namespaceFilter_NOTIN);
	virtual BOOL									DoTestCheckNode				(DataStr sParentName, DataStr sName, const CStringArray& arNamespace);
	virtual BOOL									IsCheckNodeToSkip			(DataStr sParentName, DataStr sName, const CStringArray& namespaceToSkip);
	virtual BOOL									IsCheckNodeFilter_IN		(DataStr sParentName, DataStr sName, const CStringArray& namespaceFilter_IN);
	virtual BOOL									IsCheckNodeFilter_NOTIN		(DataStr sParentName, DataStr sName, const CStringArray& namespaceFilter_NOTIN);
};

//////////////////////////////////////////////////////////////////////////////////
//								RICheckNodeFactory								//
//////////////////////////////////////////////////////////////////////////////////
//
//-------------------------------------------------------------------------------
class TB_EXPORT RICheckNodeFactory
{
public:
	virtual ~RICheckNodeFactory() {};

public:
	static RICheckNode* CreateRoot				();
	static RICheckNode* CreateNode				();
	static void			FillRICheckerStructure	(RICheckNode& pParent, RICheckNode*& pCurrNode, const CString& documentNamespace, const CArray<CheckerClass*, CheckerClass*>& rIcheckerList);
	static void			FillRICheckerStructure	(RICheckNode& pParent);
};

//===============================================================================
TB_EXPORT RICheckNode* AfxGetRICheckNodeManager(CString pProviderName);

#include "endh.dex"