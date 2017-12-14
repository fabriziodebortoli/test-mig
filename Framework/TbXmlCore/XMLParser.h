#pragma once

#include <afxdtctl.h>
#include "XMLDocObj.h"

//includere alla fine degli include del .H
#include "beginh.dex"

class CXMLNode;
class CXMLDocumentObject;

//----------------------------------------------------------------
//class CXMLParser 
//----------------------------------------------------------------
class TB_EXPORT CXMLParser
{
private:
	CString				m_strFileName;
	CString				m_strTagTesta;
	CString				m_strVal;
	CString				m_strTagTutto;
	CString				m_strTagDammi;
	CString				m_strValDammi;
	CStringArray		m_strarTag;
	CXMLDocumentObject*	m_pDocXML;
	CXMLNode*			m_pNodoDammi;
	CXMLNode*			m_pTesta;
	BOOL				m_bXMLDocOwner;

public:
	CXMLParser(const CString&, CXMLDocumentObject* = NULL);
	~CXMLParser();

private:
	int					FindNode(CXMLNode*);
	HRESULT				ExploreNode(CXMLNode*);
	long				GetChildNumber(CXMLNode*);	

public:
	BOOL				Remove(CXMLNode*);
	CXMLNode*			GetNode(const CString&, const CString&);
	BOOL				Parse();
	CString				GetGlobalTag(CXMLNode*);
	CXMLDocumentObject* GetDoc(){return m_pDocXML;}

protected:
	virtual void		OnLeafFound(CXMLNode*) = 0;
};


//----------------------------------------------------------------
//class CXMLUnparser 
//----------------------------------------------------------------
class TB_EXPORT CXMLUnparser
{
protected:
	CString				m_strFileName;
	CXMLDocumentObject*	m_pDocXML;
	BOOL				m_bXMLDocOwner;

public:
	CXMLUnparser(const CString&, CXMLDocumentObject* = NULL);
	~CXMLUnparser();

protected:
	virtual BOOL		Unparse() = 0;
};

#include "endh.dex"
