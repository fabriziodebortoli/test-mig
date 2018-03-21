
#include "stdafx.h" 

#include "interfacemacros.h"
#include "JSONFormDoc.h"
#include "formmngdlg.h"
#include "FieldInspector.h"
#include "TABBER.H"
#include "TileManager.h"
#include "TileDialog.h"
#include "bodyedit.h"
#include "NumbererService.h"
#include "JsonFormEngineEx.h"
#include "TBActivityDocument.h"
#include "TBBaseNavigation.h"
#include "DParametersDoc.h"
#include "ItemSource.h"
#include "Validator.h"
#include "ComposedHotLink.h"
#include "ControlBehaviour.h"
#include "DTBFSettings.h"
#include "JsonFrame.h"
#include "JsonForms\JsonModelGenerator\IDD_GENERATE_JSON_MODEL_FRAME.hjson"
#include "ModuleObjects\TBFSettings\JsonForms\IDD_TBF_COMPANYUSER_SETTINGS.hjson"
#include <TbGes\JsonForms\EmptyView\IDD_EMPTY_VIEW.hjson>
#include <TbGes\JsonForms\TbGes\IDD_MASTER_FRAME.hjson>
#include <TbGes\JsonForms\TbGes\IDD_BATCH_FRAME.hjson>

//includere come ultimo include all'inizio del cpp
#include "begincpp.dex"

#ifdef _DEBUG
#undef THIS_FILE
static const char THIS_FILE[] = __FILE__;
#endif

#define _AddOn_Interface_Of tbgestbges

/////////////////////////////////////////////////////////////////////////////
//				INIZIO definizione della interfaccia di Add-On
/////////////////////////////////////////////////////////////////////////////
//
//-----------------------------------------------------------------------------
BEGIN_ADDON_INTERFACE()
DATABASE_RELEASE(400)
//-----------------------------------------------------------------------------
BEGIN_TABLES()
BEGIN_REGISTER_TABLES()
REGISTER_TABLE(TAutoincrementEntities)
REGISTER_TABLE(TAutonumberEntities)
REGISTER_TABLE(TAutonumberEntitiesYears)
REGISTER_VIRTUAL_TABLE(TSummaryDetail)
REGISTER_VIRTUAL_TABLE(TNodeDetail)
END_REGISTER_TABLES()
END_TABLES()

//-----------------------------------------------------------------------------
BEGIN_TEMPLATE()
		BEGIN_DOCUMENT(_NS_DOC("TbDynamicDocument"), TPL_NO_PROTECTION)
			REGISTER_MASTER_TEMPLATE(szDefaultViewMode, CDynamicFormDoc, CMasterFrame, CDynamicFormView)
			REGISTER_BKGROUND_TEMPLATE(szBackgroundViewMode, CDynamicFormDoc)
			REGISTER_MASTER_JSON_TEMPLATE(szDefaultViewModeWeb, CDynamicFormDoc, IDD_MASTER_FRAME)
		END_DOCUMENT ()
		BEGIN_DOCUMENT (_NS_DOC("TbDynamicBatchDocument"), TPL_NO_PROTECTION)
			REGISTER_MASTER_TEMPLATE	(szDefaultViewMode,		CDynamicBatchFormDoc,	CBatchFrame,	CDynamicFormView)
			REGISTER_BKGROUND_TEMPLATE	(szBackgroundViewMode, CDynamicBatchFormDoc)
			REGISTER_MASTER_JSON_TEMPLATE(szDefaultViewModeWeb, CDynamicBatchFormDoc, IDD_BATCH_FRAME)
		END_DOCUMENT ()
		BEGIN_DOCUMENT(_NS_DOC("TbJSONDocument"), TPL_NO_PROTECTION)
			REGISTER_BKGROUND_TEMPLATE(szNoInterface, CJSONFormDoc)
			REGISTER_SLAVE_TEMPLATE(CAbstractFormDoc, CRowFormFrame, CJsonRowView)
		END_DOCUMENT()

		BEGIN_DOCUMENT(_NS_DOC("Browser"), TPL_NO_PROTECTION)
			REGISTER_MASTER_TEMPLATE(szDefaultViewMode, DBrowserDocument, CBrowserFrame, CBrowserDocumentView)
		END_DOCUMENT()

		BEGIN_DOCUMENT(_NS_DOC("TBFSettings"), TPL_ADMIN_PROTECTION)
			REGISTER_MASTER_JSON_TEMPLATE(szDefaultViewMode, DTBFSettings, IDD_TBF_COMPANYUSER_SETTINGS)
		END_DOCUMENT()

		REGISTER_SLAVE_JSON_TEMPLATE(IDD_GENERATE_JSON_MODEL_FRAME);
	END_TEMPLATE()

	BEGIN_ITEMSOURCE()
		DECLARE_ITEMSOURCE(CItemSource, _NS_IS("ItemSource"))
		DECLARE_ITEMSOURCE(CItemSourceXml, _NS_IS("ItemSourceXml"))
	END_ITEMSOURCE()

	BEGIN_VALIDATOR()
		DECLARE_VALIDATOR(CValidator, _NS_VL("Validator"))
		DECLARE_VALIDATOR(CEmptyValidator, _NS_VL("EmptyValidator"))
	END_VALIDATOR()

	BEGIN_DATA_ADAPTER()
		DECLARE_DATA_ADAPTER(CDataAdapter, _NS_DA("DataAdapter"))
	END_DATA_ADAPTER()

	//-----------------------------------------------------------------------------
	BEGIN_HOTLINK()
		DECLARE_HOTLINK(ComposedJsonHotLink,	_NS_HKL("ComposedJsonHotLink"))
		DECLARE_HOTLINK(SimHKLUser,				_NS_HKL("Users"))
		DECLARE_HOTLINK(SimHKLRole,				_NS_HKL("Roles"))
		DECLARE_HOTLINK(SimHKLCurrentUserRoles,	_NS_HKL("CurrentUserRoles"))

		DECLARE_HOTLINK(SimHKLCultureUI,		_NS_HKL("UICultures"))
		DECLARE_HOTLINK(SimHKLBarCode,			_NS_HKL("BarCode"))

		DECLARE_HOTLINK_EX(HKLEntities,			_NS_HKL("Entities"),		CDescriptionCombo)
		DECLARE_HOTLINK_EX(HKLBehaviours,		_NS_HKL("Behaviours"),		CDescriptionCombo)

		DECLARE_HOTLINK(HKLTables,				_NS_HKL("Tables"))
		DECLARE_HOTLINK(HKLTableColumns,		_NS_HKL("TableColumns"))

		DECLARE_HOTLINK_EX(HKLApplications,		_NS_HKL("Applications"),	CDescriptionCombo)
		DECLARE_HOTLINK_EX(HKLModules,			_NS_HKL("Modules"),			CDescriptionCombo)

	END_HOTLINK ()

	BEGIN_FAMILY_CLIENT_DOC()
		WHEN_FAMILY_SERVER_DOC(CAbstractFormDoc)
			ATTACH_FAMILY_CLIENT_DOC_EXCLUDE_BATCH_MODE(CDFieldInspector,	_NS_CD("FieldInspector"))
		END_FAMILY_SERVER_DOC()
		WHEN_FAMILY_SERVER_DOC(DParametersDoc)
			ATTACH_FAMILY_CLIENT_DOC(CDParametersDoc, _NS_CD("CDParametersDoc"))
		END_FAMILY_SERVER_DOC()
	END_FAMILY_CLIENT_DOC()
	
	BEGIN_REGISTER_CONTROLS()
		DECLARE_CONTROLS_FAMILY(szMTabber,		_TB("Tab Control"), DataType::Null)
		DECLARE_CONTROLS_FAMILY(szMTab,			_TB("Tab Dialog"), DataType::Null)
		DECLARE_CONTROLS_FAMILY(szMTileManager, _TB("Tile Manager"), DataType::Null)
		DECLARE_CONTROLS_FAMILY(szMTileGroup,	_TB("Tile Group"), DataType::Null)
		DECLARE_CONTROLS_FAMILY(szMTileDialog,  _TB("Tile Dialog"), DataType::Null)
		DECLARE_CONTROLS_FAMILY(szMTilePanel,	_TB("Tile Panel"), DataType::Null)
		DECLARE_CONTROLS_FAMILY(szMTilePanelTab, _TB("Tile Panel Tab"), DataType::Null)
		DECLARE_CONTROLS_FAMILY(szMBodyEdit,	_TB("BodyEdit"), DataType::Array)
		
		// associates data types, C++ runtime classes, control public names 
		BEGIN_CONTROLS_TYPE(DataType::Null)
			REGISTER_CONTROL(szMTabber,			Tabber,			_TB("Tab Control"),	CTabManager,	0, 0,	0, 0, FALSE)
			REGISTER_CONTROL(szMTab,			Tab,			_TB("Tab Dialog"),	CTabDialog, 	0, 0,	0, 0, FALSE)
			REGISTER_CONTROL(szMTileManager,	TileManager,	_TB("Tile Manager"), CTileManager,	0, 0,	0, 0, FALSE)
			REGISTER_CONTROL(szMTileGroup,		TileGroup,		_TB("Tile Group"), CTileGroup, 0, 0,	0, 0, FALSE)
			REGISTER_CONTROL(szMTileDialog,		TileDialog,		_TB("Tile Dialog"), CTileDialog, 0, 0,	0, 0, FALSE)
			REGISTER_CONTROL(szMTilePanel,		TilePanel,		_TB("Tile Panel"), CTilePanel, 0, 0,	0, 0, FALSE)
			REGISTER_CONTROL(szMTilePanelTab,	TilePanelTab,	_TB("Tile Panel Tab"), CTilePanelTab, 0, 0,	0, 0, FALSE)
			
		END_CONTROLS_TYPE()

		// associates data types, C++ runtime classes, control public names 
		BEGIN_CONTROLS_TYPE(DataType::Array)
			REGISTER_CONTROL(szMBodyEdit,	BodyEdit, _TB("Grid Control"),	CBodyEdit,		0, 0,	0, 0, TRUE)
		END_CONTROLS_TYPE()

		DECLARE_TYPE_DEFAULT_FAMILY(DataType::Array, szMBodyEdit)
	END_REGISTER_CONTROLS()

	END_REGISTRATION()
		BEGIN_BEHAVIOURS()
			DECLARE_BEHAVIOUR_SERVICE(CAutoincrementService::GetStaticName(),	CAutoincrementService)
			DECLARE_BEHAVIOUR_SERVICE(CAutonumberService::GetStaticName(),		CAutonumberService)
		END_BEHAVIOURS()
	END_END_REGISTRATION()

END_ADDON_INTERFACE()
#undef _AddOn_Interface_Of
