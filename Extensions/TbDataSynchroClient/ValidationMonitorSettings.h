#pragma once

#include <TbGeneric\DataObj.h>
#include <TbGenlibUI\SettingsTableManager.h>
#include <TbOleDb\TbExtensionsInterface.h>

#include "beginh.dex" 

/////////////////////////////////////////////////////////////////////////////
//					class ValidationMonitorSettings definition
/////////////////////////////////////////////////////////////////////////////
//
//=============================================================================
class TB_EXPORT ValidationMonitorSettings : public CParameterInfo, public IMassiveValidationSettings
{
public:
	ValidationMonitorSettings();

public:
	// Indica se la procedura di validazione massiva è stata lanciata almeno una volta 
	// (a prescindere dal risultato ottenuto)
	virtual BOOL GetNeedMassiveValidation(); 
	virtual void SetNeedMassiveValidation(DataBool bValue);
};

#include "endh.dex" 


