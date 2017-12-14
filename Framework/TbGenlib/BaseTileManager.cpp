#include "stdafx.h"
#include "BaseTileManager.h"
#include "BaseTileDialog.h"
#include "TABCORE.H"
#include "OslBaseInterface.h"


//includere come ultimo include all'inizio del cpp
#include "begincpp.dex"

#ifdef _DEBUG
#undef THIS_FILE
static const char THIS_FILE[] = __FILE__;
#endif



//======================================================================
class StaticAreaInfo : public CObject
{
public:
	int	 m_nTileGroupClientLeft;
	int	 m_nrOfStaticArea;
	int	 m_nOwnerTileIdx;

	//------------------------------------------------------------------------------
	StaticAreaInfo(int nTileGroupClientLeft, int nOwnerTileIdx, int nrOfStaticArea = 0)
	{
		m_nTileGroupClientLeft = nTileGroupClientLeft;
		m_nOwnerTileIdx = nOwnerTileIdx;
		m_nrOfStaticArea = nrOfStaticArea;
	}
};

//======================================================================
// array di static area info
class StaticAreasColumnInfo : public Array
{
	friend class StaticAreasAligner;

	int		m_nTileGroupClientLeftToAlign;
	int		m_nColumn;
	int		m_bNeedAlignment;

	StaticAreasColumnInfo(int nTileGroupClientLeftToAlign, int nColumn)
	{
		m_nColumn = nColumn;
		m_nTileGroupClientLeftToAlign = 0;
		m_bNeedAlignment = FALSE;
	}

	BOOL IsInColumn(int nTileGroupClientLeft, int nTolerance)
	{
		int nMin = m_nTileGroupClientLeftToAlign - nTolerance;
		int nMax = m_nTileGroupClientLeftToAlign + nTolerance;

		return nTileGroupClientLeft == m_nTileGroupClientLeftToAlign || (nTileGroupClientLeft >= nMin && nTileGroupClientLeft <= nMax);
	}

	// questa evita di registrare due volte la struttura di uno stesso elemento
	//------------------------------------------------------------------------------
	StaticAreaInfo* GetInfo(int nTileGroupClientLeft, int nOwnerTileIdx, int nrOfStaticArea = 0)
	{
		StaticAreaInfo* pInfo = NULL;
		// aggiunge la info solo se non l'ha già fatto
		for (int i = 0; i < GetSize(); i++)
		{
			pInfo = dynamic_cast<StaticAreaInfo*>(GetAt(i));
			if (pInfo->m_nOwnerTileIdx == nOwnerTileIdx && pInfo->m_nrOfStaticArea == nrOfStaticArea)
				return pInfo;
		}

		pInfo = new StaticAreaInfo(nTileGroupClientLeft, nOwnerTileIdx, nrOfStaticArea);
		Add(pInfo);
		return pInfo;
	}

	//------------------------------------------------------------------------------
	BOOL NeedAlignment() const
	{
		return m_bNeedAlignment;
	}

	//--------------------------------------------------------------------------------
	BOOL HasRightTileOnSameRow(TileDialogArray* pTiles, int nTop, int nTolerance, CTilePanel* pLeftPanel, BOOL& bSamePanel)
	{
		bSamePanel = FALSE;

		for (int i = 0; i < GetSize(); i++)
		{
			StaticAreaInfo* pInfo = dynamic_cast<StaticAreaInfo*>(GetAt(i));

			if (!pInfo || pInfo->m_nrOfStaticArea > 0)
				continue;

			CBaseTileDialog* pTileDialog = dynamic_cast<CBaseTileDialog*>(pTiles->GetAt(pInfo->m_nOwnerTileIdx));

			if (!pTileDialog)
				continue;

			CRect aRect;

			CWnd* pParent = pTileDialog->GetParent();

			if (pParent->IsKindOf(RUNTIME_CLASS(CTilePanelTab)))
			{
				CTilePanelTab* pTilePanelTab = dynamic_cast<CTilePanelTab*>(pParent);
				CTilePanel* pTilePanel = pTilePanelTab->GetTilePanel();

				if (pTilePanel == pLeftPanel)
				{
					bSamePanel = TRUE;
					return TRUE;
				}

				pTilePanel->GetWindowRect(aRect);
				pTileDialog->GetParentTileGroup()->ScreenToClient(aRect);

				if (!IsInColumn(aRect.left, nTolerance))
				{
					pTileDialog->GetWindowRect(aRect);
					pTileDialog->GetParentTileGroup()->ScreenToClient(aRect);
				}
			}
			else
			{
				pTileDialog->GetWindowRect(aRect);
				pTileDialog->GetParentTileGroup()->ScreenToClient(aRect);
			}

			if (aRect.top == nTop)
				return TRUE;
		}
		return FALSE;
	}

	//-----------------------------------------------------------------------------------
	void AdjustPreviousColumn(TileDialogArray* pTiles, int nEnlargeTo, int spacing, StaticAreasColumnInfo* pCurrentColumn, int nTolerance)
	{
		for (int i = 0; i < GetSize(); i++)
		{
			StaticAreaInfo* pInfo = dynamic_cast<StaticAreaInfo*>(GetAt(i));

			if (!pInfo)
				continue;

			CBaseTileDialog* pTileDialog = dynamic_cast<CBaseTileDialog*>(pTiles->GetAt(pInfo->m_nOwnerTileIdx));

			if (!pTileDialog)
				continue;

			//allarga fino al m_nTileGroupClientLeftToAlign - spacing
			CRect aRect;

			CWnd*		pParent = pTileDialog->GetParent();
			CTilePanel* pTilePanel = NULL;

			if (pParent->IsKindOf(RUNTIME_CLASS(CTilePanelTab)))
			{
				CTilePanelTab* pTilePanelTab = dynamic_cast<CTilePanelTab*>(pParent);
				pTilePanel = pTilePanelTab->GetTilePanel();
				pTilePanel->GetWindowRect(aRect);
				pTileDialog->GetParentTileGroup()->ScreenToClient(aRect);

				if (!IsInColumn(aRect.left, nTolerance))
				{
					pTileDialog->GetWindowRect(aRect);
					pTileDialog->GetParentTileGroup()->ScreenToClient(aRect);
				}
			}
			else
			{
				pTileDialog->GetWindowRect(aRect);
				pTileDialog->GetParentTileGroup()->ScreenToClient(aRect);
			}
			//devo verificare se c'e' qualche tile a destra (prossima colonna) per sapere se faccio qualcosa o no
			//se non c'e' => non faccio niente (da sola sulla riga)
			BOOL bSamePanel = FALSE;
			BOOL bHasRightTileOnSameRow = pCurrentColumn && pCurrentColumn->HasRightTileOnSameRow(pTiles, aRect.top, nTolerance, pTilePanel, bSamePanel);

			if (!bHasRightTileOnSameRow)
				continue;

			aRect.right = nEnlargeTo - spacing;

			if (bSamePanel)
			{
				//sposto la tile 
				pTileDialog->GetWindowRect(aRect);
				pTileDialog->GetParent()->ScreenToClient(aRect);
				aRect.right = nEnlargeTo - spacing;
				pTileDialog->SetWindowPos(NULL, aRect.left, aRect.top, aRect.Width(), aRect.Height(), /*SWP_NOMOVE | */SWP_NOZORDER | SWP_NOACTIVATE);
			}
			else if (pTilePanel)
			{
				pTilePanel->GetWindowRect(aRect);
				pTileDialog->GetParentTileGroup()->ScreenToClient(aRect);
				aRect.right = nEnlargeTo - spacing;
				pTilePanel->SetWindowPos(NULL, aRect.left, aRect.top, aRect.Width(), aRect.Height(), /*SWP_NOMOVE | */SWP_NOZORDER | SWP_NOACTIVATE);
			}
			else
			{
				pTileDialog->GetWindowRect(aRect);
				pTileDialog->GetParentTileGroup()->ScreenToClient(aRect);
				aRect.right = nEnlargeTo - spacing;
				pTileDialog->SetWindowPos(NULL, aRect.left, aRect.top, aRect.Width(), aRect.Height(), /*SWP_NOMOVE | */SWP_NOZORDER | SWP_NOACTIVATE);
			}


		}
	}

	//------------------------------------------------------------------------------
	void AlignColumn(TileDialogArray* pTiles, BOOL& bAdjustPreviousColumn, int nTolerance, int nSpacing)
	{
		// recupera realmente lo spazio
		for (int i = 0; i < GetSize(); i++)
		{
			StaticAreaInfo* pInfo = dynamic_cast<StaticAreaInfo*>(GetAt(i));
			// e' già posizionata giusta
			int nDelta = pInfo->m_nTileGroupClientLeft - m_nTileGroupClientLeftToAlign;

			if (nDelta == 0 || pInfo->m_nOwnerTileIdx < 0)
				continue;

			CBaseTileDialog* pTileDialog = dynamic_cast<CBaseTileDialog*>(pTiles->GetAt(pInfo->m_nOwnerTileIdx));

			if (!pTileDialog)
				continue;

			if (pTileDialog->HasParts())
			{
				// e' una wide e devo spostare la seconda static area 
				// lo spostamento va in senso contrario
				if (pInfo->m_nrOfStaticArea > 0)
					pTileDialog->ApplyDeltaToSecondStaticArea(nDelta * (-1));
			}
			else
			{
				// e' una tile dialog senza le parts
				bAdjustPreviousColumn = TRUE;

				CRect aRect;

				CWnd* pParent = pTileDialog->GetParent();

				if (pParent->IsKindOf(RUNTIME_CLASS(CTilePanelTab)))
				{
					CTilePanelTab*	pTilePanelTab = dynamic_cast<CTilePanelTab*>(pParent);
					CTilePanel*		pTilePanel = pTilePanelTab->GetTilePanel();

					if (pTilePanel)
					{
						pTilePanel->GetWindowRect(aRect);
						pTileDialog->GetParentTileGroup()->ScreenToClient(aRect);

						//testo se devo muovere il panel oppure solo le tiles
						if (IsInColumn(aRect.left, nTolerance))
						{
							//allora devo muovere il panello
							int nWidth = aRect.Width() + nDelta + nSpacing;
							aRect.left = m_nTileGroupClientLeftToAlign;
							pTilePanel->SetWindowPos(NULL, aRect.left, aRect.top, nWidth, aRect.Height(),  /*SWP_NOMOVE | */SWP_NOZORDER | SWP_NOACTIVATE);
						}
						else
						{
							//altrimento muovo la tile
							pTileDialog->GetWindowRect(aRect);
							pParent->ScreenToClient(aRect);
							int nWidth = aRect.Width() + nDelta;
							aRect.left = m_nTileGroupClientLeftToAlign;

							pTileDialog->SetWindowPos(NULL, aRect.left, aRect.top, nWidth, aRect.Height(), /*SWP_NOMOVE | */SWP_NOZORDER | SWP_NOACTIVATE);
						}
					}
				}
				else
				{
					pTileDialog->GetWindowRect(aRect);
					pParent->ScreenToClient(aRect);
					int nWidth = aRect.Width() + nDelta;
					aRect.left = m_nTileGroupClientLeftToAlign;

					pTileDialog->SetWindowPos(NULL, aRect.left, aRect.top, nWidth, aRect.Height(), /*SWP_NOMOVE | */SWP_NOZORDER | SWP_NOACTIVATE);
				}
			}
		}

	}
};

//======================================================================
// array di colonne di static areas
class StaticAreasAligner : public Array
{
	int m_nMaxColumns;
	int m_nSpaceTolerance;
	int m_nSpacing;

public:
	//------------------------------------------------------------------------------
	StaticAreasAligner()
	{
		// tolleranza di spaziatura per l'assegnazione delle colonne
		m_nSpacing = AfxGetThemeManager()->GetTileSpacing();
		m_nSpaceTolerance = CBaseTileDialog::GetTileWidth(TILE_MINI) / 2;

		Clear();
	}

	//------------------------------------------------------------------------------
	void Clear()
	{
		m_nMaxColumns = 0;
		RemoveAll();
	}

	//------------------------------------------------------------------------------
	void TryToAlign(CBaseTileGroup* pGroup)
	{
		Clear();

		CalculateStaticAreaInfo(pGroup);
		if (!m_nMaxColumns)
			return;

		for (int i = 1; i <= m_nMaxColumns; i++)
		{
			BOOL bAdjustPreviousColumn = FALSE;
			StaticAreasColumnInfo* pColumnInfo = GetColumnInfo(i);
			if (pColumnInfo && pColumnInfo->NeedAlignment())
			{
				pColumnInfo->AlignColumn(pGroup->GetTileDialogs(), bAdjustPreviousColumn, m_nSpaceTolerance, m_nSpacing);

				if (bAdjustPreviousColumn && i > 1)
				{
					StaticAreasColumnInfo* pPreviousColumnInfo = GetColumnInfo(i - 1);
					if (pPreviousColumnInfo)
						pPreviousColumnInfo->AdjustPreviousColumn(pGroup->GetTileDialogs(), pColumnInfo->m_nTileGroupClientLeftToAlign, m_nSpacing, pColumnInfo, m_nSpaceTolerance);
				}
			}
		}
	}

private:
	//------------------------------------------------------------------------------
	StaticAreasColumnInfo* GetColumnInfo(int nColumn)
	{
		// provo ad andare secca sul'elemento e controllo che sia lui
		StaticAreasColumnInfo* pInfo = dynamic_cast<StaticAreasColumnInfo*>(GetAt(nColumn - 1));
		if (pInfo && pInfo->m_nColumn == nColumn)
			return pInfo;

		// se non e' lei la cerco nell'array
		for (int i = 0; i < GetSize(); i++)
		{
			pInfo = dynamic_cast<StaticAreasColumnInfo*>(GetAt(i));
			if (pInfo && pInfo->m_nColumn == nColumn)
				return pInfo;
		}

		return NULL;
	}

	//------------------------------------------------------------------------------
	StaticAreasColumnInfo* GetColumnInfoByCoord(int nTileGroupClientLeft)
	{
		// ne approfitto per calcolare e tenere aggiornato il maxColumns
		m_nMaxColumns = 0;
		StaticAreasColumnInfo* pFound = NULL;
		for (int i = 0; i < GetSize(); i++)
		{
			StaticAreasColumnInfo* pColumnInfo = dynamic_cast<StaticAreasColumnInfo*>(GetAt(i));
			if (!pColumnInfo)
				continue;

			// aggiorno il max column
			if (m_nMaxColumns < pColumnInfo->m_nColumn)
				m_nMaxColumns = pColumnInfo->m_nColumn;

			// mi segno la colonna da tornare
			if (!pFound && pColumnInfo->IsInColumn(nTileGroupClientLeft, m_nSpaceTolerance))
				pFound = pColumnInfo;
		}

		// ho lasciato finire tutto per allineare bene MaxColumns
		if (pFound)
			return pFound;

		// creo la nuova colonna
		m_nMaxColumns++;
		pFound = new StaticAreasColumnInfo(nTileGroupClientLeft, m_nMaxColumns);
		Add(pFound);

		return pFound;
	}

	//------------------------------------------------------------------------------
	void AssignBestColumn(int nTileGroupClientLeft, int nOwnerTileIdx, int nrOfStaticArea = 0)
	{
		StaticAreasColumnInfo* pColumnInfo = GetColumnInfoByCoord(nTileGroupClientLeft);
		ASSERT(pColumnInfo);

		StaticAreaInfo* pInfo = pColumnInfo->GetInfo(nTileGroupClientLeft, nOwnerTileIdx, nrOfStaticArea);

		// nella colonna mi segno già il max left come left di allineamento colonnare per tutte 
		// che poi mi andrò ad usare quando allineerò
		if (!pColumnInfo->m_bNeedAlignment)
			pColumnInfo->m_bNeedAlignment = pInfo->m_nTileGroupClientLeft != pColumnInfo->m_nTileGroupClientLeftToAlign;

		if (pInfo->m_nTileGroupClientLeft > pColumnInfo->m_nTileGroupClientLeftToAlign)
			pColumnInfo->m_nTileGroupClientLeftToAlign = pInfo->m_nTileGroupClientLeft;
	}

	//------------------------------------------------------------------------------
	void CalculateStaticAreaInfo(CBaseTileGroup* pGroup)
	{
		// trova le static areas e le assegna ad una colonna virtuale
		CRect	aRect;

		for (int i = 0; i <= pGroup->GetTileDialogs()->GetUpperBound(); i++)
		{
			CBaseTileDialog* pTileDialog = dynamic_cast<CBaseTileDialog*>(pGroup->GetTileDialogs()->GetAt(i));

			if (!pTileDialog || !IsWindow(pTileDialog->m_hWnd))
				continue;

			pTileDialog->GetWindowRect(aRect);
			pGroup->ScreenToClient(aRect);
			AssignBestColumn(aRect.left, i);

			if (pTileDialog->HasParts())
			{
				aRect = pTileDialog->GetStaticAreaRect(1);
				AssignBestColumn(aRect.left, i, 1);
			}
		}
	}
};

/////////////////////////////////////////////////////////////////////////////
//					class CBaseTileGroup implementation
/////////////////////////////////////////////////////////////////////////////
//

static const TCHAR sTileGroup[] = _T("TileGroup");

BEGIN_MESSAGE_MAP(CBaseTileGroup, CWnd)
	ON_MESSAGE(UM_INITIALIZE_TILE_LAYOUT, OnInitializeLayout)
	ON_MESSAGE(UM_RECALC_CTRL_SIZE, OnRecalcCtrlSize)
	ON_MESSAGE(UM_VALUE_CHANGED, OnValueChanged)
	ON_MESSAGE(UM_CTRL_FOCUSED, OnCtrlFocused)
	ON_WM_SETFOCUS()
	ON_WM_ERASEBKGND()
	ON_WM_WINDOWPOSCHANGED()
END_MESSAGE_MAP()

IMPLEMENT_DYNAMIC(CBaseTileGroup, CWnd)

//------------------------------------------------------------------------------
CBaseTileGroup::CBaseTileGroup()
	:
	IDisposingSourceImpl(this),
	m_pLayoutContainer(NULL),
	m_pDocument(NULL),
	m_bFillEmptySpace(TRUE),
	m_pTileDialogStyle(NULL),
	m_bTransparent(FALSE),
	m_bSuspendedLayout(FALSE),
	m_bSuspendedResizeStaticArea(TRUE)
{
	m_pTileDialogStyle = TileStyle::Inherit(AfxGetTileDialogStyleNormal());

	EnableLayout();
	if (AfxGetApplicationContext()->IsActiveAccessibilityEnabled())
		EnableActiveAccessibility();
}

//------------------------------------------------------------------------------
CBaseTileGroup::~CBaseTileGroup()
{
	SAFE_DELETE(m_pLayoutContainer);

	for (int i = 0; i <= m_OwnedLayoutContainers.GetUpperBound(); i++)
	{
		CLayoutContainer* pLayoutContainer = m_OwnedLayoutContainers.GetAt(i);
		SAFE_DELETE(pLayoutContainer);
	}
	m_OwnedLayoutContainers.RemoveAll();

	SAFE_DELETE(m_pTileDialogStyle);

	for (int p = 0; p <= m_TilePanelArray.GetUpperBound(); p++)
		SAFE_DELETE(m_TilePanelArray.GetAt(p));
	m_TilePanelArray.RemoveAll();

	if (m_pProxy != NULL)
	{
		//force disconnect accessibility clients
		::CoDisconnectObject((IAccessible*)m_pProxy, NULL);
		m_pProxy = NULL;
	}
}

//-----------------------------------------------------------------------------
void CBaseTileGroup::EnableLayout(BOOL bEnable)
{
	SAFE_DELETE(m_pLayoutContainer);

	if (bEnable)
	{
		m_pLayoutContainer = new CLayoutContainer(this, m_pTileDialogStyle);
		m_pLayoutContainer->SetRequestedLastFlex(1);
	}
}

//-----------------------------------------------------------------------------
HRESULT CBaseTileGroup::get_accName(VARIANT varChild, BSTR *pszName)
{
	// TileGroup namespace begins with "TabDlg", use "TileGrp" instead to enhance disambiguation
	CString sNamespace = cwsprintf(_T("{0-%s}TileGrp"), GetNamespace().GetObjectName());
	*pszName = ::SysAllocString(sNamespace);

	return S_OK;
}

//-----------------------------------------------------------------------------
void CBaseTileGroup::RemoveAllTiles()
{
	for (int d = 0; d <= m_TileDialogArray.GetUpperBound(); d++)
		if (::IsWindow(m_TileDialogArray.GetAt(d)->GetSafeHwnd()))
			m_TileDialogArray.GetAt(d)->DestroyWindow();

	m_TileDialogArray.RemoveAll();

	for (int p = 0; p <= m_TilePanelArray.GetUpperBound(); p++)
		SAFE_DELETE(m_TilePanelArray.GetAt(p));

	m_TilePanelArray.RemoveAll();

	for (int i = 0; i <= m_OwnedLayoutContainers.GetUpperBound(); i++)
	{
		CLayoutContainer* pLayoutContainer = m_OwnedLayoutContainers.GetAt(i);
		SAFE_DELETE(pLayoutContainer);
	}
	m_OwnedLayoutContainers.RemoveAll();

	if (m_pLayoutContainer)
		m_pLayoutContainer->RemoveAllContainedElements();
}

//-----------------------------------------------------------------------------
void CBaseTileGroup::SetTileDialogStyle(TileStyle* pStyle)
{
	m_pTileDialogStyle->Assign(pStyle);
}

//-----------------------------------------------------------------------------
class TileGroupSiblings
{
public:
	TileGroupSiblings(HWND hParent, HWND hGroup) { m_hParent = hParent; m_hGroup = hGroup; m_nGroupIdx = -1; }

	HWND			m_hParent;
	HWND			m_hGroup;
	int				m_nGroupIdx;
	CArray<HWND>	m_arhChildren;
};

//-----------------------------------------------------------------------------
BOOL CALLBACK EnumWindowsProc(HWND hwnd, LPARAM lParam)
{
	CWnd* pWnd = CWnd::FromHandlePermanent(hwnd);
	if (
		!pWnd || !pWnd->GetParent() ||
		pWnd->GetParent()->m_hWnd != ((TileGroupSiblings*)lParam)->m_hParent || // solo i siblings
		!pWnd->IsWindowEnabled() ||
		(pWnd->GetStyle() & WS_VISIBLE) != WS_VISIBLE ||
		(pWnd->GetStyle() & WS_TABSTOP) != WS_TABSTOP
		)
		return TRUE;

	int idx = ((TileGroupSiblings*)lParam)->m_arhChildren.Add(hwnd);
	if (hwnd == ((TileGroupSiblings*)lParam)->m_hGroup)
		((TileGroupSiblings*)lParam)->m_nGroupIdx = idx;

	return TRUE;
}

//-----------------------------------------------------------------------------
BOOL CBaseTileGroup::SetNextControlFocus(CBaseTileDialog* pTileDialog, BOOL bBackward, CBaseTileDialog* pCurrTileDialog)
{
	int upperBound = m_TileDialogArray.GetUpperBound();
	if (pTileDialog == NULL && upperBound < 0)
		return FALSE;

	if (pTileDialog == NULL)
	{
		pTileDialog = m_TileDialogArray.GetAt(bBackward ? upperBound : 0);
		// per impedire la ricorsione se ritorna al punto di partenza
		if (pTileDialog == pCurrTileDialog)
		{
			//visto che non posso spostare il fuoco scateno in ogni caso un UpdateCtrlData sul ctrl che ho provato ad abbandonare
			CWnd *pWnd = CWnd::GetFocus();
			if (pWnd)
			{
				CParsedCtrl* pControl = ::GetParsedCtrl(pWnd);
				if (pControl)
					pControl->UpdateCtrlData(TRUE);
			}
			return FALSE;
		}


		CWnd* pFocusableChild = pTileDialog->GetWindow(GW_CHILD);
		int nFirstCandidateID = -1;
		int nCurrentID = -1;
		pFocusableChild = pFocusableChild ? pTileDialog->GetNextDlgTabItem(pFocusableChild, bBackward) : NULL;
		if (pFocusableChild)
		{
			nFirstCandidateID = pFocusableChild->GetDlgCtrlID();
			nCurrentID = -1;
			while (
				pFocusableChild != NULL
				&&
				nFirstCandidateID != nCurrentID               //se sono uguali, vuol dire che ha ciclato su tutti i controlli nella dialog
				&&
				(
					!pFocusableChild->IsWindowEnabled() ||
					!pFocusableChild->IsWindowVisible() ||
					pFocusableChild->IsKindOf(RUNTIME_CLASS(CStatic)) ||
					(dynamic_cast<CGridControlObj*>(pFocusableChild) == NULL && dynamic_cast<CParsedCtrl*>(pFocusableChild) == NULL)
					)
				)
			{
				pFocusableChild = pTileDialog->GetNextDlgTabItem(pFocusableChild, bBackward);
				nCurrentID = pFocusableChild->GetDlgCtrlID();
			}
		}

		if (!pFocusableChild || nFirstCandidateID == nCurrentID)
			return SetNextControlFocus(pTileDialog, bBackward, NULL);

		pFocusableChild->SetFocus();
		return TRUE;
	}

	// per evitare la ricorsione se si ritorna al punto di partenza
	if (pCurrTileDialog == NULL)
		pCurrTileDialog = pTileDialog;
	else
		if (pCurrTileDialog == pTileDialog)
			return FALSE;

	// cerco se ci sono fratelli del gruppo corrente
	TileGroupSiblings aTileGroupSiblings(GetParent()->m_hWnd, this->m_hWnd);
	EnumChildWindows(GetParent()->m_hWnd, EnumWindowsProc, (LPARAM)&aTileGroupSiblings);

	if (upperBound > 0)
	{
		CBaseTileDialog* pNextTileDialog = NULL;
		for (int i = 0; i <= upperBound; i++)
		{
			if (pTileDialog == m_TileDialogArray.GetAt(i))
			{
				int normalizedIndex = i;

				while (TRUE)
				{
					int newIndex = (bBackward ? normalizedIndex - 1 : normalizedIndex + 1);

					// devo saltare in un fratello del gruppo corrente ?
					if ((newIndex < 0 || newIndex > upperBound) && (aTileGroupSiblings.m_arhChildren.GetSize() > 1 || bBackward))
						break;

					// rimango nel gruppo corrente ciclando tra le sue tiles
					// calcolo indice modulo numero di dialog per spostarmi dall'ultima alla prima nel giro normale tab
					// e dalla prima all'ultima tile se sto facendo shift+tab 
					normalizedIndex = newIndex % (upperBound + 1);
					if (normalizedIndex == -1)
						normalizedIndex = upperBound;

					if (normalizedIndex == i)
						return FALSE;

					pNextTileDialog = m_TileDialogArray.GetAt(normalizedIndex);
					//Nel caso di TileDialog dentro CTilePanel, non basta controllare lo stato collapsed, perche' il TilePanel qunado viene "colassato" non propaga lo stile alle TileDialog
					//interne, ma le nasconde solamente
					if (IsTBWindowVisible(pNextTileDialog) && pNextTileDialog->IsDisplayed() && CParsedForm::SetTBFocus(pNextTileDialog, bBackward))
						return TRUE;
				}

				break;
			}
		}
	}

	// posso andare in fratello del gruppo corrente ?
	if (aTileGroupSiblings.m_arhChildren.GetSize() > 1)
	{
		int normalizedIndex = aTileGroupSiblings.m_nGroupIdx;

		while (TRUE)
		{
			int newIndex = (bBackward ? normalizedIndex - 1 : normalizedIndex + 1);

			if (newIndex < 0 || newIndex >= aTileGroupSiblings.m_arhChildren.GetSize())
				break;

			normalizedIndex = newIndex % aTileGroupSiblings.m_arhChildren.GetSize();
			if (normalizedIndex == -1)
				normalizedIndex = aTileGroupSiblings.m_arhChildren.GetSize() - 1;

			if (normalizedIndex == aTileGroupSiblings.m_nGroupIdx)
				return FALSE;

			CWnd* pWnd = CWnd::FromHandlePermanent(aTileGroupSiblings.m_arhChildren[normalizedIndex]);
			if (!pWnd)
				continue;

			if (pWnd->IsKindOf(RUNTIME_CLASS(CBaseTileGroup)))
			{
				if (((CBaseTileGroup*)pWnd)->SetNextControlFocus(NULL, bBackward, pCurrTileDialog))
					return TRUE;
			}
			else
				if (CParsedForm::SetTBFocus(pWnd, bBackward))
					return TRUE;
		}
	}

	// risalgo la catena dei parent
	CWnd* pParent = GetParent()->GetParent();
	CWnd* pFocusableChild = NULL;
	while (pParent != NULL && !pParent->IsKindOf(RUNTIME_CLASS(CFrameWnd)) && pFocusableChild == NULL)
	{
		int nFirstCandidateID;
		int nCurrentID;
		if (pParent->IsKindOf(RUNTIME_CLASS(CBaseTileGroup)))
		{
			if (((CBaseTileGroup*)pParent)->SetNextControlFocus(NULL, bBackward, pCurrTileDialog))
				return TRUE;
		}
		else
		{
			pFocusableChild = pParent->GetWindow(GW_CHILD);
			nFirstCandidateID = pFocusableChild->GetDlgCtrlID();
			nCurrentID = -1;
			while (
				pFocusableChild != NULL
				&&
				nFirstCandidateID != nCurrentID               //se sono uguali, vuol dire che ha ciclato su tutti i controlli nella dialog
				&&
				(
					!pFocusableChild->IsWindowEnabled() ||
					!pFocusableChild->IsWindowVisible() ||
					pFocusableChild->IsKindOf(RUNTIME_CLASS(CStatic)) ||
					(dynamic_cast<CGridControlObj*>(pFocusableChild) == NULL && dynamic_cast<CParsedCtrl*>(pFocusableChild) == NULL)
					)
				)
			{
				pFocusableChild = pParent->GetNextDlgTabItem(pFocusableChild, bBackward);
				nCurrentID = pFocusableChild->GetDlgCtrlID();
			}
		}

		if (!pFocusableChild || nFirstCandidateID == nCurrentID)
		{
			pFocusableChild = NULL;
			pParent = pParent->GetParent();
		}
	}

	if (pFocusableChild)
	{
		pFocusableChild->SetFocus();
		return TRUE;
	}

	// alla fine non rimane che ciclare nella gruppo corrente
	return SetNextControlFocus(NULL, bBackward, pCurrTileDialog);
}

//------------------------------------------------------------------------------
BOOL CBaseTileGroup::SkipRecalcCtrlSize()
{
	return !GetResizableCWnd() || m_CurSize.cx < 0 || m_CurSize.cy < 0;
}

//------------------------------------------------------------------------------
void CBaseTileGroup::SetLayoutType(CLayoutContainer::LayoutType aLayoutType)
{
	if (m_pLayoutContainer)
		m_pLayoutContainer->SetLayoutType(aLayoutType);
}

//------------------------------------------------------------------------------
void CBaseTileGroup::SetLayoutAlign(CLayoutContainer::LayoutAlign aLayoutAlign)
{
	if (m_pLayoutContainer)
		m_pLayoutContainer->SetLayoutAlign(aLayoutAlign);
}

//------------------------------------------------------------------------------
LRESULT CBaseTileGroup::OnRecalcCtrlSize(WPARAM, LPARAM)
{
	if (m_bSuspendedLayout)
		return 0L;

	DoRecalcCtrlSize();

	//if (m_TileDialogArray.GetCount() <= 0)
	//{
	//	return 0L;
	//}

	//CRect rcGroup;
	//GetAvailableRect(rcGroup);
	//HDWP hDWP = ::BeginDeferWindowPos(m_TileDialogArray.GetCount());
	//m_pLayoutContainer->Relayout(rcGroup, hDWP);
	//::EndDeferWindowPos(hDWP);

	return 0L;
}

//---------------------------------------------------------------------------------------
void CBaseTileGroup::OnWindowPosChanged(WINDOWPOS FAR* lpwndpos)
{
	if (m_bSuspendedLayout)
		return;

	CRect rcGroup(0, 0, lpwndpos->cx, lpwndpos->cy);
	if (m_pLayoutContainer)
		m_pLayoutContainer->Relayout(rcGroup);

	StaticAreasAligner aligner;
	aligner.TryToAlign(this);
}

//------------------------------------------------------------------------------
void CBaseTileGroup::OnInitialUpdate(UINT nIDC, CWnd* pParentWnd, CRect rectWnd /*= CRect(0, 0, 0, 0)*/)
{
	CWnd* pWnd = pParentWnd->GetDlgItem(nIDC);
	if (pWnd)
	{
		pWnd->GetWindowRect(&rectWnd);
		pParentWnd->ScreenToClient(rectWnd);
		pWnd->DestroyWindow();
	}
	else if (rectWnd.IsRectNull())
	{
		//è il caso in cui non esiste il Button segnaposto e dobbiamo creare tutto da zero
		rectWnd.SetRect(0, 0, 100, 200);
	}

	CWnd::Create(NULL, NULL, WS_TABSTOP | WS_CHILD | WS_VISIBLE, rectWnd, pParentWnd, nIDC);
	if (AfxGetThemeManager()->UseFlatStyle())
		ModifyStyleEx(WS_EX_CLIENTEDGE | WS_EX_STATICEDGE | WS_EX_WINDOWEDGE, 0);

	if (GetDocument() && GetDocument()->IsInStaticDesignMode())
		EnableLayout(FALSE);

	InitSizeInfo(this);

	//@@@TODO LAYOUT RESIZABLE CTRL
	SetAutoSizeCtrl(0);
	SetResizableCurSize(0, 0);

	OnBeforeCustomize();
	Customize();
	//OnAfterCustomize();
}

//------------------------------------------------------------------------------
void CBaseTileGroup::ResizeStaticArea()
{
	if (m_TileDialogArray.IsEmpty() || m_bSuspendedResizeStaticArea)
		return;

	TBThemeManager* pThemeManager = AfxGetThemeManager();
	// static area resizing is not managed
	if (pThemeManager->GetTileStaticMaxWidthUnit() == 0)
		return;

	int nMaxStaticWidth = 0;
	for (int i = 0; i <= m_TileDialogArray.GetUpperBound(); i++)
	{
		CBaseTileDialog* pWndTile = m_TileDialogArray.GetAt(i);
		nMaxStaticWidth = max(nMaxStaticWidth, pWndTile->GetMaxStaticWidth());
	}
	nMaxStaticWidth += 6; // @@@TODO perche` serve questo magic number??

	int minWidth = MulDiv(pThemeManager->GetTileStaticMinWidthUnit(), pThemeManager->GetBaseUnitsWidth(), 100);
	int maxWidth = MulDiv(pThemeManager->GetTileStaticMaxWidthUnit(), pThemeManager->GetBaseUnitsWidth(), 100);

	nMaxStaticWidth = min(max(nMaxStaticWidth, minWidth), maxWidth);

	for (int i = 0; i <= m_TileDialogArray.GetUpperBound(); i++)
	{
		CBaseTileDialog* pWndTile = m_TileDialogArray.GetAt(i);
		pWndTile->ResizeStaticAreaWidth(nMaxStaticWidth);
	}
}

//------------------------------------------------------------------------------
BOOL CBaseTileGroup::OnCommand(WPARAM wParam, LPARAM lParam)
{
	if (__super::OnCommand(wParam, lParam))
		return TRUE;

	CWnd* pParent = GetParent();
	return pParent ? pParent->SendMessage(WM_COMMAND, wParam, lParam) : FALSE;
}

//-----------------------------------------------------------------------------
BOOL CBaseTileGroup::PreTranslateMessage(MSG* pMsg)
{
#ifndef _OLD_PTM

	if (!GetDocument())
		return __super::PreTranslateMessage(pMsg);

	BOOL bHoldForwardingSysKeydownToChild = GetDocument()->m_bForwardingSysKeydownToChild;

	GetDocument()->m_bForwardingSysKeydownToChild = TRUE;

	BOOL bOk = FALSE;

	for (int i = 0; i <= m_TilePanelArray.GetUpperBound() && !bOk; i++)
	{
		CTilePanel* pTilePanel = m_TilePanelArray[i];
		if (!pTilePanel)
			continue;

		// protezione dal richiamare la PTM di chi mi ha chiamato
		if (GetDocument()->m_bForwardingSysKeydownToParent && (LPARAM)pTilePanel->GetSafeHwnd() == pMsg->lParam)
			continue;

		bOk = pTilePanel->PreTranslateMessage(pMsg);

	}

	GetDocument()->m_bForwardingSysKeydownToChild = bHoldForwardingSysKeydownToChild;

	if (bOk)
		return TRUE;

	GetDocument()->m_bForwardingSysKeydownToChild = TRUE;

	for (int j = 0; j <= m_TileDialogArray.GetUpperBound() && !bOk; j++)
	{
		CBaseTileDialog* pTileDialog = m_TileDialogArray.GetAt(j);
		if (!pTileDialog)
			continue;

		// protezione dal richiamare la PTM di chi mi ha chiamato
		if (GetDocument()->m_bForwardingSysKeydownToParent && (LPARAM)pTileDialog->GetSafeHwnd() == pMsg->lParam)
			continue;

		bOk = pTileDialog->PreTranslateMessage(pMsg);
	}

	GetDocument()->m_bForwardingSysKeydownToChild = bHoldForwardingSysKeydownToChild;

	if (bOk)
		return TRUE;

	if (GetDocument()->m_bForwardingSysKeydownToChild)
		return FALSE;

	// forwarding al parent
	return CTaskBuilderTabWnd::PreProcessSysKeyMessage(pMsg, GetDocument(), this) || __super::PreTranslateMessage(pMsg);

#else

	return __super::PreTranslateMessage(pMsg);

#endif
}

//-----------------------------------------------------------------------------
CTilePanel* CBaseTileGroup::AddPanel(CString strName, int flex /*= -1*/, UINT nPanelID /*= -1-*/)
{
	return AddPanel(m_pLayoutContainer, strName, _T(""), CLayoutContainer::HBOX, CLayoutContainer::STRETCH, flex, nPanelID);
}

//-----------------------------------------------------------------------------
CTilePanel* CBaseTileGroup::AddPanel
(
	CString strName,
	const	CString&						strTitle,
	CLayoutContainer::LayoutType	aLayoutType, /*= CLayoutContainer::HBOX*/
	CLayoutContainer::LayoutAlign	aLayoutAlign, /*= CLayoutContainer::STRETCH*/
	int								flex /*= -1*/,
	UINT							nPanelID /*= -1-*/
)
{
	return AddPanel(m_pLayoutContainer, strName, strTitle, aLayoutType, aLayoutAlign, flex, nPanelID);
}

//-----------------------------------------------------------------------------
CTilePanel* CBaseTileGroup::AddPanel
(
	CLayoutContainer*	pParentContainer,
	CString strName,
	int					flex /*= -1*/,
	UINT				nPanelID /*= -1-*/
)
{
	return AddPanel(pParentContainer, strName, _T(""), CLayoutContainer::HBOX, CLayoutContainer::STRETCH, flex, nPanelID);
}

//-----------------------------------------------------------------------------
CTilePanel* CBaseTileGroup::AddPanel
(
	CLayoutContainer*				pParentContainer,
	CString strName,
	const	CString&						strTitle,
	CLayoutContainer::LayoutType	aLayoutType, /*= CLayoutContainer::HBOX*/
	CLayoutContainer::LayoutAlign	aLayoutAlign, /*= CLayoutContainer::STRETCH*/
	int								flex /*= -1*/,
	UINT							nPanelID /*= -1-*/
)
{
	CTilePanel*  pNewPanel = new CTilePanel();

	pNewPanel->Create(this, strTitle, nPanelID);
	pNewPanel->SetHasTitle(!strTitle.IsEmpty());


	//strName TODOLUCA

	// force a flex only if not auto
	if (flex != AUTO)
		pNewPanel->SetFlex(flex, FALSE);

	pNewPanel->SetLayoutType(aLayoutType);
	pNewPanel->SetLayoutAlign(aLayoutAlign);

	pNewPanel->GetInfoOSL()->m_pParent = GetInfoOSL();
	pNewPanel->GetInfoOSL()->m_Namespace.SetChildNamespace(CTBNamespace::TILEPANEL, strName, GetInfoOSL()->m_Namespace);

	for (int i = 0; i < m_TilePanelArray.GetCount(); i++)
	{
		CTilePanel* pTilePnl = m_TilePanelArray.GetAt(i);
		if (!pTilePnl)
			continue;
		CString sTilePnlNS = pTilePnl->GetNamespace().GetObjectName();
		int res = sTilePnlNS.CompareNoCase(strName);
		ASSERT(res != 0);
		TRACE(_T("A tile panel with the same name ") + strName + _T(" already exists! Please verify the uniqueness for all panels names for the group with name ") + this->GetNamespace().GetObjectName());
	}

	m_TilePanelArray.Add(pNewPanel);
	if (pParentContainer != nullptr)
		pParentContainer->AddChildElement(pNewPanel);

	return pNewPanel;
}

//-----------------------------------------------------------------------------
CBaseTileDialog* CBaseTileGroup::AddTile
(
	CRuntimeClass*	pDialogClass,
	UINT			nIDTile,
	CString			sTileTitle,
	TileDialogSize	tileSize,
	int				flex, /*= AUTO*/
	CObject*		pOwner /*= NULL*/
)
{
	return AddTile(m_pLayoutContainer, pDialogClass, nIDTile, sTileTitle, tileSize, flex, pOwner);
}
//-----------------------------------------------------------------------------
CBaseTileDialog* CBaseTileGroup::AddTile
(
	CLayoutContainer*	pCntner,
	CBaseTileDialog*	pTileDialog,
	UINT				nIDTile,
	CString				sTileTitle,
	TileDialogSize		tileSize,
	int					flex, /*= AUTO*/
	CObject*			pOwner /*= NULL*/
)
{
	CLayoutContainer*	pContainer = pCntner ? pCntner : m_pLayoutContainer;

	AttachTile(pTileDialog, pOwner);
	if (pContainer)
		pContainer->AddChildElement(pTileDialog);

	pTileDialog->Create(nIDTile, sTileTitle, this, tileSize);

	// force a flex only if not auto
	if (flex != AUTO)
		pTileDialog->SetFlex(flex, FALSE);

	if (pContainer && pContainer->GetLayoutType() == CLayoutContainer::STRIPE)
	{   //in Stripe layout mode, tile has not a max width (TODO encapsulate this code)
		pTileDialog->SetMaxWidth(FREE);
	}

	return pTileDialog;
}
//-----------------------------------------------------------------------------
CBaseTileDialog* CBaseTileGroup::AddTile
(
	CLayoutContainer*	pCntner,
	CRuntimeClass*		pDialogClass,
	UINT				nIDTile,
	CString				sTileTitle,
	TileDialogSize		tileSize,
	int					flex, /*= AUTO*/
	CObject*			pOwner /*= NULL*/
)
{

	CBaseTileDialog* pTileDialog = dynamic_cast<CBaseTileDialog*>(pDialogClass->CreateObject());
	if (!pTileDialog)
	{
		ASSERT(FALSE);
		return NULL;
	}

	return AddTile(pCntner, pTileDialog, nIDTile, sTileTitle, tileSize, flex, pOwner);
}

//--------------------------------------------------------------------------------------------------------------------------------------
CTilePanel* CBaseTileGroup::AddStatusPanel(int nColumns /*= 2*/)
{
	ASSERT_TRACE(nColumns >= 1 && nColumns <= 8, "Bad number of columns for StatusPanel");

	CTilePanel* pStatus = AddPanel(_T("Status"), _T(""), CLayoutContainer::COLUMN);

	pStatus->SetTileDialogStyle(AfxGetTileDialogStyleHeader());
	pStatus->GetTileDialogStyle()->SetTileSpacing(2);

	pStatus->SetMaxWidth
	(
		CBaseTileDialog::GetTileWidth(TILE_MICRO) * nColumns + pStatus->GetTileDialogStyle()->GetTileSpacing() * (nColumns - 1) + 2
	);

	return pStatus;
}

//-----------------------------------------------------------------------------
void CBaseTileGroup::AttachTile(CBaseTileDialog* pTileDialog, CObject* pOwner)
{
	pTileDialog->AttachDocument(GetDocument());
	pTileDialog->GetNamespace().SetChildNamespace(CTBNamespace::FORM, pTileDialog->m_sName, GetNamespace());
	pTileDialog->AttachOwner(pOwner);

	CInfoOSL* pInfoOSL = pTileDialog->GetInfoOSL();
	ASSERT(pInfoOSL);
	if (pInfoOSL)
	{
		pInfoOSL->m_pParent = GetInfoOSL();
		pInfoOSL->SetType(OSLType_Tile);

		//security
		AfxGetSecurityInterface()->GetObjectGrant(pInfoOSL);
	}

	for (int i = 0; i < m_TileDialogArray.GetCount(); i++)
	{
		CBaseTileDialog* pTileDlg = m_TileDialogArray.GetAt(i);
		if (!pTileDlg)
			continue;
		CString sTileDlgNS = pTileDlg->GetNamespace().GetObjectName();
		int res = sTileDlgNS.CompareNoCase(pTileDialog->m_sName);
		ASSERT(res != 0);
		TRACE(_T("A tile dialog with the same name ") + pTileDialog->m_sName + _T(" already exists! Please verify the uniqueness for all tile dialogs names for the group with name ") + this->GetNamespace().GetObjectName());

	}

	m_TileDialogArray.Add(pTileDialog);
}

//-----------------------------------------------------------------------------
CLayoutContainer* CBaseTileGroup::AddContainer
(
	CLayoutContainer::LayoutType	aLayoutType,
	int								flex,
	CLayoutContainer::LayoutAlign	aLayoutAlign /*= CLayoutContainer::STRETCH*/
)
{
	return AddContainer(m_pLayoutContainer, aLayoutType, flex, aLayoutAlign);
}

//-----------------------------------------------------------------------------
CLayoutContainer* CBaseTileGroup::AddContainer
(
	CLayoutContainer*				pParentContainer,
	CLayoutContainer::LayoutType	aLayoutType,
	int								flex,
	CLayoutContainer::LayoutAlign	aLayoutAlign /*= CLayoutContainer::STRETCH*/
)
{
	CLayoutContainer* pContainer = new CLayoutContainer(pParentContainer, m_pTileDialogStyle);
	pContainer->SetLayoutType(aLayoutType);
	pContainer->SetLayoutAlign(aLayoutAlign);
	if (pParentContainer)
		pParentContainer->AddChildElement(pContainer);
	pContainer->SetFlex(flex);
	m_OwnedLayoutContainers.Add(pContainer);

	return pContainer;
}

//-----------------------------------------------------------------------------
void CBaseTileGroup::MoveTileByIDD(CBaseTileDialog* pTile, UINT nIDD, bool bAfter)
{
	ASSERT(nIDD);
	CBaseTileDialog* pTileDialogIDD = NULL;
	int nFrom = -1, nTo = -1;
	for (int p = 0; p <= m_TileDialogArray.GetUpperBound(); p++)
	{
		CBaseTileDialog* pT = m_TileDialogArray.GetAt(p);
		if (pT->GetDialogID() == nIDD)
		{
			pTileDialogIDD = pT;
			nTo = bAfter ? p + 1 : p;
			if (nFrom != -1)
				break;
		}
		else if (pT == pTile)
		{
			nFrom = p;
			if (nTo != -1)
				break;
		}
	}
	if (nFrom == -1 || nTo == -1)
	{
		ASSERT_TRACE1(FALSE, "MoveTileByIDD: Tile dialog ID not found %d", nIDD);
		return;
	}
	if (nFrom != nTo)
	{
		BOOL bOld = m_TileDialogArray.SetOwns(FALSE);
		m_TileDialogArray.RemoveAt(nFrom);
		if (nFrom > nTo)
			nTo--;
		m_TileDialogArray.InsertAt(nTo, pTile);
		m_TileDialogArray.SetOwns(bOld);

		LayoutElement* pL = pTile->GetParentElement();
		if (!pL)
		{
			if (m_pLayoutContainer)
				m_pLayoutContainer->AddChildElement(pTile);
		}
		else
		{
			pL->RemoveChildElement(pTile);
			int nNewPos = pL->FindChildElement(pTileDialogIDD);
			if (nNewPos == -1)
			{
				pL->AddChildElement(pTile);
			}
			else
			{
				if (bAfter)
					nNewPos++;

				if (nNewPos < m_TileDialogArray.GetSize())
					pL->InsertChildElement(pTile, nNewPos);
				else
					pL->AddChildElement(pTile);
			}
		}
	}

}

//-----------------------------------------------------------------------------
void CBaseTileGroup::MoveTile(CBaseTileDialog* pTile, CBaseTileDialog* pBeforeTile)
{
	ASSERT_VALID(pTile);
	ASSERT_KINDOF(CBaseTileDialog, pTile);
	ASSERT_VALID(pBeforeTile);
	ASSERT_KINDOF(CBaseTileDialog, pBeforeTile);
	ASSERT(pTile->GetParentElement() == pBeforeTile->GetParentElement());

	if (pTile->GetParentElement())
		pTile->GetParentElement()->RemoveChildElement(pTile);

	if (!pBeforeTile->GetParentElement())
	{
		if (m_pLayoutContainer)
			m_pLayoutContainer->AddChildElement(pTile);
	}
	else
	{
		int p = pBeforeTile->GetParentElement()->FindChildElement(pBeforeTile);
		pBeforeTile->GetParentElement()->InsertChildElement(pTile, p);
	}
}

//-----------------------------------------------------------------------------
CTileDesignModeParamsObj* CBaseTileGroup::GetTileDesignModeParams()
{
	if (GetDocument() && GetDocument()->GetManagedParameters())
	{
		CTileDesignModeParamsObj* pParam = dynamic_cast<CTileDesignModeParamsObj*>(GetDocument()->GetDesignModeManipulatorObj());
		if (pParam)
			return pParam;
	}

	return dynamic_cast<CTileDesignModeParamsObj*>(AfxGetThemeManager());
}

//-----------------------------------------------------------------------------
int CBaseTileGroup::GetRequiredHeight(CRect &rectAvail)
{
	if (!m_pLayoutContainer)
		return 0;

	int nHeght = m_pLayoutContainer->GetRequiredHeight(rectAvail);
	if (!nHeght && !GetTileDesignModeParams()->AreZeroSizesAllowed())
	{
		nHeght = GetMinHeight();
	}
	else if (GetTileDesignModeParams()->FreeResize())
	{
		CRect aRect;
		GetAvailableRect(aRect);
		return max(nHeght, min(aRect.Height(), rectAvail.Height()));
	}
	return nHeght;
}

//-----------------------------------------------------------------------------
int CBaseTileGroup::GetRequiredWidth(CRect &rectAvail)
{
	if (!m_pLayoutContainer)
		return 0;
	int nWidth = m_pLayoutContainer->GetRequiredWidth(rectAvail);
	BOOL bIsEmpty = !GetTileDialogs()->GetSize() && !GetTilePanels()->GetSize();
	if ((!nWidth && bIsEmpty) || GetTileDesignModeParams()->FreeResize())
	{
		return max(nWidth, rectAvail.Width());
	}

	return nWidth;
}

//-----------------------------------------------------------------------------
void CBaseTileGroup::GetAvailableRect(CRect &rectAvail)
{
	GetClientRect(rectAvail);
}

//------------------------------------------------------------------------------
int CBaseTileGroup::GetMinHeight(CRect& rect /*= CRect(0, 0, 0, 0)*/)
{
	switch (m_nMinHeight)
	{
	case ORIGINAL:
	case FREE:
	case AUTO:
	{
		if (m_pLayoutContainer)
			return m_pLayoutContainer->GetMinHeight(rect);
	}
	default: return m_nMinHeight;
	}
}

//------------------------------------------------------------------------------
BOOL CBaseTileGroup::CanDoLastFlex(FlexDim  fd)
{
	return m_pLayoutContainer ? m_pLayoutContainer->CanDoLastFlex(fd) : FALSE;
}

//-----------------------------------------------------------------------------
void CBaseTileGroup::GetUsedRect(CRect &rectUsed)
{
	GetWindowRect(rectUsed);
	//m_pLayoutContainer->GetUsedRect(rectUsed);
}

//-----------------------------------------------------------------------------
void CBaseTileGroup::Relayout(CRect &rectNew, HDWP hDWP /*= NULL*/)
{
	CDockableFrame* pFrame = dynamic_cast<CDockableFrame*>(this->GetParentFrame());
	if ((pFrame && pFrame->IsLayoutSuspended()) || !IsWindow(m_hWnd) || !IsVisible())
		return;

	// Avoid calling MoveWindow if just internal repositioning due to collapsing
	CRect rectActual;
	GetAvailableRect(rectActual);

	// questa non si puo' togliere altrimenti rimangono
	// sporche le aree tra una tile/tilepanel e l'altro
	// soprattutto nei collapse/expand

	if (this->GetParent())
		this->GetParent()->InvalidateRect(rectActual);

	SetWindowPos(NULL, rectNew.left, rectNew.top, rectNew.Width(), rectNew.Height(), SWP_NOZORDER | SWP_NOACTIVATE | SWP_FRAMECHANGED);
	UpdateWindow();

}


//-----------------------------------------------------------------------------
void  CBaseTileGroup::SetTileVisible(CString sTileName, BOOL bVisible)
{
	for (int i = 0; i <= m_TileDialogArray.GetUpperBound(); i++)
	{
		CBaseTileDialog* pWndTile = m_TileDialogArray.GetAt(i);
		if (sTileName.CompareNoCase(pWndTile->GetFormName()) == 0)
		{
			pWndTile->Show(bVisible);
			return;
		}
	}
}

//-----------------------------------------------------------------------------
BOOL CBaseTileGroup::OnEraseBkgnd(CDC* pDC)
{
	if (m_bTransparent)
		pDC->SetBkColor(TRANSPARENT);
	else
	{

		CDockableFrame* pFrame = dynamic_cast<CDockableFrame*>(this->GetParentFrame());
		if (pFrame && pFrame->IsLayoutSuspended())
		{
			CWnd* pCtrl = this->GetWindow(GW_CHILD);
			for (; pCtrl; pCtrl = pCtrl->GetNextWindow())
			{
				if (!pCtrl->IsWindowVisible())
					continue;

				CRect screen;
				pCtrl->GetWindowRect(&screen);
				this->ScreenToClient(&screen);
				pDC->ExcludeClipRect(&screen);
			}
		}

		CRect rclientRect;
		this->GetClientRect(rclientRect);

		pDC->FillRect(&rclientRect, GetTileDesignModeParams()->GetTileGroupBkgColorBrush());
		return TRUE;
	}


	return TRUE;
}

//-----------------------------------------------------------------------------
LRESULT CBaseTileGroup::OnValueChanged(WPARAM wParam, LPARAM lParam)
{
	return GetParent() ? GetParent()->SendMessage(UM_VALUE_CHANGED, wParam, lParam) : 0L;
}

//-----------------------------------------------------------------------------
LRESULT	CBaseTileGroup::OnCtrlFocused(WPARAM wParam, LPARAM lParam)
{
	return GetParent() ? GetParent()->SendMessage(UM_CTRL_FOCUSED, wParam, lParam) : 0L;
}


//-----------------------------------------------------------------------------
bool CBaseTileGroup::SetDefaultFocus()
{
	if (m_TileDialogArray.GetSize() > 0)
	{
		for (int i = 0; i <= m_TileDialogArray.GetUpperBound(); i++)
		{
			CBaseTileDialog* pDialog = m_TileDialogArray.GetAt(i);
			if (!pDialog->IsCollapsed() && pDialog->SetDefaultFocus())
				return true;
		}
	}
	return false;
}

//-----------------------------------------------------------------------------
CWnd* CBaseTileGroup::GetWndLinkedCtrl(const CTBNamespace& aNS)
{
	if (GetNamespace() == aNS)
		return this;

	CWnd* pWnd = NULL;
	for (int j = 0; j <= GetTileDialogs()->GetUpperBound(); j++)
	{
		CBaseTileDialog* pDialog = GetTileDialogs()->GetAt(j);
		if (pDialog->GetNamespace() == aNS)
			return pDialog;

		pWnd = pDialog->GetWndLinkedCtrl(aNS);
		if (pWnd)
			return pWnd;
	}

	for (int p = 0; p <= GetTilePanels()->GetUpperBound(); p++)
	{
		CTilePanel* pPanel = GetTilePanels()->GetAt(p);
		if (pPanel->GetNamespace() == aNS)
			return pPanel;

		pWnd = pPanel->GetWndLinkedCtrl(aNS);
		if (pWnd)
			return pWnd;
	}

	return pWnd;
}

//-----------------------------------------------------------------------------
void CBaseTileGroup::OnSetFocus(CWnd* pOldCWnd)
{
	__super::OnSetFocus(pOldCWnd);
}

//-----------------------------------------------------------------------------
void CBaseTileGroup::SetTransparent(BOOL bTransparent)
{
	m_bTransparent = bTransparent;
}

//-----------------------------------------------------------------------------
inline const BOOL&	CBaseTileGroup::IsTransparent() const
{
	return m_bTransparent;
}

//-----------------------------------------------------------------------------
void CBaseTileGroup::SetFlex(int nValue, BOOL bInContainerToo /*TRUE*/)
{
	LayoutElement::SetFlex(nValue, FALSE);
	//if (bInContainerToo && m_pLayoutContainer)
	//	m_pLayoutContainer->SetFlex(nValue, FALSE);
}

//-----------------------------------------------------------------------------
void CBaseTileGroup::SetSuspendResizeStaticArea(BOOL bValue)
{
	m_bSuspendedResizeStaticArea = bValue;
}

//-----------------------------------------------------------------------------
LRESULT CBaseTileGroup::OnInitializeLayout(WPARAM, LPARAM pParam)
{
	CWnd* pSender = (CWnd*)pParam;

	// se sono nell'initialUpdate della view ripristino la resize della static area sospesa
	if (
		pSender &&
		(!GetDocument() || GetDocument()->GetDesignMode() != CBaseDocument::DM_RUNTIME) &&
		(
			pSender->GetRuntimeClass()->IsDerivedFrom(RUNTIME_CLASS(CBaseFormView)) ||
			pSender->GetRuntimeClass()->IsDerivedFrom(RUNTIME_CLASS(CBaseTabManager)) ||
			pSender->GetRuntimeClass()->IsDerivedFrom(RUNTIME_CLASS(CParsedDialogWithTiles))
			)
		)
		SetSuspendResizeStaticArea(FALSE);

	//corina 
	for (int j = 0; j <= GetTileDialogs()->GetUpperBound(); j++)
	{
		CBaseTileDialog* pDialog = GetTileDialogs()->GetAt(j);
		if (pDialog)
			pDialog->InitializeLayout();
	}

	if (!m_bSuspendedResizeStaticArea)
		ResizeStaticArea();

	return 0L;
}