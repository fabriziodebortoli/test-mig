#include "StdAfx.h"

#include <TbGenlib\PARSOBJ.H>
#include "ParsedControlWrapper.h"

//--------------------------------------------------------------------------------
CParsedControlWrapper::CParsedControlWrapper(CParsedCtrl* pParsedCtrl)
	:
	m_pParsedCtrl(pParsedCtrl),
	ParsedControlWrapper((System::IntPtr)pParsedCtrl->GetCtrlCWnd()->m_hWnd)
{
}

//--------------------------------------------------------------------------------
CParsedControlWrapper::~CParsedControlWrapper(void)
{
}

//--------------------------------------------------------------------------------
void CParsedControlWrapper::UpdateView() 
{
	if (m_pParsedCtrl)
	{
		m_pParsedCtrl->UpdateCtrlStatus();
		m_pParsedCtrl->UpdateCtrlView();
	}
}