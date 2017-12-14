
#include "stdafx.h"

#include <io.h>

#include <TbXmlCore\XMLTags.h>
#include <TbXmlCore\XMLDocObj.h>
#include <TbXmlCore\XMLParser.h>

#include <TbNameSolver\Diagnostic.h>
#include <TbNameSolver\FileSystemFunctions.h>
#include <TbWebServicesWrappers\LoginManagerInterface.h>
#include <TbClientCore\ClientObjects.h>

#include <TbGeneric\OutDateObjectsInfo.h>
#include <TBGeneric\GeneralFunctions.h>
#include <TBGeneric\NumberToLiteral.h>

#include "XmlNumberToLiteralParser.h"

//includere come ultimo include all'inizio del cpp
#include "begincpp.dex"

#ifdef _DEBUG
#undef THIS_FILE
static const char THIS_FILE[] = __FILE__;
#endif

////////////////////////////////////////////////////////////////////////////////

static const TCHAR szXmlLookUp[]			= _T("LookUp");
static const TCHAR szXmlNameEntryes[]		= _T("NameEntryes");
static const TCHAR szXmlNameEntry[]		= _T("NameEntry");
static const TCHAR szForThousands[]		= _T("ForThousands");
static const TCHAR szForMillions[]		= _T("ForMillions");
static const TCHAR szXmlGroups[]			= _T("Groups");
static const TCHAR szDeclination[]		= _T("Declination");
static const TCHAR szXmlsHundreds[]		= _T("sHundreds");
static const TCHAR szXmlsThousands[]		= _T("sThousands");
static const TCHAR szXmlsMillions[]		= _T("sMillions");
static const TCHAR szXmlsMilliards[]		= _T("sMilliards");
static const TCHAR szXmlParameters[]		= _T("Parameters");
static const TCHAR szXmlUnitInvertion[]	= _T("UnitInvertion");
static const TCHAR szXmlUniversalSeparator[] = _T("UniversalSeparator");
static const TCHAR szXmlDecimalLiteral[] = _T("DecimalLiteral");
static const TCHAR szXmlCurrencySingular[] = _T("CurrencySingular");
static const TCHAR szXmlCurrencyPlural[] = _T("CurrencyPlural");
static const TCHAR szXmlCentesimalSingular[] = _T("CentesimalSingular");
static const TCHAR szXmlCentesimalPlural[] = _T("CentesimalPlural");
static const TCHAR szXmlJunction[]		= _T("Junction");
static const TCHAR szXmlSeparator[]		= _T("Separator");
static const TCHAR szXmlExeptions[]		= _T("Exeptions");
static const TCHAR szXmlException[]		= _T("Exception");
static const TCHAR szXmlDigits[]			= _T("Digits");
static const TCHAR szXmlAttrValInt[]		= _T("ValInt");
static const TCHAR szXmlAttrValStr[]		= _T("ValStr");
static const TCHAR szXmlAttrValue[]		= _T("value");
static const TCHAR szXmlAttrKind[]		= _T("kind");
static const TCHAR szXmlAttrDigit[]		= _T("digit");
static const TCHAR szXmlAttrUseJunction[]	 = _T("useJunction");
//============================================================================

IMPLEMENT_DYNAMIC(CXmlNumberToLiteralParser, CObject);

//----------------------------------------------------------------------------
// LoadLookUpFile
//   Carica il file relativo alla localizzazione attuale
//     Chiama la CheckLookUpFile
//     Chiama la NameEntryes
//     Chiama la LoadGroups
//     Chiama la LoadParameters
//
BOOL CXmlNumberToLiteralParser::LoadLookUpFile ()
{
	m_pLUM = AfxGetLoginContext()->GetObject<CNumberToLiteralLookUpTableManager>(&CLoginContext::GetNTLLookUpTableManager);
	
	if (!m_pLUM)
	{
		ASSERT(FALSE);
		return FALSE;
	}
	
	CString strCulture = AfxGetCulture();
	if (strCulture.IsEmpty()) 
		strCulture = _T("en");
	if (m_pLUM->m_Culture.CompareNoCase(strCulture) == 0)
		return TRUE;

	CTBNamespace aNs(_T("Module.Framework.TbGenlib"));
	CString strFile = AfxGetPathFinder()->GetNumberToLiteralXmlFullName(aNs, strCulture);
	
	//se non esiste il file della lingua specificata, ricado sull'inglese!
	if (!ExistFile(strFile))
		strCulture = _T("en");
	if (m_pLUM->m_Culture.CompareNoCase(strCulture) == 0)
		return TRUE;
	
	strFile = AfxGetPathFinder()->GetNumberToLiteralXmlFullName(aNs, strCulture);
	m_pLUM->Clear();
	m_pLUM->m_Culture = strCulture;

	CLocalizableXMLDocument aDocument(aNs, AfxGetPathFinder());

	// Controllo che il file xml che stò parsando sia valido
	if (!CheckLookUpFile(strFile, aDocument))
		return FALSE;

	// Carico le cifre e le eccezioni, i gruppi (centinaia, migliaia, ecc.) ed eventuali parametri dal file xml comprendendo un controllo sulla validità dei campi
	return NameEntryes(aDocument) && LoadGroups(aDocument) && LoadParameters(aDocument);
}
//----------------------------------------------------------------------------
// CheckLookUpFile
//   Controlla il file identificato da aNs
//
BOOL CXmlNumberToLiteralParser::CheckLookUpFile (CString strFile, CLocalizableXMLDocument& aDocument)
{
	// Controllo l'esistenza dei files
	if (!ExistFile(strFile))
	{
		TRACE(cwsprintf(_TB("The file {0-%s} is missing"), strFile));
		return FALSE;
	}

	// Controllo se l'xml ha un formato valido
	if (!aDocument.LoadXMLFile(strFile))
	{
		AfxGetDiagnostic()->Add (cwsprintf(_TB("Format error in xml file: {0-%s}"), strFile));
		return FALSE;
	}

	// Controllo se l'xml ha un tag radice "LookUp"
	CXMLNode* pRoot = aDocument.GetRoot();
	CString sRootName;
	if (!pRoot || !pRoot->GetName(sRootName) || sRootName.CompareNoCase (szXmlLookUp) != 0) 
	{
		AfxGetDiagnostic()->Add (cwsprintf(_TB("Missing root element in file: {0-%s}"), strFile));
		return FALSE;
	}

	return TRUE;
}
//----------------------------------------------------------------------------
// NameEntryes
//   Carica e controlla i numeri principali e le eccezioni contenute nel tag <NameEntryes>
//     Chiama la ParseTagNameEntry
//
BOOL CXmlNumberToLiteralParser::NameEntryes(CLocalizableXMLDocument& aDocument)
{
	BOOL bOk = TRUE;

	// Mi leggo la lista di NameEntry
	CXMLNodeChildsList* pNameEntryNodes = aDocument.SelectNodes (_T("/") + CString(szXmlLookUp) + _T("/") + szXmlNameEntryes + _T("/") + szXmlNameEntry);

	// Se non ci sono NameEntry non ne carico ma non ritorno errori
	if (!pNameEntryNodes || !pNameEntryNodes->GetSize())
		return TRUE;

	// anche se un TAG è errato non mi fermo a caricare il file
	// così fornisco una diagnostica completa di tutto
	for (int i=0; i <= pNameEntryNodes->GetUpperBound(); i++)
		if (pNameEntryNodes->GetAt(i) && !ParseTagNameEntry(pNameEntryNodes->GetAt(i)))
			bOk = FALSE;

	delete pNameEntryNodes;

	return bOk;
}

//-----------------------------------------------------------------------------
// ParseTagNameEntry
//   Carica e controlla la definizione delle cifre e delle eccezioni dal file xml contenuti nei tag <NameEntry>
//
BOOL CXmlNumberToLiteralParser::ParseTagNameEntry(CXMLNode* pTagNode)
{
	// leggo l'attributo ValInt del TAG
	CString	strTagValInt;
	if (!pTagNode->GetAttribute(szXmlAttrValInt, strTagValInt) || strTagValInt.IsEmpty())
	{
		AfxGetDiagnostic()->Add (_T("Elemento <Tag> privo di attributo ValInt! "));
		return FALSE;
	}

	// leggo l'attributo ValStr del TAG
	CString	strTagValStr;
	if (!pTagNode->GetAttribute(szXmlAttrValStr, strTagValStr) || strTagValStr.IsEmpty())
	{
		AfxGetDiagnostic()->Add (_T("Elemento <Tag> privo di attributo ValStr! "));
		return FALSE;
	}

	DataLng dL;
	dL.Assign(strTagValInt);
	long l = (long)dL;

	// creo l'elemento
	CNumberToLiteralLookUpTable* p_LU = new CNumberToLiteralLookUpTable(l, strTagValStr);

	ParseTagForThousands(pTagNode->SelectSingleNode(CString(szForThousands)), p_LU);
	
	ParseTagForMillions(pTagNode->SelectSingleNode(CString(szForMillions)), p_LU);

	// aggiungo l'elemento fieldtype al CNumberToLiteralLookUpTableManager
	m_pLUM->Add(p_LU);

	return TRUE;
}

//-----------------------------------------------------------------------------
BOOL CXmlNumberToLiteralParser::ParseTagForThousands
						(
							CXMLNode*						pTagNode,
							CNumberToLiteralLookUpTable*	p_LU
						)
{
	if (!pTagNode)
		return TRUE;

	CString	strTagValue;
	if (!pTagNode->GetAttribute(szXmlAttrValue, strTagValue) || strTagValue.IsEmpty())
	{
		AfxGetDiagnostic()->Add (_T("Elemento <ForThousands> privo di attributo value! "));
		return FALSE;
	}

	p_LU->m_ForThousands = strTagValue;

	return TRUE;
}

//-----------------------------------------------------------------------------
BOOL CXmlNumberToLiteralParser::ParseTagForMillions
						(
							CXMLNode*						pTagNode,
							CNumberToLiteralLookUpTable*	p_LU
						)
{
	if (!pTagNode)
		return TRUE;

	CString	strTagValue;
	if (!pTagNode->GetAttribute(szXmlAttrValue, strTagValue) || strTagValue.IsEmpty())
	{
		AfxGetDiagnostic()->Add (_T("Elemento <ForMillions> privo di attributo value! "));
		return FALSE;
	}

	p_LU->m_ForMillions = strTagValue;

	return TRUE;
}

//-----------------------------------------------------------------------------
BOOL CXmlNumberToLiteralParser::LoadGroups(CLocalizableXMLDocument& aDocument)
{
	CString	strTagUseJunction = _T("");

	CXMLNode* pMainNode = aDocument.SelectSingleNode (_T("/") + CString(szXmlLookUp) + _T("/") + CString(szXmlGroups));
	if (!pMainNode)
	{
		AfxGetDiagnostic()->Add (_TB("Loading numbers-letters conversion rules: missing element Gropus."));
		return FALSE;
	}

	// Leggo le centinaia
	CXMLNode* pNode = pMainNode->SelectSingleNode (szXmlsHundreds);
	if (!pNode)
	{
		AfxGetDiagnostic()->Add (_TB("Loading numbers-letters conversion rules: missing hundred decoding"));
		delete pMainNode;
		return FALSE;
	}
	
	// leggo l'attributo value del TAG
	CString	strTagValue;
	if (!pNode->GetAttribute(szXmlAttrValue, strTagValue) || strTagValue.IsEmpty())
	{
		AfxGetDiagnostic()->Add (_TB("Loading numbers-letters conversion rules: missing hundred decoding"));
		delete pMainNode;
		delete pNode;
		return FALSE;
	}

	// leggo l'attributo useJunction del TAG
	strTagUseJunction = _T("");;
	pNode->GetAttribute(szXmlAttrUseJunction, strTagUseJunction) || strTagUseJunction.IsEmpty();

	m_pLUM->AddNumberGroup(CNumberToLiteralLookUpTableManager::Hundreds, strTagValue, strTagUseJunction);

	LoadDeclinations(CNumberToLiteralLookUpTableManager::Hundreds, pNode);
	
	// Leggo le migliaia
	delete pNode;
	pNode = pMainNode->SelectSingleNode (szXmlsThousands);
	if (!pNode)
	{
		AfxGetDiagnostic()->Add (_TB("Loading numbers-letters conversion rules: missing thousand decoding"));
		delete pMainNode;
		return FALSE;
	}

	// leggo l'attributo value del TAG
	if (!pNode->GetAttribute(szXmlAttrValue, strTagValue) || strTagValue.IsEmpty())
	{
		AfxGetDiagnostic()->Add (_TB("Loading numbers-letters conversion rules: missing thousand decoding"));
		delete pMainNode;
		delete pNode;
		return FALSE;
	}

	// leggo l'attributo useJunction del TAG
	strTagUseJunction = _T("");;
	pNode->GetAttribute(szXmlAttrUseJunction, strTagUseJunction) || strTagUseJunction.IsEmpty();

	m_pLUM->AddNumberGroup(CNumberToLiteralLookUpTableManager::Thousands, strTagValue, strTagUseJunction);

	LoadDeclinations(CNumberToLiteralLookUpTableManager::Thousands, pNode);

	// Leggo i milioni
	delete pNode;
	pNode = pMainNode->SelectSingleNode (szXmlsMillions);
	if (!pNode)
	{
		AfxGetDiagnostic()->Add (_TB("Loading numbers-letters conversion rules: missing millions decoding"));
		delete pMainNode;
		return FALSE;
	}

	// leggo l'attributo value del TAG
	if (!pNode->GetAttribute(szXmlAttrValue, strTagValue) || strTagValue.IsEmpty())
	{
		AfxGetDiagnostic()->Add (_TB("Loading numbers-letters conversion rules: missing millions decoding"));
		delete pMainNode;
		delete pNode;
		return FALSE;
	}

	// leggo l'attributo useJunction del TAG
	strTagUseJunction = _T("");;
	pNode->GetAttribute(szXmlAttrUseJunction, strTagUseJunction) || strTagUseJunction.IsEmpty();

	m_pLUM->AddNumberGroup(CNumberToLiteralLookUpTableManager::Millions, strTagValue, strTagUseJunction);

	LoadDeclinations(CNumberToLiteralLookUpTableManager::Millions, pNode);

	// Leggo i miliardi
	delete pNode;
	pNode = pMainNode->SelectSingleNode (szXmlsMilliards);
	if (!pNode)
	{
		AfxGetDiagnostic()->Add (_TB("Loading numbers-letters conversion rules: missing thousand millions decoding"));
		delete pMainNode;
		return FALSE;
	}

	// leggo l'attributo value del TAG
	if (!pNode->GetAttribute(szXmlAttrValue, strTagValue) || strTagValue.IsEmpty())
	{
		AfxGetDiagnostic()->Add (_TB("Loading numbers-letters conversion rules: missing thousand millions decoding"));
		delete pMainNode;
		delete pNode;
		return FALSE;
	}

	// leggo l'attributo useJunction del TAG
	strTagUseJunction = _T("");;
	pNode->GetAttribute(szXmlAttrUseJunction, strTagUseJunction) || strTagUseJunction.IsEmpty();

	m_pLUM->AddNumberGroup(CNumberToLiteralLookUpTableManager::Milliards, strTagValue, strTagUseJunction);

	LoadDeclinations(CNumberToLiteralLookUpTableManager::Milliards, pNode);

	delete pNode;
	delete pMainNode;

	return TRUE;
}

//-----------------------------------------------------------------------------
BOOL CXmlNumberToLiteralParser::LoadDeclinations(CNumberToLiteralLookUpTableManager::DeclinationType eDecType, CXMLNode* aNode)
{
	CString	strTagValue;
	CString	strDigitValue;

	// Mi leggo la lista di Declinations
	CXMLNodeChildsList* pNodeDeclination = aNode->SelectNodes (CString(szDeclination));

	// Se non ci sono Declinations non ne carico ma non ritorno errori
	if (!pNodeDeclination || !pNodeDeclination->GetSize())
	{
		SAFE_DELETE(pNodeDeclination);
		return TRUE;
	}

	for (int i=0; i <= pNodeDeclination->GetUpperBound(); i++)
		if (pNodeDeclination->GetAt(i))
			if (pNodeDeclination->GetAt(i)->GetAttribute(szXmlAttrValue, strTagValue) && 
				!strTagValue.IsEmpty() &&
				pNodeDeclination->GetAt(i)->GetAttribute(szXmlAttrDigit, strDigitValue) && 
				!strDigitValue.IsEmpty())
			{
				DataInt dI;
				dI.Assign(strDigitValue);
				int nVal = (int)dI;

				m_pLUM->AddDeclination(eDecType, nVal, strTagValue);

				CXMLNode* aNodeDeclination = pNodeDeclination->GetAt(i);

				LoadDeclinationExceptions(eDecType, nVal, aNodeDeclination);
			}

	delete pNodeDeclination;

	return TRUE;
}

//-----------------------------------------------------------------------------
BOOL CXmlNumberToLiteralParser::LoadDeclinationExceptions(CNumberToLiteralLookUpTableManager::DeclinationType eDecType, int nDecValue, CXMLNode* aNode)
{
	CString	strTagKindValue;
	CString	strDigitValue;

	// Mi leggo la lista di Exception
	CXMLNodeChildsList* pNodeException = aNode->SelectNodes (CString(szXmlException));

	// Se non ci sono Exception non ne carico ma non ritorno errori
	if (!pNodeException || !pNodeException->GetSize())
	{
		SAFE_DELETE(pNodeException);
		return TRUE;
	}

	for (int i=0; i <= pNodeException->GetUpperBound(); i++)
		if (pNodeException->GetAt(i))
			if (pNodeException->GetAt(i)->GetAttribute(szXmlAttrKind, strTagKindValue) && 
				!strTagKindValue.IsEmpty() &&
				pNodeException->GetAt(i)->GetAttribute(szXmlAttrValue, strDigitValue) && 
				!strDigitValue.IsEmpty())
			{
				DataInt dI;
				dI.Assign(strDigitValue);
				int nVal = (int)dI;

				m_pLUM->AddDeclinationException(eDecType, nDecValue, strTagKindValue, nVal);
			}

	delete pNodeException;

	return TRUE;
}

//-----------------------------------------------------------------------------
BOOL CXmlNumberToLiteralParser::LoadParameters(CLocalizableXMLDocument& aDocument)
{
	BOOL bOk = TRUE;
	CString	strTagValue;

	CXMLNode* pMainNode = aDocument.SelectSingleNode (_T("/") + CString(szXmlLookUp) + _T("/") + CString(szXmlParameters));
	if (!pMainNode)
		return TRUE;
	
	// Leggo la UnitInversion
	CXMLNode* pNode = pMainNode->SelectSingleNode (szXmlUnitInvertion);
	if (pNode)
		if (pNode->GetAttribute(szXmlAttrValue, strTagValue) && !strTagValue.IsEmpty())
			if (strTagValue == _T("true"))
				m_pLUM->m_bUnitInversion = TRUE;
	
	//Leggo il separatore universale per dividere decine da unita, centinaia da decine(usato dal Brasile)
	delete pNode;
	pNode = pMainNode->SelectSingleNode (szXmlUniversalSeparator);
	if (pNode)
		if (pNode->GetAttribute(szXmlAttrValue, strTagValue) && !strTagValue.IsEmpty())
			m_pLUM->m_strUniversalSeparator = strTagValue;

	//Attributo che determina se anche la parte decimale va convertita in lettere (usato dal Brasile)
	delete pNode;
	pNode = pMainNode->SelectSingleNode (szXmlDecimalLiteral);
	if (pNode)
		if (pNode->GetAttribute(szXmlAttrValue, strTagValue) && !strTagValue.IsEmpty())
			if (strTagValue == _T("true"))
				m_pLUM->m_bDecimalLiteral = TRUE;


	//Attributo che qualifica i valori centesimali singolari
	delete pNode;
	pNode = pMainNode->SelectSingleNode (szXmlCentesimalSingular);
	if (pNode)
		if (pNode->GetAttribute(szXmlAttrValue, strTagValue) && !strTagValue.IsEmpty())
				m_pLUM->m_strCentesimalSingular = strTagValue;

	//Attributo che qualifica i valori centesimali plurali
	delete pNode;
	pNode = pMainNode->SelectSingleNode (szXmlCentesimalPlural);
	if (pNode)
		if (pNode->GetAttribute(szXmlAttrValue, strTagValue) && !strTagValue.IsEmpty())
				m_pLUM->m_strCentesimalPlural = strTagValue;

	//Attributo che definisce la currency per valori singolari
	delete pNode;
	pNode = pMainNode->SelectSingleNode (szXmlCurrencySingular);
	if (pNode)
		if (pNode->GetAttribute(szXmlAttrValue, strTagValue) && !strTagValue.IsEmpty())
				m_pLUM->m_strCurrencySingular = strTagValue;

	//Attributo che definisce la currency per valori plurali
	delete pNode;
	pNode = pMainNode->SelectSingleNode (szXmlCurrencyPlural);
	if (pNode)
		if (pNode->GetAttribute(szXmlAttrValue, strTagValue) && !strTagValue.IsEmpty())
				m_pLUM->m_strCurrencyPlural = strTagValue;

	// Leggo la Junction
	delete pNode;
	pNode = pMainNode->SelectSingleNode (szXmlJunction);
	if (pNode)
		if (pNode->GetAttribute(szXmlAttrValue, strTagValue) && !strTagValue.IsEmpty())
			m_pLUM->m_Junction = strTagValue;

	// Leggo il Separatore
	delete pNode;
	pNode = pMainNode->SelectSingleNode (szXmlSeparator);
	if (pNode)
		if (pNode->GetAttribute(szXmlAttrValue, strTagValue) && !strTagValue.IsEmpty())
		{
			m_pLUM->m_Separator = strTagValue;
			bOk = LoadSeparatorExceptions(pNode);
		}

	delete pNode;
	delete pMainNode;

	return bOk;
}

//-----------------------------------------------------------------------------
BOOL CXmlNumberToLiteralParser::LoadSeparatorExceptions(CXMLNode* aNode)
{
	CString	strTagValue;

	// Mi leggo la lista di Digits
	CXMLNodeChildsList* pNodeDigits = aNode->SelectNodes (CString(szXmlExeptions) + _T("/") + CString(szXmlDigits));

	// Se non ci sono Digits non ne carico ma non ritorno errori
	if (!pNodeDigits || !pNodeDigits->GetSize())
	{
		SAFE_DELETE(pNodeDigits);
		return TRUE;
	}

	for (int i=0; i <= pNodeDigits->GetUpperBound(); i++)
		if (pNodeDigits->GetAt(i))
			if (pNodeDigits->GetAt(i)->GetAttribute(szXmlAttrValue, strTagValue) && !strTagValue.IsEmpty())
			{
				DataInt dI;
				dI.Assign(strTagValue);
				int nVal = (int)dI;
	
				m_pLUM->AddSeparatorException(nVal);
			}

	delete pNodeDigits;

	return TRUE;
}
