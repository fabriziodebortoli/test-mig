
#pragma once

#include <TBGES\extdoc.h>

//includere alla fine degli include del .H
#include "beginh.dex"

//////////////////////////////////////////////////////////////////////////////
//             				CDTrigger
//////////////////////////////////////////////////////////////////////////////
//
class CDTrigger : public CClientDoc
{
protected:
	DECLARE_DYNCREATE(CDTrigger)

public:
	CDTrigger();
	~CDTrigger();

protected:	
	virtual	BOOL OnOkTransaction		 ();
	virtual	BOOL OnOkDelete				 ();

	virtual	BOOL OnExtraNewTransaction	 ();
	virtual	BOOL OnExtraEditTransaction	 ();
	virtual	BOOL OnExtraDeleteTransaction();

private:
	BOOL						ProcessSubscriptions		(CEventManagementData* pEventManagementData, int dAction, CString aWhen, BOOL bExport, BOOL bCallWS);
	BOOL						XmlExport					(CSubscriptionData*	pSubscriptionData);
	BOOL						CallWebService				(CSubscriptionData*	pSubscriptionData, int dAction, CString aWhen);
	CXMLDocumentObject*			CreateExportParametersFile	(CString strProfileName, CPathFinder::PosType ePosType);
	void						GetReturnMessages			(CDataObjDescription* pReturnMessages);
};

#include "endh.dex"
