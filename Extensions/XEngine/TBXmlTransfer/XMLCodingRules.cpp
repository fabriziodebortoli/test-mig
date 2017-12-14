
#include "stdafx.h" 

#include <TBGES\DBT.H>
#include <TBGES\EXTDOC.H>

#include <TBGENERIC\GLOBALS.H>

#include "XMLTransferTags.h" 
#include "XMLCodingRules.h"
#include "GenFunc.h"

#ifdef _DEBUG
#define new DEBUG_NEW
#undef THIS_FILE
static const char THIS_FILE[] = __FILE__;
#endif

static const TCHAR szParam1				[] = _T("P1");
static const TCHAR szParam2				[] = _T("P2");
static const TCHAR szParam3				[] = _T("P3");

static const TCHAR szOpenSquareBrace		[] = _T("[");
static const TCHAR szCloseSquareBrace		[] = _T("]");
static const TCHAR szAttributeSimbol		[] = _T("@");
static const TCHAR szEqualSimbol			[] = _T("=");
static const TCHAR szApex					[] = _T("'");

/////////////////////////////////////////////////////////////////////////////
//	SqlRecord					### TXMLKeyExtension ###					
/////////////////////////////////////////////////////////////////////////////
//
/////////////////////////////////////////////////////////////////////////////


//-----------------------------------------------------------------------------
IMPLEMENT_DYNCREATE(TXMLKeyExtension, SqlRecord) 

//-----------------------------------------------------------------------------
TXMLKeyExtension::TXMLKeyExtension(BOOL bCallInit)
	:
	SqlRecord 				(GetStaticName())
{
    
	BindRecord();
	if (bCallInit) Init();
}

//-----------------------------------------------------------------------------
void TXMLKeyExtension::BindRecord()
{
	BEGIN_BIND_DATA	();
		BIND_DATA	(_T("KeyFieldName"),		f_FieldName);
		BIND_DATA	(_T("KeyTableName"),		f_TableName);
		BIND_DATA	(_T("KeyExtension"),		f_Extension);
		BIND_DATA	(_T("DocumentNamespace"),	f_DocumentNamespace);
	END_BIND_DATA();    
}

//-----------------------------------------------------------------------------
LPCTSTR TXMLKeyExtension::GetStaticName() { return _T("XE_KeyExtension"); }


//////////////////////////////////////////////////////////////////////////////
//             class CCodingManager implementation
//////////////////////////////////////////////////////////////////////////////
//=============================================================================


IMPLEMENT_DYNCREATE(CCodeManager, CObject);

//=============================================================================
CCodeManager::CCodeManager(CAbstractFormDoc* pDoc /*=NULL*/, CString strCodeExtension /*=""*/)
:
m_pDocument(pDoc),
m_pOldDataObj(NULL),
m_strCodeExtension(strCodeExtension)
{
	m_pRecord	= (pDoc && pDoc->m_pDBTMaster) ? pDoc->m_pDBTMaster->GetRecord () : NULL;
	m_pOriginalRecord = m_pRecord ? m_pRecord->Create () : NULL;
	m_pDOM		= new CXMLDocumentObject(FALSE,FALSE, FALSE);
}

//=============================================================================
CCodeManager::~CCodeManager()
{
	SAFE_DELETE(m_pDOM);
	SAFE_DELETE(m_pOldDataObj);
	SAFE_DELETE(m_pOriginalRecord);
}

//=============================================================================
BOOL CCodeManager::GetNewKey()
{
	if (!m_pDocument || !m_pRecord || !m_pDOM)
	{
		ASSERT(FALSE);
		return FALSE;
	}

	CPathFinder::PosType posType = CPathFinder::STANDARD;

	CString strFile = AfxGetPathFinder()->GetDocumentCodingRulesFullName(m_pDocument->GetNamespace(), posType);
	if (!ExistFile (strFile))
		return FALSE;
	
	if (!m_pDOM->LoadXMLFile (strFile))
	{
		ASSERT(FALSE);
		return FALSE;
	}
	
	for (int i = 0; i <= m_pRecord->GetUpperBound(); i++)
	{
		if (m_pRecord->IsSpecial(i))
		{
			SAFE_DELETE(m_pOldDataObj);
			DataObj* pDataObj = m_pRecord->GetAt(i)->GetDataObj();
			m_pOldDataObj = pDataObj->DataObjClone();
			m_StrCurrentField = m_pRecord->GetColumnName(i);
			if (ApplyRules(pDataObj, m_StrCurrentField))
				return TRUE;
		}
	}

	return FALSE;
}

//=============================================================================
BOOL CCodeManager::GetNewFields(BOOL bOverwriteLockedFields /*= FALSE*/)
{
	if (!m_pDocument || !m_pRecord || !m_pDOM)
	{
		ASSERT(FALSE);
		return FALSE;
	}

	CPathFinder::PosType posType = CPathFinder::STANDARD;

	CString strFile = AfxGetPathFinder()->GetDocumentCodingRulesFullName(m_pDocument->GetNamespace(), posType);
	if (!ExistFile (strFile))
		return FALSE;
	
	if (!m_pDOM->LoadXMLFile (strFile))
	{
		ASSERT(FALSE);
		return FALSE;
	}
	
	BOOL bResult = FALSE;
	for (int i = 0; i <= m_pRecord->GetUpperBound(); i++)
	{
		if (!m_pRecord->IsSpecial (i))
		{
			SAFE_DELETE(m_pOldDataObj);
			DataObj* pDataObj = m_pRecord->GetAt(i)->GetDataObj();
			BOOL bIsValueLocked = pDataObj->IsValueLocked();
			if (bOverwriteLockedFields)
				pDataObj->SetValueLocked (FALSE);

			m_pOldDataObj = pDataObj->DataObjClone();
			m_StrCurrentField = m_pRecord->GetColumnName (i);
			if (ApplyRules(pDataObj, m_StrCurrentField))
				bResult =  TRUE;
			
			pDataObj->SetValueLocked (bIsValueLocked);
		}
	}

	return bResult;
}

//=============================================================================
BOOL CCodeManager::ApplyRules(DataObj* pDataObj, const CString& strColumnName)
{
	if (!pDataObj)
	{
		ASSERT(FALSE);
		return FALSE;
	}

	CXMLNode *pNode = m_pDOM->GetRoot();
	if (!pNode)
	{
		ASSERT(FALSE);
		return FALSE;
	}
	
	pNode = pNode->GetChildByAttributeValue (DOC_XML_CODING_TABLE_TAG, DOC_XML_NAME_ATTRIBUTE, m_pRecord->GetTableName (), FALSE);
	if (!pNode)
		return FALSE;

	pNode = pNode->GetChildByAttributeValue (XML_FIELD_TAG, DOC_XML_NAME_ATTRIBUTE, strColumnName, FALSE);
	if (!pNode)
		return FALSE;

	BOOL bOk = FALSE;
	for (int i=0; i<pNode->GetChildsNum (); i++)
	{
		if (ApplyRule(pDataObj, pNode->GetChildAt (i)))
		{
			bOk = TRUE;
			break;
		}
	}

	return bOk;
}

//=============================================================================
BOOL CCodeManager::ApplyRule(DataObj* pDataObj, CXMLNode *pRuleNode)
{
	if (!pDataObj || !pRuleNode)
	{
		ASSERT(FALSE);
		return FALSE;
	}

	// prima di turro ripristino il valore originario
	pDataObj->Assign (*m_pOldDataObj );

	CString strType;
	if (!pRuleNode->GetAttribute (XML_TYPE_ATTRIBUTE, strType))
	{
		ASSERT(FALSE);
		return FALSE;
	}

	// in base alla regola trovata, chiamo la funzione appropriata
	
	// PRIMA REGOLA: LASCIO INALTERATO IL CODICE
	if (!strType.CompareNoCase (DOC_XML_CODING_SAME_CODE))
		return ApplySameCodeRule(pDataObj);


	CString strAttribute;
	BOOL bBefore = FALSE;
	if (pRuleNode->GetAttribute (DOC_XML_CODING_INSERT_ATTRIBUTE, strAttribute)) 
		bBefore = !strAttribute.CompareNoCase (DOC_XML_CODING_BEFORE_VALUE);

	int nLength;
	if (pRuleNode->GetAttribute (DOC_XML_CODING_LENGTH_ATTRIBUTE, strAttribute)) 
	{
		nLength = _ttoi((const TCHAR*)strAttribute);
		if (nLength<MIN_EXTENSION_LENGTH) 
			nLength = MIN_EXTENSION_LENGTH;
		else if (nLength>MAX_EXTENSION_LENGTH) 
			nLength = MAX_EXTENSION_LENGTH;
	}
	else
		nLength = 0;

	// SECONDA REGOLA: APPLICO UNA ESTENSIONE FISSA O PRIMA O DOPO IL CODICE ORIGINARIO
	if (!strType.CompareNoCase (DOC_XML_CODING_CODE_EXTENSION))
		return ApplyCodeExtensionRule(pDataObj, bBefore, nLength);

	// TERZA REGOLA: APPLICO UNA ESTENSIONE NUMERICA O PRIMA O DOPO IL CODICE ORIGINARIO
	if (!strType.CompareNoCase (DOC_XML_CODING_NUMBER_PADDING))
		return ApplyNumberPaddingRule(pDataObj, bBefore, nLength);

	return FALSE;
}

//=============================================================================
BOOL CCodeManager::ApplySameCodeRule(DataObj* pDataObj)
{
	pDataObj->Assign (*m_pOldDataObj);
	return CheckRecord();
}

//=============================================================================
BOOL CCodeManager::ApplyCodeExtensionRule(DataObj* pDataObj, BOOL bBefore, int nLength, CString strExtension /*=""*/)
{
	if (strExtension.IsEmpty ())
		strExtension = m_strCodeExtension;

	if (strExtension.IsEmpty () || !pDataObj->IsKindOf (RUNTIME_CLASS(DataStr)))
	{
		ASSERT(FALSE);
		return FALSE;
	}

	const SqlColumnInfo *pInfo = m_pRecord->GetColumnInfo (pDataObj);
	
	CString strOriginalCode = ((DataStr*) pDataObj)->GetString ();
	
	// tronco se l'estensione è troppo lunga
	if (nLength!=0 && nLength<strExtension.GetLength ())
	{
		strExtension = strExtension.Left(nLength);
	}
	else 
	{
		// integro se è troppo corta
		while(strExtension.GetLength()< nLength)
			strExtension = (bBefore ? _T("") : PADDING_CHARACTER) + 
							strExtension + 
							(bBefore ? PADDING_CHARACTER : _T(""));
	}

	int dataLength = ((DataStr*) pDataObj)->GetLen ();
	int extensionLength = strExtension.GetLength();

	int availableSpace = pInfo->GetColumnLength ();
	int neededSpace = dataLength + extensionLength;

	CString strNewCode;

	if (neededSpace>availableSpace)
	{
		// devo troncare l'estensione ed azzerare il codice originario per fare spazio
		if (extensionLength>availableSpace)
		{
			strOriginalCode.Empty ();
			strExtension = strExtension.Left (availableSpace);
		}
		else
		// devo ridurre il codice originario per fare spazio
		{
			strOriginalCode = strOriginalCode.Left (availableSpace-extensionLength);
		}
	}
	
	strNewCode =	(bBefore ? strExtension : _T("")) +
					strOriginalCode +
					(bBefore ? _T("") : strExtension);

	pDataObj->Assign (strNewCode);
	return CheckRecord();
}

//=============================================================================
BOOL CCodeManager::ApplyNumberPaddingRule(DataObj* pDataObj, BOOL bBefore, int nLength)
{
	if (!pDataObj->IsKindOf (RUNTIME_CLASS(DataStr)))
	{
		ASSERT(FALSE);
		return FALSE;
	}

	TXMLKeyExtension aRec;
	SqlTable aTable(&aRec, m_pDocument->GetUpdatableSqlSession());
	TRY
	{
		aTable.Open (TRUE);
		aTable.SelectAll ();

		aTable.AddFilterColumn (aRec.f_TableName);
		aTable.AddFilterColumn (aRec.f_FieldName);
		aTable.AddFilterColumn (aRec.f_DocumentNamespace);
		
		aTable.AddParam (szParam1, aRec.f_TableName);
		aTable.SetParamValue (szParam1, DataStr(m_pRecord->GetTableName()));

		aTable.AddParam (szParam2, aRec.f_FieldName);
		aTable.SetParamValue (szParam2, DataStr(m_StrCurrentField));

		aTable.AddParam (szParam3, aRec.f_DocumentNamespace);
		aTable.SetParamValue (szParam3, DataStr(m_pDocument->GetNamespace().ToString()));

		aTable.Query ();

		if (aTable.IsEmpty ())
		{
			aTable.AddNew ();
			aRec.f_FieldName = m_StrCurrentField;
			aRec.f_TableName = m_pRecord->GetTableName();
			aRec.f_DocumentNamespace = m_pDocument->GetNamespace().ToString();
			aRec.f_Extension = 1;
			aTable.Update ();
		}
		else
		{
			aTable.Edit();
			aRec.f_Extension ++;
			if (aRec.f_Extension <=0)
			{
				ASSERT(FALSE);			//nella tabella c'era rumenta!!!
				aRec.f_Extension = 1;	
			}
			aTable.Update ();
		}
		
		aTable.Close();
	}
	CATCH(CException, e)
	{
		if (aTable.IsOpen ()) aTable.Close ();
		ASSERT(FALSE);
		return FALSE;
	}
	END_CATCH
	
	CString strExtension, strFormat;
	strFormat.Format(_T("%%.%dd"), nLength);
	strExtension.Format (strFormat, (int)aRec.f_Extension);

	return ApplyCodeExtensionRule(pDataObj, bBefore, nLength, strExtension);
}

//=============================================================================
BOOL CCodeManager::CheckRecord()
{
	if (!m_pOriginalRecord)
	{
		ASSERT(FALSE);
		return FALSE;
	}
	
	*m_pOriginalRecord = *m_pRecord;
	BOOL bFound = m_pDocument->m_pDBTMaster->FindData(FALSE);
	*m_pRecord = *m_pOriginalRecord;
	
	return !bFound;
}
