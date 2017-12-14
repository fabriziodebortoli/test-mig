
#include "stdafx.h" 

#include <tbges\interfacemacros.h>

#include "TXEParameters.h"

#ifdef _DEBUG
#undef THIS_FILE
static const char THIS_FILE[] = __FILE__;
#endif

/////////////////////////////////////////////////////////////////////////////

/////////////////////////////////////////////////////////////////////////////
//				INIZIO definizione della interfaccia di Add-On
/////////////////////////////////////////////////////////////////////////////
//
//-------------------------------------------------------------------------------
BEGIN_ADDON_INTERFACE()
	DATABASE_RELEASE(400)

	//-----------------------------------------------------------------------------
	BEGIN_TABLES()
		BEGIN_REGISTER_TABLES ()
			REGISTER_TABLE (TXEParameters)
		END_REGISTER_TABLES ()
	END_TABLES()
	
END_ADDON_INTERFACE()
