#pragma once

#include "beginh.dex"

// sizes for the grid cache and the RecordArray buffer
LONG const CACHE_PAGE_COUNT = 4;
LONG const CACHE_PAGE_SIZE = 50;
LONG const BUFFER_SIZE = CACHE_PAGE_COUNT * CACHE_PAGE_SIZE;

//////////////////////////////////////////////////////////////////////////////
//       			class DataPickerRecordSet
//////////////////////////////////////////////////////////////////////////////
//
//=============================================================================
class TB_EXPORT DataPickerRecordSet : public RecordArray
{ 
	DECLARE_DYNAMIC(DataPickerRecordSet)

public:
	DataPickerRecordSet
		(
			CRuntimeClass*		pClass, 
			CAbstractFormDoc*	pDocument
		);
	virtual ~DataPickerRecordSet();

	virtual SqlRecord*	GetPrototype		()	{ return m_pRecord; }
	virtual SqlRecord*	GetRecord			()	{ return m_pRecord; }

public:
	virtual int			GetSize					()									{ return m_nExtractedRows; }
	virtual int			GetUpperBound			()									{ return RecordArray::GetUpperBound(); }
	virtual	SqlRecord*	GetRow					(int nRow)							{ return RecordArray::GetAt(nRow); }
	virtual void		Execute					(BOOL bPreselect);
	virtual	BOOL		FindData				(LONG nRow);
	virtual	SqlRecord*	GetVirtualRow			(LONG nRow);
	virtual BOOL		SelectDeselectAllLines	();
	virtual LONG		GetFirstBufferedRowNo	()									{ return max(0, m_nLastBufferedRow - BUFFER_SIZE + 1);}
	virtual LONG		GetLastBufferedRowNo	()									{ return m_nLastBufferedRow;}
	virtual void		ChangeStatus			(int nRow, BOOL bSelected = FALSE);
	virtual void		ClearGrid				();

	virtual void		GetSelectedItems		(DataObjArray& selectedItems, DataObjArray& unselectedItems);
	virtual void		PreselectItems			(DataObjArray& selectedItems, DataObjArray& unselectedItems)		{};
	virtual void		SetPreselectedItems		(DataObjArray& selectedItems, DataObjArray& unselectedItems);

private:
	HotFilterDataPicker*	GetDocument			()					const	{ return (HotFilterDataPicker*) m_pDocument;	}
	BOOL					Open				();
	void					Close				();
	void					OnDefineQuery		();
	void					OnPrepareQuery		();
	void					OnPrepareAuxColumns	(SqlRecord* pRec);
	BOOL					FindFirstData		();
	void					SetRecord			(int nPos);
	BOOL					FindDataDown		(LONG nRow);
	BOOL					FindDataUp			(LONG nRow);
	void					Init				();

private:
	SqlTable*			m_pTable;
	SqlRecord*			m_pRecord;
	CAbstractFormDoc*	m_pDocument;
	LONG				m_nExtractedRows;
	LONG				m_nLastBufferedRow;
	LONG				m_nCurrentCursorPos;
	DataObjArray		m_SelectedItems;		// list of selected after unselect all
	DataObjArray		m_UnselectedItems;		// list of unselected after select all
};
