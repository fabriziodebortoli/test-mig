#pragma once

#include <TbGeneric\DockableFrame.h>
#include "beginh.dex"

//======================================================================
class TB_EXPORT CTaskBuilderSplitterWnd : public CBCGPSplitterWnd
{
	DECLARE_DYNCREATE(CTaskBuilderSplitterWnd);

// Attributes
public:
    BOOL	m_bPanesSwapped;
    float	m_fSplitRatio;
    int		m_nSplitResolution;

// Implementation
public:
	CTaskBuilderSplitterWnd		();
	~CTaskBuilderSplitterWnd	();
	
	CWnd* GetActivePane			(int* pRow = NULL, int* pCol = NULL);

public:
	void SetSplitRatio			(float fRatio );
	BOOL IsSplitHorizontally	() const;
	BOOL IsSplitVertically		() const { return !IsSplitHorizontally(); }
	BOOL ArePanesSwapped		() const { return m_bPanesSwapped; }

protected:
    void UpdateSplitRatio		();
    void UpdatePanes			(int cx, int cy);
    void UpdatePanes			();

public: 
    void SplitVertically		();
    void SplitHorizontally		();

	int AddWindow (CRuntimeClass* pWndClass, CCreateContext* pCreateContext, int nRow = 0, int nCol = 0); 

protected: 
	afx_msg void OnSize		(UINT nType, int cx, int cy);
	afx_msg void OnLButtonUp(UINT uFlags, CPoint point);

	DECLARE_MESSAGE_MAP()
};

#include "endh.dex"