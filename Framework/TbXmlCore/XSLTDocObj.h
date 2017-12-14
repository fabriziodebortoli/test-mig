
#pragma once

#include <comdef.h>

#include "XMLDocObj.h"

//includere alla fine degli include del .H
#include "beginh.dex"

#define DEFAULT_NAMESPACE_URI	_T("urn:XSLTObject")

//=============================================================================							
class TB_EXPORT CXSLTObject : public CCmdTarget
{
protected: 
	DECLARE_DYNCREATE(CXSLTObject)

	CString				m_strNamespaceURI;
//	nella classe derivata: 
//	DECLARE_DISPATCH_MAP()

public:
	void SetNamespaceURI(const CString &namespaceURI)	{m_strNamespaceURI = namespaceURI;}
	const CString& GetNamespaceURI()					{return m_strNamespaceURI;}

	CXSLTObject()								{ EnableAutomation(); SetNamespaceURI(DEFAULT_NAMESPACE_URI);}
	CXSLTObject(const CString &namespaceURI)	{ EnableAutomation(); SetNamespaceURI(namespaceURI);}

};



class TB_EXPORT CXSLTDocumentObject: public CXMLDocumentObject
{
	CString	m_strInputXSLTFileName, m_strOutputXSLTFileName;
	
	CXMLDocumentObject	*m_pXslDoc; //crea un free threaded

	IXSLTemplate		*m_pIXSLInputTemplate, *m_pIXSLOutputTemplate;
	CObArray			m_XSLTObjects;

public:
	CXSLTDocumentObject(BOOL bMsgMode = TRUE, BOOL bFreeThreaded = FALSE);
	virtual ~CXSLTDocumentObject();

	void		AddXSLTObject(CXSLTObject* pObj)	{m_XSLTObjects.Add (pObj);}
	void		SetXSLTFileName (const CString& strInputXSLTFileName, const CString& strOutputXSLTFileName = _T(""));

			BOOL		Transform				(const CXMLDocumentObject& inputDoc, const CXMLDocumentObject& outputDoc, IXSLProcessor* pIXSLProcessor);

	virtual BOOL		LoadXML					(LPCTSTR);
	virtual BOOL		LoadXMLFile				(const CString&);
	virtual BOOL		SaveXMLFile				(const CString&, BOOL = FALSE);

protected:
	void ClearXSLTObjects();
	IXSLProcessor* GetProcessor(BOOL bForInput);
	void ReleaseProcessor(IXSLProcessor*);
};

#include "endh.dex"
