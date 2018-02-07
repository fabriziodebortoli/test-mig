#pragma once
#include <TbGes\DParametersDoc.h>
#include <TbGenlibUI\SettingsTableManager.h>
#include "beginh.dex"


extern const TB_EXPORT CTBNamespace snsTbDataSynchroClient;
extern const TB_EXPORT TCHAR szTbDataSynchroClient[];
extern const TB_EXPORT TCHAR szDataSynchronizerMonitor[];
extern const TB_EXPORT TCHAR szRefreshTimer[];
extern const TB_EXPORT TCHAR szWebMonitorUrl[];

/////////////////////////////////////////////////////////////////////////////
//							DDSSettings 
/////////////////////////////////////////////////////////////////////////////
class DDSSettings : public DCompanyUserSettingsDoc
{
	DECLARE_DYNCREATE(DDSSettings)

	// Construction
public:
	DDSSettings();

public:
	//DataSynchronizerMonitor
	DECLARE_SETTING(Int, RefreshTimer);
	DECLARE_SETTING(Str, WebMonitorUrl);

protected:
	virtual BOOL	OnAttachData();
	virtual BOOL	GetSaveSettings(BOOL bSave = FALSE);
	// json
	void	DeclareRegisterJson();


protected:
	//{{AFX_MSG(DBarcodeSettings)
	//}}AFX_MSG
	DECLARE_MESSAGE_MAP()

};

#include "endh.dex"
