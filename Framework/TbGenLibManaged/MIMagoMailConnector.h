#pragma once

#include <TbGeneric/dataobj.h>
#include <TbGeneric/Linefile.h>

#include "beginh.dex"


//=============================================================================
class TB_EXPORT MIMagoMailConnector
{
public:
	MIMagoMailConnector() {}

	virtual ~MIMagoMailConnector(void) {}

	BOOL	OpenInfinityMailer(CString infinityTkones, CString connectionString, CString subject, CString mailText, CString sFrom, const CStringArray&  sTo, const CStringArray&  sBCC, const CStringArray&  sCC, const CStringArray& attachments, bool deferrer);
};


//=============================================================================
#include "endh.dex"
