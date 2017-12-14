
#pragma once

#include "singleob.h"
#include "TABLE.H"
#include "COLUMN.H"


//includere alla fine degli include del .H
#include "beginh.dex"

//===========================================================================
class ViewParser;
class ViewUnparser;
class CWoormDocMng;
class CRSTreeCtrl;
class CNodeTree;

//==============================================================================
//          Class SelectedObj
//==============================================================================
class SelectedObj : public CObject
{
public:
	BaseObj*    m_pBaseObj;
	CRect       m_TrackingRect;
	ActiveRect*	m_pActive;

public:
	SelectedObj (BaseObj*, CRect, CWoormDocMng*);
	virtual ~SelectedObj ();
	virtual	void	Paint	(CDC&, CPrintInfo* pInfo);

	Stack*			m_pUndoStack;

	virtual	void	AddUndoInfo() { m_pUndoStack->Push(new UndoInfo(m_pBaseObj, m_pBaseObj->GetBaseRect()));/* do nothing */ }

	int     Top         ()      { return m_pBaseObj->Top();    }
	int     Bottom      ()      { return m_pBaseObj->Bottom(); }
	int     Left        ()      { return m_pBaseObj->Left();   }
	int     Right       ()      { return m_pBaseObj->Right();  }
	int     Width       ()      { return m_pBaseObj->Width();  }
	int     Height      ()      { return m_pBaseObj->Height(); }

	void    LeftObject  (int x);
	void    TopObject   (int y);

	void    MoveObject 			(CSize);
	void    SizeObject  		(CSize);
	void    RightSizeObject 	(int);
	void    BottomSizeObject	(int);
	void    LeftSizeObject	 	(int);
	void    TopSizeObject 		(int);

	void	Undo				();

	// enable activation proprietary
	void	SetActive (BOOL bLast = TRUE);
};

//==============================================================================
//          Class CSelectedObjArray definition
//==============================================================================


// class array to avoid ripetitive cast
//===========================================================================
class TB_EXPORT CSelectedObjArray : public CObArray
{
public:
	// overloaded operator helpers
	SelectedObj* operator[](int nIndex) const	{ return (SelectedObj*) GetAt(nIndex); }
	SelectedObj*& operator[](int nIndex)		{ return (SelectedObj*&) ElementAt(nIndex); }
};                                        


//==============================================================================
//          Class SelectionRect definition
//==============================================================================


//==============================================================================
class TB_EXPORT SelectionRect : public SingleItemObj
{
public:
	CSelectedObjArray	m_SelectedObjects;

protected:

	// virtual private function
	virtual void OnTrackingDraw			(CDC&, CBCGPScrollView* pView, BOOL bShowRect);
	virtual	void Resize					(CPoint, CPoint);
	virtual	ActionType	ItemAction		(CPoint);
	
	// protected overridable virtual function 
	// don't manage clipboard copy and undo action
	virtual	void	CopyClipboard		()			{ /* do nothing */ }
	virtual	void	AddCreateUndoInfo	()			{ /* do nothing */ }
	virtual	void	AddUndoInfo			()			{ /* do nothing */ }
   
	// align space equal utility method
	void	SortPosition	(BOOL bVertical);
	void	AlignSpaceEqual	(BOOL bVertical);

public:
	SelectionRect (CWoormDocMng*);
	~SelectionRect ();

    // specific multiple selection function member
	void	AddToSelectedObjects		(BaseObj*, CRect);
	void	RemoveFromSelectedObjects	(BaseObj*);
	void	MoveMultipleSelObjects		();
	void	MoveMultipleSelObjects		(CSize offset);
	void	ClearMultipleSelObjects		();
	void	AlignMultipleSelObjects		(int hID, int vID);
	void	SizeMultipleSelObjects		(int hID, int vID, CSize size);
	BOOL	IsEmpty						() const;
	BOOL	ObjectInSelection			(BaseObj*);
	
	SelectedObj* GetSelectedObjInSelection (BaseObj*);

	int		GetSize						() const { return m_SelectedObjects.GetSize(); }
	void	BuildBaseRect				();

	// align function  for multiple selection
	void	AlignHLeft				();
	void	AlignHRight				();
	void	AlignHSpaceEqual		();

	void	AlignVTop				();
	void	AlignVBottom			();
	void	AlignVSpaceEqual		();

	void	StackLeft				();
	void	StackRight				();
	void	StackTop				();
	void	StackBottom				();

	void	CutHLeft				();
	void	CutHRight				();
	void	CutVTop					();
	void	CutVBottom				();
	
	// size function for multiple selection
	void	SizeLargeAsLast	();
	void	SizeHValue		(int Width);

	void	SizeHighAsLast	();
	void	SizeVValue		(int Height);

	// border on/off
	virtual void	ToggleBorder		(UINT nID);
	virtual BOOL	IsBorderOn			(UINT nID);
	virtual void	SnapToGrid			();
	virtual BOOL	IsTransparentOn		();
	virtual void	ToggleTransparent	();

	// generic virtual function for any MultipleSelectionObj
	virtual	void	ChangedAction	();
	virtual void 	Paint			(CDC&, CPrintInfo*);
	virtual void	Draw			(CDC& , BOOL /*bPreview*/)	{ /* do nothing */ }
	virtual	void	ShowPopup		(CPoint, CBCGPScrollView*)	{ /* do nothing */ }
	
	// definition file manager function
	virtual	BOOL	Parse	(ViewParser&)	{ return TRUE; }
	virtual	void	Unparse	(ViewUnparser&)	{ /* do nothing */}

	int GetPreviousSize()	{ return previousSize; }

	BaseObj*			GetObjAt(int i);
	const CRect&		GetRectAt(int i);
	//implementation of baseobj's virtual method to update layout object
	void				Redraw();
	void				SetWrmStyleClass		(CObject* templateObj);
	CString				GetWrmStyleClass		();

	virtual	void	UndoMoveAndSize(CRect);	/*{  do nothing} */
	virtual	void	Undo();
	INT_PTR			Find(CNodeTree* pNode);
	BOOL			ContainsAtLeastOneFieldRect();

private:
	int previousSize;
};

//==============================================================================
class TB_EXPORT DragRect : public SingleItemObj
{
public:
	// costruttori
	DragRect (CPoint ptCurrPos, CWoormDocMng*);

public:
	virtual void OnTrackingDraw	(CDC&, CBCGPScrollView* pView, BOOL bShowRect);
	virtual void Draw			(CDC& , BOOL /*bPreview*/)	{ /* do nothing */ }
	virtual	void ShowPopup		(CPoint, CBCGPScrollView*)	{ /* do nothing */}
	virtual	void ChangedAction	();
	//override of baseobj virtual method
	virtual void		Redraw				()			{/*do nothing*/ }
	virtual COLORREF*	GetBkgColor			()			{ return NULL; }
	virtual COLORREF*	GetLabelForeColor	()			{ return NULL; }
	virtual COLORREF*	GetValueForeColor	()			{ return NULL; }

	virtual void	MouseMove(CDC&, CPoint, CBCGPScrollView*);
	void			InitDraggingRect(CDC&, CPoint, CBCGPScrollView*);
private:
	CRect			GetTrackRect();//always positive size
};


//==============================================================================
class TB_EXPORT MultiColumnSelection: public CObject, public IDisposingSourceImpl
{
public:
	CActiveColumnArray		m_columns;
	CWoormDocMng*			m_pDocument = NULL;

public:
	// costruttori
	MultiColumnSelection(CWoormDocMng*);
	~MultiColumnSelection();

public:
	// specific multiple selection function member
	void			AddToSelectedColumns		(TableColumn*, CRect = NULL);
	void			RemoveAllSelectedColumns	();

	void			DeleteAllSelectedColumns	();

	int				GetSize						()	  const { return m_columns.GetSize(); }
	CString			GetAllTitles				();
	void			Redraw						();
	TableColumn*	GetAt						(int index);
	void			BuildBaseRect				();
	virtual void 	Paint						(CDC&, CPrintInfo*);
	void			ClearMultipleSelObjects		();
	INT_PTR			Find						(CNodeTree* pNode);
	BOOL			AreMultiRows				();
	
private:
	int	previousSize;
};

#include "endh.dex"


