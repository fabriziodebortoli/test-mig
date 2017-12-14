#pragma once

#include <TbGenlib\BaseTileDialog.h>
#include <TbGenlib\PARSEDT.H>
#include "ExtDocAbstract.h"
#include <TbGes\TBGridControl.h>

#include "beginh.dex"

class CParsedPanel;
class CLabelStatic;
class CGroupBoxBtn;
class CTBHotlinkControl;
class CTBPropertyGrid;
class CTileManager;
class CabManager;

//===========================================================================
class TB_EXPORT CTileDialog : public CBaseTileDialog
{
	DECLARE_DYNAMIC(CTileDialog)

protected:
	Array			m_DataToCtrlMap;	
	CTabManager*	m_pTabManager;

	void SetChildControlNamespace (const CString& sName, CParsedCtrl*);

public:
	CTileDialog();
	CTileDialog(const CString& sName, int nIDD, CWnd* pParent = NULL);
	virtual ~CTileDialog();

public:
	// Accessibility - Method used to uniquely identify an object by Ranorex Spy
	virtual HRESULT get_accName(VARIANT varChild, BSTR *pszName);

public:
	CAbstractFormDoc*	GetDocument() const	{ return (CAbstractFormDoc*)__super::GetDocument(); }
	
	virtual void EnableTileDialogControls();
	
	void				Register				(CBodyEdit* pBody);
	virtual CParsedCtrl*	AddLink
		(
			UINT			nIDC, 
			const CString&	sName,
			SqlRecord*		pRecord, 
			DataObj*		pDataObj, 
			CRuntimeClass*	pParsedCtrlClass,

			HotKeyLink*		pHotKeyLink			= NULL,
			UINT			nBtnID				= BTN_DEFAULT
		);

	virtual CParsedCtrl*	AddLink
		(
			UINT			nIDC, 
			const CString&	sName,
			SqlRecord*		pRecord, 
			DataObj*		pDataObj, 
			CRuntimeClass*	pParsedCtrlClass,

			CString			sNsHotKeyLink,
			UINT			nBtnID				= BTN_DEFAULT
		);

	virtual CExtButton*		AddLink
		(
			UINT			nIDC, 
			const CString&	sName,
			SqlRecord*		pRecord	= NULL, 
			DataObj*		pDataObj = NULL
		);

	virtual CBodyEdit*		AddLink
		(
			UINT				nIDC, 
			DBTSlaveBuffered*	pDBT, 
			CRuntimeClass*		pBodyEditClass,
			CRuntimeClass*		pRowFormViewClass = NULL,
			CString				strTitle = _T(""),
			CString				sName = _T("")
		);

	virtual  CTBGridControl* AddLinkGrid
	(
		UINT				nIDC, 
		DBTSlaveBuffered*	pDBT, 
		CRuntimeClass*		pGridControlClass,
		CString				sName
	);
	
	virtual  CTBGridControl* AddLinkGrid
	(
		UINT				nIDC, 
		RecordArray*		pRA, 
		CRuntimeClass*		pGridControlClass,
		CString				sName
	);
	
	virtual CWnd* AddLink
		(
			UINT			nIDC, 
			const CString&	sName,
			CRuntimeClass*  prtCtrl
		);

	virtual CParsedPanel* AddLink
		(
					UINT			nIDC, 
					CRuntimeClass*	pParsedPanelClass, 
					CObject*		pPanelOwner,
			const	CString&		sName, 
			const	CString&		sCaption = _T(""), 
					BOOL			bCallOnInitialUpdate = TRUE
		);

	CTBPropertyGrid*	AddLinkPropertyGrid
		(
		UINT				nIDC,
		CString				sName,
		CRuntimeClass*		pRuntimeClass = NULL
		);

	virtual CParsedCtrl* AddLinkAndCreateControl
		(
		const CString&	sName,
		DWORD			dwStyle,
		const CRect&	rect,
		UINT			nIDC,
		SqlRecord*		pRecord,
		DataObj*		pDataObj,
		CRuntimeClass*	pParsedCtrlClass,
		HotKeyLink*		pHotKeyLink = NULL,
		UINT			nBtnID = BTN_DEFAULT
		);

	virtual CBodyEdit* AddLinkAndCreateBodyEdit
		(
		CRect				rect,
		UINT				nIDC,
		DBTSlaveBuffered*	pDBT,
		CRuntimeClass*		pBodyEditClass,
		CRuntimeClass*		pRowFormViewClass = NULL,
		CString				strRowFormViewTitle = _T(""),
		CString				sBodyName = _T(""),
		CString				sRowViewName = _T("")
		);

	CLabelStatic* AddLabelLink (UINT nIDC);
	CLabelStatic* AddLabelLinkWithLine(UINT nIDC, int nSizePen = 1, int pos = CLabelStatic::LP_TOP);
	CLabelStatic* AddSeparatorLink (UINT nIDC, COLORREF crBorder, int nSizePen = 1, BOOL  bVertical = FALSE, CLabelStatic::ELinePos pos = CLabelStatic::LP_VCENTER);
	CGroupBoxBtn* AddGroupBoxLink (UINT nIDC);

	virtual CTabManager*	AddTabManager	(UINT nIDC, CRuntimeClass* pClass, const CString& sName, BOOL bCallOnInitialUpdate = TRUE);
	CTileManager*			AddTileManager(UINT nIDC, CRuntimeClass* pClass, const CString& sName, BOOL bCallOnInitialUpdate = TRUE);

	virtual	void	EnableTileDialogControlLinks	(BOOL bEnable = TRUE, BOOL bMustSetOSLReadOnly = FALSE);
	virtual BOOL	PrepareAuxData					();
	virtual BOOL	OnPrepareAuxData				()	{ return TRUE; }
	virtual	void	OnUpdateControls				(BOOL bParentIsVisible = TRUE);
	virtual	void	OnUpdateTitle();				//	{ /*override to keep the collapsed title updated after OnChanged */}
	virtual	void	OnResetDataObjs					();
	virtual BOOL	PreTranslateMessage				(MSG* pMsg);
			void	OnFindHotLinks					();

	CParsedCtrl*	GetLinkedParsedCtrl			(DataObj* pDataObj);
	CParsedCtrl*	GetLinkedParsedCtrl			(const CTBNamespace& aNS);
	CParsedCtrl*	GetLinkedParsedCtrl			(UINT nIDC);

	CBodyEdit*		GetBodyEdits				(const CTBNamespace& aNS);
	CBodyEdit*		GetBodyEdits				(int* pnStartIdx);
	
	CAbstractFormView*	GetFormView					(CRuntimeClass* pClass = NULL);

	virtual BOOL	OnInitDialog			();
	virtual	BOOL	OnCommand				(WPARAM wParam, LPARAM lParam);

	virtual	void	BuildDataControlLinks	();
	virtual void	OnBuildDataControlLinks	();
	virtual void	OnPinUnpin				();
	virtual void	DoCollapseExpand		();
	void RebuildLinks			(SqlRecord*);

	virtual void	OnDisableControlsForBatch	() {/* do nothing*/}
	virtual void	OnDisableControlsForAddNew	() {/* do nothing*/}
	virtual void	OnDisableControlsForEdit	() {/* do nothing*/}
	virtual void	OnEnableControlsForFind		() {/* do nothing*/}
	virtual void	OnDisableControlsAlways		() {/* do nothing*/}

protected:
	void			EnableTabManagerControlLinks(BOOL bEnable);

	virtual			void Relayout(CRect &rectNew, HDWP hDWP = NULL);
	virtual int		GetMinHeight(CRect& rect = CRect(0, 0, 0, 0));

private:
	CRowFormView*	GetParentRowView();
	CParsedPanel*	GetParsedPanelContainer(CWnd* pWnd);
	
};


/////////////////////////////////////////////////////////////////////////////
//		CEmptyTileDialog
/////////////////////////////////////////////////////////////////////////////
//===========================================================================
class TB_EXPORT CEmptyTileDialog : public CTileDialog
{
	DECLARE_DYNCREATE(CEmptyTileDialog)
public:
	CEmptyTileDialog();
	~CEmptyTileDialog();
};


/////////////////////////////////////////////////////////////////////////////
//		Class CBatchHeaderTileDlg Declaration
/////////////////////////////////////////////////////////////////////////////
class TB_EXPORT CBatchHeaderTileDlg : public CTileDialog
{
	DECLARE_DYNCREATE(CBatchHeaderTileDlg)

public:
	UINT			m_nIDCTitle;
	UINT			m_nIDCSubTitle;

protected:
	CLabelStatic*	m_pTitle;
	CLabelStatic*	m_pSubTitle;

public:
	CBatchHeaderTileDlg();
	CBatchHeaderTileDlg(const CString& sTitle, UINT nIDD, UINT nIDCTitle = 0, UINT nIDCSubTitle = 0);

public:
	void			SetTextTitle(DataStr aStrTitle);
	void			SetTextSubTitle(DataStr aStrSubTitle, BOOL bBoldTitle = TRUE);
	CLabelStatic*	GetTitleLabelStatic() { return m_pTitle; }
	CLabelStatic*	GetSubTitleLabelStatic() { return m_pSubTitle; }

protected:
	virtual void	BuildDataControlLinks();
};

/////////////////////////////////////////////////////////////////////////////
//					CServicesHeaderTileDlg
/////////////////////////////////////////////////////////////////////////////
//
class TB_EXPORT CServicesHeaderTileDlg : public CTileDialog
{
	DECLARE_DYNCREATE(CServicesHeaderTileDlg)

public:
	// Construction
	CServicesHeaderTileDlg();

protected:
	virtual void BuildDataControlLinks();
};