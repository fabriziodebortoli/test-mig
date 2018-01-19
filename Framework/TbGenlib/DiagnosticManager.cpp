#include "stdafx.h" 

#include <TbNameSolver\ApplicationContext.h>
#include <TbNameSolver\LoginContext.h>
#include <TbGeneric\LineFile.h>
#include <TbGeneric\FontsTable.h>
#include <TbGenlib\Parsobj.h>
#include <TbGenlib\BaseApp.h>
#include <TbFrameworkImages\CommonImages.h>
#include <TbFrameworkImages\GeneralFunctions.h>
#include <TbGenlib\TBToolBar.h>
#include <TbGes\DocumentSession.h>
#include <TbGenlib\BaseFrm.hjson> //JSON AUTOMATIC UPDATE


#include "DiagnosticManager.h"

// resources
#include "messages.hjson" //JSON AUTOMATIC UPDATE

//includere come ultimo include all'inizio del cpp
#include "begincpp.dex"

#ifdef _DEBUG
#undef THIS_FILE
static const char THIS_FILE[] = __FILE__;
#endif

static const TCHAR szXmlAltMessagesQuery[]		= _T("/Messages");
static const TCHAR szXmlMessageTag[]			= _T("Message");
static const TCHAR szXmlExplainAttribute[]		= _T("explain");
static const TCHAR szXmlPriorityLevelAttribute[]= _T("priorityLevel");
static const TCHAR szXmlTypeAttribute[]			= _T("type");
static const TCHAR szXmlPriorityLevelErrors[]	= _T("0");
static const TCHAR szXmlPriorityLevelWarnings[]	= _T("1");
static const TCHAR szXmlPriorityLevelInfos[]	= _T("2");

// xml grammar of the managed C# diagnostic manager
static const TCHAR	szXmlFileTag[]				= _T("File");
static const TCHAR	szXmlMessagesQuery[]		= _T("/File/Messages");
static const TCHAR	szXmlMessagesTag[]			= _T("Messages");
static const TCHAR	szXmlMessageTextTag[]		= _T("MessageText");
static const TCHAR	szXmlExtendedInfosTag[]		= _T("ExtendedInfos");
static const TCHAR	szXmlExtendedInfoTag[]		= _T("ExtendedInfo");
static const TCHAR	szXmlCreationDateAttribute[]= _T("creationdate");
static const TCHAR	szXmlLogTypeAttribute[]		= _T("logtype");
static const TCHAR	szXmlTimeAttribute[]		= _T("time");
static const TCHAR	szXmlTextAttribute[]		= _T("text");
static const TCHAR	szXmlNameAttribute[]		= _T("name");
static const TCHAR	szXmlValueAttribute[]		= _T("value");
static const TCHAR	szXmlTypeError[]			= _T("Error");
static const TCHAR	szXmlTypeWarning[]			= _T("Warning");
static const TCHAR	szXmlTypeInfo[]				= _T("Information");
static const TCHAR	szLogTypeApplication[]		= _T("Application");
static const TCHAR	szStyleSheet[]				= _T("xml-stylesheet");
static const TCHAR	szStyleSheetType[]			= _T("type='text/xsl' href='%s'");

static const TCHAR	szMessageLineFeed[]			= _T("\r\n");
//==============================================================================
//	Different viewers classes to show diagnostic depending on the context
//==============================================================================

// base viewer class: no view
//------------------------------------------------------------------------------

//------------------------------------------------------------------------------
CMsgBoxViewer::CMsgBoxViewer ()
{
}

// message box for simple starting operations
//------------------------------------------------------------------------------
BOOL CMsgBoxViewer::Show (CDiagnostic* pDiagnostic, BOOL bClearMessages)
{
	if (!pDiagnostic->IsTopLevel())
	{
		// anche le warning sono comunque errori in sessioni batch, e la Clear
		// e` inutile (i messaggi precedenti devono rimanere). Gli Hint non
		// vengono volutamente sentiti come errori (sono warning piu` morbide).
		return !(pDiagnostic->ErrorFound() || pDiagnostic->WarningFound());
	}
	
	
	// CMessages cannot be used in unattended start: it crashes
	CString sMsg;
	CStringArray arMessages;
	pDiagnostic->ToStringArray(arMessages);

	for (int i=0; i <= arMessages.GetUpperBound(); i++)
		sMsg += arMessages.GetAt(i) + _T("\n");

	::MessageBox(NULL, sMsg, _T("Task Builder.Net: TbLoader Diagnostic "), MB_OK);

	if (bClearMessages)
		pDiagnostic->ClearMessages();
	
	return TRUE;
}
static TCHAR szDocumentImage[]	= _T("Document");
static TCHAR szErrorImage[]		= _T("Error");
static TCHAR szWarningImage[]	= _T("Warning");
static TCHAR szInfoImage[] = _T("Information");

#define COL_MSG_LEN 100


//==============================================================================
//          Class CMessageReport declaration
//==============================================================================
//
//------------------------------------------------------------------------------
IMPLEMENT_DYNAMIC(CMessageReport, CBCGPReportCtrl)

BEGIN_MESSAGE_MAP(CMessageReport, CBCGPReportCtrl)
	ON_WM_CONTEXTMENU()
	ON_COMMAND(ID_COPY_SINGLE_MSG,	CopyMessage)
	ON_COMMAND(ID_COPY_GROUP_MSG,	CopyGroupMessages)
	ON_COMMAND(ID_COPY_ALL_MSG,		CopyAllMessages)
	ON_MESSAGE(UM_GET_CONTROL_DESCRIPTION, OnGetControlDescription)
END_MESSAGE_MAP()

//------------------------------------------------------------------------------
CMessageReport::CMessageReport()
:
CBCGPReportCtrl(),
	m_pDiagnostic	(NULL),
	m_nInfoCount	(0),
	m_nWarningCount	(0),
	m_nErrorCount	(0)
{	
	SetScrollBarsStyle(CBCGPScrollBar::BCGP_SBSTYLE_VISUAL_MANAGER);
}

//---------------------------------------------------------------------------------------------------------------
CMessageReport::~CMessageReport()
{
}

//---------------------------------------------------------------------------------------------------------------
void CMessageReport::SetDiagnostic(CDiagnostic* pDiagnostic)
{
	m_pDiagnostic = pDiagnostic;
}

//---------------------------------------------------------------------------------------------------------------
void CMessageReport::LoadImages()
{
	CString iconNs;
	HICON hIcon = NULL;
	CONST INT ImageSize = ScalePix(20);

	CBCGPToolBarImages lstImages;
	lstImages.SetImageSize(CSize(ImageSize, ImageSize));
	/*Gdiplus::Color colorMaskPng;
	lstImages.SetTransparentColor(RGB(colorMaskPng.GetRed(), colorMaskPng.GetGreen(), colorMaskPng.GetBlue()));*/

	CDC *pdc = GetDC();
	//info
	iconNs = TBGlyph(szIconInfo);
	if (!iconNs.IsEmpty())
	{
		hIcon = TBLoadImage(iconNs, pdc, ImageSize);
	}
	lstImages.AddIcon(hIcon);
	//warning
	iconNs = TBGlyph(szIconWarning);
	if (!iconNs.IsEmpty())
	{
		hIcon = TBLoadImage(iconNs, pdc, ImageSize);
	}
	//error
	lstImages.AddIcon(hIcon);
	iconNs = TBGlyph(szIconError);
	if (!iconNs.IsEmpty())
	{
		hIcon = TBLoadImage(iconNs, pdc, ImageSize);
	}
	lstImages.AddIcon(hIcon);
	lstImages.ExportToImageList(m_Images);

	SetImageList(&m_Images);
	
	if (pdc)
		ReleaseDC(pdc);
}

//---------------------------------------------------------------------------------------------------------------
void CMessageReport::OnInitControl()
{
	LoadImages();

	InsertColumn(0, _T("Level"), 0);
	InsertColumn(1, _T("Banner"), 0);
	InsertColumn(2, _T("MsgNumb"), 0);
	InsertColumn(3, _T("MsgSub"), 0);
	InsertColumn(4, _T("Type"), 10, 0);
	InsertColumn(5, _T("Message"), COL_MSG_LEN);
			
	SetColumnVisible(0, FALSE);
	SetColumnVisible(1, FALSE);
	SetColumnVisible(2, FALSE);
	SetColumnVisible(3, FALSE);

	SetTextColumn(2, FALSE);
	SetTextColumn(3, FALSE);

	EnableMultipleSort(TRUE);
	SetSortColumn(2, TRUE, TRUE);
	SetSortColumn(3, TRUE, TRUE);
	
	//Type icon
	SetColumnAlign(4, HDF_RIGHT);
	SetTextColumn(4, FALSE);
	GetColumnsInfo().SetColumnWidth(4, ScalePix(25));
	SetColumnLocked(4);

	
	EnableHeader(FALSE);
	EnableColumnAutoSize(TRUE);

	SetWholeRowSel(TRUE);
	SetSingleSel(TRUE);
	EnableInvertSelOnCtrl();

	SetCustomColors(AfxGetThemeManager()->GetBackgroundColor(), AfxGetThemeManager()->GetEnabledControlForeColor(), AfxGetThemeManager()->GetTileDialogTitleBkgColor(), AfxGetThemeManager()->GetTileDialogTitleForeColor(), AfxGetThemeManager()->GetBackgroundColor(), AfxGetThemeManager()->GetBackgroundColor());
	SetFont(AfxGetThemeManager()->GetFormFont());
	FistLoading();	
}


//------------------------------------------------------------------------------
CString CMessageReport::GetGroupName(int iColumn, CBCGPGridItem* pItem)
{
	ASSERT_VALID(this);
	ASSERT_VALID(pItem);
	if (iColumn == 0)
		return cwsprintf(_TB("Level {0-%s}"), pItem->GetValue().bstrVal);

	if (iColumn == 1)
		return cwsprintf(_TB("{0-%s}"), pItem->GetValue().bstrVal);

	return CBCGPGridCtrl::GetGroupName(iColumn, pItem);
}

static const TCHAR szRowBanner[] = _T("=================================================");
//------------------------------------------------------------------------------
int CMessageReport::AddLevelMessages(CDiagnosticLevel* pLevel, WORD filterStatus, int& level, int &nMaxLevel)
{
	if (!pLevel)
		return 0;
	
	CDiagnosticItem* pItem;
	CString sMessage;
	int nAddedRow = 0;
	if (level <= m_arNumMsgForLevel.GetUpperBound())
		nAddedRow = m_arNumMsgForLevel.GetAt(level);
	else
		m_arNumMsgForLevel.Add(0);
	
	CString strBanner = pLevel->GetOpeningBanner();
	int nImageType = -1;
	if (strBanner.IsEmpty())
		strBanner = _TB("Messages");

	BOOL bShowInfo = (filterStatus & SHOW_INFO) == SHOW_INFO;
	BOOL bShowWarning = (filterStatus & SHOW_WARNING) == SHOW_WARNING;
	BOOL bShowError = (filterStatus & SHOW_ERROR) == SHOW_ERROR;

	CString strLevel = cwsprintf(_TB("%d"), level);
	CString strType;
	
	for (int i = 0; i < pLevel->m_arMessages.GetSize(); i++)
	{
		pItem = (CDiagnosticItem*)pLevel->m_arMessages.GetAt(i);
		if (!pItem)
			continue;

		if (pItem->IsNestedLevel())
		{
			level++;
			nMaxLevel++;
			int nMsg = AddLevelMessages((CDiagnosticLevel*)pItem, filterStatus, level, nMaxLevel);
			if (nMsg <= 0)
				nMaxLevel--;
			level--;
		}

		sMessage = pItem->GetMessageText();

		//ho dovuto cablarlo... magari vedere con i gestionalisti di eliminarlo @@BAUZI
		if (sMessage.Compare(_T("__________________________________________________________________________")) == 0 ||
			sMessage.Compare(szRowBanner) == 0 ||
			pItem->GetType() == CDiagnostic::Banner)
			continue;

		switch (pItem->GetType())
		{
		case CDiagnostic::Info:
		{
			nImageType = 0;
			m_nInfoCount++;
			strType = _T("Info");
			if (!bShowInfo)
				continue;

			break;
		}

		case CDiagnostic::Warning:
		{
			nImageType = 1;
			m_nWarningCount++;
			strType = _T("Warning");
			if (!bShowWarning)
				continue;
			break;
		}

		case CDiagnostic::Error:
		case CDiagnostic::FatalError:

		{
			nImageType = 2;
			m_nErrorCount++;
			strType = _T("Error");
			if (!bShowError)
				continue;
			break;
		}
		default: break;
		}

		if (pItem->GetErrCode() != szNoErrorCode)
			sMessage = pItem->GetErrCode() + _T(" - ") + sMessage;


		//Devo effettuare i seguenti controlli
		//1. messaggio con \r\n inseriti dal programmatore -> effettuo il token del messaggio dividendo per \r\n e per ogni stringa ottenuta vado al caso 2
		//2. messaggio più lungo di COL_MSG_LEN
		int curPos = 0;
		CString strToken = sMessage.Tokenize(_T("\r\n"), curPos);
		CStringArray strMessagesArray;
		CString strMsgLine;
		CStringArray strArray;


		//sMessage.Tokenize(_T(" "), curPos);
		while (strToken != "")
		{
			strMessagesArray.Add(strToken);
			strToken = sMessage.Tokenize(_T("\r\n"), curPos);
		}
		if (strMessagesArray.GetSize() == 0)
			strMessagesArray.Add(sMessage);


		CString strTokenizeMsg;
		for (int i = 0; i < strMessagesArray.GetSize(); i++)
		{
			strTokenizeMsg = strMessagesArray.GetAt(i);
			strMsgLine.Empty();
			if (strTokenizeMsg.GetLength() > COL_MSG_LEN)
			{
				curPos = 0;
				strToken = strTokenizeMsg.Tokenize(_T(" "), curPos);
				while (strToken != "")
				{
					if (strMsgLine.GetLength() + strToken.GetLength() > COL_MSG_LEN)
					{
						strArray.Add(strMsgLine);
						strMsgLine.Empty();

					}
					if (!strMsgLine.IsEmpty())
						strMsgLine += _T(" ");
					strMsgLine += strToken;
					strToken = strTokenizeMsg.Tokenize(_T(" "), curPos);
				}
				//ultima linea <= COL_MSG_LEN
				strArray.Add(strMsgLine);
			}
			else
				strArray.Add(strTokenizeMsg);
		}
	
		
		nAddedRow++;
		for (int j = 0; j < strArray.GetCount(); j++)
		{
			strMsgLine = strArray.GetAt(j);
			CBCGPGridRow* pRow = CreateRow(GetColumnCount());			
			pRow->GetItem(0)->SetValue((LPCTSTR)strLevel);
			pRow->GetItem(1)->SetValue((LPCTSTR)strBanner);
			pRow->GetItem(2)->SetValue(nAddedRow);
			pRow->GetItem(3)->SetValue(j);			
			pRow->GetItem(4)->SetImage((j == 0) ? nImageType : -1);
			pRow->GetItem(4)->SetData((j == 0) ? pItem->GetType() : 0);
			pRow->GetItem(5)->SetValue((LPCTSTR)strMsgLine);
			
			AddRow(pRow, FALSE);
		}

		m_arNumMsgForLevel.SetAt(level, nAddedRow);
	}

	return nAddedRow;
}

//------------------------------------------------------------------------------
void CMessageReport::FistLoading()
{
	if (!m_pDiagnostic)
		return;
	
	int nLevel = 0;
	int nMaxLevel = 0;
	m_arNumMsgForLevel.RemoveAll();

	AddLevelMessages(m_pDiagnostic->GetStartingLevel(), SHOW_INFO | SHOW_WARNING | SHOW_ERROR, nLevel, nMaxLevel);
	//abbiamo più livelli
	//-------------------
	// Set group columns:
	//-------------------
	if (nMaxLevel > 0)
	{
		InsertGroupColumn(0, 0 /* Level */);
		InsertGroupColumn(1, 1 /* Banner */);
	}

	AdjustLayout();
}

//------------------------------------------------------------------------------
void CMessageReport::Load(WORD filterStatus)
{
	if (!m_pDiagnostic)
		return;

	//Clear();
	RemoveAll();
	m_nInfoCount = 0;
	m_nWarningCount = 0;
	m_nErrorCount = 0;

	int nLevel = 0;
	int nMaxLevel = 0;
	AddLevelMessages(m_pDiagnostic->GetStartingLevel(), filterStatus, nLevel, nMaxLevel);
	
	AdjustLayout();
}

//------------------------------------------------------------------------------
void CMessageReport::OnContextMenu(CWnd*, CPoint point)
{
	CMenu menu;
	menu.CreatePopupMenu();

	//verifico il tipo di riga su cui viene attivato il context menu
	CBCGPGridRow* pRow = GetCurSel();
	if (!pRow)
		return;
	if (pRow->IsGroup())
		menu.AppendMenu(MF_STRING, ID_COPY_GROUP_MSG, _TB("Copy grouping messages"));
	else
		menu.AppendMenu(MF_STRING, ID_COPY_SINGLE_MSG, _TB("Copy this message"));

	menu.AppendMenu(MF_STRING, ID_COPY_ALL_MSG, _TB("Copy all messages"));

	if (point.x == -1 && point.y == -1)
	{
		//--------------------------------------------------------
		// Keyboard, show menu for the currently selected item(s):
		//--------------------------------------------------------
		CBCGPGridItem* pItem = GetCurSelItem();
		if (pItem != NULL)
		{
			CRect rectItem = pItem->GetRect();
			CPoint ptItem(rectItem.left, rectItem.bottom + 1);

			if (GetListRect().PtInRect(ptItem))
			{
				point = ptItem;
				ClientToScreen(&point);
			}
		}	
	}
	menu.TrackPopupMenu(TPM_LEFTALIGN | TPM_RIGHTBUTTON, point.x, point.y, this);
}


//------------------------------------------------------------------------------
void CMessageReport::CopyTextInClipboard(const CString& strText)
{
	if (strText.IsEmpty())
		return;
	try
	{
		if (!OpenClipboard())
		{
			TRACE0("Can't open clipboard\n");
			return;
		}

		if (!::EmptyClipboard())
		{
			TRACE0("Can't empty clipboard\n");
			::CloseClipboard();
			return;
		}

		if (!CopyTextToClipboardInternal(strText, strText.GetLength()))
		{
			::CloseClipboard();
			return;
		}
		
		::CloseClipboard();
	}
	catch (...)
	{
		TRACE0("CBCGPGridCtrl::Copy: out of memory\n");
	}
}

//------------------------------------------------------------------------------
CString FormatItem(int itemIdx, CBCGPGridItem* pItem)
{
	if (itemIdx == 2)
	{
		switch (pItem->GetData())
		{
			case 0: return _T("");
			case CDiagnostic::Info: 		return _T("Info");
			case CDiagnostic::Warning:		return _T("Warning");
			case CDiagnostic::Error:		
			case CDiagnostic::FatalError:	
				return _T("Error");				
		}
	}
	return pItem->FormatItem();
}

//------------------------------------------------------------------------------
CString FormatSingleRow(CBCGPGridRow* pRow, BOOL bWithParent)
{
	CBCGPGridItem* pItem = NULL;
	CString strItem;
	CString strLine;
	if (pRow->IsGroup())
		return pRow->GetName();

	if (bWithParent && pRow->GetParent())
	{
		if (pRow->GetParent()->GetParent())
		{
			strLine = pRow->GetParent()->GetParent()->GetName();
			strLine += _T("\t");
		}
		strLine += pRow->GetParent()->GetName();
		strLine += _T("\t");
		
	}
	int nStart = (bWithParent && strLine.IsEmpty()) ? 1 : 2;

	for (int i = nStart; i < pRow->GetItemCount(); i++)
	{
		pItem = pRow->GetItem(i);
		strItem = FormatItem(i, pItem);
		strItem += _T("\t");
		strLine += strItem;
	}
	return strLine;
}

//------------------------------------------------------------------------------
void CMessageReport::CopyMessage()
{
	CBCGPGridRow* pRow = GetCurSel();
	if (!pRow)
		return;

	CopyTextInClipboard(FormatSingleRow(pRow, TRUE));
}

//------------------------------------------------------------------------------
void FormatSubRowsMessages(CBCGPGridRow* pRow, CString& strText)
{
	if (!pRow)
		return;
	
	CString strLine;
	strText += pRow->GetName();
	strText += _T("\r\n");

	CList<CBCGPGridRow*, CBCGPGridRow*>	lstSubItems;
	pRow->GetSubItems(lstSubItems, TRUE);

	for (POSITION posSubItem = lstSubItems.GetHeadPosition();
		posSubItem != NULL;)
	{
		CBCGPGridRow* pSubRow = lstSubItems.GetNext(posSubItem);
		ASSERT_VALID(pSubRow);
		strLine =  FormatSingleRow(pSubRow, FALSE);
		strLine += _T("\r\n");
		strText += strLine;
	}
}

//------------------------------------------------------------------------------
void CMessageReport::CopyGroupMessages()
{
	CBCGPGridRow* pRow = GetCurSel();
	CString strText;
	FormatSubRowsMessages(pRow, strText);
	CopyTextInClipboard(strText);
}

//------------------------------------------------------------------------------
void CMessageReport::CopyAllMessages()
{
	CopyTextInClipboard(GetAllMessages());
}

//-----------------------------------------------------------------------------
CString CMessageReport::GetAllMessages()
{
	CString strText;
	CString strLine;
	CBCGPGridRow* pRow = NULL;
	CList<CBCGPGridRow*, CBCGPGridRow*>* lstSubItems = (m_lstTerminalItems.GetCount() > 0) ? &m_lstTerminalItems : &m_lstItems;
	for (POSITION posItem = lstSubItems->GetHeadPosition(); posItem != NULL;)
	{
		pRow = lstSubItems->GetNext(posItem);
		ASSERT_VALID(pRow);
		strLine = FormatSingleRow(pRow, (m_lstTerminalItems.GetCount() == 0));
		strLine += _T("\r\n");
		strText += strLine;
	}
	return strText;
}

//-----------------------------------------------------------------------------
LRESULT CMessageReport::OnGetControlDescription(WPARAM wParam, LPARAM lParam)
{
	CWndObjDescriptionContainer* pContainer = (CWndObjDescriptionContainer*)wParam;

	/*Il metodo GetWindowDescription crea da zero una descrizione(del tipo della runtimeclass passata) se non esisteva gia,
	o ripesca quella gia creata nei round trip precedenti se esisteva.
	Nella creazione assegna un id alla descrizione (m_strId), che servira' da chiave per recuperarle.
	Questo id viene creato in modo standard sulal base dell'handle di finestra.
	In alcuni casi pero finestre "differenti" hanno lo stesso id (es. parsedbitmap del bodyedit).
	In questi casi si puo' creare un ID disambiguo e passarlo al metodo GetWindowDescription.
	*/
	CString strId = (LPCTSTR)lParam;
	CTextObjDescription* pDesc = (CTextObjDescription*)pContainer->GetWindowDescription(this, RUNTIME_CLASS(CTextObjDescription), strId);
	pDesc->UpdateAttributes(this);
	
	
	//TOSO raffinare la gestione grafica
	pDesc->m_Type = CWndObjDescription::Label;

	CString strAllMessages = GetAllMessages();
	if (pDesc->m_strText != strAllMessages)
	{
		pDesc->m_strText = strAllMessages;
		pDesc->SetUpdated(&pDesc->m_strText);
	}
	return (LRESULT)pDesc;
}

//==============================================================================
//          Class CDefaultMessagesViewer declaration
//==============================================================================
//
//------------------------------------------------------------------------------
IMPLEMENT_DYNAMIC(CDefaultMessagesViewer, CParsedDialogWithTiles)
//------------------------------------------------------------------------------
BEGIN_MESSAGE_MAP(CDefaultMessagesViewer, CParsedDialogWithTiles)
	ON_COMMAND(IDCANCEL, OnCorrect)
	ON_COMMAND(ID_MESSAGES_SHOW_INFO,				OnInfoShow)
	ON_COMMAND(ID_MESSAGES_SHOW_WARNING,			OnWarningShow)
	ON_COMMAND(ID_MESSAGES_SHOW_ERROR,				OnErrorShow)
	ON_WM_TIMER()
END_MESSAGE_MAP()

//------------------------------------------------------------------------------
CDefaultMessagesViewer::CDefaultMessagesViewer()
	:
	CParsedDialogWithTiles(IDD_MESSAGES_DIALOG, NULL, _NS_DLG("Framework.TbGenlib.TbGenlib.Shared.MessagesViewer")),
	m_pDiagnostic	(NULL)
{
	m_wFilterStatus = SHOW_INFO | SHOW_WARNING | SHOW_ERROR;
}

//------------------------------------------------------------------------------
CDefaultMessagesViewer::~CDefaultMessagesViewer()
{
}

//------------------------------------------------------------------------------
BOOL CDefaultMessagesViewer::OnInitDialog()
{
	m_clrBackground = AfxGetThemeManager()->GetTileDialogTitleBkgColor();

	CParsedDialogWithTiles::OnInitDialog();
	CParsedForm::SetBackgroundColor(m_clrBackground);

	SetToolbarStyle(ToolbarStyle::BOTTOM, 32, TRUE, TRUE);
	
	CenterWindow(GetOwner());

	CRect rectGrid;
	m_wndMsgReportLocation.SubclassDlgItem(IDC_MESSAGES_PLACE_REPORT, this);
	m_wndMsgReportLocation.GetWindowRect(&rectGrid);
	ScreenToClient(rectGrid);

	m_aWndMsgReport.Create(WS_CHILD | WS_VISIBLE, rectGrid, this, IDC_MESSAGES_REPORT);
	m_aWndMsgReport.OnInitControl();

	InitToolbar();		
	DoStretch();
	// return TRUE  unless you set the focus to a control
	return FALSE;
}

//------------------------------------------------------------------------------
void CDefaultMessagesViewer::InitToolbar()
{
	//  applying developer requests on UI
	if (m_pToolBar)
	{
		if (m_pDiagnostic->IsKindOf(RUNTIME_CLASS(CMessages)))
		{
			CMessagesUIInterface* pUIIntrface = &((CMessages*)m_pDiagnostic)->m_UIInterface;

			if (pUIIntrface)
			{
				if (pUIIntrface->m_nIDDefault)
					m_pToolBar->SetDefaultButton(pUIIntrface->m_nIDDefault);

				if (!pUIIntrface->m_strOKText.IsEmpty())
					m_pToolBar->SetText(IDOK, pUIIntrface->m_strOKText);

				if (!pUIIntrface->m_strFixText.IsEmpty())
					m_pToolBar->SetText(IDCANCEL, pUIIntrface->m_strFixText);
			}
		}

		m_pToolBar->EnableButton(IDCANCEL, (m_pDiagnostic->WarningFound() || m_pDiagnostic->InfoFound()) && !m_pDiagnostic->ErrorFound());
		
		m_pToolBar->SetText(ID_MESSAGES_SHOW_INFO, cwsprintf(_TB("Info {0-%d}"), m_aWndMsgReport.GetInfoCount()));
		m_pToolBar->SetText(ID_MESSAGES_SHOW_WARNING, cwsprintf(_TB("Warnings {0-%d}"), m_aWndMsgReport.GetWarningCount()));
		m_pToolBar->SetText(ID_MESSAGES_SHOW_ERROR, cwsprintf(_TB("Errors {0-%d}"), m_aWndMsgReport.GetErrorCount()));

		if (m_aWndMsgReport.GetInfoCount() > 0 && (m_aWndMsgReport.GetWarningCount() > 0 || m_aWndMsgReport.GetErrorCount() > 0))
		{
			m_pToolBar->EnableButton(ID_MESSAGES_SHOW_INFO, TRUE);
			m_pToolBar->PressButton(ID_MESSAGES_SHOW_INFO, TRUE);
		}
		else
			m_pToolBar->EnableButton(ID_MESSAGES_SHOW_INFO, FALSE);

		if (m_aWndMsgReport.GetWarningCount() > 0 && (m_aWndMsgReport.GetInfoCount() > 0 || m_aWndMsgReport.GetErrorCount() > 0))
		{
			m_pToolBar->EnableButton(ID_MESSAGES_SHOW_WARNING, TRUE);
			m_pToolBar->PressButton(ID_MESSAGES_SHOW_WARNING, TRUE);
		}
			else
				m_pToolBar->EnableButton(ID_MESSAGES_SHOW_WARNING, FALSE);

		if (m_aWndMsgReport.GetErrorCount() > 0 && (m_aWndMsgReport.GetInfoCount() > 0 || m_aWndMsgReport.GetWarningCount() > 0))
		{
			m_pToolBar->EnableButton(ID_MESSAGES_SHOW_ERROR, TRUE);
			m_pToolBar->PressButton(ID_MESSAGES_SHOW_ERROR, TRUE);
		}
		else
			m_pToolBar->EnableButton(ID_MESSAGES_SHOW_ERROR, FALSE);
	}
	
}

//------------------------------------------------------------------------------
BOOL CDefaultMessagesViewer::Show(CDiagnostic* pDiagnostic, BOOL bClearMessages /*TRUE*/)
{
	if (!pDiagnostic)
	{
		ASSERT(FALSE);
		return FALSE;
	}

	m_pDiagnostic = pDiagnostic;
	if (m_hWnd != NULL || AfxIsCurrentlyInUnattendedMode())
		return !pDiagnostic->ErrorFound();
	ASSERT(m_hWnd == NULL);

	// non fa vedere nulla se il livello di sessione non e` il piu` elevato
	// ma se il livello di sessione e'piu'  elevato la Show torna FALSE
	// Questo vuol dire che as esempio che tipicamente in BATCH la show se c? un Warning torna FALSE
	// ATTENZIONE alle OnOkTransaction che in interattivo e in batch possono avere comportamenti divarsi!!!!!
	if (!pDiagnostic->IsTopLevel())
	{
		// anche le warning sono comunque errori in sessioni batch, e la Clear
		// e` inutile (i messaggi precedenti devono rimanere). Gli Hint non
		// vengono volutamente sentiti come errori (sono warning piu` morbide).
		return !(pDiagnostic->ErrorFound() || pDiagnostic->WarningFound());
	}

	// non fa vedere nulla se non ci sono errori o warning
	if (!pDiagnostic->HasFatalError() && !pDiagnostic->MessageFound())
	{
		if (bClearMessages)	pDiagnostic->ClearMessages();
		return TRUE;
	}

	BOOL bErrors = pDiagnostic->ErrorFound();

	int nResult = 0;
	CMessages* pMessages = NULL;
	CMessagesUIInterface* pUIIntrface = NULL;

	if (m_pDiagnostic->IsKindOf(RUNTIME_CLASS(CMessages)))
		pMessages = (CMessages*)m_pDiagnostic;

	if (pMessages)
		pUIIntrface = &pMessages->m_UIInterface;

	if (pUIIntrface && !pUIIntrface->m_strFileName.IsEmpty())
		CDiagnosticManager::LoadFromFile(pDiagnostic, pUIIntrface->m_strFileName);

	// As this class instance is shared to all methods that call diagnostic objects owned by
	// login and application thread, I have to clear the parent window because it could be 
	// initialized in a previous DoModal with a parent handle that now could be already dead.
	m_pParentWnd = NULL;
	
	m_aWndMsgReport.SetDiagnostic(m_pDiagnostic);

	nResult = DoModal();

	if (bClearMessages)
		pDiagnostic->ClearMessages(TRUE);

	return (nResult != IDCANCEL && !bErrors);
}
//------------------------------------------------------------------------------
void CDefaultMessagesViewer::OnCustomizeToolbar()
{
	if (!m_pToolBar)
		return;

	m_pToolBar->AddButton(ID_MESSAGES_SHOW_INFO, _NS_TOOLBARBTN("ShowInfo"), TBIcon(szIconInfo, IconSize::TOOLBAR), _TB("Info"));
	m_pToolBar->AddButton(ID_MESSAGES_SHOW_WARNING, _NS_TOOLBARBTN("ShowWarning"), TBIcon(szIconWarning, IconSize::TOOLBAR), _TB("Warnings"));
	m_pToolBar->AddButton(ID_MESSAGES_SHOW_ERROR, _NS_TOOLBARBTN("ShowError"), TBIcon(szIconError, IconSize::TOOLBAR), _TB("Errors"));
	
	__super::OnCustomizeToolbar();	
}


//------------------------------------------------------------------------------
void CDefaultMessagesViewer::ResizeOtherComponents(CRect aRect)
{
	__super::ResizeOtherComponents(aRect);

	if (m_aWndMsgReport.m_hWnd != 0)
		m_aWndMsgReport.SetWindowPos(NULL, 0, aRect.top, aRect.Width(), aRect.Height(), SWP_NOZORDER);
}


//------------------------------------------------------------------------------
void CDefaultMessagesViewer::OnCorrect()
{
	if (m_pDiagnostic)
		m_pDiagnostic->ClearMessages();

	EndDialog(IDCANCEL);
}

//------------------------------------------------------------------------------
void CDefaultMessagesViewer::SetFilterStatus(WORD aStatusFlag)
{
	m_wFilterStatus = (m_wFilterStatus & aStatusFlag) == aStatusFlag ? m_wFilterStatus & ~aStatusFlag : m_wFilterStatus | aStatusFlag;
}


//------------------------------------------------------------------------------
BOOL CDefaultMessagesViewer::HasFilterStatus(WORD aStatusFlag)
{
	return (m_wFilterStatus & aStatusFlag) == aStatusFlag;
}

//------------------------------------------------------------------------------
void CDefaultMessagesViewer::OnInfoShow()
{
	SetFilterStatus(SHOW_INFO);
	m_pToolBar->PressButton(ID_MESSAGES_SHOW_INFO, m_aWndMsgReport.GetInfoCount() > 0 && HasFilterStatus(SHOW_INFO));
	m_aWndMsgReport.Load(m_wFilterStatus);
}

//------------------------------------------------------------------------------
void CDefaultMessagesViewer::OnWarningShow()
{
	SetFilterStatus(SHOW_WARNING);
	m_pToolBar->PressButton(ID_MESSAGES_SHOW_WARNING, m_aWndMsgReport.GetWarningCount() > 0 && HasFilterStatus(SHOW_WARNING));
	m_aWndMsgReport.Load(m_wFilterStatus);
}

//------------------------------------------------------------------------------
void CDefaultMessagesViewer::OnErrorShow()
{
	SetFilterStatus(SHOW_ERROR);
	m_pToolBar->PressButton(ID_MESSAGES_SHOW_ERROR, m_aWndMsgReport.GetErrorCount() > 0 && HasFilterStatus(SHOW_ERROR));
	m_aWndMsgReport.Load(m_wFilterStatus);
}


//==============================================================================
//          Class CDefaultMessagesLogDialog declaration
//==============================================================================

IMPLEMENT_DYNAMIC(CDefaultMessagesLogDialog, CParsedDialogWithTiles)

BEGIN_MESSAGE_MAP(CDefaultMessagesLogDialog, CParsedDialogWithTiles)
	ON_WM_TIMER()
END_MESSAGE_MAP()

//------------------------------------------------------------------------------
CDefaultMessagesLogDialog::CDefaultMessagesLogDialog() :
	CParsedDialogWithTiles(IDD_MESSAGES_LOG, NULL, _NS_DLG("Framework.TbGenlib.TbGenlib.Shared.MessagesViewer")),
	m_bShow	(FALSE),
	m_pActiveWindows (NULL),
	m_pDiagnostic (NULL)
{
	
}

//------------------------------------------------------------------------------
CDefaultMessagesLogDialog::~CDefaultMessagesLogDialog()
{

}

//------------------------------------------------------------------------------
BOOL CDefaultMessagesLogDialog::OnInitDialog()
{
	m_clrBackground = AfxGetThemeManager()->GetTileDialogTitleBkgColor();

	CParsedDialogWithTiles::OnInitDialog();
	CParsedForm::SetBackgroundColor(m_clrBackground);

	SetToolbarStyle(ToolbarStyle::BOTTOM, 32, TRUE, TRUE);

	CenterWindow(GetOwner());

	CRect rectGrid;
	m_wndMsgReportLocation.SubclassDlgItem(IDC_MESSAGES_PLACE_LOG, this);
	m_wndMsgReportLocation.GetWindowRect(&rectGrid);
	ScreenToClient(rectGrid);

	m_aWndMsgReport.Create(WS_CHILD | WS_VISIBLE, rectGrid, this, IDC_MESSAGES_REPORT);
	m_aWndMsgReport.OnInitControl();

	DoStretch();
	// return TRUE  unless you set the focus to a control
	return FALSE;
}

//------------------------------------------------------------------------------
BOOL CDefaultMessagesLogDialog::Show(CDiagnostic* pDiagnostic, int iX/* = 0*/, int iY/* = 0*/)
{
	if (m_bShow) {
		return FALSE;
	}

	if (!pDiagnostic)
	{
		ASSERT(FALSE);
		return FALSE;
	}

	m_pDiagnostic = pDiagnostic;
	if (m_hWnd != NULL || AfxIsCurrentlyInUnattendedMode()) {
		ShowWindow(SW_SHOW);
		return TRUE;
	}
	ASSERT(m_hWnd == NULL);

	// non fa vedere nulla se non ci sono errori o warning
	if (!pDiagnostic->HasFatalError() && !pDiagnostic->MessageFound())
	{
		return TRUE;
	}

	BOOL bErrors = pDiagnostic->ErrorFound();

	int nResult = 0;
	CMessages* pMessages = NULL;
	CMessagesUIInterface* pUIIntrface = NULL;

	/*if (m_pDiagnostic->IsKindOf(RUNTIME_CLASS(CMessages)))
		pMessages = (CMessages*)m_pDiagnostic;

	if (pMessages)
		pUIIntrface = &pMessages->m_UIInterface;

	if (pUIIntrface && !pUIIntrface->m_strFileName.IsEmpty())
		CDiagnosticManager::LoadFromFile(pDiagnostic, pUIIntrface->m_strFileName);
*/
	//// As this class instance is shared to all methods that call diagnostic objects owned by
	//// login and application thread, I have to clear the parent window because it could be 
	//// initialized in a previous DoModal with a parent handle that now could be already dead.
	m_pParentWnd = NULL;
	
	m_aWndMsgReport.SetDiagnostic(m_pDiagnostic);

	// Create Form
	Create(m_nID);

	RepositionWindows(iX, iY);

	return TRUE;
}

//------------------------------------------------------------------------------
void CDefaultMessagesLogDialog::RepositionWindows(int iX, int iY)
{
	// Move the windows
	CRect rect;
	GetWindowRect(rect);
	if ((iX == 0 && iY == 0) || (rect.top == 0 && rect.bottom && rect.left == 0 && rect.right == 0))
	{
		return;
	}

	ShowWindow(SW_SHOW);
	MoveWindow(iX, iY - rect.Height(), rect.Width(), rect.Height());
	SetTimer(ID_DIAGNOSTIC_TIMER, DELAY_DIAGNOSTIC_TIMER, NULL);
	m_bShow = TRUE;
	m_pActiveWindows = GetActiveWindow();
}

//------------------------------------------------------------------------------
void CDefaultMessagesLogDialog::OnTimer(UINT nIDEvent)
{
	if (m_bShow && nIDEvent == ID_DIAGNOSTIC_TIMER)
	{
		CWnd* pWnd = GetActiveWindow();
		if (m_pActiveWindows != pWnd)
		{
			m_bShow = FALSE;
			ShowWindow(SW_HIDE);
			return;
		}
		// Update windows data
		m_aWndMsgReport.Load(SHOW_INFO | SHOW_WARNING | SHOW_ERROR);
	}
}

//==============================================================================
//          Class CMessagesViewer declaration
//==============================================================================
//
//-----------------------------------------------------------------------------
CMessagesViewer::CMessagesViewer() :
	m_pMessagesLog(NULL)
{
	
}

//-----------------------------------------------------------------------------
CMessagesViewer::~CMessagesViewer()
{
}

//-----------------------------------------------------------------------------
CWnd* CMessagesViewer::ShowNoModal(CDiagnostic* pDiagnostic, int iX /*= 0*/, int iY /*= 0*/)
{	
	if (!m_pMessagesLog)
	{
		m_pMessagesLog = new CDefaultMessagesLogDialog();
	}
	
	m_pMessagesLog->Show(pDiagnostic, iX, iY);
	return m_pMessagesLog; 
}

//-----------------------------------------------------------------------------
BOOL CMessagesViewer::Show(CDiagnostic* pDiagnostic, BOOL bClearMessages)
{	
	CDefaultMessagesViewer m_Viewer;
	return m_Viewer.Show(pDiagnostic, bClearMessages);
}


//==============================================================================
//          Class CMessagesWebViewer declaration
//==============================================================================
//
//-----------------------------------------------------------------------------
CWnd* CMessagesWebViewer::ShowNoModal(CDiagnostic* pDiagnostic, int iX /*= 0*/, int iY /*= 0*/)
{
	return NULL;
}

//-----------------------------------------------------------------------------
BOOL CMessagesWebViewer::Show(CDiagnostic* pDiagnostic, BOOL bClearMessages)
{
	CDocumentSession* pSession = (CDocumentSession*)AfxGetThreadContext()->m_pDocSession;
	ASSERT(pSession);
	BOOL b = pSession ? pSession->DiagnosticDialog(pDiagnostic) : FALSE;
	if (bClearMessages)
		pDiagnostic->ClearMessages(TRUE);
	return b;
}
//-----------------------------------------------------------------------------
CLogFileViewer::CLogFileViewer (const CString& sFileName) 
{
	m_sFileName = sFileName;
}

//-----------------------------------------------------------------------------
BOOL CLogFileViewer::Show(CDiagnostic* pDiagnostic, BOOL bClearMessages)
{
	BOOL bOk = CDiagnosticManager::LogToXml(pDiagnostic, m_sFileName);

	if (bClearMessages)
		pDiagnostic->ClearMessages();
	
	return bOk;
}

//==============================================================================
//          Class CDiagnosticManagerWriter implementation
//==============================================================================

//-----------------------------------------------------------------------------
CDiagnosticManagerWriterExtInfo::CDiagnosticManagerWriterExtInfo (const CString& sName, const CString& sValue)
{
	m_sName = sName;
	m_sValue = sValue;
}

//==============================================================================
//          Class CDiagnosticManagerWriter implementation
//==============================================================================
//-----------------------------------------------------------------------------
IMPLEMENT_DYNAMIC (CDiagnosticManagerWriter, CObject)

CDiagnosticManagerWriter::CDiagnosticManagerWriter ()
	:
	m_bValid (FALSE)
{
	m_sFileName.Empty ();
}

//-----------------------------------------------------------------------------
BOOL CDiagnosticManagerWriter::Open	(const CString& sFileName, const CString& sXSLFileName /*_T("")*/, BOOL bAppend /*TRUE*/)
{
	// already opened
	if (m_sFileName.CompareNoCase(sFileName) == 0 && m_bValid)
		return TRUE;

	m_sFileName		= sFileName;
	m_sXSLFileName = sXSLFileName;

	m_bValid = FALSE;

	// if exists I load it and I appen the file
	BOOL bLoaded = FALSE;
	if (ExistFile (m_sFileName) && bAppend)
		bLoaded  = m_LogFile.LoadXMLFile (m_sFileName);

	if (bLoaded)
	{
		m_bValid = TRUE;
		return TRUE;
	}

	// header of the file
	m_LogFile.CreateInitialProcessingInstruction ();

	if (!m_sXSLFileName.IsEmpty())
		m_LogFile.CreateInitialProcessingInstruction(szStyleSheet, cwsprintf(szStyleSheetType, m_sXSLFileName));

	CXMLNode* pRootNode = m_LogFile.CreateRoot(szXmlFileTag);
	if (!pRootNode)
	{
		TRACE2 ("CDiagnosticManagerWriter::Open	cannot write %s into user log file %s", szXmlFileTag, m_sFileName);
		ASSERT(FALSE);
		return FALSE;
	}

	SYSTEMTIME	today;
	::GetLocalTime (&today);
	
	DataDate now;
	now.SetFullDate ();
	now.Assign (today);

	pRootNode->SetAttribute (szXmlNameAttribute, GetName(sFileName));
	pRootNode->SetAttribute (szXmlCreationDateAttribute, now.FormatDataForXML());
	pRootNode->SetAttribute (szXmlLogTypeAttribute, szLogTypeApplication);
	CXMLNode* pNode = pRootNode->CreateNewChild (szXmlMessagesTag);
	if (!pNode)
	{
		TRACE2 ("CDiagnosticManagerWriter::Open	cannot write %s tag into user log file %s", szXmlMessagesTag, m_sFileName);
		ASSERT(FALSE);
		return FALSE;
	}

	m_bValid = TRUE;
	return TRUE;
}

//-----------------------------------------------------------------------------
BOOL CDiagnosticManagerWriter::Save	(const CString& sFileName)
{
	if (!m_bValid)
		return FALSE;
	
	return m_LogFile.SaveXMLFile (sFileName, TRUE);
}

//-----------------------------------------------------------------------------
BOOL CDiagnosticManagerWriter::Save	()
{	
	return Save(m_sFileName);
}

//-----------------------------------------------------------------------------
void CDiagnosticManagerWriter::AddError	(const CString& sMessage, Array* pExtendedInfos /*NULL*/)
{
	AddMessage (CDiagnosticManagerWriter::Error, sMessage, pExtendedInfos);
}

//-----------------------------------------------------------------------------
void CDiagnosticManagerWriter::AddWarning	(const CString& sMessage, Array* pExtendedInfos /*NULL*/)
{
	AddMessage (CDiagnosticManagerWriter::Warning, sMessage, pExtendedInfos);
}

//-----------------------------------------------------------------------------
void CDiagnosticManagerWriter::AddInformation (const CString& sMessage, Array* pExtendedInfos /*NULL*/)
{
	AddMessage (CDiagnosticManagerWriter::Information, sMessage, pExtendedInfos);
}

//-----------------------------------------------------------------------------
void CDiagnosticManagerWriter::AddMessage (const MessageType& eType, const CString& sMessage, Array* pExtendedInfos /*NULL*/)
{
	if (!m_bValid)
		return;

	CXMLNode* pMessagesNode = m_LogFile.SelectSingleNode(szXmlMessagesQuery);
	if (!pMessagesNode)
	{
		TRACE2 ("CDiagnosticManagerWriter::AddMessage cannot find %s tag into user log file %s", szXmlMessagesQuery, m_sFileName);
		ASSERT(FALSE);
		return ;
	}
	CXMLNode* pMessageNode = pMessagesNode->CreateNewChild (szXmlMessageTag);
	if (!pMessageNode)
	{
		TRACE2 ("CDiagnosticManagerWriter::AddMessage cannot create %s tag into user log file %s", szXmlMessageTag, m_sFileName);
		ASSERT(FALSE);
		delete pMessagesNode;
		return ;
	}

	SYSTEMTIME	today;
	::GetLocalTime (&today);
	
	DataDate now;
	now.SetFullDate ();
	now.Assign (today);

	pMessageNode->SetAttribute (szXmlTypeAttribute, TypeToXml(eType));
	pMessageNode->SetAttribute (szXmlTimeAttribute, now.FormatDataForXML());

	CXMLNode* pNode = pMessageNode->CreateNewChild (szXmlMessageTextTag);
	if (!pNode)
	{
		TRACE2 ("CDiagnosticManagerWriter::AddMessage	cannot create %s tag into user log file %s", szXmlMessageTextTag, m_sFileName);
		ASSERT(FALSE);
		delete pMessagesNode;
		return ;
	}

	pNode->SetAttribute (szXmlTextAttribute, sMessage);

	// no extended info
	if (!pExtendedInfos || pExtendedInfos->GetSize() == 0)
	{
		delete pMessagesNode;
		return;
	}

	// extended infos
	CXMLNode* pInfosNode = pMessageNode->CreateNewChild (szXmlExtendedInfosTag);
	if (!pNode)
	{
		TRACE2 ("CDiagnosticManagerWriter::AddMessage cannot create %s tag into user log file %s", szXmlExtendedInfosTag, m_sFileName);
		ASSERT(FALSE);
		delete pMessagesNode;
		return ;
	}

	CDiagnosticManagerWriterExtInfo* pInfo;
	for (int i=0; i <= pExtendedInfos->GetUpperBound(); i++)
	{
		pInfo = (CDiagnosticManagerWriterExtInfo*) pExtendedInfos->GetAt (i);
		if (pInfo && pInfo->IsKindOf(RUNTIME_CLASS(CDiagnosticManagerWriterExtInfo)))
		{
			pNode = pInfosNode->CreateNewChild (szXmlExtendedInfoTag);
			if (pNode)
			{
				pNode->SetAttribute (szXmlNameAttribute, pInfo->m_sName);
				pNode->SetAttribute (szXmlValueAttribute, pInfo->m_sValue);
			}
		}
	}

	delete pMessagesNode;
}

//-----------------------------------------------------------------------------
CString CDiagnosticManagerWriter::TypeToXml (const MessageType& eType)
{
	switch (eType)
	{
	case CDiagnosticManagerWriter::Error:
		return szXmlTypeError;
	case CDiagnosticManagerWriter::Warning:
		return szXmlTypeWarning;
	default:
		return szXmlTypeInfo;
	}
}

//-----------------------------------------------------------------------------
const CString& CDiagnosticManagerWriter::GetLogFileName () const
{
	return m_sFileName;
}

//-----------------------------------------------------------------------------
void CDiagnosticManagerWriter::SetLogFileName(const CString& sLogFileName)
{
	m_sFileName = sLogFileName;
}

//==============================================================================
//          Class CDiagnosticManager implementation
//==============================================================================
IMPLEMENT_DYNAMIC (CDiagnosticManager, CObject)   

//-----------------------------------------------------------------------------
BOOL CDiagnosticManager::LogToXml (CDiagnostic* pDiagnostic, const CString& sFileName)
{
	if (!pDiagnostic)
	{
		ASSERT(FALSE);
		return FALSE;
	}

	if (!pDiagnostic->MessageFound())
		return TRUE;

	TRY
	{
		CDiagnosticManagerWriter aWriter;
		if (!aWriter.Open (sFileName))
		{
			TRACE1 ("Cannot Log diagnostic to XML file %s: file not opened", sFileName);
			ASSERT(FALSE);
			return FALSE;
		}

		CObArray arItems;
		pDiagnostic->ToArray(arItems);
		// body
		CDiagnosticItem* pItem;
		for (int i=0; i <= arItems.GetUpperBound(); i++)
		{
			pItem = (CDiagnosticItem*) arItems.GetAt(i);
			CDiagnosticManagerWriter::MessageType aType;
			switch (pItem->GetType())
			{
				case CDiagnostic::Info:		aType = CDiagnosticManagerWriter::Information; break;
				case CDiagnostic::Warning:	aType = CDiagnosticManagerWriter::Warning; break;
				case CDiagnostic::Error:
				case CDiagnostic::FatalError:
					aType = CDiagnosticManagerWriter::Error; break;
			}

			aWriter.AddMessage (aType, pItem->GetMessageText());
		}
		if (!aWriter.Save())
		{
			TRACE1 ("Cannot Log diagnostic to XML file %s: file not saved", sFileName);
			ASSERT(FALSE);
			return FALSE;
		}

	}
	CATCH (CException, e)
	{
		TCHAR szError[1024];
		e->GetErrorMessage(szError, 1024);

		TRACE2 ("Cannot Log diagnostic to XML file %s due to the following exception %s", sFileName, szError);
		ASSERT(FALSE);
		return FALSE;
	}
	END_CATCH	

	return TRUE;
}

// Tenta di scrivere sempre il file di Log anche in UnattendedMode, in modo che
// chi ha i diritti su filesystem pu?farlo. L'unica cosa che faccio ?eliminare
// dalla CMessages il warning di mancata creazione in modo che non rompa le scatole
// quando non ci sono i diritti di scrittura (SchedulerAgent)
//------------------------------------------------------------------------------
BOOL CDiagnosticManager::LogToFile (CDiagnostic* pDiagnostic, const CString& sFileName)
{
	CString sText;
	DataDate oggi(TodayDate ());

	UINT nFlags =	CFile::modeCreate | CFile::modeWrite | 
					CFile::shareDenyWrite | CFile::typeText | CFile::modeNoTruncate;

	CLineFile aFile;

	TRY
	{
		if (!aFile.Open (sFileName, nFlags))
		{
			pDiagnostic->Add (cwsprintf(_TB("Cannot open %s log file on Custom folder.\r\nCheck folder write access attibutes. "), (LPCTSTR) sFileName), CDiagnostic::Warning);
			return FALSE;
		}

		aFile.SeekToEnd ();

		sText = _TB("======== Messages generated on") + oggi.FormatData() + _T("\n");
		aFile.WriteString(sText);

		CStringArray arMessages;
		pDiagnostic->ToStringArray(arMessages);

		for (int i=0; i <= arMessages.GetUpperBound(); i++)
			aFile.WriteString(arMessages.GetAt(i) + _T("\n"));

		sText = _TB("======== End of Messages =======");
		aFile.WriteString(sText + _T("\n"));
		aFile.Close();
	}
	CATCH(CException, e)
	{
		aFile.Close();
		TCHAR szError[1024];
		e->GetErrorMessage(szError, 1024);
		sText = _TB("Unable to generate the message log file.") + CString(szError);
		// TODOBRUNA lo schedulerAgent non ha i diritti per scrivere il file di log.
		//			 Quando si rivedr?la messaggistica, questi messaggi andranno buttati 
		//			 nell'EventManager e dovr?sparire il log.
		pDiagnostic->Add (sText, CDiagnostic::Warning);
	}
	END_CATCH	
	
	return TRUE;
}

//------------------------------------------------------------------------------
BOOL CDiagnosticManager::LoadFromFile(CDiagnostic* pDiagnostic, const CString& sFileName)
{
	CFileException	exception;  // signal file exception
	CLineFile		ifile;      // file to be parsed

	UINT nFlags = CFile::modeRead | CFile::shareDenyWrite | CFile::typeText;

	if (!ifile.Open(sFileName, nFlags, &exception))
	{
		pDiagnostic->Add (PCause(&exception), CDiagnostic::Warning);
		return FALSE;
	}

	// try to read from file
	TRY 
	{
		CString	strAll;
		CString	strBuffer;
		int		nTabSize = 16;

		while (ifile.ReadString(strBuffer.GetBuffer(1024), 1023, TRUE))
		{
			strBuffer.ReleaseBuffer();
			strAll += strBuffer;
		}

		pDiagnostic->Add (strAll, CDiagnostic::Info);
		ifile.Close();
	}
	CATCH (CFileException, e)
	{
		pDiagnostic->Add (PCause(e), CDiagnostic::Warning);
		return FALSE;
	}
	END_CATCH

	return TRUE;
}

//------------------------------------------------------------------------------
void CDiagnosticManager::ToArray (CDiagnostic* pDiagnostic, DataObjArray& arValues)
{
	arValues.RemoveAll();
	CStringArray arMessages;
	pDiagnostic->ToStringArray(arMessages);

	for (int i = 0; i <= arMessages.GetUpperBound(); i++)
		arValues.Add(new DataStr(arMessages.GetAt(i)));
}

//------------------------------------------------------------------------------
IDiagnosticViewer* AfxCreateDefaultViewer()
{
	return AfxIsRemoteInterface()
		? (IDiagnosticViewer*)new CMessagesWebViewer()
		: (IDiagnosticViewer*)new CMessagesViewer();
}