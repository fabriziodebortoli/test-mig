#pragma once

#include <TbGenlib\Parsctrl.h>
#include <TbGes\TileManager.h>
#include <TbGes\TileDialog.h>

#include "CommonObjects.h"

#include "beginh.dex"

class CDDMS;

/////////////////////////////////////////////////////////////////////////////
//				class CPaperyParsedDlg Definition
/////////////////////////////////////////////////////////////////////////////
//
class TB_EXPORT CPaperyParsedDlg : public CParsedDialogWithTiles
{
	DECLARE_DYNAMIC(CPaperyParsedDlg)

public:
	CPaperyParsedDlg(CWnd* pParent, CDDMS* pDMSClientDoc);

private:
	CDDMS*	m_pDMSClientDoc;

public:
	DataStr	m_BarcodeValue;
	DataStr	m_Notes;

public:
	virtual BOOL OnInitDialog	();
	virtual void OnOK			();
	virtual void OnCancel		();

protected:
	//{{AFX_MSG(CPaperyDlg)
	afx_msg void OnBarcodeValueChanged();
	//}}AFX_MSG

	DECLARE_MESSAGE_MAP()
};

#include  "endh.dex"