
#pragma once

#include <TBGeneric\DataObj.h>
#include <XEngine\TBXMLEnvelope\XEngineObject.h>

#include "genfunc.h"

#include "beginh.dex"

class CXMLDocumentObject;
class CSmartExportParams;

void		CloseLatestDocument();
DataObjArray/*string*/ GetExportProfileList	(DataStr nameSpace, DataInt posType, DataStr userName);
DataStr		GetExportProfilesPath		(DataStr documentNamespace, DataInt posType, DataStr userName);

DataStr		GetSiteCode();
DataBool	Import(DataLng documentHandle, DataStr envelopeFolder, DataStr& resultDescription);
DataBool	OpenDocumentAndImport(DataStr documentNamespace, DataStr envelopeFolder, DataStr& resultDescription);

DataBool	GetXMLExportParameters (DataStr documentNamespace, DataStr& xmlParams, DataObjArray& messages, DataStr code);
DataBool	GetXMLImportParameters (DataStr documentNamespace, DataStr& xmlParams, DataObjArray& messages, DataStr code);

DataBool	RunXMLExportInUnattendedMode (DataStr documentNamespace, DataStr xmlParams, DataLng& documentHandle, DataObjArray& messages);
DataBool	RunXMLImportInUnattendedMode (DataStr documentNamespace, DataBool downloadEnvelopes, DataBool validateData, DataStr xmlParams, DataLng& documentHandle, DataObjArray& messages);

//Tutto viaggia via stringa:
//		params : contenente i file xml contenente i parametri per la query di esportazione
//		result   : (non obbligatoria, poichè se non esistente viene creata dal processo di esportazione) contiente i dati xml generati dal processo di esportazione e l'eventuale nodo di errore
DataBool GetData(DataStr param, DataBool useApproximation, DataStr loginName, DataArray& result);

//Tutto viaggia via stringa:
//		data   :  contiente i file xml dei dati a partire dai quali viene eseguito il processo di salvataggio
//		result :(non obbligatoria, poichè se non esistente viene creata dal processo di importazione) contiene il postback o l'eventuale errore
DataBool SetData(DataStr data, DataInt saveAction, DataStr loginName, DataStr& result);
DataBool GetXMLParameters(DataStr param, DataBool useApproximation, DataStr loginName, DataStr& result);

//returns the HotLink definition of the specified field and exportprofile
//fieldXPath can be:
// Document/Data/DBT/Row/ExtRefField/ExtRefDBT/Row/Field
// Document/Data/DBT/Row/ExtRefField/ExtRefDBT/Field
// Document/Data/DBT/Row/ExtRefField/ExtRefDBT
// Document/Data/DBT/ExtRefField/ExtRefDBT/Row/Field
// Document/Data/DBT/ExtRefField/ExtRefDBT/Row
// Document/Data/DBT/ExtRefField/ExtRefDBT/Field
// Document/Data/DBT/ExtRefField/ExtRefDBT
// Document/Data/DBT/Row/Field
// Document/Data/DBT/Row
// Document/Data/DBT/Field
// Document/Data/DBT
DataStr GetXMLHotLink(DataStr documentNamespace, DataStr nsUri, DataStr fieldXPath, DataStr loginName);


// permette di lanciare il wizard dei profili e creare un nuovo profilo con nome strProfileName e
// legato al documento il cui namespace è passato come primo parametro
DataBool NewExportProfile(DataStr documentNamespace, DataStr newProfileName, DataInt posType, DataStr userName, DataStr& profilePath);

//clona nella posizione passata il profilo la cui la cui path è passata come parametro 
DataBool CloneExportProfile(DataStr documentNamespace, DataStr& strProfilePath, DataInt posType, DataStr userName);

// permette di lanciare il wizard dei profili e modificare il profilo la cui path è passata come parametro.
// il profilo modificato viene salvato in base a posType e useName specificato
DataBool ModifyExportProfile(DataStr documentNamespace, DataStr& profilePath, DataInt posType, DataStr userName);

// permette di lanciare il wizard dei profili e visualizzare (senza modificarlo) il profilo il cui namespace è passato come parametro
DataBool ShowExportProfile(DataStr documentNamespace, DataStr profilePath);

//allows to remove the exportprofile with the specified profilePath
DataBool DeleteExportProfile(DataStr documentNamespace, DataStr profilePath);

//allows to rename the exportprofile
DataBool RenameExportProfile(DataStr documentNamespace, DataStr& profilePath, DataStr newName);

// copy a profile to a new position
DataBool CopyExportProfile(DataStr documentNamespace, DataStr profilePath, DataInt posType, DataObjArray/*string*/ userArray);

// move a profile to a new position
DataBool MoveExportProfile(DataStr documentNamespace, DataStr profilePath, DataInt posType, DataObjArray/*string*/ userArray);

// allows to create a XSD for Magic Document
//  you need to create a document before
DataBool CreateSmartXSD(DataStr documentNamespace, DataStr& profilePath);

// return XSD document schema in string format
DataStr GetDocumentSchema(DataStr documentNamespace, DataStr profileName, DataStr forUser);

// return XSD report schema in string format
DataStr GetReportSchema(DataStr reportNamespace, DataStr forUser);

#include "endh.dex"