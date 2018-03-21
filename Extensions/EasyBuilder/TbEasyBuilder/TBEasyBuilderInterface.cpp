
#include "stdafx.h" 

#include <TbGes\Extdoc.h>
#include <TbGes\InterfaceMacros.h>
#include <TbGenlib\TBCommandInterface.h>
#include <TbGenlib\BaseApp.h>

#include "CDEasyBuilder.h"
#include "NewDocument.h"
#include "JsonDesigner.h"
#include "EBVirtualTable.h"

#ifdef _DEBUG
#undef THIS_FILE
static const char THIS_FILE[] = __FILE__;
#endif

using namespace System;
using namespace Microarea::EasyBuilder::Packager;
using namespace Microarea::TaskBuilderNet::Core::EasyBuilder;

static const TCHAR szDynamicDocNs[] = _T("Document.framework.tbges.tbges.TbDynamicDocument");
static const TCHAR szDynamicBatchDocNs[] = _T("Document.framework.tbges.tbges.TbDynamicBatchDocument");

////////////////////////////////////////////////////////////////////////////
//				Metodo statico che fa da ponte tra c# e c++ per l'aggiunta
//				di file di customizzazione alla customList
/////////////////////////////////////////////////////////////////////////////
void __stdcall OnEventDispatch(const CString& path)
{
	BaseCustomizationContext::CustomizationContextInstance->AddToCurrentCustomizationList(gcnew String(path), true, false, String::Empty, String::Empty);
}

//-----------------------------------------------------------------------------
BOOL DsnChanged(FunctionDataInterface* pRDI)
{
	//Se è attivo EasyBuilder Designer allora l'aggiunta di un nuovo report deve
	//essere notificata anche al Setup Studio, altrimenti no.
	//Questo per evitare anomalie del tipo di quella segnalata da Mattia
	//Tagliabue a Marilena:
	//Il Tagliabue installa un ebp ad un suo utente che non ha EasyBuilder Designer.
	//Questa installazione causa la comparsa di un contesto di customizzazione.
	//Il Tagliabue modifica in loco presso l'utente alcuni report: questa modifica
	//causa l'aggiunta di tali report alla customizzazione corrente.
	//Il Tagliabue decide che la personalizzazine che aveva installato prima non
	//serve più e la disinstalla, risultato: vengono disinstallati anche i report
	//che aveva modificato.
	//=>Se EasyBuilder Designer non è attivato allora i report modificati non devono essere aggiunti 
	//alla customizzazione corrente perchè, se non è presente quel modulo, significa che da
	//quella macchina non verranno mai prodotti ebp da installare su altr macchine.
	if (AfxIsActivated(TBEXT_APP, EASYSTUDIO_DESIGNER_ACT))
		AfxAttachCustomizationContextPointer(static_cast<ATTACHEVENT_FUNC>(&OnEventDispatch));

	return TRUE;
}

//-----------------------------------------------------------------------------
void BaseCustomizationContext_CustomizationContextCreation(System::Object^ e, CustomizationContextEventArgs^ args)
{
	args->CustomizationContext = gcnew Microarea::EasyBuilder::Packager::CustomizationContext();
}

//-----------------------------------------------------------------------------
BOOL ApplicationStarted(FunctionDataInterface* pRDI)
{
	BaseCustomizationContext::CustomizationContextCreation += gcnew EventHandler<CustomizationContextEventArgs^>(&BaseCustomizationContext_CustomizationContextCreation);
	return TRUE;
}

////////////////////////////////////////////////////////////////////////////
//				INIZIO definizione della interfaccia di Add-On
/////////////////////////////////////////////////////////////////////////////
//-----------------------------------------------------------------------------
BEGIN_ADDON_INTERFACE()

	BEGIN_TEMPLATE()
		BEGIN_DOCUMENT(_NS_DOC("NewDocument"), TPL_NO_PROTECTION)
			REGISTER_MASTER_TEMPLATE(szDefaultViewMode, CNewDocument, CMasterFrame, CDynamicFormView)
		END_DOCUMENT()

		BEGIN_DOCUMENT(_NS_DOC("EasyStudioDesigner"), TPL_NO_PROTECTION)
			REGISTER_MASTER_TEMPLATE(szDefaultViewMode, CEasyStudioDesignerDoc, CEasyStudioDesignerFrame, CEasyStudioDesignerView)
		END_DOCUMENT()

		BEGIN_DOCUMENT(_NS_DOC("NewBatchDocument"), TPL_NO_PROTECTION)
			REGISTER_MASTER_TEMPLATE(szDefaultViewMode, CNewBatchDocument, CBatchFrame, CDynamicFormView)
		END_DOCUMENT()
	END_TEMPLATE()

	BEGIN_FUNCTIONS()
		REGISTER_EVENT_HANDLER(_NS_EH("OnDSNChanged"), DsnChanged)
		REGISTER_EVENT_HANDLER(_NS_EH("ApplicationStarted"), ApplicationStarted)
	END_FUNCTIONS()

	BEGIN_TABLES()
		BEGIN_REGISTER_TABLES()
			REGISTER_VIRTUAL_TABLE(TEBVirtualTable)
		END_REGISTER_TABLES()
	END_TABLES()
	//-----------------------------------------------------------------------------
	BEGIN_FAMILY_CLIENT_DOC()
		WHEN_FAMILY_SERVER_DOC(CAbstractFormDoc)
			ATTACH_FAMILY_CLIENT_DOC_EXCLUDE_WIZARD_BATCH_FRAME(CDEasyBuilder, _NS_CD("EasyStudio"))
		END_FAMILY_SERVER_DOC()
	END_FAMILY_CLIENT_DOC()

END_ADDON_INTERFACE()


