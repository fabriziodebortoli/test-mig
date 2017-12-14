
#include "stdafx.h"

#include "stack.h"

//includere come ultimo include all'inizio del cpp
#include "begincpp.dex"

#ifdef _DEBUG
#undef THIS_FILE
static const char BASED_CODE THIS_FILE[] = __FILE__;
#endif

//----------------------------------------------------------------------------
Stack::Stack()
{
	m_bOwnsElements = TRUE;
}

//----------------------------------------------------------------------------
Stack::~Stack()
{
	ClearStack();
}

//----------------------------------------------------------------------------
void Stack::ClearStack()
{
	if (m_bOwnsElements)
	{
	  int n = GetSize();
	  for (int i = 0; i < n; i++) 
		if (GetAt(i)) 
			delete GetAt(i);
	}
	
	RemoveAll();
}

//----------------------------------------------------------------------------
void Stack::SetOwns(BOOL bIAmOwns)
{
	m_bOwnsElements = bIAmOwns;
}

//----------------------------------------------------------------------------
void Stack::Push(CObject* pObject)
{
	if (pObject == NULL) return;
	InsertAt(0, pObject);
}

//----------------------------------------------------------------------------
CObject* Stack::Pop()
{                                       
	if (!GetSize()) return NULL;
	
	CObject* pObject = GetAt(0);
	RemoveAt(0);
	return pObject;
}

//----------------------------------------------------------------------------
CObject* Stack::Top()
{
	return (GetSize() ? GetAt(0) : NULL);
}

//----------------------------------------------------------------------------
BOOL Stack::IsEmpty() const 
{
	return (GetSize() == 0);
}
                        
                        
//============================================================================
//                        	Class FixedSizeStack
//============================================================================

//----------------------------------------------------------------------------
FixedSizeStack::FixedSizeStack(int nStackSize) : Stack()
{
	m_nSize = nStackSize;
}


//----------------------------------------------------------------------------
void FixedSizeStack::SetSize (int nSize)
{
	if (nSize >= m_nSize)
	{
		m_nSize = nSize;
		return;
	}
	
	for (int i = GetUpperBound(); i > nSize; i--)
	{
		if (m_bOwnsElements)
			delete GetAt(i);
		RemoveAt(i);
	}
	
	m_nSize = nSize;
}

//----------------------------------------------------------------------------
void FixedSizeStack::Push(CObject* pObject)
{
	if (pObject == NULL) return;
	if (GetSize() >= m_nSize)
	{
		int last = GetUpperBound();
		if (m_bOwnsElements)
			delete GetAt(last);
		RemoveAt(last);
	}
			
	InsertAt(0,pObject);
}
