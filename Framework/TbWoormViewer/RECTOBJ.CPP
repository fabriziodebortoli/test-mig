#include "stdafx.h"

#include <math.h>
#include <afxpriv.h>

#include <TBXmlCore\XMLTags.h>

#include <TbNameSolver\PathFinder.h>

#include <TbClientCore\ClientObjects.h>
#include <TbWebServicesWrappers\LoginManagerInterface.h>

#include <TbGeneric\minmax.h>
#include <TbGeneric\GeneralFunctions.h>
#include <TbGeneric\FontsTable.h>
#include <TbGeneric\DataTypesFormatters.h>
#include <TbGeneric\TbStrings.h>

#include <TbGenlib\generic.h>
#include <TbGenlib\baseapp.h>
#include <TbGenlib\const.h>
#include <TbGenlib\Expr.h>
#include <TbGenlib\TbExplorerInterface.h>
#include <TbGenlib\RichEditControl50W.h>

#include <TbGenlibUI\FormatDialog.h>
#include <TbGenlibUI\FontsDialog.h>

#include <TbWoormEngine\edtmng.h>
#include <TbWoormEngine\RpSymTbl.h>
#include <TbWoormEngine\MultiLayout.h>

#include "export.h"           
#include "mclrdlg.h"
#include "viewpars.h"
#include "singleob.h"
#include "woormdoc.h"
#include "woormini.h"
#include "woormvw.h"
#include "woormfrm.h"
#include "RSEditorUI.h"
#include "docproperties.h"
//#include "EditorLinksDlg.h"
#include "rectobj.h"
#include "column.h"
#include "repeater.h"
#include "ExpExter.h"

#include "RSEditorUI.h"
#include "RSEditView.h"

// resources
#include "tbwoormviewer.hrc"

#include "listdlg.hjson" //JSON AUTOMATIC UPDATE
#include "rectobj.hjson" //JSON AUTOMATIC UPDATE
#include "woormdoc.hjson" //JSON AUTOMATIC UPDATE

#include "commands.hrc"

//includere come ultimo include all'inizio del cpp
#include "beginh.dex"

#ifdef _DEBUG
#undef THIS_FILE
static const char  THIS_FILE[] = __FILE__;
#endif
///////////////////////////////////////////////////////////////////////////////

static const TCHAR	szReport[] = _T("Report");
///////////////////////////////////////////////////////////////////////////////
TB_EXPORT CString EFieldShowAsToString(EFieldShowAs f)
{
	switch (f)
	{
	case EFieldShowAs::FT_NORMAL:		return _TB("Normal");
	case EFieldShowAs::FT_IMAGE:		return _TB("Image");
	case EFieldShowAs::FT_BARCODE:		return _TB("Barcode");
	case EFieldShowAs::FT_TEXTFILE:		return _TB("TextFile");
	case EFieldShowAs::FT_URL:			return _TB("Url");
	default:							return _TB("Unknown");
	}
}
///////////////////////////////////////////////////////////////////////////////


BEGIN_TB_STRING_MAP(MenuStrings)

	TB_LOCALIZED(Colors,			"&Colors...")
	TB_LOCALIZED(Borders,			"&Border...")
	TB_LOCALIZED(LineWidth,			"&Line width...")
	TB_LOCALIZED(Size,				"&Size...")
	TB_LOCALIZED(Ratios,			"&Eccentricity...")
	TB_LOCALIZED(Trasparent,		"Trans&parent Background")
	TB_LOCALIZED(DropShadow,		"Drop S&hadow...")
	TB_LOCALIZED(Hide,				"&Hide Field...")
	TB_LOCALIZED(Tooltip,			"Set t&ooltip...")
	TB_LOCALIZED(CopyAttributes,	"Copy attrib&utes from...")

	TB_LOCALIZED(StyleName,			"Style name")
		TB_LOCALIZED(SetStyle,			"Class style...")
		TB_LOCALIZED(SetStyleName,		"Set new Class Style Name...")
		TB_LOCALIZED(ClearCustomStyle,	"Clear custom styles")
		TB_LOCALIZED(Template,			"It is a Class Style &Template")
		TB_LOCALIZED(SetStyleNameForCopyObject,		"Importable template static field")
		TB_LOCALIZED(SetStyleNameForInheritObject,	"Inherit template static field")
		TB_LOCALIZED(StyleNameForCopiedObject,		"Static field imported when template was applied")
		

	TB_LOCALIZED(AnchorOptions,		"Anchor Options...")
		TB_LOCALIZED(AnchorColumn,		"Anchor to Column...")
		TB_LOCALIZED(AnchorPageLeft,	"Anchor page left")
		TB_LOCALIZED(AnchorPageRight,	"Anchor page right")

	TB_LOCALIZED(Image,				"Image...")
	TB_LOCALIZED(LoadImage,			"&Load image...")
	TB_LOCALIZED(SaveImage,			"SaveImage...")
	TB_LOCALIZED(RefreshImage,		"&Refresh image")
	TB_LOCALIZED(OriginalSize,		"&Original size")	
	TB_LOCALIZED(CurrentSize,		"&Current size")
	TB_LOCALIZED(CutImage,			"&Cut image...")
	TB_LOCALIZED(UndoCutImage,		"&Undo Cut")
	TB_LOCALIZED(KeepRatio,				"Keep image &aspect ratio")
	TB_LOCALIZED(ResizeKeepingRatio,	"Resize image &keeping aspect ratio")

	TB_LOCALIZED(FontStyle,				"Font St&yle...")
	TB_LOCALIZED(Text,					"&Text...")
	TB_LOCALIZED(Align,					"&Alignment...")

	TB_LOCALIZED(File,					"File...")
		TB_LOCALIZED(TextLoad,			"Load text from file...")
	
	TB_LOCALIZED(EmfLoad,				"&Load Enhanced Metafile")

	TB_LOCALIZED(Url,					"Url...")
	TB_LOCALIZED(SetUrl,				"Set &Url...")

	TB_LOCALIZED(Link,					"Links..")
	TB_LOCALIZED(EmailParameters,		"Email &Parameters...")
	TB_LOCALIZED(FormatStyle,			"Format St&yle...")
	TB_LOCALIZED(DynamicFormatStyle,	"Dynamic Format St&yle...")

	TB_LOCALIZED(CloneToLayout,			"Clone into an other layout...")
	TB_LOCALIZED(MoveToLayout,			"Move to an other layout...")

END_TB_STRING_MAP()

//==============================================================================
//          Class BaseRect implementation
//==============================================================================

//------------------------------------------------------------------------------
IMPLEMENT_DYNAMIC (BaseRect, SingleItemObj)

//------------------------------------------------------------------------------
BEGIN_MESSAGE_MAP(BaseRect, SingleItemObj)

	ON_COMMAND(ID_BORDERS,						SetBorders)
	ON_COMMAND(ID_RATIOS,						SetRatios)
	ON_COMMAND(ID_TRANSPARENT,					SetTransparent)

	ON_COMMAND (ID_SET_HIDDEN,					OnSetHidden)
	ON_COMMAND (ID_SET_DROPSHADOW,				OnSetDropShadow)
	ON_COMMAND (ID_COPY_ATTRIBUTES,				OnCopyAttributes)

	ON_COMMAND (ID_SELECT_STYLE,							OnSetStyle)
	ON_COMMAND (ID_CLEAR_CUSTOM_STYLE,					OnClearCustomStyle)
	ON_COMMAND (ID_SET_NEW_STYLE_NAME,						OnSetStyleName)
	ON_COMMAND (ID_SET_STYLE_NAME_FOR_COPY_OBJECT,		OnSetStyleNameForCopyObject)
	ON_COMMAND (ID_SET_STYLE_NAME_FOR_INHERIT_OBJECT,	OnSetStyleNameForInheritObject)
	ON_COMMAND (ID_TOGGLE_TEMPLATE,						ToggleTemplate)

	ON_COMMAND (ID_TOGGLE_ANCHOR_LEFT,			ToggleAnchorLeft)
	ON_COMMAND (ID_TOGGLE_ANCHOR_RIGHT,			ToggleAnchorRight)
	ON_COMMAND (ID_ANCHOR_COLUMN,				SetAnchorColumn)

	ON_COMMAND_RANGE (ID_BRING_TO_FRONT_0, (UINT)(ID_BRING_TO_FRONT_4), OnBringToFront)
	ON_COMMAND (ID_SEND_TO_BACK,										OnSendToBack)

	ON_COMMAND (ID_LAYOUT_CLONE_OBJECT,			OnLayoutCopyObject)
	ON_COMMAND (ID_LAYOUT_MOVE_OBJECT,			OnLayoutMoveObject)

END_MESSAGE_MAP()

//------------------------------------------------------------------------------
BaseRect::BaseRect (CPoint ptCurrPos, CWoormDocMng* pDocument)
	:
	SingleItemObj(ptCurrPos, pDocument),
	m_nHRatio	(0),
	m_nVRatio	(0),
	m_bHidden	(FALSE),
	m_pHideExpr	(NULL),
	m_pTextColorExpr(NULL),
	m_pBkgColorExpr	(NULL),
	m_pTooltipExpr (NULL),
	m_pBordersExpr(NULL),
	m_bAnchorPageLeft (FALSE),  //printer attributes
	m_bAnchorPageRight (FALSE),
	m_AnchorLeftColumnID(0),
	m_AnchorRightColumnID(0),
	m_nDropShadowHeight (0),
	m_crDropShadowColor (RGB(0,0,0)),
	m_bTemplate	(FALSE),
	m_bTemplateOverridden (FALSE),
	m_pDefault	(NULL)
{
}

//------------------------------------------------------------------------------
BaseRect::BaseRect (const BaseRect& source)
	:
	SingleItemObj		(source),

	m_Borders			(source.m_Borders),

	m_nHRatio			(source.m_nHRatio),
	m_nVRatio			(source.m_nVRatio),

	m_bHidden			(source.m_bHidden),
	
	m_pHideExpr			(source.m_pHideExpr			? new Expression(*(source.m_pHideExpr))			: NULL),
	m_pTextColorExpr	(source.m_pTextColorExpr	? new Expression(*(source.m_pTextColorExpr))	: NULL),
	m_pBkgColorExpr		(source.m_pBkgColorExpr		? new Expression(*(source.m_pBkgColorExpr))		: NULL),
	m_pTooltipExpr		(source.m_pTooltipExpr		? new Expression(*(source.m_pTooltipExpr))		: NULL),
	m_pBordersExpr		(source.m_pBordersExpr		? new Expression(*(source.m_pBordersExpr))		: NULL),

	m_bAnchorPageLeft	(source.m_bAnchorPageLeft),  //printer attributes
	m_bAnchorPageRight	(source.m_bAnchorPageRight),

	m_AnchorLeftColumnID	(source.m_AnchorLeftColumnID),
	m_AnchorRightColumnID	(source.m_AnchorRightColumnID),

	m_nDropShadowHeight (source.m_nDropShadowHeight),
	m_crDropShadowColor (source.m_crDropShadowColor),
	
	m_sStyleClass			(source.m_sStyleClass),
	m_bTemplate				(source.m_bTemplate),
	m_bTemplateOverridden	(source.m_bTemplateOverridden),
	m_pDefault				(source.m_pDefault)
{
}

//------------------------------------------------------------------------------
BaseRect::~BaseRect()
{
	SAFE_DELETE(m_pHideExpr)
	SAFE_DELETE(m_pTextColorExpr)
	SAFE_DELETE(m_pBkgColorExpr)
	SAFE_DELETE(m_pTooltipExpr)
	SAFE_DELETE(m_pBordersExpr)
}

//------------------------------------------------------------------------------
#define GRIDX		m_pDocument->m_pWoormIni->m_nGridX
#define GRIDY		m_pDocument->m_pWoormIni->m_nGridY

//------------------------------------------------------------------------------
BOOL BaseRect::CanDeleteField (LPCTSTR pszFieldName, CString& sLog) const
{
	 /*TODO sLog = cwsprintf(_TB("Field with Alias {0-%d}"), GetInternalID()) +  _T(" - ");*/

	if (m_pHideExpr && m_pHideExpr->HasMember(pszFieldName))
		return (sLog += _TB("Field is referenced into hidden dynamic condition")), FALSE;
	if (m_pTextColorExpr && m_pTextColorExpr->HasMember(pszFieldName))
		return (sLog += _TB("Field is referenced into text color dynamic condition")), FALSE;
	if (m_pBkgColorExpr && m_pBkgColorExpr->HasMember(pszFieldName))
		return (sLog += _TB("Field is referenced into background color dynamic condition")), FALSE;
	if (m_pTooltipExpr && m_pTooltipExpr->HasMember(pszFieldName))
		return (sLog += _TB("Field is referenced into tooltip dynamic condition")), FALSE;
	if (m_pBordersExpr && m_pBordersExpr->HasMember(pszFieldName))
		return (sLog += _TB("Field is referenced into borders dynamic condition")), FALSE;

	return TRUE;
}

//------------------------------------------------------------------------------
void BaseRect::Redraw()
{
	UpdateDocument();
}

//------------------------------------------------------------------------------
BOOL BaseRect::SetHideExpression (LPCTSTR szExpr)
{
	Parser lex(szExpr);
	Expression* pExpr = new Expression (&m_pDocument->m_ViewSymbolTable);
	if (pExpr->Parse(lex, DataType::Bool, TRUE))
	{
		m_pHideExpr = pExpr;
		return TRUE;
	}
	delete pExpr;
	return FALSE;
}

//-----------------------------------------------------------------------------
void BaseRect::UpdateDocument(BOOL bUpdateProperty)
{
	if (m_AnchorRepeaterID)
	{
		ASSERT_VALID(m_pDocument);
		Repeater* pRep = (Repeater*) (m_pDocument->GetObjects()).FindByID(m_AnchorRepeaterID);
		ASSERT_VALID(pRep);
		ASSERT_KINDOF(Repeater, pRep);
		if (pRep && pRep->IsKindOf(RUNTIME_CLASS(Repeater)))
		{
			pRep->Rebuild(&(m_pDocument->GetObjects()));
		}
	}

	CRect rect (GetRectToInvalidate());
	rect.right += m_nDropShadowHeight;
	rect.bottom += m_nDropShadowHeight;

	m_pDocument->InvalidateRect(rect);

	if (m_pDocument->m_pCurrentObj == this)
	{
		m_pDocument->m_pActiveRect->Clear();
		m_pDocument->m_pActiveRect->SetActive (GetActiveRect());
	}

	m_pDocument->UpdateWindow();

	m_pDocument->SetModifiedFlag();

	if (bUpdateProperty)
	{
		m_pDocument->GetWoormFrame()->SelectLayoutObject(this);	//TODO LAYOUT : aggiungere layout corrente come parametro
	}
}

//------------------------------------------------------------------------------
void BaseRect::SnapToGrid()
{
	int x = m_BaseRect.left % GRIDX; 
	int y = m_BaseRect.top % GRIDY;

	if (x > (GRIDX / 2)) x = x - GRIDX;
	if (y > (GRIDY / 2)) y = y - GRIDY;

	int cx = m_BaseRect.Width() % GRIDX; 
	int cy = m_BaseRect.Height() % GRIDY;

	if (cx > (GRIDX / 2)) cx = cx - GRIDX;
	if (cy > (GRIDY / 2)) cy = cy - GRIDY;

	m_BaseRect.OffsetRect(-x, -y);
	m_BaseRect.right -= cx;
	m_BaseRect.bottom -= cy;

	UpdateDocument();
}

//------------------------------------------------------------------------------
void BaseRect::ToggleBorder(UINT nID)
{
	// conta il numero di bordi presenti e ibverte rispetto ai piu' numerosi
	if (nID == ID_TOGGLE_BORDER_ALL)
	{
		int nUp = m_Borders.top + m_Borders.bottom +
			m_Borders.left + m_Borders.right;

		m_Borders.top = m_Borders.bottom =
			m_Borders.left = m_Borders.right = (nUp < 3);
	}
	else if (nID == ID_TOGGLE_BORDER_UP)
		m_Borders.top = !m_Borders.top;
	else if (nID == ID_TOGGLE_BORDER_DOWN)
		m_Borders.bottom = !m_Borders.bottom;
	else if (nID == ID_TOGGLE_BORDER_LEFT)
		m_Borders.left = !m_Borders.left;
	else if (nID == ID_TOGGLE_BORDER_RIGHT)
		m_Borders.right = !m_Borders.right;

	UpdateDocument();
}

//------------------------------------------------------------------------------
void BaseRect::ToggleBorder(UINT nID, BOOL bOn)
{
	// conta il numero di bordi presenti e ibverte rispetto ai piu' numerosi
	if (nID == ID_TOGGLE_BORDER_ALL)
	{
		m_Borders.top = m_Borders.bottom =
			m_Borders.left = m_Borders.right = bOn;
	}
	else if (nID == ID_TOGGLE_BORDER_UP)
		m_Borders.top = bOn;
	else if (nID == ID_TOGGLE_BORDER_DOWN)
		m_Borders.bottom = bOn;
	else if (nID == ID_TOGGLE_BORDER_LEFT)
		m_Borders.left = bOn;
	else if (nID == ID_TOGGLE_BORDER_RIGHT)
		m_Borders.right = bOn;

	UpdateDocument();
}

//------------------------------------------------------------------------------
BOOL BaseRect::IsBorderOn(UINT nID)
{
	if (nID == ID_TOGGLE_BORDER_ALL)
		return
		m_Borders.top + m_Borders.bottom +
		m_Borders.left + m_Borders.right > 0;
	else if (nID == ID_TOGGLE_BORDER_UP)
		return m_Borders.top;
	else if (nID == ID_TOGGLE_BORDER_DOWN)
		return m_Borders.bottom;
	else if (nID == ID_TOGGLE_BORDER_LEFT)
		return m_Borders.left;
	else if (nID == ID_TOGGLE_BORDER_RIGHT)
		return m_Borders.right;
	return FALSE;
}

//------------------------------------------------------------------------------
void BaseRect::ToggleAnchorLeft ()
{
	m_bAnchorPageLeft = !m_bAnchorPageLeft;
	FreeFieldFromColumn(&m_pDocument->GetObjects());

	m_bAnchorPageRight = FALSE;
	m_AnchorLeftColumnID = 0;
	m_AnchorRightColumnID = 0;

	UpdateDocument();
}	

//------------------------------------------------------------------------------
void BaseRect::ToggleAnchorRight ()
{
	m_bAnchorPageRight = !m_bAnchorPageRight;
	FreeFieldFromColumn(&m_pDocument->GetObjects());

	m_bAnchorPageLeft = FALSE;
	m_AnchorLeftColumnID = 0;
	m_AnchorRightColumnID = 0;

	UpdateDocument();
}	

//------------------------------------------------------------------------------
BOOL BaseRect::HasDynamicExpr() const
{
	return m_pHideExpr || m_pBkgColorExpr || m_pTextColorExpr || m_pTooltipExpr || m_pBordersExpr;
}

//------------------------------------------------------------------------------
BOOL BaseRect::PreDraw (CDC* pDC)
{
	if (m_AnchorRepeaterID)
	{
		int nRow = -1;
		ASSERT(this->m_nRepeaterRow >= 0);

		ASSERT_VALID(m_pDocument);
		Repeater* pRep = dynamic_cast<Repeater*> (m_pDocument->GetObjects().FindByID(m_AnchorRepeaterID));
		ASSERT_VALID(pRep);

#ifdef _DEBUG
		if (pRep)
		{
			nRow = pRep->GetFieldRow(this);
			ASSERT(nRow > -1);
		}
		ASSERT(nRow == this->m_nRepeaterRow);
#endif

		if (HasDynamicExpr())
			m_pDocument->UpdateViewSymbolTable(pRep, this->m_nRepeaterRow);

		if (pRep->GetDrawMode(pDC) == BaseRect::HIDDEN)   //repeater group hide all inner objects
			return FALSE;
	}
	else if (HasDynamicExpr())
			m_pDocument->UpdateViewSymbolTable();

	if (GetDrawMode(pDC) == BaseRect::HIDDEN || GetBaseRect().Width() == 0)   //check on width for anchored hidden field
		return FALSE;

	if (pDC)
		DrawDropShadow(*pDC);

	return TRUE;
}

//------------------------------------------------------------------------------
void BaseRect::PostDraw (CDC& DC, BOOL bPreview, const CRect& inside)
{
	if (m_pDocument->m_bAllowEditing && !DC.IsPrinting() && GetBaseRect().Width() > 0)
	{
		DrawIcons (DC, inside);

		if (!bPreview) 
			DrawEditingBorder(DC);
	}
}

//------------------------------------------------------------------------------
void BaseRect::DrawIcons(CDC& DC, CRect rect)
{
	//if h w zero return
	CRect rectInside(rect);

	/*Draw field's icons in the bottom-right corner when field in in Edit mode*/
	
	// DPI scale zone
	ScaleRect(rectInside, DC);

	if (IsKindOf(RUNTIME_CLASS(Repeater)))
	{
		CTBPicture* pIcon =  m_pDocument->GetRepeaterIcon(DC);

		rectInside.top = rectInside.bottom - pIcon->GetHeight();
		rectInside.left = rectInside.right - pIcon->GetWidth();
	
		GenericDrawObj::DrawBitmap(DC, rectInside, pIcon);
		rectInside.right = rectInside.left;
	}

	if (m_nRepeaterRow > 0 || m_bInheritByTemplate)
	{
		CTBPicture* pIcon =  m_pDocument->GetLockedIcon(DC);

		rectInside.top = rectInside.bottom - pIcon->GetHeight();
		rectInside.left = rectInside.right - pIcon->GetWidth();
	
		GenericDrawObj::DrawBitmap(DC, rectInside, pIcon);
		rectInside.right = rectInside.left;
	}

	if (m_AnchorRepeaterID != 0)
	{
		CTBPicture* pIcon =  m_pDocument->GetRepeaterFieldIcon(DC);

		rectInside.top = rectInside.bottom - pIcon->GetHeight();
		rectInside.left = rectInside.right - pIcon->GetWidth();
	
		GenericDrawObj::DrawBitmap(DC, rectInside, pIcon);
		rectInside.right = rectInside.left;
	}

	DrawMode dm = GetDrawMode(&DC);
	if (dm == BaseRect::HIDDENEDITEXPR || dm == BaseRect::HIDDENEDIT)
	{
		CTBPicture* pIcon =  (dm == BaseRect::HIDDENEDITEXPR ? m_pDocument->GetHiddenWhenIcon(DC) : m_pDocument->GetHiddenIcon(DC));

		rectInside.top = rectInside.bottom - pIcon->GetHeight();
		rectInside.left = rectInside.right - pIcon->GetWidth();
	
		GenericDrawObj::DrawBitmap(DC, rectInside, pIcon);
		rectInside.right = rectInside.left;
	}

	if (m_AnchorLeftColumnID != 0)
	{
		CTBPicture* pIcon =  m_pDocument->GetAnchorIcon(DC);

		rectInside.top = rectInside.bottom - pIcon->GetHeight();
		rectInside.left = rectInside.right - pIcon->GetWidth();
	
		GenericDrawObj::DrawBitmap(DC, rectInside, pIcon);
		rectInside.right = rectInside.left;
	}

	if (m_bAnchorPageLeft != 0)
	{
		CTBPicture* pIcon =  m_pDocument->GetAnchorLeftIcon(DC);

		rectInside.top = rectInside.bottom - pIcon->GetHeight();
		rectInside.left = rectInside.right - pIcon->GetWidth();
	
		GenericDrawObj::DrawBitmap(DC, rectInside, pIcon);
		rectInside.right = rectInside.left;
	}

	if (IsKindOf(RUNTIME_CLASS(FieldRect)))
	{
		if (((FieldRect*)this)->IsEmailParameter ())
		{
			CTBPicture* pIcon =  m_pDocument->GetMailIcon(DC);

			rectInside.top = rectInside.bottom - pIcon->GetHeight();
			rectInside.left = rectInside.right - pIcon->GetWidth();
	
			GenericDrawObj::DrawBitmap(DC, rectInside, pIcon);
			rectInside.right = rectInside.left;
		}
	}

	// DPI unscale area
	UnScaleRect(rectInside, DC);
}

//---------------------------------------------------------------------------
void BaseRect::DrawEditingBorder(CDC& DC) 
{
	if (m_pDocument->m_bAllowEditing && (m_Borders == Borders(0,0,0,0) || m_BorderPen.m_nWidth == 0) && !DC.IsPrinting()/* && IsTransparent	()*/)
	{
		CRect rect (m_BaseRect);
		ScaleRect(rect, DC);

		CPoint pl[5] = 
			{ 
				CPoint(rect.left,		rect.top),
				CPoint(rect.right,		rect.top),
				CPoint(rect.right,		rect.bottom),
				CPoint(rect.left,		rect.bottom),
				CPoint(rect.left,		rect.top)
			};
		CPen pen(m_pDocument->m_pWoormIni->m_eHiddenBorderStyle, 1, m_pDocument->m_pWoormIni->m_rgb_HiddenBorderColor);
		CPen* oldPen = DC.SelectObject(&pen);
		int m = DC.SetBkMode(TRANSPARENT);

		DC.Polyline(pl, 5);

		DC.SetBkMode(m);
		DC.SelectObject(oldPen);
	}
}

//------------------------------------------------------------------------------
BOOL BaseRect::ParseBlock(ViewParser& lex)
{
	if (lex.LookAhead(T_BEGIN))
		return
			lex.ParseBegin  ()      &&
			ParseProps      (lex)   &&
			lex.ParseEnd    ();

	return ParseProp(lex, FALSE);
}

//------------------------------------------------------------------------------
BOOL BaseRect::ParseProps (ViewParser& lex)
{
	BOOL ok = TRUE;

	do { ok = ParseProp(lex) && !lex.Bad() && !lex.Eof(); }
	while (ok && !lex.LookAhead(T_END));

	return ok;
}

//------------------------------------------------------------------------------
BOOL BaseRect::ParseProp (ViewParser& lex, BOOL bBlock)
{
	BOOL ok = TRUE;

	switch (lex.LookAhead())
	{
		case T_PEN			: ok = lex.ParsePen     (m_BorderPen);		break;
		case T_BORDERS		: 
				{
					ok = lex.ParseBorders (m_Borders);	

					if (lex.Matched(T_COMMA))
					{
						WoormTable* pST = &(m_pDocument->m_ViewSymbolTable);
						m_pBordersExpr = new Expression(pST);
						m_pBordersExpr->SetStopTokens(T_SEP);
						if (!m_pBordersExpr->Parse(lex, DataType::String, TRUE))
						{
							lex.SetError(_TB("Parser error in field conditional borders expression"));
							return FALSE;
						}

						lex.ParseSep();
					}	
					break;
				}
		case T_TRANSPARENT  : ok = lex.SkipToken();	m_bTransparent = TRUE;	break;

		case T_HIDDEN :
				{
					//ormai c'e' il begin-end ed il ; finale dopo l'espressione, rimane per retrocompatibilità.
					Token arStopTokens [] = 
					{ 
						T_SEP, T_END, 
						T_LABEL,T_BARCODE_STRIP, T_FILE, T_MAIL, T_BITMAP, 
						T_FIELD, T_TEXT, T_TABLE, T_SQRRECT, T_RNDRECT, T_REPEATER, T_CHART,
						T_TEXTCOLOR, T_ALIGN, T_FONTSTYLE, T_FORMATSTYLE, T_BARCODE_STRIP, 
						T_BKGCOLOR, T_PEN, T_BORDERS, T_TRANSPARENT,
						T_SPECIAL_FIELD, T_TOOLTIP, T_STYLE, T_TEMPLATE, T_DROPSHADOW,
						T_ANCHOR_COLUMN_ID, T_ANCHOR_PAGE_LEFT, T_ANCHOR_PAGE_RIGHT, 
						T_HTML, T_LAYER
					};

					ok = ParseHidden(lex, arStopTokens, sizeof(arStopTokens)/sizeof(Token));	
					break;
				}

		case T_ANCHOR_PAGE_LEFT :
		{
			lex.SkipToken();
			m_bAnchorPageLeft = TRUE;
			break;
		}

		case T_ANCHOR_PAGE_RIGHT :
		{
			lex.SkipToken();
			m_bAnchorPageRight = TRUE;
			break;
		}
		
		case T_ANCHOR_COLUMN_ID :
		{
			lex.SkipToken();
			ok = lex.ParseAlias(m_AnchorLeftColumnID);
			if (ok && lex.Matched(T_COMMA))
				ok = lex.ParseAlias(m_AnchorRightColumnID);
			break;
		}

		case T_DROPSHADOW   : ok = ParseDropShadow		(lex);						break;
		case T_TOOLTIP		: ok = ParseTooltip			(lex);						break;

		case T_TEMPLATE		: ok = lex.SkipToken();		m_bTemplate = TRUE;			break;
		case T_STYLE :
		{
			lex.SkipToken();
			ok = lex.ParseString(m_sStyleClass);
			m_sStyleClass.Trim();
			break;
		}

		case T_LAYER:
		{
			lex.SkipToken();
			ok = lex.ParseInt(m_nLayer);
			break;
		}

		case T_END : 
			break;

		default :
			ok = OnParseProp (lex, bBlock);
			break;
	}

	return ok;
}

//------------------------------------------------------------------------------
BOOL BaseRect::ParseHidden(ViewParser& lex, Token* arStopTokens, int nStopToken)
{
	if (!lex.Match(T_HIDDEN)) 
		return FALSE;	
	m_bHidden = TRUE;	
	if (lex.Matched(T_WHEN))
	{
		WoormTable* pST = &(m_pDocument->m_ViewSymbolTable);

		m_pHideExpr = new Expression(pST);
		m_pHideExpr->SetStopTokens(arStopTokens, nStopToken);
		if (!m_pHideExpr->Parse(lex, DataType::Bool, TRUE))
			return FALSE;

		DataBool b(FALSE);
		if(!m_pHideExpr->Eval(b))
		{
			//TODO
			//lex.SetError(_TB("Evalutation error in show/hide field expression:") + _T("\r\n") + m_Label.GetText());
			//return FALSE;
		}
		m_bHidden = b;

		lex.Matched(T_SEP);
	}
	return TRUE;
}

//------------------------------------------------------------------------------
void BaseRect::UnparseHidden (ViewUnparser& ofile)
{
	if ((m_bHidden || m_pHideExpr)  && ! m_pDocument->m_Template.m_bIsSavingTemplate) 
	{
		ofile.UnparseTag (T_HIDDEN, m_pHideExpr == NULL);

		if (m_pHideExpr && !m_pHideExpr->IsEmpty())
		{
			ofile.UnparseTag (T_WHEN, FALSE);

			if (m_pDocument->m_bReplaceHiddenWhenExpr)
			{
				ofile.UnparseTag (m_bHidden ? T_TRUE : T_FALSE);
			}
			else
			{
				CString sHideWhenExpr = m_pHideExpr->ToString();
				ofile.UnparseExpr(sHideWhenExpr, FALSE);
			}
			ofile.UnparseTag (T_SEP);
		}
	}
}

//------------------------------------------------------------------------------
void BaseRect::UnparsePrintInfo (ViewUnparser& ofile)
{
	if (m_bAnchorPageLeft) 
		ofile.UnparseTag(T_ANCHOR_PAGE_LEFT, FALSE);
	if (m_bAnchorPageRight) 
		ofile.UnparseTag(T_ANCHOR_PAGE_RIGHT, FALSE);

	if (!m_pDocument->m_Template.m_bIsSavingTemplate && m_AnchorLeftColumnID)
	{
		ofile.UnparseTag(T_ANCHOR_COLUMN_ID, FALSE);
		ofile.UnparseAlias(m_AnchorLeftColumnID, FALSE);

		if (m_AnchorRightColumnID && m_AnchorLeftColumnID != m_AnchorRightColumnID)
		{
			ofile.UnparseTag(T_COMMA, FALSE);
			ofile.UnparseAlias(m_AnchorRightColumnID, FALSE);
		}
	}

	if (m_bAnchorPageLeft || m_bAnchorPageRight || m_AnchorLeftColumnID)
		ofile.UnparseCrLf();
}

//------------------------------------------------------------------------------
BOOL BaseRect::InMe (CPoint ptWhere)
{
	if (m_bInheritByTemplate)
		return FALSE;

	if (!m_pDocument->m_bAllowEditing) 
	{
		DataBool b = m_bHidden;
		if (m_pHideExpr && !m_pHideExpr->Eval(b))
		{
			//TODO
		}
		if (b)
			return FALSE;
	}
	return __super::InMe(ptWhere);
}

//------------------------------------------------------------------------------
BaseRect::DrawMode BaseRect::GetDrawMode (CDC* pDC)
{
	DataBool b = m_bHidden;
	if (m_pHideExpr && !m_pHideExpr->IsEmpty())
	{
		if (!m_pHideExpr->Eval(b))
		{
			//SetError(_TB("Evalutation error in show/hide field expression:") + GetDescription());
			if (m_pHideExpr->GetErrId() != Expression::NULL_OPR)
				TRACE(_T("Evalutation error in show/hide field expression: %s\n"), GetDescription());
		}
	}
	m_bHidden = b;

	if (m_bHidden && !m_pDocument->m_bAllowEditing) 
		return BaseRect::HIDDEN;

	if (m_bHidden && m_pDocument->m_bAllowEditing && (!pDC || pDC->IsPrinting())) //per nascondere field durante lo z-print in edit mode 
		return BaseRect::HIDDEN;
	
	if (m_pDocument->m_bAllowEditing && /*m_pDocument->m_nCurrentLayer &&*/ m_nLayer != m_pDocument->m_nCurrentLayer)
		return BaseRect::HIDDEN;

	if (m_pHideExpr && !m_pHideExpr->IsEmpty() && m_pDocument->m_bAllowEditing)
		return BaseRect::HIDDENEDITEXPR;

	if (m_bHidden && m_pDocument->m_bAllowEditing)
		return BaseRect::HIDDENEDIT;
	
	return BaseRect::VISIBLE;
}

//------------------------------------------------------------------------------
BOOL BaseRect::ParseDynamicColor (ViewParser& lex, COLORREF& rgbColor, Token tk, Expression*& pExpr)
{
	BOOL bOk = lex.Match(tk);
	if (lex.Matched(T_ASSIGN))
	{
		WoormTable* pST = &(m_pDocument->m_ViewSymbolTable);

		pExpr = new Expression(pST);
		pExpr->SetStopTokens(T_SEP);
		if (!pExpr->Parse(lex, DataType::Long, TRUE))
		{
			lex.SetError(_TB("Parser error in field conditional color expression"));
			return FALSE;
		}

		return lex.ParseSep();
	}
	else
		return 
			lex.ParseColor   (T_NULL_TOKEN, rgbColor)
			&&
			lex.ParseSep    ();
}

//------------------------------------------------------------------------------
BOOL BaseRect::ParseTextColor (ViewParser& lex, COLORREF& rgbColor)
{
	return ParseDynamicColor(lex, rgbColor, T_TEXTCOLOR, m_pTextColorExpr);
}
//------------------------------------------------------------------------------
BOOL BaseRect::ParseBkgColor (ViewParser& lex, COLORREF& rgbColor)
{
	return ParseDynamicColor(lex, rgbColor, T_BKGCOLOR, m_pBkgColorExpr);
}

//------------------------------------------------------------------------------
void BaseRect::UnparseDynamicColor (ViewUnparser& ofile, Token tk, Expression*& pExpr)
{
	if (pExpr && ! m_pDocument->m_Template.m_bIsSavingTemplate)
	{
		ofile.UnparseTag(tk, FALSE);
		ofile.UnparseTag(T_ASSIGN, FALSE);

		ofile.UnparseExpr(pExpr->ToString(), FALSE);
		ofile.UnparseSep(TRUE);
	}
}

//------------------------------------------------------------------------------
void BaseRect::ShowPopup (CPoint ptPoint, CBCGPScrollView* pView)
{
	CMenu   menu;
	menu.CreatePopupMenu();

	m_pDocument->BringToFrontPopup (this, menu);

	if (OnShowPopup(menu))
	{
		if (m_pDocument->m_Template.m_bIsTemplate || m_pDocument->m_Template.m_bLoaded)
		{
			CMenu& templateMenu = menu;
			BOOL bAllowStatic = m_bTemplate && (IsKindOf(RUNTIME_CLASS(TextRect)) || IsKindOf(RUNTIME_CLASS(SqrRect)));

			if (m_pDocument->m_Template.m_bIsTemplate)
			{
				BOOL bIsToBeCopy = bAllowStatic && m_sStyleClass.CompareNoCase(CWoormTemplate::s_sCopyStaticObject_StyleName) == 0;
				BOOL bIsToBeInherit = bAllowStatic && m_sStyleClass.CompareNoCase(CWoormTemplate::s_sInheritStaticObject_StyleName) == 0;

				if (bAllowStatic)
				{
					templateMenu.AppendMenu(MF_STRING | (bIsToBeCopy ? MF_CHECKED : 0), ID_SET_STYLE_NAME_FOR_COPY_OBJECT, MenuStrings::SetStyleNameForCopyObject());
					templateMenu.AppendMenu(MF_STRING | (bIsToBeInherit ? MF_CHECKED : 0), ID_SET_STYLE_NAME_FOR_INHERIT_OBJECT, MenuStrings::SetStyleNameForInheritObject());
				}

				if (!bIsToBeCopy && !bIsToBeInherit)
				{
					CString s = MenuStrings::SetStyleName();
					if (!m_sStyleClass.IsEmpty())
					{
						s += _T(" (") + m_sStyleClass + ')';
					}
					templateMenu.AppendMenu(MF_STRING, ID_SET_NEW_STYLE_NAME, s);

					templateMenu.AppendMenu(MF_STRING | (m_bTemplate ? MF_CHECKED : 0), ID_TOGGLE_TEMPLATE, MenuStrings::Template());
				}
			}
		}
		//----

	//	printerInfoMenu.AppendMenu(MF_STRING, ID_TOGGLE_ANCHOR_RIGHT,	MenuStrings::AnchorPageRight());
	//	printerInfoMenu.CheckMenuItem (ID_TOGGLE_ANCHOR_RIGHT, m_bAnchorPageRight ? MF_CHECKED : MF_UNCHECKED);

		if (m_pDocument->m_Layouts.GetCount() > 1)
		{
			menu.AppendMenu(MF_SEPARATOR);
			menu.AppendMenu(MF_STRING, ID_LAYOUT_CLONE_OBJECT,		MenuStrings::CloneToLayout());
			menu.AppendMenu(MF_STRING, ID_LAYOUT_MOVE_OBJECT,		MenuStrings::MoveToLayout());
		}
	}

	pView->ClientToScreen (&ptPoint);
	menu.TrackPopupMenu (TPM_LEFTBUTTON, ptPoint.x, ptPoint.y, this);
}

//------------------------------------------------------------------------------
void BaseRect::SetBorders()
{
	CBordersDlg dialog(m_Borders);
	if (dialog.DoModal() == IDOK)
	{
		UpdateDocument();
	}
}

//------------------------------------------------------------------------------
void BaseRect::DrawBorders(CDC& DC, Borders* pBorders)
{
	BOOL bNoBorders = NoBorders(DC.IsPrinting());
	if (bNoBorders) 
		return;

	if (this->m_pBordersExpr)
	{
		//pBorders = new Borders(m_Borders);
		DataStr ds;
		if (m_pBordersExpr->Eval(ds))
			pBorders->Update(ds.GetString());
	}

	__super::DrawBorders
	(
		DC,
		m_BaseRect, 
		m_BorderPen,
		pBorders,
		bNoBorders
	);
}

//------------------------------------------------------------------------------
void BaseRect::DrawRound (CDC& DC, COLORREF rgbBkgn, BOOL bPreview, BOOL bOpaque)
{
	CPen    pen(PS_INSIDEFRAME, m_BorderPen.GetWidth(), m_BorderPen.GetColor());
	CPen*   penOld = DC.SelectObject(&pen);
	CBrush* brushOld;
	CBrush  brush;

	if (bOpaque)
	{
		brush.CreateSolidBrush(rgbBkgn);
		brushOld = DC.SelectObject(&brush);
	}
	else
		brushOld = (CBrush*) DC.SelectStockObject(NULL_BRUSH);

	if (NoBorders(DC.IsPrinting()))
		DC.SelectStockObject(NULL_PEN);
	
	// make scale translation
	CRect rect = m_BaseRect;
	CPoint ratio = CPoint(m_nHRatio, m_nVRatio);

	ScaleRect(rect, DC);
	ScalePoint(ratio, DC);
	
	DC.RoundRect(rect, ratio);

	DC.SelectObject(penOld);
	DC.SelectObject(brushOld);
}

//------------------------------------------------------------------------------
void BaseRect::SetTransparent()
{
	m_bTransparent = !m_bTransparent;

	UpdateDocument();
}

//------------------------------------------------------------------------------
void BaseRect::SetRatios ()
{
	// construct font dialogs
	CRatiosDlg dialog (m_nHRatio, m_nVRatio, m_pDocument);

	if (dialog.DoModal() == IDOK)
	{                                          
		UpdateDocument();
	}
}

//------------------------------------------------------------------------------
void BaseRect::SetAnchorColumn()
{
	CAnchorFieldToColumnsDlg dialog (m_pDocument, this);

	if (dialog.DoModal() == IDOK)
	{                                          	
		UpdateDocument ();
	}
}

//------------------------------------------------------------------------------
BOOL BaseRect::SetAnchorColumn(CLayout* pObjects, TableColumn*& pLeftCol, TableColumn*& pRightCol)
{
	pLeftCol = pRightCol = NULL;
	for (int i = 0; i < pObjects->GetSize(); i++)
	{
		BaseObj* pObj = (*pObjects)[i];

		if (pObj->IsKindOf(RUNTIME_CLASS(Table)))
		{
			Table* pTable = (Table*) pObj;
			for (int nCol = 0; nCol < pTable->GetColumns().GetSize(); nCol++)
			{
				const TableColumn* pCol = pTable->GetColumn(nCol);
				if (!pLeftCol && pCol->GetInternalID() == m_AnchorLeftColumnID)
				{
					pLeftCol = const_cast<TableColumn*>(pCol);
					pLeftCol->AddAnchoredField(this);

					if (!m_AnchorRightColumnID) 
						return TRUE;
				}
				else if (pLeftCol && pCol->GetInternalID() == m_AnchorRightColumnID)
				{
					pRightCol = const_cast<TableColumn*>(pCol);
					pRightCol->AddAnchoredField(this);

					return TRUE;
				}
				else if (pLeftCol)
				{
					(const_cast<TableColumn*>(pCol))->AddAnchoredField(this);
				}
			}
		}
	}
	return FALSE;
}

//------------------------------------------------------------------------------
void BaseRect::AnchorFieldToColumn(CLayout* pObjects, WORD wLeftColID, WORD wRightColID)
{
	if (wLeftColID == 0)
		return;

	if (wLeftColID == wRightColID)
		wRightColID = 0;

	m_AnchorLeftColumnID = wLeftColID;
	m_AnchorRightColumnID = wRightColID;

	TableColumn* pLeftCol = NULL;
	TableColumn* pRightCol = NULL;
	
	if (!SetAnchorColumn(pObjects, pLeftCol, pRightCol))
	{
		m_AnchorLeftColumnID = m_AnchorRightColumnID = 0;
		return;
	}

	CRect colLeftRect(0,0,0,0);				
	if (pLeftCol->HasTotal())
		colLeftRect = pLeftCol->GetColumnRect();
	else
		colLeftRect = pLeftCol->GetColumnCellsRect();

	BOOL isFirstVisibleLeftCol = (pLeftCol->GetTable()->GetColumnIdxByPoint(CPoint(colLeftRect.TopLeft().x - 1, pLeftCol->GetTable()->m_BaseRect.CenterPoint().y)) < 0);
	
	if (pRightCol)
	{
		CRect colRightRect(0,0,0,0);
		if (pRightCol->HasTotal())
			colRightRect = pRightCol->GetColumnRect();
		else
			colRightRect = pRightCol->GetColumnCellsRect();

		MoveBaseRect(colLeftRect.left, m_BaseRect.top, colRightRect.right, m_BaseRect.top + m_BaseRect.Height(), TRUE, isFirstVisibleLeftCol);
	}
	else
	{
		MoveBaseRect(colLeftRect.left, m_BaseRect.top, colLeftRect.right, m_BaseRect.top + m_BaseRect.Height(), TRUE, isFirstVisibleLeftCol);
	}

	m_bAnchorPageLeft = FALSE;
	m_bAnchorPageRight = FALSE;
						
	if (m_pDocument->m_pCurrentObj == this && m_pDocument->m_bAllowEditing)
	{
		UpdateDocument();
	}
}

//------------------------------------------------------------------------------
void BaseRect::FreeFieldFromColumn(CLayout* pObjects)
{
	if (m_AnchorLeftColumnID)
	{
		for (int i = 0; i < pObjects->GetSize(); i++)
		{
			BaseObj* pObj = (*pObjects)[i];
			if (pObj->IsKindOf(RUNTIME_CLASS(Table)))
			{
				Table* pTable = (Table*) pObj;
				for (int nCol = 0; nCol < pTable->GetColumns().GetSize(); nCol++)
				{
					TableColumn* pCol = const_cast<TableColumn*>(pTable->GetColumn(nCol));
					
					if (m_AnchorLeftColumnID == pCol->GetInternalID())
					{
						pCol->RemoveAnchoredField(this);
						m_AnchorLeftColumnID = 0;
						if (m_AnchorRightColumnID == 0)
							goto l_end_FreeFieldFromColumn;
					}
					if (m_AnchorRightColumnID == pCol->GetInternalID())
					{
						pCol->RemoveAnchoredField(this);
						m_AnchorRightColumnID = 0;
						goto l_end_FreeFieldFromColumn;
					}
					if (m_AnchorLeftColumnID == 0) //colonne intermedie
					{
						pCol->RemoveAnchoredField(this);		
					}
				}
			}
		}
l_end_FreeFieldFromColumn:
		m_AnchorLeftColumnID = m_AnchorRightColumnID = 0;

		UpdateDocument();
	}
}

//------------------------------------------------------------------------------
void BaseRect::FreeFieldFromRepeater(CLayout* pObjects)
{
	if (m_AnchorRepeaterID)
	{
		BaseObj* pObj = pObjects->FindByID(m_AnchorRepeaterID);
		if (
				pObj &&
				m_AnchorRepeaterID == pObj->GetInternalID() &&
				pObj->IsKindOf(RUNTIME_CLASS(Repeater))
			)
		{
			Repeater* pRep = (Repeater*) pObj;
			pRep->Detach(this, TRUE);
		}

		m_AnchorRepeaterID = 0; m_nRepeaterRow = -1;
	}
}

//------------------------------------------------------------------------------
BOOL BaseRect::DeleteEditorEntry ()
{
	FreeFieldFromColumn(&m_pDocument->GetObjects());
	FreeFieldFromRepeater(&m_pDocument->GetObjects());
	return TRUE;
}

//------------------------------------------------------------------------------
void BaseRect::MoveBaseRect(int x1, int y1, int x2, int y2, BOOL bRepaint/* = TRUE*/, BOOL bIgnoreBorder/* = FALSE*/)
{	
	m_pDocument->InvalidateRect(m_BaseRect);  //old area
	//Se il campo che sto ancorando ha il bordo sinistro adeguo la x in modo che il contenuto 
	//sia allineato alla x della colonna, e non li bordo
	//(Comportamento uniforme a quello del titolo della colonna)
	if (m_Borders.left && !bIgnoreBorder)
		x1 -= m_BorderPen.GetWidth();

	m_BaseRect.SetRect( x1 , y1, x2, y2);	

	if (bRepaint)
		UpdateDocument ();
}

//------------------------------------------------------------------------------
void BaseRect::MoveBaseRect(int xOffset, int yOffset, BOOL bIgnoreBorder/* = FALSE*/)
{	
	MoveBaseRect(m_BaseRect.left + xOffset, m_BaseRect.top + yOffset , m_BaseRect.right + xOffset, m_BaseRect.bottom + yOffset, FALSE, bIgnoreBorder);
}

//------------------------------------------------------------------------------
CString BaseRect::MapFile(const CString &strFileName)
{
	// converto il path del file in modo che punti a quello del file tradotto
	CString strName = GetName(strFileName) + GetExtension(strFileName);
	
	//prima lo cerco col nome di file
	CString strNewFileName	= AfxLoadReportString (strName, m_pDocument);
	if (strName.CompareNoCase(strNewFileName) == 0)
	{
		// se non lo trovo, lo cerco col namespace
		CTBNamespace ns = AfxGetPathFinder()->GetNamespaceFromPath(strFileName);
		strName = ns.ToString();
		strNewFileName	= AfxLoadReportString (strName, m_pDocument);
	}

	if (strName.CompareNoCase(strNewFileName))
	{
		// il path è stato tradotto: lo pesco dalla Dictionary
		strNewFileName = (::GetPath(m_pDocument->GetDictionaryPath(TRUE), TRUE) + szReport + SLASH_CHAR + strName);
		
		//in caso contrario, carico il file originario
		if (ExistFile (strNewFileName))
			return strNewFileName;
	}
	
	return strFileName;	
}

//---------------------------------------------------------------------------
void BaseRect::Resize (ActionType CT, CPoint C, CPoint P)
{
	if (m_AnchorLeftColumnID && (C.x != P.x))
	{
		return;
	}
	
	BOOL bPrevPersistent = m_bPersistent;
	m_bPersistent = TRUE;
	if (bPrevPersistent != m_bPersistent)
		m_pDocument->RefreshRSTree(ERefreshEditor::Layouts, this);

	SingleItemObj::Resize(CT, C, P);
}

//------------------------------------------------------------------------------
void BaseRect::OnSetHidden ()
{	
	CHiddenWhenDlg   dialog (m_pHideExpr, m_bHidden, NULL, &(m_pDocument->m_ViewSymbolTable), this);
 
	if (dialog.DoModal() != IDOK) return;
		
	m_bHidden = dialog.GetState();

	UpdateDocument();
}

//------------------------------------------------------------------------------
void BaseRect::OnBringToFront (UINT nID)
{
	m_pDocument->OnBringToFront (nID);
}

//------------------------------------------------------------------------------
void BaseRect::OnSendToBack ()
{
	m_pDocument->OnSendToBack (this);
}

//---------------------------------------------------------------------------
void BaseRect::DrawDropShadow(CDC& DC)
{
	__super::DrawDropShadow(DC, m_BaseRect, m_nDropShadowHeight, m_crDropShadowColor);
}

//---------------------------------------------------------------------------
void BaseRect::OnCopyAttributes	()
{
	CChooseObjectDlg   dialog(m_pDocument, this);

	if (dialog.DoModal() == IDOK)
	{
		if (m_nDropShadowHeight)
		{
			CRect rect(m_BaseRect);
			rect.right += m_nDropShadowHeight;
			rect.bottom += m_nDropShadowHeight;

			m_pDocument->InvalidateRect(rect);
		}

		CopyAttributes (dialog.m_pSelected);

		UpdateDocument();
	}
}

//-----------------------------------------------------------------------------
void BaseRect::OnLayoutCopyObject ()
{
	CChooseLayoutDlg   dlg(m_pDocument);
	if (dlg.DoModal() == IDOK)
	{
		m_pDocument->LayoutCopyObject(&(m_pDocument->GetObjects()), dlg.m_pSelectedLayout, this);
	}
}

//-----------------------------------------------------------------------------
void BaseRect::OnLayoutMoveObject ()
{
	CChooseLayoutDlg   dlg(m_pDocument);
	if (dlg.DoModal() == IDOK)
	{
		m_pDocument->LayoutMoveObject(&(m_pDocument->GetObjects()),  dlg.m_pSelectedLayout, this);
	}
}

//------------------------------------------------------------------------------
BOOL BaseRect::ParseDropShadow (ViewParser& lex)
{
	lex.SkipToken();

	BOOL ok = lex.ParseInt     (m_nDropShadowHeight);
	if (!ok)
	{
		lex.SetError(_TB("Parser error in field's drop shadow height"));
		return FALSE;
	}
	ok = lex.ParseColor (T_COLOR, m_crDropShadowColor);
	if (!ok)
	{
		lex.SetError(_TB("Parser error in field field's drop shadow color"));
		return FALSE;
	}
	return TRUE;
}	

//------------------------------------------------------------------------------
void BaseRect::UnparseDropShadow (ViewUnparser& ofile)
{
	if (m_nDropShadowHeight > 0)
	{
		ofile.UnparseTag (T_DROPSHADOW, FALSE);
		ofile.UnparseInt (m_nDropShadowHeight, FALSE);
		ofile.UnparseBlank	();
		ofile.UnparseColor (T_COLOR, m_crDropShadowColor);
	}
}

//------------------------------------------------------------------------------
BOOL BaseRect::ParseTooltip (ViewParser& lex)
{
	lex.SkipToken();
	lex.Matched(T_ASSIGN);

	WoormTable* pST = &(m_pDocument->m_ViewSymbolTable);
	m_pTooltipExpr = new Expression(pST);
	m_pTooltipExpr->SetStopTokens(T_SEP);
	if (!m_pTooltipExpr->Parse(lex, DataType::String, TRUE))
	{
		lex.SetError(_TB("Parser error in field conditional tooltip expression"));
		return FALSE;
	}

	return lex.ParseSep();
}

//------------------------------------------------------------------------------
void BaseRect::UnparseTooltip (ViewUnparser& ofile)
{
	if (m_pTooltipExpr && !m_pTooltipExpr->IsEmpty() && ! m_pDocument->m_Template.m_bIsSavingTemplate)
	{
		ofile.UnparseTag(T_TOOLTIP, FALSE);
		ofile.UnparseTag(T_ASSIGN, FALSE);

		ofile.UnparseExpr(m_pTooltipExpr->ToString(), FALSE);
		ofile.UnparseSep(TRUE);
	}
}

//------------------------------------------------------------------------------
void BaseRect::OnSetDropShadow ()
{
	CDropShadowDlg dlg(m_crDropShadowColor, m_nDropShadowHeight);
	
	if (dlg.DoModal() == IDOK)
	{
		if (m_nDropShadowHeight != dlg.m_Height)
		{
			CRect rect(m_BaseRect);
			rect.right += m_nDropShadowHeight;
			rect.bottom += m_nDropShadowHeight;

			m_pDocument->InvalidateRect(rect);
		}

		m_crDropShadowColor = (COLORREF)(long) dlg.m_Color;
		m_nDropShadowHeight = (short) dlg.m_Height;

		UpdateDocument();
	}
}

//------------------------------------------------------------------------------
void BaseRect::OnSetTooltip ()
{
	CSetTooltipDlg dlg(m_pTooltipExpr, &(m_pDocument->m_ViewSymbolTable));
	dlg.DoModal();

	UpdateDocument ();
}

//------------------------------------------------------------------------------
CString BaseRect::GetTooltip (int /*nPage = -1*/, CPoint /*CPoint(0,0)*/)
{
	DataStr sTip;

	if (m_pTooltipExpr)
		m_pTooltipExpr->Eval(sTip);
	return sTip;
}

//------------------------------------------------------------------------------
void BaseRect::ToggleTemplate ()
{	
	m_bTemplate = !m_bTemplate;

	m_pDocument->SetModifiedFlag();
}

//------------------------------------------------------------------------------
void BaseRect::OnSetStyle ()
{	
	CListStyleDlg dlg(m_pDocument, m_sStyleClass, this, this);
	if (dlg.DoModal() == IDOK && m_sStyleClass.CompareNoCase(dlg.m_sStyleClass))
	{
		if (dlg.m_sStyleClass.CompareNoCase(CWoormTemplate::s_sCustom_StyleName))
		{
			ClearStyle();
		}

		m_sStyleClass = dlg.m_sStyleClass;
		m_pDocument->SetTemplateStyle(this);

		UpdateDocument();
	}
}

//------------------------------------------------------------------------------
void BaseRect::OnSetStyleName ()
{	
	CSetNameDlg   dlg(m_pDocument, this);
	if (dlg.DoModal() == IDOK  && !dlg.m_sName.IsEmpty())
	{
		m_sStyleClass = dlg.m_sName;
		m_pDefault = NULL;

		m_pDocument->SetModifiedFlag();
	}
}

//------------------------------------------------------------------------------
void BaseRect::OnSetStyleNameForCopyObject ()
{	
	if (m_sStyleClass.CompareNoCase(CWoormTemplate::s_sCopyStaticObject_StyleName) == 0)
	{
		m_sStyleClass.Empty();
	}
	else
	{
		m_sStyleClass = CWoormTemplate::s_sCopyStaticObject_StyleName;
		m_pDefault = NULL;
	}

	m_pDocument->SetModifiedFlag();
}

//------------------------------------------------------------------------------
void BaseRect::OnSetStyleNameForInheritObject ()
{	
	if (m_sStyleClass.CompareNoCase(CWoormTemplate::s_sInheritStaticObject_StyleName) == 0)
	{
		m_sStyleClass.Empty();
	}
	else
	{
		m_sStyleClass = CWoormTemplate::s_sInheritStaticObject_StyleName;
		m_pDefault = NULL;
	}

	m_pDocument->SetModifiedFlag();
}

//------------------------------------------------------------------------------
void BaseRect::OnClearCustomStyle ()
{	
	ClearStyle();

	UpdateDocument();
}

//------------------------------------------------------------------------------
void BaseRect::UnparseProp (ViewUnparser& ofile)
{
	ofile.UnparseCrLf();
	ofile.UnparseBegin ();

	if (!m_sStyleClass.IsEmpty())
	{
		ofile.UnparseTag (T_STYLE, FALSE);
		ofile.UnparseString(m_sStyleClass);
	}
	if (m_bTemplate)
		ofile.UnparseTag (T_TEMPLATE);

	if (IsNotDefaultTrasparent())	
		ofile.UnparseTag        (T_TRANSPARENT);

	if (IsNotDefaultBorderPen())	
		ofile.UnparsePen        (m_BorderPen);

	if (IsNotDefaultBorders() || (this->m_pBordersExpr && !m_pBordersExpr->IsEmpty() && !m_pDocument->m_Template.m_bIsSavingTemplate))
	{
		BOOL bDynamic = this->m_pBordersExpr && !m_pBordersExpr->IsEmpty() && !m_pDocument->m_Template.m_bIsSavingTemplate;
		
		ofile.UnparseBorders    (m_Borders, !bDynamic, !bDynamic);

		if (bDynamic)
		{
			ofile.UnparseTag(T_COMMA, FALSE);

			ofile.UnparseExpr(m_pBordersExpr->ToString(), FALSE);
			ofile.UnparseSep(TRUE);
		}
	}

	if (IsNotDefaultDropShadow())	
		UnparseDropShadow		(ofile);

	if (m_nLayer)
	{
		ofile.UnparseTag		(T_LAYER, FALSE); 
		ofile.UnparseInt		(m_nLayer);
	}

	UnparseTooltip			(ofile);

	UnparseHidden			(ofile);

	UnparsePrintInfo (ofile);

	UnparseAuxProp (ofile);

	ofile.UnparseEnd ();
}

//------------------------------------------------------------------------------
void BaseRect::SubstituteTemplateFont (CWoormDocMng* pDoc, FontIdx& f, FontIdx a, FontIdx b)
{
	if (f == a)
	{
		//if (!pDoc->m_pFontStyles->GetFontStyle(f, FontStyle::FROM_WOORM_TEMPLATE))
		FontStyle* pFont = pDoc->m_pFontStyles->GetFontStyle(b, FontStyle::FROM_STANDARD);
		if (pFont && pFont->IsNoneFont())
			return;
		f = b;
	}
}

//------------------------------------------------------------------------------
void BaseRect::RemoveTemplateFont (CWoormDocMng* pDoc, FontIdx& f, FontIdx def, LPCTSTR fnt)
{
	SubstituteTemplateFont	(pDoc, f, def, pDoc->m_pFontStyles->GetFontIdx(fnt));
}

//------------------------------------------------------------------------------
void BaseRect::SetTemplateFont (CWoormDocMng* pDoc, FontIdx& f, FontIdx def, LPCTSTR fnt)
{
	SubstituteTemplateFont	(pDoc, f, pDoc->m_pFontStyles->GetFontIdx(fnt), def);
}

//------------------------------------------------------------------------------
void BaseRect::ClearTemplateFont (CWoormDocMng* pDoc, FontIdx& f, FontIdx* def, LPCTSTR fnt)
{
	if (def)
	{
		FontStyle* pFont = pDoc->m_pFontStyles->GetFontStyle(*def, FontStyle::FROM_STANDARD);
		if (!(pFont && pFont->IsNoneFont()))
		{
			f = *def;
			return;
		}
	}
	f = pDoc->m_pFontStyles->GetFontIdx(fnt);
}

//------------------------------------------------------------------------------
void BaseRect::SetStyle (BaseRect* pTpl)
{
	if (m_bInheritByTemplate) return;

	RemoveStyle ();
	if (!pTpl) return;
	m_pDefault = pTpl;
	m_sStyleClass = pTpl->m_sStyleClass;
	//---- ----

	if (m_Borders == Borders())
		m_Borders =	m_pDefault->m_Borders;
	if (m_BorderPen == BorderPen())
		m_BorderPen = m_pDefault->m_BorderPen;

	if (!m_bTransparent)
		m_bTransparent = m_pDefault->m_bTransparent;

	if (m_nHRatio == 0 && m_nVRatio == 0)
	{
		m_nHRatio = m_pDefault->m_nHRatio;
		m_nVRatio = m_pDefault->m_nVRatio;
	}

	if (m_nDropShadowHeight == 0 && m_crDropShadowColor == 0)
	{
		m_nDropShadowHeight = m_pDefault->m_nDropShadowHeight;
		m_crDropShadowColor = m_pDefault->m_crDropShadowColor;
	}

}

//------------------------------------------------------------------------------
void	BaseRect::RemoveStyle	()
{
	if (m_bInheritByTemplate) return;

	if (!m_pDefault) return;
	//---- ----

	if (m_Borders == m_pDefault->m_Borders)
		m_Borders =	Borders();

	if (m_BorderPen == m_pDefault->m_BorderPen)
		m_BorderPen = BorderPen();

	if (m_bTransparent == m_pDefault->m_bTransparent)
		m_bTransparent = FALSE;

	if (m_nHRatio == m_pDefault->m_nHRatio && m_nVRatio == m_pDefault->m_nVRatio)
	{
		m_nHRatio = 0;
		m_nVRatio = 0;
	}

	if (m_nDropShadowHeight == m_pDefault->m_nDropShadowHeight &&
		m_crDropShadowColor == m_pDefault->m_crDropShadowColor)
	{
		m_nDropShadowHeight = 0;
		m_crDropShadowColor = 0;
	}

	//---- ----
	m_pDefault = NULL;
}

//------------------------------------------------------------------------------
void BaseRect::ClearStyle	()
{
	if (m_bInheritByTemplate) return;

	m_Borders 			= m_pDefault ? m_pDefault->m_Borders		: Borders();
	m_BorderPen 		= m_pDefault ? m_pDefault->m_BorderPen		: BorderPen();

	m_bTransparent 		= m_pDefault ? m_pDefault->m_bTransparent	: FALSE;

	m_nHRatio 			= m_pDefault ? m_pDefault->m_nHRatio		: 0;
	m_nVRatio 			= m_pDefault ? m_pDefault->m_nVRatio		: 0;

	m_nDropShadowHeight = m_pDefault ? m_pDefault->m_nDropShadowHeight	: 0;
	m_crDropShadowColor = m_pDefault ? m_pDefault->m_crDropShadowColor	: 0;
}

//------------------------------------------------------------------------------
BOOL	BaseRect::IsNotDefaultRatio () const
{
	if (m_pDefault)
		return m_nHRatio != m_pDefault->m_nHRatio ||
				m_nVRatio != m_pDefault->m_nVRatio;

	return (m_nHRatio || m_nVRatio);
}

BOOL	BaseRect::IsNotDefaultBorderPen () const
{
	if (m_pDefault)
		return m_BorderPen != m_pDefault->m_BorderPen;

	return ! m_BorderPen.IsDefault();
}

BOOL	BaseRect::IsNotDefaultBorders () const
{
	if (m_pDefault)
		return m_Borders != m_pDefault->m_Borders;

	return ! m_Borders.IsDefault();
}

BOOL	BaseRect::IsNotDefaultTrasparent () const
{
	if (m_pDefault)
		return m_bTransparent != m_pDefault->m_bTransparent;

	return m_bTransparent;
}

BOOL	BaseRect::IsNotDefaultDropShadow () const
{
	if (m_pDefault)
		return m_nDropShadowHeight != m_pDefault->m_nDropShadowHeight ||
				m_crDropShadowColor != m_pDefault->m_crDropShadowColor;

	return m_nDropShadowHeight != 0 ;
}

//---------------------------------------------------------------------------
void BaseRect::MarkTemplateOverridden()
{
	if (m_bTemplate && m_pDefault)
	{
		m_pDefault->m_bTemplateOverridden = TRUE;
	}
}

//---------------------------------------------------------------------------
//ha senso, credo, durante il salvataggio di un template da un report per compattare gli alias
void BaseRect::RenameAlias(int offset)
{
	m_AnchorLeftColumnID += offset;
	m_AnchorRightColumnID += offset;
}

//==============================================================================
//          Class SqrRect implementation
//==============================================================================

//------------------------------------------------------------------------------
IMPLEMENT_DYNAMIC (SqrRect, BaseRect)

//------------------------------------------------------------------------------
BEGIN_MESSAGE_MAP(SqrRect, BaseRect)
	//{{AFX_MSG_MAP(SqrRect)
	ON_COMMAND(ID_COLOR,   SetColors)
	//}}AFX_MSG_MAP
END_MESSAGE_MAP()

//------------------------------------------------------------------------------
SqrRect::SqrRect (CPoint ptCurrPos, CWoormDocMng* pDocument)
	:
	BaseRect    	(ptCurrPos, pDocument),
	m_rgbBkgColor	(DEFAULT_BKGCOLOR)
{
}

SqrRect::SqrRect	(const SqrRect& source) 
	: 
	BaseRect (source), 
	m_rgbBkgColor (source.m_rgbBkgColor) 
{
}

//------------------------------------------------------------------------------
COLORREF SqrRect::DoDraw(CDC& DC, BOOL bPreview, CRect& inside)
{
	DataLng bkgColor(m_rgbBkgColor);
	if (m_pBkgColorExpr)
		m_pBkgColorExpr->Eval(bkgColor);

	inside = m_BaseRect;

	if (m_nHRatio + m_nVRatio)
	{
		DrawRound(DC, (COLORREF)(long)bkgColor, bPreview, !m_bTransparent);

		CRect inside = InsideRect(DC, m_BaseRect, m_BorderPen, m_Borders, NoBorders(DC.IsPrinting()));
		CSize cs(NormalizeRect(m_nHRatio, m_BaseRect), NormalizeRect(m_nHRatio, m_BaseRect, FALSE));
		inside.InflateRect(-cs.cx, -cs.cy);
	}
	else
	{
		Borders borders(this->m_Borders);
		DrawBorders(DC, &borders);
		inside = InsideRect(DC, m_BaseRect, m_BorderPen, borders, NoBorders(DC.IsPrinting()));

		if (!m_bTransparent)
			DrawInside(DC, inside, (COLORREF)(long)bkgColor);
	}
	return (COLORREF)(long)bkgColor;
}

void SqrRect::Draw (CDC& DC, BOOL bPreview)
{           
	if (!PreDraw(&DC))
		return;

	CRect inside;
	DoDraw(DC, bPreview, inside);

	PostDraw(DC, bPreview, m_BaseRect);
}

//------------------------------------------------------------------------------
void SqrRect::SetColors()
{
	COLORREF    colors[CColorsDlg::MAX];  // m_Label, m_Value, back, border
	BOOL        bActive[CColorsDlg::MAX] = {FALSE, FALSE, TRUE, TRUE};

	colors[CColorsDlg::BACKGROUND]  = m_rgbBkgColor;
	colors[CColorsDlg::BORDER]      = m_BorderPen.GetColor();

	Expression* pDummyExpr = NULL;
	CColorsDlg dialog(colors, bActive, m_pTextColorExpr, m_pBkgColorExpr, pDummyExpr, &(m_pDocument->m_ViewSymbolTable));
	if (dialog.DoModal() != IDOK) return;

   //delete expression, sqrrect non ha colorazione dinamica
	SAFE_DELETE(pDummyExpr);
	SAFE_DELETE(m_pTextColorExpr);
	SAFE_DELETE(m_pBkgColorExpr);

	m_rgbBkgColor = (colors[CColorsDlg::BACKGROUND]);
	m_BorderPen.SetColor (colors[CColorsDlg::BORDER]);

	UpdateDocument ();
}

//------------------------------------------------------------------------------
void	SqrRect::SetStyle		(BaseRect* pTpl)
{
	if (m_bInheritByTemplate) return;

	RemoveStyle ();
	if (!pTpl) return;
	__super::SetStyle (pTpl);
	//----

	if (m_rgbBkgColor == DEFAULT_BKGCOLOR)
		m_rgbBkgColor = ((SqrRect*)m_pDefault)->m_rgbBkgColor;
}

void	SqrRect::RemoveStyle	()
{
	if (m_bInheritByTemplate) return;

	if (!m_pDefault) return;
	//----

	if (m_rgbBkgColor == ((SqrRect*)m_pDefault)->m_rgbBkgColor)
		m_rgbBkgColor = DEFAULT_BKGCOLOR;

	//----
	__super::RemoveStyle	();
}

void	SqrRect::ClearStyle	()
{
	if (m_bInheritByTemplate) return;

	m_rgbBkgColor	= m_pDefault ?  ((SqrRect*)m_pDefault)->m_rgbBkgColor		: DEFAULT_BKGCOLOR;

	//----
	__super::ClearStyle	();
}

//------------------------------------------------------------------------------
BOOL	SqrRect::IsNotDefaultBkgColor() const
{
	if (m_pDefault)
		return m_rgbBkgColor != ((SqrRect*)m_pDefault)->m_rgbBkgColor;

	return m_rgbBkgColor != DEFAULT_BKGCOLOR;
}

//------------------------------------------------------------------------------
void SqrRect::UnparseAuxProp (ViewUnparser& ofile)
{
	if (m_pBkgColorExpr != NULL && !m_pBkgColorExpr->IsEmpty())
		UnparseDynamicColor		(ofile, T_BKGCOLOR, m_pBkgColorExpr);
	else if (IsNotDefaultBkgColor())
	{
		ofile.UnparseColor    (T_BKGCOLOR, m_rgbBkgColor, FALSE);
		ofile.UnparseSep      (TRUE);
	}
}

//------------------------------------------------------------------------------
void SqrRect::Unparse (ViewUnparser& ofile)
{
	//---- Template Override
	BaseRect* pDefault = m_pDefault;
	if (m_bTemplate && m_pDocument->m_Template.m_bIsSavingTemplate)
	{
		m_pDefault = NULL;
	}
	//----

	ofile.UnparseRect                   (T_SQRRECT, m_BaseRect, FALSE);
	
	if (IsNotDefaultRatio())
		ofile.UnparseRatio				(m_nHRatio, m_nVRatio,	FALSE);

	UnparseProp(ofile);
	
	//----
	m_pDefault = pDefault;
}

//------------------------------------------------------------------------------
BOOL SqrRect::OnParseProp (ViewParser& lex, BOOL bBlock)
{
	BOOL ok = TRUE;

	switch (lex.LookAhead())
	{
		case T_BKGCOLOR		: 
		{
			ok = ParseBkgColor(lex, this->m_rgbBkgColor);
			break;
		}
		default :
			if (bBlock)
			{
				lex.SetError(_TB("Wrong option for the rectangle generic item."));
				ok = FALSE;
			}
	}
	
	return ok;
}

// per compatibilita' accetta anche la vecchia sintassi di oggetto rotondo
//------------------------------------------------------------------------------
BOOL SqrRect::Parse (ViewParser& lex)
{
	Token nTok = lex.LookAhead(T_RNDRECT) ? T_RNDRECT : T_SQRRECT;

	return
		lex.ParseRect	(nTok, m_BaseRect) &&
		lex.ParseRatio	(m_nHRatio, m_nVRatio) &&
		ParseBlock(lex);
}

//------------------------------------------------------------------------------
void SqrRect::Redraw()
{
	BaseRect::Redraw();
}


//==============================================================================
//          Class TextRect implementation
//==============================================================================

//------------------------------------------------------------------------------
IMPLEMENT_DYNAMIC (TextRect, BaseRect)

//------------------------------------------------------------------------------
BEGIN_MESSAGE_MAP(TextRect, BaseRect)
	//{{AFX_MSG_MAP(TextRect)
	ON_COMMAND(ID_ALIGN,        SetAlign)
	ON_COMMAND(ID_COLOR,        SetColors)
	ON_COMMAND(ID_FONT,         SetFontStyle)
	ON_COMMAND(ID_STATIC_TEXT,  SetStaticText)

	//ON_COMMAND(ID_MODIFY_BARCODE_ATTRIBUTES,  OnToggleVerticalEABarcode)
	ON_COMMAND (ID_TOGGLE_BARCODE,   			ToggleBarCode)
	ON_COMMAND (ID_MODIFY_BARCODE_ATTRIBUTES, 	OnBarCodeAttrs)

	//}}AFX_MSG_MAP
END_MESSAGE_MAP()

//------------------------------------------------------------------------------
TextRect::TextRect (CPoint ptCurrPos, CWoormDocMng* pDocument)
	:
	BaseRect		(ptCurrPos, pDocument)
{
	m_BaseRect.right  = m_BaseRect.left	+ DEFAULT_TEXT_RECT_WIDTH;
	m_BaseRect.bottom = m_BaseRect.top	+ DEFAULT_TEXT_RECT_HEIGHT;
}

TextRect::TextRect (const TextRect& source) 
	: 
	BaseRect		(source), 
	m_StaticText	(source.m_StaticText), 
	m_bSpecialField (source.m_bSpecialField),
	m_bMiniHtml		(source.m_bMiniHtml),
	m_pBarCode		(source.m_pBarCode ? new CBarCode(*(source.m_pBarCode)) : NULL)
{ 
}

//------------------------------------------------------------------------------
TextRect::~TextRect ()
{
	SAFE_DELETE (m_pBarCode)
}

//------------------------------------------------------------------------------

#define FMT_0(a)	if (\
	(_tcsicmp(pszToken, CSpecialField(CSpecialField::a).GetKeyword()) == 0)||\
	(_tcsicmp(pszToken, CSpecialField(CSpecialField::a).GetOldKeyword()) == 0))

#define FMT(a)	FMT_0(a)\
		return cwsprintf

//-----------------------------------------------------------------------------
CString TextRect::FormatToken(CWoormDocMng* pDocument, LPCTSTR pszToken)
{    
	int	nPage = pDocument->m_pRDEmanager->IsClosed() ? 1 : pDocument->m_pRDEmanager->CurrPageRead() + 1;
	int nLast = pDocument->m_pRDEmanager->IsClosed() ? 1 : pDocument->m_pRDEmanager->LastPage() + 1;
	CString strReportName = pDocument->GetPathName ();
	CDocProperties* pDocProp = pDocument->m_pDocProperties;
	DataDate aDate; aDate.SetTodayDate();

	FMT(SPECIAL_PAGE)				(_T("%d"),			nPage);

	FMT_0(SPECIAL_TOT_PAGE)			{
										pDocument->m_bNeedFinalInvalidate = pDocument->IsEngineRunning();
										if (!pDocument->IsEngineRunning() || (pDocument->IsEngineRunning() && pDocument->m_bIsExporting))
											return cwsprintf(_T("%d"),			nLast);
										else
											return _T("*");
									}
	FMT_0(SPECIAL_PAGE_SPLITTER)	{
										return cwsprintf(_T("%d.%d"),	nPage, 	pDocument->GetCurrentSplittedPage() + 1);
									}
	FMT_0(SPECIAL_SPLITTER)			{
										return cwsprintf(_T("%d"),	(nPage-1) * pDocument->GetNumberOfSplittedPage()	+ pDocument->GetCurrentSplittedPage() + 1);
									}
	FMT_0(SPECIAL_TOT_SPLITTER)		{
										pDocument->m_bNeedFinalInvalidate = pDocument->IsEngineRunning();
										if (!pDocument->IsEngineRunning() || (pDocument->IsEngineRunning() && pDocument->m_bIsExporting))
										{
											return cwsprintf(_T("%d"),			nLast * pDocument->GetNumberOfSplittedPage());
										}
										else
											return _T("*");
									}
	//sarebbero da rendere dipendenti dalla Culture
	FMT(SPECIAL_APPDATE)			(_T("%s"),(LPCTSTR)	AfxGetApplicationDate().Str(1));
	FMT(SPECIAL_TODAY)				(_T("%s"),(LPCTSTR)	aDate.Str(1));
	FMT(SPECIAL_TODAY2)				(_T("%0d %s %d"),	TodayDay(), (LPCTSTR)MonthName(TodayMonth()), TodayYear());
	FMT(SPECIAL_MONTH2)				(_T("%s"),(LPCTSTR)	MonthName(TodayMonth()));
	FMT(SPECIAL_HH_MM)				(_T("%02d:%02d"),	TodayHour(), TodayMinute());

	FMT(SPECIAL_YEAR)				(_T("%d"),			TodayYear());
	FMT(SPECIAL_MONTH)				(_T("%d"),			TodayMonth());
	FMT(SPECIAL_DAY)				(_T("%d"),			TodayDay());
	FMT(SPECIAL_HH)					(_T("%d"),			TodayHour());
	FMT(SPECIAL_MM)					(_T("%d"),			TodayMinute());
	FMT(SPECIAL_SEC)				(_T("%d"),			TodaySecond());

	FMT(SPECIAL_USER)				(_T("%s"),(LPCTSTR)	GetUserName(FALSE));
	FMT(SPECIAL_LOGINUSER)			(_T("%s"),(LPCTSTR)	AfxGetLoginInfos()->m_strUserName);
	FMT(SPECIAL_COMPUTER)			(_T("%s"),(LPCTSTR)	GetComputerName(FALSE));
	FMT(SPECIAL_APP_TITLE)			(_T("%s"),(LPCTSTR)	AfxGetBaseApp()->GetAppTitle());
	FMT(SPECIAL_APP_REL)			(_T("%s"),(LPCTSTR)	AfxGetLoginManager()->GetInstallationVersion());
	FMT(SPECIAL_TB_REL)				(_T("TaskBuilder %s"),(LPCTSTR)	GetTBVersion());
	FMT(SPECIAL_SYS_COMPANY_NAME)	(_T("%s"),(LPCTSTR)	(AfxGetLoginInfos()->m_strCompanyName));
	FMT(SPECIAL_REPORTNAME)			(_T("%s"),(LPCTSTR)	(strReportName) );
	FMT(SPECIAL_REPORT_TITLE)		(_T("%s"),(LPCTSTR)	(pDocProp->m_strTitle) );
	FMT(SPECIAL_REPORT_SUBJECT)		(_T("%s"),(LPCTSTR)	(pDocProp->m_strSubject) );
	FMT(SPECIAL_REPORT_AUTHOR)		(_T("%s"),(LPCTSTR)	(pDocProp->m_strAuthor) );
	FMT(SPECIAL_REPORT_COMPANY)		(_T("%s"),(LPCTSTR)	(pDocProp->m_strCompany) );
	FMT(SPECIAL_REPORT_COMMENTS)	(_T("%s"),(LPCTSTR)	(pDocProp->m_strComments) );
	FMT(SPECIAL_LICENSEE)			(_T("%s"),(LPCTSTR)	(AfxGetLoginManager()->GetUserInfoName()) );
	FMT(SPECIAL_DB_COMPANY_NAME)	(_T("%s"),(LPCTSTR)	(AfxGetLoginContext()->GetDBCompanyName()) );
	FMT(SPECIAL_PRODUCER_NAME)		(_T("%s"),(LPCTSTR)	(AfxGetLoginManager()->GetProducerName()) );
	FMT_0(SPECIAL_PRODUCT_DATE)		{
										return AfxGetLoginManager()->GetBuildDate().FormatData();
									}

	CString s = pszToken; s.Trim();
	if (::FindWord(s, L"EVAL" ) == 0)
	{
		Expression expr(pDocument->GetSymTable());
		BOOL b = expr.Parse(s.Mid(5), DataType::String);
		DataStr ds;
		if (expr.Eval(ds))
		{
			return ds.Str();
		}
	}

	/* TODO Special macro from CompanyInfo
	{
		CString s = pszToken; s.Trim();
		int ni = AfxGetLoginContext()->GetCompanyInfoCount();
		for (int i = 0; i < ni; i++)
		{
			if (pszToken.CompareNoCase(AfxGetLoginContext()->GetCompanyTagInfo(i)) == 0)
			{
				return AfxGetLoginContext()->GetCompanyInfo(i));
			}
		}
	}
	*/

	return cwsprintf(_T("%%%s%%"), pszToken);
}

//-----------------------------------------------------------------------------
CString TextRect::BuildText(CWoormDocMng* pDocument, CString strLocalizedText)
{
	TCHAR* pszString = _tcsdup(strLocalizedText.GetBuffer(strLocalizedText.GetLength())); strLocalizedText.ReleaseBuffer();
	TCHAR* pszSubString = pszString;

	TCHAR* pszToken;
	CString strTmp;

	while (TRUE)
	{
		// cerca il primo token
		if (NULL == (pszToken = _tcschr(pszSubString, SPECIAL_FIELD_SEP_START)))
			break;

		*pszToken = NULL_CHAR;
		pszToken++;
		strTmp += pszSubString;

		// cerco il token di terminazione
		if (NULL == (pszSubString = _tcschr(pszToken, SPECIAL_FIELD_SEP_END)))
			break;

		*pszSubString = NULL_CHAR;
		pszSubString++;

		strTmp += FormatToken(pDocument, pszToken);
	}

	// concateno il pezzo rimanente dopo l'ultimo token se c'e`
	strTmp += pszSubString;

	free(pszString);

	return strTmp;
}

// Cerca in una stringa delle parole tra % e le espande secondo la funzione
// FormatToken. Volendo puo` essere generalizzata come funzione non di classe
//-----------------------------------------------------------------------------
CString TextRect::GetBuildText() const
{               
	CString strText = m_StaticText.GetText();
	if (strText.IsEmpty())
		return strText;
	
	CString strLocalizedText = m_StaticText.GetLocalizedText(m_pDocument);

	if (m_bSpecialField)
	{
		strLocalizedText = BuildText(m_pDocument, strLocalizedText);
	}

	return strLocalizedText;
}

CString TextRect::BuildText ()
{               
	CString strText = m_StaticText.GetText();
	if (strText.IsEmpty())
		return strText;
	
	CString strLocalizedText = GetBuildText();

	m_StaticText.SetText(strLocalizedText);

	return strText;
}

//------------------------------------------------------------------------------
BOOL TextRect::HasDynamicExpr() const
{
	//TODO ottimizzazione: solo tag {EVAL} che è raro: si potrebbe ottimizzare
	return m_bSpecialField || __super::HasDynamicExpr();
}

//------------------------------------------------------------------------------
void TextRect::Draw (CDC& DC, BOOL bPreview)
{
	if (!PreDraw(&DC))
		return;

	StaticText _StaticText(m_StaticText);

	_StaticText.SetText(GetBuildText());

	DataLng color;
	if (m_pTextColorExpr && m_pTextColorExpr->Eval(color)) 
		_StaticText.SetTextColor((COLORREF)(long)color);
	if (m_pBkgColorExpr && m_pBkgColorExpr->Eval(color)) 
		_StaticText.SetBkgColor((COLORREF)(long)color);

	CRect inside;
	CSize cs(0, 0);

	if (m_nHRatio + m_nVRatio)
	{
		inside = InsideRect(DC, m_BaseRect, m_BorderPen, m_Borders, NoBorders(DC.IsPrinting()));
		cs = CSize(NormalizeRect(m_nHRatio, m_BaseRect), NormalizeRect(m_nHRatio, m_BaseRect, FALSE));
		inside.InflateRect(-cs.cx, -cs.cy);

		DrawRound(DC, _StaticText.m_rgbBkgColor, bPreview, !m_bTransparent);
	}
	else
	{
		Borders borders(this->m_Borders);
		DrawBorders(DC, &borders);

		inside = InsideRect(DC, m_BaseRect, m_BorderPen, borders, NoBorders(DC.IsPrinting()));
	}

	if (m_pBarCode && !m_StaticText.GetText().IsEmpty())	
	{
		DrawBarCode (DC, m_BaseRect, bPreview);
	}
	else
	{
		if (m_nHRatio + m_nVRatio)
		{
			_StaticText.Draw(DC, inside, m_pDocument, FALSE, 0, cs, m_bMiniHtml, bPreview);
		}
		else
		{
			_StaticText.Draw(DC, inside, m_pDocument, !m_bTransparent, 0, cs, m_bMiniHtml, bPreview);
		}
	}

	PostDraw(DC, bPreview, m_BaseRect);
}

//------------------------------------------------------------------------------
void TextRect::DrawHtmlPreview(CDC& DC, CRect& rect)
{

	CSize cs(0,0);
	// salva la stringa per aggiungere gli special field formattati
	CString strBackup = BuildText();

	m_StaticText.Draw(DC, rect, m_pDocument, !m_bTransparent, 0, cs, TRUE, FALSE);
	// ritorna alla sola string scritta dall'utente
	m_StaticText.SetText(strBackup);


	PostDraw(DC, FALSE, m_BaseRect);
}

// Draw a string instead of m_pBarCode if something is not good
//------------------------------------------------------------------------------
BOOL TextRect::DrawBarCode (CDC& DC, CRect& inside, BOOL bPreview)
{
	CString sErr;
	if (::CBarCode_Draw(m_pBarCode, DC, inside, m_pDocument, m_StaticText, sErr, 0, bPreview))
		return TRUE;

	CString sV = m_StaticText.GetText();
	int nAlignSaved = m_StaticText.GetAlign();

	m_StaticText.SetText(sErr + _T("\n[") + sV + ']');

	if (m_pBarCode->m_bVertical)
	{
		m_StaticText.m_nAlign &= ~(DT_SINGLELINE);
		m_StaticText.m_nAlign |= DT_EX_ORIENTATION_90 | DT_WORDBREAK;
	}

	m_StaticText.Draw(DC, inside, m_pDocument, !m_bTransparent);

	m_StaticText.SetText(sV);
	m_StaticText.SetAlign(nAlignSaved);
	return FALSE;
}

//------------------------------------------------------------------------------
void TextRect::ToggleBarCode ()
{
	if (m_pBarCode)
	{
		SAFE_DELETE(m_pBarCode);
	}
	else
	{ 
		m_pBarCode = new CBarCode ();
	}

	UpdateDocument ();
}

//------------------------------------------------------------------------------
void TextRect::OnBarCodeAttrs ()
{
	if (!m_pBarCode)
		m_pBarCode = new CBarCode ();

	if (GetInternalID() == SpecialReportField::ID.EA_BARCODE)
	{
		OnToggleVerticalEABarcode();
		return;
	}

	CBarCodeAttrsDlg dialog(m_pBarCode, &m_pDocument->m_ViewSymbolTable);

	if ((dialog.DoModal() != IDOK)) 
		return;

	UpdateDocument ();
}

//------------------------------------------------------------------------------
void TextRect::VKReturn()
{
	SetStaticText ();
}

//------------------------------------------------------------------------------
void TextRect::SetFontStyle()
{
	FontIdx nFontIdx = m_StaticText.GetFontIdx();

	// construct font dialogs
	CFontStylesDlg dialog (*(m_pDocument->m_pFontStyles), nFontIdx, FALSE, NULL, m_pDocument->GetNamespace(), FALSE, m_pDocument->m_Template.m_bIsTemplate);
	
	if (dialog.DoModal() != IDOK)
	{
		if (!m_pDocument->IsModified())
			m_pDocument->SetModifiedFlag(m_pDocument->m_pFontStyles->IsModified());
		return;
	}

	m_StaticText.SetFontIdx(nFontIdx);

	UpdateDocument ();
}

//------------------------------------------------------------------------------
BOOL TextRect::OnShowPopup (CMenu& menu)
{
	//BOOL idEA = GetInternalID() == SpecialReportField::ID.EA_BARCODE;
	//if (!idEA)
	//{
	//	menu.AppendMenu(MF_STRING | (m_pBarCode ? MF_CHECKED : 0), ID_TOGGLE_BARCODE,	_TB("Show as ba&r code"));
	//}
	if (m_pBarCode)
	{
		BOOL idEA = GetInternalID() == SpecialReportField::ID.EA_BARCODE;
		if (idEA)
			menu.AppendMenu(MF_STRING | (m_pBarCode->m_bVertical ? MF_CHECKED : 0), ID_MODIFY_BARCODE_ATTRIBUTES, _TB("EasyAttachment Vertical Barcode"));
		//else
		//	menu.AppendMenu(MF_STRING, ID_MODIFY_BARCODE_ATTRIBUTES, _TB("Barcode a&ttributes"));
	}

	return TRUE;
}

//------------------------------------------------------------------------------
void TextRect::SetColors()
{
	COLORREF    colors[CColorsDlg::MAX];  // m_Label, m_Value, back, border
	BOOL        bActive[CColorsDlg::MAX] = {FALSE, TRUE, TRUE, TRUE};

	colors[CColorsDlg::VALUE]       = m_StaticText.GetTextColor();
	colors[CColorsDlg::BACKGROUND]  = m_StaticText.GetBkgColor();
	colors[CColorsDlg::BORDER]      = m_BorderPen.GetColor();

	Expression* pDummyExpr = NULL;	
	CColorsDlg dialog(colors, bActive, m_pTextColorExpr, m_pBkgColorExpr, pDummyExpr, &(m_pDocument->m_ViewSymbolTable));
	if (dialog.DoModal() != IDOK) return;

	//delete expression, textrect ha color. dinamica solo per value e background
	SAFE_DELETE(pDummyExpr);    

	m_StaticText.SetTextColor (colors[CColorsDlg::VALUE]);
	m_StaticText.SetBkgColor  (colors[CColorsDlg::BACKGROUND]);
	m_BorderPen.SetColor      (colors[CColorsDlg::BORDER]);

	UpdateDocument ();
}

//------------------------------------------------------------------------------
void TextRect::SetStaticText()
{
	CString strText = m_StaticText.GetText();
/*
	CSpecialTextDlg dialog (strText, m_bSpecialField);

	if (dialog.DoModal() == IDOK)
	{
		m_StaticText.SetText(strText);

		UpdateDocument ();
	}
*/
	CRSEditView* pEditView = m_pDocument->GetWoormFrame()->CreateEditView();
	if (!pEditView)
		return;
	BOOL changed = FALSE;
	pEditView->EnableStringPreview();
	pEditView->LoadElement(&strText, &changed);
	pEditView->DoEvent();

	if (changed)
	{
		m_StaticText.SetText(strText);
		UpdateDocument();

		m_pDocument->RefreshObjectPropertyView();
	}
}

//------------------------------------------------------------------------------
void TextRect::SetText(const CString& str)	
{
	m_StaticText.SetText(str);

	SetSpecialField();
}

//------------------------------------------------------------------------------
BOOL TextRect::CheckSpecialField(const CString& str) const
{
	//TODO migliorare il controllo
	return str.Find('{') >= 0 && str.Find('}') > 1;
}

//------------------------------------------------------------------------------
BOOL TextRect::SetSpecialField()
{
	CString str = m_StaticText.GetText();

	m_bSpecialField = CheckSpecialField(str);

	return m_bSpecialField;
}

//------------------------------------------------------------------------------
void TextRect::SetStyle (BaseRect* pTpl)
{
	if (m_bInheritByTemplate) return;

	RemoveStyle ();
	if (!pTpl) return;
	ASSERT_KINDOF(TextRect, pTpl);
	__super::SetStyle (pTpl);
	//---- ----

	if (m_StaticText.m_rgbBkgColor == DEFAULT_BKGCOLOR)
		m_StaticText.m_rgbBkgColor = ((TextRect*)m_pDefault)->m_StaticText.m_rgbBkgColor;

	if (m_StaticText.m_rgbTextColor == DEFAULT_TEXTCOLOR)
		m_StaticText.m_rgbTextColor = ((TextRect*)m_pDefault)->m_StaticText.m_rgbTextColor;

	if (m_StaticText.m_nAlign == DEFAULT_TEXT_ALIGN)
		m_StaticText.m_nAlign = ((TextRect*)m_pDefault)->m_StaticText.m_nAlign;

	SetTemplateFont (m_pDocument, m_StaticText.m_nFontIdx, ((TextRect*)m_pDefault)->m_StaticText.m_nFontIdx, FNT_TEXT);
}

//------------------------------------------------------------------------------
void TextRect::RemoveStyle	()
{
	if (m_bInheritByTemplate) return;

	if (!m_pDefault) return;
	//---- ----

	if (m_StaticText.m_rgbBkgColor == ((TextRect*)m_pDefault)->m_StaticText.m_rgbBkgColor)
		m_StaticText.m_rgbBkgColor = DEFAULT_BKGCOLOR;

	if (m_StaticText.m_rgbTextColor == ((TextRect*)m_pDefault)->m_StaticText.m_rgbTextColor)
		m_StaticText.m_rgbTextColor = DEFAULT_TEXTCOLOR;

	if (m_StaticText.m_nAlign == ((TextRect*)m_pDefault)->m_StaticText.m_nAlign)
		m_StaticText.m_nAlign = DEFAULT_TEXT_ALIGN;

	RemoveTemplateFont(m_pDocument, m_StaticText.m_nFontIdx, ((TextRect*)m_pDefault)->m_StaticText.m_nFontIdx, FNT_TEXT);

	//---- ----
	__super::RemoveStyle	();
}

//------------------------------------------------------------------------------
void	TextRect::ClearStyle	()
{
	if (m_bInheritByTemplate) return;

	m_StaticText.m_rgbBkgColor	= m_pDefault ?  ((TextRect*)m_pDefault)->m_StaticText.m_rgbBkgColor		: DEFAULT_BKGCOLOR;
	m_StaticText.m_rgbTextColor = m_pDefault ?  ((TextRect*)m_pDefault)->m_StaticText.m_rgbTextColor	: DEFAULT_TEXTCOLOR;
	m_StaticText.m_nAlign		= m_pDefault ?  ((TextRect*)m_pDefault)->m_StaticText.m_nAlign			: DEFAULT_TEXT_ALIGN;
	
	ClearTemplateFont (m_pDocument, m_StaticText.m_nFontIdx, m_pDefault ? &(((TextRect*)m_pDefault)->m_StaticText.m_nFontIdx) : NULL, FNT_TEXT);

	//---- ----
	__super::ClearStyle	();
}

//------------------------------------------------------------------------------
BOOL	TextRect::IsNotDefaultBkgColor() const
{
	if (m_pDefault)
		return m_StaticText.m_rgbBkgColor != ((TextRect*)m_pDefault)->m_StaticText.m_rgbBkgColor;

	return m_StaticText.m_rgbBkgColor != DEFAULT_BKGCOLOR;
}

BOOL	TextRect::IsNotDefaultTextColor() const
{
	if (m_pDefault)
		return m_StaticText.m_rgbTextColor != ((TextRect*)m_pDefault)->m_StaticText.m_rgbTextColor;

	return m_StaticText.m_rgbTextColor != DEFAULT_TEXTCOLOR;
}

BOOL	TextRect::IsNotDefaultAlignment() const
{
	if (m_pDefault)
		return m_StaticText.m_nAlign != ((TextRect*)m_pDefault)->m_StaticText.m_nAlign;

	return m_StaticText.m_nAlign != DEFAULT_TEXT_ALIGN;
}

BOOL	TextRect::IsNotDefaultFontStyle() const
{
	if (m_pDefault)
		return m_StaticText.m_nFontIdx != ((TextRect*)m_pDefault)->m_StaticText.m_nFontIdx;

	return m_StaticText.m_nFontIdx != m_pDocument->m_pFontStyles->GetFontIdx(FNT_TEXT);
}

//------------------------------------------------------------------------------
void TextRect::UnparseAuxProp (ViewUnparser& ofile)
{
	if (m_pBkgColorExpr != NULL && !m_pBkgColorExpr->IsEmpty())
		UnparseDynamicColor		(ofile, T_BKGCOLOR, m_pBkgColorExpr);
	else if (IsNotDefaultBkgColor())
	{
		ofile.UnparseColor    (T_BKGCOLOR, m_StaticText.m_rgbBkgColor, FALSE);
		ofile.UnparseSep      (TRUE);
	}

	if (m_pTextColorExpr != NULL && !m_pTextColorExpr->IsEmpty())
		UnparseDynamicColor		(ofile,  T_TEXTCOLOR, m_pTextColorExpr);
	else if (IsNotDefaultTextColor())
	{
		ofile.UnparseColor    (T_TEXTCOLOR, m_StaticText.m_rgbTextColor, FALSE);
		ofile.UnparseSep      (TRUE);
	}

	if (m_bSpecialField)	
		ofile.UnparseTag		(T_SPECIAL_FIELD);

	if (m_pBarCode)   
		ofile.UnparseBarCode     (m_pBarCode, TRUE);

	if (IsNotDefaultAlignment())	
		ofile.UnparseAlign		(m_StaticText.m_nAlign);

	if (IsNotDefaultFontStyle())	
		ofile.UnparseFont		(m_pDocument->m_pFontStyles, m_StaticText.m_nFontIdx);

	if (m_bMiniHtml)
		ofile.UnparseTag(T_HTML);
}

//------------------------------------------------------------------------------
void TextRect::Unparse (ViewUnparser& ofile)
{
	//---- Template Override
	BaseRect* pDefault = m_pDefault;
	if (m_bTemplate && m_pDocument->m_Template.m_bIsSavingTemplate)
	{
		m_pDefault = NULL;
	}
	//----

	ofile.UnparseRect	(T_TEXT, m_BaseRect, FALSE);

	if (IsNotDefaultRatio())
		ofile.UnparseRatio	(m_nHRatio, m_nVRatio, FALSE);

	if (GetInternalID())
		ofile.UnparseAlias(GetInternalID(), FALSE);

	ofile.UnparseCEdit	(
							ofile.IsLocalizableTextInCurrentLanguage() ?
								ofile.LoadReportString(m_StaticText.GetText()) :
								m_StaticText.GetText(), 
							FALSE
						); 
	
	UnparseProp (ofile);
	
	//----
	m_pDefault = pDefault;
}

//------------------------------------------------------------------------------
BOOL TextRect::ParseBarCode(ViewParser& lex)
{
	m_pBarCode = new CBarCode();  

	return lex.ParseBarCode(m_pBarCode);
}

void TextRect::SetBarCode(DWORD eBarCodeType, BOOL bVertical /*= FALSE*/)
{
	SAFE_DELETE(m_pBarCode);
	m_pBarCode = new CBarCode();  
	m_pBarCode->m_nBarCodeType = CBarCodeTypes::BarCodeType(eBarCodeType);
	if (bVertical)
		m_pBarCode->m_bVertical = TRUE;
}

void TextRect::OnToggleVerticalEABarcode ()
{
	m_pBarCode->m_bVertical = !m_pBarCode->m_bVertical;
	
	UpdateDocument ();
}

//------------------------------------------------------------------------------
BOOL TextRect::OnParseProp (ViewParser& lex, BOOL bBlock)
{
	BOOL ok = TRUE;

	switch (lex.LookAhead())
	{
		case T_BKGCOLOR     : ok = ParseBkgColor		(lex, m_StaticText.m_rgbBkgColor);	break;
		case T_TEXTCOLOR    : ok = ParseTextColor		(lex, m_StaticText.m_rgbTextColor);	break;

		case T_SPECIAL_FIELD: ok = lex.SkipToken();		m_bSpecialField= TRUE;		break;
		case T_HTML:		  ok = lex.SkipToken();		m_bMiniHtml = TRUE;			break;

		case T_ALIGN        : ok = lex.ParseAlign       (m_StaticText.m_nAlign);    break;

		case T_FONTSTYLE    :
					ok = lex.ParseFont(m_pDocument->m_pFontStyles, m_StaticText.m_nFontIdx);
					break;

		case T_BARCODE_STRIP      : 
					ok = ParseBarCode         (lex);						
					break;

		default :
			if (bBlock)
			{
				lex.SetError(_TB("Wrong option for text object."));
				ok = FALSE;
			}
	}
	return ok;
}

//------------------------------------------------------------------------------
BOOL TextRect::Parse (ViewParser& lex)
{
	CString strText;

	BOOL ok =
		lex.ParseRect   (T_TEXT, m_BaseRect) &&
		lex.ParseRatio	(m_nHRatio, m_nVRatio);

	if (lex.LookAhead(T_ALIAS))
		ok = ok && lex.ParseAlias  (m_wInternalID);

	ok = ok &&
		lex.ParseCEdit  (strText) &&
		ParseBlock      (lex);

	// bug fix 13.016 compatibility code  to unparse correct grammar
	if (CheckSpecialField(strText))
	{
		int idx;
		CString str(strText); str.MakeLower();
		TCHAR* psz = _T("{date (dd mounth yy)}");	//lowercase!!!
		if ((idx = str.Find(psz)) >= 0)
		{
			strText = 
				strText.Left(idx + 1) +
				//SPECIAL_FIELD_SEP_START +
				CSpecialField::GetKeyword(CSpecialField::SPECIAL_TODAY2) +
				//SPECIAL_FIELD_SEP_END +
				strText.Mid(idx - 1 + _tcslen(psz))
				;
		}

		CString sSpecialTotPage = CSpecialField::GetKeyword(CSpecialField::SPECIAL_TOT_PAGE); sSpecialTotPage.MakeLower();
		if ((idx = str.Find(sSpecialTotPage)) >= 0)
		{
			m_pDocument->m_bCanPreviewPagesOnPrinting = FALSE;
		}
		else
		{
			sSpecialTotPage = CSpecialField::GetKeyword(CSpecialField::SPECIAL_TOT_SPLITTER); sSpecialTotPage.MakeLower();
			if ((idx = str.Find(sSpecialTotPage)) >= 0)
			{
				m_pDocument->m_bCanPreviewPagesOnPrinting = FALSE;
			}
		}

		m_bSpecialField = TRUE;
	}

	m_StaticText.SetText(strText);
	return ok;
}

//------------------------------------------------------------------------------
void TextRect::SetAlign()
{
	AlignType   nAlign = m_StaticText.GetAlign();

	CAlignDlg   dialog(nAlign);
	dialog.SetAllowFieldSet(TRUE);

	if (dialog.DoModal() == IDOK)
	{
		m_StaticText.SetAlign(nAlign);

		UpdateDocument ();
	}
}

//---------------------------------------------------------------------------
void BaseRect::CopyAttributes (CObject* pSource)
{
	if (pSource->IsKindOf(RUNTIME_CLASS(BaseRect)))
	{
		BaseRect* pSrc = (BaseRect*) pSource;
		//-----
		this->m_BaseRect.bottom		= this->m_BaseRect.top + (pSrc->m_BaseRect.bottom - pSrc->m_BaseRect.top);
		this->m_BaseRect.right		= this->m_BaseRect.left + (pSrc->m_BaseRect.right - pSrc->m_BaseRect.left);

		this->m_BorderPen			= pSrc->m_BorderPen;
		this->m_Borders				= pSrc->m_Borders;
		this->m_nHRatio				= pSrc->m_nHRatio;
		this->m_nVRatio				= pSrc->m_nVRatio;
		this->m_bTransparent		= pSrc->m_bTransparent;
		this->m_nDropShadowHeight	= pSrc->m_nDropShadowHeight;
		this->m_crDropShadowColor	= pSrc->m_crDropShadowColor;
	}
}

//---------------------------------------------------------------------------
void TextRect::CopyAttributes (CObject* pSource)
{
	BaseRect::CopyAttributes (pSource);

	if (pSource->IsKindOf(RUNTIME_CLASS(BaseRect)))
	{
		if (pSource->IsKindOf(RUNTIME_CLASS(TextRect)))
		{
			TextRect* pSrc = (TextRect*) pSource;
			//-----
			this->m_sStyleClass	= pSrc->m_sStyleClass;

			this->m_StaticText.m_nFontIdx		=	pSrc->m_StaticText.m_nFontIdx;
			this->m_StaticText.m_rgbBkgColor	=	pSrc->m_StaticText.m_rgbBkgColor;
			this->m_StaticText.m_rgbTextColor	=	pSrc->m_StaticText.m_rgbTextColor;
			this->m_StaticText.m_nAlign			=	pSrc->m_StaticText.m_nAlign;
		}
		else if (pSource->IsKindOf(RUNTIME_CLASS(FieldRect)))
		{
			FieldRect* pSrc = (FieldRect*) pSource;
			//-----

			this->m_StaticText.m_nFontIdx		=	pSrc->m_Value.m_nFontIdx;
			this->m_StaticText.m_rgbBkgColor	=	pSrc->m_Value.m_rgbBkgColor;
			this->m_StaticText.m_rgbTextColor	=	pSrc->m_Value.m_rgbTextColor;
			//this->m_StaticText.m_nAlign			=	pSrc->m_Value.m_nAlign;
		}
	}	
	else if (pSource->IsKindOf(RUNTIME_CLASS(TableColumn)))
	{
		TableColumn* pSrc= (TableColumn*) pSource;
		//-----
		this->m_StaticText.m_nFontIdx		=	pSrc->m_Title.GetFontIdx();	
		this->m_StaticText.m_rgbBkgColor	=	pSrc->m_Title.GetBkgColor();
		this->m_StaticText.m_rgbTextColor	=	pSrc->m_Title.GetTextColor();
		//this->m_StaticText.m_nAlign			=	pSrc->m_Title.GetAlign();
		if (pSrc->m_ColumnTitlePen.m_rgbColor != RGB(255,255,255))
			this->m_BorderPen.m_rgbColor	=	pSrc->m_ColumnTitlePen.m_rgbColor;
	}
}

//==============================================================================
//          Class FileRect implementation
//==============================================================================

//------------------------------------------------------------------------------
IMPLEMENT_DYNAMIC (FileRect, TextRect)

//------------------------------------------------------------------------------
BEGIN_MESSAGE_MAP(FileRect, TextRect)
	//{{AFX_MSG_MAP(FileRect)
	ON_COMMAND(ID_TEXT_LOAD,   LoadFromFile)
	ON_COMMAND(ID_STATIC_TEXT,  LoadFromFile)
	//}}AFX_MSG_MAP(
END_MESSAGE_MAP()

//------------------------------------------------------------------------------
FileRect::FileRect (CPoint ptCurrPos, const CString& strFileName, CWoormDocMng* pDocument)
	:
	TextRect    	(ptCurrPos, pDocument),
	m_strFileName	(strFileName),
	m_pRichEditCtrl	(NULL),
	m_bIsRtf		(FALSE)
{
	m_BaseRect.right = m_BaseRect.left	+ DEFAULT_FILE_RECT_WIDTH;
	m_BaseRect.bottom= m_BaseRect.top	+ DEFAULT_FILE_RECT_HEIGHT;

	if (!m_strFileName.IsEmpty()) 
		ReadTextFromFile (m_strFileName);
}

//------------------------------------------------------------------------------
FileRect::FileRect (const FileRect& source)
	:
	TextRect    	(source),
	m_strFileName	(source.m_strFileName),
	m_pRichEditCtrl	(NULL),
	m_bIsRtf		(source.m_bIsRtf)
{
}

//------------------------------------------------------------------------------
FileRect::~FileRect ()
{
	SAFE_DELETE(m_pRichEditCtrl);
}

//------------------------------------------------------------------------------
CString FileRect::GetDescription (BOOL) const
{ 
	if (m_strFileName.IsEmpty())
		return _T("<empty>");

	CTBNamespace aFileNs = AfxGetPathFinder()->GetNamespaceFromPath(m_strFileName);
	CString s = (aFileNs.IsValid() && (aFileNs.GetType() == CTBNamespace::TEXT || aFileNs.GetType() == CTBNamespace::FILE)) ?
				aFileNs.ToString() : m_strFileName;

	return s; 
}

//------------------------------------------------------------------------------
BOOL FileRect::OnShowPopup (CMenu& menu)
{
	return __super::OnShowPopup (menu);
}

//------------------------------------------------------------------------------
void FileRect::Unparse (ViewUnparser& ofile)
{	
	//---- Template Override
	BaseRect* pDefault = m_pDefault;
	if (m_bTemplate && m_pDocument->m_Template.m_bIsSavingTemplate)
	{
		m_pDefault = NULL;
	}
	//----

	ofile.UnparseRect	(T_FILE, m_BaseRect, FALSE);

	if (IsNotDefaultRatio())
		ofile.UnparseRatio	(m_nHRatio, m_nVRatio, FALSE);

	// salvo sempre il namespace completo
	CTBNamespace aFileNs = AfxGetPathFinder()->GetNamespaceFromPath(m_strFileName);
	if (aFileNs.IsValid() && (aFileNs.GetType() == CTBNamespace::TEXT || aFileNs.GetType() == CTBNamespace::FILE))
		ofile.UnparseString(aFileNs.ToString(), FALSE);
	else
		ofile.UnparseString(m_strFileName, FALSE);

	UnparseProp (ofile);
	
	//----
	m_pDefault = pDefault;
}

//------------------------------------------------------------------------------
BOOL FileRect::Parse (ViewParser& lex)
{
	BOOL ok =
		lex.ParseRect   (T_FILE, m_BaseRect)  &&
		lex.ParseRatio	(m_nHRatio, m_nVRatio) &&
		lex.ParseString (m_strFileName)       &&
		ParseBlock      (lex);

	if (ok) 
		Reload(FALSE);

	return ok;
}

//------------------------------------------------------------------------------
void FileRect::Reload(BOOL bInvalidate)
{
	if (!m_strFileName.IsEmpty())
	{
		if (!ExistFile(m_strFileName))
			m_strFileName = AfxGetPathFinder()->FromNs2Path(m_strFileName, CTBNamespace::TEXT, CTBNamespace::FILE);

		if (ExistFile(m_strFileName))
		{
			ReadTextFromFile(m_strFileName);
		}
	}
	if (bInvalidate) Invalidate();
}

//------------------------------------------------------------------------------
void FileRect::ReadTextFromFile(const CString &strFileName)
{
	CString sFile = MapFile(strFileName);
	CString sText;

	m_bIsRtf = FALSE;
	SAFE_DELETE(m_pRichEditCtrl);

	int nIdx = sFile.ReverseFind('.');
	if (nIdx > -1)
	{
		CString sExt = sFile.Mid(nIdx);
		m_bIsRtf = sExt.CompareNoCase(L".rtf") == 0;
	}

	if (::LoadLineTextFile (strFileName, sText))
		SetText (sText);
	else
		SetText (_T(""));
}

//------------------------------------------------------------------------------
void FileRect::LoadFromFile()
{
	CString strName (m_strFileName);

	CTBNamespace aNamespace(CTBNamespace::TEXT);
	aNamespace.SetNamespace(L"Text." + m_pDocument->GetNamespace().GetApplicationName() + '.' + m_pDocument->GetNamespace().GetModuleName());

	ITBExplorer* pExplorer = AfxCreateTBExplorer(ITBExplorer::OPEN, aNamespace);
	pExplorer->SetCanLink();

	if (!pExplorer->Open())
		return;

	CString strPath;	
	pExplorer->GetSelPathElement(strPath);
	
	if (!strPath.IsEmpty())
		m_strFileName = strPath;	

	if (m_strFileName != strName)
	{
		ReadTextFromFile (m_strFileName);

		UpdateDocument ();
	}
}

//---------------------------------------------------------------------------
void FileRect::Draw (CDC& DC, BOOL bPreview)
{
	if (!m_bIsRtf)
		return __super::Draw(DC, bPreview);
	
	//PROTOTIPO
	//CString sMessage( _T("{\\rtf1 RE50W by Jim Dunne Copyright (C) 2005 - 2008\\par http://www.topjimmy.net/tjs \\par {\\field{\\*\\fldinst{HYPERLINK mailto:jim@dunnes.net }}{\\fldrslt{\\cf1\\ul jim@dunnes.net}}}}"));

	if (!PreDraw(&DC))
		return;

	if (m_pRichEditCtrl == NULL)
	{
		CRect rect (m_BaseRect);

		m_pRichEditCtrl = new CRichEditControl50W;

		m_pRichEditCtrl->Create
							(
								WS_BORDER | WS_CHILD | WS_VISIBLE | WS_CLIPSIBLINGS | WS_VSCROLL | ES_AUTOVSCROLL | WS_HSCROLL | ES_AUTOHSCROLL | ES_MULTILINE | ES_READONLY,
								rect, 
								m_pDocument->GetWoormView(), 
								m_pDocument->m_idcCounter++
							);

		// Send Initialization messages to the window

		//Set the control to accept the maximum amount of text
		m_pRichEditCtrl->LimitText50W(-1);	

		//Set other options flags
		DWORD REOptions = (ECO_AUTOVSCROLL | ECO_AUTOHSCROLL | ECO_NOHIDESEL | ECO_SAVESEL | ECO_SELECTIONBAR);

		//Set other options to the control
		m_pRichEditCtrl->SetOptions50W(	
			ECOOP_OR,	//The type of operation
			REOptions );	//Options

		//Set the contol to automatically detect URLs
		m_pRichEditCtrl->SendMessage( EM_AUTOURLDETECT, TRUE, 0);  

		//Set the event masks for the rich edit control
		m_pRichEditCtrl->SetEventMask50W(
			ENM_SELCHANGE | ENM_LINK	//New event mask for the rich edit control
			);

		//Set the default character formatting...see RichEditControl50W.cpp for function definition
		m_pRichEditCtrl->SetDefaultCharFormat50W(	
			CFM_COLOR | CFM_BOLD | CFM_SIZE | CFM_FACE | CFM_BACKCOLOR,	//Mask options 
			RGB(0,0,0),			//Text Color	
			!CFE_BOLD,			//Text Effects
			//_T("Trebuchet MS"),	//Font name
			//200,					//Font yHeight
			_T("Arial"),			//Font name
			100,					//Font yHeight
			RGB(255,255,255));	//Font background color

	}

	Borders borders(this->m_Borders);
	DrawBorders(DC, &borders);
	CRect inside = InsideRect(DC, m_BaseRect, m_BorderPen, borders, NoBorders(DC.IsPrinting()));

	m_pRichEditCtrl->SetSel50W(0, -1);

	CString sBody = this->m_StaticText.GetText();

	ReplaceTag(sBody);

	m_pRichEditCtrl->SetTextTo50WControl
					(
						sBody,	//sMessage, //Write the text in m_csMessage to the RE Control 
						ST_SELECTION,					//SETTEXT flags value
						1200							//SETTEXT codepage value
					);		

	if (DC.IsPrinting())
	{
		ScaleRect(inside, DC);
		//----
		// TODO NON funziona

		int cxPhysOffset = GetDeviceCaps(DC.m_hDC, PHYSICALOFFSETX);
		int cyPhysOffset = GetDeviceCaps(DC.m_hDC, PHYSICALOFFSETY);
    
		int cxPhys = GetDeviceCaps(DC.m_hDC, PHYSICALWIDTH);
		int cyPhys = GetDeviceCaps(DC.m_hDC, PHYSICALHEIGHT);

		// Create "print preview". 
		//m_pRichEditCtrl->SendMessage(EM_SETTARGETDEVICE, (WPARAM)DC.m_hDC, 0); //cxPhys/2);

		FORMATRANGE fr;

		fr.hdc       = DC.m_hDC;
		fr.hdcTarget = DC.m_hDC;

		// Set page rect to physical page size in twips.
		fr.rcPage.top    = 0;  
		fr.rcPage.left   = 0;  
		fr.rcPage.right  = MulDiv(cxPhys, 1440, GetDeviceCaps(DC.m_hDC, LOGPIXELSX));  
		fr.rcPage.bottom = MulDiv(cyPhys, 1440, GetDeviceCaps(DC.m_hDC, LOGPIXELSY)); 
		//fr.rcPage = m_BaseRect;

		// Set the rendering rectangle to the printable area of the page.
		fr.rc.left   = cxPhysOffset;
		fr.rc.right  = cxPhysOffset + cxPhys;
		fr.rc.top    = cyPhysOffset;
		fr.rc.bottom = cyPhysOffset + cyPhys;
		//fr.rc = m_BaseRect;

		fr.chrg.cpMin = 0;
		fr.chrg.cpMax = -1;
		//m_pRichEditCtrl->SendMessage(EM_SETSEL, 0, (LPARAM)-1);          // Select the entire contents.
		//m_pRichEditCtrl->SendMessage(EM_EXGETSEL, 0, (LPARAM)&fr.chrg);  // Get the selection into a CHARRANGE.

		m_pRichEditCtrl->SendMessage(EM_FORMATRANGE, TRUE, (LPARAM)&fr);

		m_pRichEditCtrl->SendMessage(EM_DISPLAYBAND, 0, (LPARAM)&m_BaseRect); 

		//m_pRichEditCtrl->SendMessage(EM_FORMATRANGE, FALSE, 0);
		
	}
	else
		m_pRichEditCtrl->RedrawWindow();

	PostDraw(DC, bPreview, inside);
}

/*-------------------------------------------------------------------------------------------------------------
BOOL PrintRTF(HWND hwnd, HDC hdc)
{
    DOCINFO di = { sizeof(di) };
    
    if (!StartDoc(hdc, &di))
    {
        return FALSE;
    }

    int cxPhysOffset = GetDeviceCaps(hdc, PHYSICALOFFSETX);
    int cyPhysOffset = GetDeviceCaps(hdc, PHYSICALOFFSETY);
    
    int cxPhys = GetDeviceCaps(hdc, PHYSICALWIDTH);
    int cyPhys = GetDeviceCaps(hdc, PHYSICALHEIGHT);

    // Create "print preview". 
    SendMessage(hwnd, EM_SETTARGETDEVICE, (WPARAM)hdc, cxPhys/2);

    FORMATRANGE fr;

    fr.hdc       = hdc;
    fr.hdcTarget = hdc;

    // Set page rect to physical page size in twips.
    fr.rcPage.top    = 0;  
    fr.rcPage.left   = 0;  
    fr.rcPage.right  = MulDiv(cxPhys, 1440, GetDeviceCaps(hDC, LOGPIXELSX));  
    fr.rcPage.bottom = MulDiv(cyPhys, 1440, GetDeviceCaps(hDC, LOGPIXELSY)); 

    // Set the rendering rectangle to the pintable area of the page.
    fr.rc.left   = cxPhysOffset;
    fr.rc.right  = cxPhysOffset + cxPhys;
    fr.rc.top    = cyPhysOffset;
    fr.rc.bottom = cyPhysOffset + cyPhys;

    SendMessage(hwnd, EM_SETSEL, 0, (LPARAM)-1);          // Select the entire contents.
    SendMessage(hwnd, EM_EXGETSEL, 0, (LPARAM)&fr.chrg);  // Get the selection into a CHARRANGE.

    BOOL fSuccess = TRUE;

    // Use GDI to print successive pages.
    while (fr.chrg.cpMin < fr.chrg.cpMax && fSuccess) 
    {
        fSuccess = StartPage(hdc) > 0;
        
        if (!fSuccess) break;
        
        int cpMin = ::SendMessage(hwnd, EM_FORMATRANGE, TRUE, (LPARAM)&fr);
        
        if (cpMin <= fr.chrg.cpMin) 
        {
            fSuccess = FALSE;
            break;
        }
        
        fr.chrg.cpMin = cpMin;
        fSuccess = EndPage(hdc) > 0;
    }
    
    ::SendMessage(hwnd, EM_FORMATRANGE, FALSE, 0);
    
    if (fSuccess)
    {
        EndDoc(hdc);
    } 
    else  
    {
        AbortDoc(hdc);
    }
    
    return fSuccess;
}

// First obtain a pointer to a printer DC.
CPageSetupDialog psDlg;
if (IDOK == psDlg.DoModal())
{
   CDC* pMyPrinterDC = CDC::FromHandle(psDlg.CreatePrinterDC());

   FORMATRANGE fr;

   // Get the page width and height from the printer. 
   long lPageWidth = ::MulDiv(pMyPrinterDC->GetDeviceCaps(PHYSICALWIDTH),
       1440, pMyPrinterDC->GetDeviceCaps(LOGPIXELSX));
   long lPageHeight = ::MulDiv(pMyPrinterDC->GetDeviceCaps(PHYSICALHEIGHT),
       1440, pMyPrinterDC->GetDeviceCaps(LOGPIXELSY));
   CRect rcPage(0, 0, lPageWidth, lPageHeight);

   // Format the text and render it to the printer.
   fr.hdc = pMyPrinterDC->m_hDC;
   fr.hdcTarget = pMyPrinterDC->m_hDC;
   fr.rc = rcPage;
   fr.rcPage = rcPage;
   fr.chrg.cpMin = 0;
   fr.chrg.cpMax = -1;
   m_myRichEditCtrl.FormatRange(&fr, TRUE);

   // Update the display with the new formatting.
   RECT rcClient;
   m_myRichEditCtrl.GetClientRect(&rcClient);
   m_myRichEditCtrl.DisplayBand(&rcClient);   

   pMyPrinterDC->DeleteDC();
}
-----------------------------------------------------------------------------*/

//---------------------------------------------------------------------------
void FileRect::ReplaceTag (CString& sBody)
{
	//if (m_pDocument == NULL || m_pDocument->m_pEngine == NULL || m_pDocument->m_pEngine->GetSymTable() == NULL)
	//	return;
	if (m_pDocument == NULL)
		return;

	CString sTag (L"$$");
	for (;;)
	{
		int nStartPos = sBody.Find(sTag);
		if (nStartPos < 0) break;
		int nEndPos = sBody.Find(sTag, nStartPos + sTag.GetLength());
		if (nEndPos < 0) break;
		
		CString sParamName = sBody.Mid(nStartPos + sTag.GetLength(), nEndPos - nStartPos - sTag.GetLength());
		int nParamNameLenght = sParamName.GetLength();
		sParamName.Trim();

		CString sParamValue;
		
		SymField* pF = m_pDocument->m_ViewSymbolTable.GetField(sParamName);
		DataObj* pV = pF ? pF->GetData() : NULL;
		if (pV)
			sParamValue = pV->FormatData();
		else
			sParamValue = L"??" + sParamName + L"??";
		
		sBody = sBody.Left(nStartPos) + sParamValue + sBody.Mid(nEndPos + sTag.GetLength());
	}
}

//==============================================================================
//          Class CCutBitmapDlg declaration/implementation
//			utility "Ritaglia" per classe GraphRect
//==============================================================================

//------------------------------------------------------------------------------
class CCutBitmapDlg : public CLocalizableDialog
{
	DECLARE_DYNAMIC(CCutBitmapDlg)

protected:
	CDlgToolBar		m_toolBar;


	CTBPicture	m_Bitmap;
	CRect		m_rectBitmap;

	BOOL&		m_bParamIsCutted;		//parametri di uscita
	CRect&		m_rectParamSelectedCut;

	BOOL 		m_bIsSelected;		//variabili di lavoro
	CRect		m_rectWorkCut;
	CPoint		m_ptFrom;			
	CPoint		m_ptTo;
	BOOL		m_bIsTracking;
	//CPen		m_penRect;

	//gestione scrolling 
	int m_nScrollStepX;
	int m_nScrollStepY;

	int m_nPageSizeY;
	int m_nScrollMaxY;

	int m_nPageSizeX;
	int m_nScrollMaxX;

	int m_nScrollPosX;
	int m_nScrollPosY;
	
	int m_nHeight;
	int m_nWidth;

	int m_nHeightToolbar;
	//----

public:
	CCutBitmapDlg		
		( 
			CWnd* pParent, 
			const CString& strFileName, 
			CRect& rectSelCut, 
			BOOL& bIsCutted 
		);
	~CCutBitmapDlg ();

public:
	virtual BOOL    OnInitDialog	();
	virtual void    OnOK			();

	afx_msg void	OnCancelCut		();
	
	virtual	void	OnPaint			();

	afx_msg void	OnMouseMove		( UINT nFlags, CPoint point );
	afx_msg void	OnLButtonDown	( UINT nFlags, CPoint point );
	afx_msg void	OnLButtonUp		( UINT nFlags, CPoint point );

	afx_msg void OnSize				(UINT nType, int cx, int cy);
	afx_msg void OnVScroll			(UINT nCode, UINT nPos, CScrollBar* pScrollBar);
	afx_msg void OnHScroll			(UINT nCode, UINT nPos, CScrollBar* pScrollBar);
			void PrepareScrollInfo	(int cx, int cy);
	
	DECLARE_MESSAGE_MAP()
};

//------------------------------------------------------------------------------
IMPLEMENT_DYNAMIC ( CCutBitmapDlg, CLocalizableDialog );

//------------------------------------------------------------------------------
BEGIN_MESSAGE_MAP(CCutBitmapDlg, CLocalizableDialog)
	//{{AFX_MSG_MAP(CCutBitmapDlg)
	ON_WM_VSCROLL	()
	ON_WM_HSCROLL	()
	ON_WM_SIZE		()

	ON_WM_PAINT			()
	ON_WM_MOUSEMOVE		()
	ON_WM_LBUTTONDOWN	()
	ON_WM_LBUTTONUP		()

	ON_COMMAND( ID_BMP_DOPASTE, OnOK)
	//}}AFX_MSG_MAP
END_MESSAGE_MAP()

//------------------------------------------------------------------------------
CCutBitmapDlg::CCutBitmapDlg ( CWnd* pParentWnd, const CString& strFileName, CRect& rectSelCut, BOOL& bIsCutted )
	:
	CLocalizableDialog		(IDD_BMP_CUT, pParentWnd),

	m_rectParamSelectedCut	(rectSelCut),

	m_bParamIsCutted		(bIsCutted),
	m_bIsTracking			(FALSE),
	m_bIsSelected			(bIsCutted),

	m_nHeightToolbar		(0),

	m_nHeight				(0),
	m_nWidth				(0),

	m_nScrollStepX			(0),
	m_nScrollStepY			(0),

	m_nPageSizeY			(0),
	m_nScrollMaxY			(0),

	m_nPageSizeX			(0),
	m_nScrollMaxX			(0),

	m_nScrollPosX			(0),
	m_nScrollPosY			(0)
{
	if (!m_Bitmap.ReadFile(strFileName, TRUE, TRUE))
		return;

	// calcolo del rettangolo di taglio. Si deve normalizzare con 
	// l'altezza della toolbar dopo che la stessa e` stata creata
	if (bIsCutted)
	{
		m_rectWorkCut.left  = rectSelCut.left;
		m_rectWorkCut.right  = rectSelCut.right;

		// verso di orientamento dei bitmap diverso a secondo del tipo
		if (m_Bitmap.TopLeftOrientation())
		{
			m_rectWorkCut.top  = rectSelCut.bottom;
			m_rectWorkCut.bottom  = rectSelCut.top;
		}
		else
		{
			m_rectWorkCut.top  = m_Bitmap.GetHeight() - rectSelCut.top;
			m_rectWorkCut.bottom  = m_Bitmap.GetHeight() - rectSelCut.bottom;
		}
	}
	else
	{
		m_rectWorkCut.left = 0 ;
		m_rectWorkCut.top  = 0 ;

		m_rectWorkCut.right = m_Bitmap.GetWidth();
		m_rectWorkCut.bottom = m_Bitmap.GetHeight();
	}
	m_nHeight = m_Bitmap.GetHeight();
	m_nWidth = m_Bitmap.GetWidth();
}

//------------------------------------------------------------------------------
CCutBitmapDlg::~CCutBitmapDlg ()
{
	if ( GetCapture () == this )
		ReleaseCapture ();
}

//------------------------------------------------------------------------------
BOOL CCutBitmapDlg::OnInitDialog ()
{
	CLocalizableDialog::OnInitDialog ();

	CRect rcClientStart;
	GetClientRect (rcClientStart);

	// Create toolbar at the top of the dialog window
	if ( m_toolBar.Create (this) )
	{
		m_toolBar.LoadToolBar (IDR_CUT_TOOLBAR);
		m_toolBar.GetToolBarCtrl().EnableButton(ID_BMP_DOPASTE, m_bIsSelected);
	}

	// And position the control bars
	RepositionBars (AFX_IDW_CONTROLBAR_FIRST, AFX_IDW_CONTROLBAR_LAST, 0);
	ShowWindow(SW_SHOW);
	// ---------------------------------------------------------------------------------

	CRect rectToolbar;
	m_toolBar.GetWindowRect(&rectToolbar);
	m_nHeightToolbar = rectToolbar.Height();

	m_rectBitmap = CRect(0, m_nHeightToolbar, m_Bitmap.GetWidth(), m_Bitmap.GetHeight() + m_nHeightToolbar);
	m_rectWorkCut.top  += m_nHeightToolbar ;
	m_rectWorkCut.bottom  += m_nHeightToolbar ;

	return FALSE;
}

//------------------------------------------------------------------------------
void CCutBitmapDlg::PrepareScrollInfo (int cx, int cy)
{
	int nOldHeight = m_nHeight;
	int nWidthVScroll = 0;
	int nOldWidth = m_nWidth;
	int nHeightHScroll = 0;

	m_nHeight = m_Bitmap.GetHeight() + m_nHeightToolbar;
	m_nWidth = m_Bitmap.GetWidth();

	m_nScrollStepX = 10;
	m_nScrollStepY = 10;

	m_nScrollPosX = 0;
	m_nScrollPosY = 0;

	if (cy < m_nHeight) {
		m_nScrollMaxY = m_nHeight;
		m_nPageSizeY = cy;
		m_nScrollPosY = min (m_nScrollPosY, m_nHeight - m_nPageSizeY);
   }
	else
		m_nScrollMaxY = m_nScrollPosY = m_nPageSizeY = 0;

	SCROLLINFO si;
	si.fMask = SIF_PAGE | SIF_RANGE | SIF_POS | SIF_DISABLENOSCROLL;

	si.nMin = 0;
	si.nMax = m_nScrollMaxY;
	si.nPos = m_nScrollPosY;
	si.nPage = m_nPageSizeY;

	SetScrollInfo (SB_VERT, &si, TRUE);

	if (cx < m_nWidth) {
		m_nScrollMaxX = m_nWidth;
		m_nPageSizeX = cx;
		m_nScrollPosX = min (m_nScrollPosX, m_nWidth - m_nPageSizeX);
	}
	else
		m_nScrollMaxX = m_nScrollPosX = m_nPageSizeX = 0;

	si.nMin = 0;
	si.nMax = m_nScrollMaxX;
	si.nPos = m_nScrollPosX;
	si.nPage = m_nPageSizeX;

	SetScrollInfo (SB_HORZ, &si, TRUE);
}

//------------------------------------------------------------------------------
void CCutBitmapDlg::OnSize (UINT nType, int cx, int cy)
{
	CLocalizableDialog::OnSize (nType, cx, cy);	
	// ----

	PrepareScrollInfo(cx,cy);

	// And position the control bars
	RepositionBars (AFX_IDW_CONTROLBAR_FIRST, AFX_IDW_CONTROLBAR_LAST, 0);
	RedrawWindow();
}

//------------------------------------------------------------------------------
void CCutBitmapDlg::OnVScroll (UINT nCode, UINT nPos, CScrollBar* pScrollBar)
{
	int nDelta;
	int nMaxPos = m_nHeight - m_nPageSizeY;

	switch (nCode) {

	case SB_LINEUP:
		if (m_nScrollPosY <= 0)
			return;
		nDelta = -(min (m_nScrollStepY, m_nScrollPosY));
		break;

	case SB_PAGEUP:
		if (m_nScrollPosY <= 0)
			return;
		nDelta = -(min (m_nPageSizeY, m_nScrollPosY));
		break;

	case SB_THUMBPOSITION:
		nDelta = (int) nPos - m_nScrollPosY;
		break;

	case SB_PAGEDOWN:
		if (m_nScrollPosY >= nMaxPos)
			return;
		nDelta = min (m_nPageSizeY, nMaxPos - m_nScrollPosY);
		break;

	case SB_LINEDOWN:
		if (m_nScrollPosY >= nMaxPos)
			return;
		nDelta = min (m_nScrollStepY, nMaxPos - m_nScrollPosY);
		break;

	default: // Ignore other scroll bar messages
		return;
	}

	m_nScrollPosY += nDelta;
	SetScrollPos (SB_VERT, m_nScrollPosY, TRUE);
	ScrollWindow (0, -nDelta);

	RedrawWindow();
}

//------------------------------------------------------------------------------
void CCutBitmapDlg::OnHScroll (UINT nCode, UINT nPos, CScrollBar* pScrollBar)
{
	int nDelta;
	int nMaxPos = m_nWidth - m_nPageSizeX;

	switch (nCode) {

	case SB_LINEUP:
		if (m_nScrollPosX <= 0)
			return;
		nDelta = -(min (m_nScrollStepX, m_nScrollPosX));
		break;

	case SB_PAGEUP:
		if (m_nScrollPosX <= 0)
			return;
		nDelta = -(min (m_nPageSizeX, m_nScrollPosX));
		break;

	case SB_THUMBPOSITION:
		nDelta = (int) nPos - m_nScrollPosX;
		break;

	case SB_PAGEDOWN:
		if (m_nScrollPosX >= nMaxPos)
			return;
		nDelta = min (m_nPageSizeX, nMaxPos - m_nScrollPosX);
		break;

	case SB_LINEDOWN:
		if (m_nScrollPosX >= nMaxPos)
			return;
		nDelta = min (m_nScrollStepX, nMaxPos - m_nScrollPosX);
		break;

	default: // Ignore other scroll bar messages
		return;
	}

	m_nScrollPosX += nDelta;
	SetScrollPos (SB_HORZ, m_nScrollPosX, TRUE);
	ScrollWindow (-nDelta, 0);
	RedrawWindow();
}

//------------------------------------------------------------------------------
void CCutBitmapDlg::OnPaint ()
{
	CPaintDC DC (this);
	m_toolBar.SetWindowPos
			(
				NULL, 0, 0, 0, 0,
				SWP_NOSIZE | SWP_NOZORDER | SWP_NOACTIVATE
			);


	m_toolBar.GetToolBarCtrl().EnableButton(ID_BMP_DOPASTE, m_bIsSelected);
	DC.SetWindowOrg (m_nScrollPosX, m_nScrollPosY);

	CPoint ptOrigin (0, m_nHeightToolbar);
	CSize szBitmap	(m_Bitmap.GetWidth(), m_Bitmap.GetHeight());
	CRect rc		(ptOrigin, szBitmap);

	m_Bitmap.DrawPicture(DC, rc);

	CPoint arptZone[5];
	if ( m_bIsSelected || m_bIsTracking )
	{
		arptZone[0] = CPoint ( m_rectWorkCut.left,	m_rectWorkCut.top );
		arptZone[1] = CPoint ( m_rectWorkCut.right, m_rectWorkCut.top );
		arptZone[2] = CPoint ( m_rectWorkCut.right, m_rectWorkCut.bottom );
		arptZone[3] = CPoint ( m_rectWorkCut.left,	m_rectWorkCut.bottom );
		arptZone[4] = CPoint ( m_rectWorkCut.left,	m_rectWorkCut.top );

		DC.Polyline(arptZone, 5);
	}
}

//------------------------------------------------------------------------------
void CCutBitmapDlg::OnOK ()
{
	if ( ! m_bIsSelected ) 
	{
		return;
	}

	// normalizziamo eliminando la size della toolbar.
	m_rectWorkCut.bottom -= m_nHeightToolbar;
	m_rectWorkCut.top -= m_nHeightToolbar;

	//---- questi non cambiano mai
	m_rectParamSelectedCut.left = m_rectWorkCut.left;
	m_rectParamSelectedCut.right = m_rectWorkCut.right;

	// a seconda del tipo di bitmap (JPG) la disposizione in memoria
	// fatta da LeadTools non corrisponde alle coordinate windows
	if (m_Bitmap.TopLeftOrientation())
	{
		m_rectParamSelectedCut.top = m_rectWorkCut.bottom;
		m_rectParamSelectedCut.bottom = m_rectWorkCut.top;
	}
	else
	{
		m_rectParamSelectedCut.top = m_Bitmap.GetHeight() - m_rectWorkCut.top;
		m_rectParamSelectedCut.bottom = m_Bitmap.GetHeight() - m_rectWorkCut.bottom;
	}

	m_bParamIsCutted = TRUE;

	CLocalizableDialog::OnOK();
}

//--------------------------------------------------------------------------
void CCutBitmapDlg::OnLButtonDown	( UINT nFlags, CPoint point )
{
	CClientDC dc (this);
	dc.SetWindowOrg (m_nScrollPosX, m_nScrollPosY);
	dc.DPtoLP(&point);

	if( m_rectBitmap.PtInRect(point) ) 
	{
		m_bIsTracking = TRUE;
		m_bIsSelected = FALSE;
		m_toolBar.GetToolBarCtrl () . EnableButton(ID_BMP_DOPASTE, m_bIsSelected);

		m_ptFrom = point;
		m_ptTo = point;

		SetCursor( AfxGetApp()->LoadStandardCursor(IDC_SIZEALL) );

		SetCapture ();

		RedrawWindow();
	}
}

//------------------------------------------------------------------------------
void CCutBitmapDlg::OnMouseMove ( UINT nFlags, CPoint point )
{
	CClientDC dc (this);
	dc.SetWindowOrg (m_nScrollPosX, m_nScrollPosY);
	dc.DPtoLP( &point );

	if ( GetCapture () == this && m_bIsTracking && m_rectBitmap.PtInRect (point) )
	{
		SetCursor(AfxGetApp()->LoadStandardCursor (IDC_SIZEALL));
		m_ptTo = point;
		m_rectWorkCut = CRect( min(m_ptFrom.x, m_ptTo.x), max(m_ptFrom.y, m_ptTo.y), max(m_ptFrom.x, m_ptTo.x), min(m_ptFrom.y, m_ptTo.y) );
		
		InvalidateRect(m_rectBitmap, FALSE);
		UpdateWindow();
	} 
	else 
		SetCursor(AfxGetApp()->LoadStandardCursor (IDC_ARROW));
}

//------------------------------------------------------------------------------
void CCutBitmapDlg::OnLButtonUp ( UINT nFlags, CPoint point )
{
	CClientDC dc (this);
	dc.SetWindowOrg (m_nScrollPosX, m_nScrollPosY);
	dc.DPtoLP( &point );

	if ( GetCapture () == this && m_bIsTracking )
	{
		m_bIsTracking = FALSE;
		ReleaseCapture ();
		SetCursor( AfxGetApp()->LoadStandardCursor (IDC_ARROW) );
		
		if ( m_rectBitmap.PtInRect (point) )
		{
			m_ptTo = point;
			m_rectWorkCut = CRect( min(m_ptFrom.x, m_ptTo.x), max(m_ptFrom.y, m_ptTo.y), max(m_ptFrom.x, m_ptTo.x), min(m_ptFrom.y, m_ptTo.y) );
			m_bIsSelected = (m_rectWorkCut.Width() > 0) && (m_rectWorkCut.Height() != 0);
			m_toolBar.GetToolBarCtrl () . EnableButton(ID_BMP_DOPASTE, m_bIsSelected);
		}
		InvalidateRect (m_rectBitmap, FALSE);
		UpdateWindow();
	}
}



//---------------------------------------------------------------------------

//==============================================================================
//          Class GraphRect implementation
//==============================================================================

//------------------------------------------------------------------------------
IMPLEMENT_DYNAMIC (GraphRect, SqrRect)

//------------------------------------------------------------------------------
BEGIN_MESSAGE_MAP(GraphRect, SqrRect)
	//{{AFX_MSG_MAP(GraphRect)
	ON_COMMAND(ID_BMP_LOAD,        LoadFromFile)
	ON_COMMAND(ID_BMP_SAVE,        SaveToFile)
	ON_COMMAND(ID_BMP_STANDARD,    RefreshStandardSize)
	ON_COMMAND(ID_BMP_CURRENT,     RefreshCurrentSize)

	ON_UPDATE_COMMAND_UI(ID_PROPORTIONAL,	OnUpdateProportionalUI )
	ON_UPDATE_COMMAND_UI(ID_BMP_CUT,		OnUpdateCutUI )
	ON_COMMAND(ID_RESIZEPROPORTIONAL, OnResizeProportional )
	ON_COMMAND(ID_BMP_CUT,		OnCutBitmap )
	ON_COMMAND(ID_CANCEL_CUT,	OnCancelCut )
	//}}AFX_MSG_MAP
END_MESSAGE_MAP()

//------------------------------------------------------------------------------
GraphRect::GraphRect(CPoint ptCurrPos, const CString& sImage, CWoormDocMng* pDocument)
	:
	SqrRect 		(ptCurrPos,pDocument),
	m_sImage		(sImage),
	m_bIsCutted		(FALSE),
	m_nAlign		(DEFAULT_GRAPH_ALIGN)
{
	// if no strName is passed, don't create m_Bitmap structure.
	if (!m_sImage.IsEmpty())
	{
		if (m_Bitmap.ReadFile(m_sImage, TRUE, TRUE))
		{
			ConstrainBaseRect();
		}
	}
}

//------------------------------------------------------------------------------
GraphRect::GraphRect(const GraphRect& source)
	:
	SqrRect 		(source),

	m_sImage		(source.m_sImage),
	m_bIsCutted		(source.m_bIsCutted),
	m_rectCutted	(source.m_rectCutted),
	m_nAlign		(source.m_nAlign)
{
	if (!m_sImage.IsEmpty())
	{
		m_Bitmap.ReadFile(m_sImage, TRUE, TRUE);
		m_Bitmap.m_ImageFitMode = source.m_Bitmap.m_ImageFitMode;
	}
}

//------------------------------------------------------------------------------
CString GraphRect::GetDescription (BOOL) const
{ 
	if (m_sImage.IsEmpty())
		return _T("<empty>");

	CTBNamespace aFileNs(CTBNamespace::IMAGE, m_sImage);
	if (!aFileNs.IsValid())
		aFileNs = AfxGetPathFinder()->GetNamespaceFromPath(m_sImage);

	CString s = (aFileNs.IsValid() && (aFileNs.GetType() == CTBNamespace::IMAGE || aFileNs.GetType() == CTBNamespace::FILE)) 
				? aFileNs.ToString() : m_sImage;

	return s; 
}

//------------------------------------------------------------------------------
CRect GraphRect::CalculateBitmapWithFitMode(
		CRect&		rectCuttedHereToFit,
		BOOL&		bHaveToCutted
	)
{
	CRect rectInside = InsideRect(m_BaseRect, m_BorderPen, m_Borders, FALSE);
	if (!m_Bitmap.IsOk())
		return m_BaseRect;

	GenericDrawObj::CalculateBitmapWithFitMode(rectInside, rectCuttedHereToFit, bHaveToCutted, &m_Bitmap, m_bTransparent, m_nAlign, m_bIsCutted, m_rectCutted);
	return rectInside;
}

//------------------------------------------------------------------------------
void GraphRect::Draw (CDC& DC, BOOL bPreview)
{
	if (!PreDraw(&DC))
		return;

	CRect inside;
	COLORREF bkgColor = DoDraw(DC, bPreview, inside);

	if (m_Bitmap.IsOk())
		GenericDrawObj::DrawBitmapWithFitMode(DC, inside, &m_Bitmap, m_bTransparent, bkgColor, m_nAlign, m_bIsCutted, m_rectCutted);

	PostDraw(DC, bPreview, m_BaseRect);
}

//------------------------------------------------------------------------------
BOOL GraphRect::OnShowPopup (CMenu&   menu)
{
	CMenu dataMenu;
	dataMenu.CreatePopupMenu();

	if (!m_AnchorLeftColumnID)
		menu.AppendMenu (MF_STRING, ID_BMP_STANDARD,				MenuStrings::OriginalSize());

	if (!m_AnchorLeftColumnID)
		menu.AppendMenu ( MF_STRING, ID_RESIZEPROPORTIONAL,			MenuStrings::ResizeKeepingRatio());

	return TRUE;
}
//------------------------------------------------------------------------------

void GraphRect::OnUpdateProportionalUI (CCmdUI* pCmdUI)
{
	pCmdUI->SetCheck( m_Bitmap.IsBestFitSetted() );
}
//------------------------------------------------------------------------------
void GraphRect::OnUpdateCutUI (CCmdUI* pCmdUI)
{
	pCmdUI->SetCheck( m_bIsCutted );
}
//------------------------------------------------------------------------------
void GraphRect::OnResizeProportional ()
{
	m_pDocument->InvalidateRect(m_BaseRect);

	CTBPicture::ImageFitMode prevFitMode = m_Bitmap.m_ImageFitMode;
	m_Bitmap.m_ImageFitMode = CTBPicture::ImageFitMode::BEST;
	RectForDrawBitmap( NULL, m_BaseRect, m_rectCutted, &m_Bitmap, m_bIsCutted );
	m_Bitmap.m_ImageFitMode = prevFitMode;
		
	Invalidate(TRUE);
}
//-----------------------------------------------------------------------------------------
void GraphRect::OnCutBitmap ()
{
	CRect rectCutted = m_rectCutted;
	BOOL bIsCutted = m_bIsCutted;

	CCutBitmapDlg dlg
		( 
			this, 
			m_sImage,
			rectCutted, 
			bIsCutted
		);
	if (dlg.DoModal() == IDOK)
	{
		if (bIsCutted && rectCutted.Height() && rectCutted.Width())
		{
			m_rectCutted = rectCutted;
			m_bIsCutted = bIsCutted;
		}
		else m_bIsCutted = FALSE;
	}
	
	Invalidate(TRUE);
}

void GraphRect::OnCancelCut ()
{
	m_bIsCutted = FALSE;
	
	Invalidate(TRUE);
}

//------------------------------------------------------------------------------
void GraphRect::Reload()
{
	m_Bitmap.Reload();
	Invalidate();
}

//------------------------------------------------------------------------------
void GraphRect::LoadFromFile()
{
	CString strName (m_sImage);

	CTBNamespace aNamespace(CTBNamespace::IMAGE);
	aNamespace.SetNamespace(L"Image." + m_pDocument->GetNamespace().GetApplicationName() + '.' + m_pDocument->GetNamespace().GetModuleName());

	ITBExplorer* pExplorer = AfxCreateTBExplorer(ITBExplorer::OPEN, aNamespace);
	pExplorer->SetCanLink();

	if (!pExplorer->Open())
		return;

	CString strPath;	
	pExplorer->GetSelPathElement(strPath);
	
	if (!strPath.IsEmpty())
	{
		CTBNamespace aFileNs = AfxGetPathFinder()->GetNamespaceFromPath(strPath);
		if (aFileNs.IsValid() && (aFileNs.GetType() == CTBNamespace::IMAGE || aFileNs.GetType() == CTBNamespace::FILE))
			m_sImage = aFileNs.ToString();
		else
			m_sImage = strPath;
	}

	m_Bitmap.ReadFile(m_sImage, TRUE, TRUE);
	Invalidate(); 
}

//------------------------------------------------------------------------------
void GraphRect::SaveToFile()
{
	CString strName (m_sImage);
	CTBNamespace aNamespace(CTBNamespace::IMAGE);
	CTBNamespace aExistNamespace = AfxGetPathFinder()->GetNamespaceFromPath(m_sImage);
	if (!aExistNamespace.IsValid())
	{
		aNamespace.SetNamespace(L"Image." + m_pDocument->GetNamespace().GetApplicationName() + '.' + m_pDocument->GetNamespace().GetModuleName());
	}
	else
		aNamespace = aExistNamespace;

	ITBExplorer* pExplorer = AfxCreateTBExplorer(ITBExplorer::SAVE, aNamespace, TRUE, FALSE, FALSE, FALSE);
	BOOL bOk = pExplorer->Open ();
	CStringArray arPath;
	
	pExplorer->GetSelPathElements(&arPath);
	
	if (bOk && arPath.GetSize())
	{
		m_sImage = arPath.GetAt(0);

		if (m_Bitmap.SaveFile(m_sImage))
			m_pDocument->UpdateWindow();

		CTBNamespace aFileNs = AfxGetPathFinder()->GetNamespaceFromPath(m_sImage);
		if (aFileNs.IsValid() && (aFileNs.GetType() == CTBNamespace::IMAGE || aFileNs.GetType() == CTBNamespace::FILE))
			m_sImage = aFileNs.ToString();
	}
}

//------------------------------------------------------------------------------
void GraphRect::ConstrainBaseRect()
{
	int nPenTop     = m_Borders.top   ? m_BorderPen.GetWidth() : 0;
	int nPenBottom  = m_Borders.bottom? m_BorderPen.GetWidth() : 0;
	int nPenLeft    = m_Borders.left  ? m_BorderPen.GetWidth() : 0;
	int nPenRight   = m_Borders.right ? m_BorderPen.GetWidth() : 0;

	CPoint ptPoint (m_Bitmap.GetWidth(), m_Bitmap.GetHeight());

	m_BaseRect.bottom = m_BaseRect.top + ptPoint.y + nPenTop + nPenBottom;
	m_BaseRect.right  = m_BaseRect.left+ ptPoint.x + nPenLeft + nPenRight;
}

//------------------------------------------------------------------------------
void GraphRect::Invalidate(BOOL bCurrentSize /*=FALSE*/ )
{
	// refresh image based on streching mode
	if (!bCurrentSize || m_Bitmap.m_ImageFitMode != CTBPicture::ImageFitMode::STRETCH)
	{
		// send WM_PAINT for Draw to all client area
		ConstrainBaseRect();

		m_pDocument->Invalidate(FALSE);
	}

	UpdateDocument ();
}


//------------------------------------------------------------------------------
void GraphRect::SetBorders()
{
	SqrRect::SetBorders();

	Invalidate();
}

//------------------------------------------------------------------------------
void GraphRect::RefreshCurrentSize()
{
	m_Bitmap.Reload();

	Invalidate(TRUE);
}

//------------------------------------------------------------------------------
void GraphRect::RefreshStandardSize()
{
	if (m_Bitmap.ImageIsValid())
	{
		if (m_bIsCutted)
		{
			m_BaseRect.right = m_BaseRect.left + abs(m_rectCutted.Width());
			m_BaseRect.bottom = m_BaseRect.top + abs(m_rectCutted.Height());
		}
		else
		{
			m_BaseRect.right = m_BaseRect.left + abs(m_Bitmap.GetWidth());
			m_BaseRect.bottom = m_BaseRect.top + abs(m_Bitmap.GetHeight());
		}
		Invalidate(TRUE);
	}
}

//------------------------------------------------------------------------------
void GraphRect::Unparse (ViewUnparser& ofile)
{
	//---- Template Override
	BaseRect* pDefault = m_pDefault;
	if (m_bTemplate && m_pDocument->m_Template.m_bIsSavingTemplate)
	{
		m_pDefault = NULL;
	}
	//----

	ofile.UnparseRect(T_BITMAP, m_BaseRect, FALSE);

	if (m_Bitmap.IsBestFitSetted())
		ofile.UnparseTag (T_PROPORTIONAL, FALSE);
	else if (m_Bitmap.m_ImageFitMode == CTBPicture::ImageFitMode::NORMAL)
		ofile.UnparseTag(T_NATIVE, FALSE);

	if ( m_bIsCutted )
		ofile.UnparseRect (m_rectCutted, FALSE);
	
	// salvo sempre il namespace
	CTBNamespace aFileNs = AfxGetPathFinder()->GetNamespaceFromPath(m_sImage);
	if (aFileNs.IsValid() && (aFileNs.GetType() == CTBNamespace::IMAGE || aFileNs.GetType() == CTBNamespace::FILE))
		ofile.UnparseString(aFileNs.ToString(), FALSE);
	else
		ofile.UnparseString(m_sImage, FALSE);

	UnparseProp(ofile);
	//----
	m_pDefault = pDefault;
}

BOOL GraphRect::IsNotDefaultAlignment() const
{
	if (m_pDefault)
		return (m_nAlign != ((GraphRect*)m_pDefault)->m_nAlign);

	return (m_nAlign != DEFAULT_GRAPH_ALIGN);
}

void GraphRect::UnparseAuxProp(ViewUnparser& ofile)
{
	if (IsNotDefaultAlignment())
		ofile.UnparseAlign(m_nAlign);

	//unparse del colore di sfondo
	SqrRect::UnparseAuxProp(ofile);
}

//------------------------------------------------------------------------------
BOOL GraphRect::OnParseProp(ViewParser& lex, BOOL bBlock)
{
	BOOL ok = TRUE;

	if (lex.LookAhead(T_ALIGN))
		ok = lex.ParseAlign(m_nAlign);

	//parse del colore di sfondo
	BOOL superOk = SqrRect::OnParseProp(lex, FALSE);

	return ok && superOk;
}

BOOL GraphRect::Parse (ViewParser& lex)
{
	m_Bitmap.m_ImageFitMode = CTBPicture::ImageFitMode::STRETCH;
	m_bIsCutted = FALSE;

	BOOL ok = lex.ParseRect (T_BITMAP, m_BaseRect);
	if (!ok)
		return FALSE;

	if (lex.Matched(T_PROPORTIONAL))
	{
		m_Bitmap.m_ImageFitMode = CTBPicture::ImageFitMode::BEST;
	}
	else if (lex.Matched(T_NATIVE))
	{
		m_Bitmap.m_ImageFitMode = CTBPicture::ImageFitMode::NORMAL;
	}

	if (lex.LookAhead(T_RECT) )
	{
		ok = lex.ParseRect (m_rectCutted);
		if (ok)
			m_bIsCutted = TRUE;
	}

	ok = ok && lex.ParseString (m_sImage);

	ok = ok && ParseBlock (lex);

	if (ok)
	{
		if (!m_Bitmap.ReadFile(m_sImage, TRUE, TRUE))
			ok = lex.SetError(_TB("File open error"), m_sImage);
	}
	return ok;
}

//==============================================================================
//          Class MetafileRect implementation
//==============================================================================

//------------------------------------------------------------------------------
IMPLEMENT_DYNAMIC (MetafileRect, SqrRect)

//------------------------------------------------------------------------------
BEGIN_MESSAGE_MAP(MetafileRect, SqrRect)
	ON_COMMAND(ID_BMP_LOAD,        LoadFromFile)
END_MESSAGE_MAP()

//------------------------------------------------------------------------------
MetafileRect::MetafileRect(CPoint ptCurrPos, const CString& strEmfName, CWoormDocMng* pDocument)
	:
	SqrRect 		(ptCurrPos,pDocument),
	m_strFileName	(strEmfName)
{
	CString strName (m_strFileName);

	if ( m_BaseRect.left == m_BaseRect.right ) m_BaseRect.right += 3;
	if ( m_BaseRect.top == m_BaseRect.bottom ) m_BaseRect.bottom += 3;

	// if no strName is passed, don't create m_Bitmap structure.
	if (!strName.IsEmpty())
	{
		if (GetExtension(strName).IsEmpty())
			strName += ".EMF";
		m_emfMetafile.LoadFromFile(strName);
	}
}

//------------------------------------------------------------------------------
MetafileRect::MetafileRect(const MetafileRect& source)
	:
	SqrRect 		(source),

	m_strFileName	(source.m_strFileName)
{
	if (!m_strFileName.IsEmpty()/* && ExistFile(m_strEmfName)*/)
		m_emfMetafile.LoadFromFile(m_strFileName);
}

//------------------------------------------------------------------------------
CString MetafileRect::GetDescription (BOOL) const
{ 
	if (m_strFileName.IsEmpty())
		return _T("[Metafile] <empty>");

	CTBNamespace aFileNs = AfxGetPathFinder()->GetNamespaceFromPath(m_strFileName);
	CString s = (aFileNs.IsValid() && (aFileNs.GetType() == CTBNamespace::IMAGE || aFileNs.GetType() == CTBNamespace::FILE)) ?
				aFileNs.ToString() : m_strFileName;

	return _T("[MetaFile] ") + s; 
}

//---------------------------------------------------------------------------
void MetafileRect::Draw (CDC& DC, BOOL bPreview)
{
	if (!PreDraw(&DC))
		return;

	CRect inside ;
	/*COLORREF bkgColor = */DoDraw(DC, bPreview, inside);

	ScaleRect(inside, DC);
	
	m_emfMetafile.Play (DC, &inside);

	PostDraw(DC, bPreview, m_BaseRect);
}

//------------------------------------------------------------------------------
BOOL MetafileRect::OnShowPopup (CMenu& menu)
{
	CMenu dataMenu;
	dataMenu.CreatePopupMenu();
	menu.AppendMenu(MF_STRING|MF_POPUP, (UINT)dataMenu.GetSafeHmenu() , MenuStrings::Image());
	if (!m_strFileName.IsEmpty())
	{
		CTBNamespace aFileNs = AfxGetPathFinder()->GetNamespaceFromPath(m_strFileName);
		CString s = (aFileNs.IsValid() && (aFileNs.GetType() == CTBNamespace::IMAGE || aFileNs.GetType() == CTBNamespace::FILE)) ?
				aFileNs.ToString() : m_strFileName;
		dataMenu.AppendMenu(MF_STRING|MF_GRAYED, 0, s);
	}
	dataMenu.AppendMenu(MF_STRING, ID_BMP_LOAD,    MenuStrings::EmfLoad());

	menu.AppendMenu(MF_SEPARATOR);
	return TRUE;
}

//------------------------------------------------------------------------------
void MetafileRect::LoadFromFile()
{
	CString strName (m_strFileName);

	CTBNamespace aNamespace(CTBNamespace::IMAGE);
	aNamespace.SetNamespace(L"Image." + m_pDocument->GetNamespace().GetApplicationName() + '.' + m_pDocument->GetNamespace().GetModuleName());

	ITBExplorer* pExplorer = AfxCreateTBExplorer(ITBExplorer::OPEN, aNamespace);
	pExplorer->SetCanLink();

	if (!pExplorer->Open())
		return;

	CString strPath;	
	pExplorer->GetSelPathElement(strPath);
	
	if (!strPath.IsEmpty())
		m_strFileName = strPath;	

	if (m_strFileName != strName )
	{
		// if no strName is passed, don't create m_Bitmap structure.
		if (!strName.IsEmpty())
		{
			if (GetExtension(strName).IsEmpty())
				strName += ".EMF";
			m_emfMetafile.LoadFromFile (strName);
		}
		
		Invalidate(); 
	}
}

//------------------------------------------------------------------------------
void MetafileRect::Invalidate(BOOL bCurrentSize)
{
	// refresh image based on streching mode
	if (!bCurrentSize)
	{
		// send WM_PAINT for Draw to all client area
		m_pDocument->Invalidate(FALSE);
	}

	UpdateDocument ();
}

//------------------------------------------------------------------------------
void MetafileRect::SetBorders()
{
	SqrRect::SetBorders();

	Invalidate();
}

//------------------------------------------------------------------------------
void MetafileRect::Unparse (ViewUnparser& ofile)
{
	//---- Template Override
	BaseRect* pDefault = m_pDefault;
	if (m_bTemplate && m_pDocument->m_Template.m_bIsSavingTemplate)
	{
		m_pDefault = NULL;
	}
	//----

	// cambia volutamente il tipo dell'oggetto
	ofile.UnparseRect (T_BITMAP/*T_METAFILE*/, m_BaseRect, FALSE);

	// salvo sempre il namespace
	CTBNamespace aFileNs = AfxGetPathFinder()->GetNamespaceFromPath(m_strFileName);
	if (aFileNs.IsValid() && (aFileNs.GetType() == CTBNamespace::IMAGE || aFileNs.GetType() == CTBNamespace::FILE))
		ofile.UnparseString(aFileNs.ToString(), FALSE);
	else
		ofile.UnparseString(m_strFileName, FALSE);

	UnparseProp(ofile);

	//----
	m_pDefault = pDefault;
}

//------------------------------------------------------------------------------
BOOL MetafileRect::Parse (ViewParser& lex)
{
	CString sFileName;
	if (
			lex.ParseRect	(T_METAFILE, m_BaseRect) &&
			lex.ParseString (sFileName) && 
			ParseBlock		(lex)
		)
	{
		m_strFileName = AfxGetPathFinder()->FromNs2Path(sFileName, CTBNamespace::IMAGE, CTBNamespace::FILE);

		// if no strName is passed, don't create m_Bitmap structure.
		if (!m_strFileName.IsEmpty())
		{
			if (GetExtension(m_strFileName).IsEmpty())
				m_strFileName += _T(".emf");

			if (ExistFile(m_strFileName) && !m_emfMetafile.LoadFromFile(m_strFileName))
				return lex.SetError(_TB("File open error"), m_strFileName);
		}
	}
	return TRUE;
}

//==============================================================================
//          Class FieldRect implementation
//==============================================================================
const CString FieldRect::EmailParamToString(EmailParameter e)
{
	switch (e)
	{
	case EP_None:					return _TB("None");						//0

	case EP_From:					return _TB("Sender");					//1
	case EP_Subject:				return _TB("Subject");					//2
	case EP_Body:					return _TB("Message body");				//3
	case EP_Cc:						return _TB("Carbon copy");				//4
	case EP_Bcc:					return _TB("Blind carbon copy");		//5

	case EP_Attachment:				return _TB("Attached");					//6
	case EP_RecipientIdentity:		return _TB("Recipient identity");		//7 - Development Only - utilizzato dalle proc. batch per segnalare gli errori
	case EP_To:						return _TB("Recipient");				//8
	case EP_AttachmentReportName:	return _TB("Attachment Report Name");	//9
	case EP_TemplateFileName:		return _TB("Template body File Name");	//10

	case EP_Fax:					return _TB("Fax");						//11
	case EP_Addressee:				return _TB("Addressee");				//12
	case EP_Address:				return _TB("Address");					//13
	case EP_City:					return _TB("City");						//14
	case EP_County:					return _TB("County");					//15
	case EP_Country:				return _TB("Country");					//16
	case EP_ZipCode:				return _TB("ZipCode");					//17
	case EP_ISOCode:				return _TB("ISOCode");					//18
	case EP_DeliveryType:			return _TB("Delivery type");			//19
	case EP_PrintType:				return _TB("Print type");				//20

	case EP_To_by_Certified:		return _TB("Certified recipient");		//21

	default:						return _TB("Unknown");
	}
}

//------------------------------------------------------------------------------
IMPLEMENT_DYNAMIC (FieldRect, BaseRect)

//------------------------------------------------------------------------------
BEGIN_MESSAGE_MAP(FieldRect, BaseRect)

	ON_COMMAND(ID_ALIGN,						SetAlign)
	ON_COMMAND(ID_MODIFY_ENTRY,					ModifyField)
	ON_COMMAND(ID_FORMAT,						SetFormatStyle)
	ON_COMMAND(ID_FONT,							SetFontStyle)

	ON_COMMAND (ID_COLOR,           			SetColors)
	ON_COMMAND (ID_FIELD_LABEL,     			SetFieldLabel)
	ON_COMMAND (ID_LABEL_FONT,      			SetLabelFontStyle)
	ON_COMMAND (ID_ALIGN,						SetAlign)
	ON_COMMAND (ID_LABEL_ALIGN,					SetLabelAlign)

	ON_COMMAND (ID_TOGGLE_BITMAP,   			ToggleBitmap)
	ON_COMMAND (ID_TOGGLE_BARCODE,   			ToggleBarCode)
	ON_COMMAND (ID_TOGGLE_TEXT_FILE,			ToggleTextFile)
	ON_COMMAND (ID_MODIFY_BARCODE_ATTRIBUTES, 	OnBarCodeAttrs)

	ON_UPDATE_COMMAND_UI (ID_PROPORTIONAL,		OnUpdateProportionalUI )
	ON_UPDATE_COMMAND_UI (ID_BMP_CUT,			OnUpdateCutUI )

	ON_COMMAND (ID_RESIZEPROPORTIONAL,			OnResizeProportional )
	ON_COMMAND (ID_BMP_CUT,						OnCutBitmap )
	ON_COMMAND (ID_CANCEL_CUT,					OnCancelCut )
	ON_COMMAND (ID_BMP_STANDARD,				RefreshStandardSize)
	//ON_COMMAND (ID_LINK,						ToggleLink)

	ON_COMMAND_RANGE (ID_TOGGLE_FROM, (UINT)(ID_TOGGLE_TO_CERTIFIED),	ToggleEmailParameter)
	ON_COMMAND (ID_TOGGLE_APPEND_MAIL_PART,								ToggleAppendRecipients)

	ON_COMMAND(ID_DYNAMIC_FORMAT_STYLE,	OnDynamicFormatStyle)

END_MESSAGE_MAP()

//------------------------------------------------------------------------------
FieldRect::FieldRect (CPoint ptCurrPos, CWoormDocMng* pDocument, WORD wID)
	:
	BaseRect		(ptCurrPos, pDocument),
	m_ShowAs		(FT_NORMAL),
	m_pBitmap		(NULL),
	m_pBarCode		(NULL),
	m_bIsCutted		(FALSE),
	m_eEmailParameter (EP_None),
	m_bAppendMailPart (TRUE),
	m_pLabelTextColorExpr (NULL),
	m_pLabelTextExpr (NULL),
	m_pFormatExpr	(NULL),
	m_pFormatter	(NULL)
{
	m_wInternalID 	= wID;
	m_Value.SetLocalizable(FALSE);

	// if 0 mean readed from file otherwise created by user.
	if (wID > 0 && wID < SpecialReportField::REPORT_LOWER_SPECIAL_ID)
	{
		// created by a new wID from editor manager if present also use default m_Value
		DataType    dtype   = m_pDocument->m_pEditorManager->GetDataType(m_wInternalID);
		int         nLen     = m_pDocument->m_pEditorManager->GetLen(m_wInternalID);

		if (dtype == DATA_TXT_TYPE)
			m_Value.SetAlign(DEFAULT_TEXT_ALIGN);

		// build pFormatter for this field and use correct len if any
		SetFormatIdx(m_pDocument->m_pFormatStyles->GetFormatIdx(dtype));
		int lenF = m_pDocument->m_pFormatStyles->GetPaddedLen(m_nFormatIdx, &m_pDocument->GetNamespace());
		if (lenF && dtype != DATA_STR_TYPE) nLen = lenF;

		CDC* pDC = m_pDocument->GetWoormView()->GetDC();
		const FontStyle* pFont = m_pDocument->m_pFontStyles->GetFontStyle(m_Value.m_nFontIdx, &m_pDocument->GetNamespace());
		// rebuild size based on font size
		CSize cf_size = pFont ? pFont->GetStringWidth(pDC, nLen) : CSize (0,0);

		m_pDocument->GetWoormView()->ReleaseDC(pDC);

		int cx  = Max (MINCELL_X, (int)cf_size.cx + 8 * MARGIN_FROM_BORDER);
		int cy  = Max (MINCELL_Y, (int)cf_size.cy);

		m_BaseRect = CRect (ptCurrPos, CSize(cx, cy));

		// Format data for display purpose
		//
		Format();
	}
	else
	{
		// by default string
		if (!m_wInternalID)
		{
			ASSERT_VALID(m_pDocument->m_pFormatStyles);

			SetFormatIdx(m_pDocument->m_pFormatStyles->GetFormatIdx(DATA_STR_TYPE));
		}
		else
		{
			ASSERT_VALID(m_pDocument->m_pEditorManager);
			DataType dtype = m_pDocument->m_pEditorManager->GetDataType(m_wInternalID);
			// created default type because completly modified by Parse
			SetFormatIdx(m_pDocument->m_pFormatStyles->GetFormatIdx(dtype));
		}
	}

	if (wID > 0 && wID < SpecialReportField::REPORT_LOWER_SPECIAL_ID)
	{
		if (!m_pDocument->m_pWoormIni->m_bNoLabelCreate) 
			m_Label.SetText(m_pDocument->m_pEditorManager->GetPublicName(wID));

		// add m_Label cy size based on font size
		const FontStyle* pFont = m_pDocument->m_pFontStyles->GetFontStyle(m_Label.m_nFontIdx, &m_pDocument->GetNamespace());

		CDC* pDC = m_pDocument->GetWoormView()->GetDC();
		CSize cf_size = pFont ? pFont->GetStringWidth(pDC) : CSize (0,0);
		m_pDocument->GetWoormView()->ReleaseDC(pDC);

		m_BaseRect.bottom += Max (MINCELL_Y, (int)cf_size.cy);
	}

	// se richiesto dall'utente crea l'oggetto senza centratura verticale
	if (m_pDocument->m_pWoormIni->m_bBottomAlign)
	{
		m_Value.m_nAlign &= ~DT_VCENTER;
		m_Value.m_nAlign |= DT_BOTTOM;
	}
	m_Value.m_nAlign |= DT_EX_VCENTER_LABEL;
}

//------------------------------------------------------------------------------
FieldRect::FieldRect (const FieldRect& source)
	:
	BaseRect			(source),

	m_ShowAs			(source.m_ShowAs),
	m_bMiniHtml			(source.m_bMiniHtml),

	m_Value 			(source.m_Value),
	m_nFormatIdx 		(source.m_nFormatIdx),
	m_pFormatter		(NULL),

	m_pBitmap			(source.m_pBitmap ? new CTBPicture(*(source.m_pBitmap)) : NULL),
	m_pBarCode			(source.m_pBarCode ? new CBarCode(*(source.m_pBarCode)) : NULL),

	m_bIsCutted 		(source.m_bIsCutted),
	m_rectCutted 		(source.m_rectCutted),

	m_eEmailParameter 	(source.m_eEmailParameter),
	m_bAppendMailPart   (source.m_bAppendMailPart),

	m_Label					(source.m_Label),

	m_pLabelTextExpr		(source.m_pLabelTextExpr		? new Expression(*source.m_pLabelTextExpr)		: NULL),
	m_pLabelTextColorExpr	(source.m_pLabelTextColorExpr	? new Expression(*source.m_pLabelTextColorExpr) : NULL),

	m_pFormatExpr			(source.m_pFormatExpr			? new Expression(*source.m_pFormatExpr)			: NULL)
{
}

//------------------------------------------------------------------------------
FieldRect::~FieldRect ()
{
	SAFE_DELETE (m_pLabelTextExpr)
	SAFE_DELETE (m_pLabelTextColorExpr)

	SAFE_DELETE (m_pBitmap)
	SAFE_DELETE (m_pBarCode)

	SAFE_DELETE (m_pFormatExpr)

	ClearDynamicAttributes();
}

void FieldRect::SetInternalID()
{ ASSERT(FALSE); }		//FieldRect use it

//---------------------------------------------------------------------------
void FieldRect::RenameAlias(int offset)
{
	__super::RenameAlias(offset);
	m_wInternalID += offset;
}

//------------------------------------------------------------------------------
void FieldRect::ClearDynamicAttributes ()
{
	m_arBrowsedOnPage.RemoveAll();
	m_arWrongOnPage.RemoveAll();
	m_arTooltip.RemoveAll();
}

//------------------------------------------------------------------------------
BOOL FieldRect::IsBrowsed(int page)
{
	for (int i=0; i < m_arBrowsedOnPage.GetSize(); i++)
		if (page == m_arBrowsedOnPage[i])
			return TRUE;
	return FALSE;
}

//------------------------------------------------------------------------------
BOOL FieldRect::IsWrong(int page)
{
	for (int i=0; i < m_arWrongOnPage.GetSize(); i++)
		if (page == m_arWrongOnPage[i])
			return TRUE;
	return FALSE;
}

//------------------------------------------------------------------------------
BOOL FieldRect::CanDeleteField	(LPCTSTR pszFieldName, CString& sLog) const
{
	sLog = this->GetFieldName() + _T(" - ");

	CString sLog1;
	if (!__super::CanDeleteField(pszFieldName, sLog1))
		return (sLog += sLog1), FALSE;

	if (m_pLabelTextColorExpr && m_pLabelTextColorExpr->HasMember(pszFieldName))
		return (sLog += _TB("Field label's text color condition")), FALSE;
	if (m_pLabelTextExpr && m_pLabelTextExpr->HasMember(pszFieldName))
		return (sLog += _TB("Field label's text")), FALSE;
	if (m_pFormatExpr && m_pFormatExpr->HasMember(pszFieldName))
		return (sLog += _TB("Field's formatter condition")), FALSE;

	sLog.Empty();
	return TRUE;
}

//------------------------------------------------------------------------------
CString FieldRect::GetTooltip(int nPage) 
{
	CString sTip = __super::GetTooltip(nPage);

	if (nPage < 0 || nPage >= m_arTooltip.GetSize())
		return sTip;
	return  sTip + '\n' + m_arTooltip[nPage];
}

//------------------------------------------------------------------------------
void FieldRect::SetFontStyle()
{
	FontIdx nFontIdx = m_Value.GetFontIdx();

	// construct font dialogs
	CFontStylesDlg dialog (*(m_pDocument->m_pFontStyles), nFontIdx, FALSE, NULL, m_pDocument->GetNamespace(), FALSE, m_pDocument->m_Template.m_bIsTemplate);

	if (dialog.DoModal() != IDOK)
	{
		if (!m_pDocument->IsModified())
			m_pDocument->SetModifiedFlag(m_pDocument->m_pFontStyles->IsModified());
		return;
	}

	m_Value.SetFontIdx(nFontIdx);

	UpdateDocument ();
}

//------------------------------------------------------------------------------
Formatter* FieldRect::GetFormatter()
{
	if (!m_pFormatter || m_pDocument->m_bAllowEditing)
		m_pFormatter = m_pDocument->GetFormatter(m_nFormatIdx);
	ASSERT_VALID(m_pFormatter);
	return m_pFormatter;
}

//------------------------------------------------------------------------------
void FieldRect::Format()
{
	m_Value.Format(m_wInternalID, GetFormatter(), m_pDocument);

	m_pDocument->InvalidateRect(m_BaseRect, m_bTransparent);
}

// in caso di cambio di tipo devo anche aggiornare il formattatore
//------------------------------------------------------------------------------
void FieldRect::ModifyField ()	
{ 
	DataType olddtype = m_pDocument->m_pEditorManager->GetDataType(m_wInternalID);
	if (m_pDocument->m_pEditorManager->ModifyField (m_wInternalID))
	{
		DataType dtype = m_pDocument->m_pEditorManager->GetDataType(m_wInternalID);
		if (olddtype != dtype)
			SetFormatIdx(m_pDocument->m_pFormatStyles->GetFormatIdx(dtype));
	}
}

//------------------------------------------------------------------------------
BOOL FieldRect::CanSearched () const
{ 
	return m_pDocument->m_pFormatStyles->GetDataType(m_nFormatIdx) == DATA_STR_TYPE; 
}

//------------------------------------------------------------------------------
void FieldRect::SetFormatStyle ()
{
	FormatIdx nTmpIdx = m_nFormatIdx;

	CFormatDlg dialog (*(m_pDocument->m_pFormatStyles), nTmpIdx, FALSE, NULL, m_pDocument->GetNamespace());

	if (dialog.DoModal() == IDOK)
	{
		// must reformat all becuase style can used by other object
		SetFormatIdx(nTmpIdx);

		m_pDocument->FormatAll();
	}

	UpdateDocument ();
}

//------------------------------------------------------------------------------
BOOL FieldRect::CheckTypeConsistence(CString& sError)
{
	// check consinstency between editor manager and Format manager
	// must be used AFTER editor manager end Parse.
	DataType    fmt = m_pDocument->m_pFormatStyles->GetDataType(m_nFormatIdx);
	DataType    edt = m_pDocument->m_pEditorManager->GetDataType(m_wInternalID);
	BOOL        ok = fmt.m_wType == edt.m_wType;

	// Bad wID relation
	if (!ok)
	{
		sError = cwsprintf
							(
								_TB("Check format style consistence") + _T(" - ") +
								_TB("Id {0-%d}. Wrong data type\r\n{1-%s} expected\r\n{2-%s}  found\r\n"),
								(int) m_wInternalID,
								(LPCTSTR) FromDataTypeToTokenString(fmt),
								(LPCTSTR) FromDataTypeToTokenString(edt)
							);
		
		m_pDocument->Message 
							(
								sError,
								MB_OK | MB_ICONSTOP
							);
	}
	return ok;
}

//------------------------------------------------------------------------------
BOOL FieldRect::AssignData (WORD wID, RDEManager* RDEmanager)
{
	if (m_wInternalID != wID) 
		return FALSE;

	if (GetInternalID() == SpecialReportField::ID.LAYOUT)
		ForceDisableData ();

	if (m_AnchorRepeaterID) 
		return FALSE;

	RDEmanager->GetDataValue(m_Value.m_RDEdata);

	Format();

	return TRUE;
}

//------------------------------------------------------------------------------
BOOL FieldRect::ExecCommand (WORD wID, RDEManager* pRDEmanager)
{
	if (m_wInternalID != wID) 
		return FALSE;

	// command logic for current object
	switch (pRDEmanager->GetCommand())
	{
		case RDEManager::INPUT_LOWER_DATA:
		{
			m_Value.SetText(m_pDocument->m_sCulture_FIRST);
			break;
		}
		case RDEManager::INPUT_UPPER_DATA:
		{
			m_Value.SetText(m_pDocument->m_sCulture_LAST);
			break;
		}
	}

	pRDEmanager->SkipData();
	return TRUE;
}

//------------------------------------------------------------------------------
void FieldRect::DisableData ()
{
	if (GetInternalID() == SpecialReportField::ID.LAYOUT)
		return;

	ForceDisableData ();
}

//------------------------------------------------------------------------------
void FieldRect::ForceDisableData ()
{
	m_Value.DisableData();

	m_pDocument->InvalidateRect(m_BaseRect);
}

//------------------------------------------------------------------------------
BOOL FieldRect::DeleteEditorEntry ()
{
	__super::DeleteEditorEntry();

	if (m_wInternalID)
	{
		if (m_pDocument->m_Layouts.ExistsFieldID(m_wInternalID, TRUE))
			return TRUE;

		return m_pDocument->m_pEditorManager->DeleteField
			(m_wInternalID/*TRUE , m_pDocument->m_pWoormIni->m_bAlwaysHidden*/);
	}
	return TRUE;
}

//------------------------------------------------------------------------------
void FieldRect::SetAlign()
{
	AlignType   nAlign = m_Value.GetAlign();
	CAlignDlg   dialog(nAlign);

	dialog.SetAllowCenterBottom(TRUE);
	dialog.AttachDocument(m_pDocument);

	if (dialog.DoModal() == IDOK)
	{
		m_Value.SetAlign(nAlign);

		UpdateDocument ();
	}
}

//------------------------------------------------------------------------------
void FieldRect::SetLabelAlign()
{
	AlignType   nAlign = m_Label.GetAlign();
	CAlignDlg   dialog(nAlign);
	dialog.SetAllowFieldSet(TRUE);
	//dialog.SetAllow90(FALSE);

	if (dialog.DoModal() == IDOK)
	{
		m_Label.SetAlign(nAlign);

		UpdateDocument ();
	}
}

//------------------------------------------------------------------------------
void FieldRect::SetLabelFontStyle()
{
	FontIdx nFontIdx = m_Label.GetFontIdx();

	// construct font dialogs
	CFontStylesDlg dialog (*(m_pDocument->m_pFontStyles), nFontIdx, FALSE, NULL, m_pDocument->GetNamespace(), FALSE, m_pDocument->m_Template.m_bIsTemplate);

	if (dialog.DoModal() != IDOK)
	{
		if (!m_pDocument->IsModified())
			m_pDocument->SetModifiedFlag(m_pDocument->m_pFontStyles->IsModified());
		return;
	}

	m_Label.SetFontIdx(nFontIdx);

	m_pDocument->Invalidate();
	UpdateDocument ();
}

//------------------------------------------------------------------------------
Formatter* FieldRect::GetDynamicFormatter	() const
{
	if (!m_pFormatExpr)
		return NULL;

	DataStr sFormatStyle;
	if (m_pFormatExpr->Eval(sFormatStyle))
	{
		ASSERT(m_pDocument->m_pFormatStyles);
		Formatter* fmt = m_pDocument->m_pFormatStyles->GetFormatter(sFormatStyle, & m_pDocument->GetNamespace());
		return fmt;
	}

	return NULL;
}

// Draw m_Bitmap strFileName instead of m_Bitmap if file don't exist
//------------------------------------------------------------------------------
void FieldRect::DrawBitmap (CDC& DC, CRect& inside)
{
	ASSERT_VALID(m_pBitmap);
	if (!m_pBitmap)
		return;

	CString sFileName(m_Value.GetText());
	sFileName.Trim();

	if (!sFileName.IsEmpty())
	{
		//se l'immagine non è valida, scrivo la stringa contenuta nel value
		if (!m_pBitmap->ReadFile(sFileName, TRUE, TRUE) || !m_pBitmap->IsOk())
			m_Value.Draw(DC, inside, m_pDocument, !m_bTransparent);
		else
		{
			GenericDrawObj::DrawBitmapWithFitMode(DC, inside, m_pBitmap, m_bTransparent, m_Value.m_rgbBkgColor, m_Value.GetAlign(), m_bIsCutted, m_rectCutted);

			if (IsOSVistaOrGreater() && m_pDocument)
				m_pDocument->m_bNeedFinalInvalidate = TRUE;
		}
	}
	return;
}

// Draw a string instead of m_pBarCode if something is not good
//------------------------------------------------------------------------------
void FieldRect::DrawBarCode (CDC& DC, CRect& inside, BOOL bPreview)
{
	ASSERT_VALID(m_pBarCode);
	if (!m_pBarCode)
		return;

	int nRow = 0;
	if (m_AnchorRepeaterID)
	{
		nRow = m_nRepeaterRow;
	}
		
	CString sErr;
	if (m_Value.GetText().IsEmpty())
	{
		m_Value.Draw(DC, inside, m_pDocument, !m_bTransparent);
	}
	else if (!::CBarCode_Draw(m_pBarCode, DC, inside, m_pDocument, m_Value, sErr, nRow, bPreview))
	{
		CString sOriginValue = m_Value.GetText();
		CString sV = sOriginValue;
		//se è un barcode composto (esiste il tag 0x01) allora solo la prima parte è il valore del barcode
		if (sV.Find(CBarCodeTypes::BC_SEP) > 0)
			sV = sV.Left(sV.Find(CBarCodeTypes::BC_SEP));

		int nAlignSaved = m_Value.GetAlign();

		m_Value.SetText(sErr + _T("\n[") + sV + ']');

		if (m_pBarCode->m_bVertical)
		{
			m_Value.m_nAlign &= ~(DT_SINGLELINE);
			m_Value.m_nAlign |= DT_EX_ORIENTATION_90|DT_WORDBREAK;
		}
		m_Value.Draw(DC, inside, m_pDocument, !m_bTransparent);

		m_Value.SetText(sOriginValue);
		m_Value.SetAlign(nAlignSaved);
	}
}

//------------------------------------------------------------------------------
CString FieldRect::LoadTextFile (const CString& strFile)
{	
	CString strText;
	if (!strFile.IsEmpty())
		if (!LoadLineTextFile (strFile, strText))
			strText.Empty();
	return strText;
}
//------------------------------------------------------------------------------
void FieldRect::DrawTextFile (CDC& DC, CRect& inside)
{
	CString strFileName = m_Value.GetText();
	if (strFileName.IsEmpty())
	{
		m_Value.Draw(DC, inside, m_pDocument, !m_bTransparent);
		return;
	}

	strFileName = AfxGetPathFinder()->FromNs2Path(strFileName, CTBNamespace::TEXT, CTBNamespace::FILE);

	CString strFileText = LoadTextFile (strFileName);

	m_Value.SetText(strFileText);
	m_Value.Draw(DC, inside, m_pDocument, !m_bTransparent);
	m_Value.SetText(strFileName);
}

//------------------------------------------------------------------------------
void FieldRect::DrawUrlFile (CDC& DC, CRect& inside)
{
	if (DC.IsPrinting())
		return;

	CString strUrl (m_Value.GetText());	strUrl.Trim();
	if (strUrl.IsEmpty())
		return;

	if (m_ctrlWeb.m_hWnd == 0)
	{
		CRect rect (m_BaseRect);
		GetParent()->ClientToScreen(rect);
		m_pDocument->GetWoormView()->ScreenToClient(rect);

		int ret = m_ctrlWeb.Create(rect, m_pDocument->GetWoormView(), m_pDocument->m_idcCounter++);
	}

	try
	{
		TRACE(strUrl + '\n');

		m_ctrlWeb.Navigate (strUrl);
	}
	catch(...)
	{
		ASSERT(FALSE);
	}
}

//------------------------------------------------------------------------------
void FieldRect::EvalExpressions(Value& _Value, Label& _Label)
{
	DataLng color;
	if (m_pTextColorExpr && !m_pTextColorExpr->IsEmpty() && m_pTextColorExpr->Eval(color))
		_Value.SetTextColor((COLORREF)(long)color);
	if (m_pBkgColorExpr && !m_pBkgColorExpr->IsEmpty() && m_pBkgColorExpr->Eval(color))
		_Value.SetBkgColor((COLORREF)(long)color);

	DataStr lab;
	if (m_pLabelTextExpr && !m_pLabelTextExpr->IsEmpty() && m_pLabelTextExpr->Eval(lab))
	{
		_Label.SetText(lab);
		_Label.SetLocalizable(FALSE);
	}
	if (m_pLabelTextColorExpr && !m_pLabelTextColorExpr->IsEmpty() && m_pLabelTextColorExpr->Eval(color))
		_Label.SetTextColor((COLORREF)(long)color);

	// dynamic format style
	if (m_pFormatExpr)
	{
		Formatter* fmt = GetDynamicFormatter();
		if (fmt)
		{
			_Value.FormatWithValue(fmt, m_pDocument);
		}
	}
}

//------------------------------------------------------------------------------
void FieldRect::DrawNormal (CDC& DC, CRect& inside, CSize& cs)
{
	//copy standard m_Value to avoid make persistent dynamic attribute such as class Cell
	Value _Value(m_Value);
	Label _Label(m_Label);
	//----

	int internalID = GetInternalID();

	BOOL bShowLink = !DC.IsPrinting() &&
		m_pDocument->GetEnableLink(NULL, internalID, -1, TRUE);

	CWoormView* pWoormView = (CWoormView*)m_pDocument->GetFirstView();

#ifndef TBWEB 
	if (bShowLink && pWoormView && pWoormView->GetReportDesc() && !_Value.GetText().IsEmpty())
	{
		pWoormView->GetReportDesc()->AddLink(inside, internalID);
	}
#endif //TBWEB

	int flags = bShowLink ? GenericText::LINKED : GenericText::NORMAL;
	if (m_pDocument->m_pRDEmanager && !m_pDocument->m_pRDEmanager->IsClosed())
	{
		int page = m_pDocument->m_pRDEmanager->CurrPageRead();
		BOOL bBrowsed = IsBrowsed(page);
		BOOL bWrong = IsWrong(page);
		if (bShowLink && bBrowsed)
			flags |= GenericText::BROWSED;
		if (bWrong)
			flags |= GenericText::WRONG;
	}
	
	EvalExpressions(_Value, _Label);

	// if present m_Label, must write them in transparent mode
	if (ShowLabels (DC.IsPrinting()))
	{
		// must Draw m_Value first in opaque mode (i.e. fill rectangle)
		CString sLabetText = _Label.GetLocalizedText(m_pDocument);
		CString sValueText = _Value.GetText(); // ?? perchè ? GetLocalizedText(m_pDocument);
		if 
			(
				AfxGetFontStyleTable()->GetUseVCenterBottomAlignInWoormFields() && 
				(_Value.m_nAlign & DT_EX_VCENTER_LABEL) == DT_EX_VCENTER_LABEL &&
				!sLabetText.IsEmpty() &&
				!sValueText.IsEmpty() && 
				(_Value.GetAlign() & DT_VCENTER) == DT_VCENTER
			)
		{
			//GetText ritorna logical unit
			CSize csLabel = GetTextSize(&DC, sLabetText);
			CSize csValue = GetTextSize(&DC, sValueText);
			//DEVO  fare l' unscale dei due CPoint
			UnScaleSize(csLabel, DC);
			UnScaleSize(csValue, DC);

			CRect rectValue (inside);
			int h = inside.Height();
			if ((csLabel.cy + csValue.cy) <= h)
				rectValue.top += csLabel.cy;
			else
				rectValue.top = max (rectValue.top, rectValue.bottom - csValue.cy); //si sovrappongono: simulo il DT_BOTTOM
			
			if (!m_bTransparent)
			{
				CRect rect (inside);
				ScaleRect(rect, DC);

				CBrush brush;
				brush.CreateSolidBrush(_Value.GetBkgColor());
				CBrush*	old_brush = DC.SelectObject(&brush);
				
				DC.FillRect(rect, &brush);
				DC.SelectObject(old_brush);
			}

			_Value.Draw(DC, rectValue, m_pDocument, !m_bTransparent, flags, CSize(0,0), m_bMiniHtml);
			_Label.Draw(DC, inside, m_pDocument, FALSE, 0, cs);
		}
		else
		{
			_Value.Draw(DC, inside, m_pDocument, !m_bTransparent, flags, CSize(0, 0), m_bMiniHtml);
			_Label.Draw(DC, inside, m_pDocument, FALSE, 0, cs);
		}
	}
	else
	{
		_Value.Draw(DC, inside, m_pDocument, !m_bTransparent, flags, CSize(0, 0), m_bMiniHtml);
	}
}

//------------------------------------------------------------------------------
BOOL FieldRect::HasDynamicExpr() const
{
	return m_pLabelTextColorExpr || m_pLabelTextExpr || m_pFormatExpr || __super::HasDynamicExpr();
}

//------------------------------------------------------------------------------
void FieldRect::Draw (CDC& DC, BOOL bPreview)
{
	if (!PreDraw(&DC) )
		return;

	CRect inside;

	if (m_nHRatio + m_nVRatio)
	{
		inside = InsideRect(DC, m_BaseRect, m_BorderPen, m_Borders, NoBorders(DC.IsPrinting()));
		CSize cs (NormalizeRect(m_nHRatio, m_BaseRect), NormalizeRect(m_nHRatio, m_BaseRect, FALSE));
		inside.InflateRect(-cs.cx,-cs.cy);

		DataLng color (m_Value.m_rgbBkgColor);
		if (m_pBkgColorExpr)
			m_pBkgColorExpr->Eval(color);

		DrawRound(DC, (long)color, bPreview, !m_bTransparent);
	}
	else 
	{
		Borders borders(this->m_Borders);
		DrawBorders(DC, &borders);
		inside = InsideRect(DC, m_BaseRect, m_BorderPen, borders, NoBorders(DC.IsPrinting()));
	}

	if (inside.Height() && inside.Width())
	{
		switch (m_ShowAs)
		{
			case FT_IMAGE:
			{
				DrawBitmap (DC, inside);
				break;
			}
			case FT_TEXTFILE:
			{
				DrawTextFile(DC, inside);
				break;
			}
			case FT_URL:
			{
				DrawUrlFile(DC, inside);
				break;
			}
			case FT_BARCODE:
			{
				DrawBarCode(DC, inside, bPreview);
				break;
			}
			default:
			{
				DrawNormal (DC, inside/*, cs*/);
			}
		}

		// disegna le evidenziazioni per segnalare che il campo potra' essere esportato
		if (m_pDocument->m_pExportData && m_pDocument->m_pExportData->IncludeField(m_wInternalID))
			GenericDrawObj::DrawForExport(DC, m_BaseRect);
		}

	PostDraw(DC, bPreview, m_BaseRect);
}


//---------------------------------------------------------------------------
DataType FieldRect::GetDataType() const
{
	SymField* pField = m_pDocument->m_ViewSymbolTable.GetFieldByID(GetInternalID());
	if (pField)
	{
		return pField->GetDataType();
	}
	//ASSERT(FALSE);
	return DataType::Null;
}

//------------------------------------------------------------------------------
CString FieldRect::GetFieldName () const
{
	SymField* pField = m_pDocument->m_ViewSymbolTable.GetFieldByID(m_wInternalID);
	if (pField)
	{
		return pField->GetName();
	}
	return _T("<UNKNOWN FIELD>");
}
//------------------------------------------------------------------------------
CString FieldRect::GetTagName() const
{
	SymField* pField = m_pDocument->m_ViewSymbolTable.GetFieldByID(m_wInternalID);
	if (pField)
	{
		CString tag = pField->GetTag();
		if (tag == _T(""))
			tag = GetFieldName();

		return tag;
	}
	return _T("<UNKNOWN FIELD>");
}

//------------------------------------------------------------------------------
CString FieldRect::GetDescription (BOOL bShowLabel /*= TRUE*/) const
{
	CString s; 
	if (bShowLabel)
	{
		s.Format(_T("%s (Id:%d) %s"), GetFieldName(), m_wInternalID,  m_Label.GetLocalizedText(m_pDocument));
		s.Replace(_T("\\n"), _T(" "));
	}
	else
		s.Format(_T("%s (Id:%d)"), GetFieldName(), m_wInternalID);
	return s;
}

//------------------------------------------------------------------------------
CString FieldRect::GetCaption() const
{
	return m_Label.GetLocalizedText(m_pDocument);
}

//------------------------------------------------------------------------------
BOOL FieldRect::OnShowPopup (CMenu& menu)
{
	return TRUE;
}

//------------------------------------------------------------------------------
void FieldRect::OnDynamicFormatStyle ()
{
	//copia espressioni originarie, da ripristinare in caso di annullamento dell'overwrite
	Expression* saveExpr = m_pFormatExpr 
									?	new Expression(*m_pFormatExpr)
									:	NULL;

	CDynamicFormatStyleColumnDlg dialog(
								m_pFormatExpr,
								&(m_pDocument->m_ViewSymbolTable),
								m_pDocument,
								GetDataType()
							);

	if (dialog.DoModal() != IDOK) 
	{
		SAFE_DELETE(m_pFormatExpr);
		m_pFormatExpr = saveExpr;
		saveExpr = NULL;
		return;
	}
	SAFE_DELETE (saveExpr);

	UpdateDocument ();
}

//------------------------------------------------------------------------------
void FieldRect::SetColors()
{
	COLORREF    colors[CColorsDlg::MAX];  // m_Label, m_Value, back, border
	BOOL        bActive[CColorsDlg::MAX] = {TRUE, TRUE, TRUE, TRUE};

	colors[CColorsDlg::LABEL]       = m_Label.GetTextColor();
	colors[CColorsDlg::VALUE]       = m_Value.GetTextColor();
	colors[CColorsDlg::BACKGROUND]  = m_Value.GetBkgColor();
	colors[CColorsDlg::BORDER]      = m_BorderPen.GetColor();

	CColorsDlg dialog(colors, bActive,  m_pTextColorExpr, m_pBkgColorExpr, m_pLabelTextColorExpr, &(m_pDocument->m_ViewSymbolTable));
	if (dialog.DoModal() != IDOK) return;

	m_Label.SetTextColor  (colors[CColorsDlg::LABEL]);
	m_Value.SetTextColor  (colors[CColorsDlg::VALUE]);
	m_Value.SetBkgColor   (colors[CColorsDlg::BACKGROUND]);
	m_BorderPen.SetColor  (colors[CColorsDlg::BORDER]);

	UpdateDocument ();
}

//------------------------------------------------------------------------------
void FieldRect::SetFieldLabel()
{
	CString strLabel = m_Label.GetText(); 
/*	
	CEditTextExprDlg dialog(strLabel, m_pLabelTextExpr, &(m_pDocument->m_ViewSymbolTable)); 
	if (dialog.DoModal() == IDOK)
	{
		m_Label.SetText(strLabel);
		UpdateDocument ();
	}
*/
	CRSEditView* pEditView = m_pDocument->GetWoormFrame()->CreateEditView();
	if (!pEditView)
		return;
	BOOL changed = FALSE;
	pEditView->EnableStringPreview();
	pEditView->LoadElement(&strLabel, &changed);
	pEditView->DoEvent();

	if (changed)
	{
		m_Label.SetText(strLabel);
		UpdateDocument();

		m_pDocument->RefreshObjectPropertyView();
	}
}

//------------------------------------------------------------------------------
void	FieldRect::SetStyle		(BaseRect* pTpl)
{
	RemoveStyle ();
	if (!pTpl) return;
	__super::SetStyle (pTpl);
	//---- ----

	if (m_Value.m_rgbBkgColor == DEFAULT_BKGCOLOR)
		m_Value.m_rgbBkgColor = ((FieldRect*)m_pDefault)->m_Value.m_rgbBkgColor;

	if (m_Value.m_rgbTextColor == DEFAULT_TEXTCOLOR)
		m_Value.m_rgbTextColor = ((FieldRect*)m_pDefault)->m_Value.m_rgbTextColor;

	if (m_Value.m_nAlign == DEFAULT_FIELD_ALIGN)
	{
		m_Value.m_nAlign = ((FieldRect*)m_pDefault)->m_Value.m_nAlign;
	}

	SetTemplateFont (m_pDocument, m_Value.m_nFontIdx, ((FieldRect*)m_pDefault)->m_Value.m_nFontIdx, FNT_TEXT);

	if (m_Label.m_rgbTextColor == DEFAULT_TEXTCOLOR)
		m_Label.m_rgbTextColor = ((FieldRect*)m_pDefault)->m_Label.m_rgbTextColor;

	if (m_Label.m_nAlign == DEFAULT_LABEL_ALIGN)
		m_Label.m_nAlign = ((FieldRect*)m_pDefault)->m_Label.m_nAlign;

	SetTemplateFont (m_pDocument, m_Label.m_nFontIdx, ((FieldRect*)m_pDefault)->m_Label.m_nFontIdx, FNT_LABEL);
}

//------------------------------------------------------------------------------
void	FieldRect::RemoveStyle	()
{
	if (!m_pDefault) return;
	//---- ----

	if (m_Value.m_rgbBkgColor == ((FieldRect*)m_pDefault)->m_Value.m_rgbBkgColor)
		m_Value.m_rgbBkgColor = DEFAULT_BKGCOLOR;

	if (m_Value.m_rgbTextColor == ((FieldRect*)m_pDefault)->m_Value.m_rgbTextColor)
		m_Value.m_rgbTextColor = DEFAULT_TEXTCOLOR;

	if (m_Value.m_nAlign == ((FieldRect*)m_pDefault)->m_Value.m_nAlign) 
	{
		m_Value.m_nAlign = DEFAULT_FIELD_ALIGN;
	}

	RemoveTemplateFont(m_pDocument, m_Value.m_nFontIdx, ((FieldRect*)m_pDefault)->m_Value.m_nFontIdx, FNT_TEXT);

	if (m_Label.m_rgbTextColor == ((FieldRect*)m_pDefault)->m_Label.m_rgbTextColor)
		m_Label.m_rgbTextColor = DEFAULT_TEXTCOLOR;

	if (m_Label.m_nAlign == ((FieldRect*)m_pDefault)->m_Label.m_nAlign)
		m_Label.m_nAlign = DEFAULT_LABEL_ALIGN;

	RemoveTemplateFont(m_pDocument, m_Label.m_nFontIdx, ((FieldRect*)m_pDefault)->m_Label.m_nFontIdx, FNT_LABEL);

	//---- ----
	__super::RemoveStyle	();
}

//------------------------------------------------------------------------------
void	FieldRect::ClearStyle	()
{
	m_Value.m_rgbBkgColor	= m_pDefault ?  ((FieldRect*)m_pDefault)->m_Value.m_rgbBkgColor		: DEFAULT_BKGCOLOR;
	m_Value.m_rgbTextColor	= m_pDefault ?  ((FieldRect*)m_pDefault)->m_Value.m_rgbTextColor	: DEFAULT_TEXTCOLOR;
	m_Value.m_nAlign		= m_pDefault ?  ((FieldRect*)m_pDefault)->m_Value.m_nAlign			: DEFAULT_FIELD_ALIGN;
	
	ClearTemplateFont (m_pDocument, m_Value.m_nFontIdx, m_pDefault ? &(((FieldRect*)m_pDefault)->m_Value.m_nFontIdx) : NULL, FNT_TEXT);

	m_Label.m_rgbTextColor	= m_pDefault ?  ((FieldRect*)m_pDefault)->m_Label.m_rgbTextColor	: DEFAULT_TEXTCOLOR;
	m_Label.m_nAlign		= m_pDefault ?  ((FieldRect*)m_pDefault)->m_Label.m_nAlign			: DEFAULT_LABEL_ALIGN;
	
	ClearTemplateFont (m_pDocument, m_Label.m_nFontIdx, m_pDefault ? &(((FieldRect*)m_pDefault)->m_Label.m_nFontIdx) : NULL, FNT_LABEL);

	//---- ----
	__super::ClearStyle	();
}

//------------------------------------------------------------------------------
BOOL	FieldRect::IsNotDefaultBkgColor() const
{
	if (m_pDefault)
		return m_Value.m_rgbBkgColor != ((FieldRect*)m_pDefault)->m_Value.m_rgbBkgColor;

	return m_Value.m_rgbBkgColor != DEFAULT_BKGCOLOR;
}

BOOL	FieldRect::IsNotDefaultTextColor() const
{
	if (m_pDefault)
		return m_Value.m_rgbTextColor != ((FieldRect*)m_pDefault)->m_Value.m_rgbTextColor;

	return m_Value.m_rgbTextColor != DEFAULT_TEXTCOLOR;
}

BOOL	FieldRect::IsNotDefaultAlignment() const
{
	if (m_pDefault)
		return (m_Value.m_nAlign != ((FieldRect*)m_pDefault)->m_Value.m_nAlign);

	return (m_Value.m_nAlign != DEFAULT_FIELD_ALIGN);
}

BOOL	FieldRect::IsNotDefaultFontStyle() const
{
	if (m_pDefault)
		return m_Value.m_nFontIdx != ((FieldRect*)m_pDefault)->m_Value.m_nFontIdx;

	return m_Value.m_nFontIdx != m_pDocument->m_pFontStyles->GetFontIdx(FNT_TEXT);
}

BOOL	FieldRect::IsNotDefaultLabelTextColor() const
{
	if (m_pDefault)
		return m_Label.m_rgbTextColor != ((FieldRect*)m_pDefault)->m_Label.m_rgbTextColor;

	return m_Label.m_rgbTextColor != DEFAULT_TEXTCOLOR;
}

BOOL	FieldRect::IsNotDefaultLabelAlignment() const
{
	if (m_pDefault)
		return m_Label.m_nAlign != ((FieldRect*)m_pDefault)->m_Label.m_nAlign;

	return m_Label.m_nAlign != DEFAULT_LABEL_ALIGN;
}

BOOL	FieldRect::IsNotDefaultLabelFontStyle() const
{
	if (m_pDefault)
		return m_Label.m_nFontIdx != ((FieldRect*)m_pDefault)->m_Label.m_nFontIdx;

	return m_Label.m_nFontIdx != m_pDocument->m_pFontStyles->GetFontIdx(FNT_LABEL);
}

//------------------------------------------------------------------------------
void FieldRect::UnparseAuxProp (ViewUnparser& ofile)
{
	BOOL bCol    = m_pTextColorExpr != NULL || IsNotDefaultTextColor();
	BOOL bBkg    = m_pBkgColorExpr != NULL || IsNotDefaultBkgColor();
	BOOL bAlg    = IsNotDefaultAlignment();
	BOOL bFnt    = IsNotDefaultFontStyle();

	BOOL bLab    = m_pLabelTextExpr != NULL || !m_Label.GetText().IsEmpty();
	BOOL bLcol   = bLab && (m_pLabelTextColorExpr != NULL || IsNotDefaultLabelTextColor());
	BOOL bLalg   = bLab && IsNotDefaultLabelAlignment(); 
	BOOL bLfnt   = bLab && IsNotDefaultLabelFontStyle();
		
	BOOL bLblk   = (bLcol + bLalg + bLfnt) > 0;

	if (m_ShowAs == FT_IMAGE)
	{
		ASSERT_VALID(m_pBitmap);
		ofile.UnparseTag (T_BITMAP);

		if (m_pBitmap->IsBestFitSetted())
		{
			ofile.UnparseTag (T_PROPORTIONAL, FALSE);
		}
		else if (m_pBitmap->m_ImageFitMode == CTBPicture::ImageFitMode::NORMAL)
		{
			ofile.UnparseTag(T_NATIVE, FALSE);
		}

		if (m_bIsCutted)
		{
			ofile.UnparseRect (m_rectCutted, FALSE);
		}
	}
	else if (m_ShowAs == FT_BARCODE)  
		ofile.UnparseBarCode(m_pBarCode, TRUE);
	else if (m_ShowAs == FT_TEXTFILE)  
		ofile.UnparseTag(T_FILE);
	else if (m_ShowAs == FT_URL)   
		ofile.UnparseTag(T_LINKURL);
	
	if (IsEmailParameter())
	{
		ofile.UnparseTag (T_MAIL, FALSE);
		ofile.UnparseBlank ();
		EmailParameter em = GetEmailParameter();
		ofile.UnparseInt ((int)em, FALSE);
		if (
				m_eEmailParameter == EP_Body || m_eEmailParameter == EP_Subject ||
				m_eEmailParameter == EP_To || m_eEmailParameter == EP_Cc || m_eEmailParameter == EP_Bcc ||
				m_eEmailParameter == EP_Attachment || m_eEmailParameter == EP_To_by_Certified
			)
		{
			ofile.UnparseTag (T_COMMA, FALSE);
			ofile.UnparseBool (m_bAppendMailPart, FALSE);
		}
		ofile.UnparseBlank();
	}

	if (bCol) 
	{
		if (m_pTextColorExpr && !m_pTextColorExpr->IsEmpty())
		{
			UnparseDynamicColor		(ofile,  T_TEXTCOLOR, m_pTextColorExpr);
		}
		else //if (IsNotDefaultTextColor())
		{
			ofile.UnparseColor    (T_TEXTCOLOR, m_Value.m_rgbTextColor, FALSE);
			ofile.UnparseSep      (TRUE);
		}
	}
	if (bBkg) 
	{
		if (m_pBkgColorExpr && !m_pBkgColorExpr->IsEmpty())
			UnparseDynamicColor		(ofile,  T_BKGCOLOR, m_pBkgColorExpr);
		else //if (IsNotDefaultBkgColor())
		{
			ofile.UnparseColor    (T_BKGCOLOR, m_Value.m_rgbBkgColor, FALSE);
			ofile.UnparseSep      (TRUE);
		}
	}

	if (bAlg)	
		ofile.UnparseAlign  (m_Value.m_nAlign);

	if (bFnt)   
		ofile.UnparseFont           (m_pDocument->m_pFontStyles, m_Value.m_nFontIdx);

	if (m_bMiniHtml)
		ofile.UnparseTag(T_HTML, TRUE);

	if (bLab)
	{
		ofile.UnparseTag (T_LABEL, FALSE);
		if (m_pLabelTextExpr != NULL && ! m_pDocument->m_Template.m_bIsSavingTemplate) 
		{
			ofile.UnparseTag (T_ASSIGN, FALSE);

			ofile.UnparseExpr(m_pLabelTextExpr->ToString(), FALSE);
			ofile.UnparseTag (T_SEP, TRUE);
		}
		else
			ofile.UnparseString (ofile.IsLocalizableTextInCurrentLanguage() ? ofile.LoadReportString(m_Label.GetText()) : m_Label.GetText(),   TRUE);
	}

	if (bLblk)	
		ofile.UnparseBegin ();

	if (bLcol)	
	{
		if (m_pLabelTextColorExpr && !m_pLabelTextColorExpr->IsEmpty())
			UnparseDynamicColor		(ofile,  T_TEXTCOLOR, m_pLabelTextColorExpr);
		else //if (IsNotDefaultLabelTextColor())
		{
			ofile.UnparseColor    (T_TEXTCOLOR, m_Label.m_rgbTextColor, FALSE);
			ofile.UnparseSep      (TRUE);
		}
	}

	if (bLalg)	ofile.UnparseAlign	(m_Label.m_nAlign);

	if (bLfnt)	ofile.UnparseFont	(m_pDocument->m_pFontStyles, m_Label.m_nFontIdx);

	if (bLblk)	
		ofile.UnparseEnd ();
}

//------------------------------------------------------------------------------
void FieldRect::Unparse (ViewUnparser& ofile)
{
	if (GetInternalID() >= SpecialReportField::REPORT_LOWER_SPECIAL_ID )
	{
		return;
	}

	//---- Template Override
	BaseRect* pDefault = m_pDefault;
	if (m_bTemplate && m_pDocument->m_Template.m_bIsSavingTemplate)
	{
		m_pDefault = NULL;
	}
	//----

	ofile.UnparseRect   (T_FIELD, m_BaseRect,               		FALSE);

	if (IsNotDefaultRatio())
		ofile.UnparseRatio (m_nHRatio, m_nVRatio,					FALSE);

	ofile.UnparseAlias (m_wInternalID,                		        FALSE);
	ofile.Write (_T(" /* ") + GetFieldName() +  _T(" */ "),			FALSE);

	if (m_pFormatExpr && !m_pDocument->m_Template.m_bIsSavingTemplate)
	{
		ofile.UnparseTag(T_FORMATSTYLE, FALSE);
		ofile.UnparseTag(T_ASSIGN, FALSE);

		ofile.UnparseExpr(m_pFormatExpr->ToString(), FALSE);
		ofile.UnparseSep(FALSE);
	}
	else
		ofile.UnparseFormat (m_pDocument->m_pFormatStyles, m_nFormatIdx,FALSE);

	UnparseProp (ofile);
	
	//----
	m_pDefault = pDefault;
}

//------------------------------------------------------------------------------
BOOL FieldRect::GetSchema (CXSDGenerator* pSchema, WoormTable* pRst)
{
	WoormField* pField = pRst->GetFieldByID(GetInternalID());
	if (!pField)
	{
		ASSERT(pField);
		return FALSE;
	}
	if (pField->GetSkipXml())
		return TRUE;

	CString strType = pField->GetData()->GetXMLType(FALSE);

	pSchema->InsertElement(pField->GetXmlName(), strType, _T("1"), _T("1")); 

	return TRUE;
}

//------------------------------------------------------------------------------
BOOL FieldRect::ParseLabelBlock(ViewParser& lex)
{
	if (lex.LookAhead(T_BEGIN))
		return
			lex.ParseBegin  ()      &&
			ParseLabelProps (lex)   &&
			lex.ParseEnd    ();

	return ParseLabelProp(lex, FALSE);
}

//------------------------------------------------------------------------------
BOOL FieldRect::ParseLabelProps (ViewParser& lex)
{
	BOOL ok = TRUE;

	do { ok = ParseLabelProp(lex) && !lex.Bad() && !lex.Eof(); }
	while (ok && !lex.LookAhead(T_END));

	return ok;
}

//------------------------------------------------------------------------------
BOOL FieldRect::ParseLabelProp (ViewParser& lex, BOOL bBlock)
{
	BOOL ok = TRUE;

	switch (lex.LookAhead())
	{
		case T_TEXTCOLOR    : 
			ok = ParseDynamicColor (lex, m_Label.m_rgbTextColor, T_TEXTCOLOR, m_pLabelTextColorExpr); 	
			break;

		case T_ALIGN        : ok = lex.ParseAlign       (m_Label.m_nAlign);			break;
		case T_FONTSTYLE    :
				ok = lex.ParseFont (m_pDocument->m_pFontStyles, m_Label.m_nFontIdx);
				break;

		case T_END : return ok;
		default :
			if (bBlock)
			{
				lex.SetError(_TB("Wrong option for the rectangle generic item."));
				ok = FALSE;
			}
	}
	return ok;
}

//------------------------------------------------------------------------------
BOOL FieldRect::ParseLabel(ViewParser& lex)
{
	CString strText;
	BOOL ok = lex.ParseTag    (T_LABEL);
	if (!ok) return FALSE;
	if (lex.Matched(T_ASSIGN))
	{
		WoormTable* pST = &(m_pDocument->m_ViewSymbolTable);

		m_pLabelTextExpr = new Expression(pST);
		m_pLabelTextExpr->SetStopTokens(T_SEP);
		if (!m_pLabelTextExpr->Parse(lex, DataType::String, TRUE))
		{
			lex.SetError(_TB("Parser error in field label expression:") + _T("\r\n")/* + sTitle*/);
			return FALSE;
		}
		if (!lex.ParseTag (T_SEP))
			return FALSE;
	}
	else
		ok = lex.ParseCEdit (strText);
			
	ok = ok && ParseLabelBlock (lex);
	
	m_Label.SetText(strText);
	return ok;
}

//------------------------------------------------------------------------------
BOOL FieldRect::ParseBitmap(ViewParser& lex)
{
	BOOL ok = lex.SkipToken();

	m_pBitmap = new CTBPicture();

	if (lex.Matched(T_PROPORTIONAL) )
	{
		m_pBitmap->m_ImageFitMode = CTBPicture::ImageFitMode::BEST;
	}
	else if (lex.Matched(T_NATIVE))
	{
		m_pBitmap->m_ImageFitMode = CTBPicture::ImageFitMode::NORMAL;
	}

	if (lex.LookAhead(T_RECT) )
	{
		ok = lex.ParseRect (m_rectCutted);
		if ( ok )
			m_bIsCutted = TRUE;
	}

	return ok;
}

//------------------------------------------------------------------------------
void FieldRect::SetBarCode(DWORD eBarCodeType, BOOL bVertical /*= FALSE*/)
{
	SAFE_DELETE(m_pBarCode);
	m_pBarCode = new CBarCode();  
	m_pBarCode->m_nBarCodeType = CBarCodeTypes::BarCodeType(eBarCodeType);
	if (bVertical)
		m_pBarCode->m_bVertical = TRUE;
}

//------------------------------------------------------------------------------
BOOL FieldRect::ParseBarCode(ViewParser& lex)
{
	m_pBarCode = new CBarCode();  

	return lex.ParseBarCode(m_pBarCode);
}

//------------------------------------------------------------------------------
BOOL FieldRect::OnParseProp (ViewParser& lex, BOOL bBlock)
{
	BOOL ok = TRUE;

	switch (lex.LookAhead())
	{
		case T_TEXTCOLOR    : ok = ParseTextColor (lex, m_Value.m_rgbTextColor);	break;
		case T_BKGCOLOR     : ok = ParseBkgColor (lex, m_Value.m_rgbBkgColor);		break;

		case T_ALIGN        : 
							{
								ok = lex.ParseExtendedAlign (m_Value.m_nAlign);	
								if (
										(m_Value.m_nAlign & DT_VCENTER) &&
										m_pDocument->m_pWoormIni &&
										m_pDocument->m_pWoormIni->m_bForceVerticalAlignLabelRelative
									)
									m_Value.m_nAlign |= DT_EX_VCENTER_LABEL;
								break;
							}
		case T_LABEL        : ok = ParseLabel (lex); break;

		case T_BITMAP: ok = ParseBitmap(lex); if (ok) m_ShowAs = FT_IMAGE; break;

		case T_BARCODE_STRIP: ok = ParseBarCode(lex); if (ok) m_ShowAs = FT_BARCODE; break;

		case T_FILE: lex.SkipToken(); m_ShowAs = FT_TEXTFILE; break;

		case T_LINKURL: lex.SkipToken(); m_ShowAs = FT_URL; break;

		case T_FONTSTYLE    :
				ok = lex.ParseFont  (m_pDocument->m_pFontStyles, m_Value.m_nFontIdx);
				break;

		case T_MAIL			: 
			{
				lex.SkipToken();
				int i;
				ok = lex.ParseInt(i);
				if (ok && i > EP_None && i <= EP_Last)
				{
					m_eEmailParameter = (EmailParameter) i;	

					if (lex.Matched(T_COMMA))
					{
						BOOL bAppend = TRUE;
						ok = lex.ParseBool(bAppend);
						m_bAppendMailPart = bAppend;
					}
				}
				else
				{
					m_eEmailParameter = EP_None;
					m_bAppendMailPart = FALSE;
				}
				break;
			}

		case T_HTML:		  ok = lex.SkipToken();		m_bMiniHtml = TRUE;			break;

		default :
			if (bBlock)
			{
				lex.SetError(_TB("The specified field option is wrong."));
				ok = FALSE;
			}
	}
	return ok;
}

//------------------------------------------------------------------------------
BOOL FieldRect::Parse (ViewParser& lex)
{
	BOOL ok =
		lex.ParseRect   (T_FIELD, m_BaseRect) &&
		lex.ParseRatio	(m_nHRatio, m_nVRatio) &&
		lex.ParseAlias  (m_wInternalID);
	
	if (ok && lex.Matched(T_FORMATSTYLE))
	{
		if (lex.Matched(T_ASSIGN))
		{
			m_pFormatExpr = new Expression(&m_pDocument->m_ViewSymbolTable);
			Token arStopToken[] = 
			{ 
				T_SEP
			};
			m_pFormatExpr->SetStopTokens(arStopToken, sizeof(arStopToken) / sizeof(Token));
			if (!m_pFormatExpr->Parse(lex, DataType::String, TRUE))
			{
				lex.SetError(_TB("Parser error in column conditional formatter style expression:") + _T("\r\n")/* + sTitle*/);
				return FALSE;
			}
			ok = lex.ParseSep();
		}
		else if (!lex.ParseFormat (m_pDocument->m_pFormatStyles, m_nFormatIdx, FALSE))
			return FALSE;
	}

	if (m_nFormatIdx == NO_FORMAT || m_pFormatExpr)
	{
		SymField* pF = m_pDocument->m_ViewSymbolTable.GetFieldByID(m_wInternalID);
		if (pF)
			SetFormatIdx(m_pDocument->m_pFormatStyles->GetFormatIdx(pF->GetDataType()));
		else
			SetFormatIdx(0);
	}

	ok = ok && ParseBlock (lex);

	if (ok)	
		m_pDocument->m_pEditorManager->SetLastId(m_wInternalID);
	
	return ok;
}

//------------------------------------------------------------------------------
void FieldRect::ToggleBitmap ()
{
	if (m_ShowAs == EFieldShowAs::FT_IMAGE)
	{
		SAFE_DELETE(m_pBitmap);

		m_ShowAs = EFieldShowAs::FT_NORMAL;
	}
	else
	{ 
		SAFE_DELETE(m_pBarCode);

		m_pBitmap = new CTBPicture();

		m_ShowAs = EFieldShowAs::FT_IMAGE;
	}

	UpdateDocument();
}

//------------------------------------------------------------------------------
void FieldRect::ToggleBarCode ()
{
	if (m_ShowAs == EFieldShowAs::FT_BARCODE)
	{
		SAFE_DELETE(m_pBarCode);

		m_ShowAs = EFieldShowAs::FT_NORMAL;
	}
	else
	{ 
		SAFE_DELETE (m_pBitmap);

		m_pBarCode = new CBarCode ();

		m_ShowAs = EFieldShowAs::FT_BARCODE;
	}

	UpdateDocument ();
}

// m_bTextFile is alternative to m_pBitmap
//
//------------------------------------------------------------------------------
void FieldRect::ToggleTextFile ()
{
	if (m_ShowAs == EFieldShowAs::FT_TEXTFILE)
	{
		m_ShowAs = EFieldShowAs::FT_NORMAL;
	}
	else
	{
		SAFE_DELETE(m_pBitmap);
		SAFE_DELETE(m_pBarCode);

		m_ShowAs = EFieldShowAs::FT_TEXTFILE;

		m_Label.SetText(_T(""));
		m_Value.SetAlign(DEFAULT_TEXT_ALIGN);
	}
	
	UpdateDocument ();
}

//------------------------------------------------------------------------------
/*
void FieldRect::ToggleLink ()
{
	WORD InternalID = this->GetInternalID();
	CString fieldName = this->m_Label.GetLocalizedText(m_pDocument);
	
	CManageLinkDlg dialog (fieldName, m_pDocument, InternalID);
	
	dialog.DoModal();
}
*/
//------------------------------------------------------------------------------
void FieldRect::ToggleAppendRecipients	()
{
	m_bAppendMailPart = !m_bAppendMailPart;
}

//------------------------------------------------------------------------------
void FieldRect::ToggleEmailParameter	(UINT nID)
{
	int ep = nID - ID_TOGGLE_FROM + 1;
	if (ep < EP_None || ep > EP_Last)
	{
		ep = EP_None;
	}
	m_bAppendMailPart = FALSE;

	if (m_eEmailParameter == (FieldRect::EmailParameter)(ep))
	{
		m_eEmailParameter = EP_None;
	}
	else
	{
		m_eEmailParameter = (FieldRect::EmailParameter)(ep);

		if (m_eEmailParameter != EP_None)
		{	//viene mantenuto solo l'ultimo selezionato
			for (int i = 0; i < m_pDocument->GetObjects().GetCount(); i++)
			{
				BaseObj* pObj = (BaseObj*) (m_pDocument->GetObjects().GetAt(i));
				if (!pObj->IsKindOf(RUNTIME_CLASS(FieldRect)))
					continue;
				if (pObj == this) continue;
				
				FieldRect* pF = (FieldRect*) pObj;
				if (pF->m_eEmailParameter == (FieldRect::EmailParameter)(ep))
				{
					m_pDocument->InvalidateRect(pF->m_BaseRect);
					pF->m_eEmailParameter = EP_None;
				}
			}
		}
	}

	UpdateDocument ();
}

//------------------------------------------------------------------------------
void FieldRect::OnBarCodeAttrs ()
{
	ASSERT(m_ShowAs == FT_BARCODE);
	m_ShowAs = FT_BARCODE;

	if (!m_pBarCode)
	{
		SAFE_DELETE(m_pBitmap);

		m_pBarCode = new CBarCode ();
	}

	CBarCodeAttrsDlg dialog(m_pBarCode, &m_pDocument->m_ViewSymbolTable);

	if ((dialog.DoModal() != IDOK)) 
		return;

	UpdateDocument ();
}

//------------------------------------------------------------------------------
void FieldRect::VKReturn()
{
	SetFieldLabel ();
}

//------------------------------------------------------------------------------
void FieldRect::OnUpdateProportionalUI (CCmdUI* pCmdUI)
{
	pCmdUI->SetCheck( m_pBitmap->IsBestFitSetted());
}

//------------------------------------------------------------------------------
void FieldRect::OnUpdateCutUI (CCmdUI* pCmdUI)
{
	pCmdUI->SetCheck( m_bIsCutted );
}

//------------------------------------------------------------------------------
void FieldRect::OnResizeProportional ()
{
	m_pDocument->InvalidateRect(m_BaseRect);

	CTBPicture::ImageFitMode prevFitMode = m_pBitmap->m_ImageFitMode;
	m_pBitmap->m_ImageFitMode = CTBPicture::ImageFitMode::BEST;

	RectForDrawBitmap(NULL, m_BaseRect, m_rectCutted, m_pBitmap, m_bIsCutted);

	m_pBitmap->m_ImageFitMode = prevFitMode;

	Invalidate(TRUE);
}

//------------------------------------------------------------------------------
void FieldRect::OnCutBitmap ()
{
	CRect rectCutted = m_rectCutted;
	BOOL bIsCutted = m_bIsCutted;

	CString strName (m_Value.GetText());
		
	if ( strName.IsEmpty() ) return;
	
	if (GetExtension(strName).IsEmpty())
		strName += ".BMP";

	CCutBitmapDlg dlg
		( 
			this, strName,
			rectCutted, bIsCutted
		);

	if (dlg.DoModal() == IDOK)
	{
		if (bIsCutted && rectCutted.Height() && rectCutted.Width())
		{
			m_rectCutted = rectCutted;
			m_bIsCutted = bIsCutted;
		}
		else m_bIsCutted = FALSE;
	}
	
	Invalidate(TRUE);
}

//------------------------------------------------------------------------------
void FieldRect::OnCancelCut ()
{
	m_bIsCutted = FALSE;
	
	Invalidate(TRUE);
}

//------------------------------------------------------------------------------
void FieldRect::RefreshStandardSize()
{
	if (m_pBitmap  && m_pBitmap->ImageIsValid())
	{
		if (m_bIsCutted)
		{
			m_BaseRect.right = m_BaseRect.left + abs(m_rectCutted.Width());
			m_BaseRect.bottom = m_BaseRect.top + abs(m_rectCutted.Height());
		}
		else
		{
			m_BaseRect.right = m_BaseRect.left + abs(m_pBitmap->GetWidth());
			m_BaseRect.bottom = m_BaseRect.top + abs(m_pBitmap->GetHeight());
		}
		Invalidate(TRUE);
	}
}

//--------------------------------------------------------------------------------
void FieldRect::Invalidate(BOOL bCurrentSize /*=FALSE*/ )
{
	// refresh image based on streching mode
	if (!bCurrentSize)
	{
		// send WM_PAINT for Draw to all client area
		ConstrainBaseRect();

		m_pDocument->Invalidate(FALSE);
	}

	if (m_ShowAs == FT_URL && m_ctrlWeb.m_hWnd)
		m_ctrlWeb.Refresh();

	UpdateDocument ();
}

//--------------------------------------------------------------------------------
void FieldRect::ConstrainBaseRect()
{
	int nPenTop     = m_Borders.top   ? m_BorderPen.GetWidth() : 0;
	int nPenBottom  = m_Borders.bottom? m_BorderPen.GetWidth() : 0;
	int nPenLeft    = m_Borders.left  ? m_BorderPen.GetWidth() : 0;
	int nPenRight   = m_Borders.right ? m_BorderPen.GetWidth() : 0;

	CPoint ptPoint (m_pBitmap->GetWidth(), m_pBitmap->GetHeight());

	m_BaseRect.bottom = m_BaseRect.top + ptPoint.y + nPenTop + nPenBottom;
	m_BaseRect.right  = m_BaseRect.left+ ptPoint.x + nPenLeft + nPenRight;
}

//---------------------------------------------------------------------------
void FieldRect::CopyAttributes	(CObject* pSource)
{
	BaseRect::CopyAttributes (pSource);

	if (pSource->IsKindOf(RUNTIME_CLASS(BaseRect)) )
	{
		if (pSource->IsKindOf(RUNTIME_CLASS(TextRect)))
		{
			TextRect* pSrc = (TextRect*) pSource;
			//-----
			this->m_Value.m_nFontIdx		=	pSrc->m_StaticText.m_nFontIdx;
			this->m_Value.m_rgbBkgColor		=	pSrc->m_StaticText.m_rgbBkgColor;
			this->m_Value.m_rgbTextColor	=	pSrc->m_StaticText.m_rgbTextColor;
			this->m_Value.m_nAlign			=	pSrc->m_StaticText.m_nAlign;
		}
		else if (pSource->IsKindOf(RUNTIME_CLASS(FieldRect)))
		{
			FieldRect* pSrc = (FieldRect*) pSource;
			//-----
			this->m_sStyleClass	= pSrc->m_sStyleClass;

			this->m_Value.m_nFontIdx		=	pSrc->m_Value.m_nFontIdx;
			this->m_Value.m_rgbBkgColor		=	pSrc->m_Value.m_rgbBkgColor;
			this->m_Value.m_rgbTextColor	=	pSrc->m_Value.m_rgbTextColor;
			this->m_Value.m_nAlign			=	pSrc->m_Value.m_nAlign;

			this->m_Label.m_nFontIdx		=	pSrc->m_Label.m_nFontIdx;
			this->m_Label.m_rgbBkgColor		=	pSrc->m_Label.m_rgbBkgColor;
			this->m_Label.m_rgbTextColor	=	pSrc->m_Label.m_rgbTextColor;
			this->m_Label.m_nAlign			=	pSrc->m_Label.m_nAlign;

			if (this->GetDataType() == pSrc->GetDataType())
				this->SetFormatIdx(pSrc->GetFormatIdx());
		}
	}	
	else if (pSource->IsKindOf(RUNTIME_CLASS(TableColumn)))
	{
		TableColumn* pSrc= (TableColumn*) pSource;
		//-----
		this->m_Value.m_nFontIdx		=	pSrc->GetColumnFontIdx();
		this->m_Value.m_rgbBkgColor		=	pSrc->m_Cells[0]->m_Value.GetBkgColor();
		this->m_Value.m_rgbTextColor	=	pSrc->m_Cells[0]->m_Value.GetTextColor();
		this->m_Value.m_nAlign			=	pSrc->m_nAlignType;

		if (pSrc->m_ColumnPen.m_rgbColor != RGB(255,255,255))
			this->m_BorderPen.m_rgbColor	=	pSrc->m_ColumnPen.m_rgbColor;

		if (this->GetDataType() == pSrc->GetDataType())
			this->SetFormatIdx(pSrc->GetColumnFormatIdx());
	}
}


//------------------------------------------------------------------------------
void FieldRect::MouseMove(CDC& cdc, CPoint point, CBCGPScrollView* pScrollView, BOOL bTransMode)
{
	//devo capire se sto cercando di muovere la tabella o spostando le colonne o ancora trasformandole colonne in fieldrect
	if (m_nAction == MOVE && bTransMode)
	{
		if (!m_bTransMode)
		{
			m_bTransMode = TRUE;
			m_pDocument->Invalidate(TRUE);
		}

		ClipCursor(NULL);
		
		m_pDocument->GetWoormView()->ManageDragToTransform(point);
		return;
	}

	m_bTransMode = FALSE;

	__super::MouseMove(cdc, point, pScrollView);
}

//==============================================================================
//          Class UrlRect implementation
//==============================================================================

//------------------------------------------------------------------------------
IMPLEMENT_DYNAMIC (UrlRect, SqrRect)

//------------------------------------------------------------------------------
BEGIN_MESSAGE_MAP(UrlRect, SqrRect)
	ON_COMMAND(ID_BMP_LOAD,        SetUrl)
END_MESSAGE_MAP()

//------------------------------------------------------------------------------
UrlRect::UrlRect(CPoint ptCurrPos, const CString& strUrl, CWoormDocMng* pDocument)
	:
	SqrRect 	(ptCurrPos, pDocument),
	m_strUrl	(strUrl)
{
	if ( m_BaseRect.left == m_BaseRect.right ) m_BaseRect.right += 3;
	if ( m_BaseRect.top == m_BaseRect.bottom ) m_BaseRect.bottom += 3;
}

//------------------------------------------------------------------------------
UrlRect::UrlRect(const UrlRect& source)
	:
	SqrRect 	(source),
	m_strUrl	(source.m_strUrl)
{
}

//---------------------------------------------------------------------------
void UrlRect::Draw (CDC& DC, BOOL bPreview)
{
	if (!PreDraw(&DC))
		return;

	if (m_ctrlWeb.m_hWnd == 0)
	{
		CRect rect (m_BaseRect);

		int ret = m_ctrlWeb.Create(rect, m_pDocument->GetWoormView(), m_pDocument->m_idcCounter++);
	}

	if (!m_strUrl.IsEmpty())
	{
		try
		{
			m_ctrlWeb.Navigate (m_strUrl);
		}
		catch(...)
		{
			ASSERT(FALSE);
		}
	}

	PostDraw(DC, bPreview, m_BaseRect);
}

//------------------------------------------------------------------------------
BOOL UrlRect::OnShowPopup (CMenu& menu)
{
	CMenu dataMenu;
	dataMenu.CreatePopupMenu();
	menu.AppendMenu(MF_STRING|MF_POPUP, (UINT)dataMenu.GetSafeHmenu(), MenuStrings::Url());
	if (!m_strUrl.IsEmpty())
	{
		dataMenu.AppendMenu(MF_STRING|MF_GRAYED, 0, m_strUrl);
	}
	dataMenu.AppendMenu(MF_STRING, ID_BMP_LOAD, MenuStrings::SetUrl());

	menu.AppendMenu(MF_SEPARATOR);

	return TRUE;
}

//------------------------------------------------------------------------------
void UrlRect::Invalidate(BOOL bCurrentSize)
{
	// refresh image based on streching mode
	if (!bCurrentSize)
	{
		// send WM_PAINT for Draw to all client area
		m_pDocument->Invalidate(FALSE);
	}

	m_ctrlWeb.Refresh();

	UpdateDocument ();
}

//------------------------------------------------------------------------------
void UrlRect::SetBorders()
{
	SqrRect::SetBorders();

	Invalidate();
}

//------------------------------------------------------------------------------
void UrlRect::SetUrl()
{
	//TODO input url and GET String
}

//------------------------------------------------------------------------------
void UrlRect::Unparse (ViewUnparser& ofile)
{
	//---- Template Override
	BaseRect* pDefault = m_pDefault;
	if (m_bTemplate && m_pDocument->m_Template.m_bIsSavingTemplate)
	{
		m_pDefault = NULL;
	}
	//----

	ofile.UnparseRect (T_LINKURL, m_BaseRect, FALSE);

	ofile.UnparseString(m_strUrl, FALSE);

	UnparseProp(ofile);
	
	//----
	m_pDefault = pDefault;
}

//------------------------------------------------------------------------------
BOOL UrlRect::Parse (ViewParser& lex)
{
	if (
			lex.ParseRect	(T_LINKURL, m_BaseRect) &&
			lex.ParseString (m_strUrl) && 
			ParseBlock		(lex)
		)
	{
		return TRUE;
	}

	return FALSE;
}


//==============================================================================
//          Class BarCodeFieldRect implementation
//==============================================================================

//------------------------------------------------------------------------------
IMPLEMENT_DYNAMIC (BarCodeFieldRect, FieldRect)

//------------------------------------------------------------------------------
BEGIN_MESSAGE_MAP(BarCodeFieldRect, FieldRect)
END_MESSAGE_MAP()

//------------------------------------------------------------------------------
BarCodeFieldRect::BarCodeFieldRect (CPoint ptCurrPos, CWoormDocMng* pDocument, WORD wID)
	:
	FieldRect		(ptCurrPos, pDocument, wID)
{
}

//------------------------------------------------------------------------------
BarCodeFieldRect::BarCodeFieldRect (const BarCodeFieldRect& source)
	:
	FieldRect			(source)
{
}

//------------------------------------------------------------------------------
BarCodeFieldRect::~BarCodeFieldRect ()
{
}
