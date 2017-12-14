#include "stdafx.h"

#include <io.h>

#include <TbXmlCore\XMLTags.h>
#include <TbXmlCore\XMLDocObj.h>
#include <TbXmlCore\XMLParser.h>

#include <TbNameSolver\Chars.h>
#include <TbNameSolver\PathFinder.h>
#include <TbNameSolver\Diagnostic.h>

#include <TBGeneric\GeneralFunctions.h>
#include <TBGeneric\ReferenceObjectsInfo.h>
#include <TBGeneric\FormatsTable.h>

#include "XmlReferenceObjectsParser.h"
#include "DataObjDescriptionEx.h"

#include "XMLReferenceObjectsParser.h"

//includere come ultimo include all'inizio del cpp
#include "begincpp.dex"

#ifdef _DEBUG
#undef THIS_FILE
static const char THIS_FILE[] = __FILE__;
#endif

/////////////////////////////////////////////////////////////////////////////
// 				class CXMLReferenceObjectsParserBase Implementation
/////////////////////////////////////////////////////////////////////////////
// Class moved from TbParser to TbGenlib to avoid circular dependency (because this class needs 
// CDataObjDescriptionExpr, which is in TbGenlib)
//
IMPLEMENT_DYNAMIC(CXMLReferenceObjectsParserBase, CObject)
//----------------------------------------------------------------------------

