#include "StdAfx.h"
#include <TBGeneric\globals.h>

#include "TBToolStripMenuItem.h"
#include "TBCmdUI.h"


using namespace System;
using namespace System::Windows::Forms;

using namespace Microarea::Framework::TBApplicationWrapper;



//-----------------------------------------------------------------------------
TBToolStripMenuItem::TBToolStripMenuItem(int commandID, TBMenuStrip^ ownerMenu)
{
	this->commandID = commandID;
	this->ownerMenu = ownerMenu;
	m_bPreviousState = false;
	m_bEnableChanged = false; 
	m_bWindowEnabled = true;
}

//-----------------------------------------------------------------------------
void TBToolStripMenuItem::OnClick(EventArgs^ e)
{
	if (commandID == -1)
		return;
	::PostMessage((HWND)ownerMenu->WindowHandle.ToInt32(), WM_COMMAND, commandID, NULL);
}

//-----------------------------------------------------------------------------
void TBToolStripMenuItem::OnDropDownOpened(EventArgs^ e)
{
	TBMenuStrip ^menu = this->ownerMenu;
	bool bActualWindowEnable = (CWnd::FromHandle((HWND)menu->WindowHandle.ToInt32())->IsWindowEnabled() == TRUE);
	m_bEnableChanged = m_bWindowEnabled != bActualWindowEnable;
	m_bWindowEnabled = bActualWindowEnable;
	
	for (int i = 0; i < this->DropDownItems->Count; i++)
	{
		ToolStripItem ^item = this->DropDownItems[i];
		
		if (item->GetType() == TBToolStripMenuItem::typeid)
		{
			TBToolStripMenuItem^ tbItem = (TBToolStripMenuItem^) item;
			if (m_bWindowEnabled)
			{	
				CProxyCTBCmdUI ui(tbItem->CommandID);
				::SendMessage((HWND)menu->WindowHandle.ToInt32(), UM_UPDATE_EXTERNAL_MENU, (WPARAM)&ui, NULL);
				ui.AssignUpdates(tbItem);
				if (m_bEnableChanged)
					RestoreItemEnabledState(tbItem);
			}
			else if (m_bEnableChanged)
				AssignItemEnabledState(tbItem);
		}
	}
}

//-----------------------------------------------------------------------------
void TBToolStripMenuItem::RestoreItemEnabledState(TBToolStripMenuItem^ menuItem)
{
	menuItem->Enabled = m_bPreviousState;
}

//-----------------------------------------------------------------------------
void TBToolStripMenuItem::AssignItemEnabledState(TBToolStripMenuItem^ menuItem)
{
	
	m_bPreviousState = menuItem->Enabled;
	menuItem->Enabled = false;
	
}
//-----------------------------------------------------------------------------
void TBToolStripMenuItem::Text::set(System::String^ value) 
{
	int idx = value->IndexOf("\t");
	if (idx != -1)
	{
		__super::Text = value->Substring(0, idx); 
		ShortcutKeyDisplayString = value->Substring(idx + 1);
	}
	else
	{
		__super::Text = value; 
	}

} 
		
//-----------------------------------------------------------------------------
void TBMenuStrip::ParseMenu(CMenu* pMenu, ToolStripItemCollection ^items, TBMenuStrip^ mainMenu)
{
	CString strText;
	for (int i = 0; i < (int)pMenu->GetMenuItemCount(); i++)
	{
		pMenu->GetMenuString(i, strText, MF_BYPOSITION);
		if (strText.IsEmpty())
		{
			items->Add(gcnew ToolStripSeparator());
			continue;
		}
		TBToolStripMenuItem^ item = gcnew TBToolStripMenuItem(pMenu->GetMenuItemID(i), mainMenu);
		item->Text = gcnew System::String((LPCTSTR)strText);
		items->Add(item);
		CMenu* pSubMenu = pMenu->GetSubMenu(i);
		if (pSubMenu && ::IsMenu (pMenu->m_hMenu))
			ParseMenu(pSubMenu, item->DropDownItems, mainMenu);		
	}
}

//-----------------------------------------------------------------------------
TBMenuStrip^ TBMenuStrip::FromMenuHandle(IntPtr windowHandle, IntPtr menuHandle)
{
	HMENU hMenu = (HMENU)menuHandle.ToInt32();
	if (!::IsMenu(hMenu))
		return nullptr;

	CMenu* pMenu = CMenu::FromHandle(hMenu);
	TBMenuStrip^ mainMenu = gcnew TBMenuStrip(windowHandle);
	
	ParseMenu(pMenu, mainMenu->Items, mainMenu);

	return mainMenu;
}