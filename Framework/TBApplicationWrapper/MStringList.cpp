#include "StdAfx.h"
#include "MStringList.h"

using namespace Microarea::Framework::TBApplicationWrapper;
using namespace System::Collections::Generic;

using namespace System;

ref class StringEnum : public IEnumerator<System::String^>
{
	CStringArray* m_parList;
	int i;
public: 
	StringEnum(CStringArray* parList)
		: m_parList(parList), i(-1)
	{
	}
	~StringEnum()
	{
	}
	property System::String^ Current
	{
		virtual System::String^ get() 
		{
			return i < 0 || i >= m_parList->GetCount() 
				? nullptr 
				: gcnew System::String((*m_parList)[i]); 
		}
	}
	property Object^ Current1
	{
		virtual Object^ get() = System::Collections::IEnumerator::Current::get 
		{
			return Current; 
		}
	} 

	virtual bool MoveNext()
	{
		i++;
		return i < m_parList->GetCount();
	}
	virtual void Reset()
	{
		i = -1;
	}
};

//-----------------------------------------------------------------------------                      
IEnumerator<System::String^>^ MStringList::GetEnumerator1(void) 
{
	return gcnew StringEnum(m_parList);	
}
	
//-----------------------------------------------------------------------------                      
System::Collections::IEnumerator^ MStringList::GetEnumerator()
{
	return GetEnumerator1();
}

//-----------------------------------------------------------------------------                      
int MStringList::Count::get() 
{
	return m_parList->GetCount(); 
}
//-----------------------------------------------------------------------------                      
bool MStringList::IsReadOnly::get() { return false; }
//-----------------------------------------------------------------------------                      
void MStringList::Add(System::String^ s)
{
	m_parList->Add(s);
}
//-----------------------------------------------------------------------------                      
bool MStringList::Remove(System::String^ s)
{
	for (int i = 0; i < m_parList->GetCount(); i++)
		if ((*m_parList)[i] == s)
		{
			m_parList->RemoveAt(i);
			return true;
		}
	return false;
}
//-----------------------------------------------------------------------------                      
void MStringList::Clear()
{
	m_parList->RemoveAll();
}
//-----------------------------------------------------------------------------                      
bool MStringList::Contains(System::String^ s)
{
	for (int i = 0; i < m_parList->GetCount(); i++)
		if ((*m_parList)[i] == s)
			return true;
	return false;
}

//-----------------------------------------------------------------------------                      
void MStringList::RemoveAt(int index)
{
	if (index < 0 || index >= m_parList->GetCount())
		throw gcnew IndexOutOfRangeException();
	
	m_parList->RemoveAt(index);
}
//-----------------------------------------------------------------------------                      
void MStringList::Insert(int index, System::String^ s)
{
	m_parList->SetAtGrow(index, s);
}
//-----------------------------------------------------------------------------                      
int MStringList::IndexOf(System::String^ s)
{
	for (int i = 0; i < m_parList->GetCount(); i++)
		if ((*m_parList)[i] == s)
			return i;
	return -1;
}

//-----------------------------------------------------------------------------                      
void MStringList::set_Item(int index, System::String^ s)
{
	m_parList->SetAtGrow(index, s);
}
//-----------------------------------------------------------------------------                      
System::String^ MStringList::get_Item(int index)
{
	if (index < 0 || index >= m_parList->GetCount())
		throw gcnew IndexOutOfRangeException();
		
	return gcnew System::String(m_parList->GetAt(index));
}

//-----------------------------------------------------------------------------                      
void MStringList::CopyTo(cli::array<System::String^>^ ar, int index)
{
	for (int i = 0; i < m_parList->GetCount(); i++)
		ar[index + i] = gcnew System::String((*m_parList)[i]);
}