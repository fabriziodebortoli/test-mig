
#pragma once

#include "XMLDataMng.h"

#include "beginh.dex"

//=============================================================================
// CXMLImpExpDialog dialog
//=============================================================================
//
class CXMLImportTuningResult : public CParsedDialog
{
	friend class CXMLImpExpController;

	DECLARE_DYNAMIC(CXMLImportTuningResult)

	Array*		m_pChanges;
	CString		m_strLogFile;
	BOOL		m_bSuccess;
	CButton		m_bnNotify;
	CTBListBox	m_lbxChanges;
	CButton		m_bnSendEmail;
	CString		m_strEmailFrom;
	CMessages	m_aMessages;
	BOOL		m_bWndCollapsed;
	int			m_nExpandedHeight;
	int			m_nCollapsedHeight;
	int			m_nWndWidth;

public:
	CXMLImportTuningResult	(
								CWnd* pParent, 
								const CString& strLogFile, 
								Array* pChanges, 
								BOOL bSuccess
							);   
	virtual ~CXMLImportTuningResult();

private:
	void InitializeControls	();
	void CalculateCollapse	();
	void LoadListBox		();
	BOOL CanSendEmail		();
	BOOL IsValidAddress		(const CString& sAddress);
	
protected:
	virtual BOOL OnInitDialog	();

	DECLARE_MESSAGE_MAP()
	afx_msg void OnLogFileClicked();
	afx_msg void OnNotifyClicked();
	afx_msg void OnSendEmail	();
};

//=============================================================================
// CXMLImpExpDialog dialog
//=============================================================================
//
class CXMLImpExpDialog : public CLocalizableDialog
{
	friend class CXMLImpExpController;

	DECLARE_DYNAMIC(CXMLImpExpDialog)

	BOOL			m_bAborted;
	Scheduler		m_Scheduler;
	CProgressCtrl	m_ProgressBar;
	//int				m_nMaxStep;
	

public:
	CXMLImpExpDialog(CWnd* pParent = NULL);   
	virtual ~CXMLImpExpDialog();

	
private:
	void SetMessage	(const CString& strMessage);

protected:
	DECLARE_MESSAGE_MAP()
	virtual void PostNcDestroy();
	virtual BOOL OnInitDialog();
	virtual void OnCancel();

};


//=============================================================================
// CXMLImpExpController 
//=============================================================================
class CXMLImpExpController : public CNoTrackObject
{
private:
	CXMLImpExpDialog	*m_pDialog;

public:
	CXMLImpExpController();
	BOOL IsBusy()		{ return m_pDialog != NULL; }
	BOOL IsAborted()	{ return m_pDialog ? m_pDialog->m_bAborted : TRUE;}
	
	BOOL ShowDialog();
	void CloseDialog();

	void SetMessage	(const CString& strMessage);
};


CXMLImpExpController* AfxGetXMLImpExpController();

#include "endh.dex"
