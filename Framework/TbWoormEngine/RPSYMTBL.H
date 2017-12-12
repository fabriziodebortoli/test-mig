
#pragma once

#include <TbGeneric\DataObj.h>
#include <TbParser\Parser.h>
#include <TbParser\SymTable.h>

#include <TbGeneric\RdeProtocol.h>

#include <TbGeneric\DataObj.h>

#include <TbParser\Parser.h>
#include <TbParser\SymTable.h>

#include "RepTable.h"

//includere alla fine degli include del .H
#include "beginh.dex"

class WoormTable;
class WoormField;



//============================================================================
//		class CStaticFieldArrayCache/CFieldCache
// serve a mantenere il valore dei campi delle regole di richiesta fra 
// run successivi dello stesso report (distinzioni per campi ReInit e Hidden Static)
//============================================================================
class TB_EXPORT CFieldCache: public CObject
{
	friend class CStaticFieldArrayCache;
public:
	CFieldCache(const CString&  strFieldName, WORD wFieldID, DataObj*  pDataObj) 
		: 
		m_strFieldName (strFieldName), 
		m_wFieldID (wFieldID),
		m_pDataObj (pDataObj)
	{} 

	CFieldCache() 
		: 
		m_wFieldID (0),
		m_pDataObj (NULL)
	{} 

	virtual 	~CFieldCache() 	{ if (m_pDataObj) delete m_pDataObj; } 

protected:
	CString		m_strFieldName;
	WORD		m_wFieldID;
	DataObj*    m_pDataObj;
};

///////////////////////////////////////////////////////////////////////////////
class TB_EXPORT CStaticFieldArrayCache : public Array
{
protected:
	int Add						(const CString& strName, WORD wID, DataObj* pValue);

public:
	CStaticFieldArrayCache() {}

	int CheckAndAddField		(WoormField* pRF);

	DataObj* GetFromName		(const CString& strName);
	DataObj* GetFromID			(WORD wID);

	BOOL RetrieveStaticValue	(WoormField* pRF);
	BOOL RetrieveStaticValue	(WORD wID, DataObj&);
	BOOL RetrieveArguments		(WoormTable* pWT, DataStr& sXml);

	CFieldCache* GetAt			(int nIdx)					{ return (CFieldCache*)Array::GetAt(nIdx); }

	CString GetName				(int nIdx)					{ return GetAt(nIdx)->m_strFieldName; }
	WORD GetID					(int nIdx)					{ return GetAt(nIdx)->m_wFieldID; }
	DataObj* GetData			(int nIdx)					{ return GetAt(nIdx)->m_pDataObj; }

	void RemoveAll				();
};    


#include "endh.dex"
