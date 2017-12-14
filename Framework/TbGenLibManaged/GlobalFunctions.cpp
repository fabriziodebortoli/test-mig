#include "stdafx.h"
#include <TbNameSolver\ThreadContext.h>
#include <TbNameSolver\LoginContext.h>
#include <TBGeneric\TbStrings.h>
#include "GlobalFunctions.h"
#include "StaticFunctions.h"

using namespace System;
using namespace System::Collections;
using namespace System::Globalization;
using namespace System::IO;
using namespace Microarea::Library::TBApplicationWrapper;

//--------------------------------------------------------------------------------
BOOL LoadInstalledLanguages (CStringArray& arNames, CStringArray& arDescriptions)
{
	array<CultureInfo^>^ cultures = Microarea::TaskBuilderNet::Core::Generic::InstallationData::GetInstalledDictionaries();

	arNames.RemoveAll();	arDescriptions.RemoveAll();
	for (int i=0; i < cultures->Length; i++)
	{
		CultureInfo^ c = cultures[i];
		CString s = c->Name;
		arNames.Add(s);
		s = c->DisplayName;
		if (c->Name == String::Empty) 
			arDescriptions.Add('<' + _TB("Native language") + '>');
		else
			arDescriptions.Add(s);
	}
	return TRUE;
}

//--------------------------------------------------------------------------------
CString ConvertToBase64(const CString& sFileName)
{
	return Microarea::TaskBuilderNet::Core::Generic::Functions::ConvertToBase64(gcnew System::String(sFileName));
}

CString ConvertToBase64Str(const CString& cmd)
{
	return Microarea::TaskBuilderNet::Core::Generic::Functions::ConvertToBase64Str(gcnew System::String(cmd));
}


CString ConvertToAESStr(const CString& cmd)
{
	return Microarea::TaskBuilderNet::Core::Generic::Functions::ConvertToAESStr(gcnew System::String(cmd));
}

//--------------------------------------------------------------------------------
CString HTTPGet(const CString& sUrl)
{
	return Microarea::TaskBuilderNet::Core::Generic::HttpHelper::HttpGet(gcnew String(sUrl));
}

//--------------------------------------------------------------------------------
CString OpenCrsFile(const CString& sFileName)
{
	return Microarea::TaskBuilderNet::Core::Generic::Functions::OpenCrsFile(gcnew System::String(sFileName));
}

//----------------------------------------------------------------------------
bool CallDynamicRowFormView(HWND hwndOwner, void* pDocument, void* pDBT)
{
	return StaticFunctions::onShowRowView ? StaticFunctions::onShowRowView((IntPtr)hwndOwner, (IntPtr)pDocument, (IntPtr)pDBT) : false;
}

//----------------------------------------------------------------------------
CString StringFormat(CString formatString, CStringArray& args)
{
	System::Collections::Generic::List<System::String^>^ list = gcnew System::Collections::Generic::List<System::String^>();

	for (int i = 0; i < args.GetCount(); i++)
		list->Add(gcnew System::String(args.GetAt(i)));

	return CString(System::String::Format(gcnew System::String(formatString), list->ToArray()));
}
