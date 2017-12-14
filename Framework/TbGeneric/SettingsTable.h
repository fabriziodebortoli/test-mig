#pragma once

#include <TbNameSolver\TBNamespaces.h>
#include <TbGeneric\Array.h>
#include <TbGeneric\FunctionCall.h>

//includere alla fine degli include del .H
#include "beginh.dex"

class CTBNamespace;
class DataType;
class DataObj;
class DataObjArray;

class SettingsTable;
DECLARE_SMART_LOCK_PTR(SettingsTable)

typedef int	FormatIdx;



// caratteristiche di un singolo settaggio
//============================================================================
class TB_EXPORT SettingObject : public CDataObjDescription
{
	DECLARE_DYNCREATE(SettingObject)

	enum SettingSource 
		{
			CURRENT_VALUE,
			DEFAULT_VALUE,
			FROM_STANDARD, 
			FROM_ALLCOMPANYUSERS, 
			FROM_ALLCOMPANYUSER, 
			FROM_COMPANYUSERS, 
			FROM_COMPANYUSER 
		};

private:
	SettingSource	m_Source;
	int				m_nRelease;
	BOOL			m_bDeleted;

public:
	SettingObject	();

public:
	const int&				GetRelease		() const;
	const SettingSource&	GetSource		() const;
	const BOOL&				IsDeleted		() const;

	// metodi di settaggio
	void SetSource		(const SettingSource& aSource);
	void SetRelease		(const int& nRelease);
	void SetDeleted		(const BOOL& bValue);

public:
	SettingObject*	Clone	() const;
	void			Assign	(const SettingObject& so);

	const SettingObject& operator= (const SettingObject& so);
};

// possibili configurazioni di un settaggio a seconda dalle varie sorgenti
//============================================================================
class TB_EXPORT SettingObjects : public Array
{
	DECLARE_DYNCREATE(SettingObjects)

private:
	CString		m_sSettingName;

public:
	SettingObjects ();
	SettingObjects (const CString& sSettingName);
	SettingObjects (const SettingObjects&);

public:
	SettingObject*	GetAt				(int nIdx) const;
	const CString&	GetName				() const;
	SettingObject*	GetSetting			();
	SettingObject*	GetSetting			(SettingObject::SettingSource aFrom);
	SettingObject*	GetOriginalSetting	();

	const BOOL			IsDeleted		();

	// metodi di set
	void AddSetting			(SettingObject* pSetting);
	void SetName			(const CString& sName);
	void SetModified		(const BOOL& bValue);
	void SetSettingValue	(const CString& sSettingName, const DataObj& aValue, const int nRelease = 0, BOOL bIsDefault = FALSE);
	void SetCurrentValue	();
	void SetDeleted			(const BOOL& bValue);

public:
	SettingObjects* Clone	() const;
	void		    Assign	(const SettingObjects& set);

	// operatori
	const SettingObjects& operator= (const SettingObjects& source);
};

// caratteristiche di una sezione di settaggi
//============================================================================
class TB_EXPORT SettingsSection : public CObject
{
friend class SettingsGroup;
friend class SettingsTable;
friend class XMLSettingsParser;

	DECLARE_DYNCREATE(SettingsSection)

protected:
	CString			m_sSectionName;
	CString			m_sFileName;	// identificano la provenienza
	CTBNamespace	m_Owner;
	int				m_nRelease;

	// data di ultima modifica dei files comuni mentre
	// quelli per utente saranno ricaricati sempre.
	DataDate		m_LastFileDateStandard;	
	DataDate		m_LastFileDateAllComUsers;
	DataDate		m_LastFileDateCompanyUsers;

private:
	Array			m_Settings;			
	
public:
	SettingsSection	();
	~SettingsSection();

public:
	const Array&		GetSettings	() const;

private:
	SettingsSection*	GetExactSection		(const CTBNamespace& aOwner, const CString& sFileName, const CString& sSection);
	SettingObject*		GetSetting			(const CString& sSettingName, BOOL bOriginal = FALSE);
	SettingObject*		GetSetting			(const CString& sSettingName, const SettingObject::SettingSource aFrom);
	DataObj*			GetSettingValue		(const CString& sSettingName, const DataObj& aDefault);
	const DataDate&		GetLastFileDate		(const SettingObject::SettingSource aFrom) const;
	
	const CString&		GetName			() { return m_sSectionName; }
	const CString&		GetFileName		() { return m_sFileName; }
	const CTBNamespace&	GetOwner		() { return m_Owner; }
	const int&			GetRelease		() { return m_nRelease; }

	const BOOL			IsDeleted		();

	// metodi di settaggio
	void	SetName				(const CString& sName);
	void	SetFileName			(const CString& sName);
	void	SetOwner			(const CTBNamespace& aNamespace);
	void	SetRelease			(const int& nRelease);
	void	SetModified			(const BOOL& bValue);
	void	SetLastFileDate		(const SettingObject::SettingSource aFrom, const DataDate& bDate);

	void	AddSetting			(SettingObject* pSetting);
	void	SetSettingValue		(const CString& sSettingName, const DataObj& aValue, const int nRelease = 0, BOOL bIsDefault = FALSE);
	void	ClearSettings		(const BOOL& bAllCommons, const BOOL& bSingleUser);
	void	SetDeleted			(const BOOL& bValue);
	
	void	UpdateCurrentValues	();


	SettingsSection* Clone	(BOOL bWithSettings = TRUE) const;
	void			 Assign	(const SettingsSection& set, BOOL bWithSettings = TRUE);
public:
	const SettingsSection& operator= (const SettingsSection& source);
};

// I gruppi di settings sono suddivisioni per applicazione e servono per 
// ottimizzare ricerche e salvataggio. Ogni gruppo è un array di SettingsSection
//============================================================================
class TB_EXPORT SettingsGroup : public Array
{
	DECLARE_DYNCREATE(SettingsGroup)

public:
	CString		m_sGroup;

public:
	SettingsGroup ();
	SettingsGroup (const CString& sGroup);
	SettingsGroup (const SettingsGroup&);

public:
	SettingsSection*	GetAt			(int nIdx) const;
	SettingsSection*	GetExactSection	(const CTBNamespace&, const CString& sFileName, const CString& sSection);
	SettingsSection*	GetBestSection	(const CTBNamespace&, const CString& sSection, const LPCTSTR sFileName = NULL);
	const CString&		GetGroup		();
	void				GetFileSections	(const CTBNamespace&, const CString& sFileName, Array& aSections);

public:
	SettingsGroup* Clone	() const;
	void		   Assign	(const SettingsGroup& set);

	// operatori
	const SettingsGroup& operator= (const SettingsGroup& source);
};


//============================================================================
class TB_EXPORT SettingsTable : public Array, public CTBLockable
{
public:
	SettingsTable ();
	SettingsTable (const SettingsTable&);

	DataObj*			GetSettingValue	(const CTBNamespace&, const CString& sSection, const CString& sSetting, const DataObj& aDefault, const LPCTSTR sFileName = NULL);
	DataObj*			GetSettingValue	(const CTBNamespace&, const CString& sSection, const CString& sSetting, const LPCTSTR sFileName = NULL);
	void SetSettingValue		(
									const CTBNamespace& aNamespace, 
									const CString& sFileName,
									const CString& sSection, 
									const CString& sSettingName, 
									const DataObj& aValue, 
									const int nRelease = 0
								);
public:
	SettingsGroup*				GetAt			(int nIdx) const;
	SettingsGroup*				GetGroup		(const CTBNamespace&);
	const SettingsSection*		GetBestSection	(const CTBNamespace&, const CString& sSection, const LPCTSTR sFileName = NULL);
	SettingsSection*			GetExactSection	(const CTBNamespace&, const CString& sFileName, const CString& sSection);

	// ritornano le sezioni di un file
	void	GetFileSections		(const CTBNamespace&, const CString& sFileName, Array& aSections);
	void	GetFileSectionsNames(const CTBNamespace&, const CString& sFileName, CStringArray& aSectionsNames, BOOL bExcludeDeleted = FALSE);

	// metodi di settaggio
	SettingsSection* AddSection	(
									const CTBNamespace& aNamespace, 
									const CString& sFileName,
									const CString& sSection, 
									const int& nRelease = 0
								);
	void AddSection				(SettingsSection*);
	void RemoveSection			(SettingsSection*);
	void SetModified			(const CTBNamespace& aNamespace, const BOOL& bValue);

	DataDate GetLastFileDate	(
									const CTBNamespace& aNamespace,
									const CString& sFileName,
									SettingObject::SettingSource aFrom
								);
	void UpdateCurrentValuesOf	(
									const CTBNamespace&	aModule, 
									const CString&		sFileName, 
									const CString&		sSection 
								);

	void CopyCurrentValuesFrom	(
									SettingsTablePtr	pToTable,
									const CTBNamespace&	aModule, 
									const CString&		sFileName, 
									const CString&		sSection 
								);

	void ClearSettingsOf		(
									const CTBNamespace&	aModule, 
									const CString&		sFileName, 
									const CString&		sSection, 
									const BOOL&			bAllCommons,
									const BOOL&			bSingleUser
								);
public:
	// operatori
	const SettingsTable& operator= (const SettingsTable& source);

	//for lock tracer
	virtual LPCSTR			GetObjectName() const { return "SettingsTable"; }
};


// General Functions
//-----------------------------------------------------------------------------
TB_EXPORT SettingsTablePtr AFXAPI AfxGetSettingsTable	();

TB_EXPORT DataObj* AFXAPI AfxGetSettingValue		(
														const CTBNamespace& aNs,
														const CString& sSection, 
														const CString& sSetting,
														const DataObj& aDefault,
														const LPCTSTR sFileName = NULL
													);

TB_EXPORT DataObj* AFXAPI AfxGetSettingValue		(
														const CTBNamespace& aNs, 
														const CString& sSection, 
														const CString& sSetting, 
														const LPCTSTR sFileName = NULL
													);

TB_EXPORT void AFXAPI AfxSetSettingValue			(
														const CTBNamespace& aNs,
														const CString& sSection, 
														const CString& sSetting,
														const DataObj& aValue,
														const LPCTSTR sFileName = NULL,
														const int nRelease = 0
													);
TB_EXPORT SettingsSection* AFXAPI AfxCreateSettingsSection
													(
														const CTBNamespace& aNs, 
														const CString& sFileName,
														const CString& sSection, 
														const int& nRelease = 0
													);
TB_EXPORT void AfxRemoveSettingsSection				(
														const CTBNamespace& aNs, 
														const CString& sFileName,
														const CString& sSection
													);

TB_EXPORT const SettingsSection* AfxGetBestSection	(
														const CTBNamespace&, 
														const CString& sSection, 
														const LPCTSTR sFileName = NULL
													);

//=============================================================================        
#include "endh.dex"
