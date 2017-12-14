#pragma once 

#include <TbGeneric\DataObj.h>
#include <tboledb\sqlrec.h>
#include <tbges\TBLUPDAT.H>
#include <tbges\HotLink.h>

class SqlConnection;

//includere alla fine degli include del .H
#include "beginh.dex"

///////////////////////////////////////////////////////////////////////////////
//					TRS_Configuration definition
///////////////////////////////////////////////////////////////////////////////
//-----------------------------------------------------------------------------
class TB_EXPORT TRS_Configuration :public SqlRecord
{
	DECLARE_DYNCREATE(TRS_Configuration) 

public:
	DataLng		f_OfficeID;
	DataStr		f_UsedEntries;
	DataBool	f_IsValid;

public:
	TRS_Configuration(BOOL bCallInit = TRUE);

public:
    virtual void BindRecord	();	

public:
	static  LPCTSTR  GetStaticName();
};

///////////////////////////////////////////////////////////////////////////////
//					TRS_Subjects definition
///////////////////////////////////////////////////////////////////////////////
//-----------------------------------------------------------------------------
class TB_EXPORT TRS_Subjects : public SqlRecord
{
	DECLARE_DYNCREATE(TRS_Subjects) 

public:
	DataLng		f_SubjectID;
	DataBool	f_IsWorker;
	DataStr 	f_ResourceType;
	DataStr 	f_ResourceCode;
	DataLng 	f_WorkerID;
	DataStr		f_Description;

	// local
	DataStr		l_Description;
	DataStr		l_WRLabel;

public:
	TRS_Subjects(BOOL bCallInit = TRUE);

public:
    virtual void	BindRecord			();	
	//virtual CString GetRecordDescription() const;

public:
	static LPCTSTR  GetStaticName();

	static TCHAR	szSubjectID[];
};

///////////////////////////////////////////////////////////////////////////////
//					TRS_SubjectsHierarchy definition
///////////////////////////////////////////////////////////////////////////////
//-----------------------------------------------------------------------------
class TB_EXPORT TRS_SubjectsHierarchy : public SqlRecord
{
	DECLARE_DYNCREATE(TRS_SubjectsHierarchy) 

public:
	DataLng		f_MasterSubjectID;
	DataLng		f_SlaveSubjectID;
	DataInt		f_NrLevel;

public:
	TRS_SubjectsHierarchy(BOOL bCallInit = TRUE);

public:
    virtual void BindRecord	();	

public:
	static LPCTSTR	GetStaticName();

	static TCHAR	szMasterSubjectID[]; 
	static TCHAR	szSlaveSubjectID[];
};

///////////////////////////////////////////////////////////////////////////////
//					RS_TmpOldHierarchies definition
///////////////////////////////////////////////////////////////////////////////
//-----------------------------------------------------------------------------
class TB_EXPORT TRS_TmpOldHierarchies : public SqlRecord
{
	DECLARE_DYNCREATE(TRS_TmpOldHierarchies) 

public:
	DataLng		f_MasterSubjectID;
	DataLng		f_SlaveSubjectID;
	DataInt		f_NrLevel;

public:
	TRS_TmpOldHierarchies(BOOL bCallInit = TRUE);

public:
    virtual void BindRecord	();	

public:
	static  LPCTSTR  GetStaticName();
};

///////////////////////////////////////////////////////////////////////////////
//					TRS_SubjectsGrants definition
///////////////////////////////////////////////////////////////////////////////
//-----------------------------------------------------------------------------
class TB_EXPORT TRS_SubjectsGrants : public SqlRecord
{
	DECLARE_DYNCREATE(TRS_SubjectsGrants) 

public:
	DataLng		f_SubjectID;
	DataStr		f_EntityName;
	DataLng		f_RowSecurityID;
	DataEnum	f_GrantType;
	DataBool	f_Inherited;
	DataBool	f_IsImplicit;
	DataLng		f_WorkerID;	

	// locals
	DataStr     l_Code;
	DataStr     l_Description;
	DataBool	l_Selected;
	
public:
	TRS_SubjectsGrants(BOOL bCallInit = TRUE);

public:
    virtual void BindRecord	();	

public:
	static LPCTSTR  GetStaticName();

	//fields name
	static const CString&	s_sSubjectID;
	static const CString&	s_sEntityName; 
	static const CString&	s_sRowSecurityID;
	static const CString&	s_sGrantType;
	static const CString&	s_sInherited;
	static const CString&	s_sWorkerID;
	static const CString&	s_sIsImplicit;	
	//locals name
	static const CString&	s_sCode;
	static const CString&	s_sDescription;
	static const CString&	s_sSelected;

};

//=============================================================================
//		TableReader Definitions
//=============================================================================
//-----------------------------------------------------------------------------
class TB_EXPORT TRRS_SubjectsByWorkerID : public TableReader
{
	DECLARE_DYNAMIC(TRRS_SubjectsByWorkerID)
	
protected:
	DataLng		m_SubjectID;
	DataStr 	m_ResourceType;
	DataStr 	m_ResourceCode;
	DataBool	m_IsWorker;
	DataLng 	m_WorkerID;

public:
	TRRS_SubjectsByWorkerID(CBaseDocument* pDocument = NULL);

protected:
	virtual void OnDefineQuery  ();
	virtual void OnPrepareQuery ();
	virtual BOOL IsEmptyQuery	();

public:
	FindResult	FindRecord(const DataLng& aWorkerID);

	void Clear();

	TRS_Subjects* GetRecord () const
		{
			ASSERT(m_pRecord->IsKindOf(RUNTIME_CLASS(TRS_Subjects)));
			return (TRS_Subjects*) m_pRecord;
		}
};

//-----------------------------------------------------------------------------
class TB_EXPORT TRRS_SubjectsByResource : public TableReader
{
	DECLARE_DYNAMIC(TRRS_SubjectsByResource)
	
protected:
	DataLng		m_SubjectID;
	DataStr 	m_ResourceType;
	DataStr 	m_ResourceCode;
	DataBool	m_IsWorker;
	DataLng 	m_WorkerID;

public:
	TRRS_SubjectsByResource(CBaseDocument* pDocument = NULL);

protected:
	virtual void OnDefineQuery  ();
	virtual void OnPrepareQuery ();
	virtual BOOL IsEmptyQuery	();

public:
	FindResult	FindRecord(const DataStr& sResourceCode, const DataStr&	sResourceType);

	void Clear();

	TRS_Subjects* GetRecord () const
		{
			ASSERT(m_pRecord->IsKindOf(RUNTIME_CLASS(TRS_Subjects)));
			return (TRS_Subjects*) m_pRecord;
		}
};

//=============================================================================
//		TableUpdater Definition							
//=============================================================================
//-----------------------------------------------------------------------------
class TB_EXPORT TURS_SubjectsGrants : public TableUpdater
{
	DECLARE_DYNAMIC(TURS_SubjectsGrants)
	
public:
	DataStr m_strEntityName;
	DataLng m_nRowSecurityID;
	DataLng m_nSubjectID;

public:
	TURS_SubjectsGrants (CAbstractFormDoc* pDocument = NULL, CMessages* pMessages = NULL);
		
protected:
	virtual void	OnDefineQuery		();
	virtual void	OnPrepareQuery		();
	virtual BOOL 	IsEmptyQuery		();

public:	
	FindResult FindRecord(const DataLng& aSubjectID, const DataStr& strEntityName, const DataLng& nRowSecurityID, BOOL bLock);	

	TRS_SubjectsGrants* GetRecord() const
	{
		ASSERT(m_pRecord->IsKindOf(RUNTIME_CLASS(TRS_SubjectsGrants)));
		return (TRS_SubjectsGrants*) m_pRecord;
	}
};


//=============================================================================
//		TableUpdater Definition							
//=============================================================================
//-----------------------------------------------------------------------------
class TB_EXPORT TURS_Configuration : public TableUpdater
{
	DECLARE_DYNAMIC(TURS_Configuration)
	
public:
	DataLng		m_nOfficeID;
	DataStr		m_strUsedEntries;
	DataBool	m_bIsValid;

public:
	TURS_Configuration(CAbstractFormDoc* pDocument = NULL, CMessages* pMessages = NULL);
		
protected:
	virtual void OnDefineQuery	();
	virtual void OnPrepareQuery	();
	virtual BOOL IsEmptyQuery	();

public:	
	FindResult FindRecord(const DataLng& nOfficeID, BOOL bLock);

	TRS_Configuration* GetRecord() const
	{
		ASSERT(m_pRecord->IsKindOf(RUNTIME_CLASS(TRS_Configuration)));
		return (TRS_Configuration*) m_pRecord;
	}
};

//////////////////////////////////////////////////////////////////////////////
//             			RowSecurityAddOnFields
//////////////////////////////////////////////////////////////////////////////
//-----------------------------------------------------------------------------
class TB_EXPORT RowSecurityAddOnFields: public SqlAddOnFieldsColumn
{
	DECLARE_DYNCREATE(RowSecurityAddOnFields)

public:
	DataLng			f_RowSecurityID;
	DataBool		f_IsProtected;
	DataEnum		l_CurrentWorkerGrantType;
	DataEnum		l_SpecificWorkerGrantType;

public:   
	RowSecurityAddOnFields();

public:   
	virtual int		BindAddOnFields(int nStartPos = 0);

public:
	static const CString&  s_sRowSecurityID;
	static const CString&  s_sIsProtected;
	static const CString&  s_sCurrentWorkerGrantType;	
	static const CString&  s_sSpecificWorkerGrantType;
};

/////////////////////////////////////////////////////////////////////////////
//	Hotlink		HKLRowSecurity
/////////////////////////////////////////////////////////////////////////////
// Hotlink generico che deve agganciarsi ad una delle tabelle referenziate
// dall'entita' corrente (il Cliente, la Pratica, etc.)
// A seconda del RowSecurityID e del nome entita' letti dalla RS_SubjectsGrants 
// deve andare a leggere i dati dalla tabella di OFM correlata
/////////////////////////////////////////////////////////////////////////////
//-----------------------------------------------------------------------------
class TB_EXPORT HKLRowSecurity : public HotKeyLink
{
	DECLARE_DYNCREATE (HKLRowSecurity)

public:
	HKLRowSecurity();
	HKLRowSecurity(CRuntimeClass* pRecClass, const CString& strDocNamespace);

private:
	DataLng		m_nRowSecurityID;
	CString		m_strKeyField;
	CString		m_strDescriptionField;

public:
	void SetRowSecurityID	(const DataLng& nRowSecurityID)			{ m_nRowSecurityID		= nRowSecurityID; }
	void SetKeyField		(const CString& strKeyField)			{ m_strKeyField			= strKeyField; }
	void SetKeyDescription	(const CString& strDescriptionField)	{ m_strDescriptionField = strDescriptionField; }

public:
	virtual void OnDefineQuery	(SelectionType nQuerySelection);
	virtual void OnPrepareQuery	(DataObj*, SelectionType nQuerySelection);
};


/////////////////////////////////////////////////////////////////////////////
//	TableUpdater		TURowSecurity
/////////////////////////////////////////////////////////////////////////////
// TableUpdater generico che deve agganciarsi ad una delle tabelle referenziate
// dall'entita' corrente (il Cliente, la Pratica, etc.)
// A seconda del RowSecurityID e del nome entita' letti dalla RS_SubjectsGrants 
// deve andare a modificare i dati dalla tabella di OFM correlata
/////////////////////////////////////////////////////////////////////////////
//-----------------------------------------------------------------------------
class TB_EXPORT TURowSecurity : public TableUpdater
{
	DECLARE_DYNCREATE (TURowSecurity)

private:
	DataLng m_RowSecurityID;

public:
	TURowSecurity(CRuntimeClass* pRecClass, SqlSession* pSession);

protected:
	virtual void	OnDefineQuery	();
	virtual void	OnPrepareQuery	();
	virtual BOOL 	IsEmptyQuery	();

public:
	FindResult FindRecord(const DataLng& nRowSecurityID, BOOL bLock = FALSE);			
	RowSecurityAddOnFields* GetRowSecurityAddOnFields();

public:
	SqlRecord* GetRecord() const { return m_pRecord; }
};


#include "endh.dex"