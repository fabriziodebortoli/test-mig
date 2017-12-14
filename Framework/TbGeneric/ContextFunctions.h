#pragma once

#include "DataObj.h"

//includere alla fine degli include del .H
#include "beginh.dex"

// useful helper function for objects in a context bag
TB_EXPORT BOOL		CheckContextObject	(CContextBag* pContextBag, const CString& strName);
TB_EXPORT DataObj*	AddContextDataObj	(CContextBag* pContextBag, const DataStr& aName, CRuntimeClass* pClass, BOOL bAssertWhenExists = TRUE);
TB_EXPORT DataStr*  AddContextString	(CContextBag* pContextBag, const DataStr& aName);
TB_EXPORT DataInt*  AddContextInt		(CContextBag* pContextBag, const DataStr& aName);
TB_EXPORT DataDate* AddContextDate		(CContextBag* pContextBag, const DataStr& aName);
TB_EXPORT DataStr*  LookupContextString	(CContextBag* pContextBag, const DataStr& aName);
TB_EXPORT DataInt*  LookupContextInt	(CContextBag* pContextBag, const DataStr& aName);
TB_EXPORT DataDate* LookupContextDate	(CContextBag* pContextBag, const DataStr& aName);
TB_EXPORT DataStr	ReadContextString	(CContextBag* pContextBag, const DataStr& aName);
TB_EXPORT DataInt	ReadContextInt		(CContextBag* pContextBag, const DataStr& aName);
TB_EXPORT DataDate	ReadContextDate		(CContextBag* pContextBag, const DataStr& aName);

// useful helper function for thread context objects
TB_EXPORT BOOL		AfxCheckContextObject	(const CString& strName);
TB_EXPORT DataStr*  AfxAddContextString		(const DataStr& aName);
TB_EXPORT DataInt*  AfxAddContextInt		(const DataStr& aName);
TB_EXPORT DataDate* AfxAddContextDate		(const DataStr& aName);
TB_EXPORT DataStr*  AfxLookupContextString	(const DataStr& aName);
TB_EXPORT DataInt*  AfxLookupContextInt		(const DataStr& aName);
TB_EXPORT DataDate* AfxLookupContextDate	(const DataStr& aName);
TB_EXPORT DataStr	AfxReadContextString	(const DataStr& aName);
TB_EXPORT DataInt	AfxReadContextInt		(const DataStr& aName);
TB_EXPORT DataDate	AfxReadContextDate		(const DataStr& aName);

//includere alla fine degli include del .H
#include "endh.dex"
