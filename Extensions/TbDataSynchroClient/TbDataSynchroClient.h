// TbDataSynchroClient.h : main header file for the TbDataSynchroClient DLL
//

#pragma once

#ifndef __AFXWIN_H__
	#error "include 'stdafx.h' before including this file for PCH"
#endif

#include "resource.h"		// main symbols


// CTbDataSynchroClientApp
// See TbDataSynchroClient.cpp for the implementation of this class
//

class CTbDataSynchroClientApp : public CWinApp
{
public:
	CTbDataSynchroClientApp();

// Overrides
public:
	virtual BOOL InitInstance();

	DECLARE_MESSAGE_MAP()
};
