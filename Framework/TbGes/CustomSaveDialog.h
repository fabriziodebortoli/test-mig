#pragma once

#include <TbGeneric\DataObj.h>
#include <TbGeneric\SettingsTable.h>

#include <TBGenlib\ParsObj.h>
#include <TBGenlib\BaseTileDialog.h>
#include <TBGenlib\ParsBtn.h>
#include <TBGenlib\PARSLBX.H>
#include <TBGenlib\SettingsTableManager.h>

//includere alla fine degli include del .H
#include "beginh.dex"

class CCustomSaveInterface;
//============================================================================
class TB_EXPORT CCustomSaveDialog : public CParsedDialogWithTiles,  public CCustomSaveDialogObj
{
	DECLARE_DYNCREATE(CCustomSaveDialog);

protected:
	CBoolButton	m_bStandard;
	CBoolButton	m_bCurrentUser;
	CBoolButton	m_bAllUsers;
	CBoolButton	m_bSelUsers;
	CBoolButton	m_bCurrentCompany;
	CBoolButton	m_bAllCompanies;
	CTBListBox	m_lbxUsers;

private:
	CCustomSaveInterface*				m_pSelections;
	BOOL								m_bAllCompaniesEnabled;
	CMap<HWND, HWND, BOOL, BOOL>		m_MapCtr;

public:
	CCustomSaveDialog(CCustomSaveInterface* pInerface, CWnd* pParent);
	CCustomSaveDialog();

public:
	virtual void EnableAllCompanies(const BOOL& bEnable);
	virtual void SetInterface(CCustomSaveInterface* pInterface, CWnd* pParent);
	virtual int ShowDialog();

private:
	void	FillUsers			();
	void	InitDefaults		();
	BOOL	CheckSelections		();

protected:
	virtual	BOOL OnInitDialog	();
	virtual	void OnOK			();

protected:
	afx_msg void	OnModeChanged	();
	afx_msg HBRUSH	OnCtlColor(CDC* pDC, CWnd* pWnd, UINT nCtlColor);

	DECLARE_MESSAGE_MAP();
};

#include "endh.dex"
