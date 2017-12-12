
#include "stdafx.h"

#include "bitset.h"
#include "begincpp.dex"

#ifdef _DEBUG
#undef THIS_FILE
static const char BASED_CODE THIS_FILE[] = __FILE__;
#endif

//=============================================================================
//						Class BitSet implementation
//=============================================================================

// constructors
//~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~
BitSet::BitSet(unsigned long size)
{
	unsigned long alloc;

	m_nLength = size;
	alloc = (size + 7) / 8;
	m_pchData = new unsigned char[(unsigned int)alloc];
	memset(m_pchData,'\x00',(unsigned int)alloc);
}

//~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~
BitSet::BitSet(const BitSet& bs)
{
	unsigned long alloc;

	m_nLength = bs.m_nLength;
	alloc = (bs.m_nLength + 7) / 8;
	m_pchData = new unsigned char[(unsigned int)alloc];
	memcpy(m_pchData, bs.m_pchData, (unsigned int)alloc);
}

// destructor
//~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~
BitSet::~BitSet(void)
{
	if (m_pchData != NULL) delete m_pchData;
}

// assignment operator
//~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~
void BitSet::operator = (const BitSet& bs)
{
	unsigned long alloc;

	if (m_nLength != bs.m_nLength)
	{
		m_nLength = bs.m_nLength;
		alloc = (bs.m_nLength + 7) / 8;
		if (m_pchData != NULL)
			delete m_pchData;

		m_pchData = new unsigned char[(unsigned int)alloc];
		memcpy(m_pchData, bs.m_pchData, (unsigned int)alloc);
	}
	else
		memcpy(m_pchData, bs.m_pchData, (unsigned int)((m_nLength + 7) / 8));
}

// union operators
//~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~
BitSet BitSet::operator & (const BitSet& bs)
{
	BitSet result;
	unsigned long bit;

	if (m_nLength < bs.m_nLength)
	{
		result = bs;
		for (bit = 0; bit < m_nLength; ++bit)
			if ((*this)[bit])
				result.On(bit);
	}
	else
	{
		result = *this;
		for (bit = 0; bit < bs.m_nLength; ++bit)
			if (bs[bit])
				result.On(bit);
	}

	return result;
}

//~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~
BitSet& BitSet::operator &= (const BitSet& bs)
{
	*this = *this & bs;
	return *this;
}

// synonyms for union operators
//~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~
BitSet BitSet::operator + (const BitSet& bs)
{
	BitSet result = *this & bs;
	return result;
}

//~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~
BitSet& BitSet::operator += (const BitSet& bs)
{
	*this &= bs;
	return *this;
}

// intersection operators
//~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~
BitSet BitSet::operator | (const BitSet& bs)
{
	BitSet result;
	unsigned long max;

	if (m_nLength > bs.m_nLength)
	{
		result = BitSet(m_nLength);
		max    = bs.m_nLength;
	}
	else
	{
		result = BitSet(bs.m_nLength);
		max    = m_nLength;
	}

	for (unsigned long bit = 0; bit < max; ++bit)
	if ((*this)[bit] & bs[bit])
		result.On(bit);

	return result;
}

//~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~
BitSet& BitSet::operator |= (const BitSet& bs)
{
	*this = *this | bs;
	return *this;
}

// difference operators
//~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~
BitSet BitSet::operator - (const BitSet& bs)
{
	BitSet result = *this;
	unsigned long stop = (m_nLength < bs.m_nLength) ? m_nLength : bs.m_nLength;

	for (unsigned long bit = 0; bit < stop; ++bit)
		if (bs[bit])
			result.Off(bit);

	return result;
}

//~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~
BitSet& BitSet::operator -= (const BitSet& bs)
{
	*this = *this - bs;
	return *this;
}

// complement operator
//~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~
BitSet BitSet::operator ~ ()
{
	BitSet result(m_nLength);

	for (unsigned long bit = 0; bit < m_nLength; ++bit)
		if ((*this)[bit])
			result.Off(bit);
		else
			result.On(bit);

	return result;
}

// comparison operators
//~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~
int BitSet::operator == (const BitSet & bs)
{
	if (m_nLength != bs.m_nLength) return 0;

	for (unsigned long bit = 0; bit < m_nLength; ++bit)
		if ((*this)[bit] != bs[bit])
			return 0;
	return 1;
}

//~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~
int BitSet::operator != (const BitSet& bs)
{
	if (m_nLength != bs.m_nLength) return 1;

	unsigned long bit = 0;

	while (bit < m_nLength)
		if ((*this)[bit] == bs[bit])
			++bit;
		else
			return 1;

	return 0;
}

//~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~
CharSet::CharSet(char* pchValues) : BitSet(256)
{
	while (*pchValues != '\x00')
	{
		On(*pchValues);
		++pchValues;
	}
}
