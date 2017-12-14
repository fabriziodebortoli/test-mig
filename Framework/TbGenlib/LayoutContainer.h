#pragma once

#include <TbNameSolver/CallbackHandler.h>
#include <TbGeneric\WndObjDescription.h>
#include <TbGeneric\TileStyle.h>
//includere alla fine degli include del .H
#include "beginh.dex"

class CBaseTileDialog;
class TileStyle;
class LayoutElement;
class CWndLayoutContainerDescription;

typedef CArray<LayoutElement*, LayoutElement*&> LayoutElementArray;

//*****************************************************************************
// ILayoutObject - useful for EB wrapping
//*****************************************************************************
class TB_EXPORT ILayoutObject
{
public:
	virtual const LayoutElementArray*	GetContainedElements()	= 0;
	virtual const	CString				GetElementNameSpace() = 0;
};

//*****************************************************************************
// LayoutElement
//*****************************************************************************
class TB_EXPORT LayoutElement : public ILayoutObject
{
	friend class CLayoutContainer;
	friend class Tile;

public:
	enum FlexDim { HEIGHT, WIDTH };

	static const int AUTO = -1;
	static const int ORIGINAL = -2;
	static const int FREE = -3;

public:
	LayoutElement();

			void			SetParentElement	(LayoutElement* pParent)				{ m_pParentElement = pParent; }
			LayoutElement*	GetParentElement	()										{ return m_pParentElement; }

	virtual LayoutElement*	AddChildElement		(LayoutElement* pChild)					{ pChild->SetParentElement(this); return pChild; }
	virtual void			RemoveChildElement	(LayoutElement* pChild)					{ ASSERT_TRACE(pChild->GetParentElement() == this,"element is not a child"); pChild->SetParentElement(NULL); }
	virtual int				FindChildElement	(LayoutElement* pChild)					{ ASSERT_TRACE(FALSE, "Must be reimplemented in the derived class"); return -1; }
	virtual void			InsertChildElement	(LayoutElement* pChild, int nPos)		{ ASSERT_TRACE(FALSE, "Must be reimplemented in the derived class"); }
	virtual BOOL			IsFlexAuto			()										{ return m_nFlex == AUTO; }
	virtual BOOL			CanDoLastFlex		(FlexDim)								{ return TRUE; }

public:	
	// method to reimplement for the LayoutElement interface 
	virtual BOOL	IsVisible				()					= 0;
	virtual int		GetRequiredHeight		(CRect &rectAvail)	= 0; // must return the required heigth FOR ITSELF, return rectAvail.Height() if the element want to occupy all the available space
	virtual int		GetRequiredWidth		(CRect &rectAvail)	= 0; // the same for the witdh
	virtual void	GetAvailableRect		(CRect &rectAvail)	= 0; // returns the RECT available to the contained layout
	virtual void	Relayout				(CRect &rectNew, HDWP hDWP = NULL)	= 0; // apply internal layout in the specified RECT
	virtual void	GetUsedRect				(CRect &rectUsed)	= 0; // returns the RECT actually used for the contained layout (maybe bigger than assigned, that is, scrollbars are needed)

	// propagate to the parent container the request to recalculate the layout, or execute it if top-level element
	virtual void	RequestRelayout();

	BOOL	IsFlex(FlexDim fd) { return GetFlex(fd) > 0; }
	virtual int		GetFlex(FlexDim) { return IsFlexAuto() ? 1 : m_nFlex; }
	virtual Tile*	AsATile() { return NULL; }
	virtual int		GetMinWidth			()									{ return m_nMinWidth < 0 ? 0 : m_nMinWidth; }
	virtual int		GetMaxWidth			()									{ return m_nMaxWidth < 0 ? INT_MAX : m_nMaxWidth; }
	virtual int		GetMinHeight		(CRect& rect = CRect(0, 0, 0, 0))	{ return m_nMinHeight < 0 ? 0 : m_nMinHeight; }

protected:
			int		GetRequestedLastFlex()									{ return m_nRequestedLastFlex; }

private:	
	// backward compatibility, STRIPE layout only
	virtual CBaseTileDialog*	GetTileDialog		()	{ return NULL; }
	virtual CWnd*				GetCWnd				()	{ return NULL; }
	virtual BOOL				IsFillEmptySpaceMode()	{ return FALSE; }

public:
	virtual void	SetFlex				(int nFlex, BOOL bInContainerToo = TRUE)	{ m_nFlex = nFlex; }
			void	SetRequestedLastFlex(int nFlex = 1)								{ m_nRequestedLastFlex = nFlex; }
	virtual void	SetMinWidth			(int nWidth)								{ m_nMinWidth = nWidth; }
	virtual void	SetMaxWidth			(int nWidth)								{ m_nMaxWidth = nWidth; }
	virtual void	SetMinHeight		(int nHeight)								{ m_nMinHeight = nHeight; }

	virtual	void	SetGroupCollapsible	(BOOL bSet = TRUE)		{ m_bGroupCollapsible = bSet; }
	virtual	BOOL	IsGroupCollapsible	()						{ return m_bGroupCollapsible; }

	virtual const	LayoutElementArray*	GetContainedElements()	{ return NULL; }

protected:
	int				m_nMinHeight;
	int				m_nMinWidth;
	int				m_nMaxWidth;
	LayoutElement*	m_pParentElement;

private:
	int		m_nFlex;				// flex index for proportional layouts (i.e.: hbox, vbox)
	int		m_nRequestedLastFlex;
	BOOL	m_bGroupCollapsible;	// this element groups other elements (i.e tiles or tile panels) which collapse/expand as a whole
};

typedef CArray<CLayoutContainer*, CLayoutContainer*&> LayoutContainerArray;

class CBaseTileGroup;
class Layout;

//*****************************************************************************
// CLayoutContainer
//*****************************************************************************
class TB_EXPORT CLayoutContainer : public CObject, public LayoutElement, public IDisposingSourceImpl
{
	friend class LayoutElement;
	friend class CBaseTileGroup;

	DECLARE_DYNAMIC(CLayoutContainer)

	enum LayoutType		{ STRIPE = 0, COLUMN = 1, HBOX = 3, VBOX = 4, NONE = -1 };
	enum LayoutAlign	{ BEGIN = 0, MIDDLE = 1, END = 2, STRETCH = 3, STRETCHMAX = 4, NO_ALIGN = - 1 };

public:
	CLayoutContainer(LayoutElement*	pOwner, TileStyle*&	pTileStyle);
	~CLayoutContainer();

public:
	LayoutType GetLayoutType	()	{ return m_LayoutType; }
	LayoutAlign GetLayoutAlign	()	{ return m_LayoutAlign; }
	
	void SetLayoutType	(LayoutType aLayoutType)	{ m_LayoutType = aLayoutType; }

	void SetLayoutAlign	(LayoutAlign aLayoutAlign)	{ m_LayoutAlign = aLayoutAlign; }
	
	CWndLayoutContainerDescription* GetDescription(CWndObjDescriptionContainer* pContainer);

	virtual LayoutElement*	AddChildElement		(LayoutElement* pChild);
	virtual void			RemoveChildElement	(LayoutElement* pChild);
	virtual int				FindChildElement	(LayoutElement* pChild);
	virtual void			InsertChildElement	(LayoutElement* pChild, int nPos);
	
	virtual	void			SetGroupCollapsible	(BOOL bSet = TRUE);
	virtual void			AddOwnedContainer	(CLayoutContainer* pContainer);
			void			ClearChildElements	();

private:
	void DoStripeLayout		(CRect& rectContainer, HDWP hDWP = NULL);
	void DoHBoxLayout		(CRect& rectContainer);
	void DoVBoxLayout		(CRect& rectContainer);
	void DoColumnLayout		(CRect& rectContainer);

	void PrepareColumnLayout	(Layout* pLayout, CRect& rectContainer);
	void PrepareHBoxLayout		(Layout* pLayout, CRect& rectContainer);
	void PrepareVBoxLayout		(Layout* pLayout, CRect& rectContainer);

	// backward compatibility, STRIPE layout only
	void ResizeBottomElements	(int bottomLine, LayoutElementArray& bottomElements, const CRect& rcGroup, HDWP hDWP);
	void ResizeRightElements	(LayoutElementArray& rightElements, const CRect& rcGroup, HDWP hDWP);
	void AdjustScrollSize		(CRect& rectContainer);

	int		CalculateRowBottom						(Layout* pLayout, int nRowIndex);
	void	AlignTilesOnTheSameRowToBottomRow		(Layout* pLayout, BOOL bFillEmptySpace);
	void	AlignLastBottomTilesToBottomOfContainer	(Layout* pLayout, BOOL bFillEmptySpace, CRect &rectContainer);
	void	StretchHorizontal						(Layout* pLayout, BOOL bFillEmptySpace, CRect &rectContainer, int tileSpacing);


	void	CalculateIdealFlexCondition(CRect &rectContainer);
	CScrollView* GetParentScrollView();
	//end backward compatibility

public:	
	// LayoutElement interface implementation
	virtual const	CString				GetElementNameSpace();
	virtual const	LayoutElementArray*	GetContainedElements()					{ return &m_Elements; }
	virtual			BOOL				IsVisible			()					{ return TRUE; }
	virtual			int					GetRequiredHeight	(CRect &rectAvail);
	virtual			int					GetRequiredWidth	(CRect &rectAvail);
	virtual			void				GetAvailableRect	(CRect &rectAvail);
	virtual			void				Relayout			(CRect& rectNew, HDWP hDWP = NULL);
	virtual			void				GetUsedRect			(CRect &rectUsed);

public:	
	virtual	int		GetMinWidth			();
	virtual	int		GetMinHeight		(CRect& rect = CRect(0, 0, 0, 0));
	virtual	BOOL	CanDoLastFlex		(FlexDim fd);
	virtual	int		GetFlex				(FlexDim);

protected:
	void RemoveAllContainedElements()	{ m_Elements.RemoveAll(); }

	
private:
	LayoutElementArray	m_Elements;
	LayoutType			m_LayoutType;
	LayoutAlign			m_LayoutAlign;
	CRect				m_LastInvalidatedRect;
	TileStyle*&			m_pTileStyle;

	// backward compatibility, STRIPE layout only
	CScrollView*		m_pParentScrollView;
	BOOL				m_bDoingLayout;

	// centralizzato meccanismo di cancellazione container
	// owned dal layout 
	CObArray			m_OwnedContainers;
};


//=============================================================================
//			Class EnumLayoutContainerDescriptionAssociations
//=============================================================================
// Singleton containing all the enum-description associations
class EnumLayoutContainerDescriptionAssociations
{
public:
	EnumDescriptionCollection<CLayoutContainer::LayoutType>		m_arLayoutType;
	EnumDescriptionCollection<CLayoutContainer::LayoutAlign>	m_arLayoutAlign;
	EnumDescriptionCollection<TileDialogStyle>					m_arTileDialogStyle;


public:
	EnumLayoutContainerDescriptionAssociations()
	{
		InitEnumLayoutContainerDescriptionStructures();
	}

	void InitEnumLayoutContainerDescriptionStructures();
};

//=============================================================================
//			Class CWndLayoutContainerDescription
//=============================================================================
class TB_EXPORT CWndLayoutContainerDescription : public CWndPanelDescription
{
	DECLARE_DYNCREATE(CWndLayoutContainerDescription);

	static EnumLayoutContainerDescriptionAssociations singletonEnumLayoutContainerDescription;

protected:
	CWndLayoutContainerDescription()
	{
		m_Type = CWndObjDescription::LayoutContainer;
	}

public:
	CLayoutContainer::LayoutType m_LayoutType = CLayoutContainer::COLUMN;
	CLayoutContainer::LayoutAlign m_LayoutAlign = CLayoutContainer::STRETCH;
	CString		m_strIcon;
	IconTypes	m_IconType = IconTypes::IMG;
	Bool3 m_bIsCollapsible = B_UNDEFINED;
	bool  m_bIsCollapsed = false;
	bool m_bShowAsTile = false;//solo per TilePanel
	bool m_bManageUnpinned = false;//only for TileGroup
	bool m_bOwnsPane = true;//only for TileGroup
	TileDialogStyle m_Style = TDS_NONE;
	int   m_nFlex = -1;

	CWndLayoutContainerDescription(CWndObjDescription* pParent)
		: CWndPanelDescription(pParent)
	{
		m_Type = CWndObjDescription::LayoutContainer;
	}

	virtual void Assign(CWndObjDescription* pDesc);
	virtual void SerializeJson(CJsonSerializer& strJson);
	virtual void ParseJson(CJsonFormParser& parser);
	virtual void AddChildWindows(CWnd* pWnd);
	virtual void UpdateWindowText(CWnd *pWnd) { /*do nothing */ }

	void UpdateLayoutAttributes(CLayoutContainer* pLayoutContainer);

	static	CString		GetEnumDescription(CLayoutContainer::LayoutType value);
	static	void		GetEnumValue(CString description, CLayoutContainer::LayoutType& retVal);

	static	CString		GetEnumDescription(CLayoutContainer::LayoutAlign value);
	static	void		GetEnumValue(CString description, CLayoutContainer::LayoutAlign& retVal);

	static	CString		GetEnumDescription(TileDialogStyle value);
	static	void		GetEnumValue(CString description, TileDialogStyle& retVal);

	static	CString		GetEnumDescription(IconTypes value);
	static	void		GetEnumValue(CString description, IconTypes& retVal);

	CWndLayoutContainerDescription* GetChildLayoutContainer(CLayoutContainer* pLayoutEl);

};

//=============================================================================
//			Class CWndLayoutContainerDescription
//=============================================================================
class TB_EXPORT CWndLayoutStatusContainerDescription : public CWndLayoutContainerDescription
{
	DECLARE_DYNCREATE(CWndLayoutStatusContainerDescription);

private:
	CWndLayoutStatusContainerDescription()
	{
		m_Type = CWndObjDescription::LayoutContainer;
		m_LayoutType = CLayoutContainer::COLUMN;
		m_LayoutAlign = CLayoutContainer::BEGIN;
	}
};


#include "endh.dex"

/*
LAYOUTS
=======

STRIPE: for backward compatibility only. It arranges the elements in stripes of Theme.TilePreferredHeightUnit, not exceeding
Theme.TileMaxHeightUnit. Elements are stacked vertically until the required height is reached, then a new stack is started on 
an adjacent column. When no more space is available on the right, a new stripe is started below. The bottom elements of each 
stripe are streteched to the stripe height. 

COLUMN: elements are arranged in a multi-column format where the width of each column can be specified as a percentage or 
fixed width, but the height is allowed to vary based on the content. 
The logic makes two passes through the set of contained elements: during the first layout pass, all elements that either have a fixed 
width or none specified (auto) are skipped, but their widths are subtracted from the overall container width. 
During the second pass, all elements with columnWidths are assigned widths in proportion to their percentages based on the total 
remaining container width. In other words, percentage width elements are designed to fill the space left over by all the fixed-width 
and/or auto-width elements. Because of this, while you can specify any number of columns with different percentages, the columnWidths 
must always add up to 1 (or 100%) when added together, otherwise your layout may not render as expected.

HBOX: elements are arranged horizontally across their container. This layout optionally divides available horizontal space between 
contained elements having a numeric "flex" configuration. Each child element with a flex property will be flexed horizontally 
according to each element's relative flex value compared to the sum of all elements with a flex value specified. Any child elements that 
have a flex = 0 will not be 'flexed' (the initial size will not be changed).

This layout may also be used to set the heights of child elements by configuring it with the align option.


*/
