#include "stdafx.h"

#include <TbFrameworkImages\GeneralFunctions.h>
#include <TbFrameworkImages\CommonImages.h>

#include "TBRepositoryManager.h"
#include "SOSObjects.h"
#include "BDSOSDocSender.h"
#include "UISOSDocSender.h"

#include "EasyAttachment\JsonForms\UISOSDocSender\IDD_SOSDOCSENDER_WIZARD.hjson"
#include "EasyAttachment\JsonForms\UISOSDocSender\IDD_SOSDOCSENDER_TOOLBAR.hjson"

using namespace Microarea::TaskBuilderNet::Core::WebServicesWrapper;

//////////////////////////////////////////////////////////////////////////////
//						CSOSDocSenderWizardFormView
//////////////////////////////////////////////////////////////////////////////
//-----------------------------------------------------------------------------
IMPLEMENT_DYNCREATE(CSOSDocSenderWizardFormView, CWizardFormView)

//-----------------------------------------------------------------------------
BEGIN_MESSAGE_MAP(CSOSDocSenderWizardFormView, CWizardFormView)
	//{{AFX_MSG_MAP(CSOSDocSenderWizardFormView)
	ON_WM_TIMER()
	//}}AFX_MSG_MAP
END_MESSAGE_MAP()

//-----------------------------------------------------------------------------
CSOSDocSenderWizardFormView::CSOSDocSenderWizardFormView()
	:
	CWizardFormView(_NS_VIEW("SOSDocSenderView"), IDD_SOSDOCSENDER_WIZARD_VIEW)
{
}

//-----------------------------------------------------------------------------
void CSOSDocSenderWizardFormView::UpdateTitle()
{
	GetDocument()->SetTitle(_TB("SOS Document Sender"));
}

//------------------------------------------------------------------------------
void CSOSDocSenderWizardFormView::OnTimer(UINT nUI)
{
	if (nUI == CHECK_SOSDOCSENDER_TIMER)
		GetDocument()->DoOnTimer();
}


/////////////////////////////////////////////////////////////////////////////
//			class CSOSDocSenderResultsBodyEdit Implementation
/////////////////////////////////////////////////////////////////////////////
//=============================================================================
IMPLEMENT_DYNCREATE(CSOSDocSenderResultsBodyEdit, CJsonBodyEdit)

//-----------------------------------------------------------------------------	
CSOSDocSenderResultsBodyEdit::CSOSDocSenderResultsBodyEdit()
{
}
