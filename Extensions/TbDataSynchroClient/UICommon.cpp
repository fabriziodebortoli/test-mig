#include "stdafx.h"

#include <TbGeneric\\DataObj.h>
#include "TbDataSynchroClientEnums.h"
#include "UICommon.hrc"

#include <TbFrameworkImages\CommonImages.h>
#include <TbFrameworkImages\GeneralFunctions.h>

static TCHAR szNamespace[] = _T("Image.Framework.TbFrameworkImages.Images.%s.%s.png");

static TCHAR szGlyphF[] = _T("Glyph");


//-----------------------------------------------------------------------------
CString OnGetSyncStatusBmp(DataEnum aSynchStatus, DataStr aProvider)
{
	int		nResult;
	TCHAR	bufferOk[512];
	TCHAR	bufferWait[512];
	TCHAR	bufferError[512];
	TCHAR	bufferExcluded[512];
	TCHAR	bufferDMSOk[512];
	TCHAR	bufferDMSError[512];

	nResult = swprintf_s(bufferOk, szNamespace, szGlyphF, szGlyphOk);
	nResult = swprintf_s(bufferWait, szNamespace, szGlyphF, szGlyphWait);
	nResult = swprintf_s(bufferError, szNamespace, szGlyphF, szIconError);
	nResult = swprintf_s(bufferExcluded, szNamespace, szGlyphF, szGlyphRemove);
	nResult = swprintf_s(bufferDMSOk, szNamespace, szGlyphF, szIconDMSOk);
	nResult = swprintf_s(bufferDMSError, szNamespace, szGlyphF, szIconDMSError);


	if (aProvider == _T("DMSInfinity"))
	{ 
		if (aSynchStatus == E_SYNCHROSTATUS_TYPE_SYNCHRO)
			return bufferDMSOk;
		if (aSynchStatus == E_SYNCHROSTATUS_TYPE_ERROR)
			return bufferDMSError;
	}
		
	if (aSynchStatus == E_SYNCHROSTATUS_TYPE_NOSYNCHRO || aSynchStatus == E_SYNCHROSTATUS_TYPE_WAIT)
		return bufferWait;	// IDB_DATASYNCHRO_SYNCHSTATUSWAIT_SMALL;
	else if (aSynchStatus == E_SYNCHROSTATUS_TYPE_SYNCHRO)
		return bufferOk;		// IDB_DATASYNCHRO_SYNCHSTATUSOK_SMALL;
	else if (aSynchStatus == E_SYNCHROSTATUS_TYPE_ERROR)
		return bufferError;		//  IDB_DATASYNCHRO_SYNCHSTATUSERROR_SMALL;
	else if (aSynchStatus == E_SYNCHROSTATUS_TYPE_EXCLUDED)
		return bufferExcluded;		// IDB_DATASYNCHRO_SYNCHSTATUSEXCLUDED_SMALL;
	else
		return bufferWait;			// IDB_DATASYNCHRO_SYNCHSTATUSWAIT_SMALL;
}

//-----------------------------------------------------------------------------
CString OnGetSyncDirectionBmp(DataEnum aSynchDirection)
{
	int		nResult;
	TCHAR	bufferIn[512];
	TCHAR	bufferOut[512];

	nResult = swprintf_s(bufferIn, szNamespace, szGlyphF, szGlyphInbound);
	nResult = swprintf_s(bufferOut, szNamespace, szGlyphF, szGlyphOutbound);

	if (aSynchDirection == E_SYNCHRODIRECTION_TYPE_OUTBOUND)
		return bufferOut;	//IDB_DATASYNCHRO_SYNCHDIRECTION_OUTBOUND_SMALL;
	else
		return bufferIn;	//IDB_DATASYNCHRO_SYNCHDIRECTION_INBOUND_SMALL;
}