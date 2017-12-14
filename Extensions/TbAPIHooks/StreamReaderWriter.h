#pragma once


class CStreamReaderWriter
{
public:
	void WriteString(CMemFile* pStream, const CString& str);
	void WriteString(CMemFile* pStream, const CStringA& str);
	void Write(CMemFile* pStream, const BYTE* pBuff, UINT nSize)
	{
		pStream->Write(pBuff, nSize);
	}
	template <class T> void Write(CMemFile* pStream, T t)
	{
		pStream->Write(&t, sizeof(t));
	}
	void ReadString(CMemFile* pStream, CString& str);
	void ReadString(CMemFile* pStream, CStringA& str);
	void Read(CMemFile* pStream, BYTE* pBuff, UINT nSize)
	{
		pStream->Read(pBuff, nSize);
	}
	template <class T> void Read(CMemFile* pStream, T& t)
	{
		pStream->Read(&t, sizeof(t));
	}
};


