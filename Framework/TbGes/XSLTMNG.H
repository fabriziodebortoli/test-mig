
#pragma once

#include <TbXmlCore\XSLTDocObj.h>

//----------------------------------------------------------------------------
//includere alla fine degli include del .H
#include "beginh.dex"

#define DEFAULT_NAMESPACE_XSLF_URI	_T("urn:XSLTFunction")

class CAbstractFormDoc;

//=============================================================================							
class TB_EXPORT CXSLTFunction : public CXSLTObject
{
protected: 
	DECLARE_DYNCREATE(CXSLTFunction)

	CAbstractFormDoc*	m_pDocument;
//	nella classe derivata: 
//	DECLARE_DISPATCH_MAP()

public:
	void AttachDocument(CAbstractFormDoc* pDocument);

	CXSLTFunction()								: CXSLTObject(DEFAULT_NAMESPACE_XSLF_URI), m_pDocument(NULL)	{ }
	CXSLTFunction(const CString &namespaceURI)	: CXSLTObject(namespaceURI), m_pDocument(NULL)	{ }

};



//=============================================================================							
class TB_EXPORT CXSLTManager: public CObject
{
protected: 
	
	CAbstractFormDoc	*m_pDocument;
	IXSLTemplate		*m_pIXSLTemplate;
	IXSLProcessor		*m_pIXSLProcessor;

	CXMLDocumentObject	*m_pXslDoc; //crea un free threaded
	CXMLDocumentObject	*m_pOutputDoc;
	BOOL Transform(CXMLDocumentObject* pInputDoc);

public:
	CXSLTManager(CAbstractFormDoc * pDoc);
	~CXSLTManager();
	BOOL Initialize(const CString& strXSLTFileName);
	
	BOOL TransformForImport(CXMLDocumentObject* pInputDoc);
	//BOOL TransformForImport(CString strInputDoc);
	BOOL TransformForExport(CXMLDocumentObject* pInputDoc);
	//BOOL TransformForExport(CString strInputDoc);

	BOOL Transform (CXMLDocumentObject* pInputDoc, const CString& strXSLTFileName);
	//BOOL Transform (CString strInputDoc, const CString& strXSLTFileName);

	CXMLDocumentObject* GetOutput() {return m_pOutputDoc;}
protected:
	
// diagnostics
#ifdef _DEBUG
public: 
    void Dump(CDumpContext&) const;
    void AssertValid() const;
#endif
};


#include "endh.dex"
