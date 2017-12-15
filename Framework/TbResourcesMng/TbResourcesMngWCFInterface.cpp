//BEGIN_HEADING - file version: 1.0
//WARNING - automatically generated code - DO NOT EDIT THIS FILE!

#include "stdafx.h"

//BEGIN_SOURCE_INCLUDE
#include	"tworkers.h"
#include	"rmfunctions.h"
//END_SOURCE_INCLUDE

#include <atlsafe.h>
#include <TbGeneric\WebServiceStateObjects.h>
#include <TBNameSolver\Templates.h>
//END_HEADING


namespace TbResourcesMngTbResourcesMng {
	//File: Framework\TbResourcesMng\RMFunctions.cpp.
	extern "C" __declspec(dllexport) bool __SetWorkerByName(HWND _hwnd, BSTR aUserName, BSTR aWorkerName, BSTR aWorkerLastName) throw(...)
	{
		DataBool returnValueParam;
		DataStr aUserNameParam;
		aUserNameParam.SetCollateCultureSensitive(TRUE);
		aUserNameParam.SetSoapValue(aUserName);
		DataStr aWorkerNameParam;
		aWorkerNameParam.SetCollateCultureSensitive(TRUE);
		aWorkerNameParam.SetSoapValue(aWorkerName);
		DataStr aWorkerLastNameParam;
		aWorkerLastNameParam.SetCollateCultureSensitive(TRUE);
		aWorkerLastNameParam.SetSoapValue(aWorkerLastName);
		returnValueParam = AfxInvokeThreadGlobalFunction<DataBool, DataStr, DataStr, DataStr>(_hwnd, &SetWorkerByName, aUserNameParam, aWorkerNameParam, aWorkerLastNameParam);
		return returnValueParam.GetSoapValue();
	}
	//File: Framework\TbResourcesMng\RMFunctions.cpp.
	extern "C" __declspec(dllexport) bool __SetWorkerByID(HWND _hwnd, BSTR aUserName, long aWorkerID) throw(...)
	{
		DataBool returnValueParam;
		DataStr aUserNameParam;
		aUserNameParam.SetCollateCultureSensitive(TRUE);
		aUserNameParam.SetSoapValue(aUserName);
		DataLng aWorkerIDParam;
		aWorkerIDParam.SetSoapValue(aWorkerID);
		returnValueParam = AfxInvokeThreadGlobalFunction<DataBool, DataStr, DataLng>(_hwnd, &SetWorkerByID, aUserNameParam, aWorkerIDParam);
		return returnValueParam.GetSoapValue();
	}

	//File: Framework\TbResourcesMng\TWorkers.cpp.
	extern "C" __declspec(dllexport) long __GetLoggedWorkerID(HWND _hwnd) throw(...)
	{

		DataLng returnValueParam;

		returnValueParam = AfxInvokeThreadGlobalFunction<DataLng>(_hwnd, &GetLoggedWorkerID);


		return returnValueParam.GetSoapValue();
	}

	//File: Framework\TbResourcesMng\TWorkers.cpp.
	extern "C" __declspec(dllexport) BSTR __GetWorkerName(HWND _hwnd, long WorkerID) throw(...)
	{

		DataStr returnValueParam;
		returnValueParam.SetCollateCultureSensitive(TRUE);

		DataLng WorkerIDParam;
		WorkerIDParam.SetSoapValue(WorkerID);

		returnValueParam = AfxInvokeThreadGlobalFunction<DataStr, DataLng>(_hwnd, &GetWorkerName, WorkerIDParam);


		return returnValueParam.GetSoapValue();
	}

	//File: Framework\TbResourcesMng\TWorkers.cpp.
	extern "C" __declspec(dllexport) BSTR __GetLoggedWorkerName(HWND _hwnd) throw(...)
	{

		DataStr returnValueParam;
		returnValueParam.SetCollateCultureSensitive(TRUE);

		returnValueParam = AfxInvokeThreadGlobalFunction<DataStr>(_hwnd, &GetLoggedWorkerName);


		return returnValueParam.GetSoapValue();
	}
}

TB_REGISTER_SOAP_SERVICE(TbResourcesMngTbResourcesMng::CTbResourcesMngTbResourcesMng)