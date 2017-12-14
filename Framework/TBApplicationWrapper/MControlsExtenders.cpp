#include "stdafx.h"
#include "MControlsExtenders.h"
#include "MParsedControls.h"

using namespace Microarea::Framework::TBApplicationWrapper;

//----------------------------------------------------------------------------
MBaseWindowWrapperProperties::MBaseWindowWrapperProperties(IEasyBuilderComponentExtendable^ owner, EasyBuilderComponent^ parentComponent, System::String^ name)
	:
	EasyBuilderComponentExtender(owner, parentComponent, name)
{
}

//----------------------------------------------------------------------------
BaseWindowWrapper^ MBaseWindowWrapperProperties::ExtendedWindow::get()
{
	return (BaseWindowWrapper^) ExtendedObject;
}

#pragma region MEditProperties

//----------------------------------------------------------------------------
MEditProperties::MEditProperties(IEasyBuilderComponentExtendable^ owner, EasyBuilderComponent^ parentComponent, System::String^ name)
	:
	MBaseWindowWrapperProperties(owner, parentComponent, name)
{
}

//----------------------------------------------------------------------------
bool MEditProperties::Number::get()
{
	CWndObjDescription* pDescri = ExtendedWindow->GetWndObjDescription();
	return pDescri && pDescri->IsKindOf(RUNTIME_CLASS(CEditObjDescription)) ?
		((CEditObjDescription*)pDescri)->m_bNumber
		: ExtendedWindow->HasStyle(ES_NUMBER);
}
//----------------------------------------------------------------------------
void MEditProperties::Number::set(bool value)
{
	CWndObjDescription* pDescri = ExtendedWindow->GetWndObjDescription();
	if (pDescri && pDescri->IsKindOf(RUNTIME_CLASS(CEditObjDescription)))
		((CEditObjDescription*)pDescri)->m_bNumber = value;
	ExtendedWindow->SetStyle(SET_STYLE_PARAMS(ES_NUMBER));
}

//----------------------------------------------------------------------------
bool MEditProperties::Multiline::get()
{
	CWndObjDescription* pDescri = ExtendedWindow->GetWndObjDescription();
	return pDescri && pDescri->IsKindOf(RUNTIME_CLASS(CEditObjDescription)) ?
		((CEditObjDescription*)pDescri)->m_bMultiline
		: ExtendedWindow->HasStyle(ES_MULTILINE);
}
//----------------------------------------------------------------------------
void MEditProperties::Multiline::set(bool value)
{
	CWndObjDescription* pDescri = ExtendedWindow->GetWndObjDescription();
	if (pDescri && pDescri->IsKindOf(RUNTIME_CLASS(CEditObjDescription)))
		((CEditObjDescription*)pDescri)->m_bMultiline = value;
	ExtendedWindow->SetStyle(SET_STYLE_PARAMS(ES_MULTILINE));
}

//----------------------------------------------------------------------------
bool MEditProperties::AutoHScroll::get()
{
	CWndObjDescription* pDescri = ExtendedWindow->GetWndObjDescription();
	return pDescri && pDescri->IsKindOf(RUNTIME_CLASS(CEditObjDescription)) ?
		((CEditObjDescription*)pDescri)->m_bAutoHScroll
		: ExtendedWindow->HasStyle(ES_AUTOHSCROLL);
}
//----------------------------------------------------------------------------
void MEditProperties::AutoHScroll::set(bool value)
{
	CWndObjDescription* pDescri = ExtendedWindow->GetWndObjDescription();
	if (pDescri && pDescri->IsKindOf(RUNTIME_CLASS(CEditObjDescription)))
		((CEditObjDescription*)pDescri)->m_bAutoHScroll = value;
	ExtendedWindow->SetStyle(SET_STYLE_PARAMS(ES_AUTOHSCROLL));
}

//----------------------------------------------------------------------------
bool MEditProperties::AutoVScroll::get()
{
	CWndObjDescription* pDescri = ExtendedWindow->GetWndObjDescription();
	return pDescri && pDescri->IsKindOf(RUNTIME_CLASS(CEditObjDescription)) ?
		((CEditObjDescription*)pDescri)->m_bAutoVScroll
		: ExtendedWindow->HasStyle(ES_AUTOVSCROLL);
}
//----------------------------------------------------------------------------
void MEditProperties::AutoVScroll::set(bool value)
{
	CWndObjDescription* pDescri = ExtendedWindow->GetWndObjDescription();
	if (pDescri && pDescri->IsKindOf(RUNTIME_CLASS(CEditObjDescription)))
		((CEditObjDescription*)pDescri)->m_bAutoVScroll = value;
	ExtendedWindow->SetStyle(SET_STYLE_PARAMS(ES_AUTOVSCROLL));
}

//----------------------------------------------------------------------------
bool MEditProperties::NoHideSelection::get()
{
	CWndObjDescription* pDescri = ExtendedWindow->GetWndObjDescription();
	return pDescri && pDescri->IsKindOf(RUNTIME_CLASS(CEditObjDescription)) ?
		((CEditObjDescription*)pDescri)->m_bNoHideSelection
		: ExtendedWindow->HasStyle(ES_NOHIDESEL);
}
//----------------------------------------------------------------------------
void MEditProperties::NoHideSelection::set(bool value)
{
	CWndObjDescription* pDescri = ExtendedWindow->GetWndObjDescription();
	if (pDescri && pDescri->IsKindOf(RUNTIME_CLASS(CEditObjDescription)))
		((CEditObjDescription*)pDescri)->m_bNoHideSelection = value;
	ExtendedWindow->SetStyle(SET_STYLE_PARAMS(ES_NOHIDESEL));
}

//----------------------------------------------------------------------------
bool MEditProperties::Password::get()
{
	CWndObjDescription* pDescri = ExtendedWindow->GetWndObjDescription();
	return pDescri && pDescri->IsKindOf(RUNTIME_CLASS(CEditObjDescription)) ?
		((CEditObjDescription*)pDescri)->m_bPassword
		: ExtendedWindow->HasStyle(ES_PASSWORD);
}
//----------------------------------------------------------------------------
void MEditProperties::Password::set(bool value)
{
	CWndObjDescription* pDescri = ExtendedWindow->GetWndObjDescription();
	if (pDescri && pDescri->IsKindOf(RUNTIME_CLASS(CEditObjDescription)))
		((CEditObjDescription*)pDescri)->m_bPassword = value;
	ExtendedWindow->SetStyle(SET_STYLE_PARAMS(ES_PASSWORD));
}

//----------------------------------------------------------------------------
bool MEditProperties::ReadOnly::get()
{
	CWndObjDescription* pDescri = ExtendedWindow->GetWndObjDescription();
	return pDescri && pDescri->IsKindOf(RUNTIME_CLASS(CEditObjDescription)) ?
		((CEditObjDescription*)pDescri)->m_bReadOnly
		: ExtendedWindow->HasStyle(ES_READONLY);
}
//----------------------------------------------------------------------------
void MEditProperties::ReadOnly::set(bool value)
{
	CWndObjDescription* pDescri = ExtendedWindow->GetWndObjDescription();
	if (pDescri && pDescri->IsKindOf(RUNTIME_CLASS(CEditObjDescription)))
		((CEditObjDescription*)pDescri)->m_bReadOnly = value;
	ExtendedWindow->SetStyle(SET_STYLE_PARAMS(ES_READONLY));
}

//----------------------------------------------------------------------------
bool MEditProperties::UpperCase::get()
{
	CWndObjDescription* pDescri = ExtendedWindow->GetWndObjDescription();
	return pDescri && pDescri->IsKindOf(RUNTIME_CLASS(CEditObjDescription)) ?
		((CEditObjDescription*)pDescri)->m_bUpperCase
		: ExtendedWindow->HasStyle(ES_UPPERCASE);
}
//----------------------------------------------------------------------------
void MEditProperties::UpperCase::set(bool value)
{
	CWndObjDescription* pDescri = ExtendedWindow->GetWndObjDescription();
	if (pDescri && pDescri->IsKindOf(RUNTIME_CLASS(CEditObjDescription)))
		((CEditObjDescription*)pDescri)->m_bUpperCase = value;
	ExtendedWindow->SetStyle(SET_STYLE_PARAMS(ES_UPPERCASE));
}

//----------------------------------------------------------------------------
bool MEditProperties::LowerCase::get()
{
	CWndObjDescription* pDescri = ExtendedWindow->GetWndObjDescription();
	return pDescri && pDescri->IsKindOf(RUNTIME_CLASS(CEditObjDescription)) ?
		((CEditObjDescription*)pDescri)->m_bLowerCase
		: ExtendedWindow->HasStyle(ES_LOWERCASE);
}
//----------------------------------------------------------------------------
void MEditProperties::LowerCase::set(bool value)
{
	CWndObjDescription* pDescri = ExtendedWindow->GetWndObjDescription();
	if (pDescri && pDescri->IsKindOf(RUNTIME_CLASS(CEditObjDescription)))
		((CEditObjDescription*)pDescri)->m_bLowerCase = value;
	ExtendedWindow->SetStyle(SET_STYLE_PARAMS(ES_LOWERCASE));
}

//----------------------------------------------------------------------------
bool MEditProperties::WantReturn::get()
{
	CWndObjDescription* pDescri = ExtendedWindow->GetWndObjDescription();
	return pDescri && pDescri->IsKindOf(RUNTIME_CLASS(CEditObjDescription)) ?
		((CEditObjDescription*)pDescri)->m_bWantReturn
		: ExtendedWindow->HasStyle(ES_WANTRETURN);
}
//----------------------------------------------------------------------------
void MEditProperties::WantReturn::set(bool value)
{
	CWndObjDescription* pDescri = ExtendedWindow->GetWndObjDescription();
	if (pDescri && pDescri->IsKindOf(RUNTIME_CLASS(CEditObjDescription)))
		((CEditObjDescription*)pDescri)->m_bWantReturn = value;
	ExtendedWindow->SetStyle(SET_STYLE_PARAMS(ES_WANTRETURN));
}

//----------------------------------------------------------------------------
ETextAlign MEditProperties::TextAlignment::get()
{
	CWndObjDescription* pDescri = ExtendedWindow->GetWndObjDescription();
	if (pDescri && pDescri->IsKindOf(RUNTIME_CLASS(CEditObjDescription)))
		return (ETextAlign)((CEditObjDescription*)pDescri)->m_textAlign;

	if (ExtendedWindow->HasStyle(ES_CENTER))	return ETextAlign::Center;
	else if (ExtendedWindow->HasStyle(ES_RIGHT))	return ETextAlign::Right;
	return ETextAlign::Left;
}
//----------------------------------------------------------------------------
void MEditProperties::TextAlignment::set(ETextAlign value)
{
	CWndObjDescription* pDescri = ExtendedWindow->GetWndObjDescription();
	if (pDescri && pDescri->IsKindOf(RUNTIME_CLASS(CEditObjDescription)) && ((CEditObjDescription*)pDescri)->m_textAlign != (::TextAlignment)value)
	{
		((CEditObjDescription*)pDescri)->m_textAlign = (::TextAlignment)value;
		((CEditObjDescription*)pDescri)->SetUpdated(&(((CEditObjDescription*)pDescri)->m_textAlign));
	}

	switch (value){
	case ETextAlign::Left:ExtendedWindow->SetStyle(ES_RIGHT | ES_CENTER, ES_LEFT);  break;
	case ETextAlign::Right:ExtendedWindow->SetStyle(ES_LEFT | ES_CENTER, ES_RIGHT); break;
	case ETextAlign::Center:	ExtendedWindow->SetStyle(ES_LEFT | ES_RIGHT, ES_CENTER); break;
	}
}


#pragma endregion 


#pragma region MComboBoxProperties

//----------------------------------------------------------------------------
MComboBoxProperties::MComboBoxProperties(IEasyBuilderComponentExtendable^ owner, EasyBuilderComponent^ parentComponent, System::String^ name)
	:
	MBaseWindowWrapperProperties(owner, parentComponent, name)
{
}

//----------------------------------------------------------------------------
bool MComboBoxProperties::UpperCase::get()
{
	CWndObjDescription* pDescri = ExtendedWindow->GetWndObjDescription();
	return pDescri && pDescri->IsKindOf(RUNTIME_CLASS(CComboDescription)) ?
		((CComboDescription*)pDescri)->m_bUpperCase
		: ExtendedWindow->HasStyle(CBS_UPPERCASE);
}
//----------------------------------------------------------------------------
void MComboBoxProperties::UpperCase::set(bool value)
{
	CWndObjDescription* pDescri = ExtendedWindow->GetWndObjDescription();
	if (pDescri && pDescri->IsKindOf(RUNTIME_CLASS(CComboDescription)))
		((CComboDescription*)pDescri)->m_bUpperCase = value;
	ExtendedWindow->SetStyle(SET_STYLE_PARAMS(CBS_UPPERCASE));
}

//----------------------------------------------------------------------------
bool MComboBoxProperties::Sort::get()
{
	CWndObjDescription* pDescri = ExtendedWindow->GetWndObjDescription();
	return pDescri && pDescri->IsKindOf(RUNTIME_CLASS(CComboDescription)) ?
		((CComboDescription*)pDescri)->m_bSort
		: ExtendedWindow->HasStyle(CBS_SORT);
}
//----------------------------------------------------------------------------
void MComboBoxProperties::Sort::set(bool value)
{
	CWndObjDescription* pDescri = ExtendedWindow->GetWndObjDescription();
	if (pDescri && pDescri->IsKindOf(RUNTIME_CLASS(CComboDescription)))
		((CComboDescription*)pDescri)->m_bSort = value;
	ExtendedWindow->SetStyle(SET_STYLE_PARAMS(CBS_SORT));
}

//----------------------------------------------------------------------------
bool MComboBoxProperties::OemConvert::get()
{
	CWndObjDescription* pDescri = ExtendedWindow->GetWndObjDescription();
	return pDescri && pDescri->IsKindOf(RUNTIME_CLASS(CComboDescription)) ?
		((CComboDescription*)pDescri)->m_bOemConvert
		: ExtendedWindow->HasStyle(CBS_OEMCONVERT);
}
//----------------------------------------------------------------------------
void MComboBoxProperties::OemConvert::set(bool value)
{
	CWndObjDescription* pDescri = ExtendedWindow->GetWndObjDescription();
	if (pDescri && pDescri->IsKindOf(RUNTIME_CLASS(CComboDescription)))
		((CComboDescription*)pDescri)->m_bOemConvert = value;
	ExtendedWindow->SetStyle(SET_STYLE_PARAMS(CBS_OEMCONVERT));
}

//----------------------------------------------------------------------------
bool MComboBoxProperties::NoIntegralHeight::get()
{
	CWndObjDescription* pDescri = ExtendedWindow->GetWndObjDescription();
	return pDescri && pDescri->IsKindOf(RUNTIME_CLASS(CComboDescription)) ?
		((CComboDescription*)pDescri)->m_bNoIntegralHeight 
		: ExtendedWindow->HasStyle(CBS_NOINTEGRALHEIGHT);
}

//----------------------------------------------------------------------------
void MComboBoxProperties::NoIntegralHeight::set(bool value)
{
	CWndObjDescription* pDescri = ExtendedWindow->GetWndObjDescription();
	if (pDescri && pDescri->IsKindOf(RUNTIME_CLASS(CComboDescription)))
		((CComboDescription*)pDescri)->m_bNoIntegralHeight = value;
	ExtendedWindow->SetStyle(SET_STYLE_PARAMS(CBS_NOINTEGRALHEIGHT));
}

//----------------------------------------------------------------------------
bool MComboBoxProperties::AutoHScroll::get()
{
	CWndObjDescription* pDescri = ExtendedWindow->GetWndObjDescription();
	return pDescri && pDescri->IsKindOf(RUNTIME_CLASS(CComboDescription)) ?
		((CComboDescription*)pDescri)->m_bAuto 
		: ExtendedWindow->HasStyle(CBS_AUTOHSCROLL);
}

//----------------------------------------------------------------------------
void MComboBoxProperties::AutoHScroll::set(bool value)
{
	CWndObjDescription* pDescri = ExtendedWindow->GetWndObjDescription();
	if (pDescri && pDescri->IsKindOf(RUNTIME_CLASS(CComboDescription)))
		((CComboDescription*)pDescri)->m_bAuto = value;
	ExtendedWindow->SetStyle(SET_STYLE_PARAMS(CBS_AUTOHSCROLL));
}

//----------------------------------------------------------------------------
EComboType MComboBoxProperties::ComboType::get()
{
	CWndObjDescription* pDescri = ExtendedWindow->GetWndObjDescription();
	if (pDescri && pDescri->IsKindOf(RUNTIME_CLASS(CComboDescription)))
		return (EComboType)((CComboDescription*)pDescri)->m_nComboType ;

	if (ExtendedWindow->HasStyle(CBS_DROPDOWNLIST))		return EComboType::DropDownList;
	else if (ExtendedWindow->HasStyle(CBS_DROPDOWN))	return EComboType::DropDown;
	return EComboType::Simple;
}
//----------------------------------------------------------------------------
void MComboBoxProperties::ComboType::set(EComboType value)
{
	CWndObjDescription* pDescri = ExtendedWindow->GetWndObjDescription();
	if(pDescri && pDescri->IsKindOf(RUNTIME_CLASS(CComboDescription)) && ((CComboDescription*)pDescri)->m_nComboType != (::ComboType)value)
	{
		((CComboDescription*)pDescri)->m_nComboType = (::ComboType)value;
		((CComboDescription*)pDescri)->SetUpdated(&(((CComboDescription*)pDescri)->m_nComboType));
	}

	switch (value){
	case EComboType::Simple:
		ExtendedWindow->SetStyle(CBS_DROPDOWN | CBS_DROPDOWNLIST, CBS_SIMPLE);   break;
	case EComboType::DropDown:
		ExtendedWindow->SetStyle(CBS_SIMPLE | CBS_DROPDOWNLIST, CBS_DROPDOWN);	 break;
	case EComboType::DropDownList:
		ExtendedWindow->SetStyle(CBS_SIMPLE | CBS_DROPDOWN, CBS_DROPDOWNLIST); 
	}
}

//----------------------------------------------------------------------------
EOwnerDrawType MComboBoxProperties::ComboOwnerDrawType::get()
{
	CWndObjDescription* pDescri = ExtendedWindow->GetWndObjDescription();
	if (pDescri && pDescri->IsKindOf(RUNTIME_CLASS(CComboDescription)))
		return (EOwnerDrawType)((CComboDescription*)pDescri)->m_nComboOwnerDrawType;

	if (ExtendedWindow->HasStyle(CBS_OWNERDRAWFIXED))			return EOwnerDrawType::Fixed;
	else if (ExtendedWindow->HasStyle(CBS_OWNERDRAWVARIABLE))	return EOwnerDrawType::Variable;
	return EOwnerDrawType::None;
}
//----------------------------------------------------------------------------
void MComboBoxProperties::ComboOwnerDrawType::set(EOwnerDrawType value)
{
	CWndObjDescription* pDescri = ExtendedWindow->GetWndObjDescription();
	if (pDescri && pDescri->IsKindOf(RUNTIME_CLASS(CComboDescription)) && ((CComboDescription*)pDescri)->m_nComboOwnerDrawType != (::OwnerDrawType)value)
	{
		((CComboDescription*)pDescri)->m_nComboOwnerDrawType = (::OwnerDrawType)value;
		((CComboDescription*)pDescri)->SetUpdated(&(((CComboDescription*)pDescri)->m_nComboOwnerDrawType));
	}

	switch (value){
	case EOwnerDrawType::Fixed:ExtendedWindow->SetStyle(CBS_OWNERDRAWVARIABLE, CBS_OWNERDRAWFIXED);		break;
	case EOwnerDrawType::Variable:ExtendedWindow->SetStyle(CBS_OWNERDRAWFIXED, CBS_OWNERDRAWVARIABLE);	break;
	}
}

#pragma endregion 


#pragma region MLabelProperties
//----------------------------------------------------------------------------
MLabelProperties::MLabelProperties(IEasyBuilderComponentExtendable^ owner, EasyBuilderComponent^ parentComponent, System::String^ name)
	:
	MBaseWindowWrapperProperties(owner, parentComponent, name)
{
}

//----------------------------------------------------------------------------
MLabelProperties::~MLabelProperties()
{
	delete m_pCustomFont;
	m_pCustomFont = NULL;
}

//----------------------------------------------------------------------------
bool MLabelProperties::Bitmap::get()
{
	CWndObjDescription* pDescri = ExtendedWindow->GetWndObjDescription();
	return pDescri && pDescri->IsKindOf(RUNTIME_CLASS(CLabelDescription)) ?
		((CLabelDescription*)pDescri)->m_bBitmap
		: ExtendedWindow->HasStyle(SS_BITMAP);
}
//----------------------------------------------------------------------------
void MLabelProperties::Bitmap::set(bool value)
{
	CWndObjDescription* pDescri = ExtendedWindow->GetWndObjDescription();
	if (pDescri && pDescri->IsKindOf(RUNTIME_CLASS(CLabelDescription)))
		((CLabelDescription*)pDescri)->m_bBitmap = value;
	ExtendedWindow->SetStyle(SET_STYLE_PARAMS(SS_BITMAP));
}

//----------------------------------------------------------------------------
bool MLabelProperties::BlackFrame::get()
{
	CWndObjDescription* pDescri = ExtendedWindow->GetWndObjDescription();
	return pDescri && pDescri->IsKindOf(RUNTIME_CLASS(CLabelDescription)) ?
		((CLabelDescription*)pDescri)->m_bBlackFrame
		: ExtendedWindow->HasStyle(SS_BLACKFRAME);
}
//----------------------------------------------------------------------------
void MLabelProperties::BlackFrame::set(bool value)
{
	CWndObjDescription* pDescri = ExtendedWindow->GetWndObjDescription();
	if (pDescri && pDescri->IsKindOf(RUNTIME_CLASS(CLabelDescription)))
		((CLabelDescription*)pDescri)->m_bBlackFrame = value;
	ExtendedWindow->SetStyle(SET_STYLE_PARAMS(SS_BLACKFRAME));
}

//----------------------------------------------------------------------------
bool MLabelProperties::BlackRect::get()
{
	CWndObjDescription* pDescri = ExtendedWindow->GetWndObjDescription();
	return pDescri && pDescri->IsKindOf(RUNTIME_CLASS(CLabelDescription)) ?
		((CLabelDescription*)pDescri)->m_bBlackRect
		: ExtendedWindow->HasStyle(SS_BLACKRECT);
}
//----------------------------------------------------------------------------
void MLabelProperties::BlackRect::set(bool value)
{
	CWndObjDescription* pDescri = ExtendedWindow->GetWndObjDescription();
	if (pDescri && pDescri->IsKindOf(RUNTIME_CLASS(CLabelDescription)))
		((CLabelDescription*)pDescri)->m_bBlackRect = value;
	ExtendedWindow->SetStyle(SET_STYLE_PARAMS(SS_BLACKRECT));
}

//----------------------------------------------------------------------------
bool MLabelProperties::CenterImage::get()
{
	CWndObjDescription* pDescri = ExtendedWindow->GetWndObjDescription();
	return pDescri && pDescri->IsKindOf(RUNTIME_CLASS(CLabelDescription)) ?
		((CLabelDescription*)pDescri)->m_bCenterImage
		: ExtendedWindow->HasStyle(SS_CENTERIMAGE);
}
//----------------------------------------------------------------------------
void MLabelProperties::CenterImage::set(bool value)
{
	CWndObjDescription* pDescri = ExtendedWindow->GetWndObjDescription();
	if (pDescri && pDescri->IsKindOf(RUNTIME_CLASS(CLabelDescription)))
		((CLabelDescription*)pDescri)->m_bCenterImage = value;
	ExtendedWindow->SetStyle(SET_STYLE_PARAMS(SS_CENTERIMAGE));
}

//----------------------------------------------------------------------------
bool MLabelProperties::EditControl::get()
{
	CWndObjDescription* pDescri = ExtendedWindow->GetWndObjDescription();
	return pDescri && pDescri->IsKindOf(RUNTIME_CLASS(CLabelDescription)) ?
		((CLabelDescription*)pDescri)->m_bEditControl
		: ExtendedWindow->HasStyle(SS_EDITCONTROL);
}
//----------------------------------------------------------------------------
void MLabelProperties::EditControl::set(bool value)
{
	CWndObjDescription* pDescri = ExtendedWindow->GetWndObjDescription();
	if (pDescri && pDescri->IsKindOf(RUNTIME_CLASS(CLabelDescription)))
		((CLabelDescription*)pDescri)->m_bEditControl = value;
	ExtendedWindow->SetStyle(SET_STYLE_PARAMS(SS_EDITCONTROL));
}

//----------------------------------------------------------------------------
bool MLabelProperties::EndEllipsis::get()
{
	CWndObjDescription* pDescri = ExtendedWindow->GetWndObjDescription();
	return pDescri && pDescri->IsKindOf(RUNTIME_CLASS(CLabelDescription)) ?
		((CLabelDescription*)pDescri)->m_bEndEllipsis
		: ExtendedWindow->HasStyle(SS_ENDELLIPSIS);
}
//----------------------------------------------------------------------------
void MLabelProperties::EndEllipsis::set(bool value)
{
	CWndObjDescription* pDescri = ExtendedWindow->GetWndObjDescription();
	if (pDescri && pDescri->IsKindOf(RUNTIME_CLASS(CLabelDescription)))
		((CLabelDescription*)pDescri)->m_bEndEllipsis = value;
	ExtendedWindow->SetStyle(SET_STYLE_PARAMS(SS_ENDELLIPSIS));
}

//----------------------------------------------------------------------------
System::Drawing::Font^ MLabelProperties::Font::get()
{
	CWndObjDescription* pDesc = ExtendedWindow->GetWndObjDescription();
	System::Drawing::FontStyle fontStyle = System::Drawing::FontStyle::Regular;
	CFont* pFont = m_pCustomFont ? m_pCustomFont : AfxGetControlFont(); //default per TB quanfo il font name corrisponde a questo non va serializzato
	if (pFont == NULL)
			return nullptr;
	LOGFONT lf;
	pFont->GetLogFont(&lf);
	int hToPoint = 0; 
	if (pDesc && pDesc->m_pFontDescription) 
	{
		if( pDesc->m_pFontDescription->m_bIsBold)		fontStyle = fontStyle | System::Drawing::FontStyle::Bold;
		if (pDesc->m_pFontDescription->m_bIsItalic)		fontStyle = fontStyle | System::Drawing::FontStyle::Italic;
		if (pDesc->m_pFontDescription->m_bIsUnderline)	fontStyle = fontStyle | System::Drawing::FontStyle::Underline;
		if (pDesc->m_pFontDescription->m_nFontSize = 0)
			hToPoint = (int)pDesc->m_pFontDescription->m_nFontSize;
		else
			hToPoint = GetDisplayFontPointSize(lf.lfHeight);
		return gcnew System::Drawing::Font(
			gcnew System::String(pDesc->m_pFontDescription->m_strFaceName), 
			(float)hToPoint, fontStyle);
	}
		
	if (lf.lfItalic) fontStyle = fontStyle | System::Drawing::FontStyle::Italic;
	if (lf.lfUnderline) fontStyle = fontStyle | System::Drawing::FontStyle::Underline;
	if (lf.lfWeight>=700) fontStyle = fontStyle | System::Drawing::FontStyle::Bold;
	hToPoint = GetDisplayFontPointSize(lf.lfHeight);
	return gcnew System::Drawing::Font(gcnew System::String(lf.lfFaceName), (float)hToPoint, fontStyle);

}
//----------------------------------------------------------------------------
void MLabelProperties::Font::set(System::Drawing::Font^ value)
{
	CWndObjDescription* pDesc = ExtendedWindow->GetWndObjDescription();
	int pointToH = abs(GetDisplayFontHeight((int)(value->SizeInPoints)));

	if (pDesc)
		pDesc->SetFont(value->Name, (float)pointToH, value->Bold, value->Italic, value->Underline);
	CWnd* pWnd = ExtendedWindow->GetWnd();
	if (!pWnd)
		return;
	CCustomFont* pCustomFont = dynamic_cast<CCustomFont*>(pWnd);
	if (pCustomFont){
		pCustomFont->SetOwnFont(value->Bold, value->Italic, value->Underline, (int)(value->SizeInPoints), CString(value->Name));
		return;
	}
	
	LOGFONT lf;
	ZeroMemory(&lf, sizeof(LOGFONT));
	CFont* pCurrentFont = pWnd->GetFont();
	if (pCurrentFont)
		pCurrentFont->GetLogFont(&lf);
	CFont* pFont = new CFont();
	
	lf.lfHeight = pointToH;		
	lf.lfItalic = value->Italic;
	lf.lfUnderline = value->Underline; 
	lf.lfWeight = value->Bold ? 700 : 400;
	_tcscpy_s(lf.lfFaceName, CString(value->Name));

	pFont->CreateFontIndirect(&lf);
	pWnd->SetFont(pFont);

	delete m_pCustomFont;
	m_pCustomFont = pFont;
	
}

//----------------------------------------------------------------------------
bool MLabelProperties::GrayFrame::get()
{
	CWndObjDescription* pDescri = ExtendedWindow->GetWndObjDescription();
	return pDescri && pDescri->IsKindOf(RUNTIME_CLASS(CLabelDescription)) ?
		((CLabelDescription*)pDescri)->m_bGrayFrame
		: ExtendedWindow->HasStyle(SS_GRAYFRAME);
}
//----------------------------------------------------------------------------
void MLabelProperties::GrayFrame::set(bool value)
{
	CWndObjDescription* pDescri = ExtendedWindow->GetWndObjDescription();
	if (pDescri && pDescri->IsKindOf(RUNTIME_CLASS(CLabelDescription)))
		((CLabelDescription*)pDescri)->m_bGrayFrame = value;
	ExtendedWindow->SetStyle(SET_STYLE_PARAMS(SS_GRAYFRAME));
}

//----------------------------------------------------------------------------
bool MLabelProperties::GrayRect::get()
{
	CWndObjDescription* pDescri = ExtendedWindow->GetWndObjDescription();
	return pDescri && pDescri->IsKindOf(RUNTIME_CLASS(CLabelDescription)) ?
		((CLabelDescription*)pDescri)->m_bGrayRect
		: ExtendedWindow->HasStyle(SS_GRAYRECT);
}
//----------------------------------------------------------------------------
void MLabelProperties::GrayRect::set(bool value)
{
	CWndObjDescription* pDescri = ExtendedWindow->GetWndObjDescription();
	if (pDescri && pDescri->IsKindOf(RUNTIME_CLASS(CLabelDescription)))
		((CLabelDescription*)pDescri)->m_bGrayRect = value;
	ExtendedWindow->SetStyle(SET_STYLE_PARAMS(SS_GRAYRECT));
}

//----------------------------------------------------------------------------
bool MLabelProperties::Icon::get()
{
	CWndObjDescription* pDescri = ExtendedWindow->GetWndObjDescription();
	return pDescri && pDescri->IsKindOf(RUNTIME_CLASS(CLabelDescription)) ?
		((CLabelDescription*)pDescri)->m_bIcon
		: ExtendedWindow->HasStyle(SS_ICON);
}
//----------------------------------------------------------------------------
void MLabelProperties::Icon::set(bool value)
{
	CWndObjDescription* pDescri = ExtendedWindow->GetWndObjDescription();
	if (pDescri && pDescri->IsKindOf(RUNTIME_CLASS(CLabelDescription)))
		((CLabelDescription*)pDescri)->m_bIcon = value;
	ExtendedWindow->SetStyle(SET_STYLE_PARAMS(SS_ICON));
}

//----------------------------------------------------------------------------
bool MLabelProperties::LeftNoWrap::get()
{
	CWndObjDescription* pDescri = ExtendedWindow->GetWndObjDescription();
	return pDescri && pDescri->IsKindOf(RUNTIME_CLASS(CLabelDescription)) ?
		((CLabelDescription*)pDescri)->m_bLeftNoWrap
		: ExtendedWindow->HasStyle(SS_LEFTNOWORDWRAP);
}
//----------------------------------------------------------------------------
void MLabelProperties::LeftNoWrap::set(bool value)
{
	CWndObjDescription* pDescri = ExtendedWindow->GetWndObjDescription();
	if (pDescri && pDescri->IsKindOf(RUNTIME_CLASS(CLabelDescription)))
		((CLabelDescription*)pDescri)->m_bLeftNoWrap = value;
	ExtendedWindow->SetStyle(SET_STYLE_PARAMS(SS_LEFTNOWORDWRAP));
}

//----------------------------------------------------------------------------
bool MLabelProperties::NoPrefix::get()
{
	CWndObjDescription* pDescri = ExtendedWindow->GetWndObjDescription();
	return pDescri && pDescri->IsKindOf(RUNTIME_CLASS(CLabelDescription)) ?
		((CLabelDescription*)pDescri)->m_bNoPrefix
		: ExtendedWindow->HasStyle(SS_NOPREFIX);
}
//----------------------------------------------------------------------------
void MLabelProperties::NoPrefix::set(bool value)
{
	CWndObjDescription* pDescri = ExtendedWindow->GetWndObjDescription();
	if (pDescri && pDescri->IsKindOf(RUNTIME_CLASS(CLabelDescription)))
		((CLabelDescription*)pDescri)->m_bNoPrefix = value;
	ExtendedWindow->SetStyle(SET_STYLE_PARAMS(SS_NOPREFIX));
}

//----------------------------------------------------------------------------
bool MLabelProperties::OwnerDraw::get()
{
	CWndObjDescription* pDescri = ExtendedWindow->GetWndObjDescription();
	return pDescri && pDescri->IsKindOf(RUNTIME_CLASS(CLabelDescription)) ?
		((CLabelDescription*)pDescri)->m_bOwnerDraw
		: ExtendedWindow->HasStyle(SS_OWNERDRAW);
}
//----------------------------------------------------------------------------
void MLabelProperties::OwnerDraw::set(bool value)
{
	CWndObjDescription* pDescri = ExtendedWindow->GetWndObjDescription();
	if (pDescri && pDescri->IsKindOf(RUNTIME_CLASS(CLabelDescription)))
		((CLabelDescription*)pDescri)->m_bOwnerDraw = value;
	ExtendedWindow->SetStyle(SET_STYLE_PARAMS(SS_OWNERDRAW));
}

//----------------------------------------------------------------------------
bool MLabelProperties::PathEllipsis::get()
{
	CWndObjDescription* pDescri = ExtendedWindow->GetWndObjDescription();
	return pDescri && pDescri->IsKindOf(RUNTIME_CLASS(CLabelDescription)) ?
		((CLabelDescription*)pDescri)->m_bPathEllipsis
		: ExtendedWindow->HasStyle(SS_PATHELLIPSIS);
}
//----------------------------------------------------------------------------
void MLabelProperties::PathEllipsis::set(bool value)
{
	CWndObjDescription* pDescri = ExtendedWindow->GetWndObjDescription();
	if (pDescri && pDescri->IsKindOf(RUNTIME_CLASS(CLabelDescription)))
		((CLabelDescription*)pDescri)->m_bPathEllipsis = value;
	ExtendedWindow->SetStyle(SET_STYLE_PARAMS(SS_PATHELLIPSIS));
}

//----------------------------------------------------------------------------
bool MLabelProperties::Simple::get()
{
	CWndObjDescription* pDescri = ExtendedWindow->GetWndObjDescription();
	return pDescri && pDescri->IsKindOf(RUNTIME_CLASS(CLabelDescription)) ?
		((CLabelDescription*)pDescri)->m_bSimple
		: ExtendedWindow->HasStyle(SS_SIMPLE);
}
//----------------------------------------------------------------------------
void MLabelProperties::Simple::set(bool value)
{
	CWndObjDescription* pDescri = ExtendedWindow->GetWndObjDescription();
	if (pDescri && pDescri->IsKindOf(RUNTIME_CLASS(CLabelDescription)))
		((CLabelDescription*)pDescri)->m_bSimple = value;
	ExtendedWindow->SetStyle(SET_STYLE_PARAMS(SS_SIMPLE));
}

//----------------------------------------------------------------------------
bool MLabelProperties::WhiteFrame::get()
{
	CWndObjDescription* pDescri = ExtendedWindow->GetWndObjDescription();
	return pDescri && pDescri->IsKindOf(RUNTIME_CLASS(CLabelDescription)) ?
		((CLabelDescription*)pDescri)->m_bWhiteFrame
		: ExtendedWindow->HasStyle(SS_WHITEFRAME);
}
//----------------------------------------------------------------------------
void MLabelProperties::WhiteFrame::set(bool value)
{
	CWndObjDescription* pDescri = ExtendedWindow->GetWndObjDescription();
	if (pDescri && pDescri->IsKindOf(RUNTIME_CLASS(CLabelDescription)))
		((CLabelDescription*)pDescri)->m_bWhiteFrame = value;
	ExtendedWindow->SetStyle(SET_STYLE_PARAMS(SS_WHITEFRAME));
}

//----------------------------------------------------------------------------
bool MLabelProperties::WhiteRect::get()
{
	CWndObjDescription* pDescri = ExtendedWindow->GetWndObjDescription();
	return pDescri && pDescri->IsKindOf(RUNTIME_CLASS(CLabelDescription)) ?
		((CLabelDescription*)pDescri)->m_bWhiteRect
		: ExtendedWindow->HasStyle(SS_WHITERECT);
}
//----------------------------------------------------------------------------
void MLabelProperties::WhiteRect::set(bool value)
{
	CWndObjDescription* pDescri = ExtendedWindow->GetWndObjDescription();
	if (pDescri && pDescri->IsKindOf(RUNTIME_CLASS(CLabelDescription)))
		((CLabelDescription*)pDescri)->m_bWhiteRect = value;
	ExtendedWindow->SetStyle(SET_STYLE_PARAMS(SS_WHITERECT));
}

//----------------------------------------------------------------------------
bool MLabelProperties::WordEllipsis::get()
{
	CWndObjDescription* pDescri = ExtendedWindow->GetWndObjDescription();
	return pDescri && pDescri->IsKindOf(RUNTIME_CLASS(CLabelDescription)) ?
		((CLabelDescription*)pDescri)->m_bWordEllipsis
		: ExtendedWindow->HasStyle(SS_WORDELLIPSIS);
}
//----------------------------------------------------------------------------
void MLabelProperties::WordEllipsis::set(bool value)
{
	CWndObjDescription* pDescri = ExtendedWindow->GetWndObjDescription();
	if (pDescri && pDescri->IsKindOf(RUNTIME_CLASS(CLabelDescription)))
		((CLabelDescription*)pDescri)->m_bWordEllipsis = value;
	ExtendedWindow->SetStyle(SET_STYLE_PARAMS(SS_WORDELLIPSIS));
}

//----------------------------------------------------------------------------
bool MLabelProperties::Notify::get()
{
	CWndObjDescription* pDescri = ExtendedWindow->GetWndObjDescription();
	return pDescri && pDescri->IsKindOf(RUNTIME_CLASS(CLabelDescription)) ?
		((CLabelDescription*)pDescri)->m_bNotify
		: ExtendedWindow->HasStyle(SS_NOTIFY);
}
//----------------------------------------------------------------------------
void MLabelProperties::Notify::set(bool value)
{
	CWndObjDescription* pDescri = ExtendedWindow->GetWndObjDescription();
	if (pDescri && pDescri->IsKindOf(RUNTIME_CLASS(CLabelDescription)))
		((CLabelDescription*)pDescri)->m_bNotify = value;
	ExtendedWindow->SetStyle(SET_STYLE_PARAMS(SS_NOTIFY));
}

//----------------------------------------------------------------------------
EEtchedFrame MLabelProperties::EtchedFrame::get()
{
	CWndObjDescription* pDescri = ExtendedWindow->GetWndObjDescription();
	if( pDescri && pDescri->IsKindOf(RUNTIME_CLASS(CLabelDescription)) )
		 return (EEtchedFrame)((CLabelDescription*)pDescri)->m_EtchedFrame;

	if (ExtendedWindow->HasStyle(SS_ETCHEDFRAME))		return EEtchedFrame::All;
	else if (ExtendedWindow->HasStyle(SS_ETCHEDHORZ))	return EEtchedFrame::Horz;
	else if (ExtendedWindow->HasStyle(SS_ETCHEDVERT))	return EEtchedFrame::Vert;
	return EEtchedFrame::None;
}
//----------------------------------------------------------------------------
void MLabelProperties::EtchedFrame::set(EEtchedFrame value)
{
	CWndObjDescription* pDescri = ExtendedWindow->GetWndObjDescription();
	if (pDescri && pDescri->IsKindOf(RUNTIME_CLASS(CLabelDescription)) && ((CLabelDescription*)pDescri)->m_EtchedFrame != (::EtchedFrameType)value)
	{
		((CLabelDescription*)pDescri)->m_EtchedFrame = (::EtchedFrameType)value;
		((CLabelDescription*)pDescri)->SetUpdated(&(((CLabelDescription*)pDescri)->m_EtchedFrame));
	}

	switch (value){
	case EEtchedFrame::All:	ExtendedWindow->SetStyle(SS_ETCHEDHORZ | SS_ETCHEDVERT, SS_ETCHEDFRAME); break;
	case EEtchedFrame::Horz:ExtendedWindow->SetStyle(SS_ETCHEDFRAME | SS_ETCHEDVERT, SS_ETCHEDHORZ); break;
	case EEtchedFrame::Vert:ExtendedWindow->SetStyle(SS_ETCHEDFRAME | SS_ETCHEDHORZ, SS_ETCHEDVERT); break;
	}
}

//----------------------------------------------------------------------------
ETextAlign MLabelProperties::TextAlignment::get()
{
	CWndObjDescription* pDescri = ExtendedWindow->GetWndObjDescription();
	if (pDescri && pDescri->IsKindOf(RUNTIME_CLASS(CLabelDescription)))
		return (ETextAlign)((CLabelDescription*)pDescri)->m_textAlign;

	if (ExtendedWindow->HasStyle(SS_CENTER))		return ETextAlign::Center;
	else if (ExtendedWindow->HasStyle(SS_RIGHT))	return ETextAlign::Right;
	return ETextAlign::Left;		//è il default
}
//----------------------------------------------------------------------------
void MLabelProperties::TextAlignment::set(ETextAlign value)
{
	CWndObjDescription* pDescri = ExtendedWindow->GetWndObjDescription();
	if (pDescri && pDescri->IsKindOf(RUNTIME_CLASS(CLabelDescription)) && ((CLabelDescription*)pDescri)->m_textAlign != (::TextAlignment)value)
	{
		((CLabelDescription*)pDescri)->m_textAlign = (::TextAlignment)value;
		((CLabelDescription*)pDescri)->SetUpdated(&(((CLabelDescription*)pDescri)->m_textAlign));
	}

	switch (value){
		case ETextAlign::Left:		ExtendedWindow->SetStyle(SS_RIGHT | SS_CENTER, SS_LEFT);   break;
		case ETextAlign::Right:		ExtendedWindow->SetStyle(SS_LEFT | SS_CENTER, SS_RIGHT);  break;
		case ETextAlign::Center:	ExtendedWindow->SetStyle(SS_LEFT | SS_RIGHT, SS_CENTER); break;
	}
}

//----------------------------------------------------------------------------
bool MLabelProperties::Sunken::get()
{
	CWndObjDescription* pDescri = ExtendedWindow->GetWndObjDescription();
	return pDescri && pDescri->IsKindOf(RUNTIME_CLASS(CLabelDescription)) ?
		((CLabelDescription*)pDescri)->m_bSunken
		: ExtendedWindow->HasStyle(SS_SUNKEN);
}

//----------------------------------------------------------------------------
void MLabelProperties::Sunken::set(bool value)
{
	CWndObjDescription* pDescri = ExtendedWindow->GetWndObjDescription();
	if (pDescri && pDescri->IsKindOf(RUNTIME_CLASS(CLabelDescription)))
		((CLabelDescription*)pDescri)->m_bSunken = value;
	ExtendedWindow->SetStyle(SET_STYLE_PARAMS(SS_SUNKEN));
}

#pragma endregion 


#pragma region MListBoxProperties

//----------------------------------------------------------------------------
MListBoxProperties::MListBoxProperties(IEasyBuilderComponentExtendable^ owner, EasyBuilderComponent^ parentComponent, System::String^ name)
	:
	MBaseWindowWrapperProperties(owner, parentComponent, name)
{
}

//----------------------------------------------------------------------------
bool MListBoxProperties::Notify::get()
{
	CWndObjDescription* pDescri = ExtendedWindow->GetWndObjDescription();
	return pDescri && pDescri->IsKindOf(RUNTIME_CLASS(CListDescription)) ?
		((CListDescription*)pDescri)->m_bNotify
		: ExtendedWindow->HasStyle(LBS_NOTIFY);
}
//----------------------------------------------------------------------------
void MListBoxProperties::Notify::set(bool value)
{
	CWndObjDescription* pDescri = ExtendedWindow->GetWndObjDescription();
	if (pDescri && pDescri->IsKindOf(RUNTIME_CLASS(CListDescription)))
		((CListDescription*)pDescri)->m_bNotify = value;
	ExtendedWindow->SetStyle(SET_STYLE_PARAMS(LBS_NOTIFY));
}

//----------------------------------------------------------------------------
bool MListBoxProperties::DisableNoScroll::get()
{
	CWndObjDescription* pDescri = ExtendedWindow->GetWndObjDescription();
	return pDescri && pDescri->IsKindOf(RUNTIME_CLASS(CListDescription)) ?
		((CListDescription*)pDescri)->m_bDisableNoScroll
		: ExtendedWindow->HasStyle(LBS_DISABLENOSCROLL);
}
//----------------------------------------------------------------------------
void MListBoxProperties::DisableNoScroll::set(bool value)
{
	CWndObjDescription* pDescri = ExtendedWindow->GetWndObjDescription();
	if (pDescri && pDescri->IsKindOf(RUNTIME_CLASS(CListDescription)))
		((CListDescription*)pDescri)->m_bDisableNoScroll = value;
	ExtendedWindow->SetStyle(SET_STYLE_PARAMS(LBS_DISABLENOSCROLL));
}

//----------------------------------------------------------------------------
bool MListBoxProperties::HasStrings::get()
{
	CWndObjDescription* pDescri = ExtendedWindow->GetWndObjDescription();
	return pDescri && pDescri->IsKindOf(RUNTIME_CLASS(CListDescription)) ?
		((CListDescription*)pDescri)->m_bHasStrings
		: ExtendedWindow->HasStyle(LBS_HASSTRINGS);
}
//----------------------------------------------------------------------------
void MListBoxProperties::HasStrings::set(bool value)
{
	CWndObjDescription* pDescri = ExtendedWindow->GetWndObjDescription();
	if (pDescri && pDescri->IsKindOf(RUNTIME_CLASS(CListDescription)))
		((CListDescription*)pDescri)->m_bHasStrings = value;
	ExtendedWindow->SetStyle(SET_STYLE_PARAMS(LBS_HASSTRINGS));
}

//----------------------------------------------------------------------------
bool MListBoxProperties::NoIntegralHeight::get()
{
	CWndObjDescription* pDescri = ExtendedWindow->GetWndObjDescription();
	return pDescri && pDescri->IsKindOf(RUNTIME_CLASS(CListDescription)) ?
		((CListDescription*)pDescri)->m_bNoIntegralHeight
		: ExtendedWindow->HasStyle(LBS_NOINTEGRALHEIGHT);
}
//----------------------------------------------------------------------------
void MListBoxProperties::NoIntegralHeight::set(bool value)
{
	CWndObjDescription* pDescri = ExtendedWindow->GetWndObjDescription();
	if (pDescri && pDescri->IsKindOf(RUNTIME_CLASS(CListDescription)))
		((CListDescription*)pDescri)->m_bNoIntegralHeight = value;
	ExtendedWindow->SetStyle(SET_STYLE_PARAMS(LBS_NOINTEGRALHEIGHT));
}

//----------------------------------------------------------------------------
bool MListBoxProperties::Sort::get()
{
	CWndObjDescription* pDescri = ExtendedWindow->GetWndObjDescription();
	return pDescri && pDescri->IsKindOf(RUNTIME_CLASS(CListDescription)) ?
		((CListDescription*)pDescri)->m_bSort
		: ExtendedWindow->HasStyle(LBS_SORT);
}
//----------------------------------------------------------------------------
void MListBoxProperties::Sort::set(bool value)
{
	CWndObjDescription* pDescri = ExtendedWindow->GetWndObjDescription();
	if (pDescri && pDescri->IsKindOf(RUNTIME_CLASS(CListDescription)))
		((CListDescription*)pDescri)->m_bSort = value;
	ExtendedWindow->SetStyle(SET_STYLE_PARAMS(LBS_SORT));
}

//----------------------------------------------------------------------------
bool MListBoxProperties::WantKeyInput::get()
{
	CWndObjDescription* pDescri = ExtendedWindow->GetWndObjDescription();
	return pDescri && pDescri->IsKindOf(RUNTIME_CLASS(CListDescription)) ?
		((CListDescription*)pDescri)->m_bWantKeyInput
		: ExtendedWindow->HasStyle(LBS_WANTKEYBOARDINPUT);
}
//----------------------------------------------------------------------------
void MListBoxProperties::WantKeyInput::set(bool value)
{
	CWndObjDescription* pDescri = ExtendedWindow->GetWndObjDescription();
	if (pDescri && pDescri->IsKindOf(RUNTIME_CLASS(CListDescription)))
		((CListDescription*)pDescri)->m_bWantKeyInput = value;
	ExtendedWindow->SetStyle(SET_STYLE_PARAMS(LBS_WANTKEYBOARDINPUT));
}

//----------------------------------------------------------------------------
bool MListBoxProperties::MultiColumn::get()
{
	CWndObjDescription* pDescri = ExtendedWindow->GetWndObjDescription();
	return pDescri && pDescri->IsKindOf(RUNTIME_CLASS(CListDescription)) ?
		((CListDescription*)pDescri)->m_bMultiColumn
		: ExtendedWindow->HasStyle(LBS_MULTICOLUMN);
}
//----------------------------------------------------------------------------
void MListBoxProperties::MultiColumn::set(bool value)
{
	CWndObjDescription* pDescri = ExtendedWindow->GetWndObjDescription();
	if (pDescri && pDescri->IsKindOf(RUNTIME_CLASS(CListDescription)))
		((CListDescription*)pDescri)->m_bMultiColumn = value;
	ExtendedWindow->SetStyle(SET_STYLE_PARAMS(LBS_MULTICOLUMN));
}

//----------------------------------------------------------------------------
EOwnerDrawType MListBoxProperties::OwnerDrawType::get()
{
	CWndObjDescription* pDescri = ExtendedWindow->GetWndObjDescription();
	if (pDescri && pDescri->IsKindOf(RUNTIME_CLASS(CListDescription)))
		return (EOwnerDrawType)((CListDescription*)pDescri)->m_nOwnerDraw;

	if (ExtendedWindow->HasStyle(LBS_OWNERDRAWFIXED))		return EOwnerDrawType::Fixed;
	else if (ExtendedWindow->HasStyle(LBS_OWNERDRAWVARIABLE))	return EOwnerDrawType::Variable;
	return EOwnerDrawType::None;
}
//----------------------------------------------------------------------------
void MListBoxProperties::OwnerDrawType::set(EOwnerDrawType value)
{
	CWndObjDescription* pDescri = ExtendedWindow->GetWndObjDescription();
	if (pDescri && pDescri->IsKindOf(RUNTIME_CLASS(CListDescription)) && ((CListDescription*)pDescri)->m_nOwnerDraw != (::OwnerDrawType)value)
	{
		((CListDescription*)pDescri)->m_nOwnerDraw = (::OwnerDrawType)value;
		((CListDescription*)pDescri)->SetUpdated(&(((CListDescription*)pDescri)->m_nOwnerDraw));
	}

	switch (value){
	case EOwnerDrawType::Fixed:	ExtendedWindow->SetStyle(LBS_OWNERDRAWVARIABLE, LBS_OWNERDRAWFIXED);		break;
	case EOwnerDrawType::Variable:ExtendedWindow->SetStyle(LBS_OWNERDRAWFIXED, LBS_OWNERDRAWVARIABLE);	break;
	}
}

//----------------------------------------------------------------------------
ESelectionType MListBoxProperties::SelectionType::get()
{
	CWndObjDescription* pDescri = ExtendedWindow->GetWndObjDescription();
	if (pDescri && pDescri->IsKindOf(RUNTIME_CLASS(CListDescription)))
		return (ESelectionType)((CListDescription*)pDescri)->m_nSelection;

	if (ExtendedWindow->HasStyle(LBS_EXTENDEDSEL))	return ESelectionType::Extended;
	else if (ExtendedWindow->HasStyle(LBS_MULTIPLESEL))	return ESelectionType::Multiple;
	return ESelectionType::None;
}
//----------------------------------------------------------------------------
void MListBoxProperties::SelectionType::set(ESelectionType value)
{
	CWndObjDescription* pDescri = ExtendedWindow->GetWndObjDescription();
	if (pDescri && pDescri->IsKindOf(RUNTIME_CLASS(CListDescription)) && ((CListDescription*)pDescri)->m_nSelection != (::SelectionType)value)
	{
		((CListDescription*)pDescri)->m_nSelection = (::SelectionType)value;
		((CListDescription*)pDescri)->SetUpdated(&(((CListDescription*)pDescri)->m_nSelection));
	}

	switch (value){
	case ESelectionType::None:		ExtendedWindow->SetStyle(LBS_EXTENDEDSEL | LBS_MULTIPLESEL, LBS_NOSEL);	break;
	case ESelectionType::Extended:	ExtendedWindow->SetStyle(LBS_EXTENDEDSEL | LBS_NOSEL, LBS_EXTENDEDSEL); break;
	case ESelectionType::Multiple:	ExtendedWindow->SetStyle(LBS_EXTENDEDSEL | LBS_NOSEL, LBS_MULTIPLESEL); break;
	}
};



#pragma endregion 


#pragma region MButtonProperties

//----------------------------------------------------------------------------
MButtonProperties::MButtonProperties(IEasyBuilderComponentExtendable^ owner, EasyBuilderComponent^ parentComponent, System::String^ name)
	:
	MBaseWindowWrapperProperties(owner, parentComponent, name)
{
}

//----------------------------------------------------------------------------
bool MButtonProperties::Multiline::get()
{
	CWndObjDescription* pDescri = ExtendedWindow->GetWndObjDescription();
	return pDescri && pDescri->IsKindOf(RUNTIME_CLASS(CWndButtonDescription)) ?
		((CWndButtonDescription*)pDescri)->m_bMultiline
		: ExtendedWindow->HasStyle(BS_MULTILINE);
}
//----------------------------------------------------------------------------
void MButtonProperties::Multiline::set(bool value)
{
	CWndObjDescription* pDescri = ExtendedWindow->GetWndObjDescription();
	if (pDescri && pDescri->IsKindOf(RUNTIME_CLASS(CWndButtonDescription)))
		((CWndButtonDescription*)pDescri)->m_bMultiline = value;
	ExtendedWindow->SetStyle(SET_STYLE_PARAMS(BS_MULTILINE));
}

//----------------------------------------------------------------------------
bool MButtonProperties::Bitmap::get()
{
	CWndObjDescription* pDescri = ExtendedWindow->GetWndObjDescription();
	return pDescri && pDescri->IsKindOf(RUNTIME_CLASS(CWndButtonDescription)) ?
		((CWndButtonDescription*)pDescri)->m_bBitmap
		: ExtendedWindow->HasStyle(BS_BITMAP);
}
//----------------------------------------------------------------------------
void MButtonProperties::Bitmap::set(bool value)
{
	CWndObjDescription* pDescri = ExtendedWindow->GetWndObjDescription();
	if (pDescri && pDescri->IsKindOf(RUNTIME_CLASS(CWndButtonDescription)))
		((CWndButtonDescription*)pDescri)->m_bBitmap = value;
	ExtendedWindow->SetStyle(SET_STYLE_PARAMS(BS_BITMAP));
}

//----------------------------------------------------------------------------
bool MButtonProperties::Icon::get()
{
	CWndObjDescription* pDescri = ExtendedWindow->GetWndObjDescription();
	return pDescri && pDescri->IsKindOf(RUNTIME_CLASS(CWndButtonDescription)) ?
		((CWndButtonDescription*)pDescri)->m_bIcon
		: ExtendedWindow->HasStyle(BS_ICON);
}
//----------------------------------------------------------------------------
void MButtonProperties::Icon::set(bool value)
{
	CWndObjDescription* pDescri = ExtendedWindow->GetWndObjDescription();
	if (pDescri && pDescri->IsKindOf(RUNTIME_CLASS(CWndButtonDescription)))
		((CWndButtonDescription*)pDescri)->m_bIcon = value;
	ExtendedWindow->SetStyle(SET_STYLE_PARAMS(BS_ICON));
}

//----------------------------------------------------------------------------
bool MButtonProperties::Flat::get()
{
	CWndObjDescription* pDescri = ExtendedWindow->GetWndObjDescription();
	return pDescri && pDescri->IsKindOf(RUNTIME_CLASS(CWndButtonDescription)) ?
		((CWndButtonDescription*)pDescri)->m_bFlat
		: ExtendedWindow->HasStyle(BS_FLAT);
}
//----------------------------------------------------------------------------
void MButtonProperties::Flat::set(bool value)
{
	CWndObjDescription* pDescri = ExtendedWindow->GetWndObjDescription();
	if (pDescri && pDescri->IsKindOf(RUNTIME_CLASS(CWndButtonDescription)))
		((CWndButtonDescription*)pDescri)->m_bFlat = value;
	ExtendedWindow->SetStyle(SET_STYLE_PARAMS(BS_FLAT));
}

//----------------------------------------------------------------------------
bool MButtonProperties::ClipChildren::get()
{
	CWndObjDescription* pDescri = ExtendedWindow->GetWndObjDescription();
	return pDescri && pDescri->IsKindOf(RUNTIME_CLASS(CWndButtonDescription)) ?
		((CWndButtonDescription*)pDescri)->m_bClipChildren
		: ExtendedWindow->HasStyle(WS_CLIPCHILDREN);
}
//----------------------------------------------------------------------------
void MButtonProperties::ClipChildren::set(bool value)
{
	CWndObjDescription* pDescri = ExtendedWindow->GetWndObjDescription();
	if (pDescri && pDescri->IsKindOf(RUNTIME_CLASS(CWndButtonDescription)))
		((CWndButtonDescription*)pDescri)->m_bClipChildren = value;
	ExtendedWindow->SetStyle(SET_STYLE_PARAMS(WS_CLIPCHILDREN));
}

//----------------------------------------------------------------------------
ETextAlign MButtonProperties::TextAlignment::get()
{
	CWndObjDescription* pDescri = ExtendedWindow->GetWndObjDescription();
	if (pDescri && pDescri->IsKindOf(RUNTIME_CLASS(CWndButtonDescription)))
		return (ETextAlign)((CWndButtonDescription*)pDescri)->m_textAlign;

	if (ExtendedWindow->HasStyle(BS_CENTER))		return ETextAlign::Center;
	else if (ExtendedWindow->HasStyle(BS_RIGHT))	return ETextAlign::Right;
	return ETextAlign::Left;		//è il default	
}
//----------------------------------------------------------------------------
void MButtonProperties::TextAlignment::set(ETextAlign value)
{
	CWndObjDescription* pDescri = ExtendedWindow->GetWndObjDescription();
	if (pDescri && pDescri->IsKindOf(RUNTIME_CLASS(CWndButtonDescription)) && ((CWndButtonDescription*)pDescri)->m_textAlign != (::TextAlignment)value)
	{
		((CWndButtonDescription*)pDescri)->m_textAlign = (::TextAlignment)value;
		((CWndButtonDescription*)pDescri)->SetUpdated(&(((CWndButtonDescription*)pDescri)->m_textAlign));
	}

	switch (value) {
	case ETextAlign::Left:		ExtendedWindow->SetStyle(BS_CENTER | BS_RIGHT, BS_LEFT);   break;
	case ETextAlign::Right:		ExtendedWindow->SetStyle(BS_CENTER | BS_LEFT, BS_RIGHT);  break;
	case ETextAlign::Center:	ExtendedWindow->SetStyle(BS_LEFT | BS_RIGHT, BS_CENTER); break;
	}
}

//----------------------------------------------------------------------------
EVerticalAlign MButtonProperties::VerticalAlign::get()
{
	CWndObjDescription* pDescri = ExtendedWindow->GetWndObjDescription();
	if (pDescri && pDescri->IsKindOf(RUNTIME_CLASS(CWndButtonDescription)))
		return (EVerticalAlign)((CWndButtonDescription*)pDescri)->m_vertAlign;

	if (ExtendedWindow->HasStyle(BS_VCENTER))	return EVerticalAlign::Center;
	else if (ExtendedWindow->HasStyle(BS_TOP))	return EVerticalAlign::Top;
	return EVerticalAlign::Bottom;
}
//----------------------------------------------------------------------------
void MButtonProperties::VerticalAlign::set(EVerticalAlign value)
{
	CWndObjDescription* pDescri = ExtendedWindow->GetWndObjDescription();
	if (pDescri && pDescri->IsKindOf(RUNTIME_CLASS(CWndButtonDescription)) && ((CWndButtonDescription*)pDescri)->m_vertAlign != (::VerticalAlignment)value)
	{
		((CWndButtonDescription*)pDescri)->m_vertAlign = (::VerticalAlignment)value;
		((CWndButtonDescription*)pDescri)->SetUpdated(&(((CWndButtonDescription*)pDescri)->m_vertAlign));
	}

	switch (value){
	case EVerticalAlign::Top:		ExtendedWindow->SetStyle(BS_VCENTER | BS_BOTTOM, BS_TOP);	break;
	case EVerticalAlign::Center:	ExtendedWindow->SetStyle(BS_TOP | BS_BOTTOM, BS_VCENTER);	break;
	case EVerticalAlign::Bottom:	ExtendedWindow->SetStyle(BS_TOP | BS_VCENTER, BS_BOTTOM);	break;
	}
};




#pragma endregion 


#pragma region MCheckBoxProperties

//----------------------------------------------------------------------------
MCheckBoxProperties::MCheckBoxProperties(IEasyBuilderComponentExtendable^ owner, EasyBuilderComponent^ parentComponent, System::String^ name)
	:
	MButtonProperties(owner, parentComponent, name)
{
}

//----------------------------------------------------------------------------
bool MCheckBoxProperties::LabelOnLeft::get()
{
	CWndObjDescription* pDescri = ExtendedWindow->GetWndObjDescription();
	return pDescri && pDescri->IsKindOf(RUNTIME_CLASS(CWndCheckRadioDescription)) ?
		((CWndCheckRadioDescription*)pDescri)->m_bLabelOnLeft
		: ExtendedWindow->HasStyle(BS_LEFTTEXT);
}
//----------------------------------------------------------------------------
void MCheckBoxProperties::LabelOnLeft::set(bool value)
{
	CWndObjDescription* pDescri = ExtendedWindow->GetWndObjDescription();
	if (pDescri && pDescri->IsKindOf(RUNTIME_CLASS(CWndCheckRadioDescription)))
		((CWndCheckRadioDescription*)pDescri)->m_bLabelOnLeft = value;
	ExtendedWindow->SetStyle(SET_STYLE_PARAMS(BS_LEFTTEXT));
}

//----------------------------------------------------------------------------
bool MCheckBoxProperties::Auto::get()
{
	CWndObjDescription* pDescri = ExtendedWindow->GetWndObjDescription();
	return pDescri && pDescri->IsKindOf(RUNTIME_CLASS(CWndCheckRadioDescription)) ?
		((CWndCheckRadioDescription*)pDescri)->m_bAutomatic
		: ExtendedWindow->HasStyle(BS_AUTOCHECKBOX);
}
//----------------------------------------------------------------------------
void MCheckBoxProperties::Auto::set(bool value)
{
	CWndObjDescription* pDescri = ExtendedWindow->GetWndObjDescription();
	if (pDescri && pDescri->IsKindOf(RUNTIME_CLASS(CWndCheckRadioDescription)))
		((CWndCheckRadioDescription*)pDescri)->m_bAutomatic = value;
	ExtendedWindow->SetStyle(value ? BS_CHECKBOX : BS_AUTOCHECKBOX, value ? BS_AUTOCHECKBOX : BS_CHECKBOX);
}

#pragma endregion


#pragma region MRadioButtonProperties

//----------------------------------------------------------------------------
MRadioButtonProperties::MRadioButtonProperties(IEasyBuilderComponentExtendable^ owner, EasyBuilderComponent^ parentComponent, System::String^ name)
	:
	MButtonProperties(owner, parentComponent, name)
{
}

//----------------------------------------------------------------------------
bool MRadioButtonProperties::LabelOnLeft::get()
{
	CWndObjDescription* pDescri = ExtendedWindow->GetWndObjDescription();
	return pDescri && pDescri->IsKindOf(RUNTIME_CLASS(CWndCheckRadioDescription)) ?
		((CWndCheckRadioDescription*)pDescri)->m_bLabelOnLeft
		: ExtendedWindow->HasStyle(BS_LEFTTEXT);
}
//----------------------------------------------------------------------------
void MRadioButtonProperties::LabelOnLeft::set(bool value)
{
	CWndObjDescription* pDescri = ExtendedWindow->GetWndObjDescription();
	if (pDescri && pDescri->IsKindOf(RUNTIME_CLASS(CWndCheckRadioDescription)))
		((CWndCheckRadioDescription*)pDescri)->m_bLabelOnLeft = value;
	ExtendedWindow->SetStyle(SET_STYLE_PARAMS(BS_LEFTTEXT));
}

//----------------------------------------------------------------------------
bool MRadioButtonProperties::Auto::get()
{
	CWndObjDescription* pDescri = ExtendedWindow->GetWndObjDescription();
	return pDescri && pDescri->IsKindOf(RUNTIME_CLASS(CWndCheckRadioDescription)) ?
		((CWndCheckRadioDescription*)pDescri)->m_bAutomatic
		: ExtendedWindow->HasStyle(BS_AUTORADIOBUTTON);
}

//----------------------------------------------------------------------------
void MRadioButtonProperties::Auto::set(bool value)
{
	CWndObjDescription* pDescri = ExtendedWindow->GetWndObjDescription();
	if (pDescri && pDescri->IsKindOf(RUNTIME_CLASS(CWndCheckRadioDescription)))
		((CWndCheckRadioDescription*)pDescri)->m_bAutomatic = value;
	ExtendedWindow->SetStyle(value ? BS_RADIOBUTTON : BS_AUTORADIOBUTTON, value ? BS_AUTORADIOBUTTON : BS_RADIOBUTTON);
}

#pragma endregion



#pragma region MGroupBoxProperties

//----------------------------------------------------------------------------
MGroupBoxProperties::MGroupBoxProperties(IEasyBuilderComponentExtendable^ owner, EasyBuilderComponent^ parentComponent, System::String^ name)
	:
	MButtonProperties(owner, parentComponent, name)
{
}

#pragma endregion



#pragma region MPushButtonProperties

//----------------------------------------------------------------------------
MPushButtonProperties::MPushButtonProperties(IEasyBuilderComponentExtendable^ owner, EasyBuilderComponent^ parentComponent, System::String^ name)
	:
	MButtonProperties(owner, parentComponent, name)
{
}

//----------------------------------------------------------------------------
bool MPushButtonProperties::Default::get()
{
	CWndObjDescription* pDescri = ExtendedWindow->GetWndObjDescription();
	return pDescri && pDescri->IsKindOf(RUNTIME_CLASS(CPushButtonDescription)) ?
		((CPushButtonDescription*)pDescri)->m_bDefault
		: ExtendedWindow->HasStyle(BS_DEFPUSHBUTTON);

}
//----------------------------------------------------------------------------
void MPushButtonProperties::Default::set(bool value)
{
	CWndObjDescription* pDescri = ExtendedWindow->GetWndObjDescription();
	if (pDescri && pDescri->IsKindOf(RUNTIME_CLASS(CPushButtonDescription)))
		((CPushButtonDescription*)pDescri)->m_bDefault = value;
	ExtendedWindow->SetStyle(value ? BS_PUSHBUTTON : BS_DEFPUSHBUTTON, value ? BS_DEFPUSHBUTTON : BS_PUSHBUTTON);
}

//----------------------------------------------------------------------------
bool MPushButtonProperties::OwnerDraw::get()
{
	CWndObjDescription* pDescri = ExtendedWindow->GetWndObjDescription();
	return pDescri && pDescri->IsKindOf(RUNTIME_CLASS(CPushButtonDescription)) ?
		((CPushButtonDescription*)pDescri)->m_bOwnerDraw
		: ExtendedWindow->HasStyle(BS_OWNERDRAW);

}
//----------------------------------------------------------------------------
void MPushButtonProperties::OwnerDraw::set(bool value)
{
	CWndObjDescription* pDescri = ExtendedWindow->GetWndObjDescription();
	if (pDescri && pDescri->IsKindOf(RUNTIME_CLASS(CPushButtonDescription)))
		((CPushButtonDescription*)pDescri)->m_bOwnerDraw = value;
	ExtendedWindow->SetStyle(SET_STYLE_PARAMS(BS_OWNERDRAW));
}

#pragma endregion

