#include "StdAfx.h"

#include <AfxTempl.h>
#include <AfxOle.h>

#include <tboledb\sqlrec.h>
#include <tbges\extdoc.h>
#include <tbges\dbt.h>

// local
#include "TBEDataCoDec.h"

//includere alla fine degli include del .H
#include "beginh.dex"

#ifdef _DEBUG
#define new DEBUG_NEW
#endif

///////////////////////////////////////////////////////////////////////////////
// class CTBEDataCodecMap
//-----------------------------------------------------------------------------
IMPLEMENT_DYNAMIC(CTBEDataCodecMap, CObject)
//-----------------------------------------------------------------------------
CTBEDataCodecMap::_Item::~_Item()
{
	if (m_bDelete)
		delete m_pCodec;
}

//-----------------------------------------------------------------------------
CTBEDataCodecMap::CTBEDataCodecMap()
{
}

//-----------------------------------------------------------------------------
CTBEDataCodecMap::~CTBEDataCodecMap()
{
	// Distruggo gli oggetti della la mappa interna all'oggetto
	Reset();
}


//-----------------------------------------------------------------------------
void	CTBEDataCodecMap::Reset()
{
	POSITION	pos = m_Map.GetStartPosition();
	CLIPFORMAT	key;
	_Item*		pObj;

	while(pos != NULL)
	{
		m_Map.GetNextAssoc(pos, key, pObj);
		if (pObj)
			delete pObj;
	}

	m_Map.RemoveAll();

}

//-----------------------------------------------------------------------------
CTBEDataCoDec* CTBEDataCodecMap::AddCodec(CLIPFORMAT	cf, CTBEDataCoDec*	pCodec, BOOL bOwn)
{
	_Item*	pDummy;

	if (m_Map.Lookup(cf, pDummy))
	{
		TRACE0("CTBEDataCodecMap::AddCodec: clipformat already exists\n");
		ASSERT(FALSE);

		if (bOwn)
			delete pCodec;

		return NULL;
	}

	pDummy = new _Item(pCodec, bOwn);
	m_Map.SetAt(cf, pDummy);

	return pCodec;
}

//-----------------------------------------------------------------------------
CTBEDataCoDec*	CTBEDataCodecMap::GetCodec	(CLIPFORMAT	cf)
{
	_Item*	pDummy;

	if (m_Map.Lookup(cf, pDummy))
	{
		return pDummy->GetCoDec();
	}

	return NULL;
}

///////////////////////////////////////////////////////////////////////////////
// class CTBEDataCoDecPastedRecord
//-----------------------------------------------------------------------------
CTBEDataCoDecPastedRecord::CTBEDataCoDecPastedRecord (DBTSlaveBuffered* pDBT, CBodyEdit* pBody, CTBEDataCoDecDBT* pDataCoDecDBT, int nr, SqlRecord* pTargetRecord) 
	: 
	CTBEDataCoDecRecordToValidate (pDBT, pBody, pDataCoDecDBT), 
	m_nr(nr),
	m_pTargetRecord (pTargetRecord)
{
}

//-----------------------------------------------------------------------------
MapNameFields* CTBEDataCoDecPastedRecord::GetFieldsMap() 
{ return  m_pDataCoDecDBT->GetRecord(m_nr)->GetFieldsMap(); }


///////////////////////////////////////////////////////////////////////////////
// class CTBEDataCoDecRecordToValidate
//-----------------------------------------------------------------------------
CString CTBEDataCoDecRecordToValidate::GetSourceDBTClassName() 
{ return  m_pDataCoDecDBT->GetDBTName(); }

CString CTBEDataCoDecRecordToValidate::GetSourceDBTNamespace() 
{ return  m_pDataCoDecDBT->GetDBTNamespace(); }

CString CTBEDataCoDecRecordToValidate::GetSourceSqlRecordClassName() 
{ return  m_pDataCoDecDBT->GetSqlRecCName(); }

CString CTBEDataCoDecRecordToValidate::GetSourceTableName() 
{ return  m_pDataCoDecDBT->GetTableName(); }

///////////////////////////////////////////////////////////////////////////////
// class CTBEDataCoDec
//-----------------------------------------------------------------------------
IMPLEMENT_DYNAMIC(CTBEDataCoDec, CObject)
//-----------------------------------------------------------------------------
CTBEDataCoDec::CTBEDataCoDec()
{
	m_pClassesInfo	= new CTBEDataCoDecClassInfo();
}

//-----------------------------------------------------------------------------
CTBEDataCoDec::~CTBEDataCoDec()
{
	if (m_pClassesInfo)
		delete m_pClassesInfo;
}

//-----------------------------------------------------------------------------
void	CTBEDataCoDec::AddAllDBTSlaves		(DBTMaster*			pDbt)
{
	for (int c = 0; c <= pDbt->GetDBTSlaves()->GetUpperBound(); c++)
	{
		DBTSlave*			pDBTSlave		= DYNAMIC_DOWNCAST(DBTSlave, pDbt->GetDBTSlaves()->GetAt(c));
		DBTSlaveBuffered*	pDBTSlaveBuff	= DYNAMIC_DOWNCAST(DBTSlaveBuffered, pDbt->GetDBTSlaves()->GetAt(c));
		if (pDBTSlave && !pDBTSlaveBuff)
		{
			AddDBTSlave(pDBTSlave);
		}
	}
}

//-----------------------------------------------------------------------------
void	CTBEDataCoDec::AddAllDBTSlavesBuff	(DBTMaster*			pDbt)
{
	for (int c = 0; c <= pDbt->GetDBTSlaves()->GetUpperBound(); c++)
	{
		DBTSlaveBuffered*	pDBTSlaveBuff = DYNAMIC_DOWNCAST(DBTSlaveBuffered, pDbt->GetDBTSlaves()->GetAt(c));
		if (pDBTSlaveBuff)
		{
			AddDBTSlaveBuff(pDBTSlaveBuff);
		}
	}
}
//-----------------------------------------------------------------------------
void	CTBEDataCoDec::AddDropFormat		(CLIPFORMAT cf)
{
	CLIPFORMAT	aCodecKey;

	// Verifico che ci sia un codec che gestisca il formato cf
	ASSERT(m_AcceptFormatsAssociationMap.Lookup(cf,aCodecKey));
	if (m_AcceptFormatsAssociationMap.Lookup(cf,aCodecKey))
	{
		// Aggiungo il formato alla lista dei formati con cui si può fare il drop
		for (int c = 0; c <= m_AcceptDropFormats.GetUpperBound(); c++)
		{
			if (m_AcceptDropFormats.GetAt(c) == cf)
				return;
		}
		m_AcceptDropFormats.Add(cf);
	}
}



//-----------------------------------------------------------------------------
BOOL	CTBEDataCoDec::AcceptDropFormat		(CLIPFORMAT cf)
{
	CLIPFORMAT	aCodecKey;

	// Verifico che ci sia un codec che gestisca il formato cf
	if (m_AcceptFormatsAssociationMap.Lookup(cf,aCodecKey))
	{
		for (int c = 0; c <= m_AcceptDropFormats.GetUpperBound(); c++)
		{
			if (m_AcceptDropFormats.GetAt(c) == cf)
				return TRUE;
		}
	}

	return FALSE;
}
//-----------------------------------------------------------------------------
BOOL	CTBEDataCoDec::AcceptDropFormat	(COleDataObject*	pDataObject)
{
	CLIPFORMAT aRetVal;
	CTBEDataCoDec* codecFound = NULL;
	return AcceptDropFormat(pDataObject,&aRetVal,codecFound);
}

//-----------------------------------------------------------------------------
BOOL	CTBEDataCoDec::AcceptDropFormat		(COleDataObject*	pDataObject, CLIPFORMAT* pRetVal, CTBEDataCoDec*& codecFound)
{
	// Parametri paer la ricerca nella mappa
	CLIPFORMAT	cf;
	CLIPFORMAT	aCodecKey;

	// Controlla se nella clipboard c'è un tipo di dato conosciuto
	for	(POSITION	pos = m_AcceptFormatsAssociationMap.GetStartPosition(); pos; )
	{
		m_AcceptFormatsAssociationMap.GetNextAssoc(pos, cf, aCodecKey);
		if (pDataObject->IsDataAvailable(cf))   // Nella clpbrd ho trovato i dati in formato cf a cui ho associato un codec
		{
			BOOL	bfoundCfForDrop = FALSE;

			// Controllo che si possa fare il drop con il formato trovato
			for (int c = 0; !bfoundCfForDrop && c <= m_AcceptDropFormats.GetUpperBound(); c++)
				bfoundCfForDrop = m_AcceptDropFormats.GetAt(c) == cf;

			// Se posso fare il drop valorizzo i risultati
			if (bfoundCfForDrop)
			{
				codecFound = m_AcceptFormatsMap.GetCodec(aCodecKey);
				*pRetVal = cf;
				return TRUE;
			}
		}
	}

	//Nessun formato per il DROP presente nella clipboard
	return FALSE;
}

//-----------------------------------------------------------------------------
void	CTBEDataCoDec::AddPasteCodecFormat(CLIPFORMAT cf, CTBEDataCoDec* pCodec, BOOL bOwn)
{
	m_AcceptFormatsMap.AddCodec(cf,pCodec,bOwn);
}

//-----------------------------------------------------------------------------
void CTBEDataCoDec::SetCodecAssociation(CLIPFORMAT cfSource, CLIPFORMAT cfCodecTarget)
{
	CLIPFORMAT	aCodecKey;

	if (m_AcceptFormatsAssociationMap.Lookup(cfSource,aCodecKey))
	{
		TRACE2("CTBEDataCoDec::SetCodecAssociation: clipformat is already linked to the codec identified by %d!\n",cfSource,aCodecKey);
		ASSERT(FALSE);
		return;
	}

	if (m_AcceptFormatsMap.GetCodec(cfCodecTarget) == NULL)
	{
		TRACE2("CTBEDataCoDec::SetCodecAssociation: it's impossible to link the clipformat %d to the codec %d!\n ",cfSource,cfCodecTarget);
		ASSERT(FALSE);
		return;
	}

	m_AcceptFormatsAssociationMap.SetAt(cfSource,cfCodecTarget);
}

//-----------------------------------------------------------------------------
BOOL CTBEDataCoDec::FindClipBoardFormat(CTBEDataCoDec*& codecFound, CLIPFORMAT& cfFound)
{
	// leggo le info dalla clipboard
	COleDataObject	obj;
	CLIPFORMAT	cf;
	CLIPFORMAT	aCodecKey;

	codecFound = NULL;
	cfFound = -1;

	if (obj.AttachClipboard())
	{
		// Controlla se nella clipboard c'è un tipo di dato conosciuto
		for	(POSITION	pos = m_AcceptFormatsAssociationMap.GetStartPosition(); pos; )
		{
			m_AcceptFormatsAssociationMap.GetNextAssoc(pos, cf, aCodecKey);
			if (obj.IsDataAvailable(cf))
			{
				codecFound = m_AcceptFormatsMap.GetCodec(aCodecKey);
				cfFound = cf;
				return codecFound != NULL;
			}
		}
	}
	//TRACE0("CTBEDataCoDec::FindClipBoardFormat: no well-known format in the clipboard!\n");
	//ASSERT(FALSE);
	return NULL;
}
//-----------------------------------------------------------------------------
BOOL CTBEDataCoDec::FindClipBoardFormat()
{
	CTBEDataCoDec* codecFound = NULL;
	CLIPFORMAT cfFound = 0;
	return FindClipBoardFormat(codecFound,cfFound);
}

///////////////////////////////////////////////////////////////////////////////
//	CTBEDataCoDecRecord
//-----------------------------------------------------------------------------
IMPLEMENT_DYNAMIC(CTBEDataCoDecRecord, CObject)
//-----------------------------------------------------------------------------
CTBEDataCoDecRecord::CTBEDataCoDecRecord(CTBEDataCoDecDBT*	pDBT)
{
	m_pData		= NULL;
	m_bOwnData	= FALSE;
	m_pDBT		= pDBT;
}

//-----------------------------------------------------------------------------
CTBEDataCoDecRecord::~CTBEDataCoDecRecord()
{
	Reset();
}

//-----------------------------------------------------------------------------
void	CTBEDataCoDecRecord::Reset()
{
	for	(POSITION	pos = m_Fields.GetStartPosition(); pos; )
	{
		DataObj*	pData;
		CString		field;
		m_Fields.GetNextAssoc(pos, field, pData);
		delete pData;
	}
	m_Fields.RemoveAll();

	SetData(NULL, FALSE);
}

//-----------------------------------------------------------------------------
void		CTBEDataCoDecRecord::SetData(CObject*	pObject, BOOL bOwn)
{
	if (m_pData && m_bOwnData)
	{
		delete m_pData;
	}

	m_pData		= pObject;
	m_bOwnData	= bOwn;
}

//-----------------------------------------------------------------------------
void	CTBEDataCoDecRecord::Init()
{
	for	(POSITION	pos = m_Fields.GetStartPosition(); pos; )
	{
		DataObj*	pData;
		CString		field;
		m_Fields.GetNextAssoc(pos, field, pData);
		pData->Clear();
	}

	SetData(NULL, FALSE);
}

//-----------------------------------------------------------------------------
void	CTBEDataCoDecRecord::SetFieldValue	(LPCTSTR	szFieldName, DataObj*	pDataObj)
{
	DataObj*	pData = NULL;
	CString		field = szFieldName; field.MakeUpper();

	if (m_Fields.Lookup(field, pData))
	{
		pData->Assign(*pDataObj);
	}
	else
	{
		m_Fields.SetAt(field, pDataObj->DataObjClone());
	}
}

//-----------------------------------------------------------------------------
BOOL	CTBEDataCoDecRecord::GetDataType		(LPCTSTR szFieldName, DataType&	dt)
{
	DataObj*	pData = NULL;
	CString		field = szFieldName; field.MakeUpper();

	if (m_Fields.Lookup(field, pData))
	{
		dt = pData->GetDataType();
		return TRUE;
	}

	return FALSE;
}

//-----------------------------------------------------------------------------
DataObj*	CTBEDataCoDecRecord::GetDataObj		(LPCTSTR szFieldName) const
{
	DataObj*	pData = NULL;
	CString		field = szFieldName; field.MakeUpper();

	if (m_Fields.Lookup(field, pData))
	{
		return pData;
	}

	return NULL;
}

//-----------------------------------------------------------------------------
CTBEDataCoDecRecord& CTBEDataCoDecRecord::operator= (const CTBEDataCoDecRecord& source)
{
	Reset();

	for	(POSITION	pos = source.m_Fields.GetStartPosition(); pos; )
	{
		DataObj*	pData;
		CString		field;
		source.m_Fields.GetNextAssoc(pos, field, pData);

		m_Fields.SetAt(field, pData->DataObjClone());
	}

	if (source.m_pData && source.m_bOwnData)
	{
		SetData(NULL, FALSE);
		// non può essere fatto
		// (ci sarebbero due record che cercano di cancellare lo stesso oggetto)
		ASSERT(FALSE);
	}
	else
	{
		SetData(source.m_pData, FALSE);
	}
	return *this;
}

//-----------------------------------------------------------------------------
void CTBEDataCoDecRecord::GetFieldValue(LPCTSTR	szFieldName, DataObj* pDataObj)
{
	DataObj*	pData = NULL;
	CString		field = szFieldName;  field.MakeUpper();

	if (m_Fields.Lookup(field, pData))
	{
		pDataObj->Assign(*pData);
	}
	else
	{
		TRACE1("CTBEDataCoDecRecord::GetFieldValue: not found field: %s\n", szFieldName);
		pDataObj->Clear();
	}
}

///////////////////////////////////////////////////////////////////////////////
//	CTBEDataCoDecDBT
//-----------------------------------------------------------------------------
IMPLEMENT_DYNAMIC(CTBEDataCoDecDBT, CObject)

//-----------------------------------------------------------------------------
CTBEDataCoDecDBT::CTBEDataCoDecDBT(DBTType	type, const CString& table, const CString& dbt_name, const CString& recname, const CString& dbt_ns)
	:
	m_TableName		(table),
	m_DBTName		(dbt_name),
	m_SqlRecordName	(recname),
	m_DBTType		(type),
	m_DBTNamespace	(dbt_ns)
{
	m_DBTID = 0;
}

//-----------------------------------------------------------------------------
CTBEDataCoDecDBT::~CTBEDataCoDecDBT()
{
}

//-----------------------------------------------------------------------------
CTBEDataCoDecRecord*	CTBEDataCoDecDBT::AddRecord		()
{
	CTBEDataCoDecRecord*	pNewRec = new CTBEDataCoDecRecord(this);
	m_Records.Add(pNewRec);
	return pNewRec;
}

///////////////////////////////////////////////////////////////////////////////
// class CTBEDataCoDecDBTArray
// ----------------------------------------------------------------------------
IMPLEMENT_DYNAMIC(CTBEDataCoDecDBTArray, CObject);	// salto la gerachia dei template

///////////////////////////////////////////////////////////////////////////////
//	CTBEDataCoDecDocument
//-----------------------------------------------------------------------------
IMPLEMENT_DYNAMIC(CTBEDataCoDecDocument, CObject)
//-----------------------------------------------------------------------------
CTBEDataCoDecDocument::CTBEDataCoDecDocument()
{
	m_pDBTMaster	= NULL;
}

//-----------------------------------------------------------------------------
CTBEDataCoDecDocument::~CTBEDataCoDecDocument()
{
	if (m_pDBTMaster)
		delete m_pDBTMaster;
}

//-----------------------------------------------------------------------------
CTBEDataCoDecDBT*	CTBEDataCoDecDocument::GetDBTSlave	(CRuntimeClass*	pDBTClass)
{
	return GetDBTSlave(pDBTClass->m_lpszClassName);
}

//-----------------------------------------------------------------------------
CTBEDataCoDecDBT*	CTBEDataCoDecDocument::GetDBTSlave	(LPCSTR szClassName)
{
	for (int  c = 0; c <= m_DBTSlaves.GetUpperBound(); c++)
	{
		CTBEDataCoDecDBT*	pDBT = m_DBTSlaves.GetAt(c);
		if (pDBT->GetDBTName() == szClassName)
		{
			return pDBT;
		}
	}

	return NULL;
}


//-----------------------------------------------------------------------------
// Ritorna il DBT con un certo SqlRecord
CTBEDataCoDecDBT*	CTBEDataCoDecDocument::GetDBTSlaveSqlRec	(CRuntimeClass*	pSqlRecClass)
{
	return GetDBTSlaveSqlRec(pSqlRecClass->m_lpszClassName);
}

//-----------------------------------------------------------------------------
// Ritorna il DBT con un certo SqlRecord
CTBEDataCoDecDBT*	CTBEDataCoDecDocument::GetDBTSlaveSqlRec	(LPCSTR			szSqlRecClassName)
{
	for (int  c = 0; c <= m_DBTSlaves.GetUpperBound(); c++)
	{
		CTBEDataCoDecDBT*	pDBT = m_DBTSlaves.GetAt(c);
		CString				pDBTSqlRecName = pDBT->GetSqlRecCName();
		if (pDBTSqlRecName == szSqlRecClassName)
		{
			return pDBT;
		}
	}
	return NULL;
}

//-----------------------------------------------------------------------------
CTBEDataCoDecDBT* CTBEDataCoDecDocument::GetDBTSlaveBuffered	()
{
	for (int c = 0; c <= m_DBTSlaves.GetUpperBound(); c++)
	{
		CTBEDataCoDecDBT* pDBT = m_DBTSlaves.GetAt(c);
		if (pDBT->GetDBTType() == CTBEDataCoDecDBT::SLAVEBUFFERED)
			return m_DBTSlaves.GetAt(c);
	}
	return NULL;
}

//-----------------------------------------------------------------------------
CTBEDataCoDecDBT*	CTBEDataCoDecDocument::AddDBTMaster(const CString& dbt_name, const CString& dbt_ns, const CString& rec_name, const CString& tbl_name)
{
	ASSERT(m_pDBTMaster == NULL);

	m_pDBTMaster	= new CTBEDataCoDecDBT
						   (
								CTBEDataCoDecDBT::MASTER,
								tbl_name,
								dbt_name,
								rec_name,
								dbt_ns
						   );

	return m_pDBTMaster;
}


//-----------------------------------------------------------------------------
CTBEDataCoDecDBT*	CTBEDataCoDecDocument::AddDBTSlave			(const CString& dbt_name, const CString& dbt_ns, const CString& rec_name, const CString& tbl_name)
{
	CTBEDataCoDecDBT*	pDBT = new CTBEDataCoDecDBT
						   (
								CTBEDataCoDecDBT::SLAVE,
								tbl_name,
								dbt_name,
								rec_name,
								dbt_ns
						   );

	m_DBTSlaves.Add(pDBT);
	return pDBT;
}

//-----------------------------------------------------------------------------
CTBEDataCoDecDBT*	CTBEDataCoDecDocument::AddDBTSlaveBuffered(const CString& dbt_name, const CString& dbt_ns, const CString& rec_name, const CString& tbl_name)
{
	CTBEDataCoDecDBT*	pDBT = new CTBEDataCoDecDBT
						   (
								CTBEDataCoDecDBT::SLAVEBUFFERED,
								tbl_name,
								dbt_name,
								rec_name,
								dbt_ns
						   );

	m_DBTSlaves.Add(pDBT);
	return pDBT;
}

//-----------------------------------------------------------------------------
CTBEDataCoDecDBT*	CTBEDataCoDecDocument::GetDBTByID					(int nID)
{
	if (m_pDBTMaster && m_pDBTMaster->GetDBTID() == nID)
		return m_pDBTMaster;

	for (int c = 0; c <= m_DBTSlaves.GetUpperBound(); c++)
	{
		CTBEDataCoDecDBT*	pDBT = m_DBTSlaves.GetAt(c);
		if (pDBT->GetDBTID() == nID)
			return pDBT;
	}

	return NULL;
}

///////////////////////////////////////////////////////////////////////////////
//	CTBEDataCoDecClassInfo
//-----------------------------------------------------------------------------
IMPLEMENT_DYNAMIC(CTBEDataCoDecClassInfo, CObject)
//-----------------------------------------------------------------------------
CTBEDataCoDecClassInfo::CTBEDataCoDecClassInfo()
{
}

//-----------------------------------------------------------------------------
CTBEDataCoDecClassInfo::~CTBEDataCoDecClassInfo()
{
	Reset();
}

//-----------------------------------------------------------------------------
void	CTBEDataCoDecClassInfo::Reset()
{
	CString			key;
	CStringList*	pList;
	for(POSITION pos = m_Hierarchy.GetStartPosition(); pos; )
	{
		m_Hierarchy.GetNextAssoc(pos, key, pList);
		if (pList)	delete pList;
	}

	m_Hierarchy.RemoveAll();
}

//-----------------------------------------------------------------------------
void	CTBEDataCoDecClassInfo::AddClass(CRuntimeClass*	pRTClass)
{
	CRuntimeClass*	pRTCursor = pRTClass;
	CStringList*	pList = NULL;

	while(pRTCursor)
	{
		// verifico se la classe e' stata già processata...
		CString			class_name(pRTCursor->m_lpszClassName);
		CStringList*	pDummy;
		if (m_Hierarchy.Lookup(class_name, pDummy))
		{
			if (pRTCursor == pRTClass)
			{
				break;	// fine elaborazione
			}
		}

		if (!pList)
		{
			pList = new CStringList;
			m_Hierarchy.SetAt(class_name, pList);
		}

		// aggiunge in coda...
		pList->AddTail(class_name);

		// avanza alla classe padre...
		CRuntimeClass*	pRTParent = NULL;
		if(pRTCursor->m_pfnGetBaseClass)
			pRTParent = pRTCursor->m_pfnGetBaseClass();

		pRTCursor = pRTParent;
	}
}

//-----------------------------------------------------------------------------
int		CTBEDataCoDecClassInfo::GetClassList(CStringList&	sl)
{
	int	count = -1;

	for(POSITION pos = m_Hierarchy.GetStartPosition(); pos; )
	{
		count++;
		CString			key;
		CStringList*	pData;
		m_Hierarchy.GetNextAssoc(pos, key, pData);
		sl.AddTail(key);
	}

	return count;
}

//-----------------------------------------------------------------------------
CStringList*	CTBEDataCoDecClassInfo::GetClassHierarchy(LPCSTR szClassName)
{
	CStringList*	pList = NULL;
	CString			key(szClassName);
	if (m_Hierarchy.Lookup(key, pList))
		return pList;

	return NULL;
}

//-----------------------------------------------------------------------------
void CTBEDataCoDecClassInfo::SetClassHierarchy(LPCSTR szClassName, CStringList&	hierarchy)
{
	CString			class_name(szClassName);
	CStringList*	pList;
	if (m_Hierarchy.Lookup(class_name, pList))
		return;

	TRACE1("CTBEDataCoDecClassInfo::SetClassHierarchy %s\n", class_name);

	pList = new CStringList();
	m_Hierarchy.SetAt(class_name, pList);

	for (POSITION pos = hierarchy.GetHeadPosition(); pos; )
	{
		pList->AddTail(hierarchy.GetNext(pos));
	}
}

//-----------------------------------------------------------------------------
BOOL			CTBEDataCoDecClassInfo::IsClassKindOf(LPCSTR cname, LPCSTR cparent)
{
	CString			class_name(cname);

	CStringList*	pList;
	if (!m_Hierarchy.Lookup(class_name, pList))
		return FALSE;

	for(POSITION pos = pList->GetHeadPosition(); pos; )
	{
		CString	parent = pList->GetNext(pos);
		if (parent == cparent)
			return TRUE;
	}

	return FALSE;
}

//-----------------------------------------------------------------------------
BOOL			CTBEDataCoDecClassInfo::IsClassKindOf(LPCSTR cname, CRuntimeClass*	pRTClass)
{
	return IsClassKindOf(cname, pRTClass->m_lpszClassName);
}

