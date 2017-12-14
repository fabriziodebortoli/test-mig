
#pragma once 

#include <TbGeneric\DataObj.h>
#include <tboledb\sqlrec.h>

class SqlConnection;

//includere alla fine degli include del .H
#include "beginh.dex"


#define DOCUMENT_NSTYPE 0
#define REPORT_NSTYPE  1


/* record che mappa la singola tabella di auditing, viene costruito in modo dinamico
   considerando i campi fissi:
    AU_ID			 : segmento di chiave (vedi anomalia 13679)
 	AU_OperationData : datetime dell'operazione
	AU_OperationType : tipo dell'operazione da tracciare: inserimento\modifica\cancellazione\cambio chiave 
	AU_UserID 		 : id dello user che ha effettuato la modifica
	AU_NameSpaceID	 : (se disponibile) id associato al namespace del documento
  e i campi variabili che dipendono dalle chiavi e di altri campi da inserire della tabella
  sotto tracciatura
*/

///////////////////////////////////////////////////////////////////////////////
//								TAuditingRecord definition
///////////////////////////////////////////////////////////////////////////////
class TB_EXPORT TAuditingRecord : public SqlRecord
{
	friend AuditingManager;

private:
	CString		m_strTracedTable; //tabella da mettere sotto protezione
	int			m_nStartVarFieldsPos; // posizione di bind in cui iniziano i campi variabili

public:
	DataLng		f_ID;
	DataDate	f_OperData;
	DataInt		f_OperType;
	DataStr		f_LoginName;
	DataLng		f_NamespaceID;

public:
	TAuditingRecord(CString strTracedTable, CString strAuditTable);
	~TAuditingRecord();

private:
	void BindVariableFields(int& nPos);

public:
	int GetStartVarFieldsPos() const { return m_nStartVarFieldsPos; }
	const CString& GetTracedTableName() const { return m_strTracedTable;}

public:
    virtual void BindRecord	();	
};


// SqlRecord relativa alla tabella AUDIT_Tables contenente l'elenco delle tabelle
// poste sotto tracciatura
//////////////////////////////////////////////////////////////////////////////
//								TAuditTables
//////////////////////////////////////////////////////////////////////////////
//-----------------------------------------------------------------------------
class TB_EXPORT TAuditTables : public SqlRecord
{
	DECLARE_DYNCREATE(TAuditTables) 
	
public:
	DataStr		f_TableName;
	DataDate	f_StartTrace;	
	DataBool	f_Suspended;	

public:
	TAuditTables(BOOL bCallInit = TRUE);

public:
    virtual void BindRecord	();	

public:
	static  LPCTSTR  GetStaticName();
};


//tabella di lookup tra namespace ed id associato allo stesso
//////////////////////////////////////////////////////////////////////////////
//								TNameSpaces
//////////////////////////////////////////////////////////////////////////////
//-----------------------------------------------------------------------------
class TB_EXPORT TNamespaces : public SqlRecord
{
	DECLARE_DYNCREATE(TNamespaces) 
	
public:
	DataLng	f_ID;
	DataStr	f_Namespace;
	DataInt	f_Type;

public:
	TNamespaces(BOOL bCallInit = TRUE);

public:
    virtual void BindRecord	();	
	  virtual void Init		();	

public:
	static  LPCTSTR  GetStaticName();
};



#include "endh.dex"

