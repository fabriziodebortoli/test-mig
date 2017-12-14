
#pragma once


#include "XMLEnvMng.h"

#include <TBGeneric\array.h>
#include <TBGenlib\TbTreeCtrl.h>

//includere alla fine degli include del .H
#include "beginh.dex"



// tree comune per la visualizzazione degli envelope 
// per dare l'opportunità all'utente di effettuare la propria scelta
////////////////////////////////////////////////////////////////////
//	Class CEnvelopeTree
////////////////////////////////////////////////////////////////////
//------------------------------------------------------------------
class TB_EXPORT CEnvelopeTree : public CTBTreeCtrl
{
public:
	CEnvelopeTree(CXMLEnvClassArray*, BOOL bMultiSelect = TRUE);

public:
	BOOL				FillTree			();
	virtual void		SelectAll			(HTREEITEM, UINT, BOOL bIsRoot=TRUE);
	BOOL				GetEnvSelArray		(CXMLEnvElemArray*);

protected:
	CXMLEnvClassArray* m_pXMLEnvClassArray;

protected:
	void GetSelectedHItems(CHTreeItemsArray* pHItemsArray, HTREEITEM hItem);
	void DeselectAll();

	//{{AFX_MSG(CRxEnvelopeTree)
	afx_msg void OnLButtonDown(UINT nFlags, CPoint point);
	//}}AFX_MSG

	DECLARE_MESSAGE_MAP()	
};


// tree per la visualizzazione degli envelope ricevuti
// per dare l'opportunità all'utente di effettuare la propria scelta
////////////////////////////////////////////////////////////////////
//	Class CRxEnvelopeTree
////////////////////////////////////////////////////////////////////
//------------------------------------------------------------------
class TB_EXPORT CRxEnvelopeTree : public CEnvelopeTree
{
public:
	CRxEnvelopeTree(CXMLEnvClassArray*, BOOL bMultiSelect = TRUE);

public:
	BOOL				FillTree			();
	virtual	void		InitializeImageList	();
};

// tree per la visualizzazione degli envelope da inviare
// per dare l'opportunità all'utente di effettuare la propria scelta
////////////////////////////////////////////////////////////////////
//	Class CTxEnvelopeTree
////////////////////////////////////////////////////////////////////
//------------------------------------------------------------------
class TB_EXPORT CTxEnvelopeTree : public CEnvelopeTree
{
public:
	CTxEnvelopeTree(CXMLEnvClassArray*);

public:
	BOOL				FillTree			();
	virtual	void		InitializeImageList	();

};

#include "endh.dex"

