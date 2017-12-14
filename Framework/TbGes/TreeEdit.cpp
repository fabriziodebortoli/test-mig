#include "StdAfx.h"
#include <TbNameSolver\Templates.h>
#include <TbFrameworkImages\CommonImages.h>
#include <TbFrameworkImages\GeneralFunctions.h>
#include "TreeEdit.h"
#include "Dbt.h"
#include "EXTDOC.H"
#include "ExtDocView.h"
#include "BODYEDIT.H"
#include "SlaveViewContainer.h"



//=============================================================================
class DBTPtr : public TDisposablePtr<DBTSlaveBuffered>
{
	UINT* m_pnTreeObjectsHash;

public:
	DBTPtr() : m_pnTreeObjectsHash(NULL){}
	void SetTreeObjectsHash(UINT* pnTreeObjectsHash)
	{
		m_pnTreeObjectsHash = pnTreeObjectsHash;
	}
	virtual void OnDisposing(){ if (m_pnTreeObjectsHash) *m_pnTreeObjectsHash = 0; }
};

//=============================================================================
class RecPtr : public TDisposablePtr<SqlRecord>
{
	UINT* m_pnTreeObjectsHash;

public:
	RecPtr() : m_pnTreeObjectsHash(NULL){}
	void SetTreeObjectsHash(UINT* pnTreeObjectsHash)
	{
		m_pnTreeObjectsHash = pnTreeObjectsHash;
	}
	virtual void OnDisposing(){ if (m_pnTreeObjectsHash) *m_pnTreeObjectsHash = 0; }
};

//=============================================================================
class CTreeBag : public CObject
{
	UINT* m_pnTreeObjectsHash;
public:
	CTreeBag(DBTObject* pDBT, SqlRecord *pRecord, UINT* pnTreeObjectsHash)
		:
	m_pnTreeObjectsHash(pnTreeObjectsHash)
	{
		m_DBTPtr.Assign(pDBT); 
		m_DBTPtr.SetTreeObjectsHash(m_pnTreeObjectsHash);
		m_RecordPtr.Assign(pRecord); 
		m_RecordPtr.SetTreeObjectsHash(m_pnTreeObjectsHash);
	}
	
	DBTPtr m_DBTPtr;
	RecPtr m_RecordPtr;
};

//=============================================================================
class CNodeLevelInfo
{
public:
	CNodeLevelInfo()
		:
		m_nBodyIDC(NULL), 
		m_nRowViewIDC(NULL), 
		m_pBodyEdit(NULL),
		m_bUseBodyEdit(FALSE),
		m_nCurrentRow(-1),
		m_bActive(FALSE)
	  {
		  m_sDBTImage = szDBTImage;
		  m_sRecordImage = szRecordImage;
	  }

	CTBNamespace	m_nsDBT;
	UINT			m_nBodyIDC;
	UINT			m_nRowViewIDC;
	CBodyEdit*		m_pBodyEdit;
	BOOL			m_bUseBodyEdit;
	CString			m_sDBTImage;
	CString			m_sRecordImage;
	int				m_nCurrentRow;
	BOOL			m_bActive;
};

//-----------------------------------------------------------------------------
int CNodeLevelInfoArray::Compare(const void *arg1, const void *arg2) 
{
	CNodeLevelInfo* i1=*(CNodeLevelInfo **)arg1;
	CNodeLevelInfo* i2=*(CNodeLevelInfo **)arg2;

	if (i1->m_nsDBT == i2->m_nsDBT)
		return 0;
	
	DBTSlaveBuffered* pBuff1 = i1->m_pBodyEdit->GetDBT();
	DBTSlaveBuffered* pBuff2 = i2->m_pBodyEdit->GetDBT();
	if (!pBuff1 && !pBuff2)
		return 0;
	if (!pBuff1)
		return 1;
	if (!pBuff2)
		return -1;
	
	DBTObject* pObj = (DBTObject*)pBuff1->GetMaster();
	while (pObj)
	{
		if (pObj->GetNamespace() == i2->m_nsDBT)
			return 1;
		pObj = pObj->IsKindOf(RUNTIME_CLASS(DBTSlave)) ? (DBTObject*)((DBTSlave*)pObj)->GetMaster() : NULL;
	}
	pObj = (DBTObject*)pBuff2->GetMaster();
	while(pObj)
	{
		if (pObj->GetNamespace() == i1->m_nsDBT)
			return -1;
		pObj = pObj->IsKindOf(RUNTIME_CLASS(DBTSlave)) ? (DBTObject*)((DBTSlave*)pObj)->GetMaster() : NULL;
	};
	return i1->m_nsDBT.ToString().Compare(i2->m_nsDBT.ToString());
}

//=============================================================================
class ToggleBOOL
{
public:
	BOOL &m_b;
	ToggleBOOL(BOOL& b) : m_b(b)
	{
		m_b ++;
	}
	~ToggleBOOL()
	{
		m_b --;
	}
};

//=============================================================================
IMPLEMENT_DYNCREATE(CDBTTreeEdit, CTreeViewAdvCtrl)

//-----------------------------------------------------------------------------
BEGIN_MESSAGE_MAP(CDBTTreeEdit, CTreeViewAdvCtrl)
END_MESSAGE_MAP()

//-----------------------------------------------------------------------------
CDBTTreeEdit::CDBTTreeEdit()
	: 
	m_pDBT(NULL), 
	m_bUpdatingContent(0), 
	m_bUpdatingObjectMap(0),
	m_bSelecting(FALSE), 
	m_bCreatingStructure(0),
	m_nTreeStructureHash(0), 
	m_nTempTreeStructureHash(0), 
	m_nTreeContentHash(0), 
	m_nTempTreeContentHash(0),
	m_nTreeObjectsHash(0),
	m_nTempTreeObjectsHash(0),
	m_pActiveWnd(NULL),
	m_bInited(FALSE),
	m_bExpanded(FALSE),
	m_nKeyCounter(0), 
	m_nInitialSelectionLevel(-1),
	m_nInitialExpansionLevel(-1),
	m_bCanHide(TRUE),
	m_bCanResize(TRUE),
	m_nWidth(0)
{
}

//-----------------------------------------------------------------------------
CDBTTreeEdit::~CDBTTreeEdit(void)
{
	ClearObjectMap();
	for (int i=0; i < m_NodeInfoMap.GetCount(); i++)
		delete m_NodeInfoMap[i];
}

//-----------------------------------------------------------------------------
void CDBTTreeEdit::RefreshTree(BOOL bUpdateObjectMap, BOOL bUpdateTreeContent, BOOL bUpdateTreeStructure)
{
	m_nKeyCounter = 0;
	m_nTempTreeStructureHash = 0;
	m_nTempTreeContentHash = 0;
	m_nTempTreeObjectsHash = 0;
	//livelli crescenti di modifica:
	
	//sono cambiati solo i puntatori ai dbt (entro in Edit o vado in browse)
	if (bUpdateObjectMap)
	{
		m_bUpdatingObjectMap++;
		ClearObjectMap();
	}

	//è cambiato il contenuto 
	if (bUpdateTreeContent)
		m_bUpdatingContent++;

	//è cambiata la struttura dell'albero
	if (bUpdateTreeStructure)
	{
		m_bCreatingStructure++;
		ClearTree();
	}

	AddDBT(_T(""), m_pDBT);

	if (bUpdateObjectMap)
		m_bUpdatingObjectMap--;
	if (bUpdateTreeContent)
		m_bUpdatingContent--;
	if (bUpdateTreeStructure)
		m_bCreatingStructure--;
	
	m_nTreeObjectsHash = m_nTempTreeObjectsHash;
	m_nTreeContentHash = m_nTempTreeContentHash;
	m_nTreeStructureHash = m_nTempTreeStructureHash;
	
	const DBTObject* pRoot = m_pDBT;
	while(pRoot->IsKindOf(RUNTIME_CLASS(DBTSlave)) && ((DBTSlave*)pRoot)->GetMaster())
		pRoot = ((DBTSlave*)pRoot)->GetMaster();

	CString sDocumentKey = pRoot->GetRecord()->GetPrimaryKeyDescription();
	if (m_sDocumentKey != sDocumentKey)
	{
		if (m_nInitialExpansionLevel == -1)
		{
			ExpandAll();
			m_bExpanded = TRUE;
		}
		else
		{
			CollapseAll();
			ExpandLevels(m_nInitialExpansionLevel);
			m_bExpanded = FALSE;
		}
		m_sDocumentKey = sDocumentKey;
		if (m_nInitialSelectionLevel != -1)
		{
			int nLevel = m_nInitialSelectionLevel;
			while (nLevel >= m_nKeyCounter)
				nLevel--;
			m_sSelectedNode = cwsprintf(_T("%d"), nLevel);
			VERIFY(SetNodeAsSelected(m_sSelectedNode));
			Select(m_sSelectedNode);
		}
	}
	else
	{
		if (bUpdateObjectMap||bUpdateTreeContent||bUpdateTreeStructure)
		{
			if (!m_sSelectedNode.IsEmpty())
			{
				if (GetObjectFromMap(m_sSelectedNode) == NULL)
				{
					//non esiste piu' il nodo
					int nLevel = max(m_nInitialSelectionLevel, 0);
					while (nLevel >= m_nKeyCounter)
						nLevel--;
					m_sSelectedNode = cwsprintf(_T("%d"), nLevel);
				}

				VERIFY(SetNodeAsSelected(m_sSelectedNode));
				Select(m_sSelectedNode);
			}
		}
	}	
}

//-----------------------------------------------------------------------------
BOOL CDBTTreeEdit::IsDataModified()
{
	if (m_bCreatingStructure)//evito ricorsioni in caso di inserimento di nodi che scatenano change con delle UpdateDataView
		return FALSE;

	if (!m_bInited)
		DelayedInit();

	//ciclo sui dbt ma non li aggiungo, ne calcolo solo lo hash per vedere se sono cambiati
	m_nTempTreeStructureHash = 0;
	m_nTempTreeContentHash = 0;
	m_nTempTreeObjectsHash = 0;
	m_nKeyCounter = 0;

	AddDBT(_T(""), m_pDBT); 
	
	//devo rinfrescare se è cambiata la struttura dei dbt oppure il contenuto oppure il nodo attivo
	return m_nTempTreeStructureHash != m_nTreeStructureHash || m_nTreeContentHash != m_nTempTreeContentHash || m_nTreeObjectsHash != m_nTempTreeObjectsHash;
}

//-----------------------------------------------------------------------------
void CDBTTreeEdit::Attach(DBTSlaveBuffered* pDBT)			
{
	m_pDBT = pDBT; 
	AttachDocument(pDBT->GetDocument()); 
}

//-----------------------------------------------------------------------------
void CDBTTreeEdit::UpdateCtrlView()
{
	if (m_bCreatingStructure || m_bSelecting)
		return;

	BOOL bUpdateObjectMap = m_nTempTreeObjectsHash != m_nTreeObjectsHash;
	BOOL bUpdateTreeContent = m_nTempTreeContentHash != m_nTreeContentHash;
	BOOL bUpdateTreeStructure = m_nTempTreeStructureHash != m_nTreeStructureHash;
	RefreshTree(bUpdateObjectMap, bUpdateTreeContent, bUpdateTreeStructure);
}

//-----------------------------------------------------------------------------
void CDBTTreeEdit::UpdateCtrlStatus()
{
	CTreeBag *pCurrentBag = GetObjectFromMap(m_sSelectedNode);
	CBodyEdit* pEdit = pCurrentBag ? GetBodyEdit(pCurrentBag->m_DBTPtr) : NULL;
	EnableToolBarCommand(CMD_NEW, pEdit ? (pEdit->CanInsertRowByTreeEdit(FALSE) == TRUE) : FALSE);
	EnableToolBarCommand(CMD_DELETE, pEdit ? (pEdit->InternalCanDeleteRow() == TRUE) : FALSE);
}

//-----------------------------------------------------------------------------
void CDBTTreeEdit::SetDataReadOnly(BOOL bRO)
{
	m_pDBT->SetReadOnly(bRO, TRUE);
}

//-----------------------------------------------------------------------------
BOOL CDBTTreeEdit::ForceUpdateCtrlView(int)
{
	return FALSE;
}

//-----------------------------------------------------------------------------
CString CDBTTreeEdit::GetNewKey(CObject* pObj)
{
	return cwsprintf(_T("%i"), m_nKeyCounter++);
}

//----------------------------------------------------------------------------
void CDBTTreeEdit::SelectNode(DBTSlaveBuffered* pDBT, SqlRecord* pRecord)
{
	CString sKey = GetKeyFromMap(pDBT, pRecord);
	SetNodeAsSelected(sKey);
	UpdateCtrlStatus();
}

//----------------------------------------------------------------------------
void CDBTTreeEdit::AddToHash(const CString& strContent, const CString& sKey, int nAdditionalInfo, void* pObject)
{
	//calcolo lo hash degli oggetti
	m_nTempTreeObjectsHash = (m_nTempTreeObjectsHash<<5) + m_nTempTreeObjectsHash + ((int)pObject);	//copiato da CString

	//calcolo lo hash dei contenuti
	const TCHAR* pch = strContent;
	while( *pch != 0 )
	{
		m_nTempTreeContentHash = (m_nTempTreeContentHash<<5) + m_nTempTreeContentHash + (*pch);	//copiato da CString
		pch++;
	}
	m_nTempTreeContentHash = (m_nTempTreeContentHash<<5) + m_nTempTreeContentHash + nAdditionalInfo;	

	//calcolo lo hash della struttura usando le chiavi come proxy
	pch = sKey;
	while( *pch != 0 )
	{
		m_nTempTreeStructureHash = (m_nTempTreeStructureHash<<5) + m_nTempTreeStructureHash + (*pch);	//copiato da CString
		pch++;
	}
}

//-----------------------------------------------------------------------------
CString	CDBTTreeEdit::GetDBTImage (DBTSlaveBuffered* pDBTObject)
{
	return _T("");
}

//-----------------------------------------------------------------------------
CString	CDBTTreeEdit::GetRecordImage (DBTSlaveBuffered* pDBTObject, SqlRecord* pRecord)
{
	return _T("");
}

//-----------------------------------------------------------------------------
CString CDBTTreeEdit::AddRecord(const CString& sParentNode, DBTSlaveBuffered* pDBT, SqlRecord* pRec, CNodeLevelInfo* pInfo)
{
	CString sKey = GetNewKey(pRec);
	CString sDescri = pRec->GetRecordDescription();
	sDescri.Replace(_T("\n"), _T(" "));
	
	AddToHash(sDescri, sKey, pDBT->GetCurrentRow() == pRec, pRec);
	
	if (m_bUpdatingObjectMap)
		AddToMap(sKey, pDBT, pRec);

	if (m_bCreatingStructure)
	{
		CString s = GetSelectedNodeKey();
		CString sImage = GetRecordImage(pDBT, pRec);
		if (sImage.IsEmpty())
			sImage = pInfo ? pInfo->m_sRecordImage : szRecordImage;
		InsertChild(sParentNode, sDescri, sKey, sImage); 
		SetNodeAsSelected(s);
	}
	else if (m_bUpdatingContent)//se ho cambiato la struttura, il teto del nodo è già impostato
	{
		//anche se la chiave è creata ex novo, il criterio non cambia, 
		//e siccome la struttura non è cambiata la chiave prodotta per quel nodo sarà sempre la stessa
		SetUpdateTextNode(sKey, sDescri); 		
	}

	return sKey;
}

//-----------------------------------------------------------------------------
void CDBTTreeEdit::AddDBT(const CString& sParentNode, DBTSlaveBuffered* pDBT)
{
	CString sDBTKey = GetNewKey(pDBT);
	CNodeLevelInfo* pInfo = GetNodeLevelInfo(pDBT);
	CString sTitle = pDBT->GetTitle();
	
	AddToHash(sTitle, sDBTKey, pDBT->IsReadOnly(), pDBT);
	
	if (m_bUpdatingObjectMap)
		AddToMap(sDBTKey, pDBT, NULL);

	if (m_bCreatingStructure)
	{
		CString s = GetSelectedNodeKey();
		CString sImage = GetDBTImage(pDBT);
		if (sImage.IsEmpty())
			sImage = pInfo ? pInfo->m_sDBTImage : szDBTImage;

		InsertChild(sParentNode, sTitle, sDBTKey, sImage);
		SetNodeAsSelected(s);
	}
	
	if (m_bUpdatingContent)
	{
		//anche se la chiave è creata ex novo, il criterio non cambia, 
		//e siccome la struttura non è cambiata la chiave prodotta per quel nodo sarà sempre la stessa
		SetUpdateTextNode(sDBTKey, sTitle);
	}
	
	ASSERT(pInfo);
	if (pInfo->m_bUseBodyEdit)
		return;
	
	for (int i = 0; i < pDBT->GetSize(); i++)
	{
		SqlRecord* pRec = pDBT->GetRow(i);
		CString sKey = AddRecord(sDBTKey, pDBT, pRec, pInfo); 
		OnAfterAddRecord(sKey, pRec);
		
		CStringArray ar; 
		pDBT->GetSlavesDBTS(ar);
		for (int j = 0; j < ar.GetCount(); j++)
		{
			DBTSlave* pSlave = pDBT->GetDBTSlave(ar[j], pRec, TRUE);
			if (pSlave && pSlave->IsKindOf(RUNTIME_CLASS(DBTSlaveBuffered)))
				AddDBT(sKey, (DBTSlaveBuffered*)pSlave);	
		}
	}
}

//-----------------------------------------------------------------------------
CSlaveViewContainer* CDBTTreeEdit::CreateSlaveViewContainer(CWnd* pParent, UINT nIDC)
{
	CSlaveViewContainer* pRowViewContainer = NULL;
	if (!m_RowViewContainerMap.Lookup(nIDC, pRowViewContainer))
	{
		pRowViewContainer = new CSlaveViewContainer(nIDC, pParent, GetDocument());
		m_RowViewContainerMap[nIDC] = pRowViewContainer;
	}
	return pRowViewContainer;
}

//-----------------------------------------------------------------------------
void CDBTTreeEdit::SetNodeImages(DBTSlaveBuffered* pDBTObject, const CString& sDBTNodeImage, const CString& sRecordNodeImage)
{
	CNodeLevelInfo* pInfo = GetNodeLevelInfo(pDBTObject);
	if (!pInfo)
	{
		ASSERT(FALSE);
		return;
	}
	if (!sDBTNodeImage.IsEmpty())
		pInfo->m_sDBTImage = sDBTNodeImage;
	if (!sRecordNodeImage.IsEmpty())
		pInfo->m_sRecordImage = sRecordNodeImage;
}

//-----------------------------------------------------------------------------
CBodyEdit* CDBTTreeEdit::AddBody
	(
	DBTSlaveBuffered*	pDBTObject, 
	UINT				nBodyIDC,
	UINT				nViewIDC,
	CRuntimeClass*		pBodyClass, 
	CRuntimeClass*		pViewClass,
	CString				strRowFormViewTitle, /*=_T("")*/
	CString				sBodyName,			/*= _T("")*/
	CString				sRowViewName/*= _T("")*/
	)
{
	CWnd* pParentWnd = GetParent();
	CParsedForm* pParentForm = GetParsedForm(pParentWnd);
	
	CSlaveViewContainer* pRowViewContainer = CreateSlaveViewContainer(pParentWnd, nViewIDC);
	
	CRect r; 
	pRowViewContainer->GetWindowRect(r);
	CBodyEdit* pEdit = (CBodyEdit*)::AddLinkAndCreateBodyEdit(
		CRect(CPoint(0,0), r.Size()),
		pParentForm,
		pRowViewContainer, 
		pParentForm->GetControlLinks(),
		nBodyIDC, 
		pDBTObject, 
		pBodyClass,
		pViewClass,
		strRowFormViewTitle,
		sBodyName, 
		sRowViewName);

	pEdit->ShowWindow(SW_HIDE);
	pEdit->m_nDirStrech = 0;//lo streccierà la slaveviewcontainer

	CNodeLevelInfo* pInfo = new CNodeLevelInfo;
	pInfo->m_nBodyIDC = nBodyIDC;
	pInfo->m_nRowViewIDC = nViewIDC;
	pInfo->m_pBodyEdit = pEdit;
	pInfo->m_bUseBodyEdit = TRUE;
	pInfo->m_nsDBT = pDBTObject->GetNamespace();
	m_NodeInfoMap.Add(pInfo);
	
	return pEdit;
}

//-----------------------------------------------------------------------------
void CDBTTreeEdit::OnSizeChanged() 
{
	int nNewWidth = GetSize().cx;
	int offset = m_nWidth == 0 ? 0 : nNewWidth - m_nWidth;
	m_nWidth = nNewWidth;
	if (offset == 0 || !IsAnimating())
		return;
	POSITION pos = m_RowViewContainerMap.GetStartPosition();
	while (pos)
	{
		UINT key;
		CSlaveViewContainer* pVal;
		m_RowViewContainerMap.GetNextAssoc(pos, key, pVal);
		CRect r;
		pVal->GetWindowRect(r);
		pVal->GetParent()->ScreenToClient(r);
		r.left += offset;
		pVal->MoveWindow(r, TRUE);
		pVal->OnRecalcCtrlSize(NULL, NULL);
	}
}

//-----------------------------------------------------------------------------
CBodyEdit* CDBTTreeEdit::AddView(DBTSlaveBuffered* pDBTObject, UINT nBodyIDC, UINT nViewIDC, CRuntimeClass* pViewClass)
{
	CWnd* pParentWnd = GetParent();
	CParsedForm* pParentForm = GetParsedForm(pParentWnd);
	CBodyEdit* pEdit = (CBodyEdit*)::AddLinkAndCreateBodyEdit(
		CRect(0, 0, 100, 100),
		pParentForm,
		pParentWnd,
		pParentForm->GetControlLinks(),
		nBodyIDC, 
		pDBTObject, 
		RUNTIME_CLASS(CBodyEdit),
		pViewClass,
		_T(""),
		cwsprintf(_T("DUMMYBODY%d"), m_NodeInfoMap.GetCount())
		);
	pEdit->ShowWindow(SW_HIDE);
	
	CSlaveViewContainer* pRowViewContainer = CreateSlaveViewContainer(pParentWnd, nViewIDC);
	pEdit->SetRowViewContainer(pRowViewContainer, FALSE);
	
	CNodeLevelInfo* pInfo = new CNodeLevelInfo;
	pInfo->m_nBodyIDC = nBodyIDC;
	pInfo->m_nRowViewIDC = nViewIDC;
	pInfo->m_pBodyEdit = pEdit;
	pInfo->m_bUseBodyEdit = FALSE;
	pInfo->m_nsDBT = pDBTObject->GetNamespace();
	m_NodeInfoMap.Add(pInfo);
	return pEdit;
}

//-----------------------------------------------------------------------------
void CDBTTreeEdit::OnToolBarCommand(int cmdId)
{
	switch(cmdId)
	{
	case CMD_NEW:
		{
			DBTObject* pDBT;
			SqlRecord* pRecord;
			GetSelectedNodeObjects(pDBT, pRecord);
			if (pDBT)
			{
				DBTSlaveBuffered* pBuff = (DBTSlaveBuffered*)pDBT;
				CBodyEdit* pEdit = GetBodyEdit(pBuff);
				CNodeLevelInfo* pInfo = GetNodeLevelInfo(pBuff);
				{//scope per la ToggleBOOL
					ToggleBOOL b(m_bCreatingStructure);
					
				/*	//PERMETTE Inserimento oltre che ADD, ma c'e' anomalia Anomalia in test su Prog/Mig n.5118 (MAGIX) - Spec. 0 - Riga 29
					//prendo la riga corrente
					int idx = pEdit->GetCurrRecordIdx();
					//se la riga corrente non è l'ultima, si tratta di inserimento
					BOOL bInsert = idx < pBuff->GetUpperBound();
					//mi sposto nella riga successiva (se fosse l'ultima, l'istruzione aggiunge la riga)
					if (!pEdit->SetCurrRecord(idx + 1, TRUE))
						return;
					
					if (bInsert && !pEdit->DoInsertRecord())
						return;*/
				
					/*	FIX	 Anomalia in test su Prog/Mig n.5118 (MAGIX) - Spec. 0 - Riga 29*/
					/* Chiesto a Paolo, per ora non c'e' esigenza di inserire la riga, ma basta l'aggiunta in coda*/
					//mi sposto su ultima riga, per aggiungere in fondo)
					if (!pEdit->SetCurrRecord(pBuff->GetUpperBound() + 1, TRUE))
						return;
				}

				pInfo->m_nCurrentRow = pEdit->GetCurrRecordIdx();
				SqlRecord* pNewRecord = pEdit->GetCurrRecord();
				m_sSelectedNode.Empty();
				RefreshTree(TRUE, TRUE, TRUE);
				SelectNode(pBuff, pNewRecord);
			}
			break;
		}
	case CMD_DELETE:
		{
			DBTObject* pDBT;
			SqlRecord* pRecord;
			GetSelectedNodeObjects(pDBT, pRecord);
			if (pDBT && pRecord)
			{
				DBTSlaveBuffered* pBuff = (DBTSlaveBuffered*)pDBT;
				CBodyEdit* pEdit = GetBodyEdit(pBuff);
				{//scope per la ToggleBOOL
					ToggleBOOL b(m_bCreatingStructure);
					pEdit->DeleteRecord();
				}
				CNodeLevelInfo* pInfo = GetNodeLevelInfo(pBuff);
				pInfo->m_nCurrentRow = pEdit->GetCurrRecordIdx();
				m_sSelectedNode.Empty();
				SqlRecord* pNewRecord = pEdit->GetCurrRecord();
				RefreshTree(TRUE, TRUE, TRUE);
				SelectNode(pBuff, pNewRecord);
			}
			break;
		}	
	case CMD_TOGGLE_EXPAND:
		{
			ToggleExpansion();
			break;
		}
	}
	__super::OnToolBarCommand(cmdId);
}

//-----------------------------------------------------------------------------
void CDBTTreeEdit::DelayedInit()
{
	m_bInited = TRUE;
	
	SetNodeStateIcon(TRUE); 
	 
	AddImage(szDBTImage, AfxGetPathFinder()->GetFileNameFromNamespace(TBGlyph(szDBTImage), _T("")));;
	AddImage(szRecordImage, AfxGetPathFinder()->GetFileNameFromNamespace(TBGlyph(szRecordImage), _T("")));

	if (m_bCanHide)
		AddHidingCommand();
	if (m_bCanResize)
		AddResizeControl();

	AddToolBarCommand(CMD_NEW, AfxGetPathFinder()->GetFileNameFromNamespace(			TBGlyph(szIconNew),		_T("")), _TB("Add new"), _T('N'), TRUE, FALSE, FALSE);
	AddToolBarCommand(CMD_DELETE, AfxGetPathFinder()->GetFileNameFromNamespace(			TBGlyph(szIconDelete),	_T("")), _TB("Delete"), _T('D'), TRUE, FALSE, FALSE);
	AddToolBarCommand(CMD_TOGGLE_EXPAND, AfxGetPathFinder()->GetFileNameFromNamespace(	TBGlyph(szIconToggle), _T("")), _TB("Toggle node expansion"), _T('T'), TRUE, FALSE, FALSE);

	EnableToolBarCommand(CMD_NEW, FALSE);
	EnableToolBarCommand(CMD_DELETE, FALSE);

	AddControls(); 
	SetAllowDrop(FALSE);
	SetViewContextMenu(FALSE);
	SetDragAndDropOnSameLevel(FALSE);
	SetSelectionMode(CTreeViewAdvCtrl::F_SINGLE);

	//ordino i nodi in base alla gerarchia dei dbt associati
	m_NodeInfoMap.Sort();

	RefreshTree(TRUE, TRUE, TRUE); 

	if (m_pDBT->IsKindOf(RUNTIME_CLASS(DBTSlaveBuffered)))
		Activate(m_pDBT);

	OnLoad();
}

//-----------------------------------------------------------------------------
void CDBTTreeEdit::ToggleExpansion()
{
	m_bExpanded = !m_bExpanded;
	if (m_bExpanded)
		ExpandAll();
	else
		CollapseAll();
}

//-----------------------------------------------------------------------------
void CDBTTreeEdit::ActivateDBTView(DBTSlaveBuffered* pDBT, SqlRecord* pRecord)
{
	CBodyEdit* pEdit = NULL;
	for (int i = 0; i < m_NodeInfoMap.GetCount(); i++)
	{
		CNodeLevelInfo* pInfo = m_NodeInfoMap[i];
		pEdit = pInfo->m_pBodyEdit;
		BOOL bHasContentToShow = pDBT && (pInfo->m_bUseBodyEdit || pRecord);
		pInfo->m_bActive = bHasContentToShow && (pInfo->m_nsDBT == pDBT->GetNamespace());
	
		if (!pEdit)
			continue;
		
		//il nodo visualizza una griglia: allora attivo il bodyedit
		if (pInfo->m_bUseBodyEdit)
		{
			OnActivateRecord(pInfo->m_bActive, pDBT, pRecord);
			pEdit->ShowWindow(pInfo->m_bActive ? SW_SHOW : SW_HIDE); 
			if (pInfo->m_bActive)
			{
				CSlaveViewContainer* pRowViewContainer = NULL;
				if (m_RowViewContainerMap.Lookup(pInfo->m_nRowViewIDC, pRowViewContainer))
					pRowViewContainer->CalcSlaveViewSize();
			}
			continue;
		}
		//altrimenti ho una rowview da visualizzare
		CRowFormView* pView = pEdit->GetRowFormView();
		//non esiste ancora? chiamo la CallDialog per crearla
		if (!pView)
		{
			if (!pInfo->m_bActive)
				continue;

			pInfo->m_pBodyEdit->CallDialog(); 
			pView = pEdit->GetRowFormView();
			CSlaveViewContainer* pRowViewContainer = NULL;
			if (m_RowViewContainerMap.Lookup(pInfo->m_nRowViewIDC, pRowViewContainer))
				pRowViewContainer->CalcSlaveViewSize();
		}
		
		if (!pView)
			continue;

		CWnd* pFrame = pView->GetParentFrame();
		if (!pFrame)
		{
			ASSERT(FALSE);
			continue;
		}

		OnActivateRecord(pInfo->m_bActive, pDBT, pRecord);
		if (pInfo->m_bActive)
		{
			pFrame->ShowWindow(SW_SHOW);
			pFrame->UpdateWindow();//forza il ridisegno, altrimenti ci sono problemi con gli static
		}
		else
		{
			pFrame->ShowWindow(SW_HIDE);
		}
	}
}

//-----------------------------------------------------------------------------
CBodyEdit* CDBTTreeEdit::GetBodyEdit(DBTSlaveBuffered* pBuff)
{
	if (pBuff == NULL)
		return NULL;
	CNodeLevelInfo* pInfo = GetNodeLevelInfo(pBuff);
	return pInfo ? pInfo->m_pBodyEdit : NULL;
}

//-----------------------------------------------------------------------------
CNodeLevelInfo* CDBTTreeEdit::GetNodeLevelInfo(DBTSlaveBuffered* pDBT)
{
	for (int i = 0; i < m_NodeInfoMap.GetCount(); i++)
	{
		CNodeLevelInfo* pInfo = m_NodeInfoMap[i];
		if (pInfo->m_nsDBT == pDBT->GetNamespace())
			return pInfo;
	}
	return NULL;
}

//-----------------------------------------------------------------------------
void CDBTTreeEdit::ClearObjectMap()
{
	POSITION pos = m_ObjectMap.GetStartPosition();
	while(pos)
	{
		CString sKey;
		CTreeBag* ptr;
		m_ObjectMap.GetNextAssoc(pos, sKey, ptr);
		delete ptr;
	}
	m_ObjectMap.RemoveAll();
	m_KeyMap.RemoveAll();
}

//-----------------------------------------------------------------------------
void CDBTTreeEdit::AddToMap(const CString& sKey, DBTSlaveBuffered* pDBT, SqlRecord* pRec)
{
	m_ObjectMap[sKey] = new CTreeBag(pDBT, pRec, &m_nTreeObjectsHash);
	m_KeyMap[pRec ? (CObject*)pRec : (CObject*)pDBT] = sKey;
}

//-----------------------------------------------------------------------------
CString CDBTTreeEdit::GetKeyFromMap(DBTSlaveBuffered* pDBT, SqlRecord* pRec)
{
	CString sKey;
	VERIFY(m_KeyMap.Lookup(pRec ? (CObject*)pRec : (CObject*)pDBT, sKey));
	return sKey;
}

//-----------------------------------------------------------------------------
CTreeBag* CDBTTreeEdit::GetObjectFromMap(const CString& sKey)
{
	if (sKey.IsEmpty())
		return NULL;
	CTreeBag* ptr;
	if (!m_ObjectMap.Lookup(sKey, ptr))
	{
		return NULL;
	}
	return ptr;
}

//-----------------------------------------------------------------------------
void CDBTTreeEdit::Activate(DBTSlaveBuffered* pDBT, SqlRecord* pRecord)
{
	if (pDBT->GetMaster() && pDBT->GetMaster()->IsKindOf(RUNTIME_CLASS(DBTSlaveBuffered)))
		Activate((DBTSlaveBuffered*)pDBT->GetMaster(), pDBT->GetMasterRecord());
	for (int i = 0; i < pDBT->GetSize(); i++)
	{
		SqlRecord *pRow = pDBT->GetRow(i);
		if (pRow == pRecord)
		{
			CBodyEdit* pEdit = GetBodyEdit(pDBT);
			pEdit->SetDBT(pDBT);
			CNodeLevelInfo* pInfo = GetNodeLevelInfo(pDBT);
			pInfo->m_nCurrentRow = i;
			pEdit->SetCurrRecord(i);//TODOPERASSO ottimizzare salvandosi da qualche parte l'indice in fase di costruzione dell'albero
			if (pInfo->m_bUseBodyEdit)
			{
				pEdit->UpdateBodyStatus();//aggiorno perché potrebbe essere cambiato lo stato di readonly
				pEdit->Invalidate();
			}
		}
	}
}

//-----------------------------------------------------------------------------
void CDBTTreeEdit::Activate(DBTSlaveBuffered* pDBT)
{
	if (!pDBT)
		return;//non devo inibire il processo di attivazione

	if (pDBT->GetMaster() && pDBT->GetMaster()->IsKindOf(RUNTIME_CLASS(DBTSlaveBuffered)))
		Activate((DBTSlaveBuffered*)pDBT->GetMaster(), pDBT->GetMasterRecord());
	
	CBodyEdit* pEdit = GetBodyEdit(pDBT);
	pEdit->SetDBT(pDBT);
	pEdit->UpdateBodyStatus();//aggiorno perché potrebbe essere cambiato lo stato di readonly
	if (pEdit->GetCurrRecordIdx() != pDBT->GetCurrentRowIdx())
		pEdit->SetCurrRecord(pDBT->GetCurrentRowIdx());
	pEdit->Invalidate();
}

//-----------------------------------------------------------------------------
void CDBTTreeEdit::OnSelectionChanged()
{
	if (m_bSelecting || m_bCreatingStructure)
		return;
	
	Select(GetSelectedNodeKey());
}

//-----------------------------------------------------------------------------
void CDBTTreeEdit::Select(const CString& sKey)
{
	if (sKey.IsEmpty())
		return;
	
	ToggleBOOL tb1(m_bSelecting);

	CTreeBag *pBag = GetObjectFromMap(sKey);
	if (!pBag)
	{
		ASSERT(FALSE); 
		return;
	}

	if (!pBag->m_DBTPtr)
		return;
	
	//se sto cambiando il nodo, devo controllare se mi posso spostare dal nodo precedente
	if (m_sSelectedNode != sKey)
	{
		CTreeBag *pCurrentBag = GetObjectFromMap(m_sSelectedNode);
		CBodyEdit* pCurrEdit = pCurrentBag ? GetBodyEdit(pCurrentBag->m_DBTPtr) : NULL;
		if (pCurrEdit && !pCurrEdit->CanLeaveCurrPos(-1, FALSE))
		{
			//se non mi sono spostato, rimetto il nodo selezionato in precedenza
			SetNodeAsSelected(m_sSelectedNode);
			return;
		}
		m_sSelectedNode = sKey;
	}
	//sia che stia cambiando, sia che il nodo sia già selezionato, devo comunque forzare
	//il processo di attivazione degli oggetti collegati perché potrebbero
	//avere bisogno di un refresh
	if (pBag->m_RecordPtr)
		Activate(pBag->m_DBTPtr, pBag->m_RecordPtr);
	else
		Activate(pBag->m_DBTPtr);
	
	pBag = GetObjectFromMap(sKey);//lo ripesco, perché se il tree si è refreshato l'oggetto potrebbe non essere più valido
	if (!pBag)
	{
		ASSERT(FALSE); 
		return;
	}
	ActivateDBTView(pBag->m_DBTPtr, pBag->m_RecordPtr);	
	
	CBodyEdit* pEdit = GetBodyEdit(pBag->m_DBTPtr);
	
	EnableToolBarCommand(CMD_NEW, pEdit ? (pEdit->CanInsertRowByTreeEdit(FALSE) == TRUE) : FALSE);
	EnableToolBarCommand(CMD_DELETE, pEdit ? (pEdit->InternalCanDeleteRow() == TRUE) : FALSE);
	
	return;
}

//-----------------------------------------------------------------------------
void CDBTTreeEdit::GetSelectedNodeObjects(DBTObject*& pDBT, SqlRecord*& pRecord)
{
	CString sKey = GetSelectedNodeKey();
	CTreeBag *pBag = GetObjectFromMap(sKey);
	if (!pBag)
	{
		pDBT = NULL;
		pRecord = NULL;
		return;
	}

	pDBT = pBag->m_DBTPtr;
	pRecord = pBag->m_RecordPtr;
}