
#include "stdafx.h" 
#include <TbXmlCore\XMLDocObj.h>
#include <TbClientCore\ClientObjects.h>
#include <TbGeneric\DataObj.h>
#include <TbGeneric\FontsTable.h>
#include <TbGeneric\FormatsTable.h>
#include <TbGeneric\EnumsTable.h>
#include <TbGeneric\GeneralFunctions.h>
#include <TbGenlib\Const.h>
#include <TbGenlib\Expparse.h>
#include <TbGenlib\TBCommandInterface.h>
#include <TbNameSolver\PathFinder.h>
#include <TbNameSolver\FileSystemFunctions.h>
#include <TbOledb\SqlRec.h>
#include <TbGes\XMLGesInfo.h>
#include <TbWoormViewer\WoormDoc.h>


#include "AuditingManager.h"
#include "AuditTables.h"
#include "AuditReportGenerator.h"

static const TCHAR szAuditReportTemplate[]	=	_T("AuditReport.template");
static const TCHAR szLocalizeReportTitles[]=	_T("LocalizeReportTitles.xml");

//MARKS del template
static const TCHAR szTableName[]	=	_T("@AUDIT_TABLENAME");
static const TCHAR szCOUNT []		=	_T("@COUNT");
static const TCHAR szCOLUMNS[]	=	_T("@COLUMNS");
static const TCHAR szVARIABLES[]	=	_T("@VARIABLES");
static const TCHAR szTABLEFIELDS[]=	_T("@TABLEFIELDS");
static const TCHAR szWHERECLAUSE[]=	_T("@WHERECLAUSE");
static const TCHAR szPKCOLUMNS[]=	_T("@PKCOLUMNS");

//sono i tag utilizzati per identificare le stringhe da localizzare
// vengono considerate le stringhe presenti in LocalizeReportTitles
static const TCHAR szTABLETITLE[]	=	_T("@TABLETITLE");
static const TCHAR szTODATE []	=	_T("@TODATE");
static const TCHAR szFROMDATE []	=	_T("@FROMDATE");
static const TCHAR szCHANGEKEY[]	=	_T("@CHANGEKEY");
static const TCHAR szDELETED[]	=	_T("@DELETED");
static const TCHAR szUPDATED[]	=	_T("@UPDATED");
static const TCHAR szINSERTED[]	=	_T("@INSERTED");
static const TCHAR szUSER[]		=	_T("@USER");
static const TCHAR szOPERATIONDATA []	=	_T("@OPERATIONDATA");
static const TCHAR szOPERATION []	=	_T("@OPERATION");
static const TCHAR szDOCUMENT []	=	_T("@DOCUMENT");
static const TCHAR szAPPLICATION[]=	_T("@APPLICATION");
static const TCHAR szMODULE[]		=	_T("@MODULE");
static const TCHAR szINSERT[]		=	_T("@INSERT");
static const TCHAR szUPDATE[]		=	_T("@UPDATE");
static const TCHAR szDELETE[]		=	_T("@DELETE");
static const TCHAR szFILTERTITLE []	=	_T("@FILTERTITLE");
static const TCHAR szDATERANGE []		=	_T("@DATERANGE");
static const TCHAR szALL[]			=	_T("@ALL");
static const TCHAR szROWS[]			=	_T("@ROWS");

#define LOCALIZE_TAG(a) 	if (strBuffer.Find(a) >= 0)	strBuffer.Replace(a, GetLocalizedTitle(a));

const int FIXCOLUMNS = 8;
//primo alias disponibile
const int NEXTALIAS = 27;

//////////////////////////////////////////////////////////////////////////////
//             					ReportGenerator
//////////////////////////////////////////////////////////////////////////////
//


//-----------------------------------------------------------------------------
ReportGenerator::ReportGenerator(CTBNamespace* pNamespace, AuditingManager* pAuditMng, CXMLFixedKeyArray* pFixedKey)
:
	m_pNamespace	(pNamespace),
	m_pAuditMng		(pAuditMng),
	m_pFixedKey		(pFixedKey),
	m_pXMLTitleDoc	(NULL)
{
}

//-----------------------------------------------------------------------------
int ReportGenerator::GetColumnWidth(const SqlColumnInfo* pItem)
{
	BOOL bString = pItem->m_DataObjType == DATA_STR_TYPE;

	int nLen = bString
		? pItem->m_lLength
		: AfxGetFormatStyleTable()->GetOutputCharLen(pItem->m_DataObjType, m_pNamespace);
	
	if (nLen < 0)
		return 0;

	// poichè il pageInfo è solo uno short, limito un pochino le colonne
	// molto grosse, così da fare il display di più colonne possibili.
	if (nLen > 40)	nLen = 40;

	CDC* pDC = AfxGetMainWnd()->GetDC();

	int nFontIdx			= AfxGetFontStyleTable()->GetFontIdx(bString  || pItem->m_DataObjType == DATA_DATE_TYPE  ? FNT_CELL_STRING : FNT_CELL_NUM);
	const FontStyle* pFont	= AfxGetFontStyleTable()->GetFontStyle(nFontIdx, m_pNamespace);
	int nWidth				= pFont->GetStringWidth (pDC, nLen).cx;

	int nFontTitleIdx		= AfxGetFontStyleTable()->GetFontIdx(FNT_COLUMN_TITLE);
	const FontStyle* pTFont	= AfxGetFontStyleTable()->GetFontStyle(nFontTitleIdx, m_pNamespace);
	int nWidthTitle			= pTFont->GetStringWidth(pDC, pItem->m_strColumnName).cx;

	AfxGetMainWnd()->ReleaseDC(pDC);

	return max(nWidthTitle, nWidth) + 8;
}


//-----------------------------------------------------------------------------
CString ReportGenerator::GetTypeWRM(const DataType& aDataType)
{
	CString strTypeWRM = FromDataTypeToTokenString(aDataType);

	if	(aDataType == DATA_ENUM_TYPE)
	{
		strTypeWRM += _T("[\"");

		if (aDataType == DATA_ENUM_TYPE)
			strTypeWRM += AfxGetEnumsTable()->GetEnumTagName(aDataType.m_wTag);

		strTypeWRM += _T("\"]");
	}
	return strTypeWRM;
}

//"Campo2" Alias 2 Width 30 FormatStyle "Bool" ;
//-----------------------------------------------------------------------------
void ReportGenerator::InsertColumns(CLineFile& fOutputFile)
{
	CString strBuffer;
	int nAliasCount = NEXTALIAS; 
	
	for (int i = m_pAuditMng->GetAuditRec()->GetStartVarFieldsPos(); i <= m_pAuditMng->GetAuditRec()->GetUpperBound(); i++)
	{
		const SqlColumnInfo* pItem = m_pAuditMng->GetAuditRec()->GetColumnInfo(i);
		if (!pItem) 
			continue;

		if (pItem->m_bSpecial) 
		{
			m_strPkColumns = m_strPkColumns + cwsprintf
				(
					_T("\t%s %s Alias 1,%d \n"),
					(LPCTSTR) GetTypeWRM(pItem->m_DataObjType),
					(LPCTSTR) pItem->GetColumnTitle(),
					nAliasCount
				);
		}

		strBuffer = cwsprintf
		(
			_T("				\"%s\" Alias %d Width %d FormatStyle \"%s\";"),
			(LPCTSTR) AfxLoadDatabaseString(pItem->GetColumnTitle(), m_pAuditMng->GetAuditRec()->GetTracedTableName()),
			nAliasCount++, 
			GetColumnWidth(pItem),
			(LPCTSTR) FromDataTypeToFormatName(pItem->m_DataObjType) 
		);

		fOutputFile.WriteString(strBuffer + _T("\n"));
	}
}


//Bool      Campo2    [1] Alias 2  Column ;
//------------------------------------------------------------------------------
void ReportGenerator::InsertVariables(CLineFile& fOutputFile)
{
	CString strBuffer;
	int nAlias = NEXTALIAS;

	for (int i = m_pAuditMng->GetAuditRec()->GetStartVarFieldsPos(); i <= m_pAuditMng->GetAuditRec()->GetUpperBound(); i++)
	{
		const SqlColumnInfo* pItem = m_pAuditMng->GetAuditRec()->GetColumnInfo(i);
		if (!pItem) 
			continue;

		
		CString strVarName;
		
		strVarName = _T("v_") + pItem->m_strColumnName;
		strBuffer = cwsprintf
		(
			_T("			 %s    %s    [%d] Alias %d column;"),
			(LPCTSTR) GetTypeWRM(pItem->m_DataObjType),
			(LPCTSTR) strVarName,
			GetColumnWidth(pItem),
			nAlias++ 
		);
		fOutputFile.WriteString(strBuffer + _T("\n"));	
	}
}


//-----------------------------------------------------------------------------
void ReportGenerator::InsertTableFields(CLineFile& fOutputFile)
{
	CString strBuffer;
	BOOL bFirst = TRUE;
	for (int i = m_pAuditMng->GetAuditRec()->GetStartVarFieldsPos(); i <=  m_pAuditMng->GetAuditRec()->GetUpperBound(); i++)
	{
		const SqlColumnInfo* pItem = m_pAuditMng->GetAuditRec()->GetColumnInfo(i);
		if (!pItem) 
			continue;

		if (!bFirst)
			fOutputFile.WriteString(_T(",\n"));

		bFirst = FALSE;

		strBuffer = cwsprintf
		(
			 _T("				%s        Into %s"),
			(LPCTSTR) pItem->m_strColumnName,
			(LPCTSTR) (_T("v_") + pItem->m_strColumnName)
		);		
		fOutputFile.WriteString(strBuffer);
	}

	fOutputFile.WriteString(_T("\n"));
}

//-----------------------------------------------------------------------------
void ReportGenerator::ModifyWhereClause(CLineFile& fOutputFile, CString& strBuffer)
{
	CString strWhere;

	//inserisco l'eventuale filtraggio dovuto ai campi fissi
	// leggo il nome del campo e il valore dal file dbts.xml associato al documento nel dbtmaster
	// nel nodo FixedKeys
	if (m_pFixedKey)
	{	
		for (int nFix = 0; nFix <= m_pFixedKey->GetUpperBound(); nFix++)
		{ 
			if (m_pFixedKey->GetAt(nFix) && !m_pFixedKey->GetAt(nFix)->GetName().IsEmpty())
			{ 
				DataObj* pDataObj = m_pAuditMng->GetAuditRec()->GetDataObjFromColumnName(m_pFixedKey->GetAt(nFix)->GetName());
				if (pDataObj)
				{
					DataObj* pClone = pDataObj->DataObjClone();
					pClone->AssignFromXMLString(m_pFixedKey->GetAt(nFix)->GetValue());
					strWhere += cwsprintf(_T(" And %s == %s"), m_pFixedKey->GetAt(nFix)->GetName(), ExpUnparse::UnparseData(*pClone));
					delete pClone;
				}
			}
		}
	}
	
	strBuffer.Replace(szWHERECLAUSE, strWhere.IsEmpty() ? _T("") : cwsprintf(_T("\n%s"),strWhere));
	fOutputFile.WriteString(strBuffer);

	fOutputFile.WriteString(_T("\n"));
}


//-----------------------------------------------------------------------------
CString ReportGenerator::GetLocalizedTitle(LPCTSTR szTag)
{
	CString strLocalized;
	CString strTag(szTag);
	strTag.Replace(_T("@"), _T(""));
	CXMLNode* pNode = m_pXMLTitleDoc->GetRootChildByName(strTag);
	if (pNode)
		pNode->GetText(strLocalized);
	
	return strLocalized;
}

//-----------------------------------------------------------------------------
void ReportGenerator::Localize(CString& strBuffer)
{
	LOCALIZE_TAG(szTABLETITLE);
	LOCALIZE_TAG(szTODATE);
	LOCALIZE_TAG(szFROMDATE);	
	LOCALIZE_TAG(szCHANGEKEY);	
	LOCALIZE_TAG(szDELETED);	
	LOCALIZE_TAG(szUPDATED);	
	LOCALIZE_TAG(szINSERTED);	
	LOCALIZE_TAG(szUSER);		
	LOCALIZE_TAG(szOPERATIONDATA);
	LOCALIZE_TAG(szOPERATION);	
	LOCALIZE_TAG(szDOCUMENT);	
	LOCALIZE_TAG(szAPPLICATION);
	LOCALIZE_TAG(szMODULE);	
	LOCALIZE_TAG(szINSERT);		
	LOCALIZE_TAG(szUPDATE);		
	LOCALIZE_TAG(szDELETE);		
	LOCALIZE_TAG(szFILTERTITLE);
	LOCALIZE_TAG(szDATERANGE);	
	LOCALIZE_TAG(szALL);		
	LOCALIZE_TAG(szROWS);
}

//-----------------------------------------------------------------------------
void ReportGenerator::WriteReport(CLineFile& fOutputFile, CString& strBuffer)
{
	//prima traduco le variabili contenente i titoli da visualizzare
	Localize(strBuffer);

	if (strBuffer.Find(szTableName) >= 0)
		strBuffer.Replace(szTableName, m_pAuditMng->GetAuditRec()->GetTableName());

	//  sono nella parte grafica del report
	if (strBuffer.Find(szCOUNT) >= 0)
	{
		strBuffer.Replace(szCOUNT, cwsprintf(_T("%d"), FIXCOLUMNS + (m_pAuditMng->GetAuditRec()->GetSize() - m_pAuditMng->GetAuditRec()->GetStartVarFieldsPos())));
		fOutputFile.WriteString(strBuffer);
		return;
	}

	//  sono nella parte grafica del report
	//  devo inserire le nuove colonne da visualizzare
	if (strBuffer.Find(szCOLUMNS) >= 0)
	{
		InsertColumns(fOutputFile);	
		return;
	}
	//  Hyperlink al documento identificato dal namespace
	if (strBuffer.Find(szPKCOLUMNS) >= 0)
	{
		fOutputFile.WriteString(m_strPkColumns);
		return;
	}

	// sono nella parte di estrazione del report
	// devo inserire le variabili
	if (strBuffer.Find(szVARIABLES) >= 0)
	{
		InsertVariables(fOutputFile);	
		return;
	}

	if (strBuffer.Find(szTABLEFIELDS) >= 0)
	{
		InsertTableFields(fOutputFile);	
		return;
	}

	if (strBuffer.Find(szWHERECLAUSE) >= 0)
	{
		ModifyWhereClause(fOutputFile, strBuffer);	
		return;
	}

	fOutputFile.WriteString(strBuffer + _T("\n"));
}

//-----------------------------------------------------------------------------
CString ReportGenerator::GetReportName(const CString& strReportPath)
{
	// il nome di un report di auditing è Report_AUDIT_TABLENAMEXXX.wrm dove XXX è un numero progressimo 
	// in modo da aver + report di auditing associati ad un singolo documento per alluser e singolo utente 
	// seguendo la logica di personalizzazione dei report
	CString strAuditName = _T("Report_") + m_pAuditMng->GetAuditRec()->GetTableName();
	CString strReportName;

	// prima provo con il semplice nome senza numero
	if (!::ExistFile(strReportPath + SLASH_CHAR + strAuditName + DOT_CHAR + FileExtension::WRM_EXT()))
		return strAuditName;

	for (int i = 1; i <= 9999; i++)
	{
		strReportName = cwsprintf(_T("%s%d"), strAuditName, i);
		if (!::ExistFile(strReportPath + SLASH_CHAR + strReportName + DOT_CHAR + FileExtension::WRM_EXT()))
			return strReportName;
	}

	return strAuditName;
}

//-----------------------------------------------------------------------------
CTBNamespace* ReportGenerator::CreateReportFromTemplate(BOOL bAllUsers, const CString& strUser)
{
	CTBNamespace auditNamespace(CTBNamespace::MODULE, szExtensionsApp + CTBNamespace::GetSeparator() + _T("TbAuditing"));

	CString strModuleReportPath = AfxGetPathFinder()->GetModuleReportPath(*m_pNamespace, bAllUsers ? CPathFinder::ALL_USERS : CPathFinder::USERS, strUser, TRUE);
	CString strAuditReportPath = AfxGetPathFinder()->GetModuleReportPath(auditNamespace, CPathFinder::STANDARD);
	// recupero il file template da cui creare il report di auditing e il file contenente la descrizione dei titoli
	CString strTemplateName =  strAuditReportPath + SLASH_CHAR + szAuditReportTemplate;
	CString strTitleFileName = strAuditReportPath + SLASH_CHAR + szLocalizeReportTitles;
			
	
	//creo prima un file temporaneo 
	CString strTempFileName(GetTempName(FileExtension::WRM_EXT()));
		
	CString strReportName = GetReportName(strModuleReportPath);

	CTBNamespace* pReportNs = new CTBNamespace(CTBNamespace::REPORT, m_pNamespace->GetApplicationName() + CTBNamespace::GetSeparator() + m_pNamespace->GetModuleName()); 
	pReportNs->SetObjectName(strReportName);

	// nome completo del file da creare
	strReportName = ::MakeFilePath(strModuleReportPath, strReportName, FileExtension::WRM_EXT());

	CLineFile fSourceFile;
	CLineFile fOutputFile;
 
    // apro il template
	TRY
	{
		CString strBuffer;
		if (
				fSourceFile.Open(strTemplateName, CFile::modeRead | CFile::shareDenyWrite | CFile::typeText)	&&
				fOutputFile.Open(strTempFileName, CFile::modeWrite | CFile::shareExclusive | CFile::modeCreate | CFile::typeText)
			)		
		{
			//leggo il file xml contenente i nomi da tradurre dei titoli delle colonne, report, askdialog....
			m_pXMLTitleDoc = new CLocalizableXMLDocument(auditNamespace, AfxGetPathFinder());
			if (!m_pXMLTitleDoc->LoadXMLFile(strTitleFileName))
			{
				AfxMessageBox(cwsprintf(_TB("Error reading the column's title about the file {0-%s}.\nThe report has not been create."), strTitleFileName)); 
				return FALSE;
			}
			
			fOutputFile.SetFormat(CLineFile::UTF8);
			while (fSourceFile.ReadString(strBuffer))
				WriteReport(fOutputFile, strBuffer);

			// Close both files
			fOutputFile.Close();
			fSourceFile.Close();
			
			m_pXMLTitleDoc->Close();
			delete m_pXMLTitleDoc;
			m_pXMLTitleDoc = NULL;

			//se tutto è andato bene rinomino il file temporaneo
			if (RenameFilePath(strTempFileName, strReportName))
			{
				if (AfxMessageBox(cwsprintf(_TB("Auditing report {0-%s} has been created.\nDo you want to view it?"), ::GetName(strReportName)), MB_YESNO) == IDYES) 
				{
					CWoormDoc* pWoormDoc = AfxGetTbCmdManager()->RunWoormReport(strReportName);
					if (pWoormDoc)
						pWoormDoc->GetFirstView()->ShowWindow(SW_SHOW);
				}
				return pReportNs;
			}
		}
		return NULL;
	}	
	CATCH(CFileException, e)
	{
		if (m_pXMLTitleDoc)
		{
			m_pXMLTitleDoc->Close();
			delete m_pXMLTitleDoc;
			m_pXMLTitleDoc = NULL;
		}
		return NULL;
		//CString strMessage = e->GetErrorMessage();
	}
	END_CATCH
}
