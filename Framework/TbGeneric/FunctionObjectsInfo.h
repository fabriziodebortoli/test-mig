#pragma once

#include <TbNameSolver\TbNamespaces.h>
#include <TbGeneric\DataObj.h>
#include <TbGeneric\FunctionCall.h>

//includere alla fine degli include del .H
#include "beginh.dex"

// FunctionObjects
//----------------------------------------------------------------
class TB_EXPORT CFunctionObjectsDescription : public CObject
{
	DECLARE_DYNCREATE(CFunctionObjectsDescription)
	bool					m_bLoaded;

	CBaseDescriptionArray	m_arFunctions;

public:
	CFunctionObjectsDescription();

public:
	
	// metodi di lettura
	CFunctionDescription*	GetFunctionInfo		(const CTBNamespace&) const;
	CFunctionDescription*	GetFunctionInfo		(const CString&) const;
	CFunctionDescription*	GetFunctionInfo		(const int&) const;
	CBaseDescriptionArray*	GetFunctionsByName	(const CString& sName) const;

	const CBaseDescriptionArray& GetFunctions	() const;

	bool		IsLoaded () { return m_bLoaded; }
	void		SetLoaded(bool bValue) { m_bLoaded = bValue;}

	void AddFunction	(CFunctionDescription*);
};

//----------------------------------------------------------------
class TB_EXPORT CFunctionDescriptionArray : public CObArray
{
public:
	CString m_sClassName;

	CFunctionDescriptionArray(){}
	virtual ~CFunctionDescriptionArray () { RemoveAll(); }

	INT_PTR	Add	(CFunctionDescription* pF) { return __super::Add (pF); }

	CFunctionDescription* GetAt		(INT_PTR nIdx) const { return (CFunctionDescription*)__super::GetAt (nIdx); }

	//CFunctionDescription* GetFunction	(const CTBNamespace& aNS) const;
	CFunctionDescription*	GetFunction			(const CString& sName) const;
};

//----------------------------------------------------------------
class TB_EXPORT CMapFunctionDescription : public CMapStringToOb
{
public:
	CMapFunctionDescription() {}

	virtual ~CMapFunctionDescription () 
		{ 
			POSITION pos;
			CString key;
			CFunctionDescription* pa;
   
			// Iterate through the entire map
			for (pos = GetStartPosition(); pos != NULL; )
			{
				GetNextAssoc(pos, key, (CObject*&)pa);
				if (pa) delete pa;
			}
			RemoveAll(); 
		}

};

#include "endh.dex"
