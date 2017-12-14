
#pragma once

#include <TbNameSolver\TBNamespaces.h>

//includere alla fine degli include del .H
#include "beginh.dex"

class CXMLUniversalKeyGroup;
class AddOnModule;
class CXMLProfileInfo;

/////////////////////////////////////////////////////////////////////////////

/////////////////////////////////////////////////////////////////////////////
CString	GetProfilePath(
						const CTBNamespace& nsDocument, 
						const CString& strProfile, 
						CPathFinder::PosType ePosType, 
						const CString& userName = _T(""), 
						BOOL bCreate			 =FALSE, 
						BOOL bUseCustomSearch	= FALSE
					   );

CString	GetSchemaProfileFile	(CXMLProfileInfo* pProfileInfo);
BOOL	ExistProfile			(const CTBNamespace&, const CString& strProfileName);
BOOL	ExistProfile			(const CString& strProfilePath);
BOOL	GetProfileNamespace		(const CTBNamespace& aDocumentNamespace, const CString& strProfileName, CTBNamespace& aProfileNamespace);


CString GetDocFileFromDocProfilePath			(const CString&);
CString GetXRefFileFromDocProfilePath			(const CString&);
CString GetUsrCriteriaFileFromDocProfilePath	(const CString&);
CString GetFieldFileFromDocProfilePath			(const CString&);
CString	GetHKLFileFromDocProfilePath			(const CString&);

// gestione del file di memorizzazione dei valori preferenziali assegnati alle 
// variabili dei criteri di estrazione dell'esportazione
CString GetExpCriteriaVarFile					(const CTBNamespace& nsDocument, const CString& strProfileName);

// servono per lo scheduler
CString GetExpCriteriaVarFile					(const CTBNamespace& nsDocument, const CString& strProfileName, const CString& strFileName);
CString MakeExpCriteriaVarFile					(const CTBNamespace& nsDocument, const CString& strProfileName, const CString& strFileName, CPathFinder::PosType posType, const CString& userRole =_T(""));

CString GetImpCriteriaVarFile					(const CTBNamespace& nsDocument, const CString& strFileName);
CString MakeImpCriteriaVarFile					(const CTBNamespace& nsDocument, const CString& strFileName, CPathFinder::PosType ePosType, const CString& strUserRole /*=_T("")*/);

TB_EXPORT CString GetSmartSchemaProfileFile		(const CTBNamespace& nsDocument, const CString& strProfilePath);
TB_EXPORT CString MakeSmartSchemaProfileFile	(const CTBNamespace&, const CString& strProfilePath, CPathFinder::PosType, const CString& strUserRole =_T(""));

int		CompareNoFormat							(const CString&, const CString&);

//@@BAUZI
//dato il namespace di un documento ritorta tutti i profili ad esso associati utilizzando la logica della personalizzazione
//per utente in base al posType specifito. Ogni singolo profilo viene considerato:
// prima quello presente nella directory custom dell'utente logginato o specificato in strUserName
// poi quello presente nella directory allusers
// infine quello presente nella standard
void GetAllExportProfiles(const CTBNamespace& nsDocument, CStringArray* pProfilesList, CPathFinder::PosType ePosType, const CString& strUserName);


//carico le seguenti informazioni contenute nel file XSLT passato come parametro:
//	- descrizione del foglio (se previsto)
//	- eventuale namespace del documento di destinazione (se previsto)
void GetXSLTInformation(const CString& strXSLTFileName, CString& strXSLTDescri, CTBNamespace& nsTransDoc);


/////////////////////////////////////////////////////////////////////////////

#include "endh.dex"

