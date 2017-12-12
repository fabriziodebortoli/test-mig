#pragma once

//includere alla fine degli include del .H
#include "beginh.dex"

//=============================================================================
class TB_EXPORT Stack : public CObArray
{
public:
	Stack();
	virtual ~Stack();

	void		SetOwns		(BOOL);
	void		ClearStack	();
	BOOL		IsEmpty		() const;

	virtual	void		Push	(CObject*);
	virtual	CObject*	Pop		();
	virtual	CObject*	Top		();

protected:
	BOOL		m_bOwnsElements;
};
                              
//=============================================================================
class TB_EXPORT FixedSizeStack : public Stack
{
public:
	FixedSizeStack (int nStackSize = 200);

	virtual void Push	 (CObject*);
			void SetSize (int nSize);

protected:
	int	m_nSize;
};

#include "endh.dex"
