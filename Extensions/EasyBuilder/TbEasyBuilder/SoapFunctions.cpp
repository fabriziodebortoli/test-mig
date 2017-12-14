
#include "StdAfx.h"
#include "CDEasyBuilder.h"

#include ".\soapfunctions.h"

using namespace Microarea::EasyBuilder::Packager;
using namespace Microarea::TaskBuilderNet::Core::Generic;
using namespace Microarea::EasyBuilder::UI;
using namespace Microarea::EasyBuilder::DBScript;

using namespace System;
using namespace System::Threading;
using namespace System::Globalization;


//----------------------------------------------------------------------------
///<summary>
///Allow to open EasyStudio settings form
///</summary>
//[TBWebMethod(securityhidden=true, woorm_method=false)]
void OpenEasyBuilderSettings()
{
	Thread::CurrentThread->CurrentUICulture = gcnew CultureInfo(gcnew String(AfxGetCulture()));

	SafeThreadCallContext^ sc = gcnew SafeThreadCallContext();
	try
	{
		SettingForm^ settingForm = gcnew SettingForm(nullptr);
		settingForm->ShowDialog();
		delete settingForm;
		settingForm = nullptr;
	}
	finally
	{	
		delete sc;
	}
}

//----------------------------------------------------------------------------
///<summary>
///Allow to open EasyStudio settings form
///</summary>
//[TBWebMethod(securityhidden=true, woorm_method=false)]
void OpenMenuEditor()
{
	Thread::CurrentThread->CurrentUICulture = gcnew CultureInfo(gcnew String(AfxGetCulture()));

	SafeThreadCallContext^ sc = gcnew SafeThreadCallContext();
	try
	{
		if (!AfxIsActivated(TBEXT_APP, EASYSTUDIO_DESIGNER_ACT))
			return;

		HWND menuWindowHandle = AfxGetMenuWindowHandle();

		PostMessage(menuWindowHandle, UM_OPEN_MENUEDITOR, NULL, NULL);
	}
	finally
	{
		delete sc;
	}
}

//----------------------------------------------------------------------------
///<summary>
///Allow to open the Setup Studio
///</summary>
//[TBWebMethod(securityhidden=true, woorm_method=false)]
void OpenCustomizationManager()
{
	Thread::CurrentThread->CurrentUICulture = gcnew CultureInfo(gcnew String(AfxGetCulture()));

	SafeThreadCallContext^ sc = gcnew SafeThreadCallContext();
	try
	{
		HWND menuWindowHandle = AfxGetMenuWindowHandle();

		PostMessage(menuWindowHandle, UM_OPEN_CUSTOMIZATIONMANAGER, NULL, NULL);
	}
	finally
	{
		delete sc;
	}
}

//----------------------------------------------------------------------------
///<summary>
///Allow to apply catalog changes for EasyStudio Customizations
///</summary>
//[TBWebMethod(securityhidden=true, woorm_method=false)]
void UpdateCatalogIfNeeded()
{
	DatabaseChangesCurrentRelease::UpdateCatalogIfNeeded();
}