
#include "stdafx.h" 

#include <TBGES\DBT.H>
#include <TBGENERIC\GLOBALS.H>


#include <XENGINE\TBXMLEnvelope\XMLEnvelopeTags.h>

#include "XMLTransferTags.h"
#include "XMLEvents.h"
#include "XMLCodingRules.h"


#ifdef _DEBUG
#define new DEBUG_NEW
#undef THIS_FILE
static const char THIS_FILE[] = __FILE__;
#endif

#define LEN_KEY				256

static const TCHAR szFieldLeftSeparator	[] = _T("<!");
static const TCHAR szFieldRightSeparator	[] = _T("!>");
static const TCHAR szValueSeparator		[] = _T("@");
static const TCHAR szParam1				[] = _T("P1");
static const TCHAR szParam2				[] = _T("P2");
static const TCHAR szParam3				[] = _T("P3");

/////////////////////////////////////////////////////////////////////////////
//	SqlRecord					### LOSTANDFOUND ###					
/////////////////////////////////////////////////////////////////////////////
//
/////////////////////////////////////////////////////////////////////////////


//-----------------------------------------------------------------------------
IMPLEMENT_DYNCREATE(TLostAndFound, SqlRecord) 

//-----------------------------------------------------------------------------
TLostAndFound::TLostAndFound(BOOL bCallInit)
	:
	SqlRecord 				(GetStaticName())
{
    
	f_TableName.SetUpperCase();

	BindRecord();
	if (bCallInit) Init();
}

//-----------------------------------------------------------------------------
void TLostAndFound::BindRecord()
{
	BEGIN_BIND_DATA	();
		BIND_DATA	(_T("OriginalKey"),			f_Key				);
		BIND_DATA	(_T("KeyTableName"),		f_TableName			);
		BIND_DATA	(_T("DocumentNamespace"),	f_DocumentNamespace	);
		BIND_DATA	(_T("UniversalKey"),		f_UniversalKey		);
		BIND_DATA	(_T("CreationDate"),		f_Date				);
	END_BIND_DATA();    
}

//-----------------------------------------------------------------------------
LPCTSTR TLostAndFound::GetStaticName() { return _T("XE_LostAndFound"); }


//////////////////////////////////////////////////////////////////////////////
//             class CDataManagerEvents implementation
//////////////////////////////////////////////////////////////////////////////
//=============================================================================
//FUNZIONI COMUNI AGLI EVENT MANAGER DI IMPORT E EXPORT
IMPLEMENT_DYNCREATE(CDataManagerEvents, CEventManager);

//----------------------------------------------------------------------------
///poiche' ho due clientdoc per documento (uno di import e uno di export)
BOOL CDataManagerEvents::CanGoOn()
{
	CClientDoc* pDoc = GetClientDoc();
	
	if (!pDoc) return FALSE;

	CBaseDocument* pMaster = pDoc->GetMasterDocument();
	
	if (!pMaster)
	{
		ASSERT(FALSE);
		return FALSE;
	}

	CXMLDataManager* pDataMng = (CXMLDataManager*)pMaster->GetXMLDataManager();
	ASSERT(pDataMng);
	return 
		(pDataMng->GetStatus() == CXMLDataManagerObj::XML_MNG_IMPORTING_DATA && pDoc->IsKindOf(RUNTIME_CLASS(CXMLDataImportDoc))) ||
		(pDataMng->GetStatus() == CXMLDataManagerObj::XML_MNG_EXPORTING_DATA && pDoc->IsKindOf(RUNTIME_CLASS(CXMLDataExportDoc)));
}


//----------------------------------------------------------------------------
BOOL CDataManagerEvents::CreateWhereClause (CXMLNode *pNode, SqlTable* pTable)
{
	if (!pNode || !pTable )
	{
		ASSERT(FALSE);
		return FALSE;
	}

	CString strField, strValue;
	CXMLNode *pAttr = pNode->GetFirstAttribute ();
	BOOL bCreated = FALSE;
	while(pAttr)
	{
		if (!pAttr->GetName (strField) || !pAttr->GetText (strValue))
		{
			ASSERT(FALSE);
			return FALSE;
		}

		// salto l'attributo che individua il nome della UniversalKey
		if (strField == XML_UNIVERSAL_KEY_NAME_ATTRIBUTE)
		{
			pAttr = pNode->GetNextAttribute ();
			continue;
		}

		if (!AddWhereClause(strField, strValue, pTable))
			return FALSE;
		
		bCreated = TRUE;
		
		pAttr = pNode->GetNextAttribute ();
	}

	return bCreated;
}

//----------------------------------------------------------------------------
BOOL CDataManagerEvents::CreateWhereClause (CXMLNodeChildsList *pNodeList, SqlTable* pTable)
{
	if (!pNodeList || !pTable )
	{
		ASSERT(FALSE);
		return FALSE;
	}

	CString strField, strValue;
	CXMLNode *pNode = NULL;
	for (int i=0; i<pNodeList->GetSize (); i++)
	{
		pNode = pNodeList->GetAt (i);
		if (!pNode ||
			!pNode->GetAttribute (DOC_XML_PK_ATTRIBUTE, strField) || 
			!pNode->GetAttribute (DOC_XML_VALUE_ATTRIBUTE, strValue))
			{
				ASSERT(FALSE);
				return FALSE;
			}
			
			if (!AddWhereClause(strField, strValue, pTable))
				return FALSE;
	}
	return TRUE;
}

//----------------------------------------------------------------------------
BOOL CDataManagerEvents::AddWhereClause (const CString &strField, const CString &strValue, SqlTable* pTable)
{
	if (!pTable)
	{
		ASSERT(FALSE);
		return FALSE;
	}
	
	if (strField.IsEmpty () || strValue.IsEmpty ())
		return FALSE;

	SqlRecord *pRec = pTable->GetRecord ();
	if (!pRec)
	{
		ASSERT(FALSE);
		return FALSE;
	}
	
	DataObj *pObj = pRec->GetDataObjFromColumnName(strField);
	if (!pObj)
	{
		ASSERT(FALSE);
		return FALSE;
	}

	pTable->AddParam (strField, *pObj);
	pTable->AddFilterColumn (strField);
	pObj->AssignFromXMLString ((LPCTSTR)strValue);
	pTable->SetParamValue (strField, *pObj);

	return TRUE;
}

//----------------------------------------------------------------------------
BOOL CDataManagerEvents::AssignNewFieldsToKey (CXMLNodeChildsList *pKeyList, SqlRecord *pRec)
{
	if (!pKeyList || !pRec)
	{
		ASSERT(FALSE);
		return FALSE;
	}

	CString strField, strValue;
	CXMLNode *pKey = NULL;
	DataObj *pObj = NULL;
	for(int i=0; i<pKeyList->GetSize (); i++)
	{
		pKey = pKeyList->GetAt (i);
		if (!pKey || !pKey->GetAttribute(DOC_XML_PK_ATTRIBUTE, strField))
		{
			ASSERT(FALSE);
			return FALSE;
		}

		pObj = pRec->GetDataObjFromColumnName(strField);
		if (!pObj)
		{
			ASSERT(FALSE);
			return FALSE;
		}
		
		if (CXMLDataManager::IsNotEmptyDataObj(pObj))
		{
			strValue = pObj->FormatDataForXML ();
		}
		else
		{
			ASSERT(FALSE);
			return FALSE;
		}

		pKey->SetAttribute (DOC_XML_VALUE_ATTRIBUTE, strValue);
	}

	return TRUE;
}

//----------------------------------------------------------------------------
BOOL CDataManagerEvents::AssignKeyToRecord (CXMLNodeChildsList *pKeyList, SqlRecord *pRec)
{
	if (!pKeyList || !pRec)
	{
		ASSERT(FALSE);
		return FALSE;
	}

	CString strField, strValue;
	CXMLNode *pKey = NULL;
	DataObj *pObj = NULL;
	for(int i=0; i<pKeyList->GetSize (); i++)
	{
		pKey = pKeyList->GetAt (i);
		if (!pKey ||
			!pKey->GetAttribute (DOC_XML_PK_ATTRIBUTE, strField) || 
			!pKey->GetAttribute (DOC_XML_VALUE_ATTRIBUTE, strValue))
		{
			ASSERT(FALSE);
			return FALSE;
		}

		pObj = pRec->GetDataObjFromColumnName(strField);
		if (!pObj)
		{
			ASSERT(FALSE);
			return FALSE;
		}
		
		pObj->AssignFromXMLString (strValue);
	}

	return TRUE;
}

//----------------------------------------------------------------------------
BOOL CDataManagerEvents::AssignUnKeyToRecord (CXMLNodeChildsList *pUnKeyList, SqlRecord *pRec)
{
	if (!pUnKeyList || !pRec)
	{
		ASSERT(FALSE);
		return FALSE;
	}

	CXMLNode *pNode = NULL;
	CString strField, strValue;
	BOOL bAssigned = FALSE;
	for (int i=0; i<pUnKeyList->GetSize (); i++)
	{
		pNode = pUnKeyList->GetAt (i);
		if (!pNode)
		{
			ASSERT(FALSE);
			return FALSE;
		}
		
		CXMLNode *pAttr = pNode->GetFirstAttribute ();
		while(pAttr)
		{
			if (!pAttr->GetName (strField) || !pAttr->GetText (strValue))
			{
				ASSERT(FALSE);
				return FALSE;
			}

			// salto l'attributo che individua il nome della UniversalKey
			if (strField == XML_UNIVERSAL_KEY_NAME_ATTRIBUTE)
			{
				pAttr = pNode->GetNextAttribute ();
				continue;
			}

			DataObj *pObj = pRec->GetDataObjFromColumnName(strField);
			if (!pObj)
			{
				ASSERT(FALSE);
				return FALSE;
			}
			
			pObj->AssignFromXMLString (strValue);
			bAssigned = TRUE;
			pAttr = pNode->GetNextAttribute ();
		}
	}
	return bAssigned;
}

//----------------------------------------------------------------------------
BOOL CDataManagerEvents::AssignNewFieldsToUnKey (CXMLNodeChildsList *pUnKeyList, SqlRecord *pRec)
{
	if (!pUnKeyList || !pRec)
	{
		ASSERT(FALSE);
		return FALSE;
	}

	CXMLNode *pNode = NULL;
	CString strField;
	BOOL bAssigned = FALSE;
	for (int i=0; i<pUnKeyList->GetSize (); i++)
	{
		pNode = pUnKeyList->GetAt (i);
		if (!pNode)
		{
			ASSERT(FALSE);
			return FALSE;
		}
		
		CXMLNode *pAttr = pNode->GetFirstAttribute ();
		while(pAttr)
		{
			if (!pAttr->GetName (strField))
			{
				ASSERT(FALSE);
				return FALSE;
			}

			// salto l'attributo che individua il nome della UniversalKey
			if (strField == XML_UNIVERSAL_KEY_NAME_ATTRIBUTE)
			{
				pAttr = pNode->GetNextAttribute ();
				continue;
			}

			DataObj *pObj = pRec->GetDataObjFromColumnName(strField);
			if (!pObj)
			{
				ASSERT(FALSE);
				return FALSE;
			}
	
			pAttr->SetText (CXMLDataManager::IsNotEmptyDataObj(pObj) ? pObj->FormatDataForXML () : _T(""));	
			bAssigned = TRUE;

			pAttr = pNode->GetNextAttribute ();
		}
	}

	return bAssigned;
}

//----------------------------------------------------------------------------
BOOL CDataManagerEvents::UniversalKeyToString (CXMLNode *pNode, CString& strOut)
{
	if (!pNode)
	{
		ASSERT(FALSE);
		return FALSE;
	}

	CStringArray strArray;

	CString strField, strValue;
	BOOL bOk = FALSE;
	CXMLNode *pAttr = pNode->GetFirstAttribute ();
	while(pAttr)
	{
		if (!pAttr->GetName (strField) || !pAttr->GetText (strValue))
		{
			ASSERT(FALSE);
			return FALSE;
		}
		
		// salto l'attributo che individua il nome della UniversalKey
		if (strField == XML_UNIVERSAL_KEY_NAME_ATTRIBUTE)
		{
			pAttr = pNode->GetNextAttribute ();
			continue;
		}

		if (strField.IsEmpty () || strValue.IsEmpty ())
			return FALSE;

		AppendValue(strArray, strField, strValue);
		
		pAttr = pNode->GetNextAttribute ();
		bOk=TRUE;
	}
	
	strOut.Empty ();
	for (int i=0; i<strArray.GetSize (); i++)
		strOut += strArray.GetAt (i);

	return bOk;
}

//----------------------------------------------------------------------------
BOOL CDataManagerEvents::KeyToString (CXMLNodeChildsList *pNodeList, CString& strOut)
{
	if (!pNodeList)
	{
		ASSERT(FALSE);
		return FALSE;
	}

	CStringArray strArray;

	CString strField, strValue;
	CXMLNode * pNode = NULL;
	BOOL bOk = FALSE;
	int i = 0; 
	for (i = 0  ; i<pNodeList->GetSize (); i++)
	{
		pNode = pNodeList->GetAt (i);

		if (!pNode || 
			!pNode->GetAttribute (DOC_XML_PK_ATTRIBUTE, strField) || 
			!pNode->GetAttribute (DOC_XML_VALUE_ATTRIBUTE, strValue))
		{
			ASSERT(FALSE);
			return FALSE;
		}
		
		if (strField.IsEmpty () || strValue.IsEmpty ())
			return FALSE;

		AppendValue(strArray, strField, strValue);
		bOk=TRUE;
	}

	strOut.Empty ();
	for (i=0; i<strArray.GetSize (); i++)
		strOut += strArray.GetAt (i);

	return TRUE;
}

//----------------------------------------------------------------------------
BOOL CDataManagerEvents::StringToUniversalKey (const CString& strIn, CXMLNode *pNode)
{
	if (!pNode)
	{
		ASSERT(FALSE);
		return FALSE;
	}

	CString strField, strValue;
	BOOL bOk = FALSE;

	CXMLNode *pAttr = pNode->GetFirstAttribute ();
	while(pAttr)
	{
		pAttr->GetName (strField);

		// salto l'attributo che individua il nome della UniversalKey
		if (strField == XML_UNIVERSAL_KEY_NAME_ATTRIBUTE)
		{
			pAttr = pNode->GetNextAttribute ();
			continue;
		}

		if (!ExtractValue(strIn, strField, strValue))
			return FALSE;

		if (!pAttr->SetText (strValue))
			return FALSE;

		bOk = TRUE;
		pAttr = pNode->GetNextAttribute ();
	}
	return bOk;
}

//----------------------------------------------------------------------------
BOOL CDataManagerEvents::StringToKey (const CString& strIn, CXMLNodeChildsList *pNodeList)
{
	if (!pNodeList)
	{
		ASSERT(FALSE);
		return FALSE;
	}

	CXMLNode * pNode = NULL;
	CString strField, strValue;

	for (int i=0; i<pNodeList->GetSize (); i++)
	{
		pNode = pNodeList->GetAt (i);	
		if (!pNode ||
			!pNode->GetAttribute (DOC_XML_PK_ATTRIBUTE, strField))
		{
			ASSERT(FALSE);
			return FALSE;
		}
		
		if (!ExtractValue(strIn, strField, strValue))
			return FALSE;

		if (!pNode->SetAttribute (DOC_XML_VALUE_ATTRIBUTE, strValue))
			return FALSE;
	}
	return TRUE;
}


//----------------------------------------------------------------------------
BOOL CDataManagerEvents::ExtractValue(const CString& strToParse, const CString& strInInputField, CString &strValue)
{
	strValue.Empty ();
		
	CString strInputField = strInInputField;
	strInputField.MakeUpper();

	CString strField = szFieldLeftSeparator + strInputField + szFieldRightSeparator;

	int startPos = 0, endPos, valueLength;
	CString strValueLength, strCheckValue;
	while(TRUE)
	{
		startPos = strToParse.Find (strField, startPos);
		if (startPos == -1) return FALSE;
		startPos += strField.GetLength ();

		endPos = strToParse.Find (szValueSeparator, startPos);
		if (endPos == -1) return FALSE;
		
		strValueLength = strToParse.Mid (startPos, endPos - startPos);
		
		// devo verificare se è effettivamente un numero intero
		valueLength = _ttoi((const TCHAR *)strValueLength);
		strCheckValue.Format (_T("%d"), valueLength);

		if (!strValueLength.Compare (strCheckValue))
			break;
	}

	endPos++ ;
	strValue = strToParse.Mid (endPos, valueLength);
	
	return !strValue.IsEmpty ();
}

//----------------------------------------------------------------------------
BOOL CDataManagerEvents::AppendValue (CStringArray& strArray, const CString& strInField, const CString &strValue)
{
	// <fieldname>nn&fieldvalue
	CString strLength, strToAdd, strField;
	
	strField = strInField;
	strField.MakeUpper();

	strLength.Format (_T("%d"), strValue.GetLength ());
	strToAdd = szFieldLeftSeparator + 
								strField + 
								szFieldRightSeparator + 
								strLength + 
								szValueSeparator +
								strValue;

	int i = 0;
	for (i = 0 ; i<strArray.GetSize (); i++)
	{
		if (strToAdd < strArray.GetAt (i)) break;
	}

	strArray.InsertAt (i, strToAdd);

	return TRUE;
}


//----------------------------------------------------------------------------
CAbstractFormDoc* CDataManagerEvents::GetDocument ()
{
	CClientDoc* pDoc = GetClientDoc();
	if (!pDoc) return FALSE;

	CBaseDocument* pMaster = pDoc->GetMasterDocument();
	if (!pMaster || !pMaster->IsKindOf(RUNTIME_CLASS(CAbstractFormDoc)))
	{
		ASSERT(FALSE);
		return NULL;
	}
	
	return (CAbstractFormDoc*) pMaster;
}

//----------------------------------------------------------------------------
DBTMaster* CDataManagerEvents::GetDBTMaster ()
{
	CAbstractFormDoc* pMaster = GetDocument ();
	if (!pMaster)
	{
		ASSERT(FALSE);
		return NULL;
	}

	DBTMaster* pDBT = pMaster->m_pDBTMaster;	
	if (!pDBT)
	{
		ASSERT(FALSE);
		return NULL;
	}

	return pDBT;
}

//----------------------------------------------------------------------------
SqlRecord* CDataManagerEvents::GetRuntimeRecord ()
{	
	DBTMaster* pDBT = GetDBTMaster ();	
	if (!pDBT)
	{
		ASSERT(FALSE);
		return NULL;
	}

	return pDBT->GetRecord();
}

//----------------------------------------------------------------------------
BOOL CDataManagerEvents::SelectKeyFromDocTable (CXMLNodeChildsList *pKeyList, CXMLNodeChildsList *pUnKeyList)
{
	SqlRecord *pRec = GetRuntimeRecord ();
	if (!pRec || !pKeyList || !pUnKeyList)
	{
		ASSERT(FALSE);
		return NULL;
	}
	
	SqlTable aTable(pRec, GetDocument()->GetReadOnlySqlSession());

	BOOL bResult = FALSE;
	try
	{
		for (int i=0; i<pUnKeyList->GetSize(); i++)
		{
			DBTMaster *pDBT = GetDBTMaster ();
			if (!pDBT) throw -1;
			
			aTable.Open ();
			
			// mi faccio inizializzare la query dal documento
			// per filtrare il giusto tipo di documento
			// (ad es., imposta correttamente il TipoCliFor per i clienti/fornitori)
			pDBT->OnPrepareForXImportExport(&aTable);
			
			CXMLNode *pUnKey = pUnKeyList->GetAt (i);		
			if (CreateWhereClause(pUnKey,  &aTable))
			{
				aTable.Query();
				if (!aTable.IsEmpty())
				{
					bResult = TRUE;
					if (!AssignNewFieldsToKey(pKeyList, aTable.GetRecord())) throw -1;
					aTable.Close ();
					break;
				}
			}
			aTable.Close ();
		}
	}
	catch(...)
	{
 		if (aTable.IsOpen ()) aTable.Close ();
		bResult = FALSE;
	}

	return bResult;
}


//----------------------------------------------------------------------------
BOOL CDataManagerEvents::InsertKeyIntoDocTable (CXMLNodeChildsList *pKeyList, CXMLNodeChildsList *pUnKeyList)
{
	SqlRecord* pRec = SelectRecordFromDocTable (pKeyList);
	if (pRec)
	{
		// esiste già, ma non è quello corrispondente alla mia universal key
		return FALSE;
	}

	SqlRecord *pRecord = GetRuntimeRecord();
	if (!pRecord || !pKeyList || !pUnKeyList)
	{
		ASSERT(FALSE);
		return FALSE;
	}

	SqlTable aTable(pRecord, GetDocument()->GetUpdatableSqlSession());
	try
	{
		aTable.Open(TRUE);
		aTable.SelectAll();
		aTable.Query();
		aTable.AddNew ();
		DBTMaster *pDBT = GetDBTMaster ();
		if (!pDBT) throw -1;
		pDBT->Init();

		BOOL bResult = FALSE;
		
		if (!AssignUnKeyToRecord(pUnKeyList, pRecord)) throw -1;
		
		if (!AssignKeyToRecord(pKeyList, pRecord)) throw -1;
		
		aTable.Update ();
		aTable.Close ();
	}
	catch(...)
	{
		if (aTable.IsOpen ()) aTable.Close ();
		ASSERT(FALSE);
		return FALSE;
	}

	return TRUE;
}

//----------------------------------------------------------------------------
BOOL CDataManagerEvents::SelectKeyFromLostAndFound (CXMLNodeChildsList *pKeyList, CXMLNodeChildsList *pUnKeyList)
{
	SqlRecord* pRec = GetRuntimeRecord();
	CAbstractFormDoc *pDoc = GetDocument ();

	if (!pKeyList || !pUnKeyList || !pRec || !pDoc)
	{
		ASSERT(FALSE);
		return FALSE;
	}

	TLostAndFound aRec;
	SqlTable aTable (&aRec, GetDocument()->GetReadOnlySqlSession());
	BOOL bFound = FALSE;
	try
	{		
		for (int i=0; i<pUnKeyList->GetSize(); i++)
		{
			aTable.Open ();
			aTable.SelectAll ();
			
			CXMLNode *pUnKey = pUnKeyList->GetAt (i);
			CString strFilter;
			if (UniversalKeyToString(pUnKey, strFilter))
			{				
				//filtro sulla chiave
				aTable.AddFilterColumn (aRec.f_UniversalKey);
				aTable.AddParam (szParam1, aRec.f_UniversalKey);
				aTable.SetParamValue (szParam1, DataStr(strFilter));

				//filtro sulla tabella
				aTable.AddFilterColumn (aRec.f_TableName);
				aTable.AddParam (szParam2, aRec.f_TableName);
				DataStr strTable = pRec->GetTableName();
				strTable.SetUpperCase();
				aTable.SetParamValue (szParam2, strTable);
				
				//filtro sul namespace di documento
				aTable.AddFilterColumn (aRec.f_DocumentNamespace);
				aTable.AddParam (szParam3, aRec.f_DocumentNamespace);
				aTable.SetParamValue (szParam3, DataStr(pDoc->GetNamespace().ToString()));

				aTable.Query();
				if (!aTable.IsEmpty())
				{
					if (!StringToKey(aRec.f_Key.GetString (), pKeyList)) throw -1;
					bFound = TRUE;
					aTable.Close();
					break;
				}
			}
			aTable.Close();
		}
	}
	catch(...)
	{
		if (aTable.IsOpen ()) aTable.Close ();
		ASSERT(FALSE);
		return FALSE;
	}
	
	return bFound;
}


//----------------------------------------------------------------------------
BOOL CDataManagerEvents::InsertKeyIntoLostAndFound (CXMLNodeChildsList *pKeyList, CXMLNodeChildsList *pUnKeyList)
{
	SqlRecord* pRec = GetRuntimeRecord();
	CAbstractFormDoc *pDoc = GetDocument ();
	
	if (!pKeyList || !pUnKeyList || !pRec || !pDoc)
	{
		ASSERT(FALSE);
		return FALSE;
	}

	CString strKey, strUnKey;
	BOOL bHaveUnKey = FALSE;

	// inserisco la prima delle UniversalKeys non vuota (di solito ne ho solo una)
	for (int i=0; i<pUnKeyList->GetSize(); i++)
	{
		CXMLNode *pUnKey = pUnKeyList->GetAt (i);
		if (!pUnKey)
		{
			ASSERT(FALSE);
			return FALSE;
		}
		bHaveUnKey = UniversalKeyToString(pUnKey, strUnKey);
		if (bHaveUnKey) break;
	}

	if (!bHaveUnKey || !KeyToString(pKeyList, strKey))
	{
		ASSERT(FALSE);
		return FALSE;
	}

	TLostAndFound aRec;
	//BugFix #20664: prima l'inserimento nella lost&found veniva fatto utilizzando la transazione del documento, questo faceva in modo però che in caso di rollback l'informazione della Lost&Found
	// andasse persa
	SqlSession* pSqlSession = GetDocument()->GetSqlConnection()->GetNewSqlSession(); 
	SqlTable aTable(&aRec, pSqlSession);	
	BOOL bResult = FALSE;
	TRY
	{
		aTable.Open (TRUE);
		aTable.SetAutocommit();
		aTable.SelectAll();
		aTable.Query();
		aTable.AddNew();
		aRec.f_Key = strKey;
		aRec.f_UniversalKey = strUnKey;
		aRec.f_TableName =  pRec->GetTableName();
		aRec.f_DocumentNamespace = pDoc->GetNamespace().ToString();
		
		SYSTEMTIME aTime;
		GetSystemTime(&aTime);
		aRec.f_Date = DataDate(aTime);

		bResult = aTable.Update();
		aTable.Close();
		pSqlSession->Close();
		delete pSqlSession;
	}
	CATCH(SqlException, e)
	{
		//GetDocument->Message(e->m_strError);		
		if (aTable.IsOpen ()) 
			aTable.Close ();
		if (pSqlSession)
			pSqlSession->Close();
		pSqlSession->Close();
		delete pSqlSession;

		ASSERT(FALSE);
		return FALSE;
	}
	END_CATCH

	return bResult;
}

//----------------------------------------------------------------------------
BOOL CDataManagerEvents::DeleteKeyFromLostAndFound (CXMLNodeChildsList *pKeyList)
{
	SqlRecord* pRec = GetRuntimeRecord();
	CAbstractFormDoc *pDoc = GetDocument ();
	
	if (!pKeyList || !pRec || !pDoc)
	{
		ASSERT(FALSE);
		return FALSE;
	}

	TLostAndFound aRec;
	SqlTable aTable (&aRec, GetDocument()->GetUpdatableSqlSession());
	// quando viene chiamatail documento non sia sotto transazione. Visto che è una singola
	// riga di update pongo la tabella in autocommit
	aTable.SetAutocommit();
	try
	{
		aTable.Open (TRUE);
		aTable.SelectAll ();
		
		CString strFilter;
		if (!KeyToString(pKeyList, strFilter)) throw -1;

		//filtro sulla chiave
		aTable.AddParam (szParam1, aRec.f_Key);
		aTable.AddFilterColumn (aRec.f_Key);
		aTable.SetParamValue (szParam1, DataStr(strFilter));

		//filtro sulla tabella
		aTable.AddParam (szParam2, aRec.f_TableName);
		aTable.AddFilterColumn (aRec.f_TableName);
		DataStr strTable = pRec->GetTableName();
		strTable.SetUpperCase();
		aTable.SetParamValue (szParam2, strTable);
	
		//filtro sul namespace di documento
		aTable.AddFilterColumn (aRec.f_DocumentNamespace);
		aTable.AddParam (szParam3, aRec.f_DocumentNamespace);
		aTable.SetParamValue (szParam3, DataStr(pDoc->GetNamespace().ToString()));

		aTable.Query();
		if (!aTable.IsEmpty())
			aTable.Delete();
		aTable.Close();
	}
	catch(...)
	{
		if (aTable.IsOpen ()) aTable.Close ();
		ASSERT(FALSE);
		return FALSE;
	}

	return TRUE;
}


//----------------------------------------------------------------------------
BOOL CDataManagerEvents::GetNewKey (CXMLNodeChildsList *pKeyList)
{
	CClientDoc* pClientDoc = GetClientDoc();
	if (!pClientDoc)
	{
		ASSERT(FALSE);
		return FALSE;
	}

	CAbstractFormDoc* pDoc = (CAbstractFormDoc*)pClientDoc->GetMasterDocument();
	if (!pDoc || !pDoc->IsKindOf (RUNTIME_CLASS(CAbstractFormDoc)))
	{
		ASSERT(FALSE);
		return FALSE;
	}

	if (!pDoc->m_pDBTMaster || !pDoc->m_pDBTMaster->GetRecord ())
	{
		ASSERT(FALSE);
		return FALSE;
	}

	SqlRecord * pRec = pDoc->m_pDBTMaster->GetRecord ();

	pDoc->SetAssignedCounter (NULL);
	// la chiedo al documento
	if (pDoc->OnPreparePKForXMLImport())
	{	
		// se il documento ha assegnato un counter, lo blocco per tenermelo per la procedura di import
		DataObj* pDataObj = pDoc->GetAssignedCounter();
		if (pDataObj)
			pDataObj->SetValueLocked ();

		if (!AssignNewFieldsToKey(pKeyList, pRec))
			return FALSE;
	}
	else
	{
		AssignKeyToRecord (pKeyList, pRec);
		
		CString strSiteCode;
		CXMLDataManager* pXMLDataMng = (CXMLDataManager*) pDoc->GetXMLDataManager();
		if (pXMLDataMng && pXMLDataMng->IsKindOf(RUNTIME_CLASS(CXMLDataManager)))
			strSiteCode = pXMLDataMng->GetCurrentSiteCode();
			 
		CCodeManager aCodeManager (pDoc, strSiteCode); 
		if (aCodeManager.GetNewKey ())
			return AssignNewFieldsToKey(pKeyList, pRec);
	}

	// devo comunque controllare che la chiave non sia presente su database
	SqlRecord *pCheckRec = SelectRecordFromDocTable (pKeyList);
	if (pCheckRec)
	{
		// se arrivo qui, tutti i tentativi di inserire un nuovo record
		// sono falliti
		return FALSE;
	}

	return TRUE;
}

//----------------------------------------------------------------------------
BOOL CDataManagerEvents::SelectUnKeyFromDocTable (CXMLNodeChildsList *pKeyList, CXMLNodeChildsList *pUnKeyList)
{
	SqlRecord * pRecord = SelectRecordFromDocTable(pKeyList);
	if (!pRecord)
		return FALSE;

	BOOL bOk = TRUE;
	bOk = AssignNewFieldsToUnKey(pUnKeyList, pRecord);
	
	return bOk;
}

//----------------------------------------------------------------------------
SqlRecord* CDataManagerEvents::SelectRecordFromDocTable (CXMLNodeChildsList *pKeyList)
{
	SqlRecord *pRec = GetRuntimeRecord ();
	if (!pRec || !pKeyList)
	{
		ASSERT(FALSE);
		return NULL;
	}

	SqlTable aTable(pRec,  GetDocument()->GetReadOnlySqlSession());
	SqlRecord* pRetVal = NULL;

	try
	{
		DBTMaster *pDBT = GetDBTMaster ();
		if (!pDBT) throw -1;
		
		aTable.Open ();
		
		// mi faccio inizializzare la query dal documento
		// per filtrare il giusto tipo di documento
		// (ad es., importa correttamente il TipoCliFor per i clienti/fornitori)
		pDBT->OnPrepareForXImportExport(&aTable);
			
		if (CreateWhereClause(pKeyList,  &aTable))
		{
			aTable.Query();

			if (!aTable.IsEmpty())
				pRetVal = aTable.GetRecord ();
		}

		aTable.Close ();
	}
	catch(...)
	{
		if (aTable.IsOpen ()) aTable.Close ();
		ASSERT(FALSE);
		return NULL;
	}

	return pRetVal;
}

//////////////////////////////////////////////////////////////////////////////
//             class CImportEvents implementation
//////////////////////////////////////////////////////////////////////////////
//=============================================================================

BEGIN_TB_EVENT_MAP(CImportEvents)
	TB_EVENT_EX (CImportEvents, LostAndFound, INT_PTR_VOIDPTR)
	TB_EVENT_EX (CImportEvents, AlternativeSearch, INT_PTR_VOIDPTR)
END_TB_EVENT_MAP

IMPLEMENT_DYNCREATE(CImportEvents, CDataManagerEvents);

//----------------------------------------------------------------------------
CClientDoc*	CImportEvents::GetClientDoc	()
{
	if (!m_pDocument || !m_pDocument->IsKindOf(RUNTIME_CLASS(CClientDoc)))
	{
		ASSERT(FALSE);
		return NULL;
	}
	return (CClientDoc*)m_pDocument;
}

//----------------------------------------------------------------------------
int CImportEvents::LostAndFound (void *pInOut)
{
	if (!CanGoOn()) return FUNCTION_OK;

	if (!pInOut) 
	{
		ASSERT(FALSE);
		return FUNCTION_ERROR;
	}
	
	CXMLNode *pInputKey = (CXMLNode*) pInOut;
	if (!AfxIsValidAddress(pInputKey, sizeof(CXMLNode)))
	{
		ASSERT(FALSE);
		return FUNCTION_ERROR;
	}

#ifdef _DEBUG
	CString strXML;
	pInputKey->GetXML (strXML);
	TRACE("CImportEvents::LostAndFound: XML of the Universal Key:\n");
	TRACE("%ws\n", strXML);
#endif
	
	//se ci fosse un tag error lo elimino (ritento l'operazione)
	CXMLNode *pError =  pInputKey->GetChildByName (DOC_XML_ERROR_TAG);
	if (pError)
		pInputKey->RemoveChild (pError); 

	CString strPrefix = GET_NAMESPACE_PREFIX(pInputKey);
	CXMLNodeChildsList *pKeyList = pInputKey->SelectNodes (strPrefix+DOC_XML_KEY_TAG, strPrefix);
	if (!pKeyList)
	{
		pError = pInputKey->CreateNewChild(DOC_XML_ERROR_TAG);
		if (pError)
			pError->SetText (cwsprintf(_TB("Node {0-%s} not found. "), DOC_XML_KEY_TAG));
		return FUNCTION_ERROR;
	}
	
	CXMLNodeChildsList *pUnKeyList = pInputKey->SelectNodes (strPrefix+XML_UNIVERSAL_KEY_TAG, strPrefix);
	if (!pUnKeyList)
	{
		pError = pInputKey->CreateNewChild(DOC_XML_ERROR_TAG);
		if (pError)
			pError->SetText (cwsprintf(_TB("Node {0-%s} not found. "), XML_UNIVERSAL_KEY_TAG));
		SAFE_DELETE(pKeyList);
		return FUNCTION_ERROR;
	}

	if (!SelectKeyFromDocTable(pKeyList, pUnKeyList) && 
		!SelectKeyFromLostAndFound(pKeyList, pUnKeyList))
	{	
		if (!GetNewKey(pKeyList))
		{
			pError = pInputKey->CreateNewChild(DOC_XML_ERROR_TAG);
			if (pError)
				pError->SetText (cwsprintf(_TB("Unable to get a new primary key for the document")));
			SAFE_DELETE(pKeyList);
			SAFE_DELETE(pUnKeyList);
			return FUNCTION_ERROR;
		}

		if (!InsertKeyIntoLostAndFound(pKeyList, pUnKeyList))
		{
			pError = pInputKey->CreateNewChild(DOC_XML_ERROR_TAG);
			if (pError)
				pError->SetText (cwsprintf(_TB("Unable to enter the new primary key in the Lost And Found table")));
			SAFE_DELETE(pKeyList);
			SAFE_DELETE(pUnKeyList);
			return FUNCTION_ERROR;
		}
	}

	SAFE_DELETE(pKeyList);
	SAFE_DELETE(pUnKeyList);

	return FUNCTION_OK;
}

//----------------------------------------------------------------------------
int CImportEvents::AlternativeSearch (void *pInOut)
{
	if (!CanGoOn()) return FUNCTION_OK;

	if (!pInOut) 
	{
		ASSERT(FALSE);
		return FUNCTION_ERROR;
	}

	CXMLNode *pInputKey = (CXMLNode*) pInOut;
	if (!AfxIsValidAddress(pInputKey, sizeof(CXMLNode)))
	{
		ASSERT(FALSE);
		return FUNCTION_ERROR;
	}

#ifdef _DEBUG
	CString strXML;
	pInputKey->GetXML (strXML);
	TRACE("CImportEvents::AlternativeSearch: XML of the Universal Key:\n");
	TRACE("%ws\n", strXML);
#endif

	//se ci fosse un tag error lo elimino (ritento l'operazione)
	CXMLNode *pError =  pInputKey->GetChildByName (DOC_XML_ERROR_TAG);
	if (pError)
		pInputKey->RemoveChild (pError); 

	CString strPrefix = GET_NAMESPACE_PREFIX(pInputKey);
	CXMLNodeChildsList *pKeyList = pInputKey->SelectNodes (strPrefix+DOC_XML_KEY_TAG, strPrefix);
	if (!pKeyList)
	{
		pError = pInputKey->CreateNewChild(DOC_XML_ERROR_TAG);
		if (pError)
			pError->SetText (cwsprintf(_TB("Node {0-%s} not found. "), DOC_XML_KEY_TAG));
		return FUNCTION_ERROR;
	}
	
	CXMLNodeChildsList *pUnKeyList = pInputKey->SelectNodes (strPrefix+XML_UNIVERSAL_KEY_TAG, strPrefix);
	if (!pUnKeyList)
	{
		pError = pInputKey->CreateNewChild(DOC_XML_ERROR_TAG);
		if (pError)
			pError->SetText (cwsprintf(_TB("Node {0-%s} not found. "), XML_UNIVERSAL_KEY_TAG));
		SAFE_DELETE(pKeyList);
		return FUNCTION_ERROR;
	}
	
	if (!SelectKeyFromDocTable(pKeyList, pUnKeyList) && 
		!SelectKeyFromLostAndFound(pKeyList, pUnKeyList))
	{
		if (!GetNewKey(pKeyList))
		{
			pError = pInputKey->CreateNewChild(DOC_XML_ERROR_TAG);
			if (pError)
				pError->SetText (cwsprintf(_TB("Unable to get a new primary key for the document")));
			SAFE_DELETE(pKeyList);
			SAFE_DELETE(pUnKeyList);
			return FUNCTION_ERROR;
		}

		// inserisco il record nella tabella solo se sto tasformando
		// la chiave per un external reference (altrimenti non ha senso inserire 
		// provvisoriamente qualcosa che verrebbe inserito subito dopo dalla
		// procedura di import, perché avverrebbe un aggiornamento invece di un inserimento
		// e questo non va bene qualora i parametri del documento non lascino la 
		// possibilità di effettuare aggiornamenti su record esistenti
		// in tal caso uso la lostandfound
		CString strIsExtRef;
		pInputKey->GetAttribute (DOC_XML_EXTREF_ATTRIBUTE, strIsExtRef);
		BOOL bIsForExtRef = strIsExtRef.CompareNoCase (FormatBoolForXML(TRUE)) == 0;
		if (bIsForExtRef)
		{
			if (!InsertKeyIntoDocTable(pKeyList, pUnKeyList))
			{
				pError = pInputKey->CreateNewChild(DOC_XML_ERROR_TAG);
				if (pError)
					pError->SetText (cwsprintf(_TB("Unable to enter the new primary key in the document table")));
				SAFE_DELETE(pKeyList);
				SAFE_DELETE(pUnKeyList);
				return FUNCTION_ERROR;
			}
		}
		else
		{
			if (!InsertKeyIntoLostAndFound(pKeyList, pUnKeyList))
			{
				pError = pInputKey->CreateNewChild(DOC_XML_ERROR_TAG);
				if (pError)
					pError->SetText (cwsprintf(_TB("Unable to enter the new primary key in the Lost And Found table")));
				SAFE_DELETE(pKeyList);
				SAFE_DELETE(pUnKeyList);
				return FUNCTION_ERROR;
			}
		}
	}

	SAFE_DELETE(pKeyList);
	SAFE_DELETE(pUnKeyList);

	return FUNCTION_OK;
}

//----------------------------------------------------------------------------
BOOL CImportEvents::RemoveFromLostAndFound (CXMLNode *pUniversalKey)
{
	CString strPrefix = GET_NAMESPACE_PREFIX(pUniversalKey);
	CXMLNodeChildsList *pKeyList = pUniversalKey->SelectNodes (strPrefix+DOC_XML_KEY_TAG, strPrefix);
	
	if (!pKeyList)
		return FALSE;

	BOOL bResult = DeleteKeyFromLostAndFound(pKeyList);
	SAFE_DELETE(pKeyList);
	return bResult;
}



//////////////////////////////////////////////////////////////////////////////
//             class CExportEvents implementation
//////////////////////////////////////////////////////////////////////////////
//=============================================================================

BEGIN_TB_EVENT_MAP(CExportEvents)
	TB_EVENT_EX (CExportEvents, LostAndFound, INT_PTR_VOIDPTR)
	TB_EVENT_EX (CExportEvents, AlternativeSearch, INT_PTR_VOIDPTR)
END_TB_EVENT_MAP

IMPLEMENT_DYNCREATE(CExportEvents, CDataManagerEvents);

//----------------------------------------------------------------------------
CClientDoc*	CExportEvents::GetClientDoc	()
{
	if (!m_pDocument || !m_pDocument->IsKindOf(RUNTIME_CLASS(CClientDoc)))
	{
		ASSERT(FALSE);
		return NULL;
	}
	return (CClientDoc*)m_pDocument;
}

//----------------------------------------------------------------------------
int CExportEvents::LostAndFound (void *pInOut)
{
	if (!CanGoOn()) return FUNCTION_OK;

	if (!pInOut) 
	{
		ASSERT(FALSE);
		return FUNCTION_ERROR;
	}
	
	CXMLNode *pInputKey = (CXMLNode*) pInOut;
	if (!AfxIsValidAddress(pInputKey, sizeof(CXMLNode)))
	{
		ASSERT(FALSE);
		return FUNCTION_ERROR;
	}
	
	//se ci fosse un tag error lo elimino (ritento l'operazione)
	CXMLNode *pError =  pInputKey->GetChildByName (DOC_XML_ERROR_TAG);
	if (pError)
		pInputKey->RemoveChild (pError); 

	CString strPrefix = GET_NAMESPACE_PREFIX(pInputKey);
	CXMLNodeChildsList *pKeyList = pInputKey->SelectNodes (strPrefix+DOC_XML_KEY_TAG, strPrefix);
	if (!pKeyList)
	{
		pError = pInputKey->CreateNewChild(DOC_XML_ERROR_TAG);
		if (pError)
			pError->SetText (cwsprintf(_TB("Node {0-%s} not found. "), DOC_XML_KEY_TAG));
		return FUNCTION_ERROR;
	}
	
	CXMLNodeChildsList *pUnKeyList = pInputKey->SelectNodes (strPrefix+XML_UNIVERSAL_KEY_TAG, strPrefix);
	if (!pUnKeyList)
	{
		pError = pInputKey->CreateNewChild(DOC_XML_ERROR_TAG);
		if (pError)
			pError->SetText (cwsprintf(_TB("Node {0-%s} not found. "), XML_UNIVERSAL_KEY_TAG));
		SAFE_DELETE(pKeyList);
		return FUNCTION_ERROR;
	}

	if (!SelectUnKeyFromDocTable(pKeyList, pUnKeyList))
	{
		pError = pInputKey->CreateNewChild(DOC_XML_ERROR_TAG);
		if (pError)
			pError->SetText (cwsprintf(_TB("Unable to get the alternative key from the document table")	));
		SAFE_DELETE(pKeyList);
		SAFE_DELETE(pUnKeyList);
		return FUNCTION_ERROR;
	}

	SAFE_DELETE(pKeyList);
	SAFE_DELETE(pUnKeyList);

	return  FUNCTION_OK;
}

//----------------------------------------------------------------------------
int CExportEvents::AlternativeSearch (void *pInOut)
{
	return LostAndFound(pInOut);
}