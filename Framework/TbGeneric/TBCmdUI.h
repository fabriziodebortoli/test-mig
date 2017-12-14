#pragma once

#include "beginh.dex"

class TB_EXPORT CTBCmdUI :
	public CCmdUI
{
protected:
	enum UpdateType 
	{
		UpdateNone		= 0,
		UpdateEnable	= 1,
		UpdateSetCheck	= 2,
		UpdateSetRadio	= 4,
		UpdateSetText	= 8 };
		
	int m_UptateType;

	BOOL	m_bEnabled;
	BOOL	m_bRadio;
	int		m_nCheck;
	CString	m_strText;

public:
	CTBCmdUI(int commandID);
	~CTBCmdUI(void);

	virtual void Enable(BOOL bOn = TRUE);
	virtual void SetCheck(int nCheck = 1);   // 0, 1 or 2 (indeterminate)
	virtual void SetRadio(BOOL bOn = TRUE);
	virtual void SetText(LPCTSTR lpszText);

	BOOL	GetEnabled()	{ return m_bEnabled;	}
	BOOL	GetRadio()		{ return m_bRadio;		}
	int		GetCheck()		{ return m_nCheck;		}
	const CString& GetText(){ return m_strText;		}
};
#include "endh.dex"