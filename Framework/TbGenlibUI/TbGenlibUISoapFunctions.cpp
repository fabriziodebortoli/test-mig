
#include "StdAfx.h"

#include <TbGeneric\WebServiceStateObjects.h>
#include <TbGenlib\generic.hjson> //JSON AUTOMATIC UPDATE
#include <TbGenlib\ParsObj.h>
#include <TbGenlib\AboutBox.h>

#include <TbGenlibUI\FormatDialog.h>
#include <TbGenlibUI\FontsDialog.h>
#include <TbGenlibUI\TBExplorer.h>

#include <TbGenlibUI\FormatDialog.hjson> //JSON AUTOMATIC UPDATE
#include <TbGenlibUI\FontsDialog.hjson> //JSON AUTOMATIC UPDATE
#include <tbgenlibUI\TBManageFile.h>
#include <TbGenlib\Basefrm.hjson> //JSON AUTOMATIC UPDATE

#include <TbGenLibManaged\GlobalFunctions.h>

#include "BrowserDlg.h"
#include "soapfunctions.h"



//-----------------------------------------------------------------------------
///<summary>
///Opening a text file
///</summary>
//[TBWebMethod(defaultsecurityroles="aUnprotected Report Manager", woorm_method=false)]
DataBool ExecOpenText()
{
	CString sName;
	CTBNamespace aNameSpace (CTBNamespace::TEXT);
	
	CTBExplorer TbExplorer (CTBExplorer::OPEN, aNameSpace);
	if (!TbExplorer.Open())
		return FALSE;

	CStringArray aSelectedPaths;
	TbExplorer.GetSelPathElements(&aSelectedPaths);
	if (!aSelectedPaths.GetSize())
		return FALSE;

	return AfxGetTbCmdManager()->RunEditor(aSelectedPaths.GetAt(0)) != NULL;
}


//-----------------------------------------------------------------------------
///<summary>
///Opening format window
///</summary>
//[TBWebMethod(defaultsecurityroles="aUnprotected Report Manager", woorm_method=false, securityhidden=true)]
DataBool ExecOpenFormatter ()
{	
	if (!AfxGetLoginInfos()->m_bAdmin)
	{
		AfxMessageBox(_TB("Warning, this option is available only for administrator."));
		return FALSE;
	}

	FormatIdx   formatIdx	= 0;	
	CFormatDlg* pDialog		= new CFormatDlg(*(AfxGetWritableFormatStyleTable()), formatIdx, TRUE) ; // don't check formatIdx
	VERIFY(pDialog->DoModeless());
	
	return TRUE;
}

//-----------------------------------------------------------------------------
///<summary>
///Opening font style window
///</summary>
//[TBWebMethod(defaultsecurityroles="aUnprotected Report Manager", woorm_method=false, securityhidden=true)]
DataBool ExecOpenFont ()
{
	if (!AfxGetLoginInfos()->m_bAdmin)
	{
		AfxMessageBox(_TB("Warning, this option is available only for administrator."));
		return FALSE;
	}

	FontIdx nFontIdx = 0;
	
	CFontStylesDlg* pDialog	= new CFontStylesDlg(*AfxGetWritableFontStyleTable(), nFontIdx, TRUE) ; // don't check formatIdx
	VERIFY(pDialog->DoModeless());

	return TRUE;
}

//-----------------------------------------------------------------------------
///<summary>
///Opening manage file window
///</summary>
//[TBWebMethod(defaultsecurityroles="aUnprotected Report Manager", woorm_method=false)]
DataBool ExecManageFile ()
{
	CManageFileWizMasterDlg ManageFile;
	ManageFile.DoModal();

	return TRUE;
}

//-----------------------------------------------------------------------------
///<summary>
///Opening font style window
///</summary>
//[TBWebMethod(defaultsecurityroles="aUnprotected Report Manager", woorm_method=false)]
DataBool EnumsViewer ()
{
	return AfxRunEnumsViewer();
}

//-----------------------------------------------------------------------------
///<summary>
///Set application date
///</summary>
//[TBWebMethod(woorm_method=false)]
void SetApplicationDate (DataDate aDate)
{
	AfxSetApplicationDate(aDate);
}

//-----------------------------------------------------------------------------
///<summary>
///Set application date
///</summary>
//[TBWebMethod(woorm_method=false)]
void SetApplicationDateIMago(DataDate aDate)
{
	AfxSetApplicationDateIMago(aDate);
}

//-----------------------------------------------------------------------------
///<summary>
///Set application date like system date
///</summary>
//[TBWebMethod(woorm_method=false)]
void SetApplicationDateToSystemDate ()
{
	AfxSetApplicationDate(DataDate (TodayDate()));

	return;
}

//-----------------------------------------------------------------------------
///<summary>
///Set application date
///</summary>
//[TBWebMethod]
DataDate SetApplicationDate2 (DataDate aDate)
{
	return AfxSetApplicationDate(aDate);
}

//-----------------------------------------------------------------------------
///<summary>
///Show information about framework
///</summary>
//[TBWebMethod(woorm_method=false)]
void ShowAboutFramework ()
{
	CAboutBox* pDialog = new CAboutBox;
	VERIFY(pDialog->DoModeless());
	
	return;
}

//-----------------------------------------------------------------------------
///<summary>
///Opening a report file
///</summary>
//[TBWebMethod(defaultsecurityroles="aUnprotected Report Manager", woorm_method=false)]
DataBool ExecOpenReport(DataStr&/*ciString*/ reportNamespace, DataStr&/*ciString*/ reportPath)
{
       CString sName;
       CTBNamespace aNameSpace (CTBNamespace::REPORT);
       
       CTBExplorer TbExplorer (CTBExplorer::OPEN, aNameSpace);
       if (!TbExplorer.Open())
             return FALSE;

       CStringArray aSelectedPaths;
       TbExplorer.GetSelPathElements(&aSelectedPaths);
       if (!aSelectedPaths.GetSize())
             return FALSE;
       TbExplorer.GetSelNameSpace(aNameSpace);
       
       reportNamespace = aNameSpace.ToString();
	   reportPath = aSelectedPaths.GetAt(0);
	   return TRUE;
}
