#include "stdafx.h"

#include <io.h>
#include <TbNameSolver\FileSystemFunctions.h>
#include <TBNameSolver\ApplicationContext.h>

#include "DataFileInfo.h"

//includere come ultimo include all'inizio del cpp
#include "begincpp.dex"

#ifdef _DEBUG
#undef THIS_FILE
static const char THIS_FILE[] = __FILE__;
#endif
//============================================================================

/////////////////////////////////////////////////////////////////////////////
//						CDataFileElementField
/////////////////////////////////////////////////////////////////////////////
//
//----------------------------------------------------------------------------
CDataFileElementField::CDataFileElementField(const CString& sName, DataObj* pValue)
	:
	m_sName		(sName),
	m_pValue	(pValue)
{
}

//----------------------------------------------------------------------------
CDataFileElementField::~CDataFileElementField	()
{
	delete m_pValue;
}

/////////////////////////////////////////////////////////////////////////////
//						CDataFileElementFieldType
/////////////////////////////////////////////////////////////////////////////
//
//----------------------------------------------------------------------------
CDataFileElementFieldType::CDataFileElementFieldType(CString sName,	DataType Type, DataBool bHidden, DataBool bKey)
	:
	m_sName		(sName),
	m_Type		(Type),
	m_bHidden	(bHidden),
	m_bKey		(bKey)
{}

	
//----------------------------------------------------------------------------
CDataFileElementFieldType::CDataFileElementFieldType(const CDataFileElementFieldType& aElementFieldType)
{
	m_sName	= aElementFieldType.m_sName;
	m_Type	= aElementFieldType.m_Type;
}

/////////////////////////////////////////////////////////////////////////////
//						CDataFileElementType
/////////////////////////////////////////////////////////////////////////////
//
//----------------------------------------------------------------------------
CDataFileElementType::CDataFileElementType()
	:
	m_nKey(-1)
{
}

//----------------------------------------------------------------------------
// GetElementType
//    restituisce il tipo del field sName
//    se non esiste restituisce DataType::String
//
DataType CDataFileElementType::GetElementType(CString sName)
{
	for (int i=0; i <= GetUpperBound(); i++)
		if (GetAt(i) && GetAt(i)->m_sName.CompareNoCase(sName) == 0)
			return GetAt(i)->m_Type;

	ASSERT_TRACE1(FALSE,"Field not found: %s", sName);
	return DataType::String;
}

//----------------------------------------------------------------------------
// GetElement
//    restituisce la descrizione del field sName
//    se non esiste restituisce NULL
//
CDataFileElementFieldType* CDataFileElementType::GetElement(CString sName)
{
	for (int i=0; i <= GetUpperBound(); i++)
		if (GetAt(i) && GetAt(i)->m_sName.CompareNoCase(sName) == 0)
			return GetAt(i);

	TRACE("CDataFileElementType::GetElement: not found field %s\n", sName);
	return NULL;
}

//----------------------------------------------------------------------------
// GetElementPos
//    restituisce l'indice dell'elemento sName
//    se non esiste restituisce -1
//
int CDataFileElementType::GetElementPos(CString sName)
{
	for (int i=0; i <= GetUpperBound(); i++)
		if (GetAt(i) && GetAt(i)->m_sName.CollateNoCase(sName) == 0)
			return i;

	TRACE("CDataFileElementType::GetElementPos:not found field %s\n", sName);
	return -1;
}

/////////////////////////////////////////////////////////////////////////////
//						CDataFileElement
/////////////////////////////////////////////////////////////////////////////
//
//----------------------------------------------------------------------------
// GetElementValue
//    restituisce il valore dell'elemento sName
//    se non esiste NULL
//
DataObj* CDataFileElement::GetElementValue(const CString&  sName)
{
	for (int i=0; i <= GetUpperBound(); i++)
		if (GetAt(i) && GetAt(i)->m_sName.CompareNoCase(sName) == 0)
			return GetAt(i)->m_pValue;

	TRACE("CDataFileElement::GetElementValue: not found field %s\n", sName);
	return NULL;
}

//----------------------------------------------------------------------------
// ElementExist
//    indica se esiste l'elemento sName
//
BOOL CDataFileElement::ElementExist(const CString&  sName)
{
	for (int i=0; i <= GetUpperBound(); i++)
		if (GetAt(i) && GetAt(i)->m_sName.CompareNoCase(sName) == 0)
			return TRUE;

	return FALSE;
}

/////////////////////////////////////////////////////////////////////////////
//						CDataFileElements
/////////////////////////////////////////////////////////////////////////////
//

//----------------------------------------------------------------------------
CDataFileElement* CDataFileElements::GetElement(const CString& strKeyName, const CString& strKeyValue)
{
	for (int i=0; i <= GetUpperBound(); i++)
		if (GetAt(i)->GetElementValue(strKeyName)->Str().CompareNoCase(strKeyValue) == 0)
			return GetAt(i);	

	return NULL;
}


//----------------------------------------------------------------------------
void CDataFileElements::RemoveElement(const CString& strKeyName, const CString& strKeyValue)
{
	for (int i=0; i <= GetUpperBound(); i++)
		if (GetAt(i)->GetElementValue(strKeyName)->Str().CompareNoCase(strKeyValue) == 0)
		{
			if (GetAt(i)->m_bFromCustom)
				RemoveAt(i);	
		}
}

/////////////////////////////////////////////////////////////////////////////
//						CDataFileInfo
/////////////////////////////////////////////////////////////////////////////
//
CDataFileInfo::CDataFileInfo() 
: 
	m_bInvalid(FALSE),
	m_bAllowChanges(FALSE), 
	m_bUseProductLanguage(FALSE), 
	m_bEnableAddElements(FALSE)
{
}

//----------------------------------------------------------------------------
CDataFileInfo::CDataFileInfo (const CTBNamespace& ns, BOOL bAllowChanges /*= FALSE*/,  BOOL bUseProductLanguage /*= FALSE*/) 
: 
	m_Namespace(ns), 
	m_bUseProductLanguage(bUseProductLanguage), 
	m_bFilterLike(FALSE),
	m_bEnableAddElements(FALSE),
	m_bAllowChanges(bAllowChanges),
	m_bInvalid(FALSE)
{
	m_strFileName = m_Namespace.GetObjectName();
	if (::GetExtension(m_strFileName).IsEmpty())
		m_strFileName += szXmlExt;
}


//----------------------------------------------------------------------------
CString CDataFileInfo::GetDescription(int nElement)
{
	CString sDescription = _T("");
	for (int i=0; i <= m_arElementTypes.GetUpperBound(); i++)
		if (!m_arElementTypes[i]->m_bHidden)
		{
			DataObj* po = m_arElements[nElement]->GetElementValue(m_arElementTypes[i]->m_sName);
			sDescription += (po->Str() + _T(" "));
		}

	return sDescription;
}


// GetValue
//    restituisce valore di ritorno della combo
//----------------------------------------------------------------------------
CString CDataFileInfo::GetValue(int nElement)
{
	CString sKeyName = m_arElementTypes.GetKeyName();
	return m_arElements[nElement]->GetElementValue(sKeyName)->FormatData();
}


// GetElement
//    restituisce il puntatore al valore di sName con chiave pKeyValue
//    se non esite ritorna NULL
//
//----------------------------------------------------------------------------
DataObj* CDataFileInfo::GetElement(CString pKeyValue, CString sName)
{
	// recupero il nome della chiave
	CString sKeyName = m_arElementTypes.GetKeyName();

	for (int i=0; i <= m_arElements.GetUpperBound(); i++)
		if (m_arElements[i]) //non si sà mai
		{
			DataObj* pValue = m_arElements[i]->GetElementValue(sKeyName);
			if (pValue)
			{
				CString sKeyValue = pValue->Str();
				if (sKeyValue.CompareNoCase(pKeyValue) == 0)
					return m_arElements[i]->GetElementValue(sName);
			}
		}

	return NULL;
}


//----------------------------------------------------------------------------
// ChangeKey
//    modifica il campo chiave con sName
//
void CDataFileInfo::ChangeKey(CString sName)
{
	if (!m_bAllowChanges)
	{
		ASSERT_TRACE(m_bAllowChanges,"Changes are denied!");
		return;
	}

	CDataFileElementFieldType* pElementType = m_arElementTypes.GetElement(sName);

	// controllo che l'elemento richesto esista
	if (!pElementType)
	{
		ASSERT_TRACE1(FALSE,"Field not found: %s", sName);
		return;
	}

	// controllo che il campo che si vuole settare come chiave sia una stringa
	if (m_arElementTypes.GetElementType(sName) != DataType::String)
	{
		ASSERT_TRACE1(FALSE,"The field %s is not a string", sName);
		return;
	}

	// controllo non ci siano elementi che non lo contengono
	for (int i=0; i <= m_arElements.GetUpperBound(); i++)
		if (!m_arElements[i]->ElementExist(sName))
		{
			ASSERT_TRACE1(FALSE,"The field %s isn't in all the elements", sName);
			return;
		}
	
	// tolgo la vecchia chiave
	m_arElementTypes[m_arElementTypes.GetKey()]->m_bKey = FALSE;

	// setto la nuova chiave
	pElementType->m_bKey = TRUE;
	m_arElementTypes.SetKey(m_arElementTypes.GetElementPos(sName));
}

//----------------------------------------------------------------------------
// SetHidden
//    nasconde/visualizza il campo sName
//
void CDataFileInfo::SetHidden(CString sNomeCampo, BOOL bValue /*= TRUE*/)
{
	if (!m_bAllowChanges)
	{
		ASSERT_TRACE1(m_bAllowChanges,"Changes are denied for field %s",sNomeCampo);
		return;
	}

	CDataFileElementFieldType* pElementType = m_arElementTypes.GetElement(sNomeCampo);

	// controllo che l'elemento richesto esista
	if (!pElementType)
	{
		ASSERT_TRACE1(FALSE,"Field not found: %s", sNomeCampo);
		return;
	}
	
	BOOL bOk;
	if (bValue)
	{
		bOk = FALSE;
		// se stò nascondendo il campo controllo che non sia l'unico campo visibile
		for (int i=0; i <= m_arElementTypes.GetUpperBound() && !bOk; i++)
			if (!m_arElementTypes[i]->m_bHidden && m_arElementTypes[i]->m_sName != sNomeCampo)
				bOk = TRUE;
	}
	else
	{
		bOk = TRUE;
		// se stò visualizzando il campo controllo non ci siano elementi che non lo contengono
		for (int i=0; i <= m_arElements.GetUpperBound() && bOk; i++)
			if (!m_arElements[i]->ElementExist(sNomeCampo))
				bOk = FALSE;
	}

	if (bOk)
		pElementType->m_bHidden = bValue;
	else
	{
		if (bValue)
			ASSERT_TRACE1(FALSE,"The field %s is the only visible one", sNomeCampo) // no ; needed here due to macro expansion!
		else
			ASSERT_TRACE1(FALSE,"The field %s isn't in all the elements", sNomeCampo);
	}	
}

//----------------------------------------------------------------------------
// SetType
//    modifico il tipo del campo sName
//
void CDataFileInfo::SetType(CString sNomeCampo, DataType tType)
{
	if (!m_bAllowChanges)
	{
		ASSERT_TRACE1(m_bAllowChanges,"Changes are denied for field %s", sNomeCampo);
		return;
	}

	// non posso modificare il tipo di un campo chiave
	if (m_arElementTypes.GetKeyName().CompareNoCase(sNomeCampo) == 0)
	{
		ASSERT_TRACE1(FALSE,"The field %s isn't a key", sNomeCampo);
		return;
	}
	
	CDataFileElementFieldType* pElementType = m_arElementTypes.GetElement(sNomeCampo);

	// controllo che l'elemento richesto esista
	if (!pElementType)
	{
		ASSERT_TRACE1(FALSE,"Field not found: %s", sNomeCampo);
		return;
	}

	pElementType->m_Type = tType;
}

//restituisce l'elemento il cui valore della chiave è passato come argomento
//----------------------------------------------------------------------------
CDataFileElement* CDataFileInfo::GetDataFileElement(const CString& strKeyValue)
{
	return m_arElements.GetElement(m_arElementTypes.GetKeyName(), strKeyValue);
}

//l'utente può aggiungere elementi al file xml (mediante file custom) solo se abbiamo un solo ElementType (che è anche chiave)
//----------------------------------------------------------------------------
void  CDataFileInfo::AddElement(const CString& strValue)
{
	DataObj* dataValue = NULL;
	if (strValue.IsEmpty())
		return;
	
	DataType tTipo = m_arElementTypes.GetKeyType();
	dataValue = DataObj::DataObjCreate(tTipo);
	dataValue->Assign(strValue);

	// aggiungo l'elemento  al CDataFileInfo
	CDataFileElement* pElement = new CDataFileElement();
	pElement->m_bFromCustom = TRUE;
	pElement->Add(new CDataFileElementField(GetKeyName(), dataValue));
	m_arElements.Add(pElement);
}

//----------------------------------------------------------------------------
void CDataFileInfo::RemoveElement(const CString& strValue)
{
	m_arElements.RemoveElement(m_arElementTypes.GetKeyName(), strValue);
}





/////////////////////////////////////////////////////////////////////////////
//						CDataFilesManager
/////////////////////////////////////////////////////////////////////////////
//
CDataFilesManager::~CDataFilesManager()
{
}

//----------------------------------------------------------------------------
// GetDataFile
//    restituisce il puntatore alla CDataFileInfo indicata da pszDataFileNamespace
//    se non esiste restituisce NULL - controlli e messaggistica sono demandati al chiamante
//
CDataFileInfo* CDataFilesManager::GetDataFile(LPCTSTR pszDataFileNamespace)
{
	TB_LOCK_FOR_READ();
	for (int i=0; i <= m_arDataFiles.GetUpperBound(); i++)
	{
		CDataFileInfo* pFileInfo = ((CDataFileInfo*)m_arDataFiles[i]);
		if (pFileInfo->m_Namespace.ToString().CompareNoCase(pszDataFileNamespace) == 0)
		{
			// se il DataFileInfo esiste e non è cambiato è buono
			if (!pFileInfo->m_bAllowChanges)
				return (CDataFileInfo*) m_arDataFiles[i];			
		}
	}
	return NULL;
}

/////////////////////////////////////////////////////////////////////////////
//						AfxGetDataFilesManager
/////////////////////////////////////////////////////////////////////////////
//
//----------------------------------------------------------------------------
CDataFilesManager* AfxGetDataFilesManager() { return AfxGetApplicationContext()->GetObject<CDataFilesManager>(&CApplicationContext::GetDataFilesManager); }