
#include "stdafx.h" 

#include <TbGes\extdoc.h>
#include <TbGes\JsonFrame.h>
#include <TbGes\InterfaceMacros.h>

#include "TAbsenceReasons.h"
#include "TCalendars.h"
#include "TResources.h"
#include "TWorkers.h"
#include "RMFunctions.h"

#include "DAbsenceReasons.h"
#include "DArrangements.h"
#include "DCalendars.h"
#include "DResources.h"
#include "DResourceTypes.h"
#include "DWorkers.h"
#include "CDWorkerWindow.h"
#include "BDWorkerWindow.h"
#include "BDResourcesLayout.h"

#include "ModuleObjects\AbsenceReasons\JsonForms\IDD_ABSENCEREASONS.hjson"
#include "ModuleObjects\Arrangements\JsonForms\IDD_ARRANGEMENTS.hjson"
#include "ModuleObjects\Calendars\JsonForms\IDD_CALENDARS.hjson"
#include "ModuleObjects\ResourceTypes\JsonForms\IDD_RESOURCETYPES.hjson"
#include "ModuleObjects\Resources\JsonForms\IDD_RESOURCES.hjson"
#include "ModuleObjects\ResourcesLayout\JsonForms\IDD_RESOURCES_LAYOUT.hjson"
#include "ModuleObjects\Workers\JsonForms\IDD_WORKERS.hjson"
#include "ModuleObjects\WorkerWindow\JsonForms\IDD_WORKER_WINDOW.hjson"
#include "JsonForms\ResourcesLayoutAction\IDD_RESOURCES_LAYOUT_ACTION.hjson"

#ifdef _DEBUG
#undef THIS_FILE
static const char THIS_FILE[] = __FILE__;
#endif

// per assegnare il workerID corrispondente alla login, 
// utilizzato poi da TB per scrivere nel TBCreatedID e TBModifiedID
//-----------------------------------------------------------------------------
BOOL CheckWorkers(FunctionDataInterface* FPOF) 
{
	CCheckWorker* pCheck = new CCheckWorker();
	DataLng aWorkerID = pCheck->GetWorkerID();
	pCheck->IntegrateConvertedWorkers();
	SAFE_DELETE(pCheck);

	if (aWorkerID.IsEmpty())
	{
		AfxGetDiagnostic()->Add(_TB("Invalid worker for current login"));
		AfxSetValid(FALSE);
		return FALSE;
	}

	AfxSetWorkerId(aWorkerID);
	AfxSetWorkersTable((CWorkersTableObj*)(new CWorkersTable()));  // serve per avere l'elenco dei workers
	return TRUE;
}

////////////////////////////////////////////////////////////////////////////
//				INIZIO definizione della interfaccia di Add-On
/////////////////////////////////////////////////////////////////////////////
//
//-----------------------------------------------------------------------------
BEGIN_ADDON_INTERFACE()
	DATABASE_RELEASE(401)

	//-----------------------------------------------------------------------------
	BEGIN_TABLES()
		BEGIN_REGISTER_TABLES()
			REGISTER_TABLE(TResources)
			REGISTER_TABLE(TResourcesDetails)
			REGISTER_TABLE(TResourcesFields)
			REGISTER_TABLE(TResourcesAbsences)
			REGISTER_TABLE(TResourceTypes)
			REGISTER_TABLE(TWorkers)
			REGISTER_TABLE(TWorkersDetails)
			REGISTER_TABLE(TWorkersFields)
			REGISTER_TABLE(TWorkersArrangement)
			REGISTER_TABLE(TWorkersAbsences)
			REGISTER_TABLE(TArrangements)
			REGISTER_TABLE(TAbsenceReasons)
			REGISTER_TABLE(TCalendars)
			REGISTER_TABLE(TCalendarsHolidays)
			REGISTER_TABLE(TCalendarsShifts)

			REGISTER_VIRTUAL_TABLE(VCalendarWorkingPeriod)
		END_REGISTER_TABLES()
	END_TABLES()

	//-----------------------------------------------------------------------------
	BEGIN_TEMPLATE()
		BEGIN_DOCUMENT(_NS_DOC("ResourceTypes"), TPL_NO_PROTECTION)
			REGISTER_MASTER_JSON_TEMPLATE(szDefaultViewMode, DResourceTypes, IDD_RESOURCETYPES)
		END_DOCUMENT()

		BEGIN_DOCUMENT(_NS_DOC("Arrangements"), TPL_NO_PROTECTION)
			REGISTER_MASTER_JSON_TEMPLATE(szDefaultViewMode, DArrangements, IDD_ARRANGEMENTS)
		END_DOCUMENT()

		BEGIN_DOCUMENT(_NS_DOC("AbsenceReasons"), TPL_NO_PROTECTION)
			REGISTER_MASTER_JSON_TEMPLATE(szDefaultViewMode, DAbsenceReasons, IDD_ABSENCEREASONS)
		END_DOCUMENT()

		BEGIN_DOCUMENT(_NS_DOC("Resources"), TPL_NO_PROTECTION)
			REGISTER_MASTER_JSON_TEMPLATE(szDefaultViewMode, DResources, IDD_RESOURCES)
		END_DOCUMENT()

		BEGIN_DOCUMENT(_NS_DOC("Workers"), TPL_NO_PROTECTION)
			REGISTER_MASTER_JSON_TEMPLATE(szDefaultViewMode, DWorkers, IDD_WORKERS)
		END_DOCUMENT()

		BEGIN_DOCUMENT(_NS_DOC("Calendars"), TPL_NO_PROTECTION)
			REGISTER_MASTER_JSON_TEMPLATE(szDefaultViewMode, DCalendars, IDD_CALENDARS)
		END_DOCUMENT()

		BEGIN_DOCUMENT(_NS_DOC("ResourcesLayout"), TPL_NO_PROTECTION)
			REGISTER_MASTER_JSON_TEMPLATE(szDefaultViewMode, BDResourcesLayout, IDD_RESOURCES_LAYOUT)
			REGISTER_BKGROUND_TEMPLATE(szBackgroundViewMode, BDResourcesLayout)
		END_DOCUMENT()

		BEGIN_DOCUMENT(_NS_DOC("WorkerWindow"), TPL_NO_PROTECTION)
			REGISTER_MASTER_JSON_TEMPLATE(szDefaultViewMode, BDWorkerWindow, IDD_WORKER_WINDOW)
			REGISTER_BKGROUND_TEMPLATE(szBackgroundViewMode, BDWorkerWindow)
		END_DOCUMENT()

		REGISTER_SLAVE_JSON_TEMPLATE(IDD_RESOURCES_LAYOUT_ACTION)

	END_TEMPLATE()
	
	//-----------------------------------------------------------------------------
	BEGIN_HOTLINK()
		DECLARE_HOTLINK_EX	(HKLResourceTypes,	_NS_HKL("ResourceTypes"),	CDescriptionCombo)
		DECLARE_HOTLINK_EX	(HKLResources,		_NS_HKL("Resources"),		CDescriptionCombo)
		DECLARE_HOTLINK_EX	(HKLWorkers,		_NS_HKL("Workers"),			CLongIDCombo)
		DECLARE_HOTLINK		(HKLArrangements,	_NS_HKL("Arrangements"))
		DECLARE_HOTLINK		(HKLCalendars,		_NS_HKL("Calendars"))
		DECLARE_HOTLINK		(HKLAbsenceReasons,	_NS_HKL("AbsenceReasons"))
	END_HOTLINK()

	//-----------------------------------------------------------------------------
	BEGIN_FAMILY_CLIENT_DOC()
		if (AfxGetLoginManager()->GetEdition() != _T("Standard"))
		{
			// client doc su ogni documento per la visualizzazione del dettaglio del worker
			WHEN_FAMILY_SERVER_DOC(CAbstractFormDoc)
				ATTACH_FAMILY_CLIENT_DOC_EXCLUDE_UNATTENDED_BATCH_MODE(CDWorkerWindow, _NS_CD("CDWorkerWindow"))
			END_FAMILY_SERVER_DOC()
		}
	END_FAMILY_CLIENT_DOC()
	
	//-----------------------------------------------------------------------------
	BEGIN_FUNCTIONS()
		REGISTER_EVENT_HANDLER(szOnDSNChanged, CheckWorkers)
	END_FUNCTIONS()
	
	//-----------------------------------------------------------------------------
	BEGIN_REGISTER_CONTROLS()
		BEGIN_CONTROLS_TYPE(DataType::String)
			REGISTER_STATIC_CONTROL(WorkerStatic,			_T("Worker Static"),			CWorkerStatic,			TRUE)
			REGISTER_STATIC_CONTROL(ResourcesPictureStatic, _T("ResourcesPicture Static"),	CResourcesPictureStatic,TRUE)
		END_CONTROLS_TYPE()
	END_REGISTER_CONTROLS()

END_ADDON_INTERFACE()
