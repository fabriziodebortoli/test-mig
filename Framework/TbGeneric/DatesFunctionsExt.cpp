#include "stdafx.h"

#include <TbGeneric\TbStrings.h>

#include "DatesFunctions.h"

//includere come ultimo include all'inizio del cpp
#include "begincpp.dex"

#ifdef _DEBUG
#undef THIS_FILE
static const char THIS_FILE[] = __FILE__;
#endif

// return the month name
//
//----------------------------------------------------------------------------
CString MonthName (WORD wMonth)
{
	switch 	(wMonth)
	{
		case 1 : return _TB("January");
		case 2 : return _TB("February");	
		case 3 : return _TB("March");	
		case 4 : return _TB("April");	
		case 5 : return _TB("May");	
		case 6 : return _TB("June");	
		case 7 : return _TB("July");	
		case 8 : return _TB("August");	
		case 9 : return _TB("September");	
		case 10 : return _TB("October");	
		case 11 : return _TB("November");	
		case 12 : return _TB("December");	
	}              
	
	return _T("");
}
