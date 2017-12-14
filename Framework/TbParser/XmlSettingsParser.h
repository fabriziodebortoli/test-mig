#pragma once

#include "XmlBaseDescriptionParser.h"

//includere alla fine degli include del .H
#include "beginh.dex"

class COutDateSettingsSectionDescription;
class CStatusBarMsg;

// grammatica degli oggetti parametri
//----------------------------------------------------------------
class TB_EXPORT XMLSettingsParser : public CObject
{
	DECLARE_DYNAMIC(XMLSettingsParser)

private:
	CXMLDocumentObject*	m_pXmlDocument;

	// per evitarmi tonnellate di parametri
	CTBNamespace					m_Owner;
	CBaseDescriptionArray			m_OutDates;
	CString							m_sFileName;
	DataDate						m_DataFile;

	// parametri di save
public:
	SettingObject::SettingSource	m_Source;

private:
	BOOL	m_bEmpty;

public:
	XMLSettingsParser	();
	~XMLSettingsParser	();

public:
	BOOL LoadSettings	(
							const CTBNamespace& aModule, 
							const CPathFinder*	pPathFinder, 
							const CString&		sUser, 
							const CBaseDescriptionArray& aOutDates,
							CStatusBarMsg*		pStatusBar
						);
	BOOL Unparse		(
							const CPathFinder*	pPathFinder, 
							const CTBNamespace& aModule,
							const CString&		sFileName, 
							const CString&		sSection, 
							const int&			nRelease, 
							const CString&		sUser,
							SettingsTable*		pSettingsTable
						);
	BOOL RefreshTable	(
							const CPathFinder*	pPathFinder, 
							const CTBNamespace& aModule, 
							const CString&		sFileName,
							const CString&		sUser,
							SettingsTablePtr	pSettingsTable,
							BOOL				bOnlyModified = TRUE
						);
private:
	void LoadFiles		(const CString sDir);

	BOOL Parse			(CXMLDocumentObject*, SettingsTablePtr, const CString& sFile);

	BOOL ParseSection	(CXMLNode*, SettingsSection*);
	BOOL ParseSetting	(CXMLNode*, SettingObject*, const CString& sSectionName);

	void UnparseSetting	(CXMLNode*, SettingObject*,		BOOL bToUpdate);
	void UnparseSection	(CXMLNode*, SettingsSection*,	BOOL bToUpdate);

	BOOL IsOutDated		(const CString sSectionName, const int& nRelease);
	BOOL IsOutDated		(const CString sSectionName, const CString sSettingName, const int& nRelease);

	COutDateSettingsSectionDescription* GetOutDatedSection(const CString sSectionName);

	DataDate		GetFileDate			(const CString& sFileName);
	BOOL			IsModifiedFile		(
											const CTBNamespace&					aModule, 
											const CString&						sFileName, 
											const SettingObject::SettingSource	aFrom,
											SettingsTablePtr					pSettingsTable
										);
	SettingObject*	GetSettingToUnparse (SettingObjects* pSettings, BOOL bToUpdate);
};

#include "endh.dex"
