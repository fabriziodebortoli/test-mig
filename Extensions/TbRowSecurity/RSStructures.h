#pragma once 

#include <TbGes\Dbt.h>
#include <TbOledb\TbExtensionsInterface.h>

//includere alla fine degli include del .H
#include "beginh.dex"

class CSubjectsManager;
class CSubjectCacheArray;
class TRS_Subjects;
class TRS_SubjectsGrants;

// Gestisce le informazioni inerenti ad una entità da proteggere
///////////////////////////////////////////////////////////////////////////////
//							RSEntityInfo definition
///////////////////////////////////////////////////////////////////////////////
//=============================================================================
class TB_EXPORT RSEntityInfo : public CObject
{
public:
	CString m_strName;
	CString m_strTitle;
	CString m_strDescription;
	CString m_strMasterTable;
	CString m_strAutonumberNamespace;

	CTBNamespace m_MasterTableNamespace;
	CArray<CTBNamespace> m_arrDocNamespace; //l'array serve per OFM	
	CStringArray m_arColumns; 	
	BOOL m_bUsed;							//se l'entità è utilizzata

public:
	RSEntityInfo() : m_bUsed(FALSE) {}

public:
	BOOL Parse(CXMLNode* pnEntity);
	void Assign(RSEntityInfo*);
};

// Gestisce le informazioni inerenti ad una entità da proteggere
///////////////////////////////////////////////////////////////////////////////
//							RSEntityInfo definition
///////////////////////////////////////////////////////////////////////////////
//=============================================================================
class TB_EXPORT CRSEntityArray : public Array
{
public:
	RSEntityInfo* 	GetAt		(int nIndex)	const	{ return (RSEntityInfo*) Array::GetAt(nIndex);	}
	RSEntityInfo*&	ElementAt	(int nIndex)			{ return (RSEntityInfo*&) Array::ElementAt(nIndex); }
	
	RSEntityInfo* 	operator[]	(int nIndex)	const	{ return GetAt(nIndex);	}
	RSEntityInfo*&	operator[]	(int nIndex)			{ return ElementAt(nIndex);	}

	int Add(RSEntityInfo* pElem) { return Array::Add((CObject*)pElem); }
};


//rappresenta il singolo segmento da proteggere
///////////////////////////////////////////////////////////////////////////////
//						RSSingleColumn definition
///////////////////////////////////////////////////////////////////////////////
//=============================================================================
class TB_EXPORT RSSingleColumn : public CObject
{
public:
	CString m_strEntityColumn;
	CString m_strTableColumn;

public:
	BOOL Parse(CXMLNode* pnColumn);
};


//rappresenta i segmenti necessari per identificare il dato da proteggere
// es per i clienti abbiamo due segmenti: CustSuppType e CustSupp
///////////////////////////////////////////////////////////////////////////////
//						RSProtectedColumns definition
///////////////////////////////////////////////////////////////////////////////
//=============================================================================
class TB_EXPORT RSProtectedColumns : public CObject
{
public:
	//i dati da proteggere per Entity potrebbero esser anche su più colonne
	CArray<RSSingleColumn*, RSSingleColumn*> m_arProtectedColumns;

public:
	~RSProtectedColumns();

public:
	BOOL Parse(CXMLNode* pnColumns);
};


// Gestisce le informazioni inerenti ad una tabella da proteggere
///////////////////////////////////////////////////////////////////////////////
//						RSEntityTableInfo definition
///////////////////////////////////////////////////////////////////////////////
//=============================================================================
class TB_EXPORT RSEntityTableInfo : public CObject
{
public:
	RSEntityInfo* m_pEntityInfo; 
	// anche su più colonne
	//  i dati da proteggere per Entity potrebbero essere presenti su più campi 
	//(esempio per gli agenti: nei clienti abbiamo il campo Agente ed il campo Capo Area)
	 CArray<RSProtectedColumns*, RSProtectedColumns*> m_arAllProtectedColumns; 

public:
	RSEntityTableInfo();
	~RSEntityTableInfo();

public:
	BOOL Parse(CXMLNode* pnEntity);

public:
	CString GetFilterText				(SqlTable* pTable, SqlTableItem* pTableItem);
	CString GetSelectGrantString		(SqlTable* pTable); //used in HotKeyLink class
	void ValorizeRowSecurityParameters	(SqlTable* pTable);
};

// Gestisce le informazioni inerenti ad una tabella da proteggere
///////////////////////////////////////////////////////////////////////////////
//					RSProtectedTableInfo definition
///////////////////////////////////////////////////////////////////////////////
//=============================================================================
class TB_EXPORT RSProtectedTableInfo : public CObject
{
public:
	CString m_strTableName;	
	CTBNamespace m_TableNamespace;
	Array m_arProtectedInfo; //map of RSEntityTableInfo elements

public:
	RSProtectedTableInfo() {}
	~RSProtectedTableInfo();

public:
	BOOL Parse(CXMLNode* pnNode);
};

///////////////////////////////////////////////////////////////////////////////
//							RSProtectedTableInfoArray definition
///////////////////////////////////////////////////////////////////////////////
//=============================================================================
class TB_EXPORT RSProtectedTableInfoArray : public Array
{
public:
	RSProtectedTableInfo* 	GetAt		(int nIndex)	const	{ return (RSProtectedTableInfo*) Array::GetAt(nIndex);	}
	RSProtectedTableInfo*&	ElementAt	(int nIndex)			{ return (RSProtectedTableInfo*&) Array::ElementAt(nIndex); }
	
	RSProtectedTableInfo* 	operator[]	(int nIndex)	const	{ return GetAt(nIndex);	}
	RSProtectedTableInfo*&	operator[]	(int nIndex)			{ return ElementAt(nIndex);	}

	int Add(RSProtectedTableInfo* pElem) { return Array::Add((CObject*)pElem); }
};

///////////////////////////////////////////////////////////////////////////////
// CRSHierarchyRow definition: identifica una riga della tabella RS_SubjectsHierarchy
///////////////////////////////////////////////////////////////////////////////
//=============================================================================
class TB_EXPORT CRSHierarchyRow : public CObject
{
public:
	int	m_MasterSubjectID;
	int	m_SlaveSubjectID;
	int	m_NrLevel;

	BOOL m_bVisited; // nasce a FALSE, serve per capire se l'elemento e' stato "visitato" durante le ricerche

public:
	CRSHierarchyRow(int nMasterSubjectID, int nSlaveSubjectID, int nLevel);

public:
	BOOL Match(CRSHierarchyRow* pElement) { return Match(pElement->m_MasterSubjectID, pElement->m_SlaveSubjectID, pElement->m_NrLevel); };
	BOOL Match(int nMasterSubjectID, int nSlaveSubjectID, int nLevel);
};

///////////////////////////////////////////////////////////////////////////////
//					CRSHierarchyRowArray definition
///////////////////////////////////////////////////////////////////////////////
//=============================================================================
class TB_EXPORT CRSHierarchyRowArray : public Array
{
public:
	CRSHierarchyRow* 	GetAt		(int nIndex)	const	{ return (CRSHierarchyRow*) Array::GetAt(nIndex);	}
	CRSHierarchyRow*&	ElementAt	(int nIndex)			{ return (CRSHierarchyRow*&) Array::ElementAt(nIndex); }
	
	CRSHierarchyRow* 	operator[]	(int nIndex)	const	{ return GetAt(nIndex);	}
	CRSHierarchyRow*&	operator[]	(int nIndex)			{ return ElementAt(nIndex);	}

public:	
	void Add(int nMasterSubjectID, int nSlaveSubjectID, int nLevel);

	CRSHierarchyRow* GetElement(CRSHierarchyRow* pElement) { return GetElement(pElement->m_MasterSubjectID, pElement->m_SlaveSubjectID, pElement->m_NrLevel); };
	CRSHierarchyRow* GetElement(int nMasterSubjectID, int nSlaveSubjectID, int nLevel);
};

//classe che si occupa di fare il cache delle informazioni lette (on-demand) della tabella TRS_SubjectsHierarchy
///////////////////////////////////////////////////////////////////////////////
//						CSubjectCache definition
///////////////////////////////////////////////////////////////////////////////
//=============================================================================
class CSubjectCache : public CObject
{
	DECLARE_DYNAMIC(CSubjectCache)

public:	
	int	m_SubjectID;
	CRSResourceElement* m_pResourceElement;
	
	Array*	m_pMasterSubjects;			// di tipo CSubjectHierarchy
	Array*	m_pSlaveSubjects;			// di tipo CSubjectHierarchy

public:
	CSubjectCache(TRS_Subjects* pSubjectsRec);
	CSubjectCache(const CSubjectCache&);
	~CSubjectCache();

public:
	int		GetWorkerID		() const { return (m_pResourceElement) ? m_pResourceElement->m_WorkerID : -1; }	
	BOOL	IsWorker		() const { return m_pResourceElement && m_pResourceElement->m_IsWorker;}
	CString GetResourceType	() const { return (m_pResourceElement) ? m_pResourceElement->m_ResourceType : _T(""); }	
	CString GetResourceCode	() const { return (m_pResourceElement) ? m_pResourceElement->m_ResourceCode : _T(""); }		
	CString GetSubjectTitle () const { return (m_pResourceElement) ? m_pResourceElement->m_Description : _T(""); }		

	void CopyHierarchyInfo(CSubjectCache*, CSubjectCacheArray*);
	void LoadHierarchyInfo(SqlSession*, CSubjectsManager*);
};

///////////////////////////////////////////////////////////////////////////////
//						CSubjectCacheArray definition
///////////////////////////////////////////////////////////////////////////////
//=============================================================================
class TB_EXPORT CSubjectCacheArray : public Array
{
public:
	CSubjectCache* 	GetAt		(int nIndex)	const	{ return (CSubjectCache*) Array::GetAt(nIndex);	}
	CSubjectCache*&	ElementAt	(int nIndex)			{ return (CSubjectCache*&) Array::ElementAt(nIndex); }
	
	CSubjectCache* 	operator[]	(int nIndex)	const	{ return GetAt(nIndex);	}
	CSubjectCache*&	operator[]	(int nIndex)			{ return ElementAt(nIndex);	}

public:
	int	Add	(CSubjectCache* pElement)	{ return Array::Add(pElement); };

	CSubjectCache* GetSubjectCache				(int nSubjectID);
	CSubjectCache* GetSubjectCacheFromWorkerID	(int nWorkerID);
	CSubjectCache* GetSubjectCacheFromResource	(const CString& resourceType, const CString& resourceCode);
	int			   GetSubjectID					(int nWorkerID);
};

///////////////////////////////////////////////////////////////////////////////
//						CSubjectHierarchy definition
///////////////////////////////////////////////////////////////////////////////
//=============================================================================
class CSubjectHierarchy : public CObject
{
	DECLARE_DYNAMIC(CSubjectHierarchy)

public:
	CSubjectCache* m_pSubject;
	short		   m_nrLevel;

public:
	CSubjectHierarchy(CSubjectCache* pSubject, short nLevel);
	~CSubjectHierarchy();
};

//////////////////////////////////////////////////////////////////////////////
//             class DBTEntitySubjectsGrants declaration
//////////////////////////////////////////////////////////////////////////////
//=============================================================================
class TB_EXPORT DBTEntitySubjectsGrants : public DBTSlaveBuffered
{
	DECLARE_DYNAMIC(DBTEntitySubjectsGrants)

public:
	DataStr m_EntityName;

public:
	DBTEntitySubjectsGrants	(CRuntimeClass*, CAbstractFormDoc*, const CString&);

public:
	CAbstractFormDoc*		GetDocument				() const { return (CAbstractFormDoc*) m_pDocument; }
	TRS_SubjectsGrants*		GetSubjectGrantsRec		() const { return (TRS_SubjectsGrants*)GetRecord(); }
	TRS_SubjectsGrants*		GetGrantRecordForSubject(int nSubjectID);

	void					InitSubjectsGrants		();
	void					AddRemoveImplicitGrants	(Array* pSubjectsToGrant, int nOnwerWorkerID, bool bRemove);
	void					ModifyExplicitGrants	(TRS_SubjectsGrants* pSubjectsGrantsRec, Array* pSubjectsToGrant, DataEnum grantType);

protected:
	virtual	void		OnDefineQuery		();
	virtual	void		OnPrepareQuery		();
	virtual void		OnPreparePrimaryKey	(int, SqlRecord*);
	virtual DataObj*	OnCheckPrimaryKey	(int, SqlRecord*) { return NULL; }
	virtual	DataObj*	GetDuplicateKeyPos	(SqlRecord*) { return NULL; }
	virtual	BOOL		OnOkTransaction		();
	virtual BOOL		FindData			(BOOL bPrepareOld = TRUE);
};

#include "endh.dex"