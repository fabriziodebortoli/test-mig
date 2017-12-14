#include "StdAfx.h"
#include "TBCmdUI.h"
using namespace System::Windows::Forms;
using namespace Microarea::Framework::TBApplicationWrapper;



//-----------------------------------------------------------------------------
void CProxyCTBCmdUI::AssignUpdates(TBToolStripMenuItem^ menuItem)
{
	if (m_UptateType == CTBCmdUI::UpdateNone) return;

	if ((m_UptateType & CTBCmdUI::UpdateEnable) == CTBCmdUI::UpdateEnable)
		menuItem->Enabled = (m_bEnabled == TRUE);

	if ((m_UptateType & CTBCmdUI::UpdateSetCheck) == CTBCmdUI::UpdateSetCheck)
	{
		switch (m_nCheck)
		{
		case 0:
			menuItem->CheckState = CheckState::Unchecked;
			break;
		case 1:
			menuItem->CheckState = CheckState::Checked;
			break;
		case 2:
			menuItem->CheckState = CheckState::Indeterminate;
			break;
		}
	}
	if ((m_UptateType & CTBCmdUI::UpdateSetRadio) == CTBCmdUI::UpdateSetRadio)
		System::Diagnostics::Debug::Fail("SetRadio non supported!");

	if ((m_UptateType & CTBCmdUI::UpdateSetText) == CTBCmdUI::UpdateSetText)
	{
		menuItem->Text = gcnew System::String((LPCTSTR)m_strText);
		if (menuItem->Text->Length == 0)
			menuItem->Visible = false;
		else
			menuItem->Visible = true;

	}


}