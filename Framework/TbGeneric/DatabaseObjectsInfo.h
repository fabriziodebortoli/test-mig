#pragma once

#include <TbNameSolver\TbNamespaces.h>
#include <TbGeneric\DataObj.h>
#include <TbGeneric\FunctionCall.h>

//includere alla fine degli include del .H
#include "beginh.dex"

#define TABLE_TYPE	1
#define VIEW_TYPE	2
#define PROC_TYPE	3
#define VIRTUAL_TYPE 4

//=============================================================================        
class TB_EXPORT CDbFieldDescription : public CDataObjDescription
{
	DECLARE_DYNAMIC(CDbFieldDescription)


public:
	CDbFieldDescription(const CString& strName, DataObj* pValue, const CTBNamespace& ownerModule);
	CDbFieldDescription (const CDbFieldDescription* pDescri);
	CDbFieldDescription (const CTBNamespace& ownerModule);

public:
	enum DbColumnType { Column, Variable, Parameter };

private:
	CTBNamespace			m_OwnerModule;
	DbColumnType			m_eColType;
	BOOL					m_bIsSegmentKey;
	BOOL					m_bIsAddOn;
	int						m_nCreationRelease;
	CString					m_sContextName;
	
public:
	DbColumnType	GetColType			() const { return m_eColType; }
	BOOL			IsAddOn				() const { return m_bIsAddOn; }
	int				GetCreationRelease	() const { return m_nCreationRelease; }
	const CString&		GetContextName() const { return m_sContextName; }
	BOOL			IsSegmentKey		() const { return m_bIsSegmentKey; }
	const CTBNamespace&	GetOwnerModule		() const  { return m_OwnerModule; }

	void 	SetColType			(DbColumnType aColType) { m_eColType = aColType; }
	void 	SetIsAddOn			(BOOL bIsAddOn)			{ m_bIsAddOn = bIsAddOn; }
	void	SetCreationRelease	(const int nRelease)	{ m_nCreationRelease = nRelease; }
	void	SetContextName		(const CString& sName) { m_sContextName = sName; }
	void	SetIsSegmentKey		(BOOL bIsKey)			{ m_bIsSegmentKey = bIsKey; }
	
	
	void	Assign				(const CDbFieldDescription* pDescri);
	
};

// Table/View/Procedure
//----------------------------------------------------------------
class TB_EXPORT CDbObjectDescription : public CBaseDescription
{
	DECLARE_DYNCREATE(CDbObjectDescription)

public:
	enum DeclarationType { None = 0, Coded = 1, Dynamic = 2 };

private:
	int				m_nCreationRelease;
	int				m_nCreationStep;
	Array			m_arDynamicFields;
	DeclarationType	m_DeclarationType;
	CTBNamespace	m_TemplateNamespace;
	BOOL			m_bMasterTable;

public:
	CDbObjectDescription (CTBNamespace::NSObjectType aNSType = CTBNamespace::TABLE);

public:
	virtual const CString	GetTitle	() const;
	
	const int				GetCreationRelease		() const { return m_nCreationRelease; }
	const int				GetCreationStep			() const { return m_nCreationStep; }
	const Array&			GetDynamicFields		() const { return m_arDynamicFields; }
	const int				GetSqlRecType			() const;
	CDbFieldDescription*	GetDynamicFieldByName	(const CString& sName) const;
	const DeclarationType	GetDeclarationType		() const { return m_DeclarationType; }
	const CTBNamespace&	GetTemplateNamespace		() const { return m_TemplateNamespace; }
	const BOOL				IsMasterTable()			const { return m_bMasterTable; }

	// metodi di settaggio
	void	SetCreationRelease(const int nRelease)	{ m_nCreationRelease = nRelease; }
	void	SetCreationStep		(const int nStep)	{ m_nCreationStep = nStep; }
	void	SetDeclarationType(const CDbObjectDescription::DeclarationType bValue) { m_DeclarationType = bValue; };
	void	SetTemplateNamespace(const CTBNamespace& aNs)	{ m_TemplateNamespace = aNs; }
	void	SetMasterTable(BOOL bSet)						 { m_bMasterTable = bSet; }

	void	AddDynamicField		(CDbFieldDescription* pField);
	void	RemoveDynamicField	(int nIdx);
	void	RemoveAllDynamicFields();
};

// AdditionalColumns/AlterTable
//----------------------------------------------------------------
class TB_EXPORT CAlterTableDescription : public CBaseDescription
{
	DECLARE_DYNCREATE(CAlterTableDescription)

private:
	int	m_nCreationRelease;
	int m_nCreationStep;

public:
	CAlterTableDescription ();

public:
	const int&	GetCreationRelease	() const { return m_nCreationRelease; }
	const int&	GetCreationStep() const { return m_nCreationStep; }

	// metodi di settaggio
	void	SetCreationRelease(const int& nRelease) { m_nCreationRelease = nRelease; }
	void	SetCreationStep		(const int& nStep)  { m_nCreationStep = nStep; }
};

// AdditionalColumns/Table
//----------------------------------------------------------------
class TB_EXPORT CAddColsTableDescription : public CBaseDescription
{
	DECLARE_DYNCREATE(CAddColsTableDescription)

public:
	CBaseDescriptionArray	m_arAlterTables;

public:
	CAddColsTableDescription ();
};

//----------------------------------------------------------------
class TB_EXPORT CAlterTableDescriptionArray : public CBaseDescriptionArray, public CTBLockable
{
	DECLARE_DYNAMIC(CAlterTableDescriptionArray)
public:
	void AddAddOnFieldOnTable(CAddColsTableDescription* pNewDescri);

public:
	virtual LPCSTR  GetObjectName() const { return "CAlterTableDescriptionArray"; }

};

//=============================================================================        
const TB_EXPORT CAlterTableDescriptionArray*	AFXAPI AfxGetAddOnFieldsTable ();
const TB_EXPORT CAddColsTableDescription*		AFXAPI AfxGetAddOnFieldsOnTable (const CTBNamespace& aTableNs);

//=============================================================================        
//			DatabaseObjectsTable needed structures related to #3617
//=============================================================================        

//=============================================================================        
class TB_EXPORT CDbReleaseDescription : public CObject
{
	DECLARE_DYNAMIC(CDbReleaseDescription)

private:
	int		m_nRelease;
	CString	m_sSignature;

public:
	CDbReleaseDescription(const CString& sSignature, const int& nRelease);

public:
	const CString&	GetSignature	() const { return m_sSignature; }
	const int&		GetRelease	() const { return m_nRelease; }
	
	void SetRelease (const int& nRelease);
};

//=============================================================================        
class TB_EXPORT DatabaseReleasesTable : public CObject, public CTBLockable
{
private:
	CMapStringToOb*	m_pReleases;

public:
	DatabaseReleasesTable ();
	~DatabaseReleasesTable ();

public:
	const CString	GetSignatureOf	(CString sKey) const;
	const int		GetReleaseOf	(CString sKey) const;

	BOOL AddRelease	(CString sKey, const CString& sSignature, const int& nRelease);
	CMapStringToOb* GetDatabaseReleaseMap() const {return m_pReleases;	}


public:
	virtual LPCSTR  GetObjectName() const { return "DatabaseReleasesTable"; }
};

//=============================================================================        
class TB_EXPORT DatabaseObjectsTable : public CObject, public CTBLockable
{
	friend class CXMLDatabaseObjectsParser;
	friend class CApplicationsLoader;

	DECLARE_DYNAMIC(DatabaseObjectsTable)

private:
	CMapStringToOb	m_DbObjects;

public:
	DatabaseObjectsTable ();
	~DatabaseObjectsTable();

public:
	CDbObjectDescription*	GetDescription	(CString sTableName) const;
	BOOL					ModuleHasObjects(const CTBNamespace& nsModule) const;
	void					ClearForRelease(int nRelease, const CTBNamespace& ownerModule, CStringArray& arRemovedTables, CStringArray& arRemovedFields);
public:
	virtual LPCSTR  GetObjectName() const { return "DatabaseObjectsTable"; }

	// table write methods
	int		AddObject	(CDbObjectDescription* pDescri);

private:
	int		Merge		(	
							CDbObjectDescription* pExistingDescri, 
							CDbObjectDescription* pNewDescri
						);
	BOOL	MergeField	(	
							CDbObjectDescription*		pExistingDescri, 
							const CDbObjectDescription*	pNewDescri,
							const CDbFieldDescription*	pField
						);
};


DECLARE_SMART_LOCK_PTR(DatabaseObjectsTable)
DECLARE_CONST_SMART_LOCK_PTR(DatabaseObjectsTable)

DECLARE_SMART_LOCK_PTR(DatabaseReleasesTable)
DECLARE_CONST_SMART_LOCK_PTR(DatabaseReleasesTable)

// General Functions
//-----------------------------------------------------------------------------
TB_EXPORT DatabaseObjectsTableConstPtr	AFXAPI AfxGetDatabaseObjectsTable();
TB_EXPORT DatabaseObjectsTablePtr		AFXAPI AfxGetWritableDatabaseObjectsTable();

TB_EXPORT DatabaseReleasesTableConstPtr	AFXAPI AfxGetDatabaseReleasesTable();
TB_EXPORT DatabaseReleasesTablePtr		AFXAPI AfxGetWritableDatabaseReleasesTable();

#include "endh.dex"
