#include "stdafx.h"
#include <TbGeneric\Globals.h>

#include "UserControlWrappers.h"

using namespace System;
//===========================================================================
//							UnmanagedEventsArgs
//===========================================================================

//---------------------------------------------------------------------------------------
IMPLEMENT_DYNAMIC(UnmanagedEventsArgs, CObject)

//---------------------------------------------------------------------------------------
UnmanagedEventsArgs::UnmanagedEventsArgs (const CString& sError)
{
	m_sError = sError;
}

//===========================================================================
//							CUserControlHandlerObj
//===========================================================================
//---------------------------------------------------------------------------------------
CUserControlHandlerObj::CUserControlHandlerObj  ()
{
}

//---------------------------------------------------------------------------------------
CUserControlHandlerObj::~CUserControlHandlerObj ()	
{

	/*if (collectOnDispose)
	{
		//GC::Collect ();
		//GC::WaitForPendingFinalizers (); //should not be used: ti causes deadlocks!
		//if you have DisconnectedContext exceptions, probably you haven't correctly disposed your objects
		//and later, when they are garbage collected, it's too late because thei context has gone	
	}*/
}

//===========================================================================
//							CUserControlWrapperObj
//===========================================================================
//---------------------------------------------------------------------------------------
CUserControlWrapperObj::CUserControlWrapperObj () 
	:
	m_pManHandler (NULL)
{
}

//---------------------------------------------------------------------------------------
CUserControlWrapperObj::~CUserControlWrapperObj()
{	

}

//---------------------------------------------------------------------------------------
BOOL CUserControlWrapperObj::CreateControl (DWORD dwStyle, const RECT& rect, CWnd* pParentWnd, UINT nID, CWnd* pOwnerWnd)
{
	CUserControlHandlerObj* pHandler = GetHandler();
	
	if (!pHandler)
		return FALSE;
	
	if (!pHandler->CreateControl (dwStyle, rect, pParentWnd, nID, pOwnerWnd))
		return FALSE;

	return TRUE;
}

//---------------------------------------------------------------------------------------
CUserControlHandlerObj* CUserControlWrapperObj::GetHandler ()
{
	if (m_pManHandler == nullptr)
	{
		ASSERT_TRACE (FALSE, _T("CUserControlWrapperObj not initialized!"));
		return NULL;
	}

	return m_pManHandler;
}

//---------------------------------------------------------------------------------------
HWND CUserControlWrapperObj::GetControlHandle ()
{
	return m_pManHandler ?  m_pManHandler->GetControlHandle() : NULL;
}

//---------------------------------------------------------------------------------------
void CUserControlWrapperObj::OnAfterAttachControl ()
{
	OnInitControl();
}

//---------------------------------------------------------------------------------------
void CUserControlWrapperObj::OnInitControl ()
{
}
