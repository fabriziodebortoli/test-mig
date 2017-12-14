
#include "stdafx.h" 

#include <TbGenlib\messages.h>
#include <TbOleDb\InterfaceMacros.h>
#include <TbOleDb\SqlConnect.h>

#include "AuditReportGenerator.h"
#include "AuditTables.h"


/////////////////////////////////////////////////////////////////////////////
//				AddOn-Interface declaration
/////////////////////////////////////////////////////////////////////////////
//
//-----------------------------------------------------------------------------
BEGIN_ADDON_INTERFACE()

	DATABASE_RELEASE(400)

	//-----------------------------------------------------------------------------
	BEGIN_TABLES()
		BEGIN_REGISTER_TABLES	()
			REGISTER_TABLE	(TAuditTables)
			REGISTER_TABLE	(TNamespaces)
		END_REGISTER_TABLES		()
	END_TABLES()

END_ADDON_INTERFACE()


