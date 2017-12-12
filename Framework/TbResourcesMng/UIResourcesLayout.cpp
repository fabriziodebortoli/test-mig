#include "stdafx.h"

#include "BDResourcesLayout.h"
#include "UIResourcesLayout.h"

#include "ModuleObjects\ResourcesLayout\JsonForms\IDD_RESOURCES_LAYOUT_TOOLBAR.hjson"

#ifdef _DEBUG
#undef THIS_FILE                                                        
static char THIS_FILE[] = __FILE__;     
#endif                                

////////////////////////////////////////////////////////////////////////////////////////
//			class CResourcesLayoutTreeVView implementation
////////////////////////////////////////////////////////////////////////////////////////
//---------------------------------------------------------------------------------------
IMPLEMENT_DYNCREATE(CResourcesLayoutTreeVView, CJsonFormView)

//----------------------------------------------------------------------------------------
CResourcesLayoutTreeVView::CResourcesLayoutTreeVView()
	:
	CJsonFormView(IDD_RESOURCES_LAYOUT_TREE_VIEW)
{
}

//////////////////////////////////////////////////////////////////////////////////////
//				class CResourcesLayoutDetailsView implementation
//////////////////////////////////////////////////////////////////////////////////////
//------------------------------------------------------------------------------------
IMPLEMENT_DYNCREATE(CResourcesLayoutDetailsView, CJsonFormView)

//------------------------------------------------------------------------------------
CResourcesLayoutDetailsView::CResourcesLayoutDetailsView()
	:	
	CJsonFormView(IDD_RESOURCES_LAYOUT_DETAILS_VIEW)
{
}