
#pragma once

#include <TbGes\TileManager.h>
#include <TbGes\TileDialog.h>
#include <TbGes\BODYEDIT.H>

#include "beginh.dex"

class CAttachmentsArray;
class DMSAttachmentInfo;
class CDDMS;
class CAttachmentPaneView;
class DMSAttachmentsList;
class VBookmark;
class CAttachmentBookmarksBodyEdit;

//////////////////////////////////////////////////////////////////////////////
//						    CAttachmentsListBox
//			contiene il nome del file e il suo AttachmentID
//////////////////////////////////////////////////////////////////////////////
//=============================================================================
class TB_EXPORT CAttachmentsListBox : public CLongListBox, public ResizableCtrl
{
	DECLARE_DYNCREATE(CAttachmentsListBox)

public:
	CAttachmentsListBox();
	~CAttachmentsListBox();

private:
	CImageList*					m_pImageList;
	CParsedCtrlDropFilesTarget* m_pParsedCtrlTarget;
	DMSAttachmentsList*			m_pAttachments;

private:
	void BuildImageList();
	int GetImageIdx(DataStr sExtensionType);

public:
	virtual BOOL OnInitCtrl		();
	virtual void OnFillListBox	();

	virtual void OnDropFiles		(CStringArray* pDroppedFiles);
	virtual BOOL OnSubFolderFound	();

			void  InitSizeInfo		() { ResizableCtrl::InitSizeInfo(this); }

public:
	void SetAttachmentsArray(DMSAttachmentsList*);

protected:
	afx_msg void	OnContextMenu	(CWnd* pWnd, CPoint ptMousePos);
	afx_msg	LRESULT	OnRecalcCtrlSize(WPARAM, LPARAM);

	DECLARE_MESSAGE_MAP()
};

//////////////////////////////////////////////////////////////////////////////
//						    CAttachmentPane
//				docking pane per la gestione degli allegati
//////////////////////////////////////////////////////////////////////////////
//=============================================================================
class CAttachmentPane : public CTaskBuilderDockPane
{
	DECLARE_DYNCREATE(CAttachmentPane)
	
public:
	CDDMS* m_pDMSClientDoc;

public:
	CAttachmentPane();
	CAttachmentPane(CDDMS* pClientDoc);

protected:
	virtual BOOL CanFloat	() const { return FALSE; }
	virtual BOOL CanBeClosed() const { return FALSE; }
	virtual void OnSlide	(BOOL bSlideOut);

public:
	CAttachmentPaneView* GetAttachmentPaneView() const;

	void SetDMSClientDoc		(CDDMS* pCDDMS);
	void OnNewAttachCompleted	();
	void OnAfterLoadAttachments	();
	void OnUpdateControls();	//effettua il refresh dei control

protected:
	//{{AFX_MSG(CAttachmentPane)
	afx_msg BOOL OnHelpInfo(HELPINFO* pHelpInfo);
	//}}AFX_MSG

	DECLARE_MESSAGE_MAP()
};

//////////////////////////////////////////////////////////////////////////////
//						    CAttachmentPaneView
//////////////////////////////////////////////////////////////////////////////
//=============================================================================
class CAttachmentPaneView : public CParsedDialogWithTiles
{
	DECLARE_DYNCREATE(CAttachmentPaneView)

private:
	CDDMS*	m_pDMSClientDoc;

public:
	UINT m_IIDTGDetails;
	UINT m_IIDTGPreview;
	UINT m_IIDTGBookmarks;
	UINT m_IIDTGBarcode;
	UINT m_IIDTGSos;

public:
	CAttachmentPaneView();

private:
	void EnableControlLinks();

public:
	CDDMS*	GetDMSClientDoc	() const;

	void	OnNewAttachCompleted	();
	void	OnAfterLoadAttachments	();

public:
	virtual	BOOL OnInitDialog		();
	virtual void OnCustomizeToolbar	();
	virtual BOOL OnPopulatedDropDown(UINT id);

protected:
	//{{AFX_MSG(CAttachmentPaneView)
	afx_msg void OnAttachmentChanged			();
	afx_msg void OnBarcodeChanged				();

	afx_msg void OnNewAttachment				();
	afx_msg void OnSaveAttachment				();
	afx_msg void OnEditAttachment				();
	afx_msg void OnUndoChangesAttachment		();	
	afx_msg void OnDeleteAttachment				();
	afx_msg void OnViewAttachment				();
	afx_msg void OnCopyAttachment				();
	afx_msg void OnSendAttachment				();
	afx_msg void OnReloadAttachment				();
	afx_msg void OnCheckOutAttachment			();
	afx_msg void OnCheckInAttachment			();

	afx_msg void OnUpdateNewAttachment			(CCmdUI*);
	afx_msg void OnUpdateSaveAttachment			(CCmdUI*);
	afx_msg void OnUpdateEditAttachment			(CCmdUI*);
	afx_msg void OnUpdateUndoChangesAttachment	(CCmdUI*);
	afx_msg void OnUpdateDeleteAttachment		(CCmdUI*);
	afx_msg void OnUpdateViewAttachment			(CCmdUI*);
	afx_msg void OnUpdateCopyAttachment			(CCmdUI*); 
	afx_msg void OnUpdateSendAttachment			(CCmdUI*);
	afx_msg void OnUpdateCheckOutAttachment		(CCmdUI*);
	afx_msg void OnUpdateCheckInAttachment		(CCmdUI*);
	afx_msg void OnUpdateReloadAttachments		(CCmdUI*);
	//}}AFX_MSG
	DECLARE_MESSAGE_MAP()
};

//////////////////////////////////////////////////////////////////////////////
//					    CAttachmentBaseTileDlg
//			TileDialog di base da cui derivare tutte le altre
//////////////////////////////////////////////////////////////////////////////
//=============================================================================
class CAttachmentBaseTileDlg : public CTileDialog
{
	DECLARE_DYNCREATE(CAttachmentBaseTileDlg)

public:
	CAttachmentBaseTileDlg();
	CAttachmentBaseTileDlg(const CString& sName, int nIDD, CWnd* pParent = NULL);

public:
	CDDMS* GetDMSClientDoc() const;

	virtual void EnableTileDialogControls();
	virtual void EnableTileDialogControlLinks(BOOL bEnable = TRUE, BOOL bMustSetOSLReadOnly = FALSE);

	virtual void OnAfterLoadAttachments() {}
	virtual void OnNewAttachCompleted() {}
};

//////////////////////////////////////////////////////////////////////////////
//					    CAttachmentTileMng
//////////////////////////////////////////////////////////////////////////////
//=============================================================================
class CAttachmentTileMng : public CTileManager
{
	DECLARE_DYNCREATE(CAttachmentTileMng)

public:
	CAttachmentTileMng();

protected:
	virtual void Customize();
};

//////////////////////////////////////////////////////////////////////////////
//					    CAttachmentHeadTileGrp
//////////////////////////////////////////////////////////////////////////////
//=============================================================================
class TB_EXPORT CAttachmentHeadTileGrp : public CTileGroup
{
	DECLARE_DYNCREATE(CAttachmentHeadTileGrp)

public:
	void OnAfterLoadAttachments();
	void OnNewAttachCompleted();

protected:
	virtual void Customize();
};

//////////////////////////////////////////////////////////////////////////////
//					    CAttachmentBodyTileDlg
//////////////////////////////////////////////////////////////////////////////
//=============================================================================
class TB_EXPORT CAttachmentBodyTileDlg : public CAttachmentBaseTileDlg
{
	DECLARE_DYNCREATE(CAttachmentBodyTileDlg)

public:
	CAttachmentBodyTileDlg();
	~CAttachmentBodyTileDlg();

protected:
	virtual void BuildDataControlLinks();
	virtual void EnableTileDialogControlLinks(BOOL bEnable = TRUE, BOOL bMustSetOSLReadOnly = FALSE);
};

//////////////////////////////////////////////////////////////////////////////
//					    CAttachmentHeadTileDlg
//////////////////////////////////////////////////////////////////////////////
//=============================================================================
class CAttachmentHeadTileDlg : public CAttachmentBaseTileDlg
{
	DECLARE_DYNCREATE(CAttachmentHeadTileDlg)

public:
	CAttachmentHeadTileDlg();

private:
	CAttachmentsListBox*	m_pAttListBox;
	DataLng					m_Attachment;

public:
	virtual	void BuildDataControlLinks			(); 
	virtual void OnAfterLoadAttachments			();
	virtual void OnNewAttachCompleted			();
	virtual void EnableTileDialogControlLinks	(BOOL bEnable = TRUE, BOOL bMustSetOSLReadOnly = FALSE);

	void AttachFiles(CStringArray* pDroppedFiles);

protected:
	//{{AFX_MSG(CAttachmentHeadTileDlg)	
	afx_msg void OnAttachmentsListDblClick	();
	//}}AFX_MSG

	DECLARE_MESSAGE_MAP()
};

//////////////////////////////////////////////////////////////////////////////
//					    CAttachmentDetailsTileGrp
//////////////////////////////////////////////////////////////////////////////
//=============================================================================
class TB_EXPORT CAttachmentDetailsTileGrp : public CTileGroup
{
	DECLARE_DYNCREATE(CAttachmentDetailsTileGrp)

protected:
	virtual void Customize();
};

//////////////////////////////////////////////////////////////////////////////
//					    CAttachmentDetailsTileDlg
//////////////////////////////////////////////////////////////////////////////
//=============================================================================
class CAttachmentDetailsTileDlg : public CAttachmentBaseTileDlg
{
	DECLARE_DYNCREATE(CAttachmentDetailsTileDlg)

public:
	CAttachmentDetailsTileDlg();

public:
	virtual	void BuildDataControlLinks();
};

//////////////////////////////////////////////////////////////////////////////
//					    CAttachmentPreviewTileGrp
//////////////////////////////////////////////////////////////////////////////
//=============================================================================
class TB_EXPORT CAttachmentPreviewTileGrp : public CTileGroup
{
	DECLARE_DYNCREATE(CAttachmentPreviewTileGrp)

protected:
	virtual void Customize();
};

//////////////////////////////////////////////////////////////////////////////
//					    CAttachmentPreviewTileDlg
//////////////////////////////////////////////////////////////////////////////
//=============================================================================
class CAttachmentPreviewTileDlg : public CAttachmentBaseTileDlg
{
	DECLARE_DYNCREATE(CAttachmentPreviewTileDlg)

public:
	CAttachmentPreviewTileDlg();

public:		
	virtual	void BuildDataControlLinks			();
	virtual BOOL OnPrepareAuxData				();
	virtual void EnableTileDialogControlLinks	(BOOL bEnable = TRUE, BOOL bMustSetOSLReadOnly = FALSE);
};

//////////////////////////////////////////////////////////////////////////////
//					    CAttachmentBookmarksTileGrp
//////////////////////////////////////////////////////////////////////////////
//=============================================================================
class TB_EXPORT CAttachmentBookmarksTileGrp : public CTileGroup
{
	DECLARE_DYNCREATE(CAttachmentBookmarksTileGrp)

protected:
	virtual void Customize();
};

//////////////////////////////////////////////////////////////////////////////
//					    CAttachmentBookmarksTileDlg
//////////////////////////////////////////////////////////////////////////////
//=============================================================================
class CAttachmentBookmarksTileDlg : public CAttachmentBaseTileDlg
{
	DECLARE_DYNCREATE(CAttachmentBookmarksTileDlg)

private:
	CAttachmentBookmarksBodyEdit* m_pBookmarkBE;

public:
	CAttachmentBookmarksTileDlg();

public:
	virtual	void BuildDataControlLinks			();
	virtual void EnableTileDialogControlLinks	(BOOL bEnable = TRUE, BOOL bMustSetOSLReadOnly = FALSE);

protected:
	//{{AFX_MSG(CAttachmentBookmarksTileDlg)
	afx_msg void OnBEBookmarkTypeChanged();
	afx_msg void OnBESearchFieldChanged	();
	afx_msg void OnBEBookmarkRowChanged	();
	//}}AFX_MSG

	DECLARE_MESSAGE_MAP()
};

//////////////////////////////////////////////////////////////////////////////
//					    CAttachmentBarcodeTileGrp
//////////////////////////////////////////////////////////////////////////////
//=============================================================================
class TB_EXPORT CAttachmentBarcodeTileGrp : public CTileGroup
{
	DECLARE_DYNCREATE(CAttachmentBarcodeTileGrp)

protected:
	virtual void Customize();
};

//////////////////////////////////////////////////////////////////////////////
//					    CAttachmentBarcodeTileDlg
//////////////////////////////////////////////////////////////////////////////
//=============================================================================
class CAttachmentBarcodeTileDlg : public CAttachmentBaseTileDlg
{
	DECLARE_DYNCREATE(CAttachmentBarcodeTileDlg)

public:
	DataBool m_bManualBarcodeDetection;
	DataStr m_StaticNotes;

public:
	CAttachmentBarcodeTileDlg();

public:
	virtual	void BuildDataControlLinks			();
	virtual void EnableTileDialogControlLinks	(BOOL bEnable = TRUE, BOOL bMustSetOSLReadOnly = FALSE);

protected:
	//{{AFX_MSG(CAttachmentBarcodeTileDlg)
	afx_msg void OnDetectBarcodeClicked	();
	//}}AFX_MSG

	DECLARE_MESSAGE_MAP()
};

//////////////////////////////////////////////////////////////////////////////
//					    CAttachmentSOSTileGrp
//////////////////////////////////////////////////////////////////////////////
//=============================================================================
class TB_EXPORT CAttachmentSOSTileGrp : public CTileGroup
{
	DECLARE_DYNCREATE(CAttachmentSOSTileGrp)

protected:
	virtual void Customize();
};

//////////////////////////////////////////////////////////////////////////////
//					    CAttachmentSOSTileDlg
//////////////////////////////////////////////////////////////////////////////
//=============================================================================
class CAttachmentSOSTileDlg : public CAttachmentBaseTileDlg
{
	DECLARE_DYNCREATE(CAttachmentSOSTileDlg)

public:
	DataStr		m_DocStatusDescription;
	DataStr		m_DocStatusImage;
	DataStr		m_AbsoluteCode;
	DataStr		m_LotID;
	DataStr		m_RegistrationDate;
	DataStr		m_Info;
	DataBool	m_bEnableAttachForSOS;

	DataStr		m_StaticAbsoluteCode;
	DataStr		m_StaticLotID;
	DataStr		m_StaticRegistrationDate;

public:
	CAttachmentSOSTileDlg();

public:
	virtual	void BuildDataControlLinks			();
	virtual void EnableTileDialogControlLinks	(BOOL bEnable = TRUE, BOOL bMustSetOSLReadOnly = FALSE);

private:
	CString GetDocStatusImage	(Microarea::TaskBuilderNet::Core::WebServicesWrapper::StatoDocumento statoDocumento);
	void	HideSOSControls		();

protected:
	//{{AFX_MSG(CAttachmentSOSTileDlg)
	afx_msg void OnSendSOSClicked();
	//}}AFX_MSG

	DECLARE_MESSAGE_MAP()
};

#include "endh.dex"
