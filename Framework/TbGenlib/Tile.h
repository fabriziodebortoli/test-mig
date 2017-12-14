#pragma once

#include <afxwin.h>
#include <TbGeneric\TileStyle.h>
#include "TileButtons.h"

#include "beginh.dex"

class CBaseTileGroup;
class LayoutElement;

class TB_EXPORT Tile
{
	friend class CCollapseButton;
	friend class CPinButton;

public:
	Tile(TileStyle* pStyle);
	~Tile();

public:
			void		SetTitle				(const CString& strTitle)	{ m_strTitle = strTitle; }
			CString		GetTitle				()							{ return m_strTitle; } 
			void		SetCollapsedDescription (CString strDescription)	{ m_strCollapsedDescription = strDescription; }
			// retain customized attribute
			virtual void		SetTileStyle			(TileStyle* pStyle);
			void	SetTileStyleByName(CString sName);
			TileStyle*	GetTileStyle			() const					{ return m_pTileStyle; }


			// set if the tile can be collapsed by the user
			void	SetCollapsible			(BOOL bSet = TRUE);
			// collapse / expand the tile 
			virtual void	SetCollapsed			(BOOL bCollapsed = TRUE);
			BOOL	IsCollapsed				()							{ return m_bCollapsed; }

			// set if the tile can be pinned/unpinned by the user
			void	SetPinnable				(BOOL bSet = TRUE);
			BOOL	IsPinnable				();
			// pin / unpin the tile 
			virtual void	SetPinned		(BOOL bPinned = TRUE);
			BOOL	IsPinned				()							{ return m_bPinned; }
			void	ForceSetPinned			(BOOL bPinned /*= TRUE*/);

			void	SetResetValuesAfterUnpin(BOOL bSet = TRUE) { m_bResetValuesAfterUnpin = bSet; }
			BOOL	GetResetValuesAfterUnpin() { return m_bResetValuesAfterUnpin; }

			void	SetHasTitle				(BOOL bSet = TRUE);

			BOOL	IsEnabled				()							{ return m_bEnabled; }

			int		GetTitleHeight()	{ return m_nTitleHeight;  }
	virtual void	Enable					(BOOL bEnable);

	void	Show(BOOL bVisible);

	BOOL	HasTransparentBackground();
	virtual BOOL IsGroupCollapsible();

protected:
	// needed to implement the Tile interface
	virtual CWnd*				GetTileCWnd			()	= 0;
	virtual CBaseTileGroup*		GetParentTileGroup	()	= 0;
	virtual void				DoCollapseExpand	()	= 0;
	virtual void				DoPinUnpin			()	= 0;
	virtual LayoutElement*		AsALayoutElement	()	= 0;

protected:
	// need to be called in the derived classes
	// call it in the OnPaint (ON_WM_PAINT message)
	void		OnTilePaint			(CDC* pDC);
	// call it in the Create, after the creation of the visual component
	void		TileCreate			(const CString& strTitle);

	// @@@TODO: scoprire perche` BaseTileDialog ha bisogno di chiamare il posizionamento del bottone nella WindowPosChanged
	void		OnTilePosChanged	();

protected:
	// discard customized attributes
	void		ResetTileStyle		(TileStyle* pStyle);
	void		CollapseExpand		();
	void		PinUnpin			();
	void		RefreshPinButton	();

private:
	CString	GetCurrentTitle		()			{ return IsCollapsed() && !m_strCollapsedDescription.IsEmpty() ? m_strCollapsedDescription : m_strTitle; }
	BOOL	HasCollapsedButton	() const;
	BOOL	HasPinnedButton		() const;
	// show the collapsed/expanded button if the tile allows it
	void	ShowCollapsedButton	();
	// show the pinned/unpinned button if the tile allows it
	void	ShowPinnedButton	();
	void	CalculateTitleHeight();

protected:
	int					m_nTitleHeight; // title height after rescaling
	BOOL				m_bProcessingSiblingRequest; // used to avoid multiple call to relayout while responding to a sibling request (i.e. collapse/expand)
	TileStyle*			m_pTileStyle;	// its own style
	BOOL				m_bPinned;

private:
	CString				m_strTitle;
	CString				m_strCollapsedDescription;
	BOOL				m_bCollapsed;
	CCollapseButton		m_CollapseButton;
	CPinButton			m_PinButton;
	int					m_nCollapseButtonOffsetY;
	BOOL				m_bEnabled;
	BOOL				m_bResetValuesAfterUnpin;

protected:
	BOOL				m_bVisible;
};

#include "endh.dex"
