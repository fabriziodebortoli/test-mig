
#include "stdafx.h"

#include "UIResourcesLayoutAction.h"

#include "JsonForms\ResourcesLayoutAction\IDD_RESOURCES_LAYOUT_ACTION.hjson"
#include "JsonForms\ResourcesLayoutAction\IDD_RESOURCES_LAYOUT_ACTION_TOOLBAR.hjson"

#ifdef _DEBUG
#undef THIS_FILE
static char THIS_FILE[] = __FILE__;
#endif

/////////////////////////////////////////////////////////////////////////////
//		class CResourcesLayoutActionSlaveView Implementation
/////////////////////////////////////////////////////////////////////////////
//-----------------------------------------------------------------------------
IMPLEMENT_DYNCREATE(CResourcesLayoutActionSlaveView, CJsonSlaveFormView)

//-----------------------------------------------------------------------------
BEGIN_MESSAGE_MAP(CResourcesLayoutActionSlaveView, CJsonSlaveFormView)
	ON_COMMAND	(ID_RESOURCES_LAYOUT_ACTION_MOVE,	OnMoveClick)
	ON_COMMAND	(ID_RESOURCES_LAYOUT_ACTION_COPY,	OnCopyClick)
	ON_COMMAND	(ID_RESOURCES_LAYOUT_ACTION_CANCEL,	OnCancelClick)
END_MESSAGE_MAP()

//-----------------------------------------------------------------------------
CResourcesLayoutActionSlaveView::CResourcesLayoutActionSlaveView()
{}

//-----------------------------------------------------------------------------
BDResourcesLayout* CResourcesLayoutActionSlaveView::GetDocument () const { return (BDResourcesLayout*) m_pDocument; }

//-----------------------------------------------------------------------------
void CResourcesLayoutActionSlaveView::OnMoveClick()
{
	if (m_pDocument)
		GetDocument()->m_DragDropAction = DRAG_DROP_MOVE;
	GetParentFrame()->PostMessage(WM_CLOSE);
}

//-----------------------------------------------------------------------------
void CResourcesLayoutActionSlaveView::OnCopyClick()
{
	if (m_pDocument)
		GetDocument()->m_DragDropAction = DRAG_DROP_COPY;
	GetParentFrame()->PostMessage(WM_CLOSE);
}

//-----------------------------------------------------------------------------
void CResourcesLayoutActionSlaveView::OnCancelClick()
{
	if (m_pDocument)
		GetDocument()->m_DragDropAction = DRAG_DROP_CANCEL;
	GetParentFrame()->PostMessage(WM_CLOSE);
}
