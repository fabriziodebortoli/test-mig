
#include "stdafx.h"


#include <TbFrameworkImages\CommonImages.h>
#include <TbFrameworkImages\GeneralFunctions.h>

#include <TbGenlib\addonmng.h>
#include <TbGenlib\baseapp.h>
#include <TbGenlib\xmlModuleObjectsInfo.h>
#include <TbGenlib\generic.h>
#include <TbGenlib\parsctrl.h>
#include <TbGenlib\parsedt.h>
#include <TbNameSolver\PathFinder.h>

#include <TbOledb\oledbmng.h>
#include <TbOledb\sqltable.h>
#include <TbOledb\wclause.h>


#include "XMLGesInfo.h"
#include "EXTDOC.h"
#include "dbt.h"
#include "extdoc.h"
#include "barquery.h"
#include "barqydlg.h"
#include "XRefManager.h"
#include "xmlControls.h"
#include "XMLDocGenerator.h"

//resource
#include "XMLDocGenerator.hjson" //JSON AUTOMATIC UPDATE

//includere come ulo include all'inizio del cpp
#include "begincpp.dex"

#define TREE_ITEM_TYPE_ROOT 0
#define TREE_ITEM_TYPE_DBT	1

#define HEADER_VERSION		"1"

static const TCHAR szXMLFilesFilter[] = _T("File XML (*.xml)|*.xml|Tutti i file (*.*)|*.*||");
static const TCHAR szMaxDocDef	 [] = _T("10");
static const TCHAR szVersionDef	 [] = _T("1");


/////////////////////////////////////////////////////////////////////////////
// CDocumentoInfo
/////////////////////////////////////////////////////////////////////////////
class CDocumentoInfo : public CObject
{
public:
	CString m_strNamespace;
	CString m_strTitle;	

public:
	CDocumentoInfo(const CString& strNamespace, const CString& strTitle)
	:
	m_strNamespace(strNamespace),
	m_strTitle(strTitle)
	{}
};



//---------------------------------------------------------------------------
//	CXMLDocGenerator 
//---------------------------------------------------------------------------
//
//---------------------------------------------------------------------------
CXMLDocObjectInfo* CXMLDocGenerator::MakeInfo(CAbstractFormDoc* pDocument)
{
	if (!pDocument)
	{
		ASSERT(FALSE);
		return NULL;
	}

	const CDocumentDescription* pDocDescri = AfxGetDocumentDescription(pDocument->GetNamespace());
	BOOL isDynamic = (pDocDescri) ? pDocDescri->IsDynamic() : FALSE;

	CXMLDocObjectInfo* pDocInfo = pDocument->GetXMLDocInfo();
	if (pDocInfo)
	{
		if (pDocInfo->GetHeaderInfo() || pDocInfo->GetDBTInfoArray())
			return pDocInfo;
		delete pDocInfo;
	}

	pDocInfo = new CXMLDocObjectInfo(pDocument->GetNamespace());
	//informazioni di header
	CXMLHeaderInfo* pHeaderInfo = pDocInfo->m_pHeaderInfo = new CXMLHeaderInfo(pDocument->GetNamespace());

	pHeaderInfo->m_strVersion = HEADER_VERSION;
	pHeaderInfo->m_nMaxDocument = HEADER_DEFAULT_DOCUMENT_NUM;
		
	pHeaderInfo->m_strUrlData = pDocument->GetNamespace().GetObjectName(CTBNamespace::DOCUMENT);
	if (pHeaderInfo->m_strUrlData.IsEmpty())
		pHeaderInfo->m_strUrlData = pDocument->GetNamespace().ToString();

	if (pHeaderInfo->m_strUrlData.Right(4).CompareNoCase(szXmlExt) != 0)
		pHeaderInfo->m_strUrlData += szXmlExt;

	//array dei dbt
	CXMLDBTInfoArray* pXMLDBTInfoArray = pDocInfo->m_pDBTArray = new CXMLDBTInfoArray();

	/*CXMLClientDocInfoArray* pClientDocsInfo = NULL;
	if (pDocument->m_pClientDocs && pDocument->m_pClientDocs->GetSize() > 0)
	{
		pDocInfo->m_pClientDocsInfo = pClientDocsInfo = new CXMLClientDocInfoArray;
	
		CClientDoc* pClient = NULL;
		for (int nCli = 0; nCli < pDocument->m_pClientDocs->GetSize(); nCli++)
		{
			pClient = pDocument->m_pClientDocs->GetAt(nCli);
			if (pClient)
				pClientDocsInfo->Add(new CXMLClientDocInfo(pClient->m_Namespace, pClient->m_Namespace.ToString()));
 		}
	}
*/

	//master
	CXMLDBTInfo* pDBTMasterInfo = new CXMLDBTInfo;

	if (!pDocument->m_pDBTMaster ||
		!pDBTMasterInfo->SetDBTInfo(pDocument->m_pDBTMaster))
	{
		ASSERT(FALSE);
		TRACE1("CXMLDocGenerator::MakeInfo(): it's impossible to create the XML information about the DBTMaster of the document %s", pDocument->GetNamespace().ToString());
		
		if (pDocInfo) 
			delete pDocInfo;

		return NULL;	
	}

	
	pXMLDBTInfoArray->Add(pDBTMasterInfo);
	
	//array slave
	CXMLDBTInfo* pDBTSlaveInfo = NULL; 
	DBTSlave* pDBTSlave = NULL;
	if (pDocument->m_pDBTMaster->GetDBTSlaves())
	{
		for (int i = 0; i < pDocument->m_pDBTMaster->GetDBTSlaves()->GetSize(); i++)
		{
			//slave
			pDBTSlave = pDocument->m_pDBTMaster->GetDBTSlaves()->GetAt(i);
			if(pDBTSlave->IsDBTOnView())
				continue;

			pDBTSlaveInfo = new CXMLDBTInfo;
			pDBTSlaveInfo->SetDBTInfo(pDBTSlave);
		
			pXMLDBTInfoArray->Add(pDBTSlaveInfo);
		
			if (isDynamic)
				pDBTSlaveInfo->SetFrom(CXMLDBTInfo::CUSTOM);
			
			//è stato aggiunto da un clientdoc
			if (pDBTSlave->GetClientDocOwner())
			{
				pDBTSlaveInfo->SetFromClientDoc();
				if (!pDocInfo->m_pClientDocsInfo)
					pDocInfo->m_pClientDocsInfo = new CXMLClientDocInfoArray();

				//inserisco le info anche nelle info del clientdoc
				pDocInfo->m_pClientDocsInfo->AddDBTInfoToClient(pDBTSlave->GetClientDocOwner()->m_Namespace, pDBTSlaveInfo);
			}
		}
	}
	
	return pDocInfo;
}

//---------------------------------------------------------------------------
CXMLDocObjectInfo* CXMLDocGenerator::GenerateNewXMLDocInfo(CAbstractFormDoc* pDocument, CXMLDocObjectInfo* pOldDocObjInfo)
{
	if (!pOldDocObjInfo)
		return MakeInfo(pDocument);

	
	CXMLDocObjectInfo* pDocInfo = new CXMLDocObjectInfo(*pOldDocObjInfo);

	//verifico il dbt dbtmaster
	if (!pDocument->m_pDBTMaster)
		return false;

	//clientdocs
	if (pDocument->m_pClientDocs && pDocument->m_pClientDocs->GetSize() > 0)
	{
		if (!pDocInfo->GetClientDocInfoArray())
		{
			pDocInfo->m_pClientDocsInfo = new CXMLClientDocInfoArray();
			pDocInfo->m_pClientDocsInfo->SetOwns(FALSE);
		}
		
		CClientDoc* pClient = NULL;
		for (int nCli = 0; nCli < pDocument->m_pClientDocs->GetSize(); nCli++)
		{
			pClient = pDocument->m_pClientDocs->GetAt(nCli);

			if (pClient && !pDocInfo->m_pClientDocsInfo->GetClientFromNamespace(pClient->m_Namespace))
				pDocInfo->m_pClientDocsInfo->Add(new CXMLClientDocInfo(pClient->m_Namespace, pClient->m_Namespace.ToString()));			
 		}
	}

	//gestione DBTs
	if (!pDocInfo->GetDBTInfoArray())
		pDocInfo->m_pDBTArray = new CXMLDBTInfoArray();
		//pDocInfo->m_pDBTArray->SetOwns(FALSE);

	// dbtMaster
	CXMLDBTInfo* pDBTMasterInfo = pDocInfo->GetDBTFromNamespace(pDocument->m_pDBTMaster->GetNamespace());
	if (!pDBTMasterInfo)
	{
		pDBTMasterInfo = new CXMLDBTInfo();
		pDBTMasterInfo->SetDBTInfo(pDocument->m_pDBTMaster);
		pDocInfo->m_pDBTArray->Add(pDBTMasterInfo);
	}

	//slave e slavebuffered
	CXMLDBTInfo* pDBTSlaveInfo = NULL; 
	DBTSlave* pDBTSlave = NULL;
	if (pDocument->m_pDBTMaster->GetDBTSlaves())
	{
		for (int i = 0; i < pDocument->m_pDBTMaster->GetDBTSlaves()->GetSize(); i++)
		{
			//slave
			pDBTSlave = pDocument->m_pDBTMaster->GetDBTSlaves()->GetAt(i);
			if(pDBTSlave->IsDBTOnView())
				continue;

			pDBTSlaveInfo = pDocInfo->GetDBTFromNamespace(pDBTSlave->GetNamespace());
			if (!pDBTSlaveInfo)
			{
				pDBTSlaveInfo = new CXMLDBTInfo;
				pDBTSlaveInfo->SetDBTInfo(pDBTSlave);
				pDocInfo->m_pDBTArray->Add(pDBTSlaveInfo);

				//è stato aggiunto da un clientdoc
				if (pDBTSlave->GetClientDocOwner())
				{
					pDBTSlaveInfo->SetFromClientDoc();
					//inserisco le info anche nelle info del clientdoc
					pDocInfo->m_pClientDocsInfo->AddDBTInfoToClient(pDBTSlave->GetClientDocOwner()->m_Namespace, pDBTSlaveInfo);
				}
			}
		}
	}
	
	return pDocInfo;
}


//----------------------------------------------------------------------------
// CDocDescrMngPage dialog
//----------------------------------------------------------------------------

IMPLEMENT_DYNAMIC(CDocDescrMngPage, CParsedDialogWithTiles)
BEGIN_MESSAGE_MAP(CDocDescrMngPage, CParsedDialogWithTiles)
	//{{AFX_MSG_MAP(CDocDescrMngPage)
	ON_WM_CONTEXTMENU()
	ON_BN_CLICKED(ID_DOCDESCRI_LOAD, OnLoadDescription)
	ON_BN_CLICKED(ID_DOCDESCRI_SAVE, OnSaveDescription)
	
	ON_CBN_SELCHANGE(IDC_DOCDESCRI_COMBO_MODS, OnComboModsChanged)
	//}}AFX_MSG_MAP
END_MESSAGE_MAP()

//----------------------------------------------------------------------------
CDocDescrMngPage::CDocDescrMngPage(CWnd* pParent /*=NULL*/)
	:
	CParsedDialogWithTiles	(IDD_DOC_DESCR_PROPPAGE),
	m_bModified				(FALSE),
	m_pOldDocObjInfo		(NULL),
	m_pNewDocObjInfo		(NULL),
	m_pAllDocuments			(NULL),
	m_nProfileVersion		(0),
	m_oldModuleItemIdx		(0),
	m_oldDocumentItemIdx	(0)
{
}


//----------------------------------------------------------------------------
CDocDescrMngPage::~CDocDescrMngPage()
{
	if(m_pOldDocObjInfo)
		delete m_pOldDocObjInfo;

	if (m_pNewDocObjInfo)
		delete m_pNewDocObjInfo;
	
	if (m_pAllDocuments)
		delete m_pAllDocuments;	
} 

//----------------------------------------------------------------------------
BOOL CDocDescrMngPage::OnInitDialog() 
{
	m_clrBackground = AfxGetThemeManager()->GetTileDialogTitleBkgColor();

	CParsedDialogWithTiles::OnInitDialog();
	CParsedForm::SetBackgroundColor(m_clrBackground);

	SetToolbarStyle(ToolbarStyle::TOP, DEFAULT_TOOLBAR_HEIGHT, FALSE, TRUE);

	VERIFY (m_cbEnvelopeClass.	SubclassDlgItem(IDC_DOC_DESCR_ENVCLASSL_LIST,	this));
	VERIFY (m_TreeCtrl.			SubclassDlgItem(IDC_DOC_DESCR_TREE,	this));
	VERIFY(m_DocNsEdit.			SubclassDlgItem(IDC_DOC_NS, this));

	VERIFY(m_DataUrlEdit.SubclassDlgItem(IDC_DOC_DESCR_DATAURL_EDIT, this));

	m_TreeCtrl.InitializeImageList();
	FillModsCombo();

	EnableControls(FALSE);
		
	return TRUE; 
}

//------------------------------------------------------------------------------
void CDocDescrMngPage::OnCustomizeToolbar()
{
	//  applying developer requests on UI
	if (m_pToolBar)
	{
		m_pToolBar->AddComboBox(IDC_DOCDESCRI_COMBO_MODS, _NS_TOOLBARBTN("ComboMods"), 300, (WS_CHILD | WS_VISIBLE | CBS_NOINTEGRALHEIGHT | CBS_DROPDOWNLIST | WS_VSCROLL | CBS_SORT), _TB("Modules"));
		m_pToolBar->AddComboBox(IDC_DOCDESCRI_COMBO_DOCS, _NS_TOOLBARBTN("ComboDocs"), 300, (WS_CHILD | WS_VISIBLE | CBS_NOINTEGRALHEIGHT | CBS_DROPDOWNLIST | WS_VSCROLL | CBS_SORT), _TB("Documents"));
		m_pToolBar->AddSeparator();
		m_pToolBar->AddButton(ID_DOCDESCRI_LOAD, _NS_TOOLBARBTN("Load"), TBIcon(szIconExecute, IconSize::TOOLBAR), _TB("Load"));
		m_pToolBar->AddButton(ID_DOCDESCRI_SAVE, _NS_TOOLBARBTN("Save"), TBIcon(szIconSave, IconSize::TOOLBAR), _TB("Save"));
		m_pToolBar->AddButton(IDCANCEL, _NS_TOOLBARBTN("Escape"), TBIcon(szIconEscape, IconSize::TOOLBAR), _TB("Escape"));
	}
}

//--------------------------------------------------------------------------
void CDocDescrMngPage::FillModsCombo()
{
	CStringArray	aModules;
	CTBNamespace	Ns;
	CString			strMods = _T("");

	m_pAllDocuments = new Array();

	for (int i = 0; i < AfxGetAddOnAppsTable()->GetSize(); i++)
	{
		AddOnApplication* pAddOnApp = AfxGetAddOnAppsTable()->GetAt(i);
		if (!pAddOnApp || !pAddOnApp->m_pAddOnModules)
			continue;

		BOOL bGoodApp = FALSE;
		for (int j = 0; j < pAddOnApp->m_pAddOnModules->GetSize(); j++)
		{
			//carico solo i moduli attivi
			AddOnModule* pAddOnMod = pAddOnApp->m_pAddOnModules->GetAt(j);
			if (!pAddOnMod || !AfxIsActivated(pAddOnMod->GetApplicationName(), pAddOnMod->GetModuleName())
				|| pAddOnMod->GetModuleName().CompareNoCase(EASYSTUDIO_MODULE_NAME) == 0)
				continue;

			CBaseDescriptionArray* pDocArray = AfxGetDocumentDescriptionsOf(pAddOnMod->m_Namespace);
			Array* modDocs = new Array();
			for (int k = 0; k <= pDocArray->GetUpperBound(); k++)
			{
				CDocumentDescription* pDescri = (CDocumentDescription*)pDocArray->GetAt(k);
				if (pDescri && pDescri->IsRunnableAlone() && pDescri->GetFirstViewMode() && pDescri->GetFirstViewMode()->GetType() == VMT_DATAENTRY)
					modDocs->Add(new CDocumentoInfo(pDescri->GetNamespace().ToString(), pDescri->GetTitle()));
			}
			if (modDocs->GetSize() > 0)
			{
				m_pToolBar->AddComboSortedItem(IDC_DOCDESCRI_COMBO_MODS, pAddOnMod->GetModuleTitle(), (DWORD)modDocs);
				m_pAllDocuments->Add(modDocs);
			}
			else
				delete modDocs;

			if (pDocArray)
				delete pDocArray;
		}
	}
	if (m_pToolBar->GetComboCount(IDC_DOCDESCRI_COMBO_MODS) > 0)
		m_pToolBar->SetComboItemSel(IDC_DOCDESCRI_COMBO_MODS, 0);

	OnComboModsChanged();
}

//----------------------------------------------------------------------------
void CDocDescrMngPage::OnComboModsChanged()
{
	m_pToolBar->RemoveAllComboItems(IDC_DOCDESCRI_COMBO_DOCS);
	
	int	nCurSel = m_pToolBar->GetComboItemSel(IDC_DOCDESCRI_COMBO_MODS);
	Array* modDocs = (Array*)m_pToolBar->GetComboItemData(IDC_DOCDESCRI_COMBO_MODS, nCurSel);
	if (modDocs)
	{
		for (int i = 0; i < modDocs->GetSize(); i++)
		{
			CDocumentoInfo* pDoc = (CDocumentoInfo*)modDocs->GetAt(i);
			if (pDoc)
				m_pToolBar->AddComboSortedItem(IDC_DOCDESCRI_COMBO_DOCS, pDoc->m_strTitle, (DWORD)pDoc);
		}	
	}

	if (m_pToolBar->GetComboCount(IDC_DOCDESCRI_COMBO_DOCS) > 0)
		m_pToolBar->SetComboItemSel(IDC_DOCDESCRI_COMBO_DOCS, 0);
}

//----------------------------------------------------------------------------
void PostCreateDocument(CAbstractFormDoc* pDocument)
{
	if (pDocument->GetMasterFrame())
		pDocument->GetMasterFrame()->ShowWindow(SW_HIDE);
}

//----------------------------------------------------------------------------
void CDocDescrMngPage::EnableControls(BOOL bEnable)
{
	m_cbEnvelopeClass.EnableWindow(bEnable);
	m_TreeCtrl.EnableWindow(bEnable);
	m_DataUrlEdit.EnableWindow(bEnable);
	m_DocNsEdit.EnableWindow(bEnable);

	if (!bEnable)
	{
		m_TreeCtrl.DeleteAllItems();
		m_DataUrlEdit.ClearCtrl();
		m_DocNsEdit.ClearCtrl();
		m_cbEnvelopeClass.Clear();
	}
}
//----------------------------------------------------------------------------
void CDocDescrMngPage::ReturnToOldSelection()
{
	m_pToolBar->SetComboItemSel(IDC_DOCDESCRI_COMBO_MODS, m_oldModuleItemIdx);
	OnComboModsChanged();
	m_pToolBar->SetComboItemSel(IDC_DOCDESCRI_COMBO_DOCS, m_oldDocumentItemIdx);		
}

//----------------------------------------------------------------------------
void CDocDescrMngPage::OnLoadDescription()
{
	int	nCurSel = m_pToolBar->GetComboItemSel(IDC_DOCDESCRI_COMBO_DOCS);
	if (nCurSel < 0)
		return;

	CDocumentoInfo* pDocInfo = (CDocumentoInfo*)m_pToolBar->GetComboItemData(IDC_DOCDESCRI_COMBO_DOCS, nCurSel);

	if (!pDocInfo || pDocInfo->m_strNamespace.IsEmpty())
		return;

	if (m_DocNsEdit.GetValue().CollateNoCase(pDocInfo->m_strNamespace) == 0)
		return;
	
	if (!SaveModified())
	{
		ReturnToOldSelection();
		return;
	}
	if (m_pOldDocObjInfo)
		delete m_pOldDocObjInfo;

	if (m_pNewDocObjInfo)
		delete m_pNewDocObjInfo;

	m_DocNsEdit.SetValue(pDocInfo->m_strNamespace);
	//GetDlgItem(IDC_DOC_NS)->SetWindowTextW(*pDocNs);
	CTBNamespace aNsDocument(pDocInfo->m_strNamespace);
	const CDocumentDescription* docDescription = AfxGetDocumentDescription(aNsDocument);
	CAbstractFormDoc* pDocument = NULL;
	
	BeginWaitCursor();
	if (docDescription)
	{
		const CBaseDocument* pBaseDoc = AfxGetTbCmdManager()->RunDocument(pDocInfo->m_strNamespace, szDefaultViewMode, FALSE);
		
		if (pBaseDoc && pBaseDoc->IsKindOf(RUNTIME_CLASS(CAbstractFormDoc)))
		{
			pDocument = (CAbstractFormDoc*)pBaseDoc;
			AfxInvokeThreadGlobalProcedure<CAbstractFormDoc*>(pDocument->GetThreadId(), &PostCreateDocument, pDocument);
		}
	}	
	EndWaitCursor();

	BOOL bContinue = (pDocument && pDocument->m_pDBTMaster);
	EnableControls(bContinue);
	if (!bContinue)
	{
		if (pDocument)
			pDocument->CloseDocument();
		ReturnToOldSelection();
		return;
	}

	CXMLDocObjectInfo* pOldDocInfo = new CXMLDocObjectInfo(pDocument->GetNamespace());
	if (pOldDocInfo->LoadAllFiles())
		m_pOldDocObjInfo = pOldDocInfo;
	else
	{
		delete pOldDocInfo;
		m_pOldDocObjInfo = NULL;
	}

	CXMLDocGenerator aDocGenerator;
	m_pNewDocObjInfo = aDocGenerator.GenerateNewXMLDocInfo(pDocument, m_pOldDocObjInfo);
	m_bModified = TRUE;
	

	if (m_pNewDocObjInfo)
	{
		//riempie i controlli dell'header e l'albero
		FillHeader();
		m_TreeCtrl.EnableDBTNaming();
		m_TreeCtrl.SetXMLDocObjInfo(m_pNewDocObjInfo);
		m_TreeCtrl.FillTree(m_pOldDocObjInfo);
	}
	else
		EnableControls(bContinue);

	m_oldModuleItemIdx = m_pToolBar->GetComboItemSel(IDC_DOCDESCRI_COMBO_MODS);
	m_oldDocumentItemIdx = m_pToolBar->GetComboItemSel(IDC_DOCDESCRI_COMBO_DOCS);

	pDocument->CloseDocument();
}

//----------------------------------------------------------------------------
void CDocDescrMngPage::FillHeader()
{
	GetDlgItem(IDC_DOC_DESCR_VERSION_EDIT)->SetWindowText(m_pNewDocObjInfo->GetVersion());
	
	CString strUrlData = m_pNewDocObjInfo->GetUrlData();
	if(strUrlData.IsEmpty())
		strUrlData = m_pNewDocObjInfo->GetNamespaceDoc().ToString();
	
	if(strUrlData.Right(4).CompareNoCase(szXmlExt) != 0)
		strUrlData += szXmlExt;

	GetDlgItem(IDC_DOC_DESCR_DATAURL_EDIT)->SetWindowText(strUrlData);
	FillEnvClassCombo(m_pNewDocObjInfo->GetEnvClass());
}

//----------------------------------------------------------------------------
void CDocDescrMngPage::FillEnvClassCombo(const CString& strEnvClass)
{
	// se è già stata caricata effettuo solo la selezione sulla stringa strEnvClass
	// se diversa da Empty
	ASSERT(AfxGetAddOnAppsTable());
	const CStringArray* pEnvClassArray;
	int nCurrSel = 0;
	int nPos = 0;
	
	if (m_cbEnvelopeClass.GetCount() > 0)
	{
		nPos = m_cbEnvelopeClass.SelectString(-1, (LPCTSTR)strEnvClass);		
		if (nPos == m_cbEnvelopeClass.GetCurSel())
			return;		
		if (nPos != LB_ERR) 
			nCurrSel = nPos;
	}
	else	
	for (int i = 0; i <= AfxGetAddOnAppsTable()->GetUpperBound(); i++)
	{
		pEnvClassArray = AfxGetAddOnAppsTable()->GetAt(i)->GetAvailableDocEnvClasses(m_pNewDocObjInfo->GetNamespaceDoc());
		if (pEnvClassArray)
			for (int j = 0; j <= pEnvClassArray->GetUpperBound(); j++)
			{
				CString sClass = pEnvClassArray->GetAt(j);
				CString sItem;
				BOOL existing = FALSE;
				for (int k = 0; k < m_cbEnvelopeClass.GetCount(); k++)
				{
					m_cbEnvelopeClass.GetLBText(k, sItem);
					if (sItem.CompareNoCase(sClass) == 0)
					{
						existing = TRUE;
						break;
					}
				}
				if (existing)
					continue;

				nPos = m_cbEnvelopeClass.InsertString(-1, sClass);
				if (!strEnvClass.IsEmpty() && pEnvClassArray->GetAt(j).CompareNoCase(strEnvClass) == 0)
					nCurrSel = nPos;
			}
	}

	m_cbEnvelopeClass.SetCurSel	(nCurrSel);
}

//----------------------------------------------------------------------------
void CDocDescrMngPage::OnContextMenu(CWnd* pWnd, CPoint point) 
{
	m_TreeCtrl.OnContextMenu(pWnd, point);
}

//----------------------------------------------------------------------------
BOOL CDocDescrMngPage::IsDifferentDescription()
{
	if (m_pNewDocObjInfo && !m_pOldDocObjInfo)
		return TRUE;

	BOOL bIsDifferent =  (m_pNewDocObjInfo && m_pOldDocObjInfo) &&
		((!(m_pNewDocObjInfo->m_pHeaderInfo && m_pOldDocObjInfo->m_pHeaderInfo && *m_pNewDocObjInfo->m_pHeaderInfo == *m_pOldDocObjInfo->m_pHeaderInfo)) ||
		(!(m_pNewDocObjInfo->m_pDBTArray && m_pOldDocObjInfo->m_pDBTArray && *m_pNewDocObjInfo->m_pDBTArray == *m_pOldDocObjInfo->m_pDBTArray))
			);
	
	return bIsDifferent;
}

//----------------------------------------------------------------------------
void CDocDescrMngPage::OnSaveDescription()
{
	if (IsDifferentDescription())
	{
		UpdateDataValue(m_pNewDocObjInfo);
		if (m_pNewDocObjInfo->SaveAllFiles())
		{
			if (m_pOldDocObjInfo)
				delete m_pOldDocObjInfo;
			//se ho salvato mediante il bottone della toolbar ora l'old è uguale al new
			m_pOldDocObjInfo = new CXMLDocObjectInfo(*m_pNewDocObjInfo);
			m_TreeCtrl.FillTree(m_pOldDocObjInfo);
		}
		else
			AfxGetDiagnostic()->Show(TRUE);
	}
}

//----------------------------------------------------------------------------
BOOL CDocDescrMngPage::SaveModified()
{
	if (IsDifferentDescription())
	{
		if (AfxMessageBox(_TB("The document description has been modified. Would you like to save before close it?"), MB_YESNO | MB_ICONQUESTION) == IDYES)
		{
			UpdateDataValue(m_pNewDocObjInfo);
			if (!m_pNewDocObjInfo->SaveAllFiles())
			{
				AfxGetDiagnostic()->Add(_TB("Would you like to continue?"), CDiagnostic::Warning);
				return AfxGetDiagnostic()->Show(TRUE);
			}
		}		
	}
	return TRUE;
}

//----------------------------------------------------------------------------
void CDocDescrMngPage::OnCancel() 
{
	if (SaveModified())
		EndDialog(IDCANCEL);
}

//----------------------------------------------------------------------------
void CDocDescrMngPage::UpdateDataValue(CXMLDocObjectInfo* pXMLDocObject)
{
	if (!pXMLDocObject)
		return;
	
	CString strVal;
	
	GetDlgItemText(IDC_DOC_DESCR_DATAURL_EDIT, strVal);
	if(strVal.Right(4).CompareNoCase(szXmlExt) != 0)
		strVal += szXmlExt;

	pXMLDocObject->SetUrlData(strVal);

	GetDlgItemText(IDC_DOC_DESCR_VERSION_EDIT, strVal);
	pXMLDocObject->SetVersion(strVal);

	GetDlgItemText(IDC_DOC_DESCR_ENVCLASSL_LIST, strVal);
	pXMLDocObject->SetEnvClass(strVal);
}

