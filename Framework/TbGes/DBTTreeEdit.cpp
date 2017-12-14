///////////////////////////////////////////////////////////////////////////////
//-----------------------------------------------------------------------------

#include "stdafx.h"

#include <tbges\extdoc.h>

// local
#include "DbtTreeEdit.h"

#ifdef _DEBUG
#define new DEBUG_NEW
#endif

//-----------------------------------------------------------------------------
//-----------------------------------------------------------------------------
void CTreeBodyEditNodeInfo::SetInfo(const CString& sInfo)
{
	if (sInfo.IsEmpty())
	{
		m_nLevel	= 1;
		m_nParent	= 0;
		m_bHasChild = FALSE;
		m_bExpanded = FALSE;
		m_idBitmap	= -1;
	}
	else
		_stscanf_s(sInfo, _T("%d|%d|%d|%d|%d"), &m_nLevel, &m_nParent, &m_bHasChild, &m_bExpanded, &m_idBitmap);
}

///////////////////////////////////////////////////////////////////////////////
// DBTTree
//-----------------------------------------------------------------------------
IMPLEMENT_DYNAMIC(DBTTree, DBTSlaveBuffered)
//-----------------------------------------------------------------------------
DBTTree::DBTTree
		(
			CRuntimeClass*		pClass,
			CAbstractFormDoc*	pDocument,
			const CString&		sName,
			BOOL				bAllowEmpty ,
			BOOL				bCheckDuplicateKey
		)
		:
		DBTSlaveBuffered(pClass, pDocument, sName, bAllowEmpty, bCheckDuplicateKey),

		m_nTreeDataIdx	(-1),
		m_nTreeEditIdx	(-1),
		
		m_nTreeIDIdx (-1),

		m_bShowTreeMsg (TRUE)
{
	// carico tutti i record da database
	m_nPreloadStep = -1;
	
	SetAllowFilter(TRUE); //Occorre chiamare il metodo cha alloca l'array AllRecords
}

//-----------------------------------------------------------------------------
DBTTree::~DBTTree()
{
}

//-----------------------------------------------------------------------------
void DBTTree::Init()
{
	DBTSlaveBuffered::Init();

	// cerco l'indice dei tree data nel record.
	m_nTreeDataIdx		= m_pRecord->Lookup(GetTreeDataObj());
	m_nTreeEditIdx		= m_pRecord->Lookup(GetTreeEditObj());
	// non sono definiti i campi TreeData nel tree.
	ASSERT(m_nTreeDataIdx != -1);
	ASSERT(m_nTreeEditIdx != -1);

	//se il metodo virtuale è stato reimplementato
	if (GetTreeIDObj())
	{
		m_nTreeIDIdx	= m_pRecord->Lookup(GetTreeIDObj());
		ASSERT(m_nTreeIDIdx != -1);
	}
}

//-----------------------------------------------------------------------------
DataStr&	DBTTree::GetTreeData(SqlRecord* pRec)		
{ 
	ASSERT_VALID(pRec);
	ASSERT(m_nTreeDataIdx >= 0);
	DataObj* pdata = pRec->GetDataObjAt(m_nTreeDataIdx);
	ASSERT_VALID(pdata);
	ASSERT_KINDOF(DataStr, pdata);

	return *((DataStr*)pdata);		
}

DataStr&	DBTTree::GetTreeData(int nIdx)				
{ return GetTreeData(GetRow(nIdx)); }

DataStr&	DBTTree::GetAllTreeData(int nidx)			
{ 
	ASSERT_VALID(m_pAllRecords);
	return GetTreeData(m_pAllRecords->GetAt(nidx));			
}
//-----------------------------------------------------------------------------

DataObj&	DBTTree::GetTreeID(SqlRecord* pRec)
{ 
	ASSERT_VALID(pRec);
	ASSERT(m_nTreeIDIdx >= 0);
	DataObj* pdata = pRec->GetDataObjAt(m_nTreeIDIdx);
	ASSERT_VALID(pdata);
	//ASSERT_KINDOF(DataLng, pdata);
	return *pdata;			
}

DataObj&	DBTTree::GetTreeID(int nIdx)					
{ return GetTreeID(GetRow(nIdx)); }

DataObj&	DBTTree::GetAllTreeID(int nidx)				
{ 
	ASSERT_VALID(m_pAllRecords);
	return GetTreeID(m_pAllRecords->GetAt(nidx));				
}

//-----------------------------------------------------------------------------
DataObj&	DBTTree::GetTreeEdit(SqlRecord* pRec)		
{ 
	ASSERT_VALID(pRec);
	ASSERT(m_nTreeEditIdx >= 0);
	DataObj* pdata = pRec->GetDataObjAt(m_nTreeEditIdx);
	ASSERT_VALID(pdata);
	return *pdata; 
}

DataObj&	DBTTree::GetTreeEdit(int nIdx)				
{ return GetTreeEdit(GetRow(nIdx)); }

DataObj&	DBTTree::GetAllTreeEdit(int nidx)
{ 
	ASSERT_VALID(m_pAllRecords);
	return GetTreeEdit(m_pAllRecords->GetAt(nidx));			
}

//-----------------------------------------------------------------------------
void DBTTree::SetLevelAll(int nLevel)
{
	if (CanDoSetLevelAll())
	{
		ASSERT_VALID(m_pAllRecords);
		// prima di tutto comprimo al primo livello...
		SetAllExpand(FALSE, FALSE);
		for (int c = 0; c <= m_pAllRecords->GetUpperBound(); c++)
		{
			CTreeBodyEditNodeInfo ni(GetAllTreeData(c));
			if (ni.HasChild() && (ni.GetLevel() < nLevel-1))
			{
				ni.SetExpanded(TRUE);
				GetAllTreeData(c) = ni.GetInfo();
			}
		}

		GetDocument()->SetModifiedFlag();

		BuildTree();
	}
}

//------------------------------------------------------------------------------
BOOL DBTTree::MoveLeft(BOOL bMoveNextBrothers)
{
	if (!CanDoMoveLeft() || !OnBeforeMoveLeft()) 
		return FALSE;

	if (GetCurrentRowIdx() == -1)
	{
		TRACE0("DBTTree::MoveLeft(): no right check in the CanDoMoveLeft()\n");
		ASSERT(FALSE);
		return FALSE;
	}

	int nCurrRecIdx = RemapIndexF2A(GetCurrentRowIdx());
	int nPrevRecIdx = nCurrRecIdx-1;

	if (nCurrRecIdx == 0)
	{
		TRACE0("DBTTree::MoveLeft(): no right check in the CanDoMoveLeft()\n");
		ASSERT(FALSE);
		return FALSE;
	}

	CTreeBodyEditNodeInfo CurrRec_ni(GetAllTreeData(nCurrRecIdx));
	int nCurrRecLevel = CurrRec_ni.GetLevel();

	if (nCurrRecLevel <= 1)
	{
		TRACE0("DBTTree::MoveLeft(): no right check in the CanDoMoveLeft()\n");
		ASSERT(FALSE);
		return FALSE;
	}

	// indento la riga corrente e tutte le sue righe figlie/sorelle

	// aggiorno il campo PARENT della riga corrente
	int nCurrRecParent = CurrRec_ni.GetParent();
	CTreeBodyEditNodeInfo ParentRec_ni(GetAllTreeData(nCurrRecParent-1));
	CurrRec_ni.SetParent(ParentRec_ni.GetParent());

	// aggiorno il campo LEVEL delle righe figlie/sorelle della riga corrente
	// aggiorno il campo PARENT delle righe sorelle della riga corrente
	for (int nSuccRecIdx = nCurrRecIdx+1; nSuccRecIdx <= GetUnfilteredUpperBound(); nSuccRecIdx++)
	{
		CTreeBodyEditNodeInfo SuccRec_ni(GetAllTreeData(nSuccRecIdx));
		int nSuccRecLevel = SuccRec_ni.GetLevel();

		if (nSuccRecLevel > nCurrRecLevel || (bMoveNextBrothers && nSuccRecLevel == nCurrRecLevel))
		{
			if (nSuccRecLevel == nCurrRecLevel)
				SuccRec_ni.SetParent(CurrRec_ni.GetParent());

			SuccRec_ni.SetLevel(--nSuccRecLevel);
		}
		else
		{
			if (nSuccRecLevel == nCurrRecLevel)
			{
				CTreeBodyEditNodeInfo ni(GetAllTreeData(nSuccRecIdx - 1));
				ni.SetHasChild(1);
				GetAllTreeData(nSuccRecIdx-1) = ni.GetInfo();
			}

			break;
		}
		// memorizzo i TREEDATA della riga successiva
		GetAllTreeData(nSuccRecIdx) = SuccRec_ni.GetInfo();
	}

	// aggiorno il campo LEVEL della riga corrente
	CurrRec_ni.SetLevel(--nCurrRecLevel);

	// memorizzo i TREEDATA della riga corrente
	GetAllTreeData(nCurrRecIdx) = CurrRec_ni.GetInfo();

	// aggiorno il campo HASCHILD della riga precedente
	CTreeBodyEditNodeInfo PrevRec_ni(GetAllTreeData(nPrevRecIdx));

	if (PrevRec_ni.GetLevel() == CurrRec_ni.GetLevel())
	{
		PrevRec_ni.SetHasChild(FALSE);

		// memorizzo i TREEDATA della riga precedente
		GetAllTreeData(nPrevRecIdx) = PrevRec_ni.GetInfo();
	}
	GetDocument()->SetModifiedFlag();

	//BuildTree();

	OnAfterMoveLeft();

	return TRUE;
}

//------------------------------------------------------------------------------
BOOL DBTTree::MoveRight(BOOL bMoveNextBrothers)
{
	if (!CanDoMoveRight() || !OnBeforeMoveRight()) 
		return FALSE;

	if (GetCurrentRowIdx() == -1)
	{
		TRACE0("DBTTree::MoveRight(): no right check in the CanDoMoveRight()\n");
		ASSERT(FALSE);
		return FALSE;
	}

	int nCurrRecIdx = RemapIndexF2A(GetCurrentRowIdx());
	int nPrevRecIdx = nCurrRecIdx-1;
	if (nCurrRecIdx == 0)
	{
		TRACE0("DBTTree::MoveRight(): no right check in the CanDoMoveRight()\n");
		ASSERT(FALSE);
		return FALSE;
	}

	// altrimenti è un record successivo del DBT

	CTreeBodyEditNodeInfo CurrRec_ni(GetAllTreeData(nCurrRecIdx));
	CTreeBodyEditNodeInfo PrevRec_ni(GetAllTreeData(nPrevRecIdx));

	int nCurrRecLevel = CurrRec_ni.GetLevel();
	int nPrevRecLevel = PrevRec_ni.GetLevel();

	if (nPrevRecLevel < nCurrRecLevel)
	{
		TRACE0("DBTTree::MoveRight(): no right check in the CanDoMoveRight()\n");
		ASSERT(FALSE);
		return FALSE;
	}

	// la riga corrente non è indentata rispetto alla precedente,
	// per cui posso indentare la riga corrente e tutte le sue righe figlie/sorelle

	// aggiorno i campi HASCHILD e EXPANDED della riga precedente
	if (nPrevRecLevel == nCurrRecLevel)
	{
		PrevRec_ni.SetHasChild(TRUE);
		PrevRec_ni.SetExpanded(TRUE);

		// memorizzo i TREEDATA della riga precedente
		GetAllTreeData(nPrevRecIdx) = PrevRec_ni.GetInfo();
	}

	// aggiorno il campo PARENT della riga corrente
	if (nPrevRecLevel == nCurrRecLevel)
	{
		CurrRec_ni.SetParent(nPrevRecIdx + 1);
	}
	else // nPrevRecLevel > nCurrRecLevel
	{
		ASSERT(nPrevRecLevel > nCurrRecLevel);
		// risalgo l'albero fino a trovare il primo padre della riga precedente
		// alla riga corrente che può essere padre anche di quest'ultima
		while (PrevRec_ni.GetLevel() > nCurrRecLevel + 1)
		{
			PrevRec_ni.SetInfo(GetAllTreeData(PrevRec_ni.GetParent()-1));
		}

		CurrRec_ni.SetParent(PrevRec_ni.GetParent());
	}

	// aggiorno il campo LEVEL delle righe figlie/sorelle della riga corrente
	// aggiorno il campo PARENT delle righe sorelle della riga corrente
	for (int nSuccRecIdx = nCurrRecIdx+1; nSuccRecIdx <= GetUnfilteredUpperBound(); nSuccRecIdx++)
	{
		CTreeBodyEditNodeInfo SuccRec_ni(GetAllTreeData(nSuccRecIdx));
		int nSuccRecLevel = SuccRec_ni.GetLevel();

		if (nSuccRecLevel > nCurrRecLevel || (bMoveNextBrothers && nSuccRecLevel == nCurrRecLevel))
		{
			if (nSuccRecLevel == nCurrRecLevel)
				SuccRec_ni.SetParent(CurrRec_ni.GetParent());

			SuccRec_ni.SetLevel(++nSuccRecLevel);
		}
		else
			break;

		// memorizzo i TREEDATA della riga successiva
		GetAllTreeData(nSuccRecIdx) = SuccRec_ni.GetInfo();
	}

	// aggiorno il campo LEVEL della riga corrente
	CurrRec_ni.SetLevel(++nCurrRecLevel);

	// memorizzo i TREEDATA della riga corrente
	GetAllTreeData(nCurrRecIdx) = CurrRec_ni.GetInfo();

	GetDocument()->SetModifiedFlag();

	//BuildTree();

	OnAfterMoveRight();

	return TRUE;
}

//-----------------------------------------------------------------------------
// aggiorno il TreeData.Parent in fase di rinumerazione righe
void DBTTree::UpdateTreeDataParent(RecordArray* parRecords, int startIndex, int oldKey, int newKey)
{
	for (int i = startIndex; i < parRecords->GetSize(); i++)
	{
		DataStr& dsref = GetTreeData(parRecords->GetAt(i));
		CTreeBodyEditNodeInfo ni(dsref);
		if (ni.GetParent() == oldKey)
		{
			ni.SetParent(newKey);
			dsref = ni.GetInfo();
		}
	}
}

//------------------------------------------------------------------------------
SqlRecord* DBTTree::InsertRoot()
{
	return AddRecord();
}

//------------------------------------------------------------------------------
SqlRecord* DBTTree::AddRecord() 
{
	ASSERT(m_pRecords);
	int nIdxNewRec = -1;
	SqlRecord* pRec = Insert
						( 
							(this->m_nCurrentRow > -1 && this->m_nCurrentRow == GetUpperBound() ? 
								this->m_nCurrentRow : 
								GetUpperBound() + 1
							),
							nIdxNewRec
							);

	// Da la possibilita' al programmatore di sapere che e' stato aggiunto un 
	// record. Il programmatore decide cosa fare reimplementando opportunamente.
	// Di default non viene fatto nulla
	OnAfterInsertRow(nIdxNewRec, pRec);
	GetClientDocs()->OnAfterInsertRow(this, nIdxNewRec, pRec);

	return pRec;
}

//------------------------------------------------------------------------------
SqlRecord* DBTTree::Insert/*Broter*/(int nRow, int& nIdxNewRec)
{
	int	nIdx = RemapIndexF2A(nRow);

	CTreeBodyEditNodeInfo ni
					(
						nIdx >= 0 && nIdx <= m_pAllRecords->GetUpperBound() ? 
						GetAllTreeData(nIdx).GetString() :
						_T("")
					);

	// scorro fino all'ultimo figlio del ramo su cui era posizionato...
	// verra inserito un nuovo nodo di pari livello
	int c;
	for (c = nIdx + 1; c <= m_pAllRecords->GetUpperBound(); c++)
	{
		CTreeBodyEditNodeInfo cni(GetAllTreeData(c));

		if (ni.GetLevel() >= cni.GetLevel())
		{
			break;
		}
	}

	// inserisco il record
	int rc = RemapIndexA2F(c);

	SqlRecord*	pNewRec = InsertRecord(rc);
	if (!pNewRec)
		return NULL;

	// completo il treedata del nuovo record (inizializzato dalla OnPrepareRow)
	CTreeBodyEditNodeInfo cni(GetTreeData(pNewRec));
		cni.SetLevel	(ni.GetLevel());
		cni.SetParent	(ni.GetParent());
		cni.SetHasChild	(FALSE);
		cni.SetExpanded	(FALSE);
	GetTreeData(pNewRec) = cni.GetInfo();

	// aggiorno il campo HASCHILD del parent
	if (ni.GetLevel() > 1)
	{
		SqlRecord* pParentRec = m_pRecords->GetAt(ni.GetParent());
		if (pParentRec)
		{
			CTreeBodyEditNodeInfo Prev_ni(GetTreeData(pParentRec));
			Prev_ni.SetHasChild(TRUE);
			GetTreeData(pParentRec) = Prev_ni.GetInfo();
		}
	}

	BuildTree();

	nIdxNewRec = RemapIndexA2F(c);

	// Da la possibilita' al programmatore di sapere che e' stato aggiunto un 
	// record. Il programmatore decide cosa fare reimplementando opportunamente.
	// Di default non viene fatto nulla
	OnAfterInsertRow(nIdxNewRec, pNewRec);
	GetClientDocs()->OnAfterInsertRow(this, nIdxNewRec, pNewRec);

	return pNewRec;
}

//-----------------------------------------------------------------------------
SqlRecord* DBTTree::InsertChild(int nRow, int& nIdxNewRec)
{
	int	nIdx = RemapIndexF2A(nRow);
	ASSERT(nIdx >= 0 && nIdx <= m_pAllRecords->GetUpperBound());

	CTreeBodyEditNodeInfo ni(GetAllTreeData(nIdx));

	// scorro fino all'ultimo figlio...
	int c;
	for (c = nIdx + 1; c <= m_pAllRecords->GetUpperBound(); c++)
	{
		CTreeBodyEditNodeInfo cni(GetAllTreeData(c));

		if (ni.GetLevel() >= cni.GetLevel())
		{
			break;
		}
	}

	// inserisco il record
	int rc = RemapIndexA2F(c);

	SqlRecord*	pNewRec = InsertRecord(rc);
	if (!pNewRec)
		return NULL;

	// completo il treedata del nuovo record (inizializzato dalla OnPrepareRow)
	CTreeBodyEditNodeInfo cni(GetTreeData(pNewRec));
		cni.SetLevel	(ni.GetLevel() + 1);
		cni.SetParent	(nIdx + 1);
		cni.SetHasChild	(FALSE);
		cni.SetExpanded	(FALSE);
	GetTreeData(pNewRec) = cni.GetInfo();

	// aggiorno il treedata del nodo padre
	ni.SetHasChild();
	ni.SetExpanded();
	GetAllTreeData(nIdx) = ni.GetInfo();

	BuildTree();

	nIdxNewRec = RemapIndexA2F(c);

	// Da la possibilita' al programmatore di sapere che e' stato aggiunto un 
	// record. Il programmatore decide cosa fare reimplementando opportunamente.
	// Di default non viene fatto nulla
	OnAfterInsertRow(nIdxNewRec, pNewRec);
	GetClientDocs()->OnAfterInsertRow(this, nIdxNewRec, pNewRec);

	return pNewRec;
}

//-----------------------------------------------------------------------------
void DBTTree::Sort(int nRow, int& nIdxLastSortedRec, BOOL bToRemap /*= TRUE*/)
{
	int nIdx, c, d, startlevel;

	if (bToRemap && nRow != -1)
		nIdx = RemapIndexF2A(nRow);
	else
		nIdx = nRow;

	if (nRow != -1)
	{
		CTreeBodyEditNodeInfo ni(GetAllTreeData(nIdx));
		startlevel = ni.GetLevel();
	}
	else
		startlevel = 0;

	// ciclo sui nodi figli
	for (c = nIdx + 1; c <= GetUnfilteredUpperBound(); c++)
	{
		CTreeBodyEditNodeInfo cni(GetAllTreeData(c));

		// controllo che sia un figlio
		if (cni.GetLevel() <= startlevel)
			break;

		// se il nodo a sua volta ha dei figli, ordino prima questi (ricorsione)
		if (cni.HasChild())
		{
			Sort(c, nIdxLastSortedRec, FALSE);
			c = nIdxLastSortedRec;
		}
	}

	// dopo aver sistemato i "nipoti" ordino i figli

	for (c = nIdx + 1; c <= GetUnfilteredUpperBound(); c++)
	{
		CTreeBodyEditNodeInfo cni(GetAllTreeData(c));

		// controllo che sia un figlio
		if (cni.GetLevel() <= startlevel)
			break;

		// se non è un figlio diretto
		if (cni.GetLevel() > startlevel + 1)
			continue;

		// parto dal secondo figlio fino all'ultimo figlio, facendo salire in alto il record
		// con il valore minore nella colonna del treeedit
		for (d = c + 1; d <= GetUnfilteredUpperBound(); d++)
		{
			CTreeBodyEditNodeInfo dni(GetAllTreeData(d));

			// se non appartiene alla discendenza
			if (dni.GetLevel() <= startlevel)
				break;

			// se non è un figlio diretto
			if (dni.GetLevel() > startlevel + 1)
				continue;

			SqlRecord*	pRecC		= m_pAllRecords->GetAt(c);
			DataObj&	aDataObjC	= GetAllTreeEdit(c);

			SqlRecord*	pRecD		= m_pAllRecords->GetAt(d);
			DataObj&	aDataObjD	= GetAllTreeEdit(d);

			if(aDataObjD.IsLessThan(aDataObjC))
			{
				// swap dei records...
				SwapRecords(c, pRecC, d, pRecD, startlevel + 1);
			}
		}
	}
	nIdxLastSortedRec = (c > GetUnfilteredUpperBound() ? GetUnfilteredUpperBound() : c - 1);
}

//-----------------------------------------------------------------------------
void DBTTree::SwapRecords(int nRowA, SqlRecord* pRecA, int nRowB, SqlRecord* pRecB, int nLevel)
{
	ASSERT (nRowA < nRowB);

	int c, d;
	int nRowAHasMoreChildren = 0;

	// scambio le posizioni dei nodi
	m_pAllRecords->SetAt(nRowA, pRecB);
	m_pAllRecords->SetAt(nRowB, pRecA);

	CTreeBodyEditNodeInfo cni;
	CTreeBodyEditNodeInfo dni;

	for (
			c = nRowA + 1, d = nRowB + 1;
			c <= GetUnfilteredUpperBound() && d <= GetUnfilteredUpperBound();
			c++, d++
		)
	{
		cni.SetInfo(GetAllTreeData(c));
		dni.SetInfo(GetAllTreeData(d));

		// scambio le posizioni dei nodi figli (finché ci sono figli su entrambi i nodi)
		if (cni.GetLevel() > nLevel && dni.GetLevel() > nLevel)
		{
			SqlRecord*	pRecC		= m_pAllRecords->GetAt(c);
			SqlRecord*	pRecD		= m_pAllRecords->GetAt(d);

			m_pAllRecords->SetAt(c, pRecD);
			m_pAllRecords->SetAt(d, pRecC);

			// aggiorno il campo parent
			cni.SetParent(cni.GetParent() + nRowB - nRowA);
			dni.SetParent(dni.GetParent() - nRowB + nRowA);

			GetAllTreeData(c) = dni.GetInfo();
			GetAllTreeData(d) = cni.GetInfo();
		}
		else
		{
			if (cni.GetLevel() <= nLevel && dni.GetLevel() > nLevel)
				nRowAHasMoreChildren = -1;
			else if (cni.GetLevel() > nLevel && dni.GetLevel() <= nLevel)
			{
				nRowAHasMoreChildren = 1;
				d--;
			}
			break;
		}
	}

	// sposto i nodi figli del nodo che ne ha di più
	cni.SetInfo(GetAllTreeData(c));

	if (d > GetUnfilteredUpperBound() && cni.GetLevel() > nLevel)
		nRowAHasMoreChildren = 1;

	switch (nRowAHasMoreChildren)
	{
	case 1:	// ha più figli il primo nodo
		while(cni.GetLevel() > nLevel)
		{
			CTreeBodyEditNodeInfo eni, fni;

			for (int e = c; e < d && e < GetUnfilteredUpperBound(); e++)
			{
				eni.SetInfo(GetAllTreeData(e));
				fni.SetInfo(GetAllTreeData(e + 1));

				SqlRecord*	pRecE		= m_pAllRecords->GetAt(e);
				SqlRecord*	pRecF		= m_pAllRecords->GetAt(e + 1);

				m_pAllRecords->SetAt(e, pRecF);
				m_pAllRecords->SetAt(e + 1, pRecE);

				// aggiorno il campo parent
				eni.SetParent	(eni.GetParent() + 1);
				GetAllTreeData	(e + 1)	= eni.GetInfo();

				if (fni.GetLevel() > nLevel)
				{
					fni.SetParent	(fni.GetParent() - 1);
					GetAllTreeData	(e) = fni.GetInfo();
				}
			}
			cni.SetInfo(GetAllTreeData(c));
		}
		break;

	case -1: // ha più figli il secondo nodo
		for (; d <= GetUnfilteredUpperBound(); d++, c++)
		{
			CTreeBodyEditNodeInfo eni, fni;

			dni.SetInfo(GetAllTreeData(d));

			if (dni.GetLevel() <= nLevel)
				break;

			for (int e = d; e > c && e > 0; e--)
			{
				eni.SetInfo(GetAllTreeData(e));
				fni.SetInfo(GetAllTreeData(e - 1));

				SqlRecord*	pRecE		= m_pAllRecords->GetAt(e);
				SqlRecord*	pRecF		= m_pAllRecords->GetAt(e - 1);

				m_pAllRecords->SetAt(e, pRecF);
				m_pAllRecords->SetAt(e - 1, pRecE);

				// aggiorno il campo parent
				eni.SetParent	(eni.GetParent() - 1);
				GetAllTreeData	(e - 1)	= eni.GetInfo();

				if (fni.GetLevel() > nLevel)
				{
					fni.SetParent	(fni.GetParent() + 1);
					GetAllTreeData	(e) = fni.GetInfo();
				}
			}
		}
		break;
	};
}

//-----------------------------------------------------------------------------
void DBTTree::MakeVisible(int nIdx)
{
	CTreeBodyEditNodeInfo ni(GetAllTreeData(nIdx));

	while (ni.GetParent() > 0)
	{
		int parent = ni.GetParent() - 1;

		if (parent < 0 || parent > GetUnfilteredUpperBound())
			return;

		ni.SetInfo(GetAllTreeData(parent));

		if (ni.HasChild()) // ridondante
			ni.SetExpanded(TRUE);

		GetAllTreeData(parent) = ni.GetInfo();
	}

	GetDocument()->SetModifiedFlag();
	BuildTree();
}

//------------------------------------------------------------------------------
BOOL DBTTree::CanDoMoveLeft()
{
	if (m_pDocument == NULL || (m_pDocument->GetFormMode() != CBaseDocument::NEW &&
								m_pDocument->GetFormMode() != CBaseDocument::EDIT))
	{
//		if (GetShowTreeMsg())
//			AfxMessageBox("L'indentazione può essere modificata solo in fase di inserimento o modifica!\n", MB_OK | MB_ICONINFORMATION);
		return FALSE;
	}

	if (GetCurrentRowIdx() == -1)
	{
		// il BE è vuoto oppure non è selezionata nessuna riga
//		if (m_bShowTreeMsg)
//			AfxMessageBox("Non è selezionata nessuna riga!\n", MB_OK | MB_ICONINFORMATION);
		return FALSE;
	}

	if (RemapIndexF2A(GetCurrentRowIdx()) == 0)
	{
		// il record corrente è il primo record del BE,
		// per cui non posso indentare;
		// starà sempre al livello 1
//		if (m_bShowTreeMsg)
//			AfxMessageBox("La riga corrente è la prima della griglia: non può essere indentata!\n", MB_OK | MB_ICONINFORMATION);
		return FALSE;
	}

	CTreeBodyEditNodeInfo CurrRec_ni(GetAllTreeData(GetCurrentRowIdx()));

	if (CurrRec_ni.GetLevel() <= 1)
	{
		// La riga corrente è a livello 1;
		//non può essere indentata a sinistra
//		if (m_bShowTreeMsg)
//			AfxMessageBox("La riga corrente è a livello 1: non può essere indentata a sinistra!\n", MB_OK | MB_ICONINFORMATION);
		return FALSE;
	}

	return TRUE;
}

//------------------------------------------------------------------------------
BOOL DBTTree::CanDoMoveRight()
{
	if (m_pDocument == NULL || (m_pDocument->GetFormMode() != CBaseDocument::NEW &&
								m_pDocument->GetFormMode() != CBaseDocument::EDIT))
	{
//		if (GetShowTreeMsg())
//			AfxMessageBox("L'indentazione può essere modificata solo in fase di inserimento o modifica!\n", MB_OK | MB_ICONINFORMATION);
		return FALSE;
	}

	if (GetCurrentRowIdx() == -1)
	{
		// il BE è vuoto oppure non è selezionata nessuna riga
//		if (m_bShowTreeMsg)
//			AfxMessageBox("Non è selezionata nessuna riga!\n", MB_OK | MB_ICONINFORMATION);
		return FALSE;
	}

	int nCurrRecIdx = RemapIndexF2A(GetCurrentRowIdx());
	int nPrevRecIdx = nCurrRecIdx-1;
	if (nCurrRecIdx == 0)
	{
		// il record corrente è il primo record del BE,
		// per cui non posso indentare;
		// starà sempre al livello 1
//		if (m_bShowTreeMsg)
//			AfxMessageBox("La riga corrente è la prima della griglia: non può essere indentata!\n", MB_OK | MB_ICONINFORMATION);
		return FALSE;
	}

	// altrimenti è un record successivo del DBT

	CTreeBodyEditNodeInfo CurrRec_ni(GetAllTreeData(nCurrRecIdx));
	CTreeBodyEditNodeInfo PrevRec_ni(GetAllTreeData(nPrevRecIdx));

	if (PrevRec_ni.GetLevel() < CurrRec_ni.GetLevel())
	{
		// la riga corrente è già indentata rispetto alla precedente,
		// per cui non posso indentare ulteriormente
//		if (m_bShowTreeMsg)
//			AfxMessageBox("La riga corrente è già indentata rispetto alla precedente:\nnon può essere indentata ulteriormente a destra!\n", MB_OK | MB_ICONINFORMATION);
		return FALSE;
	}

	return TRUE;
}

//------------------------------------------------------------------------------
BOOL DBTTree::CanDoExpandNode(int nRow)
{
	if(nRow < 0 || nRow > GetUpperBound())
	{
//		if (m_bShowTreeMsg)
//			AfxMessageBox("Non è selezionata nessuna riga!\n", MB_OK | MB_ICONINFORMATION);
		return FALSE;
	}
	CTreeBodyEditNodeInfo ni(GetTreeData(nRow));
	return ni.HasChild();
}

//------------------------------------------------------------------------------
BOOL DBTTree::CanDoCollapseNode(int nRow)
{
	if(nRow < 0 || nRow > GetUpperBound())
	{
//		if (m_bShowTreeMsg)
//			AfxMessageBox("Non è selezionata nessuna riga!\n", MB_OK | MB_ICONINFORMATION);
		return FALSE;
	}
	CTreeBodyEditNodeInfo ni(GetTreeData(nRow));
	return ni.HasChild();
}

//-----------------------------------------------------------------------------
BOOL DBTTree::CanDoSortAll()
{
	if (m_pDocument == NULL || (m_pDocument->GetFormMode() != CBaseDocument::NEW &&
								m_pDocument->GetFormMode() != CBaseDocument::EDIT))
		return FALSE;

	return TRUE;
}

//-----------------------------------------------------------------------------
BOOL DBTTree::CanDoSortNode()
{
	if (m_pDocument == NULL || (m_pDocument->GetFormMode() != CBaseDocument::NEW &&
								m_pDocument->GetFormMode() != CBaseDocument::EDIT))
		return FALSE;

	int nRow = GetCurrentRowIdx();
	if(nRow < 0 || nRow > GetUpperBound())
		return FALSE;

	return TRUE;
}

//-----------------------------------------------------------------------------
BOOL DBTTree::CanDoSearch()
{
	if (m_pDocument == NULL || (m_pDocument->GetFormMode() != CBaseDocument::NEW &&
								m_pDocument->GetFormMode() != CBaseDocument::EDIT))
		return FALSE;

	if (IsEmpty())
		return FALSE;

	return TRUE;
}

//-----------------------------------------------------------------------------
void DBTTree::SetNodeExpand	(int nRow, CTreeBodyEditNodeInfo& ni)
{
	ni.ToggleExpand();
	GetTreeData(GetRow(nRow)) = ni.GetInfo();

	BuildTree();

	GetDocument()->SetModifiedFlag();

	m_nCurrentRow = nRow;
}

//-----------------------------------------------------------------------------
void DBTTree::SetAllExpand(BOOL bExpand, BOOL bBuildTree)
{
	for (int c = 0; c <= m_pAllRecords->GetUpperBound(); c++)
	{
		CTreeBodyEditNodeInfo ni(GetAllTreeData(c));
		if (ni.HasChild())
		{
			ni.SetExpanded(bExpand);
			GetAllTreeData(c) = ni.GetInfo();
		}
	}

	if (bBuildTree)
	{
		BuildTree();

		GetDocument()->SetModifiedFlag();
	}
}

//-----------------------------------------------------------------------------
void DBTTree::SetNodeExpand(int nRow, BOOL bExpand, BOOL bBuildTree)
{
	int nIdx = RemapIndexF2A(nRow);

	int	nParentLevel = -1;
	int c = nIdx;
	for (; c < m_pAllRecords->GetUpperBound(); c++)
	{
		CTreeBodyEditNodeInfo ni(GetAllTreeData(c));
		if (c == nIdx && !ni.HasChild())
			return;

		if (nParentLevel == -1)
		{
			nParentLevel = ni.GetLevel();
		}
		else if (nParentLevel >= ni.GetLevel())
			break;	// se il livello e' <= del padre ho finito...

		if (ni.HasChild())
		{
			ni.SetExpanded(bExpand);
			GetAllTreeData(c) = ni.GetInfo();
		}
	}

	if (bBuildTree)
	{
		GetDocument()->SetModifiedFlag();

	//TODO
		//allineamento m_pRecord per abilitare ricostruzione tree limitata alle righe coinvolte nell'espansione del solo ramo coinvolto
		//if (c <= m_pAllRecords->GetUpperBound())
		//{
		//	SqlRecord* pRec = m_pAllRecords->GetAt(c);
		//	int j = nRow + 1;
		//	for (; j <= m_pRecords->GetUpperBound(); j++)
		//	{
		//		if (m_pRecords->GetAt(j) == pRec)
		//			break;
		//	}
		//	ASSERT(j <= m_pRecords->GetUpperBound());
		//	m_pRecords->RemoveAt(nRow, j - nRow + 1 );
		//}
		//else
		//{
		//	m_pRecords->RemoveAt(nRow, m_pRecords->GetSize() - nRow);
		//	//m_pRecords->SetSize(nRow);
		//}
		//ricostruzione parziale
		//BuildTree(nRow, c);

	//ELSE
		BuildTree();
	//----
	}
}

//-----------------------------------------------------------------------------
//TODO da testare
// Ricreo una porzione della struttura ad albero partendo dai records...
void DBTTree::BuildTree(int start, int end)
{
	ASSERT(m_pAllRecords);	// prima deve essere chiamata la InitializeTree
	ASSERT(!m_pRecords->IsOwnsElements());

	for (int c = start; c <= min(end, m_pAllRecords->GetUpperBound()); c++)
	{
		SqlRecord*	pRec = m_pAllRecords->GetAt(c);
		CTreeBodyEditNodeInfo ni(GetTreeData(pRec));

		m_pRecords->Add(pRec);

		if (ni.HasChild() && !ni.IsExpanded())
		{
			for (c++; c <= min(end, m_pAllRecords->GetUpperBound()); c++)
			{
				CTreeBodyEditNodeInfo cni(GetAllTreeData(c));
				if (ni.GetLevel() >= cni.GetLevel())
				{
					c--;
					break;
				}
			}
		}
	}

	if (m_bCheckDuplicateKey)
	{
	     for (int i = start; i <= min(end, GetUpperBound()); i++)
            OnPreparePrimaryKey(i, GetRow(i));
	}
}

//-----------------------------------------------------------------------------
// Crea la struttura ad albero partendo dai records...
void DBTTree::BuildTree()
{
	ASSERT(m_pAllRecords);	// prima deve essere chiamata la InitializeTree

	// svuoto il buffer corrente
	ASSERT(!m_pRecords->IsOwnsElements());
	m_pRecords->RemoveAll(); //TODO verificare se serve la RemoveAllRecords();

	// comincio la costruzione...
	for (int c = 0; c <= m_pAllRecords->GetUpperBound(); c++)
	{
		SqlRecord*	pRec = m_pAllRecords->GetAt(c);
		CTreeBodyEditNodeInfo ni(GetTreeData(pRec));
		m_pRecords->Add(pRec);

		if (ni.HasChild() && !ni.IsExpanded())
		{
			for (c++; c <= m_pAllRecords->GetUpperBound(); c++)
			{
				CTreeBodyEditNodeInfo cni(GetAllTreeData(c));
				if (ni.GetLevel() >= cni.GetLevel())
				{
					c--;
					break;
				}
			}
		}
	}

	if (m_bCheckDuplicateKey)
	{
	     for (int i = 0; i <= GetUpperBound(); i++)
            OnPreparePrimaryKey(i, GetRow(i));
	}
}

//-----------------------------------------------------------------------------
int	DBTTree::GetMaxLevel()
{
	int	nMaxLevel = -1;
	ASSERT_VALID(m_pAllRecords);
	if (!m_pAllRecords)
		for (int c = 0; c <= m_pAllRecords->GetUpperBound(); c++)
		{
			CTreeBodyEditNodeInfo		ni(GetAllTreeData(c));

			if (ni.GetLevel() > nMaxLevel)
				nMaxLevel = ni.GetLevel();
		}
	return nMaxLevel;
}

//-----------------------------------------------------------------------------
int	DBTTree::GetParent(int nRow)
{
	CTreeBodyEditNodeInfo n(GetTreeData(nRow));
	for (int c = nRow - 1; c >= 0; c--)
	{
		CTreeBodyEditNodeInfo ni(GetTreeData(c));
		if (ni.GetLevel() < n.GetLevel())
			return c;
	}

	return -1;
}

//-----------------------------------------------------------------------------
SqlRecord* DBTTree::InsertRecord(int nRow)
{
	if (
		(m_bAllowFilter && (nRow < 0 || nRow > GetUpperBound())) 
		||
		(!m_bAllowFilter && (nRow < 0 || nRow > GetUnfilteredUpperBound()))
		)
	{
		return DBTSlaveBuffered::AddRecord();
	}

	SqlRecord* pRec = DBTSlaveBuffered::InsertRecord(nRow);

	int	nIdx = RemapIndexF2A(nRow + 1);

	m_bAllowFilter = FALSE; 
	RecordArray* pTemp = m_pRecords;
	m_pRecords = m_pAllRecords;

	CTreeBodyEditNodeInfo ni(GetTreeData(pRec));

	// aggiorno il campo PARENT delle righe successive
	for (int c = nIdx; c <= m_pRecords->GetUpperBound(); c++)
	{
		SqlRecord*	pNextRec = m_pRecords->GetAt(c);
		CTreeBodyEditNodeInfo cni(GetTreeData(pNextRec));

		if (cni.GetParent() < (nIdx - 1) || cni.GetParent() == 0)
			continue;

		cni.SetParent(cni.GetParent() + 1);
		GetTreeData(pNextRec) = cni.GetInfo();
	}

	m_pRecords = pTemp;
	m_bAllowFilter = TRUE;

	return pRec;
}

//-----------------------------------------------------------------------------
BOOL DBTTree::DeleteRecord(int nRow)
{
	SqlRecord* pRec = m_pRecords->GetAt(nRow);
	CTreeBodyEditNodeInfo ni(GetTreeData(pRec));

	//visualizzo eventuali figli
	if (ni.HasChild())
	{
		SetNodeExpand(nRow, TRUE, TRUE);
		ni.SetExpanded(TRUE);
	}

	//indice della prima riga che sarà cancellata
	int nAllIdx = RemapIndexF2A(nRow);

	CArray <int, int> arRows2Deleted;
	arRows2Deleted.Add(nRow);
	//identifico eventuali figli da cancellare
	if (ni.HasChild())
	{
		for (int nIdx = nRow + 1; nIdx <= m_pRecords->GetUpperBound(); nIdx++)
		{
			SqlRecord*	pChildRec = m_pRecords->GetAt(nIdx);
			CTreeBodyEditNodeInfo cni(GetTreeData(pChildRec));
			// se ho finito con i figli esco...
			if (ni.GetLevel() >= cni.GetLevel())
			{
				ASSERT(arRows2Deleted.GetSize() > 1);
				break;
			}
			arRows2Deleted.Add(nIdx);
		}
	}

	//cancello le righe al contrario per mantenere gli indici coerenti
	BOOL bRetVal = TRUE;
	int nDeleted = 0;
	for (int i = arRows2Deleted.GetUpperBound(); i >= 0; i--)
	{
		int j = arRows2Deleted.GetAt(i);
		bRetVal = DBTSlaveBuffered::DeleteRecord(j);
		if (!bRetVal) break;
		nDeleted++;
	}

	m_bAllowFilter = FALSE; 
    RecordArray* pTemp = m_pRecords;
    m_pRecords = m_pAllRecords;

	if (nDeleted)
	{
	   // aggiorno il campo PARENT delle righe successive
		for (int c = nAllIdx; c <= m_pRecords->GetUpperBound(); c++)
		{
			SqlRecord* pRec = m_pRecords->GetAt(c);
			CTreeBodyEditNodeInfo cni(GetTreeData(c));

			int parent = cni.GetParent();
			if (parent < nAllIdx)
				continue;
			
			if (parent) 
				parent -= nDeleted;
			if (parent < 0) 
			{ 
				ASSERT(FALSE); 
				parent = 0;
			} 

			cni.SetParent(parent);

			ASSERT (cni.GetParent() >= 0 && cni.GetParent() <= m_pRecords->GetUpperBound());
			
			GetTreeData(c) = cni.GetInfo();
		}
 
		// se il nodo cancellato ha un nodo padre...
		// aggiorno il campo HASCHILD del padre
		if (ni.GetParent() > 0)
		{
			// se il nodo padre ha una riga successiva...
			if (ni.GetParent() <= m_pRecords->GetUpperBound())
			{
				SqlRecord* pParentRec = m_pRecords->GetAt(ni.GetParent()-1);

				CTreeBodyEditNodeInfo ni_Parent(GetTreeData(pParentRec));

				SqlRecord* pNextRec = m_pRecords->GetAt(ni.GetParent());

				CTreeBodyEditNodeInfo ni_Next(GetTreeData(pNextRec));

				if (ni_Next.GetLevel() <= ni_Parent.GetLevel())
				{
					// il nodo padre non ha più figli
					ni_Parent.SetHasChild(FALSE);
					GetTreeData(pParentRec) = ni_Parent.GetInfo(); 
				}
				else //if (ni_Next.GetLevel() > ni_Parent.GetLevel())
				{
					// il nodo padre ha altri figli
					ASSERT(ni_Parent.HasChild()); 
				}
			}
			else
			{
				SqlRecord* pParentRec = m_pRecords->GetAt(ni.GetParent()-1);
				CTreeBodyEditNodeInfo ni_Parent(GetTreeData(pParentRec));
				// il nodo padre non ha più figli
				ni_Parent.SetHasChild(FALSE);
				GetTreeData(pParentRec) = ni_Parent.GetInfo(); 
			}
		}
	}

    m_pRecords = pTemp;
    m_bAllowFilter = TRUE;

	return bRetVal;
}

//-----------------------------------------------------------------------------
BOOL DBTTree::FindData(BOOL bPrepareOld /*= TRUE*/)
{
	BOOL bOk = DBTSlaveBuffered::FindData(bPrepareOld);

	BuildTree();

	return bOk;
}

//-----------------------------------------------------------------------------
void DBTTree::OnPrepareRow(int nRow, SqlRecord* pRec)
{
	ASSERT_VALID(pRec);
	CTreeBodyEditNodeInfo CurrRec_ni;

	if (nRow == 0)
	{
		CurrRec_ni.SetLevel(1);
	}
	else
	{
		CTreeBodyEditNodeInfo PrevRec_ni(GetTreeData(nRow - 1));

		CurrRec_ni.SetLevel		(PrevRec_ni.GetLevel());
		CurrRec_ni.SetParent	(PrevRec_ni.GetParent());
		CurrRec_ni.SetIDB		(PrevRec_ni.GetIDB());
	}

	ASSERT(pRec == GetRow(nRow));
	GetTreeData(pRec) = CurrRec_ni.GetInfo();
}

//-----------------------------------------------------------------------------
void DBTTree::SetTreeIcon(SqlRecord* pSqlRec, int nIdxBmp)
{   
	DataStr& dsref = GetTreeData(pSqlRec);
	CTreeBodyEditNodeInfo ni(dsref);

	ni.SetIDB(nIdxBmp);
	dsref = ni.GetInfo();
}

//-----------------------------------------------------------------------------
BOOL DBTTree::HasChild(SqlRecord* pSqlRec)
{   
	DataStr& dsref = GetTreeData(pSqlRec);
	CTreeBodyEditNodeInfo ni(dsref);

	return ni.HasChild();
}

//-----------------------------------------------------------------------------
BOOL DBTTree::IsExpanded(SqlRecord* pSqlRec)
{   
	DataStr& dsref = GetTreeData(pSqlRec);
	CTreeBodyEditNodeInfo ni(dsref.GetString());

	return ni.IsExpanded();
}

//-----------------------------------------------------------------------------
int DBTTree::GetLevel(SqlRecord* pSqlRec)
{   
	DataStr& dsref = GetTreeData(pSqlRec);
	CTreeBodyEditNodeInfo ni(dsref.GetString());

	return ni.GetLevel();
}

//-----------------------------------------------------------------------------
int DBTTree::GetRowIndex(SqlRecord* pSqlRec)
{   
	ASSERT(m_pAllRecords);
	for (int i = 0; i <= this->m_pAllRecords->GetUpperBound(); i++)
	{
		if (pSqlRec == this->m_pAllRecords->GetAt(i))
		{
			return i; 
		}
	}
	return -1;
}

//-----------------------------------------------------------------------------
int	DBTTree::GetParentAllRecords(int nRow)
{
	if (nRow < 0) return -1;
	CTreeBodyEditNodeInfo n(GetAllTreeData(nRow));
	for (int c = nRow - 1; c >= 0; c--)
	{
		CTreeBodyEditNodeInfo ni(GetAllTreeData(c));
		if (ni.GetLevel() < n.GetLevel())
			return c;
	}
	return -1;
}

//-----------------------------------------------------------------------------
SqlRecord* DBTTree::GetParent(SqlRecord* pSqlRec)
{   
	ASSERT(m_pAllRecords);
	
	int idxRec = GetRowIndex(pSqlRec);
	if (idxRec <= 0) 
		return NULL;
	int idxParent = GetParentAllRecords(idxRec);
	if (idxParent < 0)
		return NULL;
	return m_pAllRecords->GetAt(idxParent);
}

//-----------------------------------------------------------------------------
//Se viene chiamata con NULL ritorna il primo figlio
//richiamandola con un altro record ritorna il discendente successivo
BOOL DBTTree::GetChild(SqlRecord* pRecParent, SqlRecord*& pRecChild)
{
	ASSERT(m_pAllRecords);

	CTreeBodyEditNodeInfo ni(GetTreeData(pRecParent));
	int idx = GetRowIndex(pRecParent);

	// scorro fino all'ultimo figlio...
	BOOL bFound = FALSE;
	int c;
	for (c = idx + 1; c <= m_pAllRecords->GetUpperBound(); c++)
	{
		CTreeBodyEditNodeInfo cni(GetAllTreeData(c));

		if (ni.GetLevel() >= cni.GetLevel())
		{
			break;
		}
		if (pRecChild == NULL)
		{
			pRecChild = m_pAllRecords->GetAt(c);
			return TRUE;
		}

		if (!bFound)
		{
			bFound = (pRecChild == m_pAllRecords->GetAt(c));
			continue;
		}
		pRecChild = m_pAllRecords->GetAt(c);
		return TRUE;
	}
	return FALSE;
}

//-----------------------------------------------------------------------------
BOOL DBTTree::GetChilds(SqlRecord* pRecParent, RecordArray& Ar)
{
	ASSERT(m_pAllRecords);

	CTreeBodyEditNodeInfo ni(GetTreeData(pRecParent));
	int idx = GetRowIndex(pRecParent);

	int s =  Ar.GetSize();
	// scorro fino all'ultimo figlio...
	int c;
	for (c = idx + 1; c <= m_pAllRecords->GetUpperBound(); c++)
	{
		CTreeBodyEditNodeInfo cni(GetAllTreeData(c));

		if (ni.GetLevel() >= cni.GetLevel())
		{
			break;
		}

		Ar.Add(m_pAllRecords->GetAt(c));
	}
	return Ar.GetSize() > s;
}

//-----------------------------------------------------------------------------
void DBTTree::PrepareTreeData1()
{
	RecordArray* pRecords = m_pAllRecords ? m_pAllRecords : m_pRecords;
	if (pRecords->GetCount() == 0)
		return;

	SqlRecord* pParentRec (NULL);
	int nIdxParent = -1;
	for (int r = 0; r < pRecords->GetCount(); r++)
	{
		SqlRecord* pRow = (*pRecords)[r];

		if (!pParentRec || !GetTreeEdit(pParentRec).IsEqual(GetTreeEdit(pRow)))
		{
			pParentRec = CreateRecord();

			pParentRec->SetNeverStorable(TRUE);
			pParentRec->SetDataHideAndReadOnly(TRUE, this->m_nTreeEditIdx);

			CTreeBodyEditNodeInfo parent_ni;
				parent_ni.SetLevel(1);
				parent_ni.SetHasChild(1);
				parent_ni.SetExpanded(1);
			GetTreeData(pParentRec) = parent_ni.GetInfo();

			GetTreeEdit(pParentRec).Assign(GetTreeEdit(pRow));
			GetTreeEdit(pParentRec).SetAlwaysReadOnly();
#ifdef _DEBUG
			GetTreeData(pParentRec).SetHide(FALSE);
#endif
			pRecords->InsertAt(r, (CObject*)pParentRec, 1);
			nIdxParent = r;
		}

		CTreeBodyEditNodeInfo row_ni;
			row_ni.SetLevel(2);
			row_ni.SetParent(nIdxParent + 1);
		GetTreeData(pRow) = row_ni.GetInfo();

		GetTreeEdit(pRow).SetAlwaysReadOnly();
		GetTreeEdit(pRow).SetHide();
	}
}

//-----------------------------------------------------------------------------
void DBTTree::DoAfterInsertRow1(int /*nRow*/, SqlRecord* pRec)
{   
	CTreeBodyEditNodeInfo ni(GetTreeData(pRec));

	if (ni.GetLevel() == 1)
	{
		pRec->SetNeverStorable(TRUE);
		pRec->SetDataHideAndReadOnly(TRUE, this->m_nTreeEditIdx);
#ifdef _DEBUG
		GetTreeData(pRec).SetHide(FALSE);
#endif
	}
	else
	{
		int nParent = ni.GetParent();
		SqlRecord* pParentRec = GetRow(nParent);
		ASSERT_VALID(pParentRec);
		ASSERT(!GetTreeEdit(pParentRec).IsEmpty());

		DataObj& pTreeEdit = GetTreeEdit(pRec);
			pTreeEdit.Assign(GetTreeEdit(pParentRec));
			pTreeEdit.SetAlwaysReadOnly();
			pTreeEdit.SetHide();
	}
}

//-----------------------------------------------------------------------------
BOOL DBTTree::GetRowsByLevel(RecordArray& ar, int level)
{
	ASSERT_VALID(m_pAllRecords);
	if (!m_pAllRecords)
		return FALSE;

	ASSERT(ar.GetCount() == 0);
	ar.RemoveAll();
	ar.SetOwns(FALSE);

	for (int c = 0; c <= m_pAllRecords->GetUpperBound(); c++)
	{
		CTreeBodyEditNodeInfo cni(GetAllTreeData(c).GetString());

		if (cni.GetLevel() == level)
		{
			ar.Add(m_pAllRecords->GetAt(c));
		}
	}
	return ar.GetSize() > 0;
}

//=============================================================================
