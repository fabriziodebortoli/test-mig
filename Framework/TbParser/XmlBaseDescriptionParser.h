#pragma once

//includere alla fine degli include del .H
#include "beginh.dex"

class CPathFinder;
class CDataObjDescription;
class CXMLNode;
class CBaseDescription;
class Array;

//=============================================================================
//							XmlAttributeValidator
// eval XML attributes related to activation, allowISO, denyISO, allowedInput
// syntax contained into attributes values
//=============================================================================
class TB_EXPORT CXmlAttributeValidator : public CObject
{
public:
	static BOOL  IsValidCountry		(const CString& sCondition, const BOOL& bIsAllow);
	static BOOL  IsValidActivation	(const CString& sActivationExpression);
	static DWORD EvalInputAllowed	(CString sInputAllowedExpression);
	static BOOL	 IsValidGroup		(int nGroupId, const Array* groupArray);

private:
	static BOOL EvalIsValidActivationExpr(const int& nFirstKeyIndex, CString& sExpression, const TCHAR szCurrentOperator);
};

// rappresenta l'informazione di registrazione comune a tutti
//----------------------------------------------------------------
class TB_EXPORT CXMLBaseDescriptionParser : public CObject
{
	DECLARE_DYNAMIC(CXMLBaseDescriptionParser)

private:
	CString			m_sTagName;
	BOOL			m_bExpandNS;

public:
	CXMLBaseDescriptionParser ();
	CXMLBaseDescriptionParser (const CString& sTagName, BOOL bExpandNs = TRUE);
	~CXMLBaseDescriptionParser ();

public:
	const CString&	GetTagName		() const { return m_sTagName; }
	const BOOL&		ExpandNamespace	() const { return m_bExpandNS; }

	// metodi di settaggio
	void SetTagName			(const CString& sTagName);
	void SetExpandNamespace	(const BOOL& bValue);

public:
	virtual BOOL Parse	(CXMLNode*, CBaseDescription*, const CTBNamespace& aParent);
	virtual void Unparse(CXMLNode*, CBaseDescription*);
};

#include "endh.dex"
