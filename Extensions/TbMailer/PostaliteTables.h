#pragma once 

#include <TbGeneric\DataObj.h>
#include <tboledb\sqlrec.h>
#include <TbGes\TBLUPDAT.H>

//includere alla fine degli include del .H
#include "beginh.dex"

class SqlConnection;
///////////////////////////////////////////////////////////////////////////////
//								TMsgQueue Record definition
///////////////////////////////////////////////////////////////////////////////
class TB_EXPORT TMsgQueue : public SqlRecord
{
	DECLARE_DYNCREATE(TMsgQueue) 
public:
	DataLng		f_MsgID;
	DataLng		f_LotID;
	DataStr		f_Fax;							
	DataStr		f_Addressee;
	DataStr		f_Address;
	DataStr		f_ZipCode;
	DataStr		f_City;
	DataStr		f_County;
	DataStr		f_Country;

	DataStr		f_Subject;

	DataStr		f_DocNamespace;
	DataStr		f_DocPrimaryKey;
	DataStr		f_AddresseeNamespace;
	DataStr		f_AddresseePrimaryKey;

	DataStr		f_DocFileName;
	DataLng		f_DocPages;
	DataLng		f_DocSize;
	//DataBlob	f_DocImage
	DataInt		f_DeliveryType;
	DataInt		f_PrintType;
public:
	TMsgQueue	(BOOL bCallInit = TRUE);

    virtual void BindRecord	();	
	virtual void Init		();	

	static  LPCTSTR  GetStaticName();
};


///////////////////////////////////////////////////////////////////////////////
//								Reduced MA_Company Record definition
///////////////////////////////////////////////////////////////////////////////

/////////////////////////////////////////////////////////////////////////////
//			class  TEnhMovimentiMagazzinoRighe definition		
/////////////////////////////////////////////////////////////////////////////
//
//==========================================================================
class TB_EXPORT TMsgLots : public SqlRecord
{
	DECLARE_DYNCREATE(TMsgLots) 
	
public:
	DataLng		f_LotID;
	DataStr		f_Description;
	DataInt		f_Status;
	DataLng		f_IdExt;
	DataLng		f_StatusExt;
	DataStr		f_StatusDescriptionExt;
	DataLng		f_ErrorExt;
	DataInt		f_DeliveryType;
	DataInt		f_PrintType;
	DataMon		f_TotalAmount;
	DataMon		f_PrintAmount;
	DataMon		f_PostageAmount;
	DataDate	f_SendAfter;
	DataLng		f_TotalPages;
	DataStr		f_Fax;
	DataStr		f_Addressee;
	DataStr		f_Address;
	DataStr		f_ZipCode;
	DataStr		f_City;
	DataStr		f_County;
	DataStr		f_Country;
	DataStr		f_AddresseeNamespace;
	DataStr		f_AddresseePrimaryKey;
	DataBool	f_Incongruous;

public:
	TMsgLots(BOOL bCallInit = TRUE);
	
public:
    virtual void	BindRecord	();	
	static  LPCTSTR  GetStaticName();
};

//=============================================================================
//		TableUpdater Definition							
//=============================================================================
//-----------------------------------------------------------------------------
class TB_EXPORT TUMsgQueue : public TableUpdater
{
	DECLARE_DYNAMIC(TUMsgQueue)
	
public:
	DataLng		m_MsgId;
	
public:
	TUMsgQueue (CAbstractFormDoc* pDocument = NULL, CMessages* pMessages = NULL);
		
protected:
	virtual void	OnDefineQuery		();
	virtual void	OnPrepareQuery		();
	virtual BOOL 	IsEmptyQuery		();

public:	
	FindResult	FindRecord	(const DataLng& aMsgId, BOOL bLock);
	
	TMsgQueue* GetRecord() const
		{
			ASSERT(m_pRecord->IsKindOf(RUNTIME_CLASS(TMsgQueue)));
			return (TMsgQueue*) m_pRecord;
		}
};

//=============================================================================
//		TableUpdater Definition							
//=============================================================================
//-----------------------------------------------------------------------------
class TB_EXPORT TUMsgLots : public TableUpdater
{
	DECLARE_DYNAMIC(TUMsgLots)
	
public:
	DataLng		m_LotId;
	
public:
	TUMsgLots (CAbstractFormDoc* pDocument = NULL, CMessages* pMessages = NULL);
		
protected:
	virtual void	OnDefineQuery		();
	virtual void	OnPrepareQuery		();
	virtual BOOL 	IsEmptyQuery		();

public:	
	FindResult	FindRecord	(const DataLng& aMsgId, BOOL bLock);
	
	TMsgLots* GetRecord() const
		{
			ASSERT(m_pRecord->IsKindOf(RUNTIME_CLASS(TMsgLots)));
			return (TMsgLots*) m_pRecord;
		}
};

///////////////////////////////////////////////////////////////////////////////
#include "endh.dex"
