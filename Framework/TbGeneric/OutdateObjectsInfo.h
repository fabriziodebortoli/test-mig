#pragma once

#include <TbNameSolver\TbNamespaces.h>
#include <TbGeneric\DataObj.h>
#include <TbGeneric\FunctionCall.h>

//includere alla fine degli include del .H
#include "beginh.dex"

// Oggetto OutDate generico. Serve per Reports, ecc..
//----------------------------------------------------------------
class TB_EXPORT COutDateObjectDescription : public CBaseDescription
{
	DECLARE_DYNCREATE(COutDateObjectDescription)

public:
	enum OutDateOperator { ND, LT, LE, EQ, GT, GE };

private:
	int				m_nRelease;
	OutDateOperator	m_Operator;

public:
	COutDateObjectDescription ();
	COutDateObjectDescription (CTBNamespace::NSObjectType aType);

public:
	const int&				GetRelease	() const { return m_nRelease; }
	const OutDateOperator&	GetOperator	() const { return m_Operator; }

	// metodi di settaggio
	void SetRelease		(const int& nRelease);
	void SetOperator	(const OutDateOperator& ope);

	BOOL IsOutDate		(const int& nRelease) const;
};

// Parameters Settings OutDates: 
//----------------------------------------------------------------
class TB_EXPORT COutDateSettingsSectionDescription : public COutDateObjectDescription
{
	DECLARE_DYNCREATE(COutDateSettingsSectionDescription)

private:
	CString					m_sOwner;
	CBaseDescriptionArray	m_Settings;

public:
	COutDateSettingsSectionDescription ();

public:
	const CString&			GetOwner	() const { return m_sOwner; }
	CBaseDescriptionArray&	GetSettings () { return m_Settings; }

	// metodi di settaggio
	void SetOwner		(const CString& sOwner);

	BOOL IsOutDateSetting(const CString& sSetting, const int& nRelease) const;
};

// OutDate Objects
//----------------------------------------------------------------
class TB_EXPORT COutDateObjectsDescription : CObject
{
	DECLARE_DYNCREATE(COutDateObjectsDescription)

	friend class CXMLOutDateObjectsParser;

private:
	CBaseDescriptionArray	m_arReportsInfo;
	CBaseDescriptionArray	m_arSettingsInfo;

public:
	COutDateObjectsDescription ();

public:
	void	Clear			();
	void	ClearSettings	();

	COutDateObjectDescription*	GetOutDateObjectInfo	(const CTBNamespace& sNamespace) const;

	const CBaseDescriptionArray&	GetReports	() const;
	const CBaseDescriptionArray&	GetSettings	() const;
};

#include "endh.dex"
