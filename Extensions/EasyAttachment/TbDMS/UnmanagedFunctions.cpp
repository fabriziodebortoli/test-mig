#include "stdafx.h" 

#include <TbClientCore\ClientObjects.h>
#include <TBNameSolver\ThreadContext.h>
#include <tbnamesolver\FileSystemFunctions.h>
#include <TBGENERIC\globals.h>
#include <TBGENERIC\dataobj.h>
#include <TBGENLIB\baseapp.h>
#include <TbOleDb\OleDbMng.h>

#include "TbRepositoryManager.h"


//this file is used to implement all function which need only unmanaged file. 
// For example to call templated functions as GetObject
// the code AfxGetLoginContext()->GetObject<TbRepositoryManager>() in a mixedmode file gives rise to a linker error
////-----------------------------------------------------------------------------
//TbRepositoryManager* AFXAPI AfxGetTbRepositoryManager()
//{ 
//	return AfxGetLoginContext()->GetObject<TbRepositoryManager>();
//}  
