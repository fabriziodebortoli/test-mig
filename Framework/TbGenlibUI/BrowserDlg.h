#pragma once

//#include <TbGeneric\wrpWeb/Browser2.h>
#include <TbGenlib\CEFClasses.h>
//includere alla fine degli include del .H
#include "beginh.dex"

class CModelessBrowserDlg;
class CBrowserObj;

class TB_EXPORT CBrowserDlg : public CLocalizableDialog, protected CBrowserEventsObj
{
	DECLARE_DYNAMIC(CBrowserDlg)
	DECLARE_MESSAGE_MAP()

	//CWebBrowser2	m_ctrlWebBrowser ;
	CString			m_strURL;
	CBrowserObj*	m_pBrowser;
public:

	CBrowserDlg				();
	CBrowserDlg				(const CString &strURL);
	
	void SetURL				(const CString &strURL) {m_strURL = strURL;}
	CString GetURL			() {return m_strURL;}
	void Navigate			(CString strURL);
	void SetCookie			(const CString& sUrl, const CString& sName, const CString& sValue);
	void ExecuteJavascript	(const CString& sCode);
protected:
	void OnAfterCreated		(CBrowserObj* pBrowser);
	void OnBeforeClose		(CBrowserObj* pBrowser);
	void AdjustPosition		(int cx, int cy);
	
	BOOL OnInitDialog		(); 


	afx_msg void OnSize		(UINT, int, int );
};

//=============================================================================
class TB_EXPORT CModelessBrowserDlg : public CBrowserDlg
{
	DECLARE_DYNAMIC(CModelessBrowserDlg)
	DECLARE_MESSAGE_MAP()

private:
	
public:
	CModelessBrowserDlg	();
	CModelessBrowserDlg	(const CString &strURL);
public:
	BOOL Create		();

	afx_msg void OnClose();
	afx_msg void OnDestroy();

};



/////////////////////////////////////////////////////////////////////////////

#include "endh.dex"
