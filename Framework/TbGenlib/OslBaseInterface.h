
#pragma once

#include <TBNameSolver\LoginContext.h>

#include <TbGeneric\Array.h>
#include <TbGeneric\LocalizableObjs.h>

#include "baseapp.h"					

#include "oslinfo.h"	
#include "TbCommandInterface.h"

//includere alla fine degli include del .H
#include "beginh.dex"
//=============================================================================

///////////////////////////////////////////////////////////////////////////////
//								CBaseSecurityInterfaceOSL
///////////////////////////////////////////////////////////////////////////////

class TB_EXPORT CBaseSecurityInterface: public CObject
{
	DECLARE_DYNAMIC(CBaseSecurityInterface);

	virtual void	ResetKnownFlags () {}

public:
	CBaseSecurityInterface ();
	virtual			~CBaseSecurityInterface ();

	virtual BOOL	IsSecurityEnabled () const { return FALSE; } 

	virtual BOOL	IsSuperUser () const { return FALSE; }

	virtual BOOL	GetObjectGrant (CInfoOSL*);

	virtual CPropertyPage* OpenOslAdminDlgProtectDoc(CLocalizablePropertySheet*, CBaseDocument*) { return NULL; }

	void AttachToLoginContext() { AfxGetLoginContext()->AttachSecurityInterface(this); }
};

//-----------------------------------------------------------------------------
inline CBaseSecurityInterface* AFXAPI AfxGetSecurityInterface()
{
	CLoginContext* pContext = AfxGetLoginContext();
	if (!pContext)
		return NULL;
	return pContext->GetObject<CBaseSecurityInterface>(&CLoginContext::GetSecurityInterface);
}

//-----------------------------------------------------------------------------
//return TRUE for Admin or when the function in protected by security
TB_EXPORT BOOL IsFunctionAllowed(LPCTSTR szNsFun);
//=============================================================================
#include "endh.dex"
