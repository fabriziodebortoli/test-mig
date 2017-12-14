
#include "stdafx.h"

#include <TbNameSolver\TbNamespaces.h>
#include <TbParser\SymTable.h>

#include "funproto.h"

//includere come ultimo include all'inizio del cpp
#include "begincpp.dex"

#ifdef _DEBUG
#undef THIS_FILE
static const char BASED_CODE THIS_FILE[] = __FILE__;
#endif

///////////////////////////////////////////////////////////////////////////////

////////////////////////////////////////////////////////////////////////////////
//									ReportDataInterface
////////////////////////////////////////////////////////////////////////////////
//
//------------------------------------------------------------------------------
IMPLEMENT_DYNCREATE(FunctionDataInterface, CFunctionDescription)

//------------------------------------------------------------------------------
FunctionDataInterface::FunctionDataInterface(CFunctionDescription* pFuncPrototype, SymTable* pSymTable/*=NULL*/)
	:
	CFunctionDescription	(*pFuncPrototype),
	IOSLObjectManager		(OSLType_Function),
	m_pSymTable				(pSymTable),
	m_pComponentClass		(NULL),
	m_pfFunction			(NULL),
	m_pControlClass			(NULL)
{
}

//------------------------------------------------------------------------------
FunctionDataInterface::FunctionDataInterface(const FunctionDataInterface& aRDI)
	:
	CFunctionDescription	(aRDI),
	IOSLObjectManager		(*const_cast<FunctionDataInterface&>(aRDI).GetInfoOSL()),
	m_pSymTable				(aRDI.m_pSymTable),
	m_pComponentClass		(aRDI.m_pComponentClass),
	m_pfFunction			(aRDI.m_pfFunction),
	m_pControlClass			(aRDI.m_pControlClass)
{
}

//------------------------------------------------------------------------------
FunctionDataInterface::FunctionDataInterface(const CTBNamespace& ns, TPFUNCTION	 pfFunction)
	:
	CFunctionDescription	(ns),
	IOSLObjectManager		(OSLType_Function),
	m_pSymTable				(NULL),
	m_pComponentClass		(NULL),
	m_pfFunction			(pfFunction),
	m_pControlClass			(NULL)
{
}

//------------------------------------------------------------------------------
FunctionDataInterface::FunctionDataInterface()
	:
	IOSLObjectManager		(OSLType_Function),
	m_pSymTable				(NULL),
	m_pComponentClass		(NULL),
	m_pfFunction			(NULL),
	m_pControlClass			(NULL)
{
}

//------------------------------------------------------------------------------
FunctionDataInterface::FunctionDataInterface(const CTBNamespace& ns, CRuntimeClass* pComponentClass, CRuntimeClass* pControlClass /*= NULL*/)
	:
	CFunctionDescription	(ns),
	IOSLObjectManager		(OSLType_Function),
	m_pSymTable				(NULL),
	m_pComponentClass		(pComponentClass),
	m_pfFunction			(NULL),
	m_pControlClass			(pControlClass)
{
}

//------------------------------------------------------------------------------
FunctionDataInterface::~FunctionDataInterface()
{
}

//------------------------------------------------------------------------------
CBaseDocument* FunctionDataInterface::GetDocument()
{
	return m_pSymTable ? m_pSymTable->GetDocument() : NULL;
}

//------------------------------------------------------------------------------
void FunctionDataInterface::Assign (const FunctionDataInterface& fd)
{
	m_pSymTable			= fd.m_pSymTable;

	*GetInfoOSL()		= *const_cast<FunctionDataInterface&>(fd).GetInfoOSL();
	m_pComponentClass	= fd.m_pComponentClass;
	m_pfFunction		= fd.m_pfFunction;
	m_pControlClass		= fd.m_pControlClass;

	__super::Assign(fd);
}


/////////////////////////////////////////////////////////////////////////////
// Diagnostics

#ifdef _DEBUG
void FunctionDataInterface::Dump(CDumpContext& dc) const
{
	ASSERT_VALID(this);
	AFX_DUMP0(dc, "FunctionDataInterface");
}

void FunctionDataInterface::AssertValid() const
{
	CFunctionDescription::AssertValid();
}
#endif //_DEBUG
/////////////////////////////////////////////////////////////////////////////
