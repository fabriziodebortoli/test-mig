#include "StdAfx.h"
#include "TBToolTip.h"

//----------------------------------------------------------------------------
LRESULT TBToolTip::DefWindowProc(UINT message, WPARAM wParam, LPARAM lParam)
{
	switch (message)
	{
	case TTM_ACTIVATE:
	case TTM_SETDELAYTIME:
	case TTM_ADDTOOLA:
	case TTM_ADDTOOLW:
	case TTM_DELTOOLA:
	case TTM_DELTOOLW:
	case TTM_NEWTOOLRECTA:
	case TTM_NEWTOOLRECTW:
	case TTM_RELAYEVENT:
	case TTM_GETTOOLINFOA:
	case TTM_GETTOOLINFOW:
	case TTM_SETTOOLINFOA:
	case TTM_SETTOOLINFOW:
	case TTM_HITTESTA:
	case TTM_HITTESTW:
	case TTM_GETTEXTA:
	case TTM_GETTEXTW:
	case TTM_UPDATETIPTEXTA:
	case TTM_UPDATETIPTEXTW:
	case TTM_GETTOOLCOUNT:
	case TTM_ENUMTOOLSA:
	case TTM_ENUMTOOLSW:
	case TTM_GETCURRENTTOOLA:
	case TTM_GETCURRENTTOOLW:
	case TTM_WINDOWFROMPOINT:
	case TTM_TRACKACTIVATE:
	case TTM_TRACKPOSITION:
	case TTM_SETTIPBKCOLOR:
	case TTM_SETTIPTEXTCOLOR:
	case TTM_GETDELAYTIME:
	case TTM_GETTIPBKCOLOR:
	case TTM_GETTIPTEXTCOLOR:
	case TTM_SETMAXTIPWIDTH:
	case TTM_GETMAXTIPWIDTH:
	case TTM_SETMARGIN:
	case TTM_GETMARGIN:
	case TTM_POP:
	case TTM_UPDATE:
	case TTM_GETBUBBLESIZE:
	case TTM_ADJUSTRECT:
	case TTM_SETTITLEA:
	case TTM_SETTITLEW:
	case TTM_POPUP:
	case TTM_GETTITLE:
		return 0L;
	default:
	{
		return __super::DefWindowProc(message, wParam, lParam);
	}
	}
}