
#include "stdafx.h"
//#include "afxpriv.h"

#include <TbClientCore\ClientObjects.h>
#include <TbWebServicesWrappers\LoginManagerInterface.h>

#include <TbGeneric\Tools.h>
#include <TbGeneric\LocalizableObjs.h>

#include "funproto.h"
#include "baseapp.h"

#include "Oslinfo.h"
#include "oslbaseinterface.h"

#include "parsobj.h"

//includere come ultimo include all'inizio del cpp
#include "begincpp.dex"

#ifdef _DEBUG
#undef THIS_FILE
static const char BASED_CODE THIS_FILE[] = __FILE__;
#endif

IMPLEMENT_DYNAMIC(COslTreeItem, CObject);

//=================================================================================================
TB_EXPORT BOOL OSLCheckConstraint
		(
			const CTBNamespace& ns, int nGrant, 
			BOOL bDefault, BOOL bOnlyAdmin
		);



///////////////////////////////////////////////////////////////////////////////
//								CInfoOSL
///////////////////////////////////////////////////////////////////////////////
CString CInfoOSL::FormatType(OSLTypeObject	eType)
{
	switch(eType)
	{
		case OSLType_Report:
			return _TB("Report");

		case OSLType_Template:
			return _TB("Document - Data Entry");
		case OSLType_BatchTemplate:
			return _TB("Document - Procedure Batch");
		case OSLType_FinderDoc:
			return _TB("Document - Finder");

		case OSLType_SlaveTemplate:
			return _TB("Form - Slave");
		case OSLType_RowSlaveTemplate:
			return _TB("Form - Row View");
		case OSLType_EmbeddedView:
			return _TB("Form - Embedded View");

		case OSLType_TabDialog:
			return _TB("Tab");
		case OSLType_Tabber:
			return _TB("Tabber");

		case OSLType_Tile:
			return _TB("Tile");
		case OSLType_TileManager:
			return _TB("Tile Manager");

		case OSLType_TilePanelTab:
			return _TB("Tile Panel Tab");
		case OSLType_TilePanel:
			return _TB("Tile Panel");

		case OSLType_PropertyGrid:
			return _TB("Property Grid");

		case OSLType_Toolbar:
			return _TB("Toolbar");
		case OSLType_ToolbarButton:
			return _TB("Toolbar Button");

		case OSLType_BodyEdit:
			return _TB("Grid");
		case OSLType_BodyEditColumn:
			return _TB("Grid column");

		case OSLType_Control:
			return _TB("Control");

		case OSLType_Skip:
			return _TB("<Skip>");
		case OSLType_Null:
			return _TB("<Null>");
		case OSLType_Wrong:
			return _TB("<Wrong>");
	}
	return _TB("<unknown type>") + ':' + cwsprintf(L"%d", eType);
}

const DWORD* CInfoOSL::m_ardwAllGrantForObjectType = CInfoOSL::GetAllGrantForObjectType();

//-----------------------------------------------------------------------------
DWORD* CInfoOSL::GetAllGrantForObjectType ()
{
	static DWORD ar [ OSLType_Wrong ];

	for (int i = 0; i < OSLType_Wrong; i++)
		ar[i] = OSL_GRANT_EXECUTE;
	
	ar [OSLType_Null]			= OSL_GRANT_ALL_GRANT;
	ar [OSLType_Skip]			= OSL_GRANT_ALL_GRANT;
	//----

	ar [OSLType_AddOnApp]		= OSL_GRANT_SILENTEXECUTE | OSL_GRANT_EXECUTE | OSL_GRANT_NOT_PROTECTED;
	ar [OSLType_AddOnMod]		= OSL_GRANT_SILENTEXECUTE | OSL_GRANT_EXECUTE | OSL_GRANT_NOT_PROTECTED;
	
	ar [OSLType_Report]			= OSL_GRANT_EXECUTE | OSL_GRANT_EDIT | OSL_GRANT_EXPORT;

	ar [OSLType_BatchTemplate]	= OSL_GRANT_SILENTEXECUTE | OSL_GRANT_EXECUTE | OSL_GRANT_CUSTOMIZEFORM;
	ar [OSLType_BkgAdmTemplate] = OSL_GRANT_SILENTEXECUTE | OSL_GRANT_EXECUTE;
	ar [OSLType_FinderDoc] = 
		(OSL_GRANT_EXECUTE | OSL_GRANT_EDIT | OSL_GRANT_BROWSE | OSL_GRANT_CUSTOMIZEFORM | OSL_GRANT_EDITQUERY);

	ar [OSLType_Template]		= 
		(OSL_GRANT_SILENTEXECUTE | OSL_GRANT_EXECUTE | OSL_GRANT_BROWSE | OSL_GRANT_BROWSE_EXTENDED | OSL_GRANT_EDIT | OSL_GRANT_NEW | OSL_GRANT_DELETE | OSL_GRANT_CUSTOMIZEFORM | OSL_GRANT_EDITQUERY);
	ar [OSLType_SlaveTemplate]	= 
		(OSL_GRANT_EXECUTE | OSL_GRANT_EDIT | OSL_GRANT_NEW);
	ar [OSLType_EmbeddedView]	= 
		(OSL_GRANT_EXECUTE | OSL_GRANT_EDIT | OSL_GRANT_NEW);

	ar [OSLType_RowSlaveTemplate] = 
		(OSL_GRANT_EXECUTE | OSL_GRANT_EDIT | OSL_GRANT_NEW | OSL_GRANT_BE_ADDROW | OSL_GRANT_BE_DELETEROW);
	ar [OSLType_BodyEdit]		= 
		(OSL_GRANT_EXECUTE | OSL_GRANT_EDIT | OSL_GRANT_NEW | OSL_GRANT_BE_ADDROW | OSL_GRANT_BE_DELETEROW | OSL_GRANT_BE_SHOWROWVIEW);
	ar [OSLType_BodyEditColumn]	= 
		(OSL_GRANT_EXECUTE | OSL_GRANT_EDIT | OSL_GRANT_NEW);

	ar [OSLType_TabDialog]		= 
		(OSL_GRANT_EXECUTE | OSL_GRANT_EDIT | OSL_GRANT_NEW);
	ar [OSLType_Tabber]		= 
		(OSL_GRANT_EXECUTE | OSL_GRANT_EDIT | OSL_GRANT_NEW);
	
	ar [OSLType_Tile]		= 
		(OSL_GRANT_EXECUTE | OSL_GRANT_EDIT | OSL_GRANT_NEW);
	ar [OSLType_TileManager]		= 
		(OSL_GRANT_EXECUTE | OSL_GRANT_EDIT | OSL_GRANT_NEW);

	ar[OSLType_TilePanelTab] =
		(OSL_GRANT_EXECUTE | OSL_GRANT_EDIT | OSL_GRANT_NEW);
	ar[OSLType_TilePanel] =
		(OSL_GRANT_EXECUTE | OSL_GRANT_EDIT | OSL_GRANT_NEW);

	ar[OSLType_PropertyGrid] =
		(OSL_GRANT_EXECUTE | OSL_GRANT_EDIT | OSL_GRANT_NEW);

	ar [OSLType_Toolbar]		= 
		(OSL_GRANT_EXECUTE | OSL_GRANT_EDIT | OSL_GRANT_NEW);
	ar [OSLType_ToolbarButton]		= 
		(OSL_GRANT_EXECUTE | OSL_GRANT_EDIT | OSL_GRANT_NEW);

	ar [OSLType_Control]		= 
		(OSL_GRANT_EXECUTE | OSL_GRANT_EDIT | OSL_GRANT_NEW);

	ar [OSLType_Table]			= OSL_GRANT_EXECUTE | OSL_GRANT_EXPORT | OSL_GRANT_IMPORT;
	ar [OSLType_View]			= OSL_GRANT_EXECUTE | OSL_GRANT_EXPORT | OSL_GRANT_IMPORT;

	ar [OSLType_Function]		= OSL_GRANT_EXECUTE;
	ar [OSLType_HotLink]		= OSL_GRANT_EXECUTE | OSL_GRANT_EXPORT | OSL_GRANT_EDITQUERY;

	//ar [OSLType_OfficeDocument]		= OSL_GRANT_EXECUTE;
	//ar [OSLType_ClientDoc]			= OSL_GRANT_EXECUTE;
	//ar [OSLType_Constraint]			= OSL_GRANT_ALL_GRANT;
	//ar [OSLType_DBT]				= (OSL_GRANT_EXECUTE | OSL_GRANT_EDIT | OSL_GRANT_NEW | OSL_GRANT_DELETE);

	return ar;
}
//------------------------------------------------------------------------------

void CInfoOSL::SetDefaultGrant()
{
	BOOL bOk = m_eType >= OSLType_Null && m_eType < OSLType_Wrong;
	//se ha fallito vuol dire che c'e' un Add-On Module NON correttamente inizializzato 
	if (!bOk)
	{
		CString strSpiegazione = 
_T("BUG: probably a bad compiled DLL or a previous version of it has been loaded  (could be loaded from an uncorrect folder path by the operating system)\n")
_T("FIX: build all the dlls specified into the configuration file or disable the ones that cannot be loaded\n")
_T("THE PROGRAM IS RUNNING IN A BAD STATE: please, exit now.");

		TRACE(strSpiegazione);
		AfxMessageBox(strSpiegazione, MB_ICONSTOP);
	}
	ASSERT(bOk);
	//----

	if (m_eType == OSLType_BatchTemplate || m_eType == OSLType_FinderDoc)
		m_dwGrant = bOk ? m_ardwAllGrantForObjectType [OSLType_Template] : m_ardwAllGrantForObjectType [OSLType_Null];	
	else
		m_dwGrant = bOk ? m_ardwAllGrantForObjectType [m_eType] : m_ardwAllGrantForObjectType [OSLType_Null];	

	m_dwInheritMask = 0;
}

//-------------------------------------------------------------------
BOOL CInfoOSL::CanDo(DWORD grant)
{
	ASSERT(m_eType >= OSLType_Null && m_eType < OSLType_Wrong);
	
	if ( (m_dwGrant & OSL_GRANT_PROTECTION_FLAG_KNOWN) == 0 )
		if ( ! AfxGetSecurityInterface()->GetObjectGrant(this) )
			return FALSE;

	return (m_dwGrant & grant) == grant;
}


