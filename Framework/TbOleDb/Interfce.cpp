
#include "stdafx.h" 

#include <tboledb\sqlrec.h>
#include <tboledb\sqlmark.h>
#include <tboledb\sqlconnect.h>
#include <tboledb\interfacemacros.h>

//includere come ultimo include all'inizio del cpp
#include "begincpp.dex"

#ifdef _DEBUG
#undef THIS_FILE
static const char THIS_FILE[] = __FILE__;
#endif

/////////////////////////////////////////////////////////////////////////////
//				INIZIO definizione della interfaccia di Add-On
/////////////////////////////////////////////////////////////////////////////
//
#define _AddOn_Interface_Of tboledbtboledb

//-----------------------------------------------------------------------------
BEGIN_ADDON_INTERFACE()
	//-----------------------------------------------------------------------------
	DATABASE_RELEASE(401)

	//-----------------------------------------------------------------------------
	BEGIN_TABLES()
   		BEGIN_REGISTER_TABLES	()
   			REGISTER_TABLE	(SqlDBMark) 
		END_REGISTER_TABLES		()
	END_TABLES()

	//-----------------------------------------------------------------------------

END_ADDON_INTERFACE()
#undef _AddOn_Interface_Of