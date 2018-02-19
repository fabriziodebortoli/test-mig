#include "stdafx.h"

#include "ThreadContext.h"
#include "TBResourcesMap.h"
#include "PathFinder.h"
#include "FileSystemFunctions.h"
#include "Chars.h"
const static TCHAR szKeySep = _T(';');



//=============================================================================
//								Global Methods
//=============================================================================
//-----------------------------------------------------------------------------
CTBResourcesMap* AFXAPI AfxGetTBResourcesMap()
{ 
	return AfxGetApplicationContext()->GetTbResourcesMap();
}              

//=============================================================================
//								CTBAcceleratorTable
//==================================================================================
class CTBAcceleratorTable : public CObject
{
	friend class CTBResourcesMap;

private:
	int		m_nrOfEntries;
	LPACCEL	m_pAccelTable;

public:
	CTBAcceleratorTable (LPACCEL lpAccel, const int& nrEntries);
	~CTBAcceleratorTable();
};

//-----------------------------------------------------------------------------
CTBAcceleratorTable::CTBAcceleratorTable (LPACCEL lpAccel, const int& nrEntries)
	:
	m_pAccelTable (lpAccel),
	m_nrOfEntries (nrEntries)
{
}

//-----------------------------------------------------------------------------
CTBAcceleratorTable::~CTBAcceleratorTable ()
{
	// TODOBRUNA (ELEMENTI)
	delete m_pAccelTable;
}

//=============================================================================
//								CTBResourcesMap
//=============================================================================

// ranges allowed starts from the end of normal TaskBuilder resource numbers
//-----------------------------------------------------------------------------
CTBResourcesMap::CTBResourcesMap ()
	:
	m_nLastTbResource (MinTbResource),
	m_nLastTbControl  (MinTbControl),
	m_nLastTbCommand  (MinTbCommand)
{
	InitializeMap ();
}

//-----------------------------------------------------------------------------
CTBResourcesMap::~CTBResourcesMap ()
{
	POSITION pos = m_DynamicAcceleratorTables.GetStartPosition();
	while (pos)
	{
		CObject* pObject;
		CTBAcceleratorTable* pTable;
		CString sKey;
		m_DynamicAcceleratorTables.GetNextAssoc(pos, sKey, pObject);
		if (!pObject)
			continue;
		
		pTable = (CTBAcceleratorTable*) pObject;
		delete pTable;
	}
}

//-----------------------------------------------------------------------------
void CTBResourcesMap::InitializeMap()
{
	TB_LOCK_FOR_WRITE();
	//queste macro non devono avere un id dinamico, ance se sono usate dentro a file json
	//per questo faccio in modo che la funzione che restituisce l'id dinamico ritorni invece quello di sistema, statico

	ADD_FIXED_CTRL(IDC_STATIC_AREA);
	ADD_FIXED_CTRL(IDC_STATIC_AREA_2);
	ADD_FIXED_CTRL(IDC_STATIC_AREA_3);


	ADD_FIXED_CTRL(IDC_STATIC_1);
	ADD_FIXED_CTRL(IDC_STATIC_2);
	ADD_FIXED_CTRL(IDC_STATIC_3);
	ADD_FIXED_CTRL(IDC_STATIC_4);
	ADD_FIXED_CTRL(IDC_STATIC_5);
	ADD_FIXED_CTRL(IDC_STATIC_6);
	ADD_FIXED_CTRL(IDC_STATIC_7);
	ADD_FIXED_CTRL(IDC_STATIC_8);
	ADD_FIXED_CTRL(IDC_STATIC_9);
	ADD_FIXED_CTRL(IDC_STATIC_10);
	ADD_FIXED_CTRL(IDC_STATIC_11);
	ADD_FIXED_CTRL(IDC_STATIC_12);
	ADD_FIXED_CTRL(IDC_STATIC_13);
	ADD_FIXED_CTRL(IDC_STATIC_14);
	ADD_FIXED_CTRL(IDC_STATIC_15);
	ADD_FIXED_CTRL(IDC_STATIC_16);
	ADD_FIXED_CTRL(IDC_STATIC_17);
	ADD_FIXED_CTRL(IDC_STATIC_18);
	ADD_FIXED_CTRL(IDC_STATIC_19);
	ADD_FIXED_CTRL(IDC_STATIC_20);
	ADD_FIXED_CTRL(IDC_STATIC_21);
	ADD_FIXED_CTRL(IDC_STATIC_22);
	ADD_FIXED_CTRL(IDC_STATIC_23);
	ADD_FIXED_CTRL(IDC_STATIC_24);
	ADD_FIXED_CTRL(IDC_STATIC_25);
	ADD_FIXED_CTRL(IDC_STATIC_26);
	ADD_FIXED_CTRL(IDC_STATIC_27);
	ADD_FIXED_CTRL(IDC_STATIC_28);
	ADD_FIXED_CTRL(IDC_STATIC_29);

	ADD_FIXED_CTRL(IDOK);
	ADD_FIXED_CTRL(IDCANCEL);
	ADD_FIXED_CTRL(IDABORT);
	ADD_FIXED_CTRL(IDRETRY);
	ADD_FIXED_CTRL(IDIGNORE);
	ADD_FIXED_CTRL(IDYES);
	ADD_FIXED_CTRL(IDNO);
	ADD_FIXED_CTRL(IDCLOSE);
	ADD_FIXED_CTRL(IDHELP);
	ADD_FIXED_CTRL(IDTRYAGAIN);
	ADD_FIXED_CTRL(IDCONTINUE);
	ADD_FIXED_CTRL(IDTIMEOUT);
	ADD_FIXED_CTRL(IDC_STATIC);
	ADD_FIXED_CTRL(ID_FILE_NEW);
	ADD_FIXED_CTRL(ID_FILE_OPEN);
	ADD_FIXED_CTRL(ID_FILE_CLOSE);
	ADD_FIXED_CTRL(ID_FILE_SAVE);
	ADD_FIXED_CTRL(ID_FILE_SAVE_AS);
	ADD_FIXED_CTRL(ID_FILE_PAGE_SETUP);
	ADD_FIXED_CTRL(ID_FILE_PRINT_SETUP);
	ADD_FIXED_CTRL(ID_FILE_PRINT);
	ADD_FIXED_CTRL(ID_FILE_PRINT_DIRECT);
	ADD_FIXED_CTRL(ID_FILE_PRINT_PREVIEW);
	ADD_FIXED_CTRL(ID_FILE_UPDATE);
	ADD_FIXED_CTRL(ID_FILE_SAVE_COPY_AS);
	ADD_FIXED_CTRL(ID_FILE_SEND_MAIL);
	ADD_FIXED_CTRL(ID_FILE_NEW_FRAME);
	ADD_FIXED_CTRL(ID_FILE_MRU_FIRST);
	ADD_FIXED_CTRL(ID_FILE_MRU_FILE1);
	ADD_FIXED_CTRL(ID_FILE_MRU_FILE2);
	ADD_FIXED_CTRL(ID_FILE_MRU_FILE3);
	ADD_FIXED_CTRL(ID_FILE_MRU_FILE4);
	ADD_FIXED_CTRL(ID_FILE_MRU_FILE5);
	ADD_FIXED_CTRL(ID_FILE_MRU_FILE6);
	ADD_FIXED_CTRL(ID_FILE_MRU_FILE7);
	ADD_FIXED_CTRL(ID_FILE_MRU_FILE8);
	ADD_FIXED_CTRL(ID_FILE_MRU_FILE9);
	ADD_FIXED_CTRL(ID_FILE_MRU_FILE10);
	ADD_FIXED_CTRL(ID_FILE_MRU_FILE11);
	ADD_FIXED_CTRL(ID_FILE_MRU_FILE12);
	ADD_FIXED_CTRL(ID_FILE_MRU_FILE13);
	ADD_FIXED_CTRL(ID_FILE_MRU_FILE14);
	ADD_FIXED_CTRL(ID_FILE_MRU_FILE15);
	ADD_FIXED_CTRL(ID_FILE_MRU_FILE16);
	ADD_FIXED_CTRL(ID_FILE_MRU_LAST);
	ADD_FIXED_CTRL(ID_EDIT_CLEAR);
	ADD_FIXED_CTRL(ID_EDIT_CLEAR_ALL);
	ADD_FIXED_CTRL(ID_EDIT_COPY);
	ADD_FIXED_CTRL(ID_EDIT_CUT);
	ADD_FIXED_CTRL(ID_EDIT_FIND);
	ADD_FIXED_CTRL(ID_EDIT_PASTE);
	ADD_FIXED_CTRL(ID_EDIT_PASTE_LINK);
	ADD_FIXED_CTRL(ID_EDIT_PASTE_SPECIAL);
	ADD_FIXED_CTRL(ID_EDIT_REPEAT);
	ADD_FIXED_CTRL(ID_EDIT_REPLACE);
	ADD_FIXED_CTRL(ID_EDIT_SELECT_ALL);
	ADD_FIXED_CTRL(ID_EDIT_UNDO);
	ADD_FIXED_CTRL(ID_EDIT_REDO);
	ADD_FIXED_CTRL(ID_WINDOW_NEW);
	ADD_FIXED_CTRL(ID_WINDOW_ARRANGE);
	ADD_FIXED_CTRL(ID_WINDOW_CASCADE);
	ADD_FIXED_CTRL(ID_WINDOW_TILE_HORZ);
	ADD_FIXED_CTRL(ID_WINDOW_TILE_VERT);
	ADD_FIXED_CTRL(ID_WINDOW_SPLIT);
	ADD_FIXED_CTRL(AFX_IDM_WINDOW_FIRST);
	ADD_FIXED_CTRL(AFX_IDM_WINDOW_LAST);
	ADD_FIXED_CTRL(AFX_IDM_FIRST_MDICHILD);
	ADD_FIXED_CTRL(ID_APP_ABOUT);
	ADD_FIXED_CTRL(ID_APP_EXIT);
	ADD_FIXED_CTRL(ID_HELP_INDEX);
	ADD_FIXED_CTRL(ID_HELP_FINDER);
	ADD_FIXED_CTRL(ID_HELP_USING);
	ADD_FIXED_CTRL(ID_CONTEXT_HELP);
	ADD_FIXED_CTRL(ID_HELP);
	ADD_FIXED_CTRL(ID_DEFAULT_HELP);
	ADD_FIXED_CTRL(ID_NEXT_PANE);
	ADD_FIXED_CTRL(ID_PREV_PANE);
	ADD_FIXED_CTRL(ID_FORMAT_FONT);
	ADD_FIXED_CTRL(AFX_ID_PREVIEW_CLOSE);
	ADD_FIXED_CTRL(AFX_ID_PREVIEW_NUMPAGE);
	ADD_FIXED_CTRL(AFX_ID_PREVIEW_NEXT);
	ADD_FIXED_CTRL(AFX_ID_PREVIEW_PREV);
	ADD_FIXED_CTRL(AFX_ID_PREVIEW_PRINT);
	ADD_FIXED_CTRL(AFX_ID_PREVIEW_ZOOMIN);
	ADD_FIXED_CTRL(AFX_ID_PREVIEW_ZOOMOUT);
	ADD_FIXED_CTRL(ID_VIEW_TOOLBAR);
	ADD_FIXED_CTRL(ID_VIEW_STATUS_BAR);
	ADD_FIXED_CTRL(ID_VIEW_REBAR);
	ADD_FIXED_CTRL(ID_VIEW_AUTOARRANGE);
	ADD_FIXED_CTRL(ID_VIEW_SMALLICON);
	ADD_FIXED_CTRL(ID_VIEW_LARGEICON);
	ADD_FIXED_CTRL(ID_VIEW_LIST);
	ADD_FIXED_CTRL(ID_VIEW_DETAILS);
	ADD_FIXED_CTRL(ID_VIEW_LINEUP);
	ADD_FIXED_CTRL(ID_VIEW_BYNAME);
	ADD_FIXED_CTRL(AFX_ID_VIEW_MINIMUM);
	ADD_FIXED_CTRL(AFX_ID_VIEW_MAXIMUM);
	ADD_FIXED_CTRL(ID_RECORD_FIRST);
	ADD_FIXED_CTRL(ID_RECORD_LAST);
	ADD_FIXED_CTRL(ID_RECORD_NEXT);
	ADD_FIXED_CTRL(ID_RECORD_PREV);
	ADD_FIXED_CTRL(ID_SEPARATOR);
}

// locked from caller
//-----------------------------------------------------------------------------
void CTBResourcesMap::AddFixedResource (const TbResourceType aType, const CString& sName, UINT nID)
{
	GetCollection(aType).SetAt(sName, nID); 
	CJsonResource r;
	r.m_strName = sName;
	r.m_dwId = nID;
	GetReverseCollection(aType).SetAt(nID, r);
	m_TbFixedResources.SetAt(sName, nID);
}
//-----------------------------------------------------------------------------
BOOL CTBResourcesMap::IsFixedResource(const CString& sName)
{
	TB_LOCK_FOR_READ();
	DWORD nID;
	return m_TbFixedResources.Lookup(sName, nID);
}
//-----------------------------------------------------------------------------
UINT CTBResourcesMap::GetExistingTbResourceID(LPCTSTR sPartialNamespace, const TbResourceType aType)
{
	Map& coll = GetCollection(aType);

	DWORD pValue = NULL;
	{ //scope di lock in lettura, non togliere
		TB_LOCK_FOR_READ();
		if (coll.Lookup(sPartialNamespace, pValue) && pValue)
			return  pValue;
	}
	return 0;
}
//-----------------------------------------------------------------------------
UINT CTBResourcesMap::GetTbResourceID(LPCTSTR sPartialNamespace, const TbResourceType aType, int nCount /*= 1*/, LPCTSTR sContext /*= NULL*/)
{
	if (!sPartialNamespace || !*sPartialNamespace)
		return 0;

	//CString sKey = sPartialNamespace;
	//if (sContext)
	//	sKey.Append(sContext);

	Map& coll = GetCollection(aType);
	DWORD pValue = NULL;
	{ //scope di lock in lettura, non togliere
		TB_LOCK_FOR_READ();
		if (coll.Lookup(sPartialNamespace, pValue) && pValue)
			return  pValue;
	}
	TB_LOCK_FOR_WRITE();

	// someone could have writtend the number in the meanwhile as 
	// we avoided to use read lock before
	if (coll.Lookup(sPartialNamespace, pValue) && pValue)
	{
		return pValue;
	}

	DWORD wId = GetNextTbResourceID(aType);
	if (IsOutOfRange(wId, aType))
	{
		ASSERT(FALSE);
		CString sMsg = GetOutOfRangeMessage(sPartialNamespace, wId, aType);
		TRACE1("\nCTBResourcesMap::GetTbResourceID: %s", sMsg);
		AfxGetDiagnostic()->Add(sMsg, CDiagnostic::Warning);
	}

	coll.SetAt(sPartialNamespace, wId);
	CJsonResource r;
	r.m_strName = sPartialNamespace;
	r.m_strContext = sContext;
	r.m_dwId = wId;
	GetReverseCollection(aType).SetAt(wId, r);

	for (int i = 1; i <= nCount; i++)
	{
		int wNextId = GetNextTbResourceID(aType);
		if (IsOutOfRange(wNextId, aType))
		{
			ASSERT(FALSE);
			CString sMsg = GetOutOfRangeMessage(sPartialNamespace, wNextId, aType);
			TRACE1("\nCTBResourcesMap::GetTbResourceID: %s", sMsg);
			AfxGetDiagnostic()->Add(sMsg, CDiagnostic::Warning);
		}
	}
	return wId;
}

//-----------------------------------------------------------------------------
CStringEntry CTBResourcesMap::GetString(UINT nID)
{
	TB_LOCK_FOR_READ ()

	CStringEntry entry;
	m_StringTable.Lookup(nID, entry);
	
	return entry;
}

//-----------------------------------------------------------------------------
CJsonResource CTBResourcesMap::DecodeID(const TbResourceType aType, UINT nID)
{
	TB_LOCK_FOR_READ()
	ReverseMap& coll = GetReverseCollection(aType);
	CJsonResource sVal;
	if (coll.Lookup(nID, sVal))
		return sVal;
	
	return sVal;
}

// Forbidden Map usage does not need locks as:
// - map is written on application standard objects loading process (InitInstance)
// - map never changes its composition during application lifetime
//-----------------------------------------------------------------------------
void CTBResourcesMap::AddForbiddenID (TbResourceType aType, UINT nValue)
{
	BOOL bAlreadyAssignedDynamically = FALSE;

	//read lock context
	{
		TB_LOCK_FOR_READ()

		switch (aType)
		{
		case TbCommands:
//			if (nValue <= m_nLastTbCommand)
//				bAlreadyAssignedDynamically = TRUE;
//			break;
		case TbControls:
			if (nValue <= m_nLastTbControl)
				bAlreadyAssignedDynamically = TRUE;
			break;
		default:
			if (nValue <= m_nLastTbResource)
				bAlreadyAssignedDynamically = TRUE;
			break;	
		}
	}

	if (bAlreadyAssignedDynamically)
	{
//		//ASSERT(FALSE); TODOBRUNA
		TRACE2 ("\nRequested value %d for resource %d not assigned!!! Resource already assigned dynamically !!!!!", nValue, aType);
		return;
	}

	LPVOID pValue = NULL;
	CString sKey;
	sKey.Format(_T("%d"), nValue);

	if (!m_ForbiddenResources.Lookup(sKey, (LPVOID) pValue)) 
		m_ForbiddenResources.SetAt(sKey, (LPVOID) pValue); 
}

//-----------------------------------------------------------------------------
UINT CTBResourcesMap::AddString(const CString sId, LPCTSTR lpszString, LPCTSTR lpszFile)
{
	UINT nId = GetTbResourceID(sId, TbCommands);
	if (GetString(nId).lpszString[0])
		return nId;
	TB_LOCK_FOR_WRITE();
	CStringEntry entry;
	entry.lpszFile = lpszFile;
	entry.lpszString = lpszString;
	m_StringTable.SetAt(nId, entry);
	return nId;
}

//-----------------------------------------------------------------------------
void CTBResourcesMap::AddAcceleratorTable(const CString& sName, LPACCEL accelTable, const int& nrOfEntries)
{
	TB_LOCK_FOR_WRITE()
	CString s = sName;
	m_DynamicAcceleratorTables.SetAt(s.MakeLower(), new CTBAcceleratorTable(accelTable, nrOfEntries));
}

//-----------------------------------------------------------------------------
void CTBResourcesMap::GetAcceleratorTable(UINT nID, LPACCEL accelTable, int& nEntries)
{
	CJsonResource sResource = DecodeID(TbResources, nID);
	if (!sResource.m_strName.IsEmpty())
		GetAcceleratorTable(sResource.m_strName, accelTable, nEntries);
}

//-----------------------------------------------------------------------------
void CTBResourcesMap::GetAcceleratorTable(const CString& sTableName, LPACCEL accelTable, int& nEntries)
{
	TB_LOCK_FOR_READ ()

	CString sIDName;
	sIDName = sIDName.MakeLower(); 
	CString sString;
	CObject* pObject;
	m_DynamicAcceleratorTables.Lookup(sTableName, pObject);
	
	if (!pObject || !pObject->IsKindOf(RUNTIME_CLASS(CTBAcceleratorTable)))
		return ;

	accelTable = ((CTBAcceleratorTable*) pObject)->m_pAccelTable;
	nEntries = ((CTBAcceleratorTable*) pObject)->m_nrOfEntries;
}

// write lock on the map fixed by the caller
//-----------------------------------------------------------------------------
UINT CTBResourcesMap::GetNextTbResourceID (const TbResourceType aType, UINT nValue /*0*/)
{
	// value requested
	if (nValue)
		return nValue;

	LPVOID pValue = NULL;

	// when i take a new number I have to skip forbidden resources
	DWORD nID = 0;
	CString sKey;

	do 
	{
		switch (aType)
		{
		case TbCommands:
		//	m_nLastTbCommand++;	//conflitto di numerazione fra IDC_ e ID_, (es. bottone di dialog e bottone di toolbar), devo usare la stessa mappa
		//	nID = m_nLastTbCommand;
		//	break;
		case TbControls:
			m_nLastTbControl++;
			nID = m_nLastTbControl;
			break;
		default:
			m_nLastTbResource++;
			nID = m_nLastTbResource;
			break;	
		}

		sKey.Format(_T("%d"), nID);
	}
	while (m_ForbiddenResources.Lookup(sKey, pValue) && !IsOutOfRange(nID, aType));

	return nID;
}

//-----------------------------------------------------------------------------
Map& CTBResourcesMap::GetCollection(const TbResourceType aType)
{
	switch (aType)
	{
	case TbCommands:
//		return m_TbCommands;//conflitto di numerazione fra IDC_ e ID_, (es. bottone di dialog e bottone di toolbar), devo usare la stessa mappa
	case TbControls:
		return m_TbControls;
	default:
		return m_TbResources;
	}
}

//-----------------------------------------------------------------------------
ReverseMap& CTBResourcesMap::GetReverseCollection(const TbResourceType aType)
{
	switch (aType)
	{
	case TbCommands:
//		return m_ReversedTbCommands;//conflitto di numerazione fra IDC_ e ID_, (es. bottone di dialog e bottone di toolbar), devo usare la stessa mappa
	case TbControls:
		return m_ReversedTbControls;
	default:
		return m_ReversedTbResources;
	}
}


//-----------------------------------------------------------------------------
/*static*/ BOOL CTBResourcesMap::IsOutOfRange (const DWORD& nResourceID, const TbResourceType aType)
{
	switch (aType)
	{
	case TbCommands:
//		return nResourceID > MaxTbCommand;//conflitto di numerazione fra IDC_ e ID_, (es. bottone di dialog e bottone di toolbar), devo usare la stessa mappa
	case TbControls:
		return nResourceID > MaxTbControl;
	default:
		return nResourceID > MaxTbResource;
	}
}

//-----------------------------------------------------------------------------
CString	CTBResourcesMap::GetOutOfRangeMessage (const CString& sPartialNamespace, const DWORD& nResourceID, const TbResourceType aType)
{
	CString sMessage = _T("Application ha reached the maximum number of the resources ID allowed for ");
	CString sType;
	switch (aType)
	{
	case TbCommands:
		sType = _T(" MFC Commands (IDM, IDMM)");
		break;
	case TbControls:
		sType = _T(" MFC Controls (IDC)");
		break;
	default:
		sType = _T(" MFC Resources (IDD, IDR, ID, IDS)");
		break;	
	}
	sMessage += sType;
	TCHAR szBuffer [255];

	wsprintf(szBuffer, _T("\r\n The last number assigned to the resource %s is %d  and it is out of range. No more resources available."), sPartialNamespace, sType);
	sMessage += szBuffer;
	sMessage += _T("\r\nPlease LogOff and restart.");
	
	return sMessage;
}


//-----------------------------------------------------------------------------
void CJsonResource::GetInfo(CString& sFile, CTBNamespace& moduleNamespace) const
{
	int nPos = 0;
	//posso avere tre token (il json è nella cartella di modulo) app.mod.category
	//oppure quattro (il json è nella cartella di documento) app.mod.doc.category
	CString type = m_strContext.Tokenize(_T("."), nPos);
	bool modOwner = false, docOwner = type ==_T("D"); //posso trovare 'M' (modulo) o 'D' (documento), o il nome dell'applicazione
	if (!docOwner)
		modOwner = type ==_T("M");
	BOOL unknownOwner = !docOwner && !modOwner;
	CString app = unknownOwner ? type : m_strContext.Tokenize(_T("."), nPos);
	CString mod = nPos == -1 ? _T("") : m_strContext.Tokenize(_T("."), nPos);
	CString category = nPos == -1 ? _T("") : m_strContext.Tokenize(_T("."), nPos);
	moduleNamespace = CTBNamespace(CTBNamespace::MODULE, app + _T('.') + mod);
	if (docOwner || unknownOwner) //se l'owner è il documento, oppure non lo so, lo cerco nella descrizione del documento stesso
	{
		sFile = AfxGetPathFinder()->GetModuleObjectsPath(moduleNamespace, CPathFinder::STANDARD);
		sFile += SLASH_CHAR + category + SLASH_CHAR + szJsonForms + SLASH_CHAR + m_strName + szTBJsonFileExt;
		if (docOwner || ExistFile(sFile))
			return;
	}
	//altrimenti in una sottocartella della jsonforms di modulo
	sFile = AfxGetPathFinder()->GetJsonFormPath(moduleNamespace, CPathFinder::PosType::STANDARD);
	if (!category.IsEmpty() && category != szJsonForms)
		sFile += SLASH_CHAR + category;
	sFile += SLASH_CHAR + m_strName + szTBJsonFileExt;
}
//-----------------------------------------------------------------------------
CString CJsonResource::GetFile()
{
	if (m_strFile.IsEmpty())
		GetInfo(m_strFile, m_OwnerNamespace);
	return m_strFile;
}

//-----------------------------------------------------------------------------
CTBNamespace CJsonResource::GetOwnerNamespace()
{
	if (m_OwnerNamespace.IsEmpty())
		GetInfo(m_strFile, m_OwnerNamespace);
	return m_OwnerNamespace;
}

//-----------------------------------------------------------------------------
void CJsonResource::PopulateFromFile(const CString& sFile)
{
	CString sContext;
	CString sTempFolder = GetPath(sFile);
	CString sCategory;
	CString sApp, sMod;
	bool bDocument = sTempFolder.Find(szModuleObjects) != -1;

	if (bDocument)//se sono direttamente in una cartella jsonforms, allora è un json di documento
	{
		sContext += _T("D.");
		ASSERT(GetName(sTempFolder) == szJsonForms);
		sTempFolder = GetPath(sTempFolder);//cartella di documento
		sCategory = GetName(sTempFolder);
		sTempFolder = GetPath(sTempFolder);//ModuleObjects
		ASSERT(GetName(sTempFolder) == szModuleObjects);
		sTempFolder = GetPath(sTempFolder);//modulo
		sMod = GetName(sTempFolder);
		sTempFolder = GetPath(sTempFolder);//applicazione
		sApp = GetName(sTempFolder);
	}
	else
	{
		sContext += _T("M.");
		sCategory = GetName(sTempFolder);
		if (sCategory != szJsonForms)
			sTempFolder = GetPath(sTempFolder);//jsonforms
		ASSERT(GetName(sTempFolder) == szJsonForms);
		sTempFolder = GetPath(sTempFolder);//modulo
		sMod = GetName(sTempFolder);
		sTempFolder = GetPath(sTempFolder);//applicazione
		sApp = GetName(sTempFolder);
	}
	sContext += sApp + _T('.') + sMod + _T('.') + sCategory;
	m_strContext = sContext;
	m_strName = GetName(sFile);
	m_strFile = sFile;
	m_OwnerNamespace = CTBNamespace(CTBNamespace::MODULE, sApp + _T('.') + sMod);

}

//-----------------------------------------------------------------------------
void CJsonResource::SplitNamespace(CString sNamespace, CString& sResourceName, CString& sContext)
{
	int idx = sNamespace.ReverseFind(_T('.'));
	if (idx == -1)
	{
		sResourceName = sNamespace;
		return;
	}
	sResourceName = sNamespace.Mid(idx+1);
	sContext = sNamespace.Left(idx);
}