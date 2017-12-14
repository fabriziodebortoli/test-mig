
#pragma once

#include <TbGeneric\DataObj.h>

#include "radardef.h"

//includere alla fine degli include del .H
#include "beginh.dex"

class DataObj;

class CRadarDoc;

class SqlColumnInfo;
class SqlRecord;
class CPrivacyFormatter;

//	Definizione di RadarCell
//=============================================================================
class TB_EXPORT RadarCell : public CObject
{
public:
	CString	m_szValue;
	BOOL	m_bMultiLines;

public:
	RadarCell (LPCTSTR szValue, BOOL bMultiLines = FALSE);

// Diagnostics
#ifdef _DEBUG
public:
	void Dump		(CDumpContext&)	const;
	void AssertValid()				const { CObject::AssertValid(); }
#endif // _DEBUG
};

//	Definizione di RadarColumn
//=============================================================================
class TB_EXPORT RadarColumn : public Array
{
	friend class RadarBody;
protected:
	RadarBody*	m_pBody;
	BOOL m_bSelect;

	// column attributes
	DataType	m_DataType;

	// Indice dello stile di formattazione dall'interno della tabella degli stili di formattazione
	int m_nIdxFmtStyle;
	int	m_nIdx;
	int	m_nStartIdxSetChars;

	int	m_nLen;
	int	m_nScreenWidth;

	CPrivacyFormatter* m_pPrivacyFormatter;  //serve per visualizzare un dato privato (vedi RowSecurity e OSL) che l'utente corrente non può visualizzare

	// title and field name
	CString	m_szName;
	CString	m_szOrigTitle;
	CString	m_szTitle;
	CString	m_szQualifiedName;

public:
	// drawing
	UINT m_nAlign;

	int m_nXStart;
	int m_nXStartOffset;
	int m_nXEnd;
	int m_nXEndOffset;

public:
	RadarColumn	(RadarBody*	pBody, int nIdx, const SqlColumnInfo*, int nWidth, LPCTSTR szQualifiedName, LPCTSTR lpszFmtStyle/* = NULL*/);
	~RadarColumn();

public:
	// Aggiornano il contenuto della cella
	void AddData		(DataObj*, int nNrMultiLines);
	void InsertDataAt	(int nRow, DataObj*, int nNrMultiLines);
	void SetDataAt		(int nRow, DataObj*, int nNrMultiLines);
	BOOL GetString		(int nRow, CString&);

	void UpdateMultiLinesFlag (int nNrMultiLines);
	int  CalcScreenWidth (CRadarDoc* pDoc, CDC* pDC);

public:
	// Gestiscono la formattazzione
	void SetFormatIdx (LPCTSTR lpszFmtStyle = NULL );

public:
	// generic funtions
	void CompressTitle (int nWidth);

public:
	BOOL		FormatString (DataObj*, CString& strFmt, int nNrMultiLines = -1);
	DataType	GetDataType		();
	int			GetIdx			();
	LPCTSTR		GetName			();
	LPCTSTR		GetQualifiedName ();
	LPCTSTR		GetTitle		();
	LPCTSTR		GetOriginalTitle();

	int			GetLen			();
	int			GetScreenWidth	();
	void		SetScreenWidth	(int width);

	BOOL		IsSelected		();
	int			GetIdxFmtStyle	();

private:
	int		Add		(RadarCell*);
	void	InsertAt(int, RadarCell*);
	void	SetAt	(int, RadarCell*);

	RadarCell* GetAt(int nRow);

	BOOL CheckIsMultiLines (CString strFmt, int nNrMultiLines);

// Diagnostics
#ifdef _DEBUG
public:
	void Dump		(CDumpContext&)	const;
	void AssertValid()				const { Array::AssertValid(); }
#endif // _DEBUG
};

//	Definizione di RadarBody
//=============================================================================
class TB_EXPORT RadarBody : public CObject
{
	friend class CRadarDoc;
private:
	CRadarDoc*	m_pRadar;

	BOOL m_bSameDataType;
    BOOL m_bSelColumnsContinue;

	int	m_nSelColumn;
	int m_nCorrectCharWidth;
	int	m_nNrMultiLines;

	Array m_GrpTable;

protected:
	int m_nNrSelColumns;

	int	m_nActiveGrpRow;
	int	m_nFirstIdxGrpRow;
	int	m_nNrMaxFormatRows;

public:
	int m_nFirstVisibleRow;

public:
	RadarBody (CRadarDoc* pRadar);
	~RadarBody();

public:
	RadarColumn*	GetColumn				(int nCol);
	RadarColumn*	GetColumnFromPoint		(const CPoint&);
	int				GetColumnIdxFromPoint	(const CPoint&);
	SelectCursor	GetCursorAction			(const CPoint&);

	int	GetActiveGrpRow		();
	int	GetFirstIdxGrpRow	();
	int	GetLastIdxGrpRow	(int nVisibleRow);
	int	GetNrSelColumns		();
	int	GetGrpTableNrRows	();
	int	GetGrpTableNrCols	();
	int	GetIdxGrpRowNextPage(int nVisibleRow);
	int	GetIdxGrpRowPrevPage(int nVisibleRow);
	int GetCorrectCharWidth ();
	int GetCorrectVisibleRows(int nVisibleRow);

	int	GetXStartSelColumn	();
	int	GetXEndSelColumn	();

	// columns operations (delete column/s)
	void RemoveSelectedColumns	(RadarColumnsIdxArray*);

	// columns operations (set column graphic attrib)
	void SetColumnToWidthChar	(int nCol, int nWidth, BOOL bUpdate = TRUE);
	void SetWidthSelColumn		(int nWidth);
	void UpdateColumnsStartEnd	(int nStartCol, BOOL bResetStartFirstCol = FALSE);
	void UpdateColumnsWidth		(BOOL bAllCols, RadarIntArray* pWidthCols);
	void UpdateColumnsWidth		(BOOL bAllCols, int nWidth);
	void UpdateColumnsWidthOnFonts		(CRadarDoc* pDoc, int startCol = 0);

	// columns operations (select column/s)
	void SetSizeGrpTable		(int nVisibleRows);

	void SetCorrectCharWidth		(int nViewWidth, int nTLBWidth);
	void SetFirstIdxGrpRowFromEnd	(int nVisibleRows);
	void SetActiveAndFirstIdxGrpRow (ActiveAndFirst, int nVisibleRows, int nRec = 0);
	BOOL IsInsideVisibleRowsRange	(
										int nFirstIdxSqlRec,
										int nSizeCircleList,
										int nVisibleRows,
										BOOL nNextDir
									);

public:
	void WriteColumnsConfig	(LPCTSTR lpszSection, LPCTSTR lpszRadarFile);
	BOOL ReadColumnsConfig	(LPCTSTR lpszSection, LPCTSTR lpszRadarFile, SqlRecord*, RadarColumnsIdxArray*);

public:
	int FindColumn(CPoint& LPMousePos);

	void DeselectAllColumns	(int& nXStart, int& nXEnd);

	DataType CtrlKeyDownEnable	(int nCol);
	DataType ShiftKeyEnable		(int nCol, int& nXStart, int& nXEnd);
	DataType SelectColumn		(int nCol, int& nXStart, int& nXEnd);
	DataType SelectAllColumns	(int& nXStart, int& nXEnd);

public:
	BOOL IncreaseMultiLines	();
	BOOL DecreaseMultiLines	();

	BOOL CanDoIncreaseMultiLines();
	BOOL CanDoDecreaseMultiLines();

	int		GetNrMultiLines 	();
	void	SetNrMultiLines		(int nNrMultiLines);

	void UpdateOnlyStrFields (SqlRecord*, int nRow);

private:
	int		CheckColumnWidth	(const SqlColumnInfo*);
	BOOL	CanDoShiftColumns	(int nCol);

	DataType	SetColumnsFlags	();
	void 		SelectedColumns(int nIdxStart, int nIdxEnd, int nInc, BOOL bSelect);

protected:
	void GelAllSelColumnsName (CStringArray&);

	// Aggiungono e inseriscono le informazioni relative ad una singola colonna nell'array
	int		AddColumn	(int nColIdx, const SqlColumnInfo*, LPCTSTR szQualifiedName);
	int		InsertColumn(int nPos, int nColIdx, const SqlColumnInfo*, LPCTSTR szQualifiedName);
	void	SetColumnAt	(int nPos, int nColIdx, const SqlColumnInfo*, int nWidth, LPCTSTR szQualifiedName, LPCTSTR lpszFmtStyle/* = NULL*/);
	
	int		FindColumn		(const CString& sQualifiedName);
	BOOL	MoveColumn		(int nFrom, int nTo);
	BOOL	SetColumnTitle	(int nIdxCol, LPCTSTR szTitle);
	BOOL	SetColumnAlign	(int nIdxCol, UINT align);	//DT_LEFT, DT_CENTER, DT_RIGHT
	BOOL	RemoveColumn	(int nIndex);

	// Aggiungono e inseriscono un DatoObj nella colonna corrispondente
	void AddDataToColumn (RadarColumn*, DataObj*);

	// Aggiungono un SqlRecord nell'array di record formattati
	void AddRowGrpTable		(SqlRecord*);
	void AddRowHeadGrpTable	(SqlRecord*);
	void AddRowTailGrpTable	(SqlRecord*);

protected:
	// Funzione utilizzate dal CRadarDoc per abilitare alcune funzionalità grafiche e di ricerca
	BOOL CanDoEnableDeleteColumns	();
	BOOL CanDoEnableLeftShift		();
	BOOL CanDoEnableRightShift		();
	BOOL CanDoEnableFindStupid		();

	void ReformatColumn			(RadarColumn*, DataObj*, int nInsertIdx);
	void UpdateMultiLinesFlag	();

	// rows operation
	void RemoveAllRows	();
	void RemoveRow		(int nRow);

	// columns operation
	void ShiftColumnLeft	(int& nXStart, int& nXEnd);
	void ShiftColumnRight	(int& nXStart, int& nXEnd);

// Diagnostics
#ifdef _DEBUG
public:
	void Dump		(CDumpContext&)	const;
	void AssertValid()				const { CObject::AssertValid(); }
#endif // _DEBUG
};

///////////////////////////////////////////////////////////////////////////////
///////////////////////////////////////////////////////////////////////////////
//
//	Funzioni Inline
//
///////////////////////////////////////////////////////////////////////////////
///////////////////////////////////////////////////////////////////////////////
inline DataType	RadarColumn::GetDataType()	{ return m_DataType;}

inline int RadarColumn::GetIdx			()	{ return m_nIdx;	}
inline int RadarColumn::GetIdxFmtStyle	()	{ return m_nIdxFmtStyle;	}

inline int RadarColumn::GetLen				()	{ return m_nLen;	}
inline int RadarColumn::GetScreenWidth		()	{ return m_nScreenWidth;	}
inline void RadarColumn::SetScreenWidth		(int width)	{ m_nScreenWidth = width; }

inline int RadarColumn::Add		(RadarCell* pCell)	{ ASSERT_VALID (pCell); return Array::Add (pCell);} 

inline LPCTSTR RadarColumn::GetName			()	{ return m_szName;	}
inline LPCTSTR RadarColumn::GetTitle		()	{ return m_szTitle;	}
inline LPCTSTR RadarColumn::GetOriginalTitle()	{ return m_szOrigTitle;	}
inline LPCTSTR RadarColumn::GetQualifiedName()	{ return m_szQualifiedName;	}

inline BOOL RadarColumn::IsSelected	()	{ return m_bSelect;	}

inline void	RadarColumn::InsertAt	(int nRow, RadarCell* pCell) { ASSERT_VALID (pCell); Array::InsertAt(nRow, pCell); } 
inline void	RadarColumn::SetAt		(int nRow, RadarCell* pCell) { ASSERT_VALID (pCell); Array::SetAt	(nRow, pCell); } 

//-----------------------------------------------------------------------------
inline int RadarBody::GetCorrectCharWidth(){ return m_nCorrectCharWidth;		}
inline int RadarBody::GetNrSelColumns	() { return m_nNrSelColumns;			}
inline int RadarBody::GetGrpTableNrRows	() { return GetColumn(0)->GetSize();	}
inline int RadarBody::GetGrpTableNrCols	() { return m_GrpTable.GetSize();		}
inline int RadarBody::GetActiveGrpRow	() { return m_nActiveGrpRow;			}
inline int RadarBody::GetFirstIdxGrpRow	() { return m_nFirstIdxGrpRow;			}
inline int RadarBody::GetXStartSelColumn() { return GetColumn(m_nSelColumn)->m_nXStart; }
inline int RadarBody::GetXEndSelColumn	() { return GetColumn(m_nSelColumn)->m_nXEnd; }
inline int RadarBody::GetNrMultiLines 	() { return m_nNrMultiLines; }

inline int RadarBody::GetLastIdxGrpRow	(int nVisibleRows)
	{ return m_nFirstIdxGrpRow + GetCorrectVisibleRows(nVisibleRows); }
inline int RadarBody::GetCorrectVisibleRows(int nVisibleRows)
	{ return  nVisibleRows < GetGrpTableNrRows() ? nVisibleRows : GetGrpTableNrRows();}

inline BOOL RadarBody::CanDoEnableLeftShift		() { return CanDoShiftColumns (0); } 
inline BOOL RadarBody::CanDoEnableRightShift	() { return CanDoShiftColumns (GetGrpTableNrCols() - 1); }
inline BOOL RadarBody::CanDoEnableFindStupid	() { return m_bSameDataType && GetGrpTableNrCols(); }
inline BOOL RadarBody::CanDoIncreaseMultiLines	() { return m_nNrMultiLines < MULTI_LINES_MAX ;}
inline BOOL RadarBody::CanDoDecreaseMultiLines	() { return m_nNrMultiLines > MULTI_LINES_MIN ;}
inline BOOL RadarBody::CanDoShiftColumns		(int nCol)
	{ return GetGrpTableNrCols() > 1 && !GetColumn(nCol)->IsSelected() &&  m_bSelColumnsContinue; } 
inline BOOL RadarBody::CanDoEnableDeleteColumns	()
	{ return GetGrpTableNrCols() && GetNrSelColumns() && GetNrSelColumns() != GetGrpTableNrCols();}

inline void RadarBody::AddDataToColumn (RadarColumn* pCol, DataObj* pData)
	{ ASSERT_VALID (pCol); pCol->AddData (pData, m_nNrMultiLines); }

inline RadarColumn* RadarBody::GetColumn (int nCol)
	{ ASSERT_VALID ((RadarColumn *) m_GrpTable[nCol]); return (RadarColumn *) m_GrpTable[nCol];}


#include "endh.dex"
