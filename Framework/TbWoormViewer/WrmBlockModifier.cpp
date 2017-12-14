
#include "stdafx.h"

// library declaration
#include <TbNamesolver\IFileSystemManager.h>

#include <TbClientCore\ClientObjects.h>
#include <TbWebServicesWrappers\LoginManagerInterface.h>

#include <TbGeneric\GeneralFunctions.h>

#include <TbGenlib\messages.h>
#include <TbGenlib\parsctrl.h>
#include <TbGenlib\dirtreeCtrl.h>			
#include <TbGeneric\tools.h>			
#include <TbGenlib\baseapp.h>			
#include <TbGenlib\const.h>	
#include <TbGenlib\baseapp.h>
#include <TbGenlib\oslbaseinterface.h>
#include <TbGenlib\generic.hjson> //JSON AUTOMATIC UPDATE

#include <TbWoormEngine\ExportSymbols.h>

#include <TbParser\EnumsParser.h>

#include "DocProperties.h"
#include "WoormDoc.h"
#include "WoormVw.h"
#include "WoormIni.h"

#include "WrmBlockModifier.hjson" //JSON AUTOMATIC UPDATE
#include "WrmBlockModifier.h"

//includere come ultimo include all'inizio del cpp
#include "begincpp.dex"

#ifdef _DEBUG
#undef THIS_FILE
static const char BASED_CODE THIS_FILE[] = __FILE__;
#endif
/////////////////////////////////////////////////////////////////////////////

///////////////////////////////////////////////////////////////////////////////
//							CWrmBlockModifierDlg
///////////////////////////////////////////////////////////////////////////////
//
//-----------------------------------------------------------------------------
CWrmBlockModifierDlg::CWrmBlockModifierDlg()
	:
	CBatchDialog	(IDD_WRMBLOCKMODIFIER)
{
}


///////////////////////////////////////////////////////////////////////////////
BEGIN_MESSAGE_MAP(CWrmBlockModifierDlg, CBatchDialog)
	//{{AFX_MSG_MAP(CWrmBlockModifierDlg)
	ON_COMMAND			(IDC_WRMBLOCKMODIFIER_BROWSE,	OnBrowse)
	ON_COMMAND			(IDC_WRMBLOCKMODIFIER_GENSYMTABLE,	OnGenSymTable)
	ON_COMMAND			(IDC_WRMBLOCKMODIFIER_SAVE_ENUMS,	OnSaveEnums)
	//}}AFX_MSG_MAP
END_MESSAGE_MAP()


//-----------------------------------------------------------------------------
BOOL CWrmBlockModifierDlg::OnInitDialog()
{
	CBatchDialog::OnInitDialog();

	// Subclass ed inizializzazione del control
	VERIFY (m_BtnBrowse.		SubclassDlgItem (IDC_WRMBLOCKMODIFIER_BROWSE,	this));
	VERIFY (m_NomeFile.			SubclassEdit	(IDC_WRM_FILE_NAME,		this));
	VERIFY (m_Status.			SubclassDlgItem	(IDC_WRMBLOCKMODIFIER_STATUS,	this));

	VERIFY (m_BtnOslEncrypt.	SubclassDlgItem (IDC_WRMBLOCKMODIFIER_ENCRYPT,	this));
	VERIFY (m_BtnOslDecrypt.	SubclassDlgItem (IDC_WRMBLOCKMODIFIER_DECRYPT,	this));
	VERIFY (m_BtnOslNoChangeCryptState.	SubclassDlgItem (IDC_WRMBLOCKMODIFIER_NOCHANGECRYPTSTATE,	this));

	m_BtnOslNoChangeCryptState.SetCheck(1);
	if (AfxGetLoginInfos()->m_bAdmin)
	{
		m_BtnOslEncrypt.EnableWindow(TRUE);
		m_BtnOslDecrypt.EnableWindow(TRUE);
		m_BtnOslNoChangeCryptState.EnableWindow(TRUE);
	}

//	AfxGetTbCmdManager()->LoadNeededLibraries(CTBNamespace(), NULL, LoadBaseOnly);
//  AfxGetDiagnostic()();

	return TRUE;
}

//----------------------------------------------------------------------------
void CWrmBlockModifierDlg::OnBrowse()
{
	CString strDir = m_NomeFile.GetValue();
	CChooseDirDlg dlg(strDir);
	if (dlg.DoModal() == IDOK)
		m_NomeFile.SetValue(dlg.GetSelectedDir());
}

//-----------------------------------------------------------------------------
void CWrmBlockModifierDlg::EnableIDOK (BOOL bEnable )
{
	((CButton*) GetDlgItem (IDOK))->EnableWindow (bEnable);
}

//-----------------------------------------------------------------------------
void CWrmBlockModifierDlg::OnBatchExecute()
{                                                             
	CString strFileName = m_NomeFile.GetValue();
	if (!IsPathName(strFileName))
	{                    
		AfxMessageBox (cwsprintf(_TB("{0-%s} path not found "), (LPCTSTR)strFileName), MB_OK | MB_ICONSTOP);
		return;
    }
	
	//disabilito il pulsante di sfoglia
	((CButton*) GetDlgItem (IDC_WRMBLOCKMODIFIER_BROWSE))->EnableWindow (FALSE);
	AfxGetApp()->BeginWaitCursor();	

	m_Status.ResetContent();
	DoCheck(strFileName, _T(".") + FileExtension::WRM_EXT());

	AfxGetApp()->EndWaitCursor();	
	//riabilito il pulsante di sfoglia
	((CButton*) GetDlgItem (IDC_WRMBLOCKMODIFIER_BROWSE))->EnableWindow ();
}

  
//-----------------------------------------------------------------------------
void CWrmBlockModifierDlg::DoUpdate(CString strInitialPath, CString strFindExt)
{
	CStringArray arFiles;
	CStringArray arFolders;
	AfxGetFileSystemManager()->GetPathContent (strInitialPath, TRUE, &arFolders, TRUE, _T("*") + strFindExt, &arFiles);

	int i = 0;
	for (i = 0; i <= arFolders.GetUpperBound (); i++)
	{
		if (IsDlgButtonChecked(IDC_WRMBLOCKMODIFIER_SUBDIR) == BST_CHECKED)
			DoUpdate(strInitialPath + SLASH_CHAR + arFolders.GetAt(i), strFindExt);
	}

	for (i=0; i <= arFiles.GetUpperBound (); i++)
	{      
		CString strFilePath = arFiles.GetAt(i);

		// se includo le subdirectory entro in ricorsione
		// salto i file che non sono dei report
		CString strExt = GetExtension(strFilePath);
		if (strExt.CompareNoCase(strFindExt))
			continue;

		CTBNamespace ns = AfxGetPathFinder()->GetNamespaceFromPath(strFilePath);
		if (!AfxIsActivated(ns.GetApplicationName(), ns.GetModuleName()))
			continue;

		// istanzia il report per farne fare il Parse 
		CWoormInfo aWoormInfo;
		aWoormInfo.AddReport(strFilePath);
		aWoormInfo.m_bOwnedByReport = FALSE;
		aWoormInfo.m_bHideFrame = FALSE;

		// apro il report senza farne la run
		CWoormDocMng* pDoc = (CWoormDocMng*)AfxGetTbCmdManager()->RunWoormReport(&aWoormInfo, NULL, NULL, FALSE, FALSE);
		if (pDoc)
		{
			// costringe il parsing della parte relativa al motore dati che di default
			// non viene fatta in apertura del documento
			if (pDoc->ForceEngineParse())
			{
				pDoc->SetModifiedFlag();
				//------------------------------

				//pDoc->m_nReportRelease = pDoc->m_nRelease;
				strFilePath.Replace(L".wrm", L"_chk.wrm");
				if (!pDoc->OnSaveDocument(strFilePath))
				{	
					m_Status.AddString(_T("ERROR on save ") + strFilePath);
					TRACE(_T("ERROR on save ") + strFilePath + _T("\n"));
				}
				else
				{
					CWoormInfo aWoormInfo;
					aWoormInfo.AddReport(strFilePath);
					aWoormInfo.m_bOwnedByReport = FALSE;
					
					CWoormDocMng* pDoc = (CWoormDocMng*)AfxGetTbCmdManager()->RunWoormReport(&aWoormInfo, NULL, NULL, FALSE, FALSE);
					if (pDoc)
					{
						// costringe il parsing della parte relativa al motore dati che di default
						// non viene fatta in apertura del documento
						if (pDoc->ForceEngineParse())
						{

						}
						else 
						{	
							m_Status.AddString(_T("NOT RE-EDITED ") + strFilePath);
							TRACE(_T("NOT RE-EDITED ") + strFilePath + _T("\n"));
						}
						pDoc->OnCloseDocument(); 
					}
					else
					{
						TRACE(_T("NOT RE-OPEN  ") + strFilePath + _T("\n"));
						m_Status.AddString(_T("NOT RE-OPEN ") + strFilePath);
					}
				}
				//	m_Status.AddString(strFilePath);
			}
			else 
			{	
				m_Status.AddString(_T("NOT EDITED ") + strFilePath);
				TRACE(_T("NOT EDITED ") + strFilePath+ _T("\n"));
			}
			pDoc->OnCloseDocument(); 
			//m_Status.AddString(_T("OK ") + strFilePath);
		}
		else 
		{
			TRACE(_T("NOT OPEN  ") + strFilePath + _T("\n"));
			m_Status.AddString(_T("NOT OPEN ") + strFilePath);
		}
	}
}

//-----------------------------------------------------------------------------
void CWrmBlockModifierDlg::DoCheck(CString strInitialPath, CString strFindExt)
{
	CStringArray arFiles;
	CStringArray arFolders;
	AfxGetFileSystemManager()->GetPathContent(strInitialPath, TRUE, &arFolders, TRUE, _T("*") + strFindExt, &arFiles);

	int i = 0;
	for (i = 0; i <= arFolders.GetUpperBound(); i++)
	{
		if (IsDlgButtonChecked(IDC_WRMBLOCKMODIFIER_SUBDIR) == BST_CHECKED)
			DoCheck(strInitialPath + SLASH_CHAR + arFolders.GetAt(i), strFindExt);
	}

	for (i = 0; i <= arFiles.GetUpperBound(); i++)
	{
		CString strFilePath = arFiles.GetAt(i);

		// se includo le subdirectory entro in ricorsione
		// salto i file che non sono dei report
		CString strExt = GetExtension(strFilePath);
		if (strExt.CompareNoCase(strFindExt))
			continue;

		CTBNamespace ns = AfxGetPathFinder()->GetNamespaceFromPath(strFilePath);
		if (!AfxIsActivated(ns.GetApplicationName(), ns.GetModuleName()))
			continue;

		// istanzia il report per farne fare il Parse 
		CWoormInfo aWoormInfo;
		aWoormInfo.AddReport(strFilePath);
		aWoormInfo.m_bOwnedByReport = FALSE;
		aWoormInfo.m_bHideFrame = FALSE;

		// apro il report senza farne la run
		CWoormDocMng* pDoc = (CWoormDocMng*)AfxGetTbCmdManager()->RunWoormReport(&aWoormInfo, NULL, NULL, FALSE, FALSE);
		if (pDoc)
		{
			// costringe il parsing della parte relativa al motore dati che di default
			// non viene fatta in apertura del documento
			if (pDoc->ForceEngineParse())
			{
				//	m_Status.AddString(strFilePath);
			}
			else
			{
				CString sErrors; CStringArray_Concat(aWoormInfo.m_arErrors, sErrors);
				m_Status.AddString(_T("NOT EDITED ") + strFilePath + L" " + sErrors);
				TRACE(_T("NOT EDITED ") + strFilePath + _T("\n"));

			}

			pDoc->OnCloseDocument();
			//m_Status.AddString(_T("OK ") + strFilePath);
		}
		else
		{
			TRACE(_T("NOT OPEN  ") + strFilePath + _T("\n"));
			m_Status.AddString(_T("NOT OPEN ") + strFilePath);
		}
	}
}

//-----------------------------------------------------------------------------
void CWrmBlockModifierDlg::OnGenSymTable()
{
	AfxGetApp()->BeginWaitCursor();	
	AfxGetExportSymbols()->Activate();

	int startMod = 50;//0, 24, 50
	int endMod = 59;//23, 49, 58

	// itero su tutte le AddOnApplication per scoprire si è registrato all'evento
	for (int i = 0; i <= AfxGetAddOnAppsTable()->GetUpperBound(); i++)
	{
		AddOnApplication* pAddOnApp = AfxGetAddOnAppsTable()->GetAt(i);

		if (
			pAddOnApp->m_strAddOnAppName.CompareNoCase(MAGONET_APP)
			&&
			pAddOnApp->m_strAddOnAppName.CompareNoCase(OFM_APP)
			&&
			pAddOnApp->m_strAddOnAppName.CompareNoCase(ACM_APP)
			&&
			pAddOnApp->m_strAddOnAppName.CompareNoCase(FSM_APP)
			&&
			pAddOnApp->m_strAddOnAppName.CompareNoCase(PAINET_APP)
			) 
			continue;

		AfxGetExportSymbols()->Activate();
		for (int j = 0; j <= pAddOnApp->m_pAddOnModules->GetUpperBound(); j++) 
		{
			if (j < startMod) 
				continue;
			if (j > endMod) 
				break;

			AddOnModule* pAddOnMod = pAddOnApp->m_pAddOnModules->GetAt(j);

			CString sM; sM.Format(_T("c:\\temp\\Woorm\\%d-%s.csv"), j, pAddOnMod->GetModuleName());
			TRACE(_T("processing %s module\n"), sM);

			CString sPath = AfxGetPathFinder()->GetModuleReportPath(pAddOnMod->m_Namespace, CPathFinder::STANDARD);

			DoGenSymTable(sPath, _T(".wrm"));
			//goto l_end;
		}
		CString sMod; sMod.Format(_T("c:\\temp\\Woorm\\%d-%d.csv"), startMod, endMod);
		AfxGetExportSymbols()->SaveCSV2(sMod);
		AfxGetExportSymbols()->DeActivate();
	}
//l_end:
}

//-----------------------------------------------------------------------------
void CWrmBlockModifierDlg::OnGenSymTable2()
{
	CString sPath(_T("c:\\microareaserver\\development\\running\\standard\\applications\\erp\\"));
	AfxGetExportSymbols()->Activate();

	DoGenReportSymTable(sPath + _T("sales\\report\\buffetti8903fform.wrm"));
	DoGenReportSymTable(sPath + _T("salespeople\\report\\enasarcofrombalances-det.wrm"));
	DoGenReportSymTable(sPath + _T("SalesPeople\\Report\\ENASARCOPostalPaymentSlip.wrm"));

	AfxGetExportSymbols()->SaveCSV2(_T("c:\\temp\\woorm\\trereport.csv"));
	AfxGetExportSymbols()->DeActivate();;
}

//----------------------------------------------------------------------------
void CWrmBlockModifierDlg::DoGenSymTable(CString strInitialPath, CString strFindExt)
{
	CFileFind finder;   
	BOOL bWorking = finder.FindFile(strInitialPath + _T("\\*.*"));   
	while (bWorking)
	{      
		bWorking = finder.FindNextFile();
		CString strFilePath = finder.GetFilePath();

		// evito i file non interessanti compreso "." e ".." per evitare ricorsione infinita
		if (finder.IsHidden()  || finder.IsSystem() || finder.IsDots())
		{
			continue;
		}

		// se includo le subdirectory entro in ricorsione
		if (finder.IsDirectory() && IsDlgButtonChecked(IDC_WRMBLOCKMODIFIER_SUBDIR) == BST_CHECKED)
		{
			//DoGenSymTable(strFilePath, strFindExt);
			continue;
		}

		// salto i file che non sono dei report
		CString strExt = GetExtension(strFilePath);
		if (strExt.CompareNoCase(strFindExt))
			continue;
		
		DoGenReportSymTable(strFilePath);
	}
}

//----------------------------------------------------------------------------
void CWrmBlockModifierDlg::DoGenReportSymTable(CString strFilePath)
{
	TRACE(_T("Processing file %s\n"), strFilePath);

	// istanzia il report per farne fare il Parse 
	CWoormInfo aWoormInfo;
	aWoormInfo.AddReport(strFilePath);
	aWoormInfo.m_bOwnedByReport = FALSE;

	// apro il report senza farne la run
	CWoormDocMng* pDoc = (CWoormDocMng*)AfxGetTbCmdManager()->RunWoormReport(&aWoormInfo, NULL, NULL, FALSE, FALSE);
	if (pDoc)
	{
		if (!pDoc->ForceEngineParse())
		{
			m_Status.AddString(_T("SKIP for ERROR on editing:") + strFilePath);
			TRACE(_T("SKIP for ERROR on editing:%s\n"), strFilePath);
		}
		else
		{
			m_Status.AddString(_T("Ready for editing:") + strFilePath);
			TRACE(_T("Ready for editing: %s\n"), strFilePath);

			if (!pDoc->PrepareRunEngine(strFilePath))
			{
				m_Status.AddString(_T("SKIP for ERROR on pre-run:") + strFilePath);
				TRACE(_T("SKIP for ERROR on pre-run: %s\n"), strFilePath);
			}
			else
			{
				m_Status.AddString(_T("Ready to run:") + strFilePath);
				TRACE(_T("Ready to run: %s\n"), strFilePath);
			}
		}
		
		pDoc->OnCloseDocument(); 
	}
	else 
	{
		m_Status.AddString(_TB("NOT OPEN:") + strFilePath);
		TRACE(_T("NOT OPEN: %s\n"), strFilePath);
	}
}

//=============================================================================
void CWrmBlockModifierDlg::OnSaveEnums()
{
	//CPathFinder* pPathFinder = AfxGetPathFinder();
	//CStringArray aErrors;
	//XmlEnumsUnparser enumsUnparser;

	//AfxGetApp()->BeginWaitCursor();

	////CStatusBarMsg	msgBar(TRUE, TRUE); 
	////msgBar.Show(_TB("Enums saving in progress...."));

	//enumsUnparser.UnparseLookUpTable(aErrors, _T("c:\\MicroareaServer\\Development\\running\\standard\\Applications\\Erp\\enums.xml"));
	//// itero su tutte le AddOnApplication
	//for (int i = 0; i <= AfxGetAddOnAppsTable()->GetUpperBound(); i++)
	//{
	//	AddOnApplication* pAddOnApp = AfxGetAddOnAppsTable()->GetAt(i);
	//	for (int j = 0; j <= pAddOnApp->m_pAddOnModules->GetUpperBound(); j++) 
	//	{
	//		AddOnModule* pAddOnMod = pAddOnApp->m_pAddOnModules->GetAt(j);
	//		enumsUnparser.UnparseModuleTags(pAddOnMod->m_Namespace, aErrors, pPathFinder->GetEnumsFullName(pAddOnMod->m_Namespace) + _T("_en.xml"));
	//	}
	//}

}

//-----------------------------------------------------------------------------


/* azioni per la criptazione
			// se non c'e` il guid forza il modificato per aggiungere il GUID durante l'unparse
			if 
				(
					(m_BtnOslEncrypt.GetCheck() == 1 && ! pDoc->m_bIsCrypted)
				||
					(m_BtnOslDecrypt.GetCheck() == 1 && pDoc->m_bIsCrypted)
				)
			{
				....
					if ( m_BtnOslEncrypt.GetCheck() == 1 && ! pDoc->m_bIsCrypted)
						pDoc->m_bSaveCrypted = TRUE;
					if ( m_BtnOslDecrypt.GetCheck() == 1 && pDoc->m_bIsCrypted)
						pDoc->m_bSaveCrypted = FALSE;
*/
 
