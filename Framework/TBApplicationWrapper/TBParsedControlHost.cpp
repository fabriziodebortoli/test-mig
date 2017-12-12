#include "StdAfx.h"


#include <TbGeneric\Globals.h>
#include <TbGenlib\PARSOBJ.H>

#include "TBParsedControlHost.h"

using namespace Microarea::Framework::TBApplicationWrapper;
using namespace System::Windows::Forms;

void TBParsedControlHost::WndProc(Message% m)
{
	if (m.Msg == WM_COMMAND)
	{ 
		WPARAM wParam = (WPARAM)(int)m.WParam;
		LPARAM lParam = (LPARAM)(int)m.LParam;
		DECLARE_WM_COMMAND_PARAMS(wParam, lParam, nID, nCode, hWndCtrl);
		HWND hCtrlFocus = NULL;
		//uso il metodo della parsed form per smistare correttamente i messaggi ai parsed controls
		CParsedForm::DoCommand(wParam, lParam, (HWND)(int)Handle, &hCtrlFocus);
	}
	__super::WndProc(m);
}