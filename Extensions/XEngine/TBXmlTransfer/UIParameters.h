#pragma once

#include <TbGeneric\dataobj.h>
#include <TbGeneric\dibitmap.h>

#include <TbGenlib\parsctrl.h>
#include <TbGenlib\TBPropertyGrid.h>
#include <TbGenlib\BaseDoc.h>

#include <TbGes\extdoc.h>
#include <TbGes\tabber.h>
#include <TbGes\TileDialog.h>
#include <TbGes\TileManager.h>

#include <TbOledb\sqlrec.h>

//includere alla fine degli include del .H
#include "beginh.dex"

//////////////////////////////////////////////////////////////////////////////
//						 CXEParametersView
//////////////////////////////////////////////////////////////////////////////
class CXEParametersView : public CMasterFormView
{
	DECLARE_DYNCREATE(CXEParametersView)
	
public:	
	CXEParametersView();
	
public:		
	DXEParameters*	GetDocument() const { return (DXEParameters*) __super::GetDocument(); }
	TXEParameters* 	GetXEParameters() const { return GetDocument()->GetXEParameters(); }

public:	
	virtual	void BuildDataControlLinks();
};

/////////////////////////////////////////////////////////////////////////////
//						CXEParametersTileGroup
/////////////////////////////////////////////////////////////////////////////
class TB_EXPORT CXEParametersTileGroup : public CTileGroup
{
	DECLARE_DYNCREATE(CXEParametersTileGroup)

protected:
	virtual	void Customize();
};

/////////////////////////////////////////////////////////////////////////////
//						CXEParametersTileDialog
/////////////////////////////////////////////////////////////////////////////
class CXEParametersTileDialog : public CTileDialog
{
	DECLARE_DYNCREATE(CXEParametersTileDialog)

public:
	CXEParametersTileDialog();

	virtual	void BuildDataControlLinks();

public:
	DXEParameters*	GetDocument() const { return (DXEParameters*)m_pDocument; }
	TXEParameters* 	GetXEParameters() const { return GetDocument()->GetXEParameters(); }
};

/////////////////////////////////////////////////////////////////////////////
//					CXEParametersPropertyGrid
/////////////////////////////////////////////////////////////////////////////
class TB_EXPORT CXEParametersPropertyGrid : public CTBPropertyGrid
{
	DECLARE_DYNCREATE(CXEParametersPropertyGrid)

public:
	CXEParametersPropertyGrid();

public:
	DXEParameters*	GetXEParametersDoc() { return (DXEParameters*)GetDocument();  }
	TXEParameters* 	GetXEParameters()  { return GetXEParametersDoc()->GetXEParameters(); }

protected:
	virtual void OnCustomize();
};

#include "endh.dex"