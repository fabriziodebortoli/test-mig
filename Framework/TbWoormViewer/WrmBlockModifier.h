#pragma once

#include <TbGeneric\DataObj.h>
#include <TbGeneric\mlistbox.h>
#include <TbGenlib\batchdlg.h>

//includere alla fine degli include del .H
#include "beginh.dex"

//============================================================================= 
class TB_EXPORT CWrmBlockModifierDlg : public CBatchDialog
{
protected:
	CStrEdit	m_NomeFile;
	CButton		m_BtnBrowse;
	CTBListBox	m_Status;
	CButton		m_BtnOslEncrypt;
	CButton		m_BtnOslDecrypt;
	CButton		m_BtnOslNoChangeCryptState;

public:
	CWrmBlockModifierDlg ();

	void EnableIDOK	(BOOL bEnable = TRUE);

protected:
	virtual BOOL OnInitDialog	();
	virtual	void OnBatchExecute	();

	void DoUpdate		(CString strInitialPath, CString strFindExt);
	void DoCheck		(CString strInitialPath, CString strFindExt);
	void DoGenSymTable	(CString strInitialPath, CString strFindExt);
	void DoGenReportSymTable	(CString strPath);

	//{{AFX_MSG(CWrmBlockModifierDlg)
	afx_msg void OnBrowse();
	afx_msg void OnGenSymTable();
	afx_msg void OnGenSymTable2();
	afx_msg void OnSaveEnums();
	//}}AFX_MSG
	DECLARE_MESSAGE_MAP()
};

#include "endh.dex"
