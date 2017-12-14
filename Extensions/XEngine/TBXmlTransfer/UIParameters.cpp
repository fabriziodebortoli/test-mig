
#include "stdafx.h"

#include <TBGENLIB\DirTreeCtrl.h>

#include <TBOLEDB\Sqltable.h>

#include <TbClientCore\ClientObjects.h>
#include <TbWebServicesWrappers\LoginManagerInterface.h>

#include <TBGES\dbt.h>
#include <TBGES\tabber.h>

#include <XEngine\TBXMLEnvelope\TXEParameters.h>

#include <XEngine\TBXMLEnvelope\GenFunc.h>
#include <XEngine\TBXMLEnvelope\XEngineObject.h>

#include <TBGES\extdoc.hjson> //JSON AUTOMATIC UPDATE
#include <XEngine\TBXMLEnvelope\XMLEnvMng.hjson> //JSON AUTOMATIC UPDATE

// local declarations
#include "DParameters.h"  
#include "UIParameters.h"  
#include "UIParameters.hjson"  

#define RDCPROP(a,b,c,d)	(\
	a->IsVirtual(a->Lookup(&(a->b))) ? a->GetColumnName(&(a->b)) : a->GetTableName() + _T("_") + a->GetColumnName(&(a->b))\
	),c,d,&(a->b)


static TCHAR szGlobal		[] = _T("Global");
static TCHAR szEngineServer	[] = _T("XEngineServer");
static TCHAR szEnginePing	[] = _T("XEngineCliPingInt");


typedef NET_API_STATUS (CALLBACK *PFUNC)(LPCWSTR, DWORD, LPBYTE *, DWORD, LPDWORD, LPDWORD, DWORD, LPCWSTR, LPDWORD);
	

//////////////////////////////////////////////////////////////////////////////
//					CXEParametersView
//////////////////////////////////////////////////////////////////////////////
//-----------------------------------------------------------------------------
IMPLEMENT_DYNCREATE(CXEParametersView, CMasterFormView)

//-----------------------------------------------------------------------------
CXEParametersView::CXEParametersView()
	:
	CMasterFormView(_NS_VIEW("XEngineParameters"), IDD_XE_PARAMETERS)
{
}

//-----------------------------------------------------------------------------
void CXEParametersView::BuildDataControlLinks()
{     
	AddTileGroup
		(
		IDC_TG_XE_PARAMETERS,
		RUNTIME_CLASS(CXEParametersTileGroup),
		_NS_TABMNG("Main Data")
		);
}

//////////////////////////////////////////////////////////////////////////////
//					CXEParametersTileGroup
//////////////////////////////////////////////////////////////////////////////
//-----------------------------------------------------------------------------
IMPLEMENT_DYNCREATE(CXEParametersTileGroup, CTileGroup)

//-----------------------------------------------------------------------------
void CXEParametersTileGroup::Customize()
{
	SetLayoutType(CLayoutContainer::VBOX);
	SetLayoutAlign(CLayoutContainer::STRETCH);

	AddTile
		(
		RUNTIME_CLASS(CXEParametersTileDialog),
		IDD_TD_XE_PARAMETERS,
		_T(""),//titolo vuoto e da non mostrare
		TILE_STANDARD
		);
}

//////////////////////////////////////////////////////////////////////////////
//					CXEParametersTileDialog
//////////////////////////////////////////////////////////////////////////////
//-----------------------------------------------------------------------------
IMPLEMENT_DYNCREATE(CXEParametersTileDialog, CTileDialog)

//-----------------------------------------------------------------------------
CXEParametersTileDialog::CXEParametersTileDialog()
	:
	CTileDialog(_T("General"), IDD_TD_XE_PARAMETERS, NULL)
{
	SetHasTitle(FALSE);//non mostra il titolo
}

//-----------------------------------------------------------------------------
void CXEParametersTileDialog::BuildDataControlLinks()
{
	AddLinkPropertyGrid
		(
			IDC_XE_PARAMETERS_PROPERTY_GRID,
			_T("XEParametersPropertyGrid"),
			RUNTIME_CLASS(CXEParametersPropertyGrid)
		);
}

//////////////////////////////////////////////////////////////////////////////
//				CXEParametersPropertyGrid
//////////////////////////////////////////////////////////////////////////////
//-----------------------------------------------------------------------------
IMPLEMENT_DYNCREATE(CXEParametersPropertyGrid, CTBPropertyGrid)

//-----------------------------------------------------------------------------
CXEParametersPropertyGrid::CXEParametersPropertyGrid()
{
}

//-----------------------------------------------------------------------------
void CXEParametersPropertyGrid::OnCustomize()
{
	TXEParameters* pRec = GetXEParameters();
	if (!pRec)
		return;

	// Gruppo Site Parameters
	CTBProperty* pGroupProperty = AddProperty(_T("SiteParams"),_TB("Site Parameters"),_TB("Site Parameters"));

	CTBProperty* pFieldProp = AddSubItem
		(
		pGroupProperty,
		RDCPROP(pRec, f_DomainName, _TB("Domain name"), _TB("The name of domain")),
		IDC_XE_PARAMS_DOMAIN_NAME,
		0, 
		RUNTIME_CLASS(CStrEdit)
		);
	
	pFieldProp = AddSubItem
		(
		pGroupProperty,
		RDCPROP(pRec, f_SiteName, _TB("Site name"), _TB("The name of site")),
		IDC_XE_PARAMS_SITE_NAME,
		0,
		RUNTIME_CLASS(CStrEdit)
		);
	
	pFieldProp = AddSubItem
		(
		pGroupProperty,
		RDCPROP(pRec, f_SiteCode, _TB("Site code"), _TB("The code of site")),
		IDC_XE_PARAMS_SITE_CODE,
		0,
		RUNTIME_CLASS(CStrEdit)
		);

	pFieldProp = AddSubItem
		(
		pGroupProperty,
		RDCPROP(pRec, f_ExportPath, _TB("Export path"), _TB("The path where save exported files")),
		IDC_XE_PARAMS_EXPORT_PATH,
		0,
		RUNTIME_CLASS(CBrowsePathEdit)
		);

	CBrowsePathEdit* pBrowsePathEdit = (CBrowsePathEdit*)pFieldProp->GetControl();
	pBrowsePathEdit->SetCtrlStyle(pBrowsePathEdit->GetCtrlStyle() | PATH_STYLE_AS_PATH);

	pFieldProp = AddSubItem
		(
		pGroupProperty,
		RDCPROP(pRec, f_ImportPath, _TB("Import path"), _TB("The path where save imported files")),
		IDC_XE_PARAMS_IMPORT_PATH,
		0,
		RUNTIME_CLASS(CBrowsePathEdit)
		);
	pBrowsePathEdit = (CBrowsePathEdit*)pFieldProp->GetControl();
	pBrowsePathEdit->SetCtrlStyle(pBrowsePathEdit->GetCtrlStyle() | PATH_STYLE_AS_PATH);

	pFieldProp = AddSubItem
		(
		pGroupProperty,
		RDCPROP(pRec, f_EncodTypeUTF8, _TB("XML file encoding"), _TB("Yes: UTF8 - No: UTF16")),
		IDC_XE_PARAMS_ENCODING_TYPE,
		CBS_DROPDOWNLIST, 
		RUNTIME_CLASS(CBoolCombo)
		);

	// Gruppo Import/Export Parameters
	pGroupProperty = AddProperty(_T("ImportExportParams"), _TB("Import/Export Parameters"), _TB("Import/Export Parameters"));

	pFieldProp = AddSubItem
		(
		pGroupProperty,
		RDCPROP(pRec, f_MaxDoc, _TB("Max. nr. of documents contained in export file"), _TB("Default value: 10")),
		IDC_XE_PARAMS_MAX_DOC,
		0,
		RUNTIME_CLASS(CIntEdit)
		);
	pFieldProp->GetControl()->SetRange(HEADER_MIN_DOCUMENT_NUM, HEADER_MAX_DOCUMENT_NUM);

	pFieldProp = AddSubItem
		(
		pGroupProperty,
		RDCPROP(pRec, f_MaxKByte, _TB("Max. size for export file (in KB)"), _TB("Default value: 100")),
		IDC_XE_PARAMS_MAX_KBYTE,
		0,
		RUNTIME_CLASS(CIntEdit)
		);
	pFieldProp->GetControl()->SetRange(HEADER_MIN_DOC_DIMENSION, HEADER_MAX_DOC_DIMENSION);

	pFieldProp = AddSubItem
		(
		pGroupProperty,
		RDCPROP(pRec, f_EnvPaddingNum, _TB("Nr. of digits to use for the envelope file indexing"), _TB("Default value: 4")),
		IDC_XE_PARAMS_ENV_PADDING_NR,
		0,
		RUNTIME_CLASS(CIntEdit)
		);
	pFieldProp->GetControl()->SetRange(HEADER_MIN_PADDING_NUM, HEADER_MAX_PADDING_NUM);

	pFieldProp = AddSubItem
		(
		pGroupProperty,
		RDCPROP(pRec, f_UseEnvClassExt, _TB("Enable subgroups in the export group"), _TB("")),
		IDC_XE_PARAMS_USE_ENV_CLASS_EXT,
		CBS_DROPDOWNLIST,
		RUNTIME_CLASS(CBoolCombo)
		);

	pFieldProp = AddSubItem
		(
		pGroupProperty,
		RDCPROP(pRec, f_UseAttribute, _TB("Use attributes for exported values"), _TB("")),
		IDC_XE_PARAMS_USE_ATTRIBUTE,
		CBS_DROPDOWNLIST,
		RUNTIME_CLASS(CBoolCombo)
		);

	pFieldProp = AddSubItem
		(
		pGroupProperty,
		RDCPROP(pRec, f_UseEnumAsNum, _TB("Export enumerated data as integer"), _TB("")),
		IDC_XE_PARAMS_USE_ENUM_AS_NUM,
		CBS_DROPDOWNLIST,
		RUNTIME_CLASS(CBoolCombo)
		);
}

#undef RDCPROP
