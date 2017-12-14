#pragma once

//includere alla fine degli include del .H
#include "beginh.dex"

//===========================================================================
class TB_EXPORT CriticalArea
{
private:
	int	m_nInUse;

public:
	CriticalArea()	{ m_nInUse = 0;	}

public:
	void	Lock		()	{ m_nInUse = !0;};
	void	Unlock		()	{ m_nInUse = 0;	};
	int		IsLocked	()	{ return m_nInUse ? (!0) : (!(m_nInUse = (!0))); }
};

#include "endh.dex"
