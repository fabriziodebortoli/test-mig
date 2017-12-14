#pragma once

#include "beginh.dex"
//=============================================================================

public ref class StaticFunctions
{
public:
	delegate bool ShowRowView(System::IntPtr hwndOwner, System::IntPtr documentPtr, System::IntPtr dbtPtr);
	
	static ShowRowView^ onShowRowView;
	
	static System::String^ GetFileFromJsonFileId(System::String ^ jsonFileId);

};
//----------------------------------------------------------------------------
class DataObj;
System::Object^ ConverDataObj(DataObj* pObj);
//=============================================================================

#include "endh.dex"