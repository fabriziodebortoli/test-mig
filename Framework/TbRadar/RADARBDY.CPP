
#include "stdafx.h"

#include <TbGeneric\DataTypesFormatters.h>
#include <TbGeneric\GeneralFunctions.h>

#include <TbGenlib\messages.h>
#include <TbGeneric\DataObj.h>
#include <TbGeneric\FormatsTable.h>
#include <TbGeneric\EnumsTable.h>
#include <TbGeneric\FontsTable.h>

#include <TbOledb\sqlrec.h>
#include <TbOledb\sqlcatalog.h>
#include <TbOledb\sqlaccessor.h>			
#include <TbOledb\sqltable.h>			

#include "radardoc.h"
#include "radarfrm.h"
#include "radarvw.h"
#include "radarbar.h"
#include "radarbdy.h"

#include "radar.hjson" //JSON AUTOMATIC UPDATE


//includere come ultimo include all'inizio del cpp
#include "begincpp.dex"

#ifdef _DEBUG
#undef THIS_FILE
static const char BASED_CODE THIS_FILE[] = __FILE__;
#endif

#define DEFAULT_TITLE_OFFSET	5

///////////////////////////////////////////////////////////////////////////////
// Implementazione di RadarCell
///////////////////////////////////////////////////////////////////////////////
//-----------------------------------------------------------------------------
RadarCell::RadarCell (LPCTSTR lpszValue, BOOL bMultiLines)
	:
	m_szValue		(lpszValue),
	m_bMultiLines	(bMultiLines)
{
}

///////////////////////////////////////////////////////////////////////////////
// Diagnostics
#ifdef _DEBUG
void RadarCell::Dump (CDumpContext& dc) const
{
	ASSERT_VALID (this);
	dc << " RadarCell\n\tValore: " << m_szValue << "\n";
	CObject::Dump(dc);
}
#endif // _DEBUG

///////////////////////////////////////////////////////////////////////////////
// Implementazione di RadarColumn
///////////////////////////////////////////////////////////////////////////////
//-----------------------------------------------------------------------------
RadarColumn::RadarColumn	(
								RadarBody*		pBody,
								int				nIdx,
								const SqlColumnInfo*	pSqlColInfo,
								int				nLen,
								LPCTSTR			szQualifiedName,
								LPCTSTR			lpszFmtStyle  /* = NULL */
							)
	:
	m_pBody				(pBody),				
	m_bSelect			(FALSE),
	m_nIdx				(nIdx),
	m_nXEnd				(0),
	m_nXEndOffset		(0),
	m_nXStart			(0),
	m_nStartIdxSetChars (0),
	m_nXStartOffset		(DEFAULT_COLUMN_OFFSET),
	m_nLen				(nLen),
	m_nScreenWidth		(0),
	m_szQualifiedName	(szQualifiedName),
	m_pPrivacyFormatter	(NULL)
{
	m_szName		= pSqlColInfo->GetColumnName	();
	m_DataType		= pSqlColInfo->GetDataObjType	();
	// All'inizio i due titoli sono uguali. Poi al variare della dimensione della colonna
	// se necessario viene modificato solo il titolo m_szTitle, nell'altra variabile ci
	// sarà sempre il valore originale
	m_szTitle		= pSqlColInfo->GetColumnTitle	();	//AfxLoadDatabaseString(m_strColumnName, m_strTableName);
	m_szOrigTitle	= m_szTitle;

	// A seconda del datatype mi predispongo i valori per la ExtTextOut utilizzata nella Paint
	m_nAlign = (m_DataType == DATA_STR_TYPE || m_DataType == DATA_TXT_TYPE || m_DataType == DATA_ENUM_TYPE)
					? DT_LEFT
					: (m_DataType == DATA_BOOL_TYPE ? DT_CENTER : DT_RIGHT);

	// Setta l'indice dello stile di formattazione
	SetFormatIdx(lpszFmtStyle);

	m_pPrivacyFormatter = new CPrivacyFormatter();
}

//-----------------------------------------------------------------------------
RadarColumn::~RadarColumn()
{
	if (m_pPrivacyFormatter)
		delete m_pPrivacyFormatter;
}

//-----------------------------------------------------------------------------
void RadarColumn::SetFormatIdx	(LPCTSTR lpszFmtStyle /* = NULL */)
{
	// Controllo se esiste la stringa identificante lo stile di formattazione salvata sul
	// file di inizializzazione
	if (lpszFmtStyle)
	{
		// Stile fi formattazione indicato nella stringa
		m_nIdxFmtStyle = AfxGetFormatStyleTable ()->GetFormatIdx(lpszFmtStyle);

		if (m_nIdxFmtStyle >= 0)
			return;
		// Lo stile del carattere indicato non è stato trovato, inserisco il relativo messaggio nella CMessage
		CString msg = cwsprintf	(
									_TB("The formatting style '{0-%s}' of column '{1-%s}' is wrong or has been deleted. The formatting style '{2-%s}' will be used instead."),
									lpszFmtStyle,
									(LPCTSTR) m_szName,
									(LPCTSTR) (AfxGetFormatStyleTable ()->GetStyleName(AfxGetFormatStyleTable ()->GetFormatIdx(m_DataType)))
								);
		AfxGetDiagnostic()->Add	(msg);
	}

	// Stile di formattazione di default per quel tipo di dato
	m_nIdxFmtStyle = (int) (AfxGetFormatStyleTable ()->GetFormatIdx(m_DataType));
	if (m_nIdxFmtStyle >= 0)
		return;

	// Lo stile del carattere di default non è stato trovato, inserisco il relativo messaggio nella CMessage
	CString msg = cwsprintf	(
								_TB("The formatting style '{0-%s}' of column '{1-%s}' is wrong or has been deleted. The data will be formatted as a simple string."),
								(LPCTSTR) AfxGetFormatStyleTable ()->GetStyleName(AfxGetFormatStyleTable ()->GetFormatIdx(m_DataType)),
								(LPCTSTR) m_szName
							);
	AfxGetDiagnostic()->Add(msg);
}

//-----------------------------------------------------------------------------
void RadarColumn::AddData (DataObj *pData, int nNrMultiLines)
{
	if (!pData) 
		return;
	CString str;
	BOOL bMultiLines = FormatString (pData, str, nNrMultiLines);
	Add (new RadarCell (str, bMultiLines));
}

//-----------------------------------------------------------------------------
void RadarColumn::InsertDataAt (int nRow, DataObj *pData, int nNrMultiLines)
{
	CString str;
	BOOL bMultiLines = FormatString (pData, str, nNrMultiLines);
	InsertAt (nRow, new RadarCell (str, bMultiLines));
}

//-----------------------------------------------------------------------------
void RadarColumn::SetDataAt (int nRow, DataObj *pData, int nNrMultiLines)
{
	RadarCell* pCell = GetAt(nRow);
	pCell->m_bMultiLines = FormatString (pData, pCell->m_szValue, nNrMultiLines);
	SetAt (nRow, pCell);
}

//-----------------------------------------------------------------------------
BOOL RadarColumn::GetString (int nRow, CString& szValue)
{
	RadarCell* pCell = GetAt(nRow);
	szValue = pCell->m_szValue;
	return pCell->m_bMultiLines;
}

//-----------------------------------------------------------------------------
RadarCell* RadarColumn::GetAt (int nRow)
{
	RadarCell* pCell = (RadarCell*) Array::GetAt(nRow);
	ASSERT_VALID (pCell);
	return pCell;
}

//-----------------------------------------------------------------------------
BOOL RadarColumn::FormatString (DataObj *pData, CString& strFmt, int nNrMultiLines /* -1 */)
{
	ASSERT_VALID (pData);
	//se il dato è protetto ed l'utente corrente non lo può visualizzare (protetto o da OSL o dalla RowSecurity)	
	if (pData->IsOSLHide() || pData->IsPrivate())
	{
		CString strValue = _T("*");
		if (m_pPrivacyFormatter)  m_pPrivacyFormatter->FormatDataObj (*pData, strFmt);
	}
	else
	{
	// Alla colonna non è stato associato nessun formattatore per il tipo di DatoObj
	// indicato
	if (m_nIdxFmtStyle <= 0)
		strFmt = pData->FormatData();
	else	
		AfxGetFormatStyleTable()->GetFormatter(m_nIdxFmtStyle, NULL)->FormatDataObj(*pData, strFmt);
	}
	// Il dato obj non è di tipo stringa, non esiste quindi il concetto di stringa su più linee
	if ((m_DataType != DATA_STR_TYPE && m_DataType != DATA_TXT_TYPE) || nNrMultiLines == -1)
		return FALSE;

	return CheckIsMultiLines (strFmt, nNrMultiLines);
}

// Algoritmo di skrinking. Agisce sul titolo della colonna, quando la lunghezza
// dello stesso è maggiore della dimensione della colonna.
// Per ridurre la lunghezza del titolo occorre eliminare i caratteri che lo
// compongono partendo da quelli meno significativi, in modo da mantenere
// sempre una certa comprensione. Si è quindi adottata la seguente algoritmo 
// di eliminazione:
//	- quelli contenuti nell'array szSetChar	(partendo dalla testa);
//	- le vocali								(partendo dalla coda);
//	- le consonanti							(partendo dalla coda).
// Il procedimento si interrompe appena la lunghezza del titolo è minore alla
// dimensione della colonna.
// Se invece la dimensione della colonna aumenta, si ripristina il titolo
// originale e si applica l'algoritmo sopra esposto.
//-----------------------------------------------------------------------------
const TCHAR* szSetChars = {_T(".,?\"'-_=+!:;\t*&()[]\\#/ ")};

void RadarColumn::CompressTitle (int nOrigWidth)
{
	int nWidth = nOrigWidth - DEFAULT_TITLE_OFFSET;
	if (nWidth <= 0)
		return;

	CClientDC dc(NULL);
	CSize sizeStr (dc.GetTextExtent(m_szTitle, m_szTitle.GetLength()));
	// Controlla il titolo attualmente utilizzato è contenuto nella colonna
	if (sizeStr.cx < nWidth)
	{
		if (m_szTitle == m_szOrigTitle)
			return;

		// Ripristina il titolo originale
		m_szTitle	= m_szOrigTitle;
		sizeStr		= dc.GetTextExtent(m_szTitle, m_szTitle.GetLength());
		m_nStartIdxSetChars = 0;  
		if (sizeStr.cx < nWidth)
			return;
	}

	int		nLen	= _tcslen (szSetChars);
	int		nLenStr = m_szTitle.GetLength();
	TCHAR*	pChar	= m_szTitle.GetBuffer (nLenStr);

	// Rimuove i caratteri dell'array szSetChars
	TCHAR* pTmpChar;
	for (; m_nStartIdxSetChars < nLen; m_nStartIdxSetChars++)
	{
		pTmpChar = _tcsrchr(pChar, szSetChars[m_nStartIdxSetChars]);
		while (pTmpChar)
		{
			*pTmpChar = NULL_CHAR;
			TB_TCSCAT (pChar, pTmpChar);
			++pTmpChar;
			CSize sizeStr (dc.GetTextExtent(pChar, --nLenStr));
			// Controlla se la lunghezza è minore della dimensione della colonna
			if (sizeStr.cx < nWidth)
			{
				m_szTitle.ReleaseBuffer();
				return;
			}
			pTmpChar = _tcsrchr(pChar, szSetChars[m_nStartIdxSetChars]);
		}
	}
	// Rimuove le vocali
	pTmpChar = pChar + nLenStr - 1;
	for (; pTmpChar != pChar; pTmpChar--)
		if (*pTmpChar == _T('a') || *pTmpChar == _T('e') || *pTmpChar == _T('u') || *pTmpChar == _T('i') || *pTmpChar == _T('o'))
		{
			*pTmpChar = NULL_CHAR;

			TB_TCSCAT (pChar, pTmpChar);
			++pTmpChar;
			CSize sizeStr (dc.GetTextExtent(pChar, --nLenStr));
			// Controlla se la lunghezza è minore della dimensione della colonna
			if (sizeStr.cx < nWidth)
			{
				m_szTitle.ReleaseBuffer(nLenStr);
				return;
			}
		}
	// Rimuove le consonanti
	sizeStr = dc.GetTextExtent(pChar, nLenStr);
	while (sizeStr.cx >= nWidth)
	{
		// adjust the title string 
		pTmpChar	= pChar + (--nLenStr);
		*pTmpChar	= NULL_CHAR;
		sizeStr		= dc.GetTextExtent(pChar, nLenStr);
	}
	m_szTitle.ReleaseBuffer(nLenStr);
}

//-----------------------------------------------------------------------------
void RadarColumn::UpdateMultiLinesFlag (int nNrMultiLines)
{
	// Se la colonna non contiene un dato di tipo stringa la salto, in qunato
	// per esse non esiste il concetto di multiline
	if (m_DataType != DATA_STR_TYPE && m_DataType != DATA_TXT_TYPE)
		return;

	for (int nRow = 0; nRow < GetSize(); nRow++)
	{
		RadarCell* pCell = GetAt(nRow);
		pCell->m_bMultiLines = CheckIsMultiLines (pCell->m_szValue, nNrMultiLines);
	}
}

//-----------------------------------------------------------------------------
BOOL RadarColumn::CheckIsMultiLines (CString strFmt, int nNrMultiLines)
{
	// La stringa è ad una singola linea
	int nIdx = strFmt.Find (_T("\r\n"));
	if (nIdx < 0)
		return FALSE;

	// Se la riga è su più linee si deve controllare se il numero di linee attualmente
	// in suo sia sufficiente per contenerla, altrimenti la devo troncare
	CString strTmpFmt (strFmt.Mid (nIdx+2));
	for (int nNrLine = 2; nNrLine <= nNrMultiLines; nNrLine++)
	{
		nIdx = strTmpFmt.Find (_T("\r\n"));
		// La riga è tutta visualizzabile
		if (nIdx < 0)
			return FALSE;

		strTmpFmt = strTmpFmt.Mid (nIdx+2);
	}
	// Aggiungo i simboli di segnalazione che la riga continua
	return TRUE;
}

///////////////////////////////////////////////////////////////////////////////
// Diagnostics
#ifdef _DEBUG
void RadarColumn::Dump (CDumpContext& dc) const
{
	ASSERT_VALID (this);
	AFX_DUMP1 (dc, " RadarColumn: Nome Campo: ", m_szName);
	AFX_DUMP1 (dc, "; Titolo Colonna: ", m_szOrigTitle);
	AFX_DUMP0 (dc, "\n");
	Array::Dump(dc);
}
#endif // _DEBUG

///////////////////////////////////////////////////////////////////////////////
// Implementazione di RadarBody
///////////////////////////////////////////////////////////////////////////////
//-----------------------------------------------------------------------------
RadarBody::RadarBody (CRadarDoc* pRadar)
	:
	m_pRadar				(pRadar),
	m_nActiveGrpRow			(0),
	m_nFirstIdxGrpRow		(0),
	m_nFirstVisibleRow		(0),
	m_nNrSelColumns			(0),
	m_nNrMaxFormatRows		(100), //TODO originale 50 monti 
	m_nCorrectCharWidth		(0),
	m_nNrMultiLines			(MULTI_LINES_MIN),
	m_nSelColumn			(-1),
	m_bSelColumnsContinue	(FALSE),
	m_bSameDataType			(FALSE)
{
}

//-----------------------------------------------------------------------------
RadarBody::~RadarBody	()
{
}

// Setta gli opportuni flag, in base alla colonne selezionate dall'utente.
//-----------------------------------------------------------------------------
//void RadarBody::SetColumnsFlags()
DataType RadarBody::SetColumnsFlags()
{
	ASSERT (m_nNrSelColumns);

	if (m_nNrSelColumns == 1)
	{
		// Essendoci un'unica colonna selezionata sarà sicuramnente dello stesso
		// tipo e la selezione è continua
		m_bSameDataType			= TRUE;
		m_bSelColumnsContinue	= TRUE;

// @@ TODO forse è da ottimizzare
		// Ricerca della colonna selezionata
		for (int nIdx = 0; nIdx < GetGrpTableNrCols(); nIdx++)
		{
			RadarColumn* pCol = GetColumn(nIdx);
			if (pCol->m_bSelect)
				return pCol->m_DataType;
		}
	}
	m_bSelColumnsContinue = (m_nNrSelColumns == GetGrpTableNrCols());

	int nBlockSel = 0;
	DataType nDataType(DATA_NULL_TYPE);
	for (int nIdx = 0; nIdx < GetGrpTableNrCols(); nIdx++)
	{
		RadarColumn* pCol = GetColumn(nIdx);
		if (pCol->m_bSelect)
		{
			if (!m_bSelColumnsContinue && nBlockSel == 0)
			{
				m_bSelColumnsContinue = TRUE;
				nBlockSel++;
			}
			if (nBlockSel == 2 && m_bSelColumnsContinue)
				m_bSelColumnsContinue = FALSE;
				
			// same data type
			if (nDataType == DATA_NULL_TYPE)
			{
				m_bSameDataType	= TRUE;
				nDataType		= pCol->m_DataType;
			}
			else if (m_bSameDataType && nDataType != pCol->m_DataType)
				m_bSameDataType = FALSE;
		}
		else if (nBlockSel == 1)
			nBlockSel++;
    }
	return nDataType;
}

//-----------------------------------------------------------------------------
void RadarBody::SelectedColumns(int nIdxStart, int nIdxEnd, int nInc, BOOL bSelect)
{
	// selects the new columns
	for (int nIdx = nIdxStart; nIdx <= nIdxEnd; nIdx++)
	{
		GetColumn (nIdx)->m_bSelect = bSelect;
		m_nNrSelColumns += nInc;
	}
}

// Aggiunge le informazioni di una colonna nell'array
//-----------------------------------------------------------------------------
int RadarBody::AddColumn (int nColIdx, const SqlColumnInfo* pSqlColInfo, LPCTSTR szQualifiedName)
{
	ASSERT_VALID (pSqlColInfo);

	if (!pSqlColInfo->m_bNativeColumnExpr)
	{
		if (pSqlColInfo->m_bVirtual)
			return -1;

		if (!pSqlColInfo->m_bVisible)
			return -1;
	}

	return m_GrpTable.Add (new RadarColumn (this, nColIdx, pSqlColInfo, CheckColumnWidth (pSqlColInfo), szQualifiedName, NULL));
}

// Inserisce le informazioni di una colonna nella posizione indicata (nCol) nell'array.
// Viene utilizzata dalla procedura di inserimento delle colonne cancellate.
// NB: la conversione a CObject è ASSOLUTAMENTE NECESSARIA.
//-----------------------------------------------------------------------------
int RadarBody::InsertColumn (int nPos, int nColIdx, const SqlColumnInfo* pSqlColInfo, LPCTSTR szQualifiedName)
{
	ASSERT_VALID (pSqlColInfo);
	RadarColumn* pCol = new RadarColumn(this, nColIdx, pSqlColInfo, CheckColumnWidth (pSqlColInfo), szQualifiedName, NULL);
	m_GrpTable.InsertAt(nPos, (CObject*)pCol);	//serve il cast per l'ambiguità dell'overloading con parametro di tipo CObArray (RadarColumn derica da esso)
	return nPos;
}

// Aggiunge nella posizione indicata da (nCol) le informazioni di una colonna.
// Viene chiamata quando esiste un precedente salvataggio sul file .INI,
//-----------------------------------------------------------------------------
void RadarBody::SetColumnAt(int nPos, int nColIdx, const SqlColumnInfo* pSqlColInfo, int nLen, LPCTSTR szQualifiedName, LPCTSTR lpszFmtStyle)
{
	// Inserisce nella posizione indicata da nCol la nuova colonna.
	RadarColumn* pCol = new RadarColumn(this, nColIdx, pSqlColInfo, nLen, szQualifiedName, lpszFmtStyle);
	m_GrpTable.SetAtGrow (nPos, pCol);
}

//-----------------------------------------------------------------------------
int RadarBody::FindColumn (const CString& sQualifiedName)
{
	for (int i = 0; i < m_GrpTable.GetSize(); i++)
	{
		RadarColumn* pCol = (RadarColumn*) m_GrpTable.GetAt(i);
		ASSERT_VALID(pCol);

		if (pCol->m_szQualifiedName.CompareNoCase(sQualifiedName) == 0)
			return i;
	}
	return -1;
}

//-----------------------------------------------------------------------------
BOOL RadarBody::MoveColumn (int nFrom, int nTo)
{
	if (
		nFrom < 0 || nFrom >= m_GrpTable.GetSize() ||
		nTo < 0 || nTo > m_GrpTable.GetSize() ||
		nFrom == nTo
		)
	{
		ASSERT_TRACE(FALSE,  _T("Radar fails to move columns: it was called with wrong column index"));
		return FALSE;
	}

	RadarColumn* pCol = (RadarColumn*) m_GrpTable.GetAt(nFrom);

	m_GrpTable.SetAt(nFrom, NULL);	//altrimenti lo deleta
	m_GrpTable.RemoveAt(nFrom);

	m_GrpTable.InsertAt(nTo > nFrom ? nTo - 1 : nTo, (CObject*)pCol);	//cast necessario per disambiguare overloading di InsertAt
	
	UpdateColumnsStartEnd(max(0, min(nFrom, nTo - 1)), TRUE);

	return TRUE;
}

//-----------------------------------------------------------------------------
BOOL RadarBody::SetColumnTitle (int nIdxCol, LPCTSTR szTitle)
{
	if (nIdxCol < 0 || nIdxCol >= m_GrpTable.GetSize())
	{
		ASSERT_TRACE(FALSE,  _T("Radar fails to set column title: it was called with wrong column index"));
		return FALSE;
	}

	RadarColumn* pCol = (RadarColumn*) m_GrpTable.GetAt(nIdxCol);
	pCol->m_szOrigTitle = pCol->m_szTitle = szTitle;
	return TRUE;
}

//-----------------------------------------------------------------------------
BOOL RadarBody::SetColumnAlign (int nIdxCol, UINT align)
{
	if (nIdxCol < 0 || nIdxCol >= m_GrpTable.GetSize())
	{
		ASSERT_TRACE(FALSE,  _T("Radar fails to set column title: it was called with wrong column index"));
		return FALSE;
	}

	RadarColumn* pCol = (RadarColumn*) m_GrpTable.GetAt(nIdxCol);
	pCol->m_nAlign = align;
	return TRUE;
}

//-----------------------------------------------------------------------------
BOOL RadarBody::RemoveColumn (int nIndex)
{
	if (nIndex < 0 || nIndex >= m_GrpTable.GetSize())
	{
		ASSERT_TRACE(FALSE,  _T("Radar fails to remove column: it was called with wrong column index"));
		return FALSE;
	}

	m_GrpTable.RemoveAt(nIndex);
	
	UpdateColumnsStartEnd(nIndex, TRUE);

	return TRUE;
}

//-----------------------------------------------------------------------------
int RadarColumn::CalcScreenWidth (CRadarDoc* pDoc, CDC* pDC)
{
	int nTitleWidth	= ::GetEditSize(pDC, pDoc->GetRadarView()->GetFont(), GetOriginalTitle()).cx;

	DataType aType = GetDataType();

		// le stringhe le lascio stare in modo che l'utente possa definirsele come vuole
		if (aType.m_wType == DATA_STR_TYPE || aType.m_wType == DATA_TXT_TYPE)
	{
		return max(nTitleWidth, ::GetEditSize(pDC, pDoc->GetRadarView()->GetFont(), GetLen(), TRUE).cx);
	}

	Formatter* pCurrFmt = AfxGetFormatStyleTable()->GetFormatter(GetIdxFmtStyle(), &pDoc->GetNamespace());
		if (!pCurrFmt)
	{
		return max(nTitleWidth, GetLen() * m_pBody->GetCorrectCharWidth());
	}

	CString strData;
	int nWidth = 0;

		// enumerativo
		if (aType.m_wType == DATA_ENUM_TYPE)
		{
			ASSERT(aType.m_wTag);
			int iv = AfxGetEnumsTable()->GetEnumLongerItemValue(aType.m_wTag);
			if (iv != 0xFFFF)
			{
				nWidth = GetEditSize(pDC, pDoc->GetRadarView()->GetFont(), pCurrFmt->GetTitle()).cx;
			}
			else
			nWidth = 1;
		}
		else
		nWidth = pCurrFmt->GetInputWidth(pDC, 0, pDoc->GetRadarView()->GetFont()).cx; 
	
		// aggiungo un pelino di margine
	nWidth += 2;

		// prima grandezza minima del titolo
	if (nWidth < nTitleWidth)
		nWidth = nTitleWidth;

		// poi controllo che non sia troppo piccola
	if (nWidth <  COLUMN_CHAR_WIDTH_MIN_VALUE)
		nWidth = COLUMN_CHAR_WIDTH_MIN_VALUE;

	return nWidth;
}

//-----------------------------------------------------------------------------
void RadarBody::UpdateColumnsWidthOnFonts (CRadarDoc* pDoc, int startCol/* = 0*/)
{
	// A questo punto posso verificare di non sotto dimensionarmi 
	// in automatico rispetto al default di formattazione, altrimenti
	// l'effetto sono una serie di colonne di cui non si legge nulla.

	CDC* pDC = pDoc->GetRadarView()->GetDC();

	for (int i = startCol; i <= m_GrpTable.GetUpperBound(); i++)
	{
		RadarColumn* pColumn = (RadarColumn*) m_GrpTable.GetAt(i);
		if (!pColumn)
			continue;
		if (pColumn->GetScreenWidth() > 0)
			continue;

		int nWidth = pColumn->CalcScreenWidth(pDoc, pDC);

		SetColumnToWidthChar (i, nWidth, FALSE);
	}

	pDoc->GetRadarView()->ReleaseDC(pDC);

	UpdateColumnsStartEnd (0, TRUE);
}

// Calcola la larghezza della colonna, prendendo il valore massimo
// tra la dimensione fisica del campo e la lunghezza del titolo. Questo valore
// non può essere inferiore ad una soglia prestabilita. Il risultato finale viene
// poi convertito in pixel per ottimizzare il tempo di 'paint'.
//-----------------------------------------------------------------------------
int RadarBody::CheckColumnWidth (const SqlColumnInfo* pSqlColInfo)
{
	// Calcola la corretta dimensione della colonna, prendendo il massimo tra la
	// lunghezza descrizione e quella effettiva del campo.
	int nWidth	= pSqlColInfo->GetColumnLength ();
	//int nLen	= (int) (pSqlColInfo->GetColumnTitle().GetLength ());

	//if (nLen > nWidth)
	//	nWidth = nLen;

 	// Comunque non può essere minore di COLUMN_CHAR_WIDTH_MIN_VALUE e Maggiore di 
 	// COLUMN_CHAR_WIDTH_MAX_VALUE. Viene quindi effettuata
 	// la conversione in pixel.
	if (nWidth < COLUMN_CHAR_WIDTH_MIN_VALUE)
		nWidth = COLUMN_CHAR_WIDTH_MIN_VALUE;
	else if (nWidth > COLUMN_CHAR_WIDTH_MAX_VALUE)
		nWidth = COLUMN_CHAR_WIDTH_MAX_VALUE;

	return nWidth /* * GetCorrectCharWidth()*/;
}

// Aggiunge alla tabella la nuova riga di dati
//-----------------------------------------------------------------------------
void RadarBody::AddRowGrpTable(SqlRecord* pRec)
{
	for (int nCol = 0; nCol < GetGrpTableNrCols(); nCol++)
	{
		RadarColumn* pCol = GetColumn(nCol);
		AddDataToColumn (pCol, pRec->GetDataObjAt(pCol->GetIdx()));
	}
}

// Inserisce in testa alla tabella la nuova riga di dati. Se il numero di righe
// supera il valore massimo viene rimossa l'ultima.
//-----------------------------------------------------------------------------
void RadarBody::AddRowHeadGrpTable(SqlRecord* pRec)
{
	// Inserisce il nuovo record in testa all'array di record fomattati
	for (int nCol = 0; nCol < GetGrpTableNrCols(); nCol++)
	{
		RadarColumn* pCol = GetColumn(nCol);
		pCol->InsertDataAt (0, pRec->GetDataObjAt(pCol->GetIdx()), m_nNrMultiLines);
	}
	// Rimozione degli eventuali record formattati in eccedeneza
	if (GetGrpTableNrRows() > m_nNrMaxFormatRows)
		RemoveRow (m_nNrMaxFormatRows);
}

// Aggiunge alla tabella la nuova riga di dati. Se il numero di righe supera il
// valore massimo viene rimossa la prima.
//-----------------------------------------------------------------------------
void RadarBody::AddRowTailGrpTable (SqlRecord* pRec)
{
	AddRowGrpTable (pRec);
	if (GetGrpTableNrRows() > m_nNrMaxFormatRows)
		RemoveRow (0);
}

// Aggiorna il DatoObj formattato contenuto dell'array dei record formattati nella
// colonna identificata da pCol 
//-----------------------------------------------------------------------------
void RadarBody::ReformatColumn (RadarColumn*, DataObj*, int)
{
/*	@@ TODO
	int				nRec = m_pRadarBody->m_nFirstIdxGrpRow;

	for (int nIdx = 0; nIdx < m_pRadarBody->GetLastIdxGrpRow(); nIdx++)
	{
		SqlRecord* pRec = m_pSqlRecCircleList->GetSqlRecord (nRec++);
		m_pRadarBody->ReformatColumn (pCol, pRec->GetDataObjAtEx(pCol->GetColumnIdx()), nIdx);
	}
	ASSERT (pCol);
	pCol->SetDataAt(nInsertIdx, pData);
*/
}

// Ripulisce l'array dei record formattati
//-----------------------------------------------------------------------------
void RadarBody::RemoveAllRows ()
{
	for (int nCol = 0; nCol < GetGrpTableNrCols(); nCol++)
		GetColumn(nCol)->RemoveAll();
}

// Rimuove un intero record nell'array dei record formattati
//-----------------------------------------------------------------------------
void RadarBody::RemoveRow (int nRow)
{
	for (int nCol = 0; nCol < GetGrpTableNrCols(); nCol++)
		GetColumn(nCol)->RemoveAt(nRow);
}

// Sposta la colonna o le colonne selezionate verso sinistra. Se le colonne confinano
// già con il bordo sinistra la procedura esegue nessuno spostamento.
//-----------------------------------------------------------------------------
void RadarBody::ShiftColumnLeft(int& nXStart, int& nXEnd)
{
	ASSERT (!GetColumn (0)->m_bSelect);
	ASSERT (m_nNrSelColumns && m_nNrSelColumns < GetGrpTableNrCols ());

	int		nStartCol	= -1; 
	BOOL	bMoved		= FALSE;
	for (int nCol = 1; nCol < GetGrpTableNrCols (); nCol++)
	{
		RadarColumn* p1 = GetColumn (nCol);
		if (!p1->m_bSelect)
		{
			// Ricerca della prima colonna selezionata
			if (!bMoved)
				continue;

			// Ha trovato un colonna non selezionata, ma lo spostamento può avvenire solo
			// se la selezione è continua, per cui ho completata l'operazione
			UpdateColumnsStartEnd (nStartCol, TRUE);
			nXStart	= GetColumn (nStartCol)->m_nXStart;
			nXEnd	= GetColumn (nCol)->m_nXEnd;
			return;
		}
		RadarColumn* p2 = GetColumn (nCol - 1);

		// Sposto le due colonne adiacenti all'interno dell'array
		m_GrpTable.SetAt (nCol, p2);
		m_GrpTable.SetAt (nCol - 1, p1);
		if (!bMoved)
		{
			// Memorizzo l'indice del primo scambio di colonna
			nStartCol	= nCol == 1 ? 0 : nCol - 2;
			bMoved		= TRUE;
		}
	}
	// La colonna o le colonne sono state spostate sino al bordo destro della finestra
	UpdateColumnsStartEnd (nStartCol, TRUE);
	nXStart	= GetColumn (nStartCol)->m_nXStart;
	nXEnd	= GetColumn (GetGrpTableNrCols () -1)->m_nXEnd;
}

// Sposta la colonna o le colonne selezionate verso destra. Se le colonne confinano
// già con il bordo destra la procedura esegue nessuno spostamento.
//-----------------------------------------------------------------------------
void RadarBody::ShiftColumnRight(int& nXStart, int& nXEnd)
{
	ASSERT (!GetColumn (GetGrpTableNrCols() - 1)->m_bSelect);
	ASSERT (m_nNrSelColumns && m_nNrSelColumns < GetGrpTableNrCols ());

	BOOL	bMoved	= FALSE;
	int		nEndCol	= -1; 

	for (int nCol = (GetGrpTableNrCols () - 2); nCol >= 0; nCol--)
	{
		RadarColumn* p1 = GetColumn (nCol);
		if (!p1->m_bSelect)
		{
			// Ricerca della prima colonna selezionata
			if (!bMoved)
				continue;

			// Ha trovato un colonna non selezionata, ma lo spostamento può avvenire solo
			// se la selezione è continua, per cui ho completata l'operazione
			UpdateColumnsStartEnd (nCol, TRUE);
			nXStart	= GetColumn (nCol + 1)->m_nXStart;
			nXEnd	= GetColumn (nEndCol)->m_nXEnd;
			return;
		}
		RadarColumn* p2 = GetColumn (nCol + 1);

		// Sposto le due colonne adiacenti all'interno dell'array
		m_GrpTable.SetAt (nCol, p2);
		m_GrpTable.SetAt (nCol + 1, p1);
		if (!bMoved)
		{
			// Memorizzo l'ultimo indice di colonna ai cui è avvenuto lo scambio 
			nEndCol	= nCol + 1;
			bMoved	= TRUE;
		}
	}
	ASSERT (nEndCol >= 0);
	UpdateColumnsStartEnd (0, TRUE);
	nXStart	= GetColumn (0)->m_nXStart;
	nXEnd	= GetColumn (nEndCol)->m_nXEnd;
}

// Ricerca nell'array dei record formattati la colonna su cui è posizionato il
// mouse. Se la ricerca ha esito positivo ritorna l'indice della colonna, altrimenti
// -1;
//-----------------------------------------------------------------------------
int	RadarBody::GetColumnIdxFromPoint (const CPoint& LPMousePos)
{
	int nNrCols = GetGrpTableNrCols() - 1;
	if (LPMousePos.x > GetColumn(nNrCols--)->m_nXEnd)
		return -1;

	for (int nCol = 0; nCol < nNrCols; nCol++)
		if (LPMousePos.x < GetColumn(nCol)->m_nXEnd)
			return nCol;
	return nNrCols;
}

// Ricerca nell'array dei record formattati la colonna su cui è posizionato il
// mouse. Se la ricerca ha esito positivo ritorna la colonna, altrimenti NULL.
//-----------------------------------------------------------------------------
RadarColumn* RadarBody::GetColumnFromPoint (const CPoint& LPMousePos)
{
	for (int nCol = 0; nCol < GetGrpTableNrCols(); nCol++)
		if (LPMousePos.x < GetColumn(nCol)->m_nXEnd)
			return GetColumn(nCol);
	return NULL;
}

// Rimuove le colonne selezionate dall'utente e le inserisce nell'array delle
// colonne cancellate.
//-----------------------------------------------------------------------------
void RadarBody::RemoveSelectedColumns (RadarColumnsIdxArray* pDeleteCols)
{
	ASSERT (m_nNrSelColumns != GetGrpTableNrCols() && m_nNrSelColumns);

	int nStartCol = -1;
	for (int nCol = 0; nCol < GetGrpTableNrCols(); /* must be empty */ )
	{
		RadarColumn* pCol = GetColumn (nCol);
		if (!pCol->m_bSelect)
		{
			nCol++;
			continue;
		}
		if (nStartCol < 0)
			nStartCol = nCol ? nCol - 1: nCol;

		// Aggiunge le informazioni sulla colonna nell'array delle colonne cancellate
		pDeleteCols->Add (pCol->GetTitle(), pCol->GetIdx());

		// Rimozione della colonna dall'array delle colonne
		pCol->RemoveAll();

		// Rimozione dell'intera colonna dall'array dei record formattati
		m_GrpTable.RemoveAt (nCol);
		if (--m_nNrSelColumns == 0)
			break;
	}
	m_bSameDataType			= FALSE;
	m_bSelColumnsContinue	= FALSE;
	UpdateColumnsStartEnd (nStartCol, TRUE);
}

//-----------------------------------------------------------------------------
DataType RadarBody::CtrlKeyDownEnable (int nCol)
{
	RadarColumn* pCol = GetColumn (nCol);
	// Deseleziona o seleziona la colonna a secondo del suo stato precedente
	pCol->m_bSelect = !pCol->m_bSelect;

	if (pCol->m_bSelect)
		m_nNrSelColumns++;
	else if (--m_nNrSelColumns == 0)
	{
		m_bSameDataType			= FALSE;
		m_bSelColumnsContinue	= FALSE;
		return DATA_NULL_TYPE;
	}
	return SetColumnsFlags();
}

//-----------------------------------------------------------------------------
DataType RadarBody::ShiftKeyEnable (int nCol, int& nXStart, int& nXEnd)
{
	if (m_nNrSelColumns == 0)
		return DATA_NULL_TYPE;

	int nIdxFirstSel= -1;
	int nIdxLastSel	= -1;

	// Calculates the first and last index of the selected columns
	for (int nIdx = 0; nIdx < GetGrpTableNrCols (); nIdx++)
		if (GetColumn(nIdx)->IsSelected())
		{
			if (nIdxFirstSel < 0)
			{
				// La colonna selezionata è la corrente, non devo fare niente
				if (nIdx == nCol)
					return DATA_NULL_TYPE;

				nIdxFirstSel = nIdx;
			}
			nIdxLastSel = nIdx;
		}
		else if (nIdxFirstSel >= 0)
			break;

	// if the selected column if the last do nothing
	if (nCol == nIdxLastSel)
		return DATA_NULL_TYPE;

	int nIdxStart, nIdxEnd, nInc;
	BOOL bSelect;
	if (nCol < nIdxFirstSel)
	{
		ASSERT (nIdxFirstSel > 0);
		nIdxStart	= nCol;
		nIdxEnd		= nIdxFirstSel - 1;
		nInc		= 1;
		bSelect		= TRUE;
	}
	else if	(nCol > nIdxLastSel)
	{
		nIdxStart	= nIdxLastSel + 1;
		nIdxEnd		= nCol;
		nInc		= 1;
		bSelect		= TRUE;
	}
	else
	{
		nIdxStart	= nCol + 1;
		nIdxEnd		= nIdxLastSel;
		nInc		= -1;
		bSelect		= FALSE;
	}
	nXStart	= GetColumn (nIdxStart)->	m_nXStart;
	nXEnd	= GetColumn (nIdxEnd)->		m_nXEnd;

	SelectedColumns (nIdxStart, nIdxEnd, nInc, bSelect);
	return SetColumnsFlags();
}

// Seleziona tutte le colonne
//-----------------------------------------------------------------------------
DataType RadarBody::SelectAllColumns (int& nXStart, int& nXEnd)
{
	nXStart	= -1;
	nXEnd	= -1;
	// Se tutte le colonne sono già selezionate esce
	if (m_nNrSelColumns == GetGrpTableNrCols())
		return DATA_NULL_TYPE;

	// Setta i flag di selezione
	m_nNrSelColumns = GetGrpTableNrCols();
	// Se tutte le colonne sono selezionate non posso spostare di posizione le colonne
	// per cui devo dire checla selezione non è continua
	m_bSelColumnsContinue = FALSE;

	m_bSameDataType	= TRUE;
	DataType nDataType = GetColumn(0)->m_DataType;
	
	for (int nCol = 0; nCol < m_nNrSelColumns; nCol++)
	{
		// Seleziona la colonna
		RadarColumn* pCol = GetColumn(nCol);
		if (!pCol->m_bSelect)
		{
			pCol->m_bSelect	= TRUE;
			if (pCol->m_nXEnd > nXEnd)
				nXEnd = pCol->m_nXEnd;
		}
		if (nXStart < 0)
			nXStart = pCol->m_nXStart;

		// Diverso Tipo, NON è possibile eseguira una find stupida
		if (m_bSameDataType && nDataType != pCol->m_DataType)
		{
			m_bSameDataType = FALSE;
			nDataType = DATA_NULL_TYPE;
		}
    }
    return nDataType;
}

// Deseleziona tutte le colonne precedentemente selezionate
//-----------------------------------------------------------------------------
void RadarBody::DeselectAllColumns (int& nXStart, int& nXEnd)
{
	nXStart = -1;
	nXEnd	= -1;
	// Se tutte le colonne sono deselezionate esce
	if (m_nNrSelColumns == 0)
		return;

	// Resetta i flag di selezione
	m_bSameDataType			= FALSE;
	m_bSelColumnsContinue	= FALSE;

	RadarColumn* pCol = NULL;
	for (int nCol = 0; nCol < GetGrpTableNrCols(); nCol++)
	{
		pCol = GetColumn(nCol);
		// Controlla se la colonna è selezionata
		if (!pCol->m_bSelect)
			continue;
    
    	if (nXStart < 0)
    		nXStart = pCol->m_nXStart;

		// Deseleziona la colonna
		pCol->m_bSelect = FALSE;
		// Non esistono più colonne selezionate interrompo la ricerca
		if (--m_nNrSelColumns == 0)
			break;
	}
	ASSERT_VALID (pCol);
	nXEnd = pCol->m_nXEnd;
}

// Assegna alla colonna identificata dall'indice nCol il nuovo valore della
// suo larghezza.
//-----------------------------------------------------------------------------
void RadarBody::SetColumnToWidthChar (int nCol, int nWidth, BOOL bUpdate/* = TRUE*/)
{
	RadarColumn* pCol = GetColumn(nCol);

	// Setta la larghezza in pixel
	pCol->SetScreenWidth(nWidth);

	// Aggiorna il titolo addattandolo al nuovo valore di larghezza
	pCol->CompressTitle (pCol->GetScreenWidth());

	// Aggiorna le coordinate iniziali e finali della colonna
	if (bUpdate)
	UpdateColumnsStartEnd (nCol);
}

// Calcola in base al numero di righe visibile nella view corrente il numero
// massimo di record da formattare.
//-----------------------------------------------------------------------------
void RadarBody::SetSizeGrpTable (int nVisibleRows)
{
	if (nVisibleRows > m_nNrMaxFormatRows)
		m_nNrMaxFormatRows = 2 * nVisibleRows;
}

// Ritorna il cursore da visualizzare in base alla corrente posizione del mouse
//-----------------------------------------------------------------------------
SelectCursor RadarBody::GetCursorAction (const CPoint& LPMousePos)
{
	RadarColumn* pCol;
	for (int nCol = 0; nCol < GetGrpTableNrCols(); nCol++)
	{
		pCol = GetColumn(nCol);

		if (LPMousePos.x < pCol->m_nXEndOffset)
			break;
		if (LPMousePos.x > (pCol->m_nXEnd + DEFAULT_COLUMN_OFFSET))
			continue;

		m_nSelColumn = nCol;
		return COL_RESIZE;
	}
	return ARROW;
}

// Modifica la dimensione delle colonna e aggiorna il titolo e le coordinate di
// partenza e di arrivo delle colonne successive. 
// Viene chiamata dal ridimensionamento da mouse.
//-----------------------------------------------------------------------------
void RadarBody::SetWidthSelColumn (int nWidth)
{
	// assign a new value of width and increase the total width of table
	ASSERT (m_nSelColumn >= 0);
	RadarColumn* pCol = GetColumn(m_nSelColumn);

	pCol->m_nScreenWidth += (nWidth - pCol->m_nXEnd);

	pCol->CompressTitle(pCol->GetScreenWidth());

	// updates the start and end coordinate
	UpdateColumnsStartEnd (m_nSelColumn);

	m_nSelColumn = -1;
}

// Aggiorna le coordinate di partenza e di arrivo di ogni singola colonna.
// L'aggiornamento parte dalla colonna nFromCol.
//-----------------------------------------------------------------------------
void RadarBody::UpdateColumnsStartEnd (int nFromCol, BOOL bResetStartFirstCol/*= FALSE*/)
{
	ASSERT (nFromCol >= 0 && m_GrpTable.GetSize());
	RadarColumn* pCol = GetColumn(nFromCol);
	if (nFromCol == 0 && bResetStartFirstCol)
	{
		pCol->m_nXStart			= 0;
		pCol->m_nXStartOffset	= DEFAULT_COLUMN_OFFSET;
	}

	pCol->m_nXEnd		= pCol->m_nXStart + pCol->GetScreenWidth();
	pCol->m_nXEndOffset	= pCol->m_nXEnd - DEFAULT_COLUMN_OFFSET;
	int nXPrecColEnd	= pCol->m_nXEnd;

	// Aggiornamento dei valori di partenza e di arrivo sia per le righe di separazione
	// tra le colonne che per il testo da visualizzare
	for (nFromCol++; nFromCol < GetGrpTableNrCols(); nFromCol++)
	{
		RadarColumn* pCol = GetColumn(nFromCol);

		pCol->m_nXStart			= nXPrecColEnd + 1;
		pCol->m_nXStartOffset	= pCol->m_nXStart + DEFAULT_COLUMN_OFFSET;
		pCol->m_nXEnd			= pCol->m_nXStart + pCol->GetScreenWidth();
		pCol->m_nXEndOffset		= pCol->m_nXEnd - DEFAULT_COLUMN_OFFSET;

		nXPrecColEnd			= pCol->m_nXEnd;
	}
}

// Ricerca una colonna in base alla posizione del mouse
//-----------------------------------------------------------------------------
int RadarBody::FindColumn(CPoint& LPMousePos)
{
	int	nCol	= 0;
	int nNumCol	= GetGrpTableNrCols();
	RadarColumn* pCol = GetColumn (nCol);

	while (LPMousePos.x > pCol->m_nXEnd)
	{
		if (++nCol >= nNumCol)
			return -1;

		pCol = GetColumn (nCol);
	}	 
	return nCol;
}

// In base al punto passato come argomento individuo la colonna da prendere in 
// esame
//-----------------------------------------------------------------------------
DataType RadarBody::SelectColumn (int nCol, int& nXStart, int& nXEnd)
{
	RadarColumn* pCol = GetColumn (nCol);
	
	// Se la colonna era già selezionata non devo fare niente
	//if (pCol->m_bSelect)
	//	return DATA_NULL_TYPE;
	BOOL bToggleSelection = pCol->m_bSelect;

	// Deseleziono le eventuali colonne selezionate e seleziono la nuova
	DeselectAllColumns(nXStart, nXEnd);

	// Aggiorno gli eventuali estremi di invalidazione
	if (nXStart > pCol->m_nXStart)
		nXStart = pCol->m_nXStart;
	if (nXEnd	< pCol->m_nXEnd)
		nXEnd = pCol->m_nXEnd;

	if (bToggleSelection)
	{
		return DATA_NULL_TYPE;
	}

	pCol->m_bSelect = TRUE;

	m_nNrSelColumns			= 1;
	m_bSameDataType			= TRUE;
	m_bSelColumnsContinue	= TRUE;
	return pCol->m_DataType;
}

//-----------------------------------------------------------------------------
void RadarBody::SetFirstIdxGrpRowFromEnd(int nVisibleRows)
{
	m_nFirstIdxGrpRow = GetGrpTableNrRows() - GetCorrectVisibleRows(nVisibleRows);
	if (m_nFirstIdxGrpRow < 0)
		m_nFirstIdxGrpRow =0;
}

//-----------------------------------------------------------------------------
void RadarBody::SetActiveAndFirstIdxGrpRow (ActiveAndFirst nAction, int nVisibleRows, int nRec)
{
	switch (nAction)
	{
		case MULTI_LINES:
			ASSERT (m_nActiveGrpRow >= m_nFirstIdxGrpRow);
			m_nFirstIdxGrpRow = m_nActiveGrpRow - GetCorrectVisibleRows(nVisibleRows) + 1;
			break;

		case START_FROM_TOP:
			m_nActiveGrpRow		= 0;
			m_nFirstIdxGrpRow	= 0;
			break;

		case START_FROM_BOTTOM:
			m_nActiveGrpRow = GetGrpTableNrRows() - 1;
			SetFirstIdxGrpRowFromEnd (nVisibleRows);
			break;

		case FROM_FIND_NEXT:
			m_nActiveGrpRow = nRec;
			if (m_nActiveGrpRow >= GetLastIdxGrpRow(nVisibleRows))
				m_nFirstIdxGrpRow = m_nActiveGrpRow - GetCorrectVisibleRows(nVisibleRows) + 1;
			break;

		case FROM_FIND_PREV:
			m_nActiveGrpRow	= nRec;
			if (m_nActiveGrpRow < m_nFirstIdxGrpRow)
				m_nFirstIdxGrpRow = m_nActiveGrpRow;
			break;

		case FROM_SHOW_ROW:
			SetFirstIdxGrpRowFromEnd (nVisibleRows);
			if (m_nActiveGrpRow < m_nFirstIdxGrpRow)
				m_nFirstIdxGrpRow = m_nActiveGrpRow;
			break;
	}
	ASSERT (m_nActiveGrpRow		>= 0);
	ASSERT (m_nFirstIdxGrpRow	>= 0);
}

// Aggiorna l'indice della prima riga a schermo attualemente visualizzata in
// accordo con la riga attiva. La funzione ritorna TRUE se l'operzaione è stata
// eseguita nel rango delle righe visibili, altrimenti ritorna FALSE.
// Viene richiamata esclusivamente dal metodo id OnSize dell CRadarView
//-----------------------------------------------------------------------------
BOOL RadarBody::IsInsideVisibleRowsRange
	(
		int		nFirstIdxSqlRec,
		int		nSizeCircleList,
		int		nVisibleRows,
		BOOL	bNextDir
	)
{
	if (m_nFirstIdxGrpRow > m_nActiveGrpRow)
	{
		m_nFirstIdxGrpRow = m_nActiveGrpRow;
		return TRUE;
	}
	if (m_nActiveGrpRow < GetLastIdxGrpRow (nVisibleRows) || (m_nActiveGrpRow + nFirstIdxSqlRec) >= nSizeCircleList)
	{
		m_nFirstIdxGrpRow = m_nActiveGrpRow - GetCorrectVisibleRows(nVisibleRows) + 1;
		if (m_nFirstIdxGrpRow < 0)
			m_nFirstIdxGrpRow = 0;
		return TRUE;
	}
	int nLastRow = m_nFirstIdxGrpRow + nVisibleRows;
	if (nLastRow <= GetGrpTableNrRows() || bNextDir)
		return FALSE;

	int nFirstRow		= m_nFirstIdxGrpRow - (nLastRow - GetGrpTableNrRows());
	m_nFirstIdxGrpRow	= nFirstRow >= 0 ? nFirstRow : 0;
	return TRUE;
}

// Calcola l'indice della nuova riga attiva a frointe di una tasto di pagina 'giù'
//-----------------------------------------------------------------------------
int RadarBody::GetIdxGrpRowNextPage(int nVisibleRow)
{
	int nLastRow = GetLastIdxGrpRow(nVisibleRow) - 1;
	if (nLastRow < 0)
		return 0;

	return m_nActiveGrpRow == nLastRow ? nLastRow + GetCorrectVisibleRows(nVisibleRow) - 1 : nLastRow;
}

//-----------------------------------------------------------------------------
int RadarBody::GetIdxGrpRowPrevPage(int nVisibleRow)
{
	int nRow = m_nFirstIdxGrpRow;
	if (m_nActiveGrpRow == m_nFirstIdxGrpRow)
		nRow -= (GetCorrectVisibleRows(nVisibleRow) - 1);

	return nRow;
}

//-----------------------------------------------------------------------------
void RadarBody::SetCorrectCharWidth (int nViewWidth, int nTLBWidth)
{
	m_nCorrectCharWidth = nViewWidth > nTLBWidth ? nViewWidth : nTLBWidth;
}

// Salva gli attributi su esplicita richiesta dell'utente
//-----------------------------------------------------------------------------
void RadarBody::WriteColumnsConfig (LPCTSTR lpszSection, LPCTSTR lpszRadarFile)
{
	ASSERT (lpszSection);
	ASSERT (lpszRadarFile);
	CString strWidth;
	// Salvo quindi l'attuale configurazione grafica delle colonne
	int nc = m_pRadar->m_pCallerSqlRecord->GetSize();
	for (int nCol = 0; nCol < GetGrpTableNrCols(); nCol++)
	{
		RadarColumn* pCol = GetColumn(nCol);

		strWidth = cwsprintf (_T("%d,%.3lf"), nCol, LPtoMU (pCol->GetScreenWidth()));

		::WritePrivateProfileString (lpszSection, pCol->GetIdx() < nc ? pCol->GetName() : pCol->GetQualifiedName(), strWidth, lpszRadarFile);
	}
}

// Salva gli attributi che vengono salvati senza richiedere conferma all'utente 
//-----------------------------------------------------------------------------
BOOL RadarBody::ReadColumnsConfig
	(
		LPCTSTR					lpszSection,
		LPCTSTR					lpszRadarFile,
		SqlRecord*				pSqlRec,
		RadarColumnsIdxArray*	pDeletedCols
	)
{
	ASSERT (lpszSection);
	ASSERT (lpszRadarFile);
	ASSERT (pSqlRec);
	ASSERT_VALID (pDeletedCols);

	// Caricamento degli attributi grafici di ogni singola colonna
	CString strBuffer;
	TCHAR* pszValue = strBuffer.GetBuffer (4096);
	int sz = pSqlRec->GetSizeEx();
	for (int nIdx = 0; nIdx < sz; nIdx++)
	{
		const SqlColumnInfo* pSqlColInfo = pSqlRec->GetColumnInfo(nIdx);
		// Se è un campo locale non devo prenderlo in considerazione
		if (pSqlColInfo->m_bVirtual)
			continue;

		CString sColName = pSqlRec->GetQualifiedColumnName(pSqlColInfo);

		SqlBindingElem* pAcc = m_pRadar->m_pTable->m_pColumnArray->GetParamByName(sColName);
		if (!pAcc)
			continue;

		// Se la colonna non viene trovata nel file di configurazione allora la inserisco nell'array
		// delle colonne cancellate
		if (!::GetPrivateProfileString (lpszSection, nIdx < pSqlRec->GetSize()  ? pSqlColInfo->GetColumnName() : sColName, _T(""), pszValue, 1024, lpszRadarFile))
		{
			pDeletedCols->Add (pSqlColInfo->GetColumnTitle (), nIdx);
			continue;
		}
		// Divisione della stringa letta nel file .INI
		TCHAR* nextToken;
		TCHAR* pszOrder			= _tcstok_s(pszValue,	_T(","), &nextToken);
		TCHAR* pszScreenWidth	= _tcstok_s(NULL,		_T(","), &nextToken);
		// Se la colonna non è presente la inserisco nell'array delle colonne cancellate
		if (!pszOrder || !pszScreenWidth)
		{
			AfxGetDiagnostic()->Add (cwsprintf	(_TB("Graphic attributes in the column '{0-%s}' are missing or not valid. Current values: column width (pixel): {1-%s}\r\norder:{2-%s}. The column will be added to the list of deleted columns."), (LPCTSTR) sColName, pszValue, pszScreenWidth, pszOrder));
			pDeletedCols->Add (pSqlColInfo->GetColumnTitle (), nIdx);
			continue;
		}
		// Inserisco nella posizione indicata da pszOrder la colonna nel rispettivo array 
		int pos = _ttoi (pszOrder);
		SetColumnAt (pos, nIdx, pSqlColInfo, CheckColumnWidth (pSqlColInfo), sColName, NULL);

		if (this->m_pRadar->GetRadarView()->GetRadVersion() == RAD_VERSION)
		{
			int width = MUtoLP(_tstof (pszScreenWidth));
			if (width > 0)
				GetColumn(pos) ->SetScreenWidth(width);
		}
	}
	strBuffer.ReleaseBuffer ();
	// Verifico che almeno una colonna è stata caricata
	if (!GetGrpTableNrCols())
		return FALSE;

	// Elimino gli oggetti vuoti presenti nell'array
	for (int nCol = 0; nCol < m_GrpTable.GetSize(); nCol++)
		if (!m_GrpTable[nCol])
			m_GrpTable.RemoveAt (nCol--);
	return TRUE;
}

//-----------------------------------------------------------------------------
BOOL RadarBody::IncreaseMultiLines	()
{
	if (!CanDoIncreaseMultiLines ())
		return FALSE;

	m_nNrMultiLines++;
	return TRUE;
}

//-----------------------------------------------------------------------------
BOOL RadarBody::DecreaseMultiLines	()
{
	if (!CanDoDecreaseMultiLines ())
		return FALSE;

	m_nNrMultiLines--;
	return TRUE;
}

//-----------------------------------------------------------------------------
void RadarBody::SetNrMultiLines (int nNrMultiLines)
{
	m_nNrMultiLines = nNrMultiLines > MULTI_LINES_MAX || nNrMultiLines < MULTI_LINES_MIN
						? MULTI_LINES_MIN
						: nNrMultiLines;
}

//-----------------------------------------------------------------------------
void RadarBody::UpdateMultiLinesFlag ()
{
	for (int nCol = 0; nCol < GetGrpTableNrCols(); nCol++)
		GetColumn (nCol)->UpdateMultiLinesFlag (m_nNrMultiLines);
}

// Aggiorna le colonne selezionate o tutte le colonne con i valori di larghezza
// definiti nell'array.
//-----------------------------------------------------------------------------
void RadarBody::UpdateColumnsWidth (BOOL bAllCols, RadarIntArray* pWidthCols)
{
	ASSERT_VALID (pWidthCols);
	int nCharWidth = GetCorrectCharWidth ();
	int nStartCol = -1;

	for (int nCol = 0; nCol < GetGrpTableNrCols(); nCol++)
		if (bAllCols || GetColumn(nCol)->IsSelected())
		{
			if (nStartCol == -1) nStartCol = nCol;

			SetColumnToWidthChar (nCol, nCharWidth * pWidthCols->GetInt (nCol), FALSE);
		}
	if (nStartCol > -1)
		UpdateColumnsStartEnd (nStartCol);
}

// Aggiorna le colonne selezionate o tutte le colonne 
// con i valori di larghezza indicato
//-----------------------------------------------------------------------------
void RadarBody::UpdateColumnsWidth (BOOL bAllCols, int nWidth)
{
	ASSERT (nWidth > 0);
	nWidth = nWidth * GetCorrectCharWidth ();
	int nStartCol = -1;

	for (int nCol = 0; nCol < GetGrpTableNrCols(); nCol++)
		if (bAllCols || GetColumn(nCol)->IsSelected())
		{
			if (nStartCol == -1) nStartCol = nCol;

			SetColumnToWidthChar (nCol, nWidth, FALSE);
		}
	if (nStartCol > -1)
		UpdateColumnsStartEnd (nStartCol);
}

// Riempie il vettore con il nome delle colonne 
//-----------------------------------------------------------------------------
void RadarBody::GelAllSelColumnsName (CStringArray& fieldArray)
{
	int nNrSelCol = GetNrSelColumns ();
	ASSERT (nNrSelCol > 0);
	for (int nCol = 0; nCol < GetGrpTableNrCols() && nNrSelCol > 0; nCol++)
	{
		RadarColumn* pCol = GetColumn(nCol);
		// Controlla se la colonna è selezionata
		if (!pCol->IsSelected ())
			continue;

		fieldArray.Add (pCol->GetQualifiedName ());
		nNrSelCol--;
	}
}

///////////////////////////////////////////////////////////////////////////////
// Diagnostics
#ifdef _DEBUG
void RadarBody::Dump (CDumpContext& dc) const
{
	ASSERT_VALID (this);
	AFX_DUMP0(dc, " RadarBody\n");
	dc << "\tm_FirstIdxGrpRow = " << m_nFirstIdxGrpRow << "\n";
	CObject::Dump(dc);
}
#endif // _DEBUG
