
#include "stdafx.h" 

#include <TBGES\extdoc.h>
#include <TBGES\InterfaceMacros.h>

#include <XEngine\TBXmlEnvelope\XEngineObject.H>

#include "XMLDataMng.h"
#include "ExpCriteriaDlg.h"
#include "ExpCriteriaWiz.h"
#include "ImpCriteriaWiz.h"
#include "XMLEvents.h"
#include "XMLCodingRules.h"
#include "GenFunc.h"
#include "DParameters.h"
#include "UIParameters.h"

#ifdef _DEBUG
#undef THIS_FILE
static const char THIS_FILE[] = __FILE__;
#endif

//-----------------------------------------------------------------------------
BOOL OnQueryCanCloseApp(FunctionDataInterface* pInterface)
{ 
	XEngineObject *pObject = AfxGetXEngineObject(FALSE);
	if (pObject)
		pObject->CloseLatestDocument();

	return TRUE;
}

////////////////////////////////////////////////////////////////////////////
//				INIZIO definizione della interfaccia di Add-On
/////////////////////////////////////////////////////////////////////////////
//
//-----------------------------------------------------------------------------
BEGIN_ADDON_INTERFACE()
	
	BEGIN_TEMPLATE()
		BEGIN_DOCUMENT (_NS_DOC("ExportCriteria"), TPL_NO_PROTECTION)
		REGISTER_MASTER_TEMPLATE(szDefaultViewMode, CExpCriteriaWizardDoc, CExpCriteriaWizardFrame, CExpCriteriaWizardView)
		END_DOCUMENT ()
		
		BEGIN_DOCUMENT (_NS_DOC("ImportCriteria"), TPL_NO_PROTECTION)
		REGISTER_MASTER_TEMPLATE(szDefaultViewMode, CImpCriteriaWizardDoc, CImpCriteriaWizardFrame, CImpCriteriaWizardView)
		END_DOCUMENT ()
		
		BEGIN_DOCUMENT(_NS_DOC("XEngineParameters"), TPL_NO_PROTECTION)
			REGISTER_MASTER_TEMPLATE	(szDefaultViewMode,		DXEParameters,			CMasterFrame,	CXEParametersView)
		END_DOCUMENT()

	END_TEMPLATE()
	
	//-----------------------------------------------------------------------------
	BEGIN_TABLES()
		BEGIN_REGISTER_TABLES ()
			REGISTER_TABLE (TLostAndFound)
			REGISTER_TABLE (TXMLKeyExtension)
		END_REGISTER_TABLES ()
	END_TABLES()

	//-----------------------------------------------------------------------------
	BEGIN_HOTLINK()
	END_HOTLINK ()

	BEGIN_FAMILY_CLIENT_DOC()
		WHEN_FAMILY_SERVER_DOC(CAbstractFormDoc)
			ATTACH_FAMILY_CLIENT_DOC_EXCLUDE_BATCH_MODE	(CXMLDataExportDoc,			_NS_CD("CDExportWizard"))
			ATTACH_FAMILY_CLIENT_DOC_EXCLUDE_BATCH_MODE	(CXMLDataImportDoc,			_NS_CD("CDImportWizard"))
			ATTACH_FAMILY_CLIENT_DOC					(CXMLDataManagerClientDoc,	_NS_CD("CDDataManagerInit"))
		END_FAMILY_SERVER_DOC()
	END_FAMILY_CLIENT_DOC()
	
	//-----------------------------------------------------------------------------
	BEGIN_FUNCTIONS()
		REGISTER_EVENT_HANDLER	(_NS_EH("OnQueryCanCloseApp"),	OnQueryCanCloseApp)
	END_FUNCTIONS()

END_ADDON_INTERFACE()


