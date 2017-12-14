#pragma once

//#include <TbGeneric\DataObj.h>
#include <TbGenlib\parsctrl.h>
#include <TbGenlib\parscbx.h>
//per l'editor di expression
#include <TbWoormEngine\edtcmm.h>

// #include "common.h"
#include "textobj.h"
#include "colorcbx.h"
#include "BordPen.h"


//includere alla fine degli include del .H
#include "beginh.dex"


//===========================================================================
class ViewParser;
class ViewUnparser;
class Options;
class FontStyleTable;
class CWoormDocMng;
class CColumnArray;
class CLayout;
class Table;
class FieldRect;
class BaseRect;
class WoormTable;
class TableColumn;


//===========================================================================
class TB_EXPORT Options
{
	friend class COptionsDlg;
	friend class CRS_ObjectPropertyView;
	friend class CRSPageProp;
public:
	// nome del bitmap di sfondo
	CString		m_strBkgnBitmap;
	CPoint		m_BitmapOrigin;

	// default printer flags & info
	DataStr		m_strDefaultPrinter;

protected:
	// default printer flags & info
	DataBool m_bDefaultPrnNoBitmap;
	DataBool m_bDefaultPrnNoBorders;
	DataBool m_bDefaultPrnShowLabels;
	DataBool m_bDefaultPrnShowTitles;
	DataBool m_bDefaultPrnUseDraftFont;

	// printer flags
	BOOL	m_bPrnNoBitmap;
	BOOL	m_bPrnNoBorders;
	BOOL	m_bPrnShowLabels;
	BOOL	m_bPrnShowTitles;
	BOOL	m_bPrnUseDraftFont;

	/// console (video) flags
	BOOL	m_bConNoBitmap;
	BOOL	m_bConNoBorders;
	BOOL	m_bConShowLabels;
	BOOL	m_bConShowTitles;

public:
	BOOL	m_bCreateXsd1;
	BOOL	m_bCreateXsd2;

private:
	void	Init				();
	int		Changed				();

	BOOL	ParseOptions		(ViewParser& lex);
	BOOL	ParseOption			(ViewParser& lex, BOOL blk = TRUE);
	BOOL	ParseBlock			(ViewParser& lex);

	CWoormDocMng*		m_pDocument;

public:
	Options(CWoormDocMng*);
	Options(const Options&);

public:
	CWoormDocMng*		GetDocument() const { return m_pDocument; }

	void	operator =	(const Options&);
	int		operator ==	(Options&);
	int		operator !=	(Options&);

	BOOL	Parse	(ViewParser&);
	void	Unparse	(ViewUnparser&);

	void	LoadDefault (const CTBNamespace& aWrmNamespace);
	void	SaveDefault (const CTBNamespace& aWrmNamespace);

	BOOL NoBitmap		(CWoormDocMng* pDocument, BOOL bPrinting);
	BOOL NoBorders		(CWoormDocMng* pDocument, BOOL bPrinting);
	BOOL ShowTitles		(CWoormDocMng* pDocument, BOOL bPrinting);
	BOOL ShowLabels		(CWoormDocMng* pDocument, BOOL bPrinting);
	BOOL UseDraftFont	(CWoormDocMng* pDocument, BOOL bPrinting);

	BOOL MustCreateXsd1() const { return m_bCreateXsd1; }
	BOOL MustCreateXsd2() const { return m_bCreateXsd2; }
	BOOL MustCreateXsd() const { return m_bCreateXsd1 || m_bCreateXsd2; }
};

//===========================================================================
class TB_EXPORT CSpecialField : public CObject
{
	DECLARE_DYNAMIC(CSpecialField)
public:
	enum ESpecialField
	{
		SPECIAL_PAGE,
		SPECIAL_TOT_PAGE,
		SPECIAL_PAGE_SPLITTER,
		SPECIAL_SPLITTER,
		SPECIAL_TOT_SPLITTER,

		SPECIAL_APPDATE,
		SPECIAL_TODAY,
		SPECIAL_TODAY2,
		SPECIAL_YEAR,
		SPECIAL_MONTH,
		SPECIAL_MONTH2,
		SPECIAL_DAY,
		SPECIAL_HH_MM,
		SPECIAL_HH,
		SPECIAL_MM,
		SPECIAL_SEC,
		SPECIAL_USER,
		SPECIAL_LOGINUSER,
		SPECIAL_COMPUTER,
		SPECIAL_APP_TITLE,
		SPECIAL_APP_REL,
		SPECIAL_TB_REL,
		SPECIAL_SYS_COMPANY_NAME,
		SPECIAL_LICENSEE,
		SPECIAL_REPORTNAME,
		SPECIAL_REPORT_TITLE,
		SPECIAL_REPORT_SUBJECT,
		SPECIAL_REPORT_AUTHOR,
		SPECIAL_REPORT_COMPANY,
		SPECIAL_REPORT_COMMENTS,

		SPECIAL_DB_COMPANY_NAME,
		SPECIAL_PRODUCER_NAME,
		SPECIAL_PRODUCT_DATE,

		SPECIAL_EVAL_EXPR,

		//SPECIAL_DATABASE_NAME,
		SPECIAL_CUSTOM
	};

private:
	ESpecialField m_eField;
public:
	CString			m_sKey;
	CString			m_sDescription;
public:
	CSpecialField(ESpecialField aField) : m_eField(aField) {}
	CSpecialField(CString sKey, CString sDescription) 
		: m_eField(SPECIAL_CUSTOM), m_sKey(sKey), m_sDescription(sDescription) {}

	CString GetKeyword()
	{
		if (m_eField == SPECIAL_CUSTOM)
			return m_sKey;

		return GetKeyword(m_eField);
	}

	CString GetOldKeyword()
	{
		return GetOldKeyword(m_eField);
	}

	static CString GetKeyword(ESpecialField f)
	{
		switch (f)
		{
			case SPECIAL_PAGE:				return _T("Page");
			case SPECIAL_TOT_PAGE:			return _T("Page Number");

			case SPECIAL_PAGE_SPLITTER:		return _T("Page.Splitter");
			case SPECIAL_SPLITTER:			return _T("Splitter Page");
			case SPECIAL_TOT_SPLITTER:		return _T("Total Splitter Pages");

			case SPECIAL_APPDATE:			return _T("Application Date");
			case SPECIAL_TODAY:				return _T("Date (dd-mm-yy)");
			case SPECIAL_TODAY2:			return _T("Date (dd month yy)");
			case SPECIAL_YEAR:				return _T("Year");
			case SPECIAL_MONTH:				return _T("Month");
			case SPECIAL_MONTH2:			return _T("Month Name");
			case SPECIAL_DAY:				return _T("Day");
			case SPECIAL_HH_MM:				return _T("Time");
			case SPECIAL_HH:				return _T("Hours");
			case SPECIAL_MM:				return _T("Minutes");
			case SPECIAL_SEC:				return _T("Seconds");

			case SPECIAL_USER:				return _T("Window User");
			case SPECIAL_LOGINUSER:			return _T("Logged User");
			case SPECIAL_COMPUTER:			return _T("Computer Name");

			case SPECIAL_APP_TITLE:			return _T("Application Name");
			case SPECIAL_APP_REL:			return _T("Version");
			case SPECIAL_TB_REL:			return _T("TaskBuilder Version");
			case SPECIAL_LICENSEE:			return _T("Licensee");
			case SPECIAL_PRODUCER_NAME:		return _T("Producer Name");
			case SPECIAL_PRODUCT_DATE:		return _T("Product Date");

			case SPECIAL_REPORTNAME:		return _T("Report Name");
			case SPECIAL_REPORT_TITLE:		return _T("Report Title");
			case SPECIAL_REPORT_SUBJECT:	return _T("Report Subject");
			case SPECIAL_REPORT_AUTHOR:		return _T("Report Author");
			case SPECIAL_REPORT_COMPANY:	return _T("Report Owner");
			case SPECIAL_REPORT_COMMENTS:	return _T("Report Remarks");

			case SPECIAL_SYS_COMPANY_NAME:	return _T("Company Name");
			case SPECIAL_DB_COMPANY_NAME:	return _T("Company.Name");
			//case SPECIAL_DATABASE_NAME:		return _T("Database");

			case SPECIAL_EVAL_EXPR:			return  _T("EVAL <woorm-string-expression>"); 

			case SPECIAL_CUSTOM:			return _T("<CUSTOM>");
		}
		
		ASSERT(FALSE);
		return _T("");
	}

	static CString GetOldKeyword(ESpecialField f)
	{
		switch (f)
		{
			case SPECIAL_PAGE:				return _T("Pagina");
			case SPECIAL_TOT_PAGE:			return _T("Totale pagine");
			case SPECIAL_APPDATE:			return _T("Data delle operazioni");
			case SPECIAL_TODAY:				return _T("Data (gg-mm-aa)");
			case SPECIAL_TODAY2:			return _T("Data (gg mese aa)");
			case SPECIAL_YEAR:				return _T("Anno");
			case SPECIAL_MONTH:				return _T("Mese");
			case SPECIAL_MONTH2:			return _T("Nome del mese");
			case SPECIAL_DAY:				return _T("Giorno");
			case SPECIAL_HH_MM:				return _T("Ora e minuto");
			case SPECIAL_HH:				return _T("Ora");
			case SPECIAL_MM:				return _T("Minuto");
			case SPECIAL_SEC:				return _T("Secondo");
			case SPECIAL_USER:				return _T("Utente");
			case SPECIAL_LOGINUSER:			return _T("Utente Applicativo");
			case SPECIAL_COMPUTER:			return _T("Computer");
			case SPECIAL_APP_TITLE:			return _T("Applicazione");
			case SPECIAL_APP_REL:			return _T("Versione");
			case SPECIAL_TB_REL:			return _T("TaskBuilder");
			case SPECIAL_SYS_COMPANY_NAME:	return _T("Azienda");
			//case SPECIAL_DATABASE_NAME:		return _T("Database");
			case SPECIAL_LICENSEE:			return _T("Licenziatario");
			case SPECIAL_REPORTNAME:		return _T("Nome del report");
			case SPECIAL_REPORT_TITLE:		return _T("Titolo del report");
			case SPECIAL_REPORT_SUBJECT:	return _T("Oggetto del report");
			case SPECIAL_REPORT_AUTHOR:		return _T("Autore del report");
			case SPECIAL_REPORT_COMPANY:	return _T("Società proprietaria del report");
			case SPECIAL_REPORT_COMMENTS:	return _T("Commenti al report");
		}
		
		//ASSERT(FALSE);
		return _T("");
	}

	CString GetDescription()
	{
		if (m_eField == SPECIAL_CUSTOM)
			return m_sDescription;

		return GetDescription(m_eField);
	}

	static CString GetDescription(ESpecialField f)
	{
		switch (f)
		{
			case SPECIAL_PAGE:				return _TB("Page");
			case SPECIAL_TOT_PAGE:			return _TB("Total pages");

			case SPECIAL_PAGE_SPLITTER:		return _TB("Page.Splitter");
			case SPECIAL_SPLITTER:			return _TB("Splitter page");
			case SPECIAL_TOT_SPLITTER:		return _TB("Total splitter pages");

			case SPECIAL_APPDATE:			return _TB("Date of operations");
			case SPECIAL_TODAY:				return _TB("Date (dd-mm-yy)");
			case SPECIAL_TODAY2:			return _TB("Date (dd month yy)");
			case SPECIAL_YEAR:				return _TB("Year");
			case SPECIAL_MONTH:				return _TB("Month");
			case SPECIAL_MONTH2:			return _TB("Month name");
			case SPECIAL_DAY:				return _TB("Day");
			case SPECIAL_HH_MM:				return _TB("Hour and minute");
			case SPECIAL_HH:				return _TB("Hour");
			case SPECIAL_MM:				return _TB("Minute");
			case SPECIAL_SEC:				return _TB("Second");
			case SPECIAL_USER:				return _TB("User");
			case SPECIAL_LOGINUSER:			return _TB("User Application");
			case SPECIAL_COMPUTER:			return _TB("Computer");
			case SPECIAL_APP_TITLE:			return _TB("Application");
			case SPECIAL_APP_REL:			return _TB("Release");
			case SPECIAL_TB_REL:			return _TB("TaskBuilder");
			case SPECIAL_LICENSEE:			return _TB("Licensee");
			case SPECIAL_PRODUCER_NAME:		return _TB("Producer Name");
			case SPECIAL_PRODUCT_DATE:		return _TB("Product Date");
			case SPECIAL_REPORTNAME:		return _TB("Report name");
			case SPECIAL_REPORT_TITLE:		return _TB("Report title");
			case SPECIAL_REPORT_SUBJECT:	return _TB("Report object");
			case SPECIAL_REPORT_AUTHOR:		return _TB("Report author");
			case SPECIAL_REPORT_COMPANY:	return _TB("Company owning the report");
			case SPECIAL_REPORT_COMMENTS:	return _TB("Comments on report");

			case SPECIAL_SYS_COMPANY_NAME:	return _TB("System Company Name");
			case SPECIAL_DB_COMPANY_NAME:	return _TB("Master Company Name");
			//case SPECIAL_DATABASE_NAME:		return _TB("Database Name");

			case SPECIAL_EVAL_EXPR:			return  _T("EVAL <woorm-string-expression>");

			case SPECIAL_CUSTOM:			return  _T("<CUSTOM description>");;
		}
		
		ASSERT(FALSE);
		return _T("");
	}

};

//===========================================================================

class TB_EXPORT CHtmlTag : public CObject
{
	DECLARE_DYNAMIC(CHtmlTag)
public:
	// non aggiungere nulla in cima e in fondo alla lista
	// gli attributi vanno aggiunti subito di seguito al tag a cui appartengono per mantenere il ciclo for nel metodo FillHtmlTags in CRSTreeCtrl
	enum EHtmlTag
	{
		HTML_TAG_NEWLINE,
		HTML_TAG_BOLD,
		HTML_TAG_ITALIC,
		HTML_TAG_UNDERLINE,
		HTML_TAG_STRIKEOUT,
		HTML_TAG_UNORDERED_LIST,
		HTML_TAG_ORDERED_LIST,
		HTML_TAG_LIST_ITEM,
		HTML_TAG_FONT,	
		HTML_ATTRIBUTE_FACE,
		HTML_ATTRIBUTE_SIZE,
		HTML_ATTRIBUTE_COLOR,
		HTML_TAG_DIV,
		HTML_ATTRIBUTE_STYLE,
		HTML_PROPERTY_WIDTH,
		HTML_PROPERTY_ALIGN,
		HTML_PROPERTY_OVERFLOW,
		HTML_PROPERTY_FLOAT,
		HTML_PROPERTY_FONTCOLOR,
		HTML_PROPERTY_BKGCOLOR,
		HTML_TAG_LAST
	};

private:
	EHtmlTag m_eHtmlTag;

public:
	BOOL m_bIsAttribute;
	CString m_sName;
	CString m_sTag;
	CString m_sDescription;
	CString m_sExample;

public:
	CHtmlTag(EHtmlTag aTag) : m_eHtmlTag(aTag) {}
	/*~CHtmlTag();*/
	CString GetHtmlTag();
	CString GetHtmlFragment();
	CString GetHtmlName();
	BOOL IsAttribute();
	BOOL IsStyleProperty();
	CString GetExample();
	CString GetDescription();

	static CString GetHtmlName(EHtmlTag t)
	{
		switch (t)
		{
		case HTML_TAG_NEWLINE:		return _T("New Line");
		case HTML_TAG_BOLD:		return _T("Bold");
		case HTML_TAG_ITALIC:		return _T("Italic");
		case HTML_TAG_UNDERLINE:		return _T("Underline");
		case HTML_TAG_STRIKEOUT:		return _T("Strikeout");
		case HTML_TAG_UNORDERED_LIST:		return _T("Unordered List");
		case HTML_TAG_ORDERED_LIST:			return _T("Ordered List");
		case HTML_TAG_LIST_ITEM:			return _T("List Item");
		case HTML_TAG_DIV:					return _T("Div");
		case HTML_ATTRIBUTE_STYLE:			return _T("Style");
		case HTML_PROPERTY_WIDTH:		return _T("Width");
		case HTML_PROPERTY_ALIGN:		return _T("Align");
		case HTML_PROPERTY_OVERFLOW:		return _T("Overflow");
		case HTML_PROPERTY_FLOAT:		return _T("Float");
		case HTML_PROPERTY_FONTCOLOR:		return _T("Font color");
		case HTML_PROPERTY_BKGCOLOR:		return _T("Background color");
		case HTML_TAG_FONT:		return _T("Font");
		case HTML_ATTRIBUTE_FACE:		return _T("Face");
		case HTML_ATTRIBUTE_SIZE:		return _T("Size");
		case HTML_ATTRIBUTE_COLOR:		return _T("Color");

		}

		ASSERT(FALSE);
		return _T("");
	}
	static CString GetHtmlTag(EHtmlTag t)
	{
		switch (t)
		{
		case HTML_TAG_NEWLINE:			return _T("br");
		case HTML_TAG_BOLD:				return _T("b");
		case HTML_TAG_ITALIC:			return _T("i");
		case HTML_TAG_UNDERLINE:		return _T("u");
		case HTML_TAG_STRIKEOUT:		return _T("s");
		case HTML_TAG_UNORDERED_LIST:	return _T("ul");
		case HTML_TAG_ORDERED_LIST:		return _T("ol");
		case HTML_TAG_LIST_ITEM:		return _T("li");
		case HTML_TAG_DIV:				return _T("div");
		case HTML_ATTRIBUTE_STYLE:			return _T("style");
		case HTML_PROPERTY_WIDTH:		return _T("width");
		case HTML_PROPERTY_ALIGN:		return _T("text-align");
		case HTML_PROPERTY_OVERFLOW:		return _T("overflow");
		case HTML_PROPERTY_FLOAT:		return _T("float");
		case HTML_PROPERTY_FONTCOLOR:		return _T("color");
		case HTML_PROPERTY_BKGCOLOR:		return _T("background-color");
		case HTML_TAG_FONT:				return _T("font");
		case HTML_ATTRIBUTE_FACE:		return _T("face");
		case HTML_ATTRIBUTE_SIZE:		return _T("size");
		case HTML_ATTRIBUTE_COLOR:		return _T("color");
		}

		ASSERT(FALSE);
		return _T("");
	}
	static CString GetDescription(EHtmlTag t)
	{
		switch (t)
		{
		case HTML_TAG_NEWLINE:					return _TB("Tag that enters a line break.");
		case HTML_TAG_BOLD:						return _TB("Span of text displayed in boldface.");
		case HTML_TAG_ITALIC:					return _TB("Span of text displayed in italic type.");
		case HTML_TAG_UNDERLINE:				return _TB("Span of text displayed underlined.");
		case HTML_TAG_STRIKEOUT:				return _TB("Span of text displayed with a line through it.");
		case HTML_TAG_UNORDERED_LIST:			return _TB("An unordered (bulleted) list of item.");
		case HTML_TAG_ORDERED_LIST:				return _TB("An ordered list of item.");
		case HTML_TAG_LIST_ITEM:				return _TB("An item of a list.");
		case HTML_TAG_DIV:						return _TB("A document section.");
		case HTML_ATTRIBUTE_STYLE:				return _TB("Define style information of a tag.");
		case HTML_PROPERTY_WIDTH:				return _TB("Width property of style attribute. It defines the width of a document section.");
		case HTML_PROPERTY_ALIGN:				return _TB("Alignment property of style attribute. It defines the horizontal text alignment.");
		case HTML_PROPERTY_OVERFLOW:			return _TB("Overflow property of style attribute, it sets what to do with content that renders outside the document section.");
		case HTML_PROPERTY_FLOAT:				return _TB("Float property of style attribute. It specifies whether or not an element should float.");
		case HTML_PROPERTY_FONTCOLOR:			return _TB("Font color property of style attribute.");
		case HTML_PROPERTY_BKGCOLOR:			return _TB("Background color property of style attribute.");
		case HTML_TAG_FONT:						return _TB("Span of text displayed with specified font.");
		case HTML_ATTRIBUTE_FACE:				return _TB("Specify the font of text.");
		case HTML_ATTRIBUTE_SIZE:				return _TB("Specify the font size of text.");
		case HTML_ATTRIBUTE_COLOR:				return _TB("Specify the color of text.");
		}

		ASSERT(FALSE);
		return _T("");
	}
	static CString GetExample(EHtmlTag t)
	{
		switch (t)
		{
		case HTML_TAG_NEWLINE:			return _TB("<br>");
		case HTML_TAG_BOLD:				return _TB("<b>my bold tex</b>");
		case HTML_TAG_ITALIC:			return _TB("<i>my italic text</i>");
		case HTML_TAG_UNDERLINE:		return _TB("<u>my underlined text</u>");
		case HTML_TAG_STRIKEOUT:		return _TB("<s>my text with a strikethrougth</s>");
		case HTML_TAG_UNORDERED_LIST:	return _TB("<ul>my unordered list</ul>");
		case HTML_TAG_ORDERED_LIST:		return _TB("<ol>my ordered list</ol>");
		case HTML_TAG_LIST_ITEM:		return _TB("<li>my item of the list</li>");
		case HTML_TAG_DIV:				return _TB("<div>my text or other sub-sections</div>");
		case HTML_ATTRIBUTE_STYLE:		return _TB("<div style='color:red; width:100px'>my red text in a section wide 100 pixel </div>");
		case HTML_PROPERTY_WIDTH:		return _TB("<div style='width:100px'>my text in a section wide 100 pixel </div>");
		case HTML_PROPERTY_ALIGN:		return _TB("<div style='text-align:right'>my text aligned to the right</div>");
		case HTML_PROPERTY_OVERFLOW:	return _TB("<div style='overflow:auto'>my text </div>");
		case HTML_PROPERTY_FLOAT:		return _TB("<div style='float:right'>my text in a section floated to the right</div>");
		case HTML_PROPERTY_FONTCOLOR:	return _TB("<div style='color:red; '>my red text </div>");
		case HTML_PROPERTY_BKGCOLOR:	return _TB("<div style='background-color:powderblue;'>my text in a section with a powderblue background</div>");
		case HTML_TAG_FONT:				return _TB("<font>my text differently formatted </font>");
		case HTML_ATTRIBUTE_FACE:		return _TB("<font face='Arial'>my text with Arial face </font>");
		case HTML_ATTRIBUTE_SIZE:		return _TB("<font size='16'>my text with size 16 </font>");
		case HTML_ATTRIBUTE_COLOR:		return _TB("<font color='red'>my red text</font>");
		}
		ASSERT(FALSE);
		return _T("");
	}
	static BOOL IsAttribute(EHtmlTag t)
	{
		switch (t)
		{
		case HTML_TAG_NEWLINE:			return FALSE;
		case HTML_TAG_BOLD:				return FALSE;
		case HTML_TAG_ITALIC:			return FALSE;
		case HTML_TAG_UNDERLINE:		return FALSE;
		case HTML_TAG_STRIKEOUT:		return FALSE;
		case HTML_TAG_UNORDERED_LIST:	return FALSE;
		case HTML_TAG_ORDERED_LIST:		return FALSE;
		case HTML_TAG_LIST_ITEM:		return FALSE;
		case HTML_TAG_DIV:				return FALSE;
		case HTML_ATTRIBUTE_STYLE:		return TRUE;
		case HTML_PROPERTY_WIDTH:		return FALSE;
		case HTML_PROPERTY_ALIGN:		return FALSE;
		case HTML_PROPERTY_OVERFLOW:	return FALSE;
		case HTML_PROPERTY_FLOAT:		return FALSE;
		case HTML_PROPERTY_FONTCOLOR:	return FALSE;
		case HTML_PROPERTY_BKGCOLOR:	return FALSE;
		case HTML_TAG_FONT:				return FALSE;
		case HTML_ATTRIBUTE_FACE:		return TRUE;
		case HTML_ATTRIBUTE_SIZE:		return TRUE;
		case HTML_ATTRIBUTE_COLOR:		return TRUE;
		}

		ASSERT(FALSE);
		return FALSE;
	}
	static BOOL IsStyleProperty(EHtmlTag t)
	{
		switch (t)
		{
		case HTML_TAG_NEWLINE:			return FALSE;
		case HTML_TAG_BOLD:				return FALSE;
		case HTML_TAG_ITALIC:			return FALSE;
		case HTML_TAG_UNDERLINE:		return FALSE;
		case HTML_TAG_STRIKEOUT:		return FALSE;
		case HTML_TAG_UNORDERED_LIST:	return FALSE;
		case HTML_TAG_ORDERED_LIST:		return FALSE;
		case HTML_TAG_LIST_ITEM:		return FALSE;
		case HTML_TAG_DIV:				return FALSE;
		case HTML_ATTRIBUTE_STYLE:		return FALSE;
		case HTML_PROPERTY_WIDTH:		return TRUE;
		case HTML_PROPERTY_ALIGN:		return TRUE;
		case HTML_PROPERTY_OVERFLOW:	return TRUE;
		case HTML_PROPERTY_FLOAT:		return TRUE;
		case HTML_PROPERTY_FONTCOLOR:	return TRUE;
		case HTML_PROPERTY_BKGCOLOR:	return TRUE;
		case HTML_TAG_FONT:				return FALSE;
		case HTML_ATTRIBUTE_FACE:		return FALSE;
		case HTML_ATTRIBUTE_SIZE:		return FALSE;
		case HTML_ATTRIBUTE_COLOR:		return FALSE;
		}

		ASSERT(FALSE);
		return FALSE;
	}
	static CString GetHtmlFragment(EHtmlTag t)
	{
		CString myFragment;
		if (t == HTML_TAG_NEWLINE)
			myFragment = _T("<br>");
		else if (IsAttribute(t))
			myFragment = GetHtmlTag(t) + _T("=''");
		else if (IsStyleProperty(t))
			myFragment = GetHtmlTag(t) + _T(":;");
		else
			myFragment = _T("<") + GetHtmlTag(t) + _T(">") + _T("</") + GetHtmlTag(t) + _T(">");
		return myFragment;
	}
};

//===========================================================================

class TB_EXPORT CQueryTag : public CObject
{
	DECLARE_DYNAMIC(CQueryTag)
public:
	// non aggiungere nulla in cima e in fondo alla lista
	// gli attributi vanno aggiunti subito di seguito al tag a cui appartengono per mantenere il ciclo for nel metodo FillHtmlTags in CRSTreeCtrl
	enum EQueryTag
	{
		QUERY_TAG_COL,
		QUERY_TAG_IN,
		QUERY_TAG_EVAL,

		QUERY_TAG_WHEN_EXPAND,
		QUERY_TAG_WHEN_EXPAND_ELSE,
		QUERY_TAG_WHEN_INCLUDE,
		QUERY_TAG_ELSE,

		QUERY_TAG_DATE,
		QUERY_TAG_DATETIME,
		QUERY_TAG_ENUM,
		QUERY_TAG_TRUE,
		QUERY_TAG_FALSE,
		QUERY_TAG_STR,

		QUERY_TAG_OUT,
		QUERY_TAG_REF,

		//------------
		QUERY_TAG_LAST
	};

private:
	EQueryTag m_eTag;

public:
	BOOL m_bIsAttribute;
	CString m_sName;
	CString m_sTag;
	CString m_sDescription;
	CString m_sExample;

public:
	CQueryTag(EQueryTag aTag) : m_eTag(aTag) {}

	CString GetTag();
	CString GetFragment();
	CString GetName();
	BOOL IsAttribute();
	BOOL IsStyleProperty();
	CString GetExample();
	CString GetDescription();

	static CString GetName(EQueryTag t)
	{
		switch (t)
		{
		case QUERY_TAG_COL:					return _T("Column");
		case QUERY_TAG_IN:					return _T("Input parameter");
		case QUERY_TAG_OUT:					return _T("Output parameter");
		case QUERY_TAG_REF:					return _T("Input/Output parameter");
		case QUERY_TAG_EVAL:				return _T("Expression parameter");

		case QUERY_TAG_WHEN_EXPAND:			return _T("Dynamic fragment");
		case QUERY_TAG_WHEN_EXPAND_ELSE:	return _T("Alternative fragment");
		case QUERY_TAG_ELSE:				return _T("ELSE sub tag");
		case QUERY_TAG_WHEN_INCLUDE:		return _T("Dynamic fragment by field");

		case QUERY_TAG_TRUE:				return _T("TRUE value");
		case QUERY_TAG_FALSE:				return _T("FALSE value");
		case QUERY_TAG_STR:					return _T("TEXT value");
		case QUERY_TAG_ENUM:				return _T("ENUM value");
		case QUERY_TAG_DATE:				return _T("Date value");
		case QUERY_TAG_DATETIME:			return _T("DateTime value");
		}

		ASSERT(FALSE);
		return _T("");
	}

	static CString GetTag(EQueryTag t)
	{
		switch (t)
		{
		case QUERY_TAG_COL:					return _T("{COL field }");
		case QUERY_TAG_IN:					return _T("{IN field }");
		case QUERY_TAG_OUT:					return _T("{OUT field }");
		case QUERY_TAG_REF:					return _T("{REF field }");

		case QUERY_TAG_EVAL:				return _T("{EVAL expr }");

		case QUERY_TAG_WHEN_EXPAND:			return _T("{WHEN expr EXPAND { sql } }");
		case QUERY_TAG_WHEN_EXPAND_ELSE:	return _T("{WHEN expr EXPAND { sql } ELSE { sql } }");
		case QUERY_TAG_ELSE:				return _T("ELSE { sql }");
		case QUERY_TAG_WHEN_INCLUDE:		return _T("{WHEN expr INCLUDE field }");

		case QUERY_TAG_TRUE:				return _T("{TRUE}");
		case QUERY_TAG_FALSE:				return _T("{FALSE}");
		case QUERY_TAG_STR:					return _T("{\"text\"}");
		case QUERY_TAG_ENUM:				return _T("{ tag : item }");
		case QUERY_TAG_DATE:				return _T("{d\"dd/mm/yyyy\"}");
		case QUERY_TAG_DATETIME:			return _T("{dt\"dd/mm/yyyy hh:mm:ss\"}");
		}

		ASSERT(FALSE);
		return _T("");
	}

	static CString GetDescription(EQueryTag t)
	{
		switch (t)
		{
		case QUERY_TAG_COL:					return _TB("Tag that select a column");
		case QUERY_TAG_IN:					return _TB("Tag that add an input parameter");
		case QUERY_TAG_EVAL:				return _TB("Tag that add an input parameter by evaluate the expression");
		case QUERY_TAG_OUT:					return _TB("Tag that add an output parameter to a store procedure call");
		case QUERY_TAG_REF:					return _TB("Tag that add an input/output parameter to a store procedure call");

		case QUERY_TAG_WHEN_EXPAND:			return _TB("Tag that add dynamic sql fragment when the evaluated condition is true");
		case QUERY_TAG_WHEN_EXPAND_ELSE:	return _TB("Tag that add alternative sql fragment depends by evaluated condition");
		case QUERY_TAG_WHEN_INCLUDE:		return _TB("Tag that add dynamic sql fragment from the field current value when the evaluated condition is true");
		case QUERY_TAG_ELSE:				return _TB("Sub tag alternative fragment could nested into a tag WHEN-EXPAND");

		case QUERY_TAG_TRUE:				return _T("{TRUE}");
		case QUERY_TAG_FALSE:				return _T("{FALSE}");
		case QUERY_TAG_STR:					return _TB("{\"text\"}");
		case QUERY_TAG_ENUM:				return _TB("{ numeric-tag : numeric-item }");
		case QUERY_TAG_DATE:				return _T("{d\"dd/mm/yyyy\"}");
		case QUERY_TAG_DATETIME:			return _T("{dt\"dd/mm/yyyy hh:mm:ss\"}");
		}

		//ASSERT(FALSE);
		return _T("");
	}

	static CString GetExample(EQueryTag t)
	{
		switch (t)
		{
		case QUERY_TAG_COL:					return _T("{COL w_CustSupp }");
		case QUERY_TAG_IN:					return _T("{IN w_AskStartCode }");
		case QUERY_TAG_OUT:					return _T("{OUT field }");
		case QUERY_TAG_REF:					return _T("{REF field }");

		case QUERY_TAG_EVAL:				return _T("{EVAL expr }");

		case QUERY_TAG_WHEN_EXPAND:			return _T("{WHEN expr EXPAND { sql } }");
		case QUERY_TAG_WHEN_EXPAND_ELSE:	return _T("{WHEN expr EXPAND { sql } ELSE { sql } }");
		case QUERY_TAG_ELSE:				return _T("ELSE { sql }");
		case QUERY_TAG_WHEN_INCLUDE:		return _T("{WHEN expr INCLUDE field }");

		case QUERY_TAG_TRUE:				return _T("{TRUE}");
		case QUERY_TAG_FALSE:				return _T("{FALSE}");
		case QUERY_TAG_STR:					return _T("{\"text\"}");
		case QUERY_TAG_ENUM:				return _T("{ tag : item }");
		case QUERY_TAG_DATE:				return _T("{d\"dd/mm/yyyy\"}");
		case QUERY_TAG_DATETIME:			return _T("{dt\"dd/mm/yyyy hh:mm:ss\"}");
		}
		//ASSERT(FALSE);
		return _T("");
	}

	static BOOL IsAttribute(EQueryTag t)
	{
		return FALSE;
	}
	static BOOL IsStyleProperty(EQueryTag t)
	{
		return FALSE;
	}

	static CString GetFragment(EQueryTag t)
	{
		return GetTag(t);
	}
};

//===========================================================================

class TB_EXPORT CBordersDlg: public CParsedDialog
{
protected:
	Borders&	m_Borders;

public:
	CBordersDlg(Borders&, CWnd* = NULL);

protected:
	virtual BOOL OnInitDialog	();
	virtual void OnOK			();

	DECLARE_MESSAGE_MAP()
	DECLARE_DYNAMIC(CBordersDlg);
};

//===========================================================================
class TB_EXPORT CTableBordersDlg: public CParsedDialog
{
protected:
	TableBorders&	m_Borders;
	Table*			m_pTable;

public:
	CTableBordersDlg(Table* pTable, TableBorders&, CWnd* = NULL);

protected:
	virtual BOOL OnInitDialog	();
	virtual void OnOK			();
	
	afx_msg void    OnSetCustomPen		();
	afx_msg void    OnRemoveCustomPen	();

	DECLARE_MESSAGE_MAP()
	DECLARE_DYNAMIC(CTableBordersDlg);
};

//===========================================================================
class TB_EXPORT COptionsDlg : public CParsedDialog
{
protected:
	Options&		m_Options;
	CComboBoxExt	m_PrintersCombo;
	CNamespaceEdit	m_NamespaceEdit;

	CMetricEdit	m_BitmapX;
	CMetricEdit	m_BitmapY;
	CWoormDocMng*	m_pDocument;

public:
	COptionsDlg(Options&, CWoormDocMng* pDocument, CWnd* = NULL);

protected:
	void CenterBitmap	(BOOL bX);
	void EnableControls	();
	void LoadPrinters	();
	void SavePrinters	();

protected:
	virtual BOOL OnInitDialog	();
	virtual void OnOK			();

	//{{AFX_MSG(COptionsDlg)
	afx_msg void OnBitmapCenterX();
	afx_msg void OnBitmapCenterY();
	afx_msg void OnPrintDraft();
	afx_msg void OnDefaultPrintDraft();
	afx_msg void OnCheckCreateXsd1();
	afx_msg void OnCheckCreateXsd2();
	//}}AFX_MSG

	DECLARE_MESSAGE_MAP()
	DECLARE_DYNAMIC(COptionsDlg);
};

//===========================================================================
class TB_EXPORT CAlignDlg : public CParsedDialog
{
protected:
	AlignType&	m_nAlign;
	BOOL&		m_bAllColumns;
	BOOL		m_bAskColumn;
	BOOL		m_bAllowFieldSet;
	BOOL		m_bAllowCenterBottom;
	BOOL		m_bAllowVertical;

public:
	CAlignDlg(AlignType&, CWnd* = NULL);
	CAlignDlg(UINT anIDG, AlignType&, BOOL&, CWnd* = NULL);

	void SetAllowFieldSet(BOOL bAllow) { m_bAllowFieldSet = bAllow; }
	void SetAllowCenterBottom(BOOL bAllow) { m_bAllowCenterBottom = bAllow; }
	void SetAllowVertical(BOOL bAllow) { m_bAllowVertical = bAllow; }
	  
protected:  

	virtual BOOL OnInitDialog	();
	virtual void OnOK			();

	//{{AFX_MSG(CAlignDlg)
	afx_msg	void SetButtonStatus();
	afx_msg	void SetVCenter();
	afx_msg	void SetNoVCenter();

	afx_msg void OnClickFieldSet();
	afx_msg void OnClick90();
	afx_msg void OnClick270();
	//}}AFX_MSG

	DECLARE_MESSAGE_MAP()
	DECLARE_DYNAMIC(CAlignDlg);
};

//===========================================================================
class TB_EXPORT CRatiosDlg: public CParsedDialog
{
protected:
	CMetricEdit	m_HRatioEdit;
	CMetricEdit	m_VRatioEdit;
	int&		m_HRatio;
	int&		m_VRatio;
	CWoormDocMng*	m_pDocument;

public:
	CRatiosDlg(int&, int&, CWoormDocMng* pDoc);

protected:
	virtual BOOL OnInitDialog	();
	virtual void OnOK			();

	//{{AFX_MSG(CRatiosDlg)
	//}}AFX_MSG

	DECLARE_MESSAGE_MAP()
	DECLARE_DYNAMIC(CRatiosDlg);
};

//===========================================================================
class TB_EXPORT CLineWidthDlg: public CParsedDialog
{
protected:
	CMetricEdit		m_LineWidthEdit;
	BorderPen&		m_BorderPen;

	BOOL&			m_bAllColumns;
	BOOL			m_bAskColumn;

	DataLng			m_Color;
	CColorEdit		m_edtColor;

public:
	CLineWidthDlg(BorderPen&, CWnd* = NULL);
	CLineWidthDlg(BorderPen&, BOOL&, CWnd* = NULL);

protected:
			void	CheckLineWidthButtons	(int width);

	virtual BOOL	OnInitDialog	();
	virtual void	OnOK			();

	//{{AFX_MSG(CLineWidthDlg)
	afx_msg void    OnPenTiny			();
	afx_msg void    OnPenSmall			();
	afx_msg void    OnPenMedium			();
	afx_msg void    OnPenLarge			();
	afx_msg void    OnPenExtra			();
	afx_msg void    OnKillFocusPenSize	();
	//}}AFX_MSG

	DECLARE_MESSAGE_MAP()
	DECLARE_DYNAMIC(CLineWidthDlg);
};

//==============================================================================
class TB_EXPORT CTextDlg: public CParsedDialog
{
protected:
	CString&	m_strText;
	CEdit		m_TextEdit;
	int			m_nSize;

public:
	CTextDlg (CString&, CWnd* = NULL);

	// limit m_nSize of the only text item in dialog (0 means non limits)
	void	limitTextSize (int m_nSize);

protected:
	virtual BOOL OnInitDialog	();
	virtual void OnOK();
	DECLARE_DYNAMIC(CTextDlg);
};

//==============================================================================
class TB_EXPORT CSpecialTextDlg: public CParsedDialog
{
protected:
	CString&	m_strText;
	BOOL&		m_bSpecialField;
	CEdit		m_TextEdit;
	CTBListBox	m_ListBox;

public:
	CSpecialTextDlg (CString&, BOOL& bSpecialField, CWnd* = NULL);

protected:
	virtual BOOL OnInitDialog	();
	virtual void OnOK			();

	//{{AFX_MSG(CSpecialTextDlg)
	afx_msg void OnSpecialSelected();
	//}}AFX_MSG

	DECLARE_MESSAGE_MAP()
	DECLARE_DYNAMIC(CSpecialTextDlg);
};

//===========================================================================
class TB_EXPORT CMulAlignDlg : public CParsedDialog
{
protected:
	int& m_hID;
	int& m_vID;
public:

	CMulAlignDlg (int& m_hID, int& m_vID, CWnd* = NULL);

protected:
	virtual BOOL OnInitDialog	();
	virtual void OnOK			();

	//{{AFX_MSG(CMulAlignDlg)
	//}}AFX_MSG

	DECLARE_MESSAGE_MAP()
	DECLARE_DYNAMIC(CMulAlignDlg);
};

//===========================================================================
class TB_EXPORT CMulSizeDlg : public CParsedDialog
{
protected:
	CSize&		m_nSize;
	int& 		m_hID;
	int&		m_vID;
	CMetricEdit	m_CxValueEdit;
	CMetricEdit	m_CyValueEdit;

public:
	CMulSizeDlg (int& m_hID, int& m_vID, CSize&, CWnd* = NULL);

	BOOL			InitEdit		(int nID, int aValue, int aMin, int aMax);
	CMetricEdit&	GetMetricEdit	(int nID);

protected:
	virtual	BOOL OnInitDialog	();
	virtual	void OnOK			();

	//{{AFX_MSG(CMulSizeDlg)
	afx_msg	void OnClickCX			();
	afx_msg	void OnClickCY			();
	//}}AFX_MSG

	DECLARE_MESSAGE_MAP()
	DECLARE_DYNAMIC(CMulSizeDlg);
};

//===========================================================================
class TB_EXPORT CObjValueDlg : public CParsedDialog
{
protected:
	CRect&		m_Rect;
	BOOL		m_bOnlyMove;
	BOOL		m_bOnlyHeight;
	CMetricEdit	m_ObjTopEdit;
	CMetricEdit	m_ObjLeftEdit;
	CMetricEdit	m_ObjWidthEdit;
	CMetricEdit	m_ObjHeightEdit;

public:
	CObjValueDlg	(CRect& aRect, BOOL aOnlyMove = FALSE, CWnd* aParent = NULL, BOOL m_bOnlyHeight = FALSE);

	BOOL			InitEdit		(int nID, int aValue, int aMin, int aMax);
	CMetricEdit&	GetMetricEdit	(int nID);

protected:
	// standard dialog function
	virtual	BOOL OnInitDialog	();
	virtual	void OnOK			();

	//{{AFX_MSG(CObjValueDlg)
	//}}AFX_MSG

	DECLARE_MESSAGE_MAP()
	DECLARE_DYNAMIC(CObjValueDlg);
};

//===========================================================================
class TB_EXPORT CTableHeightDlg : public CParsedDialog
{
protected:
	CMetricEdit	m_ObjHTableTitle;
	CMetricEdit	m_ObjHColumnsTitle;
	CMetricEdit	m_ObjHRows;
	CMetricEdit	m_ObjHTotals;

	int&	m_nHTableTitle;
	int&	m_nHColumnsTitle;
	int&	m_nHRows;
	int&	m_nHTotals;

public:
	CTableHeightDlg	
		(	
			int&	nHTableTitle,
			int&	nHColumnsTitle,
			int&	nHRows,
			int&	nHTotals,
			CWnd*	aParent
		);

	BOOL			InitEdit		(int nID, int aValue, int aMin, int aMax);
	CMetricEdit&	GetMetricEdit	(int nID);

protected:
	// standard dialog function
	virtual	BOOL OnInitDialog	();
	virtual	void OnOK			();

	//{{AFX_MSG(CTableHeightDlg)
	//}}AFX_MSG

	DECLARE_MESSAGE_MAP()
	DECLARE_DYNAMIC(CTableHeightDlg);
};

//===========================================================================
class TB_EXPORT CRepeaterDlg : public CParsedDialog
{
protected:
	CIntEdit	m_Rows;
	CIntEdit	m_Columns;

	CMetricEdit	m_RowOffset;
	CMetricEdit	m_ColumnOffset;
public:
	int	m_nRows;
	int	m_nColumns;
	int	m_nYOffset;
	int	m_nXOffset;
	BOOL m_bByColumn;

public:
	CRepeaterDlg	
		(	
			int		nRows,
			int		nColumns,
			int		nYOffset,
			int		nXOffset,
			BOOL	bByColumn,
			CWnd*	aParent
		);

	BOOL			InitEdit		(int nID, int aValue, int aMin, int aMax);
	CMetricEdit&	GetMetricEdit	(int nID);

protected:
	// standard dialog function
	virtual	BOOL OnInitDialog	();
	virtual	void OnOK			();

	//{{AFX_MSG(CRepeaterDlg)
	//}}AFX_MSG

	DECLARE_MESSAGE_MAP()
	DECLARE_DYNAMIC(CRepeaterDlg);
};

//===========================================================================
class TB_EXPORT CWidthDlg : public CParsedDialog
{
protected:
	CMetricEdit	m_Width;

	int&	m_nWidth;

public:
	CWidthDlg	
		(	
			int&	nWidth,
			CWnd*	aParent
		);

protected:
	// standard dialog function
	virtual	BOOL OnInitDialog	();
	virtual	void OnOK			();

	//{{AFX_MSG(CWidthDlg)
	//}}AFX_MSG

	DECLARE_MESSAGE_MAP()
	DECLARE_DYNAMIC(CWidthDlg);
};

//===========================================================================
class TB_EXPORT CTableRowsDlg : public CParsedDialog
{
protected:
	CIntEdit	m_Rows;

	int&	m_nRows;
public:
	int		m_nPrevRows;

public:
	CTableRowsDlg	
		(	
			int&	nRows,
			CWnd*	aParent
		);

protected:
	// standard dialog function
	virtual	BOOL OnInitDialog	();
	virtual	void OnOK			();

	//{{AFX_MSG(CTableRowsDlg)
	//}}AFX_MSG

	DECLARE_MESSAGE_MAP()
	DECLARE_DYNAMIC(CTableRowsDlg);
};

//===========================================================================
class TB_EXPORT CGotoPageDlg : public CParsedDialog
{
protected:
	CIntEdit	m_PageEdit;
	int&		m_nPage;
	int			m_nMaxPage;

public:
	CGotoPageDlg	(int& aPage, int m_nMaxPage, CWnd* aParent = NULL);

	BOOL		InitEdit	(int nID, int aValue, int aMax);
	BOOL		GetValue	(int& value) const;

protected:
	// standard dialog function
	virtual	BOOL OnInitDialog	();
	virtual	void OnOK			();

	//{{AFX_MSG(CGotoPageDlg)
	//}}AFX_MSG


	DECLARE_MESSAGE_MAP()
	DECLARE_DYNAMIC(CGotoPageDlg);
};



//===========================================================================
class TB_EXPORT CHiddenWhenDlg : public CParsedDialog
{
protected:
	CExpEdit			m_edtHideExpr;
	BOOL				m_bHidden;
	SymTable*			m_pSymTable;
	Expression*&		m_pHideExpr;
	CString				m_strLabelColumn;
	BaseRect*			m_pBaseRect;
	BOOL				m_bNoHiddenRadio;
public:
	CHiddenWhenDlg (Expression*& pHideExpr, BOOL bHidden, CString strLabelColumn, SymTable* pSymTable, BaseRect* pBaseRect = NULL, CWnd* = NULL);
	CHiddenWhenDlg (Expression*& pHideExpr, SymTable* pSymTable, CWnd* aParent = NULL);
	
	BOOL			GetState();

protected:
	// standard dialog function
	virtual	BOOL	OnInitDialog	();
	virtual	void	OnOK			();
	virtual void	OnCancel		();

	//{{AFX_MSG(CHiddenWhenDlg)
	afx_msg	void	ChangeRadioSelection		();
	//}}AFX_MSG

	DECLARE_MESSAGE_MAP()
	DECLARE_DYNAMIC(CHiddenWhenDlg);
};

//===========================================================================
class TB_EXPORT CSetTooltipDlg : public CParsedDialog
{
protected:
	CExpEdit			m_edtExpr;
	SymTable*			m_pSymTable;
	Expression*&		m_pExpr;

public:
	CSetTooltipDlg	(Expression*& pExpr, SymTable* pSymTable, CWnd* = NULL);

protected:
	// standard dialog function
	virtual	BOOL	OnInitDialog	();
	virtual	void	OnOK			();
	virtual void	OnCancel		();

	//{{AFX_MSG(CSetTooltipDlg)
	//}}AFX_MSG

	DECLARE_MESSAGE_MAP()
	DECLARE_DYNAMIC(CSetTooltipDlg);
};

//===========================================================================
class TB_EXPORT CListHiddenColsDlg : public CParsedDialog
{
protected:
	WoormTable*		m_pSymTable;
	CColumnArray*		m_pColumnArrayHidden;
	CColumnArray*		m_pColumnArrayAll;	
	CMultiListBox		m_lbColumns;
	CWoormDocMng*			m_pDocument;
	Table*				m_pTable;

public:
	CListHiddenColsDlg (CColumnArray* pColumnArrayHidden, CColumnArray* pColumnArrayAll, CWoormDocMng*	pDocument,Table* pTable, CWnd* = NULL);
	
protected:
	// standard dialog function
	virtual	BOOL	OnInitDialog	();
	virtual	void	OnOK			();
	virtual void	OnCancel		();

	//{{AFX_MSG(CListHiddenColsDlg)
	afx_msg	void	EditHiddenWhen	();
	//}}AFX_MSG


	DECLARE_MESSAGE_MAP()
	DECLARE_DYNAMIC(CListHiddenColsDlg);
};

//===========================================================================
class TB_EXPORT CEditTextExprDlg : public CParsedDialog
{
protected:
	WoormTable*		m_pSymTable;
	CExpEdit			m_edtTextExpr;
	CEdit				m_TextEdit;
	CButton				m_StaticDescRadioBtn;
	CButton				m_CondDescRadioBtn;
	CString&			m_strLabel;
public:
	Expression*&		m_pExpr;

public:
	CEditTextExprDlg (CString& strLabel, Expression*&	pExpr, WoormTable*		pSymTable, CWnd* = NULL);
	
protected:
	// standard dialog function
	virtual	BOOL	OnInitDialog	();
	virtual	void	OnOK			();
	virtual void	OnCancel		();

	//{{AFX_MSG(CEditTextExprDlg)
	afx_msg	void	ChangeRadioSelection		();
	//}}AFX_MSG

	DECLARE_MESSAGE_MAP()
	DECLARE_DYNAMIC(CEditTextExprDlg);
};


//===========================================================================
class TB_EXPORT CAnchorFieldToColumnsDlg: public CParsedDialog
{
protected:
	CComboBoxExt	m_cbxTables;
	CComboBoxExt	m_cbxLeftCols;
	CComboBoxExt	m_cbxRightCols;

	CWoormDocMng*	m_pDocument;
	BaseRect*	m_pBaseRect;

	WORD		m_SelectedLeftColumnID;
	WORD		m_SelectedRightColumnID;

	Table*		m_pTable;

public:
	CAnchorFieldToColumnsDlg(CWoormDocMng* pDoc, BaseRect*	pBaseRect);

protected:
	virtual BOOL OnInitDialog	();
	virtual void OnOK			();

	//{{AFX_MSG(CAnchorFieldToColumnsDlg)
	afx_msg	void	TableChanged	();
	afx_msg	void	ColumnLeftChanged	();
	afx_msg	void	ColumnRightChanged	();
	//}}AFX_MSG

	void FillColumns(CComboBox& cbxCols, WORD& wSelectedLeftColumnID);

	DECLARE_MESSAGE_MAP()
	DECLARE_DYNAMIC(CAnchorFieldToColumnsDlg);
};

//===========================================================================
class TB_EXPORT CChooseObjectDlg: public CParsedDialog
{
protected:
	CComboBoxExt	m_cbxLayouts;
	CComboBoxExt	m_cbxTables;
	CComboBoxExt	m_cbxFieldsColumns;

	CWoormDocMng*		m_pDocument;
	CObject*		m_pCurrentObj;	//opzionale, filtra gli oggetti per escludere il corrente (copia attributi)
	BOOL			m_bChooseBaseObj;	//x Layout copy and Move Object

public:
	CLayout*	m_pSelectedLayout;
	Table*			m_pSelectedTable;
	BaseRect*		m_pSelectedBaseRect;
	TableColumn*	m_pSelectedCol;

	CObject*		m_pSelected;	//Non posso usare BaseObj perchè TableColumn non è in gerarchia

public:
	CChooseObjectDlg (CWoormDocMng* pDoc, CObject* pCurrent = NULL, BOOL bChooseBaseObj = FALSE);

protected:
	virtual BOOL OnInitDialog	();
	virtual void OnOK			();

	//{{AFX_MSG(CChooseObjectDlg)
	afx_msg	void	LayoutChanged	();
	afx_msg	void	TableChanged	();
	//}}AFX_MSG

	void FillLayouts	();
	void FillTables		(CLayout* pObjects);
	void FillFields		(CLayout* pObjects);
	void FillColumns	(CLayout* pObjects);

	DECLARE_MESSAGE_MAP()
	DECLARE_DYNAMIC(CChooseObjectDlg);
};

//===========================================================================
class TB_EXPORT CChooseLayoutDlg: public CParsedDialog
{
protected:
	CComboBoxExt	m_cbxLayouts;
	CWoormDocMng*		m_pDocument;

public:
	CLayout*	m_pSelectedLayout;

public:
	CChooseLayoutDlg (CWoormDocMng* pDoc, CWnd* parent = NULL);

protected:
	virtual BOOL OnInitDialog	();
	virtual void OnOK			();

	//{{AFX_MSG(CChooseObjectDlg)
	afx_msg	void	LayoutChanged	();
	//}}AFX_MSG

	void FillLayouts	();

	DECLARE_MESSAGE_MAP()
	DECLARE_DYNAMIC(CChooseLayoutDlg);
};

//===========================================================================
class TB_EXPORT CSetNameDlg: public CParsedDialog
{
protected:
	CWoormDocMng*	m_pDocument;
public:
	CString		m_sName;

	CSetNameDlg (CWoormDocMng*	pDocument, CWnd* aParent = NULL);

protected:
	virtual void OnOK ();
	virtual BOOL OnInitDialog();
	DECLARE_DYNAMIC(CSetNameDlg);

};

//===========================================================================
class TB_EXPORT CListStyleDlg : public CParsedDialog
{
protected:
	CMultiListBox		m_lbClasses;
	CWoormDocMng*			m_pDocument;
	CObject*			m_pObject;

public:
	CString				m_sStyleClass;

	CListStyleDlg (CWoormDocMng* pDocument, CString sClass, CObject* pObj, CWnd* parent/*= NULL*/);
	
protected:
	virtual	BOOL	OnInitDialog	();
	virtual	void	OnOK			();
	virtual void	OnCancel		();

			void	OnNew			();
			void	OnDblClick		();
	virtual	void	OnSpecialOK		();
	virtual	void	OnEmptyOK		();

	DECLARE_MESSAGE_MAP()
	DECLARE_DYNAMIC(CListStyleDlg);
};

//===========================================================================
#include "endh.dex"
