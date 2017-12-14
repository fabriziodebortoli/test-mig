#include "stdafx.h"

#include "NumberToLiteral.h"

//includere come ultimo include all'inizio del cpp
//#include "begincpp.dex"

#ifdef _DEBUG
#undef THIS_FILE
static const char THIS_FILE[] = __FILE__;
#endif
//============================================================================

/////////////////////////////////////////////////////////////////////////////
//						CNumberToLiteralLookUpTable
/////////////////////////////////////////////////////////////////////////////
//
//----------------------------------------------------------------------------
CNumberToLiteralLookUpTable::CNumberToLiteralLookUpTable(
					long		lValue,
					CString		sDescription
				)
	:
	m_Value			(lValue),
	m_Description	(sDescription),
	m_ForThousands	(sDescription),
	m_ForMillions	(sDescription)
{
}

/////////////////////////////////////////////////////////////////////////////
//						CNumberToLiteralLookUpTableList
/////////////////////////////////////////////////////////////////////////////
//
//----------------------------------------------------------------------------
// GetElementValue
//    restituisce il valore in lettere dell'elemento aValue
//    se non esiste stringa vuota
//
CString CNumberToLiteralLookUpTableList::GetElementDescription(long aValue, long aIndex)
{
	for (int i=0; i <= GetUpperBound(); i++)
		if (GetAt(i) && GetAt(i)->m_Value == aValue)
		{
			switch (aIndex)
			{
				case 6:
					return GetAt(i)->m_ForMillions;
				case 9:
				case 0:
					return GetAt(i)->m_ForThousands;
				default:
					return GetAt(i)->m_Description;
			}
		}

	return _T("");
}

//----------------------------------------------------------------------------
// ElementExist
//    indica se esiste l'elemento aValue
//
BOOL CNumberToLiteralLookUpTableList::ElementExist(long aValue)
{
	for (int i=0; i <= GetUpperBound(); i++)
		if (GetAt(i) && GetAt(i)->m_Value == aValue)
			return TRUE;

	return FALSE;
}
/////////////////////////////////////////////////////////////////////////////
//						CSeparatorException
/////////////////////////////////////////////////////////////////////////////
//
//----------------------------------------------------------------------------
CSeparatorException::CSeparatorException(int aValue)
	:
	m_Value			(aValue)
{
}

/////////////////////////////////////////////////////////////////////////////
//						CDeclinationException
/////////////////////////////////////////////////////////////////////////////
//
//----------------------------------------------------------------------------
CDeclinationException::CDeclinationException(CString aKind, int aValue)
	:
	m_Value			(aValue),
	m_Kind			(aKind)
{
}

//----------------------------------------------------------------------------
bool CDeclinationException::IsException(int aValue)
{
	DataInt nValue;
	nValue.Assign(aValue);
	CString aVal = nValue.FormatData();

	DataInt nmValue;
	nmValue.Assign(m_Value);
	CString amVal = nmValue.FormatData();

	int nValLen = amVal.GetLength();

	if (m_Kind == _T("S"))
	{
		if (aVal.Left(nValLen) == amVal)
			return true;
	}
	else if (m_Kind == _T("E"))
	{
		if (aVal.Right(nValLen) == amVal)
			return true;
	}
	else
	{
		if (aVal == amVal)
			return true;
	}

	return false;
}

/////////////////////////////////////////////////////////////////////////////
//						CDeclination
/////////////////////////////////////////////////////////////////////////////
//
//----------------------------------------------------------------------------
CDeclination::CDeclination(int aValue, CString aDescription)
	:
	m_Value			(aValue),
	m_Description	(aDescription)
{
	m_pDeclinationExceptionList = new(CDeclinationExceptionList);
}

//----------------------------------------------------------------------------
CDeclination::~CDeclination()
{
	SAFE_DELETE(m_pDeclinationExceptionList);
}

//----------------------------------------------------------------------------
void CDeclination::AddDeclinationException(CString aKind, int aValue)
{
	m_pDeclinationExceptionList->Add(new CDeclinationException(aKind, aValue));
}

//----------------------------------------------------------------------------
bool CDeclination::IsException(int aValue)
{
	if (m_pDeclinationExceptionList->GetUpperBound() >= 0)
		for (int idx = 0; idx <= m_pDeclinationExceptionList->GetUpperBound(); idx++)
		{
			if (m_pDeclinationExceptionList->GetAt(idx)->IsException(aValue))
				return true;
		}
	return false;
}

/////////////////////////////////////////////////////////////////////////////
//						CNumberGroup
/////////////////////////////////////////////////////////////////////////////
//
//----------------------------------------------------------------------------
CNumberGroup::CNumberGroup(CString aValue, CString aUseJunction)
	:
	m_Value				(aValue),
	m_bUseJunction		(aUseJunction)
{
	m_pDeclinationList = new(CDeclinationList);
}

//----------------------------------------------------------------------------
CNumberGroup::~CNumberGroup()
{
	SAFE_DELETE(m_pDeclinationList);
}

//----------------------------------------------------------------------------
void CNumberGroup::AddDeclination(int aValue, CString aDescription)
{
	m_pDeclinationList->Add(new CDeclination(aValue, aDescription));
}

//----------------------------------------------------------------------------
CString CNumberGroup::GetDescription(int nValue, int lastDigit) const
{
	if (m_pDeclinationList && m_pDeclinationList->GetUpperBound() >= 0)
		for (int idx = 0; idx <= m_pDeclinationList->GetUpperBound(); idx++)
		{
			if (m_pDeclinationList->GetAt(idx)->m_Value == lastDigit)
			{
				if (!m_pDeclinationList->GetAt(idx)->IsException(nValue))
					return m_pDeclinationList->GetAt(idx)->m_Description;
			}
		}
	return m_Value;
}

//----------------------------------------------------------------------------
void CNumberGroup::AddDeclinationException(int decValue, CString kind, int val)
{
	if (m_pDeclinationList->GetUpperBound() >= 0)
		for (int idx = 0; idx <= m_pDeclinationList->GetUpperBound(); idx++)
		{
			if (m_pDeclinationList->GetAt(idx)->m_Value == decValue)
			{
				m_pDeclinationList->GetAt(idx)->AddDeclinationException(kind, val);
			}
		}
}

/////////////////////////////////////////////////////////////////////////////
//						CNumberToLiteralLookUpTableManager
/////////////////////////////////////////////////////////////////////////////
//

IMPLEMENT_DYNCREATE(CNumberToLiteralLookUpTableManager, CObject);

//----------------------------------------------------------------------------
CNumberToLiteralLookUpTableManager::CNumberToLiteralLookUpTableManager()
:
	m_bUnitInversion	(FALSE),
	m_bDecimalLiteral	(FALSE),
	m_pLookUpList	(NULL),
	m_pExceptions	(NULL),
	m_pHundreds		(NULL),
	m_pThousands	(NULL),
	m_pMillions		(NULL),
	m_pMilliards	(NULL),
	m_strUniversalSeparator (_T("")),
	m_strCurrencySingular(_T("")),
	m_strCurrencyPlural(_T("")),
	m_strCentesimalSingular(_T("")),
	m_strCentesimalPlural(_T(""))
{
	m_pLookUpList = new(CNumberToLiteralLookUpTableList);
	m_pExceptions = new(CSeparatorExceptionList);
}

//----------------------------------------------------------------------------
CNumberToLiteralLookUpTableManager::~CNumberToLiteralLookUpTableManager()
{
	SAFE_DELETE(m_pHundreds);
	SAFE_DELETE(m_pThousands);
	SAFE_DELETE(m_pMillions);
	SAFE_DELETE(m_pMilliards);
	SAFE_DELETE(m_pLookUpList);
	SAFE_DELETE(m_pExceptions);
}

//----------------------------------------------------------------------------
void CNumberToLiteralLookUpTableManager::Add(CNumberToLiteralLookUpTable* pLU)
{
	if (!m_pLookUpList->ElementExist(pLU->m_Value))
		m_pLookUpList->Add(pLU);
}

//----------------------------------------------------------------------------
void CNumberToLiteralLookUpTableManager::AddSeparatorException(int aValue)
{
	m_pExceptions->Add(new CSeparatorException(aValue));
}

//----------------------------------------------------------------------------
void CNumberToLiteralLookUpTableManager::AddDeclination (DeclinationType eDecType, int aValue, CString aDescription)
{
	switch(eDecType)
	{
		case CNumberToLiteralLookUpTableManager::Hundreds:
			m_pHundreds->AddDeclination(aValue, aDescription);
			break;
		case CNumberToLiteralLookUpTableManager::Thousands:
			m_pThousands->AddDeclination(aValue, aDescription);
			break;
		case CNumberToLiteralLookUpTableManager::Millions:
			m_pMillions->AddDeclination(aValue, aDescription);
			break;
		case CNumberToLiteralLookUpTableManager::Milliards:
			m_pMilliards->AddDeclination(aValue, aDescription);
			break;
	}
}

//----------------------------------------------------------------------------
void CNumberToLiteralLookUpTableManager::AddDeclinationException(DeclinationType eDecType, int aDecValue, CString aKind, int aExcValue)
{
	switch(eDecType)
	{
		case CNumberToLiteralLookUpTableManager::Hundreds:
			m_pHundreds->AddDeclinationException(aDecValue, aKind, aExcValue);
			break;
		case CNumberToLiteralLookUpTableManager::Thousands:
			m_pThousands->AddDeclinationException(aDecValue, aKind, aExcValue);
			break;
		case CNumberToLiteralLookUpTableManager::Millions:
			m_pMillions->AddDeclinationException(aDecValue, aKind, aExcValue);
			break;
		case CNumberToLiteralLookUpTableManager::Milliards:
			m_pMilliards->AddDeclinationException(aDecValue, aKind, aExcValue);
			break;
	}
}

//----------------------------------------------------------------------------
void CNumberToLiteralLookUpTableManager::AddNumberGroup (DeclinationType eDecType, CString aValue, CString aUseJunction)
{
	switch(eDecType)
	{		
		case CNumberToLiteralLookUpTableManager::Hundreds:
			m_pHundreds = new CNumberGroup(aValue, aUseJunction);
		break;
			case CNumberToLiteralLookUpTableManager::Thousands:
			m_pThousands = new CNumberGroup(aValue, aUseJunction);
		break;
			case CNumberToLiteralLookUpTableManager::Millions:
			m_pMillions = new CNumberGroup(aValue, aUseJunction);
		break;
			case CNumberToLiteralLookUpTableManager::Milliards:
			m_pMilliards = new CNumberGroup(aValue, aUseJunction);
		break;
	}
}

//----------------------------------------------------------------------------
void CNumberToLiteralLookUpTableManager::Clear()
{
	SAFE_DELETE(m_pHundreds);
	SAFE_DELETE(m_pThousands);
	SAFE_DELETE(m_pMillions);
	SAFE_DELETE(m_pMilliards);
	m_Junction.Empty();
	m_Culture.Empty();
	m_Separator.Empty();
	m_strCentesimalSingular.Empty();
	m_strCentesimalPlural.Empty();
	m_strCurrencySingular.Empty();
	m_strCurrencyPlural.Empty();
	m_strUniversalSeparator.Empty();
	m_bUnitInversion = FALSE;
	m_bDecimalLiteral = FALSE;

	m_pLookUpList->RemoveAll();
	m_pExceptions->RemoveAll();
}
