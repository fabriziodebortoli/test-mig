
#pragma once

#include <TBOleDB\sqlrec.h>
#include <TBOleDB\sqltable.h>

#include <TBGes\tblread.h>
#include <TBGes\tblupdat.h>
#include <TBGeneric\dataobj.h>

#include "beginh.dex"

/////////////////////////////////////////////////////////////////////////////
//						class TExtGuid definition
/////////////////////////////////////////////////////////////////////////////
//
//=============================================================================
class TB_EXPORT TExtGuid : public SqlRecord
{
	DECLARE_DYNCREATE(TExtGuid) 

public:
	DataGuid		f_DocGuid;

public:
	TExtGuid(BOOL bCallInit = TRUE);

public:
	virtual void	BindRecord	();	
public:
	static LPCTSTR 	GetStaticName();
};

/////////////////////////////////////////////////////////////////////////////
//					class TRExtGuid definition
/////////////////////////////////////////////////////////////////////////////
//
//=============================================================================
class TB_EXPORT TRExtGuid : public TableReader
{
	DECLARE_DYNAMIC(TRExtGuid)
	
public:
	DataGuid 	m_DocGuid;
	
public:
	TRExtGuid (CAbstractFormDoc* pDocument = NULL);

protected:
	virtual void	OnDefineQuery	();
	virtual void	OnPrepareQuery	();
	virtual BOOL 	IsEmptyQuery	();

public:
	TableReader::FindResult FindRecord(const DataGuid& aDocGuid);
	TExtGuid* GetRecord() const
		{
			ASSERT(m_pRecord->IsKindOf(RUNTIME_CLASS(TExtGuid)));
			return (TExtGuid*) m_pRecord;
		}
};

/////////////////////////////////////////////////////////////////////////////
//						class TUExtGuid definition
/////////////////////////////////////////////////////////////////////////////
//
//=============================================================================
class TB_EXPORT TUExtGuid : public TableUpdater
{
	DECLARE_DYNAMIC(TUExtGuid)
	
public:
	DataGuid		m_DocGuid;
	
public:
	TUExtGuid (CAbstractFormDoc* pDocument = NULL, CDiagnostic* pDiagnostic = NULL);

protected:
	virtual void	OnDefineQuery	();
	virtual void	OnPrepareQuery	();
	virtual BOOL 	IsEmptyQuery	();

public:
	TExtGuid* GetRecord() const
		{
			ASSERT(m_pRecord->IsKindOf(RUNTIME_CLASS(TExtGuid)));
			return (TExtGuid*) m_pRecord;
		}
	FindResult FindRecord(const DataGuid& aExtGuidId, BOOL bLock = FALSE); 	
};

#include "endh.dex"
