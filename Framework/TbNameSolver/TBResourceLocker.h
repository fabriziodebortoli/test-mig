#pragma once

#include <afxmt.h>

//includere alla fine degli include del .H
#include "beginh.dex"

#define LOCK_TIMEOUT	INFINITE //30000	//milliseconds
class TB_EXPORT CTBLockException : public CException
{

};
class CTBLockable;
///=============================================================================
class CTBLock
{
	friend class CTBLockable;
protected:

	::CCriticalSection      m_Locker;
	::CEvent				m_ResourceAvailable;
	CDWordArray				m_arReadingThreads;
	DWORD					m_nWritingThread;
	int						m_nWritingCount;
	CTBLockable*			m_pLockable;

public:
	//-----------------------------------------------------------------------------
	CTBLock() :
		m_pLockable(NULL), m_nWritingThread(0), m_nWritingCount(0)
	{
	};

	//-----------------------------------------------------------------------------
	virtual ~CTBLock()
	{
	}

	BOOL Lock_DataRead(BOOL bTry);
	void Unlock_DataRead();
	BOOL Lock_DataWrite(BOOL bTry);
	void Unlock_DataWrite();

};

//=============================================================================
class TB_EXPORT CTBLockable 
{
	friend class CTBMultiAutoLock;
	friend class BaseSmartLock;
	
private:
	mutable CTBLock			m_InternalCritical;	//for internal use
		
	static	BOOL			m_sbLockingEnabled;
	static	BOOL			m_sbLockTraceEnabled;
	    
public:
	
	CTBLockable();
	~CTBLockable();

	virtual LPCSTR  GetObjectName() const = 0;
	static void EnableLocking (BOOL bEnable);
private:
	//-----------------------------------------------------------------------------
	BOOL IsLockingEnabled() const { return m_sbLockingEnabled; }
	//-----------------------------------------------------------------------------
	BOOL	Lock(const BOOL bForWriting, const DWORD dwMilliseconds, LPCSTR pszFunction, BOOL bTry) const throw();
	//-----------------------------------------------------------------------------
	void	Unlock(const BOOL bForWriting, LPCSTR pszFunction) const;
	
};


//=============================================================================
class TB_EXPORT CTBMultiAutoLock
{
private:
	BOOL			m_bForWriting;
	LPCSTR			m_pszFunction;
	BOOL			m_bLockAcquired;
	const CTBLockable* m_pLockableObj;
public:
	CTBMultiAutoLock();
	CTBMultiAutoLock(BOOL bForWriting, const CTBLockable* const pLockableObj, LPCSTR lpszFunction, const DWORD dwMilliseconds = LOCK_TIMEOUT);
	~CTBMultiAutoLock();

	void GetLock(BOOL bForWriting, const CTBLockable* const pLockableObj, LPCSTR lpszFunction, const DWORD dwMilliseconds = LOCK_TIMEOUT);
	BOOL TryGetLock(BOOL bForWriting, const CTBLockable* const pLockableObj, LPCSTR lpszFunction, const DWORD dwMilliseconds = LOCK_TIMEOUT);
};


//=============================================================================
class TB_EXPORT BaseSmartLock
{
protected:
   	const CTBLockable*	m_pLockable;
	BOOL			m_bForWriting;
	LPSTR			m_pszFunction;
	BOOL			m_bLockAcquired;

	BaseSmartLock (LPSTR pszFunction);
	BaseSmartLock (const CTBLockable* ptr, BOOL bForWriting, LPSTR pszFunction);
public:
	void Assign(const BaseSmartLock& sPtr);
	virtual ~BaseSmartLock ();
};

//=============================================================================
template <class T> class SmartLockPtr : public BaseSmartLock
{
public:
   	//-----------------------------------------------------------------------------
	SmartLockPtr (T* ptr, BOOL bForWriting)
		: BaseSmartLock(ptr, bForWriting, __FUNCTION__)
	{
	}
   	//-----------------------------------------------------------------------------
	SmartLockPtr (const SmartLockPtr& sPtr)	
		: BaseSmartLock(__FUNCTION__)
	{
		Assign(sPtr);
	} 

	//-----------------------------------------------------------------------------
	SmartLockPtr& operator = (const SmartLockPtr& sPtr)	
	{
		Assign(sPtr);
		return *this;
	}
	
   	//-----------------------------------------------------------------------------
	~SmartLockPtr ()						
	{
	}
	//-----------------------------------------------------------------------------
	T* GetPointer()
	{
		return (T*) m_pLockable;
	}
	//-----------------------------------------------------------------------------
	operator BOOL	()				
	{
		return m_pLockable!=NULL;
	}
	//-----------------------------------------------------------------------------
	BOOL operator !()					
	{
		return m_pLockable==NULL;
	}
   	
   	//-----------------------------------------------------------------------------
	BOOL operator == (T* ptr)		
	{
		return m_pLockable == ptr;
	}
   	
	//-----------------------------------------------------------------------------
	BOOL operator == (int ptr)		
	{
		return m_pLockable == (T*)ptr;
	}
	//-----------------------------------------------------------------------------
	BOOL operator != (T* ptr)		
	{
		return m_pLockable != ptr;
	}
	//-----------------------------------------------------------------------------
	BOOL operator != (int ptr)		
	{
		return m_pLockable != (T*)ptr;
	}
   	//-----------------------------------------------------------------------------
	T* operator ->		()				
	{
		return (T*)m_pLockable;
	}
	//-----------------------------------------------------------------------------
	T& operator *		()				
	{
		return *((T*)m_pLockable);
	}
};



#define TB_LOCK_FOR_WRITE()			CTBMultiAutoLock _tbMultiLock(TRUE, this, __FUNCTION__ );
#define BEGIN_TB_LOCK_FOR_WRITE()	{ CTBMultiAutoLock _tbMultiLock(TRUE, this, __FUNCTION__ );
#define END_TB_LOCK_FOR_WRITE()		} 

#define TB_LOCK_FOR_READ()			CTBMultiAutoLock _tbMultiLock(FALSE, this, __FUNCTION__ );
#define BEGIN_TB_LOCK_FOR_READ()	{ CTBMultiAutoLock _tbMultiLock(FALSE, this, __FUNCTION__ );
#define END_TB_LOCK_FOR_READ()		} 

#define BEGIN_TB_LOCK_TIMEOUT_FOR_WRITE(milliseconds)	TRY { CTBMultiAutoLock _tbMultiLock(TRUE, this, __FUNCTION__, milliseconds);
#define BEGIN_TB_LOCK_TIMEOUT_FOR_READ(milliseconds)	TRY { CTBMultiAutoLock _tbMultiLock(FALSE, this, __FUNCTION__, milliseconds);

#define WHEN_FAILED()									} CATCH(CTBLockException, e) { 

#define END_TB_LOCK_TIMEOUT()							e->Delete();} END_CATCH

#define TB_OBJECT_LOCK(pLockable)		CTBMultiAutoLock _tbMultiLock(TRUE, pLockable, __FUNCTION__);
#define BEGIN_TB_OBJECT_LOCK(pLockable)	{ CTBMultiAutoLock _tbMultiLock(TRUE, pLockable, __FUNCTION__);
#define END_TB_OBJECT_LOCK()			} 
#define TB_OBJECT_LOCK_WHEN(condition, pLockable) CTBMultiAutoLock _tbMultiLock;\
													if (condition)\
														_tbMultiLock.GetLock(TRUE, pLockable, __FUNCTION__);
#define TB_OBJECT_LOCK_FOR_READ(pLockable)		CTBMultiAutoLock _tbMultiLock(FALSE, pLockable, __FUNCTION__);
#define BEGIN_TB_OBJECT_LOCK_FOR_READ(pLockable){ CTBMultiAutoLock _tbMultiLock(FALSE, pLockable, __FUNCTION__);


#define DECLARE_LOCKABLE(aClass, aVariable)\
class aClass##aVariable##_TBLockable : public aClass, public CTBLockable\
	{ virtual LPCSTR GetObjectName() const { return #aClass "." #aVariable; }\
} aVariable;

#define DECLARE_SMART_LOCK_PTR(aClass)			typedef SmartLockPtr<aClass> aClass##Ptr;
#define DECLARE_CONST_SMART_LOCK_PTR(aClass)	typedef SmartLockPtr<const aClass> aClass##ConstPtr;

//=============================================================================
#include "endh.dex"
