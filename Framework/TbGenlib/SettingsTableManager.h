#pragma once

#include <TbNameSolver\TBNamespaces.h>
#include <TbGeneric\Schedule.h>

#include "TbGenlibSettings.h"

//includere alla fine degli include del .H
#include "beginh.dex"

class DataObj;
class SettingsSection;
class DataObjArray;
class CCustomSaveInterface;

// questa classe consente di organizzare in sezioni i parametri
//-----------------------------------------------------------------------------
class TB_EXPORT CParameterInfo : public CObject
{
	DECLARE_DYNAMIC	(CParameterInfo)

public: 
	CParameterInfo(const CTBNamespace& aOwner, const CString& sFileName, const CString& sSection);

public:
	DataObj*	GetSettingValue	(
									const CString& sSetting,
									const DataObj& aDefault
								);
	void		SetSettingValue	(
									const CString& sSetting,
									const DataObj& aValue
								);

protected:
	CTBNamespace m_Owner;
	CString		 m_sFileName;
	CString		 m_sSection;
	BOOL		 m_bWrite;

protected:
	void BindParam	(const CString& sSetting, DataObj&		aValue);

public:
	BOOL WriteParameters();
	void ReadParameters	();

protected:
	virtual void BindParameters	() { };
};

// classe di interfaccia che ritorna le istruzioni di save selezionate
//=============================================================================
class TB_EXPORT CCustomSaveInterface : public CObject
{
public:
	// modalità di salvataggio
	enum CustomSaveMode
	{
		STANDARD,
		ALLCOMPANY_USERS,
		COMPANY_USERS,
	};

public:
	BOOL			m_bSaveAllFile;
	BOOL			m_bSaveAllUsers;
	CustomSaveMode	m_eSaveMode;
	CStringArray	m_aUsers;

public:
	CCustomSaveInterface();
};


//-----------------------------------------------------------------------------

class CSaveSettingsState
{
private:
	CStringArray	m_SavingState;
	Scheduler		m_Scheduler;

public:
	CSaveSettingsState();

public:
	void StartSave	(const CTBNamespace& aNs, const CString& sFile);
	void EndSave	(const CTBNamespace& aNs, const CString& sFile);

private:
	CString	GetKey	(const CTBNamespace& aNs, const CString& sFile);
	int		FindKey	(const CString& sKey);
};

// Metodi di settaggio e scrittura
//-----------------------------------------------------------------------------
TB_EXPORT BOOL	AFXAPI AfxSaveSettings	(
											const CTBNamespace&, 
											const CString& sFileName,	
											const CString& sSection, 
											const int& nRelease = 0,
											const BOOL bAskOnSave = FALSE,
											CCustomSaveInterface* pInterface = NULL
										);
TB_EXPORT BOOL	AfxSaveSettingsFile		(
											const CTBNamespace& aNamespace, 
											const CString& sFileName,
											const BOOL bAskOnSave = FALSE,
											CCustomSaveInterface* pInterface = NULL
										);

TB_EXPORT BOOL AfxSaveSettingsFile		(TbBaseSettings* pSettings,	const BOOL bAskOnSave);

//=============================================================================        
class TB_EXPORT CCustomSaveDialogObj
{
public:
	virtual void SetInterface		(CCustomSaveInterface* pInterface, CWnd* pParent) = 0;
	virtual void EnableAllCompanies	(const BOOL& bEnable) = 0;
	virtual int  ShowDialog			() = 0;

};

//=============================================================================        

//Macros da usare nei nella definizione del nome dei files di setting, delle sezioni e dei singoli nomi
#define _SET_FILE(ns)		_T(ns) //File di Settings
#define _SET_SECTION(ns)	_T(ns) //Sezione di Settings
#define _SET_NAME(ns)		_T(ns) //Nome di Settings
#include "endh.dex"
