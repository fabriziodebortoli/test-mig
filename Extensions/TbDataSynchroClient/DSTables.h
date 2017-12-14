#pragma once 

#include <TbGeneric\DataObj.h>
#include <tboledb\sqlrec.h>

#include <tbges\TBLREAD.H>
#include <tbges\TBLUPDAT.H>


//includere alla fine degli include del .H
#include "beginh.dex"



///////////////////////////////////////////////////////////////////////////////
//					TDS_Providers definition
//
///////////////////////////////////////////////////////////////////////////////
//-----------------------------------------------------------------------------
class TB_EXPORT TDS_Providers :public SqlRecord
{
	DECLARE_DYNCREATE(TDS_Providers) 

public:
	DataStr		f_Name;
	DataStr		f_Description;
	DataBool	f_Disabled;
	DataStr		f_ProviderUrl;
	DataStr		f_ProviderUser;
	DataStr		f_ProviderPassword;	
	DataBool	f_SkipCrtValidation;
	DataStr		f_ProviderParameters;
	DataBool	f_IsEAProvider;
	DataStr		f_IAFModules;
public:
	TDS_Providers(BOOL bCallInit = TRUE);

public:
    virtual void BindRecord	();	

public:
	static  LPCTSTR  GetStaticName();
};

///////////////////////////////////////////////////////////////////////////////
//					TDS_ActionsLog definition
//
///////////////////////////////////////////////////////////////////////////////
//-----------------------------------------------------------------------------
class TB_EXPORT TDS_ActionsLog :public SqlRecord
{
	DECLARE_DYNCREATE(TDS_ActionsLog) 

public:
	DataLng		f_LogId;
	DataStr		f_ProviderName; //provider Name (DS_Poviders)
	DataStr		f_DocNamespace; //document namespace	
	DataGuid	f_DocTBGuid; // unique identifier of a document instance (using the field TBGuid in document master table)
	DataEnum	f_ActionType; //the action  (INSERT | UPADATE | DELETE)
	DataStr		f_ActionData; // used by provider 
	DataEnum	f_SynchDirection;//  synchronization direction (INBOUND | OUTBOUND)
	DataText	f_SynchXMLData; //using for merge during error manager	
	DataEnum	f_SynchStatus; // synchronization status
	DataStr		f_SynchMessage; // the possible error message

	//DataStr		l_WorkerDescri; //campo locale per avere la decodifica del workerID	
		
	
public:
	TDS_ActionsLog(BOOL bCallInit = TRUE);

public:
    virtual void BindRecord	();	

public:
	static  LPCTSTR  GetStaticName();

	static TCHAR	szLogId[];	
	static TCHAR	szDocNamespace[];
	static TCHAR	szDocTBGuid[]; 
	static TCHAR	szActionType[];
	static TCHAR	szActionData[];
	static TCHAR	szSynchDirection[];
	static TCHAR	szSynchXMLData[];
	static TCHAR	szSynchStatus[];	
	static TCHAR	szSynchMessage[];
	static TCHAR	szProviderName[];
};




/////////////////////////////////////////////////////////////////////////////
//			class TEnhDS_ActionsLog definition					   //
/////////////////////////////////////////////////////////////////////////////
//
//===========================================================================
class TB_EXPORT TEnhDS_ActionsLog: public TDS_ActionsLog
{
	DECLARE_DYNCREATE(TEnhDS_ActionsLog) 
	
public:
	DataStr	l_Code;
	DataStr	l_Description;
	DataStr	l_WorkerDescri; //campo locale per avere la decodifica del workerID
	DataStr	l_SynchStatusBmp;
	DataStr	l_SynchDirectionBmp;
		
public:
	TEnhDS_ActionsLog(BOOL bCallInit = TRUE);
	
public:
    virtual void BindRecord();	
};

///////////////////////////////////////////////////////////////////////////////
//					TDS_ActionsQueue definition
//
///////////////////////////////////////////////////////////////////////////////
//-----------------------------------------------------------------------------
class TB_EXPORT TDS_ActionsQueue :public SqlRecord
{
	DECLARE_DYNCREATE(TDS_ActionsQueue) 

public:
	DataLng		f_LogId;
	DataStr		f_ProviderName; //provider Name (DS_Poviders)
	DataStr		f_ActionName; //Document namespace in OUTBOUND; entity/action name in INBOUND
	DataEnum	f_SynchDirection;//  synchronization direction (INBOUND | OUTBOUND)
	DataText	f_SynchXMLData; //XML for SetData in INBOUND; Empty in OUTBOUND
	DataEnum	f_SynchStatus; // synchronization status
	DataStr		f_SynchFilters; // filter condition
	
	
public:
	TDS_ActionsQueue(BOOL bCallInit = TRUE);

public:
    virtual void BindRecord	();	

public:
	static  LPCTSTR  GetStaticName();
	
	static TCHAR	szLogId[];		
	static TCHAR	szActionName[];
	static TCHAR	szSynchDirection[];
	static TCHAR	szSynchXMLData[];
	static TCHAR	szSynchStatus[];	
	static TCHAR	szSynchFilters[];
	static TCHAR	szProviderName[];
};


///////////////////////////////////////////////////////////////////////////////
//					TDS_SynchronizationInfo definition
//
///////////////////////////////////////////////////////////////////////////////
//-----------------------------------------------------------------------------
class TB_EXPORT TDS_SynchronizationInfo : public SqlRecord
{
	DECLARE_DYNCREATE(TDS_SynchronizationInfo) 

public:
	DataStr		f_ProviderName; //provider Name (DS_Poviders)
	DataGuid	f_DocTBGuid; // unique identifier of a document instance (using the field TBGuid in document master table)
	DataStr		f_DocNamespace; //document namespace
	DataEnum	f_SynchStatus; // synchronization status
	DataDate	f_SynchDate; // last synchronization date
	DataEnum	f_SynchDirection;// last synchronization direction (INBOUND | OUTBOUND)
	DataInt		f_WorkerID; //the last worker that has synchronized document data
	DataEnum	f_LastAction; //the last action fired from client (INSERT | UPADATE | DELETE)
	DataDate	f_StartSynchDate; // Start Synchronization date
	

public:
	TDS_SynchronizationInfo(BOOL bCallInit = TRUE);

public:
    virtual void BindRecord	();	

public:
	static  LPCTSTR  GetStaticName();
	
	static TCHAR	szProviderName[];
	static TCHAR	szDocTBGuid[]; 	
	static TCHAR	szDocNamespace[];
	static TCHAR	szSynchStatus[];	
	static TCHAR	szSynchDate[];
	static TCHAR	szSynchDirection[];
	static TCHAR	szWorkerID[];
	static TCHAR	szLastAction[];	
	static TCHAR	szStartSynchDate[];
};


///////////////////////////////////////////////////////////////////////////////
//					TDS_AttachmentSynchroInfo definition
//
///////////////////////////////////////////////////////////////////////////////
//-----------------------------------------------------------------------------
class TB_EXPORT TDS_AttachmentSynchroInfo : public SqlRecord
{
	DECLARE_DYNCREATE(TDS_AttachmentSynchroInfo) 

public:
	DataStr		f_ProviderName; //provider Name (DS_Poviders)
	DataGuid	f_DocTBGuid; // unique identifier of a document instance (using the field TBGuid in document master table)
	DataLng		f_AttachmentID;
	DataStr		f_DocNamespace; //docum	ent namespace
	DataEnum	f_SynchStatus; // synchronization status
	DataDate	f_SynchDate; // last synchronization date
	DataEnum	f_SynchDirection;// last synchronization direction (INBOUND | OUTBOUND)
	DataInt		f_WorkerID; //the last worker that has synchronized document data
	DataEnum	f_LastAction; //the last action fired from client (INSERT | UPADATE | DELETE)
	
public:
	TDS_AttachmentSynchroInfo(BOOL bCallInit = TRUE);
	
public:
    virtual void BindRecord	();	

public:
	static  LPCTSTR  GetStaticName();
	
	static TCHAR	szProviderName[];
	static TCHAR	szDocTBGuid[]; 	
	static TCHAR	szAttachmentID[];
	static TCHAR	szDocNamespace[];
	static TCHAR	szSynchStatus[];	
	static TCHAR	szSynchDate[];
	static TCHAR	szSynchDirection[];
	static TCHAR	szWorkerID[];
	static TCHAR	szLastAction[];	
};

///////////////////////////////////////////////////////////////////////////////
//					TEnhDS_AttachmentSynchroInfo definition
//
///////////////////////////////////////////////////////////////////////////////
//-----------------------------------------------------------------------------
class TB_EXPORT TEnhDS_AttachmentSynchroInfo: public TDS_AttachmentSynchroInfo
{
	DECLARE_DYNCREATE(TEnhDS_AttachmentSynchroInfo) 
	
public:
	DataStr	l_FileName;
	DataStr	l_AttDescription;
	DataStr	l_WorkerDescri; //campo locale per avere la decodifica del workerID
	DataStr	l_SynchStatusBmp;
			
public:
	TEnhDS_AttachmentSynchroInfo(BOOL bCallInit = TRUE);
	
public:
    virtual void BindRecord();	
};

/////////////////////////////////////////////////////////////////////////////
//			class TEnhDS_SynchronizationInfo definition					   //
/////////////////////////////////////////////////////////////////////////////
//
//===========================================================================
class TB_EXPORT TEnhDS_SynchronizationInfo: public TDS_SynchronizationInfo
{
	DECLARE_DYNCREATE(TEnhDS_SynchronizationInfo) 
	
public:
	DataStr	l_Code;
	DataStr	l_Description;
	DataStr	l_WorkerDescri; //campo locale per avere la decodifica del workerID
	DataStr	l_SynchStatusBmp;
	DataStr	l_SynchDirectionBmp;
	DataStr	l_SynchMessage;	

public:
	TEnhDS_SynchronizationInfo(BOOL bCallInit = TRUE);
	
public:
    virtual void BindRecord();	
};

///////////////////////////////////////////////////////////////////////////////
//					TDS_SynchroFilter definition
//
///////////////////////////////////////////////////////////////////////////////
//-----------------------------------------------------------------------------
class TB_EXPORT TDS_SynchroFilter :public SqlRecord
{
	DECLARE_DYNCREATE(TDS_SynchroFilter) 

public:
	DataStr		f_DocNamespace;  //document namespace
	DataStr		f_ProviderName;  //provider Name (DS_Poviders)
	DataStr		f_SynchroFilter; //the last filters set using "Massive Synchronization" procedure

public:
	TDS_SynchroFilter(BOOL bCallInit = TRUE);

public:
    virtual void BindRecord	();	

public:
	static  LPCTSTR  GetStaticName();
};

///////////////////////////////////////////////////////////////////////////////
//					TRDS_SynchronizationInfo definition
//
///////////////////////////////////////////////////////////////////////////////

//-----------------------------------------------------------------------------
class TB_EXPORT TRDS_SynchronizationInfo : public TableReader
{
	DECLARE_DYNAMIC(TRDS_SynchronizationInfo)
	
protected:
	DataGuid	m_DocTBGuid;
	DataStr		m_ProviderName;

public:
	TRDS_SynchronizationInfo(CBaseDocument* pDocument = NULL);

protected:
	virtual void OnDefineQuery  ();
	virtual void OnPrepareQuery ();
	virtual BOOL IsEmptyQuery	();

public:
	FindResult	FindRecord(const DataGuid& DocTBGuid, const DataStr&  providerName);

	void Clear();

	TDS_SynchronizationInfo* GetRecord () const
		{
			ASSERT(m_pRecord->IsKindOf(RUNTIME_CLASS(TDS_SynchronizationInfo)));
			return (TDS_SynchronizationInfo*) m_pRecord;
		}
};


///////////////////////////////////////////////////////////////////////////////
//				class RRDS_SynchronizationInfoByStatus definition
///////////////////////////////////////////////////////////////////////////////
//
//=============================================================================
class TB_EXPORT RRDS_SynchronizationInfoByStatus: public RowsetReader
{
	DECLARE_DYNAMIC(RRDS_SynchronizationInfoByStatus)
	
private:
	DataStr		m_ProviderName;
	DataEnum	m_SynchStatus;
	DataStr		m_DocNamespace;
	DataBool	m_bDelta;
	DataDate	m_StartSynchDate;

public:
	RRDS_SynchronizationInfoByStatus (DataBool bDelta, DataDate startSynchDate, CBaseDocument* pDocument = NULL);

protected:
	virtual void OnDefineQuery  ();
	virtual void OnPrepareQuery ();
	virtual BOOL IsEmptyQuery	();

public:
	RowsetReader::FindResult FindRecord(const DataStr& aProviderName, const DataStr& aDocNamespace, const DataEnum& aSynchStatus);
	TDS_SynchronizationInfo * GetRecord () const
		{
			ASSERT(m_pRecord->IsKindOf(RUNTIME_CLASS(TDS_SynchronizationInfo)));
			return (TDS_SynchronizationInfo *) m_pRecord;
		}	
};


///////////////////////////////////////////////////////////////////////////////
//					TRDS_ActionsLog definition
//
///////////////////////////////////////////////////////////////////////////////

//-----------------------------------------------------------------------------
class TB_EXPORT TRDS_ActionsLog : public TableReader
{
	DECLARE_DYNAMIC(TRDS_ActionsLog)
	
protected:
	DataGuid	m_DocTBGuid;
	DataStr		m_ProviderName;

public:
	TRDS_ActionsLog(CBaseDocument* pDocument = NULL);

protected:
	virtual void OnDefineQuery  ();
	virtual void OnPrepareQuery ();
	virtual BOOL IsEmptyQuery	();

public:
	FindResult	FindRecord(const DataGuid& DocTBGuid, const DataStr&  providerName);

	void Clear();

	TDS_ActionsLog* GetRecord () const
		{
			ASSERT(m_pRecord->IsKindOf(RUNTIME_CLASS(TDS_ActionsLog)));
			return (TDS_ActionsLog*) m_pRecord;
		}
};

//=============================================================================
//		TUDS_SynchronizationInfo Definition							
//=============================================================================
//-----------------------------------------------------------------------------
class TB_EXPORT TUDS_SynchronizationInfo : public TableUpdater
{
	DECLARE_DYNAMIC(TUDS_SynchronizationInfo)
	
public:
	DataGuid	m_DocTBGuid;
	DataStr		m_ProviderName;

public:
	TUDS_SynchronizationInfo (CAbstractFormDoc* pDocument = NULL, CMessages* pMessages = NULL);
		
protected:
	virtual void	OnDefineQuery		();
	virtual void	OnPrepareQuery		();
	virtual BOOL 	IsEmptyQuery		();

public:	
	FindResult FindRecord(const DataGuid& docTBGuid, const DataStr& providerName, BOOL bLock);	

	TDS_SynchronizationInfo* GetRecord() const
	{
		ASSERT(m_pRecord->IsKindOf(RUNTIME_CLASS(TDS_SynchronizationInfo)));
		return (TDS_SynchronizationInfo*) m_pRecord;
	}
};

///////////////////////////////////////////////////////////////////////////////
//					TRDS_Providers definition
///////////////////////////////////////////////////////////////////////////////
//-----------------------------------------------------------------------------
class TB_EXPORT TRDS_Providers : public TableReader
{
	DECLARE_DYNAMIC(TRDS_Providers)
	
protected:
	DataStr		m_ProviderName;

public:
	TRDS_Providers(CBaseDocument* pDocument = NULL);

protected:
	virtual void OnDefineQuery  ();
	virtual void OnPrepareQuery ();
	virtual BOOL IsEmptyQuery	();

public:
	FindResult	FindRecord(const DataStr&  providerName);

	void Clear();

	TDS_Providers* GetRecord () const
		{
			ASSERT(m_pRecord->IsKindOf(RUNTIME_CLASS(TDS_Providers)));
			return (TDS_Providers*) m_pRecord;
		}
};

//=============================================================================
//		TUDS_Providers Definition							
//=============================================================================
//-----------------------------------------------------------------------------
class TB_EXPORT TUDS_Providers : public TableUpdater
{
	DECLARE_DYNAMIC(TUDS_Providers)
	
public:
	DataStr		m_ProviderName;

public:
	TUDS_Providers (CAbstractFormDoc* pDocument = NULL, CMessages* pMessages = NULL);
		
protected:
	virtual void	OnDefineQuery		();
	virtual void	OnPrepareQuery		();
	virtual BOOL 	IsEmptyQuery		();

public:	
	FindResult FindRecord(const DataStr& providerName, BOOL bLock);	

	TDS_Providers* GetRecord() const
	{
		ASSERT(m_pRecord->IsKindOf(RUNTIME_CLASS(TDS_Providers)));
		return (TDS_Providers*) m_pRecord;
	}
};

///////////////////////////////////////////////////////////////////////////////
//					TUDR_SynchroFilter definition
//
///////////////////////////////////////////////////////////////////////////////

//-----------------------------------------------------------------------------
class TB_EXPORT TRDS_SynchroFilter : public TableReader
{
	DECLARE_DYNAMIC(TRDS_SynchroFilter)
	
protected:
	DataStr	m_DocNamespace;
	DataStr	m_ProviderName;

public:
	TRDS_SynchroFilter(CBaseDocument* pDocument = NULL);

protected:
	virtual void OnDefineQuery  ();
	virtual void OnPrepareQuery ();
	virtual BOOL IsEmptyQuery	();

public:
	FindResult	FindRecord(const DataStr& docNamespace, const DataStr& providerName);

	void Clear();

	TDS_SynchroFilter* GetRecord () const
		{
			ASSERT(m_pRecord->IsKindOf(RUNTIME_CLASS(TDS_SynchroFilter)));
			return (TDS_SynchroFilter*) m_pRecord;
		}
};

//=============================================================================
//		TUDS_SynchroFilter Definition							
//=============================================================================
//-----------------------------------------------------------------------------
class TB_EXPORT TUDS_SynchroFilter : public TableUpdater
{
	DECLARE_DYNAMIC(TUDS_SynchroFilter)
	
public:
	DataStr	m_DocNamespace;
	DataStr	m_ProviderName;
	
public:
	TUDS_SynchroFilter (CAbstractFormDoc* pDocument = NULL, CMessages* pMessages = NULL);
		
protected:
	virtual void	OnDefineQuery		();
	virtual void	OnPrepareQuery		();
	virtual BOOL 	IsEmptyQuery		();

public:	
	FindResult FindRecord(const DataStr& docNamespace, const DataStr& providerName, BOOL bLock);	

	TDS_SynchroFilter* GetRecord() const
	{
		ASSERT(m_pRecord->IsKindOf(RUNTIME_CLASS(TDS_SynchroFilter)));
		return (TDS_SynchroFilter*) m_pRecord;
	}
};

//strutture di ausilio 

#define	NOTIFY_DELETE			0x0001
#define	NOTIFY_UPDATE			0x0010
#define	NOTIFY_INSERT			0x0100

/////////////////////////////////////////////////////////////////////////////
//			class SynchroTableReader definition							   //
/////////////////////////////////////////////////////////////////////////////
//
//=========================================================================================
class TB_EXPORT SynchroTableReader : public TableReader
{
	DECLARE_DYNAMIC(SynchroTableReader)

private:
	CStringArray*		m_pDescriptionFields;
	CStringArray*		m_pKeyFieldsToUse;
	
public:	
	DataGuid m_tbGuid;	

public:
	SynchroTableReader(CRuntimeClass* pSqlRecordClass, SqlSession* pSqlSession);

protected:
	virtual void OnDefineQuery  ();
	virtual void OnPrepareQuery ();
	virtual BOOL IsEmptyQuery	();

public:
	void SetFieldsToSelect(CStringArray* pKFields, CStringArray* pDescriptionFields);

	FindResult	FindRecord(const DataGuid& tbGuid);

	SqlRecord* GetRecord () const	{ return m_pRecord;}
};

/////////////////////////////////////////////////////////////////////////////
//			class CSynchroDocInfo definition					   //
/////////////////////////////////////////////////////////////////////////////
//
class TB_EXPORT CSynchroDocInfo : public CObject
{
private:
	CTBNamespace		m_tableNamespace;
	CRuntimeClass*		m_pSqlRecordClass;
	SqlSession*			m_pSqlSession;
	SynchroTableReader* m_pSynchroTR;
	CStringArray		m_DescriptionFields;
	CStringArray		m_KeyFieldsToUse;
	BOOL				m_bIsValid;

public:
	CString				m_docTitle;
	CString				m_strDocNamespace;
	WORD				m_wActionMode;
	CString				m_OnlyForDMS;
	CString				m_iMagoConfigurations;

	
public:
	CSynchroDocInfo(const CString& aDocNamespace);
	~CSynchroDocInfo();

public:
	void SetDecodingInfo(SqlSession* pSession);

public:
	void SetActionMode(const CString& strActionMode);
	BOOL IsValid() const { return m_bIsValid; }
	void GetDecodingInfo(const DataGuid&, DataStr& recordKey, DataStr& recordDescription);
};

/////////////////////////////////////////////////////////////////////////////
//			class CSynchroDocInfoArray definition					   //
/////////////////////////////////////////////////////////////////////////////
//
class TB_EXPORT CSynchroDocInfoArray : public Array
{
public:
	CSynchroDocInfo* GetAt(int nIdx) const { return (CSynchroDocInfo*)Array::GetAt(nIdx); }
	int Add(CSynchroDocInfo* pSynchroDocInfo)  { return Array::Add(pSynchroDocInfo); }

	CSynchroDocInfo* GetDocumentByNs(const CString& strDocNamespace) const;
};

/////////////////////////////////////////////////////////////////////////////
//						class VOptionalParam definition
/////////////////////////////////////////////////////////////////////////////
class TB_EXPORT VProviderParams : public SqlVirtualRecord
{
	DECLARE_DYNCREATE (VProviderParams) 

public:
	DataStr			l_Name;
	DataStr			l_Description;
	DataStr			l_Value;		

public:
	VProviderParams(BOOL bCallInit   = TRUE) ;


public:
    virtual void BindRecord();
	virtual BOOL IsEmpty();

public:
	static LPCTSTR   GetStaticName();
};

/********************************************* TABELLE PER PROCEDURA DI VALIDAZIONE ***********************************************************/
///////////////////////////////////////////////////////////////////////////////
//					TDS_ValidationInfo definition
//
///////////////////////////////////////////////////////////////////////////////
//-----------------------------------------------------------------------------
class TB_EXPORT TDS_ValidationInfo :public SqlRecord
{
	DECLARE_DYNCREATE(TDS_ValidationInfo)

public:
	DataStr		f_ProviderName; 
	DataGuid	f_DocTBGuid;
	DataStr		f_ActionName;
	DataStr		f_DocNamespace;
	DataBool	f_FKError;
	DataBool	f_XSDError;
	DataBool	f_UsedForFilter;
	DataText	f_MessageError;
	DataDate	f_ValidationDate;

public:
	TDS_ValidationInfo(BOOL bCallInit = TRUE);

public:
    virtual void BindRecord	();	

public:
	static  LPCTSTR  GetStaticName();

	static TCHAR TDS_ValidationInfo::szProviderName[];
	static TCHAR TDS_ValidationInfo::szDocTBGuid[];
	static TCHAR TDS_ValidationInfo::szActionName[];
	static TCHAR TDS_ValidationInfo::szDocNamespace[];
	static TCHAR TDS_ValidationInfo::szFKError[];
	static TCHAR TDS_ValidationInfo::szXSDError[];
	static TCHAR TDS_ValidationInfo::szUsedForFilter[];
	static TCHAR TDS_ValidationInfo::szMessageError[];
	static TCHAR TDS_ValidationInfo::szValidationDate[];
};

///////////////////////////////////////////////////////////////////////////////
//					TDS_ValidationFKtoFix definition
//
///////////////////////////////////////////////////////////////////////////////
//-----------------------------------------------------------------------------
class TB_EXPORT TDS_ValidationFKtoFix :public SqlRecord
{
	DECLARE_DYNCREATE(TDS_ValidationFKtoFix)

public:
	DataLng		f_ID;
	DataStr		f_ProviderName; 
	DataStr		f_DocNamespace;
	DataStr		f_TableName;
	DataStr		f_FieldName;
	DataStr		f_ValueToFix;
	DataInt		f_RelatedErrors;
	DataDate	f_ValidationDate;

public:
	TDS_ValidationFKtoFix(BOOL bCallInit = TRUE);

public:
    virtual void BindRecord	();	

public:
	static  LPCTSTR  GetStaticName();

	static TCHAR TDS_ValidationFKtoFix::szID			[];
	static TCHAR TDS_ValidationFKtoFix::szProviderName	[];
	static TCHAR TDS_ValidationFKtoFix::szDocNamespace	[];
	static TCHAR TDS_ValidationFKtoFix::szTableName		[];
	static TCHAR TDS_ValidationFKtoFix::szFieldName		[];
	static TCHAR TDS_ValidationFKtoFix::szValueToFix	[];
	static TCHAR TDS_ValidationFKtoFix::szRelatedErrors	[];
	static TCHAR TDS_ValidationFKtoFix::szValidationDate[];
};

///////////////////////////////////////////////////////////////////////////////
//					TRDS_ValidationInfo definition
//
///////////////////////////////////////////////////////////////////////////////

//-----------------------------------------------------------------------------
class TB_EXPORT TRDS_ValidationInfo : public TableReader
{
	DECLARE_DYNAMIC(TRDS_ValidationInfo)
	
protected:
	DataGuid	m_DocTBGuid;
	DataStr		m_ProviderName;

public:
	TRDS_ValidationInfo			(CBaseDocument* pDocument = NULL);

protected:
	virtual void OnDefineQuery  ();
	virtual void OnPrepareQuery ();
	virtual BOOL IsEmptyQuery	();

public:
	FindResult	FindRecord(const DataGuid& DocTBGuid, const DataStr&  providerName);
	
	TDS_ValidationInfo* GetRecord() const
	{
		ASSERT(m_pRecord->IsKindOf(RUNTIME_CLASS(TDS_ValidationInfo)));
		return (TDS_ValidationInfo*) m_pRecord;
	}
};

//=============================================================================
//		TUDS_ValidationInfo Definition							
//=============================================================================
//-----------------------------------------------------------------------------
class TB_EXPORT TUDS_ValidationInfo : public TableUpdater
{
	DECLARE_DYNAMIC(TUDS_ValidationInfo)
	
public:
	DataGuid	m_DocTBGuid;
	DataStr		m_ProviderName;

public:
	TUDS_ValidationInfo(CAbstractFormDoc* pDocument = NULL, CMessages* pMessages = NULL);
		
protected:
	virtual void	OnDefineQuery		();
	virtual void	OnPrepareQuery		();
	virtual BOOL 	IsEmptyQuery		();

public:	
	FindResult FindRecord(const DataGuid& docTBGuid, const DataStr& providerName, BOOL bLock);	

	TDS_ValidationInfo* GetRecord() const
	{
		ASSERT(m_pRecord->IsKindOf(RUNTIME_CLASS(TDS_ValidationInfo)));
		return (TDS_ValidationInfo*) m_pRecord;
	}
};

///////////////////////////////////////////////////////////////////////////////
//					TRDS_ValidationFKtoFix definition
///////////////////////////////////////////////////////////////////////////////
//
//-----------------------------------------------------------------------------
class TB_EXPORT TRDS_ValidationFKtoFix : public TableReader
{
	DECLARE_DYNAMIC(TRDS_ValidationFKtoFix)
	
protected:
	DataStr		m_ProviderName;
	DataStr		m_DocNamespace;
	DataStr		m_TableName;
	DataStr		m_FieldName;
	DataStr		m_Value;

public:
	TRDS_ValidationFKtoFix		(CBaseDocument* pDocument = NULL);

protected:
	virtual void OnDefineQuery  ();
	virtual void OnPrepareQuery ();
	virtual BOOL IsEmptyQuery	();

public:
	FindResult	FindRecord	(
								const DataStr&  aProviderName,
						 		const DataStr&  aDocNamespace,
								const DataStr&  aTableName,
								const DataStr&  aFieldName,
						 		const DataStr&  aValue
							);
	
	TDS_ValidationFKtoFix* GetRecord() const
	{
		ASSERT(m_pRecord->IsKindOf(RUNTIME_CLASS(TDS_ValidationFKtoFix)));
		return (TDS_ValidationFKtoFix*) m_pRecord;
	}
};

///////////////////////////////////////////////////////////////////////////////
//					TUDS_ValidationFKtoFix definition
///////////////////////////////////////////////////////////////////////////////
//
//-----------------------------------------------------------------------------
class TB_EXPORT TUDS_ValidationFKtoFix : public TableUpdater
{
	DECLARE_DYNAMIC(TUDS_ValidationFKtoFix)
	
public:
	DataStr		m_ProviderName;
	DataStr		m_DocNamespace;
	DataStr		m_TableName;
	DataStr		m_FieldName;
	DataStr		m_Value;

public:
	TUDS_ValidationFKtoFix(CAbstractFormDoc* pDocument = NULL, CMessages* pMessages = NULL);
		
protected:
	virtual void	OnDefineQuery		();
	virtual void	OnPrepareQuery		();
	virtual BOOL 	IsEmptyQuery		();

public:	
	FindResult FindRecord(
						 	const DataStr&  aProviderName,
						 	const DataStr&  aDocNamespace,
							const DataStr&  aTableName,
							const DataStr&  aFieldName,
						 	const DataStr&  aValue,
								  BOOL		bLock
						 );

	TDS_ValidationFKtoFix* GetRecord() const
	{
		ASSERT(m_pRecord->IsKindOf(RUNTIME_CLASS(TDS_ValidationFKtoFix)));
		return (TDS_ValidationFKtoFix*) m_pRecord;
	}
};

///////////////////////////////////////////////////////////////////////////////
//					VSynchronizationInfo definition
//
///////////////////////////////////////////////////////////////////////////////
//-----------------------------------------------------------------------------
class TB_EXPORT VSynchronizationInfo : public SqlVirtualRecord
{
	DECLARE_DYNCREATE(VSynchronizationInfo)

public:
	DataStr		l_ProviderName; //provider Name (DS_Poviders)
	DataGuid	l_DocTBGuid; // unique identifier of a document instance (using the field TBGuid in document master table)
	DataStr		l_DocNamespace; //document namespace
	DataEnum	l_SynchStatus; // synchronization status
	DataDate	l_SynchDate; // last synchronization date
	DataEnum	l_SynchDirection;// last synchronization direction (INBOUND | OUTBOUND)
	DataInt		l_WorkerID; //the last worker that has synchronized document data
	DataEnum	l_LastAction; //the last action fired from client (INSERT | UPADATE | DELETE)
	DataDate	l_StartSynchDate; // Start Synchronization date
	
public:
	VSynchronizationInfo(BOOL bCallInit = TRUE);

public:
	virtual void BindRecord();
	static LPCTSTR   GetStaticName();
public:
	static TCHAR	szProviderName[];
	static TCHAR	szDocTBGuid[];
	static TCHAR	szDocNamespace[];
	static TCHAR	szSynchStatus[];
	static TCHAR	szSynchDate[];
	static TCHAR	szSynchDirection[];
	static TCHAR	szWorkerID[];
	static TCHAR	szLastAction[];
	static TCHAR	szStartSynchDate[];
};

///////////////////////////////////////////////////////////////////////////////
//					VSynchronizationInfo definition
//
///////////////////////////////////////////////////////////////////////////////
//-----------------------------------------------------------------------------
class TB_EXPORT VEnh_SynchronizationInfo : public VSynchronizationInfo
{
	DECLARE_DYNCREATE(VEnh_SynchronizationInfo)

public:

public:
	DataStr	l_Code;
	DataStr	l_Description;
	DataStr	l_WorkerDescri; //campo locale per avere la decodifica del workerID
	DataStr	l_SynchStatusBmp;
	DataStr	l_SynchDirectionBmp;
	DataStr	l_SynchMessage;

public:
	VEnh_SynchronizationInfo(BOOL bCallInit = TRUE);

public:
	virtual void BindRecord();
	static LPCTSTR   GetStaticName();

};

///////////////////////////////////////////////////////////////////////////////
//					TDS_ActionsLog definition
//
///////////////////////////////////////////////////////////////////////////////
//-----------------------------------------------------------------------------
class TB_EXPORT VActionsLog :public SqlVirtualRecord
{
	DECLARE_DYNCREATE(VActionsLog)

public:
	DataLng		f_LogId;
	DataStr		f_ProviderName; //provider Name (DS_Poviders)
	DataStr		f_DocNamespace; //document namespace	
	DataGuid	f_DocTBGuid; // unique identifier of a document instance (using the field TBGuid in document master table)
	DataEnum	f_ActionType; //the action  (INSERT | UPADATE | DELETE)
	DataStr		f_ActionData; // used by provider 
	DataEnum	f_SynchDirection;//  synchronization direction (INBOUND | OUTBOUND)
	DataText	f_SynchXMLData; //using for merge during error manager	
	DataEnum	f_SynchStatus; // synchronization status
	DataStr		f_SynchMessage; // the possible error message
	DataStr		l_SynchStatusBmp;
	DataStr		l_SynchDirectionBmp;
	DataStr		l_WorkerDescri;
public:
	VActionsLog(BOOL bCallInit = TRUE);

public:
	virtual void BindRecord();
};



#include "endh.dex"
