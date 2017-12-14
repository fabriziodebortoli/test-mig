#include "StdAfx.h"

#include <TbGes\EventMng.H>
#include <TbWoormViewer\WOORMDOC.H>
#include <TBGes\ExtDoc.h>
#include <TBGes\DBT.h>
#include <TBGes\XMLGesInfo.h>
#include <TBGes\BODYEDIT.H>
#include <TBGes\Tabber.h>
#include <TBGes\TBRadarInterface.h>
#include "BusinessObjectParams.h"

/////////////////////////////////////////////////////////////////////////////
// 				class CBusinessObjectInvocationInfo Implementation
/////////////////////////////////////////////////////////////////////////////
//----------------------------------------------------------------------------	
CBusinessObjectInvocationInfo::CBusinessObjectInvocationInfo (bool bIsExposing)
	:
	m_pCaller(NULL)
{
	isExposing = bIsExposing == true;
}

//-----------------------------------------------------------------------------
CBusinessObjectInvocationInfo::~CBusinessObjectInvocationInfo()
{
	SAFE_DELETE(m_pCaller);
}

//-----------------------------------------------------------------------------
void CBusinessObjectInvocationInfo::CreateNewDocumentOf(CBaseDocument* pDoc)
{
	if (IsExposing())
		pDoc->SetDesignMode(CBaseDocument::DM_RUNTIME);
}

//-----------------------------------------------------------------------------
void CBusinessObjectInvocationInfo::SetCaller(Microarea::TaskBuilderNet::Core::EasyBuilder::EasyBuilderComponent^ caller)
{
	SAFE_DELETE(m_pCaller);
	if (caller != nullptr)
	{
		m_pCaller = new CBusinessObjectComponent();
		m_pCaller->Component = caller;
	}
}

