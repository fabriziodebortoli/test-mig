
#pragma once

#include <TbGeneric\DataObj.h>

#include "beginh.dex"

class CAuditingMngObj;

//------------------------------------------------------------------------------------------------
// nuova implementazione con chiamata diretta a funzione
bool SetAuditManagerFunction (CString tableName, CAuditingMngObj** pAuditMng);     // mi crea l'auditing manager da agganciare al catalog entry 
// funzionalitá di start e stop del sistema di auditing
DataBool OpenAuditing		(); 
DataBool CloseAuditing		(); 


#include "endh.dex"
