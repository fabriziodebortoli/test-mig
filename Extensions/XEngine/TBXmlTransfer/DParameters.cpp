#include "stdafx.h"

#include <TBGENLIB\DirTreeCtrl.h>

#include <XEngine\TBXMLEnvelope\TXEParameters.h>
#include <XEngine\TBXMLEnvelope\GenFunc.h>
#include <XEngine\TBXMLEnvelope\XEngineObject.h>

#include "DParameters.h"
#include "UIParameters.hjson"

static TCHAR szP1[] = _T("P1");

//////////////////////////////////////////////////////////////////////////////
//							DBTXEParameters								    //
//////////////////////////////////////////////////////////////////////////////
//-----------------------------------------------------------------------------	
IMPLEMENT_DYNAMIC(DBTXEParameters, DBTMaster)

//-----------------------------------------------------------------------------	
DBTXEParameters::DBTXEParameters
	(
		CRuntimeClass*		pClass, 
		CAbstractFormDoc*	pDocument
	)
	:
	DBTMaster (pClass, pDocument, _NS_DBT("XEParameters"))
{
}

//-----------------------------------------------------------------------------	
void DBTXEParameters::OnPrepareBrowser(SqlTable* pTable)
{
	pTable->SelectAll();
}

//-----------------------------------------------------------------------------
void DBTXEParameters::OnDefineQuery()
{
	m_pTable->SelectAll();
}

//-----------------------------------------------------------------------------
void DBTXEParameters::OnPrepareQuery()
{
}

//-----------------------------------------------------------------------------
BOOL DBTXEParameters::OnCheckPrimaryKey()
{
	return TRUE;
}

//-----------------------------------------------------------------------------
void DBTXEParameters::OnPreparePrimaryKey()
{
}

//--------------------------------------------------------------------------------
void DBTXEParameters::OnDisableControlsForEdit()
{
}

//////////////////////////////////////////////////////////////////////////////
//						DXEParameters				                        //
//////////////////////////////////////////////////////////////////////////////
//-----------------------------------------------------------------------------
IMPLEMENT_DYNCREATE(DXEParameters, CAbstractFormDoc)

//-----------------------------------------------------------------------------
BEGIN_MESSAGE_MAP(DXEParameters, CAbstractFormDoc)
	//{{AFX_MSG_MAP(DXEParameters)
	ON_EN_VALUE_CHANGED(IDC_XE_PARAMS_SITE_NAME, OnSiteChanged)
	ON_EN_VALUE_CHANGED(IDC_XE_PARAMS_IMPORT_PATH, OnImportPathChanged)
	ON_EN_VALUE_CHANGED(IDC_XE_PARAMS_EXPORT_PATH, OnExportPathChanged)
	//}}AFX_MSG_MAP
END_MESSAGE_MAP()

//-----------------------------------------------------------------------------
TXEParameters* DXEParameters::GetXEParameters() const { return (TXEParameters*)m_pDBTXEParameters->GetRecord(); }

//-----------------------------------------------------------------------------
BOOL DXEParameters::OnAttachData()
{ 
	SetFormTitle(_TB("XEngine Parameters"));
	
	SetOnlyOneRecord(TRUE);

	m_pDBTXEParameters = new DBTXEParameters(RUNTIME_CLASS(TXEParameters), this);

	return Attach(m_pDBTXEParameters);
}

//-----------------------------------------------------------------------------
BOOL DXEParameters::OnPrepareAuxData()
{   
	// serve?
	//m_bUTF16 = !GetXEParameters()->f_EncodTypeUTF8;
	//UpdateDataView();

	if (
		GetFormMode() != CAbstractFormDoc::BROWSE &&
		(GetXEParameters()->f_ExportPath.IsEmpty() || GetXEParameters()->f_ImportPath.IsEmpty())
		)
	{
		if (GetXEParameters()->f_ExportPath.IsEmpty())
			GetXEParameters()->f_ExportPath = AfxGetPathFinder()->TransformInRemotePath(AfxGetDynamicInstancePath());

		if (GetXEParameters()->f_ImportPath.IsEmpty())
			GetXEParameters()->f_ImportPath = AfxGetPathFinder()->TransformInRemotePath(AfxGetDynamicInstancePath());

		SetModifiedFlag(TRUE);
		UpdateDataView();
	}

	if (m_pMaxDoc)
		m_pMaxDoc->SetModifyFlag(TRUE);
	if (m_pMaxKByte)
		m_pMaxKByte->SetModifyFlag(TRUE);
	if (m_pPaddingNum)
		m_pPaddingNum->SetModifyFlag(TRUE);

	return TRUE;
}

//-----------------------------------------------------------------------------
BOOL DXEParameters::OnOkTransaction()
{
	if (!CAbstractFormDoc::OnOkTransaction())
		return FALSE;

	// era presente solo nel codice dei parametri di import/export (giusto lasciarlo?)
	if (AfxGetLoginContext()->GetOpenDocuments() > 1)
	{
		Message(_TB("Before saving you have to close all other opened documents"));
		return FALSE;
	}

	TXEParameters* pRec = GetXEParameters();

	if (!pRec)
	{
		ASSERT(FALSE);
		return FALSE;
	}

	if (pRec->f_SiteName.IsEmpty() || pRec->f_SiteCode.IsEmpty() || pRec->f_DomainName.IsEmpty())
		m_pMessages->Add(_TB("Warning: you need to specify the site name, the site code and the domain name for the process run properly."));

	if (!pRec->f_ImportPath.IsEmpty() && !ExistPath(pRec->f_ImportPath.GetString()))
		m_pMessages->Add(cwsprintf(_TB("The specified import path:\r\n\t{0-%s}\r\ndoes not exist."), (LPCTSTR)pRec->f_ImportPath.GetString()));

	if (!pRec->f_ExportPath.IsEmpty() && !ExistPath(pRec->f_ExportPath.GetString()))
		m_pMessages->Add(cwsprintf(_TB("The specified export path:\r\n\t{0-%s}\r\ndoes not exist."), (LPCTSTR)pRec->f_ExportPath.GetString()));

	if (m_pMessages->ErrorFound())
	{
		m_pMessages->Show(TRUE);
		return FALSE;
	}

	return TRUE;
}

//-----------------------------------------------------------------------------
BOOL DXEParameters::CanDoEditRecord()
{
	return CAbstractFormDoc::CanDoEditRecord() && AfxGetLoginInfos()->m_bAdmin;
}

//-----------------------------------------------------------------------------
BOOL DXEParameters::CanDoNewRecord()
{
	return CAbstractFormDoc::CanDoNewRecord() && AfxGetLoginInfos()->m_bAdmin;
}

// aggiorna i parametri 
//-----------------------------------------------------------------------------
void DXEParameters::SaveXEParameters()
{
	AfxGetXEngineObject()->UpdateParameters(GetXEParameters());
}

//-----------------------------------------------------------------------------
void DXEParameters::CheckIsLocalPath(const CString& strPath)
{
	if (!strPath.IsEmpty() && !IsServerPath(strPath))
		AfxMessageBox(cwsprintf(_TB("The specified path:\r\n\t{0-%s}\ris local on this computer, so is not visible from the other remote clients.\r\nIf other remote clients use xml export/import procedure you are suggested to set a network path."), strPath));
}

//-----------------------------------------------------------------------------
void DXEParameters::OnImportPathChanged()
{
	CheckIsLocalPath(GetXEParameters()->f_ImportPath);
	UpdateDataView();
}

//-----------------------------------------------------------------------------
void DXEParameters::OnExportPathChanged()
{
	CheckIsLocalPath(GetXEParameters()->f_ExportPath);
	UpdateDataView();
}

//-----------------------------------------------------------------------------
void DXEParameters::OnSiteChanged()
{
	TXEParameters*	pRec = GetXEParameters();
	if (!pRec)
		return;

	if (pRec->f_SiteCode.IsEmpty())
		pRec->f_SiteCode = pRec->f_SiteName.GetString().Left(LEN_SITE_CODE);
	UpdateDataView();
}