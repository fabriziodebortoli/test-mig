#pragma once

#include "UserControlWrappers.h"
#include "beginh.dex"

class CUserControlHandlerObj;
class AttachControlEventArg;
//===========================================================================
class TB_EXPORT CParsedUserControlWrapper : public CUserControlWrapperObj
{
public:
	CParsedUserControlWrapper();

public:
	void Enable	(BOOL bValue = TRUE);
	
protected:
	virtual CString GetValue				();
	virtual void	SetValue				(const CString& sValue);
	virtual LRESULT OnGetControlDescription	(WPARAM wParam, LPARAM lParam);
	virtual void	OnAfterAttachControl	();

public:
	virtual void	AttachControl			(AttachControlEventArg* pArg);

public:
	// event map
	virtual void PerformLosingFocus	();
	virtual void PerformTextChanged	();

};

#include "endh.dex"