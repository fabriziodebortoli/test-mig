#pragma once

#include <TbGenlib\\NumbererInfo.h>
#include <TbGes\ExtDocAbstract.h>

#include "beginh.dex"

class SqlTable;
class SqlRecord;

////////////////////////////////////////////////////////////////////////////////
///							CNumbererServiceObj
////////////////////////////////////////////////////////////////////////////////
//
//-----------------------------------------------------------------------------
class TB_EXPORT CNumbererServiceObj : public CObject, public INumbererService
{
protected:
	CAbstractFormDoc*	m_pDocument;
	SqlSession*			m_pSqlSession;
	bool				m_bOpenedInUpdate;

protected:
	CNumbererServiceObj(SqlSession* pSession = NULL);
	~CNumbererServiceObj();

public:
	virtual void OnInitService	();

	// INumbererService interface default implementation
	virtual bool	ReadInfo	(IBehaviourRequest* pRequest);
	virtual bool	ReadNumber  (IBehaviourRequest* pRequest);
			bool	WriteNumber	(IBehaviourRequest* pRequest);

	virtual CArray<CString>* GetLinkedControls() { return NULL; }
protected:
	SqlSession*	GetSqlSession	(bool bIsUpdatable = false);
	void		AssignNumber	(DataObj* pData, const DataLng& aNewNumber, DataDate* pDataDate = NULL);
	CFormatMask	GetFormatMask	();

protected:
	virtual const bool	CanPerformNumbering	(CNumbererRequest* pRequest) const;

	// pure virtual methods
	virtual	CString		GetFormatMaskField	() = 0;
	virtual bool		OnReadNextNumber	(CNumbererRequest* pRequest, DataLng& aNextNumber) = 0;
	virtual bool		OnWriteNextNumber	(CNumbererRequest* pRequest, DataLng& aNextNumber) = 0;
};

////////////////////////////////////////////////////////////////////////////////
///						TAutoincrementEntities
////////////////////////////////////////////////////////////////////////////////
//
//-----------------------------------------------------------------------------
class TB_EXPORT TAutoincrementEntities : public SqlRecord
{
	DECLARE_DYNCREATE(TAutoincrementEntities)

public:
	DataStr		f_Entity;
	DataLng		f_LastNumber;

public:
	TAutoincrementEntities(BOOL bCallInit = TRUE);

public:
	virtual void	BindRecord();

public:
	static LPCTSTR GetStaticName();
};

////////////////////////////////////////////////////////////////////////////////
///							CAutoincrementService
////////////////////////////////////////////////////////////////////////////////
//
//-----------------------------------------------------------------------------
class TB_EXPORT CAutoincrementService : public CNumbererServiceObj
{
	DECLARE_DYNCREATE(CAutoincrementService)

	SqlTable*				m_pEntityTable;
	TAutoincrementEntities	m_Entities;

public:
	CAutoincrementService (SqlSession* pSession = NULL);
	~CAutoincrementService();

	static LPCTSTR GetStaticName() { return _T("Framework.TbGes.TbGes.Autoincrement"); }

public:
	// metodi derivati da IBehaviourService
	virtual bool CanExecuteRequest(const BehaviourEvents& evnt, IBehaviourRequest* pRequest);
	virtual bool IsCompatibleWith (IBehaviourRequest* pRequest);

	DECLARE_BEHAVIOUR_EVENTMAP()

	// metodi reimplementabili dai servizi derivati
	virtual bool	OnReadNextNumber (CNumbererRequest* pRequest, DataLng& aNextNumber);
	virtual bool	OnWriteNextNumber(CNumbererRequest* pRequest, DataLng& aNextNumber);

	// INumbererService Interface
	virtual bool ReadInfo		(IBehaviourRequest* pRequest);

protected:
	// metodi usati dai servizi derivati
	bool	ReadNextNumber	(CNumbererRequest* pRequest, DataLng& aNextNumber);
	bool	WriteNextNumber	(CNumbererRequest* pRequest, DataLng& aNextNumber);
	bool	RestoreNumber	(CNumbererRequest* pRequest, bool useOldData);

private:
	// gestione della query
	void		OpenTables				(bool bIsUpdatable = false);
	void		CloseTables				();
	
	bool	ReadInfo				(CNumbererRequest* pRequest, bool bForUpdate, bool useOldData);

	// gestione della formattazione
	virtual CString GetFormatMaskField	();
	const bool	CanPerformNumbering		(CNumbererRequest* pRequest) const;
	
	// gestione degli eventi gestiti
	bool	OnDeleteTransaction	(IBehaviourRequest* pRequest);
	bool	OnBeforeEscape		(IBehaviourRequest* pRequest);
	bool	OnLockDocumentForNew(IBehaviourRequest* pRequest);
	bool	OnFormModeChanged	(IBehaviourRequest* pRequest);

public:
	// metodi a disposizione dei programmatori per batch e oggetti non di documento
	bool	GetNextNumber(const CString& strEntity, DataObj* pDataObj);
};

////////////////////////////////////////////////////////////////////////////////
///							TAutonumberEntities
////////////////////////////////////////////////////////////////////////////////
//
//-----------------------------------------------------------------------------
class TB_EXPORT TAutonumberEntities : public SqlRecord
{
	DECLARE_DYNCREATE(TAutonumberEntities)

public:
	DataStr		f_Entity;
	DataStr		f_FormattedMask;
	DataBool	f_IsYearEntity;
	DataLng		f_LastNumber;
	DataBool	f_Disabled;

public:
	TAutonumberEntities(BOOL bCallInit = TRUE);

public:
	virtual void	BindRecord();

public:
	static LPCTSTR GetStaticName();
};

////////////////////////////////////////////////////////////////////////////////
///						TAutonumberEntitiesYears
////////////////////////////////////////////////////////////////////////////////
//
//-----------------------------------------------------------------------------
class TB_EXPORT TAutonumberEntitiesYears : public SqlRecord
{
	DECLARE_DYNCREATE(TAutonumberEntitiesYears)

public:
	DataStr		f_Entity;
	DataInt		f_Year;
	DataLng		f_LastNumber;	

public:
	TAutonumberEntitiesYears (BOOL bCallInit = TRUE);

public:
	virtual void	BindRecord();

public:
	static LPCTSTR GetStaticName();
};

////////////////////////////////////////////////////////////////////////////////
///							CAutonumberService
////////////////////////////////////////////////////////////////////////////////
//
//-----------------------------------------------------------------------------
class TB_EXPORT CAutonumberService : public CNumbererServiceObj
{
	DECLARE_DYNCREATE(CAutonumberService)

	SqlTable*					m_pEntityTable;
	SqlTable*					m_pEntityYearsTable;
	TAutonumberEntities			m_Entities;
	TAutonumberEntitiesYears	m_EntitiesYears;

public:
	CAutonumberService(SqlSession* pSession = NULL);
	~CAutonumberService();

	static LPCTSTR GetStaticName() { return _T("Framework.TbGes.TbGes.Autonumber"); }

public:
	// metodi derivati da IBehaviourService
	virtual bool CanExecuteRequest	(const BehaviourEvents& evnt, IBehaviourRequest* pRequest);
	virtual bool IsCompatibleWith	(IBehaviourRequest* pRequest);

	DECLARE_BEHAVIOUR_EVENTMAP()

	// metodi reimplementabili dai servizi derivati
	virtual bool	OnReadNextNumber (CNumbererRequest* pRequest, DataLng& aNextNumber);
	virtual bool	OnWriteNextNumber(CNumbererRequest* pRequest, DataLng& aNextNumber);

	// INumbererService Interface
	virtual bool ReadInfo(IBehaviourRequest* pRequest);

protected:
	// metodi usati dai servizi derivati
	bool	ReadNextNumber	(CNumbererRequest* pRequest, DataLng& aNextNumber);
	bool	WriteNextNumber	(CNumbererRequest* pRequest, DataLng& aNextNumber);
	bool	RestoreNumber	(CNumbererRequest* pRequest, bool useOldData);

private:
	// gestione della query
	void	OpenTables			(bool bIsUpdatable = false);
	void	OpenYearsTables		(bool bIsUpdatable = false);
	void	CloseTables			();
	
	bool	ReadInfo				(CNumbererRequest* pRequest, bool bForUpdate, bool useOldData);
	void	GetDataFromPreviousYear	(TAutonumberEntitiesYears* pRecEntityYear);

	// gestione della formattazione
	virtual CString GetFormatMaskField();
	const bool	CanPerformNumbering	  (CNumbererRequest* pRequest) const;
	
	CDateNumbererRequestParams*	GetDateParams (CNumbererRequest* pRequest);

	// gestione degli eventi gestiti
	bool	OnFormModeChanged	(IBehaviourRequest* pRequest);
	bool	OnLockDocumentForNew(IBehaviourRequest* pRequest);
	bool	OnDeleteTransaction	(IBehaviourRequest* pRequest);

public:
	// metodi a disposizione dei programmatori per batch e oggetti non di documento
	bool	GetNextNumber(const CString& strEntity, DataObj* pDataObj, DataDate* pDataDate);
	bool	SetNextNumber(const CString& strEntity, DataObj* pDataObj, DataDate* pDataDate);
};

////////////////////////////////////////////////////////////////////////////////
// Classe generale per apportare i metodi automatici di binding dei numeratori
////////////////////////////////////////////////////////////////////////////////
//-----------------------------------------------------------------------------
class TB_EXPORT CNumbererBinder : public IBehaviourConsumer
{
public:
	CNumbererBinder();

	CNumbererRequest*	BindAutoincrement	(CObject* pOwner, DataObj* pDataBinding, const CString& sEntity);
	void				BindAutoincrement	(CNumbererRequest* pRequest);
	void				BindAutonumber		(CNumbererRequest* pRequest, CNumbererRequestParams* pParams = NULL);
	void				BindAutonumber		(CObject* pOwner, DataObj* pDataBinding, const CString& sEntity);
	void				EnableNumberer(DataObj* pDataBinding, const BOOL bValue = TRUE);
};

#include "endh.dex"
