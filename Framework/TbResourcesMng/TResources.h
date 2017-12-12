#pragma once

#include <TBGes\extdoc.h>
#include <TBGes\hotlink.h>
#include <TbGes\TBLREAD.h>
#include <TbGes\TBLUPDAT.h>

#include "beginh.dex"

#define LEN_RM_DESCRIPTION		32	// Length Descrizione
#define LEN_RM_DESCRI_MULTILINE	256	// Descrizione su più righe

/////////////////////////////////////////////////////////////////////////////
//	SqlRecord				TResources
/////////////////////////////////////////////////////////////////////////////
//
//=============================================================================
class TB_EXPORT TResources : public SqlRecord // (vecchio TCompanyResources)
{
	DECLARE_DYNCREATE(TResources)

public:
	DataStr 	f_ResourceType;
	DataStr 	f_ResourceCode;
	DataStr 	f_Description;
	DataLng 	f_Manager;
	DataStr 	f_Notes;
	DataStr 	f_ImagePath;
	DataStr 	f_CostCenter;
	DataBool	f_Disabled;
	DataBool	f_HideOnLayout;
	DataStr 	f_DomicilyAddress;
	DataStr 	f_DomicilyCity;
	DataStr 	f_DomicilyCounty;
	DataStr 	f_DomicilyZip;
	DataStr 	f_DomicilyCountry;
	DataStr 	f_Telephone1;
	DataStr 	f_Telephone2;
	DataStr 	f_Telephone3;
	DataStr 	f_Telephone4;
	DataStr 	f_Email;
	DataStr 	f_URL;
	DataStr		f_SkypeID;
	DataStr		f_Branch;
	DataStr		f_Latitude;
	DataStr		f_Longitude;
	DataStr		f_Address2;
	DataStr		f_StreetNo;
	DataStr		f_District;
	DataStr		f_FederalState;
	DataStr		f_ISOCountryCode;
	DataStr		l_ManagerDes;

public:
	TResources(BOOL bCallInit = TRUE);

public:
	virtual void	BindRecord	();
	
public:
	static  LPCTSTR  GetStaticName();
};

//=============================================================================
class TB_EXPORT TRResources : public TableReader // (vecchio TRCompanyResources)
{
	DECLARE_DYNAMIC(TRResources)
	
protected:
	DataStr m_ResourceCode;
	DataStr m_ResourceType;

public:
	TRResources(CAbstractFormDoc*	pDocument = NULL);

protected:
	virtual void OnDefineQuery  ();
	virtual void OnPrepareQuery ();
	virtual BOOL IsEmptyQuery	();

public:
	TableReader::FindResult	FindRecord(const DataStr& aResourceCode, const DataStr&	aResourceType);

	void Clear();

	TResources* GetRecord() const
		{
			ASSERT(m_pRecord->IsKindOf(RUNTIME_CLASS(TResources)));
			return (TResources*)m_pRecord;
		}
};

//=============================================================================
//		TableUpdater Definition							
//=============================================================================
//-----------------------------------------------------------------------------
class TB_EXPORT TUResources : public TableUpdater // (vecchio TUCompanyResources)
{
	DECLARE_DYNAMIC(TUResources)
	
public:
	DataStr	m_ResourceType;
	DataStr m_ResourceCode;

public:
	TUResources(CAbstractFormDoc* pDocument = NULL, CMessages* pMessages = NULL);
		
protected:
	virtual void	OnDefineQuery		();
	virtual void	OnPrepareQuery		();
	virtual BOOL 	IsEmptyQuery		();

public:	
	FindResult	FindRecord(const DataStr& aResourceType, const DataStr& aResourceCode, BOOL bLock);	

	TResources* GetRecord() const
	{
		ASSERT(m_pRecord->IsKindOf(RUNTIME_CLASS(TResources)));
		return (TResources*)m_pRecord;
	}
};

/////////////////////////////////////////////////////////////////////////////
//	HotKeyLink				HKLResources
/////////////////////////////////////////////////////////////////////////////
//-----------------------------------------------------------------------------
class TB_EXPORT HKLResources : public HotKeyLink // (vecchio HKLCompanyResources)
{
	DECLARE_DYNCREATE(HKLResources)

public:
	HKLResources();

public:
	enum SelectionDisabled { ACTIVE, DISABLED, BOTH };

public:
	void SetSelDisabled	(SelectionDisabled	SelectionType)	{ m_SelectionDisabled = SelectionType; }
	void SetCodeType	(DataStr			CodeType)		{ m_CodeType = CodeType; }

protected:
	virtual void		OnDefineQuery	(SelectionType nQuerySelection);
	virtual void		OnPrepareQuery	(DataObj*, SelectionType nQuerySelection);
	virtual DataObj*	GetDataObj		() const { return &GetRecord()->f_ResourceCode; };
	virtual	BOOL		Customize		(const DataObjArray&);
	virtual BOOL		IsValid			();
	virtual void		OnCallLink		();

private:
	void DefineDisabled	();
	void PrepareDisabled();

public:
	TResources* GetRecord() const
		{
			ASSERT(m_pRecord->IsKindOf(RUNTIME_CLASS(TResources)));
			return (TResources*)m_pRecord;
		}

protected:
	SelectionDisabled	m_SelectionDisabled;
	DataStr				m_CodeType;
};

/////////////////////////////////////////////////////////////////////////////
//	SqlRecord				TResourcesDetails
/////////////////////////////////////////////////////////////////////////////
//=============================================================================
class TB_EXPORT TResourcesDetails : public SqlRecord //(vecchio TCompanyResourcesDetails)
{
	DECLARE_DYNCREATE(TResourcesDetails)

public:
	DataStr 	f_ResourceType;
	DataStr 	f_ResourceCode;
	DataBool	f_IsWorker; 
	DataStr 	f_ChildResourceType;
	DataStr 	f_ChildResourceCode;
	DataLng 	f_ChildWorkerID;

	DataStr		l_WorkerDesc;
	DataStr		l_ManagerDesc;

public:
	TResourcesDetails(BOOL bCallInit = TRUE);

public:
	virtual void	BindRecord	();
	
public:
	static  LPCTSTR  GetStaticName();
};

//=============================================================================
//		TableUpdater Definition							
//=============================================================================
//-----------------------------------------------------------------------------
class TB_EXPORT TUResourcesDetails : public TableUpdater // (vecchio TUCompanyResourcesDetails) 
{
	DECLARE_DYNAMIC(TUResourcesDetails)
	
public:
	DataStr 	m_ResourceType;
	DataStr 	m_ResourceCode;
	DataBool	m_IsWorker;
	DataStr 	m_ChildResourceType;
	DataStr 	m_ChildResourceCode;
	DataLng 	m_ChildWorkerID;	

public:
	TUResourcesDetails(CAbstractFormDoc* pDocument = NULL, CMessages* pMessages = NULL);
		
protected:
	virtual void	OnDefineQuery		();
	virtual void	OnPrepareQuery		();
	virtual BOOL 	IsEmptyQuery		();

public:	
	FindResult	FindRecord	(const DataStr&		aResourceType,		// General resource
							 const DataStr&		aResourceCode, 
							 const DataBool&	aIsWorker,
							 const DataStr&		aChildResourceType, 
							 const DataStr&		aChildResourceCode, 
							 const DataLng&		aChildWorkerID, 
								   BOOL			bLock);

	FindResult	FindRecord	(const DataStr&		aResourceType,		// Not Worker
							 const DataStr&		aResourceCode, 
							 const DataStr&		aChildResourceType, 
							 const DataStr&		aChildResourceCode, 
								   BOOL			bLock);	

	FindResult	FindRecord	(const DataStr&		aResourceType,		// Worker
							 const DataStr&		aResourceCode, 
							 const DataLng&		aChildWorkerID, 
								   BOOL			bLock);	
	
	TResourcesDetails* GetRecord() const
	{
		ASSERT(m_pRecord->IsKindOf(RUNTIME_CLASS(TResourcesDetails)));
		return (TResourcesDetails*)m_pRecord;
	}
};

/////////////////////////////////////////////////////////////////////////////
//	SqlRecord				TResourcesFields
/////////////////////////////////////////////////////////////////////////////
//
//=============================================================================
class TB_EXPORT TResourcesFields : public SqlRecord // (vecchio TCompanyResourcesFields)
{
	DECLARE_DYNCREATE(TResourcesFields)

public:
	DataStr 	f_ResourceType;
	DataStr 	f_ResourceCode;
	DataInt 	f_Line;
	DataStr 	f_FieldName;
	DataStr 	f_FieldValue;
	DataStr 	f_Notes;
	DataBool	f_HideOnLayout;

public:
	TResourcesFields(BOOL bCallInit = TRUE);

public:
	virtual void	BindRecord	();
	
public:
	static  LPCTSTR  GetStaticName();
};

/////////////////////////////////////////////////////////////////////////////
//	SqlRecord				TResourcesAbsences
/////////////////////////////////////////////////////////////////////////////
//
//=============================================================================
class TB_EXPORT TResourcesAbsences : public SqlRecord // (vecchio TCompanyResourcesBreakdown)
{
	DECLARE_DYNCREATE(TResourcesAbsences)

public:
	DataStr 	f_ResourceType;
	DataStr 	f_ResourceCode;
	DataStr 	f_Reason;
	DataDate	f_StartingDate;
	DataDate	f_EndingDate;
	DataLng 	f_Manager;
	DataStr		f_Notes;

	DataStr		l_ManagerDesc;

public:
	TResourcesAbsences(BOOL bCallInit = TRUE);

public:
	virtual void	BindRecord	();
	
public:
	static  LPCTSTR  GetStaticName();
};

/////////////////////////////////////////////////////////////////////////////
//	SqlRecord				TResourceTypes
/////////////////////////////////////////////////////////////////////////////
//
//=============================================================================
class TB_EXPORT TResourceTypes : public SqlRecord
{
	DECLARE_DYNCREATE(TResourceTypes)

public:
	DataStr 	f_ResourceType;
	DataStr 	f_Description;
	DataStr		f_ImagePath;

public:
	TResourceTypes(BOOL bCallInit = TRUE);

public:
	virtual void	BindRecord();

public:
	static  LPCTSTR  GetStaticName();
};

/////////////////////////////////////////////////////////////////////////////
//	Hotlink					### ResourceTypes ###	
/////////////////////////////////////////////////////////////////////////////
//
class TB_EXPORT HKLResourceTypes : public HotKeyLink
{
	DECLARE_DYNCREATE(HKLResourceTypes)

public:
	HKLResourceTypes();

protected:
	virtual void		OnDefineQuery	(SelectionType nQuerySelection);
	virtual void		OnPrepareQuery	(DataObj*, SelectionType nQuerySelection);
	virtual DataObj*	GetDataObj		() const { return &(GetRecord()->f_ResourceType); };

public:
	// local useful function
	TResourceTypes* GetRecord() const
	{
		ASSERT(m_pRecord->IsKindOf(RUNTIME_CLASS(TResourceTypes)));
		return (TResourceTypes*)m_pRecord;
	}
};

/////////////////////////////////////////////////////////////////////////////
//	TableReader				### ResourceTypes ###			TRResourceTypes
/////////////////////////////////////////////////////////////////////////////
//
class TB_EXPORT TRResourceTypes : public TableReader
{
	DECLARE_DYNAMIC(TRResourceTypes)

public:
	DataStr m_ResourceType;

public:
	TRResourceTypes(CAbstractFormDoc* pDocument = NULL);

protected:
	virtual void OnDefineQuery	();
	virtual void OnPrepareQuery	();
	virtual BOOL IsEmptyQuery	();

public:
	// local useful function
	TableReader::FindResult FindRecord(const DataStr& aResourceType);

	TResourceTypes* GetRecord() const
	{
		ASSERT(m_pRecord->IsKindOf(RUNTIME_CLASS(TResourceTypes)));
		return (TResourceTypes*)m_pRecord;
	}
};

//=============================================================================
class TB_EXPORT RRResourcesByType : public RowsetReader
{
	DECLARE_DYNAMIC(RRResourcesByType)

protected:
	DataStr m_ResourceType;

public:
	RRResourcesByType(CAbstractFormDoc*	pDocument = NULL);

protected:
	virtual void OnDefineQuery();
	virtual void OnPrepareQuery();
	virtual BOOL IsEmptyQuery();

public:
	TableReader::FindResult	FindRecord(const DataStr& aResourceType);

	TResources* GetRecord() const
	{
		ASSERT(m_pRecord->IsKindOf(RUNTIME_CLASS(TResources)));
		return (TResources*)m_pRecord;
	}
};

#include "endh.dex"
