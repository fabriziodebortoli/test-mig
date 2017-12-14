#pragma once

//includere alla fine degli include del .H
#include "beginh.dex"

//================================================================================
class TB_EXPORT CBinaryFileReader
{
	CFile m_File;

public:
			CBinaryFileReader(const CString &strFile);
	virtual ~CBinaryFileReader();

public:
	// parsing methods
	UINT	ReadUInt	();
	BYTE	ReadByte	();
	CString ReadString	();
};

#include "endh.dex"
