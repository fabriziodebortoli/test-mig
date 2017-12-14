
#pragma once 

#include <TBGENERIC\dataobj.h>

//includere alla fine degli include del .H
#include "beginh.dex"

CString OnGetSyncStatusBmp				(DataEnum aSynchStatus, DataStr aProvider = CRMInfinityProvider);
CString OnGetSyncDirectionBmp			(DataEnum aSynchDirection);

#include "endh.dex"