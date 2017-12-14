#pragma once

#include "sourcesafeitemcollection.h"
#include "sourceControlWrapperLibrary.h"
#include "beginh.dex"

//================================================================================
class TB_EXPORT CSourceSafeItem
{
	long m_nHandle;

public:
	CSourceSafeItem();
	CSourceSafeItem(long handle);
	CSourceSafeItem(const CSourceSafeItem& source);
	CSourceSafeItem operator = (const CSourceSafeItem& source);

	~CSourceSafeItem(void);

	BOOL IsValid() { return m_nHandle != 0; }

	CString GetName();
	CString GetPath();
	int GetType();
	BOOL IsCheckedOutToMe();
	BOOL IsCheckedOut();
	BOOL IsProject();
	CString GetLocalPath();
	void SetLocalPath(const CString strPath);
	BOOL GetBinary();
	void SetBinary(BOOL bSet);
	
	CSourceSafeItemCollection GetItems();
	void CheckIn(const CString local, const CString comment);
	void CheckOut(const CString local, const CString comment, BOOL updateLocal);
	void UndoCheckOut(const CString local);
	void Rename(const CString newName);
	void GetLatestVersion(const CString local);
	void Delete();
	CSourceSafeItem Add(const CString local, const CString comment, BOOL isProject);
	BOOL IsDifferent(const CString localPath);
	void Label(const CString label, const CString comment);
};
#include "endh.dex"