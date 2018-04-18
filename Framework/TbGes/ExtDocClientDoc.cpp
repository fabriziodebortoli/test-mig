
#include "stdafx.h"

#include <TbGeneric\DataObj.h>
#include <TbGeneric\LocalizableObjs.h>
#include <TbGenlib\parsobj.h>
#include <TbGenlib\generic.h>
#include "browser.h"
#include "bodyedit.h"
#include "dbt.h"
#include "eventmng.h"
#include "extdoc.h"
#include "tabber.h"
#include "tilemanager.h"
#include "JsonFormEngineEx.h"

//includere come ultimo include all'inizio del cpp
#include "begincpp.dex"
//============================================================================

//////////////////////////////////////////////////////////////////////////////
//					CClientDoc implementation
//////////////////////////////////////////////////////////////////////////////
//
IMPLEMENT_DYNCREATE(CClientDoc, CCmdTarget);

//-----------------------------------------------------------------------------
CClientDoc::CClientDoc()
	:
	IDisposingSourceImpl	(this),

	m_pServerDocument		(NULL), 
	m_MsgRouting			(CD_MSG_BEFORE), 
	m_MsgState				(ON_BEFORE_MSG),
	m_pEventManager			(NULL)
{
}


//-----------------------------------------------------------------------------
CClientDoc::~CClientDoc()
{
	SAFE_DELETE(m_pEventManager);
}

//-----------------------------------------------------------------------------
void CClientDoc::Init(const CString& sDocName, const CTBNamespace& aParent)
{
	m_Namespace.SetNamespace(aParent);
	m_Namespace.SetObjectName(CTBNamespace::DOCUMENT, sDocName, TRUE);
}

//-----------------------------------------------------------------------------
void CClientDoc::Attach(CAbstractFormDoc* pDocument)
{ 
	m_pServerDocument = pDocument; 
}

//-----------------------------------------------------------------------------
void CClientDoc::Attach(CEventManager* pEvMng)
{
	ASSERT(pEvMng);
	ASSERT(pEvMng->IsKindOf(RUNTIME_CLASS(CEventManager)));

	pEvMng->AttachDocument(this);
}

//-----------------------------------------------------------------------------
void CClientDoc::AttachHotLink(HotKeyLink* pHKL, CString strName /*_T("")*/)
{
	m_pServerDocument->Attach(pHKL, strName);
}

//-----------------------------------------------------------------------------
BOOL CClientDoc::AttachDBTSlave(DBTSlave* pDBTSlave)
{
	if (m_pServerDocument->m_pDBTMaster)
	{
		m_pServerDocument->m_pDBTMaster->Attach(pDBTSlave);
		pDBTSlave->InstantiateFromClientDoc(this);
		return TRUE;
	}
	return FALSE;
}
//-----------------------------------------------------------------------------
CAbstractFormView* CClientDoc::GetViewByCtrlID (UINT nIDC)
{
	if (!m_pServerDocument || !m_pServerDocument->GetMasterFrame())
		return NULL;

	CAbstractFormFrame* pFrame = m_pServerDocument->GetMasterFrame();
	return pFrame-> GetViewByCtrlID (nIDC);
}
//-----------------------------------------------------------------------------
void CClientDoc::SetTabDialogImage(UINT nTabberIDD, UINT nTabDialogIDD, CString aNsImage)
{
	CAbstractFormView* pView = GetViewByCtrlID(nTabberIDD);
	if (!pView)
		return;
	
	CBaseTabManager* pTabManager = pView->GetTabber(nTabberIDD);
	if (!pTabManager)
		return;

	pTabManager->SetTabDialogImage(nTabDialogIDD, aNsImage);
}
//-----------------------------------------------------------------------------
void CClientDoc::OnPrepareAuxData(CTabDialog* pTab)
{ 
	OnPrepareAuxData(pTab->GetDlgCtrlID());
}
//-----------------------------------------------------------------------------
void CClientDoc::OnPrepareAuxData(CTileGroup* pGroup) 
{ 
	OnPrepareAuxData(pGroup->GetDlgCtrlID());
}
//-----------------------------------------------------------------------------
void CClientDoc::OnPrepareAuxData(CTileDialog* pTile)
{
	OnPrepareAuxData(pTile->GetDlgCtrlID());
}

//-----------------------------------------------------------------------------
void CClientDoc::AddTabDialog
	(
	UINT			nTabDlgID,
	CRuntimeClass*	pTabDlgClass,
	const CString&	strTabMngRTName,
	int				nOrdPos,
	UINT			nBeforeIDD,
	const CString	nsSelectorImage,
	const CString	sSelectorTooltip
	)
{
	m_TabDialogs.Add
		(
		new CClientDocTabDlg
			(
				nTabDlgID,
				pTabDlgClass,
				strTabMngRTName,
				nOrdPos,
				nBeforeIDD,
				nsSelectorImage,
				sSelectorTooltip
			)
		);
}

//-----------------------------------------------------------------------------
void CClientDoc::AddTileGroup
	(
	CRuntimeClass*  pTileGroupClass,
	CString			sNameTileGroup,
	CString			sTitleTileGroup,
	UINT			nTileGroupID,
	CString			sTileGroupImage,
	CString			sTooltip,
	int				nOrdPos,
	UINT			nBeforeIDD,
	const CString&	strTileMngRTName
	)
{
	m_TileGroups.Add
		(
			new CClientDocTileGroup
			(
				pTileGroupClass,
				sNameTileGroup,
				sTitleTileGroup,
				sTileGroupImage,
				sTooltip,
				nTileGroupID,
				strTileMngRTName
			)
		);
}

//-----------------------------------------------------------------------------
void CClientDoc::AddTileDialog
	(
	UINT			nTileDlgID,
	CRuntimeClass*	pTileDlgClass,
	const CString&	strTileDlgTitle,
	TileDialogSize	aTileSize,
	CRuntimeClass*	pTileGroupClass,
	UINT			nBeforeIDD,
	int				nFlex,
	UINT			nAfterIDD
	)
{
	m_TileDialogs.Add
		(
		new CClientDocTileDialog
			(
			nTileDlgID,
			pTileDlgClass,
			strTileDlgTitle,
			aTileSize,
			nFlex,
			pTileGroupClass,
			nBeforeIDD,
			nAfterIDD
			)
		);
}

//-----------------------------------------------------------------------------
void CClientDoc::AddTileDialog
	(
	UINT			nTileDlgID,
	CRuntimeClass*	pTileDlgClass,
	const CString&	strTileDlgTitle,
	TileDialogSize	aTileSize,
	UINT			nTileGroupID,
	UINT			nBeforeIDD,
	int				nFlex,
	UINT			nAfterIDD
	)
{
	m_TileDialogs.Add
		(
		new CClientDocTileDialog
			(
			nTileDlgID,
			pTileDlgClass,
			strTileDlgTitle,
			aTileSize,
			nFlex,
			nTileGroupID,
			nBeforeIDD,
			nAfterIDD
			)
		);
}
//-----------------------------------------------------------------------------
void CClientDoc::SetTabTileGroupImage (UINT nTabberIDD, UINT nTileGroupID, CString aNsImage)
{
	CAbstractFormView* pView = GetViewByCtrlID(nTabberIDD);
	if (!pView)
		return;
	
	CBaseTabManager* pTabManager = pView->GetTabber(nTabberIDD);
	if (!pTabManager)
		return;

	UINT nIDD = ((CTabManager*)pTabManager)->GetTabDialogID(nTileGroupID);
	if (nIDD > 0)
		pTabManager->SetTabDialogImage(nIDD, aNsImage);

}

//-----------------------------------------------------------------------------
void CClientDoc::CustomizeTabber(CTabManager* pTabMng)
{
	USES_CONVERSION;
	CString strTabManagerName = pTabMng->GetNamespace().GetObjectName();

	// tab dialogs
	for (int i = 0; i <= m_TabDialogs.GetUpperBound(); i++)
	{
		CClientDocTabDlg* pTabDlg = m_TabDialogs.GetAt(i);
		if 
			(
				(pTabDlg->m_strTabMngRTName.CompareNoCase(A2T((LPSTR)pTabMng->GetRuntimeClass()->m_lpszClassName)) == 0)	||
				(pTabDlg->m_strTabMngRTName.CompareNoCase(strTabManagerName) == 0) ||
				(pTabDlg->m_strTabMngRTName.IsEmpty() && !pTabDlg->m_bAttached)
			)
		{
			pTabMng->AddDialog(pTabDlg->m_pTabDlgClass, pTabDlg->m_nTabDlgID, pTabDlg->m_nOrdPos, pTabDlg->m_nBeforeIDD, pTabDlg->m_nsSelectorImage, pTabDlg->m_sSelectorTooltip);
			pTabDlg->m_bAttached = TRUE;
		}
	}

	// tile groups
	if (pTabMng->IsKindOf(RUNTIME_CLASS(CTileManager)) && m_TileGroups.GetCount() > 0)
	{
		CTileManager* pTileManager = (CTileManager*)pTabMng;
		for (int i = 0; i <= m_TileGroups.GetUpperBound(); i++)
		{
			CClientDocTileGroup* pTileGroup = m_TileGroups.GetAt(i);
			if 
				(
					(pTileGroup->m_strTileMngRTName.CompareNoCase(A2T((LPSTR)pTileManager->GetRuntimeClass()->m_lpszClassName)) == 0)	||
					(pTileGroup->m_strTileMngRTName.CompareNoCase(strTabManagerName) == 0) ||
					(pTileGroup->m_strTileMngRTName.IsEmpty() && !pTileGroup->m_bAttached)
				)
			{
				pTileManager->AddTileGroup(
					pTileGroup->m_pTileGroupClass,
					pTileGroup->m_sNameTileGroup,
					pTileGroup->m_sTitleTileGroup,
					pTileGroup->m_sTileGroupImage,
					pTileGroup->m_sTooltip,
					pTileGroup->m_nTileGroupID);

				pTileGroup->m_bAttached = TRUE;
			}
		}
	}
}
//-----------------------------------------------------------------------------
void CClientDoc::CustomizeTileGroup(CTileGroup* pTileGroup)
{
	if (m_TileDialogs.GetCount() <= 0)
		return;

	for (int i = 0; i <= m_TileDialogs.GetUpperBound(); i++)
	{
		CClientDocTileDialog* pTileDialog = m_TileDialogs.GetAt(i);

		//se mi è stato specificato un ID, deve appiccicarmi al tile group con qeull'id
		if (pTileDialog->m_nTileGroupID != 0 && 
			(!pTileGroup->m_pDlgInfoItem || pTileDialog->m_nTileGroupID != pTileGroup->m_pDlgInfoItem->GetTileGroupID()))
			continue;

		//altrimenti se mi è stato specificata una runtime class, allora il tiel group deve essere di quel tipo
		if (pTileDialog->m_pTileGroupClass != NULL &&
			pTileDialog->m_pTileGroupClass != pTileGroup->GetRuntimeClass() &&
			!pTileGroup->GetRuntimeClass()->IsDerivedFrom(pTileDialog->m_pTileGroupClass))
			continue;
		
		CBaseTileDialog* pTile = (pTileDialog->m_pTileDlgClass == RUNTIME_CLASS(CJsonTileDialog)) 
			? pTileGroup->AddJsonTile(pTileDialog->m_nTileDlgID)
			: pTileGroup->AddTile(pTileDialog->m_pTileDlgClass, pTileDialog->m_nTileDlgID, pTileDialog->m_strTileDlgTitle, pTileDialog->m_TileSize);

		if (pTileDialog->m_nBeforeIDD != 0)
			pTileGroup->MoveTileByIDD(pTile, pTileDialog->m_nBeforeIDD, false);
		else if (pTileDialog->m_nAfterIDD != 0)
			pTileGroup->MoveTileByIDD(pTile, pTileDialog->m_nAfterIDD, true);

		//@@TODO non gestibile in questo modo?
		//pTileDialog->m_bAttached = TRUE;
		
	}
}

//-----------------------------------------------------------------------------
CTBToolbarButton* CClientDoc::FindButtonPtr(UINT nCommandID, const CString& sToolBarName)
{
	if (m_pServerDocument && m_pServerDocument->GetMasterFrame() && m_pServerDocument->GetMasterFrame()->GetTabbedToolBar())
	{
		CTBTabbedToolbar* pTabbedToolbar = m_pServerDocument->GetMasterFrame()->GetTabbedToolBar();
		CTBToolBar* pToolBar = pTabbedToolbar->FindToolBar(sToolBarName);
		if (pToolBar)
		{
			return pToolBar->FindButtonPtr(nCommandID);
		}
	}
	return NULL;
}

//-----------------------------------------------------------------------------
void CClientDoc::AddComboBox(UINT nID, const CString& aLibNamespace, const CString& sName, int nWidth, DWORD dwStyle, const CString& sToolBarName)
{
	m_pServerDocument->m_ToolBarButtons.AddComboBox(nID, aLibNamespace, sName, nWidth, dwStyle, sToolBarName);
}

//-----------------------------------------------------------------------------
void CClientDoc::AddEdit(UINT nID, const CString& aLibNamespace, const CString& sName, int nWidth, DWORD dwStyle, const CString& sToolBarName)
{
	m_pServerDocument->m_ToolBarButtons.AddEdit(nID, aLibNamespace, sName, nWidth, dwStyle, sToolBarName);
}
	
//-----------------------------------------------------------------------------
void CClientDoc::AddLabel (UINT nID, const CString& szText, const CString& sToolBarName)
{
	m_pServerDocument->m_ToolBarButtons.AddLabel(nID, szText, sToolBarName);
}
	
//-----------------------------------------------------------------------------
void CClientDoc::AddSeparator(const CString& sToolBarName)
{
	m_pServerDocument->m_ToolBarButtons.AddSeparator(sToolBarName);
}
//-----------------------------------------------------------------------------
void CClientDoc::AddDropdownMenuItem(UINT nCommandID, UINT_PTR nIDNewItem, const CString& sNewItem, const CString& sToolBarName)
{
	m_pServerDocument->m_ToolBarButtons.AddDropdownMenuItem(nCommandID, nIDNewItem, sNewItem, sToolBarName);
}

//-----------------------------------------------------------------------------
void CClientDoc::AddButton 
		(
					UINT		nCommandID, 
			const	CString&	sName, 
			const	CString&	sImageNameSpace,
			const	CString&	sText, 
			const	CString&	sToolBarName /*= _T("")*/,
			const	CString&	sToolTip /*= _T("")*/,
					BOOL		bDropdown /*= FALSE*/
		)
{
	if (sToolBarName.IsEmpty())
	{
		ASSERT(FALSE);
		return;
	}

	m_pServerDocument->m_ToolBarButtons.AddButton(
		nCommandID,
		sName,
		sText,
		sImageNameSpace,
		sToolBarName,
		sToolTip,
		bDropdown);

	
}

//-----------------------------------------------------------------------------
void CClientDoc::SetButtonInfo(UINT nID, UINT nStyle, UINT nIDImage, LPCTSTR lpszText, BOOL bPng)
{
	CTBTabbedToolbar* pTabbedToolbar = m_pServerDocument->GetMasterFrame()->GetTabbedToolBar();
	ASSERT(pTabbedToolbar);

	pTabbedToolbar->SetButtonInfo(nID, nStyle, nIDImage, lpszText, bPng);
}
//-----------------------------------------------------------------------------
void CClientDoc::DeclareVariable(const CString & sName, DataObj* pDataObj)
{
	m_pServerDocument->DeclareVariable(sName, pDataObj);
}
//-----------------------------------------------------------------------------
void CClientDoc::DeclareVariable(const CString & sName, DataObj& aDataObj)
{
	DeclareVariable(sName, &aDataObj);
}
//-----------------------------------------------------------------------------
CTBToolBar* CClientDoc::GetToolBar(const CString& sToolBarName)
{
	CMasterFrame* pFrame = m_pServerDocument->GetMasterFrame();

	CTBTabbedToolbar* pTabbedToolbar = pFrame->GetTabbedToolBar();
	if (pTabbedToolbar && m_pServerDocument->GetMasterFrame()->HasToolbar())
	{
		ASSERT(pTabbedToolbar);
		CTBToolBar* pToolBar = pTabbedToolbar->FindToolBar(sToolBarName);
		return pToolBar;
	}
	return NULL;
}
//-----------------------------------------------------------------------------
BOOL CClientDoc::CreateJsonToolbar(UINT nID)
{
	CMasterFrame* pFrame = m_pServerDocument->GetMasterFrame();
	return pFrame->CreateJsonToolbar(nID);
}

//------------------------------------------------------------------------------
CView* CClientDoc::CreateSlaveView (const CRuntimeClass* pClass, const CString& strSubTitle, const CRuntimeClass* pClientClass, const CString& strFormName)
{
	// I have to notify client doc form existance
	CTBNamespace aFormNs;
	aFormNs.SetChildNamespace(CTBNamespace::FORM, strFormName, m_pServerDocument->GetNamespace());

	return m_pServerDocument->CreateSlaveView(pClass, strSubTitle, pClientClass, strFormName);
}

//------------------------------------------------------------------------------
CView* CClientDoc::CreateSlaveView(UINT nFrameId, CWnd* pParent /*=NULL*/, BOOL bModal /*= FALSE*/)
{
	return m_pServerDocument->CreateSlaveView(nFrameId, pParent, bModal);
}

//------------------------------------------------------------------------------
CManagedDocComponentObj* CClientDoc::GetComponent (CString& sParentNamespace, CString& sName)
{
	return NULL;
}

//-----------------------------------------------------------------------------
void CClientDoc::GetComponents (CManagedDocComponentObj* pRequest, Array& returnedComponents)
{
}

//-----------------------------------------------------------------------------
BOOL CClientDoc::NamespaceEquals(const CString& aCDNamespace)
{
	return m_Namespace.ToUnparsedString().CompareNoCase(aCDNamespace) == 0;
}

//-----------------------------------------------------------------------------
void CClientDoc::PopulateMessagesIDsArrayForPushToClients(CArray<int>& arIDs)
{
	m_pServerDocument->PopulateIDsArrayFromMessageMap(GetMessageMap(), arIDs);
}

//////////////////////////////////////////////////////////////////////////////
//					CClientDocArray implementation
//////////////////////////////////////////////////////////////////////////////
//
#define VOID_ITERATOR(a)\
	void CClientDocArray::a() \
	{ \
		for (int i = 0; i <= this->GetUpperBound(); i++) GetAt(i)->a(); \
	}

#define BOOL_ITERATOR(a)\
	BOOL CClientDocArray::a() \
	{ \
		BOOL bOk = TRUE; \
		for (int i = 0; i <= GetUpperBound(); i++) \
			bOk = bOk && GetAt(i)->a(); \
		return bOk; \
	}

#define LOCK_ITERATOR(a)\
	CAbstractFormDoc::LockStatus CClientDocArray::a() \
	{ \
		for (int i = 0; i <= GetUpperBound(); i++) \
		{ \
			CAbstractFormDoc::LockStatus status = GetAt(i)->a(); \
			if (status != CAbstractFormDoc::ALL_LOCKED) return status; \
		} \
		return CAbstractFormDoc::ALL_LOCKED; \
	}

//-----------------------------------------------------------------------------
int CClientDocArray::Add(CClientDoc* pClientDoc, BOOL bCheckDuplicates /*= TRUE*/)	
{ 
	if (bCheckDuplicates)
		for (int i = 0; i <= GetUpperBound(); i++)
			//se è già stato associato lo devo distuggere e non agganciarlo di nuovo
			if (GetAt(i)->GetRuntimeClass() == pClientDoc->GetRuntimeClass()) 
			{
				delete pClientDoc;
				pClientDoc = NULL;
				return -1;
			}
	return Array::Add(pClientDoc);
}

//-----------------------------------------------------------------------------
void CClientDocArray::Attach (CAbstractFormDoc* pDocument)	
{ 
	for (int i = 0; i <= GetUpperBound(); i++)
		GetAt(i)->Attach(pDocument);
}

//-----------------------------------------------------------------------------
void CClientDocArray::CustomizeTabber(CTabManager* pTabMng)
{
	for (int i = 0; i <= GetUpperBound(); i++)
		GetAt(i)->CustomizeTabber(pTabMng);
}

//-----------------------------------------------------------------------------
void CClientDocArray::CustomizeTileGroup(CTileGroup* pTileGroup)
{
	for (int i = 0; i <= GetUpperBound(); i++)
		GetAt(i)->CustomizeTileGroup(pTileGroup);
}

//-----------------------------------------------------------------------------
BOOL CClientDocArray::PreTranslateMsg(HWND hWnd, MSG* pMsg)
{
	for (int i = 0; i <= GetUpperBound(); i++)
	{
		CClientDoc* pCD = GetAt(i);
		if (pCD && pCD->PreTranslateMsg(hWnd, pMsg)) return TRUE;
	}
	
	return FALSE;
}

//-----------------------------------------------------------------------------
void CClientDocArray::CustomizeBodyEdit(CBodyEdit* pBodyEdit)
{
	for (int i = 0; i <= GetUpperBound(); i++)
		GetAt(i)->CustomizeBodyEdit(pBodyEdit);
}

//-----------------------------------------------------------------------------
void CClientDocArray::CustomizeGridControl(CTBGridControl* pGridControl)
{
	for (int i = 0; i <= GetUpperBound(); i++)
		GetAt(i)->CustomizeGridControl(pGridControl);
}

//-----------------------------------------------------------------------------
BOOL CClientDocArray::OnPostCreateClient(CBodyEdit* pBodyEdit)
{
	for (int i = 0; i <= GetUpperBound(); i++)
		if (!GetAt(i)->OnPostCreateClient(pBodyEdit))
			return FALSE;
	return TRUE;
}

//-----------------------------------------------------------------------------
BOOL CClientDocArray::OnShowingBodyEditContextMenu(CBodyEdit* pBodyEdit, CMenu* pMenu, int nCol, int nRow, CPoint ptClient)
{
	BOOL bOk = TRUE;
	for (int i = 0; i <= GetUpperBound(); i++)
		bOk = GetAt(i)->OnShowingBodyEditContextMenu(pBodyEdit, pMenu, nCol, nRow, ptClient) && bOk;

	return bOk;
}

//-----------------------------------------------------------------------------
void CClientDocArray::OnInitializeUI(const CTBNamespace& aFormNs)
{	
	for (int i = 0; i <= GetUpperBound(); i++)
		GetAt(i)->OnInitializeUI(aFormNs);
}

//-----------------------------------------------------------------------------
void CClientDocArray::OnBuildDataControlLinks(CTabDialog* pTabDlg)
{	
	for (int i = 0; i <= GetUpperBound(); i++)
		GetAt(i)->OnBuildDataControlLinks(pTabDlg);
}

//-----------------------------------------------------------------------------
void CClientDocArray::OnDestroyTabDialog(CTabDialog* pTabDlg)
{	
	for (int i = 0; i <= GetUpperBound(); i++)
		GetAt(i)->OnDestroyTabDialog(pTabDlg);
}
//-----------------------------------------------------------------------------
CString CClientDocArray::OnGetCaption(CAbstractFormView* pView)
{	
	if (this == NULL)
		return  _T("");
	for (int i = 0; i <= GetUpperBound(); i++)
	{
		CString sCaption = GetAt(i)->OnGetCaption(pView);
		if (!sCaption.IsEmpty())
			return sCaption;
	}
	return _T("");
}

//-----------------------------------------------------------------------------
void CClientDocArray::OnBuildDataControlLinks(CAbstractFormView* pView)
{
	for (int i = 0; i <= GetUpperBound(); i++)
		GetAt(i)->OnBuildDataControlLinks(pView);
}

//-----------------------------------------------------------------------------
void CClientDocArray::OnBuildDataControlLinks(CTileDialog* pTile)
{
	for (int i = 0; i <= GetUpperBound(); i++)
		GetAt(i)->OnBuildDataControlLinks(pTile);
}
//-----------------------------------------------------------------------------
void CClientDocArray::OnPrepareAuxData(UINT nID)
{	
	for (int i = 0; i <= GetUpperBound(); i++)
		GetAt(i)->OnPrepareAuxData(nID);
}
//-----------------------------------------------------------------------------
void CClientDocArray::OnPrepareAuxData(CTileGroup* pTileGroup)
{	
	for (int i = 0; i <= GetUpperBound(); i++)
		GetAt(i)->OnPrepareAuxData(pTileGroup);
}
//-----------------------------------------------------------------------------
void CClientDocArray::OnPrepareAuxData(CTabDialog* pTabDlg)
{
	for (int i = 0; i <= GetUpperBound(); i++)
		GetAt(i)->OnPrepareAuxData(pTabDlg);
}

//-----------------------------------------------------------------------------
void CClientDocArray::OnPrepareAuxData(CAbstractFormView* pView)
{
	for (int i = 0; i <= GetUpperBound(); i++)
		GetAt(i)->OnPrepareAuxData(pView);
}

//-----------------------------------------------------------------------------
void CClientDocArray::OnPrepareAuxData(CTileDialog* pTileDlg)
{
	for (int i = 0; i <= GetUpperBound(); i++)
		GetAt(i)->OnPrepareAuxData(pTileDlg);
}

//-----------------------------------------------------------------------------
void CClientDocArray::OnEnableControlsForFind(DBTObject* pDBTObj)
{
	for (int i = 0; i <= GetUpperBound(); i++)
		GetAt(i)->OnEnableControlsForFind(pDBTObj);
}

//-----------------------------------------------------------------------------
void CClientDocArray::OnDisableControlsForEdit(DBTObject* pDBTObj)
{
	for (int i = 0; i <= GetUpperBound(); i++)
		GetAt(i)->OnDisableControlsForEdit(pDBTObj);
}

//-----------------------------------------------------------------------------
void CClientDocArray::OnDisableControlsForAddNew(DBTObject* pDBTObj)
{
	for (int i = 0; i <= GetUpperBound(); i++)
		GetAt(i)->OnDisableControlsForAddNew(pDBTObj);
}

//-----------------------------------------------------------------------------
void CClientDocArray::OnDisableControlsAlways(DBTObject* pDBTObj)
{
	for (int i = 0; i <= GetUpperBound(); i++)
		GetAt(i)->OnDisableControlsAlways(pDBTObj);
}

//-----------------------------------------------------------------------------
void CClientDocArray::OnDisableControlsAlways (CTabDialog* pTabDialog)
{
	for (int i = 0; i <= GetUpperBound(); i++)
		GetAt(i)->OnDisableControlsAlways(pTabDialog);
}

//-----------------------------------------------------------------------------
void CClientDocArray::OnPrepareAuxColumns(DBTSlaveBuffered* pDBT, SqlRecord* pSql)
{
	for (int i = 0; i <= GetUpperBound(); i++)
		GetAt(i)->OnPrepareAuxColumns(pDBT, pSql);
}

//-----------------------------------------------------------------------------
void CClientDocArray::OnPrepareOldAuxColumns(DBTSlaveBuffered* pDBT, SqlRecord* pSql)
{
	for (int i = 0; i <= GetUpperBound(); i++)
		GetAt(i)->OnPrepareOldAuxColumns(pDBT, pSql);
}

//-----------------------------------------------------------------------------
BOOL CClientDocArray::CanCreateControl(UINT idc)
{
	if(this == NULL)
		return TRUE;
	for (int i = 0; i <= GetUpperBound(); i++)
		if (!GetAt(i)->CanCreateControl(idc))
			return FALSE;
	return TRUE;
}
//-----------------------------------------------------------------------------
void CClientDocArray::OnParsedControlCreated(CParsedCtrl* pCtrl)
{
	if (this == NULL)
		return;
	for (int i = 0; i <= GetUpperBound(); i++)
		GetAt(i)->OnParsedControlCreated(pCtrl);
}
//-----------------------------------------------------------------------------
void CClientDocArray::OnColumnInfoCreated(ColumnInfo* pColInfo)
{
	if (this == NULL)
		return;
	for (int i = 0; i <= GetUpperBound(); i++)
		GetAt(i)->OnColumnInfoCreated(pColInfo);
}

//-----------------------------------------------------------------------------
void CClientDocArray::OnPropertyCreated(CTBProperty * pProperty)
{
	if (this == NULL)
		return;
	for (int i = 0; i <= GetUpperBound(); i++)
		GetAt(i)->OnPropertyCreated(pProperty);
}

//-----------------------------------------------------------------------------
BOOL CClientDocArray::OnGetToolTipProperties(CBETooltipProperties* pTooltip)
{
	if (this == NULL)
		return FALSE;//false: non ho trovato il tooltip
	for (int i = 0; i <= GetUpperBound(); i++)
		if (GetAt(i)->OnGetToolTipProperties(pTooltip))
			return TRUE;
	return FALSE;
}

//-----------------------------------------------------------------------------
void CClientDocArray::EnableBodyEditButtons(CBodyEdit* pBodyEdit)
{
	if (this == NULL)
		return;
	for (int i = 0; i <= GetUpperBound(); i++)
		GetAt(i)->EnableBodyEditButtons(pBodyEdit);
}

//-----------------------------------------------------------------------------
void CClientDocArray::OnPinUnpin(CBaseTileDialog* pTileDialog)
{
	if (this == NULL)
		return;
	for (int i = 0; i <= GetUpperBound(); i++)
		GetAt(i)->OnPinUnpin(pTileDialog);
}
//-----------------------------------------------------------------------------
void CClientDocArray::OnPinUnpin(UINT nDialogId, bool isPinned)
{
	if (this == NULL)
		return;
	for (int i = 0; i <= GetUpperBound(); i++)
		GetAt(i)->OnPinUnpin(nDialogId, isPinned);
}
//-----------------------------------------------------------------------------
void CClientDocArray::OnUpdateTitle(CBaseTileDialog* pTileDialog)
{
	if (this == NULL)
		return;
	for (int i = 0; i <= GetUpperBound(); i++)
		GetAt(i)->OnUpdateTitle(pTileDialog);
}
//-----------------------------------------------------------------------------
void CClientDocArray::OnUpdateTitle(UINT nDialogId)
{
	if (this == NULL)
		return;
	for (int i = 0; i <= GetUpperBound(); i++)
		GetAt(i)->OnUpdateTitle(nDialogId);
}
//-----------------------------------------------------------------------------
void CClientDocArray::OnSetCurrentRow(DBTSlaveBuffered* pDBT)
{
	for (int i = 0; i <= GetUpperBound(); i++)
		GetAt(i)->OnSetCurrentRow(pDBT);
}

//-----------------------------------------------------------------------------
void CClientDocArray::OnPrepareRow(DBTSlaveBuffered* pDBT, int nRow, SqlRecord* pSql)
{
	for (int i = 0; i <= GetUpperBound(); i++)
		GetAt(i)->OnPrepareRow(pDBT, nRow, pSql);
}

//-----------------------------------------------------------------------------
BOOL CClientDocArray::OnBeforeAddRow(DBTSlaveBuffered* pDBT, int nRow)
{
	// se c'è qualcosa che non va esco e non proseguo con il ciclo
	for (int i = 0; i <= GetUpperBound(); i++)
		if (!GetAt(i)->OnBeforeAddRow(pDBT, nRow)) return FALSE;

	return TRUE;
}

//-----------------------------------------------------------------------------
void CClientDocArray::OnAfterAddRow(DBTSlaveBuffered* pDBT, int nRow, SqlRecord* pSql)
{
	for (int i = 0; i <= GetUpperBound(); i++)
		GetAt(i)->OnAfterAddRow(pDBT, nRow, pSql);
}

//-----------------------------------------------------------------------------
BOOL CClientDocArray::OnBeforeInsertRow(DBTSlaveBuffered* pDBT, int nRow)
{
	// se c'è qualcosa che non va esco e non proseguo con il ciclo
	for (int i = 0; i <= GetUpperBound(); i++)
		if (!GetAt(i)->OnBeforeInsertRow(pDBT, nRow)) return FALSE;

	return TRUE;
}

//-----------------------------------------------------------------------------
void CClientDocArray::OnAfterInsertRow(DBTSlaveBuffered* pDBT, int nRow, SqlRecord* pSql)
{
	for (int i = 0; i <= GetUpperBound(); i++)
		GetAt(i)->OnAfterInsertRow(pDBT, nRow, pSql);
}

//-----------------------------------------------------------------------------
BOOL CClientDocArray::OnBeforeDeleteRow(DBTSlaveBuffered* pDBT, int nRow)
{
	// se c'è qualcosa che non va esco e non proseguo con il ciclo
	for (int i = 0; i <= GetUpperBound(); i++)
		if (!GetAt(i)->OnBeforeDeleteRow(pDBT, nRow)) return FALSE;

	return TRUE;
}

//-----------------------------------------------------------------------------
void CClientDocArray::OnAfterDeleteRow(DBTSlaveBuffered* pDBT, int nRow)
{
	for (int i = 0; i <= GetUpperBound(); i++)
		GetAt(i)->OnAfterDeleteRow(pDBT, nRow);
}

	
//-----------------------------------------------------------------------------
DataObj*  CClientDocArray::OnCheckUserData(DBTSlaveBuffered* pDBT, int nRow)
{
	for (int i = 0; i <= GetUpperBound(); i++)
	{
		DataObj* pBadData = GetAt(i)->OnCheckUserData(pDBT, nRow);
		if (pBadData) return pBadData;
	}

	return NULL;
}

//-----------------------------------------------------------------------------
DataObj* CClientDocArray::OnCheckUserRecords(DBTSlaveBuffered* pDBT, int& nRow)
{
	for (int i = 0; i <= GetUpperBound(); i++)
	{
		DataObj* pBadData = GetAt(i)->OnCheckUserRecords(pDBT, nRow);
		if (pBadData) return pBadData;
	}

	return NULL;
}

//-----------------------------------------------------------------------------
void CClientDocArray::OnBodyEditRowView(CBodyEdit* pBodyEdit)
{
	for (int i = 0; i <= GetUpperBound(); i++)
		GetAt(i)->OnBodyEditRowView(pBodyEdit);
}

//-----------------------------------------------------------------------------
void CClientDocArray::OnRowFormViewDied(CRowFormView* pRowFormView)
{
	for (int i = 0; i <= GetUpperBound(); i++)
		GetAt(i)->OnRowFormViewDied(pRowFormView);
}

//-----------------------------------------------------------------------------
void CClientDocArray::OnModifyDBTDefineQuery(DBTObject* pDBTObj, SqlTable* pTable)
{
	for (int i = 0; i <= GetUpperBound(); i++)
		GetAt(i)->OnModifyDBTDefineQuery(pDBTObj, pTable);
}

//-----------------------------------------------------------------------------
void CClientDocArray::OnModifyDBTPrepareQuery(DBTObject* pDBTObj, SqlTable* pTable)
{	
	for (int i = 0; i <= GetUpperBound(); i++)
		GetAt(i)->OnModifyDBTPrepareQuery(pDBTObj, pTable);
}

//-----------------------------------------------------------------------------
void CClientDocArray::OnPrepareBrowser(SqlTable* pTable)
{	
	for (int i = 0; i <= GetUpperBound(); i++)
		GetAt(i)->OnPrepareBrowser(pTable);
}

//-----------------------------------------------------------------------------
void CClientDocArray::OnPrepareFindQuery(SqlTable* pTable)
{	
	for (int i = 0; i <= GetUpperBound(); i++)
		GetAt(i)->OnPrepareFindQuery(pTable);
}

//-----------------------------------------------------------------------------
void CClientDocArray::OnAfterCreateAndInitDBT(DBTObject* pDBTObj)
{	
	for (int i = 0; i <= GetUpperBound(); i++)
		GetAt(i)->OnAfterCreateAndInitDBT(pDBTObj);
} 

//-----------------------------------------------------------------------------
void CClientDocArray::OnDuringBatchExecute(SqlRecord* pCurrentRecord)
{	
	for (int i = 0; i <= GetUpperBound(); i++)
		GetAt(i)->OnDuringBatchExecute(pCurrentRecord);
}

//-----------------------------------------------------------------------------
void CClientDocArray::OnLoadAttachedDocument(CFinderDoc* pFinderDoc)
{
	for (int i = 0; i <= GetUpperBound(); i++)
		GetAt(i)->OnLoadAttachedDocument(pFinderDoc);
}

//-----------------------------------------------------------------------------
BOOL CClientDocArray::OnShowingPopupMenu(UINT nIDC, CMenu* pMenu)
{
	BOOL bOk = TRUE;
	for (int i = 0; i <= GetUpperBound(); i++)
		bOk = GetAt(i)->OnShowingPopupMenu(nIDC, pMenu) && bOk;

	return bOk;
}

//-----------------------------------------------------------------------------
BOOL CClientDocArray::OnRunReport (CWoormInfo* pWi)
{
	BOOL bOk = TRUE;
	for (int i = 0; i <= GetUpperBound(); i++)
		bOk = GetAt(i)->OnRunReport(pWi) && bOk;

	return bOk;
}

//-----------------------------------------------------------------------------
BOOL CClientDocArray::OnGetToolTipText(UINT nID, CString& strMessage)
{
	for (int i = 0; i <= GetUpperBound(); i++)
		if	(GetAt(i)->OnGetToolTipText(nID, strMessage))
			return TRUE;
	return FALSE;
}

//-----------------------------------------------------------------------------
void CClientDocArray:: OnModifyHKLDefineQuery
						(
							HotKeyLink* pHKL, 
							SqlTable*	pTable, 
							HotKeyLink::SelectionType nQuerySelection /*= DIRECT_ACCESS*/
						)
{
	for (int i = 0; i <= GetUpperBound(); i++)
		GetAt(i)->OnModifyHKLDefineQuery(pHKL, pTable, nQuerySelection);
}

//-----------------------------------------------------------------------------
void CClientDocArray:: OnModifyHKLPrepareQuery
						(
							HotKeyLink* pHKL, 
							SqlTable*	pTable, 
							DataObj*	pDataObj, 
							HotKeyLink::SelectionType nQuerySelection  /*= DIRECT_ACCESS*/
						)
{
	for (int i = 0; i <= GetUpperBound(); i++)
		GetAt(i)->OnModifyHKLPrepareQuery(pHKL, pTable, pDataObj, nQuerySelection);
}

//-----------------------------------------------------------------------------
int CClientDocArray:: OnModifyHKLSearchComboQueryData
						(
							HotKeyLink* pHKL, 
							const int& nMaxItems, DataObjArray& pKeyData, CStringArray& arDescriptions
						)
{
	int nret = 0;
	for (int i = 0; i <= GetUpperBound(); i++)
		nret = max(nret, GetAt(i)->OnModifyHKLSearchComboQueryData(pHKL, nMaxItems, pKeyData, arDescriptions));
	return nret;
}

//-----------------------------------------------------------------------------
BOOL CClientDocArray::OnGetCustomColor (const CBodyEdit* pBody, CBodyEditRowSelected* pRow)
{
    BOOL bOk = FALSE;
    for (int i = 0; i <= GetUpperBound(); i++)
		bOk = GetAt(i)->OnGetCustomColor (pBody, pRow) || bOk;
    return bOk;
}

//----------------------------------------------------------------------------------
BOOL CClientDocArray::OnBeforeLoadDBT()
{
	for (int i = 0; i <= GetUpperBound(); i++)
		if (!GetAt(i)->OnBeforeLoadDBT())
			return FALSE;

	return TRUE;
}

//----------------------------------------------------------------------------------
BOOL CClientDocArray::OnBeforeUndoExtraction()
{
	for (int i = 0; i <= GetUpperBound(); i++)
		if (!GetAt(i)->OnBeforeUndoExtraction())
			return FALSE;

	return TRUE;
}

//-------------------------------------------------------------------------------
BOOL CClientDocArray::OnLoadDBT()
{
	for (int i = 0; i <= GetUpperBound(); i++)
		if (!GetAt(i)->OnLoadDBT())
			return FALSE;

	return TRUE;
}

//-------------------------------------------------------------------------------
BOOL CClientDocArray::OnAfterLoadDBT()
{
	for (int i = 0; i <= GetUpperBound(); i++)
		if (!GetAt(i)->OnAfterLoadDBT())
			return FALSE;

	return TRUE;
}

//-----------------------------------------------------------------------------
BOOL CClientDocArray::OnDblClick (const CBodyEdit* pBody, UINT nFlags, CBodyEditRowSelected* pRow)
{
	for (int i = 0; i <= GetUpperBound(); i++)
		if (GetAt(i)->OnDblClick (pBody, nFlags, pRow))
			return TRUE;
	return FALSE;
}

//-----------------------------------------------------------------------------
void CClientDocArray::OnBESelCell(CBodyEdit* pBody, SqlRecord* pCurRec, ColumnInfo* pCol)
{
	for (int i = 0; i <= GetUpperBound(); i++)
		GetAt(i)->OnBESelCell(pBody, pCurRec, pCol);
}

void CClientDocArray::OnBEShowCtrl(CBodyEdit* pBody, SqlRecord* pCurRec, ColumnInfo* pCol)
{
	for (int i = 0; i <= GetUpperBound(); i++)
		GetAt(i)->OnBEShowCtrl(pBody, pCurRec, pCol);
}

void CClientDocArray::OnBEHideCtrl(CBodyEdit* pBody, SqlRecord* pCurRec, ColumnInfo* pCol)
{
	for (int i = 0; i <= GetUpperBound(); i++)
		GetAt(i)->OnBEHideCtrl(pBody, pCurRec, pCol);
}

void CClientDocArray::OnBEEnableButton(CBodyEdit* pBody, CBEButton* pBtn)
{
	for (int i = 0; i <= GetUpperBound(); i++)
		GetAt(i)->OnBEEnableButton(pBody, pBtn);
}

//-----------------------------------------------------------------------------
BOOL CClientDocArray::OnBEBeginMultipleSel(CBodyEdit* pBody)
{
	BOOL b = FALSE;
	for (int i = 0; i <= GetUpperBound(); i++)
		b = GetAt(i)->OnBEBeginMultipleSel(pBody) || b;
	return b;
}

BOOL CClientDocArray::OnBEEndMultipleSel(CBodyEdit* pBody)
{
	BOOL b = FALSE;
	for (int i = 0; i <= GetUpperBound(); i++)
		b = GetAt(i)->OnBEEndMultipleSel(pBody) || b;
	return b;
}

BOOL CClientDocArray::OnBECustomizeSelections(CBodyEdit* pBody, SelArray& sel)
{
	BOOL b = FALSE;
	for (int i = 0; i <= GetUpperBound(); i++)
		b = GetAt(i)->OnBECustomizeSelections(pBody, sel) || b;
	return b;
}

//-----------------------------------------------------------------------------
BOOL CClientDocArray::OnBECandDoDeleteRow(CBodyEdit* pBodyEdit)
{
	BOOL b = TRUE;

	for (int i = 0; i <= GetUpperBound() && b; i++)
		b = GetAt(i)->OnBECandDoDeleteRow(pBodyEdit);

	return b;
}

//-----------------------------------------------------------------------------
BOOL CClientDocArray::OnEnableTabSelChanging(UINT nTabber, UINT nFromIDD, UINT nToIDD)
{
	BOOL bOk = TRUE;
	for (int i = 0; i <= GetUpperBound(); i++)
		bOk = GetAt(i)->OnEnableTabSelChanging (nTabber, nFromIDD, nToIDD) && bOk;
		
	return bOk;
}

//-------------------------------------------------------------------------------------------------
BOOL CClientDocArray::OnAfterOnAttachData()
{
	BOOL bOK = TRUE;
	for (int i = 0; i <= GetUpperBound() && bOK; i++)
	{
		bOK = bOK && GetAt(i)->OnAfterOnAttachData();
	}

	return bOK;
}

//-----------------------------------------------------------------------------
void CClientDocArray::OnTabSelChanged (UINT nTabber, UINT nTabIDD)
{
	for (int i = 0; i <= GetUpperBound(); i++)
		GetAt(i)->OnTabSelChanged (nTabber, nTabIDD);
}

//-----------------------------------------------------------------------------
BOOL CClientDocArray::OnToolbarDropDown(UINT nID, CMenu& menu)
{
	BOOL bOk = FALSE;
	for (int i = 0; i <= GetUpperBound(); i++)
		bOk = GetAt(i)->OnToolbarDropDown (nID, menu) || bOk;
		
	return bOk;
}

//-----------------------------------------------------------------------------
BOOL CClientDocArray::OnHKLIsValid (HotKeyLink* pHotKeyLink)
{
	BOOL bOk = TRUE;
	for (int i = 0; i <= GetUpperBound(); i++)
		bOk = GetAt(i)->OnHKLIsValid (pHotKeyLink) && bOk;
		
	return bOk;
}

//-----------------------------------------------------------------------------
BOOL CClientDocArray::OnValidateRadarSelection(SqlRecord* pRec, HotKeyLink* pHotKeyLink)
{
	BOOL bOk = TRUE;
	for (int i = 0; i <= GetUpperBound(); i++)
		bOk = GetAt(i)->OnValidateRadarSelection (pRec, pHotKeyLink) && bOk;
		
	return bOk;
}

//-----------------------------------------------------------------------------
BOOL CClientDocArray::OnValidateRadarSelection(SqlRecord* pRec, CTBNamespace nsHotLinkNamespace, HotKeyLink* pHotKeyLink)
{
	BOOL bOk = TRUE;
	for (int i = 0; i <= GetUpperBound(); i++)
		bOk = GetAt(i)->OnValidateRadarSelection (pRec, nsHotLinkNamespace, pHotKeyLink) && bOk;
		
	return bOk;
}
//-----------------------------------------------------------------------------
void CClientDocArray::OnPrepareForFind(HotKeyLinkObj* pHKL, SqlRecord* pRec)
{
	for (int i = 0; i < GetCount(); i++)
		GetAt(i)->OnPrepareForFind(pHKL, pRec);
}
//-----------------------------------------------------------------------------
void CClientDocArray::OnPrepareAuxData(HotKeyLinkObj* pHKL)
{
	for (int i = 0; i < GetCount(); i++)
		GetAt(i)->OnPrepareAuxData(pHKL);
}
//-----------------------------------------------------------------------------
BOOL CClientDocArray::OnShowStatusBarMsg(CString& sMsg)
{
	BOOL bOk = FALSE;
	for (int i = 0; i <= GetUpperBound(); i++)
		bOk = GetAt(i)->OnShowStatusBarMsg (sMsg) || bOk;
		
	return bOk;
}

//-----------------------------------------------------------------------------
void CClientDocArray::OnBeforeSave()
{
	for (int i = 0; i <= GetUpperBound(); i++)
		GetAt(i)->OnBeforeSave ();
}

//-----------------------------------------------------------------------------
void CClientDocArray::OnAfterSave()
{
	for (int i = 0; i <= GetUpperBound(); i++)
		GetAt(i)->OnAfterSave ();
}


//-----------------------------------------------------------------------------
void CClientDocArray::OnBeforeDelete()
{
	for (int i = 0; i <= GetUpperBound(); i++)
		GetAt(i)->OnBeforeDelete ();
}

//-----------------------------------------------------------------------------
void CClientDocArray::OnAfterDelete()
{
	for (int i = 0; i <= GetUpperBound(); i++)
		GetAt(i)->OnAfterDelete ();
}

//-----------------------------------------------------------------------------
CParsedCtrl* CClientDocArray::OnCreateParsedCtrl (UINT nIDC, CRuntimeClass* pParsedCtrlClass)
{
	CParsedCtrl* pCtrl = NULL;
	for (int i = 0; i <= GetUpperBound(); i++)
	{
		pCtrl = GetAt(i)->OnCreateParsedCtrl (nIDC, pParsedCtrlClass);
		if (pCtrl) break;
	}
	return pCtrl;
}

//-----------------------------------------------------------------------------
CRuntimeClass* CClientDocArray::OnModifySqlRecordClass (DBTObject* pDbt, const CString& sDBTName, CRuntimeClass* pSqlRecordClass) 
{ 
	CRuntimeClass* pClass = NULL;
	for (int i = 0; i <= GetUpperBound(); i++)
	{
		pClass = GetAt(i)->OnModifySqlRecordClass (pDbt, sDBTName, pSqlRecordClass);
		if (pClass && pClass != pSqlRecordClass) 
			return pClass;
	}
	return pSqlRecordClass;
}

//for CWizardFormDoc attach
//-----------------------------------------------------------------------------
LRESULT CClientDocArray::OnWizardNext(UINT nDlgIDD)
{
	UINT nextIDD = WIZARD_DEFAULT_TAB;
	
	for (int i = 0; i <= GetUpperBound(); i++)
	{
		nextIDD = GetAt(i)->OnWizardNext(nDlgIDD);
		if (nextIDD != WIZARD_DEFAULT_TAB) break;
	}
	return nextIDD;
}

//-----------------------------------------------------------------------------
LRESULT CClientDocArray::OnWizardBack(UINT nDlgIDD)
{
	UINT prevIDD = WIZARD_DEFAULT_TAB;	
	for (int i = 0; i <= GetUpperBound(); i++)
	{
		prevIDD = GetAt(i)->OnWizardBack(nDlgIDD);
		if (prevIDD != WIZARD_DEFAULT_TAB) break;
	}
	return prevIDD;
}

//-----------------------------------------------------------------------------
LRESULT CClientDocArray::OnWizardFinish (UINT nDlgIDD)
{
	UINT prevIDD = WIZARD_DEFAULT_TAB;
	for (int i = 0; i <= GetUpperBound(); i++)
	{
		prevIDD = GetAt(i)->OnWizardFinish(nDlgIDD);
		if (prevIDD != WIZARD_DEFAULT_TAB) break;
	}
	return prevIDD;
}

//-----------------------------------------------------------------------------
LRESULT CClientDocArray::OnBeforeWizardFinish (UINT nDlgIDD)
{
	UINT prevIDD = WIZARD_DEFAULT_TAB;
	for (int i = 0; i <= GetUpperBound(); i++)
	{
		prevIDD = GetAt(i)->OnBeforeWizardFinish(nDlgIDD);
		if (prevIDD != WIZARD_DEFAULT_TAB) break;
	}
	return prevIDD;
}

//-----------------------------------------------------------------------------
LRESULT CClientDocArray::OnWizardCancel (UINT nDlgIDD)
{
	UINT prevIDD = WIZARD_DEFAULT_TAB;
	for (int i = 0; i <= GetUpperBound(); i++)
	{
		prevIDD = GetAt(i)->OnWizardCancel(nDlgIDD);
		if (prevIDD != WIZARD_DEFAULT_TAB) break;
	}
	return prevIDD;
}

//-----------------------------------------------------------------------------
void CClientDocArray::OnUpdateWizardButtons(UINT nDlgIDD)
{
	for (int i = 0; i <= GetUpperBound(); i++)
		GetAt(i)->OnUpdateWizardButtons(nDlgIDD);	
}
//-----------------------------------------------------------------------------
LRESULT CClientDocArray::OnGetBitmapID (UINT nDlgIDD)
{
	UINT nBitmap = 0;
	for (int i = 0; i <= GetUpperBound(); i++)
	{
		nBitmap = GetAt(i)->OnGetBitmapID(nDlgIDD);
		if (nBitmap != 0) break;
	}
	return nBitmap;
}

//-----------------------------------------------------------------------------
void CClientDocArray::OnWizardActivate(UINT nDlgIDD)
{
	for (int i = 0; i <= GetUpperBound(); i++)
		GetAt(i)->OnWizardActivate(nDlgIDD);	
}

//-----------------------------------------------------------------------------
void CClientDocArray::OnWizardDeactivate(UINT nDlgIDD)
{
	for (int i = 0; i <= GetUpperBound(); i++)
		GetAt(i)->OnWizardDeactivate(nDlgIDD);	
}

//-----------------------------------------------------------------------------
BOOL CClientDocArray::OnPasteDBTRows			(CTBEDataCoDecPastedRecord& pr)
{
	BOOL bOk = TRUE;
	for (int i = 0; i <= GetUpperBound(); i++)
		bOk = GetAt(i)->OnPasteDBTRows(pr) && bOk;	
	return bOk;
}

BOOL CClientDocArray::OnValidatePasteDBTRows (RecordArray& arRows,	CTBEDataCoDecRecordToValidate& rv)
{
	BOOL bOk = TRUE;
	for (int i = 0; i <= GetUpperBound(); i++)
		bOk = GetAt(i)->OnValidatePasteDBTRows(arRows, rv) && bOk;	
	return bOk;
}

BOOL CClientDocArray::OnValidatePasteDBTRows (SqlRecord* pRec,		CTBEDataCoDecRecordToValidate& rv)
{
	BOOL bOk = TRUE;
	for (int i = 0; i <= GetUpperBound(); i++)
		bOk = GetAt(i)->OnValidatePasteDBTRows(pRec, rv) && bOk;	
	return bOk;
}

//-----------------------------------------------------------------------------
void CClientDocArray::OnActivate (CAbstractFormFrame* pFrame, UINT nState, CWnd* pWndOther, BOOL bMinimized)
{
	for (int i = 0; i <= GetUpperBound(); i++)
		GetAt(i)->OnActivate (pFrame, nState, pWndOther, bMinimized);	
}

//-----------------------------------------------------------------------------
void CClientDocArray::OnBeforeCallLink(CParsedCtrl* pCtrl)
{
	for (int i = 0; i <= GetUpperBound(); i++)
		GetAt(i)->OnBeforeCallLink(pCtrl);	
}

//-----------------------------------------------------------------------------
void CClientDocArray::OnAfterSetFormMode(CBaseDocument::FormMode oldFormMode)
{
	for (int i = 0; i <= GetUpperBound(); i++)
		GetAt(i)->OnAfterSetFormMode(oldFormMode);	
}

//-----------------------------------------------------------------------------
void CClientDocArray::OnPreparePrimaryKey (DBTObject* pDBT)
{
	for (int i = 0; i <= GetUpperBound(); i++)
		GetAt(i)->OnPreparePrimaryKey(pDBT);	
}

//-----------------------------------------------------------------------------
void CClientDocArray::OnPreparePrimaryKey (DBTSlaveBuffered* pDBT, int nRow, SqlRecord* pRecord)
{
	for (int i = 0; i <= GetUpperBound(); i++)
		GetAt(i)->OnPreparePrimaryKey(pDBT, nRow, pRecord);	
}

//-----------------------------------------------------------------------------
CManagedDocComponentObj* CClientDocArray::GetComponent (CString& sParentNamespace, CString& sName)
{
	CManagedDocComponentObj* pObj = NULL;
	for (int i = 0; i <= GetUpperBound(); i++)
	{
		pObj = GetAt(i)->GetComponent(sParentNamespace,  sName);
		if (pObj)
			return pObj;
	}
	
	return NULL;
}

//-----------------------------------------------------------------------------
void CClientDocArray::GetComponents (CManagedDocComponentObj* pRequest, Array& returnedComponents)
{
	for (int i = 0; i <= GetUpperBound(); i++)
		GetAt(i)->GetComponents(pRequest,  returnedComponents);
}

//-----------------------------------------------------------------------------
void CClientDocArray::OnBuildingSecurityTree (CTBTreeCtrl* pTree, Array* pInfoTreeItems)
{
	for (int i = 0; i <= GetUpperBound(); i++)
		GetAt(i)->OnBuildingSecurityTree(pTree, pInfoTreeItems);
}

//-----------------------------------------------------------------------------
void CClientDocArray::OnAddFormsOnDockPane(CTaskBuilderDockPane* pPane)
{
	for (int i = 0; i <= GetUpperBound(); i++)
		GetAt(i)->OnAddFormsOnDockPane(pPane);
}

//-----------------------------------------------------------------------------
void CClientDocArray::PopulateMessagesIDsArrayForPushToClients(CArray<int>& arIDs)
{
	for (int i = 0; i <= GetUpperBound(); i++)
		GetAt(i)->PopulateMessagesIDsArrayForPushToClients(arIDs);
}

//-----------------------------------------------------------------------------
void CClientDocArray::OnDMSEvent(DMSEventTypeEnum eventType, int eventKey)
{
	for (int i = 0; i <= GetUpperBound(); i++)
		GetAt(i)->OnDMSEvent(eventType, eventKey);
}

//-----------------------------------------------------------------------------
LOCK_ITERATOR(OnLockDocumentForNew)
LOCK_ITERATOR(OnLockDocumentForEdit)
LOCK_ITERATOR(OnLockDocumentForDelete)

BOOL_ITERATOR(OnBeforeOkDelete)
BOOL_ITERATOR(OnOkDelete)
BOOL_ITERATOR(OnOkEdit)
BOOL_ITERATOR(OnOkNewRecord)
BOOL_ITERATOR(OnBeforeOkTransaction)
BOOL_ITERATOR(OnOkTransaction)
BOOL_ITERATOR(SaveModified)
BOOL_ITERATOR(OnBeforeNewTransaction)
BOOL_ITERATOR(OnBeforeEditTransaction)
BOOL_ITERATOR(OnBeforeDeleteTransaction)
BOOL_ITERATOR(OnNewTransaction)
BOOL_ITERATOR(OnEditTransaction)
BOOL_ITERATOR(OnDeleteTransaction)
BOOL_ITERATOR(OnExtraNewTransaction)
BOOL_ITERATOR(OnExtraEditTransaction)
BOOL_ITERATOR(OnExtraDeleteTransaction)

BOOL_ITERATOR(CanDoDeleteRecord);
BOOL_ITERATOR(CanDoEditRecord);
BOOL_ITERATOR(CanDoNewRecord);
BOOL_ITERATOR(CanDoFirstRecord);
BOOL_ITERATOR(CanDoPrevRecord);
BOOL_ITERATOR(CanDoNextRecord);
BOOL_ITERATOR(CanDoLastRecord);
BOOL_ITERATOR(CanDoQuery);
BOOL_ITERATOR(CanDoRadar);
BOOL_ITERATOR(CanDoExecQuery);
BOOL_ITERATOR(CanDoEditQuery);

BOOL_ITERATOR(OnBeforeDeleteRecord)
BOOL_ITERATOR(OnBeforeEditRecord)
BOOL_ITERATOR(OnBeforeNewRecord)

VOID_ITERATOR(OnBeforeBrowseRecord)
VOID_ITERATOR(OnAfterBrowseRecord)
VOID_ITERATOR(OnGoInBrowseMode)
BOOL_ITERATOR(OnBeforeBatchExecute)
VOID_ITERATOR(OnAfterBatchExecute)
VOID_ITERATOR(OnBeforeCloseDocument)
VOID_ITERATOR(OnCloseServerDocument)
VOID_ITERATOR(OnHotLinkRun)
VOID_ITERATOR(OnHotLinkStop)

VOID_ITERATOR(OnDisableControlsForBatch)
VOID_ITERATOR(OnDisableControlsForAddNew)
VOID_ITERATOR(OnDisableControlsForEdit)
VOID_ITERATOR(OnEnableControlsForFind)
VOID_ITERATOR(OnDisableControlsAlways)
VOID_ITERATOR(OnSaveCurrentRecord)

BOOL_ITERATOR(OnAttachData)
BOOL_ITERATOR(OnPrepareAuxData)

BOOL_ITERATOR(OnInitAuxData)
BOOL_ITERATOR(OnExistTables)
BOOL_ITERATOR(OnInitDocument)
BOOL_ITERATOR(OnBeforeEscape)

VOID_ITERATOR(OnDocumentCreated)
VOID_ITERATOR(OnFrameCreated)

VOID_ITERATOR(OnBeforeXMLImport)
VOID_ITERATOR(OnAfterXMLImport)
VOID_ITERATOR(OnBeforeXMLExport)
VOID_ITERATOR(OnAfterXMLExport)

//============================================================================================
//=============================================================================
//////////////////////////////////////////////////////////////////////////////
//					CDFilterManager definition
//////////////////////////////////////////////////////////////////////////////
//
//=============================================================================
IMPLEMENT_DYNCREATE(CDFilterManager, CClientDoc)

//=============================================================================
CDFilterManager::CDFilterManager()
{
	m_pFilterXMLVariableList = new Array();
};

//=============================================================================
CDFilterManager::~CDFilterManager()
{
	delete m_pFilterXMLVariableList;
}

//=============================================================================
CXMLVariableArray* CDFilterManager::GetVariableArray(const CString& filterName)
{
	for (int i = 0; i< m_pFilterXMLVariableList->GetSize(); i++)
	{
		CXMLVariableArray* pXMLVariableArray = ((CXMLVariableArray*)m_pFilterXMLVariableList->GetAt(i));
		if (pXMLVariableArray && (pXMLVariableArray->m_strName.CompareNoCase(filterName) == 0))
			return pXMLVariableArray;
	}

	return NULL;
}

//=============================================================================
void CDFilterManager::AddVariableArray(const CString& filterName, CXMLVariableArray* pXMLVariableArray)
{
	pXMLVariableArray->m_strName = filterName;
	m_pFilterXMLVariableList->Add(pXMLVariableArray);
}

//=============================================================================
void CDFilterManager::ParseVariable(const CString& filterName, const CString& strXMLFilter)
{
	CXMLVariableArray* pVariableArray = GetVariableArray(filterName);
	if (pVariableArray)
		pVariableArray->ParseFromXMLString(strXMLFilter);
}

//============================================================================================
