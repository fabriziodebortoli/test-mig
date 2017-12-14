#include "stdafx.h"

#include <TbFrameworkImages\CommonImages.h>
#include <TbGes\ExtDocFrame.h>
#include <TbFrameworkImages\GeneralFunctions.h>
#include <ExtensionsImages\CommonImages.h>
#include <TbGenLibManaged\HelpManager.h>

#include "CDDMS.h"
#include "CommonObjects.h"
#include "CommonControls.h"
#include "TBDMSEnums.h"
#include "TbRepositoryManager.h"
#include "SOSObjects.h"

#include "UIAttachment.h"
#include "UIAttachment.hjson" 

#ifdef _DEBUG
#undef THIS_FILE
static char THIS_FILE[] = __FILE__;
#endif

using namespace System; 
using namespace Microarea::TBPicComponents;
using namespace Microarea::EasyAttachment::BusinessLogic;
using namespace Microarea::EasyAttachment::Components;
using namespace Microarea::TaskBuilderNet::Core::WebServicesWrapper;

struct _IMAGELIST {};

#define DMS_COMMAND_PREFIX _T("ToolbarButton.Extensions.TbDMS.TbDMS.DAttachment.")

#define DMS_HELP_ATTACHMENTPANE	_T("RefGuide.TBF.EasyAttachment.TbDMS.AttachmentPane")

//-----------------------------------------------------------------------------
CString DMSAttachmentNamespace(const CString& aName)
{
	return CString(DMS_COMMAND_PREFIX) + aName;
}

//////////////////////////////////////////////////////////////////////////////
//						    CAttachmentsListBox
//////////////////////////////////////////////////////////////////////////////
//=============================================================================
IMPLEMENT_DYNCREATE(CAttachmentsListBox, CLongListBox)

//-----------------------------------------------------------------------------
BEGIN_MESSAGE_MAP(CAttachmentsListBox, CLongListBox)
	ON_WM_CONTEXTMENU()
	ON_MESSAGE(UM_RECALC_CTRL_SIZE, OnRecalcCtrlSize)
END_MESSAGE_MAP()

//-----------------------------------------------------------------------------
CAttachmentsListBox::CAttachmentsListBox()
	:
	CLongListBox(),
	m_pImageList(NULL),
	m_pParsedCtrlTarget(NULL),
	m_pAttachments(NULL)
{	
}

//-----------------------------------------------------------------------------
CAttachmentsListBox::~CAttachmentsListBox()
{
	if (m_pImageList)
	{
		m_pImageList->DeleteImageList();
		SAFE_DELETE(m_pImageList);
	}
}

//------------------------------------------------------------------------------
LRESULT CAttachmentsListBox::OnRecalcCtrlSize(WPARAM, LPARAM)
{
	DoRecalcCtrlSize();
	return 0L;
}

//-----------------------------------------------------------------------------
void CAttachmentsListBox::SetAttachmentsArray(DMSAttachmentsList* pAttachments)
{
	m_pAttachments = pAttachments;
}

//-----------------------------------------------------------------------------
void CAttachmentsListBox::OnContextMenu(CWnd* pWnd, CPoint ptMousePos)
{
	// reimplemento i metodo e pulisco il menu di contesto ereditato dalla classe base
}

//-----------------------------------------------------------------------------
void CAttachmentsListBox::OnFillListBox()
{
	if (!m_pAttachments || m_pAttachments->GetSize() < 1)
		return;

	DMSAttachmentInfo* pAttInfo = NULL; 
	int nElem;
	for (int i = 0; i <= m_pAttachments->GetUpperBound(); i++)
	{
		pAttInfo = m_pAttachments->GetAt(i);
		nElem = AddAssociation(pAttInfo->m_Name.Str(), i); // pAttInfo->m_attachmentID);
		SetItemImage(nElem, GetImageIdx(pAttInfo->m_ExtensionType.Str()));
	}
}

//-----------------------------------------------------------------------------
BOOL CAttachmentsListBox::OnSubFolderFound()
{
	return (AfxMessageBox(_TB("Attach also files in subfolder?"), MB_YESNO | MB_ICONQUESTION) == IDYES);
}

//-----------------------------------------------------------------------------
void CAttachmentsListBox::OnDropFiles(CStringArray* pDroppedFiles)
{
	if (!pDroppedFiles)
		return;
	
	((CAttachmentHeadTileDlg*)GetParent())->AttachFiles(pDroppedFiles);	
}

//-----------------------------------------------------------------------------
BOOL CAttachmentsListBox::OnInitCtrl()
{
	CLongListBox::OnInitCtrl();

	ResizableCtrl::InitSizeInfo(this);

	// carica le bitmap delle estensioni nella imagelist che viene poi agganciata alla listbox
	BuildImageList();
	SetImageList(m_pImageList->GetSafeHandle());

	// gestione drag-drop
	m_pParsedCtrlTarget = new CParsedCtrlDropFilesTarget();
	EnableDrop(TRUE, m_pParsedCtrlTarget);
	m_pParsedCtrlTarget->SetTemporaryPath(AfxGetTbRepositoryManager()->GetEasyAttachmentTempPath());

	return TRUE;
}

// riempimento imagelist con le immagini delle estensioni dei file
//-----------------------------------------------------------------------------
void CAttachmentsListBox::BuildImageList()
{
	CBitmap bitmap;

	m_pImageList = new CImageList();
	// 20 & 20 is the size, 48 is the elements number
	m_pImageList->Create(20, 20, ILC_MASK | ILC_COLOR32, 48, 48);

	HICON hbitmap = TBLoadPng(ExtensionsGlyph(szGlyphExtDefault)); //0
	m_pImageList->Add(hbitmap);
	::DestroyIcon(hbitmap);
	hbitmap = TBLoadPng(ExtensionsGlyph(szGlyphExtAVI)); //1
	m_pImageList->Add(hbitmap);
	::DestroyIcon(hbitmap);
	hbitmap =  TBLoadPng(ExtensionsGlyph(szGlyphExtBMP)); //2
	m_pImageList->Add(hbitmap);
	::DestroyIcon(hbitmap);
	hbitmap = TBLoadPng(ExtensionsGlyph(szGlyphExtDOC)); //3
	m_pImageList->Add(hbitmap);
	::DestroyIcon(hbitmap);
	hbitmap = TBLoadPng(ExtensionsGlyph(szGlyphExtGIF)); //4
	m_pImageList->Add(hbitmap);
	::DestroyIcon(hbitmap);
	hbitmap = TBLoadPng(ExtensionsGlyph(szGlyphExtGZIP)); //5
	m_pImageList->Add(hbitmap);
	::DestroyIcon(hbitmap);
	hbitmap = TBLoadPng(ExtensionsGlyph(szGlyphExtHTML)); //6
	m_pImageList->Add(hbitmap);
	::DestroyIcon(hbitmap);
	hbitmap = TBLoadPng(ExtensionsGlyph(szGlyphExtJPG)); //7
	m_pImageList->Add(hbitmap);
	::DestroyIcon(hbitmap);
	hbitmap = TBLoadPng(ExtensionsGlyph(szGlyphExtMAIL)); //8
	m_pImageList->Add(hbitmap);
	::DestroyIcon(hbitmap);
	hbitmap = TBLoadPng(ExtensionsGlyph(szGlyphExtMP3)); //9
	m_pImageList->Add(hbitmap);
	::DestroyIcon(hbitmap);
	hbitmap = TBLoadPng(ExtensionsGlyph(szGlyphExtMPEG)); //10
	m_pImageList->Add(hbitmap);
	::DestroyIcon(hbitmap);
	hbitmap = TBLoadPng(ExtensionsGlyph(szGlyphExtPDF)); //11
	m_pImageList->Add(hbitmap);
	::DestroyIcon(hbitmap);
	hbitmap = TBLoadPng(ExtensionsGlyph(szGlyphExtPNG)); //12
	m_pImageList->Add(hbitmap);
	::DestroyIcon(hbitmap);
	hbitmap = TBLoadPng(ExtensionsGlyph(szGlyphExtPPT)); //13
	m_pImageList->Add(hbitmap);
	::DestroyIcon(hbitmap);
	hbitmap = TBLoadPng(ExtensionsGlyph(szGlyphExtRAR)); //14
	m_pImageList->Add(hbitmap);
	::DestroyIcon(hbitmap);
	hbitmap = TBLoadPng(ExtensionsGlyph(szGlyphExtRTF)); //15
	m_pImageList->Add(hbitmap);
	::DestroyIcon(hbitmap);
	hbitmap = TBLoadPng(ExtensionsGlyph(szGlyphExtTIFF)); //16
	m_pImageList->Add(hbitmap);
	::DestroyIcon(hbitmap);
	hbitmap = TBLoadPng(ExtensionsGlyph(szGlyphExtTXT)); //17
	m_pImageList->Add(hbitmap);
	::DestroyIcon(hbitmap);
	hbitmap = TBLoadPng(ExtensionsGlyph(szGlyphExtWAV)); //18
	m_pImageList->Add(hbitmap);
	::DestroyIcon(hbitmap);
	hbitmap = TBLoadPng(ExtensionsGlyph(szGlyphExtWMV)); //19
	m_pImageList->Add(hbitmap);
	::DestroyIcon(hbitmap);
	hbitmap = TBLoadPng(ExtensionsGlyph(szGlyphExtXLS)); //20
	m_pImageList->Add(hbitmap);
	::DestroyIcon(hbitmap);
	hbitmap = TBLoadPng(ExtensionsGlyph(szGlyphExtXML)); //21
	m_pImageList->Add(hbitmap);
	::DestroyIcon(hbitmap);
	hbitmap = TBLoadPng(ExtensionsGlyph(szGlyphExtZIP)); //22
	m_pImageList->Add(hbitmap);
	::DestroyIcon(hbitmap);
	hbitmap = TBLoadPng(ExtensionsGlyph(szGlyphExtPapery)); //23
	m_pImageList->Add(hbitmap);
	::DestroyIcon(hbitmap);
}

//-----------------------------------------------------------------------------
int CAttachmentsListBox::GetImageIdx(DataStr sExtensionType)
{
	// i papery non hanno il . nell'estensione
	CString extension = (sExtensionType.Str().CompareNoCase(szPapery) != 0) ? sExtensionType.Str().Mid(1) : sExtensionType.Str();

	if (extension.CompareNoCase(szAvi) == 0) return 1;
	if (extension.CompareNoCase(szBmp) == 0) return 2;
	if (extension.CompareNoCase(szDoc) == 0 || extension.CompareNoCase(szDocx) == 0) return 3;
	if (extension.CompareNoCase(szGif) == 0) return 4;
	if (extension.CompareNoCase(szGzip) == 0) return 5;
	if (extension.CompareNoCase(szHtm) == 0 || extension.CompareNoCase(szHtml) == 0) return 6;
	if (extension.CompareNoCase(szJpg) == 0 || extension.CompareNoCase(szJpeg) == 0) return 7;
	if (extension.CompareNoCase(szMsg) == 0) return 8;
	if (extension.CompareNoCase(szMp3) == 0) return 9;
	if (extension.CompareNoCase(szMpeg) == 0) return 10;
	if (extension.CompareNoCase(szPdf) == 0) return 11;
	if (extension.CompareNoCase(szPng) == 0) return 12;
	if (extension.CompareNoCase(szPpt) == 0 || extension.CompareNoCase(szPptx) == 0) return 13;
	if (extension.CompareNoCase(szRar) == 0) return 14;
	if (extension.CompareNoCase(szRtf) == 0) return 15;
	if (extension.CompareNoCase(szTif) == 0 || extension.CompareNoCase(szTiff) == 0) return 16;
	if (extension.CompareNoCase(szTxt) == 0 || extension.CompareNoCase(szConfig) == 0) return 17;
	if (extension.CompareNoCase(szWav) == 0) return 18;
	if (extension.CompareNoCase(szWmv) == 0) return 19;
	if (extension.CompareNoCase(szXls) == 0 || extension.CompareNoCase(szXlsx) == 0) return 20;
	if (extension.CompareNoCase(szXml) == 0) return 21;
	if (extension.CompareNoCase(szZip) == 0 || extension.CompareNoCase(szZip7z) == 0) return 22;
	if (extension.CompareNoCase(szPapery) == 0) return 23;

	return 0;
}


//////////////////////////////////////////////////////////////////////////
//						CAttachmentPane									//
//////////////////////////////////////////////////////////////////////////
//
//--------------------------------------------------------------------
IMPLEMENT_DYNCREATE(CAttachmentPane, CTaskBuilderDockPane)

//-----------------------------------------------------------------------------
BEGIN_MESSAGE_MAP(CAttachmentPane, CTaskBuilderDockPane)
	//{{AFX_MSG_MAP(CAttachmentPane)
	ON_WM_HELPINFO()
	//}}AFX_MSG_MAP
END_MESSAGE_MAP()

//-----------------------------------------------------------------------------
CAttachmentPane::CAttachmentPane()
	:
	CTaskBuilderDockPane(RUNTIME_CLASS(CAttachmentPaneView)),
	m_pDMSClientDoc(NULL)
{
}

//-----------------------------------------------------------------------------
CAttachmentPane::CAttachmentPane(CDDMS* pClientDoc)
	:
	CTaskBuilderDockPane(RUNTIME_CLASS(CAttachmentPaneView)),
	m_pDMSClientDoc(pClientDoc)
{
}

//-----------------------------------------------------------------------------
CAttachmentPaneView* CAttachmentPane::GetAttachmentPaneView() const
{
	return (CAttachmentPaneView*) GetFormWnd(RUNTIME_CLASS(CAttachmentPaneView));
}

//-----------------------------------------------------------------------------
void CAttachmentPane::SetDMSClientDoc(CDDMS* pCDDMS)
{
	m_pDMSClientDoc = pCDDMS;
}

//-----------------------------------------------------------------------------
void CAttachmentPane::OnUpdateControls()
{
	GetAttachmentPaneView()->OnUpdateControls();
}

//-----------------------------------------------------------------------------
void CAttachmentPane::OnNewAttachCompleted()
{
	GetAttachmentPaneView()->OnNewAttachCompleted();
}

//-----------------------------------------------------------------------------
void CAttachmentPane::OnAfterLoadAttachments()
{
	GetAttachmentPaneView()->OnAfterLoadAttachments();
}

//-----------------------------------------------------------------------------
void CAttachmentPane::OnSlide(BOOL bSlideOut)
{
	CAbstractFormDoc* pDoc = (CAbstractFormDoc*) GetDocument();

	BOOL bCanSlideOut = !pDoc ? TRUE : (pDoc->ValidCurrentRecord() && pDoc->GetFormMode() != CAbstractFormDoc::NEW &&
		pDoc->GetFormMode() != CAbstractFormDoc::FIND) || pDoc->m_bBatch;

	__super::OnSlide((bSlideOut && bCanSlideOut));
}

//-----------------------------------------------------------------------------
BOOL CAttachmentPane::OnHelpInfo(HELPINFO* pHelpInfo)
{
	return ShowHelp(DMS_HELP_ATTACHMENTPANE);
}

//////////////////////////////////////////////////////////////////////////////
//						    CAttachmentPaneView
//////////////////////////////////////////////////////////////////////////////
//
//=============================================================================
IMPLEMENT_DYNCREATE(CAttachmentPaneView, CParsedDialogWithTiles)

//-----------------------------------------------------------------------------
BEGIN_MESSAGE_MAP(CAttachmentPaneView, CParsedDialogWithTiles)
	//{{AFX_MSG_MAP(CAttachmentPaneView)
	ON_EN_VALUE_CHANGED(IDC_ATTACHMENTS_LISTBOX,		OnAttachmentChanged)
	ON_EN_VALUE_CHANGED(IDC_ATT_BARCODE,				OnBarcodeChanged)

	ON_COMMAND(ID_DMS_NEW_ATTACHMENT,					OnNewAttachment)
	ON_COMMAND(ID_DMS_SAVE_ATTACHMENT,					OnSaveAttachment)
	ON_COMMAND(ID_DMS_EDIT_ATTACHMENT,					OnEditAttachment)
	ON_COMMAND(ID_DMS_UNDOCHANGES_ATTACHMENT,			OnUndoChangesAttachment)	
	ON_COMMAND(ID_DMS_DELETE_ATTACHMENT,				OnDeleteAttachment)		
	ON_COMMAND(ID_DMS_VIEW_ATTACHMENT,					OnViewAttachment)
	ON_COMMAND(ID_DMS_COPY_ATTACHMENT,					OnCopyAttachment)
	ON_COMMAND(ID_DMS_SEND_ATTACHMENT,					OnSendAttachment)
	ON_COMMAND(ID_DMS_RELOAD_ATTACHMENTS,				OnReloadAttachment)
	ON_COMMAND(ID_DMS_CHECKOUT_ATTACHMENT,				OnCheckOutAttachment)
	ON_COMMAND(ID_DMS_CHECKIN_ATTACHMENT,				OnCheckInAttachment)
	
	ON_UPDATE_COMMAND_UI(ID_DMS_NEW_ATTACHMENT,			OnUpdateNewAttachment)
	ON_UPDATE_COMMAND_UI(ID_DMS_SAVE_ATTACHMENT,		OnUpdateSaveAttachment)
	ON_UPDATE_COMMAND_UI(ID_DMS_EDIT_ATTACHMENT,		OnUpdateEditAttachment)
	ON_UPDATE_COMMAND_UI(ID_DMS_UNDOCHANGES_ATTACHMENT, OnUpdateUndoChangesAttachment)
	ON_UPDATE_COMMAND_UI(ID_DMS_DELETE_ATTACHMENT,		OnUpdateDeleteAttachment)

	ON_UPDATE_COMMAND_UI(ID_DMS_VIEW_ATTACHMENT,		OnUpdateViewAttachment)
	ON_UPDATE_COMMAND_UI(ID_DMS_COPY_ATTACHMENT,		OnUpdateCopyAttachment)
	ON_UPDATE_COMMAND_UI(ID_DMS_SEND_ATTACHMENT,		OnUpdateSendAttachment)
	ON_UPDATE_COMMAND_UI(ID_DMS_CHECKOUT_ATTACHMENT,	OnUpdateCheckOutAttachment)
	ON_UPDATE_COMMAND_UI(ID_DMS_CHECKIN_ATTACHMENT,		OnUpdateCheckInAttachment)
	ON_UPDATE_COMMAND_UI(ID_DMS_RELOAD_ATTACHMENTS,		OnUpdateReloadAttachments)

	//}}AFX_MSG_MAP
END_MESSAGE_MAP()

//-----------------------------------------------------------------------------
CAttachmentPaneView::CAttachmentPaneView()
	:	
	CParsedDialogWithTiles(IDD_ATTACHMENT, NULL, _T("AttachmentPane"))
{
	m_bInOpenDlgCounter = FALSE;
}

//-----------------------------------------------------------------------------
BOOL CAttachmentPaneView::OnInitDialog()
{
	__super::OnInitDialog();	
	SetToolbarStyle(ToolbarStyle::TOP, 25, FALSE, TRUE);
	m_pToolBar->SetDropdown(ID_DMS_NEW_ATTACHMENT);
	m_pToolBar->SetDropdown(ID_DMS_CHECKIN_ATTACHMENT);
	m_pToolBar->EnableAlwaysDropDown(ID_DMS_CHECKIN_ATTACHMENT, FALSE);

	AddTileGroup(IDC_ATTACHMENT_HEAD, RUNTIME_CLASS(CAttachmentHeadTileGrp), _NS_TILEGRP("AttachmentHead"));

	return TRUE;
}

//-----------------------------------------------------------------------------
void CAttachmentPaneView::OnCustomizeToolbar()
{
	if (m_pToolBar == NULL) 
		return;

	m_pToolBar->AddButton(ID_DMS_NEW_ATTACHMENT, DMSAttachmentNamespace(_T("New")), TBIcon(szIconNew, TOOLBAR), _TB("New"), _TB("Create\r\nCreate a new attachment"));
	m_pToolBar->AddButton(ID_DMS_EDIT_ATTACHMENT, DMSAttachmentNamespace(_T("Edit")), TBIcon(szIconEdit, TOOLBAR), _TB("Edit"), _TB("Edit\r\nModify the current attachment data"));
	m_pToolBar->AddButton(ID_DMS_DELETE_ATTACHMENT, DMSAttachmentNamespace(_T("Delete")), TBIcon(szIconDelete, TOOLBAR), _TB("Delete"), _TB("Delete\r\nDelete the selected attachments"));
	m_pToolBar->AddButton(ID_DMS_SAVE_ATTACHMENT, DMSAttachmentNamespace(_T("Save")), TBIcon(szIconSave, TOOLBAR), _TB("Save"), _TB("Save\nSave"));	
	m_pToolBar->AddButton(ID_DMS_UNDOCHANGES_ATTACHMENT, DMSAttachmentNamespace(_T("Undo")), TBIcon(szIconUndo, TOOLBAR), _TB("Undo Changes"), _TB("Undo Changes\nUndo Changes"));
	m_pToolBar->AddButton(ID_DMS_VIEW_ATTACHMENT, DMSAttachmentNamespace(_T("View")), TBIcon(szIconPicture, TOOLBAR), _TB("View"), _TB("View\r\nView the selected attachments"));
	m_pToolBar->AddButton(ID_DMS_COPY_ATTACHMENT, DMSAttachmentNamespace(_T("Copy")), TBIcon(szIconCopy, TOOLBAR), _TB("Copy in"), _TB("Copy in\r\nCopy file in specified folder"));
	m_pToolBar->AddButton(ID_DMS_SEND_ATTACHMENT, DMSAttachmentNamespace(_T("Send")), TBIcon(szIconMail, TOOLBAR), _TB("Send"), _TB("Send\r\nSend the selected attachments using Mail Service"));
	m_pToolBar->AddButton(ID_DMS_CHECKOUT_ATTACHMENT, DMSAttachmentNamespace(_T("CheckOut")), ExtensionsIcon(szIconCheckOut, TOOLBAR), _TB("Check Out"), _TB("Check Out\nCheck Out"));
	m_pToolBar->AddButton(ID_DMS_CHECKIN_ATTACHMENT, DMSAttachmentNamespace(_T("CheckIn")), ExtensionsIcon(szIconCheckIn, TOOLBAR), _TB("Check In"), _TB("Check In\nCheck In"));
	m_pToolBar->AddButton(ID_DMS_RELOAD_ATTACHMENTS, DMSAttachmentNamespace(_T("Reload")), TBIcon(szIconRefresh, TOOLBAR), _TB("Reload"), _TB("Reload\nReload"));
}

//-----------------------------------------------------------------------------
BOOL CAttachmentPaneView::OnPopulatedDropDown(UINT nIdCommand)
{
	CTBToolBarMenu menu;
	menu.CreateMenu();
	if (nIdCommand == ID_DMS_NEW_ATTACHMENT)
	{
		menu.AppendMenu(MF_STRING, ID_DMS_NEW_ATT_FILESYSTEM, (LPTSTR)(LPCTSTR)_TB("from file system"), TBIcon(szIconOpen, CONTROL));
		
		if (AfxGetTbRepositoryManager()->GetDMSSettings()->GetSettings()->l_EnableToAttachFromRepository)
			menu.AppendMenu(MF_STRING, ID_DMS_NEW_ATT_REPOSITORY, (LPTSTR)(LPCTSTR)_TB("from repository"), ExtensionsIcon(szIconRepository, CONTROL));
		
		menu.AppendMenu(MF_STRING, ID_DMS_NEW_ATT_DEVICE, (LPTSTR)(LPCTSTR)_TB("from device"), ExtensionsIcon(szIconDevice, CONTROL));

		if (AfxGetTbRepositoryManager()->GetDMSSettings()->GetSettings()->l_EnableBarcode)
		{
			menu.AppendMenu(MF_STRING, ID_DMS_NEW_ATT_PAPERY, (LPTSTR)(LPCTSTR)_TB("papery"), ExtensionsIcon(szIconPapery, CONTROL));
			menu.AppendMenu(MF_STRING, ID_DMS_RUN_PAPERY_REPORT, (LPTSTR)(LPCTSTR)_TB("paperies from barcode report"), TBIcon(szIconReport, CONTROL));
		}
		m_pToolBar->UpdateDropdownMenu(nIdCommand, &menu);
		return TRUE;
	}
	if (nIdCommand == ID_DMS_CHECKIN_ATTACHMENT)
	{
		menu.AppendMenu(MF_STRING, ID_DMS_UNDOCHECKOUT_ATTACHMENT, (LPTSTR)(LPCTSTR)_TB("Undo"));
		m_pToolBar->UpdateDropdownMenu(nIdCommand, &menu);
		return TRUE;
	}

	return FALSE;
}

//-----------------------------------------------------------------------------
CDDMS* CAttachmentPaneView::GetDMSClientDoc() const
{
	return (CDDMS*)((CAbstractFormDoc*)GetDocument())->GetClientDoc(RUNTIME_CLASS(CDDMS));
}

//-----------------------------------------------------------------------------
void CAttachmentPaneView::EnableControlLinks()
{
	if (m_pTileGroup)
	{
		for (int i = 0; i < this->m_pTileGroup->GetTileDialogs()->GetSize(); i++)
			((CAttachmentBaseTileDlg*)m_pTileGroup->GetTileDialogs()->GetAt(i))->EnableTileDialogControlLinks();
	}
}

//-----------------------------------------------------------------------------
void CAttachmentPaneView::OnNewAttachCompleted()
{
	if (m_pTileGroup)
		((CAttachmentHeadTileGrp*)m_pTileGroup)->OnNewAttachCompleted();
	OnUpdateControls();
	EnableControlLinks();	
}

//-----------------------------------------------------------------------------
void CAttachmentPaneView::OnAfterLoadAttachments()
{
	if (m_pTileGroup)
		((CAttachmentHeadTileGrp*)m_pTileGroup)->OnAfterLoadAttachments();
	
	OnAttachmentChanged();
}

//-----------------------------------------------------------------------------
void CAttachmentPaneView::OnAttachmentChanged()
{
	GetDMSClientDoc()->SetCurrentAttachment();
	EnableControlLinks();
	OnUpdateControls();
}

//-----------------------------------------------------------------------------
void CAttachmentPaneView::OnBarcodeChanged()
{
	DataStr barcodeValue = GetDMSClientDoc()->m_pCurrAttachmentInfo->m_BarcodeValue;
	DataStr barcodeType = GetDMSClientDoc()->m_pCurrAttachmentInfo->m_BarcodeType;

	if (barcodeValue.IsEmpty())
		return; // se il barcode e' empty non procedo coi controlli

	if (!AfxGetTbRepositoryManager()->IsValidEABarcodeValue(barcodeValue))
	{
		GetDMSClientDoc()->m_pCurrAttachmentInfo->m_BarcodeValue.Clear();
		OnUpdateControls();
	}
}

//-----------------------------------------------------------------------------------  ----------------------------
void CAttachmentPaneView::OnNewAttachment()
{
}

//---------------------------------------------------------------------------------------------------------------
void CAttachmentPaneView::OnEditAttachment()
{
	GetDMSClientDoc()->m_bEditMode = TRUE;
	EnableControlLinks();
	OnUpdateControls();
}

//---------------------------------------------------------------------------------------------------------------
void CAttachmentPaneView::OnSaveAttachment()
{
	if (CheckForm(!GetDMSClientDoc()->GetServerDoc()->IsInUnattendedMode()))
	{
		if (GetDMSClientDoc()->SaveCurrentAttachment())
		{
			EnableControlLinks();
			OnUpdateControls();
		}
	}
}

//---------------------------------------------------------------------------------------------------------------
void CAttachmentPaneView::OnUndoChangesAttachment()
{
	GetDMSClientDoc()->UndoChangesAttachment();
	EnableControlLinks();
	OnUpdateControls();
}

//---------------------------------------------------------------------------------------------------------------
void CAttachmentPaneView::OnDeleteAttachment()
{
	if (GetDMSClientDoc()->DeleteCurrentAttachment())
	{
		if (m_pTileGroup)
			((CAttachmentHeadTileGrp*)m_pTileGroup)->OnAfterLoadAttachments();
		OnUpdateControls();
	}
}

//---------------------------------------------------------------------------------------------------------------
void CAttachmentPaneView::OnViewAttachment()
{
	GetDMSClientDoc()->ViewCurrentAttachment();
}

//---------------------------------------------------------------------------------------------------------------
void CAttachmentPaneView::OnCopyAttachment()
{
	AfxGetTbRepositoryManager()->SaveArchiveDocFileInFolder((int)GetDMSClientDoc()->m_pCurrAttachmentInfo->m_ArchivedDocId);
}

//---------------------------------------------------------------------------------------------------------------
void CAttachmentPaneView::OnSendAttachment()
{
	GetDMSClientDoc()->SendCurrentAttachment();
}

//---------------------------------------------------------------------------------------------------------------
void CAttachmentPaneView::OnReloadAttachment()
{
	// prima forzo il changed del controllo con il nr max di documenti da estrarre
	GetDMSClientDoc()->m_pAttachmentPane->GetAttachmentPaneView()->CheckForm(); 
	AfxGetTbRepositoryManager()->SetAttachmentPanelOptions(GetDMSClientDoc()->m_nMaxDocNrToShow);
	GetDMSClientDoc()->LoadAttachments();
}

//---------------------------------------------------------------------------------------------------------------
void CAttachmentPaneView::OnCheckOutAttachment()
{
	GetDMSClientDoc()->CheckOutCurrentAttachment();
}

//---------------------------------------------------------------------------------------------------------------
void CAttachmentPaneView::OnCheckInAttachment()
{
	GetDMSClientDoc()->CheckInCurrentAttachment();
}

//-----------------------------------------------------------------------------
void CAttachmentPaneView::OnUpdateNewAttachment(CCmdUI* pCmdUI)
{
	pCmdUI->Enable(
		((CAbstractFormDoc*)m_pDocument)->ValidCurrentRecord() && 
		!GetDMSClientDoc()->m_bEditMode && 
		((CAbstractFormDoc*)m_pDocument)->GetFormMode() != CAbstractFormDoc::NEW && 
		((CAbstractFormDoc*)m_pDocument)->GetFormMode() != CAbstractFormDoc::FIND
		);
}

//-----------------------------------------------------------------------------
void CAttachmentPaneView::OnUpdateSaveAttachment(CCmdUI* pCmdUI)
{
	pCmdUI->Enable(GetDMSClientDoc()->m_bEditMode);
}

//-----------------------------------------------------------------------------
void CAttachmentPaneView::OnUpdateUndoChangesAttachment(CCmdUI* pCmdUI)
{
	pCmdUI->Enable(GetDMSClientDoc()->m_bEditMode);
}

//---------------------------------------------------------------------------------------------------------------
void CAttachmentPaneView::OnUpdateEditAttachment(CCmdUI* pCmdUI)
{
	pCmdUI->Enable(GetDMSClientDoc()->IsCurrAttachmentValid() && !GetDMSClientDoc()->m_bEditMode);
}

//---------------------------------------------------------------------------------------------------------------
void CAttachmentPaneView::OnUpdateDeleteAttachment(CCmdUI* pCmdUI)
{
	pCmdUI->Enable(GetDMSClientDoc()->IsCurrAttachmentValid() && !GetDMSClientDoc()->m_bEditMode);
}

//---------------------------------------------------------------------------------------------------------------
void CAttachmentPaneView::OnUpdateViewAttachment(CCmdUI* pCmdUI)
{
	pCmdUI->Enable(GetDMSClientDoc()->IsCurrAttachmentValid() && !GetDMSClientDoc()->m_bEditMode && !GetDMSClientDoc()->m_pCurrAttachmentInfo->m_IsAPapery);
}

//---------------------------------------------------------------------------------------------------------------
void CAttachmentPaneView::OnUpdateCopyAttachment(CCmdUI* pCmdUI)
{
	pCmdUI->Enable(GetDMSClientDoc()->IsCurrAttachmentValid() && !GetDMSClientDoc()->m_bEditMode && !GetDMSClientDoc()->m_pCurrAttachmentInfo->m_IsAPapery);
}

//---------------------------------------------------------------------------------------------------------------
void CAttachmentPaneView::OnUpdateSendAttachment(CCmdUI* pCmdUI)
{
	pCmdUI->Enable(GetDMSClientDoc()->m_bMailActivated && GetDMSClientDoc()->IsCurrAttachmentValid() && !GetDMSClientDoc()->m_bEditMode && !GetDMSClientDoc()->m_pCurrAttachmentInfo->m_IsAPapery);
}

//---------------------------------------------------------------------------------------------------------------
void CAttachmentPaneView::OnUpdateCheckOutAttachment(CCmdUI* pCmdUI)
{
	pCmdUI->Enable(GetDMSClientDoc()->IsCurrAttachmentValid() && !GetDMSClientDoc()->m_bEditMode && GetDMSClientDoc()->m_pCurrAttachmentInfo->CanCheckOut() && !GetDMSClientDoc()->m_pCurrAttachmentInfo->m_IsAPapery);
}

//---------------------------------------------------------------------------------------------------------------
void CAttachmentPaneView::OnUpdateCheckInAttachment(CCmdUI* pCmdUI)
{
	pCmdUI->Enable(GetDMSClientDoc()->IsCurrAttachmentValid() && !GetDMSClientDoc()->m_bEditMode && GetDMSClientDoc()->m_pCurrAttachmentInfo->IsOwnCheckOut() && !GetDMSClientDoc()->m_pCurrAttachmentInfo->m_IsAPapery);
}

//---------------------------------------------------------------------------------------------------------------
void CAttachmentPaneView::OnUpdateReloadAttachments(CCmdUI* pCmdUI)
{
	pCmdUI->Enable(GetDMSClientDoc()->IsCurrAttachmentValid() && !GetDMSClientDoc()->m_bEditMode);
}

//////////////////////////////////////////////////////////////////////////////
//					    CAttachmentBaseTileDlg
//////////////////////////////////////////////////////////////////////////////
//=============================================================================
IMPLEMENT_DYNCREATE(CAttachmentBaseTileDlg, CTileDialog)

//-----------------------------------------------------------------------------
CAttachmentBaseTileDlg::CAttachmentBaseTileDlg()
	:
	CTileDialog()
{
	SetHasTitle(FALSE);
}

//-----------------------------------------------------------------------------
CAttachmentBaseTileDlg::CAttachmentBaseTileDlg(const CString& sName, int nIDD, CWnd* pParent /*= NULL*/) 
	: 
	CTileDialog(sName, nIDD, pParent) 
{
	SetHasTitle(FALSE);
}

//-----------------------------------------------------------------------------
CDDMS* CAttachmentBaseTileDlg::GetDMSClientDoc() const
{
	return(CDDMS*)GetDocument()->GetClientDoc(RUNTIME_CLASS(CDDMS));
}

//-----------------------------------------------------------------------------
void CAttachmentBaseTileDlg::EnableTileDialogControlLinks(BOOL /*bEnable  = TRUE*/, BOOL bMustSetOSLReadOnly /*=FALSE*/)
{
	if (!GetDocument())
		return;

	BOOL dmsEnable = FALSE;

	switch (GetDocument()->GetFormMode())
	{
	case CBaseDocument::NEW:
	case CBaseDocument::FIND:
		dmsEnable = FALSE;
		break;

	default:
		dmsEnable = GetDMSClientDoc()->m_bEditMode;
	}

	::EnableControlLinks(m_pControlLinks, dmsEnable, bMustSetOSLReadOnly);
	EnableTabManagerControlLinks(dmsEnable);
}

//-----------------------------------------------------------------------------
void CAttachmentBaseTileDlg::EnableTileDialogControls()
{
	EnableTileDialogControlLinks();
}

//////////////////////////////////////////////////////////////////////////////
//					  CAttachmentHeadTileGrp
//////////////////////////////////////////////////////////////////////////////
//=============================================================================
IMPLEMENT_DYNCREATE(CAttachmentHeadTileGrp, CTileGroup)

//-----------------------------------------------------------------------------
void CAttachmentHeadTileGrp::Customize()
{
	SetLayoutType(CLayoutContainer::VBOX);

	CBaseTileDialog* pTile = AddTile(RUNTIME_CLASS(CAttachmentHeadTileDlg), IDD_TD_ATTACHMENTS_LIST, _T(""), TILE_STANDARD);
	pTile->SetHasTitle(FALSE);

	AddTile(RUNTIME_CLASS(CAttachmentBodyTileDlg), IDD_TD_ATTACHMENT_BODY, _T(""), TILE_STANDARD);
}

//-----------------------------------------------------------------------------
void CAttachmentHeadTileGrp::OnAfterLoadAttachments()
{
	for (int i = 0; i < this->GetTileDialogs()->GetSize(); i++)
		((CAttachmentBaseTileDlg*)GetTileDialogs()->GetAt(i))->OnAfterLoadAttachments();
}

//-----------------------------------------------------------------------------
void CAttachmentHeadTileGrp::OnNewAttachCompleted()
{
	for (int i = 0; i < this->GetTileDialogs()->GetSize(); i++)
		((CAttachmentBaseTileDlg*)GetTileDialogs()->GetAt(i))->OnNewAttachCompleted();
}

//////////////////////////////////////////////////////////////////////////////
//					    CAttachmentBodyTileDlg
//////////////////////////////////////////////////////////////////////////////
//
//=============================================================================
IMPLEMENT_DYNCREATE(CAttachmentBodyTileDlg, CAttachmentBaseTileDlg)

//-----------------------------------------------------------------------------
CAttachmentBodyTileDlg::CAttachmentBodyTileDlg()
	:
	CAttachmentBaseTileDlg(_NS_TILEDLG("Body"), IDD_TD_ATTACHMENT_BODY)
{
}

//-----------------------------------------------------------------------------
CAttachmentBodyTileDlg::~CAttachmentBodyTileDlg()
{
}

//-----------------------------------------------------------------------------
void CAttachmentBodyTileDlg::BuildDataControlLinks()
{
	AddTileManager(IDC_ATTACHMENT_TILE_MNG, RUNTIME_CLASS(CAttachmentTileMng), _NS_TABMNG("AttachmentTileMng"));
	SetMaxWidth(295);
}

//-----------------------------------------------------------------------------
void CAttachmentBodyTileDlg::EnableTileDialogControlLinks(BOOL bEnable /*=TRUE*/, BOOL bMustSetOSLReadOnly /*=FALSE*/)
{
	CAttachmentBaseTileDlg::EnableTileDialogControlLinks(bEnable, bMustSetOSLReadOnly);

	BOOL bIsAPapery = GetDMSClientDoc()->m_pCurrAttachmentInfo->m_IsAPapery;

	CAttachmentPaneView* pAttachmentPane = GetDMSClientDoc()->m_pAttachmentPane->GetAttachmentPaneView();

	if (bIsAPapery)
		m_pTabManager->TabDialogActivate(IDC_ATTACHMENT_TILE_MNG, pAttachmentPane->m_IIDTGBarcode);
	
	m_pTabManager->TabDialogEnable(IDC_ATTACHMENT_TILE_MNG, pAttachmentPane->m_IIDTGDetails, !bIsAPapery);
	m_pTabManager->TabDialogEnable(IDC_ATTACHMENT_TILE_MNG, pAttachmentPane->m_IIDTGPreview, !bIsAPapery);
	m_pTabManager->TabDialogEnable(IDC_ATTACHMENT_TILE_MNG, pAttachmentPane->m_IIDTGBookmarks, !bIsAPapery);
	if (AfxIsActivated(TBEXT_APP, SOSCONNECTOR_FUNCTIONALITY) && AfxGetTbRepositoryManager()->GetDMSSettings()->GetSettings()->l_EnableSOS)
		m_pTabManager->TabDialogEnable(IDC_ATTACHMENT_TILE_MNG, pAttachmentPane->m_IIDTGSos, !bIsAPapery);
}

//////////////////////////////////////////////////////////////////////////////
//					  CAttachmentHeadTileDlg
//////////////////////////////////////////////////////////////////////////////
//
//=============================================================================
IMPLEMENT_DYNCREATE(CAttachmentHeadTileDlg, CAttachmentBaseTileDlg)

//-----------------------------------------------------------------------------
BEGIN_MESSAGE_MAP(CAttachmentHeadTileDlg, CAttachmentBaseTileDlg)
	//{{AFX_MSG_MAP(CAttachmentHeadTileDlg)
	ON_LBN_DBLCLK(IDC_ATTACHMENTS_LISTBOX, OnAttachmentsListDblClick)

	//}}AFX_MSG_MAP
END_MESSAGE_MAP()

//-----------------------------------------------------------------------------
CAttachmentHeadTileDlg::CAttachmentHeadTileDlg()
	:
	CAttachmentBaseTileDlg(_NS_TILEDLG("Head"), IDD_TD_ATTACHMENTS_LIST)
{
	SetHasTitle(FALSE);
}

//-----------------------------------------------------------------------------
void CAttachmentHeadTileDlg::BuildDataControlLinks()
{
	AddLink(IDC_ATTACHMENTS_COUNT, _NS_LNK("AttachmentCounter"), NULL, &GetDMSClientDoc()->m_sAttachmentsCounter, RUNTIME_CLASS(CStrStatic));
	AddLink(IDC_ATTACHMENTS_MAX_NR, _NS_LNK("AttachmentMaxNr"), NULL, &GetDMSClientDoc()->m_nMaxDocNrToShow, RUNTIME_CLASS(CIntEdit));

	m_pAttListBox = (CAttachmentsListBox*)AddLink(
		IDC_ATTACHMENTS_LISTBOX,
		_NS_LNK("AttachmentsListBox"),
		NULL,
		&GetDMSClientDoc()->m_nCurrAttachment,
		RUNTIME_CLASS(CAttachmentsListBox)
		);
	m_pAttListBox->SetAttachmentsArray(GetDMSClientDoc()->m_pAttachments);
}

//-----------------------------------------------------------------------------
void CAttachmentHeadTileDlg::OnAfterLoadAttachments()
{
	m_pAttListBox->FillListBox();
}

//-----------------------------------------------------------------------------
void CAttachmentHeadTileDlg::OnNewAttachCompleted()
{	
	m_pAttListBox->FillListBox();
}

//-----------------------------------------------------------------------------
void CAttachmentHeadTileDlg::AttachFiles(CStringArray* pDroppedFiles)
{	
	GetDMSClientDoc()->AttachFiles(pDroppedFiles);	
}

//-----------------------------------------------------------------------------
void CAttachmentHeadTileDlg::EnableTileDialogControlLinks(BOOL bEnable  /*= TRUE*/, BOOL bMustSetOSLReadOnly /*=FALSE*/)
{
	CAttachmentBaseTileDlg::EnableTileDialogControlLinks(bEnable, bMustSetOSLReadOnly);
	GetDMSClientDoc()->m_nCurrAttachment.SetReadOnly(GetDMSClientDoc()->m_bEditMode);
}

//-----------------------------------------------------------------------------
void CAttachmentHeadTileDlg::OnAttachmentsListDblClick()
{
	GetDMSClientDoc()->ViewCurrentAttachment();
}

//////////////////////////////////////////////////////////////////////////////
//						    CAttachmentTileMng
//////////////////////////////////////////////////////////////////////////////
//
//=============================================================================
IMPLEMENT_DYNCREATE(CAttachmentTileMng, CTileManager)

//-----------------------------------------------------------------------------	
CAttachmentTileMng::CAttachmentTileMng()
	:
	CTileManager()
{
	SetShowMode(NORMAL);
}

//-----------------------------------------------------------------------------
void CAttachmentTileMng::Customize()
{
	CAttachmentPaneView* pAttachmentPane = ((CAttachmentBaseTileDlg*)GetParent())->GetDMSClientDoc()->m_pAttachmentPane->GetAttachmentPaneView();
	pAttachmentPane->m_IIDTGDetails = AddTileGroup(RUNTIME_CLASS(CAttachmentDetailsTileGrp), _NS_TILEGRP("Details"), _TB("Details"), _T(""), _TB("Details"))->GetTileGroupID();
	pAttachmentPane->m_IIDTGPreview = AddTileGroup(RUNTIME_CLASS(CAttachmentPreviewTileGrp), _NS_TILEGRP("Preview"), _TB("Preview"), _T(""), _TB("Preview"))->GetTileGroupID();
	pAttachmentPane->m_IIDTGBookmarks = AddTileGroup(RUNTIME_CLASS(CAttachmentBookmarksTileGrp), _NS_TILEGRP("Bookmarks"), _TB("Bookmarks"), _T(""), _TB("Bookmarks"))->GetTileGroupID();
	
	if (AfxGetTbRepositoryManager()->GetDMSSettings()->GetSettings()->l_EnableBarcode)
		pAttachmentPane->m_IIDTGBarcode = AddTileGroup(RUNTIME_CLASS(CAttachmentBarcodeTileGrp), _NS_TILEGRP("Barcode"), _TB("Barcode"), _T(""), _TB("Barcode"))->GetTileGroupID();

	if (
		AfxIsActivated(TBEXT_APP, SOSCONNECTOR_FUNCTIONALITY) && 
		AfxGetOleDbMng()->DMSSOSEnable() &&
		AfxGetTbRepositoryManager()->GetDMSSettings()->GetSettings()->l_EnableSOS && 
		((CAttachmentBaseTileDlg*)GetParent())->GetDMSClientDoc()->IsDocumentNamespaceInSOS()
		)
		pAttachmentPane->m_IIDTGSos = AddTileGroup(RUNTIME_CLASS(CAttachmentSOSTileGrp), _NS_TILEGRP("SOS"), _TB("SOS"), _T(""), _TB("SOS"))->GetTileGroupID();
}

//////////////////////////////////////////////////////////////////////////////
//					  CAttachmentDetailsTileGrp
//////////////////////////////////////////////////////////////////////////////
//
//=============================================================================
IMPLEMENT_DYNCREATE(CAttachmentDetailsTileGrp, CTileGroup)

//-----------------------------------------------------------------------------
void CAttachmentDetailsTileGrp::Customize()
{
	SetLayoutType(CLayoutContainer::VBOX);

	AddTile(RUNTIME_CLASS(CAttachmentDetailsTileDlg), IDD_TD_ATTACHMENT_DETAILS, _T(""), TILE_STANDARD);
}

//////////////////////////////////////////////////////////////////////////////
//					  CAttachmentDetailsTileDlg
//////////////////////////////////////////////////////////////////////////////
//
//=============================================================================
IMPLEMENT_DYNCREATE(CAttachmentDetailsTileDlg, CAttachmentBaseTileDlg)

//-----------------------------------------------------------------------------
CAttachmentDetailsTileDlg::CAttachmentDetailsTileDlg()
	:
	CAttachmentBaseTileDlg(_NS_TILEDLG("Details"), IDD_TD_ATTACHMENT_DETAILS)
{
	SetHasTitle(FALSE);
}

//-----------------------------------------------------------------------------
void CAttachmentDetailsTileDlg::BuildDataControlLinks()
{
	// declare gia' fatta dentro il file CDDMS
	DMSAttachmentInfo* pCurrAttInfo = GetDMSClientDoc()->m_pCurrAttachmentInfo;
	AddLink(IDC_ATT_DETAILS_DESCRIPTION, _NS_LNK("Description"), NULL, &pCurrAttInfo->m_Description, RUNTIME_CLASS(CStrEdit));
	AddLink(IDC_ATT_DETAILS_ISMAINDOC, _NS_LNK("IsMainDoc"), NULL, &pCurrAttInfo->m_IsMainDoc, RUNTIME_CLASS(CBoolButton));
	AddLink(IDC_ATT_DETAILS_ISFORMAIL, _NS_LNK("IsForMail"), NULL, &pCurrAttInfo->m_IsForMail, RUNTIME_CLASS(CBoolButton));	
	AddLink(IDC_ATT_DETAILS_PATH, _NS_LNK("Path"), NULL, &pCurrAttInfo->m_OriginalPath, RUNTIME_CLASS(CStrStatic));
	AddLink(IDC_ATT_DETAILS_SIZE, _NS_LNK("Size"), NULL, &pCurrAttInfo->m_Size, RUNTIME_CLASS(CLongStatic));
	AddLink(IDC_ATT_DETAILS_ARCHIVE_DATE, _NS_LNK("ArchiveDate"), NULL, &pCurrAttInfo->m_ArchivedDate, RUNTIME_CLASS(CDateStatic));
	AddLink(IDC_ATT_DETAILS_ATTACH_DATE, _NS_LNK("AttachDate"), NULL, &pCurrAttInfo->m_AttachedDate, RUNTIME_CLASS(CDateStatic));
	AddLink(IDC_ATT_DETAILS_CREATED_BY, _NS_LNK("CreatedBy"), NULL, &pCurrAttInfo->m_CreatedBy, RUNTIME_CLASS(CStrStatic));
	AddLink(IDC_ATT_DETAILS_MODIFIED_BY, _NS_LNK("ModifiedBy"), NULL, &pCurrAttInfo->m_ModifiedBy, RUNTIME_CLASS(CStrStatic));
	AddLink(IDC_ATT_DETAILS_ATTACHMENT_ID, _NS_LNK("AttachmentID"), NULL, &pCurrAttInfo->m_attachmentID, RUNTIME_CLASS(CLongStatic));
	AddLink(IDC_ATT_DETAILS_ARCHIVE_ID, _NS_LNK("ArchiveID"), NULL, &pCurrAttInfo->m_ArchivedDocId, RUNTIME_CLASS(CLongStatic));
}

//////////////////////////////////////////////////////////////////////////////
//					  CAttachmentPreviewTileGrp
//////////////////////////////////////////////////////////////////////////////
//
//=============================================================================
IMPLEMENT_DYNCREATE(CAttachmentPreviewTileGrp, CTileGroup)

//-----------------------------------------------------------------------------
void CAttachmentPreviewTileGrp::Customize()
{
	SetLayoutType(CLayoutContainer::VBOX);

	AddTile(RUNTIME_CLASS(CAttachmentPreviewTileDlg), IDD_TD_ATTACHMENT_PREVIEW, _T(""), TILE_STANDARD);
}

//////////////////////////////////////////////////////////////////////////////
//					  CAttachmentPreviewTileDlg
//////////////////////////////////////////////////////////////////////////////
//
//=============================================================================
IMPLEMENT_DYNCREATE(CAttachmentPreviewTileDlg, CAttachmentBaseTileDlg)

//-----------------------------------------------------------------------------
CAttachmentPreviewTileDlg::CAttachmentPreviewTileDlg()
	:
	CAttachmentBaseTileDlg(_NS_TILEDLG("Preview"), IDD_TD_ATTACHMENT_PREVIEW)
{
	SetHasTitle(FALSE);
}

//-----------------------------------------------------------------------------
void CAttachmentPreviewTileDlg::BuildDataControlLinks()
{  
	// declare gia' fatta dentro il file CDDMS
	CTBDMSViewerCtrl* pPreview = (CTBDMSViewerCtrl*)AddLink
		(
		IDC_ATT_PREVIEW,
		_T("Preview"),
		NULL,
		&(GetDMSClientDoc()->m_pCurrAttachmentInfo->m_TemporaryPathFile),
		RUNTIME_CLASS(CTBDMSViewerCtrl)
		);
}  

//-----------------------------------------------------------------------------
BOOL CAttachmentPreviewTileDlg::OnPrepareAuxData()
{
	return TRUE;
}

//-----------------------------------------------------------------------------
void CAttachmentPreviewTileDlg::EnableTileDialogControlLinks(BOOL bEnable  /*= TRUE*/, BOOL bMustSetOSLReadOnly /*=FALSE*/)
{
	CAttachmentBaseTileDlg::EnableTileDialogControlLinks(bEnable, bMustSetOSLReadOnly);
	GetDMSClientDoc()->m_pCurrAttachmentInfo->m_TemporaryPathFile.SetReadOnly(FALSE);
}

//////////////////////////////////////////////////////////////////////////////
//					  CAttachmentBookmarksTileGrp
//////////////////////////////////////////////////////////////////////////////
//
//=============================================================================
IMPLEMENT_DYNCREATE(CAttachmentBookmarksTileGrp, CTileGroup)

//-----------------------------------------------------------------------------
void CAttachmentBookmarksTileGrp::Customize()
{
	SetLayoutType(CLayoutContainer::VBOX);

	CBaseTileDialog* pTile = AddTile(RUNTIME_CLASS(CAttachmentBookmarksTileDlg), IDD_TD_ATTACHMENT_BOOKMARKS, _T(""), TILE_STANDARD);
	
}

//////////////////////////////////////////////////////////////////////////////
//					  CAttachmentBookmarksTileDlg
//////////////////////////////////////////////////////////////////////////////
//
//=============================================================================
IMPLEMENT_DYNCREATE(CAttachmentBookmarksTileDlg, CAttachmentBaseTileDlg)

//-----------------------------------------------------------------------------
BEGIN_MESSAGE_MAP(CAttachmentBookmarksTileDlg, CAttachmentBaseTileDlg)
	//{{AFX_MSG_MAP(CAttachmentBookmarksTileDlg)
	ON_EN_VALUE_CHANGED(IDC_ATT_BE_BOOKMARK_TYPE, OnBEBookmarkTypeChanged)
	ON_EN_VALUE_CHANGED(IDC_ATT_BE_BOOKMARK_DESCRI, OnBESearchFieldChanged)
	ON_CONTROL(BEN_ROW_CHANGED, IDC_ATT_BE_BOOKMARKS, OnBEBookmarkRowChanged)
	//}}AFX_MSG_MAP
END_MESSAGE_MAP()

//-----------------------------------------------------------------------------
CAttachmentBookmarksTileDlg::CAttachmentBookmarksTileDlg()
	:
	CAttachmentBaseTileDlg(_NS_TILEDLG("Bookmarks"), IDD_TD_ATTACHMENT_BOOKMARKS)
{
	SetHasTitle(FALSE);
}

//-----------------------------------------------------------------------------
void CAttachmentBookmarksTileDlg::BuildDataControlLinks()
{
	// declare gia' fatta dentro il file CDDMS
	DMSAttachmentInfo* pCurrAttInfo = GetDMSClientDoc()->m_pCurrAttachmentInfo;
	AddLink(IDC_ATT_FREE_TAGS, _NS_LNK("FreeTags"), NULL, &pCurrAttInfo->m_FreeTag, RUNTIME_CLASS(CStrEdit));
	
	m_pBookmarkBE = (CAttachmentBookmarksBodyEdit*) AddLink
		(
		IDC_ATT_BE_BOOKMARKS,
		GetDMSClientDoc()->m_pDBTBookmarks,
		RUNTIME_CLASS(CAttachmentBookmarksBodyEdit),
		NULL,
		_TB("Bookmarks")
		);
}

//-----------------------------------------------------------------------------
void CAttachmentBookmarksTileDlg::EnableTileDialogControlLinks(BOOL bEnable /*= TRUE*/, BOOL bMustSetOSLReadOnly /*=FALSE*/)
{
	CAttachmentBaseTileDlg::EnableTileDialogControlLinks(bEnable, bMustSetOSLReadOnly);

	if (GetDMSClientDoc()->m_bEditMode)
		GetDMSClientDoc()->m_pDBTBookmarks->OnDisableControlsForEdit();
}

// intercetto il change nella combobox del tipo bookmark
//-----------------------------------------------------------------------------
void CAttachmentBookmarksTileDlg::OnBEBookmarkTypeChanged()
{
	GetDMSClientDoc()->m_pAttachmentPane->GetAttachmentPaneView()->OnUpdateControls();
}

//-----------------------------------------------------------------------------
void CAttachmentBookmarksTileDlg::OnBESearchFieldChanged()
{
	GetDMSClientDoc()->m_pAttachmentPane->GetAttachmentPaneView()->OnUpdateControls();
}

//-----------------------------------------------------------------------------
void CAttachmentBookmarksTileDlg::OnBEBookmarkRowChanged()
{
	if (m_pBookmarkBE)
		m_pBookmarkBE->OnBookmarkRowChanged();
}

//////////////////////////////////////////////////////////////////////////////
//					  CAttachmentBarcodeTileGrp
//////////////////////////////////////////////////////////////////////////////
//
//=============================================================================
IMPLEMENT_DYNCREATE(CAttachmentBarcodeTileGrp, CTileGroup)

//-----------------------------------------------------------------------------
void CAttachmentBarcodeTileGrp::Customize()
{
	SetLayoutType(CLayoutContainer::VBOX);
	AddTile(RUNTIME_CLASS(CAttachmentBarcodeTileDlg), IDD_TD_ATTACHMENT_BARCODE, _T(""), TILE_STANDARD);
}

//////////////////////////////////////////////////////////////////////////////
//					  CAttachmentBarcodeTileDlg
//////////////////////////////////////////////////////////////////////////////
//
//=============================================================================
IMPLEMENT_DYNCREATE(CAttachmentBarcodeTileDlg, CAttachmentBaseTileDlg)

//-----------------------------------------------------------------------------
BEGIN_MESSAGE_MAP(CAttachmentBarcodeTileDlg, CAttachmentBaseTileDlg)
	//{{AFX_MSG_MAP(CAttachmentBarcodeTileDlg)
	ON_BN_CLICKED(IDC_ATT_DETECT_BARCODE, OnDetectBarcodeClicked)
	//}}AFX_MSG_MAP
END_MESSAGE_MAP()

//-----------------------------------------------------------------------------
CAttachmentBarcodeTileDlg::CAttachmentBarcodeTileDlg()
	:
	CAttachmentBaseTileDlg(_NS_TILEDLG("Barcode"), IDD_TD_ATTACHMENT_BARCODE)
{
}

//-----------------------------------------------------------------------------
void CAttachmentBarcodeTileDlg::BuildDataControlLinks()
{
	// declare gia' fatta dentro il file CDDMS
	DMSAttachmentInfo* pCurrAttInfo = GetDMSClientDoc()->m_pCurrAttachmentInfo;

	AddLink(IDC_ATT_BARCODE, _NS_LNK("Barcode"), NULL, &(pCurrAttInfo->m_BarcodeValue), RUNTIME_CLASS(CStrEdit));

	CTBDMSBarcodeViewerCtrl* pBarcodeViewer = (CTBDMSBarcodeViewerCtrl*)AddLink
		(
		IDC_ATT_BARCODEVIEWER,
		_T("BarcodeViewer"),
		NULL,
		&(pCurrAttInfo->m_BarcodeValue),
		RUNTIME_CLASS(CTBDMSBarcodeViewerCtrl)
		);
	pBarcodeViewer->SetToolStripVisibility(FALSE);
	pBarcodeViewer->EnablePreviewNotAvailable(FALSE);
	pBarcodeViewer->SetSkipRecalcCtrlSize(TRUE);
	pBarcodeViewer->SetBarcode(pCurrAttInfo->m_BarcodeValue, pCurrAttInfo->m_BarcodeType); // serve per la visualizzazione del barcode

	if (pCurrAttInfo->m_IsAPapery)
		AddLink(IDC_ATT_BARCODE_NOTES, _NS_LNK("Notes"), NULL, &(pCurrAttInfo->m_Description), RUNTIME_CLASS(CStrEdit));
	else
	{
		CWnd* pWnd = GetDlgItem(IDC_ATT_BARCODE_NOTES);
		pWnd->ShowWindow(SW_HIDE);
		pWnd = GetDlgItem(IDC_ATT_STATIC_BARCODE_NOTES);
		pWnd->ShowWindow(SW_HIDE);
	}

	// visualizzo il pulsante di Detect se l'impostazione e' manuale
	if (
		AfxGetTbRepositoryManager()->GetDMSSettings()->GetSettings()->l_EnableBarcode && 
		AfxGetTbRepositoryManager()->GetDMSSettings()->GetSettings()->l_AutomaticBarcodeDetection && 
		pCurrAttInfo->m_IsAPapery
		)
	{
		CWnd* pWnd = GetDlgItem(IDC_ATT_DETECT_BARCODE);
		pWnd->ShowWindow(SW_HIDE);
	}
	else
	{
		AddLink
			(
				IDC_ATT_DETECT_BARCODE,
				_NS_LNK("ManualDetectBarcode"),
				NULL,
				&(m_bManualBarcodeDetection)
				);

		/*CExtButton* pButton = (CExtButton*)AddLink
			(
				IDC_ATT_DETECT_BARCODE,
				_NS_LNK("ManualDetectBarcode"),
				NULL,
				&(m_bManualBarcodeDetection)
				);
		pButton->SetPngImages(ExtensionsIcon(szIconBarcode, CONTROL));*/
	}
}

//-----------------------------------------------------------------------------
void CAttachmentBarcodeTileDlg::EnableTileDialogControlLinks(BOOL bEnable /*= TRUE*/, BOOL bMustSetOSLReadOnly /*=FALSE*/)
{
	CAttachmentBaseTileDlg::EnableTileDialogControlLinks(bEnable, bMustSetOSLReadOnly);
	
	DMSAttachmentInfo* pCurrAttInfo = GetDMSClientDoc()->m_pCurrAttachmentInfo;
	pCurrAttInfo->m_BarcodeValue.SetReadOnly(!GetDMSClientDoc()->m_bEditMode || GetDMSClientDoc()->m_pCurrAttachmentInfo->m_IsAPapery);
	
	CWnd* pWnd = GetDlgItem(IDC_ATT_BARCODE_NOTES);
	pWnd->ShowWindow((pCurrAttInfo->m_IsAPapery) ? SW_SHOW : SW_HIDE);
	pWnd = GetDlgItem(IDC_ATT_STATIC_BARCODE_NOTES);
	pWnd->ShowWindow((pCurrAttInfo->m_IsAPapery) ? SW_SHOW : SW_HIDE);

	// il button per il detect barcode manuale e' visibile solo se il barcode e' abilitato, il detect e' manuale e non si tratta di un papery
	if (
		AfxGetTbRepositoryManager()->GetDMSSettings()->GetSettings()->l_EnableBarcode && 
		!AfxGetTbRepositoryManager()->GetDMSSettings()->GetSettings()->l_AutomaticBarcodeDetection && 
		!pCurrAttInfo->m_IsAPapery
		)
	{
		pWnd = GetDlgItem(IDC_ATT_DETECT_BARCODE);
		pWnd->ShowWindow(SW_SHOW);
		m_bManualBarcodeDetection = GetDMSClientDoc()->m_bEditMode;
		//m_bManualBarcodeDetection.SetReadOnly(!GetDMSClientDoc()->m_bEditMode); // e' abilitato solo se sono in edit
	}
	else
	{
		pWnd = GetDlgItem(IDC_ATT_DETECT_BARCODE);
		pWnd->ShowWindow(SW_HIDE);
		m_bManualBarcodeDetection = FALSE;
	}
}

//-----------------------------------------------------------------------------
void CAttachmentBarcodeTileDlg::OnDetectBarcodeClicked()
{
	GetDMSClientDoc()->DoDetectBarcodeForCurrentAttachInfo();
	OnUpdateControls();
}

//////////////////////////////////////////////////////////////////////////////
//					  CAttachmentSOSTileGrp
//////////////////////////////////////////////////////////////////////////////
//
//=============================================================================
IMPLEMENT_DYNCREATE(CAttachmentSOSTileGrp, CTileGroup)

//-----------------------------------------------------------------------------
void CAttachmentSOSTileGrp::Customize()
{
	SetLayoutType(CLayoutContainer::VBOX);

	AddTile(RUNTIME_CLASS(CAttachmentSOSTileDlg), IDD_TD_ATTACHMENT_SOS, _T(""), TILE_STANDARD);
}

//////////////////////////////////////////////////////////////////////////////
//					  CAttachmentSOSTileDlg
//////////////////////////////////////////////////////////////////////////////
//
//=============================================================================
IMPLEMENT_DYNCREATE(CAttachmentSOSTileDlg, CAttachmentBaseTileDlg)

//-----------------------------------------------------------------------------
BEGIN_MESSAGE_MAP(CAttachmentSOSTileDlg, CAttachmentBaseTileDlg)
	//{{AFX_MSG_MAP(CAttachmentSOSTileDlg)
	ON_BN_CLICKED(IDC_ATT_SEND_TO_SOS, OnSendSOSClicked)
	//}}AFX_MSG_MAP
END_MESSAGE_MAP()

//-----------------------------------------------------------------------------
CAttachmentSOSTileDlg::CAttachmentSOSTileDlg()
	:
	CAttachmentBaseTileDlg(_NS_TILEDLG("SOS"), IDD_TD_ATTACHMENT_SOS)
{
}

//-----------------------------------------------------------------------------
void CAttachmentSOSTileDlg::BuildDataControlLinks()
{
	// declare gia' fatta dentro il file CDDMS
	DMSAttachmentInfo* pCurrAttInfo = GetDMSClientDoc()->m_pCurrAttachmentInfo;
	
	// descrizione stato
	AddLink(IDC_ATT_SOS_DISPATCH_STATUS, _NS_LNK("DispatchStatusDescription"), NULL, &(m_DocStatusDescription), RUNTIME_CLASS(CStrStatic));
	// bitmap
	CPictureStatic* pPictureStatic = (CPictureStatic*)AddLink
		(
			IDC_ATT_SOS_PICTURE, _NS_LNK("DispatchStatusImage"), NULL, &(m_DocStatusImage), RUNTIME_CLASS(CPictureStatic)
		);
	pPictureStatic->OnCtrlStyleBest();

	AddLink(IDC_ATT_SOS_CODE,		_NS_LNK("AbsoluteCode"),		NULL, &(m_AbsoluteCode),		RUNTIME_CLASS(CStrStatic));
	AddLink(IDC_ATT_SOS_LOTID,		_NS_LNK("LotID"),				NULL, &(m_LotID),				RUNTIME_CLASS(CStrStatic));
	AddLink(IDC_ATT_SOS_REGDATE,	_NS_LNK("RegistrationDate"),	NULL, &(m_RegistrationDate),	RUNTIME_CLASS(CStrStatic));
	AddLink(IDC_ATT_SEND_TO_SOS,	_NS_LNK("EnableAttachForSOS"),	NULL, &(m_bEnableAttachForSOS));
	AddLink(IDC_ATT_SOS_INFO,		_NS_LNK("Info"),				NULL, &(m_Info),				RUNTIME_CLASS(CStrStatic));

	// se l'allegato non e' presente nascondo tutti i controls
	if (!pCurrAttInfo || pCurrAttInfo->m_attachmentID <= 0)
	{
		HideSOSControls();
		return;
	}

	CWnd* pWnd;
	if (pCurrAttInfo->attachmentInfo->SOSDocumentStatus == StatoDocumento::EMPTY)
	{
		pWnd = GetDlgItem(IDC_ATT_SOS_DISPATCH_STATUS);
		if (pWnd) pWnd->ShowWindow(SW_HIDE);
		pWnd = GetDlgItem(IDC_ATT_SOS_PICTURE);
		if (pWnd) pWnd->ShowWindow(SW_HIDE);

		pWnd = GetDlgItem(IDC_ATT_SEND_TO_SOS);
		if (pWnd) pWnd->ShowWindow(SW_SHOW);
	}
	else
	{
		pWnd = GetDlgItem(IDC_ATT_SOS_DISPATCH_STATUS);
		if (pWnd) pWnd->ShowWindow(SW_SHOW);
		pWnd = GetDlgItem(IDC_ATT_SOS_PICTURE);
		if (pWnd) pWnd->ShowWindow(SW_SHOW);
		pWnd = GetDlgItem(IDC_ATT_SEND_TO_SOS);
		if (pWnd) pWnd->ShowWindow(SW_HIDE);
		pWnd = GetDlgItem(IDC_ATT_SOS_INFO);
		if (pWnd) pWnd->ShowWindow(SW_HIDE);
	}

	if (pCurrAttInfo->attachmentInfo->SOSDocumentStatus == StatoDocumento::EMPTY || pCurrAttInfo->attachmentInfo->SOSDocumentStatus < StatoDocumento::SENT)
	{
		pWnd = GetDlgItem(IDC_ATT_SOS_STATIC_CODE);
		if (pWnd) pWnd->ShowWindow(SW_HIDE);
		pWnd = GetDlgItem(IDC_ATT_SOS_CODE);
		if (pWnd) pWnd->ShowWindow(SW_HIDE);
		pWnd = GetDlgItem(IDC_ATT_SOS_STATIC_LOTID);
		if (pWnd) pWnd->ShowWindow(SW_HIDE);
		pWnd = GetDlgItem(IDC_ATT_SOS_LOTID);
		if (pWnd) pWnd->ShowWindow(SW_HIDE);
		pWnd = GetDlgItem(IDC_ATT_SOS_STATIC_REGDATE);
		if (pWnd) pWnd->ShowWindow(SW_HIDE);
		pWnd = GetDlgItem(IDC_ATT_SOS_REGDATE);
		if (pWnd) pWnd->ShowWindow(SW_HIDE);
	}
	else
	{
		pWnd = GetDlgItem(IDC_ATT_SOS_STATIC_CODE);
		if (pWnd) pWnd->ShowWindow(SW_SHOW);
		pWnd = GetDlgItem(IDC_ATT_SOS_CODE);
		if (pWnd) pWnd->ShowWindow(SW_SHOW);
		pWnd = GetDlgItem(IDC_ATT_SOS_STATIC_LOTID);
		if (pWnd) pWnd->ShowWindow(SW_SHOW);
		pWnd = GetDlgItem(IDC_ATT_SOS_LOTID);
		if (pWnd) pWnd->ShowWindow(SW_SHOW);
		pWnd = GetDlgItem(IDC_ATT_SOS_STATIC_REGDATE);
		if (pWnd) pWnd->ShowWindow(SW_SHOW);
		pWnd = GetDlgItem(IDC_ATT_SOS_REGDATE);
		if (pWnd) pWnd->ShowWindow(SW_SHOW);
	}
}

//-----------------------------------------------------------------------------
void CAttachmentSOSTileDlg::EnableTileDialogControlLinks(BOOL bEnable /*= TRUE*/, BOOL bMustSetOSLReadOnly /*=FALSE*/)
{
	CAttachmentBaseTileDlg::EnableTileDialogControlLinks(bEnable, bMustSetOSLReadOnly);

	DMSAttachmentInfo* pCurrAttInfo = GetDMSClientDoc()->m_pCurrAttachmentInfo;

	if (!pCurrAttInfo || pCurrAttInfo->m_attachmentID <= 0)
	{
		HideSOSControls();
		return;
	}

	StatoDocumento statoDoc = pCurrAttInfo->attachmentInfo->SOSDocumentStatus;

	CWnd* pWnd = GetDlgItem(IDC_ATT_SOS_DISPATCH_STATUS);
	if (pWnd) pWnd->ShowWindow((pCurrAttInfo->attachmentInfo->SOSDocumentStatus != StatoDocumento::EMPTY) ? SW_SHOW : SW_HIDE);
	pWnd = GetDlgItem(IDC_ATT_SOS_PICTURE);
	if (pWnd) pWnd->ShowWindow((pCurrAttInfo->attachmentInfo->SOSDocumentStatus != StatoDocumento::EMPTY) ? SW_SHOW : SW_HIDE);

	if (statoDoc != StatoDocumento::EMPTY)
	{
		m_DocStatusDescription = Utils::GetDocumentStatusDescription(statoDoc);
		m_DocStatusImage = GetDocStatusImage(statoDoc);
	}

	CString strMsg;
	::CanBeSentToSOSType canBeSentType = GetDMSClientDoc()->CanBeSentToSOS(strMsg);

	if (statoDoc == StatoDocumento::EMPTY || (int)statoDoc < (int)StatoDocumento::SENT)
	{	
		pWnd = GetDlgItem(IDC_ATT_SEND_TO_SOS);
		pWnd->ShowWindow((statoDoc == StatoDocumento::EMPTY) ? SW_SHOW : SW_HIDE);

		if (canBeSentType != ::CanBeSentToSOSType::BeSent)
		{
			pWnd = GetDlgItem(IDC_ATT_SOS_INFO);
			if (pWnd) pWnd->ShowWindow(SW_SHOW);
			m_Info = strMsg;
		}
		else
		{
			pWnd = GetDlgItem(IDC_ATT_SOS_INFO);
			if (pWnd) pWnd->ShowWindow(SW_HIDE);
			m_Info.Clear();
		}

		pCurrAttInfo->attachmentInfo->CreateSOSBookmark = (canBeSentType != ::CanBeSentToSOSType::NoPDFA);
		m_bEnableAttachForSOS = (DataBool)pCurrAttInfo->attachmentInfo->CreateSOSBookmark;

		pWnd = GetDlgItem(IDC_ATT_SOS_STATIC_CODE);
		if (pWnd) pWnd->ShowWindow(SW_HIDE);
		pWnd = GetDlgItem(IDC_ATT_SOS_CODE);
		if (pWnd) pWnd->ShowWindow(SW_HIDE);
		pWnd = GetDlgItem(IDC_ATT_SOS_STATIC_LOTID);
		if (pWnd) pWnd->ShowWindow(SW_HIDE);
		pWnd = GetDlgItem(IDC_ATT_SOS_LOTID);
		if (pWnd) pWnd->ShowWindow(SW_HIDE);
		pWnd = GetDlgItem(IDC_ATT_SOS_STATIC_REGDATE);
		if (pWnd) pWnd->ShowWindow(SW_HIDE);
		pWnd = GetDlgItem(IDC_ATT_SOS_REGDATE);
		if (pWnd) pWnd->ShowWindow(SW_HIDE);
	}
	else
	{
		pWnd = GetDlgItem(IDC_ATT_SEND_TO_SOS);
		if (pWnd) pWnd->ShowWindow(SW_HIDE);
		// Absolute Code
		pWnd = GetDlgItem(IDC_ATT_SOS_STATIC_CODE);
		if (pWnd) pWnd->ShowWindow(SW_SHOW);
		pWnd = GetDlgItem(IDC_ATT_SOS_CODE);
		if (pWnd) pWnd->ShowWindow(SW_SHOW);
		m_AbsoluteCode = String::IsNullOrWhiteSpace((String^)(pCurrAttInfo->attachmentInfo->SOSAbsoluteCode)) 
						? _TB("<not available>") : pCurrAttInfo->attachmentInfo->SOSAbsoluteCode->ToString();
		// LotID
		pWnd = GetDlgItem(IDC_ATT_SOS_STATIC_LOTID);
		if (pWnd) pWnd->ShowWindow(SW_SHOW);
		pWnd = GetDlgItem(IDC_ATT_SOS_LOTID);
		if (pWnd) pWnd->ShowWindow(SW_SHOW);
		m_LotID = String::IsNullOrWhiteSpace((String^)(pCurrAttInfo->attachmentInfo->SOSLotID)) 
				? _TB("<not available>") : pCurrAttInfo->attachmentInfo->SOSLotID->ToString();
		// RegistrationDate
		pWnd = GetDlgItem(IDC_ATT_SOS_STATIC_REGDATE);
		if (pWnd) pWnd->ShowWindow(SW_SHOW);
		pWnd = GetDlgItem(IDC_ATT_SOS_REGDATE);
		if (pWnd) pWnd->ShowWindow(SW_SHOW);
		m_RegistrationDate = (pCurrAttInfo->attachmentInfo->SOSRegistrationDate == (DateTime)System::Data::SqlTypes::SqlDateTime::MinValue)
							? _TB("<not available>") : ((DateTime)(pCurrAttInfo->attachmentInfo->SOSRegistrationDate)).ToShortDateString();
	}
}

//-----------------------------------------------------------------------------
void CAttachmentSOSTileDlg::HideSOSControls()
{
	CWnd* pWnd = GetDlgItem(IDC_ATT_SOS_DISPATCH_STATUS);
	if (pWnd) pWnd->ShowWindow(SW_HIDE);
	pWnd = GetDlgItem(IDC_ATT_SOS_PICTURE);
	if (pWnd) pWnd->ShowWindow(SW_HIDE);
	pWnd = GetDlgItem(IDC_ATT_SOS_STATIC_CODE);
	if (pWnd) pWnd->ShowWindow(SW_HIDE);
	pWnd = GetDlgItem(IDC_ATT_SOS_CODE);
	if (pWnd) pWnd->ShowWindow(SW_HIDE);
	pWnd = GetDlgItem(IDC_ATT_SOS_STATIC_LOTID);
	if (pWnd) pWnd->ShowWindow(SW_HIDE);
	pWnd = GetDlgItem(IDC_ATT_SOS_LOTID);
	if (pWnd) pWnd->ShowWindow(SW_HIDE);
	pWnd = GetDlgItem(IDC_ATT_SOS_STATIC_REGDATE);
	if (pWnd) pWnd->ShowWindow(SW_HIDE);
	pWnd = GetDlgItem(IDC_ATT_SOS_REGDATE);
	if (pWnd) pWnd->ShowWindow(SW_HIDE);
	pWnd = GetDlgItem(IDC_ATT_SEND_TO_SOS);
	if (pWnd) pWnd->ShowWindow(SW_HIDE);
	pWnd = GetDlgItem(IDC_ATT_SOS_INFO);
	if (pWnd) pWnd->ShowWindow(SW_HIDE);
}

// evento intercettato sul click del pulsante di abilitazione all'invio in SOS 
// per gli allegati che sono privi dei bookmark necessari per la SOS
//-----------------------------------------------------------------------------
void CAttachmentSOSTileDlg::OnSendSOSClicked()
{
	GetDMSClientDoc()->CreateNewSosDocument();
	// ridisegno i controls
	EnableTileDialogControlLinks();
	OnUpdateControls();
}

//-----------------------------------------------------------------------------
CString CAttachmentSOSTileDlg::GetDocStatusImage(StatoDocumento statoDoc)
{
	switch (statoDoc)
	{
	case StatoDocumento::SENT:
		return ExtensionsGlyph(szGlyphSOSDocSent);
		break;
	case StatoDocumento::TORESEND:
		return ExtensionsGlyph(szGlyphSOSDocToResend);
		break;
	case StatoDocumento::DOCTEMP:
		return ExtensionsGlyph(szGlyphSOSDocTemp);
		break;
	case StatoDocumento::DOCSTD:
		return ExtensionsGlyph(szGlyphSOSDocStd);
		break;
	case StatoDocumento::DOCRDY:
		return ExtensionsGlyph(szGlyphSOSDocRdy);
		break;
	case StatoDocumento::DOCSIGN:
		return ExtensionsGlyph(szGlyphSOSDocSign);
		break;
	case StatoDocumento::DOCKO:
		return ExtensionsGlyph(szGlyphSOSDocKO);
		break;
	case StatoDocumento::TOSEND:
		return ExtensionsGlyph(szGlyphSOSDocToSend);
		break;
	case StatoDocumento::IDLE:
	case StatoDocumento::WAITING:
	default:
		return ExtensionsGlyph(szGlyphSOSDocIdle);
		break;
	//case StatoDocumento::DOCREP:
	}
}

