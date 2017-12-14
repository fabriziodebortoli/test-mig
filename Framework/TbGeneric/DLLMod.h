#pragma once

//includere alla fine degli include del .H
#include "beginh.dex"

extern "C" int APIENTRY DllMain(HINSTANCE , DWORD , LPVOID );

//The AllocationBase member of MEMORY_BASIC_INFORMATION contains the memory 
//address where the DLL was loaded and this is also the DLL's HINSTANCE value.
#define	GET_DLL_HINSTANCE(hInstance) \
	{\
		MEMORY_BASIC_INFORMATION mbi; \
		VirtualQuery(DllMain, &mbi, sizeof(mbi)); \
		hInstance = (HINSTANCE)mbi.AllocationBase; \
	}

inline HINSTANCE GetDllInstance(const void* pDllObject)
{
	//prendo l'HINSTANCE della 
	MEMORY_BASIC_INFORMATION mbi;
	VirtualQuery(pDllObject, &mbi, sizeof(mbi));
	return (HINSTANCE)mbi.AllocationBase;
}

// Questa classe imposta nel costruttore la dll di default in cui cercare le risorse usando il puntatore passato per trovare la dll
class CSetResourceHandle
{
	HINSTANCE m_hOld;
public:
	CSetResourceHandle(HINSTANCE hDllInstance)
	{
		//prendo l'HINSTANCE della 
		HINSTANCE hOld = AfxGetResourceHandle();
		if (hDllInstance != hOld)
		{
			AfxSetResourceHandle(hDllInstance);
			m_hOld = hOld;
		}
		else
		{
			m_hOld = NULL;
		}
	}
	~CSetResourceHandle()
	{
		if (m_hOld)
			AfxSetResourceHandle(m_hOld);
	}
};

#include "endh.dex"
