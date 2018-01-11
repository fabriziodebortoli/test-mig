
#pragma once

#include <TbGeneric\Array.h>
#include <TbGeneric\DataObj.h>
#include <TbGeneric\RDEProtocol.h>
#include <TbGeneric\FunctionObjectsInfo.h>

#include <TbNameSolver\CallbackHandler.h>

//includere alla fine degli include del .H
#include "beginh.dex"

//============================================================================
class CBaseDocument;
class DataObj;
class Parser;
class SymTable;

//============================================================================
class TB_EXPORT SymField : public CObject, public IDisposingSourceImpl
{
	friend class SymTable;
	DECLARE_DYNAMIC(SymField)

private:
	CString		m_strName;
	WORD		m_wId;		//such as Woorm 'Alias'

	DataType    m_DataType;
	DataObj*	m_pData;
	BOOL		m_bOwnData;

	SymTable*	m_pTable;

	CFunctionDescriptionArray* m_parMethods;

	int			m_nRefCount;
	int			m_nLeftRefCount;

	CString		m_strTitle;
	CString		m_strTag;

	IDataProvider* m_pProvider;

	CObject*	m_pCustomData;

public:
	SymField (const CString& strName, DataType dt = DataType::Null, WORD nId = SpecialReportField::NO_INTERNAL_ID, DataObj* pValue = NULL, BOOL bCloneValue = TRUE);
	SymField (const SymField&);

	virtual ~SymField ();

	//own Table
	void				SetSymTable		(SymTable* pTable)			{ m_pTable = pTable; }
	SymTable*			GetSymTable		()	const					{ return m_pTable; }

	virtual void		SetDataPtr		(DataObj* pData, BOOL bOwnData = TRUE);
	BOOL				AllocData		();
	virtual void		SetDataType		(const DataType& dataType, BOOL bArray = FALSE);

	DataType 			GetDataType		() const					{ return m_DataType; }
	BOOL				IsArray			() const					{ return GetDataType().m_wType == DATA_ARRAY_TYPE; }

	WORD				GetId			() const					{ return m_wId;		}	//m_wInternalId, m_nAlias
	void				SetId			(WORD wId) 					{ m_wId = wId;		}	

	const CString&		GetName			() const					{ return m_strName;	}	//m_strPublicName
	void				SetName			(LPCTSTR pszName);	

	const CString&		GetTag			() const					{ return m_strTag;	}	
	void				SetTag			(LPCTSTR pszTag);	

	const CString&		GetTitle		(BOOL b=TRUE) const			{ if (b) return m_strTitle.IsEmpty() ? m_strName : m_strTitle;	return m_strTitle; }	
	void				SetTitle		(LPCTSTR pszTitle)			{ m_strTitle = pszTitle;	}	

	void				SetCustomData	(CObject* pCustomData)		{ m_pCustomData = pCustomData; }
	CObject*			GetCustomData	() const					{ return m_pCustomData; }

	virtual DataObj*	GetData			(int /*nDataLevel*/ = -1) const;
	virtual DataObj*	GetRepData		() 	const { return GetData(); }
	void				AssignData		(const DataObj& aData);
	virtual void		SetData			(DataObj& aData) 	{ AssignData(aData); }

	void				SetProvider			(IDataProvider* pProvider) { m_pProvider = pProvider; }
	IDataProvider*		GetProvider			() const { return m_pProvider; }
	virtual const DataObj*	GetIndexedData		(int nIndx) const;
	void				AssignIndexedData	(int nIndx, const DataObj& aData);

	virtual int			GetLen			() const					{ return 0; }
	virtual CString		GetDescription	() const					{ return _T(""); }

	void	IncRefCount		()			{		 m_nRefCount++; }
	void	DecRefCount		()			{		 m_nRefCount--; }
	int		GetRefCount		() const	{ return m_nRefCount; }

	void	IncLeftRefCount	()			{		 m_nLeftRefCount++; }
	void	DecLeftRefCount	()			{		 m_nLeftRefCount--; }
	int		GetLeftRefCount	() const	{ return m_nLeftRefCount; }

	virtual BOOL	IsRuleField () const { return FALSE; }
			BOOL	IsOwnData () const { return m_bOwnData; }
	virtual BOOL	OwnThreadContextVar	() const { return FALSE; }
	virtual BOOL	IsInput() const { return TRUE; }
	virtual BOOL	IsLowerLimit() const { return FALSE; }
	virtual BOOL	IsUpperLimit() const { return FALSE; }
	virtual BOOL	IsAsk() const { return FALSE; }
	virtual BOOL	IsReInit() const { return FALSE; }
	virtual BOOL	IsStatic() const { return FALSE; }
	virtual BOOL	IsHidden() const { return FALSE; }

protected:
	void	AddMethods (const CFunctionDescriptionArray& arMethods);
public:
	void	ClearMethods () { SAFE_DELETE(m_parMethods); }
	BOOL	AddMethods (const CString& sClassName,				const CMapFunctionDescription* mapMethods);
	BOOL	AddMethods (const CRuntimeClass* rtStopBaseClass,	const CMapFunctionDescription* mapMethods);

	CFunctionDescription*	FindMethod (const CString& sName) const;
	CFunctionDescriptionArray* GetMethodsList() const { return m_parMethods; }

	virtual void	RuleNullified		() {}
	virtual void	RuleUpdated			() {}
	virtual void	RuleDataProcessed	() {}

#ifdef _DEBUG
public:
	void Dump(CDumpContext& dc) const;
	void AssertValid() const;
#endif //_DEBUG
};

//============================================================================

class TB_EXPORT SymTable : public Array, public IDisposingSource
{
	friend class SymField;
	DECLARE_DYNAMIC(SymTable)
private:
	CCallbackHandler	m_Handler;

protected:
	CBaseDocument*	m_pDocument;
	SymTable*		m_pParentSymTable;

	int				m_nDataLevel;
	int				m_nCountSpecialFields;
	
	CMapStringToOb*	m_pmapFieldsByName;
	CMapStringToOb*	m_pmapFieldsByTag;

private:
	//serve a tracciare i campi modificati da una assegnazione in un blocco di istruzioni
	//TODO potrebbe essere un CArray<SymField*>
	CStringArray*	m_parFieldsModified = NULL;
	CStringArray*	m_parFieldsUsed = NULL;
public:
	CStringArray*   TraceFieldsModified	(CStringArray* ar) { CStringArray* arOld = GetRoot()->m_parFieldsModified; GetRoot()->m_parFieldsModified = ar; return arOld; }
	void			TraceFieldModify	(const CString& name, BOOL noDuplicate = TRUE) 
						{ 
							if (GetRoot()->m_parFieldsModified)
							{ 
								if (noDuplicate)
									if (CStringArray_Find(*GetRoot()->m_parFieldsModified, name) > -1) return;

								GetRoot()->m_parFieldsModified->Add(name);
							} 
						}

	CStringArray*   TraceFieldsUsed(CStringArray* ar) { CStringArray* arOld = GetRoot()->m_parFieldsUsed; GetRoot()->m_parFieldsUsed = ar; return arOld; }
	void			TraceFieldsUsed(const CString& name, BOOL noDuplicate = TRUE)
	{
		if (GetRoot()->m_parFieldsUsed)
		{
			if (noDuplicate)
				if (CStringArray_Find(*GetRoot()->m_parFieldsUsed, name) > -1) return;

			GetRoot()->m_parFieldsUsed->Add(name);
		}
	}

public:
	SymTable();
	virtual ~SymTable();

	void				Clear			();
	void				UseMapNames		();
	void				UseMapTags		();

	void				SetParent		(SymTable* pParentSymTable) { m_pParentSymTable = pParentSymTable; }
	SymTable*			GetParent		() const					{ return m_pParentSymTable; }
	SymTable*			GetRoot			() const					{ return m_pParentSymTable ? m_pParentSymTable->GetRoot() : const_cast<SymTable*>(this); }

	void	 			SetDataLevel	(int level)					{ m_nDataLevel = level; }
	int		 			GetDataLevel	()	const					{ return m_nDataLevel; }

	virtual void		SetDocument		(CBaseDocument*	pDocument)	{ m_pDocument = pDocument; }
	CBaseDocument*		GetDocument		() const					{ return m_pDocument; }
	virtual BOOL		DispatchFunctionCall(CFunctionDescription* pFunc) { return FALSE; }
	SymField*			GetAt			(INT_PTR nIndex) const { return (SymField*)__super::GetAt(nIndex); }

	virtual SymField*	GetField		(LPCTSTR pszName) const  { return GetField(pszName, TRUE); }
	virtual SymField*	GetField		(LPCTSTR pszName, BOOL bFindParent) const;
	virtual SymField*	GetFieldByTag	(LPCTSTR pszTag) const;
	virtual SymField*	GetFieldByID	(WORD id)	const;

	BOOL	 			ExistField		(LPCTSTR s) const { return GetField(s) != NULL; }

	BOOL				DelField		(LPCTSTR);
	BOOL				DelField		(WORD);
	BOOL				RenameField		(LPCTSTR oldName, LPCTSTR newName);

	virtual INT_PTR		Add				(SymField*);

	BOOL				IsEmpty			() const { return GetSize() <= m_nCountSpecialFields; }
	// Woorm
	virtual WORD		GetCurId		() const { return 0; }
	virtual void		SetLastId		(WORD ) {}

	virtual void		DeleteMeAsLocalScope();
	virtual SymTable*	CreateLocalScope();

protected:
	virtual BOOL		OnBeforeDelField (SymField*) { return TRUE; }
	void				AddSpecial		 ();

	class Alias : public CObject
	{
		public:
			CString m_strAlias;
			CString m_strExpanded;

			Alias(LPCTSTR sAlias, LPCTSTR sExpanded)
				:
				m_strAlias(sAlias),
				m_strExpanded(sExpanded)
			{}
	};
	Array	m_arAlias;

public:
			void	AddAlias			(const CString& sAlias, const CString&  sExpanded) { m_arAlias.Add(new Alias(sAlias, sExpanded)); }
			BOOL	ExpandAlias			(const CString& sName, CString& sExpandedName) const;
	
	virtual	BOOL	ResolveCallMethod	(CString sFuncName, CFunctionDescription& aMethods, CString& sHandleName) const;
	virtual BOOL	ResolveCallQuery	(CString, CFunctionDescription&, CString&) const { return FALSE; }
	virtual BOOL	ResolveCallProcedure (CString, CFunctionDescription&) const { return FALSE; }

			INT_PTR	Append				(const SymTable&);

	static CString s_SelfReference;

	BOOL UnParse (CXMLNode* pRootNode);

	CString GenerateName(LPCTSTR pszFromName, const CString& strPrefix);

	//aggiunge una callback da chiamare alla distruzione del documento
	void AddDisposingHandler (CObject* pListener, ON_DISPOSING_METHOD pHandler) { m_Handler.AddDisposingHandler(pListener, pHandler); }
	void RemoveDisposingHandlers (CObject* pListener) { m_Handler.RemoveDisposingHandlers(pListener); }

};

//-----------------------------------------------------------------------------
TB_EXPORT int CompareFieldByName(CObject* po1, CObject* po2);
TB_EXPORT int CompareFieldByID(CObject* po1, CObject* po2);

//-----------------------------------------------------------------------------

typedef TDisposablePtr<SymTable> SymTablePtr;
//=============================================================================
#include "endh.dex"
