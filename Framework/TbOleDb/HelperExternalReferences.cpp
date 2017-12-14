 
#include "stdafx.h"

#include <TbGes\XMLGesInfo.h>

#include "HelperExternalReferences.h"

//includere come ultimo include all'inizio del cpp
#include "begincpp.dex"

#ifdef _DEBUG
#undef THIS_FILE
static const char THIS_FILE[] = __FILE__;
#endif

#ifdef _DEBUG
#define new DEBUG_NEW
#endif

//=============================================================================

// comparazione di oggetti CTableDBT (dbts.xml) da ordinare per nome tabella
//-----------------------------------------------------------------------------
int CompareDBTTables(CObject* po1, CObject* po2)
{
	CHelperExternalReferences::CTableDBT* p1 = (CHelperExternalReferences::CTableDBT*)po1;
	CHelperExternalReferences::CTableDBT* p2 = (CHelperExternalReferences::CTableDBT*)po2;

	return p1->m_sTableName.CompareNoCase(p2->m_sTableName);
}

// comparazione di oggetti CTableSingleExtRef (externalreferences.xml) da ordinare per nome colonna
//-----------------------------------------------------------------------------
int CompareDBTExtRef(CObject* po1, CObject* po2)
{
	CHelperExternalReferences::CTableSingleExtRef* p1 = (CHelperExternalReferences::CTableSingleExtRef*)po1;
	CHelperExternalReferences::CTableSingleExtRef* p2 = (CHelperExternalReferences::CTableSingleExtRef*)po2;

	return p1->m_sForeignKey.CompareNoCase(p2->m_sForeignKey);
}

//////////////////////////////////////////////////////////////////////////////
//							CHelperExternalReferences
//////////////////////////////////////////////////////////////////////////////
IMPLEMENT_DYNAMIC(CHelperExternalReferences, CObject);

//-----------------------------------------------------------------------------
CHelperExternalReferences::CHelperExternalReferences()
	:	m_pHelperSqlCatalog		(NULL),
		m_bOwnedHelperSqlCatalog(FALSE),
		m_bDBTsLoaded		(FALSE)
{
	m_arDBTs.SetOwns(TRUE);
	m_arDBTs.SetCompareFunction(::CompareDBTTables);//setto funzione di ordinamento per nome tabella

	m_arExternalReferences.SetOwns(TRUE);
}

//-----------------------------------------------------------------------------
CHelperExternalReferences::~CHelperExternalReferences()
{
	// se istanziato localmente distrugge l'oggetto helpersqlcatalog
	if (m_bOwnedHelperSqlCatalog)
	{
		SAFE_DELETE(m_pHelperSqlCatalog);
	}
	// pulisce gli array e cancella gli oggetti (v. SetOwns(TRUE))
	m_arDBTs.RemoveAll();
	m_arExternalReferences.RemoveAll();
	// pulisce le mappe
	m_mapNSDocToDbts.RemoveAll();
	m_mapTableToExtRefs.RemoveAll();
}

// carica tutti i file dbts.xml
//-----------------------------------------------------------------------------
BOOL CHelperExternalReferences::LoadDBTs()
{
	// se array già caricato esce
	if (m_bDBTsLoaded)
		return TRUE;

	LoadHelperSqlCatalog();

	// cicla sui moduli e cerca per ogni modulo carica tutti i dbts.xml
	CHelperSqlCatalog::CModuleTables* pMT;
	for (int i = 0; i < m_pHelperSqlCatalog->m_arModules.GetSize(); i++)
	{
		pMT = dynamic_cast<CHelperSqlCatalog::CModuleTables*>(m_pHelperSqlCatalog->m_arModules.GetAt(i));
		ASSERT_VALID(pMT);
		ASSERT_VALID(pMT->m_pModule);

		if (!pMT->m_pModule->m_sModulePath.IsEmpty())
			LoadModuleDbts(pMT->m_pModule);
	}

	m_bDBTsLoaded = TRUE;

	return TRUE;
}

// carica tutti i file dbts.xml di un singolo modulo
//-----------------------------------------------------------------------------
void CHelperExternalReferences::LoadModuleDbts(AddOnModule* pAddOnModule)
{
	// Scorre tutte le directory del path ModuleObjects e per ogni directory
	// cerca il file Description\Dbts.xml e lo carica
	HANDLE hFind;
    WIN32_FIND_DATA fd;
	BOOL bResult = FALSE;
	CString strTmpPath = pAddOnModule->m_sModulePath + SLASH_CHAR + _T("ModuleObjects");
	CString strModuleObjectsPath;
	CString sFileName;
	CString sUpperDocNS;
	CString sSlaveName;
	CString sDbtsXmlFileName, sDbtsXmlPath;
	if (strTmpPath.Left(2) == _T("\\\\"))
		strTmpPath = _T("\\\\?\\UNC\\") + strTmpPath.Mid(2);

	if (!IsDirSeparator(strTmpPath.Right (1))) 
		strTmpPath += SLASH_CHAR;

	strModuleObjectsPath = strTmpPath;
	strTmpPath += _T("*.*");
    
	if ((hFind = ::FindFirstFile ((LPCTSTR) strTmpPath, &fd)) != INVALID_HANDLE_VALUE)
	{
		CString sTableName;
		CString sDBTNS;
		CString sDocNS;
		CXMLNode* pDBTMasterNode;
		CXMLNode* pDBTSlavesNode;
		CXMLNode* pDBTSlaveNode;
		CXMLNode* pDBTChildNode;
		CTableDBT* pTableDBTMaster;
		CTableDBT* pTableDBTSlave;
		do
		{
			if (fd.dwFileAttributes & FILE_ATTRIBUTE_DIRECTORY)
			{
				sFileName = fd.cFileName;
				// salta le cartelle ., .. e DBInfo perché non contengono documenti
				if (sFileName != _T(".") &&
					sFileName != _T("..") &&
					sFileName.CompareNoCase(_T("DBInfo")) != 0)
				{
					sDbtsXmlPath	 = strModuleObjectsPath + sFileName + SLASH_CHAR + _T("Description");
					sDbtsXmlFileName = sDbtsXmlPath + SLASH_CHAR + _T("Dbts.xml");
					if (ExistFile(sDbtsXmlFileName))
					{
						// se esiste il file dbts.xml lo apre e lo carica in array
						sTableName.Empty();
						sDBTNS.Empty();
						sDocNS.Empty();
						CLocalizableXMLDocument aXML(pAddOnModule->m_Namespace, AfxGetPathFinder());
						aXML.LoadXMLFile(sDbtsXmlFileName);//carica il file dbts.xml
						pDBTMasterNode = aXML.GetRootChildByName(_T("Master")); //cerca il nodo Master del DBT
						if (pDBTMasterNode)
						{
							if (pDBTMasterNode->GetAttribute(_T("namespace"), sDBTNS))//cerca l'attributo namespace del DBT Master
							{
								sDocNS = sDBTNS.Left(sDBTNS.ReverseFind(_T('.')));//prende il namespace del dbt esclusa l'ultima parte come namespace del documento
								pDBTChildNode = pDBTMasterNode->GetChildByName(_T("Table"));//cerca il nodo figlio Table
								if (pDBTChildNode)
									pDBTChildNode->GetText(sTableName);//ne prende la descrizione
							}
						}
						if (!sTableName.IsEmpty() &&
							!sDBTNS.IsEmpty() &&
							!sDocNS.IsEmpty())
						{
							// maiuscolo docns per mappa
							sUpperDocNS = sDocNS;
							sUpperDocNS.MakeUpper();
							// se nome tabella, ns dbt e ns documento presenti allora aggiunge nell'array dei dbts.xml
							// unitamente al path del file dbts.xml e al namespace del modulo e lo indica come MASTER
							pTableDBTMaster = new CTableDBT(sTableName, sDBTNS, sDocNS, sDbtsXmlPath, pAddOnModule->m_Namespace.ToString(), CTableDBT::MASTER);
							m_arDBTs.Add(pTableDBTMaster); 
							m_mapNSDocToDbts.SetAt(sUpperDocNS, pTableDBTMaster);//aggiunge il DocNS nella mappa per ricerche successive
							// cicla sui nodi figli per gli slaves
							pDBTSlavesNode = pDBTMasterNode->GetChildByName(_T("Slaves"));
							if (pDBTSlavesNode)
							{
								pDBTSlaveNode = pDBTSlavesNode->GetFirstChild();
								while (pDBTSlaveNode)
								{
									sTableName.	Empty();
									sDBTNS.		Empty();
									if (pDBTSlaveNode->GetAttribute(_T("namespace"), sDBTNS))//cerca l'attributo namespace del DBT Slave
									{
										pDBTChildNode = pDBTSlaveNode->GetChildByName(_T("Table"));//cerca il nodo figlio Table
										if (pDBTChildNode)
											pDBTChildNode->GetText(sTableName);//ne prende la descrizione
									}

									if (!sTableName.IsEmpty() &&
										!sDBTNS.IsEmpty())
									{
										// se nome tabella, ns dbt presenti allora aggiunge nell'array dei dbts.xml e all'array degli slave nel master
										// unitamente al path del file dbts.xml e al namespace del modulo e lo indica come SLAVE o SLAVEBUFFERED in base al nome del nodo
										pDBTSlaveNode->GetName(sSlaveName);
										CTableDBT::DBTType eDBTType = sSlaveName.CompareNoCase(_T("Slave")) == 0 ? CTableDBT::SLAVE : CTableDBT::SLAVEBUFFERED;
										pTableDBTSlave = new CTableDBT(sTableName, sDBTNS, sDocNS, sDbtsXmlPath, pAddOnModule->m_Namespace.ToString(), eDBTType, pTableDBTMaster);
										m_arDBTs.Add(pTableDBTSlave); 
										pTableDBTMaster->m_arDBTSlaves.Add(pTableDBTSlave);
									}
									pDBTSlaveNode = pDBTSlavesNode->GetNextChild();
								}
							}
						}
					}
				}
			}
		} while(::FindNextFile(hFind, &fd));
		::FindClose (hFind);
	}

	// sort su m_arDBTs per nome tabella
	m_arDBTs.QuickSort();
}

// carica gli external references di una tabella by name (MA_???)
//-----------------------------------------------------------------------------
BOOL CHelperExternalReferences::LoadByTableName(const CString& sTableName)
{
	// se tabella già caricata allora ok
	if (IsTableAlreadyLoaded(sTableName))
		return TRUE;

	LoadHelperSqlCatalog();

	// se array dbts.xml non caricato lo carico adesso
	if (!m_bDBTsLoaded)
		LoadDBTs();

	// cerca il nome della tabella nell'array dei dbts.xml
	// può essere che la stessa tabella sia usata in più dbts.xml
	// quindi ciclo finché la trovo la prima volta e poi finché
	// non la trovo più (array ordinato per nome tabella)
	CTableDBT* pDBT;

	// Cerca l'elemento più vicino con sTableName. Tuttavia dato che potrebbero 
	// esserci più elementi con stessa tabella occorre possibilemente aggiustare
	// la ricerca manualmente
	CTableDBT aDBTSearch(sTableName);
	int idx = m_arDBTs.BinarySearch(&aDBTSearch);
	if (idx > 0)
	{
		for (int i = idx - 1; i >= 0; i--)
		{
			pDBT = (CTableDBT*)m_arDBTs.GetAt(i);
			if (pDBT->m_sTableName.CompareNoCase(sTableName) == 0)
				idx = i;
			else 
				break;
		}
	}
	if (idx < 0)
		return FALSE;

	/* //@@BEFOREMAPTODELETE
	BOOL bTableFound = FALSE;
	*/

	CTableExtRefs* pExtRefs = NULL;
	for (int i = idx; i < m_arDBTs.GetSize(); i++)
	{
		pDBT = (CTableDBT*)m_arDBTs.GetAt(i);
		if (pDBT->m_sTableName.CompareNoCase(sTableName) == 0)
		{
			LoadExternalReferences(pDBT, &pExtRefs);
			//bTableFound = TRUE;//@@BEFOREMAPTODELETE
		}
		else //if (bTableFound)//@@BEFOREMAPTODELETE
			break;
	}

	if (pExtRefs)
		pExtRefs->m_arExtRefs.QuickSort();//ordina solo quando la tabella è completata

	return TRUE;
}

//-----------------------------------------------------------------------------
void CHelperExternalReferences::LoadExternalReferences(CTableDBT* pDBT, CTableExtRefs** ppExtRefs)
{
	// maiuscolo nome tabella per mappa
	CString sUpperTableName = pDBT->m_sTableName;
	sUpperTableName.MakeUpper();
	// se non esiste il file ExternalReferences.xml esce
	CString sExtRefsXmlFileName = pDBT->m_sDbtsxmlPath + SLASH_CHAR + _T("ExternalReferences.xml");
	if (!ExistFile(sExtRefsXmlFileName))
		return;

	// se non esiste ancora l'oggetto CTableExtRefs lo crea
	if (!*ppExtRefs)
	{
		*ppExtRefs = new CTableExtRefs(pDBT->m_sTableName);
		(*ppExtRefs)->m_arExtRefs.SetCompareFunction(::CompareDBTExtRef);
		m_arExternalReferences.Add(*ppExtRefs);//aggiunge in array degli external references
		m_mapTableToExtRefs.SetAt(sUpperTableName, *ppExtRefs);//aggiunge il TableName nella mappa per ricerche successive
	}

	CTableExtRefs* pExtRefs = *ppExtRefs;
	CString sDBTNS;
	CString sExtDocNS;
	CString sForeignTable;
	CString sForeignKey;
	CString sPrimaryKey;
	CString sExpression;
	CString sUpperDocNS;
	CXMLNode* pExtRefsNode;
	CXMLNode* pExtRefNode;
	CXMLNode* pExpressionNode;
	CXMLNode* pForeignKeysNode;
	CXMLNode* pKeySegmentNode;
	CXMLNode* pForeignNode;
	CXMLNode* pPrimaryNode;
	CTableDBT* pDBTExt;
	CTableSingleExtRef* pSingleRef;

	CLocalizableXMLDocument aXML(CTBNamespace(pDBT->m_sModuleNS), AfxGetPathFinder());
	aXML.LoadXMLFile(sExtRefsXmlFileName);//carica il file dbts.xml
	CXMLNode* pDBTNode = aXML.GetFirstRootChild(); //cerca il primo nodo DBT
	while (pDBTNode)
	{
		//cerca l'attributo namespace del DBT
		if (pDBTNode->GetAttribute(_T("namespace"), sDBTNS) && sDBTNS.CompareNoCase(pDBT->m_sDBTNS) == 0)
		{
			pExtRefsNode = pDBTNode->GetChildByName(_T("ExternalReferences"));
			pExtRefNode = pExtRefsNode->GetFirstChild();
			while (pExtRefNode)
			{
				if (pExtRefNode->GetAttribute(_T("namespace"), sExtDocNS))//cerca l'attributo namespace dell'external reference
				{
					sForeignTable.Empty();
					// maiuscolo nsdoc per mappa
					sUpperDocNS = sExtDocNS;
					sUpperDocNS.MakeUpper();
					// cerca la tabella external con il namespace di documento nel dbts.xml 
					pDBTExt = NULL;
					if (m_mapNSDocToDbts.Lookup(sUpperDocNS, (CObject*&)pDBTExt))
						sForeignTable = pDBTExt->m_sTableName;
/* //@@BEFOREMAPTODELETE
					for (int i = 0; i < m_arDBTs.GetSize(); i++)
					{
						pDBTExt = (CTableDBT*)m_arDBTs.GetAt(i);
						if (pDBTExt->m_sDocNS.CompareNoCase(sExtDocNS) == 0)
						{
							sForeignTable = pDBTExt->m_sTableName;
							break;
						}
					}
*/
					// se non ha trovato la tabella external è inutile andare avanti
					if (!sForeignTable.IsEmpty())
					{
						sExpression.Empty();
						// cerca il nodo Keys
						pExpressionNode = pExtRefNode->GetChildByName(_T("Expression"));
						if (pExpressionNode)
							pExpressionNode->GetText(sExpression);
						// cerca il nodo Keys
						pForeignKeysNode = pExtRefNode->GetChildByName(_T("Keys"));
						if (pForeignKeysNode)
						{
							// cicla sui nodi KeySegment e per ognuno di essi
							// cerca il node Foreign e Primary
							pKeySegmentNode  = pForeignKeysNode->GetFirstChild();
							while (pKeySegmentNode)
							{
								pForeignNode = pKeySegmentNode->GetChildByName(_T("Foreign"));
								pPrimaryNode = pKeySegmentNode->GetChildByName(_T("Primary"));
								if (pForeignNode && 
									pPrimaryNode &&
									pForeignNode->GetText(sForeignKey) &&
									pPrimaryNode->GetText(sPrimaryKey))
								{
									pSingleRef = NULL;
									// cerca se esiste già l'elemento
									for (int i = 0; i < pExtRefs->m_arExtRefs.GetSize(); i++)
									{
										pSingleRef = (CTableSingleExtRef*)pExtRefs->m_arExtRefs.GetAt(i);
										// Esiste già in array?
										if (pSingleRef->IsEqual(sForeignKey, sForeignTable, sPrimaryKey, sExtDocNS, sExpression))
											break;
										pSingleRef = NULL;//annullo puntatore per controllo all'uscita
									}
									if (!pSingleRef)
									{
										pSingleRef = new CTableSingleExtRef(sForeignKey, sForeignTable, sPrimaryKey, sExtDocNS, sExpression);
										pExtRefs->m_arExtRefs.Add(pSingleRef);
									}
								}
								pKeySegmentNode = pForeignKeysNode->GetNextChild();
							}
						}
					}
				}
				pExtRefNode = pExtRefsNode->GetNextChild();
			}
			// avendo trovato il nodo DBT esce
			break;
		}
		pDBTNode = aXML.GetNextRootChild();
	}
}

 // loads xternal refferences to a specific table
void CHelperExternalReferences::LoadExternalReferencesToTable(CTableDBT* pDBT, CTableExtRefs** ppExtRefs, CString tableName)
{
	// maiuscolo nome tabella per mappa
	CString sUpperTableName = pDBT->m_sTableName;
	sUpperTableName.MakeUpper();
	// se non esiste il file ExternalReferences.xml esce
	CString sExtRefsXmlFileName = pDBT->m_sDbtsxmlPath + SLASH_CHAR + _T("ExternalReferences.xml");
	if (!ExistFile(sExtRefsXmlFileName))
		return;

	// se non esiste ancora l'oggetto CTableExtRefs lo crea
	if (!*ppExtRefs)
	{
		*ppExtRefs = new CTableExtRefs(pDBT->m_sTableName);
		(*ppExtRefs)->m_arExtRefs.SetCompareFunction(::CompareDBTExtRef);
		m_arExternalReferences.Add(*ppExtRefs);//aggiunge in array degli external references
		m_mapTableToExtRefs.SetAt(sUpperTableName, *ppExtRefs);//aggiunge il TableName nella mappa per ricerche successive
	}

	CTableExtRefs* pExtRefs = *ppExtRefs;
	CString sDBTNS;
	CString sExtDocNS;
	CString sForeignTable;
	CString sForeignKey;
	CString sPrimaryKey;
	CString sExpression;
	CString sUpperDocNS;
	CXMLNode* pExtRefsNode;
	CXMLNode* pExtRefNode;
	CXMLNode* pExpressionNode;
	CXMLNode* pForeignKeysNode;
	CXMLNode* pKeySegmentNode;
	CXMLNode* pForeignNode;
	CXMLNode* pPrimaryNode;
	CTableDBT* pDBTExt;
	CTableSingleExtRef* pSingleRef;

	CLocalizableXMLDocument aXML(CTBNamespace(pDBT->m_sModuleNS), AfxGetPathFinder());
	aXML.LoadXMLFile(sExtRefsXmlFileName);//carica il file dbts.xml
	CXMLNode* pDBTNode = aXML.GetFirstRootChild(); //cerca il primo nodo DBT
	while (pDBTNode)
	{
		//cerca l'attributo namespace del DBT
		if (pDBTNode->GetAttribute(_T("namespace"), sDBTNS) && sDBTNS.CompareNoCase(pDBT->m_sDBTNS) == 0)
		{
			pExtRefs->m_sCurrDocNS = pDBT->m_sDocNS;
			pExtRefsNode = pDBTNode->GetChildByName(_T("ExternalReferences"));
			pExtRefNode = pExtRefsNode->GetFirstChild();
			while (pExtRefNode)
			{
				if (pExtRefNode->GetAttribute(_T("namespace"), sExtDocNS))//cerca l'attributo namespace dell'external reference
				{
					sForeignTable.Empty();
					// maiuscolo nsdoc per mappa
					sUpperDocNS = sExtDocNS;
					sUpperDocNS.MakeUpper();
					// cerca la tabella external con il namespace di documento nel dbts.xml 
					pDBTExt = NULL;
					if (m_mapNSDocToDbts.Lookup(sUpperDocNS, (CObject*&)pDBTExt))
						sForeignTable = pDBTExt->m_sTableName;
					/* //@@BEFOREMAPTODELETE
					for (int i = 0; i < m_arDBTs.GetSize(); i++)
					{
					pDBTExt = (CTableDBT*)m_arDBTs.GetAt(i);
					if (pDBTExt->m_sDocNS.CompareNoCase(sExtDocNS) == 0)
					{
					sForeignTable = pDBTExt->m_sTableName;
					break;
					}
					}
					*/
					// se non ha trovato la tabella external è inutile andare avanti
					if (!sForeignTable.IsEmpty() && (sForeignTable.CompareNoCase(tableName)==0)	)
					{
						sExpression.Empty();
						// cerca il nodo Keys
						pExpressionNode = pExtRefNode->GetChildByName(_T("Expression"));
						if (pExpressionNode)
							pExpressionNode->GetText(sExpression);
						// cerca il nodo Keys
						pForeignKeysNode = pExtRefNode->GetChildByName(_T("Keys"));
						if (pForeignKeysNode)
						{
							// cicla sui nodi KeySegment e per ognuno di essi
							// cerca il node Foreign e Primary
							pKeySegmentNode = pForeignKeysNode->GetFirstChild();
							while (pKeySegmentNode)
							{
								pForeignNode = pKeySegmentNode->GetChildByName(_T("Foreign"));
								pPrimaryNode = pKeySegmentNode->GetChildByName(_T("Primary"));
								if (pForeignNode &&
									pPrimaryNode &&
									pForeignNode->GetText(sForeignKey) &&
									pPrimaryNode->GetText(sPrimaryKey))
								{
									pSingleRef = NULL;
									// cerca se esiste già l'elemento
									for (int i = 0; i < pExtRefs->m_arExtRefs.GetSize(); i++)
									{
										pSingleRef = (CTableSingleExtRef*)pExtRefs->m_arExtRefs.GetAt(i);
										// Esiste già in array?
										if (pSingleRef->IsEqual(sForeignKey, sForeignTable, sPrimaryKey, sExtDocNS, sExpression))
											break;
										pSingleRef = NULL;//annullo puntatore per controllo all'uscita
									}
									if (!pSingleRef)
									{
										pSingleRef = new CTableSingleExtRef(sForeignKey, sForeignTable, sPrimaryKey, sExtDocNS, sExpression);
										pExtRefs->m_arExtRefs.Add(pSingleRef);
									}
								}
								pKeySegmentNode = pForeignKeysNode->GetNextChild();
							}
						}
					}
				}
				pExtRefNode = pExtRefsNode->GetNextChild();
			}
			// avendo trovato il nodo DBT esce
			break;
		}
		pDBTNode = aXML.GetNextRootChild();
	}
}

// external references di una tabella già caricata?
//-----------------------------------------------------------------------------
BOOL CHelperExternalReferences::IsTableAlreadyLoaded(const CString& sTableName)
{
	BOOL bLoaded = FALSE;

	CTableExtRefs* pExtRefs = NULL;
	// maiuscolo nome tabella per mappa
	CString sUpperTableName = sTableName;
	sUpperTableName.MakeUpper();
	
/* // @@BEFOREMAPTODELETE
	for (int i = 0; !bLoaded && i < m_arExternalReferences.GetSize(); i++)
	{
		pExtRefs = (CTableExtRefs*)m_arExternalReferences.GetAt(i);
		bLoaded |= (pExtRefs->m_sTableName.CompareNoCase(sTableName) == 0);
	}
*/
	bLoaded = m_mapTableToExtRefs.Lookup(sUpperTableName, (CObject*&)pExtRefs);

	return bLoaded;
}

// setta il puntatore di un esistente helpersqlcatalog
//-----------------------------------------------------------------------------
void CHelperExternalReferences::SetHelperSqlCatalog(CHelperSqlCatalog* pHelperSqlCatalog)
{
	ASSERT(m_pHelperSqlCatalog == NULL);

	if (!pHelperSqlCatalog)
		return;

	m_pHelperSqlCatalog = pHelperSqlCatalog;
}

// crea e carica, se non esistente, l'oggetto helpersqlcatalog
//-----------------------------------------------------------------------------
void CHelperExternalReferences::LoadHelperSqlCatalog()
{
	// è caricato il catalog?
	if (!m_pHelperSqlCatalog)
	{
		CWaitCursor wc;

		m_pHelperSqlCatalog = new CHelperSqlCatalog();
		m_pHelperSqlCatalog->Load();
		m_bOwnedHelperSqlCatalog = TRUE;
	}
}

//-----------------------------------------------------------------------------
CHelperExternalReferences::CTableExtRefs* CHelperExternalReferences::GetTableExtRefs(const CString& sTableName)
{
	CWaitCursor wc;
	if (!LoadByTableName(sTableName))
		return NULL;

	CTableExtRefs* pER = NULL;

	CString sUpperTableName = sTableName;
	sUpperTableName.MakeUpper();
	
	if (!m_mapTableToExtRefs.Lookup(sUpperTableName, (CObject*&)pER))
		return NULL;

	if (pER->m_arExtRefs.GetCount() == 0)
		return NULL;

	return pER;
}


//-----------------------------------------------------------------------------
Array* CHelperExternalReferences::GetExtRefsToTable(const CString& sTableName)
{
	CWaitCursor wc;

	LoadDBTs();
	Array* refsArr = new Array();

	for (int k = 0;k < m_arDBTs.GetCount();k++)
	{
		CTableExtRefs* ppExtRefs = new CTableExtRefs(((CTableDBT*)m_arDBTs.GetAt(k))->m_sTableName);

		LoadExternalReferencesToTable((CTableDBT*)m_arDBTs.GetAt(k), &ppExtRefs, sTableName);

		BOOL found = FALSE;
		if (ppExtRefs->m_arExtRefs.GetCount()>0)
		{
			for (int i = 0;i < refsArr->GetCount();i++)
			{
				CTableExtRefs* refs = (CTableExtRefs*)refsArr->GetAt(i);
				if (refs->m_sTableName.CompareNoCase(ppExtRefs->m_sTableName) == 0 && refs->m_sCurrDocNS.CompareNoCase(ppExtRefs->m_sCurrDocNS) == 0)
					found = TRUE;
			} 
			
			if (!found)
				refsArr->Add(ppExtRefs);
		}
	}

	return refsArr;
}