
#include "stdafx.h"


#include <TbGeneric\globals.h>
#include <TbNameSolver\PathFinder.h>

#include "ExtDoc.h"
#include "xsltmng.h"



//includere come ultimo include all'inizio del cpp
#include "begincpp.dex"
//============================================================================

#ifdef _DEBUG
#undef THIS_FILE
static const char BASED_CODE THIS_FILE[] = __FILE__;
#endif

/////////////////////////////////////////////////////////////////////////////
// CXSLTFunction

IMPLEMENT_DYNCREATE (CXSLTFunction, CXSLTObject)

//---------------------------------------------------------------------------
void CXSLTFunction::AttachDocument(CAbstractFormDoc* pDocument)
{
	ASSERT(pDocument);

	m_pDocument		= pDocument;
}

/*
	nella classe derivata: 

	BEGIN_DISPATCH_MAP( <nome-classe>, CXSLTFunction )
		DISP_FUNCTION( <nome-classe>, "ToUpperXSLT", ToUpper, VT_BSTR, VTS_BSTR)
	END_DISPATCH_MAP()

	(esempio di una funzione chiamata 'ToUpperXSLT' nel foglio di stile, 
	'ToUpper' nella classe, che restituisce una stringa e prende come input una stringa)
*/




/////////////////////////////////////////////////////////////////////////////
// CXSLTManager


//---------------------------------------------------------------------------
CXSLTManager::CXSLTManager(CAbstractFormDoc *pDoc)
{
	ASSERT(pDoc);
	
	m_pDocument = pDoc;
	m_pIXSLTemplate=NULL;
	m_pIXSLProcessor=NULL;
	
	m_pXslDoc = new CXMLDocumentObject (FALSE,TRUE, TRUE);
	m_pOutputDoc = new CXMLDocumentObject ();

	HRESULT hr = CoCreateInstance (		CLSID_XSLTemplate60, 
										NULL, 
										CLSCTX_INPROC_SERVER,
										IID_IXSLTemplate, 
										(LPVOID*)(&m_pIXSLTemplate));
	if(FAILED(hr)|| !m_pIXSLTemplate)
	{
		ASSERT(FALSE);
		if (m_pIXSLTemplate)
		{
			m_pIXSLTemplate->Release();
			m_pIXSLTemplate = NULL;
		}
	}
}

//---------------------------------------------------------------------------
CXSLTManager::~CXSLTManager()
{
	if (m_pIXSLTemplate)
		m_pIXSLTemplate->Release();
	if (m_pIXSLProcessor)
		m_pIXSLProcessor->Release();
	
	SAFE_DELETE(m_pXslDoc);
	SAFE_DELETE(m_pOutputDoc);
}


//---------------------------------------------------------------------------
BOOL CXSLTManager::Initialize(const CString& strXSLTFileName)
{
	if (!m_pDocument)
	{
		ASSERT(FALSE);
		return FALSE;
	}

	HRESULT hr;

	if (!ExistFile(strXSLTFileName))
		return FALSE;

	m_pXslDoc->SetAsync (FALSE);
	if (!m_pXslDoc->LoadXMLFile (strXSLTFileName))
	{

		ASSERT(FALSE);
		TRACE1("CXSLTManager::Initialize: it's impossible to load the file %s\n",strXSLTFileName); 
		return FALSE;
	}	

	try
	{
		hr = m_pIXSLTemplate->putref_stylesheet(m_pXslDoc->GetIXMLDOMDocumentPtr ());
		if (FAILED(hr)) throw hr;

		if (m_pIXSLProcessor)
		{
			m_pIXSLProcessor->Release();
			m_pIXSLProcessor = NULL;
		}

		hr = m_pIXSLTemplate->createProcessor(&m_pIXSLProcessor);
		if (FAILED(hr)) throw hr;
		
		// se il documento ha un gestore di funzioni esterne, lo aggancio
		if (m_pDocument->m_pXSLTFunction)
		{
			ASSERT(!m_pDocument->m_pXSLTFunction->GetNamespaceURI().IsEmpty());
			hr=m_pIXSLProcessor->addObject	(
											m_pDocument->m_pXSLTFunction->GetIDispatch(FALSE),
											_bstr_t(m_pDocument->m_pXSLTFunction->GetNamespaceURI())
											);
			if (FAILED(hr)) throw hr;
		}
	}
	catch(...)
	{
		
		if (m_pIXSLProcessor)
		{
			m_pIXSLProcessor->Release();
			m_pIXSLProcessor = NULL;
		}
		ASSERT(FALSE);
		return FALSE;
	}
	
	return TRUE;
}

//---------------------------------------------------------------------------
BOOL CXSLTManager::TransformForImport(CXMLDocumentObject* pInputDoc)
{
	return Transform(pInputDoc, AfxGetPathFinder()->GetDocumentXSLTImportFullName (m_pDocument->GetNamespace()));
}

////---------------------------------------------------------------------------
//BOOL CXSLTManager::TransformForImport(CString strInputDoc)
//{
//	CXMLDocumentObject inputDoc;
//    if(!inputDoc.SetAsync (FALSE) || !inputDoc.LoadXMLFile(strInputDoc))
//	{
//		ASSERT(FALSE);
//		return FALSE;
//	}
//	
//	return TransformForImport(&inputDoc);
//}

//---------------------------------------------------------------------------
BOOL CXSLTManager::TransformForExport(CXMLDocumentObject* pInputDoc)
{
	return Transform(pInputDoc, AfxGetPathFinder()->GetDocumentXSLTExportFullName (m_pDocument->GetNamespace()));
}

//---------------------------------------------------------------------------
//BOOL CXSLTManager::TransformForExport(CString strInputDoc)
//{
//	CXMLDocumentObject inputDoc;
//    if(!inputDoc.SetAsync (FALSE) || !inputDoc.LoadXMLFile(strInputDoc))
//	{
//		ASSERT(FALSE);
//		return FALSE;
//	}
//	
//	return TransformForExport(&inputDoc);
//}

//---------------------------------------------------------------------------
BOOL CXSLTManager::Transform(CXMLDocumentObject* pInputDoc)
{
	if(!pInputDoc)
	{
		ASSERT(FALSE);
		return FALSE;
	}

	HRESULT hr;
	VARIANT_BOOL bResult = VARIANT_FALSE; 
	
	if(m_pIXSLProcessor)
	{
		hr = m_pIXSLProcessor->put_input(_variant_t (pInputDoc->GetIXMLDOMDocumentPtr ()));
		if (FAILED(hr)) return FALSE;

		hr = m_pIXSLProcessor->put_output(_variant_t (m_pOutputDoc->GetIXMLDOMDocumentPtr ()));
		if (FAILED(hr)) return FALSE;

		hr = m_pIXSLProcessor->transform(&bResult);
		if ( FAILED(hr) || bResult==VARIANT_FALSE ) return FALSE;	
	}
	else
		return FALSE;
                            
	return TRUE;
}



//---------------------------------------------------------------------------
BOOL CXSLTManager::Transform(CXMLDocumentObject* pInputDoc, const CString& strXSLTFileName)
{
	return Initialize(strXSLTFileName) && Transform(pInputDoc);
}

//---------------------------------------------------------------------------
/////////////////////////////////////////////////////////////////////////////
// Diagnostics

#ifdef _DEBUG
void CXSLTManager::Dump(CDumpContext& dc) const
{
	ASSERT_VALID(this);
	AFX_DUMP0(dc, "CXSLTManager\n");
}

void CXSLTManager::AssertValid() const
{
	CObject::AssertValid();
}
#endif //_DEBUG

//===============================================================================