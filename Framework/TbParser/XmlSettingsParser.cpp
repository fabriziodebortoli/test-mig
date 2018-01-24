#include "stdafx.h"

#include <io.h>

#include <TbXmlCore\XMLTags.h>
#include <TbXmlCore\XMLDocObj.h>
#include <TbXmlCore\XMLParser.h>

#include <TbNameSolver\Chars.h>
#include <TbNameSolver\PathFinder.h>
#include <TbNameSolver\FileSystemFunctions.h>
#include <TbNameSolver\IFileSystemManager.h>
#include <TbNameSolver\Diagnostic.h>

#include <TbGeneric\SettingsTable.h>
#include <TbGeneric\OutDateObjectsInfo.h>
#include <TbGeneric\StatusBarMessages.h>
#include <TbGeneric\GeneralFunctions.h>

#include "XmlSettingsParser.h"

//includere come ultimo include all'inizio del cpp
#include "begincpp.dex"

#ifdef _DEBUG
#undef THIS_FILE
static const char THIS_FILE[] = __FILE__;
#endif

// contatore riservato alla attivazione
static const TCHAR szTbWebServicesWrappersCounterFile[]	= _T("MDServices.config");
static const TCHAR szSearchSettingsKey[]					= _T("*.config");

//----------------------------------------------------------------------------------------------
//							XMLSettingsParser
//----------------------------------------------------------------------------------------------
//
//----------------------------------------------------------------------------------------------
IMPLEMENT_DYNAMIC(XMLSettingsParser, CObject)

//----------------------------------------------------------------------------------------------
XMLSettingsParser::XMLSettingsParser()
	:
	m_pXmlDocument	(NULL),
	m_bEmpty		(FALSE)
{
	m_pXmlDocument = new CXMLDocumentObject();
}

//----------------------------------------------------------------------------------------------
XMLSettingsParser::~XMLSettingsParser()
{
	delete m_pXmlDocument;
}

//-----------------------------------------------------------------------------
BOOL XMLSettingsParser::LoadSettings	(	
											const CTBNamespace&			aNamespace, 
											const CPathFinder*			pPathFinder, 
											const CString&				sUser,  
											const CBaseDescriptionArray& aOutDates,
											CStatusBarMsg*				pStatusBar
										)
{
	if (!pPathFinder || !aNamespace.IsValid())
		return FALSE;

	AfxGetApp()->BeginWaitCursor();

	pStatusBar->Show(cwsprintf(_TB("Loading Settings for the %s..."), aNamespace.ToString()));

	// inizializzo gli elementi comuni
	m_Owner		= aNamespace;
	m_OutDates	= aOutDates;

	//  modulo
	m_Source	= SettingObject::FROM_STANDARD;
	LoadFiles (pPathFinder->GetModuleSettingsPath(aNamespace, CPathFinder::STANDARD));
	
	m_Source	= SettingObject::FROM_COMPANYUSERS;
	LoadFiles (pPathFinder->GetModuleSettingsPath (aNamespace, CPathFinder::ALL_USERS));

	m_Source	= SettingObject::FROM_COMPANYUSER;
	LoadFiles (pPathFinder->GetModuleSettingsPath (aNamespace, CPathFinder::USERS, sUser));

	AfxGetSettingsTable()->SetModified(aNamespace, FALSE);

	AfxGetApp()->EndWaitCursor();

	return TRUE;
}

// Carica in un'array di stringhe l'elenco dei files da caricare
//-----------------------------------------------------------------------------
void XMLSettingsParser::LoadFiles (const CString sDir)
{ 
	CStringArray aFiles;
	AfxGetFileSystemManager ()->GetFiles (sDir, szSearchSettingsKey, &aFiles);
	// per tutti i files letti carico i parametri, non ho motivo
	// particolare di invalidare il loading dei moduli
	CString sFilename;
	
	for (int i=0; i <= aFiles.GetUpperBound(); i++)
	{
		sFilename = aFiles.GetAt(i);

		if (sFilename.IsEmpty() || _tcsicmp(sFilename, sDir + SLASH_CHAR + szTbWebServicesWrappersCounterFile) == 0)
			continue;
		
		if (m_pXmlDocument->LoadXMLFile(sFilename))
			Parse (m_pXmlDocument, AfxGetSettingsTable(), sFilename);
	}
}

// si occupa di verificare se il file letto è stato modificato
//----------------------------------------------------------------------------------------------
BOOL XMLSettingsParser::IsModifiedFile (
											const CTBNamespace&					aModule, 
											const CString&						sFileName, 
											const SettingObject::SettingSource	aFrom,
											SettingsTablePtr					pSettingsTable
										)
{
	DataDate aNewDate		= GetFileDate(sFileName);
	DataDate aOriginalDate	= pSettingsTable->GetLastFileDate
								(
									aModule, 
									GetNameWithExtension(sFileName), 
									aFrom
								);

	return aNewDate != aOriginalDate;
}

//----------------------------------------------------------------------------------------------
DataDate XMLSettingsParser::GetFileDate (const CString& sFileName)
{
	DataDate aFileDate;
	aFileDate.SetFullDate(TRUE);

	CFileStatus aFileStatus;
	GetStatus(sFileName, aFileStatus);
	aFileDate = aFileStatus.m_mtime;

	return aFileDate;
}

//----------------------------------------------------------------------------------------------
BOOL XMLSettingsParser::Parse (CXMLDocumentObject* pDoc, SettingsTablePtr pTable, const CString& sFile)
{
	CString sNodeName;

	CXMLNode* pRoot = pDoc->GetRoot();
	if (!pRoot || !pRoot->GetName(sNodeName) || _tcsicmp(sNodeName, XML_SETTINGS_TAG) != 0) 
	{
		AfxGetDiagnostic()->Add (cwsprintf(_TB("The file {0-%s}, {1-%s} has no root element. Settings file not loaded."),
			(LPCTSTR)m_Owner.ToString(), (LPCTSTR) m_sFileName), CDiagnostic::Warning);
			
		return FALSE;
	}
	
	CXMLNodeChildsList* pNodes = pRoot->GetChilds();
	if (!pNodes)
		return TRUE;

	m_DataFile	= GetFileDate(sFile);
	// woorm report config files has more extensions, I have to preserve them
	m_sFileName	= GetNameWithExtension(sFile);

	// parso le sezioni
	SettingsSection* pSection;
	CXMLNode* pNode;
	for (int i=0; i <= pNodes->GetUpperBound(); i++)
	{
		pNode = pNodes->GetAt(i);

		if	(!pNode || !pNode->GetName(sNodeName) || sNodeName != XML_SECTION_TAG)
			continue;

		pSection = new SettingsSection ();
		
		pSection->SetFileName		(m_sFileName);
		pSection->SetOwner			(m_Owner);
		pSection->SetLastFileDate	(m_Source, m_DataFile);

		if (ParseSection (pNode, pSection))
			pTable->AddSection (pSection);
		else
			delete pSection;
	}

	// forzo l'update dei current values
	pTable->UpdateCurrentValuesOf(m_Owner, m_sFileName, _T(""));
	return TRUE;
}

//----------------------------------------------------------------------------------------------
BOOL XMLSettingsParser::Unparse (
									const CPathFinder*	pPathFinder, 
									const CTBNamespace& aModule,
									const CString&		sFileName, 
									const CString&		sSection, 
									const int&			nRelease, 
									const CString&		sUser, 
									SettingsTable*		pTable
								)
{
	m_Owner	= aModule;

	CStatusBarMsg	msgBar(TRUE, TRUE); 
	msgBar.Show(_TB("Saving Module Parameters..."));

	CXMLDocumentObject aDoc;

	CXMLNode* pRoot = aDoc.CreateRoot(XML_SETTINGS_TAG);
	if (!pRoot)
	{
		ASSERT(FALSE);
		return FALSE;
	}

	m_bEmpty = TRUE;

	// gruppi e sezioni
	SettingsGroup* pGroup = pTable->GetGroup(m_Owner);
	SettingsSection* pSection = NULL;
	if (pGroup)
		for (int i=0; i <= pGroup->GetUpperBound(); i++)
		{
			pSection = pGroup->GetAt(i);
			if (_tcsicmp(pSection->GetFileName(), sFileName) == 0)
			{
				// indica che la sezione e i suoi settings vanno aggiornati
				BOOL bUpdateSection = sSection.IsEmpty() || _tcsicmp(pSection->GetName(), sSection) == 0;
				UnparseSection(pRoot, pSection, bUpdateSection);
			}
		}

	// salvo il file come richiesto
	BOOL bAllUsers		= (m_Source == SettingObject::FROM_ALLCOMPANYUSERS || m_Source == SettingObject::FROM_COMPANYUSERS);
	CString sFile;
	if (m_Source == SettingObject::FROM_STANDARD) 
		sFile = pPathFinder->GetModuleSettingsPath(aModule, CPathFinder::STANDARD) + SLASH_CHAR + sFileName;
	else
		sFile = pPathFinder->GetModuleSettingsPath
					(
						aModule, 
						bAllUsers ? CPathFinder::ALL_USERS : CPathFinder::USERS,
						sUser, 
						TRUE
					) + SLASH_CHAR + sFileName;

	// se il file si svuota lo elimino da disco
	BOOL bOk = TRUE;
	if (!m_bEmpty)
		bOk = aDoc.SaveXMLFile(sFile, FALSE);
	else 
	{
		// se esiste il vecchio file lo rimuovo
		if (ExistFile(sFile))
			DeleteFile(sFile);

		// I remove directory only if it remains empty
		CStringArray aFiles;
		AfxGetFileSystemManager ()->GetFiles (GetPath(sFile), szSearchSettingsKey, &aFiles);

		if (aFiles.GetSize() == 0)
			RemoveEmptyParentFolders (GetPath(sFile));
	}

	return bOk;
}

//----------------------------------------------------------------------------------------------
BOOL XMLSettingsParser::ParseSection (CXMLNode* pNode, SettingsSection* pSection)
{
	if (!pSection)
		return FALSE;

	CString sTemp;
	if (!pNode || pNode->GetAttribute(XML_NAME_ATTRIBUTE, sTemp) && sTemp.IsEmpty())
	{
		AfxGetDiagnostic()->Add (cwsprintf(_TB("In the file {0-%s}, {1-%s} there is an unnamed section. Section ignored."),
			(LPCTSTR)m_Owner.ToString(), (LPCTSTR) m_sFileName), CDiagnostic::Warning);
		return FALSE;
	}
	
	pSection->SetName (sTemp);

	pNode->GetAttribute(XML_RELEASE_ATTRIBUTE, sTemp);
	int nRelease = _ttoi((LPCTSTR) sTemp);

	if (m_Source != SettingObject::FROM_STANDARD && IsOutDated(pSection->GetName(), nRelease))
		return FALSE;

	pSection->SetRelease(nRelease);

	CXMLNode* pObjNode;
	CXMLNodeChildsList* pNodes;
	SettingObject* pNewSetting;
	CString sNodeName;

	pNodes = pNode->GetChilds();
	
	if (!pNodes)
		return FALSE;

	// figli della sezione
	for (int i=0; i <= pNodes->GetUpperBound(); i++)
	{
		pObjNode = pNodes->GetAt(i);

		if	(!pObjNode || !pObjNode->GetName(sNodeName) || sNodeName != XML_SETTING_TAG)
			continue;

		pNewSetting = new SettingObject ();
		pNewSetting->SetNamespace(m_Owner);
		pNewSetting->SetSource(m_Source);
		if (ParseSetting (pObjNode, pNewSetting, pSection->GetName()))
			pSection->AddSetting(pNewSetting);
		else
			delete pNewSetting;
	}
	
	return TRUE;
}

//----------------------------------------------------------------------------------------------
void XMLSettingsParser::UnparseSection (CXMLNode* pNode, SettingsSection* pSection, BOOL bToUpdate)
{
	CXMLNode* pNewNode = pNode->CreateNewChild (XML_SECTION_TAG);
	
	CString sTemp;
	pNewNode->SetAttribute	(XML_NAME_ATTRIBUTE, (LPCTSTR) pSection->GetName());

	// di questa sezione va unparsata sempre la release della generale
	// settata dal programmatore o letta dalla tabella generale
	SettingsSection* pRealSection = AfxGetSettingsTable()->GetExactSection
			(
				pSection->GetOwner(),
				pSection->GetFileName(),
				pSection->GetName()
			);
	if (pRealSection)
		pSection->SetRelease(pRealSection->GetRelease());

	if (pSection->GetRelease() > 0)
	{
		const rsize_t nLen = 10;
		TCHAR szRelease[nLen];
		_itot_s(pSection->GetRelease(), szRelease, nLen, 10);
		pNewNode->SetAttribute	(XML_RELEASE_ATTRIBUTE, szRelease);
	}

	// poi i settaggi
	SettingObject*	pSetting;
	SettingObjects* pSOjects;
	for (int i=0; i <= pSection->GetSettings().GetUpperBound(); i++)
	{
		pSOjects = (SettingObjects*) pSection->GetSettings().GetAt(i);
		pSetting = GetSettingToUnparse(pSOjects, bToUpdate);

		if (pSetting)
			UnparseSetting(pNewNode, pSetting, bToUpdate);
	}

	// se non ho settaggi butto via la sezione
	if (!pNewNode->GetChilds())
		pNode->RemoveChild(pNewNode);
}

// si occupa di decidere quale versione salvare nel file, sulla base della scaletta di priorità
// delle personalizzazioni e sul fatto che il settaggio vada o meno salvato
//----------------------------------------------------------------------------------------------
SettingObject* XMLSettingsParser::GetSettingToUnparse (SettingObjects* pSettings, BOOL bToUpdate)
{
	if (!pSettings)
		return NULL;

	// se non devo salvare il setting, rimetto nel file il valore originale
	// se non esisteva ritorna NULL e non verrà unparsato 
	if (!bToUpdate)
		return pSettings->GetSetting(m_Source);
	
	SettingObject* pParent = NULL;
	SettingObject* pCurrent= pSettings->GetSetting();

	// scaletta progressiva di priorità (non mettere i break!)
	switch (m_Source)
	{
	case SettingObject::FROM_COMPANYUSER:
		if (!pParent)	pParent = pSettings->GetSetting(SettingObject::FROM_COMPANYUSERS);
	case SettingObject::FROM_COMPANYUSERS:
		if (!pParent)	pParent = pSettings->GetSetting(SettingObject::FROM_ALLCOMPANYUSER);
	case SettingObject::FROM_ALLCOMPANYUSER:
		if (!pParent)	pParent = pSettings->GetSetting(SettingObject::FROM_ALLCOMPANYUSERS);
	case SettingObject::FROM_ALLCOMPANYUSERS:
		if (!pParent)	pParent = pSettings->GetSetting(SettingObject::FROM_STANDARD);
	}

	if (!pParent) pParent = pSettings->GetSetting(SettingObject::DEFAULT_VALUE);

	// ritorno il nuovo valore se ci sono modifiche oppure se non esisteva l'originario
	// ho spezzato in tre if per meglio leggere i casi
	if (!pParent)
	{
		// si tratta del valore di default, quindi non devo unparsare
		if (pCurrent && pCurrent->GetSource() == SettingObject::DEFAULT_VALUE)
			return NULL;

		return pCurrent;
	}

	// è stato eliminato
	if (!pCurrent && pParent)
	{
		// rimane a default o a standard (non modificabile mai)
		if	(
				pParent->GetSource() == SettingObject::DEFAULT_VALUE || 
				pParent->GetSource() == SettingObject::FROM_STANDARD
			)
			return NULL;

		// altrimenti ritorno il parent
		return pParent;
	}

	// è modificato rispetto all'originario
	if (pParent->IsArray() != pCurrent->IsArray())
		return pCurrent;

	if	(
			(!pParent->GetValue() && pCurrent->GetValue()) ||
			(pParent->GetValue() && !pCurrent->GetValue()) ||
			(pParent->GetValue() && pCurrent->GetValue() && *pParent->GetValue() != *pCurrent->GetValue())
		)
		return pCurrent;
	
	return NULL;
}

//----------------------------------------------------------------------------------------------
BOOL XMLSettingsParser::ParseSetting (CXMLNode* pNode, SettingObject* pSetting, const CString& sSectionName)
{
	// fino ad oggi è tutto limitato
	if (!pNode || !pSetting || !pSetting->Parse(pNode))
		return FALSE;

	CString sTemp;
	pNode->GetAttribute(XML_RELEASE_ATTRIBUTE, sTemp);
	int nRelease = _ttoi((LPCTSTR) sTemp);
	pSetting->SetRelease(nRelease);

	return m_Source == SettingObject::FROM_STANDARD || !IsOutDated(sSectionName, pSetting->GetName(), nRelease);
}

//----------------------------------------------------------------------------------------------
void XMLSettingsParser::UnparseSetting (CXMLNode* pNode, SettingObject* pSetting, BOOL bToUpdate)
{
	m_bEmpty = FALSE;

	CXMLNode* pNewNode = pNode->CreateNewChild (XML_SETTING_TAG);
	if (pSetting->GetRelease() > 0)
	{
		const rsize_t nLen = 10;
		TCHAR szRelease[nLen];
		_itot_s(pSetting->GetRelease(), szRelease, nLen, 10);
		pNewNode->SetAttribute (XML_RELEASE_ATTRIBUTE, szRelease);
	}

	// fino ad oggi è tutto limitato
	pSetting->Unparse(pNewNode);
}

// verifica se è stata dichiarata OutOfDate l'intera sezione
//----------------------------------------------------------------------------------------------
BOOL XMLSettingsParser::IsOutDated (const CString sSectionName, const int& nRelease)
{
	COutDateSettingsSectionDescription* pSection = GetOutDatedSection(sSectionName);
	
	return pSection && pSection->IsOutDate(nRelease);
}

// verifica se è stata dichiarata OutOfDate il singolo parametro
//----------------------------------------------------------------------------------------------
BOOL XMLSettingsParser::IsOutDated (const CString sSectionName, const CString sSettingName, const int& nRelease)
{
	COutDateSettingsSectionDescription* pSection = GetOutDatedSection(sSectionName);

	if (!pSection)
		return FALSE;

	COutDateObjectDescription* pSetting;
	for (int i=0; i <= pSection->GetSettings().GetUpperBound(); i++)
	{
		pSetting = (COutDateObjectDescription*) pSection->GetSettings().GetAt(i);
		if (_tcsicmp(pSetting->GetName(), sSettingName) == 0)
			return pSetting->IsOutDate(nRelease);
	}

	return FALSE;
}

//----------------------------------------------------------------------------------------------
COutDateSettingsSectionDescription* XMLSettingsParser::GetOutDatedSection (const CString sSectionName)
{
	COutDateSettingsSectionDescription* pOutDate;

	for (int i=0; i <= m_OutDates.GetUpperBound(); i++)
	{
		pOutDate = (COutDateSettingsSectionDescription*) m_OutDates.GetAt(i);
		if	(
				_tcsicmp(pOutDate->GetOwner(), m_sFileName) == 0 &&
				_tcsicmp(pOutDate->GetName(), sSectionName) == 0
			)
			return pOutDate;
	}

	return NULL;
}

// si occupa di ricaricare nella tabella i files che non sono aggiornati e travasa 
// i valori nella nuova tabella
//----------------------------------------------------------------------------------------------
BOOL XMLSettingsParser::RefreshTable(
										const CPathFinder*	pPathFinder, 
										const CTBNamespace& aModule, 
										const CString&		sFileName,
										const CString&		sUser,
										SettingsTablePtr	pSettingsTable,
										BOOL				bOnlyModified /*TRUE*/

									)
{
	// mi salvo il vecchio valore
	SettingObject::SettingSource aSource = m_Source;
	m_Owner = aModule;

	BOOL bRefreshed = TRUE;
	CString sPath;
	
	// standard
	sPath = pPathFinder->GetModuleSettingsPath(aModule, CPathFinder::STANDARD) + SLASH_CHAR + sFileName;

	m_Source = SettingObject::FROM_STANDARD;
	if (ExistFile(sPath) && (!bOnlyModified || IsModifiedFile(aModule, sPath, m_Source, pSettingsTable)))
		if (!m_pXmlDocument->LoadXMLFile(sPath) || !Parse (m_pXmlDocument, pSettingsTable, sPath))
			bRefreshed = FALSE;

	// Company/allUsers
	sPath = pPathFinder->GetModuleSettingsPath(aModule, CPathFinder::ALL_USERS, _T(""), FALSE) + SLASH_CHAR + sFileName;
	m_Source = SettingObject::FROM_COMPANYUSERS;

	if (ExistFile(sPath) && (!bOnlyModified || IsModifiedFile(aModule, sPath, m_Source, pSettingsTable)))
		if (!m_pXmlDocument->LoadXMLFile(sPath) || !Parse (m_pXmlDocument, pSettingsTable, sPath))
			bRefreshed = FALSE;

	// Company/User viene riletto sempre
	sPath = pPathFinder->GetModuleSettingsPath(aModule, CPathFinder::USERS, sUser, FALSE) + SLASH_CHAR + sFileName;
	m_Source = SettingObject::FROM_COMPANYUSER;

	if (ExistFile(sPath))
		if (!m_pXmlDocument->LoadXMLFile(sPath) ||!Parse (m_pXmlDocument, pSettingsTable, sPath))
			bRefreshed = FALSE;

	// ripristino il valore di entrata
	m_Source = aSource;

	return bRefreshed;
}
