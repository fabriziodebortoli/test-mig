#include "stdafx.h"

#include <io.h>

#include <TbNameSolver\Diagnostic.h>

#include <TbGeneric\GeneralFunctions.h>
#include <TbGeneric\TBStrings.h>

#include "BinaryFileReader.h"

//includere come ultimo include all'inizio del cpp
#include "begincpp.dex"

#ifdef _DEBUG
#undef THIS_FILE
static const char THIS_FILE[] = __FILE__;
#endif

const static int nrLength = 4;
//--------------------------------------------------------------------------------
CBinaryFileReader::CBinaryFileReader (const CString &strFile)
{
	m_File.Open(strFile, CFile::modeRead | CFile::shareDenyWrite);
}

//--------------------------------------------------------------------------------
CBinaryFileReader::~CBinaryFileReader()
{
	m_File.Close();
}

//--------------------------------------------------------------------------------
UINT CBinaryFileReader::ReadUInt()
{	
	UINT n;
	if (m_File.Read(&n, nrLength) != nrLength)
		AfxGetDiagnostic()->Add (cwsprintf(_TB("CBinaryFileReader::ParseUInt() of file %s cannot read number"), m_File.GetFileName()), CDiagnostic::Error);

	return n;
}

//--------------------------------------------------------------------------------
BYTE CBinaryFileReader::ReadByte()
{
	BYTE byte;
	if (m_File.Read(&byte, 1) != 1)
		AfxGetDiagnostic()->Add (cwsprintf(_TB("CBinaryFileReader::ParseByte() of file %s cannot read byte"), m_File.GetFileName()), CDiagnostic::Error);

	return byte;
}

//--------------------------------------------------------------------------------
CString CBinaryFileReader::ReadString ()
{
	UINT len = ReadUInt();
	if (len == 0)
		return _T("");

	char* buff = (char*) alloca(len);
	
	if (m_File.Read(buff, len) != len)
	{
		AfxGetDiagnostic()->Add (cwsprintf(_TB("CBinaryFileReader::ParseString() of file %s cannot read text"), m_File.GetFileName()), CDiagnostic::Error);
		return _T("");
	}

	int n = (len + 1 ) * sizeof(wchar_t);
	wchar_t *pwBuff = (wchar_t *) alloca(n);
	int m_nCount = MultiByteToWideChar(CP_UTF8, 0, buff, len, pwBuff, n);
	pwBuff[m_nCount] = L'\0';

	return CString(pwBuff);
}
