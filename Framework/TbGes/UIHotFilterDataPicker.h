
#pragma once

#include <TbGeneric\dibitmap.h>
#include "TileDialog.h"
#include "ExtDocView.h"
#include "ExtDocFrame.h"

//#include "HotFilterQueryParser.h"
#include "TBGridControl.h"
#include "HotFilterDataPicker.h"
#include "UIHotFilterDataPicker.hjson"

#include "beginh.dex"

/////////////////////////////////////////////////////////////////////////////
//					CHotFilterDataPickerTBGrid 
/////////////////////////////////////////////////////////////////////////////

//============================================================================
class TB_EXPORT CHotFilterDataPickerTBGrid : public CTBGridControl
{
	DECLARE_DYNCREATE (CHotFilterDataPickerTBGrid)

public:
	CHotFilterDataPickerTBGrid(const CString sName = _T(""));
	~CHotFilterDataPickerTBGrid(void);

public:
	virtual	void	Customize		();

	void Attach				(DataPickerRecordSet* pRecordSet) { m_pRecordSet = pRecordSet; }
	void CreateGridLayout	();

	void SetRowsCount	(int nRows);
	void ClearRowsCount	();

protected:
	virtual BOOL			IsColumnReadOnly	(int nColumn);
	virtual void			OnHeaderColumnClick (int nColumn);
	virtual void			OnItemChanged		(CBCGPGridItem* pItem, int nRow, int nColumn);
	virtual void			SetCurrentRecord	(int nRow);

private:
	DataPickerRecordSet*	m_pRecordSet; 

protected:
	//{{AFX_MSG( CHotFilterDataPickerTBGrid )
	//}}AFX_MSG
	DECLARE_MESSAGE_MAP()
};

/////////////////////////////////////////////////////////////////////////////
//				CHotFilterDataPickerResultsTBGridTileDlg 
/////////////////////////////////////////////////////////////////////////////
//
class TB_EXPORT CHotFilterDataPickerResultsTBGridTileDlg : public CTileDialog
{
	DECLARE_DYNCREATE(CHotFilterDataPickerResultsTBGridTileDlg)

public:
	CHotFilterDataPickerResultsTBGridTileDlg();
	virtual ~CHotFilterDataPickerResultsTBGridTileDlg();
	
public:
	virtual	void	BuildDataControlLinks	();
	virtual BOOL	OnPrepareAuxData		();

public:
	HotFilterDataPicker*	GetDocument	() const { return (HotFilterDataPicker*) m_pDocument; }

protected:
	DECLARE_MESSAGE_MAP()
};

//=============================================================================
class TB_EXPORT CHotFilterDataPickerView : public CMasterFormView
{
	DECLARE_DYNCREATE(CHotFilterDataPickerView)

public:	
	CHotFilterDataPickerView();
	
public:	
	HotFilterDataPicker* GetDocument	() const;

public:	
	virtual	void BuildDataControlLinks	();
	virtual BOOL OnPrepareAuxData		();

protected:
	//{{AFX_MSG(CHotFilterDataPickerView)
	//}}AFX_MSG
	DECLARE_MESSAGE_MAP()
};

//=============================================================================
class TB_EXPORT CHotFilterDataPickerFrame : public CBatchFrame
{ 
	DECLARE_DYNCREATE(CHotFilterDataPickerFrame)
	
protected:
    CHotFilterDataPickerFrame();

public:
	virtual BOOL OnCustomizeTabbedToolBar	(CTBTabbedToolbar*	pTabbedBar);
	DECLARE_MESSAGE_MAP()
};

#include "endh.dex"

