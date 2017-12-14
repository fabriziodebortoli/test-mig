
#pragma once

#include <TBOLEDB\sqlrec.h>
#include <TBOLEDB\sqltable.h>


//includere alla fine degli include del .H
#include "beginh.dex"

#define DEFAULT_EXTENSION_LENGTH	4
#define MIN_EXTENSION_LENGTH		2
#define MAX_EXTENSION_LENGTH		8
#define PADDING_CHARACTER			_T("_")

/////////////////////////////////////////////////////////////////////////////
//	SqlRecord					### TXMLLatestKeyPrefix ###					
/////////////////////////////////////////////////////////////////////////////
//
/////////////////////////////////////////////////////////////////////////////
class TXMLKeyExtension : public SqlRecord
{
	DECLARE_DYNCREATE(TXMLKeyExtension) 

public:
	DataStr		f_FieldName;
	DataStr		f_TableName;
	DataInt		f_Extension;
	DataStr		f_DocumentNamespace;

public:
	TXMLKeyExtension(BOOL bCallInit = TRUE);

public:
    virtual void	BindRecord	();	
	
public:
	static  LPCTSTR  GetStaticName();
};


//////////////////////////////////////////////////////////////////////////////
class CCodeManager : public CObject 
{
	DECLARE_DYNCREATE(CCodeManager);

	CAbstractFormDoc	*m_pDocument;
	SqlRecord			*m_pRecord;
	SqlRecord			*m_pOriginalRecord;
	DataObj				*m_pOldDataObj;
	CString				m_StrCurrentField;
	CString				m_strCodeExtension;
	CXMLDocumentObject	*m_pDOM;

public:

	CCodeManager	(CAbstractFormDoc* pDoc = NULL, CString strCodeExtension = _T(""));
	~CCodeManager	();

	BOOL			GetNewKey();
	BOOL			GetNewFields(BOOL bOverwriteLockedFields = FALSE);

protected:
	BOOL			ApplyRules(DataObj* pDataObj, const CString& strColumnName);
	BOOL			ApplyRule(DataObj* pDataObj, CXMLNode *pRuleNode);
	BOOL			ApplySameCodeRule(DataObj* pDataObj);
	BOOL			ApplyCodeExtensionRule(DataObj* pDataObj, BOOL bBefore, int nLength, CString strExtension = _T(""));
	BOOL			ApplyNumberPaddingRule(DataObj* pDataObj, BOOL bBefore, int nLength);
	BOOL			CheckRecord();
};


#include "endh.dex"
