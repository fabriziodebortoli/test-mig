#pragma once

#include "beginh.dex"

//===========================================================================
class TB_EXPORT EnumsViewerWrapper : public CObject
{
	DECLARE_DYNCREATE(EnumsViewerWrapper)

	EnumsViewerWrapper	() {}

public:
	static bool Open		(CString aCulture, CString aInstallation);
	static bool Close		();
	static bool IsClosed	();
};

#include "endh.dex"