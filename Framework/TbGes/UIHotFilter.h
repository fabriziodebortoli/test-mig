#pragma once

#include "TileDialog.h"
#include "HotFilter.h"

#include "beginh.dex"

class CItemsListEdit;

// HFL interface changed elements
#define	HFL_ELEMENT_RADIO		0x01
#define	HFL_ELEMENT_FROM		0x02
#define	HFL_ELEMENT_TO			0x03
#define	HFL_ELEMENT_ITEMLIST	0x04
#define	HFL_ELEMENT_QUERY		0x05
// useful to define custom elements in the form (HFL_ELEMENT_LAST + xx)
#define	HFL_ELEMENT_LAST		0x10

//////////////////////////////////////////////////////////////////////////////////////////
//								CHotFilterTileObj		
//////////////////////////////////////////////////////////////////////////////////////////
//
//--------------------------------------------------------------------------------------
class TB_EXPORT CHotFilterTileObj : public CTileDialog
{
	friend class CHotFilterObj;

	DECLARE_DYNAMIC(CHotFilterTileObj)

public:
    CHotFilterTileObj(const CString& sName, UINT nIDD=0);
    ~CHotFilterTileObj();

	virtual void	AttachOwner	(CObject* pOwner);
	virtual void	OnPinUnpin	();

protected:
	HotFilterObj* m_pHotFilter;
};

//////////////////////////////////////////////////////////////////////////////////////////
//							CHotFilterRangeTile		
//////////////////////////////////////////////////////////////////////////////////////////
//
//--------------------------------------------------------------------------------------
class TB_EXPORT CHotFilterRangeTile : public CHotFilterTileObj
{
	friend class HotFilterRange;
	friend class HotFilterDataPicker;

	DECLARE_DYNCREATE(CHotFilterRangeTile)

public:
    CHotFilterRangeTile	(const CString& sName, int nIDD);
    CHotFilterRangeTile	();
	~CHotFilterRangeTile();

public:
	virtual	void	BuildDataControlLinks		();
	virtual void	OnDisableControlsForBatch	();
	virtual void	AttachOwner					(CObject* pOwner);

protected:
	HotFilterRange* GetHotFilter()	{ return (HotFilterRange*)m_pHotFilter; }

protected:
	//{{AFX_MSG(CHotFilterRangeTile)
		afx_msg void OnFromChanged			();
		afx_msg void OnToChanged			();
	//}}AFX_MSG
	DECLARE_MESSAGE_MAP()
};

//////////////////////////////////////////////////////////////////////////////////////////
//						CHotFilterRangeWithSelectionTile		
//////////////////////////////////////////////////////////////////////////////////////////
//
//--------------------------------------------------------------------------------------
class TB_EXPORT CHotFilterRangeWithSelectionTile : public CHotFilterRangeTile
{
	friend class HotFilterRange;
	friend class HotFilterDataPicker;

	DECLARE_DYNCREATE(CHotFilterRangeWithSelectionTile)

public:
    CHotFilterRangeWithSelectionTile (const CString& sName, int nIDD);
    CHotFilterRangeWithSelectionTile ();
	~CHotFilterRangeWithSelectionTile();

public:
	virtual	void	BuildDataControlLinks		();
	virtual void	AttachOwner					(CObject* pOwner);

protected:
	//{{AFX_MSG(CHotFilterRangeWithSelectionTile)
		afx_msg void OnRadiobuttonChanged	();
	//}}AFX_MSG
	DECLARE_MESSAGE_MAP()
};

//////////////////////////////////////////////////////////////////////////////////////////
//							CHotFilterRangeDateTile		
//////////////////////////////////////////////////////////////////////////////////////////
//
//--------------------------------------------------------------------------------------
class TB_EXPORT CHotFilterRangeDateTile : public CHotFilterTileObj
{
	friend class HotFilterDateRange;

	DECLARE_DYNCREATE(CHotFilterRangeDateTile)

public:
	CHotFilterRangeDateTile (const CString& sName, int nIDD);
    CHotFilterRangeDateTile ();
    ~CHotFilterRangeDateTile();

public:
	virtual	void	BuildDataControlLinks		();
	virtual void	OnDisableControlsForBatch	();
	virtual void	AttachOwner					(CObject* pOwner);

	HotFilterDateRange* GetHotFilter() { return (HotFilterDateRange*)m_pHotFilter; }

protected:
	//{{AFX_MSG(CHotFilterRangeDateTile)
		afx_msg void OnFromChanged			();
		afx_msg void OnToChanged			();
	//}}AFX_MSG
	DECLARE_MESSAGE_MAP()
};

//////////////////////////////////////////////////////////////////////////////////////////
//						CHotFilterDateRangeWithSelectionTile		
//////////////////////////////////////////////////////////////////////////////////////////
//
//--------------------------------------------------------------------------------------
class TB_EXPORT CHotFilterDateRangeWithSelectionTile : public CHotFilterRangeDateTile
{
	friend class HotFilterDateRange;

	DECLARE_DYNCREATE(CHotFilterDateRangeWithSelectionTile)

public:
    CHotFilterDateRangeWithSelectionTile();
    ~CHotFilterDateRangeWithSelectionTile();

public:
	virtual	void	BuildDataControlLinks		();
	virtual void	AttachOwner					(CObject* pOwner);

protected:
	//{{AFX_MSG(CHotFilterDateRangeWithSelectionTile)
		afx_msg void OnRadiobuttonChanged	();
	//}}AFX_MSG
	DECLARE_MESSAGE_MAP()
};

//////////////////////////////////////////////////////////////////////////////////////////
//								CHotFilterPickerTile		
//////////////////////////////////////////////////////////////////////////////////////////
//
//--------------------------------------------------------------------------------------
class TB_EXPORT CHotFilterPickerTile : public CHotFilterRangeWithSelectionTile
{
	friend class HotFilterRange;

	DECLARE_DYNCREATE(CHotFilterPickerTile)

public:
    CHotFilterPickerTile();

public:
	virtual	void	BuildDataControlLinks	();

protected:
	//{{AFX_MSG(CHotFilterRangeWithSelectionTile)
		afx_msg void OnOpenSelection		();
		afx_msg void OnQueryChanged			();
	//}}AFX_MSG
	DECLARE_MESSAGE_MAP()
};

//////////////////////////////////////////////////////////////////////////////////////////
//								CHotFilterListPopupTile		
//////////////////////////////////////////////////////////////////////////////////////////
//
//--------------------------------------------------------------------------------------
class TB_EXPORT CHotFilterListPopupTile : public CHotFilterTileObj
{
	DECLARE_DYNCREATE(CHotFilterListPopupTile)

public:
    CHotFilterListPopupTile();

	virtual	void	BuildDataControlLinks	();

	virtual void	AttachOwner(CObject* pOwner);
	virtual void	OnPinUnpin ();

	HotFilterList* GetHotFilter() { return (HotFilterList*)m_pHotFilter; }

private:
	DataStr				m_ItemsList;
	CItemsListEdit*		m_pCItemsListEdit;

protected:
	//{{AFX_MSG(CHotFilterListPopupTile)
		afx_msg void OnOpenSelection	();
	//}}AFX_MSG
	DECLARE_MESSAGE_MAP()
};

//////////////////////////////////////////////////////////////////////////////////////////
//								CHotFilterListListboxTile		
//////////////////////////////////////////////////////////////////////////////////////////
//
//--------------------------------------------------------------------------------------
class TB_EXPORT CHotFilterListListboxTile : public CHotFilterTileObj
{
	DECLARE_DYNCREATE(CHotFilterListListboxTile)

public:
    CHotFilterListListboxTile();

	virtual	void	BuildDataControlLinks	();

	virtual void	AttachOwner(CObject* pOwner);
	virtual void	OnPinUnpin ();

	HotFilterList* GetHotFilter() { return (HotFilterList*)m_pHotFilter; }
private:
	DataStr				m_ItemsList;
	CItemsListEdit*		m_pCItemsListEdit;

protected:
	//{{AFX_MSG(CHotFilterListTile)
		afx_msg void OnItemListChanged	();
	//}}AFX_MSG
	DECLARE_MESSAGE_MAP()
};

//////////////////////////////////////////////////////////////////////////////////////////
//								CHotFilterListCheckboxTile		
//////////////////////////////////////////////////////////////////////////////////////////
//
//--------------------------------------------------------------------------------------
class TB_EXPORT CHotFilterCheckListboxTile : public CHotFilterTileObj
{
	DECLARE_DYNCREATE(CHotFilterCheckListboxTile)

public:
	CHotFilterCheckListboxTile();

	virtual	void	BuildDataControlLinks();

	virtual void	AttachOwner(CObject* pOwner);
	virtual void	OnPinUnpin();
	HotFilterList* GetHotFilter() { return (HotFilterList*)m_pHotFilter; }

private:
	CParsedCheckListBox*	m_pParsedCheckListBox;

protected:
	//{{AFX_MSG(CHotFilterListTile)
	afx_msg void OnItemListChanged();
	//}}AFX_MSG
	DECLARE_MESSAGE_MAP()
};

#include "endh.dex"
