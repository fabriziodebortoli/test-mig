
#pragma once


#include <TBOleDB\sqlrec.h>
#include <TBOleDB\sqltable.h>

#include "xmldatamng.h"

//includere alla fine degli include del .H
#include "beginh.dex"

/////////////////////////////////////////////////////////////////////////////
//	SqlRecord					### LOSTANDFOUND ###					
/////////////////////////////////////////////////////////////////////////////
//
/////////////////////////////////////////////////////////////////////////////
class TLostAndFound : public SqlRecord
{
	DECLARE_DYNCREATE(TLostAndFound) 

public:
	DataStr		f_Key;
	DataStr		f_TableName;
	DataStr		f_UniversalKey;
	DataStr		f_DocumentNamespace;
	DataDate	f_Date;

public:
	TLostAndFound(BOOL bCallInit = TRUE);

public:
    virtual void	BindRecord	();	
	
public:
	static  LPCTSTR  GetStaticName();
};

//////////////////////////////////////////////////////////////////////////////
class CDataManagerEvents : public CEventManager
{
	DECLARE_DYNCREATE(CDataManagerEvents);

public:
	BOOL CreateWhereClause	(CXMLNode *pNode, SqlTable* pTable);
	BOOL CreateWhereClause	(CXMLNodeChildsList *pNodeList, SqlTable* pTable);
	BOOL AddWhereClause		(const CString &strField, const CString &strValue, SqlTable* pTable);
	
	BOOL AssignNewFieldsToKey	(CXMLNodeChildsList *pKeyList, SqlRecord *pRec);
	BOOL AssignNewFieldsToUnKey	(CXMLNodeChildsList *pUnKeyList, SqlRecord *pRec);

	BOOL AssignKeyToRecord		(CXMLNodeChildsList *pKeyList, SqlRecord *pRec);
	BOOL AssignUnKeyToRecord	(CXMLNodeChildsList *pKeyList, SqlRecord *pRec);
	
	BOOL SelectKeyFromDocTable			(CXMLNodeChildsList *pKeyList, CXMLNodeChildsList *pUnKeyList);
	BOOL SelectUnKeyFromDocTable		(CXMLNodeChildsList *pKeyList,  CXMLNodeChildsList *pUnKeyList);

	BOOL InsertKeyIntoDocTable			(CXMLNodeChildsList *pKeyList, CXMLNodeChildsList *pUnKeyList);

	BOOL SelectKeyFromLostAndFound		(CXMLNodeChildsList *pKeyList,  CXMLNodeChildsList *pUnKeyList);
	BOOL InsertKeyIntoLostAndFound		(CXMLNodeChildsList *pKeyList,  CXMLNodeChildsList *pUnKeyList);
	BOOL DeleteKeyFromLostAndFound		(CXMLNodeChildsList *pKeyList);

	SqlRecord* SelectRecordFromDocTable (CXMLNodeChildsList *pKeyList);
	BOOL GetNewKey						(CXMLNodeChildsList *pKeyList);
	
	BOOL UniversalKeyToString			(CXMLNode *pNode, CString& strOut);
	BOOL StringToUniversalKey			(const CString& strIn, CXMLNode *pNode);
	BOOL KeyToString					(CXMLNodeChildsList *pNodeList, CString& strOut);
	BOOL StringToKey					(const CString& strIn, CXMLNodeChildsList *pNodeList);

	BOOL ExtractValue					(const CString& strToParse, const CString& strInputField, CString &strValue);
	BOOL AppendValue					(CStringArray& strArray, const CString& strField, const CString &strValue);
	
	CAbstractFormDoc*	GetDocument			();
	DBTMaster*			GetDBTMaster		();
	SqlRecord*			GetRuntimeRecord	();

	BOOL				CanGoOn				();
	virtual CClientDoc*	GetClientDoc		() {return NULL;}
};

//////////////////////////////////////////////////////////////////////////////
class CImportEvents : public CDataManagerEvents
{
	DECLARE_TB_EVENT_MAP ();
	DECLARE_DYNCREATE(CImportEvents);

	int					LostAndFound		(void* pInOut);
	int					AlternativeSearch	(void *pInOut);
	BOOL				RemoveFromLostAndFound (CXMLNode *pUniversalKey);
	virtual CClientDoc*	GetClientDoc		();

};

//////////////////////////////////////////////////////////////////////////////
class CExportEvents : public CDataManagerEvents
{
	DECLARE_TB_EVENT_MAP ();
	DECLARE_DYNCREATE(CExportEvents);

	int					LostAndFound		(void* pInOut);
	int					AlternativeSearch	(void *pInOut);

	virtual CClientDoc*	GetClientDoc		();

};
#include "endh.dex"
