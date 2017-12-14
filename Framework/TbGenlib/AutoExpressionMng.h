
#pragma once

#include "generic.h"
#include <TbGeneric\Array.h>
#include <TbGeneric\DataObj.h>
#include "parsobj.h"

//includere alla fine degli include del .H
#include "beginh.dex"

//-----------------------------------------------------------
//Classe che contiene la stringa di AutoExpression, 
//il puntatore al DataObjAssociato, il nome della variabile
//-----------------------------------------------------------
class TB_EXPORT CAutoExpressionData : public CObject
{
	DECLARE_DYNCREATE (CAutoExpressionData);

	friend class CAutoExpressionDataArray;

protected:
	CString		m_strExpression;
	CString		m_strVarName;
	DataObj*	m_pDataObj;

public:
	CAutoExpressionData			();
	CAutoExpressionData			(const CString&, const CString&, DataObj*);
	CAutoExpressionData			(const CAutoExpressionData&);

protected:
	void		Assign			(const CAutoExpressionData&);

public:
	void		SetExpression	(const CString&);
	CString		GetExpression	();

	void		SetVarName		(const CString&);
	CString		GetVarName		();

	void		SetDataObj		(DataObj*);
	DataObj*	GetDataObj		();

private:
	BOOL		IsEqual			(const CAutoExpressionData&) const;

public:
	BOOL					operator ==	(const CAutoExpressionData& aVar)	const { return IsEqual	(aVar); }
	BOOL					operator !=	(const CAutoExpressionData& aVar)	const { return !IsEqual	(aVar); }
	CAutoExpressionData&	operator =	(const CAutoExpressionData&);
};

//-----------------------------------------------------------
class TB_EXPORT CAutoExpressionDataArray : public Array
{
	DECLARE_DYNCREATE (CAutoExpressionDataArray);

public:
	CAutoExpressionDataArray	();

public:
	CAutoExpressionData*		GetAt					(int nIndex) const			{ return (CAutoExpressionData*) Array::GetAt(nIndex);		}
	CAutoExpressionData*&		ElementAt				(int nIndex)				{ return (CAutoExpressionData*&) Array::ElementAt(nIndex); }
	int							Add						(CAutoExpressionData* pVar)	{ return Array::Add(pVar); }
	int							Add						(const CString&, const CString&, DataObj*);
	
	CString						GetVarNameByDataObj		(DataObj* pDataObj);
	CString						GetExpressionByDataObj	(DataObj*);
	DataObj*					GetDataObjByVarName		(const CString&);
	CString						GetExpressionByVarName	(const CString&);

public:
	BOOL						IsEqual					(const CAutoExpressionDataArray&) const;

	CAutoExpressionData*		operator[]				(int nIndex) const	{ return GetAt(nIndex);		}
	CAutoExpressionData*& 		operator[]				(int nIndex)		{ return ElementAt(nIndex);	}
	BOOL						operator==				(const CAutoExpressionDataArray& aVarArray) const { return IsEqual	(aVarArray); }
	BOOL						operator!=				(const CAutoExpressionDataArray& aVarArray) const { return !IsEqual	(aVarArray); }
	CAutoExpressionDataArray&	operator=				(const CAutoExpressionDataArray& aVarArray);	
};

//------------------------------------------------------------
//Ogni oggetto che possiede dei dati da gestire attraverso auto expresion
//deve avere un CAutoExpressionMng, che si occupa di linkare le sue vars
//con le espressioni. Si occupa anche della valutazione delleespressioni
//------------------------------------------------------------
class TB_EXPORT CAutoExpressionMng : public CObject
{
	DECLARE_DYNCREATE (CAutoExpressionMng);

private:
	CAutoExpressionDataArray	m_AutoExpressionDataArray;

public:
	CAutoExpressionMng					();

public:
	void					Add						(CAutoExpressionData*);
	void					Add						(const CString&, const CString&, DataObj*);

	BOOL					EvaluateExpression		(const CString&, DataObj*);
	
	CString					GetVarNameByDataObj		(DataObj*);
	CString					GetExpressionByDataObj	(DataObj*);
	CString					GetExpressionByVarName	(const CString&);
	DataObj*				GetDataObjByVarName		(const CString&);

	CAutoExpressionData*	GetAt					(int i)	{return m_AutoExpressionDataArray.GetAt(i);}
	int						GetSize					()		{return m_AutoExpressionDataArray.GetSize();}

	void					RemoveAll				();
	void					RemoveAt				(int i)	{m_AutoExpressionDataArray.RemoveAt(i);}
};

#include "endh.dex"

