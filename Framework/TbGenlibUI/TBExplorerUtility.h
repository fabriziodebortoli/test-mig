#pragma once

#include <TbGenlib\PARSLBX.H>
#include <TbGenlib\PARSCBX.H>
#include <TbGenlib\PARSEDT.H>

#include "beginh.dex"

class CTBObjDetails;
//===========================================================================
class TB_EXPORT CTreeItemLoc : public CObject		// per localizzazione (parte non localizzata)
{
public: 
	CString m_strName;

public: 
	CTreeItemLoc (CString strName);
};

//--------------------------------------------------------------------------
class TB_EXPORT CSaveAdminDialog : public CParsedDialog
{
	DECLARE_DYNAMIC(CSaveAdminDialog)

public:
	CTBListBox		m_ListUserSave;

	CStringArray	m_aListUsers;
	CStringArray&	m_aUsrsSave;

	CString&		m_strReportName;

	BOOL			m_bFromSave;
	BOOL			m_bSaveAllUsers;
	BOOL			m_bSaveStandard;
	BOOL			m_bSaveCurrLanguage;
public:	
	CSaveAdminDialog(const CStringArray& aUsrs, CStringArray& aUsrsToSave, CString& sReportName, BOOL bFromSave = FALSE, CWnd* pParent = NULL);   
	CSaveAdminDialog(UINT nIDD, const CStringArray& aUsrs, CStringArray& aUsrsToSave, CString& sReportName, BOOL bFromSave = FALSE, CWnd* pParent = NULL);
	virtual ~CSaveAdminDialog();

protected:
	BOOL	FillUserList			();

protected:
	virtual void DoDataExchange(CDataExchange* pDX);    
    
protected:
//{{AFX_MSG(CTBFileDlg)
afx_msg	void	OnBtnSelAllUsrsClicked	();
afx_msg	void	OnBtnSelStdClicked		();
virtual BOOL	OnInitDialog			();
virtual void	OnOK					();
afx_msg	void	OnBtnSelCurrLangClicked	();
//}}AFX_MSG
DECLARE_MESSAGE_MAP()
};

//--------------------------------------------------------------------------
class TB_EXPORT CFullSaveAdminDialog : public CSaveAdminDialog
{
	DECLARE_DYNAMIC(CFullSaveAdminDialog)
protected:
	DataStr m_dsApplication;
	DataStr m_dsModule;
	DataStr m_dsName;

	CStrEdit			m_edtName;
	CDescriptionCombo	m_cbxApps;
	CDescriptionCombo	m_cbxMods;

	CTBNamespace	m_Namespace;
	CString			m_sNamespace;
	CStringArray	m_arUsersToSave;
public:
	CFullSaveAdminDialog(const CTBNamespace& ns, CWnd* pParent = NULL);
	virtual ~CFullSaveAdminDialog() {}

	BOOL	GetIsSavedInCurrentLanguage() { return m_bSaveCurrLanguage; }
	int		GetSavePath(CStringArray&);				// restituzione array di stringhe delle path in cui salvare

protected:
	
	virtual BOOL	OnInitDialog();
	//virtual void	OnOK();

	afx_msg void OnNameChanged();
	afx_msg void OnApplicationChanged();
	afx_msg void OnModuleChanged();

	DECLARE_MESSAGE_MAP()
};

//--------------------------------------------------------------------------
class TB_EXPORT CAskCopyLinkDialog : public CParsedDialog
{
	DECLARE_DYNAMIC(CAskCopyLinkDialog)

public:
	BOOL		m_bSaveLink;
	CButton		m_rd_SaveLnk;	
	CButton		m_rd_CopyObj;	

public:	
	CAskCopyLinkDialog(CWnd* pParent = NULL);   
	virtual ~CAskCopyLinkDialog();

//protected:

protected:
	virtual void DoDataExchange(CDataExchange* pDX);    
    
protected:
//{{AFX_MSG(CAskCopyLinkDialog)
virtual BOOL	OnInitDialog			();
virtual void	OnOK					();
afx_msg	void	OnRdCopyClicked			();
afx_msg	void	OnRdLinkClicked			();
//}}AFX_MSG
DECLARE_MESSAGE_MAP()
};

//-----------------------------------------------------------------------------
TB_EXPORT BOOL GetReportTitle(const CString& sReportPath, CString& sTitle, CString& sSubject);

//-----------------------------------------------------------------------------
#include "endh.dex"