#pragma once

#include <TbNameSolver\TbNamespaces.h>
#include "DataObj.h"
#include "FunctionCall.h"
#include "FunctionObjectsInfo.h"

//includere alla fine degli include del .H
#include "beginh.dex"

class SqlTable;

// descrizione di una colonna di un HotKeyLink
//----------------------------------------------------------------
class TB_EXPORT CComboColumnDescription : public CObject
{
	DECLARE_DYNCREATE(CComboColumnDescription)

private:
	int		m_nLength;
	CString	m_sSource;
	CString	m_sLabel;
	CString	m_sNotLocalizedLabel;
	CString m_sWhen;
	CString m_sFormatter;

public:
	CComboColumnDescription ();
	CComboColumnDescription (const CComboColumnDescription& cd);

public:
	const int&		GetLength			();
	const CString&	GetSource			();
	const CString&	GetLabel			();
	const CString&	GetWhen				();
	const CString&	GetFormatter		();
	const CString&	GetNotLocalizedLabel();

	// metodi di settaggio
	void SetLength			(const int& nLength);
	void SetSource			(const CString& sSource);
	void SetLabel			(const CString& sLabel);
	void SetNotLocalizedLabel(const CString& sLabel);
	void SetWhen			(const CString& sWhen);
	void SetFormatter		(const CString& sFormatter);
	BOOL IsValid			() const;

public:
	CComboColumnDescription& operator=	(const CComboColumnDescription& cd);
	BOOL					operator!=	(const CComboColumnDescription& cd);
	BOOL					operator==	(const CComboColumnDescription& cd);
	
	void					Assign	(const CComboColumnDescription& cd);
	BOOL					IsEqual	(const CComboColumnDescription& cd);
};

//----------------------------------------------------------------
class TB_EXPORT QueryObjectBase : public  CObject
{
	DECLARE_DYNAMIC(QueryObjectBase)
public:
	virtual DataBool	Define			(DataStr sSql) = 0;
	virtual DataBool	Open			() = 0;
	virtual DataBool	Read			() = 0;
	virtual DataBool	Close			() = 0;
	virtual DataBool	Execute			() = 0;
	virtual DataBool	ReadOne			() = 0;

	virtual DataStr		GetColumnName	(DataInt col) = 0;
	virtual DataObj*	GetData			(DataStr sName) = 0;
	virtual DataStr		GetValue		(DataStr sName) = 0;

	virtual SqlTable* GetSqlTable() = 0;
};

// descrizione di un HotKeyLink
//----------------------------------------------------------------
class TB_EXPORT CHotlinkDescription : public CFunctionDescription
{
	DECLARE_DYNCREATE(CHotlinkDescription)
public:
	enum ESelectionType		{ NO_SEL, DIRECT, CODE, DESCRIPTION, COMBO, CUSTOM}; //clone di HotKeyLinkObj::SelectionType
	enum ESelectionMode		{ QUERY, REPORT, SCRIPT };

	class CSelectionType : public CObject
	{
		public:
			CString m_sName;
			CString m_sSelectionName;
			ESelectionType m_eType;
			CString m_sTitle;
			BOOL m_bVisible;

			CSelectionType
				(
					const CString& sName,
					const CString& sType,
					ESelectionType eType,
					const CString& sTitle,
					const BOOL bVisible = TRUE
				)
				:
					m_sName		(sName),
					m_sSelectionName(sType),
					m_eType		(eType),
					m_sTitle	(sTitle),
					m_bVisible	(bVisible)
				{}

			CSelectionType
				(
					const CSelectionType& src
				)
				:
					m_sName		(src.m_sName),
					m_sSelectionName(src.m_sSelectionName),
					m_eType		(src.m_eType),
					m_sTitle	(src.m_sTitle),
					m_bVisible(src.m_bVisible)
				{}
	};

	class CSelectionMode : public CObject
	{
		public:
			CString			m_sName;
			ESelectionMode	m_eMode;
			CString			m_sBody;

			QueryObjectBase* m_pQuery;

			CSelectionMode
				(
					const CString&	sName,
					ESelectionMode	eMode,
					CString			sBody
				)
				:
					m_sName		(sName),
					m_eMode		(eMode),
					m_sBody		(sBody),
					m_pQuery	(NULL)
				{}

			CSelectionMode
				(
					const CSelectionMode&	src
				)
				:
					m_sName		(src.m_sName),
					m_eMode		(src.m_eMode),
					m_sBody		(src.m_sBody),
					m_pQuery	(NULL)
				{}

			virtual ~CSelectionMode()
			{
				SAFE_DELETE(m_pQuery)
			}

	};

protected:
	CString m_sDbField;
	BOOL	m_bHasComboBox;
	Array	m_arComboBox;

	CString m_sDbTable;
	CString m_sDbFieldDescription;

	CString m_sCallLink;
	BOOL	m_bAddOnFlyEnabled;
	BOOL	m_bMustExistData;
	BOOL	m_bSearchOnLinkEnabled;

	BOOL	m_bLoadFullRecord;

	CString	m_sAskDialogs;
	
	CString m_sDatafile;
	CString m_sClassName;
public:
	Array	m_arSelectionTypes;
	Array	m_arSelectionModes;

	CFunctionObjectsDescription		m_EventsInfo;

	CHotlinkDescription ();
	CHotlinkDescription (const CHotlinkDescription& hd);

public:
	BOOL IsDynamic() const { return m_arSelectionModes.GetSize(); }
	BOOL IsXml() const { return !m_sDatafile.IsEmpty(); }

	void Clear	();

	virtual const CString GetTitle				() const;

	const CString&		GetDbField				() const;
	void				SetDbField				(const CString& sField);
	const CString&		GetDbTable				() const;
	void				SetDbTable				(const CString& sTable);
	const CString&		GetDbFieldDescription	() const;
	void				SetDbFieldDescription	(const CString& sField);
	const CString&		GetClassName			() const { return m_sClassName; }
	void				SetClassName			(const CString& sName){ m_sClassName = sName; }
	
	BOOL 				HasComboBox				() const;
	void				SetHasComboBox			(BOOL bValue);

	Array&				GetComboBox				();
	void				AddComboColumn			(CComboColumnDescription* pDescri);

	const CString&		GetCallLink				() const;
	void				SetCallLink				(const CString& sTable);

	BOOL				IsAddOnFlyEnabled		() const						{ return m_bAddOnFlyEnabled; }
	void				SetAddOnFlyEnabled		(BOOL bValue)					{ m_bAddOnFlyEnabled = bValue; }

	BOOL				IsMustExistData			() const						{ return m_bMustExistData; }
	void				SetMustExistData		(BOOL bValue)					{ m_bMustExistData = bValue; }

	BOOL				IsSearchOnLinkEnabled	() const						{ return m_bSearchOnLinkEnabled; }
	void				SetSearchOnLinkEnabled	(BOOL bValue)					{ m_bSearchOnLinkEnabled = bValue; }

	BOOL				IsLoadFullRecord		() const						{ return m_bLoadFullRecord; }
	void				SetLoadFullRecord		(BOOL bValue)					{ m_bLoadFullRecord = bValue; }

	const CString&		GetAskDialogs			() const						{ return m_sAskDialogs; }
	void				SetAskDialogs			(const CString& sAskDialogs)	{ m_sAskDialogs = sAskDialogs; }

	const CString&		GetDatafile				() const						{ return m_sDatafile; }
	void				SetDatafile				(const CString& sDatafile)		{ m_sDatafile = sDatafile; }

public:
	CHotlinkDescription&	operator=	(const CHotlinkDescription& hd);
	BOOL					operator!=	(const CHotlinkDescription& hd);
	BOOL					operator==	(const CHotlinkDescription& hd);
	
	void					Assign		(const CHotlinkDescription& hd);
	BOOL					IsEqual		(const CHotlinkDescription& hd);

	CHotlinkDescription::CSelectionMode* GetSelectionMode (CHotlinkDescription::ESelectionType, int nCustomCode = -1);

	static CString	s_SelectionType_Name;
	static CString	s_FilterValue_Name;
	static CString	s_SelectionType_Direct;
	static CString	s_SelectionType_Combo;
	static CString	s_SelectionType_Code;
	static CString	s_SelectionType_Description;
	static CString	s_SelectionType_Custom;

	static CString	s_ModeType_Query;
	static CString	s_ModeType_Script;
	static CString	s_ModeType_Report;
};

// ReferenceObjects
//----------------------------------------------------------------
class TB_EXPORT CReferenceObjectsDescription : CObject
{
	DECLARE_DYNCREATE(CReferenceObjectsDescription)

	bool					m_bLoaded;

public:
	CBaseDescriptionArray	m_arHotLinks;

public:
	CReferenceObjectsDescription ();

public:
	bool		IsLoaded () { return m_bLoaded; }
	void		SetLoaded(bool bValue) { m_bLoaded = bValue; }


	CHotlinkDescription*	GetHotlinkInfo	(const CTBNamespace& aNamespace) const;
	const CBaseDescriptionArray&	GetHotLinks		() const;

	// metodi di settaggio
	void	AddHotlink		(CHotlinkDescription* pDescri);
	void	RemoveHotlink (CString tableName);
};

#include "endh.dex"
