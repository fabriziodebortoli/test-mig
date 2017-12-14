#pragma once

#include <tbgeneric\dataobj.h>
#include <TbOledb\sqlrec.h>


//includere alla fine degli include del .H
#include "beginh.dex"

//=============================================================================

class	SqlRecord;
class	DataObj;
class	CAbstractFormDoc;
class	DBTMaster;
class	DBTSlave;
class	DBTSlaveBuffered;
class	RecordArray;
class	CTBEDataCoDecDBT;
class	CTBEDataCoDecClassInfo;
class	CBodyEdit;
class	CTBEDataCoDec;

#define TBE_CFVPARAMS(r, f) (LPCTSTR)r->GetColumnName(&(r->f)), &(r->f)

///////////////////////////////////////////////////////////////////////////////
// class CTBEDataCodecMap
//-----------------------------------------------------------------------------
class TB_EXPORT CTBEDataCodecMap : public CObject
{
	DECLARE_DYNAMIC(CTBEDataCodecMap)
protected:
	class	_Item : public CObject
	{
	protected:
		BOOL				m_bDelete;
		CTBEDataCoDec*		m_pCodec;
	public:
		_Item(CTBEDataCoDec*	pCodec, BOOL bDelete)
		{
			m_bDelete	= bDelete;
			m_pCodec	= pCodec;
		}

		virtual ~_Item();

		CTBEDataCoDec*	GetCoDec() const {return m_pCodec; }
	};

protected:
	CMap<CLIPFORMAT, CLIPFORMAT&, _Item*, _Item*&>	m_Map;

public:
	CTBEDataCodecMap();
	virtual ~CTBEDataCodecMap();


			void	Reset		();
	CTBEDataCoDec*	AddCodec	(CLIPFORMAT	cf, CTBEDataCoDec*	pCodec, BOOL bOwn);
	CTBEDataCoDec*	GetCodec	(CLIPFORMAT	cf);
};


///////////////////////////////////////////////////////////////////////////////
// class CTBEDataCodec
//-----------------------------------------------------------------------------
class TB_EXPORT CTBEDataCoDec : public CObject
{
	DECLARE_DYNAMIC(CTBEDataCoDec)
protected:
	CTBEDataCoDecClassInfo*			m_pClassesInfo;
	CUIntArray						m_AcceptDropFormats;

	// Mappa contenete i riferimenti alle istanze dei codec: un codec può gestire traduzioni tra formati
	CTBEDataCodecMap				m_AcceptFormatsMap;
	// Mappa associativa formato/codec (uno stesso codec potrebbe gestire più formati
	// allora è inutile instaziare più copie dello stesso codec)
	CMap<CLIPFORMAT, CLIPFORMAT&, CLIPFORMAT, CLIPFORMAT&>	m_AcceptFormatsAssociationMap;

public:
						CTBEDataCoDec		();
	virtual				~CTBEDataCoDec		();

	virtual	BOOL		Encode				(CAbstractFormDoc*	pDocument	)	= 0;
	virtual	BOOL		Encode				(CBodyEdit*	pBody		)	= 0;
	virtual	BOOL		Save				(CArchive&	ar)						= 0;
	virtual	CLIPFORMAT	GetClipFormat		()									= 0;
	virtual	BOOL		Load				(LPCTSTR szFileName,	CLIPFORMAT	cfLoaded = 0)	= 0;

	virtual	BOOL		NewDocument			(CAbstractFormDoc*	pDocument	)	= 0;
	virtual	void		Close				()									= 0;
	virtual	void		SetFileName			(LPCTSTR			szFName		)	= 0;
	// Imposta il CF caricato nel file (che può essere diverso da quello del codec
	// perché un codec può gestire più clipboard)
	virtual	void		SetLoadedCF			(CLIPFORMAT			cfLoaded	)	= 0;
	virtual	CLIPFORMAT	GetLoadedCF			()									= 0;

	virtual	void		AddDBTMaster		(DBTMaster*			pDbt)			= 0;
	virtual	void		AddDBTSlave			(DBTSlave*			pDbt)			= 0;
	virtual	void		AddDBTSlaveBuff		(DBTSlaveBuffered*	pDbt, RecordArray* pSelections = NULL) = 0;
	virtual	void		AddCustomData		()									= 0;

	virtual	void		AddAllDBTSlaves		(DBTMaster*			pDbt)			;
	virtual	void		AddAllDBTSlavesBuff	(DBTMaster*			pDbt)			;
	CTBEDataCoDecClassInfo*	GetClassesInfo	()const {return m_pClassesInfo;}

			void		AddDropFormat		(CLIPFORMAT cf);

			BOOL		AcceptDropFormat	(CLIPFORMAT cf);
			BOOL		AcceptDropFormat	(COleDataObject*	pDataObject);
			BOOL		AcceptDropFormat	(COleDataObject*	pDataObject, CLIPFORMAT *pRetVal, CTBEDataCoDec*& codecFound);

						// Al formato cf è associato il codec pCodec (è un modo per identificare il codec)
			void		AddPasteCodecFormat	(CLIPFORMAT cf, CTBEDataCoDec* pCodec, BOOL bOwn);
						// Il paste dal formato cfSource è gestito dal codec associato (tramite AddPasteCodecFormat)
						// al formato cfCodecTarget.
			void		SetCodecAssociation	(CLIPFORMAT cfSource, CLIPFORMAT cfCodecTarget);
			BOOL		FindClipBoardFormat(CTBEDataCoDec*& codecFound, CLIPFORMAT& cfFound);
			BOOL		FindClipBoardFormat();

			virtual	BOOL		AddSession			(CGuid&)	{ return TRUE; }
			virtual	CGuid		GetSession			()			{ return NULL_GUID; }

protected:
	virtual	void		AddRecord			(SqlRecord*			pRecord)	= 0;
	virtual	void		AddRecordExtraInfo	(SqlRecord*			pRecord)	= 0;
	virtual	BOOL		ParseClassesInfo	()								= 0;
	virtual	BOOL		UnParseClassesInfo	()								= 0;
};

///////////////////////////////////////////////////////////////////////////////
typedef CMap<CString, LPCTSTR, DataObj*, DataObj*> MapNameFields;

///////////////////////////////////////////////////////////////////////////////
class TB_EXPORT CTBEDataCoDecRecordToValidate
{
public:
	DBTSlaveBuffered*	m_pDBT;
	CBodyEdit*			m_pBody;
	CTBEDataCoDecDBT*	m_pDataCoDecDBT;

public:
	CTBEDataCoDecRecordToValidate (DBTSlaveBuffered* pDBT, CBodyEdit* pBody, CTBEDataCoDecDBT* pDataCoDecDBT) 
		:  
			m_pDBT			(pDBT),
			m_pBody			(pBody),
			m_pDataCoDecDBT	(pDataCoDecDBT)
		{}

	CString		GetSourceDBTClassName();
	CString		GetSourceDBTNamespace();
	CString		GetSourceSqlRecordClassName();
	CString		GetSourceTableName();
};

///////////////////////////////////////////////////////////////////////////////
class TB_EXPORT CTBEDataCoDecPastedRecord : public CTBEDataCoDecRecordToValidate
{
public:
	SqlRecord*				m_pTargetRecord;

	//Current row values map
	int						m_nr;
	MapNameFields*			GetFieldsMap();	//{return  m_pDataCoDecDBT->GetRecord(m_nr)->GetFieldsMap();}

	CStringArray			m_arMissingFields;
	CStringArray			m_arIncompatibleFields;

protected:
	//optional lookup fields name. It has to load before call PasteFieldsValue
	//by override method named CAbstractFormDoc::OnPasteDBTRows 
	CMapStringToString		m_mapNames;
	CStringArray			m_arSkipNames;	//colums name to skip on paste

public:
	CTBEDataCoDecPastedRecord 
		(
			DBTSlaveBuffered* pDBT, CBodyEdit* pBody, CTBEDataCoDecDBT* pDataCoDecDBT, 
			int nr, SqlRecord* pTargetRecord
		); 
	
	virtual ~CTBEDataCoDecPastedRecord () { /*SAFE_DELETE(m_pTargetRecord);*/ }

	void SetEntryFieldName(LPCTSTR key, LPCTSTR value) { CString sKey = key; sKey.MakeUpper(); m_mapNames.SetAt(sKey, value); }
	BOOL LookupFieldName(LPCTSTR key, CString& value) { CString sKey = key; sKey.MakeUpper(); return m_mapNames.Lookup(sKey, value); }

	virtual BOOL PasteFieldValue (LPCTSTR szFieldName, DataObj* pDataObj)
	{
		if (CStringArray_Find(m_arSkipNames, szFieldName) > -1)
			return TRUE;

		DataObj*	pData = NULL;
		CString		field = szFieldName; field.MakeUpper();

		BOOL bIsRenamed = m_mapNames.Lookup(field, field); //optional lookup fields name

		if (GetFieldsMap()->Lookup(field, pData))
		{
			if (DataType::IsCompatible(pData->GetDataType(), pDataObj->GetDataType()))
			{
				pDataObj->Assign(*pData);
				//return TRUE;
			}
			else
			{
				m_arIncompatibleFields.Add(szFieldName);
				//return FALSE;
			}
		}
		else
		{
			pDataObj->Clear();
			m_arMissingFields.Add(szFieldName);
			//return FALSE;
		}
		return TRUE;
	}

	virtual BOOL PasteFieldsValue()
	{
		//assign data by column name
		for (int col = 0; col <= m_pTargetRecord->GetUpperBound(); col++)
		{
			PasteFieldValue(m_pTargetRecord->GetColumnName(col), m_pTargetRecord->GetDataObjAt(col));
		}
		return TRUE;
	}
};


///////////////////////////////////////////////////////////////////////////////
//	CTBEDataCoDecRecord
//-----------------------------------------------------------------------------
class TB_EXPORT CTBEDataCoDecRecord : public CObject
{
	DECLARE_DYNAMIC(CTBEDataCoDecRecord)
protected:
	MapNameFields		m_Fields;
	CObject*			m_pData;
	BOOL				m_bOwnData;

	CTBEDataCoDecDBT*	m_pDBT;

public:
	CTBEDataCoDecRecord(CTBEDataCoDecDBT*	pDBT = NULL);
	virtual ~CTBEDataCoDecRecord();

	void		Init			();
	void		Reset			();
	void		SetFieldValue	(LPCTSTR	szFieldName, DataObj*	pDataObj);
	void		GetFieldValue	(LPCTSTR	szFieldName, DataObj*	pDataObj);
	BOOL		GetDataType		(LPCTSTR szFieldName, DataType&	dt);
	DataObj*	GetDataObj		(LPCTSTR szFieldName) const;

	CTBEDataCoDecRecord& operator = (const CTBEDataCoDecRecord& source);

	void				SetData(CObject*	pObject, BOOL bOwn);
	CObject*			GetData() const {return m_pData;}

	CTBEDataCoDecDBT*	GetDBT() const {return m_pDBT;}
	MapNameFields*		GetFieldsMap () { return &m_Fields; }

};

///////////////////////////////////////////////////////////////////////////////
//	CTBEDataCoDecRecordArray
//-----------------------------------------------------------------------------
class TB_EXPORT CTBEDataCoDecRecordArray : public TArray<CTBEDataCoDecRecord>
{
};

///////////////////////////////////////////////////////////////////////////////
//	CTBEDataCoDecDBT
//-----------------------------------------------------------------------------
class TB_EXPORT CTBEDataCoDecDBT : public CObject
{
	DECLARE_DYNAMIC(CTBEDataCoDecDBT)
public:
	enum	DBTType { MASTER, SLAVE, SLAVEBUFFERED };

protected:
	const	CString							m_TableName;
	const	CString							m_DBTName;
	const	CString							m_SqlRecordName;
	const	DBTType							m_DBTType;
	const	CString							m_DBTNamespace;

			CTBEDataCoDecRecordArray		m_Records;
			int								m_DBTID;

public:
	CTBEDataCoDecDBT				(DBTType	type, const CString& table, const CString& dbt_name, const CString& recname, const CString& dbt_ns);
	virtual ~CTBEDataCoDecDBT		();

	int								GetUpperBound	()			const {return m_Records.GetUpperBound(); }
	int								GetSize			()			const {return m_Records.GetSize(); }
	CTBEDataCoDecRecord*			GetRecord		(int nRow)	const {return m_Records.GetAt(nRow); }
	CTBEDataCoDecRecord*			AddRecord		()				  ;

	const	CString&				GetTableName	() const {return m_TableName;}
	const	CString&				GetDBTName		() const {return m_DBTName;}
	const	CString&				GetDBTNamespace	() const {return m_DBTNamespace;}
	const	DBTType&				GetDBTType		() const {return m_DBTType;}
	const	CString&				GetSqlRecCName	() const {return m_SqlRecordName; }

			int						GetDBTID		() const {return m_DBTID;}
			void					SetDBTID		(int id) {m_DBTID = id; }
};

///////////////////////////////////////////////////////////////////////////////
//	CTBEDataCoDecDBTArray
//-----------------------------------------------------------------------------
class TB_EXPORT CTBEDataCoDecDBTArray : public TArray<CTBEDataCoDecDBT>
{
	DECLARE_DYNAMIC(CTBEDataCoDecDBTArray);

};

///////////////////////////////////////////////////////////////////////////////
//	CTBEDataCoDecDocument
//-----------------------------------------------------------------------------
class TB_EXPORT CTBEDataCoDecDocument : public CObject
{
	DECLARE_DYNAMIC(CTBEDataCoDecDocument)
protected:
	CString							m_DocName;
	CString							m_DocNamespace;
	CGuid							m_gSession;

	CTBEDataCoDecDBT*				m_pDBTMaster;
	CTBEDataCoDecDBTArray			m_DBTSlaves;

public:
	CTBEDataCoDecDocument();
	virtual ~CTBEDataCoDecDocument();

	CTBEDataCoDecDBT*			GetDBTMaster		() const	{ return m_pDBTMaster; }
	CTBEDataCoDecDBT*			GetDBTSlave			(LPCSTR			szClassName);
	CTBEDataCoDecDBT*			GetDBTSlave			(CRuntimeClass*	pDBTClass);
	CTBEDataCoDecDBT*			GetDBTByID			(int nID);

	void						SetDocName			(LPCTSTR szDocName) {m_DocName = szDocName; }
	CString						GetDocName			() const {return m_DocName; }
	void						SetDocNamespace		(LPCTSTR szDocNamespace) { m_DocNamespace = szDocNamespace; }
	CString						GetDocNamespace		() const {return m_DocNamespace; }

	void						SetSession			(GUID g) { m_gSession = g; }
	CGuid						GetSession			() const { return m_gSession; }

	CTBEDataCoDecDBT*			AddDBTMaster		(const CString& dbt_name, const CString& dbt_ns, const CString& rec_name, const CString& tbl_name);
	CTBEDataCoDecDBT*			AddDBTSlave			(const CString& dbt_name, const CString& dbt_ns, const CString& rec_name, const CString& tbl_name);
	CTBEDataCoDecDBT*			AddDBTSlaveBuffered	(const CString& dbt_name, const CString& dbt_ns, const CString& rec_name, const CString& tbl_name);

	CTBEDataCoDecDBT*			GetDBTSlaveSqlRec	(CRuntimeClass*	pSqlRecClass);
	CTBEDataCoDecDBT*			GetDBTSlaveSqlRec	(LPCSTR			szSqlRecClassName);
	CTBEDataCoDecDBT*			GetDBTSlaveBuffered	();
};

///////////////////////////////////////////////////////////////////////////////
//	CTBEDataCoDecClassInfo
//-----------------------------------------------------------------------------
class TB_EXPORT CTBEDataCoDecClassInfo : public CObject
{
	DECLARE_DYNAMIC(CTBEDataCoDecClassInfo)
protected:
	CMap<CString, LPCTSTR, CStringList*, CStringList*>	m_Hierarchy;

public:
	CTBEDataCoDecClassInfo();
	virtual ~CTBEDataCoDecClassInfo();

	void			AddClass(CRuntimeClass*	pRTClass);

	int				GetClassList(CStringList&	sl);
	CStringList*	GetClassHierarchy(LPCSTR szClassName);
	void			SetClassHierarchy(LPCSTR szClassName, CStringList&	hierarchy);
	void			Reset();

	BOOL			IsClassKindOf(LPCSTR cname, LPCSTR cparent);
	BOOL			IsClassKindOf(LPCSTR cname, CRuntimeClass*	pRTClass);
};

//==============================================================================
#include "endh.dex"
