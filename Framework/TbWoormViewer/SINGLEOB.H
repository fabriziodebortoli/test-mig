
#pragma once

#include "baseobj.h"
#include "bordpen.h"

//includere alla fine degli include del .H
#include "beginh.dex"

//===========================================================================
class ViewParser;
class ViewUnparser;
class CWoormDocMng;


//===========================================================================
class TB_EXPORT SingleItemObj : public BaseObj
{
	DECLARE_DYNAMIC (SingleItemObj)
	friend class CRS_ObjectPropertyView;

public:
	enum ActionType
		{
			LEAVE, NORD, SUD, EST, WEST, NW, NE, SW, SE, MOVE, DRAGGING
		};

protected:
	ActionType	m_nAction;
	BorderPen	m_BorderPen;

protected:
	// protected virtual function
	BOOL		Action				(CPoint);
	void		SetObjCursor		(ActionType);
	void		SetObjCursor		();
	void		BeforeAction		();
	void		AfterAction			();
	void 		ClipActionCursor	(CDC&, CPoint, CBCGPScrollView*);
	void 		SetActionCursor		(CPoint);

    // virtual private function
	virtual void		OnTrackingDraw	(CDC&, CBCGPScrollView* pView, BOOL bShowRect);
	virtual	void		Resize			(CPoint, CPoint);
	virtual void		Resize			(ActionType, CPoint, CPoint);
	virtual	ActionType	ItemAction		(CPoint);

	// showing flags member function
	virtual	BOOL	NoBorders	(BOOL printing);
	virtual	BOOL	ShowLabels	(BOOL printing);

public:
	// costruttori
	SingleItemObj	(CPoint, CWoormDocMng*);
	SingleItemObj	(const SingleItemObj& source);

public:
	BorderPen	GetBorderPen() { return m_BorderPen; }

	// costruttori
	// single-object virtual member function
	virtual void 	Paint			(CDC&, CPrintInfo*);
	virtual	void	Format			()			{ /* do nothing */}

	// mouse clicked in object move area
	virtual	BOOL	InMoveArea		(CPoint);

	// definition file manager function
	virtual	BOOL	Parse		(ViewParser&)	{ return TRUE; }
	virtual	void	Unparse		(ViewUnparser&)	{ /* do nothing */}

	// moving member function
	virtual	void	LeftObject			(int x);
	virtual	void	TopObject			(int y);
	virtual	void	MoveObject			(CSize);
	virtual	void	SizeObject			(CSize);
	virtual	void	RightSizeObject		(int);
	virtual	void	BottomSizeObject	(int);
	virtual	void	LeftSizeObject		(int);
	virtual	void	TopSizeObject		(int);
	virtual	BOOL	MovingObject		() const;

	// fro RDE search purpouse
	virtual	BOOL	CanSearched		() const;

	// single-object pure virtual member function
	virtual void	Draw	(CDC&, BOOL bPreview) = 0;
	
	// virtual key accelerator management
	virtual	void	VKReturn	()	{ /* do nothing */}

	ActionType	GetAction() { return m_nAction; }

protected:
	//{{AFX_MSG(SingleItemObj)
	afx_msg	void	LineWidth		();
	afx_msg	void	SetSize			();
	//}}AFX_MSG

	DECLARE_MESSAGE_MAP()
};

//////////////////////////////////////////////////////////////////////////////
//////////////////////////////////////////////////////////////////////////////
//                                                                          //
//                 I N L I N E    S T A R T    D E F I N I T I O N          //
//                                                                          //
//////////////////////////////////////////////////////////////////////////////
//////////////////////////////////////////////////////////////////////////////

//=============================================================================
inline void SingleItemObj::Resize 	(CPoint c, CPoint p)	{ Resize(m_nAction,c,p); }
inline void SingleItemObj::SetObjCursor		()	{ SetObjCursor (m_nAction); }
inline BOOL SingleItemObj::MovingObject		() const { return m_nAction == MOVE; }
inline BOOL	SingleItemObj::CanSearched		() const { return FALSE; }

#include "endh.dex"
