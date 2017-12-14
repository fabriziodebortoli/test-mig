#include "stdafx.h"

#include "XMLParser.h"
#include  <io.h>
//#include  <stdio.h>
//#include  <stdlib.h>

#ifdef _DEBUG
#undef THIS_FILE
static const char THIS_FILE[] = __FILE__;
#endif


//----------------------------------------------------------------------------------------------
//	CXMLParser
//----------------------------------------------------------------------------------------------
CXMLParser::CXMLParser(const CString& strFileName, CXMLDocumentObject*	pDocXML /* = NULL*/)
	:
	m_pDocXML		(pDocXML),
	m_pNodoDammi	(NULL),
	m_pTesta		(NULL),
	m_strFileName	(strFileName)
{
	m_bXMLDocOwner = (pDocXML == NULL); 
	if (m_bXMLDocOwner)
		m_pDocXML = new CXMLDocumentObject;
}


//----------------------------------------------------------------------------------------------
CXMLParser::~CXMLParser()
{
	if (m_bXMLDocOwner && m_pDocXML)
		delete m_pDocXML;
}

//fa funzionare il tutto: scorre tutto l'albero e ad ongi tag incontrata chiama TrovataTag
//e ad ogni foglia dell'albero chiama TrovataFoglia
//----------------------------------------------------------------------------------------------
BOOL CXMLParser::Parse ()
{
	ASSERT_VALID(m_pDocXML);

	if (!m_pDocXML->IsLoaded())
	{
		USES_CONVERSION;
		if(_access(T2CA((LPCTSTR)m_strFileName),00) == -1)
		{
			m_pTesta = NULL;
			return FALSE;
		}

		m_pDocXML->SetValidateOnParse(TRUE);
		if (!m_pDocXML->LoadXMLFile(m_strFileName))
		{
			CString strError;
			if (m_pDocXML->GetParseErrorString(strError))
				strError += _T("\n") + strError;
			TRACE2("CXMLParser::Parse: File %s, Error %s \n", m_strFileName, strError);
			return FALSE;
		}
	}

	m_pTesta = m_pDocXML->GetRoot();
	
	if(!m_pTesta) 
		return FALSE;

	CString strTesta;
	m_pTesta->GetName(strTesta);

	if (!strTesta.IsEmpty())
	{
		CString strTag = _T("<");
		strTag += strTesta;
		strTag += _T(">");
		m_strarTag.Add(strTag);

		ExploreNode(m_pTesta);
	}

	return TRUE;
}


//scorre tutto
//----------------------------------------------------------------------------------------------
HRESULT CXMLParser::ExploreNode(CXMLNode* pNodo)
{
	ASSERT_VALID(m_pDocXML);
	
	long				nRecCount;
	CString				strFields;
	CXMLNode*			pNodoFiglio = NULL;

	if(!pNodo)
	{
		ASSERT(FALSE);
		return 0;
	}

	nRecCount = pNodo->GetChildsNum();
	if (!nRecCount)	
		return 0;

	for (int i= 0; i < nRecCount; i++)
	{
		pNodoFiglio = pNodo->GetChildAt(i);
		if(pNodoFiglio == NULL)
		{
			ASSERT(FALSE);
			return 0;
		}

		pNodoFiglio->GetName(strFields);

		if(pNodoFiglio->GetChildsNum())
			ExploreNode(pNodoFiglio);
		else
		{
			//se non ha figli
			pNodoFiglio->GetText(m_strVal);
			if (m_strVal == _T(""))
			{
				CString strTag;
				pNodoFiglio->GetName(strTag);
				pNodoFiglio->GetText(m_strVal);
			}
			OnLeafFound(pNodoFiglio);
		}
	}
	return 1;
}

//----------------------------------------------------------------------------------------------
CXMLNode* CXMLParser::GetNode(const CString& Tag, const CString& Val)
{
	ASSERT_VALID(m_pDocXML);
	
	if(Tag.IsEmpty())
		return NULL;

	m_strTagDammi = Tag;

	m_strValDammi = Val;

	m_pNodoDammi = NULL;

	CXMLNode* pNodo = NULL;
	pNodo = m_pDocXML->GetRoot();
	if(pNodo == NULL)
	{
		ASSERT(FALSE);
		return NULL;
	}

	FindNode(pNodo);

	if(m_pNodoDammi == NULL)
	{
		ASSERT(FALSE);
		return NULL;
	}

	return m_pNodoDammi;
}

//----------------------------------------------------------------------------------------------
BOOL CXMLParser::Remove(CXMLNode* pNodo)
{
	ASSERT_VALID(m_pDocXML);
	
	if(pNodo == NULL)
	{
		ASSERT(FALSE);
		return 0;
	}

	m_pDocXML->RemoveNode(pNodo);

	return TRUE;
}

//----------------------------------------------------------------------------------------------
int CXMLParser::FindNode(CXMLNode* pNodo)
{
	ASSERT_VALID(m_pDocXML);
	
	long				nRecCount;
	CString				strFields;
	CXMLNode*			pNodoFiglio = NULL;

	if(pNodo == NULL)
	{
		ASSERT(FALSE);
		return 0;
	}
		
	nRecCount = pNodo->GetChildsNum();
	if (!nRecCount)
		return 0;

	for (int i= 0; i < nRecCount; i++)
	{
		pNodoFiglio = pNodo->GetChildAt(i);
		if(pNodoFiglio == NULL)
		{
			ASSERT(FALSE);
			return 0;
		}

		pNodoFiglio->GetName(strFields);

		if(pNodoFiglio->GetChildsNum())
		{
			if(m_strValDammi == _T("") && m_strTagDammi == m_strTagTutto)
			{
				m_pNodoDammi =  pNodoFiglio;
				return 1;
			}
			if(FindNode(pNodoFiglio))
				return 1;
		}
		else
		{
			m_strVal.Empty();

			pNodoFiglio->GetText(m_strVal);

			if(m_strValDammi == m_strVal && m_strTagDammi == GetGlobalTag(pNodoFiglio))
			{
				m_pNodoDammi = pNodoFiglio;
				return 1;
			}
		}
	}
	return 0;
}

//----------------------------------------------------------------------------------------------
CString CXMLParser::GetGlobalTag(CXMLNode* pNodo)
{
	ASSERT_VALID(m_pDocXML);
	
	if(pNodo == NULL)
	{
		ASSERT(FALSE);
		return _T("");
	}

	CString strTagTutto = _T("");
	CString tag;

	m_strarTag.RemoveAll();

	CXMLNode* pNodo2 = NULL;
	pNodo2 = pNodo->GetParentNode();
	if(pNodo2 == NULL)
	{
		ASSERT(FALSE);
		return _T("");
	}

	while (pNodo2 != NULL)
	{
		pNodo2->GetName(tag);

		CString t = _T("<");
		t += tag;
		t += _T(">");

		m_strarTag.Add(t);

		pNodo2 = pNodo2->GetParentNode();
	}

	for (int i = (m_strarTag.GetSize() - 1) ; i >= 0 ; i--)
		strTagTutto += m_strarTag.GetAt(i);

	return strTagTutto;
}

//----------------------------------------------------------------------------------------------
//	CXMLUnparser
//----------------------------------------------------------------------------------------------
CXMLUnparser::CXMLUnparser(const CString& strFileName, CXMLDocumentObject*	pDocXML /* = NULL*/)
	:
	m_pDocXML		(pDocXML),
	m_strFileName	(strFileName)
{
	m_bXMLDocOwner = (pDocXML == NULL); 
	if (m_bXMLDocOwner)
		m_pDocXML = new CXMLDocumentObject;
}

//----------------------------------------------------------------------------------------------
CXMLUnparser::~CXMLUnparser()
{
	if (m_bXMLDocOwner && m_pDocXML)
		delete m_pDocXML;
}


