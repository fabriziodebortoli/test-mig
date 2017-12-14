#pragma once

#include <stddef.h>
#include <string.h>


//includere alla fine degli include del .H
#include "beginh.dex"

//=============================================================================
//						Class BitSet definition
//=============================================================================
class TB_EXPORT BitSet
{
protected:
	unsigned long	m_nLength;
	unsigned char*	m_pchData;

	BitSet()
	{
		m_nLength 		= 0L;
		m_pchData  		= NULL;
	}

public:
	// constructors
	BitSet(unsigned long size);
	BitSet(const BitSet& bs);

	// destructor
	~BitSet(void);

	// assignment operator
	void operator = (const BitSet& bs);

	// Get number of bits in set
	long Size() { return m_nLength; }

	// operation methods
	void On		(unsigned long bit)	{ if (bit < m_nLength) m_pchData[bit / 8] |= (unsigned char)(1 << (bit & 7)); }
	void Off	(unsigned long bit)	{ if (bit < m_nLength) m_pchData[bit / 8] &= ~(unsigned char)(1 << (bit & 7)); }

	// turn all bits in set on or off
	void AllOn()	{ memset(m_pchData,'\xFF',(size_t)((m_nLength + 7) / 8)); }
	void AllOff()	{ memset(m_pchData,'\x00',(size_t)((m_nLength + 7) / 8)); }

	// union operators
	BitSet operator &  (const BitSet& bs);
	BitSet& operator &= (const BitSet& bs);
	// synonyms for union operators
	BitSet operator +  (const BitSet& bs);
	BitSet& operator += (const BitSet& bs);
	// intersection operators
	BitSet operator |  (const BitSet& bs);
	BitSet& operator |= (const BitSet& bs);
	// difference operators
	BitSet operator -  (const BitSet& bs);
	BitSet& operator -= (const BitSet& bs);
	// complement operator
	BitSet operator ~ ();
	// comparison operator
	int operator == (const BitSet& bs);
	int operator != (const BitSet& bs);

	// value retrieval method
	int operator [] (unsigned long bit) const {	return (bit < m_nLength) ? (m_pchData[bit / 8] & (1 << (bit & 7))) : 0; }
};


class TB_EXPORT CharSet : public BitSet
{
public:
	// constructors
	CharSet()				: BitSet(256)	{}
	CharSet(CharSet& cs)	: BitSet(cs)	{}
	CharSet(char* pValues);

	// operator
	void operator = (CharSet& cs) { BitSet::operator = (cs); }
};

#include "endh.dex"
