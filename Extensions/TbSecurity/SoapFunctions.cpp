
#include "StdAfx.h"

#include "soapfunctions.h"
#include "OslSecurityInterface.h"


//----------------------------------------------------------------------------
///<summary>
///Initialize Security module
///</summary>
//[TBWebMethod(securityhidden=true, woorm_method=false)]
DataBool StartSecurity()
{
	try
	{
		(new CSecurityInterface())->AttachToLoginContext();
		return TRUE;
	}
	catch (CException* e)
	{
		AfxGetDiagnostic()->Add(_TB("Failed to start security"));
		AfxGetDiagnostic()->Add(e);
		e->Delete();
		return FALSE;
	}
}