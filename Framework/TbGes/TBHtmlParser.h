
//includere alla fine degli include del .H
#include "beginh.dex"

// codeproject source HTML Parser C++
// http://www.codeproject.com/Articles/663186/HTML-Parser-Cplusplus-Demo-Project

#include "HtmlParser\LiteHTMLReader.h"  
#include "HtmlParser\HtmlElementCollection.h"

enum HtmlAlign {
	NONEALIGN,
	LEFT,
	RIGHT,
	CENTER
};

enum HtmlOverflow {
	NONEOVERFLOW,
	VISIBLE,
	HIDDEN,
	SCROLL,
	AUTO
};



typedef struct
{
	BOOL		bFontSize;
	BOOL		bFontColor;
	BOOL		bBackground;
	BOOL		bBold;
	BOOL		bItalic;
	BOOL		bUnderline;
	BOOL		bStrikethrough;

	int			iFontSize;
	COLORREF	cText;
	COLORREF	cBackground;

	CRect		rectOrig;
	CRect		rectNext;

	CString		sFace;

	// List
	INT			iListLavel;
	CString		sListMarker;
	INT			iListCounter;

	BOOL			bDiv;
	INT				iDivWidth;
	HtmlAlign		iDivFloat;
	HtmlAlign		iDivTextAlign;
	HtmlOverflow	iOverflow;

	CString			sHref;

} stHtmlFont;

typedef struct
{
	CRect		rectNext;
	CRect		rectCalc;
} stHtmlCalc;

//-----------------------------------------------------------------------------
class TB_EXPORT TBHtmlParser
{
private:

	/*
	<font size=”n” color=”color”>: definizione del font e del colore,
	<a style=”color:#80BFFF”>: per settare solo il color,
	<a style="background:#80BFFF">: per il background,
	<B> : Bold,
	<I> : Italic,
	<u> : underline,
	<Br> : porta a capo forzatamente anche quado c’è spazio sufficiente per scrivere su un’unica linea
	*/

	const CString HtmlHash		= _T("#");
	const CString HtmlSemicolon = _T(";");
	const CString HtmlColon		= _T(":");

	const CString HtmlStartTag	= _T("<");
	const CString HtmlStopTag	= _T(">");

	const CString HtmlUnitPx	= _T("PX");

	const CString HtmlPosLeft	= _T("LEFT");
	const CString HtmlPosRight  = _T("RIGHT");
	const CString HtmlPosCenter = _T("CENTER");
	const CString HtmlVisible	= _T("VISIBLE");
	const CString HtmlHidden	= _T("HIDDEN");
	const CString HtmlScroll	= _T("SCROLL");
	const CString HtmlAuto		= _T("AUTO");
		
	// Tag Html
	const CString HtmlTagOpen	= _T("<HTML>");		// Html tag compone string open
	const CString HtmlTagClose  = _T("</HTML>");	// Html tag compone string close

	const CString HtmlTag		= _T("HTML");	// Html tag
	const CString HtmlTagFont	= _T("FONT");	// Select the Font
	const CString HtmlTagA		= _T("A");		// Attribute 
	const CString HtmlTagB		= _T("B");		// Bold
	const CString HtmlTagI		= _T("I");		// Italic
	const CString HtmlTagU		= _T("U");		// Underline
	const CString HtmlTagS		= _T("S");		// Strikethrough Element 
	const CString HtmlTagBR		= _T("BR");		// Line break
	
	const CString HtmlTagUL			= _T("UL");		// Unordered List
	const CString HtmlTagOL			= _T("OL");		// Ordered List
	const CString HtmlTagLI			= _T("LI");		// Each list
	const CString HtmlTagLIClose	= _T("/LI");	// Each list

	const CString HtmlTagDIV		= _T("DIV");	// DIV
	const CString HtmlTagSPAN		= _T("SPAN");	// DIV

	// list-style-type type
	const CString HtmlListStyleType  = _T("LIST-STYLE-TYPE");	// The Style Attribute
	const CString HtmlListType		 = _T("TYPE");				// The Type Attribute

	// List marker
	const CString HtmlListMarkedDisc				= _T("DISC");
	const CString HtmlListMarkedCircle				= _T("CIRCLE");
	const CString HtmlListMarkedSquare				= _T("SQUARE");
	const CString HtmlListMarkedNone				= _T("NONE");
	const CString HtmlListMarkedNumber				= _T("1");
	const CString HtmlListMarkedUppercaseLetters	= _T("A");
	const CString HtmlListMarkedLowercaseLetters	= _T("a");

	// Html Attribute
	const CString HtmlAttStyle	= _T("STYLE");	// Style
	const CString HtmlAttSize	= _T("SIZE");	// Size font
	const CString HtmlAttColor	= _T("COLOR");	// Color text
	const CString HtmlAttHref	= _T("HREF");	// URL - Specifies the URL of the page the link goes to

	// Html Attribute internal setting
	const CString HtmlAttInColor		= _T("COLOR");				// Text color
	const CString HtmlAttInBackground	= _T("BACKGROUND-COLOR");	// Background color
	const CString HtmlAttInFace			= _T("FACE");				// <font face='verdana'>This is some text!</font>
	const CString HtmlAttInWidth		= _T("WIDTH");				// DIV: width
	const CString HtmlAttInFloat		= _T("FLOAT");				// DIV: float 
	const CString HtmlAttInTextAlign	= _T("TEXT-ALIGN");			// DIV: text - align
	const CString HtmlAttInOverflow		= _T("OVERFLOW");			// DIV: overflow

	// URL map
	CMap<CString, LPCTSTR, CRect, CRect&> m_mapURL;

	BOOL m_bPreview;

public:
		TBHtmlParser();
		virtual ~TBHtmlParser();
		BOOL HTMLDcStringSplitter(CDC* pDc, CRect rect, CString stHTML, CStringArray* pStrArrayHTML);
		void HTMLDcRender(CDC* pDc, CRect rect, CString stHTML, BOOL bPreview);
		CString	GetHref(CRect inRect);

private:
		void HtmlToArray(CString stHtml, CStringArray* pStrArraySplit, CUIntArray* pUiArray);
		void HTMLSplitter(CString stHtml, CStringArray* pStrArrayHTML);
		void SplitHtmlText(CString stHtml, UINT nCar, CString* split1, CString* split2);
		CString HtmlCorrection(CString inHTML);

		HtmlTree GetHtmlhTree(CString tHTML);

		CRect RenderText(HtmlNode node, CDC* pDc, CRect rect, stHtmlFont stFont, BOOL bCalc = FALSE);
		stHtmlCalc RenderTextRecursive(HtmlNode node, CDC* pDc, CRect rect, stHtmlFont stFont, BOOL bCalc);
				
		CString GetAttributeValue(CString stTag, CString StAtt);
		CString GetSubAttributeValue(CString stTag, CString StAtt);

		CString GetTag(CString stTag);
		COLORREF ConvertHexColorToColorRef(CString val);
		BOOL ConvertColorToColorRef(CString sFontColor, COLORREF& colorRef);

		stHtmlFont HtmlFontInit(CDC* pDc);

		HtmlAlign		GetAlign(CString val);
		HtmlOverflow	GetOverflow(CString val);
};

//-----------------------------------------------------------------------------
#include "endh.dex"
