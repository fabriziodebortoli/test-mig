#pragma once

#include "beginh.dex"

//===========================================================================
//		THIS FILE HAVE TO BE INCLUDED IN UNMANAGED FRAMEWORK
//===========================================================================

// this class is the base class of a managed parameter passing through unmanaged code
//===========================================================================
class TB_EXPORT UnmanagedEventsArgs : public CObject
{
	DECLARE_DYNAMIC(UnmanagedEventsArgs)

private:
	CString	m_sError;

public:
	UnmanagedEventsArgs	(const CString& sError);
};

// abstract handler of the managed control 
//===========================================================================
class TB_EXPORT CUserControlHandlerObj : public CObject
{
	friend class CUserControlWrapperObj;

public:
	CUserControlHandlerObj  ();
	~CUserControlHandlerObj ();

protected:
	virtual BOOL	 CreateControl	(DWORD dwStyle, const RECT& rect, CWnd* pParentWnd, UINT nID, CWnd* pOwnerWnd) = 0;
	virtual HWND	 GetControlHandle() = 0;

public:
	virtual void	 AttachControl	(UnmanagedEventsArgs* pArg) = 0;
	virtual void	 AttachWindow	(CWnd* pWnd) = 0;
};

// the abstract class to derive managed component
//===========================================================================
class TB_EXPORT CUserControlWrapperObj 
{
	friend class CManagedParsedCtrl;

protected:
	CUserControlHandlerObj* m_pManHandler;	// managed communication handler

public:
	CUserControlWrapperObj ();
	~CUserControlWrapperObj ();

public:
	CUserControlHandlerObj*	GetHandler			();
	HWND					GetControlHandle	();
protected:
	BOOL CreateControl			(DWORD dwStyle, const RECT& rect, CWnd* pParentWnd, UINT nID, CWnd* pOwnerWnd);
	void OnAfterAttachControl	();

	virtual void OnInitControl	();
};

class TB_EXPORT CObjectWrapperObj
{
public:
	virtual ~CObjectWrapperObj(){}
};

//=============================================================================


#define ENSURE_USER_CONTROL(r, cl, uc, szNotInitialized) if (!GetHandler() || GetUserControl(this) == nullptr) { \
									ASSERT_TRACE (FALSE, szNotInitialized); \
									cl^ uc = nullptr; \
									return r; \
								} \
								cl^ uc = GetUserControl(this);

#define VOID_ENSURE_USER_CONTROL(cl, uc, szNotInitialized) if (!GetHandler() || GetUserControl(this) == nullptr) { \
									ASSERT_TRACE (FALSE, szNotInitialized); \
									cl^ uc = nullptr; \
									return; \
								} \
								cl^ uc = GetUserControl(this); 

//=============================================================================
#include "endh.dex"

