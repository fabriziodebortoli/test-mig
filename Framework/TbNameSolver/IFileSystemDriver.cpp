#include "stdafx.h"

#include "IFileSystemDriver.h"

//includere come ultimo include all'inizio del cpp
#include "begincpp.dex"

#ifdef _DEBUG
#undef THIS_FILE
static const char THIS_FILE[] = __FILE__;
#endif

//-----------------------------------------------------------------------------
IFileSystemDriver::IFileSystemDriver () : m_bStarted (FALSE)
{
}

//-----------------------------------------------------------------------------
BOOL IFileSystemDriver::IsStarted () const
{
	// no lock is required as setted in InitInstance and never changed
	return m_bStarted;
}

//-----------------------------------------------------------------------------
BOOL IFileSystemDriver::Start ()
{
	// no lock is required as setted in InitInstance and never changed
	m_bStarted = TRUE;
	return TRUE;
}

//-----------------------------------------------------------------------------
BOOL IFileSystemDriver::Stop ()
{
	// no lock is required as setted in InitInstance and never changed
	m_bStarted = FALSE;
	return TRUE;
}
