
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
#include <TBGeneric\FormNSChanges.h>

#include "FormNSChangesParser.h"

//includere come ultimo include all'inizio del cpp
#include "begincpp.dex"

#ifdef _DEBUG
#undef THIS_FILE
static const char THIS_FILE[] = __FILE__;
#endif

////////////////////////////////////////////////////////////////////////////////
static const TCHAR szXmlFormNsChanges[]	= _T("FormNsChanges");
static const TCHAR szXmlFormNsChange[]	= _T("FormNsChange");
static const TCHAR szXmlNsRelease[]		= _T("NsRelease");

static const TCHAR szXmlAttrOld[]			= _T("old");
static const TCHAR szXmlAttrNew[]			= _T("new");
static const TCHAR szXmlAttrExactMatch[]	= _T("exactMatch");

//============================================================================

IMPLEMENT_DYNAMIC(CFormNSChangeParser, CObject);

//----------------------------------------------------------------------------
// LoadFormNSChange
//   Carica il file identificato da pFormNSChangeArray->m_Namespace
//     Chiama la CheckFormNSChange
//     Chiama la LoadFormNsChanges
//
BOOL CFormNSChangeParser::LoadFormNSChange (CFormNSChangeArray* pFormNSChangeArray, int dNsRelease, const CTBNamespace& nsDoc)
{
	if (!pFormNSChangeArray)
	{
		ASSERT(FALSE);
		return FALSE;
	}

	p_mFNC = pFormNSChangeArray;

	CString sFileName = AfxGetPathFinder()->GetDocumentFormNSChangesFile(nsDoc);
	if (!ExistFile(sFileName))
		return FALSE;

	CXMLDocumentObject* pDocument = new CXMLDocumentObject();

	// Controllo che il file xml che stò parsando sia valido
	if (!CheckFormNSChange(sFileName, *pDocument))
		return FALSE;

	// Carico l'header e gli elementi del file xml comprendendo un controllo sulla validità dei campi
	BOOL bOk = LoadFormNsChanges(*pDocument, dNsRelease);

	SAFE_DELETE(pDocument);

	return bOk;
}
//----------------------------------------------------------------------------
// CheckFormNSChange
//   Controlla il file identificato da aNs
//
BOOL CFormNSChangeParser::CheckFormNSChange (CString strFile, CXMLDocumentObject& aDocument)
{
	// Controllo l'esistenza dei files
	if (!ExistFile(strFile))
	{
		TRACE(_T("Missing file: " + strFile));
		return FALSE;
	}
	// Controllo se l'xml ha un formato valido
	if (!aDocument.LoadXMLFile(strFile))
	{
		AfxGetDiagnostic()->Add (_TB("The xml file has a format error."));
		return FALSE;
	}

	// Controllo se l'xml ha un tag radice "FormNsChanges"
	CXMLNode* pRoot = aDocument.GetRoot();
	CString sRootName;
	if (!pRoot || !pRoot->GetName(sRootName) || sRootName.CompareNoCase (szXmlFormNsChanges) != 0) 
	{
		AfxGetDiagnostic()->Add(_TB("The root node of the xml file is missing."));
		return FALSE;
	}

	return TRUE;
}
//----------------------------------------------------------------------------
// LoadFormNsChanges
//   Carica e controlla gli elementi del file xml contenuti nel tag <FormNsChanges>
//     Chiama la ParseTagFormNsChange
//
BOOL CFormNSChangeParser::LoadFormNsChanges(CXMLDocumentObject& aDocument, int dNsRelease)
{
	BOOL bOk = TRUE;

	// Parso il tag tag "NsRelease"
	CXMLNode* pNsRelease = aDocument.SelectSingleNode(_T("/") + CString(szXmlFormNsChanges) + _T("/") + szXmlNsRelease);
	if (!pNsRelease)
	{
		AfxGetDiagnostic()->Add(_TB("The release node of the xml file is missing."));
		return FALSE;
	}
	
	CString sNsReleaseValue;
	if (!pNsRelease->GetText(sNsReleaseValue))
	{
		AfxGetDiagnostic()->Add (_TB("<Tag> NsRelease without value!"));
		return FALSE;
	}
	
	delete(pNsRelease);

	p_mFNC->m_NsRelease = atoi((CStringA)sNsReleaseValue);

	if (dNsRelease >= p_mFNC->m_NsRelease) //è già stato aggiornato
		return FALSE;

	CXMLNodeChildsList* pFormNsChangeNodes = aDocument.SelectNodes (_T("/") + CString(szXmlFormNsChanges) + _T("/") + szXmlFormNsChange);

	// Se non ci sono FieldType non ne carico ma non ritorno errori
	if (!pFormNsChangeNodes || !pFormNsChangeNodes->GetSize())
		return TRUE;

	// anche se un TAG è errato non mi fermo a caricare il file
	// così fornisco una diagnostica completa di tutto
	for (int i=0; i <= pFormNsChangeNodes->GetUpperBound(); i++)
		if (pFormNsChangeNodes->GetAt(i) && !ParseTagFormNsChange(pFormNsChangeNodes->GetAt(i)))
			bOk = FALSE;

	delete pFormNsChangeNodes;

	return bOk;
}

//-----------------------------------------------------------------------------
// ParseTagFormNsChange
//   Carica e controlla un singolo field del file xml contenuto nel tag <Field>
//
BOOL CFormNSChangeParser::ParseTagFormNsChange(CXMLNode* pTagNode)
{
	BOOL bOk = TRUE;

	// leggo l'attributo old del TAG
	CString	strTagOld;
	if (!pTagNode->GetAttribute(szXmlAttrOld, strTagOld) || strTagOld.IsEmpty())
	{
		AfxGetDiagnostic()->Add (_TB("<Tag> element without attribute old!"));
		return FALSE;
	}

	// leggo l'attributo new del TAG
	CString	strTagNew;
	if (!pTagNode->GetAttribute(szXmlAttrNew, strTagNew) || strTagNew.IsEmpty())
	{
		AfxGetDiagnostic()->Add (_TB("<Tag> element without attribute new!"));
		return FALSE;
	}

	// leggo l'attributo new del TAG
	CString	strTagExactMatch;
	if (!pTagNode->GetAttribute(szXmlAttrExactMatch, strTagExactMatch) || strTagExactMatch.IsEmpty())
	{
		AfxGetDiagnostic()->Add (_TB("<Tag> element without attribute exactMatch!"));
		return FALSE;
	}

	DataBool bTagExactMatch;
	bTagExactMatch.AssignFromXMLString(strTagExactMatch);

	p_mFNC->Add(new CFormNSChange(strTagOld, strTagNew, bTagExactMatch));

	return bOk;
}