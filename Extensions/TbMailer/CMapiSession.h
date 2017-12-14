#pragma once
#include <TbGeneric/CMapi.h>
//includere alla fine degli include del .H
#include "beginh.dex"
//=======================================================================

TB_EXPORT IMapiSession* NewMapiSession(BOOL bMultiThreaded = TRUE, BOOL bNoLogonUI = FALSE, BOOL bNoInitializeMAPI = FALSE, CDiagnostic* pDiagnostic = NULL);

//=============================================================================
#include "endh.dex"
