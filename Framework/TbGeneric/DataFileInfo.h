#pragma once

#include <TBNameSolver\TBResourceLocker.h>
#include <TbNameSolver\TbNamespaces.h>
#include <TbGeneric\DataObj.h>

//includere alla fine degli include del .H
#include "beginh.dex"
//=============================================================================

// field di un datafile: un dataobj per il valore  e il name
//----------------------------------------------------------------
class TB_EXPORT CDataFileElementField: public CObject
{
public:
	CString		m_sName;
	DataObj*	m_pValue;

public:
	CDataFileElementField	(const CString& sName,	DataObj* pValue);
	~CDataFileElementField();
};

// attributi di un datafile field: name, key, hidden e datatype
//----------------------------------------------------------------
class TB_EXPORT CDataFileElementFieldType: public CObject
{
public:
	CString		m_sName;
	DataType	m_Type;
	DataBool	m_bHidden;
	DataBool	m_bKey;


public:
	CDataFileElementFieldType	(CString sName,	DataType Type, DataBool bHidden, DataBool bKey);
	CDataFileElementFieldType	(const CDataFileElementFieldType&);
};

// array di  element field
//----------------------------------------------------------------
class TB_EXPORT CDataFileElement: public Array
{
public:
	BOOL m_bFromCustom;  //is true if the element in customer file because the element added in interactive mode that will be saved in customer file


public:
	CDataFileElement () : m_bFromCustom(FALSE)	{}

public:
	CDataFileElementField* 	GetAt			(int nIndex) const					{ return (CDataFileElementField*) Array::GetAt(nIndex);	}
	CDataFileElementField*&	ElementAt		(int nIndex)						{ return (CDataFileElementField*&) Array::ElementAt(nIndex); }
	int						Add				(CDataFileElementField* pObj)		{ return Array::Add(pObj); }
	
	CDataFileElementField* 	operator[]		(int nIndex) const					{ return GetAt(nIndex);	}
	CDataFileElementField*& operator[]		(int nIndex)						{ return ElementAt(nIndex);	}

	DataObj*				GetElementValue	(const CString& sName);
	BOOL					ElementExist	(const CString&  sName);
	
	
};

// array di  element 
//----------------------------------------------------------------
class TB_EXPORT CDataFileElements: public Array
{
public:
	CDataFileElements () {}

public:
	CDataFileElement* 	GetAt		(int nIndex) const					{ return (CDataFileElement*) Array::GetAt(nIndex);	}
	CDataFileElement*&	ElementAt	(int nIndex)						{ return (CDataFileElement*&) Array::ElementAt(nIndex); }
	int					Add			(CDataFileElement* pObj)			{ return Array::Add(pObj); }
	
	CDataFileElement* 	operator[]	(int nIndex) const					{ return GetAt(nIndex);	}
	CDataFileElement*&	operator[]	(int nIndex)						{ return ElementAt(nIndex);	}

	CDataFileElement* 	GetElement		(const CString& strKeyName, const CString& strValue);
	void				RemoveElement	(const CString& strKeyName, const CString& strKeyValue);
};

// array di  element fieldtype
//----------------------------------------------------------------
class TB_EXPORT CDataFileElementType: public Array
{
public:
	CDataFileElementType ();
//
private:
	int		m_nKey;								//Contiene l'indice del campo chiave

public:
	CDataFileElementFieldType* 	GetAt		(int nIndex) const					{ return (nIndex >= 0 && nIndex <= GetUpperBound()) ?  (CDataFileElementFieldType*) Array::GetAt(nIndex) : NULL; }
	int							Add			(CDataFileElementFieldType* pObj)	{ return Array::Add(pObj); }
	
	CDataFileElementFieldType* 	operator[]	(int nIndex) const					{ return GetAt(nIndex);	}

	void						RemoveAt(int nIndex)							{ if (nIndex >= 0 && nIndex <= GetUpperBound()) Array::RemoveAt(nIndex); }

	void						SetKey			(int nKey)						{m_nKey = nKey; }
	int							GetKey			()								{return m_nKey; }
	CString						GetKeyName		()								{return GetAt(m_nKey)->m_sName; }
	DataType					GetKeyType		()								{return GetAt(m_nKey)->m_Type; }
	DataType					GetElementType	(CString sName);
	CDataFileElementFieldType*	GetElement		(CString sName);
	int							GetElementPos	(CString sName);
};

// Struttura in memoria di un data file (namespace, header element  
// e array di datafile element
//----------------------------------------------------------------
class TB_EXPORT CDataFileInfo: public CObject
{
public:
	CTBNamespace			m_Namespace;
	CString					m_strFileName;

	CDataFileElements		m_arElements;
	CDataFileElementType	m_arElementTypes;

	BOOL					m_bAllowChanges;

	BOOL					m_bUseProductLanguage;
	BOOL					m_bFilterLike;

	BOOL					m_bInvalid; // il file xml contenente i dati risulta modificato, questo significa che la prima volta che riutilizzo il DataFileInfo lo devo ricaricare
	BOOL					m_bEnableAddElements;

public:
	CDataFileInfo ();
	CDataFileInfo (const CTBNamespace& ns, BOOL m_bAllowChanges = FALSE, BOOL bUseProductLanguage = FALSE);

	CDataFileElementFieldType* GetFieldType(const CString& strName) { return m_arElementTypes.GetElement(strName); }
	CDataFileElementFieldType* GetFieldType(int nElement) {  return m_arElementTypes.GetAt(nElement); }
	int						   GetFieldTypePos(const CString& strName) { return m_arElementTypes.GetElementPos(strName); }

	CString					GetDescription	(int nElement);
	CString					GetValue		(int nElement);
	DataObj* 				GetElement		(CString pKeyValue, CString sName);
	void	 				ChangeKey		(CString sName);
	void					SetHidden		(CString sNomeCampo, BOOL bValue = TRUE);
	void					SetType			(CString sNomeCampo, DataType tType);
	CString					GetKeyName() { return m_arElementTypes.GetKeyName(); }

	CDataFileElement*		GetDataFileElement(const CString& pKeyValue); //restituisce l'elemento il cui valore della chiave è passato come argomento
	//
	//aggiunta ed elimazione element aggiutni da utente
	//questo può avvenire solo se l'xml ha un solo element type di tipo stringa (che è anche chiave)
	//es: tipo imballo, tipo via, tipo task
	void AddElement(const CString& strValue);
	void RemoveElement(const CString& strValue);
};


// Gestore dei datafile: array di fileinfo
//----------------------------------------------------------------
class TB_EXPORT CDataFilesManager : public CObject, public CTBLockable
{
protected:
	Array	m_arDataFiles;

public: 
	~CDataFilesManager();

public:
	CDataFileInfo* GetDataFile(LPCTSTR pszDataFileNamespace);
	void Add (CDataFileInfo* pdfi) { TB_LOCK_FOR_WRITE(); m_arDataFiles.Add((CObject*)pdfi); }

	virtual LPCSTR  GetObjectName() const { return "CDataFilesManager"; }
};

TB_EXPORT CDataFilesManager* AfxGetDataFilesManager();

//=============================================================================
#include "endh.dex"
