#pragma once

#include <TBNameSolver\TBResourceLocker.h>

#include "BeginH.dex"



class TB_EXPORT CProcessWrapper : public CObject, public CTBLockable
{
	DECLARE_DYNCREATE(CProcessWrapper);
	CString		m_strProcessName;
	HANDLE		m_ProcessHandle;
protected:
	CProcessWrapper() : m_ProcessHandle(NULL) {}
public:
	CProcessWrapper(const CString& strProcessName);
	~CProcessWrapper(void);

	BOOL Run(const CString& strProcessParams);
	BOOL Close();

	virtual LPCSTR  GetObjectName() const { return "CProcessWrapper"; }
};

#include "EndH.dex"