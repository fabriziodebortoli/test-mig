#pragma once

#include <TbGeneric\DataObj.h>
#include <TbGenlibUI\SettingsTableManager.h>

#include "beginh.dex" 

/////////////////////////////////////////////////////////////////////////////
//					ResourcesLayoutSettings declaration
// classe gestione dei settings del layout (mostra dettagli e disabilitati)
/////////////////////////////////////////////////////////////////////////////
//=============================================================================
class TB_EXPORT ResourcesLayoutSettings : public CParameterInfo
{
public:
	ResourcesLayoutSettings();

public:
	DataBool	GetDisabledToo	();
	void		SetDisabledToo	(DataBool aValue);
	DataBool	GetShowDetails	();
	void		SetShowDetails	(DataBool aValue);
};

#include "endh.dex" 
