#include "StdAfx.h"
#include "TBCmdUI.h"


//-----------------------------------------------------------------------------
CTBCmdUI::CTBCmdUI(int commandID)
{
	m_nID = commandID;
	m_UptateType = CTBCmdUI::UpdateNone;
	m_nCheck = 0;
}

//-----------------------------------------------------------------------------
CTBCmdUI::~CTBCmdUI(void)
{
}


//-----------------------------------------------------------------------------
void CTBCmdUI::Enable(BOOL bOn /*= TRUE*/)
{
	m_bEnabled = bOn;
	m_UptateType |= CTBCmdUI::UpdateEnable;
	m_bEnableChanged = TRUE;
}

//-----------------------------------------------------------------------------
void CTBCmdUI::SetCheck(int nCheck /*= 1*/)
{
	m_nCheck = nCheck;
	m_UptateType |= CTBCmdUI::UpdateSetCheck;
}

//-----------------------------------------------------------------------------
void CTBCmdUI::SetRadio(BOOL bOn /*= TRUE*/)
{
	m_bRadio = bOn;
	m_UptateType |= CTBCmdUI::UpdateSetRadio;
}

//-----------------------------------------------------------------------------
void CTBCmdUI::SetText(LPCTSTR lpszText)
{
	m_strText = lpszText;
	m_UptateType |= CTBCmdUI::UpdateSetText;
}

