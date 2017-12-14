#include "StdAfx.h"

#include <TbNameSolver\FileSystemFunctions.h>
#include "XSLTDocObj.h"

/////////////////////////////////////////////////////////////////////////////
// CXSLTObject
/////////////////////////////////////////////////////////////////////////////

IMPLEMENT_DYNCREATE (CXSLTObject, CCmdTarget)


/*
	nella classe derivata: 

	BEGIN_DISPATCH_MAP( <nome-classe>, CXSLTObject )
		DISP_FUNCTION( <nome-classe>, "ToUpperXSLT", ToUpper, VT_BSTR, VTS_BSTR)
	END_DISPATCH_MAP()

	(esempio di una funzione chiamata 'ToUpperXSLT' nel foglio di stile, 
	'ToUpper' nella classe, che restituisce una stringa e prende come input una stringa)
*/



/////////////////////////////////////////////////////////////////////////////
// CXSLTDocumentObject
/////////////////////////////////////////////////////////////////////////////

//=============================================================================
CXSLTDocumentObject::CXSLTDocumentObject(BOOL bMsgMode /*= TRUE*/, BOOL bFreeThreaded /*= FALSE*/)
:
CXMLDocumentObject(FALSE, bMsgMode, bFreeThreaded),
m_pXslDoc(NULL),
m_pIXSLInputTemplate(NULL),
m_pIXSLOutputTemplate(NULL)
{
}

//=============================================================================
CXSLTDocumentObject::~CXSLTDocumentObject(void)
{
	delete m_pXslDoc; 
	
	ClearXSLTObjects();

	if(m_pIXSLInputTemplate) m_pIXSLInputTemplate->Release();
	if(m_pIXSLOutputTemplate) m_pIXSLOutputTemplate->Release();
}

//=============================================================================
void CXSLTDocumentObject::ClearXSLTObjects()
{
	for (int i=0; i<m_XSLTObjects.GetSize(); i++)
		delete m_XSLTObjects[i];
	
	m_XSLTObjects.RemoveAll();
}

//=============================================================================
void CXSLTDocumentObject::SetXSLTFileName (const CString& strInputXSLTFileName, const CString& strOutputXSLTFileName) 
{
	m_strInputXSLTFileName = strInputXSLTFileName;
	m_strOutputXSLTFileName = strOutputXSLTFileName;
	if(m_pIXSLInputTemplate)
	{
		m_pIXSLInputTemplate->Release();
		m_pIXSLInputTemplate = NULL;
	}
	if(m_pIXSLOutputTemplate)
	{
		m_pIXSLOutputTemplate->Release();
		m_pIXSLOutputTemplate = NULL;
	}

}


//=============================================================================
BOOL CXSLTDocumentObject::LoadXML (LPCTSTR lpszXML)
{
	IXSLProcessor		*pIXSLProcessor = GetProcessor(TRUE);

	// se non ho il processor, mi comporto come un normale documento
	if(!pIXSLProcessor)
		return __super::LoadXML (lpszXML);

	CXMLDocumentObject tmpDoc (FALSE,FALSE,FALSE);

	if (m_pRoot)
	{
		delete m_pRoot;
		m_pRoot = NULL;
	}

	if(!tmpDoc.LoadXML (lpszXML))
	{
		ReleaseProcessor(pIXSLProcessor);
		return FALSE;
	}

	m_bLoaded = TRUE;

	BOOL bRes = Transform(tmpDoc, *this, pIXSLProcessor);
	ReleaseProcessor(pIXSLProcessor);

	return bRes;
}

//=============================================================================
BOOL CXSLTDocumentObject::LoadXMLFile (const CString& strFile)
{
	IXSLProcessor		*pIXSLProcessor = GetProcessor(TRUE);

	// se non ho il processor, mi comporto come un normale documento
	if(!pIXSLProcessor)
		return __super::LoadXMLFile (strFile);

	CXMLDocumentObject tmpDoc (FALSE,FALSE,FALSE);


	if(!tmpDoc.LoadXMLFile (strFile))
	{
		ReleaseProcessor(pIXSLProcessor);
		return FALSE;
	}

	if (m_pRoot)
	{
		delete m_pRoot;
		m_pRoot = NULL;
	}

	m_bLoaded = TRUE;

	BOOL bRes = Transform(tmpDoc, *this, pIXSLProcessor);
	ReleaseProcessor(pIXSLProcessor);
	
	return bRes;
}

//----------------------------------------------------------------------------
BOOL CXSLTDocumentObject::SaveXMLFile(const CString& strFileName, BOOL bCreatePath /*= FALSE*/)
{
	IXSLProcessor		*pIXSLProcessor = GetProcessor(FALSE);  

	// se non ho il processor, mi comporto come un normale documento
	if(!pIXSLProcessor)
		return __super::SaveXMLFile(strFileName, bCreatePath);

	CXMLDocumentObject tmpDoc;
	BOOL bRes = Transform(*this, tmpDoc, pIXSLProcessor);
	ReleaseProcessor(pIXSLProcessor);
	
	return bRes && tmpDoc.SaveXMLFile(strFileName, bCreatePath);
}

//=============================================================================
BOOL CXSLTDocumentObject::Transform (const CXMLDocumentObject& inputDoc, const CXMLDocumentObject& outputDoc, IXSLProcessor* pIXSLProcessor)
{
	HRESULT hr;
	VARIANT_BOOL bResult = VARIANT_FALSE; 
	
	if(pIXSLProcessor && inputDoc.GetIXMLDOMDocumentPtr() && outputDoc.GetIXMLDOMDocumentPtr())
	{
		hr = pIXSLProcessor->put_input(_variant_t (inputDoc.GetIXMLDOMDocumentPtr ()));
		if (FAILED(hr)) return FALSE;

		hr = pIXSLProcessor->put_output(_variant_t (outputDoc.GetIXMLDOMDocumentPtr ()));
		if (FAILED(hr)) return FALSE;

		hr = pIXSLProcessor->transform(&bResult);
		if ( FAILED(hr) || bResult==VARIANT_FALSE ) return FALSE;	
	}
	else
	{
		ASSERT(FALSE);
		return FALSE;
	}
            
	return TRUE;
}

//=============================================================================
void CXSLTDocumentObject::ReleaseProcessor(IXSLProcessor* pIXSLProcessor)
{
	if(!pIXSLProcessor) return;

	// se ho gestori di funzioni esterne, li rilascio
	for (int i=0; i<m_XSLTObjects.GetSize(); i++)
	{
		CXSLTObject *pObj = (CXSLTObject*) m_XSLTObjects[i];

		ASSERT(!pObj->GetNamespaceURI().IsEmpty());
		pIXSLProcessor->addObject	(
										NULL,	//rilascia l'oggetto
										_bstr_t(pObj->GetNamespaceURI())
									);
	}
	pIXSLProcessor->Release();

}

//=============================================================================
IXSLProcessor* CXSLTDocumentObject::GetProcessor(BOOL bForInput)
{
	IXSLProcessor *pIXSLProcessor = NULL;

	IXSLTemplate **ppTemplate = bForInput 
									? &m_pIXSLInputTemplate 
									: (m_strInputXSLTFileName==m_strOutputXSLTFileName) //ottimizzo
										? &m_pIXSLInputTemplate
										: &m_pIXSLOutputTemplate;
	try
	{
		if(bForInput)
		{
			if(!ExistFile (m_strInputXSLTFileName)) return NULL;
		}
		else
		{
			if(!ExistFile (m_strOutputXSLTFileName)) return NULL;
		}

		HRESULT hr;

		if(!*ppTemplate)
		{
			
			hr = CoCreateInstance (		CLSID_XSLTemplate60, 
											NULL, 
											CLSCTX_INPROC_SERVER,
											IID_IXSLTemplate, 
											(LPVOID*)(ppTemplate));
		
			if(FAILED(hr) || !*ppTemplate) 
				throw hr;
			
			delete m_pXslDoc;
			m_pXslDoc = new CXMLDocumentObject (FALSE, FALSE, TRUE);

			m_pXslDoc->SetAsync (FALSE);
			CString strXSLTFileName = bForInput ? m_strInputXSLTFileName : m_strOutputXSLTFileName;
			if (!m_pXslDoc->LoadXMLFile (strXSLTFileName))
			{
				TRACE1("It's impossible to load the file %s\n",strXSLTFileName); 
				ASSERT(FALSE);
				throw -1;
			}	

			hr = (*ppTemplate)->putref_stylesheet(m_pXslDoc->GetIXMLDOMDocumentPtr());
			if (FAILED(hr)) throw hr;
		}

	
		hr = (*ppTemplate)->createProcessor(&pIXSLProcessor);
		if (FAILED(hr) || !pIXSLProcessor) throw hr;
		
		// se ho gestori di funzioni esterne, li aggancio
		for (int i=0; i<m_XSLTObjects.GetSize(); i++)
		{
			CXSLTObject *pObj = (CXSLTObject*) m_XSLTObjects[i];

			ASSERT(!pObj->GetNamespaceURI().IsEmpty());
			hr=pIXSLProcessor->addObject	(
											pObj->GetIDispatch(TRUE),
											_bstr_t(pObj->GetNamespaceURI())
											);
			if (FAILED(hr)) throw hr;
		}
	}
	catch(...)
	{
		ASSERT(FALSE);

		if (*ppTemplate)
		{
			(*ppTemplate)->Release();
			(*ppTemplate) = NULL;
		}

		if (pIXSLProcessor)
		{
			pIXSLProcessor->Release();
			pIXSLProcessor = NULL;
		}
	}

	return pIXSLProcessor;
	
}
