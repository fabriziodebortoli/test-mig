#pragma once

#include "textobj.h"
#include "viewpars.h"

#include "rectobj.h"
#include "WoormDoc.h"

//includere alla fine degli include del .H
#include "beginh.dex"

//==============================================================================

class Repeater;

class TB_EXPORT RepeaterObjects : public Array
{
	friend class Repeater;
	DECLARE_DYNAMIC (RepeaterObjects)

protected:
	Repeater*	m_pRepeater;

//public:
	RepeaterObjects(Repeater* pRep);
	RepeaterObjects(Repeater* pRep, const RepeaterObjects& source);
	virtual  ~RepeaterObjects();

	CLayout* GetMasterObjects();

	virtual void RemoveAll();

	void	AddChild	(BaseObj* pObj);
	void	RemoveChild	(BaseObj* pObj);

	void	Replicate();

	void	Draw					(CDC&, BOOL bPreview, int nStart = 1);
	void	ClearDynamicAttributes	();
	void	DisableData				();

	int		GetFieldRow				(BaseObj* pO);

	BaseObj* GetFieldByPosition		(const CPoint&, int& nRow);
	BaseObj* GetMasterFieldByPosition (const CPoint& pt);

	void	MoveBaseRect	(int xOffset, int yOffset, BOOL bIgnoreBorder = FALSE);
	BaseObj* operator		[](int nIndex) const { return (BaseObj*)GetAt(nIndex); }

protected:
	void	Detach	(BaseObj* pObj);
	void	RemoveDuplicateTopBorders(CLayout* pRow);
	void	RemoveDuplicateLeftBorders(CLayout* pRow);
};

///////////////////////////////////////////////////////////////////////////////
class TB_EXPORT Repeater : public SqrRect
{
	friend class RepeaterObjects;
	friend class  CRSTreeCtrl;
	DECLARE_DYNAMIC (Repeater)

public:
	int m_nRows;
	int m_nColumns;
	int m_nXOffset;
	int m_nYOffset;

protected:
	BOOL m_bByColumn;

	RepeaterObjects m_Objects;

	int m_nCurrentRow;	//RDE

public:
	int	m_nViewCurrentRow;	//VIEW

	Repeater	(CPoint, CWoormDocMng*, WORD wID);
	Repeater	(const Repeater& source);
	virtual  ~Repeater();

	virtual BaseObj* Clone() const { return new Repeater(*this); }

	virtual int		RowsNumber		() const { return m_nRows * m_nColumns; }					
	virtual int		GetViewCurrentRow	() const { return m_nViewCurrentRow; }

	virtual CString GetDescription (BOOL = TRUE) const;
	CString GetName (BOOL bStringName = FALSE) const;

	CLayout* GetChildObjects	() { return m_Objects.GetMasterObjects(); }
	CLayout* GetRowObjects	(int nRow);

	void	Attach	(BaseObj* pObj, BOOL bRepaint = FALSE);
	void	Detach	(BaseObj* pObj, BOOL bRepaint = FALSE);

	void	Rebuild		(CLayout*);

	virtual void	Draw			(CDC&, BOOL bPreview);
	virtual	CRect	GetRectToInvalidate ();

	virtual BOOL	AssignData		(WORD wID, RDEManager* pRDEmanager);
	virtual BOOL	ExecCommand		(WORD, RDEManager*);
	virtual	void	ResetCounters	();
	virtual	void	DisableData		();

	virtual	BOOL	Parse	(ViewParser&);
	virtual	void	Unparse	(ViewUnparser&);

	virtual	void	MoveBaseRect	(int x1, int y1, int x2, int y2, BOOL bRepaint = TRUE, BOOL bIgnoreBorder = FALSE);
	virtual	void	MoveBaseRect	(int xOffset, int yOffset, BOOL bIgnoreBorder = FALSE);
	virtual	void	MoveObjects		(int xOffset, int yOffset, BOOL bIgnoreBorder = FALSE);
	virtual	void	MoveObject		(CSize);
	
	virtual BOOL	DeleteEditorEntry ();
	virtual	void	ClearDynamicAttributes();

	virtual BOOL	ExistChildID	(WORD wID);
	FieldRect*		GetCellFromID	(int nRow, WORD wID);
	int				GetFieldRow		(BaseObj* pObj);

	BaseObj*		GetFieldByPosition			(const CPoint&, int& nRow);
	BaseObj*		GetMasterFieldByPosition	(const CPoint& pt);

	virtual BOOL OnShowPopup (CMenu&   menu);

	void	OnRebuild	();
	void	OnFitTheContent();
	void	OnProperties ();

	virtual BOOL	CanSearched () const { return FALSE; }
	virtual WORD	GetRDESearchID () const { return 0; }
	virtual void	ChangedAction ();

	void GetAnchoredFields(CArray<BaseRect*, BaseRect*>& arAnchoredFields);
	
	//implementation of baseobj's virtual method to update layout object
	void	Redraw();
	
	//TODO REPEATER 	virtual BOOL	GetSchema(CXSDGenerator*, WoormTable*) { return FALSE; }

	BOOL	Contains	(BaseObj* pObj);

	BOOL	IsEmptyRow	(int nRow);

	//virtual BOOL	CanDeleteField(LPCTSTR sName, CString& sLog) const;

	DECLARE_MESSAGE_MAP()
};

#include "endh.dex"