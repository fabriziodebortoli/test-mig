
#include "stdafx.h" 

// library declaration
#include <tbgenlib\messages.h>
#include <tbgenlib\baseapp.h>
#include <tboledb\interfacemacros.h>
#include <TbWoormEngine\inputmng.h>
#include <TbWoormEngine\report.h>
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
#define _AddOn_Interface_Of tbwoormenginetbwoormengine

//-----------------------------------------------------------------------------
BEGIN_ADDON_INTERFACE()

BEGIN_TEMPLATE()

BEGIN_DOCUMENT(_NS_DOC("TbAbstractWoorm"), TPL_FUNCTION_PROTECTION)
	REGISTER_MASTER_TEMPLATE(szDefaultViewMode, CWoormDoc, CAbstractWoormFrame, CAbstractWoormView)
END_DOCUMENT()

END_TEMPLATE()

END_ADDON_INTERFACE()
#undef _AddOn_Interface_Of