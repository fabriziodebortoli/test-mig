#pragma once

#include <TbGenlib\baseapp.h>

#include "beginh.dex"

//==============================================================================
class TB_EXPORT CTaskBuilderApp : public CBaseApp, public IHostApplication
{   
private:
	bool m_bControlSoapExecution;
	CMapStringToPtr m_loginFunctionMap;
public:
	CTaskBuilderApp();

protected:
	BOOL LoadSoapNeededLibraries	(LPCTSTR lpszModuleNamespace, CString& strError);
	BOOL CanExecuteSoapMethod		(CLoginContext* pContext, LPCTSTR lpszActionNamespace, CString& strError);
	BOOL EnableSoapExecutionControl	(BOOL bSet);

private:
	// initialization
	BOOL InitApplicationContext	(const CString& strFileServer, const CString& strInstallation, const CString& strMasterSolutionName);
	void InitApplicationsAndUI	();
	void InitServerObjects		(const CString& strFileServer, const CString& strWebServer, const CString& strInstallation, const CString& strMasterSolutionName);
	void InitExtensions			();
	void StartWCFServices		();

public:	
	// command line parsing. Load a specified argument 
	// (command line has to be formatted as name=value)
	CString	GetArgumentValue(const CString& strArgumentName);
	BOOL	HasArgument		(const CString& strArgumentName);

public:
	// starting and closing application
	virtual BOOL	InitInstance();
	virtual	int		ExitInstance();
	virtual int		Run();
#ifdef DEBUG
	virtual BOOL PumpMessage() { return __super::PumpMessage(); }
#endif
public:
	virtual BOOL	IsMasterFrame	(CRuntimeClass*);
	static void AdjustClosingMessageViewer();
protected:
	virtual BOOL UnattendedStart()	{ return FALSE; }

	afx_msg void OnCloseLogin		(WPARAM wParam, LPARAM lParam);
	afx_msg void OnThreadTimer		(WPARAM, LPARAM);

	DECLARE_MESSAGE_MAP()
};
//==============================================================================
TB_EXPORT BOOL AfxReloadApplication(const CString& sAppName);

#include "endh.dex"
