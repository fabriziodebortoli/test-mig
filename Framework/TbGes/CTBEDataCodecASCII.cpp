#include "StdAfx.h"

#include <tbgeneric\enumstable.h>

#include <tbparser\parser.h>
#include <tboledb\sqlrec.h>

#include <tbges\dbt.h>
#include <tbges\extdoc.h>

#include "BodyEdit.h"

#include "CTBEDataCoDecASCII.h"

//-----------------------------------------------------------------------------
const TToken UserTokensTable [] = 
{
	{ (Token)T_CODEC_DOCUMENT      			, _T("TBE_Document") },
	{ (Token)T_CODEC_DBTMASTER				, _T("TBE_DBTMaster") },
	{ (Token)T_CODEC_DBTSLAVEBUFFERED		, _T("TBE_DBTSlaveBuffered") },
	{ (Token)T_CODEC_DBTSLAVE				, _T("TBE_DBTSlave") },
	{ (Token)T_CODEC_SQLRECORD				, _T("TBE_SqlRecord") },
	{ (Token)T_CODEC_SQLTABLE				, _T("TBE_SqlTable") },
	{ (Token)T_CODEC_FIELDS					, _T("TBE_Fields") },
	{ (Token)T_CODEC_VALUES					, _T("TBE_Values") },
	{ (Token)T_CODEC_EXTRA_INFO				, _T("TBE_ExtraInfo") },
	{ (Token)T_CODEC_DBT_ID					, _T("TBE_DBTID") },
	{ (Token)T_CODEC_CLASSINFO				, _T("TBE_ClassInfo") },
	{ (Token)T_CODEC_CLASS					, _T("TBE_Class") },
	//{ (Token)T_CODEC_SESSION				, _T("TBE_Session") },

	//terminatore per loop sui token
	{ T_NULL_TOKEN							, NULL}
};

//-----------------------------------------------------------------------------
IMPLEMENT_DYNAMIC(CTBEDataCoDecASCII, CTBEDataCoDec)
//-----------------------------------------------------------------------------
CTBEDataCoDecASCII::CTBEDataCoDecASCII(LPCTSTR szFName)
	:
	m_strFName		(szFName),
	m_cfLoaded		(0),
	m_nIndent		(0),
	m_pDocument		(NULL),
	m_pParser		(NULL),
	m_pUnparser		(NULL),
	m_pParsedDoc	(NULL),
	m_nDBTID		(0)
{
}

//-----------------------------------------------------------------------------
CTBEDataCoDecASCII::~CTBEDataCoDecASCII()
{
	//ASSERT(m_pDocument	== NULL);
	ASSERT(m_pParser	== NULL);
	ASSERT(m_pUnparser	== NULL);

	if (m_pParsedDoc)
		delete m_pParsedDoc;
}

//-----------------------------------------------------------------------------
BOOL CTBEDataCoDecASCII::Encode(CAbstractFormDoc* pDocument)
{
	NewDocument(pDocument);

	Close();

	return TRUE;
}

//-----------------------------------------------------------------------------
BOOL CTBEDataCoDecASCII::Encode(CBodyEdit* pBody)
{
	CWaitCursor		wait;

	RecordArray	ra;
	pBody->CopySelections(ra);

	NewDocument		(pBody->GetDocument());
		pBody->m_PasteOrDropTargetCell.gSession.GenerateGuid();
		AddSession		(pBody->m_PasteOrDropTargetCell.gSession);
		AddDBTMaster	(pBody->GetDBT()->GetDocument()->m_pDBTMaster);
		AddDBTSlaveBuff	(pBody->GetDBT(), &ra);
		AddCustomData	();
	Close			();

	return TRUE;
}

//-----------------------------------------------------------------------------
BOOL CTBEDataCoDecASCII::Save(CArchive&	ar)
{
	ASSERT(!m_strFName.IsEmpty());
	TRACE1("CTBEDataCoDecASCII::Save::clipboard data to file: %s\n", m_strFName);

	ar.WriteString(m_strFName);
	return TRUE;
}

//-----------------------------------------------------------------------------
BOOL CTBEDataCoDecASCII::NewDocument(CAbstractFormDoc* pDocument)
{
	GetClassesInfo()->AddClass(pDocument->GetRuntimeClass());

	m_pDocument	= pDocument;
	m_pUnparser	= new Unparser();
	m_pUnparser->Attach(UserTokensTable);
	m_nIndent	= 0;
	m_nDBTID	= 0;

	int	nRelease	= TBECODEC_RELEASE;

	BuildFileName();
	if (!m_pUnparser->Open(m_strFName))
	{
		delete m_pUnparser;
		m_pUnparser = NULL;
		m_pDocument	= NULL;
		return FALSE;
	}

	GetUnParser()->UnparseTag	(T_RELEASE,	FALSE);
	GetUnParser()->UnparseInt	(nRelease,	TRUE);
	GetUnParser()->UnparseTag	(T_BEGIN,	TRUE);

	IncIndent();

	UnParseIndent();
	GetUnParser()->UnparseUserTag	(T_CODEC_DOCUMENT, FALSE);
	GetUnParser()->UnparseString(CString(pDocument->GetRuntimeClass()->m_lpszClassName), TRUE);
	GetUnParser()->UnparseTag	(T_COMMA, FALSE);
	GetUnParser()->UnparseString(pDocument->GetNamespace().ToString(), TRUE);

	UnParseIndent();
	GetUnParser()->UnparseTag	(T_BEGIN, TRUE);

	return TRUE;
}

//-----------------------------------------------------------------------------
BOOL CTBEDataCoDecASCII::AddSession (CGuid g)	
{
	GetUnParser()->UnparseGUID (g.GetValue());
	return TRUE;
}
//-----------------------------------------------------------------------------
CGuid CTBEDataCoDecASCII::GetSession ()	
{
	return m_pParsedDoc ? m_pParsedDoc->GetSession() : NULL_GUID;
}

//-----------------------------------------------------------------------------
void CTBEDataCoDecASCII::Close()
{
	m_pDocument = NULL;
	if (GetUnParser())
	{
		UnParseIndent();
		GetUnParser()->UnparseTag	(T_END, TRUE);

		DecIndent();
		GetUnParser()->UnparseTag	(T_END,	TRUE);
		GetUnParser()->Close();
		delete m_pUnparser;
		m_pUnparser = NULL;

		// scrivo le class info
		UnParseClassesInfo();
	}
}

//-----------------------------------------------------------------------------
void CTBEDataCoDecASCII::AddDBTMaster(DBTMaster* pDbt)
{
	if (pDbt == NULL)
		return;
	GetClassesInfo()->AddClass(pDbt->GetRuntimeClass());
	ASSERT(GetUnParser());
	SqlRecord*	pRec = pDbt->GetRecord();

	IncIndent();
	UnParseIndent();

		GetUnParser()->UnparseUserTag	(T_CODEC_DBTMASTER, FALSE);
		GetUnParser()->UnparseString(CString(pDbt->GetRuntimeClass()->m_lpszClassName), FALSE);
		GetUnParser()->UnparseTag	(T_COMMA, FALSE);
		GetUnParser()->UnparseString(pDbt->GetNamespace().ToUnparsedString(), TRUE);

		GetUnParser()->UnparseUserTag	(T_CODEC_SQLRECORD, FALSE);
		GetUnParser()->UnparseString(CString(pRec->GetRuntimeClass()->m_lpszClassName), FALSE);

		GetUnParser()->UnparseUserTag	(T_CODEC_SQLTABLE, FALSE);
		GetUnParser()->UnparseString(pRec->GetTableName(), FALSE);

		GetUnParser()->UnparseUserTag	(T_CODEC_DBT_ID, FALSE);
		GetUnParser()->UnparseInt	(++m_nDBTID, TRUE);

		UnParseIndent();

		GetUnParser()->UnparseTag	(T_BEGIN,	TRUE);
		IncIndent();
			GetUnParser()->UnparseCrLf		();
			AddRecordFields	(pRec);
			GetUnParser()->UnparseCrLf	();
			AddRecord		(pRec);
		DecIndent();
		UnParseIndent();
		GetUnParser()->UnparseTag	(T_END,		TRUE);
		GetUnParser()->UnparseCrLf	();

	DecIndent();
}

//-----------------------------------------------------------------------------
void CTBEDataCoDecASCII::AddDBTSlave(DBTSlave*	pDbt)
{
	GetClassesInfo()->AddClass(pDbt->GetRuntimeClass());
	ASSERT(GetUnParser());
	SqlRecord*	pRec = pDbt->GetRecord();

	IncIndent();
	UnParseIndent();

		GetUnParser()->UnparseUserTag	(T_CODEC_DBTSLAVE, FALSE);
		GetUnParser()->UnparseString(CString(pDbt->GetRuntimeClass()->m_lpszClassName), FALSE);
		GetUnParser()->UnparseTag	(T_COMMA, FALSE);
		GetUnParser()->UnparseString(pDbt->GetNamespace().ToUnparsedString(), TRUE);

		GetUnParser()->UnparseUserTag	(T_CODEC_SQLRECORD, FALSE);
		GetUnParser()->UnparseString(CString(pRec->GetRuntimeClass()->m_lpszClassName), FALSE);

		GetUnParser()->UnparseUserTag	(T_CODEC_SQLTABLE, FALSE);
		GetUnParser()->UnparseString(pRec->GetTableName(), FALSE);

		GetUnParser()->UnparseUserTag	(T_CODEC_DBT_ID, FALSE);
		GetUnParser()->UnparseInt	(++m_nDBTID, TRUE);

		UnParseIndent();
		GetUnParser()->UnparseTag	(T_BEGIN,	TRUE);
		IncIndent();
			GetUnParser()->UnparseCrLf		();
			AddRecordFields	(pRec);
			GetUnParser()->UnparseCrLf	();
			AddRecord		(pRec);
		DecIndent();
		UnParseIndent();
		GetUnParser()->UnparseTag	(T_END,		TRUE);
		GetUnParser()->UnparseCrLf	();

	DecIndent();

}

//-----------------------------------------------------------------------------
void CTBEDataCoDecASCII::AddDBTSlaveBuff(DBTSlaveBuffered* pDbt, RecordArray* pSelections)
{
	GetClassesInfo()->AddClass(pDbt->GetRuntimeClass());
	ASSERT(GetUnParser());
	SqlRecord*	pRec = pDbt->GetRecord();

	IncIndent();
	UnParseIndent();

		GetUnParser()->UnparseUserTag	(T_CODEC_DBTSLAVEBUFFERED, FALSE);
		GetUnParser()->UnparseString(CString(pDbt->GetRuntimeClass()->m_lpszClassName), FALSE);
		GetUnParser()->UnparseTag	(T_COMMA, FALSE);
		GetUnParser()->UnparseString(pDbt->GetNamespace().ToUnparsedString(), TRUE);

		GetUnParser()->UnparseUserTag	(T_CODEC_SQLRECORD, FALSE);
		GetUnParser()->UnparseString(CString(pRec->GetRuntimeClass()->m_lpszClassName), FALSE);

		GetUnParser()->UnparseUserTag	(T_CODEC_SQLTABLE, FALSE);
		GetUnParser()->UnparseString(pRec->GetTableName(), FALSE);

		GetUnParser()->UnparseUserTag	(T_CODEC_DBT_ID, FALSE);
		GetUnParser()->UnparseInt	(++m_nDBTID, TRUE);

		UnParseIndent();

		GetUnParser()->UnparseTag	(T_BEGIN,	TRUE);
		IncIndent();

			GetUnParser()->UnparseCrLf		();
			AddRecordFields	(pRec);

			if (pSelections)
			{
				for (int c = 0; c <= pSelections->GetUpperBound(); c++)
				{
					GetUnParser()->UnparseCrLf		();
					AddRecord		(pSelections->GetAt(c));
				}
			}
			else
			{
				for (int c = 0; c <= pDbt->GetUpperBound(); c++)
				{
					GetUnParser()->UnparseCrLf		();
					AddRecord		(pDbt->GetRow(c));
				}
			}

		DecIndent();

		UnParseIndent();
		GetUnParser()->UnparseTag	(T_END,		TRUE);
		GetUnParser()->UnparseCrLf	();

	DecIndent();
}

//-----------------------------------------------------------------------------
void CTBEDataCoDecASCII::AddRecord(SqlRecord* pRecord)
{
	UnParseIndent();
	GetUnParser()->UnparseUserTag		(T_CODEC_VALUES,	TRUE);
	UnParseIndent();
	GetUnParser()->UnparseTag		(T_BEGIN,		TRUE);
	IncIndent();

		for (int nIdx = 0; nIdx <= pRecord->GetUpperBound(); nIdx++)
		{
			if (!SkipField(pRecord,nIdx)) // Serve per escludere alcuni campi (ad. es. stringhe troppo lunghe)
			{
				UnParseIndent();	// x allineamento
				UnParseTDTDataElement			(pRecord, 	nIdx);
				GetUnParser()->UnparseTag		(T_SEP);
			}
		}

		AddRecordExtraInfo(pRecord);

	DecIndent();

	UnParseIndent();
	GetUnParser()->UnparseTag		(T_END,		TRUE);
}

//-----------------------------------------------------------------------------
void CTBEDataCoDecASCII::AddRecordExtraInfo(SqlRecord* pRecord)
{
/*
	UnParseIndent();	GetUnParser()->UnparseTag(T_CODEC_EXTRA_INFO, TRUE);

	UnParseIndent();	GetUnParser()->UnparseTag(T_BEGIN, TRUE);
	UnParseIndent();	GetUnParser()->UnparseTag(T_END, TRUE);
*/
}

//-----------------------------------------------------------------------------
void CTBEDataCoDecASCII::AddRecordFields(SqlRecord*	pRecord)
{
	GetClassesInfo()->AddClass(pRecord->GetRuntimeClass());

	UnParseIndent();
	GetUnParser()->UnparseUserTag		(T_CODEC_FIELDS,	TRUE);
	UnParseIndent();
	GetUnParser()->UnparseTag		(T_BEGIN,		TRUE);
	IncIndent();
		for (int nIdx = 0; nIdx <= pRecord->GetUpperBound(); nIdx++)
		if (!SkipField(pRecord,nIdx))
		{
			const SqlColumnInfo*	pSqlColInfo = pRecord->GetColumnInfo(nIdx);
			CString strName (pSqlColInfo->GetColumnName()); strName.MakeUpper();
			Token	tk		= FromDataTypeToToken(pSqlColInfo->GetDataObjType());

			UnParseIndent();

			CString	tks = cwsprintf(tk);
			CString	filler(' ', 15 - tks.GetLength());

			GetUnParser()->UnparseTag	(tk,			FALSE);
			GetUnParser()->UnparseID	(filler,		FALSE);
			GetUnParser()->UnparseString(strName, 		TRUE);
		}
	DecIndent();

	UnParseIndent();
	GetUnParser()->UnparseTag		(T_END,		TRUE);
}

//-----------------------------------------------------------------------------
// vedi --- > void SqlDBLoader::UnParseTDTDataElement(Unparser& oFile, int nIdx, int& nCount)
void CTBEDataCoDecASCII::UnParseTDTDataElement(SqlRecord* pRecord, int nIdx)
{
	DataObj* pDataObj = pRecord->GetDataObjAt(nIdx);		//TODO GetDataObjAtEx

	switch (pDataObj->GetDataType().m_wType)
	{
		case DATA_STR_TYPE	:
		{
			DataStr s = *((DataStr*)pDataObj);
			// lunghezza del dato della colonna + gli apici + la virgola
			GetUnParser()->UnparseTDTString(s.GetString(), FALSE);
			break;
		}
		case DATA_TXT_TYPE	:
		{
			DataText s = *((DataText*)pDataObj);
			// lunghezza del dato della colonna + gli apici + la virgola
			GetUnParser()->UnparseCEdit(s.GetString(), FALSE);
			break;
		}
		case DATA_GUID_TYPE	:
		{
			DataGuid s = *((DataGuid*)pDataObj);
			// lunghezza del dato della colonna + gli apici + la virgola
			GetUnParser()->UnparseString(s.Str(), FALSE);
			break;
		}

		case DATA_INT_TYPE	:
		{
			DataInt i = *((DataInt*)pDataObj);
			GetUnParser()->UnparseInt(i, FALSE);
			break;
		}

		case DATA_LNG_TYPE	:
		{
			DataLng l = *((DataLng*)pDataObj);
			if (pDataObj->IsATime())	//@@ElapsedTime
				GetUnParser()->UnparseString(l.Str(), FALSE);
			else
				GetUnParser()->UnparseLong(l, FALSE);
			break;
		}

		case DATA_DBL_TYPE	:
		{
			DataDbl d = *((DataDbl*)pDataObj);
			GetUnParser()->UnparseDouble(d, FALSE);
			break;
		}

		case DATA_MON_TYPE	:
		{
			DataDbl m = *((DataDbl*)pDataObj);
			GetUnParser()->UnparseDouble(m, FALSE);
			break;
		}

		case DATA_QTA_TYPE	:
		{
			DataDbl q = *((DataDbl*)pDataObj);
			GetUnParser()->UnparseDouble(q, FALSE);
			break;
		}

		case DATA_PERC_TYPE	:
		{
			DataDbl p = *((DataDbl*)pDataObj);
			GetUnParser()->UnparseDouble(p, FALSE);
			break;
		}

		case DATA_BOOL_TYPE	:
		{
			DataBool b = *((DataBool*)pDataObj);
			GetUnParser()->UnparseBool(b, FALSE);
			break;
		}

		case DATA_ENUM_TYPE	:
		{
			DataEnum e = *((DataEnum*)pDataObj);
			DataType aType1 = pRecord->GetColumnInfo(nIdx)->m_DataObjType;
			DataType aType2 = e.GetDataType();

			// probabilmente c'e' un dato sbagliato nel tag (esempio ALTER TABLE ADD COLUMN
			// senza default e non inizializzate successivamente dal programmatore). Devo
			// parsare comunque qulcosa di sensato per poter successivamente ricaricare dal .TDT
			if (aType1.m_wTag != aType2.m_wTag)
				e.Assign(aType1.m_wTag, AfxGetEnumsTable()->GetEnumDefaultItemValue(aType1.m_wTag));

			GetUnParser()->UnparseEnum(e.GetValue(), FALSE);
			break;
		}

		case DATA_DATE_TYPE	:
		{
			DataDate &dDate = *((DataDate*)pDataObj);
			GetUnParser()->UnparseString(dDate.Str(-1, 0), FALSE);
			break;
		}
	}
}

//-----------------------------------------------------------------------------
void CTBEDataCoDecASCII::UnParseIndent()
{
	CString	filler('\t', m_nIndent);
	GetUnParser()->Write(filler, FALSE);
}

//-----------------------------------------------------------------------------
BOOL CTBEDataCoDecASCII::Load(CArchive&	ar,	CLIPFORMAT cfLoaded)
{
	ar.ReadString(m_strFName);

	return Load(m_strFName, cfLoaded);
}


//-----------------------------------------------------------------------------
void CTBEDataCoDecASCII::SetFileName(LPCTSTR szFName)
{
	// si puo' cambiare il nome solo se non ci sono operazioni in corso
	ASSERT(!m_pUnparser);
	ASSERT(!m_pParser);
	m_strFName = szFName;
}

//-----------------------------------------------------------------------------
void CTBEDataCoDecASCII::BuildFileName(BOOL bForce /* FALSE */)
{
	USES_CONVERSION;

	if (m_strFName.IsEmpty() || bForce)
	{
		CString	tmp_path;
		TCHAR* pBuf = tmp_path.GetBuffer(MAX_PATH);
		GetTempPath(MAX_PATH, pBuf);
		tmp_path.ReleaseBuffer();

		if	(
				tmp_path.GetLength() > 2  &&
				tmp_path.Right(1) != '\\' &&
				tmp_path.Right(1) != '/'
			)
		{
			tmp_path += '\\';
		}

		m_strFName.Format(_T("%s_%s.txt"), tmp_path, A2W(GetRuntimeClass()->m_lpszClassName));
	}
}

//-----------------------------------------------------------------------------
BOOL CTBEDataCoDecASCII::Load(LPCTSTR szFileName, CLIPFORMAT cfLoaded)
{
	if (m_pParsedDoc)
		delete m_pParsedDoc;

	SetFileName(szFileName);
	SetLoadedCF(cfLoaded);

	ASSERT(!m_strFName.IsEmpty());
	TRACE1("CTBEDataCoDecASCII::Load: clipboard data from file: %s\n", m_strFName);

	if (!ExistFile(m_strFName))
		return FALSE;

	if (!ParseClassesInfo())
		return FALSE;

	BOOL	retCode = FALSE;
	m_pParsedDoc	= new CTBEDataCoDecDocument();
	m_pParser		= new Parser(UserTokensTable);

	GetParser()->Attach(NULL);

	if (GetParser()->Open(m_strFName))
	{
		int	nRelease	= TBECODEC_RELEASE;
		if (!GetParser()->ParseTag (T_RELEASE) || !GetParser()->ParseInt (nRelease))
		{
			delete m_pParser;
			m_pParser = NULL;
 			return FALSE;
		}

		ASSERT(nRelease == TBECODEC_RELEASE);

		if (GetParser()->ParseTag(T_BEGIN))
		{
			// ora mi aspetto il tag document...
			if (GetParser()->LookAhead() == T_CODEC_DOCUMENT)
			{
				ParseDocument();
			}
		}

		GetParser()->ParseTag(T_END);
	}
	else
	{
		retCode = FALSE;
	}

	delete m_pParser;
	m_pParser = NULL;

	return retCode;
}

//-----------------------------------------------------------------------------
void CTBEDataCoDecASCII::ParseDocument()
{
	int	errCode = 0;
	CString document_name, document_ns;

	if	(
			!GetParser()->ParseUserTag(T_CODEC_DOCUMENT)	||
			!GetParser()->ParseString(document_name)
		)
			errCode = 100;

	if	(	GetParser()->Matched(T_COMMA) && 
			!GetParser()->ParseString	(document_ns)
		)
			errCode = 1001;


	if	(
			!GetParser()->ParseTag(T_BEGIN)
		)
			errCode = 1002;

	m_pParsedDoc->SetDocName(document_name);
	m_pParsedDoc->SetDocNamespace(document_ns);

	if (GetParser()->LookAhead() == T_UUID)
	{
		GUID g;
		GetParser()->ParseGUID(g);
		m_pParsedDoc->SetSession(g);
	}

	while((errCode == 0) && !GetParser()->Bad() && !GetParser()->LookAhead(T_EOF))
	{
		if (GetParser()->LookAhead() == T_CODEC_DBTMASTER)
		{
			errCode = ParseDBTMaster();
			continue;
		}

		if (GetParser()->LookAhead() == T_CODEC_DBTSLAVE)
		{
			errCode = ParseDBTSlave();
			continue;
		}

		if (GetParser()->LookAhead() == T_CODEC_DBTSLAVEBUFFERED)
		{
			errCode = ParseDBTSlaveBuffered();
			continue;
		}

		break;
	}

	if ((errCode == 0) && !GetParser()->ParseTag(T_END))
		errCode = 200;

#ifdef _DEBUG
	if (errCode != 0)
	{
		TRACE1("CTBEDataCoDecASCII::ParseDocument: error %d\n", errCode);
		ASSERT(FALSE);
	}
#endif
}

//-----------------------------------------------------------------------------
int	CTBEDataCoDecASCII::ParseDBTMaster()
{
	int	errCode = 0;

	CString		dbt_ns;
	CString		dbt_name;
	CString		rec_name;
	CString		tbl_name;
	int			dbt_id = 0;

	if	(
			!GetParser()->ParseUserTag	(T_CODEC_DBTMASTER) ||
			!GetParser()->ParseString	(dbt_name)
		)
	{
		errCode = 100;
	}

	if	(
			GetParser()->Matched(T_COMMA) && 
			!GetParser()->ParseString	(dbt_ns)
	)
		errCode = 100;

	if	(
			!GetParser()->ParseUserTag	(T_CODEC_SQLRECORD) ||
			!GetParser()->ParseString	(rec_name)			||
			!GetParser()->ParseUserTag	(T_CODEC_SQLTABLE)	||
			!GetParser()->ParseString	(tbl_name)			||
			!GetParser()->ParseUserTag	(T_CODEC_DBT_ID)	||
			!GetParser()->ParseInt		(dbt_id)
		)
	{
		errCode = 100;
	}

/*
	TRACE1("Reading (%d) ", dbt_id);
	TRACE0("DBTMaster-> ");
	TRACE3("%s  SqlRec -> %s  Table -> %s\n",
		dbt_name,
		rec_name,
		tbl_name
	);
*/
	CTBEDataCoDecDBT*	pDBTMaster = m_pParsedDoc->AddDBTMaster(dbt_name, dbt_ns, rec_name, tbl_name);
	pDBTMaster->SetDBTID(dbt_id);

	if ((errCode == 0) && !GetParser()->ParseTag(T_BEGIN))
		errCode = 113;

	// leggo il record prototipo
	CTBEDataCoDecRecord*		pRec	= pDBTMaster->AddRecord();
	if (!pRec)
		errCode = 114;

	CStringArray			fld_order;

	if (errCode == 0)
		errCode = ParseFields(fld_order, *pRec);

	if (errCode == 0)
		errCode = ParseRecord(fld_order, *pRec);

	if ((errCode == 0) && !GetParser()->ParseTag(T_END))
		errCode = 115;

	return errCode;
}

//-----------------------------------------------------------------------------
int	CTBEDataCoDecASCII::ParseDBTSlave()
{
	int	errCode = 0;
	CString		dbt_ns;
	CString		dbt_name;
	CString		rec_name;
	CString		tbl_name;
	int			dbt_id = 0;

	if	(
			!GetParser()->ParseUserTag	(T_CODEC_DBTSLAVE) ||
			!GetParser()->ParseString	(dbt_name)
		)
	{
		errCode = 210;
	}

	if	(
			GetParser()->Matched(T_COMMA) && 
			!GetParser()->ParseString	(dbt_ns)
	)
		errCode = 210;

	if	(
			!GetParser()->ParseUserTag	(T_CODEC_SQLRECORD) ||
			!GetParser()->ParseString	(rec_name)			||
			!GetParser()->ParseUserTag	(T_CODEC_SQLTABLE)	||
			!GetParser()->ParseString	(tbl_name)			||
			!GetParser()->ParseUserTag	(T_CODEC_DBT_ID)	||
			!GetParser()->ParseInt		(dbt_id)
		)
	{
		errCode = 210;
	}

/*
	TRACE1("Reading (%d) ", dbt_id);
	TRACE0("DBTSlave-> ");
	TRACE3("%s  SqlRec -> %s  Table -> %s\n",
		dbt_name,
		rec_name,
		tbl_name
	);
*/
	CTBEDataCoDecDBT*	pDBTSlave= m_pParsedDoc->AddDBTSlave(dbt_name, dbt_ns, rec_name, tbl_name);
	pDBTSlave->SetDBTID(dbt_id);

	if ((errCode == 0) && !GetParser()->ParseTag(T_BEGIN))
		errCode = 213;

	// leggo il record prototipo
	CTBEDataCoDecRecord*		pRec	= pDBTSlave->AddRecord();
	if (!pRec)
		errCode = 214;

	CStringArray			fld_order;

	if (errCode == 0)
		errCode = ParseFields(fld_order, *pRec);

	if (errCode == 0)
		errCode = ParseRecord(fld_order, *pRec);

	if ((errCode == 0) && !GetParser()->ParseTag(T_END))
		errCode = 215;
	return errCode;
}

//-----------------------------------------------------------------------------
int	CTBEDataCoDecASCII::ParseDBTSlaveBuffered(CTBEDataCoDecDBT** ppDBT)
{
	int	errCode = 0;
	CString		dbt_ns;
	CString		dbt_name;
	CString		rec_name;
	CString		tbl_name;
	int			dbt_id = 0;

	if	(
			!GetParser()->ParseUserTag	(T_CODEC_DBTSLAVEBUFFERED) ||
			!GetParser()->ParseString	(dbt_name)
		)
	{
		errCode = 210;
	}

	if	(
			GetParser()->Matched(T_COMMA) && 
			!GetParser()->ParseString	(dbt_ns)
	)
		errCode = 210;

	if	(
			!GetParser()->ParseUserTag	(T_CODEC_SQLRECORD) ||
			!GetParser()->ParseString	(rec_name)			||
			!GetParser()->ParseUserTag	(T_CODEC_SQLTABLE)	||
			!GetParser()->ParseString	(tbl_name)			||
			!GetParser()->ParseUserTag	(T_CODEC_DBT_ID)	||
			!GetParser()->ParseInt		(dbt_id)
		)
	{
		errCode = 210;
	}

/*
	TRACE1("Reading (%d) ", dbt_id);
	TRACE0("DBTSlaveBuffered-> ");
	TRACE3("%s-%s  SqlRec -> %s  Table -> %s\n",
		dbt_name, dbt_ns, 
		rec_name,
		tbl_name
	);
*/
	CTBEDataCoDecDBT*	pDBTSlaveBuff= m_pParsedDoc->AddDBTSlaveBuffered(dbt_name, dbt_ns, rec_name, tbl_name);
	pDBTSlaveBuff->SetDBTID(dbt_id);

	if (ppDBT)
		*ppDBT = pDBTSlaveBuff;

	if ((errCode == 0) && !GetParser()->ParseTag(T_BEGIN))
		errCode = 313;

	// leggo il record prototipo
	CTBEDataCoDecRecord		proto_rec;

	CStringArray			fld_order;

	if (errCode == 0)
		errCode = ParseFields(fld_order, proto_rec);

	while	(
				(errCode == 0) &&
				(GetParser()->LookAhead() == T_CODEC_VALUES)
			)
	{
		CTBEDataCoDecRecord*		pRec	= pDBTSlaveBuff->AddRecord();
		if (!pRec)
		{
			errCode = 314;
			break;
		}

		// copia la struttura
		*pRec = proto_rec;

		errCode = ParseRecord(fld_order, *pRec);
	}


	if ((errCode == 0) && !GetParser()->ParseTag(T_END))
		errCode = 315;
	return errCode;
}

//-----------------------------------------------------------------------------
int	CTBEDataCoDecASCII::ParseFields(CStringArray& fld_order, CTBEDataCoDecRecord& rec)
{
	int	errCode = 0;

	if	(
			!GetParser()->ParseUserTag(T_CODEC_FIELDS) ||
			!GetParser()->ParseTag(T_BEGIN)
		)
		errCode = 1000;

	while	(
				(errCode == 0) &&
				(GetParser()->LookAhead() != T_END) &&
				(GetParser()->LookAhead() != T_EOF)
			)
	{
		// il fomato e' dato da TipoCampo  NomeCampo
		Token		tk = GetParser()->LookAhead();	GetParser()->SkipToken();
		CString		name;
		GetParser()->ParseString(name);

		// costruisco l'associazione
		DataType	dt = FromTokenToDataType(tk);
		DataObj*	pDataObj = DataObj::DataObjCreate(dt);

/*
#ifdef _DEBUG
		{
			CString	dbg;
			dbg = cwsprintf(tk);
			TRACE2("CTBEDataCoDecASCII::ParseFields-> %s ->%s\n", name, dbg);
		}
#endif
*/
		name.MakeUpper();
		fld_order.Add(name);
		rec.SetFieldValue(name, pDataObj);
		delete pDataObj;
	}

	if	(
			(errCode == 0) &&
			!GetParser()->ParseTag(T_END)
		)
	{
		errCode = 1001;
	}

	return errCode;
}

//-----------------------------------------------------------------------------
int	CTBEDataCoDecASCII::ParseRecord(CStringArray& fld_order, CTBEDataCoDecRecord& rec)
{
	int	errCode = 0;
	int	fld_count = 0;

	if	(
			!GetParser()->ParseUserTag(T_CODEC_VALUES) ||
			!GetParser()->ParseTag(T_BEGIN)
		)
		errCode = 2000;

	while	(
				(errCode == 0) &&
				(GetParser()->LookAhead() != T_END) &&
				(GetParser()->LookAhead() != T_EOF)
			)
	{
		if (GetParser()->LookAhead() == T_CODEC_EXTRA_INFO)
		{
			if (!ParseRecordExtraInfo(rec))
				errCode = 2005;
			continue;
		}

		CString	fld;
		if (fld_count <= fld_order.GetUpperBound())
		{
			fld = fld_order.GetAt(fld_count);
		}

		if (fld.IsEmpty())
		{
			errCode = 2001;
			break;
		}

		if (!ParseTDTDataElement(fld, rec))
			errCode = 2002;

		if (!GetParser()->Match(T_SEP))
			errCode = 2004;

		fld_count++;
	}

#ifdef _DEBUG
	if ((errCode == 0) && (fld_count != fld_order.GetSize()))
	{
		ASSERT(FALSE);
	}
#endif

	if	(
			(errCode == 0) &&
			!GetParser()->ParseTag(T_END)
		)
	{
		errCode = 2003;
	}

	return errCode;
}

//-----------------------------------------------------------------------------
BOOL CTBEDataCoDecASCII::ParseRecordExtraInfo(CTBEDataCoDecRecord& rec)
{
	// leggo fino al blocco END (la gestione dovrebbe essere reimplementata nelle derivate)
	BOOL bOk = TRUE;

	while (bOk && !GetParser()->LookAhead(T_END) && !GetParser()->LookAhead(T_EOF) )
		bOk = GetParser()->SkipToken() && !GetParser()->Bad() && !GetParser()->Eof();

	return (bOk && GetParser()->ParseTag(T_END));
}

//-----------------------------------------------------------------------------
BOOL CTBEDataCoDecASCII::ParseTDTDataElement(LPCTSTR fld_name, CTBEDataCoDecRecord& rec)
{
	DataObj*	pDataObj = rec.GetDataObj(fld_name);
	if (!pDataObj)
	{
		ASSERT(FALSE);
		return FALSE;
	}

	switch(pDataObj->GetDataType().m_wType)
	{
		case DATA_STR_TYPE	:
		{
			CString strCurr;
			if (!GetParser()->ParseTDTString(strCurr))
				return FALSE;
			*((DataStr*)pDataObj) = strCurr;
			break;
		}
		case DATA_TXT_TYPE	:
		{
			CString strCurr;
			if (!GetParser()->ParseCEdit(strCurr))
				return FALSE;
			((DataText*)pDataObj)->Assign(strCurr);
			break;
		}
		case DATA_GUID_TYPE	:
		{
			CString strCurr;
			if (!GetParser()->ParseString(strCurr))
				return FALSE;
			*((DataGuid*)pDataObj) = strCurr;
			break;
		}
		case DATA_INT_TYPE	:
		{
			int nCurr;
			if (!GetParser()->ParseSignedInt(nCurr))
				return FALSE;

			*((DataInt*)pDataObj) = nCurr;

			break;
		}
		case DATA_LNG_TYPE	:
		{
			if (pDataObj->IsATime())	//@@ElapsedTime
			{
				DWORD dwValue = 0;
				if (!GetParser()->ParseDateTimeString(T_TELAPSED_TIME, &dwValue))
					return FALSE;

				*((DataLng*)pDataObj) = (long)dwValue;
			}
			else
			{
				long lCurr;
				if (!GetParser()->ParseSignedLong(lCurr))
					return FALSE;

				*((DataLng*)pDataObj) = lCurr;
			}
			break;
		}
		case DATA_DBL_TYPE	:
		{
			double dCurr;
			if (!GetParser()->ParseSignedDouble(dCurr))
				return FALSE;
			*((DataDbl*)pDataObj) = dCurr;
			break;
		}
		case DATA_MON_TYPE	:
		{
			double dCurr;
			if (!GetParser()->ParseSignedDouble(dCurr))
				return FALSE;
			*((DataMon*)pDataObj) = dCurr;
			break;
		}
		case DATA_QTA_TYPE	:
		{
			double dCurr;
			if (!GetParser()->ParseSignedDouble(dCurr))
				return FALSE;
			*((DataQty*)pDataObj) = dCurr;
			break;
		}
		case DATA_PERC_TYPE	:
		{
			double dCurr;
			if (!GetParser()->ParseSignedDouble(dCurr))
				return FALSE;
			*((DataPerc*)pDataObj) = dCurr;
			break;
		}
		case DATA_BOOL_TYPE	:
		{
			BOOL bCurr;
			if (!GetParser()->ParseBool(bCurr))
				return FALSE;
			*((DataBool*)pDataObj) = bCurr;
			break;
		}
		case DATA_ENUM_TYPE	:
		{
			DWORD eCurr;
			if (GetParser()->ParseComplexData(&eCurr, NULL, TRUE) != DATA_ENUM_TYPE)
				return FALSE;
			*((DataEnum*)pDataObj) = eCurr;
			break;
		}
		case DATA_DATE_TYPE	:
		{
			DWORD dwValue1 = 0;
			DWORD dwValue2 = 0;

			DataDate	&dDate = *((DataDate*)pDataObj);
			if (!GetParser()->ParseDateTimeString(FromDataTypeToToken(dDate.GetDataType()), &dwValue1, &dwValue2))
				return FALSE;

			dDate.Assign((long)dwValue1, (long)dwValue2);
			break;
		}
	}

	return TRUE;
}

//-----------------------------------------------------------------------------
BOOL CTBEDataCoDecASCII::UnParseClassesInfo()
{
	USES_CONVERSION;

	if (!GetClassesInfo())
		return TRUE;

	// creo l'unparser x il secondo file...
	Unparser	unparser;
	unparser.Attach(UserTokensTable);
	CString		fname;
	fname.Format(_T("%s.cinfo"), m_strFName);

	if (!unparser.Open(fname))
		return FALSE;

	Unparser*	pOldUnparser = m_pUnparser;
	m_pUnparser = &unparser;

	int	nRelease = TBECODEC_RELEASE_CI;

	unparser.UnparseTag	(T_RELEASE,	FALSE);
	unparser.UnparseInt	(nRelease,	TRUE);
	unparser.UnparseCrLf();
	UnParseIndent();	unparser.UnparseUserTag(T_CODEC_CLASSINFO);

	UnParseIndent();	unparser.UnparseTag(T_BEGIN);
	IncIndent();
		CStringList	class_list;
		GetClassesInfo()->GetClassList(class_list);
		for (POSITION pos = class_list.GetHeadPosition(); pos; )
		{
			CString	cname = class_list.GetNext(pos);

			if (!cname.IsEmpty())
			{
				UnParseIndent();	unparser.UnparseUserTag	(T_CODEC_CLASS, FALSE);
									unparser.UnparseString(cname);

				UnParseIndent();	unparser.UnparseTag(T_BEGIN);
				IncIndent();

					CStringList*	pClassHierarchy = GetClassesInfo()->GetClassHierarchy(T2A((LPCTSTR)cname));
					if (pClassHierarchy)
					{
						for (POSITION pos_h = pClassHierarchy->GetHeadPosition(); pos_h; )
						{
							CString	parent_name = pClassHierarchy->GetNext(pos_h);
							if (!parent_name.IsEmpty())
							{
								UnParseIndent();
								unparser.UnparseString(parent_name);
							}
						}
					}

				DecIndent();
				UnParseIndent();	unparser.UnparseTag(T_END);
									unparser.UnparseCrLf();
			}
		}
	DecIndent();
	UnParseIndent();	unparser.UnparseTag(T_END);
						unparser.UnparseCrLf();
	m_pUnparser = NULL;
	return TRUE;
}

//-----------------------------------------------------------------------------
BOOL CTBEDataCoDecASCII::ParseClassesInfo()
{
	USES_CONVERSION;

	if (!GetClassesInfo())
		return TRUE;

	GetClassesInfo()->Reset();

	// creo il parser x il secondo file...
	Parser parser(UserTokensTable);
	//parser.Attach(NULL);

	CString		fname;
	fname.Format(_T("%s.cinfo"), m_strFName);

	if (!parser.Open(fname))
		return FALSE;

	int	nRelease = TBECODEC_RELEASE_CI;
	if	(
			!parser.ParseTag(T_RELEASE) ||
			!parser.ParseInt(nRelease) ||
			!parser.ParseUserTag(T_CODEC_CLASSINFO) ||
			!parser.ParseTag(T_BEGIN)
		)
		return FALSE;

	if (nRelease != TBECODEC_RELEASE_CI)
		return FALSE;

	while(!parser.LookAhead(T_END) && !parser.LookAhead(T_EOF))
	{
		CString	cname;
		if	(
				!parser.ParseUserTag	(T_CODEC_CLASS) ||
				!parser.ParseString	(cname)			||
				!parser.ParseTag	(T_BEGIN)
			)
		{
			return FALSE;
		}

		// carico la gerarchia di classi
		CStringList	lista;
		while(!parser.LookAhead(T_END) && !parser.LookAhead(T_EOF))
		{
			CString	parent;
			parser.ParseString(parent);
			lista.AddTail(parent);
		}

		if (parser.ParseTag(T_END))
		{
			GetClassesInfo()->SetClassHierarchy(T2A((LPCTSTR)cname), lista);
		}
	}

	if (!parser.ParseTag(T_END))
		return FALSE;

	return TRUE;
}


//-----------------------------------------------------------------------------
void CTBEDataCoDecASCII::AddCustomData()
{

}
