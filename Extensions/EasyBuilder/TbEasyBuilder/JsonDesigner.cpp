#include "stdafx.h"

#include <TbGeneric\JsonFormEngine.h>
#include <TbGenlib\TBSplitterWnd.h>

#include <TBApplicationWrapper\resource.hjson>

#include "JsonDesigner.h"
#include "CDEasyBuilder.h"
#include "TBEasyBuilder.h"
#include "CDEasyBuilder.hjson"
#include "JsonConnector.h"

using namespace System;
using namespace System::Threading;
using namespace System::Globalization;
using namespace Microarea::EasyBuilder::UI;
using namespace Microarea::TaskBuilderNet::UI::WinControls::Generic;
using namespace System::Windows::Forms;

//////////////////////////////////////////////////////////////////////////////////	
//								CEasyStudioDesignerDialog class implementation
//////////////////////////////////////////////////////////////////////////////////	

IMPLEMENT_DYNAMIC(CEasyStudioDesignerDialog, CAbstractFormView)

BEGIN_MESSAGE_MAP(CEasyStudioDesignerDialog, CAbstractFormView)
	ON_WM_SHOWWINDOW()
END_MESSAGE_MAP()
//--------------------------------------------------------------------------------
CEasyStudioDesignerDialog::CEasyStudioDesignerDialog(const CString& sJsonFile, CJsonContextObj* pContext)
	:
	CAbstractFormView(_T(""), 0),
	m_strJsonFile(sJsonFile)
{
	m_DummyParent.SetRuntimeState(CWndObjDescription::STATIC);
	m_pJsonContext = pContext;
	Init();
}
//--------------------------------------------------------------------------------
CEasyStudioDesignerDialog::CEasyStudioDesignerDialog(const CString& sJsonFile)
	:
	CAbstractFormView(_T(""), 0),
	m_strJsonFile(sJsonFile)
{
	m_DummyParent.SetRuntimeState(CWndObjDescription::STATIC);
	CJsonResource res;
	res.PopulateFromFile(m_strJsonFile);
	m_pJsonContext = CJsonFormEngineObj::GetInstance()->CreateContext(res, false);
	Init();
}

//--------------------------------------------------------------------------------
void CEasyStudioDesignerDialog::Init()
{
	//m_bAutoDestroy = true;
	ASSERT(m_pJsonContext);
	if (m_pJsonContext && m_pJsonContext->m_pDescription)
	{
		//così anche la root ha un parent, serve per l'aggiornamento
		m_DummyParent.m_Children.Add(m_pJsonContext->m_pDescription);
		m_pJsonContext->m_pDescription->SetParent(&m_DummyParent);

		//CTBNamespace aNS(m_pJsonContext->m_pDescription->m_strContext);
		m_arFiles.Add(m_strJsonFile);
		//if (aNS.GetType() == CTBNamespace::DOCUMENT)
		//{
		//	Split(NULL, m_arFiles, m_pJsonContext->m_pDescription, m_pJsonContext->m_pDescription->m_strContext);
		//}
	}
}


//--------------------------------------------------------------------------------
BOOL CEasyStudioDesignerDialog::Create(CWnd* pParent, CBaseDocument* pDoc)
{
	CWndObjDescription* pDescri = m_pJsonContext->m_pDescription;
	if (!pDescri)
		return FALSE;
	m_nID = AfxGetTBResourcesMap()->GetTbResourceID(m_pJsonContext->m_JsonResource.m_strName, TbResources, 1, m_pJsonContext->m_JsonResource.m_strContext);
	CString sName = CJsonFormEngineObj::GetObjectName(pDescri);
	SetFormName(sName);
	//AttachDocument(pDoc);
	//la creo sempre child, altrimenti non posso visualizzarla all'interno dell'editor
	bool bChild = pDescri->m_bChild;
	pDescri->m_bChild = true;
	CRect oldRect = pDescri->GetRect();
	pDescri->SetRect(CRect(0, 0, pDescri->m_Width, pDescri->m_Height), FALSE);

	// views are always created with a border!
	CCreateContext ctx;
	ctx.m_pCurrentDoc = pDoc;
	if (!JsonCreate(0, CRect(0, 0, 0, 0), pParent, m_nID, &ctx, FALSE))
		return FALSE;
	CBaseFormView::OnInitialUpdate();
	CalculateOSLInfo();
	m_pJsonContext->BuildDataControlLinks();

	//poi però ripristino il valore
	pDescri->m_bChild = bChild;
	pDescri->SetRect(oldRect, FALSE);

	return TRUE;
}
//--------------------------------------------------------------------------------
CEasyStudioDesignerDialog::~CEasyStudioDesignerDialog()
{
	if (m_DummyParent.m_Children.GetCount())
	{
		m_DummyParent.m_Children[0]->SetParent(NULL);
		m_DummyParent.m_Children.RemoveAt(0);
	}
}
//--------------------------------------------------------------------------
void CEasyStudioDesignerDialog::OnShowWindow(BOOL bShow, UINT nStatus)
{
	//override di quello di parsed dialog
	//non applico stili bcg perché mi fanno saltare gli stili dei bottoni e poi li salvo sballati in json!
	/*if (m_pOwner && bShow && !nStatus && !IsVisualManagerStyle())
	{
		ApplyTBVisualManager();
		if (!GetJsonContext())
			SetDefaultFont();
	}*/
}
//--------------------------------------------------------------------------------
void CEasyStudioDesignerDialog::AppendDefine(CString& sBuffer, CWndObjDescription* pDesc)
{
	CString sJsonId = pDesc->GetJsonID();
	if (sJsonId.IsEmpty())
		ASSERT(false);
	else if (!AfxGetTBResourcesMap()->IsFixedResource(sJsonId) && pDesc->GetParent() != &m_DummyParent)
	{
		sBuffer.Append(_T("#define "));
		sBuffer.Append(sJsonId);
		sBuffer.Append(_T("\t"));
		switch (pDesc->GetResourceType())
		{
		case TbCommands:
			sBuffer.Append(_T("GET_ID("));
			sBuffer.Append(sJsonId);
			sBuffer.Append(_T(")\r\n"));
			break;
		default:
			sBuffer.Append(_T("GET_IDC("));
			sBuffer.Append(sJsonId);
			sBuffer.Append(_T(")\r\n"));
			break;
		}

	}
	if (pDesc->m_pAccelerator)
	{
		for (int i = 0; i < pDesc->m_pAccelerator->m_arItems.GetCount(); i++)
		{
			CAcceleratorItemDescription* pItem = pDesc->m_pAccelerator->m_arItems.GetAt(i);
			sBuffer.Append(_T("#define "));
			sBuffer.Append(pItem->m_sId);
			sBuffer.Append(_T("\t"));
			sBuffer.Append(_T("GET_ID("));
			sBuffer.Append(pItem->m_sId);
			sBuffer.Append(_T(")\r\n"));
		}
	}
	for (int i = 0; i < pDesc->m_Children.GetCount(); i++)
	{
		AppendDefine(sBuffer, pDesc->m_Children[i]);
	}
}
//--------------------------------------------------------------------------------
bool CEasyStudioDesignerDialog::UpdateDescription(HWND hwnd)
{
	CWndObjDescription* pDesc = CWndObjDescription::GetFrom(hwnd);
	if (!pDesc || !pDesc->GetParent())
		return FALSE;

	//aggiorno tutti gli attributi di finestra
	CWndObjDescription* pChild = (hwnd == m_hWnd)//la root e' sempre una view, non devo farmi gestire da lei l'aggiornamento della descrizione
		? NULL
		: (CWndObjDescription*)::SendMessage(hwnd, UM_GET_CONTROL_DESCRIPTION, (WPARAM)&(pDesc->GetParent()->m_Children), (LPARAM)(LPCTSTR)pDesc->GetJsonID());
	if (!pChild)
		pDesc->UpdateAttributes(CWnd::FromHandle(hwnd));
	ASSERT(!pChild || pChild == pDesc);

	//la UpdateAttributes mi cambia lo stato a UPDATED, ma a me serve PARSED, perché in questo
	//stato tutti gli attributi diversi dal default vengono serializzati, 
	//altrimenti mi verrebbero serializzati solo quelli cambiati
	pDesc->SetParsed(true);
	return TRUE;
}
//--------------------------------------------------------------------------------
bool CEasyStudioDesignerDialog::AddDescription(HWND hwnd, HWND hwndParent)
{
	CWndObjDescription* pDesc = CWndObjDescription::GetFrom(hwndParent);
	if (!pDesc)
		return FALSE;
	//aggiorno tutti gli attributi di finestra
	CWndObjDescription* pChild = (CWndObjDescription*)::SendMessage(hwnd, UM_GET_CONTROL_DESCRIPTION, (WPARAM)&(pDesc->m_Children), NULL);
	if (!pChild)
		pChild = pDesc->m_Children.AddWindow(CWnd::FromHandle(hwnd));

	if (!pChild)
		return FALSE;

	pChild->AttachTo(hwnd);
	return TRUE;
}
//--------------------------------------------------------------------------------
bool CEasyStudioDesignerDialog::DeleteDescription(HWND hwnd)
{
	CWndObjDescription* pDesc = CWndObjDescription::GetFrom(hwnd);
	if (!pDesc)
		return FALSE;
	CWndObjDescription* pParent = pDesc->GetParent();
	if (!pParent)
		return FALSE;
	pParent->m_Children.RemoveItem(pDesc);
	return TRUE;
}
//--------------------------------------------------------------------------------
bool CEasyStudioDesignerDialog::UpdateTabOrder(HWND hwnd)
{
	CWndObjDescription* pDesc = CWndObjDescription::GetFrom(hwnd);
	if (!pDesc)
		return FALSE;
	CWndObjDescription* pParent = pDesc->GetParent();
	if (!pParent)
		return FALSE;
	pParent->m_Children.SortByTabOrder();
	return TRUE;
}

//--------------------------------------------------------------------------------
void CEasyStudioDesignerDialog::ShowError(LPCTSTR szError)
{
	CMessages messages;
	messages.Add(szError);
	messages.Show();
}
//--------------------------------------------------------------------------------
CString CEasyStudioDesignerDialog::GetCode()
{
	CWndObjDescription* pDesc = m_pJsonContext->m_pDescription;
	if (!pDesc)
		return _T("");
	//la UpdateAttributes mi cambia lo stato a UPDATED, ma a me serve PARSED, perché in questo
	//stato tutti gli attributi diversi dal default vengono serializzati, 
	//altrimenti mi verrebbero serializzati solo quelli cambiati
	pDesc->SetParsed(true);
	CJsonSerializer ser;
	pDesc->SerializeJson(ser);
	return ser.GetJson();
}

//--------------------------------------------------------------------------------
bool CEasyStudioDesignerDialog::SaveFile()
{
	CWndObjDescription* pDesc = m_pJsonContext->m_pDescription->DeepClone();
	CWndObjDescriptionContainer container;
	container.Add(pDesc);
	CStringArray arFilesToSave;

	//posso avere un contesto rappresentato da un namespace di documento, oppure un generico token (file provenienti da RC)
	//nel primo caso, salvo nel percorso individuato dal namespace, ed eventualmente splitto eventuali client doc json, nel secondo nel percorso originario
	//CTBNamespace aNS(pDesc->m_strContext);
	//CString sPath = AfxGetPathFinder()->GetJsonFormPath(aNS);
	//if (sPath.IsEmpty())
	//	sPath = m_strJsonFile;
	//else
	//	sPath += SLASH_CHAR + pDesc->GetJsonID() + _T(".tbjson");

	CString sPath = m_strJsonFile;
	arFilesToSave.Add(sPath);

	CServerFormDescription* pDescri = NULL;
	//	if (aNS.GetType() == CTBNamespace::DOCUMENT)
	//	{
	//		Split(&container, arFilesToSave, pDesc, pDesc->m_strContext);
	//		pDescri = AfxGetApplicationContext()->GetObject<CServerFormDescriArray>(&CApplicationContext::GetClientFormsTable)->Get(pDesc->GetJsonID(), TRUE);
	//	}

		//calcolo la stringa che individua il path del file json
	CJsonResource res;
	res.PopulateFromFile(sPath);

	CStringArray arFilesToDelete;
	//controllo se i file che avevo in partenza ci sono anche fra quelli che sto per salvare
	for (int i = 0; i < m_arFiles.GetCount(); i++)
	{
		CString sFile = m_arFiles[i];
		bool found = false;
		for (int j = 0; j < arFilesToSave.GetCount(); j++)
		{
			if (arFilesToSave[j].CompareNoCase(sFile) == 0)
			{
				found = true;
				break;
			}
		}
		//se uno non c'è, significa che è stato rimosso dal salvataggio, perché accorpato ad un altro,
		//quindi devo toglierlo dal file xml dei client forms e dal file system
		//lo tolgo però in fondo, dopo che ho salvato con successo i nuovi
		if (!found)
		{
			arFilesToDelete.Add(sFile);
		}
	}
	CString HjsonExtension = _T(".hjson");
	//ho dei file da rimuovere o dei file aggiunti (cambio di Context, aggiunta di elementi, rimozone di elementi)
	//avviso con la lista dei file che salverò o modificherò
	if (arFilesToDelete.GetCount() || arFilesToSave.GetCount() != m_arFiles.GetCount())
	{
		CMessages messages;
		messages.Add(_TB("The following files will be created or updated:"), CMessages::MSG_HINT);
		for (int i = 0; i < arFilesToSave.GetCount(); i++)
		{
			CString sFile = arFilesToSave[i];
			messages.Add(sFile, CMessages::MSG_HINT);
			PathRemoveExtension(sFile.GetBuffer());
			sFile.ReleaseBuffer();
			sFile += HjsonExtension;
			messages.Add(sFile, CMessages::MSG_HINT);
		}
		if (arFilesToDelete.GetCount())
		{
			messages.Add(_TB("The following files will be deleted:"), CMessages::MSG_HINT);
			for (int i = 0; i < arFilesToDelete.GetCount(); i++)
			{
				CString sFile = arFilesToDelete[i];
				messages.Add(sFile, CMessages::MSG_HINT);
				PathRemoveExtension(sFile.GetBuffer());
				sFile.ReleaseBuffer();
				sFile += HjsonExtension;
				messages.Add(sFile, CMessages::MSG_HINT);
			}
		}
		messages.Add(_TB("Do you want to continue?"), CMessages::MSG_HINT);
		if (!messages.Show())
			return FALSE;
	}
	ASSERT(container.GetCount() == arFilesToSave.GetCount());
	for (int i = container.GetUpperBound(); i >= 0; i--)
	{
		CString sJsonFile = arFilesToSave[i];
		//quelli dopo il primo sono i clientforms
		CWndObjDescription* pChild = container[i];
		RecursiveCreateFolders(GetPath(sJsonFile));

		CLineFile file;
		if (!file.Open(sJsonFile, CFile::modeCreate | CFile::modeWrite | CFile::typeText, NULL, CLineFile::UTF8))
		{
			ShowError(cwsprintf(_TB("Cannot save file {0-%s}"), sJsonFile));
			return FALSE;
		}

		CJsonSerializer ser;
		pChild->SerializeJson(ser);
		file.WriteString(ser.GetJson());
		file.Close();

		CString sHJsonFile = sJsonFile;
		PathRemoveExtension(sHJsonFile.GetBuffer());
		sHJsonFile.ReleaseBuffer();
		sHJsonFile += HjsonExtension;
		if (!file.Open(sHJsonFile, CFile::modeCreate | CFile::modeWrite | CFile::typeText, NULL, CLineFile::UTF8))
		{
			ShowError(cwsprintf(_TB("Cannot save file {0-%s}"), sHJsonFile));
			return FALSE;
		}
		CString hJson;
		hJson.Append(_T("#pragma once\r\n"));
		hJson.Append(_T("#include\t<TbNameSolver\\TBResourcesMap.h>\r\n"));
		hJson.Append(_T("#define "));
		hJson.Append(res.m_strName);
		hJson.Append(_T("\t"));
		hJson.Append(_T("GET_IDD("));
		hJson.Append(res.m_strName);
		hJson.Append(_T(","));
		hJson.Append(res.m_strContext);
		hJson.Append(_T(")\r\n"));
		AppendDefine(hJson, pChild);
		file.WriteString(hJson);
		file.Close();
		/*//se sono diverso dal primo, sono un client form, quindi devo aggiornare il file xml
		if (pDescri != NULL && i > 0)
		{
			CTBNamespace ns(CTBNamespace::MODULE, pChild->m_strContext);
			CClientFormDescription* pClient = pDescri->AddClient(GetName(sJsonFile), ns);
			if (!pClient->PersistOnFileSystem())
			{
				ShowError(cwsprintf(_TB("Cannot add a new client form to file {0-%s}"), AfxGetPathFinder()->GetClientDocumentObjectsFullName(pClient->m_Module)));
				return FALSE;
			}
		}*/
	}

	//faccio pulizia dei file vecchi, solo adesso che ho salvato i nuovi
	for (int i = 0; i < arFilesToDelete.GetCount(); i++)
	{
		CString sFile = arFilesToDelete[i];
		CString sId = GetName(sFile);
		bool isMasterFile = sFile.CompareNoCase(m_arFiles[0]) == 0;
		//il primo è il master file, non è nel file xml dei client forms
		if (pDescri && !isMasterFile && !pDescri->RemoveClient(sId, TRUE))
		{
			ShowError(_TB("Cannot remove client form from file ClientDocumentObjects.xml"));
			return FALSE;
		}
		
	}
	//i miei nuovi file sono quelli che ho appena salvato
	m_arFiles.RemoveAll();
	m_arFiles.Append(arFilesToSave);
	m_strJsonFile = arFilesToSave[0];
	return TRUE;
}

/////////////////////////////////////////////////////////////////////////////////////////////////
///								CEasyStudioDesignerDoc
////////////////////////////////////////////////////////////////////////////////////////////////
IMPLEMENT_DYNCREATE(CEasyStudioDesignerDoc, CDynamicFormDoc)

BEGIN_MESSAGE_MAP(CEasyStudioDesignerDoc, CDynamicFormDoc)
END_MESSAGE_MAP()

//--------------------------------------------------------------------------------
CEasyStudioDesignerDoc::CEasyStudioDesignerDoc()
{
}

//--------------------------------------------------------------------------------
CEasyStudioDesignerDoc::~CEasyStudioDesignerDoc()
{
}


//--------------------------------------------------------------------------------
BOOL CEasyStudioDesignerDoc::OnOpenDocument(LPCTSTR lpszPathName)
{
	if (!__super::OnOpenDocument(lpszPathName))
		return FALSE;

	LPAUXINFO lpInfo = GET_AUXINFO(lpszPathName);

	if (lpInfo)
	{
		m_sInitialFile = (LPCTSTR)lpInfo;
	}
	return TRUE;
}

//--------------------------------------------------------------------------------
BOOL CEasyStudioDesignerDoc::InitDocument()
{
	//thread managed: in lingua, così vedo l'editor tradotto
	Thread::CurrentThread->CurrentUICulture = gcnew CultureInfo(gcnew String(AfxGetCulture()));

	//thread c++: in lingua base, non devo vedere le form tradotte se le sto editando
	AfxGetThreadContext()->SetUICulture(_T(""));

	return __super::InitDocument();
}

//--------------------------------------------------------------------------------
void CEasyStudioDesignerDoc::OnFrameCreated()
{
	if (!CDEasyBuilder::IsLicenseForEasyBuilderVerified())
	{
		PostMessage(WM_CLOSE, 0, 0);
		return;
	}
	PostMessage(WM_COMMAND, ID_FORM_EDITOR_EDIT, 0);
}

//////////////////////////////////////////////////////////////////////////////////	
//								CEasyStudioDesignerView class implementation
//////////////////////////////////////////////////////////////////////////////////	
//-----------------------------------------------------------------------------
IMPLEMENT_DYNCREATE(CEasyStudioDesignerView, CDynamicFormView)
BEGIN_MESSAGE_MAP(CEasyStudioDesignerView, CDynamicFormView)
	ON_WM_ERASEBKGND()
	ON_WM_DESTROY()
END_MESSAGE_MAP()

//-----------------------------------------------------------------------------
CEasyStudioDesignerView::CEasyStudioDesignerView()
	: CDynamicFormView(_T("EasyStudioDesigner"))
{
	SetCenterControls(FALSE);
}

//-----------------------------------------------------------------------------
CEasyStudioDesignerView::~CEasyStudioDesignerView()
{

}
//-----------------------------------------------------------------------------
void CEasyStudioDesignerView::OnDestroy()
{
	__super::OnDestroy();
	if (m_pDialog)
	{
		m_pDialog->DestroyWindow();
	}
}

//-----------------------------------------------------------------------------
BOOL CEasyStudioDesignerView::OnWndMsg(UINT message, WPARAM wParam, LPARAM lParam, LRESULT* pResult)
{
	if (message == WM_COMMAND)
	{
		UINT nID = wParam;
		if (nID == ID_CREATE_JSON_EDITOR)
		{
			m_Connector = gcnew JsonConnector(this, gcnew String(((CEasyStudioDesignerDoc*)GetDocument())->m_sInitialFile));

			IntPtr handle = (IntPtr)System::Runtime::InteropServices::GCHandle::Alloc(m_Connector);
			*pResult = (LRESULT)handle.ToInt64();
			return TRUE;
		}
	}

	return CDynamicFormView::OnWndMsg(message, wParam, lParam, pResult);
}
//-----------------------------------------------------------------------------
BOOL CEasyStudioDesignerView::CloseDialog(const CString& sFile, bool isDocOutline)
{
	CAbstractFormDoc* pDoc = GetDocument();
	const CDocumentDescription* pDescri = pDoc->GetXmlDescription();
	pDoc->SetTitle(pDescri->GetTitle());
	m_sCurrentFile = _T("");
	if (isDocOutline) return TRUE;

	if (m_pDialog && m_pDialog->m_strJsonFile.CompareNoCase(sFile) == 0)
	{
		m_pDialog->DestroyWindow();
		m_pDialog = NULL;
		m_nCodePointer = -1;
		m_arCodeStack.RemoveAll();
		return TRUE;
	}
	return FALSE;
}

//-----------------------------------------------------------------------------
void CEasyStudioDesignerView::UpdateSourceCode()
{
	if (m_pDialog)
		UpdateSourceCode(m_pDialog->GetCode());
}
//-----------------------------------------------------------------------------
void CEasyStudioDesignerView::UpdateSourceCode(CString sCode)
{
	if (!m_CodeControl)
		return;

	for (int i = m_arCodeStack.GetUpperBound(); i >= m_nCodePointer + 1; i--)
		m_arCodeStack.RemoveAt(i);
	m_nCodePointer = m_arCodeStack.Add(sCode);

	m_CodeControl->Code = gcnew String(sCode);
	Select(_T(""));
	m_CodeControl->Refresh();
}

//-----------------------------------------------------------------------------
System::String^ CEasyStudioDesignerView::Undo()
{
	if (m_nCodePointer <= 0)
		return System::String::Empty;
	m_nCodePointer--;
	CString sCode = m_arCodeStack[m_nCodePointer];
	return SetCodeAndUpdate(sCode);
}

//-----------------------------------------------------------------------------
System::String^ CEasyStudioDesignerView::Redo()
{
	if (m_nCodePointer + 1 > m_arCodeStack.GetUpperBound())
		return System::String::Empty;
	m_nCodePointer++;
	CString sCode = m_arCodeStack[m_nCodePointer];
	return SetCodeAndUpdate(sCode);
}

//-----------------------------------------------------------------------------
System::String^ CEasyStudioDesignerView::SetCodeAndUpdate(CString sCode)
{
	UpdateFromSourceCode(sCode);
	System::String^ reNewCode = gcnew String(sCode);
	m_CodeControl->Code = reNewCode;
	return reNewCode;
}

//-----------------------------------------------------------------------------
BOOL CEasyStudioDesignerView::UpdateFromSourceCode(const CString& sCode)
{
	if (!m_pDialog)
		return FALSE;

	CString sFile = m_pDialog->m_strJsonFile;
	CJsonContextObj* pNewContext = CJsonFormEngineObj::GetInstance()->CreateContext();
	pNewContext->m_JsonResource.PopulateFromFile(sFile);
	pNewContext->m_strCurrentResourceContext = pNewContext->m_JsonResource.m_strContext;
	CArray<CWndObjDescription*>ar;
	CJsonFormEngineObj::ParseDescriptionFromText(ar, pNewContext, sCode, NULL, NULL, CWndObjDescription::Undefined);
	if (ar.GetSize())
	{
		ASSERT(ar.GetSize() == 1);
		CWndObjDescription* pDesc = ar[0];
		if (pDesc->IsKindOf(RUNTIME_CLASS(CWndPanelDescription)))
			pNewContext->m_pDescription = (CWndPanelDescription*)pDesc;
	}
	
	if (pNewContext->m_pDescription == NULL)
		AfxGetDiagnostic()->Show();
	m_pDialog->DestroyWindow();
	m_pDialog = new CEasyStudioDesignerDialog(sFile, pNewContext);
	return m_pDialog->Create(this, GetDocument());
}
//-----------------------------------------------------------------------------
BOOL CEasyStudioDesignerView::OpenDialog(const CString& sFile, bool isDocOutline)
{
	CAbstractFormDoc* pDoc = GetDocument();
	m_sCurrentFile = GetName(sFile);
	pDoc->SetTitle(m_sCurrentFile);
	if (isDocOutline) return TRUE;
	
	if (m_pDialog)
	{
		m_pDialog->DestroyWindow();

		m_nCodePointer = -1;
		m_arCodeStack.RemoveAll();
	}
	m_pDialog = new CEasyStudioDesignerDialog(sFile);
	if (!m_pDialog->Create(this, GetDocument()))
	{
		CloseDialog(sFile, isDocOutline);
		return FALSE;
	}
	//voglio le scrollBar se l'area diventa più piccola dell'area stessa della dialog
	CRect dialogRect;
	m_pDialog->GetWindowRect(dialogRect);
	SetScrollSizes(MM_TEXT, dialogRect.Size());
	return TRUE;
}

//-----------------------------------------------------------------------------
void CEasyStudioDesignerView::SetDirty(bool bDirty)
{
	CString sFile = m_sCurrentFile;
	if (m_pDialog)
		sFile = GetName(m_pDialog->m_strJsonFile);
	CAbstractFormDoc* pDoc = GetDocument();
	if(sFile)
		pDoc->SetTitle(bDirty ? sFile + '*' : sFile);
	else 
	{
		const CDocumentDescription* pDescri = pDoc->GetXmlDescription();
		pDoc->SetTitle(pDescri->GetTitle());
	}

}

//-----------------------------------------------------------------------------
void CEasyStudioDesignerView::BuildDataControlLinks()
{
	//EnableWindow(FALSE);
}


//-----------------------------------------------------------------------------
BOOL CEasyStudioDesignerView::OnEraseBkgnd(CDC* pDC)
{
	CRect r;
	GetClientRect(r);
	CBrush	brush;
	brush.CreateSolidBrush(RGB(128, 128, 128));

	pDC->FillRect(r, &brush);

	return FALSE;
}
//-----------------------------------------------------------------------------
void CEasyStudioDesignerView::Select(const CString& sId)
{
	if (m_CodeControl)
	{
		m_CodeControl->SelectCode(gcnew String(sId));
	}

}


//-----------------------------------------------------------------------------
IMPLEMENT_DYNCREATE(CEasyStudioDesignerFrame, CMasterFrame)
//-----------------------------------------------------------------------------
CEasyStudioDesignerFrame::CEasyStudioDesignerFrame()
	:
	CMasterFrame()
{
	m_bHasToolbar = FALSE;
	m_bHasStatusBar = FALSE;
}



