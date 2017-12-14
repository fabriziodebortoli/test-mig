
#pragma once

#include <TbGenlib\Funproto.h>
#include <TbGenlib\BaseEventMng.h>
#include "DBT.h"

//----------------------------------------------------------------------------
//includere alla fine degli include del .H
#include "beginh.dex"

class CAbstractFormDoc;
class CClientDoc;


// macro per la gestione del routing ai clientdoc
#define FOREACH_CLIENT_DOC(msgRoutingMode)\
{\
CClientDocArray* pClientDocs = NULL;\
if(m_pDocument && m_pDocument->IsKindOf(RUNTIME_CLASS(CAbstractFormDoc)))\
	pClientDocs = ((CAbstractFormDoc*)m_pDocument)->m_pClientDocs;\
for (int i=0; pClientDocs && i<pClientDocs->GetSize (); i++)\
	{\
		CClientDoc* pClientDoc;\
		pClientDoc = pClientDocs->GetAt(i);\
		if(pClientDoc && pClientDoc->m_pEventManager &&\
				(pClientDoc->GetMsgRoutingMode() == CClientDoc::CD_MSG_BOTH ||\
				pClientDoc->GetMsgRoutingMode() == CClientDoc::msgRoutingMode ))\

#define DO_CLIENT_ACTION(strParams, msgState)\
				if(pClientDoc->m_pEventManager)\
				{\
					pClientDoc->SetMsgState(msgState);\
					currentResult = pClientDoc->m_pEventManager->FireAction strParams;\
					if (currentResult < result) result = currentResult;\
				}\


#define END_FOREACH\
	}\
}\

#define XML_ACTIONS_TAG					_T("Actions")

#define XML_NAME_ATTRIBUTE				_T("name")
#define XML_AFTER_CHANGE_ATTRIBUTE		_T("afterchange")

#define XML_TABLE_TAG					_T("Table")
#define XML_FIELD_TAG					_T("Field")
#define XML_CHANGING_FIELDS_TAG			_T("ChangingFields")
#define XML_CHANGING_FIELD_TAG			_T("ChangingField")

/*
	La funzione puo` essere definita nel documento oppure nella classe di gestione dell'evento

	Gestisce gli eventi del documento; questa classe deve essere 
	derivata dal programmatore ed estesa con le macro 
	DECLARE_TB_EVENT_MAP, BEGIN_TB_EVENT_MAP, END_TB_EVENT_MAP, TB_EVENT, TB_EVENT_EX
	tali macro forniscono la possibilita' di invocare dinamicamente un metodo a partire 
	dalla stringa che contiene il suo nome usando il metodo FireAction di CAbstractFormDoc
*/
//////////////////////////////////////////////////////////////////////////////////////////
//						CEventManager	Definition
//////////////////////////////////////////////////////////////////////////////////////////
class TB_EXPORT CEventManager: public CBaseEventManager
{
friend class CTableEvents;
friend class CAbstractFormDoc;

	DECLARE_DYNCREATE (CEventManager)

public:
	CEventManager ();
	
public:
	void AttachDocument(CAbstractFormDoc* pDocument);
	void AttachDocument(CClientDoc* pDocument);

public:
	virtual int FireAction(const CString& funcName, CString* pstrInputOutput);
	virtual int FireAction(const CString& funcName, void* pVoidInputOutput);
	virtual int FireAction(const CString& funcName);	
	virtual int FireAction(const CString& funcName, CFunctionDescription*);	
	virtual BOOL ExistAction(const CString& funcName, MappedFunction** ppMappedFunction = NULL, CObject** ppObj = NULL);

protected: 
	CCmdTarget*		m_pDocument; //common parent of CClientDoc and CAbstractFormDoc

	
	MappedFunction* SearchFunctionInDoc(CString funcName, CObject** pClass);
	MappedFunction* SearchFunctionInDialog(CString funcName, CObject** pClass);

	virtual MappedFunction* GetFunctionPointer(const CString& funcName, CObject** pClass, const CString&  strClassName = _T(""));
	static CString GetClassDocument(CObject* pObj);

// diagnostics
#ifdef _DEBUG
public: 
    void Dump(CDumpContext&) const;
    void AssertValid() const;
#endif
};

/*
	Gestisce gli eventi di import;
	Importando i file XML con X-Tech e' inoltre possibile invocare le 
	funzioni mappate tramite la definizione di un
	file XML di configurazione (actions.xml) con la seguente struttura:

		<?xml version="1.0" encoding="UTF-8"?>
		<Actions>
		   <Table Name="nome-tabella-1">
			   <Field Name="nome-campo-1" AfterChange = "NomeFunzione-1" />
			   ...
			   <Field Name="nome-campo-n" AfterChange = "NomeFunzione-n" />
			</Table>
		</Actions>
*/



//=============================================================================							
//SEZIONE SPECIFICA DELL'IMPORTAZIONE DI DOCUMENTI XML
//=============================================================================							

//macro per mappare un evento associato ad un HotLink
//uso il puntatore al puntatore all'HotLink perché non
//è detto che quando viene effettuato l'attach dell'event manager
//l'hot link sis già stato istanziato
#define TB_HKL_EVENT(theClass, theFunction, theHotLink)\
			m_FunctionList.AddTail(new XMLMappedFunction((VOID_PTR_VOID) &theClass::theFunction, CString(#theClass), CString(#theFunction), (HotKeyLink**)&((theClass*)m_pDocument)->theHotLink));\

//classe per mappare un evento associato ad un HotLink
class TB_EXPORT XMLMappedFunction : public MappedFunction
{
public:
	XMLMappedFunction(VOID_PTR_VOID funcPtr, CString className, CString funcName, HotKeyLink** ppHKL) 
	:
	MappedFunction(funcPtr, className, funcName)
	{
		ASSERT(ppHKL);
		m_ppHKL = ppHKL;
	};

	HotKeyLink** m_ppHKL;

protected:
	DECLARE_DYNAMIC(XMLMappedFunction);
};

/*
CTableEvents:
	CFieldEvent:
		CFieldFunction
			FunciontName, pClientDoc = NULL
			CReleatedFields

			

*/

class CFieldEvent;
class CFieldFunction;
//=============================================================================							
class TB_EXPORT CReleatedField: public CObject
{
public:
	CString			m_strTableName;
	CString			m_strFieldName;
	CObArray		m_arChangingFuncts; //#bugfix 18457 elenco delle funzioni che modificano il campo. 
	CFieldFunction*	m_pChangingFunct; //è l'ultima funzione che modifica il campo

public:
	CReleatedField(const CString& strTableName, const CString& strFieldName)
		:
		m_strTableName	(strTableName),
		m_strFieldName	(strFieldName),
		m_pChangingFunct (NULL)
	{}
	//~CReleatedField();

public:
	BOOL XMLParse	(CXMLNode* pNode);
	void XMLUnparse	(CXMLNode* pNode);
	void AddFunction(CFieldFunction* pFieldFunct) { m_arChangingFuncts.Add((CObject*)pFieldFunct); }
	void SetFunction(CFieldFunction* pFieldFunct) { m_pChangingFunct = pFieldFunct; }

};

//=============================================================================							
class TB_EXPORT CChangedTables : public CObject
{
public:
	CString			m_strFileName;
	CString			m_strDocTitle;
	CStringArray	m_arChangedTables;

public:
	CChangedTables(const CString& strFileName, const CString& strDocTitle)
	:
	m_strFileName(strFileName),
	m_strDocTitle(strDocTitle)
	{
	}
};
//=============================================================================							
class TB_EXPORT CFieldFunction: public CObject
{
public: 
	CString			m_strFunction;
	CClientDoc*		m_pClientDoc;
	CFieldEvent*	m_pFieldEvent;
	CObArray		m_arReleatedFields; 
	BOOL			m_ToBeFired; 
	BOOL			m_bModified;
	

public:
	CFieldFunction(const CString& strFunction, CFieldEvent*	pFieldEvent, CClientDoc* pClientDoc = NULL)
		:
	m_strFunction	(strFunction),
	m_pClientDoc	(pClientDoc),
	m_pFieldEvent	(pFieldEvent),
	m_ToBeFired		(FALSE),
	m_bModified		(FALSE)
	{

	}

	~CFieldFunction();

public:
	BOOL XMLParse			(CXMLNode* pNode, CObArray* pGlobalChangingFields, CClientDoc* pClientDoc = NULL);
	void XMLUnparse			(CXMLNode* pNode, CClientDoc* pClientDoc = NULL);
	void AddReleatedField	(CObArray* pGlobalReleatedFields, const CString& strTableName, const CString& strFieldName);
};


//=============================================================================							
class TB_EXPORT CFieldEvent: public CObject
{
public: 
	CString			m_strTableName;
	CString			m_strFieldName;
	Array			m_arFunctions;
	CTableEvents*	m_pTableEvents;


public:
	CFieldEvent(const CString& strFieldName, const CString& strTableName, CTableEvents*	pTableEvents)
	:
	m_strFieldName	(strFieldName),
	m_strTableName	(strTableName),
	m_pTableEvents	(pTableEvents)
	{}

public:
	BOOL XMLParse	(CXMLNode* pNode, CObArray* pGlobalChangingFields, CClientDoc* pClientDoc = NULL);
	void XMLUnparse	(CXMLNode* pNode, CClientDoc* pClientDoc = NULL);
};


//=============================================================================							
class TB_EXPORT CTableEvents: public CObject
{
public: 
	CString		m_strTableName;
	BOOL		m_bPrepared;
	Array		m_arFieldEvents;
	CStringArray m_arImportingFields; //#bugfix 18457 contiene l'elenco dei campi presenti nell'envelope	
	BOOL		m_bImportingFieldsLoaded;
	BOOL		 m_bModified;

public:
	CTableEvents(const CString& strTableName) 
	: 
	m_strTableName		(strTableName),
	m_bPrepared			(FALSE),
	m_bModified			(FALSE),
	m_bImportingFieldsLoaded (FALSE)
	{}

	virtual ~CTableEvents() 	{ Clear(); };

	BOOL XMLParse	(CXMLNode* pNode,  CObArray* pGlobalChangingFields, CClientDoc* pClientDoc = NULL);
	void XMLUnparse	(CXMLNode* pNode, CClientDoc* pClientDoc = NULL);

	CFieldEvent* GetFieldEvents(const CString& strFieldName);
	void ClearImportingFields();
	BOOL IsAnImportingField(const CString& strFieldName);
	
protected:
	void Clear();
// diagnostics
#ifdef _DEBUG
public: 
    void Dump(CDumpContext&) const;
    void AssertValid() const;
#endif
};

//=============================================================================							
class TB_EXPORT CXMLEventManager : public CEventManager
{
friend class CTableEvents;
friend class CAbstractFormDoc;

	DECLARE_DYNCREATE (CXMLEventManager)
public:
	Array	m_arReleatedFields; //optimization
	Array	m_AfterChangeFieldEvents;

	CObArray* m_pEventsToFire;

public:
	CXMLEventManager ();
	~CXMLEventManager();

	enum EventListType	{ BEFORE_CHANGE, AFTER_CHANGE, START_DEFAULT, END_DEFAULT};

	void Initialize()	{ LoadEvents(); } 

	void OnAfterChangeField	(DBTMaster* pMaster);
	void OnAfterChangeField	(SqlRecord *pRecord, Array* pRecordEvents);
	void AddReleatedField	(CFieldFunction* pFieldFunction, const CString& strTableName, const CString& strFieldName);


protected: 
	BOOL LoadEvents(); 	
	BOOL LoadFileActionsEvents(CClientDoc* pClientDoc = NULL);
	BOOL SaveFileActionsEvents(CClientDoc* pClientDoc = NULL, Array* pChanges = NULL);

	CXMLDocumentObject* OpenXMLActionsDocument(BOOL bIsSaving, CClientDoc* pClientDoc = NULL);

public:
	BOOL	SaveEvents(Array* pChanges = NULL);
	void	InitTablesEvents();
	void	InitSingleTableEvents(CTableEvents* pTableEvents);
	void	SetImportingFields(CString strTableName, CStringArray* pImportingFields);

	CTableEvents* GetEvents(const CString& tableName, EventListType type);
	void OnAfterChangeField(SqlRecord *pRecord, CTableEvents *pEvents);

	BOOL AddEvent(CTableEvents* pEvents, EventListType type);
	
	int FireAction(const CString& funcName, DataObj* pDataObj);

	CFieldEvent* GetFieldEvents(const CString& tableName, const CString& fieldName);

// diagnostics
#ifdef _DEBUG
public: 
    void Dump(CDumpContext&) const;
    void AssertValid() const;
#endif
};

#include "endh.dex"
