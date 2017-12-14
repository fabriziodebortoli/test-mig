#include "stdafx.h"
#include <oledberr.h>

#include <TbGeneric\DataObj.h>

#include "sqlcatalog.h"
#include "oledbmng.h"
#include "sqltable.h"
#include "sqlaccessor.h"
#include "wclause.h"

//includere come ultimo include all'inizio del cpp
#include "begincpp.dex"

#ifdef _DEBUG
#undef THIS_FILE
static const char  THIS_FILE[] = __FILE__;
#endif

#ifdef _DEBUG
#define new DEBUG_NEW
#endif

#define STATUS_LEN			4
#define LENGTH_LEN			4
#define BUFFER_SIZE			4096

const int nEmptySqlRecIdx	= -1;

/////////////////////////////////////////////////////////////////////////////
//						SqlBindingElem
/////////////////////////////////////////////////////////////////////////////
//
//-----------------------------------------------------------------------------
IMPLEMENT_DYNAMIC(SqlBindingElem, CObject)

//-----------------------------------------------------------------------------
SqlBindingElem::SqlBindingElem
	(
		const CString&		strBindName, 
		const DBTYPE&		eDBType,
		DataObj*			pDataObj,
		const int&			nSqlRecIdx,
		const DBPARAMIO&	eType /*= DBPARAMIO_NOTPARAM*/,
		const CString&		strColumnName
	)
	:
	m_strBindName	(strBindName),
	m_pDataObj		(pDataObj),
    m_pOldDataObj	(NULL),
	m_pBindingBlob	(NULL),
	m_dwStatus		(DBSTATUS_E_UNAVAILABLE),
	m_nDBType		(eDBType),
	m_eParamIO		(eType),
	m_bUpdatable	(FALSE),
	m_bOwnData		(FALSE),
	m_bAutoIncrement(FALSE),
	m_bUseLength	(FALSE),
	m_nSqlRecIdx	(nSqlRecIdx),
	m_bReadOnly		(FALSE)
{ 
	// m_pDataObj lo creo per i parametri di INPUT negli altri casi mi viene passato 
	// puntatore

	DBLENGTH nAllocSize;
	if (m_pDataObj)
	{ 
		nAllocSize = (eDBType == DBTYPE_WSTR || eDBType == DBTYPE_STR)
					?  m_pDataObj->GetColumnLen()
					:  m_pDataObj->GetOleDBSize();
		// m_pOldDataObj mi serve per le operazioni di modica del parametro di bind
		// in questo modo modifico il buffer solo se il dato é effettivamente cambiato
		// preallochiamo la dimensione del campo in base alla lunghezza definita
		m_pOldDataObj = DataObj::DataObjCreate(m_pDataObj->GetDataType());
		m_pOldDataObj->	SetAllocSize(nAllocSize);

		//se sono alla presenza di un campo di tipo blob devo istanziare l'ausilio per i blob
		if (m_pDataObj->GetDataType() == DATA_TXT_TYPE)
			m_pBindingBlob = new CBindingBlob(m_pDataObj, (strColumnName.IsEmpty()) ? strBindName : strColumnName);
	}
	else
		ASSERT(FALSE);	
}

//-----------------------------------------------------------------------------
SqlBindingElem::~SqlBindingElem()
{
	if (m_pOldDataObj)
		delete m_pOldDataObj;
	
	if (m_pDataObj && m_bOwnData)
		delete m_pDataObj;

	if (m_pBindingBlob)
		delete m_pBindingBlob;
}

//-----------------------------------------------------------------------------
void SqlBindingElem::ManageUnicodeInDB (SqlRowSet* pRowSet) 
{
	if	(!m_pDataObj || m_pDataObj->GetDataType() != DATA_TXT_TYPE || !pRowSet || !m_pBindingBlob)
		return;

	CString strTableName, strColName;
	int nIdx = m_pBindingBlob->m_strColumnName.Find(DOT_CHAR);
	
	// per fisico si intende un dato appartenente and una tabella ossia : table.variabile
	if (nIdx > 0)
	{
		strTableName = m_pBindingBlob->m_strColumnName.Left(nIdx);					
		strColName = m_pBindingBlob->m_strColumnName.Mid(nIdx + 1);
	}	
	else
		strColName = m_pBindingBlob->m_strColumnName;

	TRY
	{
		// is a sqltable working in select
		if (pRowSet->IsKindOf(RUNTIME_CLASS(SqlTable)))
		{
			SqlRecord* pRecord = NULL;
			SqlTable* pTable = (SqlTable*) pRowSet;
				
			//se non ho nessun sqlrecord associato o nessuna join allora imposto la proprietà dell'unicode a TRUE
			if (!pTable->GetRecord() && (!pTable->GetTableArray() || pTable->GetTableArray()->GetCount()  <= 0))
			{
				m_pBindingBlob->SetUnicodeInDB(TRUE);
				return;
			}

			//first I search in the primary table
			if (strTableName.IsEmpty() || pTable->GetTableName().CompareNoCase(strTableName) == 0 || strTableName.CompareNoCase(pTable->GetAlias()) == 0)
			{
				pRecord = pTable->GetRecord();
			}
			else
				//se sono alla presenza di una join tra più tabelle
				if (!strTableName.IsEmpty() && pTable->GetTableArray() && pTable->GetTableArray()->GetCount() > 0)
				{
					SqlTableItem* joinTable = pTable->GetTableArray()->GetTableByName(strTableName, strTableName);
					//se non l'ho trovata vado per alias
					if(!joinTable)
					{
						int nIdx = pTable->GetTableArray()->GetTableIndex(strTableName);
						joinTable = (nIdx > -1) ? joinTable : NULL;
					}

					if (joinTable && joinTable->m_pRecord)
						pRecord = joinTable->m_pRecord;						
				}
			
			int nPos = (pRecord) ? pRecord->GetIndexFromColumnName(strColName) : -1;
			
			if (nPos < 0)
			{
				m_pBindingBlob->SetUnicodeInDB(TRUE);
				if (m_eParamIO != DBPARAMTYPE_INPUT)
				{
					ASSERT(FALSE);
					TRACE2 ("Error binding blob management for table %s, column %s ", strTableName, m_strBindName);
				}
			}
			else
			{
				const SqlColumnInfo* pColInfo = pRecord->GetColumnInfo(nPos);
				
				BOOL bUnicode = pColInfo ? pColInfo->m_nSqlDataType == DBTYPE_WSTR : TRUE;

				//if (!bUnicode && pRecord->IsKindOf(RUNTIME_CLASS(SqlRecordDynamic)) && pColInfo && pColInfo->GetTableName().Compare(_T("_DYNAMIC_")) == 0)
				//{
				//	bUnicode = TRUE; // pRecord->GetConnection() ? pRecord->GetConnection()->UseUnicode() : FALSE;
				//}

				m_pBindingBlob->SetUnicodeInDB(bUnicode);
			}
			return;			
		}

		if (!pRowSet->m_pSqlConnection)
		{
			ASSERT(FALSE);
			TRACE1 ("No SqlConnection information available. Error binding blob management for table column %s ", m_strBindName);
			return;
		}

		// generic rowsets working on other operations
		for (int i=0; i <= pRowSet->m_strCurrentTables.GetUpperBound(); i++)
		{
			const SqlTableInfo* pTableInfo = pRowSet->m_pSqlConnection->GetTableInfo(pRowSet->m_strCurrentTables.GetAt(i));
			if (!pTableInfo)
				continue;
			
			if (strTableName.IsEmpty() || pTableInfo->GetTableName().CompareNoCase(strTableName) != 0)
			{
				if (!m_pBindingBlob)
				{
					// is a datatext without binding blob!
					ASSERT(FALSE);
					TRACE2 ("Error binding blob management for table %s, column %s ", pRowSet->m_strCurrentTables.GetAt(i), m_strBindName);
					return;
				}

				const SqlColumnInfo* pColInfo = pTableInfo->GetColumnInfo(strColName);
				if (!pColInfo)
				{
					m_pBindingBlob->SetUnicodeInDB(TRUE);
					if (m_eParamIO != DBPARAMTYPE_INPUT)
					{
						ASSERT(FALSE);
						TRACE2 ("Error binding blob management for table %s, column %s ", pTableInfo->GetTableName(), m_strBindName);
					}
				}	
				else
					m_pBindingBlob->SetUnicodeInDB(pColInfo->m_nSqlDataType == DBTYPE_WSTR); 
				return;
			}			
		}
	}
	CATCH(SqlException, e)
	{
		// is a datatext without binding blob!
		ASSERT(FALSE);
		TRACE1 ("Error binding blob management for column %s ", m_strBindName);
	}
	END_CATCH
}

//-----------------------------------------------------------------------------
BOOL SqlBindingElem::IsUnicodeInDB () const
{
	if (m_pBindingBlob)
		return m_pBindingBlob->IsUnicodeInDB();
	
	return FALSE;
}

// Salva il precedente parametro per minimizzare le requery con stessi valori
//-----------------------------------------------------------------------------
void SqlBindingElem::SetParamValue(const DataObj& aDataObj)
{ 
	AssignOldDataObj(*m_pDataObj);
	m_pDataObj->Assign(aDataObj);
}

//-----------------------------------------------------------------------------
DataType SqlBindingElem::GetDataType() const
{
	if (!m_pDataObj)
	{
		ASSERT(FALSE);
		return DataType::Null;
	}
	return m_pDataObj->GetDataType();
}

//-----------------------------------------------------------------------------
void SqlBindingElem::AssignOldDataObj(const DataObj& aDataObj)
{
	m_pOldDataObj->Assign(aDataObj);
}

//-----------------------------------------------------------------------------
void SqlBindingElem::ClearOldDataObj()
{
	m_pOldDataObj->Clear(); 
}
	
//-----------------------------------------------------------------------------
void SqlBindingElem::ReadBlobValue(BYTE* pBuffer)
{
	m_pDataObj->Clear();
	m_pBindingBlob->Read(pBuffer);
}

//-----------------------------------------------------------------------------
long SqlBindingElem::WriteBlobValue(BYTE* pBuffer)
{
	return m_pBindingBlob->Write(pBuffer);
}

//-----------------------------------------------------------------------------
CString SqlBindingElem::GetBindName(BOOL bQualified /*= FALSE*/)
{
	CString sBind = m_strBindName;
	if (!bQualified)
	{
		int idx = sBind.Find('.');
		if (idx > 0)
			sBind = sBind.Mid(idx + 1);
	}
	return sBind;
}
/////////////////////////////////////////////////////////////////////////////
//						SqlBindingElemArray SqlObject
/////////////////////////////////////////////////////////////////////////////
//
//-----------------------------------------------------------------------------
IMPLEMENT_DYNAMIC(SqlBindingElemArray, Array)
	
/////////////////////////////////////////////////////////////////////////////
//						SqlAccessor SqlObject
/////////////////////////////////////////////////////////////////////////////
//
//-----------------------------------------------------------------------------
IMPLEMENT_DYNAMIC(SqlAccessor, SqlBindingElemArray)

//-----------------------------------------------------------------------------
SqlAccessor::SqlAccessor(SqlRowSet* pSqlRowSet) 
: 
	m_pBindBuffer		(NULL),
	m_bBlobAlreadyBinded(FALSE),
	m_pSqlRowSet		(pSqlRowSet)
{
	Clear();
}

//-----------------------------------------------------------------------------
SqlAccessor:: ~SqlAccessor()
{	
	if (m_pBindBuffer)
		delete[] m_pBindBuffer; 
} 

//-----------------------------------------------------------------------------
DBLENGTH SqlAccessor::GetBindLen(SqlBindingElem* pElem) const
{ 
	if (!pElem || !pElem->m_pDataObj)
		return 0;

	DataObj* pDataObj = pElem->m_pDataObj;

	if (pDataObj->GetDataType() == DATA_GUID_TYPE && m_pSqlRowSet->m_pSqlConnection->GetDBMSType() != DBMS_SQLSERVER)
		return (pElem->m_nDBType == DBTYPE_WSTR)
				? pDataObj->GetAllocSize() * sizeof(TCHAR) + sizeof(TCHAR)
				: pDataObj->GetAllocSize() + sizeof(CHAR);
	
	return pDataObj->GetOleDBSize(pElem->m_nDBType == DBTYPE_WSTR);
}

//-----------------------------------------------------------------------------
BOOL SqlAccessor::CanAddStorageObject() const
{
	if (m_pSqlRowSet->m_pSqlConnection->GetProviderInfo()->MultiStorageObjects())
		return FALSE;
	
	return !m_bBlobAlreadyBinded;
}

//-----------------------------------------------------------------------------
int	SqlAccessor::Add
					(
						const CString&	strName, 
							DataObj*	pDataObj, 
						const DBTYPE&	eDBType,			  
						const DBPARAMIO& eType,
						const int&		nSqlRecIdx,
							BOOL		bAutoIncrement /*=FALSE*/,
						const CString&	strColumnName,
							 int		nInsertPos /*= -1*/ //se valorizzato il parametro viene inserito nell'nInsertPos posizione dell'array
					)
{

	m_pSqlRowSet->MakeProcTimeOperation(START_TIME, PROC_ADD_BIND_ELEMENT);	

	if (eDBType == DBTYPE_IUNKNOWN)
	{
		if (!CanAddStorageObject())
		{
			ASSERT(FALSE);
			TRACE("SqlAccessor::Add: it's not possible in a query to have two or more columns with DataText type");
			return -1;
		}
		m_bBlobAlreadyBinded = TRUE;
	}

	
	// lo stato non mi serve se sono dei parametri in input
	if (eType !=  DBPARAMIO_INPUT)  
		m_nOutNumb++;

	SqlBindingElem* pBindElem = new SqlBindingElem(strName, eDBType, pDataObj, nSqlRecIdx, eType, strColumnName);
	pBindElem->ManageUnicodeInDB(m_pSqlRowSet);
	pBindElem->m_bAutoIncrement = bAutoIncrement;

	m_lBufferSize += GetBindLen(pBindElem);

	// nel buffer di binding devo passare/ricevere anche l'informazione della lunghezza del dato da fornire/leggere al/dal provider
	// La lunghezza serve per :
	// i DataText : lettura/scrittura attraverso uno stream
	// i DataStr in Oracle a causa del padding delle stringhe
	pBindElem->m_bUseLength = pDataObj->GetDataType() == DATA_TXT_TYPE || 
							  (m_pSqlRowSet->m_pSqlConnection->GetDBMSType() == DBMS_ORACLE && pDataObj->GetDataType() == DATA_STR_TYPE);

	if (pBindElem->m_bUseLength) 
		m_nUseLengthNumb++;
	
	int nIdx = -1;
	if (nInsertPos > -1 && GetSize() > 0 && nInsertPos >=0 && nInsertPos < GetSize()-1)
	{
		__super::InsertAt(nInsertPos, pBindElem);	
		nIdx = nInsertPos;
	}
	else
		nIdx = __super::Add(pBindElem);	
	m_pSqlRowSet->MakeProcTimeOperation(STOP_TIME, PROC_ADD_BIND_ELEMENT);
	return nIdx;
}

//-----------------------------------------------------------------------------
int SqlAccessor::Add
					(
						const CString&	strName, 
						const DataType& nDataType, 
						const DBTYPE&	eDBType,
						const DBLENGTH& nLen, 
						const int&		nSqlRecIdx,
						const DBPARAMIO& eType /*= DBPARAMIO_INPUT*/,
						const CString&	strColumnName,
							 int		nInsertPos /*= -1*/ //se valorizzato il parametro viene inserito nell'nInsertPos posizione dell'array
					)
{
	DataObj* pDataObj = DataObj::DataObjCreate(nDataType);
	// preallochiamo la dimensione del campo in base alla lunghezza definita
	pDataObj->SetAllocSize(nLen);

	int nIdx = Add(strName, pDataObj, eDBType, eType, nSqlRecIdx, FALSE, strColumnName, nInsertPos);
	GetAt(nIdx)->m_bOwnData = TRUE; //lo devo distruggere nel distruttore
	
	return nIdx;
}


//-----------------------------------------------------------------------------
SqlBindingElem* SqlBindingElemArray::GetParamByName(const CString& strName)
{
	BOOL bQualified = strName.Find('.')  > 0;

	for (int i = 0; i <= GetUpperBound(); i++ )
	{
		CString sBind = GetAt(i)->GetBindName(bQualified);
		if (sBind.CompareNoCase(strName) == 0)
			return GetAt(i);
	}
	
	return NULL;	
}

//-----------------------------------------------------------------------------
SqlBindingElem* SqlBindingElemArray::GetElemByDataObj(const DataObj* pDataObj)
{
	for (int i = 0; i <= GetUpperBound(); i++ )
		if (GetAt(i)->GetDataObj() == pDataObj)
			return GetAt(i);
	
	return NULL;	
}

//-----------------------------------------------------------------------------
DataObj* SqlBindingElemArray::GetDataObj(const CString& strName)
{
	SqlBindingElem* pElem = GetParamByName(strName);
	
	return (pElem) ? pElem->m_pDataObj : NULL;
}

//-----------------------------------------------------------------------------
DataObj* SqlBindingElemArray::GetOldDataObj(const CString& strName)
{
	SqlBindingElem* pElem = GetParamByName(strName);
	
	return (pElem) ? pElem->m_pOldDataObj : NULL;
}

//-----------------------------------------------------------------------------
DataObj* SqlBindingElemArray::GetDataObjAt(int nIdx)
{ 
	return GetAt(nIdx)->m_pDataObj;		
}

//-----------------------------------------------------------------------------
const CString& SqlBindingElemArray::GetParamName(int nIdx)	
{ 
	return GetAt(nIdx)->m_strBindName;	
}

//-----------------------------------------------------------------------------
BOOL SqlBindingElemArray::SameValues() const
{
	for (int i = 0; i <= GetUpperBound(); i++)
		if (!GetAt(i)->SameValue())
			return FALSE;
			
	return TRUE;
}

//-----------------------------------------------------------------------------
BOOL SqlAccessor::InitBuffer()
{
	if (!m_pBindBuffer)
		return FALSE;

	BYTE*	 pCurrPos	= m_pBindBuffer;
	BYTE*	 pStatus	= NULL;
	BYTE*	 pLength = NULL;
    DBLENGTH lLen; 

	SqlBindingElem* pElem = NULL;	

	m_pSqlRowSet->MakeProcTimeOperation(START_TIME, PROC_INIT_BUFFER);

	for (int nIdx = 0; nIdx < GetSize(); nIdx++)
	{
		pElem = GetAt(nIdx);
		if (!pElem)
			continue;

		pElem->ClearOldDataObj();
		pElem->m_dwStatus = DBSTATUS_E_UNAVAILABLE;
		lLen	= GetBindLen(pElem);
		pLength = NULL;
		
		switch (pElem->m_nDBType)
		{
			// Per i DataText in SqlServer azzero solo il campo relativo alla lunghezza		
			case DBTYPE_IUNKNOWN:
				pLength = pCurrPos + lLen;
				pStatus = pLength + LENGTH_LEN;
				break;
			
			case DBTYPE_WSTR:
			case DBTYPE_STR:
				memset(pCurrPos, 0, lLen); //pulisco il contenuto del dato
				if (pElem->m_bUseLength)
				{
					pLength = pCurrPos + lLen;
					pStatus = pLength + LENGTH_LEN;					
					
				}
				else
					pStatus = pCurrPos + lLen;
				break;
				
			default:
				memset(pCurrPos, 0, lLen); //pulisco il contenuto del dato
				pStatus = pCurrPos + lLen;
		}	


		if (pLength)
			memset(pLength, 0, LENGTH_LEN); //pulisco il contenuto della lunghezza
		memcpy(pStatus, &(pElem->m_dwStatus), STATUS_LEN); //metto lo stato a DBSTATUS_E_UNAVAILABLE
		pCurrPos = pStatus + STATUS_LEN;	
	}
	m_pSqlRowSet->MakeProcTimeOperation(STOP_TIME, PROC_INIT_BUFFER);
	return TRUE;
}

// copia il valore contenuto negli elementi di binding nel buffer di appoggio per 
// il CAccessor
// Tre casi diversi:
// DBPARAMIO_NOPARAM: sono in fase di insert/update di un record. Aggiorno lo stato dei
// solo campi modificati (se sono in update)
// DBPARAMIO_INPUT: per le query con i parametri. Aggiorno il valore dei parametri 9se questo é cambiato)
// DBPARAMIO_OUTPUT | DBPARAMIO_OUTPUT: per le storedprocedure con i parametri di tipo input/output

// il parametro viene considerato in fase di scrittura/lettura se:
// é la prima volta che si esegue la query (lo stato é DBSTATUS_E_UNAVAILABLE)
// é cambiato il valore rispetto alla fetch precedente
// viene rieseguita la query e bisogna considerare i parametri in input (vedi 
//-----------------------------------------------------------------------------
BOOL SqlAccessor::FixupBuffer(BOOL bForced)
{
	USES_CONVERSION;

	if (!m_pBindBuffer) //@@OLEexception TODO
		return FALSE;

	BYTE*	 pCurrPos	= m_pBindBuffer;
	DataObj* pDataObj	= NULL;
    DBLENGTH lLen; 

	
	BOOL bUpdate = FALSE;	
	BOOL bStatusOk = bForced;
	long lWrite = 0;

	SqlBindingElem* pElem = NULL;
	
	m_pSqlRowSet->MakeProcTimeOperation(START_TIME, PROC_FIXUP_OPERATION);

	for (int nIdx = 0; nIdx < GetSize(); nIdx++)
	{
		pElem = GetAt(nIdx);
        if (!pElem)
			continue;

		pDataObj = pElem->m_pDataObj;		
		lLen	 = GetBindLen(pElem);	

		// la prima volta che aggiorno il buffer ho lo stato in DBSTATUS_E_UNAVAILABLE
		// anche per i parametri di solo input
		if (pElem->m_bAutoIncrement && m_pSqlRowSet->m_pSqlConnection->GetDBMSType() == DBMS_SQLSERVER)
			bStatusOk = FALSE; //in questo modo non devo salvare il dato di tipo incremental
		else
		{
			if 	(
					bForced ||
					pElem->m_dwStatus == DBSTATUS_E_UNAVAILABLE ||
					!pElem->SameValue() 
				)				
			
			{	
				bStatusOk = TRUE; // devo settare lo status a OK
				if (pElem->m_nDBType != DBTYPE_IUNKNOWN)
					memset(pCurrPos, 0, lLen);
				bUpdate = TRUE; //cé almeno un valore invariato.... E'possibile fare il salvataggio
				// setto lo stato a OK in modo da poter salvare sul db la modifica
				
				switch (pElem->m_nDBType)
				{
					case DBTYPE_WSTR:
					case DBTYPE_STR:
						// per i parametri in OUTPUT delle stored procedure di ORACLE 
						if (m_pSqlRowSet->m_pSqlConnection->GetDBMSType() == DBMS_ORACLE &&	(pElem->m_eParamIO & DBPARAMIO_OUTPUT) == DBPARAMIO_OUTPUT)
						{
							memcpy(pCurrPos + lLen, &lLen, LENGTH_LEN);
							break;
						}

						// per i booleani ho lunghezza fissa 1 e due valori possibili				
						if (pElem->GetDataType() == DATA_BOOL_TYPE)
						{
							if (pElem->m_nDBType == DBTYPE_STR)
							{
								CHAR aBoolChar;
								aBoolChar = ((*(DataBool*)pDataObj) == TRUE) ? '1' : '0';
								memcpy(pCurrPos, &aBoolChar, sizeof(CHAR));
							}
							else
							{
								TCHAR aBoolTChar;
								aBoolTChar = ((*(DataBool*)pDataObj) == TRUE) ? _T('1') : _T('0');
								memcpy(pCurrPos, &aBoolTChar, sizeof(TCHAR));
							}
							break;
						} 

						// per i DataGuid in ORACLE ho lunghezza fissa a 38
						if (pElem->GetDataType() == DATA_GUID_TYPE)
						{
							CString strGUID = ((DataGuid*)pDataObj)->Str(1);
							memcpy(
									pCurrPos, 
									(pElem->m_nDBType == DBTYPE_WSTR) ? (void*)((LPCTSTR)strGUID) : (void*)(T2A(strGUID)), 
									lLen);
							break;
						}

						if (pElem->GetDataType() == DATA_STR_TYPE)
						{
							if (((DataStr*)pDataObj)->GetLen() > 1)	
								((DataStr*)pDataObj)->StripBlank();
							//I need to truncate the strings longer than allocsize
							DBLENGTH lSizeof = (pElem->m_nDBType == DBTYPE_WSTR) ? sizeof(TCHAR) : sizeof(CHAR);							
							lWrite = (((DataStr*)pDataObj)->GetLen() * lSizeof < lLen - lSizeof)
										? ((DataStr*)pDataObj)->GetLen() * lSizeof
										: lLen - lSizeof;
							
							// In ORACLE: if it is an empty string I need to store it as blank string (see in ORACLE the difference
							// beetween empty and NULL string)
							if(pDataObj->IsEmpty())
							{
								if (m_pSqlRowSet->m_pSqlConnection->GetDBMSType() == DBMS_ORACLE)
								{
									if (lWrite == 0) lWrite = lSizeof;
									TCHAR p = BLANK_CHAR;
									memcpy(pCurrPos, &p, lSizeof);
								}
							}//se SQLSERVER non scrivo niente poichè la porzione del buffer è stata già messa a empty
							else
							{
								void* p = pDataObj->GetOleDBDataPtr();
								if (pElem->m_nDBType != DBTYPE_WSTR)
								{
									USES_CONVERSION;
									p = T2A((LPCTSTR)p);
								}
								memcpy(pCurrPos, p, lWrite);
							}
						}

						if (pElem->m_bUseLength)						
							memcpy(pCurrPos + lLen, &lWrite, LENGTH_LEN);
						break;
				
					// per i DataText in SqlServer
					case DBTYPE_IUNKNOWN: 
							pElem->ManageUnicodeInDB(m_pSqlRowSet);
							lWrite = pElem->WriteBlobValue(pCurrPos);						
							memcpy(pCurrPos + lLen, &lWrite, LENGTH_LEN);
						break;

					default:
						memcpy(pCurrPos, pDataObj->GetOleDBDataPtr(), lLen);
				}
				pElem->AssignOldDataObj(*pDataObj);
			}
			//in questo modo si decide se salvare o meno i dati modificati
			pElem->m_dwStatus = (bStatusOk) ? DBSTATUS_S_OK : DBSTATUS_S_IGNORE;
		}

		pCurrPos += lLen;
		if (pElem->m_bUseLength)
			pCurrPos += LENGTH_LEN;	
	
		if ((pElem->m_eParamIO & DBPARAMIO_OUTPUT) == DBPARAMIO_OUTPUT || pElem->m_eParamIO == DBPARAMIO_NOTPARAM)
		{
			memcpy(pCurrPos, &(pElem->m_dwStatus), STATUS_LEN);
			pCurrPos += STATUS_LEN;
		}
	}
	m_pSqlRowSet->MakeProcTimeOperation(STOP_TIME, PROC_FIXUP_OPERATION);

	return bUpdate;
}

// dal buffer di appoggio dell'Accessor trasferisco le informazioni lette dal database
// ai dataobj bindati. Questo ha senso solo per le colonne bindate e per i parametri di output

// il parametro bOnlyAutoInc mi permette di valorizzare solo i campi autoincrement dopo
// un'operazione di inserimento
//-----------------------------------------------------------------------------
BOOL SqlAccessor::FixupBindElements(BOOL bOnlyAutoInc /*=FALSE*/)
{
	if (!m_pBindBuffer) 
		return FALSE;
		
	BYTE*	 pCurrPos	= m_pBindBuffer;
	DataObj* pDataObj	= NULL;
    DBLENGTH lLen; 
	
	long lByte = 0;
	BOOL bUseLength = FALSE;

	SqlBindingElem* pElem = NULL;

    m_pSqlRowSet->MakeProcTimeOperation(START_TIME, PROC_FIXUP_OPERATION);

	for (int nIdx = 0; nIdx < GetSize(); nIdx++)
	{
		pElem = GetAt(nIdx);
        if (!pElem)
			continue;

		// skippo i parametri di input
		if (pElem->m_eParamIO == DBPARAMIO_INPUT)
		{
			pCurrPos += GetBindLen(pElem);
			if (pElem->m_bUseLength)
				pCurrPos += LENGTH_LEN;
			continue;
		}

		pDataObj = pElem->m_pDataObj;		
		lLen	 = GetBindLen(pElem);
		
		//leggo lo stato dal buffer
		if (pElem->m_bUseLength)
			memcpy(&(GetAt(nIdx)->m_dwStatus), pCurrPos + lLen + LENGTH_LEN, STATUS_LEN);
		else
			memcpy(&(GetAt(nIdx)->m_dwStatus), pCurrPos + lLen, STATUS_LEN);

		if (
				(bOnlyAutoInc && pElem->m_bAutoIncrement  && m_pSqlRowSet->m_pSqlConnection->GetDBMSType() == DBMS_SQLSERVER) ||
				(!bOnlyAutoInc && pElem->m_dwStatus ==  DBSTATUS_S_OK)
			)
		{
			switch (pElem->m_nDBType)
			{
				case DBTYPE_WSTR:
				case DBTYPE_STR:

					// DataBool 
					if (pElem->GetDataType() == DATA_BOOL_TYPE)
					{
						((DataBool*)pDataObj)->Assign(pCurrPos, 0, pElem->m_nDBType == DBTYPE_WSTR); 
						break;
					}

					// DataGuid in ORACLE
					if (pDataObj->GetDataType() == DATA_GUID_TYPE)
					{
                        ((DataGuid*)pDataObj)->Assign((LPCTSTR)pCurrPos);
						break;
					}

					if (pElem->GetDataType() == DATA_STR_TYPE)
					{
						// DataStr ed in Oralce anche i DataText
						lByte = 0;
						if (pElem->m_bUseLength) //devo leggere la lunghezza del dato letto
							memcpy(&lByte, pCurrPos + lLen, LENGTH_LEN);
						//In Oracle the NULL string is handled as blank string
						long lSizeof = (pElem->m_nDBType == DBTYPE_WSTR) ? sizeof(BLANK_CHAR) : sizeof(CHAR);
						if (m_pSqlRowSet->m_pSqlConnection->GetDBMSType() == DBMS_ORACLE && lByte == lSizeof) 
						{
							TCHAR p = NULL; 
							memcpy(&p, pCurrPos, lByte);
							if (p == BLANK_CHAR)
								((DataStr*)pDataObj)->Clear();
							else
								((DataStr*)pDataObj)->Assign(pCurrPos, lByte, pElem->m_nDBType == DBTYPE_WSTR);
						}
						else
						{
							// okkio che con DB Oracle e campo Unicode il buffer non viene assegnato bene
							if (pElem->m_nDBType == DBTYPE_WSTR && m_pSqlRowSet->m_pSqlConnection->GetDBMSType() == DBMS_ORACLE)
								lByte = 0;
							((DataStr*)pDataObj)->Assign(pCurrPos, lByte, pElem->m_nDBType == DBTYPE_WSTR);
						}

						if (m_pSqlRowSet->m_pSqlConnection->GetProviderInfo()->StripSpaces())
							((DataStr*)pDataObj)->StripBlank();
					}
					break;

				case DBTYPE_IUNKNOWN: //per i DataText in SqlServer
					pElem->ManageUnicodeInDB(m_pSqlRowSet);
					pElem->ReadBlobValue(pCurrPos);
					break;

				default: 
					pDataObj->Assign(pCurrPos, lLen);	
			}
		}
		else
		{
			if (!bOnlyAutoInc)
			{
				pDataObj->Clear();			
				if (pElem->m_dwStatus ==  DBSTATUS_S_ISNULL)
					pElem->m_dwStatus = DBSTATUS_S_OK;
				else//Bug fix 13772
					if (pElem->m_dwStatus == DBSTATUS_E_UNAVAILABLE) // c'è qualcosa che non va.. Non rendo disponibile il record letto
						return FALSE; 
			}
		}
		
		pElem->AssignOldDataObj(*pDataObj);
		pCurrPos += lLen + STATUS_LEN;	
		if (pElem->m_bUseLength)
			pCurrPos += LENGTH_LEN;
	}
	m_pSqlRowSet->MakeProcTimeOperation(STOP_TIME,PROC_FIXUP_OPERATION);

	return TRUE;
}

//-----------------------------------------------------------------------------
CString SqlAccessor::GetStatusError(int nIdx)
{
	SqlBindingElem* pBindCol = GetAt(nIdx);
	
	switch (pBindCol->m_dwStatus)
	{
		case DBSTATUS_E_BADACCESSOR:	
			return cwsprintf(_TB("Error DBSTATUS_E_BADACCESSOR in column {0-%s}"), (LPCTSTR)pBindCol->m_strBindName);
		case DBSTATUS_E_CANTCONVERTVALUE:	
			return cwsprintf(_TB("Error DBSTATUS_E_CANTCONVERTVALUE in column {0-%s}"), (LPCTSTR)pBindCol->m_strBindName);
		case DBSTATUS_E_SIGNMISMATCH:	
			return cwsprintf(_TB("Error DBSTATUS_E_SIGNMISMATCH in column {0-%s}"), (LPCTSTR)pBindCol->m_strBindName);
		case DBSTATUS_E_DATAOVERFLOW:	
			return cwsprintf(_TB("Error DBSTATUS_E_DATAOVERFLOW in column {0-%s}"), (LPCTSTR)pBindCol->m_strBindName);
		case DBSTATUS_E_CANTCREATE:	
			return cwsprintf(_TB("Error DBSTATUS_E_CANTCREATE in column {0-%s}"), (LPCTSTR)pBindCol->m_strBindName);
		case DBSTATUS_E_UNAVAILABLE:	
			return cwsprintf(_TB("Error DBSTATUS_E_UNAVAILABLE in column {0-%s}"), (LPCTSTR)pBindCol->m_strBindName);
	}
	return _TB("Error reading data");
}

//-----------------------------------------------------------------------------
BOOL SqlAccessor::CheckStatus(CDiagnostic* pDiagnostic)
{
	if (!m_pBindBuffer) //@@OLEexception TODO
		return FALSE;
		
	BYTE*	 pCurrPos	= m_pBindBuffer;
	DBLENGTH lLen; 
	BOOL bOk = TRUE;

	SqlBindingElem* pElem = NULL;

	for (int nIdx = 0; nIdx < GetSize(); nIdx++)
	{
		pElem = GetAt(nIdx);
		if (!pElem)
			continue;

		lLen = GetBindLen(pElem);
		//leggo lo stato dal buffer
		memcpy(&(pElem->m_dwStatus), pCurrPos + lLen, STATUS_LEN);
		if (pElem->m_dwStatus !=  DBSTATUS_S_OK)
		{
			pDiagnostic->Add(GetStatusError(nIdx));
			bOk = FALSE;
		}
	}

	return bOk;
}

//-----------------------------------------------------------------------------
void SqlAccessor::Clear()
{
	RemoveAll();
	if (m_pBindBuffer)
	{	
		delete[] m_pBindBuffer;
		m_pBindBuffer = NULL;	
	}
	m_lBufferSize = 0;
	m_nOutNumb = 0;
	m_nUseLengthNumb = 0;
	m_bBlobAlreadyBinded = FALSE;
}

/////////////////////////////////////////////////////////////////////////////
//						SqlColumnArray declaration
/////////////////////////////////////////////////////////////////////////////
//
//-----------------------------------------------------------------------------
IMPLEMENT_DYNAMIC(SqlColumnArray, SqlAccessor)

//-----------------------------------------------------------------------------
SqlColumnArray::SqlColumnArray(SqlRowSet* pRowSet)
:
	SqlAccessor(pRowSet)
{
}		

//-----------------------------------------------------------------------------
BOOL SqlColumnArray::BindColumns()
{
	if (m_pSqlRowSet->IsNull())
		return FALSE;

	if (GetSize() == 0)
		return TRUE;

	m_lBufferSize += (STATUS_LEN * m_nOutNumb);

	m_lBufferSize += (LENGTH_LEN * m_nUseLengthNumb);

	m_pSqlRowSet->MakeDBTimeOperation(START_TIME, DB_BIND_COLUMNS);

	if (!m_pBindBuffer)
		m_pBindBuffer = new BYTE[m_lBufferSize];
	//creo l'accessor per la bind delle colonne
	m_pSqlRowSet->CreateAccessor(GetSize(), m_pBindBuffer, m_lBufferSize); 	
	
	BYTE* pCurrPos = m_pBindBuffer;
	long nLen; 
	BYTE* pStatus = NULL;
	BYTE* pLength = NULL;

	SqlBindingElem* pElem = NULL;

	// effettuo la bind di ogni singola colonna
    for (int nIdx = 0; nIdx < GetSize(); nIdx++)
	{
		pElem = GetAt(nIdx);
		if (!pElem)
			continue;

		nLen = GetBindLen(pElem);
		
		// devo considerare la lunghezza in caso di:
		// DBTYPE_IUNKNOWN in SQLSERVER (DataText)
		// DBTYPE_WSTR in ORALCE (DataStr e DataText)
		// DBTYPE_WSTR in MYSQL (DataText)
		if (pElem->m_bUseLength)
		{
			pLength = pCurrPos + nLen;
			pStatus = pLength + LENGTH_LEN;
		}
		else
		{
			pLength = NULL;
			pStatus = pCurrPos + nLen;
		}

		m_pSqlRowSet->AddBindEntry
			(		
				(DBORDINAL)nIdx + 1, 
				pElem->m_nDBType,
				nLen,  
				(void*)pCurrPos, 			
				pLength, 
				pStatus
		);

		// nel caso dei blob devo passare anche il puntatore al DBOBJECT x la gestione dello stream
		if (pElem->m_nDBType == DBTYPE_IUNKNOWN)
			m_pSqlRowSet->CreateStreamObject(nIdx);

		pCurrPos = pStatus + STATUS_LEN;	

	}
	m_pSqlRowSet->MakeDBTimeOperation(STOP_TIME, DB_BIND_COLUMNS);

	return TRUE;
}

// mi serve per il blob.
// la close del CCommand eseguita prima di eseguire la query parametrizzata, mi cancella 
// i dbobject associati alle colonne di tipo blob.. Devo di nuovo assegnarli
//-----------------------------------------------------------------------------
BOOL SqlColumnArray::RibindColumns()
{
	if (m_pSqlRowSet->IsNull())
		return FALSE;
	
	// se non ho dei blob evito di fare il ciclo
	if (m_nUseLengthNumb <= 0)
		return TRUE;

	for (int nIdx = 0; nIdx < GetSize(); nIdx++)
	{
		if (!GetAt(nIdx))
			continue;

		// riassegno il puntatore ad un DBOBJECT x la gestione dello stream
		if (GetAt(nIdx)->m_nDBType == DBTYPE_IUNKNOWN)
			m_pSqlRowSet->CreateStreamObject(nIdx);
		
	}	
	return TRUE;
}

// per i campi di tipo incrementale, in fase di inserimento di una nuova riga, devo
// leggermi immediatamente dopo il valore assegnatomi dal DBMS
//-----------------------------------------------------------------------------
BOOL SqlColumnArray::FixupAutoIncColumns()
{
	return FixupBindElements(TRUE);
}

//-----------------------------------------------------------------------------
BOOL SqlColumnArray::FixupColumns()
{
	return FixupBindElements();
}

//-----------------------------------------------------------------------------
int	SqlColumnArray::Add(const CString& strName, DataObj* pDataObj, const DBTYPE& eDBType, const int& nSqlRecIdx, BOOL bAutoIncrement /*=FALSE*/, int nInsertPos /*= -1*/)
{
	return SqlAccessor::Add(strName, pDataObj, eDBType, DBPARAMIO_NOTPARAM, nSqlRecIdx, bAutoIncrement, _T(""), nInsertPos);
}

//-----------------------------------------------------------------------------
int SqlColumnArray::Add(SqlRecord* pRecord, int nIdx, int nInsertPos /*= -1*/)
{
	ASSERT(pRecord);
	if (!pRecord)
		return -1;

	SqlRecordItem* pRecItem			= pRecord->GetAt(nIdx);
	CString sQualifiedColumnName	= pRecord->GetQualifiedColumnName(pRecItem->m_pColumnInfo); 
	DataObj* pDataObj				= pRecItem->GetDataObj();

	if (pRecItem->m_pColumnInfo->m_bNativeColumnExpr && m_pSqlRowSet && m_pSqlRowSet->IsKindOf(RUNTIME_CLASS(SqlTable)))
	{
		//expand sQualifiedColumnName
		SymTable* pSymTable = ((SqlTable*)m_pSqlRowSet)->m_pSymTable;
		if (pSymTable)
		{
			ExpandContentOfClause(sQualifiedColumnName, pSymTable, pRecord->GetConnection());
		}
	}

	return SqlAccessor::Add
					(
						sQualifiedColumnName,
						pDataObj,
						pRecord->GetConnection()->GetSqlDataType(pDataObj->GetDataType()),	
						DBPARAMIO_NOTPARAM,
						nIdx, 
						pRecItem->m_pColumnInfo->m_bAutoIncrement,
						 _T(""), 
						 nInsertPos
					);
}

//-----------------------------------------------------------------------------
int SqlColumnArray::Add(SqlRecord* pRecord, DataObj* pDataObj, int nInsertPos /*= -1*/)
{
	if (!pRecord || !pDataObj)
		return -1;

	int nIdx = pRecord->Lookup(pDataObj);
	if (nIdx < 0) return -1;

	
	return SqlAccessor::Add
					(
						pRecord->GetQualifiedColumnName(nIdx), 
						pDataObj,
						pRecord->GetConnection()->GetSqlDataType(pDataObj->GetDataType()),
						DBPARAMIO_NOTPARAM,
						nIdx,
						pRecord->IsAutoIncrement(nIdx),
						_T(""), 
						nInsertPos
					);	
}


/////////////////////////////////////////////////////////////////////////////
//						SqlParamArray declaration
/////////////////////////////////////////////////////////////////////////////
//
//-----------------------------------------------------------------------------
IMPLEMENT_DYNAMIC(SqlParamArray, SqlAccessor)

//-----------------------------------------------------------------------------
SqlParamArray::SqlParamArray(SqlRowSet* pRowSet)
:
	SqlAccessor		(pRowSet),
	m_pParamInfo	(NULL),
	m_pOrdinals		(NULL)
{
}

//-----------------------------------------------------------------------------
SqlParamArray::~SqlParamArray()
{
	SAFE_DELETE(m_pParamInfo);
	SAFE_DELETE(m_pOrdinals);	
}

//-----------------------------------------------------------------------------
int SqlParamArray::GetParamPosition(const CString& strParamName) const
{
	for (int i = 0; i <= GetUpperBound(); i++)
		if (GetAt(i) && GetAt(i)->m_strBindName.CompareNoCase(strParamName) == 0)
			return i;
			
	return -1;
}

//-----------------------------------------------------------------------------
void SqlParamArray::SetParamValue(const CString& strParamName, const DataObj& aDataObj)
{
	int nPos = GetParamPosition(strParamName);
	if (nPos != -1)
	{
		GetAt(nPos)->SetParamValue(aDataObj);
		return;
	}
			
	ASSERT(FALSE);
}

//-----------------------------------------------------------------------------
void SqlParamArray::GetParamValue(const CString& strParamName, DataObj* pDataObj) const
{
	int nPos = GetParamPosition(strParamName);
	if (nPos != -1)
	{
		GetAt(nPos)->GetParamValue(pDataObj);
		return;
	}
			
	ASSERT(FALSE);
}

//-----------------------------------------------------------------------------
void SqlParamArray::GetParamValue(int nIdx, DataObj* pDataObj) const
{
	ASSERT(nIdx >= 0 && nIdx <= GetUpperBound());
	GetAt(nIdx)->GetParamValue(pDataObj);
}


//-----------------------------------------------------------------------------
DataType SqlParamArray::GetParamDataType(const CString& strParamName) const
{
	int nPos = GetParamPosition(strParamName);
	if (nPos != -1)
		return GetAt(nPos)->GetDataType();

	ASSERT(FALSE);
	return DATA_NULL_TYPE;
}

//-----------------------------------------------------------------------------
 DataType SqlParamArray::GetParamDataType(int i) const
{
	ASSERT(i >= 0 && i <= GetUpperBound());
	return GetAt(i)->GetDataType();
}

//-----------------------------------------------------------------------------
LPTSTR GetDescriType(DBTYPE eDBType)
{
	switch (eDBType)
	{
		case DBTYPE_STR: return _T("DBTYPE_VARCHAR"); 
		case DBTYPE_WSTR: return _T("DBTYPE_WVARCHAR"); 
		case DBTYPE_I4:return _T("DBTYPE_I4"); 
		case DBTYPE_I2:return _T("DBTYPE_I2"); 		
		case DBTYPE_R8:return _T("DBTYPE_R8"); 
		case DBTYPE_DBTIMESTAMP:return _T("DBTYPE_DBTIMESTAMP");
		case DBTYPE_GUID:return _T("DBTYPE_GUID");
		case DBTYPE_IUNKNOWN:return _T("DBTYPE_IUNKNOWN");	
		default: return _T("DBTYPE_STR");
	}
}	

//-----------------------------------------------------------------------------
HRESULT SqlParamArray::SetParameterInfo()
{
	if (GetSize() == 0)
		return S_OK;

	if (m_pSqlRowSet->IsNull())
		return E_FAIL;

	long ulParams = GetSize();
	
	if (!m_pParamInfo)
		m_pParamInfo = new DBPARAMBINDINFO[ulParams];

	if (!m_pOrdinals)
		m_pOrdinals = new ULONG[ulParams];

	DBPARAMIO eType;

	SqlBindingElem* pElem = NULL;
	for (int nIdx = 0; nIdx <= GetUpperBound(); nIdx++)
	{
        pElem = GetAt(nIdx);
        if (!pElem)
			continue;

		eType = pElem->m_eParamIO;
	
		DWORD dwFlags = 0;
		if (eType == DBPARAMTYPE_INPUT ||
			eType == DBPARAMTYPE_INPUTOUTPUT)
			dwFlags |= DBPARAMFLAGS_ISINPUT;

		if (eType == DBPARAMTYPE_OUTPUT ||
			eType == DBPARAMTYPE_INPUTOUTPUT ||
			eType == DBPARAMTYPE_RETURNVALUE)
			dwFlags |= DBPARAMFLAGS_ISOUTPUT;
		

		m_pParamInfo[nIdx].pwszDataSourceType = T2OLE(GetDescriType(pElem->m_nDBType));
		m_pParamInfo[nIdx].bPrecision	= 0;
		m_pParamInfo[nIdx].bScale		= 0;
		m_pParamInfo[nIdx].dwFlags		= dwFlags;
		m_pParamInfo[nIdx].pwszName		= NULL;
		m_pParamInfo[nIdx].ulParamSize	= GetBindLen(pElem);

		m_pOrdinals[nIdx] = nIdx+1;
	}

	HRESULT hr = m_pSqlRowSet->SetParameterInfo(GetSize(), m_pOrdinals, m_pParamInfo);
	
	return (hr = S_OK || hr == DB_S_TYPEINFOOVERRIDDEN)
			? S_OK
			: hr; 
}


//-----------------------------------------------------------------------------
BOOL SqlParamArray::BindParameters()
{
	if (GetSize() == 0)
		return TRUE;
	
	if (m_pSqlRowSet->IsNull())
		return FALSE;

	m_lBufferSize += (STATUS_LEN * m_nOutNumb);
	m_lBufferSize += (LENGTH_LEN * m_nUseLengthNumb);
	
	if (!m_pBindBuffer)
		m_pBindBuffer  = new BYTE[m_lBufferSize];
	
	memset(m_pBindBuffer, 0, m_lBufferSize);
		
	DataObj* pDataObj = NULL;
	DBLENGTH nLen;
	//riempo la SqlParamArray con nome parametro e dataobj
	m_pSqlRowSet->CreateParameterAccessor( GetSize(), m_pBindBuffer, m_lBufferSize);
	
	BYTE* pCurrPos = m_pBindBuffer;
	BYTE* pStatus = NULL;
	BYTE* pLength = NULL;

	BOOL bUseLength = FALSE;
	
	SqlBindingElem* pElem = NULL;
	
	//gestione performance
	m_pSqlRowSet->MakeDBTimeOperation(START_TIME, DB_BIND_PARAMETERS);
	for (int nIdx = 0; nIdx <= GetUpperBound(); nIdx++)
	{
        pElem = GetAt(nIdx);
        if (!pElem)
			continue;

		pDataObj = pElem->m_pDataObj;
		nLen	 = GetBindLen(pElem);
	
		pLength = (pElem->m_bUseLength)
				   ? pCurrPos + nLen
				   : NULL;

		// nel caso del parametro di output devo avere anche lo stato
		pStatus = ((pElem->m_eParamIO & DBPARAMIO_OUTPUT) == DBPARAMIO_OUTPUT) 
				  ? pCurrPos + nLen
				  : NULL;

		if (pStatus && pElem->m_bUseLength)
			pStatus += LENGTH_LEN;

		m_pSqlRowSet->AddParameterEntry(
										(DBORDINAL)nIdx + 1,
										pElem->m_nDBType,
										nLen,  
										(void*)pCurrPos,
										pLength, 
										pStatus,
										pElem->m_eParamIO
									);

		pCurrPos += nLen;

		if ((pElem->m_eParamIO & DBPARAMIO_OUTPUT) == DBPARAMIO_OUTPUT) 
			pCurrPos += STATUS_LEN;
		
		// nel caso dei blob devo passare anche il puntatore al DBOBJECT x la gestione dello stream
		// e considerare anche il campo length
		if (pElem->m_nDBType == DBTYPE_IUNKNOWN)
			m_pSqlRowSet->CreateParamStreamObject(nIdx);

		if (pElem->m_bUseLength)
			pCurrPos += LENGTH_LEN;
	}
	m_pSqlRowSet->MakeDBTimeOperation(STOP_TIME, DB_BIND_PARAMETERS);

	//return SetParameterInfo() == S_OK && FixupParameters();
	//for SQLServer and PRB: E_FAIL Returned from Prepare() When SQL Statement Contains a Parameter in a Subquery 
	// (MSDN: Q235053) I postpone the SetParameterInfo call in SqlTable::Prepare method
	return FixupParameters();
}

//-----------------------------------------------------------------------------
BOOL SqlParamArray::FixupParameters()
{
	return FixupBuffer(TRUE); 
}

//-----------------------------------------------------------------------------
BOOL SqlParamArray::FixupOutParams()
{
	return FixupBindElements();
}


//classe di ausilio per effettuare il binding di un blob
//////////////////////////////////////////////////////////////////////////////
//					CBindingBlob declaration
//////////////////////////////////////////////////////////////////////////////
//
CBindingBlob::CBindingBlob(DataObj* pDataObj, const CString& strColumnName)
:
	m_pISSHelper		(NULL),
	m_pBuffer			(NULL),
	m_pUnk				(NULL),
	m_pDataObj			(pDataObj), // rappresenta il dataobj da cui leggete o su cui scrivere le informazioni di scambio con il db
	m_strColumnName		(strColumnName),
	m_ulBytesRead		(0)
	//,m_iReadPos		(0)    
	//,m_iWritePos		(0)    
{
	//m_pUnk = new IUnknown*;
	m_pISSHelper = new CISSHelper;
}

//---------------------------------------------------------------------------------------
CBindingBlob::~CBindingBlob()
{
	if (m_pISSHelper)
		delete m_pISSHelper;

	if (m_pBuffer)
		delete m_pBuffer;
}

//-----------------------------------------------------------------------------
void CBindingBlob::SetUnicodeInDB(const BOOL& bValue) 
{ 
	m_pISSHelper->m_bInUnicode = bValue; 
}

//---------------------------------------------------------------------------------------
void CBindingBlob::Clear() 
{
	m_ulBytesRead = 0;
	//m_iWritePos	= 0;
	if (m_pBuffer)
		memset(m_pBuffer, 0, BUFFER_SIZE + (m_pISSHelper->m_bInUnicode ? sizeof(TCHAR) : sizeof(char))); //pulisco il contenuto del dato

	m_pISSHelper->Clear();
}

//-----------------------------------------------------------------------------
void CBindingBlob::Read(BYTE* pBuffer)
{
	if (!m_pBuffer)
		// mi tengo un carattere per il terminatore
		m_pBuffer = new BYTE[BUFFER_SIZE + (m_pISSHelper->m_bInUnicode ? sizeof(TCHAR) : sizeof(char))];
			
	Clear();
	
	ISequentialStream* pISequentialStream = (*(ISequentialStream**)pBuffer);

	do
	{
		pISequentialStream->Read(m_pBuffer, BUFFER_SIZE, &m_ulBytesRead);
		//quando copio l'ultimo stream setto anche le properties del dataobj: vedi ultimo parametro a TRUE
		if (m_ulBytesRead > 0)
			m_pDataObj->AppendFromSStream(m_pBuffer, (BUFFER_SIZE > m_ulBytesRead) ? m_ulBytesRead : BUFFER_SIZE, m_pISSHelper->m_bInUnicode, (BUFFER_SIZE > m_ulBytesRead));

		memset(m_pBuffer, 0, BUFFER_SIZE); //pulisco il contenuto del dato
	}
	while (BUFFER_SIZE == m_ulBytesRead);	
}

//-----------------------------------------------------------------------------
long CBindingBlob::Write(BYTE* pBuffer)
{
	ISequentialStream* pISequentialStream = NULL;
	Clear();

	IUnknown ** pUnk = (IUnknown**)pBuffer;
	if (*pUnk)
		(*pUnk)->Release();	

	void* p = m_pDataObj->GetOleDBDataPtr();
	if (!m_pISSHelper->m_bInUnicode)
	{
		USES_CONVERSION;
		p = T2A((LPCTSTR)p);
	}

	int l = m_pISSHelper->m_bInUnicode ? (((DataText*)m_pDataObj)->GetLen() * sizeof(TCHAR)) : (((DataText*)m_pDataObj)->GetLen() * sizeof(char));
	ULONG lWrite = 0;
	m_pISSHelper->Write(p, l, &lWrite);
	m_pISSHelper->QueryInterface(IID_ISequentialStream,(void**) &pISequentialStream);

	// Assign the data as ISequentialStream in the BLOB column
	//pISequentialStream = (ISequentialStream*) m_pISSHelper; 
	memcpy(pBuffer, &pISequentialStream, sizeof(IUnknown*));
	
	return lWrite;
}

//-----------------------------------------------------------------------------
BOOL CBindingBlob::IsUnicodeInDB () const
{
	return m_pISSHelper->m_bInUnicode;
}

//classe di ausilio per la gestione degli stream
//////////////////////////////////////////////////////////////////////////////
//					CISSHelper declaration
//////////////////////////////////////////////////////////////////////////////
//
CISSHelper::CISSHelper()
{
	m_cRef		= 0;
	m_pBuffer	= NULL;
	m_ulLength	= 0;
	m_iReadPos	= 0;
	m_iWritePos	= 0;
	m_bInUnicode= FALSE;
}

//-----------------------------------------------------------------------------
CISSHelper::~CISSHelper()
{
	Clear();
}

//-----------------------------------------------------------------------------
void CISSHelper::Clear() 
{
	CoTaskMemFree( m_pBuffer );
	m_pBuffer	= NULL;
	m_ulLength	= 0;
	m_iReadPos	= 0;
	m_iWritePos	= 0;
	m_cRef		= 0;
}


//-----------------------------------------------------------------------------
ULONG CISSHelper::AddRef(void)
{
	return ++m_cRef;
}

//-----------------------------------------------------------------------------
ULONG CISSHelper::Release(void)
{
	return --m_cRef;
}

//-----------------------------------------------------------------------------
HRESULT CISSHelper::QueryInterface( REFIID riid, void** ppv )
{
	*ppv = NULL;
	if ( riid == IID_IUnknown )			 *ppv = this;
	if ( riid == IID_ISequentialStream ) *ppv = this;
	if ( *ppv )
	{
		( (IUnknown*) *ppv )->AddRef();
		return S_OK;
	}
	return E_NOINTERFACE;
}

//-----------------------------------------------------------------------------
HRESULT CISSHelper::Read( void *pv,	ULONG cb, ULONG* pcbRead )
{
	// Check parameters.
	if ( pcbRead ) *pcbRead = 0;
	if ( !pv ) return STG_E_INVALIDPOINTER;
	if ( 0 == cb ) return S_OK; 

	// Calculate bytes left and bytes to read.
	ULONG cBytesLeft = m_ulLength - m_iReadPos;
	ULONG cBytesRead = cb > cBytesLeft ? cBytesLeft : cb;

	// If no more bytes to retrive return S_FALSE.
	if ( 0 == cBytesLeft ) return S_FALSE;

	// Copy to users buffer the number of bytes requested or remaining
	memcpy( pv, (void*)((BYTE*)m_pBuffer + m_iReadPos), cBytesRead );
	m_iReadPos += cBytesRead;

	// Return bytes read to caller.
	if ( pcbRead ) *pcbRead = cBytesRead;
	if ( cb != cBytesRead ) return S_FALSE; 

	return S_OK;
}
        
//-----------------------------------------------------------------------------
HRESULT CISSHelper::Write( const void *pv, ULONG cb, ULONG* pcbWritten )
{
	// Check parameters.
	if ( !pv ) return STG_E_INVALIDPOINTER;
	if ( pcbWritten ) *pcbWritten = 0;
	if ( 0 == cb ) return S_OK;

	// Enlarge the current buffer.
	m_ulLength += cb;
    
	// Grow internal buffer to new size.
	if (m_bInUnicode)
		m_pBuffer = CoTaskMemRealloc( m_pBuffer, m_ulLength + sizeof(TCHAR));
	else
		m_pBuffer = CoTaskMemRealloc( m_pBuffer, m_ulLength + sizeof(char));

	// Check for out of memory situation.
	if ( NULL == m_pBuffer ) 
	{
		Clear();
		return E_OUTOFMEMORY;
	}

	// Copy callers memory to internal buffer and update write position.
	memcpy( (void*)((BYTE*)m_pBuffer + m_iWritePos), pv, cb );
	m_iWritePos += cb;	
	if (m_bInUnicode) 
		((TCHAR*)m_pBuffer)[cb/sizeof(TCHAR)] = _T('\0');
	else
		((char*) m_pBuffer)[cb] = '\0';
	
	
	// Return bytes written to caller.
	if ( pcbWritten ) *pcbWritten = cb;

	return S_OK;
}
