
#pragma once

#include <TbGeneric\DataObj.h>

#include "beginh.dex"

DataBool StartMailConnector		();
DataBool ConfigureSmtpParameter	();
DataBool ConfigureMailParameters();

DataBool PostaLiteSubscribe		();
DataBool GetPostaLiteDocument	(DataLng msgID, DataStr& tempFileName);

#include "endh.dex"
