//BEGIN_HEADING - file version: 1.0
//WARNING - automatically generated code - DO NOT EDIT THIS FILE!

#include "stdafx.h"

//BEGIN_SOURCE_INCLUDE
#include	"soapfunctions.h"
//END_SOURCE_INCLUDE

#include <atlsafe.h>
#include <TbGeneric\WebServiceStateObjects.h>
#include <TBNameSolver\Templates.h>
//END_HEADING


namespace TbAuditingTbAuditing {

	//File: Extensions\TbAuditing\SoapFunctions.cpp.
	extern "C" __declspec(dllexport) bool __OpenAuditing(HWND _hwnd) throw(...)
	{

		DataBool returnValueParam;

		returnValueParam = AfxInvokeThreadGlobalFunction<DataBool>(_hwnd, &OpenAuditing);


		return returnValueParam.GetSoapValue();
	}

	//File: Extensions\TbAuditing\SoapFunctions.cpp.
	extern "C" __declspec(dllexport) bool __CloseAuditing(HWND _hwnd) throw(...)
	{

		DataBool returnValueParam;

		returnValueParam = AfxInvokeThreadGlobalFunction<DataBool>(_hwnd, &CloseAuditing);


		return returnValueParam.GetSoapValue();
	}
}

TB_REGISTER_SOAP_SERVICE(TbAuditingTbAuditing::CTbAuditingTbAuditing)