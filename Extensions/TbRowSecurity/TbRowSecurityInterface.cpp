
#include "stdafx.h" 

#include <TbNameSolver\CompanyContext.h>
#include <TbGenlib\messages.h>
#include <TbGenlib\baseapp.h>
#include <TbOleDb\InterfaceMacros.h>
#include <TbOleDb\SqlConnect.h>
#include <TBGES\InterfaceMacros.h>

#include "RSStructures.h"
#include "RSManager.h"
#include "RSTables.h"
#include "UIRSGrants.h"
#include "CDRSGrants.h"
#include "BDRSMaintenance.h"
#include "UIRSMaintenance.h"



//----------------------------------------------------------------------------
//[TBWebMethod(name = RunRSMaintenance , woorm_method=false)]
///<summary>
///Row Security Configuration and Maintenance Procedure
///</summary>
/// <remarks>This function runs batch procedure Row Security Configuration and Maintenance</remarks>
/// <returns>No return value</returns>
void RunRSMaintenance()
{
	if (!AfxRowSecurityEnabled())
	{
		AfxMessageBox(_TB("Unable to open Grants Layout data-entry!\r\nPlease, check in Administration Console if this company uses RowSecurity."));
		return;
	}

	CString strDocNamespace = AfxGetRowSecurityManager()->GetMaintenanceDocNamespace();
	//se l'applicativo mi ha dato il nome di una funzione/documento da lanciare allora lancio la funzione passata altrimenti
	if (strDocNamespace.IsEmpty())
		AfxGetTbCmdManager()->RunDocument(_T("Extensions.TbRowSecurity.TbRowSecurity.RSMaintenance"), szDefaultViewMode, TRUE);
	else
	{
		//verifico se il namespace è di tipo documento o funzione
		CTBNamespace docNamespace(strDocNamespace);
		CTBNamespace::NSObjectType objType = docNamespace.GetType();
		switch (objType)
		{
			case CTBNamespace::FUNCTION:
				AfxGetTbCmdManager()->RunFunction(strDocNamespace); break;
			case CTBNamespace::DOCUMENT:
				AfxGetTbCmdManager()->RunDocument(strDocNamespace); break;
			default: break;
		}
	}
}

/////////////////////////////////////////////////////////////////////////////
//				AddOn-Interface declaration
/////////////////////////////////////////////////////////////////////////////
//---------------------------------------------------------------------------------
BOOL InitRowSecurityManager(FunctionDataInterface* pRDI)
{
	if (!AfxRowSecurityEnabled())
		return TRUE;

	CCompanyContext* pContext = AfxGetCompanyContext();
	if (pContext && !pContext->GetRowSecurityManager())
	{
		CRowSecurityManager* pRowSecurityMng = new CRowSecurityManager();
		pContext->AttachRowSecurityManager(pRowSecurityMng);
		pRowSecurityMng->LoadProtectionInformation();

		if (!pRowSecurityMng->IsValid())
			AfxGetDiagnostic()->Add(_TB("In order to preserve the Row Security Layer, you have to run the Row Security Configuration procedure!"), CDiagnostic::Warning);
	}
	
	return TRUE;
}


//---------------------------------------------------------------------------------
BEGIN_ADDON_INTERFACE()

//-----------------------------------------------------------------------------
BEGIN_TABLES()
	BEGIN_REGISTER_TABLES()
		REGISTER_TABLE(TRS_Subjects)
		REGISTER_TABLE(TRS_SubjectsHierarchy)
		REGISTER_TABLE(TRS_SubjectsGrants)
		REGISTER_TABLE(TRS_TmpOldHierarchies)
		REGISTER_TABLE(TRS_Configuration)
		REGISTER_VIRTUAL_TABLE(VRSEntities);
		END_REGISTER_TABLES()
	END_TABLES()

	//l'aggiunta della colonna RowSecurityID la faccio utilizzando la tecnica degli AddOnNewColumn ma visto che non so a priori il nome della tabella a cui aggiungere la colonna (dipende delle entità
	//apportate dal programma gestionale) effettuo l'add utilizzando direttamente il metodo virtuale anzichè chiamare le macro
	//-----------------------------------------------------------------------------
	virtual void AOI_AddOnNewColumns(const SqlCatalogEntry* pCatalogEntry, CRTAddOnNewFieldsArray* pSqlAddOnColumnsInfo, const CString& strSignature, const CTBNamespace& aNamespace) 
	{
		//if (AfxRowSecurityEnabled() && AfxGetRowSecurityManager()->IsEntityMasterTable(pCatalogEntry->GetNamespace()))
		if (AfxRowSecurityEnabled())
			pSqlAddOnColumnsInfo->Add(new CRTAddOnNewFields(RUNTIME_CLASS(RowSecurityAddOnFields), strSignature, pCatalogEntry->GetSqlRecordClass(), aNamespace));
	}

	//-----------------------------------------------------------------------------
	BEGIN_FUNCTIONS()
		REGISTER_EVENT_HANDLER	(szOnDSNChanged,	InitRowSecurityManager)
	END_FUNCTIONS()

	//-----------------------------------------------------------------------------
	BEGIN_FAMILY_CLIENT_DOC()
		WHEN_FAMILY_SERVER_DOC(CAbstractFormDoc)
		EXCLUDE_DOC_FROM_ATTACH_WHEN(!AfxRowSecurityEnabled() || !AfxGetRowSecurityManager()->IsValid() || (!AfxGetRowSecurityManager()->GetEntityInfo(pDoc->GetNamespace()) || !AfxGetRowSecurityManager()->GetEntityInfo(pDoc->GetNamespace())->m_bUsed))
			ATTACH_FAMILY_CLIENT_DOC_EXCLUDE_BATCH_MODE(CRSGrantsClientDoc,	_NS_CD("CDRSGrants"))
		END_FAMILY_SERVER_DOC()
	END_FAMILY_CLIENT_DOC()

	//-----------------------------------------------------------------------------
	BEGIN_TEMPLATE()
		
		REGISTER_BASE_TEMPLATE (CAbstractFormDoc,	CRSEntityGrantsFrame,	CRSEntityGrantsView)
	
		BEGIN_DOCUMENT(_NS_DOC("RSMaintenance"), TPL_NO_PROTECTION)
			REGISTER_MASTER_TEMPLATE(szDefaultViewMode, BDRSMaintenance, CRSMaintenanceFrame, CRSMaintenanceView)
		END_DOCUMENT()
	END_TEMPLATE()
	
END_ADDON_INTERFACE()