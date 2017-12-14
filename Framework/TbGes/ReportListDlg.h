#pragma once

#include <TbGenlib\ParsObj.h>
#include <TbGeneric\mlistbox.h>

//includere alla fine degli include del .H
#include "beginh.dex"

class CAbstractFormDoc;
class CReportMenuNode;

//===========================================================================
class TB_EXPORT CListDocumentReportsDlg : public CParsedDialog
{
	DECLARE_DYNAMIC(CListDocumentReportsDlg)
protected:
	CMultiListBox		m_mlbReports;
	CReportMenuNode*&	m_pReports;
	CAbstractFormDoc*	m_pDoc;

public:
	CListDocumentReportsDlg(CAbstractFormDoc* pDoc, CReportMenuNode*& pReportRootNode, CWnd* = NULL);
	virtual ~CListDocumentReportsDlg();
	
protected:
	// standard dialog function
	virtual	BOOL	OnInitDialog	();
	virtual	void	OnOK			();
	virtual void	OnCancel		();

	//{{AFX_MSG(CListDocumentReportsDlg)
	afx_msg	void	OnReportSelected();
	//}}AFX_MSG


    DECLARE_MESSAGE_MAP()
};


//==========================================================================================
#include "endh.dex"