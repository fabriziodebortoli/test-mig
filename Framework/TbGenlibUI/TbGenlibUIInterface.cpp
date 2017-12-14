
#include "stdafx.h" 

// library declaration
#include <tbgenlib\messages.h>
#include <tbgenlib\baseapp.h>
#include <tbgenlib\interfacemacros.h>
#include <tbgenlib\TbCommandInterface.h>
#include "TbExplorer.h"

#include "paddoc.h"
#include "paddoc.hjson"
//includere come ultimo include all'inizio del cpp
#include "begincpp.dex"

#ifdef _DEBUG
#undef THIS_FILE
static const char THIS_FILE[] = __FILE__;
#endif


/////////////////////////////////////////////////////////////////////////////
//				INIZIO definizione della interfaccia di Add-On
/////////////////////////////////////////////////////////////////////////////
//
#define _AddOn_Interface_Of tbgenlibuitbgenlibui

//-----------------------------------------------------------------------------
BEGIN_ADDON_INTERFACE()
	//-----------------------------------------------------------------------------
	BEGIN_TEMPLATE()
		BEGIN_DOCUMENT (_NS_DOC("TbEditor"), TPL_NO_PROTECTION)
			nResource = IDR_EDITOR;
		REGISTER_MASTER_TEMPLATE(szDefaultViewMode, CPadDoc, CPadFrame, CPadView)
		END_DOCUMENT ()
	END_TEMPLATE()
END_ADDON_INTERFACE()
#undef _AddOn_Interface_Of