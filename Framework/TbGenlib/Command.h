
#pragma once

//includere alla fine degli include del .H
#include "beginh.dex"
//============================================================================
class TB_EXPORT TBScript: public CObject 
{
public:
	virtual BOOL	Exec	() = 0;
	virtual BOOL	Parse	(Parser&) = 0;
	virtual SymTable* GetSymTable() const = 0;
};

//////////////////////////////////////////////////////////////////////////////
#include "endh.dex"
