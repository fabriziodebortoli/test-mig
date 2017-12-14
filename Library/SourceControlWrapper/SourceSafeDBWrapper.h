#pragma once

#include "sourcesafeitem.h"
#include "sourcesafeitemcollection.h"


#include "beginh.dex"


//================================================================================
class TB_EXPORT CSourceSafeDBWrapper
{
private: 
	long m_nHandle;

public:
	CSourceSafeDBWrapper();
	CSourceSafeDBWrapper(const CString strRemoteServer);
	~CSourceSafeDBWrapper(void); 

	BOOL IsValid() { return m_nHandle != 0; }

	BOOL IsOpen();
	BOOL Open(const CString iniPath, const CString userName, const CString password);
	BOOL CheckOutFile(const CString  file, const CString  localPath);
	BOOL CheckInFile(const CString file, const CString localPath);
	CSourceSafeItem CreateProject(const CString path, const CString comment, BOOL recursive);
	CSourceSafeItem GetItem(const CString aVssPath);
	CString GetCurrentProject();
	CSourceSafeItemCollection GetItems(const CString aVssPath);

} ;
#include "endh.dex"
