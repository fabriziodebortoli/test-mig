#pragma once
class CWoormView;
#include <TbGenlib\ParsEdt.h>
#include "beginh.dex"
// CTBPrintDialog
class CTBPrintButtonOk : public CButton
{
	
	DECLARE_MESSAGE_MAP();
public:
	CString	m_strPdfFile;
};

class TB_EXPORT CTBPrintDialog : public CPrintDialog
{
	DECLARE_DYNAMIC(CTBPrintDialog)
	CWoormView*			m_pView;
	CButton				m_chkSetApplicationPrinter;
	CButton				m_chkPrintOnLetterhead;
	CTBPrintButtonOk	m_bOkButton;
	HWND				m_hWndPersistent;

public:
	BOOL	m_bMaintainPrinter;
	BOOL	m_bShowPrintOnLetterhead;
	DataBool*	m_pbPrintOnLetterhead;

public:
	CTBPrintDialog(BOOL bPrintSetupOnly, // TRUE for Print Setup, FALSE for Print Dialog
		DWORD dwFlags = PD_ALLPAGES | PD_USEDEVMODECOPIES | PD_NOPAGENUMS
			| PD_HIDEPRINTTOFILE | PD_NOSELECTION,
		CWnd* pParentWnd = NULL);
	
	CTBPrintDialog (CPrintDialog* pSource, CWoormView* pView, BOOL bMaintainPrinter, BOOL bShowPrintOnLetterhead, DataBool* pbPrintOnLetterhead);

	virtual ~CTBPrintDialog();
	INT_PTR DoModal();

	afx_msg void OnToggleApplicationPrinter();
	afx_msg void OnTogglePrintOnLetterhead();

	DECLARE_MESSAGE_MAP();

private:

	void Assign(CPrintDialog* pDialog);
	void AddCustomTemplate();

public:
	virtual BOOL OnInitDialog();
};
#include "endh.dex"
