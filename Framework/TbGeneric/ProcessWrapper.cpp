#include "StdAfx.h"

#include <TbNameSolver\FileSystemFunctions.h>

#include "ProcessWrapper.h"

IMPLEMENT_DYNCREATE(CProcessWrapper, CObject)
//-----------------------------------------------------------------------------
CProcessWrapper::CProcessWrapper(const CString& strProcessName)
:
	m_ProcessHandle(0)
{
	m_strProcessName = strProcessName;
}

//-----------------------------------------------------------------------------
CProcessWrapper::~CProcessWrapper(void)
{
	Close();
}

//-----------------------------------------------------------------------------
BOOL CProcessWrapper::Run(const CString& strProcessParams)
{
	if (!Close())
		return FALSE;

	TB_LOCK_FOR_WRITE();
	
	BOOL bOk = FALSE;

	if (::ExistFile (m_strProcessName))
	{
		CString sCmdLine = m_strProcessName + strProcessParams;
		LPTSTR psz = (LPTSTR) (LPCTSTR) sCmdLine;
	
		PROCESS_INFORMATION ProcessInfo;
		STARTUPINFO StartupInfo = {0};
		StartupInfo.cb = sizeof(STARTUPINFO);
		if (CreateProcess(NULL, psz, NULL, NULL, FALSE, 0, NULL, NULL, &StartupInfo, &ProcessInfo))
			m_ProcessHandle = ProcessInfo.hProcess;
	}

	return bOk;
}

//-----------------------------------------------------------------------------
BOOL CProcessWrapper::Close()
{
	TB_LOCK_FOR_WRITE();

	UINT nExitcode = 0;

	if (m_ProcessHandle > 0)
	{
		::TerminateProcess(m_ProcessHandle, nExitcode);
		m_ProcessHandle = 0;
	}

	return nExitcode == 0;
}