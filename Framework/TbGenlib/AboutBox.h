#pragma once

#include <TbGeneric\DiBitmap.h>

//includere alla fine degli include del .H
#include "beginh.dex"

//=============================================================================
// 				class CAboutBox dialog
//=============================================================================
class TB_EXPORT CAboutBox : public CParsedDialog
{
	DECLARE_DYNAMIC(CAboutBox)
protected:
	CEdit*			m_pAddOnVersions;
	CTranspBmpCtrl*	m_pImage;

// Construction
public:
	CAboutBox (CWnd* pParent = NULL);
	virtual ~CAboutBox();

private:
	void	FillAddOnList		();

// Implementation
protected:
	virtual	BOOL OnInitDialog	();
	virtual	void OnOK			();

	// Generated message map functions
	//{{AFX_MSG(CAboutBox)
	//}}AFX_MSG
	DECLARE_MESSAGE_MAP()

// diagnostics
#ifdef _DEBUG
public:	
	void Dump(CDumpContext&) const;
	void AssertValid() const;
#endif

};

#include "endh.dex"
