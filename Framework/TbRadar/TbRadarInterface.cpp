
#include "stdafx.h" 

#include <TbOleDb\SqlTable.h>
#include <TbGes\interfacemacros.h>
#include <TbGes\Bodyedit.h>
#include <TbWoormViewer\woormdoc.hjson>

#include "radardoc.h"
#include "radarfrm.h"
#include "radarvw.h"
#include "WrmRdrdoc.h"
#include "WrmRdrfrm.h"
#include "WrmRdrvw.h"

#ifdef _DEBUG
#undef THIS_FILE
static const char THIS_FILE[] = __FILE__;
#endif

/////////////////////////////////////////////////////////////////////////////
//				INIZIO definizione della interfaccia di Add-On
/////////////////////////////////////////////////////////////////////////////
//
//-----------------------------------------------------------------------------
BEGIN_ADDON_INTERFACE()

	//-----------------------------------------------------------------------------
	BEGIN_TEMPLATE()
		BEGIN_DOCUMENT (_NS_DOC("Radar"), TPL_NO_PROTECTION)
			nResource = IDR_EXTDOC;
			REGISTER_MASTER_TEMPLATE(szDefaultViewMode, CRadarDoc,	CRadarFrame,	CRadarView)
		END_DOCUMENT ()
		BEGIN_DOCUMENT (_NS_DOC("WoormRadar"), TPL_NO_PROTECTION)
			nResource = IDR_LISTER;
			REGISTER_MASTER_TEMPLATE(szDefaultViewMode, CWrmRadarDoc,	CWrmRadarFrame,	CWrmRadarView)
		END_DOCUMENT ()
	END_TEMPLATE()
	//-----------------------------------------------------------------------------
END_ADDON_INTERFACE()
