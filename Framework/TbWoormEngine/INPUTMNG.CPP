
#include "stdafx.h"

#include <TbNameSolver\PathFinder.h>
#include <TbGeneric\EnumsTable.h>
#include <TbGeneric\DataObj.h>
#include <TbGeneric\schedule.h>
#include <TbGeneric\FormatsTable.h>
#include <TbGeneric\globals.h>

#include <TbGenlib\baseapp.h>
#include <TbGenlib\basedoc.h>
#include <TbGenlib\expr.h>
#include <TbGenlib\hlinkobj.h>
#include <TbGenlib\parsctrl.h>
#include <TbGenlib\tabcore.h>
#include <TbGenlib\AutoExpressionMng.h>
#include <TbGenlib\LayoutContainer.h>

#include <TbFrameworkImages\CommonImages.h>
#include <TbFrameworkImages\GeneralFunctions.h>

#include "askdata.h"
#include "inputmng.h"
#include "repfield.h"
#include "reptable.h"
#include "repengin.h"
#include "procdata.h"

// resources
#include "inputmng.hjson" 


//includere come ultimo include all'inizio del cpp
#include "begincpp.dex"

#ifdef _DEBUG
#undef THIS_FILE
static const char BASED_CODE THIS_FILE[] = __FILE__;
#endif

//============================================================================
#define LEFT_INNER_BORDER     5
#define RIGHT_INNER_BORDER    5
#define TOP_INNER_BORDER      5
#define BOTTOM_INNER_BORDER   5
#define INTER_ENTRY_GAP       3
#define INTER_LINE_GAP        3
#define INTER_CAPTION_GAP     1

#define MAGIC_DELTA_WIDTH	  20
#define MAGIC_DELTA_HEIGHT	  50

//class CAskView;
//class CAskFrame;

//============================================================================
//	AskEntry implementation
//============================================================================

//----------------------------------------------------------------------------
AskEntry::AskEntry(AskGroup* pOwner, AskFieldData* pAskFieldData)
	:
	m_pOwnerAskGroup	(pOwner),
	m_pData				(NULL),
	m_pStatic			(NULL),
	m_pCtrl				(NULL),
	m_nCtlId			(0),
	m_nLen				(10),
	m_nPrec				(0),
	m_nRows				(1),
	m_pInitExpr			(NULL),
	m_bHidden			(FALSE),
	//m_bHide				(FALSE),
	m_nMinWidth			(0),
	m_nMinHeight		(0),
	m_nMinAlign			(0),
	m_nCaptionWidth		(0),
	m_nCaptionHeight	(0),
	m_nFieldWidth		(0),
	m_nFieldHeight		(0),
	m_nAlign			(0),
	m_pAskFieldData		(pAskFieldData)
{
	m_bOwnsAskFieldData = (pAskFieldData == NULL);
	if (m_bOwnsAskFieldData)
		m_pAskFieldData = new AskFieldData(pOwner->m_pAskGroupData);
}

//----------------------------------------------------------------------------
AskEntry::~AskEntry ()
{
	if(m_bOwnsAskFieldData)
		SAFE_DELETE(m_pAskFieldData);

	if (m_pStatic)
		SAFE_DELETE(m_pStatic);

	if (m_pCtrl)
		SAFE_DELETE(m_pCtrl);
}

//----------------------------------------------------------------------------
void AskEntry::SetEntrySize(int len, int prec /*= 0*/)
{
	ASSERT(m_pData);

	m_nLen = len;

	if (m_nLen < 0) return;

	if (m_pData->GetDataType() == DATA_BOOL_TYPE) return;

	m_nPrec = prec;
	m_nRows = prec;		//@@TODO
}

//----------------------------------------------------------------------------
BOOL AskEntry::AllocItems(CWnd* pParent, int& CtlId, BOOL bGroup, CBaseDocument* pDocument)
{
	//prj. 6258 
	SAFE_DELETE(m_pCtrl);

	//if (m_bHidden || m_nLen < 0)	
	if (m_nLen < 0)
	{
		m_bHidden = TRUE;
		m_nLen = 1;
		//return TRUE; //probabile vecchia implementazione undocumented simile a quella legata al campo m_bHidden
	}

	DWORD style = 0;

	OnPresetStyle(style);

	style |= WS_CHILD | WS_TABSTOP;
	if (bGroup)
		style |= WS_GROUP;

	if (m_pAskFieldData->m_pCaptionExpr)
	{
		DataStr ds;
		if (!m_pAskFieldData->m_pCaptionExpr->Eval(ds))
		{
			CString mess = _TB("Error evaluating show/hide field expression:") + GetPublicName() + _T(" - ") + m_pAskFieldData->m_pWhenExpr->GetErrDescription();
			m_pOwnerAskGroup->m_pOwnerInputMng->Message(mess);
		}
		else
			m_pAskFieldData->m_strCaption = m_strLocalizedCaption = ds.GetString();
	}

	// di default l'input del testo NON viene limitato
	int nLimitText = 0;

	// alloco subito l'hotlink perchè così posso testarne le funzionalità al runtime
	HotKeyLinkObj* pHL = NULL;
	BOOL bCanBeCombo = FALSE;
	if (m_pAskFieldData->m_nsHotLink.IsValid())
	{
		CRuntimeClass* pControlClass = NULL;
		pHL = AfxGetTbCmdManager()->RunHotlink(m_pAskFieldData->m_nsHotLink, NULL, &pControlClass);
		if (!pHL)
		{
			m_pOwnerAskGroup->m_pOwnerInputMng->Message(_TB("Hotlink used not found for:" + GetPublicName()));
		}

		if (pHL)
			pHL->InitNamespace();

		bCanBeCombo = (!pHL || pHL->IsFillListBoxEnabled()) && m_pAskFieldData->m_CtrlStyle != AskFieldData::EDIT_HKL;
		
		if (m_pAskFieldData->m_bMultiSelectionCombo && bCanBeCombo)
		{
			if (pControlClass && pControlClass->IsDerivedFrom(RUNTIME_CLASS(CMSStrCombo)))
				m_pCtrl = ::GetParsedCtrl(pControlClass->CreateObject());
			else
			{
				CMSStrCombo* pCbx = new CMSStrCombo;
				m_pCtrl = ::GetParsedCtrl(pCbx);

				if (pControlClass && pControlClass->IsDerivedFrom(RUNTIME_CLASS(CLongIDCombo)))
					pCbx->SetDataTypeKey(DataType::Long);
			}

			dynamic_cast<CMSStrCombo*>(m_pCtrl->GetCtrlCWnd())->SetDecodeListKey();
		}	
		else if (pControlClass)
		{
			CObject* pCtrl = pControlClass->CreateObject();
			m_pCtrl = ::GetParsedCtrl(pCtrl);
		}
	}
	
	if (m_pCtrl == NULL) switch (m_pData->GetDataType().m_wType)
	{
		case DATA_STR_TYPE  :
		case DATA_TXT_TYPE  :
		{
			if (bCanBeCombo)
			{
				m_pCtrl = new CStrCombo;
			}
			else
			{
				m_pCtrl = new CStrEdit;

				// m_nRows > 1 indica che l'input del testo non deve essere multilinea
				if (m_nRows > 1)
					style |= ES_AUTOVSCROLL | ES_WANTRETURN | ES_MULTILINE;
			}

			// m_nRows = 0 indica che l'input del testo deve essere limitato
			//if (m_nRows == 0) 
				nLimitText = m_nLen;// il testo viene limitato alla larghezza del control

			break;
		}
		case DATA_INT_TYPE  :
		{
			if (bCanBeCombo)
				m_pCtrl = new CIntCombo;
			else
			{
				m_pCtrl = new CIntEdit;
				m_nRows = 1;
			}

			break;
		}
		case DATA_LNG_TYPE  :
		{
			if (m_pData->IsATime())		//@@ElapsedTime
			{
				m_pCtrl = new CElapsedTimeEdit;
			}
			else if (bCanBeCombo)
			{
				m_pCtrl = new CLongCombo;
			}
			else
			{
				m_pCtrl = new CLongEdit;
				m_nRows = 1;
			}

			break;
		}
		case DATA_DBL_TYPE  :
		{
			m_pCtrl = bCanBeCombo ? (CParsedCtrl*) new CDoubleCombo : (CParsedCtrl*) new CDoubleEdit;
			m_nRows = 1;

			break;
		}
		case DATA_MON_TYPE  :
		{
			m_pCtrl = bCanBeCombo ? (CParsedCtrl*) new CMoneyCombo : (CParsedCtrl*) new CMoneyEdit;
			m_nRows = 1;

			break;
		}
		case DATA_QTA_TYPE  :
		{
			m_pCtrl = bCanBeCombo ? (CParsedCtrl*) new CQuantityCombo : (CParsedCtrl*) new CQuantityEdit;
			m_nRows = 1;

			break;
		}
		case DATA_PERC_TYPE  :
		{
			m_pCtrl = bCanBeCombo ? (CParsedCtrl*) new CPercCombo : (CParsedCtrl*) new CPercEdit;
			m_nRows = 1;

			break;
		}
		case DATA_DATE_TYPE :
		{
			if (m_pData->IsFullDate())
			{
				if (m_pData->IsATime())
					m_pCtrl = new CTimeEdit;
				else
				{
					m_pCtrl = new CDateTimeEdit;
					m_pCtrl->Attach (BTN_CALENDAR_ID);
				}

				m_nRows = 1;
			}
			else
				if (bCanBeCombo)
				{
					m_pCtrl = new CDateCombo;
				}
				else
				{
					m_pCtrl = new CDateEdit;
					m_pCtrl->Attach (BTN_CALENDAR_ID);
					m_nRows = 1;
				}

			break;
		}
		case DATA_BOOL_TYPE :
		{
			switch (m_pAskFieldData->m_CtrlStyle)
			{
				case AskFieldData::CHECK_BOX	:
				case AskFieldData::RADIO_BUTTON:
				{
					m_pCtrl = new CBoolButton;
					style |= (m_pAskFieldData->m_CtrlStyle == AskFieldData::CHECK_BOX ? BS_AUTOCHECKBOX : BS_AUTORADIOBUTTON);
					if (m_pAskFieldData->m_bLeftTextBool)
						style |= BS_RIGHTBUTTON | BS_RIGHT;

					m_nLen = m_strLocalizedCaption.GetLength();	// to get size of button + caption
					break;
				}
				case AskFieldData::EDIT	:
				{
					m_pCtrl = new CBoolEdit;
					m_nRows = 1;
					break;
				}
				default:	ASSERT(FALSE); return FALSE;
			}

			break;
		}
		case DATA_ENUM_TYPE :
		{
			m_pCtrl = new CEnumCombo;
			style |= CBS_DROPDOWNLIST | CBS_SORT | WS_VSCROLL;

			if (m_nRows <= 0)
			{
				const EnumItemArray*	pItems = AfxGetEnumsTable()->GetEnumItems(m_pData->GetDataType().m_wTag);
				if (pItems)
					m_nRows = pItems->GetSize();
			}

			break;
		}
		default			  :
			return FALSE;
	}
 
	if (m_nRows <= 0)
		m_nRows = 1;

	if (m_nRows > 1 && m_pCtrl->GetCtrlCWnd()->IsKindOf(RUNTIME_CLASS(CParsedCombo)))
		((CParsedCombo*) m_pCtrl)->SetMaxRows(m_nRows);

	if (pHL)
	{
		m_pCtrl->Attach(BTN_DEFAULT);
		m_pCtrl->AttachHotKeyLink(pHL, TRUE);
		pHL->EnableAddOnFly(FALSE);
		pHL->MustExistData(FALSE);
	}

	if (m_pCtrl->GetCtrlCWnd()->IsKindOf(RUNTIME_CLASS(CParsedCombo)))
	{
		style |= CBS_AUTOHSCROLL | CBS_DROPDOWN | WS_VSCROLL;

		if (pHL && pHL->IsKindOf(RUNTIME_CLASS(XmlHotKeyLink)))
		{
			style |= CBS_SORT;
		}
	}

	if (m_pData->IsKindOf(RUNTIME_CLASS(DataDbl)) && m_nPrec)
		m_pCtrl->SetCtrlNumDec(m_nPrec);

	if (m_pCtrl->GetCtrlCWnd()->IsKindOf(RUNTIME_CLASS(CParsedEdit)))
		style |= ES_AUTOHSCROLL;

	m_pCtrl->Attach(m_pData);

	if (m_pAskFieldData->m_nInputLimit == T_LOWER_LIMIT)
		m_pCtrl->SetCtrlStyle(m_pCtrl->GetCtrlStyle() | CTRL_STYLE_SHOW_FIRST);
	else
		if (m_pAskFieldData->m_nInputLimit == T_UPPER_LIMIT)
			m_pCtrl->SetCtrlStyle(m_pCtrl->GetCtrlStyle() | CTRL_STYLE_SHOW_LAST);
		                     
	m_nCtlId = AfxGetTBResourcesMap()->GetTbResourceID(cwsprintf(_T("IDC_AE_%d"), CtlId++), TbControls);

	if (pHL)
		pHL->PreCreateOwnerCtrl(m_pCtrl, style);

	if (!m_pCtrl->Create(style, CRect(0,0,0,0), pParent, m_nCtlId))
		return FALSE;

	if (!m_strAutomaticExpression.IsEmpty())
		m_pCtrl->AssignAutomaticExpression(m_strAutomaticExpression);

	if	(
			m_pCtrl->GetCtrlCWnd()->IsKindOf(RUNTIME_CLASS(CEdit)) ||
			m_pCtrl->GetCtrlCWnd()->IsKindOf(RUNTIME_CLASS(CListBox))
		)
		::SetWindowLong(m_pCtrl->GetCtrlCWnd()->m_hWnd, GWL_EXSTYLE, WS_EX_CLIENTEDGE);

	//if (!m_strLocalizedCaption.IsEmpty())
		if (m_pData->GetDataType() != DATA_BOOL_TYPE || m_pAskFieldData->m_CtrlStyle == AskFieldData::EDIT)
		{
			SAFE_DELETE(m_pStatic);

			if (!m_pStatic)
				m_pStatic = new CStatic;
			
			if (!m_pStatic->Create(m_strLocalizedCaption, WS_CHILD | WS_VISIBLE, CRect(0,0,0,0), pParent))
				return FALSE;
		}
		else
			m_pCtrl->GetCtrlCWnd()->SetWindowText(m_strLocalizedCaption);
	
	if (nLimitText > 0)
		m_pCtrl->SetCtrlMaxLen(nLimitText, TRUE);
	
	if (pDocument)
		m_pCtrl->AttachDocument(pDocument);

	if (pHL)
		pHL->DoOnCreatedOwnerCtrl();

	m_pCtrl->ShowCtrl(m_bHidden ? SW_HIDE : SW_NORMAL);
	m_pCtrl->UpdateCtrlStatus();

	if (m_pStatic)
		m_pStatic->ShowWindow(m_bHidden ? SW_HIDE : SW_NORMAL);
	
	return TRUE;
}

//----------------------------------------------------------------------------
BOOL AskEntry::SetAutomaticExpression(const CString& strExpr)
{ 
	if (m_pCtrl) 
		m_pCtrl->AssignAutomaticExpression(strExpr); 
	return SetAutomaticExpression(strExpr, FALSE); 
}

//----------------------------------------------------------------------------
void AskEntry::SetFont(CFont* pFont, BOOL bRedraw)
{
	if (m_pStatic)
		m_pStatic->SetFont(pFont, bRedraw);
		
	if (m_pCtrl)
		m_pCtrl->SetCtrlFont(pFont, bRedraw);
}

//----------------------------------------------------------------------------
void AskEntry::SetFocus()
{
	if (m_pCtrl)
		m_pCtrl->SetCtrlFocus(TRUE);
}

//----------------------------------------------------------------------------
void AskEntry::EvalMinSize()
{
	if (!m_pCtrl) return;

	CSize	cs;
	CFont*	pFont = m_pCtrl->GetCtrlCWnd()->GetFont();

	CDC* pDC = m_pCtrl->GetCtrlCWnd()->GetDC();

	if (m_pData->GetDataType() != DATA_BOOL_TYPE || m_pAskFieldData->m_CtrlStyle == AskFieldData::EDIT)
	{
		if (m_pStatic)
		{
			cs = GetTextSize(pDC, m_strLocalizedCaption, pFont);
			//An. nel report FixedAssestsJournal - Prefix No. (Year) => (Year) veniva tagliato
			//GetTextSize da dim errata in alcuni casi (Open Sans tema di default)
			m_nCaptionWidth	 = cs.cx + (int)(0.1 * cs.cx);
			m_nCaptionHeight = cs.cy;

			// NB: CAMBIARE QUI SE SI VUOLE UN DEFAULT DIVERSO (T_TOP o T_LEFT)
			// add gap between caption and control
			if (m_pAskFieldData->m_nCaptionPos != T_TOP)
				m_nMinAlign = m_nCaptionWidth + GetTextSize(pDC, _T("M"), pFont).cx;
		}
	}

	if (m_pCtrl->GetCtrlCWnd()->IsKindOf(RUNTIME_CLASS(CDescriptionCombo)))
	{
		((CDescriptionCombo*)m_pCtrl->GetCtrlCWnd())->SetMaxItemsLen(m_nLen / m_nRows + 1);
	}

	cs = m_pCtrl->SetCtrlSize(m_nLen / m_nRows + 1, m_nRows, TRUE);

	if (m_pCtrl->GetCtrlCWnd()->IsKindOf(RUNTIME_CLASS(CParsedCombo)))
	{
		cs.cy = 0;
		if (m_pCtrl->GetCtrlCWnd()->m_hWnd)
		{
			CRect rect;
			m_pCtrl->GetCtrlCWnd()->GetClientRect(rect);
			cs.cy = rect.Height();
		}
		if (!cs.cy)
 			cs.cy = GetEditSize(pDC, pFont, 1, 1).cy + 2;
	}
 		
	if (m_pAskFieldData->m_nInputLimit == T_LOWER_LIMIT)
		m_nFieldWidth = max(cs.cx, GetTextSize(pDC, CParsedCtrl::Strings::FIRST(), pFont).cx);
	else if (m_pAskFieldData->m_nInputLimit == T_UPPER_LIMIT)
		m_nFieldWidth = max(cs.cx, GetTextSize(pDC, CParsedCtrl::Strings::LAST(), pFont).cx);
	else
		m_nFieldWidth = cs.cx;

	m_nFieldHeight = cs.cy;

	m_nMinWidth = m_nFieldWidth;

	if (m_pAskFieldData->m_nCaptionPos == T_TOP && (m_pData->GetDataType() != DATA_BOOL_TYPE || m_pAskFieldData->m_CtrlStyle == AskFieldData::EDIT))
	{
		m_nMinHeight = m_nCaptionHeight + m_nFieldHeight;
		if (m_pStatic) m_nMinHeight += INTER_CAPTION_GAP;
	}
	else
		m_nMinHeight = (m_nCaptionHeight > m_nFieldHeight) ? m_nCaptionHeight : m_nFieldHeight;

	m_pCtrl->GetCtrlCWnd()->ReleaseDC(pDC);
}

//----------------------------------------------------------------------------
void AskEntry::Layout(int Left, int Top, int, int)
{
	if (!m_pCtrl) 
		return;

	if (m_pStatic)
	{
		AdjustClientRect(*m_pStatic, m_nCaptionWidth, m_nCaptionHeight);

		if (m_pAskFieldData->m_nCaptionPos != T_TOP)
		{
			CSize	cs;
			CFont*	pFont = m_pCtrl->GetCtrlCWnd()->GetFont();

			CDC* pDC = m_pCtrl->GetCtrlCWnd()->GetDC();

			m_pStatic->ModifyStyle(0, ES_RIGHT);

			int leftLabel = m_nAlign + Left - 2 - m_nCaptionWidth;

			// Elsa disse : "La regola è che siano centrate"
			m_pStatic->SetWindowPos(NULL, leftLabel, Top + (m_nFieldHeight - m_nCaptionHeight) / 2, 0, 0, SWP_NOSIZE | SWP_NOZORDER);
		}
		else
			m_pStatic->SetWindowPos(NULL, Left, Top, 0, 0, SWP_NOSIZE | SWP_NOZORDER);
	}

	if	(
			m_pAskFieldData->m_nCaptionPos == T_TOP	&&
			(m_pData->GetDataType() != DATA_BOOL_TYPE || m_pAskFieldData->m_CtrlStyle == AskFieldData::EDIT)
		)
	{
		Top += m_nCaptionHeight;
		if (m_pStatic) Top += INTER_CAPTION_GAP;
	}
	else
		if	(
				m_pData->GetDataType() != DATA_BOOL_TYPE ||
				m_pAskFieldData->m_CtrlStyle == AskFieldData::EDIT ||
				!m_pAskFieldData->m_bLeftBoolAlign
			)
			Left += m_nAlign;

	m_pCtrl->SetCtrlPos(NULL, Left, Top , 0 , 0 , SWP_NOSIZE | SWP_NOZORDER);
}

//----------------------------------------------------------------------------
void AskEntry::QueryHiddenStatus()
{
	if (m_pAskFieldData->m_pWhenExpr && !m_pAskFieldData->m_bDynamicHidden)
	{
		DataBool b(TRUE);
		if (!m_pAskFieldData->m_pWhenExpr->Eval(b))
		{
			CString mess = _TB("Error evaluating show/hide field expression:") + GetPublicName() + _T(" - ") + m_pAskFieldData->m_pWhenExpr->GetErrDescription();
			m_pOwnerAskGroup->m_pOwnerInputMng->Message(mess);

			if (m_pOwnerAskGroup->m_pOwnerInputMng->GetDesignMode() == InputMng::DESIGN)
				b = TRUE;
		}

		m_bHidden = m_pOwnerAskGroup->m_pOwnerInputMng->GetDesignMode() == InputMng::RUNNING && !((BOOL)b);
	}
}

//----------------------------------------------------------------------------
void AskEntry::DoValueChanged(BOOL bSetAutomaticExpression, BOOL bInitData /*=TRUE*/, LPCTSTR pszName /*= NULL*/, CObArray* parCascadeChangedEntries /*= NULL*/, COutParamBag* pBag/* = NULL*/)
{
	//if (m_bHidden) return;
	if (m_pCtrl && bSetAutomaticExpression)
	{
		m_pCtrl->SetAutomaticExpression(m_strAutomaticExpression);
		return;
	}

	BOOL bSetStatusHide = FALSE;
	DataBool bHide = m_bHidden/*(FALSE)*/;
	if (!pszName || (m_pAskFieldData->m_pWhenExpr && m_pAskFieldData->m_pWhenExpr->HasMember(pszName)))
	{
		bSetStatusHide = TRUE;

		DataBool bShow = !m_bHidden/*(TRUE)*/;
		if (m_pAskFieldData->m_pWhenExpr && !m_pAskFieldData->m_pWhenExpr->Eval(bShow))
		{
			CString mess = GetPublicName() + _T(": ") + m_pAskFieldData->m_pWhenExpr->GetErrDescription();
			m_pOwnerAskGroup->m_pOwnerInputMng->Message(mess);

			if (m_pOwnerAskGroup->m_pOwnerInputMng->GetDesignMode() == InputMng::DESIGN)
				bShow = TRUE;
		}

		bHide = m_pOwnerAskGroup->m_pOwnerInputMng->GetDesignMode() == InputMng::RUNNING && !bShow;

		if ((BOOL) bHide)
			m_pData->SetValueChanged(FALSE);
	}

	if (!pszName || (m_pAskFieldData->m_pCaptionExpr && m_pAskFieldData->m_pCaptionExpr->HasMember(pszName)))
	{
		DataStr strNewCaption;
		if (m_pAskFieldData->m_pCaptionExpr)
			if (!m_pAskFieldData->m_pCaptionExpr->Eval(strNewCaption))
			{
				CString mess = GetPublicName() + _T(": ") + m_pAskFieldData->m_pCaptionExpr->GetErrDescription();
				m_pOwnerAskGroup->m_pOwnerInputMng->Message(mess);
			}
			else
			{
				m_pAskFieldData->m_strCaption = strNewCaption.GetString();
				if (m_pStatic)
				{
					m_pStatic->SetWindowText(m_pAskFieldData->m_strCaption);

					CFont* pF = m_pStatic->GetFont();
					CSize csPrev = ::GetTextSize(pF, m_strLocalizedCaption);
					CSize csNew = ::GetTextSize(pF, m_pAskFieldData->m_strCaption);
					int diff = csNew.cx - csPrev.cx;
					//if (diff > 0)
					{
						CRect rect; m_pCtrl->GetCtrlCWnd()->GetWindowRect(&rect);
						rect.left += diff; rect.right += diff;
						
						m_pCtrl->GetCtrlCWnd()->GetParent()->ScreenToClient(rect);
						m_pCtrl->GetCtrlCWnd()->MoveWindow(rect);

						m_pStatic->GetWindowRect(&rect);
						rect.right = rect.left + csNew.cx;
						m_pStatic->GetParent()->ScreenToClient(rect);
						m_pStatic->MoveWindow(&rect);
					}
				}
				else if (m_pCtrl)
				{
					m_pCtrl->GetCtrlCWnd()->SetWindowText(m_pAskFieldData->m_strCaption);
				}
				m_strLocalizedCaption = strNewCaption.GetString();
			}
	}

	BOOL bSetStatusReadOnly = FALSE;
	DataBool bReadOnly(FALSE);
	if (!pszName || (m_pAskFieldData->m_pReadOnlyExpr && m_pAskFieldData->m_pReadOnlyExpr->HasMember(pszName)))
	{
		bSetStatusReadOnly = TRUE;

		if (m_pAskFieldData->m_pReadOnlyExpr && !m_pAskFieldData->m_pReadOnlyExpr->Eval(bReadOnly))
		{
			CString mess = GetPublicName() + _T(": ") + m_pAskFieldData->m_pReadOnlyExpr->GetErrDescription();
			m_pOwnerAskGroup->m_pOwnerInputMng->Message(mess);

			bReadOnly = FALSE;
		}

		if ((BOOL) bReadOnly)
			m_pData->SetValueChanged(FALSE);
	}

	if (!pszName || (m_pInitExpr && m_pInitExpr->HasMember(pszName)))
	{
		if (bInitData)
		{
			if (!pszName)
				m_pData->Clear();	//TODO GERMANO: PULISCE sempre CAMPO DI ASK
		
			if (m_pInitExpr)
			{
				if (
						!m_pData->IsValueChanged() ||
						(m_pAskFieldData->m_CtrlStyle == AskFieldData::RADIO_BUTTON && pszName)
					)
				{
					if (!m_pInitExpr->Eval(*m_pData))
					{
						CString mess = GetPublicName() + _T(": ") + m_pInitExpr->GetErrDescription();
						m_pOwnerAskGroup->m_pOwnerInputMng->Message(mess);

						if (m_pOwnerAskGroup->m_pOwnerInputMng->GetDesignMode() == InputMng::RUNNING)
							return;
					}

					if (pszName && m_pInitExpr->HasMember(pszName) && parCascadeChangedEntries)
					{
						parCascadeChangedEntries->Add(this);
					}
				}
			}
		}

		if (pBag) 
			pBag->m_arFieldsInitialized.Add(GetPublicName());
		if (!m_bHidden && m_pCtrl && m_pCtrl->GetCtrlCWnd()->GetParentOwner())
			m_pCtrl->UpdateCtrlView();
	}

	if (bSetStatusHide/* && !m_bHidden*/)
	{
		m_pData->SetHide((BOOL) bHide);

		if (m_pCtrl)
		{
			m_pCtrl->ShowCtrl((BOOL)bHide ? SW_HIDE : SW_NORMAL);
			if (m_pStatic)
				m_pStatic->ShowWindow((BOOL)bHide ? SW_HIDE : SW_NORMAL);

			m_pCtrl->UpdateCtrlStatus();
		}
	}
	
	if (bSetStatusReadOnly)
	{
		m_pData->SetReadOnly((BOOL) bReadOnly);

		if (m_pCtrl && m_pCtrl->GetCtrlCWnd()->GetParentOwner())
			m_pCtrl->UpdateCtrlStatus();
	}
}

//----------------------------------------------------------------------------
BOOL AskEntry::SetAutomaticExpression(const CString& strDataExpr, BOOL bSetValue)
{
	ASSERT(m_pData);

	if (bSetValue)
		return SetAutomaticExpressionAndData(strDataExpr, m_strAutomaticExpression, m_pData, TRUE);

	m_strAutomaticExpression = strDataExpr;
	return TRUE;
}

//----------------------------------------------------------------------------
void AskEntry::SetupQueryComponentsEdit(LPCTSTR pszName)
{
	if (!m_pCtrl) return;

	if (m_pAskFieldData->m_nsHotLink.IsValid())
	{
		TRACE(L"SetupQueryComponentsEdit(%s) called on %s\n", pszName, this->GetPublicName());
		BOOL bValued = FALSE;
		for (int i = 0; i <= m_pAskFieldData->m_HotLinkParamsExpr.GetUpperBound(); i++)
		{
			Expression* pParamExpr = (Expression*) m_pAskFieldData->m_HotLinkParamsExpr[i];
			if (!pParamExpr) continue;	// parametro opzionale

			if (pszName && !pParamExpr->HasMember(pszName))
				continue;

			if (!pParamExpr->Eval(*(m_HLComponentParamsData[i])))
			{
				CString mess = GetPublicName() + cwsprintf(_TB("(parameter {0-%d} HotLink): "), i + 1) + pParamExpr->GetErrDescription();
				m_pOwnerAskGroup->m_pOwnerInputMng->Message(mess);
			}
			else
				bValued = TRUE;
		}

		if (bValued && !m_pCtrl->GetHotLink()->DispatchCustomize(m_HLComponentParamsData))
		{
			CString mess = GetPublicName() + _T(" : Personalizzazione HotLink fallita");
			m_pOwnerAskGroup->m_pOwnerInputMng->Message(mess);
		}
	}
}

//----------------------------------------------------------------------------
BOOL AskEntry::AddHotLinkParam()
{
	if (!m_pAskFieldData->m_nsHotLink.IsValid())
		return TRUE;

	AddOnModule* pAddOnMod = AfxGetAddOnModule(m_pAskFieldData->m_nsHotLink);
	if(!pAddOnMod)
		return FALSE;

	CFunctionDescription* pHKLFunction = pAddOnMod->m_XmlDescription.GetParamObjectInfo(m_pAskFieldData->m_nsHotLink);
	if (!pHKLFunction)
		return FALSE;

	for(int i = 0; i <= m_pAskFieldData->m_HotLinkParamsExpr.GetUpperBound(); i++)
	{
		DataType aDataType = pHKLFunction->GetParamDescription(i)->GetDataType();
		m_HLComponentParamsData.Add(m_pAskFieldData->m_HotLinkParamsExpr[i] ? DataObj::DataObjCreate(aDataType) : NULL);
	}

	return TRUE;
}

//----------------------------------------------------------------------------
BOOL AskEntry::GetEdit(BOOL bGetAutomaticExpression)
{
	ASSERT(m_pData);
	BOOL bSet = FALSE;
		
	if (m_pAskFieldData->m_nInputLimit == T_LOWER_LIMIT)
	{
		m_pData->SetLowerValue(m_nLen);
		bSet = TRUE;
	}
	else if (m_pAskFieldData->m_nInputLimit == T_UPPER_LIMIT)
	{
		m_pData->SetUpperValue(m_nLen);
		bSet = TRUE;
	}

	if (m_pCtrl && bGetAutomaticExpression)
	{
		m_strAutomaticExpression = m_pCtrl->GetAutomaticExpression();
		bSet = TRUE;
	}
	
	if (m_pCtrl && m_pCtrl->GetCtrlCWnd() && m_pCtrl->GetCtrlCWnd()->IsKindOf(RUNTIME_CLASS(CMSStrCombo)))
	{
		CMSStrCombo* pMSCbx = (CMSStrCombo*) m_pCtrl->GetCtrlCWnd();

		CString sElements = pMSCbx->GetSqlNativeValues();

		m_pData->Assign(sElements);
		bSet = TRUE;
	}

	return bSet;
}

//----------------------------------------------------------------------------
void AskEntry::GetXmlParameters(CExportDataNetAskObject & entry)
{
	ASSERT_VALID(m_pOwnerAskGroup->m_pOwnerInputMng->m_pSymTable);
	if (!m_pOwnerAskGroup->m_pOwnerInputMng->m_pSymTable)
		return;

	//CXMLNode* pNode = pParentNode->CreateNewChild(GetPublicName());

	WoormField* pField = m_pOwnerAskGroup->m_pOwnerInputMng->m_pSymTable->GetField(GetPublicName());
	ASSERT(pField);
	if (!pField)
		return;

	entry.m_Names = GetPublicName();

	CString strValue = pField->GetData()->FormatDataForXML(pField->GetData()->GetDataType() != DATA_DATE_TYPE);
	entry.m_Value = strValue;

	entry.m_Type = FromTBTypeToNetType(pField->GetData()->GetDataType());

	entry.m_Length = cwsprintf(L"%d", pField->GetLen());

	CString sCtrl;
	switch (m_pAskFieldData->m_CtrlStyle)
	{
	case AskFieldData::CtrlStyle::RADIO_BUTTON:
		sCtrl = L"Radio";
		break;
	case AskFieldData::CtrlStyle::EDIT:
	case AskFieldData::CtrlStyle::EDIT_HKL:
		sCtrl = L"Text";
		break;
	case AskFieldData::CtrlStyle::CHECK_BOX:
		sCtrl = L"Check";
		break;
	case AskFieldData::CtrlStyle::COMBO_BOX:
		sCtrl = L"Combo";
		break;
	}
	entry.m_ControlType = sCtrl;

	if (m_pAskFieldData->m_pCaptionExpr)
	{
		entry.m_Title = m_pAskFieldData->m_pCaptionExpr->ToString();
	}
	else
		entry.m_Title = m_pAskFieldData->m_strCaption;

	//if (m_pReadOnlyExpr)
	//{
	//	pNode->SetAttribute(_T("readonly"), m_pReadOnlyExpr->ToString(););
	//}
}

//============================================================================
//	AskGroup implementation
//============================================================================

//----------------------------------------------------------------------------
AskGroup::AskGroup(InputMng* pOwner, AskGroupData* pAskGroupData)
	:
	m_pOwnerInputMng			(pOwner),
	m_pGroupBox					(NULL),
	m_bHidden					(FALSE),
	m_pAskGroupData				(pAskGroupData),
	m_bHiddenTitleGroupBoxAlways(FALSE)
{
	m_bOWnsAskGroupData = (pAskGroupData == NULL);
	if (m_bOWnsAskGroupData)
		m_pAskGroupData = new AskGroupData(*(m_pOwnerInputMng->m_pSymTable), m_pOwnerInputMng->m_pDocument);

	m_TileDialogSize = TILE_STANDARD;
}

//----------------------------------------------------------------------------
AskGroup::~AskGroup()
{
	if(m_bOWnsAskGroupData)
		SAFE_DELETE(m_pAskGroupData);

	SAFE_DELETE(m_pGroupBox);
}

//----------------------------------------------------------------------------
void AskGroup::QueryHiddenStatus()
{
	if (m_pAskGroupData->m_pWhenExpr)
	{
		DataBool b(FALSE);
		if (!m_pAskGroupData->m_pWhenExpr->Eval(b))
		{
			CString mess = _TB("Error evaluating show/hide expression applied to some fields in ask rules:") + m_strLocalizedCaption + _T(" - ") + m_pAskGroupData->m_pWhenExpr->GetErrDescription();
			m_pOwnerInputMng->Message(mess);

			if (m_pOwnerInputMng->GetDesignMode() == InputMng::DESIGN)
				b = TRUE;
		}

		m_bHidden = m_pOwnerInputMng->GetDesignMode() == InputMng::RUNNING && !((BOOL)b);
	}
}

//----------------------------------------------------------------------------
BOOL AskGroup::AllocItems(CWnd* pParent, int& nCtlId, CBaseDocument* pDocument)
{                                               
	if (m_Entries.GetSize() == 0) return FALSE;
	
	SAFE_DELETE(m_pGroupBox);

	m_pGroupBox = new CButton;

	CButton* pCB = (CButton*)m_pGroupBox;
	pCB->Create(_T(""), WS_CHILD | BS_GROUPBOX, CRect(0,0,0,0), pParent, DUMMY_ID);

	QueryHiddenStatus();

	AskEntry* pAE;
	BOOL bOneEntryVisible = FALSE;
	BOOL bGroupAssigned = FALSE;
	for (int i = 0; i <= m_Entries.GetUpperBound(); i++)
	{
		pAE = (AskEntry*)m_Entries[i];

		pAE->m_strLocalizedCaption = AfxLoadReportString(pAE->m_pAskFieldData->m_strCaption, pDocument);		

		//se il group deve sparire allora forzo la sparizione anche di tutte le entry
		if (m_bHidden) 
			pAE->m_bHidden = m_bHidden;
		else
			pAE->QueryHiddenStatus();

		if (!pAE->m_bHidden)
			bOneEntryVisible = TRUE;

		BOOL bGroup = FALSE;
		if 
			(
				!bGroupAssigned &&
				pAE->m_pData &&
				pAE->m_pData->GetDataType() == DataType::Bool &&
				pAE->m_pAskFieldData->m_CtrlStyle == AskFieldData::RADIO_BUTTON
			)
		{
			bGroupAssigned = bGroup = TRUE;
		}

		BOOL bResult = pAE->AllocItems(pParent, nCtlId, bGroup, pDocument);
		
		if (!bResult)
			return FALSE;
	}              

	if (!bOneEntryVisible)
		m_bHidden = TRUE;

	return TRUE;
}

//----------------------------------------------------------------------------
BOOL AskGroup::IsOneBoolCltrAligned () const
{
	AskEntry* pAE;

	for (int i = 0; i <= m_Entries.GetUpperBound(); i++)
	{
		pAE = (AskEntry*)m_Entries[i];
		if (
				pAE->m_pCtrl && 
				pAE->m_pCtrl->GetCtrlCWnd()->IsKindOf(RUNTIME_CLASS(CBoolButton)) &&
				!pAE->m_pAskFieldData->m_bLeftBoolAlign
			)
			return TRUE;
	}

	return FALSE;
}

//----------------------------------------------------------------------------
void AskGroup::SetFont(CFont* pFont, BOOL bRedraw)
{
	m_pGroupBox->SetFont(pFont, bRedraw);
	
	AskEntry* pAE;

	for (int i = 0; i <= m_Entries.GetUpperBound(); i++)
	{
		pAE = (AskEntry*)m_Entries[i];
		pAE->SetFont(pFont, bRedraw);
	}                                                      
}

//----------------------------------------------------------------------------
BOOL AskGroup::SetFocus()
{
	for (int i = 0; i < m_Entries.GetSize(); i++)
	{
		AskEntry* pAE = (AskEntry*)m_Entries[i];
		if (pAE && !pAE->m_bHidden) 
		{
			pAE->SetFocus();
			return TRUE;
		}
	}
	return FALSE;
}

//----------------------------------------------------------------------------
void AskGroup::ShowWindows()
{
	if (m_pAskGroupData->m_bHiddenTitle || m_bHidden)
		return;

	AskEntry* pAE;
	for (int i = 0; i <= m_Entries.GetUpperBound(); i++)
	{
		pAE = (AskEntry*)m_Entries[i];
		if (!pAE->m_bHidden)
		{
			m_pGroupBox->ShowWindow(SW_SHOW);
			return;
		}
	}                                                      
}

//----------------------------------------------------------------------------
void AskGroup::DoValueChanged(BOOL bSetAutomaticExpression, BOOL bInitData /*=TRUE*/, COutParamBag* pBag /*=NULL*/)
{
	for (int i = 0; i <= m_Entries.GetUpperBound(); i++)
	{
		AskEntry* pAE = (AskEntry*) m_Entries[i];

		BOOL bInitDataEntry = bInitData;
		if (bInitData)
		{
			bInitDataEntry = (pBag == NULL || pBag->m_parStaticFields == NULL || pBag->m_parStaticFields->GetFromName(pAE->GetPublicName()) == NULL);
			if (!bInitDataEntry && pBag && pBag->m_pSymTable)
			{
				WoormField* pF = pBag->m_pSymTable->GetField(pAE->GetPublicName());
				if (pF && pF->IsReInit())
				{
					bInitDataEntry = TRUE;
				}
			}
		}
		//---- skip dei campi valorizzati da WoormInfo
		if (bInitDataEntry && pBag) 
			for (int j = 0; j < pBag->m_arInField.GetSize(); j++)
			{
				WoormField* pF = pBag->m_arInField.GetAt(j);
				if (pF && pAE->GetPublicName().CompareNoCase(pF->GetName()) == 0)
				{
					bInitDataEntry = FALSE;
					break;
				}
			}

		pAE->DoValueChanged(bSetAutomaticExpression, bInitDataEntry, NULL, NULL, pBag);
	}
}

//----------------------------------------------------------------------------
void AskGroup::SetupQueryComponentsEdits()
{
	for (int i = 0; i <= m_Entries.GetUpperBound(); i++)
		((AskEntry*)m_Entries[i])->SetupQueryComponentsEdit();
}

//----------------------------------------------------------------------------
BOOL AskGroup::GetEdits(BOOL bGetAutomaticExpression)
{
	for (int i = 0 ; i <= m_Entries.GetUpperBound(); i++)
		((AskEntry*)m_Entries[i])->GetEdit(bGetAutomaticExpression);

	return TRUE;
}

//----------------------------------------------------------------------------
void AskGroup::EvalMinSize()
{
	AskEntry* pAE;
	int i;

	int totalEntriesHeight = 0;

	m_nMinAlign = 0;
	m_nMinWidth = 0;
	m_nCaptionHeight = 0;
    
	for (i = 0; i <= m_Entries.GetUpperBound(); i++)
	{
		pAE = (AskEntry*)m_Entries[i];
		if (!pAE->m_pCtrl) continue;

		pAE->EvalMinSize();

		if (m_nMinAlign < pAE->m_nMinAlign)
			m_nMinAlign = pAE->m_nMinAlign;

		totalEntriesHeight += (pAE->m_nMinHeight + INTER_ENTRY_GAP);
	}
    
	totalEntriesHeight += TOP_INNER_BORDER + BOTTOM_INNER_BORDER - INTER_ENTRY_GAP;

	for (i = 0; i <= m_Entries.GetUpperBound(); i++)
	{
		pAE = (AskEntry*)m_Entries[i];
		if (!pAE->m_pCtrl) continue;

		if (pAE->m_nMinAlign > 0 || !pAE->m_pAskFieldData->m_bLeftBoolAlign)
		{
			if (m_nMinWidth < pAE->m_nMinWidth)
				m_nMinWidth = pAE->m_nMinWidth;
		}
		else
			if (m_nMinWidth + m_nMinAlign < pAE->m_nMinWidth)
				m_nMinWidth = pAE->m_nMinWidth - m_nMinAlign;
	}
    
	if (!m_bHiddenTitleGroupBoxAlways && !m_pAskGroupData->m_bHiddenTitle)
	{
		m_pGroupBox->SetWindowText(m_strLocalizedCaption);
		
		// + "M" to draw correctly the upper right corner
		CDC* pDC = m_pGroupBox->GetDC();
		CSize cs = GetTextSize(pDC, m_strLocalizedCaption + _T("M"), m_pGroupBox->GetFont());
		m_pGroupBox->ReleaseDC(pDC);

		m_nCaptionHeight = cs.cy;
		if (m_strLocalizedCaption.IsEmpty())
			m_nCaptionHeight /= 2;

		totalEntriesHeight += m_nCaptionHeight;

		if (m_nMinWidth + m_nMinAlign < cs.cx)
			m_nMinWidth = cs.cx - m_nMinAlign;
	}

	m_nMinWidth += LEFT_INNER_BORDER + RIGHT_INNER_BORDER;
	m_nMinHeight = totalEntriesHeight;
}

//----------------------------------------------------------------------------
void AskGroup::Layout(int Left, int Top, int Width, int Height)
{
	if (!m_bHiddenTitleGroupBoxAlways && !m_pAskGroupData->m_bHiddenTitle)
	{
		AdjustClientRect(*m_pGroupBox, Width, Height);
		m_pGroupBox->SetWindowPos(NULL, Left, Top, 0, 0, SWP_NOSIZE | SWP_NOZORDER);
	}

	Top += TOP_INNER_BORDER;
	for (int i = 0; i <= m_Entries.GetUpperBound(); i++)
	{
		AskEntry* pAE=(AskEntry*)m_Entries[i];
		if (!pAE->m_pCtrl) 
			continue;

		pAE->m_nAlign = m_nAlign;
		pAE->Layout (
						Left + LEFT_INNER_BORDER,
						Top + m_nCaptionHeight,
						Width - LEFT_INNER_BORDER - RIGHT_INNER_BORDER,
						pAE->m_nMinHeight
					);

		Top += INTER_LINE_GAP + pAE->m_nMinHeight;
	}
}

//----------------------------------------------------------------------------
AskEntry* AskGroup::AddEntry(AskFieldData* pAskFieldData, WoormField* pRepField)
{
	if (!pRepField)
		return NULL;

	AskEntry* pEntry = new AskEntry(this, pAskFieldData);

	Expression* pInitExpr = pRepField->GetInitExpression();
	if (pInitExpr && pInitExpr->IsEmpty())
		pInitExpr = NULL;

	pEntry->m_pData = pRepField->GetData();
	pEntry->m_pInitExpr = pInitExpr;

	if (pAskFieldData->m_bHiddenInput)
		pEntry->SetEntrySize(-1);
	else
	{
		pEntry->SetEntrySize(pRepField->GetLen(), pRepField->GetNumDec());

		if (!pEntry->AddHotLinkParam())
		{
			delete pEntry;
			return NULL;
		}
	}

	m_Entries.Add(pEntry);
	return pEntry;
}

//----------------------------------------------------------------------------
void AskGroup::GetXmlParameters(CExportDataNetAskDialogGroup& dataGroup, int nGroup)
{
	CString strName;
	strName.Format(_T("Group%d"), nGroup);

	dataGroup.m_Title = m_pAskGroupData->m_strTitle;
	dataGroup.m_Names = strName;

	for (int i = 0; i < this->m_Entries.GetSize(); i++)
	{
		AskEntry* pAskEntry = (AskEntry*)m_Entries[i];
		ASSERT_VALID(pAskEntry);

		CExportDataNetAskObject* entry = new CExportDataNetAskObject();
		pAskEntry->GetXmlParameters(*entry);
		dataGroup.m_arEntry.Add(entry);
	}
}

////////////////////////////////////////////////////////////////////////////////
//							CDynamicContainerTileGrp
//////////////////////////////////////////////////////////////////////////////////
IMPLEMENT_DYNCREATE(CDynamicContainerTileGrp, CTileGroup)

//----------------------------------------------------------------------------------
void CDynamicContainerTileGrp::Customize()
{
	SetLayoutType(CLayoutContainer::VBOX);

	CDynamicContainerDlg* pParent = dynamic_cast<CDynamicContainerDlg*>(GetParent());
	InputMng& inpMng = pParent->m_OwnInputMng;

	CTilePanel* pPanel = (CTilePanel*)AddPanel(_T("DynamicContainerTileGrp"), _T(""), CLayoutContainer::COLUMN);

	if (pPanel)
	{
		pParent->m_pTilePanel = pPanel;
		pPanel->ModifyTabberStyle(CBCGPTabWnd::STYLE_DOTS);

		pPanel->SetTileStyle(AfxGetTileDialogStyleNormal());
		pPanel->SetTileDialogStyle(AfxGetTileDialogStyleFilter());

		m_WideWidth		= CBaseTileDialog::GetTileWidth(TILE_WIDE);
		m_LargeWidth	= CBaseTileDialog::GetTileWidth(TILE_LARGE);
		m_StandardWidth = CBaseTileDialog::GetTileWidth(TILE_STANDARD);
		m_MiniWidth		= CBaseTileDialog::GetTileWidth(TILE_MINI);
		
		BOOL bFoundErr = FALSE;
		CAskDialogDataAndGroups* pAskDialogDataAndGroups = NULL;

		//gestisco la prima dialog visibile
		for (int i = inpMng.m_idxLastDialog; i < inpMng.m_AllAskDialogDataAndGroups.GetCount(); i++)
		{
			CAskDialogDataAndGroups* pADG = inpMng.m_AllAskDialogDataAndGroups.GetAt(i);

			if(!pAskDialogDataAndGroups) pAskDialogDataAndGroups = inpMng.m_AllAskDialogDataAndGroups.GetAt(i);

			inpMng.CreateAskDialog(pPanel, pADG, TRUE);
		}

		if (pPanel->GetTabsNum() < 1)
			return;

		int iTabActive = pAskDialogDataAndGroups->m_idxTab;
		CTilePanelTab* pTabActive = pPanel->GetTab(iTabActive);

		if (!pTabActive)
			return;

		pParent->m_nFirstActiveTab = iTabActive;
	}

}

//////////////////////////////////////////////////////////////////////////////
//					    CDynamicContainerTileDlg
//////////////////////////////////////////////////////////////////////////////
//
//=============================================================================
IMPLEMENT_DYNCREATE(CDynamicContainerTileDlg, CTileDialog)

BEGIN_MESSAGE_MAP(CDynamicContainerTileDlg, CTileDialog)
	ON_WM_ERASEBKGND()
END_MESSAGE_MAP()

//----------------------------------------------------------------------------
CDynamicContainerTileDlg::CDynamicContainerTileDlg()
	:
	CTileDialog(_NS_TILEDLG("TileDlg"), IDD_INPUT_DLG)
{
}

//------------------------------------------------------------------------------
void CDynamicContainerTileDlg::SetTileDlgName(CString sName)
{
	GetInfoOSL()->m_Namespace.SetObjectName(sName);
	m_sName = sName;
}

//----------------------------------------------------------------------------
BOOL CDynamicContainerTileDlg::OnEraseBkgnd(CDC* pDC)
{
	if (GetParentTileGroup() && GetParentTileGroup()->GetTileDialogs() && GetParentTileGroup()->GetTileDialogs()->GetSize() == 1)
		Sleep(20);

	return __super::OnEraseBkgnd(pDC);
}

/////////////////////////////////////////////////////////////////////////////////////////////
//						CDynamicContainerDlg
////////////////////////////////////////////////////////////////////////////////////////
//---------------------------------------------------------------------------------------------
IMPLEMENT_DYNAMIC(CDynamicContainerDlg, CParsedDialogWithTiles)

//-----------------------------------------------------------------------------------------------
BEGIN_MESSAGE_MAP(CDynamicContainerDlg, CParsedDialogWithTiles)
	//{{AFX_MSG_MAP(CDynamicContainerDlg )

	ON_UPDATE_COMMAND_UI(ID_GO_PREV, OnUpdateCommandGoPrev)
	ON_UPDATE_COMMAND_UI(ID_GO_NEXT, OnUpdateCommandGoNext)

	ON_UPDATE_COMMAND_UI(IDOK,			OnUpdateCommandOK)

	ON_COMMAND			(ID_GO_PREV, OnGoPrevClick)
	ON_COMMAND			(ID_GO_NEXT, OnGoNextClick)

	ON_WM_WINDOWPOSCHANGING	()
	ON_WM_MOVING()
	ON_MESSAGE(UM_GET_REPORT_NAMESPACE, OnGetReportNamespace)
	ON_MESSAGE(UM_GET_LOCALIZER_INFO, OnGetLocalizerInfo)
	ON_MESSAGE(UM_GET_CONTROL_DESCRIPTION, OnGetControlDescription)
	ON_REGISTERED_MESSAGE(BCGM_CHANGE_ACTIVE_TAB, OnChangeActiveTab)
	ON_REGISTERED_MESSAGE(BCGM_CHANGING_ACTIVE_TAB, OnChangingActiveTab)
	ON_WM_MOUSEWHEEL()
	//}}AFX_MSG_MAP
END_MESSAGE_MAP()

//----------------------------------------------------------------------------
CDynamicContainerDlg::CDynamicContainerDlg(InputMng& Mng, CWnd* pParent /*= NULL*/)
	:
	CParsedDialogWithTiles	(IDD_INPUT_DLG_WITH_TILES, pParent),
	m_OwnInputMng			(Mng),
	m_pTilePanel			(NULL),
	m_bFromNavigator		(FALSE),
	m_nFirstActiveTab		(-1),
	m_bCreated				(FALSE)
{
}

//------------------------------------------------------------------------------
void CDynamicContainerDlg::SetDefaultActionButton()
{
	m_pToolBar->SetDefaultAction(ID_GO_NEXT);

	m_pToolBar->AdjustLayout();
}

//--------------------------------------------------------------------------
LRESULT CDynamicContainerDlg::OnChangeActiveTab(WPARAM wParam, LPARAM lParam)
{
	if (!m_pTileGroup || !m_pTilePanel)
		return 0;

	//manage WindowText and Focus when tab changing 
	CTilePanelTab* pCrtTab = m_pTilePanel->GetActiveTab();

	if (!pCrtTab)
		return 0;

	SetWindowText(pCrtTab->GetTitle());

	int nPos = m_pTilePanel->GetTabIdx(pCrtTab);

	if (m_OwnInputMng.m_AllAskDialogDataAndGroups.GetCount() == 0)
		return 0;

	CAskDialogDataAndGroups* pAskDialogDataAndGroups = m_OwnInputMng.GetAskDialog(nPos);
	
	if (pAskDialogDataAndGroups)
		m_OwnInputMng.UpdateDynCtrlGrp(pAskDialogDataAndGroups);

	pCrtTab->Invalidate();

	CRect tRect;
	m_pToolBar->GetWindowRect(tRect);
	int BottomPartHeight = tRect.Height();

	CRect dRect;
	GetClientRect(dRect);
	dRect.bottom -= BottomPartHeight;

	m_pTilePanel->Relayout(dRect);

	return 0;
}

//--------------------------------------------------------------------------
LRESULT CDynamicContainerDlg::OnChangingActiveTab(WPARAM wParam, LPARAM lParam)
{
	if (!m_pTileGroup || !m_pTilePanel)
		return 0;

	int nNewTab = (int) wParam;

	if (!m_bFromNavigator)
	{
		if (!CheckForm())
			return 1;

		for(int i = m_pTilePanel->GetActiveTabNum(); i <= nNewTab - 1; i++)
			if (!DoGoNextClick(i))
				return 1;

		// la DoGoNextClick potrebbe aver cambiato la sequenza di tab esistenti dopo quella che 
		// si sta abbandonando
		if (nNewTab > m_pTilePanel->GetTabsNum() - 1)
		{
			//non raggiungo la tab desiderata ma rimango dove sono
			m_OwnInputMng.m_idxLastDialog = m_OwnInputMng.GetIdxAskDialog(m_pTilePanel->GetTabsNum() - 1);
			return 1; 
		}
		//se sono andato indietro di tab la 'DoGoNextClick' non è stata eseguita e non ha aggiornato m_idxLastDialog
		m_OwnInputMng.m_idxLastDialog = m_OwnInputMng.GetIdxAskDialog(nNewTab);
	}
	return 0;
}

//--------------------------------------------------------------------------
LRESULT CDynamicContainerDlg::OnGetReportNamespace(WPARAM wParam, LPARAM lParam)
{
	if (m_OwnInputMng.m_pDocument)
		return (LRESULT)m_OwnInputMng.m_pDocument->GetNamespace().ToString().AllocSysString();

	return NULL;
}

//-----------------------------------------------------------------------------
LRESULT CDynamicContainerDlg::OnGetControlDescription(WPARAM wParam, LPARAM lParam)
{
	CWndObjDescriptionContainer* pContainer = (CWndObjDescriptionContainer*)wParam;

	/*Il metodo GetWindowDescription crea da zero una descrizione(del tipo della runtimeclass passata) se non esisteva gia,
	o ripesca quella gia creata nei round trip precedenti se esisteva.
	Nella creazione assegna un id alla descrizione (m_strId), che servira' da chiave per recuperarle.
	Questo id viene creato in modo standard sulla base dell'handle di finestra.
	In alcuni casi pero finestre "differenti" hanno lo stesso id (es. parsedbitmap del bodyedit).
	In questi casi si puo' creare un ID disambiguo e passarlo al metodo GetWindowDescription.
	*/
	CString strId = (LPCTSTR)lParam;

	CWndDialogDescription* pDesc = (CWndDialogDescription*)pContainer->GetWindowDescription(this, RUNTIME_CLASS(CWndDialogDescription), strId);
	pDesc->m_bIsModal = TRUE;
	pDesc->UpdateAttributes(this);

	pDesc->AddChildWindows(this);
	return (LRESULT)pDesc;
}

//-----------------------------------------------------------------------------
LRESULT CDynamicContainerDlg::OnGetLocalizerInfo(WPARAM wParam, LPARAM lParam)
{
	return GetLocalizerInfo(wParam, lParam);
}

//----------------------------------------------------------------------------------------------
void CDynamicContainerDlg::CenterMe(CWnd* pWndParent)
{
	CRect aRectParent;
	pWndParent->GetWindowRect(aRectParent);

	int wScreen = aRectParent.Width();
	int hScreen = aRectParent.Height();

	CRect aRect;
	GetWindowRect(aRect);

	int x = aRectParent.left + wScreen / 2 - aRect.Width() / 2;
	int y = aRectParent.top + hScreen / 2 - m_OwnInputMng.m_MinDialogHeight / 2;

	SetWindowPos(NULL, x, y, 0, 0, SWP_NOSIZE | SWP_NOZORDER | SWP_NOACTIVATE);
}

//-------------------------------------------------------------------------------------------
void CDynamicContainerDlg::AskModal()
{
	// simulated DoModal in order to allow hotlink document 
	// browsing and other documents operations
	if (m_pParentWnd)
		m_pParentWnd->EnableWindow(FALSE); //verra' riabililtata dalla enddialog 

	if (!Create(IDD_INPUT_DLG_WITH_TILES, m_pParentWnd))
	{
		if (m_pParentWnd)
			m_pParentWnd->EnableWindow(TRUE); //se fallisce la creazione, riabilito la parent

		m_OwnInputMng.m_nPressedBtn = IDCANCEL;
		return;
	}

	// I insert ask dialog in Show/Hide feature when 
	// dockable tab manager changes tab

	CenterMe(GetOwner());

	m_bCreated = TRUE;

	CDockableFrame* pDockableFrame = dynamic_cast<CDockableFrame*>(m_pParentWnd);
	if (pDockableFrame)
		pDockableFrame->AddChild(m_hWnd);

	::BringWindowToTop(GetSafeHwnd());
	ShowWindow(SW_SHOW);

	//metto il thread in modal state
	CPushMessageLoopDepthMng __pushLoopDepth(MODAL_STATE);
	AfxGetThreadContext()->RaiseCallBreakEvent();

	//aspetto che venga premuto un bottone
	m_OwnInputMng.m_nPressedBtn = -1;
	m_OwnInputMng.m_pEngineScheduler->LoopUntil(m_OwnInputMng.m_nPressedBtn, FALSE, m_pToolBar);

	if (pDockableFrame)
		pDockableFrame->RemoveChild(m_hWnd);
}

//---------------------------------------------------------------------------------------------------
void CDynamicContainerDlg::OnGoPrevClick()
{
	if (!CheckForm())
		return;

	int nPos = m_pTilePanel->GetActiveTabNum();

	if (nPos == 0)
		return;

	nPos--;

	CTilePanelTab* pTab = m_pTilePanel->GetTab(nPos);

	if (nPos < 0 || !pTab)
		return;

	SetWindowText(pTab->GetTitle());

	m_pTilePanel->SetActiveTab(nPos);
	m_OwnInputMng.SetFocusFirst(nPos);

	pTab->Invalidate();

}

//-------------------------------------------------------------------------------------------------------
CAskDialogDataAndGroups* CDynamicContainerDlg::GetNextAskDialog(BOOL& bFoundErrors)
{
	int currentDlg = m_OwnInputMng.m_idxLastDialog;

	// se sto passando solo da un tab all'altro (m_bFromNavigator = FALSE) non rieseguo gli eventi di after, on e abort (CanDoNextAskDialog)
	if (currentDlg < 0 || (m_bFromNavigator && !m_OwnInputMng.CanDoNextAskDialog(currentDlg)))
	{
		bFoundErrors = TRUE;
		if (m_OwnInputMng.GetDesignMode() == InputMng::RUNNING)
			return NULL;
	}

	CAskDialogDataAndGroups* pAskDialogDataAndGroups = m_OwnInputMng.GetNextAskDialog(m_bFromNavigator, bFoundErrors);

	if (bFoundErrors && m_OwnInputMng.GetDesignMode() == InputMng::RUNNING)
		return NULL;
	
	SetDefaultActionButton();

	return pAskDialogDataAndGroups;
}

//-----------------------------------------------------------------------------------------------------
void CDynamicContainerDlg::OnGoNextClick()
{
	BOOL bResult = DoGoNextClick();
	if(!bResult)
	{ 
		m_OwnInputMng.ReadAllEdits(this);
		m_OwnInputMng.m_nPressedBtn = IDOK;
		EndDialog(IDOK);
	}

}

//-----------------------------------------------------------------------------------------------------
BOOL CDynamicContainerDlg::DoGoNextClick( int idTab /*=-1*/)
{	
	if (idTab < 0)
	{		
		if (!CheckForm())
			return TRUE;
		//non sto navigando fra le tab ma uso i pulsanti next/ok
		m_bFromNavigator = TRUE;
		//distruggo le eventuali tab già create
		ManageAfterValueChanged();
	}

	BOOL bFoundErrors = FALSE;
	
	CAskDialogDataAndGroups* pAskDialogDataAndGroups = GetNextAskDialog(bFoundErrors);

	if (bFoundErrors)
	{
		//Non ho ancora settato i parametri d'errore , me ne preoccupo in uscita nella ExecAllAsk
		//m_OwnInputMng.ShowMessages();

		if (m_OwnInputMng.GetDesignMode() == InputMng::RUNNING)
		{
			m_bFromNavigator = FALSE;
			return idTab == -2;
		}
	}

	if (!pAskDialogDataAndGroups)     
	{
		m_bFromNavigator = FALSE;
		return FALSE;
	}

	if (idTab >= 0)
	{
		//stavo navigando fra tab -> non devo creare la tab da visualizzare: esiste già
		return TRUE;
	}

	//creazione della tab solo se il tab corrispondente non esiste
	if (pAskDialogDataAndGroups->m_idxTab < 0)
	{
		// preinizializzo la dialog con i dati (eventuali) forniti dal controllore esterno
		//if (m_OwnInputMng.GetDesignMode() == InputMng::RUNNING && dynamic_cast<CWoormDoc*>(m_OwnInputMng.m_pDocument))
		//	m_OwnInputMng.SyncExternalControllerInfo(FALSE, pAskDialogDataAndGroups);

		m_OwnInputMng.CreateAskDialog(m_pTilePanel, pAskDialogDataAndGroups, TRUE);

		CRect aRect;
		GetClientRect(aRect);
		int nWidth = aRect.Width();
		SetWindowPos(NULL, 0, 0, nWidth + MAGIC_DELTA_WIDTH, GetDialogMinHeight(), SWP_NOMOVE | SWP_NOZORDER);
		
	}

	if (pAskDialogDataAndGroups->m_idxTab > -1)
	{
		m_pTilePanel->SetActiveTab(pAskDialogDataAndGroups->m_idxTab);
		m_OwnInputMng.SetFocusFirst(pAskDialogDataAndGroups->m_idxTab);
		
		CTilePanelTab* pCrtTab = m_pTilePanel->GetTab(pAskDialogDataAndGroups->m_idxTab);
		if (pCrtTab)
			pCrtTab->Invalidate();
	}
	
	m_bFromNavigator = FALSE;

	return pAskDialogDataAndGroups->m_idxTab > -1;
}

//--------------------------------------------------------------------------------------------------
void CDynamicContainerDlg::OnUpdateCommandGoPrev(CCmdUI* pCmdUI)
{
	pCmdUI->Enable(m_pTilePanel && m_pTilePanel->GetActiveTabNum() > 0);
}

//------------------------------------------------------------------------------------------------------
void CDynamicContainerDlg::OnUpdateCommandGoNext(CCmdUI* pCmdUI)
{
//	disabilitata gestione del OK Germano & Corina 25/7/2016
	pCmdUI->Enable(m_pTilePanel != NULL);// && m_nNextAskDialogTab != -1);
}

//------------------------------------------------------------------------------------------------------
void CDynamicContainerDlg::OnUpdateCommandOK(CCmdUI* pCmdUI)
{
//	disabilitata gestione del OK Germano & Corina 25/7/2016
	pCmdUI->Enable(TRUE);//m_nNextAskDialogTab == -1);
}

//---------------------------------------------------------------------------------
void CDynamicContainerDlg::OnCustomizeToolbar()
{
	CString sOK			= _TB("OK");
	CString sCancel		= _TB("Cancel");
	CString sGoPrev		= _TB("Previous");
	CString sGoNext		= _TB("Next");
	
	m_pToolBar->SetAutoHideToolBarButton(TRUE);

	if (m_OwnInputMng.m_AllAskDialogDataAndGroups.GetCount() > 1)
	{
		/*m_pToolBar->AddButton
		(
			ID_GO_PREV,
			_T("goprev"),
			TBIcon(szIconPrev, TOOLBAR),
			sGoPrev,
			_T("")
		);

		m_pToolBar->AddButton
		(
			ID_GO_NEXT,
			_T("gonext"),
			TBIcon(szIconNext, TOOLBAR),
			sGoNext,
			_T("")
		);*/
		m_pToolBar->AddButtonToRight
		(
			ID_GO_PREV,
			_T("goprev"),
			TBIcon(szIconPrev, TOOLBAR),
			sGoPrev,
			_T("")
		);
	}
		m_pToolBar->AddButtonToRight
		(
			ID_GO_NEXT,
			_T("gonext"),
			TBIcon(szIconNext, TOOLBAR),
			sGoNext,
			_T("")
		);

		/*m_pToolBar->AddButtonToRight
		(
			IDOK,
			_T("ok"),
			TBIcon(szIconOk, TOOLBAR),
			sOK,
			_T("")
		);*/

	m_pToolBar->AddButtonToRight
	(
		IDCANCEL,
		_T("cancel"),
		TBIcon(szIconEscape, TOOLBAR),
		sCancel,
		_T("")
	);

	//m_pToolBar->SetDefaultAction(ID_GO_NEXT);
}

//----------------------------------------------------------------------------------------------------------------------
int CDynamicContainerDlg::DecideOptimalWidth(CDynamicContainerTileGrp* pGroup)
{
	int				MinDialogWidth			= 0;
	BOOL			bAtLeast2MiniContiguous = FALSE;
	BOOL			bIsLarge				= FALSE;
	BOOL			bIsWide					= FALSE;
	TileDialogSize	maxTileDialogSize		= TILE_MINI;
	int				maxGroups				= 0;

	if (!m_pTilePanel)
		return 0;

	for (int i = 0; i < m_OwnInputMng.m_AllAskDialogDataAndGroups.GetCount() && !bIsWide; i++)
	{
		CAskDialogDataAndGroups* pAskDialogDataAndGroups = m_OwnInputMng.m_AllAskDialogDataAndGroups.GetAt(i);

		if (!pAskDialogDataAndGroups)
			continue;

		int nMiniContiguous = 1;
		TileDialogSize oldTileSize = TILE_WIDE;

		if (maxGroups < pAskDialogDataAndGroups->m_Groups.GetCount())
			maxGroups = pAskDialogDataAndGroups->m_Groups.GetCount();

		for (int j = 0; j < pAskDialogDataAndGroups->m_Groups.GetCount() && !bIsWide; j++)
		{
			AskGroup* pAG = (AskGroup*)pAskDialogDataAndGroups->m_Groups.GetAt(j);

			if (pAG->m_bHidden)
				continue;

			if (pAG->m_TileDialogSize == TILE_WIDE)
				bIsWide = TRUE;

			if (pAG->m_TileDialogSize == TILE_LARGE)
				bIsLarge = TRUE;

			if (pAG->m_TileDialogSize == TILE_MINI && oldTileSize == TILE_MINI)
				nMiniContiguous++;

			if (!bAtLeast2MiniContiguous && nMiniContiguous > 1)
				bAtLeast2MiniContiguous = TRUE;

			oldTileSize = pAG->m_TileDialogSize;

			if (pAG->m_TileDialogSize != TILE_MINI)
				maxTileDialogSize = pAG->m_TileDialogSize;
		}

	}

	if (!pGroup)
		return FALSE;

	if (bIsWide)
		MinDialogWidth = pGroup->m_WideWidth + MAGIC_DELTA_WIDTH;
	else if (bIsLarge)
		MinDialogWidth = pGroup->m_LargeWidth + MAGIC_DELTA_WIDTH;
	else if (bAtLeast2MiniContiguous)
		MinDialogWidth = 2 * pGroup->m_MiniWidth + m_pTilePanel->GetTileDialogStyle()->GetTileSpacing() + MAGIC_DELTA_WIDTH;
	else
	{
		switch (maxTileDialogSize)
		{
		case TILE_MINI:
			MinDialogWidth = pGroup->m_MiniWidth + MAGIC_DELTA_WIDTH;
			break;
		case TILE_STANDARD:
			MinDialogWidth = pGroup->m_StandardWidth + MAGIC_DELTA_WIDTH;
			break;
		default:
			break;
		}
	}

	//verifico cmq il numero massimo di groups
	if (maxGroups > 4)
		MinDialogWidth = MinDialogWidth = pGroup->m_LargeWidth + MAGIC_DELTA_WIDTH;

	return MinDialogWidth;
}

//----------------------------------------------------------------------------
BOOL CDynamicContainerDlg::OnInitDialog()
{
	EnableLayout();

	CParsedDialogWithTiles::OnInitDialog();

	SetToolbarStyle(ToolbarStyle::BOTTOM, 32, FALSE, TRUE);

	AddTileGroup
		(
			IDC_INPUT_DLG_WITH_TILES_TILE_GROUP,
			RUNTIME_CLASS(CDynamicContainerTileGrp),
			_NS_TILEGRP("DynamicContainerTileGrp")
		);

	if (m_nFirstActiveTab < 0)
		return FALSE;

	CDynamicContainerTileGrp* pGroup = dynamic_cast<CDynamicContainerTileGrp*>(m_pTileGroup);

	if (!pGroup)
		return FALSE;

	int nMinDialogWidth = DecideOptimalWidth(pGroup);
	m_OwnInputMng.m_MinDialogHeight = 0;//GetDialogMinHeight(TRUE);

	if (!DialogsCleanup(m_nFirstActiveTab))
		return FALSE;

	//reset livello di errore
	m_OwnInputMng.ResetErrorLevel();

	SetDefaultActionButton();
	m_OwnInputMng.SetFocusFirst(0);
	
	SetWindowPos(NULL, 0, 0, nMinDialogWidth, m_OwnInputMng.m_MinDialogHeight, SWP_NOMOVE | SWP_NOZORDER);
	
	ModifyStyle(0, WS_THICKFRAME);
	
	OnUpdateControls();

	return FALSE;
}

//---------------------------------------------------------------------------------
BOOL CDynamicContainerDlg::DialogsCleanup(int idTab)
{
	CTilePanelTab* pTabActive = m_pTilePanel->GetTab(idTab);

	if (!pTabActive)
		return FALSE;

	SetWindowText(pTabActive->GetTitle());
	m_pTilePanel->SetActiveTab(idTab);

	CAskDialogDataAndGroups* pAskDialogDataAndGroups = m_OwnInputMng.GetAskDialog(idTab);

	if(!pAskDialogDataAndGroups)
	return FALSE;

	for(int i = m_pTilePanel->GetTabsNum() - 1; i >= 0; i--)
	{
		CTilePanelTab* pTab = m_pTilePanel->GetTab(i);

		if(pTab == pTabActive)
			pAskDialogDataAndGroups->m_idxTab = 0;
		else
			m_OwnInputMng.RemoveTilePanelTab(m_pTilePanel, i);
	}

	return TRUE;
}

//---------------------------------------------------------------------------------
BOOL CDynamicContainerDlg::PreProcessMessage(MSG* pMsg)
{
	if (pMsg->message == WM_KEYDOWN && pMsg->wParam == VK_ESCAPE)
	{
		m_OwnInputMng.m_nPressedBtn = IDCANCEL;
		EndDialog(IDCANCEL);

		return TRUE;
	}

	if (pMsg->message == WM_KEYDOWN && pMsg->wParam == VK_RETURN)
		if ((GetKeyState(VK_SHIFT) & 0x8000) == 0x8000)
		{
			OnGoPrevClick();
			return TRUE;
		}

	return __super::PreProcessMessage(pMsg);
}

//-------------------------------------------------------------------------------------
void CDynamicContainerDlg::OnMoving(UINT nFlags, LPRECT pRect)
{
	CRect aParentRect;
	GetOwner()->GetWindowRect(aParentRect);

	if (pRect->bottom > aParentRect.bottom)
	{
		
		pRect->top = aParentRect.bottom - m_OwnInputMng.m_MinDialogHeight;
		pRect->bottom = pRect->top + m_OwnInputMng.m_MinDialogHeight;
	}
}

//---------------------------------------------------------------------------------
void CDynamicContainerDlg::OnWindowPosChanging(WINDOWPOS FAR* wndPos)
{
	if (!m_pTileGroup || !m_pTilePanel)
		return;

	m_OwnInputMng.m_MinDialogHeight = GetDialogMinHeight(m_pTilePanel->GetTabsNum() == 1);// +MAGIC_DELTA_HEIGHT;

	CRect aRect;
	GetWindowRect(aRect);
	if (wndPos->cy < m_OwnInputMng.m_MinDialogHeight)
	{
		wndPos->cy = m_OwnInputMng.m_MinDialogHeight;
		
		if (m_bCreated)
			wndPos->y = aRect.top;
	}
}

//------------------------------------------------------------------------------------------------
int CDynamicContainerDlg::GetDialogMinHeight(BOOL bWithoutTabber /*= FALSE*/)
{
	if (!m_pTileGroup || !m_pTilePanel)
		return 0;

	CRect tRectToolBar;
	m_pToolBar->GetWindowRect(tRectToolBar);

	int tabberHeight = 0;
	if (!bWithoutTabber)
		tabberHeight += m_pTilePanel->GetTabberDefaultHeight();

	int BottomPartHeight = tabberHeight + tRectToolBar.Height();

	CRect dRect;
	GetClientRect(dRect);
	dRect.bottom = 0;
	
	BottomPartHeight += ScalePix(MAGIC_DELTA_HEIGHT);

	return BottomPartHeight + m_pTileGroup->GetMinHeight(dRect);
}

//----------------------------------------------------------------------------
BOOL CDynamicContainerDlg::OnCommand(WPARAM wParam, LPARAM lParam)
{
	DECLARE_WM_COMMAND_PARAMS(wParam, lParam, nID, nCode, hWndCtrl);

	switch (nID)
	{
		/*case IDOK:
			if (m_OwnInputMng.m_ErrorMessages.IsShowingMessages())
				return FALSE;

			if (DoGoNextClick(-2))
				return TRUE;

			m_OwnInputMng.ReadAllEdits(this);
			m_OwnInputMng.m_nPressedBtn = IDOK;
			EndDialog(IDOK);
			return TRUE;*/

		case IDCANCEL:
			if (m_OwnInputMng.m_ErrorMessages.IsShowingMessages())
				return FALSE;

			m_OwnInputMng.m_nPressedBtn = IDCANCEL;
			EndDialog(IDCANCEL);
			return TRUE;
	}

	if (nID && nCode == EN_VALUE_CHANGED)
	{
		m_OwnInputMng.DoDynValueChanged(nID);	

		ManageAfterValueChanged();
		
		CTilePanelTab* pCrtTab = m_pTilePanel->GetActiveTab();

		if (pCrtTab)
			pCrtTab->Invalidate();

		return TRUE;
	}

	return __super::OnCommand(wParam, lParam);
}

//------------------------------------------------------------------------------------
void CDynamicContainerDlg::ManageAfterValueChanged()
{
	//disabilita tutte le prossime tab
	int nCrtTab = m_pTilePanel->GetActiveTabNum();
	RemoveAllTabsFrom(nCrtTab);
}

//----------------------------------------------------------------------------
void CDynamicContainerDlg::RemoveAllTabsFrom(int nTab)
{
	for (int i = m_pTilePanel->GetTabsNum() - 1; i > nTab; i--)
		m_OwnInputMng.RemoveTilePanelTab(m_pTilePanel, i);
}

//--------------------------------------------------------------------------
void CDynamicContainerDlg::EndDialog(int nResult)
{
	if (m_pParentWnd) m_pParentWnd->EnableWindow(TRUE);
	__super::EndDialog(nResult);
}

///////////////////////////////////////////////////////////////////////////////
//							CDynamicDlg
///////////////////////////////////////////////////////////////////////////////
//
///////////////////////////////////////////////////////////////////////////////
/*BEGIN_MESSAGE_MAP(CDynamicDlg, CParsedDialog)
	//{{AFX_MSG_MAP( CDynamicDlg )
		ON_MESSAGE (UM_GET_REPORT_NAMESPACE,	OnGetReportNamespace)
		ON_MESSAGE(UM_GET_LOCALIZER_INFO,		OnGetLocalizerInfo)
		ON_MESSAGE(UM_GET_CONTROL_DESCRIPTION,	OnGetControlDescription)
		ON_WM_MOUSEWHEEL()
	//}}AFX_MSG_MAP
END_MESSAGE_MAP()

//----------------------------------------------------------------------------
IMPLEMENT_DYNAMIC(CDynamicDlg, CParsedDialog)

//----------------------------------------------------------------------------
CDynamicDlg::CDynamicDlg(InputMng& Mng, CWnd* pParent )
	:
	CParsedDialog	(IDD_INPUT_DLG_SINGLE, pParent),
	m_OwnInputMng	(Mng)
{}

//--------------------------------------------------------------------------
LRESULT CDynamicDlg::OnGetReportNamespace(WPARAM, LPARAM)
{
	if (m_OwnInputMng.m_pDocument)
		return (LRESULT)m_OwnInputMng.m_pDocument->GetNamespace().ToString().AllocSysString();

	return NULL;
}
//-----------------------------------------------------------------------------
LRESULT CDynamicDlg::OnGetLocalizerInfo(WPARAM wParam, LPARAM lParam)
{
	return GetLocalizerInfo(wParam, lParam);
}

//----------------------------------------------------------------------------
BOOL CDynamicDlg::OnInitDialog()
{
	CParsedDialog::OnInitDialog();

	CSize dlgSize = m_OwnInputMng.CreateAskDialog(this, TRUE);
	
	if (dlgSize == CSize(0, 0))
	{
		EndDialog(IDCANCEL);
		return FALSE;
	}

	AdjustClientRect(*this, dlgSize.cx, dlgSize.cy);

	BOOL bNeedVScroll = FALSE;
	CRect r1, r2;
	if (dlgSize.cy > 700)	//risoluzioni verticali portatili 768, 900, 1024, etc
	{
		::GetMonitorRect(GetParent() ? GetParent() : this, r1, r2);

		if (dlgSize.cy > (r1.Height() - 68))
			bNeedVScroll = TRUE;
	}

	if (bNeedVScroll)
	{
		CRect rc;
		GetWindowRect(rc);

		this->m_bEnableVScrolling = TRUE;
		this->ModifyStyle(0, WS_VSCROLL );
		this->InitScroll();
		this->PrepareScrollInfo(rc.Width(), rc.Height());

		rc.bottom = rc.top + (r1.Height() - 68);
		SetWindowPos(NULL, 0, 0, rc.Width() + 10, rc.Height(), SWP_NOMOVE | SWP_NOZORDER);
	}

	CenterWindow(GetOwner());

	return FALSE;
}

//----------------------------------------------------------------------------
BOOL CDynamicDlg::OnCommand(WPARAM wParam, LPARAM lParam)
{
	DECLARE_WM_COMMAND_PARAMS(wParam, lParam, nID, nCode, hWndCtrl);

	switch (nID)
	{
		case IDOK:
			m_OwnInputMng.ReadAllEdits(this);
			m_OwnInputMng.m_nPressedBtn = IDOK;
			EndDialog(IDOK);
			return TRUE;

		case IDCANCEL:
			m_OwnInputMng.m_nPressedBtn = IDCANCEL;
			EndDialog(IDCANCEL);
			return TRUE;
	}
	
	if (nID && nCode == EN_VALUE_CHANGED)
	{
		m_OwnInputMng.DoDynValueChanged(nID);
		return TRUE;
	}

	return CParsedDialog::OnCommand(wParam, lParam);
}

//--------------------------------------------------------------------------
void CDynamicDlg::EndDialog(int nResult)
{ 
	if (m_pParentWnd) m_pParentWnd->EnableWindow(TRUE);
	__super::EndDialog(nResult);
}    

//----------------------------------------------------------------------------
void CDynamicDlg::AskModal()
{
	// simulated DoModal in order to allow hotlink document 
	// browsing and other documents operations
	if (m_pParentWnd) 
		m_pParentWnd->EnableWindow(FALSE); //verra' riabililtata dalla enddialog 

	if (!Create(IDD_INPUT_DLG_SINGLE, m_pParentWnd))
	{
		if (m_pParentWnd) 
			m_pParentWnd->EnableWindow(TRUE); //se fallisce la creazione, riabilito la parent

		m_OwnInputMng.m_nPressedBtn = IDCANCEL;
		return;
	}

	// I insert ask dialog in Show/Hide feature when 
	// dockable tab manager changes tab
	CDockableFrame* pDockableFrame = dynamic_cast<CDockableFrame*>(m_pParentWnd);
	if (pDockableFrame)
		pDockableFrame->AddChild(m_hWnd);

	::BringWindowToTop (GetSafeHwnd());
	ShowWindow(SW_SHOW);

	//metto il thread in modal state
	CPushMessageLoopDepthMng __pushLoopDepth(MODAL_STATE);
	AfxGetThreadContext()->RaiseCallBreakEvent();

	//aspetto che venga premuto un bottone
	m_OwnInputMng.m_nPressedBtn = -1;
	m_OwnInputMng.m_pEngineScheduler->LoopUntil(m_OwnInputMng.m_nPressedBtn);

	if (pDockableFrame)
		pDockableFrame->RemoveChild(m_hWnd);
}

//-----------------------------------------------------------------------------
LRESULT CDynamicDlg::OnGetControlDescription(WPARAM wParam, LPARAM lParam)
{
	CWndObjDescriptionContainer* pContainer = (CWndObjDescriptionContainer*)wParam;

	//Il metodo GetWindowDescription crea da zero una descrizione(del tipo della runtimeclass passata) se non esisteva gia,
	//o ripesca quella gia creata nei round trip precedenti se esisteva.
	//Nella creazione assegna un id alla descrizione (m_strId), che servira' da chiave per recuperarle.
	//Questo id viene creato in modo standard sulla base dell'handle di finestra.
	//In alcuni casi pero finestre "differenti" hanno lo stesso id (es. parsedbitmap del bodyedit).
	//In questi casi si puo' creare un ID disambiguo e passarlo al metodo GetWindowDescription.
	
	CString strId = (LPCTSTR)lParam;

	CWndDialogDescription* pDesc = (CWndDialogDescription*)pContainer->GetWindowDescription(this, RUNTIME_CLASS(CWndDialogDescription), strId);
	pDesc->m_bIsModal = TRUE;
	pDesc->UpdateAttributes(this);

	pDesc->AddChildWindows(this);
	return (LRESULT)pDesc;
}*/

///////////////////////////////////////////////////////////////////////////////
//						InputMng methods.                                                       //
///////////////////////////////////////////////////////////////////////////////
//----------------------------------------------------------------------------
InputMng::InputMng(CBaseDocument* pDocument, WoormTable* pSymTable, BOOL bOwnsAskDialogData)
	:
	m_pDocument				(pDocument),
	m_pSymTable				(pSymTable),
	m_nRetVal				(0),
	m_pBag					(NULL),
	m_nPressedBtn			(-1),
	m_idxLastDialog			(-1),
	m_strCustomErr			(_T("")),
	m_strErrorDetail		(_T("")),
	m_ErrorLevel			(NONE),
	m_nCtlId				(0),
	m_DesignMode			(InputMng::RUNNING),
	m_pEngineScheduler		(NULL),
	m_pCDynamicContainerDlg	(NULL)
{
	m_bOwnsAskDialogData = bOwnsAskDialogData;
	if (m_bOwnsAskDialogData)
	{
		ASSERT(pSymTable);
		m_AllAskDialogDataAndGroups.Add(new CAskDialogDataAndGroups(new AskDialogData(*pSymTable, pDocument)));
	}
}

//----------------------------------------------------------------------------
InputMng::~InputMng()
{
	CleanUpAskDialogDataAndGroups();
}

//--------------------------------------------------------------------------------
void InputMng::CleanUpAskDialogDataAndGroups()
{
	for (int i = m_AllAskDialogDataAndGroups.GetCount() - 1; i >= 0; i--)
	{
		if (m_bOwnsAskDialogData)
			SAFE_DELETE(m_AllAskDialogDataAndGroups.GetAt(i)->m_pAskDialogData);

		SAFE_DELETE(m_AllAskDialogDataAndGroups.GetAt(i));

		m_AllAskDialogDataAndGroups.RemoveAt(i);
	}
}

//--------------------------------------------------------------------------------
void InputMng::Message(LPCTSTR lpszText)
{
	if (m_pDocument && m_pDocument->IsInUnattendedMode())
	{
		m_pDocument->m_pMessages->Add(lpszText);
		return;
	}

	m_ErrorMessages.Add(lpszText);

//	AfxMessageBox(lpszText);
}

//---------------------------------------------------------------------------------
void InputMng::InitFieldsOfHiddenDialog(int nDialog)
{
	if (m_AllAskDialogDataAndGroups.GetCount() <= 0)
		return;

	if (m_AllAskDialogDataAndGroups.GetAt(nDialog)->m_idxTab >= 0 && m_pCDynamicContainerDlg)
		RemoveTilePanelTab(m_pCDynamicContainerDlg->m_pTilePanel, m_AllAskDialogDataAndGroups.GetAt(nDialog)->m_idxTab);

	AskDialogData* pAskDialogData = m_AllAskDialogDataAndGroups.GetAt(nDialog)->m_pAskDialogData;

	for (int k = 0; k <= m_AllAskDialogDataAndGroups.GetAt(nDialog)->m_Groups.GetUpperBound(); k++)
	{
		AskGroup* pAG = (AskGroup*)m_AllAskDialogDataAndGroups.GetAt(nDialog)->m_Groups[k];

		for (int j = 0; j <= pAG->m_Entries.GetUpperBound(); j++)
		{
			AskEntry* pAE = (AskEntry*)pAG->m_Entries[j];
			if (!pAE->m_pData)
				continue;

			//---- skip dei campi valorizzati da WoormInfo
			if (m_pBag)
			{
				for (int n = 0; n < m_pBag->m_arInField.GetSize(); n++)
				{
					WoormField* pF = m_pBag->m_arInField.GetAt(n);
					if (pAE->GetPublicName().CompareNoCase(pF->GetName()) == 0)
						goto l_skipField;
				}

				if (CStringArray_Find(m_pBag->m_arFieldsInitialized, pAE->GetPublicName()) >= 0)
					goto l_skipField;
			}

			//---- inizializzo i campi che non sono valorizzati da WoormInfo o da precedenti esecuzioni
			if (pAE->m_pInitExpr)
			{
				if (!pAE->m_pInitExpr->Eval(*pAE->m_pData))
				{
					CString mess = pAskDialogData->m_strTitle + _T("/") + pAG->m_strLocalizedCaption + _T("/") + pAE->GetPublicName() + _T(": ") + pAE->m_pInitExpr->GetErrDescription();
					Message(mess);
					
					if (GetDesignMode() == InputMng::RUNNING)
						return;
				}

				m_pBag->m_arFieldsInitialized.Add(pAE->GetPublicName());
			}

			pAE->GetEdit(FALSE);
//----
l_skipField:;
		}
	}
}

//----------------------------------------------------------------------------
/*BOOL InputMng::AskModal(CWnd* pParent)
{
	if (pParent == NULL && m_pDocument)
		pParent = CWnd::FromHandle(m_pDocument->GetFrameHandle());

	CDynamicDlg aCDynamicDlg(*this, pParent);
	aCDynamicDlg.AskModal();

	return m_nPressedBtn == IDOK;
}*/

//----------------------------------------------------------------------------
BOOL InputMng::AskModalTiles(CWnd* pParent)
{
	if (pParent == NULL && m_pDocument) 
		pParent = CWnd::FromHandle(m_pDocument->GetFrameHandle());
	
	m_pCDynamicContainerDlg = new CDynamicContainerDlg(*this, pParent);
	m_pCDynamicContainerDlg->AskModal();
	SAFE_DELETE(m_pCDynamicContainerDlg);

	return m_nPressedBtn == IDOK;
}

//----------------------------------------------------------------------------
BOOL InputMng::SyncAllAskSavedFromExternalController()
{
	if (!m_pDocument || !m_pDocument->m_pExternalControllerInfo)
		return TRUE;

	for (int nDlg = 0; nDlg < m_AllAskDialogDataAndGroups.GetCount(); nDlg++)
	{
		CAskDialogDataAndGroups* pAskDialogDataAndGroups = m_AllAskDialogDataAndGroups.GetAt(nDlg);

		if (pAskDialogDataAndGroups == NULL)
			return TRUE;

		// sincronizzazione con controllore esterno
		SyncExternalControllerInfo(TRUE, pAskDialogDataAndGroups);
	}

	return TRUE;
}

//----------------------------------------------------------------------------
CSize InputMng::CreateAskDialog(CParsedForm* pForm, BOOL bInitData /*= TRUE*/)
{
	CString	strTitle;
	CWnd* pDialog = pForm->GetFormCWnd();

	if (m_AllAskDialogDataAndGroups.GetAt(0)->m_pAskDialogData->GetTitle().Find(AskDialogData::GetEmptyTitle()) == -1)
		strTitle = AfxLoadReportString(m_AllAskDialogDataAndGroups.GetAt(0)->m_pAskDialogData->GetTitle(), m_pDocument);
	else
		strTitle.Empty();

	pDialog->SetWindowText(strTitle);
	
	int nCtlId = 0;
	BOOL bOk = m_AllAskDialogDataAndGroups.GetAt(0)->m_Groups.GetSize() > 0;

	int i = 0;
	for (i = 0; bOk && i <= m_AllAskDialogDataAndGroups.GetAt(0)->m_Groups.GetUpperBound(); i++)
	{
		AskGroup* pAG = (AskGroup*)m_AllAskDialogDataAndGroups.GetAt(0)->m_Groups[i];
		if (pAG->m_pAskGroupData->m_strTitle.Find(AskGroupData::GetEmptyTitle()) == -1)
			strTitle = pAG->m_pAskGroupData->m_strTitle;
		else
			strTitle.Empty();

		pAG->m_strLocalizedCaption = AfxLoadReportString(strTitle, m_pDocument);
		if (pAG->AllocItems(pDialog, nCtlId, m_pDocument))
			pAG->SetFont(pDialog->GetFont());
		else
			bOk = FALSE;
	}

	if (!bOk)
	{
		CString mess = _TB("Initialization error in the data request window");
		Message(mess);

		return CSize(0, 0);
	}

	CDC* pDC = pDialog->GetDC();
	CSize cs = GetTextSize(pDC, 2, pDialog->GetFont());
	pDialog->ReleaseDC(pDC);

	int LeftOuterBorder = cs.cx;
	int RightOuterBorder = cs.cx;
	int TopOuterBorder = cs.cy;
	int BottomOuterBorder = cs.cy;
	int InterBoxGap = cs.cy / 2;

	//  Phase   1:  Determine subordinate minimal requirements
	int totalBoxesHeight = 0;
	int totalBoxesWidth = 0;
	int maxWidth = 0;
	int maxAlign = 0;

	for (i = 0; i <= m_AllAskDialogDataAndGroups.GetAt(0)->m_Groups.GetUpperBound(); i++)
	{
		AskGroup* pAG = (AskGroup*)m_AllAskDialogDataAndGroups.GetAt(0)->m_Groups[i];
		pAG->EvalMinSize();
		if (maxWidth < pAG->m_nMinWidth)
			maxWidth = pAG->m_nMinWidth;

		if (maxAlign < pAG->m_nMinAlign)
			maxAlign = pAG->m_nMinAlign;

		totalBoxesHeight += pAG->m_nMinHeight + InterBoxGap;
	}

	totalBoxesHeight -= InterBoxGap;
	totalBoxesWidth = maxWidth + maxAlign;

	CRect r(0, 0, 0, 0);

	CWnd* pWndOK = pDialog->GetDlgItem(IDOK);
	CWnd* pWndCANCEL = pDialog->GetDlgItem(IDCANCEL);

	if (pWndOK)
		pWndOK->GetWindowRect(r);

	int borBtnW = r.right - r.left + 1;
	int borBtnH = r.bottom - r.top + 1;
	int borBtnGap = borBtnW / 4;

	int minBtnsWidth = (2 * borBtnW) + borBtnGap + 2;

	int boxLeft, buttonLeft;
	int dialogWidth = LeftOuterBorder + RightOuterBorder;

	if (minBtnsWidth < totalBoxesWidth)
	{
		// HERE MASTER ARE BOXES
		dialogWidth += totalBoxesWidth;
		boxLeft = LeftOuterBorder;
		buttonLeft = (dialogWidth - minBtnsWidth) / 2;
	}
	else
	{
		// HERE MASTER ARE BUTTONS
		dialogWidth += minBtnsWidth;
		boxLeft = (dialogWidth - totalBoxesWidth) / 2;
		buttonLeft = LeftOuterBorder;
	}

	//  Phase   2:  Locate everybody
	int yCursor = TopOuterBorder;

	for (i = 0; i <= m_AllAskDialogDataAndGroups.GetAt(0)->m_Groups.GetUpperBound(); i++)
	{
		AskGroup* pAG = (AskGroup*)m_AllAskDialogDataAndGroups.GetAt(0)->m_Groups[i];
		pAG->m_nAlign = maxAlign;
		pAG->Layout(boxLeft, yCursor, totalBoxesWidth, pAG->m_nMinHeight);
		yCursor += pAG->m_nMinHeight + InterBoxGap;
	}

	if (pWndOK)
	{
		pDialog->GetDlgItem(IDOK)->SetWindowPos(NULL, buttonLeft, yCursor, 0, 0, SWP_NOSIZE | SWP_NOZORDER);
		buttonLeft += borBtnW + borBtnGap;
	}

	if (pWndCANCEL)
	{
		pDialog->GetDlgItem(IDCANCEL)->SetWindowPos(NULL, buttonLeft, yCursor, 0, 0, SWP_NOSIZE | SWP_NOZORDER);
		buttonLeft += borBtnW + borBtnGap;
	}

	yCursor += borBtnH + BottomOuterBorder;

	for (i = 0; i <= m_AllAskDialogDataAndGroups.GetAt(0)->m_Groups.GetUpperBound(); i++)
	{
		AskGroup* pAG = (AskGroup*)m_AllAskDialogDataAndGroups.GetAt(0)->m_Groups[i];

		pAG->ShowWindows();

		pAG->DoValueChanged(FALSE, bInitData, m_pBag);
	}

	// deve essere fatta DOPO che tutti i control non stati inizializzati
	for (i = 0; i <= m_AllAskDialogDataAndGroups.GetAt(0)->m_Groups.GetUpperBound(); i++)
		((AskGroup*)m_AllAskDialogDataAndGroups.GetAt(0)->m_Groups[i])->SetupQueryComponentsEdits();

	for (i = 0; i <= m_AllAskDialogDataAndGroups.GetAt(0)->m_Groups.GetUpperBound(); i++)
	{
		AskGroup* pAG = (AskGroup*)m_AllAskDialogDataAndGroups.GetAt(0)->m_Groups[i];
		if (pAG && !pAG->m_bHidden)
		{
			if (pAG->SetFocus())
				break;
		}
	}

	return CSize(dialogWidth, yCursor);
}

//----------------------------------------------------------------------------
void InputMng::CreateAskDialog(CTilePanel* pPanel, CAskDialogDataAndGroups* pAskDialogDataAndGroups, BOOL bInitData /*= TRUE*/)
{
	CString	strTitle;
	CWnd* pDialog = NULL; 
	
	AskDialogData* pAskDialogData = pAskDialogDataAndGroups->m_pAskDialogData;
	
	if(pAskDialogData->GetTitle().Find(AskDialogData::GetEmptyTitle()) == -1)
		strTitle = AfxLoadReportString(pAskDialogData->GetTitle(), m_pDocument);
	else
	{
		strTitle.Empty();
		//strTitle = _TB("Criteria...");	??Senza titolo :-( ?
	}

	CString strName = strTitle.Trim();

	CTilePanelTab* pTab = pPanel->AddTab(strName, strTitle);	
	if (!pTab)
		return; 

	pTab->ShowWindow(SW_HIDE);
	pAskDialogDataAndGroups->m_idxTab = pPanel->GetTabIdx(pTab);

	for (int i = 0; i <= pAskDialogDataAndGroups->m_Groups.GetUpperBound(); i++)
	{
		AskGroup* pAG = (AskGroup*)pAskDialogDataAndGroups->m_Groups[i];

		if (pAG->m_bHidden)
		{
			//TODO A.24762
			pAskDialogDataAndGroups->m_Tiles.Add(NULL);
			continue;
		}
		//hide title for group box
		pAG->m_bHiddenTitleGroupBoxAlways = TRUE;

		if (pAG->m_pAskGroupData->m_strTitle.Find(AskGroupData::GetEmptyTitle()) == -1)
			strTitle = pAG->m_pAskGroupData->m_strTitle;
		else
			strTitle.Empty();
			
		CDynamicContainerTileDlg* pTile = (CDynamicContainerTileDlg*)pPanel->AddTile(pTab, RUNTIME_CLASS(CDynamicContainerTileDlg), IDD_INPUT_DLG, AfxLoadReportString(strTitle, m_pDocument), TILE_STANDARD);
		pTile->SetTileDlgName(_T("TileDlg_") + DataInt(i).ToString());
		ASSERT(pTile);
		//if (pAG->m_bHidden) //TODO A.24762
		//{
		//	pTile->Show(FALSE);
		//}

		pAskDialogDataAndGroups->m_Tiles.Add(pTile);

		pTile->SetPinnable(FALSE);
		pDialog = pTile->GetFormCWnd();

		if (!pAG->AllocItems(pDialog, m_nCtlId, m_pDocument))
		{
			CString mess = _TB("Initialization error in the data request window");
			Message(mess);

			return;
		}

		if (pAG->m_bHidden)//TODO A.24762
			continue;
		
		pAG->m_strLocalizedCaption = AfxLoadReportString(strTitle, m_pDocument);
		pAG->SetFont(pDialog->GetFont());

		pAG->EvalMinSize();

		int nTileHeight = pAG->m_nMinHeight + pTile->GetTitleHeight();
		pTile->SetMinHeight(nTileHeight);
	}

	CDynamicContainerTileGrp* pGroup = dynamic_cast<CDynamicContainerTileGrp*>(pPanel->GetParent());
	ASSERT(pGroup);
	
	//prova a vedere la larghezza del gruppo per decidere che tipo di tile aggiungere
	for (int i = 0; i <= pAskDialogDataAndGroups->m_Groups.GetUpperBound(); i++)
	{
		CTileDialog* pTile = pAskDialogDataAndGroups->m_Tiles.GetAt(i);
		if (!pTile)
			continue;

		AskGroup* pAG = (AskGroup*)pAskDialogDataAndGroups->m_Groups[i];
		
		if (pAG->m_bHidden)		//TODO A.24762
		{
			pTile->ChangeSizeTo(CSize(0, 0), TRUE);
			continue;
		}

		pAG->m_nAlign = pAG->m_nMinAlign;

		int dimAG = pAG->m_nMinWidth + pAG->m_nAlign;
		
		if (dimAG <= pGroup->m_MiniWidth)
		{
			pTile->SetTileSize(TILE_MINI);
			pAG->m_TileDialogSize = TILE_MINI;
		}
		else if (dimAG <= pGroup->m_StandardWidth)
		{
			pTile->SetTileSize(TILE_STANDARD);
			pAG->m_TileDialogSize = TILE_STANDARD;
		}
		else if (dimAG <= pGroup->m_LargeWidth)
		{
			pTile->SetTileSize(TILE_LARGE);
			pAG->m_TileDialogSize = TILE_LARGE;
		}
		else if (dimAG <= pGroup->m_WideWidth)
		{
			pTile->SetTileSize(TILE_WIDE);
			pAG->m_TileDialogSize = TILE_WIDE;
		}

		pTile->ChangeSizeTo(CSize(dimAG, pAG->m_nMinHeight + pTile->GetTitleHeight()), TRUE);
	}

	for (int i = 0; i <= pAskDialogDataAndGroups->m_Groups.GetUpperBound(); i++)
	{
		CTileDialog* pTile = pAskDialogDataAndGroups->m_Tiles.GetAt(i);
		if (!pTile)
			continue;

		AskGroup* pAG = (AskGroup*) pAskDialogDataAndGroups->m_Groups[i];

		if (pAG->m_bHidden) //TODO A.24762
			continue;

		CRect rTile;
		pTile->GetWindowRect(rTile);
		pTile->ScreenToClient(rTile);
		pAG->Layout(2, pTile->GetTitleHeight(), rTile.Width() - 3, rTile.Height() - pTile->GetTitleHeight());
	}

	for (int i = 0; i <= pAskDialogDataAndGroups->m_Groups.GetUpperBound(); i++)
	{
		AskGroup* pAG = (AskGroup*)pAskDialogDataAndGroups->m_Groups[i];

		if (pAG->m_bHidden && pAskDialogDataAndGroups->m_Tiles.GetAt(i))
			pAskDialogDataAndGroups->m_Tiles.GetAt(i)->Show(FALSE);

		pAG->ShowWindows();
		pAG->DoValueChanged(FALSE, bInitData, m_pBag);
	}

	// deve essere fatta DOPO che tutti i control sono stati inizializzati
	for (int i = 0; i <= pAskDialogDataAndGroups->m_Groups.GetUpperBound(); i++)
		((AskGroup*)pAskDialogDataAndGroups->m_Groups[i])->SetupQueryComponentsEdits();

	pTab->ShowWindow(SW_SHOW);
}

//-----------------------------------------------------------------------------------
int InputMng::GetIdxAskDialog(int nByIdxTab)
{
	CAskDialogDataAndGroups* pAskDialogDataAndGroups = NULL;

	for (int i = 0; i < m_AllAskDialogDataAndGroups.GetCount(); i++)
	{
		CAskDialogDataAndGroups* pAGD = m_AllAskDialogDataAndGroups.GetAt(i);
		if (pAGD->m_idxTab == nByIdxTab)
			return i;
	}

	return -1;
}

//----------------------------------------------------------------------------------------
CAskDialogDataAndGroups* InputMng::GetAskDialog(int nByIdxTab)
{
	CAskDialogDataAndGroups* pAskDialogDataAndGroups = NULL;

	for (int i = 0; i < m_AllAskDialogDataAndGroups.GetCount(); i++)
	{
		CAskDialogDataAndGroups* pAGD = m_AllAskDialogDataAndGroups.GetAt(i);
		if (pAGD->m_idxTab == nByIdxTab)
			return pAGD;
	}

	return NULL;
}

//----------------------------------------------------------------------------------
void InputMng::SetFocusFirst(int idxTab)
{
	if (m_AllAskDialogDataAndGroups.GetCount() == 0)
		return;

	CAskDialogDataAndGroups* pAskDialogDataAndGroups = GetAskDialog(idxTab);

	if (!pAskDialogDataAndGroups)
		return;

	for (int i = 0; i <= pAskDialogDataAndGroups->m_Groups.GetUpperBound(); i++)
	{
		AskGroup* pAG = (AskGroup*)pAskDialogDataAndGroups->m_Groups[i];
		if (pAG && !pAG->m_bHidden)
		{
			if (pAG->SetFocus())
				break;
		}
	}
}

//---------------------------------------------------------------------------------------
BOOL InputMng::CanDoNextAskDialog(int nDialog)
{
	//do after della stessa
	CAskDialogDataAndGroups*	pAskDialogDataAndGroups = m_AllAskDialogDataAndGroups.GetAt(nDialog);
	AskDialogData*				pAskDialogData			= pAskDialogDataAndGroups->m_pAskDialogData;

	//reset livello di errore
	ResetErrorLevel();


	//--------------------------------
	if (!pAskDialogDataAndGroups->IsAfterBlockExecuted())
	{
		SetErrorLevel(InputMng::AFTER_BLOCK);

		if (m_DesignMode == InputMng::RUNNING)
			return FALSE;
	}

	//----------------------------------
	DataBool bAbortRun(FALSE);
	if (!pAskDialogDataAndGroups->IsOnExprEvalued(bAbortRun))
	{
		SetErrorLevel(InputMng::ON_EXPR);
		m_strCustomErr.Format(_TB("Error in valuation of the validation clause of a request rule {0-%s}"), pAskDialogData->GetName());

		if (m_DesignMode == InputMng::RUNNING)
			return FALSE;
	}
	if (bAbortRun)
	{		SetErrorLevel(InputMng::ABORT_EXPR);

		DataStr sAbortMsg;
		if (!pAskDialogDataAndGroups->IsAbortExprEvalued(sAbortMsg))
		{
			m_strCustomErr.Format(_TB("Error in valuation of the expression of a request rule {0-%s}"), pAskDialogData->GetName());
			m_strErrorDetail = pAskDialogData->m_pAbortExpr->GetErrDescription();

			if (m_DesignMode == InputMng::RUNNING)
				return FALSE;
		}

		m_strCustomErr = sAbortMsg.FormatData();
		
		return FALSE;
	}
	
	return TRUE;
}

//-------------------------------------------------------------------------------
CAskDialogDataAndGroups* InputMng::GetNextAskDialog(BOOL bFromNavigator, BOOL& bFoundErrors)
{
	CWoormDoc* pWoormDoc = dynamic_cast<CWoormDoc*>(m_pDocument);

	//resetto il livello di error
	ResetErrorLevel();
	
	//vado a vedere la prossima enabled
	for (int i = m_idxLastDialog + 1; i < m_AllAskDialogDataAndGroups.GetCount(); i++)
	{
		CAskDialogDataAndGroups* pAskDialogAndGroups = m_AllAskDialogDataAndGroups.GetAt(i);

		// bFromNavigator a TRUE = sta andando avanti col next
		if (
				bFromNavigator 
				&& !m_bRunningFromExternalController 
				&& (!pWoormDoc || !pWoormDoc->m_bHideAllAskDialogs) 
				&& !pAskDialogAndGroups->IsBeforeBlockEecuted()
			)
		{
			SetErrorLevel(InputMng::BEFORE_BLOCK);

			bFoundErrors = TRUE;
			if (m_DesignMode == InputMng::RUNNING)
			{ 
				m_idxLastDialog = i;
				return NULL;
			}
		}

		if (
				m_DesignMode == InputMng::RUNNING &&
				(
					!m_bRunningFromExternalController
					&& pWoormDoc 
					&& (pWoormDoc->IsInUnattendedMode() || pWoormDoc->m_bHideAllAskDialogs)
				)
			)
		{
			if(bFromNavigator)InitFieldsOfHiddenDialog(i);
			continue;
		}

		DataBool bWhenResult(TRUE);
		if (!pAskDialogAndGroups->IsWhenExprEvalued(bWhenResult))
		{
			SetErrorLevel(InputMng::WHEN_EXPR);
			m_strCustomErr.Format(_TB("Error in valuation of the visualization clause of a request rule {0-%s}"), pAskDialogAndGroups->m_pAskDialogData->GetName());
			m_strErrorDetail = pAskDialogAndGroups->m_pAskDialogData->m_pWhenExpr->GetErrDescription();

			bFoundErrors = TRUE;
			if (m_DesignMode == InputMng::RUNNING)
			{
				m_idxLastDialog = i;
				return NULL;
			}
		}

		if (!bWhenResult && m_DesignMode == InputMng::RUNNING)
		{
			if(bFromNavigator) InitFieldsOfHiddenDialog(i);
			continue;
		}

		if (bWhenResult || m_DesignMode == InputMng::DESIGN)
		{
			BOOL bSomethingVisible = SomethingVisible(pAskDialogAndGroups, bFoundErrors);

			if (bFoundErrors && m_DesignMode == InputMng::RUNNING)
			{
				m_idxLastDialog = i;
				return NULL;
			}

			if (bSomethingVisible || m_DesignMode == InputMng::DESIGN)
			{
				m_idxLastDialog = i;
				return pAskDialogAndGroups;
			}
		}
	}

	return NULL;
}

//--------------------------------------------------------------------------------------------------
BOOL InputMng::SomethingVisible(CAskDialogDataAndGroups* pAskDialogDataAndGroups, BOOL& bFoundErrors)
{
	////init visible status for all tiles
	//for (int i = 0; i < pAskDialogDataAndGroups->m_Tiles.GetCount(); i++)
	//	pAskDialogDataAndGroups->m_Tiles.GetAt(i)->Show(TRUE);

	BOOL bSomethingVisible = FALSE;

	for (int i = 0; i <= pAskDialogDataAndGroups->m_Groups.GetUpperBound(); i++)
	{
		AskGroup* pAG = (AskGroup*)pAskDialogDataAndGroups->m_Groups[i];

		DataBool bWhenResult(TRUE);
		if (pAG->m_pAskGroupData->m_pWhenExpr && !pAG->m_pAskGroupData->m_pWhenExpr->Eval(bWhenResult))
		{
			SetErrorLevel(InputMng::WHEN_EXPR);
			m_strCustomErr.Format(_TB("Error in valuation of the visualization clause of a request rule {0-%s}"), pAskDialogDataAndGroups->m_pAskDialogData->GetName());
			m_strErrorDetail = pAskDialogDataAndGroups->m_pAskDialogData->m_pWhenExpr->GetErrDescription();

			bFoundErrors = TRUE;
			if (GetDesignMode() == InputMng::RUNNING)
				return FALSE;
		}

		pAG->m_bHidden = GetDesignMode() == InputMng::RUNNING && !bWhenResult;

		if (!pAG->m_bHidden)
			bSomethingVisible = TRUE;

		if (!pAG->m_bHidden)
		{
			AskEntry* pAE;
			BOOL bOneEntryVisible = FALSE;

			for (int j = 0; j <= pAG->m_Entries.GetUpperBound(); j++)
			{
				pAE = (AskEntry*)pAG->m_Entries[j];

				DataBool bWhenAEResult(TRUE);
				if (pAE->m_pAskFieldData->m_pWhenExpr && !pAE->m_pAskFieldData->m_pWhenExpr->Eval(bWhenAEResult))
				{
					SetErrorLevel(InputMng::WHEN_EXPR);
					m_strCustomErr.Format(_TB("Error in valuation of the visualization clause of a request rule {0-%s}"), pAskDialogDataAndGroups->m_pAskDialogData->GetName());
					m_strErrorDetail = pAskDialogDataAndGroups->m_pAskDialogData->m_pWhenExpr->GetErrDescription();

					bFoundErrors = TRUE;
					if (GetDesignMode() == InputMng::RUNNING)
						return FALSE;
				}

				pAE->m_bHidden = GetDesignMode() == InputMng::RUNNING && !bWhenAEResult;

				if (!pAE->m_bHidden)
					bOneEntryVisible = TRUE;
			}

			pAG->m_bHidden = GetDesignMode() == InputMng::RUNNING && !bOneEntryVisible;

			if (!pAG->m_bHidden)
				bSomethingVisible = TRUE;
		}
		else
		{
			//tutto hidden => distruggo tutti i controlli istanziati dall'eval
			SAFE_DELETE(pAG->m_pGroupBox);
			for (int j = 0; j <= pAG->m_Entries.GetUpperBound(); j++)
			{
				AskEntry* pAE = (AskEntry*)pAG->m_Entries[j];
				SAFE_DELETE(pAE->m_pCtrl);
				SAFE_DELETE(pAE->m_pStatic);
			}
		}

	}

	return bSomethingVisible;
}

//-------------------------------------------------------------------------------
void InputMng::RemoveTilePanelTab(CTilePanel* pPanel, int nTab)
{
	if (!pPanel)
	{
		ASSERT(pPanel);
		return;
	}

	CTilePanelTab* pTab = pPanel->GetTab(nTab);

	if (!pTab)
	{
		ASSERT(pTab);
		return;
	}

	pPanel->RemoveTab(nTab, TRUE);
	delete pTab;

	int idxAskDialog = GetIdxAskDialog(nTab);
	CAskDialogDataAndGroups* pADG = m_AllAskDialogDataAndGroups.GetAt(idxAskDialog);
	if (pADG)
	{
		pADG->m_idxTab = -1;

		for(int t = pADG->m_Tiles.GetUpperBound(); t >= 0; t--)
		{
			if (m_pCDynamicContainerDlg)
				for(int d = 0; d <= m_pCDynamicContainerDlg->m_pTileGroup->GetTileDialogs()->GetUpperBound(); d++)
				{
					CTileDialog* pTileDlg = dynamic_cast<CTileDialog*>(m_pCDynamicContainerDlg->m_pTileGroup->GetTileDialogs()->GetAt(d));
					if (pTileDlg)
						if (pADG->m_Tiles.GetAt(t) == pTileDlg)
						{
							m_pCDynamicContainerDlg->m_pTileGroup->GetTileDialogs()->RemoveAt(d);
							break;
						}
				}

			if (::IsWindow(pADG->m_Tiles.GetAt(t)->GetSafeHwnd()))
				pADG->m_Tiles.GetAt(t)->DestroyWindow();

			pADG->m_Tiles.RemoveAt(t);
		}

		for (int i = pADG->m_idxArray + 1; i < m_AllAskDialogDataAndGroups.GetCount(); i++)
			if (m_AllAskDialogDataAndGroups.GetAt(i)->m_idxTab > 0)
				m_AllAskDialogDataAndGroups.GetAt(i)->m_idxTab--;
	}
}

//-------------------------------------------------------------------------------------
BOOL InputMng::ReadAllEdits(CParsedForm* pForm)
{
	if (!pForm->CheckForm())
		return FALSE;

	AskGroup* pAG;

	for (int i = 0; i < m_AllAskDialogDataAndGroups.GetCount(); i++)
	{
		CAskDialogDataAndGroups* pAskDialogDataAndGroups = m_AllAskDialogDataAndGroups.GetAt(i);

		for (int j = 0; j <= pAskDialogDataAndGroups->m_Groups.GetUpperBound(); j++)
		{
			pAG = (AskGroup*)pAskDialogDataAndGroups->m_Groups[j];
			if (!pAG->GetEdits(m_pDocument && m_pDocument->IsEditingParamsFromExternalController()))
				return FALSE;
		}
	}
	
	return SyncAllAskSavedFromExternalController();
}

//-------------------------------------------------------------------------------------
BOOL InputMng::ShowMessages()
{
	if (m_bRunningFromExternalController)
		return TRUE;

	if (m_pEngineScheduler)
	{
		CString err = m_pEngineScheduler->FormatRTError();
		if (!err.IsEmpty())
			m_ErrorMessages.Add(err);

		m_pEngineScheduler->ResetRTError();
	}

	if (m_ErrorMessages.MessageFound())
	{
		// tapullo per non far intercettare il VK_RETURN alla dialog quando viene visualizzata la CMessages
		if (m_pCDynamicContainerDlg)
			m_pCDynamicContainerDlg->RemoveAccelerator(IDOK);

		m_ErrorMessages.Show();

		if (m_pCDynamicContainerDlg)
			m_pCDynamicContainerDlg->AppendAccelerator(IDOK, FVIRTKEY, VK_RETURN);

		return m_DesignMode == InputMng::DESIGN;
	}

	return TRUE;
}

//----------------------------------------------------------------------------
BOOL InputMng::SyncExternalControllerInfo(BOOL bSave, CAskDialogDataAndGroups* pAskDialogDataAndGroups) 
{
	if (!m_pDocument || !m_pDocument->m_pExternalControllerInfo)
		return FALSE;

	AskDialogData* pAskDialogData = pAskDialogDataAndGroups->m_pAskDialogData;
	
	for (int k = 0; k < pAskDialogDataAndGroups->m_Groups.GetSize(); k++)
	{
		AskGroup *pGroup = (AskGroup*)pAskDialogDataAndGroups->m_Groups.GetAt(k);
			
		for (int j = 0; j < pGroup->m_Entries.GetSize(); j++)
		{
			AskEntry* pEntry = (AskEntry*)pGroup->m_Entries.GetAt(j);
			if (bSave)
			{
				m_pDocument->m_pExternalControllerInfo->RetrieveControlData(pEntry);
			}
			else
			{
				if (m_pDocument->m_pExternalControllerInfo->ValorizeControlData(pEntry))
				{
					SymField* pMember = m_pSymTable->GetField(pEntry->GetPublicName());
					// aggiungo il campo a quelli statici per far sì che non venga inizializzato
					if (pMember)
					{
						ASSERT_KINDOF(WoormField, pMember);
						WoormField* pRF = (WoormField*)pMember;
						if (pEntry->m_pData)
						{
							pRF->GetRepData()->Assign(*pEntry->m_pData);
							if (m_pBag)
								m_pBag->m_parStaticFields->CheckAndAddField(pRF);
						}
					}
				}
				else
				{
					//---- inizializzo i campi che non sono valorizzati da WoormInfo o da precedenti esecuzioni
					if (pEntry->m_pInitExpr)
					{
						if (!pEntry->m_pInitExpr->Eval(*pEntry->m_pData))
						{
							CString mess = pAskDialogData->m_strTitle + _T("/") + pEntry->GetPublicName() + _T(": ") + pEntry->m_pInitExpr->GetErrDescription();
							Message(mess);
						}

						if (m_pBag)
							m_pBag->m_arFieldsInitialized.Add(pEntry->GetPublicName());
					}
				}
			}
		}
	}

	if (bSave && m_pDocument->IsEditingParamsFromExternalController())
	{
		CString	sFileName = AfxGetPathFinder()->GetXmlParametersDirectory();
		sFileName = sFileName + SLASH_CHAR + m_pDocument->m_pExternalControllerInfo->m_code + _T(".xml");

		m_pDocument->m_pExternalControllerInfo->SetRunningTaskStatus(CExternalControllerInfo::TASK_SAVE_PARAMS);
		//m_pDocument->m_pExternalControllerInfo->m_Finished.Set(); //in woorm fa casino cancella tutto LARA
		//TODO FAI METODO CHE SALVA
		CXMLDocumentObject aDoc;
		aDoc.LoadXML(m_pDocument->m_pExternalControllerInfo->GetXmlString());
		aDoc.SaveXMLFile(sFileName, TRUE);
	}

	//libero i valori impostati come locked dal processo di assegnazione
	m_pDocument->m_pExternalControllerInfo->UnlockValues();

	return TRUE;
}

//--------------------------------------------------------------------------------------
void InputMng::UpdateDynCtrlView()
{
	for (int i = 0; i < m_AllAskDialogDataAndGroups.GetCount(); i++)
	{
		CAskDialogDataAndGroups* pAskDialogDataAndGroups = m_AllAskDialogDataAndGroups.GetAt(i);
		UpdateDynCtrlGrp(pAskDialogDataAndGroups);
	}
}

//--------------------------------------------------------------------------------------
void InputMng::UpdateDynCtrlGrp(CAskDialogDataAndGroups* pAskDialogDataAndGroups)
{
	for(int j = 0; j < pAskDialogDataAndGroups->m_Groups.GetCount(); j++)
	{
		AskGroup* pAG = (AskGroup*) pAskDialogDataAndGroups->m_Groups.GetAt(j);

		for(int k = 0; k < pAG->m_Entries.GetCount(); k++)
		{
			AskEntry* pAEChanged = (AskEntry*) pAG->m_Entries[k];

			if(pAEChanged->m_pCtrl && ::IsWindow(pAEChanged->m_pCtrl->GetCtrlCWnd()->m_hWnd))
			{
				pAEChanged->m_pCtrl->UpdateCtrlView();
			}
		}
	}
}

//---------------------------------------------------------------------------------------------------
void InputMng::DoDynValueChanged(UINT nID, BOOL bSetValueChanged /*= TRUE*/)
{
	AskEntry* pAEChanged = NULL;

	for (int i = 0; i < m_AllAskDialogDataAndGroups.GetCount(); i++)
	{
		CAskDialogDataAndGroups* pAskDialogDataAndGroups = m_AllAskDialogDataAndGroups.GetAt(i);
		// cerca l'ask entry corrispondente all'ID che a generato il value changed
		for (int j = 0; j <= pAskDialogDataAndGroups->m_Groups.GetUpperBound() && pAEChanged == NULL; j++)
		{
			AskGroup* pAG = (AskGroup*)pAskDialogDataAndGroups->m_Groups[j];
			for (int k = 0; k <= pAG->m_Entries.GetUpperBound(); k++)
			{
				pAEChanged = (AskEntry*)pAG->m_Entries[k];

				if ((UINT)(pAEChanged->m_nCtlId) == nID)
					break;

				pAEChanged = NULL;
			}
		}
	}

	if (!pAEChanged)
		return;

	DoDynValueChanged(pAEChanged, bSetValueChanged);
}

//-----------------------------------------------------------------------------------------------------
void InputMng::DoDynValueChanged(const CString& sName, BOOL bSetValueChanged /*= TRUE*/)
{
	AskEntry* pAEChanged = NULL;

	for (int i = 0; i < m_AllAskDialogDataAndGroups.GetCount(); i++)
	{
		CAskDialogDataAndGroups* pAskDialogDataAndGroups = m_AllAskDialogDataAndGroups.GetAt(i);

		// cerca l'ask entry corrispondente all'ID che a generato il value changed
		for (int j = 0; j <= pAskDialogDataAndGroups->m_Groups.GetUpperBound() && pAEChanged == NULL; j++)
		{
			AskGroup* pAG = (AskGroup*)pAskDialogDataAndGroups->m_Groups[j];
			for (int k = 0; k <= pAG->m_Entries.GetUpperBound(); k++)
			{
				pAEChanged = (AskEntry*)pAG->m_Entries[k];

				if (pAEChanged->GetPublicName().CompareNoCase(sName) == 0)
					break;

				pAEChanged = NULL;
			}
		}
	}

	if (!pAEChanged)
		return;

	DoDynValueChanged(pAEChanged, bSetValueChanged);
}

//------------------------------------------------------------------------------------------------
void InputMng::DoDynValueChanged(AskEntry* pAEChanged, BOOL bSetValueChanged)
{
	if (bSetValueChanged)
		pAEChanged->m_pData->SetValueChanged();

	if (
		pAEChanged->m_pAskFieldData->m_nInputLimit == T_LOWER_LIMIT
		||
		pAEChanged->m_pAskFieldData->m_nInputLimit == T_UPPER_LIMIT
		)
	{
		if (pAEChanged->GetCtrlData()->IsKindOf(RUNTIME_CLASS(DataStr)))
		{
			DataStr* pS = (DataStr*)(pAEChanged->GetCtrlData());
			if (
				pS->GetUpperCase() == CParsedCtrl::Strings::FIRST()
				||
				pS->GetUpperCase() == CParsedCtrl::Strings::LAST()
				)
				pAEChanged->m_pData->Assign(_T(""));
		}
	}

	CString strName = pAEChanged->GetPublicName();

	// cerca in tutti gli ask entry della dialog le espressioni di inizializzazione
	// che dipendono dalla variabile che e` stata modificata e le valuta
	CObArray arCascadeChangedEntries;

	for (int i = 0; i < m_AllAskDialogDataAndGroups.GetCount(); i++)
	{
		CAskDialogDataAndGroups* pAskDialogDataAndGroups = m_AllAskDialogDataAndGroups.GetAt(i);
		for (int j = 0; j <= pAskDialogDataAndGroups->m_Groups.GetUpperBound(); j++)
		{
			AskGroup* pAG = (AskGroup*)pAskDialogDataAndGroups->m_Groups[j];
			for (int k = 0; k <= pAG->m_Entries.GetUpperBound(); k++)
			{
				AskEntry* pAE = (AskEntry*)pAG->m_Entries[k];
				if (pAE != pAEChanged)
					pAE->DoValueChanged(FALSE, TRUE, strName, &arCascadeChangedEntries, NULL);
			}
		}
	}

	for (int i = 0; i < m_AllAskDialogDataAndGroups.GetCount(); i++)
	{
		CAskDialogDataAndGroups* pAskDialogDataAndGroups = m_AllAskDialogDataAndGroups.GetAt(i);

		for (int j = 0; j <= pAskDialogDataAndGroups->m_Groups.GetUpperBound(); j++)
		{
			AskGroup* pAG = (AskGroup*)pAskDialogDataAndGroups->m_Groups[j];
			for (int k = 0; k <= pAG->m_Entries.GetUpperBound(); k++)
			{
				AskEntry* pAE = (AskEntry*)pAG->m_Entries[k];
				if (pAE != pAEChanged)
					pAE->SetupQueryComponentsEdit(strName);
			}
		}
	}

	for (int i = 0; i <= arCascadeChangedEntries.GetUpperBound(); i++)
	{
		DoDynValueChanged((AskEntry*)(arCascadeChangedEntries.GetAt(i)), FALSE);
	}

	//---- cerca una procedure di gestione dell' OnValueChanged del campo
	if (m_pSymTable)
	{
		CString strProcNameChanged = pAEChanged->GetPublicName() + L"_Changed";
		ProcedureObjItem* pPI = m_pSymTable->FindProcedure(strProcNameChanged);

		if (pPI && pPI->m_pProcedure && !pPI->m_pProcedure->IsEmpty())
		{
			CStringArray arModified;
			CStringArray* arOld = m_pSymTable->TraceFieldsModified(&arModified);

			BOOL bOk = pPI->m_pProcedure->Exec();

			for (int i = 0; i <= arModified.GetUpperBound(); i++)
			{
				DoDynValueChanged(arModified.GetAt(i), FALSE);
			}

			if (arOld && arModified.GetSize())
				arOld->Append(arModified);

			m_pSymTable->TraceFieldsModified(arOld);

			UpdateDynCtrlView();
		}
	}
}

//----------------------------------------------------------------------------
void InputMng::SetAskDlgEntryAutomaticExpression(CAutoExpressionMng* pAutoExprMng) 
{
	if (!pAutoExprMng)
		return;

	for (int i = 0 ; i <= m_AllAskDialogDataAndGroups.GetAt(0)->m_Groups.GetUpperBound(); i++)
	{
		AskGroup* pAG = (AskGroup*) m_AllAskDialogDataAndGroups.GetAt(0)->m_Groups[i];
		for (int j = 0 ; j <= pAG->m_Entries.GetUpperBound(); j++)
		{
			AskEntry* pAE = (AskEntry*)pAG->m_Entries[j];

			CString strDataExpr =  pAutoExprMng->GetExpressionByVarName(pAE->GetPublicName());
			if (!strDataExpr.IsEmpty())
			{
				pAE->SetAutomaticExpression(strDataExpr, TRUE);
				pAE->m_pCtrl->SetAutomaticExpression(strDataExpr);
			}
			else
				pAE->m_strAutomaticExpression.Empty();
		}
	}
}

//---------------------------------------------------------------------------
void InputMng::GetXmlParameters(CExportDataNetAskDialog & dialog)
{

	dialog.m_Title = m_AllAskDialogDataAndGroups.GetAt(0)->m_pAskDialogData->m_strTitle;
	dialog.m_Names = m_AllAskDialogDataAndGroups.GetAt(0)->m_pAskDialogData->m_strName;

	for (int i = 0; i < m_AllAskDialogDataAndGroups.GetAt(0)->m_Groups.GetSize(); i++)
	{
		AskGroup* pAskGroup = (AskGroup*)m_AllAskDialogDataAndGroups.GetAt(0)->m_Groups[i];
		ASSERT_VALID(pAskGroup);

		CExportDataNetAskDialogGroup * dataGroup = new CExportDataNetAskDialogGroup();
		pAskGroup->GetXmlParameters(*dataGroup, i + 1);
		dialog.m_arGroup.Add(dataGroup);
	}
}

//-------------------------------------------------------------------------------------------------------------------------------------
void InputMng::ResetErrorLevel	()
{
	m_ErrorLevel = InputMng::NONE;
	m_strCustomErr = _T("");
	m_strErrorDetail = _T(""); 

	if (m_pEngineScheduler)
		m_pEngineScheduler->ResetRTError();
}

//-------------------------------------------------------------------------------------------------------------------------------------
void InputMng::ManageErrorLevel()
{
	if (!m_pEngineScheduler)
		return;

	switch (m_ErrorLevel)
	{
	case InputMng::BEFORE_BLOCK:
		m_pEngineScheduler->SetRTError(EngineScheduler::EMPTY_MESSAGE);
		break;
	case InputMng::WHEN_EXPR:
		m_pEngineScheduler->SetRTError(EngineScheduler::REPORT_EVAL_WHEN_EXP, m_strErrorDetail, m_strCustomErr);
		break;
	case InputMng::AFTER_BLOCK:
		m_pEngineScheduler->SetRTError(EngineScheduler::EMPTY_MESSAGE);
		break;
	case InputMng::ON_EXPR:
		m_pEngineScheduler->SetRTError(EngineScheduler::REPORT_EVAL_ON_EXP, m_strErrorDetail, m_strCustomErr);
		break;
	case InputMng::ABORT_EXPR:
		if (m_strErrorDetail.IsEmpty())
			m_pEngineScheduler->SetRTError(EngineScheduler::REPORT_DIALOG_ONABORT_MESSAGE, m_strCustomErr);
		else
			m_pEngineScheduler->SetRTError(EngineScheduler::REPORT_EVAL_ONABORTMESSAGE_EXP, m_strErrorDetail, m_strCustomErr);
		break;
	}
}

/////////////////////////////////////////////////////////////////////////////
//						AskDialogInputMng methods.                         //
/////////////////////////////////////////////////////////////////////////////

//----------------------------------------------------------------------------
BOOL AskDialogInputMng::ExecAskRules(CWnd* pParent, BOOL bInitOnly)
{
	ASSERT(m_pSymTable);

	for (int j = 0; j < m_pSymTable->GetSize(); j++)
	{
		WoormField* pRepField = m_pSymTable->GetAt(j);
		if (pRepField)
		{
			if (pRepField->IsAsk())
				continue;

			DataObj* pDataObj = pRepField->GetData();
			if (pRepField->GetInitExpression() && !pRepField->GetInitExpression()->Eval(*pDataObj))
				return FALSE;
		}
	}

	if (bInitOnly)
	{
		for (int i = 0; i < m_pAskRuleData->GetCount(); i++)
			if (!m_pAskRuleData->GetAskDialog(i)->IsOnAsk() && !m_pAskRuleData->GetAskDialog(i)->InitAskFields())
				return FALSE;

		return TRUE;
	}

	return ExecAskRules(pParent);
}

//----------------------------------------------------------------------------
BOOL AskDialogInputMng::ExecAskRules(CWnd* pParent, ReportEngine* pReport/*= NULL*/, BOOL bDesignMode /*= FALSE*/, COutParamBag* pBag/*= NULL*/)
{
	ASSERT(m_pSymTable);

	return (m_pAskRuleData->GetCount() <= 0 || ExecAllAsk(pParent, pReport, bDesignMode, pBag));
}

//------------------------------------------------------------------------------------------------------------------------------------
BOOL AskDialogInputMng::ReturnExec(InputMng& inpMng, BOOL bReturn)
{
	inpMng.CleanUpAskDialogDataAndGroups();

	inpMng.ShowMessages();

	return bReturn;
}

//------------------------------------------------------------------------------------------------------------------------------------------
BOOL AskDialogInputMng::ExecAllActions(CWnd* pParent, ReportEngine* pReport /*= NULL*/, BOOL bDesignMode /*= FALSE*/, COutParamBag* pBag /*= NULL*/)
{
	return ExecAllAsk(pParent, pReport, bDesignMode, pBag, TRUE);
}

//-----------------------------------------------------------------------------------------------------------------------------------
BOOL AskDialogInputMng::ExecAllAsk(CWnd* pParent, ReportEngine* pReport /*= NULL*/, BOOL bDesignMode /*= FALSE*/, COutParamBag* pBag /*= NULL*/, BOOL bIsOnAsk /*= FALSE*/)
{
	CWoormDoc* pWoormDoc = dynamic_cast<CWoormDoc*>(m_pDocument);
	BOOL bFromWoorm = pReport && pWoormDoc;

	InputMng inpMng(m_pDocument, m_pSymTable);
	inpMng.m_bRunningFromExternalController = pReport && pReport->IsRunningFromExternalController();
	inpMng.SetDesignMode(bDesignMode ? InputMng::DESIGN : InputMng::RUNNING);
	inpMng.SetAllActions(m_AllActions);

	EngineScheduler aLocalScheduler;
	if (pReport && pReport->m_pScheduler && !bDesignMode)
		inpMng.m_pEngineScheduler = pReport->m_pScheduler;
	else
		inpMng.m_pEngineScheduler = &aLocalScheduler;
	inpMng.m_pEngineScheduler->Start();

	if (!LoadAllInputMng(&inpMng, bIsOnAsk))
		return ReturnExec(inpMng, FALSE);

	// only Input Dialog that isn't hidden are execute now
	if (!bDesignMode && pBag)
		inpMng.SetParameterDescription(pBag);

	//vedo se ho da eseguire qualche dialog - testo partendo dalla prima askdialog; se non ho niente non devo fare askmodal
	if (inpMng.m_AllAskDialogDataAndGroups.GetCount() > 0)
	{
		BOOL bFoundErrors = FALSE;
		CAskDialogDataAndGroups* pAskDialogDataAndGroups = inpMng.GetNextAskDialog(TRUE, bFoundErrors);

		if (bFoundErrors)
		{
			inpMng.ManageErrorLevel();

			if (!bDesignMode)
				return ReturnExec(inpMng, FALSE);
		}

		if (!pAskDialogDataAndGroups)     //significa che non ho nessuna ask dialog ?!
			return ReturnExec(inpMng, TRUE);
		
		// sincronizzazione con controllore esterno
		inpMng.SyncExternalControllerInfo(FALSE, pAskDialogDataAndGroups);
	}
	
	if (!inpMng.m_bRunningFromExternalController)
	{
		if (inpMng.m_AllAskDialogDataAndGroups.GetCount() > 0 && !inpMng.AskModalTiles(pParent))
		{
			if (bFromWoorm && !bDesignMode && (!m_pDocument || !m_pDocument->m_pExternalControllerInfo))
				inpMng.m_pEngineScheduler->SetRTError(EngineScheduler::QUERY_RULES_USER_BREAK);

			if (!bDesignMode)
				return ReturnExec(inpMng, FALSE);
		}

		//verifico se sono uscito con qualche errore
		if (bFromWoorm && inpMng.m_ErrorLevel != InputMng::NONE)
		{
			inpMng.ManageErrorLevel();

			if (!bDesignMode)
				return ReturnExec(inpMng, FALSE);
		}
	}
	// Se sono chiamato dal controller esterno non devo far comparire le dialog ma devo utilizzare 
	// le variabili fornite dal controller stesso
	else //non sto visualizzando le dialog, non posso usare l'evento di 'next'... devo scorrerle in un ciclo
	{
		while (inpMng.m_idxLastDialog < inpMng.m_AllAskDialogDataAndGroups.GetCount())
		{
			BOOL bFoundErrors = FALSE;
			int currentDlg = inpMng.m_idxLastDialog;
			if (currentDlg < 0 || !inpMng.CanDoNextAskDialog(currentDlg))
			{
				bFoundErrors = TRUE;
				if (inpMng.GetDesignMode() == InputMng::RUNNING)
					return NULL;
			}

			CAskDialogDataAndGroups* pAskDialogDataAndGroups = inpMng.GetNextAskDialog(TRUE,bFoundErrors);
			if (pAskDialogDataAndGroups)
				// sincronizzazione con controllore esterno
				inpMng.SyncExternalControllerInfo(FALSE, pAskDialogDataAndGroups);
			else
				break;		
		}
	}

	return !bDesignMode || ReturnExec(inpMng, TRUE);
}

//----------------------------------------------------------------------------
BOOL AskDialogInputMng::ExecAsk(CWnd* pParent, AskDialogData* pAskDialogData, ReportEngine* pReport/*= NULL*/, BOOL bDesignMode /*= FALSE*/, COutParamBag* pBag/*= NULL*/)
{
	CWoormDoc* pWoormDoc = dynamic_cast<CWoormDoc*>(m_pDocument);
	BOOL bFromWoorm = pReport && pWoormDoc;

	InputMng inpMng(m_pDocument, m_pSymTable);
	inpMng.m_bRunningFromExternalController = pReport && pReport->IsRunningFromExternalController();
	inpMng.SetDesignMode(bDesignMode ? InputMng::DESIGN : InputMng::RUNNING);

	EngineScheduler aLocalScheduler;
	if (pReport && pReport->m_pScheduler && !bDesignMode)
		inpMng.m_pEngineScheduler = pReport->m_pScheduler;
	else
		inpMng.m_pEngineScheduler = &aLocalScheduler;
	inpMng.m_pEngineScheduler->Start();

	if (!LoadInputMng(&inpMng, pAskDialogData))
		return ReturnExec(inpMng, FALSE);

	if (!bDesignMode && pBag)
		inpMng.SetParameterDescription(pBag);

	if (!bDesignMode)
	{
		if (pAskDialogData->m_pBeforeBlock && !pAskDialogData->m_pBeforeBlock->Exec())
		{
			if (bFromWoorm && !bDesignMode)
				inpMng.m_pEngineScheduler->SetRTError(EngineScheduler::EMPTY_MESSAGE);

			if (!bDesignMode)
				return ReturnExec(inpMng, FALSE);
		}

		// only Input Dialog that isn't hidden are execute now
		if (!inpMng.m_bRunningFromExternalController && pWoormDoc && (pWoormDoc->IsInUnattendedMode() || pWoormDoc->m_bHideAllAskDialogs))
		{
			inpMng.InitFieldsOfHiddenDialog(0);
			return ReturnExec(inpMng, TRUE);
		}

		// se esiste una espressione di when allora potrei anche non dover
		// eseguire la dialog nel caso sia false. Se ho un errore di valutazione
		// allora interrompo tutto.
		DataBool bWhenResult(TRUE);
		if (pAskDialogData->m_pWhenExpr && !pAskDialogData->m_pWhenExpr->Eval(bWhenResult))
		{
			if (bFromWoorm)
			{
				CString strErr;
				strErr.Format(_TB("Error in valuation of the visualization clause of a request rule {0-%s}"), pAskDialogData->GetName());
				inpMng.m_pEngineScheduler->SetRTError(EngineScheduler::REPORT_EVAL_WHEN_EXP, pAskDialogData->m_pWhenExpr->GetErrDescription(), strErr);
			}

			return ReturnExec(inpMng, FALSE);
		}

		if (!bWhenResult)
		{
			inpMng.InitFieldsOfHiddenDialog(0);
			return ReturnExec(inpMng, TRUE);
		}

		// preinizializzo la dialog con i dati (eventuali) forniti dal controllore esterno
		if (bFromWoorm)
			inpMng.SyncExternalControllerInfo(FALSE, inpMng.m_AllAskDialogDataAndGroups.GetAt(0));
	}
	// Se sono chiamato dal controller esterno non devo far comparire le dialog ma devo utilizzare 
	// le variabili fornite dal controller stesso
	if (!inpMng.m_bRunningFromExternalController)
	{
		//resetto il valore che in caso di più askdialog viene portato al valore corretto dalla 'GetNextAskDialog' 
		inpMng.m_idxLastDialog = 0;
		if (!inpMng.AskModalTiles(pParent))
		{
			if (bFromWoorm && !bDesignMode)
				inpMng.m_pEngineScheduler->SetRTError(EngineScheduler::QUERY_RULES_USER_BREAK);

			if (!bDesignMode)
				return ReturnExec(inpMng, FALSE);
		}

		// riverso le variabili nella cache dell'external controller (se c'è)
		//if (!bDesignMode && bFromWoorm)
		//	inpMng.SyncExternalControllerInfo(TRUE, inpMng.m_AllAskDialogDataAndGroups.GetAt(0));
	}

	if (!bDesignMode)
	{

		if (pAskDialogData->m_pAfterBlock && !pAskDialogData->m_pAfterBlock->Exec())
		{
			if (bFromWoorm && !bDesignMode)
				inpMng.m_pEngineScheduler->SetRTError(EngineScheduler::EMPTY_MESSAGE);

			if (!bDesignMode)
				return ReturnExec(inpMng, FALSE);
		}

		DataBool bAbortRun(FALSE);
		if ((!bFromWoorm || !pReport->IsEditingParamsFromExternalController()) && pAskDialogData->m_pOnExpr && !pAskDialogData->m_pOnExpr->Eval(bAbortRun))
		{
			if (bFromWoorm)
			{
				CString strErr;
				strErr.Format(_TB("Error in valuation of the validation clause of a request rule {0-%s}"), pAskDialogData->GetName());
				inpMng.m_pEngineScheduler->SetRTError(EngineScheduler::REPORT_EVAL_ON_EXP, pAskDialogData->m_pOnExpr->GetErrDescription(), strErr);
			}

			if (!bDesignMode)
				return ReturnExec(inpMng, FALSE);
		}

		if (bAbortRun)
		{
			DataStr sAbortMsg;
			if (pAskDialogData->m_pAbortExpr && !pAskDialogData->m_pAbortExpr->Eval(sAbortMsg))
			{
				if (bFromWoorm)
				{
					CString strErr;
					strErr.Format(_TB("Error in valuation of the expression of a request rule {0-%s}"), pAskDialogData->GetName());
					inpMng.m_pEngineScheduler->SetRTError(EngineScheduler::REPORT_EVAL_ONABORTMESSAGE_EXP, pAskDialogData->m_pAbortExpr->GetErrDescription(), strErr);
				}

				if (!bDesignMode)
					return ReturnExec(inpMng, FALSE);
			}

			//Non è un errore dell'engine, ma una segnalazione di errore applicativo (analoga al comando di script "Abort")
			if (bFromWoorm)
				inpMng.m_pEngineScheduler->SetRTError(EngineScheduler::REPORT_DIALOG_ONABORT_MESSAGE, sAbortMsg.GetString());

			if (!bDesignMode)
				return ReturnExec(inpMng, FALSE);
		}
	}
	
	return !bDesignMode || ReturnExec(inpMng, TRUE);
}

//----------------------------------------------------------------------------
BOOL AskDialogInputMng::LoadInputMng(InputMng* pInpMng, AskDialogData* pAskDialogData)
{
	ASSERT(!pInpMng->m_bOwnsAskDialogData);
	ASSERT(m_pSymTable);
	
	pInpMng->CleanUpAskDialogDataAndGroups();
	pInpMng->m_AllAskDialogDataAndGroups.Add(new CAskDialogDataAndGroups(pAskDialogData));

	if (!pInpMng->m_AllAskDialogDataAndGroups.GetAt(0)->m_pAskDialogData && m_pAskRuleData)
		pInpMng->m_AllAskDialogDataAndGroups.GetAt(0)->m_pAskDialogData = m_pAskRuleData->GetAskDialog(0);

	if (pInpMng->m_pSymTable == NULL)
		pInpMng->m_pSymTable = m_pSymTable;

	for (int nIdxGrp = 0; nIdxGrp < pInpMng->m_AllAskDialogDataAndGroups.GetAt(0)->m_pAskDialogData->m_AskGroups.GetSize(); nIdxGrp++)
	{
		AskGroupData* pTmpAskGroup = pInpMng->m_AllAskDialogDataAndGroups.GetAt(0)->m_pAskDialogData->GetAskGroup(nIdxGrp);
		if (pTmpAskGroup == NULL || pTmpAskGroup->IsEmpty())
			continue;

		AskGroup* pInputGroup = new AskGroup(pInpMng, pTmpAskGroup);
		pInpMng->m_AllAskDialogDataAndGroups.GetAt(0)->m_Groups.Add(pInputGroup);
	
		for (int idxFld = 0; idxFld < pTmpAskGroup->m_AskFields.GetSize(); idxFld++)
		{
			AskFieldData* pTmpAskField = pTmpAskGroup->GetAskField(idxFld);
			if (pTmpAskField == NULL)
				continue;
			
			if (!pInputGroup->AddEntry(pTmpAskField, m_pSymTable->GetField(pTmpAskField->m_strPublicName)))
				return FALSE;
		}
	}

	return TRUE;
}

//---------------------------------------------------------------------------------------------------
BOOL AskDialogInputMng::IsExistingAction(CString aAction)
{
	for (int i = 0; i < m_AllActions.GetCount(); i++)
		if (m_AllActions.GetAt(i) == aAction)
			return TRUE;

	return FALSE;
}

//---------------------------------------------------------------------------------------------------
BOOL AskDialogInputMng::LoadAllInputMng(InputMng* pInpMng, BOOL bIsOnAsk)
{
	ASSERT(m_pSymTable);

	pInpMng->CleanUpAskDialogDataAndGroups();

	for (int i = 0; i < pInpMng->m_AllAskDialogDataAndGroups.GetCount(); i++)
		SAFE_DELETE(pInpMng->m_AllAskDialogDataAndGroups.GetAt(i));

	pInpMng->m_AllAskDialogDataAndGroups.RemoveAll();
	
	for (int i = 0; i < m_pAskRuleData->GetCount(); i++)
	{
		AskDialogData* pAskDialogData = m_pAskRuleData->GetAskDialog(i);
		
		if ((!bIsOnAsk && pAskDialogData->IsOnAsk() /*!= bIsOnAsk*/) || (bIsOnAsk && !IsExistingAction(pAskDialogData->GetName())))
			continue;

		CAskDialogDataAndGroups* pAllData = new CAskDialogDataAndGroups();

		pAllData->m_idxArray = pInpMng->m_AllAskDialogDataAndGroups.Add(pAllData);

		pAllData->m_pAskDialogData = pAskDialogData;
		
		if (pInpMng->m_pSymTable == NULL)
			pInpMng->m_pSymTable = m_pSymTable;

		for (int nIdxGrp = 0; nIdxGrp < pAllData->m_pAskDialogData->m_AskGroups.GetSize(); nIdxGrp++)
		{
			AskGroupData* pTmpAskGroup = pAllData->m_pAskDialogData->GetAskGroup(nIdxGrp);
			if (pTmpAskGroup == NULL || pTmpAskGroup->IsEmpty())
				continue;

			AskGroup* pInputGroup = new AskGroup(pInpMng, pTmpAskGroup);
			pAllData->m_Groups.Add(pInputGroup);

			for (int idxFld = 0; idxFld < pTmpAskGroup->m_AskFields.GetSize(); idxFld++)
			{
				AskFieldData* pTmpAskField = pTmpAskGroup->GetAskField(idxFld);
				if (pTmpAskField == NULL)
					continue;

				if (!pInputGroup->AddEntry(pTmpAskField, m_pSymTable->GetField(pTmpAskField->m_strPublicName)))
				{
					ASSERT(FALSE);
					return FALSE;
				}
			}
		}

	}

	return TRUE;
}
