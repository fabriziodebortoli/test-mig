
#pragma once

#include <TbGenlib\InterfaceMacros.h>

//includere alla fine degli include del .H
#include "beginh.dex"

// useful class
class CTBNamespace;
class SqlConnection;

//----------------------------------------------------------------------------
// Macro di definizione dell'entry point esportato. Serve se si definiscono
// classi di addon personalizzate

//-------------------------------------------------------------------------------------------  Form
#define BEGIN_TABLES()	virtual BOOL AOI_RegisterTables(SqlConnection* pSqlConnection, LPCTSTR pszSignature, CTBNamespace* pModuleNS)\
{\
		CTBNamespace aTblNamespace (*pModuleNS);

#define END_TABLES()	}

//-------------------------------------------------------------------------------------------  Form
#define DATABASE_RELEASE(nRel)	virtual int AOI_DatabaseRelease() { return nRel; }

// macro utilizzate per l'aggiunta di nuove colonne ad una tabella già eistente, da
// un'altra dll
//-----------------------------------------------------------------------------
#define BEGIN_ADDON_NEW_COLUMNS() virtual void AOI_AddOnNewColumns(const SqlCatalogEntry* pCatalogEntry, CRTAddOnNewFieldsArray* pSqlAddOnColumnsInfo, const CString& strSignature, const CTBNamespace& aNamespace) {
#define WHEN_TABLE(aRec)					if (pCatalogEntry->GetSqlRecordClass() == RUNTIME_CLASS(aRec)) {	
#define ADDON_COLUMNS_CLASS(aNewCols)			pSqlAddOnColumnsInfo->Add(new CRTAddOnNewFields(RUNTIME_CLASS(aNewCols), strSignature, pCatalogEntry->GetSqlRecordClass(), aNamespace));
#define END_TABLE								return; }
#define END_ADDON_NEW_COLUMNS()		}

//=======================================================================
#include "endh.dex"
