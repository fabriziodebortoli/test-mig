#pragma once

#include <TBOleDB\sqltable.h>
#include <TBGes\hotlink.h>
#include <TbGes\TBLREAD.h>
#include <TbGes\TBLUPDAT.h>

#include "beginh.dex"

//TODOMICHI: questi a cosa servono?? 
//controllare se c'erano dei webmethod ad-hoc da rigenerare
//o servivano solo a TB per conoscere i workers e non servono piu'?
DataLng GetLoggedWorkerID();
DataStr GetWorkerName(DataLng WorkerID);
DataStr GetLoggedWorkerName();

/////////////////////////////////////////////////////////////////////////////
//	SqlRecord				TWorkers
/////////////////////////////////////////////////////////////////////////////
//
//=============================================================================
class TB_EXPORT TWorkers : public SqlRecord
{
	DECLARE_DYNCREATE(TWorkers) 

public:
	DataLng		f_WorkerID;
	DataStr 	f_Title;
	DataStr 	f_Name;
	DataStr 	f_LastName;
	DataEnum 	f_Gender;
	DataStr 	f_DomicilyAddress;
	DataStr 	f_DomicilyCity;
	DataStr 	f_DomicilyCounty;
	DataStr 	f_DomicilyZip;
	DataStr 	f_DomicilyCountry;
	DataStr 	f_DomicilyFC;
	DataStr 	f_DomicilyISOCode;
	DataStr 	f_Telephone1;
	DataStr 	f_Telephone2;
	DataStr 	f_Telephone3;
	DataStr 	f_Telephone4;
	DataStr 	f_Email;
	DataStr 	f_URL;
	DataStr		f_SkypeID;
	DataStr 	f_CostCenter;
	DataMon		f_HourlyCost;
	DataStr		f_Notes;
	DataDate	f_DateOfBirth;
	DataStr 	f_CityOfBirth;
	DataStr 	f_CivilStatus;
	DataStr 	f_RegisterNumber;
	DataDate	f_EmploymentDate;
	DataDate	f_ResignationDate;
	DataStr		f_ImagePath;
	DataBool	f_Disabled;
	DataBool	f_HideOnLayout;
	DataStr 	f_Password;
	DataBool 	f_PasswordMustBeChanged;
	DataBool 	f_PasswordCannotChange;
	DataBool 	f_PasswordNeverExpire;
	DataBool 	f_PasswordNotRenewable;
	DataDate	f_PasswordExpirationDate;
	DataInt		f_PasswordAttemptsNumber;
	DataStr 	f_CompanyLogin;
	DataStr		f_Latitude;
	DataStr		f_Longitude;
	DataStr		f_PIN;
	DataStr		f_Branch;
	DataStr		f_Address2;
	DataStr		f_StreetNo;
	DataStr		f_District;
	DataStr		f_FederalState;
	DataBool	f_IsRSEnabled;
	
	DataStr		l_WorkerDesc;
	DataStr		l_NameComplete;
	DataStr		l_NameCompleteWithLastNameFirst;

public:
	TWorkers(BOOL bCallInit = TRUE) ;

public:
	virtual void	BindRecord	();
	
public:
	static  LPCTSTR  GetStaticName();
};

//=============================================================================
//					TableReader Definition
//=============================================================================
//-----------------------------------------------------------------------------
class TB_EXPORT TRWorkers : public TableReader
{
	DECLARE_DYNAMIC(TRWorkers)
	
public:
	DataLng m_WorkerID;
	
public:
	TRWorkers(CAbstractFormDoc* pDocument = NULL);

protected:
	virtual void	OnDefineQuery	();
	virtual void	OnPrepareQuery	();
	virtual BOOL 	IsEmptyQuery	();

public:
	FindResult FindRecord(DataLng& aWorkerID);
	DataStr GetNameComplete();
	TWorkers* GetRecord() const
		{
			ASSERT(m_pRecord->IsKindOf(RUNTIME_CLASS(TWorkers)));
			return (TWorkers*) m_pRecord;
		}
	
	DataStr GetWorker();
};

//=============================================================================
//					TableReader Definition
//=============================================================================
//-----------------------------------------------------------------------------
class TB_EXPORT TRWorkersByLogin : public TableReader
{
	DECLARE_DYNAMIC(TRWorkersByLogin)

public:
	DataStr m_CompanyLogin;
	DataLng m_WorkerID;

public:
	TRWorkersByLogin(CAbstractFormDoc* pDocument = NULL);

	void ExcludeWorkerID(DataLng aWorkerID) { m_WorkerID = aWorkerID; }

protected:
	virtual void	OnDefineQuery();
	virtual void	OnPrepareQuery();
	virtual BOOL 	IsEmptyQuery();

public:
	FindResult FindRecord(DataStr& aWorkerLogin);

	TWorkers* GetRecord() const
	{
		ASSERT(m_pRecord->IsKindOf(RUNTIME_CLASS(TWorkers)));
		return (TWorkers*)m_pRecord;
	}
};

//=============================================================================
//					TableReader Definition
//=============================================================================
//-----------------------------------------------------------------------------
class TB_EXPORT TRWorkersByName : public TableReader
{
	DECLARE_DYNAMIC(TRWorkersByName)
	
public:
	DataStr m_Name;
	DataStr m_LastName;
	DataLng m_WorkerID;
	
public:
	TRWorkersByName(CAbstractFormDoc* pDocument = NULL);

	void ExcludeWorkerID(DataLng aWorkerID) { m_WorkerID = aWorkerID; }

protected:
	virtual void	OnDefineQuery	();
	virtual void	OnPrepareQuery	();
	virtual BOOL 	IsEmptyQuery	();

public:
	FindResult FindRecord(DataStr& aName, DataStr& aLastName);
	
	TWorkers* GetRecord() const
		{
			ASSERT(m_pRecord->IsKindOf(RUNTIME_CLASS(TWorkers)));
			return (TWorkers*) m_pRecord;
		}
};

//=============================================================================
//					TableReader Definition
//=============================================================================
//-----------------------------------------------------------------------------
class TB_EXPORT TRWorkersByPIN : public TableReader
{
	DECLARE_DYNAMIC(TRWorkersByPIN)
	
public:
	DataStr m_PIN;
	DataLng m_WorkerID;
public:
	TRWorkersByPIN(CAbstractFormDoc* pDocument = NULL);

	void ExcludeWorkerID(DataLng aWorkerID) { m_WorkerID = aWorkerID; }

protected:
	virtual void	OnDefineQuery	();
	virtual void	OnPrepareQuery	();
	virtual BOOL 	IsEmptyQuery	();

public:
	FindResult FindRecord(DataStr& aPIN);
	
	TWorkers* GetRecord() const
		{
			ASSERT(m_pRecord->IsKindOf(RUNTIME_CLASS(TWorkers)));
			return (TWorkers*) m_pRecord;
		}
};

//=============================================================================
//		TableUpdater Definition							
//=============================================================================
//-----------------------------------------------------------------------------
class TB_EXPORT TUWorkers : public TableUpdater
{
	DECLARE_DYNAMIC(TUWorkers)
	
public:
	DataLng m_WorkerID;

public:
	TUWorkers (CAbstractFormDoc* pDocument = NULL, CMessages* pMessages = NULL);
		
protected:
	virtual void	OnDefineQuery		();
	virtual void	OnPrepareQuery		();
	virtual BOOL 	IsEmptyQuery		();

public:	
	FindResult FindRecord(DataLng& aWorkerID, 
						  BOOL	   bLock);	
	TWorkers* GetRecord() const
	{
		ASSERT(m_pRecord->IsKindOf(RUNTIME_CLASS(TWorkers)));
		return (TWorkers*) m_pRecord;
	}
};

//=============================================================================
//		HotKeyLink Definition							
//=============================================================================
class TB_EXPORT HKLWorkers : public HotKeyLink
{
	DECLARE_DYNCREATE (HKLWorkers)

public:
	  HKLWorkers();

public:
	enum SelectionDisabled { ACTIVE, DISABLED, BOTH };

public:
	DataLng		m_WorkerID;
	DataStr		m_ResourceType;
	DataStr		m_ResourceCode;

protected:
	SelectionDisabled	m_SelectionDisabled;

protected:
	  virtual void		OnDefineQuery	(SelectionType nQuerySelection = DIRECT_ACCESS);
	  virtual void		OnPrepareQuery	(DataObj*, SelectionType nQuerySelection = DIRECT_ACCESS);
	  virtual DataObj*	GetDataObj		() const { return &(GetRecord()->f_WorkerID); };
	  virtual BOOL		IsValid			();
	  virtual BOOL		ExistData		(DataObj* pDataObj);
	  virtual BOOL		Customize		(const DataObjArray&);
	  virtual void		OnPrepareAuxData();

	  virtual CString	GetHKLDescription () const ;

private:
	void DefineDisabled();
	void PrepareDisabled();

public:
	virtual FindResult FindRecord(DataObj*, BOOL bCallLink = FALSE, BOOL bFromControl = FALSE, BOOL bAllowRunningModeForInternalUse = FALSE);

	  // local useful function
	void		SetSelDisabled		(SelectionDisabled	SelectionType)	{ m_SelectionDisabled = SelectionType; }
	void		SetWorkerID			(const DataLng& aWorkerID) { m_WorkerID = aWorkerID; }
	void		SetResource			(const DataStr& aResourceType, const DataStr& aResourceCode); 
	DataStr		GetNameComplete		(BOOL bNameFirst = TRUE) const;
	DataStr		GetAddressComplete	() const;

	TWorkers*	GetRecord			() const
			{
				  ASSERT(m_pRecord->IsKindOf(RUNTIME_CLASS(TWorkers)));
				  return (TWorkers*) m_pRecord;
			}
};

/////////////////////////////////////////////////////////////////////////////
//	SqlRecord				TWorkersDetails
/////////////////////////////////////////////////////////////////////////////
//=============================================================================
class TB_EXPORT TWorkersDetails : public SqlRecord 
{
	DECLARE_DYNCREATE(TWorkersDetails)

public:
	DataLng 	f_WorkerID; 
	DataBool	f_IsWorker;
	DataStr 	f_ChildResourceType;
	DataStr 	f_ChildResourceCode;
	DataLng 	f_ChildWorkerID;

	DataStr		l_WorkerDesc;
	DataStr		l_ManagerDesc;

public:
	TWorkersDetails(BOOL bCallInit = TRUE);

public:
	virtual void	BindRecord();

public:
	static  LPCTSTR  GetStaticName();
};

//=============================================================================
//		TableUpdater Definition							
//=============================================================================
//-----------------------------------------------------------------------------
class TB_EXPORT TUWorkersDetails : public TableUpdater
{
	DECLARE_DYNAMIC(TUWorkersDetails)

public:
	DataLng 	m_WorkerID;
	DataBool 	m_IsWorker;
	DataStr 	m_ChildResourceType;
	DataStr 	m_ChildResourceCode;
	DataLng 	m_ChildWorkerID;

public:
	TUWorkersDetails(CAbstractFormDoc* pDocument = NULL, CMessages* pMessages = NULL);

protected:
	virtual void	OnDefineQuery	();
	virtual void	OnPrepareQuery	();
	virtual BOOL 	IsEmptyQuery	();

public:
	FindResult	FindRecord(	const DataLng&	aWorkerID,
							const DataBool&	aIsWorker,
							const DataStr&	aChildResourceType,
							const DataStr&	aChildResourceCode,
							const DataLng&	aChildWorkerID,
							BOOL			bLock);
	FindResult	FindRecord(	const DataLng&	aWorkerID,
							const DataStr&	aChildResourceType,
							const DataStr&	aChildResourceCode,
							BOOL			bLock);
	FindResult	FindRecord(	const DataLng&	aWorkerID,
							const DataLng&	aChildWorkerID,
							BOOL			bLock);

	TWorkersDetails* GetRecord() const
	{
		ASSERT(m_pRecord->IsKindOf(RUNTIME_CLASS(TWorkersDetails)));
		return (TWorkersDetails*)m_pRecord;
	}
};

/////////////////////////////////////////////////////////////////////////////
//	SqlRecord				TWorkersFields
/////////////////////////////////////////////////////////////////////////////
//
//=============================================================================
class TB_EXPORT TWorkersFields : public SqlRecord
{
	DECLARE_DYNCREATE(TWorkersFields) 

public:
	DataLng		f_WorkerID;
	DataInt 	f_Line;
	DataStr 	f_FieldName;
	DataStr 	f_FieldValue;
	DataStr 	f_Notes;
	DataBool	f_HideOnLayout;

public:
	TWorkersFields(BOOL bCallInit = TRUE) ;

public:
	virtual void	BindRecord	();
	
public:
	static  LPCTSTR  GetStaticName();
};

/////////////////////////////////////////////////////////////////////////////
//	SqlRecord				TWorkersArrangement
/////////////////////////////////////////////////////////////////////////////
//
//=============================================================================
class TB_EXPORT TWorkersArrangement : public SqlRecord
{
	DECLARE_DYNCREATE(TWorkersArrangement) 

public:
	DataLng		f_WorkerID;
	DataInt 	f_Line;
	DataStr 	f_Arrangement;
	DataStr 	f_ArrangementLevel;
	DataMon 	f_BasicPay;
	DataMon 	f_TotalPay;
	DataDate	f_FromDate;
	DataDate	f_ToDate;
	DataStr 	f_Notes;

public:
	TWorkersArrangement(BOOL bCallInit = TRUE) ;

public:
	virtual void	BindRecord	();
	
public:
	static  LPCTSTR  GetStaticName();
};

/////////////////////////////////////////////////////////////////////////////
//	SqlRecord				TWorkersAbsences
/////////////////////////////////////////////////////////////////////////////
//
//=============================================================================
class TB_EXPORT TWorkersAbsences : public SqlRecord  // (vecchia TWorkersBreakdown)
{
	DECLARE_DYNCREATE(TWorkersAbsences)

public:
	DataLng 	f_WorkerID;
	DataStr 	f_Reason;
	DataDate	f_StartingDate;
	DataDate	f_EndingDate;
	DataLng 	f_Manager;
	DataStr		f_Notes;

	DataStr		l_ManagerDes;

public:
	TWorkersAbsences(BOOL bCallInit = TRUE);

public:
	virtual void	BindRecord	();
	
public:
	static  LPCTSTR  GetStaticName();
};

/////////////////////////////////////////////////////////////////////////////
//	SqlRecord				TArrangements
/////////////////////////////////////////////////////////////////////////////
//
//=============================================================================
class TB_EXPORT TArrangements : public SqlRecord
{
	DECLARE_DYNCREATE(TArrangements) 

public:
	DataStr 	f_Arrangements;
	DataStr 	f_Description;
	DataStr		f_ArrangementLevel;
	DataMon		f_BasicPay;
	DataMon		f_TotalPay;
	DataLng 	f_WorkingHours;
	DataStr		f_Notes;

public:
	TArrangements(BOOL bCallInit = TRUE) ;

public:
	virtual void	BindRecord	();
	
public:
	static  LPCTSTR  GetStaticName();
};

//=============================================================================
class TB_EXPORT HKLArrangements : public HotKeyLink
{
	DECLARE_DYNCREATE (HKLArrangements)

public:
	HKLArrangements();

protected:
	virtual void		OnDefineQuery	(SelectionType nQuerySelection);
	virtual void		OnPrepareQuery	(DataObj*, SelectionType nQuerySelection);
	virtual DataObj*	GetDataObj		() const { return &GetRecord()->f_Arrangements; };

public:
	TArrangements* GetRecord () const
		{
			ASSERT(m_pRecord->IsKindOf(RUNTIME_CLASS(TArrangements)));
			return (TArrangements*) m_pRecord;                              
		}
};

#include "endh.dex"
