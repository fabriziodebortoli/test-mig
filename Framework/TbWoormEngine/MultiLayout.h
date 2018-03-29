#pragma once

#include <TbGeneric\RdeProtocol.h>
//#include <TbGenlib\SymTable.h>

//includere alla fine degli include del .H
#include "beginh.dex"

//============================================================================
//		class IBaseObj
//============================================================================
class TB_EXPORT IBaseObj : public CWnd
{
	DECLARE_DYNAMIC(IBaseObj)

protected:
		WORD	m_wInternalID;

		IBaseObj () : m_wInternalID (0) {} //SpecialReportField::NO_INTERNAL_ID
public:
	virtual int		RowsNumber		() const { return 0; }					//Table/Repeater override it
	virtual int		GetViewCurrentRow	() const { return 0; }					//Table/Repeater override it
	virtual WORD	GetInternalID	() const { return m_wInternalID; }		//Table/Repeater/FieldRect use it
	virtual	WORD	GetRDESearchID	() const { return m_wInternalID; };		//Table/Repeater override it: returns active column's ID
	virtual void	SetInternalID	(WORD)  { ASSERT(FALSE); }		//FieldRect use it
};

#include "endh.dex"
