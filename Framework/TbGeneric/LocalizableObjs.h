
#pragma once

#include <TBNameSolver\TBREsourceLocker.h>
#include <TBNameSolver\TBNamespaces.h>
#include <TbNameSolver\PathFinder.h>
#include <TbNameSolver\chars.h>
#include <TbNameSolver\FileSystemFunctions.h>

#include <TbGenlib\generic.hjson> //JSON AUTOMATIC UPDATE

#include "DockableFrame.h"
#include "GeneralFunctions.h"

#include "tbstrings.h"

//includere alla fine degli include del .H
#include "beginh.dex"

class CWndObjDescriptionContainer;
class CTabDescription;
class CWndObjDescription;
class CJsonContextObj;

//-----------------------------------------------------------------------------
LRESULT TB_EXPORT GetLocalizerInfo(WPARAM wParam, LPARAM lParam);
//-----------------------------------------------------------------------------
TB_EXPORT CWnd* GetSafeParent(CWnd* pParent);

//*****************************************************************************
//derivare i propri oggetti da queste classi se si desidera renderli localizzabili
//*****************************************************************************

//*****************************************************************************
// CLocalizableDialog dialog
//*****************************************************************************
class TB_EXPORT CLocalizableDialog : public CBCGPDialog
{ 
	DECLARE_DYNAMIC(CLocalizableDialog)
	DECLARE_MESSAGE_MAP()

public:
	CLocalizableDialog();							// default constructor

public:
	BOOL	m_bAutoDestroy;

protected:
	BOOL	m_bIsModal;
	BOOL	m_bInOpenDlgCounter; //serve per considerare o meno la dialog tra quelle aperte come popup. Il default è TRUE tranne
								// che per CBaseTabDialog
	int		m_nResult;

	HINSTANCE m_hResourceModule;
	_declspec(deprecated("Please use GetJsonContext() instead")) CJsonContextObj* m_pJsonContext = NULL;
public:
	explicit CLocalizableDialog (LPCTSTR lpszTemplateName, CWnd* pParentWnd = NULL);
	explicit CLocalizableDialog (UINT nIDTemplate, CWnd* pParentWnd = NULL);

	virtual ~CLocalizableDialog ();

	void	SetDialogAsPopup(BOOL bPopup) { m_bInOpenDlgCounter = bPopup;}

private:
			void	LockParentEnabilitation	(BOOL bLock);

public:
	// Accessibility - Method used to uniquely identify an object by Ranorex Spy
	virtual HRESULT get_accName(VARIANT varChild, BSTR *pszName);
	virtual CString GetRanorexNamespace();

public:
	virtual BOOL	OnInitDialog	();
	virtual void	OnCancel		();
	virtual void	OnOK			();
	virtual void	EndDialog		(int nResult);
	virtual BOOL	DestroyWindow	();
	
	afx_msg BOOL	OnToolTipText			(UINT, NMHDR* pNMHDR, LRESULT* pResult);
	afx_msg	LRESULT OnGetControlDescription	(WPARAM wParam, LPARAM lParam);
	afx_msg	LRESULT OnGetLocalizerInfo		(WPARAM wParam, LPARAM lParam);
	afx_msg	LRESULT HandleInitDialog		(WPARAM, LPARAM);
	afx_msg LRESULT OnGetComponent			(WPARAM wParam, LPARAM lParam);
	virtual INT_PTR DoModal				();
	virtual BOOL	Create				(UINT nIDTemplate, CWnd* pParentWnd = NULL);
	virtual BOOL	ForceFocusToParent	();
			BOOL	IsModal			() { return m_bIsModal; }
			int		GetResult		() { return m_nResult; }

			virtual void	PostNcDestroy();

	HINSTANCE		GetResourceModule();
	void			SetResourceModule(HINSTANCE hInstance) { m_hResourceModule = hInstance; }
	virtual CJsonContextObj* GetJsonContext();

protected:
	int OnCreate(LPCREATESTRUCT lpCreateStruct);
public:
	afx_msg void OnDestroy();
};


//*****************************************************************************
// CLocalizablePropertyPage dialog
//*****************************************************************************
class TB_EXPORT CLocalizablePropertySheet : public CPropertySheet
{
	DECLARE_DYNAMIC(CLocalizablePropertySheet)
	DECLARE_MESSAGE_MAP();

	BOOL	m_bIsModal;
	HINSTANCE m_hResourceModule;
// Construction
public:
	// simple construction
	CLocalizablePropertySheet();
	explicit CLocalizablePropertySheet(UINT nIDCaption, CWnd* pParentWnd = NULL, UINT iSelectPage = 0);
	explicit CLocalizablePropertySheet(LPCTSTR pszCaption, CWnd* pParentWnd = NULL, UINT iSelectPage = 0);
	// extended construction
	CLocalizablePropertySheet(UINT nIDCaption, CWnd* pParentWnd, 
		UINT iSelectPage, HBITMAP hbmWatermark,
		HPALETTE hpalWatermark = NULL, HBITMAP hbmHeader = NULL);
	CLocalizablePropertySheet(LPCTSTR pszCaption, CWnd* pParentWnd,
		UINT iSelectPage, HBITMAP hbmWatermark,
		HPALETTE hpalWatermark = NULL, HBITMAP hbmHeader = NULL);
	virtual BOOL OnInitDialog();
	virtual INT_PTR DoModal();
	virtual BOOL ForceFocusToParent();
	virtual BOOL DestroyWindow();
	virtual void BuildPropPageArray();

	afx_msg	LRESULT		OnGetControlDescription(WPARAM wParam, LPARAM lParam);
	afx_msg	LRESULT		OnActivateTabPage	(WPARAM, LPARAM);
	afx_msg LRESULT		OnGetComponent(WPARAM wParam, LPARAM lParam);
	
	HINSTANCE			GetResourceModule();
	void				SetResourceModule(HINSTANCE hInstance) { m_hResourceModule = hInstance; }
};

//*****************************************************************************
// CLocalizablePropertyPage dialog
//*****************************************************************************
class TB_EXPORT CLocalizablePropertyPage : public CPropertyPage
{
	DECLARE_DYNAMIC(CLocalizablePropertyPage)
	
	BOOL	m_bLocalized = false;
	HINSTANCE m_hResourceModule = NULL;
	DLGTEMPLATE* m_pJsonTemplate = NULL;
	_declspec(deprecated("Please use GetJsonContext() instead")) CJsonContextObj* m_pJsonContext = NULL;
public:
	CLocalizablePropertyPage();							// default constructor

	explicit CLocalizablePropertyPage(UINT nIDTemplate, UINT nIDCaption = 0, DWORD dwSize = sizeof(PROPSHEETPAGE));
	explicit CLocalizablePropertyPage(LPCTSTR lpszTemplateName, UINT nIDCaption = 0, DWORD dwSize = sizeof(PROPSHEETPAGE));
	CLocalizablePropertyPage(UINT nIDTemplate, UINT nIDCaption, 
		UINT nIDHeaderTitle, UINT nIDHeaderSubTitle = 0, DWORD dwSize = sizeof(PROPSHEETPAGE));
	CLocalizablePropertyPage(LPCTSTR lpszTemplateName, UINT nIDCaption, 
		UINT nIDHeaderTitle, UINT nIDHeaderSubTitle = 0, DWORD dwSize = sizeof(PROPSHEETPAGE));
	
	virtual ~CLocalizablePropertyPage();

	virtual BOOL OnInitDialog();
	CTabDescription* GetControlStructure(CWndObjDescriptionContainer* pContainer, bool bActive, HWND parentHandle);
	void PreProcessPageTemplate(PROPSHEETPAGE& psp, BOOL bWizard);
	void Localize();
	const CString& GetCaption()	{return m_strCaption;}
	virtual BOOL OnSetActive();
	virtual BOOL Create(UINT nIDTemplate, CWnd* pParentWnd = NULL);
	
	HINSTANCE GetResourceModule();
	void	  SetResourceModule(HINSTANCE hInstance) { m_hResourceModule = hInstance; }
private:
	CJsonContextObj* GetJsonContext();
	
};


//*****************************************************************************
// CLocalizableMenu 
//*****************************************************************************

class TB_EXPORT CLocalizableMenu : public CMenu
{
	DECLARE_DYNCREATE(CLocalizableMenu)

public:
	BOOL LoadMenu(LPCTSTR lpszResourceName);
	BOOL LoadMenu(UINT nIDResource);
};

//*****************************************************************************
// CLocalizableFrame frame
//*****************************************************************************
class TB_EXPORT CLocalizableFrame : public CDockableFrame
{
	DECLARE_DYNCREATE(CLocalizableFrame)

protected:
	CLocalizableFrame();           // protected constructor used by dynamic creation
	virtual ~CLocalizableFrame();

	afx_msg	BOOL OnToolTipText		(UINT nID, NMHDR* pNMHDR, LRESULT* pResult);

	virtual BOOL PreCreateWindow(CREATESTRUCT& cs);
	
public:
	virtual void GetMessageString		(UINT nID, CString& rMessage) const;
	virtual BOOL OnPopulatedDropDown    (UINT nIdCommand)						{ return FALSE; }
	
	virtual void SetToolBarActive		(CWnd* pWnd)							{}

public:
	// Accessibility - Method used to uniquely identify an object by Ranorex Spy
	virtual HRESULT get_accName(VARIANT varChild, BSTR *pszName);
	virtual CString GetRanorexNamespace();

public:
	static	void ExtractToolTipText		(const CString &strFullText, NMHDR* pNMHDR, LRESULT* pResult);
	
	
	DECLARE_MESSAGE_MAP()
};

//=============================================================================
class CCommonFunctions
{

public:

	//=============================================================================
	static void ExtractToolTipText (const CString &strFullText, NMHDR* lpnmhdr, LRESULT* pResult)
	{
		
		if (!strFullText.IsEmpty())
		{
			// need to handle both ANSI and UNICODE versions of the message
			TOOLTIPTEXTA* pTTTA = (TOOLTIPTEXTA*)lpnmhdr;
			TOOLTIPTEXTW* pTTTW = (TOOLTIPTEXTW*)lpnmhdr;

			CString strTipText;
			// this is the command id, not the button index
			AfxExtractSubString (strTipText, (LPCTSTR)strFullText, 1, LF_CHAR);
			
			#ifndef _UNICODE
				if (lpnmhdr->code == TTN_NEEDTEXTA)
					lstrcpyn(pTTTA->szText, strTipText,
						(sizeof(pTTTA->szText)/sizeof(pTTTA->szText[0])));
				else
					_mbstowcsz(pTTTW->szText, strTipText,
						(sizeof(pTTTW->szText)/sizeof(pTTTW->szText[0])));
			#else
				if (lpnmhdr->code == TTN_NEEDTEXTA)
					_wcstombsz(pTTTA->szText, strTipText,
						(sizeof(pTTTA->szText)/sizeof(pTTTA->szText[0])));
				else
					lstrcpyn(pTTTW->szText, strTipText,
						(sizeof(pTTTW->szText)/sizeof(pTTTW->szText[0])));
			#endif
				*pResult = 0;

			// bring the tooltip window above other popup windows
				CWnd* pWnd = CWnd::FromHandle(lpnmhdr->hwndFrom);
				if (pWnd)
					pWnd->SetWindowPos(&CWnd::wndTop, 0, 0, 0, 0, SWP_NOACTIVATE|SWP_NOSIZE|SWP_NOMOVE|SWP_NOOWNERZORDER);
		}
	}

	//=============================================================================
	static BOOL OnToolTipText(UINT, NMHDR* pNMHDR, LRESULT* pResult, HINSTANCE dllInstance)
	{
		ASSERT(pNMHDR->code == TTN_NEEDTEXTA || pNMHDR->code == TTN_NEEDTEXTW);

		// need to handle both ANSI and UNICODE versions of the message
		TOOLTIPTEXTA* pTTTA = (TOOLTIPTEXTA*)pNMHDR;
		TOOLTIPTEXTW* pTTTW = (TOOLTIPTEXTW*)pNMHDR;
		
		CString strTipText, strFullText;
		UINT nID = pNMHDR->idFrom;
		if (pNMHDR->code == TTN_NEEDTEXTA && (pTTTA->uFlags & TTF_IDISHWND) ||
			pNMHDR->code == TTN_NEEDTEXTW && (pTTTW->uFlags & TTF_IDISHWND))
		{
			// idFrom is actually the HWND of the tool
			CWnd* pWnd = CWnd::FromHandle((HWND)nID);
			if (pWnd)
				nID = (UINT)(WORD)pWnd->GetDlgCtrlID();
		}

		if (nID != 0) // will be zero on a separator
			AfxLoadTBString(strFullText, nID, dllInstance);

		ExtractToolTipText (strFullText, pNMHDR, pResult); 	
		return TRUE;    // message was handled
	}

	// identica a quella standard, ma utilizza
	// AfxLoadTBString per localizzare la stringa del messaggio
	//-----------------------------------------------------------------------------
	static CString GetMessageString(UINT nID, HINSTANCE dllInstance) 
	{
		CString rMessage;
		// load appropriate localized string
		if (AfxLoadTBString(rMessage, nID, dllInstance) != 0)
		{
			LPTSTR lpsz = rMessage.GetBuffer(255);
			// first newline terminates actual string
			LPTSTR lpsz1 = _tcschr(lpsz, _T('\r'));
			if (lpsz1 != NULL)
				*lpsz1 = _T('\0');
			else
			{
				lpsz1 = _tcschr(lpsz, _T('\n'));
				if (lpsz1 != NULL)
					*lpsz1 = _T('\0');
			}
		}
		
		rMessage.ReleaseBuffer();
		return rMessage;
	}

	//-----------------------------------------------------------------------------
	static void LocalizeTitle(UINT nID, CString& strTitle, CREATESTRUCT& cs, HINSTANCE dllInstance)
	{
		CString strFullString;
		if (AfxLoadTBString(strFullString, nID, dllInstance))	//m_nIDHelp contiene lo stesso ID utilizzato dalla LoadFrame
		{
			AfxExtractSubString(strTitle, strFullString, 0);    // first sub-string
			cs.lpszName = strTitle;
		}
	}

};


#include "endh.dex"








