
#pragma once

#include "DictionaryClasses.h"

class CDemoView;

// CDemoFrame frame
//----------------------------------------------------------------------------
class CDemoFrame : public CFrameWnd
{
	
	DECLARE_DYNCREATE(CDemoFrame)

public:
	UINT m_nResID;
	HMODULE m_hModule;

	CDemoFrame() {}
	CDemoFrame(UINT nResID, HMODULE hModule); 
	virtual ~CDemoFrame();

protected:
	DECLARE_MESSAGE_MAP()
public:
	afx_msg void OnPaint();
};

// CDemoDialog dialog
//----------------------------------------------------------------------------

class CDemoDialog : public CDialog
{
	DECLARE_DYNAMIC(CDemoDialog)

	// ATTENZIONE!!! Ricordarsi di cancellare il puntatore restituito da questa funzione
	LPDLGTEMPLATE GetLocalDialogTemplate(UINT nResID);

	void AdjustTemplate(LPDLGTEMPLATE pTemplate);
	//BOOL IsButtonItem(DLGITEMTEMPLATE* pItem, BOOL bDialogEx);
	//void AddButtonItem(DLGITEMTEMPLATE* pItem, BOOL bIsDialogEx);
	//void RemoveButtonItems();
	BOOL IsCurrentControl(CWnd* pWnd, const CString& strCurrentText);

private:
	//CUIntArray m_arButtonIDs;
	CPtrArray m_arButtons;

public:
	CStringBlock *m_pBlock;
	
	HWND m_hwndCurrentSelection;
	CPtrArray m_NonLocalizedWindows;
	
	CBrush	m_WrongBrush;
	CBrush	m_SelBrush;

	CDemoDialog();   // standard constructor
	virtual ~CDemoDialog();
	BOOL Init(CStringBlock *pBlock, CDemoFrame *pFrame, CWnd* pParentOwner);

	void AddNotFoundString(CWnd* pWnd);
	void SetFont(CFont *pFont);

protected:

	DECLARE_MESSAGE_MAP()
public:
	virtual BOOL OnInitDialog();
	void RefreshStrings(CWnd* pWnd);

//	afx_msg HBRUSH OnCtlColor(CDC* pDC, CWnd* pWnd, UINT nCtlColor);
//	afx_msg void OnPaint();
};


// CDemoView form view
//----------------------------------------------------------------------------
class CDemoView : public CView
{
	DECLARE_DYNCREATE(CDemoView)
	
	CDemoDialog*	m_pDemoDialog;
	CPtrArray		m_arBorderWnds;

protected:
	CDemoView();     
	virtual ~CDemoView();

	virtual void OnInitialUpdate();

public:
	void DrawBorder(CWnd *pWnd, COLORREF aColor);
	void PlaceDialog();
	void PlaceFrame();
	void MarkControls();
	void UnmarkControls();
	BOOL RefreshDialog(CStringBlock *pBlock, BOOL bPositionFrame);
	BOOL CreateDemoDialog(CStringBlock *pBlock, CDemoFrame* pFrame);

	CDemoDialog* GetDemoDialog() { return m_pDemoDialog; }

#ifdef _DEBUG
	virtual void AssertValid() const;
	virtual void Dump(CDumpContext& dc) const;
#endif

protected:
	virtual void OnDraw(CDC* /*pDC*/);

	DECLARE_MESSAGE_MAP()
};



