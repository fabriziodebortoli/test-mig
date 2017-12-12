#include "stdafx.h"

#include <TbFrameworkImages\GeneralFunctions.h>
#include <TbFrameworkImages\CommonImages.h>

#include "TBRepositoryManager.h"
#include "SOSObjects.h"
#include "BDSOSAdjustAttachments.h"
#include "UISOSAdjustAttachments.h"

#include "EasyAttachment\JsonForms\UISOSAdjustAttachments\IDD_SOSADJUSTATTACH_WIZARD.hjson"
#include "EasyAttachment\JsonForms\UISOSAdjustAttachments\IDD_SOSADJUSTATTACH_TOOLBAR.hjson"

using namespace Microarea::TaskBuilderNet::Core::WebServicesWrapper;

//////////////////////////////////////////////////////////////////////////////
//						CSOSAdjustAttachmentsWizardFormView
//////////////////////////////////////////////////////////////////////////////
//-----------------------------------------------------------------------------
IMPLEMENT_DYNCREATE(CSOSAdjustAttachmentsWizardFormView, CWizardFormView)

//-----------------------------------------------------------------------------
BEGIN_MESSAGE_MAP(CSOSAdjustAttachmentsWizardFormView, CWizardFormView)
	//{{AFX_MSG_MAP(CSOSDocSenderWizardFormView)
	ON_WM_TIMER()
	//}}AFX_MSG_MAP
END_MESSAGE_MAP()

//-----------------------------------------------------------------------------
CSOSAdjustAttachmentsWizardFormView::CSOSAdjustAttachmentsWizardFormView()
	:
	CWizardFormView(_NS_VIEW("SOSAdjustAttachmentsView"), IDD_SOSADJUSTATTACH_WIZARD)
{
}

//-----------------------------------------------------------------------------
void CSOSAdjustAttachmentsWizardFormView::UpdateTitle()
{
	GetDocument()->SetTitle(_TB("SOS Adjust Attachments"));
}

//------------------------------------------------------------------------------
void CSOSAdjustAttachmentsWizardFormView::OnTimer(UINT nUI)
{
	// mettere la define nel BD!
	if (nUI == CHECK_SOSADJUSTATTACH_TIMER)
		GetDocument()->DoOnTimer();
}

/////////////////////////////////////////////////////////////////////////////
//			class CSOSAdjustAttachmentsResultsBodyEdit Implementation
/////////////////////////////////////////////////////////////////////////////
//=============================================================================
IMPLEMENT_DYNCREATE(CSOSAdjustAttachmentsResultsBodyEdit, CBodyEdit)

//-----------------------------------------------------------------------------	
CSOSAdjustAttachmentsResultsBodyEdit::CSOSAdjustAttachmentsResultsBodyEdit()
{
}