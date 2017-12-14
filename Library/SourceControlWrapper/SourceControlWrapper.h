// SourceControlWrapper.h

#pragma once

using namespace System;
using namespace System::Diagnostics;
using namespace System::Collections;

namespace SourceControlWrapper
{

	//================================================================================
	public __gc class ObjectRepository
	{
		static long latestHandle = 0;
		static Hashtable* m_pObjects = new Hashtable();
		static Hashtable* m_pReferences = new Hashtable();
		
	public:
		//--------------------------------------------------------------------------------
		static long Add (System::Object *value)
		{
			if (value == NULL) return 0;

			long handle = ++latestHandle;
			m_pObjects->Add(handle.ToString(), value);
			return handle;
		}
		
		//--------------------------------------------------------------------------------
		static void AddReference (long handle)
		{
			if (handle == 0) return;

			int nLastReference = GetReferenceCount(handle);
			
			Int32* p = new Int32(++nLastReference);	
			m_pReferences->set_Item(handle.ToString(),  reinterpret_cast<Object*>(p));
		}

		//--------------------------------------------------------------------------------
		static int RemoveReference (long handle)
		{
			if (handle == 0) return 0;

			int nLastReference = GetReferenceCount(handle);
			if (--nLastReference == 0)
				m_pReferences->Remove(handle.ToString());
			else
			{
            	Int32* p = new Int32(nLastReference);
				m_pReferences->set_Item(handle.ToString(),  reinterpret_cast<Object*>(p));
			}
			return nLastReference;
		}

		//--------------------------------------------------------------------------------
		static int GetReferenceCount (long handle)
		{
			System::Object* o = m_pReferences->get_Item(handle.ToString());
			if (o == NULL)
				return 0;
			return *reinterpret_cast<Int32*>(o);

		}

		//--------------------------------------------------------------------------------
		static void Delete(long handle)
		{
			if (handle == 0) return;

			if (RemoveReference(handle) > 0) 
				return;

			m_pObjects->Remove(handle.ToString());
		}

		//--------------------------------------------------------------------------------
		static System::Object* GetAt(long handle)
		{
			if (handle == 0) return NULL;

			return m_pObjects->get_Item(handle.ToString());
		}

	};
}
