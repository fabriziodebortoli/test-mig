#pragma once

#include <shlwapi.h>

#define TB_TCSCPY(a,b)		_tcscpy_s(a, _tcslen(b) + 1, b)
#define TB_TCSNCPY(a, b, n)	_tcsncpy_s(a, n + 1, b, n)
#define TB_WCSNCPY(a, b, n)	wcsncpy_s(a, n + 1, b, n)
#define TB_WCSCPY(a, b)		wcscpy_s(a, _tcslen(b) + 1, b)
#define TB_STRCPY(a, b)		strcpy_s(a, strlen(b) + 1, b)
#define TB_STRNCPY(a, b, n)	strncpy_s(a, n + 1, b, n)
#define TB_TCSCAT(a, b)		_tcscat_s(a, _tcslen(a) + _tcslen(b) + 1, b)

//----

#define TB_OLD_METHOD AFX_DEPRECATED("This method should not be used, use the code in its body instead (see inline implementation)")

#include "templates.h"
#include "ApplicationContext.h"

#ifdef _DEBUG

#ifdef AfxAssertFailedLine
#undef AfxAssertFailedLine
#endif

#define new DEBUG_NEW
#define DataObjCreate(dt) DataObjCreate(dt, THIS_FILE, __LINE__)
#define DataObjClone() DataObjClone(THIS_FILE, __LINE__)

#endif

// ASSERT + TRACE macros
#define ASSERT_TRACE(f,sz)\
{\
	if (!(f))\
	{\
		TRACE1("\nASSERT(%s) Failed ",CString(#f));\
		TRACE3("in %s (%s line %d):\n\t",CString(__FUNCTION__),CString(__FILE__),__LINE__);\
		TRACE(sz);\
		TRACE("\n\n");\
		ASSERT(FALSE);\
	}\
}

#define ASSERT_TRACE1(f,sz,p1)\
{\
	if (!(f))\
	{\
		TRACE1("\nASSERT(%s) Failed ",CString(#f));\
		TRACE3("in %s (%s line %d):\n\t",CString(__FUNCTION__),CString(__FILE__),__LINE__);\
		TRACE1(sz,p1);\
		TRACE("\n\n");\
		ASSERT(FALSE);\
	}\
}

#define ASSERT_TRACE2(f,sz,p1,p2)\
{\
	if (!(f))\
	{\
		TRACE1("\nASSERT(%s) Failed ",CString(#f));\
		TRACE3("in %s (%s line %d):\n\t",CString(__FUNCTION__),CString(__FILE__),__LINE__);\
		TRACE2(sz,p1,p2);\
		TRACE("\n\n");\
		ASSERT(FALSE);\
	}\
}

#define ASSERT_TRACE3(f,sz,p1,p2,p3)\
{\
	if (!(f))\
	{\
		TRACE1("\nASSERT(%s) Failed ",CString(#f));\
		TRACE3("in %s (%s line %d):\n\t",CString(__FUNCTION__),CString(__FILE__),__LINE__);\
		TRACE3(sz,p1,p2,p3);\
		TRACE("\n\n");\
		ASSERT(FALSE);\
	}\
}

// Support for collecting assertions in release also
#ifdef _DEBUG
#define ASSERT_ALWAYS(f)						ASSERT(f)
#define ASSERT_TRACE_ALWAYS(f,sz)				ASSERT_TRACE(f,sz)
#define ASSERT_TRACE_ALWAYS1(f,sz,p1)			ASSERT_TRACE1(f,sz,p1)
#define ASSERT_TRACE_ALWAYS2(f,sz,p1,p2)		ASSERT_TRACE2(f,sz,p1,p2)
#define ASSERT_TRACE_ALWAYS3(f,sz,p1,p2,p3)		ASSERT_TRACE3(f,sz,p1,p2,p3)
#else   // _DEBUG

#define ASSERT_ALWAYS(f)					((f) || !AfxGetApplicationContext()->AssertAlwaysFailedLine(#f, GetCurrentThreadId(), __FUNCTION__, __FILE__, __LINE__))
#define ASSERT_TRACE_ALWAYS(f,sz)			{((f) || !AfxGetApplicationContext()->AssertAlwaysFailedLine(#f, GetCurrentThreadId(), __FUNCTION__, __FILE__, __LINE__, sz));}
#define ASSERT_TRACE_ALWAYS1(f,sz,p1)		{((f) || !AfxGetApplicationContext()->AssertAlwaysFailedLine(#f, GetCurrentThreadId(), __FUNCTION__, __FILE__, __LINE__, sz,p1));}
#define ASSERT_TRACE_ALWAYS2(f,sz,p1,p2)	{((f) || !AfxGetApplicationContext()->AssertAlwaysFailedLine(#f, GetCurrentThreadId(), __FUNCTION__, __FILE__, __LINE__, sz,p1,p2));}
#define ASSERT_TRACE_ALWAYS3(f,sz,p1,p2,p3)	{((f) || !AfxGetApplicationContext()->AssertAlwaysFailedLine(#f, GetCurrentThreadId(), __FUNCTION__, __FILE__, __LINE__, sz,p1,p2,p3));}

#ifdef ASSERT
#undef ASSERT
#endif
#define ASSERT(f) ((void)(!AfxGetApplicationContext()->AreReleaseAssertionsEnabled() || ASSERT_ALWAYS(f)))

#ifdef ASSERT_TRACE
#undef ASSERT_TRACE
#endif
#define ASSERT_TRACE(f,sz) {if (AfxGetApplicationContext()->AreReleaseAssertionsEnabled()) ASSERT_TRACE_ALWAYS(f,sz)}

#ifdef ASSERT_TRACE1
#undef ASSERT_TRACE1
#endif
#define ASSERT_TRACE1(f,sz,p1) {if (AfxGetApplicationContext()->AreReleaseAssertionsEnabled()) ASSERT_TRACE_ALWAYS1(f,sz,p1)}

#ifdef ASSERT_TRACE2
#undef ASSERT_TRACE2
#endif
#define ASSERT_TRACE2(f,sz,p1,p2) {if (AfxGetApplicationContext()->AreReleaseAssertionsEnabled()) ASSERT_TRACE_ALWAYS2(f,sz,p1,p2)}

#ifdef ASSERT_TRACE3
#undef ASSERT_TRACE3
#endif
#define ASSERT_TRACE3(f,sz,p1,p2,p3) {if (AfxGetApplicationContext()->AreReleaseAssertionsEnabled()) ASSERT_TRACE_ALWAYS3(f,sz,p1,p2,p3)}

#endif // !_DEBUG

// The following macros set and clear, respectively, given bits
// of the C runtime library debug flag, as specified by a bitmask.
#ifdef   _DEBUG
#define  SET_CRT_DEBUG_FIELD(a) \
            _CrtSetDbgFlag((a) | _CrtSetDbgFlag(_CRTDBG_REPORT_FLAG))
#define  CLEAR_CRT_DEBUG_FIELD(a) \
            _CrtSetDbgFlag(~(a) & _CrtSetDbgFlag(_CRTDBG_REPORT_FLAG))
#else
#define  SET_CRT_DEBUG_FIELD(a)   ((void) 0)
#define  CLEAR_CRT_DEBUG_FIELD(a) ((void) 0)
#endif

#define JSON_FORMS

// #define _OLD_PTM

// #define NEWTREE 
