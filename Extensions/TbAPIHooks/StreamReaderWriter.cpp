#include "StdAfx.h"

#include <atlenc.h>

#include "TBGlobals.h"
#include "StreamReaderWriter.h"




//-----------------------------------------------------------------------------
CStringA UnicodeToUTF8(CString strData)
{
	int nBuffSize = AtlUnicodeToUTF8(strData, strData.GetLength(), NULL,0); //calcolo la dimensione richiesta per il buffer
	
	CStringA strResult;
	
	AtlUnicodeToUTF8(strData, strData.GetLength(), strResult.GetBuffer(nBuffSize), nBuffSize);	

	strResult.ReleaseBuffer();

	return strResult;
}

//-----------------------------------------------------------------------------
CString UTF8ToUnicode(CStringA strData)
{
	int nBuffSize = MultiByteToWideChar(CP_UTF8, 0, strData, strData.GetLength(), NULL, 0);//calcolo la dimensione richiesta per il buffer
	
	CString strResult;
	
	MultiByteToWideChar(CP_UTF8, 0, strData, strData.GetLength(), strResult.GetBuffer(nBuffSize), nBuffSize);

	strResult.ReleaseBuffer();

	return strResult;
}
//-----------------------------------------------------------------------------
void CStreamReaderWriter::WriteString(CMemFile* pStream, const CString& str)
{
	CStringA s = UnicodeToUTF8(str);
	WriteString(pStream, s);
}

//-----------------------------------------------------------------------------
void CStreamReaderWriter::WriteString(CMemFile* pStream, const CStringA& str)
{
	Write<int>(pStream, str.GetLength());
	pStream->Write((LPCSTR)str, str.GetLength());
}

//-----------------------------------------------------------------------------
void CStreamReaderWriter::ReadString(CMemFile* pStream, CString& str)
{
	CStringA s;
	ReadString(pStream, s);
	str = UTF8ToUnicode(s);
}

//-----------------------------------------------------------------------------
void CStreamReaderWriter::ReadString(CMemFile* pStream, CStringA& str)
{
	int l;
	Read<int>(pStream, l);
	pStream->Read(str.GetBuffer(l), l);
	str.ReleaseBuffer();
}


