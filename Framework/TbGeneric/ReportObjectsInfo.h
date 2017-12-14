#pragma once

#include <TbNameSolver\TbNamespaces.h>
#include <TbGeneric\DataObj.h>
#include <TbGeneric\FunctionCall.h>

//includere alla fine degli include del .H
#include "beginh.dex"

//----------------------------------------------------------------
class TB_EXPORT CReportMenuNode : public CObject
{
	DECLARE_DYNCREATE(CReportMenuNode)

protected:
	CString				m_sNodeTag;
	BOOL				m_bUseSubMenu;
	Array*				m_pSons;
	BOOL				m_bVisible;

public:
	CReportMenuNode*	m_pFather;

public:
			CReportMenuNode	();
	virtual ~CReportMenuNode();

public:
	CString				GetNodeTag				();
	void				SetNodeTag				(CString sNodeTag);
	BOOL				GetUseSubMenu			();
	void				SetUseSubMenu			(BOOL bUseSubMenu);
	BOOL				IsVisible				();
	void				SetVisible				(BOOL bVisible = TRUE);
	BOOL				IsLeaf					();
	BOOL				IsRoot					();
	int					GetMaxDepth				();

	int					GetSonsUpperBound		();
	CReportMenuNode*	GetSonAt				(int nSonIdx);
	void				AddSon					(CReportMenuNode* pSonToAdd);
	BOOL				RemoveSonAt				(int nSonIdx);
	void				RemoveAllSons			();
	void				InsertSonAt				(int nSonIdx, CReportMenuNode* pSonToAdd);
};

//----------------------------------------------------------------
class TB_EXPORT CDocumentReportGroupDescription : public CObject
{
	DECLARE_DYNCREATE(CDocumentReportGroupDescription)

protected:
	int			m_nId;
	CString		m_sLocalize;
	BOOL		m_bUseSubMenu;

public:
	CDocumentReportGroupDescription();
	CDocumentReportGroupDescription(int nId, CString sLocalize, BOOL bSeparator);
	virtual ~CDocumentReportGroupDescription();

public:
	int		GetId			() const			{ return m_nId; }
	void	SetId			(int nId)			{ m_nId = nId; }

	BOOL	GetUseSubMenu	() const			{ return m_bUseSubMenu; }
	void	SetUseSubMenu	(BOOL bSeparator)	{ m_bUseSubMenu = bSeparator; }

	CString GetLocalize		() const			{ return m_sLocalize; }
	void	SetLocalize		(CString sLocalize)	{ m_sLocalize = sLocalize; }
};

//----------------------------------------------------------------
class TB_EXPORT CDocumentReportDescription : public CFunctionDescription
{
	DECLARE_DYNCREATE(CDocumentReportDescription)

protected:
	BOOL								m_bIsDefault;
	CDocumentReportGroupDescription		m_GroupDescription;

public:
	CDocumentReportDescription ();
	CDocumentReportDescription (const CDocumentReportDescription& fd) { Assign(fd); }

public:
	const BOOL& IsDefault (){ return m_bIsDefault; }
	
	void SetDefault (const BOOL& bValue);

	const	CDocumentReportGroupDescription&	GetGroupDescription() const			{ return m_GroupDescription; };
			void								SetGroupDescription(const CDocumentReportGroupDescription& nGroupDescription);
	
	CDocumentReportDescription&	operator=	(const CDocumentReportDescription& fd) { return Assign(fd); }
	CDocumentReportDescription&	Assign		(const CDocumentReportDescription& fd);
};

//----------------------------------------------------------------
class TB_EXPORT CBaseReportDescriptionArray : public CBaseDescriptionArray
{
//	DECLARE_DYNCREATE(CBaseReportDescriptionArray)

public:
	int	Add	(CDocumentReportDescription* pInfo) { return Array::Add (pInfo); }

	CDocumentReportDescription* GetAt	(int nIdx) const { return (CDocumentReportDescription*) Array::GetAt (nIdx); }
	CDocumentReportDescription* GetInfo	(const CTBNamespace& aNS) const;
	CDocumentReportDescription* GetInfo	(const CString& sName) const;

public:
	CDocumentReportDescription* GetDefault();

	// operatori
	CBaseReportDescriptionArray&	operator=	(const CBaseReportDescriptionArray& ar);
	BOOL							operator!=	(const CBaseReportDescriptionArray& ar);
	BOOL							operator==	(const CBaseReportDescriptionArray& ar);

	void Assign	(const CBaseReportDescriptionArray& ar);
	BOOL IsEqual(const CBaseReportDescriptionArray& ar);
};

//----------------------------------------------------------------
class TB_EXPORT CReportObjectsDescription : public CObject
{
	DECLARE_DYNCREATE(CReportObjectsDescription)

protected:
	CBaseReportDescriptionArray	m_arReports;

public:
	CReportObjectsDescription();


public:
	CReportObjectsDescription& operator= (const CReportObjectsDescription&	rod) { m_arReports = rod.m_arReports; return *this; }

	void Clear ();
	
	// metodi di lettura
	CDocumentReportDescription* GetReportInfo	(const CTBNamespace&) const;
	CDocumentReportDescription* GetReportInfo	(const int&) const;

	CBaseReportDescriptionArray& GetReports	();
	CDocumentReportDescription*  GetDefault ();

	// metodi di settaggio
	void AddReport		(CDocumentReportDescription*);
	void RemoveReportAt	(const int&);
	BOOL RemoveReport	(CDocumentReportDescription* pDescri);
	void RemoveAll		();
};

#include "endh.dex"
