#include "stdafx.h" 

#include <TbOleDb\OleDbMng.h>
#include "PostaLiteTables.h"

//////////////////////////////////////////////////////////////////////////////
//								TMsgQueue
//////////////////////////////////////////////////////////////////////////////

//-----------------------------------------------------------------------------
IMPLEMENT_DYNCREATE(TMsgQueue, SqlRecord) 

//-----------------------------------------------------------------------------
LPCTSTR TMsgQueue::GetStaticName() { return _NS_TBL("TB_MsgQueue"); }

//-----------------------------------------------------------------------------
TMsgQueue::TMsgQueue(BOOL bCallInit)
	:
	SqlRecord(GetStaticName())
{
	BindRecord();	
	if (bCallInit) Init(); 
}

//-----------------------------------------------------------------------------
void TMsgQueue::Init()
{
	SqlRecord::Init();
}

//-----------------------------------------------------------------------------
void TMsgQueue::BindRecord()
{
	BEGIN_BIND_DATA	()

		BIND_AUTOINCREMENT	(_NS_FLD("MsgID"),			f_MsgID)

		BIND_DATA (_NS_FLD("LotID"),					f_LotID)
		BIND_DATA (_NS_FLD("Fax"),						f_Fax)
		BIND_DATA (_NS_FLD("Addressee"),				f_Addressee)
		BIND_DATA (_NS_FLD("Address"),					f_Address)
		BIND_DATA (_NS_FLD("Zip"),						f_ZipCode)
		BIND_DATA (_NS_FLD("City"),						f_City)		
		BIND_DATA (_NS_FLD("County"),					f_County)	
		BIND_DATA (_NS_FLD("Country"),					f_Country)	
		BIND_DATA (_NS_FLD("Subject"),					f_Subject)	
		BIND_DATA (_NS_FLD("DocNamespace"),				f_DocNamespace)	
		BIND_DATA (_NS_FLD("DocPrimaryKey"),			f_DocPrimaryKey)
		BIND_DATA (_NS_FLD("AddresseeNamespace"),		f_AddresseeNamespace)
		BIND_DATA (_NS_FLD("AddresseePrimaryKey"),		f_AddresseePrimaryKey)
		BIND_DATA (_NS_FLD("DocFileName"),				f_DocFileName)		
		BIND_DATA (_NS_FLD("DocPages"),					f_DocPages)	
		BIND_DATA (_NS_FLD("DocSize"),					f_DocSize)	
		BIND_DATA (_NS_FLD("DeliveryType"),				f_DeliveryType)	
		BIND_DATA (_NS_FLD("PrintType"),				f_PrintType)

	END_BIND_DATA()
}
	
/////////////////////////////////////////////////////////////////////////////
//			class  TMsgLots implementation
/////////////////////////////////////////////////////////////////////////////
//-----------------------------------------------------------------------------
IMPLEMENT_DYNCREATE(TMsgLots, SqlRecord) 

//-----------------------------------------------------------------------------
TMsgLots::TMsgLots(BOOL bCallInit  /* = TRUE */)
	:
	SqlRecord	(GetStaticName()),
	f_LotID(),
	f_Description(),
	f_Status(),
	f_IdExt(),
	f_StatusExt(),
	f_StatusDescriptionExt(),
	f_DeliveryType(),
	f_PrintType(),
	f_TotalAmount(),
	f_PrintAmount(),
	f_PostageAmount(),
	f_SendAfter(),
	f_TotalPages(),
	f_Fax(),
	f_Addressee(),
	f_Address(),
	f_ZipCode(),
	f_City(),
	f_County(),
	f_Country(),
	f_AddresseeNamespace(),
	f_AddresseePrimaryKey(),
	f_Incongruous()
{
	BindRecord();
	if (bCallInit) Init();
}

//-----------------------------------------------------------------------------
void TMsgLots::BindRecord()
{
	BEGIN_BIND_DATA	()
		BIND_DATA (_NS_FLD("LotID"),					f_LotID)
		BIND_DATA (_NS_FLD("Description"),				f_Description)
		BIND_DATA (_NS_FLD("Status"),					f_Status)
		BIND_DATA (_NS_FLD("IdExt"),					f_IdExt)
		BIND_DATA (_NS_FLD("StatusExt"),				f_StatusExt)
		BIND_DATA (_NS_FLD("StatusDescriptionExt"),		f_StatusDescriptionExt)
		BIND_DATA (_NS_FLD("ErrorExt"),					f_ErrorExt)
		BIND_DATA (_NS_FLD("DeliveryType"),				f_DeliveryType)
		BIND_DATA (_NS_FLD("PrintType"),				f_PrintType)
		BIND_DATA (_NS_FLD("TotalAmount"),				f_TotalAmount)
		BIND_DATA (_NS_FLD("PrintAmount"),				f_PrintAmount)
		BIND_DATA (_NS_FLD("PostageAmount"),			f_PostageAmount)
		BIND_DATA (_NS_FLD("SendAfter"),				f_SendAfter)
		BIND_DATA (_NS_FLD("TotalPages"),				f_TotalPages)
		BIND_DATA (_NS_FLD("Fax"),						f_Fax)
		BIND_DATA (_NS_FLD("Addressee"),				f_Addressee)
		BIND_DATA (_NS_FLD("Address"),					f_Address)
		BIND_DATA (_NS_FLD("Zip"),						f_ZipCode)
		BIND_DATA (_NS_FLD("City"),						f_City)
		BIND_DATA (_NS_FLD("County"),					f_County)
		BIND_DATA (_NS_FLD("Country"),					f_Country)
		BIND_DATA (_NS_FLD("AddresseeNamespace"),		f_AddresseeNamespace)
		BIND_DATA (_NS_FLD("AddresseePrimaryKey"),		f_AddresseePrimaryKey)
		BIND_DATA (_NS_FLD("Incongruous"),				f_Incongruous)
	END_BIND_DATA()
}

//-----------------------------------------------------------------------------
LPCTSTR TMsgLots::GetStaticName() { return _NS_TBL("TB_MsgLots"); }

/////////////////////////////////////////////////////////////////////////////
//		TableUpdater :	class TUMsgQueue
/////////////////////////////////////////////////////////////////////////////
//-----------------------------------------------------------------------------
IMPLEMENT_DYNAMIC (TUMsgQueue, TableUpdater)

//------------------------------------------------------------------------------
TUMsgQueue::TUMsgQueue (CAbstractFormDoc* pDocument, CMessages* pMessages)
	: 
	TableUpdater(RUNTIME_CLASS(TMsgQueue), (CBaseDocument*)pDocument, pMessages)
{
}

//------------------------------------------------------------------------------
void TUMsgQueue::OnDefineQuery ()
{
	m_pTable->SelectAll			();
	m_pTable->AddFilterColumn	(GetRecord()->f_MsgID);
	m_pTable->AddParam			(_T("p1"), GetRecord()->f_MsgID);
}
	
//------------------------------------------------------------------------------
void TUMsgQueue::OnPrepareQuery ()
{
	m_pTable->SetParamValue(_T("p1"),	m_MsgId);
}

//------------------------------------------------------------------------------
BOOL TUMsgQueue::IsEmptyQuery()
{
	return m_MsgId.IsEmpty(); 
}

//------------------------------------------------------------------------------
TableUpdater::FindResult TUMsgQueue::FindRecord(const DataLng& aMsgId, BOOL bLock)
{
	m_MsgId = aMsgId;
	
	return TableUpdater::FindRecord(bLock);
}                                                                           


/////////////////////////////////////////////////////////////////////////////
//		TableUpdater :	class TUMsgLots
/////////////////////////////////////////////////////////////////////////////
//-----------------------------------------------------------------------------
IMPLEMENT_DYNAMIC (TUMsgLots, TableUpdater)

//------------------------------------------------------------------------------
TUMsgLots::TUMsgLots (CAbstractFormDoc* pDocument, CMessages* pMessages)
	: 
	TableUpdater(RUNTIME_CLASS(TMsgLots), (CBaseDocument*)pDocument, pMessages)
{
}

//------------------------------------------------------------------------------
void TUMsgLots::OnDefineQuery ()
{
	m_pTable->SelectAll			();
	m_pTable->AddFilterColumn	(GetRecord()->f_LotID);
	m_pTable->AddParam			(_T("p1"), GetRecord()->f_LotID);
}
	
//------------------------------------------------------------------------------
void TUMsgLots::OnPrepareQuery ()
{
	m_pTable->SetParamValue(_T("p1"),	m_LotId);
}

//------------------------------------------------------------------------------
BOOL TUMsgLots::IsEmptyQuery()
{
	return m_LotId.IsEmpty(); 
}

//------------------------------------------------------------------------------
TableUpdater::FindResult TUMsgLots::FindRecord(const DataLng& aMsgId, BOOL bLock)
{
	m_LotId = aMsgId;
	
	return TableUpdater::FindRecord(bLock);
}                                                                           