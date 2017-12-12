// ConditionalReadOnlyFormDoc.cpp : implementation file
//

#include "stdafx.h"

#include <TbFrameworkImages\GeneralFunctions.h>
#include <TbFrameworkImages\CommonImages.h>
#include <TbGeneric\dibitmap.h>
#include <TbGes\HotLink.h>
#include <TbGenlib\BaseTileDialog.h>
#include <TbGenlib\PARSBTN.H>
#include <TbGenlib\PARSEDT.H>
#include <TbGenlib\ParsEdtOther.h>
#include <TbGenlib\HyperLink.h>


#include "TBPropertyGrid.h"
#include "basedoc.h"
//includere come ultimo include all'inizio del cpp
#include "begincpp.dex"

#ifdef _DEBUG
#undef THIS_FILE
static const char BASED_CODE THIS_FILE[] = __FILE__;
#endif

//========================================================================
IMPLEMENT_DYNAMIC(CTBProperty, CBCGPProp)

//---------------------------------------------------------------------------------------------------
CTBProperty::CTBProperty
(
	CString sName,
	CString sPropertyLeftText,
	CString sPropertyBottomText
)
	:CBCGPProp(sPropertyLeftText),
	IDisposingSourceImpl(this),
	m_sObjectName(sName),
	m_bHotLinkInline(FALSE),
	m_pParsedCtrl(NULL),
	m_bViewHKLCode(FALSE),
	m_bViewHKLDescription(FALSE),
	m_pCheckBitmap(NULL),
	m_bForcedAlwaysEnabled(FALSE),
	m_clrGroupBackground(EMPTY_COLOR),
	m_clrGroupText(EMPTY_COLOR),
	m_oldclrGroupBackground(EMPTY_COLOR),
	m_oldclrGroupText(EMPTY_COLOR)
{
	InitCollapseExpandImages();
	m_strDescr = sPropertyBottomText;

}

//----------------------------------------------------------------------------------------------
CTBProperty::CTBProperty
(
	CString sName,
	CString sPropertyLeftText,
	CString sPropertyBottomText,
	const _variant_t& varValue
)
	:
	CBCGPProp(sPropertyLeftText, varValue, sPropertyBottomText),
	IDisposingSourceImpl(this),
	m_sObjectName(sName),
	m_bHotLinkInline(FALSE),
	m_pParsedCtrl(NULL),
	m_bViewHKLCode(FALSE),
	m_bViewHKLDescription(FALSE),
	m_pCheckBitmap(NULL),
	m_bForcedAlwaysEnabled(FALSE),
	m_clrGroupBackground(EMPTY_COLOR),
	m_clrGroupText(EMPTY_COLOR),
	m_oldclrGroupBackground(EMPTY_COLOR),
	m_oldclrGroupText(EMPTY_COLOR)
{
	InitCollapseExpandImages();
	m_strDescr = sPropertyBottomText;
}

//-----------------------------------------------------------------------------------------------
CTBProperty::~CTBProperty()
{
	m_pWndCombo = NULL;
	m_pWndInPlace = NULL;
	m_pWndSpin = NULL;

	if (m_pParsedCtrl)
	{
		m_pParsedCtrl->GetCtrlCWnd()->DestroyWindow();
		delete m_pParsedCtrl;
		m_pParsedCtrl = NULL;
	}

	SAFE_DELETE(m_pCheckBitmap);

	if (m_ExpandedImage.m_hObject)
		m_ExpandedImage.Detach();
	if (m_CollapsedImage.m_hObject)
		m_CollapsedImage.Detach();
}

//---------------------------------------------------------------------------------------
void CTBProperty::InitCollapseExpandImages()
{
	ASSERT(!m_CollapsedImage.m_hObject);
	ASSERT(!m_ExpandedImage.m_hObject);
	ASSERT(!m_CollapsedImageSubGroups.m_hObject);
	ASSERT(!m_ExpandedImageSubGroups.m_hObject);

	TBThemeManager* pThemeManager = AfxGetThemeManager();

	COLORREF clrGroupBkg = pThemeManager->GetPropertyGridGroupBkgColor();
	COLORREF clrSubGroupsBkg = pThemeManager->GetPropertyGridSubGroupsBkgColor();

	CString sCollapseImage = pThemeManager->GetPropertyGridGroupCollapseImage();
	CString sExpandImage = pThemeManager->GetPropertyGridGroupExpandImage();
	if (!sCollapseImage.IsEmpty())
	{
		LoadBitmapOrPng(&m_ExpandedImage, sCollapseImage, clrGroupBkg);
		if (clrSubGroupsBkg != COLORREF(-1))
			LoadBitmapOrPng(&m_ExpandedImageSubGroups, sCollapseImage, clrSubGroupsBkg);
	}

	if (!sExpandImage.IsEmpty())
	{
		LoadBitmapOrPng(&m_CollapsedImage, sExpandImage, clrGroupBkg);
		if (clrSubGroupsBkg != COLORREF(-1))
			LoadBitmapOrPng(&m_CollapsedImageSubGroups, sExpandImage, clrSubGroupsBkg);
	}
}

//-----------------------------------------------------------------------------------------------
void CTBProperty::OnDestroyWindow()
{
	m_pWndCombo = NULL;
	m_pWndInPlace = NULL;
	m_pWndSpin = NULL;

	__super::OnDestroyWindow();
}

//-----------------------------------------------------------------------------------------------
void CTBProperty::SetHotLinkInline(BOOL bViewHKLCode /*= TRUE*/, BOOL bViewHKLDescription /*= TRUE*/)
{
	m_bHotLinkInline = TRUE;
	m_bViewHKLCode = bViewHKLCode;
	m_bViewHKLDescription = bViewHKLDescription;
}

//-----------------------------------------------------------------------------------------------
void CTBProperty::SetControl(CParsedCtrl* pCtrl)
{
	ASSERT(m_pParsedCtrl == NULL);
	m_pParsedCtrl = pCtrl;

	if (pCtrl->GetCtrlCWnd()->IsKindOf(RUNTIME_CLASS(CBoolButton)))
		m_pCheckBitmap = new CCheckBitmap(TBGlyph(szIconCheck));
}

//----------------------------------------------------------------------------------------------------
void CTBProperty::DestroyAndCreateFB(CFont* fromFont, BOOL bUnderline)
{
	if (((CTBPropertyGrid*)m_pWndList)->m_fontBold.GetSafeHandle() != NULL)
	{
		((CTBPropertyGrid*)m_pWndList)->m_fontBold.DeleteObject();
	}

	LOGFONT lf;
	memset(&lf, 0, sizeof(LOGFONT));
	fromFont->GetLogFont(&lf);
	lf.lfUnderline = bUnderline;
	((CTBPropertyGrid*)m_pWndList)->m_fontBold.CreateFontIndirect(&lf);
}

//--------------------------------------------------------------------------------------------------
COLORREF CTBProperty::GetGroupBackground()
{
	return m_clrGroupBackground;
}

//--------------------------------------------------------------------------------------------------
COLORREF CTBProperty::GetrGroupText()
{
	return m_clrGroupText;
}

//--------------------------------------------------------------------------------------------------
void CTBProperty::SetGroupBackground(COLORREF clr)
{
	m_clrGroupBackground = clr;
}

//--------------------------------------------------------------------------------------------------
void CTBProperty::SetrGroupText(COLORREF clr)
{
	m_clrGroupText = clr;
}
//--------------------------------------------------------------------------------------------------
CWndObjDescription* CTBProperty::GetControlDescription(CWndObjDescriptionContainer* pContainer, int index)
{
	CString strId = AfxGetTBResourcesMap()->DecodeID(TbControls, m_nID).m_strName;
	CWndObjDescription* pDesc = pContainer->GetWindowDescription(NULL, RUNTIME_CLASS(CWndPropertyGridItemDescription), strId);
	pDesc->m_Type = CWndObjDescription::PropertyGridItem;
	pDesc->SetID(strId);
	pDesc->m_strName = m_sObjectName;
	pDesc->m_strText = m_strName;
	pDesc->m_strHint = m_strDescr;
	for (int i = 0; i < GetSubItemsCount(); i++)
	{
		CTBProperty* pProp = (CTBProperty*)GetSubItem(i);
		pProp->GetControlDescription(&pDesc->m_Children, i);
	}
	for (int i = pDesc->m_Children.GetUpperBound(); i >= 0; i--)
	{
		CWndObjDescription* pChild = pDesc->m_Children.GetAt(i);
		if (pChild->IsRemoved())
			pDesc->m_Children.RemoveAt(i);
	}
	return pDesc;
}

//--------------------------------------------------------------------------------------------------
void CTBProperty::OnDrawValue(CDC* pDC, CRect rect)
{
	COLORREF originalTextValueColor = m_clrTextValue;

	if ((IsGroup() && !m_bIsValueList) || !HasValueField())
	{
		return;
	}

	if (m_pParsedCtrl && m_pParsedCtrl->GetCtrlCWnd()->IsKindOf(RUNTIME_CLASS(CBoolButton)))
	{
		if (!m_bInPlaceEdit)
		{
			int h = (rect.bottom - rect.top - m_pCheckBitmap->GetHeight()) / 2;

			m_pCheckBitmap->FloodDrawBitmap
			(
				pDC->m_hDC,
				rect.left + 2 + 2,
				rect.top + 2 + h,
				SRCCOPY,
				m_clrTextValue,
				(COLORREF)0xFFFFFF,
				(BOOL)*((DataBool*)m_pParsedCtrl->GetCtrlData()),
				TRUE
			);
		}
		return;
	}

	CFont* pOriginalFont = pDC->GetCurrentFont();

	if (
		m_pParsedCtrl &&
		(
			m_pParsedCtrl->GetHotLink() && !m_pParsedCtrl->GetHotLink()->GetAddOnFlyNamespace().IsEmpty() ||
			m_pParsedCtrl->GetCtrlCWnd()->IsKindOf(RUNTIME_CLASS(CLinkEdit)) && m_pParsedCtrl->GetCtrlData() && !m_pParsedCtrl->GetCtrlData()->IsEmpty()
			)
		)
	{
		if (!m_bEnabled)
		{
			m_clrTextValue = ((CTBPropertyGrid*)m_pWndList)->m_clrHyperLinkForeColor;

			pDC->SelectObject(((CTBPropertyGrid*)m_pWndList)->m_pHyperlinkFont);

			if (IsModified() && ((CTBPropertyGrid*)m_pWndList)->m_bMarkModifiedProperties)
			{
				DestroyAndCreateFB(pOriginalFont, TRUE);
			}
		}
	}
	else
	{
		pDC->SelectObject(pOriginalFont);

		if (IsModified() && ((CTBPropertyGrid*)m_pWndList)->m_bMarkModifiedProperties && !m_bEnabled)
		{
			DestroyAndCreateFB(pOriginalFont, FALSE);
		}
	}

	if (
		m_pParsedCtrl->GetCtrlData() &&
		(
			m_pParsedCtrl->GetCtrlData()->GetDataType() == DATA_INT_TYPE ||
			m_pParsedCtrl->GetCtrlData()->GetDataType() == DATA_LNG_TYPE ||
			m_pParsedCtrl->GetCtrlData()->GetDataType() == DATA_DBL_TYPE ||
			m_pParsedCtrl->GetCtrlData()->GetDataType() == DATA_MON_TYPE ||
			m_pParsedCtrl->GetCtrlData()->GetDataType() == DATA_QTA_TYPE ||
			m_pParsedCtrl->GetCtrlData()->GetDataType() == DATA_PERC_TYPE
			)
		)
	{
		CFont* pOldFont = NULL;
		if (IsModified() && ((CTBPropertyGrid*)m_pWndList)->m_bMarkModifiedProperties)
		{
			pOldFont = pDC->SelectObject(&((CTBPropertyGrid*)m_pWndList)->m_fontBold);
		}

		CString strVal = FormatProperty();

		// DECISO DA ELSA E PATRIZIA
		//CSize aSize = m_pParsedCtrl->AdaptNewSize(10, 1, TRUE);
		//rect.right = rect.left + (aSize.cx - m_pParsedCtrl->GetAllButtonsWitdh());

		rect.DeflateRect(TEXT_MARGIN, 0);

		COLORREF clrOldText = (COLORREF)-1;
		if (m_clrTextValue != (COLORREF)-1)
		{
			clrOldText = pDC->SetTextColor(m_clrTextValue);
		}

		pDC->DrawText(strVal, rect,
			DT_LEFT | DT_SINGLELINE | DT_VCENTER | DT_NOPREFIX | DT_END_ELLIPSIS);

		if (clrOldText != (COLORREF)-1)
		{
			pDC->SetTextColor(clrOldText);
		}

		m_bValueIsTrancated = pDC->GetTextExtent(strVal).cx > rect.Width();

		if (pOldFont != NULL)
		{
			pDC->SelectObject(pOldFont);
		}
	}
	else
		__super::OnDrawValue(pDC, rect);

	m_clrTextValue = originalTextValueColor;
	pDC->SelectObject(pOriginalFont);
	((CTBPropertyGrid*)m_pWndList)->CreateBoldFont();
}

//-----------------------------------------------------------------------------------------------
BOOL CTBProperty::RemoveSubItem(CTBProperty*& pProp, BOOL bDelete /*= TRUE*/)
{
	return __super::RemoveSubItem((CBCGPProp*&)pProp, bDelete);
}

//-----------------------------------------------------------------------------------------------
void CTBProperty::RemoveAllSubItems()
{
	__super::RemoveAllSubItems();
}

//-----------------------------------------------------------------------------------------------
void CTBProperty::SetGroup(BOOL isGroup)
{
	this->m_bGroup = isGroup;
}

CTBProperty* CTBProperty::AddSubProperty(CTBProperty* pChildProperty)
{
	BOOL bOk = FALSE;
	if (pChildProperty)
	{
		if (this->m_bGroup)
		{
			bOk = __super::AddSubItem(pChildProperty);
		}
		else
		{
			SetGroup(TRUE);

			bOk = __super::AddSubItem(pChildProperty);

			pChildProperty->SetOwnerList(m_pWndList);

			SetGroup(FALSE);
		}
	}
	ASSERT(bOk);
	return pChildProperty;
}

//------------------------------------------------------------------------------
BOOL CTBProperty::OnUpdateValue()
{
	ASSERT_VALID(this);

	if (!m_pWndInPlace)
		return FALSE;

	return __super::OnUpdateValue();
}

//-----------------------------------------------------------------------------------------------
void CTBProperty::UpdateStatus()
{
	if (m_pParsedCtrl)
		m_pParsedCtrl->ReadPropertiesFromJson();
	if (IsGroup())
		__super::Enable(TRUE);
	else
		__super::Enable(m_bForcedAlwaysEnabled || !m_pParsedCtrl->GetCtrlData()->IsReadOnly() && !m_pParsedCtrl->GetCtrlData()->IsAlwaysReadOnly());

	for (POSITION pos = m_lstSubItems.GetHeadPosition(); pos != NULL;)
	{
		CTBProperty* pProp = DYNAMIC_DOWNCAST(CTBProperty, m_lstSubItems.GetNext(pos));
		if (!pProp)
			continue;

		ASSERT_VALID(pProp);
		pProp->UpdateStatus();
	}
}

//-----------------------------------------------------------------------------------------------
void CTBProperty::SetDataReadOnly(BOOL bRO)
{
	if (m_pParsedCtrl)
		m_pParsedCtrl->SetDataReadOnly(bRO);

	for (POSITION pos = m_lstSubItems.GetHeadPosition(); pos != NULL;)
	{
		CTBProperty* pProp = DYNAMIC_DOWNCAST(CTBProperty, m_lstSubItems.GetNext(pos));
		if (!pProp)
			continue;

		ASSERT_VALID(pProp);
		pProp->SetDataReadOnly(bRO);
	}
}

//-----------------------------------------------------------------------------------------------
void CTBProperty::DoValueChanged()
{
	if (m_pParsedCtrl)
	{
		CWnd* pOldWndInPlace = m_pWndInPlace;
		if (!pOldWndInPlace)
			m_pWndInPlace = m_pParsedCtrl->GetCtrlCWnd();

		OnUpdateValue();

		if (!m_pParsedCtrl->GetOldCtrlData())
			m_bIsModified = FALSE;
		else if (*m_pParsedCtrl->GetOldCtrlData() != *m_pParsedCtrl->GetCtrlData())
			m_bIsModified = TRUE;
		if (m_pParsedCtrl->GetDocument())
		{
			if (!m_pParsedCtrl->GetDocument()->IsModified() && m_bIsModified)
				m_pParsedCtrl->GetDocument()->SetModifiedFlag(m_bIsModified);
		}
		if (!pOldWndInPlace)
			m_pWndInPlace = NULL;
	}
}

//-----------------------------------------------------------------------------------------------------------------
void CTBProperty::DoValuesChanged()
{
	DoValueChanged();

	if (!m_pParsedCtrl || !m_pParsedCtrl->GetCtrlCWnd()->IsKindOf(RUNTIME_CLASS(CBoolButton)))
		SetValue((_variant_t)FormatProperty());

	for (POSITION pos = m_lstSubItems.GetHeadPosition(); pos != NULL;)
	{
		CTBProperty* pProp = DYNAMIC_DOWNCAST(CTBProperty, m_lstSubItems.GetNext(pos));
		if (!pProp)
			continue;

		ASSERT_VALID(pProp);
		pProp->DoValuesChanged();
	}
}

//-----------------------------------------------------------------------------------------------------------------
void CTBProperty::SetDataModified(BOOL bMod)
{
	if (m_pParsedCtrl && m_pParsedCtrl->CanResetStatus())
		m_pParsedCtrl->SetDataModified(bMod);

	for (POSITION pos = m_lstSubItems.GetHeadPosition(); pos != NULL;)
	{
		CTBProperty* pProp = DYNAMIC_DOWNCAST(CTBProperty, m_lstSubItems.GetNext(pos));
		if (!pProp)
			continue;

		ASSERT_VALID(pProp);
		pProp->SetDataModified(bMod);
	}
}

//-------------------------------------------------------------------------------------------------------------
BOOL CTBProperty::OnEdit(LPPOINT lptClick)
{
	if (!m_bEnabled)
	{
		if (m_pParsedCtrl && m_pParsedCtrl->GetHyperLink())
		{
			ReleaseCapture();
			m_pParsedCtrl->GetHyperLink()->DoFollowHyperlink(m_pParsedCtrl->GetCtrlData());
		}
		else if (m_pParsedCtrl->GetCtrlCWnd()->IsKindOf(RUNTIME_CLASS(CLinkEdit)))
		{
			DataStr* pD = (DataStr*)m_pParsedCtrl->GetCtrlData();
			if (pD && !pD->IsEmpty())
			{
				CLinkEdit* pNsCtrl = (CLinkEdit*)m_pParsedCtrl->GetCtrlCWnd();
				pNsCtrl->SetValue(pD->GetString());
				ReleaseCapture();
				pNsCtrl->OnBrowseLink();
			}
		}

		return FALSE;
	}

	return __super::OnEdit(lptClick);
}

//-----------------------------------------------------------------------------------------------
CWnd* CTBProperty::CreateInPlaceEdit(CRect rectEdit, BOOL& bDefaultFormat)
{
	if (m_pParsedCtrl)
	{
		m_bInPlaceEdit = TRUE;
		m_pParsedCtrl->ShowCtrl(SW_SHOW);

		rectEdit.right = rectEdit.left + (rectEdit.Width() - m_pParsedCtrl->GetAllButtonsWitdh());

		// DECISO DA ELSA E PATRIZIA
		//if (m_pParsedCtrl->GetCtrlData() && m_pParsedCtrl->GetCtrlData()->GetDataType() != DATA_STR_TYPE)
		//{
		//	CSize aSize = m_pParsedCtrl->AdaptNewSize(10, 1, TRUE);
		//	rectEdit.right = rectEdit.left + (aSize.cx - m_pParsedCtrl->GetAllButtonsWitdh());
		//}

		if (m_pParsedCtrl->GetCtrlCWnd()->IsKindOf(RUNTIME_CLASS(CBoolButton)))
		{
			rectEdit.top += (rectEdit.Height() - 14) / 2 - 1; //14 is the checkbox square height
			rectEdit.left += 2;

			// cambia il valore del check se clickato con il mouse
			if (((CTBPropertyGrid*)m_pWndList)->m_bLButtonDownHit && m_pParsedCtrl->GetCtrlData())
			{
				// Nel caso la colonna selezionata sia associata ad un CBoolButton
				// la transizione di stato del valore viene forzata immediatamente
				// all'atto del click del mouse sulla colonna stessa

				// corrente valore del DataBool associato
				BOOL bValue = (BOOL)*((DataBool*)m_pParsedCtrl->GetCtrlData());

				// viene usata la SetCheck di CButton (MFC) poiche` la SetValue di CBoolButton (TB)
				// non invia il messaggio di VALUE_CHANGED
				//
				((CBoolButton*)(m_pParsedCtrl->GetCtrlCWnd()))->SetCheck(bValue ? 0 : 1);
			}
		}
		else
			if (m_pParsedCtrl->GetCtrlCWnd()->IsKindOf(RUNTIME_CLASS(CComboBox)))
				rectEdit.top -= 2;

		m_pParsedCtrl->GetCtrlCWnd()->SetWindowPos(NULL, rectEdit.left, rectEdit.top, rectEdit.Width(), rectEdit.Height(), SWP_NOZORDER);

		//if (m_pParsedCtrl->GetCtrlCWnd()->IsKindOf(RUNTIME_CLASS(CComboBox)))
		//{
		//	CComboBox* pCombo = (CComboBox*)m_pParsedCtrl->GetCtrlCWnd();
		//	VERIFY(pCombo->SetItemHeight(-1, rectEdit.Height() - 2) != CB_ERR);
		//}

		m_pParsedCtrl->UpdateCtrlStatus();
		m_pParsedCtrl->UpdateCtrlView();

		if (
			m_pParsedCtrl->GetCtrlCWnd() &&
			m_pParsedCtrl->GetCtrlData() &&
			(
				m_pParsedCtrl->GetCtrlData()->GetDataType() == DATA_INT_TYPE ||
				m_pParsedCtrl->GetCtrlData()->GetDataType() == DATA_LNG_TYPE ||
				m_pParsedCtrl->GetCtrlData()->GetDataType() == DATA_DBL_TYPE ||
				m_pParsedCtrl->GetCtrlData()->GetDataType() == DATA_MON_TYPE ||
				m_pParsedCtrl->GetCtrlData()->GetDataType() == DATA_QTA_TYPE ||
				m_pParsedCtrl->GetCtrlData()->GetDataType() == DATA_PERC_TYPE
				)
			)
		{
			CEdit* pEdit = dynamic_cast<CEdit*>(m_pParsedCtrl->GetCtrlCWnd());
			if (pEdit)
			{
				pEdit->ModifyStyle(ES_RIGHT, ES_LEFT);
			}
		}

		// bDefaultFormat = TRUE;
		return m_pParsedCtrl->GetCtrlCWnd();
	}

	return __super::CreateInPlaceEdit(rectEdit, bDefaultFormat);
}

//-----------------------------------------------------------------------------------------------
CString CTBProperty::FormatProperty()
{
	if (m_pParsedCtrl)
	{
		if (m_pParsedCtrl->GetCtrlData()->IsEmpty())
			return m_pParsedCtrl->FormatData(m_pParsedCtrl->GetCtrlData());

		if (m_pParsedCtrl->GetHotLink())
			return OnFormatHotLinkDescription();

		if (
			m_pParsedCtrl->GetCtrlCWnd()->IsKindOf(RUNTIME_CLASS(CStrEdit)) &&
			(m_pParsedCtrl->GetCtrlCWnd()->GetStyle() & ES_PASSWORD) == ES_PASSWORD &&
			!m_bInPlaceEdit
			)
			return _T("***");

		return m_pParsedCtrl->FormatData(m_pParsedCtrl->GetCtrlData());
	}

	return __super::FormatProperty();

}

//-----------------------------------------------------------------------------------------------
CString	CTBProperty::OnFormatHotLinkDescription()
{
	if (!m_pParsedCtrl)
		return _T("");
	HotKeyLinkObj* pHKL = m_pParsedCtrl->GetHotLink();
	if (pHKL->IsAutoFindable())
	{
		DataObj* pData = pHKL->GetAttachedData();
		if (pHKL->FindNeeded(pData, pHKL->GetMasterRecord()))
		{
			pHKL->OnPrepareForFind(pHKL->GetMasterRecord());
			pHKL->DoFindRecord(pData);
		}
	}
	CString sDescri;

	CString sName = pHKL->GetDescriptionField();
	if (!sName.IsEmpty())
	{
		DataObj* pObj = pHKL->GetField(sName);
		if (pObj)
			sDescri = pObj->FormatData();

	}

	if (!m_bHotLinkInline || (!m_bViewHKLCode && !m_bViewHKLDescription) || !m_bViewHKLDescription || (m_bViewHKLDescription && sDescri.IsEmpty()))
		return m_pParsedCtrl->GetCtrlData()->FormatData();

	if (m_bViewHKLCode && m_bViewHKLDescription)
		return cwsprintf(_T("%s %s"), m_pParsedCtrl->FormatData(m_pParsedCtrl->GetCtrlData()), pHKL->GetHKLDescription());

	return m_pParsedCtrl->GetCtrlData()->FormatData();
}

//-----------------------------------------------------------------------------------------------
BOOL CTBProperty::OnEndEdit()
{
	TRACE1("\r\nCTBProperty::OnEndEdit : %s\r\n", m_strName);

	BOOL bOk = __super::OnEndEdit();

	if (m_pParsedCtrl)
	{
		((CTBPropertyGrid*)m_pWndList)->m_bSetFocusOnly = TRUE;
		m_pParsedCtrl->ShowCtrl(SW_HIDE);
	}

	return bOk;

}

//-----------------------------------------------------------------------------------------------
CTBProperty* CTBProperty::GetNextEditableProperty(CList<CBCGPProp*, CBCGPProp*>* pLstProp, POSITION& pos)
{
	CTBProperty* pProp;
	POSITION posInner = m_lstSubItems.GetHeadPosition();
	if (posInner)
	{
		pProp = DYNAMIC_DOWNCAST(CTBProperty, m_lstSubItems.GetNext(posInner));
		if (!pProp)
			return NULL;

		if (pProp->GetControl() && pProp->IsVisibleInFilter())
			return pProp;

		pProp = pProp->GetNextEditableProperty(&m_lstSubItems, posInner);
		if (pProp)
			return pProp;
	}

	if (pos == NULL)
		return NULL;

	pProp = DYNAMIC_DOWNCAST(CTBProperty, pLstProp->GetNext(pos));
	if (pProp && pProp->GetControl() && pProp->IsVisibleInFilter())
		return pProp;

	return pProp ? pProp->GetNextEditableProperty(pLstProp, pos) : NULL;
}

//-----------------------------------------------------------------------------------------------
CTBProperty* CTBProperty::GetNearlySubProperty(CTBProperty* pCurrProp, CTBProperty*& pPrevProp, BOOL bPrev)
{
	CTBProperty* pSubProp = NULL;
	for (POSITION pos = m_lstSubItems.GetHeadPosition(); pos != NULL;)
	{
		if (pSubProp && pSubProp->GetControl() && pSubProp->IsVisibleInFilter())
			pPrevProp = pSubProp;

		pSubProp = DYNAMIC_DOWNCAST(CTBProperty, m_lstSubItems.GetNext(pos));
		if (!pSubProp)
			continue;

		if (pSubProp != pCurrProp)
		{
			if (pSubProp->GetControl() && pSubProp->IsVisibleInFilter())
				pPrevProp = pSubProp;

			pSubProp = pSubProp->GetNearlySubProperty(pCurrProp, pPrevProp, bPrev);
		}

		if (pSubProp == pCurrProp)
		{
			if (bPrev)
				return pPrevProp;
			else
			{
				pSubProp = pSubProp->GetNextEditableProperty(&m_lstSubItems, pos);
				return pSubProp ? pSubProp : pCurrProp;
			}
		}

		if (pSubProp)
			return pSubProp;
	}

	return NULL;
}

//-----------------------------------------------------------------------------------------------
void CTBProperty::OnDrawExpandBox(CDC* pDC, CRect rectExpand)
{
	// se non ho le immagini eseguo il codice di default dei BCG
	if (!m_ExpandedImage.m_hObject || !m_CollapsedImage.m_hObject)
		return __super::OnDrawExpandBox(pDC, rectExpand);

	CDC	dcImage;
	if (!dcImage.CreateCompatibleDC(pDC))
		return;
	HGDIOBJ old;
	if (GetHierarchyLevel() > 0)
		old = dcImage.SelectObject(m_bExpanded ? m_ExpandedImageSubGroups : m_CollapsedImageSubGroups);
	else
		old = dcImage.SelectObject(m_bExpanded ? m_ExpandedImage : m_CollapsedImage);

	pDC->BitBlt(rectExpand.left, rectExpand.top, rectExpand.Width(), rectExpand.Height(), &dcImage, 0, 0, SRCCOPY);

	dcImage.SelectObject(old);
	dcImage.DeleteDC();

}

//-------------------------------------------------------------------------------------------------
void CTBProperty::OnBeforeDrawProperty()
{
	m_clrGroupBackground = -1;
	m_oldclrGroupText = -1;

	CTBPropertyGrid* pTBGrid = dynamic_cast<CTBPropertyGrid*>(m_pWndList);
	if (!pTBGrid)
		return;

	if (m_clrGroupText != (-1))
		m_oldclrGroupText = pTBGrid->SetGroupTextColor(m_clrGroupText);

	if (m_clrGroupBackground != (-1))
		m_oldclrGroupBackground = pTBGrid->SetGroupTextColor(m_clrGroupBackground);
}

//-------------------------------------------------------------------------------------------------
void CTBProperty::OnAfterDrawProperty()
{
	CTBPropertyGrid* pTBGrid = dynamic_cast<CTBPropertyGrid*>(m_pWndList);
	if (!pTBGrid)
		return;

	if (m_oldclrGroupBackground != (-1))
		pTBGrid->SetGroupBackgroundColor(m_oldclrGroupBackground);

	if (m_oldclrGroupText != (-1))
		pTBGrid->SetGroupTextColor(m_oldclrGroupText);
}

/////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
//					class CTBPropertyGrid implementation
/////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
//==============================================================================================
IMPLEMENT_DYNCREATE(CTBPropertyGrid, CBCGPPropList)

//-------------------------------------------------------------------------------------------------
BEGIN_MESSAGE_MAP(CTBPropertyGrid, CBCGPPropList)
	//{{AFX_MSG_MAP(CTBPropertyGrid)
	ON_WM_VSCROLL()
	ON_WM_KILLFOCUS()
	ON_WM_SETFOCUS()
	ON_WM_MOUSEWHEEL()
	ON_WM_CONTEXTMENU()
	ON_WM_LBUTTONDOWN()
	ON_WM_RBUTTONDOWN()
	ON_WM_DESTROY()
	ON_MESSAGE(UM_RECALC_CTRL_SIZE, OnRecalcCtrlSize)
	ON_MESSAGE(UM_BAD_VALUE, OnBadValue)
	ON_MESSAGE(UM_LOSING_FOCUS, OnLosingFocus)
	ON_MESSAGE(UM_VALUE_CHANGED, OnValueChanged)
	ON_MESSAGE(UM_GET_CONTROL_DESCRIPTION, OnGetControlDescription)
	ON_WM_ERASEBKGND()
	//}}AFX_MSG_MAP
END_MESSAGE_MAP()

//----------------------------------------------------------------------------------------------
CTBPropertyGrid::CTBPropertyGrid(const CString sName /*= _T("")*/)
	:
	CGridControlObj(this, OSLType_PropertyGrid, sName),
	IDisposingSourceImpl(this),
	m_pBadProperty(NULL),
	m_bGridFocused(FALSE),
	m_bGridActive(FALSE),
	m_bSetFocusOnly(FALSE),
	m_bValidatingCell(FALSE),
	m_SearchBoxPrompt(_TB("Search")),
	m_pDefaultRootProperty(NULL),
	m_bDefaultRootProperty(TRUE),
	m_pTBThemeManager(NULL),
	m_pHyperlinkFont(NULL),
	m_bLButtonDownHit(FALSE),
	m_bDestroyingCompoents(FALSE)
{
	m_pTBThemeManager = AfxGetThemeManager();
}

//----------------------------------------------------------------------------------------------
CTBPropertyGrid::~CTBPropertyGrid()
{
}

//-----------------------------------------------------------------------------
LRESULT CTBPropertyGrid::OnGetControlDescription(WPARAM wParam, LPARAM lParam)
{
	CWndObjDescriptionContainer* pContainer = (CWndObjDescriptionContainer*)wParam;

	CString strId = (LPCTSTR)lParam;
	CWndObjDescription* pDesc = pContainer->GetWindowDescription(this, RUNTIME_CLASS(CWndObjDescription), strId);
	pDesc->m_Type = CWndObjDescription::PropertyGrid;
	pDesc->UpdateAttributes(this);
	pDesc->m_strName = GetName();
	pDesc->SetDeepRemoved(); //in modalità designer, tengo traccia dei cadaveri, non devono finire nel json

	for (int i = 0; i < GetPropertyCount(); i++)
	{
		CTBProperty* pProp = (CTBProperty*)GetProperty(i);
		pProp->GetControlDescription(&pDesc->m_Children, i);
	}

	//in modalità designer, elimino i cadaveri, non devono finire nel json
	for (int i = pDesc->m_Children.GetUpperBound(); i >= 0; i--)
	{
		CWndObjDescription* pChild = pDesc->m_Children.GetAt(i);
		if (pChild->IsRemoved())
			pDesc->m_Children.RemoveAt(i);
	}
	return (LRESULT)pDesc;
}
//----------------------------------------------------------------------------------------------
void CTBPropertyGrid::BeginDestroyingComponents()
{
	m_bDestroyingCompoents = TRUE;
	if (m_wndFilter.m_hWnd)
		m_wndFilter.SetFocus();
}

//----------------------------------------------------------------------------------------------
void CTBPropertyGrid::EndDestroyingComponents()
{
	m_bDestroyingCompoents = FALSE;
}

//----------------------------------------------------------------------------------------------
void CTBPropertyGrid::RemoveAllComponents()
{
	BeginDestroyingComponents();
	RemoveAll();
	EndDestroyingComponents();
}

//-----------------------------------------------------------------------------
BOOL CTBPropertyGrid::OnMouseWheel(UINT nFlags, short zDelta, CPoint pt)
{
	// per inibire l'uso della rotellina quando è attivo il FieldInspector (problemi di redraw del rettangolo)
	if (m_bIsInspecting)
		return FALSE;

	return	__super::OnMouseWheel(nFlags, zDelta, pt);
}

//-------------------------------------------------------------------------------------
BOOL CTBPropertyGrid::PreTranslateMessage(MSG* pMsg)
{
	/*
		CTBProperty* pCurrProp = DYNAMIC_DOWNCAST(CTBProperty, m_pSel);
		if (pCurrProp != NULL && pCurrProp->m_bInPlaceEdit)
		{
			if (pMsg->message == WM_KEYDOWN && pMsg->wParam == VK_RETURN)
				return DoMoveToProp(FALSE);
		}
	*/

	if (pMsg->message == WM_MOUSEMOVE && pMsg->wParam == 0)
	{
		CPoint ptCursor;
		::GetCursorPos(&ptCursor);
		ScreenToClient(&ptCursor);

		// Get the scrollBar position for remmove the capture if mouse in scroolbar zone
		CRect rectScrollBar;
		m_wndScrollVert.GetWindowRect(rectScrollBar);
		ScreenToClient(rectScrollBar);
		if (ptCursor.x >= rectScrollBar.left && ptCursor.x <= rectScrollBar.right &&
			ptCursor.y >= rectScrollBar.top  && ptCursor.y <= rectScrollBar.bottom)
		{
			if (::GetCapture() == GetSafeHwnd())
				ReleaseCapture();
		}

		CTBProperty* pProp = DYNAMIC_DOWNCAST(CTBProperty, HitTest(ptCursor));
		if (pProp)
			if (
				!pProp->m_bEnabled &&
				pProp->GetControl() &&
				pProp->GetControl()->GetCtrlData() &&
				!pProp->GetControl()->GetCtrlData()->IsEmpty() &&
				(
					pProp->GetControl()->GetHotLink() ||
					pProp->GetControl()->GetCtrlCWnd()->IsKindOf(RUNTIME_CLASS(CLinkEdit))
					)
				)
			{
				int xCenter = ((CTBPropertyGrid*)pProp->m_pWndList)->m_rectList.left + ((CTBPropertyGrid*)pProp->m_pWndList)->m_nLeftColumnWidth;
				if (ptCursor.x > xCenter)
				{
					::SetCursor(::LoadCursor(AfxFindResourceHandle(MAKEINTRESOURCE(IDC_TB_HAND), RT_GROUP_CURSOR), MAKEINTRESOURCE(IDC_TB_HAND)));
					SetCapture();
				}
				else
				{
					if (::GetCapture() == GetSafeHwnd())
						ReleaseCapture();
				}
			}
			else
			{
				if (::GetCapture() == GetSafeHwnd())
					ReleaseCapture();

			}
	}

	return GetParent()->PreTranslateMessage(pMsg) || __super::PreTranslateMessage(pMsg);
}

//-----------------------------------------------------------------------------
void CTBPropertyGrid::OnKillFocus(CWnd* pWnd)
{
	__super::OnKillFocus(pWnd);

	// se perde il fuoco per un suo control non si deve fare nella
	if (pWnd && !IsChild(pWnd))
	{
		int nRelationship = GetRelationship(this, pWnd);

		m_wKeyState = 0;
		m_bGridActive = FALSE;
		m_bGridFocused = nRelationship == FOREIGN_FOCUSED;

		if (m_pBadProperty == NULL)
		{
			CParsedForm* pForm = dynamic_cast<CParsedForm*>(GetParent());
			if (pForm)
				pForm->GetFormAncestor()->SendMessage(UM_LOSING_FOCUS, GetDlgCtrlID(), nRelationship);
		}
	}
}

//-----------------------------------------------------------------------------
void CTBPropertyGrid::OnSetFocus(CWnd* pOldWnd)
{
	if (m_bDestroyingCompoents)
		return;

	m_bGridFocused = TRUE;
	m_bGridActive = TRUE;

	__super::OnSetFocus(pOldWnd);

	if (m_bSetFocusOnly)
	{
		m_bSetFocusOnly = FALSE;
		EndEditItem(FALSE);
		return;
	}

	if (GetDocument() && GetDocument()->GetFormMode() != CBaseDocument::BROWSE)
	{
		CTBProperty* pCurrProp = DYNAMIC_DOWNCAST(CTBProperty, m_pSel);
		if (pCurrProp && pCurrProp->GetControl() && pCurrProp->IsEnabled())
		{
			pCurrProp->m_bInPlaceEdit = FALSE;
			EditItem(pCurrProp);
		}
		else
			DoMoveToProp(FALSE);
	}

}

//-----------------------------------------------------------------------------
void CTBPropertyGrid::SetFocusOnly()
{
	m_bSetFocusOnly = TRUE;

	// il messaggio di SetFocus non viene inviato se la window ha gia` il fuoco
	//
	CWnd* pFocus = CWnd::GetFocus();
	if (pFocus && pFocus->m_hWnd == m_hWnd)
		OnSetFocus(NULL);
	else
		SetFocus();
}

//-----------------------------------------------------------------------------
void CTBPropertyGrid::OnContextMenu(CWnd* pWnd, CPoint ptMousePos)
{
	__super::OnContextMenu(pWnd, ptMousePos);
}

//------------------------------------------------------------------------------
void CTBPropertyGrid::OnVScroll(UINT nSBCode, UINT nPos, CScrollBar* pScrollBar)
{
	if (nSBCode == SB_ENDSCROLL)
		return;

	if (!UpdateData(TRUE))
		return;

	// tolgo il fuoco ai controls...
	SetFocusOnly();

	__super::OnVScroll(nSBCode, nPos, pScrollBar);
}

//-----------------------------------------------------------------------------
void CTBPropertyGrid::OnLButtonDown(UINT nFlags, CPoint point)
{
	if (!UpdateData(TRUE))
		return;

	// tolgo il fuoco ai controls...
	SetFocusOnly();

	m_bLButtonDownHit = TRUE;
	__super::OnLButtonDown(nFlags, point);
	m_bLButtonDownHit = FALSE;
}

//-----------------------------------------------------------------------------
void CTBPropertyGrid::OnRButtonDown(UINT nFlags, CPoint point)
{
	if (!UpdateData(TRUE))
		return;

	// tolgo il fuoco ai controls...
	SetFocusOnly();

	__super::OnRButtonDown(nFlags, point);
}

//****************************************************************************************
void CTBPropertyGrid::OnDestroy()
{
	BeginDestroyingComponents();

	__super::OnDestroy();
}

//------------------------------------------------------------------------------
LRESULT CTBPropertyGrid::OnRecalcCtrlSize(WPARAM, LPARAM)
{
	DoRecalcCtrlSize();
	return 0L;
}

// per gestire il corretto draw delle campo di search ed i bottoni di ordinamento
// i controls delle proprietà se visibili devono essere saltati
//-----------------------------------------------------------------------------
BOOL CTBPropertyGrid::OnEraseBkgnd(CDC* pDC)
{
	CRect rclientRect;
	GetClientRect(rclientRect);

	CWnd* pCtrl = GetWindow(GW_CHILD);
	for (; pCtrl; pCtrl = pCtrl->GetNextWindow())
		if (IsTBWindowVisible(pCtrl))
		{
			CRect screen;
			pCtrl->GetWindowRect(&screen);
			ScreenToClient(&screen);
			pDC->ExcludeClipRect(&screen);
		}

	return __super::OnEraseBkgnd(pDC);
}

//-----------------------------------------------------------------------------------------------------------------
LRESULT CTBPropertyGrid::OnBadValue(WPARAM nCtrlID, LPARAM nMode)
{
	if (nCtrlID)
	{
		// Messaggio inviato da un control del body: viene chiesta alla parent
		// form l'autorizzazione gestire il messaggio di errore, dicendo pero` che
		// non e` immediato in modo che la CParsedForm::DoBadValue non faccia nessuna
		// messaggistica
		//
		CParsedForm* pForm = dynamic_cast<CParsedForm*>(GetParent());
		if (pForm && pForm->GetFormAncestor()->SendMessage(UM_BAD_VALUE, GetDlgCtrlID(), nMode & ~CTRL_IMMEDIATE_NOTIFY) == 99L)
			return 99L;
	}

	if (m_pBadProperty == NULL)
		m_pBadProperty = DYNAMIC_DOWNCAST(CTBProperty, m_pSel);

	// Deve essere effettuata una immediata diagnostica ?
	//
	if ((nMode & CTRL_IMMEDIATE_NOTIFY) != CTRL_IMMEDIATE_NOTIFY)
		return 99L;

	if (!m_pBadProperty)
		return 99L;

	CParsedCtrl* pCtrl = m_pBadProperty->GetControl();
	if (!pCtrl)
		return 99L;

	if (pCtrl->ErrorMessage())
	{
		if (!IsTBWindowVisible(pCtrl->GetCtrlCWnd()))
			SetFocus();

		return 99L;
	}

	CWnd* pWnd = GetFocus();
	if (pWnd == NULL || IsChild(pWnd))
		SetFocus();

	return 99L;
}

// gestisce il messaggio di perdita di fuoco inviato da uno dei suoi controls
//--------------------------------------------------------------------------
LRESULT CTBPropertyGrid::OnLosingFocus(WPARAM wParam, LPARAM lParam)
{
	//se sto validando la cella corrente prima di passare alla successiva,
	//potrebbe succedere che una message box o una dialog scateni la perdita di fuoco
	//che pero' non deve essere presa in considerazione, perche non e detto che il fuoco debba effettivamente
	//uscire dalla cella (potrebbe essere rimesso li' in caso di errori (es. chiamate a SetError)
	//si veda an. #13250
	if (m_bValidatingCell)
		return 0;

	// In questo modo si segnala alla parent di resettare lo stato di errore
	//
	CParsedForm* pForm = dynamic_cast<CParsedForm*>(GetParent());
	if (pForm)
		pForm->GetFormAncestor()->SendMessage(UM_LOSING_FOCUS, GetDlgCtrlID(), lParam);

	m_pBadProperty = NULL;

	int nRelationship = (int)lParam;

	if (wParam == 0)
	{
		// Il messaggio e` stato inviato non su perdita di fuoco (vedi per es. TabCore.cpp):
		// quindi per spegnere il control si da il fuoco al body stesso
		SetFocusOnly();
	}
	else
	{
		CTBProperty* pProp = DYNAMIC_DOWNCAST(CTBProperty, FindItemByID((UINT)wParam));
		if (pProp && pProp->GetControl() && IsTBWindowVisible(pProp->GetControl()->GetCtrlCWnd()))
			EndEditItem(FALSE);
	}

	if (lParam == 0)
		return (LRESULT)0;

	if (nRelationship != BROTHER_FOCUSED && nRelationship != PARENT_FOCUSED)
		m_wKeyState = 0;

	// Se il fuoco sta andando fuori da body si da` la possibilita` di verificare
	// la congruenza di tutte le righe

	if (nRelationship == RELATIVE_FOCUSED)
	{
		// se il control sta dando il fuoco ad una window che appartiene alla
		// stessa view si perde il fuoco
		m_bGridFocused = FALSE;
	}

	// se il control sta dando il fuoco ad una window che non e` suo parente
	// diretto (un control di una colonna (fratello) oppure al bodyedit stesso
	// (padre del control) allora si perde l'attivazione
	if (nRelationship != BROTHER_FOCUSED && nRelationship != PARENT_FOCUSED)
	{
		m_bGridActive = FALSE;
	}

	return (LRESULT)0;
}

//-----------------------------------------------------------------------------
LRESULT CTBPropertyGrid::OnValueChanged(WPARAM wParam, LPARAM lParam)
{
	LRESULT lResult = GetParent()->SendMessage(UM_VALUE_CHANGED, wParam, lParam);

	return lResult;
}

//-----------------------------------------------------------------------------
BOOL CTBPropertyGrid::UpdateData(BOOL bSignalError, BOOL bSendMessage /* = FALSE */)
{
	return ValidateItemData(DYNAMIC_DOWNCAST(CTBProperty, m_pSel));
}

//------------------------------------------------------------------------------
BOOL CTBPropertyGrid::ValidateItemData(CBCGPProp* pProp)
{
	BOOL bOk = FALSE;
	CTBProperty* pTBProp = DYNAMIC_DOWNCAST(CTBProperty, pProp);
	CParsedCtrl* pCtrl = pTBProp ? pTBProp->GetControl() : NULL;
	if (!pCtrl || !IsTBWindowVisible(pCtrl->GetCtrlCWnd()))
		bOk = TRUE;

	m_bValidatingCell = TRUE; //inibisco la OnLosingFocus, in questa fase il fuoco non deve uscire dalla cella

	bOk = bOk || pCtrl->UpdateCtrlData(TRUE, FALSE) && pCtrl->GetWarningID() == 0;

	if (bOk)
		m_pBadProperty = NULL;
	else
		m_pBadProperty = pTBProp;

	m_bValidatingCell = FALSE; //inibisco la OnLosingFocus, in questa fase il fuoco non deve uscire dalla cella
	return bOk;
}

//---------------------------------------------------------------------------------------------------
CTBProperty* CTBPropertyGrid::FindItemByID(UINT nID, BOOL bSearchSubItems /*= TRUE*/)
{
	return DYNAMIC_DOWNCAST(CTBProperty, __super::FindItemByID(nID, bSearchSubItems));
}

//---------------------------------------------------------------------------------------------------
CParsedCtrl* CTBPropertyGrid::GetActiveParsedCtrl()
{
	CTBProperty* pProp = DYNAMIC_DOWNCAST(CTBProperty, m_pSel);
	return pProp ? pProp->GetControl() : NULL;
}

//--------------------------------------------------------------------------------------------------
void CTBPropertyGrid::SetMarkModifiedProperties(BOOL bMarkModifiedProperties /*= TRUE*/)
{
	m_bMarkModifiedProperties = bMarkModifiedProperties;
}

//-------------------------------------------------------------------------------------------------
void CTBPropertyGrid::SetHasDefaultRootProperty(BOOL bHasRootProperty /*= TRUE*/)
{
	m_bDefaultRootProperty = bHasRootProperty;
}

//----------------------------------------------------------------------------------------------------------------
BOOL CTBPropertyGrid::DeleteProperty(CTBProperty*& pProp, BOOL bRedraw /*= TRUE*/, BOOL bAdjustLayout /*= TRUE*/)
{
	return __super::DeleteProperty((CBCGPProp*&)pProp, bRedraw, bAdjustLayout);
}

//-----------------------------------------------------------------------------------------------------------------------
BOOL CTBPropertyGrid::RemoveSubItem(CTBProperty* pParentProperty, CTBProperty*& pSubItem, BOOL bDelete /*= TRUE*/)
{
	if (pParentProperty)
		return pParentProperty->RemoveSubItem(pSubItem, bDelete);

	return FALSE;
}

//--------------------------------------------------------------------------------------------------------------------------
void CTBPropertyGrid::RemoveAllSubItems(CTBProperty* pParentProperty)
{
	if (pParentProperty)
		return pParentProperty->RemoveAllSubItems();
}

//------------------------------------------------------------------------------------------------
BOOL CTBPropertyGrid::Create(DWORD dwStyle, const RECT& rect, CWnd* pParentWnd, UINT nID)
{
	if (!__super::Create(dwStyle, rect, pParentWnd, nID))
	{
		ASSERT_TRACE(FALSE, "Error creating CTBPropertyGrid!");
		return FALSE;
	}

	m_wndScrollVert.SetVisualStyle(CBCGPScrollBar::BCGP_SBSTYLE_VISUAL_MANAGER);

	// Obbligatorio per far funzionare correttamente la OnEraseBkgnd per impedire di far sparire la toolbar (search, order..)
	// quando viene effettuata la ReLayout
	ModifyStyle(WS_CLIPCHILDREN, 0);

	Customize();
	return TRUE;
}

//------------------------------------------------------------------------------------------
BOOL CTBPropertyGrid::SubclassDlgItem(UINT nID, CWnd* pParent)
{
	ASSERT(m_pParentForm);

	if (!CBCGPPropList::SubclassDlgItem(nID, pParent))
		return FALSE;

	// Obbligatorio per far funzionare correttamente la OnEraseBkgnd per impedire di far sparire la toolbar (search, order..)
	// quando viene effettuata la ReLayout
	ModifyStyle(WS_CLIPCHILDREN, 0);

	Customize();

	return TRUE;

}

//------------------------------------------------------------------------------
void CTBPropertyGrid::OnUpdateControls(BOOL bParentIsVisible)
{
	CParsedCtrl* pParsedCtrl = GetActiveParsedCtrl();
	if (pParsedCtrl && IsTBWindowVisible(pParsedCtrl->GetCtrlCWnd()))
		pParsedCtrl->UpdateCtrlView();

	for (POSITION pos = m_lstProps.GetHeadPosition(); pos != NULL;)
	{
		CTBProperty* pListProp = DYNAMIC_DOWNCAST(CTBProperty, m_lstProps.GetNext(pos));
		if (!pListProp)
			continue;

		ASSERT_VALID(pListProp);
		pListProp->UpdateStatus();
	}

	Invalidate();
}

//-----------------------------------------------------------------------------
void CTBPropertyGrid::EnableControlLinks(BOOL bEnable /* = TRUE*/, BOOL bMustSetOSLReadOnly /*=FALSE*/)
{
	for (POSITION pos = m_lstProps.GetHeadPosition(); pos != NULL;)
	{
		CTBProperty* pListProp = DYNAMIC_DOWNCAST(CTBProperty, m_lstProps.GetNext(pos));
		if (!pListProp)
			continue;

		ASSERT_VALID(pListProp);
		pListProp->SetDataReadOnly(!bEnable);
	}
}

//------------------------------------------------------------------------------
void CTBPropertyGrid::SetDataModified(BOOL bMod)
{
	for (POSITION pos = m_lstProps.GetHeadPosition(); pos != NULL;)
	{
		CTBProperty* pListProp = DYNAMIC_DOWNCAST(CTBProperty, m_lstProps.GetNext(pos));
		if (!pListProp)
			continue;

		ASSERT_VALID(pListProp);
		pListProp->SetDataModified(bMod);
	}
}

//-------------------------------------------------------------------------------------------------
BOOL CTBPropertyGrid::EditItem(CBCGPProp* pProp, LPPOINT lptClick /*= NULL*/)
{
	m_pBadProperty = NULL;
	return __super::EditItem(pProp, lptClick);
}

//-----------------------------------------------------------------------------
void CTBPropertyGrid::OnAbortForm()
{
	CParsedCtrl* pParsedCtrl = GetActiveParsedCtrl();
	if (pParsedCtrl && IsTBWindowVisible(pParsedCtrl->GetCtrlCWnd()))
	{
		pParsedCtrl->UpdateCtrlView();
		pParsedCtrl->SetErrorID(CParsedCtrl::EMPTY_MESSAGE);
		pParsedCtrl->SetWarningID(CParsedCtrl::EMPTY_MESSAGE);
		pParsedCtrl->SetModifyFlag(FALSE);
		pParsedCtrl->EnableCtrl(FALSE);
		//		pParsedCtrl->ShowCtrl(SW_HIDE);	

		SetFocusOnly();
	}

	m_pBadProperty = NULL;
}

//-------------------------------------------------------------------------------------------------
BOOL CTBPropertyGrid::OnCheckForm(BOOL bEmitError)
{
	CParsedCtrl* pParsedCtrl = GetActiveParsedCtrl();
	if (pParsedCtrl && IsWindowEnabled())
		pParsedCtrl->UpdateCtrlData(bEmitError, TRUE);

	return TRUE;
}

//-----------------------------------------------------------------------------
BOOL CTBPropertyGrid::OnKeyHit(UINT nIDC, UINT nKey, UINT nHitState)
{
	if (nIDC == 0 || (nHitState != WM_KEYUP && nHitState != WM_KEYDOWN))
		return FALSE;

	CParsedCtrl* pParsedCtrl = GetActiveParsedCtrl();
	BOOL bDo = (nIDC == (UINT)GetDlgCtrlID() || (pParsedCtrl && nIDC == pParsedCtrl->GetCtrlID()));


	//	if (bDo) @@@@@@@@@
	//	{
	if (nHitState == WM_KEYDOWN)
		return DoKeyDown(nKey);

	return DoKeyUp(nKey);
	//    }                           

	return FALSE;
}

//------------------------------------------------------------------------------------
CTBProperty* CTBPropertyGrid::CreateProperty
(
	CString			sName,
	CString			sPropertyLeftText,
	CString			sPropertyBottomText,
	DWORD			dwStyle,
	DataObj*		pDataObj			/* = NULL*/,
	UINT			nIDC				/* = 0*/,
	CRuntimeClass*	pParsedCtrlClass	/* = NULL*/,
	HotKeyLink*		pHotKeyLink			/* = NULL*/,
	UINT			nBtnID				/* = BTN_DEFAULT*/,
	SqlRecord*		pSqlRecord			/*= NULL*/
)
{
	CTBProperty* pProp = NULL;


	if (!pDataObj)
		pProp = new CTBProperty(sName, sPropertyLeftText, sPropertyBottomText);
	else
		pProp = new CTBProperty(sName, sPropertyLeftText, sPropertyBottomText, (_variant_t)pDataObj->ToString());

	pProp->SetDescription(sPropertyBottomText);
	pProp->SetID(nIDC);

	if (pParsedCtrlClass)
	{
		DWORD dwFullStyle = WS_VISIBLE | WS_CHILD | WS_TABSTOP | dwStyle;
		CParsedCtrl* pParsedCtrl = ::CreateControl
		(
			sName,
			dwFullStyle,
			CRect(0, 0, 0, 0),
			this,
			GetDocument(),
			NULL,
			nIDC,
			pSqlRecord,
			pDataObj,
			pParsedCtrlClass,
			pHotKeyLink,
			FALSE,		//bIsARuntimeClass
			nBtnID
		);

		if (pParsedCtrl)
		{
			pParsedCtrl->GetInfoOSL()->m_pParent = GetInfoOSL();
			pParsedCtrl->GetInfoOSL()->m_Namespace.SetChildNamespace(CTBNamespace::CONTROL, sName, GetNamespace());

			if (
				GetDocument() &&
				(
				(GetDocument()->GetFormMode() == CBaseDocument::NEW && OSL_CAN_DO(pParsedCtrl->GetInfoOSL(), OSL_GRANT_NEW) == 0) ||
					(GetDocument()->GetFormMode() == CBaseDocument::EDIT && OSL_CAN_DO(pParsedCtrl->GetInfoOSL(), OSL_GRANT_EDIT) == 0)
					)
				)
				pParsedCtrl->SetDataOSLReadOnly(TRUE);

			if (OSL_CAN_DO(pParsedCtrl->GetInfoOSL(), OSL_GRANT_EXECUTE) == 0)
			{
				pParsedCtrl->SetDataOSLReadOnly(TRUE);
				pParsedCtrl->SetDataOSLHide(TRUE);
			}

			pParsedCtrl->ShowCtrl(SW_HIDE);
			pProp->SetControl(pParsedCtrl);
		}
	}

	return pProp;
}

//------------------------------------------------------------------------------------
CTBProperty* CTBPropertyGrid::AddProperty
(
	CString			sName,
	CString			sPropertyLeftText,
	CString			sPropertyBottomText,
	UINT			nIDC				/* = 0*/,
	DWORD			dwStyle				/*default*/,
	DataObj*		pDataObj			/*= NULL*/,
	CRuntimeClass*	pParsedCtrlClass	/* = NULL*/,
	HotKeyLink*		pHotKeyLink			/*= NULL*/,
	UINT			nBtnID				/*= BTN_DEFAULT*/,
	SqlRecord*		pSqlRecord			/*= NULL*/
)
{
	return AddProperty
	(
		sName,
		sPropertyLeftText,
		sPropertyBottomText,
		pDataObj,
		nIDC,
		dwStyle,
		pParsedCtrlClass,
		pHotKeyLink,
		nBtnID,
		pSqlRecord
	);
}

//------------------------------------------------------------------------------------
CTBProperty* CTBPropertyGrid::AddProperty
(
	CString			sName,
	CString			sPropertyLeftText,
	CString			sPropertyBottomText,
	DataObj*		pDataObj			/*= NULL*/,
	UINT			nIDC				/* = 0*/,
	DWORD			dwStyle				/*default*/,
	CRuntimeClass*	pParsedCtrlClass	/* = NULL*/,
	HotKeyLink*		pHotKeyLink			/*= NULL*/,
	UINT			nBtnID				/*= BTN_DEFAULT*/,
	SqlRecord*		pSqlRecord			/*= NULL*/
)
{
	CTBProperty* pProp = CreateProperty(sName, sPropertyLeftText, sPropertyBottomText, dwStyle, pDataObj, nIDC, pParsedCtrlClass, pHotKeyLink, nBtnID, pSqlRecord);

	if (m_pDefaultRootProperty == NULL)
		__super::AddProperty(pProp);
	else
		m_pDefaultRootProperty->AddSubProperty(pProp);

	return pProp;
}

//-----------------------------------------------------------------------------------------------------------------------
CTBProperty* CTBPropertyGrid::AddSubItem
(
	CTBProperty*	pParentProperty,
	CString			sName,
	CString			sPropertyLeftText,
	CString			sPropertyBottomText,
	UINT			nIDC				/*= 0*/,
	DWORD			dwStyle				/*default*/,
	DataObj*		pDataObj			/*= NULL*/,
	CRuntimeClass*	pParsedCtrlClass	/*= NULL*/,
	HotKeyLink*		pHotKeyLink			/*= NULL*/,
	UINT			nBtnID				/*= BTN_DEFAULT*/,
	SqlRecord*		pSqlRecord			/*= NULL*/
)
{
	return AddSubItem
	(
		pParentProperty,
		sName,
		sPropertyLeftText,
		sPropertyBottomText,
		pDataObj,
		nIDC,
		dwStyle,
		pParsedCtrlClass,
		pHotKeyLink,
		nBtnID,
		pSqlRecord
	);
}

//-----------------------------------------------------------------------------------------------------------------------
CTBProperty* CTBPropertyGrid::AddSubItem
(
	CTBProperty*	pParentProperty,
	CString			sName,
	CString			sPropertyLeftText,
	CString			sPropertyBottomText,
	DataObj*		pDataObj			/*= NULL*/,
	UINT			nIDC				/*= 0*/,
	DWORD			dwStyle				/*default*/,
	CRuntimeClass*	pParsedCtrlClass	/*= NULL*/,
	HotKeyLink*		pHotKeyLink			/*= NULL*/,
	UINT			nBtnID				/*= BTN_DEFAULT*/,
	SqlRecord*		pSqlRecord			/*= NULL*/
)
{
	if (!pParentProperty)
		return NULL;

	CTBProperty* pProp = CreateProperty(sName, sPropertyLeftText, sPropertyBottomText, dwStyle, pDataObj, nIDC, pParsedCtrlClass, pHotKeyLink, nBtnID, pSqlRecord);
	pParentProperty->AddSubProperty(pProp);
	if (pParentProperty->GetHierarchyLevel() > 0)
	{
		pParentProperty->SetGroupBackground(AfxGetThemeManager()->GetPropertyGridSubGroupsBkgColor());
		pParentProperty->SetrGroupText(AfxGetThemeManager()->GetPropertyGridSubGroupsForeColor());
	}
	return pProp;
}

//------------------------------------------------------------------------------------
void CTBPropertyGrid::Customize()
{
	ResizableCtrl::InitSizeInfo(this);
	this->m_nDirStrech = 3;
	InitPropList();
	OnCustomize();
}


//---------------------------------------------------------------------------------------
void CTBPropertyGrid::InitPropList()
{
	EnableToolBar();
	EnableDescriptionArea();
	EnableSearchBox(TRUE, m_SearchBoxPrompt);
	EnableHeaderCtrl(FALSE);
	EnableContextMenu(FALSE);
	MarkModifiedProperties();
	SetVSDotNetLook();

	CBaseTileDialog* pTileDialog = dynamic_cast<CBaseTileDialog*>(m_pParentForm->GetFormCWnd());
	COLORREF clrBkgGround = pTileDialog ? pTileDialog->GetTileStyle()->GetBackgroundColor() : m_pTBThemeManager->GetBackgroundColor();
	SetCustomColors
	(
		clrBkgGround,
		m_pTBThemeManager->GetEnabledControlForeColor(),
		m_pTBThemeManager->GetPropertyGridGroupBkgColor(),
		m_pTBThemeManager->GetPropertyGridGroupForeColor(),
		clrBkgGround,
		m_pTBThemeManager->GetEnabledControlForeColor(),
		-1
	);

	m_pHyperlinkFont = m_pTBThemeManager->GetHyperlinkFont();
	m_clrHyperLinkForeColor = m_pTBThemeManager->GetHyperLinkForeColor();

	SetFont(AfxGetThemeManager()->GetFormFont());

	if (!pTileDialog || pTileDialog->Tile::GetTitle().IsEmpty())
		return;

	//manage default root property m_pDefaultRootProperty
	if (!m_bDefaultRootProperty)
		return;

	// per comodità ci teniamo il puntatore
	m_pDefaultRootProperty = AddProperty(pTileDialog->GetTitle().Trim(), pTileDialog->GetTitle(), pTileDialog->GetTitle());

	//se trova il titolo nella TileDialog padre, lo rende invisibile in quanto la prima proprietà di gruppo è stata creata col suo nome
	pTileDialog->SetHasTitle(FALSE);
}

//---------------------------------------------------------------------------------------------------
BOOL CTBPropertyGrid::OnCommand(WPARAM wParam, LPARAM lParam)
{
	DECLARE_WM_COMMAND_PARAMS(wParam, lParam, nID, nCode, hCtrl);

	CWnd* pParent = GetParent();

	if (nCode == EN_VALUE_CHANGED && m_pSel)
	{
		CTBProperty* pProp = DYNAMIC_DOWNCAST(CTBProperty, m_pSel);
		if (pProp)
			pProp->DoValueChanged();
	}


	if (pParent)
		pParent->SendMessage(WM_COMMAND, wParam, lParam);

	return __super::OnCommand(wParam, lParam);
}

//-----------------------------------------------------------------------------
BOOL CTBPropertyGrid::DoMovingKey(UINT nChar)
{
	BOOL shiftDown;
	BOOL ctrlDown;
	BOOL altDown;
	GetKeyDownState(shiftDown, ctrlDown, altDown);

	if (ctrlDown)
	{
		switch (nChar)
		{
		case _T('U'):
		case _T('u'):	if (altDown) return FALSE;// NOT EATEN !!!
		case VK_UP:	DoMoveToProp(TRUE);	return TRUE;
		case _T('D'):
		case _T('d'):	if (altDown) return FALSE;// NOT EATEN !!!
		case VK_DOWN:	DoMoveToProp(FALSE);	return TRUE;
		}
	}
	else
	{
		switch (nChar)
		{
		case VK_RETURN:
		case VK_TAB: if (shiftDown) DoMoveToProp(TRUE); else	DoMoveToProp(FALSE);	return TRUE;
		}
	}

	m_wKeyState = 0;

	return FALSE;	// NOT EATEN !!!
}

//-----------------------------------------------------------------------------
BOOL CTBPropertyGrid::DoMoveToProp(BOOL bPrev)
{
	CList<CBCGPProp*, CBCGPProp*>& lst = (IsAlphabeticMode()) ? m_lstTerminalProps : m_lstProps;

	CTBProperty* pCurrProp = DYNAMIC_DOWNCAST(CTBProperty, m_pSel);
	if (pCurrProp == NULL)
	{
		POSITION pos = lst.GetHeadPosition();
		while (pos && (pCurrProp = DYNAMIC_DOWNCAST(CTBProperty, lst.GetNext(pos))) == NULL);
		if (!pos)
			return FALSE;
	}
	else
		if (!ValidateItemData(pCurrProp))
			return FALSE;

	CTBProperty* pPrevProp = pCurrProp;
	CTBProperty* pListProp = NULL;
	for (POSITION pos = lst.GetHeadPosition(); pos != NULL;)
	{
		if (pListProp && pListProp->GetControl() && pListProp->IsVisibleInFilter())
			pPrevProp = pListProp;

		pListProp = DYNAMIC_DOWNCAST(CTBProperty, lst.GetNext(pos));
		if (!pListProp)
			continue;

		if (pListProp != pCurrProp)
		{
			if (pListProp->GetControl() && pListProp->IsVisibleInFilter())
				pPrevProp = pListProp;

			pListProp = pListProp->GetNearlySubProperty(pCurrProp, pPrevProp, bPrev);
		}

		// trovata posizione del corrente ma nessuno susseguente oppure siamo su quello attuale 
		if (pListProp == pCurrProp)
		{
			if (bPrev)
				pListProp = pPrevProp;
			else
				pListProp = pListProp->GetNextEditableProperty(&lst, pos);
		}

		if (pListProp)
		{
			m_bSetFocusOnly = TRUE;
			SetCurSel(pListProp, TRUE);
			if (pListProp->IsEnabled())
				EditItem(pListProp);
			else
			{
				SetFocus();
				EnsureVisible(pListProp);
			}
			return TRUE;
		}
	}

	CWnd* pWnd = GetNextViewTabItem(bPrev);

	if (pWnd)
		pWnd->SetFocus();

	return TRUE;
}

//-----------------------------------------------------------------------------
COLORREF CTBPropertyGrid::SetGroupBackgroundColor(COLORREF clr)
{
	COLORREF old = m_clrGroupBackground;
	m_clrGroupBackground = clr;
	return old;
}

//-----------------------------------------------------------------------------
COLORREF CTBPropertyGrid::SetGroupTextColor(COLORREF clr)
{
	COLORREF old = m_clrGroupText;
	m_clrGroupText = clr;
	return old;
}

//-----------------------------------------------------------------------------
BOOL CTBPropertyGrid::OnDrawProperty(CDC* pDC, CBCGPProp* pProp) const
{
	CTBProperty* pTBProperty = dynamic_cast<CTBProperty*>(pProp);
	if (!pTBProperty)
		return 	__super::OnDrawProperty(pDC, pProp);

	pTBProperty->OnBeforeDrawProperty();

	BOOL bOk = __super::OnDrawProperty(pDC, pProp);

	pTBProperty->OnAfterDrawProperty();
	return bOk;
}

//-----------------------------------------------------------------------------
IMPLEMENT_DYNCREATE(CWndPropertyGridItemDescription, CEditObjDescription)
REGISTER_WND_OBJ_CLASS(CWndPropertyGridItemDescription, PropertyGridItem)

//-----------------------------------------------------------------------------
void CWndPropertyGridItemDescription::SerializeJson(CJsonSerializer& strJson)
{
	SERIALIZE_BOOL(m_bSort, szJsonSort, false);
	SERIALIZE_BOOL(m_bCollapsed, szJsonCollapsed, false);

	if (m_pItemSourceDescri)
	{
		m_pItemSourceDescri->SerializeJson(strJson);
	}

	__super::SerializeJson(strJson);

}

//-----------------------------------------------------------------------------
void CWndPropertyGridItemDescription::ParseJson(CJsonFormParser& parser)
{
	__super::ParseJson(parser);

	PARSE_BOOL(m_bSort, szJsonSort);
	PARSE_BOOL(m_bCollapsed, szJsonCollapsed);

	if (parser.Has(szJsonItemSource))
	{
		if (!m_pItemSourceDescri)
		{
			m_pItemSourceDescri = new CItemSourceDescription();
			m_pItemSourceDescri->SetParent(this);
		}
		m_pItemSourceDescri->ParseJson(parser);
	}
}
//-----------------------------------------------------------------------------
void CWndPropertyGridItemDescription::Assign(CWndObjDescription* pDesc)
{
	__super::Assign(pDesc);

	m_bSort = ((CWndPropertyGridItemDescription*)pDesc)->m_bSort;
	m_bCollapsed = ((CWndPropertyGridItemDescription*)pDesc)->m_bCollapsed;
	if (((CWndPropertyGridItemDescription*)pDesc)->m_pItemSourceDescri)
		m_pItemSourceDescri = ((CWndPropertyGridItemDescription*)pDesc)->m_pItemSourceDescri->Clone();
}
//-----------------------------------------------------------------------------
void CWndPropertyGridItemDescription::EvaluateExpressions(CJsonContextObj * pJsonContext, bool deep /*= true*/)
{
	__super::EvaluateExpressions(pJsonContext, deep);
	if (m_pItemSourceDescri)
		pJsonContext->Evaluate(m_pItemSourceDescri->m_Expressions, this);
}


