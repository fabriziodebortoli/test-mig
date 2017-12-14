#pragma once
#include "Templates.h"
#include "beginh.dex"

class IDisposingSource;

typedef  void (CObject::*ON_DISPOSING_METHOD) (CObject*);

class TB_EXPORT CCallbackHandler : public CObject, public CTBLockable
{
	CArray<CObject*>			m_arOnDisposingListeners;
	CArray<ON_DISPOSING_METHOD>	m_arOnDisposing;
public:
	CCallbackHandler(void);
	~CCallbackHandler(void);
	virtual LPCSTR  GetObjectName() const { return "CCallbackHandler"; }
	//aggiunge una callback da chiamare alla distruzione del documento
	void AddDisposingHandler		(CObject* pListener, ON_DISPOSING_METHOD pHandler);
	void RemoveDisposingHandlers	(CObject* pListener);
	void FireDisposing(CObject* pSender);		
};

class TB_EXPORT IDisposingSource
{
public:
	//aggiunge una callback da chiamare alla distruzione del documento
	virtual void AddDisposingHandler (CObject* pListener, ON_DISPOSING_METHOD pHandler) = 0;
	virtual void RemoveDisposingHandlers (CObject* pListener)= 0;
};

class TB_EXPORT IDisposingSourceImpl : public IDisposingSource
{
public:
	CObject*				m_pObjOwner;
	CCallbackHandler		m_Handler;

	IDisposingSourceImpl(CObject* pOwner) : m_pObjOwner(pOwner) { ASSERT_VALID(pOwner); }
	~IDisposingSourceImpl() { m_Handler.FireDisposing(m_pObjOwner); }

	//aggiunge una callback da chiamare alla distruzione del documento
	void AddDisposingHandler(CObject* pListener, ON_DISPOSING_METHOD pHandler) { m_Handler.AddDisposingHandler(pListener, pHandler); }
	void RemoveDisposingHandlers(CObject* pListener) { m_Handler.RemoveDisposingHandlers(pListener); }
};


class TB_EXPORT IDisposablePtr : public CObject
{ 

protected:
   	IDisposingSource	*m_ptr;
public:
	inline IDisposablePtr() : m_ptr(NULL) {}
	inline operator IDisposingSource* () { return (IDisposingSource*)m_ptr;}
	IDisposingSource* operator = (IDisposingSource* ptr)
	{
		if (m_ptr == ptr)
			return m_ptr;
		if (m_ptr)
			m_ptr->RemoveDisposingHandlers(this);
		m_ptr = ptr; 
		if (m_ptr)
			m_ptr->AddDisposingHandler(this, (ON_DISPOSING_METHOD) &IDisposablePtr::OnObjectDisposing);
		return m_ptr;
	}
protected:
	virtual void OnDisposing(){}
private:
	inline void OnObjectDisposing (CObject* pSender) { operator =(NULL); OnDisposing(); }
};
template <class T>
class TDisposablePtr : public IDisposablePtr
{
public:
   	inline TDisposablePtr	()									{ }
	inline TDisposablePtr	(T	*ptr)							{ __super::operator =(ptr); }
	inline TDisposablePtr	(const TDisposablePtr& sPtr)		{ __super::operator =(sPtr.m_ptr); }	
   	virtual inline ~TDisposablePtr	()							{ __super::operator =(NULL); }

	inline operator bool	()									{ return m_ptr != NULL; }

	const T&	operator *		() 								{ ASSERT(m_ptr != NULL); return *(T*)m_ptr; }
   	
	inline IDisposingSource*		operator =	(IDisposingSource* ptr)	{ return __super::operator =(ptr); }
	inline TDisposablePtr			operator =	(TDisposablePtr sPtr)	{ __super::operator =(sPtr.m_ptr); return *this;}
	inline IDisposingSource*		Assign		(IDisposingSource* ptr)	{ return __super::operator =(ptr); }

   	inline BOOL		operator ==		(T* ptr)		{return (T*)m_ptr == ptr;}
   	inline BOOL		operator !=		(T* ptr)		{return (T*)m_ptr != ptr;}
   	
	inline T*		operator ->		()	const			{ ASSERT(m_ptr != NULL); return (T*)m_ptr; }
	inline			operator T*		()				{ /*ASSERT(m_ptr != NULL); e usata per passare il puntatore, eventualmente a NULL, alle funzioni*/ return (T*)m_ptr; }
	inline T**		operator &		()				{ return (T**)&m_ptr;}
	//inline 			operator T*&	()				{return (T*&)m_ptr;}


};
//segnala con un messaggio windows la distruzione dell'oggetto
//da usarsi in un loop di lettura dei messaggi, es nella WaidDocumentEnd, che deve aspettare l'evento
//ma nel contempo processare i messaggi
template <class T>
class TBEventDisposablePtr : public TDisposablePtr<T>
{
public:
	CTBEvent m_Disposed;

	inline TBEventDisposablePtr() { }
	inline TBEventDisposablePtr(T	*ptr) { __super::operator =(ptr); }
	inline TBEventDisposablePtr(const TDisposablePtr& sPtr) { __super::operator =(sPtr.m_ptr); }

	virtual void OnDisposing() { m_Disposed.Set(); }
};
#include "endh.dex"