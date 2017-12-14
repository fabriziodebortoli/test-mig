#include "stdafx.h"

// local declarations
#include "TBRepositoryManager.h"
#include "CommonObjects.h"
#include "DDMSCategories.h"
#include "UIDMSCategories.h"

#include "EasyAttachment\JsonForms\UIDMSCategories\IDD_DMSCATEGORIES.hjson"

//Parametri per le query
static TCHAR szP1[] = _T("P1");

//////////////////////////////////////////////////////////////////////////////
//             class DBTDMSCategoriesValues implementation
//////////////////////////////////////////////////////////////////////////////
//-----------------------------------------------------------------------------	
IMPLEMENT_DYNAMIC(DBTDMSCategoriesValues, DBTSlaveBuffered)

//-----------------------------------------------------------------------------	
DBTDMSCategoriesValues::DBTDMSCategoriesValues
(
	CRuntimeClass*		pClass,
	CAbstractFormDoc*	pDocument
)
	:
	DBTSlaveBuffered(pClass, pDocument, _NS_DBT("Values"), FALSE, TRUE)
{
}

// Serve a definire sia i criteri di sort (ORDER BY chiave primaria in questo caso)
// ed i criterio di filtraggio (WHERE)
//-----------------------------------------------------------------------------
void DBTDMSCategoriesValues::OnDefineQuery()
{
	m_pTable->SelectAll();
}

//-----------------------------------------------------------------------------
void DBTDMSCategoriesValues::OnPrepareQuery()
{

}

//-----------------------------------------------------------------------------	
DataObj* DBTDMSCategoriesValues::OnCheckPrimaryKey(int /*nRow*/, SqlRecord* /*pRec*/)
{
	return NULL;
}

//-----------------------------------------------------------------------------
void DBTDMSCategoriesValues::OnPreparePrimaryKey(int nRow, SqlRecord* pRec)
{
	ASSERT(pRec->IsKindOf(RUNTIME_CLASS(VCategoryValues)));
	CDMSCategory* pDMSCategory = ((DDMSCategories*)GetDocument())->m_pCurrentDMSCategory;
	VCategoryValues* pRecValues = (VCategoryValues*)pRec;
	pRecValues->l_Name = pDMSCategory->m_Name;
}

//-----------------------------------------------------------------------------
DataObj* DBTDMSCategoriesValues::OnCheckUserData(int nRow)
{
	return NULL;
}

//-----------------------------------------------------------------------------	
void DBTDMSCategoriesValues::OnPrepareRow(int nRow, SqlRecord* pRec)
{
	// se è la prima riga che inserisco allora imposto il valore di l_Default a TRUE
	if (nRow == 0 && GetSize() == 1)
		((VCategoryValues*)pRec)->l_IsDefault = TRUE;
}

//-----------------------------------------------------------------------------	
DataObj* DBTDMSCategoriesValues::GetDuplicateKeyPos(SqlRecord* pRec)
{
	ASSERT(pRec->IsKindOf(RUNTIME_CLASS(VCategoryValues)));
	return &(((VCategoryValues*)pRec)->l_Value);
}

//-----------------------------------------------------------------------------	
CString DBTDMSCategoriesValues::GetDuplicateKeyMsg(SqlRecord* pRec)
{
	ASSERT(pRec->IsKindOf(RUNTIME_CLASS(VCategoryValues)));
	VCategoryValues* pCatValue = (VCategoryValues*)pRec;

	return cwsprintf(_TB("The {0-%s} value already exists:\r\n use a different code"), (LPCTSTR)pCatValue->l_Value.Str());
}

//-----------------------------------------------------------------------------
BOOL DBTDMSCategoriesValues::LocalFindData(BOOL)
{
	CDMSCategory* pDMSCategory = ((DDMSCategories*)GetDocument())->m_pCurrentDMSCategory;
	RemoveAll();
	if (pDMSCategory)
	{
		for (int i = 0; i < pDMSCategory->m_arCategoryValues.GetSize(); i++)
		{
			VCategoryValues* pNewRec = (VCategoryValues*)AddRecord();
			*pNewRec = *pDMSCategory->GetValueAt(i);
			pNewRec->l_IsDefault = (pDMSCategory->m_DefaultValue == pNewRec->l_Value);
			pNewRec->SetStorable();
		}
	}
	return TRUE;
}

//-----------------------------------------------------------------------------
VCategoryValues* DBTDMSCategoriesValues::GetVCategoryValues(const DataStr& value) const
{
	for (int i = 0; i < GetSize(); i++)
	{
		VCategoryValues* pRec = GetVCategoryValues(i);
		if (pRec->l_Value.CompareNoCase(value) == 0)
			return pRec;
	}
	return NULL;
}

//////////////////////////////////////////////////////////////////////////////
//               class DDMSCategories implementation
//////////////////////////////////////////////////////////////////////////////
//-----------------------------------------------------------------------------
IMPLEMENT_DYNCREATE(DDMSCategories, CAbstractFormDoc)

//-----------------------------------------------------------------------------
BEGIN_MESSAGE_MAP(DDMSCategories, CAbstractFormDoc)
	//{{AFX_MSG_MAP(DDMSCategories)
	ON_CONTROL(UM_TREEVIEWADV_SELECTION_CHANGED, IDC_DMSCATEGORIES_TREE, OnSelectionNodeChanged)

	ON_COMMAND(ID_LOAD_DMSCATEGORIES_TREE, OnLoadDMSCategoriesTree)
	ON_COMMAND(ID_SET_CURRENTNODE_TREE, OnSetCurrentNode)
	ON_COMMAND(ID_EXTDOC_NEW, OnNewRecord)
	ON_COMMAND(ID_EXTDOC_EDIT, OnEditRecord)
	ON_COMMAND(ID_EXTDOC_DELETE, OnDeleteRecord)
	ON_COMMAND(ID_EXTDOC_SAVE, OnSaveRecord)
	ON_COMMAND(ID_EXTDOC_ESCAPE, OnEscape)
	ON_COMMAND(ID_EXTDOC_REFRESH_ROWSET, OnRefreshRowset)

	ON_EN_VALUE_CHANGED(IDC_DMSCATEGORIES_BE_DEFAULT, OnBeDefaultCheckChanged)
	//}}AFX_MSG_MAP
END_MESSAGE_MAP()

//-----------------------------------------------------------------------------
VCategoryValues* DDMSCategories::GetVCategoryValues() const
{
	return (VCategoryValues*)m_pDBTCategoriesValues->GetCurrentRow();
}

//-----------------------------------------------------------------------------
DDMSCategories::DDMSCategories()
	:
	m_pCategories(NULL),
	m_pOldDMSCategory(NULL),
	m_pCurrentDMSCategory(NULL),
	m_pDBTCategoriesValues(NULL),
	m_bSaving(FALSE),
	m_bTreeView(TRUE)
{
	m_pCurrentDMSCategory = new CDMSCategory();
	m_pOldDMSCategory = new CDMSCategory();

	DisableFamilyClientDoc(TRUE);
}

//-----------------------------------------------------------------------------
DDMSCategories::~DDMSCategories()
{
	SAFE_DELETE(m_pDBTCategoriesValues);
	SAFE_DELETE(m_pCurrentDMSCategory);
	SAFE_DELETE(m_pOldDMSCategory);
	SAFE_DELETE(m_pCategories);
}

//-----------------------------------------------------------------------------
BOOL DDMSCategories::OnAttachData()
{
	SetFormTitle(_TB("DMS Categories"));

	m_pDBTCategoriesValues = new DBTDMSCategoriesValues(RUNTIME_CLASS(VCategoryValues), this);

	LoadCategories();

	DECLARE_VAR_JSON(bTreeView);

	CDMSCategory* pDMSCategory = GetCurrentDMSCategory();
	DECLARE_VAR(_T("Name"), pDMSCategory->m_Name);
	DECLARE_VAR(_T("Description"), pDMSCategory->m_Description);
	DECLARE_VAR(_T("bDisabled"), pDMSCategory->m_bDisabled);

	DECLARE_VAR_JSON(CategoryInUseMsg);

	return TRUE;
}

//----------------------------------------------------------------------------
void DDMSCategories::OnParsedControlCreated(CParsedCtrl* pCtrl)
{
	__super::OnParsedControlCreated(pCtrl);

	UINT nIDC = pCtrl->GetCtrlID();

	if (nIDC == IDC_DMSCATEGORIES_TREE)
	{
		m_pCategoriesTreeView = dynamic_cast<CDMSCategoriesTreeViewAdv*>(pCtrl);
		SetTreeView(m_pCategoriesTreeView);
		TBThemeManager* pTheme = AfxGetThemeManager();
		if (!pTheme)
			return;

		if (m_pCategoriesTreeView)
			m_pCategoriesTreeView->SetBackColorTreeView(pTheme->GetTileDialogStaticAreaBkgColor());
	}
}

//-----------------------------------------------------------------------------
void DDMSCategories::SetTreeView(CDMSCategoriesTreeViewAdv* pTreeView)
{
	ASSERT(pTreeView);
	ASSERT_VALID(pTreeView);
	ASSERT_KINDOF(CTreeViewAdvCtrl, pTreeView);
	if (m_pCategoriesTreeView)
		return;

	m_pCategoriesTreeView = pTreeView;
}

// escamotage per forzare il caricamento delle informazioni nel treeview
// non posso utilizzare la OnPrepareAuxData perche' questo documento e' privo
// dei pulsanti di browse
//-----------------------------------------------------------------------------
void DDMSCategories::OnInitializeUI(const CTBNamespace& aFormNs)
{
	CString sName = aFormNs.GetObjectName();

	if (sName.CompareNoCase(_T("IDD_TD_DMSCATEGORIES_TREE")) == 0)
		OnLoadDMSCategoriesTree();
}

//-----------------------------------------------------------------------------
void DDMSCategories::LoadCategories()
{
	if (m_pCategories)
		delete m_pCategories;
	m_pCategories = AfxGetTbRepositoryManager()->GetCategories();
}

//-----------------------------------------------------------------------------
void DDMSCategories::SetCurrentDMSCategory()
{
	if (m_strCurrentCategoryName.IsEmpty())
		InitCurrentCategory();
	else
	{
		CDMSCategory* pCategory = AfxGetTbRepositoryManager()->GetDMSCategory(m_strCurrentCategoryName);
		if (pCategory)
		{
			*m_pCurrentDMSCategory = *pCategory;
			*m_pOldDMSCategory = *pCategory;
			if (m_pCurrentDMSCategory)
				m_pDBTCategoriesValues->LocalFindData(FALSE);
			delete pCategory;
			m_CategoryInUseMsg = m_pCurrentDMSCategory->m_bInUse ? _TB("The category is used as bookmark. You can't delete it, disable it instead.") : _T("");
		}
	}
	UpdateDataView();
}

//-----------------------------------------------------------------------------
void DDMSCategories::InitCurrentCategory()
{
	m_strCurrentCategoryName.Empty();
	m_CategoryInUseMsg.Clear();
	m_pCurrentDMSCategory->Clear();
	//m_pOldDMSCategory->Clear();
	m_pDBTCategoriesValues->RemoveAll();
}

//-----------------------------------------------------------------------------
CDMSCategory* DDMSCategories::GetCurrentDMSCategory() const
{
	return m_pCurrentDMSCategory;
}

//-----------------------------------------------------------------------------
BOOL DDMSCategories::OnOkTransaction()
{
	if (!m_pCurrentDMSCategory)
		return FALSE;

	if (GetFormMode() == NEW)
	{
		for (int j = 0; j < m_pCategories->GetSize(); j++)
		{
			if (m_pCategories->GetAt(j) && m_pCategories->GetAt(j)->m_Name.CompareNoCase(m_pCurrentDMSCategory->m_Name.Str()) == 0)
			{
				Message(cwsprintf(_TB("Category {0-%s} already exists! Please specify another name."), m_pCurrentDMSCategory->m_Name.Str()));
				return FALSE;
			}
		}
	}

	if (m_pDBTCategoriesValues->IsEmpty())
	{
		Message(_TB("Unable to save empty values for this category."));
		return FALSE;
	}

	int nEmptyValuesCount = 0;
	for (int i = 0; i < m_pDBTCategoriesValues->GetSize(); i++)
	{
		VCategoryValues* pRec = m_pDBTCategoriesValues->GetVCategoryValues(i);
		if (pRec->l_Value.IsEmpty())
			nEmptyValuesCount++;
	}
	if (nEmptyValuesCount == m_pDBTCategoriesValues->GetSize())
	{
		Message(_TB("Unable to save empty values for this category."));
		return FALSE;
	}

	BOOL bOk = TRUE;
	BOOL bFound = FALSE;

	for (int i = 0; i < m_pDBTCategoriesValues->GetSize(); i++)
	{
		VCategoryValues* defRec = m_pDBTCategoriesValues->GetVCategoryValues(i);
		if (defRec->l_IsDefault == TRUE)
		{
			bFound = TRUE;
			break;
		}
	}

	if (!bFound)
		Message(_TB("You have to specify a default value for this category."));

	if (m_pCurrentDMSCategory->m_Description.IsEmpty())
		m_pCurrentDMSCategory->m_Description = m_pCurrentDMSCategory->m_Name;

	return bFound && CAbstractFormDoc::OnOkTransaction();
}

//-----------------------------------------------------------------------------
void DDMSCategories::DisableControlsForBatch()
{
	m_pCurrentDMSCategory->m_Name.SetReadOnly();
	m_pCurrentDMSCategory->m_Description.SetReadOnly();
	m_pCurrentDMSCategory->m_bDisabled.SetReadOnly();
	m_pDBTCategoriesValues->SetReadOnly();
}

//-----------------------------------------------------------------------------
void DDMSCategories::EnableControls()
{
	BOOL bEnabled = (GetFormMode() == NEW || GetFormMode() == EDIT);

	m_pCurrentDMSCategory->m_Name.SetReadOnly(GetFormMode() != NEW);
	m_pCurrentDMSCategory->m_Description.SetReadOnly(!bEnabled);
	m_pCurrentDMSCategory->m_bDisabled.SetReadOnly(!bEnabled);
	m_pDBTCategoriesValues->SetReadOnly(!bEnabled);
	m_bTreeView.SetReadOnly(bEnabled);

	m_CategoryInUseMsg.Clear(GetFormMode() == NEW);
}

//-----------------------------------------------------------------------------
void DDMSCategories::OnBeDefaultCheckChanged()
{
	VCategoryValues* currRec = (VCategoryValues*)m_pDBTCategoriesValues->GetCurrentRow();
	if (currRec->l_IsDefault)
		for (int i = 0; i < m_pDBTCategoriesValues->GetSize(); i++)
		{
			VCategoryValues* defRec = m_pDBTCategoriesValues->GetVCategoryValues(i);
			if (i != m_pDBTCategoriesValues->GetCurrentRowIdx() && defRec->l_IsDefault == TRUE)
				defRec->l_IsDefault = FALSE;
		}

	UpdateDataView();
}

//-----------------------------------------------------------------------------
void DDMSCategories::OnNewRecord()
{
	InitCurrentCategory();
	m_pCurrentDMSCategory->m_Name = _TB("New Category");
	GetNotValidView(TRUE);
	CBaseDocument::SetFormMode(NEW);
	SetModifiedFlag(FALSE);
	EnableControls();
	UpdateDataView();
	SetDefaultFocus();
}

//-----------------------------------------------------------------------------
void DDMSCategories::OnEditRecord()
{
	GetNotValidView(TRUE);
	CBaseDocument::SetFormMode(EDIT);
	SetModifiedFlag(FALSE);
	EnableControls();
	UpdateDataView();
	SetDefaultFocus();
}

//-----------------------------------------------------------------------------
void DDMSCategories::OnDeleteRecord()
{
	if (!CanDoDeleteRecord() || !m_pCurrentDMSCategory)
		return;

	if (AfxMessageBox(_TB("Are you sure you want to delete it?"), MB_YESNO | MB_ICONQUESTION | MB_DEFBUTTON2) == IDNO)
		return;


	if (AfxGetTbRepositoryManager()->DeleteDMSCategory(m_strCurrentCategoryName))
	{
		int newCatIdx;
		for (int i = 0; i < m_pCategories->GetCount(); i++)
		{
			if (m_strCurrentCategoryName.CompareNoCase(m_pCategories->GetAt(i)->m_Name.Str()) == 0)
			{
				newCatIdx = (i > 0) ? i - 1 : (i + 1 <= m_pCategories->GetUpperBound()) ? i + 1 : -1;
				m_strCurrentCategoryName = (newCatIdx > -1) ? m_pCategories->GetAt(newCatIdx)->m_Name : _T(" ");
				m_pCategories->RemoveAt(i);
				break;
			}
		}
		GetFrame()->SendMessageToDescendants(WM_COMMAND, ID_LOAD_DMSCATEGORIES_TREE);
	}
	else
		SetCurrentDMSCategory();

	//@@TODOMICHI: commentato per non fare richiamare la SelectNode esplicita del Treeview
	// per evitare un possibile tromb (chiesto a Germano)
	// GetFrame()->SendMessageToDescendants(WM_COMMAND, ID_SET_CURRENTNODE_TREE);
	GoInBrowseMode();
	UpdateDataView();
}

//-----------------------------------------------------------------------------
void DDMSCategories::OnSaveRecord()
{
	if (!CanDoSaveRecord() || !m_pCurrentDMSCategory)
		return;

	GetNotValidView(TRUE);

	if (OnOkTransaction())
	{
		m_pCurrentDMSCategory->m_arCategoryValues.RemoveAll();

		for (int i = 0; i < m_pDBTCategoriesValues->GetSize(); i++)
		{
			VCategoryValues* pDBTCatValue = m_pDBTCategoriesValues->GetVCategoryValues(i);
			// skippo le righe con valori vuoti che non vanno salvate
			if (pDBTCatValue->l_Value.IsEmpty())
				continue;

			VCategoryValues* pNewCatValue = new VCategoryValues();
			*pNewCatValue = *pDBTCatValue;
			m_pCurrentDMSCategory->m_arCategoryValues.Add(pNewCatValue);
			if (pDBTCatValue->l_IsDefault)
				m_pCurrentDMSCategory->m_DefaultValue = pDBTCatValue->l_Value;
		}

		m_bSaving = TRUE;
		BOOL bSaveOk = AfxGetTbRepositoryManager()->SaveDMSCategory(m_pCurrentDMSCategory);
		m_bSaving = FALSE;

		if (bSaveOk)
		{
			m_strCurrentCategoryName = m_pCurrentDMSCategory->m_Name;
			if (GetFormMode() == NEW)
			{
				CDMSCategory* pNewCategory = new CDMSCategory();
				*pNewCategory = *m_pCurrentDMSCategory;
				m_pCategories->Add(pNewCategory);
			}
			else
			{
				for (int i = 0; i < m_pCategories->GetCount(); i++)
				{
					if (m_strCurrentCategoryName.CompareNoCase(m_pCategories->GetAt(i)->m_Name.Str()) == 0)
					{
						CDMSCategory* pCategory = m_pCategories->GetAt(i);
						*pCategory = *m_pCurrentDMSCategory;
					}
				}
			}
			GetFrame()->SendMessageToDescendants(WM_COMMAND, ID_LOAD_DMSCATEGORIES_TREE);
			GoInBrowseMode(); // vado in browse mode solo se sono riuscita a salvare con successo
		}
		else
		{
			m_strCurrentCategoryName = m_pOldDMSCategory->m_Name.Str();
			SetCurrentDMSCategory();
		}

		//@@TODOMICHI: commentato per non fare richiamare la SelectNode esplicita del Treeview
		// per evitare un possibile tromb (chiesto a Germano)
		//GetFrame()->SendMessageToDescendants(WM_COMMAND, ID_SET_CURRENTNODE_TREE);
	}
	UpdateDataView();
}

//-----------------------------------------------------------------------------
void DDMSCategories::OnRefreshRowset()
{
	LoadCategories();
	GetFrame()->SendMessageToDescendants(WM_COMMAND, IDC_DMSCATEGORIES_TREE);
}

static TCHAR szRootKey[] = _T("RootKey");
// -------------------------------------------------------------------------------------------------------------- -
void DDMSCategories::OnLoadDMSCategoriesTree()
{
	m_bTreeViewLoaded = FALSE;

	Array* pAllNode = new Array();

	if (m_pCategories)
	{
		for (int i = 0; i <= m_pCategories->GetUpperBound(); i++)
		{
			CDMSCategory* pCategory = m_pCategories->GetAt(i);
			if (pCategory && !pCategory->m_Name.IsEmpty())
			{
				CDMSCategoryNode* pTreeNode = new CDMSCategoryNode(pCategory, szRootKey);
				pAllNode->Add(pTreeNode);
				for (int j = 0; j < pCategory->m_arCategoryValues.GetCount(); j++)
				{
					VCategoryValues* pRec = pCategory->GetValueAt(j);
					pAllNode->Add(new CDMSCategoryNode(pRec, pTreeNode->m_strKey));
				}
			}
		}
	}

	if (m_pCategoriesTreeView)
		m_pCategoriesTreeView->SetAllNodes(pAllNode);
	m_bTreeViewLoaded = TRUE;
	OnSelectionNodeChanged();
}

//---------------------------------------------------------------------------------------------------------------
void DDMSCategories::OnSetCurrentNode()
{
	if (!m_pCategoriesTreeView)
		return;

	m_pCategoriesTreeView->SelectNode(m_strCurrentCategoryName);
	m_pCategoriesTreeView->EnsureVisible(m_strCurrentCategoryName);
}

//---------------------------------------------------------------------------------------------------------------
void DDMSCategories::OnSelectionNodeChanged()
{
	if (!m_bTreeViewLoaded || !m_pCategoriesTreeView)
		return;

	CDMSCategoryNode* pCategoryNode = m_pCategoriesTreeView->GetSelectedTreeNode();
	m_strCurrentCategoryName = (pCategoryNode && pCategoryNode->IsCategory()) ? pCategoryNode->m_strKey : (pCategoryNode) ? pCategoryNode->m_strParentKey : _T("");
	SetCurrentDMSCategory();
}

//-----------------------------------------------------------------------------
void DDMSCategories::OnEscape()
{
	if (GetFormMode() == BROWSE)
		return;

	if (!CanDoEscape())
		return;

	if (
		IsModified() &&
		(GetFormMode() == EDIT || GetFormMode() == NEW) &&
		!IsInUnattendedMode() &&
		(AfxMessageBox(_TB("Are you sure you want to quit?"), MB_OKCANCEL) == IDCANCEL)
		)
		return;

	AbortAllViews();

	m_strCurrentCategoryName = m_pOldDMSCategory->m_Name.Str();
	SetCurrentDMSCategory();
	GoInBrowseMode();
}

//-----------------------------------------------------------------------------
BOOL DDMSCategories::CanRunDocument()
{
	if (!AfxGetOleDbMng()->EasyAttachmentEnable())
	{
		AfxMessageBox(_TB("Impossible to open DMS categories form!\r\nPlease, check in Administration Console if this company uses DMS."));
		return FALSE;
	}

	return TRUE;
}

//-----------------------------------------------------------------------------
BOOL DDMSCategories::CanDoEditRecord()
{
	return m_pCurrentDMSCategory && !m_pCurrentDMSCategory->m_Name.IsEmpty() && GetFormMode() == BROWSE;
}

//-----------------------------------------------------------------------------
BOOL DDMSCategories::CanDoDeleteRecord()
{
	return m_pCurrentDMSCategory && !m_pCurrentDMSCategory->m_Name.IsEmpty() && GetFormMode() == BROWSE && !m_pCurrentDMSCategory->m_bInUse;
}

//-----------------------------------------------------------------------------
BOOL DDMSCategories::CanDoNewRecord()
{
	return m_pCurrentDMSCategory && GetFormMode() == BROWSE;
}

//-----------------------------------------------------------------------------
BOOL DDMSCategories::CanDoSaveRecord()
{
	return (GetFormMode() == NEW || GetFormMode() == EDIT);
}

//-----------------------------------------------------------------------------
BOOL DDMSCategories::CanDoRefreshRowset()
{
	return TRUE;
}

//-----------------------------------------------------------------------------
BOOL DDMSCategories::CanDoEscape()
{
	return !m_bSaving;
}