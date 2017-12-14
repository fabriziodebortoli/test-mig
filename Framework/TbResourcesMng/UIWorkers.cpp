
#include "stdafx.h"

#include "TWorkers.h"
#include "UIWorkers.h"  

#include "ModuleObjects\Workers\JsonForms\IDD_WORKERS.hjson"

#ifdef _DEBUG
#undef THIS_FILE
static char THIS_FILE[] = __FILE__;
#endif

//==============================================================================
//	CWorkersView
//==============================================================================
//-----------------------------------------------------------------------------
IMPLEMENT_DYNCREATE(CWorkersView, CJsonFormView)

//-----------------------------------------------------------------------------
CWorkersView::CWorkersView()
{
}

//=============================================================================
//             class CWorkersFieldsBodyEdit implementation
//=============================================================================
//-----------------------------------------------------------------------------	
IMPLEMENT_DYNCREATE(CWorkersFieldsBodyEdit, CJsonBodyEdit)

//-----------------------------------------------------------------------------	
void CWorkersFieldsBodyEdit::Customize()
{
	__super::Customize();
}

//------------------------------------------------------------------------------
BOOL CWorkersFieldsBodyEdit::OnGetToolTipProperties(CBETooltipProperties* pTooltip)
{
	TWorkersFields* pRec = (TWorkersFields*)(pTooltip->m_pRec);

	if (pTooltip->m_nControlID == IDC_WRK_FIELDS_BE_NAME)
	{
		pTooltip->m_strTitle.Empty();
		pTooltip->m_strText.Empty();
	}
	else if (pTooltip->m_nControlID == IDC_WRK_FIELDS_BE_VALUE)
	{
		pTooltip->m_strTitle = pRec->f_FieldName.GetString();
		pTooltip->m_strText = pRec->f_FieldValue.GetString();
	}
	else
	{
		return __super::OnGetToolTipProperties(pTooltip);
	}
	return TRUE;
}

//-----------------------------------------------------------------------------	
BOOL CWorkersFieldsBodyEdit::OnGetCustomColor(CBodyEditRowSelected* pCurrentRow)
{
	ASSERT(pCurrentRow->m_pRec->IsKindOf(RUNTIME_CLASS(TWorkersFields)));

	if (pCurrentRow->m_nColumnIDC == IDC_WRK_FIELDS_BE_NAME)
	{
		pCurrentRow->m_crBkgColor	= RGB(236, 233, 216);
		pCurrentRow->m_crTextColor	= RGB(0, 0, 0);
	}
	return TRUE;
}