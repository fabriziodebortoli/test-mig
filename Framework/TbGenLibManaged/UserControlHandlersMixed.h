#pragma once

//===========================================================================
//							AttachControlEventArg
//===========================================================================
class TB_EXPORT AttachControlEventArg : public UnmanagedEventsArgs
{
private:
	gcroot<System::Windows::Forms::Control^> control;

public:
	AttachControlEventArg(System::Windows::Forms::Control^ control, const CString sError = _T("")) : UnmanagedEventsArgs(sError) { this->control = control; }

	System::Windows::Forms::Control^ GetControl() { return this->control; }
};

//===========================================================================
//							CUserControlHandlerMixed
// this class is the mixed mode interface class before using templates
//===========================================================================
class CUserControlHandlerMixed : public CUserControlHandlerObj
{
public:
	virtual System::Windows::Forms::Control^ GetWinControl () = 0;
};

