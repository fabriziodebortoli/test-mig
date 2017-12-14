#include "stdafx.h"
#include "afxwinforms.h"
#include "atlimage.h"

#include <TbNameSolver\FileSystemFunctions.h>
#include <TbGeneric\Globals.h>
#include <TbGeneric\FontsTable.h>

#include <TbGenlib\Generic.h>
#include "ParsedUserControl.h"
#include "UserControlHandlers.h"

using namespace System;
using namespace System::ComponentModel;
using namespace System::Collections;
using namespace System::Windows::Forms;
using namespace System::Data;
using namespace System::Drawing;

ref class UserControlEventsHandler : public ManagedEventsHandlerObj
{	
public:
	//---------------------------------------------------------------------------------------
	UserControlEventsHandler () 
	{
	}
	
	//---------------------------------------------------------------------------------------
	virtual void MapEvents (Object^ pObject) override
	{
		Control^ pControl = (Control^) pObject;

		pControl->LostFocus += gcnew EventHandler (this, &UserControlEventsHandler::LostFocus);		
		pControl->TextChanged += gcnew EventHandler (this, &UserControlEventsHandler::TextChanged);		
	}

	//---------------------------------------------------------------------------------------
	void LostFocus (Object^ sender, System::EventArgs^ e) 
	{
		CParsedUserControlWrapper* pObject = (CParsedUserControlWrapper*) GetControlWnd();
		pObject->PerformLosingFocus ();
	}

	//---------------------------------------------------------------------------------------
	void TextChanged (Object^ sender, System::EventArgs^ e) 
	{
		CParsedUserControlWrapper* pObject = (CParsedUserControlWrapper*) GetControlWnd();
		pObject->PerformTextChanged ();	
	}
};

//===========================================================================
//								CParsedUserControlWrapper
//===========================================================================
Control^ GetUserControl(CUserControlWrapperObj* pWrapper)
{
	if (pWrapper == NULL || pWrapper->GetHandler() == NULL)
	{
		ASSERT_TRACE (FALSE, _T("Managed User Control not initialized!"));
		return nullptr;
	}
	CUserControlHandlerMixed* p =  (CUserControlHandlerMixed*) pWrapper->GetHandler();
	return p->GetWinControl();
}

//---------------------------------------------------------------------------------------
CParsedUserControlWrapper::CParsedUserControlWrapper()
{
	m_pManHandler = new CUserControlHandler<Control>(gcnew UserControlEventsHandler());
}

//---------------------------------------------------------------------------------------
void CParsedUserControlWrapper::Enable (BOOL bValue /*TRUE*/)
{
	Control^ ctrl = GetUserControl(this);
	if (ctrl != nullptr)
		ctrl->Enabled = bValue == TRUE;
}

//---------------------------------------------------------------------------------------
CString CParsedUserControlWrapper::GetValue ()
{
	Control^ ctrl = GetUserControl(this);
	
	return ctrl == nullptr ? _T("") : CString(ctrl->Text);
}

//---------------------------------------------------------------------------------------
void CParsedUserControlWrapper::SetValue (const CString& sValue)
{
	Control^ ctrl = GetUserControl(this);
	if (ctrl != nullptr)
		ctrl->Text = gcnew String(sValue);
}

//---------------------------------------------------------------------------------------
LRESULT CParsedUserControlWrapper::OnGetControlDescription(WPARAM wParam, LPARAM lParam)
{
	return (LRESULT) 0;
}

//---------------------------------------------------------------------------------------
void CParsedUserControlWrapper::PerformLosingFocus	()
{
}

//---------------------------------------------------------------------------------------
void CParsedUserControlWrapper::PerformTextChanged	()
{
}


//---------------------------------------------------------------------------------------
void CParsedUserControlWrapper::OnAfterAttachControl ()
{
	CUserControlHandlerMixed* pHandler =  (CUserControlHandlerMixed*) GetHandler();
	if (!pHandler)
		return;

	pHandler->AttachWindow(NULL);
	pHandler->AttachWindow((CWnd*) this);
}

//---------------------------------------------------------------------------------------
void CParsedUserControlWrapper::AttachControl (AttachControlEventArg* pEventArg)
{
	CUserControlHandlerMixed* pHandler =  (CUserControlHandlerMixed*) GetHandler();
	if (!pHandler)
		return;

	pHandler->AttachControl(pEventArg);

	OnAfterAttachControl ();
}