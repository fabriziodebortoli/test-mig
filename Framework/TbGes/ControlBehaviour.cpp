#include "stdafx.h"
#include <TbGeneric\DataObj.h>
#include <TbGenlib\PARSOBJ.H>
#include "EXTDOC.H"
#include "ControlBehaviour.h"

IMPLEMENT_DYNCREATE(CControlBehaviour, CObject)
//-----------------------------------------------------------------------------
CControlBehaviour::CControlBehaviour()
{
}

//-----------------------------------------------------------------------------
CControlBehaviour::~CControlBehaviour()
{
}

//-----------------------------------------------------------------------------
void CControlBehaviour::NotifyValueChanged()
{
	if (!m_pDocument)
	{
		ASSERT(FALSE);
		return;
	}
	for (int i = 0; i < m_arControlIDs.GetCount(); i++)
	{
		m_pDocument->OnCmdMsg(m_arControlIDs[i], EN_VALUE_CHANGED, NULL, NULL);
	}
}

//-----------------------------------------------------------------------------
BOOL CControlBehaviour::OnCmdMsg(UINT nID, int nCode, void* pExtra, AFX_CMDHANDLERINFO* pHandlerInfo)
{
	if (nCode == EN_VALUE_CHANGED)
	{
		for (int i = 0; i < m_arControlIDs.GetCount(); i++)
		{
			if (nID == m_arControlIDs[i])
				OnValueChanged();
		}
	}
	return __super::OnCmdMsg(nID, nCode, pExtra, pHandlerInfo);
}

//-----------------------------------------------------------------------------
void CControlBehaviour::ReceiveInfo(CJsonParser& json)
{
	OnReceiveInfo(json);
}
