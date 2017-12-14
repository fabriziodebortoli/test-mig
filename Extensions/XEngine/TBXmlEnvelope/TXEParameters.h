#pragma once

#include <TBOleDB\sqlrec.h>
#include <TBOleDB\sqltable.h>

#include <TBGes\tblread.h>
#include <TBGeneric\dataobj.h>

//includere alla fine degli include del .H
#include "beginh.dex"

//----------------------------------------------------------------------------
//numero max di documenti da esportare in un file
#define HEADER_MIN_PADDING_NUM			0
#define HEADER_MAX_PADDING_NUM			4
#define HEADER_DEFAULT_PADDING_NUM		4

#define LEN_SITE_CODE  4

/////////////////////////////////////////////////////////////////////////////
//	class TXEParameters (per la  gestione dei parametri di XEngine)
/////////////////////////////////////////////////////////////////////////////
//--------------------------------------------------------------------------
class TB_EXPORT TXEParameters : public SqlRecord
{
	DECLARE_DYNCREATE(TXEParameters);

public:
	DataInt 	f_IdParam;
	DataStr		f_DomainName;
	DataStr		f_SiteName;
	DataStr		f_SiteCode;
	DataBool	f_EncodTypeUTF8;
	DataStr		f_ImportPath;
	DataStr		f_ExportPath;
	DataInt		f_MaxDoc; 			// 10
	DataInt		f_MaxKByte; 		// 100
	DataBool	f_UseEnvClassExt;	// di default FALSE
	DataInt		f_EnvPaddingNum;	// 4
	DataBool	f_UseAttribute;		// di default FALSE
	DataBool	f_UseEnumAsNum;		// di default TRUE
	
public:
	TXEParameters(BOOL bCallInit = TRUE);

public:
	virtual void	Init		();
	virtual void	BindRecord	();

public:
	static LPCTSTR GetStaticName();

private:
	BOOL GetDefaultForUseUTF8();
};

/////////////////////////////////////////////////////////////////////////////
//	TableReader				### PARAMETRI XENGINE ###				
/////////////////////////////////////////////////////////////////////////////
//--------------------------------------------------------------------------
class TB_EXPORT TRXEParameters : public TableReader
{
	DECLARE_DYNAMIC(TRXEParameters)
	
public:
	TRXEParameters(CAbstractFormDoc* pDocumentc);

public:
	TXEParameters* GetRecord() const
		{
			ASSERT(m_pRecord->IsKindOf(RUNTIME_CLASS(TXEParameters)));
			return (TXEParameters*) m_pRecord;
		}

protected:
	virtual void	OnDefineQuery	();
	virtual void	OnPrepareQuery	();
	virtual BOOL 	IsEmptyQuery	();

public:

// Diagnostics
#ifdef _DEBUG
	void Dump		(CDumpContext&)	const;
	void AssertValid()				const;
#endif // _DEBUG
};

#include "endh.dex"