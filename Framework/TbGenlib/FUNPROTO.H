
#pragma once

#include <afxtempl.h>		

#include <TbGeneric\Array.h>
#include <TbGeneric\DataObj.h>
#include <TbGeneric\FunctionObjectsInfo.h>
#include <TbGeneric\FunctionCall.h>

#include "OslInfo.h"

//includere alla fine degli include del .H
#include "beginh.dex"

class Parser;
class Unparser;
class SymTable;
class FunctionDataInterface;

typedef  BOOL (* TPFUNCTION) (FunctionDataInterface*);

//=============================================================================
class TB_EXPORT FunctionDataInterface : public CFunctionDescription, public IOSLObjectManager
{
	DECLARE_DYNCREATE(FunctionDataInterface)
	
protected:
	SymTable*		m_pSymTable;

public:
	CRuntimeClass*		m_pComponentClass;
	TPFUNCTION			m_pfFunction;
	CRuntimeClass*		m_pControlClass;

public:
	FunctionDataInterface(CFunctionDescription*, SymTable* = NULL);
	FunctionDataInterface(const FunctionDataInterface&);
	FunctionDataInterface(const CTBNamespace& ns, TPFUNCTION pfFunction);
	FunctionDataInterface(const CTBNamespace& ns, CRuntimeClass* pComponentClass, CRuntimeClass* pControlClass = NULL);
	FunctionDataInterface();
	virtual ~FunctionDataInterface();

public:
	SymTable*		GetSymTable		() { return m_pSymTable; }
	CBaseDocument*	GetDocument		();
	void			SetFunction			(TPFUNCTION	 pfFunction) { m_pfFunction = pfFunction; }

	void			Assign			(const FunctionDataInterface& fd);

	// Diagnostics
#ifdef _DEBUG
public:
	void Dump (CDumpContext&) const;
	void AssertValid() const;
#endif // _DEBUG
};

#include "endh.dex"
