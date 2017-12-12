
#include "stdafx.h"
#include <io.h>

#include <TBxmlcore\xmlgeneric.h>
#include <TBxmlcore\XMLSchema.h>

#include <TBGeneric\DataObj.h>
    
#include <TBGeneric\FormatsTable.h>
#include <TBNameSolver\FileSystemFunctions.h>

#include "XMLEnvelopeTags.h"
#include "XMLEnvInfo.h"
#include "GenFunc.h"
#include "XEngineObject.h"

#ifdef _DEBUG 
#undef THIS_FILE
static const char THIS_FILE[] = __FILE__;
#endif
 

//----------------------------------------------------------------
//class CXMLEnvFile implementation
//----------------------------------------------------------------
//
//----------------------------------------------------------------
CXMLEnvFile::CXMLEnvFile()
	:
	m_eFileType(UNDEF_FILE)
	
{
}

//----------------------------------------------------------------
CXMLEnvFile::CXMLEnvFile
				(
					ContentFileType eFileType, 
					LPCTSTR			lpszUrlDati, 
					LPCTSTR			lpszProfile		/*= NULL*/, 
					LPCTSTR			lpszEnvClass	/*= NULL*/, 
					LPCTSTR			lpszDocName		/*= NULL*/, 
					int				nDocNum			/*= 0*/	
				)
	:
	m_eFileType			(eFileType), 
	m_strUrlData		(lpszUrlDati), 
	m_strEnvClass		(lpszEnvClass),  
	m_strDocumentName	(lpszDocName),  
	m_nDocumentNumb		(nDocNum)		
{
}

//----------------------------------------------------------------
CXMLEnvFile::CXMLEnvFile(const CXMLEnvFile& aXMLEnvFile)
{
	*this = aXMLEnvFile;
}

//----------------------------------------------------------------
CString	CXMLEnvFile::GetFormatDocNumb()	const
{ 
	CString strMd;
	strMd.Format(_T("%d"), m_nDocumentNumb);
	
	return strMd;	
};

//----------------------------------------------------------------
void CXMLEnvFile::SetDocNumb(const CString& strDocNumb)	
{ 
	 m_nDocumentNumb = _ttoi(strDocNumb);
}

// restituisce la stringa corrispondente al tipo associata all'envfile
//----------------------------------------------------------------
CString	CXMLEnvFile::GetStrFileType() const 
{ 
	switch (m_eFileType)
	{
		case SCHEMA_FILE:	return	ENV_XML_FILE_SCHEMA_TYPE;
		case ROOT_FILE:		return	ENV_XML_FILE_ROOT_TYPE	;
		case NEXT_ROOT_FILE:return	ENV_XML_FILE_NEXT_ROOT_TYPE	;
		case XREF_FILE:		return	ENV_XML_FILE_XREF_TYPE;
		case ENV_FILE:		return	ENV_XML_FILE_ENV_TYPE;

		default: return ENV_XML_FILE_UNDEF_TYPE;
	}	
}

//----------------------------------------------------------------------------------------------
void CXMLEnvFile::SetType(const CString& strType)
{
	if (!strType.CompareNoCase(ENV_XML_FILE_SCHEMA_TYPE))		
	{
		m_eFileType = SCHEMA_FILE;
		return;
	}

	if (!strType.CompareNoCase(ENV_XML_FILE_ROOT_TYPE))
	{
		m_eFileType = ROOT_FILE;
		return;
	}

	if (!strType.CompareNoCase(ENV_XML_FILE_NEXT_ROOT_TYPE))
	{
		m_eFileType = NEXT_ROOT_FILE;
		return;
	}

	if (!strType.CompareNoCase(ENV_XML_FILE_XREF_TYPE))
	{
		m_eFileType = XREF_FILE;
		return;
	}

	if (!strType.CompareNoCase(ENV_XML_FILE_ENV_TYPE))
	{
		m_eFileType = ENV_FILE;
		return;
	}	
				
	m_eFileType =  UNDEF_FILE;
}

//----------------------------------------------------------------------------------------------
BOOL CXMLEnvFile::Parse(CXMLNode* pNode)
{
	if (!pNode) 
		return FALSE;

	CString strAttrValue;

	pNode->GetAttribute(ENV_XML_TYPE_ATTRIBUTE,		strAttrValue);
	SetType(strAttrValue);
	pNode->GetAttribute(ENV_XML_DATA_URL_ATTRIBUTE,	m_strUrlData);
	pNode->GetAttribute(ENV_XML_ENVCLASS_ATTRIBUTE,	m_strEnvClass);
	pNode->GetAttribute(ENV_XML_DOC_NAME_ATTRIBUTE,	m_strDocumentName);
	pNode->GetAttribute(ENV_XML_DOC_NUMB_ATTRIBUTE,	strAttrValue);		
	SetDocNumb(strAttrValue);

	return TRUE;
}

//----------------------------------------------------------------------------------------------
BOOL CXMLEnvFile::Unparse(CXMLNode* pNode)
{
	if (!pNode) return FALSE;
	
	pNode->SetAttribute(ENV_XML_TYPE_ATTRIBUTE,			(LPCTSTR)GetStrFileType());
	pNode->SetAttribute(ENV_XML_DATA_URL_ATTRIBUTE,		(LPCTSTR)m_strUrlData);
	
	if (!m_strEnvClass.IsEmpty())
		pNode->SetAttribute(ENV_XML_ENVCLASS_ATTRIBUTE,	(LPCTSTR)m_strEnvClass);
	
	if (!m_strDocumentName.IsEmpty())
		pNode->SetAttribute(ENV_XML_DOC_NAME_ATTRIBUTE,	(LPCTSTR)m_strDocumentName);
	
	if (m_nDocumentNumb > 0)
		pNode->SetAttribute(ENV_XML_DOC_NUMB_ATTRIBUTE,	(LPCTSTR)GetFormatDocNumb());

	return TRUE;
}

//----------------------------------------------------------------------------------------------
CXMLEnvFile& CXMLEnvFile::operator =(const CXMLEnvFile& aXMLEnvFile)
{
	if (this == &aXMLEnvFile)
		return *this;

	m_eFileType			=	aXMLEnvFile.m_eFileType;		
	m_strUrlData		=	aXMLEnvFile.m_strUrlData;	
	m_strEnvClass		=	aXMLEnvFile.m_strEnvClass;			
	m_strDocumentName	=	aXMLEnvFile.m_strDocumentName;
	m_nDocumentNumb		= 	aXMLEnvFile.m_nDocumentNumb;	

	return *this;
}

//----------------------------------------------------------------
//class CXMLEnvContentsArray
//----------------------------------------------------------------
//
CXMLEnvFile::ContentFileType CXMLEnvContentsArray::GetFileTypeAt(int nIdx) const
{
	if (
			nIdx < 0		 || 
			nIdx > GetSize() ||
			!GetAt(nIdx)	 
		)
	{
		ASSERT(FALSE);
		return CXMLEnvFile::UNDEF_FILE;
	}

	return GetAt(nIdx)->m_eFileType;
}

//----------------------------------------------------------------
CString	CXMLEnvContentsArray::GetUrlDataAt(int nIdx) const
{
	if (
			nIdx < 0		 || 
			nIdx > GetSize() ||
			!GetAt(nIdx)	 
		)
	{
		ASSERT(FALSE);
		return _T("");
	}

	return GetAt(nIdx)->m_strUrlData;
}

//----------------------------------------------------------------
CString	CXMLEnvContentsArray::GetDocumentNameAt(int nIdx) const
{
	if (
			nIdx < 0		 || 
			nIdx > GetSize() ||
			!GetAt(nIdx)	 
		) 
	{
		ASSERT(FALSE);
		return _T("");
	}

	return GetAt(nIdx)->m_strDocumentName;
}

//----------------------------------------------------------------
CXMLEnvFile* CXMLEnvContentsArray::GetEnvFileByName(LPCTSTR lpszFileName) const
{
	CString strFileName = GetNameWithExtension(lpszFileName);
	for (int i = 0; i <= GetUpperBound(); i++)
		if (GetUrlDataAt(i).CompareNoCase(strFileName)	== 0)
			return GetAt(i);
	
	return NULL;
}	

//----------------------------------------------------------------------------------------------
void CXMLEnvContentsArray::IncrementExpRecordCount(LPCTSTR lpszFileName,  int nDataInstancesNumb)
{
	CXMLEnvFile* pElem = GetEnvFileByName(lpszFileName);

	if (pElem)
		pElem->m_nDocumentNumb = nDataInstancesNumb;
}

//----------------------------------------------------------------------------------------------
BOOL CXMLEnvContentsArray::Parse(CXMLNode* pNode)
{
	if (!pNode)	
		return FALSE;
	
	CXMLNode* pInfoNode;
	CXMLEnvFile* pEnvFile;
	
	RemoveAll ();

	// itero sui figli del tag CONTENTS
	for (int i = 0; i < pNode->GetChildsNum(); i++)
	{
		pInfoNode = pNode->GetChildAt(i);
		if (pInfoNode) 
		{
			pEnvFile = new CXMLEnvFile;
			if (pEnvFile->Parse(pInfoNode))
				Add(pEnvFile);
			else
			{
				delete pEnvFile;
				pEnvFile = NULL;
			}
		}
	}
	return TRUE;
}

//----------------------------------------------------------------------------------------------
BOOL CXMLEnvContentsArray::Unparse(CXMLNode* pNode)
{
	CXMLNode* pEnvNode;

	for (int i = 0; i <= GetUpperBound(); i++)
	{		
		if (GetAt(i))
		{
			pEnvNode = pNode->CreateNewChild(ENV_XML_FILE_TAG);
			GetAt(i)->Unparse(pEnvNode);
		}
	}

	return TRUE;
}

//----------------------------------------------------------------
//class CXMLEnvDocumentInfo
//----------------------------------------------------------------
CXMLEnvDocumentInfo::CXMLEnvDocumentInfo()
{
	m_DataTime.SetFullDate();
}

//----------------------------------------------------------------
CXMLEnvDocumentInfo::CXMLEnvDocumentInfo(const CXMLEnvDocumentInfo& aEnvDocInfo)
{
	*this = aEnvDocInfo;
}

//----------------------------------------------------------------
void CXMLEnvDocumentInfo::SetCurrentDataTime()
{
	m_DataTime.SetTodayDateTime(); //chiama il ::GetLocalTime()
}

//----------------------------------------------------------------------------------------------
BOOL CXMLEnvDocumentInfo::Parse(CXMLNode* pNode)
{
	if (!pNode)	
		return FALSE;

	CXMLNode* pInfoNode;

	CString strTagValue;

	pInfoNode = pNode->GetChildByName(ENV_XML_DOMAIN_TAG);
	if (pInfoNode)
		pInfoNode->GetText(m_strDomainName);

	pInfoNode = pNode->GetChildByName(ENV_XML_SITE_TAG);
	if (pInfoNode)
		pInfoNode->GetText(m_strSiteName);
	
	pInfoNode = pNode->GetChildByName(ENV_XML_SITE_CODE_TAG);
	if (pInfoNode)
		pInfoNode->GetText(m_strSiteCode);
	
	pInfoNode = pNode->GetChildByName(ENV_XML_USER_TAG);
	if (pInfoNode)
		pInfoNode->GetText(m_strUserName);


	pInfoNode = pNode->GetChildByName(ENV_XML_DATATIME_TAG);
	if (pInfoNode)
	{
		pInfoNode->GetText(strTagValue);
		m_DataTime.AssignFromXMLString(strTagValue);
	}
	
	pInfoNode = pNode->GetChildByName(ENV_XML_ENVCLASS_TAG);
	if (pInfoNode)
		pInfoNode->GetText(m_strEnvClass);

	pInfoNode = pNode->GetChildByName(ENV_XML_ROOT_NS_TAG);
	if (pInfoNode)
	{
		CString strNameSpace;
		pInfoNode->GetText(strNameSpace);
		m_nsRootDoc = strNameSpace;
		VERIFY(m_nsRootDoc.IsValid());
	}


	return TRUE;
}	

//----------------------------------------------------------------------------------------------
BOOL CXMLEnvDocumentInfo::Unparse(CXMLNode* pNode)
{
	CXMLNode* pInfoNode = pNode->CreateNewChild(ENV_XML_DOMAIN_TAG);
	if (pInfoNode)
		pInfoNode->SetText(m_strDomainName);

	pInfoNode = pNode->CreateNewChild(ENV_XML_SITE_TAG);
	if (pInfoNode)
		pInfoNode->SetText(m_strSiteName);

	pInfoNode = pNode->CreateNewChild(ENV_XML_SITE_CODE_TAG);
	if (pInfoNode)
		pInfoNode->SetText(m_strSiteCode);

	pInfoNode = pNode->CreateNewChild(ENV_XML_USER_TAG);
	if (pInfoNode)
		pInfoNode->SetText(m_strUserName);	

	pInfoNode = pNode->CreateNewChild(ENV_XML_ENVCLASS_TAG);
	if (pInfoNode)
		pInfoNode->SetText(m_strEnvClass);

	pInfoNode = pNode->CreateNewChild(ENV_XML_ROOT_NS_TAG);
	if (pInfoNode)
		pInfoNode->SetText(m_nsRootDoc.ToString());

	pInfoNode = pNode->CreateNewChild(ENV_XML_DATATIME_TAG);
	if (pInfoNode)
		pInfoNode->SetText(m_DataTime.FormatDataForXML());

	return TRUE;
}

//----------------------------------------------------------------------------------------------
CXMLEnvDocumentInfo& CXMLEnvDocumentInfo::operator =(const CXMLEnvDocumentInfo& aXMLDocInfo)
{
	if (this == &aXMLDocInfo)
		return *this;

	m_strSiteName		=	aXMLDocInfo.m_strSiteName;	
	m_strSiteCode		=	aXMLDocInfo.m_strSiteCode;
	m_strUserName		=	aXMLDocInfo.m_strUserName;	
	m_strDomainName		=	aXMLDocInfo.m_strDomainName;
	m_DataTime			= 	aXMLDocInfo.m_DataTime;	

	return *this;
}

//	controllo se è già stato inserito il primo rootfile. 
//	quelli successivi devono avere come tipo NEXT_ROOT_FILE
//--------------------------------------------------------------------------------------------
int CXMLEnvContentsArray::Add(CXMLEnvFile* pEnvFile)
{ 
	if (pEnvFile->IsRootFile() && !m_bRootFilePresent)
		m_bRootFilePresent = TRUE;

	return Array::Add (pEnvFile); 
}

//--------------------------------------------------------------------------------------------
//	CXMLParserEnvelope implementation
//----------------------------------------------------------------------------------------------
//
//----------------------------------------------------------------------------------------------
int	CXMLEnvelopeInfo::AddEnvFile
				(
					CXMLEnvFile::ContentFileType	eFileType, 
					LPCTSTR							lpszUrlDati, 
					LPCTSTR							lpszProfile /*= NULL*/, 
					LPCTSTR							lpszEnvClass/*= NULL*/,
					LPCTSTR							lpszDocName	/*= NULL*/, 
					int								nDocNum		/*= 0*/					
				) 
{ 
	if (!GetEnvFileByName(lpszUrlDati))
		return m_aEnvContents.Add(new CXMLEnvFile(eFileType, lpszUrlDati, lpszProfile, lpszEnvClass, lpszDocName, nDocNum)); 

	return -1;
}

//----------------------------------------------------------------------------------------------
BOOL CXMLEnvelopeInfo::Parse(const CString& strEnvName, CXMLDocumentObject* pXMLDoc /*= NULL*/)
{
	if (strEnvName.IsEmpty())
		return FALSE;

	BOOL bLocal = FALSE;
	CXMLDocumentObject* pXMLDocument = pXMLDoc;
	if (!pXMLDocument)
	{
		pXMLDocument = new CXMLDocumentObject(FALSE,FALSE);
		bLocal = TRUE;
	}

	if (!pXMLDocument->LoadXMLFile(strEnvName))
	{
		if (bLocal)
			delete pXMLDocument;
		return FALSE;
	}


	CXMLNode* pRoot = pXMLDocument->GetRoot();

	if (!pRoot) return FALSE;

	CXMLNode* pInfoNode;

	pInfoNode = pRoot->GetChildByName(ENV_XML_EXPORT_ID_TAG);
	if (pInfoNode)
		pInfoNode->GetText(m_strExportID);

	pInfoNode = pRoot->GetChildByName(ENV_XML_DESCRIPTION_TAG);
	if (pInfoNode)
		pInfoNode->GetText(m_strDescription);

	pInfoNode = pRoot->GetChildByName(ENV_XML_DOC_INFO_TAG);
	if (pInfoNode)
		m_aEnvDocInfo.Parse(pInfoNode);
		
	pInfoNode = pRoot->GetChildByName(ENV_XML_CONTENTS_TAG);
	if (pInfoNode)
		m_aEnvContents.Parse(pInfoNode);		
	
	if (bLocal)
		delete pXMLDocument;

	return TRUE;
}	

//----------------------------------------------------------------------------------------------
BOOL CXMLEnvelopeInfo::Unparse(const CString& strEnvName, BOOL bDisplayMsgBox /*= TRUE*/, BOOL bCreateSchema /*= FALSE*/)
{
	if (strEnvName.IsEmpty())
		return FALSE;

	CXMLDocumentObject aXMLDoc(FALSE, bDisplayMsgBox);	
	AfxInitWithXEngineEncoding(aXMLDoc);

	if (bCreateSchema)
	{
		CString strSchemaSource = MakeFilePath(GetXMLSchemasPath(), MakeSchemaName(strEnvName));
		CString strSchemaTarget = MakeFilePath(GetPath(strEnvName), MakeSchemaName(strEnvName));

		//se esiste il file di schema, lo copio nella directory dell'envelope
		if (!::ExistFile(strSchemaSource))
			CreateEnvelopeSchema(strSchemaSource, bDisplayMsgBox);
		
		::CopyFile (strSchemaSource, strSchemaTarget, bDisplayMsgBox);
	}
	
	aXMLDoc.SetValidateOnParse(TRUE);
	aXMLDoc.SetNameSpaceURI(XTECH_NAMESPACE);

	CXMLNode* pRoot = aXMLDoc.CreateRoot(ENV_XML_ENVELOPE_ID_TAG);

	CXMLNode* pNewNode = NULL;
	CXMLNode* pLeafNode = NULL;
	if (pRoot)
	{
		// inserisco le info proprie dell'envelope
		pLeafNode = pRoot->CreateNewChild(ENV_XML_EXPORT_ID_TAG);
		if (pLeafNode)
			pLeafNode->SetText(m_strExportID);
	
		pLeafNode = pRoot->CreateNewChild(ENV_XML_DESCRIPTION_TAG);
		if (pLeafNode)
			pLeafNode->SetText(m_strDescription);
			
		// inserisco le info relative all'esportazione
		pNewNode = aXMLDoc.CreateRootChild(ENV_XML_DOC_INFO_TAG);
		if (pNewNode)
			m_aEnvDocInfo.Unparse(pNewNode);

		// inserisco le info relative al contexts
		pNewNode = aXMLDoc.CreateRootChild(ENV_XML_CONTENTS_TAG);
		if (pNewNode)
			m_aEnvContents.Unparse(pNewNode);
	}

	aXMLDoc.SaveXMLFile(strEnvName, TRUE);

	return TRUE;
}

//----------------------------------------------------------------------------------------------
BOOL CXMLEnvelopeInfo::CreateEnvelopeSchema(const CString& strSchemaName, BOOL bDisplayMsgBox)
{
	CXSDGenerator XMLSchema(XTECH_NAMESPACE, bDisplayMsgBox);
	XMLSchema.InsertElement(ENV_XML_ENVELOPE_ID_TAG, ENV_XML_ENVELOPE_ID_TAG);
	
	XMLSchema.BeginComplexType(ENV_XML_ENVELOPE_ID_TAG);
		XMLSchema.InsertElement		(ENV_XML_EXPORT_ID_TAG,		SCHEMA_XSD_DATATYPE_STRING_VALUE);
		XMLSchema.InsertElement		(ENV_XML_DESCRIPTION_TAG,	SCHEMA_XSD_DATATYPE_STRING_VALUE);
		XMLSchema.InsertElement		(ENV_XML_DOC_INFO_TAG,		ENV_XML_DOC_INFO_TAG, _T("1"), _T("1"));
		XMLSchema.InsertElement		(ENV_XML_CONTENTS_TAG,		ENV_XML_CONTENTS_TAG, _T("1"), _T("1"));
	XMLSchema.EndComplexType();

	XMLSchema.BeginComplexType(ENV_XML_DOC_INFO_TAG);
		XMLSchema.InsertElement		(ENV_XML_DOMAIN_TAG,		SCHEMA_XSD_DATATYPE_STRING_VALUE, _T("1"), _T("1"));
		XMLSchema.InsertElement		(ENV_XML_SITE_TAG,			SCHEMA_XSD_DATATYPE_STRING_VALUE, _T("1"), _T("1"));
		XMLSchema.InsertElement		(ENV_XML_SITE_CODE_TAG,		SCHEMA_XSD_DATATYPE_STRING_VALUE, _T("1"), _T("1"));
		XMLSchema.InsertElement		(ENV_XML_USER_TAG,			SCHEMA_XSD_DATATYPE_STRING_VALUE, _T("1"), _T("1"));
		XMLSchema.InsertElement		(ENV_XML_ENVCLASS_TAG,		SCHEMA_XSD_DATATYPE_STRING_VALUE, _T("1"), _T("1"));
		XMLSchema.InsertElement		(ENV_XML_ROOT_NS_TAG,		SCHEMA_XSD_DATATYPE_STRING_VALUE, _T("1"), _T("1"));
		XMLSchema.InsertElement		(ENV_XML_DATATIME_TAG,		SCHEMA_XSD_DATATYPE_STRING_VALUE, _T("1"), _T("1"));
	XMLSchema.EndComplexType();

	XMLSchema.BeginComplexType(ENV_XML_CONTENTS_TAG);
		XMLSchema.InsertElement		(ENV_XML_FILE_TAG,			ENV_XML_FILE_TAG,	_T("1"), SCHEMA_XSD_UNBOUNDED_VALUE);
	XMLSchema.EndComplexType();

	XMLSchema.BeginComplexType(ENV_XML_FILE_TAG);
		XMLSchema.InsertAttribute	(ENV_XML_TYPE_ATTRIBUTE,			SCHEMA_XSD_DATATYPE_STRING_VALUE, SCHEMA_XSD_REQUIRED_VALUE);
		XMLSchema.InsertAttribute	(ENV_XML_DATA_URL_ATTRIBUTE,		SCHEMA_XSD_DATATYPE_STRING_VALUE, SCHEMA_XSD_REQUIRED_VALUE);
		XMLSchema.InsertAttribute	(ENV_XML_ENVCLASS_ATTRIBUTE,		SCHEMA_XSD_DATATYPE_STRING_VALUE, SCHEMA_XSD_OPTIONAL_VALUE);
		XMLSchema.InsertAttribute	(ENV_XML_DOC_NAME_ATTRIBUTE,		SCHEMA_XSD_DATATYPE_STRING_VALUE, SCHEMA_XSD_OPTIONAL_VALUE);
		XMLSchema.InsertAttribute	(ENV_XML_DOC_NUMB_ATTRIBUTE,		SCHEMA_XSD_DATATYPE_INT_VALUE, SCHEMA_XSD_OPTIONAL_VALUE);		
	XMLSchema.EndComplexType();

	return XMLSchema.SaveXMLFile(strSchemaName, TRUE);
}

//----------------------------------------------------------------------------------------------
CXMLEnvelopeInfo& CXMLEnvelopeInfo::operator =(const CXMLEnvelopeInfo& aXMLEnvInfo)
{
	if (this == &aXMLEnvInfo)
		return *this;
	
	m_aEnvDocInfo = aXMLEnvInfo.m_aEnvDocInfo;
	m_aEnvContents.RemoveAll();

	for (int i =0; i <= aXMLEnvInfo.m_aEnvContents.GetUpperBound(); i++)
		m_aEnvContents.Add(new CXMLEnvFile(*aXMLEnvInfo.m_aEnvContents.GetAt(i)));

	return *this;
}
