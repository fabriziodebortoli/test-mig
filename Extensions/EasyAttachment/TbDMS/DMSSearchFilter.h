#pragma once

#include <TbOleDb\SqlRec.h>
#include <TbGes\HotLink.h>
#include <TbGes\DBT.h>

//includere alla fine degli include del .H
#include "beginh.dex"

class BDDMSRepository;

// SqlRecord relativa alla tabella DMS_Field contenente tutto l'elenco dei campi
//di ricerca
//////////////////////////////////////////////////////////////////////////////
//								TDMS_Field
//////////////////////////////////////////////////////////////////////////////
//-----------------------------------------------------------------------------
class TB_EXPORT TDMS_Field : public SqlRecord
{
	DECLARE_DYNCREATE(TDMS_Field)

public:
	DataStr		f_FieldName;
	DataStr		f_FieldDescription;
	DataStr		f_ValueType;
	//DataBool	f_IsCategory;

public:
	TDMS_Field(BOOL bCallInit = TRUE);

public:
	virtual void BindRecord();

public:
	static  LPCTSTR  GetStaticName();
};

// SqlRecord relativa alla tabella TDMS_CollectionsFields contenente l'elenco dei campi
//di ricerca di una collection
//////////////////////////////////////////////////////////////////////////////
//								TDMS_CollectionsFields
//////////////////////////////////////////////////////////////////////////////
//-----------------------------------------------------------------------------
class TB_EXPORT TDMS_CollectionsFields : public SqlRecord
{
	DECLARE_DYNCREATE(TDMS_CollectionsFields)

public:
	DataStr		f_FieldName;
	DataLng		f_CollectionID;
	DataInt		f_GroupType;

public:
	TDMS_CollectionsFields(BOOL bCallInit = TRUE);

public:
	virtual void BindRecord();

public:
	static  LPCTSTR  GetStaticName();
};

// SqlRecord relativa alla tabella TDMS_SearchFieldIndexes contenente l'elenco dei valori dei campi di ricerca
//////////////////////////////////////////////////////////////////////////////
//								TDMS_SearchFieldIndexes
//////////////////////////////////////////////////////////////////////////////
//-----------------------------------------------------------------------------
class TB_EXPORT TDMS_SearchFieldIndexes : public SqlRecord
{
	DECLARE_DYNCREATE(TDMS_SearchFieldIndexes)

public:
	DataLng		f_SearchIndexID;
	DataStr		f_FieldName;
	DataStr		f_FieldValue;
	DataStr		f_FormattedValue;

public:
	TDMS_SearchFieldIndexes(BOOL bCallInit = TRUE);

public:
	virtual void BindRecord();

public:
	static  LPCTSTR  GetStaticName();
};

/////////////////////////////////////////////////////////////////////////////
// 							 class HKLDMSFields
/////////////////////////////////////////////////////////////////////////////
//
class TB_EXPORT HKLDMSFields : public HotKeyLink
{
	DECLARE_DYNCREATE(HKLDMSFields)

private:
	TDMS_CollectionsFields* m_pCollectionsFieldsRec;
	DataLng*  m_pCollectionID;

public:
	HKLDMSFields();
	~HKLDMSFields();

public:
	void FilterForCollectionID(DataLng* pCollectionID);

protected:
	virtual void		OnDefineQuery(SelectionType nQuerySelection = DIRECT_ACCESS);
	virtual void		OnPrepareQuery(DataObj*, SelectionType nQuerySelection = DIRECT_ACCESS);

public:
	TDMS_Field* 		GetRecord() const
	{
		ASSERT(m_pRecord->IsKindOf(RUNTIME_CLASS(TDMS_Field)));
		return (TDMS_Field*)m_pRecord;
	}
};

/////////////////////////////////////////////////////////////////////////////
// 							 class HKLSearchFieldIndexes
/////////////////////////////////////////////////////////////////////////////
//
class TB_EXPORT HKLSearchFieldIndexes : public HotKeyLink
{
	DECLARE_DYNCREATE(HKLSearchFieldIndexes)

public:
	DataStr		m_FieldName;
	DataLng		m_CollectionID; //io potrei volere solo i valori che si riferiscono ad allegati che fanno riferimento ad una specifica collectionID

public:
	HKLSearchFieldIndexes();

public:
	void SetFieldName(DataStr fieldName) { m_FieldName = fieldName; }

protected:
	virtual void		OnDefineQuery(SelectionType nQuerySelection = DIRECT_ACCESS);
	virtual void		OnPrepareQuery(DataObj*, SelectionType nQuerySelection = DIRECT_ACCESS);

public:
	TDMS_SearchFieldIndexes* 		GetRecord() const
	{
		ASSERT(m_pRecord->IsKindOf(RUNTIME_CLASS(TDMS_SearchFieldIndexes)));
		return (TDMS_SearchFieldIndexes*)m_pRecord;
	}
};

///////////////////////////////////////////////////////////////////////////////
//						VSearchFieldsConditions definition
//		SqlVirtualRecord per mappare le condizioni sui campi di ricerca da usare nei filtri
///////////////////////////////////////////////////////////////////////////////
//=============================================================================
class TB_EXPORT VSearchFieldCondition : public SqlVirtualRecord
{
	DECLARE_DYNCREATE(VSearchFieldCondition)

public:
	DataLng	  l_ConditionRow;
	DataLng   l_CollectionID;
	DataLng   l_SearchFieldID;

	DataStr	  l_CollectionName;
	DataStr   l_FieldName;
	DataStr   l_FieldDescription;
	DataEnum  l_OperationType;
	DataStr   l_FormattedValue;
	DataStr   l_FieldValue;
	DataStr	  l_ValueType;
	DataEnum  l_LogicOperator;

public:
	VSearchFieldCondition(BOOL bCallInit = TRUE);

public:
	BOOL IsValidCondition() const;

public:
	virtual void	BindRecord();

public:
	static LPCTSTR   GetStaticName();
};

//////////////////////////////////////////////////////////////////////////////
//			       class DBTVSearchFieldsConditions definition
//		DBT per visualizzare un elenco di documenti archiviati
//////////////////////////////////////////////////////////////////////////////
//=============================================================================
class TB_EXPORT DBTSearchFieldsConditions : public DBTSlaveBuffered
{
	DECLARE_DYNAMIC(DBTSearchFieldsConditions)

public:
	DBTSearchFieldsConditions(CRuntimeClass* pClass, CAbstractFormDoc* pDocument);

public:
	VSearchFieldCondition*	GetCurrent					() 			const	{ return (VSearchFieldCondition*)GetCurrentRow(); }
	VSearchFieldCondition*	GetSearchFieldConditionAt	(int nRow) 	const 	{ return (VSearchFieldCondition*)GetRow(nRow); }
	VSearchFieldCondition*	GetSearchFieldCondition		()		   	const	{ return (VSearchFieldCondition*)GetRecord(); }

	int					GetCurrentRowIdx()	const 	{ return m_nCurrentRow; }
	BDDMSRepository*	GetDocument		()	const	{ return (BDDMSRepository*)DBTSlaveBuffered::GetDocument(); }

	virtual BOOL		LocalFindData	(BOOL bPrepareOld) { return TRUE; }

protected:
	virtual	void		OnDefineQuery		()	{}
	virtual	void		OnPrepareQuery		()	{}
	virtual DataObj*	OnCheckPrimaryKey	(int /*nRow*/, SqlRecord*);
	virtual void		OnPreparePrimaryKey	(int /*nRow*/, SqlRecord*);
	virtual void		OnPrepareRow		(int /*nRow*/, SqlRecord* );

	virtual BOOL		UserIsDuplicateKey	(SqlRecord*, SqlRecord*);
	virtual CString		GetDuplicateKeyMsg	(SqlRecord* pRec);
};

///////////////////////////////////////////////////////////////////////////////
//					CSearchFilter definition
// struttura per mappare i FilterEventArgs di EasyAttachment
///////////////////////////////////////////////////////////////////////////////
//=============================================================================
class TB_EXPORT CSearchFilter : public CObject
{
	DECLARE_DYNCREATE(CSearchFilter)

public:
	CSearchFilter();
	~CSearchFilter();

public:
	CStringArray	m_arWorkers;
	DataInt			m_TopDocsNumber;
	DataStr			m_DocExtensionType;
	DataDate		m_StartDate;
	DataDate		m_EndDate;
	DataStr			m_FreeTag;
	DataLng			m_CollectionID;

	DBTSearchFieldsConditions* m_pDBTSearchFieldsConditions;

	DWORD			m_wSearchLocation;
	enum SearchLocation // mappa lo stesso enum del C#
	{
		None = 0x0000,
		All = 0x0001,
		Tags = 0x0002,
		AllBookmarks = 0x0004,
		NameAndDescription = 0x008,
		Barcode = 0x0010,
		Content = 0x0020
	};

	RecordArray* m_pCachedSearchFieldsArray;

public:
	void Clear							();
	void SetSearchFieldsConditions		(DBTSearchFieldsConditions* pDBTSearchFieldsConditions) { m_pDBTSearchFieldsConditions = pDBTSearchFieldsConditions; }
	void CreateSearchFieldsConditions	();
	void RestoreFilters					();

	void SetSearchLocation	(BOOL bSet, DWORD aSearchLocationFlag)	{ m_wSearchLocation = bSet ? m_wSearchLocation | aSearchLocationFlag : m_wSearchLocation & ~aSearchLocationFlag; }
	void SetSearchAll		()										{ m_wSearchLocation = All; }
	void SetSearchNone		()										{ m_wSearchLocation = None; }
};

#include "endh.dex"