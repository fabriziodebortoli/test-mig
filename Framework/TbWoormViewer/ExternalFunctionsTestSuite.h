
#pragma once

#include <TbGeneric\DataObj.h>

#include "beginh.dex"

/*TBWebMethod*/	DataLng TestDataLngParameter(DataLng longParamIn, DataLng& longParamInOut);

/*TBWebMethod*/	DataStr TestDataStrParameter(DataStr strParamIn, DataStr& strParamInOut);


/*TBWebMethod*/ DataArray /*string*/ TestDataArrayParameter(
	DataArray /*integer*/arIntegerParamIn,
	DataArray& /*integer*/arIntegerParamInOut,
	DataArray /*string*/arStringParamIn,
	DataArray& /*string*/arStringParamInOut
);


//=============================================================================
#include "endh.dex"