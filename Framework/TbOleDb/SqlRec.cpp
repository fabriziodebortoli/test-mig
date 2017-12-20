
#include "stdafx.h"

#include <afxtempl.h>

#include <TbNameSolver\Chars.h>
#include <TbNameSolver\TBNamespaces.h>
#include <TbNameSolver\LoginContext.h>

#include <TbGeneric\globals.h>
#include <TbGeneric\crypt.h>
#include <TbGeneric\LocalizableObjs.h>
#include <TbGenlib\baseapp.h>

#include "sqlrec.h"
#include "sqltable.h"
#include "sqlcatalog.h" 
#include "oledbmng.h"
#include "lentbl.h"

//includere come ultimo include all'inizio del cpp
#include "begincpp.dex"

#ifdef _DEBUG
#undef THIS_FILE
static const char THIS_FILE[] = __FILE__;
#endif

#ifdef _DEBUG
#define new DEBUG_NEW
#endif

#define LEN_TRACE_KEY			128

//-----------------------------------------------------------------------------
/////////////////////////////////////////////////////////////////////////////
//								SqlBindItem
//////////////////////////////////////////////////////////////////////////////
//
//-----------------------------------------------------------------------------
IMPLEMENT_DYNAMIC(SqlBindItem, CObject)

//-----------------------------------------------------------------------------
SqlBindItem::SqlBindItem (DataObj* pDataObj, const CString& strColumnName)
	:
	m_pDataObj			(pDataObj),
	m_strColumnName		(strColumnName),
	m_bDynamicallyBound	(FALSE)
{
	ASSERT_VALID(m_pDataObj);
}

//-----------------------------------------------------------------------------
SqlBindItem::SqlBindItem (const SqlBindItem& bi)
	:
	m_pDataObj			(bi.m_pDataObj ? bi.m_pDataObj->Clone() : NULL),
	m_strColumnName		(bi.m_strColumnName),
	m_bDynamicallyBound	(bi.m_bDynamicallyBound),
	m_strContextElementName(bi.m_strContextElementName)
{
	ASSERT_VALID(m_pDataObj);
}

//-----------------------------------------------------------------------------
SqlBindItem::~SqlBindItem()
{
	if (m_bDynamicallyBound)
		delete m_pDataObj;
}

//-----------------------------------------------------------------------------
BOOL SqlBindItem::IsEqual(const SqlBindItem& item) const
{   
	ASSERT_VALID(m_pDataObj);
	return 
		!_tcsicmp(m_strColumnName, item.m_strColumnName) &&
    	m_pDataObj->IsEqual(*(item.m_pDataObj)) &&
		m_bDynamicallyBound == item.m_bDynamicallyBound &&
		m_strContextElementName == item.m_strContextElementName;
}
//-----------------------------------------------------------------------------
void SqlBindItem::Assign(const SqlBindItem& item)
{
	ASSERT(DataType::IsCompatible(item.m_pDataObj->GetDataType(), m_pDataObj->GetDataType()));
    m_pDataObj->Assign(*(item.m_pDataObj));

	m_strColumnName = item.m_strColumnName;
	m_bDynamicallyBound = item.m_bDynamicallyBound;
	m_strContextElementName = item.m_strContextElementName;
}

//-----------------------------------------------------------------------------
void SqlBindItem::operator = (const SqlBindItem& item)
{
	Assign(item);
}


//////////////////////////////////////////////////////////////////////////////
//							SqlRecordItem Implementation
//////////////////////////////////////////////////////////////////////////////
//
//-----------------------------------------------------------------------------
IMPLEMENT_DYNAMIC(SqlRecordItem, SqlBindItem)

//-----------------------------------------------------------------------------
SqlRecordItem::SqlRecordItem 
	(
		DataObj*				pDataObj, 
		const CString&			strColumnName, 
		const SqlColumnInfo*	pColumnInfo /*= NULL*/,
		BOOL					bDynamicallyBound /*= FALSE*/
	)
	:
	SqlBindItem						(pDataObj, strColumnName),
	m_pColumnInfo					(NULL),
	m_lLength						(0),
	m_bOwnColumnInfo				(FALSE)
{
	m_bDynamicallyBound = bDynamicallyBound;

	if (pColumnInfo)
		SetColumnInfo(pColumnInfo);
}

//-----------------------------------------------------------------------------
SqlRecordItem::SqlRecordItem (const SqlRecordItem& ri)
	:
	SqlBindItem			(ri),
	m_pColumnInfo		(ri.m_pColumnInfo),	
	m_lLength			(ri.m_lLength),
	m_bOwnColumnInfo	(FALSE)
{
}

//-----------------------------------------------------------------------------
SqlRecordItem::~SqlRecordItem ()
{
	if (m_bOwnColumnInfo)
	{
		ASSERT_VALID(m_pColumnInfo);
		SAFE_DELETE(m_pColumnInfo);
	}
}

//-----------------------------------------------------------------------------
void SqlRecordItem::SetColumnInfo(const SqlColumnInfo* pColumnInfo)
{
	ASSERT_VALID(pColumnInfo);
	m_pColumnInfo = pColumnInfo;

	const_cast<SqlColumnInfo*>(pColumnInfo)->UpdateDataObjType(m_pDataObj);
	pColumnInfo->SetDataObjInfo(m_pDataObj);
}
//-----------------------------------------------------------------------------
BOOL SqlRecordItem::IsMandatory() const
{
	CString sName = GetColumnName();
	return sName == GUID_COL_NAME || sName == CREATED_COL_NAME || sName == MODIFIED_COL_NAME || sName == CREATED_ID_COL_NAME || sName == MODIFIED_ID_COL_NAME;
}
//-----------------------------------------------------------
BOOL SqlRecordItem::IsEqual(const SqlRecordItem& item) const
{   
	if (!SqlBindItem::IsEqual(*(SqlBindItem*)&item))
		return FALSE;

	if (this->m_strColumnName.CompareNoCase(item.m_strColumnName))
		return FALSE;

	if (!m_pColumnInfo && !item.m_pColumnInfo)
		return TRUE;

	if (!m_pColumnInfo && item.m_pColumnInfo)
		return FALSE;

	return m_pColumnInfo->IsEqual(*item.m_pColumnInfo);
}

//-----------------------------------------------------------------------------
void SqlRecordItem::UpdateCollateCultureStatus ()
{
	if (m_pDataObj && m_pColumnInfo)
		m_pDataObj->SetCollateCultureSensitive (m_pColumnInfo->m_bUseCollationCulture == TRUE);
}

//-----------------------------------------------------------------------------
void SqlRecordItem::Parse(CXMLNode* pNode, BOOL bWithAttributes /*TRUE*/)
{   
	CString strValue;
	if (bWithAttributes)
	{
		if (pNode->GetAttribute(m_strColumnName, strValue))
			m_pDataObj->AssignFromXMLString(strValue);
	}
	else
	{
		if (pNode->GetChildByName(m_strColumnName)->GetText(strValue))
			m_pDataObj->AssignFromXMLString(strValue);
	}
}

//-----------------------------------------------------------------------------
void SqlRecordItem::UnParse(CXMLNode* pNode, BOOL bWithAttributes /*TRUE*/, BOOL bSoapType /*= TRUE*/)
{   
	// WARNING: when exporting records in XML, the enums and the dates must always be in "soap type" mode (numeric instead of string for enums, full UTC for dates)
	BOOL bDataObjInSOAPType = (m_pDataObj->GetDataType() == DATA_ENUM_TYPE || m_pDataObj->GetDataType() == DATA_DATE_TYPE) ? TRUE : bSoapType;
	if (bWithAttributes)
		pNode->SetAttribute(m_strColumnName, m_pDataObj->FormatDataForXML(bDataObjInSOAPType));
	else
	{
		CXMLNode* pNodeChild = pNode->CreateNewChild(m_strColumnName);
		pNodeChild->SetText(m_pDataObj->FormatDataForXML(bDataObjInSOAPType));
		pNode->AppendChild(pNodeChild);
	}
}

//-----------------------------------------------------------------------------
void SqlRecordItem::operator = (const SqlRecordItem& item)
{
	Assign(*(SqlBindItem*)&item);
	//NO crea aliasing e crash in delete del record m_pColumnInfo = item.m_pColumnInfo;
}

//-----------------------------------------------------------------------------
CString SqlRecordItem::GetColumnTitle (const CString& sTable) const
{
	return m_pColumnInfo ? m_pColumnInfo->GetColumnTitle() : AfxLoadDatabaseString(m_strColumnName, sTable);
}

//////////////////////////////////////////////////////////////////////////////
//							SqlProcParamItem Implementation
//////////////////////////////////////////////////////////////////////////////
//
//-----------------------------------------------------------------------------
IMPLEMENT_DYNAMIC(SqlProcParamItem, SqlBindItem)

//-----------------------------------------------------------------------------
SqlProcParamItem::SqlProcParamItem (DataObj* pDataObj, const CString& strColumnName, SqlProcedureParamInfo* pParamInfo /*=NULL*/)
	:
	SqlBindItem	(pDataObj, strColumnName)
{
	if (pParamInfo)
		SetParameterInfo(pParamInfo);
}

//-----------------------------------------------------------------------------
void SqlProcParamItem::SetParameterInfo(SqlProcedureParamInfo* pParamInfo)
{
	ASSERT(pParamInfo);
	m_pParameterInfo = pParamInfo;
	m_pParameterInfo->UpdateDataObjInfo(m_pDataObj);	
}

//-----------------------------------------------------------------------------
BOOL SqlProcParamItem::IsEqual(const SqlProcParamItem& item) const
{   
	return
			(
				SqlBindItem::IsEqual(*(SqlBindItem*)&item) &&
				m_pParameterInfo == item.m_pParameterInfo 
			);
}

//-----------------------------------------------------------------------------
void SqlProcParamItem::operator = (const SqlProcParamItem& item)
{
	Assign(*(SqlBindItem*)&item);
	m_pParameterInfo = item.m_pParameterInfo;
}

//////////////////////////////////////////////////////////////////////////////
//					SqlRecord Implementation
//////////////////////////////////////////////////////////////////////////////
//
//-----------------------------------------------------------------------------
//[TBWebMethod(name = GetExtraFiltering )]
///<summary>
/// Chiama il metodo per il ritorno dei frammenti di Where da aggiungere per la RowSecurity
///</summary>
DataStr GetExtraFiltering (DataArray /*[string]*/ aTablesNames, DataStr aWC)
{
	CString strExtraWC = aWC.GetString();
	for (int i = 0; i < aTablesNames.GetSize(); i++)
	{
		const SqlCatalogEntry* pCatalogEntry = AfxGetDefaultSqlConnection()->GetCatalogEntry(((DataStr*)aTablesNames.GetAt(i))->GetString());
		ASSERT_VALID(pCatalogEntry);
		if (!pCatalogEntry) 
			continue;

		SqlRecord* pRec = pCatalogEntry->CreateRecord();
		pRec->EnableExtraFiltering(TRUE);
		pRec->OnExtraFiltering(strExtraWC);
		delete pRec;
	}

	return strExtraWC;
}

//-----------------------------------------------------------------------------
IMPLEMENT_DYNCREATE(SqlRecord, ISqlRecord)

//-----------------------------------------------------------------------------
//	In Costruttore senza parametri deve essere implementato nella classe derivata
//	utilizzando quello con nome e usando il nome definito nel .cpp della classe
//	derivata
SqlRecord::SqlRecord()
	:
	m_nType				(TABLE_TYPE),
	m_arExtensions		(NULL),
	m_bCreatedAsEmpty	(false),
	m_nLocalsCount		(0),
	m_nDummyCount		(0)
{
	Initialize();
}

//-----------------------------------------------------------------------------
// il sqlrecord lo costruisco inizialmente sulla connessione di default
// se il programmatore vuole utilizzare un'altra connessione deve chiamare il
// metodo SetConnection passando il puntatore all'istanza di SqlConnect da utilizzare
SqlRecord::SqlRecord(LPCTSTR szTableName, SqlConnection* pConn /*= NULL*/, short nType /*= TABLE_TYPE*/, bool bCreatedAsEmpty /*= false*/)
	:
	m_nType				(nType),
	m_strTableName		(szTableName),
	m_arExtensions		(NULL),
	m_bCreatedAsEmpty	(bCreatedAsEmpty),
	m_nLocalsCount		(0),
	m_nDummyCount		(0)
{
	Initialize();
	SetConnection(pConn ? pConn : AfxGetDefaultSqlConnection());	
}

//-----------------------------------------------------------------------------
// Attenzione :
//		Non deve essere chiamato a causa della necessita' di chiamare la routine
//		BindRecord che e' virtuale dello specifico SqlRecord derivato. Se serve
//		bisogna implementare il Costruttore in copia nella classe finale
//	TODO: in presenza di TableInfoSorted forse si può implementare clonando completamente il master record della TableInfo
SqlRecord::SqlRecord(const SqlRecord& pRec)
	:
	m_nType				(TABLE_TYPE),
	m_arExtensions		(NULL),
	m_bCreatedAsEmpty	(false),
	m_nLocalsCount		(0),
	m_nDummyCount		(0)
{
	ASSERT(GetRuntimeClass() == pRec.GetRuntimeClass());

	Initialize();

	ASSERT(FALSE);
	ThrowSqlException(_TB("Invalid SqlRecord Builder."));
}

//-----------------------------------------------------------------------------
SqlRecord::~SqlRecord()
{
	ASSERT_VALID(this);

	m_Handler.FireDisposing(this);

	SAFE_DELETE(m_pSqlNewFieldsTable); 
	SAFE_DELETE(m_arExtensions);
}

//-----------------------------------------------------------------------------
void SqlRecord::RemoveExtension (SqlRecord* pRec)
{
	if (m_arExtensions)
	{
		for (int i = 0; i < m_arExtensions->GetSize(); i++)
		{
			if (m_arExtensions->GetAt(i) == pRec)
			{
				m_arExtensions->RemoveAt(i);
				return;
			}
		}
	}
	ASSERT(FALSE);
}

//-----------------------------------------------------------------------------
void SqlRecord::AddExtension (SqlRecord* pRec)
{
	if (!m_arExtensions)
	{
		m_arExtensions = new RecordArray();
		m_arExtensions->SetOwns(FALSE);
	}

	m_arExtensions->Add(pRec);
}

//-----------------------------------------------------------------------------
const RecordArray* SqlRecord::GetExtensions	() const
{
	return m_arExtensions;
}

//-----------------------------------------------------------------------------
const SqlRecord* SqlRecord::GetExtension (CRuntimeClass* prtc) const
{
	if (m_arExtensions)
	for (int i = 0; i < m_arExtensions->GetSize(); i++)
	{
		if (m_arExtensions->GetAt(i)->GetRuntimeClass() == prtc)
			return m_arExtensions->GetAt(i);
	}
	return NULL;
}

//-----------------------------------------------------------------------------		
int SqlRecord::GetIndexFromColumnName(const CString& strColumnName) const
{
	CString sName (strColumnName);

	int nDotCol = strColumnName.Find(L'.');
	int nDot = GetSize() > 0 ? GetAt(0)->GetColumnName().Find(L'.') : -1;

	if (nDot > -1 && nDotCol < 0)
	{
		sName = this->GetTableName() + L'.' + strColumnName;
	}
	BOOL bQualified = nDot < 0 && nDotCol > -1;

	int sz = GetSizeEx();
	for (int i = 0; i < sz; i++)
	{
		if (bQualified)
		{
			if (_tcsicmp(sName, GetAt(i)->GetQualifiedColumnName()) == 0)
				return i;
		}
		else
		{
			if (_tcsicmp(sName, GetAt(i)->GetColumnName()) == 0)
				return i;
		}
	}
	return -1;
}

//-----------------------------------------------------------------------------
//[TBWebMethod(thiscall_method=true)]
SqlRecord* SqlRecord::Clone ()
{
	SqlRecord* pRec = Create();
	*pRec = *this;
	return pRec;
}

//-----------------------------------------------------------------------------
void SqlRecord::OnCreating(SqlRecord* pRec) const
{
	if (pRec->m_strTableName.IsEmpty())
		pRec->m_strTableName = m_strTableName;
}

//-----------------------------------------------------------------------------
void SqlRecord::OnCreated(SqlRecord* pRec) const
{
}
	
//-----------------------------------------------------------------------------
SqlRecord* SqlRecord::Create () const
{
	ASSERT_VALID(m_pTableInfo);
	ASSERT_VALID(m_pTableInfo->GetSqlCatalogEntry());

	// devo assegnare subito il table name per poter ottenere una 
	// SetConnection che agganci il puntatore al m_pTableInfo corretto
	SqlRecord* pRec = (SqlRecord*) GetRuntimeClass()->CreateObject();
	ASSERT_VALID(pRec);

	OnCreating(pRec) ;
	
	pRec->SetConnection(GetConnection());

	if (m_arExtensions)
	{
		pRec->m_arExtensions = new RecordArray();

		for (int j = 0; j < m_arExtensions->GetSize(); j ++)
		{
			SqlRecord* pSubRec = m_arExtensions->GetAt(j)->Create();
			pRec->m_arExtensions->Add(pSubRec);
		}
	}

	OnCreated(pRec);
	return pRec;
}

//-----------------------------------------------------------------------------
SqlRecord& SqlRecord::operator= (const SqlRecord& record)
{
	ASSERT_VALID(this);
	ASSERT_VALID(&record);

    ASSERT(IsKindOf(record.GetRuntimeClass()));
 
	if (m_strTableName.IsEmpty())
		m_strTableName = record.m_strTableName;
	else
		ASSERT(!_tcsicmp(m_strTableName, record.m_strTableName));
 
	if (!IsValid())
		SetConnection(record.GetConnection());

	// copia anche lo stato (fare molta attenzione)
	m_wRecStatus		= record.m_wRecStatus;
	m_pSqlConnection	= record.m_pSqlConnection;
	m_bAutoIncrement	= record.m_bAutoIncrement;
	m_nType				= record.m_nType;
	
	int sz = min (GetSize(), record.GetSize());
    for (int i = 0; i < sz; i++)
		*GetAt(i) = *(record.GetAt(i));	

	if (m_arExtensions && record.m_arExtensions)
	{
		int nex = min (m_arExtensions->GetSize(), record.m_arExtensions->GetSize());
		for (int r = 0; r < nex; r++)
		{
			SqlRecord* pToRec = m_arExtensions->GetAt(r);
			ASSERT_VALID(pToRec);
			SqlRecord* pFromRec = record.m_arExtensions->GetAt(r);
			ASSERT_VALID(pFromRec);

			if (pToRec->GetRuntimeClass() != pFromRec->GetRuntimeClass())
				break;

			sz = min (pToRec->GetSize(), pFromRec->GetSize());
			for (int i = 0; i < sz; i++)
				*(pToRec->GetAt(i)) = *(pFromRec->GetAt(i));	
		}
	}

	return *this;
}

//-----------------------------------------------------------------------------
//[TBWebMethod(name = SqlRecord_Copy, thiscall_method=true)]
void SqlRecord::CopyRecord(SqlRecord* pRecSource)
{
	ASSERT_VALID(this);
	ASSERT_VALID(pRecSource);

	*this = *pRecSource;
}

//-----------------------------------------------------------------------------
void SqlRecord::Initialize()
{
	m_pSqlConnection		= NULL;
	m_pTableInfo			= NULL;
	m_pSqlNewFieldsTable	= NULL;

	m_wRecStatus			= 0;
	m_wTBBindMap			= 0;
	m_bAutoIncrement		= false;
	m_bPrepareBanner		= true;
	m_bEndBind				= false;
	m_bBindingDynamically	= false;
	m_bInsertedByUI			= false;

	f_TBGuid				= NULL_GUID;
	f_TBCreated.SetFullDate();
	f_TBModified.SetFullDate();	
}

// cambio la connessione
// se é la stessa non faccio niente altrimenti devo 
// riassegnare le informazioni della tabella e delle colonne utilzzando
// la tableinfo della nuova connessione
//-----------------------------------------------------------------------------
void SqlRecord::SetConnection(SqlConnection* pConnection)
{
	if (!pConnection)
	{
		ASSERT(FALSE);
		return;
	}

	BOOL bUpdate =
		!m_pSqlConnection
		||
		(
		m_pSqlConnection != pConnection
		&& (m_pSqlConnection->m_pCatalog != pConnection->m_pCatalog)
		);
	
	if (!bUpdate)
		return;

	if (GetSize() > 0)
		UpdateCollateCultureStatus();

	m_pSqlConnection = pConnection;

	m_pTableInfo  = m_pSqlConnection->GetTableInfo(m_strTableName);

	Clear(TRUE); // inizializza lo stato del record rendendolo valido

	if (m_pTableInfo)
	{
		if (!m_pTableInfo->IsValid())
		{
			SetValid(FALSE);
			return;
		}

		int size = m_pTableInfo->GetPreAllocSize ();


		// m_pSqlNewFieldsTable = NULL la prima volta che viene chiamata la SetConnection (vedi nel costrutture di default)
		// le volte successive non devo cancellarla neè ricrearla altrimenti in fase di rebinding delle colonne i dataobj dei campi aggiunti riferiscono ad un area di memoria 
		// non più esistente BugFix#21666
		if (!m_pSqlNewFieldsTable)
		{
			const CRTAddOnNewFieldsArray* pCRTAddOnNewFields = m_pTableInfo->GetCRTAddOnNewFields();
			if (pCRTAddOnNewFields)
			{
				m_pSqlNewFieldsTable = new SqlNewFieldsArray;
				for (int i = 0; i <= pCRTAddOnNewFields->GetUpperBound(); i++)
					if (pCRTAddOnNewFields->GetAt(i))
					{
						SqlAddOnFieldsColumn* pNewFields = (SqlAddOnFieldsColumn*) pCRTAddOnNewFields->GetRuntimeClass(i)->CreateObject();
						m_pSqlNewFieldsTable->Add(pNewFields);
						pNewFields->AttachRecParent(this);
					}
			}
		}
		
		// il rebing lo faccio solo se ho giá effettuato la bindrecord
		// Quando il metodo viene chiamato dal costruttore non ho ancora nessun campo bindato
		if (GetSize() > 0)
		{
			RebindingColumns();
		}
		else
		{ 
			SetAllocSize(size); //prealloco l'array per evitare le realloc
		}
	}
	else
		//la tabella non esiste il SqlRecord é invalido
		SetValid(FALSE);
}

//-----------------------------------------------------------------------------
void SqlRecord::SetBindingDynamically (bool bValue /*TRUE*/)
{
	m_bBindingDynamically = bValue;
}

//-----------------------------------------------------------------------------
void SqlRecord::RefreshDynamicFields ()
{
	//aggiorno il puntatore al table info che potrebbe essere cambiato
	m_pTableInfo = m_pSqlConnection->GetTableInfo(m_strTableName);
	//tolgo i camp dinamici
	for (int i = GetUpperBound(); i >= 0; i--)
	{
		SqlRecordItem* pItem = GetAt(i);
		if (pItem->m_bDynamicallyBound)
			RemoveAt(i);
	}

	//aggiungo i nuovo campi dinamici
	int pos = GetSize();
	BindDynamicDeclarations(pos);
}
//-----------------------------------------------------------------------------
void SqlRecord::BindDynamicDeclarations (int& nStartPos)
{
	DatabaseObjectsTableConstPtr dot = AfxGetDatabaseObjectsTable();
	if (m_bCreatedAsEmpty || !dot)
		return;

	const CDbObjectDescription* pDescri = dot->GetDescription(GetTableName());
	if (!pDescri)
		return;

	m_bBindingDynamically = true;

	CDbFieldDescription* pFieldDescri;
	int i;
	for (i=0; i <= pDescri->GetDynamicFields().GetUpperBound(); i++)
	{
		pFieldDescri = (CDbFieldDescription*) pDescri->GetDynamicFields().GetAt(i);
		if (!pFieldDescri || pFieldDescri->GetName().IsEmpty())
			continue;

		//campo già bindato, lo salto
		SqlRecordItem* pRecItem = GetItemByColumnName(pFieldDescri->GetName());
		if (pRecItem)
			continue;

		switch (pFieldDescri->GetColType())
		{
		case CDbFieldDescription::Column:
			if (pFieldDescri->GetContextName().IsEmpty())
				BindDataObj(nStartPos++, pFieldDescri->GetName(), *pFieldDescri->GetValue()->Clone());
			else 
				BindContextDataObj(nStartPos++, pFieldDescri->GetName(), *pFieldDescri->GetValue()->Clone(), pFieldDescri->GetContextName());
			break;
		case CDbFieldDescription::Variable:
			BindLocalDataObj(nStartPos++, pFieldDescri->GetName(), *pFieldDescri->GetValue()->Clone(), pFieldDescri->GetLength());
			break;
		// case CDbFieldDescription::Parameter:
			// store procedure parameters binding cannot be called in this method
			// as now I cannot call virtual methods. See SqlRecordProcedure::BindDynamicParameters
		default:
			break;
		}
	}

	// mandatory fields are always of sqlrecord class and
	// they are never bound as dynamic (see destructor of SqlBindItem)
	m_bBindingDynamically = false;
}


//preparo il messaggio iniziale per eventulai errori in fase di binding
//-----------------------------------------------------------------------------
void SqlRecord::PrepareMessageBanner()
{
	if (!m_bPrepareBanner)
		return;

	if (!IsValid())
		return;
	
	const SqlCatalogEntry* pCatalogEntry = m_pSqlConnection->GetCatalogEntry(m_strTableName);
	if (!pCatalogEntry)
	{
		ASSERT(FALSE);
		return;
	}

	m_pSqlConnection->AddMessage(cwsprintf
									(
										_TB("Errors occurred while binding {0-%s} {1-%s} belonging to application {2-%s} module {3-%s}."),
										GetTypeString(m_nType),
										m_strTableName,
										pCatalogEntry->GetNamespace().GetApplicationName(),
										pCatalogEntry->GetNamespace().GetObjectName(CTBNamespace::MODULE)
									),
									CDiagnostic::Info
								 );
	m_bPrepareBanner = false;
}

//-----------------------------------------------------------------------------
void SqlRecord::RebindingColumns()
{
	SqlRecordItem* pRecItem;
	int nIdx;
	//for (nIdx = 0; nIdx <= GetUpperBound(); nIdx++)
	//{
	//	pRecItem = GetAt(nIdx);		
	//	// per i campi virtuali non devo effettuare nessun rebind
	//	// per gli autoincremental e usedincontextbag mi serve temporaneamente per mantenermi il valore del booleano
	//	//
	//	if (
	//			pRecItem->m_pColumnInfo && 
	//			(pRecItem->m_pColumnInfo->m_bVirtual || pRecItem->m_pColumnInfo->m_bAutoIncrement || pRecItem->m_pColumnInfo->m_bInContextBag)
	//		)
	//		continue;
	//	pRecItem->m_pColumnInfo = NULL;
	//}

	m_bPrepareBanner = true;
	BOOL bValid = TRUE;
	BOOL bAutoincrement = FALSE;
	for (nIdx = 0; nIdx <= GetUpperBound(); nIdx++)
	{
		pRecItem = GetAt(nIdx);
		if (pRecItem->m_pColumnInfo)
		{
			if (pRecItem->m_pColumnInfo->m_bVirtual)
				continue; //non faccio nessun rebing dei campi local
		
			bAutoincrement = pRecItem->m_pColumnInfo->m_bAutoIncrement;
			pRecItem->m_pColumnInfo = NULL;
		}
		else
			bAutoincrement = FALSE;
		
		bValid = BindRecordItem(pRecItem, nIdx, bAutoincrement) && bValid;	
	}
	if (!bValid) 
		SetValid(bValid);
}

//-----------------------------------------------------------------------------
BOOL SqlRecord::BindRecordItem(SqlRecordItem* pRecItem, int nPos, BOOL bAutoIncrement /*=FALSE*/)
{
	if (!pRecItem)
		return FALSE;
	ASSERT_VALID(pRecItem);

	// data is bound by dynamically only when flag is set
	// otherwise it is bound by RebindColumns() or BindRecords()
	if (m_bBindingDynamically)
		pRecItem->m_bDynamicallyBound = TRUE;

	if (m_pTableInfo == NULL) 
		return TRUE; // per utilizzare le tabelle che non appartengono alla connessione principale
	//ASSERT_VALID(m_pTableInfo);

	const SqlColumnInfo* pColumnInfo = pRecItem->GetColumnInfo();
	if (pColumnInfo == NULL)
	{
		if (m_pTableInfo->IsSortedWithRecord())
		{
			if (pRecItem->m_bDynamicallyBound)
				pColumnInfo = m_pTableInfo->GetColumnInfo(pRecItem->m_strColumnName);
			else
				pColumnInfo = m_pTableInfo->GetPhisycalSortedColumn(nPos - m_nLocalsCount);


			if (!pRecItem->m_bDynamicallyBound)
			{
				ASSERT_VALID(pColumnInfo);
				ASSERT(pRecItem->m_strColumnName.CompareNoCase(pColumnInfo->GetColumnName()) == 0);
			}
		}

		if (pColumnInfo == NULL)
		{
			//il SqlRecord registrato utilizza il metodo più efficiente per trovare le colonne (per posizione)
			//gli altri vanno per nome (caso alla Germano in cui si hanno diversi SqlRecord sulla stessa tabella fisica)
			CRuntimeClass* pRegisteredClass = GetTableInfo()->GetSqlCatalogEntry()->GetSqlRecordClass();

			pColumnInfo = pRegisteredClass && GetRuntimeClass()->IsDerivedFrom(pRegisteredClass)
				? m_pTableInfo->GetColumnInfo(pRecItem->m_strColumnName, nPos, FALSE)
				: m_pTableInfo->GetColumnInfo(pRecItem->m_strColumnName);
		}
	}

	if (!pColumnInfo)	
	{
		PrepareMessageBanner();
		m_pSqlConnection->AddMessage(cwsprintf(
												_TB("The field {0-%s} is not defined into table {1-%s}"), 
												(LPCTSTR)pRecItem->m_strColumnName, (LPCTSTR)GetTableName()
									));	
		return FALSE;
	}
		
	//effettuo il check di compatibilitá tra i tipi
	ASSERT_VALID(pRecItem->m_pDataObj);
	if (!(CheckTypeCompatibility(pRecItem->m_pDataObj->GetDataType(), pColumnInfo->m_nSqlDataType)))
	{
		PrepareMessageBanner();
		m_pSqlConnection->AddMessage(cwsprintf
										(
											_TB("Type mismatch of the DataObj related to field {0-%s} into SqlRecord class {1-%s}"),
											(LPCTSTR)pRecItem->m_strColumnName, (LPCTSTR)CString(GetRuntimeClass()->m_lpszClassName)
										 )
									);	
		return FALSE;
	}

#ifdef _DEBUG
	if (this->GetConnection()->GetDatabaseName().CompareNoCase(AfxGetDefaultSqlConnection()->GetDatabaseName()) == 0)
	{
		// se sono in debug controllo che non sia stata già fatto una BIND con lo stesso nome di colonna
		for (int i = 0; i <= GetUpperBound(); i++)
		{
			if (GetAt(i)->m_pColumnInfo == pColumnInfo) 
			{
				TRACE2("SqlRecord::BindGenericDataObj: column %s already exists on sqlrecord class %s\n", pRecItem->m_strColumnName, (LPCTSTR)CString(GetRuntimeClass()->m_lpszClassName));
				PrepareMessageBanner();	
				m_pSqlConnection->AddMessage(cwsprintf
												(
													_TB("Binding of column {0-%s} already exists into SqlRecord class {1-%s}"),
													(LPCTSTR)pRecItem->m_strColumnName, (LPCTSTR)CString(GetRuntimeClass()->m_lpszClassName)
												)
											);
				return FALSE;
			}   
		}
        //verifico che non ci sia già un campo con lo stesso nome
        if (GetIndexFromColumnName(pRecItem->m_strColumnName) > -1)
        {
            ASSERT_TRACE2(FALSE, "Column %s already used by SqlRecord class %s\n", (LPCTSTR)pRecItem->m_strColumnName, (LPCTSTR)CString(GetRuntimeClass()->m_lpszClassName));
 			PrepareMessageBanner();	
			m_pSqlConnection->AddMessage(cwsprintf
											(
												_TB("Binding of column {0-%s} already exists into SqlRecord class {1-%s}"),
												(LPCTSTR)pRecItem->m_strColumnName, (LPCTSTR)CString(GetRuntimeClass()->m_lpszClassName)
											)
										);
       } 
       if (GetIndexFromDataObj(pRecItem->m_pDataObj) > -1)
       {
            ASSERT_TRACE2(FALSE, "Column %s already used by SqlRecord class %s\n", (LPCTSTR)pRecItem->m_strColumnName, (LPCTSTR)CString(GetRuntimeClass()->m_lpszClassName));
 			PrepareMessageBanner();	
			m_pSqlConnection->AddMessage(cwsprintf
											(
												_TB("Binding of column {0-%s} already exists into SqlRecord class {1-%s}"),
												(LPCTSTR)pRecItem->m_strColumnName, (LPCTSTR)CString(GetRuntimeClass()->m_lpszClassName)
											)
										);
       } 
	}
#endif //_DEBUG

	const_cast<SqlColumnInfo*>(pColumnInfo)->m_bAutoIncrement = (bAutoIncrement == TRUE);
	pRecItem->SetColumnInfo(pColumnInfo);
	return TRUE;
}

//-----------------------------------------------------------------------------
void SqlRecord::AddPrimaryKeyColumn	(const CString& aColumnName)
{
	int i = GetIndexFromColumnName(aColumnName);
	ASSERT (i >= 0);

	m_aSqlPrimaryKeyIndexes.Add(i);
}

//-----------------------------------------------------------------------------
SqlRecordItem* SqlRecord::BindGenericDataObj 
	(
		int	nPos, 
		const CString& strColumnName, 
		DataObj& aDataObj, 
		BOOL bAutoIncrement /*=FALSE*/,
		const SqlColumnInfo* pColInfo /*= NULL*/
	)
{
	if (m_nType == VIRTUAL_TYPE)
	{
		ASSERT(FALSE);
		return NULL;
	}

	SqlRecordItem* pRecItem = new SqlRecordItem(&aDataObj, strColumnName, pColInfo);

	if (BindRecordItem(pRecItem, nPos, bAutoIncrement))
	{
		int index = Add(pRecItem);

		if (pRecItem->IsSpecial())
		{
			m_aSqlPrimaryKeyIndexes.Add(index);
			if (m_bBindingDynamically && aDataObj.GetDataType() == DATA_STR_TYPE)
				aDataObj.SetUpperCase();
		}
	}
	else
	{
		SetValid(FALSE);
		delete pRecItem;
	}
	return pRecItem;
}

//-----------------------------------------------------------------------------
SqlRecordItem* SqlRecord::BindDataObj (int nPos, const CString& strColumnName, DataObj& aDataObj)
{
	return BindGenericDataObj(nPos, strColumnName, aDataObj);
}

//é un campo di tipo identity
//-----------------------------------------------------------------------------
SqlRecordItem* SqlRecord::BindAutoIncrementDataObj(int nPos, const CString& strColumnName, DataObj& aDataObj)
{
	SqlRecordItem* pRecItem = BindGenericDataObj(nPos, strColumnName, aDataObj, TRUE);	
	m_bAutoIncrement = TRUE;	
	return pRecItem;
}


//Bind di un campo che corrisponde ad un context element bag
//-----------------------------------------------------------------------------
SqlRecordItem* SqlRecord::BindContextDataObj(int nPos, const CString& strColumnName, DataObj& aDataObj, const CString& strContextElementName)
{
	SqlRecordItem* pRecItem = BindGenericDataObj(nPos, strColumnName, aDataObj);
	pRecItem->m_strContextElementName = strContextElementName;
	m_arContextBagElements.Add(pRecItem);
	return pRecItem;
}

//-----------------------------------------------------------------------------
SqlRecordItem* SqlRecord::BindDynamicDataObj(const CString& strColumnName, DataObj& aDataObj, int nLen)
{
	SqlRecordItem* pItem = BindLocalDataObj(0, strColumnName, aDataObj, nLen);
	m_nDummyCount++;
	if (pItem)
		pItem->m_bDynamicallyBound = TRUE;
	return pItem;
}

//-----------------------------------------------------------------------------
DataObj* SqlRecord::GetContextDataObj(const CString& strContextElementName)
{
	if (m_arContextBagElements.GetCount() <= 0)
		return NULL;

	for (int idx = 0 ; idx < m_arContextBagElements.GetCount(); idx++)
	{
		SqlRecordItem* pItem = (SqlRecordItem*)(m_arContextBagElements.GetAt(idx));

		if (pItem->m_strContextElementName == strContextElementName)
			return pItem->GetDataObj();
	}

	return NULL;
}

//-----------------------------------------------------------------------------
SqlRecordItem* SqlRecord::BindLocalDataObj (int nPos, const CString& strColumnName, DataObj& aDataObj, int nLen, BOOL bIsCollateCultureSensitive /*TRUE*/)
{
       aDataObj.SetAllocSize(nLen);
       
       SqlRecordItem* pRecItem = NULL;

#ifdef _DEBUG
	   {
		//verifico che non ci sia già un campo con lo stesso nome
        if (GetIndexFromColumnName(strColumnName) > -1)
        {
            ASSERT_TRACE2(FALSE, "Column %s already used by SqlRecord class %s\n", strColumnName, CString(GetRuntimeClass()->m_lpszClassName));
        } 
        if (GetIndexFromDataObj(&aDataObj) > -1)
        {
            ASSERT_TRACE2(FALSE, "Column %s already used by SqlRecord class %s\n", strColumnName, CString(GetRuntimeClass()->m_lpszClassName));
        } 
		}
#endif 
      
       // Attenzione deve essere per forza virtual (locale). In caso di assert
       // si sta cercando di bindare un local dataobj su una colonna fisica (naming collision)
       if (m_bBindingDynamically)
       {
		   if (m_pTableInfo == NULL)
			   return NULL;
             const SqlColumnInfo* pColInfo = m_pTableInfo->AddVirtualColumnInfo(nPos, strColumnName, aDataObj, nLen, bIsCollateCultureSensitive);
             ASSERT(pColInfo->m_bVirtual);
       
#ifdef _DEBUG
			if (pColInfo->m_pOwnerSqlRecordClass == NULL)
			{
				const_cast<SqlColumnInfo*>(pColInfo)->m_pOwnerSqlRecordClass = GetRuntimeClass();
			}
			else if (pColInfo->m_pOwnerSqlRecordClass != GetRuntimeClass())
			{
				ASSERT_TRACE2(FALSE, "Column %s already used by SqlRecord class %s\n", strColumnName, CString(pColInfo->m_pOwnerSqlRecordClass->m_lpszClassName));
			}
#endif

             if (
					pColInfo->m_DataObjType != aDataObj.GetDataType() || 
					pColInfo->m_lLength != nLen ||
					pColInfo->m_bUseCollationCulture != bIsCollateCultureSensitive
				)
             {
                    SetValid(FALSE);
                    PrepareMessageBanner();
                    m_pSqlConnection->AddMessage(cwsprintf(_TB("Duplicated local field: {0-%s} for table: {1-%s}"), strColumnName, m_strTableName));     
                    return NULL;
             }
             pRecItem = new SqlRecordItem(&aDataObj, strColumnName, const_cast<SqlColumnInfo*>(pColInfo));       
             // data is bound by dynamically only when flag is set
             // otherwise it is bound by RebindColumns() or BindRecords()
             pRecItem->m_bDynamicallyBound = TRUE;
       }
       else //caso normale: campo non dinamico
       {
             SqlColumnInfo* pColumnInfo = new SqlColumnInfo
														(
															   m_strTableName,
															   strColumnName,
															   aDataObj
														);
             pColumnInfo->m_bUseCollationCulture = bIsCollateCultureSensitive;
             pColumnInfo->m_lLength              = nLen; 
			 pColumnInfo->m_bVirtual			 = TRUE;

             pRecItem = new SqlRecordItem(&aDataObj, strColumnName, pColumnInfo);
			 pRecItem->m_bOwnColumnInfo = TRUE;
			 pRecItem->m_lLength = nLen;
       }
       
	   m_nLocalsCount++;
       Add(pRecItem);
	   return pRecItem;
}

//-----------------------------------------------------------------------------
const SqlTableInfo*	SqlRecord::GetTableInfo()
{ 
	if (!m_pTableInfo && m_pSqlConnection)
		m_pTableInfo = m_pSqlConnection->GetTableInfo(m_strTableName);
	
	return m_pTableInfo; 
}

//-----------------------------------------------------------------------------
BOOL SqlRecord::IsVirtual ()
{
	if (!this)
		return FALSE;
	 
	const SqlTableInfo* pTableInfo = GetTableInfo();
	if (!pTableInfo)
		return FALSE;
	return pTableInfo->GetSqlCatalogEntry()->IsVirtual();

}

//-----------------------------------------------------------------------------
void SqlRecord::GetTableInfo (SqlTableInfoArray& arTableInfo, BOOL bClear/*=TRUE*/, BOOL bGetExtensions/*=FALSE*/)
{ 
	if (bClear)
		arTableInfo.RemoveAll();

	if (!m_pTableInfo && m_pSqlConnection)
		arTableInfo.Add(m_pSqlConnection->GetTableInfo(m_strTableName));
	else
		arTableInfo.Add(m_pTableInfo); 

	if (m_arExtensions && bGetExtensions)
	{
		for (int i = 0; i < m_arExtensions->GetSize(); i++)
		{
			SqlRecord* pRec = m_arExtensions->GetAt(i);
			pRec->GetTableInfo (arTableInfo, FALSE);
		}
	}
}

//-----------------------------------------------------------------------------
SqlRecordItem* SqlRecord::GetItemByColumnName (const CString& strColumnName)
{
	// it doesn't check record validity, but only want to kwnow if
	// single column is already bound 
	int nIndex = GetIndexFromColumnName(strColumnName);

	return nIndex >= 0 && nIndex < GetSizeEx() ? GetAt(nIndex) : NULL;
}

//-----------------------------------------------------------------------------		
SqlRecordItem* SqlRecord::GetItemByDataObj(const DataObj* pDataObj)
{
	TRY
	{
		int nPos = Lookup(pDataObj);
		if (nPos > -1)
			return GetAt(nPos);
	}
	CATCH(SqlException, e)
	{
		TRACE(_T("%s\n"), (LPCTSTR)e->m_strError);
		THROW_LAST();	
	}
	END_CATCH

	return NULL;
}

//-----------------------------------------------------------------------------
int SqlRecord::GetIndexFromDataObj (const DataObj* pDataObj) const
{
	int sz = GetSizeEx();
	for (int i = 0; i < sz; i++)
		if (GetDataObjAt(i) == pDataObj)
			return i;
	return -1;	
}

//-----------------------------------------------------------------------------
int SqlRecord::Lookup (const CString& strColumnName) const
{
	if (!IsValid())
		return -1;
	
	int idx = GetIndexFromColumnName(strColumnName);
	if (idx >= 0)
           return idx;

	TRACE2("SqlRecord::LookUp: column %s not found in table %s\n", strColumnName, m_strTableName);
	ASSERT(FALSE);
	AfxGetLoginContext()->Lock ();
	ThrowSqlException(_TB("The specified column does not belong to the selected table."));
	return -1;
}

//-----------------------------------------------------------------------------
int SqlRecord::Lookup (const DataObj* pData) const
{
	if (!pData || !IsValid())
		return -1;

	if (m_arExtensions)
	{
		//TODO se lento espandere completamente
		//for (int i = 0; i < m_arExtensions->GetSize(); i++)
		//{
		//	SqlRecord* pSubRec = m_arExtensions->GetAt(i);

		int sz = GetSizeEx();
		for (int i = 0; i < sz; i++)
			if (GetAt(i)->m_pDataObj == pData)
				return i;
	}
	else
	{
		int s = GetSize();
		for (int i = 0; i < s; i++)
			if (GetAt(i)->m_pDataObj == pData)
				return i;
	}

#ifdef _DEBUG
	if (m_pTableInfo)
		TRACE1("SqlRecord::LookUp: dataobj not found in table %s\n", m_strTableName);
	else
		TRACE1("SqlRecord::LookUp: dataobj not found in record %s\n", this->GetRuntimeClass()->m_lpszClassName);
	ASSERT(FALSE);
#endif
	AfxGetLoginContext()->Lock ();
	USES_CONVERSION;
	ThrowSqlException(cwsprintf(_TB("The DataObj of type {0-%s} does not exist in the record {1-%s}"), (LPCTSTR)FromDataTypeToDescr(pData->GetDataType()), A2T(GetRuntimeClass()->m_lpszClassName)));
    return -1;
}

//-----------------------------------------------------------------------------
const SqlRecord* SqlRecord::LookupExtensionFromColumnIndex (int& nIndex) const
{
	ASSERT (m_arExtensions && nIndex >= GetSize());

	nIndex -= GetSize();
	for (int i = 0; i < m_arExtensions->GetSize(); i++)
	{
		SqlRecord* pSubRec = m_arExtensions->GetAt(i);
		if (nIndex < pSubRec->GetSize())
		{
			return pSubRec;
		}
		nIndex -= pSubRec->GetSize();
		if (nIndex < 0)
		{
			ASSERT(FALSE);
			break;
		}
	}
	return NULL;
}

//-----------------------------------------------------------------------------
int SqlRecord::GetExtensionIndex (int nIndex) const
{
	ASSERT (m_arExtensions && nIndex >= GetSize());

	nIndex -= GetSize();
	for (int i = 0; i < m_arExtensions->GetSize(); i++)
	{
		SqlRecord* pSubRec = m_arExtensions->GetAt(i);
		if (nIndex < pSubRec->GetSize())
		{
			return i;
		}
		nIndex -= pSubRec->GetSize();
		if (nIndex < 0)
		{
			ASSERT(FALSE);
			break;
		}
	}
	return -1;
}

//-----------------------------------------------------------------------------
SqlRecordItem* SqlRecord::GetAt (int nIndex) const
{
	if (nIndex < GetSize())
		return (SqlRecordItem*) Array::GetAt(nIndex);

	if (m_arExtensions)
	{
		SqlRecord* pSubRec = const_cast<SqlRecord*>(LookupExtensionFromColumnIndex(nIndex));
		return pSubRec ? pSubRec->GetAt(nIndex) : NULL;
	}
	return NULL;
}

//-----------------------------------------------------------------------------
DataObj* SqlRecord::GetDataObjAt (int nIndex) const
{ 
	return GetAt(nIndex)->m_pDataObj; 
}

//-----------------------------------------------------------------------------
const CString& SqlRecord::GetColumnName (int nIndex) const						
{ 
	return GetAt(nIndex)->GetColumnName(); 
}

//-----------------------------------------------------------------------------
const SqlColumnInfo* SqlRecord::GetColumnInfo (int nIndex) const 
{ 
	return GetAt(nIndex)->m_pColumnInfo; 
}

//-----------------------------------------------------------------------------
const SqlColumnInfo* SqlRecord::GetColumnInfo (const DataObj* pDataObj) const 
{ 
	TRY
	{
		int nPos = Lookup(pDataObj);
		if (nPos > -1)
			return GetColumnInfo(nPos);
	}
	CATCH(SqlException, e)
	{
		TRACE(_T("%s\n"), (LPCTSTR)e->m_strError);
		THROW_LAST();	
	}
	END_CATCH

	return NULL;
}

//-----------------------------------------------------------------------------
const SqlColumnInfo* SqlRecord::GetColumnInfo (const CString& str) const 
{ 
	TRY
	{
		int nPos = Lookup(str);
		if (nPos > -1)
			return GetColumnInfo(nPos);
	}
	CATCH(SqlException, e)
	{
		TRACE(_T("%s\n"), (LPCTSTR)e->m_strError);
		THROW_LAST();	
	}
	END_CATCH

	return NULL;
}

//-----------------------------------------------------------------------------
BOOL SqlRecord::IsEmpty() const
{
    for (int i = 0; i <= GetUpperBound(); i++)
		if (/*GetAt(i) && */!GetAt(i)->IsEmpty())
			return FALSE;
	
	return TRUE;
}
//-----------------------------------------------------------------------------
BOOL SqlRecord::IsPKEmpty() const
{
	for (int i = 0; i <= m_aSqlPrimaryKeyIndexes.GetUpperBound(); i++)
	{
		if (!GetAt(m_aSqlPrimaryKeyIndexes.GetAt(i))->IsEmpty())
			return FALSE;
	}
	
	return TRUE;
}

//-----------------------------------------------------------------------------
BOOL SqlRecord::IsEqual(const SqlRecord& record) const
{
    ASSERT(GetSizeEx() - m_nDummyCount == record.GetSizeEx() - record.m_nDummyCount);
    ASSERT(GetRuntimeClass() == record.GetRuntimeClass());
	int sz = GetSizeEx() - m_nDummyCount;
    for (int i = 0; i < sz; i++)
        if 	(*GetAt(i) != *record.GetAt(i))
			return FALSE;
    
	return TRUE;
}

// lo uso per il salvataggio sul database
//-----------------------------------------------------------------------------
BOOL SqlRecord::IsPhisicalEqual(const SqlRecord& record) const
{
	if (!IsValid())
		return FALSE;
	
    ASSERT(GetSizeEx() - m_nLocalsCount == record.GetSizeEx() - record.m_nLocalsCount);
    ASSERT(GetRuntimeClass() == record.GetRuntimeClass());

	int sz = min (GetSizeEx(), record.GetSizeEx());
    for (int i = 0; i < sz; i++)
	{
		//escludo i virtual
		if	(GetAt(i)->m_pColumnInfo->m_bVirtual) 
			continue; 
		
		if	(*GetAt(i) != *record.GetAt(i))
			return FALSE;
	}
	return TRUE;
}

//-----------------------------------------------------------------------------
BOOL SqlRecord::IsSpecial(const DataObj* pDataObj)	const
{ 
	if (!pDataObj) 
		return FALSE;
	
	for (int i = 0; i <= m_aSqlPrimaryKeyIndexes.GetUpperBound(); i++)
	{
		if (pDataObj == GetAt(m_aSqlPrimaryKeyIndexes.GetAt(i))->GetDataObj())
			return TRUE;
	}
	return FALSE;
}

//-----------------------------------------------------------------------------
BOOL SqlRecord::IsSpecial (int nIdx)	const
{ 
	for (int i = 0; i <= m_aSqlPrimaryKeyIndexes.GetUpperBound(); i++)
	{
		if (nIdx == m_aSqlPrimaryKeyIndexes.GetAt(i))
			return TRUE;
	}
	return FALSE; 
}

//-----------------------------------------------------------------------------
void SqlRecord::EndBindData(int& nStartPos /*=0*/) 
{
	// i campi sono stati già bindati
	// vedi problema in caso di enhancement
	if (m_bEndBind) return;

	BindAddOnFields(nStartPos);	
	BindMandatoryFields(nStartPos);
	BindDynamicDeclarations(nStartPos);

	// ho fatto il binding 
	m_bEndBind = true;
}

////Bind dei campi obbligatori  f_TBCreated f_TBModified f_TBCreatedID f_TBModifiedID f_TBGuid
//-----------------------------------------------------------------------------
void SqlRecord::BindMandatoryFields(int& nStartPos) 
{	
	if (m_nType == TABLE_TYPE && m_pTableInfo)
	{
		if (m_pTableInfo->ExistCreatedColumn())
		{
			ASSERT (GetIndexFromColumnName(CREATED_COL_NAME) < 0);
			ASSERT_VALID (m_pTableInfo->GetCreatedColumnInfo());
			ASSERT (m_pTableInfo->GetCreatedColumnInfo()->m_strColumnName.CompareNoCase(CREATED_COL_NAME) == 0);

			BindGenericDataObj(nStartPos++, m_pTableInfo->GetCreatedColumnInfo()->m_strColumnName, f_TBCreated, FALSE, m_pTableInfo->GetCreatedColumnInfo());
		}
		
		if (m_pTableInfo->ExistModifiedColumn())
		{
			ASSERT (GetIndexFromColumnName(MODIFIED_COL_NAME) < 0);
			//ASSERT_VALID (m_pTableInfo->GetModifiedColumnInfo());
			//ASSERT (m_pTableInfo->GetModifiedColumnInfo()->m_strColumnName.CompareNoCase(MODIFIED_COL_NAME) == 0);

			BindGenericDataObj(nStartPos++, m_pTableInfo->GetModifiedColumnInfo()->m_strColumnName, f_TBModified, FALSE, m_pTableInfo->GetModifiedColumnInfo());
		}

		if (m_pTableInfo->ExistCreatedIDColumn())
		{
			ASSERT (GetIndexFromColumnName(CREATED_ID_COL_NAME) < 0);
			ASSERT_VALID (m_pTableInfo->GetCreatedIDColumnInfo());
			ASSERT (m_pTableInfo->GetCreatedIDColumnInfo()->m_strColumnName.CompareNoCase(CREATED_ID_COL_NAME) == 0);

			BindGenericDataObj(nStartPos++, m_pTableInfo->GetCreatedIDColumnInfo()->m_strColumnName, f_TBCreatedID, FALSE, m_pTableInfo->GetCreatedIDColumnInfo());
		}
				
		if (m_pTableInfo->ExistModifiedIDColumn())
		{
			ASSERT (GetIndexFromColumnName(MODIFIED_ID_COL_NAME) < 0);

			BindGenericDataObj(nStartPos++, m_pTableInfo->GetModifiedIDColumnInfo()->m_strColumnName, f_TBModifiedID, FALSE, m_pTableInfo->GetModifiedIDColumnInfo());
		}

		//Impr# 5936
		//se è una tabella master e non è stata fatta la BIND_GUID espilicita allora effettuo il binding del TBGuid
		if (m_pTableInfo->ExistGuidColumn())
		{
			if (!HasGUID())
			{
				ASSERT(GetIndexFromColumnName(GUID_COL_NAME) < 0);
				BindGenericDataObj(nStartPos++, m_pTableInfo->GetGuidColumnInfo()->m_strColumnName, f_TBGuid, FALSE, m_pTableInfo->GetGuidColumnInfo());
				SetWithGUID();
			}
		}
		else
			if (!m_bBindingDynamically && m_pTableInfo->IsMasterTable())
				ASSERT(FALSE);
	}
}

//-----------------------------------------------------------------------------
void SqlRecord::BindAddOnFields(int& nStartPos) 
{
    if (!m_pSqlNewFieldsTable) return;
	
		//effettua il bind dei campi aggiunti 
    for (int i = 0; i <= m_pSqlNewFieldsTable->GetUpperBound(); i++)
	{
		SqlAddOnFieldsColumn* pNewFields = m_pSqlNewFieldsTable->GetAt(i);
		if (pNewFields)	
			nStartPos = pNewFields->BindAddOnFields(nStartPos);
	}
}

//-----------------------------------------------------------------------------
void SqlRecord::Init()
{
	if (!IsValid())
		return;

	// Inizializza tutti i DataObj di cui e` composto l'array
	int sz = GetSizeEx();
	for (int i = 0; i < sz; i++)
		GetAt(i)->Init();

	//effettua l'inizializzazione dei campi aggiunti
	if (m_pSqlNewFieldsTable) 
	{
		for (int i = 0; i <= m_pSqlNewFieldsTable->GetUpperBound(); i++)
		{
			SqlAddOnFieldsColumn* pNewFields = m_pSqlNewFieldsTable->GetAt(i);
			if (pNewFields) pNewFields->InitAddOnFields();
		}
	}

	// pre-initialize fields bound to context infos with their value
	if (m_arContextBagElements.GetCount() > 0 && AfxGetThreadContextBag())
	{
		for(int idx = 0 ; idx < m_arContextBagElements.GetCount(); idx++)
		{
			SqlRecordItem* pItem = (SqlRecordItem*)(m_arContextBagElements.GetAt(idx));

			if (
					pItem->m_strContextElementName.IsEmpty() ||	
					!AfxGetThreadContextBag()->LookupContextObject(pItem->m_strContextElementName)
				)
				continue;

			pItem->GetDataObj()->Assign(*(DataObj*)AfxGetThreadContextBag()->LookupContextObject(pItem->m_strContextElementName));
		}
	}
}

//-----------------------------------------------------------------------------
BOOL SqlRecord::IsDirty () const
{
	if (!IsValid())
		return FALSE;
	
    for (int i = 0; i <= GetUpperBound(); i++)
    	if (GetAt(i)->IsDirty() && ! GetAt(i)->m_pColumnInfo->m_bVirtual)
    		return TRUE;
    
    return FALSE;
}

//-----------------------------------------------------------------------------
void SqlRecord::SetDirty (BOOL bDirty)
{
	// Inizializza tutti i DataObj di cui e` composto l'array
    for (int i = 0; i <= GetUpperBound(); i++)
    	GetAt(i)->SetDirty(bDirty);
}

//-----------------------------------------------------------------------------
void SqlRecord::SetFlags(BOOL bDirty, BOOL bModified)
{
	// Inizializza tutti i DataObj di cui e` composto l'array
    for (int i = 0; i <= GetUpperBound(); i++)
    {
		GetAt(i)->SetDirty(bDirty);
    	GetAt(i)->SetModified(bModified);
    }
}

// stringa della chiave primaria, da usarsi per il salvataggio della chiave (es in EasyAttachment, TB-UrlLink)
// Esempio : fieldName1:0456001;fieldName2:12/6/1998
//-----------------------------------------------------------------------------
CString SqlRecord::GetPrimaryKeyNameValue() const 
{
	if (!IsValid())
		return _T("");
	
	CString strKey;
	for (int i = 0; i <= m_aSqlPrimaryKeyIndexes.GetUpperBound(); i++)
	{
		SqlRecordItem* pItem = GetAt(m_aSqlPrimaryKeyIndexes.GetAt(i));
		ASSERT_VALID(pItem);

		CString sv(pItem->m_pDataObj->Str(0,0));
		ASSERT(sv.Find(';') < 0);	//TODO 

		strKey += 
			pItem->m_pColumnInfo->m_strColumnName + ':' +
			sv + ';';
	}

	return strKey;
}


// valorizza le chiavi primarie di un sqlrecord a partire da una stringa formattata come
// Esempio : fieldName1:0456001;fieldName2:12/6/1998
//-----------------------------------------------------------------------------
void SqlRecord::SetPrimaryKeyNameValue(const CString& keyDescri)  
{
	if (!IsValid())
		return;	

	CString strKey;
	CStringArray arKeys;
	CString strName, strValue;
	
	int n = ::CStringArray_Split(arKeys, keyDescri);

	for (int nKey = 0; nKey < n; nKey++)
	{
		CString field = arKeys.GetAt(nKey);

		int nPos = //field.Find(_T(":"), 0);
					_tcscspn((LPCTSTR)field, L":=");
		//if (nPos >= 0)
		if (nPos < field.GetLength())
		{
			strName = field.Left(nPos);
			SqlRecordItem* pItem = this->GetItemByColumnName(strName);
			if (pItem)
			{
				strValue = field.Right(field.GetLength() - (nPos + 1));
				pItem->GetDataObj()->Assign(strValue);
			}
		}
	}
}

// forma descrittiva della chiave primaria, da usarsi nei messaggi all'utente
// Esempio : [FieldSegment1: 0456001;FieldSegment2: 12/6/1998]
//-----------------------------------------------------------------------------
CString SqlRecord::GetPrimaryKeyDescription() const 
{
	if (!IsValid())
		return _T("");
	
	CString strKey;
	for (int i = 0; i <= m_aSqlPrimaryKeyIndexes.GetUpperBound(); i++)
	{
		SqlRecordItem* pItem = GetAt(m_aSqlPrimaryKeyIndexes.GetAt(i));
		ASSERT_VALID(pItem);

		strKey += 
			AfxLoadDatabaseString(pItem->m_pColumnInfo->m_strColumnName, pItem->m_pColumnInfo->m_strTableName) + _T(": ") +
			pItem->m_pDataObj->Str() + _T("\r\n");
	}

	return strKey;
}

// valorizza le chiavi primarie di un sqlrecord a partire da una stringa formattata come
// Esempio : fieldName1:0456001;fieldName2:12/6/1998
//-----------------------------------------------------------------------------
void SqlRecord::SetColumnNameValue(const CString& keyDescri, CBaseDocument* pDoc/* = NULL*/)  
{
	if (!IsValid())
		return;	
	CString strKey;
	CStringArray arKeys;
	CString strName, strValue;

	int n = ::CStringArray_Split(arKeys, keyDescri);

	for (int nKey = 0; nKey < n; nKey++)
	{
		CString field = arKeys.GetAt(nKey);

		int nPos = //field.Find(_T(":"), 0);
					_tcscspn((LPCTSTR)field, L":=");
		//if (nPos >= 0)
		if (nPos < field.GetLength())
		{
			strName = field.Left(nPos);
			SqlRecordItem* pItem = this->GetItemByColumnName(strName);
			if (pItem)
			{
				strValue = field.Right(field.GetLength() - (nPos + 1));
				pItem->GetDataObj()->Assign(strValue);
			}
			else if (pDoc)
			{
				strValue = field.Right(field.GetLength() - (nPos + 1));
				pDoc->ValorizeVariable(strName, strValue);
			}
		}
	}
}
//-----------------------------------------------------------------------------
CString SqlRecord::GetRecordDescription() const
{
	return GetPrimaryKeyDescription();
}

//-----------------------------------------------------------------------------
CString SqlRecord::ToString() const 
{
	if (!IsValid())
		return _T("");
	
	CString str;
	for (int i = 0; i <= GetSizeEx(); i++)
	{
		SqlRecordItem* pItem = GetAt(i);
		ASSERT_VALID(pItem);

		str += 
			pItem->m_pColumnInfo->m_strColumnName + ':' +
			pItem->m_pDataObj->Str() + _T("\r\n");
	}
	return str;
}

// forma descrittiva dei campi passati come parametri
// Esempio : [nome campo1: 0456001; nome campo2: 12/6/1998]
//-----------------------------------------------------------------------------
CString SqlRecord::GetFieldsValueDescription(CStringArray* pFields) 
{
	if (!IsValid() || !pFields)
		return _T("");
	
	CString strKey;
	for (int i = 0; i <= pFields->GetUpperBound(); i++)
	{
		SqlRecordItem* pItem = GetItemByColumnName(pFields->GetAt(i));
		if (pItem)
			strKey = strKey +
				pItem->m_pColumnInfo->m_strColumnName + ':' +
				pItem->m_pDataObj->Str() + _T("\r\n");
	}

	return strKey;
}

//-------------------------------------------------------------------------------
void SqlRecord::GetKeyInXMLFormat(CString& strKey, BOOL bEnumAsString /*= FALSE*/) const
{
	if (!IsValid())
		return;
	
	strKey.Empty();
	for (int i = 0; i <= m_aSqlPrimaryKeyIndexes.GetUpperBound(); i++)
	{
		SqlRecordItem* pItem = GetAt(m_aSqlPrimaryKeyIndexes.GetAt(i));

		ASSERT(pItem->m_pDataObj);
		if (i > 0)
			strKey += BLANK_CHAR;
		strKey += (
				(pItem->m_pDataObj->GetDataType() == DATA_ENUM_TYPE && bEnumAsString)
				? pItem->m_pDataObj->FormatDataForXML(FALSE)
				: pItem->m_pDataObj->FormatDataForXML()
				);
	}
}

//-----------------------------------------------------------------------------------
void SqlRecord::GetLockedFields(DataObjArray& arLockedFields, BOOL bOnlySpecialFields /*=FALSE*/, BOOL bClearArray /*=TRUE*/ ) const
{
	if (!IsValid())
		return;
	
	if (bClearArray)
		arLockedFields.RemoveAll();

	// restituisco un sottoinsieme di puntatori ai DataObj del SqlRecord
	// non devo deletarli quando si distrugge l'array;
	arLockedFields.SetOwns (FALSE);
	
	for (int i = 0; i <= GetUpperBound(); i++)
	{
		SqlRecordItem* pItem = GetAt(i);
		if ((!bOnlySpecialFields || pItem->m_pColumnInfo->m_bSpecial) && 
			pItem->m_pDataObj->IsValueLocked ())
		{
			arLockedFields.Add(pItem->m_pDataObj);
		}
	}
}

//-------------------------------------------------------------------------------
int SqlRecord::GetAllocSize() const
{
	if (!IsValid())
		return -1;
	 	
	int nSize = sizeof(*this) + m_strQualifier.GetLength();
	for (int i = 0; i < GetSize(); i++)
	{
		SqlRecordItem* pItem = GetAt(i);
		switch (pItem->m_pColumnInfo->GetDataObjType().m_wType)
		{
			case DATA_STR_TYPE:
				nSize += pItem->m_pColumnInfo->m_lLength;
				break;

			case DATA_BLOB_TYPE:
				nSize += pItem->m_pColumnInfo->m_lLength;
				break;

			default: ;
		}
	}
	return nSize;
}

//-----------------------------------------------------------------------------
int	 SqlRecord::GetRecordSize()	const
{
	int size = 
		sizeof (*this) + 
		m_strTableName.GetAllocLength() *  sizeof(TCHAR) +
		m_strQualifier.GetAllocLength() *  sizeof(TCHAR);

	for (int i = 0; i <= GetUpperBound(); i++)
	{
		SqlRecordItem* pItem = GetAt(i);
		ASSERT(pItem->m_pDataObj);

		size +=
			sizeof(SqlRecordItem) +
			sizeof(pItem) +
			sizeof(CString) +
			pItem->m_strColumnName.GetAllocLength() *  sizeof(TCHAR) +
			pItem->m_pDataObj->GetAllocSize();

		switch (pItem->m_pDataObj->GetDataType().m_wType)
		{
			case DATA_STR_TYPE: size += sizeof(DataStr); break;
			case DATA_INT_TYPE: size += sizeof(DataInt); break;
			case DATA_LNG_TYPE: size += sizeof(DataLng); break;
			case DATA_DBL_TYPE: size += sizeof(DataDbl); break;
			case DATA_MON_TYPE: size += sizeof(DataMon); break;
			case DATA_QTA_TYPE: size += sizeof(DataQty); break;
			case DATA_PERC_TYPE: size += sizeof(DataPerc); break;
			case DATA_DATE_TYPE: size += sizeof(DataDate); break;
			case DATA_BOOL_TYPE: size += sizeof(DataBool); break;
			case DATA_ENUM_TYPE: size += sizeof(DataEnum); break;
			case DATA_GUID_TYPE: size += sizeof(DataGuid); break;
			case DATA_TXT_TYPE: size += sizeof(DataText); break;
		}
	}

	if (m_arExtensions) 
	{
		for (int j = 0; j < m_arExtensions->GetSize(); j++)
		{
			SqlRecord* pSubRec = m_arExtensions->GetAt(j);
			size += pSubRec->GetRecordSize();
		}
	}
	return size;
}

//-----------------------------------------------------------------------------
void SqlRecord::SetVisible(int nIdx, BOOL bVisible)
{
	SqlColumnInfo* pInfo = const_cast<SqlColumnInfo*>(GetColumnInfo(nIdx));
	pInfo->m_bVisible = (bVisible == TRUE); 
}

//-----------------------------------------------------------------------------
void SqlRecord::ClearVisible()
{
	if (!IsValid())
		return;
	
	for (int i = 0; i < GetSize(); i++)
	{
		SqlColumnInfo* pInfo = const_cast<SqlColumnInfo*>(GetColumnInfo(i));
		pInfo->m_bVisible = FALSE;
	}
}

//-----------------------------------------------------------------------------
SqlAddOnFieldsColumn* SqlRecord::GetAddOnFields(const CRuntimeClass* pRTNewFields) const
{
	if (!IsValid())
		return NULL;
	
	if (!pRTNewFields || !m_pSqlNewFieldsTable) return NULL;	

	for (int i = 0; i <= m_pSqlNewFieldsTable->GetUpperBound(); i++)
		if (
				m_pSqlNewFieldsTable->GetAt(i) && 
				m_pSqlNewFieldsTable->GetAt(i)->GetRuntimeClass() == pRTNewFields
			)
			return m_pSqlNewFieldsTable->GetAt(i);

	return NULL;
}

// itera sulle SqlAddOnFields per permettere al programmatore di modificare i valori
// del proprio addonfields in base a valori presenti in un altro SqlRecord (vedi caso FinderDoc)
//-----------------------------------------------------------------------------
void SqlRecord::ModifyAddOnFieldsValue(SqlRecord* pRec)
{
	if (!IsValid())
		return;
	
	if (!pRec || !m_pSqlNewFieldsTable) return;	

	for (int i = 0; i <= m_pSqlNewFieldsTable->GetUpperBound(); i++)
		if (m_pSqlNewFieldsTable->GetAt(i)) 
			m_pSqlNewFieldsTable->GetAt(i)->ModifyAddOnFieldsValue(pRec);
}

//-----------------------------------------------------------------------------
CString	SqlRecord::GetQualifiedColumnName(const SqlColumnInfo* pColumnInfo, BOOL bFindExtensions/*=TRUE*/, BOOL bMandatory/* = FALSE*/) const	
{ 
	if (!pColumnInfo)
		return _T("");

	const SqlRecord* pRec = this;

	if (m_arExtensions && bFindExtensions)
	{
		for (int i = 0; i < GetSize(); i++)
		{
			if (pColumnInfo == GetAt(i)->m_pColumnInfo)
				goto l_GetQualifiedColumnName;
		}
		for (int j = 0; j < m_arExtensions->GetSize(); j++)
		{
			const SqlRecord* pSubRec = m_arExtensions->GetAt(j);
			for (int i = 0; i < pSubRec->GetSize(); i++)
			{
				if (pColumnInfo == pSubRec->GetAt(i)->m_pColumnInfo)
				{
					pRec = pSubRec;
					goto l_GetQualifiedColumnName;
				}
			}
		}
	}

l_GetQualifiedColumnName:

	CString strColumn;
	if (pColumnInfo->m_bNativeColumnExpr)
	{
		strColumn = pColumnInfo->GetColumnName();
	}
	else 
	{
		if (!pRec->m_strQualifier.IsEmpty())
			strColumn = pRec->m_strQualifier + DOT_CHAR + pColumnInfo->GetColumnName();
		else  
		{
			if (bMandatory)
				strColumn = pRec->GetTableName() + DOT_CHAR + pColumnInfo->GetColumnName();
			else	//ATTENZIONE in alcuni punti non se lo aspetta
				strColumn = /*pRec->GetTableName() + DOT_CHAR + */pColumnInfo->GetColumnName();
		}
	}
	return strColumn;
}

//-----------------------------------------------------------------------------
CString	SqlRecord::GetQualifiedColumnName (const DataObj* pDataObj, BOOL bMandatory/* = FALSE*/) const		
{ 
	if (m_arExtensions)
	{
		int idx = Lookup(pDataObj);
		if (idx < GetSize())
			return GetQualifiedColumnName(GetColumnInfo(idx), FALSE, bMandatory);

		const SqlRecord* pRec = LookupExtensionFromColumnIndex (idx);
		return pRec->GetQualifiedColumnName(pRec->GetColumnInfo(idx), FALSE, bMandatory);
	}

	return GetQualifiedColumnName(GetColumnInfo(pDataObj), FALSE, bMandatory); 
}

//-----------------------------------------------------------------------------
CString	SqlRecord::GetQualifiedColumnName (int idx, BOOL bMandatory/* = FALSE*/) const						
{ 
	if (m_arExtensions)
	{
		if (idx < GetSize())
			return GetQualifiedColumnName(GetColumnInfo(idx), FALSE, bMandatory);

		const SqlRecord* pRec = LookupExtensionFromColumnIndex (idx);
		return pRec->GetQualifiedColumnName(pRec->GetColumnInfo(idx), FALSE, bMandatory);
	}

	return GetQualifiedColumnName(GetColumnInfo(idx), FALSE, bMandatory);
}

//-----------------------------------------------------------------------------
const CInfoOSL* SqlRecord::GetOSLTableInfo()
{
	if (m_pTableInfo)
		return m_pTableInfo->GetOSLTableInfo();

	const SqlCatalogEntry* pCatalogEntry = m_pSqlConnection->GetCatalogEntry(m_strTableName);
	if (pCatalogEntry == NULL)
		return NULL;
	return const_cast<SqlCatalogEntry*>(pCatalogEntry)->GetInfoOSL();
}

//-----------------------------------------------------------------------------		
//[TBWebMethod(thiscall_method=true)]
DataStr SqlRecord::GetFieldValue(DataStr FieldName)
{
	DataObj* pData = GetDataObjFromColumnName(FieldName);
	ASSERT(pData);
	return pData
		? pData->FormatDataForXML()
		: _T("");
}

//-----------------------------------------------------------------------------		
//[TBWebMethod(thiscall_method=true)]
void SqlRecord::SetFieldValue(DataStr FieldName, DataStr Value)
{
	DataObj* pData = GetDataObjFromColumnName(FieldName);
	ASSERT(pData);
	if (!pData)
		return;

	pData->AssignFromXMLString(Value.GetString());
}

//-----------------------------------------------------------------------------		
//[TBWebMethod(name = SqlRecord_IsStorable, thiscall_method=true)]
DataBool SqlRecord::TbScriptIsStorable	()
{ return DataBool(IsStorable()); }

//[TBWebMethod(name = SqlRecord_SetStorable, thiscall_method=true)]
void SqlRecord::TbScriptSetStorable	(DataBool bStorable)
{ SetStorable((BOOL) bStorable); }

//-----------------------------------------------------------------------------	
DataObj* SqlRecord::GetDataObjFromColumnName (const CString& str) 
{ 
	int i = GetIndexFromColumnName(str);
	if (i < 0)
		return NULL;
	
	return GetDataObjAt(i);
}

//-----------------------------------------------------------------------------	
//[TBWebMethod(thiscall_method=true)]
DataObj* SqlRecord::GetDataObjFromName (DataStr str) 
{ 
	return GetDataObjFromColumnName (str.GetString());
}

//-----------------------------------------------------------------------------
long SqlRecord::GetColumnLength(const DataObj* pDataObj) const
{
	TRY
	{
		SqlRecordItem* pItem = const_cast<SqlRecord*>(this)->GetItemByDataObj(pDataObj);
		if (pItem)
			return pItem->GetColumnLength();
	}
	CATCH(SqlException, e)
	{
		THROW_LAST();
	}
	END_CATCH
	
	return 0;
}

//-----------------------------------------------------------------------------
CString SqlRecord::GetAutoIncrementColumn() const
{
	for (int i = 0; i < GetSize(); i++)
	{
		const SqlColumnInfo* pColumn = GetColumnInfo(i);
		if (pColumn->m_bAutoIncrement)
			return pColumn->GetColumnName();
	}
	return _T("");	
}

//-----------------------------------------------------------------------------
BOOL SqlRecord::IsPresent (const DataObj* pDataObj, int& nIdx) const
{
	nIdx = GetIndexFromDataObj (pDataObj);
	return nIdx >= 0;	
}

//-----------------------------------------------------------------------------
int SqlRecord::GetNumberSpecialColumns () const
{
	return m_aSqlPrimaryKeyIndexes.GetSize();
}

//----------------------------------------------------------------------------------------
void SqlRecord::GetCopyPrimaryKeyIndexes(CUIntArray* pPrimaryKeyIndexes) const
{
	if (!pPrimaryKeyIndexes)
		return;

	for (int i = 0; i < m_aSqlPrimaryKeyIndexes.GetSize(); i++)
		pPrimaryKeyIndexes->Add(m_aSqlPrimaryKeyIndexes.GetAt(i));
}

//----------------------------------------------------------------------------------------
SqlRecordItem* SqlRecord::GetSpecialColumn(int nSegment) const
{
	return GetAt(m_aSqlPrimaryKeyIndexes.GetAt(nSegment));
}

//----------------------------------------------------------------------------------------
void SqlRecord::GetKeyStream (DataObjArray& arPkSegments, BOOL bClone /*=TRUE*/) const
{
	if (!IsValid())
		return;
	
	arPkSegments.RemoveAll();
	arPkSegments.SetOwns (bClone);

	for (int i = 0; i <= m_aSqlPrimaryKeyIndexes.GetUpperBound(); i++)
	{
		SqlRecordItem* pItem = GetAt(m_aSqlPrimaryKeyIndexes.GetAt(i));
		arPkSegments.Add (bClone ? pItem->m_pDataObj->DataObjClone() : pItem->m_pDataObj);
	}
}

//----------------------------------------------------------------------------------------
BOOL SqlRecord::Parse(CXMLNode* pNode, BOOL bWithAttributes /*TRUE*/, BOOL bParseLocal/* = FALSE*/)
{
	for (int nIdx = 0; nIdx <= GetUpperBound(); nIdx++)
	{
		if (IsVirtual(nIdx) && !bParseLocal)
			continue;

		GetAt(nIdx)->Parse(pNode, bWithAttributes);
	}
	return TRUE;
}

//----------------------------------------------------------------------------------------
BOOL SqlRecord::UnParse(CXMLNode* pNode, BOOL bWithAttributes /*TRUE*/, BOOL bUnparseLocal/* = FALSE*/, BOOL bSoapType /*= TRUE*/)
{
	for (int nIdx = 0; nIdx <= GetUpperBound(); nIdx++)
	{
		if (IsVirtual(nIdx) && !bUnparseLocal)
			continue;

		GetAt(nIdx)->UnParse(pNode, bWithAttributes, bSoapType);
	}
	return TRUE;
}

//-----------------------------------------------------------------------------
void SqlRecord::SetStatus (BOOL bValid, RecordStatus aStatusFlag)
{
	if (bValid)
		m_wRecStatus |= aStatusFlag;
	else
		m_wRecStatus &= ~aStatusFlag; 
}
	
//---------------------------------------------------------------------------------------------------------------------
void SqlRecord::Clear(BOOL bValid)	
{ m_wRecStatus = 0; SetValid(bValid); }

void SqlRecord::SetValid (BOOL bValid)		
{ SetStatus (bValid,	VALID); }

BOOL SqlRecord::IsValid		()		const		
{ return (m_wRecStatus & VALID)		== VALID;		}

void SqlRecord::SetModified	(BOOL bModified)	
{ SetStatus(bModified,	MODIFIED);	}

BOOL SqlRecord::IsModified	()		const		
{ return (m_wRecStatus & MODIFIED)	== MODIFIED;	}

BOOL SqlRecord::IsDataModified()		const
{
	for (int nCol = 0; nCol < GetSizeEx(); nCol++)
		if (GetDataObjAt(nCol)->IsModified())
			return TRUE;
	return FALSE;
}

void SqlRecord::SetDataModified(BOOL bModified)
{
	for (int nCol = 0; nCol < GetSizeEx(); nCol++)
		GetDataObjAt(nCol)->SetModified(bModified);
}

void SqlRecord::SetStorable	(BOOL bStorable)	
{ 
#ifdef _DEBUG	
	if (bStorable)
		ASSERT(!IsNeverStorable	());
#endif
	SetStatus(bStorable,	STORABLE);	
}

BOOL SqlRecord::IsStorable	()		const		
{ return (m_wRecStatus & STORABLE)	== STORABLE;	}

void SqlRecord::SetNeverStorable	(BOOL b)	
{ 
	SetStatus(b,	NEVER_STORABLE);
	if (b)
		SetStatus(FALSE,	STORABLE);
}

BOOL SqlRecord::IsNeverStorable	()		const		
{ return (m_wRecStatus & NEVER_STORABLE) == NEVER_STORABLE;	}

//-----------------------------------------------------------------------------
void SqlRecord::UpdateCollateCultureStatus ()
{
	int sz = GetSizeEx();
	for (int i = 0; i < sz; i++)
	{
		SqlRecordItem* pItem = GetAt(i);
		ASSERT(pItem);
		pItem->UpdateCollateCultureStatus();
	}		
}

//---------------------------------------------------------------------------------------------------------------------
void SqlRecord::SetReadOnly (BOOL bReadOnly)
{
	int sz = GetSizeEx();
	for (int nCol = 0; nCol < sz; nCol++)	
	{
		DataObj* pD = GetDataObjAt(nCol);
		pD->SetReadOnly	(bReadOnly);
	}
}

//---------------------------------------------------------------------------------------------------------------------
void SqlRecord::SetDataOSLReadOnly (BOOL bReadOnly, int idx/* = -1*/)
{
	if (idx > -1)
	{
		DataObj* pD = GetDataObjAt(idx);
		ASSERT(pD);
		if (pD)
			pD->SetOSLReadOnly	(bReadOnly);
		return;
	}

	int sz = GetSizeEx();
	for (int nCol = 0; nCol < sz; nCol++)	
	{
		DataObj* pD = GetDataObjAt(nCol);
		pD->SetOSLReadOnly	(bReadOnly);
	}
}

//---------------------------------------------------------------------------------------------------------------------
void SqlRecord::SetDataHideAndReadOnly (BOOL b, int idxSkipIt/* = -1*/)
{
	int sz = GetSizeEx();
	for (int nCol = 0; nCol < sz; nCol++)	
	{
		if (nCol == idxSkipIt) continue;

		DataObj* pD = GetDataObjAt(nCol);

		pD->SetAlwaysReadOnly	(b);
		pD->SetHide	(b);
	}
}

//---------------------------------------------------------------------------------------------------------------------
void SqlRecord::CopyAttribute(SqlRecord * pRec)
{	
	int sz = min(GetSizeEx(), pRec->GetSizeEx());
	for (int nCol = 0; nCol < sz; nCol++)	
	{
		DataObj* pD = GetDataObjAt(nCol);
		DataObj* pS = pRec->GetDataObjAt(nCol);

		//Influenzano l'A. 17516: Impediscono nel bodyedit l'"Add new row" in assenza del permesso di editing delle righe esistenti
		//pD->SetReadOnly		(pS->IsStateReadOnly());
		//pD->SetHide			(pS->IsStateHide());
		//pD->SetOSLReadOnly	(pS->IsOSLReadOnly());
		//pD->SetOSLHide		(pS->IsOSLHide());
		//Versione ottimizzata
		DWORD wFlag;
		if (
				wFlag = pS->GetStatus() &
						(
							DataObj::HIDE | DataObj::OSL_HIDE |
							DataObj::READONLY | DataObj::OSL_READONLY |
							DataObj::BPM_READONLY
						)
			)
				pD->SetStatus (TRUE, wFlag);

		if (pS->IsWebBound())
			pD->SetWebBound();
		//----
	}
}

//---------------------------------------------------------------------------------------------------------------------
int SqlRecord::GetSizeEx () const
{
	int nSize = GetSize();

	if (m_arExtensions) 
	{
		for (int j = 0; j < m_arExtensions->GetSize(); j++)
		{
			nSize += m_arExtensions->GetAt(j)->GetSize();
		}
	}
	return nSize;
}

//---------------------------------------------------------------------------------------------------------------------
BOOL SqlRecord::CompareFieldBy(int nFieldIndex, ECompareType cmp, const CStringArray& arCmpValues, DataObj* pPreAllocated/* = NULL*/, BOOL bCompareNoCase/* = TRUE*/, CParsedCtrl* pCtrl/*= NULL*/)
{
	ASSERT(nFieldIndex >= 0 && nFieldIndex < GetSizeEx());
	if (nFieldIndex < 0) 
		return FALSE;
	if (arCmpValues.IsEmpty())
		return FALSE;

	DataObj* pObj = GetDataObjAt(nFieldIndex);
	if (pObj == NULL) 
		return FALSE;

	if (pCtrl)
	{
		DataStr ds;

		if (pCtrl->m_nUseComponentToFormat > -1)
		{
			pObj = GetDataObjAt(pCtrl->m_nUseComponentToFormat);
			ASSERT_VALID(pObj);

			ds = pObj->Str();
		}
		else
			ds = pCtrl->FormatData(pObj);

		if (ds.IsValid())
			return ds.CompareBy(cmp, arCmpValues, pPreAllocated, bCompareNoCase);
		else
		{
			ASSERT(FALSE);
			return FALSE;
		}
	}

	return pObj->CompareBy(cmp, arCmpValues, pPreAllocated, bCompareNoCase);
}

//---------------------------------------------------------------------------------------------------------------------
BOOL SqlRecord::CompareFieldBy(int nFieldIndex, ECompareType cmp, DataObj* pFilterData, BOOL bCompareNoCase/* = TRUE*/)
{
	ASSERT(nFieldIndex >= 0 && nFieldIndex < GetSizeEx());
	if (nFieldIndex < 0)
		return FALSE;
	if (!pFilterData)
		return FALSE;

	DataObj* pObj = GetDataObjAt(nFieldIndex);
	if (pObj == NULL)
		return FALSE;

	return pObj->CompareBy(cmp, pFilterData, bCompareNoCase);
}

//---------------------------------------------------------------------------------------------------------------------
void SqlRecord::SetColumnAsNativeExpression(DataObj* pDataObj)
{
	int nPos = GetIndexFromDataObj(pDataObj);
	const SqlColumnInfo* pCol = NULL;
	if (nPos > -1)
		pCol = GetColumnInfo(nPos);
	if (pCol)
		const_cast<SqlColumnInfo*>(pCol)->m_bNativeColumnExpr = TRUE;
}
//-----------------------------------------------------------------------------	
void SqlRecord::GetJson(CJsonSerializer& jsonSerializer, BOOL bOnlyWebBound)
{
	for (int i = 0; i < GetSizeEx(); i++)
	{
		SqlRecordItem* pItem = GetAt(i);
		if (pItem->IsMandatory() || !bOnlyWebBound || pItem->GetDataObj()->IsWebBound())
		{
			jsonSerializer.OpenObject(pItem->GetBindingName());
			pItem->GetDataObj()->SerializeToJson(jsonSerializer);
			jsonSerializer.CloseObject();
		}
	}
}

//-----------------------------------------------------------------------------
void SqlRecord::SetJson(CJsonParser& jsonParser)
{
	for (int i = 0; i < GetSizeEx(); i++)
	{
		SqlRecordItem* pItem = GetAt(i);
		if (pItem->IsMandatory())
			continue;
		if (jsonParser.BeginReadObject(pItem->GetColumnName()))
		{
			pItem->GetDataObj()->AssignFromJson(jsonParser);
			jsonParser.EndReadObject();
		}
	}
}


//-----------------------------------------------------------------------------
void SqlRecord::SetWebBound(CJsonParser& jsonParser)
{
	for (int i = 0; i < GetSizeEx(); i++)
	{
		SqlRecordItem* pItem = GetAt(i);
		if (jsonParser.BeginReadObject(pItem->GetColumnName()))
		{
			pItem->GetDataObj()->SetWebBound();
			jsonParser.EndReadObject();
		}
	}
}
//-----------------------------------------------------------------------------
// diagnostics
#ifdef _DEBUG
void SqlRecord::Dump(CDumpContext& dc) const 
{	
	ASSERT_VALID(this); 
	dc << _T("\nSqlRecord = ") << this->GetRuntimeClass()->m_lpszClassName << this->GetTableName();

	__super::Dump(dc);
}
#endif
//////////////////////////////////////////////////////////////////////////////
//					SqlAddOnFieldsColumn Implementation
//////////////////////////////////////////////////////////////////////////////
//
//-----------------------------------------------------------------------------
IMPLEMENT_DYNAMIC(SqlAddOnFieldsColumn, CObject)

//-----------------------------------------------------------------------------
SqlAddOnFieldsColumn::SqlAddOnFieldsColumn()
:
	m_pSqlRecParent (NULL)
{	
}

//-----------------------------------------------------------------------------
const BOOL SqlAddOnFieldsColumn::HasColumn (const CString& sColName) const
{
	for (int i=0; i <= m_arPhysicalColumnsNames.GetUpperBound(); i++)
	{
		if (m_arPhysicalColumnsNames.GetAt(i).CompareNoCase(sColName) == 0)
			return TRUE;
	}

	return FALSE;
}


//-----------------------------------------------------------------------------
BOOL SqlAddOnFieldsColumn::IsEqual(const SqlAddOnFieldsColumn& addOnFields) const
{
	ASSERT(m_arPhysicalColumnsNames.GetSize() == addOnFields.m_arPhysicalColumnsNames.GetSize());
    ASSERT(GetRuntimeClass() == addOnFields.GetRuntimeClass());
	if (!m_pSqlRecParent || !addOnFields.m_pSqlRecParent)
	{
		ASSERT(FALSE);
		return FALSE;
	}
	ASSERT(m_pSqlRecParent->GetRuntimeClass() == addOnFields.m_pSqlRecParent->GetRuntimeClass());

	CString strName;
    for (int i = 0; i <= m_arPhysicalColumnsNames.GetUpperBound(); i++)
    {
		strName = m_arPhysicalColumnsNames.GetAt(i);
		SqlRecordItem* pItem1 = m_pSqlRecParent->GetItemByColumnName(strName);
		SqlRecordItem* pItem2 = addOnFields.m_pSqlRecParent->GetItemByColumnName(strName);
		if ((pItem1 && !pItem2) || (!pItem1 && pItem2) || (*pItem1 != *pItem2))
			return FALSE;
	}
	return TRUE;
}


//-----------------------------------------------------------------------------
SqlAddOnFieldsColumn& SqlAddOnFieldsColumn::operator= (const SqlAddOnFieldsColumn& addOnFields)
{
     ASSERT(GetRuntimeClass() == addOnFields.GetRuntimeClass());
	 ASSERT(addOnFields.m_pSqlRecParent);
	
	if (!m_pSqlRecParent)
		m_pSqlRecParent = addOnFields.m_pSqlRecParent;
	else
		 ASSERT(m_pSqlRecParent->GetRuntimeClass() == addOnFields.m_pSqlRecParent->GetRuntimeClass());

	if (m_arPhysicalColumnsNames.GetCount() == 0)
	{
		for (int i = 0; i <= addOnFields.m_arPhysicalColumnsNames.GetUpperBound(); i++)
			m_arPhysicalColumnsNames.Add(addOnFields.m_arPhysicalColumnsNames.GetAt(i));	
	}
	else
		ASSERT(m_arPhysicalColumnsNames.GetCount() == addOnFields.m_arPhysicalColumnsNames.GetCount());

	CString strName;
    for (int i = 0; i <= m_arPhysicalColumnsNames.GetUpperBound(); i++)
    {
		strName = m_arPhysicalColumnsNames.GetAt(i);
		SqlRecordItem* pItem1 = m_pSqlRecParent->GetItemByColumnName(strName);
		SqlRecordItem* pItem2 = addOnFields.m_pSqlRecParent->GetItemByColumnName(strName);

		if ((pItem1 && !pItem2) || (!pItem1 && pItem2))
			ASSERT(FALSE);
		else if (pItem1 && pItem2)
			*pItem1 = *pItem2;
	}	

	return *this;
}

//-----------------------------------------------------------------------------
// diagnostics
#ifdef _DEBUG
void SqlAddOnFieldsColumn::Dump(CDumpContext& dc) const 
{	
	ASSERT_VALID(this); 
	dc << _T("\nSqlAddOnFieldsColumn = ") << this->GetRuntimeClass()->m_lpszClassName ;

	if (m_pSqlRecParent)
		dc << _T("\nParent Record = ") << m_pSqlRecParent->GetRuntimeClass()->m_lpszClassName ;
}
#endif

//////////////////////////////////////////////////////////////////////////////
//					SqlRecordView Implementation
//////////////////////////////////////////////////////////////////////////////
//
//-----------------------------------------------------------------------------
IMPLEMENT_DYNAMIC(SqlRecordView, SqlRecord)

//-----------------------------------------------------------------------------
SqlRecordView::SqlRecordView(LPCTSTR szTableName, SqlConnection* pConn /*= NULL*/)
:
	SqlRecord	(szTableName, pConn, VIEW_TYPE)
{
}

//-----------------------------------------------------------------------------
SqlRecordView::SqlRecordView(const SqlRecordView& aSqlView)
:
	SqlRecord(aSqlView)
{
}

//////////////////////////////////////////////////////////////////////////////
//					SqlVirtualRecord Implementation
//////////////////////////////////////////////////////////////////////////////
//
//-----------------------------------------------------------------------------
IMPLEMENT_DYNAMIC(SqlVirtualRecord, SqlRecord)

//-----------------------------------------------------------------------------
SqlVirtualRecord::SqlVirtualRecord(LPCTSTR szTableName, SqlConnection* pConn /*= NULL*/)
:
	SqlRecord	(szTableName, pConn, VIRTUAL_TYPE)
{
}

//-----------------------------------------------------------------------------
SqlVirtualRecord::SqlVirtualRecord(const SqlVirtualRecord& aSqlVirtualRec)
:
	SqlRecord(aSqlVirtualRec)
{
}


//////////////////////////////////////////////////////////////////////////////
//					SqlProcedureParamList Definition
//////////////////////////////////////////////////////////////////////////////
//
//-----------------------------------------------------------------------------
SqlProcParamItem* SqlProcedureParamList::GetParamItemFromDataObj(const DataObj* pDataObj) const
{
	if (!pDataObj)
		return NULL;

	for (int i = 0; i <= GetUpperBound(); i++)
	{
		if (GetAt(i)->GetDataObj() == pDataObj)
			return GetAt(i);		
	}

	return NULL;
}

//-----------------------------------------------------------------------------
SqlProcParamItem* SqlProcedureParamList::GetParamItemFromParamInfo(SqlProcedureParamInfo* pParamInfo) const
{
	if (!pParamInfo)
		return NULL;

	for (int i = 0; i <= GetUpperBound(); i++)
	{
		if (GetAt(i)->GetParameterInfo() == pParamInfo)
			return GetAt(i);		
	}

	return NULL;
}

//-----------------------------------------------------------------------------
DataObj* SqlProcedureParamList::GetDataObjFromParamName(const CString& strParamName) const
{
	if (strParamName.IsEmpty())
		return NULL;

	for (int i = 0; i <= GetUpperBound(); i++)
	{
		if (GetAt(i)->GetColumnName().CompareNoCase(strParamName) == 0)
			return GetAt(i)->GetDataObj();		
	}

	return NULL;
}

//////////////////////////////////////////////////////////////////////////////
//					SqlRecordProcedure Definition
//////////////////////////////////////////////////////////////////////////////
//
//-----------------------------------------------------------------------------
IMPLEMENT_DYNAMIC(SqlRecordProcedure, SqlRecord)

//-----------------------------------------------------------------------------
SqlRecordProcedure::SqlRecordProcedure(LPCTSTR szProcedureName, SqlConnection* pConn/*= NULL*/)
:
	SqlRecord				(szProcedureName, pConn, PROC_TYPE),
	m_pProcedureParamList	(NULL)
{
}

//-----------------------------------------------------------------------------
SqlRecordProcedure::SqlRecordProcedure(const SqlRecordProcedure& aSqlProcedure)
:
	SqlRecord(aSqlProcedure)
{
}

//-----------------------------------------------------------------------------
SqlRecordProcedure::~SqlRecordProcedure()
{
	if (m_pProcedureParamList)
		delete m_pProcedureParamList;
}

//-----------------------------------------------------------------------------
SqlRecordProcedure*	SqlRecordProcedure::Create	()
{
	if (GetRuntimeClass(), RUNTIME_CLASS(SqlRecordProcedure))
		return new SqlRecordProcedure(m_strTableName);

	return (SqlRecordProcedure*) GetRuntimeClass()->CreateObject();
}

//-----------------------------------------------------------------------------
BOOL SqlRecordProcedure::BindParamItem(SqlProcParamItem* pParamItem, int nPos)
{
	if (!m_pProcedureParamList || !pParamItem)
	{
		ASSERT(FALSE);
		return FALSE;
	}

	const SqlTableInfo *pTableInfo = GetTableInfo();
	if (pTableInfo == NULL) 
		return FALSE;
	int nParamPos = pTableInfo->GetParamInfoPos(pParamItem->m_strColumnName, nPos);
	if (nParamPos < 0)	
		return FALSE;

	SqlProcedureParamInfo* pParamInfo = pTableInfo->GetParamAt(nParamPos);	
	if (!pParamInfo)	
	{
		PrepareMessageBanner();
		m_pSqlConnection->AddMessage(cwsprintf(_TB("The parameter {0-%s} is not defined"), pParamItem->m_strColumnName));	
		return FALSE;
	}

	//effettuo il check di compatibilitá tra i tipi
	if (!(CheckTypeCompatibility(pParamItem->m_pDataObj->GetDataType(), pParamInfo->m_nDataType)))
	{
		PrepareMessageBanner();
		m_pSqlConnection->AddMessage(cwsprintf
										(
											_TB("Type mismatch of the DataObj related to parameter {0-%s}"),
											pParamItem->m_strColumnName
										 )
									);	
		return FALSE;
	}
	// se sono in debug controllo che non sia stata già fatto una BIND con lo stesso nome di colonna
	#ifdef _DEBUG
	for (int i = 0; i <= m_pProcedureParamList->GetUpperBound(); i++)
	{
		if (m_pProcedureParamList->GetAt(i)->m_pParameterInfo == pParamInfo) 
		{
			TRACE2("SqlRecordProcedure::BindParamItem: param %s already exists on stored procedure %s\n", pParamItem->m_strColumnName, m_strTableName);
			PrepareMessageBanner();
			m_pSqlConnection->AddMessage(cwsprintf
											(
												_TB("Warning! Binding of parameter {0-%s} already exists."),
												pParamItem->m_strColumnName
											)
										 );
			return FALSE;
		}   
	}
	#endif //_DEBUG

	pParamItem->SetParameterInfo(pParamInfo);	
	return TRUE;
}

// Integrates binding of the store procedure parameters that weren't bound by 
// BindDynamicDeclarations.
//-----------------------------------------------------------------------------
void SqlRecordProcedure::BindDynamicParameters	()
{
	if (!AfxGetDatabaseObjectsTable())
		return;

	const CDbObjectDescription* pDescri = AfxGetDatabaseObjectsTable()->GetDescription(GetTableName());

	if (!pDescri)
		return;

	m_bBindingDynamically = true;

	int nPos = GetUpperBound();

	CDbFieldDescription* pFieldDescri;
	for (int i=0; i <= pDescri->GetDynamicFields().GetUpperBound(); i++)
	{
		pFieldDescri = (CDbFieldDescription*) pDescri->GetDynamicFields().GetAt(i);

		if (pFieldDescri->GetColType() != CDbFieldDescription::Parameter)
			continue;

		if  (
				pFieldDescri && 
				!pFieldDescri->GetName().IsEmpty()
			)
			BindParamDataObj(nPos++, pFieldDescri->GetName(), *pFieldDescri->GetValue()->Clone());
	}

	m_bBindingDynamically = false;
}

//-----------------------------------------------------------------------------
void SqlRecordProcedure::BindParamDataObj 
	(
		int	nPos, 
		const CString& strParamName, 
		DataObj& aDataObj
	)
{
	if (!IsValid())
		return;
	
	if (!m_pProcedureParamList)
		m_pProcedureParamList = new SqlProcedureParamList;

	int nIdx = -1;
	CString strName = strParamName;
	// La Bind dei nomi viene fatto con la convenzione ANSI 92 con @ davanti 
	// In Oracle il nome del parametro è senza la @, in SqlServer è standard
	if (m_pSqlConnection->GetDBMSType() == DBMS_ORACLE)
	{
		nIdx = strParamName.Find(_T('@'));
		strName = (nIdx >= 0) ? strParamName.Right(strParamName.GetLength() - (nIdx + 1)) : strParamName;
	}

	SqlProcParamItem* pParamItem = new SqlProcParamItem(&aDataObj, strName);
	if (BindParamItem(pParamItem, nPos))
		m_pProcedureParamList->Add(pParamItem);
	else
		delete pParamItem;
}

//-----------------------------------------------------------------------------
SqlProcParamItem* SqlRecordProcedure::GetParamItemFromName (const CString& sName)
{
	if (m_pProcedureParamList)
		for (int i = 0; i <= m_pProcedureParamList->GetUpperBound(); i++)
			if (m_pProcedureParamList->GetAt(i)->GetColumnName().CompareNoCase(sName) == 0)
				return m_pProcedureParamList->GetAt(i);
	return NULL;
}

//-----------------------------------------------------------------------------
void SqlRecordProcedure::RebindingParams()
{
	if (!m_pProcedureParamList)
		ASSERT(FALSE);

	int nIdx;
	// prima devo metterli tutti a NULL se no non funziona il controllo fatto in 
	// _DEBUG nella BindParamItem
	for (nIdx = 0; nIdx <= m_pProcedureParamList->GetUpperBound(); nIdx++)
		m_pProcedureParamList->GetAt(nIdx)->m_pParameterInfo = NULL;		

	BOOL bValid = TRUE;
	for (nIdx = 0; nIdx <= m_pProcedureParamList->GetUpperBound(); nIdx++)
		bValid = BindParamItem(m_pProcedureParamList->GetAt(nIdx), nIdx) && bValid;
	
	if (!bValid) 
		SetValid(bValid);
}

//-----------------------------------------------------------------------------
void SqlRecordProcedure::SetConnection(SqlConnection* pConnection)
{
	if (!pConnection)
	{
		ASSERT(FALSE);
		return;
	}

	if (m_pSqlConnection &&  m_pSqlConnection == pConnection)
	{
		return; 
	}

	SqlRecord::SetConnection(pConnection);
	
	BindDynamicParameters ();

	if (m_pProcedureParamList)
		RebindingParams();
}

//-----------------------------------------------------------------------------
SqlProcParamItem* SqlRecordProcedure::GetParamItemFromParamInfo(SqlProcedureParamInfo* pParamInfo) const
{
	if (!m_pProcedureParamList || !pParamInfo)
		return NULL;

	return m_pProcedureParamList->GetParamItemFromParamInfo(pParamInfo);
}

//-----------------------------------------------------------------------------
DataObj* SqlRecordProcedure::GetDataObjFromColumnName (const CString& strName) 
{
	if (strName.IsEmpty())
		return NULL;
	
	DataObj* pDataObj = NULL;
	if (m_pProcedureParamList)
		pDataObj = m_pProcedureParamList->GetDataObjFromParamName(strName);
	
	return (pDataObj) ? pDataObj : SqlRecord::GetDataObjFromColumnName(strName);
}

//-----------------------------------------------------------------------------
const CString& SqlRecordProcedure::GetColumnName(const DataObj* pDataObj) const
{
	SqlProcParamItem* pItem; 

	//prima controllo nei parametri
	if (m_pProcedureParamList) 
		pItem = m_pProcedureParamList->GetParamItemFromDataObj(pDataObj);


	// controllo nelle colonne di binding
	return  (pItem) ? pItem->m_strColumnName : SqlRecord::GetColumnName(pDataObj);
}
	
//-----------------------------------------------------------------------------
long SqlRecordProcedure::GetColumnLength(const DataObj* pDataObj) const
{
	if (!pDataObj)
		return 0;

	SqlProcParamItem* pItem = NULL; 

	//prima controllo nei parametri
	if (m_pProcedureParamList) 
		pItem = m_pProcedureParamList->GetParamItemFromDataObj(pDataObj);

	if (pItem)
	{
		if (pItem->m_pParameterInfo)
			return pItem->m_pParameterInfo->m_nMaxLength;
		ThrowSqlException
				(
					cwsprintf
						(
							_TB("The parameter {0-%s} of the stored procedure (1-%s} is invalid."),
							pItem->m_strColumnName,
							m_strTableName
						)
				  );
	}

	// controllo nelle colonne di binding
	return  SqlRecord::GetColumnLength(pDataObj);
}

//-----------------------------------------------------------------------------
BOOL SqlRecordProcedure::IsEmpty() const
{
	if (!SqlRecord::IsEmpty())
		return FALSE;

	if (m_pProcedureParamList)
	{
		for (int i = 0; i <= m_pProcedureParamList->GetUpperBound(); i++)
			if (
					m_pProcedureParamList->GetAt(i) && 
					!m_pProcedureParamList->GetAt(i)->IsEmpty()
				)
				return FALSE;
	}
	
	return TRUE;
}

//-----------------------------------------------------------------------------
BOOL SqlRecordProcedure::IsEqual(const SqlRecordProcedure& record) const
{
    if (
			!SqlRecord::IsEqual(*(SqlRecord*)&record) ||
			(m_pProcedureParamList && !record.m_pProcedureParamList) ||
			(!m_pProcedureParamList && record.m_pProcedureParamList) ||
			(
				m_pProcedureParamList && record.m_pProcedureParamList &&
				m_pProcedureParamList->GetSize() ==	record.m_pProcedureParamList->GetSize()
			)
		)
		return FALSE;

	for (int i = 0; i <= m_pProcedureParamList->GetUpperBound(); i++)
		if 	(*(m_pProcedureParamList->GetAt(i)) != *(record.m_pProcedureParamList->GetAt(i)))
				return FALSE;

	return TRUE;
}

//-----------------------------------------------------------------------------
void  SqlRecord::SetNamespace (const CTBNamespace& aNamespace)
{
	if (m_pTableInfo)
		m_pTableInfo->SetNamespace(aNamespace);
}


//////////////////////////////////////////////////////////////////////////////
//					SqlRecordDynamic Implementation
//////////////////////////////////////////////////////////////////////////////
//
IMPLEMENT_DYNCREATE(SqlRecordDynamic, SqlRecord) 

//-----------------------------------------------------------------------------
SqlRecordDynamic::SqlRecordDynamic()
:
	SqlRecord()
{
	SetValid (TRUE); //NON ha il TableInfo
	//m_nType = QUERY_TYPE;

	BindRecord ();
}

//-----------------------------------------------------------------------------
void SqlRecordDynamic::BindRecord()
{
}

//-----------------------------------------------------------------------------
void SqlRecordDynamic::SetConnection(SqlConnection* pConnection)
{
	if (!pConnection)
	{
		ASSERT(FALSE);
		return;
	}

	if (m_pSqlConnection && pConnection && m_pSqlConnection == pConnection)
		return; 

	m_pSqlConnection = pConnection;
	//m_pTableInfo  = NULL;

	Clear(TRUE); // inizializza lo stato del record rendendolo valido
}

//-----------------------------------------------------------------------------
SqlRecord* SqlRecordDynamic::Create () const
{
	SqlRecordDynamic* pRec = (SqlRecordDynamic*) __super::Create();
	ASSERT(pRec->GetSize() == 0);

	int sz = GetSizeEx();
	for (int i = 0; i < sz; i++)
	{
		SqlRecordItem* pItem = GetAt(i);
		SqlRecordItem* pNewItem = new SqlRecordItem(*pItem);
		pRec->Add(pNewItem);
	}
	return pRec;
}

IMPLEMENT_DYNAMIC(UnregisteredSqlRecord, SqlRecord)
//-----------------------------------------------------------------------------
UnregisteredSqlRecord::UnregisteredSqlRecord(LPCTSTR szTableName)
	: SqlRecord(szTableName)
{
	CreateDynamicDeclarations ();
}

//-----------------------------------------------------------------------------
SqlRecord*	UnregisteredSqlRecord::Create() const
{
	ASSERT_VALID(m_pTableInfo);
	ASSERT_VALID(m_pTableInfo->GetSqlCatalogEntry());

	// devo assegnare subito il table name per poter ottenere una 
	// SetConnection che agganci il puntatore al m_pTableInfo corretto
	UnregisteredSqlRecord* pRec = new UnregisteredSqlRecord(m_strTableName);
	ASSERT_VALID(pRec);

	OnCreating(pRec);

	pRec->SetConnection(GetConnection());

	if (m_arExtensions)
	{
		pRec->m_arExtensions = new RecordArray();

		for (int j = 0; j < m_arExtensions->GetSize(); j++)
		{
			SqlRecord* pSubRec = m_arExtensions->GetAt(j)->Create();
			pRec->m_arExtensions->Add(pSubRec);
		}
	}

	OnCreated(pRec);
	return pRec;

}

//-----------------------------------------------------------------------------
DataType UnregisteredSqlRecord::GetFirstCompatibleDataType(SqlColumnInfo* pSqlColumnInfo)
{
	if (pSqlColumnInfo->m_DataObjType != DATA_NULL_TYPE && CheckTypeCompatibility(pSqlColumnInfo->m_DataObjType, pSqlColumnInfo->m_nSqlDataType))
		return pSqlColumnInfo->m_DataObjType;
	
	CWordArray dataTypes;
   	if (!pSqlColumnInfo->GetDataObjTypes(dataTypes))
		return DATA_NULL_TYPE;
   	for (int i = 0; i < dataTypes.GetCount(); i++)
	{
		DataType dataType = dataTypes[i];
		if ((CheckTypeCompatibility(dataType, pSqlColumnInfo->m_nSqlDataType)))
		{
			//se ho una lunghezza molto lunga, non posso che essere un Text
			if ((pSqlColumnInfo->m_nSqlDataType == DBTYPE_WSTR || pSqlColumnInfo->m_nSqlDataType == DBTYPE_STR)
				&&
				pSqlColumnInfo->m_lLength > 1073741823 /*massimo del varchar*/
				&& 
				dataType != DATA_TXT_TYPE
				)
					continue;
			
			return dataType;
		}
	}
	return DATA_NULL_TYPE;
}
//-----------------------------------------------------------------------------
void UnregisteredSqlRecord::CreateDynamicDeclarations ()
{
	m_bBindingDynamically = true;

	int nPos = GetSize();
	const  Array* pColums = m_pTableInfo->GetPhysicalColumns();
	for (int i = 0; i < pColums->GetSize(); i++)
	{
		SqlColumnInfo* pSqlColumnInfo = (SqlColumnInfo*)pColums->GetAt(i);
		ASSERT (pSqlColumnInfo);
		ASSERT (!pSqlColumnInfo->m_bVirtual);
		
		//campo già bindato, lo salto
		SqlRecordItem* pRecItem = GetItemByColumnName(pSqlColumnInfo->GetColumnName());
		if (pRecItem)
			continue;

		DataType dataType = GetFirstCompatibleDataType(pSqlColumnInfo);
		if (dataType == DATA_NULL_TYPE)
			continue;

		DataObj *pDataObj = DataObj::DataObjCreate(dataType);
		if (!pDataObj)
			continue;
		BindDataObj(nPos++, pSqlColumnInfo->GetColumnName(), *pDataObj);
	}

	// mandatory fields are always of sqlrecord class and
	// they are never bound as dynamic (see destructor of SqlBindItem)
	m_bBindingDynamically = false;
}

//////////////////////////////////////////////////////////////////////////////
//					RecordArray Implementation
//////////////////////////////////////////////////////////////////////////////
//

IMPLEMENT_DYNAMIC(RecordArray, Array)

//-----------------------------------------------------------------------------
RecordArray::RecordArray() 
	: 
	m_sfs(sfsNone), 
	m_pFreeList(NULL),
	m_bObservablesEnabled(TRUE)
{
}

//-----------------------------------------------------------------------------
void RecordArray::EnableObservables(BOOL bValue /*TRUE*/)
{
	m_bObservablesEnabled = bValue;
}

//-----------------------------------------------------------------------------
BOOL RecordArray::SetOrderIndex()
{
	ASSERT(GetSize() && GetAt(0));
	if (GetSize() == 0 || GetAt(0) == 0) return FALSE;

	TCHAR* nextToken;
	TCHAR* szSortedColumns = new TCHAR [m_strOrderBy.GetLength() + 1];
	TB_TCSCPY(szSortedColumns, (LPCTSTR) m_strOrderBy);

	TCHAR* pszCol = _tcstok_s(szSortedColumns,_T(","), &nextToken);
	while(pszCol)
	{
		CString str(pszCol);
		str.TrimLeft(); str.TrimRight(); // str.MakeUpper(); Per allineamento changeset 209063 - Assert su addlink con case differente
		int nPos = str.Find(_T(" DESC"));
		if (nPos > 0)
			str.Truncate(nPos);

		int nDataIdxCol = GetAt(0)->GetIndexFromColumnName(str);
		if (nDataIdxCol < 0)
		{
			ASSERT(FALSE);
			continue;
		}

		m_arSortedColumIndex.Add(nDataIdxCol);
		m_arAscSort.Add(nPos == -1);

		pszCol = _tcstok_s(NULL, _T(","), &nextToken);
	}
	delete [] szSortedColumns;

	m_sfs = sfsOk;
	return TRUE;
}

//-----------------------------------------------------------------------------
BOOL RecordArray::SetOrderBy (LPCTSTR szOrderBy)
{
	m_strOrderBy			= szOrderBy;
	m_sfs					= szOrderBy == NULL || *szOrderBy == '\0' ? sfsNone : sfsTodo;
	m_arSortedColumIndex	.RemoveAll();
	m_arAscSort				.RemoveAll();
	m_arFormatCtrls			.RemoveAll();

	return GetSize() ? SetOrderIndex() : TRUE;
}

//--------------------------------------------------------------------------
/*virtual*/ int	RecordArray::Compare (CObject* po1, CObject* po2) const
{
	if (m_sfs == sfsNone)
		return Array::Compare(po1,po2);
	if (m_sfs == sfsTodo && ! const_cast<RecordArray*>(this)->SetOrderIndex() )
	{			
		ASSERT(FALSE);
		return 0;
	}
	SqlRecord* pR1 = (SqlRecord*) po1;
	SqlRecord* pR2 = (SqlRecord*) po2;

	for (int i=0; i < m_arSortedColumIndex.GetSize(); i++)
	{
		int idx = m_arSortedColumIndex.GetAt(i);

		DataObj* pf1 = pR1->GetDataObjAt(idx);
		DataObj* pf2 = pR2->GetDataObjAt(idx);

		if (*pf1 == *pf2) continue;

		BOOL b;// = FALSE;

		CParsedCtrl* pCtrl = m_arFormatCtrls.GetSize() ? m_arFormatCtrls.GetAt(i) : NULL;
		if (pCtrl)
		{ 
			ASSERT_VALID(pCtrl->GetCtrlCWnd());
			ASSERT(pCtrl->GetDataType() == pf1->GetDataType());

			CString s1, s2;

			if (pCtrl->m_nUseComponentToFormat > -1)
			{
				pf1 = pR1->GetDataObjAt(pCtrl->m_nUseComponentToFormat); 
				ASSERT_VALID(pf1);
				pf2 = pR2->GetDataObjAt(pCtrl->m_nUseComponentToFormat);
				ASSERT_VALID(pf2);

				s1 = pf1->Str();
				s2 = pf2->Str();
				
				if (pCtrl->GetCtrlCWnd()->GetExStyle() & WS_EX_RIGHT)
				{
					int len = max(s1.GetLength(), s2.GetLength());
					s1 = CString(pCtrl->m_cPadChar, len - s1.GetLength()) + s1;
					s2 = CString(pCtrl->m_cPadChar, len - s2.GetLength()) + s2;
				}
			}
			else
			{ 
				s1 = pCtrl->FormatData(pf1);
				s2 = pCtrl->FormatData(pf2);
			}

			b = s1.CompareNoCase(s2) < 0;
		}
		else if (pf1->IsKindOf(RUNTIME_CLASS(DataStr)))
		{
			b = ((DataStr*)pf1)->GetString().CompareNoCase(((DataStr*)pf2)->GetString()) < 0;
		}
		else
		{
			b = (*pf1 < *pf2);
		}

		if (m_arAscSort[i])
			return b ? -1 : 1;
		else
			return b ? 1 : -1;
	}
	return 0;
}

//----------------------------------------------------------------------------
void RecordArray::RemoveAll()
{
	if (m_pFreeList && m_bOwnsElements)
	{
		int n = GetSize();
		CObject* pO;
		for (int i = 0; i < n; i++) 
			if (pO = GetAt(i)) 
			{
				ASSERT_VALID(pO);

				m_pFreeList->Add(pO);
				CObArray::RemoveAll();//salto volontariamente la superclasse Array 
			}
	}
	else
		__super::RemoveAll();
}

//----------------------------------------------------------------------------
void RecordArray::RemoveAt(int nIndex, int nCount)
{
	if (m_pFreeList && m_bOwnsElements)
	{
		int n = GetSize();
		int j = nCount;
		CObject* pO;
		for (int i = nIndex; (i < n) && (j-- > 0); i++)
			if (pO = GetAt(i))
			{
				ASSERT_VALID(pO);

				m_pFreeList->Add(pO);
				CObArray::RemoveAt(nIndex, nCount);//salto volontariamente la superclasse Array 
			}

	}
	else
		__super::RemoveAt(nIndex, nCount);
}

//----------------------------------------------------------------------------
SqlRecord* RecordArray::RemoveLast()
{
	int idx = GetSize();
	if (!idx)
		return NULL;

	SqlRecord* pRec = GetAt(idx - 1);
	CObArray::RemoveAt(idx - 1);	//salto volontariamente la superclasse Array
	return pRec;
}

//----------------------------------------------------------------------------
BOOL RecordArray::Parse (CRuntimeClass* pClassRec, const CString& fileName, BOOL bWithAttributes/* = TRUE*/, BOOL bParseLocal/* = FALSE*/)
{
	CXMLDocumentObject xml;
	if (!xml.LoadXMLFile(fileName))
		return FALSE;

	CXMLNode* pRootNode = xml.GetRoot();
	if (!pRootNode || !pRootNode->GetChilds())
		return FALSE;

	return Parse (pClassRec, pRootNode, bWithAttributes, bParseLocal);
}

BOOL RecordArray::Parse (CRuntimeClass* pClassRec, CXMLNode* pParent, BOOL bWithAttributes/* = TRUE*/, BOOL bParseLocal /*= FALSE*/)
{
	ASSERT(pClassRec);
	ASSERT_VALID(pParent);
	for (int i=0; i <= pParent->GetChilds()->GetUpperBound(); i++)
	{
		CXMLNode* pParamNode = pParent->GetChilds()->GetAt(i);
		ASSERT_VALID(pParamNode);

		SqlRecord* pRec = (SqlRecord*) pClassRec->CreateObject();
		ASSERT_VALID(pRec);

		pRec->Parse(pParamNode, bWithAttributes, bParseLocal);

		Add(pRec);
	}
	return TRUE;
}

//----------------------------------------------------------------------------
BOOL RecordArray::UnParse (CXMLNode* pParent, BOOL bWithAttributes/* = TRUE*/, BOOL bUnParseLocal /*= FALSE*/, BOOL bSoapType /*= TRUE*/)
{
	ASSERT_VALID(pParent);
	for (int i = 0; i <= GetUpperBound(); i++)
	{
		SqlRecord* pRec = GetAt(i);
		ASSERT_VALID(pRec);

		CXMLNode* pNodeChild = pParent->CreateNewChild(pRec->GetTableName());	

		pRec->UnParse(pNodeChild, bWithAttributes, bUnParseLocal, bSoapType);
	}
	return TRUE;
}

//----------------------------------------------------------------------------
SqlRecord* RecordArray::FindRecordByTableName(const CString& sTableName)
{
	for (int i = 0; i <= GetUpperBound(); i++)
	{
		SqlRecord* pRec = GetAt(i);
		ASSERT_VALID(pRec);

		if (sTableName.CompareNoCase(pRec->GetTableName()) == 0)
			return pRec;
	}
	return NULL;
}

//-----------------------------------------------------------------------------	
int RecordArray::FindRecordIndex (const CString& sColumnName, const DataObj* pValue, int nStartPos/* = 0*/) const
{
	if (GetSize() == 0) 
		return -1;

	SqlRecord* pRec = GetAt(0);
	if (pRec == NULL) 
		return -1;

	int nFieldIndex = pRec->GetIndexFromColumnName(sColumnName);
	if (nFieldIndex < 0)
	{
		ASSERT(FALSE);
		return -1;
	}

	for (int i = nStartPos; i < GetSize(); i++)
	{
		pRec = GetAt(i);
		if (*const_cast<DataObj*>(pValue) == *pRec->GetDataObjAt(nFieldIndex))
			return i;
	}
	return -1;
}

//-----------------------------------------------------------------------------	
SqlRecord* RecordArray::FindRecord (const CString& sColumnName, DataObj* pValue, int nStartPos/* = 0*/)
{
	int idx = FindRecordIndex (sColumnName, pValue, nStartPos);
	if (idx < 0) 
		return NULL;

	return GetAt(idx);
}

//-----------------------------------------------------------------------------	
SqlRecord*  RecordArray::FindRecord (const CStringArray& arColumnName, const DataObjArray& arFilterValue, int nStartPos /*= 0*/)
{
	if (GetSize() == 0) 
		return NULL;

	SqlRecord* pRec = GetAt(0);
	if (pRec == NULL) 
		return NULL;

	int n = arColumnName.GetSize();
	if (n == 0) 
		return NULL;

	if (n != arFilterValue.GetSize()) 
		return NULL;

	CArray<int, int> arFieldIndex;
	for (int c = 0 ; c < n; c++)
	{
		int nFieldIndex = pRec->GetIndexFromColumnName(arColumnName[c]);
		if (nFieldIndex < 0)
		{
			ASSERT(FALSE);
			return FALSE;
		}
		arFieldIndex.Add(nFieldIndex);
	}

	for (int i = nStartPos; i < GetSize(); i++)
	{
		pRec = GetAt(i);

		BOOL bEq = TRUE;
		for (int j = 0; j < n; j++)
		{
			if (*arFilterValue[j] != *pRec->GetDataObjAt(arFieldIndex[j]))
			{
				bEq = FALSE;
				break;
			}
		}
		if (bEq)
			return pRec;
	}
	return NULL;
}

//-----------------------------------------------------------------------------
template <class T> void RecordArray::CalcSum(int nIndex, DataObj& pValue) const
{
	T& aVal = (T&)pValue;

	aVal.Clear(); //aVal = 0;
	for (int i = 0; i < GetSize(); i++)
	{
		SqlRecord* pRec = GetAt(i);

		aVal += *(T*) pRec->GetDataObjAt(nIndex);
	}
}

BOOL RecordArray::CalcSum(int nIdx, DataObj& aValue) const
{
	if (!m_bObservablesEnabled)
		return TRUE;

	DataType dt = aValue.GetDataType();

	if (dt == DataType::Money)
	{
		CalcSum<DataMon>(nIdx, aValue);
		return TRUE;
	}
	if (dt == DataType::Quantity)
	{
		CalcSum<DataQty>(nIdx, aValue);
		return TRUE;
	}
	if (dt == DataType::Percent)
	{
		CalcSum<DataPerc>(nIdx, aValue);
		return TRUE;
	}
	if (dt == DataType::Double)
	{
		CalcSum<DataDbl>(nIdx, aValue);
		return TRUE;
	}
	if (dt == DataType::ElapsedTime)
	{
		CalcSum<DataLng>(nIdx, aValue);
		return TRUE;
	}
	if (dt == DataType::Integer)
	{
		CalcSum<DataInt>(nIdx, aValue);
		return TRUE;
	}
	if (dt == DataType::Long)
	{
		CalcSum<DataLng>(nIdx, aValue);
		return TRUE;
	}

	ASSERT_TRACE1(FALSE, "Function RecordArray::CalcSum not implemented on type %s\n", dt.ToString());
	return FALSE;
}

BOOL RecordArray::CalcSum(const CString& sColumnName, DataObj& aValue) const
{
	if (!m_bObservablesEnabled)
		return TRUE;

	if (!GetSize())
		return FALSE;

	int nIdx = GetAt(0)->GetIndexFromColumnName(sColumnName);
	if (nIdx < 0)
	{
		ASSERT_TRACE1(FALSE, "function CalcSum called with wrong column name: %s.\n", sColumnName);
		return FALSE;
	}

	return CalcSum(nIdx, aValue);
}

//-----------------------------------------------------------------------------
DataObj* RecordArray::GetMaxElem(const CString& sColumnName) const
{
	if (!GetSize())
		return NULL;

	int nIndex = GetAt(0)->GetIndexFromColumnName(sColumnName);
	if (nIndex < 0)
	{
		ASSERT_TRACE1(FALSE, "function GetMaxElem called with wrong column name: %s.\n", sColumnName);
		return NULL;
	}

	return GetMaxElem(nIndex);
}

DataObj* RecordArray::GetMaxElem(int nIndex) const
{
	if (GetSize() == 0)
		return NULL; 
	
	DataObj* pMax = GetAt(0)->GetDataObjAt(nIndex);
	ASSERT_VALID(pMax);
	if (!pMax) return NULL;

	for (int i = 1; i < GetSize(); i++)
	{
		DataObj* pE = GetAt(i)->GetDataObjAt(nIndex);
		if (pMax < pE)
			pMax = pE;
	}
	return pMax;
}

//-----------------------------------------------------------------------------
DataObj* RecordArray::GetMinElem(const CString& sColumnName) const
{
	if (!GetSize())
		return NULL;

	int nIndex = GetAt(0)->GetIndexFromColumnName(sColumnName);
	if (nIndex < 0)
	{
		ASSERT_TRACE1(FALSE, "function GetMaxElem called with wrong column name: %s.\n", sColumnName);
		return NULL;
	}

	return GetMinElem(nIndex);
}

DataObj* RecordArray::GetMinElem(int nIndex) const
{
	if (GetSize() == 0)
		return NULL; 
	
	DataObj* pMin = GetAt(0)->GetDataObjAt(nIndex);
	ASSERT_VALID(pMin);
	if (!pMin) return NULL;

	for (int i = 1; i < GetSize(); i++)
	{
		DataObj* pE = GetAt(i)->GetDataObjAt(nIndex);
		if (pMin > pE)
			pMin = pE;
	}
	return pMin;
}

//=============================================================================

//-----------------------------------------------------------------------------
SqlRecordLocals::SqlRecordLocals(const CString& strTableName)
{
	m_strTableName = strTableName;
}


//-----------------------------------------------------------------------------
SqlRecordLocals::~SqlRecordLocals(void)
{
}

//-----------------------------------------------------------------------------
SqlRecord* SqlRecordLocals::Create	() const
{
	SqlRecordLocals* pRec = new SqlRecordLocals(m_strTableName);
	for (int i = 0; i < m_ColumnInfos.GetSize(); i++)
	{
		SqlColumnInfo* pInfo = (SqlColumnInfo*)m_ColumnInfos.GetAt(i);
		ASSERT_VALID(pInfo);

		SqlRecordItem* pItem = NULL;
		if (pInfo->m_RuntimeClass)
		{
			CObject* cObj = pInfo->m_RuntimeClass->CreateObject();
			ASSERT(cObj->IsKindOf(RUNTIME_CLASS(DataObj)));
			pItem = new SqlRecordItem((DataObj*)cObj, pInfo->m_strColumnName, pInfo, TRUE);
		}
		else
			pItem = new SqlRecordItem(::DataObj::DataObjCreate(pInfo->m_DataObjType), pInfo->m_strColumnName, pInfo, TRUE);

		pRec->Add(pItem);
	}
	return pRec;
}

//-----------------------------------------------------------------------------
SqlRecordItem* SqlRecordLocals::AddLocalField(::DataObj* pDataObj, const CString& strName, int lLength /*= 0*/)
{
	SqlColumnInfo* pInfo = new SqlColumnInfo(m_strTableName, strName, *pDataObj);
	pInfo->m_bVirtual = true;
	pInfo->m_lLength = lLength;
	pInfo->m_RuntimeClass = pDataObj->GetRuntimeClass();
	pInfo->UpdateDataObjType(pDataObj);

	m_ColumnInfos.Add(pInfo);	//farà la delete

	SqlRecordItem* pItem = new SqlRecordItem(pDataObj, strName, pInfo, TRUE);
	pItem->m_lLength = pInfo->m_lLength;
	Add(pItem);
	return pItem;
}

//-----------------------------------------------------------------------------
SqlRecordItem* SqlRecordLocals::AddLocalField(::DataType aDataType, const CString& strName, int lLength /*= 0*/)
{
	::DataObj* pDataObj = ::DataObj::DataObjCreate(aDataType);

	return AddLocalField(pDataObj, strName, lLength);
}

//=========================================================================
IMPLEMENT_DYNCREATE(DynamicSqlRecord, SqlRecord)

//-----------------------------------------------------------------------------
DynamicSqlRecord::DynamicSqlRecord()
	:
	SqlRecord()
{

}

//-----------------------------------------------------------------------------
DynamicSqlRecord::DynamicSqlRecord (LPCTSTR szTableName, SqlConnection* pConn /*NULL*/, short nType /*TABLE_TYPE*/, bool bCreatedAsEmpty /*false*/)
	:
	SqlRecord(szTableName, pConn, nType, bCreatedAsEmpty)
{
	BindDynamic();
}

//-----------------------------------------------------------------------------
void DynamicSqlRecord::BindDynamic()
{
	int nPos = GetSize();
	EndBindData(nPos);
}

//-----------------------------------------------------------------------------
SqlRecord&	DynamicSqlRecord::operator = (const SqlRecord& record)
{
	SqlRecord::operator=(record);

	BindDynamic();

	SqlRecord::operator=(record);

	return *this;
}

//-----------------------------------------------------------------------------
void DynamicSqlRecord::OnCreated(SqlRecord* pRec) const
{
	__super::OnCreated(pRec);
	((DynamicSqlRecord*) pRec)->BindDynamic();
}
