
#pragma once

#include "Extdoc.h"

#include "beginh.dex"

class CInspectorDialog;

//=============================================================================
class CDFieldInspector : public CClientDoc 
{
	DECLARE_DYNCREATE(CDFieldInspector)
	
public:
	CDFieldInspector();
	~CDFieldInspector();

public:
	virtual BOOL OnAttachData();

private:
	CInspectorDialog*	m_pInspectorDialog;

public:
	virtual WebCommandType OnGetWebCommandType(UINT commandID);

protected:
	//{{AFX_MSG(CDFieldInspector)
	afx_msg void OnInspect	();
	//}}AFX_MSG
	DECLARE_MESSAGE_MAP()
};

#include "endh.dex"
