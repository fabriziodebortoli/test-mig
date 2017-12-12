
#include "stdafx.h"
#include "afxpriv.h"

#include <TbGeneric\minmax.h>
#include <TbGeneric\GeneralFunctions.h>
#include <TbGeneric\TbStrings.h>
#include <TbGeneric\linefile.h>
#include <TbGeneric\LocalizableObjs.h>
#include <TbGeneric\globals.h>

#include <TbNameSolver\ThreadContext.h>

#include "lexdiag.h"

// resources
#include "lexdiag.hjson" //JSON AUTOMATIC UPDATE

//includere come ultimo include all'inizio del cpp
#include "begincpp.dex"

#ifdef _DEBUG
#undef THIS_FILE
static const char BASED_CODE THIS_FILE[] = __FILE__;
#endif

static const TCHAR szNotepad[] = _T("notepad.exe");
//============================================================================
//          Class CShowErrorsDlg definition
//============================================================================
class CShowErrorsDlg : public CLocalizableDialog
{
protected:
	LexDiagnostic*	m_lex;

protected:	
	CStatic		m_LexErrFileLabel;
	CBCGPEdit	m_LexErrFile;
	CBCGPEdit	m_LexErrMsg;
	CBCGPEdit	m_LexErrRow;
	CBCGPEdit	m_LexErrColumn;
	CBCGPListBox	m_LexErrFileList;
	CString		m_sFullDescription;

private:
	void FillErrFileListFromString	();
	void FillErrFileListFromFile	();

public:
	CShowErrorsDlg (LexDiagnostic*);

protected:
	virtual BOOL OnInitDialog	();
	
	//{{AFX_MSG(CShowErrorsDlg)
	afx_msg	void OnEditReport();
	afx_msg	void OnClipboard();
	//}}AFX_MSG

	DECLARE_MESSAGE_MAP()
	DECLARE_DYNAMIC(CShowErrorsDlg)
};

//============================================================================
//          Class LexDiagnostic implementation
//============================================================================
LexDiagnostic::LexDiagnostic(LPCTSTR pszString, const TToken* userTokenTable, long nStringLen, BOOL bAllowOpenWhenEmpty /*= FALSE*/)
	:
	Lexan (pszString, userTokenTable, nStringLen, bAllowOpenWhenEmpty)
{
	m_SkipError	= FALSE;
	m_nErrPos		= -1;
	m_nErrLine		= -1;

	m_pInputText = new CLineText(pszString, nStringLen);
}

//---------------------------------------------------------------------------
LexDiagnostic::LexDiagnostic (const TToken* userTokenTable)
:
	Lexan   		(userTokenTable),
	m_pInputText	(NULL)
{
	m_SkipError		= FALSE;
	m_nErrPos		= -1;
	m_nErrLine		= -1;
}

//---------------------------------------------------------------------------
LexDiagnostic::~LexDiagnostic ()
{
	ShowErrors();

	SAFE_DELETE (m_pInputText);
}


//---------------------------------------------------------------------------
BOOL LexDiagnostic::SetError (const CString& strErr, LPCTSTR pszString, int nPos, long nLine)
{   
	// if current id error is not set then set it!
	if (m_strError.IsEmpty())
	{
		m_SkipError		= FALSE;
		m_strError      = strErr + ' ' + pszString;
		m_nErrPos     	= nPos;
		m_nErrLine    	= nLine;
	}
	
	return FALSE;
}

//---------------------------------------------------------------------------
int LexDiagnostic::GetCurrentPos () const
{
	// standard base functionality
	return (m_nErrPos == -1)
		? Lexan::GetCurrentPos()
		: m_nErrPos;
}

//---------------------------------------------------------------------------
long LexDiagnostic::GetCurrentLine () const
{
	// standard base functionality
	return (m_nErrLine == -1)
		? Lexan::GetCurrentLine()
		: m_nErrLine;
}


//---------------------------------------------------------------------------
CString LexDiagnostic::BuildErrMsg (BOOL bClear /*=FALSE*/)
{
	if (m_strError.IsEmpty() && Bad())
	{
		CString sMsg;
		sMsg += _TB("To position shown from row and column, grammar expect the keyword:");
		sMsg += _T("\r\n") + GetTokenString(GetTokenExpected());

		CString sFound = GetCurrentStringToken();

		if (!sFound.IsEmpty())
			sMsg += _T("\r\n") + _TB("In the file, instead, it is added:") + sFound;

		return sMsg;
	}

	CString theError;
	CString msg;

	theError += m_strError;

	if (bClear) ClearError();
	return theError;
}


//---------------------------------------------------------------------------
void LexDiagnostic::ClearError ()
{
	m_SkipError	= TRUE;
	m_strError.Empty();
	m_nErrPos	= -1;
	m_nErrLine	= -1;
}

//---------------------------------------------------------------------------
CString LexDiagnostic::FormatErrors()
{
	CString sErrors;
	CFileException	exception;  // signal file exception
	CLineFile		ifile;      // file to be parsed
	TCHAR* 		pszBuffer;
	
	while ((m_pInputText->GetLine(pszBuffer)) >= 0)
	{
		sErrors += CString(pszBuffer) + _T("\r\n");
	}

	UINT nFlags = CFile::modeRead | CFile::shareDenyWrite | CFile::typeText;

	if (ifile.Open(m_strErrFileName, nFlags, &exception))
	{
		TRY // try to read from file
		{
			CString		strBuffer;
			while (ifile.ReadString(strBuffer.GetBuffer(1024),1023))
			{
				strBuffer.ReleaseBuffer();
				sErrors += strBuffer + _T("\r\n");
			}
			ifile.Close();
		}
		CATCH (CFileException, e)
		{
		}
		END_CATCH
	}

	CString sRow; sRow.Format( _T("%ld"),	GetCurrentLine());
	CString sCol; sCol.Format( _T("%ld"),	GetCurrentPos());
	
	CString sPos; sPos.Format(_T(" (row:%s, col:%s)\r\n"), sRow, sCol);
	sErrors += BuildErrMsg() + sPos;
	if (!m_strErrFileName.IsEmpty())
		sErrors += m_strErrFileName + _T("\r\n");

	return sErrors;
}


//---------------------------------------------------------------------------
void LexDiagnostic::ShowErrors (BOOL bClear /* = TRUE*/)
{
	if (m_SkipError) return;
	
	if (HardwareError())
	{                 
		CString msg = CString(GetException()) + CString(":\r\n\t") + m_strErrFileName;
		if (CTBWinThread::IsCurrentlyInUnattendedMode()) 
		{
			AfxGetDiagnostic()->Add(msg);
		}
		else
		{
			AfxMessageBox(msg, MB_OK | MB_ICONSTOP);
		}
		
	}
	
	if (!HardwareError() && (!m_strError.IsEmpty() || Bad()))
	{
		if (CTBWinThread::IsCurrentlyInUnattendedMode()) 
		{
			AfxGetDiagnostic()->Add(FormatErrors());
		}
		else
		{
			CShowErrorsDlg ErrBoxDlg (this);
			ErrBoxDlg.DoModal();
		}
	}
	
	if (bClear)
		ClearError();
}

//==============================================================================
//          Class CShowErrorsDlg implementation
//==============================================================================
BEGIN_MESSAGE_MAP(CShowErrorsDlg, CLocalizableDialog)
	//{{AFX_MSG_MAP(CShowErrorsDlg)
	ON_COMMAND (IDC_LEX_EDIT_REPORT,	OnEditReport)
	ON_COMMAND (IDC_LEX_CLIPBOARD,		OnClipboard)
	//}}AFX_MSG_MAP
END_MESSAGE_MAP()
IMPLEMENT_DYNAMIC(CShowErrorsDlg, CLocalizableDialog)
//------------------------------------------------------------------------------
CShowErrorsDlg::CShowErrorsDlg(LexDiagnostic* aLex)
	:
	CLocalizableDialog	(IDD_LEX_SHOW_ERRORS),
	m_lex	(aLex)
{
}
//------------------------------------------------------------------------------
void CShowErrorsDlg::OnClipboard ()
{
	AfxMessageBox(m_sFullDescription);
}
//------------------------------------------------------------------------------
void CShowErrorsDlg::OnEditReport ()
{
	if (m_lex->m_strErrFileName.IsEmpty())
		return;

	HINSTANCE hInst = ::ShellExecute
		(
			HWND_DESKTOP, 
			_T("open"), 
			szNotepad, 
			m_lex->m_strErrFileName, 
			NULL, 
			SW_SHOW 
		);
	if (hInst <= (HINSTANCE)32)
		AfxMessageBox (_TB("File not available for error correction!"));

	EndDialog(IDOK);
}                          

//------------------------------------------------------------------------------
void CShowErrorsDlg::FillErrFileListFromString()
{
	TCHAR* 		pszBuffer;
	int 		nLen = 0;
	int			nExtent = 0;
	int			nTabSize = 16;
	CClientDC	dc(&m_LexErrFileList);

	// read tokens by all input string and set eof condition.
	while ((nLen = m_lex->m_pInputText->GetLine(pszBuffer)) >= 0)
	{
		m_LexErrFileList.AddString(pszBuffer);
		m_sFullDescription += CString(pszBuffer) + _T("\r\n");
		nExtent = Max(nExtent, dc.GetTabbedTextExtent(pszBuffer, nLen, 1, &nTabSize).cx);
	}

	m_LexErrFileList.SetCurSel((int)m_lex->GetCurrentLine() - 1);
	m_LexErrFileList.SetTabStops(8);
	m_LexErrFileList.SetHorizontalExtent(nExtent);
}

//------------------------------------------------------------------------------
void CShowErrorsDlg::FillErrFileListFromFile()
{
	CFileException	exception;  // signal file exception
	CLineFile		ifile;      // file to be parsed


	UINT nFlags = CFile::modeRead | CFile::shareDenyWrite | CFile::typeText;

	if (!ifile.Open(m_lex->m_strErrFileName, nFlags, &exception))
	{
		m_LexErrFileList.AddString(PCause(&exception));
		return;
	}

	TRY // try to read from file
	{
		CString		strBuffer;
		int			nExtent = 0;
		int			nTabSize = 16;
		CClientDC	dc(&m_LexErrFileList);

		while (ifile.ReadString(strBuffer.GetBuffer(1024),1023))
		{
			strBuffer.ReleaseBuffer();
			m_sFullDescription += strBuffer + _T("\r\n");
			int nLen = strBuffer.GetLength();
			if (nLen)
			{
				if (strBuffer[nLen - 1] == '\n') nLen--;
				m_LexErrFileList.AddString(strBuffer.Left(nLen));
				nExtent = Max(nExtent, dc.GetTabbedTextExtent(strBuffer, strBuffer.GetLength(), 1, &nTabSize).cx);
			}
		}

		ifile.Close();
		m_LexErrFileList.SetCurSel((int)m_lex->GetCurrentLine() - 1);
		m_LexErrFileList.SetTabStops(8);
		m_LexErrFileList.SetHorizontalExtent(nExtent);
	}
	CATCH (CFileException, e)
	{
		m_LexErrFileList.AddString(PCause(e));
		return;
	}
	END_CATCH
}


//------------------------------------------------------------------------------
BOOL CShowErrorsDlg::OnInitDialog()
{                
	__super::OnInitDialog();
	::CenterWindow(this, NULL);

	m_LexErrFileLabel.	SubclassDlgItem(IDC_LEX_ERR_FILE_LABEL,	this);
	m_LexErrFile.		SubclassDlgItem(IDC_LEX_ERR_FILE,		this);
	m_LexErrMsg.		SubclassDlgItem(IDC_LEX_ERR_MSG, 		this);
	m_LexErrRow.		SubclassDlgItem(IDC_LEX_ERR_ROW, 		this);
	m_LexErrColumn.		SubclassDlgItem(IDC_LEX_ERR_COLUMN,		this);
	m_LexErrFileList.	SubclassDlgItem(IDC_LEX_ERR_FILE_LIST,	this);

	if (m_lex->m_strErrFileName.IsEmpty())
	{
		m_LexErrFileLabel.	ShowWindow(SW_HIDE);
		m_LexErrFile.		ShowWindow(SW_HIDE);

		GetDlgItem(IDC_LEX_EDIT_REPORT)->ShowWindow(SW_HIDE);
		FillErrFileListFromString ();
	}
	else
	{
		m_LexErrFile.SetWindowText(m_lex->m_strErrFileName);
		FillErrFileListFromFile	();
	}
	
	CString sRow; sRow.Format( _T("%ld"),	m_lex->GetCurrentLine());
	CString sCol; sCol.Format( _T("%ld"),	m_lex->GetCurrentPos());
	
	CString sPos; sPos.Format(_T(" (row:%s, col:%s)\r\n"), sRow, sCol);
	m_sFullDescription += m_lex->BuildErrMsg() + sPos;
	if (!m_lex->m_strErrFileName.IsEmpty())
		m_sFullDescription += m_lex->m_strErrFileName + _T("\r\n");

	m_LexErrMsg.	SetWindowText(m_lex->BuildErrMsg());
	m_LexErrRow.	SetWindowText(sRow);
	m_LexErrColumn.	SetWindowText(sCol);

	return FALSE;  // return TRUE  unless you set the focus to a control
}
