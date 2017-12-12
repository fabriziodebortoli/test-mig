#include "stdafx.h"

#include "XMLDocGenerator.hjson" //JSON AUTOMATIC UPDATE
#include "AdministrationTools.h"



//==============================================================================
//          Class CAdminDocDescriptionDlg declaration
//==============================================================================
//
//------------------------------------------------------------------------------
IMPLEMENT_DYNAMIC(CAdminDocDescriptionDlg, CParsedDialogWithTiles)
//------------------------------------------------------------------------------
BEGIN_MESSAGE_MAP(CAdminDocDescriptionDlg, CParsedDialogWithTiles)
	
END_MESSAGE_MAP()

//------------------------------------------------------------------------------
CAdminDocDescriptionDlg::CAdminDocDescriptionDlg()
	:
	CParsedDialogWithTiles(IDD_ADMINTOOLS_DOCDESCRI, NULL, _NS_DLG("Framework.TbGes.TbGes.Admin.DocDescription"))
{

}
