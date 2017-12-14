
#include "stdafx.h"

#include <TbClientCore\ClientObjects.h>
#include <TbWebServicesWrappers\LoginManagerInterface.h>

#include <TbGeneric\minmax.h>
#include <TbGeneric\globals.h>
#include <TbGeneric\DataObj.h>
#include <TbGeneric\FormatsTable.h>
#include <TbGeneric\FontsTable.h>
#include <TbGeneric\EnumsTable.h>

#include <TbGenlib\const.h>
#include <TbGenlib\FunProto.h>

#include <TbOledb\wclause.h>
#include <TbOledb\sqlrec.h>
#include <TbOledb\sqlAccessor.h>				
#include <TbOledb\sqltable.h>				

#include <TbWoormViewer\PageInfo.h>				

#include <TbGes\extdoc.h>
#include <TbGes\dbt.h>

#include "wrmmaker.h"

//includere come ultimo include all'inizio del cpp
#include "begincpp.dex"

#ifdef _DEBUG
#undef THIS_FILE
static const char BASED_CODE THIS_FILE[] = __FILE__;
#endif

//=============================================================================
#define REPORT_ROWS	    35
#define REPORT_H_ROW	16
#define REPORT_RELEASE	7

//----------------------------------------------------------------------------
int GetColumnWidth(const SqlColumnInfo* pItem, CBaseDocument* pDoc)
{
	ASSERT(pDoc);

	// fa scomparire artificialmente la colonna di tipo speciale che comunque
	// deve essere persente in tabella per poter restituire un valore
	if (!pItem->m_bVisible && pItem->m_bSpecial)
		return 0;

	BOOL bString = pItem->m_DataObjType == DATA_STR_TYPE || pItem->m_DataObjType == DATA_TXT_TYPE;

	int nLen = bString
		? pItem->m_lLength
		: AfxGetFormatStyleTable()->GetOutputCharLen(pItem->m_DataObjType, &pDoc->GetNamespace());
	
	if (nLen < 0)
		return 0;

	// poichè il pageInfo è solo uno short, limito un pochino le colonne
	// molto grosse, così da fare il display di più colonne possibili.
	if (nLen > 40)	nLen = 40;

	CAbstractFormDoc* pAbstractDoc = (CAbstractFormDoc*) pDoc;

	
	int nFontIdx			= AfxGetFontStyleTable()->GetFontIdx(bString || pItem->m_DataObjType == DATA_DATE_TYPE ? FNT_CELL_STRING : FNT_CELL_NUM);
	const FontStyle* pFont	= AfxGetFontStyleTable()->GetFontStyle(nFontIdx, &pDoc->GetNamespace());
	int nFontTitleIdx		= AfxGetFontStyleTable()->GetFontIdx(FNT_COLUMN_TITLE);
	const FontStyle* pTFont	= AfxGetFontStyleTable()->GetFontStyle(nFontTitleIdx, &pDoc->GetNamespace());
		
	int nWidth = 0;
	int nWidthTitle = 0;
	if (pAbstractDoc->GetMasterFrame()) 
	{
		CDC* pDC = pAbstractDoc->GetMasterFrame()->GetDC();
		nWidth = pFont->GetStringWidth (pDC, nLen).cx;
		nWidthTitle = pTFont->GetStringWidth(pDC, pItem->GetColumnTitle()).cx;
		pAbstractDoc->GetMasterFrame()->ReleaseDC(pDC);
	}
	else
	{
		CWnd* pMenu = AfxGetMenuWindow();
		CDC* pDC = pMenu ? pMenu->GetDC() : NULL;
		nWidth = pFont->GetStringWidth (pDC, nLen).cx;
		nWidthTitle = pTFont->GetStringWidth(pDC, pItem->GetColumnTitle()).cx;
		if (pMenu && pDC)
			pMenu->ReleaseDC(pDC);
	}
	return max(nWidthTitle, nWidth) + 8;
}

// tengo conto anche del bordo verticale (3 pixel) ((w+3)*3)
int ScaleColumnWidth(int w) 
{ return int(LPtoMU((w+3), CM, MU_SCALE, MU_DECIMAL)); }

///////////////////////////////////////////////////////////////////////////////
// class CWrmMaker
///////////////////////////////////////////////////////////////////////////////
//
///////////////////////////////////////////////////////////////////////////////

//------------------------------------------------------------------------------
CWrmMaker::CWrmMaker(CAbstractFormDoc* pDocument)
	:
	m_pSqlTable		(NULL),
	m_pSqlRecord	(NULL),
	m_pDocument		(pDocument),
	m_nAliasCount	(1),
	m_nTableColumnCount (0),
	m_bUnparseOnString (FALSE)
{
}

//------------------------------------------------------------------------------
CWrmMaker::~CWrmMaker()
{
	m_pSqlTable		= NULL;
	m_pSqlRecord	= NULL;
}

// cancella il report generato precedentemente, solo se gia` generato ed esiste 
//------------------------------------------------------------------------------
void CWrmMaker::DeleteReport()
{
	if (m_strReportName.IsEmpty())
		return;

	if (ExistFile(m_strReportName))
		DeleteFile (m_strReportName);

	m_strReportName.Empty();
}

//------------------------------------------------------------------------------
BOOL CWrmMaker::BuildWoorm(BOOL bUnparseOnString /*= FALSE*/)
{
	m_pSqlTable		= m_pDocument->m_pDBTMaster->m_pTable;
	m_pSqlRecord	= m_pDocument->m_pDBTMaster->m_pRecord;

	ASSERT(m_pSqlTable);
	ASSERT(m_pSqlRecord);

	m_strReportName.Empty();
	m_bUnparseOnString = bUnparseOnString;

	CLineFile		fOutputFile(bUnparseOnString);
    CFileException	FileException;
	BOOL			bError = FALSE;

	if (!bUnparseOnString)
	{
		// fi faccio dare un nome temporaneo che disgraziatamente finisce per .tmp e la 
		// chiamata lo crea con size vuoto, pertanto devo poi cancellarlo altrimenti 
		// rimane su disco a fare numero!
		m_strReportName = GetTempName(FileExtension::WRM_EXT()); DeleteFile(m_strReportName);

		// forza l'estensione .wrm se no sarebbe .tmp
		m_strReportName = MakeName(m_strReportName, FileExtension::WRM_EXT()); 

		// il report temporaneo viene lasciato nella temp locale per due motivi:
		// velocità di esecuzione e non apparizione del file nei report utente del menu
		// in caso rimanga appeso su disco.

		// test open error like access denied
		if (!fOutputFile.Open(m_strReportName, CFile::modeWrite | CFile::modeCreate | CFile::shareDenyNone | CFile::typeText, &FileException, CLineFile::UTF8))
		{
			AfxMessageBox(cwsprintf(_TB("Unable to open report {0-%s}")));
			return FALSE;
		}
	}
	// permetto di definire quali colonne far vedere
	m_pDocument->OnCustomizeWrmRadar();

	// scrivo il report su file
	TRY
	{                          
		WriteReport(fOutputFile);
	}
	CATCH (CFileException, e)
	{
		AfxMessageBox(cwsprintf(_TB("Unable to generate report {0-%s}:\r\n {1-%s}"), m_strReportName, e->m_cause));
		bError = TRUE;
	}
	END_CATCH

	fOutputFile.Close();

	if (m_bUnparseOnString)
		m_strReportName = fOutputFile.GetBufferString();

	return !bError;
}

//------------------------------------------------------------------------------
void CWrmMaker::WriteReport(CLineFile& fOutputFile)
{                          
	WS(fOutputFile, _T("//=============================================================================="));
	WS(fOutputFile, cwsprintf(_T("//  %d - Woorm code behind"), AfxGetApplicationYear()));
	WS(fOutputFile,_T("//=============================================================================="));
	CR(fOutputFile);
	PI(fOutputFile, _T("Release %d Rect (%d, %d, %d, %d)"));
	WS(fOutputFile, _T("PageLayout \"Default\" "));
	WS(fOutputFile, _T("Begin"));
	TT(fOutputFile, _T("	Table (%d,%d) \"%s\" Alias 1 Origin (0,0) Heights (6,20,20,20)"));
	WS(fOutputFile, _T("	EasyView Color (244,248,248) HideTitle"));
	WS(fOutputFile, _T("	Begin"));
	TF(fOutputFile, _T("		\"%s\" Alias %d Width %d FormatStyle \"%s\" %s"));

	//TODO parametrizzare colori prendendoli dal Thema
	WS(fOutputFile, cwsprintf(_T("		ColumnPen Column (0,%d)  Pen (160,160,160);"), m_nTableColumnCount -1 ));
	WS(fOutputFile, cwsprintf(_T("		Title Column (0,%d) BkgColor (249,249,249) Pen (160,160,160) ;"), m_nTableColumnCount - 1));

	//WS(fOutputFile, _T("		Body Begin All TextColor (0,0,0) ; End"));

	WS(fOutputFile, _T("	End"));
/* TODO aggiungere testo e localizzare "Page-Sheet: n.1"  "Foglio 1 della pagina 4"
	WS(fOutputFile, _T("		Text (666,988,688,1052) \"{Page.Splitter}\" \
			Begin\
				AnchorPageLeft \
				Special\
				Align 2149 ;\
			End"));
*/
	WS(fOutputFile, _T("End"));
	CR(fOutputFile);
	WS(fOutputFile, _T("Links"));
	WS(fOutputFile, _T("Begin"));
	SL(fOutputFile, _T("\tLinkRadar %s On Alias %d"));
	WS(fOutputFile, _T("\tBegin "));
	KY(fOutputFile, _T("\t%s Alias %d"));
	WS(fOutputFile, _T("\tEnd"));
	WS(fOutputFile, _T("End"));
	CR(fOutputFile);
	WS(fOutputFile, _T("Report"));
	WS(fOutputFile, _T("	Begin NoName"));
	WS(fOutputFile, _T("		Tables"));
	RT(fOutputFile, _T("			Tabella_1 [%d] Alias 1 ;"));
	WS(fOutputFile, _T("		End"));
	CR(fOutputFile);
	WS(fOutputFile, _T("		Variables"));
	RV(fOutputFile,	_T("			%s    %s    [%s] Alias %d %s;"));
	WS(fOutputFile, _T("		End"));
	CR(fOutputFile);
	WS(fOutputFile, _T("		Rules"));
	RN(fOutputFile, _T("			From %s"));
	WS(fOutputFile, _T("			Select  Not NULL"));
	RS(fOutputFile, _T("				%s        Into %s"));
	WW(fOutputFile, _T("				Where %s"));
	WO(fOutputFile, _T("				Order By %s"));
	WS(fOutputFile, _T("		End"));
	CR(fOutputFile);
	WS(fOutputFile, _T("		Events"));
	WS(fOutputFile, _T("			Report : Do"));
	WS(fOutputFile, _T("				Always" ));
	WS(fOutputFile, _T("					Begin"));
	WS(fOutputFile, _T("					End"));
	WS(fOutputFile, _T("				Before" ));
	WS(fOutputFile, _T("					Begin"));
	//WS(fOutputFile, m_strLoadFindParameters);
	WS(fOutputFile, _T("					End"));
	WS(fOutputFile, _T("				After" ));
	WS(fOutputFile, _T("					Begin"));
	WS(fOutputFile, _T("					End"));
	WS(fOutputFile, _T("		End"));
	CR(fOutputFile);
	WS(fOutputFile, _T("	End"));
}

//------------------------------------------------------------------------------
void CWrmMaker::WS(CLineFile& fOutputFile, const CString& strMask)
{
	fOutputFile.WriteString(strMask + _T("\n")); 
}

//------------------------------------------------------------------------------
void CWrmMaker::CR(CLineFile& fOutputFile)	
{
	fOutputFile.WriteString(_T("\n")); 
}

//Table (16,2) "Tabella_1" Alias 1 Origin (78,2) Heights (14,16,14,14) 
//------------------------------------------------------------------------------
void CWrmMaker::TT(CLineFile& fOutputFile,const CString& strMask)
{
	ASSERT(m_pSqlTable);
	ASSERT(m_pSqlRecord);

	// conteggia quanti sono visibili o chiave
	m_nTableColumnCount = 0;
	for (int i = 0; i < m_pSqlRecord->GetSizeEx(); i++)
	{
		const SqlColumnInfo* pColInfo = m_pSqlRecord->GetColumnInfo(i);
		if (!pColInfo || (!pColInfo->m_bVisible && !pColInfo->m_bSpecial)) 
			continue;

		m_nTableColumnCount++;
	}

	CString str;
	str = cwsprintf
	(	strMask,
		REPORT_ROWS,
		m_nTableColumnCount,
		(LPCTSTR) m_pSqlTable->GetTableTitle(),
		1 // Alias sempre 1 per la Tabella del Master
	);
	fOutputFile.WriteString(str + _T("\n"));
}


//"Release 7 Rect (50,85,600,792)"
//PageInfo (2,0,2970,A4_WIDTH,100,1,1,True ) // A4 landscape
//------------------------------------------------------------------------------
void CWrmMaker::PI(CLineFile& fOutputFile, const CString& strMask)
{
	ASSERT(m_pSqlTable);
	ASSERT(m_pSqlRecord);

	int nWidth = 0, nHeight = 0;
	for (int i = 0; i < m_pSqlRecord->GetSizeEx(); i++)
	{
		const SqlColumnInfo* pColInfo = m_pSqlRecord->GetColumnInfo(i);
		if (!pColInfo || (!pColInfo->m_bVisible && !pColInfo->m_bSpecial)) 
			continue;
		
		nWidth += ScaleColumnWidth(GetColumnWidth(pColInfo, m_pDocument)); //@@ TODO verificare i conti
	}

	// deve essere short perche PageInfo e` in decimi di millimetro
	nWidth = Min(MAXSHORT, nWidth);
    
	//nHeight = (REPORT_ROWS+2) * (REPORT_H_ROW + 3) ;  //REPORT_H_ROW: altezza riga cablata + 3: larghezza pixel, 3: fattore di conversione
	//nHeight = Min(MAXSHORT, nHeight);
	//---------------------------------
	int top = 1, left = 1, bottom = 728, right = 950;
	//49,85,448,792
	CString str;
	str = cwsprintf
	(	strMask,
		REPORT_RELEASE,
		top,
		left,
		bottom,   //calcolato dinamicamente in base alle colonne del report
		right   
	);
	fOutputFile.WriteString(str + _T("\n"));
	//------------------------------------
	str = cwsprintf(_T("PageInfo (2,0,%d,%d,100,1,1,True)"), nWidth, A4_WIDTH);
	fOutputFile.WriteString(str + _T("\n"));

	//------------------------------------
	str = cwsprintf(_T("PrinterPageInfo(1,9,%d,%d,True)\n"), A4_HEIGHT, A4_WIDTH);
	fOutputFile.WriteString(str);
}
//------------------------------------------------------------------------------
void CWrmMaker::TF1(SqlRecord* pRecord, CLineFile& fOutputFile,const CString& strMask)
{
	CString strBuffer;
	int pageWidth = 0;
	int colWidth = 0;
	int prevColWidth = 0;

	for (int i = 0; i < pRecord->GetSizeEx(); i++)
	{
		const SqlColumnInfo* pColInfo = pRecord->GetColumnInfo(i);
		if (!pColInfo) 
			continue;

		// devo conteggiare anche quelli che metterò come hidden
		if (!pColInfo->m_bVisible && !pColInfo->m_bSpecial)
		{
			m_nAliasCount++;
			continue;
		}
		colWidth = GetColumnWidth(pColInfo, m_pDocument);
		//----

		if (i > 0)
		{
			pageWidth += ScaleColumnWidth(prevColWidth);
			if ((ScaleColumnWidth(colWidth) + pageWidth) > A4_HEIGHT)	//è landscape
			{
				//set splitter sulla colonna PRECEDENTE
				fOutputFile.WriteString(_T(" Splitter;\n"));
				pageWidth = 0;
			}
			else
			{
				fOutputFile.WriteString(_T(";\n"));
			}
		}
		//----

		CString strTitle = pColInfo->GetColumnTitle();

		strBuffer = cwsprintf
		(
			strMask,
			(LPCTSTR) strTitle,
			m_nAliasCount++, 
			colWidth,
			(LPCTSTR) FromDataTypeToFormatName(pColInfo->m_DataObjType), 
			(pColInfo->m_DataObjType == DATA_TXT_TYPE ? _T("Break") : _T(""))
		);
		fOutputFile.WriteString(strBuffer);

		prevColWidth = colWidth;
		//----
	}
	fOutputFile.WriteString(_T(";\n"));
}

//"Campo2" Alias 2 Width 30 FormatStyle "Bool" ;
//------------------------------------------------------------------------------
void CWrmMaker::TF(CLineFile& fOutputFile,const CString& strMask)
{
	ASSERT(m_pSqlTable);
	ASSERT(m_pSqlRecord);
	
	m_nAliasCount = 2; // Alias sempre a partire da 2 (1 e` la tabella)
	
	TF1(m_pSqlRecord, fOutputFile, strMask);
}

//%s Alias %d
//------------------------------------------------------------------------------
void CWrmMaker::KY(CLineFile& fOutputFile,const CString& strMask)
{
	ASSERT(m_pSqlTable);
	ASSERT(m_pSqlRecord);

	CString strBuffer;
	int nPKSeg = m_pSqlRecord->GetNumberSpecialColumns();

	for (int i = 0; i < m_pSqlRecord->GetSize(); i++)
	{
		// utilizza i campi speciali (primary key)
		const SqlColumnInfo* pItem = m_pSqlRecord->GetColumnInfo(i);
		if (!pItem || !pItem->m_bSpecial) 
			continue;
		
		int a = i + 2 ;// Alias sempre a partire da 2 (1 e` la tabella)
		strBuffer = cwsprintf
		(
			strMask,
			(LPCTSTR) (m_pSqlRecord->GetTableName() + '.' + pItem->m_strColumnName),
			a
		);
		fOutputFile.WriteString(strBuffer + "\n");
	}
}


//Tabella_1 [30] Alias 1 ;
//------------------------------------------------------------------------------
void CWrmMaker::RT(CLineFile& fOutputFile,const CString& strMask)
{
	CString str = cwsprintf
	(	
		strMask,
		REPORT_ROWS,
		1 // Alias sempre 1 per la Tabella del Master
	);
	fOutputFile.WriteString(str + _T("\n"));
}

//Select %s
//------------------------------------------------------------------------------
void CWrmMaker::SL(CLineFile& fOutputFile,const CString& strMask)
{
	ASSERT(m_pSqlTable);
	CString str = cwsprintf
	(	
		strMask,
		(LPCTSTR) m_pSqlTable->GetTableName(),
		1 // Alias sempre 1 per la Tabella del Master
	);
	fOutputFile.WriteString(str + _T("\n"));
}

//------------------------------------------------------------------------------
void CWrmMaker::RV1(SqlRecord* pRecord, int& nAlias, CLineFile& fOutputFile, const CString& strMask)
{
	CString strBuffer;
	CString strTypeWRM;
	int s = pRecord->GetSize();
	int sizeex = pRecord->GetSizeEx();
	for (int i = 0; i < sizeex; i++)
	{
		const SqlColumnInfo* pItem = pRecord->GetColumnInfo(i);
		if (!pItem) 
			continue;

		strTypeWRM = FromDataTypeToTokenString(pItem->m_DataObjType);

		if	(pItem->m_DataObjType == DATA_ENUM_TYPE)
		{
			strTypeWRM += cwsprintf(_T("[%d]"), pItem->m_DataObjType.m_wTag);
		}

		CString strVarName;
		
		int nLen = GetColumnWidth(pItem, m_pDocument);

		CString strLen;
		if (pItem->m_DataObjType == DATA_TXT_TYPE)
		{
			nLen = nLen / 10;
			strLen = cwsprintf(_T("%d,1"), nLen);
		}
		else
			strLen = cwsprintf(_T("%d"), nLen);
		
		strVarName = (i < s ? _T("w_") :  cwsprintf(_T("w%d_"), pRecord->GetExtensionIndex(i))) + pItem->m_strColumnName;

		strBuffer = cwsprintf
		(
			strMask,
			(LPCTSTR) strTypeWRM,
			(LPCTSTR) strVarName,
			strLen,
			nAlias++, 
			pItem->m_bVisible ? _T("Column") : _T("Hidden")
		);
		fOutputFile.WriteString(strBuffer + _T("\n"));	
	}
}

//Bool      Campo2    [1] Alias 2  Column ;
//------------------------------------------------------------------------------
void CWrmMaker::RV(CLineFile& fOutputFile, const CString& strMask)
{
	ASSERT(m_pSqlTable);
	ASSERT(m_pSqlRecord);

	int nAlias = 2;

	RV1(m_pSqlRecord, nAlias, fOutputFile, strMask);
}

//From FORMTBL 
//------------------------------------------------------------------------------
void CWrmMaker::RN(CLineFile& fOutputFile,const CString& strMask)
{
	ASSERT(m_pSqlTable);
	ASSERT(m_pSqlRecord);

	CString str;
	str = cwsprintf
	(	strMask,
		(LPCTSTR) m_pSqlTable->GetAllTableName()
	);
	fOutputFile.WriteString(str + _T("\n"));
}

//Campo2         Into Campo2 ;
//------------------------------------------------------------------------------
void CWrmMaker::RS(CLineFile& fOutputFile, const CString& strMask)
{
	ASSERT(m_pSqlTable);
	ASSERT(m_pSqlRecord);

	RS1(m_pSqlRecord, m_pSqlTable->GetTableName(), fOutputFile, strMask);
}

//------------------------------------------------------------------------------
void CWrmMaker::RS1(SqlRecord* pRecord, CString sTableName, CLineFile& fOutputFile, const CString& strMask)
{
	ASSERT_VALID(m_pSqlTable);
	ASSERT_VALID(m_pSqlRecord);

	CString strBuffer;
	BOOL bFirst = TRUE;
	int s = pRecord->GetSize();
	int sizeex = pRecord->GetSizeEx();
	for (int i = 0; i < sizeex; i++)
	{
		const SqlColumnInfo* pItem = pRecord->GetColumnInfo(i);
		if (!pItem || (!pItem->m_bVisible && !pItem->m_bSpecial)) 
			continue;
		ASSERT_VALID(pItem);

		if (!bFirst)
			fOutputFile.WriteString(_T(",\n"));

		bFirst = FALSE;

		if (i >= s)
		{
			int k = i;
			const SqlRecord* pRecEx = pRecord->LookupExtensionFromColumnIndex (k);
			ASSERT_VALID(pRecEx);
			if (pRecEx)
				sTableName = pRecEx->GetTableName();
			
			strBuffer = cwsprintf
			(
				strMask,
				(LPCTSTR) (sTableName + '.' + pItem->m_strColumnName),
				(LPCTSTR) (cwsprintf(_T("w%d_"), pRecord->GetExtensionIndex(i)) + pItem->m_strColumnName)
			);

		}
		else
		{
			strBuffer = cwsprintf
			(
				strMask,
				(LPCTSTR) (sTableName + '.' + pItem->m_strColumnName),
				(LPCTSTR) (CString(_T("w_")) + pItem->m_strColumnName)
			);
		
		}

		fOutputFile.WriteString(strBuffer);
	}
	fOutputFile.WriteString(_T("\n"));
}

//------------------------------------------------------------------------------
void CWrmMaker::WO(CLineFile& fOutputFile,const CString& strMask)
{
	if (!m_strOrderBy.IsEmpty())
	{
		CString strBuffer = cwsprintf(strMask,(LPCTSTR) m_strOrderBy);
		fOutputFile.WriteString(strBuffer);
		fOutputFile.WriteString(_T(";"));
		fOutputFile.WriteString(_T("\n"));
	}
}

//------------------------------------------------------------------------------
void CWrmMaker::WW(CLineFile& fOutputFile,const CString& strMask)
{
	if (!m_strFilter.IsEmpty() && !m_strFindFilter.IsEmpty())
		m_strFilter = m_strFilter + _T(" AND ") + m_strFindFilter;
	else if (m_strFilter.IsEmpty() && !m_strFindFilter.IsEmpty())
		m_strFilter = m_strFindFilter;

	if (!m_strFilter.IsEmpty() )
	{
		CString strBuffer = cwsprintf(strMask,(LPCTSTR) m_strFilter);
		fOutputFile.WriteString(strBuffer);

		fOutputFile.WriteString(_T("\n"));
	}
	// mette il punto e virgola finale solo se non ci sono altre clausole di 
	// WHERE o ORDER BY
	if (m_strOrderBy.IsEmpty())
		fOutputFile.WriteString(_T(";"));
}

///////////////////////////////////////////////////////////////////////////////
// Diagnostics
#ifdef _DEBUG
void CWrmMaker::Dump (CDumpContext& dc) const
{
	ASSERT_VALID (this);
	AFX_DUMP0(dc, " CWrmMaker\n");
	CObject::Dump(dc);
}
#endif // _DEBUG

