#pragma once

#include "TBProxy.h"
#include "resource.hjson"

namespace Microarea { namespace Framework	{ namespace TBApplicationWrapper
{
	ref class GlobalContext
	{
	public:
		static bool					Valid = true;
		static Diagnostic^			GlobalDiagnostic = gcnew Diagnostic("TBApplicationWrapper");
	};

	public ref class Commands
	{
	public:
		static System::IntPtr		CmdCreateJsonEditor = (System::IntPtr)(int)ID_CREATE_JSON_EDITOR;
	};
}}}