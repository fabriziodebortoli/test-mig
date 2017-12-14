#pragma once

//includere alla fine degli include del .H
#include "beginh.dex"

//=============================================================================
class TB_EXPORT CPaletteBar : public CToolBar
{
protected:
	UINT    m_nColumns;

// Constructor
public:
	CPaletteBar();
	void SetColumns(UINT nColumns);
	UINT GetColumns() { return m_nColumns; };

	// Implementation
public:
	virtual ~CPaletteBar();
#ifdef _DEBUG
	virtual void AssertValid() const;
	virtual void Dump(CDumpContext& dc) const;
#endif

	// Generated message map functions
protected:
	//{{AFX_MSG(CPaletteBar)
	//}}AFX_MSG
	DECLARE_MESSAGE_MAP()
};

#include "endh.dex"
