
#pragma once

#include <TbNameSolver\CallbackHandler.h>
#include <TbGenlib\Funproto.H>

//----------------------------------------------------------------------------
//includere alla fine degli include del .H
#include "beginh.dex"

class CAbstractFormDoc;
class CClientDoc;

typedef int	(CObject::*INT_PTR_STRINGPTR)(CString*);
typedef int	(CObject::*INT_PTR_VOIDPTR)(void*);
//typedef void (CObject::*VOID_PTR_VOIDPTR)(void*);
typedef BOOL (CObject::*BOOL_PTR_VOID)();
typedef	void (CObject::*VOID_PTR_VOID)();
typedef int	(CObject::*INT_PTR_FUNCTIONDESCRIPTION)(CFunctionDescription*);

#define DECLARE_TB_EVENT_MAP()\
		virtual void AddFunctionPointers();

//da usare se voglio che l'event manager derivato erediti le funzioni
//mappate nell'event manager padre
#define BEGIN_TB_EVENT_MAP_EX(theClass, theSuperClass) \
		BEGIN_TB_EVENT_MAP(theClass)\
		theSuperClass::AddFunctionPointers();

//da usare se voglio che l'event manager derivato NON erediti le funzioni
//mappate nell'event manager padre
#define BEGIN_TB_EVENT_MAP(theClass) \
		void theClass::AddFunctionPointers()\
		{

#define TB_EVENT_EX(theClass, theFunction, thePrototype)\
			m_FunctionList.AddTail(new MappedFunction((thePrototype) &theClass::theFunction, _T(#theClass), _T(#theFunction)));

#define TB_EVENT(theClass, theFunction) TB_EVENT_EX(theClass, theFunction, VOID_PTR_VOID)
#define TB_BOOL_EVENT(theClass, theFunction) TB_EVENT_EX(theClass, theFunction, BOOL_PTR_VOID)

#define END_TB_EVENT_MAP	}

#define	EMPTY_LIST_IDX					-1

//=============================================================================							
class TB_EXPORT MappedFunction : public CObject
{
public:
	MappedFunction(INT_PTR_FUNCTIONDESCRIPTION funcPtr, CString className, CString funcName) 
	{
		ASSERT(funcPtr);

		m_TBFunc			= funcPtr;
		m_strPtrFunc		= NULL;
		m_voidPtrFunc		= NULL;
		m_voidFunc			= NULL;
		m_voidBoolFunc		= NULL;

		m_className			= className;
		m_FuncName			= funcName;
	};

	MappedFunction(INT_PTR_STRINGPTR funcPtr, CString className, CString funcName) 
	{
		ASSERT(funcPtr);

		m_TBFunc			= NULL;
		m_strPtrFunc		= funcPtr;
		m_voidPtrFunc		= NULL;
		m_voidFunc			= NULL;
		m_voidBoolFunc		= NULL;

		m_className			= className;
		m_FuncName			= funcName;
	};

	MappedFunction(INT_PTR_VOIDPTR funcPtr, CString className, CString funcName) 
	{
		ASSERT(funcPtr);

		m_TBFunc			= NULL;
		m_strPtrFunc		= NULL;
		m_voidPtrFunc		= funcPtr;
		m_voidFunc			= NULL;
		m_voidBoolFunc		= NULL;

		m_className			= className;
		m_FuncName			= funcName;
	};
	
	MappedFunction(VOID_PTR_VOID funcPtr, CString className, CString funcName) 
	{
		ASSERT(funcPtr);

		m_TBFunc			= NULL;
		m_strPtrFunc		= NULL;
		m_voidPtrFunc		= NULL;
		m_voidFunc			= funcPtr;
		m_voidBoolFunc		= NULL;

		m_className			= className;
		m_FuncName			= funcName;
	};

	MappedFunction(BOOL_PTR_VOID funcPtr, CString className, CString funcName)
	{
		ASSERT(funcPtr);

		m_TBFunc			= NULL;
		m_strPtrFunc		= NULL;
		m_voidPtrFunc		= NULL;
		m_voidFunc			= NULL;
		m_voidBoolFunc		= funcPtr;

		m_className			= className;
		m_FuncName			= funcName;
	};

	VOID_PTR_VOID				m_voidFunc;
	INT_PTR_STRINGPTR			m_strPtrFunc;
	INT_PTR_VOIDPTR				m_voidPtrFunc;
	BOOL_PTR_VOID				m_voidBoolFunc;
	INT_PTR_FUNCTIONDESCRIPTION	m_TBFunc;

	CString			m_className;
	CString			m_FuncName;

protected:

	DECLARE_DYNAMIC(MappedFunction);
};

/*
	La funzione puo`essere definita nella classe di gestione dell'evento
	Gestisce gli eventi questa classe deve essere derivata dal programmatore ed 
	estesa con le macro 
	DECLARE_TB_EVENT_MAP, BEGIN_TB_EVENT_MAP, END_TB_EVENT_MAP, TB_EVENT, TB_EVENT_EX
	tali macro forniscono la possibilita' di invocare dinamicamente un metodo a partire 
	dalla stringa che contiene il suo nome usando il metodo FireAction del CBaseEventManager
*/
//////////////////////////////////////////////////////////////////////////////////////////
//						CBaseEventManager	Definition
//////////////////////////////////////////////////////////////////////////////////////////
//
class TB_EXPORT CBaseEventManager: public CObject, public IDisposingSourceImpl
{

friend class CEvents;

	DECLARE_DYNCREATE (CBaseEventManager)

	CPtrList			m_FunctionList;	
	CBaseEventManager*	m_pParentEventMng;

public:
// valori di ritorno della funzione FireAction
// NB: valori minori hanno la prevalenza su valori maggiori nella composizione
// di più result
	enum EventResult {	FUNCTION_NOT_FOUND	= 1,
						FUNCTION_OK			= 0,
						FUNCTION_WARNING	= -1,
						FUNCTION_ERROR		= -2 };

public:
	CBaseEventManager ();
	~CBaseEventManager ();

public:
	virtual void Initialize()	{} 
	
	// ho diversi FireAction a differenza del tipo di funzione
	virtual int FireAction(const CString& funcName, CString* pstrInputOutput);
 	virtual int FireAction(const CString& funcName, void* pVoidInputOutput);
	virtual int FireAction(const CString& funcName);	
	virtual int FireAction(const CString& funcName, CFunctionDescription*);	

	virtual BOOL ExistAction(const CString& funcName, MappedFunction** ppMappedFunction = NULL, CObject** ppObj = NULL);
	CPtrList*	GetFunctionList() {return &m_FunctionList;}

protected: 
	BOOL IsValidClass (CObject** pClass, CString strClassName)
		{
			USES_CONVERSION; 
			return *pClass && 
				(strClassName.IsEmpty() || 
				!strClassName.CompareNoCase (A2T((LPSTR)(*pClass)->GetRuntimeClass()->m_lpszClassName)));
		}

	
	MappedFunction* SearchFunctionInEventManager(const CString& funcName, CObject** pClass);

	virtual MappedFunction* GetFunctionPointer(const CString& className, const CString& funcName);
	virtual MappedFunction* GetFunctionPointer(const CString& funcName, CObject** pClass, const CString& strClassName = _T(""));
	virtual void AddFunctionPointers() {};

// diagnostics
#ifdef _DEBUG
public: 
    void Dump(CDumpContext&) const;
    void AssertValid() const;
#endif
};

#include "endh.dex"
