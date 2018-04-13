#include "stdafx.h"

// for manifest constant INT_MAX
#include <limits.h>
#include <stdlib.h>

#include <TbNameSolver\PathFinder.h>
#include <TbNameSolver\LoginContext.h>

#include <TbClientCore\ClientObjects.h>
#include <TbWebServicesWrappers\LoginManagerInterface.h>

#include <TbGeneric\SettingsTable.h>
#include <TbGeneric\ParametersSections.h>

#include <TbParser\TokensTable.h>

#include <TbGenlib\baseapp.h>
#include <TbGenlibUI\SettingsTableManager.h>

#include <TbWoormEngine\RepEngin.h>
#include <TbWoormEngine\RpSymTbl.h>
#include <TbWoormEngine\EdtMng.h>
#include <TbWoormEngine\MultiLayout.h>

#include "Table.h"
#include "Column.h"

#include "baseobj.h"
#include "viewpars.h"
#include "woormdoc.h"
#include "woormini.h"
#include "rectobj.h"

#include "listdlg.h"

// resources
#include "listdlg.hjson" //JSON AUTOMATIC UPDATE
#include "rectobj.hjson" //JSON AUTOMATIC UPDATE

//includere come ultimo include all'inizio del cpp
#include "begincpp.dex"

#ifdef _DEBUG
#undef THIS_FILE
static const char BASED_CODE THIS_FILE[] = __FILE__;
#endif

static const TCHAR szDefaultPrinter[]			= _T("DefaultPrinter");
static const TCHAR szPrintOnLetterhead[]		= _T("Print On Letterhead");
static const TCHAR szDefaultPrnNoBitmap[]		= _T("No Bitmaps");
static const TCHAR szDefaultPrnNoBorders[]		= _T("No Borders");
static const TCHAR szDefaultPrnShowLabels[]		= _T("Show Labels");
static const TCHAR szDefaultPrnShowTitles[]		= _T("Show Titles");
static const TCHAR szDefaultPrnUseDraftFont[]	= _T("Use Draft Font");


//==============================================================================
//          Class Options implementation
//==============================================================================
//
//------------------------------------------------------------------------------
Options::Options(CWoormDocMng* pDocument)
{
	m_pDocument = pDocument;
 
	Init();
}

//------------------------------------------------------------------------------
Options::Options(const Options& opt)
{
	*this = opt;
}

//------------------------------------------------------------------------------
void Options::operator=(const Options& opt)
{
	m_pDocument			= opt.m_pDocument;

	m_strBkgnBitmap		= opt.m_strBkgnBitmap;
	m_BitmapOrigin		= opt.m_BitmapOrigin;

	m_bPrnNoBitmap		= opt.m_bPrnNoBitmap;
	m_bPrnNoBorders		= opt.m_bPrnNoBorders;
	m_bPrnShowLabels	= opt.m_bPrnShowLabels;
	m_bPrnShowTitles	= opt.m_bPrnShowTitles;
	m_bPrnUseDraftFont	= opt.m_bPrnUseDraftFont;

	m_strDefaultPrinter			= opt.m_strDefaultPrinter;

	m_bDefaultPrnNoBitmap		= opt.m_bDefaultPrnNoBitmap;
	m_bDefaultPrnNoBorders		= opt.m_bDefaultPrnNoBorders;
	m_bDefaultPrnShowLabels		= opt.m_bDefaultPrnShowLabels;
	m_bDefaultPrnShowTitles		= opt.m_bDefaultPrnShowTitles;
	m_bDefaultPrnUseDraftFont	= opt.m_bDefaultPrnUseDraftFont;

	m_bConNoBitmap		= opt.m_bConNoBitmap;
	m_bConNoBorders		= opt.m_bConNoBorders;
	m_bConShowLabels	= opt.m_bConShowLabels;
	m_bConShowTitles	= opt.m_bConShowTitles;

	m_bCreateXsd1	= opt.m_bCreateXsd1;
	m_bCreateXsd2	= opt.m_bCreateXsd2;
}

//------------------------------------------------------------------------------
void Options::Init()
{
	m_strBkgnBitmap.Empty();

	m_strDefaultPrinter.Clear();

	m_BitmapOrigin.x	= 0;
	m_BitmapOrigin.y	= 0;

	m_bPrnNoBitmap		= FALSE;
	m_bPrnNoBorders		= FALSE;
	m_bPrnShowLabels	= TRUE;
	m_bPrnShowTitles	= TRUE;
	m_bPrnUseDraftFont	= FALSE;

	m_bDefaultPrnNoBitmap		= FALSE;
	m_bDefaultPrnNoBorders		= FALSE;
	m_bDefaultPrnShowLabels		= TRUE;
	m_bDefaultPrnShowTitles		= TRUE;
	m_bDefaultPrnUseDraftFont	= FALSE;

	m_bConNoBitmap		= FALSE;
	m_bConNoBorders		= FALSE;
	m_bConShowLabels	= TRUE;
	m_bConShowTitles	= TRUE;

	m_bCreateXsd1 = m_bCreateXsd2 = FALSE;
}

// abilita la fase di unparse, pertanto deve controllare solo i valori che scrive
// nel .wrm
//------------------------------------------------------------------------------
int Options::Changed()
{
	int counter = 0;

	if (!m_strBkgnBitmap.IsEmpty())	counter++;
	if (m_BitmapOrigin.x + m_BitmapOrigin.y) counter++;

	if (m_bPrnNoBitmap)			counter++;
	if (m_bPrnNoBorders)		counter++;
	if (!m_bPrnShowLabels)		counter++;
	if (!m_bPrnShowTitles)		counter++;
	if (m_bPrnUseDraftFont)		counter++;

	if (m_bConNoBitmap)		counter++;
	if (m_bConNoBorders)	counter++;
	if (!m_bConShowLabels)	counter++;
	if (!m_bConShowTitles)	counter++;

	if (m_bCreateXsd1 || m_bCreateXsd2)	counter++;

	return counter;
}


//------------------------------------------------------------------------------
int Options::operator == (Options& opt)
{
	return
	(
		(m_strBkgnBitmap.CompareNoCase(opt.m_strBkgnBitmap) == 0)&&
		(m_strDefaultPrinter.Str().CompareNoCase(opt.m_strDefaultPrinter.Str()) == 0)&&

		(m_BitmapOrigin	   == opt.m_BitmapOrigin)	  &&
		(m_bPrnNoBitmap    == opt.m_bPrnNoBitmap)     &&
		(m_bPrnNoBorders   == opt.m_bPrnNoBorders)    &&
		(m_bPrnShowLabels  == opt.m_bPrnShowLabels)   &&
		(m_bPrnShowTitles  == opt.m_bPrnShowTitles)   &&
		(m_bPrnUseDraftFont== opt.m_bPrnUseDraftFont) &&

		(m_bDefaultPrnNoBitmap    == opt.m_bDefaultPrnNoBitmap)     &&
		(m_bDefaultPrnNoBorders   == opt.m_bDefaultPrnNoBorders)    &&
		(m_bDefaultPrnShowLabels  == opt.m_bDefaultPrnShowLabels)   &&
		(m_bDefaultPrnShowTitles  == opt.m_bDefaultPrnShowTitles)   &&
		(m_bDefaultPrnUseDraftFont== opt.m_bDefaultPrnUseDraftFont) &&

		(m_bConNoBitmap    == opt.m_bConNoBitmap)     &&
		(m_bConNoBorders   == opt.m_bConNoBorders)    &&
		(m_bConShowLabels  == opt.m_bConShowLabels)   &&
		(m_bConShowTitles  == opt.m_bConShowTitles)	&&

		(m_bCreateXsd1	== opt.m_bCreateXsd1)&&
		(m_bCreateXsd2	== opt.m_bCreateXsd2)

	);
}

//------------------------------------------------------------------------------
int Options::operator != (Options& opt)
{
	return !(*this == opt);
}

//------------------------------------------------------------------------------
void Options::LoadDefault(const CTBNamespace& aNs)
{
	if (!aNs.IsValid()) 
		return;

	// i settings sono letti e scritti con l'owner di modulo
	CTBNamespace aModNs(CTBNamespace::MODULE, aNs.GetApplicationName() + CTBNamespace::GetSeparator() + aNs.GetModuleName());

	CString sFilename = aNs.GetObjectName() + szSettingsExt;

	DataObj* pSetting = AfxGetSettingValue(aModNs, szDefaultPrinter, szDefaultPrinter,DataStr(), sFilename);
	m_strDefaultPrinter	= pSetting ? pSetting->Str() : _T(""); 

	pSetting = AfxGetSettingValue(aModNs, szDefaultPrinter, szPrintOnLetterhead, DataBool(FALSE), sFilename);
	GetDocument()->m_bPrintOnLetterhead = pSetting ? *((DataBool*)pSetting) : FALSE;

	pSetting = AfxGetSettingValue(aModNs, szDefaultPrinter, szDefaultPrnNoBitmap, DataBool(FALSE), sFilename);
	m_bDefaultPrnNoBitmap = pSetting ? *((DataBool*) pSetting) : FALSE; 

	pSetting = AfxGetSettingValue(aModNs, szDefaultPrinter, szDefaultPrnNoBorders, DataBool(FALSE), sFilename);
	m_bDefaultPrnNoBorders = pSetting ? *((DataBool*) pSetting) : FALSE; 

	pSetting = AfxGetSettingValue(aModNs, szDefaultPrinter, szDefaultPrnShowLabels, DataBool(TRUE), sFilename);
	m_bDefaultPrnShowLabels = pSetting ? *((DataBool*) pSetting) : TRUE; 

	pSetting = AfxGetSettingValue(aModNs, szDefaultPrinter,  szDefaultPrnShowTitles, DataBool(TRUE), sFilename);
	m_bDefaultPrnShowTitles = pSetting ? *((DataBool*) pSetting) : TRUE; 

	pSetting = AfxGetSettingValue(aModNs, szDefaultPrinter, szDefaultPrnUseDraftFont, DataBool(FALSE), sFilename);
	m_bDefaultPrnUseDraftFont = pSetting ? *((DataBool*) pSetting) : FALSE; 
}

//------------------------------------------------------------------------------
void Options::SaveDefault(const CTBNamespace& aNs)
{
	if (!aNs.IsValid()) 
		return;

	CString sFilename = aNs.GetObjectName() + szSettingsExt;

	// i settings sono letti e scritti con l'owner di modulo
	CTBNamespace aModNs(CTBNamespace::MODULE, aNs.GetApplicationName() + CTBNamespace::GetSeparator() + aNs.GetModuleName());

	AfxSetSettingValue (aModNs, szDefaultPrinter, szDefaultPrinter, m_strDefaultPrinter, sFilename);
	AfxSetSettingValue (aModNs, szDefaultPrinter, szPrintOnLetterhead, GetDocument()->m_bPrintOnLetterhead, sFilename);
	AfxSetSettingValue (aModNs, szDefaultPrinter, szDefaultPrnNoBitmap,	m_bDefaultPrnNoBitmap, sFilename);
	AfxSetSettingValue (aModNs, szDefaultPrinter, szDefaultPrnNoBorders, m_bDefaultPrnNoBorders, sFilename);
	AfxSetSettingValue (aModNs, szDefaultPrinter, szDefaultPrnShowLabels, m_bDefaultPrnShowLabels, sFilename);
	AfxSetSettingValue (aModNs, szDefaultPrinter, szDefaultPrnShowTitles, m_bDefaultPrnShowTitles, sFilename);
	AfxSetSettingValue (aModNs, szDefaultPrinter, szDefaultPrnUseDraftFont,	m_bDefaultPrnUseDraftFont, sFilename);

	AfxSaveSettings(aModNs, sFilename, szDefaultPrinter);
}

//------------------------------------------------------------------------------
void Options::Unparse (ViewUnparser& ofile)
{
	// if standard value don't save
	if (Changed() == 0) return;
	BOOL blk = (Changed() > 1);

	ofile.UnparseTag    (T_OPTIONS, FALSE);
	ofile.UnparseBlank  (blk);

	// begin block
	if (blk) ofile.UnparseBegin ();

	if (!m_strBkgnBitmap.IsEmpty())
	{
		ofile.UnparseTag (T_BITMAP, FALSE);

		// salvo sempre il namespace completo
		CTBNamespace aFileNs;
		if (CTBNamespace(m_strBkgnBitmap).IsValid())
			aFileNs = m_strBkgnBitmap;
		else if (IsDosName(m_strBkgnBitmap))
			aFileNs = AfxGetPathFinder()->GetNamespaceFromPath (m_strBkgnBitmap);

		if (aFileNs.IsValid() && (aFileNs.GetType() == CTBNamespace::IMAGE || aFileNs.GetType() == CTBNamespace::FILE))
			ofile.UnparseString(aFileNs.ToString(), FALSE);
		else
			ofile.UnparseString(m_strBkgnBitmap, FALSE);

		// se size diversa da 0,0,0,0 la unparsa altrimenti lascia come'
		if (m_BitmapOrigin.x + m_BitmapOrigin.y)
			ofile.UnparseOrigin (m_BitmapOrigin);
	}

	if (m_bPrnNoBitmap)		ofile.UnparseTag (T_NO_PRN_BKGN_BITMAP);
	if (m_bPrnNoBorders)	ofile.UnparseTag (T_NO_PRN_BORDERS);
	if (!m_bPrnShowLabels)	ofile.UnparseTag (T_NO_PRN_LABELS);
	if (!m_bPrnShowTitles)	ofile.UnparseTag (T_NO_PRN_TITLES);
	if (m_bPrnUseDraftFont)	ofile.UnparseTag (T_USE_DRAFT_FONT);

	if (m_bConNoBitmap)		ofile.UnparseTag (T_NO_CON_BKGN_BITMAP);
	if (m_bConNoBorders)	ofile.UnparseTag (T_NO_CON_BORDERS);
	if (!m_bConShowLabels)	ofile.UnparseTag (T_NO_CON_LABELS);
	if (!m_bConShowTitles)	ofile.UnparseTag (T_NO_CON_TITLES);
	if (m_bCreateXsd1 || m_bCreateXsd2)	
	{ 
		ofile.UnparseTag (T_CREATE_XSD, FALSE);
		if (m_bCreateXsd2)
		{
			ofile.UnparseInt (2);
		}
		else
			ofile.UnparseCrLf();
	}

	// write endig END block if needed
	if (blk) ofile.UnparseEnd();
	ofile.UnparseCrLf();
}


//------------------------------------------------------------------------------
BOOL Options::ParseOption (ViewParser& lex, BOOL blk)
{
	BOOL ok = TRUE;

	do
	{
		switch (lex.LookAhead())
		{
			case T_BITMAP: 
				{
					ok = lex.SkipToken() && lex.ParseString(m_strBkgnBitmap);

					if (ok && lex.LookAhead(T_ORIGIN))
						ok = lex.ParseOrigin(m_BitmapOrigin);
					break;
				}

			case T_EOF               : lex.SetError (_TB("Unexpected end file")); ok = FALSE;		break;
			case T_NO_PRN_BKGN_BITMAP: ok = lex.SkipToken(); m_bPrnNoBitmap     = TRUE;		break;
			case T_NO_PRN_BORDERS    : ok = lex.SkipToken(); m_bPrnNoBorders    = TRUE;		break;
			case T_NO_PRN_LABELS     : ok = lex.SkipToken(); m_bPrnShowLabels   = FALSE;	break;
			case T_NO_PRN_TITLES     : ok = lex.SkipToken(); m_bPrnShowTitles   = FALSE;	break;
			case T_USE_DRAFT_FONT	 : ok = lex.SkipToken(); m_bPrnUseDraftFont = TRUE;		break;

			case T_NO_CON_BKGN_BITMAP: ok = lex.SkipToken(); m_bConNoBitmap     = TRUE;		break;
			case T_NO_CON_BORDERS    : ok = lex.SkipToken(); m_bConNoBorders    = TRUE;		break;
			case T_NO_CON_LABELS     : ok = lex.SkipToken(); m_bConShowLabels   = FALSE;	break;
			case T_NO_CON_TITLES     : ok = lex.SkipToken(); m_bConShowTitles   = FALSE;	break;

			case T_CREATE_XSD		: 
				{
					int rel = 1;
					ok = lex.SkipToken(); 
					if (lex.LookAhead(T_INT))
						ok = lex.ParseInt(rel);
					if (rel == 2)
						m_bCreateXsd2   = TRUE;
					else
						m_bCreateXsd1   = TRUE;	
					break;
				}

			case T_END :
				if (blk) return ok;
				lex.SetError(_TB("Unexpected END"));
				return FALSE;


			default :
				if (blk)
				{
					lex.SetError(_TB("END not found in Options parse"));
					ok = FALSE;
				}
		}
	}
	while (ok && blk);

	return ok;
}

//------------------------------------------------------------------------------
BOOL Options::ParseOptions (ViewParser& lex)
{
	BOOL ok = TRUE;

	do { ok = ParseOption(lex) && !lex.Bad() && !lex.Eof(); }
	while (ok && !lex.LookAhead(T_END));

	return ok;
}


//------------------------------------------------------------------------------
BOOL Options::ParseBlock(ViewParser& lex)
{
	if (lex.LookAhead(T_BEGIN))
		return
			lex.ParseBegin  ()      &&
			ParseOptions    (lex)   &&
			lex.ParseEnd    ();

	return ParseOption(lex, FALSE);
}

//------------------------------------------------------------------------------
BOOL Options::Parse (ViewParser& lex)
{
	// don't exist option section so standrad value
	BOOL ok =TRUE;

	if (lex.LookAhead(T_OPTIONS))
		ok =
			lex.ParseTag(T_OPTIONS) &&
			ParseBlock  (lex);

	return ok;
}

//------------------------------------------------------------------------------
BOOL Options::NoBitmap(CWoormDocMng* pDocument, BOOL bPrinting)
{
	if (bPrinting)
		return (!m_strDefaultPrinter.IsEmpty() && pDocument->m_bPrintingOnDefault) 
			? m_bDefaultPrnNoBitmap
			: m_bPrnNoBitmap;

	return m_bConNoBitmap;
}

//------------------------------------------------------------------------------
BOOL Options::NoBorders	(CWoormDocMng* pDocument, BOOL bPrinting)
{
	if (bPrinting)
		return (!m_strDefaultPrinter.IsEmpty() && pDocument->m_bPrintingOnDefault) 
			? m_bDefaultPrnNoBorders
			: m_bPrnNoBorders;

	return m_bConNoBorders;
}


//------------------------------------------------------------------------------
BOOL Options::ShowTitles(CWoormDocMng* pDocument, BOOL bPrinting)
{
	if (bPrinting)
		return (!m_strDefaultPrinter.IsEmpty() && pDocument->m_bPrintingOnDefault) 
			? m_bDefaultPrnShowTitles
			: m_bPrnShowTitles;

	return m_bConShowTitles;
}

//------------------------------------------------------------------------------
BOOL Options::ShowLabels(CWoormDocMng* pDocument, BOOL bPrinting)
{
	if (bPrinting)
		return (!m_strDefaultPrinter.IsEmpty() && pDocument->m_bPrintingOnDefault) 
			? m_bDefaultPrnShowLabels
			: m_bPrnShowLabels;

	return m_bConShowLabels;
}

//------------------------------------------------------------------------------
BOOL Options::UseDraftFont(CWoormDocMng* pDocument, BOOL bPrinting)
{
	if (bPrinting && !pDocument->m_bIsExporting)
		return (!m_strDefaultPrinter.IsEmpty() && pDocument->m_bPrintingOnDefault) 
			? m_bDefaultPrnUseDraftFont
			: m_bPrnUseDraftFont;

	return FALSE;
}

//==============================================================================
//          Class CTextDlg implementation
//==============================================================================

#define UNLIMITED_TEXT  0
IMPLEMENT_DYNAMIC(CTextDlg, CParsedDialog)
//------------------------------------------------------------------------------
CTextDlg::CTextDlg (CString& aText, CWnd* aParent)
	:
	CParsedDialog	(IDD_SINGLELINE_TEXT, aParent),
	m_strText		(aText),
	m_nSize			(UNLIMITED_TEXT)
{
}

//------------------------------------------------------------------------------
BOOL CTextDlg::OnInitDialog()
{
	CParsedDialog::OnInitDialog();

	m_TextEdit.SubclassDlgItem(IDC_TEXTDATA, this);
	m_TextEdit.SetWindowText(m_strText);

	return TRUE;  // return TRUE  unless you set the focus to a control
}

//------------------------------------------------------------------------------
void CTextDlg::limitTextSize(int aSize)
{
	m_nSize = aSize;
	if (m_nSize) m_TextEdit.LimitText(m_nSize);
}

//------------------------------------------------------------------------------
void CTextDlg::OnOK()
{
	::GetDlgItemText(this, IDC_TEXTDATA, m_strText);
	EndDialog(IDOK);
}
//==============================================================================
//          Class CSpecialField implementation
//==============================================================================
IMPLEMENT_DYNAMIC(CSpecialField, CObject)

//==============================================================================
//          Class CHtmlTag implementation
//==============================================================================
IMPLEMENT_DYNAMIC(CHtmlTag, CObject)

CString CHtmlTag::GetHtmlTag()
{
	return GetHtmlTag(m_eHtmlTag);
}

CString CHtmlTag::GetHtmlName()
{
	return GetHtmlName(m_eHtmlTag);
}

CString CHtmlTag::GetDescription()
{
	return GetDescription(m_eHtmlTag);
}

CString CHtmlTag::GetExample()
{
	return GetExample(m_eHtmlTag);

}

BOOL CHtmlTag::IsAttribute()
{
	return IsAttribute(m_eHtmlTag);
}
BOOL CHtmlTag::IsStyleProperty()
{
	return IsStyleProperty(m_eHtmlTag);
}

CString CHtmlTag::GetHtmlFragment()
{
	return GetHtmlFragment(m_eHtmlTag);
}

//==============================================================================
//          Class CQueryTag implementation
//==============================================================================
IMPLEMENT_DYNAMIC(CQueryTag, CObject)

CString CQueryTag::GetTag()
{
	return GetTag(m_eTag);
}

CString CQueryTag::GetName()
{
	return GetName(m_eTag);
}

CString CQueryTag::GetDescription()
{
	return GetDescription(m_eTag);
}

CString CQueryTag::GetExample()
{
	return GetExample(m_eTag);

}

BOOL CQueryTag::IsAttribute()
{
	return IsAttribute(m_eTag);
}
BOOL CQueryTag::IsStyleProperty()
{
	return IsStyleProperty(m_eTag);
}

CString CQueryTag::GetFragment()
{
	return GetFragment(m_eTag);
}


//==============================================================================
//          Class CSpecialTextDlg implementation
//==============================================================================

BEGIN_MESSAGE_MAP(CSpecialTextDlg, CParsedDialog)
	//{{AFX_MSG_MAP(CSpecialTextDlg)
	ON_LBN_DBLCLK		(IDC_SPECIAL_FIELDS,	OnSpecialSelected)
	//}}AFX_MSG_MAP
END_MESSAGE_MAP()
IMPLEMENT_DYNAMIC(CSpecialTextDlg, CParsedDialog)

//------------------------------------------------------------------------------
CSpecialTextDlg::CSpecialTextDlg (CString& aText, BOOL& bSpecialField, CWnd* aParent)
	:
	CParsedDialog	(IDD_SPECIAL_FIELD, aParent),
	m_strText		(aText),
	m_bSpecialField	(bSpecialField)
{
}

#define BEGIN_ADD_SPECIAL() 
#define ADD_SPECIAL(a) m_ListBox.SetItemData(m_ListBox.AddString(CSpecialField(CSpecialField::a).GetDescription()), CSpecialField::a);
#define END_ADD_SPECIAL() 

//------------------------------------------------------------------------------
BOOL CSpecialTextDlg::OnInitDialog()
{
	CParsedDialog::OnInitDialog();

	m_TextEdit.SubclassDlgItem(IDC_TEXTDATA, this);
	m_ListBox.SubclassDlgItem(IDC_SPECIAL_FIELDS, this);
	m_TextEdit.SetWindowText(m_strText);

	BEGIN_ADD_SPECIAL();
		ADD_SPECIAL(SPECIAL_PAGE);
		ADD_SPECIAL(SPECIAL_TOT_PAGE);
		ADD_SPECIAL(SPECIAL_PAGE_SPLITTER);
		ADD_SPECIAL(SPECIAL_SPLITTER);
		ADD_SPECIAL(SPECIAL_TOT_SPLITTER);
		ADD_SPECIAL(SPECIAL_APPDATE);
		ADD_SPECIAL(SPECIAL_TODAY);
		ADD_SPECIAL(SPECIAL_TODAY2);
		ADD_SPECIAL(SPECIAL_YEAR);
		ADD_SPECIAL(SPECIAL_MONTH);
		ADD_SPECIAL(SPECIAL_MONTH2);
		ADD_SPECIAL(SPECIAL_DAY);
		ADD_SPECIAL(SPECIAL_HH_MM);
		ADD_SPECIAL(SPECIAL_HH);
		ADD_SPECIAL(SPECIAL_MM);
		ADD_SPECIAL(SPECIAL_SEC);
		ADD_SPECIAL(SPECIAL_USER);
		ADD_SPECIAL(SPECIAL_LOGINUSER);
		ADD_SPECIAL(SPECIAL_COMPUTER);
		ADD_SPECIAL(SPECIAL_APP_TITLE);
		ADD_SPECIAL(SPECIAL_APP_REL);
		ADD_SPECIAL(SPECIAL_TB_REL);
		ADD_SPECIAL(SPECIAL_SYS_COMPANY_NAME);
		ADD_SPECIAL(SPECIAL_LICENSEE);
		ADD_SPECIAL(SPECIAL_REPORTNAME);
		ADD_SPECIAL(SPECIAL_REPORT_TITLE);
		ADD_SPECIAL(SPECIAL_REPORT_SUBJECT);
		ADD_SPECIAL(SPECIAL_REPORT_AUTHOR);
		ADD_SPECIAL(SPECIAL_REPORT_COMPANY);
		ADD_SPECIAL(SPECIAL_REPORT_COMMENTS);
		ADD_SPECIAL(SPECIAL_DB_COMPANY_NAME);
		ADD_SPECIAL(SPECIAL_PRODUCER_NAME);
		ADD_SPECIAL(SPECIAL_PRODUCT_DATE);
		ADD_SPECIAL(SPECIAL_EVAL_EXPR);
	END_ADD_SPECIAL();

	return TRUE;  // return TRUE  unless you set the focus to a control
}


// Aggiunge il token selezionato nella corrente posizione dell'edit e tra {}
//------------------------------------------------------------------------------
void CSpecialTextDlg::OnSpecialSelected()
{
	int nCurPos = m_ListBox.GetCurSel();
	if (nCurPos == LB_ERR)
		return;

	CString strBuffer;
	CSpecialField::ESpecialField item = (CSpecialField::ESpecialField)m_ListBox.GetItemData (nCurPos);
	strBuffer = CSpecialField(item).GetKeyword();

	CString strToken = SPECIAL_FIELD_SEP_START + strBuffer + SPECIAL_FIELD_SEP_END;
	m_TextEdit.ReplaceSel(strToken);
	m_bSpecialField = TRUE;
}

//------------------------------------------------------------------------------
void CSpecialTextDlg::OnOK()
{
	::GetDlgItemText(this, IDC_TEXTDATA, m_strText);

	m_bSpecialField = m_strText.Find('{') >= 0  && m_strText.Find('}') > 1 ;

	EndDialog(IDOK);
}

//==============================================================================
//          Class CBordersDlg implementation
//==============================================================================
BEGIN_MESSAGE_MAP(CBordersDlg, CParsedDialog)
	//{{AFX_MSG_MAP(CBordersDlg)
	//}}AFX_MSG_MAP
END_MESSAGE_MAP()
IMPLEMENT_DYNAMIC(CBordersDlg, CParsedDialog)
//------------------------------------------------------------------------------
CBordersDlg::CBordersDlg (Borders& aBorders, CWnd* aParent)
	:
	CParsedDialog	(IDD_BORDERS, aParent),
	m_Borders	(aBorders)
{
}


//------------------------------------------------------------------------------
void CBordersDlg::OnOK()
{
	m_Borders.left    = IsDlgButtonChecked(IDC_BORDER_LEFT);
	m_Borders.right   = IsDlgButtonChecked(IDC_BORDER_RIGHT);
	m_Borders.top     = IsDlgButtonChecked(IDC_BORDER_TOP);
	m_Borders.bottom  = IsDlgButtonChecked(IDC_BORDER_BOTTOM);

	EndDialog(IDOK);
}


//------------------------------------------------------------------------------
BOOL CBordersDlg::OnInitDialog()
{
	CParsedDialog::OnInitDialog();

	CheckDlgButton(IDC_BORDER_LEFT,     m_Borders.left);
	CheckDlgButton(IDC_BORDER_RIGHT,    m_Borders.right);
	CheckDlgButton(IDC_BORDER_TOP,      m_Borders.top);
	CheckDlgButton(IDC_BORDER_BOTTOM,   m_Borders.bottom);

	return TRUE;  // return TRUE  unless you set the focus to a control
}

//==============================================================================
//          Class CTableBordersDlg implementation
//==============================================================================
BEGIN_MESSAGE_MAP(CTableBordersDlg, CParsedDialog)
	//{{AFX_MSG_MAP(CTableBordersDlg)
	ON_CONTROL      (BN_CLICKED,        IDC_SET_CUSTOM_ROWSEP_PEN,		OnSetCustomPen)
	ON_CONTROL      (BN_CLICKED,        IDC_REMOVE_CUSTOM_ROWSEP_PEN,	OnRemoveCustomPen)
	//}}AFX_MSG_MAP
END_MESSAGE_MAP()

IMPLEMENT_DYNAMIC(CTableBordersDlg, CParsedDialog)
//------------------------------------------------------------------------------
CTableBordersDlg::CTableBordersDlg (Table* pTable, TableBorders& aTableBorders, CWnd* aParent)
	:
	CParsedDialog	(IDD_TABLE_BORDERS, aParent),
	m_Borders		(aTableBorders),
	m_pTable		(pTable)
{
}

//------------------------------------------------------------------------------
BOOL CTableBordersDlg::OnInitDialog()
{
	CParsedDialog::OnInitDialog();

	CheckDlgButton(IDC_TABLETITLE_TOP,      m_Borders.m_bTableTitleTop);
	CheckDlgButton(IDC_TABLETITLE_BOTTOM,   m_Borders.m_bTableTitleBottom);
	CheckDlgButton(IDC_TABLETITLE_LEFT,     m_Borders.m_bTableTitleLeft);
	CheckDlgButton(IDC_TABLETITLE_RIGHT,    m_Borders.m_bTableTitleRight);

	CheckDlgButton(IDC_COLUMNTITLE_TOP,         m_Borders.m_bColumnTitleTop);
	CheckDlgButton(IDC_COLUMNTITLE_BOTTOM,      m_Borders.m_bColumnTitleBottom);
	CheckDlgButton(IDC_COLUMNTITLE_LEFT,        m_Borders.m_bColumnTitleLeft);
	CheckDlgButton(IDC_COLUMNTITLE_RIGHT,       m_Borders.m_bColumnTitleRight);

	CheckDlgButton(IDC_COLUMNTITLE_SEPARATOR,   m_Borders.m_bColumnTitleSeparator);
	CheckDlgButton(IDC_COLUMN_SEPARATOR,    m_Borders.m_bColumnSeparator);
	CheckDlgButton(IDC_ROW_SEPARATOR,       m_Borders.m_bRowSeparator);

	CheckDlgButton(IDC_BODY_TOP,            m_Borders.m_bBodyTop);
	CheckDlgButton(IDC_BODY_LEFT,           m_Borders.m_bBodyLeft);
	CheckDlgButton(IDC_BODY_BOTTOM,         m_Borders.m_bBodyBottom);
	CheckDlgButton(IDC_BODY_RIGHT,          m_Borders.m_bBodyRight);

	CheckDlgButton(IDC_TOTAL_LEFT,          m_Borders.m_bTotalLeft);
	CheckDlgButton(IDC_TOTAL_BOTTOM,        m_Borders.m_bTotalBottom);
	CheckDlgButton(IDC_TOTAL_TOP,			m_Borders.m_bTotalTop);
	CheckDlgButton(IDC_TOTAL_RIGHT,         m_Borders.m_bTotalRight);

	if (m_Borders.m_pRowSepPen == NULL)
	{
		GetDlgItem(IDC_REMOVE_CUSTOM_ROWSEP_PEN)->EnableWindow(FALSE);
	}
	return TRUE;  // return TRUE  unless you set the focus to a control
}

//-----------------------------------------------------------------------------
void CTableBordersDlg::OnSetCustomPen()
{
	BorderPen* pen = new BorderPen(m_Borders.m_pRowSepPen ? *m_Borders.m_pRowSepPen : m_pTable->GetColumnPen(0));

	CLineWidthDlg dlg(*pen, this);
	if (dlg.DoModal() == IDOK)
	{
		SAFE_DELETE(m_Borders.m_pRowSepPen);
		m_Borders.m_pRowSepPen = pen;

		GetDlgItem(IDC_REMOVE_CUSTOM_ROWSEP_PEN)->EnableWindow(TRUE);

		m_pTable->m_pDocument->InvalidateRect(m_pTable->GetBaseRect(), m_pTable->IsTransparent());
		m_pTable->m_pDocument->UpdateWindow();
		m_pTable->m_pDocument->SetModifiedFlag();
	}
	else
		delete pen;
}

//-----------------------------------------------------------------------------
void CTableBordersDlg::OnRemoveCustomPen()
{
	if (m_Borders.m_pRowSepPen)
	{
		SAFE_DELETE(m_Borders.m_pRowSepPen);

		GetDlgItem(IDC_REMOVE_CUSTOM_ROWSEP_PEN)->EnableWindow(FALSE);

		m_pTable->m_pDocument->InvalidateRect(m_pTable->GetBaseRect(), m_pTable->IsTransparent());
		m_pTable->m_pDocument->UpdateWindow();
		m_pTable->m_pDocument->SetModifiedFlag();
	}
}

//------------------------------------------------------------------------------
void CTableBordersDlg::OnOK()
{
	m_Borders.m_bTableTitleTop  = IsDlgButtonChecked(IDC_TABLETITLE_TOP);
	m_Borders.m_bTableTitleBottom  = IsDlgButtonChecked(IDC_TABLETITLE_BOTTOM);
	m_Borders.m_bTableTitleLeft = IsDlgButtonChecked(IDC_TABLETITLE_LEFT);
	m_Borders.m_bTableTitleRight = IsDlgButtonChecked(IDC_TABLETITLE_RIGHT);

	m_Borders.m_bColumnTitleTop   = IsDlgButtonChecked(IDC_COLUMNTITLE_TOP);
	m_Borders.m_bColumnTitleBottom   = IsDlgButtonChecked(IDC_COLUMNTITLE_BOTTOM);
	m_Borders.m_bColumnTitleLeft  = IsDlgButtonChecked(IDC_COLUMNTITLE_LEFT);
	m_Borders.m_bColumnTitleRight = IsDlgButtonChecked(IDC_COLUMNTITLE_RIGHT);

	m_Borders.m_bColumnTitleSeparator = IsDlgButtonChecked(IDC_COLUMNTITLE_SEPARATOR);
	m_Borders.m_bRowSeparator         = IsDlgButtonChecked(IDC_ROW_SEPARATOR);
	m_Borders.m_bColumnSeparator      = IsDlgButtonChecked(IDC_COLUMN_SEPARATOR);

	m_Borders.m_bBodyTop     = IsDlgButtonChecked(IDC_BODY_TOP);
	m_Borders.m_bBodyLeft    = IsDlgButtonChecked(IDC_BODY_LEFT);
	m_Borders.m_bBodyBottom  = IsDlgButtonChecked(IDC_BODY_BOTTOM);
	m_Borders.m_bBodyRight   = IsDlgButtonChecked(IDC_BODY_RIGHT);

	m_Borders.m_bTotalLeft   = IsDlgButtonChecked(IDC_TOTAL_LEFT);
	m_Borders.m_bTotalBottom = IsDlgButtonChecked(IDC_TOTAL_BOTTOM);
	m_Borders.m_bTotalTop	 = IsDlgButtonChecked(IDC_TOTAL_TOP);
	m_Borders.m_bTotalRight  = IsDlgButtonChecked(IDC_TOTAL_RIGHT);

	EndDialog(IDOK);
}

//==============================================================================
//          Class COptionsDlg implementation
//==============================================================================
BEGIN_MESSAGE_MAP(COptionsDlg, CParsedDialog)
	//{{AFX_MSG_MAP(COptionsDlg)
	ON_BN_CLICKED  (IDC_BKGN_BITMAP_CENTER_X,	OnBitmapCenterX)
	ON_BN_CLICKED  (IDC_BKGN_BITMAP_CENTER_Y,	OnBitmapCenterY)
	ON_BN_CLICKED  (IDC_PRINT_DRAFT,			OnPrintDraft)
	ON_BN_CLICKED  (IDC_DEFAULT_PRINT_DRAFT,	OnDefaultPrintDraft)
	ON_BN_CLICKED  (IDC_CREATEXSD1,				OnCheckCreateXsd1)
	ON_BN_CLICKED  (IDC_CREATEXSD2,				OnCheckCreateXsd2)
	//}}AFX_MSG_MAP
END_MESSAGE_MAP()

IMPLEMENT_DYNAMIC(COptionsDlg, CParsedDialog)
//------------------------------------------------------------------------------
COptionsDlg::COptionsDlg (Options& aOptions, CWoormDocMng* pDocument, CWnd* aParent)
	:
	CParsedDialog	(IDD_OPTIONS, aParent),
	m_pDocument		(pDocument),
	m_Options		(aOptions),
	m_BitmapX		(BTN_SPIN_ID),
	m_BitmapY		(BTN_SPIN_ID)
{
}

//------------------------------------------------------------------------------
void COptionsDlg::EnableControls()
{
	BOOL bEnable = m_pDocument->m_bAllowEditing;

	// bitmap
	m_NamespaceEdit.EnableWindow(bEnable);

	m_BitmapX. EnableWindow(bEnable);
	m_BitmapY. EnableWindow(bEnable);

	// video controls
	GetDlgItem(IDC_CON_NO_BITMAP)->		EnableWindow(bEnable);
	GetDlgItem(IDC_CON_NO_BORDERS)->	EnableWindow(bEnable);
	GetDlgItem(IDC_CON_SHOW_LABELS)->	EnableWindow(bEnable);
	GetDlgItem(IDC_CON_SHOW_TITLES)->	EnableWindow(bEnable);

	// bottoni vari di settaggio valori
	GetDlgItem(IDC_BKGN_BITMAP_CENTER_X)->	EnableWindow(bEnable);
	GetDlgItem(IDC_BKGN_BITMAP_CENTER_Y)->	EnableWindow(bEnable);
}


// carica le stampanti installate e seleziona quella preferenziale se la stessa
// e' presente tra quelle selezionate
//------------------------------------------------------------------------------
void COptionsDlg::LoadPrinters()
{
	m_Options.LoadDefault(m_pDocument->GetNamespace());
	CStringArray aPrinters;

	// recupera l'elenco delle stampanti e se quella di default non esiste
	// piu' (potrebbe essere stata rimossa dal control panel) allora la
	// elimina.
	int nPos = GetPrinterNames(aPrinters, m_Options.m_strDefaultPrinter.Str());
	if (nPos == -1) m_Options.m_strDefaultPrinter.Clear();

	m_PrintersCombo.AddString(_TB("No printer"));
	for (int i = 0; i <= aPrinters.GetUpperBound(); i++)
		m_PrintersCombo.AddString(aPrinters[i]);

	m_PrintersCombo.SetCurSel(nPos + 1);
}

//------------------------------------------------------------------------------
void COptionsDlg::SavePrinters()
{
	int nPos = m_PrintersCombo.GetCurSel();
	if (nPos == CB_ERR || nPos == 0)
		m_Options.m_strDefaultPrinter.Clear();
	else
	{
		CString sPrinter;
		m_PrintersCombo.GetLBText(nPos, sPrinter);
		m_Options.m_strDefaultPrinter = sPrinter;
	}
	m_Options.SaveDefault(m_pDocument->GetNamespace());
}

//------------------------------------------------------------------------------
BOOL COptionsDlg::OnInitDialog()
{
	CParsedDialog::OnInitDialog();

	// nome (aliasato) del bitmap di sfondo
	if (!m_NamespaceEdit.SubclassEdit(IDC_BKGN_BITMAP, this)) return FALSE;
	m_NamespaceEdit.SetNamespaceType(CTBNamespace::IMAGE);
	m_NamespaceEdit.SetValue(m_Options.m_strBkgnBitmap);

	if (!m_BitmapX. SubclassEdit(IDC_BKGN_BITMAP_X,  this)) return FALSE;
	if (!m_BitmapY. SubclassEdit(IDC_BKGN_BITMAP_Y,  this)) return FALSE;

	//m_BkgnBitmapEdit.SetValue(m_Options.m_strBkgnBitmap);
	m_BitmapX. SetValue(m_Options.m_BitmapOrigin.x);
	m_BitmapY. SetValue(m_Options.m_BitmapOrigin.y);

	// nome della stampante di default se esiste
	if (!m_PrintersCombo.SubclassDlgItem(IDC_DEFAULT_PRINTER, this)) return FALSE;

	// generic default for default printer
	CheckDlgButton(IDC_DEFAULT_PRN_NO_BITMAP,   m_Options.m_bDefaultPrnNoBitmap);
	CheckDlgButton(IDC_DEFAULT_PRN_NO_BORDERS,  m_Options.m_bDefaultPrnNoBorders);
	CheckDlgButton(IDC_DEFAULT_PRN_SHOW_LABELS, m_Options.m_bDefaultPrnShowLabels);
	CheckDlgButton(IDC_DEFAULT_PRN_SHOW_TITLES, m_Options.m_bDefaultPrnShowTitles);
	CheckDlgButton(IDC_DEFAULT_PRN_DRAFT_FONT,  m_Options.m_bDefaultPrnUseDraftFont);

	// generic default for printer
	CheckDlgButton(IDC_PRN_NO_BITMAP,   m_Options.m_bPrnNoBitmap);
	CheckDlgButton(IDC_PRN_NO_BORDERS,  m_Options.m_bPrnNoBorders);
	CheckDlgButton(IDC_PRN_SHOW_LABELS, m_Options.m_bPrnShowLabels);
	CheckDlgButton(IDC_PRN_SHOW_TITLES, m_Options.m_bPrnShowTitles);
	CheckDlgButton(IDC_PRN_DRAFT_FONT,  m_Options.m_bPrnUseDraftFont);

	// generic default for video
	CheckDlgButton(IDC_CON_NO_BITMAP,   m_Options.m_bConNoBitmap);
	CheckDlgButton(IDC_CON_NO_BORDERS,  m_Options.m_bConNoBorders);
	CheckDlgButton(IDC_CON_SHOW_LABELS, m_Options.m_bConShowLabels);
	CheckDlgButton(IDC_CON_SHOW_TITLES, m_Options.m_bConShowTitles);

	CheckDlgButton(IDC_CREATEXSD1, m_Options.m_bCreateXsd1);
	CheckDlgButton(IDC_CREATEXSD2, m_Options.m_bCreateXsd2);

	// abilita/disabilita i controlli se sono in editng o no e carica le 
	// stampanti installate nel computer nella apposita combo
	EnableControls();
	LoadPrinters();

	GetDlgItem(IDC_CREATEXSD1)->ShowWindow(AfxIsActivated(CLIENTNET_APP, MAGICPANE_MOD) ? SW_SHOW : SW_HIDE);
	GetDlgItem(IDC_CREATEXSD2)->ShowWindow(AfxIsActivated(CLIENTNET_APP, MAGICPANE_MOD) ? SW_SHOW : SW_HIDE);

	return TRUE;  // return TRUE  unless you set the focus to a control
}


//------------------------------------------------------------------------------
void COptionsDlg::CenterBitmap(BOOL bX)
{
	CString strName (m_NamespaceEdit.GetValue());
	if (strName.IsEmpty())
		return;

	CTBPicture	bitmap;
	VERIFY (bitmap.ReadFile (strName, TRUE));

	CSize sizePage = m_pDocument->m_PageInfo.GetPageSize_LP();
	CSize sizeBitmap(bitmap.GetWidth(), bitmap.GetHeight());

	if (bX)
		m_BitmapX. SetValue((sizePage.cx - sizeBitmap.cx) / 2);
	else
		m_BitmapY. SetValue((sizePage.cy - sizeBitmap.cy) / 2);
}

//------------------------------------------------------------------------------
void COptionsDlg::OnBitmapCenterX()	{ CenterBitmap(TRUE); }
void COptionsDlg::OnBitmapCenterY()	{ CenterBitmap(FALSE); }

//------------------------------------------------------------------------------
void COptionsDlg::OnPrintDraft()
{
	m_Options.m_bPrnNoBitmap	= TRUE;
	m_Options.m_bPrnNoBorders	= TRUE;
	m_Options.m_bPrnShowLabels	= FALSE;
	m_Options.m_bPrnShowTitles	= FALSE;
	m_Options.m_bPrnUseDraftFont= TRUE;

	CheckDlgButton(IDC_PRN_NO_BITMAP,   m_Options.m_bPrnNoBitmap);
	CheckDlgButton(IDC_PRN_NO_BORDERS,  m_Options.m_bPrnNoBorders);
	CheckDlgButton(IDC_PRN_SHOW_LABELS, m_Options.m_bPrnShowLabels);
	CheckDlgButton(IDC_PRN_SHOW_TITLES, m_Options.m_bPrnShowTitles);
	CheckDlgButton(IDC_PRN_DRAFT_FONT,  m_Options.m_bPrnUseDraftFont);
}

//------------------------------------------------------------------------------
void COptionsDlg::OnDefaultPrintDraft()
{
	m_Options.m_bDefaultPrnNoBitmap		= TRUE;
	m_Options.m_bDefaultPrnNoBorders	= TRUE;
	m_Options.m_bDefaultPrnShowLabels	= FALSE;
	m_Options.m_bDefaultPrnShowTitles	= FALSE;
	m_Options.m_bDefaultPrnUseDraftFont	= TRUE;

	CheckDlgButton(IDC_DEFAULT_PRN_NO_BITMAP,   m_Options.m_bDefaultPrnNoBitmap);
	CheckDlgButton(IDC_DEFAULT_PRN_NO_BORDERS,  m_Options.m_bDefaultPrnNoBorders);
	CheckDlgButton(IDC_DEFAULT_PRN_SHOW_LABELS, m_Options.m_bDefaultPrnShowLabels);
	CheckDlgButton(IDC_DEFAULT_PRN_SHOW_TITLES, m_Options.m_bDefaultPrnShowTitles);
	CheckDlgButton(IDC_DEFAULT_PRN_DRAFT_FONT,  m_Options.m_bDefaultPrnUseDraftFont);
}

//------------------------------------------------------------------------------
void COptionsDlg::OnCheckCreateXsd1()
{
	if (IsDlgButtonChecked(IDC_CREATEXSD1))
		CheckDlgButton(IDC_CREATEXSD2,   0);
}

//------------------------------------------------------------------------------
void COptionsDlg::OnCheckCreateXsd2()
{
	if (IsDlgButtonChecked(IDC_CREATEXSD2))
		CheckDlgButton(IDC_CREATEXSD1,   0);
}

//------------------------------------------------------------------------------
void COptionsDlg::OnOK()
{
	// Bitmap di sfondo
	m_Options.m_strBkgnBitmap = m_NamespaceEdit.GetValue();
	// origine del bitmap di sfondo
	m_Options.m_BitmapOrigin.x	= m_BitmapX. GetValue();
	m_Options.m_BitmapOrigin.y	= m_BitmapY. GetValue();

	// generic default for default printer
	m_Options.m_bDefaultPrnNoBitmap     = IsDlgButtonChecked(IDC_DEFAULT_PRN_NO_BITMAP);
	m_Options.m_bDefaultPrnNoBorders    = IsDlgButtonChecked(IDC_DEFAULT_PRN_NO_BORDERS);
	m_Options.m_bDefaultPrnShowLabels   = IsDlgButtonChecked(IDC_DEFAULT_PRN_SHOW_LABELS);
	m_Options.m_bDefaultPrnShowTitles   = IsDlgButtonChecked(IDC_DEFAULT_PRN_SHOW_TITLES);
	m_Options.m_bDefaultPrnUseDraftFont = IsDlgButtonChecked(IDC_DEFAULT_PRN_DRAFT_FONT);

	// generic default for printer
	m_Options.m_bPrnNoBitmap     = IsDlgButtonChecked(IDC_PRN_NO_BITMAP);
	m_Options.m_bPrnNoBorders    = IsDlgButtonChecked(IDC_PRN_NO_BORDERS);
	m_Options.m_bPrnShowLabels   = IsDlgButtonChecked(IDC_PRN_SHOW_LABELS);
	m_Options.m_bPrnShowTitles   = IsDlgButtonChecked(IDC_PRN_SHOW_TITLES);
	m_Options.m_bPrnUseDraftFont = IsDlgButtonChecked(IDC_PRN_DRAFT_FONT);

	// generic default for video
	m_Options.m_bConNoBitmap     = IsDlgButtonChecked(IDC_CON_NO_BITMAP);
	m_Options.m_bConNoBorders    = IsDlgButtonChecked(IDC_CON_NO_BORDERS);
	m_Options.m_bConShowLabels   = IsDlgButtonChecked(IDC_CON_SHOW_LABELS);
	m_Options.m_bConShowTitles   = IsDlgButtonChecked(IDC_CON_SHOW_TITLES);

	m_Options.m_bCreateXsd1		 = IsDlgButtonChecked(IDC_CREATEXSD1);
	m_Options.m_bCreateXsd2		 = IsDlgButtonChecked(IDC_CREATEXSD2);
	if (m_Options.m_bCreateXsd2) m_Options.m_bCreateXsd1 = FALSE;

	// salva i valori di default della stampante preferenziale
	SavePrinters();

	EndDialog(IDOK);
}

//==============================================================================
//          Class CAlignDlg implementation
//==============================================================================
BEGIN_MESSAGE_MAP(CAlignDlg, CParsedDialog)
	//{{AFX_MSG_MAP(CAlignDlg)
	ON_CONTROL      (BN_CLICKED,        IDC_DT_SINGLELINE,  SetButtonStatus)
	ON_CONTROL      (BN_CLICKED,        IDC_DT_MULTILINE,   SetButtonStatus)
	ON_CONTROL      (BN_CLICKED,        IDC_DT_VCENTER,		SetVCenter)
	ON_CONTROL      (BN_CLICKED,        IDC_DT_TOP,			SetNoVCenter)
	ON_CONTROL      (BN_CLICKED,        IDC_DT_BOTTOM,		SetNoVCenter)

	ON_CONTROL      (BN_CLICKED,        IDC_DT_FIELD_SET,	OnClickFieldSet)
	ON_CONTROL      (BN_CLICKED,        IDC_DT_90,			OnClick90)
	ON_CONTROL      (BN_CLICKED,        IDC_DT_270,			OnClick270)

	//}}AFX_MSG_MAP
END_MESSAGE_MAP()
IMPLEMENT_DYNAMIC(CAlignDlg, CParsedDialog)

//------------------------------------------------------------------------------
CAlignDlg::CAlignDlg (AlignType& anAlign, CWnd* aParent)
	:
	CParsedDialog	(IDD_ALIGN, aParent),
	m_nAlign		(anAlign),
	m_bAskColumn	(FALSE),
	m_bAllColumns	(m_bAskColumn),  
	m_bAllowFieldSet	(FALSE),
	m_bAllowCenterBottom (FALSE),
	m_bAllowVertical			(TRUE)
{
}

//------------------------------------------------------------------------------
CAlignDlg::CAlignDlg (UINT anIDG, AlignType& anAlign, BOOL& all, CWnd* aParent)
	:
	CParsedDialog	(anIDG, aParent),
	m_nAlign		(anAlign),
	m_bAskColumn	(TRUE),
	m_bAllColumns	(all),
	m_bAllowFieldSet	(FALSE),
	m_bAllowCenterBottom (FALSE),
	m_bAllowVertical (TRUE)
{
}

//------------------------------------------------------------------------------
void CAlignDlg::SetButtonStatus()
{
	BOOL multiline = IsDlgButtonChecked(IDC_DT_MULTILINE);

	GetDlgItem(IDC_DT_WORDBREAK)->  EnableWindow(multiline);
	if (!multiline) 
		CheckDlgButton(IDC_DT_WORDBREAK,	0);

	BOOL wordwrap = IsDlgButtonChecked(DT_WORDBREAK);

	GetDlgItem(IDC_DT_TOP)->        EnableWindow(!wordwrap);
	GetDlgItem(IDC_DT_VCENTER)->    EnableWindow(!wordwrap);
	GetDlgItem(IDC_DT_BOTTOM)->     EnableWindow(!wordwrap);

	if (GetDlgItem(IDC_DT_FIELD_SET))
	{
		GetDlgItem(IDC_DT_FIELD_SET)->  EnableWindow(!multiline);
		if (multiline)
			CheckDlgButton(IDC_DT_FIELD_SET,	0);
	}
}

//-----------------------------------------------------------------------------
void CAlignDlg::SetVCenter()
{
	if (GetDlgItem(IDC_DT_VCENTER_LABEL))
		GetDlgItem(IDC_DT_VCENTER_LABEL)->EnableWindow(TRUE);
	GetDlgItem(IDC_DT_90)->EnableWindow(TRUE);
	GetDlgItem(IDC_DT_270)->EnableWindow(TRUE);
}

//-----------------------------------------------------------------------------
void CAlignDlg::SetNoVCenter()
{
	if (GetDlgItem(IDC_DT_VCENTER_LABEL))
	{
		GetDlgItem(IDC_DT_VCENTER_LABEL)->EnableWindow(FALSE);
		CheckDlgButton(IDC_DT_VCENTER_LABEL,	0);
	}
	GetDlgItem(IDC_DT_90)->EnableWindow(TRUE);
	GetDlgItem(IDC_DT_270)->EnableWindow(TRUE);
}

//-----------------------------------------------------------------------------
void CAlignDlg::OnClickFieldSet()
{
	if (GetDlgItem(IDC_DT_FIELD_SET) && IsDlgButtonChecked(IDC_DT_FIELD_SET) == 1)
	{
		CheckDlgButton(IDC_DT_90,	0);
		CheckDlgButton(IDC_DT_270,	0);
		GetDlgItem(IDC_DT_90)->EnableWindow(FALSE);
		GetDlgItem(IDC_DT_270)->EnableWindow(FALSE);
	}
	else
	{
		GetDlgItem(IDC_DT_90)->EnableWindow(TRUE);
		GetDlgItem(IDC_DT_270)->EnableWindow(TRUE);
	}
}
//-----------------------------------------------------------------------------
void CAlignDlg::OnClick90()
{
	if (IsDlgButtonChecked(IDC_DT_90) == 1)
	{
		CheckDlgButton(IDC_DT_270,	0);
		if (GetDlgItem(IDC_DT_VCENTER_LABEL))
		{
			CheckDlgButton(IDC_DT_VCENTER_LABEL,	0);
		}
	}
}

//-----------------------------------------------------------------------------
void CAlignDlg::OnClick270()
{
	if (IsDlgButtonChecked(IDC_DT_270) == 1)
	{
		CheckDlgButton(IDC_DT_90,	0);
		if (GetDlgItem(IDC_DT_VCENTER_LABEL))
		{
			CheckDlgButton(IDC_DT_VCENTER_LABEL,	0);
		}
	}
}

//------------------------------------------------------------------------------
void CAlignDlg::OnOK()
{
	m_nAlign = 0;

	if (IsDlgButtonChecked(IDC_DT_SINGLELINE))  
		m_nAlign |= DT_SINGLELINE;
	if (IsDlgButtonChecked(IDC_DT_WORDBREAK))   
		m_nAlign |= DT_WORDBREAK;
	if (IsDlgButtonChecked(IDC_DT_EXPANDTABS))  
		m_nAlign |= DT_EXPANDTABS;
	if (IsDlgButtonChecked(IDC_DT_NOPREFIX))    
		m_nAlign |= DT_NOPREFIX;

	if (IsDlgButtonChecked(IDC_DT_CENTER))      
		m_nAlign |= DT_CENTER;
	else if (IsDlgButtonChecked(IDC_DT_RIGHT))  
		m_nAlign |= DT_RIGHT;

	m_nAlign &=  ~DT_EX_VCENTER_LABEL;
	if (IsDlgButtonChecked(IDC_DT_VCENTER))
	{ 
		m_nAlign |= DT_VCENTER; 
		if (m_bAllowCenterBottom)
		{
			if (
					(
						GetDocument() &&
						((CWoormDocMng*)GetDocument())->m_pWoormIni &&
						((CWoormDocMng*)GetDocument())->m_pWoormIni->m_bForceVerticalAlignLabelRelative
					)
					||
					(IsDlgButtonChecked(IDC_DT_VCENTER_LABEL) == 1)
				)
				m_nAlign |=  DT_EX_VCENTER_LABEL;
		}
	}
	else if (IsDlgButtonChecked(IDC_DT_BOTTOM))
		m_nAlign |= DT_BOTTOM;

	if (m_bAskColumn)
		m_bAllColumns = IsDlgButtonChecked(IDC_ALL_COLUMNS);

	if (GetDlgItem(IDC_DT_FIELD_SET) && IsDlgButtonChecked(IDC_DT_FIELD_SET))   
		m_nAlign |= DT_EX_FIELD_SET;

	if (IsDlgButtonChecked(IDC_DT_90))    
		m_nAlign = (m_nAlign | DT_EX_ORIENTATION_90) &  ~DT_EX_VCENTER_LABEL;
	else if (IsDlgButtonChecked(IDC_DT_270))    
		m_nAlign = (m_nAlign | DT_EX_ORIENTATION_270) &  ~DT_EX_VCENTER_LABEL;

	EndDialog(IDOK);
}

//------------------------------------------------------------------------------
BOOL CAlignDlg::OnInitDialog()
{
	CParsedDialog::OnInitDialog();

	if (m_bAskColumn)
		CheckDlgButton(IDC_ALL_COLUMNS, m_bAllColumns);
	
	if (!m_bAllowFieldSet)
	{
		if (GetDlgItem(IDC_DT_FIELD_SET))
			GetDlgItem(IDC_DT_FIELD_SET)->ShowWindow(SW_HIDE);
	}
	else if (GetDlgItem(IDC_DT_FIELD_SET))
	{
		GetDlgItem(IDC_DT_FIELD_SET)->EnableWindow(TRUE);
		CheckDlgButton(IDC_DT_FIELD_SET, (m_nAlign & DT_EX_FIELD_SET) == DT_EX_FIELD_SET);

		OnClickFieldSet();
	}

	BOOL singleline = (m_nAlign & DT_SINGLELINE) == DT_SINGLELINE;
	CheckDlgButton(IDC_DT_SINGLELINE, singleline);
	CheckDlgButton(IDC_DT_MULTILINE, !singleline);
	CheckDlgButton(IDC_DT_WORDBREAK,   !singleline &&  ((m_nAlign & DT_WORDBREAK) == DT_WORDBREAK));

	CheckDlgButton(IDC_DT_EXPANDTABS,    ((m_nAlign & DT_EXPANDTABS)   == DT_EXPANDTABS));
	CheckDlgButton(IDC_DT_NOPREFIX,      ((m_nAlign & DT_NOPREFIX)     == DT_NOPREFIX));

	if ((m_nAlign & DT_CENTER) == DT_CENTER)   
		CheckDlgButton(IDC_DT_CENTER, 1);
	else if ((m_nAlign & DT_RIGHT) == DT_RIGHT)    
		CheckDlgButton(IDC_DT_RIGHT, 1);
	else 
		CheckDlgButton(IDC_DT_LEFT, 1);

	if ((m_nAlign & DT_VCENTER) == DT_VCENTER)  
		CheckDlgButton(IDC_DT_VCENTER, 1);
	else if ((m_nAlign & DT_BOTTOM) == DT_BOTTOM)   
		CheckDlgButton(IDC_DT_BOTTOM, 1);
	else //if ((m_nAlign & DT_TOP) == DT_TOP)   
		CheckDlgButton(IDC_DT_TOP, 1);

	if (m_bAllowVertical)
	{
		CheckDlgButton(IDC_DT_90,	((m_nAlign & DT_EX_ORIENTATION_90) == DT_EX_ORIENTATION_90));
		CheckDlgButton(IDC_DT_270,	((m_nAlign & DT_EX_ORIENTATION_270) == DT_EX_ORIENTATION_270));
	}
	else
	{
		GetDlgItem(IDC_DT_90)->ShowWindow(SW_HIDE);
		GetDlgItem(IDC_DT_270)->ShowWindow(SW_HIDE);
	}

	if (GetDlgItem(IDC_DT_VCENTER_LABEL))
	{
		if (m_bAllowCenterBottom)
		{
			GetDlgItem(IDC_DT_VCENTER_LABEL)->EnableWindow(((m_nAlign & DT_VCENTER) == DT_VCENTER));
			CheckDlgButton(IDC_DT_VCENTER_LABEL, (m_nAlign & DT_EX_VCENTER_LABEL) ? 1 : 0);

			if (
				GetDocument() &&
				((CWoormDocMng*)GetDocument())->m_pWoormIni &&
				((CWoormDocMng*)GetDocument())->m_pWoormIni->m_bForceVerticalAlignLabelRelative
				)
			{
				GetDlgItem(IDC_DT_VCENTER_LABEL)->ShowWindow(SW_HIDE);
			}
		}
		else
			GetDlgItem(IDC_DT_VCENTER_LABEL)->ShowWindow(SW_HIDE);
	}

	SetButtonStatus();
	return TRUE;  // return TRUE  unless you set the focus to a control
}

//==============================================================================
//          Class CRatiosDlg implementation
//==============================================================================
BEGIN_MESSAGE_MAP(CRatiosDlg, CParsedDialog)
	//{{AFX_MSG_MAP(CRatiosDlg)
	//}}AFX_MSG_MAP
END_MESSAGE_MAP()


IMPLEMENT_DYNAMIC(CRatiosDlg, CParsedDialog)
//------------------------------------------------------------------------------
CRatiosDlg::CRatiosDlg(int& aHRatio, int& aVRatio, CWoormDocMng* pDocument)
	:
	CParsedDialog	(IDD_RATIOS),
	m_HRatio		(aHRatio),
	m_VRatio		(aVRatio),
	m_HRatioEdit	(BTN_SPIN_ID),
	m_VRatioEdit	(BTN_SPIN_ID),
	m_pDocument		(pDocument)
{
}


//------------------------------------------------------------------------------
void CRatiosDlg::OnOK()
{
	if (!CheckForm())
		return;
	
	m_HRatio = m_HRatioEdit.GetValue();
	m_VRatio = m_VRatioEdit.GetValue();

	EndDialog(IDOK);
}


// se non ho un ratio in ingresso uso quello di default
//------------------------------------------------------------------------------
BOOL CRatiosDlg::OnInitDialog()
{
	CParsedDialog::OnInitDialog();

	// link parsctrl and spin
	if 	(
			!m_HRatioEdit.SubclassEdit(IDC_HRATIO, this) ||
			!m_VRatioEdit.SubclassEdit(IDC_VRATIO, this)
		) 
	{
		EndDialog(IDCANCEL);
		return FALSE;
	}

	// input in mm with two decimal digit                                                 
	m_HRatioEdit.SetRange(0, MAX_RATIO);
	m_HRatioEdit.SetValue(m_HRatio ? m_HRatio : m_pDocument->m_pWoormIni->m_nHorzRatio);

	// input in mm with two decimal digit                                                 
	m_VRatioEdit.SetRange(0, MAX_RATIO);
	m_VRatioEdit.SetValue(m_VRatio ? m_VRatio : m_pDocument->m_pWoormIni->m_nVertRatio);

	return TRUE;  // return TRUE  unless you set the focus to a control
}



//==============================================================================
//          Class CLineWidthDlg implementation
//==============================================================================
BEGIN_MESSAGE_MAP(CLineWidthDlg, CParsedDialog)
	//{{AFX_MSG_MAP(CLineWidthDlg)
	ON_CONTROL      (BN_CLICKED,        IDC_PEN_TINY,	OnPenTiny)
	ON_CONTROL      (BN_CLICKED,        IDC_PEN_SMALL,	OnPenSmall)
	ON_CONTROL      (BN_CLICKED,        IDC_PEN_MEDIUM,	OnPenMedium)
	ON_CONTROL      (BN_CLICKED,        IDC_PEN_LARGE,	OnPenLarge)
	ON_CONTROL      (BN_CLICKED,        IDC_PEN_EXTRA,	OnPenExtra)
	ON_EN_KILLFOCUS	(IDC_LINE_WIDTH,					OnKillFocusPenSize)
	ON_CONTROL		(EN_SPIN_RELEASED,	IDC_LINE_WIDTH,	OnKillFocusPenSize)
	//}}AFX_MSG_MAP
END_MESSAGE_MAP()

IMPLEMENT_DYNAMIC(CLineWidthDlg, CParsedDialog)

//------------------------------------------------------------------------------
CLineWidthDlg::CLineWidthDlg (BorderPen& aBorderPen, CWnd* aParent)
	:
	CParsedDialog	(IDD_LINE_WIDTH,aParent),
	m_BorderPen		(aBorderPen),
	m_bAskColumn	(FALSE),
	m_bAllColumns	(m_bAskColumn),  // tricks for reference
	m_LineWidthEdit	(BTN_SPIN_ID)
{
}

//------------------------------------------------------------------------------
CLineWidthDlg::CLineWidthDlg (BorderPen& aBorderPen, BOOL& all, CWnd* aParent)
	:
	CParsedDialog	(IDD_LINE_WIDTH,aParent),
	m_BorderPen		(aBorderPen),
	m_bAskColumn	(TRUE),
	m_bAllColumns	(all),
	m_LineWidthEdit	(BTN_SPIN_ID)
{
}

//------------------------------------------------------------------------------
BOOL CLineWidthDlg::OnInitDialog()
{
	CParsedDialog::OnInitDialog();
	
	EnableToolTips(TRUE);

	if (m_bAskColumn)
	{
		CheckDlgButton(IDC_ALL_COLUMNS, m_bAllColumns);
	}
	else
	{
		GetDlgItem(IDC_ALL_COLUMNS)->ShowWindow(SW_HIDE);
		GetDlgItem(IDC_APPLY_ALL_COLUMNS)->ShowWindow(SW_HIDE);
	}
									
	CheckLineWidthButtons(m_BorderPen.GetWidth());
	
	// link parsctrl and spin
	m_LineWidthEdit.SubclassEdit(IDC_LINE_WIDTH, this);
	m_LineWidthEdit.SetRange(0, MAX_PEN_SIZE);
	m_LineWidthEdit.SetValue(m_BorderPen.GetWidth());

	m_Color = (long)m_BorderPen.m_rgbColor;
	CParsedCtrl* p = ::GetParsedCtrl(&m_edtColor);
	p->Attach(&m_Color);
	m_edtColor.Attach(BTN_COLOR_ID);
	m_edtColor.SubclassEdit(IDC_PEN_COLOR, this);
	m_edtColor.UpdateCtrlStatus();
	m_edtColor.UpdateCtrlView();

	return TRUE;  // return TRUE  unless you set the focus to a control
}

//-----------------------------------------------------------------------------
void CLineWidthDlg::CheckLineWidthButtons(int width)
{
	CheckDlgButton(IDC_PEN_TINY,	(width == PEN_TINY));
	CheckDlgButton(IDC_PEN_SMALL,	(width == PEN_SMALL));
	CheckDlgButton(IDC_PEN_MEDIUM,	(width == PEN_MEDIUM));
	CheckDlgButton(IDC_PEN_LARGE,	(width == PEN_LARGE));
	CheckDlgButton(IDC_PEN_EXTRA,	(width == PEN_EXTRA));
}

//-----------------------------------------------------------------------------
void CLineWidthDlg::OnKillFocusPenSize()
{
	if (!CheckForm(FALSE /* no message */))	
		return;

	// to refresh the default line width buttons
	CheckLineWidthButtons(m_LineWidthEdit.GetValue());
}
																		
//-----------------------------------------------------------------------------
void CLineWidthDlg::OnPenTiny	()	{ m_LineWidthEdit.SetValue(PEN_TINY);	}
void CLineWidthDlg::OnPenSmall	()	{ m_LineWidthEdit.SetValue(PEN_SMALL);	}
void CLineWidthDlg::OnPenMedium	()	{ m_LineWidthEdit.SetValue(PEN_MEDIUM);	}
void CLineWidthDlg::OnPenLarge	()	{ m_LineWidthEdit.SetValue(PEN_LARGE);	}
void CLineWidthDlg::OnPenExtra	()	{ m_LineWidthEdit.SetValue(PEN_EXTRA);	}

//------------------------------------------------------------------------------
void CLineWidthDlg::OnOK()
{
	if (!CheckForm())
		return;
		
	m_BorderPen.SetWidth(m_LineWidthEdit.GetValue());
	m_BorderPen.SetColor((long)m_Color);
	
	if (m_bAskColumn) m_bAllColumns = IsDlgButtonChecked(IDC_ALL_COLUMNS);

	EndDialog(IDOK);
}

//==============================================================================
//          Class CMulAlignDlg implementation
//==============================================================================
BEGIN_MESSAGE_MAP(CMulAlignDlg, CParsedDialog)
	//{{AFX_MSG_MAP(CMulAlignDlg)
	//}}AFX_MSG_MAP
END_MESSAGE_MAP()

IMPLEMENT_DYNAMIC(CMulAlignDlg, CParsedDialog)
//------------------------------------------------------------------------------
CMulAlignDlg::CMulAlignDlg (int& aHID, int& aVID, CWnd* aParent)
	:
	CParsedDialog	(IDD_MUL_ALIGN, aParent),
	m_hID			(aHID),
	m_vID			(aVID)
{
}


//------------------------------------------------------------------------------
void CMulAlignDlg::OnOK()
{
			if (IsDlgButtonChecked(IDC_H_NO_CHANGE))    m_hID = IDC_H_NO_CHANGE;
	else    if (IsDlgButtonChecked(IDC_H_LEFT))         m_hID = IDC_H_LEFT;
	else    if (IsDlgButtonChecked(IDC_H_RIGHT))        m_hID = IDC_H_RIGHT;
	else    if (IsDlgButtonChecked(IDC_H_SPACE_EQUAL))  m_hID = IDC_H_SPACE_EQUAL;
	else	if (IsDlgButtonChecked(IDC_STACK_LEFT))   	m_hID = IDC_STACK_LEFT;
	else    if (IsDlgButtonChecked(IDC_STACK_RIGHT))    m_hID = IDC_STACK_RIGHT;
	else	if (IsDlgButtonChecked(IDC_CUT_H_LEFT))   	m_hID = IDC_CUT_H_LEFT;
	else    if (IsDlgButtonChecked(IDC_CUT_H_RIGHT))    m_hID = IDC_CUT_H_RIGHT;

			if (IsDlgButtonChecked(IDC_V_NO_CHANGE))    m_vID = IDC_V_NO_CHANGE;
	else    if (IsDlgButtonChecked(IDC_V_TOP))          m_vID = IDC_V_TOP;
	else    if (IsDlgButtonChecked(IDC_V_BOTTOM))       m_vID = IDC_V_BOTTOM;
	else    if (IsDlgButtonChecked(IDC_V_SPACE_EQUAL))  m_vID = IDC_V_SPACE_EQUAL;
	else    if (IsDlgButtonChecked(IDC_STACK_TOP))      m_vID = IDC_STACK_TOP;
	else    if (IsDlgButtonChecked(IDC_STACK_BOTTOM))  	m_vID = IDC_STACK_BOTTOM;
	else    if (IsDlgButtonChecked(IDC_CUT_V_TOP))      m_vID = IDC_CUT_V_TOP;
	else    if (IsDlgButtonChecked(IDC_CUT_V_BOTTOM))  	m_vID = IDC_CUT_V_BOTTOM;
	
	EndDialog(IDOK);
}


//------------------------------------------------------------------------------
BOOL CMulAlignDlg::OnInitDialog()
{
	CParsedDialog::OnInitDialog();

	CheckDlgButton(IDC_H_NO_CHANGE,     m_hID == IDC_H_NO_CHANGE);
	CheckDlgButton(IDC_H_LEFT,          m_hID == IDC_H_LEFT);
	CheckDlgButton(IDC_H_RIGHT,         m_hID == IDC_H_RIGHT);
	CheckDlgButton(IDC_H_SPACE_EQUAL,   m_hID == IDC_H_SPACE_EQUAL);
	CheckDlgButton(IDC_STACK_LEFT,		m_hID == IDC_STACK_LEFT);
	CheckDlgButton(IDC_STACK_RIGHT,		m_hID == IDC_STACK_RIGHT);
	CheckDlgButton(IDC_CUT_H_LEFT,		m_hID == IDC_CUT_H_LEFT);
	CheckDlgButton(IDC_CUT_H_RIGHT,		m_hID == IDC_CUT_H_RIGHT);

	CheckDlgButton(IDC_V_NO_CHANGE,     m_vID == IDC_V_NO_CHANGE);
	CheckDlgButton(IDC_V_TOP,           m_vID == IDC_V_TOP);
	CheckDlgButton(IDC_V_BOTTOM,        m_vID == IDC_V_BOTTOM);
	CheckDlgButton(IDC_V_SPACE_EQUAL,   m_vID == IDC_V_SPACE_EQUAL);
	CheckDlgButton(IDC_STACK_TOP,		m_vID == IDC_STACK_TOP);
	CheckDlgButton(IDC_STACK_BOTTOM,	m_vID == IDC_STACK_BOTTOM);
	CheckDlgButton(IDC_CUT_V_TOP,		m_vID == IDC_CUT_V_TOP);
	CheckDlgButton(IDC_CUT_V_BOTTOM,	m_vID == IDC_CUT_V_BOTTOM);
	
	return TRUE;  // return TRUE  unless you set the focus to a control
}





//==============================================================================
//          Class CMulSizeDlg implementation
//==============================================================================
BEGIN_MESSAGE_MAP(CMulSizeDlg, CParsedDialog)
	//{{AFX_MSG_MAP(CMulSizeDlg)   
	ON_CONTROL      (BN_CLICKED,        IDC_H_NO_CHANGE,	OnClickCX)
	ON_CONTROL      (BN_CLICKED,        IDC_H_VALUE,    	OnClickCX)
	ON_CONTROL      (BN_CLICKED,        IDC_LAST_LARGE,    	OnClickCX)

	ON_CONTROL      (BN_CLICKED,        IDC_V_NO_CHANGE,	OnClickCY)
	ON_CONTROL      (BN_CLICKED,        IDC_V_VALUE,    	OnClickCY)
	ON_CONTROL      (BN_CLICKED,        IDC_LAST_HIGH,    	OnClickCY)
	//}}AFX_MSG_MAP
END_MESSAGE_MAP()

IMPLEMENT_DYNAMIC(CMulSizeDlg, CParsedDialog)
//------------------------------------------------------------------------------
CMulSizeDlg::CMulSizeDlg(int& aHID, int& aVID, CSize& aSize, CWnd* aParent)
	:
	CParsedDialog	(IDD_MUL_SIZE, aParent),
	m_nSize			(aSize),
	m_hID			(aHID),
	m_vID			(aVID),
	m_CxValueEdit	(BTN_SPIN_ID),
	m_CyValueEdit	(BTN_SPIN_ID)
{
}

//-----------------------------------------------------------------------------
void CMulSizeDlg::OnClickCX()
{
	BOOL flag = IsDlgButtonChecked(IDC_H_VALUE);

	GetDlgItem(IDC_CX_VALUE)->EnableWindow(flag);
}

//-----------------------------------------------------------------------------
void CMulSizeDlg::OnClickCY()
{
	BOOL flag = IsDlgButtonChecked(IDC_V_VALUE);

	GetDlgItem(IDC_CY_VALUE)->EnableWindow(flag);
}

//-----------------------------------------------------------------------------
BOOL CMulSizeDlg::InitEdit(int nID, int aValue, int aMin, int aMax)
{
	if (!GetMetricEdit(nID).SubclassEdit(nID, this)) return FALSE;

	// input in mm with two decimal digit                                                 
	GetMetricEdit(nID).SetRange(aMin, aMax);
	GetMetricEdit(nID).SetValue(aValue);

	return TRUE;
}


//-----------------------------------------------------------------------------
BOOL CMulSizeDlg::OnInitDialog()
{
	CParsedDialog::OnInitDialog();

	int min_width   = BaseObj::DEFAULTSIZE.cx;
	int min_height  = BaseObj::DEFAULTSIZE.cy;

	CheckDlgButton(IDC_H_NO_CHANGE, m_hID == IDC_H_NO_CHANGE);
	CheckDlgButton(IDC_LAST_LARGE,  m_hID == IDC_LAST_LARGE);
	CheckDlgButton(IDC_H_VALUE,     m_hID == IDC_H_VALUE);

	CheckDlgButton(IDC_V_NO_CHANGE, m_vID == IDC_V_NO_CHANGE);
	CheckDlgButton(IDC_LAST_HIGH,	m_vID == IDC_LAST_HIGH);
	CheckDlgButton(IDC_V_VALUE,     m_vID == IDC_V_VALUE);

	OnClickCX();
	OnClickCY();

	if	(
			!InitEdit(IDC_CX_VALUE, m_nSize.cx, min_width,  INT_MAX) ||
			!InitEdit(IDC_CY_VALUE, m_nSize.cy, min_height, INT_MAX)
		)
	{
		EndDialog(IDCANCEL);
		return FALSE;
	}
	
	return TRUE;  // return TRUE  unless you set the focus to a control
}

//-----------------------------------------------------------------------------
CMetricEdit& CMulSizeDlg::GetMetricEdit(int nID)
{
	if (nID == IDC_CX_VALUE) return m_CxValueEdit;
	if (nID == IDC_CY_VALUE)  return m_CyValueEdit;

	return m_CxValueEdit;
 }

//-----------------------------------------------------------------------------
void CMulSizeDlg::OnOK()
{
	if (!CheckForm())
		return;
		
	int width   = m_nSize.cx;
	int height  = m_nSize.cy;
			if (IsDlgButtonChecked(IDC_H_NO_CHANGE))    m_hID = IDC_H_NO_CHANGE;
	else    if (IsDlgButtonChecked(IDC_LAST_LARGE))		m_hID = IDC_LAST_LARGE;
	else    if (IsDlgButtonChecked(IDC_H_VALUE))        m_hID = IDC_H_VALUE;

			if (IsDlgButtonChecked(IDC_V_NO_CHANGE))    m_vID = IDC_V_NO_CHANGE;
	else    if (IsDlgButtonChecked(IDC_LAST_HIGH))		m_vID = IDC_LAST_HIGH;
	else    if (IsDlgButtonChecked(IDC_V_VALUE))        m_vID = IDC_V_VALUE;

	if (m_hID == IDC_H_VALUE) width = GetMetricEdit(IDC_CX_VALUE).GetValue();
	if (m_vID == IDC_V_VALUE) height = GetMetricEdit(IDC_CY_VALUE).GetValue();

	m_nSize.cx = width;
	m_nSize.cy = height;

	EndDialog(IDOK);
}


//==============================================================================
//          Class CObjValueDlg implementation
//==============================================================================
BEGIN_MESSAGE_MAP(CObjValueDlg, CParsedDialog)
	//{{AFX_MSG_MAP(CObjValueDlg)
	//}}AFX_MSG_MAP
END_MESSAGE_MAP()

IMPLEMENT_DYNAMIC(CObjValueDlg, CParsedDialog)
//------------------------------------------------------------------------------
CObjValueDlg::CObjValueDlg(CRect& aRect, BOOL aOnlyMove, CWnd* aParent, BOOL aOnlyHeight)
	:
	CParsedDialog	(IDD_OBJ_VALUE, aParent),
	m_Rect			(aRect),
	m_bOnlyMove		(aOnlyMove),
	m_bOnlyHeight	(aOnlyHeight),
	m_ObjTopEdit	(BTN_SPIN_ID),
	m_ObjLeftEdit	(BTN_SPIN_ID),
	m_ObjWidthEdit	(BTN_SPIN_ID),
	m_ObjHeightEdit	(BTN_SPIN_ID)
{
}


//-----------------------------------------------------------------------------
BOOL CObjValueDlg::InitEdit(int nID, int aValue, int aMin, int aMax)
{
	if (!GetMetricEdit(nID).SubclassEdit(nID, this)) return FALSE;

	// input in mm with two decimal digit                                                 
	GetMetricEdit(nID).SetRange(aMin, aMax);
	GetMetricEdit(nID).SetValue(aValue);

	return TRUE;
}


//-----------------------------------------------------------------------------
BOOL CObjValueDlg::OnInitDialog()
{
	CParsedDialog::OnInitDialog();

	int min_width   = BaseObj::DEFAULTSIZE.cx;
	int min_height  = BaseObj::DEFAULTSIZE.cy;

	BOOL ok =
	(
	(InitEdit(IDC_OBJ_TOP,      m_Rect.top,       0,          INT_MAX)) &&
	(InitEdit(IDC_OBJ_LEFT,     m_Rect.left,      0,          INT_MAX)) &&
	(InitEdit(IDC_OBJ_WIDTH,    m_Rect.Width(),   min_width,  INT_MAX)) &&
	(InitEdit(IDC_OBJ_HEIGHT,   m_Rect.Height(),  min_height, INT_MAX))
	);

	if (ok && m_bOnlyMove)
	{
		GetDlgItem(IDC_OBJ_WIDTH        )->EnableWindow(FALSE);
		GetDlgItem(IDC_OBJ_HEIGHT       )->EnableWindow(FALSE);
		GetDlgItem(IDC_TAG_OBJ_WIDTH    )->EnableWindow(FALSE);
		GetDlgItem(IDC_TAG_OBJ_HEIGHT   )->EnableWindow(FALSE);
	}
	if (ok && m_bOnlyHeight)
	{
		GetDlgItem(IDC_OBJ_LEFT			)->EnableWindow(FALSE);
		GetDlgItem(IDC_OBJ_WIDTH        )->EnableWindow(FALSE);
		//GetDlgItem(IDC_TAG_OBJ_WIDTH    )->EnableWindow(FALSE);
	}
						 
	// return TRUE  unless you set the focus to a control
	if (ok) return TRUE;  

	EndDialog(IDCANCEL);
	return FALSE;
}

//-----------------------------------------------------------------------------
CMetricEdit& CObjValueDlg::GetMetricEdit(int nID)
{
	if (nID == IDC_OBJ_TOP)     return m_ObjTopEdit;
	if (nID == IDC_OBJ_LEFT) return m_ObjLeftEdit;
	if (nID == IDC_OBJ_WIDTH) return m_ObjWidthEdit;
	if (nID == IDC_OBJ_HEIGHT) return m_ObjHeightEdit;

	return m_ObjTopEdit;
}

//-----------------------------------------------------------------------------
void CObjValueDlg::OnOK()
{   
	if (!CheckForm())
		return;
		
	int top;
	int left;
	int width;
	int height;

	top = GetMetricEdit(IDC_OBJ_TOP).GetValue();
	left = GetMetricEdit(IDC_OBJ_LEFT).GetValue();
	width = GetMetricEdit(IDC_OBJ_WIDTH).GetValue();
	height = GetMetricEdit(IDC_OBJ_HEIGHT).GetValue();

	m_Rect.top = top;
	m_Rect.left = left;
	m_Rect.bottom = m_Rect.top + height;
	m_Rect.right = m_Rect.left + width;

	EndDialog(IDOK);
}

//==============================================================================
//          Class CTableHeightDlg implementation
//==============================================================================
BEGIN_MESSAGE_MAP(CTableHeightDlg, CParsedDialog)
	//{{AFX_MSG_MAP(CTableHeightDlg)
	//}}AFX_MSG_MAP
END_MESSAGE_MAP()

IMPLEMENT_DYNAMIC(CTableHeightDlg, CParsedDialog)
//------------------------------------------------------------------------------
CTableHeightDlg::CTableHeightDlg		
		(	
			int&	nHTableTitle,
			int&	nHColumnsTitle,
			int&	nHRows,
			int&	nHTotals,
			CWnd*	aParent
		)
	:
	CParsedDialog		(IDD_TABLE_HEIGHTS, aParent),
	m_ObjHTableTitle	(BTN_SPIN_ID),
	m_ObjHColumnsTitle	(BTN_SPIN_ID),
	m_ObjHRows			(BTN_SPIN_ID),
	m_ObjHTotals		(BTN_SPIN_ID),
	m_nHTableTitle		(nHTableTitle),
	m_nHColumnsTitle	(nHColumnsTitle),
	m_nHRows			(nHRows),
	m_nHTotals			(nHTotals)
{
}

//-----------------------------------------------------------------------------
BOOL CTableHeightDlg::InitEdit(int nID, int aValue, int aMin, int aMax)
{
	if (!GetMetricEdit(nID).SubclassEdit(nID, this)) 
		return FALSE;

	// input in mm with two decimal digit                                                 
	GetMetricEdit(nID).SetRange(aMin, aMax);
	GetMetricEdit(nID).SetValue(aValue);

	return TRUE;
}

//-----------------------------------------------------------------------------
BOOL CTableHeightDlg::OnInitDialog()
{
	CParsedDialog::OnInitDialog();

	BOOL ok =
	(
	(InitEdit(IDC_TABLE_TITLE_HEIGHT,	m_nHTableTitle,		0, INT_MAX)) &&
	(InitEdit(IDC_COLUMNS_TITLE_HEIGHT,	m_nHColumnsTitle,	0, INT_MAX)) &&
	(InitEdit(IDC_ROWS_HEIGHT,			m_nHRows,			0, INT_MAX)) &&
	(InitEdit(IDC_TOTALS_HEIGHT,		m_nHTotals,			0, INT_MAX))
	);
						 
	// return TRUE  unless you set the focus to a control
	if (ok) 
		return TRUE;  

	EndDialog(IDCANCEL);
	return FALSE;
}

//-----------------------------------------------------------------------------
CMetricEdit& CTableHeightDlg::GetMetricEdit(int nID)
{
	if (nID == IDC_TABLE_TITLE_HEIGHT) return m_ObjHTableTitle;
	if (nID == IDC_COLUMNS_TITLE_HEIGHT) return m_ObjHColumnsTitle;
	if (nID == IDC_ROWS_HEIGHT) return m_ObjHRows;
	if (nID == IDC_TOTALS_HEIGHT) return m_ObjHTotals;
	return m_ObjHTableTitle;
}

//-----------------------------------------------------------------------------
void CTableHeightDlg::OnOK()
{   
	if (!CheckForm())
		return;
		
	m_nHTableTitle		= GetMetricEdit(IDC_TABLE_TITLE_HEIGHT).GetValue();
	m_nHColumnsTitle	= GetMetricEdit(IDC_COLUMNS_TITLE_HEIGHT).GetValue();
	m_nHRows			= GetMetricEdit(IDC_ROWS_HEIGHT).GetValue();
	m_nHTotals			= GetMetricEdit(IDC_TOTALS_HEIGHT).GetValue();

	EndDialog(IDOK);
}

//==============================================================================
//          Class CRepeaterDlg implementation
//==============================================================================
BEGIN_MESSAGE_MAP(CRepeaterDlg, CParsedDialog)
	//{{AFX_MSG_MAP(CRepeaterDlg)
	//}}AFX_MSG_MAP
END_MESSAGE_MAP()

IMPLEMENT_DYNAMIC(CRepeaterDlg, CParsedDialog)
//------------------------------------------------------------------------------
CRepeaterDlg::CRepeaterDlg		
		(	
			int	nRows,
			int	nColumns,
			int	nYOffset,
			int	nXOffset,
			BOOL bByColumn,
			CWnd*	aParent
		)
	:
	CParsedDialog	(IDD_REPEATER_PROPERTIES, aParent),
	m_RowOffset		(BTN_SPIN_ID),
	m_ColumnOffset	(BTN_SPIN_ID),
	
	m_nRows		(nRows),
	m_nColumns	(nColumns),
	m_nYOffset	(nYOffset),
	m_nXOffset	(nXOffset),
	m_bByColumn	(bByColumn)
{
}

//-----------------------------------------------------------------------------
BOOL CRepeaterDlg::InitEdit(int nID, int aValue, int aMin, int aMax)
{
	if (!GetMetricEdit(nID).SubclassEdit(nID, this)) 
		return FALSE;

	// input in mm with two decimal digit                                                 
	GetMetricEdit(nID).SetRange(aMin, aMax);
	GetMetricEdit(nID).SetValue(aValue);

	return TRUE;
}

//-----------------------------------------------------------------------------
BOOL CRepeaterDlg::OnInitDialog()
{
	CParsedDialog::OnInitDialog();

	BOOL ok =
		(
			(InitEdit(IDC_REPEATER_ROWS_INTERLINE,		m_nYOffset,		0, INT_MAX)) &&
			(InitEdit(IDC_REPEATER_COLUMNS_INTERLINE,	m_nXOffset,		0, INT_MAX))
		);
		
	VERIFY(m_Rows.SubclassEdit(IDC_REPEATER_ROWS, this)) ;
	m_Rows.SetRange(1, INT_MAX);
	m_Rows.SetValue(m_nRows);

	VERIFY(m_Columns.SubclassEdit(IDC_REPEATER_COLUMNS, this)) ;
	m_Columns.SetRange(1, INT_MAX);
	m_Columns.SetValue(m_nColumns);

	CheckDlgButton(IDC_CHK_BYCOLUMN, m_bByColumn ? 1 : 0);

	// return TRUE  unless you set the focus to a control
	if (ok) 
		return TRUE;  

	EndDialog(IDCANCEL);
	return FALSE;
}

//-----------------------------------------------------------------------------
CMetricEdit& CRepeaterDlg::GetMetricEdit(int nID)
{
	if (nID == IDC_REPEATER_ROWS_INTERLINE) return m_RowOffset;
	if (nID == IDC_REPEATER_COLUMNS_INTERLINE) return m_ColumnOffset;
	return m_RowOffset;
}

//-----------------------------------------------------------------------------
void CRepeaterDlg::OnOK()
{   
	if (!CheckForm())
		return;
		
	m_nYOffset		= GetMetricEdit(IDC_REPEATER_ROWS_INTERLINE).GetValue();
	m_nXOffset		= GetMetricEdit(IDC_REPEATER_COLUMNS_INTERLINE).GetValue();

	m_nRows			= m_Rows.GetValue();
	m_nColumns		= m_Columns.GetValue();

	m_bByColumn = this->IsDlgButtonChecked(IDC_CHK_BYCOLUMN) == 1;

	EndDialog(IDOK);
}

//==============================================================================
//          Class CWidthDlg implementation
//==============================================================================
BEGIN_MESSAGE_MAP(CWidthDlg, CParsedDialog)
	//{{AFX_MSG_MAP(CWidthDlg)
	//}}AFX_MSG_MAP
END_MESSAGE_MAP()

IMPLEMENT_DYNAMIC(CWidthDlg, CParsedDialog)
//------------------------------------------------------------------------------
CWidthDlg::CWidthDlg		
		(	
			int&	nWidth,
			CWnd*	aParent
		)
	:
	CParsedDialog		(IDD_SET_WIDTH, aParent),
	m_Width				(BTN_SPIN_ID),
	m_nWidth			(nWidth)
{
}

//-----------------------------------------------------------------------------
BOOL CWidthDlg::OnInitDialog()
{
	CParsedDialog::OnInitDialog();

	VERIFY(m_Width.SubclassEdit(IDC_SET_WIDTH, this)) ;

	// input in mm with two decimal digit                                                 
	m_Width.SetRange(0, INT_MAX);
	m_Width.SetValue(m_nWidth);

	return TRUE;
}

//-----------------------------------------------------------------------------
void CWidthDlg::OnOK()
{   
	if (!CheckForm())
		return;
		
	m_nWidth = m_Width.GetValue();

	EndDialog(IDOK);
}


//==============================================================================
//          Class CTableRowsDlg implementation
//==============================================================================
BEGIN_MESSAGE_MAP(CTableRowsDlg, CParsedDialog)
	//{{AFX_MSG_MAP(CTableRowsDlg)
	//}}AFX_MSG_MAP
END_MESSAGE_MAP()

IMPLEMENT_DYNAMIC(CTableRowsDlg, CParsedDialog)
//------------------------------------------------------------------------------
CTableRowsDlg::CTableRowsDlg		
		(	
			int&	nRows,
			CWnd*	aParent
		)
	:
	CParsedDialog		(IDD_SET_TABLE_ROWS, aParent),
	m_Rows				(BTN_SPIN_ID),
	m_nRows				(nRows),
	m_nPrevRows			(nRows)
{
}

//-----------------------------------------------------------------------------
BOOL CTableRowsDlg::OnInitDialog()
{
	CParsedDialog::OnInitDialog();

	VERIFY(m_Rows.SubclassEdit(IDC_SET_TABLE_ROWS, this)) ;

	// input in mm with two decimal digit                                                 
	m_Rows.SetRange(1, INT_MAX);
	m_Rows.SetValue(m_nRows);

	return TRUE;
}

//-----------------------------------------------------------------------------
void CTableRowsDlg::OnOK()
{   
	if (!CheckForm())
		return;
		
	m_nRows = m_Rows.GetValue();

	EndDialog(IDOK);
}

//==============================================================================
//          Class CGotoPageDlg implementation
//==============================================================================
BEGIN_MESSAGE_MAP(CGotoPageDlg, CParsedDialog)
	//{{AFX_MSG_MAP(CGotoPageDlg)
	//}}AFX_MSG_MAP
END_MESSAGE_MAP()

IMPLEMENT_DYNAMIC(CGotoPageDlg, CParsedDialog)
//------------------------------------------------------------------------------
CGotoPageDlg::CGotoPageDlg(int& aPage, int aMaxPage, CWnd* aParent)
	:
	CParsedDialog	(IDD_GOTO_PAGE, aParent),
	m_nPage			(aPage),
	m_nMaxPage		(aMaxPage),
	m_PageEdit		(BTN_SPIN_ID)
{
}


//-----------------------------------------------------------------------------
BOOL CGotoPageDlg::InitEdit(int nID, int aValue, int aMax)
{
	if (!m_PageEdit.SubclassEdit(nID, this)) return FALSE;
	m_PageEdit.SetRange(1, aMax);
	m_PageEdit.SetValue(aValue);

	return TRUE;
}

//-----------------------------------------------------------------------------
BOOL CGotoPageDlg::OnInitDialog()
{
	CParsedDialog::OnInitDialog();

	BOOL ok = InitEdit(IDC_GOTO_PAGE, m_nPage, m_nMaxPage);

	// return TRUE  unless you set the focus to a control
	if (ok) return TRUE;  

	EndDialog(IDCANCEL);
	return FALSE;
}


//-----------------------------------------------------------------------------
void CGotoPageDlg::OnOK()
{
	if (!CheckForm())
		return;
	
	m_nPage = m_PageEdit.GetValue();
		
	EndDialog(IDOK);
}

//==============================================================================
//          Class CHiddenWhenDlg implementation
//==============================================================================
BEGIN_MESSAGE_MAP(CHiddenWhenDlg, CParsedDialog)
	//{{AFX_MSG_MAP(CHiddenWhenDlg)
	ON_BN_CLICKED       (IDC_RADIO_HIDEALWAYS,	ChangeRadioSelection)
	ON_BN_CLICKED       (IDC_RADIO_HIDEEXPR,	ChangeRadioSelection)
	ON_BN_CLICKED       (IDC_RADIO_HIDENEVER,	ChangeRadioSelection)
	//}}AFX_MSG_MAP
END_MESSAGE_MAP()

IMPLEMENT_DYNAMIC(CHiddenWhenDlg, CParsedDialog)
//------------------------------------------------------------------------------
CHiddenWhenDlg::CHiddenWhenDlg (Expression*& pHideExpr, BOOL bHidden, CString strLabelColumn, SymTable* pSymTable, BaseRect* pBaseRect /*= NULL*/,CWnd* aParent /*=NULL*/)
	:
	CParsedDialog	(IDD_HIDDEN_WHEN, aParent),
	m_edtHideExpr	(pSymTable/*, RULE = null*/ ),
	m_pHideExpr		(pHideExpr),
	m_bHidden		(bHidden),
	m_pSymTable		(pSymTable),
	m_strLabelColumn(strLabelColumn),
	m_pBaseRect		(pBaseRect),
	m_bNoHiddenRadio(FALSE)
{
}

//------------------------------------------------------------------------------
CHiddenWhenDlg::CHiddenWhenDlg (Expression*& pHideExpr, SymTable* pSymTable, CWnd* aParent /*=NULL*/)
	:
	CParsedDialog	(IDD_HIDDEN_WHEN, aParent),
	m_edtHideExpr	(pSymTable),
	m_pHideExpr		(pHideExpr),
	m_bHidden		(FALSE),
	m_pSymTable		(pSymTable),
	//m_strLabelColumn(),
	m_pBaseRect		(NULL),
	m_bNoHiddenRadio(TRUE)
{
}

//-----------------------------------------------------------------------------
BOOL CHiddenWhenDlg::OnInitDialog()
{
	CParsedDialog::OnInitDialog();
	
	CString dialogCaption;
	this->GetWindowText(dialogCaption);
	dialogCaption.Append(_T("-"));
	dialogCaption.Append(m_strLabelColumn);
	this->SetWindowText(dialogCaption);
	
	m_edtHideExpr.		SubclassEdit	(IDC_EDIT_DYNAMICEXPR,       		this);

	if (m_pHideExpr == NULL)
	{
		if (m_bHidden)
			SendDlgItemMessage(IDC_RADIO_HIDEALWAYS, BM_SETCHECK, TRUE, 0);
		else
			SendDlgItemMessage(IDC_RADIO_HIDENEVER, BM_SETCHECK, TRUE, 0);
		m_edtHideExpr.EnableWindow(FALSE);
	}
	else
	{
		SendDlgItemMessage(IDC_RADIO_HIDEEXPR, BM_SETCHECK, TRUE, 0);
		m_edtHideExpr.SetWindowText(m_pHideExpr->ToString());
	}
	if (m_bNoHiddenRadio)
		GetDlgItem(IDC_RADIO_HIDEALWAYS)->ShowWindow(SW_HIDE);
	return TRUE;
}


//-----------------------------------------------------------------------------
void CHiddenWhenDlg::OnOK()
{
	CString strExpr;
	DataBool bHidden(TRUE);  //inizializzato come se si fosse selezionato "nascondi sempre"
	BOOL isNewExpr = FALSE;
	
	if (IsDlgButtonChecked(IDC_RADIO_HIDEEXPR)) 
	{
		m_edtHideExpr.GetWindowText(strExpr);
		ConvertCString(strExpr, CRLF_TO_LF);
		Parser	lex(strExpr);
		
		if (!m_pHideExpr)
		{
			m_pHideExpr = new Expression(m_pSymTable);
			isNewExpr = TRUE;
		}
		if (!m_edtHideExpr.CheckExp(lex, *m_pHideExpr, DATA_BOOL_TYPE))
		{
			if (isNewExpr)
				SAFE_DELETE(m_pHideExpr);
			
			m_edtHideExpr.SetCtrlFocus(TRUE);
			return;
		}
		
		m_pHideExpr->Eval(bHidden);
		
	}
	else
	{
		if (IsDlgButtonChecked(IDC_RADIO_HIDENEVER))
			 bHidden = FALSE;
		
		delete m_pHideExpr;
		m_pHideExpr = NULL;
	}
	
	m_bHidden = bHidden;
	
	EndDialog(IDOK);
}

//-----------------------------------------------------------------------------
void CHiddenWhenDlg::OnCancel()
{
	EndDialog(IDCANCEL);
}

//-----------------------------------------------------------------------------
void CHiddenWhenDlg::ChangeRadioSelection()
{
  if (IsDlgButtonChecked(IDC_RADIO_HIDEALWAYS) || IsDlgButtonChecked(IDC_RADIO_HIDENEVER) ) 
  {
	 m_edtHideExpr.EnableWindow(FALSE);
  }
  else  //expr in editing
  {
	m_edtHideExpr.EnableWindow(TRUE);
  }
}

//-----------------------------------------------------------------------------
BOOL CHiddenWhenDlg::GetState()
{
	return m_bHidden;
}

//==============================================================================
//          Class CSetTooltipDlg implementation
//==============================================================================
BEGIN_MESSAGE_MAP(CSetTooltipDlg, CParsedDialog)
	//{{AFX_MSG_MAP(CHiddenWhenDlg)
	//}}AFX_MSG_MAP
END_MESSAGE_MAP()

IMPLEMENT_DYNAMIC(CSetTooltipDlg, CParsedDialog)
//------------------------------------------------------------------------------
CSetTooltipDlg::CSetTooltipDlg (Expression*& pExpr, SymTable* pSymTable, CWnd* aParent /*=NULL*/)
	:
	CParsedDialog	(IDD_SET_TOOLTIP, aParent),
	m_edtExpr		(pSymTable),
	m_pExpr			(pExpr),
	m_pSymTable		(pSymTable)
{
}


//-----------------------------------------------------------------------------
BOOL CSetTooltipDlg::OnInitDialog()
{
	CParsedDialog::OnInitDialog();
		
	m_edtExpr. SubclassEdit	(IDC_EDIT_DYNAMICEXPR,       		this);

	if (m_pExpr)
		m_edtExpr.SetWindowText(m_pExpr->ToString());

	return TRUE;
}


//-----------------------------------------------------------------------------
void CSetTooltipDlg::OnOK()
{
	CString strExpr;
	BOOL isNewExpr = FALSE;
	
	m_edtExpr.GetWindowText(strExpr);
	ConvertCString(strExpr, CRLF_TO_LF);
	Parser	lex(strExpr);
		
	if (!m_pExpr)
	{
		m_pExpr = new Expression(m_pSymTable);
		isNewExpr = TRUE;
	}
	if (!m_edtExpr.CheckExp(lex, *m_pExpr, DATA_STR_TYPE))
	{
		if (isNewExpr)
			SAFE_DELETE(m_pExpr);
			
		m_edtExpr.SetCtrlFocus(TRUE);
		return;
	}
			
	EndDialog(IDOK);
}

//-----------------------------------------------------------------------------
void CSetTooltipDlg::OnCancel()
{
	EndDialog(IDCANCEL);
}

//==============================================================================
//          Class CListHiddenColsDlg implementation
//==============================================================================
BEGIN_MESSAGE_MAP(CListHiddenColsDlg, CParsedDialog)
	//{{AFX_MSG_MAP(CListHiddenColsDlg)
		ON_LBN_DBLCLK       (IDC_GENERIC_LIST,			EditHiddenWhen)
	//}}AFX_MSG_MAP
END_MESSAGE_MAP()

IMPLEMENT_DYNAMIC(CListHiddenColsDlg, CParsedDialog)
//------------------------------------------------------------------------------
CListHiddenColsDlg::CListHiddenColsDlg (CColumnArray* pColumnArrayHidden, CColumnArray* pColumnArrayAll, CWoormDocMng*	pDocument, Table* pTable, CWnd* aParent /*=NULL*/)
	:
	CParsedDialog			(IDD_GENERIC_LIST, aParent),
	m_pColumnArrayHidden	(pColumnArrayHidden),
	m_pColumnArrayAll		(pColumnArrayAll),
	m_pDocument				(pDocument),
	m_pTable				(pTable),
	m_lbColumns				(IDB_HIDDEN_FIELD, IDB_HIDDEN_FIELD_EXPR)
{
}

//-----------------------------------------------------------------------------
BOOL CListHiddenColsDlg::OnInitDialog()
{
	CParsedDialog::OnInitDialog();
	CString labelColumn;
	SetWindowText(_TB("Hidden columns"));

	m_lbColumns.SubclassDlgItem(IDC_GENERIC_LIST	,	this);
	
	for (int nCol= 0; nCol <= m_pColumnArrayHidden->GetUpperBound(); nCol++)
	{
		TableColumn* col = (TableColumn*)m_pColumnArrayHidden->GetAt(nCol);
		labelColumn.Empty();

		if (col->m_Title.GetText().IsEmpty())
		{
			SymField* rp =  m_pDocument->m_pEditorManager->GetSymTable()->GetFieldByID (col->GetInternalID());
			
			labelColumn.Append(_T("("));
			labelColumn.Append(rp->GetName()); 
			labelColumn.Append(_T(")"));
		}
		else
			labelColumn.Append(col->m_Title.GetText());

		int idx = m_lbColumns.AddString(labelColumn, _T(""), col->m_pHideExpr ? CMultiListBox::CHECK_TWO : CMultiListBox::CHECK_ONE);
		m_lbColumns.SetItemDataPtr(idx, col);
	}
	return TRUE;
}

//-----------------------------------------------------------------------------
void CListHiddenColsDlg::OnOK()
{
	
	EndDialog(IDOK);
}

//-----------------------------------------------------------------------------
void CListHiddenColsDlg::OnCancel()
{
	EndDialog(IDCANCEL);
}

//-----------------------------------------------------------------------------
void CListHiddenColsDlg::EditHiddenWhen()
{
	int selected = m_lbColumns.GetCurSel();
	if (selected < 0) return;

	TableColumn* col = (TableColumn*) m_lbColumns.GetItemDataPtr(selected);
	for (int nCol = 0; nCol <= m_pColumnArrayAll->GetUpperBound(); nCol++)
	{
		if (col == (TableColumn*)m_pColumnArrayAll->GetAt(nCol))
		{
			m_pTable->ColumnToggleHidden(nCol);

			m_lbColumns.SetFlag(selected, col->m_pHideExpr ? CMultiListBox::CHECK_TWO : (col->m_bIsHidden ? CMultiListBox::CHECK_ONE : CMultiListBox::UNCHECKED));
			return;
		}
	}
}

//==============================================================================
//          Class CEditTextExprDlg implementation
//==============================================================================
BEGIN_MESSAGE_MAP(CEditTextExprDlg, CParsedDialog)
	//{{AFX_MSG_MAP(CEditTextExprDlg)
	ON_BN_CLICKED       (IDC_RADIO_DESCR,		ChangeRadioSelection)
	ON_BN_CLICKED       (IDC_RADIO_COND_DESCR,	ChangeRadioSelection)
	//}}AFX_MSG_MAP
END_MESSAGE_MAP()

IMPLEMENT_DYNAMIC(CEditTextExprDlg, CParsedDialog)
//------------------------------------------------------------------------------
CEditTextExprDlg::CEditTextExprDlg (CString& strLabel, Expression*&	pExpr, WoormTable* pSymTable, CWnd* aParent /*=NULL*/)
	:
	CParsedDialog	(IDD_EXPRTEXT, aParent),
	m_pSymTable		(pSymTable),
	m_pExpr			(pExpr),
	m_strLabel		(strLabel)
{
	m_edtTextExpr.SetSymbolTable(pSymTable);
}


//-----------------------------------------------------------------------------
BOOL CEditTextExprDlg::OnInitDialog()
{
	CParsedDialog::OnInitDialog();
	

	m_TextEdit.				SubclassDlgItem(IDC_TEXTDATA, this);
	m_StaticDescRadioBtn.	SubclassDlgItem(IDC_RADIO_DESCR, this);
	m_CondDescRadioBtn.		SubclassDlgItem(IDC_RADIO_COND_DESCR, this);;

	m_edtTextExpr.	SubclassEdit	(IDC_EDIT_TEXTEXPR,       		this);
	m_TextEdit.SetWindowText(m_strLabel);
	CheckDlgButton(IDC_RADIO_DESCR, TRUE);
	
	if (m_pExpr != NULL)
	{
		m_edtTextExpr.SetWindowText(m_pExpr->ToString());

		CheckDlgButton(IDC_RADIO_DESCR, FALSE);
		CheckDlgButton(IDC_RADIO_COND_DESCR, TRUE);
	}
	ChangeRadioSelection();
	return TRUE;
}


//-----------------------------------------------------------------------------
void CEditTextExprDlg::OnOK()
{
	if (IsDlgButtonChecked	(IDC_RADIO_COND_DESCR))
	{
		CString strExpr;
		BOOL isNewExpr = FALSE;

		m_edtTextExpr.GetWindowText(strExpr);
		ConvertCString(strExpr, CRLF_TO_LF);
		Parser	lex(strExpr);
		
		if (strExpr.IsEmpty())         //espressione vuota
		{
			if (m_pExpr)					//se prima c'era expr la elimino
				SAFE_DELETE(m_pExpr);
			EndDialog(IDOK);
			return;
		}
		
		if (!m_pExpr)                     
		{
			m_pExpr = new Expression(m_pSymTable);
			isNewExpr = TRUE;
		}
		if (!m_edtTextExpr.CheckExp(lex, *m_pExpr, DATA_STR_TYPE))
		{
			if (isNewExpr)
				SAFE_DELETE(m_pExpr);
			
			m_edtTextExpr.SetCtrlFocus(TRUE);
			return;
		}
	}
	else if (IsDlgButtonChecked	(IDC_RADIO_DESCR))
	{
		GetDlgItemText(IDC_TEXTDATA, m_strLabel);
		SAFE_DELETE(m_pExpr); //descrizione statica, quindi l'espressione viene cancellata
	}

	EndDialog(IDOK);
}

//-----------------------------------------------------------------------------
void CEditTextExprDlg::OnCancel()
{
	EndDialog(IDCANCEL);
}

//-----------------------------------------------------------------------------
void CEditTextExprDlg::ChangeRadioSelection()
{
  if (IsDlgButtonChecked	(IDC_RADIO_COND_DESCR))
  {
	  m_TextEdit.		EnableWindow(FALSE);
	  m_edtTextExpr.	EnableWindow(TRUE);
  }
if (IsDlgButtonChecked	(IDC_RADIO_DESCR))
  {
	  m_TextEdit.		EnableWindow(TRUE);
	  m_edtTextExpr.	EnableWindow(FALSE);
  }
}
	
//==============================================================================
//          Class CAnchorFieldToColumnsDlg implementation
//==============================================================================
BEGIN_MESSAGE_MAP(CAnchorFieldToColumnsDlg, CParsedDialog)
	//{{AFX_MSG_MAP(CAnchorFieldToColumnsDlg)
	  ON_CBN_SELCHANGE    (IDC_CMB_TABLES,				TableChanged)
	  ON_CBN_SELCHANGE    (IDC_CMB_LEFTCOLUMNS,			ColumnLeftChanged)
	  ON_CBN_SELCHANGE    (IDC_CMB_RIGHTCOLUMNS,		ColumnRightChanged)
	//}}AFX_MSG_MAP
END_MESSAGE_MAP()

IMPLEMENT_DYNAMIC(CAnchorFieldToColumnsDlg, CParsedDialog)

//------------------------------------------------------------------------------
CAnchorFieldToColumnsDlg::CAnchorFieldToColumnsDlg(CWoormDocMng* pDocument, BaseRect* pBaseRect)
	:
	CParsedDialog		(IDD_ANCHOR_FIELD_TO_COLUMNS, pDocument->GetFirstView()),
	m_pDocument			(pDocument),
	m_pBaseRect			(pBaseRect),
	m_SelectedLeftColumnID	(pBaseRect->m_AnchorLeftColumnID),
	m_SelectedRightColumnID	(pBaseRect->m_AnchorRightColumnID),
	m_pTable			(NULL)
{
}	
	
//------------------------------------------------------------------------------
void CAnchorFieldToColumnsDlg::OnOK()
{
	if (
		m_pBaseRect->m_AnchorLeftColumnID == m_SelectedLeftColumnID &&
		( 
			m_pBaseRect->m_AnchorRightColumnID == m_SelectedRightColumnID 
			|| 
			(m_pBaseRect->m_AnchorRightColumnID == 0 && m_SelectedLeftColumnID == m_SelectedRightColumnID)
		) &&
		((CButton*)GetDlgItem(IDC_RADIO_ANCHOR))->GetCheck())  //non e' cambiato colonna cui e' ancorato
	{
		EndDialog(IDOK);
		return;
	}

	if (m_pBaseRect->m_AnchorLeftColumnID)  //era ancorato a una colonna, lo rimuovo
	{
		m_pBaseRect->FreeFieldFromColumn(&m_pDocument->GetObjects());
	}

	if (((CButton*)GetDlgItem(IDC_RADIO_ANCHOR))->GetCheck()) // lo ancoro ad una nuova colonna
	{
		if (
			m_SelectedLeftColumnID == m_SelectedRightColumnID &&
			m_pBaseRect && m_pBaseRect->IsKindOf(RUNTIME_CLASS(FieldRect)) &&
			((CButton*)GetDlgItem(IDC_CHK_ANCHOR_HIDDEN_TITLE))->GetCheck() == 1
			)
		{
			ASSERT(m_pTable);
			TableColumn* pCol = m_pTable->GetColumnByAlias (m_SelectedLeftColumnID);

			m_pBaseRect->SetHidden();
			FieldRect* pF = (FieldRect*) m_pBaseRect;
				
			Expression*& pTitleExpr = 	pCol->GetDynamicExpr (/*EnumDynamicExpr::*/TitleExpr);
			SAFE_DELETE(pTitleExpr);
			pTitleExpr = new Expression(&(m_pDocument->m_ViewSymbolTable));
			CString sExpr = pF->GetDataType() == DataType::String ? pF->GetFieldName() : cwsprintf(_T("Format(%s)"), pF->GetFieldName());
			Parser lex(sExpr);
			pTitleExpr->Parse(lex, DataType::String, TRUE);

			m_pDocument->InvalidateRect(pCol->GetColumnTitleRect());
		}
		m_pBaseRect->AnchorFieldToColumn(&m_pDocument->GetObjects(), m_SelectedLeftColumnID, m_SelectedRightColumnID);
	}
	
	EndDialog(IDOK);
}

//------------------------------------------------------------------------------
BOOL CAnchorFieldToColumnsDlg::OnInitDialog()
{
	int nIdx;
	CParsedDialog::OnInitDialog();

	VERIFY (m_cbxTables.SubclassDlgItem	(IDC_CMB_TABLES,	this));
	VERIFY (m_cbxLeftCols.SubclassDlgItem		(IDC_CMB_LEFTCOLUMNS,	this));
	VERIFY (m_cbxRightCols.SubclassDlgItem		(IDC_CMB_RIGHTCOLUMNS,	this));

	((CButton*)GetDlgItem(IDC_RADIO_ANCHOR))->SetCheck(TRUE);
	GetDlgItem(IDC_CHK_ANCHOR_HIDDEN_TITLE)->ShowWindow(
		(m_pBaseRect && m_pBaseRect->IsKindOf(RUNTIME_CLASS(FieldRect))) ? SW_NORMAL : SW_HIDE);
	 ((CButton*)GetDlgItem(IDC_CHK_ANCHOR_HIDDEN_TITLE))->SetCheck(0);

	Table* pTable = NULL;
	for (int i = 0; i <= m_pDocument->GetObjects().GetUpperBound(); i++)
	{
		if (m_pDocument->GetObjects()[i]->IsKindOf(RUNTIME_CLASS(Table)))
		{
			pTable = (Table*) m_pDocument->GetObjects()[i];
			ASSERT (pTable);
			if (pTable->GetTableLocalizedTitleText(m_pDocument) == _T(""))
				nIdx = m_cbxTables.AddString(cwsprintf(_TB("Table_Alias_{0-%d}"), pTable->GetInternalID()));
			else
				nIdx = m_cbxTables.AddString(pTable->GetTableLocalizedTitleText(m_pDocument));
			
			if (nIdx >= 0)
				m_cbxTables.SetItemDataPtr(nIdx, pTable);
		}
	}

	if (m_pBaseRect->m_AnchorLeftColumnID == 0)
	{		
		if (m_cbxTables.GetCount() == 0)
		{
			AfxMessageBox(_TB("No table present to anchor the field"));
			EndDialog(IDCANCEL);
			return TRUE;
		}
		m_cbxTables.SetCurSel(0);
		TableChanged(); //per forzare caricamento colonne
	}
	else	//(m_pBaseRect->m_AnchorColumnID != 0)
	{
		pTable = NULL;
		for (int i = 0; i <= m_pDocument->GetObjects().GetUpperBound(); i++)
		{
			if (m_pDocument->GetObjects()[i]->IsKindOf(RUNTIME_CLASS(Table)))
			{
				pTable = (Table*) m_pDocument->GetObjects()[i];
				for (int nCol = 0; nCol < pTable->GetColumns().GetSize(); nCol++)
				{
					const TableColumn* pCol = pTable->GetColumn(nCol);
					if (pCol->GetInternalID() == m_SelectedLeftColumnID)
					{
						CString strCurrCol = pCol->GetDescription();
						
						CString strCurrTable = pTable->GetTableLocalizedTitleText(m_pDocument);

						int nIndex = 0;
						if ( (nIndex = m_cbxTables.FindString(nIndex, strCurrTable)) != LB_ERR)
								m_cbxTables.SetCurSel( nIndex );
						else 
						{	
							strCurrTable = cwsprintf(_TB("Table_Alias_{0-%d}"), pTable->GetInternalID());
							if ( (nIndex = m_cbxTables.FindString(nIndex, strCurrTable)) != LB_ERR)
								m_cbxTables.SetCurSel( nIndex );
						}	

						TableChanged();
						
						for (int nIndex = 0; nIndex < m_cbxLeftCols.GetCount(); nIndex++)
						{
							TableColumn* pC = (TableColumn*)m_cbxLeftCols.GetItemDataPtr(nIndex);
							if (pCol == pC)
							{
								m_cbxLeftCols.SetCurSel(nIndex);
								m_SelectedLeftColumnID = m_pBaseRect->m_AnchorLeftColumnID;

								if (m_pBaseRect->m_AnchorRightColumnID == 0)
								{
									m_cbxRightCols.SetCurSel(nIndex);
									m_SelectedRightColumnID = m_SelectedLeftColumnID;
									break;
								}
							}
							if (m_pBaseRect->m_AnchorRightColumnID == pC->GetInternalID())
							{
								m_cbxRightCols.SetCurSel(nIndex);
								m_SelectedRightColumnID = m_pBaseRect->m_AnchorRightColumnID;
							}
						}
						return TRUE;
					}
				}
			}
		}
	}
	return TRUE;  // return TRUE  unless you set the focus to a control
}

//------------------------------------------------------------------------------
void CAnchorFieldToColumnsDlg::TableChanged	()
{
	Table* pTable = NULL;
	int currIdxTable = m_cbxTables.GetCurSel();

	pTable = (Table*)m_cbxTables.GetItemDataPtr(currIdxTable);
	ASSERT (pTable);
	if (pTable == NULL)
		return;

	m_pTable = pTable;

	FillColumns(m_cbxLeftCols, m_SelectedLeftColumnID);
	FillColumns(m_cbxRightCols, m_SelectedRightColumnID);
}

//------------------------------------------------------------------------------
void CAnchorFieldToColumnsDlg::FillColumns(CComboBox& cbxCols, WORD& wSelectedLeftColumnID)
{
	cbxCols.ResetContent();
	
	BOOL bSelColumnID = FALSE;
	int nIdx;
	for (int nCol = 0; nCol < m_pTable->GetColumns().GetSize(); nCol++)
	{
		const TableColumn* pCol = m_pTable->GetColumn(nCol);
		
		if (pCol->IsHidden())
			continue;
		
		nIdx = cbxCols.AddString(AfxLoadReportString(pCol->GetDescription(), m_pDocument));
		if (nIdx >= 0)
			 cbxCols.SetItemDataPtr(nIdx, const_cast<TableColumn*>(pCol));

		if (!bSelColumnID)
		{
			bSelColumnID = TRUE;
			wSelectedLeftColumnID = pCol->GetInternalID();
		}
	}
	
	cbxCols.SetCurSel(0);
	return;
}

//------------------------------------------------------------------------------
void CAnchorFieldToColumnsDlg::ColumnLeftChanged()
{
	int curLeftSelCol = m_cbxLeftCols.GetCurSel();
	TableColumn* currentLeftCol = (TableColumn*)m_cbxLeftCols.GetItemDataPtr(curLeftSelCol);
	if (currentLeftCol == NULL)
	{
		ASSERT(FALSE);
		return;
	}
	m_SelectedLeftColumnID = currentLeftCol->GetInternalID() ;
	
	//----
	int curRightSelCol = m_cbxRightCols.GetCurSel();
	TableColumn* currentRightCol = (TableColumn*)m_cbxRightCols.GetItemDataPtr(curRightSelCol);
	if (currentRightCol == NULL)
	{
		m_SelectedRightColumnID = m_SelectedLeftColumnID ;
		m_cbxRightCols.SetCurSel(curLeftSelCol);
		return;
	}

	int idxLeft		= currentLeftCol	->GetTable()->GetColumnIndexByAlias(currentLeftCol	->GetInternalID());
	int idxRight	= currentRightCol	->GetTable()->GetColumnIndexByAlias(currentRightCol	->GetInternalID());

	if (idxLeft > idxRight)
	{
		m_SelectedRightColumnID = m_SelectedLeftColumnID ;
		m_cbxRightCols.SetCurSel(curLeftSelCol);
		return;
	}
}

//------------------------------------------------------------------------------
void CAnchorFieldToColumnsDlg::ColumnRightChanged()
{
	int curRightSelCol = m_cbxRightCols.GetCurSel();
	TableColumn* currentRightCol = (TableColumn*)m_cbxRightCols.GetItemDataPtr(curRightSelCol);
	if (currentRightCol == NULL)
	{
		ASSERT(FALSE);
		return;
	}
	m_SelectedRightColumnID = currentRightCol->GetInternalID();
	//----

	int curLeftSelCol = m_cbxLeftCols.GetCurSel();
	TableColumn* currentLeftCol = (TableColumn*)m_cbxLeftCols.GetItemDataPtr(curLeftSelCol);
	if (currentLeftCol == NULL)
	{
		m_SelectedLeftColumnID = m_SelectedRightColumnID ;
		m_cbxLeftCols.SetCurSel(curRightSelCol);
		return;
	}

	int idxLeft		= currentLeftCol	->GetTable()->GetColumnIndexByAlias(currentLeftCol	->GetInternalID());
	int idxRight	= currentRightCol	->GetTable()->GetColumnIndexByAlias(currentRightCol	->GetInternalID());

	if (idxLeft > idxRight)
	{
		m_SelectedLeftColumnID = m_SelectedRightColumnID ;
		m_cbxLeftCols.SetCurSel(curRightSelCol);
		return;
	}
}

//==============================================================================
//          Class CChooseObjectDlg implementation
//==============================================================================
BEGIN_MESSAGE_MAP(CChooseObjectDlg, CParsedDialog)
	//{{AFX_MSG_MAP(CChooseObjectDlg)
	  ON_CBN_SELCHANGE    (IDC_CMB_LAYOUTS,				LayoutChanged)
	  ON_CBN_SELCHANGE    (IDC_CMB_TABLES,				TableChanged)
	//}}AFX_MSG_MAP
END_MESSAGE_MAP()


IMPLEMENT_DYNAMIC(CChooseObjectDlg, CParsedDialog)
//------------------------------------------------------------------------------
CChooseObjectDlg::CChooseObjectDlg(CWoormDocMng* pDocument, CObject* pCurrent/* = NULL*/, BOOL bChooseBaseObj /*= FALSE*/)
	:
	CParsedDialog		(IDD_CHOOSE_OBJECT, pDocument->GetFirstView()),
	m_pDocument			(pDocument),
	m_pSelectedBaseRect	(NULL),
	m_pSelectedCol		(NULL),
	m_pSelectedTable	(NULL),
	m_pSelectedLayout	(NULL),
	m_pSelected			(NULL),
	m_pCurrentObj		(pCurrent),
	m_bChooseBaseObj	(bChooseBaseObj)
{
}	
	
//------------------------------------------------------------------------------
BOOL CChooseObjectDlg::OnInitDialog()
{
	CParsedDialog::OnInitDialog();

	VERIFY (m_cbxLayouts.SubclassDlgItem		(IDC_CMB_LAYOUTS,		this));
	VERIFY (m_cbxTables.SubclassDlgItem			(IDC_CMB_TABLES,		this));
	VERIFY (m_cbxFieldsColumns.SubclassDlgItem	(IDC_CMB_LEFTCOLUMNS,	this));

	FillLayouts	();

	return TRUE;  // return TRUE  unless you set the focus to a control
}

//------------------------------------------------------------------------------
void CChooseObjectDlg::FillLayouts()
{
	m_cbxLayouts.ResetContent();

	CString strName; CObject* pObj = NULL;
	for (POSITION pos = m_pDocument->m_Layouts.GetStartPosition(); pos != NULL; pObj = NULL, strName.Empty())
	{
		m_pDocument->m_Layouts.GetNextAssoc (pos, strName, pObj );
		CLayout* pObjects = (CLayout*) pObj;
		
		if (m_bChooseBaseObj && pObjects->m_strLayoutName.CompareNoCase(m_pDocument->GetObjects().m_strLayoutName) == 0)
			continue;

		int nIdx = m_cbxLayouts.AddString(pObjects->m_strLayoutName); 
		if (nIdx >= 0)
			m_cbxLayouts.SetItemDataPtr(nIdx, pObjects);
	}

	m_cbxLayouts.SetCurSel(m_bChooseBaseObj ? 0 : m_cbxLayouts.FindString(-1, m_pDocument->GetObjects().m_strLayoutName));

	LayoutChanged ();
}

//------------------------------------------------------------------------------
void CChooseObjectDlg::FillTables(CLayout* pObjects)
{
	m_cbxTables.ResetContent();
	int nIdx;
	m_cbxTables.AddString('<' + _TB("Free fields") + '>');
	Table* pTable = NULL;
	for (int i = 0; i <= pObjects->GetUpperBound(); i++)
	{
		BaseObj * pO = (*pObjects)[i];

		if (pO->IsKindOf(RUNTIME_CLASS(Table)))
		{
			pTable = (Table*) pO;
			
			//if (pTable->GetTableLocalizedTitleText(m_pDocument) == _T(""))
			//	nIdx = m_cbxTables.AddString(cwsprintf(_TB("Table_Alias_{0-%d}"), pTable->GetInternalID()));
			//else
			nIdx = m_cbxTables.AddString(pTable->GetDescription()); //GetTableLocalizedTitleText(m_pDocument)
			if (nIdx >= 0)
				m_cbxTables.SetItemDataPtr(nIdx, pTable);
		}
	}
	m_cbxTables.SetCurSel(0);

	TableChanged ();
}

//------------------------------------------------------------------------------
void CChooseObjectDlg::FillColumns	(CLayout* pObjects)
{
	m_cbxFieldsColumns.ResetContent();
	if (m_pSelectedTable == NULL)
		return;
	
	int nIdx;
	for (int nCol = 0; nCol < m_pSelectedTable->GetColumns().GetSize(); nCol++)
	{
		const TableColumn* pCol = m_pSelectedTable->GetColumn(nCol);

		if (pCol == m_pCurrentObj)
			continue;

		CString s (pCol->GetDescription());
		nIdx = m_cbxFieldsColumns.AddString(s);
		if (nIdx >= 0)
			 m_cbxFieldsColumns.SetItemDataPtr(nIdx, const_cast<TableColumn*>(pCol));
	}
	
	m_cbxFieldsColumns.SetCurSel(0);
	return;
}

//------------------------------------------------------------------------------
void CChooseObjectDlg::FillFields(CLayout* pObjects)
{
	m_cbxFieldsColumns.ResetContent();
	
	int nIdx;
	for (int i = 0; i <= pObjects->GetUpperBound(); i++)
	{
		BaseObj* pO = (*pObjects)[i];
		if (pO == m_pCurrentObj)
			continue;

		if (pO->IsKindOf(RUNTIME_CLASS(BaseRect)))
		{
			if (pO->IsKindOf(RUNTIME_CLASS(FieldRect)))
			{
				if (((FieldRect*)pO)->GetInternalID() >= SpecialReportField::REPORT_LOWER_SPECIAL_ID)
					continue;
			}

			BaseRect* pObj = (BaseRect*) pO;
			
			CString  s(pObj->GetDescription());

			nIdx = m_cbxFieldsColumns.AddString(s);
			if (nIdx >= 0)
				 m_cbxFieldsColumns.SetItemDataPtr(nIdx, pObj);
		}
	}
	
	m_cbxFieldsColumns.SetCurSel(0);
	return;
}

//------------------------------------------------------------------------------
void CChooseObjectDlg::LayoutChanged	()
{
	int currIdx = m_cbxLayouts.GetCurSel();
	if (currIdx < 0)
	{
		m_cbxTables.ResetContent();
		return;
	}

	m_pSelectedLayout = (CLayout*) m_cbxLayouts.GetItemDataPtr(currIdx);
		
	FillTables(m_pSelectedLayout);
}

//------------------------------------------------------------------------------
void CChooseObjectDlg::TableChanged	()
{
	Table* pTable = NULL;
	int currIdxTable = m_cbxTables.GetCurSel();

	pTable = (Table*)m_cbxTables.GetItemDataPtr(currIdxTable);
	m_pSelectedTable = pTable;

	if (pTable == NULL)
		FillFields(m_pSelectedLayout);
	else if (!m_bChooseBaseObj)
		FillColumns(m_pSelectedLayout);
	else
		m_cbxFieldsColumns.ResetContent();
}

//------------------------------------------------------------------------------
void CChooseObjectDlg::OnOK()
{
	int currIdxTable = m_cbxTables.GetCurSel();
	int currIdxFields = m_cbxFieldsColumns.GetCurSel();

	if (m_bChooseBaseObj && currIdxFields == -1)
	{
		 if (m_pSelectedTable)
			EndDialog(IDOK);
		return;
	}

	if (currIdxTable == -1 || currIdxFields == -1)
		return;

	m_pSelected = (CObject*) m_cbxFieldsColumns.GetItemDataPtr(currIdxFields);
	if (!m_pSelected)
		return;

	if (m_pSelected->IsKindOf(RUNTIME_CLASS(TableColumn)))
		m_pSelectedCol = (TableColumn*)m_pSelected;
	else if (m_pSelected->IsKindOf(RUNTIME_CLASS(BaseRect)))
		m_pSelectedBaseRect = (BaseRect*)m_pSelected;

	EndDialog(IDOK);
}

//==============================================================================
//          Class CChooseLayoutDlg implementation
//==============================================================================
BEGIN_MESSAGE_MAP(CChooseLayoutDlg, CParsedDialog)
	//{{AFX_MSG_MAP(CChooseLayoutDlg)
	  ON_CBN_SELCHANGE    (IDC_CMB_LAYOUTS,				LayoutChanged)
	//}}AFX_MSG_MAP
END_MESSAGE_MAP()

IMPLEMENT_DYNAMIC(CChooseLayoutDlg, CParsedDialog)

//------------------------------------------------------------------------------
CChooseLayoutDlg::CChooseLayoutDlg(CWoormDocMng* pDocument, CWnd* parent)
	:
	CParsedDialog		(IDD_CHOOSE_LAYOUT, parent ? parent : pDocument->GetFirstView() ),
	m_pDocument			(pDocument),
	m_pSelectedLayout	(NULL)
{
}	
	
//------------------------------------------------------------------------------
BOOL CChooseLayoutDlg::OnInitDialog()
{
	CParsedDialog::OnInitDialog();

	VERIFY (m_cbxLayouts.SubclassDlgItem		(IDC_CMB_LAYOUTS,		this));

	FillLayouts	();

	return TRUE;  // return TRUE  unless you set the focus to a control
}

//------------------------------------------------------------------------------
void CChooseLayoutDlg::FillLayouts()
{
	m_cbxLayouts.ResetContent();

	CString strName; CObject* pObj = NULL;
	for (POSITION pos = m_pDocument->m_Layouts.GetStartPosition(); pos != NULL; pObj = NULL, strName.Empty())
	{
		m_pDocument->m_Layouts.GetNextAssoc (pos, strName, pObj );
		CLayout* pObjects = (CLayout*) pObj;
		
		if (pObjects == &(m_pDocument->GetObjects()))
			continue;

		int nIdx = m_cbxLayouts.AddString(pObjects->m_strLayoutName); 
		if (nIdx >= 0)
			m_cbxLayouts.SetItemDataPtr(nIdx, pObjects);
	}

	m_cbxLayouts.SetCurSel(0);

	LayoutChanged ();
}

//------------------------------------------------------------------------------
void CChooseLayoutDlg::LayoutChanged	()
{
	int currIdx = m_cbxLayouts.GetCurSel();
	if (currIdx < 0)
	{
		return;
	}

	m_pSelectedLayout = (CLayout*) m_cbxLayouts.GetItemDataPtr(currIdx);
}

//------------------------------------------------------------------------------
void CChooseLayoutDlg::OnOK()
{
	if (m_pSelectedLayout)
	{
		EndDialog(IDOK);
		return;
	}
}

//==============================================================================
//          Class CSetNameDlg implementation
//==============================================================================

IMPLEMENT_DYNAMIC(CSetNameDlg, CParsedDialog)

CSetNameDlg::CSetNameDlg (CWoormDocMng*	pDocument, CWnd* aParent /*=NULL*/)
	:
	CParsedDialog	(IDD_SET_NAME, aParent),
	m_pDocument		(pDocument)
{
}

//------------------------------------------------------------------------------
BOOL CSetNameDlg::OnInitDialog()
{
	CParsedDialog::OnInitDialog();

	return TRUE;  // return TRUE  unless you set the focus to a control
}

//------------------------------------------------------------------------------
void CSetNameDlg::OnOK ()
{
	GetDlgItem(IDC_SET_NAME)->GetWindowText(m_sName);
	m_sName.Trim();
	
	EndDialog(IDOK);
}

//==============================================================================
//          Class CSetNameDlg implementation
//==============================================================================

IMPLEMENT_DYNAMIC(CSetPassKeyDlg, CParsedDialogWithTiles)

CSetPassKeyDlg::CSetPassKeyDlg(CWnd* aParent /*=NULL*/)
	:
	CParsedDialogWithTiles(IDD_SET_PASSKEY, aParent),
	m_pDocument(NULL)
{
}

//------------------------------------------------------------------------------
BOOL CSetPassKeyDlg::OnInitDialog()
{
	__super::OnInitDialog();

	SetToolbarStyle(CParsedDialog::BOTTOM, DEFAULT_TOOLBAR_HEIGHT, TRUE, TRUE);

	return TRUE;  // return TRUE  unless you set the focus to a control
}

//------------------------------------------------------------------------------
void CSetPassKeyDlg::OnOK()
{
	GetDlgItem(IDC_SET_PASSKEY)->GetWindowText(m_sPassKey);
	m_sPassKey.Trim();

	if (ValidateKey())
	{
		EnableTBS();
		EndDialog(IDOK);
	}
}

BOOL CSetPassKeyDlg::ValidateKey()
{
	return m_sPassKey.CompareNoCase(L"250XH24PG17G7YMW") == 0;
}

BOOL CSetPassKeyDlg::EnableTBS()
{
	AfxGetApplicationContext()->SetCanUseReportEditor(TRUE);
	const_cast<CLoginInfos*>(AfxGetLoginInfos())->m_bEasyBuilderDeveloper = TRUE;
	AfxGetApplicationContext()->SetPassKeyActive(TRUE);
	return TRUE;
}

//==============================================================================
//          Class CListStyleDlg implementation
//==============================================================================
BEGIN_MESSAGE_MAP(CListStyleDlg, CParsedDialog)
	//{{AFX_MSG_MAP(CListStyleDlg)
		ON_LBN_DBLCLK   (IDC_GENERIC_LIST,				OnDblClick)
		ON_CONTROL      (BN_CLICKED, IDC_CLASS_NEW,		OnNew)
		ON_CONTROL      (BN_CLICKED, IDC_CLASS_SPECIAL_OK,		OnSpecialOK)
		ON_CONTROL      (BN_CLICKED, IDC_CLASS_EMPTY,			OnEmptyOK)
	//}}AFX_MSG_MAP
END_MESSAGE_MAP()
IMPLEMENT_DYNAMIC(CListStyleDlg, CParsedDialog)

//------------------------------------------------------------------------------
CListStyleDlg::CListStyleDlg (CWoormDocMng*	pDocument, CString sClass, CObject* pObj, CWnd* aParent /*=NULL*/)
	:
	CParsedDialog			(IDD_CLASS_LIST, aParent ? aParent : pDocument->GetFirstView()),
	m_pDocument				(pDocument),
	m_sStyleClass			(sClass),
	m_pObject				(pObj)
{
}


//-----------------------------------------------------------------------------
BOOL CListStyleDlg::OnInitDialog()
{
	CParsedDialog::OnInitDialog();

	m_lbClasses.SubclassDlgItem(IDC_GENERIC_LIST,	this);
	m_lbClasses.AddString(_T("<Default>"), _T(""));

	if (m_pDocument->m_Template.m_pWoormTpl)
	{
		CString strName; CObject* pAr = NULL;
		for (POSITION pos = m_pDocument->m_Template.m_pWoormTpl->m_Layouts.GetStartPosition(); pos != NULL; pAr = NULL, strName.Empty())
		{
			m_pDocument->m_Template.m_pWoormTpl->m_Layouts.GetNextAssoc (pos, strName, pAr);
			CLayout* pObjects = (CLayout*) pAr;

			for (int i = 0; i <= pObjects->GetUpperBound(); i++)
			{
				BaseObj* pObj = (*pObjects)[i]; 

				if (pObj->IsKindOf(RUNTIME_CLASS(Table)))
				{
					if (m_pObject->IsKindOf(RUNTIME_CLASS(BaseRect)))
						continue;

					Table* pT = (Table*) pObj;

					if (m_pObject->IsKindOf(RUNTIME_CLASS(Table)))
					{
						if (pT->m_sStyleClass.IsEmpty())
							continue;
						if (m_lbClasses.SearchString(pT->m_sStyleClass, NULL) > -1)	
							continue;	//evita eventuali doppioni del template
							
						m_lbClasses.AddString(pT->m_sStyleClass, NULL);
						continue;
					}

					if (m_pObject->IsKindOf(RUNTIME_CLASS(TableColumn)))
						for (int nC = 0; nC <= pT->GetColumns().GetUpperBound(); nC++)
						{
							TableColumn* pCol = pT->GetColumns()[nC];

							if (pCol->m_sStyleClass.IsEmpty())
								continue;
							if (m_lbClasses.SearchString(pCol->m_sStyleClass, NULL) > -1)
								continue;	//evita eventuali doppioni del template

							m_lbClasses.AddString(pCol->m_sStyleClass, NULL);
						}
				}
				else if (pObj->IsKindOf(RUNTIME_CLASS(BaseRect)))
				{
					if (m_pObject->GetRuntimeClass() != pObj->GetRuntimeClass())
						continue;

					BaseRect* pR = (BaseRect*) pObj;

					if (pR->m_sStyleClass.IsEmpty())
						continue;

					if (pR->m_sStyleClass.CompareNoCase(CWoormTemplate::s_sCopyStaticObject_StyleName) == 0)
						continue;
					if (m_lbClasses.SearchString(pR->m_sStyleClass, NULL) > -1)
						continue;	//evita eventuali doppioni del template
				
					m_lbClasses.AddString(pR->m_sStyleClass, NULL);
				}
			}
		}
	}
	m_lbClasses.AddString(CWoormTemplate::s_sCustom_StyleName, _T(""));

	//NO! troppo casino! TODO aggiungere le classi eventualmente definite nel report corrente
	return TRUE;
}

//-----------------------------------------------------------------------------
void CListStyleDlg::OnOK()
{
	int selected = m_lbClasses.GetCurSel();
	if (selected >= 0)
	{
		m_sStyleClass = m_lbClasses.GetString1(selected);
		
		EndDialog(IDOK);
	}
}

//-----------------------------------------------------------------------------
void CListStyleDlg::OnSpecialOK()
{
	m_sStyleClass = CWoormTemplate::s_sCopyStaticObject_StyleName;
	
	EndDialog(IDOK);
}

//-----------------------------------------------------------------------------
void CListStyleDlg::OnEmptyOK()
{
	m_sStyleClass.Empty();
	
	EndDialog(IDOK);
}

//-----------------------------------------------------------------------------
void CListStyleDlg::OnCancel()
{
	EndDialog(IDCANCEL);
}

//-----------------------------------------------------------------------------
void CListStyleDlg::OnNew()
{
	CString sNewClass;
	GetDlgItem(IDC_CLASS_NEW_EDIT)->GetWindowText(sNewClass);
	sNewClass.Trim();
	if (sNewClass.IsEmpty())
		return;
	int idx = m_lbClasses.AddString(sNewClass, _T(""));
	m_lbClasses.SetSel(idx);
}

//-----------------------------------------------------------------------------
void CListStyleDlg::OnDblClick()
{
	OnOK();
}
