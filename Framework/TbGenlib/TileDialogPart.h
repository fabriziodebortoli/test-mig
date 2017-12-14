#pragma once
#include <afx.h>
#include "LayoutContainer.h"

#include "beginh.dex"

//===========================================================================
class TB_EXPORT CTileDialogPart : public CObject, public LayoutElement
{
	friend class CBaseTileDialog;

	DECLARE_DYNAMIC(CTileDialogPart)
	
public:
	CTileDialogPart(CBaseTileDialog* pParent, CRect& rectStatic);
	~CTileDialogPart();

public:
			void	SetOriginalRect		(CRect &rectAvail, CTileDialogPart* pPartAfter = NULL);
			void	SetStaticAreaExtent	(int nLeft, int nRight);

	const	CRect&	GetStaticAreaRect		()							{ return m_rectStatic; }
			BOOL	IsInStaticArea			(const CRect& rectCtrl);
			BOOL	Contains				(const CRect& rectCtrl);
			BOOL	Contains				(CWnd* pWnd);
			CRect	Intersect				(const CRect& rectCtrl);

			void	AddControl				(CWnd* pCtrl, const CRect& rectCtrl, BOOL bAdd = TRUE);

			int		ResizeStaticAreaWidth	(int nOffset, int nNewWidth);
			void	ResizeStaticAreaHeight	(int nNewHeight);
			void	SetSize					();
public:
	// LayoutElement interface implementation
	virtual const	CString				GetElementNameSpace() { return _T(""); }
	virtual const	LayoutElementArray*	GetContainedElements()					{ return NULL;}
	virtual			BOOL				IsVisible			()					{ return TRUE; }
	virtual			int					GetRequiredHeight	(CRect &rectAvail);
	virtual			int					GetRequiredWidth	(CRect &rectAvail);
	virtual			void				GetAvailableRect	(CRect &rectAvail);
	virtual			void				Relayout			(CRect& rectNew, HDWP hDWP = NULL);
	virtual			void				GetUsedRect			(CRect &rectUsed);

public:
	virtual	int		GetMinWidth			();
	virtual	int		GetMaxWidth			();
	virtual	int		GetMinHeight		(CRect& rect = CRect(0, 0, 0, 0));

private:
	void	ResizeRightmostControls		(const CRect& rectNew);
	void	AdjustLocationNonDockedRightMostControls(const CRect& rectNew);
	//void	AdjustStateControlsPosition	(CWnd* pCtrl, int displX, int displY);
	void	GetControlStructure(CWndObjDescriptionContainer* pContainer, CString sId);

		
private:
	CRect					m_rectStatic;
	CRect					m_rectActual;
	CRect					m_rectOriginal;
	CBaseTileDialog*		m_pParent;
	CArray<HWND>			m_Anchored;
	CArray<HWND>			m_Controls;
	int						m_RightmostColumn;
	int						m_BottomLine;
	int						m_nAutoMinWidth;
	int						m_nAutoMaxWidth;
};

#include "endh.dex"
