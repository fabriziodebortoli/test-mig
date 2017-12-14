#include "stdafx.h"

#include <TBNameSolver\ApplicationContext.h>

#include "TbRadarInterface.h"

#ifdef _DEBUG
#undef THIS_FILE
static const char THIS_FILE[] = __FILE__;
#endif
//-----------------------------------------------------------------------------

IMPLEMENT_DYNAMIC(CTBRadarFactory, CObject)

CTBRadarFactory* AfxGetTBRadarFactory()
{
	return AfxGetApplicationContext()->GetObject<CTBRadarFactory>(&CApplicationContext::GetRadarFactory);
}

//-----------------------------------------------------------------------------
ITBRadar* AfxCreateTBRadar(HotKeyLink* pH, SqlTable* pT, SqlRecord* pR, HotKeyLinkObj::SelectionType nQuerySelection)
{ 
	return AfxGetTBRadarFactory()->CreateInstance(pH, pT, pR, nQuerySelection);
}

//-----------------------------------------------------------------------------
ITBRadar* AfxCreateTBRadar(CAbstractFormDoc* pDoc)
{ 
	return AfxGetTBRadarFactory()->CreateInstance(pDoc);
}

//-----------------------------------------------------------------------------
ITBRadar* AfxCreateTBRadar(CAbstractFormDoc* pDoc, const CString& sReportName, BOOL bTemporary)
{ 
	return AfxGetTBRadarFactory()->CreateInstance(pDoc, sReportName, bTemporary);
}


//-----------------------------------------------------------------------------
ITBRadar* CTBRadarFactory::CreateInstance(HotKeyLink* pH, SqlTable* pT, SqlRecord* pR, HotKeyLinkObj::SelectionType nQuerySelection)
{
	if (GetRuntimeClass() == RUNTIME_CLASS(CTBRadarFactory))
	{
		//TODO se possibile .... LoadLibary(TbRadar)
		ASSERT(FALSE);
		if (AfxGetTBRadarFactory() != this)
			return AfxGetTBRadarFactory()->CreateInstance(pH, pT, pR,  nQuerySelection);
	}

	TRACE("CTBRadarFactory::CreateInstance: TbRadar template isn't registered. It's impossible the TbRadar creation\n");
	ASSERT(FALSE);
	return NULL;
}

//-----------------------------------------------------------------------------
ITBRadar* CTBRadarFactory::CreateInstance(CAbstractFormDoc* pD)
{
	if (GetRuntimeClass() == RUNTIME_CLASS(CTBRadarFactory))
	{
		//TODO se possibile .... LoadLibary(TbRadar)
		ASSERT(FALSE);
		if (AfxGetTBRadarFactory() != this)
			return AfxGetTBRadarFactory()->CreateInstance(pD);
	}

	TRACE("CTBRadarFactory::CreateInstance: TbRadar template isn't registered. It's impossible the TbRadar creation\n");
	ASSERT(FALSE);
	return NULL;
}
//-----------------------------------------------------------------------------
ITBRadar* CTBRadarFactory::CreateInstance(CAbstractFormDoc* pDoc, const CString& sReportName, BOOL bTemporary)
{
	if (GetRuntimeClass() == RUNTIME_CLASS(CTBRadarFactory))
	{
		//TODO se possibile .... LoadLibary(TbRadar)
		ASSERT(FALSE);
		if (AfxGetTBRadarFactory() != this)
			return AfxGetTBRadarFactory()->CreateInstance(pDoc, sReportName, bTemporary);
	}

	TRACE("CTBRadarFactory::CreateInstance: TbRadar template isn't registered. It's impossible the TbRadar creation\n");
	ASSERT(FALSE);
	return NULL;
}
