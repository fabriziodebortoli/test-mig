
#include "stdafx.h"

#include "DResources.h"  
#include "UIResources.h"  

#include "ModuleObjects\Resources\JsonForms\IDD_RESOURCES.hjson"

#ifdef _DEBUG
#undef THIS_FILE
static char THIS_FILE[] = __FILE__;
#endif

//==============================================================================
//	CResourcesView
//==============================================================================
//-----------------------------------------------------------------------------
IMPLEMENT_DYNCREATE(CResourcesView, CJsonFormView)

//-----------------------------------------------------------------------------
CResourcesView::CResourcesView()
{
}

//=============================================================================
//             class CResourcesFieldsBodyEdit implementation
//=============================================================================
//-----------------------------------------------------------------------------	
IMPLEMENT_DYNCREATE(CResourcesFieldsBodyEdit, CJsonBodyEdit)

//-----------------------------------------------------------------------------	
void CResourcesFieldsBodyEdit::Customize()
{
	__super::Customize();
}

//------------------------------------------------------------------------------
BOOL CResourcesFieldsBodyEdit::OnGetToolTipProperties(CBETooltipProperties* pTooltip)
{	
	TResourcesFields* pRec = (TResourcesFields*)(pTooltip->m_pRec);	

	if (pTooltip->m_nControlID == IDC_RSS_FIELDS_NAME)
	{
		pTooltip->m_strTitle.Empty();
		pTooltip->m_strText.Empty();
	}
	else if (pTooltip->m_nControlID == IDC_RSS_FIELDS_VALUE)
	{
		pTooltip->m_strTitle = pRec->f_FieldName.GetString();
		pTooltip->m_strText = pRec->f_FieldValue.GetString();
	}
	else
		return __super::OnGetToolTipProperties(pTooltip);

	return TRUE;
}

//-----------------------------------------------------------------------------	
BOOL CResourcesFieldsBodyEdit::OnGetCustomColor(CBodyEditRowSelected* pCurrentRow)
{
	ASSERT(pCurrentRow->m_pRec->IsKindOf(RUNTIME_CLASS(TResourcesFields)));

	if (pCurrentRow->m_nColumnIDC == IDC_RSS_FIELDS_NAME) 
	{
		pCurrentRow->m_crBkgColor	= RGB(236, 233, 216);
		pCurrentRow->m_crTextColor	= RGB(0, 0, 0);
	}
	return TRUE;
}
