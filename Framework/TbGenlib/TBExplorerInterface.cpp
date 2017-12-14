#include "stdafx.h"

#include <TBNameSolver\ApplicationContext.h>

#include "TbExplorerInterface.h"

#ifdef _DEBUG
#undef THIS_FILE
static const char THIS_FILE[] = __FILE__;
#endif

//-----------------------------------------------------------------------------
IMPLEMENT_DYNAMIC(CBaseDocumentExplorerDlg, CParsedDialog)

IMPLEMENT_DYNAMIC(ITBExplorer, CObject)

IMPLEMENT_DYNCREATE(CTBExplorerFactory, CObject)

//-----------------------------------------------------------------------------
TB_EXPORT CTBExplorerFactory* AfxGetTBExplorerFactory()
{
	return AfxGetApplicationContext()->GetObject<CTBExplorerFactory>(&CApplicationContext::GetExplorerFactory);
}

//-----------------------------------------------------------------------------
ITBExplorer* AfxCreateTBExplorer(ITBExplorer::TBExplorerType aType, const CTBNamespace& aNameSpace, BOOL bIsNew /*= FALSE*/, BOOL bOnlyStdAndAllusr /*= FALSE*/, BOOL bIsUsr /*= FALSE*/, BOOL bSaveForAdmin /*= FALSE*/)
{ 
	CTBExplorerFactory* pFactory = AfxGetTBExplorerFactory();
	if (!pFactory) return NULL;

	return pFactory->CreateInstance(aType, aNameSpace, bIsNew, bOnlyStdAndAllusr, bIsUsr, bSaveForAdmin);
}

//-----------------------------------------------------------------------------
ITBExplorer* CTBExplorerFactory::CreateInstance(ITBExplorer::TBExplorerType aType, const CTBNamespace& aNameSpace, BOOL bIsNew /*= FALSE*/, BOOL bOnlyStdAndAllusr /*= FALSE*/, BOOL bIsUsr /*= FALSE*/, BOOL bSaveForAdmin /*= FALSE*/)
{
	if (GetRuntimeClass() == RUNTIME_CLASS(ITBExplorer))
	{
		//TODO se possibile .... LoadLibary(TbGenlibUI)
	}
	CTBExplorerFactory* pFactory = AfxGetTBExplorerFactory();
	if (pFactory && pFactory != this)
		return pFactory->CreateInstance(aType,aNameSpace, bIsNew, bOnlyStdAndAllusr, bIsUsr, bSaveForAdmin);

	TRACE(_T("TbGenlibUI library is NOT registered: cannot create the TbExplorer object\n"));
	ASSERT(FALSE);
	return NULL;
}
//-----------------------------------------------------------------------------

