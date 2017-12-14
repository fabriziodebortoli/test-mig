
#include "stdafx.h"
//#include "afxpriv.h"

#include <TbGeneric\Tools.h>

#include <TbClientCore\ClientObjects.h>
#include <TbWebServicesWrappers\LoginManagerInterface.h>

#include "baseapp.h"
#include "OslBaseInterface.h"

//includere come ultimo include all'inizio del cpp
#include "begincpp.dex"

#ifdef _DEBUG
#undef THIS_FILE
static const char BASED_CODE THIS_FILE[] = __FILE__;
#endif

//=============================================================================

///////////////////////////////////////////////////////////////////////////////
//								CBaseSecurityInterface
///////////////////////////////////////////////////////////////////////////////
IMPLEMENT_DYNAMIC(CBaseSecurityInterface, CObject);

//-------------------------------------------------------------------
CBaseSecurityInterface::CBaseSecurityInterface ()
{
	
}

//------------------------------------------------------------------------------
CBaseSecurityInterface::~CBaseSecurityInterface ()
{
}

//------------------------------------------------------------------------------
BOOL CBaseSecurityInterface::GetObjectGrant (CInfoOSL* pInfoOSL)
{ 
	ASSERT(pInfoOSL);
	ASSERT(pInfoOSL->GetType() >= OSLType_Null && pInfoOSL->GetType() < OSLType_Wrong);

	pInfoOSL->m_dwGrant = 
				OSL_GRANT_PROTECTION_FLAG_KNOWN | 
				OSL_GRANT_NOT_PROTECTED | 
				0xFFFF /*| (CInfoOSL::m_ardwAllGrantForObjectType[pInfoOSL->m_eType])*/;

	return TRUE; 
}

//-----------------------------------------------------------------------------
BOOL IsFunctionAllowed(LPCTSTR szNsFun)
{
	if (AfxGetLoginInfos()->m_bAdmin)
		return TRUE;

	CInfoOSL infoOsl (CTBNamespace(szNsFun), OSLType_Function);
	AfxGetSecurityInterface()->GetObjectGrant (&infoOsl);
	BOOL bProtected = OSL_IS_PROTECTED(&infoOsl);
		
	if (!bProtected || !OSL_CAN_DO(&infoOsl, OSL_GRANT_EXECUTE))
	{
		AfxTBMessageBox(cwsprintf(_TB("{0-%s} is available only to the system administrator or when it is protected by Security and user has grants"), szNsFun), MB_ICONWARNING| MB_OK);
		return FALSE;
	}

	return TRUE;
}

//=============================================================================
