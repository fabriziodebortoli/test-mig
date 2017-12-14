
#include "stdafx.h" 

#include <TBGES\extdoc.h>
#include <TBGES\InterfaceMacros.h>

#include "CSubscriptionData.h"
#include "CDTrigger.h"
#include "CDBone.h"
#include "TExtGuid.h"

#ifdef _DEBUG
#undef THIS_FILE
static const char THIS_FILE[] = __FILE__;
#endif



////////////////////////////////////////////////////////////////////////////
//				INIZIO definizione della interfaccia di Add-On
/////////////////////////////////////////////////////////////////////////////
//

//-----------------------------------------------------------------------------
BEGIN_ADDON_INTERFACE()
	DATABASE_RELEASE(400)

	//-----------------------------------------------------------------------------
	BEGIN_TABLES()
		BEGIN_REGISTER_TABLES ()
			REGISTER_TABLE (TExtGuid)
		END_REGISTER_TABLES ()
	END_TABLES()
	
	//-----------------------------------------------------------------------------
	BEGIN_FAMILY_CLIENT_DOC()
		WHEN_FAMILY_SERVER_DOC(CAbstractFormDoc)
			if (AfxGetSubscriptionInfo()->ExistDocumentTrigger(pDoc))
				ATTACH_FAMILY_CLIENT_DOC(CDTrigger,			_NS_CD("CDTrigger"))
			if (AfxGetSubscriptionInfo()->ExistDocumentBONE(pDoc))
				ATTACH_FAMILY_CLIENT_DOC_EXCLUDE_BATCH_MODE(CDBone,				_NS_CD("CDBone"))
		END_FAMILY_SERVER_DOC()
	END_FAMILY_CLIENT_DOC()

END_ADDON_INTERFACE()


