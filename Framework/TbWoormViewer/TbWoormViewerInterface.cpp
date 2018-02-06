
#include "stdafx.h" 

// library declaration
#include <TbGeneric\FontsTable.h>

#include <TbGenlib\messages.h>
#include <TbGenlib\baseapp.h>

#include <TbParser\FontsParser.h>

#include <tboledb\interfacemacros.h>

#include <TbGes\ExtDocFrame.h>
#include <TbGes\JsonFrame.h>

#include <TbWoormViewer\woormdoc.h>
#include <TbWoormViewer\woormfrm.h>
#include <TbWoormViewer\woormvw.h>

#include <TbWoormViewer\RSEditorUI.h>
#include <TbWoormViewer\RSEditView.h>
#include "Woormdoc.hjson"

#include <TbWoormViewer\DBarcodeSettings.h>
#include "ModuleObjects\BarcodeSettings\JsonForms\IDD_BARCODE_COMPANYUSER_SETTINGS.hjson"

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

		BEGIN_DOCUMENT (_NS_DOC("TbWoorm"), TPL_FUNCTION_PROTECTION)
			nResource = IDR_LISTER;
			REGISTER_MASTER_TEMPLATE(szDefaultViewMode, CWoormDocMng, CWoormFrame, CWoormView)
			REGISTER_SLAVE_TEMPLATE(CWoormDocMng, CRSEditorFrame, CRSEditView)
			REGISTER_SLAVE_TEMPLATE(CWoormDocMng, CRSEditorFrameFullText, CRSEditViewFullText)
			REGISTER_SLAVE_TEMPLATE(CWoormDocMng, CRSEditorDebugFrame, CRSEditViewDebug)
		END_DOCUMENT()

		BEGIN_DOCUMENT(_NS_DOC("BarcodeSettings"), TPL_NO_PROTECTION)
			REGISTER_MASTER_JSON_TEMPLATE(szDefaultViewMode, DBarcodeSettings, IDD_BARCODE_COMPANYUSER_SETTINGS)
		END_DOCUMENT()

	END_TEMPLATE()

END_ADDON_INTERFACE()
