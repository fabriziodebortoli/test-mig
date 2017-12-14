
#pragma once

#include <TbGenlib\OslInfo.h>
#include <TbGenlib\TBToolBar.h>
#include <TbNameSolver\TbNamespaces.h>
#include <TbGeneric\WndObjDescription.h>
#include <TbGenlib\ParsCbx.h>
#include "beginh.dex"

class CBaseDocument;
class CMasterToolCbx;

#define RIBBONBAR_SMALL_ICO				16
#define RIBBONBAR_LARGE_ICO				24

class CTBRibbonButtonsGroup;
class CTBRibbonCheckBox;

//=============================================================================
class CTBRibbonButton : public CBCGPRibbonButton
{
	public:
		CTBRibbonButton();
		CTBRibbonButton (UINT nID, LPCTSTR lpszText, HICON	hIcon, BOOL bAlwaysShowDescription = FALSE, HICON	hIconSmall = NULL,
						BOOL	bAutoDestroyIcon = FALSE, BOOL	bAlphaBlendIcon = FALSE);
		virtual ~CTBRibbonButton();
		virtual void DrawImage (CDC* pDC, RibbonImageType type, CRect rectImage);

	protected:
		virtual void OnUpdateCmdUI (CBCGPRibbonCmdUI* pCmdUI, CFrameWnd* pTarget, BOOL bDisableIfNoHndler);
		HICON CreateGrayscaleIcon( HICON hIcon);
};

//=============================================================================
class TB_EXPORT CTBRibbonPanel : public CBCGPRibbonPanel, public IOSLObjectManager 
{
	DECLARE_DYNCREATE(CTBRibbonPanel)

	public:
		CTBRibbonPanel(BOOL isNew = TRUE);
		virtual ~CTBRibbonPanel();
		
	public:
		CString GetName();
		const CBCGPToolBarImages*	GetIconsImageList();
		void  SetIconsImageList(CBCGPToolBarImages* pIamgeList);

		CTBRibbonButton* AddButton			(
									UINT nCommandID, 
									UINT nIDImageLarge, 
									UINT nIDImageSmall = 0, 
									const CString& aLibNamespace = _T(""), 
									const CString& sName = _T(""), 
									const CString& szText = _T(""), 
									BOOL bPNG = TRUE 
								);

		CTBRibbonButton* AddButton(UINT nCommandID, const CString& sButtonNameSpace, const CString& sImageNameSpace, const CString& szText = _T(""));

		BOOL AddEdit			(
									UINT nID, 
									const CString& sName, 
									LPCTSTR lpszLabel = _T(""),  
									int	nIDimage = -1, 
									CString aLibNamespace = _T(""),
									int nWidth = 150, 
									BOOL bPNG = TRUE
								);
		BOOL AddLaunchButton	(UINT nCommandID, UINT nIdImg = 0, BOOL bPNG = TRUE);
		BOOL AddSeparator		(BOOL bIsHoriz = FALSE);
		BOOL AddLabel			(UINT nID, const CString& sName, LPCTSTR pText, BOOL bIsMultiLine);
		CTBRibbonCheckBox* AddCheckBox	(UINT nID, const CString& sName, const CString& text);

		CTBRibbonButtonsGroup* AddGroup		(const CString& sName);

		void AttachOSLInfo		(CInfoOSL* pParent, const CString& sName);
		
	public:
		CArray<CInfoOSLButton*,CInfoOSLButton*> m_arButtonOSLInfo;
		CArray<CTBRibbonButtonsGroup*, CTBRibbonButtonsGroup*> m_arButonsGroup;

	private:
		CDC*		GetDC();
		void		ReleaseDC(CDC* pDC);

	private:
		void AddOslInfo	(UINT nCommandID, const CString& sName);

	protected:
		virtual void OnUpdateCmdUI (CBCGPRibbonCmdUI* pCmdUI, CFrameWnd* pTarget, BOOL bDisableIfNoHndler);
};

//=============================================================================
class TB_EXPORT CTBRibbonButtonsGroup : public CBCGPRibbonButtonsGroup, public IOSLObjectManager 
{
	public:
		CTBRibbonButtonsGroup();
		virtual ~CTBRibbonButtonsGroup();

	public:
		CString GetName();
		virtual void CopyFrom (const CTBRibbonButtonsGroup& src);

		CTBRibbonButton* AddButton (
							UINT nCommandID, 
							UINT nIDImageLarge, 
							UINT nIDImageSmall = 0, 
							const CString& aLibNamespace = _T(""), 
							const CString& sName = _T(""), 
							const CString& szText = _T(""), 
							BOOL bPNG = TRUE 
						);

		CTBRibbonButton* AddButton(UINT nCommandID, const CString& sButtonNameSpace, const CString& sImageNameSpace, const CString& szText = _T(""));

		BOOL AddSeparator		(BOOL bIsHoriz = TRUE);
		BOOL AddLabel			(UINT nID, const CString& sName, LPCTSTR pText, BOOL bIsMultiLine);
		BOOL AddEdit			(UINT nID, const CString& sName, LPCTSTR lpszLabel = _T(""),  int	nIDimage = -1, CString aLibNamespace = _T(""), int nWidth = 150, BOOL bPNG = TRUE);
		BOOL AddCheckBox		(UINT nID, const CString& sName, const CString& text);
		int	 AddButtonsGroupIcon(UINT nIdImg, const CString& aLibNamespace, BOOL bPNG = TRUE);

		void AttachOSLInfo		(CInfoOSL* pParent, const CString& sName);	
	public:
		CArray<CInfoOSLButton*,CInfoOSLButton*> m_arButtonOSLInfo;

	private:
		void AddOslInfo		(UINT nCommandID, const CString& sName);
};

class CTBRibbonBar;
//=============================================================================
class TB_EXPORT CTBRibbonCategory : public CBCGPRibbonCategory, public IOSLObjectManager 
{
	DECLARE_DYNCREATE(CTBRibbonCategory)

	public:
		CTBRibbonCategory();
		virtual ~CTBRibbonCategory();
		
	public:
		CString GetName();

		CTBRibbonPanel*		AddPanel	(LPCTSTR lpszName, const CString& sText, HICON pIco = NULL);
		CTBRibbonPanel*		GetPanel	(const CString& sName);
		CTBRibbonPanel*		GetPanel	(int nIndex);
		CTBRibbonCategory*	CreateCopy	(CTBRibbonBar* pParent);

		void AttachOSLInfo	(CInfoOSL* pParent, const CString& sName);
		
};

//=============================================================================
class TB_EXPORT CTBRibbonCheckBox : public CBCGPRibbonCheckBox
{
	public:
		CTBRibbonCheckBox();
		CTBRibbonCheckBox (UINT	nID, LPCTSTR lpszText);
		virtual ~CTBRibbonCheckBox();

		void SetCheckBox(BOOL bCheckBox = TRUE);
		BOOL GetCheckBox();

	public:
		void OnClick (CPoint point);
};

//=============================================================================
class TB_EXPORT CTBRibbonBar : public CBCGPRibbonBar, public IOSLObjectManager 
{
	public:
		CTBRibbonBar();
		virtual ~CTBRibbonBar();

	public:
		CString GetName();

		virtual BOOL	Create			(CWnd* pParentWnd, CString sName, DWORD dwStyle = (WS_CHILD|WS_VISIBLE|CBRS_TOP));
		void			SetCheckBox		(UINT nID, BOOL bCheckBox = TRUE);
		BOOL			GetCheckBox		(UINT nID);

		void			SetTextContent(UINT nID, const CString& text);
		CString			GetTextContent(UINT nID);

		CTBRibbonPanel*		AddPanel (CTBRibbonCategory* cat, LPCTSTR lpszName,  const CString& sText, UINT nIdImg = 0, BOOL bPNG = TRUE);
		CTBRibbonPanel*		GetPanel(const CString& sName);
		CTBRibbonPanel*		FindOrAddPanel(const CString& sName, const CString& sText = _T(""));
		
		CTBRibbonCategory* GetCategory(const CString& sName);
		CTBRibbonCategory* GetCategory (int nIndex) const;

		CTBRibbonCategory*	AddCategory (LPCTSTR	lpszName, const CString& sText, UINT nSmallImageID = 0, UINT nLargeImageID = 0);
		
		void AttachOSLInfo		(CInfoOSL* pParent);
		
		BOOL HideButton			(UINT nID, BOOL bHide = TRUE);

	protected:
		virtual CSize CalcFixedLayout(BOOL bStretch, BOOL bHorz);
		
	private:
		CString		m_sName;
		CList<UINT, UINT> lstHiddenButtons; // list of hidden button
};

#include "endh.dex"