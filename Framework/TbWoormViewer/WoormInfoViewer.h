
#pragma once

#include <TbGenlib\parsobj.h>

#include "WoormDoc.h"


//includere alla fine degli include del .H
#include "beginh.dex"

//===========================================================================

/////////////////////////////////////////////////////////////////////////////
// 				class WoormInfoViewer 
/////////////////////////////////////////////////////////////////////////////
//
class TB_EXPORT WoormInfoViewer : public CParsedDialog
{
	DECLARE_DYNAMIC(WoormInfoViewer)
private:
	CListCtrl 		m_ReportList; 
	CWoormInfo*		m_pWoormInfo;
	SymTable*		m_pSymTable;

public:
	WoormInfoViewer(CWoormInfo* pWoormInfo, SymTable*);
	~WoormInfoViewer();

private:
	void	FillReportList			();	

protected:
	virtual	BOOL	OnInitDialog	();

	DECLARE_MESSAGE_MAP()
};

/////////////////////////////////////////////////////////////////////////////
// 				class ObjectsList 
/////////////////////////////////////////////////////////////////////////////
//
class TB_EXPORT ObjectsList : public CParsedDialog
{
	DECLARE_DYNAMIC(ObjectsList)
private:
	CListCtrl 		m_ReportList; 
	CWoormDocMng*		m_pWoorm;

public:
	ObjectsList(CWoormDocMng* pWoorm);
	~ObjectsList();

private:
	void	FillReportList			();	

protected:
	virtual	BOOL	OnInitDialog	();

	DECLARE_MESSAGE_MAP()
};

#include "endh.dex"

