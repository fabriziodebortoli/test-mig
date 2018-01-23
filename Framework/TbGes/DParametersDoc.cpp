// ConditionalReadOnlyFormDoc.cpp : implementation file
//

#include "stdafx.h"

#include <TbGes\\BROWSER.H>

#include "DParametersDoc.h"
#include "UIParametersDoc.hjson"

/////////////////////////////////////////////////////////////////////////////
//				class DParametersDoc Implementation
/////////////////////////////////////////////////////////////////////////////
IMPLEMENT_DYNAMIC(DParametersDoc, CAbstractFormDoc)

//-----------------------------------------------------------------------------
BEGIN_MESSAGE_MAP(DParametersDoc, CAbstractFormDoc)
	ON_COMMAND				(ID_EXTDOC_EDIT, OnEditRecord)
	ON_COMMAND				(ID_EXTDOC_SAVE, OnSaveRecord)
	ON_UPDATE_COMMAND_UI	(ID_EXTDOC_EDIT, OnUpdateEditRecord)
END_MESSAGE_MAP()

//-----------------------------------------------------------------------------
DParametersDoc::DParametersDoc()
{
}

//-----------------------------------------------------------------------------
DParametersDoc::~DParametersDoc()
{
}

//-----------------------------------------------------------------------------------------
void DParametersDoc::OnUpdateEditRecord(CCmdUI* pCmdUI)
{
	if (m_bOnlyOneRecord)
		pCmdUI->Enable(DispatchCanDoEditRecord() || DispatchCanDoNewRecord());
	else
		CAbstractFormDoc::OnUpdateEditRecord(pCmdUI);
}

//---------------------------------------------------------------------------------
void DParametersDoc::OnEditRecord()
{
	if (!m_bOnlyOneRecord)
		CAbstractFormDoc::OnEditRecord();
	else
		if
			(
				m_pBrowser &&
				(
					!m_pBrowser->NoCurrent() ||
					m_pBrowser->Query() && 
					!m_pBrowser->IsEmpty()
				)
			)	//exists
			CAbstractFormDoc::OnEditRecord();
		else
			CAbstractFormDoc::OnNewRecord();
}


//---------------------------------------------------------------------------------
void DParametersDoc::OnSaveRecord()
{
	SetModifiedFlag(); // In questo modo se non si fanno modifiche e vanno bene i default proposti viene aggiunto il record
	__super::OnSaveRecord();
}

//-----------------------------------------------------------------------------
BOOL DParametersDoc::CanSaveParameters()
{
	// if the login context allows changing date with open docs (i.e.: during test configuration), also is allowed to save parameters
	return AfxGetBaseApp()->GetNrOpenDocuments() == 1  || AfxGetLoginContext()->GetAllowSetOpDateWithOpenDocs();
}

/////////////////////////////////////////////////////////////////////////////
//				class CDParametersDoc Implementation
/////////////////////////////////////////////////////////////////////////////
IMPLEMENT_DYNCREATE(CDParametersDoc, CClientDoc)

//-----------------------------------------------------------------------------
CDParametersDoc::CDParametersDoc() 
	: 
	CClientDoc()
{
}

//-----------------------------------------------------------------------------
CDParametersDoc::~CDParametersDoc()
{
}

//-----------------------------------------------------------------------------
BOOL CDParametersDoc::OnOkTransaction ()
{
	if (!GetServerDoc()->CanSaveParameters())
	{
		GetServerDoc()->Message(_TB("Before saving you have to close all other opened documents"));
		return FALSE; 
	}

	return TRUE;
}

//-----------------------------------------------------------------------------
BOOL CDParametersDoc::OnPrepareAuxData ()
{
	if ((GetServerDoc()->GetFormMode() == CAbstractFormDoc::EDIT || GetServerDoc()->GetFormMode() == CAbstractFormDoc::NEW)
		&& !GetServerDoc()->CanSaveParameters())
	{
		GetServerDoc()->Message(_TB("WARNING! This document may change sensible data, you won't be able to save until you close all other opened documents"), 0, 0, NULL, CMessages::MSG_WARNING);
	}

	return TRUE;
}


/////////////////////////////////////////////////////////////////////////////
//					class CSingleRecordParametersFrame Implementation
/////////////////////////////////////////////////////////////////////////////
IMPLEMENT_DYNCREATE(CSingleRecordParametersFrame, CMasterFrame)

//------------------------------------------------------------------------------
CSingleRecordParametersFrame::CSingleRecordParametersFrame()
	:
	CMasterFrame()
{
}

//--------------------------------------------------------------------------------
BOOL CSingleRecordParametersFrame::OnCustomizeTabbedToolBar(CTBTabbedToolbar* pTabbedBar)
{
	__super::OnCustomizeTabbedToolBar(pTabbedBar);

	CTBToolBar* pToolbar = pTabbedBar->FindToolBar(szToolbarNameMain);

	if (pToolbar)
	{
		pToolbar->RemoveButtonForID(ID_EXTDOC_FIND);
		pToolbar->RemoveButtonForID(ID_EXTDOC_RADAR);
		pToolbar->RemoveButtonForID(ID_EXTDOC_REPORT);
		pToolbar->RemoveButtonForID(ID_EXTDOC_EXEC_QUERY);
		pToolbar->RemoveButtonForID(ID_EXTDOC_FIRST);
		pToolbar->RemoveButtonForID(ID_EXTDOC_PREV);
		pToolbar->RemoveButtonForID(ID_EXTDOC_NEXT);
		pToolbar->RemoveButtonForID(ID_EXTDOC_LAST);
		pToolbar->RemoveButtonForID(ID_EXTDOC_QUERY);
		pToolbar->RemoveButtonForID(ID_EXTDOC_DELETE);
		pToolbar->RemoveButtonForID(ID_EXTDOC_NEW);
	}
	return TRUE;
}

/////////////////////////////////////////////////////////////////////////////
//					class CMultiRecordParametersFrame Implementation
/////////////////////////////////////////////////////////////////////////////
IMPLEMENT_DYNCREATE(CMultiRecordParametersFrame, CMasterFrame)

//------------------------------------------------------------------------------
CMultiRecordParametersFrame::CMultiRecordParametersFrame()
	:
	CMasterFrame()
{
}

//--------------------------------------------------------------------------------
BOOL CMultiRecordParametersFrame::OnCustomizeTabbedToolBar(CTBTabbedToolbar* pTabbedBar)
{
	__super::OnCustomizeTabbedToolBar(pTabbedBar);

	CTBToolBar* pToolbar = pTabbedBar->FindToolBar(_T("Main"));

	if (pToolbar)
		pToolbar->RemoveButton(pToolbar->FindButton(ID_EXTDOC_REPORT));
	
	return TRUE;
}

/////////////////////////////////////////////////////////////////////////////
//				class CParametersTileDialog Implementation
/////////////////////////////////////////////////////////////////////////////
IMPLEMENT_DYNCREATE(CParametersTileDialog, CTileDialog)

//---------------------------------------------------------------------------------------------------------------------
CParametersTileDialog::CParametersTileDialog()
:
CTileDialog(_NS_TILEDLG(""), 0)
{
	ASSERT_TRACE(FALSE, "Cannot use parameterless constructor with CParametersTileDialog");
}

//---------------------------------------------------------------------------------------------------------------------
CParametersTileDialog::CParametersTileDialog(const CString& sName, int nIDD, CWnd* pParent /*= NULL*/) 
	: 
	CTileDialog(sName, nIDD, pParent)
{
	SetTileStyle(AfxGetTileDialogStyleParameters());
	SetHasTitle(FALSE);
}

//---------------------------------------------------------------------------------------------------------------------
BOOL CParametersTileDialog::Create(UINT nIDC, const CString& strTitle, CWnd* pParentWnd, TileDialogSize tileSize)
{
	SetTitle(strTitle);
	return CBaseTileDialog::Create(nIDC, strTitle, pParentWnd, tileSize);
}

/////////////////////////////////////////////////////////////////////////////
//				class DCompanyUserSettingsDoc Implementation
/////////////////////////////////////////////////////////////////////////////
IMPLEMENT_DYNAMIC(DCompanyUserSettingsDoc, CAbstractFormDoc)

//-----------------------------------------------------------------------------
BEGIN_MESSAGE_MAP(DCompanyUserSettingsDoc, CAbstractFormDoc)
	//I Changed partono da 0x0DAC per escludere i numeri di TB che per convenzione nostra arrivano fino a 2700 (+ un margine)
	// Se CAMBIASSERO gli intervalli di TB BISOGNA cambiare qua altrimenti non vengono intercettati im messaggi
	ON_EN_VALUE_CHANGED_RANGE	(0x0DAC, 0xDFFF,	OnControlChanged)
	ON_UPDATE_COMMAND_UI		(ID_EXTDOC_EDIT,	OnUpdateEditRecord)
END_MESSAGE_MAP()

//-----------------------------------------------------------------------------
DCompanyUserSettingsDoc::DCompanyUserSettingsDoc()
	: m_pPropertyGrid(NULL)
{
	
}

//-----------------------------------------------------------------------------
DCompanyUserSettingsDoc::~DCompanyUserSettingsDoc()
{
}

//-----------------------------------------------------------------------------------------
BOOL DCompanyUserSettingsDoc::OnAttachData()
{
	// non avendo una tabella i dati li carico in entrata
	GetSaveSettings(FALSE);

	SetFormTitle(_TB("Company/User Settings"));

	return TRUE;
}

//-----------------------------------------------------------------------------------------
BOOL DCompanyUserSettingsDoc::OnPrepareAuxData()
{
	return GetSaveSettings(FALSE);
}

//-----------------------------------------------------------------------------------------
BOOL DCompanyUserSettingsDoc::OnOkTransaction()
{
	return AfxMessageBox(_TB("The program parameters have been changed.\n\nYou must select OK before these parameters will take effect.\n\nBe very careful to confirm them to prevent abnormal procedures."),MB_OKCANCEL| MB_DEFBUTTON2) == IDOK;
}

//-----------------------------------------------------------------------------------------
BOOL DCompanyUserSettingsDoc::OnEditTransaction()
{
	return GetSaveSettings(TRUE);
}

//-----------------------------------------------------------------------------------------
void DCompanyUserSettingsDoc::OnGoInBrowseMode()
{
	__super::OnGoInBrowseMode();

	// ricarica i settings. utile nel caso il salvataggio non vada a buon fine (es. l'utente ha dato Cancel alla conferma)
	GetSaveSettings(FALSE);
}

//-----------------------------------------------------------------------------------------
void DCompanyUserSettingsDoc::OnOpenCompleted()
{
	// il messaggio viene fornito a caricamento ultimato in modo che non sia possibile passare al menu e fare, ad esempio, un cambio azienda
	AfxMessageBox(_TB("The program setup parameters should be edited very carefully. Please read the Help before you make any changes."));

	__super::OnOpenCompleted();
}

//-----------------------------------------------------------------------------------------
void DCompanyUserSettingsDoc::OnControlChanged(UINT nIDC)
{
	// Dato che non ci sono né DBTMaster né DBTSlave* attachati al documento,
	// non "sente" le modifiche al documento. Pertanto occorre che ogni
	// changed su qualsiasi control faccia scattare la modifica sul documento
	SetModifiedFlag(TRUE);
}

//--------------------------------------------------------------------------------
void DCompanyUserSettingsDoc::OnUpdateEditRecord(CCmdUI* pCmdUI)
{
	pCmdUI->Enable(GetFormMode() == CBaseDocument::BROWSE);
}

//////////////////////////////////////////////////////////////////////////////
//						 CCompanyUserSettingsView
//////////////////////////////////////////////////////////////////////////////
IMPLEMENT_DYNCREATE(CCompanyUserSettingsView, CMasterFormView)

//-----------------------------------------------------------------------------
CCompanyUserSettingsView::CCompanyUserSettingsView()
	:
	CMasterFormView(_NS_VIEW("CompanyUserSettings"), IDD_COMPANY_USER_SETTINGS)
{
	 
}

//-----------------------------------------------------------------------------
void CCompanyUserSettingsView::BuildDataControlLinks()
{   
	AddTileGroup
		(
			IDC_TG_COMPANY_USER_SETTINGS,
			RUNTIME_CLASS(CCompanyUserSettingsTileGrp),
			_NS_TILEGRP("MainData")
		);
}

//////////////////////////////////////////////////////////////////////////////
//					CCompanyUserSettingsTileGrp
//////////////////////////////////////////////////////////////////////////////
IMPLEMENT_DYNCREATE(CCompanyUserSettingsTileGrp, CTileGroup)

//---------------------------------------------------------------------------------------
void CCompanyUserSettingsTileGrp::Customize()
{
	SetLayoutType(CLayoutContainer::VBOX);
	SetLayoutAlign(CLayoutContainer::STRETCH);

	// GetPropertyGridIDD avendo priorità viene esaminato per primo
	// GetPropertyGridRuntimeClass è mantenuto per retrocompatibilità
	if (GetDocument()->GetPropertyGridIDD())
		AddJsonTile(GetDocument()->GetPropertyGridIDD());
	else if (GetDocument()->GetPropertyGridRuntimeClass())
		AddTile
			(
				RUNTIME_CLASS(CCompanyUserSettingsTileDlg),
				IDD_TD_COMPANY_USER_SETTINGS,
				_T(""),//titolo vuoto e da non mostrare
				TILE_STANDARD
			);
	else
		// ASSERT(FALSE) se non ha ridefinito i metodi per ritornare 
		// o l'IDD della tile o la runtime-class del propertygrid 
		ASSERT(FALSE);
}

/////////////////////////////////////////////////////////////////////////////
//						CCompanyUserSettingsTileDlg
/////////////////////////////////////////////////////////////////////////////
IMPLEMENT_DYNCREATE(CCompanyUserSettingsTileDlg, CParametersTileDialog)

//-----------------------------------------------------------------------------
CCompanyUserSettingsTileDlg::CCompanyUserSettingsTileDlg()
:
CParametersTileDialog(_T("General"), IDD_TD_COMPANY_USER_SETTINGS)
{
	SetHasTitle(FALSE);//non mostra il titolo
}

//-----------------------------------------------------------------------------
void CCompanyUserSettingsTileDlg::BuildDataControlLinks()
{
	GetDocument()->m_pPropertyGrid = AddLinkPropertyGrid
		(
			IDC_PG_COMPANY_USER_SETTINGS,
			_T("CCompanyUserSettingsTileDlg"),
			GetDocument()->GetPropertyGridRuntimeClass()
		);

}



