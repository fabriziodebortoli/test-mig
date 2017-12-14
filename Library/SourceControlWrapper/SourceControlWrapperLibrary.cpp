#include "StdAfx.h"
#include <windows.h>
#include <_vcclrit.h>

#include ".\sourcecontrolwrapperlibrary.h"
#using <mscorlib.dll>

int CSourceControlWrapperLibrary::refCount = 0;

//--------------------------------------------------------------------------------
CSourceControlWrapperLibrary::CSourceControlWrapperLibrary(void)
{
	if (refCount++ == 0)
		__crt_dll_initialize();
}

//--------------------------------------------------------------------------------

CSourceControlWrapperLibrary::~CSourceControlWrapperLibrary(void)
{
	if (--refCount == 0)
		__crt_dll_terminate();
}
