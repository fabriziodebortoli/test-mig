// NewDocument.cpp : implementation file
//

#include "stdafx.h"
	
#include "NewDocument.h"
#include "CDEasyBuilder.h"
#include "TBEasyBuilder.h"
#include "CDEasyBuilder.hjson" //JSON AUTOMATIC UPDATE


using namespace Microarea::TaskBuilderNet::Core::EasyBuilder;
using namespace Microarea::EasyBuilder::UI;
using namespace Microarea::EasyBuilder::Packager;
using namespace Microarea::Framework::TBApplicationWrapper;

using namespace System;
using namespace System::Threading;
using namespace System::Globalization;
using namespace System::Windows::Forms;
// CNewDocument

/////////////////////////////////////////////////////////////////////////////////////////////////
///										CNewDocument
////////////////////////////////////////////////////////////////////////////////////////////////

IMPLEMENT_DYNCREATE(CNewDocument, CDynamicFormDoc)

//--------------------------------------------------------------------------------
CNewDocument::CNewDocument()
{
}

//--------------------------------------------------------------------------------
CNewDocument::~CNewDocument()
{
}

//--------------------------------------------------------------------------------
BOOL CNewDocument::InitDocument()
{
	Thread::CurrentThread->CurrentUICulture = gcnew CultureInfo(gcnew String(AfxGetCulture()));
	
	return __super::InitDocument();
}

//--------------------------------------------------------------------------------
void CNewDocument::OnFrameCreated ()
{
	if (!CDEasyBuilder::IsLicenseForEasyBuilderVerified())
	{
		PostMessage(WM_CLOSE, 0, 0);
		return;
	}

	if (!BaseCustomizationContext::CustomizationContextInstance->ExistsCurrentEasyBuilderApp)
	{
		PostMessage(WM_CLOSE, 0, 0);
		return;
	}
	String^ title;
	NativeWindow^ owner = gcnew NativeWindow();
	owner->AssignHandle(((System::IntPtr)((int)AfxGetMenuWindowHandle())));
	//solo dopo che la frame è stata creata posso entrare in editing della customizzazione
	//chiedo all'utente di indicare un modulo di customizzazione se non ne esiste uno
	//se ho creato correttamente il documento, entro in editing, altrimenti esco
	if (
		NewDocument::SaveNewDocument(
			owner,
			gcnew MDocument((System::IntPtr)(int)this),
			nullptr,
			false,
			m_bBatch==TRUE,
			title,
			nullptr
			)
		)
	{
		SetTitle(CString(title));
		PostMessage(WM_COMMAND, ID_FORM_EDITOR_EDIT, 0);
	}
	else
		PostMessage(WM_CLOSE, 0, 0);

	owner->ReleaseHandle();
}



BEGIN_MESSAGE_MAP(CNewDocument, CDynamicFormDoc)
END_MESSAGE_MAP()



/////////////////////////////////////////////////////////////////////////////////////////////////
///										CNewBatchDocument
////////////////////////////////////////////////////////////////////////////////////////////////
IMPLEMENT_DYNCREATE(CNewBatchDocument, CNewDocument)

