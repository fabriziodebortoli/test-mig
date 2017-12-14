#pragma once

#include <TbGeneric\DataObj.h>
#include <TbGes\ExtDoc.h>
#include <TbGes\ExtDocClientDoc.h>


#include "beginh.dex"


///////////////////////////////////////////////////////////////////////////////
//						CDUpdateTBModifiedMaster definition
///////////////////////////////////////////////////////////////////////////////
//
//----------------------------------------------------------------------------
class TB_EXPORT CDUpdateTBModifiedMaster : public CClientDoc
{
	DECLARE_DYNCREATE(CDUpdateTBModifiedMaster)

private:

	CAbstractFormDoc* GetServerDoc() { return (CAbstractFormDoc*) m_pServerDocument; };	

protected:

	void OnDuringBatchExecute	(SqlRecord* pCurrProcessedRecord);

};

#include "endh.dex"