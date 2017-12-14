#pragma once

#include <TbNameSolver\TbNamespaces.h>
#include <TbGeneric\ParametersSections.h>

//includere alla fine degli include del .H
#include "beginh.dex"

//=============================================================================        
//						TbGenLibSettings
//============================================================================= 
class TB_EXPORT TbGenlibSettings : public TbBaseSettings
{
public:
	TbGenlibSettings ();

	CString GetMsOfficeSectionName();

	CString GetExcelDateFormat		();
	void	SetExcelDateFormat		(const CString&);

	CString GetExcelDateTimeFormat	();
	void	SetExcelDateTimeFormat	(const CString&);

	CString GetExcelTimeFormat		();
	void	SetExcelTimeFormat		(const CString&);

protected:
	CString GetMsOfficeFormatting	(LPCTSTR szEntry);
};

#include "endh.dex"
