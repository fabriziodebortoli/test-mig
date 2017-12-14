
#pragma once

//includere alla fine degli include del .H
#include "beginh.dex"

#ifdef _DEBUG
#define TRACETIME(mess)\
{\
	SYSTEMTIME	_t;\
	GetSystemTime(&_t);\
	DebugPrintf("%s %d:%d:%d:%d\n",(LPCTSTR) mess, _t.wHour, _t.wMinute, _t.wSecond, _t.wMilliseconds );\
}

#define BEGINTRACETIME()\
{\
	SYSTEMTIME	_tBegin;\
	SYSTEMTIME	_tEnd;\
	GetSystemTime(&_tBegin);\
	long nSecBegin = (_tBegin.wHour * 60 + _tBegin.wMinute * 60 + _tBegin.wSecond) * 1000 + _tBegin.wMilliseconds;\

#define ENDTRACETIME(mess)\
	GetSystemTime(&_tEnd);\
	long nSecEnd = (_tEnd.wHour * 60 + _tEnd.wMinute * 60 + _tEnd.wSecond) * 1000 + _tEnd.wMilliseconds;\
	if (nSecEnd - nSecBegin > 0) \
		DebugPrintf(_T("%s %ld msec\n"),(LPCTSTR)  mess, nSecEnd - nSecBegin);\
}

#else
#define TRACETIME(mess)
#define BEGINTRACETIME()
#define ENDTRACETIME(mess)
#endif

#include "endh.dex"
