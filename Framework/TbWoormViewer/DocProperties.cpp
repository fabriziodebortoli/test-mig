
#include "stdafx.h"

#include <TbNameSolver\Chars.h>
#include <TbClientCore\ClientObjects.h>
#include <TbWebServicesWrappers\LoginManagerInterface.h>

#include <TbGeneric\globals.h>
#include <TbParser\Parser.h>

#include <TbGenlib\generic.h>
#include <TbGenlib\baseapp.h>

#include "woormdoc.h"
#include "viewpars.h"
#include "docproperties.h"
#include "docproperties.hjson" //JSON AUTOMATIC UPDATE

//includere come ultimo include all'inizio del cpp
#include "begincpp.dex"

#ifdef _DEBUG
#undef THIS_FILE
static const char THIS_FILE[] = __FILE__;
#endif

//-----------------------------------------------------------------------------
CString MakeDosName (const CString& strName)
{
	CString strTmp (strName);
	TCHAR* szTmp = strTmp.GetBuffer(9);
	BOOL bMaxLength =  strTmp.GetLength() > 8;
	int nCount = (bMaxLength)
				? 5
				: strTmp.GetLength();

	for (int i = 0; i <= nCount; i++)
	{
		if	(
				(szTmp[i] == URL_SLASH_CHAR)||
				(szTmp[i] == SLASH_CHAR)	||
				(szTmp[i] == _T(':'))	||
				(szTmp[i] == ASTERISK_CHAR)	||
				(szTmp[i] == _T('?'))	||
				(szTmp[i] == DBL_QUOTE_CHAR)||
				(szTmp[i] == _T('<')) ||
				(szTmp[i] == _T('>')) ||
				(szTmp[i] == _T('|')) ||
				(szTmp[i] == BLANK_CHAR) 
			)
		   szTmp[i] = _T('_');    // invalid char
	}

	if (bMaxLength) 
	{
		szTmp[6] = _T('~');
		szTmp[7] = _T('1');
	}
	szTmp[8] = NULL_CHAR;

	return szTmp;
}


/////////////////////////////////////////////////////////////////////////////
// CDocProperties 
/////////////////////////////////////////////////////////////////////////////
//
CDocProperties::CDocProperties(CWoormDocMng* pWoormDoc)
: 
	m_pWoormDoc(pWoormDoc),
	m_bModifyFlag(FALSE)
{
}

//---------------------------------------------------------------------------
BOOL CDocProperties::IsEmpty()
{ 
	return (
				m_strTitle.IsEmpty() &&
				m_strSubject.IsEmpty() &&
				m_strAuthor.IsEmpty() &&
				m_strCompany.IsEmpty() &&
				m_strComments.IsEmpty() &&
				m_strDefaultSecurityRoles.IsEmpty()
			);
}

//---------------------------------------------------------------------------
BOOL CDocProperties::Parse(Parser& lex)
{ 
	m_strTitle.Empty();
	m_strSubject.Empty();
	m_strAuthor.Empty();
	m_strCompany.Empty();
	m_strComments.Empty();
	m_strDefaultSecurityRoles.Empty();

	m_bModifyFlag	= FALSE;	

	// non è una sezione obbligatoria
	if (!lex.Matched(T_PROPERTIES)) 
		return TRUE;

	if (!lex.ParseBegin())
	   return FALSE;
		
	for(;;)
	{
		switch(lex.LookAhead())
		{
			case T_END:
			{
				return lex.ParseEnd();
			}
			case T_TITLE:
			{
				if (!lex.Matched(T_TITLE) || !lex.ParseString(m_strTitle))
					return FALSE;
				m_bModifyFlag	= TRUE;	
				break;
			}
			case T_SUBJECT:
			{
				if (!lex.Matched(T_SUBJECT) || !lex.ParseCEdit(m_strSubject))
					return FALSE;
				m_bModifyFlag	= TRUE;	
				break;
			}
			case T_AUTHOR:
			{
				if (!lex.Matched(T_AUTHOR) || !lex.ParseString(m_strAuthor))
					return FALSE;
				m_bModifyFlag	= TRUE;	
				break;
			}
			case T_REPORTPRODUCER:
			{
				if (!lex.Matched(T_REPORTPRODUCER) || !lex.ParseString(m_strCompany))
					return FALSE;
				m_bModifyFlag	= TRUE;	
				break;
			}
			case T_COMMENTS:
			{
				if (!lex.Matched(T_COMMENTS) || !lex.ParseCEdit(m_strComments))
					return FALSE;
				m_bModifyFlag	= TRUE;	
				break;
			}
			case T_DEFAULTSECURITYROLES:
			{
				if (!lex.Matched(T_DEFAULTSECURITYROLES) || !lex.ParseString(m_strDefaultSecurityRoles))
					return FALSE;
				m_bModifyFlag	= TRUE;	
				break;
			}
			default:
			{
				CString sCompany;
				if (!lex.ParseID(sCompany))
					return FALSE;
				if (sCompany.CompareNoCase(_T("Company")) == 0)
				{
					if (!lex.ParseString(m_strCompany))
						return FALSE;
					m_bModifyFlag	= TRUE;	
					break;
				}
				//----
				return FALSE;
			}
		}
	}
	m_bModifyFlag	= TRUE;	
	return TRUE;
}


//---------------------------------------------------------------------------
void CDocProperties::Unparse(Unparser& oFile)
{
	oFile.UnparseCrLf	();
    oFile.UnparseTag	(T_PROPERTIES,	TRUE);
	oFile.UnparseBegin	();

 	ViewUnparser* pVUp = (ViewUnparser*)&oFile;

    oFile.UnparseTag	(T_TITLE,		FALSE);
	oFile.UnparseString	(pVUp->LoadReportString(m_strTitle),	TRUE);

	oFile.UnparseTag	(T_SUBJECT,		FALSE);
	oFile.UnparseCEdit	(pVUp->LoadReportString(m_strSubject),	TRUE);

	oFile.UnparseTag	(T_AUTHOR,		FALSE);
	oFile.UnparseString	(m_strAuthor,	TRUE);
	oFile.UnparseTag	(T_REPORTPRODUCER,		FALSE);
	oFile.UnparseString	(m_strCompany,	TRUE);
	oFile.UnparseTag	(T_COMMENTS,	FALSE);
	oFile.UnparseCEdit	(m_strComments,	TRUE);
	oFile.UnparseTag	(T_DEFAULTSECURITYROLES,		FALSE);
	oFile.UnparseString	(m_strDefaultSecurityRoles,	TRUE);
	oFile.UnparseEnd	();
}				


/////////////////////////////////////////////////////////////////////////////
// CGeneralPropPage property page
/////////////////////////////////////////////////////////////////////////////
//
IMPLEMENT_DYNAMIC(CGeneralPropPage, CLocalizablePropertyPage)
CGeneralPropPage::CGeneralPropPage(const CString& strFileName) 
: 
	CLocalizablePropertyPage(IDD_DOCPROP_GENERAL),
	m_strFileName(strFileName)
{
}

//-----------------------------------------------------------------------------
BOOL CGeneralPropPage::OnInitDialog() 
{
	__super::OnInitDialog();

	if (m_strFileName.IsEmpty())
	{
		SetDlgItemText(IDC_DOCPROP_CREATED,		_TB("(unknown)"));
		SetDlgItemText(IDC_DOCPROP_MODIFIED,	_TB("(unknown)"));
		SetDlgItemText(IDC_DOCPROP_ACCESSED,	_TB("(unknown)"));
		return TRUE;
	}

    CFileStatus	fileStatus;
	long		size = 0;
	BYTE		attribute;
	CTime		ctime(1980, 1, 1, 0, 0, 0);  //The date and time the file was created.
	CTime		mtime(1980, 1, 1, 0, 0, 0);  //The date and time the file was last modified.
	CTime		atime(1980, 1, 1, 0, 0, 0);  // The date and time the file was last accessed for reading.

	CString strDisplay = GetName(m_strFileName) + GetExtension(m_strFileName);
	CString strPath = GetPath(m_strFileName);

	SetDlgItemText(IDC_DOCPROP_FILENAME,	(LPCTSTR)strDisplay);
	SetDlgItemText(IDC_DOCPROP_TYPE,		(LPCTSTR)_TB("WOORM document"));
	SetDlgItemText(IDC_DOCPROP_LOCATION,	(LPCTSTR)strPath);

	BOOL bStatus = CLineFile::GetStatus(m_strFileName, fileStatus);
	if (bStatus)
	{
		ctime = fileStatus.m_ctime;
		mtime = fileStatus.m_mtime;
		atime = fileStatus.m_atime;
		size  = (long) fileStatus.m_size;
		attribute = (BYTE)fileStatus.m_attribute;
	}

	//display Size
	//la size la esprimo sia in kb sia in bytes come: num KB (num1 bytes)
	double dKBSize = (size > 0) ? size / 1024 : 0;
	SetDlgItemText(IDC_DOCPROP_SIZE,		(LPCTSTR)cwsprintf(_T("%.2f KB (%d bytes)"),dKBSize, size));

	// display MS-DOS name
	strDisplay = MakeDosName(GetName(m_strFileName)) + GetExtension(m_strFileName);
	strDisplay.MakeUpper();
	SetDlgItemText(IDC_DOCPROP_MSDOS_NAME, (LPCTSTR)strDisplay);

	//display time	
	SetDlgItemText(IDC_DOCPROP_CREATED,		(LPCTSTR)ctime.Format("%A, %d %B, %Y, %H:%M:%S %p"));
	SetDlgItemText(IDC_DOCPROP_MODIFIED,	(LPCTSTR)mtime.Format("%A, %d %B, %Y, %H:%M:%S %p"));
	SetDlgItemText(IDC_DOCPROP_ACCESSED,	(LPCTSTR)atime.Format("%A, %d %B, %Y, %H:%M:%S %p"));

	//display FLAG
	((CButton*)GetDlgItem(IDC_DOCPROP_READONLY))->SetCheck((attribute & CFile::readOnly) == CFile::readOnly);
	((CButton*)GetDlgItem(IDC_DOCPROP_ARCHIVE))->SetCheck((attribute & CFile::archive) == CFile::archive);
	((CButton*)GetDlgItem(IDC_DOCPROP_HIDDEN))->SetCheck((attribute & CFile::hidden) == CFile::hidden);
	((CButton*)GetDlgItem(IDC_DOCPROP_SYSTEM))->SetCheck((attribute & CFile::system) == CFile::system);

	return TRUE;
};

IMPLEMENT_DYNAMIC(CSummaryPage, CLocalizablePropertyPage)
//-----------------------------------------------------------------------------
CSummaryPage::CSummaryPage(CDocProperties* pDocProperties)
:
	CLocalizablePropertyPage(IDD_DOCPROP_SUMMERY),
	m_pDocProperties (pDocProperties)
{
}

//-----------------------------------------------------------------------------
BOOL CSummaryPage::OnInitDialog() 
{
	__super::OnInitDialog();

	BOOL bEnable = m_pDocProperties->m_pWoormDoc->m_bAllowEditing;

	if (bEnable)
	{
		m_strTitle = m_pDocProperties->m_strTitle;
		m_strSubject = m_pDocProperties->m_strSubject;
	}
	else
	{
		m_strTitle = AfxLoadReportString(m_pDocProperties->m_strTitle, m_pDocProperties->m_pWoormDoc);
		m_strSubject = AfxLoadReportString(m_pDocProperties->m_strSubject, m_pDocProperties->m_pWoormDoc);
	}

	((CEdit*)GetDlgItem(IDC_DOCPROP_TITLE))->SetReadOnly(!bEnable);
	((CEdit*)GetDlgItem(IDC_DOCPROP_SUBJECT))->SetReadOnly(!bEnable);
	((CEdit*)GetDlgItem(IDC_DOCPROP_AUTHOR))->SetReadOnly(!bEnable);
	((CEdit*)GetDlgItem(IDC_DOCPROP_COMPANY))->SetReadOnly(!bEnable);
	((CEdit*)GetDlgItem(IDC_DOCPROP_COMMENTS))->SetReadOnly(!bEnable);

	// il namespace su nuovo report non è significativo
	if (m_pDocProperties && m_pDocProperties->m_pWoormDoc && m_pDocProperties->m_pWoormDoc->GetNamespace().GetType () == CTBNamespace::REPORT)
		GetDlgItem(IDC_DOCPROP_NS)->SetWindowText(m_pDocProperties->m_pWoormDoc->GetNamespace().ToString());

	UpdateData(FALSE);
	return TRUE;
}

//-----------------------------------------------------------------------------
void CSummaryPage::DoDataExchange(CDataExchange* pDX)
{
	CLocalizablePropertyPage::DoDataExchange(pDX);
	//{{AFX_DATA_MAP(CGeneralPage)
	DDX_Text(pDX, IDC_DOCPROP_TITLE,		m_strTitle);
	DDX_Text(pDX, IDC_DOCPROP_SUBJECT,		m_strSubject);
	DDX_Text(pDX, IDC_DOCPROP_AUTHOR,		m_pDocProperties->m_strAuthor);
	DDX_Text(pDX, IDC_DOCPROP_COMPANY,		m_pDocProperties->m_strCompany);
	DDX_Text(pDX, IDC_DOCPROP_COMMENTS,		m_pDocProperties->m_strComments);
	//}}AFX_DATA_MAP
}

//-----------------------------------------------------------------------------
void CSummaryPage::OnOK()
{
	BOOL bEnable = m_pDocProperties->m_pWoormDoc->m_bAllowEditing;
	if (bEnable)
	{
		m_pDocProperties->m_strTitle = m_strTitle;
		m_pDocProperties->m_strSubject = m_strSubject;
	}
}
/////////////////////////////////////////////////////////////////////////////
// CDocPropertiesSheet property sheet
/////////////////////////////////////////////////////////////////////////////
//
IMPLEMENT_DYNAMIC(CDocPropertiesSheet, CLocalizablePropertySheet)
//-----------------------------------------------------------------------------
CDocPropertiesSheet::CDocPropertiesSheet(CDocProperties* pDocProperties)
:
	CLocalizablePropertySheet	 (cwsprintf(_TB("Properties of {0-%s}report"), (LPCTSTR)pDocProperties->m_pWoormDoc->GetTitle())),
	m_pDocProperties (pDocProperties)
{
	m_pGeneralPage	= new CGeneralPropPage(m_pDocProperties->m_pWoormDoc->GetPathName());
	m_pSummaryPage	= new CSummaryPage(m_pDocProperties);

	AddPage(m_pGeneralPage);
	AddPage(m_pSummaryPage);
}

//-----------------------------------------------------------------------------
CDocPropertiesSheet::~CDocPropertiesSheet()
{
	SAFE_DELETE(m_pGeneralPage);
	SAFE_DELETE(m_pSummaryPage);
}

//-----------------------------------------------------------------------------
void DoPropertiesSheet(CWoormDocMng* pWoormDoc)
{
	ASSERT(pWoormDoc);

	CDocPropertiesSheet pPropSheet(pWoormDoc->m_pDocProperties);
	if (pPropSheet.DoModal() == IDOK)
	{
		pWoormDoc->m_pDocProperties->SetModifyFlag();
	}
}



