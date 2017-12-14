#pragma once
#include <TbGeneric\LocalizableObjs.h> 
#include <TbGenlibUI\BrowserDlg.h>
#include "beginh.dex"



class TB_EXPORT CDocumentBrowser : public CModelessBrowserDlg
{
	DECLARE_DYNAMIC(CDocumentBrowser)
	DECLARE_MESSAGE_MAP()
	CString m_sNamespace;
protected:
	LRESULT OnChangeFrameStatus(WPARAM wParam, LPARAM lParam);
	LRESULT OnGetNamespaceAndIcon(WPARAM wParam, LPARAM lParam);
	LRESULT OnGetDocumentTitleInfo(WPARAM wParam, LPARAM lParam);
	BOOL OnInitDialog ();
public:
	void OpenDocument(const CString& sNamespace);
	afx_msg void OnDestroy();
};
#include "endh.dex"