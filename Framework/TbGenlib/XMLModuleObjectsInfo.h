#pragma once

#include <TbXmlCore\xmlgeneric.h>

#include <TBNameSolver\TBNamespaces.h>

#include <TBClientCore\ModuleConfigInfo.h>
#include <TBClientCore\ApplicationConfigInfo.h>

#include <TBGeneric\DataBaseObjectsInfo.h>
#include <TBGeneric\FunctionObjectsInfo.h>
#include <TBGeneric\OutDateObjectsInfo.h>
#include <TBGeneric\ReferenceObjectsInfo.h>
#include <TBGeneric\DocumentObjectsInfo.h>

#include <TbGeneric\array.h>

//includere alla fine degli include del .H
#include "beginh.dex"

class CPathFinder;
class CLocalizableXMLDocument;
class CDiagnostic;
class CXMLReferenceObjectsParserBase;

// serve per avere le informazioni sui campi utilizzati dai criteri 
// programmativi, per la fase di parsing e unparsing sul file XML
//----------------------------------------------------------------
//	class CXMLVariable
//----------------------------------------------------------------
class TB_EXPORT CXMLVariable : public CObject
{
	friend class CXMLVariableArray;

	DECLARE_DYNAMIC(CXMLVariable);

private:
	CString		m_strName;
	DataObj*	m_pDataObj;
	CString		m_strXMLValue;
	int 		m_nReference;
	BOOL		m_bOwnsDataObj;

public:
	CXMLVariable	(const CString&, DataObj*, BOOL bOwnsDataObj = FALSE);
	CXMLVariable	(const CString&, DataObj&, BOOL bOwnsDataObj = FALSE);
	CXMLVariable	(const CString&, const CString&);
	CXMLVariable	(const CXMLVariable&);
	~CXMLVariable	();

protected:
	void			Assign			(const CXMLVariable&);

public:
	DataObj*		GetDataObj			()			const	{ return m_pDataObj;	} 
	void			SetDataObj(DataObj* pDataObj);
	const CString&	GetName				()			const	{ return m_strName;		}
	const CString&	GetXMLValue			()			const	{ return m_strXMLValue;	}
	CString			GetDataObjValue		()			const	{ return m_pDataObj->FormatDataForXML(); }
	void			SetDataObjValue		(LPCTSTR pszString)	{ m_pDataObj->AssignFromXMLString(pszString); }
	void			BindExternalDataObj	(DataObj* pDataObj);
	BOOL			GetOwnsDataObj		()					{ return m_bOwnsDataObj; }
public:
	void			Parse				(CXMLNode*);
	void			UnParse				(CXMLNode*);

private:
	BOOL			IsEqual			(const CXMLVariable& aXMLCriteria) const;

public:
	BOOL			operator ==		(const CXMLVariable& aXMLVar)	const { return IsEqual	(aXMLVar); }
	BOOL			operator !=		(const CXMLVariable& aXMLVar)	const { return !IsEqual	(aXMLVar); }
	CXMLVariable&	operator =		(const CXMLVariable&);

};

//////////////////////////////////////////////////////////////////////////////
//					CXMLVariableArray definition
//////////////////////////////////////////////////////////////////////////////
//
class TB_EXPORT CXMLVariableArray : public Array
{
	DECLARE_DYNCREATE (CXMLVariableArray);

public:
	CString m_strName;
	CObject* m_pBindingData; //potrebbe essere utile avere a disposizione anche l'istanza della classe che ha apportato le variabili (vedi caso dei filtri nell'integrazione con il CRM)
							 // diventa di proprietà dell'array che poi ne fa la delete

public:
	CXMLVariableArray() { m_pBindingData = NULL; }
	CXMLVariableArray	(const CXMLVariableArray& a) { m_pBindingData = NULL; *this = a; }
	~CXMLVariableArray	();

public:
	CXMLVariable* 	GetAt			(int nIndex) const		{ return (CXMLVariable*) Array::GetAt(nIndex);		}
	CXMLVariable*&	ElementAt		(int nIndex)			{ return (CXMLVariable*&) Array::ElementAt(nIndex); }
	
	int				Add				(CXMLVariable* pVar)	{ return Array::Add(pVar); }
	int				Add				(const CString&, DataObj*);
	int				Add				(const CString&, DataObj&);
	int				Add				(const CString&, const CString&);

	int				AddVariables	(CXMLVariableArray*);
	void			RemoveAll		();

public:
	BOOL			IsEqual			(const CXMLVariableArray&) const;
	int				GetVariable		(const CString&);
	BOOL			ExistVariable	(const CString&);

public:
	CXMLVariable*	GetVariableByName(const CString&);

public:
	void			Parse(CXMLNode*);
	void			UnParse(CXMLNode*);

	//parse and unparse an xml string like this:
	//<XMLVariables name = 'variableArrayName'>
	//	<AllItems>true<\AllItems>
	//	<SelItems>false<\SelItems>
	//	<FromItem><\FromItem>
	//	<ToItem><\ToItem>
	//  <AllTypes>false<\AllTypes>
	//	<SelTypes>true<\SelTypes>
	//	<FromType>Type1<\FromType>
	//	<ToType>Type2<\ToType>
	//<\XMLVariables	
	void			ParseFromXMLString(const CString& xmlString);
	CString			UnParseToXMLString();

public:	
	CXMLVariable* 	operator[]		(int nIndex) const	{ return GetAt(nIndex);		}
	CXMLVariable*& 	operator[]		(int nIndex)		{ return ElementAt(nIndex);	}

	BOOL			operator==		(const CXMLVariableArray& aVarArray) const { return IsEqual	(aVarArray); }
	BOOL			operator!=		(const CXMLVariableArray& aVarArray) const { return !IsEqual	(aVarArray); }

	CXMLVariableArray&	operator=	(const CXMLVariableArray& aVarArray);	
};

//----------------------------------------------------------------
// classi per la lettura dei files contenitori delle grammatiche
//----------------------------------------------------------------

// lettura scrittura del file EnvelopeObjects.xml
//----------------------------------------------------------------
class TB_EXPORT CEnvelopeObjectsDescription
{
public:
	CStringArray	m_arEnvClasses;
	bool			m_bLoaded;

public:
	CEnvelopeObjectsDescription ();

public:
	bool		IsLoaded () { return m_bLoaded; }
	void		SetLoaded(bool bValue) { m_bLoaded = bValue;}

	BOOL ReadFile	(const CTBNamespace&, CPathFinder* pPathFinder);
	BOOL SaveFile	(const CTBNamespace&, CPathFinder* pPathFinder);
};

// descrizione di un AddOnApplication
//=============================================================================
class TB_EXPORT CApplicationDescription : public CObject
{    
public:
	CApplicationConfigInfo				m_Info;
	CLocalizableApplicationConfigInfo	m_LocalizableInfo;
};

// descrizione di un AddOnModule. Alcune delle descrizioni sono a caricamento
// ritardato sulla base di quello che viene eseguito
//=============================================================================
class TB_EXPORT CModuleDescription : public CObject, public CTBLockable
{    
private:	
	// descrizione del modulo
	CModuleConfigInfo	m_Info;

	// descrizioni oggetti del modulo delayed
	CFunctionObjectsDescription		m_FunctionsInfo;
	CFunctionObjectsDescription		m_EventHandlersInfo;
	CReferenceObjectsDescription	m_ReferencesInfo;
	CEnvelopeObjectsDescription		m_EnvelopeInfo;


	// descrizioni oggetti del modulo non delayed
	COutDateObjectsDescription		m_OutDatesInfo;

public:
	CModuleDescription ();

	virtual LPCSTR  GetObjectName() const { return "CModuleDescription"; }

public:
	// metodi di lookup delle informazioni
	const CModuleConfigInfo&				GetConfigInfo 			() const { return m_Info; }
	const CFunctionObjectsDescription&		GetFunctionsInfo		();
	const CFunctionObjectsDescription&		GetEventHandlersInfo	();
	const CReferenceObjectsDescription&		GetReferencesInfo		();
	const CEnvelopeObjectsDescription&		GetEnvelopeInfo			();
	const COutDateObjectsDescription&		GetOutDateObjectsInfo	();

	CString GetWebMethodsModifyDate();
	BOOL  IsOutDated(const CTBNamespace& sNamespace, const int& nRelease);

	//CClientDocDescription*		GetClientDocInfo		(const CTBNamespace& aDocNS) const;
	CFunctionDescription*		GetParamObjectInfo		(const CTBNamespace& aNamespace);

	// X-Tech
	const CStringArray*			GetAvailableDocEnvClasses	(const CTBNamespace& aDocNS);

private:

	// delayed load
	void LoadEventHandlerObjects	();
	void LoadReferenceObjects		();
	void LoadReferenceObjects		(CPathFinder::PosType pos, CPathFinder::Company company = CPathFinder::CURRENT);

public:
	void LoadFunctionsObjects		();

	static BOOL LoadFunctionsObjects		
						(
							const CString& sPath,
							CFunctionObjectsDescription& arDescription,
							const CTBNamespace& ns,
							BOOL bSkipDuplicate = FALSE
						);
	static BOOL IsGoodFile	
					(
						const CString&			sFileName, 
						const CString			sTagName, 
						CXMLDocumentObject*		pDoc
					);

	static CXMLReferenceObjectsParserBase* s_pReferenceObjectsParser;
};

#include "endh.dex"
