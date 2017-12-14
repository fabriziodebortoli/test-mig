
#pragma once

#include "TExtGuid.h"

//includere alla fine degli include del .H
#include "beginh.dex"

//////////////////////////////////////////////////////////////////////////////
//             				CDBone
//////////////////////////////////////////////////////////////////////////////
//
class CDBone : public CClientDoc
{
protected:
	DECLARE_DYNCREATE(CDBone)

public:
	CDBone();
	~CDBone();

private:
	BOOL		m_bManageDataGuid;
	TRExtGuid*	m_pTRExtGuid;
	TUExtGuid*	m_pTUExtGuid;

protected:	
	virtual BOOL OnAttachData				();

	virtual	BOOL OnBeforeNewRecord			();
	virtual	BOOL OnBeforeEditRecord			();
	virtual	BOOL OnBeforeDeleteRecord		();

	virtual	BOOL OnExtraNewTransaction		();
	virtual	BOOL OnExtraEditTransaction		();
	virtual	BOOL OnExtraDeleteTransaction	();

private:
			BOOL FindExtGuid				();
			BOOL ProcessSubscriptions		(CEventManagementData* pEventManagementData);
			BOOL ShowMessage				(CSubscriptionData* pSubscriptionData);
};

#include "endh.dex"
