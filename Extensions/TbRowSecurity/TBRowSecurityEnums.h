//=================================================================
// module name	: TBRowSecurityEnums.h 
//=================================================================

#pragma once

const WORD    E_GRANT_TYPE																	= 32759;
const DWORD   E_GRANT_TYPE_DENY																= MAKELONG(0, 32759);  //2146893824
const DWORD   E_GRANT_TYPE_READ_ONLY														= MAKELONG(1, 32759);  //21468938265
const DWORD   E_GRANT_TYPE_READWRITE														= MAKELONG(2, 32759); //2146893826
const DWORD   E_GRANT_TYPE_DEFAULT															= E_GRANT_TYPE_DENY;

const WORD    E_GRANT_RULE_TYPE																= 32758;
const DWORD   E_GRANT_RULE_TYPE_ALL															= MAKELONG(0, 32758); //
const DWORD   E_GRANT_RULE_TYPE_EXCEPT														= MAKELONG(1, 32758); //
const DWORD   E_GRANT_RULE_TYPE_ONLY														= MAKELONG(2, 32758); //
const DWORD   E_GRANT_RULE_TYPE_DEFAULT														= E_GRANT_RULE_TYPE_ALL;
