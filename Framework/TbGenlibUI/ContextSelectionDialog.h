#pragma once

#include <TbGeneric\DataObj.h>

#include <TbGenlib\ParsObj.h>
#include <TbGenlib\ParsLbx.h>
#include <TbGenlib\ParsBtn.h>

//includere alla fine degli include del .H
#include "beginh.dex"

//============================================================================
class TB_EXPORT CContextSelectionDialog : public CParsedDialog
{
	DECLARE_DYNAMIC(CContextSelectionDialog)
protected:
	CBoolButton				m_bAll;
	CBoolButton				m_bSelect;
	CBCGPComboBox			m_cbxApplications;
	CResizableListBox		m_lbxModules;
	CResizableListBox		m_lbxSelected;

private:
	CStringArray*	m_pSelections;
	CString			m_sAllAppString;

public:
	CContextSelectionDialog (CStringArray* pSelections, CWnd* aParent = NULL);

private:
	void	InitDefaults			();
	void	CheckSelectionsToAdd	(CStringArray& arSelections);

	void	FillApplications		();
	void	FillModules				(const CString& sAppName);

	void	DoApplicationChanged	();
	void	DoModulesChanged		();
	void	DoAllSelectedChanged	();
	void	DoAddSelection			();
	void	DoRemoveSelection		();

	void	EnableAddRemoveButtons	();
	void	RefreshSelectedScroll	();

	CString	GetCurrentApplication	();
	CString GetContextArea			(const CString& sSelection);

	CString	FormatSelected			(const CString& sAppName, const CString& sModName);
	void	UnFormatSelected		(const CString& sSelection, CString& sAppName, CString& sModName);

protected:
	virtual	BOOL OnInitDialog	();
	virtual	void OnOK			();

protected:
	afx_msg void OnApplicationChanged	();
	afx_msg void OnModulesChanged		();
	afx_msg void OnSelectedChanged		();
	afx_msg void OnAllSelectChanged		();
	afx_msg void OnAddSelection			();
	afx_msg void OnRemoveSelection		();

	DECLARE_MESSAGE_MAP();
};

#include "endh.dex"
