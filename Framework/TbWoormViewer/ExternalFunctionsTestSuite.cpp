#include "stdafx.h"
#include "ExternalFunctionsTestSuite.h"


//----------------------------------------------------------------------------
///<summary>
///Test DataLng param
///</summary>
//[TBWebMethod(securityhidden=true, woorm_method=false)]
DataLng TestDataLngParameter(DataLng longParamIn, DataLng& longParamInOut)
{
	longParamIn++;
	longParamInOut++;
	return longParamIn;
}

//----------------------------------------------------------------------------
///<summary>
///Test DataStr param
///</summary>
//[TBWebMethod(securityhidden=true, woorm_method=false)]
DataStr TestDataStrParameter(DataStr strParamIn, DataStr& strParamInOut)
{
	strParamIn = strParamIn + _T("_modifiedByExternalFunction");
	strParamInOut = strParamInOut + _T("_modifiedByExternalFunction");
	return strParamIn;
}

//----------------------------------------------------------------------------
///<summary>
///Test DataArray param
///</summary>
//[TBWebMethod(securityhidden=true, woorm_method=false)]
DataArray /*string*/ TestDataArrayParameter(
	DataArray /*integer*/arIntegerParamIn,
	DataArray& /*integer*/arIntegerParamInOut,
	DataArray /*string*/arStringParamIn,
	DataArray& /*string*/arStringParamInOut
)
{
	arIntegerParamIn.RemoveAt(arIntegerParamIn.GetSize() - 1);
	arIntegerParamInOut.RemoveAt(arIntegerParamInOut.GetSize() - 1);
	arStringParamIn.RemoveAt(arStringParamIn.GetSize() - 1);
	arStringParamInOut.RemoveAt(arStringParamInOut.GetSize() - 1);

	arStringParamIn.GetAt(0)->Assign(_T("modified"));
	arStringParamInOut.GetAt(0)->Assign(_T("modified"));

	
	return arStringParamIn;
}


